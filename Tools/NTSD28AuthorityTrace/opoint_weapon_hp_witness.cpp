#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

static void emit_weapon_hp_birth(int type, int kind, int hp, int weapon_hp, bool present) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: WeaponHpChild\n";
    if (present) text << "weapon_hp: " << weapon_hp << '\n';
    text << "<bmp_end>\n<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
    auto child = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    std::ostringstream parent_text;
    parent_text << "<bmp_begin>\nname: WeaponHpParent\n<bmp_end>\n<frame> 0 idle\n"
        << "state: 0 wait: 100 next: 0\nopoint:\nkind: " << kind << " oid: 777 action: 0 hp: " << hp
        << "\nopoint_end:\n<frame_end>\n";
    auto parent = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(parent_text.str()));
    if (!child->ok() || !parent->ok()) throw std::runtime_error("weapon HP DAT rejected");
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(777, type, "weapon-hp-child.dat", child);
    ntsd28::BattleWorld28 world;
    world.random().reset_from_seed(42);
    ntsd28::SpawnRequest28 request;
    request.object_id = 888; request.object_type = 0; request.definition = parent;
    request.hp = 500; request.mp = 500;
    if (!world.spawn_at(20, request).success) throw std::runtime_error("weapon HP parent spawn failed");
    direct_crt_calls.clear(); direct_synchronized_calls.clear();
    const auto result = world.materialize_supported_spawns(20, catalog);
    const auto* entity = world.entity(50);
    if (!result.success || result.spawned != 1 || !entity) throw std::runtime_error("weapon HP materialization failed");
    std::cout << "{\"type\":" << type << ",\"kind\":" << kind << ",\"pointHp\":" << hp
        << ",\"weaponHpPresent\":" << (present ? "true" : "false") << ",\"weaponHp\":" << weapon_hp
        << ",\"parentLink\":" << world.entity(20)->interaction_state
        << ",\"childLink\":" << entity->interaction_state
        << ",\"nativeCalls\":" << direct_synchronized_calls.size()
        << ",\"crtCalls\":" << direct_crt_calls.size() << ",\"raw\":";
    write_entity(std::cout, *entity, 1);
    std::cout << "}\n";
}

int wmain(int, wchar_t**) {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        for (int type : {0, 1, 2, 3, 4, 5, 6})
        for (int kind : {1, 2})
        for (int hp : {-5, 0, 41}) {
            emit_weapon_hp_birth(type, kind, hp, 0, false);
            for (int weapon_hp : {-9, 0, 17, 999}) emit_weapon_hp_birth(type, kind, hp, weapon_hp, true);
        }
        return 0;
    } catch (const std::exception& error) { std::cerr << error.what() << '\n'; return 91; }
}
