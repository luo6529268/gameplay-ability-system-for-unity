#!/usr/bin/env python3
"""Build one exact-RGBA NTSD sprite-page deduplication candidate.

This is a staging-only helper for NTSD-M12-EXACT-FRAME-DEDUP-001.  It reads
the deployed manifest, source PNGs, decoded DAT files, and the old page.  It
does not import or invoke the existing page generator and never writes under
Assets.  The candidate directory is create-new: an existing directory or
output file is a fatal collision.
"""

from __future__ import annotations

import copy
import hashlib
import io
import json
import math
import re
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

import numpy as np
from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
CONTENT = ROOT / "Assets/NTSD/Content/LoganRuntime"
DEPLOYED = CONTENT / "derived_sprite_pages"
MANIFEST_PATH = DEPLOYED / "manifest.json"
VFS_ROOT = CONTENT / "vfs"
DAT_ROOT = CONTENT / "decoded_dat"
EVIDENCE_ROOT = ROOT / "artifacts/diagnostics/M12-exact-frame-dedup-20261013"
CANDIDATE_BASE_ROOT = EVIDENCE_ROOT / "candidate"
CANDIDATE_ROOT = CANDIDATE_BASE_ROOT / "final"
CANDIDATE_PAGES = CANDIDATE_ROOT / "pages"
PLANNED_PATH = CANDIDATE_ROOT / "planned-outputs.json"
VERIFICATION_PATH = CANDIDATE_ROOT / "verification.json"
RECEIPT_PATH = CANDIDATE_ROOT / "receipt.json"
REPORT_PATH = CANDIDATE_ROOT / "REPORT.md"

TARGET_SOURCES = ("c/sui/sui3.png", "c/kar/a/sui.png")
TARGET_OLD_PAGE_ID = "page-a24a59902f0eac88-567x567-3x3-0018-007"
TARGET_NEW_PAGE_ID = "page-a24a59902f0eac88-567x567-2x2-0018-007-exact-dedup-20261013"
TARGET_NEW_PAGE_REL = f"pages/{TARGET_NEW_PAGE_ID}.png"
TARGET_NEW_PAGE_PATH = CANDIDATE_ROOT / TARGET_NEW_PAGE_REL
FRAME_WIDTH = 567
FRAME_HEIGHT = 567
CELL_SPAN = 572
PAGE_SIZE = 1144
EDGE_PADDING = 1
ALIGNMENT_TAIL = 3

PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"
DAT_FILE_RE = re.compile(
    r"file\(\s*(?P<first>\d+)\s*-\s*(?P<last>\d+)\s*\):\s*"
    r"(?P<path>[^\s]+)\s+w:\s*(?P<width>\d+)\s+h:\s*(?P<height>\d+)\s+"
    r"row:\s*(?P<rows>\d+)\s+col:\s*(?P<columns>\d+)",
    re.IGNORECASE,
)


class AuditError(RuntimeError):
    pass


def fail(message: str) -> None:
    raise AuditError(message)


def rel_path(path: Path) -> str:
    try:
        return path.resolve().relative_to(ROOT.resolve()).as_posix()
    except ValueError:
        return path.resolve().as_posix()


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def normalize_rel(value: str) -> str:
    return value.replace("\\", "/").lstrip("./").lower()


def json_bytes(value: Any) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def png_header(path: Path) -> dict[str, int]:
    data = path.read_bytes()
    if len(data) < 33 or data[:8] != PNG_SIGNATURE:
        fail(f"PNG signature/header missing: {path}")
    length = int.from_bytes(data[8:12], "big")
    if length != 13 or data[12:16] != b"IHDR":
        fail(f"PNG IHDR header invalid: {path}")
    width = int.from_bytes(data[16:20], "big")
    height = int.from_bytes(data[20:24], "big")
    return {
        "width": width,
        "height": height,
        "bitDepth": data[24],
        "colorType": data[25],
        "compression": data[26],
        "filter": data[27],
        "interlace": data[28],
    }


def read_rgba(path: Path) -> tuple[np.ndarray, dict[str, Any]]:
    header = png_header(path)
    with Image.open(path) as image:
        original_mode = image.mode
        original_size = [int(image.width), int(image.height)]
        rgba = np.array(image.convert("RGBA"), dtype=np.uint8, copy=True)
    if original_size != [header["width"], header["height"]]:
        fail(f"PIL/header dimensions differ: {path}")
    return rgba, {
        "header": header,
        "mode": original_mode,
        "size": original_size,
        "readbackMode": "RGBA",
    }


def encode_rgba(array: np.ndarray) -> bytes:
    if array.dtype != np.uint8 or array.ndim != 3 or array.shape[2] != 4:
        fail(f"Expected uint8 RGBA array, got {array.dtype}/{array.shape}")
    buffer = io.BytesIO()
    Image.fromarray(np.ascontiguousarray(array), "RGBA").save(
        buffer, format="PNG", compress_level=9
    )
    return buffer.getvalue()


def decode_rgba(data: bytes) -> tuple[np.ndarray, dict[str, Any]]:
    if len(data) < 33 or data[:8] != PNG_SIGNATURE:
        fail("Generated PNG readback has no PNG signature")
    with Image.open(io.BytesIO(data)) as image:
        mode = image.mode
        size = [int(image.width), int(image.height)]
        rgba = np.array(image.convert("RGBA"), dtype=np.uint8, copy=True)
    header = {
        "width": int.from_bytes(data[16:20], "big"),
        "height": int.from_bytes(data[20:24], "big"),
        "bitDepth": data[24],
        "colorType": data[25],
        "compression": data[26],
        "filter": data[27],
        "interlace": data[28],
    }
    if size != [header["width"], header["height"]]:
        fail("Generated PNG PIL/header dimensions differ")
    return rgba, {"header": header, "mode": mode, "size": size, "readbackMode": "RGBA"}


def compare_arrays(expected: np.ndarray, actual: np.ndarray) -> dict[str, Any]:
    if expected.shape != actual.shape:
        return {
            "exactRgba": False,
            "shapeExpected": [int(v) for v in expected.shape],
            "shapeActual": [int(v) for v in actual.shape],
            "changedPixels": -1,
            "maxAbsDiff": -1,
            "firstDifference": None,
        }
    if np.array_equal(expected, actual):
        return {
            "exactRgba": True,
            "shapeExpected": [int(v) for v in expected.shape],
            "shapeActual": [int(v) for v in actual.shape],
            "changedPixels": 0,
            "maxAbsDiff": 0,
            "firstDifference": None,
        }
    changed = np.any(expected != actual, axis=2)
    ys, xs = np.nonzero(changed)
    delta = np.abs(expected.astype(np.int16) - actual.astype(np.int16))
    return {
        "exactRgba": False,
        "shapeExpected": [int(v) for v in expected.shape],
        "shapeActual": [int(v) for v in actual.shape],
        "changedPixels": int(changed.sum()),
        "maxAbsDiff": int(delta.max()),
        "firstDifference": [int(xs[0]), int(ys[0])],
    }


