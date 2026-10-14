#!/usr/bin/env python3
"""Stage a lossless NTSD D0 base-plus-RGBA-patch sprite page.

This helper is deliberately offline and staging-only.  It reads the current
LoganRuntime VFS/DAT/catalog and the current derived page manifest, then writes
an immutable candidate under the dedicated M12 D0 artifact directory.  It does
not touch Assets, DAT files, the current manifest, Unity, ASTC, or P1.

The D0 candidate keeps one full-canvas base tile (localPic 0) and packs every
non-empty full-RGBA difference bounding box into the same page.  The runtime
can replace the packed patch bytes, including alpha=0, over the shared base.
"""

from __future__ import annotations

import argparse
import copy
import csv
import hashlib
import io
import json
import re
import shutil
import sys
import traceback
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

import numpy as np
from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
CONTENT_ROOT = ROOT / "Assets/NTSD/Content/LoganRuntime"
VFS_ROOT = CONTENT_ROOT / "vfs"
DAT_ROOT = CONTENT_ROOT / "decoded_dat"
CATALOG_PATH = CONTENT_ROOT / "catalog.csv"
DEPLOYED_ROOT = CONTENT_ROOT / "derived_sprite_pages"
CURRENT_MANIFEST_PATH = DEPLOYED_ROOT / "manifest.json"
ARTIFACT_ROOT = ROOT / "artifacts/diagnostics/M12-d0-base-patch-20261013"
DEFAULT_OUTPUT_ROOT = ARTIFACT_ROOT / "candidate"

TASK_ID = "NTSD-M12-D0-BASE-PATCH-20261013"
CHANGE_ID = "NTSD-M12-D0-BASE-PATCH-001"
OPERATION_ID = "OP-NTSD-M12-D0-BASE-PATCH-20261013"
GENERATOR_VERSION = "NTSD_DAT_D0_BASE_PATCH_V1"

D0_SOURCE = "s/0/d0.png"
D0_DAT = "s/0/0.dat"
D0_OID = 299
D0_FRAME_WIDTH = 310
D0_FRAME_HEIGHT = 125
D0_COLUMNS = 2
D0_ROWS = 17
D0_FRAME_COUNT = 34
D0_STRIDE_X = 311
D0_STRIDE_Y = 126
EDGE_PADDING = 1
ALLOCATION_ALIGNMENT = 4
PAGE_LIMIT = 2048
MIN_PAGE_WIDTH = 512
MAX_PAGE_WIDTH = 2048
NEW_PAGE_TAG = "d0basepatch20261013"

FILE_RE = re.compile(
    r"^\s*file\((?P<start>\d+)(?:-(?P<end>\d+))?\):\s*"
    r"(?P<source>\S+)\s+w:\s*(?P<width>\d+)\s+h:\s*(?P<height>\d+)\s+"
    r"row:\s*(?P<columns>\d+)\s+col:\s*(?P<rows>\d+)\s*$",
    re.IGNORECASE,
)


class GenerationError(RuntimeError):
    """A bounded staging failure that should remain recorded in the candidate."""


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


def root_relative(path: Path) -> str:
    return path.resolve().relative_to(ROOT.resolve()).as_posix()


def normalize_path(value: str) -> str:
    return value.strip().replace("\\", "/").lstrip("/").lower()


def align4(value: int) -> int:
    return (int(value) + ALLOCATION_ALIGNMENT - 1) // ALLOCATION_ALIGNMENT * ALLOCATION_ALIGNMENT


def write_new_bytes(path: Path, data: bytes) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("xb") as stream:
        stream.write(data)


