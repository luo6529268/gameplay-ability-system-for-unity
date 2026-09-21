#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

int wmain() {
    try {
        for (int index = 0; index < 4; ++index) {
            const bool flip_control = index == 3;
            const std::string dat = std::string("<bmp_begin>\nname: Facing\n<bmp_end>\n<frame> 0 held\nstate: 2000 wait: ")
                + (flip_control ? "0 next: -1" : "100 next: 0")
                + "\n<frame_end>\n<frame> 1 target\nstate: 2000 wait: 100 next: 0\n<frame_end>\n";
            auto definition = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat));
            if (!definition->ok()) throw std::runtime_error("facing DAT parse failed");
            ntsd28::BattleWorld28 world;
            ntsd28::SpawnRequest28 request;
            request.object_id = 7201 + index; request.object_type = 0;
            request.definition = definition; request.hp = 500; request.mp = 500;
            if (!world.spawn_at(21, request).success) throw std::runtime_error("spawn failed");
            auto& entity = *world.entity(21);
            entity.motion.x = index == 0 || flip_control ? 3 : index == 1 ? 0 : -3;
            entity.frame.facing = index == 0 || flip_control;
            entity.frame.action = 0; entity.frame.action_latch = 0; entity.frame.frame_counter = 0;
            std::cout << "{\"index\":" << index << ",\"vx\":" << entity.motion.x
                << ",\"beforeFacing\":" << entity.frame.facing;
            const auto events = world.step_frame_slot(21);
            std::cout << ",\"events\":" << events.size() << ",\"afterFacing\":" << entity.frame.facing
                << ",\"action\":" << entity.frame.action << ",\"counter\":" << entity.frame.frame_counter << "}\n";
        }
        return 0;
    } catch (const std::exception& error) {
        std::cerr << error.what() << '\n';
        return 1;
    }
}
