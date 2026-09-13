#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4State1218ContactActionEditorTests
    {
        [Test]
        public void MechanicsResult_ReportsAlreadyGroundedEffectiveFloorContact()
        {
            var runtime = new NTSDEntityRuntime
            {
                CollisionYReference = -10,
            };
            runtime.SetPosition(0.0, -10.0, 0.0);
            runtime.SetVelocity(0.0, 0.0, 0.0);
            runtime.SyncIntegerPosition();
            var context = new CharacterMechanicsContext(
                runtime,
                Frame(170, LF2States.Falling),
                0f,
                0f,
                1.0);

            BattleMechanicsStepResult result =
                new CharacterMechanics().StepBattleLogic(context);

            Assert.That(result.EffectiveFloorContact, Is.True);
            Assert.That(result.Landed, Is.False);
            Assert.That(result.Airborne, Is.False);
        }

        [Test]
        public void ExactAlreadyGroundedState12_UsesSoft230AndDefersStatus()
        {
            LF2Character entity = CreateExact(170, LF2States.Falling);
            Prepare(entity, -10, -10.0, 6.0, 0.0);
            SetPendingStatus(entity.Runtime);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(230));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-10.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(5.0 / 3.0));
            Assert.That(entity.Runtime.Vy, Is.Zero);
            Assert.That(entity.AttackingCounter, Is.Zero);
            AssertPendingStatus(entity.Runtime);
        }

        [Test]
        public void SharedCrossingState12_CurrentAction186OrAboveUsesSoft231()
        {
            ProbeOther entity = CreateShared(187, LF2States.Falling);
            Prepare(entity, 0, -1.0, 6.0, 1.0);
            SetPendingStatus(entity.Runtime);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(231));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(2.0));
            Assert.That(entity.Runtime.Vy, Is.Zero);
            Assert.That(entity.AttackingCounter, Is.Zero);
            AssertPendingStatus(entity.Runtime);
        }

        [TestCase(9.0001, 1.0)]
        [TestCase(-9.0001, 1.0)]
        [TestCase(0.0, 11.0001)]
        public void State12HardThresholds_AreStrictAndUse185WithoutGain(
            double vx,
            double vy)
        {
            LF2Character entity = CreateExact(170, LF2States.Falling);
            Prepare(entity, 0, -vy, vx, vy);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(185));
            Assert.That(entity.Runtime.Vx, Is.InRange(-7.0, 7.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(-3.5));
            Assert.That(entity.AttackingCounter, Is.EqualTo(9));
        }

        [Test]
        public void State12HardWithoutGain_CurrentAction186OrAboveUses191()
        {
            LF2Character entity = CreateExact(187, LF2States.Falling);
            Prepare(entity, 0, -12.0, 0.0, 12.0);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(191));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(-3.5));
            Assert.That(entity.AttackingCounter, Is.EqualTo(9));
        }

        [Test]
        public void State12HardWithGain_ConsumesMotionAndUsesPickedAction()
        {
            LF2Character entity = CreateExact(187, LF2States.Falling);
            Prepare(entity, 0, -12.0, 10.0, 12.0, 2.0);
            SetPendingStatus(entity.Runtime);
            entity.Runtime.StatusDx1C0 = 560;
            entity.Runtime.StatusDy1C4 = 501;
            entity.Runtime.StatusDz1C8 = 5;

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(300));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(10.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(-49.0));
            Assert.That(entity.Runtime.Vz, Is.EqualTo(7.0));
            Assert.That(entity.Runtime.StatusDx1C0, Is.Zero);
            Assert.That(entity.Runtime.StatusDy1C4, Is.Zero);
            Assert.That(entity.Runtime.StatusDz1C8, Is.Zero);
            Assert.That(entity.Runtime.StatusGain1CC, Is.Zero);
            Assert.That(entity.Runtime.StatusPickedAction1D4, Is.EqualTo(300));
            Assert.That(entity.Runtime.StatusPickingAction1D8, Is.EqualTo(301));
            Assert.That(entity.AttackingCounter, Is.EqualTo(9));
        }

        [Test]
        public void State18AlwaysHardAndUsesPickingActionWhenGainIsPending()
        {
            ProbeOther entity = CreateShared(200, LF2States.Burning);
            Prepare(entity, 0, -1.0, 3.0, 1.0);
            SetPendingStatus(entity.Runtime);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(301));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(-1.5));
            Assert.That(entity.Runtime.StatusGain1CC, Is.Zero);
            Assert.That(entity.AttackingCounter, Is.EqualTo(9));
        }

        private static void SetPendingStatus(NTSDEntityRuntime runtime)
        {
            runtime.StatusDx1C0 = 2;
            runtime.StatusDy1C4 = 2;
            runtime.StatusDz1C8 = 2;
            runtime.StatusGain1CC = 1;
            runtime.StatusHitFacing1D0 = 0;
            runtime.StatusPickedAction1D4 = 300;
            runtime.StatusPickingAction1D8 = 301;
        }

        private static void AssertPendingStatus(NTSDEntityRuntime runtime)
        {
            Assert.That(runtime.StatusDx1C0, Is.EqualTo(2));
            Assert.That(runtime.StatusDy1C4, Is.EqualTo(2));
            Assert.That(runtime.StatusDz1C8, Is.EqualTo(2));
            Assert.That(runtime.StatusGain1CC, Is.EqualTo(1));
        }

        private static LF2Character CreateExact(int action, int state)
        {
            var entity = new LF2Character { ObjectId = 8311 };
            Load(entity, action, state);
            return entity;
        }

        private static ProbeOther CreateShared(int action, int state)
        {
            var entity = new ProbeOther { ObjectId = 8312 };
            Load(entity, action, state);
            return entity;
        }

        private static void Load(LF2Entity entity, int action, int state)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(action, state),
            };
            int[] targets = { 185, 191, 230, 231, 300, 301 };
            for (int index = 0; index < targets.Length; index++)
            {
                if (targets[index] != action)
                    frames.Add(Frame(targets[index], 0));
            }
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                entity.ObjectId,
                new LF2CharacterData
                {
                    name = "B4State1218ContactAction",
                    frames = frames,
                }));
            entity.ImmediateFrame(action);
        }

        private static LF2FrameData Frame(int id, int state)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
            };
        }

        private static void Prepare(
            LF2Entity entity,
            int reference,
            double y,
            double vx,
            double vy,
            double vz = 0.0)
        {
            entity.Runtime.CollisionYReference = reference;
            entity.Runtime.SetPosition(0.0, y, 0.0);
            entity.Runtime.SetVelocity(vx, vy, vz);
            entity.Runtime.SyncIntegerPosition();
            entity.AttackingCounter = 9;
        }

        private sealed class ProbeOther : LF2OtherObject
        {
            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.Character;
            }
        }
    }
}
#endif
