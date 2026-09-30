#include "ntsd28_playable/d3d11_renderer.h"
#include "ntsd28_playable/game_session.h"

#include <algorithm>
#include <filesystem>
#include <iostream>
#include <string>

#include <windows.h>

int wmain(int argc, wchar_t** argv) {
    if (argc != 5) {
        std::cerr << "usage: karin_alpha_warp_probe <decoded_dat_root> "
                     "<complete_vfs_root> <full.png> <without_child.png>\n";
        return 2;
    }
    const std::filesystem::path full_path(argv[3]);
    const std::filesystem::path without_path(argv[4]);
    if (full_path == without_path || std::filesystem::exists(full_path) ||
        std::filesystem::exists(without_path)) {
        std::cerr << "refusing to overwrite diagnostic PNG\n";
        return 3;
    }

    const HRESULT com_result = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    if (FAILED(com_result)) {
        std::cerr << "COM initialization failed\n";
        return 4;
    }

    int result = 0;
    {
        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 2833;
        config.background_id = 23;
        config.battle_mode = 0;
        ntsd28_playable::CombatantConfig28 karin;
        karin.slot = 0;
        karin.object_id = 77;
        karin.x = 500;
        karin.y = 0;
        karin.z = 650;
        karin.hp = 500;
        karin.mp = 500;
        karin.team = 1;
        karin.action = 415;
        karin.facing = true;
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

        ntsd28_playable::GameSession28 session(argv[1], argv[2]);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "session initialize failed: " << error << '\n';
            result = 5;
        } else {
            for (int tick = 0; tick < 3; ++tick) {
                session.set_input(0, {});
                session.set_input(1, {});
                session.step();
            }
            const auto* world = session.world();
            if (world == nullptr || world->sequence() != 3) {
                std::cerr << "world did not reach selected tick 3\n";
                result = 6;
            } else {
                const auto snapshot = session.snapshot(false, 1333, 730);
                std::size_t child_slot = ntsd28::EngineProfile28::maximum_slots;
                int child_count = 0;
                for (std::size_t slot = 0;
                     slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
                    const auto* entity = world->entity(slot);
                    if (entity != nullptr && entity->object_id == 314 &&
                        entity->owner_slot == 0) {
                        ++child_count;
                        child_slot = slot;
                        const auto* frame = entity->definition == nullptr
                                                ? nullptr
                                                : entity->definition->frame(
                                                      entity->frame.action);
                        if (frame == nullptr ||
                            frame->values.integer("state").value_or(0) !=
                                9997) {
                            std::cerr << "child state is not 9997\n";
                            result = 7;
                        }
                    }
                }
                const ntsd28::RenderSprite28* child_sprite = nullptr;
                int child_sprite_count = 0;
                for (const auto& sprite : snapshot.sprites) {
                    if (sprite.slot == child_slot) {
                        ++child_sprite_count;
                        child_sprite = &sprite;
                    }
                }
                int child_body_commands = 0;
                for (const auto& command : snapshot.entity_commands) {
                    if (command.slot == child_slot &&
                        command.kind ==
                            ntsd28::RenderEntityCommandKind28::sprite) {
                        ++child_body_commands;
                    }
                }
                if (result == 0 &&
                    (child_count != 1 || child_sprite_count != 1 ||
                     child_body_commands != 1 || child_sprite == nullptr ||
                     child_sprite->pic != 60 ||
                     child_sprite->frame.width != 79 ||
                     child_sprite->frame.height != 79 ||
                     child_sprite->frame.source_path.filename() != L"cha4.png")) {
                    std::cerr << "selected Karin child sprite invariant failed: "
                              << "children=" << child_count
                              << " sprites=" << child_sprite_count
                              << " commands=" << child_body_commands << '\n';
                    result = 7;
                }
                if (result == 0) {
                    auto without_child = snapshot;
                    auto& commands = without_child.entity_commands;
                    commands.erase(
                        std::remove_if(commands.begin(), commands.end(),
                                       [child_slot](const auto& command) {
                                           return command.slot == child_slot &&
                                                  command.kind ==
                                                      ntsd28::RenderEntityCommandKind28::sprite;
                                       }),
                        commands.end());
                    ntsd28_playable::D3D11Renderer28 renderer;
                    if (!renderer.initialize_offscreen(1333, 730, error)) {
                        std::cerr << "offscreen initialization failed: "
                                  << error << '\n';
                        result = 8;
                    } else if (!renderer.render(snapshot, false, error) ||
                               !renderer.save_offscreen_png(full_path, error)) {
                        std::cerr << "full render failed: " << error << '\n';
                        result = 9;
                    } else if (!renderer.render(without_child, false, error) ||
                               !renderer.save_offscreen_png(without_path,
                                                            error)) {
                        std::cerr << "filtered render failed: " << error
                                  << '\n';
                        result = 10;
                    } else {
                        std::cout << "tick=3 oid=314 slot=" << child_slot
                                  << " action=" << child_sprite->action
                                  << " pic=60 source="
                                  << child_sprite->frame.source_path.string()
                                  << " sourceRect="
                                  << child_sprite->frame.source_x << ','
                                  << child_sprite->frame.source_y
                                  << ",79,79 screen="
                                  << child_sprite->screen_left << ','
                                  << child_sprite->screen_top
                                  << " facing=" << child_sprite->facing
                                  << " viewport=1333x730\n";
                    }
                }
            }
        }
    }
    CoUninitialize();
    return result;
}
