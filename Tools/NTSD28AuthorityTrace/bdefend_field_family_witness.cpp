#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

static std::shared_ptr<const ntsd28::DatDocument> bdefend_definition(bool attacker, int armor_type, int raw_bdefend) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: BdefendFamily\nweapon_hp: 20\n<bmp_end>\n";
    if (!attacker && armor_type >= 0) text << "<armor>\ntype: " << armor_type
        << " ratio: 15 decrease: 50 mp: 0 fall: -1 bdefend: -1 injury: -1 delay: -1 state: 4\n<armor_end>\n";
    text << "<frame> 0 active\nstate: " << (!attacker && armor_type == 1 ? 4 : 0)
        << " wait: 100 next: 0\n";
    if (attacker) text << "itr:\nkind: 0 x: -20 y: -20 w: 60 h: 60 injury: 1 fall: 0 vrest: 1 bdefend: "
        << raw_bdefend << "\nitr_end:\n";
    else text << "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
    text << "<frame_end>\n";
    auto result = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!result->ok()) throw std::runtime_error("bdefend DAT rejected");
    return result;
}

static void write_bdefend_pair(const ntsd28::BattleWorld28& world) {
    std::cout << '[';
    write_entity(std::cout, *world.entity(0), 1);
    std::cout << ',';
    write_entity(std::cout, *world.entity(1), 1);
    std::cout << ']';
}

static void emit_bdefend(int type, int armor_type, int initial, int raw_bdefend, int armor_hp, int index) {
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    for (int slot : {0,1}) {
        ntsd28::SpawnRequest28 request;
        request.object_id = 77 + slot; request.object_type = slot == 0 ? 0 : type;
        request.definition = bdefend_definition(slot == 0, armor_type, raw_bdefend);
        request.hp = 500; request.mp = 500; request.battle_group = slot + 1;
        request.position.x = 100 + slot * 10; request.position.z = 200;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("bdefend spawn failed");
    }
    auto* target = world.entity(1);
    target->bdefend_accumulator = initial;
    target->runtime_armor_hp = armor_hp;
    world.snapshot_actions();
    const auto candidates = world.rebuild_geometric_hit_candidates();
    std::cout << "{\"index\":" << index << ",\"type\":" << type << ",\"armorType\":" << armor_type
        << ",\"initial\":" << initial << ",\"rawBdefend\":" << raw_bdefend << ",\"armorHp\":" << armor_hp
        << ",\"candidates\":" << candidates.candidates_appended << ",\"before\":";
    write_bdefend_pair(world);
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = armor_type == 1
        ? world.resolve_ordinary_type1_armor_standard_hit(0, 0, {})
        : world.resolve_ordinary_unarmored_standard_hit(0, 0);
    target = world.entity(1);
    if (!target) throw std::runtime_error("bdefend target removed before timer witness");
    std::cout << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message)
        << "\",\"selectedArmorType\":" << (result.selected_armor ? result.selected_armor->type : -1)
        << ",\"after\":";
    write_bdefend_pair(world);
    const int after_hit = target->bdefend_accumulator;
    const int after_link = target->interaction_state;
    target->motion_hold_timer = 2;
    world.advance_reaction_timers_slot(1);
    const int after_held_timer = target->bdefend_accumulator;
    target->bdefend_accumulator = after_hit;
    target->motion_hold_timer = 0;
    world.advance_reaction_timers_slot(1);
    std::cout << ",\"afterHitBdefend\":" << after_hit << ",\"afterHitLink\":" << after_link
        << ",\"afterHeldTimer\":" << after_held_timer << ",\"afterFreeTimer\":" << target->bdefend_accumulator
        << ",\"nativeCalls\":" << direct_synchronized_calls.size() << ",\"crtCalls\":" << direct_crt_calls.size() << "}\n";
}

int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        for (int type : {0,1,2,3,4,5,6})
        for (int armor : {-1,0})
        for (int initial : {-7,0,45,90})
        for (int raw : {-5,0,35,100})
            emit_bdefend(type, armor, initial, raw, 0, index++);
        for (int initial : {-7,0,45,90})
        for (int raw : {-5,0,35,100})
        for (int armor_hp : {-1,1})
            emit_bdefend(0, 1, initial, raw, armor_hp, index++);
        if (index != 256) throw std::runtime_error("bdefend case count changed");
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
