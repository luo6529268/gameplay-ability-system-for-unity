#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: hidan_natural_frame430_probe <runtime_root> <complete_vfs_root> <output_jsonl>\n";
        return 2;
    }

    const std::filesystem::path output(argv[3]);
    auto lfr_output = output;
    lfr_output.replace_extension(".lfr");
    if (std::filesystem::exists(output) || std::filesystem::exists(lfr_output)) {
        std::cerr << "refusing to overwrite existing evidence\n";
        return 3;
    }
    std::filesystem::create_directories(output.parent_path());
    std::ofstream rows(output, std::ios::binary);
    if (!rows) return 4;

    bool reached = false;
    for (int combo_tick = 13; combo_tick <= 20 && !reached; ++combo_tick) {
        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 0x28A55A5Au;
        config.character_id = 24;
        config.enemy_id = 24;
        config.background_id = 23;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;

        ntsd28_playable::CombatantConfig28 actor;
        actor.slot = 0;
        actor.object_id = 24;
        actor.x = 500;
        actor.z = 350;
        actor.hp = 500;
        actor.mp = 300;
        actor.team = 1;
        actor.action = 0;
        ntsd28_playable::CombatantConfig28 target;
        target.slot = 1;
        target.object_id = 24;
        target.x = 1200;
        target.z = 350;
        target.hp = 500;
        target.mp = 300;
        target.team = 2;
        target.action = 0;
        config.combatants = {actor, target};

        ntsd28_playable::GameSession28 session(argv[1], argv[2]);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "initialize comboTick=" << combo_tick << ": " << error << '\n';
            return 5;
        }
        ntsd28_playable::GameSessionLfr28 recorder;
        if (!recorder.begin(session, error)) {
            std::cerr << "record begin comboTick=" << combo_tick << ": " << error << '\n';
            return 6;
        }

        int first_212 = -1;
        int first_430 = -1;
        for (int tick = 1; tick <= 30; ++tick) {
            const auto* before_world = session.world();
            const auto* before = before_world == nullptr ? nullptr : before_world->entity(0);
            if (before == nullptr) return 7;
            const int pre_action = before->frame.action;

            ntsd28::InputButtons28 buttons;
            if (tick <= 2) buttons.set(ntsd28::InputKey28::attack);
            if (tick == 9 || tick == 10) buttons.set(ntsd28::InputKey28::jump);
            const bool combo = tick == combo_tick || tick == combo_tick + 1;
            if (combo) {
                buttons.set(ntsd28::InputKey28::defend);
                buttons.set(ntsd28::InputKey28::right);
                buttons.set(ntsd28::InputKey28::attack);
            }
            session.set_input(0, buttons);
            session.set_input(1, {});
            session.step();
            if (!recorder.capture_after_step(session, error)) {
                std::cerr << "capture tick=" << tick << ": " << error << '\n';
                return 8;
            }

            const auto* world = session.world();
            const auto* current = world == nullptr ? nullptr : world->entity(0);
            if (current == nullptr) return 9;
            const int action = current->frame.action;
            if (action == 212 && first_212 < 0) first_212 = tick;
            if (action == 430 && first_430 < 0) first_430 = tick;
            rows << "{\"comboTick\":" << combo_tick
                 << ",\"tick\":" << tick
                 << ",\"comboHeld\":" << (combo ? "true" : "false")
                 << ",\"inputPhase\":" << world->input_update_phase_4a0b90()
                 << ",\"preAction\":" << pre_action
                 << ",\"actorAction\":" << action
                 << ",\"tickAction\":" << current->frame.tick_action_snapshot
                 << ",\"comboFa\":" << static_cast<int>(current->input.combo_state[0])
                 << ",\"actorMp\":" << current->current_mp
                 << "}\n";
        }

        if (first_430 >= 0) {
            std::vector<std::uint8_t> bytes;
            if (!recorder.finish_to_memory(session, bytes, error)) {
                std::cerr << "record finish: " << error << '\n';
                return 10;
            }
            std::ofstream encoded(lfr_output, std::ios::binary);
            if (!encoded) return 11;
            encoded.write(reinterpret_cast<const char*>(bytes.data()),
                          static_cast<std::streamsize>(bytes.size()));
            encoded.close();
            if (!encoded) return 12;
            reached = true;
        }
        std::cout << "{\"comboTick\":" << combo_tick
                  << ",\"first212\":" << first_212
                  << ",\"first430\":" << first_430 << "}\n";
    }
    rows.close();
    if (!rows) return 13;
    return reached ? 0 : 14;
}
