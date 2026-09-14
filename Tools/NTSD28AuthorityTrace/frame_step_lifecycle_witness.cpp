#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

// Original World functions own every output. This runner supplies inputs only.
struct FrameCase {
    int type = 0, slot = 0, action = 0, declared = 1, wait = 0, next = 7;
    int latch = 0, counter = 0, state = 0, y = 0, yref = 0;
    int hp = 100, mp = 100, dest_hp = 0, dest_mp = 0, fallback = 0;
    int recmp = 0, mode = 0, double_cost = 0, waived = 0, local = 1;
    int link = 0, hold = 0, lives = 1, queued_hp = 0, cpoint = 0;
    int drain = 0, hit_d = 0, facing = 0;
};

static void emit(const char* group, const FrameCase& c) {
    std::ostringstream dat;
    dat << "<bmp_begin>\nname: FrameWitness\njump_height: -9.5 jump_distance: 4.25 jump_distancez: 2.5\n<bmp_end>\n"
        << "<stats> recmp: " << c.recmp << " <stats_end>\n";
    if (c.declared) {
        dat << "<frame> " << c.action << " current\nstate: " << c.state
            << " wait: " << c.wait << " next: " << c.next << " hit_a: " << c.drain
            << " hit_d: " << c.hit_d << " sound: current.wav\n";
        if (c.cpoint) dat << "cpoint:\nkind: 2\ncpoint_end:\n";
        dat << "<frame_end>\n";
    }
    // The source accessor supplies all other implicit zero records.
    if (c.action != 7 || !c.declared) {
        dat << "<frame> 7 destination\nwait: 0 next: " << c.fallback
            << " hp: " << c.dest_hp << " mp: " << c.dest_mp
            << " sound: destination.wav\n<frame_end>\n";
    }
    auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat.str()));
    if (!definition->ok()) throw std::runtime_error("frame witness DAT rejected");
    ntsd28::BattleWorld28 world;
    ntsd28::SpawnRequest28 request;
    request.object_id = 77; request.object_type = c.type; request.definition = definition;
    request.initial_action = 0; request.hp = 500; request.mp = c.mp;
    if (!world.spawn_at(c.slot, request).success) throw std::runtime_error("frame witness spawn failed");
    auto* e = world.entity(c.slot);
    e->frame.action = c.action; e->frame.action_latch = c.latch;
    e->frame.frame_counter = c.counter; e->frame.facing = c.facing != 0;
    e->frame.tick_action_snapshot = 44; e->frame.previous_action_078 = 45;
    e->current_hp = c.hp; e->current_mp = c.mp; e->effective_max_hp = 200;
    e->input_mode_cost_multiplier_30 = c.mode; e->input_double_cost = c.double_cost;
    e->input_cost_waived = c.waived; e->interaction_state = c.link; e->motion_hold_timer = c.hold;
    e->revive_lives_30c = c.lives; e->revive_next_hp_314 = c.queued_hp;
    e->position.y = c.y; e->position.precise_y = c.y; e->collision_y_reference = c.yref;
    e->motion.x = 1.25; e->motion.y = 2.25; e->motion.z = 3.25;
    e->input.current.set(ntsd28::InputKey28::right);
    e->input.current.set(ntsd28::InputKey28::depth_up);
    ntsd28::NativeHitResourceRules28 rules;
    rules.local_mode_enabled_49d034 = c.local != 0;
    world.set_native_hit_resource_rules(rules);
    ntsd28::FrameStepOptions28 options{ntsd28::Next999Policy::return_to_zero};
    auto events = world.step_frame_slot(c.slot, options);
    if (events.size() != 1) throw std::runtime_error("frame witness expected one frame event");
    const auto& r = events.front().frame;
    std::cout << group;
    for (int value : {c.type,c.slot,c.action,c.declared,c.wait,c.next,c.latch,c.counter,c.state,c.y,c.yref,
                      c.hp,c.mp,c.dest_hp,c.dest_mp,c.fallback,c.recmp,c.mode,c.double_cost,c.waived,c.local,
                      c.link,c.hold,c.lives,c.queued_hp,c.cpoint,c.drain,c.hit_d,c.facing}) std::cout << '\t' << value;
    std::cout << '\t' << static_cast<int>(r.status) << '\t' << r.raw_next << '\t' << r.entered_before_step
              << '\t' << r.facing_flipped << '\t' << e->frame.action << '\t' << e->frame.action_latch
              << '\t' << e->frame.frame_counter << '\t' << e->frame.facing << '\t' << e->current_hp
              << '\t' << e->current_mp << '\t' << e->effective_max_hp << '\t' << e->input_hp_consumed_total
              << '\t' << e->input_mp_consumed_total << '\t' << e->motion.x << '\t' << e->motion.y << '\t' << e->motion.z
              << '\t' << e->lifecycle_resolution_pending << '\t' << e->lifecycle_code
              << '\t' << e->sound_action_latch << '\t' << events.front().audio_events.size();
    const auto lifecycle = world.resolve_pending_lifecycle(c.slot);
    e = world.entity(c.slot);
    std::cout << '\t' << lifecycle.pending << '\t' << lifecycle.despawned << '\t' << lifecycle.encoded_resets
              << '\t' << lifecycle.rejected << '\t' << (e != nullptr)
              << '\t' << (e ? e->frame.action : -9999) << '\t' << (e ? e->frame.action_latch : -9999)
              << '\t' << (e ? e->frame.tick_action_snapshot : -9999) << '\t' << (e ? e->frame.previous_action_078 : -9999)
              << '\t' << (e ? e->runtime_state_code : -9999) << '\n';
}

