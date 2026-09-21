"""Check bounded original-function captures; never treat this as Unity parity."""
import hashlib
import json
import re
import sys
from pathlib import Path


def validate(path):
    rows = [json.loads(line) for line in path.read_text(encoding="utf-8-sig").splitlines()]
    failures = []
    checks = 0

    def check(actual, expected, label):
        nonlocal checks
        checks += 1
        if actual != expected:
            failures.append({"label": label, "actual": actual, "expected": expected})

    check(len(rows), 23, "case count")
    for row in rows:
        name = row["name"]
        fields = dict(re.findall(r"(\w+):\s*(-?\d+)", row["sourceDat"].split("opoint_end:")[0]))
        point = lambda key: int(fields[key])
        count = point("facing") // 10 if point("facing") > 10 else 1
        exceptional = {
            "missing999_continues_record": (1, 1, 0, 0),
            "missing_oid_stops_frame": (0, 0, 1, 0),
            "full_capacity_stops_frame": (0, 0, 0, 1),
            "partial_multi_stops_frame": (2, 0, 0, 1),
            "unsupported_kind_keeps_later_record": (1, 0, 0, 0),
        }
        expected = exceptional.get(name, (count + 1, 0, 0, 0))
        result = row["result"]
        check(tuple(result[k] for k in ("spawned", "rejected", "missing", "noSlot")), expected, name + ": outcomes")
        check(result["success"], name != "missing999_continues_record", name + ": success")
        children = [e for e in row["after"]["entities"] if e["raw"]["identity"]["objectId"] == row["oid"]]
        check(len(row["after"]["entities"]), result["spawned"] + 1, name + ": occupied nonfiller slots")
        calls = row["calls"]["synchronized"]
        check(len(calls), 8 if name == "random_xyz_action" else 0, name + ": RNG count")
        check(row["calls"]["crt"], [], name + ": no CRT")
        for ordinal, child in enumerate(children):
            raw = child["raw"]
            scale = int(re.search(r"defend:\s*(-?\d+)", row["targetDat"]).group(1))
            scale = scale if scale > 0 and scale != 100 else 0
            continuation = [3, 123, 4, 31998] if row["type"] in (0, 5) and row["credit"] != 2 else [1, 0, 0, 0]
            render = point("effect") if row["kind"] == 1 and point("effect") > 0 else (11 if row["type"] == 0 else 0)
            credit = (row["credit"] if row["credit"] > -1 else 20) if row["type"] == 0 else -1
            check(child["extra"][:9], [render, 2, 1, credit, scale] + continuation, name + ": resource and continuation")
            check(raw["identity"]["ownerSlot"], 9, name + ": literal owner")
            check(raw["identity"]["battleGroup"], point("team") or 3, name + ": team override")
            check(child["extra"][12:15], [137, 137, 137 if row["kind"] == 2 else 17], name + ": display and weapon HP")
            initial = point("action")
            delta = [0, 0, 0, 0]
            if calls:
                for axis in range(4):
                    magnitude, sign = calls[axis * 2:axis * 2 + 2]
                    check(magnitude["callSite"], 0x0044D3AB, name + ": magnitude site")
                    check(sign["callSite"], 0x0044D3B5, name + ": sign site")
                    check(magnitude["upperBound"], 3 if axis == 3 else 5, name + ": amplitude")
                    check(sign["upperBound"], 50, name + ": sign bound")
                    delta[axis] = magnitude["result"] * (-1 if sign["result"] >= 25 else 1)
            check(raw["frame"]["action"], initial + delta[3], name + ": raw action randomizer")
            check(child["extra"][9:12], [initial, initial, 0], name + ": birth latch/history/counter")
            xyz = [308 + delta[0], -31 + delta[1], 256 + delta[2]]
            check(child["position"], xyz + xyz, name + ": position")
            spread = ordinal * 10.0 / (count - 1) - 5.0 if count > 1 else 0.0
            vx = -8 if name == "facing1_reverse" else 8
            vx += -spread if (vx > 0 and spread > 0) or (vx < 0 and spread < 0) else spread
            base_z = 0.0 if row["kind"] == 2 else 3.0
            if name == "depth211_up_multi":
                base_z = -2.5 * 0.25
            check(child["motion"], [vx, -2, base_z + spread], name + ": base depth then double spread")
            if row["kind"] == 2:
                check(child["links"][:2], [-1, 20], name + ": child parent link")
        if row["kind"] == 2 and children:
            parent = row["after"]["entities"][0]
            check(parent["links"][0], 101 if point("framea") == 1 else 1, name + ": parent relation")
            check(parent["links"][2], children[-1]["raw"]["slot"], name + ": last child wins")
        if "following" in row:
            check(row["followingLifecycleSuccess"], True, name + ": following lifecycle")
            if name == "random_xyz_action":
                selected = next(e for e in row["following"]["entities"] if e["raw"]["slot"] == 50)
                check(selected["raw"]["frame"]["actionLatch"], 2, name + ": following binds randomized action")
                check(selected["raw"]["frame"]["previousAction"], 2, name + ": following commits history")
    return {"status": "PASS" if not failures else "FAIL", "cases": len(rows), "checks": checks,
            "sha256": hashlib.sha256(path.read_bytes()).hexdigest().upper(), "failures": failures,
            "scope": "Bounded source formulas and recorded RNG deltas; not Unity comparison or independent RNG algorithm certification."}


if __name__ == "__main__":
    result = validate(Path(sys.argv[1]))
    text = json.dumps(result, ensure_ascii=False, indent=2)
    if len(sys.argv) > 2:
        Path(sys.argv[2]).write_text(text + "\n", encoding="utf-8")
    print(text)
    sys.exit(0 if result["status"] == "PASS" else 1)
