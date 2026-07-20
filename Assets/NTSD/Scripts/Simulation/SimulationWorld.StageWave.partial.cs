using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 的 stage 波次推进与刷怪 pass。
    /// </summary>
    public partial class SimulationWorld
    {
        public void CurrentWaveStageTickAll()
        {
            ApplyCurrentWavePhaseAdvance();
            ApplyCurrentWaveImmediateStageSpawns();
        }

        public void ConfigureStageCampaigns(
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

        public bool StartInitialStageWave()
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

        private int StageSpawnEntryFactor()
        {
            int count = 0;
            for (int slot = 0; slot < 20; slot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(slot);
                if (entity == null || !IsActiveForCurrentPass(entity))
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
            StageSpawnRuntimeSlots?.Clear();
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
            StageSpawnRuntimeSlots?.Clear();

            for (int si = 0; si < spawnCount; si++)
            {
                StageSpawnRuntimeTargetTotal?.Add(0);
                StageSpawnRuntimeEntryCount?.Add(0);
                StageSpawnRuntimeSpawnedTotal?.Add(0);
                int[] slots = new int[40];
                Array.Fill(slots, -1);
                StageSpawnRuntimeSlots?.Add(slots);

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
                if (entryCount <= 0 || targetTotal <= 0)
                    continue;

                for (int i = 0; i < entryCount; i++)
                {
                    int slot = StageSpawnRuntimeSlots[si][i];
                    if (slot < 0)
                        continue;

                    LF2Entity entity = FindEntityByRuntimeSlotForQuery(slot);
                    if (entity == null || entity.ObjectId != spawn.Id)
                        StageSpawnRuntimeSlots[si][i] = -1;
                }

                for (int i = 0; i < entryCount; i++)
                {
                    if (StageSpawnRuntimeSlots[si][i] != -1)
                        continue;
                    if (StageSpawnRuntimeSpawnedTotal[si] >= targetTotal)
                        break;

                    int slot = SpawnStageImmediateEntrySlot(spawn);
                    if (slot < 0)
                        break;

                    StageSpawnRuntimeSlots[si][i] = slot;
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

                GetAllEntities(_entityScratch);
                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity == null)
                        continue;

                    int slot = entity.Runtime?.SlotIndex ?? -1;
                    if (slot < 20 || slot >= RuntimeSlotCapacity)
                        continue;

                    if (entity.ObjectId == spawn.Id)
                    {
                        _entityScratch.Clear();
                        return false;
                    }
                }

                _entityScratch.Clear();
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

        private int SpawnStageImmediateEntrySlot(BattleStageSpawnData spawn)
        {
            if (spawn == null || spawn.Id < 0)
                return -1;

            int requiredRuntimeSlot = FindFirstFreeRuntimeSlot(20, RuntimeSlotCapacity);
            if (requiredRuntimeSlot < 0)
                return -1;

            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            GameDataManager dataManager = GameDataManager.Instance;
            if (manager?.GetCharacterConfig(spawn.Id) == null || dataManager?.GetObjectById(spawn.Id) == null)
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
            entity ??= TrySpawnStageCharacterDirect(
                spawn,
                spawnX,
                spawn.Y,
                spawnZ,
                facingDir,
                hp,
                requiredRuntimeSlot);
            if (entity == null)
                return -1;
            if (entity.Runtime?.SlotIndex != requiredRuntimeSlot)
                return -1;

            if (!RestoreStageSpawnRestState(requiredRuntimeSlot, entity))
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

        internal static OPointCreateTask BuildStageSpawnTask(
            BattleStageSpawnData spawn,
            int spawnX,
            int spawnY,
            int spawnZ,
            string facingDir,
            int requiredRuntimeSlot = -1)
        {
            return new OPointCreateTask
            {
                opoint = new ObjectPoint
                {
                    oid = spawn.Id,
                    kind = 0,
                    action = spawn.Act,
                    x = spawnX,
                    y = spawnY,
                    facing = 0,
                },
                parent = null,
                team = 2,
                requiredRuntimeSlot = requiredRuntimeSlot,
                relationTeam = 2,
                holderCopySlot = -1,
                useExplicitRelationIdentity = true,
                pos = new Vector3(spawnX, spawnY, 0f),
                z = spawnZ,
                dir = facingDir,
                preserveActionZero = true,
                skipPostInitZOffset = true,
                useDirectVelocity = true,
                directVx = 0f,
                directVy = 0f,
                directVz = 0f,
                frameDelay = 0,
                attackExempt = 0,
                releaseSpawnSemantic = ReleaseSpawnSemantic.StageSpawnAt,
            };
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
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            LF2ObjectPool objectPool = LF2ObjectPool.Instance;
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            if (factory == null || referencePool == null || objectPool == null || manager == null)
                return null;

            OPointCreateTask task = BuildStageSpawnTask(
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
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;

            LF2Entity entity = factory.CreateObjectImmediate(task);
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
            LF2CharacterDataWrapper wrapper = LF2Entity.ResolveRuntimeCharacterConfig(spawn.Id);
            if (wrapper == null)
                return null;

            ObjectDefinition definition = GameDataManager.Instance?.GetObjectById(spawn.Id);
            int objectType = definition?.type ?? (int)LF2ObjectType.Character;
            if (objectType != (int)LF2ObjectType.Character)
                return null;

            OPointCreateTask task = BuildStageSpawnTask(
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
            character.ModuleBind(wrapper, spawn.Id);
            if (character.Match == null)
            {
                if (character.RequiredRuntimeSlot != requiredRuntimeSlot)
                    return null;
                Register(character);
            }
            else if (character.Match != this)
                return null;
            if (character.Runtime?.SlotIndex != requiredRuntimeSlot)
                return null;
            character.Initialize(hp, 500);
            return character;
        }
    }
}
