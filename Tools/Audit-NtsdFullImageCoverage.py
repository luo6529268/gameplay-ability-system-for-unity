#!/usr/bin/env python3
"""Independent full-catalog NTSD image/dependency/memory audit.

The audit deliberately imports the two existing offline audit helpers instead of
copying their capture traversal and production budget rules.  It owns the
full-50 orchestration, inventory witnesses, capability variants, and comparison
with the separately produced C# selector output.  It never writes production
assets or Unity project files.

All estimates in this file are image allocation estimates.  They are not Unity
process peaks, phone memory measurements, device certification, or whole-game
acceptance.
"""

from __future__ import annotations

import argparse
import csv
import copy
import hashlib
import importlib.util
import json
import math
import re
import struct
import sys
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Iterable


AUDIT_ID = "NTSD-M12-FULL-IMAGE-COVERAGE-001"
TASK_ID = "NTSD-M12-FULL-IMAGE-COVERAGE-20261014"
OPERATION_ID = "OP-NTSD-M12-FULL-IMAGE-COVERAGE-20261014"
PAGE_SIZE = 2048
RGBA_BYTES_PER_PIXEL = 4
ASTC_BLOCK_BYTES = 16
ASTC_HEADER_BYTES = 16
EXPECTED_OBJECTS = 330
EXPECTED_SELECTABLE = 50
EXPECTED_PAGES = 56
EXPECTED_PAGE_SOURCES = 35
EXPECTED_ASTC_MAPPINGS = 723
EXPECTED_ASTC_BLOCKS = 714
EXPECTED_UI_UNIQUE = 203
CAPTURE_DEFAULT = "artifacts/diagnostics/M12-astc-eight-role-20261013/eight-role-audit-worker/capture/role-resource-capture.json"
PAGES_DEFAULT = "Assets/NTSD/Content/LoganRuntime/derived_sprite_pages/manifest.json"
ASTC_DEFAULT = "Assets/NTSD/Content/LoganRuntime/derived_astc/manifest.json"
COMMON_DEFAULT = "artifacts/diagnostics/M12-astc-eight-role-20261013/common-assets-static-budget.json"


class AuditFailure(RuntimeError):
    """Input or invariant failure that must remain visible in diagnostics."""


def normalize_path(value: Any) -> str:
    return str(value or "").replace("\\", "/")


def path_from(value: Any, repository_root: Path) -> Path:
    candidate = Path(str(value)) if value else Path("")
    if candidate.is_absolute():
        return candidate.resolve()
    return (repository_root / candidate).resolve()


