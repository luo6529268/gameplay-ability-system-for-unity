#include "ntsd28/battle_world.h"
#include "ntsd28/native_ai.h"
#include "ntsd28/object_catalog.h"
#include "ntsd28/simulation_tick_driver.h"

#include <filesystem>
#include <iomanip>
#include <iostream>
#include <limits>
#include <stdexcept>

int wmain(int argc, wchar_t** argv)
{
    try {
        if (argc != 2) throw std::runtime_error("expected formal runtime root");
        ntsd28::ObjectDefinitionCatalog28 catalog;
        if (!catalog.load_extracted_root(std::filesystem::path(argv[1])).success)
            throw std::runtime_error("formal catalog load failed");
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        for (int scenario = 0; scenario < 4; ++scenario) {
            ntsd28::BattleWorld28 world;
            world.random().reset_from_seed(42);
            auto spawn = [&](std::size_t slot, int oid, int action, int x, int y, int z, int group) {
                const auto* entry = catalog.find(oid);
                if (!entry || !entry->definition) throw std::runtime_error("definition missing");
                ntsd28::SpawnRequest28 r;
                r.object_id = oid; r.object_type = entry->object_type;
                r.definition = entry->definition; r.initial_action = action;
                r.position.x = x; r.position.precise_x = x;
                r.position.y = y; r.position.precise_y = y;
                r.position.z = z; r.position.precise_z = z;
                r.hp = 500; r.mp = 200;
                r.owner_slot = static_cast<int>(slot); r.battle_group = group;
                if (!world.spawn_at(slot, r).success) throw std::runtime_error("spawn failed");
            };
            spawn(0, 99, 0, 100, 0, 600, 1);
            spawn(50, 99, 0, 700, 0, 604, 2);
            spawn(51, 902, 0, 500, -100, 600, 1);
            const auto acquired = ntsd28::NativeAi28::step_non_character_hit_fa(world, 51, &catalog);
            if (!acquired.scanned_targets || world.entity(51)->object_ai_target_slot_3f8 != 50)
                throw std::runtime_error("initial common scan did not acquire slot50");
            auto* subject = world.entity(51);
            auto* target = world.entity(50);
            subject->position.y = -100; subject->position.precise_y = -100;
            subject->motion = {};
            const bool lying = scenario == 0 || scenario == 3;
            target->current_hp = lying ? 500 : 0;
            target->frame.action = lying ? 230 : 0;
            target->frame.action_latch = target->frame.action;
            target->frame.frame_counter = 0;
            if (scenario == 2) world.entity(0)->battle_group = 2;
            auto emit = [&](int tick) {
                const auto* e = world.entity(51);
                std::cout << "{\"scenario\":" << scenario << ",\"completedTick\":" << tick
                          << ",\"sourceX\":" << e->position.precise_x
                          << ",\"sourceIntegerX\":" << e->position.x
                          << ",\"sourceY\":" << e->position.precise_y
                          << ",\"integerY\":" << e->position.y
                          << ",\"sourceZ\":" << e->position.precise_z
                          << ",\"vx\":" << e->motion.x << ",\"vy\":" << e->motion.y
                          << ",\"vz\":" << e->motion.z << ",\"action\":" << e->frame.action
                          << ",\"counter\":" << e->frame.frame_counter
                          << ",\"target\":" << e->object_ai_target_slot_3f8
                          << ",\"hp\":" << e->current_hp
                          << ",\"targetActive\":" << (world.entity(50) != nullptr ? "true" : "false")
                          << "}\n";
            };
            if (scenario == 3) {
                emit(0);
                ntsd28::SimulationTickDriver28 driver;
                const auto result = driver.step(world, catalog, {});
                emit(1);
                const auto& ai = result.native_object_hit_fa.at(51);
                if (!ai.applicable || ai.behavior != 1 || !ai.scanned_targets ||
                    ai.retained_cached_target || subject->object_ai_target_slot_3f8 != 50 ||
                    world.entity(50) == nullptr || subject->motion.x != 0.85 ||
                    result.diagnostics.unsupported_object_hit_fa != 0)
                    throw std::runtime_error("active lying cached target did not survive complete tick");
            } else {
                const auto result = ntsd28::NativeAi28::step_non_character_hit_fa(world, 51, &catalog);
                emit(0);
                const int expected_target = scenario == 2 ? 0 : 50;
                const double expected_vx = scenario == 2 ? -0.85 : 0.85;
                if (!result.applicable || !result.scanned_targets || result.retained_cached_target ||
                    subject->object_ai_target_slot_3f8 != expected_target ||
                    subject->motion.x != expected_vx || subject->current_hp != 500)
                    throw std::runtime_error("stale active target/direct alternative rule guard failed");
            }
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}

