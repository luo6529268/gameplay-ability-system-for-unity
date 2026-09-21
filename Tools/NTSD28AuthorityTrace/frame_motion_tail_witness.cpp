#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"
#include <iomanip>

static std::string json_quote(const std::string& value) {
    return "\"" + json_escape(value) + "\"";
}

struct TailCase {
    const char* name;
    const char* velocity = "dvx: 0 dvy: 0 dvz: 0";
    const char* displacement = "dx: 0 dy: 0 dz: 0";
    int delay = 0;
    bool left = false;
    int depth = 0;
    bool linked = false;
    bool missing_link = false;
    bool pending = false;
};

static void emit_tail_state(const ntsd28::EntityState28& e) {
    std::cout << "{\"position\":[" << e.position.x << ',' << e.position.y << ',' << e.position.z
        << ',' << e.position.precise_x << ',' << e.position.precise_y << ',' << e.position.precise_z
        << "],\"motion\":[" << e.motion.x << ',' << e.motion.y << ',' << e.motion.z
        << "],\"reference\":" << e.collision_y_reference
        << ",\"delay\":" << e.delay_timer_134 << '}';
}

static bool capture_following = false;

static void run_tail_case(int index, const TailCase& c) {
    const std::string dat = std::string("<bmp_begin>\nname: Tail\n<bmp_end>\n<frame> 0 tail\nstate: 0 wait: 100 next: 0 ")
        + c.velocity + ' ' + c.displacement + "\n<frame_end>\n";
    auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat));
    if (!definition->ok()) throw std::runtime_error("tail DAT parse failed");
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 request;
    request.object_id = 31981; request.object_type = 0; request.definition = definition;
    request.hp = 500; request.mp = 500;
    if (!world.spawn_at(21, request).success) throw std::runtime_error("tail spawn failed");
    auto& e = *world.entity(21);
    e.position.x = 100; e.position.y = -10; e.position.z = 250;
    e.position.precise_x = 100.75; e.position.precise_y = -10.25; e.position.precise_z = 250.5;
    e.motion.x = 4; e.motion.y = -3; e.motion.z = 2;
    e.frame.facing = c.left; e.delay_timer_134 = c.delay;
    e.lifecycle_resolution_pending = c.pending;
    e.collision_y_reference = -10;
    const std::string platform_dat = "<bmp_begin>\nname: Platform\n<bmp_end>\n<frame> 0 platform\nstate: 3003 wait: 100 next: 0 dvx: 1.5 dvy: 2.5 dvz: -2.5\n<frame_end>\n";
    if (c.linked || c.missing_link) {
        e.platform_source_slot_f4 = 20;
        if (!c.missing_link) {
            request.object_id = 31980; request.object_type = 3;
            request.definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(platform_dat));
            if (!world.spawn_at(20, request).success) throw std::runtime_error("platform spawn failed");
        }
    }
    std::cout << "{\"index\":" << index << ",\"name\":" << json_quote(c.name)
        << ",\"dat\":" << json_quote(dat) << ",\"platformDat\":" << json_quote(platform_dat)
        << ",\"left\":" << (c.left ? "true" : "false") << ",\"depth\":" << c.depth
        << ",\"linked\":" << (c.linked ? "true" : "false")
        << ",\"missingLink\":" << (c.missing_link ? "true" : "false")
        << ",\"pending\":" << (c.pending ? "true" : "false") << ",\"before\":";
    emit_tail_state(e);
    const auto result = world.apply_frame_motion(21, static_cast<ntsd28::DepthIntent28>(c.depth));
    std::cout << ",\"success\":" << (result.success ? "true" : "false") << ",\"after\":";
    emit_tail_state(e);
    if (capture_following && (index == 1 || index == 7 || index == 9)) {
        ntsd28::ObjectDefinitionCatalog28 catalog;
        catalog.upsert_definition(31981, 0, "tail.dat", definition);
        catalog.upsert_definition(31980, 3, "platform.dat",
            std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(platform_dat)));
        ntsd28::SimulationTickOptions28 options;
        options.stage_bounds = ntsd28::StageBounds28{800, 180, 350};
        options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
        const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
        std::cout << ",\"following\":";
        emit_tail_state(*world.entity(21));
        std::cout << ",\"followingRaw\":";
        write_entity(std::cout, *world.entity(21), 1);
        std::cout << ",\"followingLifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false");
    }
    std::cout << "}\n";
}

int wmain(int argc, wchar_t** argv) {
    try {
        capture_following = argc == 2 && std::wstring(argv[1]) == L"--following";
        std::cout << std::setprecision(17);
        int index = 0;
        run_tail_case(index++, TailCase{"neutral"});
        auto c = TailCase{"delay_positive"}; c.delay = 1; run_tail_case(index++, c);
        c = TailCase{"delay_negative"}; c.delay = -1; run_tail_case(index++, c);
        c = TailCase{"own_fractional_velocity_is_not_integer"}; c.velocity = "dvx: 2.5 dvy: 1.5 dvz: -2.5"; run_tail_case(index++, c);
        c = TailCase{"integer_velocity_depth"}; c.velocity = "dvx: 4 dvy: 2 dvz: 3"; c.depth = -1; run_tail_case(index++, c);
        c = TailCase{"position_x"}; c.displacement = "dx: 2.5"; run_tail_case(index++, c);
        c = TailCase{"position_x_left"}; c.displacement = "dx: 2.5"; c.left = true; run_tail_case(index++, c);
        c = TailCase{"position_all_quarter"}; c.displacement = "dx: 10 dy: -6 dz: 2"; c.delay = 2; run_tail_case(index++, c);
        c = TailCase{"override_then_quarter"}; c.velocity = "dvx: 552 dvy: 551 dvz: 548"; c.delay = 1; run_tail_case(index++, c);
        c = TailCase{"linked_then_own_position"}; c.displacement = "dx: 2.5 dy: -1.5 dz: 0.5"; c.linked = true; run_tail_case(index++, c);
        c = TailCase{"pending_skips_all"}; c.displacement = "dx: 10 dy: -6 dz: 2"; c.delay = 1; c.pending = true; run_tail_case(index++, c);
        c = TailCase{"missing_link_still_own_position"}; c.displacement = "dx: 2.5"; c.missing_link = true; run_tail_case(index++, c);
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
