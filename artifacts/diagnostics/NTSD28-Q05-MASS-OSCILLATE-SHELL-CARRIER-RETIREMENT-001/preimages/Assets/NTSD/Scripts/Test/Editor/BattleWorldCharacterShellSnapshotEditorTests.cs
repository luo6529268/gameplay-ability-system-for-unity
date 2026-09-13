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
    public sealed class BattleWorldCharacterShellSnapshotEditorTests
    {
        [Test]
        public void CaptureOwnsHeldHandleAndCharacterScalars()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var held = new LF2Weapon();
            held.SetRequiredRuntimeSlot(4);
            scope.Driver.World.Register(held);
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(character);

            character.HeldWeaponReferenceInternal = held;
            character.DeadBlinkCountInternal = 17;
            SetPrivateField(character, "_mass", 12.5f);
            SetPrivateField(character, "_initializedFromOpoint", true);
            SetPrivateField(character, "_preserveOpointActionZero", true);

            BattleWorldCharacterShellSnapshotBuffer destination =
                session.CreateCharacterShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldCharacterShellSnapshot(destination),
                Is.True);

            BattleCharacterShellSnapshot state = destination.GetState(3);
            Assert.That(state.HeldWeaponHandle.IsValid, Is.True);
            Assert.That(state.HeldWeaponHandle.Slot, Is.EqualTo(4));
            Assert.That(state.Mass, Is.EqualTo(12.5f));
            Assert.That(state.DeadBlinkCount, Is.EqualTo(17));
            Assert.That(state.InitializedFromOpoint, Is.True);
            Assert.That(state.PreserveOpointActionZero, Is.True);
            Assert.That(destination.CharacterCount, Is.EqualTo(1));
            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldCharacterShellSnapshotBuffer.CurrentSchemaVersion));
            Assert.That(destination.ProtocolSchemaVersion,
                Is.EqualTo(identity.SchemaVersion));
            Assert.That(destination.IdentityFingerprint,
                Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(destination.CapturedTick, Is.Zero);

            character.HeldWeaponReferenceInternal = null;
            character.DeadBlinkCountInternal = -1;
            SetPrivateField(character, "_mass", 99f);
            state = destination.GetState(3);
            Assert.That(state.HeldWeaponHandle.Slot, Is.EqualTo(4));
            Assert.That(state.Mass, Is.EqualTo(12.5f));
            Assert.That(state.DeadBlinkCount, Is.EqualTo(17));
        }

        [Test]
        public void UnregisteredHeldReferenceFailsWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(character);
            BattleWorldCharacterShellSnapshotBuffer destination =
                session.CreateCharacterShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldCharacterShellSnapshot(destination),
                Is.True);

            int publishedSchema = destination.SchemaVersion;
            int publishedCount = destination.CharacterCount;
            character.HeldWeaponReferenceInternal = new LF2Weapon();

            Assert.That(
                session.TryCaptureWorldCharacterShellSnapshot(destination),
                Is.False);
            Assert.That(destination.SchemaVersion, Is.EqualTo(publishedSchema));
            Assert.That(destination.CharacterCount, Is.EqualTo(publishedCount));
        }

        [Test]
        public void CapacityMismatchFailsWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var wrong = new BattleWorldCharacterShellSnapshotBuffer(
                scope.Driver.World.RuntimeSlotCapacity + 1);

            Assert.That(
                session.TryCaptureWorldCharacterShellSnapshot(wrong),
                Is.False);
            Assert.That(wrong.SchemaVersion, Is.Zero);
            Assert.That(wrong.CharacterCount, Is.Zero);
        }

        [Test]
        public void WarmCharacterShellCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(character);
            BattleWorldCharacterShellSnapshotBuffer destination =
                session.CreateCharacterShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldCharacterShellSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldCharacterShellSnapshot(destination))
                {
                    Assert.Fail($"Character shell capture failed at {index}.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static void SetPrivateField(
            LF2Character character,
            string fieldName,
            object value)
        {
            FieldInfo field = typeof(LF2Character).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(character, value);
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
                host = new GameObject("BattleWorldCharacterShellSnapshotTests")
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
