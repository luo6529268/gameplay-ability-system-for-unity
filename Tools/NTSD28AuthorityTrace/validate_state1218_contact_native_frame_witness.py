"""Independent immediate transition model; following tick is not modelled."""
import copy
import hashlib
import json
import sys
from pathlib import Path

from validate_ordinary_landing_native_frame_witness import damp, initial_entity


def validate(folder):
    data = (folder / "first.jsonl").read_bytes()
    rows = [json.loads(line) for line in data.splitlines()]
    failures = []
    checks = 0

    def check(expected, actual, path):
        nonlocal checks
        if isinstance(expected, dict) and isinstance(actual, dict):
            check(sorted(expected), sorted(actual), path + ".keys")
            for key, value in expected.items():
                check(value, actual.get(key), path + "." + key)
        elif isinstance(expected, list) and isinstance(actual, list):
            check(len(expected), len(actual), path + ".length")
            for i, (left, right) in enumerate(zip(expected, actual)):
                check(left, right, path + f"[{i}]")
        else:
            checks += 1
            if expected != actual:
                failures.append(dict(path=path, expected=expected, actual=actual))

    check(480, len(rows), "count")
    for i, row in enumerate(rows):
        p = row["params"]
        label = f"case{i}"
        entity = initial_entity(p)
        entity["pending"] = {key: p[key] for key in ("dx", "dy", "dz", "gain", "hitFacing", "picked", "picking")}
        check(entity, row["before"]["entities"][0], label + ".before")
        raw = entity["raw"]
        pos, motion = raw["position"], raw["motion"]
        pos["preciseX"] += motion["x"]
        pos["preciseZ"] += motion["z"]
        if pos["y"] >= p["floor"]:
            motion["x"] = damp(motion["x"])
            motion["z"] = damp(motion["z"])
        pos["preciseY"] += motion["y"]
        floor = min(p["floor"], 0)
        selected = p["action"]
        if pos["preciseY"] < floor:
            motion["y"] += 1.7
            if p["state"] == 12:
                base = 180 if selected < 185 else 186 if 185 < selected < 191 else None
                if base is not None:
                    selected = base + (0 if motion["y"] < -8 else 1 if motion["y"] < 1 else 2 if motion["y"] < 8 else 3)
            elif selected < 205 and motion["y"] > 1:
                selected = 205
        else:
            pos["preciseY"] = floor
            hard = p["state"] == 18 or motion["y"] > 11 or abs(motion["x"]) > 9
            if not hard:
                motion["y"] = 0
                motion["x"] /= 3
                selected = 231 if selected >= 186 else 230
                raw["frame"]["frameCounter"] = 0
            else:
                motion["y"] = -3.5
                motion["x"] = max(-7, min(7, motion["x"]))
                if p["gain"] == 1:
                    dx, dy, dz = p["dx"], p["dy"], p["dz"]
                    facing = p["hitFacing"] != 0
                    adjusted = -motion["x"] if facing else motion["x"]
                    if dx > 500:
                        motion["x"] = dx - 550
                    elif dx != 0 and ((dx >= 0 and adjusted < dx) or (dx < 0 and adjusted > dx)):
                        motion["x"] = -dx if facing else dx
                    if dy != 0:
                        motion["y"] = dy - 550 if dy > 500 else motion["y"] + dy
                    if dz != 0:
                        motion["z"] = dz - 550 if dz > 500 else motion["z"] + dz
                    for key in ("dx", "dy", "dz", "gain"):
                        entity["pending"][key] = 0
                    selected = p["picked"] if selected >= 186 and p["state"] != 18 else p["picking"]
                else:
                    selected = 185 if selected < 186 or p["state"] == 18 else 191
        raw["frame"]["action"] = selected
        original = selected == p["action"]
        declared = p["targetDeclarationRequested"] and selected == p["target"] and 0 <= selected <= 999
        available = 0 <= selected < 999 or declared or original
        state = p["state"] if original else 3 if declared else 0
        raw["frame"]["frameState"] = state
        entity.update(available=available, state=state, wait=100 if original else 37 if declared else 0,
                      next=p["action"] if original else 0)
        for axis in ("X", "Y", "Z"):
            entity["previous" + axis] = pos[axis.lower()]
            pos[axis.lower()] = int(pos["precise" + axis])
        expected = dict(entities=[entity], random=copy.deepcopy(row["before"]["random"]))
        check(expected, row["after"], label + ".after")
        check(dict(crt=[], synchronized=[]), row["calls"], label + ".calls")
        check(True, row["step"]["success"], label + ".success")
        check(False, row["step"]["environmentDamageApplied"], label + ".environment")
    report = dict(cases=len(rows), checks=checks, failures=failures, sha256=hashlib.sha256(data).hexdigest(),
                  scope="Independent initial entity and complete immediate entity transition; RNG unchanged from captured seed state. Step flags and following tick not independently modelled.")
    (folder / "independent-validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps({key: value for key, value in report.items() if key != "failures"}))
    print("failures", len(failures), failures[:3])
    return bool(failures)


if __name__ == "__main__":
    sys.exit(validate(Path(sys.argv[1])))
