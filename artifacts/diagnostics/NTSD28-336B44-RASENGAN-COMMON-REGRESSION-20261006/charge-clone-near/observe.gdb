set pagination off
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
        self.captured = False
        super().__init__("*0x%x" % address, internal=True, temporary=True)
    def stop(self):
        if self.captured:
            return False
        self.captured = True
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
AUDIO_OFFSET = 1816
EVENT_STRIDE = 48
STRING_OFFSET = 16
Entry("*'ntsd28::SimulationTickDriver28::step(ntsd28::BattleWorld28&, ntsd28::ObjectDefinitionCatalog28 const&, ntsd28::SimulationTickOptions28 const&) const'", internal=True)
Relation("*'ntsd28::BattleWorld28::resolve_special_relation_hit(unsigned long long, unsigned long long, int, ntsd28::SpecialInteractionRules28 const&)'", internal=True)
gdb.events.exited.connect(lambda event: print("FORMAL_EXIT " + str(event.exit_code)))
end
run
