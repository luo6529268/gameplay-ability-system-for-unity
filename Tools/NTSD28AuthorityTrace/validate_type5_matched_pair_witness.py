"""Independent rules for the declared synthetic type5 matched/priority witnesses.

yResolved is source-only diagnostic state, not an asserted Unity carrier.
The oracle derives outcomes from params/before; it never uses after to predict
any state mutation. Spark geometry remains the preceding spark witness's scope.
"""
import collections
import copy
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "artifacts/diagnostics/NTSD28-Q06-TYPE5-MATCHED-PAIR-SOURCE-WITNESS-001"
checks = 0


def check(condition, index, contract):
    global checks
    checks += 1
    if not condition:
        raise AssertionError(f"case {index}: {contract}")


def equal(expected, actual, index, path):
    if isinstance(expected, dict):
        check(expected.keys() == actual.keys(), index, path + ".keys")
        for key, value in expected.items():
            equal(value, actual[key], index, path + "." + key)
    elif isinstance(expected, list):
        check(len(expected) == len(actual), index, path + ".length")
        for offset, value in enumerate(expected):
            equal(value, actual[offset], index, f"{path}[{offset}]")
    else:
        check(expected == actual, index, f"{path}: expected {expected!r}, actual {actual!r}")


def frame_state(dat, action):
    match = re.search(rf"<frame> {action} fixture\nstate: (-?\d+)", dat)
    return int(match[1]) if match else 0


def rest_write(expected, p):
    a, t, _ = expected["raw"]
    if p["recover"] not in (1, 3) and a["combat"]["motionHoldTimer"] >= 0:
        a["combat"]["motionHoldTimer"] = 3
    if p["recover"] not in (2, 3):
        t["combat"]["motionHoldTimer"] = -3
    a["combat"]["attackerRest"] = 0
    expected["rest"][1][0] = 4


