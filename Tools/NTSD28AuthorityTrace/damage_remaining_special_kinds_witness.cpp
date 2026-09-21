#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_impact_entity(const ntsd28::EntityState28* e) {
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
static void write_impact_state(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":["; bool first = true;
    for (int slot : {0,1,2,70}) { if (!first) std::cout << ','; first = false; write_impact_entity(world.entity(slot)); }
    std::cout << "],\"random\":";write_b2_initial_random(std::cout,world.random().state(),world.random().synchronized_table_hash());std::cout << '}';
}
static void write_impact_calls() {
    std::cout << "{\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"synchronized\":";write_b2_synchronized_calls(std::cout);std::cout << '}';
}

struct ImpactCase {
    const char* group = "character";
    int kind = 9, type = 3, state = 0, respond = 0, environment = 0;
    int attackerHitJ = 0, targetHitJ = 0, oid = 78;
    bool declared = false;
};
static void emit_impact(int index, const ImpactCase& c) {
    const int target = c.kind == 15 ? 0 : c.state == 3005 ? 40 : c.attackerHitJ != 0 ? c.attackerHitJ : c.targetHitJ != 0 ? c.targetHitJ : 30;
    const int y = index % 2 == 0 ? -2 : -3;
    const double vy = index % 3 == 0 ? -6.0 : -1.5;
    std::ostringstream ad, td;
    ad << "<bmp_begin>\nname: ImpactAttacker\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10 hit_j: " << c.attackerHitJ << "\n"
       << "itr:\nkind: " << c.kind << " x: -100 y: -100 w: 300 h: 300 zwidth: 100 bdefend: 8 respond: " << c.respond
       << " fall: 60\nitr_end:\n<frame_end>\n";
    td << "<bmp_begin>\nname: ImpactTarget\n<bmp_end>\n<frame> 10 initial\nstate: " << c.state << " wait: 100 next: 10 hit_j: " << c.targetHitJ << "\n"
       << "bdy:\nkind: 0 x: -100 y: -100 w: 300 h: 300 zwidth: 100\nbdy_end:\n<frame_end>\n";
    if(c.declared && target >= 0 && target <= 999 && target != 10)
        td << "<frame> " << target << " destination\nstate: 3 wait: 37 next: 0\n<frame_end>\n";
    const std::string od = "<bmp_begin>\nname: ImpactOwner\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10\n<frame_end>\n";
    auto a = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(ad.str()));
    auto t = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(td.str()));
    auto o = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(od));
    if(!a->ok() || !t->ok() || !o->ok()) throw std::runtime_error("impact DAT parse failed");
    ntsd28::BattleWorld28 world; world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(77,0,"impact-attacker.dat",a);
    catalog.upsert_definition(c.oid,c.type,"impact-target.dat",t);
    catalog.upsert_definition(79,0,"impact-owner.dat",o);
    for(int slot : {0,1,2,70}) {
        ntsd28::SpawnRequest28 request;
        request.object_id = slot == 0 ? 77 : slot == 70 ? c.oid : 79;
        request.object_type = slot == 70 ? c.type : 0;
        request.definition = slot == 0 ? a : slot == 70 ? t : o;
        request.initial_action = 10; request.hp = 400; request.mp = 497;
        if(!world.spawn_at(slot,request).success) throw std::runtime_error("impact spawn failed");
        auto& e = *world.entity(slot);
        e.battle_group = slot == 70 ? 2 : 1;
        e.position.x = slot == 70 ? 350 : 300; e.position.y = slot == 70 ? y : -20; e.position.z = 250;
        e.position.precise_x = e.position.x + .25; e.position.precise_y = e.position.y - .375; e.position.precise_z = 250.5;
        e.frame.frame_counter = slot == 70 ? 8 : 7; e.frame.action_latch = slot == 70 ? 12 : 11; e.frame.previous_action_078 = 10;
    }
    // Relations are initialized after all spawns, which may clear slot references.
    world.entity(0)->owner_slot = 1; world.entity(1)->owner_slot = 2;
    auto& victim = *world.entity(70);
    victim.environment_state_320 = c.environment; victim.catch_source_slot_90 = 19; victim.impact_source_slot_164 = 23;
    victim.motion.x = 4.28; victim.motion.y = vy; victim.motion.z = -2.14;
    victim.pending_hit_impulse.total.x = 13; victim.pending_hit_impulse.total.y = 17; victim.pending_hit_impulse.total.z = 19;
    world.snapshot_actions();
    const auto candidates = world.rebuild_geometric_hit_candidates();
    if(candidates.candidates_appended != 1) throw std::runtime_error("impact expected one geometric candidate");
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"group\":\"" << c.group << "\",\"kind\":" << c.kind
        << ",\"type\":" << c.type << ",\"state\":" << c.state << ",\"respond\":" << c.respond << ",\"target\":" << target
        << ",\"attackerHitJ\":" << c.attackerHitJ << ",\"targetHitJ\":" << c.targetHitJ << ",\"oid\":" << c.oid
        << ",\"declared\":" << (c.declared ? "true" : "false") << ",\"environment\":" << c.environment
        << ",\"yInt\":" << y << ",\"y\":" << y - .375 << ",\"vy\":" << vy << ",\"seed\":42}"
        << ",\"attackerDat\":\"" << json_escape(ad.str()) << "\",\"targetDat\":\"" << json_escape(td.str()) << "\",\"ownerDat\":\"" << json_escape(od)
        << "\",\"config\":{\"slots\":[0,1,2,70],\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"hpGate\":" << defaults.selected_mode_default_hp_regen_gate_28
        << ",\"mpGate\":" << defaults.selected_mode_mp_regen_gate_2c << ",\"dropGate\":" << defaults.selected_mode_drop_gate_4c << "},\"before\":";
    write_impact_state(world); direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = world.resolve_special_relation_hit(0,0,0);
    std::cout << ",\"applied\":" << (result.status == ntsd28::WorldRelationHitStatus28::applied ? "true" : "false")
        << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message) << "\",\"after\":";
    write_impact_state(world); std::cout << ",\"calls\":"; write_impact_calls();
    ntsd28::SimulationTickOptions28 options;
    options.stage_bounds = ntsd28::StageBounds28{800,180,350}; options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28 = defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c = defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c = defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto tick = ntsd28::SimulationTickDriver28{}.step(world,catalog,options);
    std::cout << ",\"following\":"; write_impact_state(world); std::cout << ",\"followingCalls\":"; write_impact_calls();
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false") << "}\n";
}
int wmain() {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        for(int state : {0,3005}) for(bool declared : {false,true}) {
            ImpactCase c; c.state=state;c.declared=declared;emit_impact(index++,c);
        }
        for(int choice : {1,2,3}) {
            ImpactCase c;c.attackerHitJ=choice==2?0:857;c.targetHitJ=choice==1?900:998;c.declared=true;emit_impact(index++,c);
        }
        for(int type : {1,2,4,6}) for(bool preserve : {false,true}) {
            ImpactCase c;c.kind=15;c.type=type;c.state=preserve?(type==2?2000:1000):0;c.declared=!preserve;emit_impact(index++,c);
        }
        for(int oid : {201,202}) {
            ImpactCase c;c.kind=15;c.type=1;c.oid=oid;emit_impact(index++,c);
        }
        if(index!=17)throw std::runtime_error("special kind matrix count changed");
        return 0;
    } catch(const std::exception& e) { std::cerr << e.what() << '\n'; return 91; }
}
