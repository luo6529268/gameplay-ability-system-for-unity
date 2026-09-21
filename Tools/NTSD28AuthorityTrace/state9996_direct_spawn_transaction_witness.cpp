#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"


struct CloneCase {
    const char* name;
    int target_type = 0;
    bool declared = true;
    int missing_oid = 0;
    int free_slots = 950;
    int counter = 1;
    bool pending = false;
    int source_type = 0;
    int source_state = 9996;
    bool following = false;
};
static void write_entity_extra(const ntsd28::EntityState28* entity) {
    if (entity == nullptr) {
        std::cout << "null";
        return;
    }
    const auto& e = *entity;
    const auto* f = e.definition->frame(e.frame.action);
    std::cout << "{\"raw\":";
    write_entity(std::cout, e, 1);
    std::cout << ",\"available\":" << (f ? "true" : "false")
        << ",\"state\":" << (f ? f->values.integer("state").value_or(0) : 0)
        << ",\"wait\":" << (f ? f->values.integer("wait").value_or(0) : 0)
        << ",\"next\":" << (f ? f->values.integer("next").value_or(0) : 0)
        << ",\"aiProfile\":" << e.ai_profile_object_id << ",\"dropMode\":" << e.definition_drop_mode
        << ",\"incomingScale\":" << e.incoming_damage_scale_340 << ",\"modeScale\":" << e.mode_damage_scale_percent
        << ",\"pendingCount\":" << e.pending_hit_impulse.contribution_count
        << ",\"pending\":[" << e.pending_hit_impulse.total.x << ',' << e.pending_hit_impulse.total.y << ',' << e.pending_hit_impulse.total.z << ']'
        << ",\"display\":[" << e.display_score_1f0 << ',' << e.display_score_step_1f4 << ','
        << e.display_damage_total_1f8 << ',' << e.display_damage_step_1fc << ',' << e.display_current_hp_200 << ','
        << e.display_current_hp_step_204 << ',' << e.display_effective_max_hp_208 << ',' << e.display_effective_max_hp_step_20c << ']'
        << ",\"input\":";
    write_b2_input_entity(std::cout, e, 1);
    std::cout << '}';
}
static void write_state(ntsd28::BattleWorld28& world) {
    int count = 0;
    int fillers = 0;
    for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
        const auto* e = world.entity(slot);
        if (e != nullptr) {
            ++count;
            if (e->object_id == 31999) ++fillers;
        }
    }
    std::cout << "{\"activeCount\":" << count << ",\"fillerCount\":" << fillers << ",\"entities\":[";
    bool first = true;
    for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
        const auto* e = world.entity(slot);
        if (e == nullptr || e->object_id == 31999) continue;
        if (!first) std::cout << ',';
        first = false;
        write_entity_extra(e);
    }
    std::cout << "],\"random\":";
    write_b2_initial_random(std::cout, world.random().state(), world.random().synchronized_table_hash());
    std::cout << '}';
}
static void write_calls() {
    std::cout << "{\"crt\":";
    write_b2_crt_calls(std::cout);
    std::cout << ",\"synchronized\":";
    write_b2_synchronized_calls(std::cout);
    std::cout << '}';
}
static void emit(int index, const CloneCase& c) {
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    std::ostringstream source_text, target_text;
    source_text << "<bmp_begin>\nname: CloneSource\n<bmp_end>\n<frame> 20 source\nstate: " << c.source_state
        << " wait: 100 next: 20\n<frame_end>\n";
    target_text << "<bmp_begin>\nname: DirectCloneTarget\nuse_ai: 34 drop: 2 weapon_hp: 37\n<bmp_end>\n"
        << "<stats> ohp: 20 omp: 30 max_mp: 731 defend: 140 <stats_end>\n"
        << "<armor>\ntype: 1 hp: 23 recover: 17\n<armor_end>\n";
    if (c.declared) {
        for (int frame = 0; frame < 4; ++frame)
            target_text << "<frame> " << frame << " clone\nstate: 0 wait: 41 next: " << frame << "\n<frame_end>\n";
    }
    const std::string filler_text = "<bmp_begin>\nname: Filler\n<bmp_end>\n<frame> 0 filler\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
    auto source = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(source_text.str()));
    auto target = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(target_text.str()));
    auto filler = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(filler_text));
    if (!source->ok() || !target->ok() || !filler->ok()) throw std::runtime_error("clone fixture DAT parse failed");
    catalog.upsert_definition(31980, c.source_type, "clone-source.dat", source);
    for (int oid : {217, 218}) {
        if (oid != c.missing_oid) catalog.upsert_definition(oid, c.target_type, "clone-target.dat", target);
    }
    ntsd28::SpawnRequest28 request;
    request.object_id = 31980;
    request.object_type = c.source_type;
    request.definition = source;
    request.initial_action = 20;
    request.hp = 137;
    request.mp = 211;
    request.battle_group = 3;
    request.owner_slot = 9;
    request.position.x = 300;
    request.position.y = -20;
    request.position.z = 250;
    if (!world.spawn_at(0, request).success) throw std::runtime_error("clone source spawn failed");
    for (int slot = 50 + c.free_slots; slot < 1000; ++slot) {
        ntsd28::SpawnRequest28 fill;
        fill.object_id = 31999;
        fill.object_type = 5;
        fill.definition = filler;
        if (!world.spawn_at(slot, fill).success) throw std::runtime_error("clone filler spawn failed");
    }
    auto& e = *world.entity(0);
    e.frame.frame_counter = c.counter;
    e.frame.action_latch = 13;
    e.frame.previous_action_078 = 14;
    e.lifecycle_resolution_pending = c.pending;
    world.snapshot_actions();
    std::cout << "{\"index\":" << index << ",\"name\":\"" << c.name << "\",\"synthetic\":true,\"params\":{\"targetType\":" << c.target_type
        << ",\"declared\":" << (c.declared ? "true" : "false") << ",\"missingOid\":" << c.missing_oid << ",\"freeSlots\":" << c.free_slots
        << ",\"counter\":" << c.counter << ",\"pending\":" << (c.pending ? "true" : "false") << ",\"sourceType\":" << c.source_type
        << ",\"sourceState\":" << c.source_state << ",\"seed\":42},\"config\":{\"capacity\":1000,\"transientStart\":50,\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"tickOptions\":\"default except stage/controls\"},\"sourceDat\":\"" << json_escape(source_text.str())
        << "\",\"targetDat\":\"" << json_escape(target_text.str()) << "\",\"before\":";
    write_state(world);
    direct_crt_calls.clear();
    direct_synchronized_calls.clear();
    const auto pass = world.materialize_special_state_clones(0, catalog);
    std::cout << ",\"result\":{\"success\":" << (pass.success ? "true" : "false") << ",\"applicable\":" << (pass.applicable ? "true" : "false")
        << ",\"spawned\":" << pass.spawned << ",\"unresolved\":" << pass.unresolved << ",\"events\":[";
    bool first = true;
    for (const auto& event : pass.events) {
        if (!first) std::cout << ',';
        first = false;
        std::cout << "{\"cloneIndex\":" << event.clone_index << ",\"oid\":" << event.object_id << ",\"slot\":" << event.slot
            << ",\"message\":\"" << json_escape(event.message) << "\"}";
    }
    std::cout << "]},\"after\":";
    write_state(world);
    std::cout << ",\"calls\":";
    write_calls();
    if (c.following) {
        ntsd28::SimulationTickOptions28 options;
        options.stage_bounds = ntsd28::StageBounds28{800,180,350};
        options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
        direct_crt_calls.clear();
        direct_synchronized_calls.clear();
        const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
        std::cout << ",\"following\":";
        write_state(world);
        std::cout << ",\"followingCalls\":";
        write_calls();
        std::cout << ",\"followingLifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false");
    }
    std::cout << "}\n";
}
int wmain() {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        const CloneCase cases[] = {
            {"authored_type0",0,true,0,950,1,false,0,9996,true},
            {"implicit_type0",0,false},
            {"authored_type3",3,true,0,950,1,false,0,9996,true},
            {"implicit_type3",3,false},
            {"missing217",0,true,217},
            {"missing218",0,true,218},
            {"partial_capacity",0,true,0,2},
            {"full_capacity",0,true,0,0},
            {"counter_guard",0,true,0,950,2},
            {"pending_guard",0,true,0,950,1,true},
            {"type_guard",0,true,0,950,1,false,3},
            {"encoded_unresolved",0,true,0,950,1,false,0,8000000}
        };
        int index = 0;
        for (const auto& c : cases) emit(index++, c);
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 91;
    }
}
