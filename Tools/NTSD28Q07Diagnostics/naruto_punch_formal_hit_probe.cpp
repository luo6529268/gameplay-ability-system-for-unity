#include "ntsd28_playable/game_session_lfr.h"

#include <array>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

namespace {

struct Case {
    int target_x;
    int attack_tick;
    int attack_length;
};

struct CaseResult {
    bool input_seen = false;
    bool authored_hit_frame_seen = false;
    bool attributed_knockout = false;
    int first_hit_frame_tick = -1;
    int knockout_tick = -1;
};

bool run_case(const std::filesystem::path& extracted_root,
              const std::filesystem::path& complete_vfs_root,
              const Case& selected,
              std::ofstream& rows,
              std::vector<std::uint8_t>& witness_lfr,
              Case& witness_case,
              CaseResult& result,
              std::string& error) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 2;
    config.enemy_id = 2;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 attacker;
    attacker.slot = 0;
    attacker.object_id = 2;
    attacker.x = 500;
    attacker.z = 650;
    attacker.hp = 500;
    attacker.mp = 500;
    attacker.team = 1;
    attacker.action = 0;
    ntsd28_playable::CombatantConfig28 target;
    target.slot = 1;
    target.object_id = 2;
    target.x = selected.target_x;
    target.z = 650;
    target.hp = 10;
    target.mp = 500;
    target.team = 2;
    target.action = 0;
    config.combatants = {attacker, target};

    ntsd28_playable::GameSession28 session(extracted_root, complete_vfs_root);
    if (!session.initialize(config, error)) return false;
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) return false;
    std::size_t observed_knockout_count = 0;
    for (int tick = 1; tick <= 30; ++tick) {
        ntsd28::InputButtons28 buttons;
        const bool attack_pressed = tick >= selected.attack_tick &&
            tick < selected.attack_tick + selected.attack_length;
        if (attack_pressed) buttons.set(ntsd28::InputKey28::attack);
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) return false;
        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        const auto* victim = world == nullptr ? nullptr : world->entity(1);
        if (actor == nullptr) {
            error = "Naruto attacker disappeared during scan";
            return false;
        }
        if (attack_pressed) result.input_seen = true;
        const int tick_action = actor->frame.tick_action_snapshot;
        if (tick_action == 62 || tick_action == 513) {
            result.authored_hit_frame_seen = true;
            if (result.first_hit_frame_tick < 0) result.first_hit_frame_tick = tick;
        }
        const auto& knockout_events = world->knockout_events();
        int new_ko_source = -1;
        int new_ko_victim = -1;
        int new_ko_credit = -1;
        int new_ko_four_owner = -1;
        for (std::size_t index = observed_knockout_count;
             index < knockout_events.size(); ++index) {
            const auto& event = knockout_events[index];
            new_ko_source = static_cast<int>(event.source_slot);
            new_ko_victim = static_cast<int>(event.victim_slot);
            new_ko_credit = static_cast<int>(event.credit_slot);
            new_ko_four_owner = static_cast<int>(event.four_owner_slot);
            if (event.source_slot == 0 && event.victim_slot == 1 &&
                (event.credit_slot == 0 || event.four_owner_slot == 0) &&
                (tick_action == 62 || tick_action == 513) &&
                victim != nullptr && victim->current_hp <= 0) {
                result.attributed_knockout = true;
                if (result.knockout_tick < 0) result.knockout_tick = tick;
            }
        }
        observed_knockout_count = knockout_events.size();
        const auto random = world->random().state();
        rows << selected.target_x << ',' << selected.attack_tick << ','
             << selected.attack_length << ',' << tick << ','
             << (attack_pressed ? 1 : 0) << ','
             << world->input_update_phase_4a0b90() << ','
             << actor->frame.action << ',' << tick_action << ','
             << actor->current_hp << ','
             << (victim == nullptr ? -999 : victim->current_hp) << ','
             << (victim == nullptr ? -1 : victim->frame.action) << ','
             << new_ko_source << ',' << new_ko_victim << ','
             << new_ko_credit << ',' << new_ko_four_owner << ','
             << random.crt_state << ',' << random.crt_calls << ','
             << random.synchronized.counter << ','
             << random.synchronized.index << ','
             << random.synchronized.calls << ','
             << random.synchronized.last_call_site << ','
             << world->random().synchronized_table_hash() << '\n';
    }
    if (result.input_seen && result.authored_hit_frame_seen &&
        result.attributed_knockout && witness_lfr.empty()) {
        if (!recorder.finish_to_memory(session, witness_lfr, error)) return false;
        witness_case = selected;
    }
    return true;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: naruto_punch_formal_hit_probe <extracted_root> <complete_vfs_root> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    std::filesystem::create_directories(output);
    const auto csv = output / "punch-hit-scan.csv";
    const auto summary = output / "punch-hit-summary.csv";
    const auto lfr = output / "first-attributed-punch-hit.lfr";
    if (std::filesystem::exists(csv) || std::filesystem::exists(summary) ||
        std::filesystem::exists(lfr)) {
        std::cerr << "refusing to overwrite existing diagnostic output\n";
        return 3;
    }
    std::ofstream rows(csv, std::ios::binary);
    std::ofstream summaries(summary, std::ios::binary);
    if (!rows || !summaries) return 4;
    rows << "target_x,attack_tick,attack_length,tick,attack,phase,actor_action,tick_action,actor_hp,target_hp,target_action,ko_source,ko_victim,ko_credit,ko_four_owner,crt_state,crt_calls,sync_counter,sync_index,sync_calls,sync_last_site,sync_table_hash\n";
    summaries << "target_x,attack_tick,attack_length,input_seen,authored_hit_frame_seen,attributed_knockout,first_hit_frame_tick,knockout_tick\n";
    std::vector<std::uint8_t> witness_lfr;
    Case witness_case{};
    constexpr std::array<int, 6> positions{{525, 535, 540, 545, 550, 560}};
    for (const int target_x : positions) {
        for (const int attack_tick : {2, 4, 6}) {
            for (const int attack_length : {1, 2}) {
                const Case selected{target_x, attack_tick, attack_length};
                CaseResult result;
                std::string error;
                if (!run_case(argv[1], argv[2], selected, rows, witness_lfr,
                              witness_case, result, error)) {
                    std::cerr << "case X" << target_x << " attack@"
                              << attack_tick << "x" << attack_length
                              << " failed: " << error << '\n';
                    return 5;
                }
                summaries << target_x << ',' << attack_tick << ','
                          << attack_length << ','
                          << (result.input_seen ? 1 : 0) << ','
                          << (result.authored_hit_frame_seen ? 1 : 0) << ','
                          << (result.attributed_knockout ? 1 : 0) << ','
                          << result.first_hit_frame_tick << ','
                          << result.knockout_tick << '\n';
            }
        }
    }
    rows.close();
    summaries.close();
    if (!rows || !summaries) return 6;
    if (!witness_lfr.empty()) {
        std::ofstream encoded(lfr, std::ios::binary);
        if (!encoded) return 7;
        encoded.write(reinterpret_cast<const char*>(witness_lfr.data()),
                      static_cast<std::streamsize>(witness_lfr.size()));
        encoded.close();
        if (!encoded) return 8;
        std::cout << "first attributed authored punch KO: target X"
                  << witness_case.target_x << ", attack@"
                  << witness_case.attack_tick << "x"
                  << witness_case.attack_length << ", "
                  << witness_lfr.size() << " LFR bytes\n";
    } else {
        std::cout << "no attributed authored punch KO in 36 bounded cases\n";
    }
    return 0;
}
