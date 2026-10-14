#!/usr/bin/env python3
"""Build and validate a bounded four-source NTSD DAT sprite paging pilot.

This helper is deliberately offline and staging-only. It reads the current
catalog/DAT/source PNGs, generates derived RGBA pages under the M12 artifact
directory, validates every logical frame, and optionally runs the already
identified astcenc binary against those derived pages. It never writes under
Assets and never changes a DAT.
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import io
import json
import math
import re
import subprocess
import time
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Iterable

import numpy as np
from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
CONTENT_ROOT = ROOT / "Assets/NTSD/Content/LoganRuntime"
VFS_ROOT = CONTENT_ROOT / "vfs"
DAT_ROOT = CONTENT_ROOT / "decoded_dat"
ARTIFACT_ROOT = ROOT / "artifacts/diagnostics/M12-sprite-paging-pilot-20261013"
GENERATED_ROOT = ARTIFACT_ROOT / "generated"
RESULTS_ROOT = ARTIFACT_ROOT / "generator-results"
PAGE_ROOT = GENERATED_ROOT / "pages"
ASTC_ROOT = RESULTS_ROOT / "astc"
COMPARISON_ROOT = RESULTS_ROOT / "comparisons"
MONTAGE_ROOT = RESULTS_ROOT / "montage"
REVISION_ROOT = RESULTS_ROOT / "revisions"
PRIOR_ASTC_TOOL_IDENTITY = ROOT / (
    "artifacts/diagnostics/M12-astc-sample-20261012/encoder/tool-identity.json"
)
ASTC_EXE = ROOT / (
    "artifacts/diagnostics/M12-astc-sample-20261012/encoder/bin/astcenc-sse2.exe"
)

GENERATOR_VERSION = "NTSD_DAT_PAGES_V1"
PAGE_SIZE = 2048
EDGE_PADDING = 1
ALLOCATION_ALIGNMENT = 4

SOURCE_PATHS = [
    "c/dan/a/win2.png",
    "c/sasu/a/Kirin.png",
    "c/sui/sui3.png",
    "c/kar/a/sui.png",
]

FILE_RE = re.compile(
    r"^\s*file\((?P<start>\d+)(?:-(?P<end>\d+))?\):\s*"
    r"(?P<source>\S+)\s+w:\s*(?P<width>\d+)\s+h:\s*(?P<height>\d+)\s+"
    r"row:\s*(?P<columns>\d+)\s+col:\s*(?P<rows>\d+)\s*$",
    re.IGNORECASE,
)


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat()


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def rel_root(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def normalize_path(value: str) -> str:
    return value.strip().replace("\\", "/").lstrip("/").lower()


def ensure_parent(path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)


def write_new_bytes(path: Path, data: bytes) -> None:
    ensure_parent(path)
    with path.open("xb") as stream:
        stream.write(data)


def write_new_json(path: Path, value: Any) -> None:
    payload = (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
    write_new_bytes(path, payload)


def write_new_text(path: Path, value: str) -> None:
    write_new_bytes(path, value.encode("utf-8"))


def image_png_bytes(image: Image.Image) -> bytes:
    buffer = io.BytesIO()
    image.save(buffer, format="PNG", compress_level=9)
    return buffer.getvalue()


def write_new_png(path: Path, image: Image.Image) -> dict[str, Any]:
    payload = image_png_bytes(image)
    write_new_bytes(path, payload)
    return {
        "path": rel_root(path),
        "bytes": len(payload),
        "sha256": sha256_bytes(payload),
        "width": image.width,
        "height": image.height,
    }


def align4(value: int) -> int:
    return (value + ALLOCATION_ALIGNMENT - 1) // ALLOCATION_ALIGNMENT * ALLOCATION_ALIGNMENT


def metadata(path: Path) -> dict[str, Any]:
    data = path.read_bytes()
    return {
        "path": rel_root(path),
        "bytes": len(data),
        "sha256": sha256_bytes(data),
    }


def image_metadata(path: Path) -> dict[str, Any]:
    result = metadata(path)
    with Image.open(path) as image:
        result.update({"width": image.width, "height": image.height, "mode": image.mode})
    return result


def parse_catalog() -> dict[str, list[dict[str, str]]]:
    catalog_path = CONTENT_ROOT / "catalog.csv"
    with catalog_path.open("r", encoding="utf-8-sig", newline="") as stream:
        rows = list(csv.DictReader(stream))
    by_dat: dict[str, list[dict[str, str]]] = {}
    for row in rows:
        key = normalize_path(row["source_path"])
        by_dat.setdefault(key, []).append(row)
    return by_dat


def parse_dat_file(path: Path) -> list[dict[str, Any]]:
    dat_relative = path.relative_to(DAT_ROOT).as_posix()
    cumulative_native = 0
    declarations: list[dict[str, Any]] = []
    text = path.read_text(encoding="utf-8-sig")
    for line_number, line in enumerate(text.splitlines(), start=1):
        match = FILE_RE.match(line)
        if not match:
            continue
        start = int(match.group("start"))
        end_text = match.group("end")
        width = int(match.group("width"))
        height = int(match.group("height"))
        columns = int(match.group("columns"))
        rows = int(match.group("rows"))
        count = columns * rows
        # Keep authored labels only as metadata. Native firstPic/lastPic below
        # always use cumulative row*col, independent of either label spelling.
        end = int(end_text) if end_text is not None else start + count - 1
        declared_count = end - start + 1
        source_path = match.group("source").strip()
        # Both file(x-y) and file(x) declarations are normalized to the
        # row*col span before the target declaration is checked.
        declarations.append(
            {
                "datPath": dat_relative,
                "line": line_number,
                "sourcePath": source_path.replace("\\", "/"),
                "sourceKey": normalize_path(source_path),
                "declaredStart": start,
                "declaredEnd": end,
                "declaredCount": declared_count,
                "width": width,
                "height": height,
                "columns": columns,
                "rows": rows,
                "count": count,
                "firstPic": cumulative_native,
                "lastPic": cumulative_native + count - 1,
            }
        )
        cumulative_native += count
    return declarations


def parse_all_dat_declarations() -> dict[str, list[dict[str, Any]]]:
    result: dict[str, list[dict[str, Any]]] = {}
    for path in sorted(DAT_ROOT.rglob("*.dat"), key=lambda item: item.as_posix().lower()):
        for declaration in parse_dat_file(path):
            result.setdefault(declaration["sourceKey"], []).append(declaration)
    return result


def selected_declarations() -> tuple[dict[str, list[dict[str, str]]], dict[str, list[dict[str, Any]]]]:
    catalog = parse_catalog()
    declarations = parse_all_dat_declarations()
    for source_rel in SOURCE_PATHS:
        key = normalize_path(source_rel)
        if key not in declarations:
            raise RuntimeError(f"DAT declaration missing for selected source: {source_rel}")
        for declaration in declarations[key]:
            dat_key = normalize_path(declaration["datPath"])
            rows = catalog.get(dat_key, [])
            if len(rows) != 1:
                raise RuntimeError(
                    f"catalog must independently resolve exactly one row for "
                    f"{declaration['datPath']}; got {len(rows)}"
                )
            if declaration["declaredCount"] != declaration["count"]:
                raise RuntimeError(
                    f"DAT range/count mismatch at {declaration['datPath']}:{declaration['line']}: "
                    f"file range {declaration['declaredStart']}-{declaration['declaredEnd']} "
                    f"versus row*col {declaration['count']}"
                )
    return catalog, declarations


def source_absolute(source_rel: str) -> Path:
    return VFS_ROOT / Path(*source_rel.split("/"))


def dat_absolute(dat_rel: str) -> Path:
    return DAT_ROOT / Path(*dat_rel.split("/"))


def page_id(source_sha: str, frame_width: int, frame_height: int, page_columns: int,
            page_rows: int, start: int, count: int) -> str:
    # The source digest and all page geometry are intentionally visible in the
    # identity. A source basename alone is never enough for deduplication.
    return (
        f"page-{source_sha[:16]}-{frame_width}x{frame_height}-"
        f"{page_columns}x{page_rows}-{start:04d}-{count:03d}"
    )


def load_oracle_checks(
    sources: list[dict[str, Any]],
    pages: list[dict[str, Any]],
    oracle_path: Path | None,
) -> dict[str, Any]:
    if oracle_path is None:
        return {
            "status": "SKIPPED",
            "available": False,
            "path": None,
            "reason": "no --oracle-path supplied",
            "checks": [],
        }
    if not oracle_path.exists():
        return {
            "status": "SKIPPED",
            "available": False,
            "path": str(oracle_path),
            "reason": "oracle path does not exist",
            "checks": [],
        }
    oracle = json.loads(oracle_path.read_text(encoding="utf-8"))
    oracle_sheets = {}
    selected_keys = {normalize_path(x) for x in SOURCE_PATHS}
    for sheet in oracle.get("sheets", []):
        oracle_path = normalize_path(sheet.get("path", ""))
        if "/vfs/" in oracle_path:
            oracle_path = oracle_path.split("/vfs/", 1)[1]
        if oracle_path in selected_keys:
            oracle_sheets[oracle_path] = sheet
    checks: list[dict[str, Any]] = []
    for source in sources:
        sheet = oracle_sheets.get(normalize_path(source["path"]))
        if sheet is None:
            raise RuntimeError(f"oracle sheet missing: {source['path']}")
        geometry_match = all(
            [
                sheet["w"] == source["width"],
                sheet["h"] == source["height"],
                sheet["cellW"] == source["frameWidth"],
                sheet["cellH"] == source["frameHeight"],
                sheet["columns"] == source["columns"],
                sheet["rows"] == source["rows"],
            ]
        )
        own = [
            {
                "startLocalPic": page["firstLocalPic"],
                "count": page["count"],
                "columns": page["columns"],
                "rows": page["rows"],
                "width": page["width"],
                "height": page["height"],
            }
            for page in source["chunks"]
        ]
        expected = [
            {
                "startLocalPic": page["startLocalPic"],
                "count": page["count"],
                "columns": page["columns"],
                "rows": page["rows"],
                "width": page["width"],
                "height": page["height"],
            }
            for page in sheet.get("layout4x4", {}).get("pages", [])
        ]
        page_match = own == expected
        if not geometry_match or not page_match:
            raise RuntimeError(f"independent geometry disagrees with oracle for {source['path']}")
        checks.append(
            {
                "path": source["path"],
                "geometryMatch": geometry_match,
                "pageLayoutMatch": page_match,
                "oraclePageCount": len(expected),
            }
        )
    return {
        "status": "PASS",
        "available": True,
        "path": str(oracle_path),
        "reason": None,
        "checks": checks,
    }


def build_plan(oracle_path: Path | None = None) -> dict[str, Any]:
    catalog, declarations_by_source = selected_declarations()
    sources: list[dict[str, Any]] = []
    identity_cache: dict[str, list[dict[str, Any]]] = {}
    pages: list[dict[str, Any]] = []

    for source_rel in SOURCE_PATHS:
        path = source_absolute(source_rel)
        if not path.exists():
            raise RuntimeError(f"source image missing: {path}")
        source_meta = image_metadata(path)
        matched = declarations_by_source[normalize_path(source_rel)]
        geometries = {(x["width"], x["height"], x["columns"], x["rows"]) for x in matched}
        if len(geometries) != 1:
            raise RuntimeError(f"selected source has inconsistent DAT geometry: {source_rel}")
        frame_width, frame_height, columns, rows = next(iter(geometries))
        if columns * rows <= 0:
            raise RuntimeError(f"invalid DAT geometry: {source_rel}")
        with Image.open(path) as image:
            source_width, source_height = image.size
        minimum_width = (frame_width + 1) * columns
        minimum_height = (frame_height + 1) * rows
        if source_width < minimum_width or source_height < minimum_height:
            raise RuntimeError(
                f"source does not contain all DAT cells: {source_rel} "
                f"{source_width}x{source_height} < {minimum_width}x{minimum_height}"
            )
        frame_span_width = align4(frame_width + 2 * EDGE_PADDING)
        frame_span_height = align4(frame_height + 2 * EDGE_PADDING)
        max_page_columns = PAGE_SIZE // frame_span_width
        max_page_rows = PAGE_SIZE // frame_span_height
        if max_page_columns < 1 or max_page_rows < 1:
            raise RuntimeError(f"frame cannot fit on a 2048 page: {source_rel}")
        source_identity = (
            f"{source_meta['sha256']}:{frame_width}:{frame_height}:{columns}:{rows}"
        )

        bindings: list[dict[str, Any]] = []
        for declaration in matched:
            dat_path = dat_absolute(declaration["datPath"])
            dat_meta = metadata(dat_path)
            catalog_rows = catalog[normalize_path(declaration["datPath"])]
            oid = int(catalog_rows[0]["id"])
            bindings.append(
                {
                    "oid": oid,
                    "datPath": declaration["datPath"],
                    "datSha256": dat_meta["sha256"],
                    "datBytes": dat_meta["bytes"],
                    "firstPic": declaration["firstPic"],
                    "lastPic": declaration["lastPic"],
                    "datLine": declaration["line"],
                    "catalogSourcePath": catalog_rows[0]["source_path"],
                }
            )

        if source_identity not in identity_cache:
            unique_pages: list[dict[str, Any]] = []
            capacity = max_page_columns * max_page_rows
            for start in range(0, columns * rows, capacity):
                count = min(capacity, columns * rows - start)
                page_columns = max_page_columns
                page_rows = math.ceil(count / page_columns)
                width = page_columns * frame_span_width
                height = page_rows * frame_span_height
                identity_page_id = page_id(
                    source_meta["sha256"], frame_width, frame_height,
                    page_columns, page_rows, start, count
                )
                frames: list[dict[str, Any]] = []
                for offset in range(count):
                    local_pic = start + offset
                    source_x = (local_pic % columns) * (frame_width + 1)
                    source_y = (local_pic // columns) * (frame_height + 1)
                    page_col = offset % page_columns
                    page_row = offset // page_columns
                    page_x = page_col * frame_span_width + EDGE_PADDING
                    page_y_top = page_row * frame_span_height + EDGE_PADDING
                    rect_y_bottom = height - (page_y_top + frame_height)
                    frames.append(
                        {
                            "localPic": local_pic,
                            "sourceTopLeft": [source_x, source_y, frame_width, frame_height],
                            "pageTopLeft": [page_x, page_y_top, frame_width, frame_height],
                            "pageBottomLeft": [
                                page_x, rect_y_bottom, frame_width, frame_height
                            ],
                        }
                    )
                page = {
                    "id": identity_page_id,
                    "path": f"pages/{identity_page_id}.png",
                    "sourceIdentity": source_identity,
                    "sourcePath": source_rel,
                    "sourceSha256": source_meta["sha256"],
                    "frameWidth": frame_width,
                    "frameHeight": frame_height,
                    "sourceColumns": columns,
                    "sourceRows": rows,
                    "startLocalPic": start,
                    "count": count,
                    "columns": page_columns,
                    "rows": page_rows,
                    "width": width,
                    "height": height,
                    "frameSpanWidth": frame_span_width,
                    "frameSpanHeight": frame_span_height,
                    "frames": frames,
                }
                unique_pages.append(page)
                pages.append(page)
            identity_cache[source_identity] = unique_pages

        chunks = []
        for page in identity_cache[source_identity]:
            chunks.append(
                {
                    "page": page["id"],
                    "firstLocalPic": page["startLocalPic"],
                    "count": page["count"],
                    "columns": page["columns"],
                    "rows": page["rows"],
                    "width": page["width"],
                    "height": page["height"],
                    "rects": [frame["pageBottomLeft"] for frame in page["frames"]],
                }
            )
        sources.append(
            {
                "path": source_rel,
                "sourceFile": rel_root(path),
                "sha256": source_meta["sha256"],
                "bytes": source_meta["bytes"],
                "width": source_width,
                "height": source_height,
                "frameWidth": frame_width,
                "frameHeight": frame_height,
                "columns": columns,
                "rows": rows,
                "sourceIdentity": source_identity,
                "bindings": bindings,
                "chunks": chunks,
            }
        )

    dat_inputs: dict[str, dict[str, Any]] = {}
    for source in sources:
        for binding in source["bindings"]:
            dat_rel = binding["datPath"]
            dat_inputs.setdefault(
                dat_rel,
                {
                    "path": rel_root(dat_absolute(dat_rel)),
                    "datPath": dat_rel,
                    "bytes": binding["datBytes"],
                    "sha256": binding["datSha256"],
                },
            )

    oracle_checks = load_oracle_checks(sources, pages, oracle_path)
    expected_logical_mappings = sum(
        source["columns"] * source["rows"] * len(source["bindings"]) for source in sources
    )
    unique_identities = {source["sourceIdentity"] for source in sources}
    expected_unique_frames = sum(
        source["columns"] * source["rows"]
        for source in sources
        if source["sourceIdentity"] in unique_identities
        and next(s for s in sources if s["sourceIdentity"] == source["sourceIdentity"]) is source
    )
    return {
        "version": 1,
        "generatorVersion": GENERATOR_VERSION,
        "createdUtc": utc_now(),
        "manifestPath": rel_root(GENERATED_ROOT / "manifest.json"),
        "pageDirectory": rel_root(PAGE_ROOT),
        "sourceInputs": sources,
        "datInputs": list(dat_inputs.values()),
        "pages": pages,
        "expected": {
            "pageCount": len(pages),
            "logicalMappingCount": expected_logical_mappings,
            "uniqueFrameCount": expected_unique_frames,
            "uniqueSourceIdentityCount": len(unique_identities),
        },
        "oracle": oracle_checks,
    }


def write_planned_manifest(plan: dict[str, Any]) -> None:
    planned = {
        "version": plan["version"],
        "generatorVersion": plan["generatorVersion"],
        "createdUtc": plan["createdUtc"],
        "manifestPath": plan["manifestPath"],
        "pageDirectory": plan["pageDirectory"],
        "sourceInputs": [
            {
                "path": source["path"],
                "sourceFile": source["sourceFile"],
                "sha256": source["sha256"],
                "bytes": source["bytes"],
                "width": source["width"],
                "height": source["height"],
                "frameWidth": source["frameWidth"],
                "frameHeight": source["frameHeight"],
                "columns": source["columns"],
                "rows": source["rows"],
                "bindings": [
                    {
                        "oid": binding["oid"],
                        "datPath": binding["datPath"],
                        "datFile": rel_root(dat_absolute(binding["datPath"])),
                        "datSha256": binding["datSha256"],
                        "datBytes": binding["datBytes"],
                        "firstPic": binding["firstPic"],
                        "lastPic": binding["lastPic"],
                    }
                    for binding in source["bindings"]
                ],
                "chunks": [
                    {
                        "page": chunk["page"],
                        "path": f"{plan['pageDirectory']}/{chunk['page']}.png",
                        "firstLocalPic": chunk["firstLocalPic"],
                        "count": chunk["count"],
                        "columns": chunk["columns"],
                        "rows": chunk["rows"],
                        "width": chunk["width"],
                        "height": chunk["height"],
                    }
                    for chunk in source["chunks"]
                ],
            }
            for source in plan["sourceInputs"]
        ],
        "datInputs": plan["datInputs"],
        "plannedPages": [
            {
                "id": page["id"],
                "path": page["path"],
                "sourcePath": page["sourcePath"],
                "sourceSha256": page["sourceSha256"],
                "frameWidth": page["frameWidth"],
                "frameHeight": page["frameHeight"],
                "columns": page["columns"],
                "rows": page["rows"],
                "startLocalPic": page["startLocalPic"],
                "count": page["count"],
                "width": page["width"],
                "height": page["height"],
            }
            for page in plan["pages"]
        ],
        "expected": plan["expected"],
        "oracle": plan["oracle"],
    }
    write_new_json(GENERATED_ROOT / "planned-output-manifest.json", planned)


def load_plan() -> dict[str, Any]:
    path = GENERATED_ROOT / "planned-output-manifest.json"
    if not path.exists():
        raise RuntimeError(f"planned manifest missing: {path}")
    planned = json.loads(path.read_text(encoding="utf-8"))
    # Rebuild the authoritative internal plan from the current catalog/DAT and
    # compare the frozen plan's identity inputs before writing any derived file.
    current = build_plan()
    if planned["generatorVersion"] != current["generatorVersion"]:
        raise RuntimeError("planned manifest generator version mismatch")
    if planned["expected"] != current["expected"]:
        raise RuntimeError("planned manifest expected counts changed")
    planned_sources = planned["sourceInputs"]
    current_sources = current["sourceInputs"]
    for planned_source, current_source in zip(planned_sources, current_sources):
        for key in ("path", "sha256", "bytes", "width", "height", "frameWidth", "frameHeight", "columns", "rows"):
            if planned_source[key] != current_source[key]:
                raise RuntimeError(f"planned source changed for {planned_source['path']}: {key}")
        if [x["datPath"] for x in planned_source["bindings"]] != [x["datPath"] for x in current_source["bindings"]]:
            raise RuntimeError(f"planned DAT bindings changed for {planned_source['path']}")
    current["plannedPath"] = rel_root(path)
    return current


def verify_current_inputs(plan: dict[str, Any]) -> None:
    for source in plan["sourceInputs"]:
        path = ROOT / source["sourceFile"]
        current = metadata(path)
        if current["sha256"] != source["sha256"] or current["bytes"] != source["bytes"]:
            raise RuntimeError(f"source changed since plan: {source['path']}")
    for dat in plan["datInputs"]:
        path = ROOT / dat["path"]
        current = metadata(path)
        if current["sha256"] != dat["sha256"] or current["bytes"] != dat["bytes"]:
            raise RuntimeError(f"DAT changed since plan: {dat['datPath']}")


def build_page_image(source_image: Image.Image, page: dict[str, Any]) -> Image.Image:
    page_image = Image.new("RGBA", (page["width"], page["height"]), (0, 0, 0, 0))
    frame_width = page["frameWidth"]
    frame_height = page["frameHeight"]
    for frame in page["frames"]:
        source_x, source_y, _, _ = frame["sourceTopLeft"]
        page_x, page_y, _, _ = frame["pageTopLeft"]
        crop = source_image.crop((source_x, source_y, source_x + frame_width, source_y + frame_height))
        page_image.paste(crop, (page_x, page_y))
        # One-pixel edge extrusion. The remaining alignment stride is left
        # transparent and is checked as such by validate_page.
        page_image.paste(crop.crop((0, 0, 1, frame_height)), (page_x - 1, page_y))
        page_image.paste(crop.crop((frame_width - 1, 0, frame_width, frame_height)),
                         (page_x + frame_width, page_y))
        page_image.paste(crop.crop((0, 0, frame_width, 1)), (page_x, page_y - 1))
        page_image.paste(crop.crop((0, frame_height - 1, frame_width, frame_height)),
                         (page_x, page_y + frame_height))
        corner_pixels = {
            (page_x - 1, page_y - 1): crop.getpixel((0, 0)),
            (page_x + frame_width, page_y - 1): crop.getpixel((frame_width - 1, 0)),
            (page_x - 1, page_y + frame_height): crop.getpixel((0, frame_height - 1)),
            (page_x + frame_width, page_y + frame_height): crop.getpixel(
                (frame_width - 1, frame_height - 1)
            ),
        }
        for location, pixel in corner_pixels.items():
            page_image.putpixel(location, pixel)
    return page_image


def rects_overlap(first: tuple[int, int, int, int], second: tuple[int, int, int, int]) -> bool:
    first_x, first_y, first_w, first_h = first
    second_x, second_y, second_w, second_h = second
    return not (
        first_x + first_w <= second_x
        or second_x + second_w <= first_x
        or first_y + first_h <= second_y
        or second_y + second_h <= first_y
    )


def validate_page(page_path: Path, page: dict[str, Any], source_image: Image.Image) -> dict[str, Any]:
    with Image.open(page_path) as image:
        page_image = image.convert("RGBA")
    if page_image.size != (page["width"], page["height"]):
        raise RuntimeError(f"page size mismatch: {page_path}")
    page_array = np.asarray(page_image)
    occupied = np.zeros((page["height"], page["width"]), dtype=np.uint8)
    allocation_rects: list[tuple[int, int, int, int]] = []
    frame_matches = 0
    edge_matches = 0
    alignment_pixels = 0
    alignment_zero_pixels = 0
    source_frame_digests: list[str] = []
    for frame in page["frames"]:
        source_x, source_y, frame_width, frame_height = frame["sourceTopLeft"]
        page_x, page_y, _, _ = frame["pageTopLeft"]
        source_crop = source_image.crop((source_x, source_y, source_x + frame_width, source_y + frame_height))
        source_array = np.asarray(source_crop)
        page_crop = page_array[page_y:page_y + frame_height, page_x:page_x + frame_width]
        if not np.array_equal(source_array, page_crop):
            raise RuntimeError(f"frame byte mismatch at {page_path}, localPic={frame['localPic']}")
        frame_matches += 1
        source_frame_digests.append(sha256_bytes(source_crop.tobytes()))
        if not np.array_equal(page_array[page_y:page_y + frame_height, page_x - 1], source_array[:, 0]):
            raise RuntimeError(f"left edge extrusion mismatch at {page_path}")
        if not np.array_equal(page_array[page_y:page_y + frame_height, page_x + frame_width], source_array[:, -1]):
            raise RuntimeError(f"right edge extrusion mismatch at {page_path}")
        if not np.array_equal(page_array[page_y - 1, page_x:page_x + frame_width], source_array[0, :]):
            raise RuntimeError(f"top edge extrusion mismatch at {page_path}")
        if not np.array_equal(page_array[page_y + frame_height, page_x:page_x + frame_width], source_array[-1, :]):
            raise RuntimeError(f"bottom edge extrusion mismatch at {page_path}")
        if page_image.getpixel((page_x - 1, page_y - 1)) != source_crop.getpixel((0, 0)):
            raise RuntimeError(f"top-left corner extrusion mismatch at {page_path}")
        if page_image.getpixel((page_x + frame_width, page_y - 1)) != source_crop.getpixel((frame_width - 1, 0)):
            raise RuntimeError(f"top-right corner extrusion mismatch at {page_path}")
        if page_image.getpixel((page_x - 1, page_y + frame_height)) != source_crop.getpixel((0, frame_height - 1)):
            raise RuntimeError(f"bottom-left corner extrusion mismatch at {page_path}")
        if page_image.getpixel((page_x + frame_width, page_y + frame_height)) != source_crop.getpixel((frame_width - 1, frame_height - 1)):
            raise RuntimeError(f"bottom-right corner extrusion mismatch at {page_path}")
        edge_matches += 1

        slot_x = page_x - EDGE_PADDING
        slot_y = page_y - EDGE_PADDING
        allocation = (slot_x, slot_y, page["frameSpanWidth"], page["frameSpanHeight"])
        if slot_x < 0 or slot_y < 0 or slot_x + allocation[2] > page["width"] or slot_y + allocation[3] > page["height"]:
            raise RuntimeError(f"allocation exceeds page bounds at {page_path}")
        for previous in allocation_rects:
            if rects_overlap(previous, allocation):
                raise RuntimeError(f"allocation rectangles overlap at {page_path}")
        allocation_rects.append(allocation)
        occupied[slot_y:slot_y + allocation[3], slot_x:slot_x + allocation[2]] += 1

        allowed_x0 = page_x - EDGE_PADDING
        allowed_y0 = page_y - EDGE_PADDING
        allowed_x1 = page_x + frame_width + EDGE_PADDING
        allowed_y1 = page_y + frame_height + EDGE_PADDING
        slot_array = page_array[slot_y:slot_y + allocation[3], slot_x:slot_x + allocation[2]]
        allowed = np.zeros(slot_array.shape[:2], dtype=bool)
        allowed[0:allowed_y1 - allowed_y0, 0:allowed_x1 - allowed_x0] = True
        outside_edge = slot_array[~allowed]
        alignment_pixels += int(outside_edge.shape[0])
        if outside_edge.size:
            zero = np.all(outside_edge == 0, axis=1)
            alignment_zero_pixels += int(zero.sum())
            if not np.all(zero):
                raise RuntimeError(f"nonzero alignment padding at {page_path}")

    if np.any(occupied > 1):
        raise RuntimeError(f"overlapping occupied page pixels at {page_path}")
    clear_pixels = page_array[occupied == 0]
    clear_zero = int(np.all(clear_pixels == 0, axis=1).sum()) if clear_pixels.size else 0
    if clear_pixels.size and clear_zero != clear_pixels.shape[0]:
        raise RuntimeError(f"unused page padding is not transparent RGBA zero at {page_path}")
    if page["width"] > PAGE_SIZE or page["height"] > PAGE_SIZE:
        raise RuntimeError(f"page exceeds 2048 bound: {page_path}")
    if page["width"] % ALLOCATION_ALIGNMENT or page["height"] % ALLOCATION_ALIGNMENT:
        raise RuntimeError(f"page is not allocation aligned: {page_path}")
    return {
        "id": page["id"],
        "path": page["path"],
        "sha256": sha256_file(page_path),
        "bytes": page_path.stat().st_size,
        "width": page["width"],
        "height": page["height"],
        "frameMatches": frame_matches,
        "edgeMatches": edge_matches,
        "alignmentPixels": alignment_pixels,
        "alignmentZeroPixels": alignment_zero_pixels,
        "unusedPagePixels": int(clear_pixels.shape[0]),
        "unusedPageZeroPixels": clear_zero,
        "sourceFrameDigests": source_frame_digests,
    }


def final_manifest(plan: dict[str, Any], page_infos: list[dict[str, Any]]) -> dict[str, Any]:
    info_by_id = {item["id"]: item for item in page_infos}
    pages = [
        {
            "id": page["id"],
            "path": page["path"],
            "sha256": info_by_id[page["id"]]["sha256"],
            "width": page["width"],
            "height": page["height"],
        }
        for page in plan["pages"]
    ]
    sources = []
    for source in plan["sourceInputs"]:
        sources.append(
            {
                "path": source["path"],
                "sha256": source["sha256"],
                "width": source["width"],
                "height": source["height"],
                "frameWidth": source["frameWidth"],
                "frameHeight": source["frameHeight"],
                "columns": source["columns"],
                "rows": source["rows"],
                "bindings": [
                    {
                        "oid": binding["oid"],
                        "datPath": binding["datPath"],
                        "datSha256": binding["datSha256"],
                        "firstPic": binding["firstPic"],
                        "lastPic": binding["lastPic"],
                    }
                    for binding in source["bindings"]
                ],
                "chunks": [
                    {
                        "page": chunk["page"],
                        "firstLocalPic": chunk["firstLocalPic"],
                        "count": chunk["count"],
                        "columns": chunk["columns"],
                        "rows": chunk["rows"],
                        "rects": [
                            {
                                "x": rect[0],
                                "y": rect[1],
                                "width": rect[2],
                                "height": rect[3],
                            }
                            for rect in chunk["rects"]
                        ],
                    }
                    for chunk in source["chunks"]
                ],
            }
        )
    result = {
        "version": 1,
        "generatorVersion": GENERATOR_VERSION,
        "pages": pages,
        "sources": sources,
    }
    expected_top = {"version", "generatorVersion", "pages", "sources"}
    if set(result) != expected_top:
        raise RuntimeError("final manifest top-level schema changed")
    return result


def validate_manifest_schema(manifest: dict[str, Any], plan: dict[str, Any]) -> dict[str, Any]:
    if set(manifest) != {"version", "generatorVersion", "pages", "sources"}:
        raise RuntimeError("manifest top-level schema is not exact")
    if manifest["version"] != 1 or manifest["generatorVersion"] != GENERATOR_VERSION:
        raise RuntimeError("manifest version mismatch")
    if len(manifest["pages"]) != plan["expected"]["pageCount"]:
        raise RuntimeError("manifest page count mismatch")
    if len(manifest["sources"]) != len(SOURCE_PATHS):
        raise RuntimeError("manifest source count mismatch")
    for page in manifest["pages"]:
        if set(page) != {"id", "path", "sha256", "width", "height"}:
            raise RuntimeError("manifest page item schema is not exact")
        if not isinstance(page["width"], int) or not isinstance(page["height"], int):
            raise RuntimeError("manifest page dimensions must be integers")
    for source in manifest["sources"]:
        if set(source) != {
            "path", "sha256", "width", "height", "frameWidth", "frameHeight",
            "columns", "rows", "bindings", "chunks",
        }:
            raise RuntimeError(f"manifest source item schema is not exact: {source['path']}")
        for binding in source["bindings"]:
            if set(binding) != {"oid", "datPath", "datSha256", "firstPic", "lastPic"}:
                raise RuntimeError("manifest binding schema is not exact")
        for chunk in source["chunks"]:
            if set(chunk) != {"page", "firstLocalPic", "count", "columns", "rows", "rects"}:
                raise RuntimeError("manifest chunk schema is not exact")
            for rect in chunk["rects"]:
                if set(rect) != {"x", "y", "width", "height"}:
                    raise RuntimeError("manifest rect schema is not exact")
    return {
        "topLevelExact": True,
        "pageItemsExact": True,
        "sourceItemsExact": True,
        "bindingItemsExact": True,
        "chunkItemsExact": True,
        "rectItemsExact": True,
    }


def source_dat_freshness(plan: dict[str, Any]) -> dict[str, Any]:
    source_rows = []
    for source in plan["sourceInputs"]:
        current = metadata(ROOT / source["sourceFile"])
        source_rows.append(
            {
                "path": source["path"],
                "sourceFile": source["sourceFile"],
                "before": {"sha256": source["sha256"], "bytes": source["bytes"]},
                "after": {"sha256": current["sha256"], "bytes": current["bytes"]},
                "unchanged": current["sha256"] == source["sha256"] and current["bytes"] == source["bytes"],
            }
        )
    dat_rows = []
    for dat in plan["datInputs"]:
        current = metadata(ROOT / dat["path"])
        dat_rows.append(
            {
                "datPath": dat["datPath"],
                "datFile": dat["path"],
                "before": {"sha256": dat["sha256"], "bytes": dat["bytes"]},
                "after": {"sha256": current["sha256"], "bytes": current["bytes"]},
                "unchanged": current["sha256"] == dat["sha256"] and current["bytes"] == dat["bytes"],
            }
        )
    return {
        "sources": source_rows,
        "dats": dat_rows,
        "allUnchanged": all(row["unchanged"] for row in source_rows + dat_rows),
    }


def helper_description() -> str:
    return """# NTSD DAT sprite paging pilot helper

