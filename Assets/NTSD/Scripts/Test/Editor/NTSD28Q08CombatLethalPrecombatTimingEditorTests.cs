#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q08CombatLethalPrecombatTimingEditorTests
    {
        [Test]
        public void InTickLethalHitStartsNativeResultOnFollowingTick()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 0;
            world.Runtime.FunctionKeys.ResetForBattle(true);
            CreateCombatant(world, 0, 1, 7100, 0, true, 500);
            TestCombatant victim = CreateCombatant(
                world, 1, 2, 7101, 10, false, 20);
            var tickSystem = new NTSDBattleTickSystem(world);

            tickSystem.RunReleaseTick(1, buildPresentation: false);

            Assert.That(victim.Health.HP, Is.LessThanOrEqualTo(0),
                "The first full tick must actually consume the overlapping lethal itr.");
            Assert.That(world.Runtime.Results.NativeResultTimer, Is.Zero,
                "Logan classifies both living groups before this tick's lethal hit.");
            Assert.That(world.Runtime.Results.NativeLivingGroupMask,
                Is.EqualTo((1UL << 1) | (1UL << 2)));

            tickSystem.RunReleaseTick(2, buildPresentation: false);

            Assert.That(world.Runtime.Results.NativeResultTimer, Is.EqualTo(1));
            Assert.That(world.Runtime.Results.NativeResultOutputTimer, Is.EqualTo(1));
            Assert.That(world.Runtime.Results.NativeLivingGroupMask,
                Is.EqualTo(1UL << 1));
        }

        private static TestCombatant CreateCombatant(
            SimulationWorld world, int slot, int group, int objectId,
            int x, bool attacker, int hp)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 3,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            if (attacker)
            {
                frame.itrs.Add(new InteractionArea
                {
                    kind = 0,
                    x = -30,
                    y = -10,
                    w = 60,
                    h = 20,
                    zwidth = 15,
                    injury = 30,
                    dvx = 1,
                    arest = 4,
                    vrest = 1,
                });
            }
            else
            {
                frame.bodies.Add(new BodyBox
                {
                    kind = 0,
                    x = -10,
                    y = -10,
                    w = 20,
                    h = 20,
                });
            }

            var entity = new TestCombatant();
            entity.ModuleInitialize();
            entity.Name = "Q08Lethal" + slot;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = entity.Name,
                    type_sub = objectId,
                    frames = new List<LF2FrameData> { frame },
                }));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.Prev = 0;
            entity.Frame.Prev2 = 0;
            entity.Frame.Prev2D = entity.Frame.D;
            entity.Runtime.PrevFrame2 = 0;
            entity.Initialize(500, 500);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Team = group;
            entity.RelationTeam = group;
            entity.HP2Orig = 1;
            entity.Health.HP = hp;
            entity.Health.HPBound = hp;
            entity.Health.HP3 = hp;
            entity.Runtime.SetPosition(x, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            entity.RefreshRuntimeSnapshot();
            return entity;
        }

        private sealed class TestCombatant : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;
        }
    }
}
#endif
