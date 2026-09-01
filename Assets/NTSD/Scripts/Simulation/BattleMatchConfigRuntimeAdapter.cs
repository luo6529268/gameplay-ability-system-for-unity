using System;
using NTSD.App;

namespace NTSD.Simulation
{
    public static class BattleMatchConfigRuntimeAdapter
    {
        public static void ApplyMatchConfig(
            this BattleRosterRuntimeState roster,
            MatchConfig config)
        {
            roster.Reset();
            if (config?.players == null)
                return;

            int slotCount = Math.Min(config.players.Count, roster.Slots.Length);
            for (int i = 0; i < slotCount; i++)
            {
                PlayerSlotConfig player = config.players[i];
                if (player == null || !player.use)
                    continue;

                BattleSlotRuntimeState slot = roster.Slots[i];
                slot.Active = true;
                slot.IsHuman = player.isHuman;
                slot.CharacterId = player.characterId;
                slot.Team = ResolveBattleTeam(player.team, i);
                slot.InputId = ResolveInputId(player.inputId, i);
                slot.AiId = player.aiId;
                roster.ActiveSlotCount++;
            }
        }

        public static int ResolveBattleTeam(int configuredTeam, int originalSlotIndex)
        {
            if (configuredTeam == GameConfig.TeamIndependent)
                return 10 + originalSlotIndex;

            return configuredTeam > 0 ? configuredTeam : originalSlotIndex + 1;
        }

        public static int ResolveInputId(int configuredInputId, int originalSlotIndex)
        {
            return configuredInputId > 0 ? configuredInputId : originalSlotIndex + 1;
        }

        public static void ApplyBootstrapFromMatchConfig(
            this BattleSlotLabelRuntimeState labels,
            MatchConfig config)
        {
            labels.Reset();
            if (config?.players == null)
                return;

            int slotCount = Math.Min(config.players.Count, 4);
            for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
            {
                PlayerSlotConfig player = config.players[slotIndex];
                if (player == null || !player.use)
                    continue;

                labels.BattleSlotLabels[slotIndex, 0] = (char)('1' + slotIndex);
                labels.BattleSlotLabelState[slotIndex] = slotIndex + 1;
            }
        }
    }
}
