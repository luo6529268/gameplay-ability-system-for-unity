#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>

int main(int argc, char** argv) {
    if (argc != 5) {
        std::cerr << "usage: han_earthquake_session_trace <decoded_dat> <complete_vfs> <output_tsv> <149|150|264|264-air|147-near>\n";
        return 2;
    }
    const std::string start_action(argv[4]);
    if (start_action != "149" && start_action != "150" &&
        start_action != "264" && start_action != "264-air" &&
        start_action != "147-near") {
        std::cerr << "unsupported controlled Han initial action\n";
        return 2;
    }
    const bool near_catch = start_action == "147-near";
    const int initial_action = near_catch ? 147
                               : start_action == "149" ? 149
                               : start_action == "150" ? 150 : 264;
    const int quake_action = initial_action == 264 ? 265 : 150;
    const int reset_action = initial_action == 264 ? 266 : 151;

    const std::filesystem::path output(argv[3]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to overwrite existing output\n";
        return 3;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 2833;
    config.background_id = 23;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 han;
    han.slot = 0;
    han.object_id = 726;
    han.x = 500;
    han.y = start_action == "264-air" ? -20 : 0;
    han.z = 650;
    han.hp = 500;
    han.mp = 500;
    han.team = 1;
    han.action = initial_action;

    ntsd28_playable::CombatantConfig28 opponent;
    opponent.slot = 1;
    opponent.object_id = 7;
    opponent.x = near_catch ? 520 : 1200;
    opponent.z = 650;
    opponent.hp = 500;
    opponent.mp = 500;
    opponent.team = 2;
    config.combatants = {han, opponent};

    ntsd28_playable::GameSession28 session(argv[1], argv[2]);
    std::string error;
    if (!session.initialize(config, error)) {
        std::cerr << "formal GameSession initialization failed: " << error << '\n';
        return 4;
    }

    std::filesystem::create_directories(output.parent_path());
    std::ofstream rows(output, std::ios::binary);
    if (!rows) return 5;
    rows << "tick\tactor_oid\taction\tstate\tactor_x\tactor_y\tactor_vy\tcamera_x\tearthquake_owner\tbackground_offset_x\tbackground_offset_y\ttarget_action\ttarget_x\than_catch_target_slot\ttarget_catch_source_slot\n";

    const auto* initial_world = session.world();
    const auto* initial_actor = initial_world == nullptr ? nullptr : initial_world->entity(0);
    const auto* initial_target = initial_world == nullptr ? nullptr : initial_world->entity(1);
    if (initial_actor == nullptr || initial_target == nullptr) return 6;
    const auto* initial_frame = initial_actor->definition->frame(initial_actor->frame.action);
    const int initial_state = initial_frame == nullptr
                                  ? 0
                                  : initial_frame->values.integer("state").value_or(0);
    const auto initial_snapshot = session.snapshot(false);
    rows << 0 << '\t' << initial_actor->object_id << '\t'
         << initial_actor->frame.action << '\t' << initial_state << '\t'
         << initial_actor->position.x << '\t' << initial_actor->position.y
         << '\t' << initial_actor->motion.y << '\t' << session.camera_x() << '\t'
         << initial_snapshot.earthquake.owner_slot << '\t'
         << initial_snapshot.earthquake.background_offset_x << '\t'
         << initial_snapshot.earthquake.background_offset_y << '\t'
         << initial_target->frame.action << '\t' << initial_target->position.x << '\t'
         << initial_actor->catch_target_slot_8c << '\t'
         << initial_target->catch_source_slot_90 << '\n';

    bool saw_reciprocal_catch = false;
    bool saw_quake_offset = false;
    bool saw_reset = false;
    bool saw_quake_before_reset = false;
    for (int tick = 1; tick <= 24; ++tick) {
        session.set_input(0, {});
        session.set_input(1, {});
        session.step();
        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        const auto* target = world == nullptr ? nullptr : world->entity(1);
        if (actor == nullptr || target == nullptr || session.last_tick() == nullptr) {
            std::cerr << "Han, target or completed tick disappeared at tick " << tick << '\n';
            return 6;
        }

        const auto* frame = actor->definition->frame(actor->frame.action);
        const int state = frame == nullptr
                              ? 0
                              : frame->values.integer("state").value_or(0);
        const auto snapshot = session.snapshot(false);
        const auto& quake = snapshot.earthquake;
        rows << tick << '\t' << actor->object_id << '\t'
             << actor->frame.action << '\t' << state << '\t'
             << actor->position.x << '\t' << actor->position.y << '\t'
             << actor->motion.y << '\t' << session.camera_x() << '\t'
             << quake.owner_slot << '\t' << quake.background_offset_x << '\t'
             << quake.background_offset_y << '\t' << target->frame.action << '\t'
             << target->position.x << '\t' << actor->catch_target_slot_8c << '\t'
             << target->catch_source_slot_90 << '\n';

        if (actor->frame.action == 149 &&
            (target->frame.action == 130 || target->frame.action == 131) &&
            actor->catch_target_slot_8c == 1 &&
            target->catch_source_slot_90 == 0) {
            saw_reciprocal_catch = true;
        }

        if (actor->frame.action == quake_action && state == 55052 &&
            quake.background_offset_x == 2 &&
            quake.background_offset_y == 0 && quake.owner_slot == 0) {
            saw_quake_offset = true;
        }
        if (actor->frame.action == reset_action && state == 55050 &&
            quake.background_offset_x == 0 &&
            quake.background_offset_y == 0 && quake.owner_slot == 0) {
            saw_reset = true;
            saw_quake_before_reset = saw_quake_offset;
        }
    }
    rows.close();
    if (!rows) return 7;
    if ((near_catch && !saw_reciprocal_catch) || !saw_quake_offset ||
        !saw_reset || !saw_quake_before_reset) {
        std::cerr << "formal Han quake(+2,0) -> reset(0,0) sequence not observed from action "
                  << initial_action << "; reciprocal catch observed="
                  << saw_reciprocal_catch << "; rows preserved\n";
        return 8;
    }
    std::cout << "formal Han controlled mid-combo earthquake sequence observed\n";
    return 0;
}
