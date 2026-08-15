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
    public sealed class BattleWorldLivingShellSnapshotEditorTests
    {
        [Test]
        public void CaptureOwnsLivingReferencesDeadAndFractionalRecovery()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);

            var catching = new LF2Character();
            catching.SetRequiredRuntimeSlot(4);
            scope.Driver.World.Register(catching);
            var attacker = new LF2Character();
            attacker.SetRequiredRuntimeSlot(5);
            scope.Driver.World.Register(attacker);
            var living = new LF2Character();
            living.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(living);

            living.Catching = catching;
            living.Attacker = attacker;
            living.Dead = true;
            living.HitCounters.AddFall(10);
            living.HitCounters.AddBdefend(10);
            living.HitCounters.RecoverFall(-0.25f);
            living.HitCounters.RecoverBdefend(-0.5f);
            living.OnFrameTransit(int.MaxValue, false);

            BattleWorldLivingShellSnapshotBuffer destination =
                session.CreateLivingShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldLivingShellSnapshot(destination),
                Is.True);

            BattleLivingShellSnapshot state = destination.GetState(3);
            Assert.That(state.InvalidFrameTransitionCount, Is.EqualTo(1));
            Assert.That(state.CatchingHandle.IsValid, Is.True);
            Assert.That(state.CatchingHandle.Slot, Is.EqualTo(4));
            Assert.That(state.AttackerHandle.IsValid, Is.True);
            Assert.That(state.AttackerHandle.Slot, Is.EqualTo(5));
            Assert.That(state.Dead, Is.True);
            Assert.That(state.FallRecoveryAccum, Is.EqualTo(-0.25f));
            Assert.That(state.BdefendRecoveryAccum, Is.EqualTo(-0.5f));
            Assert.That(destination.LivingCount, Is.EqualTo(3));
            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldLivingShellSnapshotBuffer.CurrentSchemaVersion));
            Assert.That(destination.ProtocolSchemaVersion,
                Is.EqualTo(identity.SchemaVersion));
            Assert.That(destination.IdentityFingerprint,
                Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(destination.CapturedTick, Is.Zero);

            living.Catching = null;
            living.Attacker = null;
            living.Dead = false;
            living.HitCounters.RecoverFall(-0.5f);
            state = destination.GetState(3);
            Assert.That(state.CatchingHandle.Slot, Is.EqualTo(4));
            Assert.That(state.AttackerHandle.Slot, Is.EqualTo(5));
            Assert.That(state.Dead, Is.True);
            Assert.That(state.FallRecoveryAccum, Is.EqualTo(-0.25f));
        }

        [Test]
        public void UnregisteredLivingReferenceFailsWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var living = new LF2Character();
            living.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(living);
            BattleWorldLivingShellSnapshotBuffer destination =
                session.CreateLivingShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldLivingShellSnapshot(destination),
                Is.True);

            int publishedSchema = destination.SchemaVersion;
            int publishedCount = destination.LivingCount;
            living.Attacker = new LF2Character();

            Assert.That(
                session.TryCaptureWorldLivingShellSnapshot(destination),
                Is.False);
            Assert.That(destination.SchemaVersion, Is.EqualTo(publishedSchema));
            Assert.That(destination.LivingCount, Is.EqualTo(publishedCount));
        }

        [Test]
        public void CapacityMismatchFailsWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var wrong = new BattleWorldLivingShellSnapshotBuffer(
                scope.Driver.World.RuntimeSlotCapacity + 1);

            Assert.That(
                session.TryCaptureWorldLivingShellSnapshot(wrong),
                Is.False);
            Assert.That(wrong.SchemaVersion, Is.Zero);
            Assert.That(wrong.LivingCount, Is.Zero);
        }

        [Test]
        public void WarmLivingShellCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var living = new LF2Character();
            living.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(living);
            BattleWorldLivingShellSnapshotBuffer destination =
                session.CreateLivingShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldLivingShellSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldLivingShellSnapshot(destination))
                {
                    Assert.Fail($"Living shell capture failed at {index}.");
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
                host = new GameObject("BattleWorldLivingShellSnapshotTests")
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
