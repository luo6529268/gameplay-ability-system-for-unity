"""Independent kind8 immediate transition model; following tick is not modeled."""
import argparse
import copy
import hashlib
import json
from pathlib import Path


def validate(folder):
    raw = (folder / "first.jsonl").read_bytes()
    rows = [json.loads(line) for line in raw.splitlines()]
    checks, failures = 0, []

    def equal(expected, actual, label):
        nonlocal checks
        if isinstance(expected, dict) and isinstance(actual, dict):
            equal(sorted(expected), sorted(actual), label + ".keys")
            for key in expected:
                equal(expected[key], actual.get(key), label + "." + key)
        elif isinstance(expected, list) and isinstance(actual, list):
            equal(len(expected), len(actual), label + ".length")
            for index, (left, right) in enumerate(zip(expected, actual)):
                equal(left, right, label + f"[{index}]")
        else:
            checks += 1
            if expected != actual:
                failures.append(f"{label}: expected {expected!r}, actual {actual!r}")

    equal(134, len(rows), "rows")
    equal(raw, (folder / "repeat.jsonl").read_bytes(), "doubleRun")
    applied = 0
    for index, row in enumerate(rows):
        label = f"case{index}"
        p = row["params"]
        equal(index, row["index"], label + ".index")
        equal(42, p["seed"], label + ".seed")
        expected = copy.deepcopy(row["before"])
        attacker, target = expected["entities"]
        for slot, entity, oid, counter, latch in [(0, attacker, 77, 7, 11), (70, target, 78, 8, 12)]:
            equal(slot, entity["raw"]["slot"], label + ".slot")
            equal(oid, entity["raw"]["identity"]["objectId"], label + ".oid")
            equal(10, entity["raw"]["frame"]["action"], label + ".initialAction")
            equal(counter, entity["raw"]["frame"]["frameCounter"], label + ".counter")
            equal(latch, entity["raw"]["frame"]["actionLatch"], label + ".latch")
            equal(41, entity["healTimer"], label + ".initialHealTimer")
            equal(497, entity["raw"]["vitals"]["currentMp"], label + ".initialMp")
        equal(not p["reject"], row["applied"], label + ".applied")
        if not p["reject"]:
            applied += 1
            if p["injury"] != 0:
                target["healTimer"] = p["injury"] + 1000
            target["raw"]["vitals"]["currentMp"] += p["gain"]
            action = p["action"]
            if action != 999:
                valid = 0 <= action <= 998
                state = 3 if p["declared"] else 0
                attacker["raw"]["frame"].update(action=action, frameState=state)
                attacker.update(available=valid, state=state, wait=37 if p["declared"] else 0,
                                next=10 if p["declared"] else 0)
            mode = p["dvy"] if -1 <= p["dvy"] <= 2 else 0
            if mode != -1:
                position = attacker["raw"]["position"]
                other = target["raw"]["position"]
                if mode != 1:
                    position["preciseX"] = other["preciseX"]
                if mode != 0:
                    position["preciseY"] = other["preciseY"]
                position["preciseZ"] = other["preciseZ"] + 1
        equal(expected, row["after"], label + ".after")
        equal({"crt": [], "synchronized": []}, row["calls"], label + ".calls")
    equal(132, applied, "appliedCount")
    report = dict(rows=len(rows), checks=checks, failures=failures, applied=applied,
                  sha256=hashlib.sha256(raw).hexdigest(),
                  scope="Independent immediate transition over complete captured before; selected initial prerequisites checked. Not a complete initial-state generator or following-tick/Unity validation.")
    (folder / "independent-validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(dict(rows=len(rows), checks=checks, failures=len(failures), applied=applied, firstFailures=failures[:8])))
    return bool(failures)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("folder", type=Path)
    raise SystemExit(validate(parser.parse_args().folder))
