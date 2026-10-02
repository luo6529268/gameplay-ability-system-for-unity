#include "ntsd28_playable/game_session.h"
#include "ntsd28/combat_records.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>

int main(int argc, char** argv) {
    if (argc != 3) return 2;
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);
    std::ofstream csv(output / "held-pose.csv", std::ios::binary);
    if (!csv) return 4;
    csv << "hold_before,catcher_action,caught_action_before,caught_action_after,"
           "current_center_x,current_center_y,current_cpoint_x,current_cpoint_y,"
           "vaction_cpoint_x,vaction_cpoint_y,expected_x,expected_y,expected_z,"
           "actual_x,actual_y,actual_z,active_relations,synchronized_targets,success\n";

    const auto runtime = root / "resources" / "runtime";
    for (int caught_action : {132, 137}) {
    for (int hold : {5, 0}) {
        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 682973786u;
        config.character_id = 41;
        config.enemy_id = 75;
        config.background_id = 1;
        config.bgm_selection_49f18c = 2;
        config.battle_mode = 0;
        ntsd28_playable::CombatantConfig28 catcher_config;
        catcher_config.slot = 0;
        catcher_config.object_id = 41;
        catcher_config.x = 500;
        catcher_config.y = 0;
        catcher_config.z = 400;
        catcher_config.hp = catcher_config.base_hp = catcher_config.mp = 500;
        catcher_config.team = 1;
        catcher_config.action = 125;
        ntsd28_playable::CombatantConfig28 caught_config = catcher_config;
        caught_config.slot = 1;
        caught_config.object_id = 75;
        caught_config.x = 550;
        caught_config.action = caught_action;
        caught_config.team = 2;
        config.combatants = {catcher_config, caught_config};
        ntsd28_playable::GameSession28 session(runtime, runtime);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << error << '\n';
            return 5;
        }
        auto* world = session.world();
        auto* catcher = world->entity(0);
        auto* caught = world->entity(1);
        if (!catcher || !caught) return 6;
        catcher->frame.action = 125;
        catcher->frame.facing = false;
        caught->frame.action = caught_action;
        caught->frame.facing = false;
        catcher->catch_target_slot_8c = 1;
        caught->catch_source_slot_90 = 0;
        caught->motion_hold_timer = hold;

        const auto* catcher_frame = catcher->definition->frame(125);
        const auto* current_frame = caught->definition->frame(caught_action);
        if (!catcher_frame || !current_frame) return 7;
        const auto* catcher_block = catcher_frame->first_block("cpoint");
        const auto* current_block = current_frame->first_block("cpoint");
        if (!catcher_block || !current_block) return 8;
        const auto catcher_point = ntsd28::CombatRecordDecoder28::catch_point(*catcher_block);
        const auto current_point = ntsd28::CombatRecordDecoder28::catch_point(*current_block);
        const auto* position_frame = caught->definition->frame(catcher_point.victim_action);
        if (!position_frame) return 9;
        const auto* position_block = position_frame->first_block("cpoint");
        if (!position_block) return 10;
        const auto position_point = ntsd28::CombatRecordDecoder28::catch_point(*position_block);
        if (catcher_point.kind != 1 || catcher_point.hurtable != 1 ||
            current_point.kind != 2 || position_point.kind != 2) return 11;
        const int catcher_center_x = catcher_frame->values.integer("centerx").value_or(0);
        const int catcher_center_y = catcher_frame->values.integer("centery").value_or(0);
        const int caught_center_x = (hold == 0 ? position_frame : current_frame)
                                        ->values.integer("centerx").value_or(0);
        const int caught_center_y = (hold == 0 ? position_frame : current_frame)
                                        ->values.integer("centery").value_or(0);
        const int catch_x = catcher->position.x - catcher_center_x + catcher_point.x;
        const int catch_y = catcher->position.y - catcher_center_y + catcher_point.y;
        const int expected_x = caught_center_x - position_point.x + catch_x;
        int expected_y = caught_center_y - position_point.y + catch_y;
        int expected_z = catcher->position.z;
        if (catcher_point.z == 0) {
            if (catcher_point.cover % 10 == 0) {
                --expected_z;
                ++expected_y;
            } else {
                ++expected_z;
                --expected_y;
            }
        } else {
            expected_z += catcher_point.z;
            ++expected_y;
        }

        const auto pass = world->settle_catch_relations();
        csv << hold << ',' << catcher->frame.action << ',' << caught_action << ','
            << caught->frame.action << ',' << caught_center_x << ',' << caught_center_y
            << ',' << current_point.x << ',' << current_point.y << ','
            << position_point.x << ',' << position_point.y << ','
            << expected_x << ',' << expected_y << ',' << expected_z << ','
            << caught->position.x << ',' << caught->position.y << ','
            << caught->position.z << ',' << pass.active_relations << ','
            << pass.synchronized_targets << ',' << (pass.success ? 1 : 0) << '\n';
        if (!pass.success || pass.active_relations != 1 ||
            pass.synchronized_targets != 1 ||
            caught->frame.action != (hold == 0 ? catcher_point.victim_action : caught_action) ||
            caught->position.x != expected_x ||
            caught->position.y != expected_y ||
            caught->position.z != expected_z) {
            for (const auto& diagnostic : pass.diagnostics) std::cerr << diagnostic << '\n';
            return 12;
        }
    }
    }
    csv.close();
    return csv ? 0 : 13;
}
