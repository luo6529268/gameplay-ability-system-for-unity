#!/usr/bin/env python3
"""Bake NTSD native clamped cells into an offline derived-page candidate.

The script is intentionally staging-only. It imports the existing Build and
Trim helpers without editing them, preserves original source/DAT identities,
and writes only below the requested new artifact directory. It does not run
Unity, encode ASTC, deploy a manifest, or write Assets.
"""

from __future__ import annotations

import argparse
import copy
import hashlib
import importlib.util
import json
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
DEPLOYED_ROOT = CONTENT_ROOT / "derived_sprite_pages"
BUILD_HELPER_PATH = ROOT / "Tools/Build-NtsdSpritePages.py"
TRIM_HELPER_PATH = ROOT / "Tools/Trim-NtsdSpritePages.py"
AUTHORITY_PATH = ROOT / "docs/ai/CURRENT-AUTHORITY.md"
CLAMP_CSHARP_PATH = ROOT / "Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs"
DEFAULT_SCOPE = ROOT / "artifacts/diagnostics/M12-astc-eight-role-20261013/clamp-bake-source-scope.json"
DEFAULT_OUTPUT = ROOT / "artifacts/diagnostics/M12-astc-eight-role-20261013/clamp-baked-candidate-worker"
OUT = DEFAULT_OUTPUT
SCOPE_PATH = DEFAULT_SCOPE
PAGE_LIMIT = 2048
EDGE_PADDING = 1
ALLOCATION_ALIGNMENT = 4
GENERATOR_VERSION = "NTSD_DAT_PAGES_V1"


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat()


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def rel_root(path: Path) -> str:
    return path.resolve().relative_to(ROOT.resolve()).as_posix()


def rel_output(path: Path) -> str:
    return path.resolve().relative_to(OUT.resolve()).as_posix()


def write_new_bytes(path: Path, data: bytes) -> None:
    if path.exists():
        raise RuntimeError(f"refusing to overwrite existing output: {path}")
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("xb") as stream:
        stream.write(data)


def write_new_json(path: Path, value: Any) -> None:
    write_new_bytes(path, (json.dumps(value, ensure_ascii=False, indent=2) + "\n").encode("utf-8"))


def write_new_text(path: Path, value: str) -> None:
    write_new_bytes(path, value.encode("utf-8"))


def metadata(path: Path) -> dict[str, Any]:
    return {"path": rel_root(path), "bytes": path.stat().st_size, "sha256": sha256_file(path)}


