#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

namespace {

struct Profile {
    const char* name;
    int action;
    int y;
    bool ordinary_jump;
};

struct Result {
    int combo_tick = -1;
    int first_510 = -1;
    int first_512 = -1;
    int first_251 = -1;
    int child_slot = -1;
    int first_action_0 = -1;
};

ntsd28_playable::BattleConfig28 make_config(const Profile& profile) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 0;
    config.enemy_id = 2;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 0;
    actor.x = 600;
    actor.y = profile.y;
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = profile.action;
    auto enemy = actor;
    enemy.slot = 1;
    enemy.object_id = 2;
    enemy.x = 1100;
    enemy.y = 0;
    enemy.team = 2;
    enemy.action = 0;
    config.combatants = {actor, enemy};
    return config;
}

Result run_case(const std::filesystem::path& runtime,
                const std::filesystem::path& output,
                const Profile& profile) {
    ntsd28_playable::GameSession28 session(runtime, runtime);
    std::string error;
    if (!session.initialize(make_config(profile), error)) {
        throw std::runtime_error(std::string(profile.name) + ": " + error);
    }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) throw std::runtime_error(error);
    std::ofstream rows(output / (std::string(profile.name) + "-ticks.csv"));
    if (!rows) throw std::runtime_error("ticks output unavailable");
    rows << "tick,input_mask,sampled_mask,input_phase,pre_action,"
            "pre_hit_fa,loaded_212_hit_fa,tobi_action,tobi_y,"
            "frame_from,frame_to,frame_status,child_slot,child_action,"
            "child_x,child_y,combo_fa,action_count,first_source,"
            "first_requested,first_applied,last_source,last_requested,"
            "last_applied,oid251_spawn_events,oid251_spawn_status,"
            "oid251_spawn_slot,oid251_source_line\n";

    Result result;
    const auto* initial = session.world()->entity(0);
    if (!initial || initial->frame.action != profile.action)
        throw std::runtime_error("initial action mismatch");
    const auto* initial_jump = initial->definition->frame(212);
    if (!initial_jump ||
        initial_jump->values.integer("hit_Fa").value_or(-1) != 510)
        throw std::runtime_error("OID0 formal frame212 hit_Fa is not 510");
    int combo_end = -1;
    for (int tick = 1; tick <= 40; ++tick) {
        const auto* before = session.world()->entity(0);
        if (!before) throw std::runtime_error("Tobi disappeared");
        const int pre_action = before->frame.action;
        const auto* pre_frame = before->definition->frame(pre_action);
        const auto* frame_212 = before->definition->frame(212);
        const int pre_hit_fa = pre_frame == nullptr
                                   ? -1
                                   : pre_frame->values.integer("hit_Fa").value_or(-1);
        const int loaded_212_hit_fa = frame_212 == nullptr
                                          ? -1
                                          : frame_212->values.integer("hit_Fa").value_or(-1);
        if (result.combo_tick < 0 &&
            ((!profile.ordinary_jump && tick == 1) ||
             (profile.ordinary_jump && tick > 4 &&
              before->frame.action == 212))) {
            result.combo_tick = tick;
            combo_end = tick + 2;
        }
        ntsd28::InputButtons28 input;
        int input_mask = 0;
        if (profile.ordinary_jump && tick <= 4) {
            input.set(ntsd28::InputKey28::jump);
            input_mask = 0x20;
        } else if (result.combo_tick > 0 && tick <= combo_end) {
            input.set(ntsd28::InputKey28::defend);
            input.set(ntsd28::InputKey28::right);
            input.set(ntsd28::InputKey28::attack);
            input_mask = 0x58;
        }
        session.set_input(0, input);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error))
            throw std::runtime_error("LFR capture: " + error);
        const auto* step = session.last_tick();
        const auto* world = session.world();
        if (!step || !world || !world->entity(0))
            throw std::runtime_error("complete tick unavailable");
        const auto* tobi = world->entity(0);
        if (tobi->frame.action == 510 && result.first_510 < 0)
            result.first_510 = tick;
        if (tobi->frame.action == 512 && result.first_512 < 0)
            result.first_512 = tick;
        const ntsd28::EntityState28* child = nullptr;
        if (result.child_slot >= 0) {
            child = world->entity(static_cast<std::size_t>(result.child_slot));
            if (child && child->object_id != 251) child = nullptr;
        }
        if (result.child_slot < 0) {
            for (std::size_t slot = 2;
                 slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
                const auto* candidate = world->entity(slot);
                if (!candidate || candidate->object_id != 251) continue;
                child = candidate;
                result.child_slot = static_cast<int>(slot);
                result.first_251 = tick;
                break;
            }
        }
        if (child && child->frame.action == 0 && result.first_action_0 < 0)
            result.first_action_0 = tick;

        int from = -1;
        int to = -1;
        int status = -1;
        for (const auto& event : step->frames) {
            if (event.slot != 0) continue;
            from = event.frame.from_action;
            to = event.frame.to_action;
            status = static_cast<int>(event.frame.status);
            break;
        }
        const auto& sampled = step->recording_physical_inputs_0_7[0];
        const auto& attempts = step->inputs[0].actions;
        const auto* first_attempt = attempts.empty() ? nullptr : &attempts.front();
        const auto* last_attempt = attempts.empty() ? nullptr : &attempts.back();
        int sampled_mask = 0;
        for (int key = 0; key < 7; ++key) {
            if (sampled.pressed[static_cast<std::size_t>(key)])
                sampled_mask |= 1 << key;
        }
        int oid251_spawn_events = 0;
        int oid251_spawn_status = -1;
        int oid251_spawn_slot = -1;
        int oid251_source_line = -1;
        for (const auto& event : step->spawns.events) {
            if (event.object_id != 251) continue;
            ++oid251_spawn_events;
            if (oid251_spawn_events != 1) continue;
            oid251_spawn_status = static_cast<int>(event.status);
            oid251_spawn_slot = static_cast<int>(event.slot);
            oid251_source_line = static_cast<int>(event.source_line);
        }
        rows << tick << ',' << input_mask << ',' << sampled_mask << ','
             << step->input_update_phase_4a0b90 << ','
             << pre_action << ',' << pre_hit_fa << ','
             << loaded_212_hit_fa << ','
             << tobi->frame.action << ',' << tobi->position.y << ','
             << from << ',' << to << ',' << status << ','
             << (child ? result.child_slot : -1) << ','
             << (child ? child->frame.action : -1) << ','
             << (child ? child->position.x : 0) << ','
             << (child ? child->position.y : 0) << ','
             << static_cast<int>(tobi->input.combo_state[0]) << ','
             << attempts.size() << ','
             << (first_attempt ? first_attempt->source : "none") << ','
             << (first_attempt ? first_attempt->requested_action : -1) << ','
             << (first_attempt && first_attempt->applied ? 1 : 0) << ','
             << (last_attempt ? last_attempt->source : "none") << ','
             << (last_attempt ? last_attempt->requested_action : -1) << ','
             << (last_attempt && last_attempt->applied ? 1 : 0) << ','
             << oid251_spawn_events << ',' << oid251_spawn_status << ','
             << oid251_spawn_slot << ',' << oid251_source_line << '\n';
    }
    if (!rows) throw std::runtime_error("ticks output failed");
    if (result.first_251 >= 0) {
        std::vector<std::uint8_t> bytes;
        if (!recorder.finish_to_memory(session, bytes, error))
            throw std::runtime_error("LFR finish: " + error);
        std::ofstream lfr(output / (std::string(profile.name) + ".lfr"),
                          std::ios::binary);
        if (!lfr) throw std::runtime_error("LFR output unavailable");
        lfr.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
        if (!lfr) throw std::runtime_error("LFR output failed");
    }
    return result;
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 3) return 2;
    try {
        const std::filesystem::path runtime(argv[1]);
        const std::filesystem::path output(argv[2]);
        if (!std::filesystem::exists(runtime / "decoded_dat/data/data.txt"))
            throw std::runtime_error("formal runtime unavailable");
        if (std::filesystem::exists(output))
            throw std::runtime_error("refusing existing output directory");
        std::filesystem::create_directories(output);
        std::ofstream summary(output / "summary.csv");
        if (!summary) throw std::runtime_error("summary output unavailable");
        summary << "profile,actor_oid,combo_tick,first_510,first_512,first_251,"
                   "child_slot,first_action_0\n";
        for (const Profile profile : {
                 Profile{"jump_from_zero", 0, 0, true},
                 Profile{"controlled_212", 212, -80, false},
                 Profile{"controlled_212_high", 212, -130, false}}) {
            const auto result = run_case(runtime, output, profile);
            summary << profile.name << ",0," << result.combo_tick << ','
                    << result.first_510 << ',' << result.first_512 << ','
                    << result.first_251 << ',' << result.child_slot << ','
                    << result.first_action_0 << '\n';
        }
        if (!summary) throw std::runtime_error("summary output failed");
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 4;
    }
}
