<!-- CHANGE-RECORD
id: NTSD28-USER-FUSION-VIEW-DISTANCE-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_FUSION_DISTANCE_CHARACTERIZATION_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionRecordTransactionEditorTests.cs
authority: shipped Logan playable BattleWorld28::advance_native_fusions and formal fusion.dat; user D-024 fixed-view battle-distance exception
evidence: docs/ai/TASKS/NTSD28-USER-FUSION-VIEW-DISTANCE-WITNESS-001.md
-->

# NTSD28-USER-FUSION-VIEW-DISTANCE-WITNESS-001

Status: `FOCUSED_TEST_PASS / FIRST_DIFFERENCE_CONFIRMED`. Exact test-only scope and acceptance are in the linked Task. Pre-change Unity behavior: the active fusion module consumes current battle integer positions for the unchanged X<50/Z<8 gate; factor1 formal-source tests existed, but the configured fixed-view history case had no original Editor witness. No production rule, DAT, Scene, camera or nonbattle code change is authorized under this Record. No irreversible operation. Rollback: reverse the one test method added under this ID, without touching other work.

Actual script change: added `FusionDistanceAfterTwentyPixelMotion_CharacterizesFixedViewFirstDifference` to `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionRecordTransactionEditorTests.cs`. It reuses source row 0, exact formal fusion input, production character movement and production World fusion scan. Its two parameters explicitly assert factor1 gap40 merges, configured gap50 rejects, and both test Worlds release their reference-pool borrowers. Original Editor focused job `42504562540d48f29a0f42433c24eced`: 2 total, 2 passed, 0 failed, 0 skipped. The configured pass characterizes a **remaining gameplay parity defect**. No production correction, Battle Scene Play, formal EXE rendered witness or broader fusion/aspect test was run in this package. The report is `artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/FUSION-VIEW-DISTANCE-FIRST-DIFFERENCE.md`.
