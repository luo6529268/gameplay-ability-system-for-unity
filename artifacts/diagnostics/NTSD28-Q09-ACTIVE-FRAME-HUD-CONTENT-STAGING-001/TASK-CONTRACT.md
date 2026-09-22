# NTSD28-Q09-ACTIVE-FRAME-HUD-CONTENT-STAGING-001

Status: `READY` before any asset copy (2026-09-22). Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001`, BATCH-05/Q09, R17.

Authority: formal `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; corresponding playable `GameSession28::initialize_resources` loads `data/frame.dat`, selects `NativeFrameHudCatalog28::captured_active_index == 0`, loads `data/frame/INKHUD.dat`, and passes `native_frame_hud` into the battle render snapshot. `render_snapshot.cpp` consumes its panel/bar/team-flag entries. Formal `resources/runtime` is the D-023 DAT and battle-image content authority. Current Unity `LoganRuntime` has neither of these two DATs nor their seven selected INKHUD PNGs. The selected radar section is `bound: 0`; INKHUD2 is not the active child. This package stages only the nine positively selected source files.

Exact copied files, rooted at `Assets/NTSD/Content/LoganRuntime` (with matching Unity `.meta` files and three missing folder `.meta` files for `decoded_dat/data/frame`, `vfs/sprite/frame`, and `vfs/sprite/frame/INKHUD`):

1. `decoded_dat/data/frame.dat`
2. `decoded_dat/data/frame/INKHUD.dat`
3. `vfs/sprite/frame/INKHUD/FRAME.png`
4. `vfs/sprite/frame/INKHUD/BARS.png`
5. `vfs/sprite/frame/INKHUD/team0.png`
6. `vfs/sprite/frame/INKHUD/team1.png`
7. `vfs/sprite/frame/INKHUD/team2.png`
8. `vfs/sprite/frame/INKHUD/team3.png`
9. `vfs/sprite/frame/INKHUD/team4.png`

All nine destination files were absent before copy. Verify each source and destination length/SHA-256 and confirm the native DAT-selected seven-path closure. This stage does not implement a Unity reader or claim HUD/EXE visual parity. Do not copy INKHUD2, radar, other UI/menu images, minibar styles or backgrounds by directory. Do not edit scripts, scenes, prefabs, GameConfig, old DAT/images, audio, menus or nonbattle functionality. Existing unsaved Scene state stays untouched; no Unity Editor restart or computer-use.

Risk: importer settings/asset identity and accidental category expansion. Generate stable unique `.meta` GUIDs for only the copied files and three newly created asset folders; use the existing LoganRuntime default importer templates without altering existing `.meta` files. After staging, inspect Git scope and record exact file hashes in `ACCEPTANCE.md`. Any rollback/deletion of staged files requires the repository's explicit approval rule; no reset/clean.
