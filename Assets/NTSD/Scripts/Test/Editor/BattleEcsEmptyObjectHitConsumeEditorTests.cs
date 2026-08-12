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
    public sealed class BattleEcsEmptyObjectHitConsumeEditorTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void CharacterCandidateRows_DoNotBlockEmptyObjectPass(
            bool storeAuthority)
        {
            using var logging = new DisabledLoggingScope();
            var world = CreateWorld(forceLegacy: false);
            LF2Character attacker = CreateCharacter(
                world,
                1100,
                LF2ObjectType.Character,
                CreateAttackFrame());
            CreateCharacter(
                world,
                1101,
                LF2ObjectType.Character,
                CreateBodyFrame());
            ConfigureStoreAuthority(world, storeAuthority);
            PrepareCandidateSnapshot(world);

            world.ObjectInteractionTickAll(30);

            Assert.That(
                world.LastEmptyObjectHitConsumeSkipCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(
                world.LastObjectHitConsumeExecutedCountForDiagnostics,
                Is.Zero);
            Assert.That(attacker.Runtime.HitCandidateCount, Is.GreaterThan(0));
            world.EndCollisionCandidateConsumption();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CurrentDatTransformToObject_WithCandidate_FailsClosedAndExecutes(
            bool storeAuthority)
        {
            using var logging = new DisabledLoggingScope();
            var world = CreateWorld(forceLegacy: false);
            LF2FrameData attackFrame = CreateAttackFrame();
            MutableTypeCharacter attacker = CreateCharacter<MutableTypeCharacter>(
                world,
                1200,
                LF2ObjectType.Character,
                attackFrame);
            CreateCharacter(
                world,
                1201,
                LF2ObjectType.Character,
                CreateBodyFrame());
            ConfigureStoreAuthority(world, storeAuthority);
            PrepareCandidateSnapshot(world);
            Assert.That(attacker.Runtime.HitCandidateCount, Is.GreaterThan(0));

            attacker.CurrentDataObjectType = LF2ObjectType.SpecialAttack;
            Assert.That(
                ((BruteForceSceneQuery)world.SceneQuery)
                    .TryProveNoObjectInteractionCandidatesForCurrentTick(),
                Is.False);
            world.ObjectInteractionTickAll(31);

            Assert.That(
                world.LastEmptyObjectHitConsumeSkipCountForDiagnostics,
                Is.Zero);
            Assert.That(
                world.LastObjectHitConsumeExecutedCountForDiagnostics,
                Is.EqualTo(1));
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void UnavailableCandidateSnapshot_FailsClosedToVirtualObjectInteraction()
        {
            using var logging = new DisabledLoggingScope();
            var world = CreateWorld(forceLegacy: false);
            ProbeCharacter probe = CreateCharacter<ProbeCharacter>(
                world,
                1300,
                LF2ObjectType.SpecialAttack,
                CreateEmptyFrame());

            world.ObjectInteractionTickAll(32);

            Assert.That(
                world.LastEmptyObjectHitConsumeSkipCountForDiagnostics,
                Is.Zero);
            Assert.That(
                world.LastObjectHitConsumeExecutedCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(probe.ObjectInteractionCount, Is.EqualTo(1));
        }

        [Test]
        public void ForceLegacy_DisablesWholePassProof()
        {
            using var logging = new DisabledLoggingScope();
            var world = CreateWorld(forceLegacy: true);
            CreateCharacter(
                world,
                1400,
                LF2ObjectType.Character,
                CreateEmptyFrame());
            PrepareCandidateSnapshot(world);

            world.ObjectInteractionTickAll(33);

            Assert.That(
                world.LastEmptyObjectHitConsumeSkipCountForDiagnostics,
                Is.Zero);
            Assert.That(
                world.LastObjectHitConsumeExecutedCountForDiagnostics,
                Is.Zero);
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void WarmedEmptyObjectPassProof_AllocatesNoManagedMemory()
        {
            using var logging = new DisabledLoggingScope();
            var world = CreateWorld(forceLegacy: false);
            for (int index = 0; index < 64; index++)
            {
                CreateCharacter(
                    world,
                    1500 + index,
                    LF2ObjectType.Character,
                    CreateEmptyFrame());
            }
            PrepareCandidateSnapshot(world);

            for (int index = 0; index < 32; index++)
                world.ObjectInteractionTickAll(100 + index);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 512; index++)
                world.ObjectInteractionTickAll(200 + index);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
            Assert.That(
                world.LastEmptyObjectHitConsumeSkipCountForDiagnostics,
                Is.EqualTo(1));
            world.EndCollisionCandidateConsumption();
        }

        private static SimulationWorld CreateWorld(bool forceLegacy)
        {
            return new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity)
            {
                ForceLegacyEmptyObjectHitConsumeForDiagnostics = forceLegacy,
            };
        }

        private static void ConfigureStoreAuthority(
            SimulationWorld world,
            bool enabled)
        {
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.CollisionCandidateStoreAuthorityEnabled = enabled;
            query.CollisionCandidateStoreLegacyOracleInterval = 0;
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int objectId,
            LF2ObjectType objectType,
            LF2FrameData frame)
        {
            return CreateCharacter<LF2Character>(
                world,
                objectId,
                objectType,
                frame);
        }

        private static TCharacter CreateCharacter<TCharacter>(
            SimulationWorld world,
            int objectId,
            LF2ObjectType objectType,
            LF2FrameData frame)
            where TCharacter : LF2Character, new()
        {
            var character = new TCharacter();
            character.ModuleInitialize();
            character.Name = typeof(TCharacter).Name + objectId;
            character.ObjectId = objectId;
            if (character is IMutableDataType mutableDataType)
                mutableDataType.CurrentDataObjectType = objectType;
            LoadCurrentDat(character, objectType, frame);
            character.Initialize(500, 500);
            character.Runtime.SetPosition(0.0, 0.0, 200.0);
            character.Runtime.SyncIntegerPosition();
            character.RefreshRuntimeSnapshot();
            world.Register(character);
            return character;
        }

        private static void LoadCurrentDat(
            LF2Character character,
            LF2ObjectType objectType,
            LF2FrameData frame)
        {
            var data = new LF2CharacterData
            {
                name = character.Name,
                type_sub = (int)objectType,
                frames = new List<LF2FrameData> { frame },
            };
            character.FrameCache.Load(
                new LF2CharacterDataWrapper(character.ObjectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Frame.Prev = 0;
            character.Frame.Prev2 = 0;
            character.Frame.Prev2D = character.Frame.D;
        }

        private static void PrepareCandidateSnapshot(SimulationWorld world)
        {
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
        }

        private static LF2FrameData CreateEmptyFrame()
        {
            return new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
            };
        }

        private static LF2FrameData CreateAttackFrame()
        {
            LF2FrameData frame = CreateEmptyFrame();
            frame.itrs.Add(new InteractionArea
            {
                kind = 0,
                vrest = 1,
                x = -20,
                y = -20,
                w = 40,
                h = 40,
                zwidth = 15,
            });
            return frame;
        }

        private static LF2FrameData CreateBodyFrame()
        {
            LF2FrameData frame = CreateEmptyFrame();
            frame.bodies.Add(new BodyBox
            {
                kind = 0,
                x = -10,
                y = -10,
                w = 20,
                h = 20,
            });
            return frame;
        }

        private interface IMutableDataType
        {
            LF2ObjectType CurrentDataObjectType { get; set; }
        }

        private sealed class MutableTypeCharacter : LF2Character, IMutableDataType
        {
            public LF2ObjectType CurrentDataObjectType { get; set; }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)CurrentDataObjectType;
            }

            public override void SimObjectInteraction(int tickIndex)
            {
            }
        }

        private sealed class ProbeCharacter : LF2Character, IMutableDataType
        {
            internal int ObjectInteractionCount { get; private set; }
            public LF2ObjectType CurrentDataObjectType { get; set; }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)CurrentDataObjectType;
            }

            public override void SimObjectInteraction(int tickIndex)
            {
                ObjectInteractionCount++;
            }
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
