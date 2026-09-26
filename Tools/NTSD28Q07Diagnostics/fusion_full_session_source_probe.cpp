#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <stdexcept>

namespace {

ntsd28_playable::BattleConfig28 make_config(int primary_x, bool partner_facing) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 0;
    config.character_id = 7;
    config.enemy_id = 8;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    ntsd28_playable::CombatantConfig28 primary;
    primary.slot = 0;
    primary.object_id = 7;
    primary.x = primary_x;
    primary.z = 600;
    primary.hp = 100;
    primary.base_hp = 500;
    primary.mp = 500;
    primary.team = 1;
    primary.action = 9;
    auto partner = primary;
    partner.slot = 1;
    partner.object_id = 8;
    partner.x = 300;
    partner.facing = partner_facing;
    config.combatants = {primary, partner};
    return config;
}

void verify_catalog(const std::filesystem::path& root, std::ostream& log) {
    ntsd28::ObjectDefinitionCatalog28 catalog;
    const auto loaded = catalog.load_extracted_root(root);
    if (!loaded.success) throw std::runtime_error("formal indexed catalog failed");
    log << "catalog_entries=" << catalog.size() << '\n';
    for (int oid : {7, 8, 51}) {
        const auto* entry = catalog.find(oid);
        if (!entry || !entry->definition || !entry->definition->ok())
            throw std::runtime_error("formal indexed definition unavailable");
        log << "oid=" << oid << " registry_index=" << entry->registry_index
            << " type=" << entry->object_type << " source=" << entry->source_path
            << " readable=" << entry->readable_dat_path.u8string() << '\n';
        const int action = oid == 51 ? 290 : 9;
        const auto* frame = entry->definition->declared_frame(action);
        if (!frame || (oid != 51 && frame->values.integer("state") != 2))
            throw std::runtime_error("formal action/state gate failed");
    }
    const auto fusion = ntsd28::FusionCatalog28::load_file(root / "decoded_dat/data/fusion.dat");
    if (!fusion.ok() || fusion.records().empty()) throw std::runtime_error("fusion DAT failed");
    const auto& row = fusion.records().front();
    if (row.id1 != 7 || row.id2 != 8 || row.id3 != 51 || row.hp != 177 ||
        row.state != 2 || row.action != 290 || row.decrease != 4500)
        throw std::runtime_error("formal row0 identity mismatch");
    log << "fusion_row0=" << row.id1 << ',' << row.id2 << ',' << row.id3 << ','
        << row.hp << ',' << row.mp << ',' << row.respond << ',' << row.decrease
        << ',' << row.wait << ',' << row.state << ',' << row.action << ','
        << row.frame << ',' << row.chp << ',' << row.hit_ja << ',' << row.cover
        << ',' << row.front_hurt_action << ',' << row.back_hurt_action << '\n';
}

