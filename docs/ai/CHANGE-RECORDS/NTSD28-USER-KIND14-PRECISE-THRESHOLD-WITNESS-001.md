<!-- CHANGE-RECORD
id: NTSD28-USER-KIND14-PRECISE-THRESHOLD-WITNESS-001
status: SUPERSEDED
change-kind: D024_KIND14_PRECISE_THRESHOLD_CHARACTERIZATION_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: shipped Logan playable BattleWorld28::resolve_special_relation_hit kind14 and user D-024 fixed-view physical-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-KIND14-PRECISE-THRESHOLD-WITNESS-001.md
-->

# NTSD28-USER-KIND14-PRECISE-THRESHOLD-WITNESS-001

2026-09-24 authority correction: the 2/2 original-Editor observation at X5.5/int5 (no flag) and X6.5/int6 (flag) was valid, but it matches the paired playable source's integer `Position28::x` comparison. The label `FIRST_DIFFERENCE_CONFIRMED` below was an incorrect interpretation based on a mistaken precise-operand reading. `NTSD28-USER-KIND14-INTEGER-OPERAND-AUTHORITY-CORRECTION-001` supersedes the parity conclusion and owns the corrected assertions.

2026-09-24 execution correction: the pre-edit `PLANNED` prose below is historical. The declared test method was added in `BattleHitExecutionPlanEditorTests.cs`; original-project Editor job `cbe5118f13804bc0ad603aa023dba18d` ran exactly two parameter cases, 2/2 PASS. The real candidate and production kind14 dispatch at precise/int attacker X5.5/5 with victim X0/Vx2 produced no positive-X block, while X6.5/6 did. This is the first-difference witness only. Successor `NTSD28-USER-KIND14-PRECISE-COORDINATES-001` owns the later formal-expected assertion, renamed/expanded method, and production correction. No DAT/Scene/nonbattle edit under this witness.

Status: `FOCUSED_TEST_PASS / FIRST_DIFFERENCE_CONFIRMED`. Declared exact path, symbol, authority, current behavior, expected side effect, non-edit boundaries, acceptance and rollback are in the Task. This test adds no gameplay behavior; it characterizes a candidate source/Unity branch difference and preserves the user-approved physical-position exception. No destructive operation. After the script edit, record actual method, original Editor compile/focused results, remaining uncertainty and ledger validation here. Rollback: remove the one new test method only.
