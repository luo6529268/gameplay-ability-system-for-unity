#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

struct FullFrameCase {
    int type = 0, slot = 0, action = 0, declared = 1, next = 7, y = 0;
    int hp = 100, mp = 100, dest_hp = 0, dest_mp = 0, fallback = 0;
    int recmp = 0, mode = 0, doubled = 0, waived = 0, local = 1, hold = 0;
};

static void write_frame_entity(std::ostream& output, const ntsd28::EntityState28* entity) {
    if (!entity) { output << "null"; return; }
    output << "{\"raw\":";
    write_entity(output, *entity, 1);
    output << ",\"soundLatch\":" << entity->sound_action_latch
        << ",\"hpConsumed\":" << entity->input_hp_consumed_total
        << ",\"mpConsumed\":" << entity->input_mp_consumed_total << '}';
}

static void emit_full_frame(const char* group, const FullFrameCase& c, int index) {
    std::ostringstream dat;
    dat << "<bmp_begin>\nname: FullFrameWitness\nweapon_hp: 17\n"
        << "jump_height: -9.5 jump_distance: 4.25 jump_distancez: 2.5\n<bmp_end>\n"
        << "<stats> recmp: " << c.recmp << " <stats_end>\n";
    if (c.action != 0) dat << "<frame> 0 reset\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
    if (c.declared) dat << "<frame> " << c.action << " current\nstate: 0 wait: 0 next: " << c.next
        << " sound: current.wav\n<frame_end>\n";
    dat << "<frame> 7 destination\nstate: 0 wait: 0 next: " << c.fallback
        << " hp: " << c.dest_hp << " mp: " << c.dest_mp
        << " sound: destination.wav\n<frame_end>\n";
    auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat.str()));
    if (!definition->ok()) throw std::runtime_error("full-frame DAT rejected");
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(77, c.type, "full-frame.dat", definition);
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 request;
    request.object_id = 77; request.object_type = c.type; request.definition = definition;
    request.initial_action = 0; request.hp = 500; request.mp = 500;
    request.position.x = 100; request.position.y = c.y; request.position.z = 200;
    if (!world.spawn_at(c.slot, request).success) throw std::runtime_error("full-frame spawn failed");
    auto* entity = world.entity(c.slot);
    entity->frame.action = c.action; entity->frame.action_latch = c.action;
    entity->frame.tick_action_snapshot = c.action; entity->frame.previous_action_078 = c.action;
    entity->current_hp = c.hp; entity->current_mp = c.mp; entity->effective_max_hp = 200;
    entity->input_mode_cost_multiplier_30 = c.mode;
    entity->input_double_cost = c.doubled != 0; entity->input_cost_waived = c.waived != 0;
    entity->motion_hold_timer = c.hold;
    entity->position.precise_x = 100.25; entity->position.precise_z = 200.75;
    entity->motion.x = 1.25; entity->motion.y = c.y == 0 ? 0 : -1.25; entity->motion.z = .75;
    auto resource_rules = world.native_hit_resource_rules();
    resource_rules.local_mode_enabled_49d034 = c.local != 0;
    world.set_native_hit_resource_rules(resource_rules);
    const ntsd28_playable::BattleConfig28 defaults;
    ntsd28::SimulationTickOptions28 options;
    options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28 = defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c = defaults.selected_mode_mp_regen_gate_2c;
    options.resource_rules.negative_mp_regen_enabled_49d034 = c.local != 0;
    options.selected_mode_weapon_drop_4c = defaults.selected_mode_drop_gate_4c;
    std::cout << "{\"kind\":\"case\",\"index\":" << index << ",\"group\":\"" << group << "\",\"input\":{"
        << "\"type\":" << c.type << ",\"slot\":" << c.slot << ",\"action\":" << c.action
        << ",\"declared\":" << c.declared << ",\"next\":" << c.next << ",\"y\":" << c.y
        << ",\"hp\":" << c.hp << ",\"mp\":" << c.mp << ",\"destHp\":" << c.dest_hp << ",\"destMp\":" << c.dest_mp
        << ",\"fallback\":" << c.fallback << ",\"recmp\":" << c.recmp << ",\"mode\":" << c.mode
        << ",\"doubled\":" << c.doubled << ",\"waived\":" << c.waived << ",\"local\":" << c.local
        << ",\"hold\":" << c.hold << "},\"initial\":";
    write_frame_entity(std::cout, entity);
    std::cout << ",\"ticks\":[";
    for (int tick = 1; tick <= 3; ++tick) {
        direct_crt_calls.clear(); direct_synchronized_calls.clear();
        auto result = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
        int errors = 0;
        for (const auto& event : result.frames) if (!event.frame.ok()) ++errors;
        if (tick != 1) std::cout << ',';
        std::cout << "{\"tick\":" << tick << ",\"entity\":";
        write_frame_entity(std::cout, world.entity(c.slot));
        std::cout << ",\"frameErrors\":" << errors << ",\"lifecycleOk\":" << (result.lifecycle.success ? "true" : "false")
            << ",\"audio\":[";
        bool first = true;
        for (const auto& event : result.audio_events) {
            if (!first) std::cout << ',';
            first = false;
            std::cout << "{\"source\":" << static_cast<int>(event.source) << ",\"x\":" << event.world_x
                << ",\"channel\":" << event.native_channel << ",\"path\":\"" << json_escape(event.resource_path) << "\"}";
        }
        std::cout << "],\"crtCalls\":" << direct_crt_calls.size() << ",\"calls\":["; first = true;
        for (const auto& call : direct_synchronized_calls) {
            if (!first) std::cout << ',';
            first = false;
            std::cout << '[' << call.call_site << ',' << call.upper_bound << ',' << call.result << ','
                << call.counter_after << ',' << call.index_after << ',' << call.total_calls << ']';
        }
        std::cout << "],\"messages\":["; first = true;
        for (const auto& message : result.diagnostics.messages) {
            if (!first) std::cout << ',';
            first = false;
            std::cout << '"' << json_escape(message) << '"';
        }
        std::cout << "]}";
    }
    std::cout << "]}\n";
}

