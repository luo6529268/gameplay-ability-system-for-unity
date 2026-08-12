#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleEcsEmptyCharacterHitConsumeEditorTests
    {
        [Test]
        public void EmptyCandidateExactCharacter_SkipsResolverAndMatchesLegacy()
        {
            using var logging = new DisabledLoggingScope();
            Scenario fast = CreateScenario(forceLegacy: false);
            Scenario legacy = CreateScenario(forceLegacy: true);

            PrepareEmptyCandidateSnapshot(fast.World);
            PrepareEmptyCandidateSnapshot(legacy.World);
            fast.World.PostInteractionTickAll(17);
            legacy.World.PostInteractionTickAll(17);

            Assert.That(
                fast.World.LastEmptyCharacterHitConsumeSkipCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                fast.World.LastCharacterHitConsumeExecutedCountForDiagnostics,
                Is.Zero);
            Assert.That(
                fast.World.LastCharacterRuntimeCandidateCountGateAppliedForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                legacy.World.LastEmptyCharacterHitConsumeSkipCountForDiagnostics,
                Is.Zero);
            Assert.That(
                legacy.World.LastCharacterHitConsumeExecutedCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(fast.Character.Frame.N, Is.EqualTo(legacy.Character.Frame.N));
            Assert.That(fast.Character.Runtime.Frame, Is.EqualTo(legacy.Character.Runtime.Frame));
            Assert.That(
                fast.World.CaptureExtendedChecksumSnapshot(17).OverallChecksum,
                Is.EqualTo(
                    legacy.World
                        .CaptureExtendedChecksumSnapshot(17)
                        .OverallChecksum));
        }

        [Test]
        public void UnavailableCandidateSnapshot_FailsClosedToLegacyResolver()
        {
            using var logging = new DisabledLoggingScope();
            Scenario scenario = CreateScenario(forceLegacy: false);

            scenario.World.PostInteractionTickAll(18);

            Assert.That(
                scenario.World.LastEmptyCharacterHitConsumeSkipCountForDiagnostics,
                Is.Zero);
            Assert.That(
                scenario.World.LastCharacterHitConsumeExecutedCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void StalePublishedSnapshot_FailsClosedAndRefreshesRuntime()
        {
            using var logging = new DisabledLoggingScope();
            Scenario scenario = CreateScenario(forceLegacy: false);
            PrepareEmptyCandidateSnapshot(scenario.World);
            scenario.Character.Frame.N = 7;
            scenario.Character.Runtime.Frame = 0;

            scenario.World.PostInteractionTickAll(19);

            Assert.That(
                scenario.World.LastEmptyCharacterHitConsumeSkipCountForDiagnostics,
                Is.Zero);
            Assert.That(
                scenario.World.LastCharacterHitConsumeExecutedCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(scenario.Character.Runtime.Frame, Is.EqualTo(7));
        }

        [Test]
        public void RuntimeCandidateCountMismatch_FailsClosedToRangeProof()
        {
            using var logging = new DisabledLoggingScope();
            Scenario scenario = CreateScenario(forceLegacy: false);
            PrepareEmptyCandidateSnapshot(scenario.World);
            scenario.Character.Runtime.HitCandidateCount = 1;

            scenario.World.PostInteractionTickAll(21);

            Assert.That(
                scenario.World.LastCharacterRuntimeCandidateCountGateAppliedForDiagnostics,
                Is.Zero);
            Assert.That(
                scenario.World.LastCharacterRuntimeCandidateCountGateFallbackForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                scenario.World.LastEmptyCharacterHitConsumeSkipCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                scenario.World.LastCharacterHitConsumeExecutedCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void ExplicitRuntimeCountGateOff_PreservesExistingRangeProof()
        {
            using var logging = new DisabledLoggingScope();
            Scenario scenario = CreateScenario(forceLegacy: false);
            scenario.World.ForceLegacyCharacterRuntimeCandidateCountGateForDiagnostics = true;
            PrepareEmptyCandidateSnapshot(scenario.World);

            scenario.World.PostInteractionTickAll(22);

            Assert.That(
                scenario.World.LastCharacterRuntimeCandidateCountGateAppliedForDiagnostics,
                Is.Zero);
            Assert.That(
                scenario.World.LastEmptyCharacterHitConsumeSkipCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                scenario.World.LastCharacterHitConsumeExecutedCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void DerivedCharacter_FailsClosedAndPreservesVirtualDispatch()
        {
            using var logging = new DisabledLoggingScope();
            var world = CreateWorld(forceLegacy: false);
            ProbeCharacter character = CreateCharacter<ProbeCharacter>(world, 1001);
            PrepareEmptyCandidateSnapshot(world);

            world.PostInteractionTickAll(20);

            Assert.That(
                world.LastEmptyCharacterHitConsumeSkipCountForDiagnostics,
                Is.Zero);
            Assert.That(
                world.LastCharacterHitConsumeExecutedCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(character.PostInteractionCount, Is.EqualTo(1));
        }

        [Test]
        public void WarmedEmptyCandidateFastPath_AllocatesNoManagedMemory()
        {
            using var logging = new DisabledLoggingScope();
            Scenario scenario = CreateScenario(forceLegacy: false);
            PrepareEmptyCandidateSnapshot(scenario.World);

            for (int i = 0; i < 32; i++)
                scenario.World.PostInteractionTickAll(100 + i);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 512; i++)
                scenario.World.PostInteractionTickAll(200 + i);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
            Assert.That(
                scenario.World.LastEmptyCharacterHitConsumeSkipCountForDiagnostics,
                Is.EqualTo(1));
        }

        private static Scenario CreateScenario(bool forceLegacy)
        {
            SimulationWorld world = CreateWorld(forceLegacy);
            LF2Character character = CreateCharacter<LF2Character>(world, 1000);
            return new Scenario(world, character);
        }

        private static SimulationWorld CreateWorld(bool forceLegacy)
        {
            return new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity)
            {
                ForceLegacyEmptyCharacterHitConsumeForDiagnostics = forceLegacy,
                ForceLegacyCharacterRuntimeCandidateCountGateForDiagnostics =
                    forceLegacy,
            };
        }

        private static TCharacter CreateCharacter<TCharacter>(
            SimulationWorld world,
            int objectId)
            where TCharacter : LF2Character, new()
        {
            LF2FrameData frame0 = Frame(0);
            LF2FrameData frame7 = Frame(7);
            var data = new LF2CharacterData
            {
                name = typeof(TCharacter).Name + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame0, frame7 },
            };
            var character = new TCharacter();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(
                new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Frame.Prev = 0;
            character.Initialize(500, 500);
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Frame.Prev = 0;
            character.Frame.Prev2 = 0;
            character.Frame.Prev2D = character.Frame.D;
            character.Runtime.SetPosition(objectId * 2.0, 0.0, 200.0);
            character.Runtime.SyncIntegerPosition();
            character.RefreshRuntimeSnapshot();
            world.Register(character);
            return character;
        }

        private static LF2FrameData Frame(int frameId)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = LF2States.Standing,
                wait = 100,
                next = frameId,
                centerx = 39,
                centery = 79,
            };
        }

        private static void PrepareEmptyCandidateSnapshot(SimulationWorld world)
        {
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
        }

        private sealed class ProbeCharacter : LF2Character
        {
            public int PostInteractionCount { get; private set; }

            public override void SimPostInteraction(int tickIndex)
            {
                PostInteractionCount++;
                base.SimPostInteraction(tickIndex);
            }
        }

        private readonly struct Scenario
        {
            internal Scenario(SimulationWorld world, LF2Character character)
            {
                World = world;
                Character = character;
            }

            internal SimulationWorld World { get; }
            internal LF2Character Character { get; }
        }

        private sealed class DisabledLoggingScope : IDisposable
        {
            private readonly bool previous;

            public DisabledLoggingScope()
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