bool run_case(const std::filesystem::path& root, const std::filesystem::path& dir,
              const char* name, int primary_x, bool partner_facing = false) {
    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(make_config(primary_x, partner_facing), error)) throw std::runtime_error(error);
    auto* world = session.world();
    if (!world || !world->entity(0) || !world->entity(1)) throw std::runtime_error("roster missing");
    world->entity(0)->motion.x = 20;
    std::ofstream rows(dir / (std::string(name) + ".csv"));
    std::ofstream log(dir / (std::string(name) + "-diagnostics.txt"));
    if (!rows || !log) throw std::runtime_error("output open failed");
    rows << std::setprecision(17);
    rows << "tick,sequence,slot,active,suspended,oid,action,state,hp,mp,x,y,z,precise_x,precise_y,precise_z,vx,vy,vz,timer338,gate328,display190,partner32c,primary330,partner334,frame_counter,active_count,crt_state,crt_calls,sync_counter,sync_index,sync_calls,sync_table_hash\n";
    bool fused = false;
    for (int step = 0; step <= 3; ++step) {
        if (step) {
            session.set_input(0, {});
            session.set_input(1, {});
            session.step();
        }
        const auto rng = world->random().state();
        for (int slot = 0; slot < 2; ++slot) {
            const auto* e = world->entity(slot);
            // The public active/availability pair distinguishes retained fusion slots.
            rows << step << ',' << world->sequence() << ',' << slot << ',' << (e != nullptr)
                 << ',' << (!e && !world->slot_available_for_spawn(slot));
            if (e) {
                const auto* frame = e->definition->declared_frame(e->frame.action);
                rows << ',' << e->object_id << ',' << e->frame.action << ','
                     << (frame ? frame->values.integer("state").value_or(-1) : -1) << ','
                     << e->current_hp << ',' << e->current_mp << ',' << e->position.x << ','
                     << e->position.y << ',' << e->position.z << ',' << e->position.precise_x
                     << ',' << e->position.precise_y << ',' << e->position.precise_z << ','
                     << e->motion.x << ',' << e->motion.y << ',' << e->motion.z << ','
                     << e->input_special_timer_338 << ',' << e->input_special_gate_328 << ','
                     << e->fusion_display_timer_190 << ',' << e->fusion_partner_slot_32c << ','
                     << e->fusion_primary_definition_id_330 << ',' << e->fusion_partner_definition_id_334
                     << ',' << e->frame.frame_counter;
                fused |= e->object_id == 51;
            } else {
                // Suspended entity internals have no public accessor; empty means unobserved.
                for (int column = 0; column < 21; ++column) rows << ',';
            }
            rows << ',' << world->active_count() << ',' << rng.crt_state << ',' << rng.crt_calls
                 << ',' << rng.synchronized.counter << ',' << rng.synchronized.index << ','
                 << rng.synchronized.calls << ',' << world->random().synchronized_table_hash() << '\n';
        }
        const auto& flow = session.battle_flow();
        log << "step=" << step << " sequence=" << world->sequence() << " phase="
            << static_cast<int>(flow.phase) << " timer=" << flow.timer
            << " transition=" << flow.transition_state << '\n';
        if (const auto* tick = session.last_tick()) {
            log << "last_tick=" << tick->sequence << " fusion_success=" << tick->fusions.success
                << " fusion_source=" << tick->fusions.source_available << " fused=" << tick->fusions.fused
                << " defused=" << tick->fusions.defused << " unresolved=" << tick->fusions.unresolved << '\n';
            for (const auto& event : tick->fusions.events)
                log << "fusion_event=" << static_cast<int>(event.status) << ',' << event.primary_slot
                    << ',' << event.partner_slot << ',' << event.primary_object_id << ','
                    << event.partner_object_id << ',' << event.fused_object_id << ',' << event.message << '\n';
            for (const auto& message : tick->diagnostics.messages) log << "tick_message=" << message << '\n';
        }
        for (const auto& message : session.diagnostics()) log << "session_message=" << message << '\n';
    }
    if (!rows || !log) throw std::runtime_error("output write failed");
    return fused;
}

} // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3 && argc != 4) return 2;
    try {
        const std::filesystem::path root(argv[1]), output(argv[2]);
        if (argc == 4 && std::wstring(argv[3]) == L"opposed-v3") {
            if (std::filesystem::exists(output / "positive-v3.csv") ||
                std::filesystem::exists(output / "negative-v3.csv"))
                throw std::runtime_error("refusing to overwrite opposed traces");
            std::ofstream identity(output / "catalog-identity-v3.txt");
            verify_catalog(root, identity);
            const bool positive = run_case(root, output, "positive-v3", 304, true);
            const bool negative = run_case(root, output, "negative-v3", 315, true);
            std::cout << "positive_v3_fused=" << positive << " negative_v3_fused=" << negative << '\n';
            return positive && !negative ? 0 : 3;
        }
        if (argc == 4) {
            if (std::wstring(argv[3]) != L"negative-v2" ||
                std::filesystem::exists(output / "negative-v2.csv"))
                throw std::runtime_error("invalid correction mode or existing correction trace");
            std::ofstream identity(output / "catalog-identity-v2.txt");
            verify_catalog(root, identity);
            const bool negative = run_case(root, output, "negative-v2", 347);
            std::cout << "negative_v2_fused=" << negative << '\n';
            return negative ? 3 : 0;
        }
        if (std::filesystem::exists(output / "positive.csv") || std::filesystem::exists(output / "negative.csv")) throw std::runtime_error("refusing to overwrite trace files");
        std::filesystem::create_directories(output);
        std::ofstream identity(output / "catalog-identity.txt");
        verify_catalog(root, identity);
        const bool positive = run_case(root, output, "positive", 320);
        const bool negative = run_case(root, output, "negative", 331);
        std::cout << "positive_fused=" << positive << " negative_fused=" << negative << '\n';
        return positive && !negative ? 0 : 3;
    } catch (const std::exception& e) {
        std::cerr << e.what() << '\n';
        return 4;
    }
}
