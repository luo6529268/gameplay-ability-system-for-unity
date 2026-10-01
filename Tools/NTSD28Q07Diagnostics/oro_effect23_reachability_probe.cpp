#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <set>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 5) return 2;
    int target_x = 0;
    int actor_facing = 0;
    try {
        std::size_t x_count = 0;
        std::size_t facing_count = 0;
        const std::string x_text(argv[3]);
        const std::string facing_text(argv[4]);
        target_x = std::stoi(x_text, &x_count);
        actor_facing = std::stoi(facing_text, &facing_count);
        if (x_count != x_text.size() || facing_count != facing_text.size() ||
            (target_x != 350 && target_x != 550 && target_x != 700 &&
             target_x != 1200 && target_x != 2000) ||
            (actor_facing != 0 && actor_facing != 1))
            return 2;
    } catch (...) { return 2; }
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 20;
    config.enemy_id = 2;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 20;
    actor.x = 500;
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = 288;
    actor.facing = actor_facing != 0;
    auto target = actor;
    target.slot = 1;
    target.object_id = 2;
    target.x = target_x;
    target.action = 0;
    target.team = 2;
    config.combatants = {actor, target};

    const auto runtime = root / "resources" / "runtime";
    ntsd28_playable::GameSession28 session(runtime, runtime);
    std::string error;
    if (!session.initialize(config, error)) { std::cerr << error; return 4; }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) { std::cerr << error; return 5; }
    std::filesystem::create_directories(output);
    std::ofstream csv(output / "source-ticks.csv", std::ios::binary);
    if (!csv) return 6;
    csv << "tick,parent_action,child_count,first_child_slot,first_child_action,"
           "first_child_x,"
           "spawned,child_effect23_candidates,child_effect23_applied,"
           "last_dvx,last_impulse_x,target_action,target_x,target_hp,"
           "target_vx,target_pending_x\n"
        << std::setprecision(17);

    int first_child_tick = -1;
    int first_effect23_tick = -1;
    int first_applied_tick = -1;
    int total_candidates = 0;
    int total_applied = 0;
    for (int tick = 1; tick <= 80; ++tick) {
        std::set<std::size_t> child_slots;
        for (std::size_t slot = 2; slot < ntsd28::EngineProfile28::maximum_slots;
             ++slot) {
            const auto* entity = session.world()->entity(slot);
            if (entity && entity->object_id == 888) child_slots.insert(slot);
        }
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << error; return 7;
        }
        const auto* world = session.world();
        const auto* result = session.last_tick();
        if (!world || !result || !world->entity(0) || !world->entity(1))
            return 8;
        int first_child_slot = -1;
        int first_child_action = -1;
        int first_child_x = 0;
        int child_count = 0;
        for (std::size_t slot = 2; slot < ntsd28::EngineProfile28::maximum_slots;
             ++slot) {
            const auto* entity = world->entity(slot);
            if (!entity || entity->object_id != 888) continue;
            child_slots.insert(slot);
            ++child_count;
            if (first_child_slot < 0) {
                first_child_slot = static_cast<int>(slot);
                first_child_action = entity->frame.action;
                first_child_x = entity->position.x;
            }
        }
        if (child_count > 0 && first_child_tick < 0) first_child_tick = tick;
        int effect23_candidates = 0;
        int effect23_applied = 0;
        int last_dvx = 0;
        double last_impulse_x = 0.0;
        for (const auto& hit : result->hits) {
            if (!child_slots.count(hit.attacker_slot) || hit.target_slot != 1 ||
                !hit.interaction || hit.interaction->effect != 23) continue;
            ++effect23_candidates;
            ++total_candidates;
            last_dvx = hit.interaction->dvx;
            last_impulse_x = hit.horizontal_response.impulse_x;
            if (first_effect23_tick < 0) first_effect23_tick = tick;
            if (hit.status == ntsd28::WorldStandardHitStatus28::applied) {
                ++effect23_applied;
                ++total_applied;
                if (first_applied_tick < 0) first_applied_tick = tick;
            }
        }
        const auto* after_actor = world->entity(0);
        const auto* after_target = world->entity(1);
        csv << tick << ',' << after_actor->frame.action << ',' << child_count
            << ',' << first_child_slot << ',' << first_child_action << ','
            << first_child_x << ','
            << result->spawns.spawned << ',' << effect23_candidates << ','
            << effect23_applied << ',' << last_dvx << ',' << last_impulse_x
            << ','
            << after_target->frame.action << ',' << after_target->position.x
            << ',' << after_target->current_hp << ',' << after_target->motion.x
            << ',' << after_target->pending_hit_impulse.total.x << '\n';
    }
    csv.close();
    if (!csv) return 9;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << error; return 10;
    }
    std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
    if (!lfr) return 11;
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 12;
    std::cout << "{\"targetX\":" << target_x
              << ",\"actorFacing\":" << actor_facing
              << ",\"ticks\":80,\"firstChildTick\":" << first_child_tick
              << ",\"firstEffect23Tick\":" << first_effect23_tick
              << ",\"firstAppliedTick\":" << first_applied_tick
              << ",\"effect23Candidates\":" << total_candidates
              << ",\"effect23Applied\":" << total_applied
              << ",\"lfrBytes\":" << bytes.size() << "}\n";
    return 0;
}
