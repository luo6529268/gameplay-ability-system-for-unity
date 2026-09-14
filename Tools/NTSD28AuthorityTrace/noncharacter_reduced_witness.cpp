#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

struct ReducedCase {
    int type=1, injury=7, dvx=5;
    int parent=-1, delay=-1;
    int owner=-1, gain=0, balance=-999;
    int current=0, current_state=4, ratio=15, itr_bdefend=35;
    bool target_facing=true;
    int armor_mp=0, armor_hp=0, decrease=50, current_mp=410, armor_runtime=3, bdefend=0;
    int attacker_state=0, target_y=0, reference=0, reaction=30, effect=0, side=1;
    double attacker_vx=4.0, target_vx=5.0;
    bool facing=false;
    const char* group="base";
    bool defense=false;
};

static std::string reduced_dat(int slot, const ReducedCase& c) {
    std::ostringstream s;
    s << "<bmp_begin>\nname: ReducedWitness\nweapon_hp: 23\n<bmp_end>\n";
    if (slot==1 && !c.defense)
        s << "<armor>\ntype: 1 ratio: " << c.ratio << " decrease: " << c.decrease << " mp: " << c.armor_mp << " hp: " << c.armor_hp << " fall: -1 bdefend: -1 injury: -1 delay: " << c.delay << " state: " << (c.current!=0?c.current_state:4) << " spark: 199\n<armor_end>\n";
    s << "<frame> 0 active\nstate: " << (slot==1 ? (c.defense?7:4) : slot==0?c.attacker_state:0) << " wait: 100 next: 0 centerx: 3 centery: 5\n";
    if(slot==0) s << "itr:\nkind: 0 x: -20 y: -20 w: 60 h: 60 injury: " << c.injury << " fall: 0 dvx: " << c.dvx << " dvy: 0 dvz: 2 vrest: 4 bdefend: " << c.itr_bdefend << " effect: " << c.effect << " gain: " << c.gain << "\nitr_end:\n";
    if(slot==1) s << "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
    s << "<frame_end>\n";
    if(slot==1 && c.current!=0)
        s << "<frame> " << c.current << " current\nstate: " << c.current_state << " wait: 100 next: 0 centerx: 3 centery: 5\nbdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n<frame_end>\n";
    return s.str();
}

static void reduced_state(const ntsd28::BattleWorld28& world) {
    std::cout << "{\"raw\":[";
    for(int s=0;s<3;++s) { if(s)std::cout<<',';write_entity(std::cout,*world.entity(s),1); }
    std::cout << "],\"extra\":[";
    for(int s=0;s<3;++s) {
        if(s)std::cout<<',';
        const auto& e=*world.entity(s);const auto& p=e.pending_hit_impulse;
        std::cout << "{\"count\":" << p.contribution_count << ",\"x\":" << p.total.x << ",\"y\":" << p.total.y
            << ",\"z\":" << p.total.z << ",\"yResolved\":" << (p.y_resolved?"true":"false")
            << ",\"hpConsumed\":" << e.input_hp_consumed_total << ",\"mpConsumed\":" << e.input_mp_consumed_total
            << ",\"score\":" << e.input_score_total_348 << ",\"link\":" << e.interaction_state
            << ",\"parent\":" << e.linked_parent_slot << ",\"child\":" << e.linked_child_slot
            << ",\"kind4SourceCount\":" << e.kind4_source_count_92 << ",\"weak\":" << e.weak_timer_12c
            << ",\"incomingScale\":" << e.incoming_damage_scale_340
            << ",\"statusDx\":" << e.status_dx_1c0 << ",\"statusDy\":" << e.status_dy_1c4
            << ",\"statusDz\":" << e.status_dz_1c8 << ",\"statusGain\":" << e.status_gain_1cc
            << ",\"statusFacing\":" << e.status_hit_facing_1d0
            << ",\"statusPicked\":" << e.status_picked_action_1d4 << ",\"statusPicking\":" << e.status_picking_action_1d8 << '}';
    }
    std::cout << "],\"rest\":[";
    for(int t=0;t<3;++t) {
        if(t)std::cout<<',';
        std::cout<<'[';
        for(int a=0;a<3;++a) { if(a)std::cout<<',';std::cout<<static_cast<int>(world.victim_rest(t,a)); }
        std::cout<<']';
    }
    std::cout << "],\"sparks\":[";
    for(int s=0;s<3;++s) {
        if(s)std::cout<<',';
        const auto& e=*world.entity(s);std::cout<<'[';
        for(std::size_t i=0;i<e.spark_event_count;++i) {
            if(i)std::cout<<',';
            const auto& spark=e.spark_events[i];
            std::cout << "{\"host\":" << spark.host_slot << ",\"id\":" << spark.native_spark_id
                << ",\"x\":" << spark.world_x << ",\"y\":" << spark.world_y << '}';
        }
        std::cout<<']';
    }
    std::cout << "],\"rng\":";
    write_b2_initial_random(std::cout,world.random().state(),world.random().synchronized_table_hash());
    std::cout << '}';
}

