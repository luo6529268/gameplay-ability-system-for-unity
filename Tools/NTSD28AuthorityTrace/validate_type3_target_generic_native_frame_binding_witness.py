"""Focused generic response/binding and matched-pair model; not full hit/tick."""
import hashlib
import json
import sys
from pathlib import Path


def validate(folder):
    data = (folder / "first.jsonl").read_bytes()
    rows = [json.loads(line) for line in data.splitlines()]
    checks, failures = 0, []

    def check(expected, actual, path):
        nonlocal checks
        checks += 1
        if expected != actual:
            failures.append(dict(path=path, expected=expected, actual=actual))

    check(11, len(rows), "cases")
    for row in rows:
        p, label = row["params"], "case" + str(row["index"])
        selected = p["selected"] or (30 if p["attackerType"] == 0 else 20)
        pair = p["pair"]
        check(0, row["status"], label + ".appliedStatus")
        check(True, row["lifecycleSuccess"], label + ".reportedLifecycleFlag")
        check(selected, row["targetType3PostHitAction"], label + ".intermediateAction")
        check(pair, row["matchedProjectilePairReset"], label + ".pairReset")
        check(71 if pair else -1, row["targetProjectilePairAction"], label + ".targetPairAction")
        check(70 if pair else -1, row["attackerProjectilePairAction"], label + ".attackerPairAction")
        for i in (0, 1):
            before, after = row["before"]["entities"][i], row["after"]["entities"][i]
            old, frame = before["raw"]["frame"], after["raw"]["frame"]
            action = (70 if i == 0 else 71) if pair else old["action"] if i == 0 else selected
            available = True if pair or i == 0 else 0 <= action < 999 or (p["declared"] and action == 999)
            check(action, frame["action"], label + f".entity{i}.action")
            check(0 if pair or i == 1 else old["frameCounter"], frame["frameCounter"], label + f".entity{i}.counter")
            for key in ("actionLatch", "previousAction", "tickActionSnapshot"):
                check(old[key], frame[key], label + f".entity{i}." + key)
            check(available, after["available"], label + f".entity{i}.available")
            check(0, after["state"], label + f".entity{i}.state")
            wait = 100 if pair or i == 0 else 41 if p["declared"] else 0
            next_frame = action if pair or i == 0 or p["declared"] else 0
            check(wait, after["wait"], label + f".entity{i}.wait")
            check(next_frame, after["next"], label + f".entity{i}.next")
            for axis in ("X", "Y", "Z"):
                check(0 if pair or i == 1 else before["pending" + axis], after["pending" + axis], label + f".entity{i}.pending" + axis)
            check(before["pendingCount"] + (1 if i == 1 else 0), after["pendingCount"], label + f".entity{i}.pendingCount")
            check(395 if i == 1 else 400, after["raw"]["vitals"]["currentHp"], label + f".entity{i}.fixedHp")
        target = row["after"]["entities"][1]["raw"]
        source = row["before"]["entities"][0]["raw"]["identity"]
        for key in ("battleGroup", "ownerSlot"):
            check(source[key], target["identity"][key], label + ".target." + key)
        check(0, target["identity"]["controlSlot"], label + ".target.controlSlot")
        check(True, target["combat"]["specialHitLatch0eb"], label + ".target.specialLatch")
    result = dict(cases=len(rows), checks=checks, failures=failures, sha256=hashlib.sha256(data).hexdigest(),
                  scope="Generic response action, native descriptor, history/counter, direct ownership and pending count/totals; real matched-pair final actions and fixed HP. Not a full hit/RNG/following independent model.")
    (folder / "focused-independent-validation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "failures"}))
    print("failures", len(failures), failures[:6])
    return bool(failures)


if __name__ == "__main__":
    sys.exit(validate(Path(sys.argv[1])))
