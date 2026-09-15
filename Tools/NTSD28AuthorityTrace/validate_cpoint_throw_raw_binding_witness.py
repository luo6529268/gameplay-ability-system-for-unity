"""Validate the declared immediate throw transaction; later driver state stays separate."""
import argparse
import copy
import hashlib
import json
from pathlib import Path


def validate(folder):
    first = (folder / "first.jsonl").read_bytes()
    repeat = (folder / "repeat.jsonl").read_bytes()
    rows = [json.loads(line) for line in first.splitlines()]
    differences = []
    checks = 0

    def compare(expected, actual, path):
        nonlocal checks
        if isinstance(expected, dict):
            for key, value in expected.items():
                compare(value, actual.get(key), path + "." + key)
        elif isinstance(expected, list):
            compare(len(expected), len(actual), path + ".length")
            for index, value in enumerate(expected):
                compare(value, actual[index], path + "." + str(index))
        else:
            checks += 1
            if expected != actual:
                differences.append(f"{path}: expected={expected!r} actual={actual!r}")

    compare(True, first == repeat, "repeatEqual")
    compare(392, len(rows), "caseCount")
    paired = {}
    for index, row in enumerate(rows):
        label = f"case{index}"
        compare(index, row["index"], label + ".index")
        expected = copy.deepcopy(row["before"])
        for slot, key, declared in ((0, "next", "nextDeclared"), (1, "vaction", "victimDeclared")):
            entity = expected[slot]
            action = row[key]
            entity.update(action=action, snapshot=action, counter=0,
                          available=(0 <= action < 999 or (action == 999 and row[declared])),
                          wait=37 if row[declared] else 0, next=0)
            entity["raw"]["frame"].update(action=action, tickActionSnapshot=action,
                                          frameCounter=0, frameState=0)
        x = 89 if row["facing"] else 111
        expected[1]["raw"]["position"].update(x=x, y=-24, preciseX=x, preciseY=-24)
        expected[1]["raw"]["motion"].update(x=-1.5 if row["facing"] else 1.5, y=-2.25)
        compare(expected, row["after"], label + ".after")
        for key, value in {"thrown": 1, "transitions": int(row["select"]),
                           "definitionPreserved": True, "crtCalls": 0, "nativeCalls": 0}.items():
            compare(value, row[key], label + "." + key)
        pair = tuple(row[key] for key in ("next", "nextDeclared", "vaction", "victimDeclared", "facing", "select"))
        if row["injury"] == 0:
            paired[pair] = row["after"]
        else:
            compare(-1, row["injury"], label + ".injury")
            compare(paired[pair], row["after"], label + ".minusOneEqualsZero")
    report = {
        "cases": len(rows), "checks": checks, "differences": differences,
        "sha256": hashlib.sha256(first).hexdigest(),
        "scope": "Independent full immediate after-state derivation from before for 392 synthetic cases; no depth keys, injury -1/0. Third entity has owner slot 0. Descriptor wait/next are not Unity mutable overrides. Next full tick captured but not independently verified here. Formal EXE observation and Unity acceptance are separate.",
    }
    (folder / "validation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps({key: report[key] for key in ("cases", "checks", "sha256")} | {"differences": len(differences)}))
    return 1 if differences else 0


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("folder", type=Path)
    raise SystemExit(validate(parser.parse_args().folder))
