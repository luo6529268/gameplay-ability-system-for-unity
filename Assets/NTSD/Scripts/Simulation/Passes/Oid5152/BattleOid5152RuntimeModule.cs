using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;

namespace NTSD.Simulation
{
    /// <summary>Record-driven fusion transactions; World retains the existing pass schedule.</summary>
    internal sealed class BattleOid5152RuntimeModule
    {
        private static readonly LoganFusionCatalog LegacyFusionCatalog = LoganFusionCatalogInput.CreateLockedFallback();
        private readonly SimulationWorld world;

        static BattleOid5152RuntimeModule()
        {
        }

        internal BattleOid5152RuntimeModule(SimulationWorld world)
        {
            this.world = world;
        }

        internal void RunMaintenance(int tickIndex) => RunFusionScan(tickIndex, true);
        internal void RunFusionScan(int tickIndex) => RunFusionScan(tickIndex, false);

        internal void AdvanceReactionTimer(LF2Entity entity)
        {
            if (entity?.Runtime == null || entity.Runtime.Unk338 <= 0)
                return;
            entity.Runtime.Unk338--;
            world.RefreshRuntimeSnapshotForModule(entity);
        }

        private void RunFusionScan(int tickIndex, bool advanceTimerBeforeFusion)
        {
            LoganFusionCatalog catalog = world.RuntimeDataCatalog.FusionCatalog ?? LegacyFusionCatalog;
            if (!catalog.IsValid) return;
            world.BeginDeferredEntityMutationPass();
            try
            {
                if (advanceTimerBeforeFusion)
                    for (int slot = 0; slot < 20; slot++)
                    {
                        var entity = ActiveEntity(slot);
                        if (entity != null) AdvanceReactionTimer(entity);
                    }
                // Alignment contract: NTSD28-Q06-FUSION-RECORD-TRANSACTION-001.
                for (int recordIndex = 0; recordIndex < catalog.Records.Count; recordIndex++)
                {
                    LoganFusionRecord record = catalog.Records[recordIndex];
                    bool bypass = record.Respond == 1 ? world.Runtime.FusionFirstFeatureGate4A8428 :
                        record.Respond == 2 ? world.Runtime.FusionSecondFeatureGate4A842C :
                        record.Respond == 3 && world.Runtime.FusionFirstFeatureGate4A8428 && world.Runtime.FusionSecondFeatureGate4A842C;
                    for (int slot = 0; slot < 20; slot++)
                    {
                        LF2Entity primary = ActiveEntity(slot);
                        if (primary?.Health == null || primary.Runtime.NativeLifecycleResolutionPending ||
                            primary.GetCurrentDataObjectTypeForSimulation() != 0)
                            continue;
                        if (slot < 10 && TryMerge(primary, slot, record, bypass)) continue;
                        TrySplit(primary, record);
                    }
                }
            }
            finally { world.EndDeferredEntityMutationPass(); }
        }

        private LF2Entity ActiveEntity(int slot)
        {
            LF2Entity entity = world.GetCurrentRuntimeSlotOccupantForInteractionModule(slot);
            return entity?.Runtime == null || entity.Runtime.OidMergeDormant ? null : entity;
        }

        private static int FrameState(LF2Entity entity) => entity.FrameCache.GetNativeFrameDataById(entity.Frame.N)?.state ?? 0;

        private bool TryResolve(int id, int action, out LF2CharacterDataWrapper wrapper)
        {
            wrapper = world.RuntimeDataCatalog.IsReady
                ? world.RuntimeDataCatalog.GetCharacterConfig(id) : world.RuntimeCharacterConfigs.Resolve(id);
            if (wrapper?.characterData == null || wrapper.characterId != id || (uint)action >= LF2FrameCache.NativeMaxFrameIdExclusive)
                return false;
            if (world.RuntimeDataCatalog.IsReady && world.RuntimeDataCatalog.GetObjectDefinition(id) == null)
                return false;
            if (action < 999) return true;
            var frames = wrapper.characterData.frames;
            if (frames != null)
                foreach (LF2FrameData frame in frames)
                    if (frame != null && frame.frameId == action) return true;
            return false;
        }

