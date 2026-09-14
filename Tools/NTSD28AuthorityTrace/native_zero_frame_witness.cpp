#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"

int wmain(int, wchar_t**) {
    std::cout << "declaredId\tlookup\tdocumentOk\tdeclaredFound\truntimeFound\tframeId\twait\tnext\tstate\tpic\tchp\tcmp\tsubblocks\thpOut\tboundOut\tmpOut\n";
    for (int declared : {-999, -1, 0, 7, 856, 857, 998, 999, 1000}) {
        std::ostringstream text;
        text << "<bmp_begin>\nname: NativeZeroFrame\n<bmp_end>\n";
        if (declared != -999) {
            text << "<frame> " << declared << " declared\npic: 29 state: 14 wait: 2 next: 0 chp: 7 cmp: 5\n<frame_end>\n";
        }
        auto document = std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(text.str()));
        for (int id : {-1, 0, 7, 856, 857, 998, 999, 1000, 9999}) {
            const auto* frame = document->frame(id);
            int hp = -999, bound = -999, mp = -999;
            if (document->ok()) {
                ntsd28::BattleWorld28 world;
                ntsd28::SpawnRequest28 request;
                request.object_id = 77; request.object_type = 0; request.definition = document;
                request.initial_action = 0; request.hp = 500; request.mp = 100;
                if (!world.spawn_at(0, request).success) throw std::runtime_error("zero-frame witness spawn failed");
                auto* e = world.entity(0);
                e->frame.action = id;
                e->current_hp = 100; e->effective_max_hp = 200;
                ntsd28::ResourceSystemRules28 rules;
                rules.selected_mode_default_hp_regen_gate_28 = 0;
                rules.selected_mode_mp_regen_gate_2c = 0;
                const auto resource = world.advance_native_resources_pre_display_slot(0, rules);
                (void)resource;
                hp = e->current_hp; bound = e->effective_max_hp; mp = e->current_mp;
            }
            const auto value = [&](const char* key) { return frame == nullptr ? -999 : frame->values.integer(key).value_or(0); };
            std::cout << declared << '\t' << id << '\t' << document->ok() << '\t'
                      << (document->declared_frame(id) != nullptr) << '\t' << (frame != nullptr) << '\t'
                      << (frame == nullptr ? -999 : frame->id) << '\t' << value("wait") << '\t'
                      << value("next") << '\t' << value("state") << '\t' << value("pic") << '\t'
                      << value("chp") << '\t' << value("cmp") << '\t'
                      << (frame == nullptr ? -999 : static_cast<int>(frame->subblocks.size())) << '\t'
                      << hp << '\t' << bound << '\t' << mp << '\n';
        }
    }
    return 0;
}
