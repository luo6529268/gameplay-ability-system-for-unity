#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class GroundRoleNearestEditorTests
    {
        [TestCase(0x1234)]
        [TestCase(0x5EED)]
        [TestCase(0x7FFF)]
        public void GroundRoleNearest_RandomizedLayoutsMatchAuthorityFullScan(int seed)
        {
            const int count = 96;
            var random = new System.Random(seed);
            var world = new SimulationWorld();
            var entities = new List<LF2Character>(count);
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                for (int slot = 0; slot < count; slot++)
                {
                    int state = random.Next(0, 5) == 0 ? 14 : 0;
                    int y = random.Next(0, 4) == 0
                        ? random.Next(0, 2) == 0 ? -3 : 3
                        : random.Next(-2, 3);
                    LF2Character entity = CreateCharacter(
                        $"GroundRandom_{seed}_{slot}",
                        slot,
                        random.Next(1, 5),
                        random.Next(-9000, 9001),
                        y,
                        random.Next(-9000, 9001),
                        state);
                    if (random.Next(0, 9) == 0)
                        entity.Runtime.HP = 0;
                    world.Register(entity);
                    entities.Add(entity);
                }

                for (int query = 0; query < 16; query++)
                {
                    LF2Character self = entities[random.Next(entities.Count)];
                    int inputPhase = random.Next(0, 2) == 0 ? 1 : 2;
                    GroundParityResult result = CaptureGroundParity(
                        world,
                        self,
                        inputPhase);
                    Assert.That(result.Matches, Is.True,
                        $"seed={seed} query={query} self={self.Runtime.SlotIndex}");
                }
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void GroundRoleNearest_PreservesStrictDistanceTieSlotAndFiltering()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(world, 0, 1, 0, 0, 0, 0);
                RegisterCharacter(world, 9, 2, -5, 0, 5, 0);
                RegisterCharacter(world, 2, 2, 5, 0, -5, 0);
                RegisterCharacter(world, 4, 2, 9999, 0, 0, 0);
                RegisterCharacter(world, 1, 2, 10000, 0, 0, 0);
                RegisterCharacter(world, 6, 1, 1, 0, 0, 0);
                RegisterCharacter(world, 7, 2, 1, 3, 0, 0);
                RegisterCharacter(world, 8, 2, 1, 0, 0, 14);

                GroundParityResult result = CaptureGroundParity(world, self, 2);
                Assert.That(result.Matches, Is.True);
                Assert.That(result.SelectedSlot, Is.EqualTo(2));
                Assert.That(result.SelectedDistance, Is.EqualTo(10));
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        [Category("NTSD_W08Regression")]
        public void GroundRoleNearest_LiveRoleUpdatesAndPositionFailClosedMatchAuthority()
        {
            var world = new SimulationWorld();
            world.ForceLegacyAiNearestFilterForDiagnostics = true;
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(world, 0, 1, 0, 0, 0, 0);
                LF2Character target = RegisterCharacter(world, 7, 2, 100, 0, 10, 0);
                RegisterCharacter(world, 8, 2, 500, 3, 50, 0);

                GroundMutationResult stationary = MutateGroundRole(
                    world, self, target, 2, 100, 0, 10, 0);
                Assert.That(stationary.Matches, Is.True, "stationary ground role");
                Assert.That(stationary.FullRebuildDelta, Is.Zero);
                Assert.That(stationary.InPlaceUpdateDelta, Is.Zero);
                Assert.That(stationary.MigrationDelta, Is.Zero);

                GroundMutationResult moved = MutateGroundRole(
                    world, self, target, 2, 101, 0, 10, 0);
                Assert.That(moved.Matches, Is.True, "position mutation fail-closed path");
                Assert.That(moved.FullRebuildDelta, Is.Zero);
                Assert.That(
                    moved.InPlaceUpdateDelta + moved.MigrationDelta,
                    Is.Zero);

                GroundMutationResult airborne = MutateGroundRole(
                    world, self, target, 2, 101, 3, 10, 0);
                Assert.That(airborne.Matches, Is.True, "ground-to-air role mutation");
                Assert.That(airborne.FullRebuildDelta, Is.Zero);

                GroundMutationResult landed = MutateGroundRole(
                    world, self, target, 2, 101, 0, 10, 0);
                Assert.That(landed.Matches, Is.True, "air-to-ground role mutation");
                Assert.That(landed.FullRebuildDelta, Is.Zero);
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void GroundRoleNearest_SlotGenerationReuseMatchesAuthority()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(world, 0, 1, 0, 0, 0, 0);
                LF2Character first = RegisterCharacter(world, 7, 2, 30, 0, 0, 0);
                RegisterCharacter(world, 8, 2, 500, 3, 50, 0);
                RuntimeEntityHandle firstHandle = CurrentHandle(world, first);
                Assert.That(CaptureGroundParity(world, self, 2).Matches, Is.True);

                world.Unregister(first);
                LF2Character reused = RegisterCharacter(world, 7, 2, -40, 0, 5, 0);
                RuntimeEntityHandle reusedHandle = CurrentHandle(world, reused);
                Assert.That(reusedHandle.Slot, Is.EqualTo(firstHandle.Slot));
                Assert.That(reusedHandle.Generation, Is.GreaterThan(firstHandle.Generation));

                GroundParityResult result = CaptureGroundParity(world, self, 2);
                Assert.That(result.Matches, Is.True);
                Assert.That(result.SelectedSlot, Is.EqualTo(7));
                Assert.That(result.SelectedDistance, Is.EqualTo(45));
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void GroundRoleNearest_EmptyResultAndCorruptRoleIndexFailClosedToAllIndex()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(world, 0, 1, 0, 0, 0, 0);
                GroundParityResult empty = CaptureGroundParity(world, self, 2);
                Assert.That(empty.Matches, Is.True);
                Assert.That(empty.SelectedSlot, Is.EqualTo(-1));
                Assert.That(empty.SelectedDistance, Is.EqualTo(10000));

                RegisterCharacter(world, 5, 2, 40, 0, 0, 0);
                RegisterCharacter(world, 6, 2, 80, 3, 0, 0);
                Assert.That(InvokeBool(
                    world,
                    "AiGroundFailClosedFallbackMatchesBruteForSelfCheck",
                    self,
                    2), Is.True);
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        [Timeout(30000)]
        public void GroundRoleNearest_ThousandMixedRolesReduceVisitedRecords()
        {
            const int count = 1000;
            var world = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended,
                1100);
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(world, 0, 1, 0, 0, 0, 0);
                for (int slot = 1; slot < count; slot++)
                {
                    bool ground = slot >= 900;
                    int x = ground
                        ? 4000 + (slot - 900) * 10
                        : (slot % 30) - 15;
                    int z = ground
                        ? (slot - 900) % 7
                        : ((slot / 30) % 30) - 15;
                    RegisterCharacter(
                        world,
                        slot,
                        slot % 2 == 0 ? 1 : 2,
                        x,
                        ground ? 0 : 3,
                        z,
                        0);
                }

                GroundParityResult result = CaptureGroundParity(world, self, 2);
                Assert.That(result.Matches, Is.True);
                Assert.That(result.GroundIndexedCount, Is.EqualTo(101));
                Assert.That(result.GroundVisitedRecords, Is.LessThan(result.AllVisitedRecords));
                Assert.That(
                    result.GroundVisitedRecords * 4,
                    Is.LessThan(result.AllVisitedRecords),
                    $"ground={result.GroundVisitedRecords} all={result.AllVisitedRecords}");
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void GroundRoleNearest_WarmedQueriesAllocateNoManagedMemory()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(world, 0, 1, 0, 0, 0, 0);
                for (int slot = 1; slot < 160; slot++)
                {
                    RegisterCharacter(
                        world,
                        slot,
                        slot % 2 + 1,
                        (slot % 20) * 30,
                        slot % 5 == 0 ? 3 : 0,
                        (slot / 20) * 20,
                        0);
                }

                Func<LF2Entity, int, int, int> runQueries =
                    CreateGroundQueryDelegate(world);
                int warmChecksum = runQueries(self, 2, 64);
                Assert.That(warmChecksum, Is.Not.EqualTo(int.MinValue));

                long before = GC.GetAllocatedBytesForCurrentThread();
                int checksum = runQueries(self, 2, 512);
                long after = GC.GetAllocatedBytesForCurrentThread();

                Assert.That(checksum, Is.Not.EqualTo(int.MinValue));
                Assert.That(after - before, Is.Zero);
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void RuntimeSlotOccupancyEpoch_ChangesOnlyForSuccessfulStructuralOperations()
        {
            var table = new RuntimeSlotTable(4, 1, 2);
            var first = new LF2Character();
            var second = new LF2Character();
            ulong epoch = table.OccupancyEpoch;
            Assert.That(epoch, Is.Not.Zero);

            Assert.That(table.TryClaim(-1, first, out _), Is.False);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));
            Assert.That(table.TryClaim(0, null, out _), Is.False);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));

            Assert.That(table.TryClaim(0, first, out RuntimeEntityHandle firstHandle), Is.True);
            epoch = NextEpoch(epoch);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));
            Assert.That(table.TryClaim(0, second, out _), Is.False);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));

            Assert.That(table.AllocateLowest(0, null, out _), Is.EqualTo(-1));
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));
            Assert.That(
                table.AllocateLowest(0, second, out RuntimeEntityHandle secondHandle),
                Is.EqualTo(1));
            epoch = NextEpoch(epoch);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));
            Assert.That(table.AllocateLowest(4, second, out _), Is.EqualTo(-1));
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));

            Assert.That(table.Release(RuntimeEntityHandle.Invalid), Is.False);
            Assert.That(table.Release(0, second), Is.False);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));
            Assert.That(table.Release(firstHandle), Is.True);
            epoch = NextEpoch(epoch);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));
            Assert.That(table.Release(firstHandle), Is.False);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));
            Assert.That(table.Release(secondHandle), Is.True);
            epoch = NextEpoch(epoch);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));

            Assert.That(table.GrowTo(4), Is.True);
            Assert.That(table.GrowTo(3), Is.False);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));
            Assert.That(table.GrowTo(8), Is.True);
            epoch = NextEpoch(epoch);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));

            table.Reset();
            epoch = NextEpoch(epoch);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(epoch));

            MethodInfo setEpoch = typeof(RuntimeSlotTable).GetMethod(
                "SetOccupancyEpochForSelfCheck",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(setEpoch, Is.Not.Null);
            setEpoch.Invoke(table, new object[] { ulong.MaxValue });
            Assert.That(table.TryClaim(0, first, out _), Is.True);
            Assert.That(table.OccupancyEpoch, Is.EqualTo(1UL));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void GroundRoleNearest_OccupancyMutationAfterBuildAbortsAndFallsBack(
            bool releaseBeforeQuery)
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(world, 0, 1, 0, 0, 0, 0);
                RegisterCharacter(world, 2, 2, 20, 0, 0, 0);
                RegisterCharacter(world, 9, 2, -20, 0, 0, 0);
                object[] args =
                {
                    self,
                    new LF2Character(),
                    30,
                    0,
                    releaseBeforeQuery,
                    false,
                    false,
                };
                bool matches = (bool)RequireMethod(
                        "AiNearestOccupancyMutationFallsBackForSelfCheck")
                    .Invoke(world, args);
                Assert.That(matches, Is.True);
                Assert.That((bool)args[5], Is.True, "epoch must change");
                Assert.That((bool)args[6], Is.True, "best-first must abort");
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void GroundRoleNearest_ReleaseReuseAndGenerationMismatchAbortSafely()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(world, 0, 1, 0, 0, 0, 0);
                LF2Character candidate =
                    RegisterCharacter(world, 2, 2, 20, 0, 0, 0);

                object[] reuseArgs =
                {
                    self,
                    candidate,
                    new LF2Character(),
                    0,
                    false,
                    false,
                };
                bool reuseMatches = (bool)RequireMethod(
                        "AiNearestOccupancyReuseFallsBackForSelfCheck")
                    .Invoke(world, reuseArgs);
                Assert.That(reuseMatches, Is.True);
                Assert.That((bool)reuseArgs[4], Is.True, "generation must change");
                Assert.That((bool)reuseArgs[5], Is.True, "best-first must abort");

                object[] generationArgs = { self, candidate, 0, false };
                bool generationMatches = (bool)RequireMethod(
                        "AiNearestGenerationMismatchFallsBackForSelfCheck")
                    .Invoke(world, generationArgs);
                Assert.That(generationMatches, Is.True);
                Assert.That(
                    (bool)generationArgs[3],
                    Is.True,
                    "a stale record generation must abort");
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        private static ulong NextEpoch(ulong epoch)
        {
            epoch++;
            return epoch == 0 ? 1UL : epoch;
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            int team,
            int x,
            int y,
            int z,
            int state)
        {
            LF2Character character = CreateCharacter(
                $"GroundRole_{slot}",
                slot,
                team,
                x,
                y,
                z,
                state);
            world.Register(character);
            return character;
        }

        private static LF2Character CreateCharacter(
            string name,
            int runtimeSlot,
            int team,
            int x,
            int y,
            int z,
            int state)
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
            character.ObjectId = 1;
            character.Controller = new GroundNearestController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(1, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(runtimeSlot);
            character.Team = team;
            character.RelationTeam = team;
            character.Runtime.SetPosition(x, y, z);
            character.Runtime.SyncIntegerPosition();
            return character;
        }

        private static GroundParityResult CaptureGroundParity(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase)
        {
            MethodInfo method = RequireMethod(
                "AiGroundNearestMatchesBruteForSelfCheck");
            object[] args = { self, inputPhase, 0, 0, 0, -1, 10000 };
            bool matches = (bool)method.Invoke(world, args);
            return new GroundParityResult
            {
                Matches = matches,
                GroundVisitedRecords = (int)args[2],
                AllVisitedRecords = (int)args[3],
                GroundIndexedCount = (int)args[4],
                SelectedSlot = (int)args[5],
                SelectedDistance = (int)args[6],
            };
        }

        private static GroundMutationResult MutateGroundRole(
            SimulationWorld world,
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int x,
            int y,
            int z,
            int state)
        {
            MethodInfo method = RequireMethod(
                "AiGroundRoleMutationMatchesBruteForSelfCheck");
            object[] args =
            {
                self, candidate, inputPhase, x, y, z, state, 0, 0, 0,
            };
            bool matches = (bool)method.Invoke(world, args);
            return new GroundMutationResult
            {
                Matches = matches,
                FullRebuildDelta = (int)args[7],
                InPlaceUpdateDelta = (int)args[8],
                MigrationDelta = (int)args[9],
            };
        }

        private static bool InvokeBool(
            SimulationWorld world,
            string methodName,
            params object[] args)
        {
            return (bool)RequireMethod(methodName).Invoke(world, args);
        }

        private static Func<LF2Entity, int, int, int> CreateGroundQueryDelegate(
            SimulationWorld world)
        {
            return (Func<LF2Entity, int, int, int>)RequireMethod(
                    "RunAiGroundNearestQueriesForSelfCheck")
                .CreateDelegate(typeof(Func<LF2Entity, int, int, int>), world);
        }

        private static MethodInfo RequireMethod(string methodName)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            return method;
        }

        private static RuntimeEntityHandle CurrentHandle(
            SimulationWorld world,
            LF2Entity entity)
        {
            Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(
                entity.Runtime.SlotIndex,
                entity,
                out RuntimeEntityHandle handle), Is.True);
            return handle;
        }

        private struct GroundParityResult
        {
            public bool Matches;
            public int GroundVisitedRecords;
            public int AllVisitedRecords;
            public int GroundIndexedCount;
            public int SelectedSlot;
            public int SelectedDistance;
        }

        private struct GroundMutationResult
        {
            public bool Matches;
            public int FullRebuildDelta;
            public int InPlaceUpdateDelta;
            public int MigrationDelta;
        }

        private sealed class GroundNearestController : ILF2Controller
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
