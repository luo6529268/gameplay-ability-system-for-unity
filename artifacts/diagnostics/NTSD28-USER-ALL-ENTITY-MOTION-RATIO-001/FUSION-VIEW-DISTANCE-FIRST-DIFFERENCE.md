# D-024 formal fusion proximity first difference

Status: `ORIGINAL_EDITOR_PRODUCTION_SCAN_CONFIRMED / PRODUCTION_UNCHANGED`, 2026-09-24. Change ID `NTSD28-USER-FUSION-VIEW-DISTANCE-WITNESS-001`. Original-project Editor focused job `42504562540d48f29a0f42433c24eced` ran exactly two EditMode parameter cases, 2 passed, 0 failed, 0 skipped. A passing characterization test here records a parity defect; it is not a fixed fusion result.

The shipped Logan playable `BattleWorld28::advance_native_fusions` in `source/ntsd28_core/src/simulation/battle_world.cpp` checks raw integer `abs(primary.position.x-partner.position.x)<50` and Z gap `<8`, then writes an integer midpoint. Unity production `BattleOid5152RuntimeModule.TryMerge` checks the same constants using current battle integer positions. The focused fixture reuses the existing valid source row 0 (OID7 + OID8, source positions primary X340/partner X300, Z250, valid state/HP/team/action/slots) and exact formal `fusion.dat` bytes checked by the existing factory. It places the primary at raw absolute X320 and moves it one 20px raw-motion step through `CharacterMechanics.StepBattleLogic`, then synchronizes the integer X mirror in the same order the production integrator does before calling `SimulationWorld.Oid5152FusionScanAll(0)`.

| View setup | Movement factor | Primary current XInt | Partner XInt | Current gap | Production merge |
| --- | ---: | ---: | ---: | ---: | --- |
| Formal/default width1333 | 1 | 340 | 300 | 40 | yes; partner dormant |
| Approved fixed width2048 | 2048/1333 | 350 | 300 | 50 | no; partner remains active |

The formal source-rule state after the same raw 20px step has gap40 and therefore meets the proximity gate. The configured Unity World rejects the same pair because the 20px step already expanded at the final position outlet, while the rule threshold still consumes battle integer positions. This is a non-perceptual battle outcome: whether two characters fuse. It does not establish a rendered-EXE comparison or prove every fusion record/aspect/axis. No DAT, Scene, camera or production script was changed by this witness.

Do not fix this by changing fusion's `50` constant alone. OID219 `hit_Fa5` already proves that identical current battle gaps can come from different raw-position histories and require different source-rule results. The general source-rule reference coordinate must be preserved across birth, movement, teleports, attachments, split/merge, snapshots and resets, then used by source-rule distance gates where the paired playable code requires it. The current battle coordinate remains the user-approved full-background position for presentation and physical movement; collision, AI and stage effects need their own traced contracts.
