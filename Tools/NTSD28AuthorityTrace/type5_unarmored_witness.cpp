#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

struct Type5ReactionCase {
    int type=5, attacker_type=0, attacker_state=0, attacker_facing=0, target_facing=0;
    int target_y=0, target_x=110, fall=0, dvx=5, dvy=0, cover=0;
    int hp=500, injury=7, bdefend=0, scale=100, weak=0;
    double target_vx=0, pending_x=0, pending_y=-2.5;
};

static std::shared_ptr<const ntsd28::DatDocument> type5_reaction_definition(bool attacker, const Type5ReactionCase& c) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: WeaponReaction\nweapon_hp: 20\n";
    if (attacker && c.attacker_type==3) text << "weapon_broken_sound: weapon_reaction_attacker.wav\n";
    if (!attacker) text << "weapon_hit_sound: weapon_reaction_target.wav\n";
    text << "<bmp_end>\n<frame> 0 active\nstate: " << (attacker?c.attacker_state:0)
        << " wait: 100 next: 0 cover: " << c.cover << " centerx: 3 centery: 5\n";
    if (attacker) text << "itr:\nkind: 0 x: -20 y: -20 w: 60 h: 60 injury: " << c.injury
        << " fall: " << c.fall << " dvx: " << c.dvx << " dvy: " << c.dvy
        << " vrest: 1 bdefend: " << c.bdefend << "\nitr_end:\n";
    else text << "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
    text << "<frame_end>\n<frame> 10 response\nstate: 0 wait: 100 next: 0 dvx: 7\n<frame_end>\n";
    auto data=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!data->ok()) throw std::runtime_error("weapon reaction DAT rejected");
    return data;
}

static void write_type5_reaction_state(const ntsd28::BattleWorld28& world) {
    std::cout << "{\"raw\":[";
    write_entity(std::cout,*world.entity(0),1);std::cout << ',';write_entity(std::cout,*world.entity(1),1);
    std::cout << "],\"extra\":[";
    for(int slot=0;slot<2;++slot) {
        if(slot)std::cout<<',';
        const auto& e=*world.entity(slot);const auto& p=e.pending_hit_impulse;
        std::cout << "{\"count\":" << p.contribution_count << ",\"x\":" << p.total.x
            << ",\"y\":" << p.total.y << ",\"z\":" << p.total.z << ",\"hpConsumed\":" << e.input_hp_consumed_total
            << ",\"mpConsumed\":" << e.input_mp_consumed_total << ",\"score\":" << e.input_score_total_348
            << ",\"link\":" << e.interaction_state << ",\"parent\":" << e.linked_parent_slot
            << ",\"child\":" << e.linked_child_slot << '}';
    }
    std::cout << "],\"rest\":[";
    for(int t=0;t<2;++t) {
        if(t)std::cout<<',';
        std::cout << '[' << static_cast<int>(world.victim_rest(t,0)) << ',' << static_cast<int>(world.victim_rest(t,1)) << ']';
    }
    std::cout << "],\"sparks\":[";
    for(int slot=0;slot<2;++slot) {
        if(slot)std::cout<<',';
        const auto& e=*world.entity(slot);std::cout<<'[';
        for(std::size_t i=0;i<e.spark_event_count;++i) {
            if(i)std::cout<<',';
            const auto& spark=e.spark_events[i];
            std::cout << "{\"host\":" << spark.host_slot << ",\"id\":" << spark.native_spark_id
                << ",\"x\":" << spark.world_x << ",\"y\":" << spark.world_y << '}';
        }
        std::cout<<']';
    }
    std::cout << "]}";
}

