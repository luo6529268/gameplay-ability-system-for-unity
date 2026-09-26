#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <stdexcept>

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) return 2;
    try {
        const std::filesystem::path root(argv[1]);
        ntsd28::ObjectDefinitionCatalog28 catalog;
        if (!catalog.load_extracted_root(root).success)
            throw std::runtime_error("formal object catalog failed");
        const auto* fused = catalog.find(52);
        if (!fused || !fused->definition || !fused->definition->ok())
            throw std::runtime_error("indexed OID52 unavailable");
        std::cout << "oid52_source=" << fused->source_path
                  << " action310_declared="
                  << (fused->definition->declared_frame(310) != nullptr)
                  << " action310_resolved="
                  << (fused->definition->frame(310) != nullptr) << '\n';
        if (fused->definition->declared_frame(310))
            throw std::runtime_error("formal OID52 action310 unexpectedly declared");
        if (!fused->definition->frame(310))
            throw std::runtime_error("formal native zero-frame projection absent");

        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 0x28A55A5Au;
        config.battle_mode = 0;
        config.character_id = 10;
        config.enemy_id = 11;
        config.background_id = 23;
        config.bgm_selection_49f18c = 2;
        ntsd28_playable::CombatantConfig28 primary;
        primary.slot = 0;
        primary.object_id = 10;
        primary.x = 304;
        primary.z = 600;
        primary.hp = 100;
        primary.base_hp = 500;
        primary.mp = 500;
        primary.team = 1;
        primary.action = 9;
        auto partner = primary;
        partner.slot = 1;
        partner.object_id = 11;
        partner.x = 300;
        partner.facing = true;
        config.combatants = {primary, partner};

        ntsd28_playable::GameSession28 session(root, root);
        std::string error;
        if (!session.initialize(config, error)) throw std::runtime_error(error);
        auto* world = session.world();
        if (!world || !world->entity(0) || !world->entity(1))
            throw std::runtime_error("formal initial combatants missing");
        world->entity(0)->motion.x = 20;
        std::ofstream rows{std::filesystem::path(argv[2])};
        if (!rows) throw std::runtime_error("formal CSV output unavailable");
        rows << std::setprecision(17);
        rows << "tick,slot,active,suspended,oid,action,state,hp,mp,x,y,z,precise_x,precise_y,precise_z,vx,vy,vz,timer338,gate328,display190,partner32c,primary330,partner334,frame_counter,active_count,crt_state,crt_calls,sync_counter,sync_index,sync_calls,sync_table_hash\n";
        for (int tick = 0; tick <= 3; ++tick) {
            if (tick) {
                session.set_input(0, {});
                session.set_input(1, {});
                session.step();
            }
            const auto rng = world->random().state();
            std::cout << "tick=" << tick;
            for (int slot = 0; slot < 2; ++slot) {
                const auto* entity = world->entity(slot);
                rows << tick << ',' << slot << ',' << (entity != nullptr)
                     << ',' << (!entity && !world->slot_available_for_spawn(slot));
                if (entity) {
                    const auto* frame = entity->definition->frame(entity->frame.action);
                    rows << ',' << entity->object_id << ',' << entity->frame.action
                         << ',' << (frame ? frame->values.integer("state").value_or(0) : -1)
                         << ',' << entity->current_hp << ',' << entity->current_mp
                         << ',' << entity->position.x << ',' << entity->position.y
                         << ',' << entity->position.z << ',' << entity->position.precise_x
                         << ',' << entity->position.precise_y << ',' << entity->position.precise_z
                         << ',' << entity->motion.x << ',' << entity->motion.y
                         << ',' << entity->motion.z << ',' << entity->input_special_timer_338
                         << ',' << entity->input_special_gate_328 << ','
                         << entity->fusion_display_timer_190 << ','
                         << entity->fusion_partner_slot_32c << ','
                         << entity->fusion_primary_definition_id_330 << ','
                         << entity->fusion_partner_definition_id_334 << ','
                         << entity->frame.frame_counter;
                } else {
                    for (int column = 0; column < 21; ++column) rows << ',';
                }
                rows << ',' << world->active_count() << ',' << rng.crt_state
                     << ',' << rng.crt_calls << ',' << rng.synchronized.counter
                     << ',' << rng.synchronized.index << ',' << rng.synchronized.calls
                     << ',' << world->random().synchronized_table_hash() << '\n';
                std::cout << " slot" << slot << "_oid="
                          << (entity ? entity->object_id : -1)
                          << " slot" << slot << "_action="
                          << (entity ? entity->frame.action : -1)
                          << " slot" << slot << "_x="
                          << (entity ? entity->position.x : -1);
            }
            if (tick && session.last_tick()) {
                const auto& fusion = session.last_tick()->fusions;
                std::cout << " fused=" << fusion.fused
                          << " unresolved=" << fusion.unresolved;
                for (const auto& event : fusion.events)
                    std::cout << " event=" << event.message;
            }
            std::cout << '\n';
        }
        if (!rows) throw std::runtime_error("formal CSV write failed");
        return world->entity(0) && !world->entity(1) &&
                       world->entity(0)->object_id == 52 &&
                       world->entity(0)->frame.action == 310
                   ? 0 : 3;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 4;
    }
}
