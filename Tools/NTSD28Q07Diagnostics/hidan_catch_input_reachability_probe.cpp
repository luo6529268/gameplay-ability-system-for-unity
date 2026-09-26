#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int wmain(int argc, wchar_t** argv) {
    if (argc != 5) {
        std::cerr << "usage: hidan_catch_input_reachability_probe <runtime_root> <complete_vfs_root> <output_jsonl> <target_x:520|580|1200>\n";
        return 2;
    }
    const std::wstring target_text(argv[4]);
    if (target_text != L"520" && target_text != L"580" &&
        target_text != L"1200") {
        std::cerr << "target_x must be 520, 580, or 1200\n";
        return 2;
    }
    const int target_x = std::stoi(target_text);
    const std::filesystem::path output(argv[3]);
    auto lfr_output = output;
    lfr_output.replace_extension(".lfr");
    if (std::filesystem::exists(output) || std::filesystem::exists(lfr_output)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }
    std::filesystem::create_directories(output.parent_path());
    std::ofstream rows(output, std::ios::binary);
    if (!rows) return 4;

    for (int jump_start = 3; jump_start <= 15; jump_start += 2) {
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
        target.x = target_x;
        target.z = 350;
        target.hp = 500;
        target.mp = 300;
        target.team = 2;
        target.action = 0;
        config.combatants = {actor, target};

        ntsd28_playable::GameSession28 session(argv[1], argv[2]);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "initialize jumpStart=" << jump_start << ": " << error << '\n';
            return 5;
        }
        ntsd28_playable::GameSessionLfr28 recorder;
        const bool record_lfr = jump_start == 3;
        if (record_lfr && !recorder.begin(session, error)) {
            std::cerr << "record begin: " << error << '\n';
            return 8;
        }
        int first_235 = -1;
        int first_249 = -1;
        int first_236 = -1;
        int first_relation = -1;
        for (int tick = 1; tick <= 40; ++tick) {
            ntsd28::InputButtons28 buttons;
            if (tick <= 2) buttons.set(ntsd28::InputKey28::attack);
            if (tick == jump_start || tick == jump_start + 1) {
                buttons.set(ntsd28::InputKey28::jump);
            }
            session.set_input(0, buttons);
            session.set_input(1, {});
            session.step();
            if (record_lfr && !recorder.capture_after_step(session, error)) {
                std::cerr << "record tick " << tick << ": " << error << '\n';
                return 9;
            }
            const auto* world = session.world();
            const auto* current = world == nullptr ? nullptr : world->entity(0);
            const auto* victim = world == nullptr ? nullptr : world->entity(1);
            if (current == nullptr || victim == nullptr) {
                std::cerr << "participant disappeared at tick " << tick << '\n';
                return 6;
            }
            const int action = current->frame.action;
            if (action == 235 && first_235 < 0) first_235 = tick;
            if (action == 249 && first_249 < 0) first_249 = tick;
            if (action == 236 && first_236 < 0) first_236 = tick;
            if (current->catch_target_slot_8c == 1 &&
                victim->catch_source_slot_90 == 0 && first_relation < 0) {
                first_relation = tick;
            }
            rows << "{\"targetX\":" << target_x
                 << ",\"jumpStart\":" << jump_start
                 << ",\"tick\":" << tick
                 << ",\"input\":\"" << (tick <= 2 ? "attack" :
                     tick == jump_start || tick == jump_start + 1 ? "jump" : "none")
                 << "\",\"inputPhase\":" << world->input_update_phase_4a0b90()
                 << ",\"actorAction\":" << action
                 << ",\"tickAction\":" << current->frame.tick_action_snapshot
                 << ",\"comboAj\":" << static_cast<int>(current->input.combo_state[6])
                 << ",\"actorMp\":" << current->current_mp
                 << ",\"targetMp\":" << victim->current_mp
                 << ",\"targetHp\":" << victim->current_hp
                 << ",\"catchTarget\":" << current->catch_target_slot_8c
                 << ",\"catchSource\":" << victim->catch_source_slot_90
                 << "}\n";
        }
        if (record_lfr) {
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
        }
        std::cout << "{\"targetX\":" << target_x
                  << ",\"jumpStart\":" << jump_start
                  << ",\"first235\":" << first_235
                  << ",\"first249\":" << first_249
                  << ",\"first236\":" << first_236
                  << ",\"firstRelation\":" << first_relation
                  << "}\n";
    }
    rows.close();
    return rows ? 0 : 7;
}
