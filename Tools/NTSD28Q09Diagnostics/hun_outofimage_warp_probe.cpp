#include "ntsd28_playable/d3d11_renderer.h"
#include "ntsd28_playable/game_session.h"

#include <algorithm>
#include <filesystem>
#include <iostream>
#include <string>

#include <windows.h>

int wmain(int argc, wchar_t** argv) {
    if (argc != 5) {
        std::cerr << "usage: hun_outofimage_warp_probe <decoded_dat_root> "
                     "<complete_vfs_root> <full.png> <without_body.png>\n";
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
        ntsd28_playable::CombatantConfig28 hunter;
        hunter.slot = 0;
        hunter.object_id = 32;
        hunter.x = 500;
        hunter.y = 0;
        hunter.z = 650;
        hunter.hp = 500;
        hunter.mp = 500;
        hunter.team = 1;
        hunter.action = 95;
        hunter.facing = true;
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
        config.combatants = {hunter, opponent};

        ntsd28_playable::GameSession28 session(argv[1], argv[2]);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "session initialize failed: " << error << '\n';
            result = 5;
        } else {
            const auto* world = session.world();
            const auto* entity = world == nullptr ? nullptr : world->entity(0);
            if (entity == nullptr || entity->object_id != 32 ||
                entity->frame.action != 95) {
                std::cerr << "selected action was not retained at snapshot time\n";
                result = 6;
            } else {
                const auto snapshot = session.snapshot(false, 1333, 730);
                const ntsd28::RenderSprite28* selected_sprite = nullptr;
                int sprite_count = 0;
                for (const auto& sprite : snapshot.sprites) {
                    if (sprite.slot == 0) {
                        ++sprite_count;
                        selected_sprite = &sprite;
                    }
                }
                int body_count = 0;
                for (const auto& command : snapshot.entity_commands) {
                    if (command.slot == 0 &&
                        command.kind ==
                            ntsd28::RenderEntityCommandKind28::sprite) {
                        ++body_count;
                    }
                }
                if (sprite_count != 1 || body_count != 1 ||
                    selected_sprite == nullptr || selected_sprite->pic != 64 ||
                    selected_sprite->frame.source_x != 320 ||
                    selected_sprite->frame.source_y != 480 ||
                    selected_sprite->frame.width != 79 ||
                    selected_sprite->frame.height != 79 ||
                    selected_sprite->frame.source_path.filename() != L"hun.png") {
                    std::cerr << "selected hun sprite invariant failed: sprites="
                              << sprite_count << " commands=" << body_count
                              << '\n';
                    result = 7;
                } else {
                    auto without_body = snapshot;
                    auto& commands = without_body.entity_commands;
                    commands.erase(
                        std::remove_if(commands.begin(), commands.end(),
                                       [](const auto& command) {
                                           return command.slot == 0 &&
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
                    } else if (!renderer.render(without_body, false, error) ||
                               !renderer.save_offscreen_png(without_path, error)) {
                        std::cerr << "filtered render failed: " << error
                                  << '\n';
                        result = 10;
                    } else {
                        std::cout << "tick=" << world->sequence()
                                  << " oid=32 slot=0 action=95 pic=64 source="
                                  << selected_sprite->frame.source_path.string()
                                  << " sourceRect=320,480,79,79 screen="
                                  << selected_sprite->screen_left << ','
                                  << selected_sprite->screen_top
                                  << " facing=" << selected_sprite->facing
                                  << " viewport=1333x730\n";
                    }
                }
            }
        }
    }
    CoUninitialize();
    return result;
}
