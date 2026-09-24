# D-024 state9996 clone relative birth first difference

Historical pre-fix status: `ORIGINAL_EDITOR_PRODUCTION_BIRTH_CONFIRMED`, 2026-09-24. Original-project Editor job `f2fbd17c1a1548b480ce8166db78f62f` ran exactly two EditMode parameter cases, 2 passed, 0 failed, 0 skipped. Passing assertions characterized the then-current defect; they were not a fix. Superseding scoped correction: `NTSD28-USER-STATE9996-CHILD-RATIO-001` configured ratio RED `f15b213dffd9458a8d5fd73e3606f0d1`, all-five-child focused GREEN `f1dc8c0b632a43d4a36c33380a865e53`; default source-row immediate/following job `eae331fdc81a48958c91e511d9da16be` 2/2. Battle Scene Play and general coordinate history remain open.

The shipped Logan playable `BattleWorld28::advance_native_special_clones` uses `source->position.x + random_x`, where synchronized `random_x` is in [-3,3], and `source->position.z + 1`. Existing source-final row0 is a successful five-child OID217/218 spawn; its first child OID217 has `random_x=-3` from parent X300. The Unity test reuses this exact source row and actual `BattleLateEntityLifecycleModule.SpawnState9996Children` via the existing diagnostic boundary. Before birth it advances the parent through production `CharacterMechanics.StepBattleLogic` by raw Vx20, syncs integer X and examines the materialized child at slot50.

| View width | Parent XInt after movement | Child XInt | Relative XInt | Current screen fraction | Formal relative fraction |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 1333 | 320 | 317 | -3 | -3/1333 | -3/1333 |
| 2048 | 330 | 327 | -3 | -3/2048 | -3/1333 |

At the configured fixed full-background view the relative child offset remains three actual pixels even though the parent movement has expanded by `2048/1333`, so child-to-parent visible separation is too small. Unity also keeps raw Z+1; its precise projected-depth ratio needs a paired scoped check before the X/Z outlet is corrected. The birth task has both precise direct coordinates and explicit integer mirrors. Any correction must preserve exact synchronized random calls and the formal raw source-rule reference, and update precise/integer positions consistently. No DAT, Scene, camera or production script was changed by the witness.
