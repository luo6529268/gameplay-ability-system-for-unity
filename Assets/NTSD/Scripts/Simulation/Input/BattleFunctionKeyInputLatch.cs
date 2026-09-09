using NTSD.App;
using UnityEngine.InputSystem;

namespace NTSD.Simulation
{
    /// <summary>
    /// Legacy R8 diagnostic latch retained until B8 replaces its historical
    /// all-stats/mode2 probe. Formal physical F1-F12 routing no longer uses it.
    /// </summary>
    internal sealed class BattleFunctionKeyInputLatch
    {
        private bool pendingInitializeStatsToggle;
        private int pendingMode2Request;

        internal bool HasPendingRequest =>
            pendingInitializeStatsToggle || pendingMode2Request != 0;

        internal void CapturePhysicalEdges(
            GameConfig config,
            int localGameModeId,
            int battleGameModeId,
            SimulationDriveMode driveMode)
        {
            if (driveMode != SimulationDriveMode.LocalFreeRun)
            {
                Clear();
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            BattleFunctionKeyCommand allowed = config != null
                ? config.ResolveBattleFunctionKeyCommands(localGameModeId, battleGameModeId)
                : BattleFunctionKeyCommand.None;

            if ((allowed & BattleFunctionKeyCommand.InitializeStats) != 0 &&
                keyboard.f7Key.wasPressedThisFrame)
            {
                pendingInitializeStatsToggle = !pendingInitializeStatsToggle;
            }

            if ((allowed & BattleFunctionKeyCommand.SpawnAllWeapons) != 0 &&
                keyboard.f8Key.wasPressedThisFrame)
            {
                pendingMode2Request = 1;
            }

            if ((allowed & BattleFunctionKeyCommand.ClearWeaponPicker) != 0 &&
                keyboard.f9Key.wasPressedThisFrame)
            {
                pendingMode2Request = 2;
            }
        }

        internal void QueueForDiagnostics(BattleFunctionKeyCommand commands)
        {
            if ((commands & BattleFunctionKeyCommand.InitializeStats) != 0)
                pendingInitializeStatsToggle = !pendingInitializeStatsToggle;
            if ((commands & BattleFunctionKeyCommand.SpawnAllWeapons) != 0)
                pendingMode2Request = 1;
            if ((commands & BattleFunctionKeyCommand.ClearWeaponPicker) != 0)
                pendingMode2Request = 2;
        }

        internal bool TryConsume(out bool toggleInitializeStats, out int mode2Request)
        {
            toggleInitializeStats = pendingInitializeStatsToggle;
            mode2Request = pendingMode2Request;
            bool hadRequest = toggleInitializeStats || mode2Request != 0;
            Clear();
            return hadRequest;
        }

        internal void Clear()
        {
            pendingInitializeStatsToggle = false;
            pendingMode2Request = 0;
        }
    }
}
