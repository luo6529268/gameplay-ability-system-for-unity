#include "ntsd28/battle_world.h"
#include "ntsd28/dat_parser.h"
#include "ntsd28/kind_catalog.h"
#include "ntsd28/native_ai.h"
#include "ntsd28/native_random.h"
#include "ntsd28/object_catalog.h"
#include "ntsd28_playable/game_session.h"

#include <cstdint>
#include <exception>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <memory>
#include <stdexcept>
#include <string>
#include <utility>
#include <vector>

extern "C" std::uint32_t
__real__ZN6ntsd2814NativeRandom288crt_nextEv(ntsd28::NativeRandom28* random);

extern "C" std::int32_t
__real__ZN6ntsd2814NativeRandom2817synchronized_nextEji(
    ntsd28::NativeRandom28* random,
    std::uint32_t call_site,
    std::int32_t upper_bound);

extern "C" std::uint32_t
__wrap__ZN6ntsd2814NativeRandom288crt_nextEv(ntsd28::NativeRandom28* random) {
    return __real__ZN6ntsd2814NativeRandom288crt_nextEv(random);
}

extern "C" std::int32_t
__wrap__ZN6ntsd2814NativeRandom2817synchronized_nextEji(
    ntsd28::NativeRandom28* random,
    std::uint32_t call_site,
    std::int32_t upper_bound) {
    return __real__ZN6ntsd2814NativeRandom2817synchronized_nextEji(
        random, call_site, upper_bound);
}

