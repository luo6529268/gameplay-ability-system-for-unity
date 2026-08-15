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
    public sealed class BattleWorldWeaponShellSnapshotEditorTests
    {
        [Test]
        public void CaptureOwnsWeaponAccumulatorsAndPoolIdentity()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var weapon = new LF2Weapon();
            weapon.SetWeaponType(6);
            weapon.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(weapon);

            SetPrivateField(weapon, "_lateBreakEffectsHandled", true);
            SetPrivateField(weapon, "<InvalidInitTaskTypeCountForDiagnostics>k__BackingField", 11L);
            SetPrivateField(weapon, "_gravityToAdd", 12.25d);
            SetPrivateField(weapon, "_lastLandingVyBeforeClamp", -13.5d);

            BattleWorldWeaponShellSnapshotBuffer destination =
                session.CreateWeaponShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldWeaponShellSnapshot(destination),
                Is.True);

            BattleWeaponShellSnapshot state = destination.GetState(3);
            Assert.That(state.LateBreakEffectsHandled, Is.True);
            Assert.That(state.InvalidInitTaskTypeCount, Is.EqualTo(11));
            Assert.That(state.GravityToAdd, Is.EqualTo(12.25d));
            Assert.That(state.LastLandingVyBeforeClamp, Is.EqualTo(-13.5d));
            Assert.That(state.HasPoolWeaponType, Is.True);
            Assert.That(state.PoolWeaponType, Is.EqualTo(6));
            Assert.That(destination.WeaponCount, Is.EqualTo(1));
            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldWeaponShellSnapshotBuffer.CurrentSchemaVersion));
            Assert.That(destination.ProtocolSchemaVersion,
                Is.EqualTo(identity.SchemaVersion));
            Assert.That(destination.IdentityFingerprint,
                Is.EqualTo(identity.IdentityFingerprint));

            weapon.SetWeaponType(2);
            SetPrivateField(weapon, "_gravityToAdd", 99d);
            state = destination.GetState(3);
            Assert.That(state.PoolWeaponType, Is.EqualTo(6));
            Assert.That(state.GravityToAdd, Is.EqualTo(12.25d));
        }

        [Test]
        public void InvalidWeaponModulesFailWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var weapon = new BrokenWeapon();
            weapon.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(weapon);
            BattleWorldWeaponShellSnapshotBuffer destination =
                session.CreateWeaponShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldWeaponShellSnapshot(destination),
                Is.True);

            int publishedSchema = destination.SchemaVersion;
            int publishedCount = destination.WeaponCount;
            weapon.BreakHealthBinding();

            Assert.That(
                session.TryCaptureWorldWeaponShellSnapshot(destination),
                Is.False);
            Assert.That(destination.SchemaVersion, Is.EqualTo(publishedSchema));
            Assert.That(destination.WeaponCount, Is.EqualTo(publishedCount));
        }

        [Test]
        public void CapacityMismatchFailsWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var wrong = new BattleWorldWeaponShellSnapshotBuffer(
                scope.Driver.World.RuntimeSlotCapacity + 1);

            Assert.That(session.TryCaptureWorldWeaponShellSnapshot(wrong), Is.False);
            Assert.That(wrong.SchemaVersion, Is.Zero);
            Assert.That(wrong.WeaponCount, Is.Zero);
        }

        [Test]
        public void WarmWeaponShellCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var weapon = new LF2Weapon();
            weapon.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(weapon);
            BattleWorldWeaponShellSnapshotBuffer destination =
                session.CreateWeaponShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldWeaponShellSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldWeaponShellSnapshot(destination))
                {
                    Assert.Fail($"Weapon shell capture failed at {index}.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static void SetPrivateField(object owner, string fieldName, object value)
        {
            Type type = owner.GetType();
            FieldInfo field = null;
            while (type != null && field == null)
            {
                field = type.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            Assert.That(field, Is.Not.Null);
            field.SetValue(owner, value);
        }

        private sealed class BrokenWeapon : LF2Weapon
        {
            public void BreakHealthBinding()
            {
                Health = null;
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
                host = new GameObject("BattleWorldWeaponShellSnapshotTests")
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
