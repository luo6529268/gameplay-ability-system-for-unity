#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/native_ai.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_ai_entity(const ntsd28::EntityState28* e) {
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
        << ",\"pendingCount\":" << e->pending_hit_impulse.contribution_count << ",\"pendingX\":" << e->pending_hit_impulse.total.x << ",\"pendingY\":" << e->pending_hit_impulse.total.y
        << ",\"pendingZ\":" << e->pending_hit_impulse.total.z
        << ",\"alias\":" << e->ai_profile_object_id
        << ",\"level618\":" << e->ai_level_scaled_618 << ",\"level61c\":" << e->ai_level_scaled_61c
        << ",\"pending\":[";
    for (int i = 0; i < 7; ++i) {
        if (i != 0) std::cout << ',';
        std::cout << (e->input.pending[static_cast<ntsd28::InputKey28>(i)] ? 1 : 0);
    }
    std::cout << "],\"input\":"; write_b2_input_entity(std::cout,*e,1); std::cout << '}';
}
static void write_ai_state(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":["; bool first = true;
    for (int slot : {0,1}) { if (!first) std::cout << ','; first = false; write_ai_entity(world.entity(slot)); }
    std::cout << "],\"random\":";write_b2_initial_random(std::cout,world.random().state(),world.random().synchronized_table_hash());std::cout << '}';
}
static void write_ai_calls() {
    std::cout << "{\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"synchronized\":";write_b2_synchronized_calls(std::cout);std::cout << '}';
}



