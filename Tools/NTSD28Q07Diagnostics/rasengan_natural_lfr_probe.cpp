#include "ntsd28_playable/game_session_lfr.h"

#include <array>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

struct Case {
    const char* name;
    int attack_tick;
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
    rows << "tick,input,phase,actor_action,actor_mp,combo1,pending_attack,current_attack,previous_attack\n";
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
        } else if (tick == selected.attack_tick ||
                   tick == selected.attack_tick + 1) {
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
            error = "Naruto slot0 disappeared during diagnostic";
            return false;
        }
        rows << tick << ',' << input << ',' << world->input_update_phase_4a0b90()
             << ',' << actor->frame.action << ',' << actor->current_mp
             << ',' << static_cast<int>(actor->input.combo_state[1])
             << ',' << actor->input.pending[ntsd28::InputKey28::attack]
             << ',' << actor->input.current[ntsd28::InputKey28::attack]
             << ',' << actor->input.previous[ntsd28::InputKey28::attack]
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
              << " ticks, " << bytes.size() << " LFR bytes\n";
    return true;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: rasengan_natural_lfr_probe <decoded_dat> <vfs> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    std::filesystem::create_directories(output);
    constexpr std::array<Case, 3> cases{{
        {"first253", 34}, {"second253", 35}, {"after254", 36},
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
