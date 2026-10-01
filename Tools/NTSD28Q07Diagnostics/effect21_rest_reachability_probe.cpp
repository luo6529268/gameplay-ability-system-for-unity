#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

namespace {

int bounded_int(const char* text, int min, int max) {
    const std::string value(text);
    std::size_t parsed = 0;
    const int result = std::stoi(value, &parsed);
    if (parsed != value.size() || result < min || result > max)
        throw std::runtime_error("probe argument outside declared range");
    return result;
}

std::string one_line(std::string value) {
    for (char& ch : value) {
        if (ch == '\t' || ch == '\r' || ch == '\n') ch = ' ';
    }
    return value;
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 6) return 2;
    try {
        const std::filesystem::path authority_root(argv[1]);
        const std::filesystem::path output(argv[2]);
        const int actor_action = bounded_int(argv[3], 160, 161);
        const int first_x = bounded_int(argv[4], 450, 550);
        const int second_x = bounded_int(argv[5], 450, 650);
        if (std::filesystem::exists(output)) return 3;

        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 682973786u;
        config.character_id = 211;
        config.enemy_id = 2;
        config.background_id = 1;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;
        ntsd28_playable::CombatantConfig28 actor;
        actor.slot = 0;
        actor.object_id = 211;
        actor.x = 500;
        actor.y = 0;
        actor.z = 400;
        actor.hp = actor.base_hp = actor.mp = 500;
        actor.team = 1;
        actor.action = actor_action;
        actor.facing = false;
        auto first = actor;
        first.slot = 1;
        first.object_id = 2;
        first.x = first_x;
        first.action = 0;
        first.team = 2;
        first.facing = true;
        auto second = first;
        second.slot = 2;
        second.x = second_x;
        config.combatants = {actor, first, second};

        const auto runtime = authority_root / "resources" / "runtime";
        ntsd28_playable::GameSession28 session(runtime, runtime);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << error << '\n';
            return 4;
        }
        ntsd28_playable::GameSessionLfr28 recorder;
        if (!recorder.begin(session, error)) {
            std::cerr << error << '\n';
            return 5;
        }
        std::filesystem::create_directories(output);
        std::ofstream rows(output / "source-ticks.tsv", std::ios::binary);
        std::ofstream hits(output / "source-hits.tsv", std::ios::binary);
        if (!rows || !hits) return 6;
        rows << "tick\tactor_action\tfirst_action\tfirst_hp\tfirst_rest\tsecond_action\tsecond_hp\tsecond_rest\tcandidates_appended\thit_count\n";
        hits << "tick\tordinal\tattacker_slot\ttarget_slot\tcandidate_index\teffect\tstatus\tterminates\tmessage\n";
        int first_applied = 0;
        int first_rejected_without_terminate = 0;
        int second_applied = 0;
        for (int tick = 1; tick <= 3; ++tick) {
            for (std::size_t slot = 0; slot < 3; ++slot)
                session.set_input(slot, ntsd28::InputButtons28{});
            session.step();
            if (!recorder.capture_after_step(session, error)) {
                std::cerr << error << '\n';
                return 7;
            }
            const auto* world = session.world();
            const auto* result = session.last_tick();
            if (!world || !result || !world->entity(0) ||
                !world->entity(1) || !world->entity(2)) return 8;
            const auto* a = world->entity(0);
            const auto* b = world->entity(1);
            const auto* c = world->entity(2);
            rows << tick << '\t' << a->frame.action << '\t'
                 << b->frame.action << '\t' << b->current_hp << '\t'
                 << static_cast<int>(world->victim_rest(1, 0)) << '\t'
                 << c->frame.action << '\t' << c->current_hp << '\t'
                 << static_cast<int>(world->victim_rest(2, 0)) << '\t'
                 << result->candidates.candidates_appended << '\t'
                 << result->hits.size() << '\n';
            for (std::size_t index = 0; index < result->hits.size(); ++index) {
                const auto& hit = result->hits[index];
                const int effect = hit.interaction ? hit.interaction->effect : -1;
                const bool applied = hit.status == ntsd28::WorldStandardHitStatus28::applied;
                if (hit.attacker_slot == 0 && hit.target_slot == 1 && applied)
                    ++first_applied;
                if (hit.attacker_slot == 0 && hit.target_slot == 1 && !applied &&
                    !hit.terminates_attacker_invocation && effect == 21)
                    ++first_rejected_without_terminate;
                if (hit.attacker_slot == 0 && hit.target_slot == 2 && applied)
                    ++second_applied;
                hits << tick << '\t' << index << '\t' << hit.attacker_slot
                     << '\t' << hit.target_slot << '\t' << hit.candidate_index
                     << '\t' << effect << '\t' << static_cast<int>(hit.status)
                     << '\t' << hit.terminates_attacker_invocation << '\t'
                     << one_line(hit.message) << '\n';
            }
        }
        rows.close();
        hits.close();
        if (!rows || !hits) return 9;
        std::vector<std::uint8_t> bytes;
        if (!recorder.finish_to_memory(session, bytes, error)) {
            std::cerr << error << '\n';
            return 10;
        }
        std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
        if (!lfr) return 11;
        lfr.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
        lfr.close();
        if (!lfr) return 12;
        std::cout << "{\"action\":" << actor_action
                  << ",\"firstX\":" << first_x
                  << ",\"secondX\":" << second_x
                  << ",\"firstApplied\":" << first_applied
                  << ",\"firstRejectedWithoutTerminate\":" << first_rejected_without_terminate
                  << ",\"secondApplied\":" << second_applied
                  << ",\"lfrBytes\":" << bytes.size() << "}\n";
        return 0;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 13;
    }
}
