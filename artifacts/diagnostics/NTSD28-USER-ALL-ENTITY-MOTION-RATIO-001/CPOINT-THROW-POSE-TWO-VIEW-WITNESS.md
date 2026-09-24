# D-024 CPoint throw-position two-view witness

Status: `FOCUSED_TEST_PASS / TEST_ONLY`, Change `NTSD28-USER-CPOINT-THROW-POSE-VIEW-WITNESS-001`, 2026-09-24. Original-project Editor exact `testNames` job `a75fa674dcb1434d9324c5a5fa351453` completed 2/2 PASS, 0 failed/skipped. The job ran only the two named parameterized cases, not the full discovered suite.

The unchanged shipped-source fixture `NTSD28-Q06-CPOINT-THROW-NATIVE-RAW-BINDING-001/source/first.jsonl` row0 has `thrown=1`, static catcher X100/Y-5/Z200 and formal caught after throw X111/Y-24/Z200. Paired playable `BattleWorld28::advance_catch_relations` writes caught integer X/Y from catcher integer position and raw local CPoint/frame center, mirrors precise X/Y, and leaves Z as it was. Unity's active `LF2Entity.RunCpointAdvanceStep10` reaches `BattleCpointWriter.ApplyThrow`. The test moved catcher once with raw Vx48 through production mechanics, synchronized integer mirrors, then invoked this actual throw path.

| View | Catcher precise X | Catcher integer X | Caught integer X/Y/Z | Local caught minus catcher X |
| --- | ---: | ---: | ---: | ---: |
| 1333×730 | 148 | 148 | 159 / -24 / 200 | 11 |
| 2048×1152 | 173.7464366091523 | 173 | 184 / -24 / 200 | 11 |

The test also retained fixture throw velocity Vx1.5/Vy-2.25. This confirms that this throw-position writer preserves the raw local CPoint offset after approved physical catcher travel; it does not scale the local anchor or change DAT throw velocity. It does not prove full Driver/Play, landing injury, formal EXE visible output, other CPoint branches or missing formal-reference coordinate history. OID219/fusion remain open. No DAT, production, Scene, camera or nonbattle edit.
