using System;
using System.Collections.Generic;

namespace NTSD.App
{
    [Serializable]
    public sealed class MatchConfig
    {
        public GameModeConfig gameMode;

        public List<PlayerSlotConfig> players = new List<PlayerSlotConfig>();

        public int backgroundId = -1;
        public int difficulty = 2;

        public string stageCampaignFilePath = string.Empty;
        public int stageSeriesId;

        public int seed;
    }
}