        private bool TryMerge(LF2Entity primary, int primarySlot, LoganFusionRecord record, bool bypass)
        {
            if ((primary.ObjectId != record.Id1 && primary.ObjectId != record.Id2) || primary.Health.HP <= 0 ||
                FrameState(primary) != record.State || (record.Cover != 1 && primary.Runtime.Unk338 != 0) ||
                !(primary.Health.HP < record.Hp || bypass))
                return false;
            int partnerId = unchecked(record.Id1 + record.Id2 - primary.ObjectId);
            for (int slot = 0; slot < 20; slot++)
            {
                LF2Entity partner = ActiveEntity(slot);
                if (slot == primarySlot || partner?.Health == null || partner.Runtime.NativeLifecycleResolutionPending ||
                    partner.GetCurrentDataObjectTypeForSimulation() != 0 || partner.ObjectId != partnerId ||
                    partner.Health.HP <= 0 || partner.RelationTeam != primary.RelationTeam ||
                    (record.Cover != 1 && partner.Runtime.Unk338 != 0) || !(partner.Health.HP < record.Hp || bypass))
                    continue;
                int state = FrameState(partner);
                if (!(state == record.State || (state != 14 && slot > 9 && partner.GetRuntimeYInt() == partner.Runtime.CollisionYReference)))
                    continue;
                int x = primary.GetRuntimeXInt(), px = partner.GetRuntimeXInt();
                int z = primary.GetRenderZInt(), pz = partner.GetRenderZInt();
                bool sourceHistoryComplete = primary.Runtime.SourceRulePositionInitialized &&
                    partner.Runtime.SourceRulePositionInitialized;
                int ruleX = sourceHistoryComplete ? primary.Runtime.SourceRuleXInt : x;
                int rulePartnerX = sourceHistoryComplete ? partner.Runtime.SourceRuleXInt : px;
                int ruleZ = sourceHistoryComplete ? primary.Runtime.SourceRuleZInt : z;
                int rulePartnerZ = sourceHistoryComplete ? partner.Runtime.SourceRuleZInt : pz;
                // Alignment contract: NTSD28-USER-SOURCE-FUSION-DISTANCE-001.
                if (Math.Abs((long)ruleX - rulePartnerX) >= 50 ||
                    Math.Abs((long)ruleZ - rulePartnerZ) >= 8 ||
                    (slot <= 9 && ruleX <= rulePartnerX))
                    continue;
                if (!TryResolve(record.Id3, record.Action, out var fused)) continue;

                int bound = Math.Min(unchecked(primary.Health.HPBound + partner.Health.HPBound), primary.Health.HP3);
                primary.Health.HP = Math.Min(unchecked(primary.Health.HP + partner.Health.HP), bound);
                primary.Health.HPBound = bound;
                primary.Runtime.Unk328 = 1;
                if (record.HitJa == 1) primary.Runtime.InputSpecialGate194 = 1;
                primary.Runtime.RenderPicOffset = 0;
                primary.Runtime.Vx = 0;
                partner.Runtime.Vy = 0;
                int midpointX = unchecked(x + px) / 2, midpointZ = unchecked(z + pz) / 2;
                primary.Runtime.X = primary.Runtime.XInt = midpointX;
                primary.Runtime.Z = primary.Runtime.ZInt = midpointZ;
                if (sourceHistoryComplete)
                {
                    int sourceMidpointX = unchecked(ruleX + rulePartnerX) / 2;
                    int sourceMidpointZ = unchecked(ruleZ + rulePartnerZ) / 2;
                    primary.Runtime.SourceRuleX = primary.Runtime.SourceRuleXInt = sourceMidpointX;
                    primary.Runtime.SourceRuleZ = primary.Runtime.SourceRuleZInt = sourceMidpointZ;
                }
                primary.Runtime.Unk32C = slot;
                primary.Runtime.Unk338 = record.Decrease;
                primary.Runtime.FusionDisplayTimer190 = record.Decrease;
                primary.Runtime.Unk330 = primary.ObjectId;
                primary.Runtime.Unk334 = partner.ObjectId;
                PublishDefinition(primary, fused);
                SetAction(primary, record.Action);
                primary.Health.PP = record.Mp;
                world.InvalidateAiUnifiedRowMembershipForModule();
                partner.Runtime.OidMergeDormant = true;
                primary.RefreshRuntimeSnapshot();
                partner.RefreshRuntimeSnapshot();
                return true;
            }
            return false;
        }

