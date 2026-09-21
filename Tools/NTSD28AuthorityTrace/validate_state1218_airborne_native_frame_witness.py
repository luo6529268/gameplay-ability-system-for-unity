"""Independent airborne entity transition; excludes following tick and step flags."""
import hashlib
import json
import sys
from pathlib import Path
from validate_ordinary_landing_native_frame_witness import initial_entity


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

    check(55, len(rows), "cases")
    for row in rows:
        p, label = row["params"], "case" + str(row["index"])
        e = initial_entity(p)
        e["raw"]["combat"]["environmentState"] = p["environment"]
        e.update(environment=p["environment"], environmentSource=-1)
        check(e, row["before"]["entities"][0], label + ".before")
        raw, action = e["raw"], p["action"]
        pos, motion = raw["position"], raw["motion"]
        for axis in "xyz":
            pos["precise" + axis.upper()] += motion[axis]
        check(True, pos["preciseY"] < min(0, p["floor"]), label + ".airborne")
        motion["y"] += 1.7
        post = motion["y"]
        selected = -1
        if p["state"] == 12:
            base = 180 if action < 185 else 186 if 185 < action < 191 else None
            if base is not None:
                if base == 180 and p["environment"] < 0:
                    selected = 182 if post < 12 and (p["resourcePhase12"] + 1) % 12 >= 6 else 181
                else:
                    selected = base + (0 if post < -8 else 1 if post < 1 else 2 if post < 8 else 3)
        elif p["state"] == 18 and action < 205 and post > 1:
            selected = 205
        check(selected, row["step"]["physics"]["selectedAction"], label + ".selection")
        check(post, p["computedPostVy"], label + ".computedPostVy")
        if selected >= 0:
            raw["frame"]["action"] = selected
        current = raw["frame"]["action"]
        original = current == action
        declared = bool(p["targetDeclarationRequested"]) and current == p["target"]
        state = p["state"] if original else 3 if declared else 0
        e.update(state=state, wait=100 if original else 37 if declared else 0, next=action if original else 0)
        raw["frame"]["frameState"] = state
        for axis in "xyz":
            e["previous" + axis.upper()] = pos[axis]
            pos[axis] = int(pos["precise" + axis.upper()])
        check(e, row["after"]["entities"][0], label + ".after")
        check(row["before"]["random"], row["after"]["random"], label + ".randomUnchanged")
        check(dict(crt=[], synchronized=[]), row["calls"], label + ".calls")
    result = dict(cases=len(rows), checks=checks, failures=failures, sha256=hashlib.sha256(data).hexdigest(),
                  scope="Initial and immediate entity projection plus selection and unchanged RNG; following and other step flags not independently modelled.")
    (folder / "independent-validation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps({k: v for k, v in result.items() if k != "failures"}))
    print("failures", len(failures), failures[:3])
    return bool(failures)


if __name__ == "__main__":
    sys.exit(validate(Path(sys.argv[1])))
