#!/usr/bin/env python3
"""Build a bounded, offline ASTC 4x4 sidecar candidate for Logan body pages.

This helper is deliberately independent from the runtime loader.  It reads an
inventory and an existing derived-pages manifest, verifies every selected
input before doing any encoding, writes only to a new staging directory below
the repository's ``artifacts`` directory, and publishes the exact six-field
per-source manifest only after every requested encode and optional decode
review succeeds.

The PNG handed to astcenc is vertically flipped.  Unity's verified native
image path consumes bottom-left row order, while Pillow exposes PNG pixels in
top-left row order.  The ASTC sidecar therefore records the
``srgb-straight-rgba-bottom-left-no-mips-v1`` contract and never silently
compares a raw decoder image with the upright source.
"""

from __future__ import annotations

import argparse
import concurrent.futures
import dataclasses
import hashlib
import json
import math
import os
from pathlib import Path, PurePosixPath
import posixpath
import re
import subprocess
import sys
import time
from typing import Any, Iterable, Optional

from PIL import Image, ImageOps


PIXEL_CONTRACT = "srgb-straight-rgba-bottom-left-no-mips-v1"
ASTC_MAGIC = bytes.fromhex("13aba15c")
ASTC_BLOCK = (4, 4, 1)
ENCODER_THREADS = 2
DEFAULT_ENCODER_RELATIVE = Path(
    "artifacts/diagnostics/M12-astc-sample-20261012/encoder/bin/astcenc-sse2.exe"
)
DEFAULT_ENCODER_SHA256 = "8cc175fddb0dbba27821dd6b1bccfbb0cca9143fe3243b5f7274e298178e2509"
SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")


class InputContractError(RuntimeError):
    """An input or staging contract failed before or during a bounded run."""


@dataclasses.dataclass
class SourceRecord:
    key: str
    path_value: str
    path: Path
    expected_sha256: Optional[str]
    width: Optional[int]
    height: Optional[int]
    body_referenced: bool
    classification_evidence: str
    clamped_cell_refs: Any
    source_rows: int = 1

    @property
    def clamped(self) -> bool:
        value = self.clamped_cell_refs
        if value is None or value is False:
            return False
        if isinstance(value, (list, tuple, set, dict, str, bytes)):
            return len(value) > 0
        if isinstance(value, (int, float)):
            return value != 0
        return bool(value)

    @property
    def clamped_count(self) -> int:
        value = self.clamped_cell_refs
        if value is None or value is False:
            return 0
        if isinstance(value, (list, tuple, set, dict, str, bytes)):
            return len(value)
        if isinstance(value, (int, float)):
            return int(value)
        return 1 if value else 0


@dataclasses.dataclass
class PageRecord:
    id: str
    manifest_path: str
    path: Path
    content_relative_path: str
    manifest_sha256: str
    file_sha256: str
    width: int
    height: int
    pixel_sha256: str
    mapped_sources: list[str]


def utc_now() -> str:
    return time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime())


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def ensure_sha256(value: Any, field_name: str) -> Optional[str]:
    if value is None or value == "":
        return None
    text = str(value).strip().lower()
    if not SHA256_RE.fullmatch(text):
        raise InputContractError(f"{field_name} is not a SHA-256 value: {value!r}")
    return text


def json_load(path: Path) -> Any:
    try:
        with path.open("r", encoding="utf-8") as stream:
            return json.load(stream)
    except FileNotFoundError as exc:
        raise InputContractError(f"JSON input does not exist: {path}") from exc
    except json.JSONDecodeError as exc:
        raise InputContractError(f"invalid JSON input {path}: {exc}") from exc


