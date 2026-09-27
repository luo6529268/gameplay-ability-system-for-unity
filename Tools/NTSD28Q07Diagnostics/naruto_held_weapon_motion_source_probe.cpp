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
    if (!input) throw std::runtime_error("cannot read formal weapon DAT");
    const std::string bytes(std::istreambuf_iterator<char>{input}, {});
    auto definition = std::make_shared<const ntsd28::DatDocument>(
        ntsd28::DatParser{}.parse_text(bytes));
    if (!definition->ok()) throw std::runtime_error("formal weapon DAT parse failed");
    return definition;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) {
        std::cerr << "usage: naruto_held_weapon_motion_source_probe "
                     "<formal_runtime_root> <new_output_csv>\n";
        return 2;
    }
    const std::filesystem::path runtime_root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to overwrite a prior trace\n";
        return 3;
    }
    try {
        const auto weapon_definition = load_definition(
            runtime_root / L"decoded_dat" / L"w" / L"4.dat");
        if (weapon_definition->declared_frame(64) == nullptr ||
            weapon_definition->declared_frame(64)->values.integer("state") != 1004) {
            throw std::runtime_error("formal OID120 ground frame64 gate failed");
        }

        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 0x28A55A5Au;
        config.battle_mode = 0;
        config.character_id = 2;
        config.enemy_id = 7;
        config.background_id = 23;
        config.bgm_selection_49f18c = 2;
        ntsd28_playable::CombatantConfig28 naruto;
        naruto.slot = 0;
        naruto.object_id = 2;
        naruto.x = 200;
        naruto.z = 542;
        naruto.hp = 500;
        naruto.mp = 500;
        naruto.team = 1;
        naruto.action = 0;
        ntsd28_playable::CombatantConfig28 opponent;
        opponent.slot = 1;
        opponent.object_id = 7;
        opponent.x = 1200;
        opponent.z = 542;
        opponent.hp = 500;
        opponent.mp = 500;
        opponent.team = 2;
        opponent.action = 0;
        config.combatants = {naruto, opponent};

        ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
        std::string error;
        if (!session.initialize(config, error)) {
            throw std::runtime_error("GameSession initialize: " + error);
        }
        auto* world = session.world();
        if (world == nullptr || world->entity(0) == nullptr ||
            world->entity(1) == nullptr || !world->slot_available_for_spawn(50)) {
            throw std::runtime_error("expected formal roster and free slot50");
        }
        ntsd28::SpawnRequest28 weapon;
        weapon.object_id = 120;
        weapon.object_type = 1;
        weapon.definition = weapon_definition;
        weapon.initial_action = 64;
        weapon.position.x = 190;
        weapon.position.z = 542;
        weapon.hp = 100;
        weapon.mp = 0;
        const auto spawned = world->spawn_at(50, weapon);
        if (!spawned.success) {
            throw std::runtime_error("formal ground OID120 spawn: " + spawned.message);
        }

        std::filesystem::create_directories(output.parent_path());
        std::ofstream rows(output, std::ios::binary);
        if (!rows) throw std::runtime_error("cannot open trace CSV");
        rows << std::setprecision(17);
        rows << "tick,input,phase,actor_action,actor_link,actor_child_slot,"
                "actor_x,actor_z,actor_precise_x,actor_precise_z,"
                "weapon_present,weapon_action,weapon_link,weapon_parent_slot,"
                "weapon_x,weapon_z,weapon_precise_x,weapon_precise_z\n";
        bool pickup = false;
        int moving_held_x_ticks = 0;
        int moving_held_z_ticks = 0;
        int previous_holder_x = 0;
        int previous_weapon_x = 0;
        int previous_holder_z = 0;
        int previous_weapon_z = 0;
        bool prior_held = false;
        for (int tick = 1; tick <= 24; ++tick) {
            ntsd28::InputButtons28 buttons;
            const char* input = "none";
            if (tick <= 2) {
                buttons.set(ntsd28::InputKey28::attack);
                input = "attack";
            } else if (tick >= 8 && tick <= 14) {
                buttons.set(ntsd28::InputKey28::right);
                input = "right";
            } else if (tick >= 15 && tick <= 21) {
                buttons.set(ntsd28::InputKey28::depth_down);
                input = "down";
            }
            session.set_input(0, buttons);
            session.set_input(1, {});
            session.step();
            const auto* actor = world->entity(0);
            const auto* held = world->entity(50);
            if (actor == nullptr) throw std::runtime_error("Naruto despawned");
            const bool linked = held != nullptr &&
                actor->interaction_state % 100 == 1 &&
                actor->linked_child_slot == 50 &&
                held->interaction_state == -1 &&
                held->linked_parent_slot == 0;
            pickup = pickup || linked;
            if (linked && prior_held && actor->position.x != previous_holder_x &&
                held->position.x != previous_weapon_x) {
                ++moving_held_x_ticks;
            }
            if (linked && prior_held && actor->position.z != previous_holder_z &&
                held->position.z != previous_weapon_z) {
                ++moving_held_z_ticks;
            }
            rows << tick << ',' << input << ',' << world->input_update_phase_4a0b90()
                 << ',' << actor->frame.action << ',' << actor->interaction_state
                 << ',' << actor->linked_child_slot << ',' << actor->position.x
                 << ',' << actor->position.z << ',' << actor->position.precise_x
                 << ',' << actor->position.precise_z << ',' << (held == nullptr ? 0 : 1)
                 << ',' << (held == nullptr ? -1 : held->frame.action)
                 << ',' << (held == nullptr ? 0 : held->interaction_state)
                 << ',' << (held == nullptr ? -1 : held->linked_parent_slot)
                 << ',' << (held == nullptr ? 0 : held->position.x)
                 << ',' << (held == nullptr ? 0 : held->position.z)
                 << ',' << (held == nullptr ? 0 : held->position.precise_x)
                 << ',' << (held == nullptr ? 0 : held->position.precise_z) << '\n';
            prior_held = linked;
            if (held != nullptr) {
                previous_holder_x = actor->position.x;
                previous_weapon_x = held->position.x;
                previous_holder_z = actor->position.z;
                previous_weapon_z = held->position.z;
            }
        }
        rows.close();
        if (!rows) throw std::runtime_error("trace CSV write failed");
        std::cout << "pickup=" << pickup << " moving_held_x_ticks="
                  << moving_held_x_ticks << " moving_held_z_ticks="
                  << moving_held_z_ticks << " ticks=24\n";
        return pickup && moving_held_x_ticks >= 3 &&
                       moving_held_z_ticks >= 3 ? 0 : 7;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 4;
    }
}
