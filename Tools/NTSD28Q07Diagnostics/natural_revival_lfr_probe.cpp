#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>
#include <cwchar>

int wmain(int argc, wchar_t** argv) {
    if (argc != 4 && argc != 5) {
        std::cerr << "usage: natural_revival_lfr_probe <extracted_root> <complete_vfs_root> <output_dir> [70|365]\n";
        return 2;
    }
    const int tick_count = argc == 4 ? 70 : std::wcstol(argv[4], nullptr, 10);
    if (tick_count != 70 && tick_count != 365) return 2;
    const std::filesystem::path output(argv[3]);
    const auto lfr = output / "natural_revival_source_packets.lfr";
    const auto csv = output / "natural_revival_source_ticks.csv";
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
    ntsd28_playable::CombatantConfig28 target;
    target.slot = 1;
    target.object_id = 87;
    target.x = 550;
    target.z = 350;
    target.hp = 3;
    target.mp = 300;
    target.team = 2;
    target.revive_lives_30c = 2;
    config.combatants = {sasuke, target};

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
    rows << "tick,input,actor_action,actor_ko,target_present,target_hp,target_action,target_state,target_lives,target_phase,target_hold,target_x,target_z,revival_status,revival_action,revival_lives_before,revival_lives_after\n";
    bool lethal = false;
    bool revived = false;
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
        const auto* victim = world->entity(1);
        const auto* victim_frame = victim == nullptr ? nullptr :
            victim->definition->declared_frame(victim->frame.action);
        int revival_status = -1;
        int revival_action = -1;
        int lives_before = -1;
        int lives_after = -1;
        if (const auto* result = session.last_tick()) {
            for (const auto& event : result->revivals.events) {
                if (event.slot != 1) continue;
                revival_status = static_cast<int>(event.status);
                revival_action = event.selected_action;
                lives_before = event.lives_before;
                lives_after = event.lives_after;
                revived |= event.status == ntsd28::WorldRevivalStatus28::revived;
            }
        }
        lethal |= victim != nullptr && victim->current_hp <= 0;
        rows << tick << ',' << name << ',' << actor->frame.action << ','
             << actor->knockout_count_358 << ',' << (victim != nullptr) << ','
             << (victim == nullptr ? -1 : victim->current_hp) << ','
             << (victim == nullptr ? -1 : victim->frame.action) << ','
             << (victim_frame == nullptr ? -1 :
                 victim_frame->values.integer("state").value_or(-1)) << ','
             << (victim == nullptr ? -1 : victim->revive_lives_30c) << ','
             << (victim == nullptr ? -1 : victim->render_phase_008) << ','
             << (victim == nullptr ? -1 : victim->motion_hold_timer) << ','
             << (victim == nullptr ? -1 : victim->position.x) << ','
             << (victim == nullptr ? -1 : victim->position.z) << ','
             << revival_status << ',' << revival_action << ',' << lives_before
             << ',' << lives_after << '\n';
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
              << bytes.size() << " bytes, lethal=" << lethal
              << " revived=" << revived << '\n';
    return lethal && revived ? 0 : 13;
}
