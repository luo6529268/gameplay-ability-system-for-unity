#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

namespace {
int discover_airborne_routes(const std::filesystem::path& root,
                             const std::filesystem::path& output) {
    if (std::filesystem::exists(output)) return 3;
    ntsd28::ObjectDefinitionCatalog28 catalog;
    if (!catalog.load_extracted_root(root).success) return 4;
    std::filesystem::create_directories(output);
    std::ofstream csv(output / "airborne-opoint-routes.csv", std::ios::binary);
    csv << "parent_oid,parent_type,parent_path,predecessor,op_action,child_oid,child_type,child_path,child_action,y_offset\n";
    int count = 0;
    for (const auto& item : catalog.entries()) {
        const auto& parent = item.second;
        if (!parent.definition) continue;
        for (const auto& frame : parent.definition->frames) {
            for (const auto* point : frame.blocks("opoint")) {
                if (point->values.integer("kind").value_or(0) != 1) continue;
                const auto* child = catalog.find(point->values.integer("oid").value_or(-1));
                if (!child || !child->definition || !child->definition->frame(212)) continue;
                const int action = point->values.integer("action").value_or(0);
                const auto* initial = child->definition->frame(action);
                if (!initial || initial->values.integer("state").value_or(0) != 0) continue;
                const int offset = point->values.integer("y").value_or(0) -
                                   frame.values.integer("centery").value_or(0);
                for (const auto& predecessor : parent.definition->frames) {
                    if (predecessor.values.integer("next").value_or(0) != frame.id) continue;
                    ++count;
                    csv << parent.object_id << ',' << parent.object_type << ',' << parent.source_path << ','
                        << predecessor.id << ',' << frame.id << ',' << child->object_id << ','
                        << child->object_type << ',' << child->source_path << ',' << action << ',' << offset << '\n';
                }
            }
        }
    }
    csv.close();
    if (!csv) return 6;
    std::cout << "catalog_entries=" << catalog.size() << " airborne_routes=" << count << '\n';
    return 0;
}
}

int main(int argc, char** argv) {
    if (argc != 4 && argc != 6) return 2;
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    const std::string mode(argv[3]);
    if (argc == 4 && mode == "discover") return discover_airborne_routes(root, output);
    const bool airborne = mode == "jump";
    const bool grounded = mode == "ground";
    const bool controlled_airborne = mode == "air20" || mode == "air40";
    const bool dei_airborne = mode == "dei-air";
    const bool relative = mode == "kar433" || mode == "kar434";
    const bool spawning = argc == 6 && mode == "spawn";
    int parent_oid = 0;
    int parent_action = 0;
    if (spawning) {
        try { parent_oid = std::stoi(argv[4]); parent_action = std::stoi(argv[5]); }
        catch (...) { return 2; }
        if (parent_oid < 0 || parent_action < 0 || parent_action > 998) return 2;
    }
    if (!airborne && !grounded && !relative && !spawning && !controlled_airborne &&
        !dei_airborne) return 2;
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = spawning ? parent_oid : (relative ? 77 : (dei_airborne ? 10 : 2));
    config.enemy_id = 2;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = config.character_id;
    actor.x = 500;
    actor.y = mode == "air20" ? -20 : (mode == "air40" ? -40 : 0);
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = spawning ? parent_action : (relative ? (mode == "kar433" ? 433 : 434) : 0);
    ntsd28_playable::CombatantConfig28 target = actor;
    target.slot = 1;
    target.object_id = 2;
    target.x = 1100;
    target.y = 0;
    target.action = 0;
    target.team = 2;
    config.combatants = {actor, target};
    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(config, error)) { std::cerr << error; return 4; }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) { std::cerr << error; return 5; }
    std::ofstream csv(output / "source-entities.csv", std::ios::binary);
    std::ofstream frames(output / "source-frames.csv", std::ios::binary);
    std::ofstream rng(output / "source-rng.csv", std::ios::binary);
    if (!csv || !frames || !rng) return 6;
    csv << "tick,slot,oid,type,action,state,counter,x,y,z,vx,vy,vz,hp,owner,team\n"
        << std::setprecision(17);
    frames << "tick,slot,oid,from,to,raw_next,message\n";
    rng << "tick,crt_state,crt_calls,custom_counter,custom_index,custom_calls,last_call_site\n";
    int airborne_redirects = 0;
    int relative_selections = 0;
    for (int tick = 1; tick <= 128; ++tick) {
        ntsd28::InputButtons28 buttons;
        if (airborne && tick <= 4) buttons.set(ntsd28::InputKey28::jump);
        if (dei_airborne) {
            if (tick <= 4 || (tick >= 12 && tick <= 14)) buttons.set(ntsd28::InputKey28::jump);
            if (tick >= 8 && tick <= 10) buttons.set(ntsd28::InputKey28::defend);
            if (tick >= 10 && tick <= 12) buttons.set(ntsd28::InputKey28::depth_down);
            if (tick >= 16 && tick <= 18) buttons.set(ntsd28::InputKey28::attack);
        }
        session.set_input(0, buttons);
        session.set_input(1, ntsd28::InputButtons28{});
        session.step();
        if (!recorder.capture_after_step(session, error)) { std::cerr << error; return 7; }
        const auto* world = session.world();
        for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            const auto* entity = world->entity(slot);
            if (!entity) continue;
            const auto* frame = entity->definition->frame(entity->frame.action);
            csv << world->sequence() << ',' << slot << ',' << entity->object_id << ','
                << entity->object_type << ',' << entity->frame.action << ','
                << (frame ? frame->values.integer("state").value_or(0) : -1) << ','
                << entity->frame.frame_counter << ',' << entity->position.x << ','
                << entity->position.y << ',' << entity->position.z << ','
                << entity->motion.x << ',' << entity->motion.y << ',' << entity->motion.z << ','
                << entity->current_hp << ',' << entity->owner_slot << ',' << entity->battle_group << '\n';
        }
        for (const auto& event : session.last_tick()->frames) {
            const auto& frame = event.frame;
            frames << world->sequence() << ',' << event.slot << ',' << event.object_id << ','
                   << frame.from_action << ',' << frame.to_action << ',' << frame.raw_next
                   << ',' << frame.message << '\n';
            if (frame.from_action != 212 && frame.to_action == 212)
                ++airborne_redirects;
            if (frame.raw_next >= 1300 && frame.raw_next < 1400) ++relative_selections;
        }
        const auto state = world->random().state();
        rng << world->sequence() << ',' << state.crt_state << ',' << state.crt_calls << ','
            << state.synchronized.counter << ',' << state.synchronized.index << ','
            << state.synchronized.calls << ',' << state.synchronized.last_call_site << '\n';
    }
    csv.close(); frames.close(); rng.close();
    if (!csv || !frames || !rng) return 8;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) { std::cerr << error; return 9; }
    std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 10;
    std::cout << "mode=" << mode << " ticks=128 airborne_redirects=" << airborne_redirects
              << " relative_selections=" << relative_selections << '\n';
    return 0;
}
