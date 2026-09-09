#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C25GFrameBodyEditorTests
    {
        [Test]
        public void ExactCharacterEcsWrapperUsesTheSharedC25GCore()
        {
            string source = File.ReadAllText(Path.Combine(
                UnityEngine.Application.dataPath,
                "NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameTickPass.cs"));

            Assert.That(source, Does.Contain("character.RunNativeC25FrameBodyForWorldPass()"));
            Assert.That(source, Does.Not.Contain("character.RunReleaseFrameTickCounters()"));
            Assert.That(source, Does.Not.Contain("character.AttackingCounter++"));
        }

        [Test]
        public void TerminalPhysicalParticipantRetainsDeadStateFourteenFrame()
        {
            LF2FrameData lying = Frame(0, LF2States.Lying, 0, 1);
            var world = new SimulationWorld();
            LF2Character character = Character(
                world,
                new LF2CharacterData
                {
                    name = "C25gTerminal",
                    type_sub = (int)LF2ObjectType.Character,
                    frames = new List<LF2FrameData>
                    {
                        lying,
                        Frame(1, LF2States.Standing, 100, 1),
                    },
                },
                slot: 0,
                action: 0,
                hp: 0);
            character.HP2Orig = 1;
            character.RespawnCount = 0;

            RunC25G(character);

            Assert.That(character.Frame.N, Is.Zero);
            Assert.That(character.AttackingCounter, Is.Zero);
        }

        [Test]
        public void TypeThreeState3007SkipsAliveDrainAndUsesDeadFallbackTen()
        {
            LF2FrameData state3007 = Frame(20, 3007, 100, 20);
            state3007.hit_a = 9;
            state3007.hit_d = 0;
            var data = new LF2CharacterData
            {
                name = "C25gState3007",
                type_sub = (int)LF2ObjectType.SpecialAttack,
                frames = new List<LF2FrameData>
                {
                    Frame(10, 15, 100, 10),
                    state3007,
                },
            };

            var aliveWorld = new SimulationWorld();
            TypedCharacter alive = Typed(
                aliveWorld,
                data,
                LF2ObjectType.SpecialAttack,
                slot: 50,
                action: 20,
                hp: 5);
            RunC25G(alive);
            Assert.That(alive.Health.HP, Is.EqualTo(5));
            Assert.That(alive.Frame.N, Is.EqualTo(20));

            var deadWorld = new SimulationWorld();
            TypedCharacter dead = Typed(
                deadWorld,
                data,
                LF2ObjectType.SpecialAttack,
                slot: 50,
                action: 20,
                hp: 0);
            RunC25G(dead);
            Assert.That(dead.Health.HP, Is.Zero);
            Assert.That(dead.Frame.N, Is.EqualTo(10));
            Assert.That(dead.AttackingCounter, Is.EqualTo(1));
        }

        [Test]
        public void TypeTwoGroundedLowVelocityDoesNotUseLegacyFrameBodyEarlyReturn()
        {
            var world = new SimulationWorld();
            TypedCharacter heavy = Typed(
                world,
                new LF2CharacterData
                {
                    name = "C25gType2",
                    type_sub = (int)LF2ObjectType.HeavyWeapon,
                    frames = new List<LF2FrameData>
                    {
                        Frame(0, LF2States.HeavyWeaponInSky, 0, 1),
                        Frame(1, LF2States.HeavyWeaponInSky, 100, 1),
                    },
                },
                LF2ObjectType.HeavyWeapon,
                slot: 50,
                action: 0,
                hp: 100);
            heavy.Runtime.SetPosition(0, 0, 0);
            heavy.Runtime.SetVelocity(0, 0, 0);
            heavy.Runtime.SyncIntegerPosition();

            RunC25G(heavy);

            Assert.That(heavy.Frame.N, Is.EqualTo(1));
        }

        private static LF2Character Character(
            SimulationWorld world,
            LF2CharacterData data,
            int slot,
            int action,
            int hp)
        {
            var character = new LF2Character();
            Initialize(character, world, data, slot, action, hp);
            return character;
        }

        private static TypedCharacter Typed(
            SimulationWorld world,
            LF2CharacterData data,
            LF2ObjectType type,
            int slot,
            int action,
            int hp)
        {
            var character = new TypedCharacter(type);
            Initialize(character, world, data, slot, action, hp);
            return character;
        }

        private static void Initialize(
            LF2Character character,
            SimulationWorld world,
            LF2CharacterData data,
            int slot,
            int action,
            int hp)
        {
            character.ModuleInitialize();
            character.ObjectId = 7800 + slot;
            character.FrameCache.Load(new LF2CharacterDataWrapper(character.ObjectId, data));
            character.Initialize(500, 500);
            character.Frame.N = action;
            character.Frame.PN = action;
            character.Frame.D = character.FrameCache.GetFrameDataById(action);
            character.Trans.SyncDirectFrameData(
                character.Frame.D.wait,
                character.Frame.D.next,
                action);
            character.SetRequiredRuntimeSlot(slot);
            character.RelationTeam = 1;
            character.Health.HP = hp;
            character.Runtime.SuppressLateFrameTickUntilTick = 0;
            world.Register(character);
        }

        private static LF2FrameData Frame(int id, int state, int wait, int next)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = wait,
                next = next,
                centerx = 39,
                centery = 79,
            };
        }

        private static void RunC25G(LF2Entity entity)
        {
            entity.BeginNativeC25FrameTickForWorldPass();
            try
            {
                entity.RunNativeC25FrameBodyForWorldPass();
            }
            finally
            {
                entity.EndNativeC25FrameTickForWorldPass();
            }
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly int dataType;

            internal TypedCharacter(LF2ObjectType dataType)
            {
                this.dataType = (int)dataType;
            }

            public override int GetCurrentDataObjectTypeForSimulation() => dataType;
        }
    }
}
#endif
