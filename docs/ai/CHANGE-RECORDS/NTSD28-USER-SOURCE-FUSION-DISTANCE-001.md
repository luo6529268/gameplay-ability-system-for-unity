<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-FUSION-DISTANCE-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_FUSION_DISTANCE_MERGE_SPLIT
code-path: Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionRecordTransactionEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 advance_native_fusions; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-FUSION-DISTANCE-001.md
-->

# NTSD28-USER-SOURCE-FUSION-DISTANCE-001

Created before script edits. Formal fusion gate uses source integer X/Z, but current Unity consumes view-scaled physical integers and can reject a formally valid gap40 as physical gap50. This package changes the source-complete gate, source midpoint and split source copy while retaining physical midpoint and incomplete-source fallback. Record actual focused validation and limits after implementation.

2026-09-24 result: source-complete fusion uses source integer X/Z for native proximity and slot-order gate, writes independent source integer midpoint, and copies primary initialized source position to partner on split. Physical midpoint and incomplete-source fallback remain. Original Editor X witness job `14f7a1c593c44e0ba42e95056423f8e6` 2/2, adjacent four-record job `de2021edfa3f435697f1e9c6f46055b2` 4/4, and source-Z/physical-Z divergence job `804c1b04d9324929bd26a61f3d8fd376` 1/1 all passed. Source X gap40 now merges even when physical X gap50; source Z gap7 merges despite physical Z gap8; dual midpoints and split copy were asserted. Full Driver/Scene Play/formal EXE and broader source-writer completeness remain pending. No DAT/Scene/nonbattle change.
