#include "ntsd28/collision_geometry.h"
#include "ntsd28/dat_parser.h"
#include <algorithm>
#include <filesystem>
#include <iostream>
#include <stdexcept>
#include <vector>

int main(int argc, char** argv) {
    try {
        if (argc != 2) throw std::runtime_error("expected fixture root");
        const std::filesystem::path root = argv[1];
        std::vector<std::filesystem::path> paths;
        for (const auto& item : std::filesystem::recursive_directory_iterator(root))
            if (item.is_regular_file() && item.path().extension() == ".dat") paths.push_back(item.path());
        std::sort(paths.begin(), paths.end());
        std::cout << "path\tframe\tblock\ttype\tkind\tx\ty\tw\th\tzwidth\tz\thas_geometry\tcontrol_only\n";
        for (const auto& path : paths) {
            const auto document = ntsd28::DatParser{}.parse_file(path);
            for (std::size_t f = 0; f < document.frames.size(); ++f) {
                const auto& frame = document.frames[f];
                for (std::size_t b = 0; b < frame.subblocks.size(); ++b) {
                    const auto& block = frame.subblocks[b];
                    if (block.kind != "bdy" && block.kind != "itr") continue;
                    const auto decoded = ntsd28::CollisionGeometry28::decode(block);
                    std::cout << std::filesystem::relative(path, root).generic_string()
                              << '\t' << f << '\t' << b << '\t' << block.kind;
                    for (const auto* key : {"kind", "x", "y", "w", "h", "zwidth", "z"})
                        std::cout << '\t' << block.values.integer(key).value_or(0);
                    std::cout << '\t' << (decoded.box.has_value() ? 1 : 0)
                              << '\t' << (decoded.control_only ? 1 : 0) << '\n';
                }
            }
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
