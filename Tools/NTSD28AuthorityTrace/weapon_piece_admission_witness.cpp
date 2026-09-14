#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

struct AdmissionCase {
    int target = 0, type = 3, action = 0, declared999 = 0, present = 1;
    int source = 20, free_slots = -1, variants = 1, parent_oid = 888;
};

static std::shared_ptr<const ntsd28::DatDocument> admission_dat(const std::string& text) {
    auto dat = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text));
    if (!dat->ok()) throw std::runtime_error("admission DAT rejected");
    return dat;
}

static void run_admission(const AdmissionCase& c, int phase) {
    std::ostringstream parent_text;
    parent_text << "<bmp_begin>\nname: Parent weapon_hp: 10\n<bmp_end>\n"
        "<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n"
        "<weapon_piece>\nteam: 1\n";
    for (int variant = 0; variant < c.variants; ++variant)
        parent_text << "piece: 1\n" << (variant == 0 ? "amount: 2 " : "")
            << "oid: " << c.target << " act: " << c.action
            << " framea: 0 dvx: 0 dvy: 0 dvz: 0\npiece_end:\n";
    parent_text << "<weapon_piece_end>\n";
    auto parent = admission_dat(parent_text.str());
    std::string child_text = "<bmp_begin>\nname: Child weapon_hp: 17\n<bmp_end>\n"
        "<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
    if (c.declared999)
        child_text += "<frame> 999 terminal\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
    auto child = admission_dat(child_text);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(c.parent_oid, 1, "parent.dat", parent);
    if (c.present) catalog.upsert_definition(c.target == -1 ? 999 : c.target, c.type, "child.dat", child);
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 request;
    request.object_id = c.parent_oid; request.object_type = 1; request.definition = parent;
    request.initial_action = 0; request.hp = 500; request.mp = 500;
    request.position.x = 100; request.position.y = -20; request.position.z = 200;
    request.owner_slot = 17; request.battle_group = 9;
    if (!world.spawn_at(c.source, request).success) throw std::runtime_error("source spawn failed");
    world.entity(c.source)->weapon_hp_31c = -1;
    if (c.free_slots >= 0) {
        int remaining = c.free_slots;
        request.object_id = 4444; request.object_type = 4; request.definition = child;
        for (std::size_t slot = 50; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            if (!world.slot_available_for_spawn(slot)) continue;
            if (remaining > 0) { --remaining; continue; }
            if (!world.spawn_at(slot, request).success) throw std::runtime_error("blocker spawn failed");
        }
    }
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto before = world.random().state();
    ntsd28::WorldWeaponPiecePass28 pass;
    std::vector<std::string> messages;
    std::vector<std::size_t> frame_slots;
    bool lifecycle_ok = true;
    int frame_errors = 0;
    if (phase == 0) pass = world.materialize_weapon_piece_fragments(c.source, catalog);
    else {
        auto result = ntsd28::SimulationTickDriver28{}.step(world, catalog);
        pass = std::move(result.weapon_pieces);
        messages = std::move(result.diagnostics.messages);
        lifecycle_ok = result.lifecycle.success;
        for (const auto& frame : result.frames) {
            frame_slots.push_back(frame.slot);
            if (!frame.frame.ok()) ++frame_errors;
        }
    }
    const auto after = world.random().state();
    std::cout << "{\"phase\":" << phase << ",\"target\":" << c.target << ",\"type\":" << c.type
        << ",\"action\":" << c.action << ",\"declared999\":" << c.declared999 << ",\"present\":" << c.present
        << ",\"source\":" << c.source << ",\"freeSlots\":" << c.free_slots << ",\"variants\":" << c.variants
        << ",\"parentOid\":" << c.parent_oid << ",\"success\":" << (pass.success ? "true" : "false")
        << ",\"spawned\":" << pass.spawned << ",\"builtinSpawned\":" << pass.builtin_spawned
        << ",\"unresolved\":" << pass.unresolved << ",\"noSlot\":" << pass.skipped_no_slot
        << ",\"sourceAlive\":" << (world.entity(c.source) ? "true" : "false")
        << ",\"lifecycleOk\":" << (lifecycle_ok ? "true" : "false") << ",\"frameErrors\":" << frame_errors
        << ",\"syncCalls\":" << after.synchronized.calls - before.synchronized.calls
        << ",\"crtCalls\":" << after.crt_calls - before.crt_calls << ",\"events\":[";
    bool first = true;
    for (const auto& event : pass.events) {
        if (!first) std::cout << ',';
        first = false;
        const auto* entity = event.spawned_slot < ntsd28::EngineProfile28::maximum_slots ? world.entity(event.spawned_slot) : nullptr;
        std::cout << "{\"builtin\":" << (event.builtin_fragment ? "true" : "false")
            << ",\"oid\":" << event.object_id << ",\"action\":" << event.action
            << ",\"slot\":" << event.spawned_slot << ",\"message\":\"" << json_escape(event.message)
            << "\",\"generation\":" << (entity ? entity->presentation_generation : 0) << ",\"entity\":";
        if (entity) write_entity(std::cout, *entity, 1); else std::cout << "null";
        std::cout << '}';
    }
    std::cout << "],\"frameSlots\":["; first = true;
    for (auto slot : frame_slots) { if (!first) std::cout << ','; first = false; std::cout << slot; }
    std::cout << "],\"calls\":["; first = true;
    for (const auto& call : direct_synchronized_calls) {
        if (!first) std::cout << ',';
        first = false;
        std::cout << '[' << call.call_site << ',' << call.upper_bound << ',' << call.result << ','
            << call.counter_after << ',' << call.index_after << ',' << call.total_calls << ']';
    }
    std::cout << "],\"messages\":["; first = true;
    for (const auto& message : messages) {
        if (!first) std::cout << ',';
        first = false;
        std::cout << '"' << json_escape(message) << '"';
    }
    std::cout << "]}\n";
}

int wmain() {
    try {
        for (int phase : {0, 1}) {
            for (int target : {0, 777, -1}) for (int type : {0, 1, 2, 3, 4, 5, 6})
            for (int action : {-1, 0, 857, 998, 999, 1000}) for (int declared : {0, 1})
            for (int source : {20, 70, 998}) {
                AdmissionCase c; c.target = target; c.type = type; c.action = action;
                c.declared999 = declared; c.source = source; run_admission(c, phase);
            }
            for (int target : {0, 777, -1}) for (int present : {0, 1})
            for (int free : {-1, 0, 1}) for (int variants : {1, 2})
            for (int source : {20, 70}) for (int parent : {888, 151}) {
                AdmissionCase c; c.target = target; c.present = present; c.free_slots = free;
                c.variants = variants; c.source = source; c.parent_oid = parent; run_admission(c, phase);
            }
        }
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
