#include "ntsd28/battle_world.h"
#include "ntsd28_playable/game_session.h"
#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

namespace {

ntsd28_playable::BattleConfig28 make_config(int initial_z) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 0;
    config.character_id = 92;
    config.enemy_id = 2;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.p2_native_ai = false;

    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 92;
    actor.x = 500;
    actor.z = initial_z;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = 580;

    auto opponent = actor;
    opponent.slot = 1;
    opponent.object_id = 2;
    opponent.x = 1200;
    opponent.team = 2;
    opponent.action = 0;
    config.combatants = {actor, opponent};
    return config;
}

bool write_row(std::ofstream& output, int tick,
               const ntsd28::BattleWorld28& world) {
    const auto* actor = world.entity(0);
    if (actor == nullptr || actor->object_id != 92) return false;
    output << tick << ',' << world.sequence() << ',' << actor->frame.action
           << ',' << actor->position.z << ',' << actor->position.precise_z
           << ',' << actor->motion.z << ',' << actor->current_hp << '\n';
    return static_cast<bool>(output);
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3 && argc != 4) return 2;
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    const int initial_z = argc == 4 ? std::stoi(argv[3]) : 400;
    if (initial_z != 300 && initial_z != 380 && initial_z != 400) return 2;
    if (std::filesystem::exists(output)) return 3;

    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(make_config(initial_z), error)) {
        std::cerr << "initialize: " << error << '\n';
        return 4;
    }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) {
        std::cerr << "recorder: " << error << '\n';
        return 5;
    }
    std::filesystem::create_directories(output);
    std::ofstream rows(output / "source-ticks.csv", std::ios::binary);
    if (!rows) return 6;
    rows << "tick,world_sequence,action,source_z,precise_source_z,vz,hp\n"
         << std::setprecision(17);
    if (!write_row(rows, 0, *session.world())) return 7;
    for (int tick = 1; tick <= 24; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "record tick " << tick << ": " << error << '\n';
            return 8;
        }
        if (!write_row(rows, tick, *session.world())) return 9;
    }
    rows.close();
    if (!rows) return 10;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << "finish: " << error << '\n';
        return 11;
    }
    std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
    if (!lfr) return 12;
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 13;
    std::ofstream summary(output / "summary.txt", std::ios::binary);
    if (!summary) return 14;
    summary << "scenario=oid92-action580-neutral\n"
            << "seed=681925210\n"
            << "background=1\n"
            << "initial_source_z=" << initial_z << '\n'
            << "declared_ticks=24\n"
            << "lfr_bytes=" << bytes.size() << '\n';
    return summary ? 0 : 15;
}
