#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

namespace {
std::shared_ptr<const ntsd28::DatDocument> definition(int action, int state, int previous_state,
                                                    int depleted, int max_mp) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: DisplayPostWitness\nframe_0mp: " << depleted << "\n<bmp_end>\n"
         << "<stats> max_mp: " << max_mp << " <stats_end>\n"
         << "<frame> " << action << " current\npic: 0 state: " << state
         << " wait: 100 next: " << action << "\n<frame_end>\n"
         << "<frame> 800 previous\npic: 0 state: " << previous_state
         << " wait: 100 next: 800\n<frame_end>\n"
         << "<frame> 900 depleted\npic: 0 state: 63 wait: 100 next: 900\n<frame_end>\n";
    return std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
}

void display_cases() {
    std::cout << "type\tpending\tvalid\tvalue\ttarget\tstep\tspawnHp\tspawnDisplayHp\tspawnDisplayMax\tscore1\tdamage1\thp1\tmax1\tscore2\tdamage2\thp2\tmax2\tactualHp\tactualMax\n";
    for (int type : {0, 1, 2, 3, 4, 5, 6})
    for (int pending : {0, 1}) for (int valid : {0, 1})
    for (int value : {-5, 0, 5, 9, 10, 11, 15})
    for (int step : {-3, 0, 1, 4, 20}) {
        ntsd28::BattleWorld28 world;
        ntsd28::SpawnRequest28 request;
        request.object_id = 77; request.object_type = type; request.hp = 137;
        request.definition = definition(0, 0, 0, 0, 500);
        if (!world.spawn_at(0, request).success) throw std::runtime_error("display spawn failed");
        auto* e = world.entity(0);
        std::cout << type << '\t' << pending << '\t' << valid << '\t' << value << "\t10\t" << step
                  << '\t' << e->current_hp << '\t' << e->display_current_hp_200
                  << '\t' << e->display_effective_max_hp_208;
        e->lifecycle_resolution_pending = pending != 0;
        if (!valid) e->frame.action = 9999;
        e->input_score_total_348 = e->input_hp_consumed_total = 10;
        e->current_hp = e->effective_max_hp = 10;
        e->display_score_1f0 = e->display_damage_total_1f8 = value;
        e->display_current_hp_200 = e->display_effective_max_hp_208 = value;
        e->display_score_step_1f4 = e->display_damage_step_1fc = step;
        e->display_current_hp_step_204 = e->display_effective_max_hp_step_20c = step;
        for (int tick = 0; tick < 2; ++tick) {
            world.advance_native_display_values_slot(0);
            std::cout << '\t' << e->display_score_1f0 << '\t' << e->display_damage_total_1f8
                      << '\t' << e->display_current_hp_200 << '\t' << e->display_effective_max_hp_208;
        }
        std::cout << '\t' << e->current_hp << '\t' << e->effective_max_hp << '\n';
    }
}

struct PostCase {
    int action=0, state=0, previous=0, depleted=0, hp=100, bound=200, base=500, mp=300;
    int max_mp=500, gate=-1, timer=0, mode=0, type=0, pending=0, valid=1, stage=0;
};

