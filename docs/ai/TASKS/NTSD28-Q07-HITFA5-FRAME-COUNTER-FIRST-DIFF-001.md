# NTSD28-Q07-HITFA5-FRAME-COUNTER-FIRST-DIFF-001

Status: `VERIFIED_SCOPED_FIRST_DIFFERENCE`. Parent BATCH-04/Q07/D-024 remains open.

Authority and trigger: after `NTSD28-Q07-DEAD-NONCHAR-HITFA-MOTION-GATE-001`, the original Editor paired complete-Driver group1 passes completed tick1 HP/action/Vx/source X/Z/target, then differs at completed tick3 child action formal2/Unity1. Formal indexed `w/e.dat` frame1 declares `wait:2 next:2 hit_a:3 hit_d:1`. The paired playable `BattleWorld28::step_frame_slot` applies type3 HP drain before `FrameMachine28::step`; Unity's C25 type3 drain and special-attack dead event are possible counter writers. Static suspicion is not causal proof.

Exact diagnostic-only code scope: extend `Tools/NTSD28Q07Diagnostics/hitfa5_full_session_source_probe.cpp` CSV output with child `frame.frame_counter` and `frame.action_latch`; run to a new `run-02` directory without overwriting `run-01`. Extend `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HitFa5FullDriverEditorTests.cs` to compare the same paired-source fields to Unity `AttackingCounter` and `Trans.WaitCounter` after each completed tick, before the action assertion, after verifying the paired CSV shape. Production battle logic, DAT values, resources, scenes, config and nonbattle scripts are out of scope.

Acceptance: confirm formal executable/source/DAT identities, compile the diagnostic against the declared playable build closure, run eight-tick positive and negative cases, then refresh and run only the two original Editor complete-Driver cases to identify the earliest counter/latch difference. Preserve the old CSV/test evidence and all pre-existing worktree modifications. Record compile errors or bridge failures without converting them to a rule conclusion. A diagnosed first difference is this Task's exit, not full Q07 parity.

Rollback of diagnostic edits requires explicit approval for destructive restore; no existing artifacts may be overwritten or deleted.

Source step complete: fresh compile/run exit0; source child counter/latch on ticks1–3 are `1/1`, `2/1`, `0/2`, and all original 13 columns match preserved `run-01` rows. Original Editor test now reads the new rows; compile and run pending.

Original Editor paired run: negative eight-tick control passed; positive completed tick1 latch formal1/Unity1 passed, then counter formal1/Unity0 failed (jobs `f670a69ba2eb429cbd81af345653dc43` and `518772d72e5541d08e471e736c27f402`). This closes the diagnostic first-difference exit only. See same-ID `RESULT.md`; production fix requires a separate Task/Change.
