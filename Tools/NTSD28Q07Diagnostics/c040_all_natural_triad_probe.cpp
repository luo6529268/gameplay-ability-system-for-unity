#include "ntsd28_playable/game_session_lfr.h"
#include "ntsd28/combat_records.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

namespace {

struct ProbeCase {
    int bee_second;
    int bee_third;
    int kakuzu_start;
    int bee_x;
    int guy_x;
};

int catch_vaction(const ntsd28::EntityState28& catcher, int action) {
    const auto* frame = catcher.definition->frame(action);
    const auto* block = frame == nullptr ? nullptr : frame->first_block("cpoint");
    if (block == nullptr) return -1;
    const auto point = ntsd28::CombatRecordDecoder28::catch_point(*block);
    return point.kind == 1 ? point.victim_action : -1;
}

bool distinct_caught_pose(const ntsd28::EntityState28& victim,
                          int current_action, int selected_action) {
    const auto* current = victim.definition->frame(current_action);
    const auto* selected = victim.definition->frame(selected_action);
    if (!current || !selected) return false;
    const auto* current_block = current->first_block("cpoint");
    const auto* selected_block = selected->first_block("cpoint");
    if (!current_block || !selected_block) return false;
    const auto a = ntsd28::CombatRecordDecoder28::catch_point(*current_block);
    const auto b = ntsd28::CombatRecordDecoder28::catch_point(*selected_block);
    if (a.kind != 2 || b.kind != 2) return false;
    return current->values.integer("centerx").value_or(0) - a.x !=
               selected->values.integer("centerx").value_or(0) - b.x ||
           current->values.integer("centery").value_or(0) - a.y !=
               selected->values.integer("centery").value_or(0) - b.y;
}

std::string case_name(const ProbeCase& test) {
    return "b" + std::to_string(test.bee_second) + "-" +
           std::to_string(test.bee_third) + "-k" +
           std::to_string(test.kakuzu_start) + "-x" +
           std::to_string(test.bee_x) + "-g" +
           std::to_string(test.guy_x);
}

}  // namespace

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: c040_all_natural_triad_probe <formal_root> <new_output_directory>\n";
        return 2;
    }
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);
    std::ofstream rows(output / "source-ticks.csv", std::ios::binary);
    std::ofstream summary(output / "summary.csv", std::ios::binary);
    std::ofstream rng(output / "source-rng.csv", std::ios::binary);
    if (!rows || !summary || !rng) return 4;
    rows << "case,tick,input_phase,kakuzu_action,bee_action,bee_hold,"
            "armor_hit_applied,armor_selected,kind3_applied,"
            "catcher_settlement_action,catcher_vaction,pose_diff,"
            "catch_target,catch_source,settlement_active,settlement_sync,"
            "c040_positive,kakuzu_x,bee_x,guy_x,bee_hp,guy_hp\n"
         << std::setprecision(17);
    summary << "case,first_bee70,first_bee73,first_armor_hit,"
               "first_kind3,first_positive,bee_action_at_catch,"
               "bee_hold_at_catch,first_lfr\n";
    rng << "case,tick,crt_state,crt_calls,custom_counter,"
           "custom_index,custom_calls\n";

    const std::vector<std::pair<int, int>> bee_schedules = {
        {7, 16}, {8, 16}, {9, 16}, {9, 17},
        {9, 18}, {10, 16}, {10, 17}, {10, 18}
    };
    std::vector<ProbeCase> cases;
    for (const auto schedule : bee_schedules) {
        for (int kakuzu_start = 13; kakuzu_start <= 19; ++kakuzu_start) {
            cases.push_back({schedule.first, schedule.second,
                             kakuzu_start, 540, 560});
            cases.push_back({schedule.first, schedule.second,
                             kakuzu_start, 560, 580});
        }
    }
    cases.push_back({7, 16, 15, 1200, 1220});
    const auto runtime_root = std::filesystem::path(argv[1]) /
                              "resources" / "runtime";
    bool emitted_lfr = false;
    int positive_cases = 0;
    for (const auto test : cases) {
        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 0u;
        config.character_id = 25;
        config.enemy_id = 75;
        config.background_id = 1;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;
        ntsd28_playable::CombatantConfig28 kakuzu;
        kakuzu.slot = 0;
        kakuzu.object_id = 25;
        kakuzu.x = 500;
        kakuzu.y = 0;
        kakuzu.z = 400;
        kakuzu.hp = kakuzu.base_hp = kakuzu.mp = 500;
        kakuzu.team = 1;
        kakuzu.action = 0;
        auto bee = kakuzu;
        bee.slot = 1;
        bee.object_id = 75;
        bee.x = test.bee_x;
        bee.team = 2;
        auto guy = kakuzu;
        guy.slot = 2;
        guy.object_id = 97;
        guy.x = test.guy_x;
        config.combatants = {kakuzu, bee, guy};

        ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "initialize " << case_name(test) << ": " << error << '\n';
            return 5;
        }
        ntsd28_playable::GameSessionLfr28 recorder;
        if (!recorder.begin(session, error)) return 6;
        int first_bee70 = -1;
        int first_bee73 = -1;
        int first_armor = -1;
        int first_catch = -1;
        int first_positive = -1;
        int bee_action_at_catch = -1;
        int bee_hold_at_catch = -1;
        bool armor_seen = false;
        const auto name = case_name(test);
        for (int tick = 1; tick <= 40; ++tick) {
            ntsd28::InputButtons28 kakuzu_buttons;
            if (tick == test.kakuzu_start || tick == test.kakuzu_start + 1)
                kakuzu_buttons.set(ntsd28::InputKey28::attack);
            if (tick == test.kakuzu_start + 2 || tick == test.kakuzu_start + 3)
                kakuzu_buttons.set(ntsd28::InputKey28::jump);
            ntsd28::InputButtons28 bee_buttons;
            if (tick <= 2 || tick == test.bee_second ||
                tick == test.bee_second + 1 || tick == test.bee_third ||
                tick == test.bee_third + 1)
                bee_buttons.set(ntsd28::InputKey28::attack);
            session.set_input(0, kakuzu_buttons);
            session.set_input(1, bee_buttons);
            session.set_input(2, ntsd28::InputButtons28{});
            session.step();
            if (!recorder.capture_after_step(session, error)) return 7;
            const auto* result = session.last_tick();
            const auto* world = session.world();
            const auto* actor = world == nullptr ? nullptr : world->entity(0);
            const auto* victim = world == nullptr ? nullptr : world->entity(1);
            const auto* armored = world == nullptr ? nullptr : world->entity(2);
            if (!result || !actor || !victim || !armored) return 8;
            if (victim->frame.action == 70 && first_bee70 < 0) first_bee70 = tick;
            if (victim->frame.action == 73 && first_bee73 < 0) first_bee73 = tick;
            bool armor_hit = false;
            bool selected_armor = false;
            bool kind3 = false;
            int settlement_action = actor->frame.tick_action_snapshot;
            for (const auto& hit : result->hits) {
                if (hit.attacker_slot == 1 && hit.target_slot == 2 &&
                    hit.status == ntsd28::WorldStandardHitStatus28::applied) {
                    armor_hit = true;
                    selected_armor = hit.selected_armor.has_value();
                }
            }
            for (const auto& hit : result->relation_hits) {
                if (hit.attacker_slot == 0 && hit.target_slot == 1 &&
                    hit.interaction_kind == 3 &&
                    hit.status == ntsd28::WorldRelationHitStatus28::applied) {
                    kind3 = true;
                    settlement_action = hit.attacker_action;
                }
            }
            armor_seen = armor_seen || (armor_hit && selected_armor);
            const int vaction = catch_vaction(*actor, settlement_action);
            const bool pose_diff = vaction >= 0 &&
                distinct_caught_pose(*victim, victim->frame.action, vaction);
            const bool paired = actor->catch_target_slot_8c == 1 &&
                                victim->catch_source_slot_90 == 0;
            const bool positive = kind3 && armor_seen && paired &&
                result->catch_settlement.active_relations > 0 &&
                result->catch_settlement.synchronized_targets > 0 &&
                victim->motion_hold_timer > 0 && victim->frame.action != vaction &&
                pose_diff && result->catch_relations.input_action_transitions == 0 &&
                result->catch_relations.thrown_relations == 0 &&
                result->catch_relations.released_relations == 0;
            if (armor_hit && selected_armor && first_armor < 0) first_armor = tick;
            if (kind3 && first_catch < 0) {
                first_catch = tick;
                bee_action_at_catch = victim->frame.action;
                bee_hold_at_catch = victim->motion_hold_timer;
            }
            if (positive && first_positive < 0) first_positive = tick;
            rows << name << ',' << tick << ','
                 << world->input_update_phase_4a0b90() << ','
                 << actor->frame.action << ',' << victim->frame.action << ','
                 << victim->motion_hold_timer << ',' << armor_hit << ','
                 << selected_armor << ',' << kind3 << ',' << settlement_action
                 << ',' << vaction << ',' << pose_diff << ','
                 << actor->catch_target_slot_8c << ','
                 << victim->catch_source_slot_90 << ','
                 << result->catch_settlement.active_relations << ','
                 << result->catch_settlement.synchronized_targets << ','
                 << positive << ',' << actor->position.x << ','
                 << victim->position.x << ',' << armored->position.x << ','
                 << victim->current_hp << ',' << armored->current_hp << '\n';
            const auto state = world->random().state();
            rng << name << ',' << tick << ',' << state.crt_state << ','
                << state.crt_calls << ',' << state.synchronized.counter << ','
                << state.synchronized.index << ',' << state.synchronized.calls << '\n';
        }
        bool emitted = false;
        if (first_positive >= 0) {
            ++positive_cases;
            if (!emitted_lfr) {
                std::vector<std::uint8_t> bytes;
                if (!recorder.finish_to_memory(session, bytes, error)) return 9;
                std::ofstream lfr(output / "first-positive.lfr", std::ios::binary);
                if (!lfr) return 10;
                lfr.write(reinterpret_cast<const char*>(bytes.data()),
                          static_cast<std::streamsize>(bytes.size()));
                lfr.close();
                if (!lfr) return 11;
                emitted_lfr = emitted = true;
            }
        }
        summary << name << ',' << first_bee70 << ',' << first_bee73 << ','
                << first_armor << ',' << first_catch << ',' << first_positive
                << ',' << bee_action_at_catch << ',' << bee_hold_at_catch << ','
                << emitted << '\n';
    }
    rows.close();
    summary.close();
    rng.close();
    if (!rows || !summary || !rng) return 12;
    std::cout << "cases=" << cases.size()
              << " positive_cases=" << positive_cases << '\n';
    return 0;
}