static void emit_reduced(const ReducedCase& c, int index) {
    ntsd28::BattleWorld28 world; world.random().reset_from_seed(42);
    std::array<std::string,3> dat;
    for(int slot=0;slot<3;++slot) {
        dat[slot]=reduced_dat(slot,c);
        auto definition=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat[slot]));
        if(!definition->ok()) throw std::runtime_error("reduced DAT rejected");
        ntsd28::SpawnRequest28 r;
        r.object_id=77+slot;r.object_type=slot==1?c.type:0;r.definition=definition;
        r.hp=500+slot*10;r.mp=400+slot*10;r.battle_group=slot+1;
        r.position.x=slot==2?10000:100+slot*10;r.position.z=200;
        if(!world.spawn_at(slot,r).success)throw std::runtime_error("reduced spawn failed");
    }
    world.snapshot_actions();(void)world.rebuild_geometric_hit_candidates();
    if(world.entity(0)->hit_candidates.size()!=1)throw std::runtime_error("reduced needs one frozen candidate");
    for(int slot=0;slot<3;++slot) {
        auto& e=*world.entity(slot);
        e.frame.frame_counter=5+slot;e.frame.facing=slot==1;
        e.motion={4.0+slot,6.0+slot,5.0+slot};
        e.pending_hit_impulse.total={1.25+slot,-2.5-slot,3.75+slot};
        e.pending_hit_impulse.contribution_count=1+slot;e.pending_hit_impulse.y_resolved=true;
        e.motion_hold_timer=slot==0?5:slot==1?-7:11;
        e.hit_reaction_timer=29+slot;e.bdefend_accumulator=0;e.runtime_armor_hp=3;
        e.input_hp_consumed_total=101+slot;e.input_mp_consumed_total=201+slot;e.input_score_total_348=301+slot;
        e.status_dx_1c0=11+slot;e.status_dy_1c4=21+slot;e.status_dz_1c8=31+slot;e.status_gain_1cc=41+slot;
        e.status_hit_facing_1d0=1;e.status_picked_action_1d4=71;e.status_picking_action_1d8=72;
    }
    auto& attacker=*world.entity(0);auto& target=*world.entity(1);
    attacker.frame.facing=c.facing;attacker.motion.x=c.attacker_vx;
    target.frame.action=c.current;target.frame.facing=c.target_facing;
    target.motion.x=c.target_vx;target.hit_reaction_timer=c.reaction;
    target.position.y=c.target_y;target.collision_y_reference=c.reference;
    target.position.x=100+c.side*10;
    if(c.parent>=0){attacker.interaction_state=-1;attacker.linked_parent_slot=c.parent;}
    attacker.owner_slot=c.owner;
    if(c.balance!=-999){attacker.current_mp=c.balance;world.entity(2)->current_mp=c.balance;}
    target.current_mp=c.current_mp;target.runtime_armor_hp=c.armor_runtime;target.bdefend_accumulator=c.bdefend;
    std::cout << "{\"index\":" << index << ",\"group\":\"" << c.group << "\",\"params\":{\"seed\":42,\"candidateAttackerAction\":0,\"type\":" << c.type
        << ",\"owner\":" << c.owner << ",\"gain\":" << c.gain << ",\"balance\":" << c.balance
        << ",\"current\":" << c.current << ",\"currentState\":" << c.current_state << ",\"ratio\":" << c.ratio << ",\"itrBdefend\":" << c.itr_bdefend << ",\"targetFacing\":" << (c.target_facing?"true":"false") << ",\"parent\":" << c.parent << ",\"delay\":" << c.delay << ",\"armorMp\":" << c.armor_mp << ",\"armorHp\":" << c.armor_hp << ",\"decrease\":" << c.decrease << ",\"currentMp\":" << c.current_mp << ",\"runtimeArmorHp\":" << c.armor_runtime << ",\"bdefend\":" << c.bdefend << ",\"injury\":" << c.injury << ",\"dvx\":" << c.dvx << ",\"attackerState\":" << c.attacker_state << ",\"targetY\":" << c.target_y << ",\"reference\":" << c.reference << ",\"reaction\":" << c.reaction << ",\"effect\":" << c.effect << ",\"side\":" << c.side << ",\"attackerVx\":" << c.attacker_vx << ",\"targetVx\":" << c.target_vx << ",\"facing\":" << (c.facing?"true":"false") << ",\"defense\":" << (c.defense?"true":"false") << ",\"dat\":[";
    for(int slot=0;slot<3;++slot){if(slot)std::cout<<',';std::cout<<'"'<<json_escape(dat[slot])<<'"';}
    std::cout << "]},\"before\":";reduced_state(world);
    direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto result=c.defense?world.resolve_ordinary_unarmored_standard_hit(0,0):world.resolve_ordinary_type1_armor_standard_hit(0,0,{});
    std::cout << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message)
        << "\",\"armorDecision\":" << static_cast<int>(result.armor_decision.kind) << ",\"activationAvailable\":" << (result.armor_activation.available?"true":"false") << ",\"activationMpCost\":" << result.armor_activation.mp_cost << ",\"armorBroken\":" << (result.armor_activation.armor_hp_broken?"true":"false") << ",\"selectedArmorType\":" << (result.selected_armor?result.selected_armor->type:-1) << ",\"after\":";reduced_state(world);
    std::cout << ",\"audio\":[";
    for(std::size_t i=0;i<result.audio_events.size();++i){if(i)std::cout<<',';const auto& e=result.audio_events[i];std::cout<<"{\"source\":"<<static_cast<int>(e.source)<<",\"x\":"<<e.world_x<<",\"channel\":"<<e.native_channel<<",\"path\":\""<<json_escape(e.resource_path)<<"\"}";}
    std::cout << "],\"crt\":";write_b2_crt_calls(std::cout);std::cout<<",\"native\":";write_b2_synchronized_calls(std::cout);
    for(int slot=0;slot<3;++slot)world.entity(slot)->motion_hold_timer=0;
    (void)world.finalize_horizontal_hit_impulses();std::cout<<",\"afterReleasedHoldFinalize\":";reduced_state(world);std::cout<<"}\n";
}

