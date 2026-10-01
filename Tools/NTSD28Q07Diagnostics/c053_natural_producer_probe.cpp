#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

namespace {

constexpr std::uint32_t seed = 682973786u;

ntsd28_playable::BattleConfig28 make_config(int ank_x, int ank_y, int jira_x) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = seed;
    config.character_id = 65;
    config.enemy_id = 702;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 ank;
    ank.slot = 0;
    ank.object_id = 65;
    ank.x = ank_x;
    ank.y = ank_y;
    ank.z = 400;
    ank.hp = ank.base_hp = ank.mp = 500;
    ank.team = 1;
    ank.action = 511;

    auto jira = ank;
    jira.slot = 1;
    jira.object_id = 702;
    jira.x = jira_x;
    jira.y = 0;
    jira.team = 2;
    jira.action = 553;
    config.combatants = {ank, jira};
    return config;
}

int first_slot(const ntsd28::BattleWorld28& world, int oid) {
    for (std::size_t slot = 2; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
        const auto* entity = world.entity(slot);
        if (entity && entity->object_id == oid) return static_cast<int>(slot);
    }
    return -1;
}

int run_case(const std::filesystem::path& runtime,
             const std::filesystem::path& output, int ank_x, int ank_y,
             int jira_x) {
    ntsd28_playable::GameSession28 session(runtime, runtime);
    std::string error;
    if (!session.initialize(make_config(ank_x, ank_y, jira_x), error)) {
        std::cerr << "initialize " << ank_x << ": " << error << '\n';
        return 4;
    }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) {
        std::cerr << "record begin " << ank_x << ": " << error << '\n';
        return 5;
    }
    const std::string stem = "ank" + std::to_string(ank_x) + "-y" +
                             std::to_string(ank_y) + "-jira" +
                             std::to_string(jira_x);
    std::ofstream csv(output / (stem + ".csv"), std::ios::binary);
    if (!csv) return 6;
    csv << "tick,ank_action,jira_action,oid875_count,oid875_action50_count,"
           "oid875_action55_count,first875_action,first875_x,first875_y,"
           "first875_z,oid808_slot,oid808_action,oid808_latch,"
           "oid808_x,oid808_y,oid808_z,oid808_hit_count,oid808_applied,"
           "oid808_uj,oid808_hit_sequence\n";
    int born_875 = -1;
    int action_55 = -1;
    int born_808 = -1;
    int first_uj = -1;
    int first_double = -1;
    for (int tick = 1; tick <= 40; ++tick) {
        session.set_input(0, ntsd28::InputButtons28{});
        session.set_input(1, ntsd28::InputButtons28{});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "capture " << ank_x << " tick " << tick << ": "
                      << error << '\n';
            return 7;
        }
        const auto* world = session.world();
        const auto* result = session.last_tick();
        if (!world || !result) return 8;
        int count_875 = 0;
        int count_50 = 0;
        int count_55 = 0;
        const ntsd28::EntityState28* first_875 = nullptr;
        for (std::size_t slot = 2; slot < ntsd28::EngineProfile28::maximum_slots;
             ++slot) {
            const auto* entity = world->entity(slot);
            if (!entity || entity->object_id != 875) continue;
            if (!first_875) first_875 = entity;
            ++count_875;
            count_50 += entity->frame.action == 50;
            count_55 += entity->frame.action == 55;
        }
        const int child_slot = first_slot(*world, 808);
        const auto* child = child_slot < 0 ? nullptr : world->entity(child_slot);
        if (count_875 && born_875 < 0) born_875 = tick;
        if (count_55 && action_55 < 0) action_55 = tick;
        if (child && born_808 < 0) born_808 = tick;
        int hit_count = 0;
        int applied = 0;
        int uj = 0;
        std::string sequence;
        for (const auto& hit : result->hits) {
            if (child_slot < 0 ||
                hit.target_slot != static_cast<std::size_t>(child_slot)) continue;
            ++hit_count;
            applied += hit.status == ntsd28::WorldStandardHitStatus28::applied;
            uj += hit.target_type3_post_hit_action >= 0;
            if (!sequence.empty()) sequence += ';';
            sequence += std::to_string(hit.attacker_slot) + ':' +
                        std::to_string(static_cast<int>(hit.status)) + ':' +
                        std::to_string(hit.interaction ? hit.interaction->effect : -1) +
                        ':' + std::to_string(hit.target_type3_post_hit_action);
        }
        if (uj && first_uj < 0) first_uj = tick;
        if (uj >= 2 && first_double < 0) first_double = tick;
        const auto* ank = world->entity(0);
        const auto* jira = world->entity(1);
        csv << tick << ',' << (ank ? ank->frame.action : -1) << ','
            << (jira ? jira->frame.action : -1) << ',' << count_875 << ','
            << count_50 << ',' << count_55 << ','
            << (first_875 ? first_875->frame.action : -1) << ','
            << (first_875 ? first_875->position.x : 0) << ','
            << (first_875 ? first_875->position.y : 0) << ','
            << (first_875 ? first_875->position.z : 0) << ',' << child_slot << ','
            << (child ? child->frame.action : -1) << ','
            << (child ? child->frame.action_latch : -1) << ','
            << (child ? child->position.x : 0) << ','
            << (child ? child->position.y : 0) << ','
            << (child ? child->position.z : 0) << ',' << hit_count << ','
            << applied << ',' << uj << ',' << sequence << '\n';
    }
    csv.close();
    if (!csv) return 9;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << "record finish " << ank_x << ": " << error << '\n';
        return 10;
    }
    std::ofstream lfr(output / (stem + ".lfr"), std::ios::binary);
    if (!lfr) return 11;
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 12;
    std::cout << stem << ",born875=" << born_875 << ",action55=" << action_55
              << ",born808=" << born_808 << ",firstUj=" << first_uj
              << ",firstDouble=" << first_double << '\n';
    return 0;
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 5) {
        std::cerr << "usage: c053_natural_producer_probe <formal_runtime> "
                     "<new_output_dir> <jira_x> <ank_y>\n";
        return 2;
    }
    try {
        const std::filesystem::path output(argv[2]);
        if (std::filesystem::exists(output)) return 3;
        std::filesystem::create_directories(output);
        const int jira_x = std::stoi(argv[3]);
        const int ank_y = std::stoi(argv[4]);
        for (int ank_x : {550, 580, 610, 640, 670, 700, 730}) {
            const int result = run_case(argv[1], output, ank_x, ank_y,
                                        jira_x);
            if (result != 0) return result;
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 13;
    }
}
