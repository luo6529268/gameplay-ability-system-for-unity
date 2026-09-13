"""Read-only NTSD28 content inventory over actual native/Unity source captures.

Only the explicitly supplied output directory is written. Resource inputs are
never modified. Indexed closure is conservative and is not a runtime witness.
"""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
import struct
from collections import Counter, defaultdict
from pathlib import Path

SCHEMA = "ntsd28-content-entry-inventory-v1"
EXE_HASH = "B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033"
IMAGE_EXTENSIONS = {".png", ".bmp", ".jpg", ".jpeg"}
TEXT_EXTENSIONS = {".unity", ".prefab", ".asset", ".mat", ".controller",
                   ".overridecontroller", ".playable", ".cs", ".shader",
                   ".uxml", ".uss", ".json", ".txt", ".meta"}


def key(value):
    value = str(value).replace("\\", "/")
    if value.startswith("/") or re.match(r"^[A-Za-z]:", value) or ".." in value.split("/"):
        raise ValueError(f"Unsafe logical resource path: {value}")
    while value.startswith("./"):
        value = value[2:]
    return value.casefold()


def sha(path):
    digest = hashlib.sha256()
    with Path(path).open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest().upper()


def contained(path, root):
    try:
        Path(path).resolve().relative_to(Path(root).resolve())
        return True
    except ValueError:
        return False


def ensure_output(output, protected):
    output = Path(output).resolve()
    for root in protected:
        if contained(output, root) or contained(root, output):
            raise ValueError(f"Output overlaps protected input: {root}")
    output.mkdir(parents=True, exist_ok=True)
    return output


def registry(path):
    """Parse only data.txt's line-based registry, not DAT subblocks."""
    rows, rejected, section = [], [], None
    for number, original in enumerate(Path(path).read_text(encoding="utf-8-sig").splitlines(), 1):
        # Both current registries use # trailing comments; production Unity
        # GameDataManager.ParseObjects excludes these from the file token.
        text = original.split("#", 1)[0].strip()
        if not text or text.startswith(("#", "//")):
            continue
        lower = text.lower()
        if lower in ("<object>", "<background>"):
            section = lower[1:-1]
            continue
        if lower in ("<object_end>", "<background_end>"):
            section = None
            continue
        if section:
            match = re.fullmatch(r"id:\s*(-?\d+)\s+(?:type:\s*(-?\d+)\s+)?file:\s*(.+?)\s*", text, re.I)
            if not match:
                rejected.append({"line": number, "section": section, "reason": "UNRECOGNIZED_REGISTRY_LINE"})
                continue
            rows.append({"section": section, "id": int(match[1]),
                         "type": int(match[2]) if match[2] else None,
                         "sourcePath": match[3], "line": number,
                         "ordinal": len(rows)})
    return rows, rejected


def load_capture(path):
    result, duplicates = {}, []
    with Path(path).open(encoding="utf-8-sig") as stream:
        for line_no, line in enumerate(stream, 1):
            if not line.strip():
                continue
            data = json.loads(line)
            if "path" not in data:
                raise ValueError(f"Capture row {line_no} lacks path: {path}")
            name = key(data["path"])
            if name in result:
                duplicates.append(name)
            result[name] = data
    if duplicates:
        raise ValueError(f"Duplicate capture paths: {duplicates[:5]}")
    return result


def verify_capture_coverage(capture, root):
    expected = {key(p.relative_to(root).as_posix()) for p in root.rglob("*.dat")}
    missing, extra = sorted(expected - capture.keys()), sorted(capture.keys() - expected)
    if missing or extra:
        raise ValueError(f"Capture coverage mismatch: missing={missing[:5]}, extra={extra[:5]}")
    return len(expected)


