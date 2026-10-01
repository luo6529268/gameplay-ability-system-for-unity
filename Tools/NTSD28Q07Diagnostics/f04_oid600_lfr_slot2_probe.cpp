#include "ntsd28/battle_world.h"
#include "ntsd28_playable/game_session.h"
#include "ntsd28_playable/game_session_lfr.h"

#include <array>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

namespace {

struct SlotSample {
    int active = 0;
    int oid = -1;
    int type = -1;
    int action = -1;
    int x = 0;
    int y = 0;
    int z = 0;
    int hp = 0;
    int owner = -1;
    int group = -1;
    int e4 = 0;

    bool operator==(const SlotSample& other) const {
        return active == other.active && oid == other.oid &&
               type == other.type && action == other.action &&
               x == other.x && y == other.y && z == other.z &&
               hp == other.hp && owner == other.owner &&
               group == other.group && e4 == other.e4;
    }
};

SlotSample sample(const ntsd28_playable::GameSession28& session,
                  std::size_t slot) {
    SlotSample result;
    const auto* entity = session.world()->entity(slot);
    if (entity == nullptr) return result;
    result.active = 1;
    result.oid = entity->object_id;
    result.type = entity->object_type;
    result.action = entity->frame.action;
    result.x = entity->position.x;
    result.y = entity->position.y;
    result.z = entity->position.z;
    result.hp = entity->current_hp;
    result.owner = entity->owner_slot;
    result.group = entity->battle_group;
    result.e4 = entity->heal_timer_e4;
    return result;
}

ntsd28_playable::BattleConfig28 battle_config() {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 2833;
    config.battle_mode = 0;
    config.character_id = 2;
    config.enemy_id = 7;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;

    ntsd28_playable::CombatantConfig28 naruto;
    naruto.slot = 0;
    naruto.object_id = 2;
    naruto.x = 500;
    naruto.z = 650;
    naruto.hp = 500;
    naruto.mp = 500;
    naruto.team = 1;

    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 1;
    lee.object_id = 7;
    lee.x = 1200;
    lee.z = 650;
    lee.hp = 500;
    lee.mp = 500;
    lee.team = 2;

    ntsd28_playable::CombatantConfig28 scroll;
    scroll.slot = 2;
    scroll.object_id = 600;
    scroll.x = 500;
    scroll.y = -20;
    scroll.z = 650;
    scroll.hp = 250;
    scroll.mp = 500;
    scroll.team = 1;
    scroll.action = 0;

    config.combatants = {naruto, lee, scroll};
    return config;
}

void require(bool ok, const std::string& stage, const std::string& error) {
    if (!ok) throw std::runtime_error(stage + ": " + error);
}

void write_sample(std::ofstream& rows, const SlotSample& value) {
    rows << ',' << value.active << ',' << value.oid << ',' << value.type
         << ',' << value.action << ',' << value.x << ',' << value.y
         << ',' << value.z << ',' << value.hp << ',' << value.owner
         << ',' << value.group << ',' << value.e4;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) {
        std::cerr << "usage: f04_oid600_lfr_slot2_probe <formal_runtime_root> <new_output_dir>\n";
        return 2;
    }
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to overwrite an existing diagnostic directory\n";
        return 3;
    }
    try {
        ntsd28_playable::GameSession28 source(root, root);
        std::string error;
        require(source.initialize(battle_config(), error), "source initialize", error);
        const auto initial_scroll = sample(source, 2);
        require(initial_scroll.active == 1 && initial_scroll.oid == 600 &&
                    initial_scroll.type == 4 && initial_scroll.action == 0,
                "source OID600 initial gate", "unexpected object identity");

        ntsd28_playable::GameSessionLfr28 recorder;
        require(recorder.begin(source, error), "recorder begin", error);
        constexpr int kTicks = 12;
        std::array<std::array<SlotSample, 3>, kTicks + 1> source_samples{};
        for (std::size_t slot = 0; slot < 3; ++slot) {
            source_samples[0][slot] = sample(source, slot);
        }
        for (int tick = 1; tick <= kTicks; ++tick) {
            source.set_input(0, {});
            source.set_input(1, {});
            source.step();
            require(recorder.capture_after_step(source, error),
                    "recorder capture tick " + std::to_string(tick), error);
            for (std::size_t slot = 0; slot < 3; ++slot) {
                source_samples[tick][slot] = sample(source, slot);
            }
        }
        std::vector<std::uint8_t> encoded;
        require(recorder.finish_to_memory(source, encoded, error),
                "recorder finish", error);

        ntsd28_playable::GameSessionLfrPlayback28 playback;
        require(playback.load(encoded, ntsd28_playable::LfrTuMode28::two_tu,
                              error),
                "local playback load", error);
        ntsd28_playable::GameSession28 rebuilt(root, root);
        require(playback.initialize_session(rebuilt, error),
                "local playback initialize", error);

        std::filesystem::create_directories(output);
        const auto lfr_path = output / "slot2-source-packets.lfr";
        std::ofstream lfr(lfr_path, std::ios::binary);
        require(static_cast<bool>(lfr), "open LFR", lfr_path.string());
        lfr.write(reinterpret_cast<const char*>(encoded.data()),
                  static_cast<std::streamsize>(encoded.size()));
        lfr.close();
        require(static_cast<bool>(lfr), "write LFR", lfr_path.string());

        std::ofstream rows(output / "source-vs-local-playback.csv", std::ios::binary);
        require(static_cast<bool>(rows), "open comparison", "unable to create CSV");
        rows << "tick,slot,equal,source_active,source_oid,source_type,source_action,"
                "source_x,source_y,source_z,source_hp,source_owner,source_group,"
                "source_e4,playback_active,playback_oid,playback_type,"
                "playback_action,playback_x,playback_y,playback_z,playback_hp,"
                "playback_owner,playback_group,playback_e4\n";
        int differences = 0;
        for (int tick = 0; tick <= kTicks; ++tick) {
            if (tick > 0) {
                require(playback.step(rebuilt, error),
                        "local playback tick " + std::to_string(tick), error);
            }
            for (std::size_t slot = 0; slot < 3; ++slot) {
                const auto actual = sample(rebuilt, slot);
                const auto& expected = source_samples[tick][slot];
                const bool equal = expected == actual;
                if (!equal) ++differences;
                rows << tick << ',' << slot << ',' << (equal ? 1 : 0);
                write_sample(rows, expected);
                write_sample(rows, actual);
                rows << '\n';
            }
        }
        rows.close();
        require(static_cast<bool>(rows), "write comparison", "CSV stream failed");
        require(playback.step(rebuilt, error),
                "local playback unpaired terminal row", error);
        require(playback.verify_final_headers(rebuilt, error),
                "local playback final headers", error);
        std::cout << "initial_slot2_oid=" << initial_scroll.oid
                  << " initial_slot2_type=" << initial_scroll.type
                  << " lfr_bytes=" << encoded.size()
                  << " compared_samples=" << 3 * (kTicks + 1)
                  << " differing_samples=" << differences << '\n';
        return differences == 0 ? 0 : 5;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 4;
    }
}
