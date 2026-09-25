# Q07 `resource.dat` `<frame>` extra-image catalog scope (2026-09-25)

Read-only result. The formal `resources/runtime/decoded_dat/data/resource.dat` has 48 `pic:` paths inside `<bmp_begin>…<bmp_end>`, followed by seven more `pic:` paths inside a separate `<frame>…<frame_end>` block. All seven latter paths exist in the formal VFS and are absent from Unity's staged VFS:

`sprite/UI/extra/recording_background.png`, `human.png`, `BG1o1.png`, `BG2o2.png`, `branch.png`, `branch2.png`, and `player.png`.

`NativeResourceCatalog28::parse_text` in `source/ntsd28_core/src/rendering/native_resource_catalog.cpp:39-82` starts collection only after `<bmp_begin>` and stops at `<bmp_end>`, with a native capacity of 48. Its `entries()` therefore does **not** include the seven `<frame>` paths. In the inspected playable C++ source, `game_session.cpp:1216-1223` passes `resource.dat` to that catalog; a source search found no direct literal consumer for the seven extra filenames or `sprite/UI/extra`. This establishes that a `resource.dat` text reference alone does not make them indexed battle images.

Disposition: `NO_SELECTED_PLAYABLE_CONSUMER_PROVED`, not `CONFIRMED_UNUSED_BY_FORMAL_EXE`. Do not copy these seven to chase whole-VFS count parity or infer a battle pixel defect. This is a source/static finding, not formal GUI runtime or Unity pixel evidence; an indirect/dynamic consumer remains unknown. The 114 absent `sprite/*` paths now have ten directly narrowed owners/consumer findings across this note and `LOADING-PAUSE-CONSUMER-AUDIT-20260925.md` (two pre-battle Loading, one pause path with no proved selected image consumer, seven `<frame>` extras outside the 48-entry catalog). The remaining 104 paths retain their prior classifications or unresolved owner status. All prior VFS totals and byte hashes are unchanged. Q07/Q09/R17 remain open.

The formal root EXE identity was freshly checked in the preceding Q07 loading/pause audit as SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. No DAT, PNG, Unity script, Scene, or ProjectSettings was changed.
