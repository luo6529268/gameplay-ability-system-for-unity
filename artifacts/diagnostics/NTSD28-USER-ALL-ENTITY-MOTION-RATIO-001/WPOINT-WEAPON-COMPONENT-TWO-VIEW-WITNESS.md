# D-024 type1 weapon-component WPoint two-view witness

Status: `FOCUSED_TEST_PASS / TEST_ONLY`, Change `NTSD28-USER-WPOINT-WEAPON-COMPONENT-VIEW-WITNESS-001`, 2026-09-24. The original-project Editor compiled the new test and exact `testNames` job `13805c76522f442cac6bc7917647d39b` completed 2/2 PASS (0 failed, 0 skipped). This job ran only the two named parameterized cases, not the full 8345-case discovered suite.

The unchanged shipped-source fixture `NTSD28-Q06-HELD-NATIVE-FRAME-BINDING-001/source/first.jsonl` row21 has type1 held object and static formal pose holder X300 to held X311/Z251. The paired playable `BattleWorld28::settle_held_refill_objects` uses the holder's integer position and raw frame-local WPoints. Unity's `BattleHeldObjectWriter.RunStep12` dispatches this fixture's `LF2WeaponBase` to `LF2WeaponHeldStateResolver.ApplyHeldWPointSync` and `LF2WeaponBase.CoincideXYWithWPoint`. The test moved the holder once with raw Vx48 through production mechanics, explicitly synchronized the holder integer mirror, then ran production `HeldObjectProcessAll(1)`.

| View | Holder precise X | Holder integer X | Held integer X/Z | Local held minus holder X |
| --- | ---: | ---: | ---: | ---: |
| 1333×730 | 348 | 348 | 359 / 251 | 11 |
| 2048×1152 | 373.7464366091523 | 373 | 384 / 251 | 11 |

The component branch preserves its raw local attachment geometry after approved scaled physical holder travel. Its local WPoint offset must not be multiplied by the view factor. This result does not create the missing formal-reference coordinate history; formal EXE visible output, full Driver tick, sprite/contact appearance and other WPoint kinds remain outside this focused test. The separate non-weapon-component branch has its own row1 witness. CPoint held/throw and OID219/fusion remain open. No DAT, production script, Scene, camera or nonbattle change was made.
