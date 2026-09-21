#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

int wmain() {
    try {
        for (int index = 0; index < 3; ++index) {
            const int attack_slot = index == 0 ? 19 : 22;
            const bool snap = index != 2;
            const std::string platform = std::string("<bmp_begin>\nname: Platform\n<bmp_end>\n<frame> 0 platform\nstate: 3003 wait: 100 next: 0 attacking: ")
                + (snap ? "1" : "0") + "\nitr:\nkind: 30 x: -10 y: 0 w: 20 h: 10 zwidth: 15\nitr_end:\n<frame_end>\n";
            const std::string rider = "<bmp_begin>\nname: Rider\n<bmp_end>\n<frame> 0 rider\nstate: 0 wait: 100 next: 0\nbdy:\nkind: 0 x: -4 y: 0 w: 8 h: 4\nbdy_end:\n<frame_end>\n";
            const std::string attack = "<bmp_begin>\nname: Attack\n<bmp_end>\n<frame> 0 attack\nstate: 0 wait: 100 next: 0\nitr:\nkind: 0 x: -24 y: -20 w: 8 h: 4 injury: 1 vrest: 1\nitr_end:\n<frame_end>\n";
            ntsd28::BattleWorld28 world;
            auto spawn = [&](int slot, int oid, int type, int y, int group, const std::string& dat) {
                auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat));
                if (!definition->ok()) throw std::runtime_error("mixed DAT invalid");
                ntsd28::SpawnRequest28 r;
                r.object_id = oid; r.object_type = type; r.definition = definition;
                r.position.x = oid == 31982 ? 120 : 100; r.position.y = y; r.position.z = 250;
                r.hp = 500; r.mp = 500; r.battle_group = group;
                if (!world.spawn_at(slot, r).success) throw std::runtime_error("mixed spawn failed");
            };
            spawn(20, 31980, 3, -20, 1, platform);
            spawn(21, 31981, 0, -10, 1, rider);
            spawn(attack_slot, 31982, 0, 0, 2, attack);
            world.entity(20)->position.previous_y = 0;
            world.entity(21)->position.previous_y = -10;
            world.snapshot_actions();
            const auto result = world.rebuild_geometric_hit_candidates();
            const auto& target = *world.entity(21);
            std::cout << "{\"index\":" << index << ",\"attackSlot\":" << attack_slot
                << ",\"snap\":" << (snap ? "true" : "false")
                << ",\"platformDat\":\"" << json_escape(platform) << "\",\"riderDat\":\"" << json_escape(rider)
                << "\",\"attackDat\":\"" << json_escape(attack) << "\",\"success\":" << (result.success ? "true" : "false")
                << ",\"candidateCount\":" << world.entity(attack_slot)->hit_candidates.size()
                << ",\"targetY\":" << target.position.y << ",\"targetPreciseY\":" << target.position.precise_y
                << ",\"reference\":" << target.collision_y_reference << ",\"platformSlot\":" << target.platform_source_slot_f4 << "}\n";
        }
        return 0;
    } catch (const std::exception& e) { std::cerr << e.what() << '\n'; return 1; }
}
