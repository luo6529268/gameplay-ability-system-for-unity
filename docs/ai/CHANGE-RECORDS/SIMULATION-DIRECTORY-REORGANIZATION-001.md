# SIMULATION-DIRECTORY-REORGANIZATION-001 — Simulation 目录物理分层

<!-- CHANGE-RECORD
id: SIMULATION-DIRECTORY-REORGANIZATION-001
status: BLOCKED
code-path: Assets/NTSD/Scripts/Simulation/Ai/AiCharacterDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/AiDecisionRandomStream.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/AiDecisionSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/AiSensingKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/AiSensingSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiCharacterDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionRandomStream.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiSensingKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/BattleAiExecutionProfile.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionTypes.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiInputModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiSensingModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiSensingTypes.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiDecisionSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiSensingSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleAiExecutionProfile.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleBloodPointCatalog.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleBloodPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleBodyBoxValue.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleBodyBoxValueAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleCatchPointCatalog.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleCatchPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleCatchPointValueAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleEarlyFrameAdvanceModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleFunctionKeyInputLatch.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleInteractionPipeline.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleLogicObjectPointRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleLogicReferencePool.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleManagedMemoryBoundary.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleMatchConfigRuntimeAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleObjectPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleObjectPointValueAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleOid5152RuntimeModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRandomWeaponDropModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeAllocationGate.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeDataCatalog.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeLifecycle.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeProfile.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleSimulationWorkerBoundary.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleStageCampaignLoader.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleStageCampaignValueAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleWeaponPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleWeaponPointValueAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/ISimObject.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/ISimTickable.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDGlobal.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDSpec.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimContext.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimOrderConstants.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationConstants.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationEntityTraversal.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationPassPipeline.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorldHooks.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorldMutationTracker.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/BloodPoint/BattleBloodPointCatalog.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/BloodPoint/BattleBloodPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/BodyBox/BattleBodyBoxValue.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/BodyBox/BattleBodyBoxValueAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/CatchPoint/BattleCatchPointCatalog.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/CatchPoint/BattleCatchPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/CatchPoint/BattleCatchPointValueAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/ObjectPoint/BattleObjectPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/ObjectPoint/BattleObjectPointValueAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/WeaponPoint/BattleWeaponPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/WeaponPoint/BattleWeaponPointValueAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/SimulationDiagnosticsModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/SimulationWorld.DetailTimingDiagnostics.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleAiInputWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleAiUnifiedRowPublisher.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleBoundaryWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleCharacterInputActionResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleCharacterInputStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleCharacterInputWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterFrameAdvancePass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterFrameTickPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterInputPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterPostFrameTailPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterPreFrameBoundsPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterRecoveryPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCharacterStageZPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsContainers.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsCooldownPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsFramePostProcessPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsPositiveLinkValidationPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsShadowModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleFrameMotionStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleFrameMotionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleIdentityStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleInteractionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleProductionOwnershipInventory.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleRelationLinkStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleRelationLinkWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleResultsOutcomeHostWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleResultsReserveHostWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleResultsWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleStructuralWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleVitalStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleVitalWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleAiUnifiedRowPublisher.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleCharacterInputActionResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsContainers.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsShadowModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleProductionOwnershipInventory.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameAdvancePass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameTickPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterInputPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterPostFrameTailPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterPreFrameBoundsPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterStageZPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCooldownPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsFramePostProcessPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsPositiveLinkValidationPass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsOutcomeHostWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsReserveHostWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Stores/BattleCharacterInputStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Stores/BattleFrameMotionStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Stores/BattleIdentityStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Stores/BattleRelationLinkStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Stores/BattleVitalStore.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleAiInputWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleBoundaryWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterInputWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleFrameMotionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleRelationLinkWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleStructuralWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleVitalWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/BattleSimulationWorkerBoundary.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickHostPolicy.cs
code-path: Assets/NTSD/Scripts/Simulation/ISimObject.cs
code-path: Assets/NTSD/Scripts/Simulation/ISimTickable.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/BattleFunctionKeyInputLatch.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/SimulationFrameInputModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleLockstepSession.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldCharacterShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldEntityBaseShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldLivingShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldPendingEventSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldRestSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldRosterResultsSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldRuntimeSlotSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldSpecialOtherShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldStageSpawnSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/BattleWorldWeaponShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/InProcessLockstepChecksumWitness.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/History/LockstepChecksumHistoryRing.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/History/LockstepFrameHistoryRing.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/History/LockstepSnapshotRing.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/InProcessBattleKernelHost.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/InProcessBattleWorldBootstrap.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/InProcessLockstepAuthoritySession.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/InProcessLockstepChecksumWitness.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepChecksumHistoryRing.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepFrameHistoryRing.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepFramePacket.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepReplayJournal.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepSessionIdentity.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepSnapshotRing.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepStartBarrier.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepSyncMessages.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Replay/LockstepReplayJournal.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/BattleLockstepSession.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/InProcessBattleKernelHost.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/InProcessBattleWorldBootstrap.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/InProcessLockstepAuthoritySession.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/LockstepFramePacket.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/LockstepSessionIdentity.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/LockstepStartBarrier.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/LockstepSyncMessages.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Session/StrictDelayedInputBuffer.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCharacterShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityBaseShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldLivingShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldPendingEventSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldRestSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldRosterResultsSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldRuntimeSlotSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldSpecialOtherShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldStageSpawnSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldWeaponShellSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/StrictDelayedInputBuffer.cs
code-path: Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/NTSDGlobal.cs
code-path: Assets/NTSD/Scripts/Simulation/NTSDSpec.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/EarlyFrameAdvance/BattleEarlyFrameAdvanceModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Interaction/BattleInteractionPipeline.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Interaction/SimulationQueryAndLinkModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/RandomWeapon/BattleRandomWeaponDropModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/RandomWeapon/SimulationRandomWeaponDropBuffer.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicObjectPointRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicReferencePool.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleManagedMemoryBoundary.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleMatchConfigRuntimeAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeAllocationGate.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeDataCatalog.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeLifecycle.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeProfile.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/RuntimeCharacterConfigResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/RuntimeSlotTable.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationBattleBufferModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationObjectBucketRegistry.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationRuntimeCapacityModule.cs
code-path: Assets/NTSD/Scripts/Simulation/RuntimeCharacterConfigResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/RuntimeSlotTable.cs
code-path: Assets/NTSD/Scripts/Simulation/SimContext.cs
code-path: Assets/NTSD/Scripts/Simulation/SimOrderConstants.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiDecisionTypes.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiInputModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiSensingModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiSensingTypes.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationBattleBufferModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationConstants.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationDiagnosticsModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationEntityTraversal.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationFrameInputModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationObjectBucketRegistry.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationPassPipeline.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationQueryAndLinkModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationRandomWeaponDropBuffer.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationRegistryModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationRuntimeCapacityModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationStageWaveModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationTickHostPolicy.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.DetailTimingDiagnostics.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorldHooks.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorldMutationTracker.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/BattleStageCampaignLoader.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/BattleStageCampaignValueAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageWaveModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/StageSpawnRuntimeBufferPool.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/StageSpawnTaskConfigurator.cs
code-path: Assets/NTSD/Scripts/Simulation/StageSpawnRuntimeBufferPool.cs
code-path: Assets/NTSD/Scripts/Simulation/StageSpawnTaskConfigurator.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleResultsOutcomeHostWriterSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleResultsReserveTerminalIntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleResultsReserveTransactionSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRosterLabelBootstrapSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRuntimeSelfCheckEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldScalarStateSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationWorldModuleArchitectureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationDirectoryReorganizationExecutor.cs
authority: USER-APPROVED-SIMULATION-DIRECTORY-REORGANIZATION-2026-09-02; Assets/NTSD/Docs/simulation-directory-reorganization-plan.md
evidence: MANIFEST_142_142; GUID_142_142; CONTENT_HASH_142_142; UNITY_COMPILE_0; PATH_MATRIX_18_PASS_2_KNOWN; RELATED_200_PASS_1_KNOWN; FULL_1585_EXECUTED_5_EXTERNAL; TWO_CLEAN_PLAY_STOP; GLOBAL_LEDGER_BLOCKED_EXTERNAL
-->

