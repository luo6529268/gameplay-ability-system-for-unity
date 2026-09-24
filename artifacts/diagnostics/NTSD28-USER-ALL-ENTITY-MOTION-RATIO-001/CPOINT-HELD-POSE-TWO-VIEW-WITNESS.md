# D-024 CPoint held-pose two-view witness

Status: `FOCUSED_TEST_PASS / TEST_ONLY`, Change `NTSD28-USER-CPOINT-HELD-POSE-VIEW-WITNESS-001`, 2026-09-24. Original-project Editor exact `testNames` job `b1e0ecd2071448b580d1b62e66b45f0a` completed 2/2 PASS, 0 failed/skipped. The job ran only the two named parameterized cases, not the full discovered suite.

The unchanged shipped-source fixture `NTSD28-Q06-CPOINT-SETTLEMENT-REMAINDER-FRAME-BINDING-001/source/first.jsonl` row1 uses formal CPoint kind1/kind2 settlement. At static catcher X300, formal caught after settlement is X325/Y4/Z249. Paired playable `BattleWorld28::settle_catch_relations` writes caught position from catcher integer position and raw frame-local CPoints. Unity's active `LF2Entity.RunWeaponSyncHeldStep10` delegates to `BattleCpointWriter.SyncHeldPosition`. The test moved the catcher once with raw Vx48 through production mechanics, synchronized its integer mirror, and invoked this actual settlement entry.

| View | Catcher precise X | Catcher integer X | Caught integer X/Y/Z | Local caught minus catcher X |
| --- | ---: | ---: | ---: | ---: |
| 1333×730 | 348 | 348 | 373 / 4 / 249 | 25 |
| 2048×1152 | 373.7464366091523 | 373 | 398 / 4 / 249 | 25 |

This confirms that this held-pose writer preserves raw local CPoint geometry after approved physical catcher travel. The local attachment offset should not be view-scaled. This test does not cover the separate CPoint throw writer, nonzero CPoint Z, full Driver/Play, contact pixels, formal EXE visible output, or the missing formal-reference coordinate history. OID219/fusion remain open. No DAT, production, Scene, camera or nonbattle edit.
