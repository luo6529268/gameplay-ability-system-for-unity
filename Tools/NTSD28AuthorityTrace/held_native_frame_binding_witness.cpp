#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_held_entity(const ntsd28::EntityState28* e, bool refill = false) {
    if (!e) { std::cout << "null"; return; }
    const auto* frame = e->definition->frame(e->frame.action);
    const auto* snapshot = e->definition->frame(e->frame.tick_action_snapshot);
    std::cout << "{\"raw\":"; write_entity(std::cout, *e, 1);
    std::cout << ",\"link\":" << e->interaction_state << ",\"parent\":" << e->linked_parent_slot
        << ",\"child\":" << e->linked_child_slot << ",\"available\":" << (frame ? "true" : "false")
        << ",\"state\":" << (frame ? frame->values.integer("state").value_or(0) : 0)
        << ",\"wait\":" << (frame ? frame->values.integer("wait").value_or(0) : 0)
        << ",\"next\":" << (frame ? frame->values.integer("next").value_or(0) : 0)
        << ",\"snapshotAvailable\":" << (snapshot ? "true" : "false")
        << ",\"snapshotState\":" << (snapshot ? snapshot->values.integer("state").value_or(0) : 0)
        << ",\"input\":"; write_b2_input_entity(std::cout, *e, 1);
    if (refill) std::cout << ",\"ordinaryCreditGate2F4\":" << e->ordinary_credit_gate_2f4
        << ",\"hpConsumed\":" << e->input_hp_consumed_total << ",\"mpConsumed\":" << e->input_mp_consumed_total;
    std::cout << '}';
}
static void write_held_state(ntsd28::BattleWorld28& world, bool refill = false) {
    std::cout << "{\"entities\":["; write_held_entity(world.entity(0), refill); std::cout << ',';
    write_held_entity(world.entity(70), refill); std::cout << "],\"random\":";
    write_b2_initial_random(std::cout, world.random().state(), world.random().synchronized_table_hash());
    std::cout << '}';
}
static void write_held_result(const ntsd28::WorldHeldRefillPass28& r) {
    std::cout << "{\"success\":" << (r.success ? "true" : "false")
        << ",\"linked\":" << r.linked_children << ",\"updates\":" << r.wpoint_follow_updates
        << ",\"unsupported\":" << r.unsupported_links << ",\"terminal\":" << r.terminal_weapon_actions
        << ",\"velocityReleases\":" << r.wpoint_velocity_releases << ",\"kind3Releases\":" << r.wpoint_kind3_releases
        << ",\"charging\":" << r.charging_links << ",\"hpRefills\":" << r.hp_refill_updates
        << ",\"mpRefills\":" << r.mp_refill_updates << ",\"exhausted\":" << r.exhausted_links
        << ",\"diagnostics\":[";
    bool first = true;
    for (const auto& d : r.diagnostics) { if (!first) std::cout << ','; first = false; std::cout << '"' << json_escape(d) << '"'; }
    std::cout << "]}";
}
static void write_held_calls() {
    std::cout << "{\"crt\":"; write_b2_crt_calls(std::cout);
    std::cout << ",\"synchronized\":"; write_b2_synchronized_calls(std::cout); std::cout << '}';
}
static std::shared_ptr<const ntsd28::DatDocument> held_definition(const std::string& dat) {
    auto result = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat));
    if (!result->ok()) throw std::runtime_error("held DAT rejected");
    return result;
}
struct HeldReleaseCase {
    int kind = 1, dvx = 0, dvy = -7, dvz = 9, depth = 0, cover = 0;
    bool facing = false, declared = false;
};
struct HeldRefillCase {
    const char* group = "depletion";
    int child_oid = 122, holder_state = 17, kind = 1;
    int child_hp = 30, child_mp = 500, credit = -1;
    int parent_hp = 496, parent_bound = 498, parent_mp = 498;
    bool parent_zero = true, child_zero = true;
};
static void emit_held(int index, int type, int action, bool declared,
                      const HeldReleaseCase* release = nullptr, const HeldRefillCase* refill = nullptr) {
    const int child_oid = refill ? refill->child_oid : 78;
    std::ostringstream parent_dat, child_dat;
    parent_dat << "<bmp_begin>\nname: HeldNativeParent\n<bmp_end>\n"
        << "<frame> 10 holder\nstate: " << (refill ? refill->holder_state : 0) << " wait: 100 next: 10 centerx: 39 centery: 79\n"
        << "wpoint:\nkind: " << (release ? release->kind : refill ? refill->kind : 1)
        << " x: 50 y: 60 cover: " << (release ? release->cover : 0) << " weaponact: " << action;
    if (release) parent_dat << " dvx: " << release->dvx << " dvy: " << release->dvy << " dvz: " << release->dvz;
    parent_dat << "\nwpoint_end:\n<frame_end>\n";
    child_dat << "<bmp_begin>\nname: HeldNativeChild\nweapon_hp: 17\n<bmp_end>\n"
        << "<frame> 10 initial\nstate: 3 wait: 83 next: 10 centerx: 9 centery: 19\n<frame_end>\n";
    if (declared) child_dat << "<frame> " << action << " destination\nstate: 3 wait: 37 next: " << action
        << " centerx: 13 centery: 23\nwpoint:\nkind: 1 x: 4 y: 5 cover: 2\nwpoint_end:\n<frame_end>\n";
    if (release && release->declared) {
        for (int destination : {0,1,2,3,4,5,40})
            child_dat << "<frame> " << destination
                << " release\nstate: 0 wait: 17 next: 0 centerx: 17 centery: 27\n<frame_end>\n";
    }
    if (refill && refill->parent_zero)
        parent_dat << "<frame> 0 exhausted\nstate: 0 wait: 19 next: 0 centerx: 17 centery: 27\n<frame_end>\n";
    if (refill && refill->child_zero)
        child_dat << "<frame> 0 exhausted\nstate: 0 wait: 23 next: 0 centerx: 7 centery: 11\n<frame_end>\n";
    const auto parent_definition = held_definition(parent_dat.str());
    const auto child_definition = held_definition(child_dat.str());
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(77, 0, "held-parent.dat", parent_definition);
    catalog.upsert_definition(child_oid, type, "held-child.dat", child_definition);
    for (int slot : {0,70}) {
        ntsd28::SpawnRequest28 spawn;
        spawn.object_id = slot == 0 ? 77 : child_oid; spawn.object_type = slot == 0 ? 0 : type;
        spawn.definition = slot == 0 ? parent_definition : child_definition;
        spawn.initial_action = 10; spawn.hp = 500; spawn.mp = 500;
        if (!world.spawn_at(slot, spawn).success) throw std::runtime_error("held spawn rejected");
        auto& e = *world.entity(slot);
        e.frame.frame_counter = slot == 0 ? 5 : 7;
        e.frame.action_latch = slot == 0 ? 9 : 11;
        e.frame.previous_action_078 = 10;
        e.position.x = slot == 0 ? 300 : 450; e.position.y = slot == 0 ? 0 : -35; e.position.z = 250;
        e.position.precise_x = e.position.x; e.position.precise_y = e.position.y; e.position.precise_z = e.position.z;
        e.motion.x = slot == 0 ? 0 : 2; e.motion.y = slot == 0 ? 0 : -3; e.motion.z = slot == 0 ? 0 : 4;
        e.frame.facing = release ? (slot == 0 ? release->facing : !release->facing) : slot != 0;
        if (release && slot == 0) {
            e.input.current.set(ntsd28::InputKey28::depth_up, release->depth == 1 || release->depth == 3);
            e.input.current.set(ntsd28::InputKey28::depth_down, release->depth == 2 || release->depth == 3);
            e.input.previous = e.input.current;
            e.input.pending = e.input.current;
        }
    }
    // Establish reciprocity after both spawn mutations have finished clearing old slot links.
    world.entity(0)->interaction_state = 1;
    world.entity(0)->linked_child_slot = 70;
    world.entity(70)->interaction_state = -1;
    world.entity(70)->linked_parent_slot = 0;
    if (refill) {
        auto& parent = *world.entity(0);
        auto& child = *world.entity(70);
        parent.current_hp = refill->parent_hp; parent.effective_max_hp = refill->parent_bound;
        parent.base_max_hp = 500; parent.current_mp = refill->parent_mp;
        child.current_hp = refill->child_hp; child.current_mp = refill->child_mp;
        child.ordinary_credit_gate_2f4 = refill->credit;
        parent.input_hp_consumed_total = 13; parent.input_mp_consumed_total = 17;
        child.input_hp_consumed_total = 19; child.input_mp_consumed_total = 23;
    }
    world.snapshot_actions();
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"type\":" << type << ",\"action\":" << action
        << ",\"declared\":" << (declared ? "true" : "false") << ",\"holderSlot\":0,\"childSlot\":70,\"seed\":42";
    if (release) std::cout << ",\"releaseMode\":true,\"kind\":" << release->kind << ",\"dvx\":" << release->dvx
        << ",\"dvy\":" << release->dvy << ",\"dvz\":" << release->dvz << ",\"depth\":" << release->depth << ",\"cover\":" << release->cover
        << ",\"facing\":" << (release->facing ? "true" : "false")
        << ",\"releaseDeclared\":" << (release->declared ? "true" : "false");
    if (refill) std::cout << ",\"refillMode\":true,\"group\":\"" << refill->group
        << "\",\"childOid\":" << child_oid << ",\"holderState\":" << refill->holder_state << ",\"kind\":" << refill->kind
        << ",\"childHp\":" << refill->child_hp << ",\"childMp\":" << refill->child_mp << ",\"credit2F4\":" << refill->credit
        << ",\"parentHp\":" << refill->parent_hp << ",\"parentHpBound\":" << refill->parent_bound << ",\"parentMp\":" << refill->parent_mp
        << ",\"parentZeroDeclared\":" << (refill->parent_zero ? "true" : "false")
        << ",\"childZeroDeclared\":" << (refill->child_zero ? "true" : "false");
    std::cout << "}" << ",\"parentDat\":\"" << json_escape(parent_dat.str()) << "\",\"childDat\":\"" << json_escape(child_dat.str())
        << "\",\"config\":{\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"hpGate\":"
        << defaults.selected_mode_default_hp_regen_gate_28 << ",\"mpGate\":" << defaults.selected_mode_mp_regen_gate_2c
        << ",\"dropGate\":" << defaults.selected_mode_drop_gate_4c << ",\"controls\":\"default non-AI packets\"},\"before\":";
    write_held_state(world, refill != nullptr);
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto held = world.settle_held_refill_objects();
    std::cout << ",\"held\":"; write_held_result(held);
    std::cout << ",\"after\":"; write_held_state(world, refill != nullptr);
    std::cout << ",\"calls\":"; write_held_calls();
    ntsd28::SimulationTickOptions28 options;
    options.stage_bounds = ntsd28::StageBounds28{800,180,350};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28 = defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c = defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c = defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
    std::cout << ",\"following\":"; write_held_state(world, refill != nullptr);
    std::cout << ",\"followingCalls\":"; write_held_calls();
    std::cout << ",\"followingHeldBefore\":"; write_held_result(tick.held_refills_before_geometry);
    std::cout << ",\"followingHeldAfter\":"; write_held_result(tick.held_refills_after_hits);
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false") << ",\"audio\":[";
    bool first = true;
    for (const auto& event : tick.audio_events) {
        if (!first) std::cout << ','; first = false;
        std::cout << "{\"source\":" << static_cast<int>(event.source) << ",\"x\":" << event.world_x
            << ",\"channel\":" << event.native_channel << ",\"path\":\"" << json_escape(event.resource_path) << "\"}";
    }
    std::cout << "]}\n";
}
int wmain(int argc, wchar_t** argv) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        if (argc == 2 && std::wstring(argv[1]) == L"--refill") {
            for (int oid : {122,123}) for (int hp : {-1,0,1,2,5,6,30})
            for (int kind : {1,3}) for (int type : {6,1,0}) {
                HeldRefillCase c; c.child_oid = oid; c.child_hp = hp; c.kind = kind;
                emit_held(index++,type,20,true,nullptr,&c);
            }
            for (int oid : {122,123}) for (int hp : {5,6,30}) for (int cap : {0,1,2})
            for (bool pzero : {false,true}) for (bool czero : {false,true}) {
                HeldRefillCase c; c.group = "caps"; c.child_oid = oid; c.child_hp = hp;
                c.parent_hp = cap == 0 ? 496 : cap == 1 ? 500 : 100;
                c.parent_bound = cap == 0 ? 498 : cap == 1 ? 500 : 480;
                c.parent_mp = cap == 0 ? 498 : cap == 1 ? 500 : 497;
                c.parent_zero = pzero; c.child_zero = czero;
                emit_held(index++,6,20,true,nullptr,&c);
            }
            for (int credit : {-1,0}) for (int mp : {149,151,500}) for (int parent_mp : {497,499,500}) {
                HeldRefillCase c; c.group = "credit"; c.child_oid = 123; c.credit = credit;
                c.child_mp = mp; c.parent_mp = parent_mp;
                emit_held(index++,6,20,true,nullptr,&c);
            }
            for (int oid : {122,123}) for (int state : {0,3}) for (int kind : {1,3}) for (int type : {6,1,0}) {
                HeldRefillCase c; c.group = "non17"; c.child_oid = oid; c.holder_state = state; c.kind = kind;
                emit_held(index++,type,20,true,nullptr,&c);
            }
            for (int oid : {122,123}) for (int hp : {1,2}) for (int kind : {1,3})
            for (bool pzero : {false,true}) for (bool czero : {false,true}) {
                HeldRefillCase c; c.group = "zero_binding"; c.child_oid = oid; c.child_hp = hp; c.kind = kind;
                c.parent_zero = pzero; c.child_zero = czero;
                emit_held(index++,6,20,true,nullptr,&c);
            }
            for (int type : {6,1,0}) for (int hp : {1,30}) for (int kind : {1,3}) {
                HeldRefillCase c; c.group = "ordinary_oid"; c.child_oid = 78; c.child_hp = hp; c.kind = kind;
                emit_held(index++,type,20,true,nullptr,&c);
            }
            if (index != 242) throw std::runtime_error("held refill matrix count changed");
            return 0;
        }
        if (argc == 2 && std::wstring(argv[1]) == L"--release") {
            for (int type = 0; type <= 6; ++type)
            for (int variant = 0; variant < 5; ++variant)
            for (bool facing : {false,true}) for (int depth : {0,1,2,3})
            for (int cover : {0,1}) for (bool declared : {false,true}) {
                HeldReleaseCase release;
                release.kind = variant < 2 ? 1 : 3;
                release.dvx = variant == 0 || variant == 3 ? -6 : variant == 1 || variant == 4 ? 6 : 0;
                if (variant == 2) { release.dvy = 0; release.dvz = 0; }
                release.facing = facing; release.depth = depth; release.cover = cover; release.declared = declared;
                emit_held(index++,type,20,true,&release);
            }
            for (int type = 0; type <= 6; ++type) for (int kind : {1,3}) {
                HeldReleaseCase release; release.kind = kind; release.dvx = 6;
                emit_held(index++,type,777,false,&release);
            }
            for (int type : {1,2,4,6}) for (int depth : {1,2}) for (bool declared : {false,true}) {
                HeldReleaseCase release; release.dvx = 6; release.dvy = 0; release.dvz = 0;
                release.depth = depth; release.declared = declared;
                emit_held(index++,type,20,true,&release);
            }
            if (index != 1150) throw std::runtime_error("held release matrix count changed");
            return 0;
        }
        if (argc != 1) throw std::runtime_error("expected no arguments, --release or --refill");
        for (int type = 0; type <= 6; ++type)
        for (int action : {-1,0,20,777,856,857,900,998,999,1000,std::numeric_limits<int>::min(),std::numeric_limits<int>::max()}) {
            emit_held(index++,type,action,false);
            if (action >= 0 && action <= 999) emit_held(index++,type,action,true);
        }
        if (index != 140) throw std::runtime_error("held matrix count changed");
        return 0;
    } catch (const std::exception& e) { std::cerr << e.what() << '\n'; return 91; }
}