def read_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def verify_source_capture_identity(native, native_path, unity, current, authority, repo):
    receipt_root = native_path.parent
    receipt = read_json(receipt_root / "capture-stability.json")
    manifest = read_json(receipt_root / "build-manifest.json")
    if receipt.get("firstSha256") != sha(native_path) or not receipt.get("stable"):
        raise ValueError("Native capture output is not bound to the stable capture receipt")
    if Path(manifest["authoritySourceRoot"]).resolve() != (authority / "source").resolve():
        raise ValueError("Native build used another authority root")
    sources = manifest["sources"] + manifest["conservativeHeaders"]
    for item in sources:
        if sha(item["path"]) != item["sha256"].upper():
            raise ValueError(f"Native source/header changed: {item['path']}")
    before = load_capture(receipt_root / "authority-input-sha256-before.jsonl")
    after = load_capture(receipt_root / "authority-input-sha256-after.jsonl")
    if before != after or set(before) != set(native):
        raise ValueError("Native input receipt coverage/hash changed")
    native_root = authority / "resources/runtime/decoded_dat"
    for path, doc in native.items():
        if doc.get("sourceModel") != "SOURCE_MODEL_DIAGNOSTIC_ONLY" or sha(native_root / path) != before[path]["sha256"].upper():
            raise ValueError(f"Native capture input identity mismatch: {path}")
    checked_sources = {}
    for capture, root, mode in ((unity, native_root, "plaintext"), (current, repo / "Assets/NTSD/Config", "unity")):
        for path, doc in capture.items():
            identity = doc["inputIdentity"]
            if doc.get("auditId") != "NTSD28-B11-CONTENT-ENTRY-INVENTORY-001" or doc.get("inputMode") != mode:
                raise ValueError(f"Unity capture identity/mode mismatch: {path}")
            if Path(identity["absoluteRoot"]).resolve() != root.resolve() or identity["sha256"].upper() != sha(root / path):
                raise ValueError(f"Unity capture input changed: {path}")
            source = doc["source"]
            if source.get("loadsLibraryScriptAssemblies") is not False:
                raise ValueError("Unity capture must not use Library DLLs")
            hashes = source["linkedProductionSourceSha256"]
            if not hashes:
                raise ValueError("Missing linked production source identity")
            for relative, expected in hashes.items():
                if relative not in checked_sources:
                    checked_sources[relative] = sha(repo / relative)
                if checked_sources[relative] != expected.upper():
                    raise ValueError(f"Unity parser/converter source changed: {relative}")
            stub = source["stubBoundary"]
            if sha(repo / stub["file"]) != stub["sha256"].upper():
                raise ValueError("Unity diagnostic stub identity changed")
    return {"nativeInputFiles": len(native), "nativeSourceAndHeaderFiles": len(sources),
            "unitySourceFiles": len(checked_sources), "inputsAndSourcesMatch": True,
            "nativeBuildManifestSha256": sha(receipt_root / "build-manifest.json"),
            "classification": "SOURCE_MODEL_SUBSET_PLUS_CONSERVATIVE_HEADERS_NOT_FULL_PLAYABLE_BUILD"}


def frame_map(file):
    counts, result = Counter(), {}
    for frame in file.get("frames", []):
        number = frame["id"]
        occurrence = counts[number]
        counts[number] += 1
        result[(number, occurrence)] = frame
    return result


def fields_map(fields):
    # Diagnostic last explicit value; raw captures retain order/duplicates.
    return {str(p["key"]).lower(): p.get("value") for p in fields or []}


