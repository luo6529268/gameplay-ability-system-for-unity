#include "ntsd28/combat_records.h"
#include <cstdint>
#include <cstring>
#include <iomanip>
#include <iostream>
#include <stdexcept>

int main() {
    try {
        std::cout << "id\tfloat_bits\tstrict_int\tfirst_int\tstrict_valid\n";
        std::string line;
        while (std::getline(std::cin, line)) {
            const auto separator = line.find('\t');
            if (separator == std::string::npos) throw std::runtime_error("missing separator");
            const auto id = line.substr(0, separator);
            const auto hex = line.substr(separator + 1);
            if (hex.size() % 2) throw std::runtime_error("odd hex length");
            std::string value;
            for (std::size_t i = 0; i < hex.size(); i += 2)
                value.push_back(static_cast<char>(std::stoul(hex.substr(i, 2), nullptr, 16)));
            ntsd28::SubBlock28 block;
            for (const auto* key : {"throwvx", "injury", "caughtact"})
                block.values.add(ntsd28::RawField{"", key, value, 0, 0});
            const auto point = ntsd28::CombatRecordDecoder28::catch_point(block);
            const auto interaction = ntsd28::CombatRecordDecoder28::interaction(block);
            std::uint32_t bits = 0;
            std::memcpy(&bits, &point.throw_vx, sizeof(bits));
            std::cout << id << '\t' << std::hex << std::uppercase << std::setfill('0')
                      << std::setw(8) << bits << std::dec << '\t' << point.injury
                      << '\t' << interaction.caughtact << '\t'
                      << (block.values.integer("injury").has_value() ? 1 : 0) << '\n';
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
