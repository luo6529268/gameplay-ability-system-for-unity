<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-KIND8-RELATION-POSITION-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_KIND8_POSITION_WRITER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleKind8ControlRelationWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind8AtomicProductionIntegrationEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 kind8 precise position copy; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-KIND8-RELATION-POSITION-001.md
-->

# NTSD28-USER-SOURCE-KIND8-RELATION-POSITION-001

Created before script edits. The active canonical kind8 writer copies physical precise X/Z without updating source-rule precise X/Z. This package adds only the matching independent source precise writes under initialized-carrier gate; source integer mirrors remain unchanged until the native physics synchronization boundary. Physical output, eligibility, action/MP and legacy direct code are outside this bounded edit. Actual tests and limits will be recorded after implementation.

2026-09-24 result: `BattleKind8ControlRelationWriter.TryApply` now copies target `SourceRuleX` for normalized sync modes other than 1 and copies target `SourceRuleZ+1` for all modes other than -1, only when both source carriers are initialized. It does not touch source integer mirrors; physical precise position, Y, action, MP, eligibility and the legacy direct branch stay unchanged. The existing original-Editor production integration class gained divergent source/physical anchors for all six dvy normalization cases and a following physics integer-sync case. First pre-combined run `442e17afd6b34121a2c7bcb03b50feae` executed 14/14 PASS; final after adding the combined physics case, job `63afd00f992d418d90ab7b46439d7bb4` executed 15/15 PASS. This is `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: legacy direct resolver reachability, other relation writers, full Driver/Play/EXE and source-domain decision readers remain open.
