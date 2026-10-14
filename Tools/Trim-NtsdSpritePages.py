#!/usr/bin/env python3
"""Build a lossless transparent-margin-trimmed NTSD sprite-page candidate.

This helper is deliberately staging-only.  It reads the deployed derived-page
manifest, source PNGs, DATs, and old page PNGs, then writes a complete candidate
under the requested artifact output directory.  It never writes under Assets,
never changes the source PNG/DAT/old pages, and refuses every output collision.

The candidate keeps the existing NTSD_DAT_PAGES_V1 manifest schema.  Kirin and
the two byte-identical Sui sources receive optional per-frame ``contentRects``;
win2 remains byte-for-byte copied with its existing descriptors and chunks.
"""

from __future__ import annotations

import argparse
import copy
import hashlib
import io
import json
import os
import sys
from pathlib import Path
from typing import Any

import numpy as np
from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
CONTENT_ROOT = ROOT / "Assets" / "NTSD" / "Content" / "LoganRuntime"
DEFAULT_MANIFEST = CONTENT_ROOT / "derived_sprite_pages" / "manifest.json"
DEFAULT_OUTPUT_ROOT = ROOT / "artifacts" / "diagnostics" / "M12-transparent-trim-20261013" / "candidate"
ARTIFACT_ROOT = ROOT / "artifacts" / "diagnostics" / "M12-transparent-trim-20261013"
PAGE_LIMIT = 2048
EDGE_PADDING = 1
ALLOCATION_ALIGNMENT = 4
GENERATOR_VERSION = "NTSD_DAT_PAGES_V1"
TASK_ID = "NTSD-M12-TRANSPARENT-TRIM-20261013"
CHANGE_ID = "NTSD-M12-TRANSPARENT-TRIM-001"
OPERATION_ID = "OP-NTSD-M12-TRANSPARENT-TRIM-20261013"
WIN2_SOURCE = "c/dan/a/win2.png"
KIRIN_SOURCE = "c/sasu/a/Kirin.png"
SUI_SOURCES = ("c/sui/sui3.png", "c/kar/a/sui.png")
TRIM_SOURCES = (KIRIN_SOURCE, *SUI_SOURCES)
TRIM_SOURCE_SET = set(TRIM_SOURCES)


class TrimError(RuntimeError):
    """A bounded input, schema, or output validation failure."""


