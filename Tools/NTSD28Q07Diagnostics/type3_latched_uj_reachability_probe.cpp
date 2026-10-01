#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

namespace {

int parse_actor(const char* text) {
    const int value = std::stoi(text);
    if (value != 702 && value != 63) throw std::runtime_error("actor must be 702 or 63");
    return value;
}

int parse_target_x(const char* text) {
    const int value = std::stoi(text);
    if (value < 500 || value > 1200) throw std::runtime_error("target X outside probe range");
    return value;
}

ntsd28_playable::BattleConfig28 make_config(int actor_oid, int target_x,
                                          int third_x, int fourth_x) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = actor_oid;
    config.enemy_id = 2;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = actor_oid;
    actor.x = 500;
    actor.y = 0;
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = 553;
    auto target = actor;
    target.slot = 1;
    target.object_id = 2;
    target.x = target_x;
    target.action = 0;
    target.team = 2;
    config.combatants = {actor, target};
    if (third_x >= 0) {
        auto third = actor;
        third.slot = 2;
        third.object_id = 875;
        third.x = third_x;
        third.z = 401;
        third.action = 55;
        third.team = 2;
        config.combatants.push_back(third);
    }
    if (fourth_x >= 0) {
        auto fourth = config.combatants[2];
        fourth.slot = 3;
        fourth.x = fourth_x;
        config.combatants.push_back(fourth);
    }
    return config;
}