def compare_capture(native, unity):
    aggregate = {}
    by_file, samples = [], []
    for path, expected in sorted(native.items()):
        actual = unity.get(path)
        stats = Counter()
        if actual is None:
            by_file.append({"path": path, "status": "MISSING_UNITY_CAPTURE"})
            continue
        left_frames, right_frames = frame_map(expected), frame_map(actual)
        stats["nativeFrames"] = len(left_frames)
        stats["unityFrames"] = len(right_frames)
        stats["missingFrames"] = len(left_frames.keys() - right_frames.keys())
        stats["extraFrames"] = len(right_frames.keys() - left_frames.keys())
        for frame_key, frame in left_frames.items():
            target = right_frames.get(frame_key)
            if target is None:
                continue
            if target.get("conversionError"):
                stats["rejectedFrames"] += 1
            for index, block in enumerate(frame.get("subblocks", [])):
                others = target.get("subblocks", [])
                if index >= len(others) or block.get("kind") != others[index].get("kind"):
                    stats["subblockStructureDifference"] += 1
                    continue
                other = others[index]
                if other.get("conversionError"):
                    stats["rejectedSubblocks"] += 1
                if block.get("conversionError"):
                    stats["nativeProjectionDiagnostic"] += 1
                left, right = block.get("normalized"), other.get("normalized")
                if left is None:
                    continue
                if right is None:
                    stats["unavailableTypedProjection"] += 1
                    continue
                explicit = fields_map(block.get("fields", []))
                for field, value in left.items():
                    missing = field not in right
                    if not missing and value == right[field]:
                        continue
                    if block["kind"] == "bdy":
                        # Native capture retains parsed scalar fields here, not
                        # CollisionGeometry's final box. Do not promote this
                        # into a proven runtime/DTO gap.
                        issue = "PARSED_BODY_FIELD_REVIEW"
                    elif not missing and isinstance(value, (list, dict)) != isinstance(right[field], (list, dict)):
                        issue = "REPRESENTATION_SHAPE_REVIEW"
                    elif not missing and (value is None) != (right[field] is None):
                        issue = "REPRESENTATION_SHAPE_REVIEW"
                    else:
                        issue = "DTO_FIELD_MISSING" if missing else "TYPED_VALUE_DIFFERENCE"
                    stats[issue] += 1
                    group = (block["kind"], field, issue)
                    if group not in aggregate:
                        aggregate[group] = {"kind": group[0], "field": field, "issue": issue,
                            "occurrences": 0, "explicitOccurrences": 0, "nativeNonzeroOccurrences": 0,
                            "firstPath": path, "firstFrame": frame_key[0], "firstSubblock": index,
                            "firstNative": value, "firstUnity": right.get(field), "files": set()}
                    item = aggregate[group]
                    item["occurrences"] += 1
                    item["explicitOccurrences"] += field.lower() in explicit
                    item["nativeNonzeroOccurrences"] += value not in (0, None, "", False)
                    item["files"].add(path)
                    if len(samples) < 200:
                        samples.append({"path": path, "frame": frame_key[0], "occurrence": frame_key[1],
                                        "subblock": index, "kind": block["kind"], "field": field,
                                        "issue": issue, "native": value, "unity": right.get(field)})
        by_file.append({"path": path, "status": "SOURCE_PROJECTION_COMPARED",
            "nativeParseSuccess": expected.get("parseSuccess"), "unityParseSuccess": actual.get("parseSuccess"),
            "unityParserReturned": actual.get("parserReturned"),
            "unityParserDiagnosticErrors": actual.get("parserDiagnosticErrors"),
            "unityFullConverterSuccess": actual.get("fullConverterSuccess"),
            "unityIsolatedSubblockFailures": actual.get("isolatedSubblockFailures"),
            "comparisonScope": "NATIVE_FIELDS_TO_UNITY_SUBBLOCKS_ONLY_EXTRA_UNITY_FIELDS_NOT_SEMANTICALLY_ADJUDICATED",
            "nativeDiagnosticsCount": len(expected.get("diagnostics", [])),
            "unityDiagnosticsCount": len(actual.get("diagnostics", [])),
            "firstFrameError": next((f.get("conversionError") for f in actual.get("frames", []) if f.get("conversionError")), None),
            **stats})
    issues = []
    for group, item in sorted(aggregate.items()):
        item["fileCount"] = len(item.pop("files"))
        issues.append(item)
    return by_file, issues, samples


def image_info(path):
    with Path(path).open("rb") as stream:
        data = stream.read(40)
    if data[:8] == b"\x89PNG\r\n\x1a\n" and len(data) >= 24:
        width, height = struct.unpack(">II", data[16:24])
        return {"format": "PNG", "width": width, "height": height}
    if data[:2] == b"BM" and len(data) >= 26:
        dib = struct.unpack_from("<I", data, 14)[0]
        if dib == 12:
            width, height = struct.unpack_from("<HH", data, 18)
        else:
            width, height = struct.unpack_from("<ii", data, 18)
        return {"format": "BMP", "width": width, "height": abs(height)}
    return {"format": "UNKNOWN", "width": None, "height": None}


def file_manifest(root, extensions):
    rows = []
    for path in sorted(Path(root).rglob("*")):
        if not path.is_file() or path.suffix.lower() not in extensions:
            continue
        rows.append({"path": path.relative_to(root).as_posix(), "bytes": path.stat().st_size,
                     "hash": sha(path), **(image_info(path) if path.suffix.lower() in IMAGE_EXTENSIONS else {})})
    return rows


