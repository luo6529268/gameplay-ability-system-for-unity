<!-- CHANGE-RECORD
id: NTSD28-Q09-P02-HELD-MOTION-REAL-TICK-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HeldWeaponDualDomainEditorTests.cs
authority: formal playable presentation_interpolation.cpp and selected native held-weapon 24-tick trace with approved D-024 view scaling
evidence: docs/ai/TASKS/NTSD28-Q09-P02-HELD-MOTION-REAL-TICK-001.md
-->

# NTSD28-Q09-P02-HELD-MOTION-REAL-TICK-001

Before: the Q07 strict 24-tick held-weapon test proves rule/view movement and relation fields against formal source, but it never reads Q09's published previous/current motion rows or interpolates a genuinely held weapon. After this diagnostic-only edit, the same scenario must show pickup relation break and stable held X/Z half-tick sample without changing World checksum. The exact owned test file, native/Unity paths, acceptance, protections and rollback were declared in the Task before script modification.

Risk: the logic-only replay may publish motion rows but no GPU commands; do not call a sampler PASS a visible-pixel PASS. Existing Q07 asserts and raw CSV remain preserved. No production, DAT, Scene or nonbattle code is authorized. Validation and final status will be appended after execution.

2026-09-28 first RED: generated Editor build 0 errors/187 warnings; original Editor exact filtered job `d0c5b9da93924e77b844e74b237b5ef4` failed at the new tick2 assertion because `PublishedFrame` was null. The existing strict Q07 test explicitly passed `buildPresentation:false` on every tick; the first failure is a fixture precondition, not a production P-02 defect. The Task Contract now declares enabling presentation on only the six pair ticks 1/2, 8/9 and 16/17 in this same test file before the next edit; no production path or DAT is touched. Preserve the failed job and retest only this case.

2026-09-28 final scoped result: the owned Q07 Editor test now requests presentation at only six declared pair ticks while preserving the same 24 full Driver logic ticks and formal rows. Actual adjacent weapon motion rows exist; pickup tick2 rejects interpolation because its relation changed, stable held X at tick9 and Z at tick17 sample with the expected native-rounded half-tick rule deltas and single D-024 view scale, and real World parity checksum is unchanged across each sample. Original Editor exact job `5b88d85aa8cb40d1a8a25b52d49d6368` passed 1/1; new 24-row Unity CSV still matches the selected formal source coordinates with 0/24 differences. Generated Editor build 0 errors/187 warnings; ordered shutdown zero-residual assertions, ledger/diff and three protected Asset SHA guards passed; Editor idle/non-Play. The diagnostic-only output proves no central held-weapon GPU pixel or formal-root same-view result. See same-ID `ACCEPTANCE.md`; this Change is `FOCUSED_TEST_PASS`, Q09/P-02 remains open.