def write_json_new(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if os.path.lexists(str(path)):
        raise InputContractError(f"refusing to overwrite evidence/output: {path}")
    with path.open("x", encoding="utf-8", newline="\n") as stream:
        json.dump(value, stream, ensure_ascii=False, indent=2)
        stream.write("\n")


def write_text_new(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if os.path.lexists(str(path)):
        raise InputContractError(f"refusing to overwrite evidence/output: {path}")
    with path.open("x", encoding="utf-8", newline="\n") as stream:
        stream.write(text)


def write_bytes_new(path: Path, data: bytes) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if os.path.lexists(str(path)):
        raise InputContractError(f"refusing to overwrite evidence/output: {path}")
    with path.open("xb") as stream:
        stream.write(data)


def is_within(path: Path, root: Path) -> bool:
    try:
        path.resolve(strict=False).relative_to(root.resolve(strict=False))
        return True
    except ValueError:
        return False


def path_from_arg(project_root: Path, value: str) -> Path:
    candidate = Path(value)
    if not candidate.is_absolute():
        candidate = project_root / candidate
    return candidate.resolve(strict=False)


def normalize_slashes(value: Any) -> str:
    return str(value).replace("\\", "/")


def content_relative_path(value: Any, content_root: Path) -> str:
    """Return a normalized path relative to ContentRoot when possible."""

    raw = normalize_slashes(value).strip()
    if not raw:
        raise InputContractError("empty resource path")

    candidate = Path(raw)
    if candidate.is_absolute() or re.match(r"^[A-Za-z]:/", raw):
        absolute = candidate.resolve(strict=False)
        if not is_within(absolute, content_root):
            raise InputContractError(
                f"absolute resource path escapes ContentRoot: {value!r}"
            )
        return absolute.relative_to(content_root.resolve()).as_posix()

    root_text = normalize_slashes(str(content_root.resolve())).rstrip("/")
    raw_casefold = raw.casefold()
    root_casefold = root_text.casefold()
    if raw_casefold.startswith(root_casefold + "/"):
        return raw[len(root_text) + 1 :]

    # Some inventories preserve an Assets-relative path instead of a path
    # relative to LoganRuntime.  Strip the known root prefix without using a
    # filesystem lookup or guessing a different content root.
    marker = "assets/ntsd/content/loganruntime/"
    if raw_casefold.startswith(marker):
        return raw[len(marker) :]

    return raw.lstrip("./")


def source_key(value: Any, content_root: Path) -> str:
    relative = content_relative_path(value, content_root)
    lower = relative.casefold()
    if lower.startswith("vfs/"):
        relative = relative[4:]
    return relative.casefold()


def resolve_content_path(value: Any, content_root: Path) -> Path:
    relative = content_relative_path(value, content_root)
    candidates: list[Path]
    if relative.casefold().startswith("vfs/"):
        candidates = [content_root / PurePosixPath(relative)]
    elif relative.casefold().startswith("derived_sprite_pages/"):
        candidates = [content_root / PurePosixPath(relative)]
    else:
        candidates = [
            content_root / "vfs" / PurePosixPath(relative),
            content_root / PurePosixPath(relative),
        ]
    for candidate in candidates:
        resolved = candidate.resolve(strict=False)
        if not is_within(resolved, content_root):
            continue
        if resolved.exists():
            return resolved
    # Return the first safe candidate so the caller can report the exact
    # missing path, while never permitting a path outside ContentRoot.
    return candidates[0].resolve(strict=False)


def get_first(mapping: dict[str, Any], names: Iterable[str]) -> Any:
    for name in names:
        if name in mapping:
            return mapping[name]
    return None


def as_int(value: Any, field_name: str) -> Optional[int]:
    if value is None or value == "":
        return None
    try:
        result = int(value)
    except (TypeError, ValueError) as exc:
        raise InputContractError(f"{field_name} is not an integer: {value!r}") from exc
    if result <= 0:
        raise InputContractError(f"{field_name} must be positive: {value!r}")
    return result


def row_dimensions(row: dict[str, Any]) -> tuple[Optional[int], Optional[int]]:
    width = get_first(row, ("width", "imageWidth", "sourceWidth"))
    height = get_first(row, ("height", "imageHeight", "sourceHeight"))
    dimensions = get_first(row, ("dimensions", "size"))
    if isinstance(dimensions, dict):
        width = width if width is not None else dimensions.get("width")
        height = height if height is not None else dimensions.get("height")
    elif isinstance(dimensions, (list, tuple)) and len(dimensions) >= 2:
        width = width if width is not None else dimensions[0]
        height = height if height is not None else dimensions[1]
    return as_int(width, "inventory width"), as_int(height, "inventory height")


def row_path(row: dict[str, Any]) -> Any:
    value = get_first(
        row,
        (
            "path",
            "sourcePath",
            "source_path",
            "imagePath",
            "image_path",
            "vfsPath",
            "vfs_path",
        ),
    )
    if isinstance(value, dict):
        value = get_first(value, ("path", "sourcePath", "source_path"))
    return value


def row_source_sha(row: dict[str, Any]) -> Optional[str]:
    value = get_first(
        row,
        (
            "sourceSha256",
            "source_sha256",
            "fileSha256",
            "file_sha256",
            "sha256",
        ),
    )
    if value is None and isinstance(row.get("source"), dict):
        value = get_first(row["source"], ("sourceSha256", "sha256", "fileSha256"))
    return ensure_sha256(value, "inventory source SHA")


def truthy_reference(value: Any) -> bool:
    if value is None or value is False:
        return False
    if isinstance(value, str):
        return value.strip().casefold() not in ("", "0", "false", "no", "none")
    if isinstance(value, (list, tuple, set, dict)):
        return len(value) > 0
    return bool(value)


def row_body_classification(row: dict[str, Any]) -> tuple[Optional[bool], str]:
    if "bodyReferenced" in row:
        return truthy_reference(row["bodyReferenced"]), "row.bodyReferenced"
    if "body_referenced" in row:
        return truthy_reference(row["body_referenced"]), "row.body_referenced"
    if "bodyReferenceCount" in row:
        return int(row["bodyReferenceCount"]) > 0, "row.bodyReferenceCount"
    if "body_reference_count" in row:
        return int(row["body_reference_count"]) > 0, "row.body_reference_count"
    for field in ("category", "kind", "scope", "assetKind"):
        value = row.get(field)
        if isinstance(value, str):
            normalized = value.strip().casefold()
            if normalized == "body":
                return True, f"row.{field}=body"
            if normalized in ("head", "small", "common"):
                return False, f"row.{field}={normalized}"
    return None, "missing-body-classification"


def collection_rows(value: Any) -> list[dict[str, Any]]:
    if isinstance(value, list):
        return [item for item in value if isinstance(item, dict)]
    if isinstance(value, dict):
        result: list[dict[str, Any]] = []
        for path_value, details in value.items():
            if isinstance(details, dict):
                row = dict(details)
                row.setdefault("path", path_value)
            else:
                row = {"path": path_value, "bodyReferenced": details}
            result.append(row)
        return result
    return []


def inventory_row_list(root: dict[str, Any]) -> tuple[list[dict[str, Any]], Any]:
    rows_value = get_first(root, ("rows", "images", "entries", "items", "sources"))
    rows = [item for item in rows_value or [] if isinstance(item, dict)]
    body_collection = root.get("bodyReferenced")
    if not rows and isinstance(body_collection, (list, dict)):
        rows = collection_rows(body_collection)
    return rows, body_collection


def top_level_clamped_map(root: dict[str, Any], content_root: Path) -> dict[str, Any]:
    value = root.get("clampedCellRefs")
    if not isinstance(value, dict):
        return {}
    result: dict[str, Any] = {}
    for path_value, refs in value.items():
        result[source_key(path_value, content_root)] = refs
    return result


def load_inventory(
    path: Path,
    content_root: Path,
    allow_missing_sha: bool,
) -> tuple[dict[str, SourceRecord], dict[str, Any]]:
    root = json_load(path)
    if not isinstance(root, dict):
        raise InputContractError(f"inventory root must be an object: {path}")
    rows, body_collection = inventory_row_list(root)
    if not rows:
        raise InputContractError(f"inventory contains no image rows: {path}")

    body_collection_keys: set[str] = set()
    if isinstance(body_collection, (list, dict)):
        if isinstance(body_collection, list):
            for item in body_collection:
                if isinstance(item, str):
                    body_collection_keys.add(source_key(item, content_root))
        for row in collection_rows(body_collection):
            value = row_path(row)
            if value is not None:
                body_collection_keys.add(source_key(value, content_root))

    clamped_map = top_level_clamped_map(root, content_root)
    preliminary: list[tuple[dict[str, Any], Optional[bool], str]] = []
    for row in rows:
        value = row_path(row)
        if value is None:
            raise InputContractError(f"inventory row has no source path: {row!r}")
        body, evidence = row_body_classification(row)
        key = source_key(value, content_root)
        if body is None and key in body_collection_keys:
            body, evidence = True, "top-level bodyReferenced collection"
        preliminary.append((row, body, evidence))

    explicit_count = sum(body is not None for _, body, _ in preliminary)
    legacy = False
    if explicit_count == 0:
        if body_collection_keys:
            preliminary = [
                (row, source_key(row_path(row), content_root) in body_collection_keys, "top-level bodyReferenced path collection")
                for row, _, _ in preliminary
            ]
        else:
            unique_paths = root.get("uniquePaths")
            if len(rows) == 703 and (unique_paths is None or int(unique_paths) == 703):
                legacy = True
                preliminary = [
                    (row, True, "legacy-703-row-body-inventory")
                    for row, _, _ in preliminary
                ]
            else:
                raise InputContractError(
                    "inventory exposes neither bodyReferenced classification nor the "
                    "verified 703-row legacy body shape"
                )
    elif explicit_count != len(preliminary):
        # Unknown rows remain excluded in a mixed inventory.  This is safer
        # than guessing that a path without a classification is a body sheet.
        preliminary = [
            (row, bool(body) if body is not None else False, evidence)
            for row, body, evidence in preliminary
        ]

    records: dict[str, SourceRecord] = {}
    missing_sha_count = 0
    for row, body, evidence in preliminary:
        value = row_path(row)
        assert value is not None
        key = source_key(value, content_root)
        path_value = normalize_slashes(value)
        resolved = resolve_content_path(value, content_root)
        expected_sha = row_source_sha(row)
        if expected_sha is None and body:
            missing_sha_count += 1
            if not allow_missing_sha:
                raise InputContractError(
                    f"body inventory row has no source SHA: {path_value}; "
                    "supply an inventory with sourceSha256 or explicitly use "
                    "--allow-missing-inventory-sha for a legacy diagnostic run"
                )
        width, height = row_dimensions(row)
        clamped = get_first(row, ("clampedCellRefs", "clamped_cell_refs"))
        if clamped is None:
            clamped = clamped_map.get(key)
        record = SourceRecord(
            key=key,
            path_value=path_value,
            path=resolved,
            expected_sha256=expected_sha,
            width=width,
            height=height,
            body_referenced=bool(body),
            classification_evidence=evidence,
            clamped_cell_refs=clamped,
        )
        previous = records.get(key)
        if previous is None:
            records[key] = record
            continue
        if previous.body_referenced != record.body_referenced:
            raise InputContractError(f"conflicting body classification for {key}")
        if previous.expected_sha256 and record.expected_sha256 and previous.expected_sha256 != record.expected_sha256:
            raise InputContractError(f"conflicting source SHA values for {key}")
        if previous.width and record.width and (previous.width, previous.height) != (record.width, record.height):
            raise InputContractError(f"conflicting source dimensions for {key}")
        previous.expected_sha256 = previous.expected_sha256 or record.expected_sha256
        previous.width = previous.width or record.width
        previous.height = previous.height or record.height
        if previous.clamped_cell_refs is None and record.clamped_cell_refs is not None:
            previous.clamped_cell_refs = record.clamped_cell_refs
        previous.source_rows += 1

    selected = [record for record in records.values() if record.body_referenced]
    if not selected:
        raise InputContractError("inventory contains no bodyReferenced sources")
    metadata = {
        "path": str(path),
        "sha256": sha256_file(path),
        "bytes": path.stat().st_size,
        "rowCount": len(rows),
        "uniqueSourceCount": len(records),
        "bodySourceCount": len(selected),
        "clampedBodySourceCount": sum(record.clamped for record in selected),
        "missingBodySourceShaCount": missing_sha_count,
        "selectionMode": "legacy-703-row-body-inventory" if legacy else "explicit-bodyReferenced",
        "legacyFallbackUsed": legacy,
    }
    return records, metadata


def verify_source_inputs(
    records: dict[str, SourceRecord],
) -> list[dict[str, Any]]:
    receipts: list[dict[str, Any]] = []
    for record in sorted(records.values(), key=lambda item: item.key):
        if not record.body_referenced:
            continue
        if not record.path.exists() or not record.path.is_file():
            raise InputContractError(f"body source image does not exist: {record.path}")
        actual_sha = sha256_file(record.path)
        if record.expected_sha256 and actual_sha != record.expected_sha256:
            raise InputContractError(
                f"source SHA mismatch for {record.path}: "
                f"inventory={record.expected_sha256} actual={actual_sha}"
            )
        with Image.open(record.path) as image:
            actual_width, actual_height = image.size
        if record.width and (actual_width, actual_height) != (record.width, record.height):
            raise InputContractError(
                f"source dimensions mismatch for {record.path}: "
                f"inventory={record.width}x{record.height} actual={actual_width}x{actual_height}"
            )
        receipts.append(
            {
                "sourceKey": record.key,
                "path": record.path_value,
                "absolutePath": str(record.path),
                "inventorySha256": record.expected_sha256,
                "actualSha256": actual_sha,
                "shaMatches": record.expected_sha256 is None or actual_sha == record.expected_sha256,
                "inventoryDimensions": [record.width, record.height] if record.width else None,
                "actualDimensions": [actual_width, actual_height],
                "dimensionsMatch": record.width is None or (actual_width, actual_height) == (record.width, record.height),
                "clampedCellRefCount": record.clamped_count,
                "clampedExcluded": record.clamped,
                "classification": record.classification_evidence,
                "inventoryRowCount": record.source_rows,
            }
        )
    return receipts


def page_id_from_chunk(value: Any, page_by_id: dict[str, dict[str, Any]]) -> str:
    text = normalize_slashes(value)
    if text in page_by_id:
        return text
    stem = Path(text).stem
    if stem in page_by_id:
        return stem
    raise InputContractError(f"source chunk refers to unknown physical page: {value!r}")


def verify_pages_manifest(
    path: Path,
    content_root: Path,
) -> tuple[dict[str, PageRecord], dict[str, list[str]], dict[str, Any]]:
    root = json_load(path)
    if not isinstance(root, dict):
        raise InputContractError(f"pages manifest root must be an object: {path}")
    if root.get("version") != 1:
        raise InputContractError(f"pages manifest version must be 1: {path}")
    pages_value = root.get("pages")
    sources_value = root.get("sources")
    if not isinstance(pages_value, list) or not pages_value:
        raise InputContractError("pages manifest has no pages array")
    if not isinstance(sources_value, list) or not sources_value:
        raise InputContractError("pages manifest has no sources array")

    page_by_id_raw: dict[str, dict[str, Any]] = {}
    for page in pages_value:
        if not isinstance(page, dict):
            raise InputContractError("pages manifest contains a non-object page")
        page_id = str(page.get("id", ""))
        page_path_value = page.get("path")
        if not page_id or not page_path_value:
            raise InputContractError(f"page is missing id/path: {page!r}")
        if page_id in page_by_id_raw:
            raise InputContractError(f"duplicate page id: {page_id}")
        page_by_id_raw[page_id] = page

    page_records: dict[str, PageRecord] = {}
    page_validation: list[dict[str, Any]] = []
    manifest_dir = path.parent.resolve()
    for page_id, page in page_by_id_raw.items():
        manifest_path_value = normalize_slashes(page["path"])
        physical_path = (manifest_dir / PurePosixPath(manifest_path_value)).resolve(strict=False)
        if not is_within(physical_path, manifest_dir):
            raise InputContractError(f"page path escapes pages manifest directory: {page['path']!r}")
        if not physical_path.exists() or not physical_path.is_file():
            raise InputContractError(f"physical page does not exist: {physical_path}")
        manifest_sha = ensure_sha256(page.get("sha256"), f"page {page_id} sha256")
        assert manifest_sha is not None
        actual_sha = sha256_file(physical_path)
        if actual_sha != manifest_sha:
            raise InputContractError(
                f"physical page SHA mismatch for {page_id}: manifest={manifest_sha} actual={actual_sha}"
            )
        width = as_int(page.get("width"), f"page {page_id} width")
        height = as_int(page.get("height"), f"page {page_id} height")
        assert width is not None and height is not None
        with Image.open(physical_path) as image:
            rgba = image.convert("RGBA")
            actual_dimensions = rgba.size
            pixel_sha = sha256_bytes(rgba.tobytes())
            rgba.close()
        if actual_dimensions != (width, height):
            raise InputContractError(
                f"physical page dimensions mismatch for {page_id}: "
                f"manifest={width}x{height} actual={actual_dimensions[0]}x{actual_dimensions[1]}"
            )
        relative_to_content = physical_path.relative_to(content_root.resolve()).as_posix()
        page_records[page_id] = PageRecord(
            id=page_id,
            manifest_path=manifest_path_value,
            path=physical_path,
            content_relative_path=relative_to_content,
            manifest_sha256=manifest_sha,
            file_sha256=actual_sha,
            width=width,
            height=height,
            pixel_sha256=pixel_sha,
            mapped_sources=[],
        )
        page_validation.append(
            {
                "id": page_id,
                "manifestPath": manifest_path_value,
                "absolutePath": str(physical_path),
                "manifestSha256": manifest_sha,
                "actualSha256": actual_sha,
                "shaMatches": True,
                "width": width,
                "height": height,
                "pixelSha256": pixel_sha,
            }
        )

    page_to_sources: dict[str, list[str]] = {page_id: [] for page_id in page_records}
    source_validation: list[dict[str, Any]] = []
    seen_source_keys: set[str] = set()
    for source in sources_value:
        if not isinstance(source, dict):
            raise InputContractError("pages manifest contains a non-object source")
        source_path_value = source.get("path")
        if not source_path_value:
            raise InputContractError(f"pages source has no path: {source!r}")
        key = source_key(source_path_value, content_root)
        source_path = resolve_content_path(source_path_value, content_root)
        if not source_path.exists() or not source_path.is_file():
            raise InputContractError(f"pages source does not exist: {source_path}")
        source_manifest_sha = ensure_sha256(source.get("sha256"), f"pages source {key} sha256")
        actual_source_sha = sha256_file(source_path)
        if source_manifest_sha and actual_source_sha != source_manifest_sha:
            raise InputContractError(
                f"pages source SHA mismatch for {key}: "
                f"manifest={source_manifest_sha} actual={actual_source_sha}"
            )
        source_width = as_int(source.get("width"), f"pages source {key} width")
        source_height = as_int(source.get("height"), f"pages source {key} height")
        if source_width or source_height:
            with Image.open(source_path) as image:
                actual_source_dimensions = image.size
            if actual_source_dimensions != (source_width, source_height):
                raise InputContractError(
                    f"pages source dimensions mismatch for {key}: "
                    f"manifest={source_width}x{source_height} "
                    f"actual={actual_source_dimensions[0]}x{actual_source_dimensions[1]}"
                )
        source_validation.append(
            {
                "sourceKey": key,
                "path": normalize_slashes(source_path_value),
                "absolutePath": str(source_path),
                "manifestSha256": source_manifest_sha,
                "actualSha256": actual_source_sha,
                "shaMatches": source_manifest_sha is None or actual_source_sha == source_manifest_sha,
                "manifestDimensions": [source_width, source_height] if source_width else None,
            }
        )
        if key in seen_source_keys:
            raise InputContractError(f"duplicate source path in pages manifest: {key}")
        seen_source_keys.add(key)
        chunks = source.get("chunks")
        if not isinstance(chunks, list) or not chunks:
            raise InputContractError(f"pages source has no chunks: {key}")
        for chunk in chunks:
            if not isinstance(chunk, dict) or not chunk.get("page"):
                raise InputContractError(f"invalid chunk for pages source {key}: {chunk!r}")
            page_id = page_id_from_chunk(chunk["page"], page_by_id_raw)
            page_to_sources[page_id].append(key)
            page_records[page_id].mapped_sources.append(key)

    metadata = {
        "path": str(path),
        "sha256": sha256_file(path),
        "bytes": path.stat().st_size,
        "version": root.get("version"),
        "pageCount": len(page_records),
        "sourceCount": len(sources_value),
        "mappedPageCount": sum(bool(value) for value in page_to_sources.values()),
        "pageValidation": page_validation,
        "sourceValidation": source_validation,
    }
    return page_records, page_to_sources, metadata


def encoder_identity(path: Path, project_root: Path, explicit: bool) -> dict[str, Any]:
    if not path.exists() or not path.is_file():
        raise InputContractError(f"ASTC encoder does not exist: {path}")
    with path.open("rb") as stream:
        if stream.read(2) != b"MZ":
            raise InputContractError(f"ASTC encoder is not an MZ executable: {path}")
    actual_sha = sha256_file(path)
    expected = None if explicit else DEFAULT_ENCODER_SHA256
    if expected and actual_sha != expected:
        raise InputContractError(
            f"default ASTC encoder SHA mismatch: expected={expected} actual={actual_sha}"
        )
    version_command = [str(path), "-version"]
    version = subprocess.run(
        version_command,
        cwd=str(project_root),
        capture_output=True,
        check=False,
    )
    stdout = (version.stdout or b"").decode("utf-8", errors="replace")
    stderr = (version.stderr or b"").decode("utf-8", errors="replace")
    if version.returncode != 0:
        raise InputContractError(
            f"ASTC encoder identity command failed with exit {version.returncode}: {path}"
        )
    return {
        "path": str(path),
        "relativeToProject": path.relative_to(project_root.resolve()).as_posix()
        if is_within(path, project_root)
        else None,
        "bytes": path.stat().st_size,
        "sha256": actual_sha,
        "expectedDefaultSha256": expected,
        "matchesExpectedDefaultSha256": expected is None or actual_sha == expected,
        "explicitEncoderArgument": explicit,
        "validatedMZ": True,
        "versionCommand": version_command,
        "versionExitCode": version.returncode,
        "versionStdout": stdout,
        "versionStderr": stderr,
        "profile": "astcenc 5.6.0 -cs/-ds; sRGB LDR; 4x4; thorough; -a 1; -j 2",
    }


def validate_astc(path: Path, width: int, height: int) -> dict[str, Any]:
    data = path.read_bytes()
    if len(data) < 16:
        raise InputContractError(f"ASTC output is shorter than the 16-byte header: {path}")
    if data[:4] != ASTC_MAGIC:
        raise InputContractError(f"ASTC magic mismatch: {path}: {data[:4].hex()}")
    block = tuple(data[4:7])
    if block != ASTC_BLOCK:
        raise InputContractError(f"ASTC block mismatch for {path}: {block}")
    dimensions = tuple(int.from_bytes(data[index : index + 3], "little") for index in (7, 10, 13))
    if dimensions != (width, height, 1):
        raise InputContractError(
            f"ASTC dimensions mismatch for {path}: expected={(width, height, 1)} actual={dimensions}"
        )
    payload_bytes = math.ceil(width / 4) * math.ceil(height / 4) * 16
    expected_length = 16 + payload_bytes
    if len(data) != expected_length:
        raise InputContractError(
            f"ASTC payload length mismatch for {path}: expected={expected_length} actual={len(data)}"
        )
    return {
        "magicHex": data[:4].hex(),
        "headerBytes": 16,
        "blockDimensions": list(block),
        "dimensions": list(dimensions),
        "payloadBytes": payload_bytes,
        "expectedLength": expected_length,
        "fileBytes": len(data),
        "sha256": sha256_bytes(data),
    }


def command_line(command: list[str]) -> str:
    return subprocess.list2cmdline(command)


def prepare_png(job: dict[str, Any], output_root: Path) -> dict[str, Any]:
    prepared_path = output_root / job["preparedRelative"]
    if os.path.lexists(str(prepared_path)):
        raise InputContractError(f"prepared PNG collision: {prepared_path}")
    source_path = Path(job["representativeSourcePageAbsolute"])
    with Image.open(source_path) as image:
        upright = image.convert("RGBA")
        if upright.size != (job["width"], job["height"]):
            raise InputContractError(f"page dimensions changed while preparing: {source_path}")
        upright_pixel_sha = sha256_bytes(upright.tobytes())
        if upright_pixel_sha != job["pixelSha256"]:
            raise InputContractError(f"page pixel SHA changed while preparing: {source_path}")
        bottom_left = ImageOps.flip(upright)
        prepared_path.parent.mkdir(parents=True, exist_ok=True)
        with prepared_path.open("xb") as stream:
            bottom_left.save(stream, format="PNG", optimize=False, compress_level=0)
        prepared_pixel_sha = sha256_bytes(bottom_left.tobytes())
        bottom_left.close()
        upright.close()
    prepared_sha = sha256_file(prepared_path)
    job["preparedPngSha256"] = prepared_sha
    job["preparedPixelSha256"] = prepared_pixel_sha
    job["preparedBytes"] = prepared_path.stat().st_size
    return job


def encode_one(job: dict[str, Any], output_root: Path, encoder: Path, project_root: Path) -> dict[str, Any]:
    key = job["blockKey"]
    astc_path = output_root / job["astcRelative"]
    prepared_path = output_root / job["preparedRelative"]
    if not prepared_path.exists():
        raise InputContractError(f"prepared PNG missing before encode: {prepared_path}")
    if os.path.lexists(str(astc_path)):
        raise InputContractError(f"ASTC output collision: {astc_path}")
    astc_path.parent.mkdir(parents=True, exist_ok=True)
    command = [
        str(encoder),
        "-cs",
        str(prepared_path),
        str(astc_path),
        "4x4",
        "-thorough",
        "-a",
        "1",
        "-j",
        str(ENCODER_THREADS),
    ]
    logs = output_root / "logs"
    pre_path = logs / f"{key}.encode-pre.json"
    result_path = logs / f"{key}.encode-result.json"
    stdout_path = logs / f"{key}.encode.stdout.txt"
    stderr_path = logs / f"{key}.encode.stderr.txt"
    pre = {
        "utc": utc_now(),
        "operation": "encode",
        "blockKey": key,
        "command": command,
        "commandLine": command_line(command),
        "inputsSha256": {
            "preparedPng": job["preparedPngSha256"],
            "representativeSourcePage": job["representativeSourcePageSha256"],
        },
        "inputDimensions": [job["width"], job["height"]],
        "pixelContract": PIXEL_CONTRACT,
        "decoderOrientation": {
            "encodeInput": "Pillow RGBA after FLIP_TOP_BOTTOM; row 0 is native bottom-left",
            "decodeReview": "astcenc -ds raw output is retained, then FLIP_TOP_BOTTOM creates upright comparison",
        },
        "encoderThreads": ENCODER_THREADS,
        "preparedPng": str(prepared_path),
        "astcOutput": str(astc_path),
    }
    write_json_new(pre_path, pre)
    started_utc = utc_now()
    started = time.monotonic()
    try:
        process = subprocess.run(
            command,
            cwd=str(project_root),
            capture_output=True,
            check=False,
        )
        stdout = process.stdout or b""
        stderr = process.stderr or b""
        write_text_new(stdout_path, stdout.decode("utf-8", errors="replace"))
        write_text_new(stderr_path, stderr.decode("utf-8", errors="replace"))
        receipt: dict[str, Any] = {
            "utcStarted": started_utc,
            "utcEnded": utc_now(),
            "wallSeconds": time.monotonic() - started,
            "exitCode": process.returncode,
            "command": command,
            "commandLine": command_line(command),
            "stdoutSha256": sha256_bytes(stdout),
            "stderrSha256": sha256_bytes(stderr),
            "stdoutBytes": len(stdout),
            "stderrBytes": len(stderr),
            "outputExists": astc_path.exists(),
        }
        if process.returncode != 0:
            receipt["error"] = f"encoder exited with {process.returncode}"
            write_json_new(result_path, receipt)
            raise InputContractError(
                f"ASTC encode failed for {key} with exit {process.returncode}; see {result_path}"
            )
        if not astc_path.exists():
            receipt["error"] = "encoder exited zero but produced no output"
            write_json_new(result_path, receipt)
            raise InputContractError(f"ASTC encoder produced no output for {key}")
        header = validate_astc(astc_path, job["width"], job["height"])
        receipt["header"] = header
        receipt["outputBytes"] = astc_path.stat().st_size
        receipt["outputSha256"] = header["sha256"]
        write_json_new(result_path, receipt)
        return receipt
    except InputContractError:
        raise
    except OSError as exc:
        receipt = {
            "utcStarted": started_utc,
            "utcEnded": utc_now(),
            "wallSeconds": time.monotonic() - started,
            "exitCode": None,
            "command": command,
            "commandLine": command_line(command),
            "error": repr(exc),
        }
        write_json_new(result_path, receipt)
        raise InputContractError(f"could not execute ASTC encoder for {key}: {exc}") from exc


def pixel_metrics(expected: bytes, actual: bytes) -> dict[str, Any]:
    if len(expected) != len(actual) or len(expected) % 4 != 0:
        raise InputContractError("decode-review pixel buffer has an unexpected size")
    changed = 0
    sum_abs = [0, 0, 0, 0]
    max_abs = [0, 0, 0, 0]
    for offset in range(0, len(expected), 4):
        pixel_changed = False
        for channel in range(4):
            delta = abs(expected[offset + channel] - actual[offset + channel])
            if delta:
                pixel_changed = True
            sum_abs[channel] += delta
            if delta > max_abs[channel]:
                max_abs[channel] = delta
        if pixel_changed:
            changed += 1
    pixels = len(expected) // 4
    return {
        "pixels": pixels,
        "changedPixels": changed,
        "exactRgbaMatch": changed == 0,
        "meanAbsByChannel": [value / pixels for value in sum_abs],
        "maxAbsByChannel": max_abs,
        "lossyComparisonOnly": True,
    }


def decode_review_one(
    job: dict[str, Any],
    output_root: Path,
    encoder: Path,
    project_root: Path,
) -> dict[str, Any]:
    key = job["blockKey"]
    astc_path = output_root / job["astcRelative"]
    raw_path = output_root / "rawdecode" / f"{key}.png"
    upright_path = output_root / "upright" / f"{key}.png"
    for path in (raw_path, upright_path):
        if os.path.lexists(str(path)):
            raise InputContractError(f"decode-review output collision: {path}")
    raw_path.parent.mkdir(parents=True, exist_ok=True)
    upright_path.parent.mkdir(parents=True, exist_ok=True)
    command = [str(encoder), "-ds", str(astc_path), str(raw_path), "-j", str(ENCODER_THREADS)]
    logs = output_root / "logs"
    pre_path = logs / f"{key}.decode-pre.json"
    result_path = logs / f"{key}.decode-result.json"
    stdout_path = logs / f"{key}.decode.stdout.txt"
    stderr_path = logs / f"{key}.decode.stderr.txt"
    pre = {
        "utc": utc_now(),
        "operation": "decode-review",
        "blockKey": key,
        "command": command,
        "commandLine": command_line(command),
        "inputsSha256": {"astc": sha256_file(astc_path)},
        "decoderOrientation": {
            "rawDecode": "retained exactly as emitted by astcenc -ds",
            "uprightReview": "rawDecode FLIP_TOP_BOTTOM before comparison with source page",
        },
        "astcPath": str(astc_path),
        "rawDecodePath": str(raw_path),
        "uprightPath": str(upright_path),
    }
    write_json_new(pre_path, pre)
    started_utc = utc_now()
    started = time.monotonic()
    process = subprocess.run(command, cwd=str(project_root), capture_output=True, check=False)
    stdout = process.stdout or b""
    stderr = process.stderr or b""
    write_text_new(stdout_path, stdout.decode("utf-8", errors="replace"))
    write_text_new(stderr_path, stderr.decode("utf-8", errors="replace"))
    receipt: dict[str, Any] = {
        "utcStarted": started_utc,
        "utcEnded": utc_now(),
        "wallSeconds": time.monotonic() - started,
        "exitCode": process.returncode,
        "command": command,
        "commandLine": command_line(command),
        "stdoutSha256": sha256_bytes(stdout),
        "stderrSha256": sha256_bytes(stderr),
        "stdoutBytes": len(stdout),
        "stderrBytes": len(stderr),
    }
    if process.returncode != 0:
        receipt["error"] = f"decoder exited with {process.returncode}"
        write_json_new(result_path, receipt)
        raise InputContractError(
            f"ASTC decode review failed for {key} with exit {process.returncode}; see {result_path}"
        )
    if not raw_path.exists():
        receipt["error"] = "decoder exited zero but produced no PNG"
        write_json_new(result_path, receipt)
        raise InputContractError(f"ASTC decoder produced no raw PNG for {key}")

    with Image.open(raw_path) as decoded:
        raw_rgba = decoded.convert("RGBA")
        if raw_rgba.size != (job["width"], job["height"]):
            raise InputContractError(
                f"raw ASTC decode dimensions mismatch for {key}: {raw_rgba.size}"
            )
        upright = ImageOps.flip(raw_rgba)
        with upright_path.open("xb") as stream:
            upright.save(stream, format="PNG", optimize=False, compress_level=0)
        upright_bytes = upright.tobytes()
        raw_pixel_sha = sha256_bytes(raw_rgba.tobytes())
        upright_pixel_sha = sha256_bytes(upright_bytes)
        raw_rgba.close()
        upright.close()
    source_path = Path(job["representativeSourcePageAbsolute"])
    with Image.open(source_path) as source_image:
        source_rgba = source_image.convert("RGBA")
        source_bytes = source_rgba.tobytes()
        metrics = pixel_metrics(source_bytes, upright_bytes)
        source_rgba.close()
    receipt.update(
        {
            "rawDecodeSha256": sha256_file(raw_path),
            "rawDecodePixelSha256": raw_pixel_sha,
            "uprightSha256": sha256_file(upright_path),
            "uprightPixelSha256": upright_pixel_sha,
            "rawDecodeBytes": raw_path.stat().st_size,
            "uprightBytes": upright_path.stat().st_size,
            "orientationApplied": "FLIP_TOP_BOTTOM",
            "comparisonSource": job["representativeSource"],
            "comparisonSourceSha256": job["representativeSourcePageSha256"],
            "metrics": metrics,
        }
    )
    write_json_new(result_path, receipt)
    return receipt


def parse_decode_review(value: Optional[str]) -> Optional[int]:
    if value is None:
        return None
    if value.casefold() == "all":
        return -1
    try:
        count = int(value)
    except ValueError as exc:
        raise InputContractError("--decode-review expects a positive count or 'all'") from exc
    if count <= 0:
        raise InputContractError("--decode-review expects a positive count or 'all'")
    return count


def build_selection(
    records: dict[str, SourceRecord],
    pages: dict[str, PageRecord],
    page_to_sources: dict[str, list[str]],
    limit: Optional[int],
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]]]:
    decisions: list[dict[str, Any]] = []
    candidates: list[PageRecord] = []
    clamped_exclusions: list[dict[str, Any]] = []
    for page in pages.values():
        source_keys = page_to_sources.get(page.id, [])
        reason: Optional[str] = None
        if not source_keys:
            reason = "no-pages-sources-mapping"
        else:
            missing = [key for key in source_keys if key not in records]
            if missing:
                reason = "source-not-in-inventory"
            else:
                source_records = [records[key] for key in source_keys]
                if any(not record.body_referenced for record in source_records):
                    reason = "non-body-source"
                else:
                    clamped = [record for record in source_records if record.clamped]
                    if clamped:
                        reason = "clamped-source-rgba-compatibility"
                        clamped_exclusions.append(
                            {
                                "pageId": page.id,
                                "page": page.content_relative_path,
                                "sourceKeys": [record.key for record in clamped],
                                "clampedCellRefCounts": {
                                    record.key: record.clamped_count for record in clamped
                                },
                                "decision": "excluded-entire-physical-page",
                                "reason": "retain original RGBA compatibility; do not assume ASTC can replace out-of-bounds cells",
                            }
                        )
        selected = reason is None
        decisions.append(
            {
                "pageId": page.id,
                "page": page.content_relative_path,
                "mappedSourceKeys": source_keys,
                "selectedBodyPage": selected,
                "exclusionReason": reason,
            }
        )
        if selected:
            candidates.append(page)
    candidates.sort(key=lambda page: page.content_relative_path.casefold())
    selected_pages = candidates[:limit] if limit is not None else candidates
    selected_ids = {page.id for page in selected_pages}
    for decision in decisions:
        if decision["selectedBodyPage"] and decision["pageId"] not in selected_ids:
            decision["selectedBodyPage"] = False
            decision["exclusionReason"] = "limit"
    if not selected_pages:
        raise InputContractError("no body physical pages remain after inventory/pages selection")

    jobs_by_key: dict[str, dict[str, Any]] = {}
    page_jobs: list[dict[str, Any]] = []
    for page in selected_pages:
        block_key = f"{page.pixel_sha256}-{page.width}x{page.height}"
        prepared_relative = f"prepared/{block_key}.png"
        astc_relative = f"blocks/{block_key}.astc"
        job = jobs_by_key.get(block_key)
        if job is None:
            job = {
                "blockKey": block_key,
                "width": page.width,
                "height": page.height,
                "pixelSha256": page.pixel_sha256,
                "preparedRelative": prepared_relative,
                "astcRelative": astc_relative,
                "representativeSource": page.content_relative_path,
                "representativeSourcePageAbsolute": str(page.path),
                "representativeSourcePageSha256": page.file_sha256,
                "representativePageId": page.id,
                "sourcePages": [],
            }
            jobs_by_key[block_key] = job
        job["sourcePages"].append(page.content_relative_path)
        page_jobs.append(
            {
                "pageId": page.id,
                "source": page.content_relative_path,
                "sourceSha256": page.file_sha256,
                "width": page.width,
                "height": page.height,
                "pixelSha256": page.pixel_sha256,
                "blockKey": block_key,
                "preparedRelative": prepared_relative,
                "astcRelative": astc_relative,
                "mappedSourceKeys": page_to_sources.get(page.id, []),
                "representativeSourcePageAbsolute": str(page.path),
                "representativeSourcePageSha256": page.file_sha256,
            }
        )
    jobs = list(jobs_by_key.values())
    jobs.sort(key=lambda item: item["blockKey"])
    page_jobs.sort(key=lambda item: item["source"].casefold())
    return page_jobs, jobs, clamped_exclusions