def write_new_json(path: Path, value: Any) -> None:
    payload = (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
    write_new_bytes(path, payload)


def write_new_text(path: Path, value: str) -> None:
    write_new_bytes(path, value.encode("utf-8"))


def png_bytes(array: np.ndarray) -> bytes:
    if array.dtype != np.uint8 or array.ndim != 3 or array.shape[2] != 4:
        raise GenerationError(f"PNG input must be RGBA uint8, got shape={array.shape} dtype={array.dtype}")
    buffer = io.BytesIO()
    Image.fromarray(array, "RGBA").save(buffer, format="PNG", compress_level=9)
    return buffer.getvalue()


def read_rgba(path: Path) -> tuple[np.ndarray, dict[str, Any]]:
    try:
        with Image.open(path) as image:
            original_mode = image.mode
            rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
            size = [int(image.width), int(image.height)]
    except Exception as error:  # pragma: no cover - exercised by input failures
        raise GenerationError(f"cannot decode PNG {path}: {error}") from error
    return rgba, {"mode": original_mode, "width": size[0], "height": size[1]}


def png_header(data: bytes) -> dict[str, Any]:
    if len(data) < 33 or data[:8] != b"\x89PNG\r\n\x1a\n":
        raise GenerationError("PNG signature/header missing")
    if int.from_bytes(data[8:12], "big") != 13 or data[12:16] != b"IHDR":
        raise GenerationError("PNG IHDR header invalid")
    width = int.from_bytes(data[16:20], "big")
    height = int.from_bytes(data[20:24], "big")
    bit_depth = data[24]
    color_type = data[25]
    if bit_depth != 8 or color_type != 6:
        raise GenerationError(f"PNG is not 8-bit RGBA: bitDepth={bit_depth} colorType={color_type}")
    return {"width": width, "height": height, "bitDepth": bit_depth, "colorType": color_type, "mode": "RGBA"}


def file_metadata(path: Path, *, include_dimensions: bool = False) -> dict[str, Any]:
    data = path.read_bytes()
    result: dict[str, Any] = {
        "path": root_relative(path),
        "bytes": len(data),
        "sha256": sha256_bytes(data),
    }
    if include_dimensions:
        with Image.open(path) as image:
            result.update({"width": int(image.width), "height": int(image.height), "mode": image.mode})
    return result


def dat_absolute(relative: str) -> Path:
    return DAT_ROOT / Path(*relative.replace("\\", "/").split("/"))


def source_absolute(relative: str) -> Path:
    return VFS_ROOT / Path(*relative.replace("\\", "/").split("/"))


def parse_catalog() -> tuple[list[str], list[dict[str, str]]]:
    if not CATALOG_PATH.is_file():
        raise GenerationError(f"catalog missing: {CATALOG_PATH}")
    with CATALOG_PATH.open("r", encoding="utf-8-sig", newline="") as stream:
        reader = csv.DictReader(stream)
        if reader.fieldnames is None:
            raise GenerationError("catalog has no header")
        rows = list(reader)
        return list(reader.fieldnames), rows


def parse_dat_declarations(path: Path) -> list[dict[str, Any]]:
    if not path.is_file():
        raise GenerationError(f"DAT missing: {path}")
    cumulative_native = 0
    declarations: list[dict[str, Any]] = []
    text = path.read_text(encoding="utf-8-sig")
    for line_number, line in enumerate(text.splitlines(), start=1):
        match = FILE_RE.match(line)
        if match is None:
            continue
        start = int(match.group("start"))
        end = int(match.group("end")) if match.group("end") is not None else None
        columns = int(match.group("columns"))
        rows = int(match.group("rows"))
        count = columns * rows
        inferred_end = start + count - 1
        if end is None:
            end = inferred_end
        declarations.append(
            {
                "line": line_number,
                "sourcePath": match.group("source").replace("\\", "/"),
                "sourceKey": normalize_path(match.group("source")),
                "declaredStart": start,
                "declaredEnd": end,
                "declaredCount": end - start + 1,
                "width": int(match.group("width")),
                "height": int(match.group("height")),
                "columns": columns,
                "rows": rows,
                "count": count,
                "firstPic": cumulative_native,
                "lastPic": cumulative_native + count - 1,
            }
        )
        cumulative_native += count
    return declarations


def find_d0_catalog_row(rows: list[dict[str, str]]) -> dict[str, str]:
    matches = [row for row in rows if normalize_path(row.get("source_path", "")) == normalize_path(D0_DAT)]
    if len(matches) != 1:
        raise GenerationError(f"catalog must resolve exactly one D0 DAT row, got {len(matches)}")
    row = matches[0]
    try:
        oid = int(row["id"])
    except (KeyError, TypeError, ValueError) as error:
        raise GenerationError(f"D0 catalog row has no integer id: {row}") from error
    if oid != D0_OID:
        raise GenerationError(f"D0 catalog OID mismatch: expected {D0_OID}, got {oid}")
    return row


def verify_d0_identity() -> dict[str, Any]:
    catalog_fields, catalog_rows = parse_catalog()
    catalog_row = find_d0_catalog_row(catalog_rows)
    dat_path = dat_absolute(D0_DAT)
    declarations = parse_dat_declarations(dat_path)
    target = [item for item in declarations if item["sourceKey"] == normalize_path(D0_SOURCE)]
    if len(target) != 1:
        raise GenerationError(f"DAT must contain exactly one D0 declaration, got {len(target)}")
    declaration = target[0]
    expected = {
        "declaredStart": 30,
        "declaredEnd": 63,
        "declaredCount": 34,
        "width": D0_FRAME_WIDTH,
        "height": D0_FRAME_HEIGHT,
        "columns": D0_COLUMNS,
        "rows": D0_ROWS,
        "count": D0_FRAME_COUNT,
        "firstPic": 30,
        "lastPic": 63,
    }
    for key, expected_value in expected.items():
        if declaration[key] != expected_value:
            raise GenerationError(
                f"D0 DAT declaration mismatch at line {declaration['line']}: "
                f"{key} expected {expected_value}, got {declaration[key]}"
            )
    if normalize_path(declaration["sourcePath"]) != normalize_path(D0_SOURCE):
        raise GenerationError(f"D0 DAT source mismatch: {declaration['sourcePath']}")
    if declaration["declaredCount"] != declaration["count"]:
        raise GenerationError("D0 DAT authored range does not match row*col count")
    if declaration["firstPic"] != declaration["declaredStart"]:
        raise GenerationError(
            f"D0 cumulative nativePic mismatch: firstPic={declaration['firstPic']} "
            f"authoredStart={declaration['declaredStart']}"
        )
    return {
        "catalogFields": catalog_fields,
        "catalogRow": {
            "id": int(catalog_row["id"]),
            "sourcePath": catalog_row["source_path"],
            "type": catalog_row.get("type"),
            "lineNumber": catalog_row.get("line_number"),
        },
        "datPath": root_relative(dat_path),
        "datSha256": sha256_file(dat_path),
        "declaration": declaration,
        "authoredRangeCumulativeMatch": True,
    }


def source_frame(source: np.ndarray, local_pic: int) -> tuple[np.ndarray, tuple[int, int]]:
    x = (local_pic % D0_COLUMNS) * D0_STRIDE_X
    y = (local_pic // D0_COLUMNS) * D0_STRIDE_Y
    crop = source[y : y + D0_FRAME_HEIGHT, x : x + D0_FRAME_WIDTH]
    if crop.shape != (D0_FRAME_HEIGHT, D0_FRAME_WIDTH, 4):
        raise GenerationError(f"D0 frame {local_pic} exceeds source bounds at ({x},{y})")
    return crop.copy(), (x, y)


def build_d0_patch_records(source: np.ndarray) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    if source.ndim != 3 or source.shape[2] != 4:
        raise GenerationError(f"D0 source is not RGBA: {source.shape}")
    base, base_xy = source_frame(source, 0)
    base_sha = sha256_bytes(base.tobytes())
    items: list[dict[str, Any]] = [
        {
            "kind": "base",
            "localPic": 0,
            "width": D0_FRAME_WIDTH,
            "height": D0_FRAME_HEIGHT,
            "pixels": base,
            "nativeCellTopLeft": [int(base_xy[0]), int(base_xy[1])],
            "destinationTopLeft": [0, 0, D0_FRAME_WIDTH, D0_FRAME_HEIGHT],
            "destinationBottomLeft": [0, 0, D0_FRAME_WIDTH, D0_FRAME_HEIGHT],
            "changedPixelsFromBase": 0,
            "patchSha256": None,
        }
    ]
    patch_entries: list[dict[str, Any]] = [None]
    for local_pic in range(1, D0_FRAME_COUNT):
        frame, cell_xy = source_frame(source, local_pic)
        changed = np.any(frame != base, axis=2)
        if not np.any(changed):
            items.append(
                {
                    "kind": "empty",
                    "localPic": local_pic,
                    "width": 0,
                    "height": 0,
                    "pixels": None,
                    "nativeCellTopLeft": [int(cell_xy[0]), int(cell_xy[1])],
                    "destinationTopLeft": None,
                    "destinationBottomLeft": None,
                    "changedPixelsFromBase": 0,
                    "patchSha256": None,
                }
            )
            patch_entries.append(None)
            continue
        ys, xs = np.where(changed)
        x0 = int(xs.min())
        y0 = int(ys.min())
        x1 = int(xs.max()) + 1
        y1 = int(ys.max()) + 1
        patch = frame[y0:y1, x0:x1].copy()
        width = x1 - x0
        height = y1 - y0
        destination_bottom = [x0, D0_FRAME_HEIGHT - y1, width, height]
        items.append(
            {
                "kind": "patch",
                "localPic": local_pic,
                "width": width,
                "height": height,
                "pixels": patch,
                "nativeCellTopLeft": [int(cell_xy[0]), int(cell_xy[1])],
                "destinationTopLeft": [x0, y0, width, height],
                "destinationBottomLeft": destination_bottom,
                "changedPixelsFromBase": int(np.count_nonzero(changed)),
                "patchSha256": sha256_bytes(patch.tobytes()),
            }
        )
        patch_entries.append({"destinationTopLeft": [x0, y0, width, height], "destinationBottomLeft": destination_bottom})
    if len(items) != D0_FRAME_COUNT:
        raise GenerationError(f"D0 item count mismatch: {len(items)}")
    return items, patch_entries


def allocation_for(width: int, height: int) -> tuple[int, int]:
    return align4(width + 2 * EDGE_PADDING), align4(height + 2 * EDGE_PADDING)


def pack_shelf(items: list[dict[str, Any]], width_limit: int) -> dict[str, Any]:
    pages: list[dict[str, Any]] = []
    current: dict[str, Any] | None = None
    x = 1
    y = 1
    shelf_height = 0

    def start_page() -> None:
        nonlocal current, x, y, shelf_height
        current = {"placements": [], "maxRight": 0, "maxBottom": 0}
        x = 1
        y = 1
        shelf_height = 0

    def finish_page() -> None:
        nonlocal current
        if current is None or not current["placements"]:
            return
        page_width = align4(max(4, int(current["maxRight"])))
        page_height = align4(max(4, int(current["maxBottom"])))
        if page_width > PAGE_LIMIT or page_height > PAGE_LIMIT:
            raise GenerationError(f"D0 page exceeds 2048: {page_width}x{page_height}")
        if page_width > width_limit:
            raise GenerationError(f"shelf width limit exceeded after alignment: {page_width}>{width_limit}")
        pages.append({
            "width": page_width,
            "height": page_height,
            "placements": current["placements"],
        })
        current = None

    for item in items:
        width = int(item["width"])
        height = int(item["height"])
        allocation_width, allocation_height = allocation_for(width, height)
        if allocation_width + 1 > PAGE_LIMIT or allocation_height + 1 > PAGE_LIMIT:
            raise GenerationError(
                f"single D0 tile cannot fit page: localPic={item['localPic']} allocation={allocation_width}x{allocation_height}"
            )
        if current is None:
            start_page()
        if x + allocation_width > width_limit:
            y += shelf_height
            x = 1
            shelf_height = 0
        if y + allocation_height > PAGE_LIMIT:
            finish_page()
            start_page()
        if x + allocation_width > width_limit:
            raise GenerationError(
                f"D0 tile cannot fit shelf width {width_limit}: localPic={item['localPic']} allocation={allocation_width}"
            )
        placement = {
            "localPic": int(item["localPic"]),
            "allocationX": int(x),
            "allocationYTop": int(y),
            "allocationWidth": int(allocation_width),
            "allocationHeight": int(allocation_height),
            "x": int(x + EDGE_PADDING),
            "topY": int(y + EDGE_PADDING),
            "width": width,
            "height": height,
        }
        current["placements"].append(placement)
        current["maxRight"] = max(int(current["maxRight"]), x + allocation_width)
        current["maxBottom"] = max(int(current["maxBottom"]), y + allocation_height)
        x += allocation_width
        shelf_height = max(shelf_height, allocation_height)
    finish_page()
    return {
        "widthLimit": int(width_limit),
        "pages": pages,
        "pageCount": len(pages),
        "totalArea": int(sum(int(item["width"]) * int(item["height"]) for item in pages)),
    }


def choose_shelf(items: list[dict[str, Any]]) -> dict[str, Any]:
    choices: list[dict[str, Any]] = []
    for width_limit in range(MIN_PAGE_WIDTH, MAX_PAGE_WIDTH + 1):
        try:
            layout = pack_shelf(items, width_limit)
        except GenerationError:
            continue
        if not layout["pages"]:
            continue
        if any(
            int(page["width"]) < MIN_PAGE_WIDTH
            or int(page["width"]) > MAX_PAGE_WIDTH
            or int(page["height"]) > PAGE_LIMIT
            for page in layout["pages"]
        ):
            continue
        choices.append(layout)
    if not choices:
        raise GenerationError("no D0 shelf layout fits the requested 512..2048 page-width range")
    one_page = [choice for choice in choices if int(choice["pageCount"]) == 1]
    if not one_page:
        raise GenerationError("one-page D0 layout is infeasible within 2048; no architecture change was attempted")
    return min(one_page, key=lambda item: (int(item["totalArea"]), int(item["widthLimit"])))


def render_page(layout: dict[str, Any], items_by_local: dict[int, dict[str, Any]]) -> tuple[np.ndarray, dict[int, dict[str, Any]]]:
    page = np.zeros((int(layout["height"]), int(layout["width"]), 4), dtype=np.uint8)
    placements: dict[int, dict[str, Any]] = {}
    for placement in layout["placements"]:
        local_pic = int(placement["localPic"])
        item = items_by_local[local_pic]
        crop = item["pixels"]
        if crop is None:
            raise GenerationError(f"empty D0 frame was included in physical page: localPic={local_pic}")
        top_y = int(placement["topY"])
        x = int(placement["x"])
        width = int(placement["width"])
        height = int(placement["height"])
        if crop.shape != (height, width, 4):
            raise GenerationError(f"D0 crop shape mismatch at localPic={local_pic}: {crop.shape}")
        page[top_y : top_y + height, x : x + width] = crop
        page[top_y : top_y + height, x - 1] = crop[:, 0]
        page[top_y : top_y + height, x + width] = crop[:, -1]
        page[top_y - 1, x : x + width] = crop[0, :]
        page[top_y + height, x : x + width] = crop[-1, :]
        page[top_y - 1, x - 1] = crop[0, 0]
        page[top_y - 1, x + width] = crop[0, -1]
        page[top_y + height, x - 1] = crop[-1, 0]
        page[top_y + height, x + width] = crop[-1, -1]
        placements[local_pic] = placement
    return page, placements


def rect_from_placement(page_height: int, placement: dict[str, Any]) -> dict[str, int]:
    return {
        "x": int(placement["x"]),
        "y": int(page_height - int(placement["topY"]) - int(placement["height"])),
        "width": int(placement["width"]),
        "height": int(placement["height"]),
    }


def page_crop(page: np.ndarray, rect: dict[str, Any]) -> np.ndarray:
    x = int(rect["x"])
    y = int(rect["y"])
    width = int(rect["width"])
    height = int(rect["height"])
    top_y = page.shape[0] - y - height
    if x < 0 or y < 0 or width <= 0 or height <= 0 or x + width > page.shape[1] or top_y < 0 or top_y + height > page.shape[0]:
        raise GenerationError(f"rect exceeds page: {rect} page={page.shape}")
    return page[top_y : top_y + height, x : x + width]


def rects_overlap(first: tuple[int, int, int, int], second: tuple[int, int, int, int]) -> bool:
    return not (
        first[0] + first[2] <= second[0]
        or second[0] + second[2] <= first[0]
        or first[1] + first[3] <= second[1]
        or second[1] + second[3] <= first[1]
    )


def validate_new_page(
    page: np.ndarray,
    layout: dict[str, Any],
    items_by_local: dict[int, dict[str, Any]],
    placements_by_local: dict[int, dict[str, Any]],
) -> dict[str, Any]:
    if page.ndim != 3 or page.shape[2] != 4:
        raise GenerationError(f"new page is not RGBA: {page.shape}")
    allocation_rects: list[tuple[int, int, int, int]] = []
    frame_checks: list[dict[str, Any]] = []
    for placement in layout["pages"][0]["placements"]:
        local_pic = int(placement["localPic"])
        item = items_by_local[local_pic]
        x = int(placement["x"])
        top_y = int(placement["topY"])
        width = int(placement["width"])
        height = int(placement["height"])
        crop = item["pixels"]
        if crop is None:
            raise GenerationError(f"new page contains an empty patch tile: localPic={local_pic}")
        if not np.array_equal(page[top_y : top_y + height, x : x + width], crop):
            raise GenerationError(f"new page crop mismatch: localPic={local_pic}")
        if not np.array_equal(page[top_y : top_y + height, x - 1], crop[:, 0]):
            raise GenerationError(f"left edge extrusion mismatch: localPic={local_pic}")
        if not np.array_equal(page[top_y : top_y + height, x + width], crop[:, -1]):
            raise GenerationError(f"right edge extrusion mismatch: localPic={local_pic}")
        if not np.array_equal(page[top_y - 1, x : x + width], crop[0, :]):
            raise GenerationError(f"top edge extrusion mismatch: localPic={local_pic}")
        if not np.array_equal(page[top_y + height, x : x + width], crop[-1, :]):
            raise GenerationError(f"bottom edge extrusion mismatch: localPic={local_pic}")
        corners = (
            (x - 1, top_y - 1, crop[0, 0]),
            (x + width, top_y - 1, crop[0, -1]),
            (x - 1, top_y + height, crop[-1, 0]),
            (x + width, top_y + height, crop[-1, -1]),
        )
        for corner_x, corner_y, expected in corners:
            if not np.array_equal(page[corner_y, corner_x], expected):
                raise GenerationError(f"corner extrusion mismatch: localPic={local_pic}")
        allocation = (
            int(placement["allocationX"]),
            int(placement["allocationYTop"]),
            int(placement["allocationWidth"]),
            int(placement["allocationHeight"]),
        )
        if allocation[2] != align4(width + 2) or allocation[3] != align4(height + 2):
            raise GenerationError(f"allocation alignment mismatch: localPic={local_pic} allocation={allocation}")
        if allocation[0] < 0 or allocation[1] < 0 or allocation[0] + allocation[2] > page.shape[1] or allocation[1] + allocation[3] > page.shape[0]:
            raise GenerationError(f"allocation exceeds new page: localPic={local_pic} allocation={allocation}")
        if any(rects_overlap(allocation, prior) for prior in allocation_rects):
            raise GenerationError(f"allocation rectangles overlap at localPic={local_pic}")
        allocation_rects.append(allocation)
        frame_checks.append(
            {
                "localPic": local_pic,
                "rect": rect_from_placement(int(page.shape[0]), placement),
                "allocation": {
                    "x": allocation[0],
                    "topY": allocation[1],
                    "width": allocation[2],
                    "height": allocation[3],
                },
                "edgeExtrusionExact": True,
                "allocationAligned4": allocation[2] % 4 == 0 and allocation[3] % 4 == 0,
            }
        )
    occupied = np.zeros(page.shape[:2], dtype=bool)
    for x, top_y, width, height in allocation_rects:
        occupied[top_y : top_y + height, x : x + width] = True
    unused = page[~occupied]
    unused_zero = bool(unused.size == 0 or np.all(unused == 0))
    if not unused_zero:
        raise GenerationError("unused new-page pixels are not transparent RGBA zero")
    if page.shape[1] > PAGE_LIMIT or page.shape[0] > PAGE_LIMIT:
        raise GenerationError("new page exceeds 2048 bound")
    if page.shape[1] % ALLOCATION_ALIGNMENT or page.shape[0] % ALLOCATION_ALIGNMENT:
        raise GenerationError("new page dimensions are not allocation aligned")
    return {
        "width": int(page.shape[1]),
        "height": int(page.shape[0]),
        "placements": len(frame_checks),
        "allocationCount": len(allocation_rects),
        "allAllocationsNonOverlapping": True,
        "allAllocationsAligned4": all(item["allocationAligned4"] for item in frame_checks),
        "edgeExtrusionExact": all(item["edgeExtrusionExact"] for item in frame_checks),
        "unusedPagePixels": int(unused.shape[0]),
        "unusedPagePixelsAreRgbaZero": unused_zero,
        "frames": frame_checks,
    }


def snapshot_inputs(original_manifest: dict[str, Any]) -> dict[str, Any]:
    paths: list[Path] = [CURRENT_MANIFEST_PATH, CATALOG_PATH, source_absolute(D0_SOURCE), dat_absolute(D0_DAT)]
    for source in original_manifest["sources"]:
        source_path = source_absolute(str(source["path"]))
        if source_path not in paths:
            paths.append(source_path)
        for binding in source.get("bindings", []):
            dat_path = dat_absolute(str(binding["datPath"]))
            if dat_path not in paths:
                paths.append(dat_path)
    for page in original_manifest["pages"]:
        page_path = DEPLOYED_ROOT / Path(*str(page["path"]).replace("\\", "/").split("/"))
        if page_path not in paths:
            paths.append(page_path)
    files: list[dict[str, Any]] = []
    for path in paths:
        if not path.is_file():
            raise GenerationError(f"required input missing: {path}")
        files.append(file_metadata(path, include_dimensions=path.suffix.lower() == ".png"))
    files.sort(key=lambda item: item["path"])
    return {"capturedAtUtc": utc_now(), "files": files}


def compare_input_snapshots(before: dict[str, Any], after: dict[str, Any]) -> dict[str, Any]:
    before_map = {item["path"]: item for item in before["files"]}
    after_map = {item["path"]: item for item in after["files"]}
    unchanged = before_map == after_map
    if not unchanged:
        raise GenerationError("source/DAT/catalog/current-manifest/old-page input hash changed during generation")
    return {"unchanged": True, "fileCount": len(before_map)}


def load_current_manifest() -> dict[str, Any]:
    if not CURRENT_MANIFEST_PATH.is_file():
        raise GenerationError(f"current derived manifest missing: {CURRENT_MANIFEST_PATH}")
    try:
        manifest = json.loads(CURRENT_MANIFEST_PATH.read_text(encoding="utf-8"))
    except Exception as error:
        raise GenerationError(f"cannot parse current manifest: {error}") from error
    if not isinstance(manifest, dict) or not isinstance(manifest.get("pages"), list) or not isinstance(manifest.get("sources"), list):
        raise GenerationError("current manifest has invalid pages/sources arrays")
    if len(manifest["pages"]) != 18 or len(manifest["sources"]) != 4:
        raise GenerationError(
            f"current manifest baseline changed: expected 18 pages/4 sources, got {len(manifest['pages'])}/{len(manifest['sources'])}"
        )
    return manifest


def copy_create(source: Path, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    with source.open("rb") as src, destination.open("xb") as dst:
        for chunk in iter(lambda: src.read(1024 * 1024), b""):
            dst.write(chunk)


def validate_old_pages_and_manifest(
    original_manifest: dict[str, Any],
    candidate_manifest: dict[str, Any],
    output_root: Path,
) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    old_pages = original_manifest["pages"]
    old_sources = original_manifest["sources"]
    if candidate_manifest["pages"][: len(old_pages)] != old_pages:
        raise GenerationError("candidate changed one or more old page descriptors")
    if candidate_manifest["sources"][: len(old_sources)] != old_sources:
        raise GenerationError("candidate changed one or more old source descriptors")
    if len(candidate_manifest["pages"]) != 19 or len(candidate_manifest["sources"]) != 5:
        raise GenerationError("candidate does not contain old 18 pages/4 sources plus one D0 page/source")
    checks: list[dict[str, Any]] = []
    for page in old_pages:
        current_path = DEPLOYED_ROOT / Path(*str(page["path"]).split("/"))
        candidate_path = output_root / Path(*str(page["path"]).split("/"))
        current_bytes = current_path.read_bytes()
        candidate_bytes = candidate_path.read_bytes()
        current_sha = sha256_bytes(current_bytes)
        candidate_sha = sha256_bytes(candidate_bytes)
        if current_sha != str(page["sha256"]).lower() or candidate_sha != current_sha or candidate_bytes != current_bytes:
            raise GenerationError(f"old page changed: {page['id']}")
        header = png_header(candidate_bytes)
        if [header["width"], header["height"]] != [int(page["width"]), int(page["height"])]:
            raise GenerationError(f"old page size mismatch: {page['id']}")
        checks.append(
            {
                "id": page["id"],
                "path": page["path"],
                "descriptorExact": True,
                "bytesExact": True,
                "sha256": current_sha,
                "bytes": len(current_bytes),
                "width": int(header["width"]),
                "height": int(header["height"]),
                "mode": header["mode"],
            }
        )
    return checks, {
        "sourceDescriptorCount": len(old_sources),
        "pageDescriptorCount": len(old_pages),
        "oldSourceDescriptorsExact": True,
        "oldPageDescriptorsExact": True,
        "oldPagesBytesExact": True,
    }


def frame_map(source: dict[str, Any]) -> dict[int, tuple[dict[str, Any], int]]:
    result: dict[int, tuple[dict[str, Any], int]] = {}
    for chunk in source["chunks"]:
        first = int(chunk["firstLocalPic"])
        count = int(chunk["count"])
        rects = chunk["rects"]
        if len(rects) != count:
            raise GenerationError(f"chunk rect count mismatch: {source['path']} first={first}")
        for offset in range(count):
            local_pic = first + offset
            if local_pic in result:
                raise GenerationError(f"duplicate old frame mapping: {source['path']} localPic={local_pic}")
            result[local_pic] = (chunk, offset)
    capacity = int(source["columns"]) * int(source["rows"])
    if set(result) != set(range(capacity)):
        raise GenerationError(f"old source does not cover all local frames: {source['path']}")
    return result


def reconstruct_full_canvas(frame_width: int, frame_height: int, crop: np.ndarray, content: dict[str, Any]) -> np.ndarray:
    x = int(content["x"])
    y = int(content["y"])
    width = int(content["width"])
    height = int(content["height"])
    if x < 0 or y < 0 or width <= 0 or height <= 0 or x + width > frame_width or y + height > frame_height:
        raise GenerationError(f"content rect exceeds native canvas: {content} canvas={frame_width}x{frame_height}")
    if crop.shape != (height, width, 4):
        raise GenerationError(f"content/page crop shape mismatch: {crop.shape} versus {content}")
    output = np.zeros((frame_height, frame_width, 4), dtype=np.uint8)
    top_y = frame_height - y - height
    output[top_y : top_y + height, x : x + width] = crop
    return output


def validate_original_88(
    original_manifest: dict[str, Any],
    candidate_manifest: dict[str, Any],
    output_root: Path,
) -> dict[str, Any]:
    page_arrays: dict[str, np.ndarray] = {}
    for page in candidate_manifest["pages"]:
        path = output_root / Path(*str(page["path"]).split("/"))
        array, header = read_rgba(path)
        if [int(array.shape[1]), int(array.shape[0])] != [int(page["width"]), int(page["height"])]:
            raise GenerationError(f"candidate page dimension mismatch: {page['id']}")
        if sha256_file(path).lower() != str(page["sha256"]).lower():
            raise GenerationError(f"candidate page hash mismatch: {page['id']}")
        page_arrays[str(page["id"])] = array
    source_results: list[dict[str, Any]] = []
    total_frames = 0
    exact_frames = 0
    for source in original_manifest["sources"]:
        source_path = source_absolute(str(source["path"]))
        source_array, _ = read_rgba(source_path)
        if sha256_file(source_path).lower() != str(source["sha256"]).lower():
            raise GenerationError(f"old source hash mismatch: {source['path']}")
        mappings = frame_map(source)
        frame_results: list[dict[str, Any]] = []
        for local_pic in range(int(source["columns"]) * int(source["rows"])):
            chunk, offset = mappings[local_pic]
            page_id = str(chunk["page"])
            rect = chunk["rects"][offset]
            if page_id not in page_arrays:
                raise GenerationError(f"old frame page missing from candidate: {page_id}")
            actual_crop = page_crop(page_arrays[page_id], rect)
            content_rects = chunk.get("contentRects")
            if content_rects is None:
                content = {"x": 0, "y": 0, "width": int(source["frameWidth"]), "height": int(source["frameHeight"])}
            else:
                if len(content_rects) != int(chunk["count"]):
                    raise GenerationError(f"old content rect count mismatch: {source['path']} first={chunk['firstLocalPic']}")
                content = content_rects[offset]
            reconstructed = reconstruct_full_canvas(int(source["frameWidth"]), int(source["frameHeight"]), actual_crop, content)
            source_x = (local_pic % int(source["columns"])) * (int(source["frameWidth"]) + 1)
            source_y = (local_pic // int(source["columns"])) * (int(source["frameHeight"]) + 1)
            expected = source_array[source_y : source_y + int(source["frameHeight"]), source_x : source_x + int(source["frameWidth"])]
            if expected.shape != reconstructed.shape:
                raise GenerationError(f"old source frame exceeds source bounds: {source['path']} localPic={local_pic}")
            expected_sha = sha256_bytes(expected.tobytes())
            actual_sha = sha256_bytes(reconstructed.tobytes())
            exact = bool(np.array_equal(reconstructed, expected))
            if not exact:
                raise GenerationError(f"old 88-frame RGBA reconstruction mismatch: {source['path']} localPic={local_pic}")
            frame_results.append(
                {
                    "localPic": local_pic,
                    "nativePic": int(source["bindings"][0]["firstPic"]) + local_pic,
                    "sourceTopLeft": [int(source_x), int(source_y)],
                    "nativeStride": [int(source["frameWidth"]) + 1, int(source["frameHeight"]) + 1],
                    "page": page_id,
                    "rect": copy.deepcopy(rect),
                    "contentRect": copy.deepcopy(content),
                    "expectedRgbaSha256": expected_sha,
                    "reconstructedRgbaSha256": actual_sha,
                    "fullRgbaExact": True,
                }
            )
            total_frames += 1
            exact_frames += 1
        source_results.append(
            {
                "source": source["path"],
                "nativeFrames": len(frame_results),
                "verifiedFrames": len(frame_results),
                "fullRgbaReconstructionExact": True,
                "frames": frame_results,
            }
        )
    if total_frames != 88 or exact_frames != 88:
        raise GenerationError(f"old mapping count mismatch: total={total_frames} exact={exact_frames}")
    return {
        "sourceCount": len(source_results),
        "nativeLogicalFrames": total_frames,
        "verifiedFrames": exact_frames,
        "fullRgbaReconstructionExact": True,
        "sources": source_results,
    }


def build_candidate_manifest(
    original_manifest: dict[str, Any],
    d0_source_sha: str,
    d0_dat_meta: dict[str, Any],
    page_id: str,
    base_rect: dict[str, int],
    patch_entries: list[dict[str, Any] | None],
    d0_page_descriptor: dict[str, Any],
) -> dict[str, Any]:
    if len(patch_entries) != D0_FRAME_COUNT:
        raise GenerationError(f"replacementPatches count mismatch: {len(patch_entries)}")
    d0_binding = {
        "oid": D0_OID,
        "datPath": D0_DAT,
        "datSha256": d0_dat_meta["sha256"],
        "firstPic": 30,
        "lastPic": 63,
    }
    rects = [copy.deepcopy(base_rect) for _ in range(D0_FRAME_COUNT)]
    d0_source = {
        "path": D0_SOURCE,
        "sha256": d0_source_sha,
        "width": 621,
        "height": 2142,
        "frameWidth": D0_FRAME_WIDTH,
        "frameHeight": D0_FRAME_HEIGHT,
        "columns": D0_COLUMNS,
        "rows": D0_ROWS,
        "bindings": [d0_binding],
        "chunks": [
            {
                "page": page_id,
                "firstLocalPic": 0,
                "count": D0_FRAME_COUNT,
                "columns": D0_FRAME_COUNT,
                "rows": 1,
                "rects": rects,
                "allowExactFrameAliases": True,
                "replacementPatches": copy.deepcopy(patch_entries),
            }
        ],
    }
    candidate = {
        "version": original_manifest["version"],
        "generatorVersion": original_manifest["generatorVersion"],
        "pages": copy.deepcopy(original_manifest["pages"]) + [copy.deepcopy(d0_page_descriptor)],
        "sources": copy.deepcopy(original_manifest["sources"]) + [d0_source],
    }
    return candidate


def validate_d0_reconstruction(
    candidate_manifest: dict[str, Any],
    output_root: Path,
    source: np.ndarray,
    items_by_local: dict[int, dict[str, Any]],
    new_page_array: np.ndarray,
    new_page_id: str,
) -> dict[str, Any]:
    d0_source = next(item for item in candidate_manifest["sources"] if item["path"] == D0_SOURCE)
    chunk = d0_source["chunks"][0]
    replacement_patches = chunk.get("replacementPatches")
    if len(replacement_patches) != D0_FRAME_COUNT:
        raise GenerationError("candidate D0 replacementPatches does not contain 34 entries")
    if chunk["firstLocalPic"] != 0 or chunk["count"] != 34 or chunk["columns"] != 34 or chunk["rows"] != 1:
        raise GenerationError("candidate D0 chunk geometry is not 34x1")
    if len(chunk["rects"]) != 34 or any(rect != chunk["rects"][0] for rect in chunk["rects"]):
        raise GenerationError("candidate D0 chunk rects do not all alias the base rect")
    if chunk.get("allowExactFrameAliases") is not True or "contentRects" in chunk:
        raise GenerationError("candidate D0 chunk alias/default-fullcanvas contract is missing")
    base_rect = chunk["rects"][0]
    if [base_rect["width"], base_rect["height"]] != [D0_FRAME_WIDTH, D0_FRAME_HEIGHT]:
        raise GenerationError("candidate D0 base rect size mismatch")
    base = page_crop(new_page_array, base_rect)
    original_base, _ = source_frame(source, 0)
    if not np.array_equal(base, original_base):
        raise GenerationError("candidate D0 base rect does not reproduce localPic 0")
    frame_results: list[dict[str, Any]] = []
    changed_pixels_total = 0
    max_changed_pixels = 0
    pixels_compared = 0
    for local_pic in range(D0_FRAME_COUNT):
        expected, native_xy = source_frame(source, local_pic)
        patch_binding = replacement_patches[local_pic]
        reconstructed = base.copy()
        if patch_binding is None:
            patch_sha = None
            patch_rect = None
            destination = None
            patch_width = 0
            patch_height = 0
        else:
            source_rect = patch_binding["source"]
            destination = patch_binding["destination"]
            if [int(source_rect["width"]), int(source_rect["height"])] != [int(destination["width"]), int(destination["height"])] or int(source_rect["width"]) <= 0 or int(source_rect["height"]) <= 0:
                raise GenerationError(f"D0 patch source/destination size mismatch: localPic={local_pic}")
            patch = page_crop(new_page_array, source_rect)
            patch_rect = copy.deepcopy(source_rect)
            patch_sha = sha256_bytes(patch.tobytes())
            patch_width = int(source_rect["width"])
            patch_height = int(source_rect["height"])
            top_y = D0_FRAME_HEIGHT - int(destination["y"]) - patch_height
            x = int(destination["x"])
            if x < 0 or top_y < 0 or x + patch_width > D0_FRAME_WIDTH or top_y + patch_height > D0_FRAME_HEIGHT:
                raise GenerationError(f"D0 patch destination exceeds canvas: localPic={local_pic}")
            reconstructed[top_y : top_y + patch_height, x : x + patch_width] = patch
            frame_bbox = items_by_local[local_pic]["destinationBottomLeft"]
            if [int(destination[k]) for k in ("x", "y", "width", "height")] != [int(value) for value in frame_bbox]:
                raise GenerationError(f"D0 patch destination bbox changed: localPic={local_pic}")
        exact = bool(np.array_equal(reconstructed, expected))
        if not exact:
            raise GenerationError(f"D0 full RGBA reconstruction mismatch: localPic={local_pic}")
        expected_diff = np.any(expected != base, axis=2)
        changed_pixels = int(np.count_nonzero(expected_diff))
        changed_pixels_total += changed_pixels
        max_changed_pixels = max(max_changed_pixels, changed_pixels)
        pixels_compared += int(expected.size)
        frame_results.append(
            {
                "localPic": local_pic,
                "nativePic": 30 + local_pic,
                "nativeCellTopLeft": [int(native_xy[0]), int(native_xy[1])],
                "nativeStride": [D0_STRIDE_X, D0_STRIDE_Y],
                "baseRgbaSha256": sha256_bytes(base.tobytes()),
                "expectedRgbaSha256": sha256_bytes(expected.tobytes()),
                "reconstructedRgbaSha256": sha256_bytes(reconstructed.tobytes()),
                "changedPixelsFromBase": changed_pixels,
                "patch": None
                if patch_binding is None
                else {
                    "source": patch_rect,
                    "destination": copy.deepcopy(destination),
                    "rgbaSha256": patch_sha,
                    "size": [patch_width, patch_height],
                },
                "fullRgbaExact": True,
            }
        )
    if len(frame_results) != 34 or pixels_compared != D0_FRAME_COUNT * D0_FRAME_WIDTH * D0_FRAME_HEIGHT * 4:
        raise GenerationError("D0 reconstruction verification count mismatch")
    return {
        "source": D0_SOURCE,
        "page": new_page_id,
        "nativeFrames": D0_FRAME_COUNT,
        "verifiedFrames": len(frame_results),
        "pixelsCompared": pixels_compared,
        "changedPixelsFromBaseTotal": changed_pixels_total,
        "maxChangedPixelsFromBase": max_changed_pixels,
        "fullRgbaReconstructionExact": True,
        "frames": frame_results,
    }


def report_text(
    output_root: Path,
    input_hashes: dict[str, Any],
    identity: dict[str, Any],
    layout: dict[str, Any],
    d0_reconstruction: dict[str, Any],
    old_reconstruction: dict[str, Any],
    new_page_validation: dict[str, Any],
    old_page_summary: dict[str, Any],
    candidate_manifest: dict[str, Any],
    new_page: dict[str, Any],
) -> str:
    page = layout["pages"][0]
    nonempty = sum(1 for item in d0_reconstruction["frames"] if item["patch"] is not None)
    null_count = d0_reconstruction["nativeFrames"] - nonempty
    input_unchanged = bool(input_hashes["comparison"]["unchanged"])
    lines = [
        "# NTSD M12 D0 base-plus-RGBA-patch staging candidate",
        "",
        f"- Task: `{TASK_ID}`",
        f"- Change: `{CHANGE_ID}`",
        f"- Operation: `{OPERATION_ID}`",
        "- Status: `PASS_STAGING_ONLY`",
        f"- Output root: `{root_relative(output_root)}`",
        "",
        "## Input identity",
        "",
        f"- Source: `{D0_SOURCE}`; SHA-256 `{identity['sourceSha256']}`; PNG mode `{identity['sourceMode']}`, decoded losslessly as RGBA `{identity['sourceSize'][0]}x{identity['sourceSize'][1]}`.",
        f"- DAT: `{D0_DAT}`; SHA-256 `{identity['datSha256']}`; catalog OID `{D0_OID}`.",
        f"- DAT authored declaration: `file(30-63)` at line `{identity['datDeclaration']['line']}`, `{D0_COLUMNS}x{D0_ROWS}` cells, `{D0_FRAME_WIDTH}x{D0_FRAME_HEIGHT}`, cumulative nativePic `{identity['datDeclaration']['firstPic']}..{identity['datDeclaration']['lastPic']}`.",
        f"- Native extraction: `{D0_FRAME_COUNT}` frames, stride `{D0_STRIDE_X}x{D0_STRIDE_Y}`.",
        f"- Input hash stability after generation: `{input_unchanged}` across `{input_hashes['comparison']['fileCount']}` source/DAT/catalog/manifest/old-page files.",
        "",
        "## Page layout",
        "",
        f"- Base frame: localPic `0`, full RGBA `{D0_FRAME_WIDTH}x{D0_FRAME_HEIGHT}`.",
        f"- Non-empty replacement patches: `{nonempty}`; null patches: `{null_count}`; replacement semantics: exact RGBA byte replacement including alpha=0.",
        f"- Shelf candidates: width limits `512..2048`; selected one-page layout `{page['width']}x{page['height']}` with width limit `{layout['widthLimit']}` and area `{layout['totalArea']}` pixels.",
        f"- Page: `{new_page['id']}`; PNG `{new_page['width']}x{new_page['height']}`; PNG bytes `{new_page['bytes']}`; SHA-256 `{new_page['sha256']}`.",
        f"- Padding/allocation: edge padding `{EDGE_PADDING}`, extrusion exact `{new_page_validation['edgeExtrusionExact']}`, non-overlap `{new_page_validation['allAllocationsNonOverlapping']}`, allocation alignment `{new_page_validation['allAllocationsAligned4']}`, unused pixels RGBA-zero `{new_page_validation['unusedPagePixelsAreRgbaZero']}`, resize `False`, rotate `False`.",
        "",
        "## Reconstruction",
        "",
        f"- D0 full RGBA: `{d0_reconstruction['verifiedFrames']}/{d0_reconstruction['nativeFrames']}` exact; pixels compared `{d0_reconstruction['pixelsCompared']}`; changed pixels from base `{d0_reconstruction['changedPixelsFromBaseTotal']}`; max per-frame changed pixels `{d0_reconstruction['maxChangedPixelsFromBase']}`.",
        f"- Retained mapping: `{old_reconstruction['verifiedFrames']}/{old_reconstruction['nativeLogicalFrames']}` exact across `{old_reconstruction['sourceCount']}` existing sources.",
        f"- Combined exact frame evidence: `{d0_reconstruction['verifiedFrames'] + old_reconstruction['verifiedFrames']}/122`.",
        f"- Existing manifest/page preservation: `{old_page_summary['oldSourceDescriptorsExact']}` source descriptors exact, `{old_page_summary['oldPageDescriptorsExact']}` page descriptors exact, `{old_page_summary['oldPagesBytesExact']}` old PNG bytes exact.",
        "",
        "## Scope boundary",
        "",
        "This is a create-new offline candidate only. Original PNG/DAT/current manifest and old 18 pages were read and hash-checked; no Assets, shader, runtime, tests, Unity, Play, P1, ASTC, device, build, deletion, or replacement was performed by this helper.",
        "",
        f"Candidate manifest contains `{len(candidate_manifest['pages'])}` page descriptors and `{len(candidate_manifest['sources'])}` source descriptors; the first 18/4 entries remain exact copies of the current manifest.",
        "",
    ]
    return "\n".join(lines)


def build_failure_report(output_root: Path, error: BaseException) -> str:
    return "\n".join(
        [
            f"# {TASK_ID} staging failure",
            "",
            f"- Status: `FAIL_STAGING_ONLY`",
            f"- Output root retained: `{root_relative(output_root)}`",
            f"- Error: `{type(error).__name__}: {error}`",
            "- No Unity/P1/ASTC/build/device stage was run.",
            "",
        ]
    )


def generate(output_root: Path) -> dict[str, Any]:
    if output_root.exists():
        raise GenerationError(f"create-new output root already exists: {output_root}")
    output_root.mkdir(parents=True, exist_ok=False)
    original_manifest = load_current_manifest()
    before_inputs = snapshot_inputs(original_manifest)
    identity = verify_d0_identity()
    d0_path = source_absolute(D0_SOURCE)
    dat_path = dat_absolute(D0_DAT)
    source_array, source_image_meta = read_rgba(d0_path)
    if source_image_meta["width"] != 621 or source_image_meta["height"] != 2142:
        raise GenerationError(f"D0 source size mismatch: {source_image_meta}")
    identity.update(
        {
            "sourceSha256": sha256_file(d0_path),
            "datSha256": sha256_file(dat_path),
            "sourceSize": [int(source_array.shape[1]), int(source_array.shape[0])],
            "sourceMode": source_image_meta["mode"],
            "decodedMode": "RGBA",
            "datBytes": dat_path.stat().st_size,
            "datDeclaration": identity["declaration"],
        }
    )
    items, patch_entries_base = build_d0_patch_records(source_array)
    items_by_local = {int(item["localPic"]): item for item in items}
    layout = choose_shelf(items)
    if int(layout["pageCount"]) != 1:
        raise GenerationError("selected layout is not one page; no architecture change was attempted")
    layout_page = layout["pages"][0]
    new_page_id = f"page-{identity['sourceSha256'][:16]}-{NEW_PAGE_TAG}-{int(layout_page['width'])}x{int(layout_page['height'])}"
    old_page_ids = {str(item["id"]) for item in original_manifest["pages"]}
    if new_page_id in old_page_ids:
        raise GenerationError(f"new page id collides with current manifest: {new_page_id}")
    new_page_rel = f"pages/{new_page_id}.png"
    new_page_array, placements_by_local = render_page(layout_page, items_by_local)
    base_rect = rect_from_placement(int(layout_page["height"]), placements_by_local[0])
    replacement_patches: list[dict[str, Any] | None] = [None for _ in range(D0_FRAME_COUNT)]
    for local_pic in range(1, D0_FRAME_COUNT):
        item = items_by_local[local_pic]
        if item["kind"] == "empty":
            replacement_patches[local_pic] = None
            continue
        placement = placements_by_local.get(local_pic)
        if placement is None:
            raise GenerationError(f"non-empty D0 patch not placed: localPic={local_pic}")
        replacement_patches[local_pic] = {
            "source": rect_from_placement(int(layout_page["height"]), placement),
            "destination": {
                "x": int(item["destinationBottomLeft"][0]),
                "y": int(item["destinationBottomLeft"][1]),
                "width": int(item["destinationBottomLeft"][2]),
                "height": int(item["destinationBottomLeft"][3]),
            },
        }
    if replacement_patches != patch_entries_base:
        for index, (actual, expected) in enumerate(zip(replacement_patches, patch_entries_base)):
            if actual is None and expected is None:
                continue
            if actual is None or expected is None or actual["destination"] != {
                "x": int(expected["destinationBottomLeft"][0]),
                "y": int(expected["destinationBottomLeft"][1]),
                "width": int(expected["destinationBottomLeft"][2]),
                "height": int(expected["destinationBottomLeft"][3]),
            }:
                raise GenerationError(f"D0 replacement patch pre-layout mismatch at localPic={index}")
    page_payload = png_bytes(new_page_array)
    page_descriptor = {
        "id": new_page_id,
        "path": new_page_rel,
        "sha256": sha256_bytes(page_payload),
        "width": int(new_page_array.shape[1]),
        "height": int(new_page_array.shape[0]),
    }
    d0_source_meta = file_metadata(d0_path, include_dimensions=True)
    d0_dat_meta = file_metadata(dat_path)
    candidate_manifest = build_candidate_manifest(
        original_manifest,
        d0_source_meta["sha256"],
        d0_dat_meta,
        new_page_id,
        base_rect,
        replacement_patches,
        page_descriptor,
    )
    if candidate_manifest["sources"][-1]["width"] != int(source_array.shape[1]) or candidate_manifest["sources"][-1]["height"] != int(source_array.shape[0]):
        raise GenerationError("candidate D0 source dimensions do not match current PNG")

    for page in original_manifest["pages"]:
        source_page = DEPLOYED_ROOT / Path(*str(page["path"]).split("/"))
        destination_page = output_root / Path(*str(page["path"]).split("/"))
        copy_create(source_page, destination_page)
    new_page_path = output_root / Path(*new_page_rel.split("/"))
    write_new_bytes(new_page_path, page_payload)
    manifest_path = output_root / "manifest.json"
    write_new_json(manifest_path, candidate_manifest)

    parsed_candidate = json.loads(manifest_path.read_text(encoding="utf-8"))
    if parsed_candidate != candidate_manifest:
        raise GenerationError("candidate manifest readback differs from generated object")
    old_page_checks, old_page_summary = validate_old_pages_and_manifest(original_manifest, candidate_manifest, output_root)
    new_page_array_readback, new_page_meta = read_rgba(new_page_path)
    if not np.array_equal(new_page_array_readback, new_page_array):
        raise GenerationError("new page RGBA readback differs from generated pixels")
    if sha256_file(new_page_path) != page_descriptor["sha256"]:
        raise GenerationError("new page SHA differs from candidate descriptor")
    new_page_validation = validate_new_page(new_page_array_readback, layout, items_by_local, placements_by_local)
    d0_reconstruction = validate_d0_reconstruction(
        candidate_manifest,
        output_root,
        source_array,
        items_by_local,
        new_page_array_readback,
        new_page_id,
    )
    old_reconstruction = validate_original_88(original_manifest, candidate_manifest, output_root)
    after_inputs = snapshot_inputs(original_manifest)
    input_comparison = compare_input_snapshots(before_inputs, after_inputs)
    input_hashes = {
        "schemaVersion": 1,
        "task": TASK_ID,
        "change": CHANGE_ID,
        "operation": OPERATION_ID,
        "before": before_inputs,
        "after": after_inputs,
        "comparison": input_comparison,
    }
    write_new_json(output_root / "input-hashes.json", input_hashes)
    validation = {
        "schemaVersion": 1,
        "status": "PASS_STAGING_ONLY",
        "task": TASK_ID,
        "change": CHANGE_ID,
        "operation": OPERATION_ID,
        "sourceIdentity": identity,
        "candidateManifest": {
            "path": "manifest.json",
            "sha256": sha256_file(manifest_path),
            "bytes": manifest_path.stat().st_size,
            "pageCount": len(candidate_manifest["pages"]),
            "sourceCount": len(candidate_manifest["sources"]),
            "oldDescriptorsExact": True,
        },
        "newPage": {
            **page_descriptor,
            "bytes": new_page_path.stat().st_size,
            "mode": new_page_meta["mode"],
            "rgbaBytes": int(page_descriptor["width"]) * int(page_descriptor["height"]) * 4,
            "validation": new_page_validation,
        },
        "packingContract": {
            "resize": False,
            "rotate": False,
            "edgePadding": EDGE_PADDING,
            "allocationAlignment": ALLOCATION_ALIGNMENT,
        },
        "oldPages": old_page_checks,
        "oldManifestPreservation": old_page_summary,
        "d0": d0_reconstruction,
        "originalMapping": old_reconstruction,
        "combinedReconstruction": {
            "d0Frames": d0_reconstruction["verifiedFrames"],
            "originalFrames": old_reconstruction["verifiedFrames"],
            "totalFrames": d0_reconstruction["verifiedFrames"] + old_reconstruction["verifiedFrames"],
            "allExact": True,
        },
        "inputHashesStable": input_comparison,
        "forbiddenStagesNotRun": ["Unity", "Play", "P1", "ASTC", "device", "build", "delete", "replace"],
        "outputPolicy": "candidate and every file create-new; existing candidate root is fatal; no source/Assets write",
    }
    write_new_json(output_root / "validation.json", validation)
    receipt = {
        "schemaVersion": 1,
        "task": TASK_ID,
        "change": CHANGE_ID,
        "operation": OPERATION_ID,
        "status": "PASS_STAGING_ONLY",
        "command": {
            "pythonExecutable": sys.executable,
            "argv": sys.argv,
            "cwd": str(ROOT),
            "script": str(Path(__file__).resolve()),
        },
        "outputRoot": str(output_root),
        "inputManifest": file_metadata(CURRENT_MANIFEST_PATH),
        "candidateManifest": validation["candidateManifest"],
        "newPage": page_descriptor,
        "measurements": {
            "d0Frames": 34,
            "originalFrames": 88,
            "combinedFrames": 122,
            "newPageSize": [int(page_descriptor["width"]), int(page_descriptor["height"])],
            "newPageRgbaBytes": int(page_descriptor["width"]) * int(page_descriptor["height"]) * 4,
            "newPagePngBytes": int(new_page_path.stat().st_size),
            "nonEmptyPatchCount": sum(1 for item in replacement_patches if item is not None),
            "nullPatchCount": sum(1 for item in replacement_patches if item is None),
            "d0ChangedPixelsFromBase": d0_reconstruction["changedPixelsFromBaseTotal"],
        },
        "packingContract": {
            "resize": False,
            "rotate": False,
            "edgePadding": EDGE_PADDING,
            "allocationAlignment": ALLOCATION_ALIGNMENT,
        },
        "protection": {
            "originalPngDatCatalogManifestOldPagesUnchanged": True,
            "old18PagesAnd4SourcesExact": True,
            "collisionPolicy": "all candidate files create-new; existing output root is fatal",
        },
        "forbiddenStagesNotRun": validation["forbiddenStagesNotRun"],
        "inputHashesPath": "input-hashes.json",
        "validationPath": "validation.json",
    }
    write_new_json(output_root / "receipt.json", receipt)
    report = report_text(
        output_root,
        input_hashes,
        identity,
        layout,
        d0_reconstruction,
        old_reconstruction,
        new_page_validation,
        old_page_summary,
        candidate_manifest,
        {**page_descriptor, "bytes": new_page_path.stat().st_size},
    )
    write_new_text(output_root / "REPORT.md", report)
    expected_files = {
        "manifest.json",
        "validation.json",
        "receipt.json",
        "input-hashes.json",
        "REPORT.md",
        *[str(page["path"]) for page in candidate_manifest["pages"]],
    }
    actual_files = {path.relative_to(output_root).as_posix() for path in output_root.rglob("*") if path.is_file()}
    if actual_files != expected_files:
        raise GenerationError(f"candidate output inventory mismatch missing={sorted(expected_files - actual_files)} unexpected={sorted(actual_files - expected_files)}")
    return {
        "status": "PASS_STAGING_ONLY",
        "outputRoot": root_relative(output_root),
        "newPage": page_descriptor,
        "newPageValidation": new_page_validation,
        "d0Frames": 34,
        "originalFrames": 88,
        "combinedFrames": 122,
        "d0ChangedPixelsFromBase": d0_reconstruction["changedPixelsFromBaseTotal"],
        "inputFileCount": input_comparison["fileCount"],
        "inputHashesStable": True,
    }


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--output-root",
        type=Path,
        default=DEFAULT_OUTPUT_ROOT,
        help="create-new candidate root (default: artifacts/diagnostics/M12-d0-base-patch-20261013/candidate)",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    output_root = args.output_root if args.output_root.is_absolute() else ROOT / args.output_root
    output_existed_before_run = output_root.exists()
    try:
        result = generate(output_root)
    except Exception as error:
        if not output_existed_before_run and output_root.exists() and output_root.is_dir():
            try:
                failure = {
                    "schemaVersion": 1,
                    "task": TASK_ID,
                    "change": CHANGE_ID,
                    "operation": OPERATION_ID,
                    "status": "FAIL_STAGING_ONLY",
                    "errorType": type(error).__name__,
                    "error": str(error),
                    "utc": utc_now(),
                    "traceback": traceback.format_exc(),
                    "forbiddenStagesNotRun": ["Unity", "Play", "P1", "ASTC", "device", "build"],
                }
                failure_path = output_root / "failure.json"
                if not failure_path.exists():
                    write_new_json(failure_path, failure)
                report_path = output_root / "REPORT.md"
                if not report_path.exists():
                    write_new_text(report_path, build_failure_report(output_root, error))
            except Exception as record_error:
                print(f"ERROR while recording staging failure: {record_error}", file=sys.stderr)
        print(f"ERROR: {type(error).__name__}: {error}", file=sys.stderr)
        return 1
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
