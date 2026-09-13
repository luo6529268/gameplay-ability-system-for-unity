"""Read-only native/actual Unity definition-header audit; outputs diagnostics only."""
from collections import Counter, defaultdict
from pathlib import Path
import hashlib
import json
import re
import struct
import subprocess

REPO = Path(__file__).resolve().parents[2]
ROOT = REPO / "artifacts/diagnostics/NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001"
AUTHORITY = Path("J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan")
MOVEMENT = "walking_speed walking_speedz running_speed running_speedz heavy_walking_speed heavy_walking_speedz heavy_running_speed heavy_running_speedz jump_height jump_distance jump_distancez dash_height dash_distance dash_distancez rowing_height rowing_distance".split()
BMP_INTS = {"use_ai": "use_ai", "property": "property", "effect": "definition_effect", "weapon_hp": "weapon_hp", "weapon_drop_hurt": "weapon_drop_hurt"}
STATS_INTS = {key: key for key in "recmp caughtact normal_attack1 normal_attack2 light_throw weapon_drink heavy_throw run_heavy_throw run_attack jump_attack sky_light_throw".split()}
STATS_INTS["attacking"] = "definition_attacking"
SEQUENCES = {"walking_frame": "walking_frames", "running_frame": "running_frames", "heavy_walking_frame": "heavy_walking_frames", "heavy_running_frame": "heavy_running_frames"}


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def save(name, value):
    (ROOT / name).write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")


def verify(path, expected):
    actual = digest(path)
    if actual.lower() != expected.lower():
        raise RuntimeError(f"Input changed: {path}")


def properties(fields):
    return {field["key"]: field["value"] for field in fields}


def pairs(fields):
    return [[field["key"], field["value"]] for field in fields]


def as_double(bits):
    return struct.unpack("<d", struct.pack("<q", bits))[0]


