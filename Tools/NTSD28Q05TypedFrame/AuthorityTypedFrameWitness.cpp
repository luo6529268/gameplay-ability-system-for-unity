#include "ntsd28/combat_records.h"
#include "ntsd28/collision_geometry.h"
#include "ntsd28/dat_parser.h"
#include "ntsd28/object_spawning.h"
#include <algorithm>
#include <cstring>
#include <cstdint>
#include <cstdlib>
#include <cmath>
#include <fstream>
#include <sstream>
#include <filesystem>
#include <iomanip>
#include <iostream>
#include <stdexcept>
#include <vector>

using ntsd28::FieldBag;
// Exact helper text captured from current battle_world.cpp; see artifact hash/provenance.
double field_number_or_zero(const FieldBag& fields, std::string_view key) noexcept {
    const auto* field = fields.last(key);
    if (field == nullptr || field->value.empty()) {
        return 0.0;
    }
    char* end = nullptr;
    const double value = std::strtod(field->value.c_str(), &end);
    if (end != field->value.c_str() + field->value.size() ||
        !std::isfinite(value)) {
        return 0.0;
    }
    return value;
}

std::int64_t double_bits(double value) { std::int64_t result; std::memcpy(&result, &value, sizeof(result)); return result; }
int bits(float value) { int result; static_assert(sizeof(result) == sizeof(value)); std::memcpy(&result, &value, sizeof(result)); return result; }
void begin(const std::string& file, int frame, const char* kind, int index) { std::cout << file << '\t' << frame << '\t' << kind << '\t' << index; }
void row(const std::string& file, int frame, const char* kind, int index, const std::vector<int>& values) {
    begin(file, frame, kind, index); for (int value : values) std::cout << '\t' << value; std::cout << '\n';
}
void text_row(const std::string& file, int frame, const char* kind, int index, const std::string& text) {
    begin(file, frame, kind, index); std::cout << '\t';
    for (unsigned char byte : text) std::cout << std::hex << std::setw(2) << std::setfill('0') << static_cast<unsigned int>(byte);
    std::cout << std::dec << '\n';
}
std::vector<int> itr(const ntsd28::InteractionRecord28& r) {
    return {r.kind,r.x,r.y,r.width,r.height,r.dvx,r.dvy,r.fall,r.arest,r.vrest,r.respond,r.effect,r.drain,r.spark,r.recover,r.dbdefend,r.bdefend,r.injury,r.zwidth,r.z,r.dvz,r.sound,r.cover,r.caughtact,r.catchingact,r.pickedact,r.pickingact,r.delay,r.poison,r.confus,r.weak,r.manacle,r.join,r.mimic,r.bound,r.facing,r.dx,r.dy,r.dz,r.gain};
}
int main(int argc, char** argv) {
    try {
        if (argc == 3 && std::string(argv[1]) == "--numbers") {
            std::ifstream input(argv[2]); std::string line; int index = 0;
            while (std::getline(input, line)) {
                if (!line.empty() && line.back() == '\r') line.pop_back();
                std::string value;
                for (std::size_t i = 0; i < line.size(); i += 2) value.push_back(static_cast<char>(std::stoul(line.substr(i, 2), nullptr, 16)));
                ntsd28::FieldBag fields; fields.add({"frame", "number", value, 1, 1});
                std::cout << index++ << '\t' << double_bits(field_number_or_zero(fields, "number")) << '\n';
            }
            return 0;
        }
        if (argc != 2) throw std::runtime_error("expected DAT root");
        std::vector<std::filesystem::path> files;
        for (const auto& item : std::filesystem::recursive_directory_iterator(argv[1]))
            if (item.is_regular_file() && item.path().extension() == ".dat") files.push_back(item.path());
        std::sort(files.begin(), files.end());
        const char* frame_keys[] = {"pic","state","cover","wait","next","dvx","dvy","dvz","centerx","centery","mp","hp","hit_a","hit_d","hit_j","hit_g","hit_Fj","hit_Fa","hit_Da","hit_Ua","hit_ja","hit_aj","hit_ad","hit_jd","hit_Dj","hit_Uj","hit_f","hit_b","hit_uz","hit_dz","hold_a","hold_d","hold_j","hold_f","hold_b","hold_uz","hold_dz","centerz","chp","cmp"};
        for (const auto& path : files) {
            const auto doc = ntsd28::DatParser{}.parse_file(path);
            const auto file = std::filesystem::relative(path, argv[1]).generic_string();
            row(file, -1, "document", doc.ok() ? 1 : 0, {static_cast<int>(doc.frames.size())});
            if (!doc.ok()) continue;
            for (std::size_t f = 0; f < doc.frames.size(); ++f) {
                const auto& frame = doc.frames[f]; const int fi = static_cast<int>(f);
                std::vector<int> scalars{frame.id};
                for (const char* key : frame_keys) scalars.push_back(frame.values.integer(key).value_or(0));
                const auto* primary_body = frame.first_block("bdy");
                scalars.push_back(primary_body ? primary_body->values.integer("kind").value_or(0) : 0);
                scalars.push_back(primary_body ? primary_body->values.integer("respond").value_or(0) : 0);
                row(file, fi, "frame", 0, scalars);
                begin(file, fi, "motion", 0);
                for (const char* key : {"dvx","dvy","dvz","dx","dy","dz"}) std::cout << '\t' << double_bits(field_number_or_zero(frame.values, key));
                std::cout << '\n';
                text_row(file, fi, "name", 0, frame.caption);
                int sound_index = 0;
                for (const auto* sound : frame.values.all("sound")) text_row(file, fi, "sound", sound_index++, sound->value);
                for (const char* kind : {"bdy","itr","cpoint","opoint","wpoint","bpoint"}) {
                    int index = 0;
                    for (const auto* block : frame.blocks(kind)) {
                        const auto& v = block->values;
                        if (block->kind == "bdy") {
                            row(file,fi,kind,index,{v.integer("x").value_or(0),v.integer("y").value_or(0),v.integer("w").value_or(0),v.integer("h").value_or(0),v.integer("zwidth").value_or(0),ntsd28::CollisionGeometry28::decode(*block).box.has_value() ? 1 : 0});
                        } else if (block->kind == "itr") {
                            auto values = itr(ntsd28::CombatRecordDecoder28::interaction(*block));
                            values.push_back(ntsd28::CollisionGeometry28::decode(*block).box.has_value() ? 1 : 0); row(file,fi,kind,index,values);
                        } else if (block->kind == "cpoint") {
                            const auto r = ntsd28::CombatRecordDecoder28::catch_point(*block);
                            row(file,fi,kind,index,{r.kind,r.x,r.y,r.injury,r.cover,r.victim_action,r.attack_action,r.jump_action,r.defend_action,r.throw_action,r.front_action,r.back_action,r.up_z_action,r.down_z_action,bits(r.throw_vx),bits(r.throw_vy),r.hurtable,r.front_hurt_action,r.back_hurt_action,r.decrease,r.direction_control,r.throw_injury,bits(r.throw_vz),r.z,r.recover,r.drain,r.gain});
                        } else if (block->kind == "opoint") {
                            const auto r = ntsd28::ObjectSpawnPlanner28::decode(*block);
                            row(file,fi,kind,index,{r.kind,r.x,r.y,r.z,r.action,r.dvx,r.dvy,r.dvz,r.object_id,r.facing,r.hp,r.mp,r.team,r.reserve,r.effect,r.pic,r.center_x,r.center_y,r.center_z,r.frame_a,r.attacking,r.join,r.join_reserve,r.join_pic});
                        } else if (block->kind == "wpoint") {
                            const auto r = ntsd28::CombatRecordDecoder28::weapon_point(*block);
                            row(file,fi,kind,index,{r.kind,r.x,r.y,r.weapon_action,r.attacking,r.cover,r.dvx,r.dvy,r.dvz});
                        } else row(file,fi,kind,index,{v.integer("x").value_or(0),v.integer("y").value_or(0)});
                        ++index;
                    }
                }
            }
            for (const auto& strength : doc.weapon_strength_entries) {
                auto values = itr(ntsd28::CombatRecordDecoder28::weapon_strength_interaction({},strength));
                row(file,-1,"strength",strength.index,std::vector<int>(values.begin()+5,values.begin()+24));
            }
        }
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 1; }
}
