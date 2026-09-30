# Q09/P-02 Legacy shadow prewarm focused result (2026-09-29)

Change: `NTSD28-Q09-LEGACY-SHADOW-PREWARM-001`. The original Battle Scene was open in the original Unity Editor (PID 11944); no second Editor was started. The selected project GameConfig supplies its own Shadow prefab/descriptor. This package changed only the Legacy/Hybrid pool object-creation branch and one dedicated Editor test; it did not edit DAT values, Scene, Prefab, GameConfig asset, camera, simulation state or nonbattle code.

The test-first corrected fixture ran four selected EditMode cases under job `afa22fdf5bb74c6db903a460fb023c20`: LegacyOnly selected prefab, LegacyOnly fallback and CentralShadowBuild selected prefab failed at the missing shadow assertion; CentralOnly selected prefab passed its no-shadow assertion. The first test job failed earlier inside an uninitialized EditMode pool and was not counted as a shadow RED. The raw meaningful RED response is `legacy-shadow-prewarm-red-20260929.json`.

After the pool change, original Editor refresh compiled and reloaded scripts; exact selected job `b09b1e9b31b94c4c82d3dd10f32ade72` completed four of four tests, all PASS, total case duration 5.404 s. Tests assert the descriptor sprite/material/color/flip/mask, renderer binding, fallback, CentralOnly nonmaterialization, release/reborrow reuse, inactive reset and zero active borrowers. Raw response: `legacy-shadow-prewarm-green-20260929.json`. The job's catalog total 8619 is discovery metadata, not cases executed.

`Tools/Validate-ChangeLedger.ps1` PASSED (996 records, 28 changed governed code files; pre-existing historical warnings about declared files outside this diff remain). `git -c core.safecrlf=false diff --check` passed. Protected SHA-256 after the test:

- `NTSD_Battle.unity`: `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`
- `NTSD_Menu.unity`: `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`
- `GameConfig.asset`: `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`
- `ProjectBattleModeConfig.asset`: `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`

Limit: this proves component creation and binding in the original Editor, including reuse. It does not prove a Legacy-born actor's visible pixels, natural shadow gate, interpolation, formal EXE same-view comparison or Q09/P-02 completion. Those Play/visual checks remain open. Q07 is still the earliest open Q, and Q08/BATCH-04 remain open independently.
