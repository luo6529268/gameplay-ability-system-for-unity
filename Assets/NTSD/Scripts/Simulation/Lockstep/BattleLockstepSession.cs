using System;

namespace NTSD.Simulation.Lockstep
{
    public sealed class BattleLockstepSession
    {
        private readonly SimulationTickDriver driver;
        private readonly LockstepSessionIdentity identity;
        private readonly int inputDelayTicks;

        private int currentTick;
        private bool bootstrapComplete;
        private bool protocolErrorLatched;

        public BattleLockstepSession(
            SimulationTickDriver driver,
            LockstepSessionIdentity identity,
            int inputDelayTicks,
            int futureFrameCapacity,
            int journalCapacity)
        {
            this.driver = driver ?? throw new ArgumentNullException(nameof(driver));
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
            if (inputDelayTicks < 0)
                throw new ArgumentOutOfRangeException(nameof(inputDelayTicks));
            if (futureFrameCapacity <= inputDelayTicks)
            {
                throw new ArgumentException(
                    "The future frame capacity must exceed the configured input delay.",
                    nameof(futureFrameCapacity));
            }

            this.inputDelayTicks = inputDelayTicks;
            currentTick = driver.CurrentTickIndex;
            Buffer = new StrictDelayedInputBuffer(identity, futureFrameCapacity);
            Buffer.Reset(currentTick);
            Journal = new LockstepReplayJournal(identity, journalCapacity);
            Journal.Reset(currentTick);
            bootstrapComplete = inputDelayTicks == 0;
            Status = LockstepSessionStatus.WaitingForInput;
            LastReason = LockstepProtocolReason.None;
        }

        public LockstepSessionIdentity Identity => identity;
        public StrictDelayedInputBuffer Buffer { get; }
        public LockstepReplayJournal Journal { get; }
        public int CurrentTick => currentTick;
        public int InputDelayTicks => inputDelayTicks;
        public bool BootstrapComplete => bootstrapComplete;
        public LockstepSessionStatus Status { get; private set; }
        public LockstepProtocolReason LastReason { get; private set; }

        public LockstepProtocolReason SubmitLocal(
            int playerSlot,
            SimulationInputButtons buttons,
            SimulationInputButtons pressedButtons = SimulationInputButtons.None,
            SimulationInputButtons releasedButtons = SimulationInputButtons.None)
        {
            int targetTick = currentTick + inputDelayTicks + 1;
            var packet = new LockstepFramePacket(
                identity,
                targetTick,
                playerSlot,
                buttons,
                pressedButtons,
                releasedButtons);
            return SubmitBuffered(packet);
        }

        public LockstepProtocolReason SubmitBuffered(in LockstepFramePacket packet)
        {
            if (protocolErrorLatched)
                return LastReason;
            LockstepProtocolReason reason = Buffer.TrySubmit(packet);
            SetSubmitResult(reason);
            return reason;
        }

