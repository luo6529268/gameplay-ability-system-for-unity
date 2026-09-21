"""Focused post-effect frame access oracle, not a full hit/following model."""
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

    check(6, len(rows), "cases")
    for row in rows:
        p, label = row["params"], "case" + str(row["index"])
        previous_state = p["previousState"] if p["previousDeclared"] else 0
        rejected = p["effect"] == 20 and previous_state in (18, 19)
        check(not rejected, row["hitAttempted"], label + ".hitAttempted")
        check(0 if rejected else 1, row["candidateCount"], label + ".candidateCount")
        check(1 if rejected else 0, row["directEffectRejections"], label + ".directEffectRejections")
        if rejected:
            check(None, row["postEffectAction"], label + ".noHitResult")
            check(row["before"], row["after"], label + ".noHitMutation")
            continue
        action = -1
        if p["effect"] in (3, 30) and previous_state != 13:
            action = 200
        elif p["effect"] in (2, 21, 22) or (p["effect"] == 20 and previous_state != 18):
            action = 203
        check(action, row["postEffectAction"], label + ".postEffectAction")
        check(-1, row["attackerCatchingActionOverride"], label + ".attackerOverride")
        for i in (0, 1):
            before = row["before"]["entities"][i]
            after = row["after"]["entities"][i]
            old, frame = before["raw"]["frame"], after["raw"]["frame"]
            for key in ("actionLatch", "previousAction", "tickActionSnapshot"):
                check(old[key], frame[key], label + f".entity{i}." + key)
            check(0 if i == 1 and action > 0 else old["frameCounter"], frame["frameCounter"], label + f".entity{i}.counter")
            check(before["snapshotAvailable"], after["snapshotAvailable"], label + f".entity{i}.snapshotAvailable")
            check(before["snapshotState"], after["snapshotState"], label + f".entity{i}.snapshotState")
            check(p["hp"] - (p["injury"] if i == 1 else 0), after["raw"]["vitals"]["currentHp"], label + f".entity{i}.fixedHp")
        target = row["after"]["entities"][1]
        if action > 0:
            check(action, target["raw"]["frame"]["action"], label + ".action")
            check(True, target["available"], label + ".available")
            check(0, target["state"], label + ".state")
            check(41 if p["targetDeclared"] else 0, target["wait"], label + ".wait")
            check(203 if p["targetDeclared"] else 0, target["next"], label + ".next")
            if action == 203:
                check(target["pendingX"] >= 0, target["raw"]["frame"]["facingLeft"], label + ".facing")
        else:
            check(False, target["raw"]["frame"]["action"] in (200, 203), label + ".suppressed")
    result = dict(cases=len(rows), checks=checks, failures=failures, sha256=hashlib.sha256(data).hexdigest(),
                  scope="Independent direct post-effect selection, native descriptor, frame history/counter, fixed injury5 HP and selected203 facing. Other full-hit side effects, RNG and following are not independently modelled.")
    (folder / "focused-independent-validation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "failures"}))
    print("failures", len(failures), failures[:5])
    return bool(failures)


if __name__ == "__main__":
    sys.exit(validate(Path(sys.argv[1])))
