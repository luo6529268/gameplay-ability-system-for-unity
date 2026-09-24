<!-- CHANGE-RECORD
id: NTSD28-USER-GRAB-POSE-VIEW-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_GRAB_POSE_TWO_DOMAIN_CHARACTERIZATION_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchRelationExactFieldsProductionEditorTests.cs
authority: shipped Logan playable BattleWorld28::apply_native_relation_hit and user D-024 fixed-view battle-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-GRAB-POSE-VIEW-WITNESS-001.md
-->

# NTSD28-USER-GRAB-POSE-VIEW-WITNESS-001

Status: `FOCUSED_TEST_PASS / TEST_ONLY`. Before script modification, the linked Task declared one exact Editor test path and the source formula, Unity caller, factor1/configured inputs, expected positions, validation and rollback. Actual edit: one parameterized method `Kind3GrabAfterTargetMotion_KeepsRawLocalPoseWhileBattleHistoryChanges` in the declared file; no production or data file changed.

Original-project Editor `Assets/Refresh` then job `6fe9428e5d1f45ea9bd5e4049b3eb22e`, exact test method only: 2/2 passed, 0 failed. Factor1 target pre-grab XInt188 yielded attacker/target precise X148/140; configured target pre-grab XInt213 yielded X160.5/152.5; both pose gaps -8. Test `finally` required logic-only World ordered shutdown to succeed. See `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/GRAB-POSE-TWO-DOMAIN-WITNESS.md`.

This is a source-formula and production-writer dependency witness. It does not close the source-rule coordinate carrier, actual catch collision/visual contact, WPoint/CPoint remaining branches, OID219/fusion or D-024. No broad suite or real Battle Scene Play was run because this package only changed one Editor test. Rollback removes the one test method. Q07 remains paused.

Post-edit checks: `git diff --check` passed; the declared test file diff contains only the new 49-line method. `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` passed with 731 Records and 20 governed code files in the current diff. Original Editor returned idle and non-Play after the job. `Assets/NTSD/Scene/NTSD_Battle.unity` SHA-256 remained `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`; `Assets/NTSD/Config`, `Assets/NTSD/Scene` and `ProjectSettings` had no Git status entries. Formal EXE visible/contact behavior and real Battle Scene Play were not run for this witness.
