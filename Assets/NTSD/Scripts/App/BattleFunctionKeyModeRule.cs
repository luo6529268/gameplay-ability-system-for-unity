using System;

namespace NTSD.App
{
    [Flags]
    public enum BattleFunctionKeyCommand
    {
        None = 0,
        InitializeStats = 1 << 0,
        SpawnAllWeapons = 1 << 1,
        ClearWeaponPicker = 1 << 2,
    }

    [Serializable]
    public sealed class BattleFunctionKeyModeRule
    {
        public int gameModeId;
        public int battleGameModeId = 1;
        public bool enableF7;
        public bool enableF8;
        public bool enableF9;

        public bool Matches(int localGameModeId, int currentBattleGameModeId)
        {
            return gameModeId == localGameModeId &&
                   battleGameModeId == currentBattleGameModeId;
        }

        public BattleFunctionKeyCommand GetAllowedCommands()
        {
            BattleFunctionKeyCommand commands = BattleFunctionKeyCommand.None;
            if (enableF7)
                commands |= BattleFunctionKeyCommand.InitializeStats;
            if (enableF8)
                commands |= BattleFunctionKeyCommand.SpawnAllWeapons;
            if (enableF9)
                commands |= BattleFunctionKeyCommand.ClearWeaponPicker;
            return commands;
        }
    }
}