int child_slot(const ntsd28::BattleWorld28& world) {
    for (std::size_t slot = 2; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
        const auto* entity = world.entity(slot);
        if (entity && entity->object_id == 808) return static_cast<int>(slot);
    }
    return -1;
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 5 && argc != 6 && argc != 7) return 2;
    try {
        const std::filesystem::path authority_root(argv[1]);
        const std::filesystem::path output(argv[2]);
        const int actor_oid = parse_actor(argv[3]);
        const int target_x = parse_target_x(argv[4]);
        const int third_x = argc >= 6 ? parse_target_x(argv[5]) : -1;
        const int fourth_x = argc == 7 ? parse_target_x(argv[6]) : -1;
        if (std::filesystem::exists(output)) return 3;
        const auto runtime_root = authority_root / "resources" / "runtime";
        ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
        std::string error;
        if (!session.initialize(make_config(actor_oid, target_x, third_x,
                                            fourth_x), error)) {
            std::cerr << error << '\n';
            return 4;
        }
        ntsd28_playable::GameSessionLfr28 recorder;
        if (!recorder.begin(session, error)) {
            std::cerr << error << '\n';
            return 5;
        }
        std::filesystem::create_directories(output);
        std::ofstream rows(output / "source-ticks.csv", std::ios::binary);
        std::ofstream rng(output / "source-rng.csv", std::ios::binary);
        if (!rows || !rng) return 5;
        rows << "tick,child_slot,child_action_before,child_latch_before,"
                "child_snapshot_before,child_counter_before,child_y_before,"
                "child_action_after,child_latch_after,child_snapshot_after,"
                "child_counter_after,child_x_after,child_y_after,child_z_after,"
                "child_precise_y_after,child_vy_after,actor_action,actor_counter,"
                "target_action,third_action,third_x,active_count,spawned,child_hit_count,"
                "child_applied_count,child_uj_count,"
                "last_hit_status,last_hit_attacker,last_hit_effect,"
                "last_hit_response,last_hit_target_action,fourth_action,"
                "fourth_x,child_hit_sequence\n"
             << std::setprecision(17);
        rng << "tick,crt_state,crt_calls,sync_counter,sync_index,sync_calls\n";
        int first_spawn = -1;
        int first_153 = -1;
        int first_155 = -1;
        int first_156 = -1;
        int first_divergence = -1;
        int first_uj = -1;
        int first_double_uj = -1;
        int total_child_hits = 0;
        std::size_t total_spawned = 0;
        for (int tick = 1; tick <= 120; ++tick) {
            const auto* before_world = session.world();
            const int before_slot = child_slot(*before_world);
            const auto* before = before_slot < 0 ? nullptr : before_world->entity(before_slot);
            const int action_before = before ? before->frame.action : -1;
            const int latch_before = before ? before->frame.action_latch : -1;
            const int snapshot_before = before ? before->frame.tick_action_snapshot : -1;
            const int counter_before = before ? before->frame.frame_counter : -1;
            const int y_before = before ? before->position.y : 0;
            session.set_input(0, ntsd28::InputButtons28{});
            session.set_input(1, ntsd28::InputButtons28{});
            if (third_x >= 0) session.set_input(2, ntsd28::InputButtons28{});
            if (fourth_x >= 0) session.set_input(3, ntsd28::InputButtons28{});
            session.step();
            if (!recorder.capture_after_step(session, error)) {
                std::cerr << error << '\n';
                return 8;
            }
            const auto* world = session.world();
            const auto* result = session.last_tick();
            if (!world || !result) return 6;
            total_spawned += result->spawns.spawned;
            const int slot = child_slot(*world);
            const auto* child = slot < 0 ? nullptr : world->entity(slot);
            if (child && first_spawn < 0) first_spawn = tick;
            const int action = child ? child->frame.action : -1;
            const int latch = child ? child->frame.action_latch : -1;
            if (action == 153 && first_153 < 0) first_153 = tick;
            if (action == 155 && first_155 < 0) first_155 = tick;
            if (action == 156 && first_156 < 0) first_156 = tick;
            if (child && action != latch && first_divergence < 0)
                first_divergence = tick;
            int child_hit_count = 0;
            int child_applied_count = 0;
            int child_uj_count = 0;
            int last_status = -1;
            int last_attacker = -1;
            int last_effect = -1;
            int last_response = -1;
            int last_target_action = -1;
            std::string hit_sequence;
            for (const auto& hit : result->hits) {
                if (slot < 0 || hit.target_slot != static_cast<std::size_t>(slot))
                    continue;
                ++child_hit_count;
                ++total_child_hits;
                last_status = static_cast<int>(hit.status);
                last_attacker = static_cast<int>(hit.attacker_slot);
                last_effect = hit.interaction ? hit.interaction->effect : -1;
                last_response = hit.target_type3_post_hit_action;
                last_target_action = child ? child->frame.action : -1;
                if (!hit_sequence.empty()) hit_sequence += ';';
                hit_sequence += std::to_string(hit.attacker_slot) + ':' +
                    std::to_string(static_cast<int>(hit.status)) + ':' +
                    std::to_string(last_effect) + ':' +
                    std::to_string(last_response);
                if (hit.status == ntsd28::WorldStandardHitStatus28::applied)
                    ++child_applied_count;
                if (hit.target_type3_post_hit_action >= 0) {
                    ++child_uj_count;
                    if (first_uj < 0) first_uj = tick;
                }
            }
            if (child_uj_count >= 2 && first_double_uj < 0)
                first_double_uj = tick;
            const auto* actor = world->entity(0);
            const auto* target = world->entity(1);
            const auto* third = third_x >= 0 ? world->entity(2) : nullptr;
            const auto* fourth = fourth_x >= 0 ? world->entity(3) : nullptr;
            rows << world->sequence() << ',' << slot << ',' << action_before << ','
                 << latch_before << ',' << snapshot_before << ',' << counter_before
                 << ',' << y_before << ',' << action << ',' << latch << ','
                 << (child ? child->frame.tick_action_snapshot : -1) << ','
                 << (child ? child->frame.frame_counter : -1) << ','
                 << (child ? child->position.x : 0) << ','
                 << (child ? child->position.y : 0) << ','
                 << (child ? child->position.z : 0) << ','
                 << (child ? child->position.precise_y : 0.0) << ','
                 << (child ? child->motion.y : 0.0) << ','
                 << (actor ? actor->frame.action : -1) << ','
                 << (actor ? actor->frame.frame_counter : -1) << ','
                 << (target ? target->frame.action : -1) << ','
                 << (third ? third->frame.action : -1) << ','
                 << (third ? third->position.x : 0) << ','
                 << world->active_count() << ',' << result->spawns.spawned
                 << ',' << child_hit_count << ','
                 << child_applied_count << ',' << child_uj_count << ','
                 << last_status << ',' << last_attacker << ',' << last_effect
                 << ',' << last_response << ',' << last_target_action
                 << ',' << (fourth ? fourth->frame.action : -1)
                 << ',' << (fourth ? fourth->position.x : 0)
                 << ',' << hit_sequence << '\n';
            const auto state = world->random().state();
            rng << world->sequence() << ',' << state.crt_state << ','
                << state.crt_calls << ',' << state.synchronized.counter << ','
                << state.synchronized.index << ',' << state.synchronized.calls << '\n';
        }
        rows.close();
        rng.close();
        if (!rows || !rng) return 7;
        std::vector<std::uint8_t> packets;
        if (!recorder.finish_to_memory(session, packets, error)) {
            std::cerr << error << '\n';
            return 9;
        }
        std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
        lfr.write(reinterpret_cast<const char*>(packets.data()),
                  static_cast<std::streamsize>(packets.size()));
        lfr.close();
        if (!lfr) return 10;
        std::cout << "actor=" << actor_oid << " targetX=" << target_x
                  << " thirdX=" << third_x << " fourthX=" << fourth_x
                  << " spawn=" << first_spawn << " action153=" << first_153
                  << " action155=" << first_155 << " action156=" << first_156
                  << " divergent=" << first_divergence << " uj=" << first_uj
                  << " doubleUj=" << first_double_uj
                  << " childHits=" << total_child_hits
                  << " totalSpawned=" << total_spawned << " ticks=120"
                  << " lfrBytes=" << packets.size() << '\n';
        return 0;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 8;
    }
}
