"""Independent branch and native RNG checks; not a whole AI/world model."""
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

    def check(condition, label):
        nonlocal checks
        checks += 1
        if not condition:
            failures.append(label)

    check(first == repeat, "double-run byte identity")
    check(len(rows) == 18, "18 representative cases")
    table = []
    state = 42
    for _ in range(3000):
        state = (state * 214013 + 2531011) & 0xffffffff
        table.append(((state >> 16) & 32767) % 255 + 1)
    table.append(0)
    family = {2, 4, 6, 7, 8, 9, 10, 11, 33, 34}

    for row in rows:
        name = row["name"]
        p = row["params"]
        r = row["result"]
        before = row["before"]["random"]
        check(row["synthetic"] is True, name + " synthetic")
        check(before["crt"]["state"] == state, name + " seeded CRT")
        check(row["before"]["entities"][0]["alias"] == p["alias"], name + " persisted alias")
        count = 0
        counter = 0
        index = 0
        for calls_key, after_key in [("calls", "after"), ("laterCalls", "later")]:
            if calls_key not in row:
                continue
            calls = row[calls_key]
            check(not calls["crt"], name + " no CRT draws " + calls_key)
            for call in calls["synchronized"]:
                bound = call["upperBound"]
                if bound > 0:
                    counter = (counter + 1) % 1234
                    index = (index + 1) % 3000
                    count += 1
                    expected = (table[index] + counter) % bound
                else:
                    expected = 0
                check(call["result"] == expected, name + " independent RNG result")
                check(call["counterAfter"] == counter, name + " RNG counter")
                check(call["indexAfter"] == index, name + " RNG index")
                check(call["totalCalls"] == count, name + " RNG calls")
            scalar = row[after_key]["random"]["synchronized"]
            check(scalar["counter"] == counter and scalar["index"] == index and scalar["totalCalls"] == count,
                  name + " scalar cursor " + after_key)
        sites = [call["callSite"] for call in row["calls"]["synchronized"]]
        if p["api"] == "profile":
            classified = p["alias"] if p["alias"] in family else p["actual"]
            check(r["classified"] == classified, name + " classifier")
            check(r["special_family"] == (classified in family), name + " family")
            check(r["family_chase_pressed"] == (classified in family), name + " family chase")
            match34 = p["alias"] >= 0 and (p["alias"] == 34 or p["actual"] == 34)
            match1 = p["alias"] >= 0 and (p["alias"] == 1 or p["actual"] == 1)
            check((58 in sites) == (classified in family and match34), name + " match34 site")
            check((59 in sites) == match1, name + " match1 site")
            check(r["object_one_chase_pressed"] == (p["actual"] == 1), name + " actual1 chase")
        elif p["api"] == "special":
            gate = row["calls"]["synchronized"][0]["result"] > 0
            match33 = p["alias"] >= 0 and (p["alias"] == 33 or p["actual"] == 33)
            path = not gate and match33
            success = path and p["targetState"] == 16 and p["distance"] < 60
            check(r["rejected_by_rng_gate"] == gate, name + " gate")
            check(r["oid33_path"] == path, name + " match33")
            check((108 in sites) == path, name + " 6c site")
            check(r["returnValue"] == int(success), name + " combo success")
            check(r["unsupported_custom_profile"] == (not gate and not success and p["alias"] != 0), name + " unsupported")
            check(row["after"]["entities"][0]["input"]["input"]["comboState"][2] == (3 if success else 0), name + " combo value")
        elif p["api"] == "ordinary":
            check(r["special"]["rejected_by_rng_gate"], name + " positive gate reached")
            check(not r["stopsOuter"] and r["profile"]["applicable"], name + " continued ordinary")
        else:
            stop = p["alias"] != 0
            check(r["sampledPreviousPending"], name + " sampled old pending")
            check(r["unsupported"] == stop, name + " main stop")
            check(r["profile"]["applicable"] == (not stop), name + " downstream profile")
            if stop:
                check(sites[-1] == 60, name + " no post-stop random draws")
            if p["api"] == "tick":
                check(row["laterLifecycleSuccess"], name + " later tick completed")
                check(row["laterResult"]["sampledPreviousPending"], name + " later sampler")
                if stop:
                    check(row["after"]["entities"][0]["input"]["input"]["currentMask"] == 0, name + " final sample clears old up")
                    check(row["after"]["entities"][0]["input"]["input"]["previousMask"] == 4, name + " old up preserved as previous")
    return {"status": "FAIL" if failures else "PASS", "cases": len(rows), "checks": checks,
            "sha256": hashlib.sha256(first).hexdigest(), "failures": failures,
            "scope": "Independent alias branches, call-site presence/order constraints, full RNG result/cursor algorithm and focused sampler assertions; not full AI or full world-state model."}


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
