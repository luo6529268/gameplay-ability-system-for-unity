"""Focused independent clone transaction model; following is captured, not modeled."""
import argparse
import hashlib
import json
from pathlib import Path


def validate(folder):
    first = (folder / "first.jsonl").read_bytes()
    repeat = (folder / "repeat.jsonl").read_bytes()
    rows = [json.loads(line) for line in first.splitlines()]
    failures = []
    checks = 0

    def check(actual, expected, label):
        nonlocal checks
        checks += 1
        if actual != expected:
            failures.append({"field": label, "actual": actual, "expected": expected})

    check(first, repeat, "repeat bytes")
    check(len(rows), 12, "case count")
    table = []
    crt = 42
    for _ in range(3000):
        crt = (crt * 214013 + 2531011) & 0xffffffff
        table.append(((crt >> 16) & 32767) % 255 + 1)
    table.append(0)
    for row in rows:
        p = row["params"]
        name = row["name"]
        label = lambda key: name + "." + key
        check(row["synthetic"], True, label("synthetic"))
        check(row["config"]["capacity"], 1000, label("native capacity"))
        check(row["config"]["transientStart"], 50, label("dynamic start"))
        check(row["before"]["fillerCount"], 950 - p["freeSlots"], label("fillers"))
        check(row["before"]["activeCount"], 951 - p["freeSlots"], label("initial occupancy"))
        check(row["before"]["random"]["crt"], {"state": crt, "totalCalls": 3000}, label("initial CRT"))
        applicable = not p["pending"] and p["sourceType"] == 0
        check(row["result"]["applicable"], applicable, label("applicable"))
        process = applicable and p["counter"] == 1
        encoded = process and p["sourceState"] != 9996
        process = process and not encoded
        check(row["after"]["entities"][0], row["before"]["entities"][0], label("source unchanged"))
        expected_calls = []
        created = []
        missing = 0
        events = []
        counter = 0
        index = 0

        def draw(site, bound):
            nonlocal counter, index
            counter = (counter + 1) % 1234
            index = (index + 1) % 3000
            result = (table[index] + counter) % bound
            expected_calls.append({"callSite": site, "upperBound": bound, "result": result,
                                   "counterAfter": counter, "indexAfter": index, "totalCalls": len(expected_calls) + 1})
            return result

        if process:
            for clone in range(5):
                oid = 218 if clone == 4 else 217
                if oid == p["missingOid"]:
                    missing += 1
                    events.append((clone, oid, 1000))
                    continue
                x = 300 + draw(0x41f792, 7) - 3
                y = -20 + draw(0x41f7b6, 7) - 9
                vertical = draw(0x41f818, 15)
                if clone in (0, 2):
                    vz = draw(0x41f8a9, 2) + 3
                elif clone in (1, 3):
                    vz = -3 - draw(0x41f87f, 2)
                else:
                    vz = 1
                vx = (-10 - draw(0x41f8dd, 3) if clone < 2 else
                      draw(0x41f908, 3) + 10 if clone < 4 else draw(0x41f92e, 7) - 3)
                action = draw(0x41f955, 4)
                facing = draw(0x41f96b, 2) != 0
                if len(created) >= p["freeSlots"]:
                    events.append((clone, oid, 1000))
                    continue
                slot = 50 + len(created)
                events.append((clone, oid, slot))
                created.append((slot, oid, x, y, vx, -(vertical // 2) - 5, vz, action, facing))
        check(row["calls"]["synchronized"], expected_calls, label("complete RNG tuples"))
        check(row["calls"]["crt"], [], label("no CRT calls"))
        check(row["result"]["spawned"], len(created), label("spawned"))
        check(row["result"]["unresolved"], 1 if encoded else missing, label("unresolved"))
        check(row["result"]["success"], not encoded and missing == 0 and len(created) == (5 if process else 0), label("success"))
        if not encoded:
            check([(e["cloneIndex"], e["oid"], e["slot"]) for e in row["result"]["events"]], events, label("events"))
        check(row["after"]["activeCount"], row["before"]["activeCount"] + len(created), label("final occupancy"))
        check(row["after"]["fillerCount"], row["before"]["fillerCount"], label("filler preservation count"))
        check(len(row["after"]["entities"]), len(created) + 1, label("captured nonfillers"))
        for entity, values in zip(row["after"]["entities"][1:], created):
            slot, oid, x, y, vx, vy, vz, action, facing = values
            raw = entity["raw"]
            check(raw["slot"], slot, label("slot"))
            check(raw["identity"], {"objectId": oid, "objectType": p["targetType"], "controlSlot": 0,
                  "ownerSlot": -1, "battleGroup": 0, "participantClass": 0}, label("identity"))
            check(raw["frame"], {"action": action, "actionLatch": action, "previousAction": action,
                  "tickActionSnapshot": action, "frameCounter": 0, "frameState": 0, "facingLeft": facing}, label("frame"))
            check(raw["position"], {"x": x, "y": y, "z": 251, "preciseX": x, "preciseY": y, "preciseZ": 251}, label("position"))
            check(raw["motion"], {"x": vx, "y": vy, "z": vz}, label("motion"))
            for key in ("currentHp", "effectiveMaxHp", "baseMaxHp", "currentMp"):
                check(raw["vitals"][key], 500, label(key))
            check(raw["vitals"]["baseMaxMp"], 731, label("maxMP"))
            for key, value in [("weaponHp", 37), ("runtimeArmorHp", 23), ("armorRecoveryTimer", 17), ("attackerRest", 6)]:
                check(raw["combat"][key], value, label(key))
            check(entity["display"], [0, 0, 0, 0, 500, 0, 500, 0], label("display8"))
            check(entity["aiProfile"], 34, label("AI alias"))
            check(entity["dropMode"], 2, label("drop"))
            check(entity["incomingScale"], 0, label("spawn_at does not apply stats.defend"))
            check(entity["modeScale"], 100, label("default mode scale"))
            check(entity["pendingCount"], 0, label("pending count"))
            check(entity["pending"], [0, 0, 0], label("pending motion"))
            check([entity["available"], entity["state"], entity["wait"], entity["next"]],
                  [True, 0, 41 if p["declared"] else 0, action if p["declared"] else 0], label("descriptor"))
        sync = row["after"]["random"]["synchronized"]
        check([sync["counter"], sync["index"], sync["totalCalls"]], [counter, index, len(expected_calls)], label("cursor"))
        if "following" in row:
            check(row["followingLifecycleSuccess"], True, label("following completed only"))
    return {"status": "FAIL" if failures else "PASS", "cases": len(rows), "checks": checks,
            "sha256": hashlib.sha256(first).hexdigest(), "failures": failures,
            "scope": "Independent immediate admission, complete clone RNG tuples, allocation, resources, display8 and descriptor model. Following captured, not independently modeled; filler payloads not fully compared."}


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("folder", type=Path)
    args = parser.parse_args()
    result = validate(args.folder)
    output = args.folder / "independent-validation.json"
    if output.exists() and json.loads(output.read_text()).get("status") == "FAIL":
        archive = args.folder / "initial-model-failure.json"
        if not archive.exists():
            archive.write_bytes(output.read_bytes())
    output.write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps(result, indent=2))
    raise SystemExit(0 if result["status"] == "PASS" else 1)
