#include "ntsd28_playable/game_session.h"
#include "ntsd28_playable/scenario28.h"
#include "ntsd28/native_random.h"

#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <windows.h>

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <limits>
#include <stdexcept>
#include <string>

// The repository build command wraps these two native RNG calls. This focused
// witness preserves their ordinary behavior without adding a trace observer.
extern "C" std::uint32_t __real__ZN6ntsd2814NativeRandom288crt_nextEv(
    ntsd28::NativeRandom28* random);
extern "C" std::int32_t __real__ZN6ntsd2814NativeRandom2817synchronized_nextEji(
    ntsd28::NativeRandom28* random, std::uint32_t call_site,
    std::int32_t upper_bound);
extern "C" std::uint32_t __wrap__ZN6ntsd2814NativeRandom288crt_nextEv(
    ntsd28::NativeRandom28* random) {
    return __real__ZN6ntsd2814NativeRandom288crt_nextEv(random);
}
extern "C" std::int32_t __wrap__ZN6ntsd2814NativeRandom2817synchronized_nextEji(
    ntsd28::NativeRandom28* random, std::uint32_t call_site,
    std::int32_t upper_bound) {
    return __real__ZN6ntsd2814NativeRandom2817synchronized_nextEji(
        random, call_site, upper_bound);
}

namespace {

struct Options {
    std::filesystem::path scenario;
    std::filesystem::path output;
    std::filesystem::path resource_root;
    std::filesystem::path complete_vfs_root;
};

bool parse_options(int count, wchar_t** args, Options& result) {
    for (int index = 1; index < count; ++index) {
        if (index + 1 >= count) return false;
        const std::wstring option = args[index];
        const std::filesystem::path value = args[++index];
        if (option == L"--scenario") result.scenario = value;
        else if (option == L"--output") result.output = value;
        else if (option == L"--resource-root") result.resource_root = value;
        else if (option == L"--complete-vfs-root") result.complete_vfs_root = value;
        else return false;
    }
    return !result.scenario.empty() && !result.output.empty() &&
           !result.resource_root.empty() && !result.complete_vfs_root.empty();
}

void write_snapshot(std::ostream& output, const ntsd28::BattleWorld28& world,
                    int completed_tick) {
    const auto* target = world.entity(0);
    const auto* subject = world.entity(1);
    if (subject == nullptr || target == nullptr)
        throw std::runtime_error("fixture entity missing during full tick");
    output << std::setprecision(std::numeric_limits<double>::max_digits10)
           << "{\"completedTick\":" << completed_tick
           << ",\"worldSequence\":" << world.sequence()
           << ",\"targetOid\":" << target->object_id
           << ",\"subjectOid\":" << subject->object_id
           << ",\"targetSlot\":" << subject->object_ai_target_slot_3f8
           << ",\"action\":" << subject->frame.action
           << ",\"motionX\":" << subject->motion.x
           << ",\"motionY\":" << subject->motion.y
           << ",\"motionZ\":" << subject->motion.z
           << ",\"integerY\":" << subject->position.y
           << ",\"preciseY\":" << subject->position.precise_y
           << ",\"previousY\":" << subject->position.previous_y
           << ",\"entityCount\":" << world.active_count()
           << "}\n";
}

int run(int count, wchar_t** args) {
    Options options;
    if (!parse_options(count, args, options)) {
        std::cerr << "usage: hitfa7_target_fulltick --scenario <json> "
                     "--output <jsonl> --resource-root <folder> "
                     "--complete-vfs-root <folder>\n";
        return 2;
    }
    const auto loaded = ntsd28_playable::ScenarioLoader28::load(options.scenario);
    if (!loaded.success || loaded.scenario.ticks != 3 ||
        loaded.scenario.battle.combatants.size() != 2 ||
        !loaded.scenario.inputs.empty()) {
        std::cerr << "fixture scenario failed strict shape check\n";
        return 3;
    }
    const auto& target_config = loaded.scenario.battle.combatants[0];
    const auto& subject_config = loaded.scenario.battle.combatants[1];
    if (target_config.slot != 0 || target_config.object_id != 99 ||
        target_config.action != 0 || target_config.x != 700 ||
        target_config.y != 0 || target_config.z != 620 ||
        target_config.team != 2 || subject_config.slot != 1 ||
        subject_config.object_id != 875 || subject_config.action != 55 ||
        subject_config.x != 400 || subject_config.y != -30 ||
        subject_config.z != 600 || subject_config.team != 1) {
        std::cerr << "fixture combatants do not match the declared witness\n";
        return 3;
    }
    ntsd28_playable::GameSession28 session(
        options.resource_root, options.complete_vfs_root);
    std::string error;
    if (!session.initialize(loaded.scenario.battle, error)) {
        std::cerr << "session: " << error << '\n';
        return 4;
    }
    auto* world = session.world();
    auto* target = world != nullptr ? world->entity(0) : nullptr;
    auto* subject = world != nullptr ? world->entity(1) : nullptr;
    if (subject == nullptr || target == nullptr ||
        subject->object_id != 875 || subject->object_type != 3 ||
        subject->frame.action != 55 || subject->position.y != -30 ||
        target->object_id != 99 || target->object_type != 0) {
        std::cerr << "initialized world differs from declared fixture\n";
        return 4;
    }
    subject->object_ai_target_slot_3f8 = 0;
    subject->motion.y = 3.8;
    std::ofstream output(options.output, std::ios::binary | std::ios::trunc);
    if (!output) return 5;
    write_snapshot(output, *world, 0);
    for (int tick = 1; tick <= 3; ++tick) {
        session.set_input(0, ntsd28::InputButtons28{});
        session.set_input(1, ntsd28::InputButtons28{});
        session.step();
        world = session.world();
        if (world == nullptr) return 6;
        write_snapshot(output, *world, tick);
    }
    output.flush();
    return output ? 0 : 6;
}

}  // namespace

int wmain(int count, wchar_t** args) {
    SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX |
                 SEM_NOOPENFILEERRORBOX);
    try {
        return run(count, args);
    } catch (const std::exception& exception) {
        std::cerr << "witness_exception=" << exception.what() << '\n';
        return 91;
    }
}
