#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_subject(const ntsd28::EntityState28* entity) {
    if (!entity) { std::cout << "null"; return; }
    std::cout << "{\"raw\":";
    write_entity(std::cout, *entity, 1);
    std::cout << ",\"link\":" << entity->interaction_state
              << ",\"parent\":" << entity->linked_parent_slot
              << ",\"child\":" << entity->linked_child_slot
              << ",\"impulseY\":" << entity->pending_hit_impulse.total.y << '}';
}

static void run_dead_case(int phase, int action, int state, int hp, int motion, int relation, int render) {
    const int y = motion == 0 ? 0 : motion == 1 ? -20 : -1;
    const double vy = motion == 0 ? 0.0 : motion == 1 ? 2.0 : 10.0;
    std::ostringstream dat;
    dat << "<bmp_begin>\nname: DeadSource\n<bmp_end>\n<frame> " << action
        << " current\nstate: " << state << " wait: 100 next: 0 centerx: 0 centery: 0\n"
        << "wpoint:\nkind: " << (relation == 3 ? 3 : 1)
        << " x: 0 y: 0 weaponact: 0 cover: 2 dvx: " << (relation == 3 ? 6 : 0)
        << " dvy: 0 dvz: 0\nwpoint_end:\n<frame_end>\n";
    auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat.str()));
    auto child_definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(
        "<bmp_begin>\nname: HeldChild weapon_hp: 99\n<bmp_end>\n"
        "<frame> 0 held\nstate: 1001 wait: 100 next: 0 centerx: 0 centery: 0\n"
        "wpoint:\nkind: 1 x: 0 y: 0 weaponact: 0 cover: 2\nwpoint_end:\n<frame_end>\n"));
    if (!definition->ok() || !child_definition->ok()) throw std::runtime_error("dead witness DAT rejected");
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(777, 0, "parent.dat", definition);
    catalog.upsert_definition(888, relation == 2 ? 2 : 1, "held.dat", child_definition);
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 request;
    request.object_id = 777; request.object_type = 0; request.definition = definition;
    request.initial_action = action; request.hp = 500; request.mp = 500;
    request.position.x = 100; request.position.y = y; request.position.z = 100;
    if (!world.spawn_at(0, request).success) throw std::runtime_error("parent spawn failed");
    auto* parent = world.entity(0);
    parent->current_hp = hp; parent->motion.y = vy; parent->motion.x = 8.0;
    parent->render_phase_008 = render;
    if (relation != 0) {
        request.object_id = 888; request.object_type = relation == 2 ? 2 : 1;
        request.definition = child_definition; request.initial_action = 0;
        if (!world.spawn_at(70, request).success) throw std::runtime_error("held spawn failed");
        parent->interaction_state = relation == 2 ? 2 : 1;
        parent->linked_child_slot = 70;
        auto* child = world.entity(70);
        child->interaction_state = relation == 2 ? -2 : -1;
        child->linked_parent_slot = 0;
    }
    std::cout << "{\"phase\":" << phase << ",\"action\":" << action << ",\"state\":" << state
              << ",\"hp\":" << hp << ",\"motion\":" << motion << ",\"relation\":" << relation
              << ",\"render\":" << render << ",\"before\":";
    write_subject(parent);
    std::vector<std::string> messages;
    if (phase == 0) {
        const auto frames = world.step_frame_slot(0);
        for (const auto& frame : frames) if (!frame.frame.ok()) throw std::runtime_error(frame.frame.message);
        const auto lifecycle = world.resolve_pending_lifecycle(0);
        if (!lifecycle.success) throw std::runtime_error("lifecycle failed");
    } else {
        const auto result = ntsd28::SimulationTickDriver28{}.step(world, catalog);
        for (const auto& frame : result.frames) if (!frame.frame.ok()) throw std::runtime_error(frame.frame.message);
        if (!result.lifecycle.success) throw std::runtime_error("full lifecycle failed");
        messages = result.diagnostics.messages;
    }
    std::cout << ",\"after\":";
    write_subject(world.entity(0));
    std::cout << ",\"heldAfter\":";
    write_subject(world.entity(70));
    std::cout << ",\"messages\":[";
    for (std::size_t i = 0; i < messages.size(); ++i) {
        if (i != 0) std::cout << ',';
        std::cout << '\"' << json_escape(messages[i]) << '\"';
    }
    std::cout << "],\"syncCalls\":" << world.random().state().synchronized.calls << "}\n";
}

int wmain() {
    try {
        for (int phase : {0, 1})
        for (int action : {0,5,11,12,110,111,180,184,185,186,189,190,212,214,215})
        for (int state : {0,12,14})
        for (int hp : {-1,0,500})
        for (int motion : {0,1,2})
        for (int relation : {0,1,2,3})
        for (int render : {0,3})
            run_dead_case(phase, action, state, hp, motion, relation, render);
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n'; return 91;
    }
}
