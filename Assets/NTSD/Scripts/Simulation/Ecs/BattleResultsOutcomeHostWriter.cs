using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Observes the completed battle tick and advances the host-owned terminal
    /// guard. Post-battle Results navigation remains a separate host writer.
    /// </summary>
    internal sealed class BattleResultsOutcomeHostWriter
    {
        private readonly SimulationWorld world;

        internal BattleResultsOutcomeHostWriter(SimulationWorld world)
        {
            this.world = world;
        }

        // Alignment contract: CLIENT-CPP-RESULTS-RESERVE-TERMINAL-INTEGRATION-001.
        // C++ retains two full-domain living-team buckets before reserve/guard handling.
        internal void UpdateSummaryActivation()
        {
            BattleRuntimeState battle = world.Runtime;
            if (battle == null)
                return;

            battle.Results ??= new BattleResultsRuntimeState();
            BattleResultsRuntimeState results = battle.Results;
            if (results.IsActive)
                return;

            results.EnsureTeamIds();
            int firstTeamAlive = 0;
            int secondTeamAlive = 0;
            int slotCount = world.RuntimeSlotCapacityForDiagnostics <
                            SimulationWorld.AuthorityRuntimeSlotCapacity
                ? world.RuntimeSlotCapacityForDiagnostics
                : SimulationWorld.AuthorityRuntimeSlotCapacity;
            for (int runtimeSlot = 0; runtimeSlot < slotCount; runtimeSlot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotIncludingDormant(
                    runtimeSlot);
                if (!world.IsActiveForCurrentPassInternal(entity) ||
                    entity.FrameCache?.Wrapper?.characterData == null ||
                    entity.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character ||
                    entity.Health == null ||
                    entity.Health.HP <= 0)
                {
                    continue;
                }

                int team = entity.RelationTeam;
                int bucket = -1;
                for (int teamIndex = 0;
                     teamIndex < results.TeamCount && teamIndex < 2;
                     teamIndex++)
                {
                    if (results.TeamIds[teamIndex] != team)
                        continue;
                    bucket = teamIndex;
                    break;
                }

                if (bucket < 0 && results.TeamCount < 2)
                {
                    bucket = results.TeamCount;
                    results.TeamIds[results.TeamCount++] = team;
                }

                if (bucket == 0)
                    firstTeamAlive++;
                else if (bucket == 1)
                    secondTeamAlive++;
            }

            if (firstTeamAlive > 0 && secondTeamAlive > 0)
                results.HadBoth = true;

            if (battle.Match?.BattleGameModeId == 4 &&
                results.HadBoth &&
                (firstTeamAlive == 0 || secondTeamAlive == 0))
            {
                int emptySide = firstTeamAlive == 0 ? 0 : 1;
                if (world.TrySpawnBattleResultsReserveBeforeResults(
                        results,
                        emptySide))
                {
                    if (emptySide == 0)
                        firstTeamAlive = 1;
                    else
                        secondTeamAlive = 1;
                    results.BattleEndPhase = 0;
                    results.PendingWinner = -1;
                }
            }

            if (!results.HadBoth ||
                (firstTeamAlive > 0 && secondTeamAlive > 0))
            {
                return;
            }

            int decidedWinner = firstTeamAlive > 0
                ? 0
                : secondTeamAlive > 0
                    ? 1
                    : -1;
            if (results.BattleEndPhase == 0)
            {
                results.BattleEndPhase = 1;
                results.PendingWinner = decidedWinner;
            }
            else
            {
                results.BattleEndPhase++;
            }

            if (results.BattleEndPhase >= 11)
            {
                results.ActivateSummary(
                    results.PendingWinner,
                    results.TeamCount,
                    results.TeamIds[0],
                    results.TeamIds[1]);
            }
        }
    }
}
