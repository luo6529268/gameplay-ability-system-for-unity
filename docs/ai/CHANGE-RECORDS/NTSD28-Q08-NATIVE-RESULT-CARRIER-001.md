<!-- CHANGE-RECORD
id: NTSD28-Q08-NATIVE-RESULT-CARRIER-001
status: PLANNED
change-kind: Q08_NATIVE_RESULT_FLOW_CARRIER_AND_PRECOMBAT_PRODUCER
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsOutcomeHostWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldRosterResultsSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleFlowRedProbeEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleStateSnapshotRestoreEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs
authority: formal Logan GameSession28 precombat BattleFlow28 and source-matched full-driver group/timer fixture; Q08 result-group carrier audit
evidence: prechange native double-run and isolated Unity group/timing RED; production implementation and validation pending
-->

# NTSD28-Q08-NATIVE-RESULT-CARRIER-001

Task: `docs/ai/TASKS/NTSD28-Q08-NATIVE-RESULT-CARRIER-001.md`. Created before any implementation script edit.

Before: the current Unity result producer runs after post-frame combat, counts only two persistent UI buckets with a `HadBoth` gate, can stop the timer after a second group returns, and activates the page at phase11. Formal playable classifies 1..39 excluding5 before combat, starts on first terminal tick and latches through revival; timer milestones are 80/101/350. Existing Results state is serialized in roster schema1/aggregate25/checksum28, so adding a carrier without persistence would break replay and deterministic identity. The current menu has no formal paired story selector; do not infer one from mode1 or `StageProgressionValid`.

Planned changes: introduce independent source-derived native result fields and a precombat producer while retaining the existing result-page/UI state and mode4 reserve path; capture/restore/hash/parity-project the native fields with deliberate schema bumps and canonical restore preflight. Source classification and timer semantics are implemented as one transaction across the exact listed paths. Existing old UI timing remains a known parent Q08 mismatch until its consumer package is implemented; do not call this whole result flow aligned.

Expected side effects: snapshot/checksum schema change and new precombat result state; no Unity/GAS, Scene, resources or nonbattle change. Acceptance and rollback are as in the Task. Record actual paths/symbols, RED->PASS, compiler, SelfCheck/Play and limits immediately after edits. No script has been changed yet under this ID.

Pre-test ownership expansion: the existing `BattleStateSnapshotRestoreEditorTests.cs` is added before editing that file, only for one native result carrier capture/restore/checksum and invalid-mask no-mutation case. The original fixed scope omitted the aggregate restore fixture; no production path is added by this expansion. The preceding sentence saying no script was changed is the original prechange state; native carrier, producer, persistence and focused behavior code has since been written, with test and runtime acceptance pending.