def guid(path):
    meta = Path(str(path) + ".meta")
    if not meta.is_file():
        return None
    match = re.search(r"(?m)^guid:\s*([0-9a-fA-F]{32})\s*$", meta.read_text(encoding="utf-8-sig"))
    return match[1].lower() if match else None


def image_references(file):
    refs = []
    for sheet in file.get("sprites", []):
        if sheet.get("path"):
            refs.append(("sheet", sheet["path"]))
    for section in ("bmp", "top", "stats"):
        for prop in file.get(section, []) or []:
            value = str(prop.get("value", "")).strip()
            if Path(value).suffix.lower() in IMAGE_EXTENSIONS:
                refs.append((f"{section}:{prop['key']}", value))
    for name, value in (file.get("bmpHeader") or {}).items():
        if isinstance(value, str) and Path(value).suffix.lower() in IMAGE_EXTENSIONS:
            refs.append((f"bmpHeader:{name}", value))
    return sorted(set(refs))


def menu_image_references(file):
    refs = []
    for index, layer in enumerate(file.get("menuFace", []) or []):
        for prop in layer.get("fields", []):
            if prop.get("key", "").lower() == "pic":
                value = str(prop.get("value", ""))
                if Path(value).suffix.lower() in IMAGE_EXTENSIONS:
                    refs.append((index, value))
    return refs


def compare_sprite_layouts(native, unity):
    differences = []
    fields = ("path", "width", "height", "row", "col", "declaredFirst", "declaredLast", "effectiveFirst", "effectiveLast")
    for path in sorted(native.keys() & unity.keys()):
        left, right = native[path].get("sprites", []), unity[path].get("sprites", [])
        for index in range(max(len(left), len(right))):
            if index >= len(left) or index >= len(right):
                differences.append({"path": path, "sheet": index, "field": "declarationPresence",
                                    "native": index < len(left), "unity": index < len(right)})
                continue
            for name in fields:
                a, b = left[index].get(name), right[index].get(name)
                if name == "path":
                    a, b = key(a), key(b)
                if a != b:
                    differences.append({"path": path, "sheet": index, "field": name, "native": a, "unity": b})
    return differences


def resolve_image(root, token, dat_dir=None, project=None):
    normalized = str(token).replace("\\", "/")
    if normalized.lower().startswith("assets/") and project:
        candidate = Path(project) / normalized
    else:
        candidate = Path(dat_dir if dat_dir else root) / normalized
    if candidate.is_file():
        return candidate.resolve(), "EXACT"
    if dat_dir is None and candidate.suffix.lower() == ".bmp":
        alternate = candidate.with_suffix(".png")
        if alternate.is_file():
            return alternate.resolve(), "NATIVE_BMP_TO_PNG_FALLBACK"
    return candidate.resolve(), "MISSING"


def write_json(path, data):
    Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2, sort_keys=True) + "\n", encoding="utf-8")


