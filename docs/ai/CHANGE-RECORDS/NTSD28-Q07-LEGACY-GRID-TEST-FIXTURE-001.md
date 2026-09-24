<!-- CHANGE-RECORD
id: NTSD28-Q07-LEGACY-GRID-TEST-FIXTURE-001
status: VERIFIED
change-kind: BATTLE_CONTENT_TEST_FIXTURE
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleSpriteGridSeparatorEditorTests.cs
authority: D-023 formal character images and Q07 current old-image reference graph
evidence: docs/ai/TASKS/NTSD28-Q07-LEGACY-GRID-TEST-FIXTURE-001.md
-->

# NTSD28-Q07-LEGACY-GRID-TEST-FIXTURE-001

Before: the generic legacy grid/green gutter regression reads `naruto_0.bmp` and `sasuke_0.bmp` from the pre-migration character resource folder. These are the only two exact old indexed image literals in the current 383-image text/GUID scan. Production formal PNG uses a different alpha-preserving path, already covered by `NTSD28B11PngSheetAlphaEditorTests`.

Planned diff: within the one declared Editor test file, generate a deterministic 800×560 in-memory legacy sheet with 79×79 first-frame content, all-green horizontal row and vertical column; keep grid-rect, opaque-content-before/after, gutter-clear and inner-green assertions. No production, DAT, image, Scene, Prefab, GameConfig or nonbattle edit. No deletion authorization.

Expected side effects and rollback: test no longer needs these two old character BMP files at their production paths. Green-gutter algorithm coverage remains; actual formal PNG alpha coverage remains in the separate unchanged B11 test. Roll back only the declared test diff under repository approval if its assertions cannot be made equivalent; preserve other workspace changes.

Actual diff: replaced two `[TestCase]` paths and the file loader with one deterministic in-memory 800×560 legacy BMP-shaped grid case; kept the frame-rect, opaque content, green gutter removal and inner-frame green assertions. No production code or resource was changed under this ID.

Validation: current original Unity Editor refresh/compile completed idle after domain reload. EditMode job `91c3e05b90844d64ae160d2c24bd8003`: 3/3 passed, including both grid tests and `NTSD28B11PngSheetAlphaEditorTests.FormalNar_ProductionStageCatalogAndGpu_PreserveNativeAlpha` with formal `c/nar/nar.png` staged through production and Direct3D11 GPU. `git diff --check` passed; saved Battle/Menu SHA-256 unchanged. `Tools/Validate-ChangeLedger.ps1` result is recorded in acceptance artifact. This is a test-fixture closure only, not permission to remove old BMPs or a Q07 completion claim.
