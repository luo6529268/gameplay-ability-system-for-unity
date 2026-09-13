#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28NativeFunctionKeySessionStateEditorTests
    {
        [Test]
        public void ResetForBattle_RestoresAuthorityDefaultsAndExplicitHitResource()
        {
            var state = new NTSD28NativeFunctionKeySessionState();
            state.Apply(NTSD28NativeFunctionKeySessionCommand.ToggleHitResource);
            state.Apply(NTSD28NativeFunctionKeySessionCommand.FillActiveMp);
            state.Queue(NTSD28NativeFunctionKeySessionCommand.DropModeObjects);

            state.ResetForBattle(false);

            Assert.That(state.LockState, Is.Zero);
            Assert.That(state.HitResourceEnabled, Is.False);
            Assert.That(state.F6EventCount, Is.Zero);
            Assert.That(state.F7EventCount, Is.Zero);
            Assert.That(state.F8EventCount, Is.Zero);
            Assert.That(state.F9EventCount, Is.Zero);
            Assert.That(state.PendingFullMp, Is.False);
            Assert.That(
                state.PendingObjectCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyPendingObjectCommand.None));
            Assert.That(state.QueuedEventByte, Is.Zero);
            Assert.That(state.LastAcceptedEventByte, Is.Zero);

            state.ResetForBattle();
            Assert.That(state.HitResourceEnabled, Is.True);
        }

        [Test]
        public void EventMaskAndQueue_FoldOnlyKnownSessionBits()
        {
            var state = new NTSD28NativeFunctionKeySessionState();

            Assert.That(NTSD28NativeFunctionKeySessionState.KnownEventMask, Is.EqualTo(0xF4));
            Assert.That(
                NTSD28NativeFunctionKeySessionState.EventMask(
                    NTSD28NativeFunctionKeySessionCommand.ToggleLock),
                Is.EqualTo(0x04));
            Assert.That(
                NTSD28NativeFunctionKeySessionState.EventMask(
                    NTSD28NativeFunctionKeySessionCommand.ToggleHitResource),
                Is.EqualTo(0x10));
            Assert.That(
                NTSD28NativeFunctionKeySessionState.EventMask(
                    NTSD28NativeFunctionKeySessionCommand.FillActiveMp),
                Is.EqualTo(0x20));
            Assert.That(
                NTSD28NativeFunctionKeySessionState.EventMask(
                    NTSD28NativeFunctionKeySessionCommand.DropModeObjects),
                Is.EqualTo(0x40));
            Assert.That(
                NTSD28NativeFunctionKeySessionState.EventMask(
                    NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects),
                Is.EqualTo(0x80));
            Assert.That(
                state.Queue(NTSD28NativeFunctionKeySessionCommand.None),
                Is.False);
            Assert.That(
                state.Queue(NTSD28NativeFunctionKeySessionCommand.FillActiveMp),
                Is.True);
            Assert.That(
                state.Queue(NTSD28NativeFunctionKeySessionCommand.FillActiveMp),
                Is.True);
            Assert.That(state.QueuedEventByte, Is.EqualTo(0x20));

            Assert.That(state.Dispatch(), Is.EqualTo(0x20));
            Assert.That(state.F7EventCount, Is.EqualTo(1U));
            Assert.That(state.QueuedEventByte, Is.Zero);
        }

        [Test]
        public void Dispatch_F3RunsFirstAndLocksLaterSameWindowCommands()
        {
            var state = new NTSD28NativeFunctionKeySessionState();
            state.Queue(NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects);
            state.Queue(NTSD28NativeFunctionKeySessionCommand.ToggleHitResource);
            state.Queue(NTSD28NativeFunctionKeySessionCommand.ToggleLock);
            state.Queue(NTSD28NativeFunctionKeySessionCommand.DropModeObjects);
            state.Queue(NTSD28NativeFunctionKeySessionCommand.FillActiveMp);

            byte accepted = state.Dispatch();

            Assert.That(accepted, Is.EqualTo(0x04));
            Assert.That(state.LastAcceptedEventByte, Is.EqualTo(0x04));
            Assert.That(state.LockState, Is.EqualTo(2));
            Assert.That(state.HitResourceEnabled, Is.True);
            Assert.That(state.F6EventCount, Is.Zero);
            Assert.That(state.F7EventCount, Is.Zero);
            Assert.That(state.F8EventCount, Is.Zero);
            Assert.That(state.F9EventCount, Is.Zero);
            Assert.That(state.PendingFullMp, Is.False);
            Assert.That(
                state.PendingObjectCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyPendingObjectCommand.None));
        }

        [Test]
        public void Apply_UsesAuthoritySessionGateOrderAndOneWayLock()
        {
            var state = new NTSD28NativeFunctionKeySessionState();
            NTSD28NativeFunctionKeySessionAcceptance outside = state.Apply(
                NTSD28NativeFunctionKeySessionCommand.DropModeObjects,
                new NTSD28NativeFunctionKeySessionContext(false, false, false));
            NTSD28NativeFunctionKeySessionAcceptance wrongMain = state.Apply(
                NTSD28NativeFunctionKeySessionCommand.DropModeObjects,
                new NTSD28NativeFunctionKeySessionContext(true, false, false));
            NTSD28NativeFunctionKeySessionAcceptance delayed = state.Apply(
                NTSD28NativeFunctionKeySessionCommand.DropModeObjects,
                new NTSD28NativeFunctionKeySessionContext(true, true, false));
            NTSD28NativeFunctionKeySessionAcceptance f6IgnoresDelay = state.Apply(
                NTSD28NativeFunctionKeySessionCommand.ToggleHitResource,
                new NTSD28NativeFunctionKeySessionContext(true, true, false));
            NTSD28NativeFunctionKeySessionAcceptance firstLock = state.Apply(
                NTSD28NativeFunctionKeySessionCommand.ToggleLock);
            NTSD28NativeFunctionKeySessionAcceptance secondLock = state.Apply(
                NTSD28NativeFunctionKeySessionCommand.ToggleLock);
            NTSD28NativeFunctionKeySessionAcceptance lockedF7 = state.Apply(
                NTSD28NativeFunctionKeySessionCommand.FillActiveMp);

            Assert.That(outside.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.NotInBattle));
            Assert.That(wrongMain.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.MainStateDisallows));
            Assert.That(delayed.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.GlobalDelayActive));
            Assert.That(f6IgnoresDelay.Accepted, Is.True);
            Assert.That(firstLock.Accepted, Is.True);
            Assert.That(secondLock.Accepted, Is.False);
            Assert.That(secondLock.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.F3Locked));
            Assert.That(lockedF7.RejectReason,
                Is.EqualTo(NTSD28NativeFunctionKeyRejectReason.F3Locked));
        }

        [Test]
        public void Apply_TracksF6F7CountsAndPendingState()
        {
            var state = new NTSD28NativeFunctionKeySessionState();

            Assert.That(
                state.Apply(NTSD28NativeFunctionKeySessionCommand.ToggleHitResource)
                    .Accepted,
                Is.True);
            Assert.That(state.HitResourceEnabled, Is.False);
            Assert.That(
                state.Apply(NTSD28NativeFunctionKeySessionCommand.ToggleHitResource)
                    .Accepted,
                Is.True);
            Assert.That(state.HitResourceEnabled, Is.True);
            Assert.That(state.F6EventCount, Is.EqualTo(2U));

            state.Apply(NTSD28NativeFunctionKeySessionCommand.FillActiveMp);
            state.Apply(NTSD28NativeFunctionKeySessionCommand.FillActiveMp);
            Assert.That(state.F7EventCount, Is.EqualTo(2U));
            Assert.That(state.PendingFullMp, Is.True);
            Assert.That(state.ConsumePendingFullMp(), Is.True);
            Assert.That(state.ConsumePendingFullMp(), Is.False);
        }

        [Test]
        public void Dispatch_SameWindowF8F9AlwaysLeavesTerminatePending()
        {
            var state = new NTSD28NativeFunctionKeySessionState();
            state.Queue(NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects);
            state.Queue(NTSD28NativeFunctionKeySessionCommand.DropModeObjects);

            byte accepted = state.Dispatch();

            Assert.That(accepted, Is.EqualTo(0xC0));
            Assert.That(state.F8EventCount, Is.EqualTo(1U));
            Assert.That(state.F9EventCount, Is.EqualTo(1U));
            Assert.That(
                state.PendingObjectCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyPendingObjectCommand.TerminateObjects));
            Assert.That(
                state.ConsumePendingObjectCommand(),
                Is.EqualTo(NTSD28NativeFunctionKeyPendingObjectCommand.TerminateObjects));
            Assert.That(
                state.ConsumePendingObjectCommand(),
                Is.EqualTo(NTSD28NativeFunctionKeyPendingObjectCommand.None));

            state.ResetForBattle();
            state.Apply(NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects);
            state.Apply(NTSD28NativeFunctionKeySessionCommand.DropModeObjects);
            Assert.That(
                state.PendingObjectCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyPendingObjectCommand.DropObjects),
                "Direct session application must preserve last-accepted-command semantics.");
        }

        [Test]
        public void RuntimeRootReset_RecreatesAndResetsSessionCarrier()
        {
            var runtime = new BattleRuntimeState();
            runtime.FunctionKeys.Apply(
                NTSD28NativeFunctionKeySessionCommand.ToggleHitResource);
            runtime.FunctionKeys.Apply(
                NTSD28NativeFunctionKeySessionCommand.FillActiveMp);

            runtime.Reset();

            Assert.That(runtime.FunctionKeys, Is.Not.Null);
            Assert.That(runtime.FunctionKeys.HitResourceEnabled, Is.True);
            Assert.That(runtime.FunctionKeys.F6EventCount, Is.Zero);
            Assert.That(runtime.FunctionKeys.PendingFullMp, Is.False);
        }

        [Test]
        public void CoreSnapshotAndChecksum_TrackEverySessionCarrierField()
        {
            var world = new SimulationWorld();
            NTSD28NativeFunctionKeySessionState state = world.Runtime.FunctionKeys;
            state.RestoreForSnapshot(
                2,
                false,
                11,
                12,
                13,
                14,
                true,
                NTSD28NativeFunctionKeyPendingObjectCommand.DropObjects,
                0x80,
                0x74);
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = new BattleWorldCoreScalarSnapshot(world, identity);
            ulong before = world.CaptureRuntimeChecksum64(0, null);

            state.RestoreForSnapshot(
                0,
                true,
                0,
                0,
                0,
                0,
                false,
                NTSD28NativeFunctionKeyPendingObjectCommand.None,
                0,
                0);
            ulong after = world.CaptureRuntimeChecksum64(0, null);

            Assert.That(BattleWorldCoreScalarSnapshot.CurrentSchemaVersion, Is.EqualTo(11));
            Assert.That(BattleStateSnapshotBuffer.CurrentSchemaVersion, Is.EqualTo(20));
            Assert.That(BattleLockstepChecksumModule.CurrentSchemaVersion, Is.EqualTo(23));
            Assert.That(snapshot.FunctionKeys.LockState, Is.EqualTo(2));
            Assert.That(snapshot.FunctionKeys.HitResourceEnabled, Is.False);
            Assert.That(snapshot.FunctionKeys.F6EventCount, Is.EqualTo(11U));
            Assert.That(snapshot.FunctionKeys.F7EventCount, Is.EqualTo(12U));
            Assert.That(snapshot.FunctionKeys.F8EventCount, Is.EqualTo(13U));
            Assert.That(snapshot.FunctionKeys.F9EventCount, Is.EqualTo(14U));
            Assert.That(snapshot.FunctionKeys.PendingFullMp, Is.True);
            Assert.That(
                snapshot.FunctionKeys.PendingObjectCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyPendingObjectCommand.DropObjects));
            Assert.That(snapshot.FunctionKeys.QueuedEventByte, Is.EqualTo(0x80));
            Assert.That(snapshot.FunctionKeys.LastAcceptedEventByte, Is.EqualTo(0x74));
            Assert.That(after, Is.Not.EqualTo(before));
        }

        [Test]
        public void DispatchAndConsume_RemainAllocationFreeAfterWarmup()
        {
            var state = new NTSD28NativeFunctionKeySessionState();
            state.Queue(NTSD28NativeFunctionKeySessionCommand.ToggleHitResource);
            state.Dispatch();
            state.ResetForBattle();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            uint checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                state.ResetForBattle();
                state.Queue(NTSD28NativeFunctionKeySessionCommand.ToggleHitResource);
                state.Queue(NTSD28NativeFunctionKeySessionCommand.FillActiveMp);
                state.Queue(NTSD28NativeFunctionKeySessionCommand.DropModeObjects);
                state.Queue(NTSD28NativeFunctionKeySessionCommand.TerminateModeObjects);
                checksum += state.Dispatch();
                checksum += state.ConsumePendingFullMp() ? 1U : 0U;
                checksum += (uint)state.ConsumePendingObjectCommand();
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(checksum, Is.Not.Zero);
        }
    }
}
#endif
