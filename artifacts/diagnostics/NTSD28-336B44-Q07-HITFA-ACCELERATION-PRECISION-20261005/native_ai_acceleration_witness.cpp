#include "ntsd28/battle_world.h"
#include "ntsd28/native_ai.h"
#include "ntsd28/object_catalog.h"

#include <filesystem>
#include <iomanip>
#include <iostream>
#include <limits>
#include <stdexcept>

// Source-model diagnostic only; this binary does not replace the formal EXE.
int wmain(int argc, wchar_t** argv)
{
    try {
        if (argc != 2)
            throw std::runtime_error("expected formal resources/runtime path");
        ntsd28::ObjectDefinitionCatalog28 catalog;
        if (!catalog.load_extracted_root(std::filesystem::path(argv[1])).success)
            throw std::runtime_error("formal catalog load failed");
        const auto* target_definition = catalog.find(99);
        const auto* subject_definition = catalog.find(518);
        if (!target_definition || !subject_definition ||
            target_definition->object_type != 0 ||
            subject_definition->object_type != 3 ||
            !target_definition->definition || !subject_definition->definition)
            throw std::runtime_error("formal OID/type guards failed");
        const auto* frame = subject_definition->definition->frame(1);
        if (!frame || frame->values.integer("hit_Fa").value_or(0) != 2)
            throw std::runtime_error("formal frame guard failed");

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
        spawn(1, *subject_definition, 1, 400, -100, 600, 1);
        auto* subject = world.entity(1);
        if (!subject || subject->object_type != 3 || subject->frame.action != 1)
            throw std::runtime_error("spawned identity guard failed");
        subject->motion = {};
        subject->object_ai_target_slot_3f8 = 0;
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        auto emit = [&](int call) {
            std::cout << "{\"call\":" << call
                      << ",\"vx\":" << subject->motion.x
                      << ",\"action\":" << subject->frame.action
                      << ",\"target\":" << subject->object_ai_target_slot_3f8
                      << ",\"sourceX\":" << subject->position.precise_x
                      << ",\"sourceY\":" << subject->position.precise_y
                      << ",\"sourceZ\":" << subject->position.precise_z << "}\n";
        };
        emit(0);
        for (int call = 1; call <= 10; ++call) {
            const auto result = ntsd28::NativeAi28::step_non_character_hit_fa(world, 1, &catalog);
            if (!result.applicable || result.behavior != 2 || !result.special_tail_applied ||
                subject->object_ai_target_slot_3f8 != 0)
                throw std::runtime_error("native branch guard failed");
            emit(call);
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
