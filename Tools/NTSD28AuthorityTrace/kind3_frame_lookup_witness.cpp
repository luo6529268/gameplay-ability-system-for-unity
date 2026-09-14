#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

static std::shared_ptr<const ntsd28::DatDocument> catch_definition(bool attacker, int catching, int caught, bool declared) {
    int action = attacker ? catching : caught;
    if (action < 0) action = -action;
    std::ostringstream text;
    text << "<bmp_begin>\nname: NativeCatchLookup\n<bmp_end>\n"
        << "<frame> 0 initial\nstate: 0 wait: 100 next: 0\n";
    if (attacker) text << "itr:\nkind: 3 x: -20 y: -20 w: 60 h: 60 catchingact: " << catching
        << " caughtact: " << caught << " respond: 73\nitr_end:\n";
    else text << "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
    text << "<frame_end>\n";
    if (declared && action > 0 && action <= 999) text << "<frame> " << action << " relation\nstate: " << (attacker ? 9 : 10)
        << " wait: 100 next: 0 centerx: " << (attacker ? 3 : 5) << " centery: " << (attacker ? 7 : 11)
        << "\ncpoint:\nkind: " << (attacker ? 1 : 2) << " x: " << (attacker ? 13 : 17) << " y: 19\ncpoint_end:\n<frame_end>\n";
    auto document = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!document->ok()) throw std::runtime_error("kind3 fixture DAT rejected");
    return document;
}

static void write_catch_entity(const ntsd28::EntityState28& entity) {
    std::cout << "{\"raw\":";
    write_entity(std::cout, entity, 1);
    std::cout << ",\"catchTarget\":" << entity.catch_target_slot_8c << ",\"catchSource\":" << entity.catch_source_slot_90
        << ",\"timeout\":" << entity.catch_timeout_94 << '}';
}

static void emit_kind3(int catching, int caught, bool attacker_declared, bool target_declared, bool reversed, int index) {
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    for (int slot : {0, 1}) {
        ntsd28::SpawnRequest28 request;
        request.object_id = slot == 0 ? 77 : 78; request.object_type = 0;
        request.definition = catch_definition(slot == 0, catching, caught, slot == 0 ? attacker_declared : target_declared);
        request.hp = 500; request.mp = 500;
        request.battle_group = slot + 1;
        request.position.x = slot == 0 ? (reversed ? 110 : 100) : (reversed ? 100 : 110);
        request.position.y = slot == 0 ? -5 : -6; request.position.z = 200;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("kind3 fixture spawn failed");
        auto* entity = world.entity(slot);
        entity->position.precise_x = request.position.x + (slot == 0 ? .25 : .75);
        entity->position.precise_y = slot == 0 ? -5.5 : -6.25;
        entity->frame.action_latch = slot == 0 ? 11 : 12;
        entity->frame.frame_counter = slot == 0 ? 7 : 9;
        entity->motion.x = slot == 0 ? -7.25 : 3.25;
        entity->motion.y = slot == 0 ? 8.5 : -4.5;
        entity->motion.z = slot == 0 ? 9.75 : -5.75;
    }
    world.entity(0)->catch_target_slot_8c = 77;
    world.entity(0)->catch_timeout_94 = 88;
    world.entity(1)->catch_source_slot_90 = 66;
    world.entity(1)->hit_reaction_timer = 44;
    world.snapshot_actions();
    if (world.rebuild_geometric_hit_candidates().candidates_appended != 1) throw std::runtime_error("kind3 fixture expected one candidate");
    std::cout << "{\"index\":" << index << ",\"catching\":" << catching << ",\"caught\":" << caught
        << ",\"attackerDeclared\":" << (attacker_declared ? "true" : "false") << ",\"targetDeclared\":" << (target_declared ? "true" : "false")
        << ",\"reversed\":" << (reversed ? "true" : "false") << ",\"before\":[";
    write_catch_entity(*world.entity(0)); std::cout << ','; write_catch_entity(*world.entity(1));
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = world.resolve_kind3_catch_relation(0, 0);
    std::cout << "],\"applied\":" << (result.status == ntsd28::WorldRelationHitStatus28::applied ? "true" : "false")
        << ",\"message\":\"" << json_escape(result.message) << "\",\"nativeCalls\":" << direct_synchronized_calls.size()
        << ",\"crtCalls\":" << direct_crt_calls.size() << ",\"after\":[";
    write_catch_entity(*world.entity(0)); std::cout << ','; write_catch_entity(*world.entity(1));
    std::cout << "]}\n";
}

int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        for (int catching : {0,10,99,856,857,998,999,1000,-998,-999})
        for (int caught : {0,10,99,856,857,998,999,1000,-998,-999})
        for (bool a : {false,true}) for (bool t : {false,true}) for (bool reversed : {false,true})
            emit_kind3(catching, caught, a, t, reversed, index++);
        if (index != 800) throw std::runtime_error("kind3 case count changed");
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
