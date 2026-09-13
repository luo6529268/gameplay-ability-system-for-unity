#include "ntsd28/dat_parser.h"
#include "ntsd28/hit_candidates.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <sstream>
#include <stdexcept>
#include <string>

// Source-linked diagnostic only; the production geometry functions own results.
int main(int argc, char** argv) {
    try {
        if (argc != 2) throw std::runtime_error("expected fixture root");
        const std::filesystem::path root(argv[1]);
        std::ifstream cases(root / "cases.tsv");
        if (!cases) throw std::runtime_error("cases.tsv missing");
        std::string line;
        std::getline(cases, line);
        std::cout << "id\tcandidates\tdiagnostics\n";
        int count = 0;
        while (std::getline(cases, line)) {
            if (line.empty()) continue;
            std::istringstream row(line);
            std::string id;
            int target_x = 0, target_z = 0;
            if (!(row >> id >> target_x >> target_z) ||
                id.find_first_not_of("abcdefghijklmnopqrstuvwxyz0123456789_") != std::string::npos)
                throw std::runtime_error("invalid case row");
            const auto attacker = ntsd28::DatParser{}.parse_file(root / id / "attacker.dat");
            const auto target = ntsd28::DatParser{}.parse_file(root / id / "target.dat");
            if (attacker.frames.size() != 1 || target.frames.size() != 1)
                throw std::runtime_error("expected one frame on each side");
            ntsd28::Position28 attacker_position{}, target_position{};
            target_position.x = target_x;
            target_position.z = target_z;
            ntsd28::HitCandidateBuffer28 output;
            const auto result = ntsd28::HitCandidateBuilder28::append_pair_geometry(
                1, 0, attacker.frames.front(), false, attacker_position,
                target.frames.front(), false, target_position, output);
            std::cout << id << '\t' << result.overlapping_candidates.size()
                      << '\t' << result.diagnostics.size() << '\n';
            ++count;
        }
        if (count == 0) throw std::runtime_error("no cases");
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
