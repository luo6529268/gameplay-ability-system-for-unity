#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

struct LandingCase {
    const char* group = "binding";
    int action = 10, state = 0, hit_g = 1, floor = 0, control = -1;
    bool declared = false;
    double y = -1, vx = 6, vy = 2, vz = 2;
};
static int landing_target(const LandingCase& c) {
    return c.state == 100 ? 94 : c.action == 212 || c.state == 6 ? 215 : c.hit_g != 0 ? c.hit_g : 219;
}
static void write_landing_entity(const ntsd28::EntityState28* e) {
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
        << ",\"input\":"; write_b2_input_entity(std::cout,*e,1); std::cout << '}';
}
static void write_landing_state(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":["; write_landing_entity(world.entity(0));
    std::cout << "],\"random\":";write_b2_initial_random(std::cout,world.random().state(),world.random().synchronized_table_hash());std::cout << '}';
}
static void write_landing_calls() {
    std::cout << "{\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"synchronized\":";write_b2_synchronized_calls(std::cout);std::cout << '}';
}
static void write_landing_result(const ntsd28::WorldPhysicsResult28& result) {
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
static void emit_landing(int index, const LandingCase& c) {
    const int target = landing_target(c);
    std::ostringstream text;
    text << "<bmp_begin>\nname: OrdinaryLandingNative\n<bmp_end>\n"
        << "<frame> " << c.action << " initial\nstate: " << c.state << " wait: 100 next: " << c.action
        << " centerx: 39 centery: 79 hit_g: " << c.hit_g << "\n<frame_end>\n";
    if (c.declared) text << "<frame> " << target << " destination\nstate: 3 wait: 37 next: 0 centerx: 13 centery: 23\n<frame_end>\n";
    const auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!definition->ok()) throw std::runtime_error("ordinary landing DAT rejected");
    ntsd28::BattleWorld28 world; world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 spawn; spawn.object_id = 77; spawn.object_type = 0; spawn.definition = definition;
    spawn.initial_action = c.action; spawn.hp = 500; spawn.mp = 497;
    spawn.position.x = 300; spawn.position.y = static_cast<int>(c.y); spawn.position.z = 250;
    if (!world.spawn_at(0,spawn).success) throw std::runtime_error("ordinary landing spawn failed");
    auto& e = *world.entity(0);
    e.frame.frame_counter = 7; e.frame.action_latch = 11; e.frame.previous_action_078 = c.action;
    e.position.precise_x = 300.25; e.position.precise_y = c.y; e.position.precise_z = 250.5;
    e.position.previous_x = 280; e.position.previous_y = -40; e.position.previous_z = 230;
    e.motion.x = c.vx; e.motion.y = c.vy; e.motion.z = c.vz; e.collision_y_reference = c.floor;
    world.snapshot_actions();
    ntsd28::ObjectDefinitionCatalog28 catalog; catalog.upsert_definition(77,0,"ordinary-landing.dat",definition);
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"group\":\"" << c.group << "\",\"action\":" << c.action
        << ",\"state\":" << c.state << ",\"hitG\":" << c.hit_g << ",\"targetDeclared\":" << (c.declared ? "true" : "false")
        << ",\"target\":" << target << ",\"floor\":" << c.floor << ",\"y\":" << c.y << ",\"vx\":" << c.vx
        << ",\"vy\":" << c.vy << ",\"vz\":" << c.vz << ",\"control\":" << c.control << ",\"seed\":42},\"dat\":\"" << json_escape(text.str())
        << "\",\"config\":{\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"physicsContext\":\"default\",\"hpGate\":"
        << defaults.selected_mode_default_hp_regen_gate_28 << ",\"mpGate\":" << defaults.selected_mode_mp_regen_gate_2c
        << ",\"dropGate\":" << defaults.selected_mode_drop_gate_4c << "},\"before\":";
    write_landing_state(world); direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto step = world.step_physics(0,{});
    std::cout << ",\"step\":";write_landing_result(step);std::cout << ",\"after\":";write_landing_state(world);
    std::cout << ",\"calls\":";write_landing_calls();
    ntsd28::SimulationTickOptions28 options; options.stage_bounds = ntsd28::StageBounds28{800,180,350};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28 = defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c = defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c = defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto tick = ntsd28::SimulationTickDriver28{}.step(world,catalog,options);
    std::cout << ",\"following\":";write_landing_state(world);std::cout << ",\"followingCalls\":";write_landing_calls();
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false") << "}\n";
}
int wmain(int,wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10); int index = 0;
        for(int hit_g : {0,1,422,857,900,998,999,-1,-422,1000})
        for(int floor : {0,-10}) for(double vx : {-6.0,0.0,6.0}) {
            LandingCase c; c.hit_g=hit_g;c.floor=floor;c.y=floor-1;c.vx=vx;emit_landing(index++,c);
            if(landing_target(c)>=0 && landing_target(c)<=999){c.declared=true;emit_landing(index++,c);}
        }
        for(int variant=0;variant<5;++variant) for(int hit_g : {0,422,-1})
        for(int floor : {0,-10}) for(bool declared : {false,true}) {
            LandingCase c;c.group="priority";c.action=variant==1||variant==3||variant==4?212:10;
            c.state=variant<2?100:variant<4?6:0;c.hit_g=hit_g;c.floor=floor;c.y=floor-1;c.declared=declared;emit_landing(index++,c);
        }
        for(int control=0;control<3;++control) for(int hit_g : {0,422})
        for(int floor : {0,-10}) for(bool declared : {false,true}) {
            LandingCase c;c.group="control";c.control=control;c.hit_g=hit_g;c.floor=floor;c.declared=declared;
            c.y=control==0?floor-5:control==1?floor:floor+2;c.vy=control==1?0:1;emit_landing(index++,c);
        }
        if(index!=186)throw std::runtime_error("ordinary landing matrix count changed");
        return 0;
    } catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 91;}
}