`Tools/Build-NtsdSpritePages.py` is an offline, staging-only helper for
`NTSD-M12-DAT-SPRITE-PAGING-PILOT-20261013`.

Commands, run with `D:/anaconda3/python.exe` from the repository root:

```powershell
D:/anaconda3/python.exe Tools/Build-NtsdSpritePages.py --stage plan
# Optional independent geometry cross-check; omitted means oracle SKIPPED.
D:/anaconda3/python.exe Tools/Build-NtsdSpritePages.py --stage plan --oracle-path C:/path/to/split-feasibility-final.json
D:/anaconda3/python.exe Tools/Build-NtsdSpritePages.py --stage png
D:/anaconda3/python.exe Tools/Build-NtsdSpritePages.py --stage astc
D:/anaconda3/python.exe Tools/Build-NtsdSpritePages.py --stage verify
D:/anaconda3/python.exe Tools/Build-NtsdSpritePages.py --stage compare
```

The plan stage independently reads `catalog.csv` and all current decoded DAT
file declarations. It computes each binding's native `firstPic`/`lastPic` by
cumulative `row * col`, while accepting both `file(x-y)` and `file(x)` forms.
The oracle is optional: without `--oracle-path`, the plan records
`oracle.status = SKIPPED`; DAT parsing and PNG validation remain independent
and are still required. A supplied oracle is only a cross-check and never the
source of DAT cumulative semantics.
The PNG stage uses only the four selected VFS paths and their DAT geometry. It
preserves complete frames, uses one pixel of copied edge extrusion on every
side, aligns each allocation stride to four pixels, does not rotate/trim/scale,
and keeps every page at or below 2048 pixels in both dimensions. `rects.y` in
the final manifest is Unity bottom-left; Pillow writes and reads top-left page
coordinates internally.

