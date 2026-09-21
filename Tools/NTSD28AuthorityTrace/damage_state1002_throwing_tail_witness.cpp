#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_throw_entity(const ntsd28::EntityState28* e) {
    if (!e) { std::cout << "null"; return; }
    const auto* frame = e->definition->frame(e->frame.action);
    const auto* snapshot = e->definition->frame(e->frame.tick_action_snapshot);
    std::cout << "{\"raw\":"; write_entity(std::cout, *e, 1);
    std::cout << ",\"available\":" << (frame ? "true" : "false")
              << ",\"state\":" << (frame ? frame->values.integer("state").value_or(0) : 0)
              << ",\"wait\":" << (frame ? frame->values.integer("wait").value_or(0) : 0)
              << ",\"next\":" << (frame ? frame->values.integer("next").value_or(0) : 0)
              << ",\"snapshotAvailable\":" << (snapshot ? "true" : "false")
              << ",\"snapshotState\":" << (snapshot ? snapshot->values.integer("state").value_or(0) : 0)
              << ",\"previousX\":" << e->position.previous_x
              << ",\"previousY\":" << e->position.previous_y
              << ",\"previousZ\":" << e->position.previous_z
              << ",\"environment\":" << e->environment_state_320
              << ",\"environmentSource\":" << e->environment_source_slot_160
              << ",\"catchSource\":" << e->catch_source_slot_90
              << ",\"impactSource\":" << e->impact_source_slot_164
              << ",\"pendingX\":" << e->pending_hit_impulse.total.x
              << ",\"pendingY\":" << e->pending_hit_impulse.total.y
              << ",\"pendingZ\":" << e->pending_hit_impulse.total.z
              << ",\"input\":";
    write_b2_input_entity(std::cout, *e, 1);
    std::cout << '}';
}

static void write_throw_state(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":[";
    bool first = true;
    for (int slot : {0, 1, 2, 70}) {
        if (!first) std::cout << ',';
        first = false;
        write_throw_entity(world.entity(slot));
    }
    std::cout << "],\"random\":";
    write_b2_initial_random(std::cout, world.random().state(), world.random().synchronized_table_hash());
    std::cout << '}';
}

static void write_throw_calls() {
    std::cout << "{\"crt\":"; write_b2_crt_calls(std::cout);
    std::cout << ",\"synchronized\":"; write_b2_synchronized_calls(std::cout);
    std::cout << '}';
}

