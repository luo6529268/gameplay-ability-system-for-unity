"""Focused first-BDY action contract; not a model of the whole following tick."""
import json
import sys
from pathlib import Path

rows = [json.loads(line) for line in Path(sys.argv[1]).read_text(encoding="utf-8").splitlines()]
cases = [(1069, 69, -1, False), (1900, 900, -1, True),
         (2900, 900, -1, False), (1999, -1, -1, False),
         (1009008570, 900, 857, False), (1009008570, 900, 857, True),
         (1009990690, 999, 69, False), (1033, 33, -1, True)]
checks = 0


def equal(actual, expected, label):
    global checks
    checks += 1
    assert actual == expected, f"{label}: {actual!r} != {expected!r}"


equal(len(rows), len(cases), "case count")
for index, (row, case) in enumerate(zip(rows, cases)):
    kind, target, attacker, declared = case
    encoded = kind >= 1000000000
    equal(row["index"], index, "index")
    equal(row["params"]["bodyKind"], kind, "kind")
    equal(row["params"]["declared"], declared, "declared")
    equal(row["encodedApplied"] if encoded else row["firstBodyApplied"], True, "response applied")
    equal(row["lifecycleSuccess"], True, "following lifecycle only")
    equal(len(row["following"]["entities"]), 2, "following capture only")
    if encoded:
        equal(row["encodedChance"], 0, "chance")
        equal(row["encodedEffect"], 0, "effect")
        equal(row["encodedRngConsumed"], False, "no encoded roll")
    for side, requested in enumerate((attacker, target)):
        before = row["before"]["entities"][side]
        after = row["after"]["entities"][side]
        old = before["raw"]
        current = after["raw"]
        writes = requested < 999 if encoded else side == 1
        action = requested if writes else old["frame"]["action"]
        equal(current["frame"]["action"], action, f"{index}/{side} action")
        for field in ("actionLatch", "previousAction", "tickActionSnapshot"):
            equal(current["frame"][field], old["frame"][field], f"{index}/{side} {field}")
        equal(current["frame"]["frameCounter"], 0 if encoded and writes else old["frame"]["frameCounter"], "counter")
        equal(after["available"], action >= 0, "descriptor availability")
        if writes:
            equal(after["wait"], 41 if declared else 0, "descriptor wait")
            equal(after["next"], action if declared else 0, "descriptor next")
        equal(current["vitals"]["currentHp"], 400, "no ordinary damage")
        equal(current["identity"]["battleGroup"], 1 if not encoded and side == 1 else old["identity"]["battleGroup"], "group")
        hold = (3 if side == 0 else -3) if 1000 <= kind < 1999 else 0
        equal(current["combat"]["motionHoldTimer"], hold, "hold")

print(json.dumps({"status": "PASS", "cases": len(rows), "checks": checks,
                  "scope": "Immediate first-BDY action/descriptor/history/group/hold/HP only; not full prelude/RNG/following model."}))
