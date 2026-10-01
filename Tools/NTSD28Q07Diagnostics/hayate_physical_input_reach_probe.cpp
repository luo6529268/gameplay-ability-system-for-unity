#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

namespace {

int bounded_int(const char* value, int minimum, int maximum) {
    std::size_t length = 0;
    const int result = std::stoi(value, &length);
    if (length != std::string(value).size() ||
        result < minimum || result > maximum)
        throw std::runtime_error("argument outside declared range");
    return result;
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 5) return 2;
    try {
        const std::filesystem::path authority_root(argv[1]);
        const std::filesystem::path output(argv[2]);
        const int mode = bounded_int(argv[3], 0, 1);
        const int limit = bounded_int(argv[4], 1, 60);
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
        actor.z = 400;
        actor.hp = actor.base_hp = actor.mp = 500;
        actor.team = 1;
        actor.action = mode == 0 ? 0 : 213;
        actor.facing = false;
        auto first = actor;
        first.slot = 1;
        first.object_id = 2;
        first.x = 589;
        first.action = 0;
        first.team = 2;
        first.facing = true;
        auto second = first;
        second.slot = 2;
        second.x = 619;
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
        rows << "tick\tinput_mask\tphase\tsampled_mask\tactor_action\tactor_x\tactor_y\tactor_z\tactor_mp\tfirst_action\tfirst_hp\tfirst_x\tsecond_action\tsecond_hp\tsecond_x\tcount417\tfirst417slot\tfirst417action\tfirst417x\tcount211\tfirst211slot\tfirst211action\tfirst211x\n";
        hits << "tick\tordinal\tattacker_oid\ttarget_slot\teffect\tstatus\tterminates\n";

        bool fired_combo = false;
        int hold_combo_until = -1;
        int input_tick = -1;
        int first_160_tick = -1;
        int first_417_tick = -1;
        int first_211_tick = -1;
        int first_double_hit_tick = -1;
        for (int tick = 1; tick <= limit; ++tick) {
            const auto* before = session.world()->entity(0);
            if (!before) return 7;
            ntsd28::InputButtons28 input;
            int mask = 0;
            if (mode == 0 && tick <= 4) {
                input.set(ntsd28::InputKey28::jump);
                mask = 0x20;
            } else {
                if (!fired_combo &&
                    ((mode == 0 && before->frame.action == 212) ||
                     (mode == 1 && tick == 1))) {
                    fired_combo = true;
                    input_tick = tick;
                    hold_combo_until = tick + 2;
                }
            }
            if (fired_combo && tick <= hold_combo_until) {
                input.set(ntsd28::InputKey28::defend);
                input.set(ntsd28::InputKey28::right);
                input.set(ntsd28::InputKey28::attack);
                mask = 0x58;
            }
            session.set_input(0, input);
            session.set_input(1, ntsd28::InputButtons28{});
            session.set_input(2, ntsd28::InputButtons28{});
            session.step();
            if (!recorder.capture_after_step(session, error)) {
                std::cerr << error << '\n';
                return 8;
            }
            const auto* world = session.world();
            const auto* result = session.last_tick();
            if (!world || !result || !world->entity(0) ||
                !world->entity(1) || !world->entity(2)) return 9;
            const auto* a = world->entity(0);
            const auto* b = world->entity(1);
            const auto* c = world->entity(2);
            if (a->frame.action == 160 && first_160_tick < 0)
                first_160_tick = tick;
            int count_417 = 0;
            int count_211 = 0;
            int slot_417 = -1;
            int slot_211 = -1;
            decltype(world->entity(0)) child_417 = nullptr;
            decltype(world->entity(0)) child_211 = nullptr;
            for (std::size_t slot = 3; slot < 128; ++slot) {
                const auto* entity = world->entity(slot);
                if (!entity) continue;
                if (entity->object_id == 417) {
                    ++count_417;
                    if (!child_417) {
                        child_417 = entity;
                        slot_417 = static_cast<int>(slot);
                    }
                } else if (entity->object_id == 211) {
                    ++count_211;
                    if (!child_211) {
                        child_211 = entity;
                        slot_211 = static_cast<int>(slot);
                    }
                }
            }
            if (count_417 && first_417_tick < 0) first_417_tick = tick;
            if (count_211 && first_211_tick < 0) first_211_tick = tick;
            bool applied_first = false;
            bool applied_second = false;
            for (std::size_t ordinal = 0; ordinal < result->hits.size(); ++ordinal) {
                const auto& hit = result->hits[ordinal];
                const auto* attacker = world->entity(hit.attacker_slot);
                const int oid = attacker ? attacker->object_id : -1;
                const int effect = hit.interaction ? hit.interaction->effect : -1;
                if (oid == 211 &&
                    hit.status == ntsd28::WorldStandardHitStatus28::applied) {
                    applied_first |= hit.target_slot == 1;
                    applied_second |= hit.target_slot == 2;
                }
                hits << tick << '\t' << ordinal << '\t' << oid << '\t'
                     << hit.target_slot << '\t' << effect << '\t'
                     << static_cast<int>(hit.status) << '\t'
                     << hit.terminates_attacker_invocation << '\n';
            }
            if (applied_first && applied_second && first_double_hit_tick < 0)
                first_double_hit_tick = tick;
            const auto& sampled = result->recording_physical_inputs_0_7[0];
            const int sampled_mask =
                (sampled[ntsd28::InputKey28::jump] ? 0x20 : 0) |
                (sampled[ntsd28::InputKey28::defend] ? 0x40 : 0) |
                (sampled[ntsd28::InputKey28::right] ? 0x08 : 0) |
                (sampled[ntsd28::InputKey28::attack] ? 0x10 : 0);
            rows << tick << '\t' << mask << '\t'
                 << result->input_update_phase_4a0b90 << '\t'
                 << sampled_mask << '\t' << a->frame.action << '\t'
                 << a->position.x << '\t' << a->position.y << '\t'
                 << a->position.z << '\t' << a->current_mp << '\t'
                 << b->frame.action << '\t' << b->current_hp << '\t'
                 << b->position.x << '\t' << c->frame.action << '\t'
                 << c->current_hp << '\t' << c->position.x << '\t'
                 << count_417 << '\t' << slot_417 << '\t'
                 << (child_417 ? child_417->frame.action : -1) << '\t'
                 << (child_417 ? child_417->position.x : 0) << '\t'
                 << count_211 << '\t' << slot_211 << '\t'
                 << (child_211 ? child_211->frame.action : -1) << '\t'
                 << (child_211 ? child_211->position.x : 0) << '\n';
        }
        rows.close();
        hits.close();
        if (!rows || !hits) return 10;
        std::vector<std::uint8_t> bytes;
        if (!recorder.finish_to_memory(session, bytes, error)) {
            std::cerr << error << '\n';
            return 11;
        }
        std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
        if (!lfr) return 12;
        lfr.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
        lfr.close();
        if (!lfr) return 13;
        std::cout << "{\"mode\":" << mode
                  << ",\"inputTick\":" << input_tick
                  << ",\"first160Tick\":" << first_160_tick
                  << ",\"first417Tick\":" << first_417_tick
                  << ",\"first211Tick\":" << first_211_tick
                  << ",\"firstDoubleHitTick\":" << first_double_hit_tick
                  << ",\"lfrBytes\":" << bytes.size() << "}\n";
        return 0;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 14;
    }
}
