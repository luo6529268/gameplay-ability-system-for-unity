#include "ntsd28/battle_world.h"
#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>

namespace {

bool run_case(const std::filesystem::path& root,
              const std::filesystem::path& output,
              int initial_hp) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 0;
    config.character_id = 2;
    config.enemy_id = 2;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;

    ntsd28_playable::CombatantConfig28 catcher;
    catcher.slot = 0;
    catcher.object_id = 2;
    catcher.x = 200;
    catcher.z = 542;
    catcher.hp = 500;
    catcher.mp = 300;
    catcher.team = 1;
    catcher.action = 260;
    ntsd28_playable::CombatantConfig28 caught = catcher;
    caught.slot = 1;
    caught.x = 220;
    caught.hp = initial_hp;
    caught.team = 2;
    caught.action = 132;
    config.combatants = {catcher, caught};

    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(config, error)) {
        throw std::runtime_error("GameSession initialize: " + error);
    }
    auto* world = session.world();
    auto* actor = world == nullptr ? nullptr : world->entity(0);
    auto* victim = world == nullptr ? nullptr : world->entity(1);
    if (actor == nullptr || victim == nullptr ||
        actor->definition->frame(260) == nullptr ||
        victim->definition->frame(132) == nullptr) {
        throw std::runtime_error("indexed Naruto held-CPoint roster gate failed");
    }
    actor->catch_target_slot_8c = 1;
    actor->catch_timeout_94 = 100;
    actor->frame.frame_counter = 0;
    victim->catch_source_slot_90 = 0;
    victim->current_hp = initial_hp;
    victim->effective_max_hp = initial_hp;
    victim->incoming_damage_scale_340 = 100;
    victim->ordinary_credit_gate_2f4 = -1;

    if (std::filesystem::exists(output)) {
        throw std::runtime_error("refusing to overwrite source output");
    }
    std::ofstream rows(output, std::ios::binary);
    if (!rows) throw std::runtime_error("cannot open source output");
    rows << "tick,initial_hp,catcher_action,victim_action,victim_hp,"
            "victim_effective_hp,catch_target,catch_source,catcher_counter,"
            "catch_timeout,catcher_score,catcher_knockouts,event_count,"
            "event_time,event_type,event_victim,event_source,event_credit,"
            "event_four_owner,result_timer\n";
    bool saw_expected = false;
    for (int tick = 1; tick <= 3; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        actor = world->entity(0);
        victim = world->entity(1);
        if (actor == nullptr || victim == nullptr) {
            throw std::runtime_error("combatant disappeared during probe");
        }
        const auto& events = world->knockout_events();
        const auto* event = events.empty() ? nullptr : &events.back();
        rows << tick << ',' << initial_hp << ',' << actor->frame.action << ','
             << victim->frame.action << ',' << victim->current_hp << ','
             << victim->effective_max_hp << ',' << actor->catch_target_slot_8c
             << ',' << victim->catch_source_slot_90 << ','
             << actor->frame.frame_counter << ',' << actor->catch_timeout_94
             << ',' << actor->input_score_total_348 << ','
             << actor->knockout_count_358 << ',' << events.size() << ','
             << (event == nullptr ? -1 : static_cast<int>(event->battle_time_ticks))
             << ',' << (event == nullptr ? -1 : event->source_object_type)
             << ',' << (event == nullptr ? -1 : static_cast<int>(event->victim_slot))
             << ',' << (event == nullptr ? -1 : static_cast<int>(event->source_slot))
             << ',' << (event == nullptr ? -1 : static_cast<int>(event->credit_slot))
             << ',' << (event == nullptr ? -1 : static_cast<int>(event->four_owner_slot))
             << ',' << session.battle_flow().timer << '\n';
        if (tick == 1) {
            saw_expected = initial_hp == 30
                ? events.size() == 1 && actor->knockout_count_358 == 1 &&
                      event->battle_time_ticks == 0 && event->victim_slot == 1 &&
                      event->source_slot == 0 && event->credit_slot == 0
                : events.empty() && actor->knockout_count_358 == 0;
        }
    }
    if (!rows) throw std::runtime_error("source output write failed");
    return saw_expected;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) {
        std::cerr << "usage: held_cpoint_ko_full_tick_source_probe "
                     "<formal_runtime_root> <new_output_dir>\n";
        return 2;
    }
    try {
        const std::filesystem::path root(argv[1]);
        const std::filesystem::path output(argv[2]);
        std::filesystem::create_directories(output);
        const bool positive = run_case(root, output / "positive.csv", 30);
        const bool control = run_case(root, output / "control.csv", 100);
        std::cout << "positive=" << positive << " control=" << control << '\n';
        return positive && control ? 0 : 3;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 4;
    }
}
