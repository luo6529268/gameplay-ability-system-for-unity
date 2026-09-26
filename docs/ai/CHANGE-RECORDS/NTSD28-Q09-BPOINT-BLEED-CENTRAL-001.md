<!-- CHANGE-RECORD
id: NTSD28-Q09-BPOINT-BLEED-CENTRAL-001
status: RUNTIME_PENDING
change-kind: BATTLE_PRESENTATION_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderTypes.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCatalogCentralResourceResolverEditorTests.cs
authority: formal NTSD28 root EXE and paired playable render_snapshot.cpp bpoint plus d3d11_renderer.cpp solid draw
evidence: SOURCE_CONTENT_AUDIT_318_BPOINTS / TEST_FIRST_ORIGINAL_EDITOR_RED_6_ERRORS / ORIGINAL_EDITOR_COMPILE_0_ERRORS / NEW_TESTS_2_OF_2_PASS / ADJACENT_32_OF_32_PASS / MESH_1_OF_1_PASS / BELOW_THRESHOLD_1_OF_1_PASS / LEDGER_860_20_PASS / FOUR_PROTECTED_SHA_STABLE / PLAY_LEGACY_EXE_PIXEL_PENDING
-->

# NTSD28-Q09-BPOINT-BLEED-CENTRAL-001

Created before any script edit. Current Unity raw X/Y bpoint catalog is present, but its presentation snapshot and ordered central renderer have no bleed mark. The Task names the exact five script files, source rule, current-content boundary, nonbattle/DAT/Scene exclusions, threshold/geometry/order acceptance and rollback. The intended edit adds no manager, worker, pool, GameObject, resource asset or shutdown phase. It must not reinterpret `HPBound` as base max HP or write render coordinates into logic state.

After edits, append actual symbols and each compile, focused, Play, EXE and governance result. Until original Editor and scene/exe evidence exist, retain `IN_PROGRESS` or an honest intermediate status; never mark P-08 or Q09 complete from a command-only test.

Test-first RED: added the declared `BattlePresentationCommandWriterEditorTests.LowHpBPoint_CentralCommandFollowsBodyAndCurrentContentDefaults`. Original Editor PID11944, original project raw bridge `Assets/Refresh`, then `read_console` reported precisely six expected compile errors: two missing snapshot `BloodPoints`, three missing `BleedMark`/`CommonSolid` enum/key members, one missing constructor `bloodPoints` parameter. A parallel `get_editor_state` request had a transient closed bridge socket, but `read_console` returned the current six errors. Added the declared central resolver focused test after this RED; it has not yet been compiled. Existing unrelated test hunks were preserved.

Actual production edit now present, awaiting compile: `BattlePresentationEntitySnapshot` captures immutable current-frame `BloodPoints` and copies it through both snapshot paths. `BattlePresentationCoordinator.BuildCommands` reserves source-ordered point capacity and emits a central-only `BleedMark` immediately after a drawable body at current-content HP threshold, projecting mark coordinates from the resolved body rectangle with the existing visual scale. `BattleVisualResourceKey.CommonSolid` and `BattleCatalogCentralResourceResolver.ResolveCore` use built-in white texture with the already configured central fallback material; wrong key/render state/material fail closed. The declared two Editor test files now cover command conditions/geometry and resource resolution. No DAT, asset, Scene, combat-world or nonbattle code was changed; original Editor compilation and behavior remain unverified at this record point.

Fresh result supersedes the pending paragraph: original Editor compilation returned zero current Console errors. Exact new tests 2/2 PASS (`8c45ab0ece1248e5931bd6c829d9c05f`). First adjacent two-class job `10e3fd596ba946b282e057637dd12dcf` was 28/32, with four unrelated old trusted-command reflection fixture failures. After separately governed test-only correction, the same selected 32 tests passed 32/32 (`13260ab2c3614e79a4107c92dc026c78`). Added actual ordered-mesh vertex/quad/size assertions: 1/1 PASS (`e2b9584a0cde4989a2b4d68591f14b3e`). Added below-threshold assertion: final exact command test 1/1 PASS (`596b1cf7d4e541b1896e8ddd06b68fc4`). Current no-new-asset/no-new-shutdown-owner boundary holds. Ledger 860/20 and diff check exited0; four protected asset SHA values stayed stable. Detailed artifact: `artifacts/diagnostics/NTSD28-Q09-BPOINT-BLEED-CENTRAL-001/ACCEPTANCE.md`. Actual Battle Scene camera pixels, Legacy outlet and formal EXE comparison are not yet verified; P-08/Q09 remain open.
