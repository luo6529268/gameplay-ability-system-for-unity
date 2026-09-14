#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

namespace {
void emit_birth(ntsd28::ObjectDefinitionCatalog28& catalog, int oid, int action, int hp, int mp) {
    const auto* definition = catalog.find(oid);
    if (definition == nullptr) throw std::runtime_error("missing child definition");
    std::ostringstream text;
    text << "<bmp_begin>\nname: SpawnVitalsParent\n<bmp_end>\n<frame> 0 idle\n"
         << "pic: 0 state: 0 wait: 100 next: 0\nopoint: kind: 1 oid: " << oid
         << " action: " << action << " hp: " << hp << " mp: " << mp
         << " opoint_end:\n<frame_end>\n";
    ntsd28::SpawnRequest28 parent;
    parent.object_id = 31991; parent.object_type = 0; parent.hp = 500; parent.mp = 500;
    parent.definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
    ntsd28::BattleWorld28 world;
    if (!world.spawn_at(0, parent).success) throw std::runtime_error("parent spawn failed");
    const auto result = world.materialize_supported_spawns(0, catalog);
    if (!result.success || result.spawned != 1) throw std::runtime_error("native OPoint materialization failed for oid=" + std::to_string(oid));
    const auto* e = world.entity(50);
    if (e == nullptr) throw std::runtime_error("native child not in first transient slot");
    const auto& stats = definition->definition->stats;
    std::cout << oid << '\t' << definition->object_type << '\t' << action << '\t' << hp << '\t' << mp << '\t'
              << stats.integer("ohp").value_or(0) << '\t' << stats.integer("omp").value_or(0) << '\t'
              << stats.integer("max_mp").has_value() << '\t' << stats.integer("max_mp").value_or(0) << '\t'
              << e->current_hp << '\t' << e->effective_max_hp << '\t' << e->base_max_hp << '\t'
              << e->current_mp << '\t' << e->base_max_mp << '\t' << e->display_current_hp_200 << '\t'
              << e->display_effective_max_hp_208 << '\t' << e->display_score_1f0 << '\t'
              << e->display_damage_total_1f8 << '\t' << e->display_score_step_1f4 << '\t'
              << e->display_damage_step_1fc << '\t' << e->display_current_hp_step_204 << '\t'
              << e->display_effective_max_hp_step_20c << '\n';
}

void fixture(int oid, int type, int hp, int mp, int ohp, int omp, int max_mp = -999) {
    std::ostringstream text;
    text << "<bmp_begin>\nname: SpawnVitalsChild\nweapon_hp: 17\n<bmp_end>\n"
         << "<stats> ohp: " << ohp << " omp: " << omp;
    if (max_mp != -999) text << " max_mp: " << max_mp;
    text << " <stats_end>\n<frame> 0 idle\npic: 0 state: 0 wait: 100 next: 0\n<frame_end>\n";
    ntsd28::ObjectDefinitionCatalog28 catalog;
    catalog.upsert_definition(oid, type, "spawn-vitals-fixture.dat",
        std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str())));
    emit_birth(catalog, oid, 0, hp, mp);
}
}

int wmain(int argc, wchar_t** argv) {
    std::cout << "oid\ttype\taction\tpointHp\tpointMp\tohp\tomp\tmaxPresent\tmaxMp\thpOut\tboundOut\tbaseOut\tmpOut\tbaseMpOut\tdisplayHp\tdisplayMax\tscore\tdamage\tscoreStep\tdamageStep\thpStep\tmaxStep\n";
    if (argc == 2) {
        ntsd28::ObjectDefinitionCatalog28 catalog;
        const auto loaded = catalog.load_extracted_root(std::filesystem::path(argv[1]));
        if (!loaded.success || catalog.size() != 330) throw std::runtime_error("formal catalog load failed");
        std::vector<int> ids;
        for (const auto& entry : catalog.entries()) ids.push_back(entry.first);
        std::sort(ids.begin(), ids.end());
        for (int oid : ids) {
            if (oid <= 0) {
                std::cerr << "Skipped oid=" << oid << ": native ObjectSpawnPlanner28 ignores non-positive opoint oid\n";
                continue;
            }
            const auto* entry = catalog.find(oid);
            if (entry->definition->frames.empty()) throw std::runtime_error("formal definition has no declared frame");
            emit_birth(catalog, oid, entry->definition->frames.front().id, 101, 103);
        }
        return 0;
    }
    if (argc != 1) return 2;
    for (int oid : {5, 52, 51, 77}) for (int hp : {-7, 0, 1, 101, 501})
    for (int mp : {-7, 0, 1, 103, 501}) for (int ohp : {-1, 0, 1, 50, 100, 150})
    for (int omp : {-1, 0, 1, 50, 100, 150}) fixture(oid, 0, hp, mp, ohp, omp);
    for (int oid : {5, 52, 51, 77}) for (int type : {0, 1, 2, 3, 4, 5, 6})
    for (int max : {-999, -1, 0, 700}) fixture(oid, type, 101, 103, 50, 150, max);
    for (int percent : {1, 50, 100, 200}) fixture(77, 3, 1000000000, 999999999, percent, percent);
    return 0;
}
