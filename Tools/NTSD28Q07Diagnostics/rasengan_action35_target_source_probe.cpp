#include "ntsd28_playable/game_session_lfr.h"

#include <array>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

struct TargetPosition {
    int x;
    int z;
};

struct CaseResult {
    bool saw_action_35 = false;
    bool saw_child_candidate = false;
    bool saw_target_hp_change = false;
    int first_candidate_tick = -1;
    int first_target_hp_change_tick = -1;
};

bool run_case(const std::filesystem::path& decoded_dat,
              const std::filesystem::path& vfs,
              const TargetPosition& target_position,
              std::ofstream& rows,
              std::vector<std::uint8_t>& witness_lfr,
              TargetPosition& witness_position,
              CaseResult& result,
              std::string& error) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 2;
    config.enemy_id = 7;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 naruto;
    naruto.slot = 0;
    naruto.object_id = 2;
    naruto.x = 500;
    naruto.z = 650;
    naruto.hp = 500;
    naruto.mp = 500;
    naruto.team = 1;
    naruto.action = 0;
    ntsd28_playable::CombatantConfig28 opponent;
    opponent.slot = 1;
    opponent.object_id = 7;
    opponent.x = target_position.x;
    opponent.z = target_position.z;
    opponent.hp = 500;
    opponent.mp = 500;
    opponent.team = 2;
    opponent.action = 0;
    config.combatants = {naruto, opponent};

    ntsd28_playable::GameSession28 session(decoded_dat, vfs);
    if (!session.initialize(config, error)) return false;
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) return false;
    int previous_target_hp = 500;
    for (int tick = 1; tick <= 55; ++tick) {
        ntsd28::InputButtons28 buttons;
        const char* input = "none";
        if (tick <= 2) {
            buttons.set(ntsd28::InputKey28::defend);
            input = "defend";
        } else if (tick <= 4) {
            buttons.set(ntsd28::InputKey28::right);
            input = "right";
        } else if (tick <= 6) {
            buttons.set(ntsd28::InputKey28::jump);
            input = "jump";
        } else if (tick == 36 || tick == 37) {
            buttons.set(ntsd28::InputKey28::attack);
            input = "attack";
        }
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) return false;
        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        const auto* target = world == nullptr ? nullptr : world->entity(1);
        if (actor == nullptr || target == nullptr) {
            error = "participant disappeared during target scan";
            return false;
        }
        int child_slot = -1;
        int child_action = -1;
        int child_count = 0;
        int child_candidates_to_target = 0;
        int first_eligibility_status = -1;
        std::string first_eligibility_message;
        for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots;
             ++slot) {
            const auto* child = world->entity(slot);
            if (child == nullptr || child->object_id != 434 ||
                child->owner_slot != 0) {
                continue;
            }
            ++child_count;
            if (child_slot < 0) {
                child_slot = static_cast<int>(slot);
                child_action = child->frame.action;
            }
            if (child->frame.action == 35) result.saw_action_35 = true;
            const auto& candidates = child->hit_candidates.items();
            for (std::size_t candidate_index = 0;
                 candidate_index < candidates.size(); ++candidate_index) {
                const auto& candidate = candidates[candidate_index];
                if (candidate.target_slot == 1 && child->frame.action == 35) {
                    ++child_candidates_to_target;
                    if (first_eligibility_status < 0) {
                        const auto eligibility =
                            world->classify_ordinary_hit_eligibility(
                                slot, candidate_index);
                        first_eligibility_status =
                            static_cast<int>(eligibility.status);
                        first_eligibility_message = eligibility.message;
                    }
                }
            }
        }
        if (child_candidates_to_target != 0) {
            result.saw_child_candidate = true;
            if (result.first_candidate_tick < 0) result.first_candidate_tick = tick;
        }
        if (target->current_hp != previous_target_hp) {
            result.saw_target_hp_change = true;
            if (result.first_target_hp_change_tick < 0) {
                result.first_target_hp_change_tick = tick;
            }
        }
        rows << target_position.x << ',' << target_position.z << ',' << tick
             << ',' << input << ',' << actor->frame.action << ','
             << child_count << ',' << child_slot << ',' << child_action << ','
             << child_candidates_to_target << ',' << first_eligibility_status
             << ',' << '"' << first_eligibility_message << '"' << ','
             << target->current_hp << ',' << target->frame.action << '\n';
        previous_target_hp = target->current_hp;
    }
    if (result.saw_action_35 && result.saw_child_candidate &&
        result.saw_target_hp_change && witness_lfr.empty()) {
        if (!recorder.finish_to_memory(session, witness_lfr, error)) return false;
        witness_position = target_position;
    }
    return true;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: rasengan_action35_target_source_probe <decoded_dat> <vfs> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    std::filesystem::create_directories(output);
    const auto csv = output / "target-scan.csv";
    const auto summary = output / "target-scan-summary.csv";
    const auto lfr = output / "first-attributable-target.lfr";
    if (std::filesystem::exists(csv) || std::filesystem::exists(summary) ||
        std::filesystem::exists(lfr)) {
        std::cerr << "refusing to overwrite diagnostic output\n";
        return 3;
    }
    std::ofstream rows(csv, std::ios::binary);
    std::ofstream summaries(summary, std::ios::binary);
    if (!rows || !summaries) return 4;
    rows << "target_x,target_z,tick,input,actor_action,owned_oid434,child_slot,child_action,child_candidates_to_target,eligibility_status,eligibility_message,target_hp,target_action\n";
    summaries << "target_x,target_z,saw_action35,saw_child_candidate,saw_target_hp_change,first_candidate_tick,first_target_hp_change_tick\n";
    std::vector<std::uint8_t> witness_lfr;
    TargetPosition witness_position{};
    constexpr std::array<TargetPosition, 11> positions{{
        {515, 650}, {520, 650}, {525, 650}, {530, 650}, {535, 650},
        {540, 650}, {545, 650}, {550, 650}, {520, 640}, {530, 640},
        {520, 660},
    }};
    for (const auto& position : positions) {
        CaseResult result;
        std::string error;
        if (!run_case(argv[1], argv[2], position, rows, witness_lfr,
                      witness_position, result, error)) {
            std::cerr << "target X" << position.x << "/Z" << position.z
                      << " failed: " << error << '\n';
            return 5;
        }
        summaries << position.x << ',' << position.z << ','
                  << (result.saw_action_35 ? 1 : 0) << ','
                  << (result.saw_child_candidate ? 1 : 0) << ','
                  << (result.saw_target_hp_change ? 1 : 0) << ','
                  << result.first_candidate_tick << ','
                  << result.first_target_hp_change_tick << '\n';
    }
    rows.close();
    summaries.close();
    if (!rows || !summaries) return 6;
    if (!witness_lfr.empty()) {
        std::ofstream encoded(lfr, std::ios::binary);
        if (!encoded) return 7;
        encoded.write(reinterpret_cast<const char*>(witness_lfr.data()),
                      static_cast<std::streamsize>(witness_lfr.size()));
        encoded.close();
        if (!encoded) return 8;
        std::cout << "candidate and target HP change at X" << witness_position.x
                  << "/Z" << witness_position.z << ", "
                  << witness_lfr.size() << " LFR bytes\n";
    } else {
        std::cout << "no candidate-plus-target-HP-change in 11 bounded cases\n";
    }
    return 0;
}
