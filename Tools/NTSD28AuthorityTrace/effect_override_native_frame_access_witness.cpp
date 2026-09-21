#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_effect_entity(const ntsd28::EntityState28* e) {
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
static void write_effect_state(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":["; bool first = true;
    for (int slot : {0,70}) { if (!first) std::cout << ','; first = false; write_effect_entity(world.entity(slot)); }
    std::cout << "],\"random\":";write_b2_initial_random(std::cout,world.random().state(),world.random().synchronized_table_hash());std::cout << '}';
}
static void write_effect_calls() {
    std::cout << "{\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"synchronized\":";write_b2_synchronized_calls(std::cout);std::cout << '}';
}


struct EffectCase {
    const char* group = "binding";
    int catching = 69, caught = 70;
    bool attacker_declared = false, target_declared = false;
    int latch = 11, previous = 12, history_state = 0, history_body = -1;
    bool history_declared = false;
    int picked = 0, hp = 400;
};
static void emit_effect(int index, const EffectCase& c) {
    std::ostringstream ad, td;
    ad << "<bmp_begin>\nname: EffectAttacker\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10\n"
       << "itr:\nkind: 0 effect: 8 x: -100 y: -100 w: 300 h: 300 zwidth: 100 injury: 5 fall: 0 bdefend: 0 vrest: 1"
       << " catchingact: " << c.catching << " caughtact: " << c.caught << " pickedact: " << c.picked << "\nitr_end:\n<frame_end>\n";
    td << "<bmp_begin>\nname: EffectTarget\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10\n"
       << "bdy:\nkind: 0 x: -100 y: -100 w: 300 h: 300 zwidth: 100\nbdy_end:\n<frame_end>\n";
    // Isolate ordinary/death reaction descriptors from the override destinations.
    for(int reaction : {180,186,220})
        td << "<frame> " << reaction << " reaction_support\nstate: 0 wait: 100 next: " << reaction << "\n<frame_end>\n";
    if(c.attacker_declared) ad << "<frame> " << c.catching << " destination\nstate: 3 wait: 37 next: 0\n<frame_end>\n";
    if(c.target_declared) td << "<frame> " << c.caught << " destination\nstate: 3 wait: 41 next: 0\n<frame_end>\n";
    if(c.history_declared) {
        if(c.target_declared && c.caught==900) throw std::runtime_error("history/destination collision");
        td << "<frame> 900 history\nstate: " << c.history_state << " wait: 43 next: 0\n";
        if(c.history_body>=0) td << "bdy:\nkind: " << c.history_body << " x: 0 y: 0 w: 1 h: 1\nbdy_end:\n";
        td << "<frame_end>\n";
    }
    auto a=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(ad.str()));
    auto t=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(td.str()));
    if(!a->ok() || !t->ok())throw std::runtime_error("effect DAT parse failed");
    ntsd28::BattleWorld28 world;world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(77,0,"effect-attacker.dat",a);catalog.upsert_definition(78,0,"effect-target.dat",t);
    for(int slot : {0,70}) {
        ntsd28::SpawnRequest28 request;request.object_id=slot==0?77:78;request.object_type=0;
        request.definition=slot==0?a:t;request.initial_action=10;request.hp=slot==0?400:c.hp;request.mp=497;
        request.battle_group=slot==0?1:2;
        request.position.x=slot==0?300:350;request.position.y=0;request.position.z=250;
        if(!world.spawn_at(slot,request).success)throw std::runtime_error("effect spawn failed");
        auto& e=*world.entity(slot);
        e.position.precise_x=e.position.x+.25;e.position.precise_y=0;e.position.precise_z=250.5;
        e.frame.frame_counter=slot==0?7:8;e.frame.action_latch=slot==0?13:c.latch;
        e.frame.previous_action_078=slot==0?14:c.previous;
        e.motion.x=1.25;e.motion.y=0;e.motion.z=-.5;
        e.pending_hit_impulse.total.x=13;e.pending_hit_impulse.total.y=17;e.pending_hit_impulse.total.z=19;
    }
    world.snapshot_actions();
    const auto candidates=world.rebuild_geometric_hit_candidates();
    if(candidates.candidates_appended!=1)throw std::runtime_error("effect expected one geometric candidate");
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"group\":\"" << c.group << "\",\"catching\":" << c.catching << ",\"caught\":" << c.caught
        << ",\"attackerDeclared\":" << (c.attacker_declared?"true":"false") << ",\"targetDeclared\":" << (c.target_declared?"true":"false")
        << ",\"latch\":" << c.latch << ",\"previous\":" << c.previous << ",\"historyDeclared\":" << (c.history_declared?"true":"false")
        << ",\"historyState\":" << c.history_state << ",\"historyBody\":" << c.history_body << ",\"picked\":" << c.picked << ",\"hp\":" << c.hp << ",\"seed\":42}"
        << ",\"attackerDat\":\"" << json_escape(ad.str()) << "\",\"targetDat\":\"" << json_escape(td.str())
        << "\",\"config\":{\"slots\":[0,70],\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"hpGate\":" << defaults.selected_mode_default_hp_regen_gate_28
        << ",\"mpGate\":" << defaults.selected_mode_mp_regen_gate_2c << ",\"dropGate\":" << defaults.selected_mode_drop_gate_4c << "},\"before\":";
    write_effect_state(world);direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto result=world.resolve_ordinary_unarmored_standard_hit(0,0);
    std::cout << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message)
        << "\",\"attackerCatchingActionOverride\":" << result.attacker_catching_action_override << ",\"postEffectAction\":" << result.post_effect_action << ",\"after\":";
    write_effect_state(world);std::cout << ",\"calls\":";write_effect_calls();
    ntsd28::SimulationTickOptions28 options;options.stage_bounds=ntsd28::StageBounds28{800,180,350};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28=defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c=defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c=defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto tick=ntsd28::SimulationTickDriver28{}.step(world,catalog,options);
    std::cout << ",\"following\":";write_effect_state(world);std::cout << ",\"followingCalls\":";write_effect_calls();
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success?"true":"false") << "}\n";
}
int wmain() {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index=0;
        const int attackerActions[]={69,857,900,998,999,998,1000,71};
        const int targetActions[]={70,900,1000,999,998,999,69,72};
        for(int i=0;i<8;++i) {
            EffectCase c;c.catching=attackerActions[i];c.caught=targetActions[i];
            c.attacker_declared=i==5||i==7;c.target_declared=i==5||i==7;emit_effect(index++,c);
        }
        { EffectCase c;c.group="reader";c.latch=69;emit_effect(index++,c); }
        { EffectCase c;c.group="reader";c.latch=900;emit_effect(index++,c); }
        { EffectCase c;c.group="reader";c.previous=900;c.picked=12;emit_effect(index++,c); }
        { EffectCase c;c.group="reader";c.previous=900;c.picked=12;c.history_declared=true;c.history_state=12;emit_effect(index++,c); }
        { EffectCase c;c.group="suppression";c.latch=900;c.history_declared=true;c.history_body=50;emit_effect(index++,c); }
        { EffectCase c;c.group="suppression";c.latch=900;c.history_declared=true;c.history_state=602;emit_effect(index++,c); }
        { EffectCase c;c.group="nonpositive";c.catching=0;c.caught=-1;emit_effect(index++,c); }
        { EffectCase c;c.group="death";c.hp=1;emit_effect(index++,c); }
        if(index!=16)throw std::runtime_error("effect matrix count changed");
        return 0;
    } catch(const std::exception& e) { std::cerr<<e.what()<<'\n';return 91; }
}
