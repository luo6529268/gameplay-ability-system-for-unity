#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

struct SparkCase {
    int route=0, index=0, effect=0, spark=0, fall=0, armor_spark=0;
    int geometry=0, cover=0, facing=0, reverse=0, fill=0, other_fill=0, missing=0;
};

static std::shared_ptr<const ntsd28::DatDocument> spark_definition(bool attacker, const SparkCase& c) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: SparkTransaction\n<bmp_end>\n";
    if (!attacker && c.route != 0) text << "<armor>\ntype: " << (c.route==1?0:1)
        << " ratio: 15 decrease: 50 mp: 0 fall: -1 bdefend: -1 injury: -1 delay: -1 state: 4 spark: "
        << c.armor_spark << "\n<armor_end>\n";
    for (int action : {0,10}) {
        text << "<frame> " << action << " spark\nstate: " << (!attacker && c.route==2?4:0)
            << " wait: 100 next: 0 centerx: " << (action==0?3:31) << " centery: " << (action==0?5:41) << '\n';
        if (attacker) for (int i=0;i<3;++i) text << "itr:\nkind: 0 x: -8 y: -3 w: 17 h: 5 vrest: 1 injury: 1 fall: "
            << c.fall << " effect: " << c.effect << " spark: " << c.spark << " cover: " << c.cover << "\nitr_end:\n";
        else text << "bdy:\nkind: 0 x: -999 y: -999 w: 2000 h: 2000\nbdy_end:\n";
        text << "<frame_end>\n";
    }
    auto result=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!result->ok()) throw std::runtime_error("spark fixture DAT rejected");
    return result;
}

static void write_sparks(const ntsd28::BattleWorld28& world) {
    std::cout << '[';
    for (int slot=0;slot<2;++slot) {
        if (slot) std::cout << ',';
        const auto* e=world.entity(slot);
        std::cout << "{\"slot\":" << slot << ",\"events\":[";
        for (std::size_t i=0;i<e->spark_event_count;++i) {
            if (i) std::cout << ',';
            const auto& s=e->spark_events[i];
            std::cout << "{\"host\":" << s.host_slot << ",\"id\":" << s.native_spark_id
                << ",\"x\":" << s.world_x << ",\"y\":" << s.world_y << '}';
        }
        std::cout << "]}";
    }
    std::cout << ']';
}

static void emit_spark(const char* group, const SparkCase& c, int index) {
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    const int a=c.reverse?1:0,t=1-a;
    for (int role=0;role<2;++role) {
        ntsd28::SpawnRequest28 request;
        request.object_id=77+role; request.object_type=role==1 && c.route==1?5:0;
        request.definition=spark_definition(role==0,c); request.hp=500;request.mp=500;request.battle_group=role+1;
        request.position.x=100+role*10;request.position.z=200;
        if (!world.spawn_at(role==0?a:t,request).success) throw std::runtime_error("spark spawn failed");
    }
    world.snapshot_actions();
    const auto collection=world.rebuild_geometric_hit_candidates();
    if (world.entity(a)->hit_candidates.size()!=3) throw std::runtime_error("spark fixture needs three original candidates");
    auto* attacker=world.entity(a);auto* target=world.entity(t);
    attacker->frame.action=10;target->frame.action=10;
    attacker->frame.facing=c.facing!=0;
    attacker->position.x=-11;attacker->position.y=-5;attacker->position.z=c.geometry==1?21:c.geometry==2?19:20;
    target->position.x=c.geometry==3?-20:-4;target->position.y=-6;target->position.z=20;
    for (auto* e : {attacker,target}) {
        e->position.precise_x=e->position.x;e->position.precise_y=e->position.y;e->position.precise_z=e->position.z;
    }
    const int host=attacker->position.z>target->position.z?a:attacker->position.z<target->position.z?t:std::max(a,t);
    for (int slot=0;slot<2;++slot) {
        auto* e=world.entity(slot);
        const int count=slot==host?c.fill:c.other_fill;
        for (int i=0;i<count;++i) e->spark_events[i]={static_cast<std::size_t>(slot),70+i,-100-i,-200-i};
        e->spark_event_count=count;
    }
    if (c.missing==1) attacker->frame.tick_action_snapshot=1000;
    if (c.missing==2) target->frame.tick_action_snapshot=1000;
    std::cout << "{\"index\":" << index << ",\"group\":\"" << group << "\",\"route\":" << c.route
        << ",\"itrIndex\":" << c.index << ",\"effect\":" << c.effect << ",\"spark\":" << c.spark << ",\"fall\":" << c.fall
        << ",\"armorSpark\":" << c.armor_spark << ",\"geometry\":" << c.geometry << ",\"cover\":" << c.cover
        << ",\"facing\":" << c.facing << ",\"reverse\":" << c.reverse << ",\"fill\":" << c.fill
        << ",\"otherFill\":" << c.other_fill << ",\"missing\":" << c.missing << ",\"expectedHost\":" << host << ",\"before\":";
    write_sparks(world);
    direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto result=c.route==2?world.resolve_ordinary_type1_armor_standard_hit(a,c.index,{}):
        world.resolve_ordinary_unarmored_standard_hit(a,c.index);
    std::cout << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message)
        << "\",\"selectedArmorType\":" << (result.selected_armor?result.selected_armor->type:-1)
        << ",\"after\":";
    write_sparks(world);
    std::cout << ",\"crt\":";write_b2_crt_calls(std::cout);
    std::cout << ",\"nativeCalls\":" << direct_synchronized_calls.size() << "}\n";
}

int wmain(int,wchar_t**) {
    try {
        int n=0;
        for(int route:{0,1,2})for(int index:{0,2})for(int effect:{0,1})for(int spark:{-1,0,99,100,199,200})for(int fall:{0,60,61}) {
            SparkCase c;c.route=route;c.index=index;c.effect=effect;c.spark=spark;c.fall=fall;emit_spark("selection",c,n++);
        }
        for(int route:{1,2})for(int armor:{-1,0,99,100,199,200})for(int spark:{-1,0,105}) {
            SparkCase c;c.route=route;c.index=2;c.armor_spark=armor;c.spark=spark;emit_spark("armor",c,n++);
        }
        for(int route:{0,1,2})for(int geometry:{0,1,2,3})for(int cover:{-8,0,17})for(int facing:{0,1})for(int reverse:{0,1}) {
            SparkCase c;c.route=route;c.geometry=geometry;c.cover=cover;c.facing=facing;c.reverse=reverse;emit_spark("geometry",c,n++);
        }
        for(int route:{0,1,2})for(int geometry:{0,1,2})for(int fill:{9,10})for(int other:{0,10}) {
            SparkCase c;c.route=route;c.geometry=geometry;c.fill=fill;c.other_fill=other;emit_spark("capacity",c,n++);
        }
        for(int route:{0,1,2})for(int missing:{1,2}) {
            SparkCase c;c.route=route;c.missing=missing;emit_spark("missing",c,n++);
        }
        if(n!=438)throw std::runtime_error("spark case count changed");
        return 0;
    }catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 91;}
}
