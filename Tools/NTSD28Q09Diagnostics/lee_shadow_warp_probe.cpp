#include "ntsd28_playable/d3d11_renderer.h"
#include "ntsd28_playable/game_session.h"

#include <algorithm>
#include <filesystem>
#include <iostream>
#include <string>
#include <vector>

#include <windows.h>

int wmain(int argc, wchar_t** argv) {
    if (argc != 5) {
        std::cerr << "usage: lee_shadow_warp_probe <runtime_root> "
                     "<complete_vfs_root> <full.png> <without_children.png>\n";
        return 2;
    }
    const std::filesystem::path fullPath(argv[3]);
    const std::filesystem::path withoutPath(argv[4]);
    if (fullPath == withoutPath || std::filesystem::exists(fullPath) ||
        std::filesystem::exists(withoutPath)) {
        std::cerr << "refusing to overwrite diagnostic PNG\n";
        return 3;
    }
    const HRESULT comResult = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    if (FAILED(comResult)) {
        std::cerr << "COM initialization failed\n";
        return 4;
    }

    int result = 0;
    {
        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 682973786u;
        config.character_id = 7;
        config.enemy_id = 2;
        config.background_id = 23;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;
        ntsd28_playable::CombatantConfig28 lee;
        lee.slot = 0;
        lee.object_id = 7;
        lee.x = 500;
        lee.z = 650;
        lee.hp = 500;
        lee.mp = 500;
        lee.team = 1;
        lee.action = 0;
        ntsd28_playable::CombatantConfig28 opponent;
        opponent.slot = 1;
        opponent.object_id = 2;
        opponent.x = 1200;
        opponent.z = 650;
        opponent.hp = 500;
        opponent.mp = 500;
        opponent.team = 2;
        opponent.action = 0;
        config.combatants = {lee, opponent};

        ntsd28_playable::GameSession28 session(argv[1], argv[2]);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "session initialize failed: " << error << '\n';
            result = 5;
        } else {
            for (int tick = 1; tick <= 6; ++tick) {
                ntsd28::InputButtons28 buttons;
                if (tick == 2) buttons.set(ntsd28::InputKey28::attack);
                if (tick == 3 || tick == 4)
                    buttons.set(ntsd28::InputKey28::defend);
                session.set_input(0, buttons);
                session.set_input(1, {});
                session.step();
            }
            const auto* world = session.world();
            if (world == nullptr) {
                std::cerr << "world disappeared before selected tick\n";
                result = 6;
            } else {
                const auto snapshot = session.snapshot(false);
                std::vector<std::size_t> childSlots;
                for (std::size_t slot = 0;
                     slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
                    const auto* entity = world->entity(slot);
                    if (entity != nullptr && entity->object_id == 204 &&
                        entity->owner_slot == 0) {
                        childSlots.push_back(slot);
                    }
                }
                int childBodies = 0;
                int childShadows = 0;
                int ordinaryShadows = 0;
                for (const auto& command : snapshot.entity_commands) {
                    const bool child = std::find(
                        childSlots.begin(), childSlots.end(), command.slot) !=
                        childSlots.end();
                    if (child && command.kind ==
                                     ntsd28::RenderEntityCommandKind28::sprite)
                        ++childBodies;
                    if (child && command.kind ==
                                     ntsd28::RenderEntityCommandKind28::shadow)
                        ++childShadows;
                    if (!child && command.kind ==
                                      ntsd28::RenderEntityCommandKind28::shadow)
                        ++ordinaryShadows;
                }
                if (childSlots.size() != 5 || childBodies != 5 ||
                    childShadows != 0 || ordinaryShadows == 0) {
                    std::cerr << "selected snapshot invariant failed: children="
                              << childSlots.size() << " bodies=" << childBodies
                              << " shadows=" << childShadows
                              << " ordinary=" << ordinaryShadows << '\n';
                    result = 7;
                } else {
                    auto withoutChildren = snapshot;
                    auto& commands = withoutChildren.entity_commands;
                    commands.erase(std::remove_if(
                                       commands.begin(), commands.end(),
                                       [&childSlots](const auto& command) {
                                           return command.kind ==
                                                      ntsd28::RenderEntityCommandKind28::
                                                          sprite &&
                                                  std::find(
                                                      childSlots.begin(),
                                                      childSlots.end(),
                                                      command.slot) !=
                                                      childSlots.end();
                                       }),
                                   commands.end());
                    ntsd28_playable::D3D11Renderer28 renderer;
                    if (!renderer.initialize_offscreen(1333, 730, error)) {
                        std::cerr << "offscreen initialization failed: "
                                  << error << '\n';
                        result = 8;
                    } else if (!renderer.render(snapshot, false, error) ||
                               !renderer.save_offscreen_png(fullPath, error)) {
                        std::cerr << "full render failed: " << error << '\n';
                        result = 9;
                    } else if (!renderer.render(withoutChildren, false, error) ||
                               !renderer.save_offscreen_png(withoutPath, error)) {
                        std::cerr << "filtered render failed: " << error << '\n';
                        result = 10;
                    } else {
                        std::cout << "tick=6 children=5 body=5 childShadow=0 "
                                     "ordinaryShadow="
                                  << ordinaryShadows << " camera="
                                  << snapshot.camera.x << ',' << snapshot.camera.y
                                  << " viewport=1333x730\n";
                    }
                }
            }
        }
    }
    CoUninitialize();
    return result;
}
