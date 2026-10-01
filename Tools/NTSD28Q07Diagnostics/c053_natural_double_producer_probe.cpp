#include "ntsd28_playable/game_session.h"

#include <array>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>

namespace {

constexpr std::uint32_t seed = 682973786u;

ntsd28_playable::BattleConfig28 make_config(int first_x, int second_x) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = seed;
    config.character_id = 65;
    config.enemy_id = 702;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 first;
    first.slot = 0;
    first.object_id = 65;
    first.x = first_x;
    first.y = 0;
    first.z = 400;
    first.hp = first.base_hp = first.mp = 500;
    first.team = 1;
    first.action = 511;

    auto target = first;
    target.slot = 1;
    target.object_id = 702;
    target.x = 500;
    target.team = 2;
    target.action = 553;

    auto second = first;
    second.slot = 2;
    second.x = second_x;
    config.combatants = {first, target, second};
    return config;
}

int find_oid(const ntsd28::BattleWorld28& world, int oid) {
    for (std::size_t slot = 3; slot < ntsd28::EngineProfile28::maximum_slots;
         ++slot) {
        const auto* entity = world.entity(slot);
        if (entity && entity->object_id == oid) return static_cast<int>(slot);
    }
    return -1;
}

void run_case(const std::filesystem::path& runtime, std::ofstream& output,
              int first_x, int second_x, int& double_cases) {
    ntsd28_playable::GameSession28 session(runtime, runtime);
    std::string error;
    if (!session.initialize(make_config(first_x, second_x), error)) {
        throw std::runtime_error("initialize " + std::to_string(first_x) + "/" +
                                 std::to_string(second_x) + ": " + error);
    }
    const auto* initial = session.world();
    if (!initial || !initial->entity(0) || !initial->entity(1) ||
        !initial->entity(2) || initial->entity(0)->object_id != 65 ||
        initial->entity(1)->object_id != 702 ||
        initial->entity(2)->object_id != 65 ||
        initial->entity(0)->frame.action != 511 ||
        initial->entity(1)->frame.action != 553 ||
        initial->entity(2)->frame.action != 511) {
        throw std::runtime_error("initial three-combatant roster differs");
    }
    bool case_has_double = false;
    for (int tick = 1; tick <= 12; ++tick) {
        for (std::size_t slot = 0; slot < 3; ++slot) {
            session.set_input(slot, ntsd28::InputButtons28{});
        }
        session.step();
        const auto* world = session.world();
        const auto* result = session.last_tick();
        if (!world || !result) throw std::runtime_error("missing tick state");

        const int child_slot = find_oid(*world, 808);
        const auto* child = child_slot < 0 ? nullptr : world->entity(child_slot);
        int count_875 = 0;
        int owner0 = 0;
        int owner2 = 0;
        std::string attackers;
        for (std::size_t slot = 3;
             slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            const auto* entity = world->entity(slot);
            if (!entity || entity->object_id != 875) continue;
            ++count_875;
            owner0 += entity->owner_slot == 0;
            owner2 += entity->owner_slot == 2;
            if (!attackers.empty()) attackers += ';';
            attackers += std::to_string(slot) + ':' +
                         std::to_string(entity->owner_slot) + ':' +
                         std::to_string(entity->frame.action) + ':' +
                         std::to_string(entity->position.x);
        }
        int hit_count = 0;
        int applied_uj = 0;
        std::string hits;
        for (const auto& hit : result->hits) {
            if (child_slot < 0 ||
                hit.target_slot != static_cast<std::size_t>(child_slot)) continue;
            ++hit_count;
            const int effect = hit.interaction ? hit.interaction->effect : -1;
            applied_uj +=
                hit.status == ntsd28::WorldStandardHitStatus28::applied &&
                effect == 2 && hit.target_type3_post_hit_action == 156;
            if (!hits.empty()) hits += ';';
            hits += std::to_string(hit.attacker_slot) + ':' +
                    std::to_string(static_cast<int>(hit.status)) + ':' +
                    std::to_string(effect) + ':' +
                    std::to_string(hit.target_type3_post_hit_action);
        }
        case_has_double |= applied_uj >= 2;
        output << first_x << ',' << second_x << ',' << tick << ','
               << world->entity(0)->frame.action << ','
               << world->entity(1)->frame.action << ','
               << world->entity(2)->frame.action << ',' << count_875 << ','
               << owner0 << ',' << owner2 << ',' << attackers << ','
               << child_slot << ',' << (child ? child->frame.action : -1)
               << ',' << (child ? child->frame.action_latch : -1) << ','
               << (child ? child->current_hp : 0) << ',' << hit_count << ','
               << applied_uj << ',' << hits << '\n';
    }
    double_cases += case_has_double;
    std::cout << first_x << '/' << second_x << ",double="
              << (case_has_double ? 1 : 0) << '\n';
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: c053_natural_double_producer_probe <formal_runtime> "
                     "<new_csv_path>\n";
        return 2;
    }
    try {
        const std::filesystem::path output_path(argv[2]);
        if (std::filesystem::exists(output_path)) return 3;
        std::filesystem::create_directories(output_path.parent_path());
        std::ofstream output(output_path, std::ios::binary);
        if (!output) return 4;
        output << "ank0_x,ank2_x,tick,ank0_action,jira_action,ank2_action,"
                  "oid875_count,owner0_count,owner2_count,attackers,"
                  "oid808_slot,oid808_action,oid808_latch,oid808_hp,"
                  "oid808_hit_count,oid808_applied_uj,hits\n";
        int double_cases = 0;
        for (int first_x : {580, 610, 640}) {
            for (int second_x : {580, 610, 640}) {
                run_case(argv[1], output, first_x, second_x, double_cases);
            }
        }
        output.close();
        if (!output) return 5;
        std::cout << "double_cases=" << double_cases << "/9\n";
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 6;
    }
}
