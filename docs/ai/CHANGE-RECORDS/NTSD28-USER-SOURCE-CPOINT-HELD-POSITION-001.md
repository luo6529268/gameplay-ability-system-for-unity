<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-CPOINT-HELD-POSITION-001
status: IN_PROGRESS
change-kind: D024_SOURCE_RULE_CPOINT_HELD_POSITION_WRITER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointSettlementRemainderEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 settle_catch_relations; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-CPOINT-HELD-POSITION-001.md
-->

# NTSD28-USER-SOURCE-CPOINT-HELD-POSITION-001

Created before script edits. The held CPoint writer presently updates physical X/Z only; source-rule carrier will gain independent X/Z placement from catcher source integers and raw local offsets, with immediate integer mirror as in native settlement. Physical output remains unchanged in this package. Nonzero CPoint Z physical behavior is a separately confirmed gap and is not implicitly fixed here. Fill actual validation and limits after implementation.
