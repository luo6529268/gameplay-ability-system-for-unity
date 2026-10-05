#include "ntsd28/battle_world.h"
#include "ntsd28/object_catalog.h"
#include "ntsd28/simulation_tick_driver.h"

#include <filesystem>
#include <iomanip>
#include <iostream>
#include <limits>
#include <stdexcept>

// Full Core tick diagnostic; this executable is not the formal playable host.
int wmain(int argc, wchar_t** argv)
{
    try {
        if (argc != 2)
            throw std::runtime_error("expected formal runtime root");
        ntsd28::ObjectDefinitionCatalog28 catalog;
        if (!catalog.load_extracted_root(std::filesystem::path(argv[1])).success)
            throw std::runtime_error("formal catalog load failed");
        const auto* target_definition = catalog.find(99);
        const auto* subject_definition = catalog.find(206);
        if (!target_definition || !subject_definition ||
            target_definition->object_type != 0 || subject_definition->object_type != 3 ||
            !target_definition->definition || !subject_definition->definition)
            throw std::runtime_error("formal OID/type guard failed");
        const auto* frame = subject_definition->definition->frame(54);
        if (!frame || frame->values.integer("hit_Fa").value_or(0) != 3 ||
            frame->values.integer("wait").value_or(-1) != 3 ||
            frame->values.integer("next").value_or(-1) != 999)
            throw std::runtime_error("formal frame54/wait3/next999 guard failed");

        ntsd28::BattleWorld28 world;
        world.random().reset_from_seed(42);
        auto spawn = [&](std::size_t slot, const ntsd28::ObjectDefinitionEntry28& entry,
                         int action, int x, int y, int z, int group) {
            ntsd28::SpawnRequest28 request;
            request.object_id = entry.object_id;
            request.object_type = entry.object_type;
            request.definition = entry.definition;
            request.initial_action = action;
            request.position.x = x;
            request.position.y = y;
            request.position.z = z;
            request.position.precise_x = x;
            request.position.precise_y = y;
            request.position.precise_z = z;
            request.hp = 500;
            request.mp = 200;
            request.owner_slot = static_cast<int>(slot);
            request.battle_group = group;
            if (!world.spawn_at(slot, request).success)
                throw std::runtime_error("spawn_at failed");
        };
        spawn(0, *target_definition, 0, 1000, 0, 604, 2);
        spawn(1, *subject_definition, 54, 500, -100, 600, 1);
        auto* subject = world.entity(1);
        if (!subject || subject->frame.frame_counter != 0 ||
            subject->position.precise_x != 500 || subject->position.precise_y != -100)
            throw std::runtime_error("integer initial state guard failed");
        subject->motion = {};
        subject->object_ai_target_slot_3f8 = 0;

        ntsd28::SimulationTickDriver28 driver;
        const ntsd28::SimulationTickOptions28 options;
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        auto emit = [&](int tick, int behavior, std::size_t unsupported) {
            std::cout << "{\"completedTick\":" << tick
                      << ",\"sourceX\":" << subject->position.precise_x
                      << ",\"sourceIntegerX\":" << subject->position.x
                      << ",\"sourceY\":" << subject->position.precise_y
                      << ",\"integerY\":" << subject->position.y
                      << ",\"sourceZ\":" << subject->position.precise_z
                      << ",\"vx\":" << subject->motion.x
                      << ",\"vy\":" << subject->motion.y
                      << ",\"vz\":" << subject->motion.z
                      << ",\"action\":" << subject->frame.action
                      << ",\"counter\":" << subject->frame.frame_counter
                      << ",\"target\":" << subject->object_ai_target_slot_3f8
                      << ",\"hp\":" << subject->hp
                      << ",\"behavior\":" << behavior
                      << ",\"unsupported\":" << unsupported << "}\n";
        };
        emit(0, 0, 0);
        for (int tick = 1; tick <= 4; ++tick) {
            const auto result = driver.step(world, catalog, options);
            subject = world.entity(1);
            if (!subject || result.native_object_hit_fa.size() <= 1)
                throw std::runtime_error("subject or native consumer result missing");
            const auto& ai = result.native_object_hit_fa[1];
            emit(tick, ai.behavior, result.diagnostics.unsupported_object_hit_fa);
            if (!ai.applicable || ai.behavior != 3 || !ai.special_tail_applied ||
                result.diagnostics.unsupported_object_hit_fa != 0 ||
                subject->object_ai_target_slot_3f8 != 0 ||
                subject->frame.action != (tick < 4 ? 54 : 0) ||
                subject->frame.frame_counter != (tick < 4 ? tick : 0))
                throw std::runtime_error("four normal Core ticks/frame progression guard failed");
        }
        if (subject->position.precise_x != 507.0 || subject->position.x != 507 ||
            subject->motion.x != 2.8 || subject->position.precise_y != -100 ||
            subject->motion.y != 0 || subject->motion.z != 0)
            throw std::runtime_error("native fourth-tick result guard failed");
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
