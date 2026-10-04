#include "ntsd28_playable/game_session_lfr.h"

#include <array>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

struct Candidate {
    int emitter_id;
    int emitter_action;
    int child_id;
};

bool run_candidate(const std::filesystem::path& runtime,
                   const std::filesystem::path& output,
                   const Candidate candidate) {
    const auto csv_path = output / "source-host-rows.csv";
    const auto lfr_path = output / "source-packets.lfr";
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing existing output: " << output << '\n';
        return false;
    }
    std::filesystem::create_directories(output);

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 56;
    config.enemy_id = candidate.emitter_id;
    config.background_id = 23;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 survivor;
    survivor.slot = 0;
    survivor.object_id = 56;
    survivor.x = 500;
    survivor.z = 650;
    survivor.hp = 500;
    survivor.mp = 500;
    survivor.team = 1;
    auto emitter = survivor;
    emitter.slot = 1;
    emitter.object_id = candidate.emitter_id;
    emitter.x = 900;
    emitter.team = 2;
    emitter.action = candidate.emitter_action;
    config.combatants = {survivor, emitter};

    ntsd28_playable::GameSession28 session(runtime, runtime);
    std::string error;
    if (!session.initialize(config, error)) {
        std::cerr << "initialize OID" << candidate.emitter_id << ": "
                  << error << '\n';
        return false;
    }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) {
        std::cerr << "record begin: " << error << '\n';
        return false;
    }
    std::ofstream csv(csv_path, std::ios::binary);
    if (!csv) return false;
    csv << "host,world_tick,timer,timer_after,winner,flow_groups,"
           "world_groups,child_count,emitter_action\n";

    bool saw_single = false;
    bool saw_return = false;
    bool saw_pause = false;
    bool saw_second_single = false;
    bool saw_resume = false;
    int held_timer = -1;
    for (int host = 1; host <= 20; ++host) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "capture host " << host << ": " << error << '\n';
            return false;
        }
        const auto* world = session.world();
        if (world == nullptr) return false;
        bool group1 = false;
        bool group2 = false;
        int child_count = 0;
        for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots;
             ++slot) {
            const auto* entity = world->entity(slot);
            if (entity == nullptr || entity->object_type != 0 ||
                entity->current_hp <= 0) continue;
            if (entity->battle_group == 1) group1 = true;
            if (entity->battle_group == 2) group2 = true;
            if (entity->object_id == candidate.child_id &&
                entity->battle_group == 2) ++child_count;
        }
        const auto& flow = session.battle_flow();
        const auto* live_emitter = world->entity(1);
        const int flow_groups = static_cast<int>(flow.outcome.living_groups.size());
        csv << host << ',' << world->sequence() << ',' << flow.timer << ','
            << flow.timer_after << ',' << flow.outcome.winner_group << ','
            << flow_groups << ',' << static_cast<int>(group1) +
                                   static_cast<int>(group2)
            << ',' << child_count << ','
            << (live_emitter == nullptr ? -1 : live_emitter->frame.action)
            << '\n';
        if (!saw_single && flow_groups == 1 && flow.timer > 0) {
            saw_single = true;
        } else if (saw_single && !saw_return && child_count > 0) {
            saw_return = true;
            held_timer = flow.timer;
        } else if (saw_return && !saw_pause && flow_groups >= 2 &&
                   flow.timer == held_timer) {
            saw_pause = true;
        } else if (saw_pause && !saw_second_single && !group2) {
            saw_second_single = true;
        } else if (saw_second_single && flow_groups <= 1 &&
                   flow.timer > held_timer) {
            saw_resume = true;
        }
    }
    csv.close();
    if (!csv) return false;
    std::cout << "emitter=" << candidate.emitter_id
              << " child=" << candidate.child_id
              << " single=" << saw_single
              << " return=" << saw_return
              << " pause=" << saw_pause
              << " second_single=" << saw_second_single
              << " resume=" << saw_resume << '\n';
    if (!saw_resume) return true;

    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << "record finish: " << error << '\n';
        return false;
    }
    std::ofstream lfr(lfr_path, std::ios::binary);
    if (!lfr) return false;
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    return static_cast<bool>(lfr);
}

}  // namespace

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: return_resume_lfr_probe <formal_runtime> "
                     "<new_output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing existing output directory\n";
        return 3;
    }
    const std::array<Candidate, 2> candidates{{{220, 0, 9}, {230, 77, 14}}};
    bool success = true;
    for (const auto candidate : candidates) {
        success = run_candidate(argv[1],
                                output / std::to_string(candidate.emitter_id),
                                candidate) && success;
    }
    return success ? 0 : 4;
}
