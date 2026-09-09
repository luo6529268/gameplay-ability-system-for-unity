namespace NTSD.Simulation
{
    public enum NTSD28NativeFunctionKeyPendingObjectCommand : byte
    {
        None = 0,
        DropObjects = 1,
        TerminateObjects = 2,
    }

    internal readonly struct NTSD28NativeFunctionKeySessionContext
    {
        internal static NTSD28NativeFunctionKeySessionContext Allowed =>
            new NTSD28NativeFunctionKeySessionContext(true, true, true);

        internal NTSD28NativeFunctionKeySessionContext(
            bool battleActive,
            bool mainStateAllows,
            bool globalDelayClear)
        {
            BattleActive = battleActive;
            MainStateAllows = mainStateAllows;
            GlobalDelayClear = globalDelayClear;
        }

        internal bool BattleActive { get; }
        internal bool MainStateAllows { get; }
        internal bool GlobalDelayClear { get; }
    }

    internal readonly struct NTSD28NativeFunctionKeySessionAcceptance
    {
        internal NTSD28NativeFunctionKeySessionAcceptance(
            NTSD28NativeFunctionKeySessionCommand command,
            bool accepted,
            NTSD28NativeFunctionKeyRejectReason rejectReason)
        {
            Command = command;
            Accepted = accepted;
            RejectReason = rejectReason;
        }

        internal NTSD28NativeFunctionKeySessionCommand Command { get; }
        internal bool Accepted { get; }
        internal NTSD28NativeFunctionKeyRejectReason RejectReason { get; }
    }

    internal sealed class NTSD28NativeFunctionKeySessionState
    {
        internal const byte KnownEventMask = 0xF4;

        internal NTSD28NativeFunctionKeySessionState()
        {
            ResetForBattle();
        }

        internal int LockState { get; private set; }
        internal bool HitResourceEnabled { get; private set; }
        internal uint F6EventCount { get; private set; }
        internal uint F7EventCount { get; private set; }
        internal uint F8EventCount { get; private set; }
        internal uint F9EventCount { get; private set; }
        internal bool PendingFullMp { get; private set; }
        internal NTSD28NativeFunctionKeyPendingObjectCommand PendingObjectCommand
        {
            get;
            private set;
        }
        internal byte QueuedEventByte { get; private set; }
        internal byte LastAcceptedEventByte { get; private set; }

        internal void ResetForBattle(bool initialHitResourceEnabled = true)
        {
            LockState = 0;
            HitResourceEnabled = initialHitResourceEnabled;
            F6EventCount = 0;
            F7EventCount = 0;
            F8EventCount = 0;
            F9EventCount = 0;
            PendingFullMp = false;
            PendingObjectCommand = NTSD28NativeFunctionKeyPendingObjectCommand.None;
            QueuedEventByte = 0;
            LastAcceptedEventByte = 0;
        }

        internal static byte EventMask(
            NTSD28NativeFunctionKeySessionCommand command)
        {
            switch (command)
            {
                case NTSD28NativeFunctionKeySessionCommand.ToggleLock:
                    return 0x04;
                case NTSD28NativeFunctionKeySessionCommand.ToggleHitResource:
                    return 0x10;
                case NTSD28NativeFunctionKeySessionCommand.FillActiveMp:
                    return 0x20;
                case NTSD28NativeFunctionKeySessionCommand.DropModeObjects:
                    return 0x40;
                case NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects:
                    return 0x80;
                default:
                    return 0;
            }
        }

        internal bool Queue(NTSD28NativeFunctionKeySessionCommand command)
        {
            byte mask = EventMask(command);
            if (mask == 0)
                return false;

            QueuedEventByte = (byte)(QueuedEventByte | mask);
            return true;
        }

        internal bool QueueEventByte(byte eventByte)
        {
            byte masked = (byte)(eventByte & KnownEventMask);
            if (masked == 0)
                return false;

            QueuedEventByte = (byte)(QueuedEventByte | masked);
            return true;
        }

        internal byte Dispatch()
        {
            return Dispatch(NTSD28NativeFunctionKeySessionContext.Allowed);
        }

        // Alignment contract: NTSD28-B2-FUNCTION-KEY-SESSION-STATE-CARRIER-001.
        internal byte Dispatch(NTSD28NativeFunctionKeySessionContext context)
        {
            byte queued = QueuedEventByte;
            QueuedEventByte = 0;
            LastAcceptedEventByte = 0;

            DispatchQueued(
                queued,
                NTSD28NativeFunctionKeySessionCommand.ToggleLock,
                context);
            DispatchQueued(
                queued,
                NTSD28NativeFunctionKeySessionCommand.ToggleHitResource,
                context);
            DispatchQueued(
                queued,
                NTSD28NativeFunctionKeySessionCommand.FillActiveMp,
                context);
            DispatchQueued(
                queued,
                NTSD28NativeFunctionKeySessionCommand.DropModeObjects,
                context);
            DispatchQueued(
                queued,
                NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects,
                context);

            return LastAcceptedEventByte;
        }

        internal NTSD28NativeFunctionKeySessionAcceptance Apply(
            NTSD28NativeFunctionKeySessionCommand command)
        {
            return Apply(command, NTSD28NativeFunctionKeySessionContext.Allowed);
        }

        internal NTSD28NativeFunctionKeySessionAcceptance Apply(
            NTSD28NativeFunctionKeySessionCommand command,
            NTSD28NativeFunctionKeySessionContext context)
        {
            if (command == NTSD28NativeFunctionKeySessionCommand.None)
                return Acceptance(command, false);
            if (!context.BattleActive)
            {
                return Acceptance(
                    command,
                    false,
                    NTSD28NativeFunctionKeyRejectReason.NotInBattle);
            }
            if (!context.MainStateAllows)
            {
                return Acceptance(
                    command,
                    false,
                    NTSD28NativeFunctionKeyRejectReason.MainStateDisallows);
            }

            if (command == NTSD28NativeFunctionKeySessionCommand.ToggleLock)
            {
                if (LockState == 2)
                {
                    return Acceptance(
                        command,
                        false,
                        NTSD28NativeFunctionKeyRejectReason.F3Locked);
                }

                LockState = 2;
                return Acceptance(command, true);
            }

            if (LockState == 2)
            {
                return Acceptance(
                    command,
                    false,
                    NTSD28NativeFunctionKeyRejectReason.F3Locked);
            }

            bool needsClearGlobalDelay =
                command == NTSD28NativeFunctionKeySessionCommand.DropModeObjects ||
                command == NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects;
            if (needsClearGlobalDelay && !context.GlobalDelayClear)
            {
                return Acceptance(
                    command,
                    false,
                    NTSD28NativeFunctionKeyRejectReason.GlobalDelayActive);
            }

            switch (command)
            {
                case NTSD28NativeFunctionKeySessionCommand.ToggleHitResource:
                    F6EventCount++;
                    HitResourceEnabled = !HitResourceEnabled;
                    break;
                case NTSD28NativeFunctionKeySessionCommand.FillActiveMp:
                    F7EventCount++;
                    PendingFullMp = true;
                    break;
                case NTSD28NativeFunctionKeySessionCommand.DropModeObjects:
                    F8EventCount++;
                    PendingObjectCommand =
                        NTSD28NativeFunctionKeyPendingObjectCommand.DropObjects;
                    break;
                case NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects:
                    F9EventCount++;
                    PendingObjectCommand =
                        NTSD28NativeFunctionKeyPendingObjectCommand.TerminateObjects;
                    break;
            }

            return Acceptance(command, true);
        }

        internal bool ConsumePendingFullMp()
        {
            bool pending = PendingFullMp;
            PendingFullMp = false;
            return pending;
        }

        internal NTSD28NativeFunctionKeyPendingObjectCommand
            ConsumePendingObjectCommand()
        {
            NTSD28NativeFunctionKeyPendingObjectCommand pending =
                PendingObjectCommand;
            PendingObjectCommand = NTSD28NativeFunctionKeyPendingObjectCommand.None;
            return pending;
        }

        internal void RestoreForSnapshot(
            int lockState,
            bool hitResourceEnabled,
            uint f6EventCount,
            uint f7EventCount,
            uint f8EventCount,
            uint f9EventCount,
            bool pendingFullMp,
            NTSD28NativeFunctionKeyPendingObjectCommand pendingObjectCommand,
            byte queuedEventByte,
            byte lastAcceptedEventByte)
        {
            LockState = lockState;
            HitResourceEnabled = hitResourceEnabled;
            F6EventCount = f6EventCount;
            F7EventCount = f7EventCount;
            F8EventCount = f8EventCount;
            F9EventCount = f9EventCount;
            PendingFullMp = pendingFullMp;
            PendingObjectCommand = pendingObjectCommand;
            QueuedEventByte = (byte)(queuedEventByte & KnownEventMask);
            LastAcceptedEventByte = (byte)(lastAcceptedEventByte & KnownEventMask);
        }

        private void DispatchQueued(
            byte queued,
            NTSD28NativeFunctionKeySessionCommand command,
            NTSD28NativeFunctionKeySessionContext context)
        {
            byte mask = EventMask(command);
            if ((queued & mask) == 0)
                return;

            NTSD28NativeFunctionKeySessionAcceptance acceptance =
                Apply(command, context);
            if (acceptance.Accepted)
                LastAcceptedEventByte = (byte)(LastAcceptedEventByte | mask);
        }

        private static NTSD28NativeFunctionKeySessionAcceptance Acceptance(
            NTSD28NativeFunctionKeySessionCommand command,
            bool accepted,
            NTSD28NativeFunctionKeyRejectReason reason =
                NTSD28NativeFunctionKeyRejectReason.None)
        {
            return new NTSD28NativeFunctionKeySessionAcceptance(
                command,
                accepted,
                reason);
        }
    }
}