def validate_final_manifest(manifest: dict[str, Any]) -> None:
    if set(manifest) != {"version", "pixelContract", "entries"}:
        raise InputContractError("final manifest schema keys are not exact")
    if manifest["version"] != 1 or manifest["pixelContract"] != PIXEL_CONTRACT:
        raise InputContractError("final manifest version or pixelContract is incorrect")
    if not isinstance(manifest["entries"], list) or not manifest["entries"]:
        raise InputContractError("final manifest entries must be non-empty")
    entry_keys = {"source", "sourceSha256", "astc", "astcSha256", "width", "height"}
    for entry in manifest["entries"]:
        if set(entry) != entry_keys:
            raise InputContractError(f"final manifest entry keys are not exact: {entry}")
        ensure_sha256(entry["sourceSha256"], "final sourceSha256")
        ensure_sha256(entry["astcSha256"], "final astcSha256")
        if not isinstance(entry["source"], str) or not isinstance(entry["astc"], str):
            raise InputContractError("final manifest paths must be strings")
        if as_int(entry["width"], "final width") is None or as_int(entry["height"], "final height") is None:
            raise InputContractError("final manifest dimensions must be positive integers")


def build_arg_parser(default_project_root: Path) -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Build a staging-only ASTC 4x4 candidate from verified Logan body pages."
    )
    parser.add_argument(
        "--project-root",
        default=str(default_project_root),
        help="repository root (default: the repository containing this script)",
    )
    parser.add_argument("--inventory", required=True, help="body image inventory JSON")
    parser.add_argument(
        "--pages-manifest",
        default="Assets/NTSD/Content/LoganRuntime/derived_sprite_pages/manifest.json",
        help="derived pages manifest relative to project root",
    )
    parser.add_argument(
        "--encoder",
        default=None,
        help="explicit astcenc executable; omitting this uses the pinned SSE2 executable",
    )
    parser.add_argument(
        "--output",
        required=True,
        help="new staging directory below project-root/artifacts; it must not already exist",
    )
    parser.add_argument("--jobs", type=int, default=4, help="bounded Python encode workers (default: 4)")
    parser.add_argument("--limit", type=int, default=None, help="encode only the first N selected physical pages")
    parser.add_argument(
        "--decode-review",
        nargs="?",
        const="all",
        default=None,
        metavar="N|all",
        help="after encoding, decode the first N unique blocks or all blocks with astcenc -ds",
    )
    parser.add_argument(
        "--allow-missing-inventory-sha",
        action="store_true",
        help="legacy diagnostic escape hatch; records missing source SHA instead of accepting it as verified",
    )
    return parser


