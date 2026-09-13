#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleWorldEntityBaseShellSnapshotEditorTests
    {
        [Test]
        public void CaptureOwnsFrameEffectPhysicsHitRecordsAndTrackerHandle()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);

            var trackedParent = new LF2OtherObject();
            trackedParent.SetRequiredRuntimeSlot(4);
            scope.Driver.World.Register(trackedParent);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(entity);

            entity.SetRequiredRuntimeSlot(123);
            entity.CurrentItrIndex = 6;
            entity.TrackerParent = trackedParent;
            entity.Frame.PN = 10;
            entity.Frame.Prev = 11;
            entity.Frame.N = 12;
            entity.Frame.D = new LF2FrameData { frameId = 13 };
            entity.Frame.Prev2 = 14;
            entity.Frame.Prev2D = new LF2FrameData { frameId = 15 };
            entity.Trans.SetWait(16, 17);
            entity.Trans.SetNext(18);
            entity.Effect.Num = 19;
            entity.Effect.Dvx = 20.25f;
            entity.Effect.Dvy = 21.25f;
            entity.Effect.Stuck = true;
            entity.Effect.Oscillate = 22;
            entity.Effect.Blink = true;
            entity.Effect.Super = true;
            entity.Effect.TimeIn = 23;
            entity.Effect.TimeOut = 24;
            entity.Effect.OscillateDirection = -1;
            entity.Effect.BlinkCounter = 25;
            entity.PS.groundY = 26.25f;
            entity.PS.dir = "left";
            entity.PS.fric = 27.25f;
            entity.PS.zz = 28.25f;
            entity.PS.zBoundPositive = true;
            entity.PS.zBoundNegative = false;
            entity.PS.xBoundPositive = true;
            entity.PS.xBoundNegative = false;
            entity.AddHitRecord(29, 30, 31);
            entity.AdvanceHitRecord(0, 32);

            BattleWorldEntityBaseShellSnapshotBuffer destination =
                session.CreateEntityBaseShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldEntityBaseShellSnapshot(destination),
                Is.True);

            BattleEntityBaseShellSnapshot state = destination.GetState(3);
            Assert.That(state.RequiredRuntimeSlot, Is.EqualTo(123));
            Assert.That(state.CurrentItrIndex, Is.EqualTo(6));
            Assert.That(state.TrackerParentHandle.IsValid, Is.True);
            Assert.That(state.TrackerParentHandle.Slot, Is.EqualTo(4));
            Assert.That(state.FramePreviousNumber, Is.EqualTo(10));
            Assert.That(state.FramePreviousTick, Is.EqualTo(11));
            Assert.That(state.FrameNumber, Is.EqualTo(12));
            Assert.That(state.FrameDataId, Is.EqualTo(13));
            Assert.That(state.CollisionPreviousFrame, Is.EqualTo(14));
            Assert.That(state.CollisionFrameDataId, Is.EqualTo(15));
            Assert.That(state.TransistorWait, Is.EqualTo(16));
            Assert.That(state.TransistorWaitCounter, Is.EqualTo(17));
            Assert.That(state.TransistorNext, Is.EqualTo(18));
            Assert.That(state.EffectNumber, Is.EqualTo(19));
            Assert.That(state.EffectDvx, Is.EqualTo(20.25f));
            Assert.That(state.EffectDvy, Is.EqualTo(21.25f));
            Assert.That(state.EffectStuck, Is.True);
            Assert.That(state.EffectOscillate, Is.EqualTo(22));
            Assert.That(state.EffectBlink, Is.True);
            Assert.That(state.EffectSuper, Is.True);
            Assert.That(state.EffectTimeIn, Is.EqualTo(23));
            Assert.That(state.EffectTimeOut, Is.EqualTo(24));
            Assert.That(state.EffectOscillateDirection, Is.EqualTo(-1));
            Assert.That(state.EffectBlinkCounter, Is.EqualTo(25));
            Assert.That(state.PhysicsGroundY, Is.EqualTo(26.25f));
            Assert.That(state.PhysicsFacingLeft, Is.True);
            Assert.That(state.PhysicsFriction, Is.EqualTo(27.25f));
            Assert.That(state.PhysicsDepthOffset, Is.EqualTo(28.25f));
            Assert.That(state.PhysicsZBoundPositive, Is.True);
            Assert.That(state.PhysicsZBoundNegative, Is.False);
            Assert.That(state.PhysicsXBoundPositive, Is.True);
            Assert.That(state.PhysicsXBoundNegative, Is.False);
            Assert.That(state.HitRecordCount, Is.EqualTo(1));
            Assert.That(destination.GetHitRecordDamage(3, 0), Is.EqualTo(30));
            Assert.That(destination.GetHitRecordX(3, 0), Is.EqualTo(30));
            Assert.That(destination.GetHitRecordZ(3, 0), Is.EqualTo(31));
            Assert.That(destination.GetHitRecordLastAdvanceTick(3, 0),
                Is.EqualTo(32));
            Assert.That(destination.EntityCount, Is.EqualTo(2));
            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldEntityBaseShellSnapshotBuffer.CurrentSchemaVersion));
            Assert.That(destination.ProtocolSchemaVersion, Is.EqualTo(identity.SchemaVersion));
            Assert.That(destination.IdentityFingerprint, Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(destination.CapturedTick, Is.Zero);

            entity.Frame.N = 99;
            entity.Effect.Num = 99;
            entity.PS.fric = 99f;
            entity.AdvanceHitRecord(0, 99);
            state = destination.GetState(3);
            Assert.That(state.FrameNumber, Is.EqualTo(12));
            Assert.That(state.EffectNumber, Is.EqualTo(19));
            Assert.That(state.PhysicsFriction, Is.EqualTo(27.25f));
            Assert.That(destination.GetHitRecordDamage(3, 0), Is.EqualTo(30));
        }

        [Test]
        public void UnregisteredTrackerReferenceFailsWithoutPublishingMetadata()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(entity);
            BattleWorldEntityBaseShellSnapshotBuffer destination =
                session.CreateEntityBaseShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldEntityBaseShellSnapshot(destination),
                Is.True);

            int publishedSchema = destination.SchemaVersion;
            int publishedCount = destination.EntityCount;
            entity.TrackerParent = new LF2OtherObject();

            Assert.That(
                session.TryCaptureWorldEntityBaseShellSnapshot(destination),
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
            var wrong = new BattleWorldEntityBaseShellSnapshotBuffer(
                scope.Driver.World.RuntimeSlotCapacity + 1);

            Assert.That(
                session.TryCaptureWorldEntityBaseShellSnapshot(wrong),
                Is.False);
            Assert.That(wrong.SchemaVersion, Is.Zero);
            Assert.That(wrong.EntityCount, Is.Zero);
        }

        [Test]
        public void WarmBaseShellCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            scope.Driver.World.Register(entity);
            BattleWorldEntityBaseShellSnapshotBuffer destination =
                session.CreateEntityBaseShellSnapshotBufferForBootstrap();
            Assert.That(
                session.TryCaptureWorldEntityBaseShellSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldEntityBaseShellSnapshot(destination))
                {
                    Assert.Fail($"Base shell capture failed at {index}.");
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
                host = new GameObject("BattleWorldEntityBaseShellSnapshotTests")
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