namespace {

constexpr char capture_schema[] = "ntsd28-b0-owner-slot-exit-v1";
constexpr char formal_exe_sha256[] =
    "B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033";
constexpr std::uint32_t shared_seed = 0x00001234u;

struct Options28 {
    std::filesystem::path authority_root;
    std::filesystem::path output;
    std::string authority_source_manifest_sha256;
    std::string capture_runner_source_sha256;
    std::string capture_binary_sha256;
};

struct OwnerRecord28 {
    std::string case_id;
    int observation_tick = 0;
    int input_marker = 0;
    int slot = -1;
    int generation_ordinal = 0;
    int owner_slot = -1;
    int source_slot = -1;
    int source_owner_slot = -1;
    int target_slot_3f8 = -1;
};

void require(bool condition, const std::string& message) {
    if (!condition) throw std::runtime_error(message);
}

bool is_sha256(const std::string& value) {
    if (value.size() != 64) return false;
    for (const char character : value) {
        if (!((character >= '0' && character <= '9') ||
              (character >= 'A' && character <= 'F') ||
              (character >= 'a' && character <= 'f'))) {
            return false;
        }
    }
    return true;
}

std::string narrow_ascii(const std::wstring& value) {
    std::string result;
    result.reserve(value.size());
    for (const wchar_t character : value) {
        if (character > 0x7f) return {};
        result.push_back(static_cast<char>(character));
    }
    return result;
}

bool parse_options(int count, wchar_t** arguments, Options28& output) {
    for (int index = 1; index < count; ++index) {
        const std::wstring option(arguments[index]);
        if (index + 1 >= count) return false;
        const std::wstring value(arguments[++index]);
        if (option == L"--authority-root") {
            output.authority_root = value;
        } else if (option == L"--output") {
            output.output = value;
        } else if (option == L"--authority-source-manifest-sha256") {
            output.authority_source_manifest_sha256 = narrow_ascii(value);
        } else if (option == L"--capture-runner-source-sha256") {
            output.capture_runner_source_sha256 = narrow_ascii(value);
        } else if (option == L"--capture-binary-sha256") {
            output.capture_binary_sha256 = narrow_ascii(value);
        } else {
            return false;
        }
    }
    return !output.authority_root.empty() && !output.output.empty() &&
           is_sha256(output.authority_source_manifest_sha256) &&
           is_sha256(output.capture_runner_source_sha256) &&
           is_sha256(output.capture_binary_sha256);
}

std::shared_ptr<const ntsd28::DatDocument> parse_definition(
    const std::string& text) {
    ntsd28::DatParser parser;
    auto document = parser.parse_text(text);
    require(document.ok(), "owner-slot fixture DAT failed to parse");
    return std::make_shared<const ntsd28::DatDocument>(std::move(document));
}

std::shared_ptr<const ntsd28::DatDocument> simple_definition(
    const std::string& name,
    int state = 0,
    const std::string& extra_frames = {}) {
    return parse_definition(
        "<bmp_begin>\nname: " + name + "\n<bmp_end>\n"
        "<frame> 0 idle\npic: 0 state: " + std::to_string(state) +
        " wait: 10000 next: 0 centerx: 39 centery: 79\n"
        "<frame_end>\n" + extra_frames);
}

std::shared_ptr<const ntsd28::DatDocument> spawning_definition(
    const std::string& name,
    int child_oid,
    int state = 0) {
    return parse_definition(
        "<bmp_begin>\nname: " + name + "\n<bmp_end>\n"
        "<frame> 0 spawn\npic: 0 state: " + std::to_string(state) +
        " wait: 10000 next: 0 centerx: 39 centery: 79\n"
        "opoint: kind: 1 x: 39 y: 79 action: 0 oid: " +
        std::to_string(child_oid) +
        " facing: 0 team: 0 opoint_end:\n<frame_end>\n");
}

ntsd28::SpawnRequest28 spawn_request(
    std::shared_ptr<const ntsd28::DatDocument> definition,
    int object_id,
    int object_type,
    int owner_slot) {
    ntsd28::SpawnRequest28 request;
    request.object_id = object_id;
    request.object_type = object_type;
    request.definition = std::move(definition);
    request.initial_action = 0;
    request.hp = 500;
    request.mp = 500;
    request.owner_slot = owner_slot;
    request.position = {100, 0, 250};
    return request;
}

void append_record(std::vector<OwnerRecord28>& records,
                   std::string case_id,
                   int observation_tick,
                   int input_marker,
                   int slot,
                   int generation_ordinal,
                   const ntsd28::EntityState28& entity,
                   int source_slot,
                   int source_owner_slot) {
    records.push_back(OwnerRecord28{
        std::move(case_id),
        observation_tick,
        input_marker,
        slot,
        generation_ordinal,
        entity.owner_slot,
        source_slot,
        source_owner_slot,
        entity.object_ai_target_slot_3f8,
    });
}

ntsd28_playable::BattleConfig28 direct_config() {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = shared_seed;
    config.character_id = 99;
    config.enemy_id = 7;
    config.background_id = 23;
    config.p1_x = 560;
    config.p1_z = 650;
    config.p1_hp = 500;
    config.p1_mp = 500;
    config.p1_team = 1;
    config.p2_x = 760;
    config.p2_z = 650;
    config.p2_hp = 500;
    config.p2_mp = 500;
    config.p2_team = 2;
    config.p2_facing = true;
    return config;
}

void capture_direct_self(std::vector<OwnerRecord28>& records,
                         const std::filesystem::path& runtime_root) {
    ntsd28_playable::GameSession28 session(
        runtime_root,
        runtime_root);
    std::string error;
    require(session.initialize(direct_config(), error),
            "direct session initialization failed: " + error);
    session.step();
    for (int slot = 0; slot <= 1; ++slot) {
        const auto* entity = session.world()->entity(slot);
        require(entity != nullptr && entity->owner_slot == slot,
                "direct self owner mismatch");
        append_record(records, "direct-self", 1, 0, slot, 1, *entity,
                      slot, slot);
    }
}

void capture_two_hop_opoint(std::vector<OwnerRecord28>& records) {
    constexpr int root_slot = 50;
    constexpr int root_owner = 7;
    constexpr int first_oid = 9104;
    constexpr int second_oid = 9105;
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(
        first_oid, 0, "owner-exit-first.dat",
        spawning_definition("OwnerExitFirst", second_oid));
    catalog.upsert_definition(
        second_oid, 3, "owner-exit-second.dat",
        simple_definition("OwnerExitSecond", 3000));

    ntsd28::BattleWorld28 world;
    auto root = spawn_request(
        spawning_definition("OwnerExitRoot", first_oid), 9000, 3,
        root_owner);
    require(world.spawn_at(root_slot, root).success,
            "two-hop root spawn failed");
    const auto first = world.materialize_supported_spawns(root_slot, catalog);
    require(first.success && first.spawned == 1 && world.entity(51) != nullptr,
            "two-hop first child materialization failed");
    const auto second = world.materialize_supported_spawns(51, catalog);
    require(second.success && second.spawned == 1 && world.entity(52) != nullptr,
            "two-hop second child materialization failed");

    append_record(records, "opoint-root", 1, 0, root_slot, 1,
                  *world.entity(root_slot), root_slot, root_owner);
    append_record(records, "opoint-child-1", 1, 0, 51, 1,
                  *world.entity(51), root_slot, root_owner);
    append_record(records, "opoint-child-2", 2, 0, 52, 1,
                  *world.entity(52), 51, root_owner);
}

void capture_owner_target_deconfliction(
    std::vector<OwnerRecord28>& records,
    const std::filesystem::path& runtime_root) {
    ntsd28::ObjectDefinitionCatalog28 catalog;
    const auto loaded = catalog.load_extracted_root(runtime_root);
    const auto* target_entry = catalog.find(99);
    const auto* source_entry = catalog.find(902);
    require(loaded.success && target_entry != nullptr &&
                target_entry->definition != nullptr && source_entry != nullptr &&
                source_entry->definition != nullptr,
            "owner-target catalog fixtures are unavailable");

    ntsd28::BattleWorld28 world;
    auto target = spawn_request(
        target_entry->definition, target_entry->object_id,
        target_entry->object_type, 0);
    target.position = {700, 0, 100};
    target.battle_group = 2;
    auto source = spawn_request(
        source_entry->definition, source_entry->object_id,
        source_entry->object_type, 7);
    source.initial_action = 4;
    source.position = {400, 0, 100};
    source.battle_group = 1;
    require(world.spawn_at(0, target).success &&
                world.spawn_at(50, source).success,
            "owner-target fixtures failed to spawn");
    const auto result =
        ntsd28::NativeAi28::step_non_character_hit_fa(world, 50);
    require(result.applicable && result.common_target_path &&
                result.target_after == 0 && world.entity(50) != nullptr &&
                world.entity(50)->owner_slot == 7 &&
                world.entity(50)->object_ai_target_slot_3f8 == 0,
            "owner-target fields were not independently preserved");
    append_record(records, "owner-target-independent", 1, 0, 50, 1,
                  *world.entity(50), 50, 7);
}

void capture_f8_owner(std::vector<OwnerRecord28>& records,
                      const std::filesystem::path& runtime_root) {
    auto config = direct_config();
    config.selected_mode_drop_gate_4c = 0;
    ntsd28_playable::GameSession28 session(
        runtime_root,
        runtime_root);
    std::string error;
    require(session.initialize(config, error),
            "F8 session initialization failed: " + error);
    const auto accepted = session.submit_native_function_key(
        ntsd28_playable::NativeFunctionKeySessionCommand28::drop_mode_objects);
    require(accepted.accepted, "F8 request was rejected");
    session.step();
    const auto& drop = session.last_native_f8_drop_result();
    require(drop.consumed && drop.spawned_count > 0 && !drop.spawns.empty(),
            "F8 produced no visible spawn");
    const auto& first = drop.spawns.front();
    const auto* entity = session.world()->entity(first.slot);
    require(entity != nullptr && first.owner_slot == 99 &&
                entity->owner_slot == 99,
            "F8 first spawn owner mismatch");
    append_record(records, "f8-first-spawn", 1, 8,
                  static_cast<int>(first.slot), 1, *entity, -1, 99);
}

void capture_state9996(std::vector<OwnerRecord28>& records) {
    auto source = parse_definition(
        "<bmp_begin>\nname: OwnerExitCloneSource\n<bmp_end>\n"
        "<frame> 0 clone\npic: 30 state: 9996 wait: 1 next: 0 "
        "centerx: 39 centery: 79\n<frame_end>\n");
    auto clone = simple_definition("OwnerExitCloneTarget");
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(217, 2, "owner-exit-clone-a.dat", clone);
    catalog.upsert_definition(218, 2, "owner-exit-clone-b.dat", clone);
    ntsd28::BattleWorld28 world;
    require(world.spawn_at(0, spawn_request(source, 6, 0, 0)).success,
            "state9996 source spawn failed");
    world.entity(0)->frame.frame_counter = 1;
    world.random().reset_from_seed(shared_seed);
    const auto pass = world.materialize_special_state_clones(0, catalog);
    require(pass.success && pass.spawned == 5,
            "state9996 did not materialize five clones");
    for (int slot = 50; slot <= 54; ++slot) {
        const auto* entity = world.entity(slot);
        require(entity != nullptr && entity->owner_slot == -1,
                "state9996 clone owner mismatch");
        append_record(records, "state9996-clone", 1, 0, slot, 1,
                      *entity, 0, -1);
    }
}

void capture_type3_mutation(std::vector<OwnerRecord28>& records) {
    const auto attacker_definition = parse_definition(
        "<bmp_begin>\nname: OwnerExitType3Attacker\n<bmp_end>\n"
        "<frame> 0 attack\npic: 0 state: 3000 wait: 3 next: 0 "
        "centerx: 0 centery: 0\n"
        "itr: kind: 0 x: 0 y: 0 w: 80 h: 80 injury: 10 "
        "arest: 1 vrest: 1 itr_end:\n<frame_end>\n"
        "<frame> 40 transformed\npic: 0 state: 3000 wait: 3 next: 40\n"
        "<frame_end>\n");
    const auto target_definition = parse_definition(
        "<bmp_begin>\nname: OwnerExitType3Target\n<bmp_end>\n"
        "<frame> 0 target\npic: 0 state: 3000 wait: 3 next: 0 "
        "centerx: 0 centery: 0 hit_Uj: 25\n"
        "bdy: kind: 0 x: 0 y: 0 w: 80 h: 80 bdy_end:\n"
        "<frame_end>\n"
        "<frame> 25 generic\npic: 0 state: 3000 wait: 3 next: 25\n"
        "<frame_end>\n");
    const auto kind_catalog = ntsd28::KindCatalog28::parse_text(
        "<kind>\neffect: 209\nframe: 40\n"
        "bound: 3\nid: 8\nid: 209\nid: 213\nbound_end:\n"
        "respond: 7\nid: 200\nid: 203\nid: 205\nid: 206\nid: 207\n"
        "id: 215\nid: 216\nrespond_end:\n<kind_end>\n");
    require(kind_catalog.ok(), "type3 kind catalog failed to parse");

    ntsd28::BattleWorld28 world;
    auto attacker = spawn_request(attacker_definition, 213, 3, 7);
    auto target = spawn_request(target_definition, 206, 3, 44);
    attacker.battle_group = 4;
    target.battle_group = 9;
    attacker.position = {};
    target.position = {};
    require(world.spawn_at(50, attacker).success &&
                world.spawn_at(51, target).success,
            "type3 fixtures failed to spawn");
    world.snapshot_actions();
    require(world.rebuild_geometric_hit_candidates(0, &kind_catalog)
                    .candidates_appended == 1,
            "type3 fixture did not build one candidate");
    const auto hit = world.resolve_ordinary_unarmored_standard_hit(50, 0);
    require(hit.status == ntsd28::WorldStandardHitStatus28::applied &&
                world.entity(51) != nullptr &&
                world.entity(51)->owner_slot == 7,
            "type3 owner mutation failed");
    append_record(records, "type3-owner-mutation", 1, 0, 51, 1,
                  *world.entity(51), 50, 7);
}

void capture_slot_reuse(std::vector<OwnerRecord28>& records) {
    const auto definition = simple_definition("OwnerExitReuse");
    ntsd28::BattleWorld28 world;
    require(world.spawn_at(50, spawn_request(definition, 9300, 3, 7)).success,
            "slot reuse first spawn failed");
    const std::uint64_t first_generation =
        world.entity(50)->presentation_generation;
    append_record(records, "slot-reuse-before", 1, 0, 50, 1,
                  *world.entity(50), -1, 7);
    require(world.despawn(50).success,
            "slot reuse despawn failed");
    require(world.spawn_at(50, spawn_request(definition, 9301, 3, 13)).success,
            "slot reuse second spawn failed");
    require(world.entity(50)->presentation_generation != first_generation &&
                world.entity(50)->owner_slot == 13,
            "slot reuse generation or owner did not change");
    append_record(records, "slot-reuse-after", 2, 0, 50, 2,
                  *world.entity(50), -1, 13);
}

void write_capture(const Options28& options,
                   const std::vector<OwnerRecord28>& records) {
    std::filesystem::create_directories(options.output.parent_path());
    std::ofstream output(options.output, std::ios::binary | std::ios::trunc);
    require(output.good(), "failed to open owner-slot capture output");
    output << "{\"schema\":\"" << capture_schema
           << "\",\"producer\":\"NTSD28_PLAYABLE_SOURCE_MODEL\""
           << ",\"evidenceClass\":\"SOURCE_MODEL_DIAGNOSTIC_ONLY\""
           << ",\"formalAuthorityExeSha256\":\"" << formal_exe_sha256
           << "\",\"authoritySourceManifestSha256\":\""
           << options.authority_source_manifest_sha256
           << "\",\"captureRunnerSourceSha256\":\""
           << options.capture_runner_source_sha256
           << "\",\"captureBinarySha256\":\""
           << options.capture_binary_sha256
           << "\",\"scenarioId\":\"b0-owner-slot-production-exit\""
           << ",\"seed\":" << shared_seed
           << ",\"inputContract\":\"neutral-zero;f8-marker-8\""
           << ",\"observationUnit\":\"completed-owner-transaction\""
           << ",\"records\":[";
    for (std::size_t index = 0; index < records.size(); ++index) {
        if (index != 0) output << ',';
        const auto& record = records[index];
        output << "{\"caseId\":\"" << record.case_id
               << "\",\"observationTick\":" << record.observation_tick
               << ",\"inputMarker\":" << record.input_marker
               << ",\"slot\":" << record.slot
               << ",\"generationOrdinal\":" << record.generation_ordinal
               << ",\"ownerSlot\":" << record.owner_slot
               << ",\"sourceSlot\":" << record.source_slot
               << ",\"sourceOwnerSlot\":" << record.source_owner_slot
               << ",\"targetSlot3F8\":" << record.target_slot_3f8 << '}';
    }
    output << "]}\n";
    require(output.good(), "failed to write owner-slot capture output");
}

}  // namespace

