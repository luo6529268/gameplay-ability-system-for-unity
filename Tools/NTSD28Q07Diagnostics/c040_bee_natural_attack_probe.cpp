#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

namespace {

struct Schedule {
    int second;
    int third;
};

}  // namespace

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: c040_bee_natural_attack_probe <formal_root> <new_output_directory>\n";
        return 2;
    }
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);
    std::ofstream rows(output / "source-ticks.csv", std::ios::binary);
    std::ofstream summary(output / "summary.csv", std::ios::binary);
    std::ofstream rng(output / "source-rng.csv", std::ios::binary);
    if (!rows || !summary || !rng) return 4;
    rows << "second_start,third_start,tick,attack,input_phase,action,"
            "tick_action,edge_attack,combo_aj,actor_x,actor_hp,target_hp\n"
         << std::setprecision(17);
    summary << "second_start,third_start,first_65,first_63,first_66,"
               "first_69,first_70,first_73,lfr\n";
    rng << "second_start,third_start,tick,crt_state,crt_calls,"
           "custom_counter,custom_index,custom_calls\n";

    std::vector<Schedule> schedules;
    schedules.push_back({-1, -1});
    for (int second = 7; second <= 14; ++second) {
        for (int third = 16; third <= 30; ++third) {
            schedules.push_back({second, third});
        }
    }
    const auto runtime_root = std::filesystem::path(argv[1]) /
                              "resources" / "runtime";
    int action_73_cases = 0;
    bool emitted_replay = false;
    for (const auto schedule : schedules) {
        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 0u;
        config.character_id = 75;
        config.enemy_id = 97;
        config.background_id = 1;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;
        ntsd28_playable::CombatantConfig28 actor;
        actor.slot = 0;
        actor.object_id = 75;
        actor.x = 500;
        actor.y = 0;
        actor.z = 400;
        actor.hp = actor.base_hp = actor.mp = 500;
        actor.team = 1;
        actor.action = 0;
        auto target = actor;
        target.slot = 1;
        target.object_id = 97;
        target.x = 1200;
        target.team = 2;
        config.combatants = {actor, target};

        ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "initialize: " << error << '\n';
            return 5;
        }
        ntsd28_playable::GameSessionLfr28 recorder;
        if (!recorder.begin(session, error)) return 6;
        int first_65 = -1;
        int first_63 = -1;
        int first_66 = -1;
        int first_69 = -1;
        int first_70 = -1;
        int first_73 = -1;
        for (int tick = 1; tick <= 40; ++tick) {
            const bool attack = tick <= 2 ||
                (schedule.second > 0 &&
                 (tick == schedule.second || tick == schedule.second + 1)) ||
                (schedule.third > 0 &&
                 (tick == schedule.third || tick == schedule.third + 1));
            ntsd28::InputButtons28 buttons;
            if (attack) buttons.set(ntsd28::InputKey28::attack);
            session.set_input(0, buttons);
            session.set_input(1, ntsd28::InputButtons28{});
            session.step();
            if (!recorder.capture_after_step(session, error)) return 7;
            const auto* world = session.world();
            const auto* bee = world == nullptr ? nullptr : world->entity(0);
            const auto* guy = world == nullptr ? nullptr : world->entity(1);
            if (!bee || !guy) return 8;
            const int action = bee->frame.action;
            if (action == 65 && first_65 < 0) first_65 = tick;
            if (action == 63 && first_63 < 0) first_63 = tick;
            if (action == 66 && first_66 < 0) first_66 = tick;
            if (action == 69 && first_69 < 0) first_69 = tick;
            if (action == 70 && first_70 < 0) first_70 = tick;
            if (action == 73 && first_73 < 0) first_73 = tick;
            rows << schedule.second << ',' << schedule.third << ',' << tick
                 << ',' << attack << ',' << world->input_update_phase_4a0b90()
                 << ',' << action << ',' << bee->frame.tick_action_snapshot
                 << ',' << bee->input.edge_window[
                        static_cast<std::size_t>(ntsd28::InputKey28::attack)]
                 << ',' << static_cast<int>(bee->input.combo_state[6])
                 << ',' << bee->position.x << ',' << bee->current_hp << ','
                 << guy->current_hp << '\n';
            const auto state = world->random().state();
            rng << schedule.second << ',' << schedule.third << ',' << tick
                << ',' << state.crt_state << ',' << state.crt_calls << ','
                << state.synchronized.counter << ',' << state.synchronized.index
                << ',' << state.synchronized.calls << '\n';
        }
        bool emitted = false;
        if (first_73 >= 0) {
            ++action_73_cases;
            if (!emitted_replay) {
                std::vector<std::uint8_t> bytes;
                if (!recorder.finish_to_memory(session, bytes, error)) return 9;
                std::ofstream lfr(output / "first-action73.lfr", std::ios::binary);
                if (!lfr) return 10;
                lfr.write(reinterpret_cast<const char*>(bytes.data()),
                          static_cast<std::streamsize>(bytes.size()));
                lfr.close();
                if (!lfr) return 11;
                emitted_replay = true;
                emitted = true;
            }
        }
        summary << schedule.second << ',' << schedule.third << ','
                << first_65 << ',' << first_63 << ',' << first_66 << ','
                << first_69 << ',' << first_70 << ',' << first_73 << ','
                << emitted << '\n';
    }
    rows.close();
    summary.close();
    rng.close();
    if (!rows || !summary || !rng) return 12;
    std::cout << "cases=" << schedules.size()
              << " action73_cases=" << action_73_cases << '\n';
    return 0;
}
