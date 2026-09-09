#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C04FrameMotionProductionOwnerEditorTests
    {
        [Test]
        public void ProductionC03_DoesNotWriteVelocityUntilExplicitC04()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateCharacter(world, 0, 980);
            SeedMotion(character);

            world.NativeProducerSampleAndInputRouteAll(1);

            Assert.That(character.Runtime.Vy, Is.EqualTo(-4.0));
            character.ImmediateFrame(0);
            character.Frame.D.dvx = 8;
            character.Frame.D.dvy = 2;
            character.Frame.D.dvz = 3;
            character.Runtime.KeyUp = 0;
            character.Runtime.CdUp = 0;
            character.Runtime.KeyDown = 1;
            character.Runtime.CdDown = 5;

            world.NativeFrameMotionAll();

            AssertMotion(character, 8.0, -2.0, 3.0);
        }

        [Test]
        public void DirectCharacterInputAll_PreservesCombinedCompatibility()
        {
            var directWorld = new SimulationWorld();
            LF2Character direct = CreateCharacter(directWorld, 0, 981);
            SeedMotion(direct);
            var productionWorld = new SimulationWorld();
            LF2Character production = CreateCharacter(productionWorld, 0, 985);
            SeedMotion(production);

            directWorld.CharacterInputAll(1);
            productionWorld.NativeProducerSampleAndInputRouteAll(1);
            productionWorld.NativeFrameMotionAll();

            AssertMotion(
                direct,
                production.Runtime.Vx,
                production.Runtime.Vy,
                production.Runtime.Vz);
        }

        [Test]
        public void ExplicitC04_IgnoresFrameDelayGate()
        {
            var world = new SimulationWorld();
            LF2OtherObject entity = CreateOther(world, 50, 982);
            entity.FrameDelay = 10;
            entity.Runtime.SetVelocity(1.0, -4.0, 9.0);
            entity.Runtime.KeyDown = 1;
            entity.Runtime.CdDown = 5;

            world.NativeFrameMotionAll();

            AssertMotion(entity, 8.0, -2.0, 3.0);
            Assert.That(entity.FrameDelay, Is.EqualTo(10));
        }

        [Test]
        public void ProductionSerialTick_DoesNotRepeatC04AdditiveVelocity()
        {
            var world = new SimulationWorld();
            var entity = new MotionProbeEntity();
            Bind(entity, 50, 983);
            world.Register(entity);
            entity.Runtime.Vy = 1.0;

            world.NativeFrameMotionAll();
            Assert.That(entity.Runtime.Vy, Is.EqualTo(3.0));

            world.SerialTickAll(1, nativeFrameMotionAlreadyApplied: true);

            Assert.That(entity.ObservedBefore, Is.EqualTo(3.0));
            Assert.That(entity.ObservedAfter, Is.EqualTo(3.0));
        }

        [Test]
        public void DirectSimTU_PreservesCombinedCompatibility()
        {
            var entity = new MotionProbeEntity();
            Bind(entity, 50, 984);
            entity.Runtime.Vy = 1.0;

            entity.SimTU(1);

            Assert.That(entity.ObservedBefore, Is.EqualTo(1.0));
            Assert.That(entity.ObservedAfter, Is.EqualTo(3.0));
        }

        private static void SeedMotion(LF2Character character)
        {
            character.Runtime.SetVelocity(1.0, -4.0, 9.0);
            character.Runtime.KeyDown = 1;
            character.Runtime.CdDown = 5;
        }

        private static void AssertMotion(
            LF2Entity entity,
            double x,
            double y,
            double z)
        {
            Assert.That(entity.Runtime.Vx, Is.EqualTo(x));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(y));
            Assert.That(entity.Runtime.Vz, Is.EqualTo(z));
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            Bind(character, slot, objectId);
            world.Register(character);
            return character;
        }

        private static LF2OtherObject CreateOther(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var entity = new LF2OtherObject();
            Bind(entity, slot, objectId);
            world.Register(entity);
            return entity;
        }

        private static void Bind(LF2Entity entity, int slot, int objectId)
        {
            LF2FrameData frame = Frame();
            var data = new LF2CharacterData
            {
                name = "C04_" + objectId,
                type_sub = entity is LF2Character
                    ? (int)LF2ObjectType.Character
                    : (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData> { frame },
            };
            entity.Name = data.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.SetRequiredRuntimeSlot(slot);
        }

        private static LF2FrameData Frame()
        {
            return new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                dvx = 8,
                dvy = 2,
                dvz = 3,
                itrs = new List<InteractionArea>(),
            };
        }

        private sealed class MotionProbeEntity : LF2OtherObject
        {
            internal double ObservedBefore { get; private set; }
            internal double ObservedAfter { get; private set; }

            public override void SimTU(int tickIndex)
            {
                ObservedBefore = Runtime.Vy;
                ApplyNonCharacterFrameVelocityForFrameAdvance();
                ObservedAfter = Runtime.Vy;
            }
        }
    }
}
#endif
