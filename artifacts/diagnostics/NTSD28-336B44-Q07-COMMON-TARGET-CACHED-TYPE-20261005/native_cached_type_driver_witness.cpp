#include "ntsd28/battle_world.h"
#include "ntsd28/native_ai.h"
#include "ntsd28/object_catalog.h"
#include "ntsd28/simulation_tick_driver.h"

#include <filesystem>
#include <iomanip>
#include <iostream>
#include <limits>
#include <stdexcept>

// Controlled production OPoint/lifecycle/reuse witness; not the playable host.
int wmain(int argc, wchar_t** argv)
{
    try {
        if (argc != 2) throw std::runtime_error("expected formal runtime root");
        ntsd28::ObjectDefinitionCatalog28 catalog;
        if (!catalog.load_extracted_root(std::filesystem::path(argv[1])).success)
            throw std::runtime_error("formal catalog load failed");
        ntsd28::BattleWorld28 world;
        world.random().reset_from_seed(42);
        auto spawn = [&](std::size_t slot, int oid, int action, int x, int y, int z, int group) {
            const auto* entry = catalog.find(oid);
            if (!entry || !entry->definition) throw std::runtime_error("definition missing");
            ntsd28::SpawnRequest28 r;
            r.object_id = oid;
            r.object_type = entry->object_type;
            r.definition = entry->definition;
            r.initial_action = action;
            r.position.x = x; r.position.precise_x = x;
            r.position.y = y; r.position.precise_y = y;
            r.position.z = z; r.position.precise_z = z;
            r.hp = 500; r.mp = 200;
            r.owner_slot = static_cast<int>(slot); r.battle_group = group;
            if (!world.spawn_at(slot, r).success) throw std::runtime_error("spawn failed");
        };
        auto place = [&](std::size_t slot, int x, int y, int z) {
            auto* e = world.entity(slot);
            if (!e) throw std::runtime_error("place missing slot");
            e->position.x = x; e->position.precise_x = x;
            e->position.y = y; e->position.precise_y = y;
            e->position.z = z; e->position.precise_z = z;
            e->motion = {};
        };
        spawn(0, 2, 193, 100, 0, 600, 2);
        spawn(1, 901, 0, 100, 0, 600, 2);
        const auto clone_birth = world.materialize_supported_spawns(0, catalog);
        if (!clone_birth.success || clone_birth.spawned != 1 || !world.entity(50) ||
            world.entity(50)->object_id != 33 || world.entity(50)->object_type != 0)
            throw std::runtime_error("formal Naruto opoint did not birth clone33 at slot50");
        world.entity(0)->frame.action = 0;
        spawn(51, 902, 0, 500, -100, 600, 1);
        place(50, 700, 0, 604);
        const auto acquired = ntsd28::NativeAi28::step_non_character_hit_fa(world, 51, &catalog);
        if (!acquired.scanned_targets || world.entity(51)->object_ai_target_slot_3f8 != 50)
            throw std::runtime_error("common scan did not acquire actual clone slot50");
        // Controlled entry to an existing terminal frame, not natural input/death proof.
        world.entity(50)->frame.action = 399;
        world.entity(50)->frame.action_latch = 399;
        world.entity(50)->frame.frame_counter = 3;
        (void)world.step_frame_slot(50);
        const auto removed = world.resolve_pending_lifecycle(50);
        if (world.entity(50) != nullptr || removed.despawned != 1 ||
            world.entity(51)->object_ai_target_slot_3f8 != 50)
            throw std::runtime_error("formal terminal frame did not release slot while retaining cache");
        world.entity(1)->frame.action = 282;
        world.entity(1)->frame.frame_counter = 0;
        const auto replacement = world.materialize_supported_spawns(1, catalog);
        auto* target = world.entity(50);
        if (!replacement.success || replacement.spawned != 1 || !target ||
            target->object_id != 902 || target->object_type != 3 || target->current_hp != 500)
            throw std::runtime_error("formal Genma opoint did not reuse slot50 for living noncharacter");
        world.entity(1)->frame.action = 0;
        place(0, 100, 0, 600); place(1, 100, 0, 600);
        place(50, 700, 0, 604); place(51, 500, -100, 600);
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        auto emit = [&](int tick) {
            const auto* e = world.entity(51);
            std::cout << "{\"completedTick\":" << tick
                      << ",\"sourceX\":" << e->position.precise_x
                      << ",\"sourceIntegerX\":" << e->position.x
                      << ",\"sourceY\":" << e->position.precise_y
                      << ",\"integerY\":" << e->position.y
                      << ",\"sourceZ\":" << e->position.precise_z
                      << ",\"vx\":" << e->motion.x << ",\"vy\":" << e->motion.y
                      << ",\"vz\":" << e->motion.z << ",\"action\":" << e->frame.action
                      << ",\"counter\":" << e->frame.frame_counter
                      << ",\"target\":" << e->object_ai_target_slot_3f8
                      << ",\"hp\":" << e->current_hp << "}\n";
        };
        emit(0);
        ntsd28::SimulationTickDriver28 driver;
        const auto result = driver.step(world, catalog, {});
        emit(1);
        const auto& ai = result.native_object_hit_fa.at(51);
        if (!ai.applicable || ai.behavior != 1 || !ai.common_target_path ||
            !ai.retained_cached_target || ai.scanned_targets ||
            world.entity(51)->object_ai_target_slot_3f8 != 50 ||
            world.entity(51)->motion.x != 0.85 ||
            result.diagnostics.unsupported_object_hit_fa != 0)
            throw std::runtime_error("cached noncharacter did not survive complete tick");
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
