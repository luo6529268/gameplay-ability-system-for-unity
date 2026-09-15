"""Independent source242 refill before/immediate model; no following-tick oracle."""
import argparse
import copy
import hashlib
import json
from collections import Counter
from pathlib import Path


def initial_entity(p, child):
    slot, oid = (70, p["childOid"]) if child else (0, 77)
    state = 3 if child else p["holderState"]
    x, y, z = (450, -35, 250) if child else (300, 0, 250)
    raw = dict(slot=slot, allocationEpoch=1, active=True,
        identity=dict(objectId=oid, objectType=p["type"] if child else 0,
                      controlSlot=0, ownerSlot=-1, battleGroup=0, participantClass=0),
        frame=dict(action=10, actionLatch=11 if child else 9, previousAction=10,
                   tickActionSnapshot=10, frameCounter=7 if child else 5,
                   frameState=state, facingLeft=child),
        position=dict(x=x, y=y, z=z, preciseX=x, preciseY=y, preciseZ=z),
        motion=dict(x=2 if child else 0, y=-3 if child else 0, z=4 if child else 0),
        vitals=dict(currentHp=p["childHp"] if child else p["parentHp"],
                    effectiveMaxHp=500 if child else p["parentHpBound"], baseMaxHp=500,
                    currentMp=p["childMp"] if child else p["parentMp"], baseMaxMp=500,
                    reviveLives=1, reviveNextLives=0, reviveNextHp=0),
        combat=dict(runtimeStateCode=0, renderPhase=0, attackerRest=0, collisionYReference=0,
                    platformSourceSlot=0, hitReactionTimer=0, bdefendAccumulator=0,
                    runtimeArmorHp=0, armorRecoveryTimer=-1, motionHoldTimer=0,
                    weaponHp=17 if child else 0, specialHitLatch0eb=False,
                    environmentState=0, environmentSourceSlot=-1, objectAiExcludedGroupSourceSlot=-1),
        lifecycle=dict(resolutionPending=False, code=0))
    inputs = dict(slot=slot, allocationEpoch=1, objectId=oid, input=dict(
        currentMask=0, previousMask=0,
        edgeWindow={key: 0 for key in ("attack", "jump", "defend", "right", "left", "up", "down")},
        defendReentryCooldown=0, comboState=[0]*10, proxyTail=0, keyHistory=[-1]*5,
        runAccumulator=0, lastAction=0, remapState=0, remapIndices=list(range(7)),
        boundState=0, globalRecordState=0))
    return dict(raw=raw, link=-1 if child else 1, parent=0, child=0 if child else 70,
                available=True, state=state, wait=83 if child else 100, next=10,
                snapshotAvailable=True, snapshotState=state, input=inputs,
                ordinaryCreditGate2F4=p["credit2F4"] if child else -1,
                hpConsumed=19 if child else 13, mpConsumed=23 if child else 17)


