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
    public sealed class BattleWorldEntityRuntimeSnapshotEditorTests
    {
        [Test]
        public void SourceRuleCoordinateCarrierHasIndependentExplicitOperations()
        {
            Type type = typeof(NTSDEntityRuntime);
            foreach (string name in new[] { "SourceRuleX", "SourceRuleZ", "SourceRuleXInt", "SourceRuleZInt",
                "SourceRulePositionInitialized", "SourceRuleXBoundPositive", "SourceRuleXBoundNegative",
                "SourceRuleZBoundPositive", "SourceRuleZBoundNegative" })
            {
                Assert.That(type.GetField(name), Is.Not.Null, name);
            }
            Assert.That(type.GetMethod("SetSourceRulePosition"), Is.Not.Null);
            Assert.That(type.GetMethod("SyncSourceRuleIntegerPosition"), Is.Not.Null);
            Assert.That(type.GetMethod("ClearSourceRuleBounds"), Is.Not.Null);
        }

        [Test]
        public void CaptureCopiesEveryCanonicalRuntimeFieldAndKeepsRawSlotDistinct()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(
                scope.Driver,
                identity,
                0,
                8,
                8);
            BattleWorldEntityRuntimeSnapshotBuffer destination =
                session.CreateEntityRuntimeSnapshotBufferForBootstrap();

            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(entity);
            PopulateCanonicalFields(entity.Runtime, 1000, 3);

            NTSDEntityRuntime rawRuntime = scope.Driver.World
                .RuntimeSlotTableForModules.GetRawRuntime(300);
            PopulateCanonicalFields(rawRuntime, 5000, 300);

            Assert.That(
                session.TryCaptureWorldEntityRuntimeSnapshot(destination),
                Is.True);

            var entityCopy = new NTSDEntityRuntime();
            var rawCopy = new NTSDEntityRuntime();
            Assert.That(destination.HasEntityRuntime(3), Is.True);
            Assert.That(destination.HasEntityRuntime(300), Is.False);
            Assert.That(destination.HasRawRuntime(3), Is.True);
            Assert.That(destination.HasRawRuntime(300), Is.True);
            Assert.That(destination.TryCopyEntityRuntime(3, entityCopy), Is.True);
            Assert.That(destination.TryCopyRawRuntime(300, rawCopy), Is.True);
            AssertCanonicalFieldsEqual(entity.Runtime, entityCopy);
            AssertCanonicalFieldsEqual(rawRuntime, rawCopy);
            Assert.That(entityCopy.InputHistory, Is.Not.SameAs(entity.Runtime.InputHistory));
            Assert.That(rawCopy.InputHistory, Is.Not.SameAs(rawRuntime.InputHistory));
            Assert.That(destination.EntityRuntimeCount, Is.EqualTo(1));
            Assert.That(destination.RawRuntimeCount,
                Is.EqualTo(scope.Driver.World.RuntimeSlotCapacity));
            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion));
            Assert.That(destination.ProtocolSchemaVersion,
                Is.EqualTo(identity.SchemaVersion));
            Assert.That(destination.IdentityFingerprint,
                Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(destination.CapturedTick, Is.Zero);

            int capturedStableId = entityCopy.StableId;
            int capturedHistory = entityCopy.InputHistory[5];
            entity.Runtime.StableId++;
            entity.Runtime.InputHistory[5]++;
            Assert.That(destination.TryCopyEntityRuntime(3, entityCopy), Is.True);
            Assert.That(entityCopy.StableId, Is.EqualTo(capturedStableId));
            Assert.That(entityCopy.InputHistory[5], Is.EqualTo(capturedHistory));
        }

        [Test]
        public void SourceRuleSyncAndClearRemainIndependentAndResetRemovesHistory()
        {
            var runtime = new NTSDEntityRuntime();
            runtime.SetPosition(100.75, -30.0, 90.5);
            runtime.SyncIntegerPosition();
            runtime.SetSourceRulePosition(-12.75, 7.875);
            runtime.SyncSourceRuleIntegerPosition();
            Assert.That(runtime.SourceRulePositionInitialized, Is.True);
            Assert.That(runtime.SourceRuleXInt, Is.EqualTo(-12));
            Assert.That(runtime.SourceRuleZInt, Is.EqualTo(7));
            runtime.SetSourceRulePosition(-13.75, 8.875);
            runtime.SyncIntegerPosition();
            runtime.SyncSourceRuleIntegerPosition();
            Assert.That(runtime.SourceRuleXInt, Is.EqualTo(-13));
            Assert.That(runtime.SourceRuleZInt, Is.EqualTo(8));
            Assert.That(runtime.X, Is.EqualTo(100.75));
            Assert.That(runtime.Z, Is.EqualTo(90.5));
            Assert.That(runtime.XInt, Is.EqualTo(100));
            Assert.That(runtime.ZInt, Is.EqualTo(90));
            runtime.XBoundPositive = true;
            runtime.SourceRuleXBoundPositive = true;
            runtime.SourceRuleXBoundNegative = true;
            runtime.SourceRuleZBoundPositive = true;
            runtime.SourceRuleZBoundNegative = true;
            runtime.ClearSourceRuleBounds();
            Assert.That(runtime.XBoundPositive, Is.True);
            Assert.That(runtime.SourceRuleXBoundPositive || runtime.SourceRuleXBoundNegative ||
                runtime.SourceRuleZBoundPositive || runtime.SourceRuleZBoundNegative, Is.False);
            runtime.SourceRuleXBoundPositive = true;
            runtime.SourceRuleXBoundNegative = true;
            runtime.SourceRuleZBoundPositive = true;
            runtime.SourceRuleZBoundNegative = true;
            runtime.Reset();
            Assert.That(runtime.SourceRulePositionInitialized, Is.False);
            Assert.That(runtime.SourceRuleX, Is.Zero);
            Assert.That(runtime.SourceRuleZ, Is.Zero);
            Assert.That(runtime.SourceRuleXInt, Is.Zero);
            Assert.That(runtime.SourceRuleZInt, Is.Zero);
            Assert.That(runtime.SourceRuleXBoundPositive || runtime.SourceRuleXBoundNegative ||
                runtime.SourceRuleZBoundPositive || runtime.SourceRuleZBoundNegative, Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PreviousSourceCoordinateSnapshotSchemaIsRejected(bool component)
        {
            using var scope = new DriverScope();
            var session = new BattleLockstepSession(scope.Driver,
                StrictDelayedInputBufferEditorTests.CreateIdentity(), 0, 8, 8);
            BattleStateSnapshotBuffer snapshot = session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);
            Assert.That(snapshot.IsValid, Is.True);
            object target = component ? (object)snapshot.EntityRuntime : snapshot;
            PropertyInfo schema = target.GetType().GetProperty("SchemaVersion");
            schema.SetValue(target, (int)schema.GetValue(target) - 1);
            Assert.That(snapshot.IsValid, Is.False);
            Assert.That(session.TryRestoreAndReplay(snapshot), Is.False);
        }

        [Test]
        public void AggregateRestoreRecoversIndependentEntityAndOccupiedRawSourceRuleState()
        {
            using var scope = new DriverScope();
            scope.Driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.Manual,
                enableFrameChecksum = true,
            });
            var session = new BattleLockstepSession(scope.Driver,
                StrictDelayedInputBufferEditorTests.CreateIdentity(), 0, 8, 8);
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(entity);
            NTSDEntityRuntime raw = scope.Driver.World.RuntimeSlotTableForModules.GetRawRuntime(3);
            Assert.That(raw, Is.Not.SameAs(entity.Runtime));
            entity.Runtime.SetPosition(400.5, -20.0, 300.5);
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.SetSourceRulePosition(123.75, -34.5);
            entity.Runtime.SyncSourceRuleIntegerPosition();
            entity.Runtime.SourceRuleXBoundPositive = true;
            entity.Runtime.SourceRuleZBoundNegative = true;
            raw.SetSourceRulePosition(-77.25, 62.75);
            raw.SyncSourceRuleIntegerPosition();
            raw.SourceRuleXBoundNegative = true;
            raw.SourceRuleZBoundPositive = true;
            BattleStateSnapshotBuffer snapshot = session.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);
            var expectedEntity = new NTSDEntityRuntime();
            var expectedRaw = new NTSDEntityRuntime();
            Assert.That(snapshot.EntityRuntime.TryCopyEntityRuntime(3, expectedEntity), Is.True);
            Assert.That(snapshot.EntityRuntime.TryCopyRawRuntime(3, expectedRaw), Is.True);
            string[] fields = { "SourceRuleX", "SourceRuleZ", "SourceRuleXInt", "SourceRuleZInt",
                "SourceRulePositionInitialized", "SourceRuleXBoundPositive", "SourceRuleXBoundNegative",
                "SourceRuleZBoundPositive", "SourceRuleZBoundNegative" };
            foreach (NTSDEntityRuntime runtime in new[] { entity.Runtime, raw })
            {
                foreach (string name in fields)
                {
                    FieldInfo field = typeof(NTSDEntityRuntime).GetField(name);
                    object current = field.GetValue(runtime);
                    field.SetValue(runtime, current is bool flag ? (object)!flag :
                        current is double precise ? (object)(precise + 50.25) : (int)current + 50);
                }
            }
            Assert.That(session.TryRestoreAndReplay(snapshot), Is.True, session.LastReason.ToString());
            NTSDEntityRuntime restoredRaw = scope.Driver.World.RuntimeSlotTableForModules.GetRawRuntime(3);
            foreach (string name in fields)
            {
                FieldInfo field = typeof(NTSDEntityRuntime).GetField(name);
                Assert.That(field.GetValue(entity.Runtime), Is.EqualTo(field.GetValue(expectedEntity)), name);
                Assert.That(field.GetValue(restoredRaw), Is.EqualTo(field.GetValue(expectedRaw)), name);
            }
            Assert.That(entity.Runtime.X, Is.EqualTo(400.5));
            Assert.That(entity.Runtime.Z, Is.EqualTo(300.5));
        }

        [Test]
        public void InvalidCanonicalSourceFailsWithoutPublishingNewMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(entity);
            BattleWorldEntityRuntimeSnapshotBuffer destination =
                session.CreateEntityRuntimeSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldEntityRuntimeSnapshot(destination),
                Is.True);

            int publishedCount = destination.EntityRuntimeCount;
            int publishedSchema = destination.SchemaVersion;
            entity.Runtime.InputHistory = null;

            Assert.That(
                session.TryCaptureWorldEntityRuntimeSnapshot(destination),
                Is.False);
            Assert.That(destination.EntityRuntimeCount, Is.EqualTo(publishedCount));
            Assert.That(destination.SchemaVersion, Is.EqualTo(publishedSchema));
        }

        [Test]
        public void CapacityMismatchFailsWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var wrong = new BattleWorldEntityRuntimeSnapshotBuffer(
                scope.Driver.World.RuntimeSlotCapacity + 1);

            Assert.That(
                session.TryCaptureWorldEntityRuntimeSnapshot(wrong),
                Is.False);
            Assert.That(wrong.SchemaVersion, Is.Zero);
            Assert.That(wrong.EntityRuntimeCount, Is.Zero);
            Assert.That(wrong.RawRuntimeCount, Is.Zero);
        }

        [Test]
        public void WarmEntityAndRawRuntimeCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(entity);
            BattleWorldEntityRuntimeSnapshotBuffer destination =
                session.CreateEntityRuntimeSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldEntityRuntimeSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldEntityRuntimeSnapshot(destination))
                {
                    Assert.Fail($"Entity runtime capture failed at {index}.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static void PopulateCanonicalFields(
            NTSDEntityRuntime runtime,
            int seed,
            int requiredSlot)
        {
            int next = seed;
            FieldInfo[] fields = typeof(NTSDEntityRuntime).GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);
            for (int index = 0; index < fields.Length; index++)
            {
                FieldInfo field = fields[index];
                if (field.IsNotSerialized)
                {
                    continue;
                }

                if (field.Name == nameof(NTSDEntityRuntime.InputHistory))
                {
                    field.SetValue(runtime, new[]
                    {
                        next++, next++, next++, next++, next++, next++,
                    });
                    continue;
                }

                if (field.Name == nameof(NTSDEntityRuntime.InputRemapIndices13C))
                {
                    var remap = new byte[NTSDEntityRuntime.NativeInputRemapCount];
                    for (int remapIndex = 0;
                         remapIndex < remap.Length;
                         remapIndex++)
                    {
                        remap[remapIndex] =
                            (byte)(next++ % byte.MaxValue);
                    }
                    field.SetValue(runtime, remap);
                    continue;
                }

                if (field.FieldType == typeof(NTSD28InputProxyBlock))
                {
                    NTSD28InputProxyBlock proxy =
                        (NTSD28InputProxyBlock)field.GetValue(runtime);
                    for (int key = 0; key < proxy.EdgeWindow.Length; key++)
                        proxy.EdgeWindow[key] = (byte)(next++ % byte.MaxValue);
                    proxy.DefendReentryCooldown =
                        (byte)(next++ % byte.MaxValue);
                    for (int key = 0; key < proxy.Previous.Length; key++)
                    {
                        proxy.Previous[key] = (byte)(next++ % byte.MaxValue);
                        proxy.Current[key] = (byte)(next++ % byte.MaxValue);
                    }
                    for (int combo = 0; combo < proxy.ComboState.Length; combo++)
                        proxy.ComboState[combo] = (byte)(next++ % byte.MaxValue);
                    proxy.ProxyTail = (byte)(next++ % byte.MaxValue);
                    continue;
                }

                if (field.FieldType == typeof(int))
                {
                    field.SetValue(runtime, next++);
                }
                else if (field.FieldType == typeof(long))
                {
                    field.SetValue(runtime, (long)next++ * 1000L);
                }
                else if (field.FieldType == typeof(byte))
                {
                    field.SetValue(runtime, (byte)(next++ % byte.MaxValue));
                }
                else if (field.FieldType == typeof(bool))
                {
                    field.SetValue(runtime, (next++ & 1) != 0);
                }
                else if (field.FieldType == typeof(float))
                {
                    field.SetValue(runtime, next++ + 0.25f);
                }
                else if (field.FieldType == typeof(double))
                {
                    field.SetValue(runtime, next++ + 0.125d);
                }
                else if (field.FieldType == typeof(ulong))
                {
                    field.SetValue(runtime, (ulong)next++);
                }
                else
                {
                    Assert.Fail($"Unclassified canonical runtime field: {field.Name}");
                }
            }

            runtime.SlotIndex = requiredSlot;
        }

        private static void AssertCanonicalFieldsEqual(
            NTSDEntityRuntime expected,
            NTSDEntityRuntime actual)
        {
            FieldInfo[] fields = typeof(NTSDEntityRuntime).GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);
            for (int index = 0; index < fields.Length; index++)
            {
                FieldInfo field = fields[index];
                if (field.IsNotSerialized)
                {
                    continue;
                }

                object expectedValue = field.GetValue(expected);
                object actualValue = field.GetValue(actual);
                if (expectedValue is NTSD28InputProxyBlock expectedProxy &&
                    actualValue is NTSD28InputProxyBlock actualProxy)
                {
                    Assert.That(actualProxy, Is.Not.SameAs(expectedProxy), field.Name);
                    Assert.That(actualProxy.EdgeWindow,
                        Is.EqualTo(expectedProxy.EdgeWindow), field.Name);
                    Assert.That(actualProxy.DefendReentryCooldown,
                        Is.EqualTo(expectedProxy.DefendReentryCooldown), field.Name);
                    Assert.That(actualProxy.Previous,
                        Is.EqualTo(expectedProxy.Previous), field.Name);
                    Assert.That(actualProxy.Current,
                        Is.EqualTo(expectedProxy.Current), field.Name);
                    Assert.That(actualProxy.ComboState,
                        Is.EqualTo(expectedProxy.ComboState), field.Name);
                    Assert.That(actualProxy.ProxyTail,
                        Is.EqualTo(expectedProxy.ProxyTail), field.Name);
                }
                else if (expectedValue is Array expectedArray &&
                    actualValue is Array actualArray)
                {
                    Assert.That(actualArray, Is.Not.SameAs(expectedArray), field.Name);
                    Assert.That(actualArray, Is.EqualTo(expectedArray), field.Name);
                }
                else
                {
                    Assert.That(actualValue, Is.EqualTo(expectedValue), field.Name);
                }
            }
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
                host = new GameObject("BattleWorldEntityRuntimeSnapshotTests")
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
