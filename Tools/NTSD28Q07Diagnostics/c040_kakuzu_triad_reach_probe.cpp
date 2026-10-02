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
    int bee_action;
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
    if (current == nullptr || selected == nullptr) return false;
    const auto* a_block = current->first_block("cpoint");
    const auto* b_block = selected->first_block("cpoint");
    if (a_block == nullptr || b_block == nullptr) return false;
    const auto a = ntsd28::CombatRecordDecoder28::catch_point(*a_block);
    const auto b = ntsd28::CombatRecordDecoder28::catch_point(*b_block);
    if (a.kind != 2 || b.kind != 2) return false;
    return current->values.integer("centerx").value_or(0) - a.x !=
               selected->values.integer("centerx").value_or(0) - b.x ||
           current->values.integer("centery").value_or(0) - a.y !=
               selected->values.integer("centery").value_or(0) - b.y;
}

}  // namespace

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: c040_kakuzu_triad_reach_probe <formal_root> <new_output_directory>\n";
        return 2;
    }
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);
    std::ofstream rows(output / "source-ticks.csv", std::ios::binary);
    std::ofstream summary(output / "summary.csv", std::ios::binary);
    std::ofstream rng(output / "source-rng.csv", std::ios::binary);
    if (!rows || !summary || !rng) return 4;
    rows << "bee_initial_action,bee_initial_x,guy_initial_x,tick,"
            "catcher_before,bee_before,bee_hold_before,catcher_action,"
            "bee_action,bee_hold,armor_hit_applied,armor_selected,armor_seen,"
            "kind3_applied,catcher_settlement_action,catcher_vaction,pose_diff,"
            "catch_target,catch_source,settlement_active,settlement_sync,"
            "c040_positive,catcher_x,bee_x,guy_x,bee_hp,guy_hp\n"
         << std::setprecision(17);
    summary << "bee_initial_action,bee_initial_x,guy_initial_x,first_320,"
               "first_322,first_armor_hit,first_catch,first_positive,lfr\n";
    rng << "bee_initial_action,bee_initial_x,guy_initial_x,tick,crt_state,"
           "crt_calls,custom_counter,custom_index,custom_calls\n";

    std::vector<ProbeCase> cases;
    for (int action = 69; action <= 73; ++action) {
        for (int x : {540, 560, 580}) {
            for (int delta : {20, 40}) cases.push_back({action, x, x + delta});
        }
    }
    cases.push_back({70, 1200, 1220});
    const auto runtime_root = std::filesystem::path(argv[1]) /
                              "resources" / "runtime";
    int positive_cases = 0;
    for (const auto test : cases) {
        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 0u;
        config.character_id = 25;
        config.enemy_id = 75;
        config.background_id = 1;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;
        ntsd28_playable::CombatantConfig28 catcher;
        catcher.slot = 0;
        catcher.object_id = 25;
        catcher.x = 500;
        catcher.y = 0;
        catcher.z = 400;
        catcher.hp = catcher.base_hp = catcher.mp = 500;
        catcher.team = 1;
        catcher.action = 0;
        auto bee = catcher;
        bee.slot = 1;
        bee.object_id = 75;
        bee.x = test.bee_x;
        bee.action = test.bee_action;
        bee.team = 2;
        auto guy = catcher;
        guy.slot = 2;
        guy.object_id = 97;
        guy.x = test.guy_x;
        config.combatants = {catcher, bee, guy};

        ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "initialize: " << error << '\n';
            return 5;
        }
        ntsd28_playable::GameSessionLfr28 recorder;
        if (!recorder.begin(session, error)) return 6;
        int first_320 = -1;
        int first_322 = -1;
        int first_armor_hit = -1;
        int first_catch = -1;
        int first_positive = -1;
        bool armor_seen = false;
        for (int tick = 1; tick <= 16; ++tick) {
            const auto* before_catcher = session.world()->entity(0);
            const auto* before_bee = session.world()->entity(1);
            if (!before_catcher || !before_bee) return 7;
            const int catcher_before = before_catcher->frame.action;
            const int bee_before = before_bee->frame.action;
            const int bee_hold_before = before_bee->motion_hold_timer;
            ntsd28::InputButtons28 buttons;
            if (tick <= 2) buttons.set(ntsd28::InputKey28::attack);
            if (tick == 3 || tick == 4) buttons.set(ntsd28::InputKey28::jump);
            session.set_input(0, buttons);
            session.set_input(1, ntsd28::InputButtons28{});
            session.set_input(2, ntsd28::InputButtons28{});
            session.step();
            if (!recorder.capture_after_step(session, error)) return 8;
            const auto* result = session.last_tick();
            const auto* world = session.world();
            const auto* actor = world == nullptr ? nullptr : world->entity(0);
            const auto* victim = world == nullptr ? nullptr : world->entity(1);
            const auto* armored = world == nullptr ? nullptr : world->entity(2);
            if (!result || !actor || !victim || !armored) return 9;
            if (actor->frame.action == 320 && first_320 < 0) first_320 = tick;
            if (actor->frame.action == 322 && first_322 < 0) first_322 = tick;
            bool kind3 = false;
            bool armor_hit = false;
            bool selected_armor = false;
            int settlement_action = catcher_before;
            for (const auto& hit : result->relation_hits) {
                if (hit.attacker_slot == 0 && hit.target_slot == 1 &&
                    hit.interaction_kind == 3 &&
                    hit.status == ntsd28::WorldRelationHitStatus28::applied) {
                    kind3 = true;
                    settlement_action = hit.attacker_action;
                }
            }
            for (const auto& hit : result->hits) {
                if (hit.attacker_slot == 1 && hit.target_slot == 2 &&
                    hit.status == ntsd28::WorldStandardHitStatus28::applied) {
                    armor_hit = true;
                    selected_armor = hit.selected_armor.has_value();
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
            if (armor_hit && selected_armor && first_armor_hit < 0)
                first_armor_hit = tick;
            if (kind3 && first_catch < 0) first_catch = tick;
            if (positive && first_positive < 0) first_positive = tick;
            rows << test.bee_action << ',' << test.bee_x << ',' << test.guy_x
                 << ',' << tick << ',' << catcher_before << ',' << bee_before
                 << ',' << bee_hold_before << ',' << actor->frame.action << ','
                 << victim->frame.action << ',' << victim->motion_hold_timer << ','
                 << armor_hit << ',' << selected_armor << ',' << armor_seen << ','
                 << kind3 << ',' << settlement_action << ',' << vaction << ','
                 << pose_diff << ',' << actor->catch_target_slot_8c << ','
                 << victim->catch_source_slot_90 << ','
                 << result->catch_settlement.active_relations << ','
                 << result->catch_settlement.synchronized_targets << ','
                 << positive << ',' << actor->position.x << ','
                 << victim->position.x << ',' << armored->position.x << ','
                 << victim->current_hp << ',' << armored->current_hp << '\n';
            const auto state = world->random().state();
            rng << test.bee_action << ',' << test.bee_x << ',' << test.guy_x
                << ',' << tick << ',' << state.crt_state << ',' << state.crt_calls
                << ',' << state.synchronized.counter << ','
                << state.synchronized.index << ',' << state.synchronized.calls << '\n';
        }
        bool emitted = false;
        if (first_positive >= 0) {
            std::vector<std::uint8_t> bytes;
            if (!recorder.finish_to_memory(session, bytes, error)) return 10;
            const auto name = "bee-a" + std::to_string(test.bee_action) +
                "-x" + std::to_string(test.bee_x) + "-g" +
                std::to_string(test.guy_x) + ".lfr";
            std::ofstream encoded(output / name, std::ios::binary);
            if (!encoded) return 11;
            encoded.write(reinterpret_cast<const char*>(bytes.data()),
                          static_cast<std::streamsize>(bytes.size()));
            encoded.close();
            if (!encoded) return 12;
            emitted = true;
            ++positive_cases;
        }
        summary << test.bee_action << ',' << test.bee_x << ',' << test.guy_x
                << ',' << first_320 << ',' << first_322 << ','
                << first_armor_hit << ',' << first_catch << ','
                << first_positive << ',' << emitted << '\n';
    }
    rows.close();
    summary.close();
    rng.close();
    if (!rows || !summary || !rng) return 13;
    std::cout << "cases=" << cases.size()
              << " positive_cases=" << positive_cases << '\n';
    return 0;
}
