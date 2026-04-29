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

        /// <summary>
        /// 输入ID（对应 Action Map 中的 Player_X，X 为此 ID）
        /// </summary>
        public int inputId;

        // When isHuman==false.
        public int aiId;
    }
}
