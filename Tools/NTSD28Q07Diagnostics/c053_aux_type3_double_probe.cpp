#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

namespace {

constexpr std::uint32_t seed = 682973786u;

ntsd28_playable::BattleConfig28 make_config(int aux_x, int aux_y) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = seed;
    config.character_id = 65;
    config.enemy_id = 702;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 ank;
    ank.slot = 0;
    ank.object_id = 65;
    ank.x = 610;
    ank.z = 400;
    ank.hp = ank.base_hp = ank.mp = 500;
    ank.team = 1;
    ank.action = 511;

    auto jira = ank;
    jira.slot = 1;
    jira.object_id = 702;
    jira.x = 500;
    jira.team = 2;
    jira.action = 553;

    auto auxiliary = ank;
    auxiliary.slot = 2;
    auxiliary.object_id = 251;
    auxiliary.x = aux_x;
    auxiliary.y = aux_y;
    auxiliary.team = 1;
    auxiliary.action = 0;

    config.combatants = {ank, jira, auxiliary};
    return config;
}

int target_slot(const ntsd28::BattleWorld28& world) {
    for (std::size_t slot = 3; slot < ntsd28::EngineProfile28::maximum_slots;
         ++slot) {
        const auto* entity = world.entity(slot);
        if (entity && entity->object_id == 808) return static_cast<int>(slot);
    }
    return -1;
}

struct CaseResult {
    int first_uj = -1;
    int first_double = -1;
    int max_uj = 0;
    int terminal = -1;
};

