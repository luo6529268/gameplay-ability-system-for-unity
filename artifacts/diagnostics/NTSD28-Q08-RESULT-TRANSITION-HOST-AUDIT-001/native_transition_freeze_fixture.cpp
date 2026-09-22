#include "ntsd28_playable/game_session.h"
#include "ntsd28/engine_profile.h"

#include <filesystem>
#include <functional>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <stdexcept>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: fixture <retained-root> <complete-vfs-root>\n";
        return 2;
    }
    try {
        const auto retained = std::filesystem::u8path(argv[1]);
        const auto complete = std::filesystem::u8path(argv[2]);
        const std::vector<std::vector<int>> cases{{1, 0}, {1, 5}, {1, 40},
                                                   {1, 2, 3}, {0, 5}, {1, 2}};
        const std::vector<std::vector<int>> expected{{1}, {1}, {1},
                                                      {1, 2, 3}, {}, {1, 2}};
        for (std::size_t index = 0; index < cases.size(); ++index) {
            ntsd28_playable::BattleConfig28 config;
            config.random_seed = 0x28A55A5Au;
            config.background_id = 23;
            config.battle_scene_only_loop = true;
            const int object_ids[] = {75, 19, 6};
            for (std::size_t slot = 0; slot < cases[index].size(); ++slot) {
                ntsd28_playable::CombatantConfig28 combatant;
                combatant.slot = slot;
                combatant.object_id = object_ids[slot];
                combatant.x = 420 + static_cast<int>(slot) * 120;
                combatant.z = 610;
                combatant.hp = 500;
                combatant.mp = 200;
                combatant.team = cases[index][slot];
                config.combatants.push_back(combatant);
            }
            ntsd28_playable::GameSession28 session(retained, complete);
            std::string error;
            if (!session.initialize(config, error)) {
                throw std::runtime_error("initialize case " +
                                         std::to_string(index) + ": " + error);
            }
            for (std::size_t slot = 0; slot < cases[index].size(); ++slot) {
                auto* entity = session.world()->entity(slot);
                if (!entity) throw std::runtime_error("combatant missing");
                entity->battle_group = cases[index][slot];
            }
            if (index == 5) {
                auto* revive = session.world()->entity(0);
                if (!revive) throw std::runtime_error("revive entity missing");
                revive->current_hp = 0;
                revive->revive_lives_30c = 2;
            }
            session.step();
            const auto& flow = session.battle_flow();
            const int expected_timer = expected[index].size() < 2 ? 1 : 0;
            const int expected_winner = expected[index].size() == 1
                                            ? expected[index][0] : -1;
            std::cout << "case=" << index << " groups=";
            for (const int group : flow.outcome.living_groups)
                std::cout << group << ',';
            std::cout << " winner=" << flow.outcome.winner_group
                      << " timer=" << flow.timer << '\n';
            if (flow.outcome.living_groups != expected[index] ||
                flow.outcome.winner_group != expected_winner ||
                flow.timer != expected_timer) {
                throw std::runtime_error("full-driver flow mismatch in case " +
                                         std::to_string(index));
            }
        }
        ntsd28_playable::BattleConfig28 timing_config;
        timing_config.random_seed = 0x28A55A5Au;
        timing_config.background_id = 23;
        timing_config.battle_scene_only_loop = true;
        for (std::size_t slot = 0; slot < 2; ++slot) {
            ntsd28_playable::CombatantConfig28 combatant;
            combatant.slot = slot;
            combatant.object_id = slot == 0 ? 75 : 19;
            combatant.x = 420 + static_cast<int>(slot) * 120;
            combatant.z = 610;
            combatant.hp = 500;
            combatant.mp = 200;
            combatant.team = static_cast<int>(slot) + 1;
            timing_config.combatants.push_back(combatant);
        }
        ntsd28_playable::GameSession28 timing(retained, complete);
        std::string timing_error;
        if (!timing.initialize(timing_config, timing_error))
            throw std::runtime_error("timing initialize: " + timing_error);
        timing.world()->entity(1)->battle_group = 0;
        timing.step();
        if (timing.battle_flow().timer != 1 ||
            timing.battle_flow().outcome.living_groups != std::vector<int>{1})
            throw std::runtime_error("timer start mismatch");
        timing.world()->entity(1)->battle_group = 2;
        timing.step();
        if (timing.battle_flow().timer != 2 ||
            timing.battle_flow().outcome.living_groups != std::vector<int>{1})
            throw std::runtime_error("started timer or winner was reset");
        while (timing.battle_flow().timer < 79) timing.step();
        timing.step();
        if (timing.battle_flow().timer != 80 ||
            !timing.battle_flow().end_signal_emitted)
            throw std::runtime_error("timer 80 signal mismatch");
        while (timing.battle_flow().timer < 100) timing.step();
        timing.step();
        if (timing.battle_flow().timer != 101 ||
            !timing.battle_flow().result_record_created)
            throw std::runtime_error("timer 101 record mismatch");
        while (timing.battle_flow().timer < 143) timing.step();
        ntsd28::InputButtons28 attack;
        attack.set(ntsd28::InputKey28::attack);
        timing.set_p1_input(attack);
        timing.step();
        if (timing.battle_flow().timer != 350 ||
            !timing.battle_flow().transition_started ||
            timing.battle_flow().timer_after != 0)
            throw std::runtime_error("timer 144 continue mismatch");
        std::cout << "timing=start1,latch2,signal80,record101,continue144to350\n";

        ntsd28_playable::BattleConfig28 lethal_config;
        lethal_config.random_seed = 0x28A55A5Au;
        lethal_config.character_id = 2;
        lethal_config.enemy_id = 510;
        lethal_config.background_id = 23;
        lethal_config.battle_mode = 1;
        lethal_config.battle_scene_only_loop = true;
        lethal_config.p1_x = 500;
        lethal_config.p1_z = 600;
        lethal_config.p1_hp = 500;
        lethal_config.p1_mp = 200;
        lethal_config.p1_team = 1;
        lethal_config.p2_x = 535;
        lethal_config.p2_z = 600;
        lethal_config.p2_hp = 20;
        lethal_config.p2_mp = 200;
        lethal_config.p2_team = 2;
        ntsd28_playable::GameSession28 lethal(retained, complete);
        std::string lethal_error;
        if (!lethal.initialize(lethal_config, lethal_error))
            throw std::runtime_error("lethal initialize: " + lethal_error);
        auto* victim = lethal.world()->entity(1);
        if (!victim) throw std::runtime_error("lethal victim missing");
        victim->revive_lives_30c = 1;
        int lethal_tick = -1;
        int lethal_timer = -1;
        for (int tick = 1; tick <= 90; ++tick) {
            ntsd28::InputButtons28 input;
            if (tick == 5 || tick == 6)
                input.set(ntsd28::InputKey28::attack);
            lethal.set_p1_input(input);
            lethal.step();
            victim = lethal.world()->entity(1);
            if (victim && victim->current_hp <= 0) {
                lethal_tick = tick;
                lethal_timer = lethal.battle_flow().timer;
                break;
            }
        }
        if (lethal_tick < 0)
            throw std::runtime_error("combat input did not cause lethal hit");
        lethal.set_p1_input({});
        lethal.step();
        std::cout << "lethal=tick" << lethal_tick
                  << ",sameTimer" << lethal_timer
                  << ",nextTimer" << lethal.battle_flow().timer << '\n';
        if (lethal_timer != 0 || lethal.battle_flow().timer != 1)
            throw std::runtime_error("combat lethal next-tick timer mismatch");

        auto mode4_config = timing_config;
        mode4_config.battle_mode = 4;
        ntsd28_playable::GameSession28 mode4(retained, complete);
        std::string mode4_error;
        if (!mode4.initialize(mode4_config, mode4_error))
            throw std::runtime_error("mode4 initialize: " + mode4_error);
        auto* mode4_victim = mode4.world()->entity(1);
        if (!mode4_victim) throw std::runtime_error("mode4 victim missing");
        mode4_victim->current_hp = 0;
        mode4_victim->revive_lives_30c = 1;
        mode4.step();
        if (mode4.battle_flow().timer != 1 ||
            mode4.battle_flow().outcome.winner_group != 1)
            throw std::runtime_error("mode4 first terminal tick mismatch");
        for (int tick = 2; tick <= 349; ++tick) mode4.step();
        if (mode4.battle_flow().timer != 349 || !mode4.last_tick())
            throw std::runtime_error("mode4 pre-transition tick missing");
        const auto freeze_projection = [&mode4]() {
            std::ostringstream out;
            out << std::setprecision(17);
            for (std::size_t slot = 0;
                 slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
                const auto* entity = mode4.world()->entity(slot);
                if (!entity) continue;
                out << slot << ':' << entity->object_id << ','
                    << entity->frame.action << ',' << entity->frame.frame_counter
                    << ',' << entity->position.x << ',' << entity->position.y
                    << ',' << entity->position.z << ','
                    << entity->position.precise_x << ','
                    << entity->position.precise_y << ','
                    << entity->position.precise_z << ','
                    << entity->motion.x << ',' << entity->motion.y << ','
                    << entity->motion.z << ',' << entity->current_hp << ','
                    << entity->current_mp << ';';
            }
            const auto rng = mode4.world()->random().state();
            out << "rng:" << rng.crt_state << ',' << rng.crt_calls << ','
                << rng.synchronized.counter << ',' << rng.synchronized.index
                << ',' << rng.synchronized.calls;
            return out.str();
        };
        const auto before_transition = freeze_projection();
        mode4.step();
        std::cout << "mode4=winner" << mode4.battle_flow().outcome.winner_group
                  << ",timer" << mode4.battle_flow().timer
                  << ",transition" << mode4.battle_flow().transition_state
                  << '\n';
        if (mode4.battle_flow().timer != 350 ||
            mode4.battle_flow().transition_state != 202 ||
            mode4.last_tick() || freeze_projection() != before_transition)
            throw std::runtime_error("mode4 transition mismatch");
        mode4.step();
        if (mode4.battle_flow().transition_state != 202 ||
            mode4.last_tick() || freeze_projection() != before_transition)
            throw std::runtime_error("mode4 post-transition world mutated");
        std::cout << "mode4-freeze=349->350->next,entities+dual-rng-stable,"
                  << "lastTick=null,projectionHash="
                  << std::hex << std::hash<std::string>{}(before_transition)
                  << std::dec << '\n';
        std::cout << "PASS 6/6 groups plus timing, combat-lethal and mode4 full-driver\n";
        return 0;
    } catch (const std::exception& error) {
        std::cerr << "FAIL " << error.what() << '\n';
        return 1;
    }
}
