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
    public sealed class PreInteractionNoOpProofEditorTests
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
        private static readonly FieldInfo HeldWeaponField =
            typeof(LF2Character).GetField(
                "_heldWeapon",
                BindingFlags.Instance | BindingFlags.NonPublic);

        [Test]
        public void NeutralExactCharacters_SkipWholePassAndMatchForcedLegacy()
        {
            using var logging = new DisabledLoggingScope();
            Scenario fast = CreateNeutralScenario(32, forceLegacy: false);
            Scenario legacy = CreateNeutralScenario(32, forceLegacy: true);

            fast.World.PreInteractionTickAll(11);
            legacy.World.PreInteractionTickAll(11);

            AssertScenariosEquivalent(fast, legacy, 11);
            Assert.That(
                fast.World
                    .LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.True);
            Assert.That(
                fast.World
                    .LastPreInteractionWholePassParticipantCountForDiagnostics,
                Is.EqualTo(32));
            Assert.That(
                fast.World.LastPreInteractionExecutedCountForDiagnostics,
                Is.Zero);
            Assert.That(
                fast.World.LastPreInteractionProofSkipCountForDiagnostics,
                Is.EqualTo(96));
            Assert.That(
                fast.World.LastPreInteractionSnapshotSkipCountForDiagnostics,
                Is.EqualTo(96));

            Assert.That(
                legacy.World
                    .LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.False);
            Assert.That(
                legacy.World.LastPreInteractionExecutedCountForDiagnostics,
                Is.EqualTo(96));
            Assert.That(
                legacy.World.LastPreInteractionProofSkipCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        [Category("NTSD_U6PreInteractionCrossPass")]
        public void PostFrameProof_NeutralExactCharactersMatchesFullScanOracle()
        {
            using var logging = new DisabledLoggingScope();
            Scenario cached = CreateNeutralScenario(32, forceLegacy: false);
            Scenario fullScan = CreateNeutralScenario(32, forceLegacy: false);
            fullScan.World.ForceLegacyPreInteractionCrossPassProofForDiagnostics =
                true;

            cached.World.PostFrameAdvanceDeathCleanupAll(21);
            fullScan.World.PostFrameAdvanceDeathCleanupAll(21);
            cached.World.PreInteractionTickAll(21);
            fullScan.World.PreInteractionTickAll(21);

            AssertScenariosEquivalent(cached, fullScan, 21);
            Assert.That(
                cached.World
                    .LastPreInteractionCrossPassProofUsedForDiagnostics,
                Is.True);
            Assert.That(
                fullScan.World
                    .LastPreInteractionCrossPassProofUsedForDiagnostics,
                Is.False);
            Assert.That(
                cached.World
                    .LastPreInteractionWholePassParticipantCountForDiagnostics,
                Is.EqualTo(32));
            Assert.That(
                fullScan.World
                    .LastPreInteractionWholePassParticipantCountForDiagnostics,
                Is.EqualTo(32));
        }

        [Test]
        [Category("NTSD_U6PreInteractionCrossPass")]
        public void PostFrameProof_OccupancyChangeFailsClosedToFullScan()
        {
            using var logging = new DisabledLoggingScope();
            Scenario scenario = CreateNeutralScenario(1, forceLegacy: false);

            scenario.World.PostFrameAdvanceDeathCleanupAll(22);
            CreateCharacter<LF2Character>(
                scenario.World,
                2001,
                Frame(0, 0, null));
            scenario.World.PreInteractionTickAll(22);

            Assert.That(
                scenario.World
                    .LastPreInteractionCrossPassProofUsedForDiagnostics,
                Is.False);
            Assert.That(
                scenario.World
                    .LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.True);
            Assert.That(
                scenario.World
                    .LastPreInteractionWholePassParticipantCountForDiagnostics,
                Is.EqualTo(2));
        }

        [Test]
        [Category("NTSD_U6PreInteractionCrossPass")]
        public void PostFrameProof_NonNeutralParticipantFailsClosed()
        {
            using var logging = new DisabledLoggingScope();
            Scenario scenario = CreateNonNeutralScenario(
                forceLegacy: false,
                enableParticipantFiltering: true);

            scenario.World.PostFrameAdvanceDeathCleanupAll(23);
            scenario.World.PreInteractionTickAll(23);

            Assert.That(
                scenario.World
                    .LastPreInteractionCrossPassProofUsedForDiagnostics,
                Is.False);
            Assert.That(
                scenario.World
                    .LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.False);
            AssertHeldLinkCleared(scenario.Entities[2]);
            AssertHeldLinkCleared(scenario.Entities[3]);
            AssertHeldLinkCleared(scenario.Entities[4]);
        }

        [Test]
        public void Kind1Kind2AndStaleHeldState_FailClosedAndMatchForcedLegacy()
        {
            using var logging = new DisabledLoggingScope();
            Scenario fast = CreateNonNeutralScenario(forceLegacy: false);
            Scenario legacy = CreateNonNeutralScenario(forceLegacy: true);

            fast.World.PreInteractionTickAll(12);
            legacy.World.PreInteractionTickAll(12);

            AssertScenariosEquivalent(fast, legacy, 12);
            Assert.That(
                fast.World
                    .LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.False);
            Assert.That(
                fast.World.LastPreInteractionExecutedCountForDiagnostics,
                Is.EqualTo(15));
            Assert.That(
                fast.World.LastPreInteractionProofSkipCountForDiagnostics,
                Is.Zero);
            AssertHeldLinkCleared(fast.Entities[2]);
            AssertHeldLinkCleared(fast.Entities[3]);
            AssertHeldLinkCleared(fast.Entities[4]);
            Assert.That(
                GetHeldWeaponReference(fast.Entities[4]),
                Is.Null);
        }

        [Test]
        public void NonNeutralParticipantFiltering_SkipsOnlyProvenNoOpsAndMatchesLegacy()
        {
            using var logging = new DisabledLoggingScope();
            Scenario filtered = CreateNonNeutralScenario(
                forceLegacy: false,
                enableParticipantFiltering: true);
            Scenario legacy = CreateNonNeutralScenario(forceLegacy: true);

            filtered.World.PreInteractionTickAll(17);
            legacy.World.PreInteractionTickAll(17);

            AssertScenariosEquivalent(filtered, legacy, 17);
            Assert.That(
                filtered.World
                    .LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.False);
            Assert.That(
                filtered.World.LastPreInteractionExecutedCountForDiagnostics,
                Is.EqualTo(5));
            Assert.That(
                filtered.World.LastPreInteractionProofSkipCountForDiagnostics,
                Is.EqualTo(10));
            Assert.That(
                filtered.World
                    .LastPreInteractionCpointCheckProofSkipCountForDiagnostics,
                Is.EqualTo(4));
            Assert.That(
                filtered.World
                    .LastPreInteractionMismatchTailProofSkipCountForDiagnostics,
                Is.EqualTo(4));
            Assert.That(
                filtered.World
                    .LastPreInteractionHeldSyncProofSkipCountForDiagnostics,
                Is.EqualTo(2));
            Assert.That(
                legacy.World.LastPreInteractionExecutedCountForDiagnostics,
                Is.EqualTo(15));
            AssertHeldLinkCleared(filtered.Entities[2]);
            AssertHeldLinkCleared(filtered.Entities[3]);
            AssertHeldLinkCleared(filtered.Entities[4]);
        }

        [Test]
        [Category("NTSD_W08Regression")]
        public void SnapshotFrameWaitAndPositionMismatch_FailClosed()
        {
            using var logging = new DisabledLoggingScope();
            Scenario fast = CreateNeutralScenario(1, forceLegacy: false);
            Scenario legacy = CreateNeutralScenario(1, forceLegacy: true);

            MakePublishedSnapshotStale(fast.Entities[0]);
            MakePublishedSnapshotStale(legacy.Entities[0]);

            fast.World.PreInteractionTickAll(13);
            legacy.World.PreInteractionTickAll(13);

            AssertScenariosEquivalent(fast, legacy, 13);
            Assert.That(
                fast.World
                    .LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.False);
            Assert.That(
                fast.World.LastPreInteractionExecutedCountForDiagnostics,
                Is.EqualTo(3));
            Assert.That(
                fast.Entities[0].Runtime.Frame,
                Is.EqualTo(fast.Entities[0].Frame.N));
            Assert.That(
                fast.Entities[0].Runtime.WaitCounter,
                Is.EqualTo(fast.Entities[0].Trans.WaitCounter));
            Assert.That(
                fast.Entities[0].Runtime.XInt,
                Is.EqualTo(999),
                "Pre-interaction refresh must not normalize deliberate runtime float/integer position divergence.");
            Assert.That(fast.Entities[0].Runtime.X, Is.EqualTo(91.75));
        }

        [Test]
        public void DerivedCharacter_FailsClosedAndPreservesVirtualSideEffects()
        {
            using var logging = new DisabledLoggingScope();
            var world = CreateWorld(forceLegacy: false);
            ProbeCharacter probe = CreateCharacter<ProbeCharacter>(
                world,
                31,
                Frame(0, 0, null));
            probe.ResetProbes();

            world.PreInteractionTickAll(14);

            Assert.That(
                world.LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.False);
            Assert.That(probe.CpointCheckCount, Is.EqualTo(1));
            Assert.That(probe.MismatchTailCount, Is.EqualTo(1));
            Assert.That(probe.WeaponSyncCount, Is.EqualTo(1));
            Assert.That(probe.RefreshCount, Is.EqualTo(3));
            Assert.That(
                world.LastPreInteractionExecutedCountForDiagnostics,
                Is.EqualTo(3));
        }

        [Test]
        public void Kind2Writer_InvalidLink_AppliesAuthorityFallback()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateWorld(forceLegacy: false);
            LF2Character victim = CreateCharacter<LF2Character>(
                world,
                35,
                Frame(0, LF2States.BeingCaught, new CatchPoint { kind = 2 }));
            victim.Runtime.Y = 7.5;
            victim.Runtime.Vy = 9.0;
            victim.Runtime.FrameWaitCounter = 17;

            victim.RunCpointMismatchTailStep10();

            Assert.That(victim.Frame.N, Is.EqualTo(212));
            Assert.That(victim.Runtime.FrameWaitCounter, Is.EqualTo(17));
            Assert.That(victim.Runtime.Vy, Is.EqualTo(-3.0));
            Assert.That(victim.Runtime.Y, Is.EqualTo(-2.0));
        }

        [Test]
        public void Kind2Writer_ReciprocalKind1Link_PreservesVictim()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateWorld(forceLegacy: false);
            LF2Character catcher = CreateCharacter<LF2Character>(
                world,
                36,
                Frame(0, LF2States.Catching, new CatchPoint { kind = 1 }));
            LF2Character victim = CreateCharacter<LF2Character>(
                world,
                37,
                Frame(0, LF2States.BeingCaught, new CatchPoint { kind = 2 }));
            catcher.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = catcher.Runtime.SlotIndex;
            victim.Runtime.Y = 7.5;
            victim.Runtime.Vy = 9.0;

            victim.RunCpointMismatchTailStep10();

            Assert.That(victim.Frame.N, Is.Zero);
            Assert.That(victim.Runtime.Vy, Is.EqualTo(9.0));
            Assert.That(victim.Runtime.Y, Is.EqualTo(7.5));
        }

        [Test]
        public void WarmedKind1Writer_AllocatesNoManagedMemory()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateWorld(forceLegacy: false);
            LF2Character catcher = CreateCharacter<LF2Character>(
                world,
                38,
                Frame(0, LF2States.Catching, new CatchPoint { kind = 1 }));
            LF2Character victim = CreateCharacter<LF2Character>(
                world,
                39,
                Frame(0, LF2States.BeingCaught, new CatchPoint { kind = 2 }));
            catcher.CaughtSlotIndex = victim.Runtime.SlotIndex;
            victim.CatcherSlotIndex = catcher.Runtime.SlotIndex;

            for (int i = 0; i < 32; i++)
                catcher.RunCpointCheckStep10();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 512; i++)
                catcher.RunCpointCheckStep10();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void SuppressedDerivedCharacter_DoesNotBlockNeutralWholePassProof()
        {
            using var logging = new DisabledLoggingScope();
            var world = CreateWorld(forceLegacy: false);
            CreateCharacter<LF2Character>(
                world,
                41,
                Frame(0, 0, null));
            ProbeCharacter suppressed = CreateCharacter<ProbeCharacter>(
                world,
                42,
                Frame(0, 0, null));
            suppressed.Runtime.SuppressPreInteractionUntilTick = 100;
            suppressed.ResetProbes();

            world.PreInteractionTickAll(15);

            Assert.That(
                world.LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.True);
            Assert.That(
                world
                    .LastPreInteractionWholePassParticipantCountForDiagnostics,
                Is.EqualTo(1));
            Assert.That(suppressed.CpointCheckCount, Is.Zero);
            Assert.That(suppressed.MismatchTailCount, Is.Zero);
            Assert.That(suppressed.WeaponSyncCount, Is.Zero);
            Assert.That(suppressed.RefreshCount, Is.Zero);
        }

        [Test]
        public void ReusedSlotGeneration_ResolvesReplacementAndFailsClosed()
        {
            using var logging = new DisabledLoggingScope();
            var world = CreateWorld(forceLegacy: false);
            LF2Character original = CreateCharacter<LF2Character>(
                world,
                51,
                Frame(0, 0, null));
            int reusedSlot = original.Runtime.SlotIndex;
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    reusedSlot,
                    original,
                    out RuntimeEntityHandle originalHandle),
                Is.True);

            world.Unregister(original);
            ProbeCharacter replacement = CreateCharacter<ProbeCharacter>(
                world,
                52,
                Frame(0, 0, null));
            Assert.That(replacement.Runtime.SlotIndex, Is.EqualTo(reusedSlot));
            Assert.That(
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    reusedSlot,
                    replacement,
                    out RuntimeEntityHandle replacementHandle),
                Is.True);
            Assert.That(
                replacementHandle.Generation,
                Is.Not.EqualTo(originalHandle.Generation));
            replacement.ResetProbes();

            world.PreInteractionTickAll(16);

            Assert.That(
                world.LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.False);
            Assert.That(replacement.CpointCheckCount, Is.EqualTo(1));
            Assert.That(replacement.MismatchTailCount, Is.EqualTo(1));
            Assert.That(replacement.WeaponSyncCount, Is.EqualTo(1));
        }

        [Test]
        public void WarmedNeutralWholePassProof_AllocatesNoManagedMemory()
        {
            using var logging = new DisabledLoggingScope();
            Scenario fast = CreateNeutralScenario(64, forceLegacy: false);

            for (int i = 0; i < 32; i++)
                fast.World.PreInteractionTickAll(20 + i);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 512; i++)
                fast.World.PreInteractionTickAll(100 + i);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
            Assert.That(
                fast.World
                    .LastPreInteractionWholePassProofSucceededForDiagnostics,
                Is.True);
            Assert.That(
                fast.World.LastPreInteractionExecutedCountForDiagnostics,
                Is.Zero);
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
                    CreateCharacter<LF2Character>(
                        world,
                        1000 + i,
                        Frame(0, 0, null)));
            }

            return new Scenario(world, entities);
        }

        private static Scenario CreateNonNeutralScenario(
            bool forceLegacy,
            bool enableParticipantFiltering = false)
        {
            SimulationWorld world = CreateWorld(
                forceLegacy,
                enableParticipantFiltering);
            var entities = new List<LF2Character>
            {
                CreateCharacter<LF2Character>(
                    world,
                    1,
                    Frame(0, 0, new CatchPoint { kind = 1 })),
                CreateCharacter<LF2Character>(
                    world,
                    2,
                    Frame(0, LF2States.BeingCaught, new CatchPoint { kind = 2 })),
                CreateCharacter<LF2Character>(
                    world,
                    3,
                    Frame(0, 0, null)),
                CreateCharacter<LF2Character>(
                    world,
                    4,
                    Frame(0, 0, null)),
                CreateCharacter<LF2Character>(
                    world,
                    5,
                    Frame(0, 0, null)),
            };

            SetStaleHeldLink(entities[2], 9, 109);
            SetStaleHeldLink(entities[3], -1, 110);
            SetHeldWeaponReference(
                entities[4],
                new LF2Character());
            return new Scenario(world, entities);
        }

        private static SimulationWorld CreateWorld(
            bool forceLegacy,
            bool enableParticipantFiltering = false)
        {
            return new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity)
            {
                ForceLegacyPreInteractionForDiagnostics = forceLegacy,
                ForceLegacyPreInteractionParticipantFilteringForDiagnostics =
                    !enableParticipantFiltering,
            };
        }

        private static TCharacter CreateCharacter<TCharacter>(
            SimulationWorld world,
            int objectId,
            LF2FrameData currentFrame)
            where TCharacter : LF2Character, new()
        {
            LF2FrameData fallbackFrame =
                Frame(212, LF2States.Jump, null);
            var data = new LF2CharacterData
            {
                name = typeof(TCharacter).Name + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    currentFrame,
                    fallbackFrame,
                },
            };
            var character = new TCharacter();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = objectId;
            character.FrameCache.Load(
                new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D =
                character.FrameCache.GetFrameDataById(currentFrame.frameId);
            character.Frame.N = currentFrame.frameId;
            character.Frame.PN = currentFrame.frameId;
            character.Frame.Prev = currentFrame.frameId;
            character.Initialize(500, 500);
            character.Frame.D =
                character.FrameCache.GetFrameDataById(currentFrame.frameId);
            character.Frame.N = currentFrame.frameId;
            character.Frame.PN = currentFrame.frameId;
            character.Frame.Prev = currentFrame.frameId;
            character.Frame.Prev2 = currentFrame.frameId;
            character.Frame.Prev2D = character.Frame.D;
            character.Runtime.SetPosition(
                objectId * 3.25,
                0,
                objectId * 2.5);
            character.Runtime.SyncIntegerPosition();
            character.RefreshRuntimeSnapshot();
            world.Register(character);
            return character;
        }

        private static LF2FrameData Frame(
            int frameId,
            int state,
            CatchPoint cpoint)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 100,
                next = frameId,
                centerx = 39,
                centery = 79,
                cpoint = cpoint,
            };
        }

        private static void SetStaleHeldLink(
            LF2Character character,
            int linkState,
            int targetSlot)
        {
            character.Runtime.LinkState = linkState;
            character.Runtime.TargetSlotIndex = targetSlot;
            character.Runtime.HeldWeaponStableId = targetSlot;
        }

        private static void AssertHeldLinkCleared(LF2Character character)
        {
            Assert.That(character.Runtime.LinkState, Is.Zero);
            Assert.That(
                character.Runtime.TargetSlotIndex,
                Is.EqualTo(-1));
            Assert.That(
                character.Runtime.HeldWeaponStableId,
                Is.EqualTo(-1));
        }

        private static ILF2Object GetHeldWeaponReference(
            LF2Character character)
        {
            Assert.That(HeldWeaponField, Is.Not.Null);
            return (ILF2Object)HeldWeaponField.GetValue(character);
        }

        private static void SetHeldWeaponReference(
            LF2Character character,
            ILF2Object held)
        {
            Assert.That(HeldWeaponField, Is.Not.Null);
            HeldWeaponField.SetValue(character, held);
        }

        private static void MakePublishedSnapshotStale(
            LF2Character character)
        {
            character.Frame.N = 7;
            character.Runtime.Frame = 0;
            character.Runtime.WaitCounter = 999;
            character.Runtime.SetPosition(91.75, -3.25, 47.5);
            character.Runtime.XInt = 999;
            character.Runtime.YInt = 999;
            character.Runtime.ZInt = 999;
        }

        private static void AssertScenariosEquivalent(
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

        private sealed class ProbeCharacter : LF2Character
        {
            public int CpointCheckCount { get; private set; }
            public int MismatchTailCount { get; private set; }
            public int WeaponSyncCount { get; private set; }
            public int RefreshCount { get; private set; }

            public void ResetProbes()
            {
                CpointCheckCount = 0;
                MismatchTailCount = 0;
                WeaponSyncCount = 0;
                RefreshCount = 0;
            }

            public override void RunCpointCheckStep10()
            {
                CpointCheckCount++;
            }

            public override void RunCpointMismatchTailStep10()
            {
                MismatchTailCount++;
            }

            public override void RunWeaponSyncHeldStep10()
            {
                WeaponSyncCount++;
            }

            protected override void RefreshRuntimeFromEntity()
            {
                base.RefreshRuntimeFromEntity();
                RefreshCount++;
                Runtime.Unk330++;
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
