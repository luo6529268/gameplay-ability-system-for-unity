#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

struct PieceCase {
    int oid = 151, type = 1, weapon_hp = -1, block = 2, targets = 2;
    int free_slots = -1, pending = 0, link = 0, facing = 1, team = 1;
    int source_slot = 20, child_type = 3, max_mp = 700, stats = 1;
    std::uint32_t seed = 42;
};

static std::shared_ptr<const ntsd28::DatDocument> parse_piece_dat(const std::string& text) {
    auto result = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text));
    if (!result->ok()) throw std::runtime_error("piece witness DAT rejected");
    return result;
}

static std::string parent_dat(const PieceCase& c) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: PieceParent\nweapon_hp: 99 weapon_broken_sound: break.wav\n<bmp_end>\n"
            "<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
    if (c.block != 0) {
        text << "<weapon_piece>\nteam: " << c.team << "\n"
                "piece: 1\namount: 2 oid: -1 act: 10 framea: 0 dvx: 0 dvy: 0 dvz: 0\npiece_end:\n";
        if (c.block == 2) {
            text << "piece: 1\noid: 777 act: 80 framea: 4 dvx: 6 dvy: -8 dvz: 4\npiece_end:\n"
                    "piece: 2\namount: 1 oid: 777 act: 109 framea: 0 dvx: 0 dvy: 0 dvz: 0\npiece_end:\n";
        }
        text << "<weapon_piece_end>\n";
    }
    return text.str();
}

static std::shared_ptr<const ntsd28::DatDocument> child_dat(const PieceCase& c) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: PieceChild\nweapon_hp: 17\n<bmp_end>\n";
    if (c.stats) {
        text << "<stats> ohp: 25 omp: 50";
        if (c.max_mp != -999) text << " max_mp: " << c.max_mp;
        text << " <stats_end>\n";
    }
    text << "<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
    return parse_piece_dat(text.str());
}

static void run_piece_case(const PieceCase& c, ntsd28::ObjectDefinitionCatalog28* real = nullptr) {
    ntsd28::ObjectDefinitionCatalog28 synthetic;
    auto parent = parse_piece_dat(parent_dat(c));
    auto child = child_dat(c);
    if (real) {
        const auto* entry = real->find(c.oid);
        if (!entry) throw std::runtime_error("real piece definition missing");
        parent = entry->definition;
    } else {
        synthetic.upsert_definition(c.oid, c.type, "parent.dat", parent);
        if (c.targets >= 1) synthetic.upsert_definition(999, c.child_type, "child.dat", child);
        if (c.targets >= 2) synthetic.upsert_definition(777, c.child_type, "variant.dat", child);
    }
    auto& catalog = real ? *real : synthetic;
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(c.seed);
    ntsd28::SpawnRequest28 request;
    request.object_id = c.oid; request.object_type = c.type; request.definition = parent;
    request.initial_action = 0; request.hp = 500; request.mp = 500;
    request.position.x = 100; request.position.y = -20; request.position.z = 200;
    request.owner_slot = 17; request.battle_group = 9;
    if (!world.spawn_at(c.source_slot, request).success) throw std::runtime_error("piece source spawn failed");
    auto* source = world.entity(c.source_slot);
    source->position.precise_x = 100.25; source->position.precise_y = -20.5; source->position.precise_z = 200.75;
    source->weapon_hp_31c = c.weapon_hp; source->frame.facing = c.facing != 0;
    source->interaction_state = c.link;
    source->lifecycle_resolution_pending = c.pending != 0;
    source->lifecycle_code = c.pending ? 1000 : 0;
    if (c.pending) source->frame.action = 1000;
    if (c.free_slots >= 0) {
        int remaining = c.free_slots;
        for (std::size_t slot = 50; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            if (!world.slot_available_for_spawn(slot)) continue;
            if (remaining > 0) { --remaining; continue; }
            ntsd28::SpawnRequest28 blocker;
            blocker.object_id = 4444; blocker.object_type = 4; blocker.definition = child;
            blocker.initial_action = 0; blocker.hp = 500; blocker.mp = 500;
            if (!world.spawn_at(slot, blocker).success) throw std::runtime_error("piece blocker spawn failed");
        }
    }
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto before = world.random().state();
    const auto pass = world.materialize_weapon_piece_fragments(c.source_slot, catalog);
    const auto after = world.random().state();
    std::cout << "{\"real\":" << (real ? "true" : "false") << ",\"input\":{"
              << "\"oid\":" << c.oid << ",\"type\":" << c.type << ",\"weaponHp\":" << c.weapon_hp
              << ",\"block\":" << c.block << ",\"targets\":" << c.targets << ",\"freeSlots\":" << c.free_slots
              << ",\"pending\":" << c.pending << ",\"link\":" << c.link << ",\"facing\":" << c.facing
              << ",\"team\":" << c.team << ",\"sourceSlot\":" << c.source_slot << ",\"childType\":" << c.child_type
              << ",\"maxMp\":" << c.max_mp << ",\"stats\":" << c.stats << ",\"seed\":" << c.seed << "},"
              << "\"summary\":{\"success\":" << (pass.success ? "true" : "false")
              << ",\"triggered\":" << (pass.break_triggered ? "true" : "false")
              << ",\"builtinRequested\":" << pass.builtin_requested << ",\"builtinSpawned\":" << pass.builtin_spawned
              << ",\"requested\":" << pass.requested << ",\"spawned\":" << pass.spawned
              << ",\"unresolved\":" << pass.unresolved << ",\"noSlot\":" << pass.skipped_no_slot
              << ",\"audioCount\":" << pass.audio_events.size() << ",\"weaponHp\":" << source->weapon_hp_31c
              << ",\"pending\":" << (source->lifecycle_resolution_pending ? "true" : "false")
              << ",\"code\":" << source->lifecycle_code << "},\"rng\":{"
              << "\"crtBefore\":" << before.crt_state << ",\"crtAfter\":" << after.crt_state
              << ",\"crtCalls\":" << after.crt_calls - before.crt_calls
              << ",\"syncCalls\":" << after.synchronized.calls - before.synchronized.calls
              << ",\"counter\":" << after.synchronized.counter << ",\"index\":" << after.synchronized.index
              << ",\"tableHash\":" << world.random().synchronized_table_hash() << "},\"calls\":[";
    bool first = true;
    for (const auto& call : direct_synchronized_calls) {
        if (!first) std::cout << ','; first = false;
        std::cout << '[' << call.call_site << ',' << call.upper_bound << ',' << call.result << ','
                  << call.counter_after << ',' << call.index_after << ',' << call.total_calls << ']';
    }
    std::cout << "],\"events\":["; first = true;
    for (const auto& event : pass.events) {
        if (!first) std::cout << ','; first = false;
        std::cout << "{\"builtin\":" << (event.builtin_fragment ? "true" : "false")
                  << ",\"piece\":" << event.piece << ",\"variant\":" << event.variant_index
                  << ",\"oid\":" << event.object_id << ",\"action\":" << event.action
                  << ",\"slot\":" << event.spawned_slot << ",\"entity\":";
        const auto* fragment = event.spawned_slot < ntsd28::EngineProfile28::maximum_slots ? world.entity(event.spawned_slot) : nullptr;
        if (fragment) write_entity(std::cout, *fragment, 1); else std::cout << "null";
        std::cout << ",\"soundLatch\":" << (fragment ? fragment->sound_action_latch : -9999)
                  << ",\"opointLatch\":" << (fragment ? fragment->opoint_action_latch : -9999) << '}';
    }
    const auto lifecycle = world.resolve_pending_lifecycle(c.source_slot);
    std::cout << "],\"afterLifecycle\":{\"sourceAlive\":" << (world.entity(c.source_slot) ? "true" : "false")
              << ",\"despawned\":" << lifecycle.despawned << ",\"active\":" << world.active_count() << "}}\n";
}

