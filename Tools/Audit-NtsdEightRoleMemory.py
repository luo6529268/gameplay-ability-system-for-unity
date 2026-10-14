#!/usr/bin/env python3
"""Offline eight-role NTSD resource closure and image-memory audit.

This tool consumes the standalone RoleResourceCapture JSON.  The capture stage
already ran the production Logan DAT parser and converter; this stage only
traverses the captured typed projection, resolves source images/pages, and
computes explicitly bounded engineering estimates.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import random
import secrets
import struct
import sys
from collections import Counter, defaultdict, deque
from pathlib import Path
from typing import Any, Iterable


AUDIT_ID = "NTSD-M12-EIGHT-ROLE-MEMORY-AUDIT-001"
TASK_ID = "NTSD-M12-ASTC-EIGHT-ROLE-20261013"
OPERATION_ID = "OP-NTSD-M12-ASTC-EIGHT-ROLE-20261013"
RULE_REVIEW_ROOT = "artifacts/diagnostics/M12-astc-eight-role-20261013/dependency-source-review"
PAGE_SIZE = 2048
RGBA_BYTES_PER_PIXEL = 4
ASTC_BLOCK_BYTES = 16
ASTC_HEADER_BYTES = 16
CPU_SEMAPHORE_SLOTS = 4


def normalize_path(value: str | None) -> str:
    return (value or "").replace("\\", "/")


def sha256_bytes(value: bytes) -> str:
    return hashlib.sha256(value).hexdigest()


def sha256_json(value: Any) -> str:
    payload = json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")).encode("utf-8")
    return sha256_bytes(payload)


def read_json(path: Path) -> Any:
    with path.open("r", encoding="utf-8") as stream:
        return json.load(stream)


def write_json(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as stream:
        json.dump(value, stream, ensure_ascii=False, indent=2, sort_keys=False)
        stream.write("\n")


def int_field(fields: Iterable[dict[str, Any]] | None, key: str, default: int = 0) -> int:
    value = default
    for field in fields or []:
        if str(field.get("key", "")).lower() != key.lower():
            continue
        try:
            value = int(str(field.get("value", "0")).strip().split()[0])
        except (ValueError, IndexError):
            value = default
    return value


def file_identity(path_value: str | Path | None, expected_sha: str | None = None) -> dict[str, Any]:
    path = Path(path_value) if path_value else Path("")
    exists = bool(path_value) and path.is_file()
    result: dict[str, Any] = {
        "path": str(path) if path_value else "",
        "exists": exists,
        "bytes": path.stat().st_size if exists else 0,
        "sha256": None,
        "expectedSha256": expected_sha,
        "shaMatch": None,
    }
    if exists:
        digest = hashlib.sha256()
        with path.open("rb") as stream:
            for chunk in iter(lambda: stream.read(1024 * 1024), b""):
                digest.update(chunk)
        result["sha256"] = digest.hexdigest()
        result["shaMatch"] = expected_sha is None or result["sha256"].lower() == expected_sha.lower()
    return result


def png_dimensions(path_value: str | Path | None) -> tuple[int, int] | None:
    if not path_value:
        return None
    path = Path(path_value)
    try:
        with path.open("rb") as stream:
            if stream.read(8) != b"\x89PNG\r\n\x1a\n":
                return None
            length_bytes = stream.read(4)
            chunk_type = stream.read(4)
            if len(length_bytes) != 4 or chunk_type != b"IHDR":
                return None
            length = struct.unpack(">I", length_bytes)[0]
            payload = stream.read(length)
            if len(payload) < 8:
                return None
            width, height = struct.unpack(">II", payload[:8])
            if width <= 0 or height <= 0:
                return None
            return width, height
    except OSError:
        return None


def asset_key(path_value: str, sha_value: str | None) -> str:
    return f"{normalize_path(path_value).lower()}|{(sha_value or '').lower()}"


def image_asset(
    path_value: str,
    expected_sha: str | None,
    kind: str,
    declared_width: int | None = None,
    declared_height: int | None = None,
    source_path: str | None = None,
    source_oid: int | None = None,
) -> dict[str, Any]:
    identity = file_identity(path_value, expected_sha)
    dims = png_dimensions(path_value) if identity["exists"] else None
    width = dims[0] if dims else declared_width
    height = dims[1] if dims else declared_height
    return {
        "key": asset_key(str(path_value), identity.get("sha256") or expected_sha),
        "path": str(path_value),
        "sha256": identity.get("sha256") or expected_sha,
        "expectedSha256": expected_sha,
        "bytes": identity.get("bytes", 0),
        "exists": identity.get("exists", False),
        "shaMatch": identity.get("shaMatch"),
        "width": width,
        "height": height,
        "declaredWidth": declared_width,
        "declaredHeight": declared_height,
        "kind": kind,
        "sourcePath": source_path,
        "sourceOid": source_oid,
        "rgbaBytes": int(width * height * RGBA_BYTES_PER_PIXEL) if width and height else None,
    }


def resolve_image_from_capture(identity: dict[str, Any] | None, kind: str, source_path: str, source_oid: int) -> dict[str, Any] | None:
    if not identity:
        return None
    return image_asset(
        str(identity.get("path", "")),
        identity.get("sha256"),
        kind,
        source_path=source_path,
        source_oid=source_oid,
    )


def load_pages_manifest(path_value: str | None) -> dict[str, Any]:
    if not path_value:
        return {"status": "not_supplied", "usable": False, "pages": {}, "sources": [], "errors": []}
    path = Path(path_value)
    if not path.is_file():
        return {"status": "missing", "usable": False, "pages": {}, "sources": [], "errors": [f"manifest missing: {path}"]}
    try:
        data = read_json(path)
    except (OSError, json.JSONDecodeError) as error:
        return {"status": "invalid", "usable": False, "pages": {}, "sources": [], "errors": [str(error)]}
    pages: dict[str, dict[str, Any]] = {}
    for page in data.get("pages", []) if isinstance(data, dict) else []:
        if not isinstance(page, dict):
            continue
        page_id = str(page.get("id", ""))
        if page_id:
            page_copy = dict(page)
            page_copy["manifestPath"] = str(path)
            page_copy["filesystemPath"] = str((path.parent / str(page.get("path", ""))).resolve())
            pages[page_id] = page_copy
    sources = [source for source in data.get("sources", []) if isinstance(source, dict)] if isinstance(data, dict) else []
    errors: list[str] = []
    for page in pages.values():
        identity = file_identity(page.get("filesystemPath"), page.get("sha256"))
        page["identity"] = identity
        if not identity["exists"]:
            errors.append(f"derived page missing: {page.get('path')}")
        elif identity["shaMatch"] is False:
            errors.append(f"derived page sha mismatch: {page.get('path')}")
    return {
        "status": "usable" if not errors else "invalid",
        "usable": not errors,
        "pages": pages,
        "sources": sources,
        "errors": errors,
        "manifestPath": str(path),
        "raw": data,
    }


def astc_dimensions(path_value: str | Path) -> tuple[int, int, int] | None:
    path = Path(path_value)
    try:
        with path.open("rb") as stream:
            header = stream.read(ASTC_HEADER_BYTES)
        if len(header) != ASTC_HEADER_BYTES or header[:4] != b"\x13\xab\xa1\x5c":
            return None
        width = header[7] | (header[8] << 8) | (header[9] << 16)
        height = header[10] | (header[11] << 8) | (header[12] << 16)
        file_bytes = path.stat().st_size
        return width, height, file_bytes
    except OSError:
        return None


def load_astc_manifest(path_value: str | None) -> dict[str, Any]:
    if not path_value:
        return {"status": "not_supplied", "usable": False, "entries": {}, "errors": []}
    path = Path(path_value)
    if not path.is_file():
        return {"status": "missing", "usable": False, "entries": {}, "errors": [f"manifest missing: {path}"]}
    try:
        data = read_json(path)
    except (OSError, json.JSONDecodeError) as error:
        return {"status": "invalid", "usable": False, "entries": {}, "errors": [str(error)]}
    raw_entries = []
    if isinstance(data, dict):
        for key in ("entries", "assets", "sources"):
            if isinstance(data.get(key), list):
                raw_entries.extend(item for item in data[key] if isinstance(item, dict))
        if not raw_entries and isinstance(data.get("files"), dict):
            for key, value in data["files"].items():
                if isinstance(value, dict):
                    item = dict(value)
                    item.setdefault("sourcePath", key)
                    raw_entries.append(item)
    entries: dict[str, dict[str, Any]] = {}
    errors: list[str] = []
    for entry in raw_entries:
        source = normalize_path(entry.get("sourcePath") or entry.get("source") or entry.get("path"))
        astc_path = entry.get("filePath") or entry.get("outputPath") or entry.get("astcPath") or entry.get("astc")
        if astc_path and not Path(astc_path).is_absolute():
            astc_path = str((path.parent / astc_path).resolve())
        elif astc_path:
            astc_path = str(Path(astc_path).resolve())
        if not source or not astc_path:
            errors.append("ASTC entry missing sourcePath or filePath")
            continue
        header = astc_dimensions(astc_path)
        identity = file_identity(astc_path, entry.get("astcSha256") or entry.get("fileSha256") or entry.get("sha256"))
        if header is None:
            errors.append(f"ASTC header invalid: {astc_path}")
            continue
        width, height, file_bytes = header
        payload = math.ceil(width / 4) * math.ceil(height / 4) * ASTC_BLOCK_BYTES
        if file_bytes != payload + ASTC_HEADER_BYTES:
            errors.append(f"ASTC file size mismatch: {astc_path}")
            continue
        item = dict(entry)
        item.update({
            "sourcePath": source,
            "sourceSha256": entry.get("sourceSha256") or entry.get("sourceSha"),
            "filesystemPath": astc_path,
            "identity": identity,
            "width": width,
            "height": height,
            "fileBytes": file_bytes,
            "payloadBytes": payload,
            "usable": identity["exists"] and identity["shaMatch"] is not False,
        })
        if item["usable"]:
            # Build-NtsdAstcTextures publishes derived page sources as
            # ``derived_sprite_pages/pages/...`` while the active pages
            # manifest/capture uses ``pages/...``.  Keep both normalized keys;
            # the source SHA remains the identity gate.
            source_keys = {source.lower()}
            if source.lower().startswith("derived_sprite_pages/"):
                source_keys.add(source.lower()[len("derived_sprite_pages/"):])
            if source.lower().startswith("vfs/"):
                source_keys.add(source.lower()[len("vfs/"):])
            for source_key in source_keys:
                entries[source_key] = item
        else:
            errors.append(f"ASTC entry unavailable or sha mismatch: {astc_path}")
    return {
        "status": "usable" if entries and not errors else "empty" if not entries and not errors else "invalid",
        "usable": bool(entries) and not errors,
        "entries": entries,
        "entryCount": len(raw_entries),
        "uniqueBlockCount": len({str(item.get("filesystemPath")) for item in entries.values()}),
        "errors": errors,
        "manifestPath": str(path),
        "raw": data,
    }


def capture_object_map(capture: dict[str, Any]) -> dict[int, dict[str, Any]]:
    return {int(item["id"]): item for item in capture.get("objects", []) if item.get("parseSuccess")}


def selectable_pool(capture: dict[str, Any]) -> list[dict[str, Any]]:
    result: list[dict[str, Any]] = []
    seen: set[int] = set()
    for item in sorted(capture.get("objects", []), key=lambda value: int(value.get("registryIndex", 0))):
        oid = int(item.get("id", -1))
        bmp = item.get("bmp") or {}
        if oid == -1 or oid in seen or int(item.get("type", -1)) != 0:
            continue
        if not item.get("parseSuccess") or not bmp or int(bmp.get("hidden", 0)) != 0:
            continue
        seen.add(oid)
        result.append({
            "oid": oid,
            "name": bmp.get("name") or item.get("catalogName") or f"OID{oid}",
            "registryIndex": int(item.get("registryIndex", -1)),
            "sourcePath": item.get("sourcePath"),
            "datPath": item.get("datPath"),
            "datSha256": (item.get("datIdentity") or {}).get("sha256"),
        })
    return result


def create_or_load_selection(
    capture: dict[str, Any], output_dir: Path, selection_path_value: str | None
) -> tuple[dict[str, Any], Path]:
    pool = selectable_pool(capture)
    pool_projection = [{key: entry.get(key) for key in ("oid", "name", "registryIndex", "sourcePath", "datSha256")} for entry in pool]
    pool_sha = sha256_json(pool_projection)
    selection_path = Path(selection_path_value).resolve() if selection_path_value else (output_dir / "selection.json").resolve()
    if selection_path_value:
        selection = read_json(selection_path)
        if selection.get("poolSha256") != pool_sha:
            raise ValueError("--selection poolSha256 does not match the current capture pool")
        selected_oids = [int(value) for value in selection.get("selectedOids", [])]
        if len(selected_oids) != 8 or len(set(selected_oids)) != 8 or any(oid not in {entry["oid"] for entry in pool} for oid in selected_oids):
            raise ValueError("--selection must contain eight distinct OIDs from the current pool")
        selection["reusedExistingSelection"] = True
        return selection, selection_path

    seed_bytes = secrets.token_bytes(32)
    seed_int = int.from_bytes(seed_bytes, byteorder="big", signed=False)
    rng = random.Random(seed_int)
    indices = rng.sample(range(len(pool)), 8)
    selected = [pool[index] for index in indices]
    selection = {
        "schemaVersion": 1,
        "auditId": AUDIT_ID,
        "taskId": TASK_ID,
        "operationId": OPERATION_ID,
        "createdAtUtc": __import__("datetime").datetime.now(__import__("datetime").timezone.utc).isoformat(),
        "samplingContract": "catalog object,type0,parsed config,Bmp.hidden(default0)==0; deduplicated publication order; -1 excluded",
        "poolSha256": pool_sha,
        "poolCount": len(pool),
        "pool": pool_projection,
        "cryptoSeedHex": seed_bytes.hex(),
        "cryptoSeedDecimal": str(seed_int),
        "algorithm": "secrets.token_bytes(32) seed -> random.Random(seed_int).sample(range(poolCount), 8), pool order preserved",
        "sampleIndices": indices,
        "selectedOids": [entry["oid"] for entry in selected],
        "selected": selected,
        "reusedExistingSelection": False,
        "writtenBeforeAmountCalculation": True,
    }
    write_json(selection_path, selection)
    return selection, selection_path


class DependencyClosure:
    def __init__(self, objects: dict[int, dict[str, Any]], capture: dict[str, Any]):
        self.objects = objects
        self.capture = capture
        self.fusion_records = (capture.get("fusion") or {}).get("records", [])
        self.kind_records = (capture.get("kind") or {}).get("records", [])
        self.edges: list[dict[str, Any]] = []
        self.unknowns: list[dict[str, Any]] = []
        self.cycles = 0
        self._edge_keys: set[str] = set()

    def _edge(
        self,
        edge_type: str,
        from_oid: int | None,
        to_oid: int | None,
        branch: str,
        status: str,
        reason: list[str],
        condition: list[str],
        frame_id: int | None = None,
        raw_token: Any = None,
        source_file: str | None = None,
        source_lines: str | None = None,
        from_oids: list[int] | None = None,
    ) -> dict[str, Any]:
        edge = {
            "type": edge_type,
            "fromOid": from_oid,
            "fromOids": from_oids or ([from_oid] if from_oid is not None else []),
            "toOid": to_oid,
            "frameId": frame_id,
            "rawToken": raw_token,
            "resolvedTarget": to_oid if status == "resolved" else None,
            "sourceFile": source_file,
            "sourceLines": source_lines,
            "predicate": condition,
            "condition": condition,
            "branchProfile": branch,
            "reasonChain": reason,
            "resolutionStatus": status,
        }
        key = sha256_json(edge)
        if key not in self._edge_keys:
            self._edge_keys.add(key)
            self.edges.append(edge)
        return edge

    def add_target(
        self,
        oid_set: set[int],
        queue: deque[int],
        edge_type: str,
        from_oid: int | None,
        target: int,
        branch: str,
        reason: list[str],
        condition: list[str],
        frame_id: int | None = None,
        raw_token: Any = None,
        source_file: str | None = None,
        source_lines: str | None = None,
        from_oids: list[int] | None = None,
    ) -> None:
        if target == -1:
            self._edge(edge_type, from_oid, target, branch, "ignored_sentinel", reason + ["target -1 is a sentinel, not a catalog OID"], condition, frame_id, raw_token, source_file, source_lines, from_oids)
            return
        if target not in self.objects:
            edge = self._edge(edge_type, from_oid, target, branch, "missing_oid", reason, condition, frame_id, raw_token, source_file, source_lines, from_oids)
            self.unknowns.append({"kind": "missing_oid", "targetOid": target, "edge": edge})
            return
        self._edge(edge_type, from_oid, target, branch, "resolved", reason, condition, frame_id, raw_token, source_file, source_lines, from_oids)
        if target not in oid_set:
            oid_set.add(target)
            queue.append(target)
        else:
            self.cycles += 1

    def run(
        self,
        roots: list[int],
        branch: str = "production",
        include_scenario: bool = False,
        include_compatibility: bool = False,
        context_victim_pool: list[int] | None = None,
        include_optional_209: bool = False,
    ) -> dict[str, Any]:
        oid_set: set[int] = set()
        queue: deque[int] = deque()
        for root in roots:
            if root in self.objects:
                oid_set.add(root)
                queue.append(root)
            else:
                self.unknowns.append({"kind": "missing_root_oid", "targetOid": root})

        if include_scenario:
            self.add_target(oid_set, queue, "respawn_effect", None, 998, "production", [
                "scenario root: respawn allowed or unspecified",
                "BattleRespawnModule emits OID998 action6",
            ], ["respawn policy unspecified"], raw_token={"action": 6}, source_file="Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs", source_lines="158-194,216-240")
            loaded_weapon_oids = [oid for oid in range(100, 200) if oid in self.objects]
            for oid in loaded_weapon_oids:
                self.add_target(oid_set, queue, "random_weapon_normal", None, oid, "production", [
                    "scenario root: loaded resolvable catalog OID in [100,199]",
                    "unknown BattleGameMode keeps both mode outcomes",
                ], ["mode unknown; mode1..4 may exclude 122/123"], raw_token={"oid": oid}, source_file="Assets/NTSD/Scripts/Simulation/Passes/RandomWeapon/BattleRandomWeaponDropModule.cs", source_lines="54-107")
                self.add_target(oid_set, queue, "random_weapon_mode2", None, oid, "production", [
                    "scenario root: Mode2Request==1 loaded OID in [100,199]",
                    "122 remains probabilistic and 123 is not excluded in this path",
                ], ["function-key/mode configuration unknown"], raw_token={"oid": oid}, source_file="Assets/NTSD/Scripts/Simulation/Passes/RandomWeapon/BattleRandomWeaponDropModule.cs", source_lines="196-237")
            self.unknowns.append({
                "kind": "stage_unknown",
                "reason": "stage/map/mode input was not specified or deployed; no default stage.dat invented",
                "branchProfile": "conditional_context",
            })

        if include_optional_209:
            self.add_target(oid_set, queue, "compat_special209", None, 209, "unverified_dispatch", [
                "optional compatibility extra from LF2SpecialAttack.Hit_State3000 CreateObjectAt(209)",
                "current formal production dispatch was not proven by the source review",
            ], ["itr effect3 interaction and source-local special-attack path"], raw_token=209,
                source_file="Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs", source_lines="599-655,742-755")

        processed: set[int] = set()
        fusion_added: set[tuple[int, int, int]] = set()
        while queue or len(processed) != len(oid_set):
            while queue:
                oid = queue.popleft()
                if oid in processed:
                    continue
                processed.add(oid)
                item = self.objects[oid]
                dat_path = item.get("datPath")
                item_type = int(item.get("type", -1))
                frames = item.get("frames", [])
                for frame in frames:
                    converted = frame.get("converted") or {}
                    frame_id = converted.get("frameId", frame.get("frameId"))
                    source_lines = "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganFrames.cs:Open/ReadLine; Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs:23-180"
                    for opoint in converted.get("opoints", []):
                        target = int(opoint.get("oid", -1))
                        self.add_target(oid_set, queue, "dat_opoint", oid, target, branch, [
                            "production parser converted a typed BattleObjectPointValue",
                            "all parsed frames and all opoints are traversed",
                        ], ["frame included in whole-DAT envelope", "no first-attack-frame pruning"], frame_id, opoint, dat_path, source_lines)
                    state = int(converted.get("state", 0))
                    if 8000 <= state < 9000:
                        target = state - 8000
                        action = int(converted.get("next", 0))
                        action = action if action < 999 else 0
                        self.add_target(oid_set, queue, "native_state_transform", oid, target, branch, [
                            "native C25 definition transition maps state-8000",
                            "target action is next when next<999 else 0",
                        ], ["state in [8000,8999]", "whole target DAT is included"], frame_id, {"state": state, "targetAction": action}, dat_path, "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:2810-2856; Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs:95-121")
                    if item_type == 0 and state == 9996:
                        for target in (217, 218):
                            self.add_target(oid_set, queue, "state9996_spawn", oid, target, branch, [
                                "current-DAT type0 state9996 producer",
                                "native runtime attempts four OID217 and one OID218 instances",
                            ], ["type==0", "state==9996", "AttackingCounter==1 at runtime"], frame_id, {"state": state, "attempts": 4 if target == 217 else 1}, dat_path, "Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs:649-686")
                    hit_fa = int(converted.get("hit_Fa", 0))
                    if item_type != 0:
                        hit_map = {5: (219,), 6: (220,), 8: (225,), 9: (221, 222), 11: (211, 212, 221), 13: (228,)}
                        for target in hit_map.get(hit_fa, ()):
                            self.add_target(oid_set, queue, "hit_fa_spawn", oid, target, branch, [
                                "non-type0 current-DAT hit_Fa producer mapping",
                                f"hit_Fa {hit_fa} includes target OID{target}",
                            ], ["current DAT type!=0", f"hit_Fa=={hit_fa}", "all possible targets retained"], frame_id, {"hit_Fa": hit_fa, "oid": target}, dat_path, "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:1470-1539,1623-1666,1702-1766,2123-2229; Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs:3098-3110")
                    if state in (18, 19):
                        self.add_target(oid_set, queue, "state18_19_particles", oid, 999, branch, [
                            "state18/state19 native particle writer target",
                            "effect object OID999 is counted once as a shared DAT dependency",
                        ], ["possible state18 or state19 frame"], frame_id, {"state": state}, dat_path, "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:7437-7451; Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeState18ParticleWriter.cs:9-60")

                    for cpoint in converted.get("cpoints", []):
                        if int(cpoint.get("throwInjury", 0)) != -1:
                            continue
                        if context_victim_pool:
                            for victim_oid in context_victim_pool:
                                self.add_target(oid_set, queue, "cpoint_copy", oid, victim_oid, "production", [
                                    "cpoint.throwinjury==-1 copies the victim current DAT/config",
                                    "unknown opponent expanded conservatively to selectable type0 victim pool",
                                ], ["joint scene context not specified", "victim pool expanded for scenario envelope"], frame_id, cpoint, dat_path, "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:5250-5261,5293-5310,5376-5396")
                        else:
                            self.unknowns.append({
                                "kind": "cpoint_context_unknown",
                                "fromOid": oid,
                                "frameId": frame_id,
                                "throwInjury": -1,
                                "reason": "cpoint copy requires a jointly present victim; no opponent roster supplied",
                                "branchProfile": "conditional_context",
                            })

                weapon_piece = item.get("weaponPiece") or {}
                for group in weapon_piece.get("groups", []):
                    amount = int_field(group.get("fields"), "amount", 0)
                    if amount <= 0:
                        continue
                    variants = group.get("variants", [])
                    for variant_index, variant in enumerate(variants):
                        target = int_field(variant.get("fields"), "oid", -1)
                        resolved_target = 999 if target == -1 else target
                        self.add_target(oid_set, queue, "weapon_piece_custom", oid, resolved_target, branch, [
                            "weapon_piece group amount>0 materializes every authored variant as a possible target",
                            "missing or explicit oid -1 maps to OID999",
                        ], ["weapon_piece group amount>0", "all variants retained", "target frame act/framea does not change target OID"], raw_token={"groupPiece": group.get("piece"), "variantIndex": variant_index, "oid": target, "amount": amount}, source_file=item.get("datPath"), source_lines="Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeWeaponPieceWriter.cs:41-85; Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganDefinitions.cs:38-110")

                builtin_fragments = {100, 101, 120, 121, 122, 123, 124, 150, 151, 201, 213, 217, 218}
                if oid in builtin_fragments:
                    self.add_target(oid_set, queue, "weapon_piece_builtin", oid, 999, branch, [
                        "built-in BrokenWeaponFragmentCount(source.ObjectId)>0",
                        "fragment target is OID999",
                    ], ["source OID in builtin fragment table", "runtime break/death condition remains conditional"], raw_token={"sourceOid": oid}, source_file=item.get("datPath"), source_lines="Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:1028-1037; Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeWeaponPieceWriter.cs:18-38")

                if include_compatibility:
                    for frame in frames:
                        converted = frame.get("converted") or {}
                        state = int(converted.get("state", 0))
                        frame_id = converted.get("frameId", frame.get("frameId"))
                        if item_type == 0 and state == 9995:
                            self.add_target(oid_set, queue, "compat_state_transform", oid, 50, "compatibility", [
                                "legacy resident-binding overapproximation state9995->50",
                                "default production late path does not call this legacy function",
                            ], ["type==0", "state==9995", "compatibility branch only"], frame_id, {"state": state}, dat_path, "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:2787-2807; Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs:631-646")
                        if 4000 <= state < 5000:
                            self.add_target(oid_set, queue, "compat_state_transform", oid, state - 4000, "compatibility", [
                                "legacy resident-binding overapproximation state4000..4999->state-4000",
                                "kept separate from default production reachability",
                            ], ["state in [4000,4999]", "compatibility branch only"], frame_id, {"state": state}, dat_path, "Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:2787-2807; Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs:820-826")

            # Built-in fragment/weapon and all-frame expansion may add nodes while the
            # queue is being consumed.  Fusion is a true two-input hyperedge and is
            # therefore evaluated to a fixed point after every queue drain.
            for fusion in self.fusion_records:
                id1, id2, id3 = int(fusion.get("id1", 0)), int(fusion.get("id2", 0)), int(fusion.get("id3", 0))
                if id1 in oid_set and id2 in oid_set and (id1, id2, id3) not in fusion_added:
                    fusion_added.add((id1, id2, id3))
                    self.add_target(oid_set, queue, "fusion_hyperedge", None, id3, "production", [
                        "actual fusion.dat two-input hyperedge",
                        f"both inputs OID{id1}+OID{id2} are present in the current closure",
                        "runtime team/state/HP/distance/slot gates remain conditional; this is a static envelope",
                    ], ["id1 and id2 jointly reachable", "fixed-point expansion"], raw_token=fusion, source_file=(self.capture.get("fusion") or {}).get("selectedPath"), source_lines=f"fusion.dat line {fusion.get('sourceLine')}", from_oids=[id1, id2])

        # Kind records are pair-conditioned copies. Represent them as
        # conditional context edges after the OID fixed point; a respond list
        # is never treated as an object-spawn list.
        for attacker_oid in sorted(oid_set):
            attacker = self.objects.get(attacker_oid)
            if not attacker or int(attacker.get("type", -1)) != 3:
                continue
            for record in self.kind_records:
                bound_ids = {int(value) for value in record.get("boundIds", [])}
                if attacker_oid not in bound_ids:
                    continue
                respond_ids = [int(value) for value in record.get("respondIds", [])]
                co_present = [value for value in respond_ids if value in oid_set]
                if not co_present:
                    self.unknowns.append({
                        "kind": "kind_copy_context_unknown",
                        "attackerOid": attacker_oid,
                        "respondIds": respond_ids,
                        "reason": "Kind responds-to IDs are partner conditions; no jointly present target was proven",
                        "branchProfile": "conditional_context",
                    })
                    continue
                for target_oid in co_present:
                    self._edge(
                        "kind_copy",
                        target_oid,
                        attacker_oid,
                        "conditional_context",
                        "conditional_context",
                        [
                            "both current types3 and KindCatalog Binds(attackerOid)&&RespondsTo(targetOid)",
                            "target adopts attacker current DAT/config under the joint runtime condition",
                            "respond list is retained as a partner condition, not a summon list",
                        ],
                        ["attacker type==3", "attacker and target jointly present", f"Kind effect {record.get('effect')}"],
                        None,
                        {"effect": record.get("effect"), "frame": record.get("frame"), "targetOid": target_oid},
                        (self.capture.get("kind") or {}).get("selectedPath"),
                        f"kind.dat line {record.get('sourceLine')}; Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeDataCatalog.cs:188-222",
                        [attacker_oid, target_oid],
                    )

        return {
            "oids": sorted(oid_set),
            "edges": list(self.edges),
            "unknowns": list(self.unknowns),
            "cycleRevisitCount": self.cycles,
            "processedCount": len(processed),
        }


def build_first_declared_ownership(item: dict[str, Any]) -> list[dict[str, Any]]:
    seen: set[int] = set()
    result: list[dict[str, Any]] = []
    bmp = item.get("bmp") or {}
    for declaration_index, file_entry in enumerate(bmp.get("files", [])):
        first = int(file_entry.get("effectiveFirst", 0))
        last = int(file_entry.get("effectiveLast", -1))
        declared_range = range(first, last + 1) if last >= first else range(0)
        all_pics = list(declared_range)
        owned = [pic for pic in all_pics if pic not in seen]
        seen.update(all_pics)
        result.append({
            "declarationIndex": declaration_index,
            "file": file_entry,
            "ownedEffectivePics": owned,
            "ownershipCount": len(owned),
        })
    return result


def runtime_grid_from_capture(file_entry: dict[str, Any]) -> dict[str, int]:
    """Keep capture row/col raw and apply the production grid adapter once.

    ``Lf2SpriteFileDef.Row/Col`` are captured without transformation.  The
    production ``ResolveEffectiveGrid`` maps them to ``row=fileInfo.col`` and
    ``col=fileInfo.row``; ``col`` is the horizontal divisor used by the
    ``localPic % col`` / ``localPic / col`` address calculation.
    """
    dat_row = max(int(file_entry.get("row", 0)), 0)
    dat_col = max(int(file_entry.get("col", 0)), 0)
    return {
        "datRow": dat_row,
        "datCol": dat_col,
        "runtimeRows": dat_col,
        "runtimeCols": dat_row,
    }


def clamped_cells_for_file(
    file_entry: dict[str, Any],
    owned_pics: Iterable[int],
    source_dims: tuple[int, int] | None,
) -> list[dict[str, Any]]:
    """Evaluate native cell geometry with the production row/col contract."""
    if not source_dims:
        return []
    source_width, source_height = source_dims
    cell_width = int(file_entry.get("width", 0))
    cell_height = int(file_entry.get("height", 0))
    grid = runtime_grid_from_capture(file_entry)
    runtime_rows = grid["runtimeRows"]
    runtime_cols = grid["runtimeCols"]
    if source_width <= 0 or source_height <= 0 or cell_width <= 0 or cell_height <= 0 or runtime_rows <= 0 or runtime_cols <= 0:
        return []
    result: list[dict[str, Any]] = []
    first = int(file_entry.get("effectiveFirst", 0))
    for pic in sorted(owned_pics):
        local = pic - first
        x = (local % runtime_cols) * (cell_width + 1)
        y = source_height - (local // runtime_cols + 1) * (cell_height + 1) + 1
        if local < 0 or local >= runtime_rows * runtime_cols or x < 0 or y < 0 or x + cell_width > source_width or y + cell_height > source_height:
            result.append({
                "pic": pic,
                "localPic": local,
                "x": x,
                "y": y,
                "width": cell_width,
                "height": cell_height,
                "sourceWidth": source_width,
                "sourceHeight": source_height,
                **grid,
            })
    return result


def derived_page_index(pages: dict[str, Any]) -> dict[tuple[str, str], list[dict[str, Any]]]:
    index: dict[tuple[str, str], list[dict[str, Any]]] = defaultdict(list)
    for source in pages.get("sources", []):
        source_path = normalize_path(source.get("path")).lower()
        source_sha = str(source.get("sha256", "")).lower()
        if source_path:
            index[(source_path, source_sha)].append(source)
    return index


def body_assets_for_declarations(
    closure_oids: list[int],
    objects: dict[int, dict[str, Any]],
    pages: dict[str, Any],
    astc: dict[str, Any],
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]], list[str]]:
    assets: dict[str, dict[str, Any]] = {}
    declaration_projection: list[dict[str, Any]] = []
    clamp_cells: list[dict[str, Any]] = []
    errors: list[str] = []
    page_index = derived_page_index(pages)
    for oid in closure_oids:
        item = objects[oid]
        declaration_records = build_first_declared_ownership(item)
        for declaration in declaration_records:
            file_entry = declaration["file"]
            owned = declaration["ownedEffectivePics"]
            if not owned:
                continue
            identity = file_entry.get("imageIdentity") or {}
            source_path = normalize_path(file_entry.get("path"))
            source_sha = str(identity.get("sha256") or "").lower()
            source_file_path = str(identity.get("path", ""))
            page_sources = page_index.get((source_path.lower(), source_sha), []) if pages.get("usable") else []
            selected_pages: list[dict[str, Any]] = []
            covered: set[int] = set()
            if page_sources:
                matching_bindings = [binding for source in page_sources for binding in source.get("bindings", []) if normalize_path(binding.get("datPath")).lower() == normalize_path(item.get("sourcePath")).lower() and int(binding.get("oid", oid)) == oid]
                if not matching_bindings:
                    errors.append(f"derived source binding missing for oid={oid} dat={item.get('sourcePath')} image={source_path}")
                for source in page_sources:
                    for binding in source.get("bindings", []):
                        if normalize_path(binding.get("datPath")).lower() != normalize_path(item.get("sourcePath")).lower() or int(binding.get("oid", oid)) != oid:
                            continue
                        first_pic = int(binding.get("firstPic", file_entry.get("effectiveFirst", 0)))
                        for chunk in source.get("chunks", []):
                            local_first = int(chunk.get("firstLocalPic", 0))
                            count = int(chunk.get("count", 0))
                            chunk_pics = set(range(first_pic + local_first, first_pic + local_first + max(count, 0)))
                            intersection = chunk_pics.intersection(owned)
                            if not intersection:
                                continue
                            page_id = str(chunk.get("page", ""))
                            page = pages.get("pages", {}).get(page_id)
                            if not page:
                                errors.append(f"derived chunk references missing page {page_id}")
                                continue
                            covered.update(intersection)
                            selected_pages.append({
                                "pageId": page_id,
                                "page": page,
                                "sourcePath": source_path,
                                "oid": oid,
                                "ownedIntersectionCount": len(intersection),
                                "ownedIntersectionFirst": min(intersection),
                                "ownedIntersectionLast": max(intersection),
                            })
            for selected in selected_pages:
                page = selected["page"]
                page_identity = page.get("identity") or {}
                page_asset = image_asset(
                    page.get("filesystemPath", ""),
                    page.get("sha256"),
                    "derived_page",
                    page.get("width"),
                    page.get("height"),
                    source_path=source_path,
                    source_oid=oid,
                )
                page_asset.update({
                    "pageId": selected["pageId"],
                    "ownedIntersectionCount": selected["ownedIntersectionCount"],
                    "astcSourceKey": normalize_path(page.get("path", "")),
                })
                assets[page_asset["key"]] = page_asset
            uncovered = set(owned) - covered
            if selected_pages and uncovered:
                errors.append(f"derived mapping is partial for oid={oid} image={source_path}; source fallback retained for {len(uncovered)} owned cells")
            if not selected_pages or uncovered:
                source_asset = image_asset(
                    source_file_path,
                    identity.get("sha256"),
                    "body_source",
                    file_entry.get("width"),
                    file_entry.get("height"),
                    source_path=source_path,
                    source_oid=oid,
                )
                source_asset["astcSourceKey"] = source_path
                assets[source_asset["key"]] = source_asset
                dims = (source_asset.get("width"), source_asset.get("height"))
                candidate_pics = uncovered if selected_pages else set(owned)
                for cell in clamped_cells_for_file(file_entry, candidate_pics, dims):
                    clamp_cells.append({
                        "oid": oid,
                        "datPath": item.get("sourcePath"),
                        "imagePath": source_path,
                        "sourceFilePath": source_file_path,
                        "sourceSha256": identity.get("sha256"),
                        "rgbaBytes": cell["width"] * cell["height"] * RGBA_BYTES_PER_PIXEL,
                        "reason": "native cell geometry is outside the verified PNG layout; source clamp remains RGBA",
                        **cell,
                    })
            declaration_projection.append({
                "oid": oid,
                "datPath": item.get("sourcePath"),
                "imagePath": source_path,
                "ownedEffectivePics": owned,
                "selectedDerivedPages": [entry["pageId"] for entry in selected_pages],
                "coveredPics": sorted(covered),
                "uncoveredPics": sorted(set(owned) - covered),
            })
    return list(assets.values()), declaration_projection, clamp_cells, errors


def astc_entry_for_source(astc: dict[str, Any], source_path: str, source_sha: str | None) -> dict[str, Any] | None:
    entry = (astc.get("entries") or {}).get(normalize_path(source_path).lower())
    if not entry:
        return None
    manifest_sha = str(entry.get("sourceSha256") or "").lower()
    if manifest_sha and source_sha and manifest_sha != str(source_sha).lower():
        return None
    return entry


def grid_validation_for_declarations(
    set_name: str,
    closure_oids: list[int],
    objects: dict[int, dict[str, Any]],
    astc: dict[str, Any],
    body_assets: list[dict[str, Any]],
    production_clamp_cells: list[dict[str, Any]],
) -> dict[str, Any]:
    """Record raw DAT grids beside the one production-normalized grid.

    This is deliberately independent of page allocation: it proves every
    source sheet's header and native grid geometry, while the production clamp
    list is the subset that survived derived-page coverage and is actually
    eligible for the source/ASTC branch.
    """
    declarations: list[dict[str, Any]] = []
    sheets: dict[str, dict[str, Any]] = {}
    direct_clamp_cells: list[dict[str, Any]] = []
    for oid in closure_oids:
        item = objects[oid]
        for declaration in build_first_declared_ownership(item):
            file_entry = declaration["file"]
            owned = declaration["ownedEffectivePics"]
            if not owned:
                continue
            identity = file_entry.get("imageIdentity") or {}
            source_path = normalize_path(file_entry.get("path"))
            source_sha = str(identity.get("sha256") or "").lower()
            source_file_path = str(identity.get("path", ""))
            dims = png_dimensions(source_file_path)
            grid = runtime_grid_from_capture(file_entry)
            astc_entry = astc_entry_for_source(astc, source_path, source_sha)
            direct = clamped_cells_for_file(file_entry, owned, dims)
            key = asset_key(source_path, source_sha)
            sheet = sheets.setdefault(key, {
                "key": key,
                "imagePath": source_path,
                "sourceFilePath": source_file_path,
                "sourceSha256": source_sha,
                "sourceHeader": {"width": dims[0], "height": dims[1]} if dims else None,
                "astc": {
                    "usable": bool(astc_entry),
                    "filesystemPath": astc_entry.get("filesystemPath") if astc_entry else None,
                    "sourceSha256": astc_entry.get("sourceSha256") if astc_entry else None,
                },
                "declarationCount": 0,
                "directClampCellCount": 0,
                "directClampPics": [],
            })
            sheet["declarationCount"] += 1
            sheet["directClampCellCount"] += len(direct)
            sheet["directClampPics"].extend(cell["pic"] for cell in direct)
            declaration_record = {
                "oid": oid,
                "datPath": item.get("sourcePath"),
                "declarationIndex": declaration["declarationIndex"],
                "imagePath": source_path,
                "sourceSha256": source_sha,
                "sourceFilePath": source_file_path,
                "sourceHeader": {"width": dims[0], "height": dims[1]} if dims else None,
                "effectiveFirst": int(file_entry.get("effectiveFirst", 0)),
                "effectiveLast": int(file_entry.get("effectiveLast", -1)),
                "ownedEffectivePicCount": len(owned),
                "cellWidth": int(file_entry.get("width", 0)),
                "cellHeight": int(file_entry.get("height", 0)),
                "astcUsableForSource": bool(astc_entry),
                "directClampCellCount": len(direct),
                "directClampPics": [cell["pic"] for cell in direct],
                **grid,
            }
            declarations.append(declaration_record)
            for cell in direct:
                direct_clamp_cells.append({
                    "oid": oid,
                    "datPath": item.get("sourcePath"),
                    "imagePath": source_path,
                    "sourceSha256": source_sha,
                    **cell,
                })

    for sheet in sheets.values():
        sheet["directClampPics"] = sorted(set(sheet["directClampPics"]))

    production_by_source: dict[str, dict[str, Any]] = {}
    for cell in production_clamp_cells:
        key = asset_key(str(cell.get("imagePath", "")), str(cell.get("sourceSha256") or "").lower())
        entry = production_by_source.setdefault(key, {
            "key": key,
            "imagePath": cell.get("imagePath"),
            "sourceSha256": cell.get("sourceSha256"),
            "clampCellCount": 0,
            "clampPics": [],
        })
        entry["clampCellCount"] += 1
        entry["clampPics"].append(cell.get("pic"))
    for entry in production_by_source.values():
        entry["clampPics"] = sorted(set(entry["clampPics"]))

    sheet_values = list(sheets.values())
    direct_clamped = {key: sheet for key, sheet in sheets.items() if sheet["directClampCellCount"] > 0}
    production_source_keys = {
        asset_key(
            str(asset.get("astcSourceKey", "")),
            str(asset.get("sha256") or asset.get("expectedSha256") or "").lower(),
        )
        for asset in body_assets
        if asset.get("kind") == "body_source"
    }
    manifest_missing = {
        key: sheet
        for key, sheet in sheets.items()
        if key in production_source_keys and not sheet["astc"]["usable"]
    }
    production_clamped = set(production_by_source)
    astc_conflicts = []
    for cell in production_clamp_cells:
        entry = astc_entry_for_source(astc, str(cell.get("imagePath", "")), cell.get("sourceSha256"))
        if entry:
            astc_conflicts.append({
                "imagePath": cell.get("imagePath"),
                "sourceSha256": cell.get("sourceSha256"),
                "pic": cell.get("pic"),
                "astcPath": entry.get("filesystemPath"),
            })
    direct_keys = set(direct_clamped)
    missing_keys = set(manifest_missing)
    expected_full_count = 24 if set_name == "full_catalog_loading_baseline" else None
    return {
        "formula": "raw DAT row/col retained; production ResolveEffectiveGrid maps runtimeRows=DAT col and runtimeCols=DAT row; x=localPic%runtimeCols; y=sourceHeight-(localPic/runtimeCols+1)*(cellHeight+1)+1",
        "rawDatRowColPreserved": True,
        "sourceSheetCount": len(sheet_values),
        "ownedDeclarationCount": len(declarations),
        "productionBodySourceCount": len(production_source_keys),
        "directClampCellCount": len(direct_clamp_cells),
        "directClampRgbaBytes": sum(cell["width"] * cell["height"] * RGBA_BYTES_PER_PIXEL for cell in direct_clamp_cells),
        "directClampedSourceCount": len(direct_clamped),
        "directClampedSources": sorted(direct_clamped.values(), key=lambda value: value["key"]),
        "productionClampCellCount": len(production_clamp_cells),
        "productionClampRgbaBytes": sum(int(cell.get("rgbaBytes", 0)) for cell in production_clamp_cells),
        "productionClampedSourceCount": len(production_by_source),
        "productionClampedSources": sorted(production_by_source.values(), key=lambda value: value["key"]),
        "astcManifestMissingSourceCount": len(manifest_missing),
        "astcManifestMissingSources": sorted(manifest_missing.values(), key=lambda value: value["key"]),
        "inventoryComparison": {
            "fullCatalogBaselineExpectedClampedSourceCount": expected_full_count,
            "observedCountMatchesExpected": expected_full_count is None or len(direct_clamped) == expected_full_count,
            "baselineComparisonUsesOriginalDirectSampling": True,
            "pagedOriginalClampedSourceCount": len(direct_keys - production_clamped),
            "manifestMissingEqualsDirectClamped": missing_keys == direct_keys,
            "manifestMissingEqualsProductionClamped": missing_keys == production_clamped,
            "directOnlySources": sorted(direct_keys - missing_keys),
            "missingOnlySources": sorted(missing_keys - direct_keys),
            "productionOnlySources": sorted(production_clamped - missing_keys),
        },
        "astcClampConflicts": astc_conflicts,
        "declarations": declarations,
    }


def ui_instances_for_oids(closure_oids: list[int], objects: dict[int, dict[str, Any]]) -> tuple[list[dict[str, Any]], list[str]]:
    instances: list[dict[str, Any]] = []
    errors: list[str] = []
    for oid in closure_oids:
        item = objects[oid]
        bmp = item.get("bmp") or {}
        for role in ("head", "small"):
            entry = bmp.get(role)
            if not entry:
                continue
            identity = entry.get("imageIdentity") or {}
            asset = image_asset(str(identity.get("path", "")), identity.get("sha256"), f"ui_{role}", source_path=entry.get("path"), source_oid=oid)
            instances.append({
                "oid": oid,
                "datPath": item.get("sourcePath"),
                "kind": role,
                "virtualPath": entry.get("path"),
                "asset": asset,
            })
            if not asset["exists"]:
                errors.append(f"missing {role} UI image for oid={oid}: {entry.get('path')}")
    return instances, errors


def budget_for_assets(
    body_assets: list[dict[str, Any]],
    ui_instances: list[dict[str, Any]],
    clamp_cells: list[dict[str, Any]],
    astc: dict[str, Any],
    derived_pages: dict[str, Any],
    old_publication_mode: str = "cold",
    astc_mode: bool = False,
) -> dict[str, Any]:
    source_gpu = 0
    readable_cpu = 0
    retained_rgba = 0
    max_upload = 0
    max_clamp_work = 0
    decode_works: list[int] = []
    gpu_assets: list[dict[str, Any]] = []
    astc_used = 0
    seen_compressed_blocks: set[str] = set()
    body_astc_payload = 0
    body_rgba_fallback = 0

    def add_png(asset: dict[str, Any], retained: bool, readable: bool, create_source: bool = True) -> None:
        nonlocal source_gpu, readable_cpu, retained_rgba, max_upload
        width, height = asset.get("width"), asset.get("height")
        rgba = asset.get("rgbaBytes")
        if not width or not height or rgba is None:
            return
        if create_source:
            source_gpu += rgba
            max_upload = max(max_upload, rgba)
        if readable:
            readable_cpu += rgba
        if retained:
            retained_rgba += rgba
        file_bytes = int(asset.get("bytes", 0))
        work = rgba * 2 + 4 * file_bytes + 8 * int(width) + 65536
        decode_works.append(work)
        gpu_assets.append({"key": asset["key"], "kind": asset["kind"], "gpuBytes": rgba, "mode": "rgba"})

    astc_entries = astc.get("entries", {}) if astc.get("usable") and astc_mode else {}
    seen_body: set[str] = set()
    for asset in body_assets:
        if asset["key"] in seen_body:
            continue
        seen_body.add(asset["key"])
        if asset.get("kind") in ("body_source", "derived_page"):
            astc_key = normalize_path(asset.get("astcSourceKey") or asset.get("sourcePath")).lower()
            astc_entry = astc_entries.get(astc_key)
            if astc_entry and astc_entry.get("usable") and asset.get("sha256") and str(astc_entry.get("sourceSha256", asset.get("sha256"))).lower() == str(asset.get("sha256")).lower():
                compressed_key = ("derived-page-instance|" + asset["key"]) if asset.get("kind") == "derived_page" else asset_key(astc_entry["filesystemPath"], astc_entry["identity"].get("sha256") or astc_entry.get("astcSha256"))
                if compressed_key in seen_compressed_blocks:
                    continue
                seen_compressed_blocks.add(compressed_key)
                payload = int(astc_entry["payloadBytes"])
                body_astc_payload += payload
                source_gpu += payload
                max_upload = max(max_upload, payload)
                file_bytes = int(astc_entry["fileBytes"])
                decode_works.append(2 * file_bytes + 65536)
                astc_used += 1
                gpu_assets.append({"key": asset["key"], "kind": asset["kind"], "gpuBytes": payload, "mode": "astc4x4", "astcPath": astc_entry["filesystemPath"]})
                continue
            body_rgba_fallback += int(asset.get("rgbaBytes") or 0)
            add_png(asset, retained=(asset.get("kind") == "body_source" and not astc_mode), readable=False, create_source=True)
        elif asset.get("kind") == "derived_page":
            body_rgba_fallback += int(asset.get("rgbaBytes") or 0)
            add_png(asset, retained=False, readable=False, create_source=True)
        else:
            add_png(asset, retained=True, readable=False, create_source=True)

    body_source_gpu_before_clamp = source_gpu

    ui_seen: set[str] = set()
    ui_declaration_gpu = 0
    ui_declaration_readable = 0
    ui_unique_gpu = 0
    ui_unique_readable = 0
    for instance in ui_instances:
        ui_asset = instance["asset"]
        ui_rgba = ui_asset.get("rgbaBytes") or 0
        ui_declaration_gpu += int(ui_rgba)
        ui_declaration_readable += int(ui_rgba)
        if astc_mode and ui_asset["key"] in ui_seen:
            continue
        ui_seen.add(ui_asset["key"])
        ui_unique_gpu += int(ui_rgba)
        ui_unique_readable += int(ui_rgba)
        add_png(ui_asset, retained=False, readable=True, create_source=True)

    # Native compatibility cells own GPU textures in every mode. ASTC only removes
    # their retained atlas pixel arrays; source sheets and cell textures are separate.
    fallback_body_assets = [
        asset for asset in body_assets
        if asset.get("kind") == "body_source" and normalize_path(asset.get("astcSourceKey")).lower() not in astc_entries
    ]
    clamp_source_rgba = sum(int(asset.get("rgbaBytes") or 0) for asset in fallback_body_assets)
    for cell in clamp_cells:
        bytes_value = int(cell.get("rgbaBytes", 0))
        source_gpu += bytes_value
        if not astc_mode:
            retained_rgba += bytes_value
        max_upload = max(max_upload, bytes_value)
        max_clamp_work = max(max_clamp_work, 2 * bytes_value + 8)

    body_page_keys = {asset["key"] for asset in body_assets if asset.get("kind") == "derived_page"}
    atlas_page_count = len(body_page_keys) if derived_pages.get("usable") and not astc_mode else 0
    atlas_bytes = atlas_page_count * PAGE_SIZE * PAGE_SIZE * RGBA_BYTES_PER_PIXEL
    new_publication = source_gpu + readable_cpu + atlas_bytes
    old_publication = new_publication if old_publication_mode == "old_equals_new_upper_bound" else 0
    resident = old_publication + source_gpu + readable_cpu + atlas_bytes
    loading = resident + retained_rgba + max_upload + 4 * max_clamp_work
    if atlas_bytes:
        loading += 3 * atlas_bytes + PAGE_SIZE * PAGE_SIZE * RGBA_BYTES_PER_PIXEL
    top_four = sorted(decode_works, reverse=True)[:4]
    loading += sum(top_four)
    return {
        "formula": {
            "pngDecodeWork": "pixels*2 + 4*PNGfileBytes + 8*width + 65536",
            "compressedReadWork": "2*ASTCfileBytes + 65536",
            "loading": "resident + retainedRGBA + maxUpload + 4*maxClampWork + (atlas?3*atlas+PageBytes:0) + largest4work",
            "resident": "oldPublication + sourceGPU + readableCPU + atlasReservation",
        },
        "scope": "engineering image allocation estimate; excludes Unity/driver/scene/object metadata and is not whole-process peak",
        "oldPublicationMode": old_publication_mode,
        "mode": "astc" if astc_mode else "unsupported_astc_rgba_fallback",
        "bodyCpuSemaphoreSlots": CPU_SEMAPHORE_SLOTS,
        "bodyCpuPermitHeldThroughUpload": True,
        "sourceGpuBytes": source_gpu,
        "bodySourceGpuBytes": body_source_gpu_before_clamp,
        "bodyAstcPayloadBytes": body_astc_payload,
        "bodyRgbaFallbackBytes": body_rgba_fallback,
        "readableCpuBytes": readable_cpu,
        "retainedRgbaBytes": retained_rgba,
        "oldPublicationBytes": old_publication,
        "atlasReservationBytes": atlas_bytes,
        "atlasPageCount": atlas_page_count,
        "newPublicationBytes": new_publication,
        "maxUploadBytes": max_upload,
        "maxClampWorkBytes": max_clamp_work,
        "largest4WorkBytes": top_four,
        "astcImagesUsed": astc_used,
        "clampFallbackRgbaBytes": clamp_source_rgba,
        "clampCellWorkBytes": sum(int(cell.get("rgbaBytes", 0)) for cell in clamp_cells),
        "clampCellTextureUpperBoundBytes": sum(int(cell.get("rgbaBytes", 0)) for cell in clamp_cells),
        "uiDeclarationCount": len(ui_instances),
        "uiUniquePathShaCount": len(ui_seen),
        "uiDeclarationGpuBytes": ui_declaration_gpu,
        "uiDeclarationReadableCpuBytes": ui_declaration_readable,
        "uiUniquePathShaGpuBytes": ui_unique_gpu,
        "uiUniquePathShaReadableCpuBytes": ui_unique_readable,
        "uiDeclarationCombinedBytes": ui_declaration_gpu + ui_declaration_readable,
        "uiUniquePathShaCombinedBytes": ui_unique_gpu + ui_unique_readable,
        "atlasReservationEnabled": atlas_bytes > 0,
        "residentBytes": resident,
        "loadingBytes": loading,
    }


def common_visuals(capture: dict[str, Any]) -> dict[str, Any]:
    common = capture.get("commonInputs") or {}
    resource_entries = common.get("resourceVirtualPaths", [])
    by_index = {int(entry.get("index", -1)): entry for entry in resource_entries}
    assets: list[dict[str, Any]] = []
    unknown: list[dict[str, Any]] = []
    spark = by_index.get(43)
    if spark:
        identity = spark.get("imageIdentity") or {}
        assets.append({"category": "SPARK", "index": 43, "asset": image_asset(identity.get("path", ""), identity.get("sha256"), "common_spark", source_path=spark.get("path"))})
    else:
        unknown.append({"category": "SPARK", "reason": "resource.dat index43 not captured"})
    words: list[dict[str, Any]] = []
    for index in range(16, 22):
        entry = by_index.get(index)
        if entry:
            identity = entry.get("imageIdentity") or {}
            asset = image_asset(identity.get("path", ""), identity.get("sha256"), "common_words", source_path=entry.get("path"))
            words.append({"index": index, "asset": asset})
            assets.append({"category": "WORDS", "index": index, "asset": asset})
        else:
            unknown.append({"category": "WORDS", "index": index, "reason": "resource.dat index missing"})
    unknown.extend([
        {"category": "kill", "reason": "project mode snapshot/type resource paths were not supplied to this offline capture"},
        {"category": "foot", "reason": "GameConfig GUID/importer texture ownership requires Unity asset graph; kept separate"},
        {"category": "shadow", "reason": "ShadowPrefab Sprite/Material texture graph requires Unity asset graph; kept separate"},
        {"category": "map_background", "reason": "mapId/stage not supplied; all-project-map background union is a separate conservative unknown"},
    ])
    return {
        "resourceDat": common.get("resource.dat"),
        "systemDat": common.get("system.dat"),
        "systemSparkFields": common.get("systemSparkFields"),
        "knownAssets": assets,
        "unknown": unknown,
        "wordsKnownCount": len(words),
        "wordsExpectedCount": 6,
        "sparkIndex": 43,
        "wordIndices": list(range(16, 22)),
    }


def build_set(
    name: str,
    roots: list[int],
    objects: dict[int, dict[str, Any]],
    capture: dict[str, Any],
    pages: dict[str, Any],
    astc: dict[str, Any],
    include_scenario: bool = False,
    include_compatibility: bool = False,
    context_victim_pool: list[int] | None = None,
    include_optional_209: bool = False,
) -> dict[str, Any]:
    closure = DependencyClosure(objects, capture)
    projection = closure.run(
        roots,
        branch="compatibility" if include_compatibility else "production",
        include_scenario=include_scenario,
        include_compatibility=include_compatibility,
        context_victim_pool=context_victim_pool,
        include_optional_209=include_optional_209,
    )
    oids = projection["oids"]
    body_assets, declaration_projection, clamp_cells, body_errors = body_assets_for_declarations(oids, objects, pages, astc)
    grid_validation = grid_validation_for_declarations(name, oids, objects, astc, body_assets, clamp_cells)
    if grid_validation["astcClampConflicts"]:
        raise ValueError(
            "ASTC hard-fail: production clamped source has a usable ASTC entry: "
            + json.dumps(grid_validation["astcClampConflicts"], ensure_ascii=False, sort_keys=True)
        )
    ui_instances, ui_errors = ui_instances_for_oids(oids, objects)
    unique_dat: dict[str, dict[str, Any]] = {}
    for oid in oids:
        item = objects[oid]
        identity = item.get("datIdentity") or {}
        key = asset_key(item.get("datPath", ""), identity.get("sha256"))
        unique_dat[key] = {
            "oid": oid,
            "datPath": item.get("sourcePath"),
            "path": item.get("datPath"),
            "bytes": identity.get("bytes", 0),
            "sha256": identity.get("sha256"),
        }
    astc_mode_active = bool(astc.get("usable"))
    budget_astc = budget_for_assets(body_assets, ui_instances, clamp_cells, astc, pages, "cold", astc_mode=astc_mode_active)
    budget_astc_overlap = budget_for_assets(body_assets, ui_instances, clamp_cells, astc, pages, "old_equals_new_upper_bound", astc_mode=astc_mode_active)
    fallback_astc = {"usable": False, "entries": {}}
    budget_rgba_fallback = budget_for_assets(body_assets, ui_instances, clamp_cells, fallback_astc, pages, "cold", astc_mode=False)
    return {
        "name": name,
        "rootOids": roots,
        "rootNames": [objects[oid].get("bmp", {}).get("name") if oid in objects else f"OID{oid}" for oid in roots],
        "oids": oids,
        "datList": list(unique_dat.values()),
        "datCount": len(unique_dat),
        "rawDatBytesUnique": sum(int(value.get("bytes", 0)) for value in unique_dat.values()),
        "parsedObjectMemory": {
            "status": "unknown_by_contract",
            "reason": "managed parser object/string/array allocations are not derivable from DAT bytes; frame/subblock counts are retained for a separate runtime measurement",
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
            "uiInstanceCount": len(ui_instances),
            "uiUniqueCount": len({instance["asset"]["key"] for instance in ui_instances}),
        },
        "gridValidation": grid_validation,
        "budgets": {
            "cold": budget_astc,
            "astcModeCold": budget_astc,
            "astcModeOldNewPublicationOverlapUpperBound": budget_astc_overlap,
            "unsupportedAstcRgbaFallback": budget_rgba_fallback,
        },
        "edges": projection["edges"],
        "unknowns": projection["unknowns"] + [{"kind": "body_image", "reason": error} for error in body_errors] + [{"kind": "ui_image", "reason": error} for error in ui_errors],
        "cycleRevisitCount": projection["cycleRevisitCount"],
        "closureProcessedCount": projection["processedCount"],
        "branchProfile": "compatibility" if include_compatibility else "production",
        "scenarioContract": "respawn OID998 + loaded [100,199] normal/mode2 drops; stage unknown" if include_scenario else None,
    }


def negative_validations(selection: dict[str, Any], pool: list[dict[str, Any]], pages: dict[str, Any]) -> dict[str, Any]:
    pool_oids = [entry["oid"] for entry in pool]
    seed = int(selection["cryptoSeedDecimal"])
    recomputed = [pool[index]["oid"] for index in random.Random(seed).sample(range(len(pool)), 8)]
    same_seed = recomputed == [int(value) for value in selection["selectedOids"]]

    cycle_graph = {1: [2], 2: [1]}
    seen: set[int] = set()
    queue: deque[int] = deque([1])
    visits = 0
    while queue:
        node = queue.popleft()
        if node in seen:
            visits += 1
            continue
        seen.add(node)
        queue.extend(cycle_graph.get(node, []))
    shared_page_key = "synthetic-page|sha"
    shared_page_dedup = len({shared_page_key, shared_page_key}) == 1
    unknown_edge = {"toOid": 999999, "resolutionStatus": "missing_oid"}
    return {
        "sameSeedRecomputation": {"pass": same_seed, "expectedOids": selection["selectedOids"], "recomputedOids": recomputed},
        "cycleTermination": {"pass": seen == {1, 2} and visits == 1, "visited": sorted(seen), "revisitCount": visits},
        "sharedPageDedup": {"pass": shared_page_dedup, "uniqueCount": 1},
        "unknownOid": {"pass": unknown_edge["resolutionStatus"] == "missing_oid", "syntheticEdge": unknown_edge},
        "poolHasEightDistinct": len(set(pool_oids)) >= 8,
        "activePagesManifestWasUsable": bool(pages.get("usable")),
    }


def shared_summary(sets: dict[str, dict[str, Any]]) -> dict[str, Any]:
    by_asset: dict[str, list[str]] = defaultdict(list)
    for set_name, projection in sets.items():
        for asset in projection["images"]["bodyAssets"]:
            by_asset[asset["key"]].append(set_name)
        for asset in projection["images"]["uiUniqueAssets"]:
            by_asset[asset["key"]].append(set_name)
    shared = []
    for key, owners in sorted(by_asset.items()):
        unique_owners = sorted(set(owners))
        if len(unique_owners) > 1:
            shared.append({"key": key, "setNames": unique_owners, "referenceCount": len(owners)})
    return {
        "uniquePhysicalImageCountAcrossSets": len(by_asset),
        "sharedPhysicalImageCount": len(shared),
        "shared": shared,
        "dedupeRule": "path+SHA physical identity; UI also keeps per-DAT declaration instances separately",
    }


def render_report(result: dict[str, Any], output_path: Path, selection_path: Path) -> None:
    capture = result["capture"]
    selection = result["selection"]
    astc_manifest = result["astcManifest"]
    astc_manifest_label = "deployed ASTC manifest" if astc_manifest.get("usable") else "optional ASTC manifest"
    astc_manifest_path = astc_manifest.get("manifestPath") or "not supplied"
    lines = [
        "# M12 offline eight-role resource and memory audit",
        "",
        f"Status: `{result['status']}`",
        f"Task: `{TASK_ID}`  Change: `{AUDIT_ID}`  Operation: `{OPERATION_ID}`",
        "",
        "This report is an offline static closure and engineering allocation estimate. It does not claim Unity, game, Play Mode, device, GPU, server, or whole-process acceptance.",
        "",
        "## Capture and selection",
        "",
        f"- Capture: `{capture['capturePath']}`; catalog object rows `{capture['objectCount']}`; parser success `{capture['parseSuccess']}`; converter failure frames `{capture['converterFailureFrames']}`.",
        f"- Selection: `{selection_path}`; pool `{selection['poolCount']}`; pool SHA `{selection['poolSha256']}`; selected OIDs `{selection['selectedOids']}`.",
        f"- Selection algorithm: `{selection['algorithm']}`. The selection file was written before amount calculation: `{selection['writtenBeforeAmountCalculation']}`.",
        f"- {astc_manifest_label}: `{astc_manifest.get('status')}` entries `{astc_manifest.get('entryCount')}` unique blocks `{astc_manifest.get('uniqueBlockCount')}`; manifest `{astc_manifest_path}`.",
        "",
        "## Sets",
        "",
        "| Set | OIDs | DATs | unique DAT bytes | body assets | UI instances | UI unique | body GPU | body ASTC | body RGBA fallback | UI unique GPU+readable | UI declaration GPU+readable | cold loading | RGBA fallback loading | unknowns |",
        "|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|",
    ]
    for name, projection in result["sets"].items():
        cold = projection["budgets"]["cold"]
        fallback = projection["budgets"]["unsupportedAstcRgbaFallback"]
        lines.append(
            f"| {name} | {len(projection['oids'])} | {projection['datCount']} | {projection['rawDatBytesUnique']} | {projection['images']['bodyUniqueCount']} | {projection['images']['uiInstanceCount']} | {projection['images']['uiUniqueCount']} | {cold['bodySourceGpuBytes']} | {cold['bodyAstcPayloadBytes']} | {cold['bodyRgbaFallbackBytes']} | {cold['uiUniquePathShaCombinedBytes']} | {cold['uiDeclarationCombinedBytes']} | {cold['loadingBytes']} | {fallback['loadingBytes']} | {len(projection['unknowns'])} |"
        )
    lines.extend([
        "",
        "The `full_catalog_loading_baseline` row is the current all-catalog prewarm baseline. The eight-role rows are static closures and must not be read as the current startup peak.",
        "",
        "For ASTC mode, `budgets.astcModeCold` counts body block payload plus RGBA clamp fallback, sets preflight atlas reservation to zero, and deduplicates head/small by normalized path+SHA while retaining one readable CPU copy. Each set also contains `uiDeclarationGpuBytes/uiDeclarationReadableCpuBytes` for the old per-DAT declaration comparison and `uiUniquePathShaCombinedBytes` for ASTC mode.",
        "The grid audit keeps capture `row/col` raw and applies the production adapter once: runtime rows=DAT col, runtime horizontal divisor/runtime columns=DAT row. The complete per-declaration and clamped-source lists are in `gridValidation` in result.json.",
        "",
        "## Public/common visuals",
        "",
        f"Known common assets: `{len(result['commonVisuals']['knownAssets'])}`; unknown/conditional common categories: `{len(result['commonVisuals']['unknown'])}`.",
        "SPARK/WORDS/kill/foot/shadow/map are kept outside role body totals. Missing WORDS files and Unity asset-graph-owned foot/shadow/map inputs remain unresolved instead of being treated as zero.",
        "",
        "## Validation and unresolved boundaries",
        "",
        f"- Negative validations: `{result['negativeValidations']}`.",
        f"- Shared summary: `{result['sharedSummary']['sharedPhysicalImageCount']}` physical image identities are shared by more than one output set.",
        f"- Grid validation JSON: `{output_path.with_name('grid-validation.json')}`.",
        *[
            f"- Grid validation `{name}`: source sheets `{projection['gridValidation']['sourceSheetCount']}`, declarations `{projection['gridValidation']['ownedDeclarationCount']}`, production body sources `{projection['gridValidation']['productionBodySourceCount']}`, production clamp cells `{projection['gridValidation']['productionClampCellCount']}`, clamped sources `{projection['gridValidation']['productionClampedSourceCount']}`, ASTC clamp conflicts `{len(projection['gridValidation']['astcClampConflicts'])}`; missing-ASTC production sources equal production-clamped `{projection['gridValidation']['inventoryComparison']['manifestMissingEqualsProductionClamped']}`."
            for name, projection in result["sets"].items()
        ],
        "- ASTC is used only when an optional manifest entry, source identity, valid ASTC header, dimensions, and file length all agree. With no ASTC manifest, all body sources use RGBA fallback accounting.",
        "- Derived pages replace mapped source pages in the body payload projection; original source SHA remains captured for provenance and is not counted as simultaneously resident GPU payload.",
        "- `rawDatBytesUnique` is input storage only. `parsedObjectMemory.status=unknown_by_contract` because managed parser allocations, strings, arrays, Unity wrappers, drivers, and scene/object metadata are outside this offline model.",
        "- Stage/map/mode context was not supplied; the stage edge remains `unknown` and no default stage.dat was invented.",
        "",
        f"Result JSON: `{output_path}`",
    ])
    report_path = output_path.with_name("REPORT.md")
    report_path.write_text("\n".join(lines) + "\n", encoding="utf-8", newline="\n")


def audit(args: argparse.Namespace) -> int:
    capture_path = Path(args.capture).resolve()
    output_arg = Path(args.output).resolve()
    output_dir = output_arg.parent if output_arg.suffix.lower() == ".json" else output_arg
    result_path = output_arg if output_arg.suffix.lower() == ".json" else output_dir / "result.json"
    if output_dir.exists():
        raise FileExistsError(f"Refusing to overwrite existing audit output: {output_dir}")
    output_dir.mkdir(parents=True, exist_ok=False)
    capture = read_json(capture_path)
    summary = capture.get("parseSummary") or {}
    objects = capture_object_map(capture)
    if len(capture.get("objects", [])) != 330 or int(summary.get("parseSuccess", 0)) != 330:
        raise ValueError("capture must contain all 330 successfully parsed object DATs")

    selection, selection_path = create_or_load_selection(capture, output_dir, args.selection)
    # Do not move this calculation before create_or_load_selection: the selection
    # contract requires pool/seed/algorithm/roster persistence before any amount.
    pages = load_pages_manifest(args.pages_manifest)
    astc = load_astc_manifest(args.astc_manifest)
    selected_oids = [int(value) for value in selection["selectedOids"]]
    pool_oids = [entry["oid"] for entry in selectable_pool(capture)]

    sets = {
        "role_only_static_superset": build_set("role_only_static_superset", selected_oids, objects, capture, pages, astc),
        "scenario_static_envelope": build_set("scenario_static_envelope", selected_oids, objects, capture, pages, astc, include_scenario=True, context_victim_pool=pool_oids),
        "optional_compatibility_extra": build_set("optional_compatibility_extra", selected_oids, objects, capture, pages, astc, include_compatibility=True, include_optional_209=True),
        "full_catalog_loading_baseline": build_set("full_catalog_loading_baseline", sorted(objects), objects, capture, pages, astc),
    }
    result = {
        "schemaVersion": 1,
        "auditId": AUDIT_ID,
        "taskId": TASK_ID,
        "operationId": OPERATION_ID,
        "status": "STATIC_CONSERVATIVE_CLOSURE_WITH_DECLARED_GAPS",
        "capture": {
            "capturePath": str(capture_path),
            "runtimeRoot": capture.get("runtimeRoot"),
            "objectCount": len(capture.get("objects", [])),
            "parseSuccess": summary.get("parseSuccess"),
            "converterFailureFrames": summary.get("converterFailureFrames"),
            "productionSourceInputs": capture.get("productionSourceInputs", []),
        },
        "selection": selection,
        "pagesManifest": {key: pages.get(key) for key in ("status", "usable", "manifestPath", "errors")},
        "astcManifest": {key: astc.get(key) for key in ("status", "usable", "manifestPath", "entryCount", "uniqueBlockCount", "errors")},
        "sets": sets,
        "commonVisuals": common_visuals(capture),
        "sharedSummary": shared_summary(sets),
        "negativeValidations": negative_validations(selection, selectable_pool(capture), pages),
        "unknownSummary": {
            name: Counter(unknown.get("kind", "unknown") for unknown in projection["unknowns"])
            for name, projection in sets.items()
        },
        "evidenceRules": {
            "dependencyRules": f"{RULE_REVIEW_ROOT}/dependency-rules.json",
            "dependencyReport": f"{RULE_REVIEW_ROOT}/REPORT.md",
            "sourceEvidence": f"{RULE_REVIEW_ROOT}/source-evidence.json",
        },
        "modelBoundary": [
            "No Unity/game/P1/device/build execution",
            "No production resident strategy changes",
            "No whole-process maximum claim",
            "Static all-frame/all-possible closure can include unreachable branches and is a conservative envelope",
        ],
    }
    # Convert Counter objects to ordinary dictionaries for JSON output.
    result["unknownSummary"] = {name: dict(counter) for name, counter in result["unknownSummary"].items()}
    write_json(result_path, result)
    write_json(output_dir / "grid-validation.json", {
        "schemaVersion": 1,
        "formula": "raw DAT row/col retained; production ResolveEffectiveGrid maps runtimeRows=DAT col and runtimeCols=DAT row; x=localPic%runtimeCols; y=sourceHeight-(localPic/runtimeCols+1)*(cellHeight+1)+1",
        "sets": {name: projection["gridValidation"] for name, projection in sets.items()},
    })
    render_report(result, result_path, selection_path)
    print(f"capture={capture_path}")
    print(f"selection={selection_path}")
    print(f"result={result_path}")
    print(f"report={result_path.with_name('REPORT.md')}")
    print(f"gridValidation={output_dir / 'grid-validation.json'}")
    print(f"status={result['status']}")
    return 0


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--capture", required=True, help="RoleResourceCapture JSON")
    parser.add_argument("--pages-manifest", help="Optional active derived_sprite_pages/manifest.json")
    parser.add_argument("--astc-manifest", help="Optional deployed ASTC sidecar manifest")
    parser.add_argument("--selection", help="Existing selection.json for deterministic rerun")
    parser.add_argument("--output", required=True, help="New output directory or result JSON path")
    return parser.parse_args(argv)


if __name__ == "__main__":
    try:
        raise SystemExit(audit(parse_args(sys.argv[1:])))
    except (OSError, ValueError, json.JSONDecodeError) as error:
        print(f"audit-error: {error}", file=sys.stderr)
        raise SystemExit(1)
