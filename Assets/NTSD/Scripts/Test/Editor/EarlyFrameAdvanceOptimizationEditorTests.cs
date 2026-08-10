#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class EarlyFrameAdvanceOptimizationEditorTests
    {
        private static readonly FieldInfo[] RuntimeFields =
            typeof(NTSDEntityRuntime)
                .GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                .Where(field => !field.IsNotSerialized)
                .OrderBy(field => field.Name, StringComparer.Ordinal)
                .ToArray();
        [Test]
        public void NeutralExactCharacters_SkipSnapshotsAndMatchForcedLegacy()
        {
            using var logging = new DisabledLoggingScope();
            Scenario fast = CreateNeutralScenario(32, forceLegacy: false);
            Scenario legacy = CreateNeutralScenario(32, forceLegacy: true);

            fast.World.EarlyFrameAdvanceSpecialsAll(1);
            legacy.World.EarlyFrameAdvanceSpecialsAll(1);

            AssertEquivalent(fast, legacy, 1);
            Assert.That(
                fast.World.LastEarlyTeleportSnapshotSkipCountForDiagnostics,
                Is.EqualTo(32));
            Assert.That(
                fast.World.LastEarlyTeleportRefreshCountForDiagnostics,
                Is.Zero);
            Assert.That(
                fast.World.LastEarlyStateHandlePathUsedForDiagnostics,
                Is.True);
            Assert.That(
                legacy.World.LastEarlyTeleportRefreshCountForDiagnostics,
                Is.EqualTo(32));
        }

        [Test]
        public void ToggleGateAndTeleportTieSelection_MatchForcedLegacyAndRng()
        {
            using var logging = new DisabledLoggingScope();
            Scenario fast = CreateTeleportScenario(forceLegacy: false);
            Scenario legacy = CreateTeleportScenario(forceLegacy: true);
            uint fastRngState = fast.World.Rng.State;
            ulong fastRngCalls = fast.World.Rng.CallCount;

            fast.World.EarlyFrameAdvanceSpecialsAll(2);
            legacy.World.EarlyFrameAdvanceSpecialsAll(2);

            AssertEquivalent(fast, legacy, 2);
            Assert.That(fast.Entities[0].Runtime.XInt, Is.EqualTo(-20));
            Assert.That(fast.World.Rng.State, Is.EqualTo(fastRngState));
            Assert.That(fast.World.Rng.CallCount, Is.EqualTo(fastRngCalls));
            Assert.That(
                fast.World.LastEarlyTeleportRefreshCountForDiagnostics,
                Is.EqualTo(1));

            Scenario teammateFast =
                CreateTeleportTeammateScenario(forceLegacy: false);
            Scenario teammateLegacy =
                CreateTeleportTeammateScenario(forceLegacy: true);
            teammateFast.World.EarlyFrameAdvanceSpecialsAll(2);
            teammateLegacy.World.EarlyFrameAdvanceSpecialsAll(2);
            AssertEquivalent(teammateFast, teammateLegacy, 2);
            Assert.That(
                teammateFast.Entities[0].Runtime.XInt,
                Is.EqualTo(140));

            Scenario gatedFast =
                CreateSingleTeleportScenario(forceLegacy: false);
            Scenario gatedLegacy =
                CreateSingleTeleportScenario(forceLegacy: true);
            gatedFast.World.AdvanceBattleFlowTick(1);
            gatedLegacy.World.AdvanceBattleFlowTick(1);
            gatedFast.World.EarlyFrameAdvanceSpecialsAll(1);
            gatedLegacy.World.EarlyFrameAdvanceSpecialsAll(1);

            AssertEquivalent(gatedFast, gatedLegacy, 1);
            Assert.That(gatedFast.Entities[0].Runtime.XInt, Is.Zero);
            Assert.That(
                gatedFast.World
                    .LastEarlyTeleportSnapshotSkipCountForDiagnostics,
                Is.EqualTo(2));
        }

        [Test]
        public void State500Branches_MatchForcedLegacy()
        {
            using var logging = new DisabledLoggingScope();
            Scenario fast = CreateState500Scenario(forceLegacy: false);
            Scenario legacy = CreateState500Scenario(forceLegacy: true);

            fast.World.EarlyFrameAdvanceSpecialsAll(3);
            legacy.World.EarlyFrameAdvanceSpecialsAll(3);

            AssertEquivalent(fast, legacy, 3);
            Assert.That(fast.Entities[0].Frame.N, Is.Zero);
            Assert.That(fast.Entities[1].Frame.N, Is.EqualTo(10));
            Assert.That(
                fast.World.LastEarlyStateHandlePathUsedForDiagnostics,
                Is.True);
            Assert.That(
                fast.World.LastEarlyStateHandleFallbackCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void State501OwnerChildrenDeadAndMissingReplacement_MatchLegacy()
        {
            using var logging = new DisabledLoggingScope();
            const int replacementOid = 9000;
            LF2CharacterDataWrapper replacement =
                Wrapper(replacementOid, LF2ObjectType.Other, 0);
            var runtimeCharacterConfigs = new RuntimeCharacterConfigResolver(
                oid => oid == replacementOid ? replacement : null);
            Scenario fast =
                CreateState501Scenario(
                    replacementOid,
                    forceLegacy: false,
                    runtimeCharacterConfigs);
            Scenario legacy =
                CreateState501Scenario(
                    replacementOid,
                    forceLegacy: true,
                    runtimeCharacterConfigs);

            fast.World.EarlyFrameAdvanceSpecialsAll(4);
            legacy.World.EarlyFrameAdvanceSpecialsAll(4);

            AssertEquivalent(fast, legacy, 4);
            Assert.That(fast.Entities[1].ObjectId, Is.EqualTo(replacementOid));
            Assert.That(fast.Entities[0].ObjectId, Is.EqualTo(replacementOid));
            Assert.That(fast.Entities[2].ObjectId, Is.EqualTo(replacementOid));
            Assert.That(fast.Entities[0].Frame.N, Is.EqualTo(212));
            Assert.That(fast.Entities[2].Frame.N, Is.Zero);
            Assert.That(fast.Entities[3].ObjectId, Is.Not.EqualTo(replacementOid));
            Assert.That(fast.Entities[4].Frame.D.state, Is.EqualTo(501));
        }

        [Test]
        public void ReusedState500Slot_UsesCurrentGeneration()
        {
            using var logging = new DisabledLoggingScope();
            var world = CreateWorld(forceLegacy: false);
            LF2Character original = CreateCharacter(
                world,
                51,
                500,
                frameId: 10);
            int slot = original.Runtime.SlotIndex;
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    slot,
                    original,
                    out RuntimeEntityHandle oldHandle),
                Is.True);
            world.Unregister(original);

            LF2Character replacement = CreateCharacter(
                world,
                52,
                500,
                frameId: 10);
            replacement.TransformTargetObjectId = -1;
            Assert.That(replacement.Runtime.SlotIndex, Is.EqualTo(slot));
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    slot,
                    replacement,
                    out RuntimeEntityHandle currentHandle),
                Is.True);
            Assert.That(
                currentHandle.Generation,
                Is.Not.EqualTo(oldHandle.Generation));

            world.EarlyFrameAdvanceSpecialsAll(5);

            Assert.That(replacement.Frame.N, Is.Zero);
            Assert.That(
                world.LastEarlyStateHandlePathUsedForDiagnostics,
                Is.True);
        }

        [Test]
        public void WarmedNeutralFastPath_AllocatesNoManagedMemory()
        {
            using var logging = new DisabledLoggingScope();
            Scenario fast = CreateNeutralScenario(64, forceLegacy: false);
            for (int i = 0; i < 32; i++)
                fast.World.EarlyFrameAdvanceSpecialsAll(10 + i);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 512; i++)
                fast.World.EarlyFrameAdvanceSpecialsAll(100 + i);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
            Assert.That(
                fast.World.LastEarlyTeleportSnapshotSkipCountForDiagnostics,
                Is.EqualTo(64));
        }

        private static Scenario CreateNeutralScenario(
            int count,
            bool forceLegacy)
        {
            SimulationWorld world = CreateWorld(forceLegacy);
            var entities = new List<LF2Character>(count);
            for (int i = 0; i < count; i++)
            {
                entities.Add(
                    CreateCharacter(world, 1000 + i, 0));
            }

            return new Scenario(world, entities);
        }

        private static Scenario CreateTeleportScenario(bool forceLegacy)
        {
            SimulationWorld world = CreateWorld(forceLegacy);
            var entities = new List<LF2Character>
            {
                CreateCharacter(world, 1, LF2States.TeleportToEnemy, 0, 1, 0, 0),
                CreateCharacter(world, 2, 0, 0, 2, 100, 0),
                CreateCharacter(world, 3, 0, 0, 2, -100, 0),
            };
            return new Scenario(world, entities);
        }

        private static Scenario CreateTeleportTeammateScenario(
            bool forceLegacy)
        {
            SimulationWorld world = CreateWorld(forceLegacy);
            return new Scenario(
                world,
                new List<LF2Character>
                {
                    CreateCharacter(
                        world,
                        4,
                        LF2States.TeleportToTeammate,
                        0,
                        3,
                        0,
                        0),
                    CreateCharacter(world, 5, 0, 0, 3, 200, 0),
                    CreateCharacter(world, 6, 0, 0, 3, -200, 0),
                });
        }

        private static Scenario CreateSingleTeleportScenario(bool forceLegacy)
        {
            SimulationWorld world = CreateWorld(forceLegacy);
            return new Scenario(
                world,
                new List<LF2Character>
                {
                    CreateCharacter(
                        world,
                        11,
                        LF2States.TeleportToEnemy,
                        0,
                        1,
                        0,
                        0),
                    CreateCharacter(world, 12, 0, 0, 2, 100, 0),
                });
        }

        private static Scenario CreateState500Scenario(bool forceLegacy)
        {
            SimulationWorld world = CreateWorld(forceLegacy);
            LF2Character reset = CreateCharacter(world, 21, 500, 10);
            reset.TransformTargetObjectId = -1;
            LF2Character remain = CreateCharacter(world, 22, 500, 10);
            remain.TransformTargetObjectId = 8000;
            remain.TransformOriginalObjectId = -1;
            return new Scenario(
                world,
                new List<LF2Character> { reset, remain });
        }

        private static Scenario CreateState501Scenario(
            int replacementOid,
            bool forceLegacy,
            RuntimeCharacterConfigResolver runtimeCharacterConfigs)
        {
            SimulationWorld world = CreateWorld(
                forceLegacy,
                runtimeCharacterConfigs);
            LF2Character childBefore =
                CreateCharacter(world, 31, 0, 0, 1, 0, 0);
            LF2Character owner =
                CreateCharacter(world, 32, 501, 10, 1, 0, 0);
            LF2Character childAfter =
                CreateCharacter(world, 33, 0, 0, 1, 0, 0);
            LF2Character deadChild =
                CreateCharacter(world, 34, 0, 0, 1, 0, 0);
            LF2Character missing =
                CreateCharacter(world, 35, 501, 10, 1, 0, 0);

            owner.TransformTargetObjectId = replacementOid;
            missing.TransformTargetObjectId = replacementOid + 1;
            int ownerSlot = owner.Runtime.SlotIndex;
            childBefore.KillCount = ownerSlot;
            childAfter.KillCount = ownerSlot;
            deadChild.KillCount = ownerSlot;
            childBefore.Runtime.Y = -1.0;
            childBefore.Runtime.YInt = -1;
            childAfter.Runtime.Y = 0.5;
            childAfter.Runtime.YInt = 0;
            deadChild.Health.HP = 0;
            deadChild.Runtime.HP = 0;

            return new Scenario(
                world,
                new List<LF2Character>
                {
                    childBefore,
                    owner,
                    childAfter,
                    deadChild,
                    missing,
                });
        }

        private static SimulationWorld CreateWorld(
            bool forceLegacy,
            RuntimeCharacterConfigResolver runtimeCharacterConfigs = null)
        {
            return new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                CollisionBroadphaseBackend.BruteForce,
                runtimeCharacterConfigs)
            {
                ForceLegacyEarlyFrameAdvanceForDiagnostics = forceLegacy,
            };
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int objectId,
            int state,
            int frameId = 0,
            int team = 1,
            int x = 0,
            int z = 0)
        {
            LF2CharacterDataWrapper wrapper =
                Wrapper(objectId, LF2ObjectType.Character, state, frameId);
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = "Early" + objectId;
            character.ObjectId = objectId;
            character.Team = team;
            character.RelationTeam = team;
            character.FrameCache.Load(wrapper);
            character.Frame.D =
                character.FrameCache.GetFrameDataById(frameId);
            character.Frame.N = frameId;
            character.Frame.PN = frameId;
            character.Frame.Prev = frameId;
            character.Initialize(500, 500);
            character.Frame.D =
                character.FrameCache.GetFrameDataById(frameId);
            character.Frame.N = frameId;
            character.Frame.PN = frameId;
            character.Frame.Prev = frameId;
            character.Frame.Prev2 = frameId;
            character.Frame.Prev2D = character.Frame.D;
            character.Runtime.SetPosition(x, 0, z);
            character.Runtime.SyncIntegerPosition();
            character.RefreshRuntimeSnapshot();
            world.Register(character);
            return character;
        }

        private static LF2CharacterDataWrapper Wrapper(
            int objectId,
            LF2ObjectType objectType,
            int state,
            int frameId = 0)
        {
            var frames = new List<LF2FrameData>
            {
                new LF2FrameData
                {
                    frameId = frameId,
                    state = state,
                    wait = 100,
                    next = frameId,
                    centerx = 39,
                    centery = 79,
                },
            };
            if (frameId != 0)
            {
                frames.Add(
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = 0,
                        wait = 100,
                        next = 0,
                        centerx = 39,
                        centery = 79,
                    });
            }
            if (frameId != 212)
            {
                frames.Add(
                    new LF2FrameData
                    {
                        frameId = 212,
                        state = LF2States.Jump,
                        wait = 100,
                        next = 212,
                        centerx = 39,
                        centery = 79,
                    });
            }

            return new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = "Early" + objectId,
                    type_sub = (int)objectType,
                    frames = frames,
                });
        }

        private static void AssertEquivalent(
            Scenario fast,
            Scenario legacy,
            int tickIndex)
        {
            Assert.That(
                fast.Entities.Count,
                Is.EqualTo(legacy.Entities.Count));
            for (int i = 0; i < fast.Entities.Count; i++)
            {
                Assert.That(
                    fast.Entities[i].ObjectId,
                    Is.EqualTo(legacy.Entities[i].ObjectId));
                Assert.That(
                    fast.Entities[i].Frame.N,
                    Is.EqualTo(legacy.Entities[i].Frame.N));
                AssertRuntimeEquivalent(
                    fast.Entities[i].Runtime,
                    legacy.Entities[i].Runtime);
            }

            Assert.That(
                fast.World.Rng.State,
                Is.EqualTo(legacy.World.Rng.State));
            Assert.That(
                fast.World.Rng.CallCount,
                Is.EqualTo(legacy.World.Rng.CallCount));
            Assert.That(
                fast.World
                    .CaptureExtendedChecksumSnapshot(tickIndex)
                    .OverallChecksum,
                Is.EqualTo(
                    legacy.World
                        .CaptureExtendedChecksumSnapshot(tickIndex)
                        .OverallChecksum));
        }

        private static void AssertRuntimeEquivalent(
            NTSDEntityRuntime fast,
            NTSDEntityRuntime legacy)
        {
            foreach (FieldInfo field in RuntimeFields)
            {
                object fastValue = field.GetValue(fast);
                object legacyValue = field.GetValue(legacy);
                if (fastValue is IEnumerable fastEnumerable &&
                    fastValue is not string &&
                    legacyValue is IEnumerable legacyEnumerable)
                {
                    CollectionAssert.AreEqual(
                        fastEnumerable,
                        legacyEnumerable,
                        $"runtime field {field.Name}");
                    continue;
                }

                Assert.That(
                    fastValue,
                    Is.EqualTo(legacyValue),
                    $"runtime field {field.Name}");
            }
        }

        private readonly struct Scenario
        {
            internal Scenario(
                SimulationWorld world,
                List<LF2Character> entities)
            {
                World = world;
                Entities = entities;
            }

            internal SimulationWorld World { get; }
            internal List<LF2Character> Entities { get; }
        }

        private sealed class DisabledLoggingScope : IDisposable
        {
            private readonly bool previous;

            internal DisabledLoggingScope()
            {
                previous = Debug.unityLogger.logEnabled;
                Debug.unityLogger.logEnabled = false;
            }

            public void Dispose()
            {
                Debug.unityLogger.logEnabled = previous;
            }
        }
    }
}
#endif
