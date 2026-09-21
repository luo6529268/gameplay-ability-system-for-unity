#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static bool capture_fulltick = false;

struct PlatformCase {
    const char* name;
    int operation = 30, target_type = 0, target_state = 0;
    int target_x = 100, target_z = 250, previous_source_y = 0;
    int source_group = 1, target_group = 1;
    double itr_dvy = 0;
    int source_slot = 20, extra_slot = 22, extra_y = 0;
    bool left = false, split_frame = false, remove_source = false, physics_after = false;
};

static void emit_target(const ntsd28::EntityState28& e) {
    std::cout << "{\"position\":[" << e.position.x << ',' << e.position.y << ',' << e.position.z
        << ',' << e.position.precise_x << ',' << e.position.precise_y << ',' << e.position.precise_z
        << "],\"previousY\":" << e.position.previous_y
        << ",\"previousXYZ\":[" << e.position.previous_x << ',' << e.position.previous_y << ',' << e.position.previous_z << ']'
        << ",\"reference\":" << e.collision_y_reference
        << ",\"platformSlot\":" << e.platform_source_slot_f4
        << ",\"shadow\":" << e.render_shadow_offset_10c << '}';
}

static void run_case(const PlatformCase& c) {
    if (capture_fulltick && std::string(c.name) != "nominal" &&
        std::string(c.name) != "left_facing_motion" &&
        std::string(c.name) != "fractional_itr_negative") return;
    std::string platform_text = "<bmp_begin>\nname: Platform\n<bmp_end>\n<frame> 0 platform\n"
        "state: 3003 wait: 100 next: 0 attacking: 1 centerx: 0 dvx: 2.5 dvy: 1.5 dvz: -2.5\n"
        "itr:\nkind: " + std::to_string(c.operation) + " x: -10 y: 0 w: 20 h: 10 zwidth: 15 dvy: " + std::to_string(c.itr_dvy) + "\nitr_end:\n<frame_end>\n";
    if (c.split_frame) platform_text += "<frame> 1 current\nstate: 0 wait: 100 next: 0 attacking: 0 centerx: 100 dvx: 10.5 dvy: 1.5 dvz: -2.5\nitr:\nkind: 30 x: -10 y: 0 w: 20 h: 10 zwidth: 15\nitr_end:\n<frame_end>\n";
    if (capture_fulltick) {
        const auto at = platform_text.find("dvy: 1.5");
        platform_text.replace(at, std::string("dvy: 1.5").size(), "dvy: 535");
    }
    std::string target_text = "<bmp_begin>\nname: Rider\n<bmp_end>\n<frame> 0 rider\nstate: " +
        std::to_string(c.target_state) + " wait: 100 next: 0\n<frame_end>\n";
    auto parse = [](const std::string& text) {
        auto data = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text));
        if (!data->ok()) throw std::runtime_error("platform witness DAT invalid");
        return data;
    };
    auto platform = parse(platform_text), rider = parse(target_text);
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 request;
    request.object_id = 31980; request.object_type = 3; request.definition = platform;
    request.hp = 500; request.mp = 500; request.battle_group = c.source_group;
    request.position.x = 100; request.position.y = capture_fulltick ? -5 : -20; request.position.z = 250;
    if (!world.spawn_at(c.source_slot, request).success) throw std::runtime_error("platform spawn failed");
    if (c.extra_y != 0) {
        request.position.y = c.extra_y;
        if (!world.spawn_at(c.extra_slot, request).success) throw std::runtime_error("extra platform spawn failed");
        world.entity(c.extra_slot)->position.previous_y = 0;
    }
    request.object_id = 31981; request.object_type = c.target_type; request.definition = rider;
    request.battle_group = c.target_group;
    request.position.x = c.target_x; request.position.y = -10; request.position.z = c.target_z;
    if (!world.spawn_at(21, request).success) throw std::runtime_error("rider spawn failed");
    world.entity(c.source_slot)->position.previous_y = c.previous_source_y;
    world.entity(c.source_slot)->frame.facing = c.left;
    auto& target = *world.entity(21);
    target.position.previous_y = -10;
    target.collision_y_reference = capture_fulltick ? 0 : -999;
    target.platform_source_slot_f4 = capture_fulltick ? 0 : 99;
    target.render_shadow_offset_10c = capture_fulltick ? 0 : -999;
    world.snapshot_actions();
    if (c.split_frame) world.entity(c.source_slot)->frame.action = 1;
    std::cout << "{\"name\":\"" << c.name << "\",\"operation\":" << c.operation
        << ",\"targetType\":" << c.target_type << ",\"targetState\":" << c.target_state
        << ",\"params\":{\"targetX\":" << c.target_x << ",\"targetZ\":" << c.target_z
        << ",\"previousSourceY\":" << c.previous_source_y << ",\"sourceGroup\":" << c.source_group
        << ",\"targetGroup\":" << c.target_group << ",\"itrDvy\":" << c.itr_dvy
        << ",\"sourceSlot\":" << c.source_slot << ",\"extraSlot\":" << c.extra_slot << ",\"extraY\":" << c.extra_y
        << ",\"left\":" << (c.left ? "true" : "false") << ",\"splitFrame\":" << (c.split_frame ? "true" : "false")
        << ",\"removeSource\":" << (c.remove_source ? "true" : "false") << ",\"physicsAfter\":" << (c.physics_after ? "true" : "false") << '}'
        << ",\"sourceDat\":\"" << json_escape(platform_text) << "\",\"targetDat\":\""
        << json_escape(target_text) << "\",\"before\":";
    emit_target(target);
    if (capture_fulltick) {
        std::cout << ",\"platformBefore\":";
        emit_target(*world.entity(c.source_slot));
        ntsd28::ObjectDefinitionCatalog28 catalog;
        catalog.upsert_definition(31980, 3, "platform.dat", platform);
        catalog.upsert_definition(31981, c.target_type, "rider.dat", rider);
        ntsd28::SimulationTickOptions28 options;
        options.stage_bounds = ntsd28::StageBounds28{800, 180, 350};
        options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
        std::cout << ",\"ticks\":[";
        for (int tick_index = 1; tick_index <= 2; ++tick_index) {
            const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
            if (tick_index > 1) std::cout << ',';
            std::cout << "{\"tick\":" << tick_index << ",\"lifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false")
                << ",\"rider\":";
            emit_target(*world.entity(21));
            std::cout << ",\"platform\":";
            emit_target(*world.entity(c.source_slot));
            std::cout << '}';
        }
        std::cout << "]}\n";
        return;
    }
    const auto candidates = world.rebuild_geometric_hit_candidates();
    std::cout << ",\"candidateSuccess\":" << (candidates.success ? "true" : "false")
        << ",\"scanned\":" << candidates.platform_interactions_scanned
        << ",\"armed\":" << candidates.platform_links_armed << ",\"afterCandidate\":";
    emit_target(target);
    if (c.remove_source) {
        if (!world.despawn(c.source_slot).success) throw std::runtime_error("despawn failed");
    }
    const auto motion = world.apply_frame_motion(21, static_cast<ntsd28::DepthIntent28>(0));
    std::cout << ",\"motionSuccess\":" << (motion.success ? "true" : "false")
        << ",\"linkedMotion\":" << (motion.linked_platform_motion_applied ? "true" : "false")
        << ",\"afterMotion\":";
    emit_target(target);
    if (c.physics_after) {
        const auto physics = world.step_physics(21, ntsd28::PhysicsContext28{});
        std::cout << ",\"afterPhysics\":";
        emit_target(target);
    }
    std::cout << "}\n";
}

