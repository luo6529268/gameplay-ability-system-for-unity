#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

// Calls the unchanged playable/core resource implementation; no copied resolver.
struct MpCase {
    int oid = 77, type = 0, hp = 400, mp = 100, regen = 0, has_stats = 1;
    int bound = 0, cmp = 0, double_cost = 0, weak = 0, bonus = 0;
    int render = 0, credit = -1, mode = 1, f6 = 1, phase = 0;
    int valid_frame = 1, pending = 0;
};

void emit_mp_case(const MpCase& c) {
    ntsd28::DatParser parser;
    std::string text = "<bmp_begin>\nname: MpWitness\n<bmp_end>\n";
    if (c.has_stats)
        text += "<stats> max_mp: 500 regen_mp: " + std::to_string(c.regen) +
                " bound: " + std::to_string(c.bound) + " <stats_end>\n";
    text += "<frame> 0 idle\npic: 0 state: 0 wait: 100 next: 0 cmp: " +
            std::to_string(c.cmp) + "\n<frame_end>\n";
    ntsd28::SpawnRequest28 request;
    request.object_id = c.oid;
    request.object_type = c.type;
    request.definition = std::make_shared<const ntsd28::DatDocument>(parser.parse_text(text));
    request.hp = 500;
    request.mp = c.mp;
    ntsd28::BattleWorld28 world;
    if (!world.spawn_at(0, request).success) throw std::runtime_error("MP witness spawn failed");
    auto* e = world.entity(0);
    e->current_hp = c.hp;
    e->current_mp = c.mp;
    e->input_double_cost = c.double_cost;
    e->weak_timer_12c = c.weak;
    e->mp_regen_bonus_timer_1a4 = c.bonus;
    e->render_phase_008 = c.render;
    e->ordinary_credit_gate_2f4 = c.credit;
    e->lifecycle_resolution_pending = c.pending != 0;
    if (!c.valid_frame) e->frame.action = 9999;
    for (int i = 0; i < (c.phase == 0 ? 3 : c.phase); ++i) world.begin_native_resource_tick();
    ntsd28::ResourceSystemRules28 rules;
    rules.selected_mode_mp_regen_gate_2c = c.mode;
    rules.negative_mp_regen_enabled_49d034 = c.f6 != 0;
    (void)world.advance_native_resources_pre_display_slot(0, rules);
    std::cout << c.oid << '\t' << c.type << '\t' << c.hp << '\t' << c.mp << '\t'
              << c.regen << '\t' << c.has_stats << '\t' << c.bound << '\t' << c.cmp << '\t'
              << c.double_cost << '\t' << c.weak << '\t' << c.bonus << '\t' << c.render << '\t'
              << c.credit << '\t' << c.mode << '\t' << c.f6 << '\t' << c.phase << '\t'
              << c.valid_frame << '\t' << c.pending << '\t' << e->current_mp << '\n';
}

int wmain(int, wchar_t**) {
    std::cout << "oid\ttype\thp\tmp\tregen\thasStats\tbound\tcmp\tdoubleCost\tweak\tbonus\trender\tcredit\tmode\tf6\tphase\tvalidFrame\tpending\texpectedMp\n";
    for (int regen = -7; regen <= 15; ++regen)
        for (int mode : {-1, 0, 1, 2})
            for (int oid : {77, 51, 52}) {
                MpCase c; c.regen = regen; c.mode = mode; c.oid = oid; emit_mp_case(c);
            }
    for (int regen : {-7, -6, -3, -1, 0, 2, 14, 99})
        for (int hp : {-100, 0, 375, 499, 500, 650})
            for (int mp : {-5, 0, 149, 150, 151, 499, 500, 501})
                for (int credit : {-1, 0})
                    for (int mode : {0, 1}) {
                        MpCase c; c.regen=regen; c.hp=hp; c.mp=mp; c.credit=credit; c.mode=mode; emit_mp_case(c);
                    }
    for (int regen : {-7, -3, -1, 0, 2})
        for (int cmp : {-5, 5})
            for (int cost : {0, 1})
                for (int weak : {0, 2})
                    for (int bonus : {0, 2})
                        for (int f6 : {0, 1}) {
                            MpCase c; c.regen=regen; c.cmp=cmp; c.double_cost=cost;
                            c.weak=weak; c.bonus=bonus; c.f6=f6; c.mode=0; emit_mp_case(c);
                        }
    for (int bound : {0, 1, 2})
        for (int render : {-1, 0})
            for (int regen : {-3, 0})
                for (int mode : {0, 1}) {
                    MpCase c; c.bound=bound; c.render=render; c.regen=regen; c.mode=mode; emit_mp_case(c);
                }
    for (int phase : {0, 1, 2})
        for (int type : {0, 3})
            for (int valid : {0, 1})
                for (int pending : {0, 1}) {
                    MpCase c; c.phase=phase; c.type=type; c.valid_frame=valid; c.pending=pending;
                    c.mode=0; c.cmp=7; emit_mp_case(c);
                }
    for (int has_stats : {0, 1})
        for (int mode : {-1, 0, 1, 2}) {
            MpCase c; c.has_stats=has_stats; c.mode=mode; emit_mp_case(c);
        }
    return 0;
}
