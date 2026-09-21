#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

struct MaterializerCase {
    std::string name;
    int type = 0, kind = 1, oid = 777, state = 3000, credit = -1;
    int defend = 140, effect = 7, facing = 0, team = 4;
    int action = 0, centers = 0, framea = 0, input = 0;
    int free_slots = 950;
    bool missing = false, declared = true, follow = false;
};

static void emit_entity(const ntsd28::EntityState28& e) {
    std::cout << "{\"raw\":";
    write_entity(std::cout, e, 1);
    std::cout << ",\"input\":";
    write_b2_input_entity(std::cout, e, 1);
    std::cout << ",\"extra\":[" << e.render_phase_008 << ','
        << e.hit_resource_injury_double_1a0 << ',' << e.hit_resource_suppression_15c << ','
        << e.ordinary_credit_gate_2f4 << ',' << e.incoming_damage_scale_340 << ','
        << e.revive_lives_30c << ',' << e.revive_next_hp_314 << ','
        << e.revive_next_lives_310 << ',' << e.revive_visual_id_184 << ','
        << e.frame.action_latch << ',' << e.frame.previous_action_078 << ','
        << e.frame.frame_counter << ',' << e.display_current_hp_200 << ','
        << e.display_effective_max_hp_208 << ',' << e.weapon_hp_31c << ']';
    std::cout << ",\"links\":[" << e.interaction_state << ',' << e.linked_parent_slot << ',' << e.linked_child_slot << ']';
    std::cout << ",\"position\":[" << e.position.x << ',' << e.position.y << ',' << e.position.z
        << ',' << e.position.precise_x << ',' << e.position.precise_y << ',' << e.position.precise_z
        << "],\"motion\":[" << e.motion.x << ',' << e.motion.y << ',' << e.motion.z << "]}";
}

static void emit_world(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":[";
    bool first = true;
    int fillers = 0;
    for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
        const auto* e = world.entity(slot);
        if (!e) continue;
        if (e->object_id == 31999) { ++fillers; continue; }
        if (!first) std::cout << ',';
        first = false;
        emit_entity(*e);
    }
    std::cout << "],\"fillers\":" << fillers << ",\"random\":";
    write_b2_initial_random(std::cout, world.random().state(), world.random().synchronized_table_hash());
    std::cout << '}';
}

static void emit_rng() {
    std::cout << "{\"crt\":";
    write_b2_crt_calls(std::cout);
    std::cout << ",\"synchronized\":";
    write_b2_synchronized_calls(std::cout);
    std::cout << '}';
}

static void run_case(int index, const MaterializerCase& c) {
    std::ostringstream parent_dat, child_dat;
    parent_dat << "<bmp_begin>\nname: MaterializerParent\n<bmp_end>\n<frame> 0 parent\n"
        << "state: 0 wait: 100 next: 0 centerx: 3 centery: 4\nopoint:\nkind: " << c.kind
        << " oid: " << c.oid << " action: " << c.action << " x: 11 y: -7 z: 5 dvx: 8 dvy: -2 dvz: 3"
        << " facing: " << c.facing << " team: " << c.team << " hp: 137 mp: 211 reserve: 3 join: 123 join_reserve: 4 join_pic: 31998"
        << " effect: " << c.effect << " centerx: " << c.centers << " centery: " << c.centers
        << " centerz: " << c.centers << " framea: " << c.framea << "\nopoint_end:\n"
        << "opoint:\nkind: 1 oid: 778 action: 0\nopoint_end:\n<frame_end>\n";
    child_dat << "<bmp_begin>\nname: MaterializerChild\nweapon_hp: 17\n<bmp_end>\n"
        << "<stats> defend: " << c.defend << " <stats_end>\n";
    if (c.declared) child_dat << "<frame> " << c.action << " child\nstate: " << c.state
        << " wait: 100 next: 0\n<frame_end>\n";
    auto parse = [](const std::string& text) {
        auto result = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text));
        if (!result->ok()) throw std::runtime_error("materializer fixture parse failed");
        return result;
    };
    auto parent = parse(parent_dat.str()), child = parse(child_dat.str());
    auto neutral = parse("<bmp_begin>\nname: Neutral\n<bmp_end>\n<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n");
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(31980, 0, "parent.dat", parent);
    if (!c.missing) catalog.upsert_definition(c.oid, c.type, "child.dat", child);
    catalog.upsert_definition(778, 5, "control.dat", neutral);
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 request;
    request.object_id = 31980; request.object_type = 0; request.definition = parent;
    request.hp = 500; request.mp = 500; request.owner_slot = 9; request.battle_group = 3;
    request.position.x = 300; request.position.y = -20; request.position.z = 250;
    if (!world.spawn_at(20, request).success) throw std::runtime_error("parent spawn failed");
    for (int slot = 50 + c.free_slots; slot < 1000; ++slot) {
        request.object_id = 31999; request.object_type = 5; request.definition = neutral;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("filler spawn failed");
    }
    auto& p = *world.entity(20);
    p.ordinary_credit_gate_2f4 = c.credit;
    p.render_phase_008 = 11;
    p.hit_resource_injury_double_1a0 = 2;
    p.hit_resource_suppression_15c = 1;
    p.input.current.set(ntsd28::InputKey28::depth_up, (c.input & 1) != 0);
    p.input.current.set(ntsd28::InputKey28::depth_down, (c.input & 2) != 0);
    world.snapshot_actions();
    std::cout << "{\"index\":" << index << ",\"name\":\"" << c.name
        << "\",\"seed\":42,\"type\":" << c.type << ",\"kind\":" << c.kind
        << ",\"oid\":" << c.oid << ",\"credit\":" << c.credit << ",\"input\":" << c.input
        << ",\"freeSlots\":" << c.free_slots << ",\"sourceDat\":\"" << json_escape(parent_dat.str())
        << "\",\"targetDat\":\"" << json_escape(child_dat.str()) << "\",\"before\":";
    emit_world(world);
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = world.materialize_supported_spawns(20, catalog);
    std::cout << ",\"result\":{\"success\":" << (result.success ? "true" : "false")
        << ",\"spawned\":" << result.spawned << ",\"rejected\":" << result.rejected
        << ",\"missing\":" << result.skipped_native_missing_definition
        << ",\"noSlot\":" << result.skipped_native_no_slot << "},\"after\":";
    emit_world(world);
    std::cout << ",\"calls\":"; emit_rng();
    if (c.follow) {
        ntsd28::SimulationTickOptions28 options;
        options.stage_bounds = ntsd28::StageBounds28{800,180,350};
        options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
        direct_crt_calls.clear(); direct_synchronized_calls.clear();
        const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
        std::cout << ",\"following\":"; emit_world(world);
        std::cout << ",\"followingCalls\":"; emit_rng();
        std::cout << ",\"followingLifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false");
    }
    std::cout << "}\n";
}

int wmain() {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        std::vector<MaterializerCase> cases;
        auto add = [&](const char* name) -> MaterializerCase& {
            cases.push_back(MaterializerCase{}); cases.back().name = name; return cases.back();
        };
        add("first_oid_zero").oid = 0;
        add("first_oid_negative").oid = -1;
        add("first_kind_zero").kind = 0;
        add("first_kind_negative").kind = -1;
        auto& high = add("random_action1001"); high.action = 999; high.framea = 5; high.follow = true;
        auto& terminal = add("random_action1000"); terminal.action = 998; terminal.framea = 5; terminal.follow = true;
        for (std::size_t i = 0; i < cases.size(); ++i) run_case(static_cast<int>(i), cases[i]);
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
