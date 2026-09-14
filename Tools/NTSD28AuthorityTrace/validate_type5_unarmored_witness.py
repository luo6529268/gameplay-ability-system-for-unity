"""Validate the declared type5 source fixture against its native scalar contracts."""
import collections
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "artifacts/diagnostics/NTSD28-Q06-TYPE5-UNARMORED-SOURCE-WITNESS-001"
checks = 0


def check(value, index, contract):
    global checks
    checks += 1
    assert value, f"case {index}: {contract}"


def main():
    first = (OUTPUT / "first.jsonl").read_bytes()
    check(first == (OUTPUT / "repeat.jsonl").read_bytes(), -1, "repeat bytes")
    rows = [json.loads(line) for line in first.splitlines()]
    check(len(rows) == 585, -1, "fixture count")
    for row in rows:
        i = row["index"]
        attacker, target = row["after"]["raw"]
        extra = row["after"]["extra"]
        check(row["type"] == 5 and row["status"] == 0, i, "applied type5")
        injury = int(row["injury"] * 100 / row["scale"])
        if row["weak"] > 0:
            injury = int(injury / 2)
        hp = row["hp"] - injury
        check(target["vitals"]["currentHp"] == hp, i, "signed HP")
        check(target["vitals"]["effectiveMaxHp"] == row["hp"] - int(injury / 3), i, "HP bound")
        check(extra[1]["hpConsumed"] == injury, i, "HP consumption")
        check(target["combat"]["weaponHp"] == 20, i, "type5 has no weapon durability subtraction")
        reaction = (80 if hp <= 0 else 29) + (row["fall"] if row["fall"] != 0 else 20)
        action = 0
        directional = 224 if row["targetFacing"] == row["attackerFacing"] else 222
        if reaction > 60:
            reaction = 80
        elif reaction > 40:
            action, reaction = 226, 80 if row["targetY"] < 0 else 60
        elif reaction > 20:
            action, reaction = directional, 80 if row["targetY"] < 0 else 40
        elif reaction > 0:
            action, reaction = directional if row["targetY"] < 0 else 220, 20
        check(target["combat"]["hitReactionTimer"] == reaction, i, "native reaction tier")
        sign = -1 if row["attackerFacing"] else 1
        x = row["px"]
        if reaction == 80 and -5 < row["vx"] < 5 and row["dvx"] == 0:
            x += sign * 5
        elif row["attackerState"] == 2000:
            x += row["dvx"] if 100 < row["targetX"] else -row["dvx"]
        else:
            x += sign * row["dvx"]
        y = row["py"]
        if reaction == 80:
            if row["dvy"] != 0:
                y += row["dvy"]
                if int(row["targetY"] + y) > 0:
                    y = 12
            else:
                y -= 7
            action = 180 if (x >= 0 if row["targetFacing"] else x <= 0) else 186
        check(extra[1]["x"] == x and extra[1]["y"] == y and extra[1]["z"] == 1.25, i, "pending impulse")
        check(extra[1]["count"] == 3, i, "type5 contribution count")
        check(target["frame"]["action"] == action, i, "target action")
        check(target["frame"]["frameCounter"] == 7 and target["frame"]["actionLatch"] == 17, i, "target counters preserved")
        check(target["identity"]["battleGroup"] == 2 and target["frame"]["facingLeft"] == bool(row["targetFacing"]), i, "target identity")
        check(target["combat"]["bdefendAccumulator"] == 45, i, "Bdefend")
        check(row["after"]["rest"] == [[0, 0], [1, 0]], i, "ordinary rest only")
        check(len(row["crt"]) == 2 and [c["result"] for c in row["crt"]] == [10265, 15515], i, "spark CRT")
        check([len(v) for v in row["after"]["sparks"]] == [0, 1], i, "single spark")
        check(len(row["audio"]) == (1 if row["attackerType"] == 3 else 0), i, "no generic type5 hurt sound")
        if row["audio"]:
            check(row["audio"][0]["source"] == 4 and row["audio"][0]["x"] == 100 and row["audio"][0]["path"] == "weapon_reaction_attacker.wav", i, "attacker broken audio")
        if row["attackerState"] == 1002:
            check(len(row["native"]) == 1 and row["native"][0]["callSite"] == 0xEE and row["native"][0]["upperBound"] == 16, i, "native post random")
            check(attacker["frame"]["action"] == row["native"][0]["result"] and attacker["frame"]["frameCounter"] == 5, i, "raw attacker action")
            check(attacker["motion"] == {"x": -x * .5, "y": -4, "z": 5}, i, "1002 motion")
        else:
            check(len(row["native"]) == 0, i, "no native random")
            post = row["attackerState"] == 3000 or (row["attackerState"] == 3007 and row["cover"] in (2, 3))
            check(attacker["frame"]["action"] == (10 if post else 0) and attacker["frame"]["frameCounter"] == (0 if post else 5), i, "single attacker post")
            check(attacker["motion"] == ({"x": 0, "y": 6, "z": 7} if post else {"x": 4, "y": 6, "z": 5}), i, "post motion")
        check(attacker["frame"]["actionLatch"] == 13, i, "attacker latch")
        final = row["afterReleasedHoldFinalize"]
        for slot in (0, 1):
            prior = extra[slot]
            check(final["raw"][slot]["motion"] == {axis: prior[axis] * 2 / (prior["count"] + 1) for axis in ("x", "y", "z")}, i, "released hold finalization")
            check(all(final["extra"][slot][axis] == 0 for axis in ("count", "x", "y", "z")), i, "accumulator clearing")
    report = {"status": "PASS", "cases": len(rows), "assertions": checks, "sha256": hashlib.sha256(first).hexdigest(), "groups": dict(collections.Counter(r["group"] for r in rows)), "scope": "Source model only; synthetic frozen candidate and explicit hold release."}
    (OUTPUT / "validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report))


if __name__ == "__main__":
    main()
