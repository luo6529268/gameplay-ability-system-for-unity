#include "ntsd28_playable/game_session.h"
#include "ntsd28_playable/scenario28.h"
#include "ntsd28/fusion_catalog.h"
#include "ntsd28/kind_catalog.h"
#include "ntsd28/native_combo_hud.h"

#ifndef NOMINMAX
#define NOMINMAX
#endif
#include <windows.h>
#include <bcrypt.h>
#include <algorithm>
#include <iterator>

#include <array>
#include <cstdint>
#include <exception>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <limits>
#include <map>
#include <sstream>
#include <stdexcept>
#include <string>
#include <utility>
#include <vector>

namespace {

struct DirectCrtCall28 {
    std::uint32_t result = 0;
    std::uint32_t state_after = 0;
    std::uint64_t total_calls = 0;
};

struct DirectSynchronizedCall28 {
    std::uint32_t call_site = 0;
    std::int32_t upper_bound = 0;
    std::int32_t result = 0;
    std::int32_t counter_after = 0;
    std::int32_t index_after = 0;
    std::uint64_t total_calls = 0;
};

std::vector<DirectCrtCall28> direct_crt_calls;
std::vector<DirectSynchronizedCall28> direct_synchronized_calls;

}  // namespace

extern "C" std::uint32_t
__real__ZN6ntsd2814NativeRandom288crt_nextEv(
    ntsd28::NativeRandom28* random);

extern "C" std::int32_t
__real__ZN6ntsd2814NativeRandom2817synchronized_nextEji(
    ntsd28::NativeRandom28* random,
    std::uint32_t call_site,
    std::int32_t upper_bound);

extern "C" std::uint32_t
__wrap__ZN6ntsd2814NativeRandom288crt_nextEv(
    ntsd28::NativeRandom28* random) {
    const auto result =
        __real__ZN6ntsd2814NativeRandom288crt_nextEv(random);
    const auto after = random->state();
    direct_crt_calls.push_back(DirectCrtCall28{
        result,
        after.crt_state,
        after.crt_calls,
    });
    return result;
}

extern "C" std::int32_t
__wrap__ZN6ntsd2814NativeRandom2817synchronized_nextEji(
    ntsd28::NativeRandom28* random,
    std::uint32_t call_site,
    std::int32_t upper_bound) {
    const auto before_calls = random->synchronized_state().calls;
    const auto result =
        __real__ZN6ntsd2814NativeRandom2817synchronized_nextEji(
            random, call_site, upper_bound);
    const auto& after = random->synchronized_state();
    if (after.calls != before_calls) {
        direct_synchronized_calls.push_back(DirectSynchronizedCall28{
            call_site,
            upper_bound,
            result,
            after.counter,
            after.index,
            after.calls,
        });
    }
    return result;
}

