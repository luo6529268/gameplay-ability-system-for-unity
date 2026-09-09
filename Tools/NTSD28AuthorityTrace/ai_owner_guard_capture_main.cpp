#include "ntsd28/battle_world.h"
#include "ntsd28/dat_parser.h"
#include "ntsd28/native_ai.h"
#include "ntsd28/native_random.h"

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

constexpr char capture_schema[] = "ntsd28-b2-ai-owner-guard-v1";
constexpr char formal_exe_sha256[] =
    "B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033";
constexpr std::uint32_t shared_seed = 0x00001234u;

struct Options28 {
    std::filesystem::path output;
    std::string authority_source_manifest_sha256;
    std::string capture_runner_source_sha256;
    std::string capture_binary_sha256;
};

struct GuardRecord28 {
    int object_id = 0;
    int owner_slot = -1;
    int legacy_kill_count_marker = -1;
    int selected_slot = -1;
    bool guard_7a = false;
    bool guard_7b = false;
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
        if (option == L"--output") {
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
    return !output.output.empty() &&
           is_sha256(output.authority_source_manifest_sha256) &&
           is_sha256(output.capture_runner_source_sha256) &&
           is_sha256(output.capture_binary_sha256);
}

std::shared_ptr<const ntsd28::DatDocument> parse_definition(
    const std::string& name,
    int state) {
    ntsd28::DatParser parser;
    auto document = parser.parse_text(
        "<bmp_begin>\nname: " + name + "\n<bmp_end>\n"
        "<frame> 0 active\npic: 0 state: " + std::to_string(state) +
        " wait: 10000 next: 0 centerx: 39 centery: 79\n"
        "<frame_end>\n");
    require(document.ok(), "AI owner guard DAT failed to parse");
    return std::make_shared<const ntsd28::DatDocument>(std::move(document));
}

ntsd28::SpawnRequest28 spawn_request(
    std::shared_ptr<const ntsd28::DatDocument> definition,
    int object_id,
    int object_type,
    int owner_slot,
    int hp,
    int mp,
    int team,
    int x) {
    ntsd28::SpawnRequest28 request;
    request.object_id = object_id;
    request.object_type = object_type;
    request.definition = std::move(definition);
    request.initial_action = 0;
    request.owner_slot = owner_slot;
    request.hp = hp;
    request.mp = mp;
    request.battle_group = team;
    request.position = {x, 0, 0};
    return request;
}

GuardRecord28 run_case(int object_id,
                       int owner_slot,
                       int legacy_kill_count_marker) {
    ntsd28::BattleWorld28 world;
    const auto character = parse_definition("AiOwnerCharacter", 0);
    const auto pickup = parse_definition("AiOwnerPickup", 1000);
    require(world.spawn_at(
                    0,
                    spawn_request(
                        character, 9510, 0, owner_slot, 100, 100, 1, 0))
                    .success &&
                world.spawn_at(
                    1,
                    spawn_request(
                        character, 9511, 0, 1, 500, 100, 2, 80))
                    .success &&
                world.spawn_at(
                    20,
                    spawn_request(
                        pickup, object_id, 1, -1, 1, 100, 1, 10))
                    .success,
            "AI owner guard fixtures failed to spawn");
    auto* subject = world.entity(0);
    subject->current_hp = 100;
    subject->effective_max_hp = 500;
    subject->base_max_hp = 500;
    subject->current_mp = 100;

    const auto result = ntsd28::NativeAi28::scan_threats_and_pickups(
        world, 0, 1, 100, false, 0);
    require(result.applicable,
            "AI owner guard scan was not applicable");
    const bool expected_guard = owner_slot >= 0;
    const int expected_selected = expected_guard ? 1 : 20;
    require(result.selected_slot == expected_selected &&
                result.avoid_weapon_122 == expected_guard &&
                result.avoid_weapon_123 == expected_guard,
            "AI owner guard result mismatch");
    return GuardRecord28{
        object_id,
        owner_slot,
        legacy_kill_count_marker,
        result.selected_slot,
        result.avoid_weapon_122,
        result.avoid_weapon_123,
    };
}

void write_capture(const Options28& options,
                   const std::vector<GuardRecord28>& records) {
    std::filesystem::create_directories(options.output.parent_path());
    std::ofstream output(options.output, std::ios::binary | std::ios::trunc);
    require(output.good(), "failed to open AI owner guard output");
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
           << "\",\"scenarioId\":\"b2-ai-owner-slot-guard\""
           << ",\"seed\":" << shared_seed
           << ",\"precondition\":\"hp100/base500/mp100/mode0\""
           << ",\"records\":[";
    for (std::size_t index = 0; index < records.size(); ++index) {
        if (index != 0) output << ',';
        const auto& record = records[index];
        output << "{\"objectId\":" << record.object_id
               << ",\"ownerSlot\":" << record.owner_slot
               << ",\"legacyKillCountMarker\":"
               << record.legacy_kill_count_marker
               << ",\"selectedSlot\":" << record.selected_slot
               << ",\"guard7A\":"
               << (record.guard_7a ? "true" : "false")
               << ",\"guard7B\":"
               << (record.guard_7b ? "true" : "false") << '}';
    }
    output << "]}\n";
    require(output.good(), "failed to write AI owner guard output");
}

}  // namespace

int wmain(int count, wchar_t** arguments) {
    try {
        Options28 options;
        if (!parse_options(count, arguments, options)) {
            std::cerr
                << "usage: ai_owner_guard_capture --output <json> "
                   "--authority-source-manifest-sha256 <sha> "
                   "--capture-runner-source-sha256 <sha> "
                   "--capture-binary-sha256 <sha>\n";
            return 2;
        }

        std::vector<GuardRecord28> records;
        records.reserve(8);
        for (const int object_id : {122, 123}) {
            records.push_back(run_case(object_id, -1, -1));
            records.push_back(run_case(object_id, -1, 999));
            records.push_back(run_case(object_id, 0, -1));
            records.push_back(run_case(object_id, 37, 999));
        }
        require(records.size() == 8, "AI owner guard record count drifted");
        write_capture(options, records);
        std::cout << "ai_owner_guard_capture: PASS records="
                  << records.size() << '\n';
        return 0;
    } catch (const std::exception& exception) {
        std::cerr << "ai_owner_guard_capture: FAIL: " << exception.what()
                  << '\n';
        return 1;
    }
}
