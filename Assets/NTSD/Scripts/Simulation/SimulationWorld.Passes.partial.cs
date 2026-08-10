using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 的正式版战斗 pass 执行入口。
    /// </summary>
    public partial class SimulationWorld
    {
        internal int LastCollisionPairVRestEligibilityVisitCount { get; private set; }

        public bool ForceLegacyPreInteractionForDiagnostics { get; set; }
        public bool ForceFullCharacterInputPostRefreshForDiagnostics { get; set; }
        public int LastPreInteractionScannedCountForDiagnostics { get; private set; }
        public int LastPreInteractionExecutedCountForDiagnostics { get; private set; }
        public int LastPreInteractionProofSkipCountForDiagnostics { get; private set; }
        public int LastPreInteractionSnapshotSkipCountForDiagnostics { get; private set; }
        public int LastPreInteractionFailClosedCountForDiagnostics { get; private set; }
        public int LastPreInteractionCpointCheckProofSkipCountForDiagnostics { get; private set; }
        public int LastPreInteractionMismatchTailProofSkipCountForDiagnostics { get; private set; }
        public bool LastPreInteractionWholePassProofSucceededForDiagnostics { get; private set; }
        public int LastPreInteractionWholePassParticipantCountForDiagnostics { get; private set; }
        private readonly List<RuntimeEntityHandle> earlyState500Handles =
            new List<RuntimeEntityHandle>(16);
        private readonly List<RuntimeEntityHandle> earlyState501Handles =
            new List<RuntimeEntityHandle>(16);
        public bool ForceLegacyEarlyFrameAdvanceForDiagnostics { get; set; }
        public int LastEarlyTeleportRefreshCountForDiagnostics { get; private set; }
        public int LastEarlyTeleportSnapshotSkipCountForDiagnostics { get; private set; }
        public bool LastEarlyStateHandlePathUsedForDiagnostics { get; private set; }
        public int LastEarlyStateHandleFallbackCountForDiagnostics { get; private set; }

        public void PostCooldownInputAll(int tickIndex)
        {
            PostCooldownHumanInputAll(tickIndex);
            CharacterInputAll(tickIndex);
        }

        public void FlushQueuedObjectPointTasks()
        {
            LF2ObjectPointFactory.Instance?.FlushTasks();
        }

        public void PostCooldownHumanInputAll(int tickIndex)
        {
            RefreshActiveHumanRosterInputBindings();
            using (BeginDeferredMutationEntityPass())
            {
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    if (!IsBoundActiveHumanRosterInputEntity(entity) ||
                        !entity.TryGetSharedInputControllerForSimulation(out _))
                    {
                        continue;
                    }

                    entity.RunHumanInputPollPhase(tickIndex);
                    if (IsActiveForCurrentPass(entity))
                        RefreshRuntimeSnapshot(entity);
                }
            }
        }

        public void ClearBattleEntryInputAll()
        {
            using (BeginDeferredMutationEntityPass())
            {
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    if (entity.GetCurrentDataObjectTypeForSimulation() !=
                        (int)LF2ObjectType.Character)
                    {
                        continue;
                    }

                    entity.ClearBattleEntryInputState();
                    if (IsActiveForCurrentPass(entity))
                        RefreshRuntimeSnapshot(entity);
                }
            }
        }

        public void AiInputAndComboAll(int tickIndex)
        {
            if (tickIndex <= 1)
                return;

            EnsureAiSensingModeAvailableBeforeTick();
            BuildAiInputSlotSnapshot();
            if (AiDecisionRequiresSharedRows &&
                !AiUnifiedSnapshotExecutionOwnsCurrentPass)
                PrepareAiDecisionSharedPass();
            CompleteAiUnifiedSnapshotShadowInitialComparison();
            try
            {
                using (BeginDeferredMutationEntityPass())
                {
                    foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                    {
                        if (!entity.AiControlled ||
                            entity.GetCurrentDataObjectTypeForSimulation() != 0)
                        {
                            continue;
                        }

                        BeginAiUnifiedSnapshotExecutionConsumer(entity);
                        entity.RunCharacterInputPhase(tickIndex);
#if UNITY_INCLUDE_TESTS
                        if (aiDecisionShadowMode == AiDecisionShadowMode.SharedShadow)
                            ApplyAiDecisionSharedPostLegacyMutationForSelfCheck(entity);
#endif
                        if (IsActiveForCurrentPass(entity))
                            RefreshRuntimeSnapshot(entity);
                        if (AiUnifiedSnapshotExecutionOwnsCurrentPass)
                        {
                            RefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput(entity);
                        }
                        else
                        {
                            if (AiDecisionRequiresSharedRows)
                                RefreshAiDecisionSharedRowAfterCharacterInput(entity);
                            if (aiSensingMode == AiSensingMode.SoAAiSensing)
                            {
                                ObserveAiCandidateCharacterInputMutation(entity);
                                RefreshAiSoASensingShadowRowAfterCharacterInput(entity);
                            }
                            else
                            {
                                ObserveAiTeamHpSummaryMutation(entity);
                            }
                            if (aiSensingMode == AiSensingMode.SoAShadowAiSensing)
                                RefreshAiSoASensingShadowRowAfterCharacterInput(entity);
                            RefreshAiUnifiedSnapshotShadowRowAfterCharacterInput(entity);
                        }
                    }
                }
            }
            finally
            {
                if (AiDecisionRequiresSharedRows)
                    EndAiDecisionSharedPass();
                ClearAiInputSlotSnapshot();
            }
        }

        public void CharacterInputAll(int tickIndex)
        {
            if (tickIndex <= 1)
                return;

            EnsureAiSensingModeAvailableBeforeTick();
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            BattleAiInputDetailDiagnostics aiDetailDiagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            aiDetailDiagnostics?.BeginTick(tickIndex);
            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.CharacterInputSnapshotBuild);
            BuildAiInputSlotSnapshot();
            if (AiDecisionRequiresSharedRows &&
                !AiUnifiedSnapshotExecutionOwnsCurrentPass)
                PrepareAiDecisionSharedPass();
            CompleteAiUnifiedSnapshotShadowInitialComparison();
            detailDiagnostics?.EndPhase(
                BattleTickDetailPhase.CharacterInputSnapshotBuild);
            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.CharacterInputEntityInputPass);
            try
            {
                using (BeginDeferredMutationEntityPass())
                {
                    foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                    {
                        if (entity.GetCurrentDataObjectTypeForSimulation() !=
                            (int)LF2ObjectType.Character)
                        {
                            continue;
                        }

                        BeginAiUnifiedSnapshotExecutionConsumer(entity);
                        entity.RunCharacterInputPhaseForKnownCharacterDat(tickIndex);
#if UNITY_INCLUDE_TESTS
                        if (aiDecisionShadowMode == AiDecisionShadowMode.SharedShadow)
                            ApplyAiDecisionSharedPostLegacyMutationForSelfCheck(entity);
                        runtimeHooks.CharacterInputPassMutationOverride?.Invoke(this, entity);
#endif
                        if (IsActiveForCurrentPass(entity))
                        {
                            aiDetailDiagnostics?.BeginPhase(
                                BattleAiInputDetailPhase.RefreshRuntimeSnapshot);
                            bool forceFullPostRefresh =
                                ForceFullCharacterInputPostRefreshForDiagnostics;
#if UNITY_INCLUDE_TESTS
                            forceFullPostRefresh |=
                                runtimeHooks.CharacterInputPassMutationOverride != null;
#endif
                            if (forceFullPostRefresh)
                                RefreshRuntimeSnapshot(entity);
                            else
                                entity.RefreshRuntimeSnapshotAfterCharacterInput();
                            aiDetailDiagnostics?.RecordRefresh();
                            aiDetailDiagnostics?.EndPhase(
                                BattleAiInputDetailPhase.RefreshRuntimeSnapshot);
                        }
                        if (AiUnifiedSnapshotExecutionOwnsCurrentPass)
                        {
                            aiDetailDiagnostics?.BeginPhase(
                                BattleAiInputDetailPhase.UnifiedSnapshotExecutionRowRefresh);
                            RefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput(entity);
                            aiDetailDiagnostics?.EndPhase(
                                BattleAiInputDetailPhase.UnifiedSnapshotExecutionRowRefresh);
                        }
                        else
                        {
                            if (AiDecisionRequiresSharedRows)
                                RefreshAiDecisionSharedRowAfterCharacterInput(entity);
                            if (aiSensingMode == AiSensingMode.SoAAiSensing)
                            {
                                ObserveAiCandidateCharacterInputMutation(entity);
                                RefreshAiSoASensingShadowRowAfterCharacterInput(entity);
                            }
                            else
                            {
                                ObserveAiTeamHpSummaryMutation(entity);
                            }
                            if (aiSensingMode == AiSensingMode.SoAShadowAiSensing)
                                RefreshAiSoASensingShadowRowAfterCharacterInput(entity);
                            RefreshAiUnifiedSnapshotShadowRowAfterCharacterInput(entity);
                        }
                    }
                }
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.CharacterInputEntityInputPass);
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.CharacterInputSnapshotClear);
                if (AiDecisionRequiresSharedRows)
                    EndAiDecisionSharedPass();
                ClearAiInputSlotSnapshot();
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.CharacterInputSnapshotClear);
            }
        }

        public void Oid5152RuntimeMaintenanceAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                for (int runtimeSlot = 0; runtimeSlot < 20; runtimeSlot++)
                {
                    LF2Entity obj = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                    if (obj == null || !IsActiveForCurrentPass(obj))
                        continue;

                    if (obj.Runtime.Unk338 > 0)
                    {
                        obj.Runtime.Unk338--;
                        RefreshRuntimeSnapshot(obj);
                    }

                    if (obj.ObjectId == 51)
                    {
                        TrySplitOid51BackToPair(obj);
                    }
                    else if (obj.ObjectId == 7 || obj.ObjectId == 8)
                    {
                        TryMergeOid7Or8Into51(obj);
                    }
                }
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private bool TryMergeOid7Or8Into51(LF2Entity self)
        {
            if (self?.Runtime == null || self.Health == null)
                return false;

            int selfSlot = self.Runtime.SlotIndex;
            LF2FrameData selfFrame = self.Frame?.D;
            if (selfSlot < 0 || selfSlot >= 10 || selfFrame == null || selfFrame.state != 2)
                return false;
            if (self.Health.HP <= 0 || self.Runtime.Unk338 != 0)
                return false;
            if (!PassesOid5152HpGate(self))
                return false;

            LF2CharacterDataWrapper oid51Wrapper = runtimeCharacterConfigs.Resolve(51);
            if (oid51Wrapper == null)
                return false;

            int selfX = self.GetRuntimeXInt();
            int selfZ = self.GetRenderZInt();
            int selfRelationTeam = ResolveOid5152RelationTeam(self);
            int partnerOid = 15 - self.ObjectId;

            for (int partnerSlot = 0; partnerSlot < 20; partnerSlot++)
            {
                if (partnerSlot == selfSlot)
                    continue;

                LF2Entity partner = FindEntityByRuntimeSlotForQuery(partnerSlot);
                if (partner?.Runtime == null || partner.Health == null)
                    continue;
                if (partner.ObjectId != partnerOid || partner.Health.HP <= 0 || partner.Runtime.Unk338 != 0)
                    continue;
                if (!PassesOid5152HpGate(partner))
                    continue;
                if (ResolveOid5152RelationTeam(partner) != selfRelationTeam)
                    continue;

                LF2FrameData partnerFrame = partner.Frame?.D;
                int partnerFrameId = partner.Frame?.N ?? -1;
                if (partnerFrame == null || partnerFrameId < 0 || partnerFrameId >= LF2FrameCache.MaxFrameIdExclusive)
                    continue;
                if (partnerFrame.state == 14)
                    continue;
                if (partnerFrame.state != 2 && (partner.GetRuntimeYInt() != 0 || partnerSlot <= 9))
                    continue;

                int partnerX = partner.GetRuntimeXInt();
                int partnerZ = partner.GetRenderZInt();
                if (Mathf.Abs(selfX - partnerX) >= 50 || Mathf.Abs(selfZ - partnerZ) >= 8)
                    continue;
                if (partnerSlot <= 9 && selfX <= partnerX)
                    continue;

                int mergedHpBound = self.Health.HPBound + partner.Health.HPBound;
                if (mergedHpBound > self.Health.HP3)
                    mergedHpBound = self.Health.HP3;

                int mergedHp = self.Health.HP + partner.Health.HP;
                if (mergedHp > mergedHpBound)
                    mergedHp = mergedHpBound;

                int midpointX = (selfX + partnerX) / 2;
                int midpointZ = (selfZ + partnerZ) / 2;
                int originalSelfOid = self.ObjectId;

                self.Runtime.Unk328 = 1;
                self.Runtime.Unk32C = partnerSlot;
                self.Runtime.Unk330 = originalSelfOid;
                self.Runtime.Unk334 = partner.ObjectId;
                self.Runtime.Unk338 = 4500;
                self.Health.HPBound = mergedHpBound;
                self.Health.HP = mergedHp;
                self.Runtime.Vx = 0f;
                self.Runtime.X = midpointX;
                self.Runtime.Z = midpointZ;
                self.Runtime.XInt = midpointX;
                self.Runtime.ZInt = midpointZ;

                partner.Runtime.Vy = 0f;
                partner.Runtime.OidMergeDormant = true;

                self.TryApplyRuntimeIdentity(51, 290, false, out _);
                self.Health.PP = 500;
                self.RefreshRuntimeSnapshot();
                partner.RefreshRuntimeSnapshot();
                return true;
            }

            return false;
        }

        private bool TrySplitOid51BackToPair(LF2Entity self)
        {
            if (self?.Runtime == null || self.Health == null)
                return false;
            if (self.ObjectId != 51 || self.Runtime.Unk328 != 1 || self.Runtime.Unk338 > 0)
                return false;

            int currentFrameId = self.Frame?.N ?? -1;
            if (currentFrameId >= 9 && currentFrameId <= 260)
                return false;

            int originalOid = self.Runtime.Unk330;
            if (runtimeCharacterConfigs.Resolve(originalOid) == null)
                return false;

            int aggregateHp = self.Health.HP;
            int aggregateHpBound = self.Health.HPBound;
            int partnerSlot = self.Runtime.Unk32C;
            int partnerOid = self.Runtime.Unk334;
            double splitX = self.Runtime.X;
            double splitZ = self.Runtime.Z;
            int splitXInt = self.GetRuntimeXInt();
            int splitZInt = self.GetRenderZInt();
            double preservedVy = self.Runtime.Vy;
            double preservedVz = self.Runtime.Vz;
            string preservedDir = self.Runtime.Dir;

            self.TryApplyRuntimeIdentity(originalOid, currentFrameId, false, out _);
            self.Runtime.Unk328 = -1;
            self.Runtime.Unk338 = 900;
            self.RefreshRuntimeSnapshot();

            if (partnerSlot < 0)
                return true;

            LF2Entity partner = FindEntityByRuntimeSlotIncludingDormant(partnerSlot);
            if (partner == null || runtimeCharacterConfigs.Resolve(partnerOid) == null)
                return true;

            int halfHp = aggregateHp / 2;
            int halfHpBound = aggregateHpBound / 2;
            int partnerStableId = partner.Runtime.StableId;
            int partnerRuntimeSlot = partner.Runtime.SlotIndex;

            self.TryApplyRuntimeIdentity(originalOid, 112, false, out _);
            self.Health.HP = halfHp;
            self.Health.HPBound = halfHpBound;
            self.Health.PP = 0;
            self.Runtime.Y = 0f;
            self.Runtime.YInt = 0;
            self.Runtime.Vx = 0f;
            self.Runtime.Vy = preservedVy;
            self.Runtime.Vz = preservedVz;
            self.Runtime.Dir = preservedDir;
            self.RefreshRuntimeSnapshot();

            LF2ItrRestTracker partnerRest = partner.ItrRest;
            partnerRest?.BeginPreserveStateAcrossOwnerReset();
            try
            {
                partner.Reset();
            }
            finally
            {
                partnerRest?.EndPreserveStateAcrossOwnerReset();
            }
            // LF2Character.Reset has pool-specific defaults that differ from formal Entity::reset.
            partner.FrameDelay = 0;
            partner.KnockbackVx = 0.1;
            partner.KnockbackVy = 0.1;
            partner.KnockbackVz = 0.1;
            partner.HolderCopySlot = 99;
            partner.Effect?.Reset();
            if (partner is LF2Character partnerCharacter)
                partnerCharacter.DeadBlinkCountInternal = -1;
            if (partner.Frame != null)
            {
                partner.Frame.PN = 0;
                partner.Frame.Prev = 0;
                partner.Frame.Prev2 = 0;
                partner.Frame.Prev2D = null;
            }
            partner.Runtime.StableId = partnerStableId;
            partner.SetRuntimeSlotIndex(partnerRuntimeSlot);
            partner.Runtime.OidMergeDormant = false;
            partner.TryApplyRuntimeIdentity(partnerOid, 112, true, out _);
            partner.Health.HP = halfHp;
            partner.Health.HPBound = halfHpBound;
            partner.Health.PP = 0;
            partner.RelationTeam = self.RelationTeam;
            partner.Runtime.X = splitX;
            partner.Runtime.Y = 0f;
            partner.Runtime.Z = splitZ;
            partner.Runtime.XInt = splitXInt;
            partner.Runtime.YInt = 0;
            partner.Runtime.ZInt = splitZInt;
            partner.Runtime.Vx = 0f;
            partner.Runtime.Vy = 0f;
            partner.Runtime.Vz = 0f;
            partner.SwitchDir(preservedDir == "right" ? "left" : "right");
            partner.RefreshRuntimeSnapshot();
            return true;
        }

        private bool PassesOid5152HpGate(LF2Entity entity)
        {
            if (entity?.Health == null || entity.Health.HP <= 0)
                return false;

            return BattleGameModeId == 1 || entity.Health.HP < 177;
        }

        private static int ResolveOid5152RelationTeam(LF2Entity entity)
        {
            return entity?.RelationTeam ?? 0;
        }

        public void SerialTickAll(int tickIndex)
        {
            using (BeginDeferredMutationEntityPass())
            {
                // C# authority GameTick scans active slots in ascending order and completes
                // one entity before advancing to the next slot. The dynamic scan lets a
                // flushed producer in a later slot participate this tick; a reused lower slot
                // waits until the next tick.
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    // C# authority GameTick clears current action/direction keys immediately
                    // before each active entity enters frame advance. Prev*, cooldowns,
                    // combo state, and input history remain untouched.
                    entity.Runtime?.ClearActionInputKeys();
                    entity.Runtime?.ClearDirectionalInputKeys();
                    entity.SimTransit(tickIndex);
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    entity.SimTU(tickIndex);
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }

                CleanupState9998Entities();
            }
        }

        private void CleanupState9998Entities()
        {
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null || frame.state != 9998) continue;
                entity.FreeEntityLikeExe();
            }

            _entityScratch.Clear();
        }

        public void PostFrameAdvanceDeathCleanupAll(int tickIndex)
        {
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                entity?.Runtime?.SyncIntegerPosition();
            }

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (!PassesRespawnGate(entity))
                    continue;

                if (entity.RespawnCount <= 0)
                {
                    ApplyRespawnWithoutStoredCount(entity);
                }
                else
                {
                    ApplyRespawnFromStoredCount(entity);
                }

                if (IsActiveForCurrentPass(entity))
                    RefreshRuntimeSnapshot(entity);
            }

            _entityScratch.Clear();
        }

        private bool PassesRespawnGate(LF2Entity entity)
        {
            if (entity?.Health == null || !IsActiveForCurrentPass(entity))
                return false;

            LF2FrameData frame = entity.Frame?.D;
            if (frame == null || frame.state != LF2States.Lying || entity.Health.HP > 0)
                return false;

            int slotIndex = entity.Runtime?.SlotIndex ?? -1;
            if (slotIndex < 20 && entity.KillCount < 0 && entity.RelationTeam != 5)
                return false;

            int hitStop = entity.HitStun;
            return hitStop > 0 && hitStop < 5;
        }

        private void ApplyRespawnWithoutStoredCount(LF2Entity entity)
        {
            int hp2 = entity.HP2Orig;
            if (hp2 < 2)
            {
                entity.FreeEntityLikeExe();
                return;
            }

            entity.HP2Orig = hp2 - 1;

            int relationTeam = entity.RelationTeam;
            int sumX = 0;
            int sumZ = 0;
            int count = 0;

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity other = _entityScratch[i];
                if (other == null || other == entity || other.Health == null)
                    continue;

                if (other.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    continue;

                if (other.RelationTeam != relationTeam)
                    continue;

                sumX += other.Runtime.XInt;
                sumZ += other.Runtime.ZInt;
                count++;
            }

            if (count > 0)
            {
                int avgX = sumX / count;
                int avgZ = sumZ / count;
                entity.Runtime.X = avgX + entity.BattleRandInt(0, 51) - 26.0;
                entity.Runtime.XInt = (int)entity.Runtime.X;
                entity.Runtime.Z = avgZ + entity.BattleRandInt(0, 31) - 16.0;
                entity.Runtime.ZInt = (int)entity.Runtime.Z;
                entity.PS.x = entity.Runtime.X;
                entity.PS.z = entity.Runtime.Z;
            }

            entity.Health.PP = 500;
            entity.Health.PPBound = entity.Health.MaxPP;
            entity.Health.HPBound = entity.Health.HP3;
            entity.Health.HP = entity.Health.HPBound;
            entity.HitStun = 20;
            entity.DirectWriteFramePreserveWaitCounter(212);
            entity.PS.y = -300.0;
            entity.PS.vy = 0.0;
            entity.Runtime.Y = -300.0;
            entity.Runtime.Vy = 0.0;
            entity.Runtime.SyncIntegerPosition();
        }

        private void ApplyRespawnFromStoredCount(LF2Entity entity)
        {
            entity.HP2Orig = entity.HPOrig;
            entity.Health.PP = 0;
            entity.Health.HPBound = entity.RespawnCount;
            entity.Health.HP3 = entity.Health.HPBound;
            entity.Health.HP = entity.Health.HP3;
            entity.RespawnCount = 0;
            entity.HPOrig = 0;
            entity.RelationTeam = 1;

            if (entity.ObjectId >= 0x1E && entity.ObjectId <= 0x24)
                entity.Runtime.RenderPicOffset = 0x8C;

            entity.DirectWriteFramePreserveWaitCounter(0xDB);
            entity.AttackingCounter = 0;
            entity.FrameDelay = 0xA;

            TrySpawnRespawnEffect(entity);
        }

        private LF2Entity TrySpawnRespawnEffect(LF2Entity entity)
        {
            if (entity == null)
                return null;

            LF2Entity overrideSpawned =
                runtimeHooks.RespawnEffectSpawnOverride?.Invoke(this, entity);
            if (overrideSpawned != null)
                return overrideSpawned;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return null;

            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            if (task == null)
                return null;
            task.opoint = new ObjectPoint { oid = 998, kind = 0, action = 6, facing = 0 };
            task.parent = null;
            task.team = 0;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = entity.RelationTeam;
            task.holderCopySlot = -1;
            task.spawnerEntityIndex = entity.Runtime?.SlotIndex ?? -1;
            task.pos = new Vector3(entity.GetRuntimeXInt(), entity.GetRuntimeYInt(), entity.GetRenderZInt());
            task.z = entity.GetRenderZInt();
            task.dir = "right";
            task.useDirectVelocity = true;
            task.directVx = 0f;
            task.directVy = 0f;
            task.directVz = 0f;
            task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = entity.GetRuntimeXInt();
            task.initialRuntimeY = entity.GetRuntimeYInt();
            task.initialRuntimeZ = entity.GetRenderZInt() + 1;
            task.deferPresentationToNextTick = false;
            task.suppressLateFrameTickThisTick = false;
            task.deferFrameTickToNextTick = false;

            LF2Entity spawned;
            try
            {
                spawned = factory.CreateObjectImmediate(task);
            }
            finally
            {
                LF2ReferencePool.Instance.Recycle(task);
            }
            if (spawned == null)
                return null;

            spawned.RelationTeam = entity.RelationTeam;
            spawned.SpawnerEntityIndex = entity.Runtime?.SlotIndex ?? -1;
            spawned.RefreshRuntimeSnapshot();
            return spawned;
        }

        public void EarlyFrameAdvanceSpecialsAll(int tickIndex)
        {
            LastEarlyTeleportRefreshCountForDiagnostics = 0;
            LastEarlyTeleportSnapshotSkipCountForDiagnostics = 0;
            LastEarlyStateHandlePathUsedForDiagnostics = false;
            LastEarlyStateHandleFallbackCountForDiagnostics = 0;

            if (ForceLegacyEarlyFrameAdvanceForDiagnostics)
            {
                RunEarlyFrameAdvanceSpecialsLegacy();
                return;
            }

            bool teleportGate = FrameToggle != 0;
            bool handleSnapshotValid = TryBuildEarlyStateHandleSnapshot(
                out ulong occupancyEpoch,
                out int logicalCapacity);
            if (!handleSnapshotValid)
            {
                // A partial slot-table proof cannot stand in for the authority's
                // complete active snapshot. Rebuild the exact legacy view before
                // running any entity callbacks.
                GetActiveEntitiesByRuntimeSlot(_entityScratch);
                earlyState500Handles.Clear();
                earlyState501Handles.Clear();
            }

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity == null)
                    continue;

                bool mutated =
                    entity.RunEarlyTeleportSpecialsPhaseWithMutationReport(
                        _entityScratch,
                        teleportGate);
                if (!IsActiveForCurrentPass(entity))
                    continue;
                if (mutated)
                {
                    LastEarlyTeleportRefreshCountForDiagnostics++;
                    RefreshRuntimeSnapshot(entity);
                }
                else
                {
                    LastEarlyTeleportSnapshotSkipCountForDiagnostics++;
                }
            }

            if (handleSnapshotValid &&
                ValidateEarlyStateHandleSnapshot(
                    occupancyEpoch,
                    logicalCapacity))
            {
                LastEarlyStateHandlePathUsedForDiagnostics = true;
                if (!RunEarlyStateHandles(
                        earlyState500Handles,
                        500,
                        occupancyEpoch,
                        logicalCapacity) ||
                    !RunEarlyStateHandles(
                        earlyState501Handles,
                        501,
                        occupancyEpoch,
                        logicalCapacity))
                {
                    LastEarlyStateHandlePathUsedForDiagnostics = false;
                    LastEarlyStateHandleFallbackCountForDiagnostics++;
                    RunEarlyState500Specials(_entityScratch);
                    RunEarlyState501Specials(_entityScratch);
                }
            }
            else
            {
                LastEarlyStateHandleFallbackCountForDiagnostics++;
                RunEarlyState500Specials(_entityScratch);
                RunEarlyState501Specials(_entityScratch);
            }

            earlyState500Handles.Clear();
            earlyState501Handles.Clear();
            _entityScratch.Clear();
        }

        private void RunEarlyFrameAdvanceSpecialsLegacy()
        {
            bool teleportGate = FrameToggle != 0;
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity == null)
                    continue;

                entity.RunEarlyTeleportSpecialsPhase(
                    _entityScratch,
                    teleportGate);
                if (!IsActiveForCurrentPass(entity))
                    continue;
                LastEarlyTeleportRefreshCountForDiagnostics++;
                RefreshRuntimeSnapshot(entity);
            }

            RunEarlyState500Specials(_entityScratch);
            RunEarlyState501Specials(_entityScratch);
            _entityScratch.Clear();
        }

        private bool TryBuildEarlyStateHandleSnapshot(
            out ulong occupancyEpoch,
            out int logicalCapacity)
        {
            _entityScratch.Clear();
            earlyState500Handles.Clear();
            earlyState501Handles.Clear();

            occupancyEpoch = _runtimeSlots.OccupancyEpoch;
            logicalCapacity = _runtimeSlots.LogicalCapacity;
            for (int runtimeSlot = 0;
                 runtimeSlot < logicalCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                if (!view.Claimed)
                {
                    if (view.Entity != null)
                        return false;
                    continue;
                }

                LF2Entity entity = view.Entity;
                if (entity == null ||
                    view.Generation == 0 ||
                    entity.Runtime == null ||
                    entity.Runtime.SlotIndex != runtimeSlot)
                {
                    return false;
                }

                var handle =
                    new RuntimeEntityHandle(runtimeSlot, view.Generation);
                if (!_runtimeSlots.TryResolve(handle, out LF2Entity resolved) ||
                    !ReferenceEquals(resolved, entity))
                {
                    return false;
                }

                if (!IsActiveForCurrentPass(entity))
                    continue;

                _entityScratch.Add(entity);
                int state = entity.Frame?.D?.state ?? -1;
                if (state == 500)
                    earlyState500Handles.Add(handle);
                else if (state == 501)
                    earlyState501Handles.Add(handle);
            }

            return occupancyEpoch == _runtimeSlots.OccupancyEpoch &&
                   logicalCapacity == _runtimeSlots.LogicalCapacity;
        }

        private bool ValidateEarlyStateHandleSnapshot(
            ulong occupancyEpoch,
            int logicalCapacity)
        {
            if (occupancyEpoch != _runtimeSlots.OccupancyEpoch ||
                logicalCapacity != _runtimeSlots.LogicalCapacity)
            {
                return false;
            }

            return ValidateEarlyStateHandles(earlyState500Handles, 500) &&
                   ValidateEarlyStateHandles(earlyState501Handles, 501) &&
                   occupancyEpoch == _runtimeSlots.OccupancyEpoch;
        }

        private bool ValidateEarlyStateHandles(
            List<RuntimeEntityHandle> handles,
            int expectedState)
        {
            for (int i = 0; i < handles.Count; i++)
            {
                if (!TryResolveEarlyStateHandle(
                        handles[i],
                        expectedState,
                        out _))
                {
                    return false;
                }
            }

            return true;
        }

        private bool RunEarlyStateHandles(
            List<RuntimeEntityHandle> handles,
            int expectedState,
            ulong occupancyEpoch,
            int logicalCapacity)
        {
            for (int i = 0; i < handles.Count; i++)
            {
                if (occupancyEpoch != _runtimeSlots.OccupancyEpoch ||
                    logicalCapacity != _runtimeSlots.LogicalCapacity ||
                    !TryResolveEarlyStateHandle(
                        handles[i],
                        expectedState,
                        out LF2Entity entity))
                {
                    return false;
                }

                if (expectedState == 500)
                    RunEarlyState500Special(entity);
                else
                    RunEarlyState501Special(entity, _entityScratch);
            }

            return occupancyEpoch == _runtimeSlots.OccupancyEpoch &&
                   logicalCapacity == _runtimeSlots.LogicalCapacity;
        }

        private bool TryResolveEarlyStateHandle(
            RuntimeEntityHandle handle,
            int expectedState,
            out LF2Entity entity)
        {
            entity = null;
            if (!handle.IsValid ||
                !_runtimeSlots.TryResolve(handle, out LF2Entity resolved) ||
                resolved == null ||
                resolved.Runtime == null ||
                resolved.Runtime.SlotIndex != handle.Slot ||
                !IsActiveForCurrentPass(resolved) ||
                resolved.Frame?.D?.state != expectedState)
            {
                return false;
            }

            entity = resolved;
            return true;
        }

        private void RunEarlyState500Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                RunEarlyState500Special(entity);
            }
        }

        private void RunEarlyState500Special(LF2Entity entity)
        {
            LF2FrameData frame = entity?.Frame?.D;
            if (frame == null || frame.state != 500)
                return;

            if (entity.TransformTargetObjectId == -1 ||
                entity.TransformOriginalObjectId >= 0)
            {
                // BMD-023: state=500 reset branch must mirror baseline SetFrameImmediate:
                // write Frame + FrameWaitCounter only, never Attacking. Unity's
                // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                entity.DirectWriteFramePreserveWaitCounter(0);
                RefreshRuntimeSnapshot(entity);
            }
        }

        private void RunEarlyState501Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                RunEarlyState501Special(entity, entities);
            }
        }

        private void RunEarlyState501Special(
            LF2Entity entity,
            List<LF2Entity> activeEntities)
        {
            LF2FrameData frame = entity?.Frame?.D;
            if (frame == null ||
                frame.state != 501 ||
                entity.TransformTargetObjectId <= -1)
            {
                return;
            }

            LF2CharacterDataWrapper wrapper =
                runtimeCharacterConfigs.Resolve(entity.TransformTargetObjectId);
            if (wrapper == null)
                return;

            entity.TransformOriginalObjectId = entity.ObjectId;
            entity.FrameCache.Load(wrapper);
            entity.ObjectId = entity.TransformTargetObjectId;
            // BMD-023: state=501 transform branch must mirror baseline SetFrameImmediate:
            // write Frame + FrameWaitCounter only, never Attacking. Unity's
            // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
            entity.DirectWriteRawFramePreserveWaitCounter(0);
            RefreshRuntimeSnapshot(entity);

            int ownerSlotIndex = entity.Runtime?.SlotIndex ?? -1;
            if (ownerSlotIndex < 0)
                return;

            for (int j = 0; j < activeEntities.Count; j++)
            {
                LF2Entity child = activeEntities[j];
                if (child == null)
                    continue;
                if (child.KillCount != ownerSlotIndex)
                    continue;
                if (child.Health != null && child.Health.HP <= 0)
                    continue;

                child.FrameCache.Load(wrapper);
                child.ObjectId = entity.ObjectId;
                // BMD-023: state=501 child-transform branch must mirror baseline SetFrameImmediate.
                // The authority selects from the integer Y snapshot, not the floating render position.
                // write Frame + FrameWaitCounter only, never Attacking. Unity's
                // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                child.DirectWriteRawFramePreserveWaitCounter(
                    child.Runtime != null && child.Runtime.YInt < 0
                        ? 212
                        : 0);
                RefreshRuntimeSnapshot(child);
            }
        }

        public void FrameLogicBeforeAdvanceAll(int tickIndex)
        {
            using (BeginDeferredMutationEntityPass())
            {
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    LF2FrameData frame = entity.Frame?.D;
                    if (frame == null ||
                        frame.hit_Fa <= 0 ||
                        entity.GetCurrentDataObjectTypeForSimulation() ==
                        (int)LF2ObjectType.Character)
                    {
                        continue;
                    }

                    entity.RunFrameLogicBeforeAdvance();
                    FlushQueuedObjectPointTasks();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }
            }
        }

        internal int FindFirstFreeFrameLogicRuntimeSlot()
        {
            return FindFirstFreeRuntimeSlot(DynamicRuntimeSlotStart, RuntimeSlotCapacity);
        }

        public void CaptureCollisionFrameSnapshotsAll()
        {
            using (BeginDeferredMutationEntityPass())
            {
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    if (entity.Runtime != null &&
                        entity.Runtime.SuppressCollisionCandidateUntilTick > 0)
                    {
                        int currentTick = CurrentTickIndex;
                        if (currentTick <
                            entity.Runtime.SuppressCollisionCandidateUntilTick)
                        {
                            continue;
                        }
                    }

                    entity.CaptureCollisionFrameSnapshot();
                    RefreshRuntimeSnapshot(entity);
                }
            }

        }

        public void CollectCollisionCandidatesAll()
        {
            if (SceneQuery is BruteForceSceneQuery bruteForce)
                bruteForce.CollectCollisionCandidates();
        }

        public void TickCollisionPairVRestAll()
        {
            _runtimeRestStore.BeginCollisionPairVRestEligibility();
            int visitedItems = 0;
            for (int bucketIndex = 0;
                 bucketIndex < objectBucketRegistry.OrderedCount;
                 bucketIndex++)
            {
                List<ISimObject> items =
                    objectBucketRegistry.GetOrderedBucket(bucketIndex).items;
                for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                {
                    visitedItems++;
                    if (items[itemIndex] is not LF2Entity entity ||
                        !IsActiveForCurrentPass(entity) ||
                        entity.FrameCache?.Wrapper?.characterData == null)
                    {
                        continue;
                    }

                    int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
                    if (!_runtimeSlots.IsAddressable(runtimeSlot) ||
                        !object.ReferenceEquals(
                            _runtimeSlots.GetCurrentOccupant(runtimeSlot),
                            entity))
                    {
                        continue;
                    }

                    _runtimeRestStore.MarkCollisionPairVRestEligible(runtimeSlot);
                }
            }
            LastCollisionPairVRestEligibilityVisitCount = visitedItems;
            _runtimeRestStore.TickMarkedCollisionPairVRest();
        }

        public void EndCollisionCandidateConsumption()
        {
            if (SceneQuery is BruteForceSceneQuery bruteForce)
                bruteForce.EndCollisionCandidateConsumption();
        }

        public void LateEntityUpdateAll(int tickIndex)
        {
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            // The production object-point factory is pass-stable. Resolve it lazily so an
            // empty LateEntityUpdateAll invocation retains the existing no-auto-create behavior.
            LF2ObjectPointFactory opointFactory = null;
            bool opointFactoryResolved = false;
            if (structuralEventSink != null)
                SetStructuralEventContextForDiagnostics(tickIndex, "late-entity-update");
            _ticking = true;
            try
            {
                for (int runtimeSlot = 0; runtimeSlot < RuntimeSlotCapacity; runtimeSlot++)
                {
                    LF2Entity obj = FindEntityByRuntimeSlotCurrent(runtimeSlot);

                    if (obj == null)
                        continue;
                    if (!IsActiveForCurrentPass(obj))
                        continue;

                    if (structuralEventSink != null)
                    {
                        structuralEventCursorSlot = runtimeSlot;
                        EmitStructuralEvent(
                            "scan",
                            runtimeSlot,
                            0,
                            RuntimeSlotCapacity,
                            "active",
                            "visited",
                            StructuralSourceKind(obj),
                            runtimeSlot);
                    }

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityStateSpecial);
                    obj.RunStateSpecialPreCollision();
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityStateSpecial);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityStateSpecial);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityRecovery);
                    obj.RunPreCollisionRecoveryPhase(tickIndex);
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityRecovery);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityRecovery);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityFrameTick);
                    if (obj.Runtime == null ||
                        tickIndex >= obj.Runtime.SuppressLateFrameTickUntilTick)
                    {
                        obj.SimFrameTick(tickIndex);
                    }
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityFrameTick);
                        continue;
                    }
                    if (LateRuntimeSnapshotModeForDiagnostics ==
                        BattleLateRuntimeSnapshotMode.LegacyThree)
                    {
                        RefreshLateRuntimeSnapshot(
                            obj,
                            BattleLateRuntimeSnapshotStage.FrameTick,
                            detailDiagnostics);
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityFrameTick);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityFrameExit);
                    bool exitedLateFrameTick = HandleLateFrameTickExit(
                        obj,
                        detailDiagnostics);
                    if (exitedLateFrameTick)
                    {
                        if (obj is LF2SpecialAttack)
                            FlushQueuedObjectPointTasks();
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityFrameExit);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityFrameExit);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityDeathOpoint);
                    obj.RunLateDeathOpointPreCleanupPhase();
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityDeathOpoint);
                        continue;
                    }
                    if (LateRuntimeSnapshotModeForDiagnostics ==
                        BattleLateRuntimeSnapshotMode.LegacyThree)
                    {
                        RefreshLateRuntimeSnapshot(
                            obj,
                            BattleLateRuntimeSnapshotStage.DeathOpoint,
                            detailDiagnostics);
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityDeathOpoint);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityOpointProcess);
                    if (!opointFactoryResolved)
                    {
                        opointFactory = LF2ObjectPointFactory.Instance;
                        opointFactoryResolved = true;
                    }
                    LF2FrameData opointFrame = obj.Frame?.D;
                    bool frameHasOpoint = opointFrame != null &&
                        ((opointFrame.opoints != null && opointFrame.opoints.Count > 0) ||
                         opointFrame.opoint.HasValue);
                    if (opointFactory != null && frameHasOpoint)
                        opointFactory.ProcessOpointSpawnAlignedToCpp(obj);
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityOpointProcess);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityOpointProcess);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityCleanup);
                    bool completedLateCleanup = obj.TryRunLatePostOpointCleanupPhase();
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityCleanup);
                    if (completedLateCleanup)
                    {
                        detailDiagnostics?.BeginPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        FlushQueuedObjectPointTasks();
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        continue;
                    }

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                    obj.RunLateTailBeforePrevFrame();
                    FlushQueuedObjectPointTasks();
                    if (!IsActiveForCurrentPass(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        continue;
                    }

                    RefreshLateRuntimeSnapshot(
                        obj,
                        BattleLateRuntimeSnapshotStage.TailAndQueuedFlush,
                        detailDiagnostics);
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityPrevFrameMirror);
                    obj.MirrorLatePrevFrame();
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityPrevFrameMirror);
                }
            }
            finally
            {
                _ticking = false;
                if (structuralEventSink != null)
                    structuralEventCursorSlot = -1;
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.LateEntityFinalPendingFlush);
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.LateEntityFinalPendingFlush);
            }
        }

        private void RefreshLateRuntimeSnapshot(
            LF2Entity entity,
            BattleLateRuntimeSnapshotStage stage,
            BattleTickDetailPhaseDiagnostics diagnostics)
        {
            if (diagnostics == null)
            {
                RefreshRuntimeSnapshot(entity);
                return;
            }

            diagnostics.BeginLateRuntimeSnapshot(stage);
            try
            {
                RefreshRuntimeSnapshot(entity);
            }
            finally
            {
                diagnostics.EndLateRuntimeSnapshot(stage);
            }
        }

        public BattleLateRuntimeSnapshotMode LateRuntimeSnapshotModeForDiagnostics
        {
            get;
            set;
        } = BattleLateRuntimeSnapshotMode.ConsolidatedFinal;

        internal void RefreshLateTransitionRuntimeSnapshot(LF2Entity entity)
        {
            if (LateRuntimeSnapshotModeForDiagnostics ==
                BattleLateRuntimeSnapshotMode.ConsolidatedFinal)
            {
                return;
            }

            RefreshLateRuntimeSnapshot(
                entity,
                BattleLateRuntimeSnapshotStage.TransitionInternal,
                ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics);
        }

        private bool HandleLateFrameTickExit(
            LF2Entity entity,
            BattleTickDetailPhaseDiagnostics diagnostics)
        {
            if (entity?.Frame == null)
                return false;

            int frameId = entity.Frame.N;
            int frameGroup = frameId / 100;
            if (frameGroup == 11 || frameGroup == 12)
            {
                int ownerSlot = GetRuntimeSlotOrder(entity);
                GetAllEntities(_entityScratch);
                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity other = _entityScratch[i];
                    if (other != null && other.KillCount == ownerSlot)
                        other.HitStun = 1100 - frameId;
                }

                _entityScratch.Clear();
                entity.HitStun = 1100 - frameId;
                entity.DirectWriteFramePreserveWaitCounter(0);
                RefreshLateRuntimeSnapshot(
                    entity,
                    BattleLateRuntimeSnapshotStage.FrameExit,
                    diagnostics);
                return true;
            }

            if (frameId < 0 || frameId >= LF2FrameCache.MaxFrameIdExclusive)
            {
                entity.FreeEntityLikeExe();
                return true;
            }

            return false;
        }

        public void EntityPostFrameTailAll(int tickIndex)
        {
            foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
            {
                if (entity.Health == null)
                    continue;

                if (entity.HealTimer / 1000 == 1 && entity.Health.HP > 0)
                {
                    entity.HealTimer--;
                    if (entity.HealTimer % 8 == 0)
                    {
                        if (entity.Health.HP < entity.Health.HPBound)
                        {
                            entity.Health.HP += 8;
                            if (entity.Health.HP > entity.Health.HPBound)
                                entity.Health.HP = entity.Health.HPBound;
                        }
                        else
                        {
                            entity.HealTimer = 0;
                        }
                    }

                    if (entity.HealTimer % 1000 == 0)
                        entity.HealTimer = 0;
                }

                if (entity.CatchTimer > 0 && entity.Health.HP > 0)
                {
                    entity.CatchTimer--;
                    if (entity.CatchTimer % 8 == 0 && entity.Health.HP < entity.Health.HPBound)
                    {
                        entity.Health.HP += 8;
                        if (entity.Health.HP > entity.Health.HPBound)
                        {
                            entity.Health.HP = entity.Health.HPBound;
                            entity.CatchTimer = 0;
                        }
                    }
                }

                LF2FrameData frame = entity.Frame?.D;
                if (frame != null && frame.state == 1700)
                    entity.HealTimer = 1100;

                entity.ClearHitCandidateCarriers();
                entity.Runtime.TransientMp = 0;
                entity.Runtime.TransientMp2 = 1000;
                entity.Runtime.TransientMp3 = 1000;
                entity.Runtime.TransientMp4 = 1000;
                RefreshRuntimeSnapshot(entity);
            }

        }

        public void FramePostProcessAll()
        {
            foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
            {
                if (entity.FrameDelay != 0)
                    continue;

                if (entity.HitCount > 0)
                {
                    float denom = entity.HitCount + 1;
                    entity.PS.vx = entity.KnockbackVx * 2f / denom;
                    entity.PS.vy = entity.KnockbackVy * 2f / denom;
                    entity.PS.vz = entity.KnockbackVz * 2f / denom;
                }
                entity.KnockbackVx = 0f;
                entity.KnockbackVy = 0f;
                entity.KnockbackVz = 0f;
                entity.HitCount = 0;
                RefreshRuntimeSnapshot(entity);
            }
        }

        public void VrestTickAll(int tickIndex)
        {
            foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
            {
                entity.ItrRest?.TickArest();
                ClearAttackExemptIfCurrentFrameCannotHit(entity);
                RefreshRuntimeSnapshot(entity);
            }
        }

        private void ClearAttackExemptIfCurrentFrameCannotHit(LF2Entity entity)
        {
            if (entity == null || entity.AttackExempt <= 0)
                return;

            LF2CharacterData entityData = (entity as LF2LivingObject)?._FrameDataWrapper?.characterData
                ?? entity.FrameCache?.Wrapper?.characterData;
            if (entityData == null)
                return;

            LF2FrameData frame = entity.Frame?.D;
            bool clear = frame?.itrs == null || frame.itrs.Count == 0;
            if (!clear &&
                frame.state == LF2States.WeaponOnHand &&
                entity.Runtime != null)
            {
                int holderSlot = entity.Runtime.ResolveActiveHolderSlotIndex();
                LF2Entity holder = holderSlot >= 0
                    ? FindEntityByRuntimeSlotForQuery(holderSlot)
                    : null;
                LF2CharacterData holderData = (holder as LF2LivingObject)?._FrameDataWrapper?.characterData
                    ?? holder?.FrameCache?.Wrapper?.characterData;
                if (holder != null && holderData != null)
                {
                    LF2FrameData holderFrame = holder.Frame?.D;
                    clear = holderFrame?.wpoints == null ||
                            holderFrame.wpoints.Count == 0 ||
                            holderFrame.wpoints[0].attacking == 0;
                }
            }

            if (clear)
                entity.AttackExempt = 0;
        }

        public void PostInteractionTickAll(int tickIndex)
        {
            using (BeginDeferredMutationEntityPass())
            {
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    if (!entity.SupportsPostInteractionPhase())
                        continue;
                    if (entity.Runtime != null &&
                        tickIndex < entity.Runtime.SuppressPostInteractionUntilTick)
                    {
                        continue;
                    }

                    entity.SimPostInteraction(tickIndex);
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }
            }
        }

        public void ObjectInteractionTickAll(int tickIndex)
        {
            using (BeginDeferredMutationEntityPass())
            {
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    if (!entity.SupportsObjectInteractionPhase())
                        continue;
                    if (entity.Runtime != null &&
                        tickIndex < entity.Runtime.SuppressObjectInteractionUntilTick)
                    {
                        continue;
                    }

                    entity.SimObjectInteraction(tickIndex);
                    if (entity is LF2SpecialAttack)
                        FlushQueuedObjectPointTasks();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }
            }
        }

        public void PreInteractionTickAll(int tickIndex)
        {
            LastPreInteractionScannedCountForDiagnostics = 0;
            LastPreInteractionExecutedCountForDiagnostics = 0;
            LastPreInteractionProofSkipCountForDiagnostics = 0;
            LastPreInteractionSnapshotSkipCountForDiagnostics = 0;
            LastPreInteractionFailClosedCountForDiagnostics = 0;
            LastPreInteractionCpointCheckProofSkipCountForDiagnostics = 0;
            LastPreInteractionMismatchTailProofSkipCountForDiagnostics = 0;
            LastPreInteractionWholePassProofSucceededForDiagnostics = false;
            LastPreInteractionWholePassParticipantCountForDiagnostics = 0;

            _ticking = true;
            try
            {
                if (!ForceLegacyPreInteractionForDiagnostics &&
                    TryProveWholePreInteractionPassNoOp(
                        tickIndex,
                        out int participantCount))
                {
                    LastPreInteractionWholePassProofSucceededForDiagnostics = true;
                    LastPreInteractionWholePassParticipantCountForDiagnostics =
                        participantCount;
                    LastPreInteractionScannedCountForDiagnostics = participantCount;
                    LastPreInteractionProofSkipCountForDiagnostics =
                        participantCount * 3;
                    LastPreInteractionSnapshotSkipCountForDiagnostics =
                        participantCount * 3;
                    LastPreInteractionCpointCheckProofSkipCountForDiagnostics =
                        participantCount;
                    LastPreInteractionMismatchTailProofSkipCountForDiagnostics =
                        participantCount;
                    return;
                }

                GetActiveEntitiesByRuntimeSlot(_entityScratch);
                if (_entityScratch.Count == 0) return;

                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity?.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    LastPreInteractionScannedCountForDiagnostics++;
                    LastPreInteractionExecutedCountForDiagnostics++;
                    entity.RunCpointCheckStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }

                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity?.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    LastPreInteractionScannedCountForDiagnostics++;
                    LastPreInteractionExecutedCountForDiagnostics++;
                    entity.RunCpointMismatchTailStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }

                _entityScratch.Clear();

                // Keep the authority live ascending scan without allocating a
                // tick-capturing delegate. Newborns above the cursor join this
                // pass, while a recycled lower slot waits for the next pass.
                for (int runtimeSlot = 0;
                     runtimeSlot < _runtimeSlots.LogicalCapacity;
                     runtimeSlot++)
                {
                    LF2Entity entity = _runtimeSlots.GetCurrentOccupant(runtimeSlot);
                    if (entity == null)
                        continue;
                    if (entity.Runtime != null && tickIndex < entity.Runtime.SuppressPreInteractionUntilTick)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    LastPreInteractionScannedCountForDiagnostics++;
                    LastPreInteractionExecutedCountForDiagnostics++;
                    entity.RunWeaponSyncHeldStep10();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }
            }
            finally
            {
                _entityScratch.Clear();
                _ticking = false;
                FlushPendingUnregister();
                FlushPendingEntityDestroy();
            }
        }

        private bool TryProveWholePreInteractionPassNoOp(
            int tickIndex,
            out int participantCount)
        {
            participantCount = 0;
            int logicalCapacity = _runtimeSlots.LogicalCapacity;
            int claimedCount = _runtimeSlots.ClaimedCount;
            ulong occupancyEpoch = _runtimeSlots.OccupancyEpoch;
            long pendingDestroyEpoch =
                runtimeMutationTracker.PendingFlushDestroyEpoch;
            int pendingUnregisterCount = _pendingUnregister.Count;

            for (int runtimeSlot = 0; runtimeSlot < logicalCapacity; runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                if (!view.Claimed)
                {
                    if (view.Entity != null)
                        return false;
                    continue;
                }

                LF2Entity entity = view.Entity;
                if (entity == null ||
                    view.Generation == 0 ||
                    entity.Runtime == null ||
                    entity.Runtime.SlotIndex != runtimeSlot)
                {
                    return false;
                }

                var handle =
                    new RuntimeEntityHandle(runtimeSlot, view.Generation);
                if (!_runtimeSlots.TryResolve(handle, out LF2Entity resolved) ||
                    !ReferenceEquals(resolved, entity))
                {
                    return false;
                }

                if (entity.Runtime.SuppressPreInteractionUntilTick > tickIndex ||
                    !IsActiveForCurrentPass(entity))
                {
                    continue;
                }

                participantCount++;
                if (!TryProveNeutralPreInteractionParticipant(entity))
                    return false;
            }

            return logicalCapacity == _runtimeSlots.LogicalCapacity &&
                   claimedCount == _runtimeSlots.ClaimedCount &&
                   occupancyEpoch == _runtimeSlots.OccupancyEpoch &&
                   pendingDestroyEpoch ==
                        runtimeMutationTracker.PendingFlushDestroyEpoch &&
                   pendingUnregisterCount == _pendingUnregister.Count;
        }

        private static bool TryProveNeutralPreInteractionParticipant(
            LF2Entity entity)
        {
            if (entity == null || entity.GetType() != typeof(LF2Character))
                return false;
            if (!entity.IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp())
                return false;

            var character = (LF2Character)entity;
            NTSDEntityRuntime runtime = character.Runtime;
            if (runtime.LinkState != 0 ||
                runtime.TargetSlotIndex != -1 ||
                runtime.HeldWeaponStableId != -1 ||
                character.HeldWeaponReferenceInternal != null)
            {
                return false;
            }

            CatchPoint collisionCpoint =
                character.GetCollisionFrameData()?.cpoint;
            if (collisionCpoint != null &&
                (collisionCpoint.kind == 1 || collisionCpoint.kind == 2))
            {
                return false;
            }

            CatchPoint currentCpoint = character.Frame?.D?.cpoint;
            return currentCpoint == null ||
                   (currentCpoint.kind != 1 && currentCpoint.kind != 2);
        }

        public void RandomWeaponDropTickAll(int tickIndex)
        {
            int weaponCount = 0;
            foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
            {
                if (entity.CountsAsRandomWeaponDropCandidate())
                    weaponCount++;
            }
            if (weaponCount >= 4) return;
            if (Rng.NextInt(0, 200) != 0) return;

            int freeSlot = FindFirstFreeRuntimeSlot(DynamicRuntimeSlotStart, RuntimeSlotCapacity);
            if (freeSlot < 0) return;

            var manager = CharacterAnimtorManager.Instance;
            var dataManager = GameDataManager.Instance;
            if (manager == null || dataManager == null) return;

            SimulationRandomWeaponDropBuffer candidates = randomWeaponDropBuffer;
            candidates.Reset();
            List<ObjectDefinition> loadedObjects = dataManager.GetAllObjects();
            for (int i = 0; i < loadedObjects.Count; i++)
            {
                int oid = loadedObjects[i].id;
                if (!candidates.TryMarkUnique(oid)) continue;
                var wrapper = manager.GetCharacterConfig(oid);
                if (wrapper == null) continue;
                if (oid == 122 || oid == 123)
                {
                    if (Rng.NextInt(0, 2) == 0 ||
                        (BattleGameModeId >= 1 && BattleGameModeId <= 4))
                        continue;
                }
                candidates.TryAdd(oid);
            }
            if (candidates.Count == 0) return;

            int selectedOid = candidates[Rng.NextInt(0, candidates.Count)];
            var factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (factory == null || referencePool == null) return;

            BattleStageRuntimeState stage = Runtime?.Stage;
            int xMaxOverride = stage?.XMaxOverride ?? 0;
            int stageWidth = stage?.BaseStageWidthPx ?? 800;
            int zMin = stage?.ZMin ?? 180;
            int zMax = stage?.ZMax ?? 350;
            int r1 = Rng.NextInt(0, 30);
            int xBase = xMaxOverride == 0 ? stageWidth - 60 : xMaxOverride - 60;
            int xStep = xBase / 30;
            int r2 = Rng.NextInt(0, 30);
            int r3 = Rng.NextInt(0, 30);
            int zBase = zMax - zMin - 60;
            int zStep = zBase / 30;
            int r4 = Rng.NextInt(0, 30);
            double lf2X = r1 * xStep + r2 + 30;
            double lf2Z = r3 * zStep + r4 + zMin + 30;
            const double lf2Y = -500.0;

            OPointCreateTask spawnTask = referencePool.Fetch<OPointCreateTask>();
            if (spawnTask == null)
                return;
            spawnTask.opoint = new ObjectPoint
            {
                oid = selectedOid,
                kind = 0,
                action = 0,
                x = (int)lf2X,
                y = (int)lf2Y,
                dvx = 0,
                dvy = 0,
                facing = 0,
            };
            spawnTask.parent = null;
            spawnTask.team = 0;
            spawnTask.requiredRuntimeSlot = freeSlot;
            spawnTask.pos = new Vector3((float)lf2X, (float)lf2Y, 0f);
            spawnTask.z = (float)lf2Z;
            spawnTask.dir = "right";
            spawnTask.dvz = 0f;
            spawnTask.preserveActionZero = true;
            spawnTask.skipPostInitZOffset = true;
            spawnTask.useDirectRuntimePosition = true;
            spawnTask.directX = lf2X;
            spawnTask.directY = lf2Y;
            spawnTask.directZ = lf2Z;
            spawnTask.useDirectVelocity = true;
            spawnTask.directVx = 0.0;
            spawnTask.directVy = 0.0;
            spawnTask.directVz = 0.0;
            spawnTask.useInitialRuntimeIntPosition = true;
            spawnTask.initialRuntimeX = (int)lf2X;
            spawnTask.initialRuntimeY = (int)lf2Y;
            spawnTask.initialRuntimeZ = (int)lf2Z;

            LF2Entity spawned;
            try
            {
                spawned = factory.CreateObjectImmediate(spawnTask);
            }
            finally
            {
                referencePool.Recycle(spawnTask);
            }

            if (spawned == null || spawned.Runtime?.SlotIndex != freeSlot) return;

            spawned.Health.HP = selectedOid == 122 ? 200 : 500;
            spawned.Health.HPBound = 500;
            spawned.Health.HP3 = 500;
            spawned.Health.PP = 500;
            spawned.KillCount = -1;
            ResetCooldownsForRuntimeSlot(freeSlot);
            spawned.RefreshRuntimeSnapshot();
        }

        private void ResetCooldownsForRuntimeSlot(int runtimeSlot)
        {
            ResetCooldownsForRuntimeSlot(
                runtimeSlot,
                FindEntityByRuntimeSlotIncludingDormant(runtimeSlot));
        }

        public void Mode2RandomWeaponDropTailAll(int tickIndex)
        {
            int mode2Request = Mode2Request;
            if (mode2Request == 0)
                return;

            if (mode2Request == 1)
            {
                SpawnMode2RandomWeapons();
            }
            else if (mode2Request == 2)
            {
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    if (!entity.CountsAsRandomWeaponDropCandidate())
                        continue;

                    entity.Runtime.WeaponFlightCounter = -1;
                    RefreshRuntimeSnapshot(entity);
                }
            }

            SetMode2Request(0);
        }

        private void SpawnMode2RandomWeapons()
        {
            var manager = CharacterAnimtorManager.Instance;
            if (manager == null)
                return;

            SimulationRandomWeaponDropBuffer candidates = randomWeaponDropBuffer;
            candidates.Reset();
            for (int oid = 100; oid < 200; oid++)
            {
                var wrapper = manager.GetCharacterConfig(oid);
                if (wrapper == null)
                    continue;

                if (oid == 122 && Rng.NextInt(0, 2) == 0)
                    continue;

                candidates.TryAdd(oid);
            }

            if (candidates.Count == 0)
                return;

            ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out _, out _);
            if (stageWidth <= 60 || zMax - zMin <= 60)
                return;

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            for (int chooseIndex = 0; chooseIndex < candidates.Count; chooseIndex++)
            {
                int oid = candidates[chooseIndex];

                bool hasFreeSlot = false;
                for (int slot = DynamicRuntimeSlotStart; slot < RuntimeSlotCapacity; slot++)
                {
                    if (!_runtimeSlots.IsClaimed(slot))
                    {
                        hasFreeSlot = true;
                        break;
                    }
                }

                if (!hasFreeSlot)
                    break;

                int r1 = Rng.NextInt(0, 30);
                int r2 = Rng.NextInt(0, 30);
                int r3 = Rng.NextInt(0, 30);
                int r4 = Rng.NextInt(0, 30);
                float lf2X = r1 * ((stageWidth - 60) / 30) + r2 + 30;
                float lf2Z = r3 * ((zMax - zMin - 60) / 30) + r4 + zMin + 30;
                const float lf2Y = -500f;

                var charData = CharacterAnimtorManager.Instance?.GetCharacterData(oid);
                int flyFrame = -1;
                int minFrame = int.MaxValue;
                if (charData?.frames != null)
                {
                    foreach (var f in charData.frames)
                    {
                        if (f == null)
                            continue;
                        if (f.frameId > 0 && f.frameId < minFrame)
                            minFrame = f.frameId;
                        if (flyFrame < 0 && f.frameId > 0 &&
                            (f.state == LF2States.WeaponInSky ||
                             f.state == LF2States.WeaponThrowing ||
                             f.state == LF2States.HeavyWeaponInSky))
                        {
                            flyFrame = f.frameId;
                        }
                    }
                }

                if (flyFrame < 0)
                    flyFrame = minFrame != int.MaxValue ? minFrame : 0;

                LF2ReferencePool referencePool = LF2ReferencePool.Instance;
                if (referencePool == null)
                    break;

                OPointCreateTask spawnTask = referencePool.Fetch<OPointCreateTask>();
                if (spawnTask == null)
                    break;
                spawnTask.opoint = new ObjectPoint
                {
                    oid = oid,
                    kind = 0,
                    action = flyFrame,
                    x = Mathf.RoundToInt(lf2X),
                    y = Mathf.RoundToInt(lf2Y),
                    dvx = 0,
                    dvy = 0,
                    facing = 0,
                };
                spawnTask.parent = null;
                spawnTask.team = 0;
                spawnTask.pos = new Vector3(lf2X, lf2Y, 0f);
                spawnTask.z = lf2Z;
                spawnTask.dir = "right";
                spawnTask.dvz = 0f;
                try
                {
                    factory.CreateObjectImmediate(spawnTask);
                }
                finally
                {
                    referencePool.Recycle(spawnTask);
                }
            }
        }