def main():
    capture = json.loads((ROOT / "unity-headers.json").read_text(encoding="utf-8"))
    for path, expected in capture["productionHashes"].items():
        verify(REPO / path, expected)
    build = json.loads((REPO / "artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/native/build-manifest.json").read_text(encoding="utf-8-sig"))
    for entry in build["sources"] + build["conservativeHeaders"]:
        verify(entry["path"], entry["sha256"])
    verify(build["executable"], build["executableSha256"])
    fresh = REPO / "Temp/NTSD28DefinitionAudit/native-fresh.jsonl"
    verify(fresh, json.loads((ROOT / "native-fresh-identity.json").read_text(encoding="utf-8"))["sha256"])
    native = {}
    for line in fresh.open(encoding="utf-8"):
        document = json.loads(line)
        native[document["path"].lower()] = document
    definitions = capture["definitions"]
    for entry in definitions:
        verify(AUTHORITY / "resources/runtime/decoded_dat" / entry["path"], entry["datSha256"])
        assert native[entry["path"].lower()]["parseSuccess"]

    # Use the previously verified native executables, not Python numeric parsing.
    values = {""}
    for entry in definitions:
        doc = native[entry["path"].lower()]
        for section in (doc["bmp"], doc["stats"]):
            values.update(field["value"] for field in section)
    values = sorted(values)
    raw_meta = json.loads((REPO / "artifacts/diagnostics/NTSD28-Q05-NATIVE-NUMERIC-DECODER-001/raw-native-build-identity.json").read_text(encoding="utf-8"))
    for path, expected in raw_meta["hashes"].items():
        verify(REPO / path if not Path(path).is_absolute() else path, expected)
    integer_exe = REPO / "Temp/Q05NumericDifferential/AuthorityRawNumericWitness.exe"
    raw_input = "".join(f"{i}\t{value.encode().hex()}\n" for i, value in enumerate(values)).encode()
    (ROOT / "header-number-input.tsv").write_bytes(raw_input)
    raw_output = subprocess.check_output([str(integer_exe)], input=raw_input)
    assert raw_output == subprocess.check_output([str(integer_exe)], input=raw_input)
    (ROOT / "header-number-native.tsv").write_bytes(raw_output)
    integer = {}
    float32 = {}
    for row in raw_output.decode().splitlines()[1:]:
        cols = row.split("\t")
        value = values[int(cols[0])]
        integer[value] = int(cols[2]) if cols[4] == "1" else None
        float32[value] = int(cols[1], 16)
    double_input = ROOT / "header-double-input.hex"
    double_input.write_text("".join(value.encode().hex() + "\n" for value in values), encoding="utf-8")
    double_exe = REPO / "Temp/NTSD28Q05TypedFrame/AuthorityTypedFrameWitness.exe"
    double_output = subprocess.check_output([str(double_exe), "--numbers", str(double_input)])
    assert double_output == subprocess.check_output([str(double_exe), "--numbers", str(double_input)])
    (ROOT / "header-double-native.tsv").write_bytes(double_output)
    double = {values[int(row.split("\t")[0])]: int(row.split("\t")[1]) for row in double_output.decode().splitlines()}

    differences = []
    raw_differences = []
    missing_stats = []
    armor_differences = []
    piece_definitions = []
    bmp_carrier_gaps = []
    sequence_differences = []
    counts = Counter()
    matched = Counter()
    for entry in definitions:
        doc = native[entry["path"].lower()]
        bmp, stats = properties(doc["bmp"]), properties(doc["stats"])
        data = entry["values"]
        label = {"path": entry["path"], "id": entry["id"], "type": entry["type"]}
        for key in "shadow bound shadow_pic shadowsize drop frame_0mp sound1 sound2 smallb hidden random small_x small_y bars".split():
            if key in bmp and key not in capture["fieldTypes"]:
                scope = "BATTLE_PRESENTATION_OR_RULE_READER"
                if key in {"hidden", "random"}: scope = "EXCLUDED_SELECTION_FLOW"
                if key in {"smallb", "small_x", "small_y", "bars"}: scope = "EXCLUDED_ORDINARY_HUD_REVIEW_ONLY"
                bmp_carrier_gaps.append({**label, "key": key, "raw": bmp[key], "scope": scope,
                                         "carrier": "NO_CORRESPONDING_LF2CharacterData_MEMBER"})
        for key in MOVEMENT:
            raw = bmp.get(key, "")
            expected = double[raw]
            actual = entry["floatPromotedDoubleBits"][key]
            counts["movementComparisons"] += 1
            if expected != actual:
                category = "missing_native_default_zero" if key not in bmp else "value_or_namespace"
                if key in bmp and (entry["float32Bits"][key] & 0xffffffff) == float32[raw]:
                    category = "float32_precision"
                differences.append({**label, "section": "bmp", "key": key, "category": category,
                                    "raw": bmp.get(key), "nativeDoubleBits": expected, "unityDoubleBits": actual,
                                    "nativeValue": as_double(expected), "unityValue": as_double(actual)})
            else:
                matched["movement"] += 1
        for section, raw_fields, mapping, fallback in [
            ("bmp", bmp, BMP_INTS, 0), ("stats", stats, STATS_INTS, 0),
            ("bmp", bmp, {"walking_frame_rate": "walking_frame_rate", "running_frame_rate": "running_frame_rate"}, 1)
        ]:
            for key, member in mapping.items():
                value = integer[raw_fields.get(key, "")]
                expected = fallback if value is None else value
                actual = data[member]
                counts["mappedIntComparisons"] += 1
                if expected != actual:
                    differences.append({**label, "section": section, "key": key, "category": "integer_or_default",
                                        "raw": raw_fields.get(key), "nativeValue": expected, "unityValue": actual})
                else:
                    matched["mappedInt"] += 1
        for key in ("weapon_hit_sound", "weapon_drop_sound", "weapon_broken_sound"):
            counts["weaponSoundComparisons"] += 1
            if bmp.get(key, "") != data[key]:
                differences.append({**label, "section": "bmp", "key": key, "category": "sound_text",
                                    "nativeValue": bmp.get(key, ""), "unityValue": data[key]})
            else:
                matched["weaponSound"] += 1
        for key, value in stats.items():
            if key not in STATS_INTS:
                missing_stats.append({**label, "key": key, "raw": value, "nativeInt": integer[value],
                                      "scope": "EXCLUDED_PLATFORM_VIEWING" if key == "y" else "BATTLE_RULE_DATA",
                                      "carrier": "NO_CORRESPONDING_LF2CharacterData_FIELD_OR_STATS_CONTAINER"})
        if pairs(doc["stats"]) != pairs(entry["rawStats"]):
            raw_differences.append({**label, "section": "stats", "native": pairs(doc["stats"]), "unity": pairs(entry["rawStats"])})
        for key, member in SEQUENCES.items():
            sequence = next((seq for seq in doc.get("bmpSequences", []) if seq["name"] == key), None)
            expected = [] if sequence is None else sequence["actions"]
            counts["sequenceComparisons"] += 1
            if expected != data[member]:
                sequence_differences.append({**label, "key": key, "native": expected, "unity": data[member]})
        native_armors = doc.get("armor", [])
        counts["armorRecords"] += len(native_armors)
        if len(native_armors) != len(data["armors"]):
            armor_differences.append({**label, "nativeCount": len(native_armors), "unityCount": len(data["armors"])})
        for i, (armor, current) in enumerate(zip(native_armors, data["armors"])):
            expected = armor["normalized"]
            for key in ("type", "ratio", "decrease", "mp", "fall", "bdefend", "injury", "spark", "hp", "recover", "facing", "action", "reserve", "delay", "sound1", "sound2"):
                if expected.get(key) != current.get(key):
                    armor_differences.append({**label, "armor": i, "key": key, "native": expected.get(key), "unity": current.get(key)})
            for key, member in {"frame": "frame_ranges", "state": "states", "kind": "kinds", "id": "ids", "effect": "effects"}.items():
                actual = [[value["first"], value["last"]] for value in current[member]] if key == "frame" else current[member]
                if expected[key] != actual:
                    armor_differences.append({**label, "armor": i, "key": key, "native": expected[key], "unity": actual})
        if doc.get("weaponPiece"):
            piece_definitions.append({**label, "native": doc["weaponPiece"],
                                      "unityRawBlocks": [block for block in entry["rawBlocks"] if block["name"] == "weapon_piece"],
                                      "carrier": "NO_LF2CharacterData_WEAPON_PIECE_MEMBER"})
    save("value-differences.json", differences)
    save("raw-stats-differences.json", raw_differences)
    save("stats-carrier-gaps.json", missing_stats)
    save("armor-differences.json", armor_differences)
    save("sequence-differences.json", sequence_differences)
    save("weapon-piece-gaps.json", piece_definitions)
    save("bmp-carrier-gaps.json", bmp_carrier_gaps)
    summary = {"kind": "HEADER_AUDIT_NOT_PARITY", "definitions": len(definitions), "comparisons": dict(counts),
               "matches": dict(matched), "valueDifferenceCount": len(differences),
               "valueDifferencesByCategory": dict(Counter(row["category"] for row in differences)),
               "valueDifferencesByObjectType": dict(Counter(row["type"] for row in differences)),
               "statsCarrierGapDeclarations": len(missing_stats), "statsCarrierKeys": dict(Counter(row["key"] for row in missing_stats)),
               "statsCarrierScopes": dict(Counter(row["scope"] for row in missing_stats)),
               "rawStatsDifferences": len(raw_differences), "armorDifferences": len(armor_differences),
               "sequenceDifferences": len(sequence_differences), "weaponPieceDefinitions": len(piece_definitions),
               "bmpCarrierKeys": dict(Counter(row["key"] for row in bmp_carrier_gaps)),
               "bmpCarrierScopes": dict(Counter(row["scope"] for row in bmp_carrier_gaps))}
    save("comparison-summary.json", summary)
    save("audit-inputs.json", {"unityCaptureSha256": digest(ROOT / "unity-headers.json"), "nativeCaptureSha256": digest(fresh),
                               "integerExecutableSha256": digest(integer_exe), "doubleExecutableSha256": digest(double_exe),
                               "numericValues": len(values), "nativeBuildInputsMatched": True})
    print(json.dumps(summary, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
