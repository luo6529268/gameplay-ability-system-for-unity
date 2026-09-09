using UnityEngine.InputSystem;

namespace NTSD.Simulation
{
    internal sealed class NTSD28NativeFunctionKeyPhysicalLatch
    {
        private const ushort OneShotTrackedMask = 0x03EC;

        private ushort heldOneShotMask;
        private byte pendingSessionEventByte;
        private bool pendingLeaveBattle;
        private NTSD28NativeFunctionKeyMaintenanceCommand pendingMaintenanceCommand;

        internal NTSD28NativeFunctionKeyHostCommand CurrentContinuousHostCommand
        {
            get;
            private set;
        }

        internal bool HasPendingOneShotHandoff =>
            pendingSessionEventByte != 0 ||
            pendingLeaveBattle ||
            pendingMaintenanceCommand !=
                NTSD28NativeFunctionKeyMaintenanceCommand.None;

        internal void CapturePhysicalEdges(
            SimulationDriveMode driveMode,
            NTSD28NativeFunctionKeyRouteContext context)
        {
            if (driveMode != SimulationDriveMode.LocalFreeRun)
            {
                Clear();
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                Clear();
                return;
            }

            ushort pressedMask = 0;
            if (keyboard.f3Key.isPressed)
                pressedMask |= KeyMask(NTSD28NativeFunctionKey.F3);
            if (keyboard.f4Key.isPressed)
                pressedMask |= KeyMask(NTSD28NativeFunctionKey.F4);
            if (keyboard.f6Key.isPressed)
                pressedMask |= KeyMask(NTSD28NativeFunctionKey.F6);
            if (keyboard.f7Key.isPressed)
                pressedMask |= KeyMask(NTSD28NativeFunctionKey.F7);
            if (keyboard.f8Key.isPressed)
                pressedMask |= KeyMask(NTSD28NativeFunctionKey.F8);
            if (keyboard.f9Key.isPressed)
                pressedMask |= KeyMask(NTSD28NativeFunctionKey.F9);
            if (keyboard.f10Key.isPressed)
                pressedMask |= KeyMask(NTSD28NativeFunctionKey.F10);
            if (keyboard.f11Key.isPressed)
                pressedMask |= KeyMask(NTSD28NativeFunctionKey.F11);
            if (keyboard.f12Key.isPressed)
                pressedMask |= KeyMask(NTSD28NativeFunctionKey.F12);

            bool control = keyboard.leftCtrlKey.isPressed ||
                           keyboard.rightCtrlKey.isPressed;
            CapturePressedMaskForDiagnostics(pressedMask, control, context);
        }

        // Alignment contract: NTSD28-B2-FUNCTION-KEY-PRODUCTION-INTEGRATION-001.
        internal void CapturePressedMaskForDiagnostics(
            ushort pressedMask,
            bool control,
            NTSD28NativeFunctionKeyRouteContext context)
        {
            ushort oneShotPressed = (ushort)(pressedMask & OneShotTrackedMask);
            ushort rising = (ushort)(oneShotPressed & ~heldOneShotMask);
            heldOneShotMask = oneShotPressed;

            var modifiers = new NTSD28NativeFunctionKeyModifiers(control);
            for (int keyValue = (int)NTSD28NativeFunctionKey.F3;
                 keyValue <= (int)NTSD28NativeFunctionKey.F10;
                 keyValue++)
            {
                NTSD28NativeFunctionKey key = (NTSD28NativeFunctionKey)keyValue;
                if ((rising & KeyMask(key)) == 0)
                    continue;
                Collect(NTSD28NativeFunctionKeyRouter.Route(
                    key,
                    false,
                    modifiers,
                    context));
            }

            CurrentContinuousHostCommand = NTSD28NativeFunctionKeyHostCommand.None;
            if ((pressedMask & KeyMask(NTSD28NativeFunctionKey.F11)) != 0)
            {
                Collect(NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F11,
                    false,
                    modifiers,
                    context));
            }
            if ((pressedMask & KeyMask(NTSD28NativeFunctionKey.F12)) != 0)
            {
                Collect(NTSD28NativeFunctionKeyRouter.Route(
                    NTSD28NativeFunctionKey.F12,
                    false,
                    modifiers,
                    context));
            }
        }

        internal bool TryConsumeOneShotHandoff(
            out byte sessionEventByte,
            out bool leaveBattle,
            out NTSD28NativeFunctionKeyMaintenanceCommand maintenanceCommand)
        {
            sessionEventByte = pendingSessionEventByte;
            leaveBattle = pendingLeaveBattle;
            maintenanceCommand = pendingMaintenanceCommand;
            bool hasHandoff = HasPendingOneShotHandoff;
            pendingSessionEventByte = 0;
            pendingLeaveBattle = false;
            pendingMaintenanceCommand =
                NTSD28NativeFunctionKeyMaintenanceCommand.None;
            return hasHandoff;
        }

        internal void Clear()
        {
            heldOneShotMask = 0;
            pendingSessionEventByte = 0;
            pendingLeaveBattle = false;
            pendingMaintenanceCommand =
                NTSD28NativeFunctionKeyMaintenanceCommand.None;
            CurrentContinuousHostCommand =
                NTSD28NativeFunctionKeyHostCommand.None;
        }

        internal static ushort KeyMask(NTSD28NativeFunctionKey key)
        {
            int keyValue = (int)key;
            if (keyValue <= 0 || keyValue > 12)
                return 0;
            return (ushort)(1 << (keyValue - 1));
        }

        private void Collect(NTSD28NativeFunctionKeyRouteResult route)
        {
            switch (route.Disposition)
            {
                case NTSD28NativeFunctionKeyDisposition.SessionCommand:
                    pendingSessionEventByte = (byte)(
                        pendingSessionEventByte |
                        NTSD28NativeFunctionKeySessionState.EventMask(
                            route.SessionCommand));
                    break;
                case NTSD28NativeFunctionKeyDisposition.HostCommand:
                    if (route.HostCommand ==
                        NTSD28NativeFunctionKeyHostCommand.LeaveBattle)
                    {
                        pendingLeaveBattle = true;
                    }
                    break;
                case NTSD28NativeFunctionKeyDisposition.MaintenanceCommand:
                    pendingMaintenanceCommand = route.MaintenanceCommand;
                    break;
                case NTSD28NativeFunctionKeyDisposition.ContinuousHostCommand:
                    CurrentContinuousHostCommand = route.HostCommand;
                    break;
            }
        }
    }
}
