#include "ntsd28_playable/game_session_lfr.h"

#include <algorithm>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

bool record_case(const std::filesystem::path& runtime_root,
                 const std::filesystem::path& complete_vfs_root,
                 const std::filesystem::path& output,
                 int initial_z,
                 std::string& error) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 16;
    config.enemy_id = 7;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;

    ntsd28_playable::CombatantConfig28 gaara;
    gaara.slot = 0;
    gaara.object_id = 16;
    gaara.x = 800;
    gaara.z = initial_z;
    gaara.hp = 500;
    gaara.mp = 500;
    gaara.team = 1;
    gaara.action = 0;
    ntsd28_playable::CombatantConfig28 opponent;
    opponent.slot = 1;
    opponent.object_id = 7;
    opponent.x = 1200;
    opponent.z = initial_z;
    opponent.hp = 500;
    opponent.mp = 500;
    opponent.team = 2;
    opponent.action = 0;
    config.combatants = {gaara, opponent};

    ntsd28_playable::GameSession28 session(runtime_root, complete_vfs_root);
    if (!session.initialize(config, error)) return false;
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) return false;

    std::ofstream rows(output / "gaara-sand-blast-source.csv", std::ios::binary);
    if (!rows) {
        error = "could not open source CSV";
        return false;
    }
    rows << "tick,input,phase,action,mp,x,y,z,audio_paths,audio_tokens\n";
    int first_240 = -1;
    int first_244 = -1;
    int first_043 = -1;
    for (int tick = 1; tick <= 40; ++tick) {
        ntsd28::InputButtons28 buttons;
        const char* input = "neutral";
        if (tick <= 2) {
            buttons.set(ntsd28::InputKey28::defend);
            input = "defend";
        } else if (tick <= 4) {
            buttons.set(ntsd28::InputKey28::right);
            input = "right";
        } else if (tick <= 6) {
            buttons.set(ntsd28::InputKey28::attack);
            input = "attack";
        }
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) return false;
        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        if (actor == nullptr) {
            error = "Gaara slot0 disappeared during diagnostic";
            return false;
        }
        const int action = actor->frame.action;
        if (action == 240 && first_240 < 0) first_240 = tick;
        if (action == 244 && first_244 < 0) first_244 = tick;
        std::string paths;
        std::string tokens;
        if (const auto* last_tick = session.last_tick()) {
            for (const auto& event : last_tick->audio_events) {
                std::string path = event.resource_path;
                std::replace(path.begin(), path.end(), '\\', '/');
                if (!path.empty()) {
                    if (!paths.empty()) paths += '|';
                    paths += path;
                }
                if (!tokens.empty()) tokens += '|';
                tokens += event.source == ntsd28::WorldAudioEventSource28::builtin_channel
                    ? "builtin:" + std::to_string(event.native_channel)
                    : "path:" + path;
                if (path == "data/043.wav" && first_043 < 0)
                    first_043 = tick;
            }
        }
        rows << tick << ',' << input << ','
             << world->input_update_phase_4a0b90() << ','
             << action << ',' << actor->current_mp << ','
             << actor->position.x << ',' << actor->position.y << ','
             << actor->position.z << ',' << paths << ',' << tokens << '\n';
    }
    rows.close();
    if (!rows) {
        error = "could not finish source CSV";
        return false;
    }

    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) return false;
    std::ofstream encoded(output / "gaara-sand-blast.lfr", std::ios::binary);
    if (!encoded) {
        error = "could not open LFR";
        return false;
    }
    encoded.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
    encoded.close();
    if (!encoded) {
        error = "could not finish LFR";
        return false;
    }
    std::cout << "ticks=40 first240=" << first_240
              << " first244=" << first_244
              << " first043=" << first_043 << '\n';
    return true;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 4 && argc != 5) {
        std::cerr << "usage: gaara_sand_blast_043_probe <runtime_root> <complete_vfs_root> <new_output_dir> [initial_z]\n";
        return 2;
    }
    int initial_z = 650;
    if (argc == 5) {
        try {
            initial_z = std::stoi(argv[4]);
        } catch (const std::exception&) {
            std::cerr << "invalid initial_z\n";
            return 2;
        }
    }
    const std::filesystem::path output(argv[3]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to reuse output directory\n";
        return 3;
    }
    std::filesystem::create_directories(output);
    std::string error;
    if (!record_case(argv[1], argv[2], output, initial_z, error)) {
        std::cerr << error << '\n';
        return 4;
    }
    return 0;
}
