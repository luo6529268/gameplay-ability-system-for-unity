#define main ntsd_legacy_dat_preview_main
#include "J:/QQFile/NTSD2.4/ntsd_cpp/src/core/dat_preview_cli.cpp"
#undef main

#include "input_handler.h"

#include <algorithm>
#include <map>
#include <set>
#include <sstream>

namespace {

struct PreviewInputStep {
    int tick = 0;
    std::string tokens;
};

struct PreviewCatalogEntry {
    int oid = -1;
    int type = 0;
    std::string file;
};

struct PreviewCatalogStats {
    int entries = 0;
    int loaded = 0;
    int failed = 0;
};

const char* DEFAULT_PREVIEW_GAME_ROOT = "J:\\QQFile\\NTSD 2.4.1";

std::string preview_game_path(const std::string& game_root, const std::string& relative_path) {
    if (game_root.empty()) return relative_path;
    const char last = game_root.back();
    return last == '\\' || last == '/' ? game_root + relative_path : game_root + "\\" + relative_path;
}

std::vector<PreviewCatalogEntry> parse_preview_catalog(const char* path) {
    std::vector<PreviewCatalogEntry> entries;
    FILE* input = std::fopen(path, "rb");
    if (!input) return entries;

    char line[512];
    bool in_object = false;
    while (std::fgets(line, sizeof(line), input)) {
        const char* cursor = line;
        while (*cursor == ' ' || *cursor == '\t') ++cursor;
        if (*cursor == '#' || *cursor == '\n' || *cursor == '\r' || *cursor == '\0') continue;
        if (*cursor == '<') {
            in_object = std::strncmp(cursor, "<object>", 8) == 0;
            continue;
        }
        if (!in_object) continue;

        int oid = -1;
        int type = 0;
        char file[256] = {};
        if (std::sscanf(cursor, "id: %d", &oid) != 1) continue;
        const char* type_cursor = std::strstr(cursor, "type:");
        const char* file_cursor = std::strstr(cursor, "file:");
        if (!type_cursor || !file_cursor || std::sscanf(type_cursor, "type: %d", &type) != 1) continue;

        file_cursor += 5;
        while (*file_cursor == ' ' || *file_cursor == '\t') ++file_cursor;
        int length = 0;
        while (*file_cursor && *file_cursor != '\n' && *file_cursor != '\r' &&
               *file_cursor != '#' && length < 255) {
            file[length++] = *file_cursor++;
        }
        while (length > 0 && (file[length - 1] == ' ' || file[length - 1] == '\t')) --length;
        file[length] = '\0';
        if (oid >= 0 && file[0]) entries.push_back({oid, type, file});
    }
    std::fclose(input);
    return entries;
}

bool find_preview_stage_background_path(const std::string& game_root, int stage, std::string& result) {
    const std::string data_path = preview_game_path(game_root, "data\\data.txt");
    FILE* data = std::fopen(data_path.c_str(), "rb");
    if (!data) return false;

    char line[512];
    bool in_background = false;
    while (std::fgets(line, sizeof(line), data)) {
        const char* cursor = line;
        while (*cursor == ' ' || *cursor == '\t') ++cursor;
        if (std::strncmp(cursor, "<background>", 12) == 0) {
            in_background = true;
            continue;
        }
        if (*cursor == '<' && in_background) break;
        if (!in_background) continue;

        int id = -1;
        char path[256] = {};
        if (std::sscanf(cursor, "id: %d file: %255s", &id, path) == 2 && id == stage) {
            result = path;
            std::fclose(data);
            return true;
        }
    }
    std::fclose(data);
    return false;
}

bool load_preview_stage_background(GameWorld& world,
                                   const std::string& game_root,
                                   int stage,
                                   StageInfo& info) {
    std::string relative_path;
    if (!find_preview_stage_background_path(game_root, stage, relative_path)) return false;
    const std::string full_path = preview_game_path(game_root, relative_path);
    const std::vector<uint8_t> raw = dat_decrypt(full_path.c_str());
    if (raw.empty()) return false;

    const BgData background = parse_bg(raw);
    if (background.width <= 0 || background.zboundary_max < background.zboundary_min) return false;
    world.bg = background;
    world.stage_idx = stage;
    world.bound_left = 0;
    world.bound_right = world.bg.width;
    info.index = stage;
    info.data_path = relative_path;
    return true;
}

bool initialize_preview_world(GameWorld& world,
                              const Options& options,
                              const std::string& game_root,
                              StageInfo& stage_info,
                              PreviewCatalogStats& catalog_stats) {
    clear_rest_arrays();
    ntsd_srand(options.seed);

    const std::string data_path = preview_game_path(game_root, "data\\data.txt");
    const std::vector<PreviewCatalogEntry> catalog = parse_preview_catalog(data_path.c_str());
    catalog_stats.entries = static_cast<int>(catalog.size());
    if (catalog.empty()) return false;

    for (const PreviewCatalogEntry& entry : catalog) {
        if (world.has_char(entry.oid)) continue;
        const std::string full_path = preview_game_path(game_root, entry.file);
        const bool loaded = entry.oid == 2 && !options.naruto_dat.empty()
            ? load_plaintext_char(world, entry.oid, options.naruto_dat.c_str(), entry.type)
            : load_encrypted_char(world, entry.oid, full_path.c_str(), entry.type);
        if (loaded) ++catalog_stats.loaded;
        else ++catalog_stats.failed;
    }

    if (!world.has_char(1) || !world.has_char(2) ||
        !load_preview_stage_background(world, game_root, options.stage, stage_info)) {
        return false;
    }

    for (int i = 0; i < MAX_OBJECTS; ++i) {
        world.objects[i].reset();
        world.objects[i].slot = i;
    }

    world.object_count = 2;
    world.game_mode = 1;
    world.game_tick = 0;
    world.input_phase = 0;
    world.camera_x = 0;
    world.camera_vel = 0;

    Entity& p1 = world.objects[0];
    seed_character(p1, 0, 2, world.get_char(2), 1, 0, 0, options.p1_x, options.p1_y, options.p1_z);
    p1.frame = p1.prev_frame = p1.prev_frame2 = options.start_frame;
    p1.wait_counter = options.start_frame;
    p1.attacking = 0;
    p1.target_idx = 1;

    Entity& p2 = world.objects[1];
    seed_character(p2, 1, 1, world.get_char(1), 2, 1, 1, options.p2_x, options.p2_y, options.p2_z);
    p2.target_idx = 0;
    return true;
}

bool valid_input_token(const std::string& token) {
    return token == "A" || token == "D" || token == "W" || token == "S" ||
           token == "J" || token == "K" || token == "L";
}

bool parse_input_plan(const char* text, std::vector<PreviewInputStep>& steps) {
    if (!text || !*text) return true;
    std::stringstream plan(text);
    std::string item;
    std::map<int, std::string> by_tick;
    while (std::getline(plan, item, ',')) {
        const std::size_t colon = item.find(':');
        if (colon == std::string::npos || colon == 0 || colon + 1 >= item.size()) return false;

        int tick = 0;
        if (!parse_int(item.substr(0, colon).c_str(), tick) || tick < 1 || tick > 1800) return false;

        std::stringstream tokens(item.substr(colon + 1));
        std::string token;
        std::string normalized;
        while (std::getline(tokens, token, '+')) {
            if (!valid_input_token(token)) return false;
            if (!normalized.empty()) normalized.push_back(' ');
            normalized += token;
        }
        if (normalized.empty() || by_tick.count(tick) != 0) return false;
        by_tick.emplace(tick, normalized);
    }
    for (const auto& [tick, tokens] : by_tick) steps.push_back({tick, tokens});
    return true;
}

const char* tokens_for_tick(const std::vector<PreviewInputStep>& steps, int tick) {
    for (const PreviewInputStep& step : steps) {
        if (step.tick == tick) return step.tokens.c_str();
        if (step.tick > tick) break;
    }
    return "";
}

bool has_input_token(const char* tokens, char expected) {
    if (!tokens) return false;
    for (const char* cursor = tokens; *cursor; ++cursor) {
        if (*cursor == expected &&
            (cursor == tokens || cursor[-1] == ' ') &&
            (cursor[1] == '\0' || cursor[1] == ' ')) {
            return true;
        }
    }
    return false;
}

// The web preview supplies deterministic physical-key samples. This mirrors
// InputHandler::poll exactly, while InputHandler::apply_input remains the sole
// authority for combos, state transitions, velocity and object creation.
void apply_preview_poll(Entity& e, const char* tokens) {
    e.prev_right  = e.key_right;  e.prev_left   = e.key_left;
    e.prev_up     = e.key_up;     e.prev_down   = e.key_down;
    e.prev_attack = e.key_attack; e.prev_jump   = e.key_jump;
    e.prev_defend = e.key_defend;

    e.key_left   = has_input_token(tokens, 'A') ? 1 : 0;
    e.key_right  = has_input_token(tokens, 'D') ? 1 : 0;
    e.key_up     = has_input_token(tokens, 'W') ? 1 : 0;
    e.key_down   = has_input_token(tokens, 'S') ? 1 : 0;
    e.key_jump   = has_input_token(tokens, 'J') ? 1 : 0;
    e.key_defend = has_input_token(tokens, 'K') ? 1 : 0;
    e.key_attack = has_input_token(tokens, 'L') ? 1 : 0;

    if (e.cd_right)       e.cd_right--;
    if (e.cd_left)        e.cd_left--;
    if (e.cd_up)          e.cd_up--;
    if (e.cd_down)        e.cd_down--;
    if (e.cd_jump)        e.cd_jump--;
    if (e.cd_attack)      e.cd_attack--;
    if (e.cd_defend)      e.cd_defend--;
    if (e.cd_defend_lock) e.cd_defend_lock--;

    auto push_history = [&](int key_num) {
        e.input_history[1] = e.input_history[2];
        e.input_history[2] = e.input_history[3];
        e.input_history[3] = e.input_history[4];
        e.input_history[4] = e.input_history[5];
        e.input_history[5] = key_num;
    };

    if (!e.prev_right  && e.key_right  == 1) { e.cd_right  = 5; push_history(6); }
    if (!e.prev_left   && e.key_left   == 1) { e.cd_left   = 5; push_history(4); }
    if (!e.prev_up     && e.key_up     == 1) { e.cd_up     = 5; push_history(8); }
    if (!e.prev_down   && e.key_down   == 1) { e.cd_down   = 5; push_history(2); }
    if (!e.prev_attack && e.key_attack == 1) { e.cd_defend = 5; push_history(9); }
    if (!e.prev_defend && e.key_defend == 1) { e.cd_jump   = 5; push_history(0); }
    if (!e.prev_jump   && e.key_jump   == 1) { e.cd_attack = 5; push_history(5); }
}

bool parse_preview_options(int argc,
                           char** argv,
                           Options& options,
                           std::string& game_root,
                           int& entry_frame,
                           std::vector<PreviewInputStep>& steps) {
    std::vector<char*> legacy_args;
    legacy_args.reserve(static_cast<std::size_t>(argc));
    legacy_args.push_back(argv[0]);
    const char* input_plan = nullptr;
    game_root = DEFAULT_PREVIEW_GAME_ROOT;
    entry_frame = -1;

    for (int i = 1; i < argc; ++i) {
        if (std::strcmp(argv[i], "--entry-frame") == 0) {
            const char* value = nullptr;
            if (!read_option_value(argc, argv, i, argv[i], value) ||
                !parse_int(value, entry_frame) || entry_frame < 0 || entry_frame > 599) {
                return false;
            }
            continue;
        }
        if (std::strcmp(argv[i], "--input-plan") == 0) {
            if (!read_option_value(argc, argv, i, argv[i], input_plan)) return false;
            continue;
        }
        if (std::strcmp(argv[i], "--game-root") == 0) {
            const char* value = nullptr;
            if (!read_option_value(argc, argv, i, argv[i], value) || !value || !*value) return false;
            game_root = value;
            continue;
        }
        legacy_args.push_back(argv[i]);
    }

    if (!parse_options(static_cast<int>(legacy_args.size()), legacy_args.data(), options)) return false;
    if (entry_frame < 0) entry_frame = options.start_frame;
    return parse_input_plan(input_plan, steps);
}

void write_input_metadata(FILE* output,
                          int initial_frame,
                          int entry_frame,
                          const std::vector<PreviewInputStep>& steps) {
    std::fprintf(output, ",\"initial_frame\":%d,\"input\":{\"entry_frame\":%d,\"steps\":[",
                 initial_frame, entry_frame);
    bool first = true;
    for (const PreviewInputStep& step : steps) {
        if (!first) std::fputc(',', output);
        first = false;
        std::fprintf(output, "{\"tick\":%d,\"keys\":", step.tick);
        write_json_string(output, step.tokens);
        std::fputc('}', output);
    }
    std::fputs("]}", output);
}

void write_preview_entity(FILE* output, const Entity& entity) {
    const FrameData* frame = entity.char_data ? entity.char_data->get_frame(entity.frame) : nullptr;
    const int oid = entity.char_data ? entity.char_data->oid : -1;
    const int display_z = entity.char_data && entity.char_data->obj_type == WeaponType::CONSUMABLE3
        ? static_cast<int>(entity.z - entity.type3_visual_z_offset)
        : entity.z_int;
    std::fprintf(output,
                 "{\"slot\":%d,\"oid\":%d,\"frame\":%d,\"pic\":%d,\"facing\":%u,"
                 "\"render_pic\":%d,"
                 "\"x\":%.17g,\"y\":%.17g,\"z\":%.17g,"
                 "\"x_int\":%d,\"y_int\":%d,\"z_int\":%d,\"display_z\":%d,"
                 "\"v\":{\"x\":%.17g,\"y\":%.17g,\"z\":%.17g},"
                 "\"render_offset_x\":%d,\"frame_delay\":%d,\"hit_stop\":%d,\"team\":%d,"
                 "\"target\":%d,\"holder\":%d,\"link\":%d,\"ai\":%s}",
                 entity.slot, oid, entity.frame, frame ? frame->pic : -1, static_cast<unsigned int>(entity.facing),
                 frame ? frame->pic + entity.unk_318 : -1,
                 entity.x, entity.y, entity.z,
                 entity.x_int, entity.y_int, entity.z_int, display_z,
                 entity.vx, entity.vy, entity.vz,
                 entity.render_offset_x, entity.frame_delay, entity.hit_stop, entity.team,
                 entity.target_idx, entity.holder_idx, entity.link_state, entity.ai_controlled ? "true" : "false");
}

void write_preview_snapshot(FILE* output, const GameWorld& world, std::set<int>& observed_oids) {
    std::fprintf(output,
                 "{\"tick\":%d,\"camera_x\":%d,\"camera_vel\":%d,"
                 "\"bg\":{\"width\":%d,\"z_min\":%d,\"z_max\":%d,\"bound_left\":%d,\"bound_right\":%d},\"entities\":[",
                 world.game_tick, world.camera_x, world.camera_vel,
                 world.bg.width, world.bg.zboundary_min, world.bg.zboundary_max, world.bound_left, world.bound_right);
    bool first = true;
    for (int slot = 0; slot < MAX_OBJECTS; ++slot) {
        const Entity& entity = world.objects[slot];
        if (!entity.active) continue;
        if (entity.char_data) observed_oids.insert(entity.char_data->oid);
        if (!first) std::fputc(',', output);
        write_preview_entity(output, entity);
        first = false;
    }
    std::fputs("]}", output);
}

void write_preview_render_resources(FILE* output,
                                    const GameWorld& world,
                                    const std::set<int>& observed_oids) {
    std::fputs(",\"render_resources\":[", output);
    bool first_resource = true;
    for (int oid : world.loaded_oid_order) {
        if (observed_oids.count(oid) == 0) continue;
        const CharData* data = world.get_char(oid);
        if (!data) continue;
        if (!first_resource) std::fputc(',', output);
        first_resource = false;
        std::fprintf(output, "{\"oid\":%d,\"type\":%d,\"name\":", oid, data->obj_type);
        write_json_string(output, data->name);
        std::fputs(",\"ranges\":[", output);
        bool first_range = true;
        for (const SpriteRange& range : data->sprite_ranges) {
            if (!first_range) std::fputc(',', output);
            first_range = false;
            std::fputs("{\"file\":", output);
            write_json_string(output, range.file);
            std::fprintf(output,
                         ",\"frame_lo\":%d,\"frame_hi\":%d,\"w\":%d,\"h\":%d,\"row\":%d,\"col\":%d}",
                         range.frame_lo, range.frame_hi, range.w, range.h, range.row, range.col);
        }
        std::fputs("],\"frames\":[", output);
        bool first_frame = true;
        for (int frame_id = 0; frame_id < CharData::MAX_FRAME_ID; ++frame_id) {
            if (!data->has_frame(frame_id)) continue;
            const FrameData* frame = data->get_frame(frame_id);
            if (!frame) continue;
            if (!first_frame) std::fputc(',', output);
            first_frame = false;
            std::fprintf(output,
                         "{\"frame_id\":%d,\"pic\":%d,\"state\":%d,\"center_x\":%d,\"center_y\":%d}",
                         frame_id, frame->pic, frame->state, frame->centerx, frame->centery);
        }
        std::fputs("]}", output);
    }
    std::fputc(']', output);
}

} // namespace