int wmain(int,wchar_t**) {
    try {
        std::cout<<std::setprecision(std::numeric_limits<double>::max_digits10);int n=0;
        for(int type:{1,2,3,4,5,6})for(bool defense:{false,true})for(int injury:{-7,7}) {
            ReducedCase c;c.type=type;c.defense=defense;c.injury=injury;emit_reduced(c,n++);
        }
        for(int type:{1,6})for(int state:{0,1002,2000})for(int y:{-5,0,5})
        for(int dvx:{-5,0,5})for(int reaction:{30,80})for(bool facing:{false,true}) {
            ReducedCase c;c.group="motion";c.type=type;c.attacker_state=state;c.target_y=y;
            c.dvx=dvx;c.reaction=reaction;c.facing=facing;c.target_vx=reaction==80?0.0:5.0;emit_reduced(c,n++);
        }
        for(int effect:{22,23})for(int side:{-1,1})for(int y:{-5,0,5})for(int dvx:{-5,5}) {
            ReducedCase c;c.group="effect_position";c.type=3;c.effect=effect;c.side=side;c.target_y=y;c.dvx=dvx;emit_reduced(c,n++);
        }
        for(int side:{-1,0,1})for(double vx:{-4.0,0.0,4.0}) {
            ReducedCase c;c.group="post2000";c.type=3;c.attacker_state=2000;c.side=side;c.attacker_vx=vx;emit_reduced(c,n++);
        }
        for(int type:{1,2,3,4,5,6})for(int mp:{-3,50,200})for(int decrease:{-4,50})for(int injury:{-7,0,7})for(int offset:{-1,0,1}) {
            ReducedCase c;c.group="mp_activation";c.type=type;c.armor_mp=mp;c.decrease=decrease;c.injury=injury;
            int reduced=decrease<=0?-decrease:injury*decrease/100;
            int cost=mp<=0?-mp:reduced*mp/100;if(cost==0)cost=1;
            c.current_mp=cost+offset;emit_reduced(c,n++);
        }
        for(int type:{1,2,3,4,5,6})for(int hp:{-1,1})for(int injury:{-7,7})for(int offset:{-1,0,1}) {
            ReducedCase c;c.group="hp_activation";c.type=type;c.armor_hp=hp;c.injury=injury;c.armor_runtime=injury+offset;emit_reduced(c,n++);
        }
        for(int type:{1,2,3,4,5,6}) {
            ReducedCase c;c.group="bypass_minus_one";c.type=type;c.armor_hp=1;c.armor_runtime=-1;c.bdefend=17;emit_reduced(c,n++);
        }
        for(int type:{1,2,3,4,5,6})for(bool defense:{false,true})for(int parent:{2,9})for(int delay:{-1,105,-205}) {
            ReducedCase c;c.group="negative_parent";c.type=type;c.defense=defense;c.parent=parent;c.delay=delay;emit_reduced(c,n++);
        }
        for(int type:{3,6})for(int state:{4,7,70,75})for(int current:{30,110})for(int y:{-5,5})for(int add:{0,1,2}) {
            ReducedCase c;c.group="current_action";c.type=type;c.current=current;c.current_state=state;
            c.ratio=40;c.bdefend=39;c.itr_bdefend=add;c.armor_runtime=0;c.target_facing=false;c.target_y=y;emit_reduced(c,n++);
        }
        for(int type:{1,2,3,4,5,6})for(int owner:{-1,2,9})for(int gain:{-7,7})for(int balance:{0,6,7,420}) {
            ReducedCase c;c.group="resource_owner";c.type=type;c.owner=owner;c.gain=gain;c.balance=balance;emit_reduced(c,n++);
        }
        if(n!=987)throw std::runtime_error("reduced987 count changed");
        return 0;
    } catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 91;}
}