The final manifest is `generated/manifest.json` and intentionally contains
only the exact requested schema. All derived pages, ASTC files, decoded pages,
pixel metrics, comparisons, and montages remain below this task's
`artifacts/diagnostics/M12-sprite-paging-pilot-20261013/` staging directory.
No Unity import, scene write, DAT write, Play run, or build is performed.

All stages are create-once and fail closed. Existing planned manifests, page
PNGs, final manifests, ASTC outputs, comparisons, and verification receipts
are treated as evidence and are never overwritten. Later stages read and
reuse the existing plan/results; they do not refresh an existing run. Start a
new task-scoped artifact directory for a new generation.
"""


def run_png_stage() -> None:
    plan = load_plan()
    verify_current_inputs(plan)
    page_paths = [PAGE_ROOT / Path(page["path"]).name for page in plan["pages"]]
    for path in page_paths:
        if path.exists():
            raise RuntimeError(f"refusing to overwrite existing page: {path}")
    manifest_path = GENERATED_ROOT / "manifest.json"
    validation_path = RESULTS_ROOT / "png-validation.json"
    freshness_path = RESULTS_ROOT / "source-dat-freshness.json"
    helper_path = RESULTS_ROOT / "helper-description.md"
    for path in (manifest_path, validation_path, freshness_path, helper_path):
        if path.exists():
            raise RuntimeError(f"refusing to overwrite existing result: {path}")

    source_by_identity: dict[str, dict[str, Any]] = {}
    for source in plan["sourceInputs"]:
        source_by_identity.setdefault(source["sourceIdentity"], source)
    page_infos: list[dict[str, Any]] = []
    for identity, source in source_by_identity.items():
        source_path = ROOT / source["sourceFile"]
        with Image.open(source_path) as image:
            source_image = image.convert("RGBA")
        identity_pages = [page for page in plan["pages"] if page["sourceIdentity"] == identity]
        for page in identity_pages:
            page_image = build_page_image(source_image, page)
            page_path = PAGE_ROOT / Path(page["path"]).name
            write_new_png(page_path, page_image)
            page_infos.append(validate_page(page_path, page, source_image))
        source_image.close()

    page_infos.sort(key=lambda item: [page["id"] for page in plan["pages"]].index(item["id"]))
    manifest = final_manifest(plan, page_infos)
    manifest_schema = validate_manifest_schema(manifest, plan)
    write_new_json(manifest_path, manifest)
    freshness = source_dat_freshness(plan)
    write_new_json(freshness_path, freshness)
    validation = {
        "utc": utc_now(),
        "stage": "png",
        "manifest": rel_root(manifest_path),
        "manifestSha256": sha256_file(manifest_path),
        "pageCount": len(page_infos),
        "logicalMappingCount": sum(
            source["columns"] * source["rows"] * len(source["bindings"])
            for source in plan["sourceInputs"]
        ),
        "uniqueFrameCount": plan["expected"]["uniqueFrameCount"],
        "pageFrameMatches": sum(item["frameMatches"] for item in page_infos),
        "pageEdgeMatches": sum(item["edgeMatches"] for item in page_infos),
        "allPagesAtMost2048": all(item["width"] <= PAGE_SIZE and item["height"] <= PAGE_SIZE for item in page_infos),
        "allPageDimensionsAligned4": all(
            item["width"] % ALLOCATION_ALIGNMENT == 0 and item["height"] % ALLOCATION_ALIGNMENT == 0
            for item in page_infos
        ),
        "allFramesByteEqual": True,
        "allEdgesByteEqual": True,
        "allAlignmentPaddingTransparentZero": all(
            item["alignmentPixels"] == item["alignmentZeroPixels"] for item in page_infos
        ),
        "allUnusedPagePixelsTransparentZero": all(
            item["unusedPagePixels"] == item["unusedPageZeroPixels"] for item in page_infos
        ),
        "sourceDatFreshness": freshness,
        "manifestSchema": manifest_schema,
        "pages": page_infos,
    }
    write_new_json(validation_path, validation)
    write_new_text(helper_path, helper_description())
    print(
        json.dumps(
            {
                "stage": "png",
                "manifest": rel_root(manifest_path),
                "pages": len(page_infos),
                "logicalMappings": validation["logicalMappingCount"],
                "uniqueFrames": validation["uniqueFrameCount"],
                "allFramesByteEqual": validation["allFramesByteEqual"],
                "sourcesAndDatsUnchanged": freshness["allUnchanged"],
            },
            ensure_ascii=False,
        ),
        flush=True,
    )


def command_receipt(command: list[str], log_stem: str) -> dict[str, Any]:
    started = utc_now()
    before = time.monotonic()
    result = subprocess.run(command, cwd=ROOT, capture_output=True)
    elapsed = time.monotonic() - before
    stdout = result.stdout or b""
    stderr = result.stderr or b""
    write_new_bytes(ASTC_ROOT / "logs" / f"{log_stem}.stdout-stderr.txt", stdout + stderr)
    receipt = {
        "command": command,
        "startedUtc": started,
        "endedUtc": utc_now(),
        "wallSeconds": elapsed,
        "exitCode": result.returncode,
        "stdoutSha256": sha256_bytes(stdout),
        "stderrSha256": sha256_bytes(stderr),
        "stdoutBytes": len(stdout),
        "stderrBytes": len(stderr),
    }
    write_new_json(ASTC_ROOT / "logs" / f"{log_stem}.json", receipt)
    if result.returncode != 0:
        raise RuntimeError(f"command failed with exit {result.returncode}: {' '.join(command)}")
    return receipt


def astc_info(path: Path, width: int, height: int) -> dict[str, Any]:
    data = path.read_bytes()
    if data[:4] != bytes.fromhex("13aba15c"):
        raise RuntimeError(f"ASTC magic mismatch: {path}")
    block = list(data[4:7])
    if block != [4, 4, 1]:
        raise RuntimeError(f"ASTC block mismatch: {path}: {block}")
    dimensions = [int.from_bytes(data[index:index + 3], "little") for index in (7, 10, 13)]
    if dimensions != [width, height, 1]:
        raise RuntimeError(f"ASTC dimensions mismatch: {path}: {dimensions}")
    payload = math.ceil(width / 4) * math.ceil(height / 4) * 16
    if len(data) != 16 + payload:
        raise RuntimeError(f"ASTC payload length mismatch: {path}")
    return {
        "magicHex": data[:4].hex(),
        "blockDimensions": block,
        "dimensions": dimensions,
        "headerBytes": 16,
        "payloadBytes": payload,
        "fileBytes": len(data),
        "sha256": sha256_bytes(data),
    }


def decoded_pixel_metrics(source_path: Path, decoded_path: Path) -> dict[str, Any]:
    with Image.open(source_path) as source_image:
        source = np.asarray(source_image.convert("RGBA"), dtype=np.int16)
    with Image.open(decoded_path) as decoded_image:
        decoded = np.asarray(decoded_image.convert("RGBA"), dtype=np.int16)
    if source.shape != decoded.shape:
        raise RuntimeError(f"ASTC decoded pixel dimensions differ: {decoded_path}")
    difference = np.abs(source - decoded)
    visible = source[:, :, 3] > 0
    transparent = ~visible
    visible_difference = difference[visible] if np.any(visible) else np.zeros((0, 4), dtype=np.int16)
    return {
        "width": int(source.shape[1]),
        "height": int(source.shape[0]),
        "pixels": int(source.shape[0] * source.shape[1]),
        "maeRgba8": float(difference.mean()),
        "maxAbsRgba8": int(difference.max()) if difference.size else 0,
        "alphaMae8": float(difference[:, :, 3].mean()),
        "visiblePixels": int(visible.sum()),
        "visibleMaeRgba8": float(visible_difference.mean()) if visible_difference.size else 0.0,
        "visibleMaxAbsRgba8": int(visible_difference.max()) if visible_difference.size else 0,
        "transparentPixels": int(transparent.sum()),
        "transparentRgbNonzeroOriginal": int(np.any(source[transparent, :3] != 0, axis=1).sum()) if np.any(transparent) else 0,
        "transparentRgbNonzeroDecoded": int(np.any(decoded[transparent, :3] != 0, axis=1).sum()) if np.any(transparent) else 0,
    }


def run_astc_stage() -> None:
    plan = load_plan()
    verify_current_inputs(plan)
    manifest_path = GENERATED_ROOT / "manifest.json"
    if not manifest_path.exists():
        raise RuntimeError("PNG manifest is required before ASTC stage")
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    validate_manifest_schema(manifest, plan)
    identity_path = ASTC_ROOT / "tool-identity.json"
    results_path = ASTC_ROOT / "astc-results.json"
    if identity_path.exists() or results_path.exists():
        raise RuntimeError("ASTC result files already exist; refusing to overwrite")
    if not ASTC_EXE.exists():
        raise RuntimeError(f"ASTC executable missing: {ASTC_EXE}")
    prior_identity = json.loads(PRIOR_ASTC_TOOL_IDENTITY.read_text(encoding="utf-8"))
    current_sha = sha256_file(ASTC_EXE)
    selected_expected = next(
        item["sha256"]
        for item in prior_identity["files"]
        if item["path"].lower().endswith("astcenc-sse2.exe")
    )
    version_result = subprocess.run([str(ASTC_EXE), "-version"], cwd=ROOT, capture_output=True)
    version_stdout = (version_result.stdout or b"").decode("utf-8", errors="replace")
    version_stderr = (version_result.stderr or b"").decode("utf-8", errors="replace")
    if version_result.returncode != 0:
        raise RuntimeError("ASTC tool identity command failed")
    tool_identity = {
        "utc": utc_now(),
        "executable": rel_root(ASTC_EXE),
        "bytes": ASTC_EXE.stat().st_size,
        "sha256": current_sha,
        "priorIdentityFile": rel_root(PRIOR_ASTC_TOOL_IDENTITY),
        "priorSelectedSha256": selected_expected,
        "matchesPriorSelectedSha256": current_sha == selected_expected,
        "versionCommand": [rel_root(ASTC_EXE), "-version"],
        "versionExitCode": version_result.returncode,
        "versionStdout": version_stdout,
        "versionStderr": version_stderr,
        "profile": "sRGB LDR thorough -a 1 -j 2; -cs encode / -ds decode",
    }
    if not tool_identity["matchesPriorSelectedSha256"]:
        raise RuntimeError("ASTC executable SHA differs from existing tool identity")
    write_new_json(identity_path, tool_identity)

    encoded_pages: list[dict[str, Any]] = []
    for page in plan["pages"]:
        page_id_value = page["id"]
        source_png = PAGE_ROOT / Path(page["path"]).name
        compressed = ASTC_ROOT / "compressed" / f"{page_id_value}.astc"
        decoded = ASTC_ROOT / "decoded" / f"{page_id_value}.png"
        if compressed.exists() or decoded.exists():
            raise RuntimeError(f"refusing to overwrite ASTC output for {page_id_value}")
        compressed.parent.mkdir(parents=True, exist_ok=True)
        decoded.parent.mkdir(parents=True, exist_ok=True)
        encode_command = [
            str(ASTC_EXE), "-cs", str(source_png), str(compressed), "4x4",
            "-thorough", "-a", "1", "-j", "2",
        ]
        encode = command_receipt(encode_command, f"{page_id_value}-encode")
        header = astc_info(compressed, page["width"], page["height"])
        decode_command = [str(ASTC_EXE), "-ds", str(compressed), str(decoded), "-j", "2"]
        decode = command_receipt(decode_command, f"{page_id_value}-decode")
        with Image.open(decoded) as decoded_image:
            if decoded_image.size != (page["width"], page["height"]):
                raise RuntimeError(f"decoded ASTC size mismatch: {decoded}")
        metrics = decoded_pixel_metrics(source_png, decoded)
        encoded_pages.append(
            {
                "id": page_id_value,
                "sourcePage": page["path"],
                "sourcePageSha256": sha256_file(source_png),
                "compressed": {
                    "path": rel_root(compressed),
                    "sha256": header["sha256"],
                    "bytes": compressed.stat().st_size,
                },
                "decoded": {
                    "path": rel_root(decoded),
                    "sha256": sha256_file(decoded),
                    "bytes": decoded.stat().st_size,
                },
                "encode": encode,
                "decode": decode,
                "header": header,
                "decodedPixelMetrics": metrics,
            }
        )
        print(
            json.dumps(
                {
                    "stage": "astc",
                    "page": page_id_value,
                    "encodeExit": encode["exitCode"],
                    "decodeExit": decode["exitCode"],
                    "payloadBytes": header["payloadBytes"],
                    "decodedMaeRgba8": metrics["maeRgba8"],
                },
                ensure_ascii=False,
            ),
            flush=True,
        )
    result = {
        "utc": utc_now(),
        "tool": tool_identity,
        "profile": "sRGB LDR thorough -a 1 -j 2; -cs encode / -ds decode",
        "block": [4, 4, 1],
        "pageCount": len(encoded_pages),
        "allEncodeExitZero": all(x["encode"]["exitCode"] == 0 for x in encoded_pages),
        "allDecodeExitZero": all(x["decode"]["exitCode"] == 0 for x in encoded_pages),
        "pages": encoded_pages,
    }
    write_new_json(results_path, result)


def load_font(size: int) -> ImageFont.ImageFont:
    for candidate in (Path("C:/Windows/Fonts/segoeui.ttf"), Path("C:/Windows/Fonts/arial.ttf")):
        if candidate.exists():
            return ImageFont.truetype(str(candidate), size)
    return ImageFont.load_default()


def checkerboard(image: Image.Image, tile: int = 16) -> Image.Image:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    board = Image.new("RGB", (width, height), (44, 52, 64))
    pixels = board.load()
    for y in range(height):
        for x in range(width):
            if ((x // tile) + (y // tile)) % 2:
                pixels[x, y] = (72, 82, 96)
    board.paste(rgba, (0, 0), rgba)
    return board


def fit_image(image: Image.Image, max_width: int, max_height: int) -> Image.Image:
    if image.width <= max_width and image.height <= max_height:
        return image.copy()
    scale = min(max_width / image.width, max_height / image.height)
    size = (max(1, round(image.width * scale)), max(1, round(image.height * scale)))
    return image.resize(size, Image.Resampling.NEAREST)


def zoom_box(image: Image.Image) -> tuple[int, int, int, int]:
    rgba = image.convert("RGBA")
    alpha = np.asarray(rgba)[:, :, 3]
    nonzero = np.argwhere(alpha > 0)
    if nonzero.size:
        center_y, center_x = nonzero.mean(axis=0)
    else:
        center_x, center_y = image.width / 2, image.height / 2
    side = min(256, image.width, image.height)
    left = int(round(center_x - side / 2))
    top = int(round(center_y - side / 2))
    left = max(0, min(left, image.width - side))
    top = max(0, min(top, image.height - side))
    return left, top, left + side, top + side


def make_comparison_board(label: str, original: Image.Image, reconstructed: Image.Image,
                          astc: Image.Image, exact_match: bool, frame_metrics: dict[str, Any],
                          zoom: tuple[int, int, int, int]) -> Image.Image:
    panel_width = 520
    panel_height = 400
    margin = 24
    gap = 16
    label_height = 40
    zoom_height = 420
    board_width = margin * 2 + panel_width * 3 + gap * 2
    board_height = 96 + label_height + panel_height + 70 + label_height + zoom_height + margin
    board = Image.new("RGB", (board_width, board_height), (17, 24, 39))
    draw = ImageDraw.Draw(board)
    title_font = load_font(24)
    label_font = load_font(18)
    small_font = load_font(15)
    draw.text((margin, 18), f"{label} | exact reconstructed frame: {exact_match}", font=title_font, fill=(241, 245, 249))
    draw.text(
        (margin, 54),
        f"ASTC frame RGBA8 MAE {frame_metrics['maeRgba8']:.3f} / max {frame_metrics['maxAbsRgba8']}",
        font=small_font,
        fill=(148, 163, 184),
    )
    variants = [("Original DAT frame", original), ("Reconstructed page frame", reconstructed), ("ASTC decoded frame", astc)]
    top_y = 96
    for index, (title, image) in enumerate(variants):
        x = margin + index * (panel_width + gap)
        draw.text((x, top_y), title, font=label_font, fill=(147, 197, 253))
        display = checkerboard(fit_image(image, panel_width, panel_height))
        board.paste(display, (x, top_y + label_height))
    zoom_y = top_y + label_height + panel_height + 28
    draw.text((margin, zoom_y), f"Local zoom box {zoom} | nearest 4x where space permits", font=label_font, fill=(147, 197, 253))
    for index, (title, image) in enumerate(variants):
        x = margin + index * (panel_width + gap)
        crop = image.crop(zoom)
        crop = crop.resize((crop.width * 4, crop.height * 4), Image.Resampling.NEAREST)
        display = checkerboard(fit_image(crop, panel_width, zoom_height))
        board.paste(display, (x, zoom_y + label_height))
    return board


def frame_metrics(original: Image.Image, decoded: Image.Image) -> dict[str, Any]:
    source = np.asarray(original.convert("RGBA"), dtype=np.int16)
    target = np.asarray(decoded.convert("RGBA"), dtype=np.int16)
    diff = np.abs(source - target)
    return {
        "width": original.width,
        "height": original.height,
        "pixels": original.width * original.height,
        "maeRgba8": float(diff.mean()),
        "maxAbsRgba8": int(diff.max()) if diff.size else 0,
        "alphaMae8": float(diff[:, :, 3].mean()),
    }


def run_verify_stage() -> None:
    """Re-read every manifest binding so shared pages still prove 88 mappings."""
    plan = load_plan()
    verify_current_inputs(plan)
    manifest_path = GENERATED_ROOT / "manifest.json"
    result_path = RESULTS_ROOT / "full-logical-verification-final.json"
    description_path = RESULTS_ROOT / "helper-description-final-v2.md"
    if not manifest_path.exists():
        raise RuntimeError("PNG manifest is required before verify stage")
    if result_path.exists() or description_path.exists():
        raise RuntimeError("full logical verification outputs already exist")
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    validate_manifest_schema(manifest, plan)
    manifest_sources = {source["path"]: source for source in manifest["sources"]}
    manifest_pages = {page["id"]: page for page in manifest["pages"]}
    rows: list[dict[str, Any]] = []
    source_unique_keys: set[tuple[str, int]] = set()
    for source in plan["sourceInputs"]:
        manifest_source = manifest_sources[source["path"]]
        source_path = ROOT / source["sourceFile"]
        with Image.open(source_path) as image:
            source_image = image.convert("RGBA")
        for binding in manifest_source["bindings"]:
            for chunk in manifest_source["chunks"]:
                page_meta = manifest_pages[chunk["page"]]
                page_path = GENERATED_ROOT / "pages" / Path(page_meta["path"]).name
                with Image.open(page_path) as page_image:
                    page_rgba = page_image.convert("RGBA")
                page_array = np.asarray(page_rgba)
                for offset, rect in enumerate(chunk["rects"]):
                    local_pic = chunk["firstLocalPic"] + offset
                    source_x = (local_pic % source["columns"]) * (source["frameWidth"] + 1)
                    source_y = (local_pic // source["columns"]) * (source["frameHeight"] + 1)
                    source_box = (
                        source_x,
                        source_y,
                        source_x + source["frameWidth"],
                        source_y + source["frameHeight"],
                    )
                    page_top_y = page_meta["height"] - rect["y"] - rect["height"]
                    page_box = (
                        rect["x"],
                        page_top_y,
                        rect["x"] + rect["width"],
                        page_top_y + rect["height"],
                    )
                    if (
                        page_box[0] < 0
                        or page_box[1] < 0
                        or page_box[2] > page_meta["width"]
                        or page_box[3] > page_meta["height"]
                    ):
                        raise RuntimeError(
                            f"logical rect exceeds page bounds: {source['path']} localPic={local_pic}"
                        )
                    source_array = np.asarray(source_image.crop(source_box))
                    page_array_crop = page_array[page_box[1]:page_box[3], page_box[0]:page_box[2]]
                    exact = bool(np.array_equal(source_array, page_array_crop))
                    if not exact:
                        raise RuntimeError(
                            f"logical frame mismatch: {source['path']} "
                            f"oid={binding['oid']} localPic={local_pic}"
                        )
                    source_unique_keys.add((source["sourceIdentity"], local_pic))
                    rows.append(
                        {
                            "oid": binding["oid"],
                            "datPath": binding["datPath"],
                            "source": source["path"],
                            "localPic": local_pic,
                            "page": chunk["page"],
                            "rect": rect,
                            "exact": exact,
                        }
                    )
                page_rgba.close()
        source_image.close()

    freshness = source_dat_freshness(plan)
    result = {
        "utc": utc_now(),
        "stage": "verify",
        "manifest": rel_root(manifest_path),
        "logicalMappingCount": len(rows),
        "uniqueFrameCount": len(source_unique_keys),
        "exactLogicalMappings": sum(1 for row in rows if row["exact"]),
        "all88Exact": len(rows) == 88 and all(row["exact"] for row in rows),
        "sourceDatFreshness": freshness,
        "rows": rows,
    }
    if not result["all88Exact"]:
        raise RuntimeError("full logical verification did not cover 88 exact mappings")
    write_new_json(result_path, result)
    write_new_text(
        description_path,
        helper_description()
        + "\nThe verify stage re-reads every binding-expanded logical frame and writes "
        "full-logical-verification-final.json; for this pilot it must contain 88 exact rows "
        "and 63 unique source/local frame keys.\n",
    )
    print(
        json.dumps(
            {
                "stage": "verify",
                "logicalMappings": result["logicalMappingCount"],
                "uniqueFrames": result["uniqueFrameCount"],
                "exactLogicalMappings": result["exactLogicalMappings"],
                "all88Exact": result["all88Exact"],
            },
            ensure_ascii=False,
        ),
        flush=True,
    )


def run_compare_stage() -> None:
    plan = load_plan()
    manifest_path = GENERATED_ROOT / "manifest.json"
    astc_results_path = ASTC_ROOT / "astc-results.json"
    if not manifest_path.exists() or not astc_results_path.exists():
        raise RuntimeError("PNG and ASTC stages are required before compare stage")
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    validate_manifest_schema(manifest, plan)
    astc_results = json.loads(astc_results_path.read_text(encoding="utf-8"))
    astc_by_id = {item["id"]: item for item in astc_results["pages"]}
    comparison_result_path = COMPARISON_ROOT / "comparison-results.json"
    montage_path = MONTAGE_ROOT / "montage.png"
    pages_montage_path = MONTAGE_ROOT / "pages-montage.png"
    for path in (comparison_result_path, montage_path, pages_montage_path):
        if path.exists():
            raise RuntimeError(f"refusing to overwrite comparison output: {path}")

    desired = [
        ("win2", "c/dan/a/win2.png", 0),
        ("Kirin", "c/sasu/a/Kirin.png", 0),
        ("sui3", "c/sui/sui3.png", 0),
    ]
    boards: list[tuple[str, Path]] = []
    result_rows: list[dict[str, Any]] = []
    source_by_path = {source["path"]: source for source in plan["sourceInputs"]}
    manifest_source_by_path = {source["path"]: source for source in manifest["sources"]}
    for slug, source_rel, local_pic in desired:
        source = source_by_path[source_rel]
        manifest_source = manifest_source_by_path[source_rel]
        source_path = ROOT / source["sourceFile"]
        target_chunk = next(
            chunk for chunk in manifest_source["chunks"]
            if chunk["firstLocalPic"] <= local_pic < chunk["firstLocalPic"] + chunk["count"]
        )
        rect = target_chunk["rects"][local_pic - target_chunk["firstLocalPic"]]
        page_id_value = target_chunk["page"]
        page = next(item for item in manifest["pages"] if item["id"] == page_id_value)
        page_path = GENERATED_ROOT / "pages" / Path(page["path"]).name
        astc_page_path = ROOT / astc_by_id[page_id_value]["decoded"]["path"]
        source_x = (local_pic % source["columns"]) * (source["frameWidth"] + 1)
        source_y = (local_pic // source["columns"]) * (source["frameHeight"] + 1)
        source_box = (source_x, source_y, source_x + source["frameWidth"], source_y + source["frameHeight"])
        page_top_y = page["height"] - (rect["y"] + rect["height"])
        page_box = (rect["x"], page_top_y, rect["x"] + rect["width"], page_top_y + rect["height"])
        with Image.open(source_path) as image:
            original = image.convert("RGBA").crop(source_box)
        with Image.open(page_path) as image:
            reconstructed = image.convert("RGBA").crop(page_box)
        with Image.open(astc_page_path) as image:
            astc_frame = image.convert("RGBA").crop(page_box)
        exact_match = np.array_equal(np.asarray(original), np.asarray(reconstructed))
        if not exact_match:
            raise RuntimeError(f"comparison reconstruction mismatch: {source_rel} localPic {local_pic}")
        zoom = zoom_box(original)
        slug_dir = COMPARISON_ROOT / slug
        for path in (
            slug_dir / "original-frame.png",
            slug_dir / "reconstructed-frame.png",
            slug_dir / "astc-decoded-frame.png",
            slug_dir / "comparison.png",
        ):
            if path.exists():
                raise RuntimeError(f"refusing to overwrite comparison file: {path}")
        write_new_png(slug_dir / "original-frame.png", original)
        write_new_png(slug_dir / "reconstructed-frame.png", reconstructed)
        write_new_png(slug_dir / "astc-decoded-frame.png", astc_frame)
        metrics = frame_metrics(original, astc_frame)
        board = make_comparison_board(slug, original, reconstructed, astc_frame, exact_match, metrics, zoom)
        board_path = slug_dir / "comparison.png"
        write_new_png(board_path, board)
        boards.append((slug, board_path))
        result_rows.append(
            {
                "slug": slug,
                "source": source_rel,
                "localPic": local_pic,
                "sourceFrameBoxTopLeft": list(source_box),
                "page": page_id_value,
                "manifestRectBottomLeft": rect,
                "pageFrameBoxTopLeft": list(page_box),
                "exactReconstruction": exact_match,
                "zoomBox": list(zoom),
                "metrics": metrics,
                "files": {
                    "original": rel_root(slug_dir / "original-frame.png"),
                    "reconstructed": rel_root(slug_dir / "reconstructed-frame.png"),
                    "astcDecoded": rel_root(slug_dir / "astc-decoded-frame.png"),
                    "comparison": rel_root(board_path),
                },
            }
        )

    thumb_width = 520
    thumb_height = 340
    montage = Image.new("RGB", (thumb_width * len(boards) + 24 * (len(boards) + 1), thumb_height + 64), (15, 23, 42))
    draw = ImageDraw.Draw(montage)
    font = load_font(20)
    for index, (slug, path) in enumerate(boards):
        x = 24 + index * (thumb_width + 24)
        with Image.open(path) as image:
            thumb = fit_image(image.convert("RGB"), thumb_width, thumb_height)
        montage.paste(thumb, (x, 48))
        draw.text((x, 16), slug, font=font, fill=(241, 245, 249))
    write_new_png(montage_path, montage)

    page_thumbnails: list[Image.Image] = []
    for page in plan["pages"]:
        with Image.open(PAGE_ROOT / Path(page["path"]).name) as image:
            page_thumbnails.append(fit_image(checkerboard(image.convert("RGBA")), 320, 240))
    columns = 4
    rows = math.ceil(len(page_thumbnails) / columns)
    cell_width = 336
    cell_height = 276
    page_montage = Image.new("RGB", (columns * cell_width, rows * cell_height), (15, 23, 42))
    page_draw = ImageDraw.Draw(page_montage)
    for index, (page, thumb) in enumerate(zip(plan["pages"], page_thumbnails)):
        x = (index % columns) * cell_width + 8
        y = (index // columns) * cell_height + 8
        page_montage.paste(thumb, (x, y + 24))
        page_draw.text((x, y), f"{index:02d} {page['id'][:24]}", font=load_font(13), fill=(226, 232, 240))
    write_new_png(pages_montage_path, page_montage)
    comparison_result = {
        "utc": utc_now(),
        "samples": result_rows,
        "allExactReconstruction": all(row["exactReconstruction"] for row in result_rows),
        "montage": rel_root(montage_path),
        "pagesMontage": rel_root(pages_montage_path),
    }
    write_new_json(comparison_result_path, comparison_result)
    print(
        json.dumps(
            {
                "stage": "compare",
                "samples": len(result_rows),
                "allExactReconstruction": comparison_result["allExactReconstruction"],
                "montage": rel_root(montage_path),
            },
            ensure_ascii=False,
        ),
        flush=True,
    )


def run_plan_stage(oracle_path: Path | None = None) -> None:
    plan_path = GENERATED_ROOT / "planned-output-manifest.json"
    if plan_path.exists():
        raise RuntimeError(f"refusing to overwrite existing plan: {plan_path}")
    plan = build_plan(oracle_path)
    write_planned_manifest(plan)
    print(
        json.dumps(
            {
                "stage": "plan",
                "plannedManifest": rel_root(plan_path),
                "pages": plan["expected"]["pageCount"],
                "logicalMappings": plan["expected"]["logicalMappingCount"],
                "uniqueFrames": plan["expected"]["uniqueFrameCount"],
                "oracleStatus": plan["oracle"]["status"],
            },
            ensure_ascii=False,
        ),
        flush=True,
    )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--stage", choices=("plan", "png", "astc", "verify", "compare", "all"), required=True)
    parser.add_argument(
        "--oracle-path",
        type=Path,
        default=None,
        help="Optional feasibility JSON used only for an independent geometry cross-check.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    if args.stage == "plan":
        run_plan_stage(args.oracle_path)
    elif args.stage == "png":
        run_png_stage()
    elif args.stage == "astc":
        run_astc_stage()
    elif args.stage == "verify":
        run_verify_stage()
    elif args.stage == "compare":
        run_compare_stage()
    else:
        run_plan_stage(args.oracle_path)
        run_png_stage()
        run_astc_stage()
        run_verify_stage()
        run_compare_stage()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