int wmain(int argc, wchar_t** argv) {
    try {
        if (argc == 2) {
            ntsd28::ObjectDefinitionCatalog28 catalog;
            auto loaded = catalog.load_extracted_root(std::filesystem::path(argv[1]));
            if (!loaded.success || catalog.size() != 330) throw std::runtime_error("formal catalog failed");
            for (const auto& entry : catalog.entries()) {
                if (!entry.second.definition->weapon_piece.has_value()) continue;
                PieceCase c; c.oid = entry.second.object_id; c.type = entry.second.object_type;
                run_piece_case(c, &catalog);
            }
            return 0;
        }
        if (argc != 1) return 2;
        for (int oid : {100,101,120,121,122,123,124,150,151,201,213,217,218,888})
        for (std::uint32_t seed : {1u,42u,682973786u}) {
            PieceCase c; c.oid = oid; c.seed = seed; c.block = 0; run_piece_case(c);
        }
        for (int oid : {151,888}) for (int block : {0,1,2})
        for (int targets : {0,1,2}) for (int free : {-1,0,2}) {
            PieceCase c; c.oid = oid; c.block = block; c.targets = targets; c.free_slots = free; run_piece_case(c);
        }
        for (int type : {0,1,2,3,4,5,6}) for (int hp : {-1,0,1}) {
            PieceCase c; c.type = type; c.weapon_hp = hp; run_piece_case(c);
        }
        for (int pending : {0,1}) for (int link : {0,-1}) for (int facing : {0,1}) {
            PieceCase c; c.pending = pending; c.link = link; c.facing = facing; c.source_slot = 70; run_piece_case(c);
        }
        for (int type : {0,1,2,3,4,5,6}) for (int mp : {-999,-1,0,700}) {
            PieceCase c; c.oid = 888; c.child_type = type; c.max_mp = mp; run_piece_case(c);
        }
        for (int stats : {0,1}) for (int team : {0,1}) {
            PieceCase c; c.stats = stats; c.team = team; run_piece_case(c);
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n'; return 91;
    }
}
