#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain

#include "ntsd28/dat_parser.h"
#include "ntsd28/battle_world.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <iterator>
#include <memory>
#include <stdexcept>
#include <string>

namespace {

void run_case(const std::shared_ptr<const ntsd28::DatDocument>& definition,
              const char* label, int frame_counter, bool local_mode) {
    ntsd28::BattleWorld28 world;
    ntsd28::NativeHitResourceRules28 rules;
    rules.local_mode_enabled_49d034 = local_mode;
    rules.attacker_injury_mp_percent_34 = 75;
    rules.target_injury_mp_percent_38 = 75;
    world.set_native_hit_resource_rules(rules);

    for (int slot = 0; slot != 2; ++slot) {
        ntsd28::SpawnRequest28 request;
        request.object_id = 24;
        request.object_type = 0;
        request.definition = definition;
        request.initial_action = slot == 0 ? 239 : 135;
        request.position.x = slot == 0 ? 300 : 350;
        request.position.z = 250;
        request.hp = 100;
        request.mp = 100;
        if (!world.spawn_at(slot, request).success) {
            throw std::runtime_error("real Hidan DAT spawn failed");
        }
    }
    auto* catcher = world.entity(0);
    auto* caught = world.entity(1);
    catcher->catch_target_slot_8c = 1;
    caught->catch_source_slot_90 = 0;
    catcher->frame.frame_counter = frame_counter;

    const int before_catcher_mp = catcher->current_mp;
    const int before_caught_mp = caught->current_mp;
    const int before_caught_hp = caught->current_hp;
    const auto result = world.settle_catch_relations();
    std::cout << "{\"case\":\"" << label << "\",\"success\":"
              << (result.success ? "true" : "false")
              << ",\"frameCounter\":" << frame_counter
              << ",\"localMode\":" << (local_mode ? "true" : "false")
              << ",\"before\":{\"catcherMP\":" << before_catcher_mp
              << ",\"caughtMP\":" << before_caught_mp
              << ",\"caughtHP\":" << before_caught_hp
              << "},\"after\":{\"catcherMP\":" << catcher->current_mp
              << ",\"caughtMP\":" << caught->current_mp
              << ",\"caughtHP\":" << caught->current_hp
              << ",\"caughtMpConsumed\":" << caught->input_mp_consumed_total
              << ",\"caughtDisplayScoreStep\":" << caught->display_score_step_1f4
              << ",\"catcherCounter\":" << catcher->frame.frame_counter
              << "}}\n";
    if (!result.success) {
        throw std::runtime_error("real Hidan DAT catch settlement failed");
    }
}

void run_throw_case(
    const std::shared_ptr<const ntsd28::DatDocument>& throw_definition,
    const std::shared_ptr<const ntsd28::DatDocument>& caught_definition,
    const char* label, bool local_mode) {
    ntsd28::BattleWorld28 world;
    ntsd28::NativeHitResourceRules28 rules;
    rules.local_mode_enabled_49d034 = local_mode;
    rules.attacker_injury_mp_percent_34 = 75;
    rules.target_injury_mp_percent_38 = 75;
    world.set_native_hit_resource_rules(rules);
    for (int slot = 0; slot != 2; ++slot) {
        ntsd28::SpawnRequest28 request;
        request.object_id = slot == 0 ? 56 : 24;
        request.object_type = 0;
        request.definition = slot == 0 ? throw_definition : caught_definition;
        request.initial_action = slot == 0 ? 248 : 135;
        request.position.x = slot == 0 ? 300 : 350;
        request.position.z = 250;
        request.hp = 100;
        request.mp = 100;
        if (!world.spawn_at(slot, request).success) {
            throw std::runtime_error("real throw DAT spawn failed");
        }
    }
    auto* catcher = world.entity(0);
    auto* caught = world.entity(1);
    catcher->catch_target_slot_8c = 1;
    catcher->catch_timeout_94 = 300;
    caught->catch_source_slot_90 = 0;
    const auto result = world.advance_catch_relations();
    std::cout << "{\"case\":\"" << label << "\",\"success\":"
              << (result.success ? "true" : "false")
              << ",\"localMode\":" << (local_mode ? "true" : "false")
              << ",\"before\":{\"catcherMP\":100,\"caughtMP\":100,\"caughtHP\":100}"
              << ",\"after\":{\"catcherMP\":" << catcher->current_mp
              << ",\"caughtMP\":" << caught->current_mp
              << ",\"caughtHP\":" << caught->current_hp
              << ",\"environment\":" << caught->environment_state_320
              << ",\"displayScoreStep\":" << caught->display_score_step_1f4
              << ",\"armedThrowInjuries\":" << result.armed_throw_injuries
              << "}}\n";
    if (!result.success) {
        throw std::runtime_error("real throw DAT advance failed");
    }
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) {
        std::cerr << "usage: cpoint_resource_first_diff_witness <formal_hid.dat> <formal_rea.dat>\n";
        return 2;
    }
    const std::filesystem::path dat_path(argv[1]);
    std::ifstream input(dat_path, std::ios::binary);
    if (!input) {
        std::cerr << "cannot read formal Hidan DAT\n";
        return 3;
    }
    const std::string bytes(std::istreambuf_iterator<char>{input}, {});
    const auto definition = std::make_shared<const ntsd28::DatDocument>(
        ntsd28::DatParser{}.parse_text(bytes));
    if (!definition->ok() || definition->frame(239) == nullptr ||
        definition->frame(135) == nullptr) {
        std::cerr << "formal Hidan DAT parse/frame gate failed\n";
        return 4;
    }
    std::ifstream throw_input(std::filesystem::path(argv[2]), std::ios::binary);
    if (!throw_input) {
        std::cerr << "cannot read formal Reaper DAT\n";
        return 4;
    }
    const std::string throw_bytes(std::istreambuf_iterator<char>{throw_input}, {});
    const auto throw_definition =
        std::make_shared<const ntsd28::DatDocument>(
            ntsd28::DatParser{}.parse_text(throw_bytes));
    if (!throw_definition->ok() || throw_definition->frame(248) == nullptr) {
        std::cerr << "formal Reaper DAT parse/frame gate failed\n";
        return 4;
    }
    try {
        run_case(definition, "positive", 0, true);
        run_case(definition, "counter_control", 1, true);
        run_case(definition, "local_mode_control", 0, false);
        run_throw_case(throw_definition, definition, "throw_positive", true);
        run_throw_case(throw_definition, definition, "throw_local_mode_control", false);
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 5;
    }
    return 0;
}
