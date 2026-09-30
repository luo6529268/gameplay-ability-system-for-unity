#include "ntsd28_playable/game_session_lfr.h"

#include <algorithm>
#include <array>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

struct Case {
    const char* name;
    int followup_jump_tick;
};

bool record_case(const std::filesystem::path& decoded_dat,
                 const std::filesystem::path& vfs,
                 const std::filesystem::path& output,
                 const Case& selected,
                 std::string& error) {
    const auto lfr = output / (std::string(selected.name) + ".lfr");
    const auto csv = output / (std::string(selected.name) + "-source.csv");
    if (std::filesystem::exists(lfr) || std::filesystem::exists(csv)) {
        error = "refusing to overwrite existing case output";
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
    naruto.x = 500;
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

    ntsd28_playable::GameSession28 session(decoded_dat, vfs);
    if (!session.initialize(config, error)) return false;
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) return false;
    std::ofstream rows(csv, std::ios::binary);
    if (!rows) {
        error = "could not open source CSV";
        return false;
    }
    rows << "tick,input,phase,actor_action,actor_mp,combo1,pending_jump,current_jump,previous_jump,audio_078_count\n";
    bool saw_325 = false;
    bool saw_326 = false;
    bool saw_sound_078 = false;
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
        } else if (tick == selected.followup_jump_tick ||
                   tick == selected.followup_jump_tick + 1) {
            buttons.set(ntsd28::InputKey28::jump);
            input = "jump_followup";
        }
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
        saw_325 |= actor->frame.action == 325;
        saw_326 |= actor->frame.action == 326;
        int audio_078_count = 0;
        if (const auto* last_tick = session.last_tick()) {
            for (const auto& event : last_tick->audio_events) {
                std::string path = event.resource_path;
                std::replace(path.begin(), path.end(), '\\', '/');
                if (path == "data/078.wav") ++audio_078_count;
            }
        }
        saw_sound_078 |= audio_078_count > 0;
        rows << tick << ',' << input << ',' << world->input_update_phase_4a0b90()
             << ',' << actor->frame.action << ',' << actor->current_mp
             << ',' << static_cast<int>(actor->input.combo_state[1])
             << ',' << actor->input.pending[ntsd28::InputKey28::jump]
             << ',' << actor->input.current[ntsd28::InputKey28::jump]
             << ',' << actor->input.previous[ntsd28::InputKey28::jump]
             << ',' << audio_078_count
             << '\n';
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
    std::cout << selected.name << ": " << recorder.row_count()
              << " ticks, " << bytes.size() << " LFR bytes"
              << ", action325=" << saw_325 << ", action326=" << saw_326
              << ", audio078=" << saw_sound_078 << '\n';
    // Action 325 has wait:0 and may advance to 326 inside this same tick.
    if (!saw_326 || !saw_sound_078) {
        error = "natural follow-up Jump did not finish on 326 with audio078";
        return false;
    }
    return true;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: orasengan_jump_lfr_probe <decoded_dat> <vfs> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    std::filesystem::create_directories(output);
    constexpr std::array<Case, 1> cases{{
        {"jump_first253", 34},
    }};
    for (const auto& selected : cases) {
        std::string error;
        if (!record_case(argv[1], argv[2], output, selected, error)) {
            std::cerr << selected.name << ": " << error << '\n';
            return 3;
        }
    }
    return 0;
}
