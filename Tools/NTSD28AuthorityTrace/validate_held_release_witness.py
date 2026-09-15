"""Independent immediate held-release model; following driver state is separate."""
import argparse
import copy
import hashlib
import json
from pathlib import Path


def validate(folder):
    first = (folder / "first.jsonl").read_bytes()
    rows = [json.loads(line) for line in first.splitlines()]
    checks, failures = 0, []

    def compare(expected, actual, label):
        nonlocal checks
        if isinstance(expected, dict) and isinstance(actual, dict):
            for key, value in expected.items():
                compare(value, actual.get(key), label + "." + key)
        elif isinstance(expected, list) and isinstance(actual, list):
            compare(len(expected), len(actual), label + ".length")
            for index, (left, right) in enumerate(zip(expected, actual)):
                compare(left, right, label + f"[{index}]")
        else:
            checks += 1
            if expected != actual:
                failures.append(f"{label}: expected={expected!r} actual={actual!r}")

    compare(True, first == (folder / "repeat.jsonl").read_bytes(), "repeat")
    compare(1150, len(rows), "rows")
    state, table = 42, []
    for _ in range(3000):
        state = (state * 214013 + 2531011) & 0xffffffff
        table.append(((state >> 16) & 0x7fff) % 255 + 1)
    table.append(0)
    table_hash = 14695981039346656037
    for value in table:
        table_hash = ((table_hash ^ value) * 1099511628211) & 0xffffffffffffffff
    initial_random = {
        "crt": {"state": state, "totalCalls": 3000},
        "synchronized": {"counter": 0, "index": 0, "tableHash64": f"{table_hash:016X}",
                         "lastCallSite": 0, "totalCalls": 0},
    }
    for index, row in enumerate(rows):
        label, p = f"case{index}", row["params"]
        compare(index, row["index"], label + ".index")
        compare(42, p["seed"], label + ".seed")
        random, calls = copy.deepcopy(initial_random), []
        compare(random, row["before"]["random"], label + ".initialRandom")

        def draw(site, upper):
            sync = random["synchronized"]
            sync["counter"] = (sync["counter"] + 1) % 1234
            sync["index"] = (sync["index"] + 1) % 3000
            sync["lastCallSite"] = site
            sync["totalCalls"] += 1
            result = (table[sync["index"]] + sync["counter"]) % upper
            calls.append(dict(callSite=site, upperBound=upper, result=result,
                              counterAfter=sync["counter"], indexAfter=sync["index"],
                              totalCalls=sync["totalCalls"]))
            return result

        expected = copy.deepcopy(row["before"])
        parent, child = expected["entities"]
        raw = child["raw"]
        compare(1, parent["link"], label + ".initialHolderLink")
        compare(70, parent["child"], label + ".initialHolderChild")
        compare(-1, child["link"], label + ".initialChildLink")
        compare(0, child["parent"], label + ".initialChildParent")
        mask = (4 if p["depth"] & 1 else 0) | (8 if p["depth"] & 2 else 0)
        compare(mask, parent["input"]["input"]["currentMask"], label + ".depthCurrent")
        compare(mask, parent["input"]["input"]["previousMask"], label + ".depthPrevious")
        # Geometry uses the selected WPoint before the later release action.
        anchor_x = 289 if p["facing"] else 311
        offset = 9 if p["declared"] else 0
        x = anchor_x - offset if p["facing"] else anchor_x + offset
        y = -19 + (18 if p["declared"] else 0) + (-1 if p["cover"] == 0 else 1)
        z = 250 + (1 if p["cover"] == 0 else -1)
        raw["position"] = dict(x=x, y=y, z=z, preciseX=x, preciseY=y, preciseZ=z)
        raw["frame"]["facingLeft"] = p["facing"]
        raw["combat"]["motionHoldTimer"] = parent["raw"]["combat"]["motionHoldTimer"]
        action = p["action"]
        velocity_release = p["dvx"] != 0 and p["type"] in (1, 2, 4, 6)
        if velocity_release:
            action = draw(0x0041865E, 6) if p["type"] == 2 else 40
            raw["motion"]["x"] = -p["dvx"] if p["facing"] else p["dvx"]
            raw["motion"]["y"] = p["dvy"]
            if p["depth"] == 1:
                raw["motion"]["z"] = -p["dvz"]
            elif p["depth"] == 2:
                raw["motion"]["z"] = p["dvz"]
            if p["type"] in (1, 4, 6):
                raw["combat"]["objectAiExcludedGroupSourceSlot"] = 0
            parent["link"] = child["link"] = 0
        if p["kind"] == 3:
            action = draw(0x00418726, 6)
            vx, vy, vz = draw(0x0041873A, 7) - 3, -draw(0x00418756, 4), draw(0x00418772, 5) - 2
            raw["motion"] = dict(x=p["dvx"] if p["dvx"] else vx,
                                 y=p["dvy"] if p["dvy"] else vy,
                                 z=p["dvz"] if p["dvz"] else vz)
            parent["link"] = child["link"] = 0
        released = velocity_release or p["kind"] == 3
        state_value = 0 if released or not p["declared"] else 3
        wait = (17 if p["releaseDeclared"] else 0) if released else (37 if p["declared"] else 0)
        next_action = 0 if released or not p["declared"] else action
        raw["frame"].update(action=action, frameState=state_value)
        child.update(available=True, state=state_value, wait=wait, next=next_action)
        expected["random"] = random
        compare(expected, row["after"], label + ".after")
        compare({"crt": [], "synchronized": calls}, row["calls"], label + ".calls")
        compare(dict(success=True, linked=1, updates=1, unsupported=0, terminal=0,
                     velocityReleases=int(velocity_release), kind3Releases=int(p["kind"] == 3),
                     charging=0, hpRefills=0, mpRefills=0, exhausted=0, diagnostics=[]),
                row["held"], label + ".held")

    report = dict(rows=len(rows), checks=checks, failures=failures,
                  sha256=hashlib.sha256(first).hexdigest(),
                  scope="Full immediate release raw/links/descriptors/input/RNG model. Following tick captured, not independently modeled here.")
    (folder / "independent-validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(dict(rows=len(rows), checks=checks, failures=len(failures), firstFailures=failures[:8])))
    return bool(failures)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("folder", type=Path)
    raise SystemExit(validate(parser.parse_args().folder))
