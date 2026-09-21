#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

// Source-only matrices; the target helper describes DAT construction, never changes source physics.
struct ContactCase {
    const char* group = "boundary";
    int action = 185, state = 12, floor = 0, control = -1;
    bool declared = false;
    double y = -1, vx = 0, vy = 1, vz = 2;
    int gain = 0, picked = 300, picking = 301, hit_facing = 0;
    int dx = 2, dy = 2, dz = 2;
};
static int contact_target(const ContactCase& c) {
    double vx = c.vx;
    if (static_cast<int>(c.y) >= c.floor) {
        if (vx > .0001) { vx -= 1; if (vx < .0001) vx = 0; }
        else if (vx < -.0001) { vx += 1; if (vx > -.0001) vx = 0; }
    }
    const bool hard = c.state == 18 || c.vy > 11 || vx > 9 || vx < -9;
    if (!hard) return c.action >= 186 ? 231 : 230;
    if (c.gain == 1) return c.action >= 186 && c.state != 18 ? c.picked : c.picking;
    return c.action < 186 || c.state == 18 ? 185 : 191;
}
static void write_contact_entity(const ntsd28::EntityState28* e) {
    if (!e) { std::cout << "null"; return; }
    const auto* f = e->definition->frame(e->frame.action);
    const auto* snapshot = e->definition->frame(e->frame.tick_action_snapshot);
    std::cout << "{\"raw\":"; write_entity(std::cout,*e,1);
    std::cout << ",\"available\":" << (f ? "true" : "false")
        << ",\"state\":" << (f ? f->values.integer("state").value_or(0) : 0)
        << ",\"wait\":" << (f ? f->values.integer("wait").value_or(0) : 0)
        << ",\"next\":" << (f ? f->values.integer("next").value_or(0) : 0)
        << ",\"snapshotAvailable\":" << (snapshot ? "true" : "false")
        << ",\"snapshotState\":" << (snapshot ? snapshot->values.integer("state").value_or(0) : 0)
        << ",\"previousX\":" << e->position.previous_x << ",\"previousY\":" << e->position.previous_y << ",\"previousZ\":" << e->position.previous_z
        << ",\"pending\":{\"dx\":" << e->status_dx_1c0 << ",\"dy\":" << e->status_dy_1c4
        << ",\"dz\":" << e->status_dz_1c8 << ",\"gain\":" << e->status_gain_1cc
        << ",\"hitFacing\":" << e->status_hit_facing_1d0 << ",\"picked\":" << e->status_picked_action_1d4
        << ",\"picking\":" << e->status_picking_action_1d8 << "}"
        << ",\"input\":"; write_b2_input_entity(std::cout,*e,1); std::cout << '}';
}
static void write_contact_state(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":["; write_contact_entity(world.entity(0));
    std::cout << "],\"random\":";write_b2_initial_random(std::cout,world.random().state(),world.random().synchronized_table_hash());std::cout << '}';
}
static void write_contact_calls() {
    std::cout << "{\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"synchronized\":";write_b2_synchronized_calls(std::cout);std::cout << '}';
}
static void write_contact_result(const ntsd28::WorldPhysicsResult28& result) {
    const auto& p = result.physics;
    std::cout << "{\"success\":" << (result.success ? "true" : "false") << ",\"slot\":" << result.slot
        << ",\"message\":\"" << json_escape(result.message) << "\",\"environmentDamageApplied\":" << (result.environment_damage_applied ? "true" : "false")
        << ",\"environmentDamage\":" << result.environment_damage << ",\"physics\":{";
#define BOOL_FIELD(name, field) std::cout << "\"" name "\":" << (p.field ? "true" : "false") << ','
    BOOL_FIELD("stepped", stepped); BOOL_FIELD("motionHoldAdvanced", motion_hold_advanced);
    BOOL_FIELD("xIntegrated", x_integrated); BOOL_FIELD("zIntegrated", z_integrated);
    BOOL_FIELD("xBlocked", x_blocked); BOOL_FIELD("zBlocked", z_blocked);
    BOOL_FIELD("gravityApplied", gravity_applied); BOOL_FIELD("airborneActionSelected", airborne_action_selected);
    BOOL_FIELD("landingActionSelected", landing_action_selected); BOOL_FIELD("landingFrameCounterReset", landing_frame_counter_reset);
    BOOL_FIELD("state1218LandingResolved", state12_18_landing_resolved); BOOL_FIELD("objectLandingResolved", object_landing_resolved);
    BOOL_FIELD("weaponHpUpdated", weapon_hp_updated); BOOL_FIELD("facingFlipped", facing_flipped);
    BOOL_FIELD("landingSoundChannel4", landing_sound_channel4); BOOL_FIELD("hitMotionConsumed", hit_motion_consumed);
    BOOL_FIELD("hitMotionDeferred", hit_motion_deferred); BOOL_FIELD("collisionFlagsConsumed", collision_flags_consumed);
#undef BOOL_FIELD
    std::cout << "\"weaponHpAfter\":" << p.weapon_hp_after << ",\"selectedAction\":" << p.selected_action
        << ",\"vertical\":" << static_cast<int>(p.vertical) << "},\"audio\":[";
    bool first = true;
    for (const auto& event : result.audio_events) {
        if (!first) std::cout << ','; first = false;
        std::cout << "{\"source\":" << static_cast<int>(event.source) << ",\"x\":" << event.world_x
            << ",\"channel\":" << event.native_channel << ",\"path\":\"" << json_escape(event.resource_path) << "\"}";
    }
    std::cout << "]}";
}
static void emit_contact(int index, const ContactCase& c) {
    const int target = contact_target(c);
    std::ostringstream text;
    text << "<bmp_begin>\nname: State1218ContactNative\n<bmp_end>\n"
        << "<frame> " << c.action << " initial\nstate: " << c.state << " wait: 100 next: " << c.action
        << " centerx: 39 centery: 79\n<frame_end>\n";
    if (c.declared && target != c.action && target >= 0 && target <= 999) text << "<frame> " << target << " destination\nstate: 3 wait: 37 next: 0 centerx: 13 centery: 23\n<frame_end>\n";
    const auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!definition->ok()) throw std::runtime_error("ordinary contact DAT rejected");
    ntsd28::BattleWorld28 world; world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 spawn; spawn.object_id = 77; spawn.object_type = 0; spawn.definition = definition;
    spawn.initial_action = c.action; spawn.hp = 500; spawn.mp = 497;
    spawn.position.x = 300; spawn.position.y = static_cast<int>(c.y); spawn.position.z = 250;
    if (!world.spawn_at(0,spawn).success) throw std::runtime_error("ordinary contact spawn failed");
    auto& e = *world.entity(0);
    e.frame.frame_counter = 7; e.frame.action_latch = 11; e.frame.previous_action_078 = c.action;
    e.position.precise_x = 300.25; e.position.precise_y = c.y; e.position.precise_z = 250.5;
    e.position.previous_x = 280; e.position.previous_y = -40; e.position.previous_z = 230;
    e.motion.x = c.vx; e.motion.y = c.vy; e.motion.z = c.vz; e.collision_y_reference = c.floor;
    e.status_dx_1c0 = c.dx; e.status_dy_1c4 = c.dy; e.status_dz_1c8 = c.dz;
    e.status_gain_1cc = c.gain; e.status_hit_facing_1d0 = c.hit_facing;
    e.status_picked_action_1d4 = c.picked; e.status_picking_action_1d8 = c.picking;
    e.environment_state_320 = 0;
    world.snapshot_actions();
    ntsd28::ObjectDefinitionCatalog28 catalog; catalog.upsert_definition(77,0,"ordinary-contact.dat",definition);
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"group\":\"" << c.group << "\",\"action\":" << c.action
        << ",\"state\":" << c.state << ",\"targetDeclared\":" << ((c.declared || target == c.action) ? "true" : "false")
        << ",\"targetDeclarationRequested\":" << (c.declared ? "true" : "false") << ",\"targetIsSource\":" << (target == c.action ? "true" : "false")
        << ",\"gain\":" << c.gain << ",\"picked\":" << c.picked << ",\"picking\":" << c.picking
        << ",\"dx\":" << c.dx << ",\"dy\":" << c.dy << ",\"dz\":" << c.dz << ",\"hitFacing\":" << c.hit_facing
        << ",\"target\":" << target << ",\"floor\":" << c.floor << ",\"y\":" << c.y << ",\"vx\":" << c.vx
        << ",\"vy\":" << c.vy << ",\"vz\":" << c.vz << ",\"control\":" << c.control << ",\"seed\":42},\"dat\":\"" << json_escape(text.str())
        << "\",\"config\":{\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"physicsContext\":\"default\",\"hpGate\":"
        << defaults.selected_mode_default_hp_regen_gate_28 << ",\"mpGate\":" << defaults.selected_mode_mp_regen_gate_2c
        << ",\"dropGate\":" << defaults.selected_mode_drop_gate_4c << "},\"before\":";
    write_contact_state(world); direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto step = world.step_physics(0,{});
    std::cout << ",\"step\":";write_contact_result(step);std::cout << ",\"after\":";write_contact_state(world);
    std::cout << ",\"calls\":";write_contact_calls();
    ntsd28::SimulationTickOptions28 options; options.stage_bounds = ntsd28::StageBounds28{800,180,350};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28 = defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c = defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c = defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto tick = ntsd28::SimulationTickDriver28{}.step(world,catalog,options);
    std::cout << ",\"following\":";write_contact_state(world);std::cout << ",\"followingCalls\":";write_contact_calls();
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false") << "}\n";
}
int wmain(int,wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10); int index = 0;
        const double motions[][2] = {{0,1},{9,11},{-9,11},{9.0001,1},{-9.0001,1},{0,11.0001}};
        for(int state : {12,18}) for(int action : {185,186}) for(int floor : {0,-10})
        for(bool declared : {false,true}) for(int gain : {0,1,2}) for(const auto& motion : motions) {
            ContactCase c;c.state=state;c.action=action;c.floor=floor;c.y=floor-motion[1];c.vx=motion[0];c.vy=motion[1];
            c.declared=declared;c.gain=gain;emit_contact(index++,c);
        }
        for(int state : {12,18}) for(int action : {170,186}) for(int floor : {0,-10})
        for(int target : {-1,-900,0,69,857,900,998,999,1000}) {
            ContactCase c;c.group="pending_binding";c.state=state;c.action=action;c.floor=floor;c.y=floor-12;c.vy=12;c.gain=1;
            // Only the selected lane gets the target; the other retains an independent sentinel.
            if(action>=186 && state!=18)c.picked=target;else c.picking=target;
            emit_contact(index++,c);
            if(target>=0 && target<=999){c.declared=true;emit_contact(index++,c);}
        }
        const int pending[][3] = {{0,0,0},{-4,-2,5},{560,501,555},{2,2,-3}};
        for(int family=0;family<3;++family) for(int facing : {0,1}) for(const auto& values : pending) {
            ContactCase c;c.group="pending_motion";c.state=family==2?18:12;c.action=family==0?170:186;
            c.floor=facing?-10:0;c.y=c.floor-12;c.vx=-10;c.vy=12;c.gain=1;c.picked=900;c.picking=900;
            c.dx=values[0];c.dy=values[1];c.dz=values[2];c.hit_facing=facing;c.declared=true;emit_contact(index++,c);
        }
        for(int state : {12,18}) for(int action : {185,186}) for(int floor : {0,-10})
        for(int control : {0,1}) for(int gain : {0,1,2}) {
            ContactCase c;c.group="contact_control";c.state=state;c.action=action;c.floor=floor;c.control=control;
            c.y=control==0?floor-20:floor;c.vy=control==0?1:0;c.vx=6;c.gain=gain;emit_contact(index++,c);
        }
        if(index!=480)throw std::runtime_error("state1218 contact matrix count changed");
        return 0;
    } catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 91;}
}
