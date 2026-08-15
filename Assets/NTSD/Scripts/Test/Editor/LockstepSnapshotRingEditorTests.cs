#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class LockstepSnapshotRingEditorTests
    {
        [Test]
        public void PeriodicSnapshotsOverwriteOldestCellWithoutBreakingHistoryAlignment()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(
                scope.Driver,
                identity,
                0,
                8,
                8,
                snapshotIntervalTicks: 2,
                snapshotCapacity: 2);

            for (int tick = 1; tick <= 6; tick++)
            {
                Assert.That(
                    session.TryAdvanceManual(CanonicalNeutralFrame(tick),
                        buildPresentation: false),
                    Is.True,
                    $"tick={tick}, reason={session.LastReason}");
            }

            Assert.That(session.SnapshotRing.Count, Is.EqualTo(2));
            Assert.That(session.SnapshotRing.EarliestTick, Is.EqualTo(4));
            Assert.That(session.SnapshotRing.LatestTick, Is.EqualTo(6));
            Assert.That(session.SnapshotRing.NextCaptureTick, Is.EqualTo(8));
            Assert.That(session.SnapshotRing.TryGet(2, out _), Is.False);
            Assert.That(
                session.SnapshotRing.TryGet(4, out BattleStateSnapshotBuffer tickFour),
                Is.True);
            Assert.That(
                session.SnapshotRing.TryGet(6, out BattleStateSnapshotBuffer tickSix),
                Is.True);
            Assert.That(tickFour.CapturedTick, Is.EqualTo(4));
            Assert.That(tickSix.CapturedTick, Is.EqualTo(6));
            Assert.That(tickSix.ProtocolSchemaVersion,
                Is.EqualTo(session.FrameHistory.SchemaVersion));
            Assert.That(tickSix.ProtocolSchemaVersion,
                Is.EqualTo(session.ChecksumHistory.ProtocolSchemaVersion));
            Assert.That(tickSix.IdentityFingerprint,
                Is.EqualTo(session.FrameHistory.IdentityFingerprint));
            Assert.That(tickSix.IdentityFingerprint,
                Is.EqualTo(session.ChecksumHistory.IdentityFingerprint));
            Assert.That(session.FrameHistory.TryGet(6, out _), Is.True);
            Assert.That(session.ChecksumHistory.TryGet(6, out _), Is.True);
        }

        [Test]
        public void ResetUsesCurrentConsumedTickAsTheNextSnapshotCadenceOrigin()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(
                scope.Driver,
                identity,
                0,
                8,
                8,
                snapshotIntervalTicks: 2,
                snapshotCapacity: 2);

            Assert.That(
                session.TryAdvanceManual(CanonicalNeutralFrame(1),
                    buildPresentation: false),
                Is.True);
            Assert.That(
                session.TryAdvanceManual(CanonicalNeutralFrame(2),
                    buildPresentation: false),
                Is.True);
            Assert.That(session.SnapshotRing.Count, Is.EqualTo(1));

            session.Reset();

            Assert.That(session.SnapshotRing.Count, Is.Zero);
            Assert.That(session.SnapshotRing.NextCaptureTick, Is.EqualTo(4));
            Assert.That(session.FrameHistory.Count, Is.Zero);
            Assert.That(session.ChecksumHistory.Count, Is.Zero);
            Assert.That(
                session.TryAdvanceManual(CanonicalNeutralFrame(3),
                    buildPresentation: false),
                Is.True);
            Assert.That(session.SnapshotRing.Count, Is.Zero);
            Assert.That(
                session.TryAdvanceManual(CanonicalNeutralFrame(4),
                    buildPresentation: false),
                Is.True);
            Assert.That(session.SnapshotRing.Count, Is.EqualTo(1));
            Assert.That(session.SnapshotRing.TryGet(4, out _), Is.True);
        }

        [Test]
        public void SnapshotConfigurationMustBeFullyEnabledOrFullyDisabled()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();

            Assert.Throws<ArgumentException>(() => new BattleLockstepSession(
                scope.Driver,
                identity,
                0,
                8,
                8,
                snapshotIntervalTicks: 2,
                snapshotCapacity: 0));
            Assert.Throws<ArgumentException>(() => new BattleLockstepSession(
                scope.Driver,
                identity,
                0,
                8,
                8,
                snapshotIntervalTicks: 0,
                snapshotCapacity: 2));
        }

        [Test]
        public void WarmPeriodicSnapshotSessionDoesNotAllocate()
        {
            const int measuredTicks = 64;
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(
                scope.Driver,
                identity,
                0,
                measuredTicks + 2,
                measuredTicks + 2,
                snapshotIntervalTicks: 1,
                snapshotCapacity: 2);
            var frames = new FrameInputSet[measuredTicks + 1];
            for (int tick = 1; tick <= measuredTicks; tick++)
                frames[tick] = CanonicalNeutralFrame(tick);

            Assert.That(
                session.TryAdvanceManual(frames[1], buildPresentation: false),
                Is.True);
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool allAdvanced = true;
            for (int tick = 2; tick <= measuredTicks; tick++)
            {
                allAdvanced &= session.TryAdvanceManual(
                    frames[tick],
                    buildPresentation: false);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allAdvanced, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private static FrameInputSet CanonicalNeutralFrame(int tick)
        {
            return new FrameInputSet(tick, new[]
            {
                new SimulationPlayerInput(2, SimulationInputButtons.None),
                new SimulationPlayerInput(5, SimulationInputButtons.None),
            });
        }

        private sealed class DriverScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver previous;
            private readonly GameObject host;

            public DriverScope()
            {
                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType.GetField(
                    "<Instance>k__BackingField",
                    flags);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as SimulationTickDriver;
                instanceField.SetValue(null, null);
                host = new GameObject("LockstepSnapshotRingTests")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                Driver.RecreateWorld();
                Driver.SetPaused(true);
            }

            public SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(host);
                instanceField.SetValue(null, previous);
            }
        }
    }
}
#endif
