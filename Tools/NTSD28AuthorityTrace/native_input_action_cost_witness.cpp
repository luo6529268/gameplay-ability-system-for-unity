#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/input_routing.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_input_state(ntsd28::BattleWorld28& world) {
    if (world.entity(0) == nullptr) { std::cout << "null"; return; }
    const auto& entity = *world.entity(0);
    const auto* frame = entity.definition->frame(entity.frame.action);
    std::cout << "{\"raw\":";
    write_entity(std::cout, entity, 1);
    std::cout << ",\"runAccumulator\":" << entity.input.run_accumulator
        << ",\"available\":" << (frame ? "true" : "false")
        << ",\"nativeOptionalState\":" << (frame ? frame->values.integer("state").value_or(-1) : -1)
        << ",\"pendingMask\":" << contract_input_mask(entity.input.pending)
        << ",\"input\":";
    write_b2_input_entity(std::cout, entity, 1);
    std::cout << ",\"random\":";
    write_b2_initial_random(std::cout, world.random().state(), world.random().synchronized_table_hash());
    std::cout << ",\"resources\":{\"local\":" << (entity.input_local_resource_enabled_49d034 ? "true" : "false")
        << ",\"gate194\":" << entity.input_special_gate_194
        << ",\"modeFallback\":" << entity.input_mode_fallback_action_b8
        << ",\"lastAction\":" << entity.input_last_action_144
        << ",\"hpConsumed\":" << entity.input_hp_consumed_total
        << ",\"mpConsumed\":" << entity.input_mp_consumed_total << "}}";
}


