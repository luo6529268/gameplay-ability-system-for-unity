using System;
using System.Collections.Generic;

namespace NTSD.App
{
    [Serializable]
    public sealed class MatchConfig
    {
        public GameModeConfig gameMode;

        public List<PlayerSlotConfig> players = new List<PlayerSlotConfig>();

        // Optional battle-only overrides of the native ten-name/eight-bracket tables.
        // Empty lists retain the formal GameSession defaults.
        public List<string> nativeBattlePlayerNames = new List<string>();
        public List<bool> nativeBracketPlayerNames = new List<bool>();
        public bool nativeKnockoutFeedRuntimeDisplayEnabled = true;

        public int backgroundId = -1;
        public int difficulty = 2;

        public string stageCampaignFilePath = string.Empty;
        public int stageSeriesId;

        public int seed;

        // NTSD 2.8 defaults to 2tu. Selection UI is intentionally outside this
        // runtime value contract; callers may opt into the native 1tu cadence.
        public bool oneTuInput;
    }
}
