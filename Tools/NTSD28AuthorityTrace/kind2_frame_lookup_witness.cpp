#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

static std::shared_ptr<const ntsd28::DatDocument> pickup_definition(bool holder, int action, bool declared, int weapon_action, bool holder_declared) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: NativePickupLookup\nweapon_hp: 17\n<bmp_end>\n"
        << "<frame> 0 initial\nstate: " << (holder ? 0 : 1004) << " wait: 100 next: 0\n";
    if (holder) text << "itr:\nkind: 2 x: -20 y: -20 w: 60 h: 60\nitr_end:\n";
    else {
        text << "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
        if (action == 0 && declared) text << "wpoint:\nkind: 1 weaponact: " << weapon_action << "\nwpoint_end:\n";
    }
    text << "<frame_end>\n";
    if (!holder && declared && action > 0 && action <= 999)
        text << "<frame> " << action << " target\nstate: 1004 wait: 100 next: 0\nwpoint:\nkind: 1 weaponact: " << weapon_action << "\nwpoint_end:\n<frame_end>\n";
    if (holder && holder_declared) {
        for (int id : {115,116}) text << "<frame> " << id << " holder\nstate: 0 wait: 100 next: 0 dvx: 557\n<frame_end>\n";
        if (weapon_action > 0 && weapon_action <= 999)
            text << "<frame> " << weapon_action << " override\nstate: 0 wait: 100 next: 0 dvx: 553\n<frame_end>\n";
    }
    auto document = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!document->ok()) throw std::runtime_error("pickup DAT rejected");
    return document;
}

static void write_pickup_entity(const ntsd28::EntityState28& e) {
    std::cout << "{\"raw\":"; write_entity(std::cout, e, 1);
    std::cout << ",\"relation\":" << e.interaction_state << ",\"count\":" << e.interaction_relation_count_35c
        << ",\"child\":" << e.linked_child_slot << ",\"parent\":" << e.linked_parent_slot << ",\"descriptor\":";
    const auto* frame = e.definition->frame(e.frame.action);
    if (!frame) std::cout << "null";
    else std::cout << "{\"id\":" << frame->id << ",\"state\":" << frame->values.integer("state").value_or(0)
        << ",\"wait\":" << frame->values.integer("wait").value_or(0) << '}';
    std::cout << '}';
}

static void run_pickup(int type, int oid, int hp, int action, bool declared, int weapon_action, bool holder_declared, int index) {
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    for (int slot : {0,50}) {
        const bool holder = slot == 0;
        ntsd28::SpawnRequest28 request;
        request.object_id = holder ? 77 : oid; request.object_type = holder ? 0 : type;
        request.definition = pickup_definition(holder, action, declared, weapon_action, holder_declared);
        request.hp = holder ? 500 : hp; request.mp = 500; request.battle_group = holder ? 1 : 2;
        request.position.x = holder ? 100 : 110; request.position.y = -10; request.position.z = 200;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("pickup spawn failed");
        auto* entity = world.entity(slot);
        entity->position.precise_x = request.position.x + .25;
        entity->frame.action_latch = holder ? 23 : 24;
        entity->frame.frame_counter = holder ? 7 : 9;
        entity->motion.x = holder ? 2.5 : -2.0; entity->motion.y = -.5; entity->motion.z = .75;
    }
    world.entity(0)->interaction_relation_count_35c = 5;
    world.entity(0)->input.current.set(ntsd28::InputKey28::attack);
    world.snapshot_actions();
    if (world.rebuild_geometric_hit_candidates().candidates_appended != 1) throw std::runtime_error("pickup candidate missing");
    world.entity(50)->frame.action = action;
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    std::cout << "{\"index\":" << index << ",\"type\":" << type << ",\"oid\":" << oid << ",\"hp\":" << hp
        << ",\"action\":" << action << ",\"declared\":" << (declared ? "true" : "false") << ",\"weaponAction\":" << weapon_action
        << ",\"holderDeclared\":" << (holder_declared ? "true" : "false") << ",\"before\":[";
    write_pickup_entity(*world.entity(0)); std::cout << ','; write_pickup_entity(*world.entity(50));
    const auto result = world.resolve_special_relation_hit(0, 0, 0);
    std::cout << "],\"applied\":" << (result.status == ntsd28::WorldRelationHitStatus28::applied ? "true" : "false") << ",\"after\":[";
    write_pickup_entity(*world.entity(0)); std::cout << ','; write_pickup_entity(*world.entity(50));
    (void)world.apply_frame_motion(0, ntsd28::DepthIntent28::none);
    (void)world.step_physics(0, {});
    std::cout << "],\"holderAfterPhysics\":"; write_pickup_entity(*world.entity(0));
    std::cout << ",\"nativeCalls\":" << direct_synchronized_calls.size() << ",\"crtCalls\":" << direct_crt_calls.size() << "}\n";
}

int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        for (int variant = 0; variant < 10; ++variant) {
            int type = variant < 7 ? variant : variant < 9 ? 1 : 6;
            int oid = variant == 7 ? 120 : variant == 8 ? 124 : 78;
            int hp = variant == 9 ? 0 : 500;
            for (int action : {0,99,857,998,999,1000}) for (bool declared : {false,true})
            for (int weapon_action : {0,998,999,1000,-998}) for (bool holder_declared : {false,true})
                run_pickup(type, oid, hp, action, declared, weapon_action, holder_declared, index++);
        }
        if (index != 1200) throw std::runtime_error("pickup case count changed");
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
