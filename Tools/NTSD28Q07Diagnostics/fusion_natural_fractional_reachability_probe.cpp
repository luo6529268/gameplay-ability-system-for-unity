#include "ntsd28_playable/game_session.h"

#include <cmath>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <stdexcept>
#include <string>

namespace {

ntsd28_playable::BattleConfig28 make_config(bool row0) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = row0 ? 7 : 10;
    config.enemy_id = row0 ? 8 : 11;
    config.background_id = row0 ? 23 : 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 primary;
    primary.slot = 0;
    primary.object_id = row0 ? 7 : 10;
    primary.x = row0 ? 304 : 320;
    primary.y = 0;
    primary.z = row0 ? 600 : 400;
    primary.hp = 100;
    primary.base_hp = 500;
    primary.mp = 500;
    primary.team = 1;
    primary.action = 9;
    auto partner = primary;
    partner.slot = 1;
    partner.object_id = row0 ? 8 : 11;
    partner.x = 300;
    partner.facing = row0;
    config.combatants = {primary, partner};
    return config;
}

struct CaseResult {
    std::string name;
    int first_fusion = -1;
    int first_split = -1;
    int fractional_split = -1;
    int first_unresolved = -1;
    int terminal_tick = -1;
};

ntsd28::InputButtons28 input_for(const std::string& name,
                                 int tick, bool row0) {
    ntsd28::InputButtons28 input;
    const int jump_start = row0 ? 4470 : 170;
    const int input_end = row0 ? 4505 : 205;
    if (name == "late_jump" && tick >= jump_start && tick <= input_end) {
        input.set(ntsd28::InputKey28::jump);
    } else if (name == "right_hold" && tick >= 2 && tick <= input_end) {
        input.set(ntsd28::InputKey28::right);
    } else if (name == "diagonal_run" && !row0) {
        if ((tick >= 2 && tick <= 3) || (tick >= 6 && tick <= 15))
            input.set(ntsd28::InputKey28::right);
        if (tick >= 6 && tick <= 15)
            input.set(ntsd28::InputKey28::depth_up);
    }
    return input;
}

bool fractional(const ntsd28::EntityState28* entity) {
    if (!entity) return false;
    const auto& p = entity->position;
    return p.precise_x != static_cast<double>(p.x) ||
           p.precise_y != static_cast<double>(p.y) ||
           p.precise_z != static_cast<double>(p.z);
}

CaseResult run_case(const std::filesystem::path& root,
                    const std::string& name,
                    std::ofstream& rows, bool row0) {
    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(make_config(row0), error)) {
        throw std::runtime_error("session initialize: " + error);
    }
    CaseResult result;
    result.name = name;
    const int max_ticks = row0 ? 4550 : 240;
    for (int tick = 1; tick <= max_ticks; ++tick) {
        const auto* before = session.world()->entity(0);
        const int before_oid = before ? before->object_id : -1;
        const int before_action = before ? before->frame.action : -1;
        const int before_timer = before ? before->input_special_timer_338 : -1;
        const int bx = before ? before->position.x : 0;
        const int by = before ? before->position.y : 0;
        const int bz = before ? before->position.z : 0;
        const double bpx = before ? before->position.precise_x : 0;
        const double bpy = before ? before->position.precise_y : 0;
        const double bpz = before ? before->position.precise_z : 0;
        const bool before_fractional = fractional(before);
        session.set_input(0, input_for(name, tick, row0));
        session.set_input(1, ntsd28::InputButtons28{});
        session.step();
        const auto* last = session.last_tick();
        if (!last) {
            result.terminal_tick = tick;
            break;
        }
        const auto* world = session.world();
        const auto* after = world->entity(0);
        const auto rng = world->random().state();
        if (last->fusions.fused && result.first_fusion < 0)
            result.first_fusion = tick;
        if (last->fusions.defused && result.first_split < 0) {
            result.first_split = tick;
            if (before_fractional) result.fractional_split = tick;
        }
        if (last->fusions.unresolved && result.first_unresolved < 0)
            result.first_unresolved = tick;
        rows << name << ',' << tick << ',' << world->sequence() << ','
             << before_oid << ',' << before_action << ',' << before_timer << ','
             << bx << ',' << by << ',' << bz << ','
             << bpx << ',' << bpy << ',' << bpz << ',' << before_fractional << ','
             << (after ? after->object_id : -1) << ','
             << (after ? after->frame.action : -1) << ','
             << (after ? after->input_special_timer_338 : -1) << ','
             << (after ? after->position.x : 0) << ','
             << (after ? after->position.y : 0) << ','
             << (after ? after->position.z : 0) << ','
             << (after ? after->position.precise_x : 0) << ','
             << (after ? after->position.precise_y : 0) << ','
             << (after ? after->position.precise_z : 0) << ','
             << last->fusions.fused << ',' << last->fusions.defused << ','
             << last->fusions.unresolved << ',' << rng.crt_state << ','
             << rng.crt_calls << ',' << rng.synchronized.counter << ','
             << rng.synchronized.index << ',' << rng.synchronized.calls << '\n';
    }
    return result;
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 3 && argc != 4) return 2;
    try {
        const bool row0 = argc == 4 && std::string(argv[3]) == "row0";
        if (argc == 4 && !row0) return 2;
        const std::filesystem::path root(argv[1]);
        const std::filesystem::path output(argv[2]);
        if (!std::filesystem::exists(root / "decoded_dat/data/fusion.dat"))
            throw std::runtime_error("formal fusion DAT unavailable");
        if (std::filesystem::exists(output))
            throw std::runtime_error("refusing to overwrite output directory");
        std::filesystem::create_directories(output);
        std::ofstream rows(output / "source-ticks.csv");
        std::ofstream summary(output / "summary.txt");
        if (!rows || !summary) throw std::runtime_error("output open failed");
        rows << std::setprecision(17);
        rows << "case,tick,sequence,before_oid,before_action,before_timer,"
                "before_x,before_y,before_z,before_precise_x,before_precise_y,"
                "before_precise_z,before_fractional,after_oid,after_action,"
                "after_timer,after_x,after_y,after_z,after_precise_x,"
                "after_precise_y,after_precise_z,fused,defused,unresolved,"
                "crt_state,crt_calls,sync_counter,sync_index,sync_calls\n";
        for (const char* name : {"neutral", "late_jump", "right_hold", "diagonal_run"}) {
            if (row0 && std::string(name) == "diagonal_run") continue;
            const auto result = run_case(root, name, rows, row0);
            summary << name << " fusion=" << result.first_fusion
                    << " split=" << result.first_split
                    << " fractional_split=" << result.fractional_split
                    << " unresolved=" << result.first_unresolved
                    << " terminal=" << result.terminal_tick << '\n';
        }
        if (!rows || !summary) throw std::runtime_error("output write failed");
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 3;
    }
}
