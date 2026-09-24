<!-- CHANGE-RECORD
id: NTSD28-USER-WPOINT-POSE-VIEW-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_WPOINT_TWO_VIEW_LOCAL_POSE_CHARACTERIZATION_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06HeldNativeFrameBindingEditorTests.cs
authority: shipped Logan playable BattleWorld28::settle_held_refill_objects and user D-024 fixed-view physical-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-WPOINT-POSE-VIEW-WITNESS-001.md
-->

# NTSD28-USER-WPOINT-POSE-VIEW-WITNESS-001

Status: `FOCUSED_TEST_PASS / TEST_ONLY`. Before the script edit, the Task declared the authority, exact one-file test scope, source fixture row1, production holder movement and held writer entry, expected local anchor invariant, side-effect limits, original Editor validation and rollback. Actual edit: added `MovedHolderWPointPose_KeepsSourceLocalAnchorAtBothViews` to `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06HeldNativeFrameBindingEditorTests.cs`. It reuses unchanged row1, moves the registered holder through production `CharacterMechanics.StepBattleLogic`, explicitly synchronizes the integer mirror at the post-mechanics boundary, then runs production `SimulationWorld.HeldObjectProcessAll`. This row creates a non-`LF2WeaponBase` held entity and exercises `BattleHeldObjectWriter.SyncHeldFrameAndPosition` through `RunStep12`; it does not cover the separate weapon-component `LF2WeaponHeldStateResolver` writer.

Original-project Editor exact job `bdb426058e5f448da3a79d711c11d9f4`: 2/2 passed, 0 failed, 0 skipped. Factor1 holder precise/int X348/348 and held XInt359/ZInt251; configured view holder precise X373.7464366091523/int373 and held XInt384/ZInt251. Both have local X gap11. This verifies local WPoint pose arithmetic after scaled holder movement, not actual sprite contact, formal EXE frame capture or full Driver/Play. No production, DAT, Scene, camera or nonbattle edit. Rollback removes the one test method. Report: `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/WPOINT-POSE-TWO-DOMAIN-WITNESS.md`.
