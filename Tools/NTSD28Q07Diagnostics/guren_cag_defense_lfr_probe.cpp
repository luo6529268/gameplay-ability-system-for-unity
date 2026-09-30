#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 4) {
        std::cerr << "usage: guren_cag_defense_lfr_probe <formal_runtime_root> <new_output_dir> <lee_x>\n";
        return 2;
    }
    int lee_x = 0;
    try {
        std::size_t parsed = 0;
        lee_x = std::stoi(argv[3], &parsed);
        if (parsed != std::string(argv[3]).size() || lee_x < 540 || lee_x > 700) {
            throw std::invalid_argument("Lee X must be in 540..700");
        }
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 2;
    }
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path directory(argv[2]);
    const auto csv = directory / "source-ticks.csv";
    const auto lfr = directory / "source-packets.lfr";
    if (std::filesystem::exists(csv) || std::filesystem::exists(lfr)) {
        std::cerr << "refusing to overwrite diagnostic output\n";
        return 3;
    }
    std::filesystem::create_directories(directory);

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 84;
    config.enemy_id = 7;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 guren;
    guren.slot = 0;
    guren.object_id = 84;
    guren.x = 500;
    guren.z = 650;
    guren.hp = 500;
    guren.mp = 500;
    guren.team = 1;
    guren.action = 150;
    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 1;
    lee.object_id = 7;
    lee.x = lee_x;
    lee.z = 650;
    lee.hp = 500;
    lee.mp = 500;
    lee.team = 2;
    lee.action = 110;
    config.combatants = {guren, lee};

    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(config, error)) {
        std::cerr << "initialize: " << error << '\n';
        return 4;
    }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) {
        std::cerr << "record begin: " << error << '\n';
        return 5;
    }
    std::ofstream rows(csv, std::ios::binary);
    if (!rows) return 6;
    rows << "lee_initial_x,tick,guren_action,lee_action,lee_hp,cag_slot,cag_action,cag_x,hit_attacker,hit_target,itr_kind,itr_effect,itr_bdefend,hit_status,defense_kind\n";
    int first_cag_tick = -1;
    int first_cag_hit_tick = -1;
    for (int tick = 1; tick <= 20; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "record tick " << tick << ": " << error << '\n';
            return 7;
        }
        const auto* world = session.world();
        const auto* result = session.last_tick();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        const auto* victim = world == nullptr ? nullptr : world->entity(1);
        if (world == nullptr || result == nullptr || actor == nullptr || victim == nullptr) {
            std::cerr << "required combatant disappeared at tick " << tick << '\n';
            return 8;
        }
        int cag_slot = -1;
        int cag_action = -1;
        int cag_x = -1;
        for (std::size_t slot = 2; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            const auto* entity = world->entity(slot);
            if (entity != nullptr && entity->object_id == 619) {
                cag_slot = static_cast<int>(slot);
                cag_action = entity->frame.action;
                cag_x = entity->position.x;
                if (first_cag_tick < 0) first_cag_tick = tick;
                break;
            }
        }
        bool wrote_hit = false;
        for (const auto& hit : result->hits) {
            if (!hit.interaction || hit.target_slot != 1 ||
                hit.interaction->bdefend != 61) continue;
            rows << lee_x << ',' << tick << ',' << actor->frame.action << ','
                 << victim->frame.action << ',' << victim->current_hp << ','
                 << cag_slot << ',' << cag_action << ',' << cag_x << ','
                 << hit.attacker_slot << ',' << hit.target_slot << ','
                 << hit.interaction->kind << ',' << hit.interaction->effect << ','
                 << hit.interaction->bdefend << ','
                 << static_cast<int>(hit.status) << ','
                 << static_cast<int>(hit.defense_decision.kind) << '\n';
            if (first_cag_hit_tick < 0) first_cag_hit_tick = tick;
            wrote_hit = true;
        }
        if (!wrote_hit) {
            rows << lee_x << ',' << tick << ',' << actor->frame.action << ','
                 << victim->frame.action << ',' << victim->current_hp << ','
                 << cag_slot << ',' << cag_action << ',' << cag_x
                 << ",-1,-1,-1,-1,-1,-1,-1\n";
        }
    }
    rows.close();
    if (!rows) return 9;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << "record finish: " << error << '\n';
        return 10;
    }
    std::ofstream encoded(lfr, std::ios::binary);
    if (!encoded) return 11;
    encoded.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
    encoded.close();
    if (!encoded) return 12;
    std::cout << "lee_x=" << lee_x << " rows=" << recorder.row_count()
              << " first_cag_tick=" << first_cag_tick
              << " first_cag_hit_tick=" << first_cag_hit_tick
              << " lfr_bytes=" << bytes.size() << '\n';
    return 0;
}
