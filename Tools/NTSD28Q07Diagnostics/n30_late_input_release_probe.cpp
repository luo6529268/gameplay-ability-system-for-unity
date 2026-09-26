#include "ntsd28_playable/game_session_lfr.h"

#include <array>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

using Key = ntsd28::InputKey28;

struct Schedule {
    const char* name;
    std::array<Key, 4> keys;
};

constexpr std::array<Schedule, 4> schedules{{
    {"attack_defend", {Key::attack, Key::defend, Key::attack, Key::defend}},
    {"defend_jump", {Key::defend, Key::jump, Key::defend, Key::jump}},
    {"attack_repeat", {Key::attack, Key::attack, Key::attack, Key::attack}},
    {"attack_jump", {Key::attack, Key::jump, Key::attack, Key::jump}},
}};

ntsd28_playable::BattleConfig28 make_config() {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.character_id = 2;
    config.enemy_id = 2;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 2;
    actor.x = 500;
    actor.z = 350;
    actor.hp = 500;
    actor.mp = 300;
    actor.team = 1;
    actor.action = 0;

    ntsd28_playable::CombatantConfig28 target = actor;
    target.slot = 1;
    target.x = 1200;
    target.team = 2;
    config.combatants = {actor, target};
    return config;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: n30_late_input_release_probe <runtime_root> <complete_vfs_root> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path output_dir(argv[3]);
    const auto rows_path = output_dir / "paired-source.jsonl";
    if (std::filesystem::exists(rows_path)) {
        std::cerr << "refusing to overwrite source rows\n";
        return 3;
    }
    for (const auto& schedule : schedules) {
        if (std::filesystem::exists(output_dir / (std::string(schedule.name) + ".lfr"))) {
            std::cerr << "refusing to overwrite an LFR\n";
            return 3;
        }
    }
    std::filesystem::create_directories(output_dir);
    std::ofstream rows(rows_path, std::ios::binary);
    if (!rows) return 4;

    for (const auto& schedule : schedules) {
        ntsd28_playable::GameSession28 session(argv[1], argv[2]);
        std::string error;
        if (!session.initialize(make_config(), error)) {
            std::cerr << "initialize " << schedule.name << ": " << error << '\n';
            return 5;
        }
        ntsd28_playable::GameSessionLfr28 recorder;
        if (!recorder.begin(session, error)) {
            std::cerr << "record begin " << schedule.name << ": " << error << '\n';
            return 6;
        }
        for (int tick = 1; tick <= 20; ++tick) {
            ntsd28::InputButtons28 buttons;
            if (tick >= 2 && tick <= 8 && tick % 2 == 0) {
                buttons.set(schedule.keys[static_cast<std::size_t>((tick - 2) / 2)]);
            }
            session.set_input(0, buttons);
            session.set_input(1, {});
            session.step();
            if (!recorder.capture_after_step(session, error)) {
                std::cerr << "record tick " << schedule.name << '/' << tick << ": " << error << '\n';
                return 7;
            }
            const auto* world = session.world();
            const auto* actor = world == nullptr ? nullptr : world->entity(0);
            if (actor == nullptr) {
                std::cerr << "actor disappeared " << schedule.name << '/' << tick << '\n';
                return 8;
            }
            rows << "{\"schedule\":\"" << schedule.name << "\",\"tick\":" << tick
                 << ",\"actorAction\":" << actor->frame.action
                 << ",\"inputPhase\":" << world->input_update_phase_4a0b90()
                 << ",\"history\":[";
            for (std::size_t index = 0; index < actor->input.key_history.size(); ++index) {
                if (index != 0) rows << ',';
                rows << actor->input.key_history[index];
            }
            rows << "],\"activeCount\":" << world->active_count() << ",\"oid998\":[";
            bool first = true;
            for (std::size_t slot = 0; slot < 100; ++slot) {
                const auto* candidate = world->entity(slot);
                if (candidate == nullptr || candidate->object_id != 998) continue;
                if (!first) rows << ',';
                first = false;
                rows << "{\"slot\":" << slot << ",\"action\":"
                     << candidate->frame.action << '}';
            }
            rows << "]}\n";
        }
        std::vector<std::uint8_t> bytes;
        if (!recorder.finish_to_memory(session, bytes, error)) {
            std::cerr << "record finish " << schedule.name << ": " << error << '\n';
            return 9;
        }
        std::ofstream encoded(output_dir / (std::string(schedule.name) + ".lfr"), std::ios::binary);
        if (!encoded) return 10;
        encoded.write(reinterpret_cast<const char*>(bytes.data()),
                      static_cast<std::streamsize>(bytes.size()));
        encoded.close();
        if (!encoded) return 11;
        std::cout << schedule.name << ": 20 ticks, " << bytes.size() << " LFR bytes\n";
    }
    rows.close();
    return rows ? 0 : 12;
}
