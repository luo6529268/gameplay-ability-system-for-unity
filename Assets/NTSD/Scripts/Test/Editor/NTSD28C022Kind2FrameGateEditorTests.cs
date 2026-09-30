#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28C022Kind2FrameGateEditorTests
    {
        private const string StagedRuntime =
            "Assets/NTSD/Content/LoganRuntime";

        [Test]
        public void NarutoOneTailState1700Kind2HoldsFrameAndCounter()
        {
            var catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(StagedRuntime),
                ProjectBattleModeConfig.LoadDefault().Capture());
            Dictionary<int, LF2CharacterDataWrapper> configs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            Assert.That(configs.TryGetValue(52, out LF2CharacterDataWrapper data),
                Is.True, "current staged catalog must contain formal OID52");
            LF2Character character = CreateCharacter(data, 52, 130, 1);
            Assert.That(character.Frame.D.state, Is.EqualTo(1700));
            Assert.That(character.Frame.D.HasPrimaryCatchPoint, Is.True);
            Assert.That(character.Frame.D.PrimaryCatchPoint.Kind, Is.EqualTo(2));

            var holderFrame = new LF2FrameData
            {
                frameId = 0,
                state = 9,
                wait = 100,
                next = 0,
                cpoint = new CatchPoint
                {
                    kind = 1,
                    x = 41,
                    y = 39,
                    vaction = 130,
                },
            };
            var holderData = new LF2CharacterData
            {
                name = "C022SourceEquivalentHolder",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { holderFrame },
            };
            LF2Character holder = CreateCharacter(
                new LF2CharacterDataWrapper(7002, holderData), 7002, 0);

            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            world.Register(holder);
            world.Register(character);
            try
            {
                holder.Runtime.SetPosition(100.0, -20.0, 0.0);
                holder.Runtime.SyncIntegerPosition();
                character.Runtime.SetPosition(100.0, -20.0, 0.0);
                character.Runtime.SetVelocity(3.0, 1.0, 0.0);
                character.Runtime.SyncIntegerPosition();
                holder.CaughtSlotIndex = 1;
                holder.Runtime.CaughtDuration = 100;
                character.Runtime.CatchSourceSlot90 = 0;
                character.Runtime.CatcherSlotIndex = 0;
                character.AttackingCounter = 1;
                var input = new FrameInputSet(1,
                    Array.Empty<SimulationPlayerInput>());
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);

                Assert.That(character.Frame.N, Is.EqualTo(130));
                Assert.That(character.AttackingCounter, Is.EqualTo(1));
                Assert.That(character.Runtime.Vx, Is.EqualTo(3.0));
                Assert.That(character.Runtime.Vy, Is.EqualTo(1.0));
            }
            finally
            {
                world.Unregister(character);
                world.Unregister(holder);
            }
        }

        [Test]
        public void State10WithoutKind2DoesNotImposeTheFrameGate()
        {
            var frame = new LF2FrameData
            {
                frameId = 400,
                state = 10,
                wait = 100,
                next = 400,
            };
            var data = new LF2CharacterData
            {
                name = "C022State10NoCpointControl",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };
            LF2Character character = CreateCharacter(
                new LF2CharacterDataWrapper(7001, data), 7001, 400);
            Assert.That(character.Frame.D.HasPrimaryCatchPoint, Is.False);

            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            world.Register(character);
            try
            {
                character.Runtime.SetPosition(100.0, -20.0, 0.0);
                character.Runtime.SetVelocity(3.0, 1.0, 0.0);
                character.Runtime.SyncIntegerPosition();
                character.AttackingCounter = 1;
                var input = new FrameInputSet(1,
                    Array.Empty<SimulationPlayerInput>());
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);

                Assert.That(character.Frame.N, Is.EqualTo(400));
                Assert.That(character.AttackingCounter, Is.EqualTo(2));
                Assert.That(character.Runtime.X, Is.GreaterThan(100.0),
                    "state10 without kind2 must not suppress X integration");
            }
            finally
            {
                world.Unregister(character);
            }
        }

        [Test]
        public void CurrentNarutoOneTailKind2SuppressesIsolatedPhysics()
        {
            var catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(StagedRuntime),
                ProjectBattleModeConfig.LoadDefault().Capture());
            Dictionary<int, LF2CharacterDataWrapper> configs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            Assert.That(configs.TryGetValue(52, out LF2CharacterDataWrapper data),
                Is.True);
            LF2Character character = CreateCharacter(data, 52, 130, 1);
            Assert.That(character.Frame.D.state, Is.EqualTo(1700));
            Assert.That(character.Frame.D.HasPrimaryCatchPoint, Is.True);
            Assert.That(character.Frame.D.PrimaryCatchPoint.Kind, Is.EqualTo(2));

            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            world.Register(character);
            try
            {
                character.Runtime.SetPosition(100.0, -20.0, 0.0);
                character.Runtime.SetVelocity(3.0, 1.0, 0.0);
                character.Runtime.SyncIntegerPosition();
                Assert.That(character.RunNativePhysicsForWorldPass(1), Is.False);
                Assert.That(character.Runtime.X, Is.EqualTo(100.0));
                Assert.That(character.Runtime.Y, Is.EqualTo(-20.0));
                Assert.That(character.Runtime.Vx, Is.EqualTo(3.0));
                Assert.That(character.Runtime.Vy, Is.EqualTo(1.0));
            }
            finally
            {
                world.Unregister(character);
            }
        }

        [Test]
        public void Kind2TypeThreeSkipsDrainAndCounterInNativeFrameBody()
        {
            var caught = new LF2FrameData
            {
                frameId = 0,
                state = 3000,
                wait = 100,
                next = 0,
                hit_a = 4,
                cpoint = new CatchPoint { kind = 2 },
            };
            var free = new LF2FrameData
            {
                frameId = 1,
                state = 3000,
                wait = 100,
                next = 1,
                hit_a = 4,
            };
            var data = new LF2CharacterData
            {
                name = "C022TypeThreeFrameGate",
                type_sub = (int)LF2ObjectType.SpecialAttack,
                frames = new List<LF2FrameData> { caught, free },
            };
            LF2Character character = CreateCharacter(
                new LF2CharacterDataWrapper(7003, data), 7003, 0,
                type: LF2ObjectType.SpecialAttack);
            character.Runtime.ObjType = (int)LF2ObjectType.SpecialAttack;
            Assert.That(character.GetCurrentDataObjectTypeForSimulation(),
                Is.EqualTo((int)LF2ObjectType.SpecialAttack));
            character.Health.HP = 20;
            character.AttackingCounter = 1;

            RunNativeFrameBody(character);

            Assert.That(character.Health.HP, Is.EqualTo(20));
            Assert.That(character.Frame.N, Is.Zero);
            Assert.That(character.AttackingCounter, Is.EqualTo(1));

            character.Runtime.SetPosition(100.0, -20.0, 0.0);
            character.Runtime.SetVelocity(3.0, 1.0, 0.0);
            character.Runtime.SyncIntegerPosition();
            Assert.That(character.RunNativePhysicsForWorldPass(1), Is.False);
            Assert.That(character.Runtime.X, Is.EqualTo(100.0));
            Assert.That(character.Runtime.Y, Is.EqualTo(-20.0));
            Assert.That(character.Runtime.Vx, Is.EqualTo(3.0));
            Assert.That(character.Runtime.Vy, Is.EqualTo(1.0));

            character.SetCpointRawFramePreserveWait(1);
            character.Trans.SyncDirectFrameData(free.wait, free.next, 1);
            character.AttackingCounter = 1;
            RunNativeFrameBody(character);

            Assert.That(character.Health.HP, Is.EqualTo(16));
            Assert.That(character.Frame.N, Is.EqualTo(1));
            Assert.That(character.AttackingCounter, Is.EqualTo(2));
        }

        private static void RunNativeFrameBody(LF2Entity entity)
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

        private static LF2Character CreateCharacter(
            LF2CharacterDataWrapper data,
            int objectId,
            int initialFrame,
            int slot = 0,
            LF2ObjectType type = LF2ObjectType.Character)
        {
            LF2Character character = type == LF2ObjectType.Character
                ? new LF2Character()
                : new TypedCharacter(type);
            character.ModuleInitialize();
            character.Name = data.characterData.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(data);
            character.WriteCurrentFrameId(initialFrame);
            character.Frame.D =
                character.FrameCache.GetFrameDataById(initialFrame);
            character.Frame.PN = initialFrame;
            character.Initialize(500, 500);
            character.Trans.SyncDirectFrameData(
                character.Frame.D.wait,
                character.Frame.D.next,
                initialFrame);
            character.SetRequiredRuntimeSlot(slot);
            character.Team = slot + 1;
            character.RelationTeam = slot + 1;
            character.Runtime.ObjType = 0;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Controller = new EmptyController();
            character.AiControlled = false;
            character.PS.groundY = 0f;
            return character;
        }

        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } =
                new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsDefend => false;
            bool ILF2Controller.IsJump => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly int dataType;

            internal TypedCharacter(LF2ObjectType dataType)
            {
                this.dataType = (int)dataType;
            }

            public override int GetCurrentDataObjectTypeForSimulation() =>
                dataType;
        }
    }
}
#endif
