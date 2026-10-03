#include "ntsd28/battle_world.h"
#include "ntsd28_playable/d3d11_renderer.h"
#include "ntsd28_playable/game_session.h"
#include "ntsd28_playable/game_session_lfr.h"

#include <objbase.h>

#include <cmath>
#include <cstdint>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>
#include <vector>

namespace {

int frame_state(const ntsd28::EntityState28* entity) {
    if (entity == nullptr || entity->definition == nullptr) return -1;
    const auto* frame = entity->definition->frame(entity->frame.action);
    return frame == nullptr ? -1 : frame->values.integer("state").value_or(-1);
}

void write_row(std::ofstream& csv, int tick, const char* phase,
               const char* input, const ntsd28::BattleWorld28& world) {
    const auto* actor = world.entity(0);
    const auto* weapon = world.entity(2);
    csv << tick << ',' << phase << ',' << input << ',' << world.sequence() << ',';
    if (actor != nullptr) {
        csv << actor->frame.action << ',' << frame_state(actor) << ','
            << actor->interaction_state << ',' << actor->linked_child_slot << ','
            << actor->position.x << ',' << actor->position.y << ','
            << actor->motion.x << ',';
    } else {
        csv << "-1,-1,0,-1,0,0,0,";
    }
    if (weapon != nullptr) {
        csv << weapon->object_id << ',' << weapon->object_type << ','
            << weapon->frame.action << ',' << frame_state(weapon) << ','
            << weapon->interaction_state << ',' << weapon->linked_parent_slot
            << ',' << weapon->position.x << ',' << weapon->position.y << ','
            << weapon->motion.x << ',' << weapon->motion.y << '\n';
    } else {
        csv << "-1,-1,-1,-1,0,-1,0,0,0,0\n";
    }
}

ntsd28_playable::BattleConfig28 make_config(int weapon_x, bool kind10,
                                            int opponent_x, int source_z,
                                            int background_id) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 0;
    config.character_id = 2;
    config.enemy_id = kind10 ? 36 : 7;
    config.background_id = background_id;
    config.bgm_selection_49f18c = 2;

    ntsd28_playable::CombatantConfig28 naruto;
    naruto.slot = 0;
    naruto.object_id = 2;
    naruto.x = 200;
    naruto.z = source_z;
    naruto.hp = naruto.base_hp = naruto.mp = 500;
    naruto.team = 1;

    ntsd28_playable::CombatantConfig28 lee = naruto;
    lee.slot = 1;
    lee.object_id = kind10 ? 36 : 7;
    lee.x = kind10 ? opponent_x : 1200;
    lee.team = 2;
    lee.action = kind10 ? 243 : 0;

