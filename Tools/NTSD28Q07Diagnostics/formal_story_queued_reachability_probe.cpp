#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <cwchar>

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: formal_story_queued_reachability_probe <formal_root> <runtime_root> <output_csv>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 1;
    config.story_mission_id = 1;
    config.story_child_stage_id = 5;
    ntsd28_playable::CombatantConfig28 player;
    player.slot = 0;
    player.object_id = 11;
    player.x = 500;
    player.z = 350;
    player.hp = 500;
    player.mp = 500;
    player.team = 1;
    config.combatants = {player};

    ntsd28_playable::GameSession28 session(argv[1], argv[2]);
    std::string error;
    if (!session.initialize(config, error)) {
        std::cerr << "initialize: " << error << '\n';
        return 4;
    }
    std::ofstream rows(output, std::ios::binary);
    if (!rows) return 5;
    rows << "tick,story_active,phase,slot,oid,hp,action,state,lives,next_hp,next_lives,group,owner,revival_status,revival_action\n";
    bool found_join_birth = false;
    bool natural_lethal = false;
    bool queued_continuation = false;
    for (int tick = 0; tick <= 300; ++tick) {
        if (tick != 0) {
            session.set_input(0, {});
            session.step();
        }
        const auto* world = session.world();
        if (world == nullptr) return 6;
        const ntsd28::EntityState28* target = nullptr;
        int target_slot = -1;
        for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            const auto* candidate = world->entity(slot);
            if (candidate == nullptr || candidate->object_id != 8) continue;
            target = candidate;
            target_slot = static_cast<int>(slot);
            break;
        }
        int status = -1;
        int selected_action = -1;
        if (tick != 0) {
            if (const auto* result = session.last_tick()) {
                for (const auto& event : result->revivals.events) {
                    if (static_cast<int>(event.slot) != target_slot) continue;
                    status = static_cast<int>(event.status);
                    selected_action = event.selected_action;
                    queued_continuation |=
                        event.status == ntsd28::WorldRevivalStatus28::continuation_armed;
                }
            }
        }
        const auto* frame = target == nullptr ? nullptr :
            target->definition->declared_frame(target->frame.action);
        const auto& story = session.native_story_phase_runtime();
        rows << tick << ',' << story.active << ',' << story.phase_index_49fb6c
             << ',' << target_slot << ',' << (target == nullptr ? -1 : target->object_id)
             << ',' << (target == nullptr ? -1 : target->current_hp)
             << ',' << (target == nullptr ? -1 : target->frame.action)
             << ',' << (frame == nullptr ? -1 : frame->values.integer("state").value_or(-1))
             << ',' << (target == nullptr ? -1 : target->revive_lives_30c)
             << ',' << (target == nullptr ? -1 : target->revive_next_hp_314)
             << ',' << (target == nullptr ? -1 : target->revive_next_lives_310)
             << ',' << (target == nullptr ? -1 : target->battle_group)
             << ',' << (target == nullptr ? -1 : target->owner_slot)
             << ',' << status << ',' << selected_action << '\n';
        found_join_birth |= target != nullptr && target->revive_next_hp_314 == 500;
        natural_lethal |= target != nullptr && target->current_hp <= 0;
    }
    rows.close();
    if (!rows) return 7;
    std::cout << "rows=301 join_birth=" << found_join_birth
              << " natural_lethal=" << natural_lethal
              << " queued_continuation=" << queued_continuation << '\n';
    return found_join_birth ? 0 : 8;
}