struct AiCase {
    const char* name;
    const char* api;
    int actual;
    int alias;
    int distance = 150;
    int target_state = 0;
    int level618 = 0;
};
static void write_profile(const ntsd28::NativeAiProfiledCombatStep28& r) {
    std::cout << "{\"classified\":" << r.classified_ai_id;
#define FLAG(field) std::cout << ",\"" #field "\":" << (r.field ? "true" : "false")
    FLAG(applicable); FLAG(rng38_consumed); FLAG(rng38_rejected); FLAG(special_family);
    FLAG(family_chase_pressed); FLAG(use_ai_one_defend_pressed); FLAG(object_one_chase_pressed);
    FLAG(pressed_attack); FLAG(pressed_jump); FLAG(pressed_defend); FLAG(pressed_left); FLAG(pressed_right);
#undef FLAG
    std::cout << '}';
}
static void write_special(const ntsd28::NativeAiSpecialProfileStep28& r) {
    std::cout << "{\"returnValue\":" << r.native_return_value << ",\"rng6cValue\":" << r.rng6c_value;
#define FLAG(field) std::cout << ",\"" #field "\":" << (r.field ? "true" : "false")
    FLAG(applicable); FLAG(rng3c_consumed); FLAG(rejected_by_rng_gate); FLAG(oid33_path);
    FLAG(rng6c_consumed); FLAG(custom_profile_present); FLAG(unsupported_custom_profile); FLAG(locked_corpus_path);
#undef FLAG
    std::cout << '}';
}
static void write_main(const ntsd28::NativeAiMainStep28& r) {
    std::cout << "{\"applicable\":" << (r.applicable ? "true" : "false")
        << ",\"sampledPreviousPending\":" << (r.sampled_previous_pending ? "true" : "false")
        << ",\"unsupported\":" << (r.unsupported_custom_profile ? "true" : "false")
        << ",\"selectedSlot\":" << r.selected_slot << ",\"special\":";
    write_special(r.ordinary_combat.special_profile);
    std::cout << ",\"profile\":";
    write_profile(r.ordinary_combat.profiled_combat);
    std::cout << '}';
}
static void emit(int index, const AiCase& c) {
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;
    std::array<std::string, 2> dat;
    for (int slot : {0, 1}) {
        std::ostringstream text;
        text << "<bmp_begin>\nname: SyntheticAiAlias\n<bmp_end>\n<frame> 110 active\nstate: "
            << (slot == 0 ? 0 : c.target_state) << " wait: 100 next: 110\n<frame_end>\n";
        dat[slot] = text.str();
        auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat[slot]));
        if (!definition->ok()) throw std::runtime_error("AI DAT parse failed");
        const int oid = slot == 0 ? c.actual : 31981;
        catalog.upsert_definition(oid, 0, "ai-alias.dat", definition);
        ntsd28::SpawnRequest28 request;
        request.object_id = oid;
        request.object_type = 0;
        request.definition = definition;
        request.initial_action = 110;
        request.hp = 400;
        request.mp = 300;
        request.battle_group = slot + 1;
        request.position.x = 300 + slot * c.distance;
        request.position.z = 250;
        if (!world.spawn_at(slot, request).success) throw std::runtime_error("AI spawn failed");
    }
    auto& subject = *world.entity(0);
    subject.ai_profile_object_id = c.alias;
    subject.ai_level_scaled_618 = c.level618;
    subject.ai_level_scaled_61c = 0;
    subject.input.pending.set(ntsd28::InputKey28::depth_up);
    world.snapshot_actions();
    std::cout << "{\"index\":" << index << ",\"name\":\"" << c.name << "\",\"synthetic\":true,\"params\":{\"api\":\"" << c.api
        << "\",\"actual\":" << c.actual << ",\"alias\":" << c.alias << ",\"distance\":" << c.distance
        << ",\"targetState\":" << c.target_state << ",\"level618\":" << c.level618
        << ",\"level61c\":0,\"action\":110,\"mp\":300,\"seed\":42},\"dat\":[\"" << json_escape(dat[0]) << "\",\"" << json_escape(dat[1]) << "\"],\"before\":";
    write_ai_state(world);
    direct_crt_calls.clear();
    direct_synchronized_calls.clear();
    std::cout << ",\"result\":";
    const std::string api = c.api;
    ntsd28::NativeAiMainContext28 context;
    context.stage_bounds = ntsd28::StageBounds28{800,180,350};
    if (api == "profile") write_profile(ntsd28::NativeAi28::step_profiled_combat(world, 0, 1, 0, 0));
    else if (api == "special") write_special(ntsd28::NativeAi28::step_special_profile(world, 0, 1));
    else if (api == "ordinary") {
        ntsd28::NativeAiOrdinaryCombatContext28 ordinary_context;
        ordinary_context.stage_bounds = context.stage_bounds;
        const auto result = ntsd28::NativeAi28::step_ordinary_combat(world, 0, 1, ordinary_context);
        std::cout << "{\"stopsOuter\":" << (result.stops_outer_ai ? "true" : "false") << ",\"special\":";
        write_special(result.special_profile);
        std::cout << ",\"profile\":";
        write_profile(result.profiled_combat);
        std::cout << '}';
    }
    else if (api == "main") write_main(ntsd28::NativeAi28::step_main(world, 0, context));
    else {
        ntsd28::SimulationTickOptions28 options;
        options.stage_bounds = context.stage_bounds;
        options.native_ai = context;
        options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
        options.controls[0].native_ai = true;
        const auto tick = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
        write_main(tick.native_ai[0]);
    }
    std::cout << ",\"after\":";
    write_ai_state(world);
    std::cout << ",\"calls\":";
    write_ai_calls();
    if (api == "tick") {
        direct_crt_calls.clear();
        direct_synchronized_calls.clear();
        ntsd28::SimulationTickOptions28 options;
        options.stage_bounds = context.stage_bounds;
        options.native_ai = context;
        options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
        options.controls[0].native_ai = true;
        const auto later = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
        std::cout << ",\"laterResult\":";
        write_main(later.native_ai[0]);
        std::cout << ",\"later\":";
        write_ai_state(world);
        std::cout << ",\"laterCalls\":";
        write_ai_calls();
        std::cout << ",\"laterLifecycleSuccess\":" << (later.lifecycle.success ? "true" : "false");
    }
    std::cout << "}\n";
}
int wmain() {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        const AiCase cases[] = {
            {"recognized34", "profile", 77, 34},
            {"fallbackPositive", "profile", 77, 123},
            {"actual34Negative", "profile", 34, -1},
            {"actual34Zero", "profile", 34, 0},
            {"alias1NotActual1", "profile", 77, 1},
            {"actual1Zero", "profile", 1, 0},
            {"actual1Negative", "profile", 1, -1},
            {"specialAlias33", "special", 77, 33, 30, 16},
            {"specialActual33Zero", "special", 33, 0, 30, 16},
            {"specialActual33Negative", "special", 33, -1, 30, 16},
            {"specialZeroContinue", "special", 77, 0},
            {"specialPositiveStop", "special", 77, 123},
            {"specialNegativePositiveGate", "special", 77, -1, 150, 0, 100},
            {"ordinaryPositiveGate", "ordinary", 77, -1, 150, 0, 100},
            {"mainNegativeStop", "main", 77, -1},
            {"mainZeroContinue", "main", 77, 0},
            {"tickNegativeStop", "tick", 77, -1},
            {"tickZeroContinue", "tick", 77, 0}
        };
        int index = 0;
        for (const auto& c : cases) emit(index++, c);
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 91;
    }
}
