#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <stdexcept>
#include <string>

namespace {

std::string csv_field(const std::string& value) {
    std::string result = "\"";
    for (const char c : value) {
        if (c == '"') result += '"';
        result += c;
    }
    return result + '"';
}

ntsd28_playable::BattleConfig28 make_config() {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 65;
    config.enemy_id = 702;
    config.background_id = 1;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 anko;
    anko.slot = 0;
    anko.object_id = 65;
    anko.x = 580;
    anko.y = 0;
    anko.z = 400;
    anko.hp = anko.base_hp = anko.mp = 500;
    anko.team = 1;
    anko.action = 511;

    auto jiraiya = anko;
    jiraiya.slot = 1;
    jiraiya.object_id = 702;
    jiraiya.x = 500;
    jiraiya.team = 2;
    jiraiya.action = 553;

    auto second_anko = anko;
    second_anko.slot = 2;
    config.combatants = {anko, jiraiya, second_anko};
    return config;
}

} // namespace

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: c053_natural_double_audio_probe <formal_runtime> <new_csv_path>\n";
        return 2;
    }
    try {
        const std::filesystem::path output_path(argv[2]);
        if (std::filesystem::exists(output_path)) return 3;
        std::filesystem::create_directories(output_path.parent_path());
        std::ofstream output(output_path, std::ios::binary);
        if (!output) return 4;
        output << "tick,ordinal,source,channel,world_x,resource_path,"
                  "child_action,child_hp,hit_count,applied_uj";
#ifdef NTSD_Q10_STEREO_CAMERA_AUDIT
        output << ",camera_x";
#endif
        output << '\n';

        ntsd28_playable::GameSession28 session(argv[1], argv[1]);
        std::string error;
        if (!session.initialize(make_config(), error))
            throw std::runtime_error("initialize: " + error);
        const auto* initial = session.world();
        if (!initial || !initial->entity(0) || !initial->entity(1) ||
            !initial->entity(2) || initial->entity(0)->frame.action != 511 ||
            initial->entity(1)->frame.action != 553 ||
            initial->entity(2)->frame.action != 511)
            throw std::runtime_error("controlled three-combatant initial state differs");

        for (int tick = 1; tick <= 12; ++tick) {
            for (std::size_t slot = 0; slot < 3; ++slot)
                session.set_input(slot, ntsd28::InputButtons28{});
            session.step();
            const auto* world = session.world();
            const auto* result = session.last_tick();
            if (!world || !result) throw std::runtime_error("missing full tick");
            const auto* child = world->entity(50);
            const int child_action = child && child->object_id == 808
                ? child->frame.action : -1;
            const int child_hp = child && child->object_id == 808
                ? child->current_hp : 0;
            int hit_count = 0;
            int applied_uj = 0;
            for (const auto& hit : result->hits) {
                if (hit.target_slot != 50) continue;
                ++hit_count;
                applied_uj +=
                    hit.status == ntsd28::WorldStandardHitStatus28::applied &&
                    hit.interaction && hit.interaction->effect == 2 &&
                    hit.target_type3_post_hit_action == 156;
            }
            auto write_row = [&](int ordinal,
                                 const ntsd28::WorldAudioEvent28* event) {
                output << tick << ',' << ordinal << ','
                       << (event ? static_cast<int>(event->source) : -1) << ','
                       << (event ? event->native_channel : -1) << ','
                       << (event ? event->world_x : 0) << ','
                       << csv_field(event ? event->resource_path : "") << ','
                       << child_action << ',' << child_hp << ',' << hit_count
                       << ',' << applied_uj;
#ifdef NTSD_Q10_STEREO_CAMERA_AUDIT
                output << ',' << session.camera_x();
#endif
                output << '\n';
            };
            if (result->audio_events.empty()) write_row(-1, nullptr);
            for (std::size_t index = 0; index < result->audio_events.size(); ++index)
                write_row(static_cast<int>(index), &result->audio_events[index]);
            if (tick == 7 && (child_action != 156 || child_hp != 450 ||
                              applied_uj != 2))
                throw std::runtime_error("tick7 natural double Uj guard failed");
        }
        output.close();
        if (!output) return 5;
        std::cout << "ticks=12,double_uj_tick=7\n";
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 6;
    }
}
