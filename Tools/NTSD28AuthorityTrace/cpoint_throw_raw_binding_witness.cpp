#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

struct ThrowDestination { int action; bool declared; };

static std::shared_ptr<const ntsd28::DatDocument> victim_throw_definition(ThrowDestination destination) {
    std::ostringstream dat;
    dat << "<bmp_begin>\nname: ThrowRawBinding\nweapon_hp: 17\n<bmp_end>\n";
    dat << "<frame> 130 relation\nstate: 10 wait: 100 next: 130 centerx: 39 centery: 79\n"
        << "cpoint:\nkind: 2\ncpoint_end:\n<frame_end>\n";
    if (destination.declared) dat << "<frame> " << destination.action
        << " destination\nstate: 0 wait: 37 next: 0 centerx: 3 centery: 7\n<frame_end>\n";
    auto result = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat.str()));
    if (!result->ok()) throw std::runtime_error("throw fixture DAT rejected");
    return result;
}

static void write_throw_entity(const ntsd28::EntityState28* entity) {
    if (!entity) { std::cout << "null"; return; }
    const auto* descriptor = entity->definition->frame(entity->frame.action);
    std::cout << "{\"raw\":";
    write_entity(std::cout, *entity, 1);
    std::cout << ",\"action\":" << entity->frame.action
        << ",\"snapshot\":" << entity->frame.tick_action_snapshot
        << ",\"counter\":" << entity->frame.frame_counter
        << ",\"latch\":" << entity->frame.action_latch
        << ",\"available\":" << (descriptor ? "true" : "false")
        << ",\"wait\":" << (descriptor ? descriptor->values.integer("wait").value_or(0) : 0)
        << ",\"next\":" << (descriptor ? descriptor->values.integer("next").value_or(0) : 0)
        << ",\"catchTarget\":" << entity->catch_target_slot_8c
        << ",\"catchSource\":" << entity->catch_source_slot_90
        << ",\"timeout\":" << entity->catch_timeout_94
        << ",\"environment\":" << entity->environment_state_320
        << ",\"environmentSource\":" << entity->environment_source_slot_160 << '}';
}

static void emit_throw(ThrowDestination next, ThrowDestination victim_action,
    bool facing, bool select, int injury, int index) {
    // Build victim_action into the immutable original catcher cpoint.
    std::ostringstream replacement;
    replacement << "<bmp_begin>\nname: ThrowRawBinding\nweapon_hp: 17\n<bmp_end>\n"
        << "<frame> 100 relation\nstate: 9 wait: 100 next: " << next.action
        << " centerx: 39 centery: 79\ncpoint:\nkind: 1 x: 50 y: 60 vaction: " << victim_action.action
        << " aaction: 101 throwvx: 1.5 throwvy: -2.25 throwvz: 3 throwinjury: " << injury
        << "\ncpoint_end:\n<frame_end>\n"
        << "<frame> 101 selected\nstate: 9 wait: 23 next: 102 centerx: 399 centery: 799\n"
        << "cpoint:\nkind: 1 vaction: 131\ncpoint_end:\n<frame_end>\n";
    if (next.declared) replacement << "<frame> " << next.action
        << " destination\nstate: 0 wait: 37 next: 0 centerx: 3 centery: 7\n<frame_end>\n";
    const auto original = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(replacement.str()));
    if (!original->ok()) throw std::runtime_error("catcher DAT rejected");
    const auto victim_definition = victim_throw_definition(victim_action);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(77, 0, "catcher.dat", original);
    catalog.upsert_definition(78, 0, "victim.dat", victim_definition);
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    for (int slot : {0, 1, 2}) {
        ntsd28::SpawnRequest28 request;
        request.object_id = slot == 1 ? 78 : 77;
        request.object_type = 0;
        request.definition = slot == 1 ? victim_definition : original;
        request.initial_action = slot == 1 ? 130 : (slot == 0 ? 100 : 0);
        request.hp = 500; request.mp = 500;
        request.position.x = 100 + slot * 10; request.position.y = -5; request.position.z = 200;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("throw spawn failed");
        world.entity(slot)->frame.action_latch = 11 + slot;
        world.entity(slot)->frame.frame_counter = 7 + slot;
    }
    world.snapshot_actions();
    world.entity(2)->owner_slot = 0;
    auto* catcher = world.entity(0);
    auto* victim = world.entity(1);
    catcher->frame.facing = facing;
    catcher->catch_target_slot_8c = 1;
    catcher->catch_timeout_94 = 300;
    victim->catch_source_slot_90 = 0;
    victim->motion.z = 7;
    victim->environment_state_320 = 41;
    victim->environment_source_slot_160 = 42;
    catcher->input.current.set(ntsd28::InputKey28::attack, select);
    catcher->input.edge_window[static_cast<std::size_t>(ntsd28::InputKey28::attack)] = select ? 5 : 0;
    std::cout << "{\"index\":" << index << ",\"next\":" << next.action
        << ",\"nextDeclared\":" << (next.declared ? "true" : "false")
        << ",\"vaction\":" << victim_action.action
        << ",\"victimDeclared\":" << (victim_action.declared ? "true" : "false")
        << ",\"facing\":" << (facing ? "true" : "false")
        << ",\"select\":" << (select ? "true" : "false") << ",\"injury\":" << injury << ",\"before\":[";
    write_throw_entity(catcher); std::cout << ','; write_throw_entity(victim); std::cout << ','; write_throw_entity(world.entity(2));
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = world.advance_catch_relations();
    std::cout << "],\"thrown\":" << result.thrown_relations
        << ",\"transitions\":" << result.input_action_transitions
        << ",\"definitionPreserved\":" << (catcher->definition == original && victim->definition == victim_definition && world.entity(2)->definition == original ? "true" : "false")
        << ",\"crtCalls\":" << direct_crt_calls.size() << ",\"nativeCalls\":" << direct_synchronized_calls.size() << ",\"after\":[";
    write_throw_entity(catcher); std::cout << ','; write_throw_entity(victim); std::cout << ','; write_throw_entity(world.entity(2));
    ntsd28::SimulationTickOptions28 options;
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    const ntsd28_playable::BattleConfig28 defaults;
    options.resource_rules.selected_mode_default_hp_regen_gate_28 = defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c = defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c = defaults.selected_mode_drop_gate_4c;
    const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
    std::cout << "],\"nextTick\":[";
    write_throw_entity(world.entity(0)); std::cout << ','; write_throw_entity(world.entity(1)); std::cout << ','; write_throw_entity(world.entity(2));
    std::cout << "],\"lifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false") << "}\n";
}

int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        const ThrowDestination cases[] = {{0,true},{99,false},{900,true},{999,true},{999,false},{1000,false},{-900,false}};
        int index = 0;
        for (const auto next : cases) for (const auto victim : cases)
        for (bool facing : {false,true}) for (bool select : {false,true})
        for (int injury : {0,-1}) emit_throw(next,victim,facing,select,injury,index++);
        if (index != 392) throw std::runtime_error("throw case count changed");
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
