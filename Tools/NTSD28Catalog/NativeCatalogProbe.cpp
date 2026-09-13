#include "ntsd28/object_catalog.h"
#include <algorithm>
#include <iostream>
#include <string>
#include <vector>

static void quoted(const std::string& text) {
    constexpr char hex[] = "0123456789abcdef";
    std::cout << '"';
    for (const unsigned char ch : text) {
        if (ch == '"' || ch == '\\') std::cout << '\\' << ch;
        else if (ch < 32) std::cout << "\\u00" << hex[ch >> 4] << hex[ch & 15];
        else std::cout << ch;
    }
    std::cout << '"';
}

int wmain(int argc, wchar_t** argv) {
    if (argc != 2) return 2;
    ntsd28::ObjectDefinitionCatalog28 catalog;
    const auto result = catalog.load_extracted_root(std::filesystem::path(argv[1]));
    std::vector<const ntsd28::ObjectDefinitionEntry28*> entries;
    for (const auto& item : catalog.entries()) entries.push_back(&item.second);
    std::sort(entries.begin(), entries.end(), [](const auto* a, const auto* b) {
        return a->registry_index < b->registry_index;
    });
    std::cout << "{\"sourceModel\":\"SOURCE_MODEL_DIAGNOSTIC_ONLY\",\"success\":"
              << (result.success ? "true" : "false") << ",\"objectRows\":" << result.object_rows
              << ",\"entriesLoaded\":" << result.entries_loaded << ",\"entries\":[";
    bool comma = false;
    for (const auto* entry : entries) {
        if (comma) std::cout << ',';
        comma = true;
        std::cout << "{\"registryIndex\":" << entry->registry_index << ",\"id\":" << entry->object_id
                  << ",\"type\":" << entry->object_type << ",\"sourcePath\":";
        quoted(entry->source_path);
        std::cout << ",\"publishedFolder\":"; quoted(entry->published_folder);
        std::cout << ",\"datPath\":"; quoted(entry->readable_dat_path.u8string());
        std::cout << '}';
    }
    std::cout << "],\"diagnostics\":[";
    comma = false;
    for (const auto& diagnostic : result.diagnostics) {
        if (comma) std::cout << ',';
        comma = true;
        std::cout << "{\"line\":" << diagnostic.csv_line << ",\"message\":";
        quoted(diagnostic.message);
        std::cout << '}';
    }
    std::cout << "]}\n";
    return 0; // success/failure is data so rejected fixtures remain inspectable.
}
