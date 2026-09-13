#include "ntsd28/dat_document.h"
#include <cmath>
#include <cstdlib>
#include <cstdint>
#include <cstring>
#include <iostream>
#include <optional>
#include <stdexcept>
using ntsd28::FieldBag;
// Exact optional helper text from current playable input_routing.cpp; source identity accompanies capture.
std::optional<double> field_double(const FieldBag& fields, std::string_view key) {
    const auto* field = fields.last(key);
    if (field == nullptr || field->value.empty()) return std::nullopt;
    char* end = nullptr;
    const double value = std::strtod(field->value.c_str(), &end);
    if (end != field->value.c_str() + field->value.size() || !std::isfinite(value)) {
        return std::nullopt;
    }
    return value;
}
int main() {
    try {
        std::string line;
        while (std::getline(std::cin, line)) {
            const auto split = line.find('\t');
            if (split == std::string::npos) throw std::runtime_error("missing separator");
            const auto id = line.substr(0, split); const auto hex = line.substr(split + 1);
            std::string value;
            for (std::size_t i = 0; i < hex.size(); i += 2) value.push_back(static_cast<char>(std::stoul(hex.substr(i, 2), nullptr, 16)));
            FieldBag fields; fields.add({"bmp", "value", value, 1, 1});
            const auto integer = fields.integer("value"); const auto number = field_double(fields, "value");
            double projected = number.value_or(0.0); std::int64_t bits; std::memcpy(&bits, &projected, sizeof(bits));
            std::cout << id << '\t' << (integer.has_value()?1:0) << '\t' << integer.value_or(0) << '\t' << (number.has_value()?1:0) << '\t' << bits << '\n';
        }
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 1; }
}
