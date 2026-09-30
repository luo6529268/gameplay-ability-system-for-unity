#include "ntsd28_playable/game_session_lfr.h"
#include "ntsd28/render_snapshot.h"

#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: ita_equal_hp_root_lfr_probe <formal_runtime_root> <output_dir>\n";
        return 2;
    }

    const std::filesystem::path runtime_root(argv[1]);
    const std::filesystem::path output_root(argv[2]);
    const auto csv_path = output_root / "ita_equal_hp_source_ticks.csv";
    const auto lfr_path = output_root / "ita_equal_hp_source_packets.lfr";
    if (!std::filesystem::is_directory(output_root)) {
        std::cerr << "output directory does not exist\n";
        return 3;
    }
    if (std::filesystem::exists(csv_path) || std::filesystem::exists(lfr_path)) {
        std::cerr << "refusing to overwrite existing diagnostic output\n";
        return 4;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 2833;
    config.character_id = 2;
    config.enemy_id = 9;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 naruto;
    naruto.slot = 0;
    naruto.object_id = 2;
    naruto.x = 500;
    naruto.z = 650;
    naruto.hp = naruto.base_hp = 500;
    naruto.mp = 500;
    naruto.team = 1;
    naruto.action = 0;
    naruto.facing = false;

    ntsd28_playable::CombatantConfig28 ita;
    ita.slot = 1;
    ita.object_id = 9;
    ita.x = 540;
    ita.z = 650;
    ita.hp = ita.base_hp = 30;
    ita.mp = 500;
    ita.team = 2;
    ita.action = 0;
    ita.facing = true;
    config.combatants = {naruto, ita};

    ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
    std::string error;
    if (!session.initialize(config, error)) {
        std::cerr << "session initialize failed: " << error << '\n';
        return 5;
    }
    const auto* initial_world = session.world();
    const auto* initial_target = initial_world == nullptr
                                     ? nullptr
                                     : initial_world->entity(1);
    if (initial_target == nullptr || initial_target->current_hp != 30 ||
        initial_target->base_max_hp != 30) {
        std::cerr << "equal current/base HP initial state was not retained\n";
        return 6;
    }

    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) {
        std::cerr << "LFR begin failed: " << error << '\n';
        return 7;
    }
    std::ofstream rows(csv_path, std::ios::binary);
    if (!rows) {
        std::cerr << "cannot open tick CSV\n";
        return 8;
    }
    rows << "tick,world_sequence,attack,naruto_action,naruto_x,ita_action,ita_x,ita_hp,ita_base_hp,bleed_command_count,mark_threshold,mark_width,mark_height,mark_rgb,crt_state,crt_calls,synchronized_counter,synchronized_index,synchronized_calls\n";

    int first_damage_tick = -1;
    int selected_tick = -1;
    int selected_hp = -1;
    for (int tick = 1; tick <= 80; ++tick) {
        ntsd28::InputButtons28 buttons;
        const bool attack = tick <= 2;
        if (attack) buttons.set(ntsd28::InputKey28::attack);
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "LFR capture failed at tick " << tick << ": "
                      << error << '\n';
            return 9;
        }

        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        const auto* target = world == nullptr ? nullptr : world->entity(1);
        if (actor == nullptr || target == nullptr || actor->object_id != 2 ||
            target->object_id != 9) {
            std::cerr << "selected roster disappeared at tick " << tick << '\n';
            return 10;
        }
        if (first_damage_tick < 0 && target->current_hp < 30)
            first_damage_tick = tick;
        const auto random = world->random().state();

        const auto snapshot = session.snapshot(false, 1333, 730);
        int mark_count = 0;
        int threshold = -1;
        int width = -1;
        int height = -1;
        std::uint32_t rgb = 0;
        for (const auto& command : snapshot.entity_commands) {
            if (command.kind != ntsd28::RenderEntityCommandKind28::bleed_mark ||
                command.slot != 1) {
                continue;
            }
            if (command.item_index >= snapshot.bleed_marks.size()) {
                std::cerr << "bleed command has no mark data\n";
                return 11;
            }
            const auto& mark = snapshot.bleed_marks[command.item_index];
            ++mark_count;
            threshold = mark.hp_threshold;
            width = mark.width;
            height = mark.height;
            rgb = mark.rgb;
        }
        rows << tick << ',' << world->sequence() << ',' << attack << ','
             << actor->frame.action << ',' << actor->position.x << ','
             << target->frame.action << ',' << target->position.x << ','
             << target->current_hp << ',' << target->base_max_hp << ','
             << mark_count << ',' << threshold << ',' << width << ','
             << height << ',' << rgb << ',' << random.crt_state << ','
             << random.crt_calls << ',' << random.synchronized.counter << ','
             << random.synchronized.index << ','
             << random.synchronized.calls << '\n';

        if (first_damage_tick >= 0 && target->current_hp > 0 &&
            target->frame.action == 0 && mark_count == 1 &&
            threshold == 10 && width == 1 && height == 3 &&
            rgb == 0x00ff0000U) {
            selected_tick = tick;
            selected_hp = target->current_hp;
            break;
        }
    }
    rows.close();
    if (!rows) {
        std::cerr << "tick CSV write failed\n";
        return 12;
    }
    if (selected_tick < 0) {
        std::cerr << "natural surviving standing bleed not reached; firstDamageTick="
                  << first_damage_tick << '\n';
        return 13;
    }

    std::vector<std::uint8_t> bytes;
    if (!recorder.finish_to_memory(session, bytes, error)) {
        std::cerr << "LFR finish failed: " << error << '\n';
        return 14;
    }
    std::ofstream encoded(lfr_path, std::ios::binary);
    if (!encoded) return 15;
    encoded.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
    encoded.close();
    if (!encoded) return 16;

    std::cout << "selectedTick=" << selected_tick
              << " firstDamageTick=" << first_damage_tick
              << " itaHP=" << selected_hp
              << " itaBaseHP=30 bleedThreshold=10"
              << " recordedTicks=" << recorder.row_count()
              << " encodedBytes=" << bytes.size() << '\n';
    return 0;
}