def sha256_bytes(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest()


def sha256_json(value: Any) -> str:
    encoded = json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
    return sha256_bytes(encoded)


def read_json(path: Path) -> Any:
    with path.open("r", encoding="utf-8") as stream:
        return json.load(stream)


def write_json_new(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("x", encoding="utf-8", newline="\n") as stream:
        json.dump(value, stream, ensure_ascii=False, indent=2, sort_keys=False)
        stream.write("\n")


def write_text_new(path: Path, value: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("x", encoding="utf-8", newline="\n") as stream:
        stream.write(value)


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def png_header(path: Path) -> dict[str, Any] | None:
    try:
        with path.open("rb") as stream:
            prefix = stream.read(33)
        if len(prefix) < 33 or prefix[:8] != b"\x89PNG\r\n\x1a\n" or prefix[12:16] != b"IHDR":
            return None
        width, height, bit_depth, color_type, compression, filter_method, interlace = struct.unpack(">IIBBBBB", prefix[16:29])
        return {
            "signature": prefix[:8].hex(),
            "chunk": "IHDR",
            "width": width,
            "height": height,
            "bitDepth": bit_depth,
            "colorType": color_type,
            "compression": compression,
            "filter": filter_method,
            "interlace": interlace,
            "headerHex": prefix.hex(),
        }
    except (OSError, struct.error):
        return None


def astc_header(path: Path) -> dict[str, Any] | None:
    try:
        with path.open("rb") as stream:
            header = stream.read(ASTC_HEADER_BYTES)
        if len(header) != ASTC_HEADER_BYTES or header[:4] != b"\x13\xab\xa1\x5c":
            return None
        width = header[7] | (header[8] << 8) | (header[9] << 16)
        height = header[10] | (header[11] << 8) | (header[12] << 16)
        depth = header[13] | (header[14] << 8) | (header[15] << 16)
        expected_payload = math.ceil(width / 4) * math.ceil(height / 4) * ASTC_BLOCK_BYTES
        return {
            "magicHex": header[:4].hex(),
            "blockX": header[4],
            "blockY": header[5],
            "blockZ": header[6],
            "width": width,
            "height": height,
            "depth": depth,
            "payloadBytes": expected_payload,
            "headerHex": header.hex(),
            "fileBytes": path.stat().st_size,
            "validAstc4x4": header[:7] == bytes([0x13, 0xAB, 0xA1, 0x5C, 4, 4, 1]),
            "validLength": path.stat().st_size == expected_payload + ASTC_HEADER_BYTES,
        }
    except (OSError, struct.error):
        return None


class IdentityCache:
    """One exact SHA/header read per live path for this audit invocation."""

    def __init__(self, repository_root: Path):
        self.repository_root = repository_root
        self.identities: dict[str, dict[str, Any]] = {}
        self.png_headers: dict[str, dict[str, Any] | None] = {}
        self.astc_headers: dict[str, dict[str, Any] | None] = {}

    def _key(self, path: Path) -> str:
        return str(path.resolve()).lower()

    def identity(self, value: Any, expected_sha: str | None = None) -> dict[str, Any]:
        path = path_from(value, self.repository_root)
        key = self._key(path)
        base = self.identities.get(key)
        if base is None:
            exists = path.is_file()
            base = {
                "path": str(path),
                "exists": exists,
                "bytes": path.stat().st_size if exists else 0,
                "sha256": sha256_file(path) if exists else None,
            }
            self.identities[key] = base
        result = dict(base)
        result["expectedSha256"] = expected_sha
        result["shaMatch"] = None if not result["exists"] else expected_sha is None or str(result["sha256"]).lower() == str(expected_sha).lower()
        return result

    def png(self, value: Any) -> dict[str, Any] | None:
        path = path_from(value, self.repository_root)
        key = self._key(path)
        if key not in self.png_headers:
            self.png_headers[key] = png_header(path) if path.is_file() else None
        return self.png_headers[key]

    def astc(self, value: Any) -> dict[str, Any] | None:
        path = path_from(value, self.repository_root)
        key = self._key(path)
        if key not in self.astc_headers:
            self.astc_headers[key] = astc_header(path) if path.is_file() else None
        return self.astc_headers[key]


def import_helper(repository_root: Path):
    helper_path = repository_root / "Tools" / "Audit-NtsdEightRoleMemory.py"
    spec = importlib.util.spec_from_file_location("ntsd_eight_role_memory_full_20261014", helper_path)
    if spec is None or spec.loader is None:
        raise AuditFailure(f"无法加载只读 helper: {helper_path}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def install_helper_identity_cache(helper: Any, cache: IdentityCache) -> None:
    def cached_file_identity(path_value: Any, expected_sha: str | None = None) -> dict[str, Any]:
        identity = cache.identity(path_value, expected_sha)
        return {
            "path": str(path_value) if path_value else "",
            "exists": identity["exists"],
            "bytes": identity["bytes"],
            "sha256": identity["sha256"],
            "expectedSha256": expected_sha,
            "shaMatch": identity["shaMatch"],
        }

    def cached_png_dimensions(path_value: Any) -> tuple[int, int] | None:
        header = cache.png(path_value)
        return (int(header["width"]), int(header["height"])) if header else None

    helper.file_identity = cached_file_identity
    helper.png_dimensions = cached_png_dimensions


def add_unique_error(errors: list[str], value: str) -> None:
    if value not in errors:
        errors.append(value)


def verify_expected_identity(
    cache: IdentityCache,
    expected: dict[str, Any] | None,
    errors: list[str],
    label: str,
    require_sha: bool = True,
) -> dict[str, Any] | None:
    if not isinstance(expected, dict):
        add_unique_error(errors, f"{label}: identity record missing")
        return None
    expected_path = expected.get("path")
    if not expected_path:
        add_unique_error(errors, f"{label}: identity path missing")
        return None
    current = cache.identity(expected_path, expected.get("sha256"))
    expected_exists = bool(expected.get("exists", bool(expected.get("sha256"))))
    if expected_exists and not current["exists"]:
        add_unique_error(errors, f"{label}: captured file disappeared: {expected_path}")
    elif not expected_exists and current["exists"]:
        add_unique_error(errors, f"{label}: file appeared after capture: {expected_path}")
    if require_sha and expected_exists and current["sha256"] and expected.get("sha256") and current["sha256"].lower() != str(expected["sha256"]).lower():
        add_unique_error(errors, f"{label}: SHA-256 drift: {expected_path}")
    if expected_exists and expected.get("bytes") is not None and current["bytes"] != int(expected["bytes"]):
        add_unique_error(errors, f"{label}: byte-size drift: {expected_path}")
    return {"label": label, "captured": expected, "current": current, "shaMatch": current.get("shaMatch")}


def walk_identity_records(value: Any, key_names: set[str] | None = None, path: str = "$", seen: set[int] | None = None) -> Iterable[tuple[str, dict[str, Any]]]:
    if seen is None:
        seen = set()
    if isinstance(value, dict):
        marker = id(value)
        if marker in seen:
            return
        seen.add(marker)
        for key, item in value.items():
            if isinstance(item, dict) and (key_names is None or key in key_names):
                yield f"{path}.{key}", item
            yield from walk_identity_records(item, key_names, f"{path}.{key}", seen)
    elif isinstance(value, list):
        for index, item in enumerate(value):
            yield from walk_identity_records(item, key_names, f"{path}[{index}]", seen)


def verify_capture_inputs(capture: dict[str, Any], cache: IdentityCache) -> dict[str, Any]:
    errors: list[str] = []
    checked: list[dict[str, Any]] = []
    seen: set[tuple[str, str]] = set()

    def check(label: str, identity: dict[str, Any] | None, require_sha: bool = True) -> None:
        if not isinstance(identity, dict) or not identity.get("path"):
            return
        marker = (normalize_path(identity.get("path")).lower(), str(identity.get("sha256") or "").lower())
        if marker in seen:
            return
        seen.add(marker)
        result = verify_expected_identity(cache, identity, errors, label, require_sha=require_sha)
        if result:
            checked.append(result)

    check("catalogIdentity", capture.get("catalogIdentity"))
    for index, item in enumerate(capture.get("objects", [])):
        check(f"objects[{index}].datIdentity", item.get("datIdentity"))
    for label, identity in walk_identity_records(capture.get("commonInputs") or {}, {"identity", "imageIdentity"}):
        check(f"commonInputs{label[1:]}", identity)
    for label, identity in walk_identity_records(capture.get("productionSourceInputs") or {}, {"identity", "imageIdentity", "datIdentity"}):
        check(f"productionSourceInputs{label[1:]}", identity)
    image_count = 0
    for label, identity in walk_identity_records(capture.get("objects", []), {"imageIdentity"}):
        image_count += 1
        check(label, identity)

    dat_checks = [item for item in checked if ".datIdentity" in item["label"] or item["label"] == "catalogIdentity"]
    object_dat_checks = [item for item in dat_checks if item["label"].startswith("objects[")]
    catalog_checks = [item for item in dat_checks if item["label"] == "catalogIdentity"]
    dat_sha_mismatches = [item for item in dat_checks if item["captured"].get("sha256") and item["current"].get("sha256") != item["captured"].get("sha256")]
    return {
        "status": "PASS" if not errors else "FAIL",
        "errors": errors,
        "checkedIdentityCount": len(checked),
        "checkedCaptureImageIdentityCount": image_count,
        "checkedDatIdentityCount": len(dat_checks),
        "checkedObjectDatIdentityCount": len(object_dat_checks),
        "checkedCatalogIdentityCount": len(catalog_checks),
        "datIdentityCountInterpretation": "checkedDatIdentityCount includes the catalog/index identity; checkedObjectDatIdentityCount is the per-capture-object DAT identity count",
        "datShaMismatchCount": len(dat_sha_mismatches),
        "checks": checked,
        "contract": "live DAT/image identity must match the capture byte-for-byte; no size/path approximation",
    }


def projection_sha(projection: dict[str, Any]) -> str:
    return sha256_json({
        "roots": projection.get("rootOids", []),
        "oids": projection.get("oids", []),
        "edges": projection.get("edges", []),
        "unknowns": projection.get("unknowns", []),
    })


def astc_entry_for_asset(helper: Any, astc: dict[str, Any], asset: dict[str, Any]) -> dict[str, Any] | None:
    source_key = normalize_path(asset.get("astcSourceKey") or asset.get("sourcePath")).lower()
    entry = (astc.get("entries") or {}).get(source_key)
    if not entry or not entry.get("usable"):
        return None
    source_sha = str(asset.get("sha256") or asset.get("expectedSha256") or "").lower()
    manifest_sha = str(entry.get("sourceSha256") or "").lower()
    if source_sha and manifest_sha and source_sha != manifest_sha:
        return None
    header = entry.get("identity") or {}
    if not header.get("exists") or header.get("shaMatch") is False:
        return None
    return entry


def load_common_records(repository_root: Path, common_path: Path, cache: IdentityCache) -> dict[str, Any]:
    if not common_path.is_file():
        raise AuditFailure(f"common static inventory missing: {common_path}")
    raw = read_json(common_path)
    records: list[dict[str, Any]] = []
    errors: list[str] = []
    for item in raw.get("assets", []):
        relative_path = str(item.get("path", ""))
        identity = cache.identity(repository_root / relative_path, item.get("sha256"))
        header = cache.png(repository_root / relative_path)
        record = {
            "category": "common",
            "path": str(repository_root / relative_path),
            "relativePath": normalize_path(relative_path),
            "sha256": identity.get("sha256"),
            "expectedSha256": item.get("sha256"),
            "bytes": identity.get("bytes", 0),
            "exists": identity.get("exists", False),
            "shaMatch": identity.get("shaMatch"),
            "header": header,
            "width": header.get("width") if header else item.get("width"),
            "height": header.get("height") if header else item.get("height"),
            "rgbaBytes": (header.get("width") * header.get("height") * RGBA_BYTES_PER_PIXEL) if header else item.get("rgba32UpperBytes"),
            "readableTexture": "ShadowPrefab" in str(item.get("provenance", "")),
            "retainPixels": False,
            "provenance": item.get("provenance"),
            "guid": item.get("guid"),
            "alternate": "TrainingGroundMap" in str(item.get("provenance", "")),
        }
        records.append(record)
        if not identity.get("exists"):
            errors.append(f"common asset missing: {relative_path}")
        elif identity.get("shaMatch") is False:
            errors.append(f"common asset SHA-256 drift: {relative_path}")
        if header and (int(header["width"]) != int(item.get("width", header["width"])) or int(header["height"]) != int(item.get("height", header["height"]))):
            errors.append(f"common asset dimensions drift: {relative_path}")

    spark = raw.get("SPARK") or {}
    spark_path = str(spark.get("path", ""))
    spark_identity = cache.identity(repository_root / spark_path)
    spark_header = cache.png(repository_root / spark_path)
    spark_record = {
        "category": "SPARK",
        "path": str(repository_root / spark_path),
        "relativePath": normalize_path(spark_path),
        "sha256": spark_identity.get("sha256"),
        "expectedSha256": None,
        "bytes": spark_identity.get("bytes", 0),
        "exists": spark_identity.get("exists", False),
        "shaMatch": None if spark_identity.get("exists") else False,
        "header": spark_header,
        "width": spark_header.get("width") if spark_header else None,
        "height": spark_header.get("height") if spark_header else None,
        "rgbaBytes": (spark_header.get("width") * spark_header.get("height") * RGBA_BYTES_PER_PIXEL) if spark_header else int(spark.get("rgbaBytes", 0)),
        "readableTexture": False,
        "retainPixels": False,
        "provenance": "resource.dat index 43 / explicit common inventory",
        "guid": None,
        "alternate": False,
    }
    records.append(spark_record)
    if not spark_identity.get("exists"):
        errors.append(f"SPARK missing: {spark_path}")

    current_records = [record for record in records if not record.get("alternate")]
    current_resident = sum(int(record.get("rgbaBytes") or 0) + (int(record.get("rgbaBytes") or 0) if record.get("readableTexture") else 0) for record in current_records)
    expected_current = int(raw.get("currentCatalogMapCommonResidentUpperBytes", 0))
    if expected_current and current_resident != expected_current:
        errors.append(f"current common resident arithmetic drift: observed={current_resident} expected={expected_current}")

    return {
        "path": str(common_path),
        "sha256": sha256_file(common_path),
        "raw": raw,
        "records": records,
        "currentRecords": current_records,
        "errors": errors,
        "status": "PASS" if not errors else "FAIL",
        "currentResidentBytes": current_resident,
        "expectedCurrentResidentBytes": expected_current,
        "alternateRecords": [record for record in records if record.get("alternate")],
        "unknown": raw.get("unknown", []),
        "missingWitnesses": raw.get("killIcons", []) + ([{"path": "resource.dat words[16..21]", "reason": "optionalUnavailable"}] if int(raw.get("WORDS", {}).get("present", 0)) == 0 else []),
    }


def inspect_stage_configuration(repository_root: Path, cache: IdentityCache) -> dict[str, Any]:
    """Inspect actual serialized config before making any stage claim."""
    config_path = repository_root / "Assets/NTSD/Config/GameConfig/GameConfig.asset"
    config_identity = cache.identity(config_path)
    text = config_path.read_text(encoding="utf-8", errors="replace") if config_path.is_file() else ""
    stage_field_matches = re.findall(r"^\s*([A-Za-z0-9_]*[Ss]tage[A-Za-z0-9_]*)\s*:\s*(.*)$", text, re.MULTILINE)
    explicit_paths: list[dict[str, Any]] = []
    for field, value in stage_field_matches:
        if "path" in field.lower() or "file" in field.lower() or "campaign" in field.lower():
            raw_value = value.strip()
            candidate = path_from(raw_value, repository_root) if raw_value else None
            explicit_paths.append({
                "field": field,
                "rawValue": raw_value,
                "resolvedPath": str(candidate) if candidate else None,
                "exists": bool(candidate and candidate.is_file()),
                "identity": cache.identity(candidate) if candidate else None,
            })
    candidates = [
        repository_root / "Assets/StreamingAssets/NTSD/data/stage.dat",
        repository_root / "Assets/NTSD/Content/LoganRuntime/decoded_dat/data/stage.dat",
        repository_root / "StreamingAssets/NTSD/data/stage.dat",
    ]
    candidate_records = [{"path": str(path), "exists": path.is_file(), "identity": cache.identity(path) if path.is_file() else None} for path in candidates]
    return {
        "gameConfigPath": str(config_path),
        "gameConfigIdentity": config_identity,
        "serializedStageFields": explicit_paths,
        "knownStageCandidates": candidate_records,
        "status": "SUPPORTING_SERIALIZED_CONFIG_INSPECTION",
        "interpretation": "This is supporting file/config evidence only. It is not stage provenance proof; the selector result's MatchConfig/driver/loader record is authoritative when present.",
        "claimBoundary": "GameConfig field absence or candidate-file absence is never used alone to prove the default Match stage.",
    }


def catalog_counts(capture: dict[str, Any], pool: list[dict[str, Any]]) -> dict[str, Any]:
    objects = capture.get("objects", [])
    parse_success = [item for item in objects if item.get("parseSuccess")]
    return {
        "captureObjectCount": len(objects),
        "captureParseSuccessCount": len(parse_success),
        "expectedObjectCount": EXPECTED_OBJECTS,
        "selectableCount": len(pool),
        "expectedSelectableCount": EXPECTED_SELECTABLE,
        "typeZeroHiddenZeroContract": "type==0 and bmp.hidden==0, oid!=-1, parseSuccess, deduplicated registry order",
        "selectableOids": [int(item["oid"]) for item in pool],
    }


def ensure_inventory_counts(capture: dict[str, Any], pool: list[dict[str, Any]], pages: dict[str, Any], astc: dict[str, Any], common: dict[str, Any]) -> None:
    observed = catalog_counts(capture, pool)
    failures: list[str] = []
    if observed["captureObjectCount"] != EXPECTED_OBJECTS or observed["captureParseSuccessCount"] != EXPECTED_OBJECTS:
        failures.append(f"capture object count {observed['captureObjectCount']}/{observed['captureParseSuccessCount']} != {EXPECTED_OBJECTS}")
    if len(pool) != EXPECTED_SELECTABLE:
        failures.append(f"selectable pool count {len(pool)} != {EXPECTED_SELECTABLE}")
    if len(pages.get("pages", {})) != EXPECTED_PAGES or len(pages.get("sources", [])) != EXPECTED_PAGE_SOURCES:
        failures.append(f"page inventory {len(pages.get('pages', {}))}/{len(pages.get('sources', []))} != {EXPECTED_PAGES}/{EXPECTED_PAGE_SOURCES}")
    if int(astc.get("entryCount", -1)) != EXPECTED_ASTC_MAPPINGS or int(astc.get("uniqueBlockCount", -1)) != EXPECTED_ASTC_BLOCKS:
        failures.append(f"ASTC inventory {astc.get('entryCount')}/{astc.get('uniqueBlockCount')} != {EXPECTED_ASTC_MAPPINGS}/{EXPECTED_ASTC_BLOCKS}")
    if common.get("status") != "PASS":
        failures.extend(common.get("errors", []))
    if failures:
        raise AuditFailure("inventory contract failed: " + "; ".join(failures))


def body_cache_key(oids: Iterable[int]) -> tuple[int, ...]:
    return tuple(sorted({int(oid) for oid in oids}))


def build_projection(
    helper: Any,
    objects: dict[int, dict[str, Any]],
    capture: dict[str, Any],
    pages: dict[str, Any],
    astc: dict[str, Any],
    name: str,
    roots: list[int],
    include_scenario: bool,
    include_compatibility: bool,
    context_victim_pool: list[int] | None,
    body_cache: dict[tuple[int, ...], tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]], list[str]]],
    ui_cache: dict[tuple[int, ...], tuple[list[dict[str, Any]], list[str]]],
) -> dict[str, Any]:
    roots = [int(oid) for oid in roots]
    unknown_roots = [oid for oid in roots if oid not in objects]
    if unknown_roots:
        raise AuditFailure(f"{name}: roots do not exist in capture: {unknown_roots}")
    closure = helper.DependencyClosure(objects, capture)
    traversal = closure.run(
        roots,
        branch="compatibility" if include_compatibility else "production",
        include_scenario=include_scenario,
        include_compatibility=include_compatibility,
        context_victim_pool=context_victim_pool,
        include_optional_209=include_scenario and 209 in objects,
    )
    oids = [int(oid) for oid in traversal["oids"]]
    key = body_cache_key(oids)
    if key not in body_cache:
        body_cache[key] = helper.body_assets_for_declarations(oids, objects, pages, astc)
    body_assets, declaration_projection, clamp_cells, body_errors = body_cache[key]
    if key not in ui_cache:
        ui_cache[key] = helper.ui_instances_for_oids(oids, objects)
    ui_instances, ui_errors = ui_cache[key]
    grid_validation = helper.grid_validation_for_declarations(name, oids, objects, astc, body_assets, clamp_cells)
    unique_dat: dict[str, dict[str, Any]] = {}
    for oid in oids:
        item = objects[oid]
        identity = item.get("datIdentity") or {}
        dat_key = f"{normalize_path(item.get('datPath') or item.get('sourcePath')).lower()}|{str(identity.get('sha256') or '').lower()}"
        unique_dat[dat_key] = {
            "oid": oid,
            "datPath": item.get("sourcePath"),
            "path": item.get("datPath"),
            "bytes": int(identity.get("bytes", 0)),
            "sha256": identity.get("sha256"),
        }
    if body_errors or ui_errors:
        raise AuditFailure(f"{name}: required image inputs failed: {body_errors + ui_errors}")
    canonical_declarations = [
        (oid, normalize_path(file["path"]).lower(), file["effectiveFirst"], file["effectiveLast"],
         file["width"], file["height"], file["row"], file["col"])
        for oid in oids for file in objects[oid]["bmp"]["files"]
    ]
    unknowns = list(traversal.get("unknowns", []))
    unknowns.extend({"kind": "body_image", "reason": error} for error in body_errors)
    unknowns.extend({"kind": "ui_image", "reason": error} for error in ui_errors)
    missing_ids = sorted({int(item["targetOid"]) for item in unknowns if str(item.get("kind", "")).lower() in {"missing_oid", "missing_root"} and item.get("targetOid") is not None})
    projection = {
        "name": name,
        "phase": "Match" if include_scenario or include_compatibility else "RoleOnly",
        "rootOids": roots,
        "rootNames": [objects[oid].get("bmp", {}).get("name") or objects[oid].get("catalogName") or f"OID{oid}" for oid in roots],
        "definitionIds": oids,
        "oids": oids,
        "includeScenario": include_scenario,
        "includeCompatibility": include_compatibility,
        "contextVictimPool": list(context_victim_pool or []),
        "closure": {
            "processedCount": traversal.get("processedCount", 0),
            "cycleRevisitCount": traversal.get("cycleRevisitCount", 0),
            "definitionCount": len(oids),
            "edgeCount": len(traversal.get("edges", [])),
            "missingIdCount": len(missing_ids),
        },
        "canonicalBodyDeclarations": canonical_declarations,
        "datList": list(unique_dat.values()),
        "datCount": len(unique_dat),
        "rawDatBytesUnique": sum(int(value.get("bytes", 0)) for value in unique_dat.values()),
        "parsedObjectMemory": {
            "status": "unknown_by_contract",
            "reason": "managed parser object/string/array allocations require runtime measurement; static frame/subblock counts are retained only",
            "frameCount": sum(len(objects[oid].get("frames", [])) for oid in oids),
            "opointCount": sum(len(frame.get("converted", {}).get("opoints", [])) for oid in oids for frame in objects[oid].get("frames", [])),
            "cpointCount": sum(len(frame.get("converted", {}).get("cpoints", [])) for oid in oids for frame in objects[oid].get("frames", [])),
        },
        "images": {
            "bodyAssets": body_assets,
            "bodyDeclarations": declaration_projection,
            "uiInstances": ui_instances,
            "uiUniqueAssets": list({instance["asset"]["key"]: instance["asset"] for instance in ui_instances}.values()),
            "clampCells": clamp_cells,
            "bodyUniqueCount": len(body_assets),
            "bodyDeclarationCount": len(declaration_projection),
            "uiInstanceCount": len(ui_instances),
            "uiUniqueCount": len({instance["asset"]["key"] for instance in ui_instances}),
        },
        "gridValidation": grid_validation,
        "edges": traversal.get("edges", []),
        "unknowns": unknowns,
        "missingIds": missing_ids,
        "missingWitnesses": [item for item in unknowns if item.get("targetOid") is not None or item.get("edge")],
        "branchProfile": "compatibility" if include_compatibility else "production",
        "scenarioContract": "respawn OID998 + loaded [100,199] normal/mode2 drops; stage/map remains explicit input" if include_scenario else None,
    }
    projection["fingerprint"] = projection_sha(projection)
    return projection


def asset_key_for_record(asset: dict[str, Any]) -> str:
    return f"{normalize_path(asset.get('path') or asset.get('sourcePath')).lower()}|{str(asset.get('sha256') or asset.get('expectedSha256') or '').lower()}"


def budget_from_image_plan(images: list[dict[str, Any]], clamp_cells: list[dict[str, Any]], atlas_bytes: int = 0, old_publication_bytes: int = 0) -> dict[str, Any]:
    source_gpu = 0
    readable_cpu = 0
    retained = 0
    max_upload = 0
    max_clamp_work = 0
    works: list[int] = []
    mode_counts: Counter[str] = Counter()
    for image in images:
        width = int(image.get("width", 0))
        height = int(image.get("height", 0))
        file_bytes = int(image.get("fileBytes", 0))
        if width <= 0 or height <= 0:
            raise AuditFailure(f"budget image has invalid dimensions: {image}")
        rgba = width * height * RGBA_BYTES_PER_PIXEL
        if image.get("mode") == "astc":
            payload = math.ceil(width / 4) * math.ceil(height / 4) * ASTC_BLOCK_BYTES
            if file_bytes != payload + ASTC_HEADER_BYTES:
                raise AuditFailure(f"ASTC budget image length mismatch: {image.get('path')}")
            allocation = payload
            work = 2 * file_bytes + 65536
        else:
            allocation = rgba
            work = rgba * 2 + 4 * file_bytes + 8 * width + 65536
        source_gpu += allocation
        readable_cpu += rgba if image.get("readableTexture") else 0
        retained += rgba if image.get("retainPixels") else 0
        max_upload = max(max_upload, allocation)
        works.append(work)
        mode_counts[str(image.get("mode"))] += 1
    for cell in clamp_cells:
        width = int(cell.get("width", 0))
        height = int(cell.get("height", 0))
        rgba = width * height * RGBA_BYTES_PER_PIXEL
        source_gpu += rgba
        retained += rgba if cell.get("retainPixels") else 0
        max_upload = max(max_upload, rgba)
        max_clamp_work = max(max_clamp_work, 2 * rgba + 8)
    resident = old_publication_bytes + source_gpu + readable_cpu + int(atlas_bytes)
    loading = resident + retained + max_upload + 4 * max_clamp_work
    if atlas_bytes:
        loading += 3 * int(atlas_bytes) + PAGE_SIZE * PAGE_SIZE * RGBA_BYTES_PER_PIXEL
    largest4 = sorted(works, reverse=True)[:4]
    loading += sum(largest4)
    return {
        "formula": {
            "pngDecodeWork": "pixels*2 + 4*PNGfileBytes + 8*width + 65536",
            "compressedReadWork": "2*ASTCfileBytes + 65536",
            "loading": "resident + retainedRGBA + maxUpload + 4*maxClampWork + (atlas?3*atlas+PageBytes:0) + largest4work",
            "resident": "oldPublication + sourceGPU + readableCPU + atlasReservation",
        },
        "sourceGpuBytes": source_gpu,
        "readableCpuBytes": readable_cpu,
        "retainedRgbaBytes": retained,
        "oldPublicationBytes": int(old_publication_bytes),
        "atlasReservationBytes": int(atlas_bytes),
        "residentBytes": resident,
        "loadingBytes": loading,
        "maxUploadBytes": max_upload,
        "maxClampWorkBytes": max_clamp_work,
        "largest4WorkBytes": largest4,
        "imageCount": len(images),
        "clampCellCount": len(clamp_cells),
        "modeCounts": dict(mode_counts),
        "scope": "engineering image allocation estimate; excludes Unity/driver/scene/object metadata and is not whole-process peak",
    }


def production_image_plan(
    helper: Any,
    assets: list[dict[str, Any]],
    clamp_cells: list[dict[str, Any]],
    astc: dict[str, Any],
    ui_assets: list[dict[str, Any]],
    spark_record: dict[str, Any] | None,
    mode: str,
    include_ui: bool,
    include_spark: bool,
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    """Build the exact image plan used by MatchImageResidency.make_case.

    This deliberately does not call the obsolete EightRoleMemory budget helper.
    Production SourceTexture2D has atlasBytes=0 and retainPixels=false.  ASTC
    uses every usable source/header-matched entry; no large-only filtering is
    applied.  Missing/unusable sidecars are explicit PNG/RGBA fallback images.
    """
    if mode not in {"astc4x4_all", "rgba_fallback"}:
        raise AuditFailure(f"corrected image plan does not support mixed ASTC mode: {mode}")
    images: list[dict[str, Any]] = []
    seen: set[str] = set()
    for asset in assets:
        entry = astc_entry_for_asset(helper, astc, asset) if mode == "astc4x4_all" else None
        if entry:
            block_key = asset["key"] if asset.get("kind") == "derived_page" else normalize_path(entry.get("filesystemPath")).lower()
            if block_key in seen:
                continue
            seen.add(block_key)
            identity = entry.get("identity") or {}
            images.append({
                "path": entry.get("filesystemPath"),
                "mode": "astc",
                "width": int(asset.get("width") or entry.get("width", 0)),
                "height": int(asset.get("height") or entry.get("height", 0)),
                "fileBytes": int(entry.get("fileBytes", identity.get("bytes", 0))),
                "retainPixels": False,
                "readableTexture": False,
                "source": asset.get("path"),
                "sourceSha256": asset.get("sha256"),
                "astcSha256": identity.get("sha256"),
                "ownerKind": asset.get("kind"),
            })
            continue
        key = asset_key_for_record(asset)
        if key in seen:
            continue
        seen.add(key)
        images.append({
            "path": asset.get("path"),
            "mode": "png",
            "width": int(asset.get("width") or 0),
            "height": int(asset.get("height") or 0),
            "fileBytes": int(asset.get("bytes", 0)),
            "retainPixels": mode == "rgba_fallback" and asset.get("kind") == "body_source",
            "readableTexture": False,
            "source": asset.get("sourcePath"),
            "sourceSha256": asset.get("sha256"),
            "ownerKind": asset.get("kind"),
        })

    if include_ui:
        for asset in ui_assets:
            key = asset_key_for_record(asset)
            if key in seen:
                continue
            seen.add(key)
            images.append({
                "path": asset.get("path"),
                "mode": "png",
                "width": int(asset.get("width") or 0),
                "height": int(asset.get("height") or 0),
                "fileBytes": int(asset.get("bytes", 0)),
                "retainPixels": False,
                "readableTexture": True,
                "source": asset.get("sourcePath"),
                "sourceSha256": asset.get("sha256"),
                "ownerKind": asset.get("kind"),
            })

    if include_spark:
        if not spark_record or not spark_record.get("exists"):
            raise AuditFailure("SPARK is required for a battle image plan but was not verified")
        images.append({
            "path": spark_record.get("path"),
            "mode": "png",
            "width": int(spark_record.get("width") or 0),
            "height": int(spark_record.get("height") or 0),
            "fileBytes": int(spark_record.get("bytes", 0)),
            "retainPixels": mode == "rgba_fallback",
            "readableTexture": False,
            "source": spark_record.get("relativePath"),
            "sourceSha256": spark_record.get("sha256"),
            "ownerKind": "SPARK",
        })

    clamped_cells = [
        {
            "width": int(cell.get("width", 0)),
            "height": int(cell.get("height", 0)),
            "retainPixels": mode == "rgba_fallback",
            "oid": cell.get("oid"),
            "pic": cell.get("pic"),
            "imagePath": cell.get("imagePath"),
            "sourceSha256": cell.get("sourceSha256"),
        }
        for cell in clamp_cells
    ]
    return images, clamped_cells


def production_case_budget(
    helper: Any,
    projection: dict[str, Any],
    astc: dict[str, Any],
    common: dict[str, Any],
    *,
    mode: str,
    include_ui: bool,
    include_spark: bool,
    old_publication_bytes: int,
    add_outside_common: bool,
) -> dict[str, Any]:
    ui_assets = projection.get("allUiUniqueAssets") or projection["images"].get("uiUniqueAssets", [])
    spark_record = next((record for record in common.get("currentRecords", []) if record.get("category") == "SPARK"), None)
    images, cells = production_image_plan(
        helper,
        projection["images"].get("bodyAssets", []),
        projection["images"].get("clampCells", []),
        astc,
        ui_assets if include_ui else [],
        spark_record,
        mode,
        include_ui,
        include_spark,
    )
    # This is the same make_case arithmetic: no atlas reservation. RGBA fallback retains raw-body/SPARK/clamp pixels
    # until publication; the ASTC operation does not retain them.
    base = budget_from_image_plan(images, cells, atlas_bytes=0, old_publication_bytes=old_publication_bytes)
    outside = 0
    if add_outside_common:
        outside = int(common.get("currentResidentBytes", 0)) - int((spark_record or {}).get("rgbaBytes", 0))
        expected_outside = 9_586_712
        if outside != expected_outside:
            raise AuditFailure(f"outside current-map/foot/shadow bytes drift: {outside} != {expected_outside}")
    result = dict(base)
    result.update({
        "mode": mode,
        "includeUi": include_ui,
        "includeSpark": include_spark,
        "sourceTexture2DContract": "atlasBytes=0; ASTC no retained pixels; RGBA raw body/SPARK/clamp retainPixels=true; derived pages false; readable UI only",
        "outsideCurrentMapFootShadowBytes": outside,
        "withCurrentMapCommonResidentBytes": base["residentBytes"] + outside,
        "withCurrentMapCommonLoadingBytes": base["loadingBytes"] + outside,
        "images": images,
        "clampedCells": cells,
        "bodyAllocationCount": len(images) - len(ui_assets if include_ui else []) - int(include_spark),
        "uiUniqueCount": len(ui_assets) if include_ui else 0,
        "capabilityScope": "ASTC4x4 all usable manifest entries; no large-only or <=2048 mixed fallback variant",
    })
    return result


def oversize_refusal(projection: dict[str, Any]) -> dict[str, Any]:
    resources = []
    for asset in projection["images"].get("bodyAssets", []):
        width = int(asset.get("width") or 0)
        height = int(asset.get("height") or 0)
        if max(width, height) > 2048:
            resources.append({
                "path": asset.get("path"),
                "sourcePath": asset.get("sourcePath"),
                "kind": asset.get("kind"),
                "width": width,
                "height": height,
                "maxDimension": max(width, height),
                "oid": asset.get("oid"),
            })
    return {
        "status": "REFUSED_OVERSIZE" if resources else "ADMISSIBLE",
        "threshold": "width<=2048 and height<=2048",
        "oversizedCount": len(resources),
        "resources": resources,
        "rule": "oversized ASTC entries are refused; they are never filtered into a silent PNG fallback budget",
    }


def attach_production_budgets(
    helper: Any,
    projection: dict[str, Any],
    all_ui_instances: list[dict[str, Any]],
    common: dict[str, Any],
    astc: dict[str, Any],
    pages: dict[str, Any],
) -> None:
    """Attach the MatchImageResidency production budgets to a projection."""
    projection["allUiUniqueAssets"] = list({item["asset"]["key"]: item["asset"] for item in all_ui_instances}.values())
    projection["allUiInstanceCount"] = len(all_ui_instances)
    is_menu = not projection["images"].get("bodyAssets") and projection.get("name") == "menu"
    menu_old = 27_673_952
    if is_menu:
        astc_from_menu = production_case_budget(helper, projection, astc, common, mode="astc4x4_all", include_ui=True, include_spark=False, old_publication_bytes=0, add_outside_common=False)
        astc_cold = astc_from_menu
        rgba_from_menu = production_case_budget(helper, projection, astc, common, mode="rgba_fallback", include_ui=True, include_spark=False, old_publication_bytes=0, add_outside_common=False)
        rgba_cold = rgba_from_menu
    else:
        astc_from_menu = production_case_budget(helper, projection, astc, common, mode="astc4x4_all", include_ui=False, include_spark=True, old_publication_bytes=menu_old, add_outside_common=True)
        astc_cold = production_case_budget(helper, projection, astc, common, mode="astc4x4_all", include_ui=True, include_spark=True, old_publication_bytes=0, add_outside_common=True)
        rgba_from_menu = production_case_budget(helper, projection, astc, common, mode="rgba_fallback", include_ui=False, include_spark=True, old_publication_bytes=menu_old, add_outside_common=True)
        rgba_cold = production_case_budget(helper, projection, astc, common, mode="rgba_fallback", include_ui=True, include_spark=True, old_publication_bytes=0, add_outside_common=True)
    refusal = oversize_refusal(projection)
    projection["budgets"] = {
        "astc4x4_from_menu": astc_from_menu,
        "astc4x4_cold": astc_cold,
        "rgbaFallback_from_menu": rgba_from_menu,
        "rgbaFallback_cold": rgba_cold,
        "astc4x4_2048_capability": refusal,
    }
    projection["budgetSummary"] = {
        "astcFromMenuResidentBytes": astc_from_menu["residentBytes"],
        "astcFromMenuLoadingBytes": astc_from_menu["loadingBytes"],
        "astcFromMenuWithCurrentMapCommonResidentBytes": astc_from_menu["withCurrentMapCommonResidentBytes"],
        "astcFromMenuWithCurrentMapCommonLoadingBytes": astc_from_menu["withCurrentMapCommonLoadingBytes"],
        "astcColdResidentBytes": astc_cold["residentBytes"],
        "astcColdLoadingBytes": astc_cold["loadingBytes"],
        "astcColdWithCurrentMapCommonResidentBytes": astc_cold["withCurrentMapCommonResidentBytes"],
        "astcColdWithCurrentMapCommonLoadingBytes": astc_cold["withCurrentMapCommonLoadingBytes"],
        "rgbaFallbackFromMenuResidentBytes": rgba_from_menu["residentBytes"],
        "rgbaFallbackFromMenuLoadingBytes": rgba_from_menu["loadingBytes"],
        "rgbaFallbackFromMenuWithCurrentMapCommonResidentBytes": rgba_from_menu["withCurrentMapCommonResidentBytes"],
        "rgbaFallbackFromMenuWithCurrentMapCommonLoadingBytes": rgba_from_menu["withCurrentMapCommonLoadingBytes"],
        "rgbaFallbackColdResidentBytes": rgba_cold["residentBytes"],
        "rgbaFallbackColdLoadingBytes": rgba_cold["loadingBytes"],
        "rgbaFallbackColdWithCurrentMapCommonResidentBytes": rgba_cold["withCurrentMapCommonResidentBytes"],
        "rgbaFallbackColdWithCurrentMapCommonLoadingBytes": rgba_cold["withCurrentMapCommonLoadingBytes"],
        "astc2048Capability": refusal,
        "commonCurrentCatalogResidentBytes": common["currentResidentBytes"],
        "commonOutsideCurrentMapFootShadowBytes": astc_from_menu["outsideCurrentMapFootShadowBytes"],
    }


def edge_key(edge: dict[str, Any]) -> str:
    from_oids = ",".join(str(value) for value in edge.get("fromOids", []))
    return "|".join([
        str(edge.get("type", "")),
        from_oids,
        str(edge.get("fromOid", "")),
        str(edge.get("toOid", edge.get("resolvedTarget", ""))),
        str(edge.get("frameId", "")),
        str(edge.get("sourceFile", "")),
        str(edge.get("sourceLines", "")),
        json.dumps(edge.get("rawToken"), ensure_ascii=False, sort_keys=True, separators=(",", ":")),
    ])


def combination_analysis(
    helper: Any,
    objects: dict[int, dict[str, Any]],
    capture: dict[str, Any],
    roots: list[int],
) -> dict[str, Any]:
    """Prove whether combined context adds closure/edges beyond single unions."""
    singles: list[dict[str, Any]] = []
    union_oids: set[int] = set()
    union_edges: set[str] = set()
    union_fusion_edges: set[str] = set()
    for oid in roots:
        closure = helper.DependencyClosure(objects, capture).run(
            [oid],
            branch="compatibility",
            include_scenario=True,
            include_compatibility=True,
            context_victim_pool=[oid],
            include_optional_209=209 in objects,
        )
        single_edges = {edge_key(edge) for edge in closure.get("edges", [])}
        union_oids.update(int(value) for value in closure.get("oids", []))
        union_edges.update(single_edges)
        union_fusion_edges.update(edge_key(edge) for edge in closure.get("edges", []) if len(edge.get("fromOids", [])) > 1 or "fusion" in str(edge.get("type", "")).lower())
        singles.append({
            "oid": oid,
            "definitionIds": closure.get("oids", []),
            "edgeCount": len(closure.get("edges", [])),
            "missing": closure.get("unknowns", []),
            "fingerprint": sha256_json({"oids": closure.get("oids", []), "edges": closure.get("edges", []), "unknowns": closure.get("unknowns", [])}),
        })
    combined = helper.DependencyClosure(objects, capture).run(
        roots,
        branch="compatibility",
        include_scenario=True,
        include_compatibility=True,
        context_victim_pool=roots,
        include_optional_209=209 in objects,
    )
    combined_oids = {int(value) for value in combined.get("oids", [])}
    combined_edges = {edge_key(edge): edge for edge in combined.get("edges", [])}
    combined_only_oids = sorted(combined_oids - union_oids)
    combined_only_edges = sorted(set(combined_edges) - union_edges)
    return {
        "roots": roots,
        "singleAnalyses": singles,
        "unionSingleDefinitionIds": sorted(union_oids),
        "combinedDefinitionIds": sorted(combined_oids),
        "combinedOnlyDefinitionIds": combined_only_oids,
        "unionSingleEdgeCount": len(union_edges),
        "combinedEdgeCount": len(combined_edges),
        "combinedOnlyEdgeCount": len(combined_only_edges),
        "combinedOnlyEdgeKeys": combined_only_edges,
        "fusionOrHyperedgeSingleUnionCount": len(union_fusion_edges),
        "fusionContextDifference": bool(combined_only_oids or combined_only_edges),
        "fixedPointContract": "combined closure is traversed independently to a fusion/context fixed point; union of single-root closures is not substituted",
    }


def safe_case_name(value: str) -> str:
    normalized = re.sub(r"[^A-Za-z0-9_.-]+", "-", str(value)).strip(".-")
    if not normalized:
        raise AuditFailure(f"invalid empty case name: {value!r}")
    return normalized[:96]


def parse_plans_input(path: Path, objects: dict[int, dict[str, Any]]) -> list[dict[str, Any]]:
    raw = read_json(path)
    if not isinstance(raw, dict) or not isinstance(raw.get("cases"), list):
        raise AuditFailure("--plans-input must contain {cases:[{name,roots,additionalRoots?:[]}]}")
    result: list[dict[str, Any]] = []
    seen_names: set[str] = set()
    for index, item in enumerate(raw["cases"]):
        if not isinstance(item, dict) or not item.get("name"):
            raise AuditFailure(f"plans-input case {index} missing name")
        name = safe_case_name(str(item["name"]))
        if name in seen_names:
            raise AuditFailure(f"duplicate plans-input case name: {name}")
        seen_names.add(name)
        roots = [int(value) for value in item.get("roots", [])]
        additional = [int(value) for value in item.get("additionalRoots", [])]
        combined: list[int] = []
        for oid in roots + additional:
            if oid not in objects:
                raise AuditFailure(f"plans-input case {name} references unknown OID {oid}")
            if oid not in combined:
                combined.append(oid)
        if not combined:
            raise AuditFailure(f"plans-input case {name} has no roots")
        result.append({"name": name, "roots": roots, "additionalRoots": additional, "combinedRoots": combined})
    return result


def build_recommendations(
    pool: list[dict[str, Any]],
    role_match: dict[int, dict[str, Any]],
    role_only: dict[int, dict[str, Any]],
    capture: dict[str, Any],
    objects: dict[int, dict[str, Any]],
) -> tuple[dict[str, Any], str]:
    pool_oids = [int(item["oid"]) for item in pool]

    def body_astc_keys(projection: dict[str, Any]) -> set[str]:
        keys: set[str] = set()
        for asset in projection["images"]["bodyAssets"]:
            if asset.get("kind") not in {"body_source", "derived_page"}:
                continue
            source = normalize_path(asset.get("astcSourceKey") or asset.get("sourcePath")).lower()
            if source:
                keys.add(source + "|" + str(asset.get("sha256") or "").lower())
        return keys

    # Explicit bounded greedy heuristic: at every step select the role with the
    # largest new ASTC-source union, then closure/edge tie-breakers. It is not a
    # global maximum search and is reported as such.
    selected: list[int] = []
    covered: set[str] = set()
    greedy_steps: list[dict[str, Any]] = []
    for _ in range(min(8, len(pool_oids))):
        candidates = []
        for oid in pool_oids:
            if oid in selected:
                continue
            keys = body_astc_keys(role_match[oid])
            gain = len(keys - covered)
            candidates.append((gain, len(role_match[oid]["edges"]), len(role_match[oid]["oids"]), -oid, oid, keys))
        if not candidates:
            break
        chosen = max(candidates)
        _, edge_gain, closure_count, _, oid, keys = chosen
        selected.append(oid)
        covered.update(keys)
        greedy_steps.append({"oid": oid, "newAstcSourceCount": len(keys - (covered - keys)), "closureDefinitionCount": closure_count, "edgeCount": edge_gain})
    greedy_roots = selected

    complexity_order = sorted(
        pool_oids,
        key=lambda oid: (len(role_match[oid]["edges"]) + len(role_match[oid]["unknowns"]) * 100, len(role_match[oid]["oids"]), -oid),
        reverse=True,
    )
    dependency_roots = complexity_order[:8]

    fusion_records = (capture.get("fusion") or {}).get("records", [])
    real_fusion_roots: list[int] = []
    fusion_candidates: list[dict[str, Any]] = []
    pool_set = set(pool_oids)
    for record in fusion_records:
        pair = [int(record.get("id1", -1)), int(record.get("id2", -1))]
        pair = [oid for oid in pair if oid in pool_set]
        if len(pair) == 2:
            fusion_candidates.append({"inputs": pair, "output": record.get("id3"), "sourceLine": record.get("sourceLine")})
            for oid in pair:
                if oid not in real_fusion_roots:
                    real_fusion_roots.append(oid)
    for oid in sorted(pool_oids, key=lambda value: (len(role_match[value]["unknowns"]), len(role_match[value]["edges"])), reverse=True):
        if len(real_fusion_roots) >= 8:
            break
        if oid not in real_fusion_roots:
            real_fusion_roots.append(oid)
    real_fusion_roots = real_fusion_roots[:8]
    previous_roots = [28, 18, 65, 23, 8, 10, 66, 27]

    cases = [
        {"name": "greedy-high-astc-union", "roots": greedy_roots},
        {"name": "dependency-complex", "roots": dependency_roots},
        {"name": "real-fusion-context", "roots": real_fusion_roots},
        {"name": "previous-8-regression", "roots": previous_roots},
        {
            "name": "desktop-boundary-high",
            "roots": [77, 18, 65, 23, 8, 10, 66, 27],
            "selectionMethod": "bounded constrained substitution: replace prior OID28 with high-cost OID77, then re-evaluate the complete combined closure under the corrected RGBA 512/768 MiB boundary",
        },
        {
            "name": "desktop-fusion-context",
            "roots": [7, 8, 18, 65, 23, 10, 66, 27],
            "selectionMethod": "bounded constrained substitution: force observed OID7+OID8 fusion and retain six prior regression roles, then re-evaluate the complete combined closure under the corrected RGBA 512/768 MiB boundary",
        },
    ]
    for case in cases:
        if len(case["roots"]) != 8 or any(oid not in pool_set for oid in case["roots"]):
            raise AuditFailure(f"recommendation is not a bounded 8-role selectable case: {case}")
    payload = {"cases": cases}
    lines = [
        "# Recommended bounded eight-role cases",
        "",
        "These are static diagnostic suggestions only. The main worker owns the final scene/budget choice.",
        "",
        "- `greedy-high-astc-union`: explicit greedy heuristic. Each step maximizes newly covered ASTC source identities, with closure edge/count tie-breakers. It is not a global maximum search.",
        "- `dependency-complex`: top eight selectable roles by match edge count plus a missing-witness weight, then closure size; this is a dependency stress heuristic.",
        "- `real-fusion-context`: starts with selectable inputs from observed fusion records, then fills to eight by dependency/missing-witness score. Observed fusion pairs: " + json.dumps(fusion_candidates, ensure_ascii=False) + ".",
        "- `previous-8-regression`: preserves the prior fixed eight roots `[28,18,65,23,8,10,66,27]` for regression comparison.",
        "- `desktop-boundary-high`: bounded substitution candidate `[77,18,65,23,8,10,66,27]`; it is an admitted candidate only after the exact combined closure and corrected RGBA boundary are computed.",
        "- `desktop-fusion-context`: bounded fusion candidate `[7,8,18,65,23,10,66,27]`; OID7+OID8 is an observed fusion pair and the combined closure is recomputed to capture context differences.",
        "",
        "Each case must be recomputed as a combined closure. The full audit records single-root union versus combined fixed-point differences; a union of single-role sets is not treated as a sufficient fusion/context model. The desktop candidates are bounded samples, not a global maximum/minimum search.",
        "",
        "Greedy trace:",
        "",
    ]
    for step, detail in enumerate(greedy_steps, start=1):
        lines.append(f"{step}. OID {detail['oid']}: new ASTC sources {detail['newAstcSourceCount']}, closure definitions {detail['closureDefinitionCount']}, edges {detail['edgeCount']}.")
    return payload, "\n".join(lines) + "\n"


def asset_matrix_row(
    cache: IdentityCache,
    scope: str,
    path_value: Any,
    source_path: Any = None,
    source_sha: Any = None,
    owner_oids: Iterable[int] = (),
    kind: str = "",
    page_id: Any = None,
    astc_path: Any = None,
    astc_identity: dict[str, Any] | None = None,
    extra: dict[str, Any] | None = None,
) -> dict[str, Any]:
    expected = (astc_identity or {}).get("sha256") if scope == "astc_mapping" else source_sha
    identity = cache.identity(path_value, expected)
    source_identity = cache.identity(extra["sourceFilePath"], source_sha) if scope == "astc_mapping" and extra else identity
    header = cache.png(path_value) if str(path_value).lower().endswith(".png") else None
    astc_header_value = cache.astc(astc_path) if astc_path else None
    row: dict[str, Any] = {
        "scope": scope,
        "kind": kind,
        "path": str(path_value or ""),
        "normalizedPath": normalize_path(path_value),
        "sourcePath": normalize_path(source_path),
        "sourceSha256": source_sha,
        "sha256": identity.get("sha256"),
        "expectedSha256": identity.get("expectedSha256"),
        "exists": identity.get("exists", False),
        "bytes": identity.get("bytes", 0),
        "shaMatch": identity.get("shaMatch"),
        "header": header,
        "width": header.get("width") if header else None,
        "height": header.get("height") if header else None,
        "rgbaBytes": (header.get("width") * header.get("height") * RGBA_BYTES_PER_PIXEL) if header else None,
        "ownerOids": sorted({int(oid) for oid in owner_oids}),
        "pageId": page_id,
        "astcPath": str(astc_path or ""),
        "astcSha256": (astc_identity or {}).get("sha256"),
        "astcBytes": (astc_identity or {}).get("bytes", 0),
        "astcHeader": astc_header_value,
        "sourceShaMatches": bool(source_identity.get("exists")) and source_identity.get("shaMatch") is not False,
    }
    if extra:
        row.update(extra)
    return row


def build_resource_matrix(
    helper: Any,
    cache: IdentityCache,
    objects: dict[int, dict[str, Any]],
    pages: dict[str, Any],
    astc: dict[str, Any],
    common: dict[str, Any],
    full_projection: dict[str, Any],
    all_ui_unique: list[dict[str, Any]],
    role_projections: dict[int, dict[str, Any]],
) -> list[dict[str, Any]]:
    rows: dict[str, dict[str, Any]] = {}
    owner_by_asset: dict[str, set[int]] = defaultdict(set)
    for oid, projection in role_projections.items():
        for asset in projection["images"]["bodyAssets"]:
            owner_by_asset[asset_key_for_record(asset)].add(oid)
    for asset in full_projection["images"]["bodyAssets"]:
        key = f"body|{asset_key_for_record(asset)}"
        row = asset_matrix_row(
            cache,
            "body",
            asset.get("path"),
            asset.get("sourcePath"),
            asset.get("sha256"),
            owner_by_asset.get(asset_key_for_record(asset), set()),
            asset.get("kind", "body"),
            astc_path=(astc.get("entries", {}).get(normalize_path(asset.get("astcSourceKey") or asset.get("sourcePath")).lower()) or {}).get("filesystemPath"),
            astc_identity=(astc.get("entries", {}).get(normalize_path(asset.get("astcSourceKey") or asset.get("sourcePath")).lower()) or {}).get("identity"),
            extra={
                "assetKey": asset.get("key"),
                "astcSourceKey": normalize_path(asset.get("astcSourceKey")),
                "declarationSourceOid": asset.get("sourceOid"),
                "astcUsable": bool(
                    (astc.get("entries", {}).get(normalize_path(asset.get("astcSourceKey") or asset.get("sourcePath")).lower()) or {}).get("usable")
                ),
            },
        )
        rows.setdefault(key, row)
    for asset in all_ui_unique:
        key = f"ui|{asset_key_for_record(asset)}"
        rows.setdefault(key, asset_matrix_row(cache, "ui", asset.get("path"), asset.get("sourcePath"), asset.get("sha256"), (), asset.get("kind", "ui"), extra={"assetKey": asset.get("key")}))
    for record in common.get("records", []):
        key = f"common|{normalize_path(record.get('path')).lower()}|{str(record.get('sha256') or '').lower()}"
        rows.setdefault(key, asset_matrix_row(cache, "common", record.get("path"), record.get("relativePath"), record.get("sha256"), (), record.get("category", "common"), extra={"assetKey": key, "provenance": record.get("provenance"), "alternate": record.get("alternate", False)}))

    # Every declared page source and physical derived page is retained as an
    # independent row so 56/35 inventory counts and page SHA/header evidence are
    # inspectable even when no selected role owns the page.
    for page_id, page in sorted((pages.get("pages") or {}).items()):
        key = f"page|{page_id}|{page.get('sha256')}"
        rows.setdefault(key, asset_matrix_row(cache, "page", page.get("filesystemPath"), page.get("path"), page.get("sha256"), (), "derived_page_manifest", page_id=page_id, extra={"assetKey": key}))
    for source in pages.get("sources", []):
        source_path = normalize_path(source.get("path"))
        source_sha = source.get("sha256")
        source_asset = next((asset for asset in full_projection["images"]["bodyAssets"] if normalize_path(asset.get("astcSourceKey") or asset.get("sourcePath")).lower() == source_path.lower() and str(asset.get("sha256") or "").lower() == str(source_sha or "").lower()), None)
        source_file = source_asset.get("path") if source_asset else str(cache.repository_root / "Assets/NTSD/Content/LoganRuntime" / ("" if source_path.startswith(("derived_", "vfs/")) else "vfs") / source_path)
        key = f"page-source|{source_path.lower()}|{str(source_sha or '').lower()}"
        rows.setdefault(key, asset_matrix_row(cache, "page_source", source_file, source_path, source_sha, (), "derived_page_source", extra={"assetKey": key, "pageBindings": source.get("bindings", []), "pageChunks": source.get("chunks", [])}))

    # Use raw manifest entries for the 723 source mappings. The normalized
    # helper map intentionally has aliases, so it cannot be used as the count.
    for index, raw_entry in enumerate((astc.get("raw") or {}).get("entries", [])):
        source_path = normalize_path(raw_entry.get("sourcePath") or raw_entry.get("source") or raw_entry.get("path"))
        entry = (astc.get("entries") or {}).get(source_path.lower())
        astc_path = entry.get("filesystemPath") if entry else raw_entry.get("filePath") or raw_entry.get("outputPath") or raw_entry.get("astcPath") or raw_entry.get("astc")
        source_asset = next((asset for asset in full_projection["images"]["bodyAssets"] if normalize_path(asset.get("astcSourceKey") or asset.get("sourcePath")).lower() == source_path.lower()), None)
        source_file = source_asset.get("path") if source_asset else str(cache.repository_root / "Assets/NTSD/Content/LoganRuntime" / ("" if source_path.startswith(("derived_", "vfs/")) else "vfs") / source_path)
        source_sha = raw_entry.get("sourceSha256") or raw_entry.get("sourceSha") or (source_asset or {}).get("sha256")
        astc_identity = (entry or {}).get("identity")
        key = f"astc-mapping|{index}|{source_path.lower()}|{str(source_sha or '').lower()}"
        rows[key] = asset_matrix_row(cache, "astc_mapping", astc_path, source_path, source_sha, (), "astc4x4_mapping", astc_path=astc_path, astc_identity=astc_identity, extra={"assetKey": key, "mappingIndex": index, "sourceFilePath": source_file, "sourceAssetKnown": source_asset is not None, "astcUsable": bool(entry and entry.get("usable"))})
    result = list(rows.values())
    invalid = [row for row in result if not row["exists"] or row["shaMatch"] is False or not row["sourceShaMatches"]]
    if invalid:
        raise AuditFailure(f"Resource matrix path/SHA validation failed: {[(r['scope'], r['path']) for r in invalid[:10]]}")
    return result


def full_resource_lists(
    role_only: dict[int, dict[str, Any]],
    role_match: dict[int, dict[str, Any]],
    common: dict[str, Any],
) -> list[dict[str, Any]]:
    common_keys = [f"common|{normalize_path(record.get('path')).lower()}|{str(record.get('sha256') or '').lower()}" for record in common.get("currentRecords", [])]
    rows: list[dict[str, Any]] = []
    for oid in sorted(role_only):
        only = role_only[oid]
        match = role_match[oid]
        only_keys = sorted({asset_key_for_record(asset) for asset in only["images"]["bodyAssets"]} | {asset_key_for_record(asset) for asset in only.get("allUiUniqueAssets", [])} | set(common_keys))
        match_keys = sorted({asset_key_for_record(asset) for asset in match["images"]["bodyAssets"]} | {asset_key_for_record(asset) for asset in match.get("allUiUniqueAssets", [])} | set(common_keys))
        rows.append({
            "oid": oid,
            "name": only["rootNames"][0] if only["rootNames"] else f"OID{oid}",
            "roleOnlyDefinitionCount": len(only["oids"]),
            "matchDefinitionCount": len(match["oids"]),
            "roleOnlyBodyAssetCount": len(only["images"]["bodyAssets"]),
            "matchBodyAssetCount": len(match["images"]["bodyAssets"]),
            "allUiUniqueCount": len(only.get("allUiUniqueAssets", [])),
            "commonAssetCount": len(common_keys),
            "roleOnlyResourceCount": len(only_keys),
            "matchResourceCount": len(match_keys),
            "roleOnlyResourceKeys": only_keys,
            "matchResourceKeys": match_keys,
            "roleOnlyBudgets": only.get("budgetSummary"),
            "matchBudgets": match.get("budgetSummary"),
        })
    return rows


def missing_witnesses(projections: Iterable[dict[str, Any]]) -> list[dict[str, Any]]:
    by_key: dict[str, dict[str, Any]] = {}
    for projection in projections:
        for witness in projection.get("missingWitnesses", []):
            target = witness.get("targetOid")
            marker = json.dumps({"targetOid": target, "edge": witness.get("edge"), "kind": witness.get("kind")}, ensure_ascii=False, sort_keys=True, separators=(",", ":"))
            by_key.setdefault(marker, {"targetOid": target, "kind": witness.get("kind"), "reason": witness.get("reason"), "edge": witness.get("edge"), "projection": projection.get("name")})
    return list(by_key.values())


def oversized_resources(matrix: list[dict[str, Any]]) -> list[dict[str, Any]]:
    result = []
    for row in matrix:
        width, height = row.get("width"), row.get("height")
        if not width or not height:
            continue
        maximum = max(int(width), int(height))
        if maximum > 2048:
            result.append({
                "scope": row.get("scope"),
                "kind": row.get("kind"),
                "path": row.get("path"),
                "width": width,
                "height": height,
                "maxDimension": maximum,
                "astcUsableManifestEntry": bool(row.get("astcUsable")),
                "astcEligibilityContract": "usable manifest entry plus device ASTC support and max texture size >= source dimensions",
                "over2048Capability": True,
                "over8192": maximum > 8192,
                "ownerOids": row.get("ownerOids", []),
                "sha256": row.get("sha256"),
            })
    return result


def comparable_edge_type(value: Any) -> str:
    edge_type = str(value or "").lower()
    mapping = {
        "dat_opoint": "opoint",
        "state18_19_particles": "state18/19",
        "native_state_transform": "native state transform",
        "weapon_piece_custom": "weapon_piece",
        "weapon_piece_builtin": "builtin weapon fragment",
        "random_weapon_normal": "scenario random weapon",
        "random_weapon_mode2": "scenario random weapon",
        "state9996_spawn": "state9996",
        "fusion_hyperedge": "fusion",
        "kind_copy": "kind context",
        "respawn_effect": "scenario respawn",
    }
    return mapping.get(edge_type, edge_type)


def structural_internal_edge(edge: dict[str, Any]) -> tuple[Any, ...]:
    """Normalize only the stable closure edge identity used by the C# export.

    The selector's rule/type/source text is an export detail and is intentionally
    excluded.  Fusion is a hyperedge: its input OIDs remain grouped.  The
    independent ``kind_copy`` record is also multi-source, but it is a
    conditional interaction record rather than a fusion.  Its ``toOid`` is the
    bound attacker and its raw token carries the copresent target/frame, so it
    is normalized to the ordinary ``(fromId,toId,frameId)`` identity emitted by
    the selector.
    """
    edge_type = str(edge.get("type", "")).lower()
    raw_token = edge.get("rawToken") if isinstance(edge.get("rawToken"), dict) else {}
    if edge_type in {"kind_copy", "conditional_context"} or (
        raw_token.get("effect") == 209 and raw_token.get("targetOid") is not None
    ):
        bound_attacker = edge.get("toOid", edge.get("resolvedTarget"))
        target = raw_token.get("targetOid")
        frame = raw_token.get("frame", edge.get("frameId"))
        if bound_attacker is not None and target is not None and frame is not None:
            return (int(bound_attacker), int(target), int(frame))
    from_values = edge.get("fromOids") or ([] if edge.get("fromOid") is None else [edge.get("fromOid")])
    from_oids = tuple(sorted(int(value) for value in from_values if value is not None))
    target = edge.get("toOid", edge.get("resolvedTarget"))
    frame = edge.get("frameId")
    if len(from_oids) > 1 or "fusion" in edge_type:
        return ("fusion", from_oids, target, frame)
    return (from_oids[0] if from_oids else None, target, frame)


def structural_selector_edge(edge: dict[str, Any]) -> tuple[Any, ...]:
    """Parse the selector export into (fromId,toId,frameId), with fusion support."""
    rule = str(edge.get("rule", ""))
    rule_lower = rule.lower()
    from_values: list[int] = []
    if rule_lower.startswith("fusion:"):
        from_values = [int(value) for value in re.findall(r"OID(-?\d+)", rule.split("->", 1)[0])]
        return ("fusion", tuple(sorted(from_values)), edge.get("toId"), edge.get("frameId"))
    source_id = edge.get("fromId")
    return (int(source_id) if source_id is not None else None, edge.get("toId"), edge.get("frameId"))


# Keep these names as compatibility aliases for callers that imported the
# earlier diagnostic module, but make their semantics structural rather than
# type/rule/source-string equality.
def comparable_internal_edge(edge: dict[str, Any]) -> tuple[Any, ...]:
    return structural_internal_edge(edge)


def comparable_selector_edge(edge: dict[str, Any]) -> tuple[Any, ...]:
    return structural_selector_edge(edge)


def normalize_selector_edges(edges: Any) -> set[str]:
    normalized: set[str] = set()
    if not isinstance(edges, list):
        return normalized
    for edge in edges:
        if not isinstance(edge, dict):
            continue
        from_values = edge.get("fromOids") or edge.get("fromIds") or edge.get("sourceIds") or []
        if not isinstance(from_values, list):
            from_values = [from_values]
        from_oid = edge.get("fromOid", edge.get("fromId", edge.get("sourceId", "")))
        to_oid = edge.get("toOid", edge.get("toId", edge.get("targetId", edge.get("resolvedTarget", ""))))
        frame = edge.get("frameId", edge.get("frame", edge.get("frameIndex", "")))
        edge_type = edge.get("type", edge.get("edgeType", edge.get("kind", "")))
        source_file = edge.get("sourceFile", edge.get("sourcePath", ""))
        normalized.add("|".join([
            str(edge_type),
            ",".join(str(value) for value in from_values),
            str(from_oid),
            str(to_oid),
            str(frame),
            normalize_path(source_file),
        ]))
    return normalized


def selector_ids(data: dict[str, Any]) -> list[int]:
    for key in ("definitionIds", "selectedWrapperIds", "oids", "objectIds"):
        if isinstance(data.get(key), list):
            return [int(value) for value in data[key]]
    return []


def selector_roots(data: dict[str, Any]) -> list[int]:
    for key in ("rootIds", "rootOids", "roots"):
        if isinstance(data.get(key), list):
            return [int(value) for value in data[key]]
    return []


def selector_declaration_count(data: dict[str, Any]) -> int | None:
    value = data.get("bodySourceDeclarationCount")
    if value is not None:
        return int(value)
    declarations = data.get("bodySourceDeclarations")
    if isinstance(declarations, list):
        return sum(len(item.get("files", [])) for item in declarations if isinstance(item, dict))
    return None


def compare_one_selector(projection: dict[str, Any], selector_path: Path) -> dict[str, Any]:
    if not selector_path.is_file():
        return {"path": str(selector_path), "status": "MISSING", "comparison": "not-run"}
    data = read_json(selector_path)
    expected_ids = [int(value) for value in projection.get("oids", [])]
    actual_ids = selector_ids(data)
    expected_edges = {structural_internal_edge(edge) for edge in projection.get("edges", [])}
    actual_edges = {structural_selector_edge(edge) for edge in data.get("edges", []) if isinstance(edge, dict)}
    expected_roots = [int(value) for value in projection.get("rootOids", [])]
    actual_roots = selector_roots(data)
    expected_decl = len(projection["images"].get("bodyDeclarations", []))
    actual_decl = selector_declaration_count(data)
    identity_equal = actual_ids == expected_ids and (not actual_roots or set(actual_roots) == set(expected_roots))
    actual_geometry = [
        (int(item["definitionId"]), normalize_path(file["sourcePath"]).lower(),
         file["startFrame"], file["endFrame"], file["width"], file["height"], file["row"], file["col"])
        for item in data.get("bodySourceDeclarations", []) for file in item.get("files", [])
    ]
    expected_geometry = [tuple(value) for value in projection.get("canonicalBodyDeclarations", [])]
    geometry_equal = sorted(actual_geometry) == sorted(expected_geometry)
    declaration_equal = actual_decl == expected_decl and geometry_equal
    edge_exact = expected_edges == actual_edges
    kind_context_witnesses = []
    for edge in projection.get("edges", []):
        if str(edge.get("type", "")).lower() not in {"kind_copy", "conditional_context"}:
            continue
        kind_context_witnesses.append({
            "originalIndependentEdge": edge,
            "normalizedStructuralIdentity": list(structural_internal_edge(edge)),
            "mapping": "bound attacker edge.toOid -> rawToken.targetOid at rawToken.frame; this is Kind interaction context, not fusion",
        })
    return {
        "path": str(selector_path),
        "status": "PASS" if identity_equal and declaration_equal else "DIFF",
        "closureAndDeclarationStatus": "PASS" if identity_equal and declaration_equal else "DIFF",
        "structuralEdgeStatus": "EXACT" if edge_exact else "UNRESOLVED_SOURCE_SEMANTIC_SHAPE",
        "expectedDefinitionCount": len(expected_ids),
        "actualDefinitionCount": len(actual_ids),
        "missingDefinitionIds": sorted(set(expected_ids) - set(actual_ids)),
        "unexpectedDefinitionIds": sorted(set(actual_ids) - set(expected_ids)),
        "expectedRoots": expected_roots,
        "actualRoots": actual_roots,
        "expectedEdgeCount": len(expected_edges),
        "actualEdgeCount": len(actual_edges),
        "edgeSetExact": edge_exact,
        "edgeIdentityContract": "normal=(fromId,toId,frameId); fusion=(fusion,sorted input OIDs,toId,frameId); kind context maps bound edge.toOid -> rawToken.targetOid/rawToken.frame",
        "semanticEdgeTypesCompared": False,
        "semanticEdgeDifferenceNote": "C# rule/type/source text is not used for structural closure equality; inspect independent edge records when semantic labels are needed.",
        "kindContextNormalizationWitnesses": kind_context_witnesses,
        "independentEdgesMissingFromSelector": [list(value) for value in sorted(expected_edges - actual_edges, key=str)[:200]],
        "selectorEdgesMissingFromIndependent": [list(value) for value in sorted(actual_edges - expected_edges, key=str)[:200]],
        "expectedBodyDeclarationCount": expected_decl,
        "actualBodyDeclarationCount": actual_decl,
        "bodyDeclarationCountEqual": actual_decl == expected_decl,
        "bodyDeclarationGeometryExact": geometry_equal,
        "bodyDeclarationGeometryContract": "OID/path/effective start-end/cell width-height/row-col exact tuple",
        "independentFingerprint": projection.get("fingerprint"),
        "selectorFingerprint": data.get("fingerprint"),
        "phase": data.get("phase"),
    }


def compare_selector_outputs(
    selection_root: Path | None,
    roles: list[dict[str, Any]],
    role_only: dict[int, dict[str, Any]],
    role_match: dict[int, dict[str, Any]],
    all_selectable: dict[str, Any],
    full_catalog: dict[str, Any],
) -> dict[str, Any]:
    if selection_root is None or not selection_root.exists():
        return {
            "status": "PENDING_NOT_READY",
            "selectionReady": False,
            "selectionRoot": str(selection_root) if selection_root else None,
            "reason": "C# selector results root is not present; independent audit remains runnable and no legacy output is substituted",
        }
    comparisons: list[dict[str, Any]] = []
    roles_path = selection_root / "roles.json"
    role_status = "MISSING"
    if roles_path.is_file():
        actual_roles = read_json(roles_path)
        expected_by_oid = {int(item["oid"]): item for item in roles}
        actual_by_oid = {int(item["oid"]): item for item in actual_roles if isinstance(item, dict) and item.get("oid") is not None}
        role_status = "PASS" if set(expected_by_oid) == set(actual_by_oid) and all(expected_by_oid[oid].get("name") == actual_by_oid[oid].get("name") for oid in expected_by_oid) else "DIFF"
        comparisons.append({"path": str(roles_path), "status": role_status, "expectedCount": len(roles), "actualCount": len(actual_roles), "missingOids": sorted(set(expected_by_oid) - set(actual_by_oid)), "unexpectedOids": sorted(set(actual_by_oid) - set(expected_by_oid))})
    else:
        comparisons.append({"path": str(roles_path), "status": role_status})
    for oid in sorted(role_only):
        comparisons.append({"name": f"role-{oid}-only", **compare_one_selector(role_only[oid], selection_root / f"role-{oid}-only.json")})
        comparisons.append({"name": f"role-{oid}-match", **compare_one_selector(role_match[oid], selection_root / f"role-{oid}-match.json")})
    comparisons.append({"name": "all-selectable-match", **compare_one_selector(all_selectable, selection_root / "all-selectable-match.json")})
    comparisons.append({"name": "full-catalog", **compare_one_selector(full_catalog, selection_root / "full-catalog.json")})
    statuses = [item.get("status") for item in comparisons]
    edge_comparisons = [item for item in comparisons if "edgeSetExact" in item]
    unresolved_edge_count = sum(not bool(item.get("edgeSetExact")) for item in edge_comparisons)
    closure_declaration_pass = bool(statuses) and all(status == "PASS" for status in statuses)
    return {
        "status": "PASS_CLOSURE_DECLARATIONS_EDGE_SHAPE_UNRESOLVED" if closure_declaration_pass and unresolved_edge_count else ("PASS" if closure_declaration_pass else "DIFF"),
        "selectionReady": True,
        "selectionRoot": str(selection_root),
        "comparisonCount": len(comparisons),
        "passCount": sum(status == "PASS" for status in statuses),
        "diffCount": sum(status == "DIFF" for status in statuses),
        "missingCount": sum(status == "MISSING" for status in statuses),
        "closureAndDeclarationStatus": "PASS" if closure_declaration_pass else "DIFF",
        "structuralEdgeComparison": {
            "status": "EXACT" if unresolved_edge_count == 0 else "UNRESOLVED_SOURCE_SEMANTIC_SHAPE",
            "comparisonCount": len(edge_comparisons),
            "unresolvedCount": unresolved_edge_count,
            "contract": "normal=(fromId,toId,frameId); fusion=(fusion,sorted input OIDs,toId,frameId)",
            "semanticEdgeTypesCompared": False,
            "claimBoundary": "Only closure/declaration equality is promoted while source semantic edge-shape differences remain unresolved.",
        },
        "comparisons": comparisons,
        "boundary": "C# output is compared as an independent export; Python closure/witness/resource results remain authoritative for this audit's independent side.",
    }


def render_matrix_csv(path: Path, rows: list[dict[str, Any]]) -> None:
    fields = [
        "scope", "kind", "path", "normalizedPath", "sourcePath", "sourceSha256", "sha256", "expectedSha256",
        "exists", "bytes", "shaMatch", "width", "height", "rgbaBytes", "ownerOids", "pageId", "astcPath",
        "astcSha256", "astcBytes", "sourceShaMatches", "mappingIndex", "sourceAssetKnown", "astcUsable", "alternate", "provenance",
    ]
    with path.open("x", encoding="utf-8", newline="") as stream:
        writer = csv.DictWriter(stream, fieldnames=fields, extrasaction="ignore")
        writer.writeheader()
        for row in rows:
            record = dict(row)
            record["ownerOids"] = json.dumps(record.get("ownerOids", []), ensure_ascii=False, separators=(",", ":"))
            writer.writerow(record)


def render_rows_csv(path: Path, rows: list[dict[str, Any]], fields: list[str]) -> None:
    with path.open("x", encoding="utf-8", newline="") as stream:
        writer = csv.DictWriter(stream, fieldnames=fields, extrasaction="ignore")
        writer.writeheader()
        for row in rows:
            record = dict(row)
            for field in fields:
                if isinstance(record.get(field), (list, dict)):
                    record[field] = json.dumps(record[field], ensure_ascii=False, separators=(",", ":"))
            writer.writerow(record)


def make_menu_projection(all_ui_unique: list[dict[str, Any]]) -> dict[str, Any]:
    return {
        "name": "menu",
        "phase": "Menu",
        "rootOids": [],
        "rootNames": [],
        "definitionIds": [],
        "oids": [],
        "includeScenario": False,
        "includeCompatibility": False,
        "contextVictimPool": [],
        "closure": {"processedCount": 0, "cycleRevisitCount": 0, "definitionCount": 0, "edgeCount": 0, "missingIdCount": 0},
        "datList": [],
        "datCount": 0,
        "rawDatBytesUnique": 0,
        "parsedObjectMemory": {"status": "not-applicable", "frameCount": 0, "opointCount": 0, "cpointCount": 0},
        "images": {"bodyAssets": [], "bodyDeclarations": [], "uiInstances": [], "uiUniqueAssets": [], "clampCells": [], "bodyUniqueCount": 0, "bodyDeclarationCount": 0, "uiInstanceCount": 0, "uiUniqueCount": len(all_ui_unique)},
        "allUiUniqueAssets": all_ui_unique,
        "allUiInstanceCount": 0,
        "gridValidation": {"sourceSheetCount": 0, "ownedDeclarationCount": 0, "productionClampCellCount": 0, "astcClampConflicts": []},
        "edges": [],
        "unknowns": [],
        "missingIds": [],
        "missingWitnesses": [],
        "branchProfile": "production",
        "scenarioContract": None,
    }


def make_budget_case(helper: Any, projection: dict[str, Any], name: str, common_records: list[dict[str, Any]], astc: dict[str, Any], pages: dict[str, Any]) -> dict[str, Any]:
    common = {"currentRecords": common_records, "currentResidentBytes": sum(
        int(record.get("rgbaBytes") or 0) + (int(record.get("rgbaBytes") or 0) if record.get("readableTexture") else 0)
        for record in common_records
    )}
    # The full catalogue is the cold all-image comparison.  Role/match/case
    # entries represent the production transition from the already-published
    # menu, exactly as MatchImageResidency.py does.
    if name == "menu":
        include_ui, include_spark, old_publication, outside = True, False, 0, False
    elif name == "full-catalog":
        include_ui, include_spark, old_publication, outside = True, True, 0, True
    else:
        include_ui, include_spark, old_publication, outside = False, True, 27_673_952, True
    budget = production_case_budget(helper, projection, astc, common, mode="astc4x4_all", include_ui=include_ui, include_spark=include_spark, old_publication_bytes=old_publication, add_outside_common=outside)
    rgba_budget = production_case_budget(helper, projection, astc, common, mode="rgba_fallback", include_ui=include_ui, include_spark=include_spark, old_publication_bytes=old_publication, add_outside_common=outside)
    return {
        "name": name,
        "rootOids": projection.get("rootOids", []),
        "definitionIds": projection.get("oids", []),
        "oldPublicationBytes": old_publication,
        "atlasBytes": 0,
        "images": budget["images"],
        "clampedCells": budget["clampedCells"],
        "expected": {key: budget[key] for key in ("residentBytes", "loadingBytes", "sourceGpuBytes", "readableCpuBytes", "retainedRgbaBytes", "oldPublicationBytes", "atlasReservationBytes")},
        "rgbaFallbackExpected": {key: rgba_budget[key] for key in ("residentBytes", "loadingBytes", "sourceGpuBytes", "readableCpuBytes", "retainedRgbaBytes", "oldPublicationBytes", "atlasReservationBytes")},
        "alternateBudgets": {
            "rgbaFallback": rgba_budget,
            "astc4x4All": budget,
        },
        "withCurrentMapCommonResidentBytes": budget["withCurrentMapCommonResidentBytes"],
        "withCurrentMapCommonLoadingBytes": budget["withCurrentMapCommonLoadingBytes"],
        "rgbaFallbackWithCurrentMapCommonResidentBytes": rgba_budget["withCurrentMapCommonResidentBytes"],
        "rgbaFallbackWithCurrentMapCommonLoadingBytes": rgba_budget["withCurrentMapCommonLoadingBytes"],
        "astc2048Capability": oversize_refusal(projection),
        "auditMode": "astc4x4_all; exact MatchImageResidency make_case arithmetic; actual ASTC sidecar files and current PNG identities are embedded in images",
        "boundary": "C# --budget-input arithmetic verifies this image plan only; it is not a Unity/device/process memory claim.",
    }


def shared_duplicate_rows(matrix: list[dict[str, Any]]) -> list[dict[str, Any]]:
    by_sha: dict[str, list[dict[str, Any]]] = defaultdict(list)
    by_astc: dict[str, list[dict[str, Any]]] = defaultdict(list)
    for row in matrix:
        sha = str(row.get("sha256") or "").lower()
        if sha:
            by_sha[sha].append(row)
        astc_path = normalize_path(row.get("astcPath")).lower()
        if astc_path:
            by_astc[astc_path].append(row)
    result: list[dict[str, Any]] = []
    seen: set[str] = set()
    for sha, rows in by_sha.items():
        owners = sorted({oid for row in rows for oid in row.get("ownerOids", [])})
        paths = sorted({row.get("path") for row in rows})
        if len(rows) < 2 and len(owners) < 2:
            continue
        marker = f"sha|{sha}"
        seen.add(marker)
        result.append({"identity": marker, "identityType": "png-sha256", "sha256": sha, "referenceCount": len(rows), "referencePathCount": len(paths), "physicalPathCount": len(paths), "physicalFileDuplicate": len(paths) > 1, "ownerOids": owners, "paths": paths, "scopes": sorted({row.get("scope") for row in rows}), "groupMeaning": "same SHA identity grouped across matrix references/scopes; referenceCount is not a physical duplicate-file count"})
    for astc_path, rows in by_astc.items():
        if len(rows) < 2:
            continue
        marker = f"astc|{astc_path}"
        if marker in seen:
            continue
        reference_paths = sorted({row.get("path") for row in rows})
        physical_astc_paths = sorted({normalize_path(row.get("astcPath")).lower() for row in rows if row.get("astcPath")})
        result.append({"identity": marker, "identityType": "astc-physical-block", "astcPath": astc_path, "referenceCount": len(rows), "referencePathCount": len(reference_paths), "physicalPathCount": len(physical_astc_paths), "physicalFileDuplicate": len(physical_astc_paths) > 1, "ownerOids": sorted({oid for row in rows for oid in row.get("ownerOids", [])}), "paths": reference_paths, "scopes": sorted({row.get("scope") for row in rows}), "groupMeaning": "same ASTC physical block referenced by multiple matrix rows/scopes; this group is not a duplicate physical-file count"})
    return sorted(result, key=lambda item: item["identity"])


def input_file_records(repository_root: Path, cache: IdentityCache, paths: list[Path]) -> list[dict[str, Any]]:
    result = []
    for path in paths:
        identity = cache.identity(path)
        result.append({"path": str(path), "exists": identity["exists"], "bytes": identity["bytes"], "sha256": identity["sha256"]})
    return result


def run_audit(args: argparse.Namespace) -> int:
    repository_root = Path(args.repository_root).resolve()
    output_root = Path(args.output).resolve()
    if output_root.exists():
        raise AuditFailure(f"diagnostic output must be fresh and non-existent: {output_root}")
    if not str(output_root).lower().startswith(str((repository_root / "artifacts/diagnostics/M12-full-image-coverage-20261014/memory-worker").resolve()).lower() + "\\"):
        raise AuditFailure("--output must remain under artifacts/diagnostics/M12-full-image-coverage-20261014/memory-worker")
    output_root.mkdir(parents=True, exist_ok=False)

    cache = IdentityCache(repository_root)
    helper = import_helper(repository_root)
    install_helper_identity_cache(helper, cache)
    capture_path = path_from(args.capture or CAPTURE_DEFAULT, repository_root)
    pages_path = path_from(args.pages_manifest or PAGES_DEFAULT, repository_root)
    astc_path = path_from(args.astc_manifest or ASTC_DEFAULT, repository_root)
    common_path = path_from(args.common_input or COMMON_DEFAULT, repository_root)
    capture = read_json(capture_path)
    objects = helper.capture_object_map(capture)
    pool = helper.selectable_pool(capture)
    pages = helper.load_pages_manifest(str(pages_path))
    astc = helper.load_astc_manifest(str(astc_path))
    common = load_common_records(repository_root, common_path, cache)
    stage = inspect_stage_configuration(repository_root, cache)
    input_verification = verify_capture_inputs(capture, cache)
    input_verification["stageConfiguration"] = stage
    input_verification["sourceInputs"] = input_file_records(repository_root, cache, [
        repository_root / "Tools/Audit-NtsdFullImageCoverage.py",
        repository_root / "Tools/Audit-NtsdEightRoleMemory.py",
        repository_root / "Tools/Audit-NtsdMatchImageResidency.py",
        capture_path,
        pages_path,
        astc_path,
        common_path,
    ])
    write_json_new(output_root / "input-verification.json", input_verification)
    if input_verification["status"] != "PASS":
        failure = {"status": "FAIL", "taskId": TASK_ID, "errorType": "capture-drift", "errors": input_verification["errors"], "inputVerification": "input-verification.json"}
        write_json_new(output_root / "failure.json", failure)
        return 2
    ensure_inventory_counts(capture, pool, pages, astc, common)
    if not pages.get("usable") or not astc.get("usable"):
        raise AuditFailure(f"page/ASTC manifest invalid: pages={pages.get('errors')} astc={astc.get('errors')}")

    roles = [dict(item) for item in pool]
    write_json_new(output_root / "roles.json", roles)
    all_ui_instances, all_ui_errors = helper.ui_instances_for_oids(sorted(objects), objects)
    if all_ui_errors:
        raise AuditFailure("full UI inventory has errors: " + "; ".join(all_ui_errors))
    all_ui_unique = list({item["asset"]["key"]: item["asset"] for item in all_ui_instances}.values())
    if len(all_ui_unique) != EXPECTED_UI_UNIQUE:
        raise AuditFailure(f"full UI unique count {len(all_ui_unique)} != {EXPECTED_UI_UNIQUE}")
    body_cache: dict[tuple[int, ...], tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]], list[str]]] = {}
    ui_cache: dict[tuple[int, ...], tuple[list[dict[str, Any]], list[str]]] = {}
    role_only: dict[int, dict[str, Any]] = {}
    role_match: dict[int, dict[str, Any]] = {}
    for role in roles:
        oid = int(role["oid"])
        only = build_projection(helper, objects, capture, pages, astc, f"role-{oid}-only", [oid], False, False, [oid], body_cache, ui_cache)
        match = build_projection(helper, objects, capture, pages, astc, f"role-{oid}-match", [oid], True, True, [oid], body_cache, ui_cache)
        only["allUiUniqueAssets"] = all_ui_unique
        match["allUiUniqueAssets"] = all_ui_unique
        attach_production_budgets(helper, only, all_ui_instances, common, astc, pages)
        attach_production_budgets(helper, match, all_ui_instances, common, astc, pages)
        role_only[oid] = only
        role_match[oid] = match
        write_json_new(output_root / f"role-{oid}-only.json", only)
        write_json_new(output_root / f"role-{oid}-match.json", match)

    pool_oids = [int(role["oid"]) for role in roles]
    all_selectable = build_projection(helper, objects, capture, pages, astc, "all-selectable-match", pool_oids, True, True, pool_oids, body_cache, ui_cache)
    all_selectable["allUiUniqueAssets"] = all_ui_unique
    attach_production_budgets(helper, all_selectable, all_ui_instances, common, astc, pages)
    full_catalog = build_projection(helper, objects, capture, pages, astc, "full-catalog", sorted(objects), True, True, sorted(objects), body_cache, ui_cache)
    full_catalog["allUiUniqueAssets"] = all_ui_unique
    attach_production_budgets(helper, full_catalog, all_ui_instances, common, astc, pages)
    write_json_new(output_root / "all-selectable-match.json", all_selectable)
    write_json_new(output_root / "full-catalog.json", full_catalog)

    recommendations, rationale = build_recommendations(pool, role_match, role_only, capture, objects)
    named_cases: dict[str, dict[str, Any]] = {}

    def build_named_case(case: dict[str, Any]) -> dict[str, Any]:
        roots = list(case.get("roots", []))
        additional = list(case.get("additionalRoots", []))
        combined = list(dict.fromkeys(roots + additional))
        projection = build_projection(helper, objects, capture, pages, astc, f"case-{case['name']}", combined, True, True, combined, body_cache, ui_cache)
        projection["inputCase"] = {"name": case["name"], "roots": roots, "additionalRoots": additional}
        projection["fusionContextAnalysis"] = combination_analysis(helper, objects, capture, combined)
        projection["allUiUniqueAssets"] = all_ui_unique
        attach_production_budgets(helper, projection, all_ui_instances, common, astc, pages)
        return projection

    for case in recommendations["cases"]:
        named_cases[case["name"]] = build_named_case(case)
    if args.plans_input:
        for case in parse_plans_input(path_from(args.plans_input, repository_root), objects):
            named_cases[case["name"]] = build_named_case(case)

    # Attach exact corrected admission numbers to the bounded desktop samples
    # after their real combined closures have been constructed.  This keeps the
    # recommendation file reviewable without presenting a proxy/union estimate.
    bounded_case_budget_review: list[dict[str, Any]] = []
    for case in recommendations["cases"]:
        projection = named_cases.get(case["name"])
        if projection is None:
            continue
        summary = projection["budgetSummary"]
        rgba_resident = summary["rgbaFallbackFromMenuWithCurrentMapCommonResidentBytes"]
        rgba_loading = summary["rgbaFallbackFromMenuWithCurrentMapCommonLoadingBytes"]
        astc_resident = summary["astcFromMenuWithCurrentMapCommonResidentBytes"]
        astc_loading = summary["astcFromMenuWithCurrentMapCommonLoadingBytes"]
        case["combinedClosure"] = {
            "definitionCount": len(projection["oids"]),
            "edgeCount": len(projection["edges"]),
            "bodyAssetCount": len(projection["images"]["bodyAssets"]),
            "clampCellCount": len(projection["images"]["clampCells"]),
            "fusionContextDifference": projection.get("fusionContextAnalysis", {}).get("fusionContextDifference"),
        }
        case["desktopRgbaAdmission"] = {
            "status": "ADMITTED" if rgba_resident < 512 * 1024 * 1024 and rgba_loading < 768 * 1024 * 1024 else "OVER_LIMIT",
            "residentBytes": rgba_resident,
            "loadingBytes": rgba_loading,
            "residentMiB": rgba_resident / (1024 * 1024),
            "loadingMiB": rgba_loading / (1024 * 1024),
            "limitsMiB": {"resident": 512, "loading": 768},
            "includesOutsideCurrentMapFootShadow": True,
        }
        case["boundedBudgetReview"] = {
            "astcResidentBytes": astc_resident,
            "astcLoadingBytes": astc_loading,
            "rgbaResidentBytes": rgba_resident,
            "rgbaLoadingBytes": rgba_loading,
            "rgbaBoundaryStatus": "ADMITTED" if rgba_resident < 512 * 1024 * 1024 and rgba_loading < 768 * 1024 * 1024 else "OVER_LIMIT",
            "scope": "from-menu old publication plus SPARK and separately added current map/foot/shadow; image estimate only",
        }
        bounded_case_budget_review.append({
            "name": case["name"],
            "roots": case["roots"],
            "selectionMethod": case.get("selectionMethod"),
            "definitionCount": len(projection["oids"]),
            "edgeCount": len(projection["edges"]),
            "bodyAssetCount": len(projection["images"]["bodyAssets"]),
            "fusionContextDifference": projection.get("fusionContextAnalysis", {}).get("fusionContextDifference"),
            **case["boundedBudgetReview"],
        })

    def assert_case_budget(label: str, projection: dict[str, Any] | None, expected: dict[str, int]) -> dict[str, Any]:
        if projection is None:
            raise AuditFailure(f"required budget baseline case missing: {label}")
        summary = projection["budgetSummary"]
        actual = {
            "astcResidentBytes": int(summary["astcFromMenuWithCurrentMapCommonResidentBytes"] if label != "full-catalog" else summary["astcColdWithCurrentMapCommonResidentBytes"]),
            "astcLoadingBytes": int(summary["astcFromMenuWithCurrentMapCommonLoadingBytes"] if label != "full-catalog" else summary["astcColdWithCurrentMapCommonLoadingBytes"]),
            "rgbaResidentBytes": int(summary["rgbaFallbackFromMenuWithCurrentMapCommonResidentBytes"]),
            "rgbaLoadingBytes": int(summary["rgbaFallbackFromMenuWithCurrentMapCommonLoadingBytes"]),
        }
        if actual != expected:
            raise AuditFailure(f"budget baseline drift for {label}: actual={actual} expected={expected}")
        return {"label": label, "status": "PASS", "actual": actual, "expected": expected, "definitionCount": len(projection["oids"]), "bodyAssetCount": len(projection["images"]["bodyAssets"])}

    previous_baseline = assert_case_budget(
        "previous-8-regression",
        named_cases.get("previous-8-regression"),
        {"astcResidentBytes": 96_455_512, "astcLoadingBytes": 132_998_584, "rgbaResidentBytes": 271_621_308, "rgbaLoadingBytes": 640_495_948},
    )
    full_baseline = assert_case_budget(
        "full-catalog",
        full_catalog,
        {"astcResidentBytes": 495_342_216, "astcLoadingBytes": 558_097_352, "rgbaResidentBytes": 1_871_957_400, "rgbaLoadingBytes": 3_717_449_036},
    )
    desktop_high_baseline = assert_case_budget(
        "desktop-boundary-high",
        named_cases.get("desktop-high-review") or named_cases.get("desktop-boundary-high"),
        {"astcResidentBytes": 126_916_952, "astcLoadingBytes": 168_100_984, "rgbaResidentBytes": 393_332_392, "rgbaLoadingBytes": 855_169_944},
    )
    desktop_fusion_baseline = assert_case_budget(
        "desktop-fusion-context",
        named_cases.get("desktop-fusion-review") or named_cases.get("desktop-fusion-context"),
        {"astcResidentBytes": 110_760_952, "astcLoadingBytes": 147_304_024, "rgbaResidentBytes": 328_617_716, "rgbaLoadingBytes": 751_522_876},
    )
    budget_baseline_assertions = {
        "status": "PASS",
        "method": "exact byte equality against the accepted previous-8/full-catalog and main desktop preflight inputs",
        "checks": [previous_baseline, full_baseline, desktop_high_baseline, desktop_fusion_baseline],
        "boundary": "These are image allocation model assertions only; they do not assert Unity/device/process peak or runtime acceptance.",
    }
    rationale += "\n## Corrected desktop boundary samples\n\n"
    rationale += "The two desktop samples below are bounded candidate substitutions. Their status is based on the exact combined closure and corrected SourceTexture2D RGBA model, including the separately added current map/foot/shadow bytes; this is not a global search result.\n\n"
    for case in recommendations["cases"]:
        admission = case.get("desktopRgbaAdmission")
        if admission:
            rationale += f"- `{case['name']}` roots `{case['roots']}`: **{admission['status']}**, resident {admission['residentMiB']:.3f} MiB, loading {admission['loadingMiB']:.3f} MiB; combined definitions {case['combinedClosure']['definitionCount']}, edges {case['combinedClosure']['edgeCount']}, fusion/context difference `{case['combinedClosure']['fusionContextDifference']}`.\n"
    rationale += "\nThe original `greedy-high-astc-union`, `dependency-complex`, and `real-fusion-context` entries remain explicit ASTC/max-stress samples. Their RGBA values are reported as over-limit when they exceed the desktop boundary; they do not silently raise the boundary or become global maxima.\n"
    write_json_new(output_root / "recommended-cases.json", recommendations)
    write_text_new(output_root / "recommended-cases-rationale.md", rationale)
    for name, projection in named_cases.items():
        write_json_new(output_root / f"case-{safe_case_name(name)}.json", projection)

    menu = make_menu_projection(all_ui_unique)
    attach_production_budgets(helper, menu, all_ui_instances, common, astc, pages)
    budget_projection_list: list[tuple[str, dict[str, Any]]] = [("menu", menu)]
    budget_projection_list.extend((f"role-{oid}-only", role_only[oid]) for oid in sorted(role_only))
    budget_projection_list.extend((f"role-{oid}-match", role_match[oid]) for oid in sorted(role_match))
    budget_projection_list.extend([("all-selectable-match", all_selectable), ("full-catalog", full_catalog)])
    budget_projection_list.extend((f"case-{name}", projection) for name, projection in named_cases.items())
    budget_cases: list[dict[str, Any]] = []
    seen_budget_names: set[str] = set()
    for name, projection in budget_projection_list:
        if name in seen_budget_names:
            continue
        seen_budget_names.add(name)
        astc_case = make_budget_case(helper, projection, name, common["currentRecords"], astc, pages)
        budget_cases.append(astc_case)
        rgba = astc_case["alternateBudgets"]["rgbaFallback"]
        budget_cases.append({
            "name": name + "-rgba", "oldPublicationBytes": rgba["oldPublicationBytes"], "atlasBytes": 0,
            "images": rgba["images"], "clampedCells": rgba["clampedCells"],
            "expected": {key: rgba[key] for key in ("residentBytes", "loadingBytes", "sourceGpuBytes",
                "readableCpuBytes", "retainedRgbaBytes", "oldPublicationBytes", "atlasReservationBytes")},
        })
    write_json_new(output_root / "production-budget-cases.json", {"cases": budget_cases, "caseCount": len(budget_cases), "inputContract": "actual current PNG/ASTC paths, dimensions, file bytes, and SHA identities; no synthetic image sizes"})

    matrix = build_resource_matrix(helper, cache, objects, pages, astc, common, full_catalog, all_ui_unique, role_match)
    write_json_new(output_root / "full-resource-matrix.json", matrix)
    render_matrix_csv(output_root / "full-resource-matrix.csv", matrix)
    per_role = full_resource_lists(role_only, role_match, common)
    write_json_new(output_root / "per-role-resource-lists.json", per_role)
    render_rows_csv(output_root / "per-role-resource-lists.csv", per_role, ["oid", "name", "roleOnlyDefinitionCount", "matchDefinitionCount", "roleOnlyBodyAssetCount", "matchBodyAssetCount", "allUiUniqueCount", "commonAssetCount", "roleOnlyResourceCount", "matchResourceCount"])
    all_projections = list(role_only.values()) + list(role_match.values()) + [all_selectable, full_catalog] + list(named_cases.values())
    witnesses = missing_witnesses(all_projections)
    write_json_new(output_root / "missing-witnesses.json", {"missingIds": sorted({int(item["targetOid"]) for item in witnesses if item.get("targetOid") is not None}), "witnessCount": len(witnesses), "witnesses": witnesses})
    render_rows_csv(output_root / "missing-witnesses.csv", witnesses, ["targetOid", "kind", "projection", "reason", "edge"])
    oversized = oversized_resources(matrix)
    write_json_new(output_root / "oversized-resources.json", {"count": len(oversized), "resources": oversized, "thresholds": {"oversized": ">2048 max dimension", "astcEligibility": "usable ASTC manifest entry plus device ASTC support and max texture size >= source dimensions", "over8192": ">8192 max dimension"}, "contract": "The oversized list is a source-dimension capability/refusal inventory; it does not classify ASTC4x4 quality by a large-only threshold."})
    render_rows_csv(output_root / "oversized-resources.csv", oversized, ["scope", "kind", "path", "width", "height", "maxDimension", "astcUsableManifestEntry", "astcEligibilityContract", "over2048Capability", "over8192", "ownerOids", "sha256"])
    duplicates = shared_duplicate_rows(matrix)
    write_json_new(output_root / "shared-duplicate-map.json", {"count": len(duplicates), "entries": duplicates, "identityRule": "groups exact PNG SHA or exact physical ASTC block path across matrix scopes; count is identity groups, not physical duplicate files", "physicalDuplicateCountMeaning": "inspect each entry physicalFileDuplicate/physicalPathCount; no pixel-content approximation"})
    render_rows_csv(output_root / "shared-duplicate-map.csv", duplicates, ["identity", "identityType", "sha256", "astcPath", "referenceCount", "referencePathCount", "physicalPathCount", "physicalFileDuplicate", "ownerOids", "paths", "scopes", "groupMeaning"])
    write_json_new(output_root / "common-assets.json", common)
    write_json_new(output_root / "pages-manifest-projection.json", {"manifestPath": pages.get("manifestPath"), "status": pages.get("status"), "pageCount": len(pages.get("pages", {})), "sourceCount": len(pages.get("sources", [])), "pages": pages.get("pages", {}), "sources": pages.get("sources", []), "errors": pages.get("errors", [])})
    write_json_new(output_root / "astc-manifest-projection.json", {"manifestPath": astc.get("manifestPath"), "status": astc.get("status"), "mappingCount": astc.get("entryCount"), "normalizedKeyCount": len(astc.get("entries", {})), "uniquePhysicalBlockCount": astc.get("uniqueBlockCount"), "errors": astc.get("errors", []), "entries": [{"sourcePath": item.get("sourcePath"), "sourceSha256": item.get("sourceSha256"), "filesystemPath": item.get("filesystemPath"), "identity": item.get("identity"), "width": item.get("width"), "height": item.get("height"), "payloadBytes": item.get("payloadBytes"), "fileBytes": item.get("fileBytes"), "usable": item.get("usable")} for item in (astc.get("raw", {}).get("entries", []))]} )

    selection_root = path_from(args.selection_root, repository_root) if args.selection_root else None
    selection_comparison = compare_selector_outputs(selection_root, roles, role_only, role_match, all_selectable, full_catalog)
    write_json_new(output_root / "selection-comparison.json", selection_comparison)
    if selection_comparison.get("status") not in {"PASS", "PENDING_NOT_READY", "PASS_CLOSURE_DECLARATIONS_EDGE_SHAPE_UNRESOLVED"}:
        raise AuditFailure("Independent selector comparison failed; inspect selection-comparison.json")
    selector_input_paths = []
    if selection_root is not None:
        selector_input_paths.extend(selection_root / name for name in ("summary.json", "roles.json", "all-selectable-match.json", "full-catalog.json"))
    if args.plans_input:
        selector_input_paths.append(path_from(args.plans_input, repository_root))
    for main_preflight_name in ("desktop-cases-main-preflight-01.json", "desktop-cases-main-budget-input-01.json"):
        main_preflight_path = repository_root / "artifacts/diagnostics/M12-full-image-coverage-20261014" / main_preflight_name
        if main_preflight_path.is_file():
            selector_input_paths.append(main_preflight_path)
    selector_input_provenance = input_file_records(repository_root, cache, selector_input_paths)
    write_json_new(output_root / "selector-input-provenance.json", {
        "selectionRoot": str(selection_root) if selection_root else None,
        "plansInput": str(path_from(args.plans_input, repository_root)) if args.plans_input else None,
        "files": selector_input_provenance,
        "contract": "results-03 selector summary/exports, optional plans input, and available main preflight/budget evidence are hashed as read; results-01/results-02 are preserved superseded evidence and are not consumed",
    })

    selector_stage = None
    if selection_root is not None and (selection_root / "summary.json").is_file():
        selector_summary = read_json(selection_root / "summary.json")
        selector_stage = {
            "source": str(selection_root / "summary.json"),
            "status": selector_summary.get("status"),
            "stageAllWaveRoots": selector_summary.get("stageAllWaveRoots"),
            "stageRootsSource": selector_summary.get("stageRootsSource"),
            "stageInputScope": selector_summary.get("stageInputScope"),
            "stageConfigValue": selector_summary.get("stageConfigValue"),
            "stageResolvedPath": selector_summary.get("stageResolvedPath"),
            "stageSelectedFileExists": selector_summary.get("stageSelectedFileExists"),
            "stageParseStatus": selector_summary.get("stageParseStatus"),
            "stageUnverified": selector_summary.get("stageUnverified"),
            "stageSourceSha256": selector_summary.get("stageSourceSha256"),
        }
    authoritative_stage = {
        "authority": "C# results-03 MatchConfig/driver/loader provenance when supplied",
        "selector": selector_stage,
        "supportingSerializedConfigInspection": stage,
        "claimBoundary": "The GameConfig inspection is supporting evidence only and is not used as the stage proof.",
    }

    full_astc = full_catalog["budgetSummary"]
    full_rgba = full_catalog["budgetSummary"]
    summary_status = "PASS_SELECTION_PENDING" if selection_comparison.get("status") == "PENDING_NOT_READY" else (
        "PASS_CLOSURE_DECLARATIONS_EDGE_SHAPE_UNRESOLVED"
        if str(selection_comparison.get("status", "")).startswith("PASS_CLOSURE_DECLARATIONS")
        else "PASS_WITH_SELECTOR_COMPARISON"
    )
    summary = {
        "status": summary_status,
        "taskId": TASK_ID,
        "changeId": AUDIT_ID,
        "operationId": OPERATION_ID,
        "repositoryRoot": str(repository_root),
        "capturePath": str(capture_path),
        "captureSha256": sha256_file(capture_path),
        "catalog": catalog_counts(capture, pool),
        "inventory": {"pages": len(pages.get("pages", {})), "pageSources": len(pages.get("sources", [])), "astcMappings": astc.get("entryCount"), "astcPhysicalBlocks": astc.get("uniqueBlockCount"), "uiUnique": len(all_ui_unique), "fullBodyAssets": len(full_catalog["images"]["bodyAssets"]), "fullBodyDeclarations": len(full_catalog["images"]["bodyDeclarations"]), "fullClampCells": len(full_catalog["images"]["clampCells"])},
        "missingIds": sorted({int(item["targetOid"]) for item in witnesses if item.get("targetOid") is not None}),
        "missingWitnessCount": len(witnesses),
        "oversizedCount": len(oversized),
        "sharedIdentityGroupCount": len(duplicates),
        "sharedDuplicateCount": len(duplicates),
        "sharedDuplicateCountMeaning": "identity groups across mixed matrix scopes/path/SHA references; not a physical duplicate-file total",
        "stageConfiguration": authoritative_stage,
        "recommendedCases": recommendations,
        "boundedCaseBudgetReview": bounded_case_budget_review,
        "stressCaseBoundary": "greedy-high-astc-union, dependency-complex, and real-fusion-context remain bounded ASTC/max-stress samples; desktop admission is reported per case and is not replaced by a global maximum claim",
        "budgetBaselineAssertions": budget_baseline_assertions,
        "selectionComparison": selection_comparison,
        "selectorInputProvenance": selector_input_provenance,
        "fullCatalogBudgets": {"astc": full_astc, "rgbaFallback": full_rgba},
        "budgetMethod": "same production arithmetic as Audit-NtsdMatchImageResidency.py make_case; ASTC uses every usable exact-header/length entry; SourceTexture2D atlasBytes=0; ASTC mode retains no pixels; RGBA fallback retains raw body/SPARK/clamp CPU pixels until publication; <=2048 capability refuses oversized sources instead of filtering them",
        "boundary": "Static source/capture/resource audit only. No Unity Play, device, phone, GPU, server, process/global maximum, or scene acceptance claim.",
    }
    write_json_new(output_root / "summary.json", summary)
    return 0


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repository-root", default=".")
    parser.add_argument("--capture", default=CAPTURE_DEFAULT)
    parser.add_argument("--pages-manifest", default=PAGES_DEFAULT)
    parser.add_argument("--astc-manifest", default=ASTC_DEFAULT)
    parser.add_argument("--common-input", default=COMMON_DEFAULT)
    parser.add_argument("--selection-root", default=None, help="C# selector result root; missing root is reported as pending")
    parser.add_argument("--plans-input", default=None, help="JSON object {cases:[{name,roots,additionalRoots?:[]}]}")
    parser.add_argument("--output", required=True, help="fresh output directory under the M12 memory-worker diagnostics root")
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    try:
        return run_audit(parse_args(argv))
    except (AuditFailure, OSError, ValueError, KeyError, json.JSONDecodeError) as error:
        # The output path may not exist when argument/input validation fails.
        # Do not create an unscoped fallback artifact; callers get a concise
        # stderr failure and can choose a fresh diagnostic revision explicitly.
        print(f"FAIL: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