        public bool BootstrapNeutralDelayFrames()
        {
            if (protocolErrorLatched)
                return false;
            if (bootstrapComplete)
            {
                SetStatus(LockstepSessionStatus.Ready, LockstepProtocolReason.None);
                return true;
            }
            if (currentTick != driver.CurrentTickIndex)
            {
                SetStatus(LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            for (int delayIndex = 1; delayIndex <= inputDelayTicks; delayIndex++)
            {
                int tick = currentTick + delayIndex;
                for (int playerIndex = 0; playerIndex < identity.PlayerCount; playerIndex++)
                {
                    var neutral = new LockstepFramePacket(
                        identity,
                        tick,
                        identity.CanonicalPlayerSlots[playerIndex],
                        SimulationInputButtons.None);
                    LockstepProtocolReason reason = Buffer.TrySubmit(neutral);
                    if (reason != LockstepProtocolReason.None &&
                        reason != LockstepProtocolReason.DuplicateIdentical)
                    {
                        SetStatus(LockstepSessionStatus.ProtocolError, reason);
                        return false;
                    }
                }
            }

            bootstrapComplete = true;
            SetStatus(LockstepSessionStatus.Ready, LockstepProtocolReason.None);
            return true;
        }

        public bool TryAdvanceBuffered(bool ignorePaused = true, bool buildPresentation = true)
        {
            if (protocolErrorLatched)
                return false;
            if (!bootstrapComplete)
            {
                SetStatus(LockstepSessionStatus.WaitingForInput,
                    LockstepProtocolReason.BootstrapRequired);
                return false;
            }
            if (!ValidateDriverBoundary(ignorePaused))
                return false;
            if (!Journal.HasCapacity)
            {
                SetStatus(LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.JournalCapacityExceeded);
                return false;
            }

            int nextTick = currentTick + 1;
            if (!Buffer.TryConsumeFrame(nextTick, out FrameInputSet frame, out var reason))
            {
                SetStatus(
                    reason == LockstepProtocolReason.FrameNotReady
                        ? LockstepSessionStatus.WaitingForInput
                        : LockstepSessionStatus.ProtocolError,
                    reason);
                return false;
            }

            return ApplyExplicitFrame(frame, ignorePaused, buildPresentation);
        }

        public bool TryAdvanceManual(
            FrameInputSet frame,
            bool ignorePaused = true,
            bool buildPresentation = true)
        {
            if (protocolErrorLatched)
                return false;
            if (!ValidateDriverBoundary(ignorePaused))
                return false;
            if (frame == null || frame.TickIndex != currentTick + 1)
            {
                SetStatus(LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.WrongFrameTick);
                return false;
            }
            if (!frame.IsCanonicalFor(frame.TickIndex, identity.CanonicalPlayerSlots))
            {
                SetStatus(LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.NonCanonicalPlayerOrder);
                return false;
            }
            if (!Journal.HasCapacity)
            {
                SetStatus(LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.JournalCapacityExceeded);
                return false;
            }

            bool applied = ApplyExplicitFrame(frame, ignorePaused, buildPresentation);
            if (applied)
                Buffer.Reset(currentTick);
            return applied;
        }

        public void Reset()
        {
            currentTick = driver.CurrentTickIndex;
            Buffer.Reset(currentTick);
            Journal.Reset(currentTick);
            bootstrapComplete = inputDelayTicks == 0;
            protocolErrorLatched = false;
            SetStatus(LockstepSessionStatus.WaitingForInput, LockstepProtocolReason.None);
        }

        private bool ApplyExplicitFrame(
            FrameInputSet frame,
            bool ignorePaused,
            bool buildPresentation)
        {
            if (!driver.StepOneTick(frame, ignorePaused, buildPresentation))
            {
                SetStatus(LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverRejectedFrame);
                return false;
            }
            if (!Journal.TryRecordConsumed(frame, out var reason))
            {
                SetStatus(LockstepSessionStatus.ProtocolError, reason);
                return false;
            }

            currentTick = frame.TickIndex;
            SetStatus(LockstepSessionStatus.Advanced, LockstepProtocolReason.None);
            return true;
        }

        private bool ValidateDriverBoundary(bool ignorePaused)
        {
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }
            if (!ignorePaused && driver.IsPaused)
            {
                SetStatus(LockstepSessionStatus.WaitingForInput,
                    LockstepProtocolReason.DriverRejectedFrame);
                return false;
            }
            return true;
        }

        private void SetSubmitResult(LockstepProtocolReason reason)
        {
            if (reason == LockstepProtocolReason.None ||
                reason == LockstepProtocolReason.DuplicateIdentical)
            {
                SetStatus(
                    Buffer.IsFrameReady(currentTick + 1)
                        ? LockstepSessionStatus.Ready
                        : LockstepSessionStatus.WaitingForInput,
                    reason);
                return;
            }

            SetStatus(LockstepSessionStatus.ProtocolError, reason);
        }

        private void SetStatus(LockstepSessionStatus status, LockstepProtocolReason reason)
        {
            Status = status;
            LastReason = reason;
            if (status == LockstepSessionStatus.ProtocolError)
                protocolErrorLatched = true;
        }
    }
}
