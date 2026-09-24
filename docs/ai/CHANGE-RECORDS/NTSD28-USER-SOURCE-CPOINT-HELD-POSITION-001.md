<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-CPOINT-HELD-POSITION-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_CPOINT_HELD_POSITION_WRITER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointSettlementRemainderEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 settle_catch_relations; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-CPOINT-HELD-POSITION-001.md
-->

# NTSD28-USER-SOURCE-CPOINT-HELD-POSITION-001

Created before script edits. The held CPoint writer presently updates physical X/Z only; source-rule carrier will gain independent X/Z placement from catcher source integers and raw local offsets, with immediate integer mirror as in native settlement. Physical output remains unchanged in this package. Nonzero CPoint Z physical behavior is a separate static branch difference of unproven official-content reachability and is not implicitly fixed here. Fill actual validation and limits after implementation.

2026-09-24 result: source-only X/Z writes were added to `BattleCpointWriter.SyncHeldPosition`, with catcher source integer anchor, raw DAT-local CPoint/center offsets, pre-cover caught facing and immediate source integer mirror. Physical X/Y/Z and direction output remain unchanged. Source-incomplete cases skip the new writes. `NTSD28Q06CpointSettlementRemainderEditorTests` now asserts formal row1 source X373/Z249 under both factor1 and configured full view while physical X differs 373/398. Original Editor script reimport and focused class job `71cfcf5d9b404de4b8e53e122acc5443` passed 7/7, 0 failed/skipped. Full Driver/Play/EXE and official-content reachability of the static nonzero-CPoint-Z physical branch difference remain open. No DAT/Scene/ProjectSettings/nonbattle file was edited.
