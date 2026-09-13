#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain

// Source field/output binding only; this is not a gameplay or formal EXE trace.
int wmain(int, wchar_t**) {
    for (int value : {-1, 0, 37}) {
        ntsd28::EntityState28 entity;
        entity.slot = 0;
        entity.owner_slot = 19;
        entity.object_ai_excluded_group_source_slot_2f8 = value;
        write_entity(std::cout, entity, 1);
        std::cout << '\n';
    }
    return 0;
}