def write_csv(path, rows):
    columns = list(dict.fromkeys(k for row in rows for k in row))
    with Path(path).open("w", newline="", encoding="utf-8-sig") as stream:
        writer = csv.DictWriter(stream, fieldnames=columns)
        writer.writeheader()
        for row in rows:
            writer.writerow({k: json.dumps(v, ensure_ascii=False) if isinstance(v, (list, dict)) else v
                             for k, v in row.items()})


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--repository", required=True, type=Path)
    parser.add_argument("--authority", required=True, type=Path)
    parser.add_argument("--native-capture", required=True, type=Path)
    parser.add_argument("--unity-native-capture", required=True, type=Path)
    parser.add_argument("--unity-current-capture", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    repo, authority = args.repository.resolve(), args.authority.resolve()
    runtime, old_root = authority / "resources/runtime", repo / "Assets/NTSD/Config"
    out = ensure_output(args.output, [authority, repo / "Assets", repo / "ProjectSettings", repo / "Packages"])
    exe_hash = sha(authority / "NTSD2.8-Logan.exe")
    if exe_hash != EXE_HASH:
        raise ValueError("Formal EXE identity changed; no audit conclusion produced")
    native = load_capture(args.native_capture)
    through_unity = load_capture(args.unity_native_capture)
    current = load_capture(args.unity_current_capture)
    identity = verify_source_capture_identity(native, args.native_capture, through_unity, current, authority, repo)
    write_json(out / "capture-identity-verification.json", identity)
    coverage = {
        "native": verify_capture_coverage(native, runtime / "decoded_dat"),
        "unityOnNative": verify_capture_coverage(through_unity, runtime / "decoded_dat"),
        "unityCurrent": verify_capture_coverage(current, old_root)}
    write_json(out / "capture-coverage.json", coverage)
    new_rows, new_rejected = registry(runtime / "decoded_dat/data/data.txt")
    old_rows, old_rejected = registry(old_root / "data.txt")
    catalog = list(csv.DictReader((runtime / "catalog.csv").open(encoding="utf-8-sig")))
    native_manifest = file_manifest(runtime, IMAGE_EXTENSIONS | {".dat"})
    write_csv(out / "native-dat-image-manifest.csv", native_manifest)
    write_csv(out / "unity-dat-manifest.csv", file_manifest(old_root, {".dat"}))
    catalog_identity = Counter((r["registry_section"], int(r["id"]), key(r["source_path"]), int(r["type"]) if r["type"] else None) for r in catalog)
    registry_identity = Counter((r["section"], r["id"], key(r["sourcePath"]), r["type"]) for r in new_rows)
    write_json(out / "registry-catalog-reconciliation.json", {
        "equal": catalog_identity == registry_identity,
        "duplicateCatalogIdentities": [list(k) for k, count in catalog_identity.items() if count > 1],
        "onlyCatalog": [{"identity": list(k), "count": v} for k, v in sorted((catalog_identity - registry_identity).items())],
        "onlyRegistry": [{"identity": list(k), "count": v} for k, v in sorted((registry_identity - catalog_identity).items())]})
    write_csv(out / "catalog-full-review.csv", [{**r,
        "physicalExists": (runtime / "decoded_dat" / r["source_path"].replace("\\", "/")).is_file(),
        "nonRegistryMetadataStatus": "CATALOG_METADATA_ONLY_REACHABILITY_NOT_CERTIFIED"} for r in catalog])
    by_id_new, by_id_old = defaultdict(list), defaultdict(list)
    for row in new_rows:
        by_id_new[(row["section"], row["id"])].append(row)
    for row in old_rows:
        by_id_old[(row["section"], row["id"])].append(row)
    mappings = []
    paired_content = []
    for identity in sorted(by_id_new.keys() | by_id_old.keys()):
        newer, older = by_id_new[identity], by_id_old[identity]
        n, o = (newer[0] if newer else None), (older[0] if older else None)
        np = runtime / "decoded_dat" / n["sourcePath"].replace("\\", "/") if n else None
        op = ((repo / o["sourcePath"]) if key(o["sourcePath"]).startswith("assets/")
              else old_root / o["sourcePath"].replace("\\", "/")) if o else None
        state = ("DUPLICATE_ID_REVIEW" if len(newer) > 1 or len(older) > 1 else
                 "NEW_INDEXED_ENTRY" if not o else "OLD_ONLY_REVIEW" if not n else "MATCHED_BY_SECTION_AND_ID")
        native_exists = bool(np and contained(np, runtime / "decoded_dat") and np.is_file())
        unity_exists = bool(op and contained(op, repo / "Assets") and op.is_file())
        mappings.append({"section": identity[0], "id": identity[1], "state": state,
            "nativeCount": len(newer), "unityCount": len(older),
            "nativeType": n["type"] if n else None, "unityType": o["type"] if o else None,
            "nativePath": n["sourcePath"] if n else None, "unityPath": o["sourcePath"] if o else None,
            "nativeExists": native_exists, "unityExists": unity_exists,
            "nativeHash": sha(np) if native_exists else None,
            "unityHash": sha(op) if unity_exists else None,
            "unityGuid": guid(op) if unity_exists else None,
            "targetLogicalPath": "decoded_dat/" + n["sourcePath"].replace("\\", "/") if n else None,
            "physicalTarget": "TO_BE_FROZEN_BY_Q02", "deleteAuthorized": False})
        if native_exists and unity_exists and len(newer) == len(older) == 1:
            left = native.get(key(np.relative_to(runtime / "decoded_dat").as_posix()))
            try:
                old_key = key(op.relative_to(old_root).as_posix())
            except ValueError:
                old_key = None
            right = current.get(old_key)
            if left is not None and right is not None:
                lf, rf = frame_map(left), frame_map(right)
                changed_frames = []
                for fk in sorted(lf.keys() & rf.keys()):
                    if fields_map(lf[fk].get("fields", [])) != fields_map(rf[fk].get("fields", [])):
                        changed_frames.append(fk[0])
                paired_content.append({"section": identity[0], "id": identity[1],
                    "nativePath": n["sourcePath"], "unityPath": o["sourcePath"],
                    "nativeDeclaredFrames": len(lf), "unityDeclaredFrames": len(rf),
                    "nativeOnlyFrameOccurrences": len(lf.keys() - rf.keys()),
                    "unityOnlyFrameOccurrences": len(rf.keys() - lf.keys()),
                    "changedCommonFrameFieldBags": len(changed_frames), "firstChangedFrameIds": changed_frames[:12],
                    "nativeSpriteDeclarations": len(left.get("sprites", [])),
                    "unitySpriteDeclarations": len(right.get("sprites", [])),
                    "classification": "PARTIAL_AST_PROJECTION_ONLY_NOT_CONTENT_EQUALITY_OR_RULE_DEFECT"})
    write_csv(out / "object-mapping.csv", mappings)
    write_csv(out / "same-id-content-deltas.csv", paired_content)
    compatibility, issues, examples = compare_capture(native, through_unity)
    object_paths = {key(r["sourcePath"]) for r in new_rows if r["section"] == "object"}
    background_paths = {key(r["sourcePath"]) for r in new_rows if r["section"] == "background"}
    for row in compatibility:
        p = key(row["path"])
        row["domain"] = ("INDEXED_OBJECT_DAT" if p in object_paths else
                         "INDEXED_BACKGROUND_DAT" if p in background_paths else "ANCILLARY_DAT_OWNER_REVIEW")
    native_parser_failures = [{"path": p,
        "domain": "INDEXED_OBJECT_DAT" if p in object_paths else "INDEXED_BACKGROUND_DAT" if p in background_paths else "ANCILLARY_DAT_OWNER_REVIEW",
        "diagnostics": d.get("diagnostics", [])} for p, d in sorted(native.items()) if not d.get("parseSuccess")]
    write_json(out / "native-generic-parser-domain-review.json", {
        "failures": native_parser_failures,
        "interpretation": "Non-object resource/HUD formats can have dedicated production parsers. Generic DatParser diagnostics do not establish a formal release defect."})
    write_csv(out / "parser-compatibility-by-file.csv", compatibility)
    write_csv(out / "typed-projection-gaps.csv", issues)
    write_json(out / "first-difference-examples.json", examples)
    sprite_differences = compare_sprite_layouts(native, through_unity)
    write_csv(out / "same-input-sprite-layout-differences.csv", sprite_differences)
    images, dependencies, asset_owners = [], [], defaultdict(set)
    for side, rows, capture in (("native", new_rows, native), ("unity", old_rows, current)):
        for row in rows:
            if row["section"] != "object":
                continue
            source = row["sourcePath"].replace("\\", "/")
            dat_path = (runtime / "decoded_dat" / source if side == "native" else
                        repo / source if key(source).startswith("assets/") else old_root / source)
            try:
                relative = dat_path.relative_to(runtime / "decoded_dat" if side == "native" else old_root).as_posix()
            except ValueError:
                relative = source
            doc = capture.get(key(relative))
            if doc is None:
                continue
            for role, token in image_references(doc):
                resolved, resolution = resolve_image(runtime / "vfs", token,
                    dat_dir=dat_path.parent if side == "unity" else None, project=repo)
                allowed_root = runtime / "vfs" if side == "native" else repo / "Assets"
                if not contained(resolved, allowed_root):
                    resolution = "OUTSIDE_ALLOWED_ROOT_REVIEW"
                exists = resolution != "OUTSIDE_ALLOWED_ROOT_REVIEW" and resolved.is_file()
                images.append({"side": side, "objectId": row["id"], "type": row["type"],
                    "sourceDat": source, "role": role, "token": token,
                    "resolved": str(resolved), "resolution": resolution,
                    "hash": sha(resolved) if exists else None,
                    "guid": guid(resolved) if side == "unity" and exists else None,
                    **(image_info(resolved) if exists else {"format": None, "width": None, "height": None})})
                if side == "unity" and exists and contained(resolved, repo):
                    asset_owners[resolved.relative_to(repo).as_posix()].add(row["id"])
            for frame in doc.get("frames", []):
                for index, block in enumerate(frame.get("subblocks", [])):
                    if block.get("kind", "").lower() != "opoint":
                        continue
                    values = fields_map(block.get("fields", []))
                    try:
                        target = int(str(values.get("oid", "0")))
                    except ValueError:
                        target = None
                    dependencies.append({"side": side, "sourceId": row["id"], "sourceDat": source,
                        "frame": frame["id"], "subblock": index, "targetId": target,
                        "targetIndexed": ("object", target) in (by_id_new if side == "native" else by_id_old),
                        "edgeType": "DECLARED_OPOINT_NOT_RUNTIME_REACHABILITY"})
    write_csv(out / "indexed-image-references.csv", images)
    write_csv(out / "opoint-reference-edges.csv", dependencies)
    ancillary_images = []
    for source_path, doc in sorted(native.items()):
        if source_path in object_paths:
            continue
        for role, token in image_references(doc):
            resolved, resolution = resolve_image(runtime / "vfs", token)
            exists = contained(resolved, runtime / "vfs") and resolved.is_file()
            ancillary_images.append({"sourceDat": source_path, "role": role, "token": token,
                "resolved": str(resolved), "resolution": resolution,
                "domain": "BACKGROUND" if source_path in background_paths else "ANCILLARY_RESOURCE_OWNER_REVIEW",
                "hash": sha(resolved) if exists else None,
                "disposition": "CLASSIFY_BATTLE_SHARED_VS_USER_EXCLUDED_BEFORE_MIGRATION",
                **(image_info(resolved) if exists else {})})
    write_csv(out / "ancillary-image-references-review.csv", ancillary_images)
    menu_images = []
    for side, capture in (("native", native), ("unityOnNative", through_unity), ("unityCurrent", current)):
        for source_path, doc in sorted(capture.items()):
            for layer, token in menu_image_references(doc):
                # Native declarations and same-input Unity AST are resolved to
                # the formal VFS only for reference inventory, not loader proof.
                resolved, resolution = resolve_image(runtime / "vfs", token,
                    dat_dir=old_root / Path(source_path).parent if side == "unityCurrent" else None, project=repo)
                allowed = repo / "Assets" if side == "unityCurrent" else runtime / "vfs"
                if not contained(resolved, allowed):
                    resolution = "OUTSIDE_ALLOWED_ROOT_REVIEW"
                exists = contained(resolved, allowed) and resolved.is_file()
                menu_images.append({"side": side, "sourceDat": source_path, "layer": layer,
                    "token": token, "resolved": str(resolved), "resolution": resolution,
                    "hash": sha(resolved) if exists else None,
                    "domain": "NON_BATTLE_MENU_SELECTION_OWNER_REVIEW", "migrationAuthorized": False})
    write_csv(out / "menu-face-image-references-review.csv", menu_images)
    candidates = []
    for path in sorted(old_root.rglob("*.dat")):
        candidates.append({"path": path.relative_to(repo).as_posix(), "kind": "DAT",
            "guid": guid(path), "hash": sha(path), "status": "MIGRATION_REVIEW_NO_DELETE_AUTHORIZATION"})
    for path in sorted((repo / "Assets/NTSD/Sprite").rglob("*")):
        if not path.is_file() or path.suffix.lower() not in IMAGE_EXTENSIONS:
            continue
        relative = path.relative_to(repo).as_posix()
        candidates.append({"path": relative, "kind": "IMAGE", "guid": guid(path), "hash": sha(path),
            "objectOwners": sorted(asset_owners.get(relative, [])),
            "status": "REFERENCED_BY_OLD_INDEXED_CONTENT_REVIEW" if relative in asset_owners else "PRESERVE_NOT_IN_PROVEN_REPLACEMENT_SET"})
    wanted_guids = {c["guid"] for c in candidates if c["guid"]}
    guid_definitions = defaultdict(list)
    for candidate in candidates:
        if candidate["guid"]:
            guid_definitions[candidate["guid"]].append(candidate["path"])
    reverse_guids, text_review = defaultdict(set), []
    for path in sorted((repo / "Assets").rglob("*")):
        if not path.is_file() or path.suffix.lower() not in TEXT_EXTENSIONS:
            continue
        try:
            content = path.read_bytes()
            if b"\0" in content[:1024]:
                text_review.append({"path": path.relative_to(repo).as_posix(), "reason": "BINARY_REFERENCE_REVIEW"})
                continue
            for value in re.findall(rb"guid:\s*([0-9a-fA-F]{32})", content):
                value = value.decode("ascii").lower()
                if value in wanted_guids:
                    reverse_guids[value].add(path.relative_to(repo).as_posix())
        except OSError as error:
            text_review.append({"path": path.relative_to(repo).as_posix(), "reason": type(error).__name__})
    for row in candidates:
        row["guidReferrers"] = sorted(p for p in reverse_guids[row["guid"]] if p != row["path"] + ".meta")
        row["guidReferenceCount"] = len(row["guidReferrers"])
        row["sameGuidCandidateAssets"] = sorted(guid_definitions[row["guid"]]) if row["guid"] else []
        row["deleteAuthorized"] = False
    write_csv(out / "old-asset-disposition-review.csv", candidates)
    write_json(out / "reference-scan-limitations.json", {"unreadableOrBinary": text_review,
        "limits": ["GUID scan does not prove absence of dynamically built paths or external project references",
                   "No image is deletion-approved by this audit", "Indexed and declared edges do not prove runtime reachability"]})
    inputs = {"formalExe": exe_hash, "nativeRegistry": sha(runtime / "decoded_dat/data/data.txt"),
              "nativeCatalog": sha(runtime / "catalog.csv"), "unityRegistry": sha(old_root / "data.txt"),
              "nativeCapture": sha(args.native_capture), "unityNativeCapture": sha(args.unity_native_capture),
              "unityCurrentCapture": sha(args.unity_current_capture)}
    summary = {"schema": SCHEMA, "evidenceClass": "SOURCE_MODEL_CONTENT_DIAGNOSTIC_NOT_RUNTIME_CERTIFICATE",
        "inputs": inputs, "nativeRegistryCounts": dict(Counter(r["section"] for r in new_rows)),
        "unityRegistryCounts": dict(Counter(r["section"] for r in old_rows)), "catalogRows": len(catalog),
        "registryCatalogEqual": catalog_identity == registry_identity,
        "nativeManifestCounts": dict(Counter(Path(r["path"]).suffix.lower() for r in native_manifest)),
        "nativeCapturedFiles": len(native), "unityThroughNativeCapturedFiles": len(through_unity),
        "nativeGenericParserFailures": len(native_parser_failures),
        "nativeIndexedObjectParserFailures": sum(r["domain"] == "INDEXED_OBJECT_DAT" for r in native_parser_failures),
        "unityCurrentCapturedFiles": len(current), "registryRejectedLines": {"native": new_rejected, "unity": old_rejected},
        "mappingStates": dict(Counter(r["state"] for r in mappings)), "typedGapGroups": len(issues),
        "sameIdContentComparisons": len(paired_content),
        "sameInputRejectedFrames": sum(r.get("rejectedFrames", 0) for r in compatibility),
        "sameInputRejectedSubblocks": sum(r.get("rejectedSubblocks", 0) for r in compatibility),
        "sameInputSpriteDifferences": len(sprite_differences),
        "imageReferenceCounts": dict(Counter(r["side"] for r in images)),
        "ancillaryImageReferencesForOwnerReview": len(ancillary_images),
        "menuFaceReferencesForNonBattleReview": dict(Counter(r["side"] for r in menu_images)),
        "missingImageReferences": dict(Counter(r["side"] for r in images if r["resolution"] == "MISSING")),
        "opointEdgeCounts": dict(Counter(r["side"] for r in dependencies)),
        "oldAssetsReviewed": len(candidates), "deletionApproved": 0,
        "unresolved": ["Physical migration layout belongs to Q02", "Registry is not full semantic reachability",
                       "Code-generated asset paths and scene memory references require later review",
                       "No Unity Play or formal executable battle was run"]}
    write_json(out / "summary.json", summary)
    write_json(out / "output-hashes.json", {p.name: sha(p) for p in sorted(out.iterdir())
                                           if p.is_file() and p.name != "output-hashes.json"})
    print(json.dumps(summary, ensure_ascii=False))


if __name__ == "__main__":
    main()