namespace {

constexpr char capture_schema[] = "ntsd28-authority-source-capture-v2";
constexpr char formal_exe_sha256[] =
    "B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033";


// Alignment contract: NTSD28-Q05-TRACE-RAW-IDENTITY-JOINT-UPGRADE-001.
// Match BinaryWriter UTF-8 strings/int32 and the existing Logan semantic identity.
using Bytes28 = std::vector<unsigned char>;

Bytes28 content_read_bytes(const std::filesystem::path& path) {
    std::ifstream stream(path, std::ios::binary);
    if (!stream) throw std::runtime_error("unable to read content input");
    Bytes28 result((std::istreambuf_iterator<char>(stream)), std::istreambuf_iterator<char>());
    if (stream.bad()) throw std::runtime_error("content input read failed");
    return result;
}

Bytes28 content_sha256(const Bytes28& bytes) {
    struct Handles {
        BCRYPT_ALG_HANDLE algorithm = nullptr;
        BCRYPT_HASH_HANDLE hash = nullptr;
        Bytes28 object;
        ~Handles() {
            if (hash) BCryptDestroyHash(hash);
            if (algorithm) BCryptCloseAlgorithmProvider(algorithm, 0);
        }
    } handles;
    if (BCryptOpenAlgorithmProvider(&handles.algorithm, BCRYPT_SHA256_ALGORITHM, nullptr, 0) < 0)
        throw std::runtime_error("SHA256 provider unavailable");
    DWORD object_size = 0, returned = 0;
    if (BCryptGetProperty(handles.algorithm, BCRYPT_OBJECT_LENGTH,
                         reinterpret_cast<PUCHAR>(&object_size), sizeof(object_size), &returned, 0) < 0)
        throw std::runtime_error("SHA256 object size unavailable");
    handles.object.resize(object_size);
    Bytes28 digest(32);
    if (BCryptCreateHash(handles.algorithm, &handles.hash, handles.object.data(), object_size, nullptr, 0, 0) < 0)
        throw std::runtime_error("SHA256 creation failed");
    std::size_t offset = 0;
    while (offset < bytes.size()) {
        const auto count = static_cast<ULONG>(std::min<std::size_t>(bytes.size() - offset, 1024U * 1024U));
        if (BCryptHashData(handles.hash, const_cast<PUCHAR>(bytes.data() + offset), count, 0) < 0)
            throw std::runtime_error("SHA256 update failed");
        offset += count;
    }
    if (BCryptFinishHash(handles.hash, digest.data(), static_cast<ULONG>(digest.size()), 0) < 0)
        throw std::runtime_error("SHA256 finish failed");
    return digest;
}

std::string content_hex(const Bytes28& bytes) {
    constexpr char alphabet[] = "0123456789ABCDEF";
    std::string result;
    result.reserve(bytes.size() * 2U);
    for (unsigned char value : bytes) {
        result.push_back(alphabet[value >> 4U]);
        result.push_back(alphabet[value & 15U]);
    }
    return result;
}

void content_string(Bytes28& bytes, const std::string& value) {
    std::size_t size = value.size();
    while (size >= 128U) {
        bytes.push_back(static_cast<unsigned char>((size & 127U) | 128U));
        size >>= 7U;
    }
    bytes.push_back(static_cast<unsigned char>(size));
    bytes.insert(bytes.end(), value.begin(), value.end());
}

Bytes28 capture_content_raw(const std::filesystem::path& root) {
    ntsd28::ObjectDefinitionCatalog28 catalog;
    if (!catalog.load_extracted_root(root).success)
        throw std::runtime_error("content identity catalog load failed");
    std::vector<const ntsd28::ObjectDefinitionEntry28*> entries;
    for (const auto& pair : catalog.entries()) entries.push_back(&pair.second);
    std::sort(entries.begin(), entries.end(), [](const auto* left, const auto* right) {
        return left->registry_index < right->registry_index;
    });
    Bytes28 bytes;
    content_string(bytes, "LOGAN_OBJECT_DEFINITIONS_V1");
    content_string(bytes, content_hex(content_sha256(content_read_bytes(root / "catalog.csv"))));
    for (const auto* entry : entries) {
        const auto index = static_cast<std::uint32_t>(entry->registry_index);
        for (unsigned int shift = 0; shift < 32U; shift += 8U)
            bytes.push_back(static_cast<unsigned char>(index >> shift));
        content_string(bytes, std::filesystem::relative(entry->readable_dat_path, root).generic_u8string());
        content_string(bytes, content_hex(content_sha256(content_read_bytes(entry->readable_dat_path))));
    }
    return content_sha256(bytes);
}

// Alignment contract: NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001.
struct FusionContentInput28 {
    std::filesystem::path selected_path;
    Bytes28 input_hash;
    Bytes28 semantic_hash;
};
void content_int32(Bytes28& bytes, int value) {
    const auto bits = static_cast<std::uint32_t>(value);
    for (unsigned int shift = 0; shift < 32U; shift += 8U)
        bytes.push_back(static_cast<unsigned char>(bits >> shift));
}
FusionContentInput28 capture_fusion_input(const std::filesystem::path& decoded_root,
                                        const std::filesystem::path& extracted_root) {
    FusionContentInput28 result;
    std::vector<std::filesystem::path> candidates;
    if (!decoded_root.empty()) candidates.push_back(decoded_root / "data" / "fusion.dat");
    for (const auto& suffix : {std::filesystem::path("data"), std::filesystem::path("dat/data"),
                              std::filesystem::path("assets/data"), std::filesystem::path("NTSD2.8/data")})
        candidates.push_back(extracted_root / suffix / "fusion.dat");
    for (const auto& candidate : candidates) {
        std::error_code error;
        if (std::filesystem::is_regular_file(candidate, error)) { result.selected_path = candidate; break; }
    }
    Bytes28 file_bytes;
    ntsd28::FusionCatalog28 catalog;
    if (result.selected_path.empty()) catalog = ntsd28::FusionCatalog28::locked_runtime_table_2833();
    else {
        file_bytes = content_read_bytes(result.selected_path);
        catalog = ntsd28::FusionCatalog28::parse_text(std::string(file_bytes.begin(), file_bytes.end()));
    }
    if (!catalog.ok()) throw std::runtime_error("selected fusion content failed strict parsing");
    Bytes28 semantic;
    content_string(semantic, "NTSD28-FUSION-SEMANTIC-v1");
    content_int32(semantic, static_cast<int>(catalog.records().size()));
    for (const auto& r : catalog.records())
        for (int value : {r.id1,r.id2,r.id3,r.hp,r.mp,r.respond,r.decrease,r.wait,r.state,
                          r.action,r.frame,r.chp,r.hit_ja,r.cover,r.front_hurt_action,r.back_hurt_action})
            content_int32(semantic, value);
    result.semantic_hash = content_sha256(semantic);
    Bytes28 input;
    content_string(input, "NTSD28-FUSION-INPUT-v1");
    content_string(input, result.selected_path.empty() ? "LOCKED_TABLE" : "FILE");
    content_string(input, content_hex(result.selected_path.empty() ? result.semantic_hash : content_sha256(file_bytes)));
    result.input_hash = content_sha256(input);
    return result;
}
// Alignment contract: NTSD28-R15-NATIVE-MODE-V2-CAPTURE-001.
struct ModeContentInput28 {
    std::filesystem::path parent_path, child_path;
    int parent_priority = -1, child_priority = -1;
    std::string child_virtual_path;
    Bytes28 input_hash, semantic_hash;
    ntsd28::NativeComboHud28 combo;
};

std::filesystem::path select_mode_candidate(
    const std::vector<std::filesystem::path>& candidates, int& priority) {
    for (std::size_t i = 0; i < candidates.size(); ++i) {
        std::error_code error;
        if (std::filesystem::is_regular_file(candidates[i], error)) {
            priority = static_cast<int>(i);
            return candidates[i];
        }
        if (error && error != std::errc::no_such_file_or_directory &&
            error != std::errc::not_a_directory)
            throw std::runtime_error("mode candidate cannot be inspected");
    }
    return {};
}

ModeContentInput28 capture_mode_input(const std::filesystem::path& root,
                                     const std::filesystem::path& vfs) {
    ModeContentInput28 result;
    std::vector<std::filesystem::path> parents;
    if (!vfs.empty()) parents.push_back(vfs / "decoded_dat/data/mode.dat");
    for (const auto& suffix : {"data", "dat/data", "assets/data", "NTSD2.8/data"})
        parents.push_back(root / suffix / "mode.dat");
    result.parent_path = select_mode_candidate(parents, result.parent_priority);
    if (result.parent_path.empty()) return result;
    const auto parent = content_read_bytes(result.parent_path);
    std::istringstream tokens(std::string(parent.begin(), parent.end()));
    bool in_record = false;
    std::string token;
    while (tokens >> token) {
        if (token == "<mode_information>") { in_record = true; continue; }
        if (token == "<mode_information_end>") { in_record = false; continue; }
        if (!in_record) continue;
        if (token == "file:") { tokens >> result.child_virtual_path; break; }
        if (token.rfind("file:", 0) == 0 && token.size() > 5) {
            result.child_virtual_path = token.substr(5); break;
        }
    }
    if (result.child_virtual_path.empty())
        throw std::runtime_error("mode parent has no selected child");
    auto normalized = result.child_virtual_path;
    std::replace(normalized.begin(), normalized.end(), '\\', '/');
    const auto relative = std::filesystem::u8path(normalized).lexically_normal();
    if (relative.empty() || relative.is_absolute())
        throw std::runtime_error("invalid selected mode child path");
    for (const auto& part : relative)
        if (part == "..") throw std::runtime_error("selected mode child escapes root");
    std::vector<std::filesystem::path> children;
    if (!vfs.empty()) {
        children.push_back(vfs / "decoded_dat" / relative);
    }
    children.push_back((vfs.empty() ? root / "assets/sprites" : vfs / "vfs") / relative);
    for (const auto& suffix : {"", "dat", "assets", "NTSD2.8"})
        children.push_back(root / suffix / relative);
    result.child_path = select_mode_candidate(children, result.child_priority);
    if (result.child_path.empty()) throw std::runtime_error("selected mode child is missing");
    const auto child = content_read_bytes(result.child_path);
    std::vector<std::string> diagnostics;
    result.combo = ntsd28::NativeComboHud28::parse_text(
        std::string(child.begin(), child.end()), diagnostics);
    if (!result.combo.complete() || !diagnostics.empty())
        throw std::runtime_error("selected mode combo failed strict parsing");
    Bytes28 input;
    content_string(input, "NTSD28-MODE-COMBO-INPUT-v1");
    content_string(input, result.child_virtual_path);
    content_string(input, content_hex(content_sha256(parent)));
    content_string(input, content_hex(content_sha256(child)));
    result.input_hash = content_sha256(input);
    Bytes28 semantic;
    content_string(semantic, "NTSD28-MODE-COMBO-SEMANTIC-v1");
    for (int value : {*result.combo.bound, *result.combo.facing,
                      *result.combo.respond, *result.combo.caughtact})
        content_int32(semantic, value);
    result.semantic_hash = content_sha256(semantic);
    return result;
}

bool same_combo(const ntsd28::NativeComboHud28& a, const ntsd28::NativeComboHud28& b) {
    return a.declared == b.declared && a.bound == b.bound && a.respond == b.respond &&
        a.offset_x == b.offset_x && a.offset_y == b.offset_y && a.facing == b.facing &&
        a.times == b.times && a.state == b.state && a.caughtact == b.caughtact &&
        a.effect == b.effect && a.width == b.width && a.height == b.height &&
        a.picture_virtual_path == b.picture_virtual_path && a.name == b.name;
}

// Alignment contract: NTSD28-R15-NATIVE-KIND-V3-CAPTURE-001.
struct KindContentInput28 {
    std::filesystem::path selected_path;
    int selected_priority = -1;
    Bytes28 input_hash, semantic_hash;
};

KindContentInput28 capture_kind_input(const std::filesystem::path& decoded_root,
                                      const std::filesystem::path& extracted_root) {
    KindContentInput28 result;
    std::vector<std::filesystem::path> candidates;
    if (!decoded_root.empty()) candidates.push_back(decoded_root / "data" / "kind.dat");
    for (const auto& suffix : {std::filesystem::path("data"), std::filesystem::path("dat/data"),
                              std::filesystem::path("assets/data"), std::filesystem::path("NTSD2.8/data")})
        candidates.push_back(extracted_root / suffix / "kind.dat");
    result.selected_path = select_mode_candidate(candidates, result.selected_priority);
    Bytes28 file_bytes;
    ntsd28::KindCatalog28 catalog;
    if (result.selected_path.empty()) catalog = ntsd28::KindCatalog28::locked_runtime_table_2833();
    else {
        file_bytes = content_read_bytes(result.selected_path);
        catalog = ntsd28::KindCatalog28::parse_text(
            std::string(file_bytes.begin(), file_bytes.end()));
    }
    if (!catalog.ok()) throw std::runtime_error("selected kind content failed strict parsing");
    Bytes28 semantic;
    content_string(semantic, "NTSD28-KIND-SEMANTIC-v1");
    content_int32(semantic, static_cast<int>(catalog.records().size()));
    for (const auto& record : catalog.records()) {
        content_int32(semantic, record.effect);
        content_int32(semantic, record.frame);
        content_int32(semantic, static_cast<int>(record.bound_ids.size()));
        for (int id : record.bound_ids) content_int32(semantic, id);
        content_int32(semantic, static_cast<int>(record.respond_ids.size()));
        for (int id : record.respond_ids) content_int32(semantic, id);
    }
    result.semantic_hash = content_sha256(semantic);
    Bytes28 input;
    content_string(input, "NTSD28-KIND-INPUT-v1");
    content_string(input, result.selected_path.empty() ? "LOCKED_TABLE" : "FILE");
    content_string(input, content_hex(result.selected_path.empty()
        ? result.semantic_hash : content_sha256(file_bytes)));
    result.input_hash = content_sha256(input);
    return result;
}

struct BattleContentInput28 {
    Bytes28 object_hash;
    FusionContentInput28 fusion;
    ModeContentInput28 mode;
    KindContentInput28 kind;
    Bytes28 composite_hash;
};
BattleContentInput28 capture_battle_content(const std::filesystem::path& resource_root,
                                          const std::filesystem::path& complete_vfs_root) {
    BattleContentInput28 result;
    result.object_hash = capture_content_raw(resource_root);
    result.fusion = capture_fusion_input(complete_vfs_root.empty() ? std::filesystem::path{} :
        complete_vfs_root / "decoded_dat", resource_root);
    result.mode = capture_mode_input(resource_root, complete_vfs_root);
    result.kind = capture_kind_input(complete_vfs_root.empty() ? std::filesystem::path{} :
        complete_vfs_root / "decoded_dat", resource_root);
    const std::string tag = result.mode.parent_path.empty()
        ? "NTSD28_LOGAN_BATTLE_INPUTS_V3_KIND_ONLY" : "NTSD28_LOGAN_BATTLE_INPUTS_V3";
    Bytes28 bytes(tag.begin(), tag.end()); bytes.push_back(0);
    for (const auto* digest : {&result.object_hash, &result.fusion.input_hash, &result.fusion.semantic_hash})
        bytes.insert(bytes.end(), digest->begin(), digest->end());
    if (!result.mode.parent_path.empty())
        for (const auto* digest : {&result.mode.input_hash, &result.mode.semantic_hash})
            bytes.insert(bytes.end(), digest->begin(), digest->end());
    for (const auto* digest : {&result.kind.input_hash, &result.kind.semantic_hash})
        bytes.insert(bytes.end(), digest->begin(), digest->end());
    result.composite_hash = content_sha256(bytes);
    return result;
}
bool same_battle_content(const BattleContentInput28& left, const BattleContentInput28& right) {
    return left.object_hash == right.object_hash && left.fusion.input_hash == right.fusion.input_hash &&
           left.fusion.semantic_hash == right.fusion.semantic_hash &&
           left.fusion.selected_path == right.fusion.selected_path &&
           left.mode.parent_path == right.mode.parent_path &&
           left.mode.child_path == right.mode.child_path &&
           left.mode.parent_priority == right.mode.parent_priority &&
           left.mode.child_priority == right.mode.child_priority &&
           left.mode.child_virtual_path == right.mode.child_virtual_path &&
           left.mode.input_hash == right.mode.input_hash &&
           left.mode.semantic_hash == right.mode.semantic_hash &&
           left.kind.selected_path == right.kind.selected_path &&
           left.kind.selected_priority == right.kind.selected_priority &&
           left.kind.input_hash == right.kind.input_hash &&
           left.kind.semantic_hash == right.kind.semantic_hash;
}

std::string capture_content_json(const BattleContentInput28& content) {
    const auto& raw = content.composite_hash;
    const std::string tag = "NTSD28_LOGAN_DAT_SEMANTICS_V3";
    Bytes28 input(tag.begin(), tag.end());
    input.push_back(0);
    input.insert(input.end(), raw.begin(), raw.end());
    const auto semantic = content_sha256(input);
    std::uint64_t projection = 0;
    for (unsigned int index = 0; index < 8U; ++index)
        projection |= static_cast<std::uint64_t>(semantic[index]) << (index * 8U);
    if (projection == 0) projection = 1;
    std::ostringstream hex_projection;
    hex_projection << std::uppercase << std::hex << std::setw(16) << std::setfill('0') << projection;
    const std::string mode_fields = content.mode.parent_path.empty() ? "" :
        "\"modeInputSha256\":\"" + content_hex(content.mode.input_hash) +
        "\",\"modeSemanticSha256\":\"" + content_hex(content.mode.semantic_hash) + "\",";
    return "{\"policy\":\"logan-dat-character-images\",\"scope\":\"" +
        std::string(content.mode.parent_path.empty() ? "catalog-object-fusion-kind-definitions" :
            "catalog-object-fusion-mode-kind-definitions") + "\",\"battleInputContract\":\"" +
        (content.mode.parent_path.empty() ? "NTSD28_LOGAN_BATTLE_INPUTS_V3_KIND_ONLY" :
            "NTSD28_LOGAN_BATTLE_INPUTS_V3") + "\"," + mode_fields +
        "\"profile\":\"logan-runtime\",\"objectDefinitionSha256\":\"" + content_hex(content.object_hash) +
        "\",\"fusionInputSha256\":\"" + content_hex(content.fusion.input_hash) +
        "\",\"fusionSemanticSha256\":\"" + content_hex(content.fusion.semantic_hash) +
        "\",\"kindInputSha256\":\"" + content_hex(content.kind.input_hash) +
        "\",\"kindSemanticSha256\":\"" + content_hex(content.kind.semantic_hash) +
        "\",\"rawDefinitionSha256\":\"" + content_hex(raw) +
        "\",\"decodeContract\":\"" + tag + "\",\"semanticSha256\":\"" + content_hex(semantic) +
        "\",\"catalogFingerprint64\":\"" + hex_projection.str() +
        "\",\"schemas\":{\"entityRuntime\":17,\"aggregate\":28,\"checksum\":31,"
        "\"characterShell\":2,\"entityBaseShell\":2}}";
}

struct Options28 {
    std::filesystem::path scenario;
    std::filesystem::path output;
    std::filesystem::path domain_output;
    bool domain_v2 = false;
    bool domain_version_specified = false;
    std::filesystem::path b2_input_rng_output;
    std::filesystem::path resource_root;
    std::filesystem::path complete_vfs_root;
    std::string authority_source_manifest_sha256;
    std::string capture_runner_source_sha256;
    std::string capture_binary_sha256;
};

struct SlotEpochNormalizer28 {
    std::array<std::uint64_t, ntsd28::EngineProfile28::maximum_slots> epochs{};
    std::array<std::uint64_t, ntsd28::EngineProfile28::maximum_slots>
        presentation_generations{};
    std::array<bool, ntsd28::EngineProfile28::maximum_slots> observed{};

