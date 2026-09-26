#include "ntsd28/battle_world.h"
#include "ntsd28/dat_parser.h"
#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <iterator>
#include <memory>
#include <stdexcept>
#include <string>

namespace {

std::shared_ptr<const ntsd28::DatDocument> load_definition(
    const std::filesystem::path& path) {
    std::ifstream input(path, std::ios::binary);
    if (!input) throw std::runtime_error("cannot read indexed OID219 DAT");
    const std::string bytes(std::istreambuf_iterator<char>{input}, {});
    auto definition = std::make_shared<const ntsd28::DatDocument>(
        ntsd28::DatParser{}.parse_text(bytes));
    if (!definition->ok() || definition->declared_frame(51) == nullptr ||
        definition->declared_frame(51)->values.integer("hit_Fa") != 5) {
        throw std::runtime_error("formal OID219 frame51 hit_Fa5 gate failed");
    }
    return definition;
}

ntsd28_playable::BattleConfig28 make_config() {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 0;
    config.character_id = 2;
    config.enemy_id = 2;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;

    ntsd28_playable::CombatantConfig28 ally;
    ally.slot = 0;
    ally.object_id = 2;
    ally.x = 251;
    ally.z = 350;
    ally.hp = 500;
    ally.mp = 300;
    ally.team = 1;
    ally.action = 0;

    ntsd28_playable::CombatantConfig28 opponent = ally;
    opponent.slot = 1;
    opponent.x = 1200;
    opponent.team = 2;
    config.combatants = {ally, opponent};
    return config;
}

bool run_case(const std::filesystem::path& runtime_root,
              const std::shared_ptr<const ntsd28::DatDocument>& definition,
              const std::filesystem::path& output,
              int source_group) {
    if (std::filesystem::exists(output)) {
        throw std::runtime_error("refusing to overwrite prior source trace");
    }
    ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
    std::string error;
    if (!session.initialize(make_config(), error)) {
        throw std::runtime_error("GameSession initialize: " + error);
    }
    auto* world = session.world();
    if (world == nullptr || world->entity(0) == nullptr ||
        world->entity(1) == nullptr || !world->slot_available_for_spawn(20)) {
        throw std::runtime_error("formal roster or OID219 slot unavailable");
    }

    ntsd28::SpawnRequest28 source;
    source.object_id = 219;
    source.object_type = 3;
    source.definition = definition;
    source.initial_action = 51;
    source.position.x = 100;
    source.position.z = 542;
    source.hp = 500;
    source.mp = 500;
    source.owner_slot = 0;
    source.battle_group = source_group;
    const auto spawned = world->spawn_at(20, source);
    if (!spawned.success) {
        throw std::runtime_error("formal OID219 spawn: " + spawned.message);
    }

    std::ofstream rows(output, std::ios::binary);
    if (!rows) throw std::runtime_error("cannot open source trace");
    rows << "tick,group,source_alive,ally_x,ally_z,active_count,child_count,"
            "child_slot,child_action,child_x,child_z,child_vx,child_target,"
            "child_frame_counter,child_action_latch\n";
    rows << std::setprecision(17);
    bool saw_child = false;
    for (int tick = 1; tick <= 8; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        const auto* ally = world->entity(0);
        if (ally == nullptr) throw std::runtime_error("formal ally disappeared");
        int child_count = 0;
        int child_slot = -1;
        const ntsd28::EntityState28* first_child = nullptr;
        for (std::size_t slot = 50; slot < ntsd28::EngineProfile28::maximum_slots;
             ++slot) {
            const auto* candidate = world->entity(slot);
            if (candidate == nullptr || candidate->object_id != 219) continue;
            ++child_count;
            if (first_child == nullptr) {
                first_child = candidate;
                child_slot = static_cast<int>(slot);
            }
        }
        saw_child |= child_count > 0;
        rows << tick << ',' << source_group << ','
             << (world->entity(20) != nullptr ? 1 : 0) << ','
             << ally->position.x << ',' << ally->position.z << ','
             << world->active_count() << ',' << child_count << ','
             << child_slot << ','
             << (first_child == nullptr ? -1 : first_child->frame.action) << ','
             << (first_child == nullptr ? 0 : first_child->position.precise_x) << ','
             << (first_child == nullptr ? 0 : first_child->position.precise_z) << ','
             << (first_child == nullptr ? 0 : first_child->motion.x) << ','
             << (first_child == nullptr ? -1
                                      : first_child->object_ai_target_slot_3f8)
             << ',' << (first_child == nullptr ? -1
                                          : first_child->frame.frame_counter)
             << ',' << (first_child == nullptr ? -1
                                          : first_child->frame.action_latch)
             << '\n';
    }
    if (!rows) throw std::runtime_error("source trace write failed");
    return saw_child;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) {
        std::cerr << "usage: hitfa5_full_session_source_probe <runtime_root> <output_dir>\n";
        return 2;
    }
    try {
        const std::filesystem::path root(argv[1]);
        const std::filesystem::path output(argv[2]);
        const auto definition = load_definition(root / "decoded_dat/w/e.dat");
        std::filesystem::create_directories(output);
        const bool positive = run_case(root, definition, output / "positive.csv", 1);
        const bool negative = run_case(root, definition, output / "negative.csv", 3);
        std::cout << "positive_birth=" << positive << " negative_birth=" << negative << '\n';
        return positive && !negative ? 0 : 3;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 4;
    }
}
