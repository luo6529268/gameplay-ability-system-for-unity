#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

int wmain() {
    try {
        std::cout << std::setprecision(std::numeric_limits<double>::max_digits10);
        for (int phase : {0, 1})
        for (int state : {1000, 1002, 2000})
        for (int facing : {0, 1})
        for (int vx : {-8, 0, 8})
        for (double vy : {5.0, 9.0, 9.000000001, 10.0})
        for (int y : {-1, 0, -10})
        for (int reference : {0, -5, 3}) {
            std::ostringstream dat;
            dat << "<bmp_begin>\nname: Heavy weapon_hp: 32 weapon_drop_hurt: 4\n<bmp_end>\n"
                << "<frame> 0 initial\nstate: " << state << " wait: 100 next: 0 centerx: 39 centery: 79\n<frame_end>\n"
                << "<frame> 20 settled\nstate: 2004 wait: 100 next: 20 centerx: 39 centery: 79\n<frame_end>\n";
            auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat.str()));
            if (!definition->ok()) throw std::runtime_error("type2 DAT invalid");
            ntsd28::ObjectDefinitionCatalog28 catalog;
            catalog.upsert_definition(742, 2, "heavy.dat", definition);
            ntsd28::BattleWorld28 world;
            world.random().reset_from_seed(42);
            ntsd28::SpawnRequest28 request;
            request.object_id = 742; request.object_type = 2; request.definition = definition;
            request.initial_action = 0; request.hp = 500; request.mp = 500;
            request.position.y = y;
            if (!world.spawn_at(70, request).success) throw std::runtime_error("type2 spawn failed");
            auto* entity = world.entity(70);
            entity->frame.facing = facing != 0;
            entity->weapon_hp_31c = 20;
            entity->collision_y_reference = reference;
            entity->motion.x = vx; entity->motion.y = vy;
            std::vector<std::string> messages;
            if (phase == 0) {
                const auto result = world.step_physics(70, {});
                if (!result.success) throw std::runtime_error(result.message);
            } else {
                const auto result = ntsd28::SimulationTickDriver28{}.step(world, catalog);
                for (const auto& frame : result.frames) if (!frame.frame.ok()) throw std::runtime_error(frame.frame.message);
                if (!result.lifecycle.success) throw std::runtime_error("type2 lifecycle failed");
                messages = result.diagnostics.messages;
            }
            std::cout << "{\"phase\":" << phase << ",\"state\":" << state << ",\"facing\":" << facing
                      << ",\"vx\":" << vx << ",\"vy\":" << vy << ",\"y\":" << y << ",\"reference\":" << reference
                      << ",\"entity\":";
            if (const auto* current = world.entity(70)) write_entity(std::cout, *current, 1);
            else std::cout << "null";
            std::cout << ",\"syncCalls\":" << world.random().state().synchronized.calls << ",\"messages\":[";
            for (std::size_t i = 0; i < messages.size(); ++i) {
                if (i != 0) std::cout << ',';
                std::cout << '\"' << json_escape(messages[i]) << '\"';
            }
            std::cout << "]}\n";
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n'; return 91;
    }
}
