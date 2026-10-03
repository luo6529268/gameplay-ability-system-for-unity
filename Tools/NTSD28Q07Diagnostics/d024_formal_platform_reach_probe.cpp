#include "ntsd28_playable/game_session.h"
#include "ntsd28_playable/game_session_lfr.h"

#include <algorithm>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

namespace {

ntsd28_playable::BattleConfig28 make_config(int target_y, int target_x) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 0;
    config.character_id = 2;
    config.enemy_id = 56;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.p2_native_ai = false;

    ntsd28_playable::CombatantConfig28 target;
    target.slot = 0;
    target.object_id = 2;
    target.x = target_x;
    target.y = target_y;
    target.z = 400;
    target.hp = target.base_hp = target.mp = 500;
    target.team = 2;
    target.action = 0;
    target.facing = false;

    ntsd28_playable::CombatantConfig28 source = target;
    source.slot = 1;
    source.object_id = 56;
    source.x = 200;
    source.y = 0;
    source.team = 1;
    source.action = 182;
    source.facing = false;
    config.combatants = {target, source};
    return config;
}

bool write_row(std::ofstream& out, int case_number, int tick,
               const ntsd28::BattleWorld28& world) {
    const auto* target = world.entity(0);
    const auto* source = world.entity(1);
    if (target == nullptr || source == nullptr) return false;
    out << case_number << ',' << tick << ',' << world.sequence() << ','
        << source->frame.action << ',' << source->position.x << ','
        << source->position.y << ',' << source->position.previous_y << ','
        << source->position.precise_x << ',' << target->frame.action << ','
        << target->position.x << ',' << target->position.y << ','
        << target->position.previous_y << ',' << target->position.precise_x << ','
        << target->collision_y_reference << ','
        << target->platform_source_slot_f4 << ','
        << target->render_shadow_offset_10c << '\n';
    return static_cast<bool>(out);
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) return 2;
    const std::filesystem::path resource_root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);
    std::ofstream rows(output / "source-ticks.csv", std::ios::binary);
    std::ofstream summary(output / "summary.csv", std::ios::binary);
    if (!rows || !summary) return 4;
    rows << "case,tick,sequence,source_action,source_x,source_y,source_previous_y,"
            "source_precise_x,target_action,target_x,target_y,target_previous_y,"
            "target_precise_x,target_collision_y_reference,target_platform_slot,"
            "target_shadow_offset\n"
         << std::setprecision(17);
    summary << "case,target_initial_y,target_initial_x,link_ticks,consecutive_links,"
               "source_182_link_ticks,target_x_initial,target_x_final\n";
    constexpr int target_ys[] = {-10, -5, 0, 5};
    constexpr int target_xs[] = {205, 208};
    int case_number = 0;
    for (const int target_y : target_ys) {
        for (const int target_x : target_xs) {
            ++case_number;
            ntsd28_playable::GameSession28 session(resource_root, resource_root);
            std::string error;
            if (!session.initialize(make_config(target_y, target_x), error)) {
                std::cerr << "initialize case " << case_number << ": "
                          << error << '\n';
                return 5;
            }
            const bool capture_positive = target_y == -5 && target_x == 205;
            ntsd28_playable::GameSessionLfr28 recorder;
            if (capture_positive && !recorder.begin(session, error)) {
                std::cerr << "recorder begin: " << error << '\n';
                return 10;
            }
            if (!write_row(rows, case_number, 0, *session.world())) return 6;
            int link_ticks = 0;
            int consecutive_links = 0;
            int longest_links = 0;
            int source_182_link_ticks = 0;
            for (int tick = 1; tick <= 10; ++tick) {
                session.set_input(0, {});
                session.set_input(1, {});
                session.step();
                if (capture_positive &&
                    !recorder.capture_after_step(session, error)) {
                    std::cerr << "recorder tick " << tick << ": " << error << '\n';
                    return 11;
                }
                const auto* target = session.world()->entity(0);
                const auto* source = session.world()->entity(1);
                if (target == nullptr || source == nullptr) return 7;
                if (target->platform_source_slot_f4 == 1) {
                    ++link_ticks;
                    longest_links = std::max(longest_links, ++consecutive_links);
                    if (source->frame.action == 182) ++source_182_link_ticks;
                } else {
                    consecutive_links = 0;
                }
                if (!write_row(rows, case_number, tick, *session.world())) return 8;
            }
            summary << case_number << ',' << target_y << ',' << target_x
                    << ',' << link_ticks << ',' << longest_links << ','
                    << source_182_link_ticks << ',' << target_x << ','
                    << session.world()->entity(0)->position.x << '\n';
            if (capture_positive) {
                std::vector<std::uint8_t> bytes;
                if (!recorder.finish_to_memory(session, bytes, error)) {
                    std::cerr << "recorder finish: " << error << '\n';
                    return 12;
                }
                std::ofstream lfr(output / "positive-source.lfr", std::ios::binary);
                if (!lfr) return 13;
                lfr.write(reinterpret_cast<const char*>(bytes.data()),
                          static_cast<std::streamsize>(bytes.size()));
                lfr.close();
                if (!lfr) return 14;
            }
        }
    }
    return rows && summary ? 0 : 9;
}
