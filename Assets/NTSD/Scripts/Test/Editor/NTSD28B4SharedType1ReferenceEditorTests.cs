#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4SharedType1ReferenceEditorTests
    {
        [Test]
        public void CorePreservesContactAndEffectiveFloorWithoutClamping()
        {
            var runtime = new NTSDEntityRuntime
            {
                CollisionYReference = -10,
            };
            runtime.SetPosition(0.0, -12.0, 0.0);
            runtime.SetVelocity(0.0, 3.0, 0.0);
            runtime.SyncIntegerPosition();

            BattleNonCharacterMechanicsStepResult result =
                CharacterMechanics.StepNonCharacterBattleLogic(runtime, 0.5);

            Assert.That(result.PreviousPreciseY, Is.EqualTo(-12.0));
            Assert.That(result.ContactY, Is.EqualTo(-9.0));
            Assert.That(result.EffectiveFloorY, Is.EqualTo(-10.0));
            Assert.That(result.Type1LandingPredicate, Is.True);
            Assert.That(result.Airborne, Is.False);
            Assert.That(runtime.Y, Is.EqualTo(-9.0));
        }

        [Test]
        public void WarmCoreStepDoesNotAllocateManagedMemory()
        {
            var runtime = new NTSDEntityRuntime
            {
                CollisionYReference = -10,
            };
            runtime.SetPosition(0.0, -20.0, 0.0);
            runtime.SetVelocity(1.0, 0.0, 1.0);
            runtime.SyncIntegerPosition();
            CharacterMechanics.StepNonCharacterBattleLogic(runtime, 0.5);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
                CharacterMechanics.StepNonCharacterBattleLogic(runtime, 0.5);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void NegativeReferencePenetrationSettlesType1AtReference()
        {
            ProbeOther entity = CreateType1Shell();
            entity.Runtime.CollisionYReference = -10;
            entity.Runtime.SetPosition(0.0, -12.0, 0.0);
            entity.Runtime.SetVelocity(8.0, 3.0, 0.0);
            entity.Runtime.WeaponFlightCounter = 100;
            entity.AttackingCounter = 4;
            entity.Runtime.SyncIntegerPosition();

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(70));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-10.0));
            Assert.That(entity.Runtime.Vy, Is.Zero);
            Assert.That(entity.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(entity.Runtime.WeaponFlightCounter, Is.EqualTo(97));
            Assert.That(entity.AttackingCounter, Is.Zero);
        }

        [Test]
        public void ExactNegativeReferenceContactIsNativeNoopWithoutGravity()
        {
            ProbeOther entity = CreateType1Shell();
            entity.Runtime.CollisionYReference = -10;
            entity.Runtime.SetPosition(0.0, -13.0, 0.0);
            entity.Runtime.SetVelocity(8.0, 3.0, 0.0);
            entity.Runtime.WeaponFlightCounter = 100;
            entity.AttackingCounter = 4;
            entity.Runtime.SyncIntegerPosition();

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.Zero);
            Assert.That(entity.Runtime.Y, Is.EqualTo(-10.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(3.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(8.0));
            Assert.That(entity.Runtime.WeaponFlightCounter, Is.EqualTo(100));
            Assert.That(entity.AttackingCounter, Is.EqualTo(4));
        }

        private static ProbeOther CreateType1Shell()
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.WeaponThrowing,
                wait = 100,
                next = 0,
            };
            var data = new LF2CharacterData
            {
                name = "B4SharedType1Reference",
                type_sub = (int)LF2ObjectType.LightWeapon,
                weapon_drop_hurt = 3,
                frames = new List<LF2FrameData>
                {
                    frame,
                    new LF2FrameData
                    {
                        frameId = 70,
                        state = LF2States.WeaponOnGround,
                        wait = 100,
                        next = 70,
                    },
                },
            };
            var entity = new ProbeOther { ObjectId = 8301 };
            entity.FrameCache.Load(
                new LF2CharacterDataWrapper(entity.ObjectId, data));
            entity.ImmediateFrame(0);
            entity.SwitchDir("right");
            return entity;
        }

        private sealed class ProbeOther : LF2OtherObject
        {
            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.LightWeapon;
            }
        }
    }
}
#endif
