#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

int wmain() {
    try {
        for (int index = 0; index < 6; ++index) {
            const int previous = index == 1 || index == 3 ? 200 : 10;
            const int previous_state = index == 0 || index == 2 ? 13 : index == 4 ? 18 : 0;
            const int current = index == 2 || index == 3 ? previous : 0;
            const std::string dat = "<bmp_begin>\nname: ExitTail\n<bmp_end>\n<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n<frame> "
                + std::to_string(previous) + " prior\nstate: " + std::to_string(previous_state)
                + " wait: 100 next: 0\n<frame_end>\n";
            std::string particle_dat = "<bmp_begin>\nname: Particle\n<bmp_end>\n";
            for (int action : {0, 120, 125, 130, 135, 140})
                particle_dat += "<frame> " + std::to_string(action) + " particle\nstate: 3003 wait: 100 next: 0\n<frame_end>\n";
            auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat));
            auto particle = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(particle_dat));
            if (!definition->ok() || !particle->ok()) throw std::runtime_error("DAT rejected");
            ntsd28::BattleWorld28 world;
            world.random().reset_from_seed(42);
            ntsd28::ObjectDefinitionCatalog28 catalog;
            catalog.upsert_definition(7201, 0, "exit.dat", definition);
            catalog.upsert_definition(999, 3, "particle.dat", particle);
            ntsd28::SpawnRequest28 request;
            request.object_id = 7201; request.object_type = 0; request.definition = definition;
            request.initial_action = current; request.hp = 500; request.mp = 500;
            request.position.x = 300; request.position.y = -10; request.position.z = 250;
            if (!world.spawn_at(21, request).success) throw std::runtime_error("spawn failed");
            auto& entity = *world.entity(21);
            entity.frame.previous_action_078 = previous;
            entity.frame.action_latch = current;
            std::cout << "{\"index\":" << index << ",\"previous\":" << previous
                << ",\"previousState\":" << previous_state << ",\"current\":" << current
                << ",\"dat\":\"" << json_escape(dat) << "\",\"particleDat\":\"" << json_escape(particle_dat)
                << "\",\"beforeRandom\":";
            write_b2_initial_random(std::cout, world.random().state(), world.random().synchronized_table_hash());
            ntsd28::SimulationTickOptions28 options;
            options.stage_bounds = ntsd28::StageBounds28{800,180,350};
            options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
            const auto result = ntsd28::SimulationTickDriver28{}.step(world, catalog, options);
            int count = 0;
            std::cout << ",\"entities\":[";
            for (std::size_t slot = 0; slot < ntsd28::EngineProfile28::maximum_slots; ++slot) {
                const auto* e = world.entity(slot);
                if (!e) continue;
                if (count++) std::cout << ',';
                write_entity(std::cout, *e, 1);
            }
            std::cout << "],\"count\":" << count << ",\"audioCount\":" << result.audio_events.size()
                << ",\"lifecycleSuccess\":" << (result.lifecycle.success ? "true" : "false")
                << ",\"afterRandom\":";
            write_b2_initial_random(std::cout, world.random().state(), world.random().synchronized_table_hash());
            std::cout << "}\n";
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
