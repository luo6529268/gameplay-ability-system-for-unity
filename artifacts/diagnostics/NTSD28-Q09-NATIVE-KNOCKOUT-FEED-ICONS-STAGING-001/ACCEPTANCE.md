# Native knockout feed icons: exact content staging

Date: 2026-09-22. Status: `VERIFIED_EXACT_CONTENT_STAGING_ONLY`. BATCH-05/Q09, R17, and the overall battle-alignment goal remain open.

The formal playable `GameSession28::initialize_resources` loads the native knockout feed configuration from the selected mode child and passes it to the battle render snapshot; `d3d11_renderer.cpp` consumes the feed. The formal `decoded_dat/data/mode/ntsd.dat` has a bound `#killtext` block for modes 0, 1, and 4. Its `pic_type0..6` fields resolve to three unique battle overlay files: `sprite/kill/c.png`, `sk1.png`, and `sk2.png`. This is the exact selected PNG closure for this content package; its sound fields and other mode assets are outside scope.

The three PNGs were copied from formal `resources/runtime/vfs` into the original Unity project's `Assets/NTSD/Content/LoganRuntime/vfs` at the same relative paths. Independent source/destination checks confirm equal byte lengths and SHA-256 hashes for all three; [MANIFEST.csv](MANIFEST.csv) records those values and PNG dimensions (40×44, 40×45, and 40×40). Three asset metas and the new `sprite/kill.meta` use four distinct GUIDs; each GUID occurs exactly once among project `Assets/**/*.meta`. The staged root now has 337 DAT and 1031 PNG, leaving 224 of 1255 formal PNGs unstaged.

Git scope for this package is the three PNGs, their metas, one folder meta, and its Task/manifest/acceptance documentation. No script, Scene, Prefab, GameConfig, ProjectSettings, menu, audio, old asset, or nonbattle file was edited or deleted. The original Editor's Q09 WORDS focused request is consumed but still lacks a result file; this package makes no claim about a Unity knockout-feed reader, visible pixels, native EXE parity, or a passing runtime test.
