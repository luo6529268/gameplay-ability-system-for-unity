<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-GRAB-RELATION-POSITION-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_GRAB_RELATION_POSITION_WRITER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchRelationExactFieldsProductionEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 kind1/kind3 paired grab branch; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-GRAB-RELATION-POSITION-001.md
-->

# NTSD28-USER-SOURCE-GRAB-RELATION-POSITION-001

Created before script edits. The active shared grab relation writer currently mutates physical positions only, leaving initialized source-rule positions stale. This package adds source-only paired X placement from native integer anchor and raw DAT-local CPoint offsets, including the native immediate integer mirror. Physical positions, current facing decision, eligibility, Y and relation fields remain unchanged. Record exact validation and limits after implementation.

2026-09-24 result: `BattleInteractionWriter.AlignGrabPair` now computes source-rule pair placement independently from source integer X for both kind1 and kind3 when both carriers are initialized. Native raw-local offsets and half-gap are reused without view scaling; both source integer mirrors update immediately as the formal relation does. Source-incomplete input leaves the carrier untouched. Original-project Unity Editor reimported both declared scripts and final focused NUnit job `aabd274d553d46bda0c9b354e41642aa` passed 23/23 with 0 failures, including existing relation and no-allocation tests. Initial focused job `62841368f5164c83ab8c4ad198e983b9` passed 21/21 before two explicit boundary cases were added. This proves only this writer's focused contract; full Driver/Play/EXE, source-domain facing/eligibility and other CPoint/WPoint writers remain open. No DAT/Scene/ProjectSettings/nonbattle file was edited.