int wmain(int, wchar_t**) {
    std::cout << "group\ttype\tslot\taction\tdeclared\twait\tnext\tlatch\tcounter\tstate\ty\tyref"
                 "\thp\tmp\tdestHp\tdestMp\tfallback\trecmp\tmode\tdoubleCost\twaived\tlocal"
                 "\tlink\thold\tlives\tqueuedHp\tcpoint\tdrain\thitD\tfacing"
                 "\tstepStatus\trawNext\tentered\tflipped\tactionOut\tlatchOut\tcounterOut\tfacingOut"
                 "\thpOut\tmpOut\tboundOut\thpConsumed\tmpConsumed\tvxOut\tvyOut\tvzOut"
                 "\tpendingOut\tcodeOut\tsoundLatchOut\taudioCount\tlifePending\tdespawned\tencodedResets"
                 "\tlifeRejected\tsurvives\tfinalAction\tfinalLatch\tfinalCollisionAction\tfinalPrevious078\tfinalStateCode\n";
    for (int action : {-1,0,8,856,857,998,999,1000,9999})
    for (int declared : {0,1}) {
        if (declared && (action < 0 || action > 999)) continue;
        FrameCase c; c.action=action; c.declared=declared; c.next=0; emit("lookup",c);
    }
    for (int next : {-1299,-1101,-1000,-999,-212,-7,0,7,212,857,998,999,1000,1099,1100,1101,1199,1200,1299,1300})
    for (int type : {0,3,4})
    for (int position : {0,1,2}) {
        FrameCase c; c.next=next; c.type=type; c.y=position==0?0:-10; c.yref=position==2?-10:0;
        emit("next",c);
    }
    for (int wait : {-1,0,1,3}) for (int counter : {0,1,4}) for (int latch : {0,9}) {
        FrameCase c; c.wait=wait; c.counter=counter; c.latch=latch; emit("counter",c);
    }
    for (int mpCost : {-9,0,9}) for (int hpCost : {-6,0,6})
    for (int mp : {0,5,6,8,9,10}) for (int hp : {0,5,6,8,9,10})
    for (int fallback : {7,857,999,-999,1101,1300}) {
        FrameCase c; c.dest_mp=mpCost; c.dest_hp=hpCost; c.mp=mp; c.hp=hp; c.fallback=fallback;
        emit("cost",c);
    }
    for (int recmp : {-1,0,25,100,150}) for (int mode : {0,50,100,200})
    for (int doubled : {0,1}) for (int waived : {0,1}) for (int local : {0,1}) {
        FrameCase c; c.dest_mp=-9; c.dest_hp=-6; c.mp=10; c.hp=10; c.fallback=999;
        c.recmp=recmp; c.mode=mode; c.double_cost=doubled; c.waived=waived; c.local=local;
        emit("modifiers",c);
    }
    for (int type : {0,3,4}) for (int link : {-1,0}) for (int hold : {-1,0,1}) for (int cpoint : {0,1}) {
        FrameCase c; c.type=type; c.link=link; c.hold=hold; c.cpoint=cpoint; c.next=1000;
        emit("gates",c);
    }
    for (int type : {0,3,4}) for (int slot : {0,19,20}) for (int hp : {0,1})
    for (int lives : {1,2}) for (int queued : {0,100}) {
        FrameCase c; c.type=type; c.slot=slot; c.hp=hp; c.lives=lives; c.queued_hp=queued; c.state=14;
        emit("terminal14",c);
    }
    for (int state : {0,3007}) for (int hp : {-1,0,1,6}) for (int drain : {0,5,7})
    for (int hitD : {0,7,998,999,1101}) for (int cpoint : {0,1}) {
        FrameCase c; c.type=3; c.state=state; c.hp=hp; c.drain=drain; c.hit_d=hitD; c.cpoint=cpoint;
        emit("type3",c);
    }
    for (int next : {0,212,-212,999,-999}) {
        FrameCase c; c.action=212; c.latch=212; c.next=next; c.y=-10; emit("already212",c);
    }
    return 0;
}
