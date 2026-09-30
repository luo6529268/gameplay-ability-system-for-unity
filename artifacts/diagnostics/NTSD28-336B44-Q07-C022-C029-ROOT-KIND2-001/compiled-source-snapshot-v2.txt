#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

namespace {
int write_weapon_inventory(const std::filesystem::path& root,
                           const std::filesystem::path& output) {
    if (std::filesystem::exists(output)) return 3;
    ntsd28::ObjectDefinitionCatalog28 catalog;
    const auto loaded = catalog.load_extracted_root(root);
    if (!loaded.success) return 4;
    std::filesystem::create_directories(output.parent_path());
    std::ofstream csv(output, std::ios::binary);
    if (!csv) return 6;
    csv << "oid,type,path,frames,primary_kind2_frames\n";
    int weapons = 0;
    int kind2_frames = 0;
    for (const auto& item : catalog.entries()) {
        const auto& entry = item.second;
        if (entry.object_type != 1 && entry.object_type != 2 &&
            entry.object_type != 4 && entry.object_type != 6) continue;
        if (!entry.definition) return 7;
        int count = 0;
        for (const auto& frame : entry.definition->frames) {
            const auto* point = frame.first_block("cpoint");
            if (point && point->values.integer("kind").value_or(0) == 2) ++count;
        }
        ++weapons;
        kind2_frames += count;
        csv << entry.object_id << ',' << entry.object_type << ','
            << entry.source_path << ',' << entry.definition->frames.size() << ','
            << count << '\n';
    }
    csv.close();
    if (!csv) return 9;
    std::cout << "catalog_entries=" << loaded.entries_loaded << " weapon_entries="
              << weapons << " primary_kind2_frames=" << kind2_frames << '\n';
    return 0;
}

int parse_integer(const char* value) {
    std::size_t parsed = 0;
    const std::string text(value);
    const int result = std::stoi(text, &parsed);
    if (parsed != text.size()) throw std::invalid_argument("integer suffix");
    return result;
}

void write_entity(std::ostream& stream, const ntsd28::EntityState28& entity) {
    const auto* frame = entity.definition->frame(entity.frame.action);
    const auto* cpoint = frame == nullptr ? nullptr : frame->first_block("cpoint");
    stream << entity.frame.action << ','
           << (frame ? frame->values.integer("state").value_or(0) : -1) << ','
           << entity.frame.frame_counter << ',' << entity.position.x << ','
           << entity.position.y << ',' << entity.position.z << ','
           << entity.motion.x << ',' << entity.motion.y << ',' << entity.motion.z << ','
           << entity.current_hp << ',' << entity.catch_target_slot_8c << ','
           << entity.catch_source_slot_90 << ',' << entity.catch_timeout_94 << ','
           << entity.motion_hold_timer << ',' << entity.interaction_state << ','
           << (cpoint ? cpoint->values.integer("kind").value_or(0) : -1);
}
}

int main(int argc, char** argv) {
    if (argc == 5 && std::string(argv[3]) == "weapon-catalog")
        return write_weapon_inventory(argv[1], argv[4]);
    if (argc != 6) return 2;
    int action = 0;
    int target_x = 0;
    int jumping = 0;
    try {
        action = parse_integer(argv[3]);
        target_x = parse_integer(argv[4]);
        jumping = parse_integer(argv[5]);
    } catch (...) { return 2; }
    if ((action != 255 && action != 258) || target_x < 550 || target_x > 1200 ||
        (jumping != 0 && jumping != 1)) return 2;
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    const auto csv_path = output / "source-ticks.csv";
    const auto lfr_path = output / "source-packets.lfr";
    if (std::filesystem::exists(csv_path) || std::filesystem::exists(lfr_path))
        return 3;
    std::filesystem::create_directories(output);
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = config.enemy_id = 52;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 actor;
    actor.slot = 0;
    actor.object_id = 52;
    actor.x = 500;
    actor.z = 400;
    actor.hp = actor.base_hp = actor.mp = 500;
    actor.team = 1;
    actor.action = action;
    ntsd28_playable::CombatantConfig28 target = actor;
    target.slot = 1;
    target.x = target_x;
    target.action = 0;
    target.team = 2;
    config.combatants = {actor, target};
    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(config, error)) { std::cerr << error; return 4; }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) { std::cerr << error; return 5; }
    std::ofstream csv(csv_path, std::ios::binary);
    if (!csv) return 6;
    csv << "tick";
    for (const std::string prefix : {"actor", "target"})
        for (const std::string name : {"action", "state", "counter", "x", "y", "z",
             "vx", "vy", "vz", "hp", "catch_target", "catch_source", "catch_timeout",
             "motion_hold", "interaction", "cpoint_kind"})
            csv << ',' << prefix << '_' << name;
    csv << ",prior_target_counter,prior_target_vy,relation_hits\n" << std::setprecision(17);
    int first_catch = -1;
    double captured_vy = 0.0;
    for (int tick = 1; tick <= 32; ++tick) {
        const auto* before = session.world()->entity(1);
        if (!before) return 7;
        const int prior_counter = before->frame.frame_counter;
        const double prior_vy = before->motion.y;
        ntsd28::InputButtons28 buttons;
        if (jumping && tick <= 4) buttons.set(ntsd28::InputKey28::jump);
        session.set_input(0, ntsd28::InputButtons28{});
        session.set_input(1, buttons);
        session.step();
        if (!recorder.capture_after_step(session, error)) { std::cerr << error; return 8; }
        const auto* world = session.world();
        const auto* first = world->entity(0);
        const auto* second = world->entity(1);
        if (!first || !second) return 9;
        int relation_hits = 0;
        for (const auto& hit : session.last_tick()->relation_hits)
            if (hit.interaction_kind == 3 &&
                hit.status == ntsd28::WorldRelationHitStatus28::applied) ++relation_hits;
        if (relation_hits && first_catch < 0) { first_catch = tick; captured_vy = prior_vy; }
        csv << world->sequence() << ',';
        write_entity(csv, *first);
        csv << ',';
        write_entity(csv, *second);
        csv << ',' << prior_counter << ',' << prior_vy << ',' << relation_hits << '\n';
    }
    csv.close();
    if (!csv) return 10;
    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) { std::cerr << error; return 11; }
    std::ofstream lfr(lfr_path, std::ios::binary);
    lfr.write(reinterpret_cast<const char*>(bytes.data()),
              static_cast<std::streamsize>(bytes.size()));
    lfr.close();
    if (!lfr) return 12;
    std::cout << "action=" << action << " target_x=" << target_x << " jumping=" << jumping
              << " first_catch=" << first_catch << " captured_vy=" << captured_vy
              << " ticks=32\n";
    return 0;
}
