#include "ntsd28_playable/game_session_lfr.h"

#include <array>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

struct Schedule {
    int attack_tick;
    int attack_length;
    int defend_tick;
    int defend_length;
};

bool run_schedule(const std::filesystem::path& decoded_dat,
                  const std::filesystem::path& vfs,
                  const Schedule& selected,
                  std::ofstream& rows,
                  std::vector<std::uint8_t>& successful_lfr,
                  std::string& error) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 7;
    config.enemy_id = 2;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 0;
    lee.object_id = 7;
    lee.x = 500;
    lee.z = 650;
    lee.hp = 500;
    lee.mp = 500;
    lee.team = 1;
    lee.action = 0;
    ntsd28_playable::CombatantConfig28 opponent;
    opponent.slot = 1;
    opponent.object_id = 2;
    opponent.x = 1200;
    opponent.z = 650;
    opponent.hp = 500;
    opponent.mp = 500;
    opponent.team = 2;
    opponent.action = 0;
    config.combatants = {lee, opponent};

    ntsd28_playable::GameSession28 session(decoded_dat, vfs);
    if (!session.initialize(config, error)) return false;
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) return false;
    bool saw_frame_164 = false;
    bool saw_owned_child = false;
    for (int tick = 1; tick <= 45; ++tick) {
        ntsd28::InputButtons28 buttons;
        const bool attack = tick >= selected.attack_tick &&
                            tick < selected.attack_tick + selected.attack_length;
        const bool defend = tick >= selected.defend_tick &&
                            tick < selected.defend_tick + selected.defend_length;
        if (attack) buttons.set(ntsd28::InputKey28::attack);
        if (defend) buttons.set(ntsd28::InputKey28::defend);
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) return false;
        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        if (actor == nullptr) {
            error = "Lee slot0 disappeared during diagnostic";
            return false;
        }
        if (actor->frame.action == 164) saw_frame_164 = true;
        int owned_child_count = 0;
        for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots;
             ++slot) {
            const auto* entity = world->entity(slot);
            if (entity != nullptr && entity->object_id == 204 &&
                entity->owner_slot == 0) {
                ++owned_child_count;
            }
        }
        if (owned_child_count != 0) saw_owned_child = true;
        rows << selected.attack_tick << ',' << selected.attack_length << ','
             << selected.defend_tick << ',' << selected.defend_length << ','
             << tick << ',' << (attack ? 1 : 0) << ',' << (defend ? 1 : 0)
             << ',' << world->input_update_phase_4a0b90() << ','
             << actor->frame.action << ',' << actor->current_mp << ','
             << static_cast<int>(actor->input.key_history[3]) << ','
             << static_cast<int>(actor->input.key_history[4]) << ','
             << static_cast<int>(actor->input.combo_state[7]) << ','
             << owned_child_count << '\n';
    }
    if (saw_frame_164 && saw_owned_child && successful_lfr.empty()) {
        if (!recorder.finish_to_memory(session, successful_lfr, error)) return false;
        std::cout << "natural frame164 and owned OID204: J@"
                  << selected.attack_tick << "x" << selected.attack_length
                  << " L@" << selected.defend_tick << "x"
                  << selected.defend_length << ", " << successful_lfr.size()
                  << " LFR bytes\n";
    }
    return true;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: lee_natural_child_lfr_probe <decoded_dat> <vfs> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    std::filesystem::create_directories(output);
    const auto csv = output / "lee-jl-schedule-scan.csv";
    const auto lfr = output / "lee-natural-child.lfr";
    if (std::filesystem::exists(csv) || std::filesystem::exists(lfr)) {
        std::cerr << "refusing to overwrite diagnostic output\n";
        return 3;
    }
    std::ofstream rows(csv, std::ios::binary);
    if (!rows) return 4;
    rows << "attack_tick,attack_length,defend_tick,defend_length,tick,attack,defend,phase,actor_action,actor_mp,history3,history4,combo7,owned_oid204\n";
    std::vector<std::uint8_t> successful_lfr;
    for (const int attack_tick : {2, 4, 6, 8}) {
        for (const int attack_length : {1, 2}) {
            for (const int gap : {1, 2, 3, 4}) {
                for (const int defend_length : {1, 2}) {
                    const Schedule selected{attack_tick, attack_length,
                                            attack_tick + attack_length + gap - 1,
                                            defend_length};
                    std::string error;
                    if (!run_schedule(argv[1], argv[2], selected, rows,
                                      successful_lfr, error)) {
                        std::cerr << "source schedule failed: " << error << '\n';
                        return 5;
                    }
                    if (!successful_lfr.empty()) break;
                }
                if (!successful_lfr.empty()) break;
            }
            if (!successful_lfr.empty()) break;
        }
        if (!successful_lfr.empty()) break;
    }
    rows.close();
    if (!rows) return 6;
    if (successful_lfr.empty()) {
        std::cerr << "no bounded J,L schedule reached both frame164 and owned OID204\n";
        return 7;
    }
    std::ofstream encoded(lfr, std::ios::binary);
    if (!encoded) return 8;
    encoded.write(reinterpret_cast<const char*>(successful_lfr.data()),
                  static_cast<std::streamsize>(successful_lfr.size()));
    encoded.close();
    return encoded ? 0 : 9;
}
