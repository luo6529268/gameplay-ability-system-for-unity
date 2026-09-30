#include "ntsd28_playable/d3d11_renderer.h"
#include "ntsd28_playable/game_session_lfr.h"
#include "ntsd28/render_snapshot.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <iterator>
#include <string>
#include <vector>

#include <windows.h>

int wmain(int argc, wchar_t** argv) {
    if (argc != 6) {
        std::cerr << "usage: ita_equal_hp_lfr_playback_warp_probe "
                     "<runtime_root> <complete_vfs_root> <source.lfr> "
                     "<mark_on.png> <mark_off.png>\n";
        return 2;
    }

    const std::filesystem::path lfr_path(argv[3]);
    const std::filesystem::path on_path(argv[4]);
    const std::filesystem::path off_path(argv[5]);
    if (on_path == off_path || std::filesystem::exists(on_path) ||
        std::filesystem::exists(off_path)) {
        std::cerr << "refusing to overwrite diagnostic PNG\n";
        return 3;
    }
    std::ifstream input(lfr_path, std::ios::binary);
    if (!input) {
        std::cerr << "cannot read LFR\n";
        return 4;
    }
    const std::vector<std::uint8_t> encoded{
        std::istreambuf_iterator<char>(input), std::istreambuf_iterator<char>()};
    if (encoded.empty() || input.bad()) {
        std::cerr << "LFR read failed\n";
        return 4;
    }

    const HRESULT com_result = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    if (FAILED(com_result)) {
        std::cerr << "COM initialization failed\n";
        return 5;
    }
    int result = 0;
    {
        std::string error;
        ntsd28_playable::GameSessionLfrPlayback28 playback;
        ntsd28_playable::GameSession28 session(argv[1], argv[2]);
        if (!playback.load(encoded, ntsd28_playable::LfrTuMode28::two_tu,
                           error) ||
            playback.declared_end_tick() != 22 ||
            !playback.override_initial_action(0, 0, error) ||
            !playback.override_initial_action(1, 0, error) ||
            !playback.override_initial_facing(0, 0, error) ||
            !playback.override_initial_facing(1, 1, error) ||
            !playback.override_initial_mp(0, 500, error) ||
            !playback.override_initial_mp(1, 500, error) ||
            !playback.initialize_session(session, error)) {
            std::cerr << "LFR playback initialization failed: " << error << '\n';
            result = 6;
        } else {
            ntsd28::RenderSnapshot28 selected_snapshot;
            int selected_action = -1;
            int selected_hp = -1;
            int selected_base_hp = -1;
            for (int tick = 1; tick <= 23; ++tick) {
                if (!playback.step(session, error)) {
                    std::cerr << "LFR playback failed at tick " << tick << ": "
                              << error << '\n';
                    result = 7;
                    break;
                }
                const auto* world = session.world();
                const auto* actor = world == nullptr ? nullptr : world->entity(0);
                const auto* target = world == nullptr ? nullptr : world->entity(1);
                if (actor == nullptr || target == nullptr ||
                    actor->object_id != 2 || target->object_id != 9) {
                    std::cerr << "selected roster changed at tick " << tick << '\n';
                    result = 8;
                    break;
                }
                std::cout << "tick=" << tick
                          << " narutoAction=" << actor->frame.action
                          << " itaAction=" << target->frame.action
                          << " itaHP=" << target->current_hp
                          << " itaBaseHP=" << target->base_max_hp << '\n';
                if (tick == 22) {
                    selected_snapshot = session.snapshot(false, 1333, 730);
                    selected_action = target->frame.action;
                    selected_hp = target->current_hp;
                    selected_base_hp = target->base_max_hp;
                }
            }
            if (result == 0 &&
                (!playback.finished() ||
                 !playback.verify_final_headers(session, error))) {
                std::cerr << "LFR final headers failed: " << error << '\n';
                result = 9;
            }
            if (result == 0) {
                std::size_t selected_index =
                    selected_snapshot.entity_commands.size();
                int mark_count = 0;
                for (std::size_t index = 0;
                     index < selected_snapshot.entity_commands.size(); ++index) {
                    const auto& command = selected_snapshot.entity_commands[index];
                    if (command.kind !=
                            ntsd28::RenderEntityCommandKind28::bleed_mark ||
                        command.slot != 1) {
                        continue;
                    }
                    ++mark_count;
                    selected_index = index;
                }
                if (selected_action != 0 || selected_hp != 10 ||
                    selected_base_hp != 30 || mark_count != 1 ||
                    selected_index >= selected_snapshot.entity_commands.size() ||
                    selected_snapshot.entity_commands[selected_index].item_index >=
                        selected_snapshot.bleed_marks.size()) {
                    std::cerr << "tick-22 natural standing mark was not reached\n";
                    result = 10;
                } else {
                    const auto& mark = selected_snapshot.bleed_marks[
                        selected_snapshot.entity_commands[selected_index].item_index];
                    if (mark.slot != 1 || mark.object_id != 9 ||
                        mark.hp_threshold != 10 || mark.width != 1 ||
                        mark.height != 3 || mark.rgb != 0x00ff0000U) {
                        std::cerr << "tick-22 mark properties differ\n";
                        result = 11;
                    } else {
                        auto without_mark = selected_snapshot;
                        without_mark.entity_commands.erase(
                            without_mark.entity_commands.begin() +
                            static_cast<std::ptrdiff_t>(selected_index));
                        ntsd28_playable::D3D11Renderer28 renderer;
                        if (!renderer.initialize_offscreen(1333, 730, error)) {
                            std::cerr << "WARP initialization failed: "
                                      << error << '\n';
                            result = 12;
                        } else if (!renderer.render(selected_snapshot, false, error) ||
                                   !renderer.save_offscreen_png(on_path, error)) {
                            std::cerr << "mark-on render failed: " << error
                                      << '\n';
                            result = 13;
                        } else if (!renderer.render(without_mark, false, error) ||
                                   !renderer.save_offscreen_png(off_path, error)) {
                            std::cerr << "mark-off render failed: " << error
                                      << '\n';
                            result = 14;
                        } else {
                            std::cout << "selectedTick=22 markScreen="
                                      << mark.screen_left << ',' << mark.screen_top
                                      << " threshold=" << mark.hp_threshold
                                      << " size=" << mark.width << 'x'
                                      << mark.height << " rgb=" << mark.rgb
                                      << " viewport=1333x730 headers=PASS\n";
                        }
                    }
                }
            }
        }
    }
    CoUninitialize();
    return result;
}
