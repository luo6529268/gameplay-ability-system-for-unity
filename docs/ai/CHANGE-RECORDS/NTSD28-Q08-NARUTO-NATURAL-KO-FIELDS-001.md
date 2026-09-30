<!-- CHANGE-RECORD
id: NTSD28-Q08-NARUTO-NATURAL-KO-FIELDS-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07KnockoutFeedProductionRowEditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 tick8 knockout trace and paired playable battle_world.cpp record_native_knockout
evidence: docs/ai/TASKS/NTSD28-Q08-NARUTO-NATURAL-KO-FIELDS-001.md
-->

# NTSD28-Q08-NARUTO-NATURAL-KO-FIELDS-001

Before script edit: the existing 38-tick original-Editor Naruto production-row test proves the natural KO count/time and mode row lifetime, but not the event's source/type/owner fields. Formal root tick8 supplies exact fields; existing direct B5 tests do not substitute for a natural full Driver route. This Change is test-only and limited to one existing method. The Task records expected assertions, protected state, validation and rollback. Production behavior, old assets and nonbattle code remain untouched.

2026-09-29 code written: in `NarutoPunchPublishesKoFeedThroughNativeThirtyTickBoundary`, at the already observed tick8, assert one event plus battle time7, source type0, four owner0, victim1, source0 and credit0. The existing CSV and all remaining 38-tick/lifetime/shutdown checks are unchanged. Original Editor compile and focused run pending.

2026-09-29 focused result: formal root trace tick8 knockout fields were independently extracted to `artifacts/diagnostics/NTSD28-Q08-NARUTO-NATURAL-KO-FIELDS-001/formal-tick8-knockout-fields.json`, source trace SHA confirmed. Original Editor refresh/reload compiled the test; exact job `498abdee1c6540a8b912809fab79d9cc` passed 1/1 with six field assertions. The original 38-row CSV SHA remained unchanged, as did Battle/Menu/GameConfig/ProjectBattleModeConfig asset hashes. Editor returned idle/non-Play. No production edit. Limits and raw job are in this package's `ACCEPTANCE.md`; full Q08 gates remain pending.
