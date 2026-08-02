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
    public sealed class AiMoveModeSnapshotEditorTests
    {
        [Test]
        public void FusedSnapshotIndexBuild_PreservesAllThreeLegacyProducts()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                RegisterCharacter(world, 0, 1, 0, 0, 0, 500);
                RegisterCharacter(world, 1, 1, 10, 0, 0, 300);
                RegisterCharacter(world, 2, 5, 20, 0, 0, 0);
                RegisterCharacter(world, 3, 5, 30, 0, 0, 200);
                LF2Character special100 = RegisterCharacter(
                    world, 20, 2, 40, 0, 0, 500);
                special100.ObjectId = 100;
                LF2Character ordinary = RegisterCharacter(
                    world, 21, 2, 50, 0, 0, 500);
                ordinary.ObjectId = 42;
                LF2Character specialC8 = RegisterCharacter(
                    world, 22, 2, 60, 0, 0, 500);
                specialC8.ObjectId = 0xC8;

                Assert.That(
                    InvokeBool(
                        world,
                        "AiSnapshotIndexProductsMatchLegacyForSelfCheck"),
                    Is.True);
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void TopSelfUsesSecond_AndEqualXKeepsLowerSlot()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character self = RegisterCharacter(
                    world, 0, 1, 500, 0, 0, 500);
                RegisterCharacter(world, 1, 2, 400, 0, 10, 500);
                RegisterCharacter(world, 2, 2, 400, 0, 200, 500);

                MoveModeResult result = CaptureMoveMode(world, self, 1);
                Assert.That(result.Matches, Is.True);
                Assert.That(result.SnapshotValid, Is.True);
                Assert.That(result.TopSlot, Is.EqualTo(0));
                Assert.That(result.SecondSlot, Is.EqualTo(1));
                Assert.That(
                    result.SnapshotMoveMode,
                    Is.EqualTo(result.FullMoveMode));
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void XMinusOneDeadAndNonCharacterCandidatesAreExcluded()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                RegisterCharacter(world, 0, 2, -1, 0, 0, 500);
                RegisterCharacter(world, 1, 2, 300, 0, 0, 0);
                RegisterCharacter(
                    world,
                    2,
                    2,
                    350,
                    0,
                    0,
                    500,
                    false);
                RegisterCharacter(world, 3, 2, 250, 0, 0, 500);
                LF2Character self = RegisterCharacter(
                    world, 9, 1, 500, 0, 0, 500);

                MoveModeResult result = CaptureMoveMode(world, self, 1);
                Assert.That(result.Matches, Is.True);
                Assert.That(result.TopSlot, Is.EqualTo(9));
                Assert.That(result.SecondSlot, Is.EqualTo(3));
                Assert.That(result.SnapshotMoveMode, Is.EqualTo(1));
                Assert.That(result.FullMoveMode, Is.EqualTo(1));
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void XZAndHpMutationsInvalidateSnapshotAndUseFullScan()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                LF2Character candidate = RegisterCharacter(
                    world, 1, 2, 300, 0, 0, 500);
                LF2Character self = RegisterCharacter(
                    world, 9, 1, 800, 0, 0, 500);

                AssertMutation(world, self, candidate, 500, 100, 0);
                AssertMutation(world, self, candidate, 0, 100, 0);
                candidate.Runtime.HP = 500;
                AssertMutation(world, self, candidate, 500, 100, 50);
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void GenerationAndIdentityReuseInvalidateSnapshotAndUseCurrentSlots()
        {
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                var generationWorld = new SimulationWorld();
                LF2Character generationCandidate = RegisterCharacter(
                    generationWorld, 1, 2, 300, 0, 0, 500);
                LF2Character generationSelf = RegisterCharacter(
                    generationWorld, 9, 1, 800, 0, 0, 500);
                AssertIdentityMutation(
                    generationWorld,
                    generationSelf,
                    generationCandidate,
                    null);

                var identityWorld = new SimulationWorld();
                LF2Character identityCandidate = RegisterCharacter(
                    identityWorld, 1, 2, 300, 0, 0, 500);
                LF2Character identitySelf = RegisterCharacter(
                    identityWorld, 9, 1, 800, 0, 0, 500);
                LF2Character replacement = CreateCharacter(
                    1, 2, 100, 0, 30, 500, true);
                AssertIdentityMutation(
                    identityWorld,
                    identitySelf,
                    identityCandidate,
                    replacement);
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        [Test]
        public void WarmedSnapshotQueriesAllocateNoManagedMemory()
        {
            var world = new SimulationWorld();
            bool logging = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            try
            {
                RegisterCharacter(world, 1, 2, 300, 0, 0, 500);
                LF2Character self = RegisterCharacter(
                    world, 9, 1, 800, 0, 0, 500);
                long allocated = (long)RequireMethod(
                        "MeasureAiMoveModeSnapshotAllocationsForSelfCheck")
                    .Invoke(world, new object[] { self, 512 });
                Assert.That(allocated, Is.Zero);
            }
            finally
            {
                Debug.unityLogger.logEnabled = logging;
            }
        }

        private static void AssertMutation(
            SimulationWorld world,
            LF2Entity self,
            LF2Entity candidate,
            int hp,
            int x,
            int z)
        {
            object[] args =
            {
                self,
                candidate,
                hp,
                x,
                z,
                true,
                0,
                0,
            };
            bool matches = (bool)RequireMethod(
                    "AiMoveModeValueMutationFallsBackForSelfCheck")
                .Invoke(world, args);
            Assert.That(matches, Is.True);
            Assert.That((bool)args[5], Is.False);
            Assert.That((int)args[6], Is.EqualTo((int)args[7]));
        }

        private static void AssertIdentityMutation(
            SimulationWorld world,
            LF2Entity self,
            LF2Entity candidate,
            LF2Entity replacement)
        {
            object[] args =
            {
                self,
                candidate,
                replacement,
                true,
                0,
                0,
            };
            bool matches = (bool)RequireMethod(
                    "AiMoveModeIdentityMutationFallsBackForSelfCheck")
                .Invoke(world, args);
            Assert.That(matches, Is.True);
            Assert.That((bool)args[3], Is.False);
            Assert.That((int)args[4], Is.EqualTo((int)args[5]));
        }

        private static MoveModeResult CaptureMoveMode(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase)
        {
            object[] args =
            {
                self,
                inputPhase,
                false,
                -1,
                -1,
                0,
                0,
            };
            bool matches = (bool)RequireMethod(
                    "AiMoveModeSnapshotMatchesFullForSelfCheck")
                .Invoke(world, args);
            return new MoveModeResult
            {
                Matches = matches,
                SnapshotValid = (bool)args[2],
                TopSlot = (int)args[3],
                SecondSlot = (int)args[4],
                SnapshotMoveMode = (int)args[5],
                FullMoveMode = (int)args[6],
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
            int hp,
            bool character = true)
        {
            LF2Character entity = CreateCharacter(
                slot,
                team,
                x,
                y,
                z,
                hp,
                character);
            world.Register(entity);
            return entity;
        }

        private static LF2Character CreateCharacter(
            int runtimeSlot,
            int team,
            int x,
            int y,
            int z,
            int hp,
            bool character)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            var data = new LF2CharacterData
            {
                name = $"AiMoveMode_{runtimeSlot}",
                type_sub = 1,
                frames = new List<LF2FrameData> { frame },
            };
            LF2Character entity = character
                ? new LF2Character()
                : new NonCharacterEntity();
            entity.ModuleInitialize();
            entity.Name = data.name;
            entity.ObjectId = 1;
            entity.Controller = new EmptyController();
            entity.FrameCache.Load(new LF2CharacterDataWrapper(1, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.PN = 0;
            entity.Frame.N = 0;
            entity.Initialize(500, 500);
            entity.FrameDelay = 0;
            entity.SetRequiredRuntimeSlot(runtimeSlot);
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Runtime.HP = hp;
            entity.Runtime.SetPosition(x, y, z);
            entity.Runtime.SyncIntegerPosition();
            return entity;
        }

        private struct MoveModeResult
        {
            public bool Matches;
            public bool SnapshotValid;
            public int TopSlot;
            public int SecondSlot;
            public int SnapshotMoveMode;
            public int FullMoveMode;
        }

        private sealed class NonCharacterEntity : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.HeavyWeapon;
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
