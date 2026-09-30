#include "ntsd28_playable/game_session_lfr.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int wmain(int argc, wchar_t** argv) {
    if (argc != 4 && argc != 5 && argc != 6) {
        std::cerr << "usage: chiyo_control_itr_lfr_probe <decoded_dat> <vfs> <output_dir> [chiyo_hp [attack_defend|attack_defend_54]]\n";
        return 2;
    }
    int initial_hp = 500;
    if (argc >= 5) {
        try {
            std::size_t parsed = 0;
            initial_hp = std::stoi(argv[4], &parsed);
            if (parsed != std::wstring(argv[4]).size() || initial_hp < 1 ||
                initial_hp > 500) {
                throw std::invalid_argument("invalid HP");
            }
        } catch (const std::exception&) {
            std::cerr << "chiyo_hp must be an integer in 1..500\n";
            return 2;
        }
    }
    int continuation_tick = 0;
    if (argc == 6) {
        const std::wstring continuation(argv[5]);
        if (continuation == L"attack_defend") continuation_tick = 40;
        else if (continuation == L"attack_defend_54") continuation_tick = 54;
        else {
            std::cerr << "unsupported continuation\n";
            return 2;
        }
    }
    const std::filesystem::path output(argv[3]);
    std::filesystem::create_directories(output);
    const auto csv = output / "chiyo-control-itr-source-ticks.csv";
    const auto lfr = output / "chiyo-control-itr-source-packets.lfr";
    if (std::filesystem::exists(csv) || std::filesystem::exists(lfr)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 8;
    config.enemy_id = 2;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 chiyo;
    chiyo.slot = 0;
    chiyo.object_id = 8;
    chiyo.x = 500;
    chiyo.z = 650;
    chiyo.hp = initial_hp;
    chiyo.mp = 500;
    chiyo.team = 1;
    chiyo.action = 0;
    ntsd28_playable::CombatantConfig28 opponent;
    opponent.slot = 1;
    opponent.object_id = 2;
    opponent.x = 1200;
    opponent.z = 650;
    opponent.hp = 500;
    opponent.mp = 500;
    opponent.team = 2;
    opponent.action = 0;
    config.combatants = {chiyo, opponent};

    ntsd28_playable::GameSession28 session(argv[1], argv[2]);
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
    rows << "initial_hp,tick,input,phase,chiyo_action,chiyo_mp,oid419_count,oid854_count,oid854_frames,oid854_frame407\n";
    bool saw_chiyo_301 = false;
    bool saw_419 = false;
    bool saw_854 = false;
    bool saw_407 = false;
    const int tick_limit = continuation_tick != 0 ? 120 : 90;
    for (int tick = 1; tick <= tick_limit; ++tick) {
        ntsd28::InputButtons28 buttons;
        const char* input = "none";
        if (tick <= 2) {
            buttons.set(ntsd28::InputKey28::defend);
            input = "defend";
        } else if (tick <= 4) {
            buttons.set(ntsd28::InputKey28::depth_up);
            input = "up";
        } else if (tick <= 6) {
            buttons.set(ntsd28::InputKey28::jump);
            input = "jump";
        } else if (continuation_tick != 0 && tick >= continuation_tick &&
                   tick <= continuation_tick + 1) {
            buttons.set(ntsd28::InputKey28::attack);
            input = "attack";
        } else if (continuation_tick != 0 && tick >= continuation_tick + 2 &&
                   tick <= continuation_tick + 3) {
            buttons.set(ntsd28::InputKey28::defend);
            input = "defend";
        }
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "record tick " << tick << ": " << error << '\n';
            return 7;
        }
        const auto* world = session.world();
        const auto* actor = world == nullptr ? nullptr : world->entity(0);
        if (actor == nullptr) {
            std::cerr << "Chiyo slot0 disappeared at tick " << tick << '\n';
            return 8;
        }
        saw_chiyo_301 |= actor->frame.action == 301;
        int effect_count = 0;
        int puppet_count = 0;
        bool frame407 = false;
        std::string puppet_frames;
        for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            const auto* entity = world->entity(slot);
            if (entity == nullptr || entity->owner_slot != 0) continue;
            if (entity->object_id == 419) ++effect_count;
            if (entity->object_id == 854) {
                ++puppet_count;
                if (!puppet_frames.empty()) puppet_frames += '|';
                puppet_frames += std::to_string(slot) + ':' +
                                 std::to_string(entity->frame.action);
                frame407 |= entity->frame.action == 407;
            }
        }
        saw_419 |= effect_count != 0;
        saw_854 |= puppet_count != 0;
        saw_407 |= frame407;
        rows << initial_hp << ',' << tick << ',' << input << ',' << world->input_update_phase_4a0b90()
             << ',' << actor->frame.action << ',' << actor->current_mp << ','
             << effect_count << ',' << puppet_count << ',' << puppet_frames
             << ',' << (frame407 ? 1 : 0) << '\n';
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
    std::cout << "initial_hp=" << initial_hp << " ticks=" << recorder.row_count()
              << " chiyo301=" << saw_chiyo_301
              << " oid419=" << saw_419 << " oid854=" << saw_854
              << " frame407=" << saw_407 << " lfr_bytes=" << bytes.size() << '\n';
    return 0;
}
