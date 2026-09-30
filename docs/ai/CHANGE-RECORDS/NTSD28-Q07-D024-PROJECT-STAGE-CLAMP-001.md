<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-PROJECT-STAGE-CLAMP-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterStageZPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterPreFrameBoundsPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/BattleSimulationWorkerBoundary.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28SourceStageDepthEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleStateSnapshotRestoreEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and user D-024 project proportional fixed-view exception
evidence: docs/ai/TASKS/NTSD28-Q07-D024-PROJECT-STAGE-CLAMP-001.md
-->

# NTSD28-Q07-D024-PROJECT-STAGE-CLAMP-001

Status: `VERIFIED / SCOPED_STAGE_CLAMP_ONLY`.

Before: map physical stage Z237..760 is fed to source-rule Stage-Z and PreFrame clamp, confirmed by two original Battle first-tick RED reports. Explicit source bounds remain the formal fixture control. Current source/physical conversion is already centralized in `BattleSpatialProjection`, but stage-origin identity is absent from worker, snapshot and checksum.

Plan, effects, risks, rollback and acceptance: [Task](../TASKS/NTSD28-Q07-D024-PROJECT-STAGE-CLAMP-001.md). Preserve physical map/polygon and source explicit fixture; use the existing projection, no new ratio. A mistaken origin default or integer rounding can change the rule; run near/far and explicit-source tests plus worker/restore before claiming completion. Roll back only this change's lines, never restore dirty files wholesale.

Actual edits: `SimulationWorld` now identifies project physical stage bounds and derives source-rule limits through its existing one `BattleSpatialProjection`. Scene map snapshot sets the physical origin, explicit test snapshot sets source origin, and Stage-Z DataOriented/Legacy/Shadow plus PreFrame exact/fallback consume the same derived limits with source margin 0/1 and physical fallback for absent source history. `NTSDEntityRuntime` has a four-bound overload so source correction and physical fallback never mix domains; old three-argument callers retain old semantics. Worker request, lockstep core snapshot/restore and checksum carry the origin bit. The local core/aggregate/checksum schema versions move 15→16/31→32/35→36. Tests cover both mapped edges, noncharacter margin, worker carrier, snapshot restoration and version rejection. No Scene, map, camera, DAT, image, GameConfig, formal source or nonbattle script was edited for this package.

Validation: generated Editor C# build 0 errors; original Unity Editor Tundra build success and 0 C# errors. Stage-depth group final 16/16, worker group 20/20, full snapshot restore 1/1 and previous aggregate-version rejection 1/1. Original Battle Scene Play near/far source Z100/600 first-tick physical Z237/760, ordered stop/borrowers0, five protected hashes unchanged. Targeted `git diff --check` and Change Ledger validator exit0. The first generated build had one missing `world.` qualifier and was fixed; the first worker class filter selected zero cases, then correct namespace passed20/20. [Full evidence](../../../artifacts/diagnostics/NTSD28-Q07-D024-PROJECT-STAGE-EDGE-PLAY-001/GREEN-ACCEPTANCE-20260929.md).

Remaining risk and dependency: this package does not change AI source-depth reads, stage-wave/results reserve or state405 center writers; no-map GameConfig authoring units remain separate. Q07/BATCH-04 and the total battle-alignment goal remain open. Rollback is the inverse of this package's exact diff, preserving pre-existing dirty work.
