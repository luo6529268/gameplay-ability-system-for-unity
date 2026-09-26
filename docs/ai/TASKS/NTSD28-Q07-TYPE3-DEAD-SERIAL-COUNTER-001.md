# NTSD28-Q07-TYPE3-DEAD-SERIAL-COUNTER-001

Status: `FOCUSED_TEST_PASS / SCOPED_EIGHT_TICK_PARITY`. Parent BATCH-04/Q07/D-024 remains open.

Authority and trigger: the formal paired playable `BattleWorld28::step_frame_slot` applies type-3 `hit_a`/`hit_d` before `FrameMachine28::step`; ordinary dead type-3 frames with no positive `hit_a` do not reset `frame_counter` in the late serial pass. For indexed `w/e.dat` OID219 hit_Fa5, formal completed tick1 counter/latch is `1/1`, whereas the original Unity Editor complete Driver reaches `0/1`. Unity `LF2SpecialAttack.RunPostNativePhysicsSerialForWorldPass` calls `DieEvent` after native C25 and `Generic_Die -> SetFrameDirect(hit_d)` resets `AttackingCounter` on the same action every tick. This causal candidate must be validated with the paired positive and negative full-Driver tests.

Exact write scope: only `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs`, removing the late death-event frame redirect from the native type-3 serial tail while preserving native C25 type-3 HP drain, state-entry handling and the existing non-native legacy `DieEvent` implementation. No OID, character, DAT-value, camera, scene, input or nonbattle exception. The existing `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HitFa5FullDriverEditorTests.cs` already contains the failing counter assertion and needs no edit for this task.

Acceptance: preserve the recorded original Editor RED (formal counter1/Unity0 at completed tick1), apply the minimal production edit, refresh the original Editor, run both eight-tick paired full-Driver cases plus the neighboring type-3 native-frame transaction group. Report the next first difference if positive parity remains open. Verify script compilation, protected scene/config hashes, change-ledger validator and `git diff --check`. Full SelfCheck's existing alternate-damage assertion and natural Play are separate gates, not silently treated as passing.

Rollback: only the declared production lines, with explicit approval required before destructive Git restore; preserve all existing dirty work and evidence.

Result: original Editor paired positive/negative complete-Driver eight-tick cases 2/2 PASS and native type-3 frame neighbor 1/1 PASS after the single late-event removal. See same-ID `ACCEPTANCE.md`; no natural Battle Play or whole-Q07 claim.
