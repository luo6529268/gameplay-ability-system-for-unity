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
        if (argc != 3)
            throw std::runtime_error("expected runtime path and boundary/cross/positive mode");
        const std::wstring mode(argv[2]);
        if (mode != L"boundary" && mode != L"cross" && mode != L"positive")
            throw std::runtime_error("invalid witness mode");
        const double initial_y = mode == L"boundary" ? -11.2 : mode == L"cross" ? -1.25 : 3.75;
        const int target_y = mode == L"boundary" ? 0 : 100;
        const int target_z = mode == L"cross" ? 640 : 604;
        ntsd28::ObjectDefinitionCatalog28 catalog;
        if (!catalog.load_extracted_root(std::filesystem::path(argv[1])).success)
            throw std::runtime_error("formal catalog load failed");
        const auto* target_definition = catalog.find(99);
        const auto* subject_definition = catalog.find(902);
        if (!target_definition || !subject_definition ||
            target_definition->object_type != 0 ||
            subject_definition->object_type != 3 ||
            !target_definition->definition || !subject_definition->definition)
            throw std::runtime_error("formal OID/type guards failed");
        const auto* frame = subject_definition->definition->frame(0);
        if (!frame || frame->values.integer("hit_Fa").value_or(0) != 1)
            throw std::runtime_error("formal frame guard failed");

        ntsd28::BattleWorld28 world;
        world.random().reset_from_seed(42);
        auto spawn = [&](std::size_t slot, const ntsd28::ObjectDefinitionEntry28& entry,
                         int action, int x, double y, int z, int group) {
            ntsd28::SpawnRequest28 request;
            request.object_id = entry.object_id;
            request.object_type = entry.object_type;
            request.definition = entry.definition;
            request.initial_action = action;
            request.position.x = x;
            request.position.y = static_cast<int>(y);
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
        spawn(0, *target_definition, 0, 400, target_y, target_z, 2);
        spawn(1, *subject_definition, 0, 400, initial_y, 600, 1);
        auto* subject = world.entity(1);
        if (!subject || subject->object_type != 3 || subject->frame.action != 0)
            throw std::runtime_error("spawned identity guard failed");
        subject->position.precise_y = initial_y;
        if (subject->position.y != static_cast<int>(initial_y) ||
            subject->position.precise_y != initial_y)
            throw std::runtime_error("controlled fractional initial state guard failed");
        subject->motion = {};
        if (mode == L"cross") { subject->motion.x = 9.0; subject->motion.y = -2.8; }
        subject->object_ai_target_slot_3f8 = 0;
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        auto emit = [&](int call) {
            std::cout << "{\"call\":" << call
                      << ",\"vx\":" << subject->motion.x
                      << ",\"vy\":" << subject->motion.y
                      << ",\"vz\":" << subject->motion.z
                      << ",\"integerY\":" << subject->position.y
                      << ",\"action\":" << subject->frame.action
                      << ",\"target\":" << subject->object_ai_target_slot_3f8
                      << ",\"sourceX\":" << subject->position.precise_x
                      << ",\"sourceY\":" << subject->position.precise_y
                      << ",\"sourceZ\":" << subject->position.precise_z << "}\n";
        };
        emit(0);
        for (int call = 1; call <= 1; ++call) {
            const auto result = ntsd28::NativeAi28::step_non_character_hit_fa(world, 1, &catalog);
            if (!result.applicable || result.behavior != 1 || !result.special_tail_applied ||
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
