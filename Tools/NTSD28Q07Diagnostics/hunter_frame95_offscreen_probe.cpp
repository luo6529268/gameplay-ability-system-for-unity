#include "ntsd28_playable/d3d11_renderer.h"
#include "ntsd28_playable/game_session.h"

#include <objbase.h>

#include <filesystem>
#include <fstream>
#include <iostream>
#include <string>

int wmain(int argc, wchar_t** argv) {
    if (argc != 3) {
        std::cerr << "usage: hunter_frame95_offscreen_probe <runtime_root> <output_dir>\n";
        return 2;
    }
    const std::filesystem::path root(argv[1]);
    const std::filesystem::path output(argv[2]);
    const auto action95_png = output / "oid32_action95_tick0.png";
    const auto action0_png = output / "oid32_action0_tick0.png";
    const auto geometry_csv = output / "oid32_offscreen_sprite_geometry.csv";
    if (std::filesystem::exists(action95_png) ||
        std::filesystem::exists(action0_png) ||
        std::filesystem::exists(geometry_csv)) {
        std::cerr << "refusing to overwrite prior diagnostic output\n";
        return 3;
    }
    const HRESULT com = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    if (FAILED(com)) {
        std::cerr << "COM initialization failed\n";
        return 4;
    }

    std::ofstream geometry(geometry_csv, std::ios::binary);
    if (!geometry) {
        CoUninitialize();
        return 5;
    }
    geometry << "action,slot,oid,pic,screen_left,screen_top,width,height,source_x,source_y,source_path\n";
    const auto render_action = [&](int action,
                                   const std::filesystem::path& png) {
        ntsd28_playable::BattleConfig28 config;
        config.random_seed = 0x28A55A5Au;
        config.character_id = 32;
        config.enemy_id = 7;
        config.background_id = 23;
        ntsd28_playable::CombatantConfig28 actor;
        actor.slot = 0;
        actor.object_id = 32;
        actor.x = 500;
        actor.z = 350;
        actor.hp = 500;
        actor.mp = 500;
        actor.team = 1;
        actor.action = action;
        ntsd28_playable::CombatantConfig28 opponent;
        opponent.slot = 1;
        opponent.object_id = 7;
        opponent.x = 1200;
        opponent.z = 350;
        opponent.hp = 500;
        opponent.mp = 500;
        opponent.team = 2;
        config.combatants = {actor, opponent};

        ntsd28_playable::GameSession28 session(root, root);
        std::string error;
        if (!session.initialize(config, error)) {
            std::cerr << "session init action " << action << ": " << error << '\n';
            return false;
        }
        const auto snapshot = session.snapshot(false);
        int actor_sprite_count = 0;
        for (const auto& sprite : snapshot.sprites) {
            if (sprite.slot != 0) continue;
            ++actor_sprite_count;
            geometry << action << ',' << sprite.slot << ',' << sprite.object_id
                     << ',' << sprite.pic << ',' << sprite.screen_left << ','
                     << sprite.screen_top << ',' << sprite.frame.width << ','
                     << sprite.frame.height << ',' << sprite.frame.source_x << ','
                     << sprite.frame.source_y << ','
                     << sprite.frame.source_path.u8string() << '\n';
        }
        if (actor_sprite_count != 1 || !geometry) {
            std::cerr << "unexpected actor sprite count for action " << action
                      << ": " << actor_sprite_count << '\n';
            return false;
        }
        ntsd28_playable::D3D11Renderer28 renderer;
        if (!renderer.initialize_offscreen(1333, 730, error) ||
            !renderer.render(snapshot, false, error) ||
            !renderer.save_offscreen_png(png, error)) {
            std::cerr << "offscreen action " << action << ": " << error << '\n';
            return false;
        }
        std::cout << "action=" << action << " actor_sprites="
                  << actor_sprite_count << " total_sprites="
                  << snapshot.sprites.size() << " quads="
                  << renderer.last_quad_count() << " output="
                  << png.u8string() << '\n';
        return true;
    };

    const bool success = render_action(95, action95_png) &&
                         render_action(0, action0_png);
    geometry.close();
    CoUninitialize();
    return success && geometry ? 0 : 6;
}
