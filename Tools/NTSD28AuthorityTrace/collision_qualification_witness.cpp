#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static std::shared_ptr<const ntsd28::DatDocument> qualification_definition(bool attacker, int effect, bool declared999) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: CollisionQualification\n<bmp_end>\n<frame> 0 geometry\nstate: 0 wait: 100 next: 0\n";
    if (attacker) text << "itr:\nkind: 0 effect: " << effect << " x: -20 y: -20 w: 60 h: 60 vrest: 1 injury: 1\nitr_end:\n";
    else text << "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
    text << "<frame_end>\n<frame> 10 state18\nstate: 18 wait: 100 next: 0\n<frame_end>\n"
         << "<frame> 998 state13\nstate: 13 wait: 100 next: 0\n<frame_end>\n";
    if (declared999) text << "<frame> 999 state19\nstate: 19 wait: 100 next: 0\n<frame_end>\n";
    auto result = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!result->ok()) throw std::runtime_error("qualification definition rejected");
    return result;
}

static void write_qualification_world(const ntsd28::BattleWorld28& world, int count) {
    std::cout << '[';
    for (int slot = 0; slot < count; ++slot) {
        if (slot) std::cout << ',';
        if (const auto* entity = world.entity(slot)) write_entity(std::cout, *entity, 1);
        else std::cout << "null";
    }
    std::cout << ']';
}

static void write_pair(const ntsd28::HitCandidatePairSnapshot28& p) {
    std::cout << "{\"valid\":" << (p.valid ? "true" : "false")
        << ",\"attackerAction\":" << p.attacker_action << ",\"targetAction\":" << p.target_action
        << ",\"attackerCurrentState\":" << p.attacker_current_state << ",\"targetCurrentState\":" << p.target_current_state
        << ",\"attackerPreviousState\":" << p.attacker_previous_state << ",\"targetPreviousState\":" << p.target_previous_state
        << ",\"attackerTickState\":" << p.attacker_tick_state << ",\"targetTickState\":" << p.target_tick_state
        << ",\"attackerGroup\":" << p.attacker_battle_group << ",\"targetGroup\":" << p.target_battle_group << '}';
}

static void emit_qualification(int a, int t, int pa, int pt, int effect, bool same, bool declared999, int index) {
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    for (int slot : {0, 1}) {
        ntsd28::SpawnRequest28 request;
        request.object_id = 77 + slot; request.object_type = 0;
        request.definition = qualification_definition(slot == 0, effect, declared999);
        request.hp = 500; request.mp = 500; request.battle_group = slot == 0 || same ? 1 : 2;
        request.position.x = 100 + slot * 10; request.position.z = 200;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("qualification spawn failed");
    }
    world.snapshot_actions();
    world.entity(0)->frame.action = a; world.entity(1)->frame.action = t;
    world.entity(0)->frame.previous_action_078 = pa; world.entity(1)->frame.previous_action_078 = pt;
    std::cout << "{\"kind\":\"qualification\",\"index\":" << index << ",\"a\":" << a << ",\"t\":" << t
        << ",\"pa\":" << pa << ",\"pt\":" << pt << ",\"effect\":" << effect
        << ",\"same\":" << (same ? "true" : "false") << ",\"declared999\":" << (declared999 ? "true" : "false") << ",\"before\":";
    write_qualification_world(world, 2);
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto collected = world.rebuild_geometric_hit_candidates();
    std::cout << ",\"candidates\":" << collected.candidates_appended << ",\"success\":" << (collected.success ? "true" : "false")
        << ",\"effectRejections\":" << collected.direct_effect_rejections
        << ",\"groupRejections\":" << collected.pre_candidate_eligibility_rejections << ",\"pair\":";
    if (world.entity(0)->hit_candidates.size()) write_pair(world.entity(0)->hit_candidates.items()[0].pair_snapshot);
    else std::cout << "null";
    const auto eligibility = world.classify_ordinary_hit_eligibility(0, 0);
    std::cout << ",\"eligibility\":" << static_cast<int>(eligibility.status) << ",\"message\":\"" << json_escape(eligibility.message) << "\",\"afterCollection\":";
    write_qualification_world(world, 2);
    world.entity(0)->frame.action = 1000; world.entity(1)->frame.action = 1000;
    const auto after = world.classify_ordinary_hit_eligibility(0, 0);
    std::cout << ",\"afterCurrentMissingEligibility\":" << static_cast<int>(after.status)
        << ",\"afterMessage\":\"" << json_escape(after.message) << "\",\"afterCurrentMissing\":";
    write_qualification_world(world, 2);
    std::cout << ",\"nativeCalls\":" << direct_synchronized_calls.size() << ",\"crtCalls\":" << direct_crt_calls.size() << "}\n";
}