void emit_post(const PostCase& c) {
    ntsd28::BattleWorld28 world;
    ntsd28::SpawnRequest28 request;
    request.object_id=77; request.object_type=c.type; request.hp=c.base;
    request.definition=definition(c.action,c.state,c.previous,c.depleted,c.max_mp);
    if (!world.spawn_at(0,request).success) throw std::runtime_error("post spawn failed");
    auto* e=world.entity(0);
    e->frame.action=c.valid?c.action:9999; e->frame.previous_action_078=800;
    e->current_hp=c.hp; e->effective_max_hp=c.bound; e->base_max_hp=c.base; e->current_mp=c.mp;
    e->ordinary_credit_gate_2f4=c.gate; e->full_restore_timer_1b0=c.timer;
    e->lifecycle_resolution_pending=c.pending!=0;
    e->input_hp_consumed_total=37; e->knockout_count_358=9;
    e->revive_lives_30c=7; e->battle_group=8;
    e->position.x=11; e->position.y=-7; e->position.z=13;
    e->position.precise_x=11.25; e->position.precise_y=-7.5; e->position.precise_z=13.75;
    e->display_current_hp_200=0; e->display_effective_max_hp_208=0;
    ntsd28::ResourceSystemRules28 rules;
    rules.selected_mode_full_restore_gate_18=c.mode;
    if (c.stage==1) rules.stage_bounds=ntsd28::StageBounds28{101,-9,10};
    if (c.stage==2) rules.stage_bounds=ntsd28::StageBounds28{0,10,-9};
    world.advance_native_display_values_slot(0);
    auto pass=world.advance_native_resources_post_display_slot(0,rules);
    std::cout << c.action << '\t' << c.state << '\t' << c.previous << '\t' << c.depleted << '\t'
              << c.hp << '\t' << c.bound << '\t' << c.base << '\t' << c.mp << '\t' << c.max_mp << '\t'
              << c.gate << '\t' << c.timer << '\t' << c.mode << '\t' << c.type << '\t' << c.pending << '\t'
              << c.valid << '\t' << c.stage << '\t' << e->frame.action << '\t' << e->current_hp << '\t'
              << e->effective_max_hp << '\t' << e->current_mp << '\t' << e->input_hp_consumed_total << '\t'
              << e->knockout_count_358 << '\t' << e->revive_lives_30c << '\t' << e->battle_group << '\t'
              << e->position.x << '\t' << e->position.y << '\t' << e->position.z << '\t'
              << e->position.precise_x << '\t' << e->position.precise_y << '\t' << e->position.precise_z << '\t'
              << e->display_current_hp_200 << '\t' << e->display_effective_max_hp_208 << '\t'
              << pass.depleted_effective_max_action_updates << '\t' << pass.native_full_restore_updates << '\t'
              << pass.hp_limit_clamps << '\t' << pass.mp_limit_clamps << '\n';
}

void post_cases() {
    std::cout << "action\tstate\tprevious\tdepleted\thp\tbound\tbase\tmp\tmaxMp\tgate\ttimer\tmode\ttype\tpending\tvalid\tstage\tactionOut\thpOut\tboundOut\tmpOut\tconsumedOut\tkoOut\tlivesOut\tgroupOut\txOut\tyOut\tzOut\tpreciseX\tpreciseY\tpreciseZ\tdisplayHp\tdisplayMax\tdepletedUpdates\trestoreUpdates\thpClamps\tmpClamps\n";
    for (int prev : {0,61,62,63,64,65,66,67,405,3999,4000,4001,4999,5000,3639,3640,3645,3646})
    for (int state : {0,63}) for (int timer : {-1,0,1}) for (int mode : {-1,0,1,2,3,4})
    for (int gate : {-1,0}) {
        PostCase c;c.previous=prev;c.state=state;c.timer=timer;c.mode=mode;c.gate=gate;c.stage=1;emit_post(c);
    }
    for (int action : {0,111,112,114,115,129,130,144,145,179,180,192,193,199,200,206,207,219,220,231,232})
    for (int hp : {0,1}) for (int bound : {-1,0,1}) for (int timer : {0,1}) {
        PostCase c;c.action=action;c.hp=hp;c.bound=bound;c.depleted=900;c.timer=timer;emit_post(c);
    }
    for (int hp : {-1,0,499,500,501}) for (int bound : {-1,0,499,500,501})
    for (int mp : {-1,0,99,100,101,499,500,501}) for (int max : {-1,0,100,500}) {
        PostCase c;c.hp=hp;c.bound=bound;c.mp=mp;c.max_mp=max;emit_post(c);
    }
    for (int type : {0,1,2,3,4,5,6}) for (int pending : {0,1}) for (int valid : {0,1}) {
        PostCase c;c.type=type;c.pending=pending;c.valid=valid;c.previous=63;c.timer=1;emit_post(c);
    }
    for (int stage : {0,1,2}) {PostCase c;c.previous=405;c.stage=stage;emit_post(c);}
}
}

int wmain(int argc,wchar_t** argv) {
    if (argc!=2) return 2;
    const std::wstring mode=argv[1];
    if (mode==L"display") display_cases();
    else if (mode==L"post") post_cases();
    else return 2;
    return 0;
}
