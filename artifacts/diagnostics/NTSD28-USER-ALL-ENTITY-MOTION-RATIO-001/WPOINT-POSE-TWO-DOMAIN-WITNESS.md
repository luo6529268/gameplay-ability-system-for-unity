# D-024 WPoint held-pose two-view witness

Status: `FOCUSED_TEST_PASS / TEST_ONLY`, 2026-09-24. Original-project Editor job `bdb426058e5f448da3a79d711c11d9f4` ran exactly two `NTSD28Q06HeldNativeFrameBindingEditorTests.MovedHolderWPointPose_KeepsSourceLocalAnchorAtBothViews` cases: 2 passed, 0 failed, 0 skipped. No production code or DAT was changed.

The test reuses the existing shipped-playable source row index1 from `artifacts/diagnostics/NTSD28-Q06-HELD-NATIVE-FRAME-BINDING-001/source/first.jsonl`. That row starts with holder X300/Z250 and has the static-holder formal held output X311/Z251. The paired playable `BattleWorld28::settle_held_refill_objects` in `source/ntsd28_core/src/simulation/battle_world.cpp` uses holder integer position plus both frames' local WPoint geometry. Unity's active `NTSDBattleTickSystem` calls `HeldObjectProcessAll`, which reaches `BattleHeldObjectWriter.RunStep12` and, for this row's non-`LF2WeaponBase` held entity, `SyncHeldFrameAndPosition`.

Each registered World begins with the unchanged fixture, then the holder receives raw Vx48 through production `CharacterMechanics.StepBattleLogic`; the test synchronizes its integer position and invokes production `HeldObjectProcessAll(1)`:

| View | Holder precise X | Holder integer X | Held integer X/Z | Held minus holder integer X |
| --- | ---: | ---: | ---: | ---: |
| 1333×730 | 348 | 348 | 359 / 251 | 11 |
| 2048×1152 | 373.7464366091523 | 373 | 384 / 251 | 11 |

The same raw DAT-local offset remains11 in both views. Scaling that local sprite anchor would change the pose; the approved D-024 factor belongs to the holder's physical travel. A future source-rule carrier must calculate held reference pose from holder reference integers and the same raw local geometry, while battle pose follows the scaled holder. It cannot recover source history by inverse-scaling the final held position.

This test covers one WPoint row and the non-weapon-component writer. The active `LF2WeaponBase.Act` → `LF2WeaponHeldStateResolver.ApplyHeldWPointSync` → `LF2WeaponBase.CoincideXYWithWPoint` path also writes X/Z and needs separate configured-view acceptance before carrier implementation. CPoint held/throw, actual sprite contact, full Driver/Battle Scene Play and formal EXE observable comparison remain pending. OID219/fusion first differences remain open; Q07 stays paused.
