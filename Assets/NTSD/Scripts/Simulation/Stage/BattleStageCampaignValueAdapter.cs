using System;
using System.Collections.Generic;

namespace NTSD.Simulation
{
    public static class BattleStageCampaignValueAdapter
    {
        public static bool TryProject(
            IReadOnlyList<BattleStageCampaignData> source,
            out BattleStageCampaignSet result)
        {
            result = null;
            if (source == null)
                return false;

            if (source.Count == 0)
            {
                result = BattleStageCampaignSet.Empty;
                return true;
            }

            var campaigns = new BattleStageCampaignValue[source.Count];
            for (int campaignIndex = 0;
                 campaignIndex < source.Count;
                 campaignIndex++)
            {
                BattleStageCampaignData campaign = source[campaignIndex];
                if (campaign == null ||
                    campaign.Comment == null ||
                    campaign.Phases == null)
                {
                    return false;
                }

                var phases = new BattleStagePhaseValue[campaign.Phases.Count];
                for (int phaseIndex = 0;
                     phaseIndex < campaign.Phases.Count;
                     phaseIndex++)
                {
                    BattleStagePhaseData phase = campaign.Phases[phaseIndex];
                    if (phase == null || phase.Spawns == null)
                        return false;

                    var spawns = new BattleStageSpawnValue[phase.Spawns.Count];
                    for (int spawnIndex = 0;
                         spawnIndex < phase.Spawns.Count;
                         spawnIndex++)
                    {
                        BattleStageSpawnData spawn = phase.Spawns[spawnIndex];
                        if (spawn == null ||
                            double.IsNaN(spawn.Ratio) ||
                            double.IsInfinity(spawn.Ratio))
                        {
                            return false;
                        }

                        spawns[spawnIndex] = spawn.ToValue();
                    }

                    phases[phaseIndex] = new BattleStagePhaseValue(
                        phase.Bound,
                        spawns);
                }

                campaigns[campaignIndex] = new BattleStageCampaignValue(
                    campaign.Id,
                    campaign.Comment,
                    phases);
            }

            result = new BattleStageCampaignSet(campaigns);
            return true;
        }
    }
}
