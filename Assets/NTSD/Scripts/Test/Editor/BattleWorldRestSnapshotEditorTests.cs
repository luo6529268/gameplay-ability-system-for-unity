#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleWorldRestSnapshotEditorTests
    {
        [Test]
        public void DenseCaptureOwnsDirectionalRestValues()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            RuntimeRestStore store = scope.Driver.World.RuntimeRestStoreForServices;
            BattleWorldRestSnapshotBuffer destination =
                session.CreateRestSnapshotBufferForBootstrap();
            Assert.That(store.SetARest(3, 7), Is.True);
            Assert.That(store.SetVRest(5, 3, 11), Is.True);
            Assert.That(store.SetVRest(5, 8, 13), Is.True);

            Assert.That(session.TryCaptureWorldRestSnapshot(destination), Is.True);
            store.SetARest(3, 0);
            store.SetVRest(5, 3, 0);

            Assert.That(destination.StorageMode,
                Is.EqualTo(BattleRestSnapshotStorageMode.Dense));
            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldRestSnapshotBuffer.CurrentSchemaVersion));
            Assert.That(destination.ProtocolSchemaVersion, Is.EqualTo(identity.SchemaVersion));
            Assert.That(destination.IdentityFingerprint, Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(destination.CapturedTick, Is.Zero);
            Assert.That(destination.ARestEntryCount, Is.EqualTo(1));
            Assert.That(destination.VRestEntryCount, Is.EqualTo(2));
            Assert.That(destination.VRestRowCount, Is.EqualTo(1));
            Assert.That(destination.GetARest(3), Is.EqualTo(7));
            Assert.That(destination.GetVRest(5, 3), Is.EqualTo(11));
            Assert.That(destination.GetVRest(5, 8), Is.EqualTo(13));
        }

        [Test]
        public void LargeDesktopCaptureUsesPreparedSparseStorage()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended,
                2304);
            var host = new GameObject("BattleWorldRestSparseSnapshotTests")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            try
            {
                SimulationTickDriver driver = host.AddComponent<SimulationTickDriver>();
                SetDriverWorld(driver, world);
                LockstepSessionIdentity identity =
                    StrictDelayedInputBufferEditorTests.CreateIdentity();
                var session = new BattleLockstepSession(driver, identity, 0, 8, 8);
                BattleWorldRestSnapshotBuffer destination =
                    session.CreateRestSnapshotBufferForBootstrap();
                RuntimeRestStore store = world.RuntimeRestStoreForServices;
                Assert.That(store.SetARest(2200, 17), Is.True);
                Assert.That(store.SetVRest(2201, 2200, 19), Is.True);

                Assert.That(session.TryCaptureWorldRestSnapshot(destination), Is.True);
                Assert.That(destination.StorageMode,
                    Is.EqualTo(BattleRestSnapshotStorageMode.Sparse));
                Assert.That(destination.SparseEntryCapacity,
                    Is.EqualTo(store.PreparedSparseVRestEntryCapacity));
                Assert.That(destination.GetARest(2200), Is.EqualTo(17));
                Assert.That(destination.GetVRest(2201, 2200), Is.EqualTo(19));
                Assert.That(destination.VRestEntryCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void WarmDenseCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleWorldRestSnapshotBuffer destination =
                session.CreateRestSnapshotBufferForBootstrap();
            RuntimeRestStore store = scope.Driver.World.RuntimeRestStoreForServices;
            store.SetARest(3, 7);
            store.SetVRest(5, 3, 11);
            Assert.That(session.TryCaptureWorldRestSnapshot(destination), Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 256; index++)
            {
                if (!session.TryCaptureWorldRestSnapshot(destination))
                {
                    Assert.Fail($"Rest capture failed at {index}.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static void SetDriverWorld(
            SimulationTickDriver driver,
            SimulationWorld world)
        {
            FieldInfo worldField = typeof(SimulationTickDriver).GetField(
                "_world",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(worldField, Is.Not.Null);
            worldField.SetValue(driver, world);
            driver.SetPaused(true);
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
                host = new GameObject("BattleWorldRestSnapshotTests")
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
