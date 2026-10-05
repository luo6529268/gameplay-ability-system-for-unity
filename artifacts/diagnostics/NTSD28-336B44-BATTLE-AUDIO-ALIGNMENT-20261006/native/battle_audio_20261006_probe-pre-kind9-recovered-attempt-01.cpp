#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

#include <array>
#include <iomanip>
#include <iostream>
#include <limits>
#include <memory>
#include <sstream>
#include <stdexcept>
#include <string>
#include <utility>
#include <vector>

namespace {

constexpr int kAttackerSlot = 0;
constexpr int kTargetSlot = 70;
constexpr int kAttackerX = 300;
constexpr int kTargetX = 700;
constexpr int kFixtureZ = 250;

std::string json_escape(const std::string& value) {
    std::ostringstream escaped;
    for (const unsigned char character : value) {
        switch (character) {
            case '"':
                escaped << "\\\"";
                break;
            case '\\':
                escaped << "\\\\";
                break;
            case '\b':
                escaped << "\\b";
                break;
            case '\f':
                escaped << "\\f";
                break;
            case '\n':
                escaped << "\\n";
                break;
            case '\r':
                escaped << "\\r";
                break;
            case '\t':
                escaped << "\\t";
                break;
            default:
                if (character < 0x20U) {
                    escaped << "\\u" << std::hex << std::setw(4)
                            << std::setfill('0') << static_cast<int>(character)
                            << std::dec << std::setfill(' ');
                } else {
                    escaped << static_cast<char>(character);
                }
                break;
        }
    }
    return escaped.str();
}

const char* audio_source_name(ntsd28::WorldAudioEventSource28 source) {
    using ntsd28::WorldAudioEventSource28;
    switch (source) {
        case WorldAudioEventSource28::builtin_channel:
            return "builtin_channel";
        case WorldAudioEventSource28::frame_sound:
            return "frame_sound";
        case WorldAudioEventSource28::definition_weapon_hit:
            return "definition_weapon_hit";
        case WorldAudioEventSource28::definition_weapon_drop:
            return "definition_weapon_drop";
        case WorldAudioEventSource28::definition_weapon_broken:
            return "definition_weapon_broken";
        case WorldAudioEventSource28::mode_sound:
            return "mode_sound";
        case WorldAudioEventSource28::story_result_stop_all:
            return "story_result_stop_all";
    }
    return "unknown";
}

std::string audio_path(const ntsd28::WorldAudioEvent28& event) {
    return event.resource_path.empty() ? std::string() : event.resource_path;
}

void write_anomalies(std::ostream& output, const std::vector<std::string>& anomalies) {
    output << ",\"anomalies\":[";
    for (std::size_t i = 0; i < anomalies.size(); ++i) {
        if (i != 0) {
            output << ',';
        }
        output << '"' << json_escape(anomalies[i]) << '"';
    }
    output << ']';
}

void write_audio_events(std::ostream& output,
                        const std::vector<ntsd28::WorldAudioEvent28>& events,
                        bool ordinary,
                        int post_fall) {
    output << "[";
    for (std::size_t i = 0; i < events.size(); ++i) {
        if (i != 0) {
            output << ',';
        }
        const auto& event = events[i];
        const bool broken_from_ordinary_attacker =
            ordinary && event.source == ntsd28::WorldAudioEventSource28::definition_weapon_broken;
        const int source_slot = broken_from_ordinary_attacker ? kAttackerSlot : kTargetSlot;
        output << "{\"order\":" << i
               << ",\"source\":" << static_cast<int>(event.source)
               << ",\"sourceName\":\"" << audio_source_name(event.source)
               << "\",\"channel\":" << event.native_channel
               << ",\"resource_path\":\"" << json_escape(audio_path(event))
               << "\",\"source_slot\":" << source_slot
               << ",\"world_x\":" << event.world_x
               << ",\"postFall\":";
        if (post_fall < 0) {
            output << "null";
        } else {
            output << post_fall;
        }
        output << "}";
    }
    output << "]";
}

std::shared_ptr<const ntsd28::DatDocument> parse_dat(const std::string& text,
                                                     const char* label) {
    auto data = std::make_shared<const ntsd28::DatDocument>(
        ntsd28::DatParser{}.parse_text(text));
    if (!data->ok()) {
        throw std::runtime_error(std::string(label) + " DAT rejected");
    }
    return data;
}

std::string ordinary_dat(bool attacker, int attacker_type, int effect, int fall) {
    std::ostringstream data;
    data << "<bmp_begin>\nname: BattleAudioOrdinary\nweapon_hp: 20\n";
    if (attacker && attacker_type == 3) {
        data << "weapon_broken_sound: attacker-broken.wav\n";
    } else if (!attacker) {
        // Ordinary character-hit audio is expected to use builtin channels;
        // retain a target DAT weapon_hit_sound to prove this branch ignores it.
        data << "weapon_hit_sound: ignored-target-hit.wav\n";
    }
    data << "<bmp_end>\n";
    data << "<frame> 0 active\nstate: 0 wait: 100 next: 0 centerx: 3 centery: 5\n";
    if (attacker) {
        data << "itr:\nkind: 0 x: -1000 y: -1000 w: 2000 h: 2000 zwidth: 1000 "
               "z: 0 injury: 7 fall: " << fall
               << " dvx: 0 dvy: 0 dvz: 0 vrest: 4 bdefend: 0 effect: " << effect
               << "\nitr_end:\n";
    } else {
        data << "bdy:\nkind: 0 x: -1000 y: -1000 w: 2000 h: 2000 zwidth: 1000\n"
               "bdy_end:\n";
    }
    data << "<frame_end>\n";
    data << "<frame> 220 response\nstate: 0 wait: 100 next: 0 centerx: 3 centery: 5\n<frame_end>\n";
    data << "<frame> 222 response\nstate: 0 wait: 100 next: 0 centerx: 3 centery: 5\n<frame_end>\n";
    data << "<frame> 224 response\nstate: 0 wait: 100 next: 0 centerx: 3 centery: 5\n<frame_end>\n";
    data << "<frame> 226 response\nstate: 0 wait: 100 next: 0 centerx: 3 centery: 5\n<frame_end>\n";
    return data.str();
}

std::string reduced_dat(int slot,
                        int attacker_type,
                        int pre_state,
                        const std::string& target_hit_sound,
                        const std::string& target_drop_sound) {
    std::ostringstream data;
    data << "<bmp_begin>\nname: BattleAudioReduced\nweapon_hp: 23\n";
    if (slot == kAttackerSlot && attacker_type == 3) {
        data << "weapon_broken_sound: attacker-reduced-broken.wav\n";
    }
    if (slot == kTargetSlot && !target_hit_sound.empty()) {
        data << "weapon_hit_sound: " << target_hit_sound << "\n";
    }
    if (slot == kTargetSlot && !target_drop_sound.empty()) {
        data << "weapon_drop_sound: " << target_drop_sound << "\n";
    }
    data << "<bmp_end>\n";
    if (slot == kTargetSlot) {
        data << "<armor>\n"
             << "type: 1 ratio: 15 decrease: 50 mp: 0 hp: 0 fall: -1 "
                "bdefend: -1 injury: -1 delay: -1 state: " << pre_state
             << " spark: 199\n<armor_end>\n";
    }
    data << "<frame> 0 active\nstate: " << (slot == kTargetSlot ? pre_state : 0)
         << " wait: 100 next: 0 centerx: 3 centery: 5\n";
    if (slot == kAttackerSlot) {
        data << "itr:\nkind: 0 x: -1000 y: -1000 w: 2000 h: 2000 zwidth: 1000 "
               "z: 0 injury: 7 fall: 0 dvx: 0 dvy: 0 dvz: 0 vrest: 4 "
               "bdefend: 0 effect: 0\nitr_end:\n";
    } else if (slot == kTargetSlot) {
        data << "bdy:\nkind: 0 x: -1000 y: -1000 w: 2000 h: 2000 zwidth: 1000\n"
               "bdy_end:\n";
    }
    data << "<frame_end>\n";
    if (slot == kTargetSlot && pre_state != 0) {
        data << "<frame> " << pre_state << " current\nstate: " << pre_state
             << " wait: 100 next: 0 centerx: 3 centery: 5\n"
                "bdy:\nkind: 0 x: -1000 y: -1000 w: 2000 h: 2000 zwidth: 1000\n"
                "bdy_end:\n<frame_end>\n";
    }
    return data.str();
}

void prepare_hit_world(ntsd28::BattleWorld28& world,
                       int attacker_type,
                       int target_type,
                       const std::string& attacker_dat,
                       const std::string& target_dat,
                       bool target_has_armor,
                       int target_action,
                       int initial_reaction_timer) {
    world.random().reset_from_seed(42);
    const auto attacker_definition = parse_dat(attacker_dat, "attacker");
    const auto target_definition = parse_dat(target_dat, "target");

    ntsd28::SpawnRequest28 attacker_request;
    attacker_request.object_id = 801;
    attacker_request.object_type = attacker_type;
    attacker_request.definition = attacker_definition;
    attacker_request.initial_action = 0;
    attacker_request.position.x = kAttackerX;
    attacker_request.position.z = kFixtureZ;
    attacker_request.hp = 500;
    attacker_request.mp = 500;
    attacker_request.battle_group = 1;
    if (!world.spawn_at(kAttackerSlot, attacker_request).success) {
        throw std::runtime_error("attacker spawn failed");
    }

    ntsd28::SpawnRequest28 target_request;
    target_request.object_id = 802;
    target_request.object_type = target_type;
    target_request.definition = target_definition;
    target_request.initial_action = target_action;
    target_request.position.x = kTargetX;
    target_request.position.z = kFixtureZ;
    target_request.hp = 500;
    target_request.mp = 500;
    target_request.battle_group = 2;
    if (!world.spawn_at(kTargetSlot, target_request).success) {
        throw std::runtime_error("target spawn failed");
    }

    auto& attacker = *world.entity(kAttackerSlot);
    auto& target = *world.entity(kTargetSlot);
    attacker.frame.facing = false;
    target.frame.facing = true;
    target.frame.action = target_action;
    target.frame.frame_counter = 0;
    target.position.y = 0;
    target.collision_y_reference = 0;
    target.hit_reaction_timer = initial_reaction_timer;
    target.runtime_armor_hp = target_has_armor ? 3 : 0;
    target.bdefend_accumulator = 0;

    world.snapshot_actions();
    const auto candidates = world.rebuild_geometric_hit_candidates();
    if (!candidates.success) {
        throw std::runtime_error("geometric candidate rebuild failed");
    }
}

void validate_ordinary_audio(const std::vector<ntsd28::WorldAudioEvent28>& events,
                             int attacker_type,
                             int effect,
                             int expected_post_fall,
                             std::vector<std::string>& anomalies) {
    const bool has_post_effect_audio = effect == 2 || effect == 3 || effect == 20 ||
                                       effect == 21 || effect == 22 || effect == 23 ||
                                       effect == 30;
    const std::size_t expected_count = attacker_type == 3
                                           ? 2U
                                           : (effect == 1 ? 3U : (has_post_effect_audio ? 2U : 1U));
    if (events.size() != expected_count) {
        anomalies.push_back("unexpected ordinary event count");
        return;
    }
    std::size_t index = 0;
    if (attacker_type == 3) {
        const auto& broken = events[index++];
        if (broken.source != ntsd28::WorldAudioEventSource28::definition_weapon_broken ||
            broken.resource_path != "attacker-broken.wav" ||
            broken.world_x != kAttackerX) {
            anomalies.push_back("ordinary type3 broken event is not first at attacker X");
        }
    }
    const int base_channel = expected_post_fall == 80 ? 2 : 0;
    const int effect_channel = expected_post_fall == 80 ? 12 : 11;
    const auto check_builtin = [&](const ntsd28::WorldAudioEvent28& event, int channel, const char* label) {
        if (event.source != ntsd28::WorldAudioEventSource28::builtin_channel ||
            event.native_channel != channel || event.resource_path.size() != 0 ||
            event.world_x != kTargetX) {
            anomalies.push_back(std::string("ordinary ") + label + " event mismatch");
        }
    };
    check_builtin(events[index++], base_channel, attacker_type == 3 ? "base after broken" : "base");
    if (effect == 1) {
        check_builtin(events[index++], effect_channel, "effect after base");
        check_builtin(events[index++], base_channel, "base after effect");
    } else if (has_post_effect_audio) {
        const int post_channel = effect == 3 || effect == 30 ? 14 : 16;
        check_builtin(events[index++], post_channel, "post-effect");
    }
}

void validate_reduced_audio(const std::vector<ntsd28::WorldAudioEvent28>& events,
                            int attacker_type,
                            int pre_state,
                            const std::string& target_hit_sound,
                            const std::string& target_drop_sound,
                            std::vector<std::string>& anomalies) {
    if (events.size() != 1U) {
        anomalies.push_back("unexpected reduced event count");
        return;
    }
    const auto& event = events.front();
    if (event.world_x != kTargetX) {
        anomalies.push_back("reduced event world X is not target X");
    }
    if (attacker_type == 3) {
        if (event.source != ntsd28::WorldAudioEventSource28::definition_weapon_broken ||
            event.resource_path != "attacker-reduced-broken.wav") {
            anomalies.push_back("reduced type3 broken resource mismatch");
        }
        return;
    }
    const bool drop_state = pre_state == 7 || pre_state == 70 || pre_state == 75;
    const std::string expected_path = drop_state ? target_drop_sound : target_hit_sound;
    if (expected_path.empty()) {
        if (event.source != ntsd28::WorldAudioEventSource28::builtin_channel ||
            event.native_channel != 1 || !event.resource_path.empty()) {
            anomalies.push_back("reduced empty sound did not use builtin fallback channel 1");
        }
    } else {
        const auto expected_source = drop_state
                                          ? ntsd28::WorldAudioEventSource28::definition_weapon_drop
                                          : ntsd28::WorldAudioEventSource28::definition_weapon_hit;
        if (event.source != expected_source || event.native_channel != -1 ||
            event.resource_path != expected_path) {
            anomalies.push_back("reduced DAT sound source/path mismatch");
        }
    }
}

void emit_case_error(const std::string& group,
                     int index,
                     const std::string& params,
                     const std::string& error) {
    std::cout << "{\"index\":" << index << ",\"group\":\"" << group
              << "\",\"executed\":false,\"params\":" << params
              << ",\"audio_events\":[],\"anomalies\":[\""
              << json_escape(error) << "\"]}\n";
}

void emit_ordinary_case(int index,
                        const char* label,
                        int attacker_type,
                        int effect,
                        int initial_reaction_timer,
                        int fall) {
    const std::string params =
        std::string("{\"attackerX\":300,\"targetX\":700,\"attackerType\":") +
        std::to_string(attacker_type) + ",\"effect\":" + std::to_string(effect) +
        ",\"initialReaction\":" + std::to_string(initial_reaction_timer) +
        ",\"itrFall\":" + std::to_string(fall) +
        ",\"targetWeaponHitSound\":\"ignored-target-hit.wav\"}";
    try {
        ntsd28::BattleWorld28 world;
        prepare_hit_world(world,
                          attacker_type,
                          0,
                          ordinary_dat(true, attacker_type, effect, fall),
                          ordinary_dat(false, attacker_type, effect, fall),
                          false,
                          0,
                          initial_reaction_timer);
        std::vector<std::string> anomalies;
        if (world.entity(kAttackerSlot)->hit_candidates.size() != 1U) {
            anomalies.push_back("ordinary fixture did not produce exactly one candidate");
        }
        const auto result = world.resolve_ordinary_unarmored_standard_hit(kAttackerSlot, 0);
        const int post_fall = world.entity(kTargetSlot)->hit_reaction_timer;
        if (result.status != ntsd28::WorldStandardHitStatus28::applied) {
            anomalies.push_back("ordinary result was not applied");
        }
        if (post_fall != (initial_reaction_timer == 79 ? 80 : 20)) {
            anomalies.push_back("ordinary postFall did not match fixture target");
        }
        validate_ordinary_audio(result.audio_events,
                                attacker_type,
                                effect,
                                post_fall,
                                anomalies);
        std::cout << "{\"index\":" << index << ",\"group\":\"ordinary\",\"label\":\""
                  << label << "\",\"executed\":true,\"status\":"
                  << static_cast<int>(result.status) << ",\"statusName\":\""
                  << (result.status == ntsd28::WorldStandardHitStatus28::applied ? "applied" : "not_applied")
                  << "\",\"message\":\"" << json_escape(result.message)
                  << "\",\"params\":" << params << ",\"postFall\":" << post_fall
                  << ",\"audio_events\":";
        write_audio_events(std::cout, result.audio_events, true, post_fall);
        write_anomalies(std::cout, anomalies);
        std::cout << "}\n";
    } catch (const std::exception& error) {
        emit_case_error("ordinary", index, params, error.what());
    }
}

void emit_reduced_case(int index,
                       int attacker_type,
                       int pre_state,
                       const char* sound_kind) {
    const std::string target_hit_sound = std::string(sound_kind) == "hit"
                                             ? "target-hit.wav"
                                             : "";
    const std::string target_drop_sound = std::string(sound_kind) == "drop"
                                              ? "target-drop.wav"
                                              : "";
    const std::string params =
        std::string("{\"attackerX\":300,\"targetX\":700,\"attackerType\":") +
        std::to_string(attacker_type) + ",\"victimPreState\":" + std::to_string(pre_state) +
        ",\"datSounds\":\"" + sound_kind + "\"}";
    try {
        ntsd28::BattleWorld28 world;
        prepare_hit_world(world,
                          attacker_type,
                          0,
                          reduced_dat(kAttackerSlot, attacker_type, pre_state, "", ""),
                          reduced_dat(kTargetSlot, attacker_type, pre_state, target_hit_sound, target_drop_sound),
                          true,
                          pre_state,
                          30);
        std::vector<std::string> anomalies;
        if (world.entity(kAttackerSlot)->hit_candidates.size() != 1U) {
            anomalies.push_back("reduced fixture did not produce exactly one candidate");
        }
        const auto result = world.resolve_ordinary_type1_armor_standard_hit(kAttackerSlot, 0, {});
        const int post_fall = world.entity(kTargetSlot)->hit_reaction_timer;
        if (result.status != ntsd28::WorldStandardHitStatus28::applied) {
            anomalies.push_back("reduced result was not applied");
        }
        validate_reduced_audio(result.audio_events,
                               attacker_type,
                               pre_state,
                               target_hit_sound,
                               target_drop_sound,
                               anomalies);
        std::cout << "{\"index\":" << index << ",\"group\":\"reduced\",\"executed\":true"
                  << ",\"status\":" << static_cast<int>(result.status)
                  << ",\"statusName\":\""
                  << (result.status == ntsd28::WorldStandardHitStatus28::applied ? "applied" : "not_applied")
                  << "\",\"message\":\"" << json_escape(result.message)
                  << "\",\"params\":" << params << ",\"postFall\":" << post_fall
                  << ",\"audio_events\":";
        write_audio_events(std::cout, result.audio_events, false, post_fall);
        write_anomalies(std::cout, anomalies);
        std::cout << "}\n";
    } catch (const std::exception& error) {
        emit_case_error("reduced", index, params, error.what());
    }
}

std::string dash_dat() {
    std::ostringstream data;
    data << "<bmp_begin>\n"
         << "name: BattleAudioDash walking_speed: 2 walking_speedz: 0 "
            "running_speed: 5 running_speedz: 0 walking_frame_rate: 1 "
            "running_frame_rate: 1 dash_distance: 23 dash_height: -10 "
            "dash_distancez: 3\n"
         << "<bmp_end>\n"
         << "<frame> 0 idle\n"
            "state: 0 wait: 100 next: 0 centerx: 39 centery: 79\n"
            "<frame_end>\n"
         << "<frame> 9 running\n"
            "state: 2 wait: 100 next: 9 centerx: 39 centery: 79\n"
            "<frame_end>\n"
         << "<frame> 210 jump\n"
            "state: 4 wait: 1 next: 213 centerx: 39 centery: 79 "
            "sound: data\\017.wav\n"
            "<frame_end>\n"
         << "<frame> 213 dash\n"
            "state: 5 wait: 1 next: 215 centerx: 39 centery: 79 "
            "sound: c\\nar\\w\\a7.wav\n"
            "<frame_end>\n"
         << "<frame> 215 crouch\n"
            "state: 5 wait: 2 next: 216 centerx: 39 centery: 79 "
            "sound: data\\012.wav\n"
            "<frame_end>\n"
         << "<frame> 216 dash\n"
            "state: 5 wait: 100 next: 216 centerx: 39 centery: 79\n"
            "<frame_end>\n";
    return data.str();
}

void write_dash_audio_events(
    std::ostream& output,
    const std::vector<ntsd28::WorldAudioEvent28>& events) {
    output << "[";
    for (std::size_t index = 0; index < events.size(); ++index) {
        if (index != 0) output << ',';
        const auto& event = events[index];
        output << "{\"order\":" << index
               << ",\"source\":" << static_cast<int>(event.source)
               << ",\"sourceName\":\"" << audio_source_name(event.source)
               << "\",\"channel\":" << event.native_channel
               << ",\"resource_path\":\"" << json_escape(audio_path(event))
               << "\",\"source_slot\":0,\"world_x\":" << event.world_x
               << ",\"postFall\":null}";
    }
    output << "]";
}

struct DashTickObservation {
    int tick = 0;
    int action_before_input = 0;
    bool right = false;
    bool jump = false;
    ntsd28::SimulationTickResult28 result;
    int action_after_tick = 0;
    int frame_counter_after_tick = 0;
    double world_x_after_tick = 0.0;
};

void write_dash_input_actions(std::ostream& output,
                              const ntsd28::SimulationTickResult28& result) {
    output << "[";
    if (!result.inputs.empty()) {
        const auto& actions = result.inputs[kAttackerSlot].actions;
        for (std::size_t index = 0; index < actions.size(); ++index) {
            if (index != 0) output << ',';
            const auto& action = actions[index];
            output << "{\"order\":" << index
                   << ",\"source\":\"" << json_escape(action.source)
                   << "\",\"requested\":" << action.requested_action
                   << ",\"resolved\":" << action.resolved_action
                   << ",\"attempted\":" << (action.attempted ? "true" : "false")
                   << ",\"applied\":" << (action.applied ? "true" : "false")
                   << ",\"message\":\"" << json_escape(action.message)
                   << "\"}";
        }
    }
    output << "]";
}

void write_dash_frame_events(std::ostream& output,
                             const ntsd28::SimulationTickResult28& result) {
    output << "[";
    for (std::size_t index = 0; index < result.frames.size(); ++index) {
        if (index != 0) output << ',';
        const auto& frame = result.frames[index];
        output << "{\"order\":" << index
               << ",\"slot\":" << frame.slot
               << ",\"from\":" << frame.frame.from_action
               << ",\"to\":" << frame.frame.to_action
               << ",\"status\":" << static_cast<int>(frame.frame.status)
               << ",\"audio_events\":";
        write_dash_audio_events(output, frame.audio_events);
        output << "}";
    }
    output << "]";
}

void emit_dash_case(int index) {
    const std::string params =
        "{\"attackerX\":300,\"targetX\":700,\"objectType\":0,"
        "\"initialAction\":9,\"declaredFrames\":[9,210,213,215,216],"
        "\"inputRoute\":\"SimulationTickDriver28.step -> "
        "InputRouter28.sample_pending/step_sampled\","
        "\"inputSequence\":[\"right+jump\",\"right\",\"right+jump\","
        "\"right\",\"neutral\",\"neutral\",\"neutral\"]}";
    try {
        const auto definition = parse_dat(dash_dat(), "dash");
        ntsd28::BattleWorld28 world;
        world.random().reset_from_seed(42);
        // Use the native 1tu sampling cadence so every fixture tick reaches
        // the phase-zero pending-input copy before step_sampled.
        world.set_one_tu_4a9ff0(true);
        ntsd28::SpawnRequest28 request;
        request.object_id = 804;
        request.object_type = 0;
        request.definition = definition;
        request.initial_action = 9;
        request.position.x = kAttackerX;
        request.position.y = 0;
        request.position.z = kFixtureZ;
        request.hp = 500;
        request.mp = 500;
        request.battle_group = 1;
        if (!world.spawn_at(kAttackerSlot, request).success) {
            throw std::runtime_error("dash spawn failed");
        }

        auto& entity = *world.entity(kAttackerSlot);
        entity.frame.action = 9;
        entity.frame.action_latch = 9;
        entity.frame.previous_action_078 = 9;
        entity.frame.tick_action_snapshot = 9;
        entity.frame.frame_counter = 0;
        entity.frame.facing = false;
        // The native four-phase fallback increments this counter before
        // selecting the first running frame; -1 makes the controlled fixture
        // enter its declared action 9 on the first sampled input.
        entity.control_slot_000 = -1;
        entity.sound_action_latch = -1;
        entity.opoint_action_latch = -1;
        entity.collision_y_reference = 0;
        entity.motion = {};
        entity.input = {};

        ntsd28::ObjectDefinitionCatalog28 catalog;
        ntsd28::SimulationTickDriver28 driver;
        ntsd28::SimulationTickOptions28 options;
        std::vector<DashTickObservation> observations;
        const std::array<bool, 7> jumps{{true, false, true, false, false, false, false}};
        const std::array<bool, 7> rights{{true, true, true, true, false, false, false}};
        for (int tick = 0; tick < static_cast<int>(jumps.size()); ++tick) {
            if (tick >= 4) entity.motion.x = 0.0;
            entity.input.pending = {};
            entity.input.pending.set(ntsd28::InputKey28::right, rights[tick]);
            entity.input.pending.set(ntsd28::InputKey28::jump, jumps[tick]);
            const int action_before_input = entity.frame.action;
            auto result = driver.step(world, catalog, options);
            const auto* after = world.entity(kAttackerSlot);
            if (after == nullptr) {
                throw std::runtime_error("dash entity disappeared");
            }
            observations.push_back(DashTickObservation{
                tick,
                action_before_input,
                rights[tick],
                jumps[tick],
                std::move(result),
                after->frame.action,
                after->frame.frame_counter,
                after->position.x,
            });
        }

        std::vector<std::string> anomalies;
        bool saw_run_jump = false;
        bool saw_crouch_jump = false;
        bool saw_channel7 = false;
        bool saw_data017 = false;
        bool saw_a7 = false;
        bool saw_data012 = false;
        bool saw_frame216 = false;
        for (const auto& observation : observations) {
            if (observation.result.inputs.empty()) {
                anomalies.push_back("dash tick omitted slot-0 input result");
                continue;
            }
            for (const auto& action : observation.result.inputs[kAttackerSlot].actions) {
                saw_run_jump = saw_run_jump ||
                               action.source == "native type-0 run jump";
                saw_crouch_jump = saw_crouch_jump ||
                                  action.source == "native action-215 dash jump";
            }
            for (const auto& event : observation.result.audio_events) {
                saw_channel7 = saw_channel7 || event.native_channel == 7;
                saw_data017 = saw_data017 || event.resource_path.find("017") != std::string::npos;
                saw_a7 = saw_a7 || event.resource_path.find("a7.wav") != std::string::npos;
                saw_data012 = saw_data012 || event.resource_path.find("012.wav") != std::string::npos;
                if (event.source != ntsd28::WorldAudioEventSource28::frame_sound ||
                    event.native_channel != -1) {
                    anomalies.push_back("dash emitted a non-frame or native-channel audio event");
                }
            }
            saw_frame216 = saw_frame216 || observation.action_after_tick == 216;
        }
        if (!saw_run_jump) anomalies.push_back("dash input did not route frame9 state2 to action213");
        if (!saw_crouch_jump) anomalies.push_back("dash input did not route action215 jump");
        if (saw_channel7) anomalies.push_back("dash emitted unexpected channel7 audio");
        if (saw_data017) anomalies.push_back("dash emitted unexpected data017 audio");
        if (!saw_a7) anomalies.push_back("dash did not publish declared frame213 a7 sound");
        if (!saw_data012) anomalies.push_back("dash did not publish declared frame215 data012 sound");
        if (!saw_frame216) anomalies.push_back("dash did not reach declared frame216");

        std::cout << "{\"index\":" << index
                  << ",\"group\":\"dash\",\"executed\":true"
                  << ",\"statusName\":\"success\",\"params\":" << params
                  << ",\"ticks\":[";
        for (std::size_t index = 0; index < observations.size(); ++index) {
            if (index != 0) std::cout << ',';
            const auto& observation = observations[index];
            std::cout << "{\"tick\":" << observation.tick
                      << ",\"action_before_input\":"
                      << observation.action_before_input
                      << ",\"right\":" << (observation.right ? "true" : "false")
                      << ",\"jump\":" << (observation.jump ? "true" : "false")
                      << ",\"input_actions\":";
            write_dash_input_actions(std::cout, observation.result);
            std::cout << ",\"action_after_tick\":" << observation.action_after_tick
                      << ",\"frame_counter_after_tick\":"
                      << observation.frame_counter_after_tick
                      << ",\"world_x_after_tick\":" << observation.world_x_after_tick
                      << ",\"audio_events\":";
            write_dash_audio_events(std::cout, observation.result.audio_events);
            std::cout << ",\"frame_events\":";
            write_dash_frame_events(std::cout, observation.result);
            std::cout << "}";
        }
        std::cout << "]";
        std::cout << ",\"observed\":{\"sawRunJumpAction213\":"
                  << (saw_run_jump ? "true" : "false")
                  << ",\"sawAction215DashJump\":"
                  << (saw_crouch_jump ? "true" : "false")
                  << ",\"sawChannel7\":" << (saw_channel7 ? "true" : "false")
                  << ",\"sawData017\":" << (saw_data017 ? "true" : "false")
                  << ",\"sawFrame213A7\":" << (saw_a7 ? "true" : "false")
                  << ",\"sawFrame215Data012\":"
                  << (saw_data012 ? "true" : "false")
                  << ",\"sawFrame216\":" << (saw_frame216 ? "true" : "false") << "}"
                  << ",\"anomalies\":[";
        for (std::size_t anomaly = 0; anomaly < anomalies.size(); ++anomaly) {
            if (anomaly != 0) std::cout << ',';
            std::cout << '"' << json_escape(anomalies[anomaly]) << '"';
        }
        std::cout << "]}\n";
    } catch (const std::exception& error) {
        emit_case_error("dash", index, params, error.what());
    }
}

void emit_landing_case(int index) {
    const std::string params =
        "{\"objectType\":2,\"objectX\":700,\"channelExpectation\":4,\"vy\":10,\"y\":-1}";
    try {
        std::ostringstream data;
        data << "<bmp_begin>\nname: BattleAudioLanding weapon_hp: 32 weapon_drop_hurt: 4\n"
             << "<bmp_end>\n"
             << "<frame> 0 initial\nstate: 1000 wait: 100 next: 0 centerx: 39 centery: 79\n"
             << "<frame_end>\n";
        const auto definition = parse_dat(data.str(), "landing");
        ntsd28::BattleWorld28 world;
        world.random().reset_from_seed(42);
        ntsd28::SpawnRequest28 request;
        request.object_id = 803;
        request.object_type = 2;
        request.definition = definition;
        request.initial_action = 0;
        request.position.x = kTargetX;
        request.position.y = -1;
        request.position.z = kFixtureZ;
        request.hp = 500;
        request.mp = 500;
        request.battle_group = 2;
        if (!world.spawn_at(kTargetSlot, request).success) {
            throw std::runtime_error("landing spawn failed");
        }
        auto& entity = *world.entity(kTargetSlot);
        entity.weapon_hp_31c = 20;
        entity.collision_y_reference = 0;
        entity.motion.x = 0;
        entity.motion.y = 10.0;
        entity.motion.z = 0;
        const auto result = world.step_physics(kTargetSlot, {});
        std::vector<std::string> anomalies;
        if (!result.success) {
            anomalies.push_back("landing physics result was not successful");
        }
        if (!result.physics.landing_sound_channel4) {
            anomalies.push_back("landing physics did not set channel4 flag");
        }
        if (result.audio_events.size() != 1U ||
            result.audio_events.front().source != ntsd28::WorldAudioEventSource28::builtin_channel ||
            result.audio_events.front().native_channel != 4 ||
            result.audio_events.front().world_x != kTargetX) {
            anomalies.push_back("landing audio event is not builtin channel4 at target X");
        }
        std::cout << "{\"index\":" << index << ",\"group\":\"landing\",\"executed\":true"
                  << ",\"statusName\":\"" << (result.success ? "success" : "failed")
                  << "\",\"params\":" << params << ",\"postFall\":null,\"audio_events\":";
        write_audio_events(std::cout, result.audio_events, false, -1);
        write_anomalies(std::cout, anomalies);
        std::cout << "}\n";
    } catch (const std::exception& error) {
        emit_case_error("landing", index, params, error.what());
    }
}

}  // namespace

