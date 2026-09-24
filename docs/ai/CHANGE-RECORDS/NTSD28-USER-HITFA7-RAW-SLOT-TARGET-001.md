<!-- CHANGE-RECORD
id: NTSD28-USER-HITFA7-RAW-SLOT-TARGET-001
status: FOCUSED_TEST_PASS
change-kind: NTSD28_HITFA7_RAW_SLOT_TARGET_FALLBACK
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs; Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
authority: shipped Logan playable NativeAi28::step_non_character_hit_fa inactive-slot note and full Unity SelfCheck FL-05 first difference
evidence: docs/ai/TASKS/NTSD28-USER-HITFA7-RAW-SLOT-TARGET-001.md
-->

# NTSD28-USER-HITFA7-RAW-SLOT-TARGET-001

2026-09-24 execution correction: the pre-edit prose below is historical. Added `RealOid875PreassignedEmptySlotUsesRawTargetPosition` in the declared Q07 Editor file, loading staged official Logan OID875/action55, registering only the type3 subject at X100 and verifying target slot10 empty/raw X0. Original-project Editor job `3e887fc7321e4c7aafb40eafde6ea172` failed as expected: Vx expected -1.4 versus actual0. `LF2Entity.RunNonCharacterHitFa7FrameLogic` now uses `Match.GetRawRuntimeSlotState(targetSlot)` only when its active entity query is null and slot is addressable, and reads its XInt/ZInt; invalid/sentinel slots still return. After compilation, job `7be6d87ef35e47a3b275b9b6f5b5898a` passed 2/2: new official-content empty-slot test and existing official-content active-target/no-clone control. First full SelfCheck cleared FL-05 but found the separate stale FL-02/HITFA-7 clone assertion. After that test-only correction, one full SelfCheck request returned exact `PASS` at local 03:27:48. Formal EXE empty-slot direct observation, complete Driver and D-024 reference-coordinate parity remain pending. No DAT/Scene/nonbattle edit. Report: `artifacts/diagnostics/NTSD28-USER-HITFA7-RAW-SLOT-TARGET-001/ACCEPTANCE.md`. Rollback remains limited to this package's test method and raw-target fallback, preserving other dirty work.

Status: `FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / EXE_EMPTY_SLOT_PENDING`. Exact source/Unity caller, precondition, expected side effects, paths, invariant boundaries, test-first validation, unknown formal EXE runtime result and rollback are in the Task. Actual RED/GREEN job IDs and SelfCheck result are recorded above. `Tools/Validate-ChangeLedger.ps1` returned exit 0 on 2026-09-24; warnings about historical Records whose declared files are not in the current diff did not fail validation.