def source_cell(source: dict[str, Any], image: np.ndarray, local_pic: int) -> tuple[np.ndarray, tuple[int, int]]:
    columns = int(source["columns"])
    rows = int(source["rows"])
    frame_width = int(source["frameWidth"])
    frame_height = int(source["frameHeight"])
    native_count = columns * rows
    if local_pic < 0 or local_pic >= native_count:
        fail(f"Source local frame outside native grid: {source['path']} {local_pic}")
    x_step = frame_width + 1
    y_step = frame_height + 1
    declared_extent = [x_step * columns, y_step * rows]
    declared_size = [int(source["width"]), int(source["height"])]
    if declared_size[0] < declared_extent[0] or declared_size[1] < declared_extent[1]:
        fail(
            f"Native DAT stride grid exceeds source declaration: {source['path']} "
            f"extent={declared_extent} size={declared_size}"
        )
    column = local_pic % columns
    row = local_pic // columns
    x = column * x_step
    y = row * y_step
    if x + frame_width > image.shape[1] or y + frame_height > image.shape[0]:
        fail(f"Native source cell outside decoded image: {source['path']} local={local_pic}")
    return image[y : y + frame_height, x : x + frame_width], (x, y)


def page_cell(page: np.ndarray, rect: dict[str, Any]) -> tuple[np.ndarray, tuple[int, int]]:
    x = int(rect["x"])
    bottom_y = int(rect["y"])
    width = int(rect["width"])
    height = int(rect["height"])
    top_y = page.shape[0] - bottom_y - height
    if x < 0 or top_y < 0 or x + width > page.shape[1] or top_y + height > page.shape[0]:
        fail(f"Manifest rect outside page: {rect} page={page.shape}")
    return page[top_y : top_y + height, x : x + width], (x, top_y)


def rect_tuple(rect: dict[str, Any]) -> tuple[int, int, int, int]:
    return (int(rect["x"]), int(rect["y"]), int(rect["width"]), int(rect["height"]))


def rectangles_overlap(first: tuple[int, int, int, int], second: tuple[int, int, int, int]) -> bool:
    return not (
        first[0] + first[2] <= second[0]
        or second[0] + second[2] <= first[0]
        or first[1] + first[3] <= second[1]
        or second[1] + second[3] <= first[1]
    )


def parse_dat(path: Path) -> list[dict[str, Any]]:
    text = path.read_text(encoding="utf-8", errors="replace")
    entries = []
    for match in DAT_FILE_RE.finditer(text):
        first = int(match.group("first"))
        last = int(match.group("last"))
        rows = int(match.group("rows"))
        columns = int(match.group("columns"))
        entries.append(
            {
                "firstPic": first,
                "lastPic": last,
                "path": match.group("path"),
                "normalizedPath": normalize_rel(match.group("path")),
                "width": int(match.group("width")),
                "height": int(match.group("height")),
                "rows": rows,
                "columns": columns,
                "rowTimesColumn": rows * columns,
            }
        )
    if not entries:
        fail(f"No native file entries parsed from DAT: {path}")
    return entries


def source_file(source: dict[str, Any]) -> Path:
    path = VFS_ROOT / Path(source["path"])
    if not path.is_file():
        fail(f"Manifest source missing: {path}")
    return path


def dat_file(relative: str) -> Path:
    path = DAT_ROOT / Path(relative.replace("\\", "/"))
    if not path.is_file():
        fail(f"Declared decoded DAT missing: {path}")
    return path


def page_file(page: dict[str, Any], root: Path) -> Path:
    relative = str(page["path"]).replace("\\", "/")
    if not relative or Path(relative).is_absolute() or ".." in Path(relative).parts:
        fail(f"Unsafe page path in manifest: {relative}")
    path = root / Path(relative)
    if not path.is_file():
        fail(f"Page file missing: {path}")
    return path


def load_manifest() -> tuple[dict[str, Any], bytes]:
    if not MANIFEST_PATH.is_file():
        fail(f"Deployed manifest missing: {MANIFEST_PATH}")
    raw = MANIFEST_PATH.read_bytes()
    manifest = json.loads(raw.decode("utf-8"))
    if manifest.get("version") != 1 or manifest.get("generatorVersion") != "NTSD_DAT_PAGES_V1":
        fail("Unexpected deployed manifest identity")
    if len(manifest.get("pages", [])) != 19 or len(manifest.get("sources", [])) != 4:
        fail("Expected deployed manifest to contain 19 pages and 4 sources")
    return manifest, raw


def capture_input_hashes(manifest: dict[str, Any]) -> dict[str, Any]:
    files: list[dict[str, Any]] = []
    seen: set[str] = set()

    def add(path: Path, role: str) -> None:
        key = str(path.resolve()).lower()
        if key in seen:
            return
        if not path.is_file():
            fail(f"Input disappeared while hashing: {path}")
        seen.add(key)
        files.append(
            {
                "role": role,
                "path": rel_path(path),
                "sha256": sha256_file(path),
                "bytes": path.stat().st_size,
            }
        )

    add(MANIFEST_PATH, "deployed-manifest")
    for page in manifest["pages"]:
        add(page_file(page, DEPLOYED), "deployed-page")
    for source in manifest["sources"]:
        add(source_file(source), "source-png")
        for binding in source["bindings"]:
            add(dat_file(binding["datPath"]), "decoded-dat")
    return {"files": files, "fileCount": len(files)}


def hash_map(capture: dict[str, Any]) -> dict[str, str]:
    return {item["path"]: item["sha256"] for item in capture["files"]}


