#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 5) return 2;
    int target_x = 0;
    int attack_tick = 0;
    try {
        std::size_t parsed = 0;
        target_x = std::stoi(argv[3], &parsed);
        if (parsed != std::string(argv[3]).size()) return 2;
        attack_tick = std::stoi(argv[4], &parsed);
        if (parsed != std::string(argv[4]).size()) return 2;
    } catch (...) { return 2; }
    if (target_x < 550 || target_x > 850 || attack_tick < 2 || attack_tick > 10)
        return 2;
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    const auto csv_path = output / "source-ticks.csv";
    const auto lfr_path = output / "source-packets.lfr";
    if (std::filesystem::exists(csv_path) || std::filesystem::exists(lfr_path))
        return 3;
    std::filesystem::create_directories(output);
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 14;
    config.enemy_id = 2;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 14;
    actor.x = 500;
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = 297;
    ntsd28_playable::CombatantConfig28 target = actor;
    target.slot = 1;
    target.object_id = 2;
    target.x = target_x;
    target.action = 0;
    target.team = 2;
    config.combatants = {actor, target};
    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(config, error)) { std::cerr << error; return 4; }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) { std::cerr << error; return 5; }
    std::ofstream csv(csv_path, std::ios::binary);
    if (!csv) return 6;
    csv << "tick,actor_action,actor_hp,actor_x,target_action,target_hp,target_x,"
           "pur_slot,pur_action,pur_x,pur_y,pur_z,pur_owner,pur_group,pur_latch,"
           "transfer_count,relation_latch_count\n";
    int transfers = 0;
    for (int tick = 1; tick <= 40; ++tick) {
        ntsd28::InputButtons28 buttons;
        if (tick <= 2) buttons.set(ntsd28::InputKey28::left);
        if (tick >= attack_tick) buttons.set(ntsd28::InputKey28::attack);
        session.set_input(0, ntsd28::InputButtons28{});
        session.set_input(1, buttons);
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << error; return 7;
        }
        const auto* world = session.world();
        const auto* first = world->entity(0);
        const auto* second = world->entity(1);
        if (!first || !second) return 8;
        int transfer_count = 0;
        int relation_count = 0;
        for (const auto& hit : session.last_tick()->hits)
            if (hit.target_type3_ownership_transferred) ++transfer_count;
        for (const auto& hit : session.last_tick()->relation_hits)
            if (hit.special_hit_latch_set) ++relation_count;
        transfers += transfer_count + relation_count;
        const ntsd28::EntityState28* pur = nullptr;
        int pur_slot = -1;
        for (std::size_t slot = 2; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            const auto* candidate = world->entity(slot);
            if (candidate && candidate->object_id == 222) {
                pur = candidate;
                pur_slot = static_cast<int>(slot);
                break;
            }
        }
        csv << world->sequence() << ',' << first->frame.action << ','
            << first->current_hp << ',' << first->position.x << ','
            << second->frame.action << ',' << second->current_hp << ','
            << second->position.x << ',' << pur_slot << ','
            << (pur ? pur->frame.action : -1) << ','
            << (pur ? pur->position.x : 0) << ','
            << (pur ? pur->position.y : 0) << ','
            << (pur ? pur->position.z : 0) << ','
            << (pur ? pur->owner_slot : -1) << ','
            << (pur ? pur->battle_group : -1) << ','
            << (pur ? pur->special_hit_latch_0eb : false) << ','
            << transfer_count << ',' << relation_count << '\n';
    }
    csv.close();
    if (!csv) return 9;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << error; return 10;
    }
    std::ofstream lfr(lfr_path, std::ios::binary);
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 11;
    std::cout << "target_x=" << target_x << " attack_tick=" << attack_tick
              << " transfers=" << transfers << " ticks=40\n";
    return 0;
}
