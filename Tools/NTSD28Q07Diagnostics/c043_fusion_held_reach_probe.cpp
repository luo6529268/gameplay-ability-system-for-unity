#include "ntsd28_playable/game_session.h"
#include "ntsd28_playable/game_session_lfr.h"

#include <array>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

namespace {

struct Profile {
    const char* name;
    int input_pattern;
};

ntsd28::InputButtons28 buttons_for(int tick, int pattern) {
    ntsd28::InputButtons28 buttons;
    const bool right = pattern == 0 ? tick >= 23 :
                       pattern == 1 ? ((tick >= 23 && tick <= 24) || tick >= 27) :
                                      tick >= 1;
    if (right) buttons.set(ntsd28::InputKey28::right);
    return buttons;
}

void run_profile(const std::filesystem::path& runtime_root,
                 const std::filesystem::path& output,
                 const Profile& profile) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 0x28A55A5Au;
    config.battle_mode = 0;
    config.character_id = 7;
    config.enemy_id = 8;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 0;
    lee.object_id = 7;
    lee.x = 304;
    lee.z = 400;
    lee.hp = 100;
    lee.base_hp = 100;
    lee.mp = 500;
    lee.team = 1;
    lee.action = 0;
    auto chi = lee;
    chi.slot = 1;
    chi.object_id = 8;
    chi.x = 300;
    chi.action = 256;
    chi.facing = true;
    config.combatants = {lee, chi};

    ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
    std::string error;
    if (!session.initialize(config, error)) throw std::runtime_error(error);
    auto* world = session.world();
    if (!world || !world->entity(0) || !world->entity(1))
        throw std::runtime_error("roster unavailable");
    ntsd28_playable::GameSessionLfr28 recorder;
    if (!recorder.begin(session, error)) throw std::runtime_error(error);

    std::ofstream rows(output, std::ios::binary);
    if (!rows) throw std::runtime_error("output open failed");
    rows << "profile,tick,input_right,lee_oid,lee_action,lee_state,lee_x,lee_link,"
            "chi_present,chi_action,chi_state,chi_x,chi_link,chi_child,"
            "child_slot,child_oid,child_action,child_link,child_parent,"
            "fusion_count,held_pre_count,held_post_count,held_pre_ok,held_post_ok,"
            "held_pre_diagnostic,held_post_diagnostic\n";
    for (int tick = 1; tick <= 60; ++tick) {
        const auto input = buttons_for(tick, profile.input_pattern);
        session.set_input(0, input);
        session.set_input(1, input);
        session.step();
        if (!recorder.capture_after_step(session, error))
            throw std::runtime_error(error);
        const auto* result = session.last_tick();
        if (!result) throw std::runtime_error("tick result unavailable");
        const auto* primary = world->entity(0);
        const auto* partner = world->entity(1);
        int child_slot = -1;
        const ntsd28::EntityState28* child = nullptr;
        for (int slot = 20; slot < 100; ++slot) {
            const auto* candidate = world->entity(static_cast<std::size_t>(slot));
            if (candidate && candidate->object_id == 420) {
                child_slot = slot;
                child = candidate;
                break;
            }
        }
        const auto state = [](const ntsd28::EntityState28* entity) {
            if (!entity || !entity->definition) return -1;
            const auto* frame = entity->definition->declared_frame(entity->frame.action);
            return frame ? frame->values.integer("state").value_or(-1) : -1;
        };
        const auto first_diagnostic = [](const std::vector<std::string>& diagnostics) {
            return diagnostics.empty() ? std::string{} : diagnostics.front();
        };
        rows << profile.name << ',' << tick << ','
             << (input[ntsd28::InputKey28::right] ? 1 : 0) << ','
             << (primary ? primary->object_id : -1) << ','
             << (primary ? primary->frame.action : -1) << ',' << state(primary) << ','
             << (primary ? primary->position.x : -1) << ','
             << (primary ? primary->interaction_state : 0) << ','
             << (partner ? 1 : 0) << ','
             << (partner ? partner->frame.action : -1) << ',' << state(partner) << ','
             << (partner ? partner->position.x : -1) << ','
             << (partner ? partner->interaction_state : 0) << ','
             << (partner ? partner->linked_child_slot : -1) << ','
             << child_slot << ',' << (child ? child->object_id : -1) << ','
             << (child ? child->frame.action : -1) << ','
             << (child ? child->interaction_state : 0) << ','
             << (child ? child->linked_parent_slot : -1) << ','
             << result->fusions.fused << ','
             << result->held_refills_before_geometry.linked_children << ','
             << result->held_refills_after_hits.linked_children << ','
             << (result->held_refills_before_geometry.success ? 1 : 0) << ','
             << (result->held_refills_after_hits.success ? 1 : 0) << ','
             << first_diagnostic(result->held_refills_before_geometry.diagnostics) << ','
             << first_diagnostic(result->held_refills_after_hits.diagnostics) << '\n';
    }
    if (!rows) throw std::runtime_error("output write failed");
    rows.close();
    std::vector<std::uint8_t> packets;
    if (!recorder.finish_to_memory(session, packets, error))
        throw std::runtime_error(error);
    std::ofstream lfr(output.parent_path() /
                      (std::string(profile.name) + ".lfr"), std::ios::binary);
    if (!lfr) throw std::runtime_error("LFR output open failed");
    lfr.write(reinterpret_cast<const char*>(packets.data()),
              static_cast<std::streamsize>(packets.size()));
    if (!lfr) throw std::runtime_error("LFR output write failed");
}

}  // namespace

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) return 2;
    try {
        const std::filesystem::path runtime_root(argv[1]);
        const std::filesystem::path output_dir(argv[2]);
        if (std::filesystem::exists(output_dir))
            throw std::runtime_error("refusing to overwrite output directory");
        std::filesystem::create_directories(output_dir);
        const std::array<Profile, 3> profiles{{
            {"late_hold", 0}, {"late_double_tap", 1}, {"early_hold", 2}}};
        for (const auto& profile : profiles)
            run_profile(runtime_root, output_dir / (std::string(profile.name) + ".csv"),
                        profile);
        return 0;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 4;
    }
}
