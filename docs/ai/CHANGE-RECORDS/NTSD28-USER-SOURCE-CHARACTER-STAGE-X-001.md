<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-CHARACTER-STAGE-X-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_CHARACTER_STAGE_X
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterPreFrameBoundsPass.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SourceCharacterStageXEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28::settle_ordinary_stage_bounds; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-CHARACTER-STAGE-X-001.md
-->

# NTSD28-USER-SOURCE-CHARACTER-STAGE-X-001

Created before script edits. Existing exact/compatibility character PreFrame outlets clamp only physical X. Planned source-only type0 bounds use the same formal branch conditions on independent source X and update its integer mirror. This does not alter noncharacter physical-edge destruction or activate source gameplay readers. Results will be appended after implementation.

2026-09-24 result: `NTSDEntityRuntime.ClampSourceRuleCharacterX` applies the formal type0 slot20, relation-team5, base-stage width, phase override and hit-stop branches to initialized source X, then truncates its source integer mirror. Exact character PreFrame and compatibility character PreFrame call it after their existing physical X writers. Original Editor new source character class job `678a9a0773a64f81adc835ce47c9415c` 11/11 PASS (configured edge split, Legacy/DataOriented, seven branch cases, derived fallback, absent carrier); adjacent existing `BattleEcsCharacterPreFrameBoundsPassEditorTests` job `904c5d0e891a44a9b35ba07bc4ae2897` 6/6 PASS including warmed no-allocation. Status `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`; noncharacter stage-edge destruction semantics, full Driver/Play/EXE and source gameplay readers remain open.
