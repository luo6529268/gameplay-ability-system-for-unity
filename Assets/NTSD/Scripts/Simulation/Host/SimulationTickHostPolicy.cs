using System;

namespace NTSD.Simulation
{
    internal enum SimulationHostCadenceMode
    {
        Normal,
        Fast,
    }

    internal static class SimulationHostCadence
    {
        internal const int MaximumDebtIntervals = 2;

        internal static float IntervalSeconds(SimulationHostCadenceMode mode)
        {
            return mode == SimulationHostCadenceMode.Fast
                ? SimulationConstants.FAST_SIM_DT
                : SimulationConstants.SIM_DT;
        }
    }

    [Flags]
    internal enum SimulationHostControlCommand
    {
        None = 0,
        TogglePause = 1 << 0,
        SingleStep = 1 << 1,
        ToggleFastMode = 1 << 2,
    }

    internal readonly struct SimulationHostControlState
    {
        internal SimulationHostControlState(
            bool paused,
            SimulationHostCadenceMode cadenceMode)
        {
            Paused = paused;
            CadenceMode = cadenceMode;
        }

        internal bool Paused { get; }
        internal SimulationHostCadenceMode CadenceMode { get; }
    }

    internal readonly struct SimulationHostControlTransition
    {
        internal SimulationHostControlTransition(
            SimulationHostControlState state,
            bool requestSingleStep,
            bool cadenceChanged)
        {
            State = state;
            RequestSingleStep = requestSingleStep;
            CadenceChanged = cadenceChanged;
        }

        internal SimulationHostControlState State { get; }
        internal bool RequestSingleStep { get; }
        internal bool CadenceChanged { get; }
    }

    internal static class SimulationHostControl
    {
        internal static SimulationHostControlTransition Apply(
            SimulationHostControlCommand commands,
            SimulationHostControlState current)
        {
            bool paused = current.Paused;
            SimulationHostCadenceMode cadenceMode = current.CadenceMode;
            if ((commands & SimulationHostControlCommand.TogglePause) != 0)
                paused = !paused;

            bool requestSingleStep =
                (commands & SimulationHostControlCommand.SingleStep) != 0 &&
                paused;
            bool cadenceChanged =
                (commands & SimulationHostControlCommand.ToggleFastMode) != 0;
            if (cadenceChanged)
            {
                cadenceMode = cadenceMode == SimulationHostCadenceMode.Fast
                    ? SimulationHostCadenceMode.Normal
                    : SimulationHostCadenceMode.Fast;
            }

            return new SimulationHostControlTransition(
                new SimulationHostControlState(paused, cadenceMode),
                requestSingleStep,
                cadenceChanged);
        }
    }

    internal sealed class SimulationHostControlPhysicalEdgeLatch
    {
        private bool f1Held;
        private bool f2Held;
        private bool f5Held;

        internal SimulationHostControlCommand Capture(
            bool f1Pressed,
            bool f2Pressed,
            bool f5Pressed)
        {
            SimulationHostControlCommand commands =
                SimulationHostControlCommand.None;
            if (f1Pressed && !f1Held)
                commands |= SimulationHostControlCommand.TogglePause;
            if (f2Pressed && !f2Held)
                commands |= SimulationHostControlCommand.SingleStep;
            if (f5Pressed && !f5Held)
                commands |= SimulationHostControlCommand.ToggleFastMode;

            f1Held = f1Pressed;
            f2Held = f2Pressed;
            f5Held = f5Pressed;
            return commands;
        }

        internal void Clear()
        {
            f1Held = false;
            f2Held = false;
            f5Held = false;
        }
    }

    internal abstract class SimulationTickHostPolicy
    {
        public abstract SimulationDriveMode DriveMode { get; }
        public abstract bool UsesWallClock { get; }
        public abstract float Accumulator { get; }

        public abstract void BeginUpdate(
            float elapsedSeconds,
            LockstepSimulationSettings settings);

        public abstract bool ShouldAttemptAutomaticTick(
            int ticksAlreadyExecuted,
            LockstepSimulationSettings settings);