def main():
    first = (OUTPUT / "first.jsonl").read_bytes()
    check(first == (OUTPUT / "repeat.jsonl").read_bytes(), -1, "repeat bytes")
    rows = [json.loads(line) for line in first.splitlines()]
    check(len(rows) == 138, -1, "case count")
    groups = collections.Counter()
    branches = collections.Counter()
    for index, row in enumerate(rows):
        check(row["index"] == index, index, "stable index")
        groups[row["group"]] += 1
        p, before, after = row["params"], row["before"], row["after"]
        expected = copy.deepcopy(before)
        a, t, third = expected["raw"]
        ae, te, third_e = expected["extra"]
        check(len(p["dat"]) == 3 and len(before["raw"]) == 3, index, "three definitions and entities")
        for slot in range(3):
            check(before["raw"][slot]["frame"]["action"] == p["current"], index, "current parameter")
            check(before["raw"][slot]["frame"]["actionLatch"] == p["latch"], index, "latch parameter")
            check(before["raw"][slot]["frame"]["previousAction"] == p["previous"], index, "previous parameter")
            check(before["raw"][slot]["frame"]["tickActionSnapshot"] == p["snapshot"], index, "snapshot parameter")
            check(before["extra"][slot]["count"] == slot + 1, index, "nonzero contribution sentinel")
        check(t["combat"]["collisionYReference"] == p["collisionYReference"], index, "reference parameter")
        check(t["identity"]["objectType"] == 5, index, "type5 target")
        check(a["identity"]["objectType"] == 3, index, "type3 attacker")
        matched = p["attackerState"] == p["targetState"] and p["targetState"] in (3005, 3006)
        branch = "feedback" if p["armor"] == 0 else "rest" if p["liveRest"] > 0 else "body" if p["body"] else "matched" if matched else "normal"
        branches[branch] += 1
        check(row["matched"] == (branch == "matched"), index, "matched gate follows armor/rest/body")
        check(row["status"] == (1 if branch == "rest" else 0), index, "native result status")
        released = -1
        if branch == "matched":
            rest_write(expected, p)
            for slot in (1, 0):
                action = (p["targetLatchUj"] if slot == 1 else p["latchUj"]) or 20
                expected["raw"][slot]["frame"]["action"] = action
                expected["raw"][slot]["frame"]["frameState"] = frame_state(p["dat"][slot], action)
                expected["raw"][slot]["frame"]["frameCounter"] = 0
                for axis in "xyz":
                    expected["extra"][slot][axis] = 0
            owner = 2 if p["relation"] in (2, 4) else None if p["relation"] == 3 else 0
            if owner == 2:
                third["combat"]["motionHoldTimer"] = a["combat"]["motionHoldTimer"]
            if owner is not None and expected["raw"][owner]["combat"]["motionHoldTimer"] > 0:
                expected["raw"][owner]["combat"]["motionHoldTimer"] *= -1
                released = owner
        elif branch == "body":
            t["frame"]["action"] = p["body"] - 1000
            t["frame"]["frameState"] = frame_state(p["dat"][1], t["frame"]["action"])
            t["identity"]["battleGroup"] = a["identity"]["battleGroup"]
            a["combat"]["motionHoldTimer"] = 3
            t["combat"]["motionHoldTimer"] = -3
        elif branch == "normal":
            injury = p["injury"]
            t["vitals"]["currentHp"] -= injury
            t["vitals"]["effectiveMaxHp"] -= int(injury / 3)
            te["hpConsumed"] += injury
            t["combat"]["bdefendAccumulator"] = 45
            timer = (80 if t["vitals"]["currentHp"] <= 0 else t["combat"]["hitReactionTimer"]) + (p["fall"] or 20)
            if p["previousState"] == 13 or p["snapshotState"] == 12:
                timer = 80
            above = p["targetY"] < p["collisionYReference"]
            action = p["current"]
            if timer > 60:
                timer = 80
            elif timer > 40:
                action, timer = 226, 80 if above else 60
            elif timer > 20:
                action, timer = 222, 80 if above else 40
            elif timer > 0:
                action, timer = 222 if above else 220, 20
            t["combat"]["hitReactionTimer"] = timer
            te["x"] += p["dvx"]
            te["z"] += p["dvz"]
            te["count"] += 1
            if timer == 80:
                te["y"] -= 7
                action = 180 if te["x"] >= 0 else 186
                if p["childRelation"] == 1:
                    expected["rest"][0][2] = 45
                    expected["rest"][1][2] = 30
            t["frame"]["action"] = action
            t["frame"]["frameState"] = frame_state(p["dat"][1], action)
            for key in ("statusDx", "statusDy", "statusDz", "statusFacing"):
                te[key] = 0
            te["statusGain"], te["statusPicked"], te["statusPicking"] = 1, 191, 185
            rest_write(expected, p)
        check(row["releasedSlot"] == released, index, "hold release result slot")
        equal(expected["raw"], after["raw"], index, "raw")
        equal(expected["extra"], after["extra"], index, "extra")
        equal(expected["rest"], after["rest"], index, "rest")
        emitting = branch in ("feedback", "normal")
        check(row["native"] == [], index, "no synchronized calls")
        equal(before["rng"]["synchronized"], after["rng"]["synchronized"], index, "synchronized state")
        if emitting:
            check(len(row["crt"]) == 2, index, "spark uses two CRT calls")
            check([c["result"] for c in row["crt"]] == [10265, 15515], index, "seed42 CRT outputs")
            crt = copy.deepcopy(before["rng"]["crt"])
            for call in row["crt"]:
                crt["state"] = (crt["state"] * 214013 + 2531011) & 0xffffffff
                crt["totalCalls"] += 1
                equal({"stateAfter": crt["state"], "totalCalls": crt["totalCalls"], "result": (crt["state"] >> 16) & 32767}, call, index, "CRT transition")
            equal(crt, after["rng"]["crt"], index, "CRT scalar")
            check([len(s) for s in after["sparks"]] == [0, 1, 0], index, "one target-owned spark")
        else:
            equal(before["rng"], after["rng"], index, "early RNG unchanged")
            equal(before["sparks"], after["sparks"], index, "early no spark")
            check(row["crt"] == [], index, "early no CRT calls")
        if branch == "normal":
            check(len(row["audio"]) == 1, index, "only attacker broken sound")
            check(row["audio"][0]["source"] == 4 and row["audio"][0]["x"] == 100 and row["audio"][0]["path"] == "matched_attacker.wav", index, "attacker sound identity")
        else:
            check(row["audio"] == [], index, "early no audio")

        # Separate explicit consumer step: preserve the independently checked
        # writer result and predict finalization from its actual accepted input.
        finalized = copy.deepcopy(after)
        for slot in range(3):
            raw, extra = finalized["raw"][slot], finalized["extra"][slot]
            raw["combat"]["motionHoldTimer"] = 0
            if extra["count"] > 0:
                for axis in "xyz":
                    raw["motion"][axis] = extra[axis] * 2 / (extra["count"] + 1)
            for axis in "xyz":
                extra[axis] = 0
            extra["count"], extra["yResolved"] = 0, False
        equal(finalized, row["afterReleasedHoldFinalize"], index, "releasedHoldFinalize")
    check(groups == {"matched": 72, "different_latch_owners": 2, "nonreciprocal_parent": 2, "matched_rest_hold": 24, "cross": 2, "current900": 2, "snapshot_not_current": 2, "priority": 16, "normal_frame_reference": 12, "normal_child": 4}, -1, "group coverage")
    report = {"status": "PASS", "scope": "SOURCE_MODEL_ONLY", "cases": len(rows), "checks": checks,
              "groups": dict(groups), "branches": dict(branches), "sha256": hashlib.sha256(first).hexdigest(),
              "bytes": len(first), "sourceOnlyFields": ["extra.yResolved"],
              "notCovered": ["other target types", "nonzero armor/broken fallback", "special effects", "full spark geometry", "real EXE input", "Unity runtime"]}
    (OUTPUT / "validation.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report))


if __name__ == "__main__":
    main()
