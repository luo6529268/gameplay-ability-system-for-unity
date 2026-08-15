#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleWorldPendingEventSnapshotEditorTests
    {
        [Test]
        public void CaptureOwnsOrderedChecksumVisibleSounds()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            scope.Driver.World.PendingSounds.Add(
                new PendingSoundEvent("SFX_A", 12, 3));
            scope.Driver.World.PendingSounds.Add(
                new PendingSoundEvent("SFX_B", 14, 4));

            BattleWorldPendingEventSnapshotBuffer destination =
                session.CreatePendingEventSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldPendingEventSnapshot(destination),
                Is.True);

            Assert.That(destination.SoundCount, Is.EqualTo(2));
            Assert.That(destination.GetSound(0).Cue, Is.EqualTo("SFX_A"));
            Assert.That(destination.GetSound(0).WorldX, Is.EqualTo(12));
            Assert.That(destination.GetSound(1).Cue, Is.EqualTo("SFX_B"));
            Assert.That(destination.GetSound(1).Tick, Is.EqualTo(4));
            Assert.That(destination.PendingUnregisterCount, Is.Zero);
            Assert.That(destination.PendingSlotReleasedDestroyCount, Is.Zero);
            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldPendingEventSnapshotBuffer.CurrentSchemaVersion));

            scope.Driver.World.PendingSounds.Clear();
            Assert.That(destination.SoundCount, Is.EqualTo(2));
            Assert.That(destination.GetSound(1).Cue, Is.EqualTo("SFX_B"));
        }

        [Test]
        public void NonemptyLifecycleQueueFailsWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleWorldPendingEventSnapshotBuffer destination =
                session.CreatePendingEventSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldPendingEventSnapshot(destination),
                Is.True);

            int publishedSchema = destination.SchemaVersion;
            scope.Driver.World.BattleBuffersForServices.PendingUnregister.Add(
                new LF2Character());

            Assert.That(
                session.TryCaptureWorldPendingEventSnapshot(destination),
                Is.False);
            Assert.That(destination.SchemaVersion, Is.EqualTo(publishedSchema));
            Assert.That(destination.PendingUnregisterCount, Is.Zero);
        }

        [Test]
        public void CapacityAndInvalidCueFailWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var tooSmall = new BattleWorldPendingEventSnapshotBuffer(1);
            scope.Driver.World.PendingSounds.Add(new PendingSoundEvent("A", 0, 0));
            scope.Driver.World.PendingSounds.Add(new PendingSoundEvent("B", 0, 0));
            Assert.That(
                session.TryCaptureWorldPendingEventSnapshot(tooSmall),
                Is.False);
            Assert.That(tooSmall.SchemaVersion, Is.Zero);

            scope.Driver.World.PendingSounds.Clear();
            scope.Driver.World.PendingSounds.Add(new PendingSoundEvent(" ", 0, 0));
            Assert.That(
                session.TryCaptureWorldPendingEventSnapshot(tooSmall),
                Is.False);
            Assert.That(tooSmall.SchemaVersion, Is.Zero);
        }

        [Test]
        public void WarmPendingEventCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            scope.Driver.World.PendingSounds.Add(
                new PendingSoundEvent("SFX_WARM", 0, 0));
            BattleWorldPendingEventSnapshotBuffer destination =
                session.CreatePendingEventSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldPendingEventSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldPendingEventSnapshot(destination))
                {
                    Assert.Fail($"Pending event capture failed at {index}.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
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
                host = new GameObject("BattleWorldPendingEventSnapshotTests")
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