static std::shared_ptr<const ntsd28::DatDocument> driver_definition(int slot, int caught_action) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: CollisionQualificationDriver\n<bmp_end>\n<frame> 0 initial\nstate: 0 wait: 100 next: 0\n";
    if (slot == 0) text << "itr:\nkind: 3 x: -5 y: -20 w: 20 h: 40 catchingact: 10 caughtact: " << caught_action << " respond: 73\nitr_end:\n";
    if (slot == 1) text << "itr:\nkind: 0 x: 15 y: -20 w: 10 h: 40 injury: 1 fall: 0 vrest: 1\nitr_end:\n";
    if (slot != 0) text << "bdy:\nkind: 0 x: -5 y: -20 w: 10 h: 40\nbdy_end:\n";
    text << "<frame_end>\n";
    if (slot < 2) text << "<frame> " << (slot == 0 ? 10 : caught_action) << " caught\nstate: " << (slot == 0 ? 9 : 10)
        << " wait: 100 next: 0\ncpoint:\nkind: " << (slot == 0 ? 1 : 2) << " x: 10 y: 0 hurtable: 1\ncpoint_end:\n<frame_end>\n";
    auto result = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    if (!result->ok()) throw std::runtime_error("qualification driver DAT rejected");
    return result;
}

static void emit_driver(int caught_action) {
    ntsd28::BattleWorld28 world;
    ntsd28::ObjectDefinitionCatalog28 catalog;
    world.random().reset_from_seed(42);
    for (int slot : {0, 1, 2}) {
        auto definition = driver_definition(slot, caught_action);
        catalog.upsert_definition(77 + slot, 0, "qualification-driver.dat", definition);
        ntsd28::SpawnRequest28 request;
        request.object_id = 77 + slot; request.object_type = 0; request.definition = definition;
        request.hp = 500; request.mp = 500; request.battle_group = slot + 1;
        request.position.x = slot == 2 ? 130 : 100 + slot * 10; request.position.z = 200;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("qualification driver spawn failed");
    }
    ntsd28::SimulationTickOptions28 options;
    const ntsd28_playable::BattleConfig28 defaults;
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28 = defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c = defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c = defaults.selected_mode_drop_gate_4c;
    std::cout << "{\"kind\":\"driver\",\"caughtAction\":" << caught_action << ",\"before\":";
    write_qualification_world(world, 3);
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
    std::cout << ",\"after\":"; write_qualification_world(world, 3);
    std::cout << ",\"candidates\":" << result.candidates.candidates_appended << ",\"appliedHits\":" << result.diagnostics.applied_hits
        << ",\"relationHits\":" << result.diagnostics.applied_relation_hits << ",\"unsupportedHits\":" << result.diagnostics.unsupported_hits
        << ",\"diagnostics\":[";
    bool first = true;
    for (const auto& message : result.diagnostics.messages) { if (!first) std::cout << ','; first = false; std::cout << '"' << json_escape(message) << '"'; }
    std::cout << "],\"nativeCalls\":" << direct_synchronized_calls.size() << ",\"crtCalls\":" << direct_crt_calls.size() << "}\n";
}

int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        const std::pair<int,int> current[] = {{0,0},{10,0},{0,10},{10,10},{998,999},{999,998},{450,450},{1000,0},{0,1000},{1000,1000}};
        const std::pair<int,int> previous[] = {{0,0},{10,0},{0,10},{998,999},{450,450},{1000,1000}};
        for (const auto& c : current) for (const auto& p : previous)
        for (int effect : {0,20}) for (bool same : {false,true}) for (bool declared : {false,true})
            emit_qualification(c.first, c.second, p.first, p.second, effect, same, declared, index++);
        if (index != 480) throw std::runtime_error("qualification case count changed");
        emit_driver(10); emit_driver(998);
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
