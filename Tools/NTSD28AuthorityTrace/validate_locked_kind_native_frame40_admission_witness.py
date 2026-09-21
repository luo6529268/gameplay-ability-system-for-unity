"""Focused locked-kind identity/frame40 model; not a full hit/tick model."""
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

    check(2, len(rows), "cases")
    for row in rows:
        label = "case" + str(row["index"])
        attacker = row["before"]["entities"][0]
        before, after = row["before"]["entities"][1], row["after"]["entities"][1]
        frame = after["raw"]["frame"]
        for key, value in (("status", 0), ("transformApplied", True),
                           ("ownershipTransferred", True), ("kindEffect", 209),
                           ("kindResponseAction", 40), ("targetType3PostHitAction", 40),
                           ("matchedProjectilePairReset", False), ("lifecycleSuccess", True)):
            check(value, row[key], label + "." + key)
        for key in ("objectId", "objectType", "battleGroup", "ownerSlot"):
            check(attacker["raw"]["identity"][key], after["raw"]["identity"][key], label + ".identity." + key)
        check(before["raw"]["identity"]["controlSlot"], after["raw"]["identity"]["controlSlot"], label + ".controlPreserved")
        for key in ("action", "actionLatch", "previousAction"):
            check(40, frame[key], label + ".frame." + key)
        check(before["raw"]["frame"]["tickActionSnapshot"], frame["tickActionSnapshot"], label + ".snapshotIdPreserved")
        check(0, frame["frameCounter"], label + ".counter")
        check(True, after["available"], label + ".available")
        check(0, after["state"], label + ".state")
        check(41 if row["params"]["declared40"] else 0, after["wait"], label + ".wait")
        check(40 if row["params"]["declared40"] else 0, after["next"], label + ".next")
        check(True, after["snapshotAvailable"], label + ".newDefinitionSnapshotAvailable")
        check(0, after["snapshotState"], label + ".newDefinitionSnapshotState")
        check(True, after["raw"]["combat"]["specialHitLatch0eb"], label + ".specialLatch")
        for axis in ("X", "Y", "Z"):
            check(0, after["pending" + axis], label + ".pending" + axis)
        check(before["pendingCount"] + 1, after["pendingCount"], label + ".contributionPreserved")
        check(395, after["raw"]["vitals"]["currentHp"], label + ".fixedHp")
        for axis in ("x", "y", "z"):
            check(before["raw"]["motion"][axis], after["raw"]["motion"][axis], label + ".motion." + axis)
    result = dict(cases=len(rows), checks=checks, failures=failures, sha256=hashlib.sha256(data).hexdigest(),
                  scope="Locked catalog transform flags, identity/frame history, native40 and new-definition snapshot descriptor, pending count/totals, fixed HP and preserved motion. Not independent full damage/RNG/following.")
    (folder / "focused-independent-validation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "failures"}))
    print("failures", len(failures), failures[:6])
    return bool(failures)


if __name__ == "__main__":
    sys.exit(validate(Path(sys.argv[1])))
