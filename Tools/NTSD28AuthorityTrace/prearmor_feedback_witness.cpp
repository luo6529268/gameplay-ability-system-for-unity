#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

static std::shared_ptr<const ntsd28::DatDocument> prearmor_definition(int role, int armor, int gate, int body, bool defense) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: PrearmorFeedback\nweapon_hp: 20\n<bmp_end>\n";
    if (role == 1 && armor >= 0) text << "<armor>\ntype: " << armor
        << " ratio: 15 decrease: 50 mp: 0 fall: -1 bdefend: -1 injury: -1 delay: -1 state: 4 spark: 199\n<armor_end>\n";
    text << "<frame> 0 active\nstate: " << (role == 1 && defense ? 7 : role == 1 && armor == 1 ? 4 : 0)
        << " wait: 100 next: 0 centerx: 3 centery: 5\n";
    if (role == 0) text << "itr:\nkind: 0 x: -20 y: -20 w: 60 h: 60 injury: 1 fall: 0 vrest: 1 bdefend: 35 effect: "
        << (gate == 1 ? 12 : 0) << (gate == 1 ? " caughtact: -3" : "") << "\nitr_end:\n";
    if (role == 1) text << "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
    text << "<frame_end>\n<frame> 20 latched\nstate: 0 wait: 50 next: 0\n";
    if (role == 1) text << "bdy:\nkind: 50 x: 0 y: 0 w: 1 h: 1\nbdy_end:\n";
    text << "<frame_end>\n<frame> 30 current_body\nstate: 0 wait: 80 next: 0\n";
    if (role == 1) text << "bdy:\nkind: " << body << " x: -20 y: -20 w: 60 h: 60 respond: -1\nbdy_end:\n";
    text << "<frame_end>\n";
    auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!definition->ok()) throw std::runtime_error("prearmor DAT rejected");
    return definition;
}

static void write_prearmor_state(const ntsd28::BattleWorld28& world) {
    std::cout << "{\"raw\":[";
    for (int slot = 0; slot < 3; ++slot) {
        if (slot) std::cout << ',';
        write_entity(std::cout, *world.entity(slot), 1);
    }
    std::cout << "],\"links\":[";
    for (int slot = 0; slot < 3; ++slot) {
        if (slot) std::cout << ',';
        const auto* e = world.entity(slot);
        std::cout << "{\"state\":" << e->interaction_state << ",\"parent\":" << e->linked_parent_slot
            << ",\"child\":" << e->linked_child_slot << '}';
    }
    std::cout << "],\"rest\":[";
    for (int target = 0; target < 3; ++target) {
        if (target) std::cout << ',';
        std::cout << '[';
        for (int attacker = 0; attacker < 3; ++attacker) {
            if (attacker) std::cout << ',';
            std::cout << static_cast<int>(world.victim_rest(target, attacker));
        }
        std::cout << ']';
    }
    std::cout << "],\"sparks\":[";
    for (int slot = 0; slot < 3; ++slot) {
        if (slot) std::cout << ',';
        const auto* e = world.entity(slot);
        std::cout << '[';
        for (std::size_t i = 0; i < e->spark_event_count; ++i) {
            if (i) std::cout << ',';
            const auto& spark = e->spark_events[i];
            std::cout << "{\"host\":" << spark.host_slot << ",\"id\":" << spark.native_spark_id
                << ",\"x\":" << spark.world_x << ",\"y\":" << spark.world_y << '}';
        }
        std::cout << ']';
    }
    std::cout << "]}";
}

static void emit_prearmor(int type, int armor, int relation, int gate, int rest, int index,
    int body = 0, int bdefend = 17, bool defense = false) {
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    for (int slot = 0; slot < 3; ++slot) {
        ntsd28::SpawnRequest28 request;
        request.object_id = 77 + slot; request.object_type = slot == 1 ? type : 0;
        request.definition = prearmor_definition(slot, armor, gate, body, defense);
        request.hp = 500; request.mp = 500; request.battle_group = slot + 1;
        request.position.x = slot == 2 ? 10000 : 100 + slot * 10; request.position.z = 200;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("prearmor spawn failed");
    }
    world.snapshot_actions();
    world.rebuild_geometric_hit_candidates();
    if (world.entity(0)->hit_candidates.size() != 1) throw std::runtime_error("prearmor needs one frozen candidate");
    auto* target = world.entity(1);
    auto* child = world.entity(2);
    target->motion.y = 4.25; child->motion.y = 7.5;
    target->bdefend_accumulator = bdefend; target->runtime_armor_hp = 3;
    if (defense) target->frame.facing = true;
    if (relation != 0) {
        target->linked_child_slot = 2;
        child->linked_parent_slot = relation == 3 ? -1 : 1;
        target->interaction_state = relation == 2 ? 1 : relation == 4 ? 0 : 2;
        child->interaction_state = relation == 2 ? -1 : relation == 4 ? 0 : -2;
    }
    if (gate == 2) target->frame.action_latch = 20;
    if (body != 0) target->frame.action = 30;
    target->victim_rest_by_attacker[0] = static_cast<std::uint8_t>(rest);
    std::cout << "{\"index\":" << index << ",\"type\":" << type << ",\"armor\":" << armor
        << ",\"relation\":" << relation << ",\"gate\":" << gate << ",\"rest\":" << rest << ",\"body\":" << body
        << ",\"bdefend\":" << bdefend << ",\"defense\":" << (defense ? "true" : "false") << ",\"before\":";
    write_prearmor_state(world);
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = armor == 1 ? world.resolve_ordinary_type1_armor_standard_hit(0, 0, {})
        : world.resolve_ordinary_unarmored_standard_hit(0, 0);
    std::cout << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message)
        << "\",\"selectedArmorType\":" << (result.selected_armor ? result.selected_armor->type : -1) << ",\"after\":";
    write_prearmor_state(world);
    std::cout << ",\"crt\":"; write_b2_crt_calls(std::cout);
    std::cout << ",\"native\":"; write_b2_synchronized_calls(std::cout);
    std::cout << "}\n";
}

int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        for (int type : {1,2,3,4,5,6})
        for (int armor : {-1,0,1,2})
        for (int relation : {0,1,2,3,4})
        for (int gate : {0,1,2})
        for (int rest : {0,5})
            emit_prearmor(type, armor, relation, gate, rest, index++);
        for (int type : {1,2,3,4,5,6})
        for (int armor : {-1,0})
        for (int rest : {0,5})
        for (int body : {1033,1100500000})
            emit_prearmor(type, armor, 1, 0, rest, index++, body);
        for (int type : {1,2,3,4,5,6})
        for (int relation : {0,1,2,3,4})
        for (int gate : {0,1,2})
        for (int rest : {0,5})
            emit_prearmor(type, 1, relation, gate, rest, index++, 0, 0);
        for (int type : {1,2,3,4,5,6})
        for (int armor : {-1,0,1})
        for (int rest : {0,5})
            emit_prearmor(type, armor, 1, 0, rest, index++, 0, 0, true);
        if (index != 984) throw std::runtime_error("prearmor count changed");
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
