#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <stdexcept>

namespace {

ntsd28_playable::BattleConfig28 make_config(int peer_x) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 0;
    config.character_id = 7;
    config.enemy_id = 7;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    ntsd28_playable::CombatantConfig28 dead;
    dead.slot = 0;
    dead.object_id = 7;
    dead.x = 50;
    dead.z = 600;
    dead.hp = 1;
    dead.base_hp = 500;
    dead.mp = 77;
    dead.team = 1;
    dead.action = 230;
    auto peer = dead;
    peer.slot = 1;
    peer.x = peer_x;
    peer.hp = 500;
    peer.mp = 500;
    peer.action = 0;
    config.combatants = {dead, peer};
    return config;
}

void verify_catalog(const std::filesystem::path& root, std::ostream& output) {
    ntsd28::ObjectDefinitionCatalog28 catalog;
    const auto loaded = catalog.load_extracted_root(root);
    if (!loaded.success) throw std::runtime_error("formal indexed catalog failed");
    const auto* lee = catalog.find(7);
    if (!lee || !lee->definition || !lee->definition->ok() || lee->object_type != 0)
        throw std::runtime_error("indexed Lee character definition missing");
    const auto* lying = lee->definition->declared_frame(230);
    if (!lying || lying->values.integer("state").value_or(-1) != 14)
        throw std::runtime_error("indexed Lee action230/state14 gate failed");
    output << "catalog_entries=" << catalog.size() << '\n'
           << "oid=7 registry_index=" << lee->registry_index << " source="
           << lee->source_path << " readable=" << lee->readable_dat_path.u8string()
           << " action230_state=14\n";
}

bool run_case(const std::filesystem::path& root, const std::filesystem::path& dir,
              const char* name, int peer_x) {
    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(make_config(peer_x), error))
        throw std::runtime_error(error);
    auto* world = session.world();
    if (!world || !world->entity(0) || !world->entity(1))
        throw std::runtime_error("formal roster missing");
    auto* dead = world->entity(0);
    dead->current_hp = 0;
    dead->effective_max_hp = 500;
    dead->base_max_hp = 500;
    dead->revive_lives_30c = 2;
    dead->revive_next_hp_314 = 0;
    dead->render_phase_008 = 2;
    dead->motion_hold_timer = 3;
    dead->frame.frame_counter = 0;

    std::ofstream rows(dir / (std::string(name) + ".csv"));
    std::ofstream log(dir / (std::string(name) + "-diagnostics.txt"));
    if (!rows || !log) throw std::runtime_error("output open failed");
    rows << std::setprecision(17);
    rows << "tick,sequence,slot,active,oid,action,state,hp,base_hp,effective_hp,mp,lives,render_phase,hold_timer,frame_counter,x,y,z,precise_x,precise_y,precise_z,vx,vy,vz,active_count,crt_state,crt_calls,sync_counter,sync_index,sync_calls,sync_last_site,sync_table_hash\n";
    bool revived = false;
    for (int step = 0; step <= 3; ++step) {
        if (step) {
            session.set_input(0, {});
            session.set_input(1, {});
            session.step();
        }
        const auto rng = world->random().state();
        for (int slot = 0; slot < 2; ++slot) {
            const auto* entity = world->entity(slot);
            rows << step << ',' << world->sequence() << ',' << slot << ','
                 << (entity != nullptr);
            if (entity) {
                const auto* frame = entity->definition->declared_frame(entity->frame.action);
                rows << ',' << entity->object_id << ',' << entity->frame.action << ','
                     << (frame ? frame->values.integer("state").value_or(-1) : -1)
                     << ',' << entity->current_hp << ',' << entity->base_max_hp
                     << ',' << entity->effective_max_hp << ',' << entity->current_mp
                     << ',' << entity->revive_lives_30c << ',' << entity->render_phase_008
                     << ',' << entity->motion_hold_timer << ',' << entity->frame.frame_counter
                     << ',' << entity->position.x << ',' << entity->position.y
                     << ',' << entity->position.z << ',' << entity->position.precise_x
                     << ',' << entity->position.precise_y << ',' << entity->position.precise_z
                     << ',' << entity->motion.x << ',' << entity->motion.y
                     << ',' << entity->motion.z;
            } else {
                for (int column = 0; column < 20; ++column) rows << ',';
            }
            rows << ',' << world->active_count() << ',' << rng.crt_state << ','
                 << rng.crt_calls << ',' << rng.synchronized.counter << ','
                 << rng.synchronized.index << ',' << rng.synchronized.calls << ','
                 << rng.synchronized.last_call_site << ','
                 << world->random().synchronized_table_hash() << '\n';
        }
        const auto& flow = session.battle_flow();
        log << "step=" << step << " sequence=" << world->sequence()
            << " phase=" << static_cast<int>(flow.phase) << " timer=" << flow.timer
            << " transition=" << flow.transition_state << '\n';
        if (const auto* tick = session.last_tick()) {
            log << "last_tick=" << tick->sequence << " revival_success="
                << tick->revivals.success << " revived=" << tick->revivals.revived
                << " deferred=" << tick->revivals.deferred << '\n';
            for (const auto& event : tick->revivals.events) {
                log << "revival_event=" << static_cast<int>(event.status) << ','
                    << event.slot << ',' << event.lives_before << ','
                    << event.lives_after << ',' << event.selected_action << ','
                    << event.message << '\n';
                revived |= event.status == ntsd28::WorldRevivalStatus28::revived;
            }
            for (const auto& message : tick->diagnostics.messages)
                log << "tick_message=" << message << '\n';
        }
    }
    if (!rows || !log) throw std::runtime_error("output write failed");
    return revived;
}

} // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) return 2;
    try {
        const std::filesystem::path root(argv[1]), output(argv[2]);
        std::filesystem::create_directories(output);
        if (std::filesystem::exists(output / "positive.csv") ||
            std::filesystem::exists(output / "negative.csv"))
            throw std::runtime_error("refusing to overwrite existing raw traces");
        std::ofstream identity(output / "catalog-identity.txt");
        verify_catalog(root, identity);
        const bool positive = run_case(root, output, "positive", 100);
        const bool negative = run_case(root, output, "negative", 0);
        std::cout << "positive_revived=" << positive
                  << " negative_revived=" << negative << '\n';
        return positive && negative ? 0 : 3;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 4;
    }
}
