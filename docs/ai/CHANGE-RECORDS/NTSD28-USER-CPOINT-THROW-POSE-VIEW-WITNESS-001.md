<!-- CHANGE-RECORD
id: NTSD28-USER-CPOINT-THROW-POSE-VIEW-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_CPOINT_THROW_POSE_TWO_VIEW_CHARACTERIZATION_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointThrowRawBindingEditorTests.cs
authority: shipped Logan playable BattleWorld28 catch-relation throw branch and user D-024 fixed-view physical-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-CPOINT-THROW-POSE-VIEW-WITNESS-001.md
-->

# NTSD28-USER-CPOINT-THROW-POSE-VIEW-WITNESS-001

Status: `FOCUSED_TEST_PASS / TEST_ONLY`. Before script modification, the Task declared exact one-file test scope, unchanged formal source row0, paired CPoint throw writers, production catcher movement and throw pass, expected raw local gap11/Y-24/Z200, original Editor two-view acceptance and rollback. Actual edit: added `MovedCatcherThrowPosition_KeepsSourceLocalAnchorAtBothViews` to the declared existing Q06 Editor test. It constructs the unchanged source row0 registered World through its existing helper, moves catcher through production `CharacterMechanics.StepBattleLogic`, synchronizes integer mirrors and calls active `RunCpointAdvanceStep10` -> `BattleCpointWriter.ApplyThrow`.

Original-project Editor `refresh_unity` requested AssetDatabase import and compilation; subsequent state was idle/compiling false after reload. Exact `testNames` job `a75fa674dcb1434d9324c5a5fa351453` ran only the two parameterized cases: total2/passed2/failed0/skipped0. Factor1 catcher precise/int X148/148 and caught XInt159/YInt-24/ZInt200; configured catcher precise X173.7464366091523/int173 and caught XInt184/YInt-24/ZInt200. Both have raw local X gap11. The caught throw Vx1.5/Vy-2.25 also remained the fixture values. No production, DAT, Scene, camera or nonbattle edit. Full Driver/Play, landing injury, formal EXE visible output and dual-coordinate history remain unverified. Rollback removes the one method. Report: `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/CPOINT-THROW-POSE-TWO-VIEW-WITNESS.md`.
