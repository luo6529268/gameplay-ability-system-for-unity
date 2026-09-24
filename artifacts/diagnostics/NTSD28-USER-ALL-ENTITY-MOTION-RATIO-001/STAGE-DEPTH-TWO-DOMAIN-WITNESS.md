# D-024 independent StageZ two-domain boundary witness

Status: `FOCUSED_TEST_PASS / TEST_ONLY`, 2026-09-24. The original-project Editor final EditMode job `ddce615b36c54e6b902682f627431db7` ran exactly two declared cases in `BattleEcsCharacterStageZPassEditorTests.FixedViewMotionNearAbsoluteDepthEdge_CharacterizesTwoDomainBoundary`: 2 passed, 0 failed, 0 skipped. Its assertions include the independent pre-bound numeric Z and expected view factor; an earlier form passed job `b4333ba62f464c2385c206f329e05bc7` 2/2 before that strengthening. A passing characterization here proves a dependency of the approved fixed full-background physical distance; it does not prove that source-rule reference history already exists.

The paired shipped playable `BattleWorld28::clamp_type0_stage_depth` in `source/ntsd28_core/src/simulation/battle_world.cpp` clamps each active entity's precise Z to the absolute `StageBounds28.z_near/z_far` (with the documented non-type0 margin) and refreshes the integer mirror. It runs at the start of `settle_ordinary_stage_bounds`. Unity `NTSDBattleTickSystem.ClampCharacterZToStageBounds` invokes `SimulationWorld.ClampCharacterZToStageBoundsAll`; the default `BattleEcsCharacterStageZPass.ExecuteDataOriented` directly writes character Z/ZInt. This is a separate writer from the later preframe X/Z boundary path.

The focused test registers one exact character in the existing World helper with absolute stage Z180..350, initial Z300, raw Vz40 and no axis block. It runs production `CharacterMechanics.StepBattleLogic` at the World's configured view factor, then invokes the default production StageZ pass:

| View | Projected-Z motion factor | Pre-bound battle precise Z | Post-bound battle Z/ZInt | Formal raw source-rule result |
| --- | ---: | ---: | ---: | ---: |
| 1333×730 | 1 | 340 | 340 / 340 | 340 (inside edge) |
| 2048×1152 | 1152/730 | 363.123287671... | 350 / 350 | 340 (inside edge) |

The formal raw result is derived from the paired playable rule and the chosen synthetic input, not observed by launching the formal EXE. Under the user's D-024 exception, physical battle Z can reach the unchanged absolute stage edge earlier than raw source-rule Z. A future source-rule coordinate carrier must apply its own formally ordered Z bound and integer sync; copying the battle clamp or blocked flag would erase the history needed for rule-derived velocity/distance calculations. This does not authorize widening the stage or scaling DAT-local geometry.

This was one test-only script edit. The exact test job passed after the original Editor refreshed and reloaded. Full Driver/Battle Scene Play, formal EXE visible capture, non-character Z path, alternate StageZ modes and carrier snapshot/reset/checksum were not tested here. OID219 and fusion remain confirmed unresolved first differences; Q07 stays paused.
