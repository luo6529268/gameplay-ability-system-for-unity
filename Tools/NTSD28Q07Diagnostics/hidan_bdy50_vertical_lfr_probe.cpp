#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 4) return 2;
    int target_x = 0;
    try {
        std::size_t count = 0;
        const std::string value(argv[3]);
        target_x = std::stoi(value, &count);
        if (count != value.size() || (target_x != 520 && target_x != 550 &&
                                      target_x != 1200)) return 2;
    } catch (...) { return 2; }
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 24;
    config.enemy_id = 56;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 24;
    actor.x = 500;
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = 37;
    ntsd28_playable::CombatantConfig28 target = actor;
    target.slot = 1;
    target.object_id = 56;
    target.x = target_x;
    target.action = 259;
    target.team = 2;
    config.combatants = {actor, target};

    const auto runtime = root / "resources" / "runtime";
    ntsd28_playable::GameSession28 session(runtime, runtime);
    std::string error;
    if (!session.initialize(config, error)) { std::cerr << error; return 4; }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) { std::cerr << error; return 5; }
    std::ofstream csv(output / "source-ticks.csv", std::ios::binary);
    if (!csv) return 6;
    csv << "tick,attacker_before,target_before,target_latch_before,first_bdy_kind,"
           "applied_kind0_hits,applied_dvy_hits,vertical_accumulated,"
           "attacker_after,target_after,target_hp,target_vy,target_hold\n";
    int first_qualifying_tick = -1;
    int applied_hits = 0;
    int applied_nonzero_dvy = 0;
    for (int tick = 1; tick <= 40; ++tick) {
        const auto* before_actor = session.world()->entity(0);
        const auto* before_target = session.world()->entity(1);
        if (!before_actor || !before_target) return 7;
        const int actor_before = before_actor->frame.action;
        const int target_before = before_target->frame.action;
        const int latch_before = before_target->frame.action_latch;
        int first_bdy_kind = -1;
        if (const auto* frame = before_target->definition->frame(latch_before)) {
            if (const auto* bdy = frame->first_block("bdy"))
                first_bdy_kind = bdy->values.integer("kind").value_or(0);
        }
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << error; return 8;
        }
        const auto* after_actor = session.world()->entity(0);
        const auto* after_target = session.world()->entity(1);
        if (!after_actor || !after_target) return 9;
        int hit_count = 0;
        int dvy_count = 0;
        int vertical_accumulated = 0;
        for (const auto& hit : session.last_tick()->hits) {
            if (hit.attacker_slot != 0 || hit.target_slot != 1 ||
                hit.status != ntsd28::WorldStandardHitStatus28::applied ||
                !hit.interaction || hit.interaction->kind != 0) continue;
            ++hit_count;
            ++applied_hits;
            if (hit.interaction->dvy != 0) {
                ++dvy_count;
                ++applied_nonzero_dvy;
                if (first_qualifying_tick < 0 &&
                    (first_bdy_kind == 50 || first_bdy_kind == 52))
                    first_qualifying_tick = tick;
            }
            if (hit.vertical_response.status ==
                ntsd28::VerticalHitResponseStatus28::accumulated)
                ++vertical_accumulated;
        }
        csv << tick << ',' << actor_before << ',' << target_before << ','
            << latch_before << ',' << first_bdy_kind << ',' << hit_count << ','
            << dvy_count << ',' << vertical_accumulated << ','
            << after_actor->frame.action << ',' << after_target->frame.action
            << ',' << after_target->current_hp << ',' << after_target->motion.y
            << ',' << after_target->motion_hold_timer << '\n';
    }
    csv.close();
    if (!csv) return 10;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << error; return 11;
    }
    std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
    if (!lfr) return 12;
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 13;
    std::cout << "{\"targetX\":" << target_x
              << ",\"ticks\":40,\"appliedHits\":" << applied_hits
              << ",\"appliedNonzeroDvy\":" << applied_nonzero_dvy
              << ",\"firstQualifyingTick\":" << first_qualifying_tick
              << ",\"lfrBytes\":" << bytes.size() << "}\n";
    return 0;
}
