#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static void emit_rests(ntsd28::BattleWorld28& world) {
    std::vector<std::size_t> slots;
    for (std::size_t s = 0; s < 1000; ++s) if (world.entity(s)) slots.push_back(s);
    std::cout << "{\"slots\":[";
    for (std::size_t i = 0; i < slots.size(); ++i) {
        if (i) std::cout << ',';
        const auto& e = *world.entity(slots[i]);
        std::cout << "{\"slot\":" << slots[i] << ",\"oid\":" << e.object_id
            << ",\"attackerRest\":" << e.attacker_rest << ",\"victimRest\":[";
        for (std::size_t j = 0; j < slots.size(); ++j) {
            if (j) std::cout << ',';
            std::cout << static_cast<int>(world.victim_rest(slots[i], slots[j]));
        }
        std::cout << "]}";
    }
    std::cout << "]}";
}

static void run_case(int state, int count) {
    const std::string parent_text = "<bmp_begin>\nname: Parent\n<bmp_end>\n<frame> 0 parent\nstate: " +
        std::to_string(state) + " wait: 100 next: 0\nopoint:\nkind: 1 oid: 999 action: 0 facing: " +
        std::to_string(count * 10) + "\nopoint_end:\n<frame_end>\n";
    const std::string neutral_text = "<bmp_begin>\nname: Neutral\n<bmp_end>\n<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
    auto parse = [](const std::string& text) {
        auto dat = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text));
        if (!dat->ok()) throw std::runtime_error("rest witness DAT invalid");
        return dat;
    };
    auto parent = parse(parent_text), neutral = parse(neutral_text);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(31980, 3, "parent.dat", parent);
    catalog.upsert_definition(31981, 5, "linked.dat", neutral);
    catalog.upsert_definition(999, 5, "child.dat", neutral);
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 request;
    request.object_id = 31980; request.object_type = 3; request.definition = parent;
    request.hp = 500; request.mp = 500;
    request.position.x = 300; request.position.z = 250;
    if (!world.spawn_at(20, request).success) throw std::runtime_error("parent spawn failed");
    request.object_id = 31981; request.object_type = 5; request.definition = neutral;
    if (!world.spawn_at(21, request).success) throw std::runtime_error("linked spawn failed");
    world.entity(20)->control_slot_000 = 21;
    world.entity(21)->victim_rest_by_attacker[20] = 7;
    world.snapshot_actions();
    std::cout << "{\"state\":" << state << ",\"count\":" << count << ",\"before\":";
    emit_rests(world);
    const auto result = world.materialize_supported_spawns(20, catalog);
    std::cout << ",\"spawned\":" << result.spawned << ",\"after\":";
    emit_rests(world);
    ntsd28::SimulationTickOptions28 options;
    options.stage_bounds = ntsd28::StageBounds28{800,180,350};
    options.controls.resize(1000);
    const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
    std::cout << ",\"followingSuccess\":" << (tick.lifecycle.success ? "true" : "false") << ",\"following\":";
    emit_rests(world);
    std::cout << "}\n";
}

int wmain() {
    try {
        run_case(3003, 2);
        run_case(3003, 4);
        run_case(0, 4);
        return 0;
    } catch (const std::exception& e) { std::cerr << e.what() << '\n'; return 91; }
}