#if UNITY_INCLUDE_TESTS
        internal int[] CaptureLateRuntimeSnapshotBoundaryForSelfCheck(int mode)
        {
            return CaptureLateRuntimeSnapshotBoundaryForModeForSelfCheck(
                mode,
                (int)BattleLateRuntimeSnapshotMode.LegacyThree);
        }

        internal int[] CaptureLateRuntimeSnapshotBoundaryForModeForSelfCheck(
            int mode,
            int snapshotMode)
        {
            if (snapshotMode < (int)BattleLateRuntimeSnapshotMode.LegacyThree ||
                snapshotMode > (int)BattleLateRuntimeSnapshotMode.ConsolidatedFinal)
            {
                throw new System.ArgumentOutOfRangeException(nameof(snapshotMode));
            }

            LateRuntimeSnapshotModeForDiagnostics =
                (BattleLateRuntimeSnapshotMode)snapshotMode;
            LF2Entity entity;
            LateRuntimeSnapshotProbe probe = null;
            LateRuntimeSnapshotWeaponProbe weapon = null;
            if (mode == 3)
            {
                weapon = new LateRuntimeSnapshotWeaponProbe();
                weapon.BindData();
                entity = weapon;
            }
            else
            {
                probe = new LateRuntimeSnapshotProbe(
                    zeroHpDuringRecovery: mode == 0,
                    cleanupCompleted: mode == 2);
                entity = probe;
            }

            Register(entity);
            if (mode == 1)
                entity.Runtime.SuppressLateFrameTickUntilTick = 2;
            if (mode == 4 || mode == 5)
            {
                int exitFrame = mode == 4 ? 1100 : 1200;
                entity.Frame.N = exitFrame;
                entity.Runtime.Frame = exitFrame;
            }

            BattleTickDetailPhaseDiagnostics diagnostics =
                EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
            diagnostics.BeginTick(1);
            LateEntityUpdateAll(1);

            return new[]
            {
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.Recovery),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameTickSuppressed),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.CleanupCompleted),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameTick),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.DeathOpoint),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.TailAndQueuedFlush),
                probe?.RecoveryCount ?? 0,
                probe?.FrameTickCount ?? 0,
                probe?.FrameTickObservedHp ?? 0,
                probe?.DeathOpointCount ?? 0,
                probe?.DeathOpointObservedHp ?? 0,
                probe?.CleanupCount ?? 0,
                probe?.TailCount ?? 0,
                ObjectCount,
                weapon?.PendingDestroyObserved == true ? 1 : 0,
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameExit),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.TransitionInternal),
                entity.Runtime?.Frame ?? -1,
            };
        }

        private sealed class LateRuntimeSnapshotProbe : LF2Entity
        {
            private readonly bool zeroHpDuringRecovery;
            private readonly bool cleanupCompleted;

            internal int RecoveryCount { get; private set; }
            internal int FrameTickCount { get; private set; }
            internal int FrameTickObservedHp { get; private set; }
            internal int DeathOpointCount { get; private set; }
            internal int DeathOpointObservedHp { get; private set; }
            internal int CleanupCount { get; private set; }
            internal int TailCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum =>
                LF2ObjectType.Character;
            internal override bool UsesDynamicRuntimeSlot() => true;

            internal LateRuntimeSnapshotProbe(
                bool zeroHpDuringRecovery,
                bool cleanupCompleted)
            {
                this.zeroHpDuringRecovery = zeroHpDuringRecovery;
                this.cleanupCompleted = cleanupCompleted;
                Name = "LateRuntimeSnapshotProbe";
                ObjectId = 1;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                Health.HP = 100;
                Health.HPBound = 100;
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                var frame = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    wait = 1,
                    next = 0,
                    centerx = 0,
                    centery = 0,
                };
                FrameCache.Load(new LF2CharacterDataWrapper(
                    ObjectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = (int)LF2ObjectType.Character,
                        frames = new List<LF2FrameData> { frame },
                    }));
                Frame.D = frame;
                Frame.N = 0;
                Frame.PN = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
            }

            internal override void RunPreCollisionRecoveryPhase(int tickIndex)
            {
                RecoveryCount++;
                if (zeroHpDuringRecovery)
                    Health.HP = 0;
            }

            public override void SimFrameTick(int tickIndex)
            {
                FrameTickCount++;
                FrameTickObservedHp = Runtime.HP;
            }

            internal override void RunLateDeathOpointPreCleanupPhase()
            {
                DeathOpointCount++;
                DeathOpointObservedHp = Runtime.HP;
            }

            internal override bool TryRunLatePostOpointCleanupPhase()
            {
                CleanupCount++;
                return cleanupCompleted;
            }

            internal override void RunLateTailBeforePrevFrame()
            {
                TailCount++;
            }

            public override void Reset()
            {
            }

            public override void Init(
                LF2TaskBase task,
                LF2ObjectRenderer renderer)
            {
            }
        }

        private sealed class LateRuntimeSnapshotWeaponProbe : LF2Weapon
        {
            internal bool PendingDestroyObserved { get; private set; }

            internal void BindData()
            {
                Name = "LateRuntimeSnapshotDepletedWeapon";
                ObjectId = 100;
                SetWeaponType((int)LF2ObjectType.LightWeapon);
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                var frame = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    wait = 100,
                    next = 0,
                    centerx = 0,
                    centery = 0,
                };
                FrameCache.Load(new LF2CharacterDataWrapper(
                    ObjectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = 100,
                        weapon_hp = 1,
                        weapon_broken_sound = "LateSnapshot_Depleted",
                        frames = new List<LF2FrameData> { frame },
                    }));
                Frame.D = frame;
                Frame.PN = 0;
                Frame.N = 0;
                Frame.Prev = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 1;
                Health.HPBound = 1;
                Runtime.WeaponFlightCounter = -1;
            }

            internal override bool TryRunLatePostOpointCleanupPhase()
            {
                bool completed = base.TryRunLatePostOpointCleanupPhase();
                PendingDestroyObserved |= Runtime.PendingFlushDestroy;
                return completed;
            }
        }
#endif
    }
}
