#include "ntsd28/battle_world.h"
#include "ntsd28/dat_parser.h"
#include "ntsd28/native_random.h"

#include <cmath>
#include <cstdint>
#include <exception>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <memory>
#include <optional>
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

constexpr char capture_schema[] = "ntsd28-b4-revival-exit-v1";
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

struct RevivalRecord28 {
    std::string case_id;
    int observation_tick = 0;
    int slot = -1;
    int generation_ordinal = 0;
    int active = 0;
    int oid = -1;
    int action = -1;
    int frame_counter = 0;
    int motion_hold = 0;
    int current_hp = 0;
    int effective_max_hp = 0;
    int base_max_hp = 0;
    int current_mp = 0;
    int lives = 0;
    int next_lives = 0;
    int next_hp = 0;
    int battle_group = 0;
    int controller_slot = -1;
    int render_phase = 0;
    int visual_runtime_180 = 0;
    int visual_id_184 = 0;
    int visual_runtime_318 = 0;
    int x = 0;
    int y = 0;
    int z = 0;
    std::int64_t precise_x_milli = 0;
    std::int64_t precise_y_milli = 0;
    std::int64_t precise_z_milli = 0;
    std::int64_t motion_y_milli = 0;
    std::uint64_t synchronized_calls = 0;
    std::uint32_t last_synchronized_call_site = 0;
    std::uint64_t crt_calls = 0;
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

std::shared_ptr<const ntsd28::DatDocument> revival_definition(
    const std::string& name) {
    ntsd28::DatParser parser;
    auto document = parser.parse_text(
        "<bmp_begin>\nname: " + name + "\n<bmp_end>\n"
        "<frame> 0 idle\npic: 0 state: 0 wait: 10000 next: 0 "
        "centerx: 39 centery: 79\n<frame_end>\n"
        "<frame> 14 lying\npic: 0 state: 14 wait: 10000 next: 14 "
        "centerx: 39 centery: 79\n<frame_end>\n"
        "<frame> 212 revive\npic: 0 state: 4 wait: 10000 next: 212 "
        "centerx: 39 centery: 79\n<frame_end>\n"
        "<frame> 219 continue\npic: 0 state: 0 wait: 10000 next: 219 "
        "centerx: 39 centery: 79\n<frame_end>\n");
    require(document.ok(), "revival fixture DAT failed to parse");
    return std::make_shared<const ntsd28::DatDocument>(std::move(document));
}

ntsd28::SpawnRequest28 spawn_request(
    const std::shared_ptr<const ntsd28::DatDocument>& definition,
    int object_id,
    int slot,
    int action = 0,
    int hp = 500,
    int mp = 500,
    int group = 0) {
    ntsd28::SpawnRequest28 request;
    request.object_id = object_id;
    request.object_type = 0;
    request.definition = definition;
    request.initial_action = action;
    request.position = {100 + slot, 0, 250 + slot};
    request.hp = hp;
    request.mp = mp;
    request.owner_slot = slot;
    request.battle_group = group;
    return request;
}

void configure_dead(ntsd28::EntityState28& entity,
                    int lives,
                    int next_lives,
                    int next_hp,
                    int render_phase,
                    int group,
                    int controller_slot,
                    int visual_id,
                    int frame_counter = 6,
                    int motion_hold = 3) {
    entity.current_hp = 0;
    entity.effective_max_hp = 10;
    entity.base_max_hp = 180;
    entity.current_mp = 77;
    entity.revive_lives_30c = lives;
    entity.revive_next_lives_310 = next_lives;
    entity.revive_next_hp_314 = next_hp;
    entity.battle_group = group;
    entity.ai_target_slot_360 = controller_slot;
    entity.render_phase_008 = render_phase;
    entity.revive_visual_id_184 = visual_id;
    entity.revive_visual_runtime_180 = 9;
    entity.revive_visual_runtime_318 = 8;
    entity.frame.frame_counter = frame_counter;
    entity.motion_hold_timer = motion_hold;
}

std::int64_t milli(double value) {
    return static_cast<std::int64_t>(std::llround(value * 1000.0));
}

void append_record(std::vector<RevivalRecord28>& records,
                   std::string case_id,
                   int observation_tick,
                   int slot,
                   int generation_ordinal,
                   const ntsd28::BattleWorld28& world) {
    RevivalRecord28 record;
    record.case_id = std::move(case_id);
    record.observation_tick = observation_tick;
    record.slot = slot;
    record.generation_ordinal = generation_ordinal;
    const auto& random = world.random().state();
    record.synchronized_calls = random.synchronized.calls;
    record.last_synchronized_call_site = random.synchronized.last_call_site;
    record.crt_calls = random.crt_calls;
    const auto* entity = slot >= 0 ? world.entity(static_cast<std::size_t>(slot))
                                   : nullptr;
    if (entity == nullptr) {
        records.push_back(std::move(record));
        return;
    }

    record.active = 1;
    record.oid = entity->object_id;
    record.action = entity->frame.action;
    record.frame_counter = entity->frame.frame_counter;
    record.motion_hold = entity->motion_hold_timer;
    record.current_hp = entity->current_hp;
    record.effective_max_hp = entity->effective_max_hp;
    record.base_max_hp = entity->base_max_hp;
    record.current_mp = entity->current_mp;
    record.lives = entity->revive_lives_30c;
    record.next_lives = entity->revive_next_lives_310;
    record.next_hp = entity->revive_next_hp_314;
    record.battle_group = entity->battle_group;
    record.controller_slot = entity->ai_target_slot_360;
    record.render_phase = entity->render_phase_008;
    record.visual_runtime_180 = entity->revive_visual_runtime_180;
    record.visual_id_184 = entity->revive_visual_id_184;
    record.visual_runtime_318 = entity->revive_visual_runtime_318;
    record.x = entity->position.x;
    record.y = entity->position.y;
    record.z = entity->position.z;
    record.precise_x_milli = milli(entity->position.precise_x);
    record.precise_y_milli = milli(entity->position.precise_y);
    record.precise_z_milli = milli(entity->position.precise_z);
    record.motion_y_milli = milli(entity->motion.y);
    records.push_back(std::move(record));
}

void seed_world(ntsd28::BattleWorld28& world) {
    world.random().reset_from_seed(shared_seed);
}

void capture_direct_defaults(
    std::vector<RevivalRecord28>& records,
    const std::shared_ptr<const ntsd28::DatDocument>& definition) {
    ntsd28::BattleWorld28 world;
    seed_world(world);
    require(world.spawn_at(0, spawn_request(definition, 9000, 0)).success,
            "direct default fixture failed to spawn");
    append_record(records, "direct-defaults", 0, 0, 1, world);
}

void capture_c25_cases(
    std::vector<RevivalRecord28>& records,
    const std::shared_ptr<const ntsd28::DatDocument>& definition) {
    struct Case {
        const char* id;
        int slot;
        int lives;
    };
    constexpr Case cases[] = {
        {"c25-primary-lives2", 0, 2},
        {"c25-primary-lives1", 0, 1},
        {"c25-transient-lives2", 20, 2},
    };
    for (const auto& test : cases) {
        ntsd28::BattleWorld28 world;
        seed_world(world);
        require(world.spawn_at(
                    static_cast<std::size_t>(test.slot),
                    spawn_request(definition, 9010 + test.slot + test.lives,
                                  test.slot, 14, 0, 77, 5)).success,
                std::string(test.id) + " fixture failed to spawn");
        auto* entity = world.entity(static_cast<std::size_t>(test.slot));
        configure_dead(*entity, test.lives, 0, 0, 1, 5, -1, 0, 0, 0);
        (void)world.step_frames();
        world.advance_reaction_timers();
        append_record(records, test.id, 25, test.slot, 1, world);
    }
}

void capture_queued_cases(
    std::vector<RevivalRecord28>& records,
    const std::shared_ptr<const ntsd28::DatDocument>& definition) {
    {
        ntsd28::BattleWorld28 world;
        seed_world(world);
        require(world.spawn_at(2, spawn_request(definition, 9022, 2, 0,
                                                500, 500, 7)).success &&
                    world.spawn_at(0, spawn_request(definition, 30, 0, 14,
                                                    0, 77, 5)).success,
                "queued explicit fixtures failed to spawn");
        configure_dead(*world.entity(0), 1, 4, 180, 2, 5, 2, 0);
        const auto pass = world.advance_native_revivals({});
        require(pass.continuations == 1 && pass.deferred == 0,
                "queued explicit branch did not continue");
        append_record(records, "queued-explicit", 7, 0, 1, world);
    }
    {
        ntsd28::BattleWorld28 world;
        seed_world(world);
        require(world.spawn_at(0, spawn_request(definition, 37, 0, 14,
                                                0, 77, 5)).success,
                "queued missing fixture failed to spawn");
        configure_dead(*world.entity(0), 1, 4, 180, 2, 5, 77, 0);
        const auto pass = world.advance_native_revivals({});
        require(pass.continuations == 0 && pass.deferred == 1 && !pass.success,
                "queued missing controller did not defer");
        append_record(records, "queued-missing", 7, 0, 1, world);
    }
    {
        ntsd28::BattleWorld28 world;
        seed_world(world);
        require(world.spawn_at(0, spawn_request(definition, 37, 0, 14,
                                                0, 77, 5)).success,
                "queued fallback fixture failed to spawn");
        configure_dead(*world.entity(0), 1, 4, 180, 2, 5, -1, 0);
        const auto pass = world.advance_native_revivals({});
        require(pass.continuations == 1 && pass.deferred == 0,
                "queued inactive slot-one fallback did not continue");
        append_record(records, "queued-fallback-inactive", 7, 0, 1, world);
    }
}

void capture_terminal_and_reuse(
    std::vector<RevivalRecord28>& records,
    const std::shared_ptr<const ntsd28::DatDocument>& definition) {
    {
        ntsd28::BattleWorld28 world;
        seed_world(world);
        require(world.spawn_at(19, spawn_request(definition, 9049, 19, 14,
                                                 0, 77, 5)).success,
                "terminal primary fixture failed to spawn");
        configure_dead(*world.entity(19), 1, 0, 0, 2, 5, -1, 0);
        const auto pass = world.advance_native_revivals({});
        require(pass.deferred == 1 && world.entity(19) != nullptr,
                "terminal primary was not retained");
        append_record(records, "terminal-primary", 7, 19, 1, world);
    }
    {
        ntsd28::BattleWorld28 world;
        seed_world(world);
        require(world.spawn_at(20, spawn_request(definition, 9050, 20, 14,
                                                 0, 77, 5)).success,
                "terminal transient fixture failed to spawn");
        configure_dead(*world.entity(20), 1, 0, 0, 2, 5, -1, 0);
        const auto pass = world.advance_native_revivals({});
        require(pass.removed == 1 && world.entity(20) == nullptr,
                "terminal transient was not removed");
        append_record(records, "terminal-transient-removed", 7, 20, 1, world);
        require(world.spawn_at(20, spawn_request(definition, 9099, 20)).success,
                "removed transient slot was not reusable");
        // This reused slot models an ordinary OPoint child with explicit
        // reserve=0, not a direct physical participant initializer.
        world.entity(20)->revive_lives_30c = 0;
        append_record(records, "terminal-slot-reused", 8, 20, 2, world);
    }
}

void capture_normal_cases(
    std::vector<RevivalRecord28>& records,
    const std::shared_ptr<const ntsd28::DatDocument>& definition) {
    {
        ntsd28::BattleWorld28 world;
        seed_world(world);
        require(world.spawn_at(0, spawn_request(definition, 9060, 0, 14,
                                                0, 77, 5)).success &&
                    world.spawn_at(1, spawn_request(definition, 9061, 1, 0,
                                                    500, 500, 5)).success,
                "normal nonzero fixtures failed to spawn");
        auto* dead = world.entity(0);
        auto* peer = world.entity(1);
        configure_dead(*dead, 2, 4, 80, 2, 5, 22, 7);
        dead->position = {111, 8, 333};
        dead->position.precise_x = 123.0;
        dead->position.precise_y = 8.0;
        dead->position.precise_z = 345.0;
        dead->motion.y = 9.0;
        peer->position = {200, 0, 300};
        peer->position.precise_x = 200.0;
        peer->position.precise_z = 300.0;
        // The peer is a non-participant type-0 fixture whose OPoint reserve
        // input is zero; only its group and integer X/Z enter revival.
        peer->revive_lives_30c = 0;
        std::vector<std::optional<int>> floors(1000);
        floors[0] = -25;
        const auto pass = world.advance_native_revivals(floors);
        require(pass.revived == 1, "normal nonzero branch did not revive");
        append_record(records, "normal-nonzero", 7, 0, 1, world);
        append_record(records, "normal-peer-preserved", 7, 1, 1, world);
    }
    {
        ntsd28::BattleWorld28 world;
        seed_world(world);
        require(world.spawn_at(0, spawn_request(definition, 9070, 0, 14,
                                                0, 77, 5)).success &&
                    world.spawn_at(1, spawn_request(definition, 9071, 1, 0,
                                                    500, 500, 5)).success &&
                    world.spawn_at(2, spawn_request(definition, 9072, 2, 0,
                                                    500, 500, 5)).success,
                "normal zero-sum fixtures failed to spawn");
        auto* dead = world.entity(0);
        configure_dead(*dead, 2, 4, 80, 2, 5, 22, 7);
        dead->position = {111, 8, 333};
        dead->position.precise_x = 123.0;
        dead->position.precise_y = 8.0;
        dead->position.precise_z = 345.0;
        dead->motion.y = 9.0;
        world.entity(1)->position = {-50, 0, 100};
        world.entity(2)->position = {50, 0, 200};
        std::vector<std::optional<int>> floors(1000);
        floors[0] = 0;
        const auto pass = world.advance_native_revivals(floors);
        require(pass.revived == 1, "normal zero-sum branch did not revive");
        append_record(records, "normal-sumx-zero", 7, 0, 1, world);
    }
}

void write_capture(const Options28& options,
                   const std::vector<RevivalRecord28>& records) {
    std::filesystem::create_directories(options.output.parent_path());
    std::ofstream output(options.output, std::ios::binary | std::ios::trunc);
    require(output.good(), "failed to open revival capture output");
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
           << "\",\"scenarioId\":\"b4-revival-production-exit\""
           << ",\"seed\":" << shared_seed
           << ",\"inputContract\":\"neutral-zero\""
           << ",\"observationUnit\":\"completed-revival-route\""
           << ",\"records\":[";
    for (std::size_t index = 0; index < records.size(); ++index) {
        if (index != 0) output << ',';
        const auto& record = records[index];
        output << "{\"caseId\":\"" << record.case_id
               << "\",\"observationTick\":" << record.observation_tick
               << ",\"slot\":" << record.slot
               << ",\"generationOrdinal\":" << record.generation_ordinal
               << ",\"active\":" << record.active
               << ",\"oid\":" << record.oid
               << ",\"action\":" << record.action
               << ",\"frameCounter\":" << record.frame_counter
               << ",\"motionHold\":" << record.motion_hold
               << ",\"currentHp\":" << record.current_hp
               << ",\"effectiveMaxHp\":" << record.effective_max_hp
               << ",\"baseMaxHp\":" << record.base_max_hp
               << ",\"currentMp\":" << record.current_mp
               << ",\"lives\":" << record.lives
               << ",\"nextLives\":" << record.next_lives
               << ",\"nextHp\":" << record.next_hp
               << ",\"battleGroup\":" << record.battle_group
               << ",\"controllerSlot\":" << record.controller_slot
               << ",\"renderPhase\":" << record.render_phase
               << ",\"visualRuntime180\":" << record.visual_runtime_180
               << ",\"visualId184\":" << record.visual_id_184
               << ",\"visualRuntime318\":" << record.visual_runtime_318
               << ",\"x\":" << record.x
               << ",\"y\":" << record.y
               << ",\"z\":" << record.z
               << ",\"preciseXMilli\":" << record.precise_x_milli
               << ",\"preciseYMilli\":" << record.precise_y_milli
               << ",\"preciseZMilli\":" << record.precise_z_milli
               << ",\"motionYMilli\":" << record.motion_y_milli
               << ",\"synchronizedCalls\":" << record.synchronized_calls
               << ",\"lastSynchronizedCallSite\":"
               << record.last_synchronized_call_site
               << ",\"crtCalls\":" << record.crt_calls << '}';
    }
    output << "]}\n";
    require(output.good(), "failed to write revival capture output");
}

}  // namespace

int wmain(int count, wchar_t** arguments) {
    try {
        Options28 options;
        if (!parse_options(count, arguments, options)) {
            std::cerr
                << "usage: revival_exit_capture --authority-root <path> "
                   "--output <json> --authority-source-manifest-sha256 <sha> "
                   "--capture-runner-source-sha256 <sha> "
                   "--capture-binary-sha256 <sha>\n";
            return 2;
        }
        require(std::filesystem::is_directory(options.authority_root),
                "authority root does not exist");

        const auto definition = revival_definition("RevivalExitFixture");
        std::vector<RevivalRecord28> records;
        records.reserve(13);
        capture_direct_defaults(records, definition);
        capture_c25_cases(records, definition);
        capture_queued_cases(records, definition);
        capture_terminal_and_reuse(records, definition);
        capture_normal_cases(records, definition);
        require(records.size() == 13, "revival record count drifted");
        write_capture(options, records);
        std::cout << "revival_exit_capture: PASS records="
                  << records.size() << '\n';
        return 0;
    } catch (const std::exception& exception) {
        std::cerr << "revival_exit_capture: FAIL: " << exception.what()
                  << '\n';
        return 1;
    }
}
