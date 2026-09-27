<!-- CHANGE-RECORD
id: NTSD28-Q08-SELECTED-ARMOR-KO-FOCUSED-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorAtomicProductionIntegrationEditorTests.cs
authority: paired playable battle_world.cpp selected-armor pre-subtraction KO gate and BATCH-04 Q08
evidence: docs/ai/TASKS/NTSD28-Q08-SELECTED-ARMOR-KO-FOCUSED-001.md
-->

# NTSD28-Q08-SELECTED-ARMOR-KO-FOCUSED-001

Before edit: the existing type-1 armor production test proves injury20 -> damage10 and armor-HP20 consumption, but does not assert the KO event on lethal selected armor. The separate standard/reduced KO test's reduced call omits `selectedArmor`. Formal selected-armor gate checks a positive target HP, ordinary credit gate -1, valid attribution, and effective reduced HP damage at least the pre-hit HP, then records the event before HP subtraction. OID87 shipped content has a root-EXE witness for the selected route, but no lethal trace.

Declared edit: add a single positive/negative focused test in the exact `code-path` above; reuse existing fixture helpers, avoid production/DAT/Scene/nonbattle changes. Expected side effects are only new test assertions and Editor test output. Invariants: no duplicate KO credit, no nonlethal event, unchanged type-1 armor HP/MP damage. Acceptance and rollback are in the Task Contract. Validation pending.

After edit: added two parameterized cases to the existing type-1 armor production test class. Both call `ApplyStandardCharacterDamage` with the same available HP-armor record and injury20, starting target HP10/11, explicit ordinary credit gate -1 and native event sequence7. Assertions cover actual damage10, armor-HP consumption20, exactly one/zero credited knockout and event, and the positive event's time/source/type/credit/four-owner/victim fields. No production or resource script was changed.

Validation: original Editor refresh/compile reached idle/non-Play, its Editor assembly was newer than the source, and exact selected method job `b0a81d840ffd41eba573085700383377` passed 2/2 with zero failures/skips. Raw JSON SHA-256 `AE90C8B188EADD9DBF2B525F264D7BEA55BAA8ADBFCFBB995448F87B0EDB4A6A`. Ledger 915 Records/this path covered, `git diff --check` exit0; protected Battle/Menu Scene and mode Asset absent from Git status. Detailed proof and limits: `artifacts/diagnostics/NTSD28-Q08-SELECTED-ARMOR-KO-FOCUSED-001/ACCEPTANCE.md`. This Change closes only synthetic selected-armor KO threshold verification; formal root EXE lethal same-state full-tick, natural Play, Q08/BATCH-04 and total alignment remain open.
