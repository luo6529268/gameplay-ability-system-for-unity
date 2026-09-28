#include "ntsd28_playable/game_session.h"

#include <algorithm>
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

bool run_case(const std::filesystem::path& runtime_root,
              const std::filesystem::path& output,
              int host_x,
              int host_y) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 2833;
    config.background_id = 23;
    config.battle_mode = 0;
    config.camera_locked = true;

    ntsd28_playable::CombatantConfig28 host;
    host.slot = 0;
    host.object_id = 7;
    host.team = 1;
    host.x = host_x;
    host.y = host_y;
    host.z = 600;
    host.hp = 1;
    host.mp = 77;
    host.action = 230;
    host.facing = true;

    ntsd28_playable::CombatantConfig28 controller = host;
    controller.slot = 1;
    controller.x = 700;
    controller.hp = 500;
    controller.mp = 500;
    controller.action = 0;
    controller.facing = false;
    config.combatants = {host, controller};

    ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
    std::string error;
    if (!session.initialize(config, error)) {
        std::cerr << "Session initialization failed: " << error << '\n';
        return false;
    }
    auto* world = session.world();
    auto* dying = world == nullptr ? nullptr : world->entity(0);
    if (dying == nullptr || world->entity(1) == nullptr) return false;
    dying->current_hp = 0;
    dying->effective_max_hp = 500;
    dying->base_max_hp = 500;
    dying->revive_lives_30c = 1;
    dying->revive_next_lives_310 = 0;
    dying->revive_next_hp_314 = 400;
    dying->render_phase_008 = 2;
    dying->ai_target_slot_360 = 1;
    dying->frame.action = 230;
    dying->frame.frame_counter = 0;
    dying->motion_hold_timer = 3;

    std::ofstream rows(output, std::ios::binary);
    if (!rows) return false;
    rows << "host_x\thost_y\tcall\tsequence\tcontinuation\tpresentation_spawned"
            "\tdepth_before_culls\tdepth_after_culls\tsettlement_culls"
            "\tselected_etc_mode\tcamera_x\teffect_slot\teffect_oid"
            "\teffect_action\teffect_state\teffect_owner\teffect_x"
            "\teffect_y\teffect_z\teffect_physical_facing\tsprite_found"
            "\tsprite_facing\tsprite_left\tsprite_top\tsprite_width"
            "\tsprite_center_x\tsprite_shake_x\tfallback_expected_left"
            "\tfallback_match\n";

    bool matched = false;
    for (int call = 0; call <= 2; ++call) {
        if (call > 0) session.step();
        world = session.world();
        if (world == nullptr) return false;

        bool continuation = false;
        bool presentation_spawned = false;
        const auto* tick = session.last_tick();
        if (tick != nullptr) {
            for (const auto& event : tick->revivals.events) {
                if (event.slot != 0 ||
                    event.status != ntsd28::WorldRevivalStatus28::continuation_armed)
                    continue;
                continuation = true;
                presentation_spawned = event.presentation_spawned;
            }
        }

        const ntsd28::EntityState28* effect = nullptr;
        for (std::size_t slot = 2;
             slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            const auto* entity = world->entity(slot);
            if (entity != nullptr && entity->object_id == 998) {
                effect = entity;
                break;
            }
        }
        const auto snapshot = session.snapshot(false, 1333, 730);
        const auto* sprite = effect == nullptr
                                 ? nullptr
                                 : find_sprite(snapshot, effect->slot);
        const auto* frame = effect == nullptr || effect->definition == nullptr
                                ? nullptr
                                : effect->definition->frame(effect->frame.action);
        const int state = frame == nullptr
                              ? -1
                              : frame->values.integer("state").value_or(0);
        const int camera_x = session.camera_x();
        int expected_left = 0;
        bool fallback_match = false;
        if (effect != nullptr && sprite != nullptr) {
            const int raw_left = effect->frame.facing
                                     ? effect->position.x + sprite->center_x -
                                           sprite->frame.width +
                                           sprite->native_body_shake_x
                                     : effect->position.x - sprite->center_x +
                                           sprite->native_body_shake_x;
            const int max_left = std::max(0, 1333 - 1 - sprite->frame.width);
            expected_left = camera_x +
                            std::clamp(raw_left - camera_x, 0, max_left);
            fallback_match = sprite->screen_left == expected_left &&
                             sprite->facing == effect->frame.facing;
        }

        rows << host_x << '\t' << host_y << '\t' << call << '\t'
             << world->sequence() << '\t' << continuation << '\t'
             << presentation_spawned << '\t'
             << (tick == nullptr ? 0 : tick->stage_depth_before_geometry.grounded_object_offstage_culls) << '\t'
             << (tick == nullptr ? 0 : tick->stage_depth_after_hits.grounded_object_offstage_culls) << '\t'
             << (tick == nullptr ? 0 : tick->stage_settlement.grounded_object_offstage_culls) << '\t'
             << session.config().selected_mode_etc_mode << '\t' << camera_x
             << '\t' << (effect == nullptr ? -1 : static_cast<int>(effect->slot))
             << '\t' << (effect == nullptr ? -1 : effect->object_id)
             << '\t' << (effect == nullptr ? -1 : effect->frame.action)
             << '\t' << state
             << '\t' << (effect == nullptr ? -2 : effect->owner_slot)
             << '\t' << (effect == nullptr ? 0 : effect->position.x)
             << '\t' << (effect == nullptr ? 0 : effect->position.y)
             << '\t' << (effect == nullptr ? 0 : effect->position.z)
             << '\t' << (effect != nullptr && effect->frame.facing)
             << '\t' << (sprite != nullptr)
             << '\t' << (sprite != nullptr && sprite->facing)
             << '\t' << (sprite == nullptr ? 0 : sprite->screen_left)
             << '\t' << (sprite == nullptr ? 0 : sprite->screen_top)
             << '\t' << (sprite == nullptr ? 0 : sprite->frame.width)
             << '\t' << (sprite == nullptr ? 0 : sprite->center_x)
             << '\t' << (sprite == nullptr ? 0 : sprite->native_body_shake_x)
             << '\t' << expected_left << '\t' << fallback_match << '\n';

        if (continuation && presentation_spawned && effect != nullptr &&
            effect->frame.action == 6 && state == 9997 &&
            effect->owner_slot == -1 && sprite != nullptr && fallback_match)
            matched = true;
    }
    rows.close();
    return matched && static_cast<bool>(rows);
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: etc998_continuation_session_trace "
                     "<formal_runtime_root> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path root(argv[1]), output(argv[2]);
    const auto near = output / "left.tsv";
    const auto far = output / "right.tsv";
    if (std::filesystem::exists(near) || std::filesystem::exists(far)) {
        std::cerr << "refusing to overwrite an existing trace\n";
        return 3;
    }
    try {
        std::filesystem::create_directories(output);
        const bool left_ok = run_case(root, near, 50, -20);
        const bool right_ok = run_case(root, far, 1329, 0);
        std::cout << "left=" << left_ok << " right=" << right_ok << '\n';
        return left_ok && right_ok ? 0 : 4;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 5;
    }
}
