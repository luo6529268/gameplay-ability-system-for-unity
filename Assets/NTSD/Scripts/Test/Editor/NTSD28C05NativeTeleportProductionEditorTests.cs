#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C05NativeTeleportProductionEditorTests
    {
        [Test]
        public void NativeTeleport_RunsWhenLegacyFrameToggleWouldGate()
        {
            var world = new SimulationWorld();
            LF2OtherObject source = CreateOther(world, 50, 990, 400, relationTeam: 1);
            LF2Character target = CreateCharacter(world, 1, 991, relationTeam: 2);
            SetPosition(source, 100, -100, 100);
            SetPosition(target, 300, -200, 130);
            target.PS.groundY = -31;
            world.Runtime.Flow.FrameToggle = 1;

            world.NativeTeleportAll();

            Assert.That(source.Runtime.XInt, Is.EqualTo(180));
            Assert.That(source.Runtime.YInt, Is.EqualTo(-31));
            Assert.That(source.Runtime.ZInt, Is.EqualTo(131));
            AssertZeroMotion(source);
        }

        [Test]
        public void NativeTeleport_State401ExcludesSelfAndSynchronizesNoTargetPreciseCoordinates()
        {
            var world = new SimulationWorld();
            LF2Character source = CreateCharacter(world, 0, 992, relationTeam: 3, state: 401);
            source.Runtime.X = 100.75;
            source.Runtime.XInt = 100;
            source.Runtime.Y = -500.25;
            source.Runtime.YInt = -500;
            source.Runtime.Z = 200.75;
            source.Runtime.ZInt = 200;
            source.PS.groundY = -17;
            source.Runtime.SetVelocity(4.0, -2.0, 3.0);
            source.Runtime.SetSourceRulePosition(75.75, 180.75);
            source.Runtime.SyncSourceRuleIntegerPosition();

            world.NativeTeleportAll();

            Assert.That(source.Runtime.X, Is.EqualTo(100.0));
            Assert.That(source.Runtime.XInt, Is.EqualTo(100));
            Assert.That(source.Runtime.Y, Is.EqualTo(-17.0));
            Assert.That(source.Runtime.YInt, Is.EqualTo(-17));
            Assert.That(source.Runtime.Z, Is.EqualTo(200.0));
            Assert.That(source.Runtime.ZInt, Is.EqualTo(200));
            Assert.That(source.Runtime.SourceRuleX, Is.EqualTo(75.0));
            Assert.That(source.Runtime.SourceRuleXInt, Is.EqualTo(75));
            Assert.That(source.Runtime.SourceRuleZ, Is.EqualTo(180.0));
            Assert.That(source.Runtime.SourceRuleZInt, Is.EqualTo(180));
            AssertZeroMotion(source);
        }

        [Test]
        public void NativeTeleport_State400SelectsStrictNearestEnemyAndCopiesCollisionY()
        {
            var world = new SimulationWorld();
            LF2OtherObject source = CreateOther(world, 50, 993, 400, relationTeam: 1);
            LF2Character farther = CreateCharacter(world, 1, 994, relationTeam: 2);
            LF2Character nearest = CreateCharacter(world, 2, 995, relationTeam: 2);
            LF2Character teammate = CreateCharacter(world, 3, 996, relationTeam: 1);
            SetPosition(source, 100, -100, 100);
            SetPosition(farther, 500, 0, 200);
            SetPosition(nearest, 300, 0, 130);
            SetPosition(teammate, 110, 0, 101);
            nearest.PS.groundY = -23;

            world.NativeTeleportAll();

            Assert.That(source.Runtime.XInt, Is.EqualTo(180));
            Assert.That(source.Runtime.YInt, Is.EqualTo(-23));
            Assert.That(source.Runtime.ZInt, Is.EqualTo(131));
        }

        [Test]
        public void NativeTeleport_State401SelectsStrictFarthestLivingTeammate()
        {
            var world = new SimulationWorld();
            LF2OtherObject source = CreateOther(world, 50, 997, 401, relationTeam: 4);
            LF2Character near = CreateCharacter(world, 1, 998, relationTeam: 4);
            LF2Character far = CreateCharacter(world, 3, 999, relationTeam: 4);
            SetPosition(source, 100, -100, 100);
            SetPosition(near, 200, 0, 120);
            SetPosition(far, 500, 0, 500);
            far.PS.groundY = -29;
            source.Runtime.Dir = "left";

            world.NativeTeleportAll();

            Assert.That(source.Runtime.XInt, Is.EqualTo(560));
            Assert.That(source.Runtime.YInt, Is.EqualTo(-29));
            Assert.That(source.Runtime.ZInt, Is.EqualTo(501));
        }

        [TestCase(false, false, 400)]
        [TestCase(false, true, 400)]
        [TestCase(true, false, 400)]
        [TestCase(true, true, 400)]
        [TestCase(false, false, 401)]
        [TestCase(false, true, 401)]
        [TestCase(true, false, 401)]
        [TestCase(true, true, 401)]
        public void TeleportTargetRelativeHorizontalOffsetUsesViewRatio(
            bool configuredView,
            bool faceLeft,
            int state)
        {
            var world = new SimulationWorld();
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            LF2OtherObject source = CreateOther(world, 50, 1001, state, relationTeam: 1);
            LF2Character target = CreateCharacter(world, 1, 1002,
                relationTeam: state == 400 ? 2 : 1);
            SetPosition(source, 100, -10, 100);
            SetPosition(target, 300, -20, 130);
            source.Runtime.SetSourceRulePosition(100, 100);
            source.Runtime.SyncSourceRuleIntegerPosition();
            target.Runtime.SetSourceRulePosition(251, 90);
            target.Runtime.SyncSourceRuleIntegerPosition();
            target.PS.groundY = -20;
            source.SwitchDir(faceLeft ? "left" : "right");

            double offset = (state == 400 ? 120.0 : 60.0) *
                (configuredView ? 2048.0 / 1333.0 : 1.0);
            double expectedX = 300 + (faceLeft ? offset : -offset);
            int expectedXInt = (int)System.Math.Round(expectedX,
                System.MidpointRounding.ToEven);
            int rawOffset = state == 400 ? 120 : 60;
            int expectedSourceX = 251 + (faceLeft ? rawOffset : -rawOffset);

            world.NativeTeleportAll();
            Assert.That(source.Runtime.X, Is.EqualTo(expectedX).Within(0.000001));
            Assert.That(source.Runtime.XInt, Is.EqualTo(expectedXInt));
            Assert.That(source.Runtime.ZInt, Is.EqualTo(131));
            Assert.That(source.Runtime.YInt, Is.EqualTo(-20));
            Assert.That(source.Runtime.SourceRuleX, Is.EqualTo(expectedSourceX));
            Assert.That(source.Runtime.SourceRuleXInt, Is.EqualTo(expectedSourceX));
            Assert.That(source.Runtime.SourceRuleZ, Is.EqualTo(91));
            Assert.That(source.Runtime.SourceRuleZInt, Is.EqualTo(91));

            SetPosition(source, 100, -10, 100);
            source.Runtime.SetSourceRulePosition(100, 100);
            source.Runtime.SyncSourceRuleIntegerPosition();
            source.RunEarlyTeleportSpecialsPhase(
                new List<LF2Entity> { source, target }, false);
            Assert.That(source.Runtime.X, Is.EqualTo(expectedX).Within(0.000001));
            Assert.That(source.Runtime.XInt, Is.EqualTo(expectedXInt));
            Assert.That(source.Runtime.ZInt, Is.EqualTo(131));
            Assert.That(source.Runtime.SourceRuleX, Is.EqualTo(expectedSourceX));
            Assert.That(source.Runtime.SourceRuleXInt, Is.EqualTo(expectedSourceX));
            Assert.That(source.Runtime.SourceRuleZ, Is.EqualTo(91));
            Assert.That(source.Runtime.SourceRuleZInt, Is.EqualTo(91));
        }

        [Test]
        public void NativeTeleport_DoesNotInvokeLegacyStateExtraDispatch()
        {
            var world = new SimulationWorld();
            var probe = new LegacyDispatchProbe();
            Bind(probe, 50, 1000, 0, (int)LF2ObjectType.Other);
            world.Register(probe);

            world.NativeTeleportAll();
            Assert.That(probe.LegacyCallCount, Is.Zero);

            world.EarlyFrameAdvanceSpecialsAll(1);
            Assert.That(probe.LegacyCallCount, Is.EqualTo(1));
        }

        private static LF2OtherObject CreateOther(
            SimulationWorld world,
            int slot,
            int objectId,
            int state,
            int relationTeam)
        {
            var entity = new LF2OtherObject();
            Bind(entity, slot, objectId, state, (int)LF2ObjectType.Other);
            entity.RelationTeam = relationTeam;
            world.Register(entity);
            return entity;
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int slot,
            int objectId,
            int relationTeam,
            int state = 0)
        {
            var entity = new LF2Character();
            entity.ModuleInitialize();
            Bind(entity, slot, objectId, state, (int)LF2ObjectType.Character);
            entity.Initialize(500, 500);
            entity.RelationTeam = relationTeam;
            world.Register(entity);
            return entity;
        }

        private static void Bind(
            LF2Entity entity,
            int slot,
            int objectId,
            int state,
            int objectType)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = state,
                wait = 100,
                next = 0,
                itrs = new List<InteractionArea>(),
            };
            var data = new LF2CharacterData
            {
                name = "C05_" + objectId,
                type_sub = objectType,
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

        private static void SetPosition(
            LF2Entity entity,
            int x,
            int y,
            int z)
        {
            entity.Runtime.SetPosition(x, y, z);
            entity.Runtime.SyncIntegerPosition();
        }

        private static void AssertZeroMotion(LF2Entity entity)
        {
            Assert.That(entity.Runtime.Vx, Is.Zero);
            Assert.That(entity.Runtime.Vy, Is.Zero);
            Assert.That(entity.Runtime.Vz, Is.Zero);
        }

        private sealed class LegacyDispatchProbe : LF2OtherObject
        {
            internal int LegacyCallCount { get; private set; }

            internal override void RunEarlyTeleportSpecialsPhase(
                List<LF2Entity> entities,
                bool frameToggleGate)
            {
                LegacyCallCount++;
            }
        }
    }
}
#endif
