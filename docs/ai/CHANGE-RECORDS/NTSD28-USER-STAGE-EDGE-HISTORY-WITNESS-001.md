<!-- CHANGE-RECORD
id: NTSD28-USER-STAGE-EDGE-HISTORY-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_STAGE_EDGE_TWO_DOMAIN_CHARACTERIZATION_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsCharacterPreFrameBoundsPassEditorTests.cs
authority: shipped Logan playable BattleWorld28::settle_ordinary_stage_bounds and user D-024 fixed-view battle-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-STAGE-EDGE-HISTORY-WITNESS-001.md
-->

# NTSD28-USER-STAGE-EDGE-HISTORY-WITNESS-001

Status: `FOCUSED_TEST_PASS / TEST_ONLY`. Before the script edit, the linked Task fixed the one-test path, source and Unity owners, synthetic but source-rule-consistent stage width/position/velocity, expected side effects, validation and rollback. Actual edit: added `FixedViewMotionNearAbsoluteStageEdge_CharacterizesTwoDomainBoundary` to `Assets/NTSD/Scripts/Test/Editor/BattleEcsCharacterPreFrameBoundsPassEditorTests.cs`. It registers an exact character, runs production `CharacterMechanics.StepBattleLogic` then production ECS preframe bounds, asserting factor1 X2040 and configured clamp X2048 from the same raw Vx40. Original Editor focused job `57c29991fd964a428e6e856c41e1c209`: 2/2 PASS. This proves a scoped dependency of the approved physical-distance exception, not full parity or a stage-boundary correction. Production stage bounds/motion, DAT, Scene, camera and nonbattle framework were untouched. Full Driver/Play, formal EXE and source-reference carrier remain pending. Report: `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/STAGE-EDGE-TWO-DOMAIN-WITNESS.md`. Rollback removes only this test method and its `using NTSD.Animation` addition.
