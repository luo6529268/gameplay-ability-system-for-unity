using System;
using System.Collections.Generic;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the fixed stage-spawn slot buffers used by positive-ratio wave entries.
    /// The pool is prepared from the loaded campaign before battle and never grows
    /// after it is sealed.
    /// </summary>
    public sealed class StageSpawnRuntimeBufferPool
    {
        public const int SlotsPerSpawnEntry = 40;

        private readonly List<int[]> available = new List<int[]>();
        private bool sealedForBattle;

        public long RejectedRentCountForDiagnostics { get; private set; }

        public void Prepare(
            BattleStageCampaignSet campaigns,
            List<int> targetTotal,
            List<int> entryCount,
            List<int> spawnedTotal,
            List<int[]> activeSlots)
        {
            sealedForBattle = false;
            Recycle(activeSlots);

            int requiredEntryCount = FindMaximumSpawnEntryCount(campaigns);
            EnsureListCapacity(targetTotal, requiredEntryCount);
            EnsureListCapacity(entryCount, requiredEntryCount);
            EnsureListCapacity(spawnedTotal, requiredEntryCount);
            EnsureListCapacity(activeSlots, requiredEntryCount);
            if (available.Capacity < requiredEntryCount)
                available.Capacity = requiredEntryCount;

            while (available.Count < requiredEntryCount)
                available.Add(new int[SlotsPerSpawnEntry]);

            sealedForBattle = true;
        }

        public int[] Rent()
        {
            int index = available.Count - 1;
            if (index < 0)
            {
                if (sealedForBattle)
                {
                    RejectedRentCountForDiagnostics++;
                    return null;
                }

                return CreateClearedBuffer();
            }

            int[] buffer = available[index];
            available.RemoveAt(index);
            Array.Fill(buffer, -1);
            return buffer;
        }

        public void Recycle(List<int[]> activeSlots)
        {
            if (activeSlots == null)
                return;

            for (int index = 0; index < activeSlots.Count; index++)
            {
                int[] buffer = activeSlots[index];
                if (buffer != null && buffer.Length == SlotsPerSpawnEntry)
                    available.Add(buffer);
            }

            activeSlots.Clear();
        }

        public void Unseal()
        {
            sealedForBattle = false;
        }

        private static int FindMaximumSpawnEntryCount(
            BattleStageCampaignSet campaigns)
        {
            int maximum = 0;
            if (campaigns == null)
                return maximum;

            for (int campaignIndex = 0;
                 campaignIndex < campaigns.Count;
                 campaignIndex++)
            {
                BattleStageCampaignValue campaign = campaigns[campaignIndex];

                for (int phaseIndex = 0;
                     phaseIndex < campaign.Phases.Count;
                     phaseIndex++)
                {
                    int count = campaign.Phases[phaseIndex].Spawns.Count;
                    if (count > maximum)
                        maximum = count;
                }
            }

            return maximum;
        }

        private static int[] CreateClearedBuffer()
        {
            int[] buffer = new int[SlotsPerSpawnEntry];
            Array.Fill(buffer, -1);
            return buffer;
        }

        private static void EnsureListCapacity<T>(List<T> list, int capacity)
        {
            if (list != null && list.Capacity < capacity)
                list.Capacity = capacity;
        }
    }
}
