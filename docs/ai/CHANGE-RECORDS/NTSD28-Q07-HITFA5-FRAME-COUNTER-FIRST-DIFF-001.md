<!-- CHANGE-RECORD
id: NTSD28-Q07-HITFA5-FRAME-COUNTER-FIRST-DIFF-001
status: VERIFIED
change-kind: Q07_HITFA5_FRAME_COUNTER_FIRST_DIFFERENCE_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/hitfa5_full_session_source_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HitFa5FullDriverEditorTests.cs
authority: formal root NTSD2.8-Logan.exe paired playable BattleWorld28::step_frame_slot and FrameMachine28::step with indexed w/e.dat frame1
evidence: original Editor complete Driver after scoped HP and motion repairs first differs at completed tick3 child action formal2 versus Unity1
-->

# NTSD28-Q07-HITFA5-FRAME-COUNTER-FIRST-DIFF-001

Pre-change: paired formal source CSV has child action/X/Z/Vx/target but omits internal frame counter and action latch. Existing original Editor test stops at tick3 action before these counters are observed. Unity `LF2SpecialAttack.Generic_Die` could reset its counter each tick, while formal type3 drain writes action before frame-machine advance; this is a hypothesis requiring paired runtime data.

Planned bounded diagnostic edits and acceptance are in the Task Contract. No production logic, DAT, image, Scene or config write. Add a fresh run directory; leave prior `run-01` intact. Report source counters and original Editor counter first difference with exact tick and status. No complete parity or root EXE same-world claim.

2026-09-26 source diagnostic implementation: the existing paired-Session probe now appends `child_frame_counter` and `child_action_latch` without changing its original 13 columns. MinGW g++ C++17/`-municode` with all 28 current core units and playable `game_session.cpp`/`selection_flow.cpp` compiled exit0 to a new artifact executable. Fresh `run-02` positive/negative Session invocation exit0 (`positive_birth=1 negative_birth=0`); original 13 data columns are identical to preserved `run-01` for both eight-tick cases. Formal positive child tick1 counter/latch=1/1, tick2=2/1, tick3=0/2. The original Editor test now reads `run-02`, verifies 15-column child rows and compares `AttackingCounter`/`Trans.WaitCounter` before child action. Original Editor recompile/run and first-difference result pending. No production, DAT or Scene change in this diagnostic.

First original Editor read `f670a69ba2eb429cbd81af345653dc43`: two selected cases completed; group3 negative passed, group1 failed at completed tick1 frame counter formal1/Unity0. The action-latch assertion had been placed after the counter and was not reached. The same declared test file now compares latch first, then counter, to determine whether the latch also differs before the known counter RED. No production change.

Final diagnostic: second original Editor group1 job `518772d72e5541d08e471e736c27f402` passed completed tick1 action-latch formal1/Unity1, then failed frame counter formal1/Unity0. The earliest internal difference is therefore frame counter at tick1. Full paired source run-02 tick1/2/3 counter/latch `1/1`, `2/1`, `0/2`; its prior 13 columns are unchanged. Formal/Unity production-path static reading identifies a candidate post-C25 Unity `Generic_Die -> SetFrameDirect(hit_d)` counter reset; no production edit made under this diagnostic ID. Source compile/run exit0; Unity compilation succeeded with both jobs reaching assertions. `artifacts/diagnostics/NTSD28-Q07-HITFA5-FRAME-COUNTER-FIRST-DIFF-001/RESULT.md` records evidence and limits. Status `VERIFIED` is only this first-difference diagnosis; Q07/D-024 remain open.
