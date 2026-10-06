"""Observe the unchanged formal EXE's returned audio events; no game-state writes."""

import argparse
import datetime
import hashlib
import json
import os
from pathlib import Path
import struct
import subprocess


FORMAL_SHA = "336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3"


def write_new(path, data):
    with path.open("xb") as output:
        output.write(data)


def save_json(path, value):
    write_new(path, json.dumps(value, ensure_ascii=False, indent=2).encode("utf-8"))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--release", type=Path, required=True)
    parser.add_argument("--toolchain", type=Path, required=True)
    parser.add_argument("--fixture", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=False)
    exe = args.release / "NTSD2.8-Logan.exe"
    assert sha(exe) == FORMAL_SHA, "Formal EXE identity changed"
    environment = os.environ.copy()
    environment["PATH"] = str(args.toolchain / "bin") + ";" + str(args.toolchain / "opt/bin") + ";" + environment["PATH"]
    environment["PYTHONHOME"] = str(args.toolchain / "opt")
    environment["PYTHONDONTWRITEBYTECODE"] = "1"

    layout_source = b'''#include "ntsd28/simulation_tick_driver.h"
#include <cstddef>
#include <iostream>
int main() {
 std::cout << offsetof(ntsd28::SimulationTickResult28, audio_events) << " "
           << sizeof(ntsd28::WorldAudioEvent28) << " "
           << offsetof(ntsd28::WorldAudioEvent28, resource_path) << " "
           << sizeof(ntsd28::SimulationTickResult28);
}
'''
    layout_cpp = args.output / "layout_probe.cpp"
    layout_exe = args.output / "layout_probe.exe"
    write_new(layout_cpp, layout_source)
    compile_argv = [str(args.toolchain / "bin/g++.exe"), "-std=c++17", "-O0", "-I" + str(args.release / "source/ntsd28_core/include"), str(layout_cpp), "-o", str(layout_exe)]
    save_json(args.output / "layout-compile-argv.json", compile_argv)
    compiled = subprocess.run(compile_argv, capture_output=True, timeout=45)
    write_new(args.output / "layout-compile-stdout.log", compiled.stdout)
    write_new(args.output / "layout-compile-stderr.log", compiled.stderr)
    assert compiled.returncode == 0, "Layout helper compilation failed"
    layout = subprocess.run([str(layout_exe)], capture_output=True, timeout=10)
    assert layout.returncode == 0
    offset, stride, string_offset, result_size = map(int, layout.stdout.split())
    save_json(args.output / "layout-hypothesis.json", {"audioOffset": offset, "eventStride": stride, "stringOffset": string_offset, "resultSize": result_size, "authority": "Current header layout hypothesis, requires formal positive runtime validation"})

    baseline = args.fixture.read_bytes()
    assert len(baseline) == 6491672
    cases = [("type1-near", 120, 0, 500), ("type3-near", 206, 0, 500), ("type3-far-control", 206, 0, 1000)]
    summaries = []
    for name, target_oid, target_action, target_x in cases:
        destination = args.output / name
        destination.mkdir()
        recording = bytearray(baseline)
        struct.pack_into("<i", recording, 0x10, 4)
        struct.pack_into("<i", recording, 0x144, 3)
        struct.pack_into("<i", recording, 0x14b4, 0)
        struct.pack_into("<i", recording, 0x14b8, 2000)
        actors = [
            [1, 810, 1, 0, 500, 0, 600, 500, 0, 500, -1, 0, 0],
            [1, target_oid, 2, 0, target_x, 0, 600, 500, 1, 500, -1, 0, 0],
            [1, 2, 1, 0, 1500, 0, 600, 500, 2, 500, -1, 0, 0],
            [1, 99, 2, 0, 1800, 0, 600, 500, 3, 500, -1, 0, 0],
        ]
        for column in range(13):
            for slot in range(20):
                struct.pack_into("<i", recording, 0x1a8 + column * 0x50 + slot * 4, actors[slot][column] if slot < len(actors) else 0)
        fixture = destination / "input.lfr"
        write_new(fixture, recording)
        save_json(destination / "fixture-manifest.json", {"basePath": str(args.fixture), "baseSha": sha(args.fixture), "sha": sha(fixture), "actors": actors, "tickCount": 4, "productionDatEdited": False})
        runtime = args.release / "resources/runtime"
        argv = [str(exe), "--resource-root", str(runtime), "--complete-vfs-root", str(runtime), "--headless-playback-lfr", str(fixture), "--headless-playback-report", str(destination / "root-report.json"), "--headless-playback-trace", str(destination / "root-trace.jsonl"), "--lfr-slot0-action", "107", "--lfr-slot1-action", str(target_action), "--lfr-slot0-facing", "1", "--lfr-slot1-facing", "1", "--lfr-slot0-mp", "500", "--lfr-slot1-mp", "500"]
        save_json(destination / "root-argv.json", argv)
        # Symbols are read from this exact formal PE. These are function entries,
        # not guessed instruction addresses or rebuilt-code addresses.
        debugger_script = '''set pagination off
set confirm off
python
import gdb, struct, json
def mem(address, size):
    return bytes(gdb.selected_inferior().read_memory(address, size))
def reg(name):
    return int(gdb.parse_and_eval("$" + name))
class Returned(gdb.Breakpoint):
    def __init__(self, address, result):
        self.result = result
        super().__init__("*0x%x" % address, internal=True, temporary=True)
    def stop(self):
        try:
            sequence = struct.unpack("<Q", mem(self.result, 8))[0]
            start, finish, capacity = struct.unpack("<QQQ", mem(self.result + AUDIO_OFFSET, 24))
            assert start <= finish <= capacity and (finish - start) % EVENT_STRIDE == 0
            count = (finish - start) // EVENT_STRIDE
            assert count <= 100
            events = []
            for index in range(count):
                address = start + index * EVENT_STRIDE
                source, world_x, channel = struct.unpack("<iii", mem(address, 12))
                string_pointer, string_length = struct.unpack("<QQ", mem(address + STRING_OFFSET, 16))
                assert string_length < 4096
                path = mem(string_pointer, string_length).decode("utf-8") if string_length else ""
                events.append({"source": source, "worldX": world_x, "nativeChannel": channel, "path": path})
            print("AUDIO_OBS " + json.dumps({"sequence": sequence, "count": count, "events": events}))
        except Exception as error:
            print("AUDIO_OBS_ERROR " + repr(error))
        return False
class Entry(gdb.Breakpoint):
    def stop(self):
        result = reg("rcx")
        caller = struct.unpack("<Q", mem(reg("rsp"), 8))[0]
        Returned(caller, result)
        return False
class Relation(gdb.Breakpoint):
    def stop(self):
        print("RELATION_CALL " + json.dumps({"attackerSlot": reg("r8"), "candidateIndex": reg("r9")}))
        return False
AUDIO_OFFSET = OFFSET_VALUE
EVENT_STRIDE = STRIDE_VALUE
STRING_OFFSET = STRING_VALUE
Entry("*0x14006fb60", internal=True)
Relation("*0x140041a90", internal=True)
end
run
'''.replace("OFFSET_VALUE", str(offset)).replace("STRIDE_VALUE", str(stride)).replace("STRING_VALUE", str(string_offset))
        command_file = destination / "observe.gdb"
        write_new(command_file, debugger_script.encode("utf-8"))
        debugger_argv = [str(args.toolchain / "bin/gdborig.exe"), "-nx", "-batch", "-x", str(command_file), "--args"] + argv
        save_json(destination / "debugger-argv.json", debugger_argv)
        started = datetime.datetime.now(datetime.timezone.utc).isoformat()
        observed = subprocess.run(debugger_argv, capture_output=True, timeout=60, cwd=args.release, env=environment)
        write_new(destination / "debugger-stdout.log", observed.stdout)
        write_new(destination / "debugger-stderr.log", observed.stderr)
        stdout = observed.stdout.decode("utf-8", "replace")
        events = [json.loads(line[10:]) for line in stdout.splitlines() if line.startswith("AUDIO_OBS ")]
        errors = [line for line in stdout.splitlines() if line.startswith("AUDIO_OBS_ERROR ")]
        calls = [json.loads(line[14:]) for line in stdout.splitlines() if line.startswith("RELATION_CALL ")]
        value = {"case": name, "startedUtc": started, "debuggerExit": observed.returncode, "ticks": events, "errors": errors, "relationCalls": calls, "formalShaAfter": sha(exe)}
        save_json(destination / "observation.json", value)
        summaries.append(value)
        print(name, "gdbExit", observed.returncode, "capturedTicks", len(events), "relations", len(calls), "errors", len(errors), flush=True)
    save_json(args.output / "summary.json", {"formalSha": sha(exe), "cases": summaries})


if __name__ == "__main__":
    main()
