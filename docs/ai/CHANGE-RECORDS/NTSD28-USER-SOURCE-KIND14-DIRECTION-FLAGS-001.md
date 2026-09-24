<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-KIND14-DIRECTION-FLAGS-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_KIND14_DIRECTION_FLAGS
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleBoundaryWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SourceKind14FlagsEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable BattleWorld28 kind14 and PhysicsIntegrator28; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-KIND14-DIRECTION-FLAGS-001.md
-->

# NTSD28-USER-SOURCE-KIND14-DIRECTION-FLAGS-001

Created before script edits. Central and fallback kind14 paths currently publish only physical-view flags from physical integer positions. The planned source-only publication compares source integer X/Z under the formal strict thresholds and existing velocity/knockback gates. It leaves physical flags and ECS prediction unchanged; no gameplay source readers are activated. Tests and actual changes will be appended after implementation.

2026-09-24 result: common `BattleBoundaryWriter.TryApplyKind14DirectionalBlock` now calls an independent source-rule integer-domain publisher under both-positions-initialized gate. It compares strict ±5/±2 and existing Vx/Vz or knockback sign, writing only source-rule flags; physical flags and `BattleCharacterInputWriter` stay unchanged. The four unregistered paths (`LF2Entity`, character DAT, special attack, character hit) call the same helper after their existing physical flag logic. Source physics consumes and clears those flags on the following integration. Original-project Editor exact new class job `82b4966cfca84beeb9c6548fe66e8fbc` ran 7/7 PASS, including strict thresholds, signs, central path, no-source gate, physics consumption and unregistered entity fallback. Adjacent exact `BattleHitExecutionPlanEditorTests` two methods job `261f76c79ac94017a50cd69c8bf3ac42` ran 2/2 PASS. Two mistaken `NTSD.Test.Editor` filters executed zero tests and are not evidence. Full Driver/Play/formal EXE and active source-rule gameplay readers remain pending. Status `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`.