> 创建日期：2026-09-02
>
> 当前状态：\`BLOCKED / IMPLEMENTATION_COMPLETE / UNITY_COMPILE_0 / MANIFEST_142_142 / GUID_142_142 / FOCUSED_BASELINE_PRESERVED / TWO_CLEAN_PLAY_STOP / GLOBAL_LEDGER_BLOCKED_EXTERNAL\`

## 1. 用户要求

用户批准按照职责对 \`Assets/NTSD/Scripts/Simulation\` 进行物理目录重组。当前目录共有156个C#文件，其中根目录71个；本Change仅移动文件与对应meta，并更新依赖源码物理路径的Editor测试/工具。

## 2. 强制边界

- 不修改namespace、类型名、方法、字段、可见性或程序集边界。
- 不修改C++ release行为、30Hz、pass顺序、RNG、slot/generation、OPoint、worker、checksum、presentation或ordered shutdown。
- 不修改Scene、Prefab、DAT、URP、Input Actions、Server或Gen/Plugins。
- 所有移动以 \`docs/ai/MANIFESTS/SIMULATION-DIRECTORY-REORGANIZATION-001.csv\` 为唯一逐文件清单；反向清单即回滚方案。
- .cs与.meta必须一起移动并保持GUID；不得删除或重新生成已有meta。
- 发现未列出的硬编码路径、编译差异、GUID变化或测试first difference时立即停止。

## 3. 原状

- C#文件156；总行数74657。
- 根目录71个/38715行，Ai 6个，Ecs 37个，Input 5个，Lockstep 28个，Presentation 5个，Spatial 4个。
- 计划移动142个文件，Input/Presentation/Spatial中已合理放置的14个文件保持原位。
- 工作树已有任务外Docs、EditorBuildSettings和S0记录修改；本Change不得覆盖或清理。

## 4. 验收

1. manifest 142条source全部消失、destination全部存在，且每个meta GUID与移动前一致。
2. Simulation根目录不再平铺业务脚本；目录结构与计划一致。
3. namespace/type/API文本不因移动改变；除物理路径测试外脚本内容SHA-256保持一致。
4. Runtime与Editor compile均0 error；Unity Console无error CS。
5. architecture/source-path focused测试通过；AI、checksum、worker、shutdown达到移动前基线。
6. 完整EditMode与SelfCheck实际执行；任务外既有baseline单独记录。
7. Play/Stop后无cleanup warning，Scene dirty unchanged。
8. Change Ledger validator与scoped diff audit执行并如实记录。

## 5. 回滚

按manifest从destination反向移动回source，同时移动meta；恢复本Change更新的7个路径读取测试。禁止使用git reset/restore/clean。

## 6. 实际执行

### 6.1 R0 Preflight完成

- manifest冻结142条移动；`SIMULATION-DIRECTORY-REORGANIZATION-001-baseline.csv`
  已记录每个source的content SHA-256、meta GUID和meta SHA-256，142项均存在。
- 移动前runtime/editor `dotnet build --no-restore`均exit0、0 error。
- Unity MCP路径/架构基线job `92492d10084b44519a5a74a266c42655`
  完成20项；18项通过，只有任务前已存在的两项package version断言失败：
  `BattleRosterLabelBootstrap...StateAndClientAdapter...`与
  `BattleWorldScalarState...SelectedTypes...`均expected 0.6.0/actual 0.8.0。
- 字符串路径扫描确认7个Editor路径读取者需要在移动后精确更新；未发现runtime按物理
  C#路径加载类型。R1开始前未修改任何生产脚本内容。

### 6.2 R1根目录分层

- 按manifest移动根目录71个cs/meta；移动后立即做内容SHA、meta SHA与GUID审计，
  71/71无差异。
- 首次通过外部文件系统移动后，单独`manage_asset import` World触发Unity自动刷新
  竞态，只将World meta临时改为新GUID；按硬停止条件暂停R2，恢复冻结GUID后，Unity
  AssetDatabase `get_info`重新返回原GUID `87b29e29151e3ef4a87fd96e0b000f1c`。
  其余70项从未变化。R2改用`AssetDatabase.MoveAsset`事务，禁止重复外部移动方式。
- Unity Console `error CS`为0；architecture job
  `814cb1dc714949b2a485e6b38a65fe95`为4/4 PASS。
- Unity重新生成的csproj随后出现项目级.NET Framework 4.7.1引用netstandard2.1的
  `dotnet build`不兼容；Unity编译/Test Runner正常。该生成工程环境差异单独记录，
  不冒充Unity compile failure，也不修改ProjectSettings规避。

### 6.3 R2/R3宽目录分层与路径读取者

- 临时Editor迁移器通过`AssetDatabase.MoveAsset`事务移动剩余71个Ai/Ecs/Lockstep
  文件及meta；全部成功后已删除临时迁移器及其meta，未留下生产或Editor入口。
- 最终142条manifest审计：旧source存在0、destination缺失0、C#内容SHA差异0、
  meta GUID差异0。`SimulationWorld.cs.meta`因恢复冻结GUID时换行由LF变为CRLF，故
  byte-level meta SHA有且仅有1项变化；GUID与全部语义字段保持一致。
- Simulation根目录业务C#为0；顶层只保留13个责任目录：Ai、Core、DataContracts、
  Diagnostics、Ecs、Host、Input、Lockstep、Passes、Presentation、Runtime、Spatial、Stage。
- 仅更新7个依赖物理源码位置的Editor读取者，断言内容不变；最终扫描所有项目C#，
  manifest旧路径引用为0。

### 6.4 Unity编译、focused与完整EditMode

- Unity刷新后Console `error CS`为0；同一20项路径/架构矩阵job
  `adaaa01706c34edb9a285986fa209ff3`为18通过、2个既有package 0.6.0/0.8.0失败，
  与移动前job `92492d10084b44519a5a74a266c42655`完全一致。
- AI sensing/decision、worker、checksum、ordered shutdown、architecture组合job
  `9db729661b934bbe89dd17a6b367811a`执行201项：200通过；唯一失败为既有
  `DataOrientedProfile_MatchesLegacyFullDispatcherForPosition38`。
- 无MCP轮询干扰的完整EditMode job `c6848bc892c04e478f95c094899e3425`
  执行1585/1585，剩余5项任务外失败：position38、两个package版本断言、
  `BattleRuntimeStructureGuard`把两个既有只读`Empty`自动属性误判为mutable field、
  以及既有`RestartPolicy` expected5/actual1。
- 测试发现总数从移动前1763变为1585的178项差值已闭合到并行内容权威任务：
  旧`FormalContentClosureEditorTests`实际展开179项被删除，新
  `UnityPresentContentAuthorityEditorTests`新增1项；不是目录移动漏编译。
- 首次全量job `db7000f2673d400e8e83bc6fcebe328d`中的两个渲染压力测试失败由运行中
  MCP轮询产生的`NetworkStream disposed`错误日志污染；无轮询复跑时二者均通过，
  因而不作为项目失败证据。

### 6.5 SelfCheck与真实Play/Stop

- fresh请求结果`Temp/NTSD_BattleRuntimeSelfCheck.result`更新时间
  2026-09-02 10:53:27 +08:00；仍停在任务前相同的central-render P4
  “most recently registered renderer feature must own material and draw-mode selection”断言，
  未出现新的Simulation路径或类型失败。
- 对`Assets/NTSD/Scene/NTSD_Battle.unity`执行两轮真实Play→等待25秒→Stop，覆盖延迟
  角色生成。每轮退出后Scene均`isDirty=false`、rootCount=13，Console error/warning=0；
  无`Some objects were not cleaned up`、factory/pool auto-created或boundary carrier残留。

### 6.6 最终治理结论

- scoped diff确认生产C#仅发生路径移动，142/142内容哈希保持；7个Editor读取者只有
  明确的新物理路径变化，未修改断言语义。
- `Tools/Validate-ChangeLedger.ps1`已执行，但repository-wide gate仍因任务外
  `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001.md`缺少`code-path` metadata而exit1。
  本Change的脚本路径均由本Record覆盖；按照“不顺手修无关记录”的边界，保持
  `BLOCKED / IMPLEMENTATION_COMPLETE`，不冒充全局governance可交付。
