#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_held_entity(const ntsd28::EntityState28* e, bool refill = false) {
    if (!e) { std::cout << "null"; return; }
    const auto* frame = e->definition->frame(e->frame.action);
    const auto* snapshot = e->definition->frame(e->frame.tick_action_snapshot);
    std::cout << "{\"raw\":"; write_entity(std::cout, *e, 1);
    std::cout << ",\"link\":" << e->interaction_state << ",\"parent\":" << e->linked_parent_slot
        << ",\"child\":" << e->linked_child_slot << ",\"available\":" << (frame ? "true" : "false")
        << ",\"state\":" << (frame ? frame->values.integer("state").value_or(0) : 0)
        << ",\"wait\":" << (frame ? frame->values.integer("wait").value_or(0) : 0)
        << ",\"next\":" << (frame ? frame->values.integer("next").value_or(0) : 0)
        << ",\"snapshotAvailable\":" << (snapshot ? "true" : "false")
        << ",\"snapshotState\":" << (snapshot ? snapshot->values.integer("state").value_or(0) : 0)
        << ",\"healTimer\":" << e->encoded_heal_timer_e0
        << ",\"input\":"; write_b2_input_entity(std::cout, *e, 1);
    if (refill) std::cout << ",\"ordinaryCreditGate2F4\":" << e->ordinary_credit_gate_2f4
        << ",\"hpConsumed\":" << e->input_hp_consumed_total << ",\"mpConsumed\":" << e->input_mp_consumed_total;
    std::cout << '}';
}
static void write_held_state(ntsd28::BattleWorld28& world, bool refill = false) {
    std::cout << "{\"entities\":["; write_held_entity(world.entity(0), refill); std::cout << ',';
    write_held_entity(world.entity(70), refill); std::cout << "],\"random\":";
    write_b2_initial_random(std::cout, world.random().state(), world.random().synchronized_table_hash());
    std::cout << '}';
}
static void write_held_result(const ntsd28::WorldHeldRefillPass28& r) {
    std::cout << "{\"success\":" << (r.success ? "true" : "false")
        << ",\"linked\":" << r.linked_children << ",\"updates\":" << r.wpoint_follow_updates
        << ",\"unsupported\":" << r.unsupported_links << ",\"terminal\":" << r.terminal_weapon_actions
        << ",\"velocityReleases\":" << r.wpoint_velocity_releases << ",\"kind3Releases\":" << r.wpoint_kind3_releases
        << ",\"charging\":" << r.charging_links << ",\"hpRefills\":" << r.hp_refill_updates
        << ",\"mpRefills\":" << r.mp_refill_updates << ",\"exhausted\":" << r.exhausted_links
        << ",\"diagnostics\":[";
    bool first = true;
    for (const auto& d : r.diagnostics) { if (!first) std::cout << ','; first = false; std::cout << '"' << json_escape(d) << '"'; }
    std::cout << "]}";
}
static void write_held_calls() {
    std::cout << "{\"crt\":"; write_b2_crt_calls(std::cout);
    std::cout << ",\"synchronized\":"; write_b2_synchronized_calls(std::cout); std::cout << '}';
}

