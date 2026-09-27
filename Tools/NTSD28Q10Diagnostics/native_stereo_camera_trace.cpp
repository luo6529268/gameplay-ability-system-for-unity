#include "ntsd28_playable/game_session.h"

#include <algorithm>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>

namespace {

struct StereoMix {
    int left = 0;
    int right = 0;
};

// Diagnostic projection of audio_backend.cpp::native_stereo_mix28 with the
// locked_local_battle_stereo_layout28 333/666/-333 constants.
StereoMix project_native_stereo_mix(int world_x, int camera_x) {
    StereoMix result;
    int relative = world_x - camera_x;
    if (relative < -333) return result;
    if (relative < 0) {
        result.left = ((333 + relative) * 100) / 333;
        return result;
    }
    relative -= 333;
    if (relative < 0) {
        result.left = 100;
        return result;
    }
    relative -= 666;
    if (relative < 0) {
        result.right = ((relative + 666) * 100) / 666;
        result.left = 100 - result.right;
        return result;
    }
    relative -= 333;
    if (relative < 0) {
        result.right = 100;
        return result;
    }
    relative -= 333;
    if (relative < 0) result.right = (-relative * 100) / 333;
    return result;
}

bool run_case(const std::filesystem::path& decoded_dat,
              const std::filesystem::path& complete_vfs,
              int tap_ticks,
              int release_ticks,
              std::ofstream& rows,
              bool& saw_003,
              bool& saw_004,
              std::string& error) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 2833;
    config.background_id = 23;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 naruto;
    naruto.slot = 0;
    naruto.object_id = 2;
    naruto.x = 620;
    naruto.z = 650;
    naruto.hp = 500;
    naruto.mp = 500;
    naruto.team = 1;
    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 1;
    lee.object_id = 7;
    lee.x = 1200;
    lee.z = 650;
    lee.hp = 500;
    lee.mp = 500;
    lee.team = 2;
    config.combatants = {naruto, lee};

    ntsd28_playable::GameSession28 session(decoded_dat, complete_vfs);
    if (!session.initialize(config, error)) return false;
    for (int tick = 1; tick <= 30; ++tick) {
        const bool right = tick <= tap_ticks ||
                           tick > tap_ticks + release_ticks;
        ntsd28::InputButtons28 input;
        if (right) input.set(ntsd28::InputKey28::right);
        session.set_input(0, input);
        session.set_input(1, {});
        session.step();
        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        if (actor == nullptr || session.last_tick() == nullptr) {
            error = "formal Naruto or completed tick disappeared";
            return false;
        }
        const auto& events = session.last_tick()->audio_events;
        if (events.empty()) {
            rows << tap_ticks << '\t' << release_ticks << '\t' << tick
                 << '\t' << (right ? 1 : 0) << '\t' << actor->position.x
                 << '\t' << actor->frame.action << '\t' << session.camera_x()
                 << "\t-1\t-1\t-1\t-\t-1\t-1\n";
            continue;
        }
        for (std::size_t index = 0; index < events.size(); ++index) {
            const auto& event = events[index];
            const auto mix = project_native_stereo_mix(
                event.world_x, session.camera_x());
            rows << tap_ticks << '\t' << release_ticks << '\t' << tick
                 << '\t' << (right ? 1 : 0) << '\t' << actor->position.x
                 << '\t' << actor->frame.action << '\t' << session.camera_x()
                 << '\t' << index << '\t' << static_cast<int>(event.source)
                 << '\t' << event.world_x << '\t' << event.resource_path
                 << '\t' << mix.left << '\t' << mix.right << '\n';
            saw_003 = saw_003 || event.resource_path.find("003.wav") !=
                                       std::string::npos;
            saw_004 = saw_004 || event.resource_path.find("004.wav") !=
                                       std::string::npos;
        }
    }
    return true;
}

}  // namespace

int main(int argc, char** argv) {
    if (argc != 4) {
        std::cerr << "usage: native_stereo_camera_trace <decoded_dat> <complete_vfs> <output_tsv>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to overwrite existing output\n";
        return 3;
    }
    std::filesystem::create_directories(output.parent_path());
    std::ofstream rows(output, std::ios::binary);
    if (!rows) return 4;
    rows << "tap_ticks\trelease_ticks\ttick\tright\tactor_x\tactor_action\tcamera_x\tevent_index\tevent_source\tevent_world_x\tresource_path\tleft_percent\tright_percent\n";
    bool selected_case_found = false;
    for (int tap_ticks : {2, 1, 3}) {
        for (int release_ticks : {2, 1}) {
            bool saw_003 = false;
            bool saw_004 = false;
            std::string error;
            if (!run_case(argv[1], argv[2], tap_ticks, release_ticks,
                          rows, saw_003, saw_004, error)) {
                std::cerr << "case tap=" << tap_ticks << " release="
                          << release_ticks << " failed: " << error << '\n';
                return 5;
            }
            if (saw_003 && saw_004) {
                std::cout << "natural selected cues reached: tap=" << tap_ticks
                          << " release=" << release_ticks << '\n';
                selected_case_found = true;
                break;
            }
        }
        if (selected_case_found) break;
    }
    rows.close();
    if (!rows) return 6;
    if (!selected_case_found) {
        std::cerr << "selected 003/004 pair absent; all controlled tick rows preserved\n";
        return 7;
    }
    return 0;
}
