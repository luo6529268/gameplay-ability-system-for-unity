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
    public sealed class BattleWorldSpecialOtherShellSnapshotEditorTests
    {
        [Test]
        public void CaptureOwnsSpecialParentStateAndOtherDiagnostics()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var parent = new LF2Character();
            parent.SetRequiredRuntimeSlot(4);
            scope.Driver.World.Register(parent);
            var special = new LF2SpecialAttack();
            special.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(special);
            var other = new LF2OtherObject();
            other.SetRequiredRuntimeSlot(5);
            scope.Driver.World.Register(other);

            SetPrivateField(special, "_parent", parent);
            SetPrivateField(special, "_lastState", 3012);
            SetPrivateField(
                special,
                "<InvalidInitTaskTypeCountForDiagnostics>k__BackingField",
                7L);
            special.NoBounce = true;
            object lifecycle = GetPrivateField(other, "lifecycleModule");
            SetPrivateField(
                lifecycle,
                "<InvalidTaskTypeCountForDiagnostics>k__BackingField",
                8L);

            BattleWorldSpecialOtherShellSnapshotBuffer destination =
                session.CreateSpecialOtherShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldSpecialOtherShellSnapshot(destination),
                Is.True);

            BattleSpecialOtherShellSnapshot specialState = destination.GetState(3);
            Assert.That(specialState.Kind,
                Is.EqualTo(BattleSpecialOtherShellKind.SpecialAttack));
            Assert.That(specialState.ParentHandle.Slot, Is.EqualTo(4));
            Assert.That(specialState.LastState, Is.EqualTo(3012));
            Assert.That(specialState.NoBounce, Is.True);
            Assert.That(specialState.InvalidInitTaskTypeCount, Is.EqualTo(7));
            BattleSpecialOtherShellSnapshot otherState = destination.GetState(5);
            Assert.That(otherState.Kind,
                Is.EqualTo(BattleSpecialOtherShellKind.OtherObject));
            Assert.That(otherState.InvalidInitTaskTypeCount, Is.EqualTo(8));
            Assert.That(destination.EntityCount, Is.EqualTo(2));
            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldSpecialOtherShellSnapshotBuffer.CurrentSchemaVersion));

            SetPrivateField(special, "_lastState", 0);
            special.NoBounce = false;
            specialState = destination.GetState(3);
            Assert.That(specialState.LastState, Is.EqualTo(3012));
            Assert.That(specialState.NoBounce, Is.True);
        }

        [Test]
        public void UnregisteredParentFailsWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var special = new LF2SpecialAttack();
            special.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(special);
            BattleWorldSpecialOtherShellSnapshotBuffer destination =
                session.CreateSpecialOtherShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldSpecialOtherShellSnapshot(destination),
                Is.True);

            int publishedSchema = destination.SchemaVersion;
            int publishedCount = destination.EntityCount;
            SetPrivateField(special, "_parent", new LF2Character());

            Assert.That(
                session.TryCaptureWorldSpecialOtherShellSnapshot(destination),
                Is.False);
            Assert.That(destination.SchemaVersion, Is.EqualTo(publishedSchema));
            Assert.That(destination.EntityCount, Is.EqualTo(publishedCount));
        }

        [Test]
        public void CapacityMismatchFailsWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var wrong = new BattleWorldSpecialOtherShellSnapshotBuffer(
                scope.Driver.World.RuntimeSlotCapacity + 1);

            Assert.That(
                session.TryCaptureWorldSpecialOtherShellSnapshot(wrong),
                Is.False);
            Assert.That(wrong.SchemaVersion, Is.Zero);
            Assert.That(wrong.EntityCount, Is.Zero);
        }

        [Test]
        public void WarmSpecialOtherShellCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var special = new LF2SpecialAttack();
            special.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(special);
            BattleWorldSpecialOtherShellSnapshotBuffer destination =
                session.CreateSpecialOtherShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldSpecialOtherShellSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldSpecialOtherShellSnapshot(destination))
                {
                    Assert.Fail($"Special/other shell capture failed at {index}.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static object GetPrivateField(object owner, string fieldName)
        {
            FieldInfo field = FindPrivateField(owner.GetType(), fieldName);
            Assert.That(field, Is.Not.Null);
            return field.GetValue(owner);
        }

        private static void SetPrivateField(object owner, string fieldName, object value)
        {
            FieldInfo field = FindPrivateField(owner.GetType(), fieldName);
            Assert.That(field, Is.Not.Null);
            field.SetValue(owner, value);
        }

        private static FieldInfo FindPrivateField(Type type, string fieldName)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    return field;
                }
                type = type.BaseType;
            }

            return null;
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
                host = new GameObject("BattleWorldSpecialOtherShellSnapshotTests")
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
