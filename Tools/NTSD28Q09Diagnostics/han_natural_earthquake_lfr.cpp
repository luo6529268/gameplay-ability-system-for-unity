#include "ntsd28_playable/game_session_lfr.h"

#include <array>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

bool run_case(const std::filesystem::path& runtime_root,
              const std::filesystem::path& output_root,
              int target_x, int jump_tick, bool& passed,
              std::string& error) {
    const std::string stem = "han-action0-x" + std::to_string(target_x) +
                             "-jump" + std::to_string(jump_tick);
    const auto csv = output_root / (stem + ".csv");
    const auto lfr = output_root / (stem + ".lfr");
    if (std::filesystem::exists(csv) || std::filesystem::exists(lfr)) {
        error = "refusing to overwrite existing case output";
        return false;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 2833;
    config.character_id = 726;
    config.enemy_id = 7;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 han;
    han.slot = 0;
    han.object_id = 726;
    han.x = 500;
    han.z = 650;
    han.hp = 500;
    han.mp = 500;
    han.team = 1;
    han.action = 0;
    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 1;
    lee.object_id = 7;
    lee.x = target_x;
    lee.z = 650;
    lee.hp = 500;
    lee.mp = 500;
    lee.team = 2;
    lee.action = 0;
    config.combatants = {han, lee};

    ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
    if (!session.initialize(config, error)) return false;
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) return false;
    std::ofstream rows(csv, std::ios::binary);
    if (!rows) {
        error = "unable to open case CSV";
        return false;
    }
    rows << "tick,attack,jump,han_action,han_state,lee_action,han_x,lee_x,han_mp,lee_hp,catch_target,catch_source,quake_owner,background_x,background_y,han_collision_action,han_selected_candidate_count,han_lee_candidate_count,native_appended_candidate_count\n";

    int first_145 = -1;
    int first_grab_window = -1;
    int first_relation = -1;
    int first_quake = -1;
    int first_reset = -1;
    for (int tick = 1; tick <= 50; ++tick) {
        ntsd28::InputButtons28 buttons;
        const bool attack = tick <= 2;
        const bool jump = tick == jump_tick || tick == jump_tick + 1;
        if (attack) buttons.set(ntsd28::InputKey28::attack);
        if (jump) buttons.set(ntsd28::InputKey28::jump);
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) return false;

        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        const auto* target = world == nullptr ? nullptr : world->entity(1);
        if (actor == nullptr || target == nullptr) {
            error = "a combatant disappeared before tick 50";
            return false;
        }
        const int action = actor->frame.action;
        const auto* frame = actor->definition->frame(action);
        const int state = frame == nullptr
                              ? 0
                              : frame->values.integer("state").value_or(0);
        const auto snapshot = session.snapshot(false);
        const auto& quake = snapshot.earthquake;
        const auto* completed_tick = session.last_tick();
        if (completed_tick == nullptr) {
            error = "missing completed tick candidate result";
            return false;
        }
        std::size_t lee_candidate_count = 0;
        for (const auto& candidate : actor->hit_candidates.items()) {
            if (candidate.target_slot == 1) ++lee_candidate_count;
        }
        rows << tick << ',' << attack << ',' << jump << ',' << action << ','
             << state << ',' << target->frame.action << ','
             << actor->position.x << ',' << target->position.x << ','
             << actor->current_mp << ',' << target->current_hp << ','
             << actor->catch_target_slot_8c << ','
             << target->catch_source_slot_90 << ',' << quake.owner_slot << ','
             << quake.background_offset_x << ','
             << quake.background_offset_y << ','
             << actor->frame.tick_action_snapshot << ','
             << actor->hit_candidates.size() << ','
             << lee_candidate_count << ','
             << completed_tick->candidates.candidates_appended << '\n';

        if (action == 145 && first_145 < 0) first_145 = tick;
        if (action >= 146 && action <= 148 && first_grab_window < 0)
            first_grab_window = tick;
        if (action == 149 && actor->catch_target_slot_8c == 1 &&
            target->catch_source_slot_90 == 0 && first_relation < 0)
            first_relation = tick;
        if (action == 150 && state == 55052 && quake.owner_slot == 0 &&
            quake.background_offset_x == 2 &&
            quake.background_offset_y == 0 && first_quake < 0)
            first_quake = tick;
        if (action == 151 && state == 55050 && quake.owner_slot == 0 &&
            quake.background_offset_x == 0 &&
            quake.background_offset_y == 0 && first_quake >= 0 &&
            first_reset < 0)
            first_reset = tick;
    }
    rows.close();
    if (!rows) {
        error = "unable to finish case CSV";
        return false;
    }
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) return false;
    std::ofstream encoded(lfr, std::ios::binary);
    if (!encoded) {
        error = "unable to open case LFR";
        return false;
    }
    encoded.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
    encoded.close();
    if (!encoded) {
        error = "unable to finish case LFR";
        return false;
    }

    passed = first_145 >= 0 && first_grab_window > first_145 &&
             first_relation > first_grab_window &&
             first_quake > first_relation && first_reset > first_quake;
    std::cout << "{\"targetX\":" << target_x
              << ",\"jumpTick\":" << jump_tick
              << ",\"first145\":" << first_145
              << ",\"firstGrabWindow\":" << first_grab_window
              << ",\"firstRelation\":" << first_relation
              << ",\"firstQuake\":" << first_quake
              << ",\"firstReset\":" << first_reset
              << ",\"lfrBytes\":" << bytes.size()
              << ",\"passed\":" << (passed ? "true" : "false")
              << "}\n";
    return true;
}

}  // namespace

int main(int argc, char** argv) {
    if (argc != 3 && argc != 4) {
        std::cerr << "usage: han_natural_earthquake_lfr <runtime_root> <output_dir> [x520-jump3]\n";
        return 2;
    }
    if (argc == 4 && std::string(argv[3]) != "x520-jump3") {
        std::cerr << "unsupported selected case\n";
        return 2;
    }
    const std::filesystem::path runtime_root(argv[1]);
    const std::filesystem::path output_root(argv[2]);
    std::filesystem::create_directories(output_root);
    bool any_passed = false;
    for (const int target_x : std::array<int, 2>{520, 580}) {
        for (const int jump_tick : std::array<int, 2>{3, 5}) {
            if (argc == 4 && (target_x != 520 || jump_tick != 3))
                continue;
            bool passed = false;
            std::string error;
            if (!run_case(runtime_root, output_root, target_x, jump_tick,
                          passed, error)) {
                std::cerr << "case x" << target_x << "/jump" << jump_tick
                          << " failed: " << error << '\n';
                return 4;
            }
            any_passed |= passed;
        }
    }
    return any_passed ? 0 : 8;
}
