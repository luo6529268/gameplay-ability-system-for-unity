#include "ntsd28_playable/d3d11_renderer.h"
#include "ntsd28_playable/game_session.h"

#include <algorithm>
#include <filesystem>
#include <iostream>
#include <string>

#include <windows.h>

int wmain(int argc, wchar_t** argv) {
    if (argc != 5) {
        std::cerr << "usage: hidan_natural_frame430_warp_probe <runtime_root> "
                     "<complete_vfs_root> <body_on.png> <body_off.png>\n";
        return 2;
    }
    const std::filesystem::path body_on(argv[3]);
    const std::filesystem::path body_off(argv[4]);
    if (body_on == body_off || std::filesystem::exists(body_on) ||
        std::filesystem::exists(body_off)) {
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
        config.random_seed = 0x28A55A5Au;
        config.character_id = 24;
        config.enemy_id = 24;
        config.background_id = 23;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;

        ntsd28_playable::CombatantConfig28 actor;
        actor.slot = 0;
        actor.object_id = 24;
        actor.x = 500;
        actor.z = 350;
        actor.hp = 500;
        actor.mp = 300;
        actor.team = 1;
        actor.action = 0;
        ntsd28_playable::CombatantConfig28 target;
        target.slot = 1;
        target.object_id = 24;
        target.x = 1200;
        target.z = 350;
        target.hp = 500;
        target.mp = 300;
        target.team = 2;
        target.action = 0;
        config.combatants = {actor, target};

        ntsd28_playable::GameSession28 session(argv[1], argv[2]);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "session initialize failed: " << error << '\n';
            result = 5;
        } else {
            for (int tick = 1; tick <= 16; ++tick) {
                ntsd28::InputButtons28 buttons;
                if (tick <= 2) buttons.set(ntsd28::InputKey28::attack);
                if (tick == 9 || tick == 10)
                    buttons.set(ntsd28::InputKey28::jump);
                if (tick == 13 || tick == 14) {
                    buttons.set(ntsd28::InputKey28::defend);
                    buttons.set(ntsd28::InputKey28::right);
                    buttons.set(ntsd28::InputKey28::attack);
                }
                session.set_input(0, buttons);
                session.set_input(1, {});
                session.step();
            }

            const auto* world = session.world();
            const auto* entity = world == nullptr ? nullptr : world->entity(0);
            if (world == nullptr || world->sequence() != 16 ||
                entity == nullptr || entity->object_id != 24 ||
                entity->frame.action != 430 || entity->current_mp != 150) {
                std::cerr << "ordinary-input natural frame430 invariant failed\n";
                result = 6;
            } else {
                const auto snapshot = session.snapshot(false, 1333, 730);
                const ntsd28::RenderSprite28* actor_sprite = nullptr;
                int actor_sprites = 0;
                for (const auto& sprite : snapshot.sprites) {
                    if (sprite.slot != 0) continue;
                    ++actor_sprites;
                    actor_sprite = &sprite;
                }
                int actor_commands = 0;
                for (const auto& command : snapshot.entity_commands) {
                    if (command.slot == 0 &&
                        command.kind == ntsd28::RenderEntityCommandKind28::sprite)
                        ++actor_commands;
                }
                if (actor_sprites != 1 || actor_commands != 1 ||
                    actor_sprite == nullptr || actor_sprite->pic != 119 ||
                    actor_sprite->frame.sheet_first_pic != 117 ||
                    actor_sprite->frame.sheet_last_pic != 128 ||
                    actor_sprite->frame.source_path.filename() != L"hid6.png" ||
                    actor_sprite->frame.source_x != 722 ||
                    actor_sprite->frame.source_y != 0 ||
                    actor_sprite->frame.width != 360 ||
                    actor_sprite->frame.height != 289) {
                    std::cerr << "frame430 sprite/source-sheet invariant failed: sprites="
                              << actor_sprites << " commands=" << actor_commands << '\n';
                    result = 7;
                } else {
                    auto without_actor = snapshot;
                    auto& commands = without_actor.entity_commands;
                    commands.erase(std::remove_if(commands.begin(), commands.end(),
                        [](const auto& command) {
                            return command.slot == 0 &&
                                command.kind == ntsd28::RenderEntityCommandKind28::sprite;
                        }), commands.end());
                    ntsd28_playable::D3D11Renderer28 renderer;
                    if (!renderer.initialize_offscreen(1333, 730, error)) {
                        std::cerr << "offscreen initialize failed: " << error << '\n';
                        result = 8;
                    } else if (!renderer.render(snapshot, false, error) ||
                               !renderer.save_offscreen_png(body_on, error)) {
                        std::cerr << "body-on render failed: " << error << '\n';
                        result = 9;
                    } else if (!renderer.render(without_actor, false, error) ||
                               !renderer.save_offscreen_png(body_off, error)) {
                        std::cerr << "body-off render failed: " << error << '\n';
                        result = 10;
                    } else {
                        std::cout << "tick=16 oid=24 slot=0 action=430 pic=119"
                                  << " sheet=117-128 file=hid6.png"
                                  << " sourceRect=722,0,360,289 screen="
                                  << actor_sprite->screen_left << ','
                                  << actor_sprite->screen_top
                                  << " facing=" << actor_sprite->facing
                                  << " viewport=1333x730\n";
                    }
                }
            }
        }
    }
    CoUninitialize();
    return result;
}
