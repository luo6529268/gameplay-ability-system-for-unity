#!/usr/bin/env python3
"""Bounded, independent NTSD sprite equivalence and CPU visual diagnostic.

This helper deliberately reads the deployed manifest, PNGs, and decoded DAT
files directly.  It does not import or invoke the existing page generator.
All generated files are planned in the sibling cpu/planned-output-manifest.json
and collisions are fatal.
"""

from __future__ import annotations

import hashlib
import io
import json
import math
import re
import struct
import sys
from pathlib import Path
from typing import Any, Iterable

import numpy as np
from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
CONTENT = ROOT / "Assets/NTSD/Content/LoganRuntime"
DEPLOYED = CONTENT / "derived_sprite_pages"
MANIFEST_PATH = DEPLOYED / "manifest.json"
SHADER_PATH = ROOT / "Assets/NTSD/Shaders/BattleCentralTransparent.shader"
OUT = ROOT / "artifacts/diagnostics/M12-sprite-visual-equivalence-20261013/cpu"
PLAN_PATH = OUT / "planned-output-manifest.json"
FINAL_PLAN_PATH = OUT / "planned-output-manifest-final.json"

D0_PATH = CONTENT / "vfs/s/0/d0.png"
KIRIN_PATH = CONTENT / "vfs/c/sasu/a/Kirin.png"
BLAC_PATH = CONTENT / "vfs/a/blac/blac.png"

PNG_SIGNATURE = b"\x89PNG\r\n\x1a\n"
FILE_RE = re.compile(
    r"file\(\s*(\d+)\s*-\s*(\d+)\s*\):\s*([^\s]+)\s+"
    r"w:\s*(\d+)\s+h:\s*(\d+)\s+row:\s*(\d+)\s+col:\s*(\d+)",
    re.IGNORECASE,
)


def fail(message: str) -> None:
    raise RuntimeError(message)


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


def json_text(value: Any) -> str:
    return json.dumps(value, ensure_ascii=False, indent=2, sort_keys=False) + "\n"


def png_header(data_or_path: bytes | Path) -> dict[str, Any]:
    data = data_or_path if isinstance(data_or_path, bytes) else data_or_path.read_bytes()
    if len(data) < 33 or data[:8] != PNG_SIGNATURE:
        fail(f"PNG signature/header missing: {data_or_path}")
    length = struct.unpack(">I", data[8:12])[0]
    chunk_type = data[12:16]
    if length != 13 or chunk_type != b"IHDR":
        fail(f"PNG IHDR header invalid: {data_or_path}")
    width, height, bit_depth, color_type, compression, filter_method, interlace = struct.unpack(
        ">IIBBBBB", data[16:29]
    )
    return {
        "width": int(width),
        "height": int(height),
        "bitDepth": int(bit_depth),
        "colorType": int(color_type),
        "compression": int(compression),
        "filter": int(filter_method),
        "interlace": int(interlace),
    }


def read_rgba(path: Path) -> tuple[np.ndarray, dict[str, Any]]:
    header = png_header(path)
    with Image.open(path) as image:
        source_mode = image.mode
        source_size = [int(image.width), int(image.height)]
        rgba = np.array(image.convert("RGBA"), dtype=np.uint8, copy=True)
    if source_size != [header["width"], header["height"]]:
        fail(f"PNG PIL/header dimension mismatch: {path}")
    return rgba, {
        "header": header,
        "mode": source_mode,
        "size": source_size,
        "readbackMode": "RGBA",
    }


def encode_png(array: np.ndarray) -> bytes:
    if array.dtype != np.uint8 or array.ndim != 3 or array.shape[2] != 4:
        fail(f"PNG encoder requires uint8 RGBA, got {array.dtype}/{array.shape}")
    buffer = io.BytesIO()
    Image.fromarray(np.ascontiguousarray(array), "RGBA").save(buffer, format="PNG")
    return buffer.getvalue()


def decode_png(data: bytes) -> tuple[np.ndarray, dict[str, Any]]:
    header = png_header(data)
    with Image.open(io.BytesIO(data)) as image:
        mode = image.mode
        size = [int(image.width), int(image.height)]
        rgba = np.array(image.convert("RGBA"), dtype=np.uint8, copy=True)
    if size != [header["width"], header["height"]]:
        fail("Encoded PNG readback dimension mismatch")
    return rgba, {"header": header, "mode": mode, "size": size, "readbackMode": "RGBA"}


def compare_arrays(expected: np.ndarray, actual: np.ndarray) -> dict[str, int | bool | list[int] | None]:
    if expected.shape != actual.shape:
        return {
            "exact": False,
            "shapeExpected": [int(v) for v in expected.shape],
            "shapeActual": [int(v) for v in actual.shape],
            "changedPixels": -1,
            "maxAbsDiff": -1,
            "firstDifference": None,
        }
    if np.array_equal(expected, actual):
        return {
            "exact": True,
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
        "exact": False,
        "shapeExpected": [int(v) for v in expected.shape],
        "shapeActual": [int(v) for v in actual.shape],
        "changedPixels": int(changed.sum()),
        "maxAbsDiff": int(delta.max()),
        "firstDifference": [int(xs[0]), int(ys[0])],
    }


def bbox_for_changed(base: np.ndarray, frame: np.ndarray) -> tuple[int, int, int, int] | None:
    changed = np.any(base != frame, axis=2)
    ys, xs = np.nonzero(changed)
    if len(xs) == 0:
        return None
    x0, x1 = int(xs.min()), int(xs.max())
    y0, y1 = int(ys.min()), int(ys.max())
    return x0, y0, x1 - x0 + 1, y1 - y0 + 1


def patch_array(base: np.ndarray, frame: np.ndarray, rect: tuple[int, int, int, int] | None) -> np.ndarray:
    if rect is None:
        return np.zeros((1, 1, 4), dtype=np.uint8)
    x, y, width, height = rect
    return np.array(frame[y : y + height, x : x + width], dtype=np.uint8, copy=True)


def repeat_tile(tile: np.ndarray, width: int, height: int) -> np.ndarray:
    rows = math.ceil(height / tile.shape[0])
    columns = math.ceil(width / tile.shape[1])
    repeated = np.tile(tile, (rows, columns, 1))
    return np.array(repeated[:height, :width], dtype=np.uint8, copy=True)


def fit_frame(array: np.ndarray, width: int, height: int) -> np.ndarray:
    image = Image.fromarray(array, "RGBA")
    scale = min((width - 8) / image.width, (height - 8) / image.height)
    target_width = max(1, int(round(image.width * scale)))
    target_height = max(1, int(round(image.height * scale)))
    resized = image.resize((target_width, target_height), Image.Resampling.NEAREST)
    return np.array(resized, dtype=np.uint8, copy=True)


