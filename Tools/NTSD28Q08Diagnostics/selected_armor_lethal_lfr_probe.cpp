#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>
#include <cwchar>

int wmain(int argc, wchar_t** argv) {
    if (argc != 4 && argc != 5) {
        std::cerr << "usage: selected_armor_lethal_lfr_probe <extracted_root> <complete_vfs_root> <output_dir> [26|365]\n";
        return 2;
    }
    const int tick_count = argc == 4 ? 26 : std::wcstol(argv[4], nullptr, 10);
    if (tick_count != 26 && tick_count != 365) {
        std::cerr << "only the frozen 26-tick and bounded 365-tick recording fixtures are supported\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    const auto lfr = output / "selected_armor_lethal_source_packets.lfr";
    const auto csv = output / "selected_armor_lethal_source_ticks.csv";
    if (std::filesystem::exists(lfr) || std::filesystem::exists(csv)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.character_id = 11;
    config.enemy_id = 87;
    config.background_id = 23;
    ntsd28_playable::CombatantConfig28 sasuke;
    sasuke.slot = 0;
    sasuke.object_id = 11;
    sasuke.x = 500;
    sasuke.z = 350;
    sasuke.hp = 500;
    sasuke.mp = 500;
    sasuke.team = 1;
    sasuke.action = 110;
    ntsd28_playable::CombatantConfig28 opponent;
    opponent.slot = 1;
    opponent.object_id = 87;
    opponent.x = 550;
    opponent.z = 350;
    opponent.hp = 3;
    opponent.mp = 300;
    opponent.team = 2;
    config.combatants = {sasuke, opponent};

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
    rows << "tick,input,action,last_action_144,hp,mp,oid440_count,target_hp,target_action,target_mp,target_runtime_armor_hp,attacker_knockout_count\n";
    for (int tick = 1; tick <= tick_count; ++tick) {
        ntsd28::InputButtons28 buttons;
        const char* name = "none";
        if (tick == 2) {
            buttons.set(ntsd28::InputKey28::defend);
            name = "defend";
        } else if (tick == 4) {
            buttons.set(ntsd28::InputKey28::right);
            name = "right";
        } else if (tick == 6) {
            buttons.set(ntsd28::InputKey28::attack);
            name = "attack";
        }
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "record tick " << tick << ": " << error << '\n';
            return 7;
        }
        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        if (actor == nullptr) return 8;
        int oid440 = 0;
        for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots;
             ++slot) {
            const auto* entity = world->entity(slot);
            if (entity != nullptr && entity->object_id == 440) ++oid440;
        }
        const auto* target = world->entity(1);
        if (target == nullptr && tick <= 26) return 8;
        rows << tick << ',' << name << ',' << actor->frame.action << ','
             << actor->input_last_action_144 << ',' << actor->current_hp << ','
             << actor->current_mp << ',' << oid440;
        rows << ',' << (target == nullptr ? -1 : target->current_hp) << ','
             << (target == nullptr ? -1 : target->frame.action) << ','
             << (target == nullptr ? -1 : target->current_mp) << ','
             << (target == nullptr ? -1 : target->runtime_armor_hp) << ','
             << actor->knockout_count_358;
        rows << '\n';
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