int wmain(int count, wchar_t** arguments) {
    try {
        Options28 options;
        if (!parse_options(count, arguments, options)) {
            std::cerr
                << "usage: owner_slot_exit_capture --authority-root <path> "
                   "--output <json> --authority-source-manifest-sha256 <sha> "
                   "--capture-runner-source-sha256 <sha> "
                   "--capture-binary-sha256 <sha>\n";
            return 2;
        }
        require(std::filesystem::is_directory(options.authority_root),
                "authority root does not exist");
        const auto runtime_root =
            options.authority_root / "resources" / "runtime";
        require(std::filesystem::is_regular_file(runtime_root / "catalog.csv") &&
                    std::filesystem::is_regular_file(
                        runtime_root / "decoded_dat" / "data" / "resource.dat"),
                "authority runtime resources are incomplete");

        std::vector<OwnerRecord28> records;
        records.reserve(15);
        capture_direct_self(records, runtime_root);
        capture_two_hop_opoint(records);
        capture_owner_target_deconfliction(records, runtime_root);
        capture_f8_owner(records, runtime_root);
        capture_state9996(records);
        capture_type3_mutation(records);
        capture_slot_reuse(records);
        require(records.size() == 15, "owner-slot record count drifted");
        write_capture(options, records);
        std::cout << "owner_slot_exit_capture: PASS records="
                  << records.size() << '\n';
        return 0;
    } catch (const std::exception& exception) {
        std::cerr << "owner_slot_exit_capture: FAIL: " << exception.what()
                  << '\n';
        return 1;
    }
}
