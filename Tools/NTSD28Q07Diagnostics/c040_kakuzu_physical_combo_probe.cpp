#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: c040_kakuzu_physical_combo_probe <formal_root> <new_output_directory>\n";
        return 2;
    }
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }
    std::filesystem::create_directories(output);
    std::ofstream rows(output / "source-ticks.csv", std::ios::binary);
    std::ofstream summary(output / "summary.csv", std::ios::binary);
    if (!rows || !summary) return 4;
    rows << "jump_start,tick,attack,jump,input_phase,actor_action,"
            "tick_action,combo_aj,actor_x,target_x,actor_hp,target_hp,"
            "crt_state,crt_calls,custom_counter,custom_index,custom_calls\n";
    summary << "jump_start,first_60,first_320,first_322,first_relation,lfr\n";

    const auto runtime_root = std::filesystem::path(argv[1]) /
                              "resources" / "runtime";
    int positive_count = 0;
    for (int jump_start = 3; jump_start <= 15; ++jump_start) {
        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 0u;
        config.character_id = 25;
        config.enemy_id = 75;
        config.background_id = 1;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;
        ntsd28_playable::CombatantConfig28 actor;
        actor.slot = 0;
        actor.object_id = 25;
        actor.x = 500;
        actor.y = 0;
        actor.z = 400;
        actor.hp = actor.base_hp = actor.mp = 500;
        actor.team = 1;
        actor.action = 0;
        auto target = actor;
        target.slot = 1;
        target.object_id = 75;
        target.x = 1200;
        target.team = 2;
        config.combatants = {actor, target};

        ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "initialize jump_start=" << jump_start << ": " << error << '\n';
            return 5;
        }
        ntsd28_playable::GameSessionLfr28 recorder;
        if (!recorder.begin(session, error)) {
            std::cerr << "record begin: " << error << '\n';
            return 6;
        }
        int first_60 = -1;
        int first_320 = -1;
        int first_322 = -1;
        int first_relation = -1;
        for (int tick = 1; tick <= 32; ++tick) {
            ntsd28::InputButtons28 buttons;
            const bool attack = tick <= 2;
            const bool jump = tick == jump_start || tick == jump_start + 1;
            if (attack) buttons.set(ntsd28::InputKey28::attack);
            if (jump) buttons.set(ntsd28::InputKey28::jump);
            session.set_input(0, buttons);
            session.set_input(1, ntsd28::InputButtons28{});
            session.step();
            if (!recorder.capture_after_step(session, error)) {
                std::cerr << "record tick " << tick << ": " << error << '\n';
                return 7;
            }
            const auto* world = session.world();
            const auto* current = world == nullptr ? nullptr : world->entity(0);
            const auto* victim = world == nullptr ? nullptr : world->entity(1);
            if (!current || !victim) return 8;
            const int action = current->frame.action;
            if (action == 60 && first_60 < 0) first_60 = tick;
            if (action == 320 && first_320 < 0) first_320 = tick;
            if (action == 322 && first_322 < 0) first_322 = tick;
            if (current->catch_target_slot_8c == 1 &&
                victim->catch_source_slot_90 == 0 && first_relation < 0) {
                first_relation = tick;
            }
            const auto random = world->random().state();
            rows << jump_start << ',' << tick << ',' << attack << ',' << jump
                 << ',' << world->input_update_phase_4a0b90() << ',' << action
                 << ',' << current->frame.tick_action_snapshot << ','
                 << static_cast<int>(current->input.combo_state[6]) << ','
                 << current->position.x << ',' << victim->position.x << ','
                 << current->current_hp << ',' << victim->current_hp << ','
                 << random.crt_state << ',' << random.crt_calls << ','
                 << random.synchronized.counter << ',' << random.synchronized.index
                 << ',' << random.synchronized.calls << '\n';
        }
        bool emitted = false;
        if (first_320 >= 0 || first_322 >= 0) {
            std::vector<std::uint8_t> bytes;
            if (!recorder.finish_to_memory(session, bytes, error)) {
                std::cerr << "record finish: " << error << '\n';
                return 9;
            }
            const auto path = output /
                ("jump-" + std::to_string(jump_start) + ".lfr");
            std::ofstream encoded(path, std::ios::binary);
            if (!encoded) return 10;
            encoded.write(reinterpret_cast<const char*>(bytes.data()),
                          static_cast<std::streamsize>(bytes.size()));
            encoded.close();
            if (!encoded) return 11;
            emitted = true;
            ++positive_count;
        }
        summary << jump_start << ',' << first_60 << ',' << first_320 << ','
                << first_322 << ',' << first_relation << ',' << emitted << '\n';
    }
    rows.close();
    summary.close();
    if (!rows || !summary) return 12;
    std::cout << "cases=13,combo_reached=" << positive_count << '\n';
    return 0;
}
