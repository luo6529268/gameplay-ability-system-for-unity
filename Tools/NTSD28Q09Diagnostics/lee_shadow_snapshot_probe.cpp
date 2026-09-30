#include "ntsd28_playable/game_session.h"

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>
#include <vector>

int wmain(int argc, wchar_t** argv) {
    if (argc != 4) {
        std::cerr << "usage: lee_shadow_snapshot_probe <decoded_dat> <vfs> <output.tsv>\n";
        return 2;
    }
    const std::filesystem::path output(argv[3]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to overwrite diagnostic output\n";
        return 3;
    }

    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 682973786u;
    config.character_id = 7;
    config.enemy_id = 2;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;
    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 0;
    lee.object_id = 7;
    lee.x = 500;
    lee.z = 650;
    lee.hp = 500;
    lee.mp = 500;
    lee.team = 1;
    lee.action = 0;
    ntsd28_playable::CombatantConfig28 opponent;
    opponent.slot = 1;
    opponent.object_id = 2;
    opponent.x = 1200;
    opponent.z = 650;
    opponent.hp = 500;
    opponent.mp = 500;
    opponent.team = 2;
    opponent.action = 0;
    config.combatants = {lee, opponent};

    ntsd28_playable::GameSession28 session(argv[1], argv[2]);
    std::string error;
    if (!session.initialize(config, error)) {
        std::cerr << "session initialize failed: " << error << '\n';
        return 4;
    }
    std::ofstream rows(output, std::ios::binary);
    if (!rows) return 5;
    rows << "tick\tslot\toid\towner\taction\tpic\tchildCount\tchildBodyCommands"
            "\tchildShadowCommands\totherShadowCommands\tsnapshotSprites\tsnapshotShadows\n";

    bool selected_tick_passed = false;
    for (int tick = 1; tick <= 13; ++tick) {
        ntsd28::InputButtons28 buttons;
        if (tick == 2) buttons.set(ntsd28::InputKey28::attack);
        if (tick == 3 || tick == 4)
            buttons.set(ntsd28::InputKey28::defend);
        session.set_input(0, buttons);
        session.set_input(1, {});
        session.step();
        if (tick < 5) continue;

        const auto* world = session.world();
        if (world == nullptr) return 6;
        const auto snapshot = session.snapshot(false);
        std::vector<std::size_t> children;
        for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots;
             ++slot) {
            const auto* entity = world->entity(slot);
            if (entity != nullptr && entity->object_id == 204 &&
                entity->owner_slot == 0) {
                children.push_back(slot);
            }
        }

        int total_child_bodies = 0;
        int total_child_shadows = 0;
        int other_shadows = 0;
        for (const auto& command : snapshot.entity_commands) {
            bool is_child = false;
            for (const auto slot : children) {
                if (command.slot == slot) {
                    is_child = true;
                    break;
                }
            }
            if (is_child &&
                command.kind == ntsd28::RenderEntityCommandKind28::sprite)
                ++total_child_bodies;
            if (is_child &&
                command.kind == ntsd28::RenderEntityCommandKind28::shadow)
                ++total_child_shadows;
            if (!is_child &&
                command.kind == ntsd28::RenderEntityCommandKind28::shadow)
                ++other_shadows;
        }

        if (children.empty()) {
            rows << tick << "\t-1\t-1\t-1\t-1\t-1\t0\t0\t0\t"
                 << other_shadows << '\t' << snapshot.sprites.size() << '\t'
                 << snapshot.shadows.size() << '\n';
        }
        for (const auto slot : children) {
            const auto* entity = world->entity(slot);
            int sprite_pic = -1;
            for (const auto& sprite : snapshot.sprites) {
                if (sprite.slot == slot) {
                    sprite_pic = sprite.pic;
                    break;
                }
            }
            rows << tick << '\t' << slot << '\t' << entity->object_id
                 << '\t' << entity->owner_slot << '\t' << entity->frame.action
                 << '\t' << sprite_pic << '\t' << children.size() << '\t'
                 << total_child_bodies << '\t' << total_child_shadows << '\t'
                 << other_shadows << '\t' << snapshot.sprites.size() << '\t'
                 << snapshot.shadows.size() << '\n';
        }
        if (tick == 6) {
            selected_tick_passed = children.size() == 5 &&
                total_child_bodies == 5 && total_child_shadows == 0 &&
                other_shadows > 0;
        }
    }
    rows.close();
    if (!rows) return 7;
    if (!selected_tick_passed) {
        std::cerr << "tick6 shadow/body/control invariant failed; raw TSV retained\n";
        return 8;
    }
    std::cout << "tick6: five visible OID204 bodies, zero child shadows, "
                 "ordinary shadow control present\n";
    return 0;
}
