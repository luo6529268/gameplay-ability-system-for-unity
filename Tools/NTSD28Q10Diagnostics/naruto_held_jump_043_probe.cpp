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
                 std::string& error) {
    const auto csv = output / "naruto-held-right-jump-source.csv";
    const auto lfr = output / "naruto-held-right-jump.lfr";
    if (std::filesystem::exists(csv) || std::filesystem::exists(lfr)) {
        error = "refusing to overwrite existing evidence";
        return false;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 2;
    config.enemy_id = 7;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    ntsd28_playable::CombatantConfig28 naruto;
    naruto.slot = 0;
    naruto.object_id = 2;
    naruto.x = 800;
    naruto.z = 650;
    naruto.hp = 500;
    naruto.mp = 500;
    naruto.team = 1;
    naruto.action = 0;
    ntsd28_playable::CombatantConfig28 opponent;
    opponent.slot = 1;
    opponent.object_id = 7;
    opponent.x = 1200;
    opponent.z = 650;
    opponent.hp = 500;
    opponent.mp = 500;
    opponent.team = 2;
    opponent.action = 0;
    config.combatants = {naruto, opponent};

    ntsd28_playable::GameSession28 session(runtime_root, complete_vfs_root);
    if (!session.initialize(config, error)) return false;
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) return false;
    std::ofstream rows(csv, std::ios::binary);
    if (!rows) {
        error = "could not open source CSV";
        return false;
    }
    rows << "tick,input,phase,actor_action,actor_state,actor_x,actor_y,actor_z,actor_mp,audio_043_count,audio_paths\n";
    int total_043 = 0;
    int first_043_tick = -1;
    for (int tick = 1; tick <= 80; ++tick) {
        ntsd28::InputButtons28 buttons;
        buttons.set(ntsd28::InputKey28::right);
        if (tick >= 5) buttons.set(ntsd28::InputKey28::jump);
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) return false;
        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        if (actor == nullptr) {
            error = "Naruto slot0 disappeared during diagnostic";
            return false;
        }
        int count_043 = 0;
        std::string paths;
        if (const auto* last_tick = session.last_tick()) {
            for (const auto& event : last_tick->audio_events) {
                std::string path = event.resource_path;
                std::replace(path.begin(), path.end(), '\\', '/');
                if (!paths.empty()) paths += '|';
                paths += path;
                if (path == "data/043.wav") ++count_043;
            }
        }
        total_043 += count_043;
        if (count_043 > 0 && first_043_tick < 0) first_043_tick = tick;
        const auto* frame = actor->definition == nullptr
            ? nullptr
            : actor->definition->frame(actor->frame.action);
        const int state = frame == nullptr
            ? 0
            : frame->values.integer("state").value_or(0);
        rows << tick << ',' << (tick < 5 ? "right" : "right+jump")
             << ',' << world->input_update_phase_4a0b90()
             << ',' << actor->frame.action
             << ',' << state
             << ',' << actor->position.x
             << ',' << actor->position.y
             << ',' << actor->position.z
             << ',' << actor->current_mp
             << ',' << count_043
             << ',' << paths << '\n';
    }
    rows.close();
    if (!rows) {
        error = "could not finish source CSV";
        return false;
    }
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) return false;
    std::ofstream encoded(lfr, std::ios::binary);
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
    std::cout << "ticks=80 rows=" << recorder.row_count()
              << " audio043=" << total_043
              << " first043=" << first_043_tick << '\n';
    return true;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: naruto_held_jump_043_probe <runtime_root> <complete_vfs_root> <new_output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to reuse output directory\n";
        return 3;
    }
    std::filesystem::create_directories(output);
    std::string error;
    if (!record_case(argv[1], argv[2], output, error)) {
        std::cerr << error << '\n';
        return 4;
    }
    return 0;
}