int wmain(int argc, wchar_t** argv) {
    try {
        capture_fulltick = argc == 2 && std::wstring(argv[1]) == L"--fulltick";
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        run_case({"nominal"});
        auto c = PlatformCase{"strict_x_edge"}; c.target_x = 90; run_case(c);
        c = PlatformCase{"strict_z_edge"}; c.target_z = 265; run_case(c);
        c = PlatformCase{"previous_y_reject"}; c.previous_source_y = -30; run_case(c);
        c = PlatformCase{"type5_reject30"}; c.target_type = 5; run_case(c);
        c = PlatformCase{"type5_accept60"}; c.target_type = 5; c.operation = 60; run_case(c);
        c = PlatformCase{"same_group40"}; c.operation = 40; run_case(c);
        c = PlatformCase{"same_group_reject50"}; c.operation = 50; run_case(c);
        c = PlatformCase{"type3_state0_no_motion"}; c.target_type = 3; run_case(c);
        c = PlatformCase{"type3_state3000_motion"}; c.target_type = 3; c.target_state = 3000; run_case(c);
        c = PlatformCase{"competing_more_negative_later"}; c.extra_y = -30; run_case(c);
        c = PlatformCase{"competing_more_negative_earlier"}; c.extra_y = -30; c.extra_slot = 19; run_case(c);
        c = PlatformCase{"competing_tie_later"}; c.extra_y = -20; run_case(c);
        c = PlatformCase{"competing_tie_earlier"}; c.extra_y = -20; c.extra_slot = 19; run_case(c);
        c = PlatformCase{"current_itr_snapshot_geometry"}; c.split_frame = true; run_case(c);
        c = PlatformCase{"fractional_itr_positive"}; c.itr_dvy = 2.5; run_case(c);
        c = PlatformCase{"fractional_itr_negative"}; c.itr_dvy = -2.5; run_case(c);
        c = PlatformCase{"left_facing_motion"}; c.left = true; run_case(c);
        c = PlatformCase{"slot_zero_sentinel"}; c.source_slot = 0; run_case(c);
        c = PlatformCase{"removed_platform"}; c.remove_source = true; run_case(c);
        c = PlatformCase{"physics_history"}; c.physics_after = true; run_case(c);
        return 0;
    } catch (const std::exception& e) { std::cerr << e.what() << '\n'; return 91; }
}