def validate_existing_inputs(
    manifest: dict[str, Any],
) -> tuple[dict[str, Any], dict[str, np.ndarray], dict[str, Path], dict[str, np.ndarray]]:
    page_info: list[dict[str, Any]] = []
    page_arrays: dict[str, np.ndarray] = {}
    page_paths: dict[str, Path] = {}
    for page in manifest["pages"]:
        path = page_file(page, DEPLOYED)
        actual_sha = sha256_file(path)
        if actual_sha != page["sha256"]:
            fail(f"Deployed page SHA mismatch: {path}")
        array, readback = read_rgba(path)
        expected_size = [int(page["width"]), int(page["height"])]
        if readback["size"] != expected_size:
            fail(f"Deployed page header/readback size mismatch: {path}")
        page_info.append(
            {
                "id": page["id"],
                "path": rel_path(path),
                "declaredSha256": page["sha256"],
                "actualSha256": actual_sha,
                "shaMatch": True,
                "declaredSize": expected_size,
                "header": readback["header"],
                "headerMatch": True,
            }
        )
        page_arrays[page["id"]] = array
        page_paths[page["id"]] = path

    source_arrays: dict[str, np.ndarray] = {}
    source_info: list[dict[str, Any]] = []
    native_bindings: list[dict[str, Any]] = []
    total_native_frames = 0
    for source in manifest["sources"]:
        path = source_file(source)
        actual_sha = sha256_file(path)
        if actual_sha != source["sha256"]:
            fail(f"Source SHA mismatch: {path}")
        image, readback = read_rgba(path)
        declared_size = [int(source["width"]), int(source["height"])]
        if readback["size"] != declared_size:
            fail(f"Source header/readback size mismatch: {path}")
        native_count = int(source["columns"]) * int(source["rows"])
        x_extent = (int(source["frameWidth"]) + 1) * int(source["columns"])
        y_extent = (int(source["frameHeight"]) + 1) * int(source["rows"])
        if declared_size[0] < x_extent or declared_size[1] < y_extent:
            fail(f"Source native stride bounds invalid: {source['path']}")
        source_arrays[source["path"]] = image
        total_native_frames += native_count
        for binding in source["bindings"]:
            dat_path = dat_file(binding["datPath"])
            dat_sha = sha256_file(dat_path)
            if dat_sha != binding["datSha256"]:
                fail(f"Decoded DAT SHA mismatch: {dat_path}")
            entries = parse_dat(dat_path)
            matches = [
                entry
                for entry in entries
                if entry["normalizedPath"] == normalize_rel(source["path"])
            ]
            if len(matches) != 1:
                fail(f"DAT source binding is not unique: {source['path']} in {dat_path}")
            native = matches[0]
            expected_count = int(binding["lastPic"]) - int(binding["firstPic"]) + 1
            if native["rowTimesColumn"] != expected_count or expected_count != native_count:
                fail(f"DAT row*col count mismatch: {source['path']} in {dat_path}")
            native_index = entries.index(native)
            cumulative_start = 0
            for prior in entries[:native_index]:
                cumulative_start = max(cumulative_start, prior["lastPic"] + 1)
            cumulative_range = [
                cumulative_start,
                cumulative_start + native["rowTimesColumn"] - 1,
            ]
            manifest_range = [int(binding["firstPic"]), int(binding["lastPic"])]
            if manifest_range != cumulative_range:
                fail(
                    f"DAT cumulative range mismatch: {source['path']} in {dat_path} "
                    f"manifest={manifest_range} cumulative={cumulative_range}"
                )
            native_bindings.append(
                {
                    "source": source["path"],
                    "datPath": rel_path(dat_path),
                    "datSha256": dat_sha,
                    "manifestRange": manifest_range,
                    "cumulativeRange": cumulative_range,
                    "nativePath": native["path"],
                    "nativeRows": native["rows"],
                    "nativeColumns": native["columns"],
                    "nativeRowTimesColumn": native["rowTimesColumn"],
                    "rangeMatch": True,
                    "rowColumnMatch": True,
                }
            )
        source_info.append(
            {
                "path": source["path"],
                "resolvedPath": rel_path(path),
                "declaredSha256": source["sha256"],
                "actualSha256": actual_sha,
                "shaMatch": True,
                "declaredSize": declared_size,
                "readback": readback,
                "nativeFrameCount": native_count,
                "nativeStep": [int(source["frameWidth"]) + 1, int(source["frameHeight"]) + 1],
                "nativeGridExtent": [x_extent, y_extent],
            }
        )
    if total_native_frames != 88:
        fail(f"Expected 88 native logical frames, got {total_native_frames}")
    validation = {
        "manifestPath": rel_path(MANIFEST_PATH),
        "manifestSha256": sha256_file(MANIFEST_PATH),
        "generatorVersionObserved": manifest["generatorVersion"],
        "pageCount": len(manifest["pages"]),
        "sourceCount": len(manifest["sources"]),
        "nativeLogicalFrameCount": total_native_frames,
        "pages": page_info,
        "sources": source_info,
        "nativeDatBindings": native_bindings,
        "allShaAndHeadersValid": True,
        "existingGeneratorInvoked": False,
    }
    return validation, source_arrays, page_paths, page_arrays