        private void TrySplit(LF2Entity primary, LoganFusionRecord record)
        {
            int action = primary.Frame.N;
            if (primary.ObjectId != record.Id3 || primary.Runtime.Unk328 != 1 || primary.Runtime.Unk338 > 0 ||
                (action >= record.FrontHurtAction && action <= record.BackHurtAction)) return;
            int slot = primary.Runtime.Unk32C;
            if (slot < 0 || slot >= world.RuntimeSlotCapacity) return;
            LF2Entity partner = world.FindEntityByRuntimeSlotIncludingDormant(slot);
            if (partner?.Runtime == null || partner.Health == null || !partner.Runtime.OidMergeDormant ||
                !TryResolve(primary.Runtime.Unk330, record.Frame, out var original) ||
                !TryResolve(primary.Runtime.Unk334, record.Frame, out var other)) return;

            // A suspended native slot stays reserved; restore its saved entity without pool Reset.
            world.InvalidateAiUnifiedRowMembershipForModule();
            partner.Runtime.OidMergeDormant = false;
            PublishDefinition(primary, original);
            PublishDefinition(partner, other);
            primary.Runtime.Unk328 = -1;
            primary.Runtime.InputSpecialGate194 = 0;
            primary.Runtime.Unk338 = record.Wait;
            int hp = record.Chp == 1 ? primary.Health.HP : primary.Health.HP / 2;
            int bound = record.Chp == 1 ? primary.Health.HPBound : primary.Health.HPBound / 2;
            primary.Health.HP = partner.Health.HP = hp;
            primary.Health.HPBound = partner.Health.HPBound = bound;
            partner.Runtime.X = primary.Runtime.X;
            partner.Runtime.Y = primary.Runtime.Y;
            partner.Runtime.Z = primary.Runtime.Z;
            partner.Runtime.XInt = primary.Runtime.XInt;
            partner.Runtime.YInt = primary.Runtime.YInt;
            partner.Runtime.ZInt = primary.Runtime.ZInt;
            partner.Runtime.SourceRulePositionInitialized =
                primary.Runtime.SourceRulePositionInitialized;
            if (primary.Runtime.SourceRulePositionInitialized)
            {
                partner.Runtime.SourceRuleX = primary.Runtime.SourceRuleX;
                partner.Runtime.SourceRuleZ = primary.Runtime.SourceRuleZ;
                partner.Runtime.SourceRuleXInt = primary.Runtime.SourceRuleXInt;
                partner.Runtime.SourceRuleZInt = primary.Runtime.SourceRuleZInt;
            }
            partner.Runtime.CollisionYReference = primary.Runtime.CollisionYReference;
            primary.Runtime.Vx = partner.Runtime.Vx = partner.Runtime.Vy = 0;
            partner.SwitchDir(primary.Runtime.Dir == "right" ? "left" : "right");
            SetAction(primary, record.Frame);
            SetAction(partner, record.Frame);
            primary.Health.PP = partner.Health.PP = 0;
            partner.RelationTeam = primary.RelationTeam;
            primary.RefreshRuntimeSnapshot();
            partner.RefreshRuntimeSnapshot();
        }

        private static void PublishDefinition(LF2Entity entity, LF2CharacterDataWrapper wrapper)
        {
            entity.ObjectId = wrapper.characterId;
            entity.FrameCache.Load(wrapper);
            if (entity.GetCurrentDataObjectTypeForSimulation() == 0)
                entity.EnsureSharedCharacterDatControllerForSimulation();
            entity.InitializeNativeDefinitionIdentityForSpawn();
            var data = wrapper.characterData;
            var stats = data.NativeMetadata?.Stats;
            entity.Health.MaxMP = stats?.Int32OrDefault("max_mp", entity.Health.MaxMP) ?? entity.Health.MaxMP;
            entity.Runtime.WeaponFlightCounter = data.NativeMetadata?.Bmp.Int32OrDefault("weapon_hp", 0) ?? data.weapon_hp;
            int scale = stats?.Int32OrDefault("defend", 0) ?? 0;
            if (scale <= 0) scale = 0;
            if (entity.Runtime.ModeDamageScalePercent != 100)
                scale = scale == 0 ? entity.Runtime.ModeDamageScalePercent :
                    unchecked((int)((long)entity.Runtime.ModeDamageScalePercent * scale / 100));
            entity.Runtime.IncomingDamageScale340 = scale;
        }

        private static void SetAction(LF2Entity entity, int action)
        {
            entity.WriteCurrentFrameId(action);
            LF2FrameData frame = entity.FrameCache.GetNativeFrameDataById(action);
            entity.Frame.D = frame;
            entity.Trans.SyncDirectFrameData(frame.wait, frame.next, action);
            entity.AttackingCounter = 0;
            entity.Frame.Prev2 = action;
            entity.Frame.Prev2D = frame;
            entity.Runtime.PrevFrame2 = action;
            entity.Runtime.NativeSoundActionLatch = -1;
        }
    }
}
