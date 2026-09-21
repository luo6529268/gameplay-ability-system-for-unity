"""Focused effect8 gate/binding oracle, not a full damage or following model."""
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

    check(16, len(rows), "cases")
    for row in rows:
        p, label = row["params"], "case" + str(row["index"])
        suppressed = p["latch"] == 900 and p["historyDeclared"] and (
            p["historyState"] in (602, 603) or p["historyBody"] in (50, 52))
        matched = p["picked"] <= 0 or (p["previous"] == 900 and p["historyDeclared"] and p["historyState"] == p["picked"])
        allowed = not suppressed and matched
        hp = p["hp"] - 5
        actions = [p["catching"] if allowed and p["catching"] > 0 else -1,
                   p["caught"] if allowed and p["caught"] > 0 and hp > 0 else -1]
        check(actions[0], row["attackerCatchingActionOverride"], label + ".attackerOverride")
        check(actions[1], row["postEffectAction"], label + ".targetOverride")
        check(hp, row["after"]["entities"][1]["raw"]["vitals"]["currentHp"], label + ".fixedFixtureHp")
        for i, selected in enumerate(actions):
            before = row["before"]["entities"][i]
            after = row["after"]["entities"][i]
            old, frame = before["raw"]["frame"], after["raw"]["frame"]
            for key in ("frameCounter", "actionLatch", "previousAction", "tickActionSnapshot"):
                check(old[key], frame[key], label + f".entity{i}." + key)
            check(before["snapshotAvailable"], after["snapshotAvailable"], label + f".entity{i}.snapshotAvailable")
            check(before["snapshotState"], after["snapshotState"], label + f".entity{i}.snapshotState")
            if selected <= 0:
                check(10 if i == 0 else 220 if hp > 0 else 186, frame["action"], label + f".entity{i}.supportAction")
                continue
            declared = p["attackerDeclared" if i == 0 else "targetDeclared"]
            check(selected, frame["action"], label + f".entity{i}.action")
            check(0 <= selected < 999 or (declared and selected <= 999), after["available"], label + f".entity{i}.available")
            check(3 if declared else 0, after["state"], label + f".entity{i}.state")
            check((37 if i == 0 else 41) if declared else 0, after["wait"], label + f".entity{i}.wait")
            check(0, after["next"], label + f".entity{i}.next")
    result = dict(cases=len(rows), checks=checks, failures=failures, sha256=hashlib.sha256(data).hexdigest(),
                  scope="Fixed effect8 fixture gate/override/descriptor and frame-history invariants. Fixed injury5 HP only; full damage side effects, RNG and following not independently modelled.")
    (folder / "focused-independent-validation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "failures"}))
    print("failures", len(failures), failures[:4])
    return bool(failures)


if __name__ == "__main__":
    sys.exit(validate(Path(sys.argv[1])))
