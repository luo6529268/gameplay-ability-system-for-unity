using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 持有的 stage 波次推进与刷怪模块。
    /// </summary>
    internal sealed class SimulationStageWaveModule
    {
        private readonly SimulationWorld world;
        private readonly List<LF2Entity> entityScratch = new List<LF2Entity>(128);

        internal SimulationStageWaveModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        private List<BattleStageCampaignData> StageCampaigns => world.StageCampaigns;
        private BattleStageProgressionState StageProgression => world.StageProgression;
        private bool StageProgressionValid => world.StageProgressionValid;
        private int StageSpawnWaveApplied => world.StageSpawnWaveApplied;
        private int StageSpawnWaveDeferredEntryApplied =>
            world.StageSpawnWaveDeferredEntryApplied;
        private int StageSpawnRuntimeWave => world.StageSpawnRuntimeWave;
        private List<int> StageSpawnRuntimeTargetTotal =>
            world.StageSpawnRuntimeTargetTotal;
        private List<int> StageSpawnRuntimeEntryCount =>
            world.StageSpawnRuntimeEntryCount;
        private List<int> StageSpawnRuntimeSpawnedTotal =>
            world.StageSpawnRuntimeSpawnedTotal;
        private List<int[]> StageSpawnRuntimeSlots => world.StageSpawnRuntimeSlots;
        private BattleRuntimeState Runtime => world.Runtime;
        private int BattleGameModeId => world.BattleGameModeId;
        private int RuntimeSlotCapacity => world.RuntimeSlotCapacity;
        private DeterministicRng Rng => world.Rng;
        private RuntimeCharacterConfigResolver RuntimeCharacterConfigs =>
            world.RuntimeCharacterConfigs;
        private BattleRuntimeDataCatalog RuntimeDataCatalog =>
            world.RuntimeDataCatalog;
        private BattleLogicReferencePool LogicReferencePool =>
            world.LogicReferencePool;
        private StageSpawnTaskConfigurator StageSpawnTaskConfigurator =>
            world.StageSpawnTaskConfigurator;

        private void SetStageProgressionValid(bool value)
        {
            world.SetStageProgressionValid(value);
        }

        private void SetStageSpawnWaveApplied(int value)
        {
            world.SetStageSpawnWaveApplied(value);
        }

        private void SetStageSpawnWaveDeferredEntryApplied(int value)
        {
            world.SetStageSpawnWaveDeferredEntryApplied(value);
        }

        private void SetStageSpawnRuntimeWave(int value)
        {
            world.SetStageSpawnRuntimeWave(value);
        }

        internal void CurrentWaveStageTickAll()
        {
            ApplyCurrentWavePhaseAdvance();
            ApplyCurrentWaveImmediateStageSpawns();
        }

        internal void ConfigureStageCampaigns(
            List<BattleStageCampaignData> campaigns,
            int stageSeriesIdx,
            int initialWaveIdx)
        {
            StageCampaigns?.Clear();
            if (campaigns != null && StageCampaigns != null)
            {
                for (int i = 0; i < campaigns.Count; i++)
                {
                    if (campaigns[i] != null)
                        StageCampaigns.Add(campaigns[i]);
                }
            }

            if (StageProgression == null)
                return;

            StageProgression.StageSeriesIdx = stageSeriesIdx;
            StageProgression.WaveIdx = initialWaveIdx;
            StageProgression.Round = 0;
            StageProgression.RoundMax = 0;
            bool hasStageSeries = false;
            for (int i = 0; StageCampaigns != null && i < StageCampaigns.Count; i++)
            {
                BattleStageCampaignData stage = StageCampaigns[i];
                if (stage != null && stage.Id == stageSeriesIdx && stage.Phases != null && stage.Phases.Count > 0)
                {
                    hasStageSeries = true;
                    break;
                }
            }

            SetStageProgressionValid(hasStageSeries);
            if (StageCampaigns != null && StageCampaigns.Count > 0 && !hasStageSeries)
            {
                Debug.LogWarning(
                    $"[SimulationWorld] stage wave disabled; stage series {stageSeriesIdx} was not found in loaded campaigns");
            }
            SetStageSpawnWaveApplied(-1);
            SetStageSpawnWaveDeferredEntryApplied(-1);
            ResetStageSpawnRuntime();

            BattleStagePhaseData phase = StageProgressionCurrentPhase();
            if (phase != null && phase.Bound > 0)
                Runtime?.Stage?.ApplyPhaseBound(phase.Bound);
        }

        internal bool StartInitialStageWave()
        {
            if (!StageProgressionValid || StageProgression == null || StageProgression.WaveIdx != -1)
                return false;
            if (!StageProgressionAdvanceWave(false))
                return false;

            BattleStagePhaseData phase = StageProgressionCurrentPhase();
            if (phase != null && phase.Bound > 0)
                Runtime?.Stage?.ApplyPhaseBound(phase.Bound);

            SetStageSpawnWaveApplied(-1);
            SetStageSpawnWaveDeferredEntryApplied(-1);
            ResetStageSpawnRuntime();
            return true;
        }

        private BattleStagePhaseData StageProgressionCurrentPhase()
        {
            if (StageCampaigns == null || StageProgression == null)
                return null;

            for (int i = 0; i < StageCampaigns.Count; i++)
            {
                BattleStageCampaignData stage = StageCampaigns[i];
                if (stage == null || stage.Id != StageProgression.StageSeriesIdx)
                    continue;

                int waveIdx = StageProgression.WaveIdx;
                if (waveIdx < 0 || stage.Phases == null || waveIdx >= stage.Phases.Count)
                    return null;

                return stage.Phases[waveIdx];
            }

            return null;
        }

        private bool StageProgressionCanAdvanceWave(bool waveReady)
        {
            if (StageCampaigns == null || StageProgression == null)
                return false;

            for (int i = 0; i < StageCampaigns.Count; i++)
            {
                BattleStageCampaignData stage = StageCampaigns[i];
                if (stage == null || stage.Id != StageProgression.StageSeriesIdx)
                    continue;

                int phaseCount = stage.Phases?.Count ?? 0;
                if (StageProgression.WaveIdx >= phaseCount - 1)
                    return false;

                return StageProgression.WaveIdx == -1 || waveReady;
            }

            return false;
        }

        private bool StageProgressionAdvanceWave(bool waveReady)
        {
            if (!StageProgressionCanAdvanceWave(waveReady) || StageProgression == null)
                return false;

            StageProgression.WaveIdx++;
            return true;
        }

        private static int StageSpawnEntryHp(BattleStageSpawnData spawn)
            => spawn != null && spawn.Hp > 0 ? spawn.Hp : 500;

        internal int StageSpawnEntryFactor()
        {
            int count = 0;
            for (int slot = 0; slot < 20; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotIncludingDormant(slot);
                if (entity == null || !world.IsActiveForCurrentPassInternal(entity))
                    continue;
                if (entity.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    continue;

                count++;
                if (entity.ObjectId == 51)
                    count++;
                if (entity.ObjectId == 52)
                    count += 2;
            }

            return count;
        }

        private void ResetStageSpawnRuntime()
        {
            SetStageSpawnRuntimeWave(-1);
            StageSpawnRuntimeTargetTotal?.Clear();
            StageSpawnRuntimeEntryCount?.Clear();
            StageSpawnRuntimeSpawnedTotal?.Clear();
            Runtime?.EnsureStageSpawnBuffers().Recycle(StageSpawnRuntimeSlots);
        }

        private void EnsureCurrentWavePositiveStageRuntime(BattleStagePhaseData phase, int factor)
        {
            if (phase == null || StageProgression == null)
                return;

            int spawnCount = phase.Spawns?.Count ?? 0;
            if (StageSpawnRuntimeWave == StageProgression.WaveIdx &&
                StageSpawnRuntimeTargetTotal?.Count == spawnCount &&
                StageSpawnRuntimeEntryCount?.Count == spawnCount &&
                StageSpawnRuntimeSpawnedTotal?.Count == spawnCount &&
                StageSpawnRuntimeSlots?.Count == spawnCount)
            {
                return;
            }

            SetStageSpawnRuntimeWave(StageProgression.WaveIdx);
            StageSpawnRuntimeTargetTotal?.Clear();
            StageSpawnRuntimeEntryCount?.Clear();
            StageSpawnRuntimeSpawnedTotal?.Clear();
            Runtime?.EnsureStageSpawnBuffers().Recycle(StageSpawnRuntimeSlots);

            for (int si = 0; si < spawnCount; si++)
            {
                StageSpawnRuntimeTargetTotal?.Add(0);
                StageSpawnRuntimeEntryCount?.Add(0);
                StageSpawnRuntimeSpawnedTotal?.Add(0);
                int[] slots = Runtime.EnsureStageSpawnBuffers().Rent();
                StageSpawnRuntimeSlots?.Add(slots);
                if (slots == null)
                    continue;

                BattleStageSpawnData spawn = phase.Spawns[si];
                if (spawn == null || spawn.Id < 0 || spawn.Ratio <= 0.0)
                    continue;

                int entryCount = (int)(factor * spawn.Ratio);
                if (entryCount > 40)
                    entryCount = 40;
                if (entryCount < 0)
                    entryCount = 0;

                int targetTotal = (int)(spawn.Times * spawn.Ratio * factor);
                if (targetTotal < 0)
                    targetTotal = 0;

                StageSpawnRuntimeEntryCount[si] = entryCount;
                StageSpawnRuntimeTargetTotal[si] = targetTotal;
            }
        }

        private void RefillCurrentWavePositiveStageSpawns(BattleStagePhaseData phase)
        {
            if (phase == null || StageProgression == null)
                return;

            if (StageSpawnRuntimeWave != StageProgression.WaveIdx)
                return;
            if (StageSpawnRuntimeSlots == null || StageSpawnRuntimeSlots.Count != (phase.Spawns?.Count ?? 0))
                return;

            for (int si = 0; si < phase.Spawns.Count; si++)
            {
                BattleStageSpawnData spawn = phase.Spawns[si];
                if (spawn == null || spawn.Id < 0 || spawn.Ratio <= 0.0)
                    continue;

                int entryCount = StageSpawnRuntimeEntryCount[si];
                int targetTotal = StageSpawnRuntimeTargetTotal[si];
                int[] slots = StageSpawnRuntimeSlots[si];
                if (slots == null || entryCount <= 0 || targetTotal <= 0)
                    continue;

                for (int i = 0; i < entryCount; i++)
                {
                    int slot = slots[i];
                    if (slot < 0)
                        continue;

                    LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                    if (entity == null || entity.ObjectId != spawn.Id)
                        slots[i] = -1;
                }

                for (int i = 0; i < entryCount; i++)
                {
                    if (slots[i] != -1)
                        continue;
                    if (StageSpawnRuntimeSpawnedTotal[si] >= targetTotal)
                        break;

                    int slot = SpawnStageImmediateEntrySlot(spawn);
                    if (slot < 0)
                        break;

                    slots[i] = slot;
                    StageSpawnRuntimeSpawnedTotal[si]++;
                }
            }
        }

        private bool CurrentWaveStageSpawnsCleared(BattleStagePhaseData phase)
        {
            if (phase?.Spawns == null)
                return true;

            for (int si = 0; si < phase.Spawns.Count; si++)
            {
                BattleStageSpawnData spawn = phase.Spawns[si];
                if (spawn == null || spawn.Id < 0)
                    continue;

                world.GetAllEntities(entityScratch);
                for (int i = 0; i < entityScratch.Count; i++)
                {
                    LF2Entity entity = entityScratch[i];
                    if (entity == null)
                        continue;

                    int slot = entity.Runtime?.SlotIndex ?? -1;
                    if (slot < 20 || slot >= RuntimeSlotCapacity)
                        continue;

                    if (entity.ObjectId == spawn.Id)
                    {
                        entityScratch.Clear();
                        return false;
                    }
                }

                entityScratch.Clear();
            }

            return true;
        }

        private bool CurrentWaveStageSpawnProducersInitialized(BattleStagePhaseData phase)
        {
            if (phase?.Spawns == null || StageProgression == null)
                return true;

            bool hasImmediate = false;
            bool hasPositive = false;

            for (int si = 0; si < phase.Spawns.Count; si++)
            {
                BattleStageSpawnData spawn = phase.Spawns[si];
                if (spawn == null || spawn.Id < 0)
                    continue;

                if (spawn.Ratio > 0.0)
                    hasPositive = true;
                else
                    hasImmediate = true;
            }

            if (hasImmediate && StageSpawnWaveApplied != StageProgression.WaveIdx)
                return false;
            if (hasPositive && StageSpawnWaveDeferredEntryApplied != StageProgression.WaveIdx)
                return false;

            return true;
        }

        private void ApplyCurrentWavePhaseAdvance()
        {
            if (!StageProgressionValid)
                return;
            if (BattleGameModeId != 1 && BattleGameModeId != 2)
                return;
            if (StageProgression == null || StageProgression.WaveIdx < 0)
                return;

            BattleStagePhaseData phase = StageProgressionCurrentPhase();
            if (phase == null)
                return;
            if (!CurrentWaveStageSpawnProducersInitialized(phase))
                return;
            if (!CurrentWaveStageSpawnsCleared(phase))
                return;
            if (!StageProgressionCanAdvanceWave(true))
                return;
            if (!StageProgressionAdvanceWave(true))
                return;

            BattleStagePhaseData nextPhase = StageProgressionCurrentPhase();
            if (nextPhase != null && nextPhase.Bound > 0)
                Runtime?.Stage?.ApplyPhaseBound(nextPhase.Bound);

            SetStageSpawnWaveApplied(-1);
            SetStageSpawnWaveDeferredEntryApplied(-1);
            ResetStageSpawnRuntime();
        }

        private void ApplyCurrentWaveImmediateStageSpawns()
        {
            if (!StageProgressionValid)
                return;
            if (BattleGameModeId != 1 && BattleGameModeId != 2)
                return;
            if (StageProgression == null || StageProgression.WaveIdx < 0)
                return;

            BattleStagePhaseData phase = StageProgressionCurrentPhase();
            if (phase == null)
            {
                ResetStageSpawnRuntime();
                return;
            }

            if (StageSpawnWaveApplied != StageProgression.WaveIdx)
            {
                bool spawnedAny = false;
                for (int si = 0; si < phase.Spawns.Count; si++)
                {
                    BattleStageSpawnData spawn = phase.Spawns[si];
                    if (spawn == null || spawn.Id < 0 || spawn.Ratio > 0.0)
                        continue;

                    if (SpawnStageImmediateEntrySlot(spawn) >= 0)
                        spawnedAny = true;
                }

                if (spawnedAny || phase.Spawns.Count == 0)
                    SetStageSpawnWaveApplied(StageProgression.WaveIdx);
            }

            int factor = StageSpawnEntryFactor();
            EnsureCurrentWavePositiveStageRuntime(phase, factor);

            if (StageSpawnWaveDeferredEntryApplied != StageProgression.WaveIdx)
            {
                bool deferredSeen = false;
                for (int si = 0; si < phase.Spawns.Count; si++)
                {
                    BattleStageSpawnData spawn = phase.Spawns[si];
                    if (spawn == null || spawn.Id < 0 || spawn.Ratio <= 0.0)
                        continue;

                    deferredSeen = true;
                    int count = StageSpawnRuntimeEntryCount[si];
                    if (count <= 0)
                        continue;

                    for (int i = 0; i < count; i++)
                    {
                        int slot = SpawnStageImmediateEntrySlot(spawn);
                        if (slot < 0)
                            break;

                        StageSpawnRuntimeSlots[si][i] = slot;
                        StageSpawnRuntimeSpawnedTotal[si]++;
                    }
                }

                if (deferredSeen || phase.Spawns.Count == 0)
                    SetStageSpawnWaveDeferredEntryApplied(StageProgression.WaveIdx);
            }

            RefillCurrentWavePositiveStageSpawns(phase);
        }

        internal int SpawnStageImmediateEntrySlot(BattleStageSpawnData spawn)
        {
            if (spawn == null || spawn.Id < 0)
                return -1;

            int requiredRuntimeSlot = world.FindFirstFreeRuntimeSlotForModule(
                20,
                RuntimeSlotCapacity);
            if (requiredRuntimeSlot < 0)
                return -1;

            if (RuntimeCharacterConfigs.Resolve(spawn.Id) == null ||
                RuntimeDataCatalog.GetObjectDefinition(spawn.Id) == null)
                return -1;

            BattleStageRuntimeState stage = Runtime?.Stage;
            int xMaxOverride = stage?.XMaxOverride ?? 0;
            int baseStageWidth = stage?.BaseStageWidthPx ?? 800;
            int stageBound = xMaxOverride > 0 ? xMaxOverride : (baseStageWidth > 0 ? baseStageWidth : 800);
            int configuredZMin = stage?.ZMin ?? 180;
            int configuredZMax = stage?.ZMax ?? (configuredZMin + 1);
            int zMin = configuredZMin > 0 ? configuredZMin : 180;
            int zMax = configuredZMax > zMin ? configuredZMax : zMin + 1;
            if (zMax <= zMin)
                zMax = zMin + 1;

            int spawnX;
            if (spawn.X == -1000)
            {
                if (Rng.NextInt(0, 2) != 0)
                    spawnX = -150 - Rng.NextInt(0, 300);
                else
                    spawnX = stageBound + 150 + Rng.NextInt(0, 300);
            }
            else
            {
                spawnX = spawn.X + Rng.NextInt(0, 300);
            }

            int spawnZ = zMin + Rng.NextInt(0, Mathf.Max(1, zMax - zMin));
            int hp = spawn.Id == 122 ? 200 : StageSpawnEntryHp(spawn);
            string facingDir = spawnX > (stageBound - 794) ? "left" : "right";

            LF2Entity entity = TrySpawnStageEntityWithFactory(
                spawn,
                spawnX,
                spawn.Y,
                spawnZ,
                facingDir,
                hp,
                requiredRuntimeSlot);
            if (entity == null && !IsStageRuntimeAllocationSealed())
            {
                entity = TrySpawnStageCharacterDirect(
                    spawn,
                    spawnX,
                    spawn.Y,
                    spawnZ,
                    facingDir,
                    hp,
                    requiredRuntimeSlot);
            }
            if (entity == null)
                return -1;
            if (entity.Runtime?.SlotIndex != requiredRuntimeSlot)
                return -1;

            if (!world.RestoreStageSpawnRestState(requiredRuntimeSlot, entity))
            {
                LF2ObjectPointFactory.ReleaseRejectedSpawn(entity.Renderer, entity);
                return -1;
            }
            entity.SetPos(spawnX, spawn.Y, spawnZ);
            entity.Runtime?.SyncIntegerPosition();
            entity.SwitchDir(facingDir);
            entity.DirectWriteFrameImmediateWaitReset(spawn.Act);
            entity.FrameDelay = 0;
            entity.OwnerId = -1;
            entity.OwnerEntityIndex = -1;
            entity.SpawnerEntityIndex = -1;
            entity.Health.PP = 500;
            entity.Health.MaxPP = 500;
            entity.Health.PPBound = 500;
            entity.Health.MP = 500;
            entity.Health.MaxMP = 500;
            ApplyStageSpawnRuntimeContract(entity, hp);
            entity.RefreshRuntimeSnapshot();
            return requiredRuntimeSlot;
        }

        internal static bool UsesStageCharacterInitSemantics(int dataObjectType)
        {
            return dataObjectType == (int)LF2ObjectType.Character ||
                   dataObjectType == (int)LF2ObjectType.Other;
        }

        internal static void ApplyStageSpawnRuntimeContract(LF2Entity entity, int hp)
        {
            if (entity == null)
                return;

            int dataObjectType = entity.GetCurrentDataObjectTypeForSimulation();
            bool usesCharacterInit = UsesStageCharacterInitSemantics(dataObjectType);
            entity.HitStun = usesCharacterInit ? 20 : 0;
            entity.Unk344 = 2;
            entity.Team = 2;
            entity.RelationTeam = usesCharacterInit ? 2 : 0;
            entity.Health.HP = hp;
            entity.Health.HPBound = hp;
            entity.Health.HP3 = hp;
            entity.HolderCopySlot = entity.Runtime?.SlotIndex ?? -1;
        }

        private bool IsStageRuntimeAllocationSealed()
        {
            return LogicReferencePool?.IsBattleCapacitySealed == true;
        }

        private LF2Entity TrySpawnStageEntityWithFactory(
            BattleStageSpawnData spawn,
            int spawnX,
            int spawnY,
            int spawnZ,
            string facingDir,
            int hp,
            int requiredRuntimeSlot)
        {
            ILF2ObjectPointFactory factory =
                world.ResolveObjectPointFactoryForSimulation();
            BattleLogicReferencePool referencePool = LogicReferencePool;
            if (factory == null || referencePool == null)
                return null;

            OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
            if (task == null)
                return null;

            LF2Entity entity;
            try
            {
                StageSpawnTaskConfigurator.Configure(
                    task,
                    spawn,
                    spawnX,
                    spawnY,
                    spawnZ,
                    facingDir,
                    requiredRuntimeSlot);
                task.useInitialRuntimeIntPosition = true;
                task.initialRuntimeX = spawnX;
                task.initialRuntimeY = spawnY;
                task.initialRuntimeZ = spawnZ;
                task.targetWorld = world;
                entity = factory.CreateObjectImmediate(task);
            }
            finally
            {
                referencePool.Recycle(task);
            }

            if (entity == null || entity.Runtime?.SlotIndex != requiredRuntimeSlot)
                return null;

            entity.Health.HP = hp;
            entity.Health.HPBound = hp;
            entity.Health.HP3 = hp;
            return entity;
        }

        private LF2Entity TrySpawnStageCharacterDirect(
            BattleStageSpawnData spawn,
            int spawnX,
            int spawnY,
            int spawnZ,
            string facingDir,
            int hp,
            int requiredRuntimeSlot)
        {
            LF2CharacterDataWrapper wrapper = RuntimeCharacterConfigs.Resolve(spawn.Id);
            if (wrapper == null)
                return null;

            ObjectDefinition definition = RuntimeDataCatalog.GetObjectDefinition(spawn.Id);
            int objectType = definition?.type ?? (int)LF2ObjectType.Character;
            if (objectType != (int)LF2ObjectType.Character)
                return null;

            OPointCreateTask task = StageSpawnTaskConfigurator.CreateCold(
                spawn,
                spawnX,
                spawnY,
                spawnZ,
                facingDir,
                requiredRuntimeSlot);

            LF2Character character = new LF2Character();
            character.ModuleInitialize();
            character.SetRequiredRuntimeSlot(requiredRuntimeSlot);
            character.Init(task, null);
            character.ModuleBind(wrapper, spawn.Id, world);
            if (character.Match == null)
            {
                if (character.RequiredRuntimeSlot != requiredRuntimeSlot)
                    return null;
                world.Register(character);
            }
            else if (character.Match != world)
                return null;
            if (character.Runtime?.SlotIndex != requiredRuntimeSlot)
                return null;
            character.Initialize(hp, 500);
            return character;
        }
    }
}
