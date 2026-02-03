using System;

namespace NTSD.App
{
    [Serializable]
    public sealed class PlayerSlotConfig
    {
        public bool use;
        public bool isHuman;

        public int characterId;
        public int team;

        // When isHuman==false.
        public int aiId;
    }
}
