#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

int wmain() {
    try {
        for (int type = 0; type <= 6; ++type)
        for (int state : {0, 9998})
        for (int enter : {0, 1})
        for (int hp : {0, 500})
        for (int y : {0, -20})
        for (int slot : {0, 70}) {
            std::ostringstream dat;
            dat << "<bmp_begin>\nname: StateWitness\n<bmp_end>\n"
                << "<frame> 0 initial\nstate: " << (enter ? 0 : state)
                << " wait: " << (enter ? 0 : 100) << " next: " << (enter ? 1 : 0)
                << "\n<frame_end>\n<frame> 1 destination\nstate: " << state
                << " wait: 100 next: 1\n<frame_end>\n";
            auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat.str()));
            if (!definition->ok()) throw std::runtime_error("state witness DAT rejected");
            ntsd28::ObjectDefinitionCatalog28 catalog;
            catalog.upsert_definition(777, type, "state-witness.dat", definition);
            ntsd28::BattleWorld28 world;
            world.random().reset_from_seed(42);
            ntsd28::SpawnRequest28 request;
            request.object_id = 777; request.object_type = type; request.definition = definition;
            request.initial_action = 0; request.hp = hp; request.mp = 500;
            request.position.x = 100; request.position.y = y; request.position.z = 100;
            if (!world.spawn_at(slot, request).success) throw std::runtime_error("source spawn failed");
            ntsd28::SimulationTickDriver28 driver;
            ntsd28::SimulationTickOptions28 options;
            for (int tick = 1; tick <= 3; ++tick) {
                const auto result = driver.step(world, catalog, options);
                for (const auto& frame : result.frames)
                    if (!frame.frame.ok()) throw std::runtime_error(frame.frame.message);
                if (!result.lifecycle.success) throw std::runtime_error("native lifecycle failed");
                std::cout << "{\"type\":" << type << ",\"state\":" << state << ",\"enter\":" << enter
                          << ",\"hp\":" << hp << ",\"y\":" << y << ",\"slot\":" << slot
                          << ",\"tick\":" << tick << ",\"frameEvents\":" << result.frames.size()
                          << ",\"diagnosticMessages\":" << result.diagnostics.messages.size() << ",\"messages\":[";
                for (std::size_t i = 0; i < result.diagnostics.messages.size(); ++i) {
                    if (i != 0) std::cout << ',';
                    std::cout << '\"' << json_escape(result.diagnostics.messages[i]) << '\"';
                }
                std::cout << "],\"entity\":";
                if (const auto* entity = world.entity(slot)) write_entity(std::cout, *entity, 1);
                else std::cout << "null";
                std::cout << "}\n";
            }
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n'; return 91;
    }
}
