<!-- CHANGE-RECORD
id: NTSD28-USER-HITFA7-NONCHAR-SELF-CHECK-CORRECTION-001
status: FOCUSED_TEST_PASS
change-kind: NTSD28_HITFA7_NONCHAR_CLONE_EXPECTATION_CORRECTION
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: shipped Logan playable NativeAi28::step_non_character_hit_fa behavior7 and original Editor FL-02/HITFA-7 first difference
evidence: docs/ai/TASKS/NTSD28-USER-HITFA7-NONCHAR-SELF-CHECK-CORRECTION-001.md
-->

# NTSD28-USER-HITFA7-NONCHAR-SELF-CHECK-CORRECTION-001

2026-09-24 execution correction: the pre-edit prose below is historical. The full SelfCheck after FL-05 repair stopped at FL-02/HITFA-7 because a type-Other near-full fixture expected a same-OID clone at the last free slot; the formal `NativeAi28` noncharacter behavior7 branch does not spawn one. Only `CheckFrameLifecycleNearFullPublication` changed: hitFa7 now asserts the source remains active in its original slot, last slot stays empty, dynamic occupant count stays one below full and RNG count is unchanged, then returns before child-publication checks. The old unreachable else assertion claiming hitFa7 clone publication was removed. HitFa5/11/13 production spawn checks and runtime code remain unchanged. The original Editor recompiled; one full SelfCheck request returned `PASS` at local 2026-09-24 03:27:48. This corrects a test expectation, not a production behavior or formal EXE observable certificate. No DAT/Scene/nonbattle edit. Report: `artifacts/diagnostics/NTSD28-USER-HITFA7-RAW-SLOT-TARGET-001/ACCEPTANCE.md`. Rollback reverses only this self-check branch correction.

Status: `FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / TEST_ONLY`. Exact existing self-check method, source/Unity branch, old expectation, new assertion, acceptance and rollback are in the Task; actual change and full SelfCheck result are recorded above. No production code changed under this ID. `Tools/Validate-ChangeLedger.ps1` returned exit 0 on 2026-09-24; warnings about historical Records whose declared files are not in the current diff did not fail validation.