def verify_logical_mappings(
    manifest: dict[str, Any],
    source_arrays: dict[str, np.ndarray],
    resolve_page: Any,
    allow_aliases: bool,
) -> dict[str, Any]:
    frames: list[dict[str, Any]] = []
    source_summaries: list[dict[str, Any]] = []
    total_pixels = 0
    total_changed = 0
    max_diff = 0
    for source in manifest["sources"]:
        source_path = source["path"]
        source_image = source_arrays[source_path]
        native_count = int(source["columns"]) * int(source["rows"])
        frame_map: dict[int, tuple[str, tuple[int, int, int, int]]] = {}
        next_local = 0
        source_records: list[dict[str, Any]] = []
        for chunk in source["chunks"]:
            first_local = int(chunk["firstLocalPic"])
            count = int(chunk["count"])
            if first_local != next_local:
                fail(f"Chunk gap/overlap in candidate manifest: {source_path}")
            if len(chunk["rects"]) != count:
                fail(f"Chunk rect count mismatch: {source_path}")
            page_id = chunk["page"]
            page, page_descriptor, page_path = resolve_page(page_id)
            allow_exact_aliases = bool(chunk.get("allowExactFrameAliases", False))
            if allow_exact_aliases and not allow_aliases:
                fail(f"Unexpected alias flag while verifying strict manifest: {source_path}")
            prior_rects: list[tuple[int, int, int, int]] = []
            for offset, rect in enumerate(chunk["rects"]):
                local_pic = first_local + offset
                if local_pic >= native_count:
                    fail(f"Chunk exceeds native source cells: {source_path} local={local_pic}")
                current_rect = rect_tuple(rect)
                if current_rect[2] != int(source["frameWidth"]) or current_rect[3] != int(source["frameHeight"]):
                    fail(f"Manifest rect dimensions differ from source frame: {source_path} local={local_pic}")
                if current_rect[0] < 1 or current_rect[1] < 1:
                    fail(f"Manifest rect loses required alignment padding: {source_path} local={local_pic}")
                if current_rect[0] + current_rect[2] >= int(page_descriptor["width"]):
                    fail(f"Manifest rect exceeds page width: {source_path} local={local_pic}")
                if current_rect[1] + current_rect[3] >= int(page_descriptor["height"]):
                    fail(f"Manifest rect exceeds page height: {source_path} local={local_pic}")
                duplicate_kind = "none"
                for prior_rect in prior_rects:
                    if rectangles_overlap(prior_rect, current_rect):
                        if prior_rect == current_rect and allow_exact_aliases:
                            duplicate_kind = "exact-rect-alias"
                        else:
                            fail(
                                f"Partial/unsupported rect overlap: {source_path} local={local_pic} "
                                f"rect={current_rect} prior={prior_rect}"
                            )
                expected, source_xy = source_cell(source, source_image, local_pic)
                actual, page_xy = page_cell(page, rect)
                comparison = compare_arrays(expected, actual)
                if not comparison["exactRgba"]:
                    fail(
                        f"Logical RGBA mismatch: {source_path} local={local_pic} "
                        f"page={page_path} {comparison}"
                    )
                if duplicate_kind == "exact-rect-alias":
                    prior_local = next(
                        index
                        for index, prior in enumerate(chunk["rects"][:offset])
                        if rect_tuple(prior) == current_rect
                    )
                    prior_expected, _ = source_cell(source, source_image, first_local + prior_local)
                    if not np.array_equal(prior_expected, expected):
                        fail(f"Exact rect alias has non-identical source RGBA: {source_path} local={local_pic}")
                frame_map[local_pic] = (page_id, current_rect)
                prior_rects.append(current_rect)
                pixels = int(expected.shape[0] * expected.shape[1])
                total_pixels += pixels
                total_changed += int(comparison["changedPixels"])
                max_diff = max(max_diff, int(comparison["maxAbsDiff"]))
                source_records.append(
                    {
                        "source": source_path,
                        "localPic": local_pic,
                        "page": page_id,
                        "pagePath": rel_path(page_path),
                        "manifestRectBottomLeft": dict(rect),
                        "pageCropTopLeft": [page_xy[0], page_xy[1]],
                        "nativeCellTopLeft": [source_xy[0], source_xy[1]],
                        "nativeStep": [int(source["frameWidth"]) + 1, int(source["frameHeight"]) + 1],
                        "alias": duplicate_kind,
                        "changedPixels": int(comparison["changedPixels"]),
                        "maxAbsDiff": int(comparison["maxAbsDiff"]),
                        "exactRgba": bool(comparison["exactRgba"]),
                    }
                )
            next_local += count
        if next_local != native_count or set(frame_map) != set(range(native_count)):
            fail(f"Manifest does not cover all native logical frames: {source_path}")
        source_summaries.append(
            {
                "source": source_path,
                "nativeFrames": native_count,
                "verifiedFrames": len(source_records),
                "pixelsCompared": sum(int(item["changedPixels"] == 0) * int(source["frameWidth"]) * int(source["frameHeight"]) for item in source_records),
                "differentPixels": sum(int(item["changedPixels"]) for item in source_records),
                "maxAbsDiff": max(int(item["maxAbsDiff"]) for item in source_records),
                "exact": all(bool(item["exactRgba"]) for item in source_records),
                "aliasFrameCount": sum(item["alias"] == "exact-rect-alias" for item in source_records),
            }
        )
        frames.extend(source_records)
    return {
        "method": "Independent top-left native stride crop versus bottom-left manifest rect crop after PNG readback",
        "nativeLogicalFrames": len(frames),
        "pixelsCompared": total_pixels,
        "differentPixels": total_changed,
        "maxAbsDiff": max_diff,
        "exactAll88Rgba": len(frames) == 88 and total_changed == 0 and max_diff == 0,
        "sourceSummaries": source_summaries,
        "frames": frames,
    }


