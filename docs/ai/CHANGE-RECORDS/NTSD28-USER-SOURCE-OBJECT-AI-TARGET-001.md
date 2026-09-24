<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-OBJECT-AI-TARGET-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_OBJECT_AI_TARGET_RANKING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B0ObjectAiTarget3F8DeconflictionEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable NativeAi28 step_non_character_hit_fa; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-OBJECT-AI-TARGET-001.md
-->

# NTSD28-USER-SOURCE-OBJECT-AI-TARGET-001

Created before script edits. Formal noncharacter hit_Fa target scan ranks source integer X/Z; Unity frame-logic scan ranks scaled physical X/Z. This package owns one divergent witness and the source-complete ranking correction. Record focused outcome and limits after implementation.

2026-09-24 result: original-project Editor RED job `88b377f5ec1f4460871e43ca02d9e2c2` selected physical winner slot1 instead of formal source winner slot0. `LF2Entity.ResolveObjectAiTargetForFrameLogic` now tracks physical and source Manhattan rankings independently across eligible candidates and chooses source only when every eligible source history is initialized; otherwise physical fallback remains. Original Editor GREEN job `3284239b13a846ba8ca7a618af18a6c4` passed B0 8/8; after adding incomplete-history fallback witness, final job `c87c40594fa942c1b530668e2c2405c0` passed 9/9. Existing candidate filter, strict comparator, stale target and velocity writes are untouched. Full Driver, natural Battle Play, formal EXE and broader character AI snapshot parity remain pending. No DAT/Scene/nonbattle change.
