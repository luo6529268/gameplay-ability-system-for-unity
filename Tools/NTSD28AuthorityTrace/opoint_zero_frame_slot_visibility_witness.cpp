#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static std::shared_ptr<const ntsd28::DatDocument> parse_birth_dat(
    const std::string& text) {
    auto document = std::make_shared<const ntsd28::DatDocument>(
        ntsd28::DatParser{}.parse_text(text));
    if (!document->ok()) throw std::runtime_error("birth witness DAT rejected");
    return document;
}

static void emit_birth_world(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":[";
    bool first = true;
    for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots;
         ++slot) {
        const auto* entity = world.entity(slot);
        if (!entity) continue;
        if (!first) std::cout << ',';
        first = false;
        std::cout << "{\"slot\":" << slot
                  << ",\"oid\":" << entity->object_id
                  << ",\"action\":" << entity->frame.action
                  << ",\"counter\":" << entity->frame.frame_counter
                  << ",\"latch\":" << entity->opoint_action_latch
                  << ",\"hold\":" << entity->motion_hold_timer << '}';
    }
    std::cout << "]}";
}

static void run_birth_case(const char* name, int parent_slot,
                           int initial_hold, int ticks) {
    const std::string parent_dat =
        "<bmp_begin>\nname: BirthParent\n<bmp_end>\n"
        "<frame> 0 repeat\nstate: 0 wait: 0 next: 0\n"
        "opoint:\nkind: 1 oid: 780 action: 0 x: 0 y: 0 z: 0"
        " dvx: 0 dvy: 0 dvz: 0 facing: 0\nopoint_end:\n"
        "<frame_end>\n";
    const std::string child_dat =
        "<bmp_begin>\nname: BirthChild\n<bmp_end>\n"
        "<frame> 0 child\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
    auto parent = parse_birth_dat(parent_dat);
    auto child = parse_birth_dat(child_dat);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(779, 0, "birth-parent.dat", parent);
    catalog.upsert_definition(780, 5, "birth-child.dat", child);
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 request;
    request.object_id = 779;
    request.object_type = 0;
    request.definition = parent;
    request.initial_action = 0;
    request.hp = 500;
    request.mp = 500;
    request.position.x = 300;
    request.position.y = -20;
    request.position.z = 250;
    if (!world.spawn_at(parent_slot, request).success)
        throw std::runtime_error("birth parent spawn failed");
    world.entity(parent_slot)->motion_hold_timer = initial_hold;

    ntsd28::SimulationTickOptions28 options;
    options.stage_bounds = ntsd28::StageBounds28{800, 180, 350};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    std::cout << "{\"name\":\"" << name << "\",\"seed\":42"
              << ",\"parentSlot\":" << parent_slot
              << ",\"initialHold\":" << initial_hold
              << ",\"parentDat\":\"" << json_escape(parent_dat)
              << "\",\"childDat\":\"" << json_escape(child_dat)
              << "\",\"before\":";
    emit_birth_world(world);
    std::cout << ",\"ticks\":[";
    for (int tick = 0; tick < ticks; ++tick) {
        if (tick) std::cout << ',';
        const auto result = ntsd28::SimulationTickDriver28{}.step(
            world, catalog, options);
        std::cout << "{\"index\":" << tick
                  << ",\"spawned\":" << result.spawns.spawned
                  << ",\"world\":";
        emit_birth_world(world);
        std::cout << '}';
    }
    std::cout << "]}\n";
}

int wmain() {
    try {
        run_birth_case("repeat_zero_frame", 20, 0, 3);
        run_birth_case("unscanned_high_slot", 50, 0, 1);
        run_birth_case("already_scanned_low_slot", 60, 0, 2);
        run_birth_case("surviving_motion_hold", 20, 2, 2);
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
