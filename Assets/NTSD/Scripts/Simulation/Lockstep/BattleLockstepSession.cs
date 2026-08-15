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
            int journalCapacity,
            int frameHistoryCapacity = 0,
            int checksumHistoryCapacity = 0)
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
            int resolvedFrameHistoryCapacity = frameHistoryCapacity == 0
                ? journalCapacity
                : frameHistoryCapacity;
            FrameHistory = new LockstepFrameHistoryRing(
                identity,
                resolvedFrameHistoryCapacity);
            FrameHistory.Reset(currentTick);
            int resolvedChecksumHistoryCapacity = checksumHistoryCapacity == 0
                ? resolvedFrameHistoryCapacity
                : checksumHistoryCapacity;
            ChecksumHistory = new LockstepChecksumHistoryRing(
                identity,
                resolvedChecksumHistoryCapacity);
            ChecksumHistory.Reset(currentTick);
            bootstrapComplete = inputDelayTicks == 0;
            Status = LockstepSessionStatus.WaitingForInput;
            LastReason = LockstepProtocolReason.None;
        }

        public LockstepSessionIdentity Identity => identity;
        public StrictDelayedInputBuffer Buffer { get; }
        public LockstepReplayJournal Journal { get; }
        public LockstepFrameHistoryRing FrameHistory { get; }
        public LockstepChecksumHistoryRing ChecksumHistory { get; }
        public int CurrentTick => currentTick;
        public int InputDelayTicks => inputDelayTicks;
        public bool BootstrapComplete => bootstrapComplete;
        public LockstepSessionStatus Status { get; private set; }
        public LockstepProtocolReason LastReason { get; private set; }

        public bool TryCaptureWorldCoreScalarSnapshot(
            out BattleWorldCoreScalarSnapshot snapshot)
        {
            if (protocolErrorLatched || driver.World == null)
            {
                snapshot = default;
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                snapshot = default;
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            snapshot = driver.World.CaptureWorldCoreScalarSnapshot(identity);
            return true;
        }

        public bool TryCaptureWorldRosterResultsSnapshot(
            BattleWorldRosterResultsSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldRosterResultsSnapshot(
                identity,
                currentTick,
                destination);
        }

        public BattleWorldStageSpawnSnapshotBuffer
            CreateStageSpawnSnapshotBufferForBootstrap()
        {
            if (protocolErrorLatched || driver.World == null)
            {
                return null;
            }

            return new BattleWorldStageSpawnSnapshotBuffer(
                driver.World.RequiredStageSpawnSnapshotEntryCapacity);
        }

        public bool TryCaptureWorldStageSpawnSnapshot(
            BattleWorldStageSpawnSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldStageSpawnSnapshot(
                identity,
                currentTick,
                destination);
        }

        public BattleWorldRuntimeSlotSnapshotBuffer
            CreateRuntimeSlotSnapshotBufferForBootstrap()
        {
            if (protocolErrorLatched || driver.World == null)
            {
                return null;
            }

            return new BattleWorldRuntimeSlotSnapshotBuffer(
                driver.World.RequiredRuntimeSlotSnapshotCapacity);
        }

        public bool TryCaptureWorldRuntimeSlotSnapshot(
            BattleWorldRuntimeSlotSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldRuntimeSlotSnapshot(
                identity,
                currentTick,
                destination);
        }

        public BattleWorldEntityRuntimeSnapshotBuffer
            CreateEntityRuntimeSnapshotBufferForBootstrap()
        {
            if (protocolErrorLatched || driver.World == null)
            {
                return null;
            }

            return new BattleWorldEntityRuntimeSnapshotBuffer(
                driver.World.RequiredEntityRuntimeSnapshotCapacity);
        }

        public bool TryCaptureWorldEntityRuntimeSnapshot(
            BattleWorldEntityRuntimeSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldEntityRuntimeSnapshot(
                identity,
                currentTick,
                destination);
        }

        public BattleWorldEntityBaseShellSnapshotBuffer
            CreateEntityBaseShellSnapshotBufferForBootstrap()
        {
            if (protocolErrorLatched || driver.World == null)
            {
                return null;
            }

            return new BattleWorldEntityBaseShellSnapshotBuffer(
                driver.World.RequiredEntityBaseShellSnapshotCapacity);
        }

        public bool TryCaptureWorldEntityBaseShellSnapshot(
            BattleWorldEntityBaseShellSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldEntityBaseShellSnapshot(
                identity,
                currentTick,
                destination);
        }

        public BattleWorldLivingShellSnapshotBuffer
            CreateLivingShellSnapshotBufferForBootstrap()
        {
            if (protocolErrorLatched || driver.World == null)
            {
                return null;
            }

            return new BattleWorldLivingShellSnapshotBuffer(
                driver.World.RequiredLivingShellSnapshotCapacity);
        }

        public bool TryCaptureWorldLivingShellSnapshot(
            BattleWorldLivingShellSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldLivingShellSnapshot(
                identity,
                currentTick,
                destination);
        }

        public BattleWorldCharacterShellSnapshotBuffer
            CreateCharacterShellSnapshotBufferForBootstrap()
        {
            if (protocolErrorLatched || driver.World == null)
            {
                return null;
            }

            return new BattleWorldCharacterShellSnapshotBuffer(
                driver.World.RequiredCharacterShellSnapshotCapacity);
        }

        public bool TryCaptureWorldCharacterShellSnapshot(
            BattleWorldCharacterShellSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldCharacterShellSnapshot(
                identity,
                currentTick,
                destination);
        }

        public BattleWorldWeaponShellSnapshotBuffer
            CreateWeaponShellSnapshotBufferForBootstrap()
        {
            if (protocolErrorLatched || driver.World == null)
            {
                return null;
            }

            return new BattleWorldWeaponShellSnapshotBuffer(
                driver.World.RequiredWeaponShellSnapshotCapacity);
        }

        public bool TryCaptureWorldWeaponShellSnapshot(
            BattleWorldWeaponShellSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldWeaponShellSnapshot(
                identity,
                currentTick,
                destination);
        }

        public BattleWorldSpecialOtherShellSnapshotBuffer
            CreateSpecialOtherShellSnapshotBufferForBootstrap()
        {
            if (protocolErrorLatched || driver.World == null)
            {
                return null;
            }

            return new BattleWorldSpecialOtherShellSnapshotBuffer(
                driver.World.RequiredSpecialOtherShellSnapshotCapacity);
        }

        public bool TryCaptureWorldSpecialOtherShellSnapshot(
            BattleWorldSpecialOtherShellSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldSpecialOtherShellSnapshot(
                identity,
                currentTick,
                destination);
        }

        public BattleWorldPendingEventSnapshotBuffer
            CreatePendingEventSnapshotBufferForBootstrap()
        {
            if (protocolErrorLatched || driver.World == null)
            {
                return null;
            }

            return driver.World.CreateWorldPendingEventSnapshotBufferForBootstrap();
        }

        public bool TryCaptureWorldPendingEventSnapshot(
            BattleWorldPendingEventSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldPendingEventSnapshot(
                identity,
                currentTick,
                destination);
        }

        public BattleWorldRestSnapshotBuffer
            CreateRestSnapshotBufferForBootstrap()
        {
            if (protocolErrorLatched || driver.World == null)
            {
                return null;
            }

            return driver.World.CreateWorldRestSnapshotBufferForBootstrap();
        }

        public bool TryCaptureWorldRestSnapshot(
            BattleWorldRestSnapshotBuffer destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (protocolErrorLatched || driver.World == null)
            {
                return false;
            }
            if (driver.CurrentTickIndex != currentTick)
            {
                SetStatus(
                    LockstepSessionStatus.ProtocolError,
                    LockstepProtocolReason.DriverTickMismatch);
                return false;
            }

            return driver.World.TryCaptureWorldRestSnapshot(
                identity,
                currentTick,
                destination);
        }

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
            FrameHistory.Reset(currentTick);
            ChecksumHistory.Reset(currentTick);
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
            if (!FrameHistory.TryRecordConsumed(frame, out reason))
            {
                SetStatus(LockstepSessionStatus.ProtocolError, reason);
                return false;
            }
            int checksumSchemaVersion = driver.HasFrameChecksum
                ? BattleLockstepChecksumModule.CurrentSchemaVersion
                : 0;
            if (!ChecksumHistory.TryRecordConsumed(
                    frame.TickIndex,
                    frame.GetCanonicalHash64(),
                    checksumSchemaVersion,
                    driver.LastFrameChecksumValue,
                    out reason))
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
