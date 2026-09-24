<!-- CHANGE-RECORD
id: NTSD28-USER-C07-REVIVAL-PHASE-ASSERTION-ORDER-001
status: FOCUSED_TEST_PASS
change-kind: C07_TEST_ONLY_PHASE_DIAGNOSTIC_INDEX_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
authority: current production NTSDBattleTickSystem normal-path FrameAdvance then Stage; original Editor focused job 9155fd12b743482a85a9dcbe5976f3ad
evidence: docs/ai/TASKS/NTSD28-USER-C07-REVIVAL-PHASE-ASSERTION-ORDER-001.md
-->

# NTSD28-USER-C07-REVIVAL-PHASE-ASSERTION-ORDER-001

2026-09-24 final test-only result: the second focused RED job `1fc28ca022414a84b6bb6f1eac80d6d1` showed all preceding phase assertions passed and only old exact total34 differed from actual33. The test now asserts FrameAdvance slot27, Stage slot28 and total33. Original-project Editor recompiled after the final edit; exact single-method job `facf5cef78ec400ca6684e7e4a26eb5f` passed 1/1 and final C07 class job `6f5089efa8e54b7a98cb594ba2d382c2` passed 7/7. No production code changed under this ID. This test-only correction does not verify Battle Scene or formal EXE.

2026-09-24 actual first edit: moved the terminal `FrameAdvance` assertion from slot28 to slot27 and explicitly asserted `Stage` at slot28. Original Editor recompiled `Assembly-CSharp-Editor.dll` at 10:36:21 and focused job `1fc28ca022414a84b6bb6f1eac80d6d1` failed only the subsequent exact count: expected34, observed33. All preceding phase assertions passed. The same declared test method must update its stale total-count guard to 33 and rerun; no production change is authorized by this record.

Pre-script record: old C07 phase fixture expects sequence slot 28 `FrameAdvance`; actual original-project Editor result is `Stage`, while the current normal-path production source places `FrameAdvance` immediately before `Stage`. Change only the test's terminal phase index pair to 27/28, keep phase count and prior checks. Do not alter production or use this test-only change to claim formal EXE parity. Validation and rollback are in the Task.