def fail(message: str) -> None:
    raise TrimError(message)


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def json_bytes(value: Any) -> bytes:
    return (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def write_new_bytes(path: Path, data: bytes) -> None:
    if path.exists():
        fail(f"拒绝覆盖既有输出: {path}")
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("xb") as stream:
        stream.write(data)


def write_new_json(path: Path, value: Any) -> None:
    write_new_bytes(path, json_bytes(value))


def write_new_text(path: Path, value: str) -> None:
    write_new_bytes(path, value.encode("utf-8"))


def png_bytes(array: np.ndarray) -> bytes:
    if array.dtype != np.uint8 or array.ndim != 3 or array.shape[2] != 4:
        fail(f"PNG array 不是 RGBA uint8: shape={array.shape} dtype={array.dtype}")
    buffer = io.BytesIO()
    Image.fromarray(array, "RGBA").save(buffer, format="PNG", compress_level=9)
    return buffer.getvalue()


def read_png_rgba(path: Path) -> tuple[np.ndarray, dict[str, Any]]:
    try:
        with Image.open(path) as image:
            info = {
                "width": int(image.width),
                "height": int(image.height),
                "mode": image.mode,
                "format": image.format,
            }
            array = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
    except Exception as error:  # pragma: no cover - reported as a bounded input failure
        fail(f"无法读取 RGBA PNG {path}: {error}")
    return array, info


def align4(value: int) -> int:
    return (int(value) + ALLOCATION_ALIGNMENT - 1) // ALLOCATION_ALIGNMENT * ALLOCATION_ALIGNMENT


def normalize_rel(value: str) -> str:
    return value.replace("\\", "/").strip().lstrip("/")


def safe_relative(root: Path, relative: str, label: str) -> Path:
    normalized = normalize_rel(relative)
    candidate = Path(normalized)
    if not normalized or candidate.is_absolute() or ":" in normalized or ".." in candidate.parts:
        fail(f"{label} 路径不安全: {relative}")
    path = (root / candidate).resolve()
    root_resolved = root.resolve()
    try:
        path.relative_to(root_resolved)
    except ValueError:
        fail(f"{label} 路径越界: {relative}")
    return path


def source_path(relative: str) -> Path:
    return safe_relative(CONTENT_ROOT / "vfs", relative, "source")


def dat_path(relative: str) -> Path:
    return safe_relative(CONTENT_ROOT / "decoded_dat", relative, "DAT")


def deployed_page_path(relative: str) -> Path:
    return safe_relative(CONTENT_ROOT / "derived_sprite_pages", relative, "deployed page")


def output_path(output_root: Path, relative: str) -> Path:
    return safe_relative(output_root, relative, "candidate output")


def frame_array(source: dict[str, Any], image: np.ndarray, local_pic: int) -> tuple[np.ndarray, tuple[int, int]]:
    columns = int(source["columns"])
    rows = int(source["rows"])
    frame_width = int(source["frameWidth"])
    frame_height = int(source["frameHeight"])
    capacity = columns * rows
    if local_pic < 0 or local_pic >= capacity:
        fail(f"local frame 越界: {source['path']} local={local_pic} capacity={capacity}")
    x = (local_pic % columns) * (frame_width + 1)
    y = (local_pic // columns) * (frame_height + 1)
    if x + frame_width > image.shape[1] or y + frame_height > image.shape[0]:
        fail(f"source stride frame 越界: {source['path']} local={local_pic} xy={x},{y}")
    return image[y : y + frame_height, x : x + frame_width].copy(), (x, y)


def rect_tuple(rect: dict[str, Any]) -> tuple[int, int, int, int]:
    return (int(rect["x"]), int(rect["y"]), int(rect["width"]), int(rect["height"]))


def rect_json(rect: tuple[int, int, int, int]) -> dict[str, int]:
    return {"x": int(rect[0]), "y": int(rect[1]), "width": int(rect[2]), "height": int(rect[3])}


def nonzero_bbox(frame: np.ndarray) -> tuple[int, int, int, int] | None:
    # Any non-zero channel is content.  In particular RGB data with alpha 0 is
    # intentionally retained instead of being treated as transparent margins.
    mask = np.any(frame != 0, axis=2)
    ys, xs = np.nonzero(mask)
    if len(xs) == 0:
        return None
    left = int(xs.min())
    top = int(ys.min())
    right = int(xs.max())
    bottom = int(ys.max())
    return left, top, right - left + 1, bottom - top + 1


def trim_frame(source: dict[str, Any], local_pic: int, frame: np.ndarray, source_xy: tuple[int, int]) -> dict[str, Any]:
    frame_width = int(source["frameWidth"])
    frame_height = int(source["frameHeight"])
    bbox = nonzero_bbox(frame)
    if bbox is None:
        crop = np.zeros((1, 1, 4), dtype=np.uint8)
        crop_box = (0, 0, 1, 1)
        content = (0, 0, 1, 1)
        empty = True
    else:
        left, top, width, height = bbox
        crop = frame[top : top + height, left : left + width].copy()
        crop_box = bbox
        content = (left, frame_height - top - height, width, height)
        empty = False
    reconstructed = np.zeros_like(frame)
    content_x, content_y, content_width, content_height = content
    top = frame_height - content_y - content_height
    reconstructed[top : top + content_height, content_x : content_x + content_width] = crop
    if not np.array_equal(reconstructed, frame):
        fail(f"裁剪重建不一致: {source['path']} local={local_pic}")
    return {
        "localPic": int(local_pic),
        "sourceTopLeft": [int(source_xy[0]), int(source_xy[1])],
        "sourceFrameSha256": sha256_bytes(frame.tobytes()),
        "cropSha256": sha256_bytes(crop.tobytes()),
        "crop": crop,
        "cropBoxTopLeft": [int(value) for value in crop_box],
        "nonZeroBboxTopLeft": None if bbox is None else [int(value) for value in bbox],
        "contentRect": rect_json(content),
        "empty": bool(empty),
        "width": int(crop.shape[1]),
        "height": int(crop.shape[0]),
        "rgbaBytes": int(crop.shape[0] * crop.shape[1] * 4),
        "fullRgbaBytes": int(frame.shape[0] * frame.shape[1] * 4),
    }


def make_trim_records(source: dict[str, Any], image: np.ndarray) -> list[dict[str, Any]]:
    capacity = int(source["columns"]) * int(source["rows"])
    records = []
    for local_pic in range(capacity):
        frame, source_xy = frame_array(source, image, local_pic)
        records.append(trim_frame(source, local_pic, frame, source_xy))
    return records


def build_trailing_aliases(records: list[dict[str, Any]]) -> dict[int, int]:
    aliases = {index: index for index in range(len(records))}
    if len(records) < 2:
        return aliases
    end = len(records) - 1
    start = end
    last = records[end]
    while start > 0:
        prior = records[start - 1]
        same = (
            prior["cropSha256"] == last["cropSha256"]
            and prior["contentRect"] == last["contentRect"]
            and np.array_equal(prior["crop"], last["crop"])
        )
        if not same:
            break
        start -= 1
    if start == end:
        return aliases
    for local_pic in range(start + 1, end + 1):
        aliases[local_pic] = start
    return aliases


def simulate_shelf(records: list[dict[str, Any]], width_limit: int) -> dict[str, Any]:
    if not records:
        fail("不能为空 frame 集合打包")
    pages: list[dict[str, Any]] = []
    current: dict[str, Any] | None = None
    x = 1
    y = 1
    shelf_height = 0

    def finish_page() -> None:
        nonlocal current
        if current is None:
            return
        current["width"] = align4(max(4, int(current["maxRight"])))
        current["height"] = align4(max(4, int(current["maxBottom"])))
        if current["width"] > PAGE_LIMIT or current["height"] > PAGE_LIMIT:
            fail(f"生成页超过 2048: {current['width']}x{current['height']}")
        if current["width"] > width_limit:
            fail(f"生成页超过候选宽度: {current['width']} > {width_limit}")
        pages.append(current)
        current = None

    def start_page() -> None:
        nonlocal current, x, y, shelf_height
        current = {"placements": [], "maxRight": 0, "maxBottom": 0}
        x = 1
        y = 1
        shelf_height = 0

    for record in records:
        crop_width = int(record["width"])
        crop_height = int(record["height"])
        allocation_width = align4(crop_width + 2 * EDGE_PADDING)
        allocation_height = align4(crop_height + 2 * EDGE_PADDING)
        if allocation_width + 1 > PAGE_LIMIT or allocation_height + 1 > PAGE_LIMIT:
            fail(f"单帧无法放入 2048 页: local={record['localPic']} allocation={allocation_width}x{allocation_height}")
        if current is None:
            start_page()
        if x + allocation_width > width_limit:
            y += shelf_height
            x = 1
            shelf_height = 0
        if y + allocation_height > PAGE_LIMIT:
            finish_page()
            start_page()
        allocation_x = x
        allocation_y = y
        placement = {
            "localPic": int(record["localPic"]),
            "allocationX": int(allocation_x),
            "allocationYTop": int(allocation_y),
            "allocationWidth": int(allocation_width),
            "allocationHeight": int(allocation_height),
            "x": int(allocation_x + EDGE_PADDING),
            "topY": int(allocation_y + EDGE_PADDING),
            "width": crop_width,
            "height": crop_height,
        }
        current["placements"].append(placement)
        current["maxRight"] = max(int(current["maxRight"]), allocation_x + allocation_width)
        current["maxBottom"] = max(int(current["maxBottom"]), allocation_y + allocation_height)
        x += allocation_width
        shelf_height = max(shelf_height, allocation_height)
    finish_page()
    total_area = sum(int(page["width"]) * int(page["height"]) for page in pages)
    return {
        "pages": pages,
        "widthLimit": int(width_limit),
        "totalArea": int(total_area),
        "pageCount": len(pages),
    }


def choose_shelf(records: list[dict[str, Any]]) -> dict[str, Any]:
    max_allocation_width = max(align4(int(record["width"]) + 2) for record in records)
    candidates = [width for width in (2048, 1536, 1280, 1024, 896, 768) if width >= max_allocation_width + 1]
    if not candidates:
        candidates = [PAGE_LIMIT]
    choices = [simulate_shelf(records, width) for width in candidates]
    return min(choices, key=lambda item: (int(item["totalArea"]), int(item["pageCount"]), int(item["widthLimit"])))


def render_page(layout: dict[str, Any], records_by_local: dict[int, dict[str, Any]]) -> np.ndarray:
    page = np.zeros((int(layout["height"]), int(layout["width"]), 4), dtype=np.uint8)
    for placement in layout["placements"]:
        record = records_by_local[int(placement["localPic"])]
        crop = record["crop"]
        x = int(placement["x"])
        top_y = int(placement["topY"])
        width = int(placement["width"])
        height = int(placement["height"])
        page[top_y : top_y + height, x : x + width] = crop
        page[top_y : top_y + height, x - 1] = crop[:, 0]
        page[top_y : top_y + height, x + width] = crop[:, -1]
        page[top_y - 1, x : x + width] = crop[0, :]
        page[top_y + height, x : x + width] = crop[-1, :]
        page[top_y - 1, x - 1] = crop[0, 0]
        page[top_y - 1, x + width] = crop[0, -1]
        page[top_y + height, x - 1] = crop[-1, 0]
        page[top_y + height, x + width] = crop[-1, -1]
    return page


def build_trim_group(source_sha: str, records: list[dict[str, Any]], group_name: str) -> dict[str, Any]:
    aliases = build_trailing_aliases(records)
    physical_records = [record for record in records if aliases[int(record["localPic"])] == int(record["localPic"])]
    layout = choose_shelf(physical_records)
    placement_by_local: dict[int, dict[str, Any]] = {}
    for page_index, page in enumerate(layout["pages"]):
        for placement in page["placements"]:
            placement_by_local[int(placement["localPic"])] = {
                "pageIndex": int(page_index),
                "placement": placement,
            }
    for local_pic, physical_local in aliases.items():
        if physical_local not in placement_by_local:
            fail(f"alias physical frame 未打包: local={local_pic} physical={physical_local}")
        placement_by_local[local_pic] = copy.deepcopy(placement_by_local[physical_local])

    page_prefix = source_sha[:16]
    page_infos: list[dict[str, Any]] = []
    records_by_local = {int(record["localPic"]): record for record in records}
    for page_index, layout_page in enumerate(layout["pages"]):
        page_id = f"page-{page_prefix}-trim-{group_name}-{page_index:04d}-{int(layout_page['width'])}x{int(layout_page['height'])}"
        relative = f"pages/{page_id}.png"
        page_array = render_page(layout_page, records_by_local)
        payload = png_bytes(page_array)
        page_infos.append(
            {
                "id": page_id,
                "path": relative,
                "sha256": sha256_bytes(payload),
                "width": int(layout_page["width"]),
                "height": int(layout_page["height"]),
                "bytes": len(payload),
                "rgbaBytes": int(layout_page["width"] * layout_page["height"] * 4),
                "payload": payload,
                "array": page_array,
                "placements": copy.deepcopy(layout_page["placements"]),
            }
        )

    mappings: list[dict[str, Any]] = []
    for record in records:
        local_pic = int(record["localPic"])
        placement_info = placement_by_local[local_pic]
        placement = placement_info["placement"]
        page_info = page_infos[int(placement_info["pageIndex"])]
        page_y = int(page_info["height"]) - int(placement["topY"]) - int(placement["height"])
        mappings.append(
            {
                "localPic": local_pic,
                "physicalLocalPic": int(aliases[local_pic]),
                "alias": bool(aliases[local_pic] != local_pic),
                "page": page_info["id"],
                "pagePath": page_info["path"],
                "rect": {"x": int(placement["x"]), "y": page_y, "width": int(placement["width"]), "height": int(placement["height"])},
                "contentRect": copy.deepcopy(record["contentRect"]),
                "pageTopLeft": [int(placement["x"]), int(placement["topY"])],
            }
        )

    chunks: list[dict[str, Any]] = []
    run_start = 0
    while run_start < len(mappings):
        run_page = mappings[run_start]["page"]
        run_end = run_start + 1
        while run_end < len(mappings) and mappings[run_end]["page"] == run_page:
            run_end += 1
        run = mappings[run_start:run_end]
        chunk = {
            "page": run_page,
            "firstLocalPic": int(run_start),
            "count": int(len(run)),
            "columns": int(len(run)),
            "rows": 1,
            "rects": [copy.deepcopy(item["rect"]) for item in run],
            "contentRects": [copy.deepcopy(item["contentRect"]) for item in run],
        }
        if any(bool(item["alias"]) for item in run):
            chunk["allowExactFrameAliases"] = True
        chunks.append(chunk)
        run_start = run_end

    return {
        "name": group_name,
        "sourceSha256": source_sha,
        "records": records,
        "aliases": aliases,
        "layout": layout,
        "pages": page_infos,
        "mappings": mappings,
        "chunks": chunks,
        "physicalFrameCount": len(physical_records),
        "aliasFrameCount": sum(1 for local_pic, physical_local_pic in aliases.items() if local_pic != physical_local_pic),
    }


def load_and_validate_inputs(manifest_path: Path) -> dict[str, Any]:
    if not manifest_path.is_file():
        fail(f"manifest 不存在: {manifest_path}")
    manifest_bytes = manifest_path.read_bytes()
    try:
        manifest = json.loads(manifest_bytes.decode("utf-8"))
    except Exception as error:
        fail(f"manifest JSON 无法读取: {error}")
    if manifest.get("version") != 1 or manifest.get("generatorVersion") != GENERATOR_VERSION:
        fail("manifest 版本不是现有 NTSD_DAT_PAGES_V1")
    sources = manifest.get("sources")
    pages = manifest.get("pages")
    if not isinstance(sources, list) or not isinstance(pages, list) or not sources or not pages:
        fail("manifest 缺少 sources/pages")
    source_by_path = {str(source.get("path")): source for source in sources}
    expected_sources = {WIN2_SOURCE, KIRIN_SOURCE, *SUI_SOURCES}
    if set(source_by_path) != expected_sources:
        fail(f"manifest source 集合变化: {sorted(source_by_path)}")
    if len(source_by_path) != len(sources):
        fail("manifest source path 重复")

    input_hashes: dict[str, Any] = {
        "manifest": {"path": str(manifest_path), "sha256": sha256_bytes(manifest_bytes), "bytes": len(manifest_bytes)},
        "sources": [],
        "dats": [],
        "pages": [],
    }
    source_arrays: dict[str, np.ndarray] = {}
    for source in sources:
        path = str(source["path"])
        image_path = source_path(path)
        if not image_path.is_file():
            fail(f"source PNG 不存在: {image_path}")
        source_hash = sha256_file(image_path)
        if source_hash.lower() != str(source["sha256"]).lower():
            fail(f"source SHA 不匹配: {path} manifest={source['sha256']} actual={source_hash}")
        image, header = read_png_rgba(image_path)
        if [int(image.shape[1]), int(image.shape[0])] != [int(source["width"]), int(source["height"])]:
            fail(f"source 尺寸不匹配: {path} manifest={source['width']}x{source['height']} actual={image.shape[1]}x{image.shape[0]}")
        stride_extent = [int(source["columns"]) * (int(source["frameWidth"]) + 1), int(source["rows"]) * (int(source["frameHeight"]) + 1)]
        if stride_extent[0] > int(source["width"]) or stride_extent[1] > int(source["height"]):
            fail(f"source stride extent 超出 source canvas: {path} extent={stride_extent}")
        source_arrays[path] = image
        input_hashes["sources"].append(
            {
                "path": path,
                "absolutePath": str(image_path),
                "sha256": source_hash,
                "bytes": image_path.stat().st_size,
                "width": int(source["width"]),
                "height": int(source["height"]),
                "frameWidth": int(source["frameWidth"]),
                "frameHeight": int(source["frameHeight"]),
                "columns": int(source["columns"]),
                "rows": int(source["rows"]),
                "mode": header["mode"],
                "nativeStride": [int(source["frameWidth"]) + 1, int(source["frameHeight"]) + 1],
            }
        )
        for binding in source.get("bindings", []):
            binding_path = dat_path(str(binding["datPath"]))
            if not binding_path.is_file():
                fail(f"DAT 不存在: {binding_path}")
            binding_hash = sha256_file(binding_path)
            if binding_hash.lower() != str(binding["datSha256"]).lower():
                fail(f"DAT SHA 不匹配: {binding['datPath']} manifest={binding['datSha256']} actual={binding_hash}")
            input_hashes["dats"].append(
                {
                    "source": path,
                    "path": str(binding["datPath"]),
                    "absolutePath": str(binding_path),
                    "sha256": binding_hash,
                    "bytes": binding_path.stat().st_size,
                    "oid": int(binding["oid"]),
                    "firstPic": int(binding["firstPic"]),
                    "lastPic": int(binding["lastPic"]),
                }
            )

    page_by_id: dict[str, dict[str, Any]] = {}
    page_arrays: dict[str, np.ndarray] = {}
    for page in pages:
        page_id = str(page["id"])
        if page_id in page_by_id:
            fail(f"page id 重复: {page_id}")
        page_path = deployed_page_path(str(page["path"]))
        if not page_path.is_file():
            fail(f"deployed page 不存在: {page_path}")
        page_hash = sha256_file(page_path)
        if page_hash.lower() != str(page["sha256"]).lower():
            fail(f"deployed page SHA 不匹配: {page_id}")
        page_array, header = read_png_rgba(page_path)
        if [int(page_array.shape[1]), int(page_array.shape[0])] != [int(page["width"]), int(page["height"])]:
            fail(f"deployed page 尺寸不匹配: {page_id}")
        page_by_id[page_id] = page
        page_arrays[page_id] = page_array
        input_hashes["pages"].append(
            {
                "id": page_id,
                "path": str(page["path"]),
                "absolutePath": str(page_path),
                "sha256": page_hash,
                "bytes": page_path.stat().st_size,
                "width": int(page["width"]),
                "height": int(page["height"]),
                "mode": header["mode"],
            }
        )

    if len(input_hashes["dats"]) != 4:
        fail(f"预期 4 个 DAT binding hash，实际 {len(input_hashes['dats'])}")
    if len(input_hashes["pages"]) != 19:
        fail(f"预期当前 19 页，实际 {len(input_hashes['pages'])}")

    win2 = source_by_path[WIN2_SOURCE]
    win2_page_ids = [str(chunk["page"]) for chunk in win2["chunks"]]
    if len(set(win2_page_ids)) != len(win2_page_ids):
        fail("win2 当前 descriptor page 不唯一，无法执行原样保留")
    for page_id in win2_page_ids:
        if page_id not in page_by_id:
            fail(f"win2 chunk 引用未知 page: {page_id}")
    if not np.array_equal(source_arrays[SUI_SOURCES[0]], source_arrays[SUI_SOURCES[1]]):
        fail("共享 Sui 两个 source PNG 的 decoded RGBA 不一致")
    if str(source_by_path[SUI_SOURCES[0]]["sha256"]).lower() != str(source_by_path[SUI_SOURCES[1]]["sha256"]).lower():
        fail("共享 Sui 两个 source SHA 不一致")

    return {
        "manifest": manifest,
        "manifestBytes": manifest_bytes,
        "manifestPath": manifest_path,
        "sourceByPath": source_by_path,
        "sourceArrays": source_arrays,
        "pageById": page_by_id,
        "pageArrays": page_arrays,
        "inputHashes": input_hashes,
        "win2PageIds": win2_page_ids,
    }


def frame_map_for_chunk(chunks: list[dict[str, Any]]) -> dict[int, tuple[dict[str, Any], int]]:
    mapping: dict[int, tuple[dict[str, Any], int]] = {}
    next_local = 0
    for chunk in chunks:
        first = int(chunk["firstLocalPic"])
        count = int(chunk["count"])
        if first != next_local:
            fail(f"chunk firstLocalPic 不连续: expected={next_local} actual={first}")
        if len(chunk.get("rects", [])) != count:
            fail("chunk rect count 不一致")
        for offset in range(count):
            local = first + offset
            if local in mapping:
                fail(f"chunk local frame 重复: {local}")
            mapping[local] = (chunk, offset)
        next_local += count
    return mapping


def render_chunk_page_desc(chunk: dict[str, Any], page_by_id: dict[str, dict[str, Any]]) -> dict[str, Any]:
    page_id = str(chunk["page"])
    if page_id not in page_by_id:
        fail(f"new chunk 引用未知 page: {page_id}")
    return copy.deepcopy(page_by_id[page_id])


def target_chunks_from_group(group: dict[str, Any]) -> list[dict[str, Any]]:
    return copy.deepcopy(group["chunks"])


def build_candidate_manifest(context: dict[str, Any], groups: dict[str, dict[str, Any]]) -> dict[str, Any]:
    original = context["manifest"]
    candidate = {
        "version": original["version"],
        "generatorVersion": original["generatorVersion"],
        "pages": [],
        "sources": [],
    }
    original_pages = context["pageById"]
    for page in original["pages"]:
        if str(page["id"]) in context["win2PageIds"]:
            candidate["pages"].append(copy.deepcopy(page))
    unique_groups: list[dict[str, Any]] = []
    seen_group_ids: set[int] = set()
    for group in groups.values():
        group_identity = id(group)
        if group_identity in seen_group_ids:
            continue
        seen_group_ids.add(group_identity)
        unique_groups.append(group)
    for group in unique_groups:
        for page in group["pages"]:
            candidate["pages"].append(
                {
                    "id": page["id"],
                    "path": page["path"],
                    "sha256": page["sha256"],
                    "width": page["width"],
                    "height": page["height"],
                }
            )
    for source in original["sources"]:
        source_copy = copy.deepcopy(source)
        source_path_value = str(source["path"])
        if source_path_value in groups:
            source_copy["chunks"] = target_chunks_from_group(groups[source_path_value])
        candidate["sources"].append(source_copy)
    # Explicitly enforce the only intended source/chunk changes before any output is written.
    for source in original["sources"]:
        path = str(source["path"])
        candidate_source = next(item for item in candidate["sources"] if str(item["path"]) == path)
        if path == WIN2_SOURCE and candidate_source != source:
            fail("win2 source descriptor/chunks 发生非授权变化")
        if path not in TRIM_SOURCE_SET and candidate_source != source:
            fail(f"非目标 source 发生变化: {path}")
        if path in TRIM_SOURCE_SET:
            for key in ("path", "sha256", "width", "height", "frameWidth", "frameHeight", "columns", "rows", "bindings"):
                if candidate_source[key] != source[key]:
                    fail(f"目标 source 元数据发生非授权变化: {path} key={key}")
    if len(candidate["pages"]) != len(context["win2PageIds"]) + sum(len(group["pages"]) for group in unique_groups):
        fail("候选 page descriptor 数量异常")
    return candidate


def page_descriptor_by_id(manifest: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {str(page["id"]): page for page in manifest["pages"]}


def page_cell(page: np.ndarray, rect: dict[str, Any]) -> np.ndarray:
    x, bottom_y, width, height = rect_tuple(rect)
    top_y = page.shape[0] - bottom_y - height
    if x < 0 or bottom_y < 0 or width <= 0 or height <= 0 or x + width > page.shape[1] or top_y < 0 or top_y + height > page.shape[0]:
        fail(f"候选 rect 超出 page: rect={rect} page={page.shape}")
    return page[top_y : top_y + height, x : x + width]


def reconstruct_full(frame_width: int, frame_height: int, crop: np.ndarray, content: dict[str, Any]) -> np.ndarray:
    x, y, width, height = rect_tuple(content)
    if x < 0 or y < 0 or width <= 0 or height <= 0 or x + width > frame_width or y + height > frame_height:
        fail(f"contentRect 超出 native canvas: {content} canvas={frame_width}x{frame_height}")
    if crop.shape[1] != width or crop.shape[0] != height:
        fail(f"contentRect 与 page crop 尺寸不一致: content={content} crop={crop.shape}")
    output = np.zeros((frame_height, frame_width, 4), dtype=np.uint8)
    top_y = frame_height - y - height
    output[top_y : top_y + height, x : x + width] = crop
    return output


def validate_new_page_edges(group: dict[str, Any], candidate_page_arrays: dict[str, np.ndarray]) -> dict[str, Any]:
    records = {int(record["localPic"]): record for record in group["records"]}
    checks = []
    for page_info in group["pages"]:
        page_id = page_info["id"]
        page = candidate_page_arrays[page_id]
        occupied: list[tuple[int, int, int, int]] = []
        for placement in page_info["placements"]:
            record = records[int(placement["localPic"])]
            crop = record["crop"]
            x = int(placement["x"])
            top_y = int(placement["topY"])
            width = int(placement["width"])
            height = int(placement["height"])
            actual = page[top_y : top_y + height, x : x + width]
            if not np.array_equal(actual, crop):
                fail(f"page physical crop 与 trim source 不一致: {page_id} local={record['localPic']}")
            if not np.array_equal(page[top_y : top_y + height, x - 1], crop[:, 0]):
                fail(f"page left edge extrusion 不一致: {page_id}")
            if not np.array_equal(page[top_y : top_y + height, x + width], crop[:, -1]):
                fail(f"page right edge extrusion 不一致: {page_id}")
            if not np.array_equal(page[top_y - 1, x : x + width], crop[0, :]):
                fail(f"page top edge extrusion 不一致: {page_id}")
            if not np.array_equal(page[top_y + height, x : x + width], crop[-1, :]):
                fail(f"page bottom edge extrusion 不一致: {page_id}")
            allocation = (x - 1, top_y - 1, align4(width + 2), align4(height + 2))
            ax, ay, aw, ah = allocation
            if ax < 0 or ay < 0 or ax + aw > page.shape[1] or ay + ah > page.shape[0]:
                fail(f"page allocation 越界: {page_id} allocation={allocation} page={page.shape}")
            for prior in occupied:
                overlaps = not (ax + aw <= prior[0] or prior[0] + prior[2] <= ax or ay + ah <= prior[1] or prior[1] + prior[3] <= ay)
                if overlaps:
                    fail(f"page allocation 重叠: {page_id} {allocation} vs {prior}")
            occupied.append(allocation)
        occupied_mask = np.zeros(page.shape[:2], dtype=bool)
        for ax, ay, aw, ah in occupied:
            occupied_mask[ay : ay + ah, ax : ax + aw] = True
        outside = page[~occupied_mask]
        if outside.size and not np.all(outside == 0):
            fail(f"page 未使用区不是 RGBA zero: {page_id}")
        checks.append(
            {
                "page": page_id,
                "width": int(page.shape[1]),
                "height": int(page.shape[0]),
                "placements": len(occupied),
                "allocationAligned": all(allocation[2] % 4 == 0 and allocation[3] % 4 == 0 for allocation in occupied),
                "edgeExtrusionExact": True,
                "unusedPixelsZero": True,
            }
        )
    return {"pages": checks, "allPagesValid": True}


def validate_candidate(
    context: dict[str, Any],
    candidate: dict[str, Any],
    output_root: Path,
    groups: dict[str, dict[str, Any]],
) -> dict[str, Any]:
    candidate_manifest_path = output_path(output_root, "manifest.json")
    candidate_manifest_bytes = candidate_manifest_path.read_bytes()
    if json.loads(candidate_manifest_bytes.decode("utf-8")) != candidate:
        fail("写出的 candidate manifest 与内存 candidate 不一致")
    descriptors = page_descriptor_by_id(candidate)
    candidate_page_arrays: dict[str, np.ndarray] = {}
    candidate_page_hashes: list[dict[str, Any]] = []
    for page in candidate["pages"]:
        page_id = str(page["id"])
        path = output_path(output_root, str(page["path"]))
        if not path.is_file():
            fail(f"候选 page 缺失: {page_id}")
        actual_hash = sha256_file(path)
        if actual_hash.lower() != str(page["sha256"]).lower():
            fail(f"候选 page SHA 不匹配: {page_id}")
        array, header = read_png_rgba(path)
        if [int(array.shape[1]), int(array.shape[0])] != [int(page["width"]), int(page["height"])]:
            fail(f"候选 page 尺寸不匹配: {page_id}")
        candidate_page_arrays[page_id] = array
        candidate_page_hashes.append(
            {
                "id": page_id,
                "path": str(page["path"]),
                "sha256": actual_hash,
                "bytes": path.stat().st_size,
                "width": int(page["width"]),
                "height": int(page["height"]),
                "rgbaBytes": int(page["width"]) * int(page["height"]) * 4,
                "mode": header["mode"],
            }
        )
    used_pages: set[str] = set()
    source_validation: list[dict[str, Any]] = []
    trim_validation: list[dict[str, Any]] = []
    total_frames = 0
    exact_frames = 0
    for source in candidate["sources"]:
        source_path_value = str(source["path"])
        source_input = context["sourceByPath"][source_path_value]
        if source["sha256"].lower() != source_input["sha256"].lower():
            fail(f"候选 source SHA 变化: {source_path_value}")
        if source_path_value in TRIM_SOURCE_SET:
            expected_group = groups[source_path_value if source_path_value == KIRIN_SOURCE else SUI_SOURCES[0]]
            expected_chunks = expected_group["chunks"]
            if source["chunks"] != expected_chunks:
                fail(f"候选 target chunks 与生成结果不一致: {source_path_value}")
        else:
            if source != source_input:
                fail(f"非目标 source descriptor 变化: {source_path_value}")
        image = context["sourceArrays"][source_path_value]
        frame_map = frame_map_for_chunk(source["chunks"])
        capacity = int(source["columns"]) * int(source["rows"])
        if set(frame_map) != set(range(capacity)):
            fail(f"候选 source 未完整覆盖 native frames: {source_path_value}")
        source_exact = 0
        source_records = []
        for local_pic in range(capacity):
            chunk, offset = frame_map[local_pic]
            rect = chunk["rects"][offset]
            page_id = str(chunk["page"])
            used_pages.add(page_id)
            actual_crop = page_cell(candidate_page_arrays[page_id], rect)
            expected_full, source_xy = frame_array(source, image, local_pic)
            content_list = chunk.get("contentRects")
            if content_list is None:
                content = {"x": 0, "y": 0, "width": int(source["frameWidth"]), "height": int(source["frameHeight"])}
            else:
                if len(content_list) != int(chunk["count"]):
                    fail(f"contentRects count 不一致: {source_path_value} chunk={chunk['firstLocalPic']}")
                content = content_list[offset]
            reconstructed = reconstruct_full(int(source["frameWidth"]), int(source["frameHeight"]), actual_crop, content)
            if not np.array_equal(reconstructed, expected_full):
                fail(f"full RGBA 重建不一致: {source_path_value} local={local_pic}")
            source_exact += 1
            total_frames += 1
            exact_frames += 1
            source_records.append(
                {
                    "localPic": local_pic,
                    "nativePic": int(source["bindings"][0]["firstPic"]) + local_pic,
                    "sourceTopLeft": [int(source_xy[0]), int(source_xy[1])],
                    "nativeStride": [int(source["frameWidth"]) + 1, int(source["frameHeight"]) + 1],
                    "page": page_id,
                    "rect": copy.deepcopy(rect),
                    "contentRect": copy.deepcopy(content),
                    "fullRgbaExact": True,
                    "cropRgbaSha256": sha256_bytes(actual_crop.tobytes()),
                    "nativeRgbaSha256": sha256_bytes(expected_full.tobytes()),
                }
            )
        source_validation.append(
            {
                "source": source_path_value,
                "nativeFrames": capacity,
                "verifiedFrames": source_exact,
                "fullRgbaReconstructionExact": source_exact == capacity,
                "contentRectsPresent": any("contentRects" in chunk for chunk in source["chunks"]),
                "records": source_records,
            }
        )

    for source_path_value, group in groups.items():
        trim_validation.append(
            {
                "source": source_path_value,
                "frameWidth": int(context["sourceByPath"][source_path_value]["frameWidth"]),
                "frameHeight": int(context["sourceByPath"][source_path_value]["frameHeight"]),
                "nativeStride": [int(context["sourceByPath"][source_path_value]["frameWidth"]) + 1, int(context["sourceByPath"][source_path_value]["frameHeight"]) + 1],
                "nativeFrames": len(group["records"]),
                "physicalFrames": int(group["physicalFrameCount"]),
                "aliasFrames": int(group["aliasFrameCount"]),
                "records": [
                    {
                        key: value
                        for key, value in record.items()
                        if key != "crop"
                    }
                    for record in group["records"]
                ],
            }
        )
    if used_pages != set(descriptors):
        fail(f"候选有未使用或缺失 page descriptor: used={len(used_pages)} descriptors={len(descriptors)}")

    edge_validation = {name: validate_new_page_edges(group, candidate_page_arrays) for name, group in groups.items()}
    win2_preserved = []
    original_manifest = context["manifest"]
    original_page_by_id = context["pageById"]
    for page_id in context["win2PageIds"]:
        original_page = original_page_by_id[page_id]
        candidate_page = descriptors[page_id]
        original_path = deployed_page_path(str(original_page["path"]))
        candidate_path = output_path(output_root, str(candidate_page["path"]))
        old_bytes = original_path.read_bytes()
        new_bytes = candidate_path.read_bytes()
        if old_bytes != new_bytes or original_page != candidate_page:
            fail(f"win2 page/descriptor 非原样: {page_id}")
        win2_preserved.append(
            {
                "id": page_id,
                "descriptorExact": True,
                "bytesExact": True,
                "sha256": sha256_bytes(new_bytes),
                "bytes": len(new_bytes),
            }
        )

    protected_after = capture_current_input_hashes(context)
    if protected_after != context["inputHashes"]:
        fail("source/DAT/old manifest/page 输入在 helper 执行期间发生变化")

    return {
        "status": "PASS",
        "candidateManifest": {
            "path": "manifest.json",
            "sha256": sha256_bytes(candidate_manifest_bytes),
            "bytes": len(candidate_manifest_bytes),
        },
        "candidatePages": candidate_page_hashes,
        "usedPageCount": len(used_pages),
        "sourceCount": len(candidate["sources"]),
        "nativeLogicalFrameCount": total_frames,
        "fullRgbaReconstructionExactFrames": exact_frames,
        "fullRgbaReconstructionExact": total_frames == 88 and exact_frames == 88,
        "sourceValidation": source_validation,
        "trimValidation": trim_validation,
        "newPageEdgeValidation": edge_validation,
        "win2Preserved": win2_preserved,
        "protectedInputsUnchanged": True,
        "generatorVersion": candidate["generatorVersion"],
        "defaultFullCanvasCompatibility": all(
            all("contentRects" not in chunk for chunk in source["chunks"])
            for source in candidate["sources"]
            if str(source["path"]) == WIN2_SOURCE
        ),
        "candidateSchemaKeys": {"manifest": ["version", "generatorVersion", "pages", "sources"], "chunkOptional": ["contentRects", "allowExactFrameAliases"]},
    }


def capture_current_input_hashes(context: dict[str, Any]) -> dict[str, Any]:
    result = {
        "manifest": {
            "path": context["inputHashes"]["manifest"]["path"],
            "sha256": sha256_file(context["manifestPath"]),
            "bytes": context["manifestPath"].stat().st_size,
        },
        "sources": [],
        "dats": [],
        "pages": [],
    }
    for source in context["inputHashes"]["sources"]:
        path = Path(source["absolutePath"])
        result["sources"].append({**source, "sha256": sha256_file(path), "bytes": path.stat().st_size})
    for dat in context["inputHashes"]["dats"]:
        path = Path(dat["absolutePath"])
        result["dats"].append({**dat, "sha256": sha256_file(path), "bytes": path.stat().st_size})
    for page in context["inputHashes"]["pages"]:
        path = Path(page["absolutePath"])
        result["pages"].append({**page, "sha256": sha256_file(path), "bytes": path.stat().st_size})
    return result


def page_changes(context: dict[str, Any], candidate: dict[str, Any], groups: dict[str, dict[str, Any]]) -> dict[str, Any]:
    old_pages = page_descriptor_by_id(context["manifest"])
    new_pages = page_descriptor_by_id(candidate)
    changes = []
    for source in context["manifest"]["sources"]:
        path = str(source["path"])
        candidate_source = next(item for item in candidate["sources"] if str(item["path"]) == path)
        old_chunks = source["chunks"]
        new_chunks = candidate_source["chunks"]
        chunk_changes = []
        for old_chunk in old_chunks:
            old_first = int(old_chunk["firstLocalPic"])
            old_last = old_first + int(old_chunk["count"]) - 1
            replacements = []
            for new_chunk in new_chunks:
                new_first = int(new_chunk["firstLocalPic"])
                new_last = new_first + int(new_chunk["count"]) - 1
                if new_last >= old_first and new_first <= old_last:
                    replacement_page = new_pages[str(new_chunk["page"])]
                    if replacement_page not in replacements:
                        replacements.append(replacement_page)
            chunk_changes.append(
                {
                    "old": copy.deepcopy(old_chunk),
                    "newDescriptors": copy.deepcopy(replacements),
                    "newChunks": [
                        copy.deepcopy(new_chunk)
                        for new_chunk in new_chunks
                        if int(new_chunk["firstLocalPic"]) + int(new_chunk["count"]) - 1 >= old_first
                        and int(new_chunk["firstLocalPic"]) <= old_last
                    ],
                }
            )
        changes.append(
            {
                "source": path,
                "oldSourceDescriptor": copy.deepcopy(source),
                "newSourceDescriptor": copy.deepcopy(candidate_source),
                "sourceDescriptorChangedOnlyInChunks": source.get("chunks") != candidate_source.get("chunks") if path in TRIM_SOURCE_SET else source == candidate_source,
                "chunks": chunk_changes,
            }
        )
    page_descriptor_changes = []
    affected: dict[str, dict[str, Any]] = {}
    for source_change in changes:
        path = source_change["source"]
        for chunk_change in source_change["chunks"]:
            old_chunk = chunk_change["old"]
            old_id = str(old_chunk["page"])
            entry = affected.setdefault(
                old_id,
                {
                    "oldDescriptor": copy.deepcopy(old_pages[old_id]),
                    "affectedSources": [],
                    "replacementPageIds": [],
                    "replacementDescriptors": [],
                },
            )
            if path not in entry["affectedSources"]:
                entry["affectedSources"].append(path)
            for descriptor in chunk_change["newDescriptors"]:
                new_id = str(descriptor["id"])
                if new_id not in entry["replacementPageIds"]:
                    entry["replacementPageIds"].append(new_id)
                    entry["replacementDescriptors"].append(copy.deepcopy(descriptor))
    for page_id, old_descriptor in old_pages.items():
        entry = affected.get(page_id)
        if entry is None:
            continue
        entry["oldPageId"] = page_id
        entry["oldDescriptorSha256"] = old_descriptor["sha256"]
        entry["unchanged"] = page_id in context["win2PageIds"]
        page_descriptor_changes.append(entry)
    return {
        "schemaVersion": 1,
        "task": TASK_ID,
        "change": CHANGE_ID,
        "operation": OPERATION_ID,
        "sourceChunkChanges": changes,
        "pageDescriptorChanges": page_descriptor_changes,
        "preservedWin2PageIds": list(context["win2PageIds"]),
        "targetSources": list(TRIM_SOURCES),
    }


def build_report(
    context: dict[str, Any],
    groups: dict[str, dict[str, Any]],
    candidate: dict[str, Any],
    validation: dict[str, Any],
    diff: dict[str, Any],
    output_root: Path,
) -> str:
    old_page_rgba = sum(int(page["width"]) * int(page["height"]) * 4 for page in context["manifest"]["pages"])
    old_page_png = sum(int(item["bytes"]) for item in context["inputHashes"]["pages"])
    new_page_rgba = sum(int(page["width"]) * int(page["height"]) * 4 for page in validation["candidatePages"])
    new_page_png = sum(int(item["bytes"]) for item in validation["candidatePages"])
    target_page_rgba = sum(int(page["width"]) * int(page["height"]) * 4 for group in groups.values() for page in group["pages"])
    target_page_png = sum(int(page["bytes"]) for group in groups.values() for page in group["pages"])
    target_crop_rgba = sum(int(record["rgbaBytes"]) for group in groups.values() for record in group["records"])
    lines = [
        "# NTSD M12 transparent trim candidate",
        "",
        "Status: `PASS`（staging-only 离线候选；未运行 Unity、Play、P1、ASTC 或 device）",
        "",
        f"Task: `{TASK_ID}`  ",
        f"Change: `{CHANGE_ID}`  ",
        f"Operation: `{OPERATION_ID}`",
        "",
        "## 输入冻结与保护",
        "",
        f"- 输入 manifest: `{context['manifestPath']}`",
        f"- 输入 manifest SHA-256: `{context['inputHashes']['manifest']['sha256']}`",
        f"- 输入 source/DAT/page hash 核对：`PASS`（4 source/DAT bindings，{len(context['inputHashes']['pages'])} old pages）",
        f"- native stride：按每个 source 的 `(frameWidth+1, frameHeight+1)` 取 88 个逻辑帧；`full RGBA` 重建：`{validation['fullRgbaReconstructionExactFrames']}/{validation['nativeLogicalFrameCount']}` exact",
        f"- 原 PNG、DAT、旧 manifest、旧 page 执行后 hash 未变：`{validation['protectedInputsUnchanged']}`",
        "- win2 descriptor 与 page bytes 原样复制并校验；候选 manifest 未保留未使用的旧 Kirin/Sui page descriptor。原 Assets 旧 pages 未改未删。",
        "",
        "## 生成结果",
        "",
        f"- 候选输出根：`{output_root}`",
        f"- 候选 manifest：`{validation['candidateManifest']['path']}`，SHA-256 `{validation['candidateManifest']['sha256']}`",
        f"- 候选实际页数：`{len(validation['candidatePages'])}`（win2 原样 `{len(context['win2PageIds'])}` + trim 新页 `{sum(len(group['pages']) for group in groups.values())}`）",
        f"- 候选 page RGBA bytes：`{new_page_rgba}`；page PNG bytes：`{new_page_png}`",
        f"- trim 新页 RGBA bytes：`{target_page_rgba}`；trim 新页 PNG bytes：`{target_page_png}`",
        f"- 旧全量 page RGBA bytes（仅比较基线）：`{old_page_rgba}`；旧 page PNG bytes：`{old_page_png}`",
        f"- trim 后 source crop raw RGBA bytes（逻辑帧总和）：`{target_crop_rgba}`",
        "",
        "| source | native frames | physical tiles | exact aliases | new pages | crop RGBA bytes |",
        "|---|---:|---:|---:|---:|---:|",
    ]
    for source_path_value, group in groups.items():
        lines.append(
            f"| `{source_path_value}` | {len(group['records'])} | {group['physicalFrameCount']} | {group['aliasFrameCount']} | {len(group['pages'])} | {sum(int(record['rgbaBytes']) for record in group['records'])} |"
        )
    lines.extend(
        [
            "",
            "## trim bbox",
            "",
            "`trim-bboxes` 以 top-left source frame bbox 记录非零 RGBA 范围；`contentRect` 是同一范围在原始逻辑 canvas 内的 bottom-left 坐标。alpha 为 0 但 RGB 非零的像素按非零 RGBA 保留；全空帧使用 1x1 RGBA zero。完整逐帧记录见 `validation.json` 的 `trimValidation`。",
            "",
        ]
    )
    for source_path_value, group in groups.items():
        lines.append(f"### `{source_path_value}`")
        lines.append("")
        lines.append("| localPic | bbox top-left | contentRect bottom-left | crop | crop SHA-256 |")
        lines.append("|---:|---|---|---|---|")
        for record in group["records"]:
            bbox = record["nonZeroBboxTopLeft"] if record["nonZeroBboxTopLeft"] is not None else "empty -> 1x1 zero"
            lines.append(
                f"| {record['localPic']} | `{bbox}` | `{record['contentRect']}` | {record['width']}x{record['height']} | `{record['cropSha256']}` |"
            )
        lines.append("")
    lines.extend(
        [
            "## descriptor mapping",
            "",
            "逐项旧 page descriptor 到新 descriptor/chunk 的详细映射见 `descriptor-diff.json`。candidate manifest 保持顶层 `version=1`、`generatorVersion=NTSD_DAT_PAGES_V1`；win2 chunk 不增加 `contentRects`，因此旧格式仍按 full canvas 默认语义兼容。Kirin/Sui chunk 的 `rects` 是实际 crop 尺寸，`contentRects` 是原逻辑 canvas 坐标。",
            "",
            "## 实际命令与证据",
            "",
            "- helper 运行命令由 `receipt.json` 的 `command` 字段记录（Python executable、argv、cwd）。",
            "- `validation.json`：输入 hash、候选 page SHA/header、88/88 full RGBA 重建、page edge extrusion、4 对齐 allocation、win2 原样保护和输入不变性。",
            "- `descriptor-diff.json`：旧 descriptor -> 新 descriptor/chunk 的逐项映射。",
            "- 本任务未运行 old helper、P1、ASTC、Unity、Play、build 或 device 验收。",
            "",
        ]
    )
    return "\n".join(lines)


def build_receipt(context: dict[str, Any], output_root: Path, candidate: dict[str, Any], validation: dict[str, Any]) -> dict[str, Any]:
    return {
        "schemaVersion": 1,
        "task": TASK_ID,
        "change": CHANGE_ID,
        "operation": OPERATION_ID,
        "status": validation["status"],
        "command": {
            "pythonExecutable": sys.executable,
            "argv": list(sys.argv),
            "cwd": str(Path.cwd()),
            "script": str(Path(__file__).resolve()),
        },
        "inputManifest": context["inputHashes"]["manifest"],
        "inputHashesPath": "input-hashes.json",
        "outputRoot": str(output_root),
        "candidateManifest": validation["candidateManifest"],
        "candidatePageCount": len(candidate["pages"]),
        "nativeLogicalFrameCount": validation["nativeLogicalFrameCount"],
        "fullRgbaReconstructionExact": validation["fullRgbaReconstructionExact"],
        "protection": {
            "originalPngDatOldPagesUnchanged": validation["protectedInputsUnchanged"],
            "win2DescriptorsAndPagesExact": bool(validation["win2Preserved"]),
            "collisionPolicy": "all candidate files create-new; existing output root is fatal",
        },
        "forbiddenStagesNotRun": ["oldhelper", "P1", "ASTC", "Unity", "Play", "build", "device"],
    }


def ensure_output_root_is_new(output_root: Path) -> None:
    output_root = output_root.resolve()
    try:
        output_root.relative_to(ARTIFACT_ROOT.resolve())
    except ValueError:
        fail(f"输出目录必须位于 M12 artifact 根目录内: {output_root}")
    if output_root == ARTIFACT_ROOT.resolve():
        fail("禁止把候选输出写到 M12 artifact 根目录")
    if output_root.exists():
        fail(f"输出目录已存在，拒绝覆盖/清理: {output_root}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--manifest", type=Path, default=DEFAULT_MANIFEST)
    parser.add_argument("--output-root", type=Path, default=DEFAULT_OUTPUT_ROOT)
    return parser.parse_args()


def run() -> int:
    args = parse_args()
    manifest_path = args.manifest.resolve()
    output_root = args.output_root.resolve()
    ensure_output_root_is_new(output_root)
    context = load_and_validate_inputs(manifest_path)

    source_arrays = context["sourceArrays"]
    kirin_records = make_trim_records(context["sourceByPath"][KIRIN_SOURCE], source_arrays[KIRIN_SOURCE])
    sui_records = make_trim_records(context["sourceByPath"][SUI_SOURCES[0]], source_arrays[SUI_SOURCES[0]])
    groups = {
        KIRIN_SOURCE: build_trim_group(str(context["sourceByPath"][KIRIN_SOURCE]["sha256"]), kirin_records, "kirin"),
        SUI_SOURCES[0]: build_trim_group(str(context["sourceByPath"][SUI_SOURCES[0]]["sha256"]), sui_records, "shared-sui"),
    }
    # The second Sui source intentionally reuses exactly the same group/pages/chunks.
    groups[SUI_SOURCES[1]] = groups[SUI_SOURCES[0]]
    candidate = build_candidate_manifest(context, groups)

    # Candidate page files are the eight original win2 files plus new physical trim pages.
    for page_id in context["win2PageIds"]:
        page = context["pageById"][page_id]
        write_new_bytes(output_path(output_root, str(page["path"])), deployed_page_path(str(page["path"])).read_bytes())
    for group in (groups[KIRIN_SOURCE], groups[SUI_SOURCES[0]]):
        for page in group["pages"]:
            write_new_bytes(output_path(output_root, page["path"]), page["payload"])
    write_new_json(output_path(output_root, "manifest.json"), candidate)

    validation = validate_candidate(context, candidate, output_root, {KIRIN_SOURCE: groups[KIRIN_SOURCE], SUI_SOURCES[0]: groups[SUI_SOURCES[0]]})
    diff = page_changes(context, candidate, {KIRIN_SOURCE: groups[KIRIN_SOURCE], SUI_SOURCES[0]: groups[SUI_SOURCES[0]]})
    write_new_json(output_path(output_root, "input-hashes.json"), context["inputHashes"])
    write_new_json(output_path(output_root, "validation.json"), validation)
    write_new_json(output_path(output_root, "descriptor-diff.json"), diff)
    receipt = build_receipt(context, output_root, candidate, validation)
    write_new_json(output_path(output_root, "receipt.json"), receipt)
    write_new_text(output_path(output_root, "REPORT.md"), build_report(context, {KIRIN_SOURCE: groups[KIRIN_SOURCE], SUI_SOURCES[0]: groups[SUI_SOURCES[0]]}, candidate, validation, diff, output_root))

    print(json.dumps({
        "status": validation["status"],
        "outputRoot": str(output_root),
        "manifest": validation["candidateManifest"],
        "candidatePageCount": len(candidate["pages"]),
        "newTrimPages": sum(len(group["pages"]) for group in (groups[KIRIN_SOURCE], groups[SUI_SOURCES[0]])),
        "nativeLogicalFrameCount": validation["nativeLogicalFrameCount"],
        "fullRgbaReconstructionExact": validation["fullRgbaReconstructionExact"],
        "newPageRgbaBytes": sum(item["rgbaBytes"] for item in validation["candidatePages"]),
        "newPagePngBytes": sum(item["bytes"] for item in validation["candidatePages"]),
    }, ensure_ascii=False, indent=2))
    return 0


def main() -> int:
    try:
        return run()
    except TrimError as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 2
    except Exception as error:  # pragma: no cover - defensive bounded failure output
        print(f"ERROR: {type(error).__name__}: {error}", file=sys.stderr)
        return 3


if __name__ == "__main__":
    raise SystemExit(main())
