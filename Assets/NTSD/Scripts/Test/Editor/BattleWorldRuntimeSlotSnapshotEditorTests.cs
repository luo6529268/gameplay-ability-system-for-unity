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
    public sealed class BattleWorldRuntimeSlotSnapshotEditorTests
    {
        [Test]
        public void CaptureOwnsClaimedIdentityAndUnclaimedGeneration()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleWorldRuntimeSlotSnapshotBuffer destination =
                session.CreateRuntimeSlotSnapshotBufferForBootstrap();

            var released = new LF2OtherObject();
            released.ObjectId = 999;
            released.SetRequiredRuntimeSlot(50);
            scope.Driver.World.Register(released);
            uint claimedGeneration = scope.Driver.World
                .RuntimeSlotTableForModules.GetReadOnlyView(50).Generation;
            scope.Driver.World.Unregister(released);

            var character = new LF2Character();
            character.ObjectId = 7;
            character.Runtime.StableId = 7001;
            character.Runtime.SpawnSemantic = 3;
            character.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(character);

            Assert.That(
                session.TryCaptureWorldRuntimeSlotSnapshot(destination),
                Is.True);

            character.ObjectId = 99;
            character.Runtime.StableId = 9999;
            BattleRuntimeSlotSnapshot claimed = destination.GetSlot(3);
            BattleRuntimeSlotSnapshot unclaimed = destination.GetSlot(50);

            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldRuntimeSlotSnapshotBuffer.CurrentSchemaVersion));
            Assert.That(destination.ProtocolSchemaVersion, Is.EqualTo(identity.SchemaVersion));
            Assert.That(destination.IdentityFingerprint, Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(destination.CapturedTick, Is.EqualTo(0));
            Assert.That(destination.ClaimedCount, Is.EqualTo(1));
            Assert.That(claimed.Claimed, Is.True);
            Assert.That(claimed.Generation, Is.GreaterThan(0));
            Assert.That(claimed.EntityKind, Is.EqualTo(BattleRuntimeEntityKind.Character));
            Assert.That(claimed.StableId, Is.EqualTo(7001));
            Assert.That(claimed.ObjectId, Is.EqualTo(7));
            Assert.That(claimed.SpawnSemantic, Is.EqualTo(3));
            Assert.That(unclaimed.Claimed, Is.False);
            Assert.That(unclaimed.EntityKind, Is.EqualTo(BattleRuntimeEntityKind.None));
            Assert.That(unclaimed.Generation, Is.GreaterThan(claimedGeneration));
        }

        [Test]
        public void CapacityMismatchFailsWithoutOverwritingDestination()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleWorldRuntimeSlotSnapshotBuffer valid =
                session.CreateRuntimeSlotSnapshotBufferForBootstrap();
            Assert.That(session.TryCaptureWorldRuntimeSlotSnapshot(valid), Is.True);

            var wrong = new BattleWorldRuntimeSlotSnapshotBuffer(
                valid.SlotCapacity + 1);
            Assert.That(session.TryCaptureWorldRuntimeSlotSnapshot(wrong), Is.False);
            Assert.That(wrong.SchemaVersion, Is.Zero);
            Assert.That(wrong.ClaimedCount, Is.Zero);
        }

        [Test]
        public void WarmRuntimeSlotCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var character = new LF2Character();
            character.ObjectId = 7;
            character.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(character);
            BattleWorldRuntimeSlotSnapshotBuffer destination =
                session.CreateRuntimeSlotSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldRuntimeSlotSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldRuntimeSlotSnapshot(destination))
                {
                    Assert.Fail($"Runtime-slot capture failed at {index}.");
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
                host = new GameObject("BattleWorldRuntimeSlotSnapshotTests")
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
