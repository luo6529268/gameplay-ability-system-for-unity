#include "ntsd28/combat_records.h"
#include "ntsd28/dat_parser.h"
#include <algorithm>
#include <filesystem>
#include <iostream>
#include <iomanip>
#include <stdexcept>
#include <vector>

void write(const std::string& path, const char* type, int index, const ntsd28::InteractionRecord28& r) {
    std::cout << path << '\t' << type << '\t' << index;
    const int values[] = {r.kind, r.x, r.y, r.width, r.height, r.dvx, r.dvy, r.fall, r.arest, r.vrest, r.respond, r.effect, r.drain, r.spark, r.recover, r.dbdefend, r.bdefend, r.injury, r.zwidth, r.z, r.dvz, r.sound, r.cover, r.caughtact, r.catchingact, r.pickedact, r.pickingact, r.delay, r.poison, r.confus, r.weak, r.manacle, r.join, r.mimic, r.bound, r.facing, r.dx, r.dy, r.dz, r.gain};
    for (int value : values) std::cout << '\t' << value;
    std::cout << '\n';
}
int main(int argc, char** argv) {
    try {
        if (argc < 2 || argc > 3) throw std::runtime_error("expected fixture root and optional --raw-strength");
        const bool raw = argc == 3 && std::string(argv[2]) == "--raw-strength";
        if (argc == 3 && !raw) throw std::runtime_error("unknown mode");
        std::vector<std::filesystem::path> files;
        for (const auto& item : std::filesystem::directory_iterator(argv[1]))
            if (item.is_regular_file() && item.path().extension() == ".dat") files.push_back(item.path());
        std::sort(files.begin(), files.end());
        if (raw) std::cout << "path\tindex\tkey\tvalue_hex\n";
        else std::cout << "path\ttype\tindex\tkind\tx\ty\tw\th\tdvx\tdvy\tfall\tarest\tvrest\trespond\teffect\tdrain\tspark\trecover\tdbdefend\tbdefend\tinjury\tzwidth\tz\tdvz\tsound\tcover\tcaughtact\tcatchingact\tpickedact\tpickingact\tdelay\tpoison\tconfus\tweak\tmanacle\tjoin\tmimic\tbound\tfacing\tdx\tdy\tdz\tgain\n";
        for (const auto& file : files) {
            const auto doc = ntsd28::DatParser{}.parse_file(file);
            const auto path = file.filename().string();
            if (raw) {
                for (const auto& entry : doc.weapon_strength_entries) {
                    for (const auto& field : entry.values.fields()) {
                        std::cout << path << '\t' << entry.index << '\t' << field.key << '\t';
                        for (unsigned char byte : field.value)
                            std::cout << std::hex << std::setw(2) << std::setfill('0') << static_cast<unsigned int>(byte);
                        std::cout << std::dec << '\n';
                    }
                }
                continue;
            }
            write(path, "document", doc.ok() ? 1 : 0, {});
            for (const auto& frame : doc.frames)
                for (const auto& block : frame.subblocks)
                    if (block.kind == "itr") write(path, "itr", frame.id, ntsd28::CombatRecordDecoder28::interaction(block));
            for (const auto& entry : doc.weapon_strength_entries)
                write(path, "strength", entry.index, ntsd28::CombatRecordDecoder28::weapon_strength_interaction({}, entry));
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
