#include "ntsd28/collision_geometry.h"
#include "ntsd28/combat_records.h"
#include "ntsd28/dat_parser.h"
#include "ntsd28/object_spawning.h"

#include <algorithm>
#include <cmath>
#include <cstdlib>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <locale>
#include <sstream>
#include <stdexcept>
#include <string>
#include <string_view>
#include <system_error>
#include <vector>

namespace {

using ntsd28::ArmorBlock28;
using ntsd28::ArmorRecord28;
using ntsd28::BmpFrameSequence28;
using ntsd28::CatchPointRecord28;
using ntsd28::DatDocument;
using ntsd28::FieldBag;
using ntsd28::Frame28;
using ntsd28::InteractionRecord28;
using ntsd28::ObjectPointRecord28;
using ntsd28::ParseDiagnostic;
using ntsd28::RawField;
using ntsd28::SubBlock28;
using ntsd28::WeaponPieceBlock28;
using ntsd28::WeaponPieceGroup28;
using ntsd28::WeaponPieceVariant28;
using ntsd28::WeaponPointRecord28;
using ntsd28::WeaponStrengthEntry28;

struct Arguments {
    std::filesystem::path input_root;
    std::filesystem::path output;
};

struct CaptureStats {
    std::size_t files = 0;
    std::size_t parse_success = 0;
    std::size_t parse_failure = 0;
    std::size_t warnings = 0;
    std::size_t errors = 0;
};

template <typename Fn>
void append_member(std::ostream& output,
                   bool& first,
                   std::string_view key,
                   Fn&& append_value) {
    if (!first) {
        output << ',';
    }
    first = false;
    output << '"';
    for (const char character : key) {
        output << character;
    }
    output << "\":";
    append_value(output);
}

std::string json_quote(std::string_view value) {
    std::string result;
    result.reserve(value.size() + 2);
    result.push_back('"');
    static constexpr char hex[] = "0123456789abcdef";
    for (const unsigned char character : value) {
        switch (character) {
        case '"':
            result += "\\\"";
            break;
        case '\\':
            result += "\\\\";
            break;
        case '\b':
            result += "\\b";
            break;
        case '\f':
            result += "\\f";
            break;
        case '\n':
            result += "\\n";
            break;
        case '\r':
            result += "\\r";
            break;
        case '\t':
            result += "\\t";
            break;
        default:
            if (character < 0x20U) {
                result += "\\u00";
                result.push_back(hex[(character >> 4U) & 0x0fU]);
                result.push_back(hex[character & 0x0fU]);
            } else {
                result.push_back(static_cast<char>(character));
            }
            break;
        }
    }
    result.push_back('"');
    return result;
}

std::string path_to_utf8(const std::filesystem::path& path) {
    return path.u8string();
}

std::string normalized_relative_path(const std::filesystem::path& path) {
    std::string result = path.generic_u8string();
    std::replace(result.begin(), result.end(), '\\', '/');
    return result;
}

void append_string(std::ostream& output, std::string_view value) {
    output << json_quote(value);
}

void append_fields(std::ostream& output, const FieldBag& fields) {
    output << '[';
    bool first = true;
    for (const auto& field : fields.fields()) {
        if (!first) {
            output << ',';
        }
        first = false;
        output << '{'
               << "\"context\":" << json_quote(field.context)
               << ",\"key\":" << json_quote(field.key)
               << ",\"value\":" << json_quote(field.value)
               << ",\"line\":" << field.line
               << ",\"column\":" << field.column
               << '}';
    }
    output << ']';
}

void append_diagnostics(std::ostream& output,
                        const std::vector<ParseDiagnostic>& diagnostics) {
    output << '[';
    bool first = true;
    for (const auto& diagnostic : diagnostics) {
        if (!first) {
            output << ',';
        }
        first = false;
        output << "{\"severity\":"
               << json_quote(diagnostic.severity == ParseDiagnostic::Severity::error
                                  ? "error"
                                  : "warning")
               << ",\"line\":" << diagnostic.line
               << ",\"message\":" << json_quote(diagnostic.message)
               << '}';
    }
    output << ']';
}

template <typename T>
void append_number_member(std::ostream& output,
                          bool& first,
                          std::string_view key,
                          T value) {
    append_member(output, first, key, [value](std::ostream& value_output) {
        value_output << value;
    });
}

void append_float_member(std::ostream& output,
                         bool& first,
                         std::string_view key,
                         float value) {
    append_member(output, first, key, [value](std::ostream& value_output) {
        if (!std::isfinite(value)) {
            value_output << '0';
            return;
        }
        value_output << std::setprecision(9) << static_cast<double>(value);
    });
}

void append_integer_array(std::ostream& output, const std::vector<int>& values) {
    output << '[';
    for (std::size_t index = 0; index < values.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        output << values[index];
    }
    output << ']';
}

void append_frame_ranges(std::ostream& output,
                         const std::vector<std::pair<int, int>>& ranges) {
    output << '[';
    for (std::size_t index = 0; index < ranges.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        output << '[' << ranges[index].first << ',' << ranges[index].second << ']';
    }
    output << ']';
}

void append_interaction(std::ostream& output, const InteractionRecord28& record) {
    output << '{';
    bool first = true;
    append_number_member(output, first, "kind", record.kind);
    append_number_member(output, first, "x", record.x);
    append_number_member(output, first, "y", record.y);
    append_number_member(output, first, "w", record.width);
    append_number_member(output, first, "h", record.height);
    append_number_member(output, first, "dvx", record.dvx);
    append_number_member(output, first, "dvy", record.dvy);
    append_number_member(output, first, "fall", record.fall);
    append_number_member(output, first, "arest", record.arest);
    append_number_member(output, first, "vrest", record.vrest);
    append_number_member(output, first, "respond", record.respond);
    append_number_member(output, first, "effect", record.effect);
    append_number_member(output, first, "drain", record.drain);
    append_number_member(output, first, "spark", record.spark);
    append_number_member(output, first, "recover", record.recover);
    append_number_member(output, first, "dbdefend", record.dbdefend);
    append_number_member(output, first, "bdefend", record.bdefend);
    append_number_member(output, first, "injury", record.injury);
    append_number_member(output, first, "zwidth", record.zwidth);
    append_number_member(output, first, "z", record.z);
    append_number_member(output, first, "dvz", record.dvz);
    append_number_member(output, first, "sound", record.sound);
    append_number_member(output, first, "cover", record.cover);
    append_number_member(output, first, "caughtact", record.caughtact);
    append_number_member(output, first, "catchingact", record.catchingact);
    append_number_member(output, first, "pickedact", record.pickedact);
    append_number_member(output, first, "pickingact", record.pickingact);
    append_number_member(output, first, "delay", record.delay);
    append_number_member(output, first, "poison", record.poison);
    append_number_member(output, first, "confus", record.confus);
    append_number_member(output, first, "weak", record.weak);
    append_number_member(output, first, "manacle", record.manacle);
    append_number_member(output, first, "join", record.join);
    append_number_member(output, first, "mimic", record.mimic);
    append_number_member(output, first, "bound", record.bound);
    append_number_member(output, first, "facing", record.facing);
    append_number_member(output, first, "dx", record.dx);
    append_number_member(output, first, "dy", record.dy);
    append_number_member(output, first, "dz", record.dz);
    append_number_member(output, first, "gain", record.gain);
    output << '}';
}

void append_cpoint(std::ostream& output, const CatchPointRecord28& record) {
    output << '{';
    bool first = true;
    append_number_member(output, first, "kind", record.kind);
    append_number_member(output, first, "x", record.x);
    append_number_member(output, first, "y", record.y);
    append_number_member(output, first, "injury", record.injury);
    append_number_member(output, first, "cover", record.cover);
    append_number_member(output, first, "vaction", record.victim_action);
    append_number_member(output, first, "aaction", record.attack_action);
    append_number_member(output, first, "jaction", record.jump_action);
    append_number_member(output, first, "daction", record.defend_action);
    append_number_member(output, first, "taction", record.throw_action);
    append_number_member(output, first, "faction", record.front_action);
    append_number_member(output, first, "baction", record.back_action);
    append_number_member(output, first, "uzaction", record.up_z_action);
    append_number_member(output, first, "dzaction", record.down_z_action);
    append_float_member(output, first, "throwvx", record.throw_vx);
    append_float_member(output, first, "throwvy", record.throw_vy);
    append_number_member(output, first, "hurtable", record.hurtable);
    append_number_member(output, first, "fronthurtact", record.front_hurt_action);
    append_number_member(output, first, "backhurtact", record.back_hurt_action);
    append_number_member(output, first, "decrease", record.decrease);
    append_number_member(output, first, "dircontrol", record.direction_control);
    append_number_member(output, first, "throwinjury", record.throw_injury);
    append_float_member(output, first, "throwvz", record.throw_vz);
    append_number_member(output, first, "z", record.z);
    append_number_member(output, first, "recover", record.recover);
    append_number_member(output, first, "drain", record.drain);
    append_number_member(output, first, "gain", record.gain);
    output << '}';
}

void append_wpoint(std::ostream& output, const WeaponPointRecord28& record) {
    output << '{';
    bool first = true;
    append_number_member(output, first, "kind", record.kind);
    append_number_member(output, first, "x", record.x);
    append_number_member(output, first, "y", record.y);
    append_number_member(output, first, "weaponact", record.weapon_action);
    append_number_member(output, first, "attacking", record.attacking);
    append_number_member(output, first, "cover", record.cover);
    append_number_member(output, first, "dvx", record.dvx);
    append_number_member(output, first, "dvy", record.dvy);
    append_number_member(output, first, "dvz", record.dvz);
    output << '}';
}

void append_opoint(std::ostream& output, const ObjectPointRecord28& record) {
    output << '{';
    bool first = true;
    append_number_member(output, first, "kind", record.kind);
    append_number_member(output, first, "x", record.x);
    append_number_member(output, first, "y", record.y);
    append_number_member(output, first, "z", record.z);
    append_number_member(output, first, "action", record.action);
    append_number_member(output, first, "dvx", record.dvx);
    append_number_member(output, first, "dvy", record.dvy);
    append_number_member(output, first, "dvz", record.dvz);
    append_number_member(output, first, "oid", record.object_id);
    append_number_member(output, first, "facing", record.facing);
    append_number_member(output, first, "hp", record.hp);
    append_number_member(output, first, "mp", record.mp);
    append_number_member(output, first, "team", record.team);
    append_number_member(output, first, "reserve", record.reserve);
    append_number_member(output, first, "effect", record.effect);
    append_number_member(output, first, "pic", record.pic);
    append_number_member(output, first, "centerx", record.center_x);
    append_number_member(output, first, "centery", record.center_y);
    append_number_member(output, first, "centerz", record.center_z);
    append_number_member(output, first, "framea", record.frame_a);
    append_number_member(output, first, "attacking", record.attacking);
    append_number_member(output, first, "join", record.join);
    append_number_member(output, first, "join_reserve", record.join_reserve);
    append_number_member(output, first, "join_pic", record.join_pic);
    output << '}';
}

void append_bdy_normalized(std::ostream& output,
                           const SubBlock28& block,
                           std::string& conversion_error) {
    const auto decoded = ntsd28::CollisionGeometry28::decode(block);
    output << '{';
    bool first = true;
    append_number_member(output, first, "kind", block.values.integer("kind").value_or(0));
    append_number_member(output, first, "x", block.values.integer("x").value_or(0));
    append_number_member(output, first, "y", block.values.integer("y").value_or(0));
    append_number_member(output, first, "w", block.values.integer("w").value_or(0));
    append_number_member(output, first, "h", block.values.integer("h").value_or(0));
    append_number_member(output, first, "zwidth", block.values.integer("zwidth").value_or(0));
    append_number_member(output, first, "z", block.values.integer("z").value_or(0));
    output << '}';

    if (decoded.control_only) {
        conversion_error = "control_only_kind";
        return;
    }
    if (!decoded.box) {
        conversion_error = "missing fields:";
        for (std::size_t index = 0; index < decoded.missing_fields.size(); ++index) {
            if (index != 0) {
                conversion_error += ',';
            }
            conversion_error += decoded.missing_fields[index];
        }
    }
}

void append_normalized(std::ostream& output,
                       const SubBlock28& block,
                       std::string& conversion_error) {
    if (block.kind == "itr") {
        append_interaction(output, ntsd28::CombatRecordDecoder28::interaction(block));
    } else if (block.kind == "cpoint") {
        append_cpoint(output, ntsd28::CombatRecordDecoder28::catch_point(block));
    } else if (block.kind == "wpoint") {
        append_wpoint(output, ntsd28::CombatRecordDecoder28::weapon_point(block));
    } else if (block.kind == "opoint") {
        append_opoint(output, ntsd28::ObjectSpawnPlanner28::decode(block));
    } else if (block.kind == "bdy") {
        append_bdy_normalized(output, block, conversion_error);
    } else {
        conversion_error = "unsupported decoder: " + block.kind;
        output << "{}";
    }
}

void append_subblock(std::ostream& output, const SubBlock28& block) {
    std::string conversion_error;
    output << '{';
    bool first = true;
    append_member(output, first, "kind", [&](std::ostream& value_output) {
        append_string(value_output, block.kind);
    });
    append_number_member(output, first, "line", block.line);
    append_member(output, first, "fields", [&](std::ostream& value_output) {
        append_fields(value_output, block.values);
    });
    append_member(output, first, "normalized", [&](std::ostream& value_output) {
        append_normalized(value_output, block, conversion_error);
    });
    append_member(output, first, "conversionError", [&](std::ostream& value_output) {
        if (conversion_error.empty()) {
            value_output << "null";
        } else {
            append_string(value_output, conversion_error);
        }
    });
    output << '}';
}

void append_subblocks(std::ostream& output, const std::vector<SubBlock28>& blocks) {
    output << '[';
    for (std::size_t index = 0; index < blocks.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        append_subblock(output, blocks[index]);
    }
    output << ']';
}

void append_sprites(std::ostream& output,
                    const std::vector<ntsd28::SpriteSheet28>& sprites) {
    output << '[';
    for (std::size_t index = 0; index < sprites.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        const auto& sprite = sprites[index];
        output << "{\"path\":" << json_quote(sprite.source_path)
               << ",\"width\":" << sprite.width.value_or(-1)
               << ",\"height\":" << sprite.height.value_or(-1)
               << ",\"row\":" << sprite.rows.value_or(-1)
               << ",\"col\":" << sprite.columns.value_or(-1)
               << ",\"declaredFirst\":" << sprite.declared_first_pic
               << ",\"declaredLast\":" << sprite.declared_last_pic
               << ",\"effectiveFirst\":" << sprite.first_pic
               << ",\"effectiveLast\":" << sprite.last_pic
               << ",\"line\":" << sprite.line
               << '}';
    }
    output << ']';
}

void append_frames(std::ostream& output, const std::vector<Frame28>& frames) {
    output << '[';
    for (std::size_t index = 0; index < frames.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        const auto& frame = frames[index];
        output << '{';
        bool first = true;
        append_number_member(output, first, "id", frame.id);
        append_member(output, first, "name", [&](std::ostream& value_output) {
            append_string(value_output, frame.caption);
        });
        append_number_member(output, first, "openingLine", frame.opening_line);
        append_number_member(output, first, "closingLine", frame.closing_line);
        append_member(output, first, "fields", [&](std::ostream& value_output) {
            append_fields(value_output, frame.values);
        });
        append_member(output, first, "subblocks", [&](std::ostream& value_output) {
            append_subblocks(value_output, frame.subblocks);
        });
        output << '}';
    }
    output << ']';
}

void append_armor_normalized(std::ostream& output, const ArmorRecord28& record) {
    output << '{';
    bool first = true;
    append_number_member(output, first, "type", record.type);
    append_number_member(output, first, "ratio", record.ratio);
    append_number_member(output, first, "decrease", record.decrease);
    append_number_member(output, first, "mp", record.mp);
    append_number_member(output, first, "fall", record.fall);
    append_number_member(output, first, "bdefend", record.bdefend);
    append_number_member(output, first, "injury", record.injury);
    append_number_member(output, first, "spark", record.spark);
    append_number_member(output, first, "hp", record.hp);
    append_number_member(output, first, "recover", record.recover);
    append_number_member(output, first, "facing", record.facing);
    append_number_member(output, first, "action", record.action);
    append_number_member(output, first, "reserve", record.reserve);
    append_number_member(output, first, "delay", record.delay);
    append_member(output, first, "frame", [&](std::ostream& value_output) {
        append_frame_ranges(value_output, record.frame_ranges);
    });
    append_member(output, first, "state", [&](std::ostream& value_output) {
        append_integer_array(value_output, record.states);
    });
    append_member(output, first, "kind", [&](std::ostream& value_output) {
        append_integer_array(value_output, record.kinds);
    });
    append_member(output, first, "id", [&](std::ostream& value_output) {
        append_integer_array(value_output, record.ids);
    });
    append_member(output, first, "effect", [&](std::ostream& value_output) {
        append_integer_array(value_output, record.effects);
    });
    if (record.sound1.has_value()) {
        append_member(output, first, "sound1", [&](std::ostream& value_output) {
            append_string(value_output, *record.sound1);
        });
    }
    if (record.sound2.has_value()) {
        append_member(output, first, "sound2", [&](std::ostream& value_output) {
            append_string(value_output, *record.sound2);
        });
    }
    output << '}';
}

void append_armor(std::ostream& output, const std::vector<ArmorBlock28>& blocks) {
    output << '[';
    for (std::size_t index = 0; index < blocks.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        const auto& block = blocks[index];
        output << "{\"openingLine\":" << block.opening_line
               << ",\"closingLine\":" << block.closing_line
               << ",\"fields\":";
        append_fields(output, block.values);
        output << ",\"normalized\":";
        append_armor_normalized(output, ntsd28::CombatRecordDecoder28::armor(block));
        output << '}';
    }
    output << ']';
}

void append_weapon_strength(std::ostream& output,
                            const std::vector<WeaponStrengthEntry28>& entries) {
    output << '[';
    for (std::size_t index = 0; index < entries.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        const auto& entry = entries[index];
        output << "{\"index\":" << entry.index
               << ",\"caption\":" << json_quote(entry.caption)
               << ",\"openingLine\":" << entry.opening_line
               << ",\"fields\":";
        append_fields(output, entry.values);
        output << '}';
    }
    output << ']';
}

void append_weapon_piece_variant(std::ostream& output,
                                 const WeaponPieceVariant28& variant) {
    output << "{\"piece\":" << variant.piece
           << ",\"openingLine\":" << variant.opening_line
           << ",\"closingLine\":" << variant.closing_line
           << ",\"fields\":";
    append_fields(output, variant.values);
    output << '}';
}

void append_weapon_piece_group(std::ostream& output,
                               const WeaponPieceGroup28& group) {
    output << "{\"piece\":" << group.piece << ",\"fields\":";
    append_fields(output, group.values);
    output << ",\"variants\":[";
    for (std::size_t index = 0; index < group.variants.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        append_weapon_piece_variant(output, group.variants[index]);
    }
    output << "]}";
}

void append_weapon_piece(std::ostream& output,
                         const std::optional<WeaponPieceBlock28>& block) {
    if (!block.has_value()) {
        output << "{}";
        return;
    }
    output << "{\"openingLine\":" << block->opening_line
           << ",\"closingLine\":" << block->closing_line
           << ",\"fields\":";
    append_fields(output, block->values);
    output << ",\"groups\":[";
    for (std::size_t index = 0; index < block->groups.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        append_weapon_piece_group(output, block->groups[index]);
    }
    output << "]}";
}

void append_menu_face(std::ostream& output,
                      const std::vector<FieldBag>& layers) {
    output << '[';
    for (std::size_t index = 0; index < layers.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        output << "{\"fields\":";
        append_fields(output, layers[index]);
        output << '}';
    }
    output << ']';
}

void append_bmp_sequences(std::ostream& output,
                          const std::vector<BmpFrameSequence28>& sequences) {
    output << '[';
    for (std::size_t index = 0; index < sequences.size(); ++index) {
        if (index != 0) {
            output << ',';
        }
        const auto& sequence = sequences[index];
        output << "{\"name\":" << json_quote(sequence.name)
               << ",\"declaredCount\":" << sequence.declared_count
               << ",\"actions\":";
        append_integer_array(output, sequence.actions);
        output << ",\"openingLine\":" << sequence.opening_line
               << ",\"closingLine\":" << sequence.closing_line
               << '}';
    }
    output << ']';
}

void append_document(std::ostream& output,
                     std::string_view relative_path,
                     const DatDocument& document) {
    output << '{';
    bool first = true;
    append_member(output, first, "sourceModel", [](std::ostream& value_output) {
        append_string(value_output, "SOURCE_MODEL_DIAGNOSTIC_ONLY");
    });
    append_member(output, first, "path", [&](std::ostream& value_output) {
        append_string(value_output, relative_path);
    });
    append_member(output, first, "parseSuccess", [&](std::ostream& value_output) {
        value_output << (document.ok() ? "true" : "false");
    });
    append_member(output, first, "diagnostics", [&](std::ostream& value_output) {
        append_diagnostics(value_output, document.diagnostics);
    });
    append_member(output, first, "top", [&](std::ostream& value_output) {
        append_fields(value_output, document.top_level);
    });
    append_member(output, first, "bmp", [&](std::ostream& value_output) {
        append_fields(value_output, document.bmp);
    });
    append_member(output, first, "stats", [&](std::ostream& value_output) {
        append_fields(value_output, document.stats);
    });
    append_member(output, first, "sprites", [&](std::ostream& value_output) {
        append_sprites(value_output, document.sprite_sheets);
    });
    append_member(output, first, "frames", [&](std::ostream& value_output) {
        append_frames(value_output, document.frames);
    });
    append_member(output, first, "blocks", [&](std::ostream& value_output) {
        append_subblocks(value_output, document.orphan_subblocks);
    });
    append_member(output, first, "armor", [&](std::ostream& value_output) {
        append_armor(value_output, document.armor_blocks);
    });
    append_member(output, first, "weaponStrength", [&](std::ostream& value_output) {
        append_weapon_strength(value_output, document.weapon_strength_entries);
    });
    append_member(output, first, "weaponPiece", [&](std::ostream& value_output) {
        append_weapon_piece(value_output, document.weapon_piece);
    });
    append_member(output, first, "menuFace", [&](std::ostream& value_output) {
        append_menu_face(value_output, document.menu_face_layers);
    });
    append_member(output, first, "bmpSequences", [&](std::ostream& value_output) {
        append_bmp_sequences(value_output, document.bmp_frame_sequences);
    });
    append_number_member(output, first, "fieldCount", document.field_count());
    output << '}';
}

void append_failed_document(std::ostream& output,
                            std::string_view relative_path,
                            std::string_view error) {
    output << "{\"sourceModel\":\"SOURCE_MODEL_DIAGNOSTIC_ONLY\",\"path\":"
           << json_quote(relative_path)
           << ",\"parseSuccess\":false,\"diagnostics\":[{\"severity\":\"error\",\"line\":0,\"message\":"
           << json_quote(error)
           << "}],\"top\":[],\"bmp\":[],\"stats\":[],\"sprites\":[],\"frames\":[],\"blocks\":[],\"armor\":[],\"weaponStrength\":[],\"weaponPiece\":{},\"menuFace\":[],\"bmpSequences\":[],\"fieldCount\":0}";
}

bool has_dat_extension(const std::filesystem::path& path) {
    std::string extension = path.extension().u8string();
    std::transform(extension.begin(), extension.end(), extension.begin(), [](unsigned char value) {
        return static_cast<char>(std::tolower(value));
    });
    return extension == ".dat";
}

Arguments parse_arguments(int argc, char** argv) {
    Arguments arguments;
    for (int index = 1; index < argc; ++index) {
        const std::string argument = argv[index];
        auto require_value = [&](const char* option) -> std::string {
            if (index + 1 >= argc) {
                throw std::runtime_error(std::string("missing value for ") + option);
            }
            return argv[++index];
        };
        if (argument == "--input-root") {
            arguments.input_root = std::filesystem::u8path(require_value("--input-root"));
        } else if (argument == "--output") {
            arguments.output = std::filesystem::u8path(require_value("--output"));
        } else if (argument == "--help" || argument == "-h") {
            std::cout << "Usage: AuthorityContentCapture --input-root <decoded DAT root> --output <jsonl>\n";
            std::exit(0);
        } else {
            throw std::runtime_error("unknown option: " + argument);
        }
    }
    if (arguments.input_root.empty() || arguments.output.empty()) {
        throw std::runtime_error("--input-root and --output are required");
    }
    return arguments;
}

std::vector<std::filesystem::path> collect_dat_files(
    const std::filesystem::path& input_root) {
    std::error_code error;
    const auto absolute_root = std::filesystem::absolute(input_root, error);
    if (error || !std::filesystem::is_directory(absolute_root, error)) {
        throw std::runtime_error("input root is not a directory: " + path_to_utf8(input_root));
    }

    std::vector<std::pair<std::string, std::filesystem::path>> sorted;
    std::filesystem::recursive_directory_iterator iterator(
        absolute_root,
        std::filesystem::directory_options::skip_permission_denied,
        error);
    const std::filesystem::recursive_directory_iterator end;
    while (iterator != end) {
        if (error) {
            throw std::runtime_error("error walking input root: " + error.message());
        }
        if (iterator->is_regular_file(error) && !error && has_dat_extension(iterator->path())) {
            const auto relative = iterator->path().lexically_relative(absolute_root);
            sorted.emplace_back(normalized_relative_path(relative), iterator->path());
        }
        iterator.increment(error);
    }
    std::sort(sorted.begin(), sorted.end(), [](const auto& first, const auto& second) {
        return first.first < second.first;
    });
    std::vector<std::filesystem::path> result;
    result.reserve(sorted.size());
    for (const auto& entry : sorted) {
        result.push_back(entry.second);
    }
    return result;
}

void update_stats(CaptureStats& stats, const DatDocument& document) {
    if (document.ok()) {
        ++stats.parse_success;
    } else {
        ++stats.parse_failure;
    }
    for (const auto& diagnostic : document.diagnostics) {
        if (diagnostic.severity == ParseDiagnostic::Severity::error) {
            ++stats.errors;
        } else {
            ++stats.warnings;
        }
    }
}

int capture(const Arguments& arguments) {
    const auto files = collect_dat_files(arguments.input_root);
    const auto absolute_root = std::filesystem::absolute(arguments.input_root);
    const auto parent = arguments.output.parent_path();
    if (!parent.empty()) {
        std::error_code directory_error;
        std::filesystem::create_directories(parent, directory_error);
        if (directory_error) {
            throw std::runtime_error("unable to create output directory: " +
                                     directory_error.message());
        }
    }
    std::ofstream output(arguments.output, std::ios::binary | std::ios::trunc);
    if (!output) {
        throw std::runtime_error("unable to open output: " + path_to_utf8(arguments.output));
    }
    output.imbue(std::locale::classic());

    ntsd28::DatParser parser;
    CaptureStats stats;
    for (const auto& file : files) {
        ++stats.files;
        const auto relative = normalized_relative_path(file.lexically_relative(absolute_root));
        try {
            const auto document = parser.parse_file(file);
            update_stats(stats, document);
            append_document(output, relative, document);
        } catch (const std::exception& exception) {
            ++stats.parse_failure;
            ++stats.errors;
            append_failed_document(output, relative, exception.what());
        }
        output << '\n';
    }
    output.flush();
    if (!output) {
        throw std::runtime_error("failed while writing output: " + path_to_utf8(arguments.output));
    }
    std::cout << "sourceModel=SOURCE_MODEL_DIAGNOSTIC_ONLY"
              << " files=" << stats.files
              << " parseSuccess=" << stats.parse_success
              << " parseFailure=" << stats.parse_failure
              << " warnings=" << stats.warnings
              << " errors=" << stats.errors
              << " output=" << path_to_utf8(arguments.output)
              << '\n';
    return stats.parse_failure == 0 ? 0 : 1;
}

}  // namespace

int main(int argc, char** argv) {
    try {
        return capture(parse_arguments(argc, argv));
    } catch (const std::exception& exception) {
        std::cerr << "capture failed: " << exception.what() << '\n';
        return 2;
    }
}
