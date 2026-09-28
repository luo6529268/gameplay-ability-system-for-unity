#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>

namespace {

const ntsd28::RenderSprite28* find_sprite(
    const ntsd28::RenderSnapshot28& snapshot, std::size_t slot) {
    for (const auto& sprite : snapshot.sprites) {
        if (sprite.slot == slot) return &sprite;
    }
    return nullptr;
}

int run_case(const std::filesystem::path& decoded_root,
             const std::filesystem::path& vfs_root,
             const std::filesystem::path& output,
             int karin_x,
             int initial_action,
             bool source_facing,
             std::size_t karin_slot) {
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to overwrite existing output: " << output << '\n';
        return 3;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 2833;
    config.background_id = 23;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 karin;
    karin.slot = karin_slot;
    karin.object_id = 77;
    karin.x = karin_x;
    karin.y = 0;
    karin.z = 650;
    karin.hp = 500;
    karin.mp = 500;
    karin.team = 1;
    karin.action = initial_action;
    karin.facing = source_facing;

    ntsd28_playable::CombatantConfig28 opponent;
    opponent.slot = 1;
    opponent.object_id = 11;
    opponent.x = 1200;
    opponent.y = 0;
    opponent.z = 650;
    opponent.hp = 500;
    opponent.mp = 500;
    opponent.team = 2;
    opponent.action = 0;
    config.combatants = {karin, opponent};

    ntsd28_playable::GameSession28 session(decoded_root, vfs_root);
    std::string error;
    if (!session.initialize(config, error)) {
        std::cerr << "paired playable Session initialization failed: " << error
                  << '\n';
        return 4;
    }

    std::filesystem::create_directories(output.parent_path());
    std::ofstream rows(output, std::ios::binary);
    if (!rows) return 5;
    rows << "case_x\tinitial_action\tsource_facing\tkarin_slot\tcall\tworld_tick\tmode\tselected_etc_mode\tcamera_x"
            "\tkarin_action\tkarin_x\tkarin_y\tkarin_z"
            "\tchild_slot\tchild_oid\tchild_action\tchild_state"
            "\tchild_owner\tchild_x\tchild_y\tchild_z\tchild_physical_facing"
            "\tsprite_found\tsprite_pic\tsprite_facing"
            "\tsprite_left\tsprite_top\tsprite_width\n";

    bool saw_child_state9997 = false;
    bool saw_child_sprite = false;
    const int max_calls = initial_action == 415 ? 12 : 5;
    for (int call = 0; call <= max_calls; ++call) {
        if (call > 0) session.step();
        const auto* world = session.world();
        if (world == nullptr) return 6;
        const auto* actor = world->entity(karin_slot);
        if (actor == nullptr) return 7;
        const ntsd28::EntityState28* child = nullptr;
        for (std::size_t slot = 2;
             slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            const auto* entity = world->entity(slot);
            if (entity != nullptr && entity->object_id == 314) {
                child = entity;
                break;
            }
        }
        const auto snapshot = session.snapshot(false, 1333, 730);
        const auto* sprite = child == nullptr
                                 ? nullptr
                                 : find_sprite(snapshot, child->slot);
        const auto* frame = child == nullptr || child->definition == nullptr
                                ? nullptr
                                : child->definition->frame(child->frame.action);
        const int state = frame == nullptr
                              ? -1
                              : frame->values.integer("state").value_or(0);
        saw_child_state9997 |= child != nullptr && state == 9997;
        saw_child_sprite |= sprite != nullptr;

        rows << karin_x << '\t' << initial_action << '\t'
             << source_facing << '\t' << karin_slot << '\t' << call << '\t'
             << world->sequence() << '\t'
             << session.config().battle_mode << '\t'
             << session.config().selected_mode_etc_mode << '\t'
             << session.camera_x() << '\t' << actor->frame.action << '\t'
             << actor->position.x << '\t' << actor->position.y << '\t'
             << actor->position.z << '\t'
             << (child == nullptr ? -1 : static_cast<int>(child->slot)) << '\t'
             << (child == nullptr ? -1 : child->object_id) << '\t'
             << (child == nullptr ? -1 : child->frame.action) << '\t'
             << state << '\t'
             << (child == nullptr ? -1 : child->owner_slot) << '\t'
             << (child == nullptr ? 0 : child->position.x) << '\t'
             << (child == nullptr ? 0 : child->position.y) << '\t'
             << (child == nullptr ? 0 : child->position.z) << '\t'
             << (child != nullptr && child->frame.facing) << '\t'
             << (sprite != nullptr) << '\t'
             << (sprite == nullptr ? -1 : sprite->pic) << '\t'
             << (sprite != nullptr && sprite->facing) << '\t'
             << (sprite == nullptr ? 0 : sprite->screen_left) << '\t'
             << (sprite == nullptr ? 0 : sprite->screen_top) << '\t'
             << (sprite == nullptr ? 0 : sprite->frame.width) << '\n';
    }
    rows.close();
    if (!rows) return 8;
    if (!saw_child_state9997 || !saw_child_sprite) {
        std::cerr << "state9997 child or sprite not observed; rows retained\n";
        return 9;
    }
    return 0;
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 7 && argc != 8) {
        std::cerr << "usage: karin_state9997_session_trace "
                     "<decoded_dat> <complete_vfs> <output_tsv> "
                     "<20|500> <415|417> <right|left> [0|9]\n";
        return 2;
    }
    const std::string x(argv[4]);
    const std::string action(argv[5]);
    const std::string facing(argv[6]);
    const std::string slot = argc == 8 ? argv[7] : "0";
    if ((x != "20" && x != "500") ||
        (action != "415" && action != "417") ||
        (facing != "right" && facing != "left") ||
        (slot != "0" && slot != "9")) return 2;
    return run_case(argv[1], argv[2], argv[3], std::stoi(x),
                    std::stoi(action), facing == "left",
                    static_cast<std::size_t>(std::stoi(slot)));
}
