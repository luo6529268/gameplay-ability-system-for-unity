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

    ntsd28_playable::CombatantConfig28 victim;
    victim.slot = 0;
    victim.object_id = 2;
    victim.x = 200;
    victim.z = 542;
    victim.hp = initial_hp;
    victim.mp = 300;
    victim.team = 1;
    victim.action = 0;
    ntsd28_playable::CombatantConfig28 credit = victim;
    credit.slot = 1;
    credit.x = 1200;
    credit.hp = 500;
    credit.team = 2;
    config.combatants = {victim, credit};

    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(config, error)) {
        throw std::runtime_error("GameSession initialize: " + error);
    }
    auto* world = session.world();
    auto* active = world == nullptr ? nullptr : world->entity(0);
    auto* owner = world == nullptr ? nullptr : world->entity(1);
    if (active == nullptr || owner == nullptr ||
        active->definition->frame(0) == nullptr) {
        throw std::runtime_error("indexed Naruto roster gate failed");
    }
    active->current_hp = initial_hp;
    active->effective_max_hp = initial_hp;
    active->environment_state_320 = -1;
    active->catch_source_slot_90 = 1;
    active->impact_source_slot_164 = -1;

    if (std::filesystem::exists(output)) {
        throw std::runtime_error("refusing to overwrite source output");
    }
    std::ofstream rows(output, std::ios::binary);
    if (!rows) throw std::runtime_error("cannot open source output");
    rows << "tick,derived_phase12,initial_hp,action,hp,effective_hp,"
            "environment_state,credit_score,credit_knockouts,event_count,"
            "event_time,event_type,event_victim,event_source,event_credit,"
            "event_four_owner,result_timer\n";
    bool saw_expected = false;
    for (int tick = 1; tick <= 13; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        active = world->entity(0);
        owner = world->entity(1);
        if (active == nullptr || owner == nullptr) {
            throw std::runtime_error("combatant disappeared during probe");
        }
        const auto& events = world->knockout_events();
        const auto* event = events.empty() ? nullptr : &events.back();
        rows << tick << ',' << (tick % 12) << ',' << initial_hp << ','
             << active->frame.action << ',' << active->current_hp << ','
             << active->effective_max_hp << ','
             << active->environment_state_320 << ','
             << owner->input_score_total_348 << ','
             << owner->knockout_count_358 << ',' << events.size() << ','
             << (event == nullptr ? -1 : static_cast<int>(event->battle_time_ticks))
             << ',' << (event == nullptr ? -1 : event->source_object_type)
             << ',' << (event == nullptr ? -1 : static_cast<int>(event->victim_slot))
             << ',' << (event == nullptr ? -1 : static_cast<int>(event->source_slot))
             << ',' << (event == nullptr ? -1 : static_cast<int>(event->credit_slot))
             << ',' << (event == nullptr ? -1 : static_cast<int>(event->four_owner_slot))
             << ',' << session.battle_flow().timer << '\n';
        if (tick == 12) {
            saw_expected = initial_hp == 5
                ? events.size() == 1 && owner->knockout_count_358 == 1 &&
                      event->victim_slot == 0 && event->source_slot == 1000 &&
                      event->credit_slot == 1 && event->four_owner_slot == 1000
                : events.empty() && owner->knockout_count_358 == 0;
        }
    }
    if (!rows) throw std::runtime_error("source output write failed");
    return saw_expected;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) {
        std::cerr << "usage: negative_environment_ko_full_tick_source_probe "
                     "<formal_runtime_root> <new_output_dir>\n";
        return 2;
    }
    try {
        const std::filesystem::path root(argv[1]);
        const std::filesystem::path output(argv[2]);
        std::filesystem::create_directories(output);
        const bool positive = run_case(root, output / "positive.csv", 5);
        const bool control = run_case(root, output / "control.csv", 50);
        std::cout << "positive=" << positive << " control=" << control << '\n';
        return positive && control ? 0 : 3;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 4;
    }
}
