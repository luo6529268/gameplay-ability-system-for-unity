<!-- CHANGE-RECORD
id: NTSD28-Q09-REVIVE-LIVES-GLYPH-ANCHOR-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_PRESENTATION_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09ReviveLivesGlyphAnchorEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal NTSD28 root EXE and paired playable render_snapshot.cpp revive_lives plus d3d11_renderer.cpp glyph draw
evidence: FORMAL_SOURCE_AND_EXISTING_RENDER_SNAPSHOT_TEST_X100_TO_87 / UNITY_PRECHANGE_FORMULA_X100_TO_91 / ORIGINAL_EDITOR_EXACT_4_OF_4_PASS / ADJACENT_1_OF_1_PASS / GENERATED_EDITOR_BUILD_0_ERRORS / LEDGER_923_33_PASS / NATURAL_GPU_PENDING
-->

# NTSD28-Q09-REVIVE-LIVES-GLYPH-ANCHOR-001

Created before any script edit. Exact pre-change difference, script scope, exclusions, acceptance, risks, and rollback are in the matching Task Contract. The formal rule positions the first lives glyph at X-13 independently of text length, and clamps text to x999. Unity currently centers a two/three-character counter and discards hundreds digits. This is a battle presentation difference only; no logic position or resource DAT value is to change.

After the script edit record actual symbols, compile/focused result, protected asset hashes, remaining native and natural Play gates. Advance status only to the verified evidence layer.

Actual edits: `BattleEntityOverlayLayout.MaximumGlyphCount` grew by one; `TryBuild` now clamps the displayed lives at 999, uses X+existingRenderOffsetX-13-cameraX regardless of glyph count, and writes all decimal digits. The existing `BattleRuntimeSelfCheck.CheckBattleEntityOverlayLayoutContracts` counter cases now expect four glyphs for 100/1000 and the formal 999 clamp. New independent four-case Editor test and `.meta` are the only added scripts/assets. No script edit occurred outside the declared paths. The test was authored before production, but the runnable RED was not collected; only the formal-vs-Unity formula first difference was recorded before the fix.

Original Editor 2022.3.62f3 PID11944 used its existing local MCP bridge for force all-asset refresh and compilation. Exact four-case job `13b6aef86726485b84f0085b4a5eff7d` passed 4/4; adjacent optimized writer job `6ed041716b8d42709f340c5080a22b83` passed 1/1. Earlier misfiltered group job had 0 tests and supplies no evidence. Generated Editor C# build exited 0 errors/218 warnings. Console error filter showed only MCP client-exit entries, no compiler error. Ledger validator PASS 923/33, diff check PASS; Battle/Menu scenes Git-clean with prior SHA values. Natural scene GPU and formal EXE visual comparison are pending; P-09/Q09 remain open. Full evidence: `artifacts/diagnostics/NTSD28-Q09-REVIVE-LIVES-GLYPH-ANCHOR-001/ACCEPTANCE.md`.
