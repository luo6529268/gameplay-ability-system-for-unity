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
    public sealed class NTSD28C023AirborneIdleFrameEditorTests
    {
        private const string StagedRuntime =
            "Assets/NTSD/Content/LoganRuntime";

        [TestCase(0, -20.0, 0, 212, 1)]
        [TestCase(2, -20.0, 0, 212, 0)]
        [TestCase(0, 0.0, 0, 0, 1)]
        [TestCase(0, -20.0, -20, 212, 1)]
        public void CurrentNarutoIdleReadsJumpWaitAfterCounterIncrement(
            int initialCounter,
            double y,
            int floorReference,
            int expectedAction,
            int expectedCounter)
        {
            var catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(StagedRuntime),
                ProjectBattleModeConfig.LoadDefault().Capture());
            Dictionary<int, LF2CharacterDataWrapper> configs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(
                    catalog);
            Assert.That(configs.TryGetValue(2, out LF2CharacterDataWrapper data),
                Is.True, "current staged catalog must contain formal OID2");
            LF2FrameData idle = data.characterData.frames.Find(
                frame => frame.frameId == 0);
            LF2FrameData jump = data.characterData.frames.Find(
                frame => frame.frameId == 212);
            Assert.That(idle, Is.Not.Null);
            Assert.That(jump, Is.Not.Null);
            Assert.That(idle.state, Is.Zero);
            Assert.That(idle.wait, Is.EqualTo(3));
            Assert.That(jump.wait, Is.EqualTo(1));
            Assert.That(jump.next, Is.Zero);

            LF2Character character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.characterData.name;
            character.ObjectId = 2;
            character.FrameCache.Load(data);
            character.WriteCurrentFrameId(0);
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Initialize(500, 500);
            character.Trans.SyncDirectFrameData(idle.wait, idle.next, 0);
            character.SetRequiredRuntimeSlot(0);
            character.Team = 1;
            character.RelationTeam = 1;
            character.Runtime.ObjType = 0;
            character.Runtime.HP = 500;
            character.Runtime.HP3 = 500;
            character.Runtime.HPBound = 500;
            character.Runtime.CollisionYReference = floorReference;
            character.Runtime.SetPosition(0.0, y, 0.0);
            character.Runtime.SetVelocity(0.0, 0.0, 0.0);
            character.Runtime.SyncIntegerPosition();
            character.Controller = new EmptyController();
            character.AiControlled = false;
            character.PS.groundY = floorReference;
            character.AttackingCounter = initialCounter;

            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            world.Register(character);
            try
            {
                character.Runtime.CollisionYReference = floorReference;
                Assert.That(character.Runtime.CollisionYReference,
                    Is.EqualTo(floorReference),
                    "fixture floor reference after world registration");
                var input = new FrameInputSet(1,
                    Array.Empty<SimulationPlayerInput>());
                new NTSDBattleTickSystem(world).RunReleaseTick(
                    1, false, input);

                Assert.That(character.Frame.N, Is.EqualTo(expectedAction),
                    $"action after production tick: Y={character.Runtime.Y}, " +
                    $"YInt={character.Runtime.YInt}, " +
                    $"floor={character.Runtime.CollisionYReference}, " +
                    $"Vy={character.Runtime.Vy}, " +
                    $"counter={character.AttackingCounter}, " +
                    $"waitCounter={character.Trans.WaitCounter}, " +
                    $"frameDelay={character.FrameDelay}");
                Assert.That(character.AttackingCounter,
                    Is.EqualTo(expectedCounter),
                    "counter after the production tick");
                Assert.That(character.Trans.WaitCounter,
                    Is.EqualTo(expectedAction),
                    "frame latch after the production tick");
                if (floorReference < 0)
                    Assert.That(character.Runtime.CollisionYReference,
                        Is.Zero,
                        "native candidate pass clears the prior floor reference");
                if (initialCounter == 2 && expectedAction == 212)
                    Assert.That(character.Runtime.Vy,
                        Is.EqualTo(1.7).Within(0.0001),
                        "ordinary native gravity must remain without a jump-entry impulse");
            }
            finally
            {
                world.Unregister(character);
            }
        }

        [Test]
        public void SharedNativeFrameBodyRedirectsTypeThreeAndHonorsFloorReference()
        {
            LF2Character airborne = CreateSyntheticTypeThree(0);
            RunNativeFrameBody(airborne);
            Assert.That(airborne.Frame.N, Is.EqualTo(212));
            Assert.That(airborne.AttackingCounter, Is.Zero);
            Assert.That(airborne.Trans.WaitCounter, Is.EqualTo(212));
            Assert.That(airborne.Health.HP, Is.EqualTo(20));
            Assert.That(airborne.Runtime.Vy, Is.Zero);

            LF2Character matchingFloor = CreateSyntheticTypeThree(-20);
            RunNativeFrameBody(matchingFloor);
            Assert.That(matchingFloor.Frame.N, Is.Zero);
            Assert.That(matchingFloor.AttackingCounter, Is.EqualTo(3));
            Assert.That(matchingFloor.Trans.WaitCounter, Is.Zero);
        }

        private static LF2Character CreateSyntheticTypeThree(int floorReference)
        {
            var idle = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 3,
                next = 0,
            };
            var jump = new LF2FrameData
            {
                frameId = 212,
                state = 4,
                wait = 1,
                next = 0,
            };
            var data = new LF2CharacterData
            {
                name = "C023SharedTypeThree",
                type_sub = (int)LF2ObjectType.SpecialAttack,
                frames = new List<LF2FrameData> { idle, jump },
            };
            var character = new TypedCharacter();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = 7023;
            character.FrameCache.Load(new LF2CharacterDataWrapper(7023, data));
            character.WriteCurrentFrameId(0);
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Initialize(20, 20);
            character.Trans.SyncDirectFrameData(idle.wait, idle.next, 0);
            character.Runtime.ObjType = (int)LF2ObjectType.SpecialAttack;
            character.Runtime.CollisionYReference = floorReference;
            character.Runtime.SetPosition(0.0, -20.0, 0.0);
            character.Runtime.SetVelocity(0.0, 0.0, 0.0);
            character.Runtime.SyncIntegerPosition();
            character.Health.HP = 20;
            character.AttackingCounter = 2;
            Assert.That(character.GetCurrentDataObjectTypeForSimulation(),
                Is.EqualTo((int)LF2ObjectType.SpecialAttack));
            return character;
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

        private sealed class TypedCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.SpecialAttack;
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
    }
}
#endif
