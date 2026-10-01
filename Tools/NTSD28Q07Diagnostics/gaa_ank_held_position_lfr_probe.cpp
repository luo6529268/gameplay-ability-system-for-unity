#include "ntsd28_playable/game_session_lfr.h"
#include "ntsd28/combat_records.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 5) return 2;
    int action = 0;
    int target_x = 0;
    try {
        std::size_t parsed = 0;
        const std::string action_text(argv[3]);
        action = std::stoi(action_text, &parsed);
        if (parsed != action_text.size() ||
            action != 365) return 2;
        parsed = 0;
        const std::string position_text(argv[4]);
        target_x = std::stoi(position_text, &parsed);
        if (parsed != position_text.size() || target_x < 700 || target_x > 1200) return 2;
    } catch (...) { return 2; }
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 16;
    config.enemy_id = 65;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 16;
    actor.x = 500;
    actor.y = 0;
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = action;
    ntsd28_playable::CombatantConfig28 target = actor;
    target.slot = 1;
    target.object_id = 65;
    target.x = target_x;
    target.action = 0;
    target.team = 2;
    config.combatants = {actor, target};

    const auto runtime_root = root / "resources" / "runtime";
    ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
    std::string error;
    if (!session.initialize(config, error)) { std::cerr << error; return 4; }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) { std::cerr << error; return 5; }
    std::ofstream rows(output / "source-ticks.csv", std::ios::binary);
    std::ofstream rng(output / "source-rng.csv", std::ios::binary);
    if (!rows || !rng) return 6;
    rows << "tick,actor_action_before,target_action_before,"
            "target_hold_before,actor_vaction_before,current_pose_x,current_pose_y,"
            "vaction_pose_x,vaction_pose_y,pose_diff,c040_candidate,"
            "actor_action,target_action,actor_counter,target_counter,"
            "actor_x,target_x,actor_y,target_y,actor_vx,target_vx,target_vy,"
            "actor_hp,target_hp,actor_hold,target_hold,catch_target,catch_source,"
            "kind3_applied,active_relations,settlement_active,settlement_injuries,"
            "settlement_damage,target_input_hp_consumed\n"
         << std::setprecision(17);
    rng << "tick,crt_state,crt_calls,custom_counter,custom_index,custom_calls\n";
    int first_catch = -1;
    int first_injury = -1;
    int first_c040_candidate = -1;
    for (int tick = 1; tick <= 120; ++tick) {
        const auto* actor_before = session.world()->entity(0);
        const auto* target_before = session.world()->entity(1);
        if (!actor_before || !target_before) return 7;
        const int actor_action_before = actor_before->frame.action;
        const int target_action_before = target_before->frame.action;
        const int target_hold_before = target_before->motion_hold_timer;
        int vaction = -1;
        int current_pose_x = 0;
        int current_pose_y = 0;
        int vaction_pose_x = 0;
        int vaction_pose_y = 0;
        bool pose_diff = false;
        const auto* actor_frame = actor_before->definition->frame(actor_action_before);
        const auto* catcher_block = actor_frame == nullptr
                                        ? nullptr : actor_frame->first_block("cpoint");
        if (catcher_block != nullptr) {
            const auto catcher_cpoint = ntsd28::CombatRecordDecoder28::catch_point(
                *catcher_block);
            if (catcher_cpoint.kind == 1) vaction = catcher_cpoint.victim_action;
        }
        const auto* current_frame = target_before->definition->frame(target_action_before);
        const auto* vaction_frame = vaction < 0 ? nullptr
            : target_before->definition->frame(vaction);
        const auto* current_block = current_frame == nullptr
                                        ? nullptr : current_frame->first_block("cpoint");
        const auto* vaction_block = vaction_frame == nullptr
                                        ? nullptr : vaction_frame->first_block("cpoint");
        if (current_block != nullptr && vaction_block != nullptr) {
            const auto current_cpoint = ntsd28::CombatRecordDecoder28::catch_point(
                *current_block);
            const auto selected_cpoint = ntsd28::CombatRecordDecoder28::catch_point(
                *vaction_block);
            if (current_cpoint.kind == 2 && selected_cpoint.kind == 2) {
                current_pose_x = current_frame->values.integer("centerx").value_or(0)
                    - current_cpoint.x;
                current_pose_y = current_frame->values.integer("centery").value_or(0)
                    - current_cpoint.y;
                vaction_pose_x = vaction_frame->values.integer("centerx").value_or(0)
                    - selected_cpoint.x;
                vaction_pose_y = vaction_frame->values.integer("centery").value_or(0)
                    - selected_cpoint.y;
                pose_diff = current_pose_x != vaction_pose_x ||
                            current_pose_y != vaction_pose_y;
            }
        }
        session.set_input(0, ntsd28::InputButtons28{});
        session.set_input(1, ntsd28::InputButtons28{});
        session.step();
        if (!recorder.capture_after_step(session, error)) { std::cerr << error; return 8; }
        const auto* world = session.world();
        const auto* current = session.last_tick();
        const auto* first = world->entity(0);
        const auto* second = world->entity(1);
        if (!current || !first || !second) return 9;
        const bool c040_candidate = first->catch_target_slot_8c == 1 &&
            second->catch_source_slot_90 == 0 &&
            current->catch_settlement.active_relations != 0 &&
            second->motion_hold_timer != 0 && second->frame.action != vaction &&
            pose_diff;
        if (c040_candidate && first_c040_candidate < 0)
            first_c040_candidate = tick;
        int kind3_applied = 0;
        for (const auto& hit : current->relation_hits)
            if (hit.interaction_kind == 3 &&
                hit.status == ntsd28::WorldRelationHitStatus28::applied)
                ++kind3_applied;
        if (kind3_applied && first_catch < 0) first_catch = tick;
        if (current->catch_settlement.applied_hold_injuries &&
            first_injury < 0) first_injury = tick;
        rows << world->sequence() << ',' << actor_action_before << ','
             << target_action_before << ',' << target_hold_before << ','
             << vaction << ',' << current_pose_x << ',' << current_pose_y << ','
             << vaction_pose_x << ',' << vaction_pose_y << ','
             << (pose_diff ? 1 : 0) << ',' << (c040_candidate ? 1 : 0) << ','
             << first->frame.action << ',' << second->frame.action << ','
             << first->frame.frame_counter << ',' << second->frame.frame_counter << ','
             << first->position.x << ',' << second->position.x << ','
             << first->position.y << ',' << second->position.y << ','
             << first->motion.x << ',' << second->motion.x << ',' << second->motion.y << ','
             << first->current_hp << ',' << second->current_hp << ','
             << first->motion_hold_timer << ',' << second->motion_hold_timer << ','
             << first->catch_target_slot_8c << ',' << second->catch_source_slot_90 << ','
             << kind3_applied << ','
             << current->catch_relations.active_relations << ','
             << current->catch_settlement.active_relations << ','
             << current->catch_settlement.applied_hold_injuries << ','
             << current->catch_settlement.total_hold_damage << ','
             << second->input_hp_consumed_total << '\n';
        const auto state = world->random().state();
        rng << world->sequence() << ',' << state.crt_state << ',' << state.crt_calls << ','
            << state.synchronized.counter << ',' << state.synchronized.index << ','
            << state.synchronized.calls << '\n';
    }
    rows.close();
    rng.close();
    if (!rows || !rng) return 10;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) { std::cerr << error; return 11; }
    std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 12;
    std::cout << "action=" << action << " target_x=" << target_x
              << " first_catch=" << first_catch
              << " first_c040_candidate=" << first_c040_candidate
              << " first_injury=" << first_injury
              << " ticks=120 lfr_bytes=" << bytes.size() << '\n';
    return 0;
}
