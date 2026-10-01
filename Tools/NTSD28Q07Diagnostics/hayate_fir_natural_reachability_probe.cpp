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
    if (argc != 7) return 2;
    try {
        const std::filesystem::path authority_root(argv[1]);
        const std::filesystem::path output(argv[2]);
        const int actor_action = bounded_int(argv[3], 160, 162);
        if (actor_action == 161) return 2;
        const int first_x = bounded_int(argv[4], 500, 1330);
        const int second_x = bounded_int(argv[5], 500, 1330);
        const int tick_limit = bounded_int(argv[6], 1, 120);
        if (std::filesystem::exists(output)) return 3;

        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 682973786u;
        config.character_id = 73;
        config.enemy_id = 2;
        config.background_id = 1;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;
        ntsd28_playable::CombatantConfig28 actor;
        actor.slot = 0;
        actor.object_id = 73;
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
        rows << "tick\tactor_action\tfirst_action\tfirst_hp\tfirst_x\tsecond_action\tsecond_hp\tsecond_x\tcount417\tfirst417slot\tfirst417action\tfirst417x\tfirst417y\tfirst417z\tcount211\tfirst211slot\tfirst211action\tfirst211x\tfirst211y\tfirst211z\thits211\n";
        hits << "tick\tordinal\tattacker_slot\tattacker_oid\ttarget_slot\tcandidate_index\teffect\tstatus\tterminates\tmessage\n";

        int first_417_tick = -1;
        int first_211_tick = -1;
        int first_double_211_tick = -1;
        for (int tick = 1; tick <= tick_limit; ++tick) {
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
            const auto* actor_entity = world->entity(0);
            const auto* first_entity = world->entity(1);
            const auto* second_entity = world->entity(2);
            int count_417 = 0;
            int count_211 = 0;
            int first_417_slot = -1;
            int first_211_slot = -1;
            decltype(world->entity(0)) first_417 = nullptr;
            decltype(world->entity(0)) first_211 = nullptr;
            for (std::size_t slot = 3; slot < 128; ++slot) {
                const auto* entity = world->entity(slot);
                if (!entity) continue;
                if (entity->object_id == 417) {
                    ++count_417;
                    if (!first_417) {
                        first_417 = entity;
                        first_417_slot = static_cast<int>(slot);
                    }
                }
                if (entity->object_id == 211) {
                    ++count_211;
                    if (!first_211) {
                        first_211 = entity;
                        first_211_slot = static_cast<int>(slot);
                    }
                }
            }
            if (count_417 && first_417_tick < 0) first_417_tick = tick;
            if (count_211 && first_211_tick < 0) first_211_tick = tick;
            int applied_first = 0;
            int applied_second = 0;
            int hits_211 = 0;
            for (std::size_t index = 0; index < result->hits.size(); ++index) {
                const auto& hit = result->hits[index];
                const auto* hit_attacker = world->entity(hit.attacker_slot);
                const int attacker_oid = hit_attacker ? hit_attacker->object_id : -1;
                const int effect = hit.interaction ? hit.interaction->effect : -1;
                if (attacker_oid == 211) {
                    ++hits_211;
                    if (hit.status == ntsd28::WorldStandardHitStatus28::applied) {
                        if (hit.target_slot == 1) ++applied_first;
                        if (hit.target_slot == 2) ++applied_second;
                    }
                }
                hits << tick << '\t' << index << '\t' << hit.attacker_slot
                     << '\t' << attacker_oid << '\t' << hit.target_slot
                     << '\t' << hit.candidate_index << '\t' << effect
                     << '\t' << static_cast<int>(hit.status) << '\t'
                     << hit.terminates_attacker_invocation << '\t'
                     << one_line(hit.message) << '\n';
            }
            if (applied_first && applied_second && first_double_211_tick < 0)
                first_double_211_tick = tick;
            rows << tick << '\t' << actor_entity->frame.action
                 << '\t' << first_entity->frame.action << '\t'
                 << first_entity->current_hp << '\t' << first_entity->position.x
                 << '\t' << second_entity->frame.action << '\t'
                 << second_entity->current_hp << '\t' << second_entity->position.x
                 << '\t' << count_417 << '\t' << first_417_slot << '\t'
                 << (first_417 ? first_417->frame.action : -1) << '\t'
                 << (first_417 ? first_417->position.x : 0) << '\t'
                 << (first_417 ? first_417->position.y : 0) << '\t'
                 << (first_417 ? first_417->position.z : 0) << '\t'
                 << count_211 << '\t' << first_211_slot << '\t'
                 << (first_211 ? first_211->frame.action : -1) << '\t'
                 << (first_211 ? first_211->position.x : 0) << '\t'
                 << (first_211 ? first_211->position.y : 0) << '\t'
                 << (first_211 ? first_211->position.z : 0) << '\t'
                 << hits_211 << '\n';
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
        std::cout << "{\"actorAction\":" << actor_action
                  << ",\"firstX\":" << first_x
                  << ",\"secondX\":" << second_x
                  << ",\"ticks\":" << tick_limit
                  << ",\"first417Tick\":" << first_417_tick
                  << ",\"first211Tick\":" << first_211_tick
                  << ",\"firstDouble211Tick\":" << first_double_211_tick
                  << ",\"lfrBytes\":" << bytes.size() << "}\n";
        return 0;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 13;
    }
}
