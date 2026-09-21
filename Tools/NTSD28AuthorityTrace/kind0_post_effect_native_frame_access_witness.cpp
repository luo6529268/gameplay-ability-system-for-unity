#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_post_effect_entity(const ntsd28::EntityState28* e) {
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
        << ",\"pendingX\":" << e->pending_hit_impulse.total.x << ",\"pendingY\":" << e->pending_hit_impulse.total.y
        << ",\"pendingZ\":" << e->pending_hit_impulse.total.z
        << ",\"input\":"; write_b2_input_entity(std::cout,*e,1); std::cout << '}';
}
static void write_post_effect_state(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":["; bool first = true;
    for (int slot : {0,70}) { if (!first) std::cout << ','; first = false; write_post_effect_entity(world.entity(slot)); }
    std::cout << "],\"random\":";write_b2_initial_random(std::cout,world.random().state(),world.random().synchronized_table_hash());std::cout << '}';
}
static void write_post_effect_calls() {
    std::cout << "{\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"synchronized\":";write_b2_synchronized_calls(std::cout);std::cout << '}';
}


struct PostEffectCase {
    const char* group;
    int effect, previous, previous_state;
    bool previous_declared, target_declared;
};
static void emit_post_effect(int index, const PostEffectCase& c, bool reaction_mode = false, int snapshot_state = 0) {
    std::ostringstream ad, td;
    ad << "<bmp_begin>\nname: PostEffectAttacker\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10\n"
       << "itr:\nkind: 0 effect: " << c.effect << " x: -100 y: -100 w: 300 h: 300 zwidth: 100 injury: 5 fall: 1 bdefend: 0 vrest: 1"
       << " catchingact: 0 caughtact: 0 pickedact: 0\nitr_end:\n<frame_end>\n";
    td << "<bmp_begin>\nname: PostEffectTarget\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10\n"
       << "bdy:\nkind: 0 x: -100 y: -100 w: 300 h: 300 zwidth: 100\nbdy_end:\n<frame_end>\n";
    // Isolate ordinary/death reaction descriptors from the override destinations.
    for(int reaction : {180,186,220})
        td << "<frame> " << reaction << " reaction_support\nstate: 0 wait: 100 next: " << reaction << "\n<frame_end>\n";
    if(c.previous_declared)
        td << "<frame> " << c.previous << " previous_support\nstate: " << c.previous_state << " wait: 43 next: " << c.previous << "\n<frame_end>\n";
    if(c.target_declared)
        td << "<frame> 203 destination\nstate: 0 wait: 41 next: 203\n<frame_end>\n";
    if(reaction_mode)
        td << "<frame> 900 snapshot_support\nstate: " << snapshot_state << " wait: 43 next: 900\n"
           << "bdy:\nkind: 0 x: -100 y: -100 w: 300 h: 300 zwidth: 100\nbdy_end:\n<frame_end>\n";
    auto a=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(ad.str()));
    auto t=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(td.str()));
    if(!a->ok() || !t->ok())throw std::runtime_error("effect DAT parse failed");
    ntsd28::BattleWorld28 world;world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(77,0,"post-effect-attacker.dat",a);catalog.upsert_definition(78,0,"post-effect-target.dat",t);
    for(int slot : {0,70}) {
        ntsd28::SpawnRequest28 request;request.object_id=slot==0?77:78;request.object_type=0;
        request.definition=slot==0?a:t;request.initial_action=10;request.hp=400;request.mp=497;
        request.battle_group=slot==0?1:2;
        request.position.x=slot==0?300:350;request.position.y=0;request.position.z=250;
        if(!world.spawn_at(slot,request).success)throw std::runtime_error("effect spawn failed");
        auto& e=*world.entity(slot);
        e.position.precise_x=e.position.x+.25;e.position.precise_y=0;e.position.precise_z=250.5;
        e.frame.frame_counter=slot==0?7:8;e.frame.action_latch=slot==0?13:11;
        e.frame.previous_action_078=slot==0?14:c.previous;
        e.motion.x=1.25;e.motion.y=0;e.motion.z=-.5;
        e.pending_hit_impulse.total.x=slot==70?0:13;e.pending_hit_impulse.total.y=17;e.pending_hit_impulse.total.z=19;
    }
    world.snapshot_actions();
    if(reaction_mode) world.entity(70)->frame.tick_action_snapshot=900;
    const auto candidates=world.rebuild_geometric_hit_candidates();
    const bool early_rejection = !reaction_mode && index == 1;
    if(candidates.candidates_appended != (early_rejection ? 0 : 1) ||
       candidates.direct_effect_rejections != (early_rejection ? 1 : 0))
        throw std::runtime_error("post-effect candidate counts differ at case " + std::to_string(index));
    const bool hit_attempted = candidates.candidates_appended == 1;
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"group\":\"" << c.group
        << "\",\"effect\":" << c.effect << ",\"previous\":" << c.previous << ",\"previousState\":" << c.previous_state
        << ",\"previousDeclared\":" << (c.previous_declared?"true":"false") << ",\"targetDeclared\":" << (c.target_declared?"true":"false")
        << ",\"hp\":400,\"injury\":5,\"fall\":1,\"kind\":0,\"catching\":0,\"caught\":0,\"picked\":0,\"seed\":42";
    if(reaction_mode) std::cout << ",\"snapshot\":900,\"snapshotState\":" << snapshot_state;
    std::cout << "}" << ",\"attackerDat\":\"" << json_escape(ad.str()) << "\",\"targetDat\":\"" << json_escape(td.str())
        << "\",\"config\":{\"slots\":[0,70],\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"hpGate\":" << defaults.selected_mode_default_hp_regen_gate_28
        << ",\"mpGate\":" << defaults.selected_mode_mp_regen_gate_2c << ",\"dropGate\":" << defaults.selected_mode_drop_gate_4c << "},\"before\":";
    write_post_effect_state(world);direct_crt_calls.clear();direct_synchronized_calls.clear();
    std::cout << ",\"candidateCount\":" << candidates.candidates_appended
        << ",\"directEffectRejections\":" << candidates.direct_effect_rejections
        << ",\"hitAttempted\":" << (hit_attempted ? "true" : "false");
    if(hit_attempted) {
        const auto result=world.resolve_ordinary_unarmored_standard_hit(0,0);
        std::cout << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message)
            << "\",\"attackerCatchingActionOverride\":" << result.attacker_catching_action_override << ",\"postEffectAction\":" << result.post_effect_action;
    } else {
        // Null result fields mean no hit was attempted, not a fabricated rejected hit result.
        std::cout << ",\"status\":null,\"message\":null,\"attackerCatchingActionOverride\":null,\"postEffectAction\":null";
    }
    std::cout << ",\"after\":";
    write_post_effect_state(world);std::cout << ",\"calls\":";write_post_effect_calls();
    ntsd28::SimulationTickOptions28 options;options.stage_bounds=ntsd28::StageBounds28{800,180,350};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28=defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c=defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c=defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto tick=ntsd28::SimulationTickDriver28{}.step(world,catalog,options);
    std::cout << ",\"following\":";write_post_effect_state(world);std::cout << ",\"followingCalls\":";write_post_effect_calls();
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success?"true":"false") << "}\n";
}
int wmain(int argc, wchar_t** argv) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        if(argc == 2 && std::wstring(argv[1]) == L"--reaction") {
            emit_post_effect(0,{"snapshot_falling",0,12,0,false,false},true,12);
            emit_post_effect(1,{"snapshot_normal",0,12,0,false,false},true,0);
            return 0;
        }
        if(argc != 1) throw std::runtime_error("usage: kind0_post_effect_native_frame_access_witness [--reaction]");
        emit_post_effect(0,{"high_previous_frozen",3,900,13,true,false});
        emit_post_effect(1,{"high_previous_burning",20,900,18,true,false});
        emit_post_effect(2,{"high_previous_normal",3,900,0,true,false});
        emit_post_effect(3,{"implicit200",3,12,0,false,false});
        emit_post_effect(4,{"implicit203",20,12,0,false,false});
        emit_post_effect(5,{"declared203",20,12,0,false,true});
        return 0;
    } catch(const std::exception& e) { std::cerr<<e.what()<<'\n';return 91; }
}