int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        std::cout << "{\"kind\":\"header\",\"schema\":\"NTSD28-battle-audio-20261006-v1\","
                     "\"authority\":\"current-native-Core-source-limited\","
                     "\"fixture\":\"in-memory-DAT\",\"slots\":{\"attacker\":0,\"victim\":70},"
                     "\"positions\":{\"attackerX\":300,\"victimX\":700},"
                     "\"source_slot_note\":\"WorldAudioEvent28 carries no source slot; emitted source_slot is inferred from the native append branch and fixed fixture slots.\","
                     "\"optional_frame_sound\":\"dash_in_memory_core_fixture_appended\"}\n";

        int index = 0;
        emit_ordinary_case(index++, "effect0_postFall20", 0, 0, 0, 0);
        emit_ordinary_case(index++, "effect1_postFall20", 0, 1, 0, 0);
        emit_ordinary_case(index++, "effect0_postFall80", 0, 0, 79, 1);
        emit_ordinary_case(index++, "effect1_postFall80", 0, 1, 79, 1);
        emit_ordinary_case(index++, "type3_broken_first_ownX", 3, 0, 0, 0);
        emit_ordinary_case(index++, "effect2_postAudio16", 0, 2, 0, 0);
        emit_ordinary_case(index++, "effect3_postAudio14", 0, 3, 0, 0);
        emit_ordinary_case(index++, "effect23_postAudio16", 0, 23, 0, 0);

        for (int pre_state : {0, 7, 70, 75}) {
            emit_reduced_case(index++, 0, pre_state, "hit");
            emit_reduced_case(index++, 0, pre_state, "drop");
            emit_reduced_case(index++, 0, pre_state, "empty");
        }
        emit_reduced_case(index++, 3, 0, "hit");
        emit_landing_case(index++);
        emit_dash_case(index++);
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 91;
    }
}