    std::uint64_t observe(std::size_t slot,
                          std::uint64_t presentation_generation) {
        if (!observed[slot]) {
            observed[slot] = true;
            epochs[slot] = 1;
        } else if (presentation_generations[slot] != presentation_generation) {
            ++epochs[slot];
        }
        presentation_generations[slot] = presentation_generation;
        return epochs[slot];
    }
};

bool is_sha256(const std::string& value) {
    if (value.size() != 64) return false;
    for (const char character : value) {
        if (!((character >= '0' && character <= '9') ||
              (character >= 'A' && character <= 'F') ||
              (character >= 'a' && character <= 'f'))) {
            return false;
        }
    }
    return true;
}

std::string narrow_ascii(const std::wstring& value) {
    std::string result;
    result.reserve(value.size());
    for (const wchar_t character : value) {
        if (character > 0x7f) return {};
        result.push_back(static_cast<char>(character));
    }
    return result;
}

bool parse_options(int count, wchar_t** arguments, Options28& output) {
    for (int index = 1; index < count; ++index) {
        if (index + 1 >= count) return false;
        const std::wstring option = arguments[index];
        const std::wstring value = arguments[++index];
        if (option == L"--scenario") output.scenario = value;
        else if (option == L"--output") output.output = value;
        else if (option == L"--domain-output") output.domain_output = value;
        else if (option == L"--domain-version") {
            if (output.domain_version_specified || (value != L"1" && value != L"2")) return false;
            output.domain_version_specified = true;
            output.domain_v2 = value == L"2";
        }
        else if (option == L"--b2-input-rng-output") {
            output.b2_input_rng_output = value;
        }
        else if (option == L"--resource-root") output.resource_root = value;
        else if (option == L"--complete-vfs-root") {
            output.complete_vfs_root = value;
        } else if (option == L"--authority-source-manifest-sha256") {
            output.authority_source_manifest_sha256 = narrow_ascii(value);
        } else if (option == L"--capture-runner-source-sha256") {
            output.capture_runner_source_sha256 = narrow_ascii(value);
        } else if (option == L"--capture-binary-sha256") {
            output.capture_binary_sha256 = narrow_ascii(value);
        } else {
            return false;
        }
    }
    if (output.domain_version_specified && output.domain_output.empty()) return false;
    std::array<std::filesystem::path, 3> outputs{{
        output.output,
        output.domain_output,
        output.b2_input_rng_output,
    }};
    for (std::size_t left = 0; left < outputs.size(); ++left) {
        if (outputs[left].empty()) continue;
        for (std::size_t right = left + 1; right < outputs.size(); ++right) {
            if (outputs[right].empty()) continue;
            if (std::filesystem::absolute(outputs[left]).lexically_normal() ==
                std::filesystem::absolute(outputs[right]).lexically_normal()) {
                return false;
            }
        }
    }
    return !output.scenario.empty() && !output.output.empty() &&
           !output.resource_root.empty() && !output.complete_vfs_root.empty() &&
           is_sha256(output.authority_source_manifest_sha256) &&
           is_sha256(output.capture_runner_source_sha256) &&
           is_sha256(output.capture_binary_sha256);
}

std::string json_escape(const std::string& value) {
    std::ostringstream output;
    for (const unsigned char character : value) {
        switch (character) {
        case '"': output << "\\\""; break;
        case '\\': output << "\\\\"; break;
        case '\b': output << "\\b"; break;
        case '\f': output << "\\f"; break;
        case '\n': output << "\\n"; break;
        case '\r': output << "\\r"; break;
        case '\t': output << "\\t"; break;
        default:
            if (character < 0x20) {
                constexpr char hex[] = "0123456789ABCDEF";
                output << "\\u00" << hex[character >> 4U]
                       << hex[character & 0x0fU];
            } else {
                output << static_cast<char>(character);
            }
            break;
        }
    }
    return output.str();
}

std::string hexadecimal64(std::uint64_t value) {
    std::ostringstream output;
    output << std::uppercase << std::hex << std::setw(16)
           << std::setfill('0') << value;
    return output.str();
}

std::uint32_t contract_input_mask(const ntsd28::InputButtons28& buttons) {
    std::uint32_t result = 0;
    if (buttons[ntsd28::InputKey28::right]) result |= 1U << 0U;
    if (buttons[ntsd28::InputKey28::left]) result |= 1U << 1U;
    if (buttons[ntsd28::InputKey28::depth_up]) result |= 1U << 2U;
    if (buttons[ntsd28::InputKey28::depth_down]) result |= 1U << 3U;
    if (buttons[ntsd28::InputKey28::attack]) result |= 1U << 4U;
    if (buttons[ntsd28::InputKey28::jump]) result |= 1U << 5U;
    if (buttons[ntsd28::InputKey28::defend]) result |= 1U << 6U;
    return result;
}

struct DomainOccupant28 {
    std::size_t slot = 0;
    std::uint64_t allocation_epoch = 0;
    int object_id = 0;
};

using DomainOccupants28 = std::map<std::size_t, DomainOccupant28>;

DomainOccupants28 capture_domain_occupants(
    const ntsd28::BattleWorld28* world,
    SlotEpochNormalizer28& slot_epochs) {
    DomainOccupants28 result;
    if (world == nullptr) return result;
    for (std::size_t slot = 0;
         slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
        const auto* entity = world->entity(slot);
        if (entity == nullptr) continue;
        result.emplace(slot,
                       DomainOccupant28{
                           slot,
                           slot_epochs.observe(
                               slot, entity->presentation_generation),
                           entity->object_id});
    }
    return result;
}

void write_domain_occupants(std::ostream& output,
                            const DomainOccupants28& occupants) {
    output << '[';
    bool first = true;
    for (const auto& [slot, occupant] : occupants) {
        if (!first) output << ',';
        first = false;
        output << "{\"slot\":" << slot
               << ",\"allocationEpoch\":" << occupant.allocation_epoch
               << ",\"objectId\":" << occupant.object_id << '}';
    }
    output << ']';
}

void write_nullable_uint64(std::ostream& output,
                           const std::optional<std::uint64_t>& value) {
    if (value) output << *value;
    else output << "null";
}

void write_nullable_int(std::ostream& output,
                        const std::optional<int>& value) {
    if (value) output << *value;
    else output << "null";
}

void write_domain_event(std::ostream& output,
                        bool& first,
                        const char* kind,
                        std::size_t slot,
                        const std::optional<std::uint64_t>& previous_epoch,
                        const std::optional<std::uint64_t>& current_epoch,
                        const std::optional<int>& previous_object_id,
                        const std::optional<int>& current_object_id) {
    if (!first) output << ',';
    first = false;
    output << "{\"kind\":\"" << kind << "\",\"slot\":" << slot
           << ",\"previousAllocationEpoch\":";
    write_nullable_uint64(output, previous_epoch);
    output << ",\"currentAllocationEpoch\":";
    write_nullable_uint64(output, current_epoch);
    output << ",\"previousObjectId\":";
    write_nullable_int(output, previous_object_id);
    output << ",\"currentObjectId\":";
    write_nullable_int(output, current_object_id);
    output << '}';
}

void write_domain_events(std::ostream& output,
                         const DomainOccupants28& previous,
                         const DomainOccupants28& current,
                         bool domain_v2) {
    std::map<std::size_t, bool> slots;
    for (const auto& [slot, occupant] : previous) {
        static_cast<void>(occupant);
        slots.emplace(slot, true);
    }
    for (const auto& [slot, occupant] : current) {
        static_cast<void>(occupant);
        slots.emplace(slot, true);
    }

    output << '[';
    bool first = true;
    for (const auto& [slot, present] : slots) {
        static_cast<void>(present);
        const auto old_iterator = previous.find(slot);
        const auto new_iterator = current.find(slot);
        if (old_iterator == previous.end()) {
            const auto& value = new_iterator->second;
            write_domain_event(output,
                               first,
                               "birth",
                               slot,
                               std::nullopt,
                               value.allocation_epoch,
                               std::nullopt,
                               value.object_id);
            continue;
        }
        if (new_iterator == current.end()) {
            const auto& value = old_iterator->second;
            write_domain_event(output,
                               first,
                               "death",
                               slot,
                               value.allocation_epoch,
                               std::nullopt,
                               value.object_id,
                               std::nullopt);
            continue;
        }

        const auto& old_value = old_iterator->second;
        const auto& new_value = new_iterator->second;
        if (old_value.allocation_epoch == new_value.allocation_epoch) {
            if (old_value.object_id != new_value.object_id) {
                if (!domain_v2) {
                    throw std::runtime_error(
                        "domain occupant changed without allocation epoch");
                }
                write_domain_event(output,
                                   first,
                                   "object-id-change",
                                   slot,
                                   old_value.allocation_epoch,
                                   new_value.allocation_epoch,
                                   old_value.object_id,
                                   new_value.object_id);
            }
            continue;
        }
        if (new_value.allocation_epoch <= old_value.allocation_epoch) {
            throw std::runtime_error("domain allocation epoch is not monotonic");
        }
        write_domain_event(output,
                           first,
                           "reuse",
                           slot,
                           old_value.allocation_epoch,
                           new_value.allocation_epoch,
                           old_value.object_id,
                           new_value.object_id);
    }
    output << ']';
}

std::uint64_t call_delta(std::uint64_t previous,
                         std::uint64_t current) {
    if (current < previous) {
        throw std::runtime_error("RNG call total moved backwards");
    }
    return current - previous;
}

void write_entity(std::ostream& output,
                  const ntsd28::EntityState28& entity,
                  std::uint64_t allocation_epoch) {
    const auto* current_frame = entity.definition != nullptr
                                    ? entity.definition->frame(entity.frame.action)
                                    : nullptr;
    const int frame_state = current_frame != nullptr
                                ? current_frame->values.integer("state").value_or(0)
                                : 0;
    output << std::setprecision(std::numeric_limits<double>::max_digits10)
           << "{\"slot\":" << entity.slot
           << ",\"allocationEpoch\":" << allocation_epoch
           << ",\"active\":true"
           << ",\"identity\":{"
           << "\"objectId\":" << entity.object_id
           << ",\"objectType\":" << entity.object_type
           << ",\"controlSlot\":" << entity.control_slot_000
           << ",\"ownerSlot\":" << entity.owner_slot
           << ",\"battleGroup\":" << entity.battle_group
           << ",\"participantClass\":" << entity.participant_class_344
           << "},\"frame\":{"
           << "\"action\":" << entity.frame.action
           << ",\"actionLatch\":" << entity.frame.action_latch
           << ",\"previousAction\":" << entity.frame.previous_action_078
           << ",\"tickActionSnapshot\":" << entity.frame.tick_action_snapshot
           << ",\"frameCounter\":" << entity.frame.frame_counter
           << ",\"frameState\":" << frame_state
           << ",\"facingLeft\":" << (entity.frame.facing ? "true" : "false")
           << "},\"position\":{"
           << "\"x\":" << entity.position.x
           << ",\"y\":" << entity.position.y
           << ",\"z\":" << entity.position.z
           << ",\"preciseX\":" << entity.position.precise_x
           << ",\"preciseY\":" << entity.position.precise_y
           << ",\"preciseZ\":" << entity.position.precise_z
           << "},\"motion\":{"
           << "\"x\":" << entity.motion.x
           << ",\"y\":" << entity.motion.y
           << ",\"z\":" << entity.motion.z
           << "},\"vitals\":{"
           << "\"currentHp\":" << entity.current_hp
           << ",\"effectiveMaxHp\":" << entity.effective_max_hp
           << ",\"baseMaxHp\":" << entity.base_max_hp
           << ",\"currentMp\":" << entity.current_mp
           << ",\"baseMaxMp\":" << entity.base_max_mp
           << ",\"reviveLives\":" << entity.revive_lives_30c
           << ",\"reviveNextLives\":" << entity.revive_next_lives_310
           << ",\"reviveNextHp\":" << entity.revive_next_hp_314
           << "},\"combat\":{"
           << "\"runtimeStateCode\":" << entity.runtime_state_code
           << ",\"renderPhase\":" << entity.render_phase_008
           << ",\"attackerRest\":" << entity.attacker_rest
           << ",\"collisionYReference\":" << entity.collision_y_reference
           << ",\"platformSourceSlot\":" << entity.platform_source_slot_f4
           << ",\"hitReactionTimer\":" << entity.hit_reaction_timer
           << ",\"bdefendAccumulator\":" << entity.bdefend_accumulator
           << ",\"runtimeArmorHp\":" << entity.runtime_armor_hp
           << ",\"armorRecoveryTimer\":" << entity.armor_recovery_timer
           << ",\"motionHoldTimer\":" << entity.motion_hold_timer
           << ",\"weaponHp\":" << entity.weapon_hp_31c
           << ",\"specialHitLatch0eb\":"
           << (entity.special_hit_latch_0eb ? "true" : "false")
           << ",\"environmentState\":" << entity.environment_state_320
           << ",\"environmentSourceSlot\":" << entity.environment_source_slot_160
           << ",\"objectAiExcludedGroupSourceSlot\":" << entity.object_ai_excluded_group_source_slot_2f8
           << "},\"lifecycle\":{"
           << "\"resolutionPending\":"
           << (entity.lifecycle_resolution_pending ? "true" : "false")
           << ",\"code\":" << entity.lifecycle_code << "}}";
}

void write_header(std::ostream& output,
                  const Options28& options,
                  const ntsd28_playable::Scenario28& scenario,
                  const std::string& content_json) {
    output << "{\"kind\":\"header\""
           << ",\"schema\":\"" << capture_schema << "\""
           << ",\"content\":" << content_json
           << ",\"certificateEligible\":false"
           << ",\"evidenceClass\":\"SOURCE_MODEL_DIAGNOSTIC_ONLY\""
           << ",\"formalExeSha256\":\"" << formal_exe_sha256 << "\""
           << ",\"authoritySourceManifestSha256\":\""
           << options.authority_source_manifest_sha256 << "\""
           << ",\"captureRunnerSourceSha256\":\""
           << options.capture_runner_source_sha256 << "\""
           << ",\"captureBinarySha256\":\""
           << options.capture_binary_sha256 << "\""
           << ",\"scenarioReferenceExeSha256\":\""
           << json_escape(scenario.reference_exe_sha256) << "\""
           << ",\"scenarioDataSha256\":\""
           << json_escape(scenario.data_sha256) << "\""
           << ",\"scenarioId\":\""
           << json_escape(options.scenario.stem().u8string()) << "\""
           << ",\"firstCompletedTick\":1"
           << ",\"expectedTickCount\":" << scenario.ticks
           << ",\"slotCapacity\":" << ntsd28::EngineProfile28::maximum_slots
           << "}\n";
}

void write_domain_header(
    std::ostream& output,
    const Options28& options,
    const ntsd28_playable::Scenario28& scenario,
    const ntsd28::NativeRandomState28& initial_random,
    const DomainOccupants28& initial_occupants) {
    output << "{\"kind\":\"header\""
           << ",\"schema\":\"ntsd28-logan-b0-domain-raw-v"
           << (options.domain_v2 ? "2" : "1") << "\""
           << ",\"producer\":\"authority-source-model\""
           << ",\"evidenceClass\":\"SOURCE_MODEL_DIAGNOSTIC_ONLY\""
           << ",\"certificateEligible\":false"
           << ",\"formalExeSha256\":\"" << formal_exe_sha256 << "\""
           << ",\"scenarioId\":\""
           << json_escape(options.scenario.stem().u8string()) << "\""
           << ",\"firstCompletedTick\":1"
           << ",\"expectedTickCount\":" << scenario.ticks
           << ",\"slotCapacity\":"
           << ntsd28::EngineProfile28::maximum_slots
           << ",\"streamAvailability\":{"
           << "\"authorityCrt\":\"available\""
           << ",\"authoritySynchronized\":\"available\""
           << ",\"unityDeterministic\":\"missing\"}"
           << ",\"perCallTraceAvailability\":{"
           << "\"authorityCrt\":\"missing\""
           << ",\"authoritySynchronized\":\"missing\""
           << ",\"unityDeterministic\":\"missing\"}"
           << ",\"initialRngTotalCalls\":{"
           << "\"authorityCrt\":" << initial_random.crt_calls
           << ",\"authoritySynchronized\":"
           << initial_random.synchronized.calls
           << ",\"unityDeterministic\":null}"
           << ",\"initialOccupants\":";
    write_domain_occupants(output, initial_occupants);
    output << ",\"lifecycleDeltaProvenance\":\"snapshot-derived\"}\n";
}

void write_domain_tick(
    std::ostream& output,
    const ntsd28_playable::GameSession28& session,
    const ntsd28_playable::Scenario28& scenario,
    const std::array<ntsd28::InputButtons28,
                     ntsd28_playable::maximum_native_combatants28>& buttons,
    const ntsd28::NativeRandomState28& previous_random,
    const DomainOccupants28& previous_occupants,
    const DomainOccupants28& current_occupants,
    bool domain_v2) {
    const auto* world = session.world();
    if (world == nullptr) {
        throw std::runtime_error("domain capture world is missing");
    }
    const auto random = world->random().state();
    output << "{\"kind\":\"tick\",\"completedTick\":"
           << world->sequence()
           << ",\"input\":{\"captureBoundary\":\"applied-to-tick\""
           << ",\"players\":[";
    bool first = true;
    for (const auto& combatant : scenario.battle.combatants) {
        if (!first) output << ',';
        first = false;
        output << "{\"playerSlot\":" << combatant.slot
               << ",\"heldMask\":"
               << contract_input_mask(buttons[combatant.slot]) << '}';
    }
    output << "]},\"rng\":{"
           << "\"authorityCrt\":{\"availability\":\"available\""
           << ",\"state\":" << random.crt_state
           << ",\"totalCalls\":" << random.crt_calls
           << ",\"tickCallCount\":"
           << call_delta(previous_random.crt_calls, random.crt_calls)
           << ",\"calls\":null}"
           << ",\"authoritySynchronized\":{"
           << "\"availability\":\"available\",\"state\":{"
           << "\"counter\":" << random.synchronized.counter
           << ",\"index\":" << random.synchronized.index
           << ",\"tableHash64\":\""
           << hexadecimal64(world->random().synchronized_table_hash()) << "\""
           << ",\"lastCallSite\":"
           << random.synchronized.last_call_site << '}'
           << ",\"totalCalls\":" << random.synchronized.calls
           << ",\"tickCallCount\":"
           << call_delta(previous_random.synchronized.calls,
                         random.synchronized.calls)
           << ",\"calls\":null}"
           << ",\"unityDeterministic\":{\"availability\":\"missing\""
           << ",\"state\":null,\"totalCalls\":null"
           << ",\"tickCallCount\":null,\"calls\":null}}"
           << ",\"slots\":{\"capacity\":"
           << ntsd28::EngineProfile28::maximum_slots
           << ",\"occupants\":";
    write_domain_occupants(output, current_occupants);
    output << "},\"lifecycleDelta\":{"
           << "\"provenance\":\"snapshot-derived\",\"events\":";
    write_domain_events(output, previous_occupants, current_occupants, domain_v2);
    output << "}}\n";
}

void write_b2_initial_random(
    std::ostream& output,
    const ntsd28::NativeRandomState28& random,
    std::uint64_t table_hash) {
    output << "{\"crt\":{\"state\":" << random.crt_state
           << ",\"totalCalls\":" << random.crt_calls
           << "},\"synchronized\":{\"counter\":"
           << random.synchronized.counter
           << ",\"index\":" << random.synchronized.index
           << ",\"tableHash64\":\"" << hexadecimal64(table_hash)
           << "\",\"lastCallSite\":"
           << random.synchronized.last_call_site
           << ",\"totalCalls\":" << random.synchronized.calls << "}}";
}

void write_b2_crt_calls(std::ostream& output) {
    output << '[';
    for (std::size_t index = 0; index < direct_crt_calls.size(); ++index) {
        if (index != 0) output << ',';
        const auto& call = direct_crt_calls[index];
        output << "{\"result\":" << call.result
               << ",\"stateAfter\":" << call.state_after
               << ",\"totalCalls\":" << call.total_calls << '}';
    }
    output << ']';
}

void write_b2_synchronized_calls(std::ostream& output) {
    output << '[';
    for (std::size_t index = 0;
         index < direct_synchronized_calls.size();
         ++index) {
        if (index != 0) output << ',';
        const auto& call = direct_synchronized_calls[index];
        output << "{\"callSite\":" << call.call_site
               << ",\"upperBound\":" << call.upper_bound
               << ",\"result\":" << call.result
               << ",\"counterAfter\":" << call.counter_after
               << ",\"indexAfter\":" << call.index_after
               << ",\"totalCalls\":" << call.total_calls << '}';
    }
    output << ']';
}

void write_b2_input_rng_header(
    std::ostream& output,
    const Options28& options,
    const ntsd28_playable::Scenario28& scenario,
    const ntsd28::BattleWorld28& world) {
    const auto random = world.random().state();
    output << "{\"kind\":\"header\""
           << ",\"schema\":\"ntsd28-logan-b2-input-rng-joint-raw-v3\""
           << ",\"producer\":\"authority-source-model\""
           << ",\"evidenceClass\":\"SOURCE_MODEL_DIAGNOSTIC_ONLY\""
           << ",\"certificateEligible\":false"
           << ",\"formalExeSha256\":\"" << formal_exe_sha256 << "\""
           << ",\"scenarioId\":\""
           << json_escape(options.scenario.stem().u8string()) << "\""
           << ",\"firstCompletedTick\":1"
           << ",\"expectedTickCount\":" << scenario.ticks
           << ",\"keyOrder\":[\"W\",\"S\",\"A\",\"D\",\"J\",\"K\",\"L\"]"
           << ",\"perCallTraceAvailability\":{"
           << "\"crt\":\"available-completed-ticks\""
           << ",\"synchronized\":\"available-completed-ticks\"}"
           << ",\"perCallTraceScope\":{\"aiCursorExcluded\":false"
           << ",\"humanScenarioRequired\":false"
           << ",\"initializationExcluded\":true}"
           << ",\"initialInputPhase\":"
           << world.input_update_phase_4a0b90()
           << ",\"initialRng\":";
    write_b2_initial_random(
        output, random, world.random().synchronized_table_hash());
    output << "}\n";
}

void write_b2_edge_window(std::ostream& output,
                          const ntsd28::EntityInputState28& input) {
    const auto edge = [&](ntsd28::InputKey28 key) {
        return input.edge_window[static_cast<std::size_t>(key)];
    };
    output << "{\"attack\":" << edge(ntsd28::InputKey28::attack)
           << ",\"jump\":" << edge(ntsd28::InputKey28::jump)
           << ",\"defend\":" << edge(ntsd28::InputKey28::defend)
           << ",\"right\":" << edge(ntsd28::InputKey28::right)
           << ",\"left\":" << edge(ntsd28::InputKey28::left)
           << ",\"up\":" << edge(ntsd28::InputKey28::depth_up)
           << ",\"down\":" << edge(ntsd28::InputKey28::depth_down)
           << '}';
}

template <typename T, std::size_t Count>
void write_numeric_array(std::ostream& output,
                         const std::array<T, Count>& values) {
    output << '[';
    for (std::size_t index = 0; index < values.size(); ++index) {
        if (index != 0) output << ',';
        output << static_cast<long long>(values[index]);
    }
    output << ']';
}

void write_b2_input_entity(std::ostream& output,
                           const ntsd28::EntityState28& entity,
                           std::uint64_t allocation_epoch) {
    output << "{\"slot\":" << entity.slot
           << ",\"allocationEpoch\":" << allocation_epoch
           << ",\"objectId\":" << entity.object_id
           << ",\"input\":{\"currentMask\":"
           << contract_input_mask(entity.input.current)
           << ",\"previousMask\":"
           << contract_input_mask(entity.input.previous)
           << ",\"edgeWindow\":";
    write_b2_edge_window(output, entity.input);
    output << ",\"defendReentryCooldown\":"
           << entity.input.defend_reentry_cooldown
           << ",\"comboState\":";
    write_numeric_array(output, entity.input.combo_state);
    output << ",\"proxyTail\":"
           << static_cast<unsigned int>(entity.input.proxy_tail_de)
           << ",\"keyHistory\":";
    write_numeric_array(output, entity.input.key_history);
    output << ",\"runAccumulator\":" << entity.input.run_accumulator
           << ",\"lastAction\":" << entity.input_last_action_144
           << ",\"remapState\":" << entity.input_remap_state_138
           << ",\"remapIndices\":";
    write_numeric_array(output, entity.input_remap_indices_13c);
    output << ",\"boundState\":" << entity.bound_state_198
           << ",\"globalRecordState\":"
           << entity.input_global_record_state_20 << "}}";
}

void write_b2_input_rng_tick(
    std::ostream& output,
    const ntsd28_playable::GameSession28& session,
    const ntsd28::NativeRandomState28& previous_random,
    SlotEpochNormalizer28& slot_epochs) {
    const auto* world = session.world();
    if (world == nullptr) {
        throw std::runtime_error("B2 input/RNG capture world is missing");
    }
    const auto random = world->random().state();
    output << "{\"kind\":\"tick\",\"completedTick\":"
           << world->sequence()
           << ",\"inputPhase\":"
           << world->input_update_phase_4a0b90()
           << ",\"rng\":{\"crt\":{\"state\":" << random.crt_state
           << ",\"totalCalls\":" << random.crt_calls
           << ",\"tickCallCount\":"
           << call_delta(previous_random.crt_calls, random.crt_calls)
           << ",\"calls\":";
    write_b2_crt_calls(output);
    output << "},\"synchronized\":{\"state\":{"
           << "\"counter\":" << random.synchronized.counter
           << ",\"index\":" << random.synchronized.index
           << ",\"tableHash64\":\""
           << hexadecimal64(world->random().synchronized_table_hash())
           << "\",\"lastCallSite\":"
           << random.synchronized.last_call_site << "}"
           << ",\"totalCalls\":" << random.synchronized.calls
           << ",\"tickCallCount\":"
           << call_delta(previous_random.synchronized.calls,
                         random.synchronized.calls)
           << ",\"calls\":";
    write_b2_synchronized_calls(output);
    output << "}},\"entities\":[";
    bool first = true;
    for (std::size_t slot = 0;
         slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
        const auto* entity = world->entity(slot);
        if (entity == nullptr) continue;
        if (!first) output << ',';
        first = false;
        write_b2_input_entity(
            output,
            *entity,
            slot_epochs.observe(slot, entity->presentation_generation));
    }
    output << "]}\n";
}

void write_tick(std::ostream& output,
                const ntsd28_playable::GameSession28& session,
                SlotEpochNormalizer28& slot_epochs) {
    const auto* world = session.world();
    output << "{\"kind\":\"tick\",\"completedTick\":"
           << (world != nullptr ? world->sequence() : 0)
           << ",\"entities\":[";
    bool first = true;
    if (world != nullptr) {
        for (std::size_t slot = 0;
             slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
            const auto* entity = world->entity(slot);
            if (entity == nullptr) continue;
            if (!first) output << ',';
            first = false;
            write_entity(output,
                         *entity,
                         slot_epochs.observe(slot,
                                             entity->presentation_generation));
        }
    }
    output << "]}\n";
}

struct CaptureBundleGuard28 {
    std::vector<std::filesystem::path> paths;
    bool committed = false;
    ~CaptureBundleGuard28() noexcept {
        if (committed) return;
        for (const auto& path : paths) {
            if (path.empty()) continue;
            try {
                // A failed bundle must never leave a standalone valid side stream.
                std::ofstream failed(path, std::ios::binary | std::ios::app);
                failed << "INVALID_CAPTURE_BUNDLE\n";
            } catch (...) {}
        }
    }
};

int run(int count, wchar_t** arguments) {
    Options28 options;
    if (!parse_options(count, arguments, options)) {
        std::cerr << "usage: authority_source_capture --scenario <json> "
                     "--output <jsonl> [--domain-output <jsonl>] [--domain-version <1|2>] "
                     "[--b2-input-rng-output <jsonl>] "
                     "--resource-root <folder> "
                     "--complete-vfs-root <folder> "
                     "--authority-source-manifest-sha256 <sha> "
                     "--capture-runner-source-sha256 <sha> "
                     "--capture-binary-sha256 <sha>\n";
        return 2;
    }

    CaptureBundleGuard28 bundle{{options.output, options.domain_output, options.b2_input_rng_output}};
    const auto loaded = ntsd28_playable::ScenarioLoader28::load(options.scenario);
    if (!loaded.success) {
        for (const auto& diagnostic : loaded.diagnostics) {
            std::cerr << "scenario: " << diagnostic << '\n';
        }
        return 3;
    }

    const auto content_raw = capture_battle_content(options.resource_root, options.complete_vfs_root);
    ntsd28_playable::GameSession28 session(
        options.resource_root, options.complete_vfs_root);
    std::string error;
    if (!session.initialize(loaded.scenario.battle, error)) {
        std::cerr << "session: " << error << '\n';
        return 4;
    }
    const auto& actual_combo = session.config().native_combo_hud;
    if (content_raw.mode.parent_path.empty() ? actual_combo.has_value() :
        (!actual_combo || !same_combo(content_raw.mode.combo, *actual_combo)))
        throw std::runtime_error("initialized session combo differs from captured mode input");
    // Alignment contract: NTSD28-B2-DIRECT-RNG-PER-CALL-JOINT-TRACE-001.
    // Initialization table/BGM calls remain represented by scalar state; the
    // per-call window begins at the first completed battle tick.
    direct_crt_calls.clear();
    direct_synchronized_calls.clear();

    std::ofstream output(options.output, std::ios::binary | std::ios::trunc);
    if (!output) {
        std::cerr << "unable to open capture output\n";
        return 5;
    }
    if (!same_battle_content(capture_battle_content(options.resource_root, options.complete_vfs_root), content_raw))
        throw std::runtime_error("content inputs changed during session initialization");
    write_header(output, options, loaded.scenario, capture_content_json(content_raw));

    std::ofstream domain_output;
    if (!options.domain_output.empty()) {
        domain_output.open(options.domain_output,
                           std::ios::binary | std::ios::trunc);
        if (!domain_output) {
            std::cerr << "unable to open domain capture output\n";
            return 5;
        }
    }

    std::ofstream b2_input_rng_output;
    if (!options.b2_input_rng_output.empty()) {
        b2_input_rng_output.open(options.b2_input_rng_output,
                                 std::ios::binary | std::ios::trunc);
        if (!b2_input_rng_output) {
            std::cerr << "unable to open B2 input/RNG capture output\n";
            return 5;
        }
    }

    std::size_t input_index = 0;
    SlotEpochNormalizer28 slot_epochs;
    const auto* initial_world = session.world();
    if (initial_world == nullptr) {
        std::cerr << "initialized session has no world\n";
        return 4;
    }
    DomainOccupants28 previous_occupants = capture_domain_occupants(
        initial_world, slot_epochs);
    ntsd28::NativeRandomState28 previous_random =
        initial_world->random().state();
    if (domain_output) {
        write_domain_header(domain_output,
                            options,
                            loaded.scenario,
                            previous_random,
                            previous_occupants);
    }
    if (b2_input_rng_output) {
        write_b2_input_rng_header(
            b2_input_rng_output,
            options,
            loaded.scenario,
            *initial_world);
    }
    for (std::uint64_t tick = 0; tick < loaded.scenario.ticks; ++tick) {
        std::array<ntsd28::InputButtons28,
                   ntsd28_playable::maximum_native_combatants28> buttons{};
        while (input_index < loaded.scenario.inputs.size() &&
               loaded.scenario.inputs[input_index].tick == tick) {
            const auto& input = loaded.scenario.inputs[input_index++];
            buttons[input.slot] = input.keys;
        }
        for (std::size_t slot = 0; slot < buttons.size(); ++slot) {
            session.set_input(slot, buttons[slot]);
        }
        direct_crt_calls.clear();
        direct_synchronized_calls.clear();
        session.step();
        write_tick(output, session, slot_epochs);
        const auto* current_world = session.world();
        if (domain_output) {
            DomainOccupants28 current_occupants = capture_domain_occupants(
                current_world, slot_epochs);
            write_domain_tick(domain_output,
                              session,
                              loaded.scenario,
                              buttons,
                              previous_random,
                              previous_occupants,
                              current_occupants,
                              options.domain_v2);
            previous_occupants = std::move(current_occupants);
        }
        if (b2_input_rng_output) {
            write_b2_input_rng_tick(
                b2_input_rng_output,
                session,
                previous_random,
                slot_epochs);
        }
        if (domain_output || b2_input_rng_output) {
            previous_random = current_world->random().state();
        }
    }
    if (!same_battle_content(capture_battle_content(options.resource_root, options.complete_vfs_root), content_raw)) {
        output << "INVALID_CONTENT_INPUTS_CHANGED\n";
        throw std::runtime_error("content inputs changed during simulation");
    }
    output.flush();
    if (domain_output.is_open()) domain_output.flush();
    if (b2_input_rng_output.is_open()) b2_input_rng_output.flush();
    if (!output ||
        (domain_output.is_open() && !domain_output) ||
        (b2_input_rng_output.is_open() && !b2_input_rng_output)) {
        std::cerr << "capture write failed\n";
        return 6;
    }
    bundle.committed = true;
    std::cout << "capture_schema=" << capture_schema << '\n'
              << "capture_ticks=" << loaded.scenario.ticks << '\n'
              << "domain_capture="
              << (options.domain_output.empty() ? "disabled" : "enabled")
              << '\n'
              << "b2_input_rng_capture="
              << (options.b2_input_rng_output.empty() ? "disabled" : "enabled")
              << '\n'
              << "evidence_class=SOURCE_MODEL_DIAGNOSTIC_ONLY\n";
    return 0;
}

}  // namespace

int wmain(int count, wchar_t** arguments) {
    SetErrorMode(SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX |
                 SEM_NOOPENFILEERRORBOX);
    try {
        return run(count, arguments);
    } catch (const std::exception& exception) {
        std::cerr << "unhandled_std_exception=" << exception.what() << '\n';
        return 91;
    } catch (...) {
        std::cerr << "unhandled_cpp_exception\n";
        return 92;
    }
}
