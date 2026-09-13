#include "ntsd28/dat_parser.h"
#include <algorithm>
#include <filesystem>
#include <iomanip>
#include <iostream>
#include <stdexcept>
#include <vector>

void row(const std::string& file, const char* type, int index, std::size_t line,
         const std::string& key, const std::string& value) {
    std::cout << file << '\t' << type << '\t' << index << '\t' << line << '\t' << key << '\t';
    for (unsigned char byte : value)
        std::cout << std::hex << std::setw(2) << std::setfill('0') << static_cast<unsigned int>(byte);
    std::cout << std::dec << '\n';
}
int main(int argc, char** argv) {
    try {
        if (argc != 2) throw std::runtime_error("expected fixture root");
        std::vector<std::filesystem::path> files;
        for (const auto& item : std::filesystem::recursive_directory_iterator(argv[1]))
            if (item.is_regular_file() && item.path().extension() == ".dat") files.push_back(item.path());
        std::sort(files.begin(), files.end());
        std::cout << "file\ttype\tindex\tline\tkey\tvalue_hex\n";
        for (const auto& file : files) {
            const auto doc = ntsd28::DatParser{}.parse_file(file);
            const auto name = std::filesystem::relative(file, argv[1]).generic_string();
            row(name, "document", doc.ok() ? 1 : 0, doc.weapon_strength_entries.size(), "", "");
            for (const auto& entry : doc.weapon_strength_entries) {
                row(name, "row", entry.index, entry.opening_line, "", entry.caption);
                for (const auto& field : entry.values.fields())
                    row(name, "field", entry.index, field.line, field.key, field.value);
            }
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
