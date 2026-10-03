#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

struct Case {
    const char* name;
    int initial_action;
    int press_ticks;
    int attack_start;
    int target_x;
    int target_initial_y;
    int target_jump_start;
};

ntsd28::InputButtons28 input_for(const Case& probe, int tick) {
    ntsd28::InputButtons28 input;
    if (probe.initial_action != 0) return input;
    if (tick <= probe.press_ticks) {
        input.set(ntsd28::InputKey28::defend);
    } else if (tick <= 2 * probe.press_ticks) {
        input.set(ntsd28::InputKey28::depth_up);
    } else if (tick >= probe.attack_start &&
               tick < probe.attack_start + probe.press_ticks) {
        input.set(ntsd28::InputKey28::attack);
    }
    return input;
}

ntsd28_playable::BattleConfig28 config_for(const Case& probe) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 0;
    config.character_id = 36;
    config.enemy_id = 56;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.p2_native_ai = false;

    ntsd28_playable::CombatantConfig28 tayuya;
    tayuya.slot = 0;
    tayuya.object_id = 36;
    tayuya.x = 100;
    tayuya.y = 0;
    tayuya.z = 400;
    tayuya.hp = tayuya.base_hp = tayuya.mp = 500;
    tayuya.team = 1;
    tayuya.action = probe.initial_action;
    tayuya.facing = false;

    ntsd28_playable::CombatantConfig28 hidan = tayuya;
    hidan.slot = 1;
    hidan.object_id = 56;
    hidan.x = 200;
    hidan.team = 2;
    hidan.action = 0;

    ntsd28_playable::CombatantConfig28 platform_target = tayuya;
    platform_target.slot = 2;
    platform_target.object_id = 2;
    platform_target.x = probe.target_x;
    platform_target.y = probe.target_initial_y;
    platform_target.action = 0;
    config.combatants = {tayuya, hidan, platform_target};
    return config;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) return 2;
    const std::filesystem::path runtime(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);
    std::ofstream ticks(output / "source-ticks.csv", std::ios::binary);
    std::ofstream summary(output / "summary.csv", std::ios::binary);
    if (!ticks || !summary) return 4;
    ticks << "case,tick,input_defend,input_up,input_attack,target_jump,"
             "tayuya_action,"
             "hidan_action,hidan_x,hidan_y,hidan_environment,platform_target_action,"
             "platform_target_x,platform_target_y,platform_collision_ref,"
             "platform_source,kind10_applied,"
             "kind10_rejected\n";
    summary << "case,initial_action,press_ticks,attack_start,target_x,"
               "target_initial_y,target_jump_start,"
               "first_239,first_240,first_243,first_hidan_182,"
               "kind10_applied,kind10_rejected,frame182_link_ticks,"
               "platform_link_ticks,target_x_final\n";

    constexpr Case cases[] = {
        {"natural_2_5", 0, 2, 5, 205, -5, 0},
        {"natural_1_3", 0, 1, 3, 205, -5, 0},
        {"natural_2_7", 0, 2, 7, 205, -5, 0},
        {"natural_2_5_far", 0, 2, 5, 240, -5, 0},
        {"controlled_239", 239, 0, 0, 205, -5, 0},
        {"controlled_243", 243, 0, 0, 205, -5, 0},
        {"jump_09", 0, 2, 5, 205, 0, 9},
        {"jump_11", 0, 2, 5, 205, 0, 11},
        {"jump_13", 0, 2, 5, 205, 0, 13},
        {"jump_15", 0, 2, 5, 205, 0, 15},
        {"jump_17", 0, 2, 5, 205, 0, 17},
        {"jump_19", 0, 2, 5, 205, 0, 19},
        {"jump_21", 0, 2, 5, 205, 0, 21},
        {"jump_23", 0, 2, 5, 205, 0, 23},
        {"landing_x175", 0, 2, 5, 175, 0, 9},
        {"landing_x180", 0, 2, 5, 180, 0, 9},
        {"landing_x185", 0, 2, 5, 185, 0, 9},
        {"landing_x190", 0, 2, 5, 190, 0, 9},
        {"landing_x195", 0, 2, 5, 195, 0, 9},
    };
    for (const auto& probe : cases) {
        ntsd28_playable::GameSession28 session(runtime, runtime);
        std::string error;
        if (!session.initialize(config_for(probe), error)) {
            std::cerr << probe.name << " initialize: " << error << '\n';
            return 5;
        }
        const bool record_lfr = std::string(probe.name) == "natural_2_5" ||
                                std::string(probe.name) == "landing_x185";
        ntsd28_playable::GameSessionLfr28 recorder;
        if (record_lfr && !recorder.begin(session, error)) {
            std::cerr << probe.name << " recorder begin: " << error << '\n';
            return 8;
        }
        int first_239 = -1;
        int first_240 = -1;
        int first_243 = -1;
        int first_hidan_182 = -1;
        int kind10_applied = 0;
        int kind10_rejected = 0;
        int frame182_links = 0;
        int platform_links = 0;
        for (int tick = 1; tick <= 96; ++tick) {
            const auto input = input_for(probe, tick);
            ntsd28::InputButtons28 target_input;
            if (probe.target_jump_start > 0 &&
                tick >= probe.target_jump_start &&
                tick < probe.target_jump_start + 2) {
                target_input.set(ntsd28::InputKey28::jump);
            }
            session.set_input(0, input);
            session.set_input(1, {});
            session.set_input(2, target_input);
            session.step();
            if (record_lfr && !recorder.capture_after_step(session, error)) {
                std::cerr << probe.name << " recorder tick " << tick << ": "
                          << error << '\n';
                return 9;
            }
            const auto* world = session.world();
            const auto* tayuya = world == nullptr ? nullptr : world->entity(0);
            const auto* hidan = world == nullptr ? nullptr : world->entity(1);
            const auto* target = world == nullptr ? nullptr : world->entity(2);
            if (!tayuya || !hidan || !target || !session.last_tick()) return 6;
            if (tayuya->frame.action == 239 && first_239 < 0) first_239 = tick;
            if (tayuya->frame.action == 240 && first_240 < 0) first_240 = tick;
            if (tayuya->frame.action == 243 && first_243 < 0) first_243 = tick;
            if (hidan->frame.action == 182 && first_hidan_182 < 0)
                first_hidan_182 = tick;
            if (target->platform_source_slot_f4 == 1) {
                ++platform_links;
                if (hidan->frame.action == 182) ++frame182_links;
            }
            int tick_kind10_applied = 0;
            int tick_kind10_rejected = 0;
            for (const auto& hit : session.last_tick()->relation_hits) {
                if (hit.attacker_slot != 0 || hit.target_slot != 1 ||
                    hit.interaction_kind != 10) continue;
                if (hit.status == ntsd28::WorldRelationHitStatus28::applied)
                    ++tick_kind10_applied;
                else
                    ++tick_kind10_rejected;
            }
            kind10_applied += tick_kind10_applied;
            kind10_rejected += tick_kind10_rejected;
            ticks << probe.name << ',' << tick << ','
                  << input[ntsd28::InputKey28::defend] << ','
                  << input[ntsd28::InputKey28::depth_up] << ','
                  << input[ntsd28::InputKey28::attack] << ','
                  << target_input[ntsd28::InputKey28::jump] << ','
                  << tayuya->frame.action << ',' << hidan->frame.action << ','
                  << hidan->position.x << ',' << hidan->position.y << ','
                  << hidan->environment_state_320 << ','
                  << target->frame.action << ',' << target->position.x << ','
                  << target->position.y << ','
                  << target->collision_y_reference << ','
                  << target->platform_source_slot_f4
                  << ',' << tick_kind10_applied << ',' << tick_kind10_rejected
                  << '\n';
        }
        summary << probe.name << ',' << probe.initial_action << ','
                << probe.press_ticks << ',' << probe.attack_start << ','
                << probe.target_x << ',' << probe.target_initial_y << ','
                << probe.target_jump_start << ',' << first_239 << ',' << first_240
                << ',' << first_243 << ',' << first_hidan_182 << ','
                << kind10_applied << ',' << kind10_rejected << ','
                << frame182_links << ',' << platform_links << ','
                << session.world()->entity(2)->position.x << '\n';
        if (record_lfr) {
            std::vector<std::uint8_t> bytes;
            if (!recorder.finish_to_memory(session, bytes, error)) {
                std::cerr << probe.name << " recorder finish: " << error << '\n';
                return 10;
            }
            std::ofstream lfr(output / (std::string(probe.name) + ".lfr"),
                              std::ios::binary);
            if (!lfr) return 11;
            lfr.write(reinterpret_cast<const char*>(bytes.data()),
                      static_cast<std::streamsize>(bytes.size()));
            lfr.close();
            if (!lfr) return 12;
        }
    }
    return ticks && summary ? 0 : 7;
}
