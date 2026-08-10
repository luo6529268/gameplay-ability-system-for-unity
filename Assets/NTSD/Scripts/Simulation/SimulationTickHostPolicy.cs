using System;

namespace NTSD.Simulation
{
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

        public override SimulationDriveMode DriveMode => SimulationDriveMode.LocalFreeRun;
        public override bool UsesWallClock => true;
        public override float Accumulator => accumulator;

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
            int maximumBacklogTicks = Math.Max(
                settings.maxBacklogTicks,
                settings.maxCatchUpTicksPerFrame);
            float maximumAccumulator =
                SimulationConstants.SIM_DT * maximumBacklogTicks;
            if (accumulator > maximumAccumulator)
                accumulator = maximumAccumulator;
        }

        public override bool ShouldAttemptAutomaticTick(
            int ticksAlreadyExecuted,
            LockstepSimulationSettings settings)
        {
            return settings != null &&
                   ticksAlreadyExecuted < settings.maxCatchUpTicksPerFrame &&
                   accumulator >= SimulationConstants.SIM_DT;
        }

        public override bool ShouldBuildPresentationForNextTick(
            int ticksAlreadyExecuted,
            LockstepSimulationSettings settings)
        {
            if (settings == null)
                return true;

            float remainingAfterNextTick = accumulator - SimulationConstants.SIM_DT;
            return remainingAfterNextTick < SimulationConstants.SIM_DT ||
                   ticksAlreadyExecuted + 1 >= settings.maxCatchUpTicksPerFrame;
        }

        public override void CommitAutomaticTick()
        {
            accumulator = Math.Max(0f, accumulator - SimulationConstants.SIM_DT);
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
