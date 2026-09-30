<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F04-EXACT-MAX-HEAL-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25PHealingOwnerEditorTests.cs
authority: current 336B44 playable BattleWorld28::advance_native_healing_slot ordinary heal_timer_e4 strict overshoot
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F04-EXACT-MAX-HEAL-001.md
-->

# NTSD28-336B44-Q07-F04-EXACT-MAX-HEAL-001

Created before script edits. Formal late-slot healing retains remaining E4 timer on exact max HP, clearing only strict overshoot. Unity original kernel uses clamped HP `>=` and is expected to clear too early. Only the declared shared kernel and focused existing Editor test class may change. Test-first RED must show exact equality while overshoot and later-injury controls prevent a special-case workaround. Anticipated side effect: a character healed exactly to full can use remaining timer after new damage; encoded E0, state1700 and noncharacter ownership must remain unchanged. Rollback exact declared hunks after review. Formal root/natural Play pending.

Test script written first in `NTSD28C25PHealingOwnerEditorTests`: a kernel case distinguishes exact max (492+8=500) from overshoot (493+8>500), then verifies later injury consumes the retained timer at its next eligible boundary; a production-owner case checks the exact-max result through `LateEntityUpdateAll`. Original Editor RED is pending. No production code changed yet.

Original Editor RED job `97bd838f8b0f4d3aa86dc755ba288ed0` executed both new tests: kernel exact max expected `(HP500, timer16)` but got `(500,0)`; production owner expected timer16 but got0. This confirms the shared strict-overshoot first difference. Proceed with the already-declared kernel predicate only, then rerun the focused class.

Production kernel hunk written in `BattleLateEntityLifecycleModule.cs`: preserve the un-clamped `healingCandidate=hp+8`, clamp HP to max, and clear ordinary timer only when `healingCandidate>effectiveMaxHp`. The only new behavior is remaining E4 timing at exact max; encoded E0 and owner gates are untouched. Unity compile/focused test and final audit pending.

Original Editor post-kernel class job `757803dc9d2942a6a3edea5c7c26f598` executed 8 tests: new kernel exact/overshoot/later-injury test passed; new production-owner case expected timer16 but got0; two existing production cases expected HP416/408 but got417/409. Investigation showed `LateEntityUpdateAll` runs `ApplyHpRecovery` first at default `NativeResourcePhase12=0`, and this fixture lacks a stats record, yielding a source-backed +1 HP before ordinary late healing. Thus the production test's HP492 setup is an overshoot (493+8), not exact max, and its original RED is not evidence of F04. Before further edit, the declared test file may set its production exact starting HP to491, and rebaseline the two old production HP expectations to417/409 including this existing +1 phase. No production change beyond the strict predicate is authorized by this fixture correction.

Those three fixture assertions were corrected in the declared test file; original Editor job `e5ad28307c6843d4a0d7978bddd5dca6` then passed all eight healing-owner tests with zero failures. The kernel RED for exact max remains valid, but the original production RED is classified as an invalid overshoot fixture. Actual production owner exact max now retains timer16; strict overshoot clears it and later injury can use the remaining timer. Existing encoded E0/state1700 checks passed. The formal root EXE paired state and natural Battle Play remain unverified, so this Change stays `RUNTIME_PENDING`. Rollback only the kernel predicate and the scoped test additions/corrections after review; preserve all unrelated work.
