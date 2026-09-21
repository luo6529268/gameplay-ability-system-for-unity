"""Independent visible-state fusion contract; following-tick behavior is capture only."""
import copy
import json
import sys
from pathlib import Path

checks = 0


def compare(actual, expected, label):
    global checks
    if isinstance(expected, dict):
        assert isinstance(actual, dict), label
        for key, value in expected.items():
            compare(actual[key], value, label + "." + key)
    elif isinstance(expected, list):
        assert len(actual) == len(expected), label
        for index, value in enumerate(expected):
            compare(actual[index], value, label + f"[{index}]")
    else:
        checks += 1
        assert actual == expected, f"{label}: {actual!r} != {expected!r}"


def publish(entity, oid, role):
    raw = entity["raw"]
    raw["identity"]["objectId"] = oid
    raw["identity"]["objectType"] = 0
    entity["input"]["objectId"] = oid
    entity["aiProfile"] = 31 + role
    entity["dropMode"] = 1 + role
    raw["vitals"]["baseMaxMp"] = 600 + role * 100
    raw["combat"]["weaponHp"] = 21 + role
    entity["incomingScale"] = (110 + role * 20) * entity["modeScale"] // 100


def action(entity, frame, declared=True):
    raw = entity["raw"]["frame"]
    raw.update(action=frame, actionLatch=frame, tickActionSnapshot=frame,
               frameCounter=0, frameState=0)
    entity.update(available=True, state=0, wait=41 if declared else 0,
                  next=frame if declared else 0, snapshotAvailable=True,
                  snapshotState=0, opointLatch=-1, soundLatch=-1)


rows = [json.loads(line) for line in Path(sys.argv[1]).read_text(encoding="utf-8").splitlines()]
compare(len(rows), 4, "count")
for index, row in enumerate(rows):
    compare(row["index"], index, "index")
    record = row["catalogRecord"]
    compare([record[k] for k in ("id1", "id2", "id3", "action", "frame")],
            [10, 11, 52, 310, 112] if index == 1 else [7, 8, 51, 290, 112], "formal record")
    before = row["beforeMerge"]
    if index == 2:
        compare(row["mergeResult"]["fused"], 0, "strict boundary")
        compare(row["afterMerge"], before, "rejected merge unchanged")
        continue
    compare(row["mergeResult"]["fused"], 1, "merged")
    expected = copy.deepcopy(before)
    primary, partner = expected["entities"]
    raw, other = primary["raw"], partner["raw"]
    bound = min(raw["vitals"]["effectiveMaxHp"] + other["vitals"]["effectiveMaxHp"],
                raw["vitals"]["baseMaxHp"])
    hp = min(raw["vitals"]["currentHp"] + other["vitals"]["currentHp"], bound)
    raw["vitals"].update(currentHp=hp, effectiveMaxHp=bound, currentMp=record["mp"])
    for axis in ("x", "z"):
        midpoint = int((raw["position"][axis] + other["position"][axis]) / 2)
        raw["position"][axis] = midpoint
        raw["position"]["precise" + axis.upper()] = midpoint
    raw["motion"]["x"] = 0
    primary.update(gate328=1, fusionPartnerSlot=1, fusionPrimaryId=record["id1"],
                   fusionPartnerId=record["id2"], fusionTimer=record["decrease"],
                   fusionDisplayTimer=record["decrease"], reviveVisual318=0)
    if record["hitJa"] == 1:
        primary["gate194"] = 1
    action(primary, record["action"], index != 1)
    publish(primary, record["id3"], 2)
    expected["entities"][1] = None
    compare(row["afterMerge"], expected, "merge visible state")
    primary["fusionTimer"] = 0
    compare(row["beforeDefuse"], expected, "explicit timer intervention")
    if index == 3:
        compare(row["defuseResult"]["success"], False, "missing definition fails")
        compare(row["defuseResult"]["unresolved"], 1, "unresolved")
        compare(row["afterDefuse"], expected, "failed defuse visible state unchanged")
        continue
    partner = copy.deepcopy(before["entities"][1])
    expected["entities"][1] = partner
    primary.update(gate328=-1, gate194=0, fusionTimer=record["wait"])
    divisor = 1 if record["chp"] == 1 else 2
    for side, entity in enumerate((primary, partner)):
        publish(entity, record["id1"] if side == 0 else record["id2"], side)
        entity["raw"]["vitals"].update(currentHp=hp // divisor,
                                      effectiveMaxHp=bound // divisor, currentMp=0)
        action(entity, record["frame"])
        entity["raw"]["motion"]["x"] = 0
    partner["raw"]["position"] = copy.deepcopy(primary["raw"]["position"])
    for key in ("previousX", "previousY", "previousZ"):
        partner[key] = primary[key]
    partner["raw"]["combat"]["collisionYReference"] = primary["raw"]["combat"]["collisionYReference"]
    partner["raw"]["motion"]["y"] = 0
    partner["raw"]["frame"]["facingLeft"] = not primary["raw"]["frame"]["facingLeft"]
    partner["raw"]["identity"]["battleGroup"] = primary["raw"]["identity"]["battleGroup"]
    compare(row["defuseResult"]["defused"], 1, "defused")
    compare(row["afterDefuse"], expected, "defuse visible state")
    compare(row["lifecycleSuccess"], True, "following lifecycle only")

print(json.dumps({"status": "PASS", "cases": len(rows), "checks": checks,
                  "scope": "Visible merge/defuse state and public failure nonmutation; not private suspended bytes or full following model."}))
