#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_fall_entity(const ntsd28::EntityState28* e) {
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
static void write_fall_state(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":["; bool first = true;
    for (int slot : {0,70}) { if (!first) std::cout << ','; first = false; write_fall_entity(world.entity(slot)); }
    std::cout << "],\"random\":";write_b2_initial_random(std::cout,world.random().state(),world.random().synchronized_table_hash());std::cout << '}';
}
static void write_fall_calls() {
    std::cout << "{\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"synchronized\":";write_b2_synchronized_calls(std::cout);std::cout << '}';
}


struct FallCase {
    const char* group;
    int hp, initial_fall;
};
static void emit_fall(int index, const FallCase& c, bool vertical = false, int dvy = 0, double pending_y = 17.0) {
    std::ostringstream ad, td;
    ad << "<bmp_begin>\nname: FallAttacker\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10\n"
       << "itr:\nkind: 0 effect: 0 x: -100 y: -100 w: 300 h: 300 zwidth: 100 injury: 5 fall: 1 bdefend: 0 vrest: 1";
    if (vertical) ad << " dvy: " << dvy;
    ad << "\nitr_end:\n<frame_end>\n";
    td << "<bmp_begin>\nname: FallTarget\n<bmp_end>\n<frame> 10 initial\nstate: 0 wait: 100 next: 10\n"
       << "bdy:\nkind: 0 x: -100 y: -100 w: 300 h: 300 zwidth: 100\nbdy_end:\n<frame_end>\n";
    // Declare only the ordinary/death reaction support frames used by this fixture.
    for(int reaction : {180,186,220})
        td << "<frame> " << reaction << " reaction_support\nstate: 0 wait: 100 next: " << reaction << "\n<frame_end>\n";
    auto a=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(ad.str()));
    auto t=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(td.str()));
    if(!a->ok() || !t->ok())throw std::runtime_error("fall DAT parse failed");
    ntsd28::BattleWorld28 world;world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(77,0,"fall-attacker.dat",a);catalog.upsert_definition(78,0,"fall-target.dat",t);
    for(int slot : {0,70}) {
        ntsd28::SpawnRequest28 request;request.object_id=slot==0?77:78;request.object_type=0;
        request.definition=slot==0?a:t;request.initial_action=10;request.hp=slot==0?400:c.hp;request.mp=497;
        request.battle_group=slot==0?1:2;
        request.position.x=slot==0?300:350;request.position.y=0;request.position.z=250;
        if(!world.spawn_at(slot,request).success)throw std::runtime_error("fall spawn failed");
        auto& e=*world.entity(slot);
        e.position.precise_x=e.position.x+.25;e.position.precise_y=0;e.position.precise_z=250.5;
        e.frame.frame_counter=slot==0?7:8;e.frame.action_latch=slot==0?13:11;
        e.frame.previous_action_078=slot==0?14:12;
        e.motion.x=1.25;e.motion.y=0;e.motion.z=-.5;
        e.pending_hit_impulse.total.x=13;e.pending_hit_impulse.total.y=17;e.pending_hit_impulse.total.z=19;
    }
    world.entity(70)->hit_reaction_timer=c.initial_fall;
    if (vertical) world.entity(70)->pending_hit_impulse.total.y=pending_y;
    world.snapshot_actions();
    const auto candidates=world.rebuild_geometric_hit_candidates();
    if(candidates.candidates_appended!=1)throw std::runtime_error("fall expected one geometric candidate");
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"group\":\"" << c.group
        << "\",\"hp\":" << c.hp << ",\"initialFall\":" << c.initial_fall
        << ",\"injury\":5,\"fall\":1,\"effect\":0,\"kind\":0,\"seed\":42";
    if (vertical) std::cout << ",\"dvy\":" << dvy << ",\"initialPendingY\":" << pending_y << ",\"yInt\":0";
    std::cout << "}" << ",\"attackerDat\":\"" << json_escape(ad.str()) << "\",\"targetDat\":\"" << json_escape(td.str())
        << "\",\"config\":{\"slots\":[0,70],\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"hpGate\":" << defaults.selected_mode_default_hp_regen_gate_28
        << ",\"mpGate\":" << defaults.selected_mode_mp_regen_gate_2c << ",\"dropGate\":" << defaults.selected_mode_drop_gate_4c << "},\"before\":";
    write_fall_state(world);direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto result=world.resolve_ordinary_unarmored_standard_hit(0,0);
    std::cout << ",\"status\":" << static_cast<int>(result.status) << ",\"message\":\"" << json_escape(result.message)
        << "\",\"attackerCatchingActionOverride\":" << result.attacker_catching_action_override << ",\"postEffectAction\":" << result.post_effect_action << ",\"after\":";
    write_fall_state(world);std::cout << ",\"calls\":";write_fall_calls();
    ntsd28::SimulationTickOptions28 options;options.stage_bounds=ntsd28::StageBounds28{800,180,350};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28=defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c=defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c=defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto tick=ntsd28::SimulationTickDriver28{}.step(world,catalog,options);
    std::cout << ",\"following\":";write_fall_state(world);std::cout << ",\"followingCalls\":";write_fall_calls();
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success?"true":"false") << "}\n";
}
int wmain(int argc, wchar_t** argv) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        if (argc == 2 && std::wstring(argv[1]) == L"--vertical") {
            emit_fall(0,{"nonzero_clamp",400,79},true,2,17.0);
            emit_fall(1,{"nonzero_fraction",400,79},true,2,-1.5);
            return 0;
        }
        if (argc != 1) throw std::runtime_error("usage: standard_hit_fall80_preservation_witness [--vertical]");
        emit_fall(0,{"lethal",1,0});
        emit_fall(1,{"nonlethal80",400,79});
        emit_fall(2,{"normal20",400,0});
        return 0;
    } catch(const std::exception& e) { std::cerr<<e.what()<<'\n';return 91; }
}
