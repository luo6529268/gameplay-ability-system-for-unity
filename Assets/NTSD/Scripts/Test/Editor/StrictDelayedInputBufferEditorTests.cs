#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class StrictDelayedInputBufferEditorTests
    {
        [Test]
        public void RequiresExactlyOnePacketForEveryCanonicalHumanSlot()
        {
            LockstepSessionIdentity identity = CreateIdentity();
            var buffer = new StrictDelayedInputBuffer(identity, 8);

            Assert.That(buffer.TrySubmit(Packet(identity, 1, 5, SimulationInputButtons.Attack)),
                Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(buffer.IsFrameReady(1), Is.False);
            Assert.That(buffer.TrySubmit(Packet(identity, 1, 2, SimulationInputButtons.Left)),
                Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(buffer.IsFrameReady(1), Is.True);

            Assert.That(buffer.TryConsumeFrame(1, out FrameInputSet frame, out var reason), Is.True);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(frame.Players[0].PlayerSlot, Is.EqualTo(2));
            Assert.That(frame.Players[1].PlayerSlot, Is.EqualTo(5));
            Assert.That(buffer.TryConsumeFrame(1, out _, out reason), Is.False);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.LateOrConsumedTick));
        }

        [Test]
        public void FutureFramesMayArriveOutOfOrder()
        {
            LockstepSessionIdentity identity = CreateIdentity();
            var buffer = new StrictDelayedInputBuffer(identity, 8);

            SubmitComplete(buffer, identity, 3);
            SubmitComplete(buffer, identity, 1);
            Assert.That(buffer.IsFrameReady(3), Is.True);
            Assert.That(buffer.TryConsumeFrame(1, out _, out _), Is.True);
            Assert.That(buffer.IsFrameReady(3), Is.True);
        }

        [Test]
        public void IdenticalDuplicateIsIdempotentAndConflictIsProtocolError()
        {
            LockstepSessionIdentity identity = CreateIdentity();
            var buffer = new StrictDelayedInputBuffer(identity, 8);
            LockstepFramePacket packet = Packet(identity, 1, 2, SimulationInputButtons.Attack);

            Assert.That(buffer.TrySubmit(packet), Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(buffer.TrySubmit(packet), Is.EqualTo(LockstepProtocolReason.DuplicateIdentical));
            Assert.That(buffer.TrySubmit(Packet(identity, 1, 2, SimulationInputButtons.Jump)),
                Is.EqualTo(LockstepProtocolReason.ConflictingDuplicate));
        }

        [Test]
        public void IdentityPlayerAndCapacityViolationsFailClosed()
        {
            LockstepSessionIdentity identity = CreateIdentity();
            var buffer = new StrictDelayedInputBuffer(identity, 2);

            AssertMismatch(buffer, Packet(identity, 1, 2, SimulationInputButtons.None,
                schemaVersion: identity.SchemaVersion + 1), LockstepProtocolReason.SchemaVersionMismatch);
            AssertMismatch(buffer, Packet(identity, 1, 2, SimulationInputButtons.None,
                sessionId: identity.SessionId + 1), LockstepProtocolReason.SessionIdMismatch);
            AssertMismatch(buffer, Packet(identity, 1, 2, SimulationInputButtons.None,
                seed: identity.Seed + 1), LockstepProtocolReason.SeedMismatch);
            AssertMismatch(buffer, Packet(identity, 1, 2, SimulationInputButtons.None,
                catalogFingerprint: identity.CatalogFingerprint + 1),
                LockstepProtocolReason.CatalogFingerprintMismatch);
            AssertMismatch(buffer, Packet(identity, 1, 2, SimulationInputButtons.None,
                stageFingerprint: identity.StageFingerprint + 1),
                LockstepProtocolReason.StageFingerprintMismatch);
            AssertMismatch(buffer, Packet(identity, 1, 2, SimulationInputButtons.None,
                playerSetFingerprint: identity.PlayerSetFingerprint + 1),
                LockstepProtocolReason.PlayerSetMismatch);
            Assert.That(buffer.TrySubmit(Packet(identity, 1, 4, SimulationInputButtons.None)),
                Is.EqualTo(LockstepProtocolReason.UnknownPlayerSlot));
            Assert.That(buffer.TrySubmit(Packet(identity, 3, 2, SimulationInputButtons.None)),
                Is.EqualTo(LockstepProtocolReason.FutureWindowExceeded));
        }

        [Test]
        public void ResetClearsFramesAndMovesLateBoundary()
        {
            LockstepSessionIdentity identity = CreateIdentity();
            var buffer = new StrictDelayedInputBuffer(identity, 8);
            SubmitComplete(buffer, identity, 2);

            buffer.Reset(5);

            Assert.That(buffer.BufferedFrameCount, Is.Zero);
            Assert.That(buffer.IsFrameReady(2), Is.False);
            Assert.That(buffer.TrySubmit(Packet(identity, 5, 2, SimulationInputButtons.None)),
                Is.EqualTo(LockstepProtocolReason.LateOrConsumedTick));
        }

        [Test]
        public void WarmedSteadyStateConsumes256FramesWithoutManagedAllocation()
        {
            LockstepSessionIdentity identity = CreateIdentity();
            var buffer = new StrictDelayedInputBuffer(identity, 8);
            for (int tick = 1; tick <= 8; tick++)
            {
                buffer.TrySubmit(Packet(identity, tick, 2, SimulationInputButtons.Left));
                buffer.TrySubmit(Packet(identity, tick, 5, SimulationInputButtons.Attack));
                buffer.TryConsumeFrame(tick, out _, out _);
            }

            bool allSucceeded = true;
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 9; tick < 265; tick++)
            {
                LockstepFramePacket left = new LockstepFramePacket(
                    identity, tick, 2, SimulationInputButtons.Left);
                LockstepFramePacket attack = new LockstepFramePacket(
                    identity, tick, 5, SimulationInputButtons.Attack);
                allSucceeded &= buffer.TrySubmit(left) == LockstepProtocolReason.None;
                allSucceeded &= buffer.TrySubmit(attack) == LockstepProtocolReason.None;
                allSucceeded &= buffer.TryConsumeFrame(
                    tick,
                    out FrameInputSet frame,
                    out LockstepProtocolReason reason);
                allSucceeded &= frame != null && reason == LockstepProtocolReason.None;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allSucceeded, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        internal static LockstepSessionIdentity CreateIdentity()
        {
            return new LockstepSessionIdentity(1, 0x1234UL, 99U, 0xCA7UL, 0x57A6EUL,
                new[] { 5, 2 });
        }

        internal static LockstepFramePacket Packet(
            LockstepSessionIdentity identity,
            int tick,
            int slot,
            SimulationInputButtons buttons,
            int? schemaVersion = null,
            ulong? sessionId = null,
            uint? seed = null,
            ulong? catalogFingerprint = null,
            ulong? stageFingerprint = null,
            ulong? playerSetFingerprint = null)
        {
            return new LockstepFramePacket(
                schemaVersion ?? identity.SchemaVersion,
                sessionId ?? identity.SessionId,
                seed ?? identity.Seed,
                catalogFingerprint ?? identity.CatalogFingerprint,
                stageFingerprint ?? identity.StageFingerprint,
                playerSetFingerprint ?? identity.PlayerSetFingerprint,
                tick,
                slot,
                buttons,
                SimulationInputButtons.None,
                SimulationInputButtons.None);
        }

        private static void SubmitComplete(
            StrictDelayedInputBuffer buffer,
            LockstepSessionIdentity identity,
            int tick)
        {
            Assert.That(buffer.TrySubmit(Packet(identity, tick, 5, SimulationInputButtons.None)),
                Is.EqualTo(LockstepProtocolReason.None));
            Assert.That(buffer.TrySubmit(Packet(identity, tick, 2, SimulationInputButtons.None)),
                Is.EqualTo(LockstepProtocolReason.None));
        }

        private static void AssertMismatch(
            StrictDelayedInputBuffer buffer,
            LockstepFramePacket packet,
            LockstepProtocolReason expected)
        {
            Assert.That(buffer.TrySubmit(packet), Is.EqualTo(expected));
        }
    }
}
#endif
