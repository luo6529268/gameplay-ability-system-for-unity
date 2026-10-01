#include "ntsd28/battle_world.h"
#include "ntsd28/dat_parser.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <memory>

namespace {
void write_state(std::ostream& rows, int timeout_initial, const char* phase,
                 const ntsd28::BattleWorld28& world,
                 const ntsd28::WorldCatchRelationPass28& pass,
                 std::size_t finalized) {
    const auto* catcher = world.entity(0);
    const auto* caught = world.entity(1);
    rows << timeout_initial << ',' << phase << ',' << pass.active_relations << ','
         << pass.timeout_changes << ',' << pass.released_relations << ','
         << pass.thrown_relations << ',' << finalized << ','
         << catcher->frame.action << ',' << caught->frame.action << ','
         << catcher->frame.frame_counter << ',' << caught->frame.frame_counter << ','
         << catcher->catch_target_slot_8c << ',' << caught->catch_source_slot_90 << ','
         << catcher->catch_timeout_94 << ','
         << catcher->pending_hit_impulse.contribution_count << ','
         << caught->pending_hit_impulse.contribution_count << ','
         << caught->pending_hit_impulse.total.x << ','
         << caught->pending_hit_impulse.total.y << ','
         << caught->motion.x << ',' << caught->motion.y << '\n';
}
}

int main(int argc, char** argv) {
    if (argc != 3) return 2;
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) return 3;
    std::filesystem::create_directories(output);

    ntsd28::DatParser parser;
    const auto gaa_document = parser.parse_file(root / "c/gaa/gaa.dat");
    const auto naruto_document = parser.parse_file(root / "c/nar/nar.dat");
    if (!gaa_document.ok() || !naruto_document.ok() ||
        gaa_document.frame(128) == nullptr ||
        naruto_document.frame(130) == nullptr ||
        naruto_document.frame(180) == nullptr ||
        naruto_document.frame(181) == nullptr) return 4;
    const auto gaa = std::make_shared<const ntsd28::DatDocument>(gaa_document);
    const auto naruto = std::make_shared<const ntsd28::DatDocument>(naruto_document);
    std::ofstream rows(output / "source-pass.csv", std::ios::binary);
    if (!rows) return 5;
    rows << "initial_timeout,phase,active_relations,timeout_changes,released_relations,"
            "thrown_relations,finalized,catcher_action,caught_action,catcher_counter,"
            "caught_counter,catch_target,catch_source,timeout_after,catcher_pending_count,"
            "caught_pending_count,caught_pending_x,caught_pending_y,caught_motion_x,"
            "caught_motion_y\n" << std::setprecision(17);

    for (const int initial_timeout : {2, 3, 4}) {
        ntsd28::BattleWorld28 world;
        ntsd28::SpawnRequest28 catcher;
        catcher.object_id = 16;
        catcher.object_type = 0;
        catcher.definition = gaa;
        catcher.initial_action = 128;
        catcher.position.x = 100;
        catcher.position.z = 200;
        catcher.hp = 500;
        catcher.mp = 500;
        ntsd28::SpawnRequest28 caught = catcher;
        caught.object_id = 2;
        caught.definition = naruto;
        caught.initial_action = 130;
        caught.position.x = 90;
        if (!world.spawn_at(0, catcher).success || !world.spawn_at(1, caught).success)
            return 6;
        auto* first = world.entity(0);
        auto* second = world.entity(1);
        first->catch_target_slot_8c = 1;
        first->catch_timeout_94 = initial_timeout;
        second->catch_source_slot_90 = 0;
        first->frame.frame_counter = 7;
        second->frame.frame_counter = 8;

        const auto pass = world.advance_catch_relations();
        write_state(rows, initial_timeout, "after_catch_pass", world, pass, 0);
        const auto finalizer = world.finalize_horizontal_hit_impulses();
        write_state(rows, initial_timeout, "after_finalizer", world, pass,
                    finalizer.finalized);
    }
    rows.close();
    if (!rows) return 7;
    std::cout << "formal_oid16_action128 timeouts=2,3,4 phases=catch,finalizer\n";
    return 0;
}
