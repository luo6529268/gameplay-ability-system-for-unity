#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>

namespace {

void run_case(const std::filesystem::path& root, std::ofstream& output,
              int initial_counter, int initial_hold) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 7;
    config.enemy_id = 8;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 primary;
    primary.slot = 0;
    primary.object_id = 7;
    primary.x = 304;
    primary.y = 0;
    primary.z = 600;
    primary.hp = 100;
    primary.base_hp = 500;
    primary.mp = 500;
    primary.team = 1;
    primary.action = 9;
    auto partner = primary;
    partner.slot = 1;
    partner.object_id = 8;
    partner.x = 300;
    partner.facing = true;
    config.combatants = {primary, partner};

    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(config, error)) throw std::runtime_error(error);
    auto* first = session.world()->entity(0);
    if (!first) throw std::runtime_error("primary slot absent");
    first->frame.frame_counter = initial_counter;
    first->motion_hold_timer = initial_hold;
    const auto emit = [&](int tick) {
        const auto* world = session.world();
        const auto* entity = world->entity(0);
        const auto* child = world->entity(2);
        const auto* last = session.last_tick();
        int live_oid213 = 0;
        for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            const auto* active = world->entity(slot);
            if (active && active->object_id == 213) ++live_oid213;
        }
        output << initial_counter << ',' << initial_hold << ',' << tick << ','
               << (entity ? entity->object_id : -1) << ','
               << (entity ? entity->frame.action : -1) << ','
               << (entity ? entity->frame.action_latch : -1) << ','
               << (entity ? entity->frame.tick_action_snapshot : -1) << ','
               << (entity ? entity->frame.frame_counter : -1) << ','
               << (entity ? entity->motion_hold_timer : -999) << ','
               << (child ? child->object_id : -1) << ','
               << (last ? last->fusions.fused : 0) << ','
               << (last ? last->spawns.spawned : 0) << ','
               << live_oid213 << ',' << world->active_count() << '\n';
    };
    emit(0);
    for (int tick = 1; tick <= 3; ++tick) {
        session.set_input(0, ntsd28::InputButtons28{});
        session.set_input(1, ntsd28::InputButtons28{});
        session.step();
        if (!session.last_tick()) throw std::runtime_error("missing complete tick");
        emit(tick);
    }
}

}  // namespace

int main(int argc, char** argv) {
    if (argc != 3) return 2;
    try {
        const std::filesystem::path root(argv[1]);
        const std::filesystem::path output(argv[2]);
        if (std::filesystem::exists(output))
            throw std::runtime_error("refusing to overwrite output");
        std::ofstream rows(output);
        if (!rows) throw std::runtime_error("cannot open output");
        rows << "initial_counter,initial_hold,tick,oid,action,latch,snapshot,"
                "counter,hold,slot2_oid,fused,spawned,live_oid213,world_active_count\n";
        run_case(root, rows, 7, 3);
        run_case(root, rows, 7, 0);
        run_case(root, rows, 0, 3);
        if (!rows) throw std::runtime_error("output write failed");
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
