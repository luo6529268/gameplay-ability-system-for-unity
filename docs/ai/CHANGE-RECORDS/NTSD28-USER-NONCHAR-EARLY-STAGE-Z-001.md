<!-- CHANGE-RECORD
id: NTSD28-USER-NONCHAR-EARLY-STAGE-Z-001
status: FOCUSED_TEST_PASS
change-kind: NTSD28_NONCHAR_EARLY_STAGE_DEPTH_PASS_ORDER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterStageZPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsCharacterStageZPassEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/StageBoundsRuntimeSyncEditorTests.cs
authority: shipped Logan playable SimulationTickDriver28::step and BattleWorld28::clamp_type0_stage_depth plus user D-024 non-perceptual audit priority
evidence: docs/ai/TASKS/NTSD28-USER-NONCHAR-EARLY-STAGE-Z-001.md
-->

# NTSD28-USER-NONCHAR-EARLY-STAGE-Z-001

Status: `FOCUSED_TEST_PASS / RUNTIME_PENDING`. Created before any declared script edit. The linked Task records the formal pass order and all-active type-specific Z bound, Unity's two early character-only passes plus later non-character PreFrame fallback, exact paths, intended earlier Z/int side effects, excluded DAT/Scene/camera/nonbattle scope, focused validation and rollback. Original Editor RED job `da79757f77b140d4939946d45af63fa8` compiled and failed at the first production StageZ pass: registered weapon expected Z351, actual Z500. This is a measured production first difference, not a source-only prediction.

Actual production edits: `BattleEcsCharacterStageZPass.IsEligible` now admits all active registered entities. Both `CaptureExpected` and `ExecuteDataOriented` use type0 margin0 and other types margin1, preserve precise-Z clamp and always refresh ZInt. `SimulationStageRenderModule.ClampCharacterZToStageBoundsAll` Legacy/Shadow oracle uses the same all-active type-specific margins. X/Y, stage width, DAT, physical view scale, pass call sites and RNG are unchanged. Test edits: added a registered type1/type3 first-pass case with in-range fractional integer refresh, pending/dormant exclusions, second-pass idempotence and three mode cases to `BattleEcsCharacterStageZPassEditorTests`; updated the old non-character-skip assertion in `StageBoundsRuntimeSyncEditorTests` to formal ±1 behavior.

Original Editor refreshed/reloaded after edits. Initial target job `7463df56318e47dc8ea0465508be5086` passed 4/4 across three modes and the updated bounds test. After strengthening the new test, final targeted StageZ and StageBounds classes plus the existing WPoint two-view held witness passed 16/16 in job `6d4d1ee15b2e40fd8f623242963b3180`, 0 failed/skipped. This is compile + focused registered-World evidence. Actual full Driver/candidate/hit changes, Battle Scene Play and formal EXE observable trace remain unverified. No all-suite claim. Report: `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/NONCHAR-EARLY-STAGE-Z-ACCEPTANCE.md`. Rollback is limited to this Change's four code-path hunks; preserve all other dirty work.