static void emit_type5_reaction(const char* group, const Type5ReactionCase& c, int index) {
    ntsd28::BattleWorld28 world;world.random().reset_from_seed(42);
    for(int slot=0;slot<2;++slot) {
        ntsd28::SpawnRequest28 request;request.object_id=77+slot;request.object_type=slot==0?c.attacker_type:c.type;
        request.definition=type5_reaction_definition(slot==0,c);request.hp=slot==0?500:c.hp;request.mp=500;
        request.battle_group=slot+1;request.position.x=100+slot*10;request.position.z=200;
        if(!world.spawn_at(slot,request).success)throw std::runtime_error("weapon reaction spawn failed");
    }
    world.snapshot_actions();world.rebuild_geometric_hit_candidates();
    if(world.entity(0)->hit_candidates.size()!=1)throw std::runtime_error("weapon reaction needs one candidate");
    auto& a=*world.entity(0);auto& t=*world.entity(1);
    a.frame.facing=c.attacker_facing!=0;t.frame.facing=c.target_facing!=0;
    a.frame.frame_counter=5;t.frame.frame_counter=7;a.frame.action_latch=13;t.frame.action_latch=17;
    a.motion={4,6,5};t.motion={c.target_vx,2.5,-3};
    a.pending_hit_impulse.total={1,2,3};a.pending_hit_impulse.contribution_count=1;
    t.pending_hit_impulse.total={c.pending_x,c.pending_y,1.25};t.pending_hit_impulse.contribution_count=2;
    t.position.x=c.target_x;t.position.precise_x=c.target_x;t.position.y=c.target_y;t.position.precise_y=c.target_y;
    t.bdefend_accumulator=17;t.hit_reaction_timer=29;t.incoming_damage_scale_340=c.scale;a.weak_timer_12c=c.weak;
    std::cout << "{\"index\":" << index << ",\"group\":\"" << group << "\",\"type\":" << c.type
        << ",\"attackerType\":" << c.attacker_type << ",\"attackerState\":" << c.attacker_state
        << ",\"attackerFacing\":" << c.attacker_facing << ",\"targetFacing\":" << c.target_facing
        << ",\"targetY\":" << c.target_y << ",\"targetX\":" << c.target_x << ",\"fall\":" << c.fall
        << ",\"dvx\":" << c.dvx << ",\"dvy\":" << c.dvy << ",\"cover\":" << c.cover
        << ",\"hp\":" << c.hp << ",\"injury\":" << c.injury << ",\"bdefend\":" << c.bdefend
        << ",\"scale\":" << c.scale << ",\"weak\":" << c.weak << ",\"vx\":" << c.target_vx
        << ",\"px\":" << c.pending_x << ",\"py\":" << c.pending_y << ",\"before\":";
    write_type5_reaction_state(world);direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto result=world.resolve_ordinary_unarmored_standard_hit(0,0);
    std::cout << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message)
        << "\",\"effectiveFall\":" << (result.interaction?result.interaction->fall:0)
        << ",\"effectiveDvx\":" << (result.interaction?result.interaction->dvx:0) << ",\"after\":";
    write_type5_reaction_state(world);
    std::cout << ",\"audio\":[";
    for(std::size_t i=0;i<result.audio_events.size();++i) {
        if(i)std::cout<<',';const auto& e=result.audio_events[i];
        std::cout << "{\"source\":" << static_cast<int>(e.source) << ",\"x\":" << e.world_x
            << ",\"channel\":" << e.native_channel << ",\"path\":\"" << json_escape(e.resource_path) << "\"}";
    }
    std::cout << "],\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"native\":";write_b2_synchronized_calls(std::cout);
    a.motion_hold_timer=0;t.motion_hold_timer=0;world.finalize_horizontal_hit_impulses();
    std::cout << ",\"afterReleasedHoldFinalize\":";write_type5_reaction_state(world);std::cout << "}\n";
}

int wmain(int,wchar_t**) {
    try {
        std::cout<<std::setprecision(std::numeric_limits<double>::max_digits10);int n=0;
        for(int type:{5})for(int face:{0,1})for(int state:{0,1002,2000})for(int y:{-10,0,3})for(int fall:{0,40,41})for(int dvy:{0,8}) {
            Type5ReactionCase c;c.type=type;c.target_facing=face;c.attacker_state=state;c.target_y=y;c.fall=fall;c.dvy=dvy;emit_type5_reaction("base",c,n++);
        }
        for(int type:{5})for(int face:{0,1})for(int state:{0,1002,2000})for(int vx:{-8,0,8})for(int px:{-3,0,3})for(int dvx:{-5,0,5})for(int x:{90,110}) {
            Type5ReactionCase c;c.type=type;c.attacker_facing=face;c.attacker_state=state;c.target_vx=vx;c.pending_x=px;c.dvx=dvx;c.target_x=x;c.fall=60;emit_type5_reaction("horizontal",c,n++);
        }
        for(int type:{5})for(int at:{0,3,4})for(int state:{1002,3000,3007})for(int cover:{0,2}) {
            Type5ReactionCase c;c.type=type;c.attacker_type=at;c.attacker_state=state;c.cover=cover;emit_type5_reaction("post",c,n++);
        }
        for(int type:{5})for(int hp:{1,500})for(int injury:{-5,0,101})for(int bdefend:{0,100})for(int scale:{50,100})for(int weak:{0,1}) {
            Type5ReactionCase c;c.type=type;c.hp=hp;c.injury=injury;c.bdefend=bdefend;c.scale=scale;c.weak=weak;emit_type5_reaction("resources",c,n++);
        }
        for(int type:{5})for(int dvx:{-5,0,5})for(int y:{-10,0,3}) {
            Type5ReactionCase c;c.type=type;c.attacker_facing=1;c.target_facing=1;c.dvx=dvx;c.target_y=y;emit_type5_reaction("both_left",c,n++);
        }
        for(int type:{5})for(int y:{-10,0,20})for(int dvy:{0,8,-8})for(double py:{-2.5,10.0}) {
            Type5ReactionCase c;c.type=type;c.target_y=y;c.dvy=dvy;c.pending_y=py;emit_type5_reaction("vertical",c,n++);
        }
        for(int y:{-10,0,3})for(int face:{0,1})for(int fall:{-50,-29,-28,-9,-8,0,11,12,31,32}) {
            Type5ReactionCase c;c.type=5;c.target_y=y;c.target_facing=face;c.fall=fall;c.injury=0;emit_type5_reaction("tiers",c,n++);
        }
        if(n!=585)throw std::runtime_error("type5 reaction count changed");return 0;
    }catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 91;}
}
