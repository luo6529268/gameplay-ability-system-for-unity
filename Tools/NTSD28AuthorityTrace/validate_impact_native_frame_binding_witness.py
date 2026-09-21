"""Independent impact transition from captured before; no following-tick model."""
import copy
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
        if isinstance(expected, dict) and isinstance(actual, dict):
            check(sorted(expected), sorted(actual), path + ".keys")
            for k, v in expected.items():
                check(v, actual.get(k), path + "." + k)
        elif isinstance(expected, list) and isinstance(actual, list):
            check(len(expected), len(actual), path + ".length")
            for i, (a, b) in enumerate(zip(expected, actual)):
                check(a, b, path + f"[{i}]")
        else:
            checks += 1
            if expected != actual:
                failures.append(dict(path=path, expected=expected, actual=actual))

    check(20, len(rows), "count")
    for row in rows:
        p, label = row["params"], "case" + str(row["index"])
        expected = copy.deepcopy(row["before"])
        target = expected["entities"][3]
        raw = target["raw"]
        typ = raw["identity"]["objectType"]
        kind = p["kind"]
        applied = not ((kind == 11 and target["environment"] >= 0) or (kind == 18 and typ == 0))
        check(applied, row["applied"], label + ".applied")
        if applied:
            action = None
            if typ == 0:
                check(1, expected["entities"][0]["raw"]["identity"]["ownerSlot"], label + ".owner1")
                check(2, expected["entities"][1]["raw"]["identity"]["ownerSlot"], label + ".owner2")
                target["environment"] = -20
                raw["combat"]["environmentState"] = -20
                target["catchSource"] = 8194
                target["impactSource"] = 0
                action = p["respond"] or 182
            elif target["state"] != (2000 if typ == 2 else 1000):
                action = 0
            for axis in ("x", "z"):
                raw["motion"][axis] /= 1.07
                target["pending" + axis.upper()] = raw["motion"][axis]
            if action is not None:
                declared = p["declared"] and 0 <= action <= 999
                available = 0 <= action < 999 or declared
                state = 3 if declared else 0
                raw["frame"]["action"] = action
                raw["frame"]["frameState"] = state
                target.update(available=available, state=state, wait=37 if declared else 0, next=0)
            if raw["position"]["y"] >= -2:
                raw["position"]["y"] = -2
                raw["position"]["preciseY"] = -2.0
                raw["motion"]["y"] = -6.0
            if raw["motion"]["y"] > -6:
                raw["motion"]["y"] -= 3.0 if typ == 0 else 2.3
                target["pendingY"] = raw["motion"]["y"]
        check(expected, row["after"], label + ".after")
        check(dict(crt=[], synchronized=[]), row["calls"], label + ".calls")
    result = dict(cases=len(rows), checks=checks, failures=failures, sha256=hashlib.sha256(data).hexdigest(),
                  scope="Independent impact transition from captured before and params, all four after entities and unchanged RNG. Spawn initial generation and following tick not modelled.")
    (folder / "independent-validation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "failures"}))
    print("failures", len(failures), failures[:5])
    return bool(failures)


if __name__ == "__main__":
    sys.exit(validate(Path(sys.argv[1])))
