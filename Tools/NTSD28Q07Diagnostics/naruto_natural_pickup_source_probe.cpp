#include "ntsd28/battle_world.h"
#include "ntsd28/dat_parser.h"
#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <iterator>
#include <memory>
#include <stdexcept>
#include <string>

namespace {

std::shared_ptr<const ntsd28::DatDocument> load_definition(
    const std::filesystem::path& path) {
    std::ifstream input(path, std::ios::binary);
    if (!input) throw std::runtime_error("cannot read formal DAT");
    const std::string bytes(std::istreambuf_iterator<char>{input}, {});
    auto definition = std::make_shared<const ntsd28::DatDocument>(
        ntsd28::DatParser{}.parse_text(bytes));
    if (!definition->ok()) throw std::runtime_error("formal DAT parse failed");
    return definition;
}

struct Result {
    bool pickup = false;
    bool airborne = false;
    bool action30_pic97 = false;
    int pickup_tick = -1;
    int jump_start_tick = -1;
    int air_attack_start_tick = -1;
    int action30_tick = -1;
    int final_tick = 0;
};

Result run_case(const std::filesystem::path& runtime_root,
                const std::shared_ptr<const ntsd28::DatDocument>& weapon_definition,
                int weapon_x,
                const std::filesystem::path& output) {
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
        throw std::runtime_error("expected free native weapon slot and roster");
    }
    ntsd28::SpawnRequest28 weapon;
    weapon.object_id = 120;
    weapon.object_type = 1;
    weapon.definition = weapon_definition;
    weapon.initial_action = 64;
    weapon.position.x = weapon_x;
    weapon.position.z = 542;
    weapon.hp = 100;
    weapon.mp = 0;
    const auto spawned = world->spawn_at(50, weapon);
    if (!spawned.success) {
        throw std::runtime_error("ground OID120 spawn: " + spawned.message);
    }
    std::ofstream rows(output, std::ios::binary);
    if (!rows) throw std::runtime_error("cannot open new result CSV");
    rows << "tick,input,phase,actor_action,actor_state,actor_pic,actor_x,actor_y,actor_z,"
            "actor_mp,actor_link,actor_child_slot,weapon_present,weapon_action,"
            "weapon_state,weapon_link,weapon_parent_slot,world_objects\n";

