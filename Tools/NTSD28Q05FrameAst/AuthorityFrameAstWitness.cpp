#include "ntsd28/dat_parser.h"
#include <algorithm>
#include <filesystem>
#include <iomanip>
#include <iostream>
#include <stdexcept>
#include <vector>

void row(const std::string& file, const char* type, int frame, int block,
         const std::string& key, const std::string& value) {
    std::cout << file << '\t' << type << '\t' << frame << '\t' << block << '\t' << key << '\t';
    for (unsigned char byte : value)
        std::cout << std::hex << std::setw(2) << std::setfill('0') << static_cast<unsigned int>(byte);
    std::cout << std::dec << '\n';
}
int main(int argc, char** argv) {
    try {
        if (argc != 2) throw std::runtime_error("expected DAT root");
        std::vector<std::filesystem::path> files;
        for (const auto& item : std::filesystem::recursive_directory_iterator(argv[1]))
            if (item.is_regular_file() && item.path().extension() == ".dat") files.push_back(item.path());
        std::sort(files.begin(), files.end());
        for (const auto& file : files) {
            const auto doc = ntsd28::DatParser{}.parse_file(file);
            const auto name = std::filesystem::relative(file, argv[1]).generic_string();
            row(name, "document", doc.ok() ? 1 : 0, static_cast<int>(doc.frames.size()), "", "");
            for (std::size_t i = 0; i < doc.frames.size(); ++i) {
                const auto& frame = doc.frames[i];
                row(name, "frame", static_cast<int>(i), frame.id, "", frame.caption);
                for (const auto& field : frame.values.fields())
                    row(name, "field", static_cast<int>(i), -1, field.key, field.value);
                for (std::size_t j = 0; j < frame.subblocks.size(); ++j) {
                    const auto& block = frame.subblocks[j];
                    row(name, "block", static_cast<int>(i), static_cast<int>(j), block.kind, "");
                    for (const auto& field : block.values.fields())
                        row(name, "field", static_cast<int>(i), static_cast<int>(j), field.key, field.value);
                }
            }
        }
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 1; }
}
