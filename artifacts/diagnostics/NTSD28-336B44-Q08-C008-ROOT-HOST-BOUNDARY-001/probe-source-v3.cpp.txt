#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 4 || (std::string(argv[3]) != "held" &&
                      std::string(argv[3]) != "natural")) {
        std::cerr << "usage: result_continue_host_lfr_probe <formal_runtime> <new_output> <held|natural>\n";
        return 2;
    }
    const bool held_case = std::string(argv[3]) == "held";
    const std::filesystem::path runtime(argv[1]);
    const std::filesystem::path output(argv[2]);
    const auto csv_path = output / "source-host-rows.csv";
    const auto lfr_path = output / "source-packets.lfr";
    if (std::filesystem::exists(csv_path) || std::filesystem::exists(lfr_path)) {
        std::cerr << "refusing to overwrite diagnostic output\n";
        return 3;
    }
    std::filesystem::create_directories(output);
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 2;
    config.enemy_id = 2;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 2;
    actor.x = 500;
    actor.z = 650;
    actor.hp = 500;
    actor.mp = 500;
    actor.team = 1;
    ntsd28_playable::CombatantConfig28 target = actor;
    target.slot = 1;
    target.x = 525;
    target.hp = 10;
    target.team = 2;
    config.combatants = {actor, target};
    ntsd28_playable::GameSession28 session(runtime, runtime);
    std::string error;
    if (!session.initialize(config, error)) {
        std::cerr << "initialize: " << error << '\n';
        return 4;
    }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) {
        std::cerr << "record begin: " << error << '\n';
        return 5;
    }
    std::ofstream csv(csv_path, std::ios::binary);
    if (!csv) return 6;
    csv << "host_step,world_tick,attack,sampled_attack,phase,timer,timer_after,transition,transition_started,upper_entered,winner,actor_action,target_action,target_hp\n";
    bool finished_recording = false;
    bool saw_held_350 = false;
    bool saw_transition = false;
    int knockout_host = -1;
    int lfr_last_host = -1;
    int transition_host = -1;
    bool continue_held = false;
    std::vector<std::uint8_t> encoded;
    for (int host = 1; host <= 360; ++host) {
        ntsd28::InputButtons28 input;
        if (held_case && session.battle_flow().timer >= 141)
            continue_held = true;
        const bool attack = host == 2 || continue_held;
        if (attack) input.set(ntsd28::InputKey28::attack);
        session.set_input(0, input);
        session.set_input(1, {});
        session.step();
        const auto* world = session.world();
        const auto* first = world == nullptr ? nullptr : world->entity(0);
        const auto* second = world == nullptr ? nullptr : world->entity(1);
        if (world == nullptr || first == nullptr || second == nullptr) {
            std::cerr << "required entity disappeared at host " << host << '\n';
            return 7;
        }
        const auto& flow = session.battle_flow();
        csv << host << ',' << world->sequence() << ',' << (attack ? 1 : 0) << ','
            << (first->input.current[ntsd28::InputKey28::attack] ? 1 : 0) << ','
            << static_cast<int>(flow.phase) << ',' << flow.timer << ','
            << flow.timer_after << ',' << flow.transition_state << ','
            << (flow.transition_started ? 1 : 0) << ','
            << (flow.upper_scene_entered ? 1 : 0) << ','
            << flow.outcome.winner_group << ',' << first->frame.action << ','
            << second->frame.action << ',' << second->current_hp << '\n';
        if (knockout_host < 0 && second->current_hp <= 0 &&
            !world->knockout_events().empty()) knockout_host = host;
        if (!finished_recording) {
            if (!recorder.capture_after_step(session, error)) {
                std::cerr << "record host " << host << ": " << error << '\n';
                return 8;
            }
            const bool last_advancing_row = held_case
                ? attack && flow.timer == 350 && flow.transition_state == 0
                : flow.timer == 349;
            if (last_advancing_row) {
                if (!recorder.finish_to_memory(session, encoded, error)) {
                    std::cerr << "record finish: " << error << '\n';
                    return 9;
                }
                finished_recording = true;
                lfr_last_host = host;
                saw_held_350 = held_case;
            }
        }
        if (flow.transition_started && flow.transition_state == 2) {
            saw_transition = true;
            transition_host = host;
        }
        if (saw_transition && host == transition_host + 1) break;
    }
    csv.close();
    if (!csv || !finished_recording || !saw_transition || knockout_host < 0 ||
        (held_case && !saw_held_350)) {
        std::cerr << "required bounded natural KO/result boundary was not reached\n";
        return 10;
    }
    std::ofstream lfr(lfr_path, std::ios::binary);
    if (!lfr) return 11;
    lfr.write(reinterpret_cast<const char*>(encoded.data()),
              static_cast<std::streamsize>(encoded.size()));
    lfr.close();
    if (!lfr) return 12;
    std::cout << "case=" << argv[3] << " ko_host=" << knockout_host
              << " lfr_last_host=" << lfr_last_host
              << " root_terminal_transition_host=" << transition_host
              << " lfr_bytes=" << encoded.size() << '\n';
    return 0;
}