int wmain(int, wchar_t**) {
    try {
        const ntsd28_playable::BattleConfig28 defaults;
        if (defaults.selected_mode_default_hp_regen_gate_28 != 1 || defaults.selected_mode_mp_regen_gate_2c != 1 ||
            defaults.selected_mode_drop_gate_4c != 2) throw std::runtime_error("formal default mode contract changed");
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        std::cout << "{\"kind\":\"header\",\"scope\":\"SOURCE_MODEL_DIAGNOSTIC_ONLY\",\"certificateEligible\":false,"
            << "\"seed\":42,\"hpMode28\":1,\"mpMode2c\":1,\"drop4c\":2,\"inputMask\":0,\"ticksPerCase\":3}\n";
        int index = 0;
        for (int next : {-999,-857,0,7,212,857,998,999,1000,1100,1101,1200,1299,1300})
        for (int type : {0,3,4}) for (int slot : {0,70}) for (int y : {0,-10}) {
            FullFrameCase c; c.next=next; c.type=type; c.slot=slot; c.y=y; emit_full_frame("next",c,index++);
        }
        for (int action : {857,998}) for (int declared : {0,1}) for (int next : {0,7})
        for (int type : {0,3,4}) for (int slot : {0,70}) {
            FullFrameCase c; c.action=action; c.declared=declared; c.next=next; c.type=type; c.slot=slot; emit_full_frame("high",c,index++);
        }
        for (int hp : {5,6}) for (int mp : {8,9}) for (int fallback : {7,857,999,-999,1101,1300})
        for (int type : {0,3,4}) for (int slot : {0,70}) {
            FullFrameCase c; c.hp=hp; c.mp=mp; c.dest_hp=-6; c.dest_mp=-9; c.fallback=fallback;
            c.type=type; c.slot=slot; emit_full_frame("cost",c,index++);
        }
        for (int recmp : {0,25,150}) for (int mode : {0,50,200})
        for (int doubled : {0,1}) for (int waived : {0,1}) for (int local : {0,1}) {
            FullFrameCase c; c.hp=10; c.mp=10; c.dest_hp=-6; c.dest_mp=-9; c.fallback=1101;
            c.recmp=recmp; c.mode=mode; c.doubled=doubled; c.waived=waived; c.local=local; emit_full_frame("modifiers",c,index++);
        }
        for (int hold : {-1,1,2}) for (int type : {0,3,4}) for (int slot : {0,70}) {
            FullFrameCase c; c.hold=hold; c.type=type; c.slot=slot; c.next=1101; emit_full_frame("hold",c,index++);
        }
        if (index != 450) throw std::runtime_error("full-frame case count changed");
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
