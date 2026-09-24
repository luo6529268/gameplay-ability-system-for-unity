<!-- CHANGE-RECORD
id: NTSD28-USER-STAGE-DEPTH-HISTORY-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_STAGE_DEPTH_TWO_DOMAIN_CHARACTERIZATION_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsCharacterStageZPassEditorTests.cs
authority: shipped Logan playable BattleWorld28::clamp_type0_stage_depth and user D-024 fixed-view physical-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-STAGE-DEPTH-HISTORY-WITNESS-001.md
-->

# NTSD28-USER-STAGE-DEPTH-HISTORY-WITNESS-001

Status: `FOCUSED_TEST_PASS / TEST_ONLY`. Before the script edit, the linked Task declared the exact authority, current Unity StageZ writer, one-test path, input, expected physical/reference branch, limits, validation and rollback. Actual edit: added `FixedViewMotionNearAbsoluteDepthEdge_CharacterizesTwoDomainBoundary` and the `NTSD.Animation` test import to `Assets/NTSD/Scripts/Test/Editor/BattleEcsCharacterStageZPassEditorTests.cs`. It uses a registered exact character, production `CharacterMechanics.StepBattleLogic`, and default DataOriented `ClampCharacterZToStageBoundsAll`. The existing `NTSDEntityRuntime` has only battle position; no production carrier was added. DAT, stage bounds, camera, Scene, production scripts and nonbattle framework were untouched.

Validation: original project Editor refreshed and domain-reloaded, then exact `testNames` EditMode job `b4333ba62f464c2385c206f329e05bc7` succeeded 2/2. The test was strengthened to assert the independent numeric pre-bound Z and expected configured factor; after a second refresh/reload, final job `ddce615b36c54e6b902682f627431db7` again succeeded 2/2, 0 failed, 0 skipped. Default view raw Z300+40 produced pre-bound Z340 and post-bound Z/ZInt340. Configured `1152/730` produced pre-bound Z363.123287671... and post-bound Z/ZInt350. Formal source gives the raw model, but no formal EXE tick observation is claimed. Full Driver/Play, non-character depth and carrier snapshot/reset/checksum remain pending. Report: `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/STAGE-DEPTH-TWO-DOMAIN-WITNESS.md`. Rollback is removal of the one added test method and its import.
