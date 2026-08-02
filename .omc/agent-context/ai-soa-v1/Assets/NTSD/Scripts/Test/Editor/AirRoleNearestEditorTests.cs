#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AirRoleNearestEditorTests
    {
        [Test]
        public void SnapshotCount_TracksEmptyAirTreeAndGroundAirGroundMutation()
        {
            var world = new SimulationWorld();
            LF2Character self = CreateCharacter("Self", 33, 0, 1, 0, 0, 200, 500);
            LF2Character candidate =
                CreateCharacter("Candidate", 4, 1, 2, 40, 0, 200, 500);
            world.Register(self);
            world.Register(candidate);

            object[] arguments = { candidate, 14, 0, 0, 0, 0, 0, 0 };
            bool valid = (bool)Invoke(
                world,
                "AiAirRoleCountMutationForSelfCheck",
                arguments);

            Assert.That(valid, Is.True);
            Assert.That((int)arguments[5], Is.Zero);
            Assert.That((int)arguments[6], Is.EqualTo(1));
            Assert.That((int)arguments[7], Is.Zero);
        }

        [Test]
        public void EmptyAirFastPath_PreservesGroundSelectionDistanceTieAndSameZ()
        {
            var world = new SimulationWorld();
            LF2Character self = CreateCharacter("Self", 33, 0, 1, 0, 0, 200, 500);
            world.Register(self);
            world.Register(CreateCharacter("TieLow", 4, 1, 2, -20, 0, 205, 500));
            world.Register(CreateCharacter("TieHigh", 4, 2, 2, 20, 0, 195, 500));
            world.Register(CreateCharacter("SameTeam", 4, 3, 1, 3, 0, 200, 500));
            world.Register(CreateCharacter("DeadEnemy", 4, 4, 2, 1, 0, 200, 0));
            world.Register(CreateCharacter("YBoundary", 4, 5, 2, 100, 2, 214, 500));

            object[] arguments = { self, 0, false, 0, false, 0, 0 };
            bool matches = (bool)Invoke(
                world,
                "AiAirFastPathMatchesOracleForSelfCheck",
                arguments);

            Assert.That(matches, Is.True);
            Assert.That((int)arguments[3], Is.Zero);
            Assert.That((bool)arguments[4], Is.True);
            Assert.That((int)arguments[5], Is.Zero,
                "The production best-first path must skip only the empty air pass.");
            Assert.That((int)arguments[6], Is.EqualTo(1),
                "The direct oracle path must still execute the air pass.");
        }

        [Test]
        public void InvalidCount_DisablesFastPathAndFallsBackToUnoptimizedAirOracle()
        {
            var world = new SimulationWorld();
            LF2Character self = CreateCharacter("Self", 33, 0, 1, 0, 0, 200, 500);
            world.Register(self);
            world.Register(CreateCharacter("Enemy", 4, 1, 2, 30, 0, 205, 500));

            object[] arguments = { self, 0, true, 0, false, 0, 0 };
            bool matches = (bool)Invoke(
                world,
                "AiAirFastPathMatchesOracleForSelfCheck",
                arguments);

            Assert.That(matches, Is.True);
            Assert.That((int)arguments[5], Is.EqualTo(1));
            Assert.That((int)arguments[6], Is.EqualTo(1));
        }

        [Test]
        public void NullMutation_InvalidatesOtherwiseValidSnapshotCount()
        {
            var world = new SimulationWorld();
            world.Register(CreateCharacter("Self", 33, 0, 1, 0, 0, 200, 500));

            Assert.That(
                Invoke(
                    world,
                    "AiAirNullMutationInvalidatesCountForSelfCheck",
                    Array.Empty<object>()),
                Is.True);
        }

        [Test]
        public void InvalidCoordinate_FailClosesSnapshotCount()
        {
            var world = new SimulationWorld();
            LF2Character self = CreateCharacter("Self", 33, 0, 1, 0, 0, 200, 500);
            LF2Character candidate =
                CreateCharacter("Candidate", 4, 1, 2, 30, 0, 205, 500);
            world.Register(self);
            world.Register(candidate);

            object[] arguments = { candidate, 0, true };
            Assert.That(
                Invoke(
                    world,
                    "AiAirInvalidCoordinateInvalidatesCountForSelfCheck",
                    arguments),
                Is.True);
            Assert.That((int)arguments[1], Is.Zero);
            Assert.That((bool)arguments[2], Is.False);
        }

        [Test]
        public void ForceFullLegacyAndShadowModesStillExecuteTheirAirPasses()
        {
            var world = new SimulationWorld();
            LF2Character self = CreateCharacter("Self", 33, 0, 1, 0, 0, 200, 500);
            world.Register(self);
            world.Register(CreateCharacter("Enemy", 4, 1, 2, 30, 0, 205, 500));

            Assert.That(InvokeModePassCount(world, self, false, false, false), Is.Zero);
            Assert.That(InvokeModePassCount(world, self, true, false, false), Is.EqualTo(1));
            Assert.That(InvokeModePassCount(world, self, false, true, false), Is.EqualTo(1));
            Assert.That(InvokeModePassCount(world, self, false, false, true), Is.EqualTo(1),
                "The unoptimized legacy-spatial shadow must still execute its air pass.");
            Assert.That(InvokeModePassCount(world, self, false, true, true), Is.EqualTo(1),
                "Legacy remains the formal oracle while the shadow best-first may fast-skip.");
        }

        [Test]
        public void AirCandidateBoundariesAndRandomFilters_MatchAuthorityBrute()
        {
            var random = new Random(0xA17);
            var world = new SimulationWorld();
            LF2Character self = CreateCharacter("Self", 33, 0, 1, 0, 0, 200, 500);
            world.Register(self);
            for (int slot = 1; slot <= 64; slot++)
            {
                int state = slot % 7 == 0 ? 14 : 0;
                int y = slot % 5 == 0 ? 3 : slot % 5 == 1 ? 2 : 0;
                int team = slot % 4 == 0 ? 1 : slot % 4 == 1 ? 5 : 2;
                int hp = slot % 9 == 0 ? 0 : random.Next(1, 501);
                int x = random.Next(-260, 261);
                int z = 200 + random.Next(-45, 46);
                world.Register(CreateCharacter(
                    "Candidate" + slot,
                    4,
                    slot,
                    team,
                    x,
                    y,
                    z,
                    hp,
                    state));
            }

            Assert.That(
                Invoke(
                    world,
                    "AiNearestSpatialMatchesBruteForSelfCheck",
                    new object[] { self, 0 }),
                Is.True);
            Assert.That(
                Invoke(
                    world,
                    "AiNearestSpatialMatchesBruteForSelfCheck",
                    new object[] { self, 1 }),
                Is.True);
        }

        [Test]
        public void EmptyAirFastPath_WarmedQueriesAllocateZeroManagedBytes()
        {
            var world = new SimulationWorld();
            LF2Character self = CreateCharacter("Self", 33, 0, 1, 0, 0, 200, 500);
            world.Register(self);
            for (int slot = 1; slot <= 128; slot++)
            {
                world.Register(CreateCharacter(
                    "Ground" + slot,
                    4,
                    slot,
                    2,
                    slot * 3,
                    slot % 3 - 1,
                    180 + slot % 30,
                    500));
            }

            long allocated = (long)Invoke(
                world,
                "MeasureAiAirZeroFastPathAllocationsForSelfCheck",
                new object[] { self, 0, 512 });
            Assert.That(allocated, Is.Zero);
        }

        private static int InvokeModePassCount(
            SimulationWorld world,
            LF2Entity self,
            bool forceFull,
            bool forceLegacy,
            bool shadow)
        {
            return (int)Invoke(
                world,
                "AiAirExecutionModePassCountForSelfCheck",
                new object[] { self, 0, forceFull, forceLegacy, shadow });
        }

        private static object Invoke(
            SimulationWorld world,
            string methodName,
            object[] arguments)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            return method.Invoke(world, arguments);
        }

        private static LF2Character CreateCharacter(
            string name,
            int objectId,
            int runtimeSlot,
            int team,
            int x,
            int y,
            int z,
            int hp,
            int state = 0)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = state,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 1,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new NearestSelfCheckController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.Runtime.HP = hp;
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(runtimeSlot);
            character.Team = team;
            character.RelationTeam = team;
            character.Runtime.SetPosition(x, y, z);
            character.Runtime.SyncIntegerPosition();
            return character;
        }

        private sealed class NearestSelfCheckController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsJump => false;
            bool ILF2Controller.IsDefend => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }
    }
}
#endif
