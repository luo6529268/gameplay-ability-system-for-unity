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
    public sealed class PendingDestroySlotAdmissionEditorTests
    {
        private const int DynamicSlotStart = 50;
        private const int MobileEntityCount =
            BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities;

        [Test]
        public void PendingFlushDestroyMutationEpoch_AdvancesOnlyWhenValueChanges()
        {
            var runtime = new NTSDEntityRuntime();
            long epoch = runtime.PendingFlushDestroyMutationEpochForDiagnostics;

            runtime.PendingFlushDestroy = false;
            Assert.That(
                runtime.PendingFlushDestroyMutationEpochForDiagnostics,
                Is.EqualTo(epoch));

            runtime.PendingFlushDestroy = true;
            epoch++;
            Assert.That(runtime.PendingFlushDestroy, Is.True);
            Assert.That(
                runtime.PendingFlushDestroyMutationEpochForDiagnostics,
                Is.EqualTo(epoch));

            runtime.PendingFlushDestroy = true;
            Assert.That(
                runtime.PendingFlushDestroyMutationEpochForDiagnostics,
                Is.EqualTo(epoch));

            runtime.Reset();
            epoch++;
            Assert.That(runtime.PendingFlushDestroy, Is.False);
            Assert.That(
                runtime.PendingFlushDestroyMutationEpochForDiagnostics,
                Is.EqualTo(epoch));
        }

        [Test]
        public void SaturatedMobileProbes_SkipUntilPendingMutationAndReleaseLowestSlot()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateSaturatedMobileWorld(out List<SlotOccupant> entities);
            Func<int, int, int> probe = CreateSlotProbe(world);

            long fullScansBefore = world.PendingDestroyFullScanCount;
            long skipsBefore = world.PendingDestroySkipCount;
            long visitedBefore = world.PendingDestroyVisitedEntityCount;

            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(-1));
            Assert.That(world.PendingDestroyFullScanCount, Is.EqualTo(fullScansBefore + 1));
            Assert.That(
                world.PendingDestroyVisitedEntityCount,
                Is.EqualTo(visitedBefore + MobileEntityCount));

            for (int i = 0; i < 32; i++)
                Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(-1));
            Assert.That(world.PendingDestroyFullScanCount, Is.EqualTo(fullScansBefore + 1));
            Assert.That(world.PendingDestroySkipCount, Is.EqualTo(skipsBefore + 32));
            Assert.That(
                world.PendingDestroyVisitedEntityCount,
                Is.EqualTo(visitedBefore + MobileEntityCount));

            entities[0].Runtime.PendingFlushDestroy = true;
            entities[0].Runtime.PendingFlushDestroy = false;
            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(-1));
            Assert.That(world.PendingDestroyFullScanCount, Is.EqualTo(fullScansBefore + 2));

            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(-1));
            Assert.That(world.PendingDestroySkipCount, Is.EqualTo(skipsBefore + 33));

            entities[0].Runtime.PendingFlushDestroy = true;
            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(DynamicSlotStart));
            Assert.That(world.PendingDestroyFullScanCount, Is.EqualTo(fullScansBefore + 3));
            Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(MobileEntityCount - 1));
            Assert.That(entities[0].Runtime.SlotIndex, Is.EqualTo(-1));
        }

        [Test]
        public void OccupancyClaimReleaseAndGrow_EachInvalidatePendingScanCache()
        {
            using var logging = new DisabledLoggingScope();
            var world = new SimulationWorld(BattleRuntimeProfile.DesktopExtended, 64);
            Func<int, int, int> probe = CreateSlotProbe(world);
            var first = new SlotOccupant(1);
            world.Register(first);

            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(51));
            long fullScans = world.PendingDestroyFullScanCount;
            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(51));
            Assert.That(world.PendingDestroyFullScanCount, Is.EqualTo(fullScans));

            world.Unregister(first);
            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(DynamicSlotStart));
            fullScans++;
            Assert.That(world.PendingDestroyFullScanCount, Is.EqualTo(fullScans),
                "a successful release must invalidate the scan cache");

            var replacement = new SlotOccupant(2);
            world.Register(replacement);
            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(51));
            fullScans++;
            Assert.That(world.PendingDestroyFullScanCount, Is.EqualTo(fullScans),
                "a successful claim must invalidate the scan cache");

            for (int slot = 51; slot < 64; slot++)
                world.Register(new SlotOccupant(slot));
            Assert.That(world.RuntimeSlotCapacityForDiagnostics, Is.EqualTo(64));

            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(64));
            Assert.That(world.RuntimeSlotCapacityForDiagnostics, Is.EqualTo(256));
            fullScans = world.PendingDestroyFullScanCount;

            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(64));
            Assert.That(world.PendingDestroyFullScanCount, Is.EqualTo(fullScans + 1),
                "a successful desktop grow must invalidate the scan cache");
        }

        [Test]
        public void CrossWorldPendingMutation_DoesNotInvalidateAnotherWorldScanCache()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateSaturatedMobileWorld(out _);
            Func<int, int, int> probe = CreateSlotProbe(world);
            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(-1));
            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(-1));

            long fullScans = world.PendingDestroyFullScanCount;
            long skips = world.PendingDestroySkipCount;
            var otherWorld = new SimulationWorld();
            var otherEntity = new SlotOccupant(2001);
            otherWorld.Register(otherEntity);
            otherEntity.Runtime.PendingFlushDestroy = true;

            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(-1));
            Assert.That(world.PendingDestroyFullScanCount, Is.EqualTo(fullScans));
            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(-1));
            Assert.That(world.PendingDestroyFullScanCount, Is.EqualTo(fullScans));
            Assert.That(world.PendingDestroySkipCount, Is.EqualTo(skips + 2));
        }

        [Test]
        public void ForcedLegacyAndFastScan_ProduceEquivalentAdmissionState()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld fastWorld =
                CreateSaturatedMobileWorld(out List<SlotOccupant> fastEntities);
            SimulationWorld legacyWorld =
                CreateSaturatedMobileWorld(out List<SlotOccupant> legacyEntities);
            legacyWorld.ForceLegacyPendingDestroyScanForDiagnostics = true;
            Func<int, int, int> fastProbe = CreateSlotProbe(fastWorld);
            Func<int, int, int> legacyProbe = CreateSlotProbe(legacyWorld);

            fastEntities[0].Runtime.PendingFlushDestroy = true;
            legacyEntities[0].Runtime.PendingFlushDestroy = true;
            int fastSlot = ProbeDynamicBand(fastWorld, fastProbe);
            int legacySlot = ProbeDynamicBand(legacyWorld, legacyProbe);

            Assert.That(fastSlot, Is.EqualTo(DynamicSlotStart));
            Assert.That(legacySlot, Is.EqualTo(fastSlot));
            Assert.That(
                GetSlotView(fastWorld, DynamicSlotStart).Generation,
                Is.EqualTo(GetSlotView(legacyWorld, DynamicSlotStart).Generation));
            Assert.That(
                fastWorld.ClaimedRuntimeSlotCountForDiagnostics,
                Is.EqualTo(legacyWorld.ClaimedRuntimeSlotCountForDiagnostics));
            Assert.That(fastWorld.ObjectCount, Is.EqualTo(legacyWorld.ObjectCount));
            Assert.That(
                fastWorld.CaptureExtendedChecksumSnapshot(1).OverallChecksum,
                Is.EqualTo(legacyWorld.CaptureExtendedChecksumSnapshot(1).OverallChecksum));

            long legacyScans = legacyWorld.PendingDestroyFullScanCount;
            Assert.That(ProbeDynamicBand(legacyWorld, legacyProbe), Is.EqualTo(DynamicSlotStart));
            Assert.That(legacyWorld.PendingDestroyFullScanCount, Is.EqualTo(legacyScans + 1));
            Assert.That(legacyWorld.PendingDestroySkipCount, Is.Zero);
        }

        [Test]
        public void SaturatedMobileWarmedProbes_AllocateNoManagedMemory()
        {
            using var logging = new DisabledLoggingScope();
            SimulationWorld world = CreateSaturatedMobileWorld(out _);
            Func<int, int, int> probe = CreateSlotProbe(world);

            Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(-1));
            for (int i = 0; i < 32; i++)
                Assert.That(ProbeDynamicBand(world, probe), Is.EqualTo(-1));

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int i = 0; i < 512; i++)
                checksum += ProbeDynamicBand(world, probe);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(checksum, Is.EqualTo(-512));
            Assert.That(after - before, Is.Zero);
        }

        [Test]
        public void ResetRuntimeState_WarmedRegistry_AllocatesNoManagedMemory()
        {
            using var logging = new DisabledLoggingScope();
            var world = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended,
                128);
            var entities = new List<SlotOccupant>(32);
            for (int i = 0; i < entities.Capacity; i++)
            {
                var entity = new SlotOccupant(3000 + i);
                entities.Add(entity);
                world.Register(entity);
            }

            world.ResetRuntimeState();
            for (int i = 0; i < entities.Count; i++)
                world.Register(entities[i]);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            world.ResetRuntimeState();
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.Zero);
            Assert.That(world.ObjectCount, Is.Zero);
            Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.Zero);
        }

        private static SimulationWorld CreateSaturatedMobileWorld(
            out List<SlotOccupant> entities)
        {
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            entities = new List<SlotOccupant>(MobileEntityCount);
            for (int i = 0; i < MobileEntityCount; i++)
            {
                var entity = new SlotOccupant(i + 1);
                entities.Add(entity);
                world.Register(entity);
            }

            Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.EqualTo(MobileEntityCount));
            Assert.That(entities[0].Runtime.SlotIndex, Is.EqualTo(DynamicSlotStart));
            Assert.That(
                entities[entities.Count - 1].Runtime.SlotIndex,
                Is.EqualTo(BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity - 1));
            return world;
        }

        private static int ProbeDynamicBand(
            SimulationWorld world,
            Func<int, int, int> probe)
        {
            return probe(DynamicSlotStart, world.RuntimeSlotCapacityForDiagnostics);
        }

        private static Func<int, int, int> CreateSlotProbe(SimulationWorld world)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                "FindFirstFreeRuntimeSlot",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (Func<int, int, int>)method.CreateDelegate(
                typeof(Func<int, int, int>),
                world);
        }

        private static RuntimeSlotTable.ReadOnlySlotView GetSlotView(
            SimulationWorld world,
            int runtimeSlot)
        {
            Assert.That(
                world.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                    runtimeSlot,
                    out RuntimeSlotTable.ReadOnlySlotView view),
                Is.True);
            return view;
        }

        private sealed class SlotOccupant : LF2OtherObject
        {
            public SlotOccupant(int stableId)
            {
                StableId = stableId;
                ObjectId = 31998;
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }
        }

        private sealed class DisabledLoggingScope : IDisposable
        {
            private readonly bool original;

            public DisabledLoggingScope()
            {
                original = Debug.unityLogger.logEnabled;
                Debug.unityLogger.logEnabled = false;
            }

            public void Dispose()
            {
                Debug.unityLogger.logEnabled = original;
            }
        }
    }
}
#endif
