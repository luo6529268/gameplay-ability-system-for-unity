# NTSD28-Q09-NATIVE-KNOCKOUT-FEED-ICONS-STAGING-001

Status: `READY` before asset copy (2026-09-22). Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001`, BATCH-05/Q09, R17.

Authority: formal `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; corresponding playable `GameSession28::initialize_resources` selects the first `mode.dat` child and loads `NativeKnockoutFeedConfig28`, then passes it to the render snapshot and D3D11 renderer. The formally staged `data/mode/ntsd.dat` has `<bmp_begin> #killtext`, `bound:1`, mode 0/1/4 and `pic_type0..6` selecting exactly three unique PNG paths. This is an in-battle knockout overlay, not a menu or the Q06 knockout producer. DAT files and gameplay rules are outside this content-only Task.

Exact destination paths under `Assets/NTSD/Content/LoganRuntime/vfs`, each currently absent: `sprite/kill/c.png`, `sprite/kill/sk1.png`, `sprite/kill/sk2.png`. Stage source bytes from formal `resources/runtime/vfs` and add only their three `.meta` files plus `sprite/kill.meta`; use the existing default PNG/folder importer templates with fresh GUIDs. Do not copy audio, unrelated mode/UI assets or whole directories. Do not edit scripts, Scene, Prefab, GameConfig, ProjectSettings, menus, old images, nonbattle functionality or deletion candidates.

Acceptance: source/destination length and SHA-256 equality for all three, PNG header dimensions, GUID uniqueness across project Assets, current Git scope and exact native reference closure. Status only `VERIFIED_EXACT_CONTENT_STAGING_ONLY`; no Unity reader, on-screen knockout overlay, formal EXE pixel parity, audio or lifecycle claim. Rollback/deletion requires the repository's explicit approval rule; no reset/clean.