def validate(folder):
    first = (folder / "first.jsonl").read_bytes()
    rows = [json.loads(line) for line in first.splitlines()]
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

    compare(True, first == (folder / "repeat.jsonl").read_bytes(), "repeat")
    compare(242, len(rows), "rows")
    compare(dict(depletion=84, caps=72, credit=18, non17=24, zero_binding=32, ordinary_oid=12),
            dict(Counter(row["params"]["group"] for row in rows)), "groups")
    state, table = 42, []
    for _ in range(3000):
        state = (state * 214013 + 2531011) & 0xffffffff
        table.append(((state >> 16) & 0x7fff) % 255 + 1)
    table.append(0)
    table_hash = 14695981039346656037
    for value in table:
        table_hash = ((table_hash ^ value) * 1099511628211) & 0xffffffffffffffff
    initial_random = dict(crt=dict(state=state, totalCalls=3000), synchronized=dict(
        counter=0, index=0, tableHash64=f"{table_hash:016X}", lastCallSite=0, totalCalls=0))
    totals = Counter()
    for index, row in enumerate(rows):
        p, label = row["params"], f"case{index}"
        compare(index, row["index"], label + ".index")
        compare(42, p["seed"], label + ".seed")
        compare(20, p["action"], label + ".initialWpointAction")
        compare(True, p["declared"], label + ".initialWpointDeclared")
        compare(p["parentZeroDeclared"], "<frame> 0 " in row["parentDat"], label + ".parentZeroDefinition")
        compare(p["childZeroDeclared"], "<frame> 0 " in row["childDat"], label + ".childZeroDefinition")
        expected = dict(entities=[initial_entity(p, False), initial_entity(p, True)],
                        random=copy.deepcopy(initial_random))
        compare(expected, row["before"], label + ".before")
        parent, child = expected["entities"]
        pv, cv = parent["raw"]["vitals"], child["raw"]["vitals"]
        calls = []

        def draw(site, upper):
            sync = expected["random"]["synchronized"]
            sync["counter"] = (sync["counter"] + 1) % 1234
            sync["index"] = (sync["index"] + 1) % 3000
            sync["lastCallSite"] = site
            sync["totalCalls"] += 1
            result = (table[sync["index"]] + sync["counter"]) % upper
            calls.append(dict(callSite=site, upperBound=upper, result=result,
                              counterAfter=sync["counter"], indexAfter=sync["index"],
                              totalCalls=sync["totalCalls"]))
            return result

        hp_refill = p["childOid"] == 122
        mp_refill = p["childOid"] == 123
        charging = p["holderState"] == 17 and (hp_refill or mp_refill)
        exhausted, hp_updates, mp_updates = False, 0, 0
        if charging:
            if hp_refill and cv["currentHp"] > 0:
                cv["currentHp"] -= 1
                hp_updates = 1
                if cv["currentHp"] % 5 == 0:
                    pv["effectiveMaxHp"] = min(pv["effectiveMaxHp"] + 2, pv["baseMaxHp"])
                    pv["currentHp"] = min(pv["currentHp"] + 4, pv["effectiveMaxHp"])
                if cv["currentHp"] % 6 == 0:
                    pv["currentMp"] = min(pv["currentMp"] + 5, 500)
                exhausted = cv["currentHp"] < 1
            if not exhausted and mp_refill:
                cv["currentHp"] -= 2
                pv["currentMp"] = min(pv["currentMp"] + 3, 500)
                if child["ordinaryCreditGate2F4"] >= 0 and cv["currentMp"] > 150:
                    cv["currentMp"] = 150
                mp_updates = 1
                exhausted = cv["currentHp"] < 1
        if exhausted:
            parent["link"] = child["link"] = 0
            parent["child"] = child["parent"] = 0
            for entity, declared, wait in ((parent, p["parentZeroDeclared"], 19),
                                          (child, p["childZeroDeclared"], 23)):
                entity["raw"]["frame"].update(action=0, frameCounter=0, frameState=0)
                entity.update(state=0, wait=wait if declared else 0, next=0)
            child["raw"]["motion"]["y"] = 0
            child["raw"]["motion"]["x"] = draw(0x004181C9 if hp_refill else 0x004182C0, 7) - 3
            child["raw"]["combat"]["weaponHp"] = 0
        else:
            raw = child["raw"]
            raw["position"] = dict(x=320, y=-2, z=251, preciseX=320, preciseY=-2, preciseZ=251)
            raw["frame"].update(action=20, frameState=3, facingLeft=False)
            child.update(state=3, wait=37, next=20)
            if p["kind"] == 3:
                parent["link"] = child["link"] = 0
                action = draw(0x00418726, 6)
                vx, vy, vz = draw(0x0041873A, 7)-3, -draw(0x00418756, 4), draw(0x00418772, 5)-2
                raw["motion"] = dict(x=vx, y=vy, z=vz)
                raw["frame"].update(action=action, frameState=0)
                child.update(state=0, wait=23 if action == 0 and p["childZeroDeclared"] else 0, next=0)
        held = dict(success=True, linked=1, updates=int(not exhausted), unsupported=0, terminal=0,
                    velocityReleases=0, kind3Releases=int(not exhausted and p["kind"] == 3),
                    charging=int(charging), hpRefills=hp_updates, mpRefills=mp_updates,
                    exhausted=int(exhausted), diagnostics=[])
        compare(expected, row["after"], label + ".after")
        compare(dict(crt=[], synchronized=calls), row["calls"], label + ".calls")
        compare(held, row["held"], label + ".held")
        totals.update({key: value for key, value in held.items() if type(value) is int})
    report = dict(rows=len(rows), checks=checks, failures=failures, totals=dict(totals),
                  sha256=hashlib.sha256(first).hexdigest(),
                  scope="Independent complete before and immediate refill raw/links/descriptors/B2/2F4/consumption/RNG model. Following tick captured by source but not independently modeled here.")
    (folder / "independent-validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(dict(rows=len(rows), checks=checks, failures=len(failures), totals=dict(totals), firstFailures=failures[:8])))
    return bool(failures)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("folder", type=Path)
    raise SystemExit(validate(parser.parse_args().folder))
