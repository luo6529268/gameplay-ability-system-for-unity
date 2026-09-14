#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

struct MatchedCase {
    int attacker_state=3005, target_state=3005, current=30, latch=13, latch_uj=0, target_latch_uj=-1;
    int hold=5, relation=0, armor=-1, body=0, live_rest=0, recover=3;
    int previous_state=0, snapshot_state=0, target_y=0, reference=0, fall=0, child=0;
};

static std::string matched_dat(int slot, const MatchedCase& c) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: Type5Matched\nweapon_hp: 23\n"
         << (slot==0 ? "weapon_broken_sound: matched_attacker.wav\n" : "weapon_hit_sound: matched_target.wav\n")
         << "<bmp_end>\n";
    if(slot==1 && c.armor==0)
        text << "<armor>\ntype: 0 ratio: 15 decrease: 50 mp: 0 fall: -1 bdefend: -1 injury: -1 delay: -1 state: 4 spark: 199\n<armor_end>\n";
    for(int id : {0,13,20,30,71,90,91,900}) {
        int state=id==90 ? (slot==1?c.previous_state:0) : id==91 ? (slot==1?c.snapshot_state:0) : id==900 ? 451 : 0;
        if(id==c.current) state=slot==0?c.attacker_state:slot==1?c.target_state:0;
        int uj=id==c.latch ? (slot==1 && c.target_latch_uj>=0 ? c.target_latch_uj : c.latch_uj) : 0;
        text << "<frame> " << id << " fixture\nstate: " << state
             << " wait: 100 next: 0 hit_Uj: " << uj << " centerx: 3 centery: 5\n";
        if(slot==0) text << "itr:\nkind: 0 x: -20 y: -20 w: 60 h: 60 injury: 7 fall: " << c.fall
            << " dvx: 5 dvy: 0 dvz: 2 vrest: 4 effect: 0 recover: " << c.recover << " dx: 0 dy: 0 dz: 0\nitr_end:\n";
        if(slot==1) text << "bdy:\nkind: " << (id==c.current?c.body:0)
            << " x: -20 y: -20 w: 60 h: 60 respond: -1\nbdy_end:\n";
        text << "<frame_end>\n";
    }
    return text.str();
}

static void matched_state(const ntsd28::BattleWorld28& world) {
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

static void emit_matched(const char* group,const MatchedCase& c,int index) {
    ntsd28::BattleWorld28 world;world.random().reset_from_seed(42);
    std::array<std::string,3> dat;
    for(int s=0;s<3;++s) {
        dat[s]=matched_dat(s,c);
        auto definition=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat[s]));
        if(!definition->ok())throw std::runtime_error("matched DAT rejected");
        ntsd28::SpawnRequest28 r;r.object_id=77+s;r.object_type=s==1?5:s==0?3:0;
        r.definition=definition;r.hp=500+s*10;r.mp=400+s*10;r.battle_group=s+1;
        r.position.x=s==2?10000:100+s*10;r.position.z=200;
        if(!world.spawn_at(s,r).success)throw std::runtime_error("matched spawn failed");
    }
    world.entity(0)->frame.action=91;
    world.snapshot_actions();(void)world.rebuild_geometric_hit_candidates();
    if(world.entity(0)->hit_candidates.size()!=1)throw std::runtime_error("matched needs one frozen candidate");
    for(int s=0;s<3;++s) {
        auto& e=*world.entity(s);
        e.frame.action=c.current;e.frame.action_latch=c.latch;e.frame.previous_action_078=90;e.frame.tick_action_snapshot=91;
        e.frame.frame_counter=5+s;e.frame.facing=s==1;e.motion={4.0+s,6.0+s,5.0+s};
        e.pending_hit_impulse.total={1.25+s,-2.5-s,3.75+s};e.pending_hit_impulse.contribution_count=1+s;
        e.pending_hit_impulse.y_resolved=true;e.motion_hold_timer=s==0?c.hold:s==1?-7:11;
        e.hit_reaction_timer=29+s;e.bdefend_accumulator=17+s;e.special_hit_latch_0eb=true;
        e.input_hp_consumed_total=101+s;e.input_mp_consumed_total=201+s;e.input_score_total_348=301+s;
        e.status_dx_1c0=11+s;e.status_dy_1c4=21+s;e.status_dz_1c8=31+s;e.status_gain_1cc=41+s;
        e.status_hit_facing_1d0=1;e.status_picked_action_1d4=71;e.status_picking_action_1d8=72;
        e.linked_parent_slot=-1;e.linked_child_slot=-1;
    }
    auto& a=*world.entity(0);auto& t=*world.entity(1);auto& third=*world.entity(2);
    a.interaction_state=c.relation==1?1:c.relation>=2?-1:0;
    a.linked_parent_slot=(c.relation==2 || c.relation==4)?2:c.relation==3?9:-1;
    if(c.relation==2) {third.interaction_state=1;third.linked_child_slot=0;}
    t.position.y=c.target_y;t.position.precise_y=c.target_y;t.collision_y_reference=c.reference;
    t.victim_rest_by_attacker[0]=static_cast<std::uint8_t>(c.live_rest);
    if(c.child) {t.interaction_state=1;t.linked_child_slot=2;third.interaction_state=-1;third.linked_parent_slot=c.child==1?1:9;}
    std::cout << "{\"index\":" << index << ",\"group\":\"" << group << "\",\"params\":{\"seed\":42,\"candidateAttackerAction\":91"
        << ",\"attackerState\":" << c.attacker_state << ",\"targetState\":" << c.target_state
        << ",\"current\":" << c.current << ",\"latch\":" << c.latch << ",\"latchUj\":" << c.latch_uj
        << ",\"targetLatchUj\":" << (c.target_latch_uj>=0?c.target_latch_uj:c.latch_uj)
        << ",\"previous\":90,\"snapshot\":91,\"previousState\":" << c.previous_state << ",\"snapshotState\":" << c.snapshot_state
        << ",\"hold\":" << c.hold << ",\"relation\":" << c.relation << ",\"armor\":" << c.armor
        << ",\"body\":" << c.body << ",\"liveRest\":" << c.live_rest << ",\"recover\":" << c.recover << ",\"targetY\":" << c.target_y
        << ",\"collisionYReference\":" << c.reference << ",\"fall\":" << c.fall << ",\"childRelation\":" << c.child
        << ",\"injury\":7,\"dvx\":5,\"dvy\":0,\"dvz\":2,\"vrest\":4,\"dat\":[";
    for(int s=0;s<3;++s) {if(s)std::cout<<',';std::cout<<'"'<<json_escape(dat[s])<<'"';}
    std::cout << " ]},\"before\":";matched_state(world);
    direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto result=world.resolve_ordinary_unarmored_standard_hit(0,0);
    std::cout << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message)
        << "\",\"matched\":" << (result.matched_projectile_pair_reset?"true":"false")
        << ",\"releasedSlot\":" << result.released_motion_hold_slot << ",\"after\":";matched_state(world);
    std::cout << ",\"audio\":[";
    for(std::size_t i=0;i<result.audio_events.size();++i) {
        if(i)std::cout<<',';
        const auto& e=result.audio_events[i];
        std::cout << "{\"source\":" << static_cast<int>(e.source) << ",\"x\":" << e.world_x
            << ",\"channel\":" << e.native_channel << ",\"path\":\"" << json_escape(e.resource_path) << "\"}";
    }
    std::cout << "],\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"native\":";write_b2_synchronized_calls(std::cout);
    for(int s=0;s<3;++s)world.entity(s)->motion_hold_timer=0;
    (void)world.finalize_horizontal_hit_impulses();
    std::cout << ",\"afterReleasedHoldFinalize\":";matched_state(world);std::cout<<"}\n";
}

