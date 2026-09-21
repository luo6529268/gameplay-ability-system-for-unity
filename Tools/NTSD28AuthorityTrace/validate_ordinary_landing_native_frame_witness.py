"""Independent params-to-before/immediate ordinary type0 physics model."""
import argparse
import copy
import hashlib
import json
from collections import Counter
from pathlib import Path


def initial_entity(p):
    raw = dict(slot=0, allocationEpoch=1, active=True,
        identity=dict(objectId=77, objectType=0, controlSlot=0, ownerSlot=-1, battleGroup=0, participantClass=0),
        frame=dict(action=p["action"], actionLatch=11, previousAction=p["action"], tickActionSnapshot=p["action"],
                   frameCounter=7, frameState=p["state"], facingLeft=False),
        position=dict(x=300, y=int(p["y"]), z=250, preciseX=300.25, preciseY=p["y"], preciseZ=250.5),
        motion=dict(x=p["vx"], y=p["vy"], z=p["vz"]),
        vitals=dict(currentHp=500, effectiveMaxHp=500, baseMaxHp=500, currentMp=497, baseMaxMp=497,
                    reviveLives=1, reviveNextLives=0, reviveNextHp=0),
        combat=dict(runtimeStateCode=0, renderPhase=0, attackerRest=0, collisionYReference=p["floor"],
                    platformSourceSlot=0, hitReactionTimer=0, bdefendAccumulator=0, runtimeArmorHp=0,
                    armorRecoveryTimer=-1, motionHoldTimer=0, weaponHp=0, specialHitLatch0eb=False,
                    environmentState=0, environmentSourceSlot=-1, objectAiExcludedGroupSourceSlot=-1),
        lifecycle=dict(resolutionPending=False, code=0))
    return dict(raw=raw, available=True, state=p["state"], wait=100, next=p["action"],
        snapshotAvailable=True, snapshotState=p["state"], previousX=280, previousY=-40, previousZ=230,
        input=dict(slot=0, allocationEpoch=1, objectId=77, input=dict(currentMask=0, previousMask=0,
            edgeWindow={key: 0 for key in ("attack", "jump", "defend", "right", "left", "up", "down")},
            defendReentryCooldown=0, comboState=[0]*10, proxyTail=0, keyHistory=[-1]*5, runAccumulator=0,
            lastAction=0, remapState=0, remapIndices=list(range(7)), boundState=0, globalRecordState=0)))


def damp(value):
    if value > 0.0001:
        value -= 1.0
        return 0.0 if value < 0.0001 else value
    if value < -0.0001:
        value += 1.0
        return 0.0 if value > -0.0001 else value
    return value


