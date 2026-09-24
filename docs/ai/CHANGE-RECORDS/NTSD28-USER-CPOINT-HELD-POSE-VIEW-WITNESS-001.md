<!-- CHANGE-RECORD
id: NTSD28-USER-CPOINT-HELD-POSE-VIEW-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_CPOINT_HELD_POSE_TWO_VIEW_CHARACTERIZATION_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointSettlementRemainderEditorTests.cs
authority: shipped Logan playable BattleWorld28::settle_catch_relations and user D-024 fixed-view physical-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-CPOINT-HELD-POSE-VIEW-WITNESS-001.md
-->

# NTSD28-USER-CPOINT-HELD-POSE-VIEW-WITNESS-001

Status: `FOCUSED_TEST_PASS / TEST_ONLY`. Before script modification, the Task declared exact one-file test scope, unchanged formal source row1, paired CPoint settlement writers, production catcher movement and held pass, expected raw local gap25/Y4/Z249, original Editor two-view acceptance and rollback. Actual edit: added `MovedCatcherHeldCPointPose_KeepsSourceLocalAnchorAtBothViews` to the declared existing Q06 Editor test. It reuses the unchanged formal row1 DAT strings, materializes two registered logic entities, restores the row's initial frame/position/link state, moves the catcher through production `CharacterMechanics.StepBattleLogic`, synchronizes its integer mirrors, then invokes active `RunWeaponSyncHeldStep10` -> `BattleCpointWriter.SyncHeldPosition`.

Original-project Editor `refresh_unity` requested AssetDatabase import and compilation; subsequent state was idle/compiling false after reload. Exact `testNames` job `b1e0ecd2071448b580d1b62e66b45f0a` ran only the two parameterized cases: total2/passed2/failed0/skipped0. Factor1 catcher precise/int X348/348 and caught XInt373/YInt4/ZInt249; configured catcher precise X373.7464366091523/int373 and caught XInt398/YInt4/ZInt249. Both have local X gap25. No production, DAT, Scene, camera or nonbattle edit. Full Driver/Play, actual sprite/contact, nonzero CPoint Z, throw, formal EXE visible output and dual-coordinate history remain unverified. Rollback removes the one method. Report: `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/CPOINT-HELD-POSE-TWO-VIEW-WITNESS.md`.
