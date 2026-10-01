#include "ntsd28_playable/game_session_lfr.h"

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
            (action != 314 && action != 316)) return 2;
        parsed = 0;
        const std::string position_text(argv[4]);
        target_x = std::stoi(position_text, &parsed);
        if (parsed != position_text.size() || target_x < 500 || target_x > 1200) return 2;
    } catch (...) { return 2; }
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 17;
    config.enemy_id = 2;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 17;
    actor.x = 500;
    actor.y = 0;
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = action;
    ntsd28_playable::CombatantConfig28 target = actor;
    target.slot = 1;
    target.object_id = 2;
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
    rows << "tick,actor_action_before,target_action_before,timeout_before,"
            "actor_action,target_action,actor_counter,target_counter,"
            "actor_x,target_x,actor_y,target_y,actor_vx,target_vx,target_vy,"
            "actor_hp,target_hp,catch_target,catch_source,timeout_after,"
            "kind3_applied,active_relations,timeout_changes,released_relations,"
            "thrown_relations,target_pending_x,target_pending_y,target_pending_count\n"
         << std::setprecision(17);
    rng << "tick,crt_state,crt_calls,custom_counter,custom_index,custom_calls\n";
    int first_catch = -1;
    int first_negative_frame = -1;
    int first_release = -1;
    int first_throw = -1;
    int minimum_timeout = 1000000;
    for (int tick = 1; tick <= 160; ++tick) {
        const auto* actor_before = session.world()->entity(0);
        const auto* target_before = session.world()->entity(1);
        if (!actor_before || !target_before) return 7;
        const int actor_action_before = actor_before->frame.action;
        const int target_action_before = target_before->frame.action;
        const int timeout_before = actor_before->catch_timeout_94;
        if ((actor_action_before == 123 || actor_action_before == 124) &&
            first_negative_frame < 0) first_negative_frame = tick;
        session.set_input(0, ntsd28::InputButtons28{});
        session.set_input(1, ntsd28::InputButtons28{});
        session.step();
        if (!recorder.capture_after_step(session, error)) { std::cerr << error; return 8; }
        const auto* world = session.world();
        const auto* current = session.last_tick();
        const auto* first = world->entity(0);
        const auto* second = world->entity(1);
        if (!current || !first || !second) return 9;
        int kind3_applied = 0;
        for (const auto& hit : current->relation_hits)
            if (hit.interaction_kind == 3 &&
                hit.status == ntsd28::WorldRelationHitStatus28::applied)
                ++kind3_applied;
        if (kind3_applied && first_catch < 0) first_catch = tick;
        if (current->catch_relations.released_relations && first_release < 0)
            first_release = tick;
        if (current->catch_relations.thrown_relations && first_throw < 0)
            first_throw = tick;
        if (first->catch_target_slot_8c == 1 && first->catch_timeout_94 < minimum_timeout)
            minimum_timeout = first->catch_timeout_94;
        rows << world->sequence() << ',' << actor_action_before << ','
             << target_action_before << ',' << timeout_before << ','
             << first->frame.action << ',' << second->frame.action << ','
             << first->frame.frame_counter << ',' << second->frame.frame_counter << ','
             << first->position.x << ',' << second->position.x << ','
             << first->position.y << ',' << second->position.y << ','
             << first->motion.x << ',' << second->motion.x << ',' << second->motion.y << ','
             << first->current_hp << ',' << second->current_hp << ','
             << first->catch_target_slot_8c << ',' << second->catch_source_slot_90 << ','
             << first->catch_timeout_94 << ',' << kind3_applied << ','
             << current->catch_relations.active_relations << ','
             << current->catch_relations.timeout_changes << ','
             << current->catch_relations.released_relations << ','
             << current->catch_relations.thrown_relations << ','
             << second->pending_hit_impulse.total.x << ','
             << second->pending_hit_impulse.total.y << ','
             << second->pending_hit_impulse.contribution_count << '\n';
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
              << " first_negative_frame=" << first_negative_frame
              << " first_release=" << first_release
              << " first_throw=" << first_throw
              << " min_timeout=" << minimum_timeout
              << " ticks=160 lfr_bytes=" << bytes.size() << '\n';
    return 0;
}
