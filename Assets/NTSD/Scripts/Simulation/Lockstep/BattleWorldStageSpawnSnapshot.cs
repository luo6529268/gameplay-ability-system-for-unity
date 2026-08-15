using System;
using System.Collections.Generic;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    /// <summary>
    /// Reusable fixed-capacity storage for the mutable stage-spawn runtime lists.
    /// Capacity is selected from the loaded campaign during bootstrap. Capture
    /// never grows this buffer and fails before writing when the source contract
    /// cannot fit. Static campaign data remains identified by the stage fingerprint.
    /// </summary>
    public sealed class BattleWorldStageSpawnSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        private readonly int[] targetTotals;
        private readonly int[] entryCounts;
        private readonly int[] spawnedTotals;
        private readonly int[] runtimeSlots;

        public BattleWorldStageSpawnSnapshotBuffer(int entryCapacity)
        {
            if (entryCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(entryCapacity));
            }

            EntryCapacity = entryCapacity;
            targetTotals = new int[entryCapacity];
            entryCounts = new int[entryCapacity];
            spawnedTotals = new int[entryCapacity];
            runtimeSlots = new int[
                entryCapacity * StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry];
        }

        public int EntryCapacity { get; }
        public int ActiveEntryCount { get; private set; }
        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }
        public int RuntimeWave { get; private set; }

        public int GetTargetTotal(int entryIndex)
        {
            ValidateActiveEntryIndex(entryIndex);
            return targetTotals[entryIndex];
        }

        public int GetEntryCount(int entryIndex)
        {
            ValidateActiveEntryIndex(entryIndex);
            return entryCounts[entryIndex];
        }

        public int GetSpawnedTotal(int entryIndex)
        {
            ValidateActiveEntryIndex(entryIndex);
            return spawnedTotals[entryIndex];
        }

        public int GetRuntimeSlot(int entryIndex, int slotIndex)
        {
            ValidateActiveEntryIndex(entryIndex);
            ValidateIndex(
                slotIndex,
                StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry,
                nameof(slotIndex));
            return runtimeSlots[
                entryIndex * StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry +
                slotIndex];
        }

        public static int CalculateRequiredEntryCapacity(
            IReadOnlyList<BattleStageCampaignData> campaigns)
        {
            int maximum = 0;
            if (campaigns == null)
            {
                return maximum;
            }

            for (int campaignIndex = 0;
                 campaignIndex < campaigns.Count;
                 campaignIndex++)
            {
                BattleStageCampaignData campaign = campaigns[campaignIndex];
                if (campaign?.Phases == null)
                {
                    continue;
                }

                for (int phaseIndex = 0;
                     phaseIndex < campaign.Phases.Count;
                     phaseIndex++)
                {
                    int count = campaign.Phases[phaseIndex]?.Spawns?.Count ?? 0;
                    if (count > maximum)
                    {
                        maximum = count;
                    }
                }
            }

            return maximum;
        }

        internal bool TryCapture(
            BattleRuntimeState runtime,
            LockstepSessionIdentity identity,
            int tick)
        {
            if (!HasCanonicalSource(runtime, out int activeEntryCount))
            {
                return false;
            }

            for (int entryIndex = 0;
                 entryIndex < activeEntryCount;
                 entryIndex++)
            {
                targetTotals[entryIndex] =
                    runtime.StageSpawnRuntimeTargetTotal[entryIndex];
                entryCounts[entryIndex] =
                    runtime.StageSpawnRuntimeEntryCount[entryIndex];
                spawnedTotals[entryIndex] =
                    runtime.StageSpawnRuntimeSpawnedTotal[entryIndex];

                int[] sourceSlots = runtime.StageSpawnRuntimeSlots[entryIndex];
                int destinationBase =
                    entryIndex * StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry;
                for (int slotIndex = 0;
                     slotIndex < StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry;
                     slotIndex++)
                {
                    runtimeSlots[destinationBase + slotIndex] = sourceSlots[slotIndex];
                }
            }

            ActiveEntryCount = activeEntryCount;
            SchemaVersion = CurrentSchemaVersion;
            ProtocolSchemaVersion = identity.SchemaVersion;
            IdentityFingerprint = identity.IdentityFingerprint;
            CapturedTick = tick;
            RuntimeWave = runtime.StageSpawnRuntimeWave;
            return true;
        }

        internal bool TryRestoreTo(BattleRuntimeState runtime)
        {
            if (SchemaVersion != CurrentSchemaVersion ||
                runtime?.StageSpawnRuntimeTargetTotal == null ||
                runtime.StageSpawnRuntimeEntryCount == null ||
                runtime.StageSpawnRuntimeSpawnedTotal == null ||
                runtime.StageSpawnRuntimeSlots == null)
            {
                return false;
            }

            StageSpawnRuntimeBufferPool pool = runtime.EnsureStageSpawnBuffers();
            pool.Recycle(runtime.StageSpawnRuntimeSlots);
            for (int entryIndex = 0; entryIndex < ActiveEntryCount; entryIndex++)
            {
                int[] rented = pool.Rent();
                if (rented == null)
                    return false;
                runtime.StageSpawnRuntimeSlots.Add(rented);
            }

            runtime.StageSpawnRuntimeTargetTotal.Clear();
            runtime.StageSpawnRuntimeEntryCount.Clear();
            runtime.StageSpawnRuntimeSpawnedTotal.Clear();
            for (int entryIndex = 0; entryIndex < ActiveEntryCount; entryIndex++)
            {
                runtime.StageSpawnRuntimeTargetTotal.Add(targetTotals[entryIndex]);
                runtime.StageSpawnRuntimeEntryCount.Add(entryCounts[entryIndex]);
                runtime.StageSpawnRuntimeSpawnedTotal.Add(spawnedTotals[entryIndex]);
                int[] destination = runtime.StageSpawnRuntimeSlots[entryIndex];
                if (destination == null ||
                    destination.Length != StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry)
                {
                    return false;
                }

                int sourceBase =
                    entryIndex * StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry;
                for (int slotIndex = 0;
                     slotIndex < StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry;
                     slotIndex++)
                {
                    destination[slotIndex] = runtimeSlots[sourceBase + slotIndex];
                }
            }

            runtime.StageSpawnRuntimeWave = RuntimeWave;
            return true;
        }

        private bool HasCanonicalSource(
            BattleRuntimeState runtime,
            out int activeEntryCount)
        {
            activeEntryCount = 0;
            if (runtime?.StageSpawnRuntimeTargetTotal == null ||
                runtime.StageSpawnRuntimeEntryCount == null ||
                runtime.StageSpawnRuntimeSpawnedTotal == null ||
                runtime.StageSpawnRuntimeSlots == null)
            {
                return false;
            }

            activeEntryCount = runtime.StageSpawnRuntimeTargetTotal.Count;
            if (activeEntryCount > EntryCapacity ||
                runtime.StageSpawnRuntimeEntryCount.Count != activeEntryCount ||
                runtime.StageSpawnRuntimeSpawnedTotal.Count != activeEntryCount ||
                runtime.StageSpawnRuntimeSlots.Count != activeEntryCount)
            {
                return false;
            }

            for (int entryIndex = 0;
                 entryIndex < activeEntryCount;
                 entryIndex++)
            {
                int[] slots = runtime.StageSpawnRuntimeSlots[entryIndex];
                if (slots == null ||
                    slots.Length != StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry)
                {
                    return false;
                }
            }

            return true;
        }

        private void ValidateActiveEntryIndex(int entryIndex)
        {
            ValidateIndex(entryIndex, ActiveEntryCount, nameof(entryIndex));
        }

        private static void ValidateIndex(
            int index,
            int count,
            string parameterName)
        {
            if ((uint)index >= (uint)count)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    internal sealed class BattleWorldStageSpawnSnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleWorldStageSpawnSnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal int RequiredEntryCapacity =>
            BattleWorldStageSpawnSnapshotBuffer.CalculateRequiredEntryCapacity(
                world.Runtime?.StageCampaigns);

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldStageSpawnSnapshotBuffer destination)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            return destination.TryCapture(world.Runtime, identity, tick);
        }
    }
}