def validate(folder):
    raw_bytes = (folder / "first.jsonl").read_bytes()
    rows = [json.loads(line) for line in raw_bytes.splitlines()]
    checks, failures = 0, []

    def compare(expected, actual, label):
        nonlocal checks
        if isinstance(expected, dict) and isinstance(actual, dict):
            compare(sorted(expected), sorted(actual), label + ".keys")
            for key, value in expected.items():
                compare(value, actual.get(key), label + "." + key)
        elif isinstance(expected, list) and isinstance(actual, list):
            compare(len(expected), len(actual), label + ".length")
            for i, (left, right) in enumerate(zip(expected, actual)):
                compare(left, right, label + f"[{i}]")
        else:
            checks += 1
            if expected != actual:
                failures.append(f"{label}: expected={expected!r} actual={actual!r}")

    compare(True, raw_bytes == (folder / "repeat.jsonl").read_bytes(), "repeat")
    compare(186, len(rows), "rows")
    compare(dict(binding=102, priority=60, control=24), dict(Counter(r["params"]["group"] for r in rows)), "groups")
    state, table = 42, []
    for _ in range(3000):
        state = (state * 214013 + 2531011) & 0xffffffff
        table.append(((state >> 16) & 0x7fff) % 255 + 1)
    table.append(0)
    table_hash = 14695981039346656037
    for value in table:
        table_hash = ((table_hash ^ value) * 1099511628211) & 0xffffffffffffffff
    random = dict(crt=dict(state=state, totalCalls=3000), synchronized=dict(counter=0, index=0,
        tableHash64=f"{table_hash:016X}", lastCallSite=0, totalCalls=0))
    totals = Counter()
    for index, row in enumerate(rows):
        p, label = row["params"], f"case{index}"
        compare(index, row["index"], label + ".index")
        compare(42, p["seed"], label + ".seed")
        compare(False, p["state"] in (12,18), label + ".excludedStates")
        selected = 94 if p["state"] == 100 else 215 if p["action"] == 212 or p["state"] == 6 else p["hitG"] if p["hitG"] != 0 else 219
        compare(selected, p["target"], label + ".declaredTarget")
        expected = dict(entities=[initial_entity(p)], random=copy.deepcopy(random))
        compare(expected, row["before"], label + ".before")
        entity = expected["entities"][0]
        raw = entity["raw"]
        pos, motion = raw["position"], raw["motion"]
        flags = {key: False for key in ("stepped", "motionHoldAdvanced", "xIntegrated", "zIntegrated", "xBlocked", "zBlocked",
            "gravityApplied", "airborneActionSelected", "landingActionSelected", "landingFrameCounterReset", "state1218LandingResolved",
            "objectLandingResolved", "weaponHpUpdated", "facingFlipped", "landingSoundChannel4", "hitMotionConsumed", "hitMotionDeferred", "collisionFlagsConsumed")}
        flags.update(stepped=True, xIntegrated=motion["x"] != 0, zIntegrated=motion["z"] != 0,
                     collisionFlagsConsumed=True, weaponHpAfter=0, selectedAction=-1, vertical=4)
        pos["preciseX"] += motion["x"]
        pos["preciseZ"] += motion["z"]
        if pos["y"] >= p["floor"]:
            motion["x"] = damp(motion["x"])
            motion["z"] = damp(motion["z"])
        old_y = pos["preciseY"]
        pos["preciseY"] += motion["y"]
        floor = p["floor"] if p["floor"] < 0 else 0
        if pos["preciseY"] < floor:
            motion["y"] += 1.7
            flags.update(gravityApplied=True, vertical=0)
        else:
            pos["preciseY"] = floor
            if old_y < floor:
                motion["y"] = 0.0
                motion["x"] /= 3.0
                flags.update(landingActionSelected=True, landingFrameCounterReset=True, selectedAction=selected, vertical=1)
                valid = 0 <= selected <= 998 or selected == 999 and p["targetDeclared"]
                frame_state = 3 if p["targetDeclared"] else 0
                raw["frame"].update(action=selected, frameCounter=0, frameState=frame_state)
                entity.update(available=valid, state=frame_state, wait=37 if p["targetDeclared"] else 0, next=0)
                totals["landings"] += 1
            else:
                flags["vertical"] = 2
                totals["contactWithoutLanding"] += 1
        if flags["gravityApplied"]:
            totals["airborne"] += 1
        entity.update(previousX=pos["x"], previousY=pos["y"], previousZ=pos["z"])
        pos.update(x=int(pos["preciseX"]), y=int(pos["preciseY"]), z=int(pos["preciseZ"]))
        compare(expected, row["after"], label + ".after")
        compare(dict(success=True, slot=0, message="", environmentDamageApplied=False, environmentDamage=0,
                     physics=flags, audio=[]), row["step"], label + ".step")
        compare(dict(crt=[], synchronized=[]), row["calls"], label + ".calls")
    report = dict(rows=len(rows), checks=checks, failures=failures, totals=dict(totals),
                  sha256=hashlib.sha256(raw_bytes).hexdigest(),
                  scope="Independent full before and immediate ordinary type0 physics model from params and fixed initial state, including previousXYZ/descriptor/B2/RNG/all step fields. Following tick is captured but not independently modeled; Unity previousXYZ mapping not asserted.")
    (folder / "independent-validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(dict(rows=len(rows), checks=checks, failures=len(failures), totals=dict(totals), firstFailures=failures[:8])))
    return bool(failures)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("folder", type=Path)
    raise SystemExit(validate(parser.parse_args().folder))
