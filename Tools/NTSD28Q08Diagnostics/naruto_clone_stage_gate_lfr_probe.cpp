#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: naruto_clone_stage_gate_lfr_probe <formal_runtime_root> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path runtime_root(argv[1]);
    const std::filesystem::path output_root(argv[2]);
    const auto lfr_path = output_root / "naruto_clone_stage_gate_source_packets.lfr";
    const auto csv_path = output_root / "naruto_clone_stage_gate_source_ticks.csv";
    if (std::filesystem::exists(lfr_path) || std::filesystem::exists(csv_path)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }
    if (!std::filesystem::is_directory(output_root)) {
        std::cerr << "output directory does not exist\n";
        return 4;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.battle_mode = 0;
    config.background_id = 23;
    config.difficulty_level_4a0c30 = 1;
    ntsd28_playable::CombatantConfig28 naruto;
    naruto.slot = 0;
    naruto.object_id = 2;
    naruto.team = 1;
    naruto.x = 0;
    naruto.z = 650;
    naruto.hp = naruto.base_hp = 500;
    naruto.mp = 500;
    naruto.action = 121;
    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 1;
    lee.object_id = 7;
    lee.team = 2;
    lee.x = 1200;
    lee.z = 650;
    lee.hp = lee.base_hp = 500;
    lee.mp = 500;
    lee.facing = true;
    config.combatants = {naruto, lee};

    ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
    std::string error;
    if (!session.initialize(config, error)) {
        std::cerr << "initialize: " << error << '\n';
        return 5;
    }
    if (session.config().selected_mode_stage_gate_50 != 1) {
        std::cerr << "selected mode stage gate is not the frozen value 1\n";
        return 6;
    }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) {
        std::cerr << "record begin: " << error << '\n';
        return 7;
    }
    std::ofstream rows(csv_path, std::ios::binary);
    if (!rows) return 8;
    rows << "tick,selected_mode_stage_gate_50,slot,oid,x,action,crt_state,crt_calls,synchronized_counter,synchronized_index,synchronized_calls\n";
    for (int tick = 1; tick <= 3; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "record tick " << tick << ": " << error << '\n';
            return 9;
        }
        const auto* world = session.world();
        if (world == nullptr) return 10;
        const auto random = world->random().state();
        for (std::size_t slot : {0u, 1u, 50u}) {
            const auto* entity = world->entity(slot);
            rows << tick << ',' << session.config().selected_mode_stage_gate_50 << ','
                 << slot << ',' << (entity == nullptr ? -1 : entity->object_id)
                 << ',' << (entity == nullptr ? 0.0 : entity->position.precise_x)
                 << ',' << (entity == nullptr ? -1 : entity->frame.action)
                 << ',' << random.crt_state << ',' << random.crt_calls
                 << ',' << random.synchronized.counter
                 << ',' << random.synchronized.index
                 << ',' << random.synchronized.calls << '\n';
        }
    }
    rows.close();
    if (!rows) return 11;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << "record finish: " << error << '\n';
        return 12;
    }
    std::ofstream encoded(lfr_path, std::ios::binary);
    if (!encoded) return 13;
    encoded.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
    encoded.close();
    if (!encoded) return 14;
    std::cout << "recorded " << recorder.row_count() << " ticks, "
              << bytes.size() << " bytes\n";
    return 0;
}
