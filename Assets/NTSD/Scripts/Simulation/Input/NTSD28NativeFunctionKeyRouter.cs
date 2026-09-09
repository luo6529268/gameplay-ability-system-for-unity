namespace NTSD.Simulation
{
    internal enum NTSD28NativeFunctionKey : byte
    {
        None = 0,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,
    }

    internal enum NTSD28NativeFunctionKeyDisposition : byte
    {
        NotFunctionKey = 0,
        RejectedAutoRepeat,
        RejectedByContext,
        MaintenanceCommand,
        HostCommand,
        SessionCommand,
        ContinuousHostCommand,
        NativeNoAction,
    }

    internal enum NTSD28NativeFunctionKeyRejectReason : byte
    {
        None = 0,
        AutoRepeat,
        NotInBattle,
        MainStateDisallows,
        F3Locked,
        GlobalDelayActive,
    }

    internal enum NTSD28NativeFunctionKeyHostCommand : byte
    {
        None = 0,
        TogglePause,
        SingleStep,
        LeaveBattle,
        ToggleFastMode,
        VolumeDown,
        VolumeUp,
    }

    internal enum NTSD28NativeFunctionKeySessionCommand : byte
    {
        None = 0,
        ToggleLock,
        ToggleHitResource,
        FillActiveMp,
        DropModeObjects,
        TerminateModeObjects,
    }

    internal enum NTSD28NativeFunctionKeyMaintenanceCommand : byte
    {
        None = 0,
        RetryPendingRecording,
        DiscardPendingRecording,
    }

    internal readonly struct NTSD28NativeFunctionKeyModifiers
    {
        internal static NTSD28NativeFunctionKeyModifiers None =>
            new NTSD28NativeFunctionKeyModifiers(false);

        internal NTSD28NativeFunctionKeyModifiers(bool control)
        {
            Control = control;
        }

        internal bool Control { get; }
    }

    internal readonly struct NTSD28NativeFunctionKeyRouteContext
    {
        internal static NTSD28NativeFunctionKeyRouteContext Allowed =>
            new NTSD28NativeFunctionKeyRouteContext(true, true, false, true);

        internal NTSD28NativeFunctionKeyRouteContext(
            bool battleActive,
            bool mainStateAllows,
            bool f6F9Locked,
            bool globalDelayClear)
        {
            BattleActive = battleActive;
            MainStateAllows = mainStateAllows;
            F6F9Locked = f6F9Locked;
            GlobalDelayClear = globalDelayClear;
        }

        internal bool BattleActive { get; }
        internal bool MainStateAllows { get; }
        internal bool F6F9Locked { get; }
        internal bool GlobalDelayClear { get; }
    }

    internal readonly struct NTSD28NativeFunctionKeyRouteResult
    {
        internal NTSD28NativeFunctionKeyRouteResult(
            NTSD28NativeFunctionKey key,
            NTSD28NativeFunctionKeyDisposition disposition,
            NTSD28NativeFunctionKeyHostCommand hostCommand =
                NTSD28NativeFunctionKeyHostCommand.None,
            NTSD28NativeFunctionKeySessionCommand sessionCommand =
                NTSD28NativeFunctionKeySessionCommand.None,
            NTSD28NativeFunctionKeyMaintenanceCommand maintenanceCommand =
                NTSD28NativeFunctionKeyMaintenanceCommand.None,
            NTSD28NativeFunctionKeyRejectReason rejectReason =
                NTSD28NativeFunctionKeyRejectReason.None)
        {
            Key = key;
            Disposition = disposition;
            HostCommand = hostCommand;
            SessionCommand = sessionCommand;
            MaintenanceCommand = maintenanceCommand;
            RejectReason = rejectReason;
        }

        internal NTSD28NativeFunctionKey Key { get; }
        internal NTSD28NativeFunctionKeyDisposition Disposition { get; }
        internal NTSD28NativeFunctionKeyHostCommand HostCommand { get; }
        internal NTSD28NativeFunctionKeySessionCommand SessionCommand { get; }
        internal NTSD28NativeFunctionKeyMaintenanceCommand MaintenanceCommand { get; }
        internal NTSD28NativeFunctionKeyRejectReason RejectReason { get; }

        internal bool IsFunctionKey => Key != NTSD28NativeFunctionKey.None;

        internal bool IsOneShotCommand =>
            Disposition == NTSD28NativeFunctionKeyDisposition.MaintenanceCommand ||
            Disposition == NTSD28NativeFunctionKeyDisposition.HostCommand ||
            Disposition == NTSD28NativeFunctionKeyDisposition.SessionCommand;
    }

    internal static class NTSD28NativeFunctionKeyRouter
    {
        private const int FirstFunctionVirtualKey = 0x70;
        private const int LastFunctionVirtualKey = 0x7B;

        internal static NTSD28NativeFunctionKeyRouteResult RouteVirtualKey(
            int virtualKey,
            bool repeated)
        {
            return RouteVirtualKey(
                virtualKey,
                repeated,
                NTSD28NativeFunctionKeyModifiers.None,
                NTSD28NativeFunctionKeyRouteContext.Allowed);
        }

        internal static NTSD28NativeFunctionKeyRouteResult RouteVirtualKey(
            int virtualKey,
            bool repeated,
            NTSD28NativeFunctionKeyModifiers modifiers,
            NTSD28NativeFunctionKeyRouteContext context)
        {
            return Route(FromVirtualKey(virtualKey), repeated, modifiers, context);
        }

        internal static NTSD28NativeFunctionKeyRouteResult Route(
            NTSD28NativeFunctionKey key,
            bool repeated)
        {
            return Route(
                key,
                repeated,
                NTSD28NativeFunctionKeyModifiers.None,
                NTSD28NativeFunctionKeyRouteContext.Allowed);
        }

        // Alignment contract: NTSD28-B2-FUNCTION-KEY-ROUTE-CONTRACT-001.
        internal static NTSD28NativeFunctionKeyRouteResult Route(
            NTSD28NativeFunctionKey key,
            bool repeated,
            NTSD28NativeFunctionKeyModifiers modifiers,
            NTSD28NativeFunctionKeyRouteContext context)
        {
            if (key == NTSD28NativeFunctionKey.None)
            {
                return new NTSD28NativeFunctionKeyRouteResult(
                    key,
                    NTSD28NativeFunctionKeyDisposition.NotFunctionKey);
            }

            if (key == NTSD28NativeFunctionKey.F11 ||
                key == NTSD28NativeFunctionKey.F12)
            {
                return new NTSD28NativeFunctionKeyRouteResult(
                    key,
                    NTSD28NativeFunctionKeyDisposition.ContinuousHostCommand,
                    key == NTSD28NativeFunctionKey.F11
                        ? NTSD28NativeFunctionKeyHostCommand.VolumeDown
                        : NTSD28NativeFunctionKeyHostCommand.VolumeUp);
            }

            if (repeated)
            {
                return Rejected(
                    key,
                    NTSD28NativeFunctionKeyDisposition.RejectedAutoRepeat,
                    NTSD28NativeFunctionKeyRejectReason.AutoRepeat);
            }

            if (modifiers.Control && key == NTSD28NativeFunctionKey.F9)
            {
                return new NTSD28NativeFunctionKeyRouteResult(
                    key,
                    NTSD28NativeFunctionKeyDisposition.MaintenanceCommand,
                    maintenanceCommand:
                        NTSD28NativeFunctionKeyMaintenanceCommand.RetryPendingRecording);
            }

            if (modifiers.Control && key == NTSD28NativeFunctionKey.F10)
            {
                return new NTSD28NativeFunctionKeyRouteResult(
                    key,
                    NTSD28NativeFunctionKeyDisposition.MaintenanceCommand,
                    maintenanceCommand:
                        NTSD28NativeFunctionKeyMaintenanceCommand.DiscardPendingRecording);
            }

            if (!context.BattleActive)
            {
                return Rejected(
                    key,
                    NTSD28NativeFunctionKeyDisposition.RejectedByContext,
                    NTSD28NativeFunctionKeyRejectReason.NotInBattle);
            }

            if (!context.MainStateAllows)
            {
                return Rejected(
                    key,
                    NTSD28NativeFunctionKeyDisposition.RejectedByContext,
                    NTSD28NativeFunctionKeyRejectReason.MainStateDisallows);
            }

            bool isF6F9 = key >= NTSD28NativeFunctionKey.F6 &&
                          key <= NTSD28NativeFunctionKey.F9;
            if (isF6F9 && context.F6F9Locked)
            {
                return Rejected(
                    key,
                    NTSD28NativeFunctionKeyDisposition.RejectedByContext,
                    NTSD28NativeFunctionKeyRejectReason.F3Locked);
            }

            bool needsClearGlobalDelay = key == NTSD28NativeFunctionKey.F8 ||
                                         key == NTSD28NativeFunctionKey.F9;
            if (needsClearGlobalDelay && !context.GlobalDelayClear)
            {
                return Rejected(
                    key,
                    NTSD28NativeFunctionKeyDisposition.RejectedByContext,
                    NTSD28NativeFunctionKeyRejectReason.GlobalDelayActive);
            }

            switch (key)
            {
                case NTSD28NativeFunctionKey.F1:
                    return Host(key, NTSD28NativeFunctionKeyHostCommand.TogglePause);
                case NTSD28NativeFunctionKey.F2:
                    return Host(key, NTSD28NativeFunctionKeyHostCommand.SingleStep);
                case NTSD28NativeFunctionKey.F3:
                    return Session(key, NTSD28NativeFunctionKeySessionCommand.ToggleLock);
                case NTSD28NativeFunctionKey.F4:
                    return Host(key, NTSD28NativeFunctionKeyHostCommand.LeaveBattle);
                case NTSD28NativeFunctionKey.F5:
                    return Host(key, NTSD28NativeFunctionKeyHostCommand.ToggleFastMode);
                case NTSD28NativeFunctionKey.F6:
                    return Session(
                        key,
                        NTSD28NativeFunctionKeySessionCommand.ToggleHitResource);
                case NTSD28NativeFunctionKey.F7:
                    return Session(key, NTSD28NativeFunctionKeySessionCommand.FillActiveMp);
                case NTSD28NativeFunctionKey.F8:
                    return Session(
                        key,
                        NTSD28NativeFunctionKeySessionCommand.DropModeObjects);
                case NTSD28NativeFunctionKey.F9:
                    return Session(
                        key,
                        NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects);
                case NTSD28NativeFunctionKey.F10:
                    return new NTSD28NativeFunctionKeyRouteResult(
                        key,
                        NTSD28NativeFunctionKeyDisposition.NativeNoAction);
                default:
                    return new NTSD28NativeFunctionKeyRouteResult(
                        NTSD28NativeFunctionKey.None,
                        NTSD28NativeFunctionKeyDisposition.NotFunctionKey);
            }
        }

        internal static NTSD28NativeFunctionKey FromVirtualKey(int virtualKey)
        {
            if (virtualKey < FirstFunctionVirtualKey ||
                virtualKey > LastFunctionVirtualKey)
            {
                return NTSD28NativeFunctionKey.None;
            }

            return (NTSD28NativeFunctionKey)(
                (virtualKey - FirstFunctionVirtualKey) + 1);
        }

        private static NTSD28NativeFunctionKeyRouteResult Host(
            NTSD28NativeFunctionKey key,
            NTSD28NativeFunctionKeyHostCommand command)
        {
            return new NTSD28NativeFunctionKeyRouteResult(
                key,
                NTSD28NativeFunctionKeyDisposition.HostCommand,
                command);
        }

        private static NTSD28NativeFunctionKeyRouteResult Session(
            NTSD28NativeFunctionKey key,
            NTSD28NativeFunctionKeySessionCommand command)
        {
            return new NTSD28NativeFunctionKeyRouteResult(
                key,
                NTSD28NativeFunctionKeyDisposition.SessionCommand,
                sessionCommand: command);
        }

        private static NTSD28NativeFunctionKeyRouteResult Rejected(
            NTSD28NativeFunctionKey key,
            NTSD28NativeFunctionKeyDisposition disposition,
            NTSD28NativeFunctionKeyRejectReason reason)
        {
            return new NTSD28NativeFunctionKeyRouteResult(
                key,
                disposition,
                rejectReason: reason);
        }
    }
}
