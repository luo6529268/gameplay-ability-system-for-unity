# D-024 stage-edge two-domain dependency witness

Status: `ORIGINAL_EDITOR_SCOPED_BOUNDARY_DIFFERENCE_CONFIRMED / PRODUCTION_UNCHANGED`, 2026-09-24. Original-project Editor focused job `57c29991fd964a428e6e856c41e1c209`: exactly two EditMode cases, 2 passed, 0 failed, 0 skipped. Passing cases show the expected effect of the user's approved scaled physical travel; they do **not** classify this battle-position difference as an unauthorized defect or close the source-rule coordinate carrier.

Source authority: shipped Logan playable `BattleWorld28::settle_ordinary_stage_bounds` clamps type0 precise X to the absolute `StageBounds28.width`, and `SimulationTickDriver28::step` invokes the ordinary stage-bounds tail after prior movement/fusion phases. The Unity test registers an exact production character in a World whose absolute stage width is2048, starting X2000/Z250 with raw Vx40. It runs `CharacterMechanics.StepBattleLogic` with factor1 or `2048/1333`, then `SimulationWorld.ApplyPreFrameBoundsAll` through the default `BattleEcsCharacterPreFrameBoundsPass`.

| View width | Pre-boundary battle precise X | Post-boundary battle X/XInt | Formal raw position at this step |
| ---: | ---: | ---: | ---: |
| 1333 | 2040 | 2040/2040 | 2040, inside width2048 |
| 2048 | `2000 + 40×2048/1333 ≈ 2061.46` | 2048/2048 | 2040, still inside width2048 |

The battle edge contact at the larger fixed view is a consequence of the approved D-024 actual displacement. A future source-rule reference history must independently preserve the formal raw precise/int trajectory and apply its own source stage edge rule; copying the battle clamp or blocked-axis flag would incorrectly set it to2048 here. This is a test of production mechanics and production ECS bounds in a synthetic registered World, not a full Driver/Scene/EXE trace, and it does not settle collision or AI consumer domain choices. No DAT, Scene, camera or production script changed in this package.