        public abstract bool ShouldBuildPresentationForNextTick(
            int ticksAlreadyExecuted,
            LockstepSimulationSettings settings);

        public abstract void CommitAutomaticTick();
        public abstract void Reset();
    }

    internal sealed class OfflineLocalTickPolicy : SimulationTickHostPolicy
    {
        private float accumulator;
        private SimulationHostCadenceMode cadenceMode;

        public override SimulationDriveMode DriveMode => SimulationDriveMode.LocalFreeRun;
        public override bool UsesWallClock => true;
        public override float Accumulator => accumulator;
        internal SimulationHostCadenceMode CadenceMode => cadenceMode;
        internal float ActiveIntervalSeconds =>
            SimulationHostCadence.IntervalSeconds(cadenceMode);

        internal bool SetCadenceMode(SimulationHostCadenceMode nextMode)
        {
            if (cadenceMode == nextMode)
                return false;

            cadenceMode = nextMode;
            accumulator = 0f;
            return true;
        }

        public override void BeginUpdate(
            float elapsedSeconds,
            LockstepSimulationSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds) ||
                elapsedSeconds < 0f)
            {
                elapsedSeconds = 0f;
            }

            accumulator += elapsedSeconds;
            float maximumAccumulator =
                ActiveIntervalSeconds * SimulationHostCadence.MaximumDebtIntervals;
            if (accumulator > maximumAccumulator)
                accumulator = maximumAccumulator;
        }

        public override bool ShouldAttemptAutomaticTick(
            int ticksAlreadyExecuted,
            LockstepSimulationSettings settings)
        {
            return settings != null &&
                   ticksAlreadyExecuted <
                       SimulationHostCadence.MaximumDebtIntervals &&
                   accumulator >= ActiveIntervalSeconds;
        }

        public override bool ShouldBuildPresentationForNextTick(
            int ticksAlreadyExecuted,
            LockstepSimulationSettings settings)
        {
            return true;
        }

        public override void CommitAutomaticTick()
        {
            accumulator = Math.Max(0f, accumulator - ActiveIntervalSeconds);
        }

        public override void Reset()
        {
            accumulator = 0f;
        }
    }

    internal sealed class ManualReplayTickPolicy : SimulationTickHostPolicy
    {
        public override SimulationDriveMode DriveMode => SimulationDriveMode.Manual;
        public override bool UsesWallClock => false;
        public override float Accumulator => 0f;

        public override void BeginUpdate(
            float elapsedSeconds,
            LockstepSimulationSettings settings)
        {
        }

        public override bool ShouldAttemptAutomaticTick(
            int ticksAlreadyExecuted,
            LockstepSimulationSettings settings)
        {
            return false;
        }

        public override bool ShouldBuildPresentationForNextTick(
            int ticksAlreadyExecuted,
            LockstepSimulationSettings settings)
        {
            return true;
        }

        public override void CommitAutomaticTick()
        {
        }

        public override void Reset()
        {
        }
    }

    internal sealed class NetworkLockstepTickPolicy : SimulationTickHostPolicy
    {
        public override SimulationDriveMode DriveMode => SimulationDriveMode.LockstepBuffered;
        public override bool UsesWallClock => false;
        public override float Accumulator => 0f;

        public override void BeginUpdate(
            float elapsedSeconds,
            LockstepSimulationSettings settings)
        {
        }

        public override bool ShouldAttemptAutomaticTick(
            int ticksAlreadyExecuted,
            LockstepSimulationSettings settings)
        {
            // Authoritative frames are consumed explicitly by BattleLockstepSession.
            // A future server-frame-gap policy can supply a bounded automatic budget
            // here without reintroducing Unity wall-clock ownership.
            return false;
        }

        public override bool ShouldBuildPresentationForNextTick(
            int ticksAlreadyExecuted,
            LockstepSimulationSettings settings)
        {
            return true;
        }

        public override void CommitAutomaticTick()
        {
        }

        public override void Reset()
        {
        }
    }
}
