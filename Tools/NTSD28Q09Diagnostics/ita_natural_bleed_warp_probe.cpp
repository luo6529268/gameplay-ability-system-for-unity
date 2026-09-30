#include "ntsd28_playable/d3d11_renderer.h"
#include "ntsd28_playable/game_session.h"

#include <algorithm>
#include <filesystem>
#include <iostream>
#include <string>

#include <windows.h>

int wmain(int argc, wchar_t** argv) {
    if (argc != 5) {
        std::cerr << "usage: ita_natural_bleed_warp_probe <runtime_root> "
                     "<complete_vfs_root> <mark_on.png> <mark_off.png>\n";
        return 2;
    }
    const std::filesystem::path on_path(argv[3]);
    const std::filesystem::path off_path(argv[4]);
    if (on_path == off_path || std::filesystem::exists(on_path) ||
        std::filesystem::exists(off_path)) {
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
        config.character_id = 2;
        config.enemy_id = 9;
        config.background_id = 23;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;

        ntsd28_playable::CombatantConfig28 naruto;
        naruto.slot = 0;
        naruto.object_id = 2;
        naruto.x = 500;
        naruto.z = 650;
        naruto.hp = 500;
        naruto.base_hp = 500;
        naruto.mp = 500;
        naruto.team = 1;
        naruto.action = 0;
        naruto.facing = false;

        ntsd28_playable::CombatantConfig28 ita;
        ita.slot = 1;
        ita.object_id = 9;
        ita.x = 540;
        ita.z = 650;
        ita.hp = 180;
        ita.base_hp = 500;
        ita.mp = 500;
        ita.team = 2;
        ita.action = 0;
        ita.facing = true;
        config.combatants = {naruto, ita};

        ntsd28_playable::GameSession28 session(argv[1], argv[2]);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "session initialize failed: " << error << '\n';
            result = 5;
        } else {
            int first_damage_tick = -1;
            int selected_tick = -1;
            ntsd28::RenderSnapshot28 selected_snapshot;
            std::size_t selected_command_index = 0;
            for (int tick = 1; tick <= 80; ++tick) {
                ntsd28::InputButtons28 buttons;
                if (tick <= 2) buttons.set(ntsd28::InputKey28::attack);
                session.set_input(0, buttons);
                session.set_input(1, {});
                session.step();

                const auto* world = session.world();
                const auto* actor = world == nullptr ? nullptr : world->entity(0);
                const auto* target = world == nullptr ? nullptr : world->entity(1);
                if (actor == nullptr || target == nullptr ||
                    actor->object_id != 2 || target->object_id != 9) {
                    std::cerr << "selected roster disappeared at tick=" << tick << '\n';
                    result = 6;
                    break;
                }
                if (first_damage_tick < 0 && target->current_hp < 180)
                    first_damage_tick = tick;
                const auto snapshot = session.snapshot(false, 1333, 730);
                int mark_count = 0;
                std::size_t mark_command_index = 0;
                for (std::size_t i = 0; i < snapshot.entity_commands.size(); ++i) {
                    const auto& command = snapshot.entity_commands[i];
                    if (command.kind ==
                            ntsd28::RenderEntityCommandKind28::bleed_mark &&
                        command.slot == 1) {
                        ++mark_count;
                        mark_command_index = i;
                    }
                }
                std::cout << "tick=" << tick
                          << " narutoAction=" << actor->frame.action
                          << " narutoX=" << actor->position.x
                          << " itaAction=" << target->frame.action
                          << " itaHp=" << target->current_hp
                          << " itaMarks=" << mark_count << '\n';
                if (first_damage_tick >= 0 && target->current_hp > 0 &&
                    target->frame.action == 0 && mark_count == 1) {
                    selected_tick = tick;
                    selected_snapshot = snapshot;
                    selected_command_index = mark_command_index;
                    break;
                }
            }

            if (result == 0 && selected_tick < 0) {
                std::cerr << "natural standing bleed mark not reached; "
                          << "firstDamageTick=" << first_damage_tick << '\n';
                result = 7;
            }
            if (result == 0) {
                const auto selected_command =
                    selected_snapshot.entity_commands[selected_command_index];
                if (selected_command.item_index >=
                    selected_snapshot.bleed_marks.size()) {
                    std::cerr << "selected bleed command has no mark data\n";
                    result = 8;
                } else {
                    const auto& mark = selected_snapshot.bleed_marks[
                        selected_command.item_index];
                    if (mark.slot != 1 || mark.object_id != 9 ||
                        mark.hp_threshold != 166 || mark.width != 1 ||
                        mark.height != 3 || mark.rgb != 0x00ff0000U) {
                        std::cerr << "selected formal bleed fields differ\n";
                        result = 8;
                    } else {
                        auto without_mark = selected_snapshot;
                        without_mark.entity_commands.erase(
                            without_mark.entity_commands.begin() +
                            static_cast<std::ptrdiff_t>(selected_command_index));
                        ntsd28_playable::D3D11Renderer28 renderer;
                        if (!renderer.initialize_offscreen(1333, 730, error)) {
                            std::cerr << "offscreen initialization failed: "
                                      << error << '\n';
                            result = 9;
                        } else if (!renderer.render(selected_snapshot, false, error) ||
                                   !renderer.save_offscreen_png(on_path, error)) {
                            std::cerr << "mark-on render failed: " << error << '\n';
                            result = 10;
                        } else if (!renderer.render(without_mark, false, error) ||
                                   !renderer.save_offscreen_png(off_path, error)) {
                            std::cerr << "mark-off render failed: " << error << '\n';
                            result = 11;
                        } else {
                            std::cout << "selectedTick=" << selected_tick
                                      << " firstDamageTick=" << first_damage_tick
                                      << " slot=1 markScreen=" << mark.screen_left
                                      << ',' << mark.screen_top
                                      << " threshold=" << mark.hp_threshold
                                      << " size=" << mark.width << 'x' << mark.height
                                      << " rgb=" << mark.rgb
                                      << " viewport=1333x730\n";
                        }
                    }
                }
            }
        }
    }
    CoUninitialize();
    return result;
}
