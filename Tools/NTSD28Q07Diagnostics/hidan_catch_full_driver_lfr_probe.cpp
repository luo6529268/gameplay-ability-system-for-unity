#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int wmain(int argc, wchar_t** argv) {
    if (argc != 5) {
        std::cerr << "usage: hidan_catch_full_driver_lfr_probe <decoded_dat> <complete_vfs_root> <output_dir> <target_x:520|1200>\n";
        return 2;
    }
    const std::wstring target_text(argv[4]);
    if (target_text != L"520" && target_text != L"1200") {
        std::cerr << "target_x must be 520 or 1200\n";
        return 2;
    }
    const int target_x = target_text == L"520" ? 520 : 1200;
    const std::filesystem::path output(argv[3]);
    std::filesystem::create_directories(output);
    const auto csv = output / "hidan_catch_source_ticks.csv";
    const auto lfr = output / "hidan_catch_source_packets.lfr";
    if (std::filesystem::exists(csv) || std::filesystem::exists(lfr)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.character_id = 24;
    config.enemy_id = 24;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 catcher;
    catcher.slot = 0;
    catcher.object_id = 24;
    catcher.x = 500;
    catcher.z = 350;
    catcher.hp = 500;
    catcher.mp = 300;
    catcher.team = 1;
    catcher.action = 236;
    ntsd28_playable::CombatantConfig28 target;
    target.slot = 1;
    target.object_id = 24;
    target.x = target_x;
    target.z = 350;
    target.hp = 500;
    target.mp = 300;
    target.team = 2;
    target.action = 0;
    config.combatants = {catcher, target};

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
    rows << "tick,actorAction,actorTickAction,targetAction,actorMp,targetMp,targetHp,catchTarget,catchSource,actorFrameCounter,targetFrameCounter\n";
    int first_relation_tick = -1;
    int first_held_injury_tick = -1;
    int first_throw_injury_tick = -1;
    int previous_target_hp = 500;
    int previous_target_environment = 0;
    for (int tick = 1; tick <= 24; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "record tick " << tick << ": " << error << '\n';
            return 7;
        }
        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        const auto* victim = world == nullptr ? nullptr : world->entity(1);
        if (actor == nullptr || victim == nullptr) {
            std::cerr << "physical Hidan entity disappeared at tick " << tick << '\n';
            return 8;
        }
        const bool reciprocal = actor->catch_target_slot_8c == 1 &&
            victim->catch_source_slot_90 == 0;
        if (reciprocal && first_relation_tick < 0) first_relation_tick = tick;
        if (victim->current_hp < previous_target_hp &&
            first_held_injury_tick < 0) first_held_injury_tick = tick;
        if (victim->environment_state_320 != previous_target_environment &&
            victim->environment_state_320 > 0 &&
            first_throw_injury_tick < 0) first_throw_injury_tick = tick;
        rows << tick << ',' << actor->frame.action << ','
             << actor->frame.tick_action_snapshot << ','
             << victim->frame.action << ',' << actor->current_mp << ','
             << victim->current_mp << ',' << victim->current_hp << ','
             << actor->catch_target_slot_8c << ','
             << victim->catch_source_slot_90 << ','
             << actor->frame.frame_counter << ','
             << victim->frame.frame_counter << '\n';
        previous_target_hp = victim->current_hp;
        previous_target_environment = victim->environment_state_320;
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
    std::cout << "{\"targetX\":" << target_x
              << ",\"ticks\":24,\"firstRelationTick\":"
              << first_relation_tick
              << ",\"firstHeldInjuryTick\":"
              << first_held_injury_tick
              << ",\"firstThrowEnvironmentTick\":"
              << first_throw_injury_tick
              << ",\"lfrBytes\":" << bytes.size() << "}\n";
    return 0;
}