static void emit_throw_case(int index, int attacker_state, int target_type,
                            bool declare_random_actions = true) {
    std::ostringstream attacker_dat, target_dat;
    attacker_dat << "<bmp_begin>\nname: ThrowTailAttacker\n<bmp_end>\n"
                 << "<frame> 20 initial\nstate: " << attacker_state
                 << " wait: 100 next: 20\n"
                 << "itr:\nkind: 0 effect: 0 x: -100 y: -100 w: 300 h: 300 zwidth: 100"
                 << " injury: 5 fall: 1 bdefend: 0 vrest: 1 dvx: 3\nitr_end:\n<frame_end>\n";
    for (int action = 0; action < (declare_random_actions ? 16 : 1); ++action)
        attacker_dat << "<frame> " << action << " random\nstate: 0 wait: 30 next: "
                     << action << "\n<frame_end>\n";
    target_dat << "<bmp_begin>\nname: ThrowTailTarget\n<bmp_end>\n"
               << "<frame> 10 initial\nstate: " << (target_type == 3 ? 3005 : 1000)
               << " wait: 100 next: 10\n"
               << "bdy:\nkind: 0 x: -100 y: -100 w: 300 h: 300 zwidth: 100\nbdy_end:\n<frame_end>\n";
    for (int action : {180, 186, 220, 222, 224, 226})
        target_dat << "<frame> " << action << " reaction\nstate: 0 wait: 100 next: "
                   << action << "\n<frame_end>\n";
    const std::string owner_dat = "<bmp_begin>\nname: ThrowTailOwner\n<bmp_end>\n"
        "<frame> 10 idle\nstate: 0 wait: 100 next: 10\n<frame_end>\n";
    auto attacker = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(attacker_dat.str()));
    auto target = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(target_dat.str()));
    auto owner = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(owner_dat));
    if (!attacker->ok() || !target->ok() || !owner->ok()) throw std::runtime_error("throw DAT parse failed");

    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(77, 4, "throw-attacker.dat", attacker);
    catalog.upsert_definition(78, target_type, "throw-target.dat", target);
    catalog.upsert_definition(79, 0, "throw-owner.dat", owner);
    for (int slot : {0, 1, 2, 70}) {
        ntsd28::SpawnRequest28 request;
        request.object_id = slot == 0 ? 77 : slot == 70 ? 78 : 79;
        request.object_type = slot == 0 ? 4 : slot == 70 ? target_type : 0;
        request.definition = slot == 0 ? attacker : slot == 70 ? target : owner;
        request.initial_action = slot == 0 ? 20 : 10;
        request.hp = 400; request.mp = 497;
        request.battle_group = slot == 70 ? 2 : 1;
        request.position.x = slot == 70 ? 350 : 300;
        request.position.y = slot == 70 ? -2 : -20;
        request.position.z = 250;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("throw spawn failed");
        auto& e = *world.entity(slot);
        e.position.precise_x = e.position.x + .25;
        e.position.precise_y = e.position.y - .375;
        e.position.precise_z = 250.5;
        e.frame.frame_counter = slot == 70 ? 8 : 7;
        e.frame.action_latch = slot == 70 ? 12 : 11;
        e.frame.previous_action_078 = slot == 0 ? 20 : 10;
        e.pending_hit_impulse.total.x = slot == 70 ? 13 : 2;
        e.pending_hit_impulse.total.y = 17;
        e.pending_hit_impulse.total.z = 19;
        e.motion.x = slot == 70 ? 4.28 : 1.25;
        e.motion.y = slot == 70 ? -6.0 : 0.0;
        e.motion.z = -.5;
    }
    world.entity(0)->owner_slot = 1;
    world.entity(1)->owner_slot = 2;
    world.snapshot_actions();
    const auto candidates = world.rebuild_geometric_hit_candidates();
    if (candidates.candidates_appended != 1) throw std::runtime_error("throw expected one candidate");
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"attackerType\":4,\"type\":"
              << target_type << ",\"attackerState\":" << attacker_state
              << ",\"randomActionsDeclared\":" << (declare_random_actions ? "true" : "false")
              << ",\"oid\":78,\"seed\":42},\"attackerDat\":\"" << json_escape(attacker_dat.str())
              << "\",\"targetDat\":\"" << json_escape(target_dat.str())
              << "\",\"ownerDat\":\"" << json_escape(owner_dat)
              << "\",\"config\":{\"slots\":[0,1,2,70],\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"hpGate\":"
              << defaults.selected_mode_default_hp_regen_gate_28 << ",\"mpGate\":"
              << defaults.selected_mode_mp_regen_gate_2c << ",\"dropGate\":"
              << defaults.selected_mode_drop_gate_4c << "},\"before\":";
    write_throw_state(world);
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = world.resolve_ordinary_unarmored_standard_hit(0, 0);
    std::cout << ",\"status\":" << static_cast<int>(result.status)
              << ",\"attackerPostHitAction\":" << result.attacker_post_hit_action
              << ",\"message\":\"" << json_escape(result.message) << "\",\"after\":";
    write_throw_state(world);
    std::cout << ",\"calls\":"; write_throw_calls();
    ntsd28::SimulationTickOptions28 options;
    options.stage_bounds = ntsd28::StageBounds28{800, 180, 350};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28 = defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c = defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c = defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
    std::cout << ",\"following\":"; write_throw_state(world);
    std::cout << ",\"followingCalls\":"; write_throw_calls();
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false") << "}\n";
}

int wmain() {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        emit_throw_case(0, 1002, 3);
        emit_throw_case(1, 0, 3);
        emit_throw_case(2, 1002, 4);
        emit_throw_case(3, 1002, 3, false);
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 91;
    }
}
