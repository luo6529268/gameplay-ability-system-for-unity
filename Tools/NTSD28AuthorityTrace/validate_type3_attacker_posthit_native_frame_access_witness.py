"""Independent selected-frame transition model, not a full hit/tick model."""
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

    check(13, len(rows), "cases")
    for row in rows:
        p, label = row["params"], "case" + str(row["index"])
        before, after = row["before"]["entities"][0], row["after"]["entities"][0]
        old, frame = before["raw"]["frame"], after["raw"]["frame"]
        applies = p["attackerState"] == 3000 or (p["attackerState"] == 3007 and p["cover"] in (2, 3))
        action = (p["selected"] or 10) if applies else old["action"]
        available = (0 <= action < 999 or (p["declared"] and action == 999)) if applies else True
        declared = p["declared"] if applies else True
        check(0, row["status"], label + ".appliedStatus")
        check(True, row["lifecycleSuccess"], label + ".reportedLifecycleFlag")
        check(action if applies else -1, row["attackerPostHitAction"], label + ".postHitAction")
        check(action, frame["action"], label + ".action")
        check(0 if applies else old["frameCounter"], frame["frameCounter"], label + ".counter")
        for key in ("actionLatch", "previousAction", "tickActionSnapshot"):
            check(old[key], frame[key], label + "." + key)
        check(available, after["available"], label + ".available")
        check(0 if applies else p["attackerState"], after["state"], label + ".state")
        check((41 if declared else 0) if applies else 100, after["wait"], label + ".wait")
        check((action if declared else 0) if applies else 20, after["next"], label + ".next")
        check(0.0 if applies else before["raw"]["motion"]["x"], after["raw"]["motion"]["x"], label + ".motionX")
        z = (p["selectedDvx"] if declared else 0) if applies and available else before["raw"]["motion"]["z"]
        check(z, after["raw"]["motion"]["z"], label + ".motionZ")
        check(before["raw"]["motion"]["y"], after["raw"]["motion"]["y"], label + ".motionY")
        check(398 if p["armorRequested"] else 395, row["after"]["entities"][1]["raw"]["vitals"]["currentHp"], label + ".fixedFixtureHp")
        if p["armorRequested"]:
            check(1, row["selectedArmorType"], label + ".selectedArmorType")
            check(1, row["armorDecision"], label + ".armorApplies")
            check(True, row["activationAvailable"], label + ".activationAvailable")
            check(False, row["armorBroken"], label + ".armorBroken")
            check(0, row["activationMpCost"], label + ".activationMpCost")
    result = dict(cases=len(rows), checks=checks, failures=failures, sha256=hashlib.sha256(data).hexdigest(),
                  scope="Selected-frame action/descriptor/counter/history and motion XYZ transition, fixed fixture HP and reported armor activation/status/lifecycle flags. No independent full hit side-effect, RNG or following model.")
    (folder / "focused-independent-validation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "failures"}))
    print("failures", len(failures), failures[:6])
    return bool(failures)


if __name__ == "__main__":
    sys.exit(validate(Path(sys.argv[1])))
