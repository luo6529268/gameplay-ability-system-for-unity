using System;

namespace NTSD.Simulation.Lockstep
{
    public enum InProcessBattleKernelHostStatus : byte
    {
        Ready = 0,
        Advanced = 1,
        Faulted = 2,
    }

    /// <summary>
    /// Logic-only S0 host. It owns one world and never consults the Unity driver
    /// singleton, a GameObject, rendering state, wall-clock time, or a transport.
    /// </summary>
    public sealed class InProcessBattleKernelHost
    {
        private readonly LockstepStartBarrier barrier;
        private readonly SimulationWorld world;
        private readonly NTSDBattleTickSystem tickSystem;
        private int currentTick;
        private int diagnosticSnapshotCaptureCount;

        public InProcessBattleKernelHost(
            LockstepStartBarrier barrier,
            int replicaIndex,
            int journalCapacity)
            : this(
                barrier,
                replicaIndex,
                journalCapacity,
                InProcessBattleWorldBootstrap.CreateWorldForBarrier(barrier))
        {
        }

        internal InProcessBattleKernelHost(
            LockstepStartBarrier barrier,
            int replicaIndex,
            int journalCapacity,
            SimulationWorld world)
        {
            this.barrier = barrier ?? throw new ArgumentNullException(nameof(barrier));
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            if (replicaIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(replicaIndex));
            if (journalCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(journalCapacity));

            ReplicaIndex = replicaIndex;
            // Alignment contract: CLIENT-FORMAL-KERNEL-WORLD-BOOTSTRAP-FACTORY-SEAM-001.
            InProcessBattleWorldBootstrap.PrepareWorldForHost(this.barrier, this.world);
            tickSystem = new NTSDBattleTickSystem(world);
            Journal = new LockstepReplayJournal(barrier.Identity, journalCapacity);
            FrameHistory = new LockstepFrameHistoryRing(
                barrier.Identity,
                journalCapacity);
            ChecksumHistory = new LockstepChecksumHistoryRing(
                barrier.Identity,
                journalCapacity);
            Status = InProcessBattleKernelHostStatus.Ready;
            LastReason = LockstepProtocolReason.None;
        }

        public LockstepStartBarrier Barrier => barrier;
        public int ReplicaIndex { get; }
        public int CurrentTick => currentTick;
        public InProcessBattleKernelHostStatus Status { get; private set; }
        public LockstepProtocolReason LastReason { get; private set; }
        public Exception LastException { get; private set; }
        public ulong LastInputHash { get; private set; }
        public ulong LastStateChecksum { get; private set; }
        public LockstepReplayJournal Journal { get; }
        public LockstepFrameHistoryRing FrameHistory { get; }
        public LockstepChecksumHistoryRing ChecksumHistory { get; }
        internal int DiagnosticSnapshotCaptureCount => diagnosticSnapshotCaptureCount;
        internal SimulationWorld WorldForDiagnostics => world;

        internal BattleLockstepChecksumSnapshot CaptureDiagnosticSnapshot(
            FrameInputSet frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            if (frame.TickIndex != currentTick)
            {
                throw new InvalidOperationException(
                    "An S0 mismatch snapshot must be captured at the host's current tick.");
            }

            diagnosticSnapshotCaptureCount++;
            return world.CaptureLockstepChecksumSnapshot(frame.TickIndex, frame);
        }

        internal bool CanStep(
            FrameInputSet frame,
            out LockstepProtocolReason reason)
        {
            if (Status == InProcessBattleKernelHostStatus.Faulted)
            {
                reason = LastReason;
                return false;
            }
            if (frame == null || frame.TickIndex != currentTick + 1)
            {
                reason = LockstepProtocolReason.WrongFrameTick;
                return false;
            }
            if (!barrier.IsCanonicalFrame(frame))
            {
                reason = LockstepProtocolReason.NonCanonicalPlayerOrder;
                return false;
            }
            if (!Journal.HasCapacity)
            {
                reason = LockstepProtocolReason.JournalCapacityExceeded;
                return false;
            }
            if (world.Runtime?.Flow == null ||
                world.Runtime.Flow.CurrentTickIndex != currentTick)
            {
                reason = LockstepProtocolReason.DriverTickMismatch;
                return false;
            }

            reason = LockstepProtocolReason.None;
            return true;
        }

        internal bool TryStepOneTick(FrameInputSet frame)
        {
            if (!CanStep(frame, out LockstepProtocolReason reason))
            {
                LatchFault(reason, null);
                return false;
            }

            try
            {
                int tickIndex = frame.TickIndex;
                world.Runtime.Flow.SparkRenderFrame = tickIndex;
                world.ApplyFrameInputSet(frame);
                BattleTickCompletion completion = tickSystem.RunSimulationWorkerTick(
                    tickIndex,
                    buildPresentation: false);
                // Alignment contract: CLIENT-FORMAL-KERNEL-FULL-RETURN-COMMIT-SEAM-001.
                // A partial C++-aligned pass chain is terminal and publishes no tick result.
                if (completion != BattleTickCompletion.FullReturn)
                {
                    LatchFault(LockstepProtocolReason.DriverRejectedFrame, null);
                    return false;
                }

                ulong inputHash = frame.GetCanonicalHash64();
                ulong stateChecksum = world.CaptureRuntimeChecksum64(
                    tickIndex,
                    frame);
                if (!Journal.TryRecordConsumed(frame, out reason) ||
                    !FrameHistory.TryRecordConsumed(frame, out reason) ||
                    !ChecksumHistory.TryRecordConsumed(
                        tickIndex,
                        inputHash,
                        BattleLockstepChecksumModule.CurrentSchemaVersion,
                        stateChecksum,
                        out reason))
                {
                    LatchFault(reason, null);
                    return false;
                }

                currentTick = tickIndex;
                LastInputHash = inputHash;
                LastStateChecksum = stateChecksum;
                LastReason = LockstepProtocolReason.None;
                Status = InProcessBattleKernelHostStatus.Advanced;
                return true;
            }
            catch (Exception exception)
            {
                LatchFault(LockstepProtocolReason.DriverRejectedFrame, exception);
                return false;
            }
        }

        private void LatchFault(
            LockstepProtocolReason reason,
            Exception exception)
        {
            LastReason = reason;
            LastException = exception;
            Status = InProcessBattleKernelHostStatus.Faulted;
        }
    }
}
