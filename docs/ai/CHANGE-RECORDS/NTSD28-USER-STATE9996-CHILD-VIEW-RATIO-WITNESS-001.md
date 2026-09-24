<!-- CHANGE-RECORD
id: NTSD28-USER-STATE9996-CHILD-VIEW-RATIO-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_STATE9996_CHILD_RELATIVE_BIRTH_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State9996DirectSpawnEditorTests.cs
authority: shipped Logan playable BattleWorld28::advance_native_special_clones and source-final row0; user D-024 fixed-view battle-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-STATE9996-CHILD-VIEW-RATIO-WITNESS-001.md
-->

# NTSD28-USER-STATE9996-CHILD-VIEW-RATIO-WITNESS-001

Status: `FOCUSED_TEST_PASS / FIRST_DIFFERENCE_CONFIRMED`. Task contains exact authority, current Unity behavior, declared one-test path, expected side effects, acceptance and rollback. Only a focused characterization test was authorized here; production script, DAT, camera, Scene and nonbattle framework are out of scope. Actual edit: one parameterized `State9996ChildAfterParentMotion_CharacterizesFixedViewBirthOffset` method in `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State9996DirectSpawnEditorTests.cs`. It reuses formal source-final row0, production parent movement, actual native clone birth and shutdown checks. Original Editor job `f2fbd17c1a1548b480ce8166db78f62f` finished 2/2 PASS, confirming factor1 relative -3/1333 but configured relative -3/2048. This passing characterization is a **remaining D-024 parity defect**. Production fix, Scene Play, formal EXE view and broader aspects are not covered. Report: `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/STATE9996-CHILD-VIEW-FIRST-DIFFERENCE.md`. Rollback remains removal of this test method only, preserving other work.
