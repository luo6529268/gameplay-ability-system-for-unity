#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleEcsLateTailNoOpEditorTests
    {
        [Test]
        public void ProductionDefault_PreservesAuthorityTail()
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            CreateCharacter<LF2Character>(world, 77, previousState: 0);

            world.LateEntityUpdateAll(10);

            Assert.That(world.ForceLegacyLateTailNoOpForDiagnostics, Is.True);
            Assert.That(world.LastLateTailNoOpSkipCountForDiagnostics, Is.Zero);
            Assert.That(world.LastLateTailExecutedCountForDiagnostics, Is.EqualTo(1));
        }

        [Test]
        public void NeutralExactCharacters_SkipTailAndMatchForcedLegacy()
        {
            SimulationWorld fast = CreateWorld(forceLegacy: false);
            SimulationWorld legacy = CreateWorld(forceLegacy: true);
            for (int i = 0; i < 8; i++)
            {
                CreateCharacter<LF2Character>(fast, 100 + i, previousState: 0);
                CreateCharacter<LF2Character>(legacy, 100 + i, previousState: 0);
            }

            fast.LateEntityUpdateAll(11);
            legacy.LateEntityUpdateAll(11);

            Assert.That(fast.LastLateTailNoOpSkipCountForDiagnostics, Is.EqualTo(8));
            Assert.That(fast.LastLateTailExecutedCountForDiagnostics, Is.Zero);
            Assert.That(legacy.LastLateTailNoOpSkipCountForDiagnostics, Is.Zero);
            Assert.That(legacy.LastLateTailExecutedCountForDiagnostics, Is.EqualTo(8));
            Assert.That(fast.Rng.State, Is.EqualTo(legacy.Rng.State));
            Assert.That(fast.Rng.CallCount, Is.EqualTo(legacy.Rng.CallCount));
            Assert.That(
                fast.CaptureExtendedChecksumSnapshot(11).OverallChecksum,
                Is.EqualTo(legacy.CaptureExtendedChecksumSnapshot(11).OverallChecksum));
        }

        [Test]
        public void TransitionState_FailsClosedToAuthorityTail()
        {
            SimulationWorld world = CreateWorld(forceLegacy: false);
            CreateCharacter<LF2Character>(world, 201, previousState: 18);

            world.LateEntityUpdateAll(12);

            Assert.That(world.LastLateTailNoOpSkipCountForDiagnostics, Is.Zero);
            Assert.That(world.LastLateTailExecutedCountForDiagnostics, Is.EqualTo(1));
        }

        [Test]
        public void LowRuntimeSlot_FailsClosedToN30AuthorityTail()
        {
            SimulationWorld world = CreateWorld(forceLegacy: false);
            LF2Character character =
                CreateCharacter<LF2Character>(world, 301, previousState: 0);
            character.Runtime.SlotIndex = 5;

            world.LateEntityUpdateAll(13);

            Assert.That(world.LastLateTailNoOpSkipCountForDiagnostics, Is.Zero);
            Assert.That(world.LastLateTailExecutedCountForDiagnostics, Is.EqualTo(1));
        }

        [Test]
        public void DerivedCharacter_PreservesVirtualTailSideEffects()
        {
            SimulationWorld world = CreateWorld(forceLegacy: false);
            ProbeCharacter character =
                CreateCharacter<ProbeCharacter>(world, 401, previousState: 0);

            world.LateEntityUpdateAll(14);

            Assert.That(world.LastLateTailNoOpSkipCountForDiagnostics, Is.Zero);
            Assert.That(world.LastLateTailExecutedCountForDiagnostics, Is.EqualTo(1));
            Assert.That(character.TailCallCount, Is.EqualTo(1));
        }

        [Test]
        public void WarmedNeutralTailProof_AllocatesNoManagedMemory()
        {
            SimulationWorld world = CreateWorld(forceLegacy: false);
            for (int i = 0; i < 32; i++)
                CreateCharacter<LF2Character>(world, 500 + i, previousState: 0);

            world.LateEntityUpdateAll(20);
            long before = GC.GetAllocatedBytesForCurrentThread();

            world.LateEntityUpdateAll(21);

            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero);
            Assert.That(world.LastLateTailNoOpSkipCountForDiagnostics, Is.EqualTo(32));
        }

        private static SimulationWorld CreateWorld(bool forceLegacy)
        {
            return new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity)
            {
                ForceLegacyLateTailNoOpForDiagnostics = forceLegacy,
            };
        }

        private static TCharacter CreateCharacter<TCharacter>(
            SimulationWorld world,
            int objectId,
            int previousState)
            where TCharacter : LF2Character, new()
        {
            LF2FrameData currentFrame = Frame(0, 0);
            LF2FrameData previousFrame = Frame(1, previousState);
            var data = new LF2CharacterData
            {
                name = typeof(TCharacter).Name + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    currentFrame,
                    previousFrame,
                },
            };
            var character = new TCharacter();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(
                new LF2CharacterDataWrapper(objectId, data));
            character.Initialize(500, 500);
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Frame.Prev = 1;
            character.Frame.Prev2 = 0;
            character.Frame.Prev2D = character.Frame.D;
            character.Runtime.SuppressLateFrameTickUntilTick = 100;
            character.Runtime.SetPosition(objectId * 2.0, 0, objectId * 1.5);
            character.Runtime.SyncIntegerPosition();
            character.RefreshRuntimeSnapshot();
            world.Register(character);
            character.Runtime.SlotIndex = 20 + (objectId % 900);
            character.Runtime.SuppressLateFrameTickUntilTick = 100;
            return character;
        }

        private static LF2FrameData Frame(int frameId, int state)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 100,
                next = frameId,
                centerx = 39,
                centery = 79,
            };
        }

        private sealed class ProbeCharacter : LF2Character
        {
            internal int TailCallCount { get; private set; }

            internal override void RunLateTailBeforePrevFrame()
            {
                TailCallCount++;
                base.RunLateTailBeforePrevFrame();
            }
        }
    }
}
#endif
