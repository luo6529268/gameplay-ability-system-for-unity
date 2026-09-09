#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5NativeComboCarriersEditorTests
    {
        [Test]
        public void EntityRuntime_DefaultResetAndCanonicalCopyAreIndependent()
        {
            var source = new NTSDEntityRuntime();
            Assert.That(source.NativeComboHitCount1E0, Is.Zero);
            Assert.That(source.NativeComboHitLastTick1E4, Is.Zero);

            source.NativeComboHitCount1E0 = 7;
            source.NativeComboHitLastTick1E4 = 123UL;
            source.ComboDja = 3;
            source.ComboCountAtk = 99;
            var destination = new NTSDEntityRuntime();
            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            Assert.That(destination.NativeComboHitCount1E0, Is.EqualTo(7));
            Assert.That(destination.NativeComboHitLastTick1E4, Is.EqualTo(123UL));
            Assert.That(destination.ComboDja, Is.EqualTo(3));
            Assert.That(destination.ComboCountAtk, Is.EqualTo(99));

            destination.ResetInputState();
            Assert.That(destination.NativeComboHitCount1E0, Is.EqualTo(7));
            Assert.That(destination.NativeComboHitLastTick1E4, Is.EqualTo(123UL));
            destination.Reset();
            Assert.That(destination.NativeComboHitCount1E0, Is.Zero);
            Assert.That(destination.NativeComboHitLastTick1E4, Is.Zero);
        }

        [Test]
        public void WorldRuntime_DefaultResetRestoreAndCoreSnapshotAreExact()
        {
            var runtime = new BattleRuntimeState();
            AssertTuple(runtime.NativeCombo, false, 0, 1, 50, 0);

            runtime.NativeCombo.RestoreForSnapshot(true, 1, 2, 77, 1);
            var world = new SimulationWorld();
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 2, 77, 1);
            var snapshot = new BattleWorldCoreScalarSnapshot(
                world,
                StrictDelayedInputBufferEditorTests.CreateIdentity());
            runtime.Reset();
            world.Runtime.NativeCombo.Reset();

            AssertTuple(runtime.NativeCombo, false, 0, 1, 50, 0);
            Assert.That(snapshot.NativeCombo.RecordPresent, Is.True);
            Assert.That(snapshot.NativeCombo.Bound, Is.EqualTo(1));
            Assert.That(snapshot.NativeCombo.Facing, Is.EqualTo(2));
            Assert.That(snapshot.NativeCombo.Respond, Is.EqualTo(77));
            Assert.That(snapshot.NativeCombo.CaughtAct, Is.EqualTo(1));
        }

        [Test]
        public void EntitySnapshotAndEcsShadowPreserveNativeComboFields()
        {
            SimulationWorld world = World(out LF2Character entity);
            entity.Runtime.NativeComboHitCount1E0 = 4;
            entity.Runtime.NativeComboHitLastTick1E4 = 66UL;
            world.CaptureBattleEcsShadowForDiagnostics(9);
            Assert.That(
                world.TryGetBattleEcsShadowEntityForDiagnostics(
                    3,
                    out BattleEcsShadowEntityView view),
                Is.True);
            Assert.That(view.NativeComboHitCount1E0, Is.EqualTo(4));
            Assert.That(view.NativeComboHitLastTick1E4, Is.EqualTo(66UL));

            var snapshot = new BattleWorldEntityRuntimeSnapshotBuffer(
                world.MaxRuntimeSlotsForServices);
            Assert.That(snapshot.TryCapture(
                world.RuntimeSlotTableForModules,
                StrictDelayedInputBufferEditorTests.CreateIdentity(),
                9), Is.True);
            entity.Runtime.NativeComboHitCount1E0 = 0;
            entity.Runtime.NativeComboHitLastTick1E4 = 0;
            Assert.That(snapshot.TryCopyEntityRuntime(3, entity.Runtime), Is.True);
            Assert.That(entity.Runtime.NativeComboHitCount1E0, Is.EqualTo(4));
            Assert.That(entity.Runtime.NativeComboHitLastTick1E4, Is.EqualTo(66UL));
        }

        [Test]
        public void FullSnapshotRestoreRestoresEntityAndWorldComboDomains()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(entity);
            entity.Runtime.NativeComboHitCount1E0 = 6;
            entity.Runtime.NativeComboHitLastTick1E4 = 88UL;
            scope.Driver.World.Runtime.NativeCombo.RestoreForSnapshot(
                true, 1, 0, 31, 1);
            BattleStateSnapshotBuffer snapshot =
                session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);

            entity.Runtime.NativeComboHitCount1E0 = 0;
            entity.Runtime.NativeComboHitLastTick1E4 = 0;
            scope.Driver.World.Runtime.NativeCombo.Reset();
            Assert.That(scope.Driver.TryRestoreBattleStateSnapshot(
                identity,
                snapshot,
                out BattleStateSnapshotRestoreFailure failure),
                Is.True,
                failure.ToString());

            Assert.That(entity.Runtime.NativeComboHitCount1E0, Is.EqualTo(6));
            Assert.That(entity.Runtime.NativeComboHitLastTick1E4, Is.EqualTo(88UL));
            AssertTuple(scope.Driver.World.Runtime.NativeCombo, true, 1, 0, 31, 1);
        }

        [Test]
        public void ChecksumParityAndSchemasTrackBothDomains()
        {
            SimulationWorld world = World(out LF2Character entity);
            ulong baseline = world.CaptureRuntimeChecksum64(0, null);
            entity.Runtime.NativeComboHitCount1E0 = 2;
            entity.Runtime.NativeComboHitLastTick1E4 = 7UL;
            ulong entityChanged = world.CaptureRuntimeChecksum64(0, null);
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 1, 50, 1);
            ulong worldChanged = world.CaptureRuntimeChecksum64(0, null);
            string parity = world.CaptureParityFrameSnapshot(0).ToJson(full: true);

            Assert.That(entityChanged, Is.Not.EqualTo(baseline));
            Assert.That(worldChanged, Is.Not.EqualTo(entityChanged));
            Assert.That(parity, Does.Contain("\"nativeComboHitCount1E0\":2"));
            Assert.That(parity, Does.Contain("\"nativeComboHitLastTick1E4\":7"));
            Assert.That(parity, Does.Contain("\"recordPresent\":true"));
            Assert.That(parity, Does.Contain("\"respond\":50"));
            Assert.That(BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(12));
            Assert.That(BattleWorldCoreScalarSnapshot.CurrentSchemaVersion,
                Is.EqualTo(11));
            Assert.That(BattleStateSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(20));
            Assert.That(BattleLockstepChecksumModule.CurrentSchemaVersion,
                Is.EqualTo(23));
        }

        [Test]
        public void WarmCanonicalCopyAndChecksumAllocateZero()
        {
            var source = new NTSDEntityRuntime
            {
                NativeComboHitCount1E0 = 3,
                NativeComboHitLastTick1E4 = 91UL,
            };
            var destination = new NTSDEntityRuntime();
            SimulationWorld world = World(out LF2Character entity);
            entity.Runtime.NativeComboHitCount1E0 = 3;
            entity.Runtime.NativeComboHitLastTick1E4 = 91UL;
            world.Runtime.NativeCombo.RestoreForSnapshot(true, 1, 1, 50, 1);
            source.TryCopyCanonicalStateTo(destination);
            world.CaptureRuntimeChecksum64(0, null);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            ulong checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                source.TryCopyCanonicalStateTo(destination);
                checksum ^= world.CaptureRuntimeChecksum64(index, null);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(destination.NativeComboHitCount1E0, Is.EqualTo(3));
            Assert.That(destination.NativeComboHitLastTick1E4, Is.EqualTo(91UL));
            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static void AssertTuple(
            NTSD28NativeComboRuntimeState state,
            bool recordPresent,
            int bound,
            int facing,
            int respond,
            int caughtAct)
        {
            Assert.That(state, Is.Not.Null);
            Assert.That(state.RecordPresent, Is.EqualTo(recordPresent));
            Assert.That(state.Bound, Is.EqualTo(bound));
            Assert.That(state.Facing, Is.EqualTo(facing));
            Assert.That(state.Respond, Is.EqualTo(respond));
            Assert.That(state.CaughtAct, Is.EqualTo(caughtAct));
        }

        private static SimulationWorld World(out LF2Character entity)
        {
            var world = new SimulationWorld();
            entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            return world;
        }

        private sealed class DriverScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver previous;
            private readonly GameObject host;

            public DriverScope()
            {
                const BindingFlags flags = BindingFlags.Static |
                                           BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType.GetField(
                    "<Instance>k__BackingField",
                    flags);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as SimulationTickDriver;
                instanceField.SetValue(null, null);
                host = new GameObject("NTSD28B5NativeComboCarriersTests")
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