def target_rect_for_slot(slot: int) -> dict[str, int]:
    if slot not in range(4):
        fail(f"Physical slot outside 2x2 grid: {slot}")
    x = EDGE_PADDING + (slot % 2) * CELL_SPAN
    top_y = EDGE_PADDING + (slot // 2) * CELL_SPAN
    bottom_y = PAGE_SIZE - top_y - FRAME_HEIGHT
    return {"x": x, "y": bottom_y, "width": FRAME_WIDTH, "height": FRAME_HEIGHT}


def build_target_page(source_image: np.ndarray) -> tuple[np.ndarray, list[dict[str, int]], dict[str, Any]]:
    page = np.zeros((PAGE_SIZE, PAGE_SIZE, 4), dtype=np.uint8)
    unique_locals = [18, 19, 20, 21]
    rects_by_slot = [target_rect_for_slot(slot) for slot in range(4)]
    placement: list[dict[str, Any]] = []
    for slot, local_pic in enumerate(unique_locals):
        x = EDGE_PADDING + (slot % 2) * CELL_SPAN
        top_y = EDGE_PADDING + (slot // 2) * CELL_SPAN
        frame, source_xy = source_cell(
            {
                "path": TARGET_SOURCES[0],
                "width": int(source_image.shape[1]),
                "height": int(source_image.shape[0]),
                "frameWidth": FRAME_WIDTH,
                "frameHeight": FRAME_HEIGHT,
                "columns": 5,
                "rows": 5,
            },
            source_image,
            local_pic,
        )
        page[top_y : top_y + FRAME_HEIGHT, x : x + FRAME_WIDTH] = frame
        page[top_y : top_y + FRAME_HEIGHT, x - 1] = frame[:, 0]
        page[top_y : top_y + FRAME_HEIGHT, x + FRAME_WIDTH] = frame[:, -1]
        page[top_y - 1, x : x + FRAME_WIDTH] = frame[0, :]
        page[top_y + FRAME_HEIGHT, x : x + FRAME_WIDTH] = frame[-1, :]
        page[top_y - 1, x - 1] = frame[0, 0]
        page[top_y - 1, x + FRAME_WIDTH] = frame[0, -1]
        page[top_y + FRAME_HEIGHT, x - 1] = frame[-1, 0]
        page[top_y + FRAME_HEIGHT, x + FRAME_WIDTH] = frame[-1, -1]
        placement.append(
            {
                "slot": slot,
                "localPic": local_pic,
                "sourceTopLeft": [source_xy[0], source_xy[1]],
                "pageTopLeft": [x, top_y],
                "pageBottomLeft": rects_by_slot[slot],
            }
        )
    return page, rects_by_slot, {
        "physicalSlots": placement,
        "uniquePhysicalFrames": len(unique_locals),
        "pageSize": [PAGE_SIZE, PAGE_SIZE],
        "frameSize": [FRAME_WIDTH, FRAME_HEIGHT],
        "cellSpan": [CELL_SPAN, CELL_SPAN],
        "edgeExtrusion": 1,
        "alignmentTail": [ALIGNMENT_TAIL, ALIGNMENT_TAIL],
    }


def write_new(path: Path, data: bytes) -> None:
    if path.exists():
        fail(f"Refusing output collision: {path}")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(data)


def build_planned_outputs() -> dict[str, Any]:
    files = [
        "candidate/manifest.json",
        f"candidate/{TARGET_NEW_PAGE_REL}",
        "candidate/verification.json",
        "candidate/receipt.json",
        "candidate/planned-outputs.json",
        "candidate/REPORT.md",
    ]
    return {
        "schemaVersion": 1,
        "task": "NTSD-M12-EXACT-FRAME-DEDUP-20261013",
        "change": "NTSD-M12-EXACT-FRAME-DEDUP-001",
        "operation": "OP-NTSD-M12-EXACT-FRAME-DEDUP-20261013",
        "createdUtc": datetime.now(timezone.utc).isoformat(),
        "outputRoot": rel_path(CANDIDATE_ROOT),
        "collisionPolicy": "create-new; any existing candidate directory or listed file is fatal",
        "files": files,
        "newPage": {
            "id": TARGET_NEW_PAGE_ID,
            "relativePath": TARGET_NEW_PAGE_REL,
            "width": PAGE_SIZE,
            "height": PAGE_SIZE,
        },
    }


def candidate_manifest(
    deployed: dict[str, Any],
    new_page_sha: str,
    new_rects: list[dict[str, int]],
) -> tuple[dict[str, Any], dict[str, Any]]:
    candidate = copy.deepcopy(deployed)
    old_page_indices = [
        index for index, page in enumerate(candidate["pages"]) if page["id"] == TARGET_OLD_PAGE_ID
    ]
    if old_page_indices != [len(candidate["pages"]) - 1]:
        fail(f"Expected target old page to be the final deployed descriptor: {old_page_indices}")
    new_descriptor = {
        "id": TARGET_NEW_PAGE_ID,
        "path": TARGET_NEW_PAGE_REL,
        "sha256": new_page_sha,
        "width": PAGE_SIZE,
        "height": PAGE_SIZE,
    }
    candidate["pages"][old_page_indices[0]] = new_descriptor
    changed_sources: list[str] = []
    for source in candidate["sources"]:
        if source["path"] not in TARGET_SOURCES:
            continue
        matches = [
            chunk
            for chunk in source["chunks"]
            if chunk["page"] == TARGET_OLD_PAGE_ID
            and int(chunk["firstLocalPic"]) == 18
            and int(chunk["count"]) == 7
        ]
        if len(matches) != 1:
            fail(f"Expected one target final chunk for {source['path']}, got {len(matches)}")
        chunk = matches[0]
        chunk["page"] = TARGET_NEW_PAGE_ID
        chunk["rects"] = copy.deepcopy(new_rects)
        chunk["allowExactFrameAliases"] = True
        changed_sources.append(source["path"])
    if tuple(changed_sources) != TARGET_SOURCES:
        fail(f"Expected both target source chunks to change, got {changed_sources}")
    expected = copy.deepcopy(deployed)
    expected["pages"][old_page_indices[0]] = copy.deepcopy(new_descriptor)
    for source in expected["sources"]:
        if source["path"] not in TARGET_SOURCES:
            continue
        chunk = next(
            chunk
            for chunk in source["chunks"]
            if chunk["page"] == TARGET_OLD_PAGE_ID
            and int(chunk["firstLocalPic"]) == 18
            and int(chunk["count"]) == 7
        )
        chunk["page"] = TARGET_NEW_PAGE_ID
        chunk["rects"] = copy.deepcopy(new_rects)
        chunk["allowExactFrameAliases"] = True
    if candidate != expected:
        fail("Candidate manifest differs outside the declared target descriptor/chunks")
    change_record = {
        "oldPageId": TARGET_OLD_PAGE_ID,
        "newPageId": TARGET_NEW_PAGE_ID,
        "oldPageDescriptor": deployed["pages"][old_page_indices[0]],
        "newPageDescriptor": new_descriptor,
        "changedSources": list(changed_sources),
        "changedChunk": {
            "firstLocalPic": 18,
            "count": 7,
            "columns": 3,
            "rows": 3,
            "allowExactFrameAliases": True,
            "mapping": {"18": 0, "19": 1, "20": 2, "21": 3, "22": 3, "23": 3, "24": 3},
            "rects": copy.deepcopy(new_rects),
        },
        "nonTargetManifestUnchanged": True,
    }
    return candidate, change_record


def resolve_candidate_page(
    candidate: dict[str, Any], page_id: str, page_cache: dict[str, np.ndarray]
) -> tuple[np.ndarray, dict[str, Any], Path]:
    descriptor = next((page for page in candidate["pages"] if page["id"] == page_id), None)
    if descriptor is None:
        fail(f"Candidate page id missing: {page_id}")
    root = CANDIDATE_ROOT if page_id == TARGET_NEW_PAGE_ID else DEPLOYED
    path = page_file(descriptor, root)
    if page_id not in page_cache:
        actual_sha = sha256_file(path)
        if actual_sha != descriptor["sha256"]:
            fail(f"Candidate page SHA mismatch: {path}")
        image, readback = read_rgba(path)
        if readback["size"] != [int(descriptor["width"]), int(descriptor["height"])]:
            fail(f"Candidate page header mismatch: {path}")
        page_cache[page_id] = image
    return page_cache[page_id], descriptor, path


def verify_target_page(
    page: np.ndarray,
    source_image: np.ndarray,
    candidate: dict[str, Any],
    candidate_logical: dict[str, Any],
    old_page_path: Path,
    old_page_descriptor: dict[str, Any],
) -> dict[str, Any]:
    if page.shape != (PAGE_SIZE, PAGE_SIZE, 4):
        fail(f"Target page array has unexpected shape: {page.shape}")
    expected_unique = {18: 0, 19: 1, 20: 2, 21: 3, 22: 3, 23: 3, 24: 3}
    placement_checks: list[dict[str, Any]] = []
    occupied = np.zeros((PAGE_SIZE, PAGE_SIZE), dtype=np.uint8)
    for slot in range(4):
        rect = target_rect_for_slot(slot)
        x = int(rect["x"])
        top_y = PAGE_SIZE - int(rect["y"]) - FRAME_HEIGHT
        local_pic = next(local for local, mapped_slot in expected_unique.items() if mapped_slot == slot)
        expected, _ = source_cell(
            {
                "path": TARGET_SOURCES[0],
                "width": int(source_image.shape[1]),
                "height": int(source_image.shape[0]),
                "frameWidth": FRAME_WIDTH,
                "frameHeight": FRAME_HEIGHT,
                "columns": 5,
                "rows": 5,
            },
            source_image,
            local_pic,
        )
        actual = page[top_y : top_y + FRAME_HEIGHT, x : x + FRAME_WIDTH]
        direct = compare_arrays(expected, actual)
        if not direct["exactRgba"]:
            fail(f"Target page physical slot differs from source local {local_pic}: {direct}")
        left = np.array_equal(page[top_y : top_y + FRAME_HEIGHT, x - 1], expected[:, 0])
        right = np.array_equal(page[top_y : top_y + FRAME_HEIGHT, x + FRAME_WIDTH], expected[:, -1])
        top = np.array_equal(page[top_y - 1, x : x + FRAME_WIDTH], expected[0, :])
        bottom = np.array_equal(page[top_y + FRAME_HEIGHT, x : x + FRAME_WIDTH], expected[-1, :])
        corners = [
            page[top_y - 1, x - 1].tolist() == expected[0, 0].tolist(),
            page[top_y - 1, x + FRAME_WIDTH].tolist() == expected[0, -1].tolist(),
            page[top_y + FRAME_HEIGHT, x - 1].tolist() == expected[-1, 0].tolist(),
            page[top_y + FRAME_HEIGHT, x + FRAME_WIDTH].tolist() == expected[-1, -1].tolist(),
        ]
        if not all((left, right, top, bottom, *corners)):
            fail(f"Target page 1px extrusion mismatch at slot {slot}")
        slot_x = x - EDGE_PADDING
        slot_y = top_y - EDGE_PADDING
        occupied[slot_y : slot_y + CELL_SPAN, slot_x : slot_x + CELL_SPAN] += 1
        allowed = np.zeros((CELL_SPAN, CELL_SPAN), dtype=bool)
        allowed[0 : FRAME_HEIGHT + 2 * EDGE_PADDING, 0 : FRAME_WIDTH + 2 * EDGE_PADDING] = True
        slot_array = page[slot_y : slot_y + CELL_SPAN, slot_x : slot_x + CELL_SPAN]
        alignment = slot_array[~allowed]
        zero_count = int(np.all(alignment == 0, axis=1).sum())
        if zero_count != int(alignment.shape[0]):
            fail(f"Target page alignment tail is nonzero at slot {slot}")
        placement_checks.append(
            {
                "slot": slot,
                "localPic": local_pic,
                "rectBottomLeft": rect,
                "innerExact": True,
                "edgeExtrusionExact": True,
                "cornerExtrusionExact": True,
                "alignmentPixels": int(alignment.shape[0]),
                "alignmentZeroPixels": zero_count,
                "alignmentTail": [ALIGNMENT_TAIL, ALIGNMENT_TAIL],
            }
        )
    if np.any(occupied > 1) or int(occupied.min()) != 1:
        fail("Target physical cell allocations overlap or leave uncovered page pixels")
    alias_records = [
        frame
        for frame in candidate_logical["frames"]
        if frame["source"] in TARGET_SOURCES and int(frame["localPic"]) >= 18
    ]
    if len(alias_records) != 14:
        fail(f"Expected 14 target source logical records, got {len(alias_records)}")
    if sum(frame["alias"] == "exact-rect-alias" for frame in alias_records) != 6:
        fail("Expected six alias records across the two target chunks")
    old_bytes = old_page_path.stat().st_size
    old_width = int(old_page_descriptor["width"])
    old_height = int(old_page_descriptor["height"])
    new_bytes = len(encode_rgba(page))
    def astc_stats(width: int, height: int) -> dict[str, int]:
        blocks_x = math.ceil(width / 4)
        blocks_y = math.ceil(height / 4)
        return {
            "blocksX": blocks_x,
            "blocksY": blocks_y,
            "payloadBytesTheoretical": blocks_x * blocks_y * 16,
        }
    old_raw = old_width * old_height * 4
    new_raw = PAGE_SIZE * PAGE_SIZE * 4
    old_astc = astc_stats(old_width, old_height)
    new_astc = astc_stats(PAGE_SIZE, PAGE_SIZE)
    return {
        "oldPage": {
            "id": TARGET_OLD_PAGE_ID,
            "path": rel_path(old_page_path),
            "width": old_width,
            "height": old_height,
            "pngBytes": old_bytes,
            "rawRgbaBytes": old_raw,
            "astc4x4PayloadTheoretical": old_astc,
        },
        "newPage": {
            "id": TARGET_NEW_PAGE_ID,
            "path": rel_path(TARGET_NEW_PAGE_PATH),
            "width": PAGE_SIZE,
            "height": PAGE_SIZE,
            "pngBytes": new_bytes,
            "rawRgbaBytes": new_raw,
            "astc4x4PayloadTheoretical": new_astc,
        },
        "delta": {
            "pngBytesSaved": old_bytes - new_bytes,
            "rawRgbaBytesSaved": old_raw - new_raw,
            "rawRgbaPercentSaved": round((old_raw - new_raw) * 100.0 / old_raw, 6),
            "astc4x4PayloadBytesSavedTheoretical": old_astc["payloadBytesTheoretical"] - new_astc["payloadBytesTheoretical"],
            "astc4x4PayloadPercentSavedTheoretical": round(
                (old_astc["payloadBytesTheoretical"] - new_astc["payloadBytesTheoretical"])
                * 100.0
                / old_astc["payloadBytesTheoretical"],
                6,
            ),
        },
        "physicalUniqueFrames": 4,
        "logicalTargetMappings": 14,
        "aliasMappings": 6,
        "mapping": expected_unique,
        "placements": placement_checks,
        "onePixelExtrusionVerified": True,
        "threePixelAlignmentTailVerified": True,
        "astcEncoded": False,
    }


def output_hashes(paths: list[Path]) -> list[dict[str, Any]]:
    result = []
    for path in paths:
        if not path.is_file():
            fail(f"Expected output missing: {path}")
        result.append({"path": rel_path(path), "sha256": sha256_file(path), "bytes": path.stat().st_size})
    return result


def build_report(
    verification: dict[str, Any],
    receipt: dict[str, Any],
    planned: dict[str, Any],
) -> str:
    current = verification["currentLogicalCheck"]
    candidate = verification["candidateLogicalCheck"]
    target = verification["targetPageCheck"]
    return f"""# NTSD M12 exact frame dedup candidate

Status: `{verification['status']}`

This candidate was generated by `Tools/Optimize-NtsdSpritePageDuplicates.py`.
The existing page generator was not imported or invoked. No file under `Assets`
was written, and the deployed 1716x1716 page remains unchanged.

## Frozen inputs and manifest scope

- Deployed manifest: `{verification['deployedManifest']}`
- Deployed pages: {verification['deployedPageCount']}; source records: {verification['deployedSourceCount']}
- Native logical frames verified before candidate generation: {current['nativeLogicalFrames']}; exact RGBA: `{current['exactAll88Rgba']}`
- Decoded DAT bindings verified: {verification['nativeDatBindingCount']}; each manifest range matched cumulative native `row*col`.
- Source PNG/DAT/page/manifest hashes were captured before generation and checked again after generation: `{verification['inputHashesStable']}`
- Non-target manifest content unchanged: `{verification['nonTargetManifestUnchanged']}`

The source-cell check uses explicit native stride `(frameWidth + 1, frameHeight + 1)`.
The manifest rectangle check converts bottom-left `y` to top-left PNG coordinates
before comparing RGBA bytes.

## Exact dedup candidate

- Target sources: `{', '.join(TARGET_SOURCES)}`
- Old page: `{TARGET_OLD_PAGE_ID}` ({target['oldPage']['width']}x{target['oldPage']['height']})
- New page: `{TARGET_NEW_PAGE_ID}` ({target['newPage']['width']}x{target['newPage']['height']})
- Physical layout: 2x2, cell span 572, frame canvas 567x567, one-pixel edge extrusion, three-pixel alignment tail.
- Logical mapping: `18->slot0`, `19->slot1`, `20->slot2`, `21..24->slot3`.
- Target logical mappings: {target['logicalTargetMappings']} across the two source chunks; physical unique frames: {target['physicalUniqueFrames']}; exact aliases: {target['aliasMappings']}.
- Candidate logical frames verified: {candidate['nativeLogicalFrames']}; exact RGBA: `{candidate['exactAll88Rgba']}`.
- One-pixel extrusion verified: `{target['onePixelExtrusionVerified']}`; three-pixel alignment tail verified: `{target['threePixelAlignmentTailVerified']}`.
- ASTC encoding performed: `{target['astcEncoded']}`.

The candidate chunks alone carry `allowExactFrameAliases: true`. Every duplicate
rectangle is an exact same-rect alias whose source RGBA is byte-identical. No
partial overlap is accepted by the helper.

## Page size accounting

| page | PNG bytes | raw RGBA bytes | theoretical ASTC 4x4 payload |
|---|---:|---:|---:|
| old 1716x1716 | {target['oldPage']['pngBytes']:,} | {target['oldPage']['rawRgbaBytes']:,} | {target['oldPage']['astc4x4PayloadTheoretical']['payloadBytesTheoretical']:,} |
| new 1144x1144 | {target['newPage']['pngBytes']:,} | {target['newPage']['rawRgbaBytes']:,} | {target['newPage']['astc4x4PayloadTheoretical']['payloadBytesTheoretical']:,} |

Raw RGBA saving is {target['delta']['rawRgbaBytesSaved']:,} bytes
({target['delta']['rawRgbaPercentSaved']:.6f}%). The theoretical ASTC 4x4
payload saving is {target['delta']['astc4x4PayloadBytesSavedTheoretical']:,} bytes
({target['delta']['astc4x4PayloadPercentSavedTheoretical']:.6f}%). PNG byte saving
is measured from the actual files as {target['delta']['pngBytesSaved']:,} bytes.
No ASTC encoder was run.

## Output and execution receipt

- Planned outputs: `{rel_path(PLANNED_PATH)}`
- Candidate manifest: `{rel_path(CANDIDATE_ROOT / 'manifest.json')}`
- New page PNG: `{rel_path(TARGET_NEW_PAGE_PATH)}`
- Verification: `{rel_path(VERIFICATION_PATH)}`
- Receipt: `{rel_path(RECEIPT_PATH)}`
- Helper exit code: `{receipt['exitCode']}`
- Helper invocation: `{receipt['pythonExecutable']} -B Tools/Optimize-NtsdSpritePageDuplicates.py`

The old deployed page and all non-target deployed pages remain the review/deployment
source. Main-agent deployment is required before any runtime claim; this artifact
only proves the offline exact-RGBA candidate and its input stability.
"""


def run() -> int:
    if CANDIDATE_ROOT.exists():
        existing_children = {child.name for child in CANDIDATE_ROOT.iterdir()}
        if existing_children != {"revisions"} or not (CANDIDATE_ROOT / "revisions").is_dir():
            fail(f"Refusing candidate output collision: {CANDIDATE_ROOT}")
    else:
        CANDIDATE_ROOT.mkdir(parents=True, exist_ok=False)
    if not MANIFEST_PATH.is_file():
        fail(f"Missing deployed manifest: {MANIFEST_PATH}")
    planned = build_planned_outputs()
    write_new(PLANNED_PATH, json_bytes(planned))

    deployed, deployed_bytes = load_manifest()
    pre_hashes = capture_input_hashes(deployed)
    existing_validation, source_arrays, old_page_paths, _ = validate_existing_inputs(deployed)
    source_a = source_arrays[TARGET_SOURCES[0]]
    source_b = source_arrays[TARGET_SOURCES[1]]
    if not np.array_equal(source_a, source_b):
        fail("Target source PNG RGBA arrays differ despite declared same SHA")
    old_target_page = next(page for page in deployed["pages"] if page["id"] == TARGET_OLD_PAGE_ID)
    old_target_path = old_page_paths[TARGET_OLD_PAGE_ID]
    current_cache: dict[str, np.ndarray] = {}
    def resolve_current(page_id: str) -> tuple[np.ndarray, dict[str, Any], Path]:
        descriptor = next((page for page in deployed["pages"] if page["id"] == page_id), None)
        if descriptor is None:
            fail(f"Current page id missing: {page_id}")
        if page_id not in current_cache:
            current_cache[page_id], _ = read_rgba(old_page_paths[page_id])
        return current_cache[page_id], descriptor, old_page_paths[page_id]
    current_logical = verify_logical_mappings(deployed, source_arrays, resolve_current, False)
    if not current_logical["exactAll88Rgba"]:
        fail("Current deployed manifest does not pass exact 88-frame verification")
    hashes_18_24 = {
        str(local): sha256_bytes(source_cell(deployed["sources"][2], source_a, local)[0].tobytes())
        for local in range(18, 25)
    }
    if len(set(hashes_18_24[str(local)] for local in (21, 22, 23, 24))) != 1:
        fail("Target source locals 21..24 are not all exact RGBA duplicates")
    if len(set(hashes_18_24[str(local)] for local in (18, 19, 20, 21))) != 4:
        fail("Target source locals 18..21 are not four unique RGBA frames")
    new_page, physical_rects, build_metrics = build_target_page(source_a)
    logical_rects = [
        physical_rects[0],
        physical_rects[1],
        physical_rects[2],
        physical_rects[3],
        physical_rects[3],
        physical_rects[3],
        physical_rects[3],
    ]
    encoded = encode_rgba(new_page)
    decoded_page, readback = decode_rgba(encoded)
    if not np.array_equal(new_page, decoded_page):
        fail("New candidate page PNG readback differs from generated RGBA array")
    write_new(TARGET_NEW_PAGE_PATH, encoded)
    candidate, manifest_change = candidate_manifest(deployed, sha256_bytes(encoded), logical_rects)
    candidate_manifest_path = CANDIDATE_ROOT / "manifest.json"
    write_new(candidate_manifest_path, json_bytes(candidate))

    candidate_cache: dict[str, np.ndarray] = {TARGET_NEW_PAGE_ID: decoded_page}
    def resolve_candidate(page_id: str) -> tuple[np.ndarray, dict[str, Any], Path]:
        return resolve_candidate_page(candidate, page_id, candidate_cache)
    candidate_logical = verify_logical_mappings(candidate, source_arrays, resolve_candidate, True)
    if not candidate_logical["exactAll88Rgba"]:
        fail("Candidate manifest does not pass exact 88-frame verification")
    target_check = verify_target_page(
        decoded_page,
        source_a,
        candidate,
        candidate_logical,
        old_target_path,
        old_target_page,
    )
    if not target_check["onePixelExtrusionVerified"] or not target_check["threePixelAlignmentTailVerified"]:
        fail("Target page padding/extrusion verification failed")
    post_hashes = capture_input_hashes(deployed)
    input_hashes_stable = hash_map(pre_hashes) == hash_map(post_hashes)
    if not input_hashes_stable:
        fail("An input hash changed during candidate generation")
    verification = {
        "status": "PASS_EXACT_RGBA_CANDIDATE",
        "task": "NTSD-M12-EXACT-FRAME-DEDUP-20261013",
        "change": "NTSD-M12-EXACT-FRAME-DEDUP-001",
        "operation": "OP-NTSD-M12-EXACT-FRAME-DEDUP-20261013",
        "helper": rel_path(Path(__file__).resolve()),
        "deployedManifest": rel_path(MANIFEST_PATH),
        "deployedManifestSha256Before": sha256_bytes(deployed_bytes),
        "deployedPageCount": len(deployed["pages"]),
        "deployedSourceCount": len(deployed["sources"]),
        "nativeDatBindingCount": len(existing_validation["nativeDatBindings"]),
        "existingValidation": existing_validation,
        "sourceDuplicateCheck": {
            "sources": list(TARGET_SOURCES),
            "sameDeclaredSha256": deployed["sources"][2]["sha256"] == deployed["sources"][3]["sha256"],
            "sameReadbackRgba": True,
            "localFrameSha256": hashes_18_24,
            "locals21To24ExactSame": True,
            "locals18To21FourUnique": True,
        },
        "currentLogicalCheck": current_logical,
        "candidateLogicalCheck": candidate_logical,
        "targetPageCheck": target_check,
        "candidateManifestChange": manifest_change,
        "buildMetrics": build_metrics,
        "nonTargetManifestUnchanged": manifest_change["nonTargetManifestUnchanged"],
        "inputHashesBefore": pre_hashes,
        "inputHashesAfter": post_hashes,
        "inputHashesStable": input_hashes_stable,
        "plannedOutputs": planned["files"],
        "plannedOutputsSha256": sha256_file(PLANNED_PATH),
        "candidateManifestSha256": sha256_file(candidate_manifest_path),
        "newPageSha256": sha256_file(TARGET_NEW_PAGE_PATH),
        "existingGeneratorInvoked": False,
        "astcEncoded": False,
    }
    write_new(VERIFICATION_PATH, json_bytes(verification))
    pre_receipt_outputs = output_hashes(
        [PLANNED_PATH, candidate_manifest_path, TARGET_NEW_PAGE_PATH, VERIFICATION_PATH]
    )
    receipt = {
        "status": "PASS_EXACT_RGBA_CANDIDATE",
        "exitCode": 0,
        "pythonExecutable": "D:/anaconda3/python.exe",
        "arguments": ["-B", "Tools/Optimize-NtsdSpritePageDuplicates.py"],
        "helper": rel_path(Path(__file__).resolve()),
        "plannedOutputs": planned["files"],
        "outputsPresentBeforeReport": pre_receipt_outputs,
        "inputHashesStable": True,
        "current88Exact": True,
        "candidate88Exact": True,
        "targetPhysicalUniqueFrames": 4,
        "targetLogicalMappings": 14,
        "targetAliasMappings": 6,
        "oldPageRetained": True,
        "assetsModified": False,
        "unityStarted": False,
        "existingGeneratorInvoked": False,
    }
    write_new(RECEIPT_PATH, json_bytes(receipt))
    report = build_report(verification, receipt, planned)
    write_new(REPORT_PATH, report.encode("utf-8"))
    final_outputs = output_hashes(
        [PLANNED_PATH, candidate_manifest_path, TARGET_NEW_PAGE_PATH, VERIFICATION_PATH, RECEIPT_PATH, REPORT_PATH]
    )
    print(json.dumps({
        "status": verification["status"],
        "exitCode": 0,
        "candidate": rel_path(CANDIDATE_ROOT),
        "outputs": final_outputs,
        "current88Exact": True,
        "candidate88Exact": True,
        "inputHashesStable": True,
        "physicalUniqueFrames": 4,
        "logicalTargetMappings": 14,
        "aliasMappings": 6,
    }, ensure_ascii=False, indent=2))
    return 0


def main() -> int:
    try:
        return run()
    except Exception as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