    Result result;
    for (int tick = 1; tick <= 70; ++tick) {
        const auto* before = world->entity(0);
        if (before == nullptr) throw std::runtime_error("Naruto disappeared");
        if (result.pickup && result.jump_start_tick < 0 && tick > 2 &&
            before->frame.action == 0) {
            result.jump_start_tick = tick;
        }
        const auto* before_frame = before->definition->frame(before->frame.action);
        const int before_state = before_frame == nullptr
                                     ? -1
                                     : before_frame->values.integer("state").value_or(-1);
        if (result.jump_start_tick >= 0 &&
            result.air_attack_start_tick < 0 &&
            tick >= result.jump_start_tick + 2 && before_state == 4 &&
            before->position.y < before->collision_y_reference) {
            result.air_attack_start_tick = tick;
        }

        ntsd28::InputButtons28 buttons;
        const char* input = "none";
        if (tick <= 2 || (result.air_attack_start_tick >= 0 &&
                          tick <= result.air_attack_start_tick + 1)) {
            buttons.set(ntsd28::InputKey28::attack);
            input = "attack";
        } else if (result.jump_start_tick >= 0 &&
                   tick <= result.jump_start_tick + 1) {
            buttons.set(ntsd28::InputKey28::jump);
            input = "jump";
        }
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();

        const auto* actor = world->entity(0);
        const auto* held = world->entity(50);
        if (actor == nullptr) throw std::runtime_error("Naruto despawned");
        const auto* frame = actor->definition->frame(actor->frame.action);
        const int state = frame == nullptr
                              ? -1
                              : frame->values.integer("state").value_or(-1);
        const int pic = frame == nullptr
                            ? -1
                            : frame->values.integer("pic").value_or(-1);
        const auto* weapon_frame = held == nullptr
                                       ? nullptr
                                       : held->definition->frame(held->frame.action);
        const int weapon_state = weapon_frame == nullptr
                                     ? -1
                                     : weapon_frame->values.integer("state").value_or(-1);
        if (!result.pickup && held != nullptr &&
            actor->interaction_state % 100 == 1 &&
            actor->linked_child_slot == 50 && held->interaction_state == -1) {
            result.pickup = true;
            result.pickup_tick = tick;
        }
        if (state == 4 && actor->position.y < actor->collision_y_reference) {
            result.airborne = true;
        }
        if (!result.action30_pic97 && actor->frame.action == 30 && pic == 97 &&
            result.pickup) {
            result.action30_pic97 = true;
            result.action30_tick = tick;
        }
        rows << tick << ',' << input << ',' << world->input_update_phase_4a0b90()
             << ',' << actor->frame.action << ',' << state << ',' << pic
             << ',' << actor->position.x << ',' << actor->position.y
             << ',' << actor->position.z << ',' << actor->current_mp
             << ',' << actor->interaction_state << ',' << actor->linked_child_slot
             << ',' << (held == nullptr ? 0 : 1)
             << ',' << (held == nullptr ? -1 : held->frame.action)
             << ',' << weapon_state
             << ',' << (held == nullptr ? 0 : held->interaction_state)
             << ',' << (held == nullptr ? -1 : held->linked_parent_slot)
             << ',' << world->active_count() << '\n';
        result.final_tick = tick;
        if (result.action30_pic97 && tick >= result.action30_tick + 3) break;
    }
    rows.close();
    if (!rows) throw std::runtime_error("result CSV write failed");
    return result;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) {
        std::cerr << "usage: naruto_natural_pickup_source_probe "
                     "<runtime_root> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[2]);
    const auto near_csv = output / "near-source.csv";
    const auto far_csv = output / "far-source.csv";
    const auto summary = output / "summary.txt";
    if (std::filesystem::exists(near_csv) || std::filesystem::exists(far_csv) ||
        std::filesystem::exists(summary)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }
    try {
        std::filesystem::create_directories(output);
        const auto weapon_definition = load_definition(
            std::filesystem::path(argv[1]) / L"decoded_dat" / L"w" / L"4.dat");
        if (weapon_definition->declared_frame(64) == nullptr ||
            weapon_definition->declared_frame(64)->values.integer("state") != 1004) {
            throw std::runtime_error("formal OID120 ground frame64 gate failed");
        }
        const Result near = run_case(argv[1], weapon_definition, 190,
                                     near_csv);
        const Result far = run_case(argv[1], weapon_definition, 800,
                                    far_csv);
        std::ofstream report(summary, std::ios::binary);
        if (!report) throw std::runtime_error("cannot open summary");
        report << "near pickup=" << near.pickup
               << " pickup_tick=" << near.pickup_tick
               << " jump_start_tick=" << near.jump_start_tick
               << " airborne=" << near.airborne
               << " air_attack_start_tick=" << near.air_attack_start_tick
               << " action30_pic97=" << near.action30_pic97
               << " action30_tick=" << near.action30_tick
               << " final_tick=" << near.final_tick << '\n';
        report << "far pickup=" << far.pickup
               << " pickup_tick=" << far.pickup_tick
               << " action30_pic97=" << far.action30_pic97
               << " final_tick=" << far.final_tick << '\n';
        report.close();
        if (!report) throw std::runtime_error("summary write failed");
        std::cout << "near pickup=" << near.pickup
                  << " action30_pic97=" << near.action30_pic97
                  << " far pickup=" << far.pickup << '\n';
        return near.pickup && near.airborne && near.action30_pic97 && !far.pickup
                   ? 0
                   : 7;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 4;
    }
}
