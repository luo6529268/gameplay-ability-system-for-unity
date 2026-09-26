<!-- CHANGE-RECORD
id: NTSD28-Q09-PROJECT-MODE-TEST-FIXTURES-001
status: FOCUSED_TEST_PASS
change-kind: Q09_PROJECT_MODE_TEST_FIXTURE_MIGRATION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KnockoutFeedRowSnapshotEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KilltextModeInputEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KillIconPublicationEditorTests.cs
authority: user-excluded original mode DAT and approved ProjectBattleModeConfig asset production mode source
evidence: docs/ai/TASKS/NTSD28-Q09-PROJECT-MODE-TEST-FIXTURES-001.md; artifacts/diagnostics/NTSD28-Q07-NATIVE-KNOCKOUT-EVENT-CLOCK-001/q09-fixture-failure.json
-->

# NTSD28-Q09-PROJECT-MODE-TEST-FIXTURES-001

Pre-change: row projection SetUp reads excluded `decoded_dat/data/mode/ntsd.dat` and fails with `DirectoryNotFoundException`; mode-input test also reads that removed path; icon publication fixture copies both excluded mode DAT files into temporary content. These were historical test assumptions, not current production ownership. Production already captures `ProjectBattleModeConfig.LoadDefault().Capture()` and converts it through `LoganModeKnockoutFeedInput.FromProjectSnapshot`; the asset itself remains unchanged.

Declared change: three Editor tests only. Use the saved project Asset snapshot for normal mode feed and icon candidate. Build the one excluded-victim row variation from a transient in-memory project config, dispose it, and preserve all other test expectations and selected PNG bytes. Preserve parser grammar tests as synthetic parser-only cases. No runtime script, DAT, Scene, ProjectSettings or nonbattle change. Exact validation and rollback in the Task.

Post-change code written: row test SetUp now takes `LoganModeKnockoutFeedInput.FromProjectSnapshot(ProjectBattleModeConfig.LoadDefault().Capture())`; the excluded-victim variant clones the loaded Asset in memory, changes only that clone, captures a snapshot and destroys it. Mode-input test now asserts the saved Asset projection and its normalized `/` paths; its two synthetic parser tests remain. Icon publication candidate and malformed-PNG candidate both receive the saved project snapshot, while fixture construction copies only the three formal icon PNGs, not mode DAT. These are the three declared Editor scripts; compile and focused original-Editor runs are pending. No production/Asset/Scene/DAT mutation.

Original Editor first focused run: compile 0 errors; 9 tests selected and 8 passed, 1 failed in icon candidate capture with `Native SPARK requires both resource.dat and system.dat.` This is a transient fixture completeness issue: its inherited WORDS fixture already copies `resource.dat` and production candidate therefore requires formal `system.dat` plus the referenced `SPARK.png`. Correct only the declared icon test fixture and rerun focused tests. Failure preserved at `artifacts/diagnostics/NTSD28-Q09-PROJECT-MODE-TEST-FIXTURES-001/original-editor-focused-9-result.json`.

Focused rerun: icon test 1/1 PASS (job `1a47ea0b216f4b5a9bafb3415176ae1b`), then all affected 9/9 PASS (job `5a1f607e6235465bb15349eb5e892d1a`), with nine actual completed selections. The latter covers 30-tick feed row boundary, missing/excluded actors, mode-name gate and worker publication, project-mode projection and synthetic parser errors, seven alias icons, failed-publication retention and old-catalog lease release. Original Editor assembly timestamp `2026-09-26 05:48:41` is later than the final source edit `05:48:23`; Console errors 0. Source search has no excluded mode DAT path in the three modified test scripts. Menu/Battle/GameConfig/ProjectBattleModeConfig hashes remained `785F828C...81E13` / `2EE465D8...8B77A` / `0527D737...CB8EA7` / `88E10D43...686F55C`. No production code, saved Asset, DAT or Scene was edited by this package. Reports are in the package diagnostic directory. Q09 visual and formal EXE A/B remain open.

Final checks: `Tools/Validate-ChangeLedger.ps1` exited 0 (`PASSED`, 856 Records, 15 governed code files in current diff); `git diff --check` exited 0. The Editor is idle EditMode. After the tests, Console contains only two intentional BMPLoader Error logs from the malformed-PNG rejection case; that case declares and consumes the expected log with `LogAssert.Expect`, and all nine tests pass. Do not report post-test Console as empty. Validator log is in the package diagnostic directory.
