#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

namespace {

constexpr std::uint32_t seed = 682973786u;

ntsd28_playable::BattleConfig28 make_config(int tobi_y) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = seed;
    config.character_id = 53;
    config.enemy_id = 2;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 tobi;
    tobi.slot = 0;
    tobi.object_id = 53;
    tobi.x = 600;
    tobi.y = tobi_y;
    tobi.z = 400;
    tobi.hp = tobi.base_hp = tobi.mp = 500;
    tobi.team = 1;
    tobi.action = 510;

    auto enemy = tobi;
    enemy.slot = 1;
    enemy.object_id = 2;
    enemy.x = 1100;
    enemy.y = 0;
    enemy.team = 2;
    enemy.action = 0;
    config.combatants = {tobi, enemy};
    return config;
}

struct CaseResult {
    int first_birth = -1;
    int first_action_2 = -1;
    int first_action_3 = -1;
    int first_action_0 = -1;
    int first_slot = -1;
    int terminal = -1;
};

CaseResult run_case(const std::filesystem::path& runtime, int tobi_y,
                    std::ofstream& rows, int max_ticks,
                    const std::filesystem::path* lfr_path = nullptr) {
    ntsd28_playable::GameSession28 session(runtime, runtime);
    std::string error;
    if (!session.initialize(make_config(tobi_y), error)) {
        throw std::runtime_error("initialize y=" + std::to_string(tobi_y) +
                                 ": " + error);
    }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (lfr_path && !recorder.begin(session, error)) {
        throw std::runtime_error("LFR begin: " + error);
    }

    CaseResult result;
    const auto* initial = session.world()->entity(0);
    rows << tobi_y << ",0," << (initial ? initial->frame.action : -1)
         << ',' << (initial ? initial->position.x : 0)
         << ',' << (initial ? initial->position.y : 0)
         << ",-1,-1,-1,0,0,0,-1,-1,-1\n";
    for (int tick = 1; tick <= max_ticks; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (lfr_path && !recorder.capture_after_step(session, error)) {
            throw std::runtime_error("LFR capture tick " +
                                     std::to_string(tick) + ": " + error);
        }
        const auto* world = session.world();
        if (!world || !session.last_tick()) {
            result.terminal = tick;
            break;
        }
        const auto* tobi = world->entity(0);
        const ntsd28::EntityState28* projectile = nullptr;
        int projectile_slot = -1;
        if (result.first_slot >= 0) {
            projectile = world->entity(static_cast<std::size_t>(result.first_slot));
            if (projectile && projectile->object_id == 251) {
                projectile_slot = result.first_slot;
            } else {
                projectile = nullptr;
            }
        }
        if (!projectile && result.first_slot < 0) {
            for (std::size_t slot = 2;
                 slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
                const auto* candidate = world->entity(slot);
                if (!candidate || candidate->object_id != 251) continue;
                projectile = candidate;
                projectile_slot = static_cast<int>(slot);
                result.first_slot = projectile_slot;
                result.first_birth = tick;
                break;
            }
        }
        if (projectile) {
            const int action = projectile->frame.action;
            if (action == 2 && result.first_action_2 < 0)
                result.first_action_2 = tick;
            if (action == 3 && result.first_action_3 < 0)
                result.first_action_3 = tick;
            if (action == 0 && result.first_action_0 < 0)
                result.first_action_0 = tick;
        }
        int frame_from = -1;
        int frame_to = -1;
        int frame_status = -1;
        for (const auto& event : session.last_tick()->frames) {
            if (event.slot != 0) continue;
            frame_from = event.frame.from_action;
            frame_to = event.frame.to_action;
            frame_status = static_cast<int>(event.frame.status);
            break;
        }
        rows << tobi_y << ',' << tick << ','
             << (tobi ? tobi->frame.action : -1) << ','
             << (tobi ? tobi->position.x : 0) << ','
             << (tobi ? tobi->position.y : 0) << ','
             << projectile_slot << ','
             << (projectile ? projectile->frame.action : -1) << ','
             << (projectile ? projectile->current_hp : -1) << ','
             << (projectile ? projectile->position.x : 0) << ','
             << (projectile ? projectile->position.y : 0) << ','
             << (projectile ? projectile->position.z : 0) << ','
             << frame_from << ',' << frame_to << ',' << frame_status << '\n';
    }
    if (lfr_path) {
        if (result.terminal >= 0)
            throw std::runtime_error("LFR capture reached terminal tick");
        std::vector<std::uint8_t> bytes;
        if (!recorder.finish_to_memory(session, bytes, error))
            throw std::runtime_error("LFR finish: " + error);
        if (std::filesystem::exists(*lfr_path))
            throw std::runtime_error("refusing existing LFR output");
        std::ofstream output(*lfr_path, std::ios::binary);
        output.write(reinterpret_cast<const char*>(bytes.data()),
                     static_cast<std::streamsize>(bytes.size()));
        if (!output) throw std::runtime_error("LFR output write failed");
    }
    return result;
}

