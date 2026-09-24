<!-- CHANGE-RECORD
id: NTSD28-USER-WPOINT-WEAPON-COMPONENT-VIEW-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_WPOINT_WEAPON_COMPONENT_TWO_VIEW_CHARACTERIZATION_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06HeldNativeFrameBindingEditorTests.cs
authority: shipped Logan playable BattleWorld28::settle_held_refill_objects and user D-024 fixed-view physical-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-WPOINT-WEAPON-COMPONENT-VIEW-WITNESS-001.md
-->

# NTSD28-USER-WPOINT-WEAPON-COMPONENT-VIEW-WITNESS-001

Status: `FOCUSED_TEST_PASS / TEST_ONLY`. Before the script edit, the Task declared the exact test file, unchanged source fixture row21, formal and Unity active writers, two-view expected integer positions, side-effect limits, focused original Editor validation and rollback. Actual edit: added `MovedHolderWeaponComponentWPointPose_KeepsSourceLocalAnchorAtBothViews` to the declared existing Editor test file. It asserts source row21/type1 and Unity `LF2WeaponBase`, moves the holder through production `CharacterMechanics.StepBattleLogic`, synchronizes holder integer position and calls production `HeldObjectProcessAll(1)`; this selects `LF2WeaponHeldStateResolver.ApplyHeldWPointSync` and `LF2WeaponBase.CoincideXYWithWPoint` through the active writer.

Original-project Editor PID288224 at `I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity`: `refresh_unity` requested AssetDatabase refresh and compilation; the subsequent editor state was idle/compiling false after domain reload. Exact `testNames` job `13805c76522f442cac6bc7917647d39b` ran the two parameterized cases, total2/passed2/failed0/skipped0. Factor1 holder precise/int X348/348 and held XInt359/ZInt251; configured view holder precise X373.7464366091523/int373 and held XInt384/ZInt251. Both keep local gap11. This is a focused registered-World pose arithmetic witness, not formal EXE visible output, full Driver/Play or sprite contact. No production, DAT, Scene, camera or nonbattle edit. Rollback removes the one method. Report: `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/WPOINT-WEAPON-COMPONENT-TWO-VIEW-WITNESS.md`.
