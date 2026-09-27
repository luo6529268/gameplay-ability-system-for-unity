# NTSD28-Q09-REVIVE-LIVES-GLYPH-ANCHOR-001

Status: `FOCUSED_TEST_PASS / NATURAL_GPU_PENDING`. Parent `NTSD28-UNITY-BATTLE-REALIGNMENT-001`, BATCH-05/Q09/P-09. Q07/Q08 and the Q09 aggregate remain open.

Authority: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; paired playable `source/ntsd28_core/src/rendering/render_snapshot.cpp` `native_revive_lives_text` and `RenderSnapshotBuilder28::build`, `source/ntsd28_playable/src/d3d11_renderer.cpp` revive-lives command, and `source/ntsd28_core/tests/render_snapshot_tests.cpp` visible x3 at X100 -> left87.

Current Unity: `BattleEntityOverlayLayout.TryBuild` uses `x + offset - (9 * glyphCount)/2 - cameraX`; at lives=2, X100 it places the first glyph at X91 rather than formal X87. For 100 or more lives it emits only `x` plus two low digits, whereas formal text clamps to `x999` and draws up to four glyphs. The counter Play probe covered Y motion, not this X/text rule.

Declared script scope: `Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs` for the shared allocation-free layout; one focused Editor test under `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09ReviveLivesGlyphAnchorEditorTests.cs` and its Unity `.meta`; and only the obsolete `hp2Cases`/counter-length expectation in `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` (existing other dirty hunks protected). No DAT, PNG, Scene, Prefab, camera, gameplay World, nonbattle UI, or audio change.

Acceptance: preserve the current visible HP2Orig>1 condition; for 2, 10, 100, 1000 lives emit `x2`, `x10`, `x100`, `x999` at X-13-cameraX, 9 pixels apart, without allocations or label changes. Run narrow compilation and the exact focused test in the original project if the existing Editor is available; compare protected Scene/Asset hashes, `git diff --check`, and Change Ledger validation. Do not claim P-09/Q09 fully complete: mode, frame-bound, natural GPU and EXE same-state visual gates remain separate.

Risk: a shared overlay layout also feeds Legacy command paths; use one common fix and guard counter/label order. Roll back only the exact hunks of this ID after reviewing pre-existing dirty work; never reset or clean the worktree.

Scoped result and actual validation are recorded in `artifacts/diagnostics/NTSD28-Q09-REVIVE-LIVES-GLYPH-ANCHOR-001/ACCEPTANCE.md`. No runnable pre-fix RED was collected; the formal-vs-Unity formula difference was identified before the production edit.
