#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_body_entity(const ntsd28::EntityState28* e) {
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
        << ",\"catchSource\":" << e->catch_source_slot_90 << ",\"impactSource\":" << e->impact_source_slot_164
        << ",\"pendingCount\":" << e->pending_hit_impulse.contribution_count << ",\"pendingX\":" << e->pending_hit_impulse.total.x << ",\"pendingY\":" << e->pending_hit_impulse.total.y
        << ",\"pendingZ\":" << e->pending_hit_impulse.total.z
        << ",\"input\":"; write_b2_input_entity(std::cout,*e,1); std::cout << '}';
}
static void write_body_state(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":["; bool first = true;
    for (int slot : {0,70}) { if (!first) std::cout << ','; first = false; write_body_entity(world.entity(slot)); }
    std::cout << "],\"random\":";write_b2_initial_random(std::cout,world.random().state(),world.random().synchronized_table_hash());std::cout << '}';
}
static void write_body_calls() {
    std::cout << "{\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"synchronized\":";write_b2_synchronized_calls(std::cout);std::cout << '}';
}


struct BodyCase { int kind, target_action, attacker_action; bool declared; };
static void emit_body(int index, const BodyCase& c) {
    std::ostringstream ad, td;
    ad << "<bmp_begin>\nname: FirstBodyAttacker\n<bmp_end>\n<frame> 20 initial\nstate: 0 wait: 100 next: 20\n"
       << "itr:\nkind: 0 effect: 0 x: -100 y: -100 w: 300 h: 300 zwidth: 100 injury: 5 fall: 1 bdefend: 0 vrest: 1\nitr_end:\n<frame_end>\n";
    td << "<bmp_begin>\nname: FirstBodyTarget\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10\n"
       << "bdy:\nkind: " << c.kind << " respond: -1 x: -100 y: -100 w: 300 h: 300 zwidth: 100\nbdy_end:\n<frame_end>\n";
    if(c.declared) {
        td << "<frame> " << c.target_action << " response\nstate: 0 wait: 41 next: " << c.target_action << "\n<frame_end>\n";
        if(c.attacker_action>=0) ad << "<frame> " << c.attacker_action << " response\nstate: 0 wait: 41 next: " << c.attacker_action << "\n<frame_end>\n";
    }
    auto a=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(ad.str()));
    auto t=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(td.str()));
    if(!a->ok() || !t->ok())throw std::runtime_error("first-body DAT parse failed");
    ntsd28::BattleWorld28 world;world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(31980,0,"firstbody-attacker.dat",a);catalog.upsert_definition(31981,0,"firstbody-target.dat",t);
    for(int slot : {0,70}) {
        ntsd28::SpawnRequest28 request;request.object_id=slot==0?31980:31981;request.object_type=0;
        request.definition=slot==0?a:t;request.initial_action=slot==0?20:10;request.hp=400;request.mp=497;
        request.battle_group=slot==0?1:2;
        request.position.x=slot==0?300:350;request.position.y=0;request.position.z=250;
        if(!world.spawn_at(slot,request).success)throw std::runtime_error("first-body spawn failed");
        auto& e=*world.entity(slot);
        e.position.precise_x=e.position.x+.25;e.position.precise_y=0;e.position.precise_z=250.5;
        e.frame.frame_counter=slot==0?7:8;e.frame.action_latch=slot==0?13:11;
        e.frame.previous_action_078=slot==0?14:12;
        e.motion.x=1.25;e.motion.y=0;e.motion.z=-.5;
        e.pending_hit_impulse.total.x=13;e.pending_hit_impulse.total.y=17;e.pending_hit_impulse.total.z=19;
    }
    world.entity(70)->pending_hit_impulse.contribution_count=1;
    world.snapshot_actions();
    const auto candidates=world.rebuild_geometric_hit_candidates();
    if(candidates.candidates_appended!=1)throw std::runtime_error("first-body expected one geometric candidate");
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"bodyKind\":" << c.kind << ",\"targetAction\":" << c.target_action
        << ",\"attackerAction\":" << c.attacker_action << ",\"declared\":" << (c.declared?"true":"false")
        << ",\"respond\":-1,\"chance\":0,\"effect\":0,\"initialPendingCount\":1,\"seed\":42}"
        << ",\"attackerDat\":\"" << json_escape(ad.str()) << "\",\"targetDat\":\"" << json_escape(td.str())
        << "\",\"config\":{\"slots\":[0,70],\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"hpGate\":" << defaults.selected_mode_default_hp_regen_gate_28
        << ",\"mpGate\":" << defaults.selected_mode_mp_regen_gate_2c << ",\"dropGate\":" << defaults.selected_mode_drop_gate_4c << "},\"before\":";
    write_body_state(world);direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto result=world.resolve_ordinary_unarmored_standard_hit(0,0);
    if(!result.first_body_action_response_applied && !result.encoded_body_response_applied)
        throw std::runtime_error("first-body response did not apply at case " + std::to_string(index) + ": " + result.message);
    if(world.entity(0)->current_hp!=400 || world.entity(70)->current_hp!=400)
        throw std::runtime_error("first-body unexpectedly applied HP damage");
    std::cout << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message)
        << "\",\"firstBodyApplied\":" << (result.first_body_action_response_applied?"true":"false")
        << ",\"firstBodyKind\":" << result.first_body_action_kind << ",\"firstBodyRespond\":" << result.first_body_action_respond
        << ",\"firstBodyTargetAction\":" << result.first_body_target_action << ",\"firstBodyGroup\":" << result.first_body_projected_group
        << ",\"firstBodyHold\":" << (result.first_body_action_hold_applied?"true":"false")
        << ",\"encodedRecognized\":" << (result.encoded_body_recognized?"true":"false")
        << ",\"encodedApplied\":" << (result.encoded_body_response_applied?"true":"false")
        << ",\"encodedTargetAction\":" << result.encoded_body_target_action << ",\"encodedAttackerAction\":" << result.encoded_body_attacker_action
        << ",\"encodedChance\":" << result.encoded_body_chance << ",\"encodedEffect\":" << result.encoded_body_effect
        << ",\"encodedRngConsumed\":" << (result.encoded_body_rng_consumed?"true":"false")
        << ",\"encodedChancePassed\":" << (result.encoded_body_chance_passed?"true":"false")
        << ",\"encodedManualDamage\":" << result.encoded_body_manual_damage << ",\"after\":";
    write_body_state(world);std::cout << ",\"calls\":";write_body_calls();
    ntsd28::SimulationTickOptions28 options;options.stage_bounds=ntsd28::StageBounds28{800,180,350};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28=defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c=defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c=defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto tick=ntsd28::SimulationTickDriver28{}.step(world,catalog,options);
    std::cout << ",\"following\":";write_body_state(world);std::cout << ",\"followingCalls\":";write_body_calls();
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success?"true":"false") << "}\n";
}
int wmain() {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        emit_body(0,{1069,69,-1,false});
        emit_body(1,{1900,900,-1,true});
        emit_body(2,{2900,900,-1,false});
        emit_body(3,{1999,-1,-1,false});
        emit_body(4,{1000000000+900*10000+857*10,900,857,false});
        emit_body(5,{1000000000+900*10000+857*10,900,857,true});
        emit_body(6,{1000000000+999*10000+69*10,999,69,false});
        emit_body(7,{1033,33,-1,true});
        return 0;
    } catch(const std::exception& e) { std::cerr<<e.what()<<'\n';return 91; }
}
