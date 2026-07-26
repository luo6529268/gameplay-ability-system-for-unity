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
    public sealed class TeamPartitionGroundNearestEditorTests
    {
        [Test]
        [Timeout(30000)]
        public void TwoTeamThousandEntities_ReducesVisitedRecordsAndMatchesAuthority()
        {
            const int count = 1000;
            var world = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended,
                1100);
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(
                    world,
                    0,
                    1,
                    0,
                    0,
                    0,
                    0);
                for (int slot = 1; slot < count; slot++)
                {
                    bool enemy = slot >= count / 2;
                    RegisterCharacter(
                        world,
                        slot,
                        enemy ? 2 : 1,
                        enemy ? 5000 + slot - count / 2 : slot,
                        0,
                        slot % 11,
                        0);
                }

                TeamPartitionResult result = CapturePartitionParity(
                    world,
                    self,
                    0);
                Assert.That(result.Matches, Is.True);
                Assert.That(result.AllowedPartitionCount, Is.EqualTo(1));
                Assert.That(result.PartitionHandled, Is.True);
                Assert.That(
                    result.PartitionVisitedRecords,
                    Is.LessThan(result.GroundVisitedRecords));
                Assert.That(
                    result.PartitionVisitedRecords * 4,
                    Is.LessThan(result.GroundVisitedRecords),
                    $"partition={result.PartitionVisitedRecords} " +
                    $"ground={result.GroundVisitedRecords}");
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void TeamsOneTwoFiveSeven_PreserveAuthorityPhaseRules()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character teamOneSelf = RegisterCharacter(
                    world, 0, 1, 0, 0, 0, 0);
                RegisterCharacter(world, 1, 2, 40, 0, 0, 0);
                RegisterCharacter(world, 2, 5, 20, 0, 0, 0);
                RegisterCharacter(world, 3, 7, 60, 0, 0, 0);
                LF2Character teamFiveSelf = RegisterCharacter(
                    world, 4, 5, -20, 0, 0, 0);

                AssertDecision(world, teamOneSelf, 0, 3, false);
                AssertDecision(world, teamOneSelf, 1, 1, false);
                AssertDecision(world, teamOneSelf, 4, 3, false);
                AssertDecision(world, teamFiveSelf, 1, 3, false);
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void TeamAndPositionMutations_UseTheirRequiredExactFallbacks()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(
                    world, 0, 1, 0, 0, 0, 0);
                LF2Character candidate = RegisterCharacter(
                    world, 1, 2, 60, 0, 0, 0);
                RegisterCharacter(world, 2, 1, 10, 0, 0, 0);

                Assert.That(
                    InvokeBool(
                        world,
                        "AiGroundTeamPartitionMutationFallbackForSelfCheck",
                        self,
                        candidate,
                        0,
                        7,
                        60),
                    Is.True,
                    "A pure team mutation must invalidate only the team partition.");
                Assert.That(
                    InvokeBool(
                        world,
                        "AiGroundTeamPartitionMutationFallbackForSelfCheck",
                        self,
                        candidate,
                        0,
                        7,
                        -30),
                    Is.True,
                    "A position mutation must invalidate every stale spatial index.");
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void Phase1TeamFiveMembershipMutations_InvalidateIndexedShortcut()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(
                    world, 0, 1, 0, 0, 0, 0);
                LF2Character candidate = RegisterCharacter(
                    world, 1, 2, 10, 0, 0, 0);
                RegisterCharacter(world, 2, 5, 100, 0, 0, 0);

                Phase1MutationResult entered = CapturePhase1Mutation(
                    world,
                    self,
                    candidate,
                    5);
                Assert.That(entered.Matches, Is.True);
                Assert.That(entered.Phase1ListValid, Is.False);
                Assert.That(entered.SelectedSlot, Is.EqualTo(1));

                Phase1MutationResult exited = CapturePhase1Mutation(
                    world,
                    self,
                    candidate,
                    2);
                Assert.That(exited.Matches, Is.True);
                Assert.That(exited.Phase1ListValid, Is.False);
                Assert.That(exited.SelectedSlot, Is.EqualTo(2));
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void WarmedSinglePartitionQueries_AllocateNoManagedMemory()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(
                    world, 0, 1, 0, 0, 0, 0);
                for (int slot = 1; slot < 160; slot++)
                {
                    RegisterCharacter(
                        world,
                        slot,
                        slot < 80 ? 1 : 2,
                        slot * 20,
                        0,
                        slot % 9,
                        0);
                }

                long allocated = (long)RequireMethod(
                        "MeasureAiGroundTeamPartitionAllocationsForSelfCheck")
                    .Invoke(world, new object[] { self, 0, 512 });
                Assert.That(allocated, Is.Zero);
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        private static void AssertDecision(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase,
            int expectedAllowedPartitions,
            bool expectedHandled)
        {
            TeamPartitionResult result = CapturePartitionParity(
                world,
                self,
                inputPhase);
            Assert.That(result.Matches, Is.True, $"phase={inputPhase}");
            Assert.That(
                result.AllowedPartitionCount,
                Is.EqualTo(expectedAllowedPartitions),
                $"phase={inputPhase}");
            Assert.That(
                result.PartitionHandled,
                Is.EqualTo(expectedHandled),
                $"phase={inputPhase}");
        }

        private static TeamPartitionResult CapturePartitionParity(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase)
        {
            object[] args =
            {
                self,
                inputPhase,
                -1,
                false,
                0,
                0,
                -1,
                10000,
            };
            bool matches = (bool)RequireMethod(
                    "AiGroundTeamPartitionMatchesBruteForSelfCheck")
                .Invoke(world, args);
            return new TeamPartitionResult
            {
                Matches = matches,
                AllowedPartitionCount = (int)args[2],
                PartitionHandled = (bool)args[3],
                PartitionVisitedRecords = (int)args[4],
                GroundVisitedRecords = (int)args[5],
                SelectedSlot = (int)args[6],
                SelectedDistance = (int)args[7],
            };
        }

        private static Phase1MutationResult CapturePhase1Mutation(
            SimulationWorld world,
            LF2Entity self,
            LF2Entity candidate,
            int candidateTeam)
        {
            object[] args =
            {
                self,
                candidate,
                candidateTeam,
                true,
                -1,
            };
            bool matches = (bool)RequireMethod(
                    "AiPhase1TeamMutationMatchesBruteForSelfCheck")
                .Invoke(world, args);
            return new Phase1MutationResult
            {
                Matches = matches,
                Phase1ListValid = (bool)args[3],
                SelectedSlot = (int)args[4],
            };
        }

        private static bool InvokeBool(
            SimulationWorld world,
            string methodName,
            params object[] args)
        {
            return (bool)RequireMethod(methodName).Invoke(world, args);
        }

        private static MethodInfo RequireMethod(string methodName)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            return method;
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
                $"GroundTeamPartition_{slot}",
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

        private struct TeamPartitionResult
        {
            public bool Matches;
            public int AllowedPartitionCount;
            public bool PartitionHandled;
            public int PartitionVisitedRecords;
            public int GroundVisitedRecords;
            public int SelectedSlot;
            public int SelectedDistance;
        }

        private struct Phase1MutationResult
        {
            public bool Matches;
            public bool Phase1ListValid;
            public int SelectedSlot;
        }

        private sealed class GroundNearestController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } =
                new SimInputBuffer();
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
