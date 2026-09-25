#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: nin_frame31_lfr_probe <runtime_root> <runtime_root> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    const auto lfr = output / "oid30_frame31_source_packets.lfr";
    const auto csv = output / "oid30_frame31_source_ticks.csv";
    if (std::filesystem::exists(lfr) || std::filesystem::exists(csv)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.character_id = 30;
    config.enemy_id = 7;
    config.background_id = 23;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 30;
    actor.x = 500;
    actor.z = 350;
    actor.hp = 500;
    actor.mp = 500;
    actor.team = 1;
    actor.action = 31;
    ntsd28_playable::CombatantConfig28 opponent;
    opponent.slot = 1;
    opponent.object_id = 7;
    opponent.x = 1200;
    opponent.z = 350;
    opponent.hp = 500;
    opponent.mp = 500;
    opponent.team = 2;
    config.combatants = {actor, opponent};

    ntsd28_playable::GameSession28 session(argv[1], argv[2]);
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
    std::ofstream rows(csv, std::ios::binary);
    if (!rows) return 6;
    rows << "tick,action,base_pic,visual_offset,effective_pic,actor_sprite_count,total_sprite_count\n";
    for (int tick = 1; tick <= 12; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "record tick " << tick << ": " << error << '\n';
            return 7;
        }
        const auto* world = session.world();
        const auto* current = world == nullptr ? nullptr : world->entity(0);
        if (current == nullptr) return 8;
        const auto* frame = current->definition->frame(current->frame.action);
        const int base_pic = frame == nullptr
                                 ? -1
                                 : frame->values.integer("pic").value_or(-1);
        const auto snapshot = session.snapshot(false);
        int actor_sprites = 0;
        for (const auto& sprite : snapshot.sprites) {
            if (sprite.slot == 0) ++actor_sprites;
        }
        rows << tick << ',' << current->frame.action << ',' << base_pic << ','
             << current->revive_visual_runtime_318 << ','
             << base_pic + current->revive_visual_runtime_318 << ','
             << actor_sprites << ',' << snapshot.sprites.size() << '\n';
    }
    rows.close();
    if (!rows) return 9;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << "record finish: " << error << '\n';
        return 10;
    }
    std::ofstream encoded(lfr, std::ios::binary);
    if (!encoded) return 11;
    encoded.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
    encoded.close();
    if (!encoded) return 12;
    std::cout << "recorded " << recorder.row_count() << " ticks, "
              << bytes.size() << " bytes\n";
    return 0;
}
