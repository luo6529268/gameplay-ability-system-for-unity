<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-COORDINATE-PHYSICS-001
status: FOCUSED_TEST_PASS
change-kind: D024_SOURCE_RULE_PHYSICS_WRITER
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SourceCoordinatePhysicsEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4IdentityXExtrasEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable PhysicsIntegrator28::step; user D-024
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-COORDINATE-PHYSICS-001.md
-->

# NTSD28-USER-SOURCE-COORDINATE-PHYSICS-001

Created before script edits. The existing Unity physics moves physical X/Z by the approved view factor, but leaves the independent source-rule coordinate at its earlier value. The planned source writer integrates raw Vx/Vz subject to independent source directional flags, consumes those flags, applies the already-separated identity X and type-3 Z extras, and truncates to native integer mirrors after motion. Physical output, DAT fields, velocity/friction, scene and nonbattle flows must remain unchanged. Source-rule gameplay readers remain inactive until the remaining writer history and first-difference gates close. Validation and actual edits will be appended after implementation.

2026-09-24 result: all three existing shared physics entrypoints now advance initialized source-rule precise X/Z by the raw pre-friction Vx/Vz, use independent directional source flags, consume them, and truncate their source integer mirrors. The two already-separated identity X and type-3 hit_j Z extra writers also advance the raw source carrier while leaving scaled physical output unchanged. Uninitialized source carriers stay absent. A new focused original-Editor class passed 7/7 (job `4e1be8c363ec481091d531816ee02dbc`); the existing weapon identity/type-3 matrix with added source assertions passed 12/12 (job `67b096e873c94be793ffca1ff325a424`); adjacent fixed-view ratio class passed 11/11 (job `5669e1b69a214f6f89be8cad2f7d19fc`). The first filter request before asset import executed **zero** tests and is not evidence. `Tools/Validate-ChangeLedger.ps1` passed with 755 Records; `git diff --check` passed. Original Battle Scene SHA stayed `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`; no DAT/Scene/Content/ProjectSettings status changes. This is `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`, not live source-rule parity: kind14 source flag producers, teleport/stage/relation source writers, full Driver/Play/EXE and OID219/fusion readers remain open. The adjacent ratio class includes known-defect characterization cases and its PASS must not be read as D-024 closure.
