<!-- CHANGE-RECORD
id: NTSD28-USER-OPOINT-KIND1-RANDOM-BIRTH-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: D024_KIND1_OPOINT_RANDOM_BIRTH_RATIO
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06OpointMaterializerEditorTests.cs; Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeOpointBirthWriter.cs
authority: shipped Logan playable BattleWorld28::materialize_spawn_intents kind1 random order and user D-024 fixed-view battle-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-OPOINT-KIND1-RANDOM-BIRTH-RATIO-001.md
-->

# NTSD28-USER-OPOINT-KIND1-RANDOM-BIRTH-RATIO-001

Status: `PLANNED / TEST_FIRST`. Before either script edit, the linked Task declares exact code paths, source authority, Unity first difference, test sequence, invariant boundaries and rollback. Production status must remain PLANNED until the RED test demonstrates the configured view discrepancy. The final report must distinguish a D-024 battle outlet correction from the still-absent source-rule coordinate carrier and real Battle Scene/EXE validation.

2026-09-24 test-first result: added `Kind1RandomBirthX_UsesOneFixedViewRatioAfterNativeDraw` and its local fixture helper in the declared Editor test file, without changing repository DAT. Original-project Editor refreshed and compiled it; targeted job `126a974eb91243fba35b1eaa740971b0` ran exactly 1 case and FAILED at the configured screen-fraction assertion: raw factor1 random X delta `147`, expected configured `225.84846211552889`, actual `147`. Prior assertions in the same test passed for nonzero factor1 random delta, two synchronized draws, identical call counts between views, and unchanged Y/Z under this X-only fixture. This is the required measured RED before production edit; no broad suite was run.

Production edit now written in declared `BattleNativeOpointBirthWriter.InitializeBirth`: before, kind-1 random X/Z were added directly as raw integers and precise X/Z were overwritten with those integers; after, each already-drawn raw delta is multiplied once by the World X/projected-Z view factor at the final child birth offset, integer mirrors truncate the scaled precise result, and precise X/Z retain fractional displacement. The same four random calls remain in X/Y/Z/action order, Y and action remain raw, and factor1 arithmetic remains the formal integer value. No DAT, Scene, camera, unrelated code or source-rule reference carrier changed. Original Editor compile, GREEN, default-source materializer regression, real Play and final audit still pending at this checkpoint.

After the initial X-only GREEN job `d531a9f5424246a586014df8c72520bb` (1/1), the same declared test method was expanded to cover the writer's second transformed axis: the fixture now has nonzero `centerz:80`, asserts Z screen fraction `1152/730`, integer mirror truncation and four synchronized draws (X and Z), while Y stays unchanged. This is a test strengthening in the same code path and Change ID; the expanded version awaits original-Editor compilation and focused execution.

Final focused status: strengthened original-Editor job `99d1c0184d6245dfb8232342eb223661` 1/1 PASS for X and projected Z, integer mirrors, unchanged Y and synchronized call order. Existing source-row immediate materializer job `840455c81a6349c89a9efcc35f418ab3` 2/2 PASS for World and component routes at factor1. `git diff --check` passed; `Tools/Validate-ChangeLedger.ps1` passed with 732 records and 22 governed code files in the entire current diff. Battle Scene SHA remains `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`; no Git status entries under Config, LoganRuntime content, Scene or ProjectSettings. The full result and limits are in `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/KIND1-OPOINT-RANDOM-BIRTH-RATIO-ACCEPTANCE.md`.

`FOCUSED_TEST_PASS / RUNTIME_PENDING` is the honest limit. The configured-view fixture used synthetic source-formula-compatible OPoint amplitude values based on formal Yagura418 but did not drive actual Yagura in Battle Scene or compare formal EXE pixels. The new battle position cannot stand in for raw source-rule history; OID219/fusion and other writers remain open. Rollback is limited to the new test method/helper and the changed X/Z delta lines in the shared writer; other dirty work is preserved.
