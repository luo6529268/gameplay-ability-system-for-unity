#include "ntsd28/battle_world.h"
#include "ntsd28_playable/game_session.h"
#include "ntsd28_playable/game_session_lfr.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

namespace {

void write_entity(std::ofstream& stream, int tick, const char* phase,
                  int slot, const ntsd28::BattleWorld28& world) {
    const auto* entity = world.entity(static_cast<std::size_t>(slot));
    stream << tick << ',' << phase << ',' << slot << ',' << world.sequence() << ',';
    if (entity == nullptr) {
        stream << "-1,-1,-1,0,0,0,0,0,0,0\n";
        return;
    }
    stream << entity->object_id << ',' << entity->object_type << ','
           << entity->frame.action << ',' << entity->position.x << ','
           << entity->position.y << ',' << entity->position.z << ','
           << entity->position.precise_z << ',' << entity->current_hp << ','
           << entity->battle_group << ',' << entity->frame.frame_counter << '\n';
}

ntsd28_playable::BattleConfig28 make_config(int target_x, int background_id) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 0;
    config.character_id = 7;
    config.enemy_id = 2;
    config.background_id = background_id;
    config.bgm_selection_49f18c = 2;

    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 0;
    lee.object_id = 7;
    lee.x = 500;
    lee.z = 400;
    lee.hp = lee.base_hp = lee.mp = 500;
    lee.team = 1;
    lee.action = 24;

    ntsd28_playable::CombatantConfig28 naruto = lee;
    naruto.slot = 1;
    naruto.object_id = 2;
    naruto.x = target_x;
    naruto.team = 2;
    naruto.action = 0;
    config.combatants = {lee, naruto};
    return config;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 4 && argc != 5) return 2;
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    int target_x = 0;
    int background_id = 23;
    try {
        std::size_t parsed = 0;
        const std::wstring value(argv[3]);
        target_x = std::stoi(value, &parsed);
        if (parsed != value.size() || target_x < 350 || target_x > 1200)
            return 2;
    } catch (...) { return 2; }
    if (argc == 5) {
        try {
            std::size_t parsed = 0;
            const std::wstring value(argv[4]);
            background_id = std::stoi(value, &parsed);
            if (parsed != value.size() ||
                (background_id != 1 && background_id != 23)) return 2;
        } catch (...) { return 2; }
    }
    if (std::filesystem::exists(output)) return 3;

    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(make_config(target_x, background_id), error)) {
        std::cerr << "initialize: " << error << '\n';
        return 4;
    }
    const auto* lee = session.world()->entity(0);
    const auto* naruto = session.world()->entity(1);
    if (lee == nullptr || naruto == nullptr || lee->object_id != 7 ||
        naruto->object_id != 2 || lee->frame.action != 24 ||
        naruto->frame.action != 0) return 5;

    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) {
        std::cerr << "recorder: " << error << '\n';
        return 6;
    }
    std::filesystem::create_directories(output);
    std::ofstream entities(output / "source-ticks.csv", std::ios::binary);
    std::ofstream hits(output / "relation-hits.csv", std::ios::binary);
    std::ofstream rng(output / "source-rng.csv", std::ios::binary);
    if (!entities || !hits || !rng) return 7;
    entities << "tick,phase,slot,sequence,oid,type,action,x,y,z,precise_z,hp,team,counter\n"
             << std::setprecision(17);
    hits << "tick,attacker_slot,target_slot,kind,status,attacker_action,target_action,message\n";
    rng << "tick,crt_state,crt_calls,custom_counter,custom_index,custom_calls\n";
    write_entity(entities, 0, "initial", 0, *session.world());
    write_entity(entities, 0, "initial", 1, *session.world());

    int first_kind8 = -1;
    int applied_count = 0;
    constexpr int kTicks = 12;
    for (int tick = 1; tick <= kTicks; ++tick) {
        write_entity(entities, tick, "before", 0, *session.world());
        write_entity(entities, tick, "before", 1, *session.world());
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "record tick " << tick << ": " << error << '\n';
            return 8;
        }
        const auto* result = session.last_tick();
        if (result == nullptr) return 9;
        for (const auto& hit : result->relation_hits) {
            hits << tick << ',' << hit.attacker_slot << ',' << hit.target_slot
                 << ',' << hit.interaction_kind << ','
                 << static_cast<int>(hit.status) << ',' << hit.attacker_action
                 << ',' << hit.target_action << ',' << hit.message << '\n';
            if (hit.attacker_slot == 0 && hit.target_slot == 1 &&
                hit.interaction_kind == 8 &&
                hit.status == ntsd28::WorldRelationHitStatus28::applied) {
                ++applied_count;
                if (first_kind8 < 0) first_kind8 = tick;
            }
        }
        write_entity(entities, tick, "after", 0, *session.world());
        write_entity(entities, tick, "after", 1, *session.world());
        const auto state = session.world()->random().state();
        rng << session.world()->sequence() << ',' << state.crt_state << ','
            << state.crt_calls << ',' << state.synchronized.counter << ','
            << state.synchronized.index << ',' << state.synchronized.calls << '\n';
    }

    std::vector<std::uint8_t> packets;
    if (!recorder.finish_to_memory(session, packets, error)) {
        std::cerr << "finish recorder: " << error << '\n';
        return 10;
    }
    std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
    if (!lfr) return 11;
    lfr.write(reinterpret_cast<const char*>(packets.data()),
              static_cast<std::streamsize>(packets.size()));
    lfr.close();
    entities.close();
    hits.close();
    rng.close();
    if (!lfr || !entities || !hits || !rng) return 12;

    std::ofstream summary(output / "summary.txt", std::ios::binary);
    if (!summary) return 13;
    summary << "target_x=" << target_x << " ticks=" << kTicks
            << " first_kind8_tick=" << first_kind8
            << " kind8_applied_count=" << applied_count;
    if (argc == 5) summary << " background_id=" << background_id;
    summary << '\n';
    std::cout << "target_x=" << target_x
              << " first_kind8_tick=" << first_kind8
            << " kind8_applied_count=" << applied_count;
    if (argc == 5) std::cout << " background_id=" << background_id;
    std::cout << '\n';
    return 0;
}
