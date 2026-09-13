#include "ntsd28/combat_records.h"
#include "ntsd28/dat_parser.h"

#include <algorithm>
#include <cstdint>
#include <cstring>
#include <filesystem>
#include <iomanip>
#include <iostream>
#include <stdexcept>
#include <vector>

namespace {
std::uint32_t bits(float value) {
    static_assert(sizeof(float) == sizeof(std::uint32_t));
    std::uint32_t result = 0;
    std::memcpy(&result, &value, sizeof(result));
    return result;
}
void write_bits(float value) {
    std::cout << std::hex << std::uppercase << std::setfill('0')
              << std::setw(8) << bits(value) << std::dec;
}
}

int main(int argc, char** argv) {
    try {
        if (argc != 2) throw std::runtime_error("expected DAT fixture directory");
        std::vector<std::filesystem::path> paths;
        for (const auto& entry : std::filesystem::directory_iterator(argv[1])) {
            if (entry.is_regular_file() && entry.path().extension() == ".dat")
                paths.push_back(entry.path());
        }
        std::sort(paths.begin(), paths.end());
        if (paths.empty()) throw std::runtime_error("no DAT fixtures");
        std::cout << "id\tthrowvx_bits\tthrowvy_bits\tthrowvz_bits\tinjury\twpoint_x"
                     "\tcaughtact\tcatchingact\tpickedact\tpickingact\n";
        for (const auto& path : paths) {
            const auto document = ntsd28::DatParser{}.parse_file(path);
            if (document.frames.size() != 1) throw std::runtime_error("expected one frame");
            const auto& frame = document.frames.front();
            const auto* catch_block = frame.first_block("cpoint");
            const auto* weapon_block = frame.first_block("wpoint");
            const auto* interaction_block = frame.first_block("itr");
            if (!catch_block || !weapon_block || !interaction_block)
                throw std::runtime_error("required decoder block absent");
            const auto point = ntsd28::CombatRecordDecoder28::catch_point(*catch_block);
            const auto weapon = ntsd28::CombatRecordDecoder28::weapon_point(*weapon_block);
            const auto interaction = ntsd28::CombatRecordDecoder28::interaction(*interaction_block);
            std::cout << path.stem().string() << '\t';
            write_bits(point.throw_vx); std::cout << '\t';
            write_bits(point.throw_vy); std::cout << '\t';
            write_bits(point.throw_vz); std::cout << '\t' << point.injury << '\t' << weapon.x
                << '\t' << interaction.caughtact << '\t' << interaction.catchingact
                << '\t' << interaction.pickedact << '\t' << interaction.pickingact << '\n';
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