    ntsd28_playable::CombatantConfig28 weapon;
    weapon.slot = 2;
    weapon.object_id = 600;
    weapon.x = weapon_x;
    weapon.y = -20;
    weapon.z = source_z;
    weapon.hp = weapon.base_hp = 250;
    weapon.mp = 0;
    weapon.team = 1;
    weapon.action = 0;
    config.combatants = {naruto, lee, weapon};
    return config;
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc < 4 || argc > 8) return 2;
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    const std::wstring mode(argv[3]);
    const bool kind10 = mode == L"kind10" && argc >= 5 && argc <= 8;
    const bool capture_offscreen = kind10 && argc == 8 &&
                                   std::wstring(argv[7]) == L"--offscreen-tick39";
    if ((mode != L"near" && mode != L"far" && !kind10) ||
        (!kind10 && argc != 4 && argc != 5 && argc != 6) ||
        (argc == 8 && !capture_offscreen)) return 2;
    int opponent_x = 0;
    int source_z = 542;
    int background_id = 23;
    if (kind10) {
        try { opponent_x = std::stoi(argv[4]); }
        catch (...) { return 2; }
        if (opponent_x < 350 || opponent_x > 800) return 2;
    }
    if (argc >= (kind10 ? 6 : 5)) {
        try { source_z = std::stoi(argv[kind10 ? 5 : 4]); }
        catch (...) { return 2; }
        if (source_z < 180 || source_z > 542) return 2;
    }
    if (argc == (kind10 ? 7 : 6) || capture_offscreen) {
        try { background_id = std::stoi(argv[kind10 ? 6 : 5]); }
        catch (...) { return 2; }
        if (background_id != 1 && background_id != 23) return 2;
    }
    if (std::filesystem::exists(output)) return 3;

    ntsd28_playable::GameSession28 session(root, root);
    std::string error;
    if (!session.initialize(make_config(mode == L"far" ? 800 : 190,
                                        kind10, opponent_x, source_z,
                                        background_id), error)) {
        std::cerr << "initialize: " << error << '\n';
        return 4;
    }
    const auto* initial_weapon = session.world()->entity(2);
    if (initial_weapon == nullptr || initial_weapon->object_id != 600 ||
        initial_weapon->object_type != 4 || initial_weapon->frame.action != 0) {
        std::cerr << "formal initial OID600/type4/action0 gate failed\n";
        return 5;
    }
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) {
        std::cerr << "recorder: " << error << '\n';
        return 6;
    }

    std::filesystem::create_directories(output);
    std::ofstream csv(output / "source-ticks.csv", std::ios::binary);
    if (!csv) return 7;
    csv << "tick,phase,input,sequence,actor_action,actor_state,actor_relation,"
           "actor_child_slot,actor_x,actor_y,actor_vx,weapon_oid,weapon_type,"
           "weapon_action,weapon_state,weapon_relation,weapon_parent_slot,"
           "weapon_x,weapon_y,weapon_vx,weapon_vy\n" << std::setprecision(17);
    write_row(csv, 0, "initial", "none", *session.world());

    std::ofstream opponent;
    std::ofstream relations;
    if (kind10) {
        opponent.open(output / "opponent-ticks.csv", std::ios::binary);
        relations.open(output / "relation-hits.csv", std::ios::binary);
        if (!opponent || !relations) return 7;
        opponent << "tick,phase,oid,action,state,x,y,z,hp\n";
        relations << "tick,attacker_slot,target_slot,kind,status,"
                     "attacker_action,target_action,target_vx_after,message\n";
        const auto* enemy = session.world()->entity(1);
        opponent << "0,initial," << enemy->object_id << ','
                 << enemy->frame.action << ',' << frame_state(enemy) << ','
                 << enemy->position.x << ',' << enemy->position.y << ','
                 << enemy->position.z << ',' << enemy->current_hp << '\n';
    }

    int ground_tick = -1;
    int pickup_tick = -1;
    int throw_action_tick = -1;
    int high_release_tick = -1;
    int attack_for_pickup = 0;
    int neutral_after_pickup = 0;
    bool pickup_attempted = false;
    bool throw_attempted = false;
    int kind10_hit_count = 0;
    int first_state1000_high_tick = -1;
    int f02_return_tick = -1;
    bool previous_state1000_high = false;
    constexpr int kTicks = 128;
    for (int tick = 1; tick <= kTicks; ++tick) {
        const auto* actor_before = session.world()->entity(0);
        const auto* weapon_before = session.world()->entity(2);
        if (actor_before == nullptr) return 8;
        if (weapon_before != nullptr && frame_state(weapon_before) == 1004 &&
            ground_tick < 0) {
            ground_tick = tick - 1;
        }
        if (!pickup_attempted && ground_tick >= 0 &&
            frame_state(actor_before) == 0) {
            pickup_attempted = true;
            attack_for_pickup = 2;
        }

        ntsd28::InputButtons28 buttons;
        const char* input = "none";
        if (attack_for_pickup > 0) {
            buttons.set(ntsd28::InputKey28::attack);
            --attack_for_pickup;
            input = "pickup_attack";
        } else if (pickup_tick >= 0 && !throw_attempted &&
                   neutral_after_pickup >= 2 &&
                   actor_before->interaction_state == 4 &&
                   (frame_state(actor_before) == 0 ||
                    frame_state(actor_before) == 1)) {
            buttons.set(ntsd28::InputKey28::attack);
            throw_attempted = true;
            input = "light_throw_attack";
        }
        write_row(csv, tick, "before", input, *session.world());
        if (kind10) {
            const auto* enemy = session.world()->entity(1);
            opponent << tick << ",before," << enemy->object_id << ','
                     << enemy->frame.action << ',' << frame_state(enemy) << ','
                     << enemy->position.x << ',' << enemy->position.y << ','
                     << enemy->position.z << ',' << enemy->current_hp << '\n';
        }
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (!recorder.capture_after_step(session, error)) {
            std::cerr << "record tick " << tick << ": " << error << '\n';
            return 9;
        }
        if (capture_offscreen && tick == 39) {
            const auto snapshot = session.snapshot(false);
            int naruto_count = 0;
            int pic3_count = 0;
            int pic8_count = 0;
            std::ofstream geometry(output / "tick39-sprites.csv", std::ios::binary);
            if (!geometry) return 14;
            geometry << "slot,oid,pic,screen_left,screen_top,width,height,"
                        "source_x,source_y,source_path\n";
            for (const auto& sprite : snapshot.sprites) {
                geometry << sprite.slot << ',' << sprite.object_id << ','
                         << sprite.pic << ',' << sprite.screen_left << ','
                         << sprite.screen_top << ',' << sprite.frame.width << ','
                         << sprite.frame.height << ',' << sprite.frame.source_x
                         << ',' << sprite.frame.source_y << ','
                         << sprite.frame.source_path.u8string() << '\n';
                if (sprite.slot == 0 && sprite.object_id == 2 && sprite.pic == 1)
                    ++naruto_count;
                if (sprite.slot == 51 && sprite.object_id == 219 && sprite.pic == 3)
                    ++pic3_count;
                if (sprite.slot == 50 && sprite.object_id == 219 && sprite.pic == 8)
                    ++pic8_count;
            }
            geometry.close();
            if (!geometry || snapshot.sprites.size() != 5 ||
                naruto_count != 1 || pic3_count != 1 || pic8_count != 1) {
                std::cerr << "formal tick39 sprite identity gate failed\n";
                return 15;
            }
            const HRESULT com = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
            if (FAILED(com)) {
                std::cerr << "COM initialization failed\n";
                return 16;
            }
            bool rendered = false;
            {
                ntsd28_playable::D3D11Renderer28 renderer;
                rendered = renderer.initialize_offscreen(1333, 730, error) &&
                           renderer.render(snapshot, false, error) &&
                           renderer.save_offscreen_png(
                               output / "tick39-offscreen.png", error);
            }
            CoUninitialize();
            if (!rendered) {
                std::cerr << "formal tick39 offscreen render: " << error << '\n';
                return 17;
            }
        }
        write_row(csv, tick, "after", input, *session.world());
        if (kind10) {
            const auto* enemy = session.world()->entity(1);
            opponent << tick << ",after," << enemy->object_id << ','
                     << enemy->frame.action << ',' << frame_state(enemy) << ','
                     << enemy->position.x << ',' << enemy->position.y << ','
                     << enemy->position.z << ',' << enemy->current_hp << '\n';
            for (const auto& hit : session.last_tick()->relation_hits) {
                if (hit.attacker_slot != 1 || hit.target_slot != 2) continue;
                relations << tick << ',' << hit.attacker_slot << ','
                          << hit.target_slot << ',' << hit.interaction_kind << ','
                          << static_cast<int>(hit.status) << ','
                          << hit.attacker_action << ',' << hit.target_action
                          << ',' << hit.target_motion_x_after << ','
                          << hit.message << '\n';
                if (hit.interaction_kind == 10 &&
                    hit.status == ntsd28::WorldRelationHitStatus28::applied)
                    ++kind10_hit_count;
            }
        }

        const auto* actor = session.world()->entity(0);
        const auto* weapon = session.world()->entity(2);
        if (actor != nullptr && pickup_tick < 0 &&
            actor->interaction_state == 4 && actor->linked_child_slot == 2) {
            pickup_tick = tick;
        }
        if (pickup_tick >= 0 && input == std::string("none")) {
            ++neutral_after_pickup;
        }
        if (actor != nullptr && throw_action_tick < 0 &&
            (actor->frame.action == 45 || actor->frame.action == 47)) {
            throw_action_tick = tick;
        }
        if (weapon != nullptr && high_release_tick < 0 && pickup_tick >= 0 &&
            weapon->interaction_state == 0 && frame_state(weapon) == 1002 &&
            std::abs(weapon->motion.x) > 9.0) {
            high_release_tick = tick;
        }
        const bool state1000_high = weapon != nullptr &&
            frame_state(weapon) == 1000 && std::abs(weapon->motion.x) > 9.0;
        if (state1000_high && first_state1000_high_tick < 0)
            first_state1000_high_tick = tick;
        if (previous_state1000_high && weapon != nullptr &&
            frame_state(weapon) == 1002 && f02_return_tick < 0)
            f02_return_tick = tick;
        previous_state1000_high = state1000_high;
    }
    csv.close();
    if (!csv) return 10;
    if (kind10) {
        opponent.close();
        relations.close();
        if (!opponent || !relations) return 10;
    }
    std::ofstream summary(output / "summary.txt", std::ios::binary);
    if (!summary) return 11;
    summary << "mode=" << (kind10 ? "kind10" :
                          (mode == L"near" ? "near" : "far"))
            << " opponent_x=" << opponent_x
            << " ticks=" << kTicks << " ground_tick=" << ground_tick
            << " pickup_attempted=" << pickup_attempted
            << " pickup_tick=" << pickup_tick
            << " throw_attempted=" << throw_attempted
            << " throw_action_tick=" << throw_action_tick
            << " high_release_tick=" << high_release_tick
            << " kind10_hits=" << kind10_hit_count
            << " first_state1000_high_tick=" << first_state1000_high_tick
            << " f02_return_tick=" << f02_return_tick << '\n';
    summary.close();
    if (!summary) return 11;
    if (high_release_tick >= 0) {
        std::vector<std::uint8_t> bytes;
        if (!recorder.finish_to_memory(session, bytes, error)) {
            std::cerr << "finish recorder: " << error << '\n';
            return 12;
        }
        std::ofstream lfr(output / "source-packets.lfr", std::ios::binary);
        if (!lfr) return 13;
        lfr.write(reinterpret_cast<const char*>(bytes.data()),
                  static_cast<std::streamsize>(bytes.size()));
        lfr.close();
        if (!lfr) return 13;
    }
    std::cout << "ground_tick=" << ground_tick
              << " pickup_tick=" << pickup_tick
              << " throw_action_tick=" << throw_action_tick
              << " high_release_tick=" << high_release_tick
              << " kind10_hits=" << kind10_hit_count
              << " first_state1000_high_tick=" << first_state1000_high_tick
              << " f02_return_tick=" << f02_return_tick << '\n';
    return 0;
}
