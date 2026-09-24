<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-STAGE-DEPTH-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_STAGE_DEPTH_WRITER
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterStageZPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SourceStageDepthEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28::clamp_type0_stage_depth; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-STAGE-DEPTH-001.md
-->

# NTSD28-USER-SOURCE-STAGE-DEPTH-001

Created before script edits. The current three reachable Unity depth-clamp outlets update physical Z only. This package adds an independent source-rule precise/int Z clamp under the formal type-specific limits without changing physical view scaling, stage data or gameplay source readers. Actual diff and focused results will be appended after implementation.

2026-09-24 result: `NTSDEntityRuntime.ClampSourceRuleZ` clamps an initialized independent source precise Z and truncates its integer mirror, doing nothing for absent carriers. The default DataOriented StageZ writer, legacy/ShadowCompare stage module, and later `LF2Entity.ApplyPreFrameZBounds` call it with type0 margin0 and non-type0 margin1. Existing physical Z, stage data and display are unchanged. Original Editor exact new class job `2ee603505bf54356aad5e906640d1740` ran 6/6 PASS across three pass modes, configured view, both object types, near/far edges, fallback and absent carrier. Adjacent existing `BattleEcsCharacterStageZPassEditorTests` job `efd66f1a81464b969455f09f5eed5621` ran 9/9 PASS including ShadowCompare, all-active noncharacters and the warmed 1000-entity zero-allocation check. Status `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`; source-rule stage X, full Driver/Play/EXE and source gameplay readers remain pending.
