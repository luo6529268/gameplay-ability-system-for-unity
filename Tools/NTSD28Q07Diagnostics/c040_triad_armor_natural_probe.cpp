#include "ntsd28_playable/game_session_lfr.h"
#include "ntsd28/combat_records.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

namespace {

int catch_vaction(const ntsd28::EntityState28& catcher, int action) {
    const auto* frame = catcher.definition->frame(action);
    const auto* block = frame == nullptr ? nullptr : frame->first_block("cpoint");
    if (block == nullptr) return -1;
    const auto point = ntsd28::CombatRecordDecoder28::catch_point(*block);
    return point.kind == 1 ? point.victim_action : -1;
}

bool distinct_caught_pose(const ntsd28::EntityState28& caught,
                          int current_action, int selected_action) {
    const auto* current = caught.definition->frame(current_action);
    const auto* selected = caught.definition->frame(selected_action);
    if (current == nullptr || selected == nullptr) return false;
    const auto* current_block = current->first_block("cpoint");
    const auto* selected_block = selected->first_block("cpoint");
    if (current_block == nullptr || selected_block == nullptr) return false;
    const auto a = ntsd28::CombatRecordDecoder28::catch_point(*current_block);
    const auto b = ntsd28::CombatRecordDecoder28::catch_point(*selected_block);
    if (a.kind != 2 || b.kind != 2) return false;
    return current->values.integer("centerx").value_or(0) - a.x !=
               selected->values.integer("centerx").value_or(0) - b.x ||
           current->values.integer("centery").value_or(0) - a.y !=
               selected->values.integer("centery").value_or(0) - b.y;
}

}  // namespace

int main(int argc, char** argv) {
    if (argc != 5) return 2;
    int bee_x = 0;
    int guy_x = 0;
    try {
        bee_x = std::stoi(argv[3]);
        guy_x = std::stoi(argv[4]);
    } catch (...) { return 2; }
    if (bee_x < 550 || bee_x > 1200 || guy_x < bee_x || guy_x > 1250)
        return 2;
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);
    const auto runtime_root = std::filesystem::path(argv[1]) /
                              "resources" / "runtime";
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 21;
    config.enemy_id = 75;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 21;
    actor.x = 500;
    actor.y = 0;
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = 415;
    auto caught = actor;
    caught.slot = 1;
    caught.object_id = 75;
    caught.x = bee_x;
    caught.action = 73;
    caught.team = 2;
    auto armored = actor;
    armored.slot = 2;
    armored.object_id = 97;
    armored.x = guy_x;
    armored.action = 0;
    armored.team = 1;
    config.combatants = {actor, caught, armored};

    ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
    std::string error;
    if (!session.initialize(config, error)) { std::cerr << error; return 4; }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) { std::cerr << error; return 5; }
    std::ofstream rows(output / "source-ticks.csv", std::ios::binary);
    std::ofstream rng(output / "source-rng.csv", std::ios::binary);
    if (!rows || !rng) return 6;
    rows << "tick,catcher_before,caught_before,caught_hold_before,"
            "kind3_applied,armor_hit_applied,armor_selected,armor_hit_seen,"
            "catcher_settlement_action,catcher_vaction,"
            "caught_action,caught_hold,pose_diff,catch_source,catch_target,"
            "settlement_active,settlement_synchronized,c040_positive,"
            "catcher_x,caught_x,armor_x,caught_hp,armor_hp\n"
         << std::setprecision(17);
    rng << "tick,crt_state,crt_calls,custom_counter,custom_index,custom_calls\n";
    int first_catch = -1;
    int first_armor_hit = -1;
    int first_positive = -1;
    bool armor_hit_seen = false;
    for (int tick = 1; tick <= 16; ++tick) {
        const auto* before_catcher = session.world()->entity(0);
        const auto* before_caught = session.world()->entity(1);
        if (!before_catcher || !before_caught) return 7;
        const int catcher_before = before_catcher->frame.action;
        const int caught_before = before_caught->frame.action;
        const int caught_hold_before = before_caught->motion_hold_timer;
        for (int slot = 0; slot < 3; ++slot)
            session.set_input(slot, ntsd28::InputButtons28{});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << error;
            return 8;
        }
        const auto* result = session.last_tick();
        const auto* world = session.world();
        const auto* catcher = world->entity(0);
        const auto* victim = world->entity(1);
        const auto* armor = world->entity(2);
        if (!result || !catcher || !victim || !armor) return 9;
        bool kind3_applied = false;
        bool armor_hit_applied = false;
        bool armor_selected = false;
        int settlement_action = catcher_before;
        for (const auto& hit : result->relation_hits) {
            if (hit.attacker_slot == 0 && hit.target_slot == 1 &&
                hit.interaction_kind == 3 &&
                hit.status == ntsd28::WorldRelationHitStatus28::applied) {
                kind3_applied = true;
                settlement_action = hit.attacker_action;
            }
        }
        for (const auto& hit : result->hits) {
            if (hit.attacker_slot == 1 && hit.target_slot == 2 &&
                hit.status == ntsd28::WorldStandardHitStatus28::applied) {
                armor_hit_applied = true;
                armor_selected = hit.selected_armor.has_value();
            }
        }
        armor_hit_seen = armor_hit_seen || (armor_hit_applied && armor_selected);
        const int vaction = catch_vaction(*catcher, settlement_action);
        const bool pose_diff = vaction >= 0 &&
            distinct_caught_pose(*victim, victim->frame.action, vaction);
        const bool paired = catcher->catch_target_slot_8c == 1 &&
                            victim->catch_source_slot_90 == 0;
        const bool positive = kind3_applied && armor_hit_seen && paired &&
            result->catch_settlement.active_relations > 0 &&
            result->catch_settlement.synchronized_targets > 0 &&
            victim->motion_hold_timer > 0 && victim->frame.action != vaction &&
            pose_diff && result->catch_relations.input_action_transitions == 0 &&
            result->catch_relations.thrown_relations == 0 &&
            result->catch_relations.released_relations == 0;
        if (kind3_applied && first_catch < 0) first_catch = tick;
        if (armor_hit_applied && first_armor_hit < 0) first_armor_hit = tick;
        if (positive && first_positive < 0) first_positive = tick;
        rows << world->sequence() << ',' << catcher_before << ',' << caught_before
             << ',' << caught_hold_before << ',' << kind3_applied << ','
             << armor_hit_applied << ',' << armor_selected << ',' << armor_hit_seen << ','
             << settlement_action << ',' << vaction << ','
             << victim->frame.action << ',' << victim->motion_hold_timer << ','
             << pose_diff << ',' << victim->catch_source_slot_90 << ','
             << catcher->catch_target_slot_8c << ','
             << result->catch_settlement.active_relations << ','
             << result->catch_settlement.synchronized_targets << ','
             << positive << ',' << catcher->position.x << ',' << victim->position.x
             << ',' << armor->position.x << ',' << victim->current_hp << ','
             << armor->current_hp << '\n';
        const auto state = world->random().state();
        rng << world->sequence() << ',' << state.crt_state << ',' << state.crt_calls
            << ',' << state.synchronized.counter << ',' << state.synchronized.index
            << ',' << state.synchronized.calls << '\n';
    }
    rows.close();
    rng.close();
    if (!rows || !rng) return 10;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << error;
        return 11;
    }
    std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 12;
    std::cout << "bee_x=" << bee_x << " guy_x=" << guy_x
              << " first_catch=" << first_catch
              << " first_armor_hit=" << first_armor_hit
              << " first_c040_positive=" << first_positive
              << " ticks=16 lfr_bytes=" << bytes.size() << '\n';
    return 0;
}
