#include "ntsd28/battle_world.h"
#include "ntsd28/dat_parser.h"
#include "ntsd28_playable/game_session.h"

#include <array>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <iterator>
#include <memory>
#include <stdexcept>
#include <string>

namespace {

std::shared_ptr<const ntsd28::DatDocument> load_weapon(
    const std::filesystem::path& runtime_root) {
    const auto path = runtime_root / "decoded_dat" / "w" / "6.dat";
    std::ifstream input(path, std::ios::binary);
    if (!input) throw std::runtime_error("cannot read formal OID600 DAT");
    const std::string bytes(std::istreambuf_iterator<char>{input}, {});
    auto definition = std::make_shared<const ntsd28::DatDocument>(
        ntsd28::DatParser{}.parse_text(bytes));
    if (!definition->ok() || definition->frame(0) == nullptr ||
        definition->frame(40) == nullptr ||
        definition->frame(0)->values.integer("state") != 1000 ||
        definition->frame(40)->values.integer("state") != 1002) {
        throw std::runtime_error("formal OID600 frame0/40 gate failed");
    }
    return definition;
}

void run_case(const std::filesystem::path& runtime_root,
              const std::shared_ptr<const ntsd28::DatDocument>& definition,
              double initial_vx, std::ofstream& rows) {
    ntsd28_playable::BattleConfig28 config;
    config.random_seed = 2833;
    config.character_id = 2;
    config.enemy_id = 7;
    config.background_id = 23;
    config.bgm_selection_49f18c = 2;
    config.battle_mode = 0;

    ntsd28_playable::CombatantConfig28 naruto;
    naruto.slot = 0;
    naruto.object_id = 2;
    naruto.x = 100;
    naruto.z = 650;
    naruto.team = 1;
    ntsd28_playable::CombatantConfig28 lee;
    lee.slot = 1;
    lee.object_id = 7;
    lee.x = 1200;
    lee.z = 650;
    lee.team = 2;
    config.combatants = {naruto, lee};

    ntsd28_playable::GameSession28 session(runtime_root, runtime_root);
    std::string error;
    if (!session.initialize(config, error)) {
        throw std::runtime_error("formal session initialize: " + error);
    }
    auto* world = session.world();
    if (world == nullptr || !world->slot_available_for_spawn(50)) {
        throw std::runtime_error("formal OID600 slot50 is unavailable");
    }
    ntsd28::SpawnRequest28 request;
    request.object_id = 600;
    request.object_type = 4;
    request.definition = definition;
    request.initial_action = 0;
    request.position.x = 500;
    request.position.y = -20;
    request.position.z = 650;
    request.hp = 250;
    request.mp = 0;
    request.owner_slot = 0;
    request.battle_group = 1;
    const auto spawned = world->spawn_at(50, request);
    if (!spawned.success) {
        throw std::runtime_error("formal OID600 spawn: " + spawned.message);
    }
    auto* weapon = world->entity(50);
    if (weapon == nullptr || weapon->frame.action != 0) {
        throw std::runtime_error("formal OID600 frame0 precondition failed");
    }
    weapon->position.y = -20;
    weapon->position.precise_y = -20.0;
    weapon->motion.x = initial_vx;
    weapon->motion.y = 0.0;
    weapon->motion.z = 0.0;

    session.set_input(0, {});
    session.set_input(1, {});
    session.step();
    const auto* tick = session.last_tick();
    weapon = world->entity(50);
    if (tick == nullptr || weapon == nullptr || tick->sequence != 1) {
        throw std::runtime_error("formal OID600 did not survive one host tick");
    }
    const ntsd28::WorldFrameEvent28* frame_event = nullptr;
    for (const auto& event : tick->frames) {
        if (event.slot == 50) {
            frame_event = &event;
            break;
        }
    }
    if (frame_event == nullptr) {
        throw std::runtime_error("formal OID600 frame pass was not reported");
    }
    rows << initial_vx << ',' << tick->sequence << ','
         << frame_event->frame.from_action << ','
         << frame_event->frame.to_action << ','
         << weapon->frame.action << ',' << weapon->position.x << ','
         << weapon->position.y << ',' << weapon->position.precise_x << ','
         << weapon->position.precise_y << ',' << weapon->motion.x << ','
         << weapon->motion.y << '\n';
}

}  // namespace

int main(int argc, char** argv) {
    if (argc != 3) {
        std::cerr << "usage: fast_weapon_oid600_full_tick_source_probe "
                     "<formal_runtime_root> <new_output_csv>\n";
        return 2;
    }
    const std::filesystem::path runtime_root(argv[1]);
    const std::filesystem::path output(argv[2]);
    if (std::filesystem::exists(output)) {
        std::cerr << "refusing to overwrite a previous trace\n";
        return 3;
    }
    try {
        const auto definition = load_weapon(runtime_root);
        std::filesystem::create_directories(output.parent_path());
        std::ofstream rows(output, std::ios::binary);
        if (!rows) throw std::runtime_error("cannot create source trace");
        rows << std::setprecision(17);
        rows << "initial_vx,tick,frame_from,frame_to,final_action,x,y,precise_x,precise_y,vx,vy\n";
        for (const double vx : std::array<double, 4>{{-20.0, 20.0, -9.0, 9.0}}) {
            run_case(runtime_root, definition, vx, rows);
        }
        rows.close();
        if (!rows) throw std::runtime_error("source trace write failed");
        std::cout << "OID600 formal playable source complete ticks: 4\n";
        return 0;
    } catch (const std::exception& exception) {
        std::cerr << exception.what() << '\n';
        return 4;
    }
}