int wmain(int,wchar_t**) {
    try {
        std::cout<<std::setprecision(std::numeric_limits<double>::max_digits10);int n=0;
        for(int state:{3005,3006})for(int variant:{0,1,2})for(int hold:{-5,0,5})for(int relation:{0,1,2,3}) {
            MatchedCase c;c.attacker_state=c.target_state=state;c.hold=hold;c.relation=relation;
            c.latch=variant==1?900:13;c.latch_uj=variant==1?71:variant==2?900:0;
            emit_matched("matched",c,n++);
        }
        for(int state:{3005,3006}) {
            MatchedCase c;c.attacker_state=c.target_state=state;c.latch_uj=71;c.target_latch_uj=900;
            emit_matched("different_latch_owners",c,n++);
            c=MatchedCase{};c.attacker_state=c.target_state=state;c.relation=4;
            emit_matched("nonreciprocal_parent",c,n++);
        }
        for(int state:{3005,3006})for(int hold:{-5,0,5})for(int relation:{0,1,2,3}) {
            MatchedCase c;c.attacker_state=c.target_state=state;c.hold=hold;c.relation=relation;c.recover=0;
            emit_matched("matched_rest_hold",c,n++);
        }
        for(int state:{3005,3006}) {
            MatchedCase c;c.attacker_state=state;c.target_state=state==3005?3006:3005;emit_matched("cross",c,n++);
            c.target_state=state;c.current=900;emit_matched("current900",c,n++);
            c.current=30;c.attacker_state=c.target_state=0;c.previous_state=state;c.snapshot_state=state;
            emit_matched("snapshot_not_current",c,n++);
        }
        for(int state:{3005,3006})for(int armor:{-1,0})for(int rest:{0,5})for(int body:{0,1033}) {
            MatchedCase c;c.attacker_state=c.target_state=state;c.armor=armor;c.live_rest=rest;c.body=body;
            emit_matched("priority",c,n++);
        }
        for(int forced:{0,1,2})for(int y:{-5,5})for(int reference:{-10,10}) {
            MatchedCase c;c.attacker_state=c.target_state=0;c.target_y=y;c.reference=reference;
            c.previous_state=forced==1?13:0;c.snapshot_state=forced==2?12:0;
            emit_matched("normal_frame_reference",c,n++);
        }
        for(int child:{1,2})for(int fall:{0,60}) {
            MatchedCase c;c.attacker_state=c.target_state=0;c.child=child;c.fall=fall;
            emit_matched("normal_child",c,n++);
        }
        if(n!=138)throw std::runtime_error("matched case count changed");
        return 0;
    }catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 91;}
}
