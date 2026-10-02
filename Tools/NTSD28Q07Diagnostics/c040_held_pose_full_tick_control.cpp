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
    std::ofstream csv(output / "full-tick.csv", std::ios::binary);
    if (!csv) return 4;
    csv << "initial_action,initial_hold,catcher_action_before,caught_action_before,"
           "caught_hold_before,catcher_x_before,caught_x_before,caught_y_before,"
           "catcher_action_after,caught_action_after,caught_hold_after,"
           "catcher_x_after,caught_x_after,caught_y_after,caught_z_after,"
           "catch_target_after,catch_source_after,settlement_active,"
           "settlement_synchronized,current_center_y,vaction_center_y,"
           "current_cpoint_x,vaction_cpoint_x,split_reached\n";

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
            const int catcher_action_before = catcher->frame.action;
            const int caught_action_before = caught->frame.action;
            const int caught_hold_before = caught->motion_hold_timer;
            const int catcher_x_before = catcher->position.x;
            const int caught_x_before = caught->position.x;
            const int caught_y_before = caught->position.y;

            session.set_input(0, ntsd28::InputButtons28{});
            session.set_input(1, ntsd28::InputButtons28{});
            session.step();
            const auto* tick = session.last_tick();
            catcher = world->entity(0);
            caught = world->entity(1);
            if (!tick || !catcher || !caught) return 7;
            const auto* catcher_frame = catcher->definition->frame(catcher->frame.action);
            const auto* caught_frame = caught->definition->frame(caught->frame.action);
            const auto* catcher_block = catcher_frame == nullptr
                                            ? nullptr : catcher_frame->first_block("cpoint");
            const auto* caught_block = caught_frame == nullptr
                                           ? nullptr : caught_frame->first_block("cpoint");
            int vaction = -1;
            int current_cpoint_x = 0;
            int vaction_cpoint_x = 0;
            int current_center_y = 0;
            int vaction_center_y = 0;
            bool different_points = false;
            bool different_centers = false;
            if (catcher_block != nullptr && caught_block != nullptr) {
                const auto catcher_point = ntsd28::CombatRecordDecoder28::catch_point(
                    *catcher_block);
                const auto current_point = ntsd28::CombatRecordDecoder28::catch_point(
                    *caught_block);
                vaction = catcher_point.victim_action;
                const auto* vaction_frame = caught->definition->frame(vaction);
                const auto* vaction_block = vaction_frame == nullptr
                                                ? nullptr : vaction_frame->first_block("cpoint");
                if (catcher_point.kind == 1 && current_point.kind == 2 &&
                    vaction_block != nullptr) {
                    const auto position_point =
                        ntsd28::CombatRecordDecoder28::catch_point(*vaction_block);
                    if (position_point.kind == 2) {
                        current_cpoint_x = current_point.x;
                        vaction_cpoint_x = position_point.x;
                        current_center_y = caught_frame->values.integer("centery").value_or(0);
                        vaction_center_y = vaction_frame->values.integer("centery").value_or(0);
                        different_points = current_cpoint_x != vaction_cpoint_x;
                        different_centers = current_center_y != vaction_center_y;
                    }
                }
            }
            const bool split_reached = tick->catch_settlement.active_relations != 0 &&
                tick->catch_settlement.synchronized_targets != 0 &&
                caught->motion_hold_timer > 0 && caught->frame.action != vaction &&
                (different_points || different_centers);
            csv << caught_action << ',' << hold << ',' << catcher_action_before << ','
                << caught_action_before << ',' << caught_hold_before << ','
                << catcher_x_before << ',' << caught_x_before << ',' << caught_y_before << ','
                << catcher->frame.action << ',' << caught->frame.action << ','
                << caught->motion_hold_timer << ',' << catcher->position.x << ','
                << caught->position.x << ',' << caught->position.y << ','
                << caught->position.z << ',' << catcher->catch_target_slot_8c << ','
                << caught->catch_source_slot_90 << ','
                << tick->catch_settlement.active_relations << ','
                << tick->catch_settlement.synchronized_targets << ','
                << current_center_y << ',' << vaction_center_y << ','
                << current_cpoint_x << ',' << vaction_cpoint_x << ','
                << (split_reached ? 1 : 0) << '\n';
        }
    }
    csv.close();
    return csv ? 0 : 8;
}
