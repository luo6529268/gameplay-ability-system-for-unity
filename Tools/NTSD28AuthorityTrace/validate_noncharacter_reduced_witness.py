"""Independent arithmetic checks for the base reduced witness; not full-scope acceptance."""
import copy
import hashlib
import json
import math
import sys
from pathlib import Path

ROOT = Path("artifacts/diagnostics/NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001") / (sys.argv[1] if len(sys.argv)>1 else "owner987")

def main():
    first = (ROOT / "first.jsonl").read_bytes()
    assert first == (ROOT / "repeat.jsonl").read_bytes()
    rows = [json.loads(line) for line in first.splitlines()]
    assert len(rows) == {"base24": 24, "motion273": 273, "resource675": 675, "relation747": 747, "action843": 843, "owner987": 987}[ROOT.name]
    checks = 0
    for row in rows:
        p, before, after = row["params"], row["before"], row["after"]
        # States70/75 defend from either direction before type1 armor selection.
        defended = p["defense"] or (p.get("current", 0) != 0 and p.get("currentState", 4) in (70, 75))
        def equal(actual, expected, label):
            nonlocal checks
            assert actual == expected, (row["index"], label, actual, expected)
            checks += 1
        def compare(actual, expected, label):
            if isinstance(expected, dict):
                equal(set(actual), set(expected), label + " keys")
                for key in expected:
                    compare(actual[key], expected[key], label + "." + key)
            elif isinstance(expected, list):
                equal(len(actual), len(expected), label + " length")
                for i, value in enumerate(expected):
                    compare(actual[i], value, label + "." + str(i))
            elif isinstance(expected, float):
                equal(math.isclose(actual, expected, rel_tol=1e-13, abs_tol=1e-12), True, label)
            else:
                equal(actual, expected, label)
        finalized = copy.deepcopy(after)
        for slot in range(3):
            raw, extra = finalized["raw"][slot], finalized["extra"][slot]
            raw["combat"]["motionHoldTimer"] = 0
            if extra["count"] > 0:
                for axis in ("x", "y", "z"):
                    raw["motion"][axis] = extra[axis] * 2.0 / (extra["count"] + 1)
            for axis in ("x", "y", "z"):
                extra[axis] = 0.0
            extra["count"] = 0
            extra["yResolved"] = False
        compare(row["afterReleasedHoldFinalize"], finalized, "finalizer")
        equal(row["status"], 0, "applied")
        equal(row["selectedArmorType"], -1 if defended else 1, "selected armor")
        mp_setting, hp_setting = p.get("armorMp", 0), p.get("armorHp", 0)
        decrease = p.get("decrease", 50)
        reduced = -decrease if decrease <= 0 else int(p["injury"] * decrease / 100)
        mp_damage = (-mp_setting if mp_setting < 0 else int(reduced * mp_setting / 100)) if mp_setting else 0
        cost = mp_damage if mp_damage else 1
        bypass = p.get("bdefend", 0) > p.get("ratio", 15)
        broken = not mp_setting and hp_setting != 0 and p.get("runtimeArmorHp", 3) <= p["injury"]
        available = p.get("currentMp", 410) >= cost if mp_setting else not broken
        if "armorDecision" in row:
            equal(row["armorDecision"], 0 if defended else 2 if bypass else 1, "armor selection")
            if not defended and not bypass:
                equal(row["activationAvailable"], available, "activation boundary")
                equal(row["activationMpCost"], cost if mp_setting else 0, "activation MP cost")
                equal(row["armorBroken"], broken, "activation HP broken")
        crt = copy.deepcopy(before["rng"]["crt"])
        draws = []
        equal(len(row["crt"]), 2, "spark CRT count")
        for call in row["crt"]:
            crt["state"] = (crt["state"] * 214013 + 2531011) & 0xffffffff
            crt["totalCalls"] += 1
            draw = (crt["state"] >> 16) & 32767
            draws.append(draw)
            compare(call, dict(stateAfter=crt["state"], totalCalls=crt["totalCalls"], result=draw), "CRT event")
        compare(after["rng"]["crt"], crt, "CRT scalar")
        sync = copy.deepcopy(before["rng"]["synchronized"])
        if p.get("attackerState", 0) == 1002:
            sync.update(counter=1, index=1, lastCallSite=0xF3, totalCalls=1)
            compare(row["native"], [dict(callSite=0xF3, upperBound=16, result=3, counterAfter=1, indexAfter=1, totalCalls=1)], "synchronized event")
        else:
            compare(row["native"], [], "no synchronized event")
        compare(after["rng"]["synchronized"], sync, "synchronized scalar")
        ap, tp = before["raw"][0]["position"], before["raw"][1]["position"]
        base_x = max(ap["x"] + 3 - 40, tp["x"]) if p.get("facing", False) else min(ap["x"] - 3 + 40, tp["x"])
        base_y = ap["y"] + 30 - 20 - 5
        top = tp["y"] - 5
        if base_y < top:
            base_y = int((base_y + top) / 2)
        elif base_y > tp["y"]:
            base_y = int((base_y + tp["y"]) / 2)
        spark_id = 99 if not defended and not bypass and available else 10
        expected_sparks = [[], [dict(host=1, id=spark_id, x=base_x + draws[1] % 9 - 4,
                                    y=base_y + tp["z"] + draws[0] % 9 - 4)], []]
        compare(after["sparks"], expected_sparks, "spark geometry and route")
        if not defended and (bypass or not available):
            # Preserve the semantic distinction between a broken-armor pointer and pre-existing -1.
            expected_armor_hp = 0 if broken and not bypass else before["raw"][1]["combat"]["runtimeArmorHp"]
            equal(after["raw"][1]["combat"]["runtimeArmorHp"], expected_armor_hp, "fallback broken pointer")
            hp_damage = 0 if p["type"] == 6 else p["injury"]
            equal(after["raw"][1]["vitals"]["currentHp"], before["raw"][1]["vitals"]["currentHp"] - hp_damage, "fallback raw HP damage")
            equal(after["raw"][1]["vitals"]["effectiveMaxHp"], before["raw"][1]["vitals"]["effectiveMaxHp"] - int(hp_damage / 3), "fallback HP bound")
            equal(after["raw"][1]["vitals"]["currentMp"], before["raw"][1]["vitals"]["currentMp"], "fallback no armor MP charge")
            # Remaining unarmored response is retained as source output, without claiming an independent full-tail proof.
            continue
        damage = 0 if p["type"] == 6 else int(p["injury"] / 10) if defended else 0 if mp_setting else reduced
        expected_mp_damage = 0 if p["type"] == 6 or defended else mp_damage
        b, a = before["raw"][1], after["raw"][1]
        equal(a["vitals"]["currentHp"], b["vitals"]["currentHp"] - damage, "HP")
        equal(a["vitals"]["effectiveMaxHp"], b["vitals"]["effectiveMaxHp"] - int(damage / 3), "HP bound")
        equal(a["vitals"]["currentMp"], b["vitals"]["currentMp"] - expected_mp_damage, "MP")
        durability = p["injury"] if p["type"] in (1, 2, 4, 6) else 0
        equal(a["combat"]["weaponHp"], b["combat"]["weaponHp"] - durability, "durability")
        armor_delta = -p["injury"] if p["type"] != 6 and not defended and hp_setting > 0 else 0
        equal(a["combat"]["runtimeArmorHp"], b["combat"]["runtimeArmorHp"] + armor_delta, "armor HP mutation")
        equal(after["extra"][1]["mpConsumed"], before["extra"][1]["mpConsumed"] + expected_mp_damage, "MP consumption")
        equal(a["frame"]["frameCounter"], 0, "counter")
        state = p.get("attackerState", 0)
        sign = -1.0 if p.get("facing", False) else 1.0
        air = p.get("targetY", 0) > p.get("reference", 0)
        reaction = p.get("reaction", 30)
        target_vx = p.get("targetVx", 5.0)
        attacker_x = before["raw"][0]["position"]["x"]
        target_x = b["position"]["x"]
        if air:
            impulse = sign * (6.0 if reaction == 80 and -6 < target_vx < 6 and p["dvx"] < 6 else p["dvx"])
        elif reaction == 80 and -3 < target_vx < 3 and p["dvx"] == 0:
            impulse = (6.0 if attacker_x < target_x else -6.0) if state == 2000 else sign
        elif state == 2000:
            impulse = p["dvx"] if attacker_x < target_x else -p["dvx"]
        else:
            impulse = sign * p["dvx"] / 2.0
        equal(after["extra"][1]["x"], before["extra"][1]["x"] + impulse, "horizontal")
        ba, aa = before["raw"][0], after["raw"][0]
        if state == 1002:
            equal(len(row["native"]), 1, "one reduced random call")
            equal(row["native"][0]["callSite"], 0xF3, "reduced callsite")
            equal(row["native"][0]["upperBound"], 16, "random bound")
            equal(aa["frame"]["action"], row["native"][0]["result"], "raw random action")
            equal(aa["motion"]["x"], -after["extra"][1]["x"] * 0.5, "post X")
            equal(aa["motion"]["y"], -4, "post Y")
            equal(math.isclose(aa["motion"]["z"], ba["motion"]["z"] / -1.5, abs_tol=1e-12), True, "post Z")
        else:
            equal(row["native"], [], "no synchronized RNG")
            away = state == 2000 and ((attacker_x > target_x and ba["motion"]["x"] >= 0) or (attacker_x < target_x and ba["motion"]["x"] <= 0))
            for axis in ("x", "z"):
                equal(aa["motion"][axis], ba["motion"][axis] / (2.5 if away else 1), "post damping " + axis)
        equal(after["extra"][1]["count"], before["extra"][1]["count"] + 1, "contribution")
        equal(after["extra"][1]["hpConsumed"], before["extra"][1]["hpConsumed"] + damage, "HP consumption")
        for slot in range(3):
            for key in ("statusDx", "statusDy", "statusDz", "statusGain", "statusFacing", "statusPicked", "statusPicking"):
                equal(after["extra"][slot][key], before["extra"][slot][key], "status unchanged")
        equal(row["audio"], [], "no noncharacter reduced sound")
        expected = copy.deepcopy(before)
        et = expected["raw"][1]
        et["vitals"]["currentHp"] -= damage
        et["vitals"]["effectiveMaxHp"] -= int(damage / 3)
        et["vitals"]["currentMp"] -= expected_mp_damage
        et["combat"]["weaponHp"] -= durability
        et["combat"]["runtimeArmorHp"] += armor_delta
        if et["combat"]["runtimeArmorHp"] <= 0:
            et["combat"]["bdefendAccumulator"] += p.get("itrBdefend", 35)
        if et["vitals"]["currentHp"] <= 0:
            et["combat"]["hitReactionTimer"] = 80
        if not air:
            threshold = 30 if defended else max(p.get("ratio", 15), 30)
            if et["combat"]["bdefendAccumulator"] > threshold and et["frame"]["frameState"] in (7, 70, 75):
                et["frame"]["action"] = 112
                et["frame"]["frameState"] = 0
            elif et["frame"]["action"] == 110:
                et["frame"]["action"] = 111
                et["frame"]["frameState"] = 0
        et["frame"]["frameCounter"] = 0
        delay = p.get("delay", -1)
        if defended or delay == -1:
            et["combat"]["motionHoldTimer"] = -5
            expected["raw"][0]["combat"]["motionHoldTimer"] = 3
        else:
            low = delay - int(delay / 100) * 100
            middle = int(delay / 100)
            middle -= int(middle / 100) * 100
            et["combat"]["motionHoldTimer"] -= low
            expected["raw"][0]["combat"]["motionHoldTimer"] += middle
        if p.get("parent", -1) == 2:
            expected["raw"][2]["combat"]["motionHoldTimer"] = expected["raw"][0]["combat"]["motionHoldTimer"]
        expected["rest"][1][0] = 4
        ee = expected["extra"][1]
        ee["x"] += impulse
        ee["count"] += 1
        ee["hpConsumed"] += damage
        ee["mpConsumed"] += expected_mp_damage
        ea = expected["raw"][0]
        if state == 1002:
            ea["frame"]["action"] = 3  # First synchronized seed42 value, also asserted against the event below.
            equal(row["native"][0]["result"], 3, "seed42 first draw")
            ea["frame"]["frameState"] = 0
            ea["motion"]["x"] = -ee["x"] * 0.5
            ea["motion"]["y"] = -4.0
            ea["motion"]["z"] /= -1.5
        elif state == 2000 and away:
            ea["motion"]["x"] /= 2.5
            ea["motion"]["z"] /= 2.5
        owner = p.get("owner", -1)
        resource_slot = 0 if owner == -1 else 2 if owner == 2 else -1
        gain = p.get("gain", 0)
        if p["type"] != 6 and resource_slot >= 0:
            vitals = expected["raw"][resource_slot]["vitals"]
            if gain < 0:
                if -gain <= vitals["currentMp"]:
                    vitals["currentMp"] += gain
                    expected["extra"][resource_slot]["mpConsumed"] -= gain
            elif vitals["currentMp"] + gain <= vitals["baseMaxMp"]:
                vitals["currentMp"] += gain
        for section in ("raw", "extra", "rest"):
            compare(after[section], expected[section], "reduced." + section)
    result = dict(status="PASS", scope="PARTIAL_FALLBACK_TRANSACTION_PENDING", cases=len(rows), checks=checks,
                  sha256=hashlib.sha256(first).hexdigest(), bytes=len(first))
    (ROOT / "validation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps(result))

if __name__ == "__main__":
    main()
