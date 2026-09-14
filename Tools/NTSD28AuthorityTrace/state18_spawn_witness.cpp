#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

struct State18Case {
    int previous = 18, current = 0, source = 20, present = 1, free_slots = -1;
    int delay = 0, type = 3, action = 0, declared999 = 0, pending = 0, composite = 0;
    std::uint32_t seed = 42;
};

static std::shared_ptr<const ntsd28::DatDocument> state18_dat(const std::string& text) {
    auto dat = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text));
    if (!dat->ok()) throw std::runtime_error("state18 DAT rejected");
    return dat;
}

static void run_state18(const State18Case& c, int phase, const ntsd28::ObjectDefinitionCatalog28* formal = nullptr) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: Parent weapon_hp: 17\n<bmp_end>\n"
        "<frame> 0 current\nstate: " << c.current << " wait: " << (c.composite ? 0 : 100) << " next: 0\n";
    if (c.composite) text << "opoint:\nkind: 1 oid: 777 action: 0\nopoint_end:\n";
    text << "<frame_end>\n<frame> 1 previous\nstate: " << c.previous << " wait: 100 next: 1\n<frame_end>\n";
    if (c.declared999) text << "<frame> 999 declared_terminal\nstate: " << c.current << " wait: 100 next: 0\n<frame_end>\n";
    if (c.composite) text << "<weapon_piece>\nteam: 1\npiece: 1\namount: 1 oid: 777 act: 0 framea: 0 dvx: 0 dvy: 0 dvz: 0\npiece_end:\n<weapon_piece_end>\n";
    auto parent = state18_dat(text.str());
    auto child = state18_dat("<bmp_begin>\nname: Particle weapon_hp: 17\n<bmp_end>\n"
        "<stats> ohp: 25 omp: 50 max_mp: 700 <stats_end>\n"
        "<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n"
        "<frame> 140 particle\nstate: 0 wait: 100 next: 140\n<frame_end>\n");
    int oid = c.composite ? 151 : 888;
    ntsd28::ObjectDefinitionCatalog28 catalog = formal ? *formal : ntsd28::ObjectDefinitionCatalog28{};
    if (formal) child = formal->find(999)->definition;
    catalog.upsert_definition(oid, 1, "parent.dat", parent);
    if (c.present) catalog.upsert_definition(999, c.type, "particle.dat", child);
    if (c.composite) catalog.upsert_definition(777, 3, "other-child.dat", child);
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(c.seed);
    ntsd28::SpawnRequest28 request;
    request.object_id = oid; request.object_type = 1; request.definition = parent;
    request.initial_action = 0; request.hp = 500; request.mp = 500;
    request.position.x = 100; request.position.y = -20; request.position.z = 200;
    request.owner_slot = 17; request.battle_group = 9;
    if (!world.spawn_at(c.source, request).success) throw std::runtime_error("state18 source spawn failed");
    auto* source = world.entity(c.source);
    source->frame.previous_action_078 = 1;
    source->frame.action = c.action; source->frame.facing = true;
    source->position.precise_x = 100.25; source->position.precise_y = -20.5; source->position.precise_z = 200.75;
    source->motion.x = 3.25; source->motion.y = -1.25; source->motion.z = 0.75;
    source->weapon_hp_31c = c.composite ? -1 : 17;
    source->lifecycle_resolution_pending = c.pending != 0;
    source->lifecycle_code = c.pending ? 1101 : 0;
    if (c.free_slots >= 0) {
        int remaining = c.free_slots;
        request.object_id = 4444; request.object_type = 4; request.definition = child;
        for (std::size_t slot = 50; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            if (!world.slot_available_for_spawn(slot)) continue;
            if (remaining > 0) { --remaining; continue; }
            if (!world.spawn_at(slot, request).success) throw std::runtime_error("state18 blocker failed");
        }
    }
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto before = world.random().state();
    ntsd28::WorldState18BrokenWeaponPass28 particles;
    ntsd28::WorldWeaponPiecePass28 pieces;
    std::vector<std::string> messages;
    std::vector<std::size_t> frame_slots;
    int frame_errors = 0;
    bool lifecycle_ok = true;
    if (phase == 0) particles = world.materialize_state18_broken_weapon_particles(c.source, catalog, c.delay);
    else {
        ntsd28::SimulationTickOptions28 options;
        options.global_delay_duration_4a9f6c = c.delay;
        auto result = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
        particles = std::move(result.state18_broken_weapon);
        pieces = std::move(result.weapon_pieces);
        messages = std::move(result.diagnostics.messages);
        lifecycle_ok = result.lifecycle.success;
        for (const auto& frame : result.frames) {
            frame_slots.push_back(frame.slot);
            if (!frame.frame.ok()) ++frame_errors;
        }
    }
    const auto after = world.random().state();
    std::cout << (formal ? "{\"formal\":true,\"phase\":" : "{\"phase\":") << phase << ",\"previous\":" << c.previous << ",\"current\":" << c.current
        << ",\"source\":" << c.source << ",\"present\":" << c.present << ",\"freeSlots\":" << c.free_slots
        << ",\"delay\":" << c.delay << ",\"type\":" << c.type << ",\"action\":" << c.action
        << ",\"declared999\":" << c.declared999 << ",\"pending\":" << c.pending << ",\"composite\":" << c.composite
        << ",\"seed\":" << c.seed << ",\"success\":" << (particles.success ? "true" : "false")
        << ",\"requested\":" << particles.requested << ",\"spawned\":" << particles.spawned
        << ",\"unresolved\":" << particles.unresolved << ",\"builtinSpawned\":" << pieces.builtin_spawned
        << ",\"datPiecesSpawned\":" << pieces.spawned << ",\"syncCalls\":" << after.synchronized.calls - before.synchronized.calls
        << ",\"crtCalls\":" << after.crt_calls - before.crt_calls << ",\"frameErrors\":" << frame_errors
        << ",\"lifecycleOk\":" << (lifecycle_ok ? "true" : "false") << ",\"sourceAfter\":";
    if (const auto* active = world.entity(c.source)) write_entity(std::cout, *active, 1); else std::cout << "null";
    std::cout << ",\"particleSlots\":[";
    bool first = true;
    for (const auto& event : particles.events) {
        if (!first) std::cout << ',';
        first = false;
        std::cout << event.spawned_slot;
    }
    std::cout << "],\"pieceSlots\":["; first = true;
    for (const auto& event : pieces.events) {
        if (!first) std::cout << ',';
        first = false;
        std::cout << "[" << (event.builtin_fragment ? 1 : 0) << ',' << event.object_id << ',' << event.spawned_slot << ']';
    }
    std::cout << "],\"children\":["; first = true;
    for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
        const auto* entity = world.entity(slot);
        if (!entity || slot == static_cast<std::size_t>(c.source) || entity->object_id == 4444) continue;
        if (!first) std::cout << ',';
        first = false;
        std::cout << "{\"generation\":" << entity->presentation_generation << ",\"soundLatch\":" << entity->sound_action_latch << ",\"raw\":";
        write_entity(std::cout, *entity, 1);
        std::cout << '}';
    }
    std::cout << "],\"frameSlots\":["; first = true;
    for (auto slot : frame_slots) { if (!first) std::cout << ','; first = false; std::cout << slot; }
    std::cout << "],\"calls\":["; first = true;
    for (const auto& call : direct_synchronized_calls) {
        if (!first) std::cout << ',';
        first = false;
        std::cout << '[' << call.call_site << ',' << call.upper_bound << ',' << call.result << ','
            << call.counter_after << ',' << call.index_after << ',' << call.total_calls << ']';
    }
    std::cout << "],\"messages\":["; first = true;
    for (const auto& message : messages) { if (!first) std::cout << ','; first = false; std::cout << '"' << json_escape(message) << '"'; }
    std::cout << "]}\n";
}

