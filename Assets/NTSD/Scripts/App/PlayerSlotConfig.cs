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
        /// 输入 ID，对应运行时的玩家输入映射，例如 `Player_1`。
        /// </summary>
        public int inputId;

        // 当 isHuman == false 时使用。
        public int aiId;
    }
}
