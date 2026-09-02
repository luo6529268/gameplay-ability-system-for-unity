using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the release-compatible OID 7/8 to 51 merge and split lifecycle.
    /// World retains only the stable scheduling façade.
    /// </summary>
    internal sealed class BattleOid5152RuntimeModule
    {
        private readonly SimulationWorld world;

        internal BattleOid5152RuntimeModule(SimulationWorld world)
        {
            this.world = world;
        }

        internal void RunMaintenance(int tickIndex)
        {
            world.BeginDeferredEntityMutationPass();
            try
            {
                for (int runtimeSlot = 0; runtimeSlot < 20; runtimeSlot++)
                {
                    LF2Entity entity =
                        world.FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                    if (entity == null ||
                        !world.IsActiveForCurrentPassInternal(entity))
                    {
                        continue;
                    }

                    if (entity.Runtime.Unk338 > 0)
                    {
                        entity.Runtime.Unk338--;
                        world.RefreshRuntimeSnapshotForModule(entity);
                    }

                    if (entity.ObjectId == 51)
                    {
                        TrySplitOid51BackToPair(entity);
                    }
                    else if (entity.ObjectId == 7 || entity.ObjectId == 8)
                    {
                        TryMergeOid7Or8Into51(entity);
                    }
                }
            }
            finally
            {
                world.EndDeferredEntityMutationPass();
            }
        }

        private bool TryMergeOid7Or8Into51(LF2Entity self)
        {
            if (self?.Runtime == null || self.Health == null)
                return false;

            int selfSlot = self.Runtime.SlotIndex;
            LF2FrameData selfFrame = self.Frame?.D;
            if (selfSlot < 0 ||
                selfSlot >= 10 ||
                selfFrame == null ||
                selfFrame.state != 2)
            {
                return false;
            }
            if (self.Health.HP <= 0 || self.Runtime.Unk338 != 0)
                return false;
            if (!PassesHpGate(self))
                return false;

            LF2CharacterDataWrapper oid51Wrapper =
                world.RuntimeCharacterConfigs.Resolve(51);
            if (oid51Wrapper == null)
                return false;

            int selfX = self.GetRuntimeXInt();
            int selfZ = self.GetRenderZInt();
            int selfRelationTeam = ResolveRelationTeam(self);
            int partnerOid = 15 - self.ObjectId;

            for (int partnerSlot = 0; partnerSlot < 20; partnerSlot++)
            {
                if (partnerSlot == selfSlot)
                    continue;

                LF2Entity partner =
                    world.FindEntityByRuntimeSlotForQuery(partnerSlot);
                if (partner?.Runtime == null || partner.Health == null)
                    continue;
                if (partner.ObjectId != partnerOid ||
                    partner.Health.HP <= 0 ||
                    partner.Runtime.Unk338 != 0)
                {
                    continue;
                }
                if (!PassesHpGate(partner))
                    continue;
                if (ResolveRelationTeam(partner) != selfRelationTeam)
                    continue;

                LF2FrameData partnerFrame = partner.Frame?.D;
                int partnerFrameId = partner.Frame?.N ?? -1;
                if (partnerFrame == null ||
                    partnerFrameId < 0 ||
                    partnerFrameId >= LF2FrameCache.MaxFrameIdExclusive)
                {
                    continue;
                }
                if (partnerFrame.state == 14)
                    continue;
                if (partnerFrame.state != 2 &&
                    (partner.GetRuntimeYInt() != 0 || partnerSlot <= 9))
                {
                    continue;
                }

                int partnerX = partner.GetRuntimeXInt();
                int partnerZ = partner.GetRenderZInt();
                if (Mathf.Abs(selfX - partnerX) >= 50 ||
                    Mathf.Abs(selfZ - partnerZ) >= 8)
                {
                    continue;
                }
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
                // Alignment contract: R8-AIROWGEN-001. Dormancy changes the
                // unified snapshot Included set without releasing this handle.
                world.InvalidateAiUnifiedRowMembershipForModule();
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
            if (self.ObjectId != 51 ||
                self.Runtime.Unk328 != 1 ||
                self.Runtime.Unk338 > 0)
            {
                return false;
            }

            int currentFrameId = self.Frame?.N ?? -1;
            if (currentFrameId >= 9 && currentFrameId <= 260)
                return false;

            int originalOid = self.Runtime.Unk330;
            if (world.RuntimeCharacterConfigs.Resolve(originalOid) == null)
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

            LF2Entity partner =
                world.FindEntityByRuntimeSlotIncludingDormant(partnerSlot);
            if (partner == null ||
                world.RuntimeCharacterConfigs.Resolve(partnerOid) == null)
            {
                return true;
            }

            int halfHp = aggregateHp / 2;
            int halfHpBound = aggregateHpBound / 2;
            int partnerStableId = partner.Runtime.StableId;
            int partnerRuntimeSlot = partner.Runtime.SlotIndex;

            // Alignment contract: R8-AIROWGEN-001. The dormant partner is not
            // present in the active unified row set. End that publication before
            // reset writes through the still-bound original-generation stores.
            world.InvalidateAiUnifiedRowMembershipForModule();

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

            // LF2Character.Reset has pool-specific defaults that differ from
            // formal Entity::reset.
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
            partner.RestoreStableIdAfterLifecycleReset(partnerStableId);
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

        private bool PassesHpGate(LF2Entity entity)
        {
            if (entity?.Health == null || entity.Health.HP <= 0)
                return false;

            return world.BattleGameModeId == 1 || entity.Health.HP < 177;
        }

        private static int ResolveRelationTeam(LF2Entity entity)
        {
            return entity?.RelationTeam ?? 0;
        }
    }
}