static void run_kind8(int index, int action, bool declared, int sync,
                      int injury = 3, int gain = 7, bool reject = false) {
    std::ostringstream ad, td;
    ad << "<bmp_begin>\nname: Kind8Attacker\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10\n"
       << "itr:\nkind: 8 x: -100 y: -100 w: 300 h: 300 zwidth: 100 bdefend: 8 respond: " << (reject ? 1 : 0)
       << " dvx: " << action << " dvy: " << sync << " injury: " << injury << " caughtact: " << gain << "\nitr_end:\n<frame_end>\n";
    if (declared) ad << "<frame> " << action << " target\nstate: 3 wait: 37 next: 10\n<frame_end>\n";
    td << "<bmp_begin>\nname: Kind8Target\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10\n"
       << "bdy:\nkind: 0 x: -100 y: -100 w: 300 h: 300 zwidth: 100\nbdy_end:\n<frame_end>\n";
    auto a = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(ad.str()));
    auto t = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(td.str()));
    if (!a->ok() || !t->ok()) throw std::runtime_error("kind8 DAT parse failed");
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(77, 0, "kind8-attacker.dat", a);
    catalog.upsert_definition(78, 0, "kind8-target.dat", t);
    for (int slot : {0,70}) {
        ntsd28::SpawnRequest28 request;
        request.object_id = slot == 0 ? 77 : 78; request.object_type = 0;
        request.definition = slot == 0 ? a : t; request.initial_action = 10;
        request.hp = 400; request.mp = 497;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("kind8 spawn failed");
        auto& e = *world.entity(slot);
        e.battle_group = slot == 0 || reject ? 1 : 2;
        e.position.x = slot == 0 ? 300 : 350; e.position.y = slot == 0 ? -20 : -30; e.position.z = 250;
        e.position.precise_x = slot == 0 ? 300.25 : 350.75;
        e.position.precise_y = slot == 0 ? -20.5 : -30.25;
        e.position.precise_z = slot == 0 ? 250.125 : 250.375;
        e.frame.frame_counter = slot == 0 ? 7 : 8;
        e.frame.action_latch = slot == 0 ? 11 : 12;
        e.frame.previous_action_078 = 10;
        e.encoded_heal_timer_e0 = 41;
    }
    world.snapshot_actions();
    const auto candidates = world.rebuild_geometric_hit_candidates();
    if (candidates.candidates_appended != 1) throw std::runtime_error("kind8 expected one geometric candidate");
    if (reject) world.entity(70)->battle_group = 2;
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"action\":" << action << ",\"declared\":" << (declared ? "true" : "false")
        << ",\"dvy\":" << sync << ",\"injury\":" << injury << ",\"gain\":" << gain << ",\"reject\":" << (reject ? "true" : "false") << ",\"seed\":42}"
        << ",\"attackerDat\":\"" << json_escape(ad.str()) << "\",\"targetDat\":\"" << json_escape(td.str())
        << "\",\"config\":{\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"hpGate\":" << defaults.selected_mode_default_hp_regen_gate_28
        << ",\"mpGate\":" << defaults.selected_mode_mp_regen_gate_2c << ",\"dropGate\":" << defaults.selected_mode_drop_gate_4c << "},\"before\":";
    write_held_state(world);
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = world.resolve_special_relation_hit(0, 0, 0);
    std::cout << ",\"applied\":" << (result.status == ntsd28::WorldRelationHitStatus28::applied ? "true" : "false")
        << ",\"message\":\"" << json_escape(result.message) << "\",\"after\":";
    write_held_state(world); std::cout << ",\"calls\":"; write_held_calls();
    ntsd28::SimulationTickOptions28 options;
    options.stage_bounds = ntsd28::StageBounds28{800,180,350};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28 = defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c = defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c = defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
    std::cout << ",\"following\":"; write_held_state(world);
    std::cout << ",\"followingCalls\":"; write_held_calls();
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false") << "}\n";
}
int wmain() {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        for (int action : {0,1,69,444,856,857,900,998,999,-1,1000})
            for (bool declared : {false,true}) {
                if (declared && (action < 0 || action > 999)) continue;
                for (int sync : {-2,-1,0,1,2,3}) run_kind8(index++, action, declared, sync);
            }
        for (int action : {69,999}) for (int injury : {0,-3,3}) for (int gain : {-7,0})
            run_kind8(index++, action, false, -1, injury, gain);
        for (int action : {69,999}) run_kind8(index++, action, false, 2, 3, 7, true);
        if (index != 134) throw std::runtime_error("kind8 count mismatch");
        return 0;
    } catch (const std::exception& e) { std::cerr << e.what() << '\n'; return 91; }
}
