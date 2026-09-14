#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

struct HpCase {
    int type=0, hp=100, bound=200, base=500, mp=100;
    int regen_hp=0, regen_dhp=0, stats=1, chp=0;
    int weak=0, max_double=0, hp_double=0, mode=0, phase=0, valid=1, pending=0;
};

void emit_hp_case(const HpCase& c) {
    std::string data="<bmp_begin>\nname: HpWitness\n<bmp_end>\n";
    if (c.stats) data += "<stats> max_mp: 500 regen_hp: " + std::to_string(c.regen_hp) +
        " regen_dhp: " + std::to_string(c.regen_dhp) + " <stats_end>\n";
    data += "<frame> 0 idle\npic: 0 state: 0 wait: 100 next: 0 chp: " +
        std::to_string(c.chp) + "\n<frame_end>\n";
    ntsd28::DatParser parser;
    ntsd28::SpawnRequest28 request;
    request.object_id=77; request.object_type=c.type; request.hp=500; request.mp=c.mp;
    request.definition=std::make_shared<const ntsd28::DatDocument>(parser.parse_text(data));
    ntsd28::BattleWorld28 world;
    if (!world.spawn_at(0,request).success) throw std::runtime_error("HP witness spawn failed");
    auto* e=world.entity(0);
    e->current_hp=c.hp; e->effective_max_hp=c.bound; e->base_max_hp=c.base; e->current_mp=c.mp;
    e->weak_timer_12c=c.weak; e->effective_max_regen_double_1a8=c.max_double;
    e->hp_regen_double_1ac=c.hp_double; e->render_phase_008=-1;
    e->lifecycle_resolution_pending=c.pending!=0;
    if (!c.valid) e->frame.action=9999;
    for (int i=0;i<(c.phase==0?12:c.phase);++i) world.begin_native_resource_tick();
    ntsd28::ResourceSystemRules28 rules;
    rules.selected_mode_default_hp_regen_gate_28=c.mode;
    (void)world.advance_native_resources_pre_display_slot(0,rules);
    std::cout<<c.type<<'\t'<<c.hp<<'\t'<<c.bound<<'\t'<<c.base<<'\t'<<c.mp<<'\t'
        <<c.regen_hp<<'\t'<<c.regen_dhp<<'\t'<<c.stats<<'\t'<<c.chp<<'\t'<<c.weak<<'\t'
        <<c.max_double<<'\t'<<c.hp_double<<'\t'<<c.mode<<'\t'<<c.phase<<'\t'<<c.valid<<'\t'
        <<c.pending<<'\t'<<e->current_hp<<'\t'<<e->effective_max_hp<<'\t'<<e->current_mp<<'\n';
}

int wmain(int,wchar_t**) {
    std::cout<<"type\thp\tbound\tbase\tmp\tregenHp\tregenDhp\tstats\tchp\tweak\tmaxDouble\thpDouble\tmode\tphase\tvalid\tpending\texpectedHp\texpectedBound\texpectedMp\n";
    for (int hp:{-2,-1,0,1,2,3,4,5,99})
        for (int dhp:{-1,0,1,2,5,6,99})
            for (int chp:{-7,-3,-2,-1,0,1,2,3,7})
                for (int mode:{-1,0,1,2}) {
                    HpCase c; c.regen_hp=hp;c.regen_dhp=dhp;c.chp=chp;c.mode=mode;emit_hp_case(c);
                }
    for (int hd:{0,1,2}) for (int md:{0,1,2}) for (int weak:{0,1,2})
        for (int hp:{-1,0,2}) for (int dhp:{0,3}) for (int chp:{-5,0,5}) for (int mode:{0,1}) {
            HpCase c;c.hp_double=hd;c.max_double=md;c.weak=weak;c.regen_hp=hp;
            c.regen_dhp=dhp;c.chp=chp;c.mode=mode;emit_hp_case(c);
        }
    for (int hp:{-1,0,99,100,199,200,499,500,501}) for (int bound:{0,100,200,500})
        for (int mp:{-1,0,499,500,501}) for (int weak:{0,1}) {
            HpCase c;c.hp=hp;c.bound=bound;c.mp=mp;c.weak=weak;emit_hp_case(c);
        }
    for (int base:{0,100,500}) for (int mp:{-1,0,99,100,101,499,500,501}) {
        HpCase c;c.base=base;c.mp=mp;c.weak=1;c.chp=7;c.regen_hp=2;emit_hp_case(c);
    }
    for (int stats:{0,1}) for (int mode:{-1,0,1,2}) for (int chp:{-5,0,5}) for (int hd:{0,1}) {
        HpCase c;c.stats=stats;c.mode=mode;c.chp=chp;c.hp_double=hd;emit_hp_case(c);
    }
    for (int phase=0;phase<12;++phase) for (int type:{0,3}) for (int valid:{0,1}) for (int pending:{0,1}) {
        HpCase c;c.phase=phase;c.type=type;c.valid=valid;c.pending=pending;c.chp=7;emit_hp_case(c);
    }
    return 0;
}