def run(args: argparse.Namespace) -> dict[str, Any]:
    project_root = Path(args.project_root).resolve(strict=False)
    if not project_root.exists() or not project_root.is_dir():
        raise InputContractError(f"project root does not exist: {project_root}")
    if args.jobs <= 0:
        raise InputContractError("--jobs must be positive")
    if args.limit is not None and args.limit <= 0:
        raise InputContractError("--limit must be positive")
    decode_limit = parse_decode_review(args.decode_review)

    output_root = path_from_arg(project_root, args.output)
    artifacts_root = (project_root / "artifacts").resolve(strict=False)
    if not is_within(output_root, artifacts_root) or output_root == artifacts_root:
        raise InputContractError(
            f"--output must be a child of project-root/artifacts: {output_root}"
        )
    if os.path.lexists(str(output_root)):
        raise InputContractError(f"refusing to reuse existing output directory: {output_root}")

    inventory_path = path_from_arg(project_root, args.inventory)
    pages_manifest_path = path_from_arg(project_root, args.pages_manifest)
    if not inventory_path.exists() or not inventory_path.is_file():
        raise InputContractError(f"inventory does not exist: {inventory_path}")
    if not pages_manifest_path.exists() or not pages_manifest_path.is_file():
        raise InputContractError(f"pages manifest does not exist: {pages_manifest_path}")

    content_root = pages_manifest_path.parent.parent.resolve(strict=False)
    if not content_root.exists() or not content_root.is_dir():
        raise InputContractError(f"derived pages ContentRoot does not exist: {content_root}")

    if args.encoder:
        encoder_path = path_from_arg(project_root, args.encoder)
        explicit_encoder = True
    else:
        encoder_path = (project_root / DEFAULT_ENCODER_RELATIVE).resolve(strict=False)
        explicit_encoder = False

    # All read-only checks happen before the new staging directory is created.
    records, inventory_metadata = load_inventory(
        inventory_path,
        content_root,
        allow_missing_sha=args.allow_missing_inventory_sha,
    )
    source_receipts = verify_source_inputs(records)
    pages, page_to_sources, pages_metadata = verify_pages_manifest(
        pages_manifest_path,
        content_root,
    )
    encoder_metadata = encoder_identity(encoder_path, project_root, explicit_encoder)
    # Unpaged regular sheets are physical sources too; mapped originals are not uploaded.
    mapped_keys = {key for keys in page_to_sources.values() for key in keys}
    for record in records.values():
        if not record.body_referenced or record.key in mapped_keys:
            continue
        with Image.open(record.path) as image:
            rgba = image.convert("RGBA")
            width, height = rgba.size
            pixel_sha = sha256_bytes(rgba.tobytes())
        identifier = "native:" + record.key
        relative = record.path.relative_to(content_root).as_posix()
        actual_hash = sha256_file(record.path)
        pages[identifier] = PageRecord(identifier, relative, record.path, relative, actual_hash,
            actual_hash, width, height, pixel_sha, [record.key])
        page_to_sources[identifier] = [record.key]

    # Fully baked, validated derived pages contain the native edge-clamp result.
    # Raw clamped sources stay excluded; only their bounded replacement pages qualify.
    baked_keys = {source_key(source["path"], content_root)
        for source in json_load(pages_manifest_path).get("sources", [])
        if source.get("clampBaked") is True}
    selection_records = {key: dataclasses.replace(record, clamped_cell_refs=[])
        if key in baked_keys else record for key, record in records.items()}
    page_jobs, jobs, clamped_exclusions = build_selection(
        selection_records,
        pages,
        page_to_sources,
        args.limit,
    )

    # Detect path collisions before touching the new staging directory.
    prepared_paths = [job["preparedRelative"] for job in jobs]
    astc_paths = [job["astcRelative"] for job in jobs]
    if len(prepared_paths) != len(set(prepared_paths)) or len(astc_paths) != len(set(astc_paths)):
        raise InputContractError("derived prepared/ASTC names collide before encoding")

    output_root.mkdir(parents=True, exist_ok=False)
    try:
        write_json_new(output_root / "encoder-identity.json", encoder_metadata)
        write_json_new(
            output_root / "input-validation.json",
            {
                "utc": utc_now(),
                "pixelContract": PIXEL_CONTRACT,
                "inventory": inventory_metadata,
                "bodySourceReceipts": source_receipts,
                "pagesManifest": pages_metadata,
                "clampedBodySources": [
                    {
                        "sourceKey": record.key,
                        "path": record.path_value,
                        "clampedCellRefCount": record.clamped_count,
                        "decision": "RGBA compatibility; no ASTC substitution",
                    }
                    for record in sorted(records.values(), key=lambda item: item.key)
                    if record.body_referenced and record.clamped
                ],
            },
        )

        for job in jobs:
            prepare_png(job, output_root)
        jobs_by_key = {job["blockKey"]: job for job in jobs}
        for page in page_jobs:
            prepared_job = jobs_by_key[page["blockKey"]]
            page["preparedPngSha256"] = prepared_job["preparedPngSha256"]
            page["preparedPixelSha256"] = prepared_job["preparedPixelSha256"]
            page["preparedBytes"] = prepared_job["preparedBytes"]

        output_plan = {
            "schemaVersion": 1,
            "utc": utc_now(),
            "stageOnly": True,
            "projectRoot": str(project_root),
            "contentRoot": str(content_root),
            "pixelContract": PIXEL_CONTRACT,
            "blockDimensions": list(ASTC_BLOCK),
            "encoderThreads": ENCODER_THREADS,
            "jobs": args.jobs,
            "limit": args.limit,
            "decodeReview": args.decode_review,
            "inventory": inventory_metadata,
            "pagesManifest": pages_metadata,
            "encoder": encoder_metadata,
            "selection": {
                "selectedPhysicalPageCount": len(page_jobs),
                "uniqueBlockCount": len(jobs),
                "pageEntries": page_jobs,
                "uniqueBlocks": jobs,
                "clampedExclusions": clamped_exclusions,
            },
            "orientationProtocol": {
                "sourcePng": "Pillow top-left RGBA",
                "preparedPng": "FLIP_TOP_BOTTOM before astcenc -cs",
                "unityContract": PIXEL_CONTRACT,
                "decodeReview": "retain rawdecode, then FLIP_TOP_BOTTOM to upright",
            },
        }
        write_json_new(output_root / "output-plan.json", output_plan)

        failures: list[str] = []
        results: dict[str, dict[str, Any]] = {}
        with concurrent.futures.ThreadPoolExecutor(max_workers=args.jobs) as executor:
            future_by_key = {
                executor.submit(encode_one, job, output_root, encoder_path, project_root): job["blockKey"]
                for job in jobs
            }
            for future in concurrent.futures.as_completed(future_by_key):
                key = future_by_key[future]
                try:
                    results[key] = future.result()
                except Exception as exc:  # retain all worker logs, then fail closed
                    failures.append(f"{key}: {exc}")
        if failures:
            raise InputContractError(
                "one or more ASTC encodes failed; no final manifest written:\n" + "\n".join(failures)
            )
        if set(results) != {job["blockKey"] for job in jobs}:
            raise InputContractError("ASTC result set is incomplete; no final manifest written")

        review_results: list[dict[str, Any]] = []
        if decode_limit is not None:
            review_jobs = jobs if decode_limit == -1 else jobs[:decode_limit]
            for job in review_jobs:
                review_results.append(
                    decode_review_one(job, output_root, encoder_path, project_root)
                )
            write_json_new(
                output_root / "decode-review.json",
                {
                    "utc": utc_now(),
                    "requested": args.decode_review,
                    "selectedUniqueBlockCount": len(review_jobs),
                    "pixelContract": PIXEL_CONTRACT,
                    "orientation": "rawdecode retained; FLIP_TOP_BOTTOM applied for upright comparison",
                    "results": review_results,
                },
            )

        entries = []
        for page in page_jobs:
            result = results[page["blockKey"]]
            entries.append(
                {
                    "source": page["source"],
                    "sourceSha256": page["sourceSha256"],
                    "astc": page["astcRelative"].replace("\\", "/"),
                    "astcSha256": result["outputSha256"],
                    "width": int(page["width"]),
                    "height": int(page["height"]),
                }
            )
        manifest = {
            "version": 1,
            "pixelContract": PIXEL_CONTRACT,
            "entries": entries,
        }
        validate_final_manifest(manifest)
        write_json_new(output_root / "manifest.json", manifest)

        summary = {
            "status": "PASS",
            "utc": utc_now(),
            "manifest": str(output_root / "manifest.json"),
            "manifestSha256": sha256_file(output_root / "manifest.json"),
            "selectedPhysicalPageCount": len(page_jobs),
            "uniqueBlockCount": len(jobs),
            "encodedBlockCount": len(results),
            "decodeReviewCount": len(review_results),
            "pixelContract": PIXEL_CONTRACT,
            "clampedExcludedPageCount": len(clamped_exclusions),
            "outputRoot": str(output_root),
        }
        write_json_new(output_root / "summary.json", summary)
        print(json.dumps(summary, ensure_ascii=False), flush=True)
        return summary
    except Exception as exc:
        # The staging directory and all completed logs are deliberately kept.
        # No cleanup or overwrite is attempted after a failure.
        failure_path = output_root / "failure.json"
        if output_root.exists() and not os.path.lexists(str(failure_path)):
            try:
                write_json_new(
                    failure_path,
                    {
                        "utc": utc_now(),
                        "status": "FAILED_PARTIAL_OUTPUT_RETAINED",
                        "errorType": type(exc).__name__,
                        "error": str(exc),
                        "outputRoot": str(output_root),
                        "finalManifestWritten": (output_root / "manifest.json").exists(),
                    },
                )
            except Exception:
                pass
        raise


def main(argv: Optional[list[str]] = None) -> int:
    default_project_root = Path(__file__).resolve().parents[1]
    parser = build_arg_parser(default_project_root)
    args = parser.parse_args(argv)
    run(args)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