def load_module(name: str, path: Path):
    spec = importlib.util.spec_from_file_location(name, path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"unable to load helper: {path}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def align4(value: int) -> int:
    return (int(value) + ALLOCATION_ALIGNMENT - 1) // ALLOCATION_ALIGNMENT * ALLOCATION_ALIGNMENT


def source_path(relative: str) -> Path:
    path = (VFS_ROOT / Path(*relative.split("/"))).resolve()
    path.relative_to(VFS_ROOT.resolve())
    return path


def dat_path(relative: str) -> Path:
    path = (DAT_ROOT / Path(*relative.split("/"))).resolve()
    path.relative_to(DAT_ROOT.resolve())
    return path


def output_path(relative: str) -> Path:
    path = (OUT / Path(*relative.replace("\\", "/").split("/"))).resolve()
    path.relative_to(OUT.resolve())
    return path


def read_scope() -> dict[str, Any]:
    if not SCOPE_PATH.is_file():
        raise RuntimeError(f"scope file missing: {SCOPE_PATH}")
    scope = json.loads(SCOPE_PATH.read_text(encoding="utf-8"))
    sources = scope.get("sources")
    if not isinstance(sources, list) or not sources or any(not isinstance(source, str) for source in sources):
        raise RuntimeError("scope must contain a non-empty string sources list")
    normalized_sources = [source.replace("\\", "/").removeprefix("vfs/") for source in sources]
    if len(set(normalized_sources)) != len(normalized_sources):
        raise RuntimeError("scope contains duplicate sources")
    if scope.get("originalDATPNGProtected") is not True and scope.get("originalPNGDatProtected") is not True:
        raise RuntimeError("scope must explicitly protect original PNG/DAT inputs")
    if scope.get("onlyDerivedCandidate") is False:
        raise RuntimeError("scope cannot authorize non-derived output")
    scope["sources"] = normalized_sources
    source_headers = scope.get("sourceHeaders")
    if source_headers is not None:
        if not isinstance(source_headers, list):
            raise RuntimeError("scope sourceHeaders must be a list when present")
        header_sources: list[str] = []
        for header in source_headers:
            if not isinstance(header, dict) or not isinstance(header.get("source"), str):
                raise RuntimeError("scope sourceHeaders contains an invalid entry")
            header_source = str(header["source"]).replace("\\", "/").removeprefix("vfs/")
            if header_source not in normalized_sources:
                raise RuntimeError(f"scope sourceHeaders contains out-of-scope source: {header_source}")
            if header_source in header_sources:
                raise RuntimeError(f"scope sourceHeaders contains duplicate source: {header_source}")
            if int(header.get("width", 0)) <= 0 or int(header.get("height", 0)) <= 0:
                raise RuntimeError(f"scope source header dimensions are invalid: {header_source}")
            header_sources.append(header_source)
        if set(header_sources) != set(normalized_sources):
            raise RuntimeError("scope sourceHeaders must cover every source exactly once")
    return scope


def strict_build_failure(build: Any, sources: list[str]) -> dict[str, Any] | None:
    build.SOURCE_PATHS = list(sources)
    try:
        build.selected_declarations()
    except Exception as error:
        return {"type": type(error).__name__, "message": str(error)}
    return None


def resolve_scope(build: Any, scope: dict[str, Any]) -> dict[str, Any]:
    catalog = build.parse_catalog()
    declarations_by_source = build.parse_all_dat_declarations()
    strict_failure = strict_build_failure(build, list(scope["sources"]))
    scope_headers = {}
    for header in scope.get("sourceHeaders", []):
        source_key = str(header["source"]).replace("\\", "/").removeprefix("vfs/")
        scope_headers[source_key] = header
    resolved: list[dict[str, Any]] = []
    for source_rel in scope["sources"]:
        image_path = source_path(source_rel)
        actual_source_sha = sha256_file(image_path)
        with Image.open(image_path) as image:
            source_width, source_height = image.size
        expected_header = scope_headers.get(source_rel)
        if expected_header is not None:
            expected_dimensions = (int(expected_header["width"]), int(expected_header["height"]))
            if (int(source_width), int(source_height)) != expected_dimensions:
                raise RuntimeError(
                    f"scope source header dimensions mismatch at {source_rel}: "
                    f"scope={expected_dimensions[0]}x{expected_dimensions[1]} "
                    f"actual={source_width}x{source_height}"
                )
            expected_sha = str(expected_header.get("sourceSha256", "")).lower()
            if expected_sha and actual_source_sha.lower() != expected_sha:
                raise RuntimeError(
                    f"scope source header SHA mismatch at {source_rel}: "
                    f"scope={expected_sha} actual={actual_source_sha}"
                )
        declarations = declarations_by_source.get(build.normalize_path(source_rel), [])
        if not declarations:
            raise RuntimeError(f"no formal decoded DAT declaration resolves source: {source_rel}")
        geometries = sorted({(int(d["width"]), int(d["height"]), int(d["columns"]), int(d["rows"])) for d in declarations})
        bindings = []
        total_frames = 0
        total_clamped = 0
        anomaly_count = 0
        for declaration in declarations:
            dat_rel = str(declaration["datPath"])
            dat_file = dat_path(dat_rel)
            catalog_rows = catalog.get(build.normalize_path(dat_rel), [])
            if len(catalog_rows) != 1:
                raise RuntimeError(f"catalog must resolve one OID for {dat_rel}, got {len(catalog_rows)}")
            frame_width, frame_height, dat_row, dat_col = map(
                int,
                (declaration["width"], declaration["height"], declaration["columns"], declaration["rows"]),
            )
            count = dat_row * dat_col
            clamped: list[int] = []
            for local_pic in range(count):
                source_x = (local_pic % dat_row) * (frame_width + 1)
                source_y = source_height - (local_pic // dat_row + 1) * (frame_height + 1) + 1
                if source_x < 0 or source_y < 0 or source_x + frame_width > source_width or source_y + frame_height > source_height:
                    clamped.append(local_pic)
            range_anomaly = int(declaration["declaredCount"]) != count
            anomaly_count += int(range_anomaly)
            total_frames += count
            total_clamped += len(clamped)
            bindings.append(
                {
                    "oid": int(catalog_rows[0]["id"]),
                    "datPath": dat_rel,
                    "datSha256": sha256_file(dat_file),
                    "datBytes": dat_file.stat().st_size,
                    "firstPic": int(declaration["firstPic"]),
                    "lastPic": int(declaration["lastPic"]),
                    "datLine": int(declaration["line"]),
                    "declaredRange": [int(declaration["declaredStart"]), int(declaration["declaredEnd"])],
                    "declaredCount": int(declaration["declaredCount"]),
                    "rowColCount": count,
                    "geometry": [frame_width, frame_height, dat_row, dat_col],
                    "declaredRangeAnomaly": range_anomaly,
                    "clampedLocalPics": clamped,
                    "catalogSourcePath": str(catalog_rows[0]["source_path"]),
                }
            )
        excluded = len(geometries) != 1
        resolved.append(
            {
                "source": source_rel,
                "sourceAbsolutePath": str(image_path.resolve()),
                "sourceSha256": actual_source_sha,
                "sourceBytes": image_path.stat().st_size,
                "sourceDimensions": [int(source_width), int(source_height)],
                "scopeSourceHeader": copy.deepcopy(expected_header) if expected_header is not None else None,
                "declarationCount": len(declarations),
                "geometries": [list(geometry) for geometry in geometries],
                "status": "EXCLUDED_MULTI_GEOMETRY" if excluded else "SUPPORTED",
                "exclusionReason": "one source descriptor cannot represent multiple DAT geometries without a schema change" if excluded else None,
                "totalDeclaredCells": total_frames,
                "totalClampedCells": total_clamped,
                "declaredRangeAnomalyCount": anomaly_count,
                "bindings": bindings,
            }
        )
    return {
        "operation": scope["operation"],
        "scopeFile": rel_root(SCOPE_PATH),
        "scopeFileSha256": sha256_file(SCOPE_PATH),
        "frozenSourceCount": len(scope["sources"]),
        "resolvedSources": resolved,
        "supportedSources": [row["source"] for row in resolved if row["status"] == "SUPPORTED"],
        "excludedSources": [row["source"] for row in resolved if row["status"] != "SUPPORTED"],
        "supportedSourceCount": sum(row["status"] == "SUPPORTED" for row in resolved),
        "excludedSourceCount": sum(row["status"] != "SUPPORTED" for row in resolved),
        "frozenClampCells": sum(row["totalClampedCells"] for row in resolved),
        "supportedClampCells": sum(row["totalClampedCells"] for row in resolved if row["status"] == "SUPPORTED"),
        "declaredRangeAnomalyCount": sum(row["declaredRangeAnomalyCount"] for row in resolved),
        "strictBuildHelperSelectedDeclarationsFailure": strict_failure,
    }


def assert_scope_matches_frozen(current: dict[str, Any]) -> None:
    frozen_path = OUT / "exact-scope.json"
    if not frozen_path.is_file():
        raise RuntimeError(f"preflight exact scope missing: {frozen_path}")
    frozen = json.loads(frozen_path.read_text(encoding="utf-8"))
    for key in ("scopeFileSha256", "frozenSourceCount", "supportedSources", "excludedSources", "supportedClampCells", "frozenClampCells"):
        if current.get(key) != frozen.get(key):
            raise RuntimeError(f"scope drift at {key}: current={current.get(key)} frozen={frozen.get(key)}")
    frozen_by_source = {row["source"]: row for row in frozen["resolvedSources"]}
    for row in current["resolvedSources"]:
        if row["source"] not in frozen_by_source:
            raise RuntimeError(f"scope source was not present in frozen exact scope: {row['source']}")
        prior = frozen_by_source[row["source"]]
        for key in ("sourceSha256", "sourceBytes", "sourceDimensions", "status", "geometries", "totalDeclaredCells", "totalClampedCells"):
            if row[key] != prior[key]:
                raise RuntimeError(f"scope drift for {row['source']} at {key}")
        current_bindings = [(b["oid"], b["datPath"], b["datSha256"], b["firstPic"], b["lastPic"], b["geometry"]) for b in row["bindings"]]
        frozen_bindings = [(b["oid"], b["datPath"], b["datSha256"], b["firstPic"], b["lastPic"], b["geometry"]) for b in prior["bindings"]]
        if current_bindings != frozen_bindings:
            raise RuntimeError(f"scope binding drift for {row['source']}")


def clamp_frame_vectorized(source_top_left: np.ndarray, source_x: int, source_y: int, frame_width: int, frame_height: int) -> np.ndarray:
    if source_top_left.ndim != 3 or source_top_left.shape[2] != 4 or source_top_left.dtype != np.uint8:
        raise ValueError("source must be RGBA uint8")
    source_bottom_up = np.flipud(source_top_left)
    ys = np.clip(source_y + np.arange(frame_height, dtype=np.int64), 0, source_top_left.shape[0] - 1)
    xs = np.clip(source_x + np.arange(frame_width, dtype=np.int64), 0, source_top_left.shape[1] - 1)
    return np.flipud(source_bottom_up[np.ix_(ys, xs)].copy())


def clamp_frame_direct(source_top_left: np.ndarray, source_x: int, source_y: int, frame_width: int, frame_height: int) -> np.ndarray:
    source_bottom_up = np.flipud(source_top_left)
    output = np.zeros((frame_height, frame_width, 4), dtype=np.uint8)
    for y in range(frame_height):
        sampled_y = max(0, min(source_bottom_up.shape[0] - 1, source_y + y))
        for x in range(frame_width):
            sampled_x = max(0, min(source_bottom_up.shape[1] - 1, source_x + x))
            output[frame_height - 1 - y, x] = source_bottom_up[sampled_y, sampled_x]
    return output


def run_clamp_fixtures() -> dict[str, Any]:
    bottom = np.zeros((5, 6, 4), dtype=np.uint8)
    for y in range(bottom.shape[0]):
        for x in range(bottom.shape[1]):
            bottom[y, x] = np.array([(y * 17 + x) & 255, (y * 31 + x * 3) & 255, (y * 7 + x * 5) & 255, 255], dtype=np.uint8)
    source_top = np.flipud(bottom)
    cases = [
        {"name": "negative-single-cell", "sourceX": -2, "sourceY": -1, "width": 3, "height": 3},
        {"name": "multi-cell-mixed-edge", "sourceX": 4, "sourceY": 3, "width": 4, "height": 4},
        {"name": "normal-in-bounds", "sourceX": 1, "sourceY": 1, "width": 3, "height": 2},
    ]
    results = []
    for case in cases:
        vectorized = clamp_frame_vectorized(source_top, case["sourceX"], case["sourceY"], case["width"], case["height"])
        direct = clamp_frame_direct(source_top, case["sourceX"], case["sourceY"], case["width"], case["height"])
        results.append(
            {
                **case,
                "vectorizedSha256": sha256_bytes(vectorized.tobytes()),
                "directSha256": sha256_bytes(direct.tobytes()),
                "exact": bool(np.array_equal(vectorized, direct)),
            }
        )
    return {"status": "PASS" if all(row["exact"] for row in results) else "FAIL", "sourceDimensions": [6, 5], "cases": results}


def all_exact_aliases(records: list[dict[str, Any]]) -> dict[int, int]:
    aliases: dict[int, int] = {}
    physical_by_key: dict[tuple[Any, ...], list[int]] = {}
    for record in records:
        key = (
            str(record["sourceFrameSha256"]),
            int(record["contentRect"]["x"]),
            int(record["contentRect"]["y"]),
            int(record["contentRect"]["width"]),
            int(record["contentRect"]["height"]),
            int(record["width"]),
            int(record["height"]),
        )
        physical = None
        for candidate in physical_by_key.get(key, []):
            if np.array_equal(records[candidate]["crop"], record["crop"]):
                physical = candidate
                break
        if physical is None:
            physical_by_key.setdefault(key, []).append(int(record["localPic"]))
            aliases[int(record["localPic"])] = int(record["localPic"])
        else:
            aliases[int(record["localPic"])] = physical
    return aliases


def raw_region_info(source_array: np.ndarray, source_x: int, source_y: int, frame_width: int, frame_height: int) -> dict[str, Any]:
    top_y = source_array.shape[0] - source_y - frame_height
    x0, x1 = max(0, source_x), min(source_array.shape[1], source_x + frame_width)
    y0, y1 = max(0, top_y), min(source_array.shape[0], top_y + frame_height)
    if x0 >= x1 or y0 >= y1:
        data = np.zeros((0, 0, 4), dtype=np.uint8)
    else:
        data = source_array[y0:y1, x0:x1]
    return {"boxTopLeft": [int(x0), int(y0), int(max(0, x1 - x0)), int(max(0, y1 - y0))], "sha256": sha256_bytes(data.tobytes()), "bytes": int(data.size)}


def make_records(trim: Any, source: dict[str, Any], source_array: np.ndarray) -> tuple[list[dict[str, Any]], dict[int, int]]:
    frame_width = int(source["frameWidth"])
    frame_height = int(source["frameHeight"])
    dat_row = int(source["columns"])
    dat_col = int(source["rows"])
    records = []
    for local_pic in range(dat_row * dat_col):
        source_x = (local_pic % dat_row) * (frame_width + 1)
        source_y = source_array.shape[0] - (local_pic // dat_row + 1) * (frame_height + 1) + 1
        baked = clamp_frame_vectorized(source_array, source_x, source_y, frame_width, frame_height)
        record = trim.trim_frame(source, local_pic, baked, (source_x, source_y))
        raw_region = raw_region_info(source_array, source_x, source_y, frame_width, frame_height)
        in_bounds = source_x >= 0 and source_y >= 0 and source_x + frame_width <= source_array.shape[1] and source_y + frame_height <= source_array.shape[0]
        raw_top_y = source_array.shape[0] - source_y - frame_height
        raw_frame_sha = None
        if in_bounds:
            raw_frame = source_array[raw_top_y:raw_top_y + frame_height, source_x:source_x + frame_width]
            raw_frame_sha = sha256_bytes(raw_frame.tobytes())
            if not np.array_equal(raw_frame, baked):
                raise RuntimeError(f"normal grid changed by clamp formula: {source['path']} localPic={local_pic}")
        record.update(
            {
                "sourceXBottomLeft": int(source_x),
                "sourceYBottomLeft": int(source_y),
                "sourceTopLeftOrigin": [int(source_x), int(raw_top_y)],
                "clampApplied": not in_bounds,
                "rawSourceFrameSha256": raw_frame_sha,
                "rawSourceRegion": raw_region,
                "bakedFrameSha256": sha256_bytes(baked.tobytes()),
            }
        )
        records.append(record)
    aliases = all_exact_aliases(records)
    return records, aliases


def build_candidate_manifest(sources: list[dict[str, Any]], groups: dict[str, dict[str, Any]]) -> dict[str, Any]:
    pages = []
    source_descriptors = []
    for source in sources:
        group = groups[source["source"]]
        for page in group["pages"]:
            pages.append({"id": page["id"], "path": page["path"], "sha256": page["sha256"], "width": int(page["width"]), "height": int(page["height"])})
        source_descriptors.append(
            {
                "path": source["source"],
                "sha256": source["sourceSha256"],
                "width": int(source["sourceDimensions"][0]),
                "height": int(source["sourceDimensions"][1]),
                "frameWidth": int(source["geometries"][0][0]),
                "frameHeight": int(source["geometries"][0][1]),
                "columns": int(source["geometries"][0][2]),
                "rows": int(source["geometries"][0][3]),
                "clampBaked": True,
                "bindings": [
                    {"oid": int(binding["oid"]), "datPath": binding["datPath"], "datSha256": binding["datSha256"], "firstPic": int(binding["firstPic"]), "lastPic": int(binding["lastPic"])}
                    for binding in source["bindings"]
                ],
                "chunks": copy.deepcopy(group["chunks"]),
            }
        )
    return {"version": 1, "generatorVersion": GENERATOR_VERSION, "pages": pages, "sources": source_descriptors}


def protected_snapshot() -> dict[str, Any]:
    manifest_path = DEPLOYED_ROOT / "manifest.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    paths = [manifest_path] + [DEPLOYED_ROOT / Path(*str(page["path"]).split("/")) for page in manifest["pages"]]
    return {"utc": utc_now(), "pageCount": len(manifest["pages"]), "paths": [{**metadata(path), "absolutePath": str(path.resolve())} for path in paths]}


def compare_snapshots(before: dict[str, Any], after: dict[str, Any]) -> dict[str, Any]:
    left = {row["path"]: row for row in before["paths"]}
    right = {row["path"]: row for row in after["paths"]}
    rows = []
    for path in sorted(set(left) | set(right)):
        a, b = left.get(path), right.get(path)
        rows.append({"path": path, "beforeSha256": None if a is None else a["sha256"], "afterSha256": None if b is None else b["sha256"], "unchanged": a is not None and b is not None and a["sha256"] == b["sha256"] and a["bytes"] == b["bytes"]})
    return {"allUnchanged": all(row["unchanged"] for row in rows), "rows": rows}


def selected_input_paths(current: dict[str, Any]) -> list[Path]:
    paths = [Path(__file__).resolve(), SCOPE_PATH.resolve(), AUTHORITY_PATH.resolve(), BUILD_HELPER_PATH.resolve(), TRIM_HELPER_PATH.resolve(), CLAMP_CSHARP_PATH.resolve()]
    for row in current["resolvedSources"]:
        paths.append(Path(row["sourceAbsolutePath"]).resolve())
        for binding in row["bindings"]:
            paths.append(dat_path(str(binding["datPath"])).resolve())
    unique: dict[str, Path] = {}
    for path in paths:
        unique[str(path).casefold()] = path
    return [unique[key] for key in sorted(unique)]


def input_snapshot(current: dict[str, Any]) -> dict[str, Any]:
    return {
        "scope": rel_root(SCOPE_PATH),
        "inputs": [{**metadata(path), "absolutePath": str(path.resolve())} for path in selected_input_paths(current)],
    }


def load_context(build: Any, initialize: bool = False) -> tuple[dict[str, Any], dict[str, Any], list[dict[str, Any]]]:
    scope = read_scope()
    current = resolve_scope(build, scope)
    frozen_path = OUT / "exact-scope.json"
    if frozen_path.exists():
        assert_scope_matches_frozen(current)
    elif initialize:
        write_new_json(frozen_path, current)
    else:
        raise RuntimeError(f"preflight exact scope missing: {frozen_path}")
    supported = [row for row in current["resolvedSources"] if row["status"] == "SUPPORTED"]
    return scope, current, supported


def stage_plan(build: Any, trim: Any) -> None:
    scope, current, _ = load_context(build, initialize=True)
    fixtures = run_clamp_fixtures()
    if fixtures["status"] != "PASS":
        raise RuntimeError("clamp fixtures failed")
    protected_before = protected_snapshot()
    write_new_json(OUT / "input-hashes-before.json", input_snapshot(current))
    write_new_json(OUT / "fixture-validation.json", fixtures)
    write_new_json(OUT / "protected-inputs-before.json", protected_before)
    write_new_json(
        OUT / "helper-identity.json",
        {
            "utc": utc_now(),
            "newHelper": {**metadata(Path(__file__)), "absolutePath": str(Path(__file__).resolve())},
            "buildHelper": {**metadata(BUILD_HELPER_PATH), "absolutePath": str(BUILD_HELPER_PATH.resolve())},
            "trimHelper": {**metadata(TRIM_HELPER_PATH), "absolutePath": str(TRIM_HELPER_PATH.resolve())},
            "generatorVersion": GENERATOR_VERSION,
        },
    )
    write_new_json(
        OUT / "helper-runtime-plan.json",
        {
            "status": "PLAN_PASS",
            "utc": utc_now(),
            "supportedSourceCount": current["supportedSourceCount"],
            "excludedSources": current["excludedSources"],
            "supportedClampCells": current["supportedClampCells"],
            "strictBuildHelperSelectedDeclarationsFailure": current["strictBuildHelperSelectedDeclarationsFailure"],
            "scopeSourceHeaderCount": len(scope.get("sourceHeaders", [])),
            "exactScope": "exact-scope.json",
            "inputHashesBefore": "input-hashes-before.json",
            "fixture": "fixture-validation.json",
            "protectedBefore": "protected-inputs-before.json",
            "nextStages": ["produce", "verify"],
            "astcRun": False,
        },
    )


def stage_produce(build: Any, trim: Any) -> None:
    scope, current, supported = load_context(build)
    fixture_path = OUT / "fixture-validation.json"
    if not fixture_path.is_file() or json.loads(fixture_path.read_text(encoding="utf-8"))["status"] != "PASS":
        raise RuntimeError("fixture validation must pass before produce")
    groups: dict[str, dict[str, Any]] = {}
    source_arrays: dict[str, np.ndarray] = {}
    original_alias = trim.build_trailing_aliases
    alias_observations = []
    try:
        for source in supported:
            source_array = np.asarray(Image.open(source_path(source["source"])).convert("RGBA"), dtype=np.uint8).copy()
            source_arrays[source["source"]] = source_array
            geometry = source["geometries"][0]
            trim_source = {"path": source["source"], "columns": int(geometry[2]), "rows": int(geometry[3]), "frameWidth": int(geometry[0]), "frameHeight": int(geometry[1])}
            records, aliases = make_records(trim, trim_source, source_array)
            observed = original_alias(records)
            alias_observations.append({"source": source["source"], "existingHelperTrailingAliases": [local for local, physical in observed.items() if local != physical], "workerAllExactAliases": [local for local, physical in aliases.items() if local != physical]})
            trim.build_trailing_aliases = all_exact_aliases
            group = trim.build_trim_group(source["sourceSha256"], records, "clamp-baked-" + source["source"].replace("/", "-").replace(".png", ""))
            group["aliases"] = aliases
            groups[source["source"]] = group
    finally:
        trim.build_trailing_aliases = original_alias

    candidate = build_candidate_manifest(supported, groups)
    page_arrays = {}
    edge_checks = {}
    for source in supported:
        group = groups[source["source"]]
        page_arrays.update({page["id"]: page["array"] for page in group["pages"]})
        edge_checks[source["source"]] = trim.validate_new_page_edges(group, {page["id"]: page["array"] for page in group["pages"]})
    for group in groups.values():
        for page in group["pages"]:
            write_new_bytes(output_path(page["path"]), page["payload"])
    write_new_json(OUT / "candidate-manifest.json", candidate)

    frame_contract_sources = []
    logical_mappings = []
    for source in supported:
        group = groups[source["source"]]
        records_by_local = {int(record["localPic"]): record for record in group["records"]}
        mappings_by_local = {int(mapping["localPic"]): mapping for mapping in group["mappings"]}
        geometry = source["geometries"][0]
        frame_rows = []
        for local_pic in range(int(geometry[2]) * int(geometry[3])):
            record = records_by_local[local_pic]
            mapping = mappings_by_local[local_pic]
            content = mapping["contentRect"]
            pivot = {"x": (int(geometry[0]) * 0.5 - int(content["x"])) / int(content["width"]), "y": -int(content["y"]) / int(content["height"])}
            base = {
                "source": source["source"],
                "localPic": local_pic,
                "physicalLocalPic": int(mapping["physicalLocalPic"]),
                "alias": bool(mapping["alias"]),
                "nativeCanvas": {"width": int(geometry[0]), "height": int(geometry[1])},
                "nativecanvas": [int(geometry[0]), int(geometry[1])],
                "pic": {"localPic": local_pic},
                "sourceXBottomLeft": record["sourceXBottomLeft"],
                "sourceYBottomLeft": record["sourceYBottomLeft"],
                "sourceTopLeftOrigin": record["sourceTopLeftOrigin"],
                "clampApplied": bool(record["clampApplied"]),
                "rawSourceFrameSha256": record["rawSourceFrameSha256"],
                "rawSourceRegion": record["rawSourceRegion"],
                "bakedFrameSha256": record["bakedFrameSha256"],
                "cropSha256": record["cropSha256"],
                "page": mapping["page"],
                "pagePath": mapping["pagePath"],
                "rect": mapping["rect"],
                "contentRect": content,
                "pivot": pivot,
            }
            frame_rows.append(base)
            for binding in source["bindings"]:
                native_pic = int(binding["firstPic"]) + local_pic
                if native_pic > int(binding["lastPic"]):
                    raise RuntimeError(f"native pic outside binding: {source['source']} local={local_pic}")
                logical = copy.deepcopy(base)
                logical["pic"] = {"localPic": local_pic, "nativePic": native_pic}
                logical["ownership"] = {"oid": int(binding["oid"]), "datPath": binding["datPath"], "firstPic": int(binding["firstPic"]), "lastPic": int(binding["lastPic"]), "ownsNativePic": True}
                logical["datSha256"] = binding["datSha256"]
                logical_mappings.append(logical)
        frame_contract_sources.append({"source": source["source"], "sourceSha256": source["sourceSha256"], "nativeCanvas": {"width": int(geometry[0]), "height": int(geometry[1])}, "grid": {"columns": int(geometry[2]), "rows": int(geometry[3])}, "bindings": copy.deepcopy(source["bindings"]), "physicalFrameCount": int(group["physicalFrameCount"]), "exactAliasCount": int(group["aliasFrameCount"]), "frames": frame_rows})

    frame_contract = {"schemaVersion": 1, "generatorVersion": GENERATOR_VERSION, "clampBaked": True, "sources": frame_contract_sources, "logicalMappings": logical_mappings, "logicalMappingCount": len(logical_mappings), "uniqueFrameCount": sum(len(source["frames"]) for source in frame_contract_sources), "physicalFrameCount": sum(source["physicalFrameCount"] for source in frame_contract_sources), "exactAliasCount": sum(source["exactAliasCount"] for source in frame_contract_sources), "independentVerification": "independent-verification.json"}
    write_new_json(OUT / "frame-contract.json", frame_contract)

    comparison_sources = []
    for source in supported:
        group = groups[source["source"]]
        pages = group["pages"]
        source_png_bytes = int(source["sourceBytes"])
        source_rgba_bytes = int(source["sourceDimensions"][0]) * int(source["sourceDimensions"][1]) * 4
        page_rgba = sum(int(page["width"]) * int(page["height"]) * 4 for page in pages)
        page_png = sum(len(page["payload"]) for page in pages)
        logical_baked = sum(int(record["rgbaBytes"]) for record in group["records"])
        physical_baked = sum(int(group["records"][local]["rgbaBytes"]) for local, physical in group["aliases"].items() if local == physical)
        comparison_sources.append({"source": source["source"], "sourceSha256": source["sourceSha256"], "sourcePngBytes": source_png_bytes, "sourceSheetRgbaBytes": source_rgba_bytes, "logicalBakedFrameRgbaBytes": logical_baked, "physicalBakedCropRgbaBytes": physical_baked, "clampedCells": int(source["totalClampedCells"]), "candidatePageCount": len(pages), "candidatePageRgbaBytes": page_rgba, "candidatePagePngBytes": page_png, "pageIds": [page["id"] for page in pages]})
    totals = {"sourcePngBytes": sum(row["sourcePngBytes"] for row in comparison_sources), "sourceSheetRgbaBytes": sum(row["sourceSheetRgbaBytes"] for row in comparison_sources), "logicalBakedFrameRgbaBytes": sum(row["logicalBakedFrameRgbaBytes"] for row in comparison_sources), "physicalBakedCropRgbaBytes": sum(row["physicalBakedCropRgbaBytes"] for row in comparison_sources), "candidatePageRgbaBytes": sum(row["candidatePageRgbaBytes"] for row in comparison_sources), "candidatePagePngBytes": sum(row["candidatePagePngBytes"] for row in comparison_sources), "candidatePageCount": sum(row["candidatePageCount"] for row in comparison_sources)}
    write_new_json(OUT / "resource-comparison.json", {"sources": comparison_sources, "totals": totals, "astc": "not run by this helper"})

    write_new_json(OUT / "helper-call-receipt.json", {"utc": utc_now(), "newHelper": rel_root(Path(__file__)), "buildHelper": metadata(BUILD_HELPER_PATH), "trimHelper": metadata(TRIM_HELPER_PATH), "buildCalls": ["parse_catalog", "parse_all_dat_declarations", "parse_dat_file", "source_absolute", "dat_absolute", "sha256_file"], "trimCalls": ["trim_frame", "build_trim_group", "choose_shelf", "simulate_shelf", "render_page", "validate_new_page_edges"], "strictBuildSelectedDeclarationsFailure": current["strictBuildHelperSelectedDeclarationsFailure"], "aliasObservations": alias_observations, "excludedSources": current["excludedSources"], "astcRun": False})
    write_new_json(OUT / "candidate-entry.json", {"status": "OFFLINE_CLAMP_BAKED_CANDIDATE", "manifest": "candidate-manifest.json", "pageDirectory": "pages", "sourceCount": len(supported), "excludedSources": current["excludedSources"], "clampBaked": True, "generatorVersion": GENERATOR_VERSION, "astcRun": False, "independentVerification": "independent-verification.json"})
    write_new_text(OUT / "PRODUCE-REPORT.md", "\n".join(["# Clamp-baked candidate production", "", "Status: `PRODUCE_PASS` (no ASTC, no Unity)", "", f"- Supported sources: `{len(supported)}`; excluded: `{current['excludedSources']}`", f"- Supported clamp cells: `{current['supportedClampCells']}`", f"- Candidate pages: `{totals['candidatePageCount']}`", f"- Candidate page RGBA bytes: `{totals['candidatePageRgbaBytes']}`", f"- Candidate page PNG bytes: `{totals['candidatePagePngBytes']}`", f"- Exact aliases: `{frame_contract['exactAliasCount']}`", "- Original source PNG/DAT/current manifest/old pages remain protected; independent verify is the next stage.", ""]) )


def page_cell(page: np.ndarray, rect: dict[str, Any]) -> np.ndarray:
    x, bottom_y, width, height = int(rect["x"]), int(rect["y"]), int(rect["width"]), int(rect["height"])
    top_y = page.shape[0] - bottom_y - height
    if x < 0 or bottom_y < 0 or width <= 0 or height <= 0 or x + width > page.shape[1] or top_y < 0 or top_y + height > page.shape[0]:
        raise RuntimeError(f"page rect out of bounds: {rect} page={page.shape}")
    return page[top_y:top_y + height, x:x + width]


def reconstruct(frame_width: int, frame_height: int, crop: np.ndarray, content: dict[str, Any]) -> np.ndarray:
    x, y, width, height = int(content["x"]), int(content["y"]), int(content["width"]), int(content["height"])
    if crop.shape[:2] != (height, width):
        raise RuntimeError(f"contentRect/crop mismatch: {content} crop={crop.shape}")
    result = np.zeros((frame_height, frame_width, 4), dtype=np.uint8)
    top_y = frame_height - y - height
    result[top_y:top_y + height, x:x + width] = crop
    return result


def stage_verify(build: Any, trim: Any) -> None:
    scope, current, supported = load_context(build)
    candidate_path = OUT / "candidate-manifest.json"
    if not candidate_path.is_file():
        raise RuntimeError("candidate manifest missing before verify")
    candidate = json.loads(candidate_path.read_text(encoding="utf-8"))
    page_by_id = {str(page["id"]): page for page in candidate["pages"]}
    page_arrays: dict[str, np.ndarray] = {}
    page_checks = []
    for page_id, page in page_by_id.items():
        path = output_path(str(page["path"]))
        actual_sha = sha256_file(path)
        if actual_sha.lower() != str(page["sha256"]).lower():
            raise RuntimeError(f"candidate page SHA mismatch: {page_id}")
        with Image.open(path) as image:
            array = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
        if [int(array.shape[1]), int(array.shape[0])] != [int(page["width"]), int(page["height"])] or int(page["width"]) > PAGE_LIMIT or int(page["height"]) > PAGE_LIMIT or int(page["width"]) % 4 or int(page["height"]) % 4:
            raise RuntimeError(f"candidate page dimension/alignment invalid: {page_id}")
        page_arrays[page_id] = array
        page_checks.append({"id": page_id, "path": str(page["path"]), "sha256": actual_sha, "bytes": path.stat().st_size, "width": int(page["width"]), "height": int(page["height"])})

    expected_by_source = {row["source"]: row for row in supported}
    used_pages: set[str] = set()
    rows = []
    source_results = []
    for source_descriptor in candidate["sources"]:
        source_rel = str(source_descriptor["path"])
        if source_rel not in expected_by_source:
            raise RuntimeError(f"candidate contains out-of-scope source: {source_rel}")
        source = expected_by_source[source_rel]
        if source_descriptor.get("clampBaked") is not True:
            raise RuntimeError(f"candidate source missing clampBaked marker: {source_rel}")
        geometry = source["geometries"][0]
        expected_bindings = [{"oid": int(binding["oid"]), "datPath": binding["datPath"], "datSha256": binding["datSha256"], "firstPic": int(binding["firstPic"]), "lastPic": int(binding["lastPic"])} for binding in source["bindings"]]
        actual_bindings = [{"oid": int(binding["oid"]), "datPath": str(binding["datPath"]), "datSha256": str(binding["datSha256"]), "firstPic": int(binding["firstPic"]), "lastPic": int(binding["lastPic"])} for binding in source_descriptor["bindings"]]
        if expected_bindings != actual_bindings:
            raise RuntimeError(f"candidate binding mismatch: {source_rel}")
        source_array = np.asarray(Image.open(source_path(source_rel)).convert("RGBA"), dtype=np.uint8).copy()
        mapping: dict[int, tuple[dict[str, Any], int]] = {}
        next_local = 0
        for chunk in source_descriptor["chunks"]:
            first, count = int(chunk["firstLocalPic"]), int(chunk["count"])
            if first != next_local or len(chunk["rects"]) != count or len(chunk.get("contentRects", [])) != count:
                raise RuntimeError(f"candidate chunk sequence/content rect mismatch: {source_rel}")
            for offset in range(count):
                mapping[first + offset] = (chunk, offset)
            next_local += count
        capacity = int(geometry[2]) * int(geometry[3])
        if next_local != capacity:
            raise RuntimeError(f"candidate omitted logical frames: {source_rel}")
        frame_results = []
        exact_groups: dict[tuple[str, int, int, int, int], list[tuple[str, dict[str, Any]]]] = {}
        for local_pic in range(capacity):
            chunk, offset = mapping[local_pic]
            page_id = str(chunk["page"])
            used_pages.add(page_id)
            rect = chunk["rects"][offset]
            content = chunk["contentRects"][offset]
            actual_crop = page_cell(page_arrays[page_id], rect)
            baked_expected = clamp_frame_vectorized(source_array, (local_pic % int(geometry[2])) * (int(geometry[0]) + 1), source_array.shape[0] - (local_pic // int(geometry[2]) + 1) * (int(geometry[1]) + 1) + 1, int(geometry[0]), int(geometry[1]))
            reconstructed = reconstruct(int(geometry[0]), int(geometry[1]), actual_crop, content)
            exact = bool(np.array_equal(baked_expected, reconstructed))
            if not exact:
                raise RuntimeError(f"independent baked RGBA mismatch: {source_rel} localPic={local_pic}")
            baked_sha = sha256_bytes(baked_expected.tobytes())
            key = (baked_sha, int(content["x"]), int(content["y"]), int(content["width"]), int(content["height"]))
            exact_groups.setdefault(key, []).append((page_id, rect))
            clamp_applied = any(value for value in [
                (local_pic % int(geometry[2])) * (int(geometry[0]) + 1) < 0,
                source_array.shape[0] - (local_pic // int(geometry[2]) + 1) * (int(geometry[1]) + 1) + 1 < 0,
                (local_pic % int(geometry[2])) * (int(geometry[0]) + 1) + int(geometry[0]) > source_array.shape[1],
                source_array.shape[0] - (local_pic // int(geometry[2]) + 1) * (int(geometry[1]) + 1) + 1 + int(geometry[1]) > source_array.shape[0],
            ])
            for binding in source["bindings"]:
                native_pic = int(binding["firstPic"]) + local_pic
                if native_pic > int(binding["lastPic"]):
                    raise RuntimeError(f"native range mismatch: {source_rel} localPic={local_pic}")
                rows.append({"source": source_rel, "oid": int(binding["oid"]), "datPath": binding["datPath"], "datSha256": binding["datSha256"], "firstPic": int(binding["firstPic"]), "lastPic": int(binding["lastPic"]), "localPic": local_pic, "nativePic": native_pic, "page": page_id, "rect": rect, "contentRect": content, "bakedFrameSha256": baked_sha, "reconstructedRgbaSha256": sha256_bytes(reconstructed.tobytes()), "clampApplied": clamp_applied, "fullRgbaExact": exact})
            frame_results.append({"localPic": local_pic, "page": page_id, "rect": rect, "contentRect": content, "bakedFrameSha256": baked_sha, "clampApplied": clamp_applied, "fullRgbaExact": exact})
        for key, placements in exact_groups.items():
            if len(placements) > 1 and any(placements[index] != placements[0] for index in range(1, len(placements))):
                raise RuntimeError(f"exact duplicate frames were not shared: {source_rel} key={key}")
        source_results.append({"source": source_rel, "frameCount": capacity, "bindingCount": len(source["bindings"]), "clampedFrameCount": sum(int(row["clampApplied"]) for row in frame_results), "fullRgbaExact": all(row["fullRgbaExact"] for row in frame_results), "frames": frame_results})
    if used_pages != set(page_by_id):
        raise RuntimeError(f"candidate contains unused pages: {sorted(set(page_by_id) - used_pages)}")

    before = json.loads((OUT / "input-hashes-before.json").read_text(encoding="utf-8"))
    input_after = []
    input_rows = []
    for item in before["inputs"]:
        path = Path(item["absolutePath"])
        actual = {"path": item["path"], "absolutePath": str(path.resolve()), "bytes": path.stat().st_size, "sha256": sha256_file(path)}
        input_after.append(actual)
        input_rows.append({"path": item["path"], "beforeSha256": item["sha256"], "afterSha256": actual["sha256"], "beforeBytes": item["bytes"], "afterBytes": actual["bytes"], "unchanged": item["sha256"].lower() == actual["sha256"].lower() and int(item["bytes"]) == actual["bytes"]})
    input_comparison = {"allUnchanged": all(row["unchanged"] for row in input_rows), "rows": input_rows}
    protected_after = protected_snapshot()
    protected_before = json.loads((OUT / "protected-inputs-before.json").read_text(encoding="utf-8"))
    protected_comparison = compare_snapshots(protected_before, protected_after)
    write_new_json(OUT / "input-hashes-after.json", input_after)
    write_new_json(OUT / "input-hashes-comparison.json", input_comparison)
    write_new_json(OUT / "protected-inputs-after.json", protected_after)
    write_new_json(OUT / "protected-inputs-comparison.json", protected_comparison)
    result = {"status": "PASS", "method": "Independent disk readback: raw source PNG + formal DAT parser + candidate manifest/page PNGs; generation arrays were not reused.", "scopeSourceCount": len(scope["sources"]), "supportedSourceCount": len(supported), "excludedSources": current["excludedSources"], "nativeLogicalMappings": len(rows), "fullRgbaReconstructionExact": all(row["fullRgbaExact"] for row in rows), "pageCount": len(page_checks), "allPagesAtMost2048AndAligned": all(item["width"] <= PAGE_LIMIT and item["height"] <= PAGE_LIMIT and item["width"] % 4 == 0 and item["height"] % 4 == 0 for item in page_checks), "allPagesUsed": True, "selectedInputsUnchanged": bool(input_comparison["allUnchanged"]), "protectedManifestAndOldPagesUnchanged": bool(protected_comparison["allUnchanged"]), "pages": page_checks, "sources": source_results, "logicalMappings": rows}
    write_new_json(OUT / "independent-verification.json", result)
    write_new_text(OUT / "REPORT.md", "\n".join(["# NTSD clamp-baked candidate", "", "Status: `PASS` (offline candidate only; ASTC not run)", "", f"- Frozen sources: `{len(scope['sources'])}`", f"- Supported sources: `{len(supported)}`", f"- Excluded: `{current['excludedSources']}`", f"- Supported clamp cells: `{current['supportedClampCells']}`", f"- Native logical mappings independently verified: `{len(rows)}`", f"- Full baked RGBA reconstruction exact: `{result['fullRgbaReconstructionExact']}`", f"- Candidate pages: `{len(page_checks)}`", f"- Page <=2048 and 4-aligned: `{result['allPagesAtMost2048AndAligned']}`", f"- Source/DAT/helper input hashes unchanged: `{result['selectedInputsUnchanged']}`", f"- Current manifest and old pages unchanged: `{result['protectedManifestAndOldPagesUnchanged']}`", "- No ASTC encode, Unity, Play, P1, deployment, Assets write, C# write, DAT write, or existing helper write was performed.", ""]) )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--stage", choices=("plan", "produce", "verify", "all"), required=True)
    parser.add_argument("--scope", type=Path, default=DEFAULT_SCOPE)
    parser.add_argument("--output-root", type=Path, default=DEFAULT_OUTPUT)
    return parser.parse_args()


def main() -> int:
    global OUT, SCOPE_PATH
    args = parse_args()
    OUT = args.output_root.resolve()
    SCOPE_PATH = args.scope.resolve()
    OUT.mkdir(parents=True, exist_ok=True)
    build = load_module("ntsd_bake_build", BUILD_HELPER_PATH)
    trim = load_module("ntsd_bake_trim", TRIM_HELPER_PATH)
    if args.stage in ("plan", "all"):
        stage_plan(build, trim)
    if args.stage in ("produce", "all"):
        stage_produce(build, trim)
    if args.stage in ("verify", "all"):
        stage_verify(build, trim)
    print(json.dumps({"status": "PASS", "stage": args.stage, "outputRoot": str(OUT)}, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as error:
        failure = {"status": "FAIL", "type": type(error).__name__, "message": str(error), "traceback": traceback.format_exc(), "utc": utc_now()}
        failure_path = OUT / "worker-failure.json"
        if not failure_path.exists():
            write_new_json(failure_path, failure)
        print(json.dumps(failure, ensure_ascii=False, indent=2), file=sys.stderr)
        raise