void write_summary(std::ofstream& summary, int y, const CaseResult& value) {
    summary << y << ',' << value.first_birth << ',' << value.first_slot
            << ',' << value.first_action_2 << ',' << value.first_action_3
            << ',' << value.first_action_0 << ',' << value.terminal << '\n';
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: c053_tobi_firz_reach_probe "
                     "<formal_runtime> <new_output_dir>\n";
        return 2;
    }
    try {
        const std::filesystem::path runtime(argv[1]);
        const std::filesystem::path output(argv[2]);
        if (!std::filesystem::exists(runtime / "decoded_dat/data/data.txt"))
            throw std::runtime_error("formal runtime unavailable");
        if (std::filesystem::exists(output)) {
            std::cerr << "refusing to overwrite output directory\n";
            return 3;
        }
        std::filesystem::create_directories(output);
        std::ofstream summary(output / "summary.csv");
        std::ofstream rows(output / "ticks.csv");
        if (!summary || !rows) throw std::runtime_error("output open failed");
        summary << "tobi_y,first_birth,first_slot,first_action2,"
                   "first_action3,first_action0,terminal\n";
        rows << "tobi_y,tick,tobi_action,tobi_x,tobi_y_after_tick,"
                "projectile_slot,projectile_action,projectile_hp,"
                "projectile_x,projectile_y,projectile_z,"
                "frame_from,frame_to,frame_status\n";
        int capture_y = 0;
        bool found = false;
        for (int y : {-80, -140, -200, 0}) {
            const auto value = run_case(runtime, y, rows, 40);
            write_summary(summary, y, value);
            if (!found && value.first_birth >= 0) {
                capture_y = y;
                found = true;
            }
        }
        if (found) {
            const auto capture = output / "capture";
            std::filesystem::create_directory(capture);
            std::ofstream capture_rows(capture / "ticks.csv");
            if (!capture_rows) throw std::runtime_error("capture rows open failed");
            capture_rows << "tobi_y,tick,tobi_action,tobi_x,tobi_y_after_tick,"
                            "projectile_slot,projectile_action,projectile_hp,"
                            "projectile_x,projectile_y,projectile_z,"
                            "frame_from,frame_to,frame_status\n";
            const auto lfr = capture / "source.lfr";
            const auto value = run_case(runtime, capture_y, capture_rows, 40,
                                        &lfr);
            std::ofstream capture_summary(capture / "summary.csv");
            capture_summary << "tobi_y,first_birth,first_slot,first_action2,"
                               "first_action3,first_action0,terminal\n";
            write_summary(capture_summary, capture_y, value);
            if (!capture_rows || !capture_summary)
                throw std::runtime_error("capture output write failed");
        }
        if (!summary || !rows)
            throw std::runtime_error("matrix output write failed");
        std::cout << "capture_y=" << (found ? capture_y : 9999) << '\n';
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 4;
    }
}
