#include "ntsd28_playable/game_session_lfr.h"

#include <algorithm>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

struct Case {
    int defend_ticks;
    int up_ticks;
    int jump_ticks;
    bool hold_defend;
};

struct Result {
    bool action_110 = false;
    bool action_311 = false;
    bool stereo_event = false;
    int action_tick = -1;
    int event_tick = -1;
};

bool run_case(const std::filesystem::path& decoded_dat,
              const std::filesystem::path& vfs,
              const Case& selected,
              const std::filesystem::path& output,
              bool record,
              Result& result,
              std::string& error) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 507;
    config.enemy_id = 7;
    config.background_id = 23;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 kim;
    kim.slot = 0;
    kim.object_id = 507;
    kim.x = 500;
    kim.z = 481;
    kim.hp = 500;
    kim.mp = 500;
    kim.team = 1;
    kim.action = 0;
    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 1;
    lee.object_id = 7;
    lee.x = 1200;
    lee.z = 481;
    lee.hp = 500;
    lee.mp = 500;
    lee.team = 2;
    lee.action = 0;
    config.combatants = {kim, lee};

    ntsd28_playable::GameSession28 session(decoded_dat, vfs);
    if (!session.initialize(config, error)) return false;
    ntsd28_playable::GameSessionLfr28 recorder;
    if (record && !recorder.begin(session, error)) return false;

    std::ofstream rows;
    if (record) {
        rows.open(output / "source-events.csv", std::ios::binary);
        if (!rows) {
            error = "could not open source event CSV";
            return false;
        }
        rows << "tick,input,action,mp,camera_x,event_index,event_source,event_world_x,event_path\n";
    }

    for (int tick = 1; tick <= 55; ++tick) {
        ntsd28::InputButtons28 input;
        std::string input_name = "neutral";
        const int up_end = selected.defend_ticks + selected.up_ticks;
        const int jump_end = up_end + selected.jump_ticks;
        if (tick <= selected.defend_ticks) {
            input.set(ntsd28::InputKey28::defend);
            input_name = "defend";
        } else if (tick <= up_end) {
            input.set(ntsd28::InputKey28::depth_up);
            if (selected.hold_defend) input.set(ntsd28::InputKey28::defend);
            input_name = selected.hold_defend ? "defend+up" : "up";
        } else if (tick <= jump_end) {
            input.set(ntsd28::InputKey28::jump);
            if (selected.hold_defend) input.set(ntsd28::InputKey28::defend);
            input_name = selected.hold_defend ? "defend+jump" : "jump";
        }
        session.set_input(0, input);
        session.set_input(1, {});
        session.step();
        if (record && !recorder.capture_after_step(session, error)) return false;

        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        const auto* last_tick = session.last_tick();
        if (actor == nullptr || last_tick == nullptr) {
            error = "Kimimaro or completed tick disappeared";
            return false;
        }
        const int action = actor->frame.action;
        result.action_110 |= action == 110;
        if (action == 311) {
            result.action_311 = true;
            if (result.action_tick < 0) result.action_tick = tick;
        }
        if (last_tick->audio_events.empty() && record) {
            rows << tick << ',' << input_name << ',' << action << ','
                 << actor->current_mp << ',' << session.camera_x()
                 << ",-1,-1,-1,-\n";
        }
        for (std::size_t index = 0; index < last_tick->audio_events.size(); ++index) {
            const auto& event = last_tick->audio_events[index];
            std::string path = event.resource_path;
            std::replace(path.begin(), path.end(), '\\', '/');
            if (path == "c/kim/w/j1.wav") {
                result.stereo_event = true;
                if (result.event_tick < 0) result.event_tick = tick;
            }
            if (record) {
                rows << tick << ',' << input_name << ',' << action << ','
                     << actor->current_mp << ',' << session.camera_x() << ','
                     << index << ',' << static_cast<int>(event.source) << ','
                     << event.world_x << ',' << path << '\n';
            }
        }
    }

    if (!record) return true;
    rows.close();
    if (!rows) {
        error = "could not finish source event CSV";
        return false;
    }
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) return false;
    std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
    if (!lfr) {
        error = "could not open LFR";
        return false;
    }
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) {
        error = "could not finish LFR";
        return false;
    }
    return true;
}

}  // namespace

int main(int argc, char** argv) {
    if (argc != 4) {
        std::cerr << "usage: kim_j1_natural_reach_probe <decoded_dat> <vfs> <new_output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to overwrite existing output directory\n";
        return 3;
    }
    std::filesystem::create_directories(output);
    std::ofstream summary(output / "timing-grid.csv", std::ios::binary);
    if (!summary) return 4;
    summary << "defend_ticks,up_ticks,jump_ticks,hold_defend,action_110,action_311,stereo_event,action_tick,event_tick\n";

    Case winner{};
    bool selected_found = false;
    for (int defend_ticks : {2, 1, 3}) {
        for (int up_ticks : {2, 1, 3}) {
            for (int jump_ticks : {2, 1, 3}) {
                for (bool hold_defend : {false, true}) {
                    const Case selected{defend_ticks, up_ticks, jump_ticks,
                                        hold_defend};
                    Result result;
                    std::string error;
                    if (!run_case(argv[1], argv[2], selected, output, false,
                                  result, error)) {
                        std::cerr << "source case failed: " << error << '\n';
                        return 5;
                    }
                    summary << defend_ticks << ',' << up_ticks << ','
                            << jump_ticks << ',' << hold_defend << ','
                            << result.action_110 << ',' << result.action_311
                            << ',' << result.stereo_event << ','
                            << result.action_tick << ',' << result.event_tick
                            << '\n';
                    if (result.action_110 && result.action_311 &&
                        result.stereo_event) {
                        winner = selected;
                        selected_found = true;
                        break;
                    }
                }
                if (selected_found) break;
            }
            if (selected_found) break;
        }
        if (selected_found) break;
    }
    summary.close();
    if (!summary) return 6;

    if (!selected_found) winner = {2, 2, 2, false};
    Result recorded;
    std::string error;
    if (!run_case(argv[1], argv[2], winner, output, true,
                  recorded, error)) {
        std::cerr << "recorded source case failed: " << error << '\n';
        return 7;
    }
    if (selected_found !=
        (recorded.action_110 && recorded.action_311 && recorded.stereo_event)) {
        std::cerr << "recorded result differs from bounded search\n";
        return 8;
    }
    std::cout << "selected defend=" << winner.defend_ticks
              << " up=" << winner.up_ticks
              << " jump=" << winner.jump_ticks
              << " hold_defend=" << winner.hold_defend
              << " action_tick=" << recorded.action_tick
              << " event_tick=" << recorded.event_tick << '\n';
    return selected_found ? 0 : 10;
}
