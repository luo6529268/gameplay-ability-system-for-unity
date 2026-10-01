#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 3) return 2;
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 36;
    config.enemy_id = 2;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 36;
    actor.x = 500;
    actor.y = 0;
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = 243;
    ntsd28_playable::CombatantConfig28 target = actor;
    target.slot = 1;
    target.object_id = 2;
    target.x = 550;
    target.hp = target.base_hp = 18;
    target.action = 0;
    target.team = 2;
    config.combatants = {actor, target};

    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(config, error)) { std::cerr << error; return 4; }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) { std::cerr << error; return 5; }
    std::ofstream entities(output / "source-entities.csv", std::ios::binary);
    std::ofstream frames(output / "source-frames.csv", std::ios::binary);
    std::ofstream rng(output / "source-rng.csv", std::ios::binary);
    if (!entities || !frames || !rng) return 6;
    entities << "tick,slot,oid,type,action,state,counter,x,y,z,vx,vy,vz,hp,owner,team,revive_lives,revive_next_hp\n"
             << std::setprecision(17);
    frames << "tick,slot,oid,from,to,counter_after,message\n";
    rng << "tick,crt_state,crt_calls,custom_counter,custom_index,custom_calls,last_call_site\n";
    int terminal_holds = 0;
    int zero_hp_ticks = 0;
    for (int tick = 1; tick <= 128; ++tick) {
        session.set_input(0, ntsd28::InputButtons28{});
        session.set_input(1, ntsd28::InputButtons28{});
        session.step();
        if (!recorder.capture_after_step(session, error)) { std::cerr << error; return 7; }
        const auto* world = session.world();
        for (std::size_t slot = 0; slot < 2; ++slot) {
            const auto* entity = world->entity(slot);
            if (entity == nullptr) return 8;
            const auto* frame = entity->definition->frame(entity->frame.action);
            entities << world->sequence() << ',' << slot << ',' << entity->object_id << ','
                     << entity->object_type << ',' << entity->frame.action << ','
                     << (frame ? frame->values.integer("state").value_or(0) : -1) << ','
                     << entity->frame.frame_counter << ',' << entity->position.x << ','
                     << entity->position.y << ',' << entity->position.z << ','
                     << entity->motion.x << ',' << entity->motion.y << ',' << entity->motion.z << ','
                     << entity->current_hp << ',' << entity->owner_slot << ','
                     << entity->battle_group << ',' << entity->revive_lives_30c << ','
                     << entity->revive_next_hp_314 << '\n';
            if (slot == 1 && entity->current_hp <= 0) ++zero_hp_ticks;
        }
        for (const auto& event : session.last_tick()->frames) {
            const auto* current = world->entity(event.slot);
            const auto& frame = event.frame;
            frames << world->sequence() << ',' << event.slot << ',' << event.object_id << ','
                   << frame.from_action << ',' << frame.to_action << ','
                   << (current == nullptr ? -1 : current->frame.frame_counter) << ','
                   << frame.message << '\n';
            if (event.slot == 1 &&
                frame.message == "terminal physical participant retained state-14 lying frame")
                ++terminal_holds;
        }
        const auto state = world->random().state();
        rng << world->sequence() << ',' << state.crt_state << ',' << state.crt_calls << ','
            << state.synchronized.counter << ',' << state.synchronized.index << ','
            << state.synchronized.calls << ',' << state.synchronized.last_call_site << '\n';
    }
    entities.close(); frames.close(); rng.close();
    if (!entities || !frames || !rng) return 9;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) { std::cerr << error; return 10; }
    std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 11;
    std::cout << "ticks=128 terminal_holds=" << terminal_holds
              << " target_zero_hp_ticks=" << zero_hp_ticks << '\n';
    return 0;
}
