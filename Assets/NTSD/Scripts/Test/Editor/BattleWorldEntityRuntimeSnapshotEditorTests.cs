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
                if (expectedValue is Array expectedArray &&
                    actualValue is Array actualArray)
                {
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
