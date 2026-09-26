#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int wmain(int argc, wchar_t** argv) {
    if (argc != 4 && argc != 5) {
        std::cerr << "usage: sasuke_needle_lfr_probe <decoded_dat> <vfs> <output_dir> [target_x: 550|1200]\n";
        return 2;
    }
    const std::wstring target_x_text = argc == 5 ? argv[4] : L"1200";
    if (target_x_text != L"550" && target_x_text != L"1200") {
        std::cerr << "target_x must be 550 or 1200\n";
        return 2;
    }
    const int target_x = target_x_text == L"550" ? 550 : 1200;
    const std::filesystem::path output(argv[3]);
    const auto lfr = output / "sasuke_needle_source_packets.lfr";
    const auto csv = output / "sasuke_needle_source_ticks.csv";
    if (std::filesystem::exists(lfr) || std::filesystem::exists(csv)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.character_id = 11;
    config.enemy_id = 7;
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
    opponent.object_id = 7;
    opponent.x = target_x;
    opponent.z = 350;
    opponent.hp = 500;
    opponent.mp = 500;
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
    rows << "tick,input,action,last_action_144,hp,mp,oid440_count";
    if (argc == 5) rows << ",target_hp,target_action";
    rows << '\n';
    for (int tick = 1; tick <= 26; ++tick) {
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
        if (target == nullptr) return 8;
        rows << tick << ',' << name << ',' << actor->frame.action << ','
             << actor->input_last_action_144 << ',' << actor->current_hp << ','
             << actor->current_mp << ',' << oid440;
        if (argc == 5) rows << ',' << target->current_hp << ',' << target->frame.action;
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
