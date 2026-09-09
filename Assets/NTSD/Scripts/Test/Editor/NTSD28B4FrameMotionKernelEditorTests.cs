#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4FrameMotionKernelEditorTests
    {
        [Test]
        public void XAxisUsesFacingClampAndOverrideWithoutFacing()
        {
            AssertMotion(3, 0, 0, false, 0, 1, 0, 0, 3, 0, 0);
            AssertMotion(3, 0, 0, false, 0, 5, 0, 0, 5, 0, 0);
            AssertMotion(3, 0, 0, true, 0, -1, 0, 0, -3, 0, 0);
            AssertMotion(-3, 0, 0, false, 0, 1, 0, 0, -3, 0, 0);
            AssertMotion(-3, 0, 0, true, 0, -1, 0, 0, 3, 0, 0);
            AssertMotion(551, 0, 0, true, 0, 99, 0, 0, 1, 0, 0);
        }

        [Test]
        public void YAxisAddsOrdinaryAndOverridesAboveThreshold()
        {
            AssertMotion(0, 2, 0, false, 0, 0, 3, 0, 0, 5, 0);
            AssertMotion(0, -2, 0, false, 0, 0, 3, 0, 0, 1, 0);
            AssertMotion(0, 551, 0, false, 0, 0, 3, 0, 0, 1, 0);
        }

        [Test]
        public void ZAxisRequiresIntentUnlessOverrideEncodingIsUsed()
        {
            AssertMotion(0, 0, 4, false, 0, 0, 0, 7, 0, 0, 7);
            AssertMotion(0, 0, 4, false, -1, 0, 0, 7, 0, 0, -4);
            AssertMotion(0, 0, 4, false, 1, 0, 0, 7, 0, 0, 4);
            AssertMotion(0, 0, 551, false, 0, 0, 0, 7, 0, 0, 1);
        }

        [Test]
        public void ProductionBothDepthKeysUseNoneRegardlessOfCooldowns()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateCharacter(world, dvz: 4);
            character.Runtime.Vz = 7;
            character.Runtime.KeyUp = 1;
            character.Runtime.KeyDown = 1;
            character.Runtime.CdUp = 9;
            character.Runtime.CdDown = 1;

            world.NativeFrameMotionAll();

            Assert.That(character.Runtime.Vz, Is.EqualTo(7));
        }

        [Test]
        public void ProductionSingleDepthKeyUsesStrictIntent()
        {
            var upWorld = new SimulationWorld();
            LF2Character up = CreateCharacter(upWorld, dvz: 4);
            up.Runtime.Vz = 7;
            up.Runtime.KeyUp = 1;
            up.Runtime.KeyDown = 0;
            upWorld.NativeFrameMotionAll();
            Assert.That(up.Runtime.Vz, Is.EqualTo(-4));

            var downWorld = new SimulationWorld();
            LF2Character down = CreateCharacter(downWorld, dvz: 4);
            down.Runtime.Vz = 7;
            down.Runtime.KeyUp = 0;
            down.Runtime.KeyDown = 1;
            downWorld.NativeFrameMotionAll();
            Assert.That(down.Runtime.Vz, Is.EqualTo(4));
        }

        private static void AssertMotion(
            int dvx,
            int dvy,
            int dvz,
            bool facingLeft,
            int depthIntent,
            double vx,
            double vy,
            double vz,
            double expectedVx,
            double expectedVy,
            double expectedVz)
        {
            BattleNativeFrameMotionKernel.Apply(
                dvx,
                dvy,
                dvz,
                facingLeft,
                depthIntent,
                ref vx,
                ref vy,
                ref vz);
            Assert.That((vx, vy, vz),
                Is.EqualTo((expectedVx, expectedVy, expectedVz)));
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int dvz)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 100,
                next = 0,
                dvx = 0,
                dvy = 0,
                dvz = dvz,
                centerx = 39,
                centery = 79,
            };
            var data = new LF2CharacterData
            {
                name = "B4FrameMotion",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = 8300;
            character.FrameCache.Load(
                new LF2CharacterDataWrapper(character.ObjectId, data));
            character.Initialize(500, 500);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Frame.D = frame;
            character.Trans.SyncDirectFrameData(100, 0, 0);
            character.SetRequiredRuntimeSlot(50);
            world.Register(character);
            return character;
        }
    }
}
#endif
