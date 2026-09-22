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
        const auto freeze_projection = [](const ntsd28_playable::GameSession28& session) {
            std::ostringstream out;
            out << std::setprecision(17);
            for (std::size_t slot = 0;
                 slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
                const auto* entity = session.world()->entity(slot);
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
            const auto rng = session.world()->random().state();
            out << "rng:" << rng.crt_state << ',' << rng.crt_calls << ','
                << rng.synchronized.counter << ',' << rng.synchronized.index
                << ',' << rng.synchronized.calls;
            return out.str();
        };
        const auto before_transition = freeze_projection(mode4);
        mode4.step();
        std::cout << "mode4=winner" << mode4.battle_flow().outcome.winner_group
                  << ",timer" << mode4.battle_flow().timer
                  << ",transition" << mode4.battle_flow().transition_state
                  << '\n';
        if (mode4.battle_flow().timer != 350 ||
            mode4.battle_flow().transition_state != 202 ||
            mode4.last_tick() || freeze_projection(mode4) != before_transition)
            throw std::runtime_error("mode4 transition mismatch");
        mode4.step();
        if (mode4.battle_flow().transition_state != 202 ||
            mode4.last_tick() || freeze_projection(mode4) != before_transition)
            throw std::runtime_error("mode4 post-transition world mutated");
        std::cout << "mode4-freeze=349->350->next,entities+dual-rng-stable,"
                  << "lastTick=null,projectionHash="
                  << std::hex << std::hash<std::string>{}(before_transition)
                  << std::dec << '\n';

        auto ordinary_config = timing_config;
        ordinary_config.battle_scene_only_loop = false;
        ordinary_config.battle_mode = 0;
        ntsd28_playable::GameSession28 ordinary(retained, complete);
        std::string ordinary_error;
        if (!ordinary.initialize(ordinary_config, ordinary_error))
            throw std::runtime_error("ordinary initialize: " + ordinary_error);
        auto* ordinary_victim = ordinary.world()->entity(1);
        if (!ordinary_victim)
            throw std::runtime_error("ordinary victim missing");
        ordinary_victim->current_hp = 0;
        ordinary_victim->revive_lives_30c = 1;
        for (int tick = 1; tick <= 349; ++tick) ordinary.step();
        if (ordinary.battle_flow().timer != 349 || !ordinary.last_tick())
            throw std::runtime_error("ordinary pre-transition tick missing");
        const auto ordinary_before = freeze_projection(ordinary);
        const auto* ordinary_world = ordinary.world();
        ordinary.step();
        if (ordinary.battle_flow().timer != 350 ||
            ordinary.battle_flow().transition_state != 2 ||
            ordinary.last_tick() || ordinary.in_selection() ||
            ordinary.frontend_scene() != ntsd28_playable::FrontendScene28::result ||
            freeze_projection(ordinary) != ordinary_before)
            throw std::runtime_error("ordinary transition tick mismatch");
        ordinary.step();
        if (ordinary.battle_flow().transition_state != 1 ||
            !ordinary.in_selection() || ordinary.world() != ordinary_world ||
            ordinary.frontend_scene() != ntsd28_playable::FrontendScene28::selection ||
            ordinary.last_tick())
            throw std::runtime_error("ordinary upper-selection entry mismatch");
        std::cout << "ordinary=350:result+transition2+noCombat;"
                  << "next:selection+transition1+sameWorld\n";

        auto battle_only_config = ordinary_config;
        battle_only_config.battle_scene_only_loop = true;
        ntsd28_playable::GameSession28 battle_only(retained, complete);
        std::string battle_only_error;
        if (!battle_only.initialize(battle_only_config, battle_only_error))
            throw std::runtime_error("battle-only initialize: " + battle_only_error);
        auto* battle_only_victim = battle_only.world()->entity(1);
        if (!battle_only_victim)
            throw std::runtime_error("battle-only victim missing");
        battle_only_victim->current_hp = 0;
        battle_only_victim->revive_lives_30c = 1;
        for (int tick = 1; tick <= 349; ++tick) battle_only.step();
        if (battle_only.battle_flow().timer != 349 || !battle_only.last_tick())
            throw std::runtime_error("battle-only pre-transition tick missing");
        const auto battle_only_before = freeze_projection(battle_only);
        battle_only.step();
        if (battle_only.battle_flow().timer != 350 ||
            battle_only.battle_flow().transition_state != 2 ||
            battle_only.last_tick() || battle_only.in_selection() ||
            freeze_projection(battle_only) != battle_only_before)
            throw std::runtime_error("battle-only transition tick mismatch");
        battle_only.world()->restore_input_update_phase_4a0b90(1);
        const auto random_before_rematch = battle_only.world()->random().state();
        const int input_phase_before_rematch =
            battle_only.world()->input_update_phase_4a0b90();
        if (battle_only.config().bgm_selection_49f18c != 0 ||
            input_phase_before_rematch != 1)
            throw std::runtime_error("rematch random/phase fixture precondition");
        battle_only.step();
        const auto random_after_rematch = battle_only.world()->random().state();
        const int input_phase_after_rematch =
            battle_only.world()->input_update_phase_4a0b90();
        std::cout << "rematch-rng=crt:" << random_before_rematch.crt_state
                  << '/' << random_before_rematch.crt_calls << "->"
                  << random_after_rematch.crt_state << '/'
                  << random_after_rematch.crt_calls
                  << ",sync:" << random_before_rematch.synchronized.counter
                  << '/' << random_before_rematch.synchronized.index
                  << '/' << random_before_rematch.synchronized.calls << "->"
                  << random_after_rematch.synchronized.counter << '/'
                  << random_after_rematch.synchronized.index << '/'
                  << random_after_rematch.synchronized.calls
                  << ",lastSite=" << std::hex
                  << random_after_rematch.synchronized.last_call_site
                  << std::dec << ",inputPhase="
                  << input_phase_before_rematch << "->"
                  << input_phase_after_rematch << '\n';
        if (random_after_rematch.crt_state !=
                random_before_rematch.crt_state ||
            random_after_rematch.crt_calls !=
                random_before_rematch.crt_calls ||
            random_after_rematch.synchronized.calls !=
                random_before_rematch.synchronized.calls + 1 ||
            random_after_rematch.synchronized.last_call_site !=
                0x004021E0u ||
            random_after_rematch.synchronized.table !=
                random_before_rematch.synchronized.table ||
            input_phase_after_rematch != input_phase_before_rematch ||
            battle_only.selected_bgm_virtual_path().empty())
            throw std::runtime_error("first rematch RNG/input phase mismatch");
        const auto* recreated_victim = battle_only.world()->entity(1);
        if (!recreated_victim || recreated_victim->current_hp <= 0 ||
            battle_only.battle_flow().phase != ntsd28::BattleFlowPhase28::active ||
            battle_only.in_selection() ||
            battle_only.frontend_scene() != ntsd28_playable::FrontendScene28::battle)
            throw std::runtime_error("battle-only rematch was not recreated");
        std::cout << "battle-only=350:result+transition2+noCombat;"
                  << "next:recreatedBattle+victimHp"
                  << recreated_victim->current_hp << '\n';
        const auto* second_world = battle_only.world();
        const bool second_cycle_loop_enabled =
            battle_only.config().battle_scene_only_loop;
        auto* second_victim = battle_only.world()->entity(1);
        if (!second_victim)
            throw std::runtime_error("second-cycle victim missing");
        second_victim->current_hp = 0;
        second_victim->revive_lives_30c = 1;
        for (int tick = 1; tick <= 349; ++tick) battle_only.step();
        if (battle_only.battle_flow().timer != 349 || !battle_only.last_tick())
            throw std::runtime_error("second-cycle pre-transition tick missing");
        const auto second_before = freeze_projection(battle_only);
        battle_only.step();
        std::cout << "second-cycle=loopFlag" << second_cycle_loop_enabled
                  << ",timer" << battle_only.battle_flow().timer
                  << ",transition" << battle_only.battle_flow().transition_state
                  << ",scene" << static_cast<int>(battle_only.frontend_scene())
                  << '\n';
        if (battle_only.battle_flow().timer != 350 ||
            battle_only.battle_flow().transition_state != 2 ||
            battle_only.last_tick() || battle_only.in_selection() ||
            freeze_projection(battle_only) != second_before)
            throw std::runtime_error("second-cycle transition tick mismatch");
        battle_only.step();
        const bool second_cycle_selected = battle_only.in_selection();
        const bool second_cycle_same_world = battle_only.world() == second_world;
        const auto* second_final_victim = battle_only.world()->entity(1);
        std::cout << "second-cycle-next=loopFlag"
                  << battle_only.config().battle_scene_only_loop
                  << ",transition" << battle_only.battle_flow().transition_state
                  << ",scene" << static_cast<int>(battle_only.frontend_scene())
                  << ",selected" << second_cycle_selected
                  << ",sameWorld" << second_cycle_same_world
                  << ",victimHp"
                  << (second_final_victim ? second_final_victim->current_hp : -999)
                  << '\n';
        if (second_cycle_loop_enabled || !second_cycle_selected ||
            !second_cycle_same_world ||
            battle_only.battle_flow().transition_state != 1 ||
            battle_only.frontend_scene() !=
                ntsd28_playable::FrontendScene28::selection ||
            !second_final_victim || second_final_victim->current_hp != 0 ||
            battle_only.last_tick())
            throw std::runtime_error("second-cycle upper selection mismatch");
        std::cout << "PASS 6/6 groups plus timing, combat-lethal and mode4 full-driver\n";
        return 0;
    } catch (const std::exception& error) {
        std::cerr << "FAIL " << error.what() << '\n';
        return 1;
    }
}
