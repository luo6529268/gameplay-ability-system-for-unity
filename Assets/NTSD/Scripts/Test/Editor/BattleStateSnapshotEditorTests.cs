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
    public sealed class BattleStateSnapshotEditorTests
    {
        [Test]
        public void AggregatePublishesOnlyWhenEveryDomainMatches()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleStateSnapshotBuffer destination =
                session.CreateBattleStateSnapshotBufferForBootstrap();
            var character = new LF2Character();
            character.ObjectId = 7;
            character.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(character);

            Assert.That(
                session.TryCaptureBattleStateSnapshot(destination),
                Is.True);

            Assert.That(destination.IsValid, Is.True);
            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleStateSnapshotBuffer.CurrentSchemaVersion));
            Assert.That(destination.ProtocolSchemaVersion,
                Is.EqualTo(identity.SchemaVersion));
            Assert.That(destination.IdentityFingerprint,
                Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(destination.CapturedTick, Is.Zero);
            Assert.That(destination.Core.Flow.CurrentTickIndex, Is.Zero);
            Assert.That(destination.RuntimeSlots.GetSlot(3).Claimed, Is.True);
            Assert.That(destination.EntityRuntime.HasEntityRuntime(3), Is.True);
            Assert.That(destination.EntityBaseShell.HasEntity(3), Is.True);
            Assert.That(destination.LivingShell.HasLiving(3), Is.True);
            Assert.That(destination.CharacterShell.HasCharacter(3), Is.True);
        }

        [Test]
        public void FailedComponentCaptureInvalidatesAggregatePublication()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleStateSnapshotBuffer destination =
                session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureBattleStateSnapshot(destination),
                Is.True);

            scope.Driver.World.BattleBuffersForServices.PendingUnregister.Add(
                new LF2Character());

            Assert.That(
                session.TryCaptureBattleStateSnapshot(destination),
                Is.False);
            Assert.That(destination.IsValid, Is.False);
            Assert.That(destination.SchemaVersion, Is.Zero);
            Assert.That(destination.IdentityFingerprint, Is.Zero);
        }

        [Test]
        public void WarmAggregateCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleStateSnapshotBuffer destination =
                session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureBattleStateSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 128; index++)
            {
                if (!session.TryCaptureBattleStateSnapshot(destination))
                {
                    Assert.Fail($"Aggregate capture failed at {index}.");
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
                host = new GameObject("BattleStateSnapshotTests")
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