CaseResult run_case(const std::filesystem::path& runtime, int aux_x,
                    int aux_y, std::ofstream& hits, int max_ticks,
                    const std::filesystem::path* lfr_path = nullptr,
                    std::ofstream* rows = nullptr) {
    ntsd28_playable::GameSession28 session(runtime, runtime);
    std::string error;
    if (!session.initialize(make_config(aux_x, aux_y), error)) {
        throw std::runtime_error("initialize x=" + std::to_string(aux_x) +
                                 " y=" + std::to_string(aux_y) + ": " + error);
    }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (lfr_path && !recorder.begin(session, error)) {
        throw std::runtime_error("LFR begin: " + error);
    }
    CaseResult result;
    for (int tick = 1; tick <= max_ticks; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.set_input(2, {});
        session.step();
        if (lfr_path && !recorder.capture_after_step(session, error)) {
            throw std::runtime_error("LFR capture tick " +
                                     std::to_string(tick) + ": " + error);
        }
        const auto* step = session.last_tick();
        const auto* world = session.world();
        if (!step || !world) {
            result.terminal = tick;
            break;
        }
        const int target = target_slot(*world);
        if (rows) {
            *rows << tick;
            for (int slot : {0, 1, 2, 50, 51}) {
                const auto* entity = world->entity(slot);
                *rows << ',' << (entity ? 1 : 0) << ','
                      << (entity ? entity->object_id : -1) << ','
                      << (entity ? entity->frame.action : -1) << ','
                      << (entity ? entity->current_hp : -1) << ','
                      << (entity ? entity->position.x : 0) << ','
                      << (entity ? entity->position.y : 0) << ','
                      << (entity ? entity->position.z : 0);
            }
            const auto rng = world->random().state();
            *rows << ',' << rng.crt_state << ',' << rng.crt_calls << ','
                  << rng.synchronized.counter << ','
                  << rng.synchronized.index << ','
                  << rng.synchronized.calls << '\n';
        }
        if (target < 0) continue;
        int uj_count = 0;
        bool auxiliary_hit = false;
        bool generated_hit = false;
        for (const auto& hit : step->hits) {
            if (hit.target_slot != static_cast<std::size_t>(target)) continue;
            const auto* attacker = world->entity(hit.attacker_slot);
            const int attacker_oid = attacker ? attacker->object_id : -1;
            const bool applied =
                hit.status == ntsd28::WorldStandardHitStatus28::applied;
            const bool uj = applied && hit.target_type3_post_hit_action >= 0;
            uj_count += uj;
            auxiliary_hit |= uj && hit.attacker_slot == 2 &&
                             attacker_oid == 251;
            generated_hit |= uj && attacker_oid == 875;
            hits << aux_x << ',' << aux_y << ',' << tick << ','
                 << hit.attacker_slot << ',' << attacker_oid << ','
                 << target << ',' << static_cast<int>(hit.status) << ','
                 << (hit.interaction ? hit.interaction->effect : -1) << ','
                 << hit.target_type3_post_hit_action << ','
                 << (world->entity(target) ? world->entity(target)->current_hp
                                            : -1) << '\n';
        }
        if (uj_count && result.first_uj < 0) result.first_uj = tick;
        if (uj_count > result.max_uj) result.max_uj = uj_count;
        if (auxiliary_hit && generated_hit && result.first_double < 0)
            result.first_double = tick;
    }
    if (lfr_path) {
        if (result.terminal >= 0) {
            throw std::runtime_error("capture terminal tick " +
                                     std::to_string(result.terminal));
        }
        std::vector<std::uint8_t> bytes;
        if (!recorder.finish_to_memory(session, bytes, error)) {
            throw std::runtime_error("LFR finish: " + error);
        }
        if (std::filesystem::exists(*lfr_path)) {
            throw std::runtime_error("refusing existing LFR output");
        }
        std::ofstream lfr(*lfr_path, std::ios::binary);
        lfr.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
        if (!lfr) throw std::runtime_error("LFR output write failed");
    }
    return result;
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 3 && argc != 5) {
        std::cerr << "usage: c053_aux_type3_double_probe "
                     "<formal_runtime> <new_output_dir> [aux_x aux_y]\n";
        return 2;
    }
    try {
        const std::filesystem::path runtime(argv[1]);
        const std::filesystem::path output(argv[2]);
        if (!std::filesystem::exists(
                runtime / "decoded_dat/data/data.txt")) {
            throw std::runtime_error("formal runtime unavailable");
        }
        if (std::filesystem::exists(output)) {
            std::cerr << "refusing to overwrite output directory\n";
            return 3;
        }
        std::filesystem::create_directories(output);
        std::ofstream summary(output / "summary.csv");
        std::ofstream hits(output / "hits.csv");
        if (!summary || !hits) throw std::runtime_error("output open failed");
        summary << "aux_x,aux_y,first_uj,first_double,max_uj,terminal\n";
        hits << "aux_x,aux_y,tick,attacker_slot,attacker_oid,target_slot,"
                "status,effect,post_uj,target_hp_after_tick\n";
        if (argc == 5) {
            const int aux_x = std::stoi(argv[3]);
            const int aux_y = std::stoi(argv[4]);
            std::ofstream rows(output / "source-ticks.csv");
            if (!rows) throw std::runtime_error("source rows open failed");
            rows << "tick";
            for (int slot : {0, 1, 2, 50, 51}) {
                rows << ",slot" << slot << "_active"
                     << ",slot" << slot << "_oid"
                     << ",slot" << slot << "_action"
                     << ",slot" << slot << "_hp"
                     << ",slot" << slot << "_x"
                     << ",slot" << slot << "_y"
                     << ",slot" << slot << "_z";
            }
            rows << ",crt_state,crt_calls,sync_counter,sync_index,"
                    "sync_calls\n";
            const auto lfr_path = output / "source.lfr";
            const auto result = run_case(runtime, aux_x, aux_y, hits, 40,
                                         &lfr_path, &rows);
            summary << aux_x << ',' << aux_y << ',' << result.first_uj
                    << ',' << result.first_double << ',' << result.max_uj
                    << ',' << result.terminal << '\n';
            if (!summary || !hits || !rows) {
                throw std::runtime_error("capture output write failed");
            }
            std::cout << "capture x=" << aux_x << " y=" << aux_y
                      << " firstDouble=" << result.first_double
                      << " maxUj=" << result.max_uj << '\n';
            return 0;
        }
        int cases = 0;
        int doubles = 0;
        for (int aux_x = 670; aux_x <= 730; aux_x += 5) {
            for (int aux_y = -90; aux_y <= -30; aux_y += 10) {
                const auto result = run_case(runtime, aux_x, aux_y, hits, 12);
                summary << aux_x << ',' << aux_y << ',' << result.first_uj
                        << ',' << result.first_double << ',' << result.max_uj
                        << ',' << result.terminal << '\n';
                ++cases;
                if (result.first_double >= 0) {
                    ++doubles;
                    std::cout << "double x=" << aux_x << " y=" << aux_y
                              << " tick=" << result.first_double << '\n';
                }
            }
        }
        if (!summary || !hits) throw std::runtime_error("output write failed");
        std::cout << "cases=" << cases << " doubles=" << doubles << '\n';
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 4;
    }
}