int wmain(int argc, wchar_t** argv) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        if (argc == 2) {
            ntsd28::ObjectDefinitionCatalog28 formal;
            auto loaded = formal.load_extracted_root(std::filesystem::path(argv[1]));
            if (!loaded.success || formal.size() != 330 || !formal.find(999)) throw std::runtime_error("formal catalog missing");
            for (int phase : {0, 1}) for (int previous : {18, 19}) for (int current : {0, 18, 19})
            for (int slot : {20, 70}) for (std::uint32_t seed : {2u, 42u}) for (int composite : {0, 1}) {
                State18Case c; c.previous = previous; c.current = current; c.source = slot;
                c.seed = seed; c.composite = composite; c.type = formal.find(999)->object_type;
                run_state18(c, phase, &formal);
            }
            return 0;
        }
        if (argc != 1) return 2;
        for (int phase : {0, 1}) {
            for (int previous : {0, 18, 19}) for (int current : {0, 18, 19})
            for (std::uint32_t seed : {1u, 42u, 682973786u}) for (int present : {0, 1})
            for (int free : {-1, 0, 1}) for (int slot : {20, 70}) {
                State18Case c; c.previous = previous; c.current = current; c.seed = seed;
                c.present = present; c.free_slots = free; c.source = slot; run_state18(c, phase);
            }
            for (int previous : {18, 19}) for (int current : {0, 18, 19}) for (int delay : {-1, 1, 3}) {
                State18Case c; c.previous = previous; c.current = current; c.delay = delay; run_state18(c, phase);
            }
            for (int type : {0, 1, 2, 3, 4, 5, 6}) for (int slot : {20, 70, 998}) {
                State18Case c; c.type = type; c.source = slot; run_state18(c, phase);
            }
            for (int action : {0, 999, 1000}) for (int declared : {0, 1}) for (int pending : {0, 1}) {
                State18Case c; c.action = action; c.declared999 = declared; c.pending = pending; run_state18(c, phase);
            }
            for (int slot : {20, 70}) for (int free : {-1, 0, 1, 3}) for (int present : {0, 1}) {
                State18Case c; c.composite = 1; c.source = slot; c.free_slots = free; c.present = present; run_state18(c, phase);
            }
            for (std::uint32_t seed = 0; seed < 32; ++seed)
            for (int present : {0, 1}) for (int free : {-1, 0, 1}) for (int slot : {20, 70}) {
                State18Case c; c.current = 18; c.seed = seed; c.present = present;
                c.free_slots = free; c.source = slot; run_state18(c, phase);
            }
        }
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