struct CostCase {
    std::string group = "request";
    int request = 99, declared = 0, state = 12, cost = 0, hpField = 0;
    int hp = 500, mp = 500, gate = 0, caught = 0, mode = 0;
    bool local = true, facing = false, redirectDeclared = false;
    int current = 700; double vx = 0;
};
static void emit_cost(int index, const CostCase& c) {
    std::ostringstream dat;
    dat << "<bmp_begin>\nname: NativeInputActionCost\nrowing_height: -6 rowing_distance: 9\n<bmp_end>\n"
        << "<stats> caughtact: " << c.caught << " <stats_end>\n";
    dat << "<frame> " << c.current << " current\nstate: " << (c.group == "rowing" ? 3 : c.group == "builtin" ? 0 : 12)
        << " wait: 37 next: " << c.current;
    if (c.group != "rowing" && c.group != "builtin") dat << " hit_a: " << c.request;
    dat << "\n<frame_end>\n";
    if (c.declared) {
        int target = c.group == "rowing" ? (c.current == 182 ? 100 : 108) : c.group == "builtin" ? 65 : c.request == 999 || c.request == -999 ? 0 : std::abs(c.request);
        dat << "<frame> " << target << " cost\nstate: " << c.state << " wait: 19 next: 0 mp: " << c.cost << " hp: " << c.hpField << "\n<frame_end>\n";
    }
    if (c.redirectDeclared) dat << "<frame> 999 redirect\nstate: 12 wait: 29 next: 0 mp: 999 hp: 999\n<frame_end>\n";
    const auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat.str()));
    if (!definition->ok()) throw std::runtime_error("cost DAT rejected");
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 spawn;
    spawn.object_id = 77; spawn.object_type = 0; spawn.definition = definition;
    spawn.initial_action = c.current; spawn.hp = c.hp; spawn.mp = c.mp;
    if (!world.spawn_at(0, spawn).success) throw std::runtime_error("cost spawn failed");
    auto& e = *world.entity(0);
    e.current_hp = c.hp; e.current_mp = c.mp;
    e.frame.action_latch = 11; e.frame.frame_counter = 7; e.frame.facing = c.facing;
    e.motion.x = c.vx; e.motion.y = 2;
    e.input_local_resource_enabled_49d034 = c.local;
    e.input_special_gate_194 = c.gate; e.input_mode_fallback_action_b8 = c.mode;
    e.input_last_action_144 = 321; e.input_hp_consumed_total = 17; e.input_mp_consumed_total = 23;
    world.snapshot_actions();
    auto key = c.group == "rowing" ? ntsd28::InputKey28::jump : ntsd28::InputKey28::attack;
    e.input.current.set(key, true); e.input.previous = e.input.current; e.input.pending = e.input.current;
    e.input.edge_window[static_cast<std::size_t>(key)] = 5;
    std::cout << "{\"index\":" << index << ",\"group\":\"" << c.group << "\",\"params\":{\"request\":" << c.request
        << ",\"declared\":" << (c.declared ? "true" : "false") << ",\"state\":" << c.state << ",\"cost\":" << c.cost
        << ",\"hpField\":" << c.hpField << ",\"hp\":" << c.hp << ",\"mp\":" << c.mp
        << ",\"gate\":" << c.gate << ",\"caught\":" << c.caught << ",\"mode\":" << c.mode
        << ",\"local\":" << (c.local ? "true" : "false") << ",\"facing\":" << (c.facing ? "true" : "false")
        << ",\"redirectDeclared\":" << (c.redirectDeclared ? "true" : "false") << ",\"current\":" << c.current << ",\"vx\":" << c.vx << "},\"dat\":\"" << json_escape(dat.str()) << "\",\"before\":";
    write_input_state(world);
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = ntsd28::InputRouter28{}.step_sampled(e, world.random());
    std::cout << ",\"after\":"; write_input_state(world);
    std::cout << ",\"crtCalls\":"; write_b2_crt_calls(std::cout);
    std::cout << ",\"synchronizedCalls\":"; write_b2_synchronized_calls(std::cout);
    std::cout << ",\"calls\":[";
    bool first = true;
    for (const auto& call : result.actions) {
        if (!first) std::cout << ','; first = false;
        std::cout << "{\"source\":\"" << json_escape(call.source) << "\",\"requested\":" << call.requested_action
            << ",\"resolved\":" << call.resolved_action << ",\"attempted\":" << (call.attempted ? "true" : "false")
            << ",\"applied\":" << (call.applied ? "true" : "false") << ",\"mpCost\":" << call.mp_cost
            << ",\"hpCost\":" << call.hp_cost << ",\"effectiveMaxHpCost\":" << call.effective_max_hp_cost
            << ",\"message\":\"" << json_escape(call.message) << "\"}";
    }
    std::cout << "],\"diagnostics\":["; first = true;
    for (const auto& d : result.diagnostics) { if (!first) std::cout << ','; first = false; std::cout << '"' << json_escape(d) << '"'; }
    std::cout << "],\"depthIntent\":" << static_cast<int>(result.depth_intent);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(77, 0, "input-cost.dat", definition);
    ntsd28::SimulationTickOptions28 options;
    options.stage_bounds = ntsd28::StageBounds28{800, -10000, 10000};
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    const ntsd28_playable::BattleConfig28 defaults;
    options.resource_rules.selected_mode_default_hp_regen_gate_28 = defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c = defaults.selected_mode_mp_regen_gate_2c;
    options.selected_mode_weapon_drop_4c = defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
    std::cout << ",\"following\":"; write_input_state(world);
    std::cout << ",\"followingRandom\":";
    write_b2_initial_random(std::cout, world.random().state(), world.random().synchronized_table_hash());
    std::cout << ",\"followingCrtCalls\":"; write_b2_crt_calls(std::cout);
    std::cout << ",\"followingSynchronizedCalls\":"; write_b2_synchronized_calls(std::cout);
    std::cout << ",\"lifecycleSuccess\":" << (tick.lifecycle.success ? "true" : "false") << "}\n";
}
int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        for (int requested : {0,99,-99,900,-900,999,-999,1000,-1000,std::numeric_limits<int>::min()}) {
            CostCase c; c.request = requested; emit_cost(index++,c);
            if (requested != 0 && requested != std::numeric_limits<int>::min() && std::abs(requested) <= 999) {
                c.declared = 1; emit_cost(index++,c);
            }
        }
        for (int state : {1000999,2000999,1013900,2020900})
        for (int hp : {13,14}) for (int mp : {19,20}) for (bool local : {false,true}) {
            CostCase c; c.group="encoded"; c.request=900; c.declared=1; c.state=state; c.cost=1020; c.hpField=3;
            c.hp=hp;c.mp=mp;c.local=local;emit_cost(index++,c);
        }
        for (int requested : {99,900}) for (int declared : {0,1})
        for (bool destination : {false,true}) for (int state : {1000999,2000999}) {
            CostCase c;c.group="redirect_binding";c.request=requested;c.declared=declared;
            c.redirectDeclared=destination;c.state=state;c.cost=1020;c.hpField=3;
            c.mp=state==2000999?0:20;c.local=state!=2000999;emit_cost(index++,c);
        }
        for (int priority : {0,1,2}) for (int value : {999,1000}) {
            CostCase c;c.group="fallback_priority";c.request=-900;c.declared=1;c.cost=1020;c.hpField=3;c.mp=19;
            c.gate=priority==0?value:0;c.caught=priority==1?value:priority==0?901:0;c.mode=priority==2?value:902;
            emit_cost(index++,c);
        }
        for (int sign : {-1,1}) for (int fallback : {0,999,1000}) for (int priority : {0,1,2}) {
            CostCase c; c.group="fallback";c.request=sign*900;c.declared=1;c.cost=1020;c.hpField=3;c.mp=19;
            c.gate=priority==0?fallback:0;c.caught=priority<=1?fallback:0;c.mode=fallback;
            emit_cost(index++,c);
        }
        for (int current : {182,188}) for (int cost : {-7,0,7}) for (int mp : {6,7}) for (bool facing : {false,true}) {
            CostCase c;c.group="rowing";c.current=current;c.declared=1;c.cost=cost;c.mp=mp;c.facing=facing;c.vx=facing?-2:2;
            emit_cost(index++,c);
        }
        for (int current : {182,188}) for (bool facing : {false,true}) for (double vx : {-2.0,-0.5,0.0,0.5,2.0}) {
            CostCase c;c.group="rowing";c.current=current;c.facing=facing;c.vx=vx;emit_cost(index++,c);
        }
        for (int cost : {-7,0,7}) for (int mp : {0,6,7}) for (bool local : {false,true}) {
            CostCase c;c.group="builtin";c.declared=1;c.cost=cost;c.mp=mp;c.local=local;emit_cost(index++,c);
        }
        CostCase c;c.group="builtin";emit_cost(index++,c);
        return 0;
    } catch (const std::exception& e) {std::cerr << e.what() << '\n';return 91;}
}