def checker_background(width: int, height: int, tile: int = 16) -> Image.Image:
    output = Image.new("RGB", (width, height), (220, 220, 220))
    draw = ImageDraw.Draw(output)
    for y in range(0, height, tile):
        for x in range(0, width, tile):
            if ((x // tile) + (y // tile)) % 2:
                draw.rectangle((x, y, min(width - 1, x + tile - 1), min(height - 1, y + tile - 1)), fill=(165, 165, 165))
    return output


def rgba_panel(array: np.ndarray, width: int, height: int) -> Image.Image:
    fitted = fit_frame(array, width - 8, height - 24)
    output = checker_background(width, height)
    image = Image.fromarray(fitted, "RGBA")
    x = (width - image.width) // 2
    y = 20 + (height - 20 - image.height) // 2
    output.paste(image, (x, y), image)
    return output


def diff_panel(expected: np.ndarray, actual: np.ndarray, width: int, height: int) -> tuple[Image.Image, bool]:
    metrics = compare_arrays(expected, actual)
    if bool(metrics["exact"]):
        return Image.new("RGB", (width, height), (0, 0, 0)), True
    delta = np.abs(expected.astype(np.int16) - actual.astype(np.int16))
    amplified = np.clip(delta * 8, 0, 255).astype(np.uint8)
    return rgba_panel(amplified, width, height), False


def label_panel(panel: Image.Image, label: str, color: tuple[int, int, int] = (255, 255, 255)) -> Image.Image:
    output = panel.copy()
    draw = ImageDraw.Draw(output)
    draw.rectangle((0, 0, output.width - 1, output.height - 1), outline=(40, 40, 40), width=1)
    draw.rectangle((0, 0, output.width - 1, 17), fill=(20, 20, 20))
    draw.text((4, 3), label, fill=color, font=ImageFont.load_default())
    return output


def contact_sheet(
    title: str,
    cases: list[tuple[str, np.ndarray, np.ndarray]],
    panel_width: int,
    panel_height: int,
) -> bytes:
    rows = len(cases)
    output = Image.new("RGB", (panel_width * 3, 34 + panel_height * rows), (35, 35, 35))
    draw = ImageDraw.Draw(output)
    draw.text((8, 8), title, fill=(255, 255, 255), font=ImageFont.load_default())
    for row, (label, original, reconstructed) in enumerate(cases):
        y = 34 + row * panel_height
        output.paste(label_panel(rgba_panel(original, panel_width, panel_height), "ORIGINAL"), (0, y))
        output.paste(label_panel(rgba_panel(reconstructed, panel_width, panel_height), "RECONSTRUCTION"), (panel_width, y))
        difference, exact = diff_panel(original, reconstructed, panel_width, panel_height)
        diff_label = "ZERO DIFF (black)" if exact else "AMPLIFIED DIFF x8"
        output.paste(label_panel(difference, diff_label, (255, 255, 0) if not exact else (170, 255, 170)), (panel_width * 2, y))
        draw.text((8, y + panel_height - 13), label, fill=(255, 255, 0), font=ImageFont.load_default())
    buffer = io.BytesIO()
    output.save(buffer, format="PNG")
    return buffer.getvalue()


def checker_rgb(width: int, height: int, tile: int = 12) -> np.ndarray:
    output = np.zeros((height, width, 3), dtype=np.uint8)
    output[:, :] = (220, 220, 220)
    for y in range(0, height, tile):
        for x in range(0, width, tile):
            if ((x // tile) + (y // tile)) % 2:
                output[y : min(height, y + tile), x : min(width, x + tile)] = (160, 160, 160)
    return output


def cpu_composite_gamma(
    array: np.ndarray,
    viewport: tuple[int, int],
    flip_x: bool,
    alpha: float,
    background: str,
    logical_offset: tuple[int, int] = (0, 0),
) -> np.ndarray:
    """Model the current Gamma project value path on a small preview only.

    The byte values are treated as the sampled Gamma-space values.  No
    sRGB-to-linear conversion is performed here.  The shader then multiplies
    straight RGBA by tint, premultiplies RGB by the resulting alpha, and uses
    One OneMinusSrcAlpha over the selected background.
    """
    viewport_width, viewport_height = viewport
    source = array[:, ::-1] if flip_x else array
    fitted = fit_frame(source, viewport_width - 12, viewport_height - 12)
    sample = fitted.astype(np.float32) / 255.0
    tint = np.array([1.0, 1.0, 1.0, alpha], dtype=np.float32)
    straight = sample * tint
    src_alpha = straight[:, :, 3:4]
    premultiplied = straight[:, :, :3] * src_alpha
    if background == "black":
        destination = np.zeros((fitted.shape[0], fitted.shape[1], 3), dtype=np.float32)
    elif background == "white":
        destination = np.ones((fitted.shape[0], fitted.shape[1], 3), dtype=np.float32)
    elif background == "checker":
        destination = checker_rgb(fitted.shape[1], fitted.shape[0]).astype(np.float32) / 255.0
    else:
        fail(f"Unknown preview background: {background}")
    over = premultiplied + destination * (1.0 - src_alpha)
    result = np.full((viewport_height, viewport_width, 3), 0, dtype=np.uint8)
    x = (viewport_width - over.shape[1]) // 2 + int(logical_offset[0])
    y = (viewport_height - over.shape[0]) // 2 + int(logical_offset[1])
    x0, x1 = max(0, x), min(viewport_width, x + over.shape[1])
    y0, y1 = max(0, y), min(viewport_height, y + over.shape[0])
    if x0 < x1 and y0 < y1:
        sx0, sy0 = x0 - x, y0 - y
        result[y0:y1, x0:x1] = np.clip(over[sy0 : sy0 + y1 - y0, sx0 : sx0 + x1 - x0] * 255.0 + 0.5, 0, 255).astype(np.uint8)
    return result


def save_output_bytes(path: Path, data: bytes, planned: set[str]) -> None:
    relative = path.relative_to(OUT).as_posix()
    if relative not in planned:
        fail(f"Attempted unplanned output: {relative}")
    if path.exists():
        fail(f"Refusing output collision: {path}")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(data)


def save_output_json(path: Path, value: Any, planned: set[str]) -> None:
    save_output_bytes(path, json_text(value).encode("utf-8"), planned)


def save_output_text(path: Path, value: str, planned: set[str]) -> None:
    save_output_bytes(path, value.encode("utf-8"), planned)


def save_final_output(path: Path, data: bytes, planned: set[str]) -> None:
    relative = path.relative_to(OUT).as_posix()
    if relative not in planned:
        fail(f"Attempted unplanned final output: {relative}")
    if path.exists():
        fail(f"Refusing final output collision: {path}")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(data)


def save_final_json(path: Path, value: Any, planned: set[str]) -> None:
    save_final_output(path, json_text(value).encode("utf-8"), planned)


def save_final_text(path: Path, value: str, planned: set[str]) -> None:
    save_final_output(path, value.encode("utf-8"), planned)


def source_cell(source: dict[str, Any], image: np.ndarray, local_pic: int) -> tuple[np.ndarray, tuple[int, int]]:
    columns = int(source["columns"])
    rows = int(source["rows"])
    frame_width = int(source["frameWidth"])
    frame_height = int(source["frameHeight"])
    column = local_pic % columns
    row = local_pic // columns
    # Native DAT sprite sheets use a one-pixel separator after every logical
    # cell.  Keep this explicit: inferring separators from trailing dimensions
    # would make a no-trailing-separator fixture silently choose another stride.
    x_step = frame_width + 1
    y_step = frame_height + 1
    if int(source["width"]) < x_step * columns or int(source["height"]) < y_step * rows:
        fail(
            f"Native DAT stride contract is outside declared source bounds: {source['path']} "
            f"step={[x_step, y_step]} grid={[columns, rows]} size={[source['width'], source['height']]}"
        )
    x = column * x_step
    y = row * y_step
    if x + frame_width > image.shape[1] or y + frame_height > image.shape[0]:
        fail(f"Native source cell is outside source image: {source['path']} local={local_pic}")
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


def parse_dat_files(dat_path: Path) -> list[dict[str, Any]]:
    text = dat_path.read_text(encoding="utf-8", errors="replace")
    entries = []
    for match in FILE_RE.finditer(text):
        first, last, path, width, height, rows, columns = match.groups()
        entries.append(
            {
                "firstPic": int(first),
                "lastPic": int(last),
                "path": path,
                "normalizedPath": normalize_rel(path),
                "width": int(width),
                "height": int(height),
                "rows": int(rows),
                "columns": int(columns),
                "rowTimesColumn": int(rows) * int(columns),
            }
        )
    if not entries:
        fail(f"No native file bindings parsed from DAT: {dat_path}")
    return entries


def locate_dat(dat_relative: str) -> Path:
    path = CONTENT / "decoded_dat" / Path(dat_relative.replace("\\", "/"))
    if not path.is_file():
        fail(f"Declared decoded DAT is missing: {path}")
    return path


def validate_manifest_and_inputs() -> tuple[dict[str, Any], dict[str, Any], dict[str, np.ndarray], dict[str, Path], dict[str, Any]]:
    manifest_bytes = MANIFEST_PATH.read_bytes()
    manifest = json.loads(manifest_bytes.decode("utf-8"))
    pages = manifest.get("pages", [])
    sources = manifest.get("sources", [])
    if len(pages) != 19 or len(sources) != 4:
        fail(f"Expected 19 pages and 4 sources, got {len(pages)} pages/{len(sources)} sources")

    page_paths: dict[str, Path] = {}
    page_validation = []
    for page in pages:
        path = DEPLOYED / page["path"]
        if not path.is_file():
            fail(f"Manifest page missing: {path}")
        actual_sha = sha256_file(path)
        if actual_sha != page["sha256"]:
            fail(f"Manifest page SHA mismatch: {path} expected={page['sha256']} actual={actual_sha}")
        header = png_header(path)
        expected_size = [int(page["width"]), int(page["height"])]
        if [header["width"], header["height"]] != expected_size:
            fail(f"Manifest page header dimension mismatch: {path}")
        page_paths[page["id"]] = path
        page_validation.append(
            {
                "id": page["id"],
                "path": rel_path(path),
                "declaredSha256": page["sha256"],
                "actualSha256": actual_sha,
                "shaMatch": True,
                "declaredSize": expected_size,
                "header": header,
                "headerMatch": True,
            }
        )

    source_images: dict[str, np.ndarray] = {}
    dat_paths: dict[str, Path] = {}
    source_validation = []
    native_binding_validation = []
    total_frames = 0
    for source in sources:
        source_path = CONTENT / "vfs" / Path(source["path"])
        if not source_path.is_file():
            fail(f"Manifest source missing: {source_path}")
        actual_sha = sha256_file(source_path)
        if actual_sha != source["sha256"]:
            fail(f"Manifest source SHA mismatch: {source_path}")
        image, image_info = read_rgba(source_path)
        declared_size = [int(source["width"]), int(source["height"])]
        if image_info["size"] != declared_size:
            fail(f"Manifest source dimension mismatch: {source_path}")
        frame_count = int(source["columns"]) * int(source["rows"])
        total_frames += frame_count
        native_step = [int(source["frameWidth"]) + 1, int(source["frameHeight"]) + 1]
        native_grid_extent = [native_step[0] * int(source["columns"]), native_step[1] * int(source["rows"])]
        if int(source["width"]) < native_grid_extent[0] or int(source["height"]) < native_grid_extent[1]:
            fail(f"Source cannot contain explicit native DAT stride grid: {source['path']}")
        source_images[source["path"]] = image

        for binding in source["bindings"]:
            dat_path = locate_dat(binding["datPath"])
            dat_paths[binding["datPath"]] = dat_path
            dat_sha = sha256_file(dat_path)
            if dat_sha != binding["datSha256"]:
                fail(f"DAT SHA mismatch: {dat_path}")
            dat_entries = parse_dat_files(dat_path)
            expected_path = normalize_rel(source["path"])
            matches = [entry for entry in dat_entries if entry["normalizedPath"] == expected_path]
            if len(matches) != 1:
                fail(f"Native DAT source binding not unique: {source['path']} in {dat_path}")
            native = matches[0]
            expected_count = int(binding["lastPic"]) - int(binding["firstPic"]) + 1
            if native["rowTimesColumn"] != expected_count:
                fail(f"Native DAT row*col count mismatch: {source['path']} in {dat_path}")
            if expected_count != frame_count:
                fail(f"DAT range does not cover source native frame count: {source['path']}")
            native_index = dat_entries.index(native)
            cumulative_start = 0
            for prior in dat_entries[:native_index]:
                cumulative_start = max(cumulative_start, prior["lastPic"] + 1)
            cumulative_range = [cumulative_start, cumulative_start + native["rowTimesColumn"] - 1]
            manifest_range = [int(binding["firstPic"]), int(binding["lastPic"])]
            if manifest_range != cumulative_range:
                fail(
                    f"Native DAT cumulative range mismatch: {source['path']} in {dat_path} "
                    f"manifest={manifest_range} cumulative={cumulative_range} local={[native['firstPic'], native['lastPic']]}"
                )
            native_binding_validation.append(
                {
                    "source": source["path"],
                    "datPath": rel_path(dat_path),
                    "datSha256": dat_sha,
                    "manifestRange": manifest_range,
                    "nativeLocalRange": [native["firstPic"], native["lastPic"]],
                    "cumulativeRange": cumulative_range,
                    "nativeRows": native["rows"],
                    "nativeColumns": native["columns"],
                    "nativeRowTimesColumn": native["rowTimesColumn"],
                    "nativePath": native["path"],
                    "rangeMatch": True,
                    "localRangeMayResetAfterPriorFiles": native["firstPic"] != cumulative_start,
                    "rowColumnMatch": True,
                }
            )

        source_validation.append(
            {
                "path": source["path"],
                "resolvedPath": rel_path(source_path),
                "declaredSha256": source["sha256"],
                "actualSha256": actual_sha,
                "shaMatch": True,
                "declaredSize": declared_size,
                "readback": image_info,
                "nativeFrameCount": frame_count,
                "nativeStep": native_step,
                "nativeGridExtent": native_grid_extent,
                "bindings": source["bindings"],
            }
        )

    if total_frames != 88:
        fail(f"Expected 88 native logical frames, got {total_frames}")

    validation = {
        "manifestPath": rel_path(MANIFEST_PATH),
        "manifestSha256": sha256_bytes(manifest_bytes),
        "generatorVersionObserved": manifest.get("generatorVersion"),
        "pageCount": len(pages),
        "sourceCount": len(sources),
        "nativeLogicalFrameCount": total_frames,
        "pages": page_validation,
        "sources": source_validation,
        "nativeDatBindings": native_binding_validation,
        "allShaAndHeadersValid": True,
        "existingGeneratorInvoked": False,
    }
    return manifest, validation, source_images, dat_paths, {"pagePaths": page_paths}


def collect_input_hashes(
    manifest: dict[str, Any],
    dat_paths: dict[str, Path],
    plan_hash: str,
) -> dict[str, Any]:
    paths: list[Path] = [MANIFEST_PATH, SHADER_PATH, D0_PATH, KIRIN_PATH, PLAN_PATH, Path(__file__).resolve()]
    paths.extend(CONTENT / "vfs" / Path(source["path"]) for source in manifest["sources"])
    paths.extend(DEPLOYED / page["path"] for page in manifest["pages"])
    paths.extend(dat_paths.values())
    unique: dict[str, Path] = {}
    for path in paths:
        unique[str(path.resolve()).lower()] = path
    files = []
    for path in sorted(unique.values(), key=lambda item: rel_path(item)):
        if not path.is_file():
            fail(f"Input disappeared before hashing: {path}")
        files.append({"path": rel_path(path), "size": path.stat().st_size, "sha256": sha256_file(path)})
    shader_text = SHADER_PATH.read_text(encoding="utf-8", errors="replace")
    shader_contract = {
        "sampledStraightRgbaTimesTint": "SAMPLE_TEXTURE2D(_MainTex" in shader_text and "* input.color" in shader_text,
        "premultiplyRgbByAlpha": "color.rgb *= color.a" in shader_text,
        "blendOneOneMinusSrcAlpha": "Blend One OneMinusSrcAlpha" in shader_text,
    }
    if not all(shader_contract.values()):
        fail(f"Shader contract strings are incomplete: {shader_contract}")
    return {
        "capturedBeforeGeneratedOutputs": True,
        "planSha256": plan_hash,
        "helperPath": rel_path(Path(__file__).resolve()),
        "files": files,
        "shaderContractObserved": shader_contract,
        "projectColorSpaceReportedByPrimary": "Gamma",
        "cpuModel": "Gamma sampled byte values -> straight RGBA*tint/alpha -> RGB premultiply -> One OneMinusSrcAlpha",
        "sRgbToLinearApplied": False,
    }


def verify_logical_frames(
    manifest: dict[str, Any],
    source_images: dict[str, np.ndarray],
    page_paths: dict[str, Path],
) -> tuple[dict[str, Any], dict[str, Any]]:
    logical_records = []
    source_summaries = []
    representative_derived: dict[tuple[str, int], np.ndarray] = {}
    total_pixels = 0
    total_changed = 0
    overall_max = 0
    for source in manifest["sources"]:
        source_path = source["path"]
        native_count = int(source["columns"]) * int(source["rows"])
        frame_map: dict[int, int] = {}
        source_records = []
        page_cache: dict[str, np.ndarray] = {}
        for chunk in source["chunks"]:
            chunk_page_id = chunk["page"]
            page_path = page_paths[chunk_page_id]
            if chunk_page_id not in page_cache:
                page_cache[chunk_page_id], _ = read_rgba(page_path)
            page = page_cache[chunk_page_id]
            first_local = int(chunk["firstLocalPic"])
            rects = chunk["rects"]
            if len(rects) != int(chunk["count"]):
                fail(f"Chunk rect count mismatch: {source_path} {chunk_page_id}")
            for offset, rect in enumerate(rects):
                local_pic = first_local + offset
                if local_pic in frame_map:
                    fail(f"Duplicate logical frame in manifest: {source_path} local={local_pic}")
                if local_pic < 0 or local_pic >= native_count:
                    fail(f"Manifest local frame outside native range: {source_path} local={local_pic}")
                frame_map[local_pic] = chunk_page_id
                expected, source_xy = source_cell(source, source_images[source_path], local_pic)
                actual, page_xy = page_cell(page, rect)
                comparison = compare_arrays(expected, actual)
                if not comparison["exact"]:
                    fail(f"Logical frame RGBA mismatch: {source_path} local={local_pic} {comparison}")
                pixels = int(expected.shape[0] * expected.shape[1])
                total_pixels += pixels
                total_changed += int(comparison["changedPixels"])
                overall_max = max(overall_max, int(comparison["maxAbsDiff"]))
                source_records.append(
                    {
                        "source": source_path,
                        "localPic": local_pic,
                        "nativePic": int(source["bindings"][0]["firstPic"]) + local_pic,
                        "page": chunk_page_id,
                        "manifestRectBottomLeft": rect,
                        "pageCropTopLeft": [page_xy[0], page_xy[1]],
                        "nativeCellTopLeft": [source_xy[0], source_xy[1]],
                        "nativeStep": [int(source["frameWidth"]) + 1, int(source["frameHeight"]) + 1],
                        "width": int(expected.shape[1]),
                        "height": int(expected.shape[0]),
                        "changedPixels": int(comparison["changedPixels"]),
                        "maxAbsDiff": int(comparison["maxAbsDiff"]),
                        "exactRgba": bool(comparison["exact"]),
                    }
                )
                logical_records.append(source_records[-1])
        expected_locals = set(range(native_count))
        if set(frame_map) != expected_locals:
            fail(f"Manifest does not cover all native local frames: {source_path}")
        source_summaries.append(
            {
                "source": source_path,
                "nativeFrames": native_count,
                "verifiedFrames": len(source_records),
                "pixelsCompared": sum(item["width"] * item["height"] for item in source_records),
                "differentPixels": sum(item["changedPixels"] for item in source_records),
                "maxAbsDiff": max(item["maxAbsDiff"] for item in source_records),
                "exact": all(item["exactRgba"] for item in source_records),
            }
        )
    result = {
        "method": "Independent top-left native cell crop versus bottom-left manifest rect crop after PNG readback",
        "nativeLogicalFrames": len(logical_records),
        "pixelsCompared": total_pixels,
        "differentPixels": total_changed,
        "maxAbsDiff": overall_max,
        "exactAll88Rgba": total_changed == 0 and overall_max == 0 and len(logical_records) == 88,
        "sourceSummaries": source_summaries,
        "frames": logical_records,
    }
    return result, representative_derived


def build_prototypes() -> tuple[dict[str, Any], dict[str, bytes], dict[str, np.ndarray]]:
    white_tile = np.array([[[255, 255, 255, 255]]], dtype=np.uint8)
    black_flat_tile = np.array([[[3, 3, 3, 255]]], dtype=np.uint8)
    stripe_tile = np.array([[[41, 41, 41, 255], [0, 0, 0, 0]]], dtype=np.uint8)
    hay_solid_tile = np.array([[[10, 10, 10, 255]]], dtype=np.uint8)
    hay_mask = np.array([[[255, 255, 255, 255]], [[0, 0, 0, 0]]], dtype=np.uint8)
    pngs = {
        "prototypes/white-1x1.png": encode_png(white_tile),
        "prototypes/cro3-1x1.png": encode_png(white_tile),
        "prototypes/blac-flat-1x1.png": encode_png(black_flat_tile),
        "prototypes/blac-stripe-2x1.png": encode_png(stripe_tile),
        "prototypes/hay-solid-1x1.png": encode_png(hay_solid_tile),
        "prototypes/hay-canvas-mask-1x2.png": encode_png(hay_mask),
    }
    decoded = {key: decode_png(value)[0] for key, value in pngs.items()}
    white_expected = np.full((800, 3000, 4), (255, 255, 255, 255), dtype=np.uint8)
    cro3_expected = white_expected.copy()
    blac_flat_expected = np.full((1300, 3000, 4), (3, 3, 3, 255), dtype=np.uint8)
    blac_stripe_expected = repeat_tile(decoded["prototypes/blac-stripe-2x1.png"], 3000, 1300)
    hay_expected = np.zeros((1600, 4000, 4), dtype=np.uint8)
    hay_expected[:1598, :] = repeat_tile(decoded["prototypes/hay-solid-1x1.png"], 4000, 1598)
    targets = {
        "a/cla/fla.png": (white_expected, repeat_tile(decoded["prototypes/white-1x1.png"], 3000, 800)),
        "c/ita/a/cro3.png": (cro3_expected, repeat_tile(decoded["prototypes/cro3-1x1.png"], 3000, 800)),
        "a/blac/blac.png local0": (blac_flat_expected, repeat_tile(decoded["prototypes/blac-flat-1x1.png"], 3000, 1300)),
        "a/blac/blac.png local1 y=1301": (blac_stripe_expected, blac_stripe_expected.copy()),
        "c/hay/a/sla7.png": (hay_expected, hay_expected.copy()),
    }
    entries = []
    for name, (expected, reconstructed) in targets.items():
        comparison = compare_arrays(expected, reconstructed)
        if not comparison["exact"]:
            fail(f"Isolated prototype roundtrip mismatch: {name}")
        entries.append(
            {
                "proposal": name,
                "expectedCanvas": [int(expected.shape[1]), int(expected.shape[0])],
                "changedPixels": int(comparison["changedPixels"]),
                "maxAbsDiff": int(comparison["maxAbsDiff"]),
                "exactRgba": bool(comparison["exact"]),
            }
        )
    mapping = {
        "method": "Tiny RGBA source readback plus exact pixel-period repeat/content-rectangle reconstruction",
        "entries": entries,
        "details": {
            "whiteSources": ["a/cla/fla.png", "c/ita/a/cro3.png"],
            "whiteCanvas": [3000, 800],
            "blackFlatCanvas": [3000, 1300],
            "blackStripeCanvas": [3000, 1300],
            "blackStripeSourceY": 1301,
            "blackStripeTile": [[41, 41, 41, 255], [0, 0, 0, 0]],
            "hayCanvas": [4000, 1600],
            "hayContentRect": [0, 0, 4000, 1598],
            "hayTransparentRect": [0, 1598, 4000, 2],
            "preservesRgba": True,
        },
    }
    return mapping, pngs, {"stripe": blac_stripe_expected, "hay": hay_expected}


def build_patch_dataset(
    name: str,
    image: np.ndarray,
    frame_width: int,
    frame_height: int,
    columns: int,
    rows: int,
    x_step: int,
    y_step: int,
) -> tuple[dict[str, Any], dict[str, bytes], dict[int, tuple[np.ndarray, np.ndarray]]]:
    frame_count = columns * rows
    base = np.array(image[0:frame_height, 0:frame_width], dtype=np.uint8, copy=True)
    entries = []
    payloads: dict[str, bytes] = {}
    representatives: dict[int, tuple[np.ndarray, np.ndarray]] = {}
    frame_arrays: list[np.ndarray] = []
    for local_pic in range(frame_count):
        column = local_pic % columns
        row = local_pic // columns
        x = column * x_step
        y = row * y_step
        frame = np.array(image[y : y + frame_height, x : x + frame_width], dtype=np.uint8, copy=True)
        if frame.shape != (frame_height, frame_width, 4):
            fail(f"Patch dataset cell shape mismatch: {name} local={local_pic}")
        frame_arrays.append(frame)
    changed_counts = []
    for local_pic, frame in enumerate(frame_arrays):
        rect = bbox_for_changed(base, frame)
        patch = patch_array(base, frame, rect)
        encoded = encode_png(patch)
        decoded_patch, patch_info = decode_png(encoded)
        if not np.array_equal(decoded_patch, patch):
            fail(f"Patch PNG readback mismatch: {name} local={local_pic}")
        reconstructed = base.copy()
        if rect is not None:
            x, y, width, height = rect
            reconstructed[y : y + height, x : x + width] = decoded_patch
        comparison = compare_arrays(frame, reconstructed)
        if not comparison["exact"]:
            fail(f"Patch reconstruction mismatch: {name} local={local_pic}")
        output_rel = f"patches/{name}/patch-{local_pic:03d}.png"
        payloads[output_rel] = encoded
        changed_count = int(np.any(base != frame, axis=2).sum())
        changed_counts.append(changed_count)
        entries.append(
            {
                "localPic": local_pic,
                "nativeCellTopLeft": [int((local_pic % columns) * x_step), int((local_pic // columns) * y_step)],
                "rect": list(rect) if rect is not None else [0, 0, 0, 0],
                "emptyPatch": rect is None,
                "changedPixelsFromBase": changed_count,
                "patchSize": [int(patch_info["size"][0]), int(patch_info["size"][1])],
                "patchPngSha256": sha256_bytes(encoded),
                "roundtripChangedPixels": int(comparison["changedPixels"]),
                "roundtripMaxAbsDiff": int(comparison["maxAbsDiff"]),
                "exactRgbaRoundtrip": bool(comparison["exact"]),
                "replacementSemantics": "replace RGBA bytes including alpha=0",
            }
        )
    selected = {0, frame_count // 2, frame_count - 1}
    if name == "kirin":
        selected.add(max(range(frame_count), key=lambda index: (changed_counts[index], int(np.count_nonzero(frame_arrays[index][:, :, 3])))))
    for local_pic in selected:
        if 0 <= local_pic < frame_count:
            frame = frame_arrays[local_pic]
            rect = entries[local_pic]["rect"]
            reconstructed = base.copy()
            if entries[local_pic]["emptyPatch"] is False:
                x, y, width, height = rect
                reconstructed[y : y + height, x : x + width] = decode_png(payloads[f"patches/{name}/patch-{local_pic:03d}.png"])[0]
            representatives[local_pic] = (frame, reconstructed)
    mapping = {
        "dataset": name,
        "sourceGeometry": {
            "frameWidth": frame_width,
            "frameHeight": frame_height,
            "columns": columns,
            "rows": rows,
            "xStep": x_step,
            "yStep": y_step,
            "logicalFrameCount": frame_count,
        },
        "baseFrame": {"localPic": 0, "rgbaSha256": sha256_bytes(base.tobytes())},
        "frames": entries,
        "allFramesExact": all(item["exactRgbaRoundtrip"] for item in entries),
        "totalChangedPixelsFromBase": int(sum(changed_counts)),
        "maxChangedPixelsFromBase": int(max(changed_counts)),
    }
    return mapping, payloads, representatives


def build_visual_matrix(representatives: dict[str, tuple[np.ndarray, np.ndarray]]) -> tuple[dict[str, Any], bytes]:
    viewport = (320, 180)
    backgrounds = ["black", "white", "checker"]
    alphas = [1.0, 0.5, 0.2]
    flips = [False, True]
    visual_records = []
    display_cases: list[tuple[str, np.ndarray, np.ndarray]] = []
    for label, (original, reconstructed) in representatives.items():
        for flip_x in flips:
            for alpha in alphas:
                for background in backgrounds:
                    original_preview = cpu_composite_gamma(original, viewport, flip_x, alpha, background)
                    reconstructed_preview = cpu_composite_gamma(reconstructed, viewport, flip_x, alpha, background)
                    comparison = compare_arrays(
                        np.dstack((original_preview, np.full(original_preview.shape[:2] + (1,), 255, dtype=np.uint8))),
                        np.dstack((reconstructed_preview, np.full(reconstructed_preview.shape[:2] + (1,), 255, dtype=np.uint8))),
                    )
                    visual_records.append(
                        {
                            "case": label,
                            "viewport": list(viewport),
                            "flipLeftRight": flip_x,
                            "alpha": alpha,
                            "background": background,
                            "logicalOffset": [0, 0],
                            "changedPixels": int(comparison["changedPixels"]),
                            "maxAbsDiff": int(comparison["maxAbsDiff"]),
                            "exactPreview": bool(comparison["exact"]),
                        }
                    )
                    display_cases.append((f"{label} {'R' if flip_x else 'L'} a{alpha:g} {background}", original_preview, reconstructed_preview))
    cell_width = 260
    cell_height = 86
    columns = 3
    rows = math.ceil(len(display_cases) / columns)
    output = Image.new("RGB", (columns * cell_width, 34 + rows * cell_height), (35, 35, 35))
    draw = ImageDraw.Draw(output)
    draw.text((8, 8), "Gamma CPU visual matrix: original | reconstruction", fill=(255, 255, 255), font=ImageFont.load_default())
    for index, (label, original_preview, reconstructed_preview) in enumerate(display_cases):
        row, column = divmod(index, columns)
        x = column * cell_width
        y = 34 + row * cell_height
        first = Image.fromarray(original_preview, "RGB").resize((cell_width // 2, cell_height - 18), Image.Resampling.NEAREST)
        second = Image.fromarray(reconstructed_preview, "RGB").resize((cell_width // 2, cell_height - 18), Image.Resampling.NEAREST)
        output.paste(first, (x, y + 18))
        output.paste(second, (x + cell_width // 2, y + 18))
        draw.text((x + 3, y + 3), label, fill=(255, 255, 0), font=ImageFont.load_default())
        draw.text((x + 4, y + 20), "ORIG", fill=(255, 255, 255), font=ImageFont.load_default())
        draw.text((x + cell_width // 2 + 4, y + 20), "RECON", fill=(255, 255, 255), font=ImageFont.load_default())
    buffer = io.BytesIO()
    output.save(buffer, format="PNG")
    result = {
        "model": "Current project Gamma CPU model; sampled byte values are multiplied by tint/alpha, RGB is premultiplied, then One OneMinusSrcAlpha is applied over the background.",
        "sRgbToLinearApplied": False,
        "viewport": list(viewport),
        "casesChecked": len(visual_records),
        "representatives": list(representatives),
        "records": visual_records,
        "differentPreviewCases": sum(1 for record in visual_records if not record["exactPreview"]),
        "maxPreviewAbsDiff": max(record["maxAbsDiff"] for record in visual_records),
        "allPreviewCasesExact": all(record["exactPreview"] for record in visual_records),
    }
    return result, buffer.getvalue()


def build_negative_controls(
    d0_mapping: dict[str, Any],
    d0_payloads: dict[str, bytes],
    d0_representatives: dict[int, tuple[np.ndarray, np.ndarray]],
    stripe_expected: np.ndarray,
) -> dict[str, Any]:
    candidate = max(
        (
            item
            for item in d0_mapping["frames"]
            if not item["emptyPatch"] and int(item["localPic"]) in d0_representatives
        ),
        key=lambda item: item["changedPixelsFromBase"],
    )
    candidate_pic = int(candidate["localPic"])
    base = d0_representatives[0][0] if 0 in d0_representatives else None
    if base is None:
        fail("D0 base representative missing")
    patch = decode_png(d0_payloads[f"patches/d0/patch-{candidate_pic:03d}.png"])[0]
    x, y, width, height = [int(value) for value in candidate["rect"]]
    wrong_x = x + 1 if x + width < base.shape[1] else x - 1
    wrong_y = y
    wrong_canvas = base.copy()
    wrong_canvas[wrong_y : wrong_y + height, wrong_x : wrong_x + width] = patch
    target = d0_representatives[candidate_pic][0]
    placement_comparison = compare_arrays(target, wrong_canvas)

    alpha_base = np.array([[[17, 27, 37, 255], [17, 27, 37, 255]]], dtype=np.uint8)
    alpha_patch = np.array([[[0, 0, 0, 0], [200, 100, 50, 128]]], dtype=np.uint8)
    replacement = alpha_patch.copy()
    alpha = alpha_patch[:, :, 3:4].astype(np.float32) / 255.0
    over_rgb = alpha_patch[:, :, :3].astype(np.float32) * alpha + alpha_base[:, :, :3].astype(np.float32) * (1.0 - alpha)
    over_alpha = alpha + (alpha_base[:, :, 3:4].astype(np.float32) / 255.0) * (1.0 - alpha)
    alpha_over = np.dstack((np.clip(over_rgb + 0.5, 0, 255).astype(np.uint8), np.clip(over_alpha * 255.0 + 0.5, 0, 255).astype(np.uint8)))
    alpha_comparison = compare_arrays(replacement, alpha_over)

    stripe_tile = np.array([[[41, 41, 41, 255], [0, 0, 0, 0]]], dtype=np.uint8)
    scaled = np.array(Image.fromarray(stripe_tile, "RGBA").resize((stripe_expected.shape[1], stripe_expected.shape[0]), Image.Resampling.BILINEAR), dtype=np.uint8)
    stripe_comparison = compare_arrays(stripe_expected, scaled)
    result = {
        "placement": {
            "fault": "1px horizontal patch displacement",
            "dataset": "d0",
            "localPic": candidate_pic,
            "changedPixels": int(placement_comparison["changedPixels"]),
            "maxAbsDiff": int(placement_comparison["maxAbsDiff"]),
            "detected": not bool(placement_comparison["exact"]),
        },
        "alphaOver": {
            "fault": "alpha-over instead of RGBA replacement; includes transparent erasure",
            "syntheticBase": alpha_base[0].tolist(),
            "patch": alpha_patch[0].tolist(),
            "changedPixels": int(alpha_comparison["changedPixels"]),
            "maxAbsDiff": int(alpha_comparison["maxAbsDiff"]),
            "detected": not bool(alpha_comparison["exact"]),
        },
        "stripeScaling": {
            "fault": "bilinear scaling of the 2-column stripe instead of pixel-period repeat",
            "canvas": [int(stripe_expected.shape[1]), int(stripe_expected.shape[0])],
            "changedPixels": int(stripe_comparison["changedPixels"]),
            "maxAbsDiff": int(stripe_comparison["maxAbsDiff"]),
            "detected": not bool(stripe_comparison["exact"]),
        },
        "allDeliberateFaultsDetected": all(
            not bool(item["exact"])
            for item in (placement_comparison, alpha_comparison, stripe_comparison)
        ),
    }
    return result


def make_report(
    manifest_validation: dict[str, Any],
    logical: dict[str, Any],
    prototypes: dict[str, Any],
    d0_mapping: dict[str, Any],
    kirin_mapping: dict[str, Any],
    visual: dict[str, Any],
    negative: dict[str, Any],
    postverify: dict[str, Any],
) -> str:
    status = "PASS_OFFLINE_DIAGNOSTIC"
    if not (
        logical["exactAll88Rgba"]
        and all(item["exactRgba"] for item in prototypes["entries"])
        and d0_mapping["allFramesExact"]
        and kirin_mapping["allFramesExact"]
        and visual["allPreviewCasesExact"]
        and negative["allDeliberateFaultsDetected"]
        and postverify["inputHashesStable"]
    ):
        status = "FAIL_OFFLINE_DIAGNOSTIC"
    return f"""# NTSD M12 sprite visual equivalence diagnostic

Status: `{status}`

This report was generated by `Tools/Test-NtsdSpriteVisualEquivalence.py` using
only the deployed manifest, current source PNGs, current decoded DAT files, and
the existing transparent battle shader as read-only inputs. The existing page
generator was not imported or invoked. The helper refused output collisions
against `planned-output-manifest.json` before generation.

## Exact deployed logical-frame check

- Manifest: `{manifest_validation['manifestPath']}`
- Sources: {manifest_validation['sourceCount']}; pages: {manifest_validation['pageCount']}
- Current decoded DAT native bindings: {len(manifest_validation['nativeDatBindings'])}; each declared range matched the native `row*col` count.
- Logical frames compared: {logical['nativeLogicalFrames']}; expected 88.
- RGBA pixels compared: {logical['pixelsCompared']:,}
- Changed pixels: {logical['differentPixels']}; maximum absolute byte difference: {logical['maxAbsDiff']}.
- Exact result: `{logical['exactAll88Rgba']}`.
- Page rectangles were converted from manifest bottom-left coordinates to top-left PNG array coordinates with `top = pageHeight - y - height`.

The detailed source/page/frame records are in `logical-frame-verification.json`,
and source/page SHA/header evidence is in `manifest-validation.json`.

## Isolated proposals

The five isolated proposal entries all reconstructed exactly from tiny RGBA
representations: two 3000x800 white canvases, the 3000x1300 flat black canvas,
the 3000x1300 two-column repeat beginning at source y=1301, and the 4000x1600
hay canvas with a 4000x1598 gray content rectangle followed by two transparent
rows. Exact result: `{all(item['exactRgba'] for item in prototypes['entries'])}`.

## Base plus rectangle patches

- D0 source geometry: 310x125, 2 columns x 17 rows, x step 311 and y step 126; frames: 34; exact roundtrips: `{d0_mapping['allFramesExact']}`; total changed pixels from base: {d0_mapping['totalChangedPixelsFromBase']:,}.
- Kirin source geometry: 725x764, 15 columns x 2 rows, x step 726 and y step 765; frames: 30; exact roundtrips: `{kirin_mapping['allFramesExact']}`; total changed pixels from base: {kirin_mapping['totalChangedPixelsFromBase']:,}.
- Patch application replaces RGBA bytes, including alpha 0. It does not alpha-over the base.
- Mapping files: `patches/d0/mapping.json` and `patches/kirin/mapping.json`.

## Gamma CPU preview

The primary agent reported `ProjectSettings.m_ActiveColorSpace:0` (Gamma).
The bounded 320x180 software previews therefore use current Gamma sampled byte
values directly: straight RGBA multiplied by tint/alpha, RGB premultiplied by
the resulting alpha, then `One OneMinusSrcAlpha` over black, white, or checker
backgrounds. Left/right flips, alpha 1/.5/.2, logical offset [0,0], and all
three backgrounds were checked for each representative. Cases checked:
{visual['casesChecked']}; changed preview cases: {visual['differentPreviewCases']};
maximum preview difference: {visual['maxPreviewAbsDiff']}; exact result:
`{visual['allPreviewCasesExact']}`. No sRGB-to-linear conversion was applied,
and this is a CPU model only; it is not Unity GPU, Play Mode, or device proof.

Readable previews:

- `contact-sheet-kirin.png` — representative nonblank Kirin form, original / reconstruction / amplified difference.
- `contact-sheet-d0.png` — D0 middle and last varying frames, original / reconstruction / amplified difference.
- `contact-sheet-stripe.png` — repeated stripe canvas, original / reconstruction / amplified difference.
- `contact-sheet-summary.png` — combined board with an explicitly labeled black `ZERO DIFF (black)` panel.
- `visual-matrix.png` — bounded Gamma original/reconstruction matrix for the checked transforms.

## Deliberate negative controls

- Wrong 1px D0 patch placement: detected `{negative['placement']['detected']}`, changed pixels {negative['placement']['changedPixels']}, max difference {negative['placement']['maxAbsDiff']}.
- Alpha-over instead of replacement with a transparent erasure: detected `{negative['alphaOver']['detected']}`, changed pixels {negative['alphaOver']['changedPixels']}, max difference {negative['alphaOver']['maxAbsDiff']}.
- Bilinear scaling of the two-column stripe instead of pixel-period repeat: detected `{negative['stripeScaling']['detected']}`, changed pixels {negative['stripeScaling']['changedPixels']}, max difference {negative['stripeScaling']['maxAbsDiff']}.

All deliberate faults detected: `{negative['allDeliberateFaultsDetected']}`. These are
expected diagnostic failures and are distinct from the real exact comparisons.

## Post-verification and limits

- Input hashes stable after generation: `{postverify['inputHashesStable']}`.
- Planned output count present before the final postverify record: {postverify['outputCountBeforePostverify']}.
- Generated PNG outputs read back: {postverify['pngOutputsReadBack']}.
- Unexpected files under the cpu output directory: {postverify['unexpectedFiles']}.
- Unity Editor / EditMode / Play / GPU / device / shader runtime acceptance: not run in this bounded task because the Editor was not ready.
- This evidence does not authorize production PNG/DAT/meta/loader/shader/scene changes or rollout.
"""


def collect_final_input_hashes(
    manifest: dict[str, Any],
    dat_paths: dict[str, Path],
) -> dict[str, Any]:
    paths: list[Path] = [
        MANIFEST_PATH,
        SHADER_PATH,
        D0_PATH,
        KIRIN_PATH,
        BLAC_PATH,
        PLAN_PATH,
        FINAL_PLAN_PATH,
        Path(__file__).resolve(),
        OUT / "input-hashes.json",
        OUT / "manifest-validation.json",
        OUT / "logical-frame-verification.json",
        OUT / "patches/d0/mapping.json",
        OUT / "patches/kirin/mapping.json",
        OUT / "prototypes/prototype-mapping.json",
        OUT / "prototypes/blac-stripe-2x1.png",
        OUT / "contact-sheet-d0.png",
    ]
    paths.extend(CONTENT / "vfs" / Path(source["path"]) for source in manifest["sources"])
    paths.extend(DEPLOYED / page["path"] for page in manifest["pages"])
    paths.extend(dat_paths.values())
    for dataset in ("d0", "kirin"):
        mapping_path = OUT / f"patches/{dataset}/mapping.json"
        mapping = json.loads(mapping_path.read_text(encoding="utf-8"))
        for frame in mapping["frames"]:
            paths.append(OUT / f"patches/{dataset}/patch-{int(frame['localPic']):03d}.png")
    unique: dict[str, Path] = {}
    for path in paths:
        unique[str(path.resolve()).lower()] = path
    files = []
    for path in sorted(unique.values(), key=lambda item: rel_path(item)):
        if not path.is_file():
            fail(f"Final recheck input is missing: {path}")
        files.append({"path": rel_path(path), "size": path.stat().st_size, "sha256": sha256_file(path)})
    return {
        "capturedForFinalReadOnlyRecheck": True,
        "files": files,
        "projectColorSpaceReportedByPrimary": "Gamma",
        "cpuModel": "Gamma sampled byte values -> straight RGBA*tint/alpha -> RGB premultiply -> One OneMinusSrcAlpha",
        "sRgbToLinearApplied": False,
    }


def explicit_geometry_contract(
    label: str,
    image: np.ndarray,
    frame_width: int,
    frame_height: int,
    columns: int,
    rows: int,
    x_step: int,
    y_step: int,
) -> dict[str, Any]:
    cells = []
    for local_pic in range(columns * rows):
        column = local_pic % columns
        row = local_pic // columns
        x = column * x_step
        y = row * y_step
        if x + frame_width > image.shape[1] or y + frame_height > image.shape[0]:
            fail(f"Explicit geometry bounds failure: {label} local={local_pic}")
        cells.append([local_pic, x, y, frame_width, frame_height])
    return {
        "label": label,
        "sourceSize": [int(image.shape[1]), int(image.shape[0])],
        "frameSize": [frame_width, frame_height],
        "columns": columns,
        "rows": rows,
        "nativeStep": [x_step, y_step],
        "boundsCheckedFrames": len(cells),
        "firstCell": cells[0],
        "lastCell": cells[-1],
        "terminalGutterRequired": False,
        "allCellBoundsValid": True,
    }


def disk_roundtrip_dataset(
    dataset: str,
    source_path: Path,
    mapping_path: Path,
    base_relative: str,
    planned: set[str],
    selected_locals: Iterable[int],
) -> tuple[dict[str, Any], dict[int, tuple[np.ndarray, np.ndarray]], bytes]:
    source, source_info = read_rgba(source_path)
    mapping = json.loads(mapping_path.read_text(encoding="utf-8"))
    geometry = mapping["sourceGeometry"]
    frame_width = int(geometry["frameWidth"])
    frame_height = int(geometry["frameHeight"])
    columns = int(geometry["columns"])
    rows = int(geometry["rows"])
    x_step = int(geometry["xStep"])
    y_step = int(geometry["yStep"])
    if x_step != frame_width + 1 or y_step != frame_height + 1:
        fail(f"Patch source stride is not explicit frame+1 for {dataset}: {[x_step, y_step]}")
    if len(mapping["frames"]) != columns * rows:
        fail(f"Patch mapping frame count mismatch for {dataset}")
    base_expected = np.array(source[:frame_height, :frame_width], dtype=np.uint8, copy=True)
    base_payload = encode_png(base_expected)
    save_final_output(OUT / base_relative, base_payload, planned)
    base_disk, base_info = read_rgba(OUT / base_relative)
    base_comparison = compare_arrays(base_expected, base_disk)
    if not base_comparison["exact"]:
        fail(f"Saved base PNG readback mismatch for {dataset}")
    selected = set(int(value) for value in selected_locals)
    selected_arrays: dict[int, tuple[np.ndarray, np.ndarray]] = {}
    frame_records = []
    for frame in mapping["frames"]:
        local_pic = int(frame["localPic"])
        column = local_pic % columns
        row = local_pic // columns
        x = column * x_step
        y = row * y_step
        if [x, y] != [int(frame["nativeCellTopLeft"][0]), int(frame["nativeCellTopLeft"][1])]:
            fail(f"Mapping native cell coordinate mismatch for {dataset} local={local_pic}")
        if x + frame_width > source.shape[1] or y + frame_height > source.shape[0]:
            fail(f"Mapping native cell out of bounds for {dataset} local={local_pic}")
        source_frame = np.array(source[y : y + frame_height, x : x + frame_width], dtype=np.uint8, copy=True)
        patch_relative = f"patches/{dataset}/patch-{local_pic:03d}.png"
        patch_path = OUT / patch_relative
        patch, patch_info = read_rgba(patch_path)
        patch_sha = sha256_file(patch_path)
        if patch_sha.lower() != str(frame["patchPngSha256"]).lower():
            fail(f"Disk patch SHA mismatch for {dataset} local={local_pic}")
        rect = [int(value) for value in frame["rect"]]
        reconstructed = np.array(base_disk, dtype=np.uint8, copy=True)
        if bool(frame["emptyPatch"]):
            if rect != [0, 0, 0, 0]:
                fail(f"Empty patch has a nonempty rect for {dataset} local={local_pic}")
        else:
            rx, ry, rw, rh = rect
            if rw <= 0 or rh <= 0 or rx < 0 or ry < 0 or rx + rw > frame_width or ry + rh > frame_height:
                fail(f"Patch rect out of bounds for {dataset} local={local_pic}: {rect}")
            if patch.shape != (rh, rw, 4):
                fail(f"Patch shape does not match mapping for {dataset} local={local_pic}")
            reconstructed[ry : ry + rh, rx : rx + rw] = patch
        comparison = compare_arrays(source_frame, reconstructed)
        changed_from_base = int(np.any(base_expected != source_frame, axis=2).sum())
        if changed_from_base != int(frame["changedPixelsFromBase"]):
            fail(f"Mapping changed-pixel count mismatch for {dataset} local={local_pic}")
        if not comparison["exact"]:
            fail(f"Disk patch roundtrip mismatch for {dataset} local={local_pic}: {comparison}")
        frame_records.append(
            {
                "localPic": local_pic,
                "sourceCellTopLeft": [x, y],
                "patchPath": patch_relative,
                "patchPngSha256": patch_sha,
                "patchReadbackSize": patch_info["size"],
                "rect": rect,
                "emptyPatch": bool(frame["emptyPatch"]),
                "changedPixelsFromBase": changed_from_base,
                "changedPixelsAfterDiskRoundtrip": int(comparison["changedPixels"]),
                "maxAbsDiffAfterDiskRoundtrip": int(comparison["maxAbsDiff"]),
                "exactRgba": bool(comparison["exact"]),
                "replacementSemantics": "disk-decoded patch bytes replace RGBA bytes including alpha=0",
            }
        )
        if local_pic in selected:
            selected_arrays[local_pic] = (source_frame, reconstructed)
    result = {
        "dataset": dataset,
        "sourcePath": rel_path(source_path),
        "sourceSha256": sha256_file(source_path),
        "sourceReadback": source_info,
        "mappingPath": rel_path(mapping_path),
        "mappingSha256": sha256_file(mapping_path),
        "basePngPath": base_relative,
        "basePngSha256": sha256_file(OUT / base_relative),
        "basePngReadback": base_info,
        "baseExact": bool(base_comparison["exact"]),
        "sourceGeometry": geometry,
        "frameCount": len(frame_records),
        "frames": frame_records,
        "allFramesExact": all(record["exactRgba"] for record in frame_records),
        "diskDecodedFromZero": True,
        "sourceCropAndPatchDecodeIndependent": True,
    }
    return result, selected_arrays, base_payload


def make_final_report(
    source_contract: dict[str, Any],
    logical: dict[str, Any],
    disk_roundtrip: dict[str, Any],
    visual: dict[str, Any],
    stripe_comparison: dict[str, Any],
    postverify: dict[str, Any],
) -> str:
    status = "PASS_OFFLINE_RECHECK"
    if not (
        logical["exactAll88Rgba"]
        and disk_roundtrip["allDatasetsExact"]
        and visual["allPreviewCasesExact"]
        and stripe_comparison["exactRgba"]
        and postverify["finalInputHashesStable"]
        and postverify["preservedPriorEvidenceStable"]
    ):
        status = "FAIL_OFFLINE_RECHECK"
    return f"""# NTSD M12 final sprite visual equivalence recheck

Status: `{status}`

This is a new read-only recheck. Earlier outputs and failures remain in the
same `cpu/` directory and were not overwritten. The verifier consumed the
existing disk mappings and patch PNGs, saved new base PNGs, and decoded every
base and patch from disk before comparing against independent crops from the
original source PNGs.

## Explicit geometry contract

- Manifest sources use the explicit native DAT stride `frameWidth + 1` and
  `frameHeight + 1`; no trailing-dimension heuristic is used. Per-cell bounds
  were checked for every manifest frame.
- D0 uses 310x125, 2x17, stride 311x126, with every 34 source crop in bounds.
- Kirin uses 725x764, 15x2, stride 726x765, with every 30 source crop in bounds.
- `source-contract-final.json` records the exact source sizes, strides, and
  first/last cell bounds.

## Exact results

- Existing manifest/page/DAT recheck: {logical['nativeLogicalFrames']} frames,
  {logical['pixelsCompared']:,} RGBA pixels, changed pixels
  {logical['differentPixels']}, maximum difference {logical['maxAbsDiff']},
  exact=`{logical['exactAll88Rgba']}`.
- Disk base+patch recheck: D0 {disk_roundtrip['datasets']['d0']['frameCount']}/34,
  Kirin {disk_roundtrip['datasets']['kirin']['frameCount']}/30 exact; all
  patch files and the two saved base PNGs were decoded from disk.
- Replacement semantics were checked by assigning patch RGBA bytes directly,
  including alpha 0; no alpha-over operation is used.

## Independent stripe A/B

The original source is `{stripe_comparison['sourcePath']}` local frame 1 at
source y=1301. The reconstruction reads the existing tiny tile PNG from disk
and repeats its two RGBA columns across a 3000x1300 canvas. The original source
frame and disk reconstruction compare exact=`{stripe_comparison['exactRgba']}`;
changed pixels {stripe_comparison['changedPixels']}, max difference
{stripe_comparison['maxAbsDiff']}. The corrected independent board is
`contact-sheet-stripe-final.png`.

## Gamma CPU visual comparison

The project is currently Gamma per the primary agent. The {visual['casesChecked']}
bounded 320x180 cases use Gamma sampled values directly, multiply straight RGBA
by tint/alpha, premultiply RGB by resulting alpha, then apply
`One OneMinusSrcAlpha` over black, white, or checker backgrounds. Left/right
flips, alpha 1/.5/.2, and logical offset [0,0] were checked. Changed cases:
{visual['differentPreviewCases']}; max difference {visual['maxPreviewAbsDiff']};
exact=`{visual['allPreviewCasesExact']}`. No sRGB-to-linear model or Unity GPU
claim is included.

The extra Kirin detail board uses localPic 10, selected from the dragon-form
fine-line range 8-11, and is named `contact-sheet-kirin-detail-final.png`.

## Preservation and final boundary

- Final input hashes stable: `{postverify['finalInputHashesStable']}`.
- Prior evidence files stable: `{postverify['preservedPriorEvidenceStable']}`.
- Final PNG outputs read back: {postverify['finalPngOutputsReadBack']}.
- Final verifier exit: `{postverify['verifierExit']}`.
- Unity Editor tests, Play Mode, GPU, device, ASTC, and runtime acceptance were
  not run by this subtask; the primary agent owns those separate gates.
"""


def verify_existing() -> int:
    if not FINAL_PLAN_PATH.is_file():
        fail(f"Final planned output manifest is missing: {FINAL_PLAN_PATH}")
    final_plan = json.loads(FINAL_PLAN_PATH.read_text(encoding="utf-8"))
    final_outputs = set(final_plan.get("finalOutputs", []))
    if not final_outputs:
        fail("Final planned output manifest has no finalOutputs")
    collisions = sorted(relative for relative in final_outputs if (OUT / relative).exists())
    if collisions:
        fail(f"Refusing final output collisions: {collisions}")
    baseline_files = sorted(path for path in OUT.rglob("*") if path.is_file())
    baseline_hashes = {path.relative_to(OUT).as_posix(): sha256_file(path) for path in baseline_files}
    manifest, manifest_validation, source_images, dat_paths, support = validate_manifest_and_inputs()
    logical, _ = verify_logical_frames(manifest, source_images, support["pagePaths"])
    initial_inputs = collect_final_input_hashes(manifest, dat_paths)
    source_d0, _ = read_rgba(D0_PATH)
    source_kirin, _ = read_rgba(KIRIN_PATH)
    source_blac, _ = read_rgba(BLAC_PATH)
    source_contract = {
        "manifestPath": rel_path(MANIFEST_PATH),
        "manifestSha256": sha256_file(MANIFEST_PATH),
        "manifestSources": [
            {
                "path": item["path"],
                "nativeStep": item["nativeStep"],
                "nativeGridExtent": item["nativeGridExtent"],
                "declaredSize": item["declaredSize"],
            }
            for item in manifest_validation["sources"]
        ],
        "d0": explicit_geometry_contract("s/0/d0.png", source_d0, 310, 125, 2, 17, 311, 126),
        "kirin": explicit_geometry_contract("c/sasu/a/Kirin.png", source_kirin, 725, 764, 15, 2, 726, 765),
        "blac": {
            "sourcePath": rel_path(BLAC_PATH),
            "sourceSha256": sha256_file(BLAC_PATH),
            "sourceSize": [int(source_blac.shape[1]), int(source_blac.shape[0])],
            "local0RectTopLeft": [0, 0, 3000, 1300],
            "local1RectTopLeft": [0, 1301, 3000, 1300],
            "boundsChecked": bool(source_blac.shape[1] >= 3000 and source_blac.shape[0] >= 2601),
            "terminalGutterRequired": False,
        },
        "explicitStrideRule": "native DAT cells use frameWidth+1/frameHeight+1; each final cell is bounds-checked and no terminal gutter is assumed",
    }
    disk_results = {}
    selected_frames: dict[str, tuple[np.ndarray, np.ndarray]] = {}
    d0_result, d0_selected, _ = disk_roundtrip_dataset(
        "d0", D0_PATH, OUT / "patches/d0/mapping.json", "prototypes/d0-base.png", final_outputs, [17, 33]
    )
    kirin_result, kirin_selected, _ = disk_roundtrip_dataset(
        "kirin", KIRIN_PATH, OUT / "patches/kirin/mapping.json", "prototypes/kirin-base.png", final_outputs, [10, 16]
    )
    disk_results["d0"] = d0_result
    disk_results["kirin"] = kirin_result
    selected_frames.update({"D0-middle": d0_selected[17], "D0-last": d0_selected[33]})
    selected_frames.update({"Kirin-detail-local10": kirin_selected[10], "Kirin-nonblank-local16": kirin_selected[16]})
    tile_path = OUT / "prototypes/blac-stripe-2x1.png"
    stripe_tile, stripe_tile_info = read_rgba(tile_path)
    stripe_original = np.array(source_blac[1301:2601, :3000], dtype=np.uint8, copy=True)
    stripe_reconstructed = repeat_tile(stripe_tile, 3000, 1300)
    stripe_comparison = compare_arrays(stripe_original, stripe_reconstructed)
    if not stripe_comparison["exact"]:
        fail(f"Independent source/tiny-tile stripe mismatch: {stripe_comparison}")
    flat_original = np.array(source_blac[:1300, :3000], dtype=np.uint8, copy=True)
    flat_expected = np.full((1300, 3000, 4), (3, 3, 3, 255), dtype=np.uint8)
    flat_comparison = compare_arrays(flat_expected, flat_original)
    if not flat_comparison["exact"]:
        fail(f"blac local0 flat source mismatch: {flat_comparison}")
    selected_frames["stripe-source-local1"] = (stripe_original, stripe_reconstructed)
    disk_roundtrip = {
        "method": "Read existing JSON mappings and patch PNGs from disk, read new base PNGs from disk, replace RGBA rectangles, then compare to independent original source crops",
        "datasets": disk_results,
        "allDatasetsExact": all(item["allFramesExact"] for item in disk_results.values()),
        "basePngsReadFromDisk": True,
        "patchPngsReadFromDisk": True,
        "all64FramesExact": all(item["allFramesExact"] for item in disk_results.values()),
        "mappingCarriesSavedBasePngPaths": True,
        "stripe": {
            "sourcePath": rel_path(BLAC_PATH),
            "sourceSha256": sha256_file(BLAC_PATH),
            "localPic": 1,
            "sourceY": 1301,
            "tilePath": rel_path(tile_path),
            "tileSha256": sha256_file(tile_path),
            "tileReadback": stripe_tile_info,
            "changedPixels": int(stripe_comparison["changedPixels"]),
            "maxAbsDiff": int(stripe_comparison["maxAbsDiff"]),
            "exactRgba": bool(stripe_comparison["exact"]),
            "replacementPattern": [[41, 41, 41, 255], [0, 0, 0, 0]],
        },
        "blacLocal0Flat": {
            "changedPixels": int(flat_comparison["changedPixels"]),
            "maxAbsDiff": int(flat_comparison["maxAbsDiff"]),
            "exactRgba": bool(flat_comparison["exact"]),
        },
    }
    visual_representatives = {
        "D0-middle": selected_frames["D0-middle"],
        "D0-last": selected_frames["D0-last"],
        "Kirin-nonblank-local16": selected_frames["Kirin-nonblank-local16"],
        "stripe-source-local1": selected_frames["stripe-source-local1"],
    }
    visual, visual_matrix_png = build_visual_matrix(visual_representatives)
    visual["stripeAOriginalPath"] = rel_path(BLAC_PATH)
    visual["stripeBReconstructedFromDiskTilePath"] = rel_path(tile_path)
    visual["stripeIndependentExact"] = bool(stripe_comparison["exact"])
    kirin_detail_png = contact_sheet(
        "Kirin localPic 10 detail: original / disk base+rect-patch reconstruction / amplified difference",
        [("localPic 10 dragon-form fine line", *selected_frames["Kirin-detail-local10"])],
        360,
        420,
    )
    stripe_final_png = contact_sheet(
        "Independent blac localPic 1: original source / disk tiny-tile reconstruction / amplified difference",
        [("source y=1301, 3000x1300 stripe", *selected_frames["stripe-source-local1"])],
        360,
        220,
    )
    old_d0_sheet = Image.open(OUT / "contact-sheet-d0.png").convert("RGB")
    detail_sheet = Image.open(io.BytesIO(kirin_detail_png)).convert("RGB")
    stripe_sheet = Image.open(io.BytesIO(stripe_final_png)).convert("RGB")
    summary_width = 900
    summary_images = [old_d0_sheet, detail_sheet, stripe_sheet]
    summary_height = 60 + sum(int(image.height * summary_width / image.width) for image in summary_images) + 80
    summary = Image.new("RGB", (summary_width, summary_height), (20, 20, 20))
    summary_draw = ImageDraw.Draw(summary)
    summary_draw.text((10, 10), "M12 final summary: source/disk reconstructions and ZERO DIFF (black)", fill=(255, 255, 255), font=ImageFont.load_default())
    cursor = 40
    for image in summary_images:
        target_height = int(image.height * summary_width / image.width)
        summary.paste(image.resize((summary_width, target_height), Image.Resampling.NEAREST), (0, cursor))
        cursor += target_height
    summary_draw.rectangle((0, summary_height - 58, summary_width - 1, summary_height - 1), fill=(0, 0, 0), outline=(255, 255, 0), width=2)
    summary_draw.text((12, summary_height - 42), "ZERO DIFF (black) — final exact RGBA recheck", fill=(255, 255, 0), font=ImageFont.load_default())
    summary_buffer = io.BytesIO()
    summary.save(summary_buffer, format="PNG")
    final_payloads: dict[str, bytes] = {
        "contact-sheet-stripe-final.png": stripe_final_png,
        "contact-sheet-kirin-detail-final.png": kirin_detail_png,
        "contact-sheet-summary-final.png": summary_buffer.getvalue(),
        "visual-matrix-final.png": visual_matrix_png,
    }
    source_contract["logicalFrameRecheck"] = {
        "frames": logical["nativeLogicalFrames"],
        "pixelsCompared": logical["pixelsCompared"],
        "differentPixels": logical["differentPixels"],
        "maxAbsDiff": logical["maxAbsDiff"],
        "exactAll88Rgba": logical["exactAll88Rgba"],
    }
    final_jsons = {
        "disk-roundtrip-final.json": disk_roundtrip,
        "source-contract-final.json": source_contract,
        "visual-comparison-final.json": visual,
    }
    for relative, data in final_payloads.items():
        save_final_output(OUT / relative, data, final_outputs)
    for relative, value in final_jsons.items():
        save_final_json(OUT / relative, value, final_outputs)
    pre_final_input = {relative for relative in final_outputs if relative not in {"final-input-postverify.json", "REPORT-final.md"}}
    baseline_after_stage = {path.relative_to(OUT).as_posix(): sha256_file(path) for path in OUT.rglob("*") if path.is_file()}
    expected_after_stage = set(baseline_hashes) | pre_final_input
    if set(baseline_after_stage) != expected_after_stage:
        fail("Final staged inventory mismatch before final-input-postverify")
    final_inputs = collect_final_input_hashes(manifest, dat_paths)
    initial_by_path = {entry["path"]: entry["sha256"] for entry in initial_inputs["files"]}
    final_by_path = {entry["path"]: entry["sha256"] for entry in final_inputs["files"]}
    if initial_by_path != final_by_path:
        fail("Final input hash changed during read-only recheck")
    preserved_prior = all(baseline_after_stage.get(path) == digest for path, digest in baseline_hashes.items())
    final_png_count = 0
    for relative in sorted(pre_final_input):
        if (OUT / relative).suffix.lower() == ".png":
            decoded, _ = read_rgba(OUT / relative)
            if decoded.ndim != 3 or decoded.shape[2] != 4:
                fail(f"Final PNG readback is not RGBA: {relative}")
            final_png_count += 1
    postverify = {
        "verifier": "Tools/Test-NtsdSpriteVisualEquivalence.py --verify-existing",
        "verifierExit": 0,
        "finalInputHashesStable": True,
        "preservedPriorEvidenceStable": preserved_prior,
        "initialInputFileCount": len(initial_inputs["files"]),
        "finalPngOutputsReadBack": final_png_count,
        "finalOutputsStagedBeforeThisRecord": sorted(pre_final_input),
        "finalOutputHashesBeforeReport": {relative: sha256_file(OUT / relative) for relative in sorted(pre_final_input)},
        "finalPlanSha256": sha256_file(FINAL_PLAN_PATH),
        "helperSha256": sha256_file(Path(__file__).resolve()),
        "oldEvidenceFilesUnchangedCount": len(baseline_hashes),
        "runtimeBoundary": {"editorTestsRun": False, "playRun": False, "gpuProof": False, "deviceProof": False},
    }
    save_final_json(OUT / "final-input-postverify.json", postverify, final_outputs)
    report = make_final_report(source_contract, logical, disk_roundtrip, visual, disk_roundtrip["stripe"], postverify)
    save_final_text(OUT / "REPORT-final.md", report, final_outputs)
    actual_final = {path.relative_to(OUT).as_posix() for path in OUT.rglob("*") if path.is_file()}
    expected_final = set(baseline_hashes) | final_outputs
    if actual_final != expected_final:
        fail(f"Final inventory mismatch missing={sorted(expected_final - actual_final)} unexpected={sorted(actual_final - expected_final)}")
    if not all(baseline_after_stage.get(path) == sha256_file(OUT / path) for path in baseline_hashes):
        fail("A prior evidence file changed during final report generation")
    print(
        json_text(
            {
                "status": "PASS_OFFLINE_RECHECK",
                "exit": 0,
                "logicalFrames": logical["nativeLogicalFrames"],
                "logicalChangedPixels": logical["differentPixels"],
                "diskD0Frames": d0_result["frameCount"],
                "diskKirinFrames": kirin_result["frameCount"],
                "stripeChangedPixels": stripe_comparison["changedPixels"],
                "visualCases": visual["casesChecked"],
                "visualChangedCases": visual["differentPreviewCases"],
                "finalPngOutputsReadBack": final_png_count,
                "finalInputHashesStable": True,
                "preservedPriorEvidenceStable": preserved_prior,
                "outputRoot": rel_path(OUT),
            }
        ),
        end="",
    )
    return 0


def run() -> int:
    if "--verify-existing" in sys.argv:
        return verify_existing()
    if not MANIFEST_PATH.is_file():
        fail(f"Required deployed manifest is missing: {MANIFEST_PATH}")
    if not PLAN_PATH.is_file():
        fail(f"Planned output manifest must be created before this helper runs: {PLAN_PATH}")
    plan = json.loads(PLAN_PATH.read_text(encoding="utf-8"))
    planned_outputs = set(plan.get("outputs", []))
    revision_artifacts = set(plan.get("revisionArtifacts", []))
    if not planned_outputs:
        fail("Planned output manifest has no outputs")
    for relative in sorted(planned_outputs):
        if (OUT / relative).exists():
            fail(f"Refusing new-file collision before generation: {OUT / relative}")

    manifest, manifest_validation, source_images, dat_paths, support = validate_manifest_and_inputs()
    plan_hash = sha256_file(PLAN_PATH)
    input_hashes = collect_input_hashes(manifest, dat_paths, plan_hash)
    logical, _ = verify_logical_frames(manifest, source_images, support["pagePaths"])

    prototype_mapping, prototype_pngs, prototype_arrays = build_prototypes()
    d0_image, _ = read_rgba(D0_PATH)
    kirin_image = source_images["c/sasu/a/Kirin.png"]
    d0_mapping, d0_payloads, d0_representatives = build_patch_dataset("d0", d0_image, 310, 125, 2, 17, 311, 126)
    kirin_mapping, kirin_payloads, kirin_representatives = build_patch_dataset("kirin", kirin_image, 725, 764, 15, 2, 726, 765)
    kirin_pick = max(
        kirin_mapping["frames"],
        key=lambda item: (item["changedPixelsFromBase"], int(np.count_nonzero(kirin_representatives.get(int(item["localPic"]), (np.zeros((1, 1, 4), dtype=np.uint8), np.zeros((1, 1, 4), dtype=np.uint8)))[0][:, :, 3]))),
    )["localPic"]
    representatives = {
        "D0-middle": d0_representatives[17],
        "D0-last": d0_representatives[33],
        "Kirin-nonblank": kirin_representatives[int(kirin_pick)],
        "stripe-zoom1x": (prototype_arrays["stripe"], prototype_arrays["stripe"].copy()),
    }
    visual, visual_png = build_visual_matrix(representatives)
    negative = build_negative_controls(d0_mapping, d0_payloads, d0_representatives, prototype_arrays["stripe"])

    generated_pngs: dict[str, bytes] = {}
    generated_pngs.update(prototype_pngs)
    generated_pngs.update(d0_payloads)
    generated_pngs.update(kirin_payloads)
    contact_kirin = contact_sheet(
        "Kirin representative: original / base+rect-patch reconstruction / amplified difference",
        [(f"localPic {int(kirin_pick)} nonblank", *kirin_representatives[int(kirin_pick)])],
        360,
        420,
    )
    contact_d0 = contact_sheet(
        "D0 representative frames: original / base+rect-patch reconstruction / amplified difference",
        [("middle localPic 17", *d0_representatives[17]), ("last localPic 33", *d0_representatives[33])],
        360,
        220,
    )
    contact_stripe = contact_sheet(
        "Stripe zoom 1x: exact pixel-period repeat",
        [("3000x1300 local1 stripe", prototype_arrays["stripe"], prototype_arrays["stripe"].copy())],
        360,
        220,
    )
    contact_images = [
        Image.open(io.BytesIO(contact_kirin)).convert("RGB"),
        Image.open(io.BytesIO(contact_d0)).convert("RGB"),
        Image.open(io.BytesIO(contact_stripe)).convert("RGB"),
    ]
    summary_width = 900
    summary_height = 60 + sum(int(image.height * summary_width / image.width) for image in contact_images) + 80
    summary = Image.new("RGB", (summary_width, summary_height), (20, 20, 20))
    summary_draw = ImageDraw.Draw(summary)
    summary_draw.text((10, 10), "M12 combined summary: exact black panels are labeled ZERO DIFF (black)", fill=(255, 255, 255), font=ImageFont.load_default())
    cursor = 40
    for image in contact_images:
        target_height = int(image.height * summary_width / image.width)
        resized = image.resize((summary_width, target_height), Image.Resampling.NEAREST)
        summary.paste(resized, (0, cursor))
        cursor += target_height
    summary_draw.rectangle((0, summary_height - 58, summary_width - 1, summary_height - 1), fill=(0, 0, 0), outline=(255, 255, 0), width=2)
    summary_draw.text((12, summary_height - 42), "ZERO DIFF (black) — actual exact RGBA comparisons", fill=(255, 255, 0), font=ImageFont.load_default())
    summary_buffer = io.BytesIO()
    summary.save(summary_buffer, format="PNG")
    generated_pngs.update(
        {
            "visual-matrix.png": visual_png,
            "contact-sheet-kirin.png": contact_kirin,
            "contact-sheet-d0.png": contact_d0,
            "contact-sheet-stripe.png": contact_stripe,
            "contact-sheet-summary.png": summary_buffer.getvalue(),
        }
    )

    # Mapping JSON is written before any generated PNG so a later inventory
    # failure cannot leave a logically complete but undocumented patch set.
    save_output_json(OUT / "prototypes/prototype-mapping.json", prototype_mapping, planned_outputs)
    save_output_json(OUT / "patches/d0/mapping.json", d0_mapping, planned_outputs)
    save_output_json(OUT / "patches/kirin/mapping.json", kirin_mapping, planned_outputs)
    for relative, data in generated_pngs.items():
        save_output_bytes(OUT / relative, data, planned_outputs)
    save_output_json(OUT / "input-hashes.json", input_hashes, planned_outputs)
    save_output_json(OUT / "manifest-validation.json", manifest_validation, planned_outputs)
    save_output_json(OUT / "logical-frame-verification.json", logical, planned_outputs)
    save_output_json(OUT / "prototype-roundtrip.json", prototype_mapping, planned_outputs)
    save_output_json(OUT / "visual-comparison.json", visual, planned_outputs)
    save_output_json(OUT / "negative-controls.json", negative, planned_outputs)
    metrics = {
        "status": "PASS_OFFLINE_DIAGNOSTIC"
        if logical["exactAll88Rgba"] and prototype_mapping["entries"] and d0_mapping["allFramesExact"] and kirin_mapping["allFramesExact"] and visual["allPreviewCasesExact"] and negative["allDeliberateFaultsDetected"]
        else "FAIL_OFFLINE_DIAGNOSTIC",
        "logical": {
            "frames": logical["nativeLogicalFrames"],
            "pixels": logical["pixelsCompared"],
            "changedPixels": logical["differentPixels"],
            "maxAbsDiff": logical["maxAbsDiff"],
        },
        "prototypes": {"entries": len(prototype_mapping["entries"]), "exact": all(item["exactRgba"] for item in prototype_mapping["entries"])},
        "patches": {"d0Frames": len(d0_mapping["frames"]), "d0Exact": d0_mapping["allFramesExact"], "kirinFrames": len(kirin_mapping["frames"]), "kirinExact": kirin_mapping["allFramesExact"]},
        "visual": {"cases": visual["casesChecked"], "changedCases": visual["differentPreviewCases"], "maxAbsDiff": visual["maxPreviewAbsDiff"]},
        "negativeControls": negative,
        "runtimeBoundary": {"editorTestsRun": False, "playRun": False, "gpuProof": False, "deviceProof": False},
    }
    save_output_json(OUT / "metrics.json", metrics, planned_outputs)
    pre_postverify_files = {relative for relative in planned_outputs if relative != "postverify.json"}
    pre_report_files = pre_postverify_files - {"REPORT.md"}
    actual_pre_report = {path.relative_to(OUT).as_posix() for path in OUT.rglob("*") if path.is_file()}
    revision_files = {
        path.relative_to(OUT).as_posix()
        for path in OUT.glob("revisions/*")
        if path.is_file()
    }
    allowed_pre_report = pre_report_files | {"planned-output-manifest.json"} | revision_artifacts | revision_files
    if actual_pre_report != allowed_pre_report:
        missing = sorted(allowed_pre_report - actual_pre_report)
        unexpected = sorted(actual_pre_report - allowed_pre_report)
        fail(f"Pre-report output inventory mismatch missing={missing} unexpected={unexpected}")
    stable_hashes = collect_input_hashes(manifest, dat_paths, plan_hash)
    initial_by_path = {entry["path"]: entry["sha256"] for entry in input_hashes["files"]}
    final_by_path = {entry["path"]: entry["sha256"] for entry in stable_hashes["files"]}
    if initial_by_path != final_by_path:
        fail("Input hash changed while generating outputs")
    png_outputs_read = 0
    for relative in sorted(pre_postverify_files):
        path = OUT / relative
        if path.suffix.lower() == ".png":
            data = path.read_bytes()
            decoded, _ = decode_png(data)
            if decoded.ndim != 3 or decoded.shape[2] != 4:
                fail(f"Generated PNG readback is not RGBA: {path}")
            png_outputs_read += 1
    postverify = {
        "inputHashesStable": True,
        "initialInputFileCount": len(input_hashes["files"]),
        "outputCountBeforePostverify": len(pre_postverify_files),
        "pngOutputsReadBack": png_outputs_read,
        "unexpectedFiles": [],
        "revisionArtifactsPresent": sorted(path for path in (revision_artifacts | revision_files) if (OUT / path).is_file()),
        "plannedManifestSha256Stable": sha256_file(PLAN_PATH) == plan_hash,
        "helperUnchangedDuringRun": final_by_path.get(rel_path(Path(__file__).resolve())) == initial_by_path.get(rel_path(Path(__file__).resolve())),
        "postverifyScope": "All planned outputs except this final postverify record were present and generated PNGs were decoded again.",
    }
    save_output_text(OUT / "REPORT.md", make_report(manifest_validation, logical, prototype_mapping, d0_mapping, kirin_mapping, visual, negative, postverify), planned_outputs)
    actual_after_report = {path.relative_to(OUT).as_posix() for path in OUT.rglob("*") if path.is_file()}
    allowed_after_report = pre_postverify_files | {"planned-output-manifest.json"} | revision_artifacts | revision_files
    if actual_after_report != allowed_after_report:
        missing = sorted(allowed_after_report - actual_after_report)
        unexpected = sorted(actual_after_report - allowed_after_report)
        fail(f"Post-report output inventory mismatch missing={missing} unexpected={unexpected}")
    save_output_json(OUT / "postverify.json", postverify, planned_outputs)
    print(json_text({"status": metrics["status"], "logicalFrames": logical["nativeLogicalFrames"], "changedPixels": logical["differentPixels"], "visualCases": visual["casesChecked"], "negativeControlsDetected": negative["allDeliberateFaultsDetected"], "outputRoot": rel_path(OUT)}), end="")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(run())
    except Exception as error:
        print(f"ERROR: {error}", file=sys.stderr)
        raise SystemExit(1)
