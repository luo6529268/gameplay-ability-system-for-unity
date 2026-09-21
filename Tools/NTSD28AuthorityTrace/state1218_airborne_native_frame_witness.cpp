#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

struct LandingCase {
    const char* group = "bands";
    int action = 170, state = 12, floor = 0, phase = 0, environment = 0;
    bool declared = false;
    double requested_post_vy = 2, y = -100, vx = 2, vy = .3, vz = 1;
};
static int landing_target(const LandingCase& c) {
    const double post = c.vy + ntsd28::PhysicsIntegrator28::ordinary_gravity;
    if (c.state == 18) return c.action < 205 && post > 1 ? 205 : -1;
    int base = c.action < 185 ? 180 : c.action > 185 && c.action < 191 ? 186 : -1;
    if (base < 0) return -1;
    if (base == 180 && c.environment < 0)
        return post < 12 && (c.phase + 1) % 12 >= 6 ? 182 : 181;
    return base + (post < -8 ? 0 : post < 1 ? 1 : post < 8 ? 2 : 3);
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
        << ",\"environment\":" << e->environment_state_320 << ",\"environmentSource\":" << e->environment_source_slot_160
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
    text << "<bmp_begin>\nname: State1218AirborneNative\n<bmp_end>\n"
        << "<frame> " << c.action << " initial\nstate: " << c.state << " wait: 100 next: " << c.action
        << " centerx: 39 centery: 79\n<frame_end>\n";
    if (c.declared && target >= 0 && target != c.action) text << "<frame> " << target << " destination\nstate: 3 wait: 37 next: 0 centerx: 13 centery: 23\n<frame_end>\n";
    const auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!definition->ok()) throw std::runtime_error("state1218 airborne DAT rejected");
    ntsd28::BattleWorld28 world; world.random().reset_from_seed(42);
    // Advance only the native global resource phase API before spawning; no entity resource work runs.
    for (int i = 0; i < c.phase; ++i) world.begin_native_resource_tick();
    ntsd28::SpawnRequest28 spawn; spawn.object_id = 77; spawn.object_type = 0; spawn.definition = definition;
    spawn.initial_action = c.action; spawn.hp = 500; spawn.mp = 497;
    spawn.position.x = 300; spawn.position.y = static_cast<int>(c.y); spawn.position.z = 250;
    if (!world.spawn_at(0,spawn).success) throw std::runtime_error("state1218 airborne spawn failed");
    auto& e = *world.entity(0);
    e.frame.frame_counter = 7; e.frame.action_latch = 11; e.frame.previous_action_078 = c.action;
    e.position.precise_x = 300.25; e.position.precise_y = c.y; e.position.precise_z = 250.5;
    e.position.previous_x = 280; e.position.previous_y = -40; e.position.previous_z = 230;
    e.motion.x = c.vx; e.motion.y = c.vy; e.motion.z = c.vz; e.collision_y_reference = c.floor;
    e.environment_state_320 = c.environment;
    world.snapshot_actions();
    ntsd28::ObjectDefinitionCatalog28 catalog; catalog.upsert_definition(77,0,"state1218-airborne.dat",definition);
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"group\":\"" << c.group << "\",\"action\":" << c.action
        << ",\"state\":" << c.state << ",\"targetDeclarationRequested\":" << (c.declared ? "true" : "false")
        << ",\"targetIsSource\":" << (target == c.action ? "true" : "false")
        << ",\"environment\":" << c.environment << ",\"resourcePhase12\":" << c.phase
        << ",\"resourcePhase3\":" << c.phase % 3 << ",\"upcomingPhase12\":" << (c.phase + 1) % 12
        << ",\"requestedPostVy\":" << c.requested_post_vy << ",\"computedPostVy\":" << c.vy + ntsd28::PhysicsIntegrator28::ordinary_gravity
        << ",\"target\":" << target << ",\"floor\":" << c.floor << ",\"y\":" << c.y << ",\"vx\":" << c.vx
        << ",\"vy\":" << c.vy << ",\"vz\":" << c.vz << ",\"seed\":42},\"dat\":\"" << json_escape(text.str())
        << "\",\"config\":{\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"physicsContext\":\"default\",\"resourceTickSetupCalls\":" << c.phase << ",\"resourcePhase12\":" << c.phase << ",\"resourcePhase3\":" << c.phase % 3 << ",\"hpGate\":"
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
        auto emit = [&index](LandingCase c) {
            c.floor = index % 2 ? -10 : 0;
            c.y = c.floor - 100;
            c.vy = c.requested_post_vy - ntsd28::PhysicsIntegrator28::ordinary_gravity;
            emit_landing(index++,c);
        };
        // Nine threshold inputs per family; requested -8 rounds above -8 after adding 1.7.
        for(int action : {170,187}) for(double post : {-8.0001,-8.0,-7.9999,.9999,1.0,1.0001,7.9999,8.0,8.0001}) {
            LandingCase c;c.action=action;c.requested_post_vy=post;emit(c);
        }
        for(int action : {170,187}) for(double post : {-9.0,0.0,2.0,9.0}) {
            LandingCase c;c.group="declared";c.action=action;c.requested_post_vy=post;c.declared=true;emit(c);
        }
        for(int action : {185,186,190,191}) {
            LandingCase c;c.group="family";c.action=action;c.requested_post_vy=2;emit(c);
        }
        for(int phase : {4,5,11}) for(double post : {11.9999,12.0,12.0001}) {
            LandingCase c;c.group="environment";c.environment=-1;c.phase=phase;c.requested_post_vy=post;emit(c);
        }
        {
            LandingCase c;c.group="environment";c.environment=-1;c.phase=10;c.requested_post_vy=11.9999;emit(c);
            c.action=187;c.phase=5;c.requested_post_vy=2;emit(c);
            c.action=170;c.environment=0;emit(c);
        }
        for(double post : {.9999,1.0,1.0001}) {
            LandingCase c;c.group="state18";c.state=18;c.action=204;c.requested_post_vy=post;emit(c);
        }
        for(int action : {205,206}) {
            LandingCase c;c.group="state18";c.state=18;c.action=action;c.requested_post_vy=2;emit(c);
        }
        for(int family : {180,186}) for(int band=0;band<4;++band) {
            const double posts[] = {-9,0,2,9};
            LandingCase c;c.group="same_frame";c.action=family+band;c.requested_post_vy=posts[band];c.declared=true;emit(c);
        }
        if(index!=55)throw std::runtime_error("state1218 airborne matrix count changed");
        return 0;
    } catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 91;}
}
