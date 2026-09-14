#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

static std::shared_ptr<const ntsd28::DatDocument> collision_definition(
    bool attacker, int snapshot, bool declared, bool replacement) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: NativeCollisionLookup\n<bmp_end>\n";
    const auto append_frame = [&](int action) {
        text << "<frame> " << action << " collision\nstate: 0 wait: 100 next: 0 centerx: "
             << (replacement && !attacker ? 500 : 0) << " centery: 0\n";
        if (attacker) text << "itr:\nkind: 0 x: -20 y: -20 w: 60 h: 60 injury: 1\nitr_end:\n";
        else text << "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
        text << "cpoint:\nkind: " << (attacker ? 1 : 2)
             << " decrease: " << (replacement ? 5 : 2)
             << " hurtable: 1\ncpoint_end:\n<frame_end>\n";
    };
    append_frame(0);
    append_frame(10);
    if (declared && snapshot > 0 && snapshot != 10 && snapshot <= 999) append_frame(snapshot);
    auto document = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!document->ok()) throw std::runtime_error("collision fixture DAT rejected");
    return document;
}

static void write_collision_entity(const ntsd28::EntityState28& entity) {
    const auto* current = entity.definition->frame(entity.frame.action);
    const auto* snapshot = entity.definition->frame(entity.frame.tick_action_snapshot);
    std::cout << "{\"raw\":";
    write_entity(std::cout, entity, 1);
    std::cout << ",\"catchTarget\":" << entity.catch_target_slot_8c
              << ",\"catchSource\":" << entity.catch_source_slot_90
              << ",\"timeout\":" << entity.catch_timeout_94
              << ",\"currentAvailable\":" << (current ? "true" : "false")
              << ",\"snapshotAvailable\":" << (snapshot ? "true" : "false")
              << ",\"snapshotCenterX\":" << (snapshot ? snapshot->values.integer("centerx").value_or(0) : 0)
              << ",\"snapshotBlocks\":" << (snapshot ? snapshot->subblocks.size() : 0) << '}';
}

static void emit_collision(int snapshot, bool declared, int current, int replace_side, bool both, int index) {
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    for (int slot : {0, 1}) {
        ntsd28::SpawnRequest28 request;
        request.object_id = 77 + slot;
        request.object_type = 0;
        request.definition = collision_definition(slot == 0, snapshot, declared, false);
        request.hp = 500;
        request.mp = 500;
        request.battle_group = slot + 1;
        request.position.x = 100 + slot * 10;
        request.position.y = -5;
        request.position.z = 200;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("collision fixture spawn failed");
        world.entity(slot)->frame.action = slot == 0 || both ? snapshot : 0;
    }
    world.snapshot_actions();
    for (int slot : {0, 1}) {
        auto* entity = world.entity(slot);
        entity->frame.action = current;
        entity->frame.action_latch = 11 + slot;
        entity->frame.frame_counter = 7 + slot;
        if (replace_side == slot + 1) entity->definition = collision_definition(slot == 0, snapshot, declared, true);
    }
    world.entity(0)->catch_target_slot_8c = 1;
    world.entity(0)->catch_timeout_94 = 80;
    world.entity(1)->catch_source_slot_90 = 0;
    std::cout << "{\"index\":" << index << ",\"snapshot\":" << snapshot
              << ",\"declared\":" << (declared ? "true" : "false")
              << ",\"current\":" << current << ",\"replaceSide\":" << replace_side
              << ",\"both\":" << (both ? "true" : "false") << ",\"before\":[";
    write_collision_entity(*world.entity(0)); std::cout << ','; write_collision_entity(*world.entity(1));
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto collection = world.rebuild_geometric_hit_candidates();
    std::cout << "],\"collectionSuccess\":" << (collection.success ? "true" : "false")
              << ",\"candidates\":" << collection.candidates_appended
              << ",\"collectionDiagnostics\":" << collection.diagnostics.size() << ",\"afterCollection\":[";
    write_collision_entity(*world.entity(0)); std::cout << ','; write_collision_entity(*world.entity(1));
    const auto caught = world.advance_catch_relations();
    std::cout << "],\"catchSuccess\":" << (caught.success ? "true" : "false")
              << ",\"activeRelations\":" << caught.active_relations
              << ",\"brokenRelations\":" << caught.broken_relations
              << ",\"timeoutChanges\":" << caught.timeout_changes << ",\"afterCatch\":[";
    write_collision_entity(*world.entity(0)); std::cout << ','; write_collision_entity(*world.entity(1));
    std::cout << "],\"nativeCalls\":" << direct_synchronized_calls.size()
              << ",\"crtCalls\":" << direct_crt_calls.size() << "}\n";
}

int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        for (int snapshot : {0, 99, 857, 998, 999, 1000, -1})
        for (bool declared : {false, true})
        for (int current : {0, 10, 1000})
        for (int replace_side : {0, 1, 2})
        for (bool both : {false, true})
            emit_collision(snapshot, declared, current, replace_side, both, index++);
        if (index != 252) throw std::runtime_error("collision case count changed");
        for (int snapshot : {0, 99, 857, 998, 999, 1000, -1})
        for (bool declared : {false, true})
        for (int replace_side : {0, 1, 2})
        for (bool both : {false, true})
            emit_collision(snapshot, declared, snapshot, replace_side, both, index++);
        if (index != 336) throw std::runtime_error("collision coincident-action case count changed");
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