int main(int argc, char** argv) {
    Options options;
    std::string game_root;
    int entry_frame = -1;
    std::vector<PreviewInputStep> input_steps;
    if (!parse_preview_options(argc, argv, options, game_root, entry_frame, input_steps)) return 2;

    GameWorld world;
    StageInfo stage_info;
    PreviewCatalogStats catalog_stats;
    if (!initialize_preview_world(world, options, game_root, stage_info, catalog_stats)) {
        std::fprintf(stderr, "Failed to load required DAT data or stage background.\n");
        return 3;
    }

    FILE* output = std::fopen(options.output.c_str(), "wb");
    if (!output) {
        std::fprintf(stderr, "Cannot open output: %s\n", options.output.c_str());
        return 4;
    }

    std::fputs("{\"metadata\":{\"runtime\":\"ntsd_cpp\",\"tick_driver\":\"SimulationTickDriver\",\"renderer\":\"none\",\"seed\":", output);
    std::fprintf(output, "%u,\"start_frame\":%d,\"ticks_requested\":%d,\"stage\":{\"index\":%d,\"data_path\":",
                 options.seed, entry_frame, options.ticks, stage_info.index);
    write_json_string(output, stage_info.data_path);
    std::fputs(",\"name\":", output);
    write_json_string(output, world.bg.name);
    std::fprintf(output, ",\"width\":%d,\"z_min\":%d,\"z_max\":%d,\"background\":{\"shadow\":{\"path\":",
                 world.bg.width, world.bg.zboundary_min, world.bg.zboundary_max);
    write_json_string(output, world.bg.shadow_path);
    std::fprintf(output, ",\"width\":%d,\"height\":%d},\"layers\":[", world.bg.shadow_w, world.bg.shadow_h);
    bool first_layer = true;
    for (const BgLayer& layer : world.bg.layers) {
        if (!first_layer) std::fputc(',', output);
        first_layer = false;
        std::fputs("{\"path\":", output);
        write_json_string(output, layer.bmp_path);
        std::fprintf(output,
                     ",\"transparency\":%d,\"parallax_width\":%d,\"x\":%d,\"y\":%d,"
                     "\"loop\":%d,\"cc\":%d,\"c1\":%d,\"c2\":%d,\"anim_counter\":%d}",
                     layer.transparency, layer.parallax_width, layer.x, layer.y,
                     layer.loop, layer.cc, layer.c1, layer.c2, layer.anim_counter);
    }
    std::fputs("]}}", output);
    write_input_metadata(output, options.start_frame, entry_frame, input_steps);
    std::fprintf(output, ",\"catalog\":{\"entries\":%d,\"loaded\":%d,\"failed\":%d}",
                 catalog_stats.entries, catalog_stats.loaded, catalog_stats.failed);
    std::fputs(",\"naruto_dat_override\":", output);
    if (options.naruto_dat.empty()) std::fputs("null", output);
    else write_json_string(output, options.naruto_dat);
    std::fputs(",\"initial\":{\"p1\":{\"x\":", output);
    std::fprintf(output, "%.17g,\"y\":%.17g,\"z\":%.17g},\"p2\":{\"x\":%.17g,\"y\":%.17g,\"z\":%.17g}}}",
                 options.p1_x, options.p1_y, options.p1_z, options.p2_x, options.p2_y, options.p2_z);
    std::fputs(",\"ticks\":[", output);

    BattleTickScheduler scheduler;
    SimulationTickDriver tick_driver(scheduler);
    InputHandler input;
    std::set<int> observed_oids;

    write_preview_snapshot(output, world, observed_oids);
    for (int tick = 0; tick < options.ticks; ++tick) {
        const int next_tick = world.game_tick + 1;
        Entity& p1 = world.objects[0];
        Entity& p2 = world.objects[1];
        if (p1.active) apply_preview_poll(p1, tokens_for_tick(input_steps, next_tick));
        if (p2.active) apply_preview_poll(p2, "");

        tick_driver.step_one_tick(world, nullptr, [&]() {
            if (world.game_tick <= 1) return;
            for (int slot = 0; slot < MAX_OBJECTS; ++slot) {
                Entity& entity = world.objects[slot];
                if (!entity.active || !entity.char_data || entity.char_data->obj_type != 0) continue;
                if (entity.ai_controlled) input.prepare_ai_input(entity, world, world.input_phase);
                input.apply_input(entity, world.camera_x, &world);
            }
        });
        std::fputc(',', output);
        write_preview_snapshot(output, world, observed_oids);
    }
    std::fputc(']', output);
    write_preview_render_resources(output, world, observed_oids);
    std::fputs("}\n", output);
    const int close_result = std::fclose(output);
    return close_result == 0 ? 0 : 5;
}
