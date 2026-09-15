#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/input_routing.h"

static void write_input_state(ntsd28::BattleWorld28& world) {
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
    std::cout << '}';
}

static void emit_input(int index, int action, bool declared, const char* state,
    bool attack, int accumulator) {
    std::ostringstream dat;
    dat << "<bmp_begin>\nname: NativeInputMissingState\n<bmp_end>\n"
        << "<frame> 0 baseline\nstate: 0 wait: 37 next: 0\n<frame_end>\n";
    if (declared) {
        dat << "<frame> " << action << " current\n";
        if (state) dat << "state: " << state << ' ';
        dat << "wait: 37 next: 0\n<frame_end>\n";
    }
    const auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat.str()));
    if (!definition->ok()) throw std::runtime_error("input DAT rejected");
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 request;
    request.object_id = 77; request.object_type = 0; request.definition = definition;
    request.initial_action = 0; request.hp = 500; request.mp = 500;
    if (!world.spawn_at(0, request).success) throw std::runtime_error("input spawn failed");
    auto& entity = *world.entity(0);
    entity.frame.action = action;
    entity.frame.action_latch = 11;
    entity.frame.frame_counter = 7;
    world.snapshot_actions();
    entity.input.current.set(ntsd28::InputKey28::attack, attack);
    entity.input.previous = entity.input.current;
    entity.input.pending = entity.input.current;
    entity.input.edge_window[static_cast<std::size_t>(ntsd28::InputKey28::attack)] = 5;
    entity.input.run_accumulator = accumulator;
    std::cout << "{\"index\":" << index << ",\"action\":" << action
        << ",\"declared\":" << (declared ? "true" : "false") << ",\"stateToken\":";
    if (state) std::cout << '"' << json_escape(state) << '"'; else std::cout << "null";
    std::cout << ",\"attack\":" << (attack ? "true" : "false")
        << ",\"runAccumulator\":" << accumulator << ",\"dat\":\"" << json_escape(dat.str())
        << "\",\"before\":";
    write_input_state(world);
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = ntsd28::InputRouter28{}.step_sampled(entity, world.random());
    std::cout << ",\"after\":";
    write_input_state(world);
    std::cout << ",\"crtCalls\":"; write_b2_crt_calls(std::cout);
    std::cout << ",\"synchronizedCalls\":"; write_b2_synchronized_calls(std::cout);
    std::cout << ",\"calls\":[";
    bool first = true;
    for (const auto& call : result.actions) {
        if (!first) std::cout << ',';
        first = false;
        std::cout << "{\"source\":\"" << json_escape(call.source) << "\",\"requested\":" << call.requested_action
            << ",\"resolved\":" << call.resolved_action << ",\"attempted\":" << (call.attempted ? "true" : "false")
            << ",\"applied\":" << (call.applied ? "true" : "false")
            << ",\"mpCost\":" << call.mp_cost << ",\"hpCost\":" << call.hp_cost
            << ",\"effectiveMaxHpCost\":" << call.effective_max_hp_cost
            << ",\"message\":\"" << json_escape(call.message) << "\"}";
    }
    std::cout << "],\"depthIntent\":" << static_cast<int>(result.depth_intent) << "}\n";
}

int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        int index = 0;
        for (int action : {99,900,999,1000,-1,110,215,182,188}) {
            for (bool attack : {false,true}) for (int accumulator : {-3,0,3}) {
                emit_input(index++, action, false, nullptr, attack, accumulator);
                if (action >= 0 && action <= 999)
                    for (const char* state : {static_cast<const char*>(nullptr),"0","4","12","85","bad","+0","1.5","2147483648"})
                        emit_input(index++, action, true, state, attack, accumulator);
            }
        }
        if (index != 432) throw std::runtime_error("input matrix count changed");
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
