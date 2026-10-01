#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: returning_group_lfr_probe <formal_runtime> <new_output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[2]);
    const auto csv_path = output / "source-host-rows.csv";
    const auto lfr_path = output / "source-packets.lfr";
    if (std::filesystem::exists(csv_path) || std::filesystem::exists(lfr_path)) {
        std::cerr << "refusing to overwrite existing diagnostic output\n";
        return 3;
    }
    std::filesystem::create_directories(output);

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 56;
    config.enemy_id = 304;
    config.background_id = 23;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 survivor;
    survivor.slot = 0;
    survivor.object_id = 56;
    survivor.x = 500;
    survivor.z = 650;
    survivor.hp = 500;
    survivor.mp = 500;
    survivor.team = 1;
    ntsd28_playable::CombatantConfig28 emitter = survivor;
    emitter.slot = 1;
    emitter.object_id = 304;
    emitter.x = 900;
    emitter.team = 2;
    emitter.action = 11;
    config.combatants = {survivor, emitter};

    const std::filesystem::path runtime(argv[1]);
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
    csv << "host,world_tick,timer,timer_after,winner,living_groups,oid56_group2,emitter_action\n";
    int first_group2_host = -1;
    int timer_before_return = -1;
    int first_paused_host = -1;
    for (int host = 1; host <= 12; ++host) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "capture host " << host << ": " << error << '\n';
            return 7;
        }
        const auto* world = session.world();
        if (world == nullptr) return 8;
        int spawned_group2 = 0;
        for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots;
             ++slot) {
            const auto* entity = world->entity(slot);
            if (entity != nullptr && entity->object_id == 56 &&
                entity->object_type == 0 && entity->battle_group == 2 &&
                entity->current_hp > 0) {
                ++spawned_group2;
            }
        }
        const auto& flow = session.battle_flow();
        const auto* live_emitter = world->entity(1);
        csv << host << ',' << world->sequence() << ',' << flow.timer << ','
            << flow.timer_after << ',' << flow.outcome.winner_group << ','
            << flow.outcome.living_groups.size() << ',' << spawned_group2 << ','
            << (live_emitter == nullptr ? -1 : live_emitter->frame.action)
            << '\n';
        if (first_group2_host < 0 && spawned_group2 > 0) {
            first_group2_host = host;
            timer_before_return = flow.timer;
        }
        if (first_group2_host >= 0 && host > first_group2_host &&
            flow.outcome.living_groups.size() == 2 &&
            flow.outcome.winner_group == -1 &&
            flow.timer == timer_before_return) {
            first_paused_host = host;
        }
    }
    csv.close();
    if (!csv) return 9;
    if (first_group2_host < 2 || timer_before_return <= 0 ||
        first_paused_host <= first_group2_host) {
        std::cerr << "natural returning-group timer gate not reached: born="
                  << first_group2_host << " timer=" << timer_before_return
                  << " paused=" << first_paused_host << '\n';
        return 10;
    }
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << "finish recording: " << error << '\n';
        return 11;
    }
    std::ofstream lfr(lfr_path, std::ios::binary);
    if (!lfr) return 12;
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 13;
    std::cout << "born=" << first_group2_host
              << " timer_before_return=" << timer_before_return
              << " paused=" << first_paused_host
              << " recorded=" << recorder.row_count() << '\n';
    return 0;
}
