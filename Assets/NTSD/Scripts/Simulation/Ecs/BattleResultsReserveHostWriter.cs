using System;

using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Owns the C++ release-live mode-4 reserve synchronization and per-entry
    /// spawn transaction. Terminal observation is connected by a later package.
    /// </summary>
    internal sealed class BattleResultsReserveHostWriter
    {
        private const int ReserveStartSlot = 20;
        private static readonly int[] ReserveOids =
        {
            30, 31, 33, 34, 39, 32, 35, 36, 37, 122, 123,
        };

        private readonly SimulationWorld world;
        private readonly int[,] liveCounts = new int[2, 11];
        private readonly int[,] missingCounts = new int[2, 11];

        internal BattleResultsReserveHostWriter(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        // Alignment contract: CLIENT-CPP-RESULTS-RESERVE-TRANSACTION-SEAM-001.
        // The authority loop commits each successful column independently.
        internal bool TrySpawnBeforeResults(
            BattleResultsRuntimeState results,
            int side)
        {
            BattleRuntimeState battle = world.Runtime;
            if (battle?.Match?.BattleGameModeId != 4 ||
                results == null ||
                side < 0 ||
                side >= 2)
            {
                return false;
            }

            SynchronizeOwner(results);
            bool spawnedAny = false;
            for (int col = 0; col < ReserveOids.Length; col++)
            {
                if (battle.ReserveCommittedTotal[side, col] <= 0 ||
                    battle.ReserveCommittedHp[side, col] <= liveCounts[side, col])
                {
                    continue;
                }

                int reserveEndSlotExclusive = Math.Min(
                    world.RuntimeSlotCapacityForDiagnostics,
                    SimulationWorld.AuthorityRuntimeSlotCapacity);
                int runtimeSlot = world.FindFirstFreeRuntimeSlotForModule(
                    ReserveStartSlot,
                    reserveEndSlotExclusive);
                if (runtimeSlot < 0)
                    break;

                int oid = ReserveOids[col];
                if (!world.TrySpawnResultsReserveEntry(
                        oid,
                        side,
                        ReserveHpForOid(oid),
                        runtimeSlot))
                {
                    continue;
                }

                if (results.ResultCommittedTotal[side, col] > 0)
                    results.ResultCommittedTotal[side, col]--;
                battle.ReserveCommittedTotal[side, col] =
                    results.ResultCommittedTotal[side, col];
                spawnedAny = true;
            }

            if (spawnedAny)
                SynchronizeOwner(results);
            return spawnedAny;
        }

        internal void SynchronizeOwner(BattleResultsRuntimeState results)
        {
            BattleRuntimeState battle = world.Runtime;
            if (battle == null || results == null)
                return;

            battle.ReserveOwnerValid = battle.Match?.BattleGameModeId == 4;
            Array.Clear(liveCounts, 0, liveCounts.Length);
            for (int side = 0; side < 2; side++)
            {
                for (int col = 0; col < ReserveOids.Length; col++)
                {
                    battle.ReserveCommittedTotal[side, col] =
                        results.ResultCommittedTotal[side, col];
                    battle.ReserveCommittedHp[side, col] =
                        results.ResultCommittedHp[side, col];
                }
            }

            int reserveEndSlotExclusive = Math.Min(
                world.RuntimeSlotCapacityForDiagnostics,
                SimulationWorld.AuthorityRuntimeSlotCapacity);
            for (int runtimeSlot = ReserveStartSlot;
                 runtimeSlot < reserveEndSlotExclusive;
                 runtimeSlot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotIncludingDormant(
                    runtimeSlot);
                if (!world.IsActiveForCurrentPassInternal(entity))
                    continue;

                int relationSide = entity.Unk344 >= 1 && entity.Unk344 <= 2
                    ? entity.Unk344 - 1
                    : entity.RelationTeam >= 1 && entity.RelationTeam <= 2
                        ? entity.RelationTeam - 1
                        : -1;
                if (relationSide < 0)
                    continue;

                int oid = LF2Entity.ResolveCurrentDataObjectId(entity);
                for (int col = 0; col < ReserveOids.Length; col++)
                {
                    if (oid != ReserveOids[col])
                        continue;
                    liveCounts[relationSide, col]++;
                    break;
                }
            }

            for (int side = 0; side < 2; side++)
            {
                for (int col = 0; col < ReserveOids.Length; col++)
                {
                    int missing = battle.ReserveCommittedTotal[side, col] -
                                  liveCounts[side, col];
                    missingCounts[side, col] = missing > 0 ? missing : 0;
                }
            }
        }

        internal int GetLiveCount(int side, int col)
        {
            return side >= 0 && side < 2 && col >= 0 && col < ReserveOids.Length
                ? liveCounts[side, col]
                : 0;
        }

        internal int GetMissingCount(int side, int col)
        {
            return side >= 0 && side < 2 && col >= 0 && col < ReserveOids.Length
                ? missingCounts[side, col]
                : 0;
        }

        internal static int ReserveOidAt(int col)
        {
            return col >= 0 && col < ReserveOids.Length ? ReserveOids[col] : -1;
        }

        internal static int ReserveHpForOid(int oid)
        {
            if (oid == 36)
                return 250;
            if (oid == 37 || oid == 35 || oid == 32 || oid == 122)
                return 200;
            if (oid == 39 || oid == 33)
                return 150;
            if (oid == 34)
                return 100;
            if (oid == 31 || oid == 30)
                return 50;
            return 500;
        }
    }
}
