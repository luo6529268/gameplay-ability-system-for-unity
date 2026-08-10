#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Spatial;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class LooseQuadtreeNearestEditorTests
    {
        [Test]
        public void Rebuild_PreparedDeepRetainedEntries_PreservesOrderAndAllocatesZeroBytes()
        {
            const int entryCount = 512;
            var preferredRoot = new SpatialAabbXZ(-4096, -4096, 4096, 4096);
            var retainedBounds = new SpatialAabbXZ(0, 0, 2, 2);
            var entries = new List<SpatialBroadphaseEntry>(entryCount);
            for (int index = 0; index < entryCount; index++)
            {
                entries.Add(new SpatialBroadphaseEntry(
                    index,
                    index,
                    retainedBounds));
            }

            var warmup = new LooseQuadtreeBroadphase(1, 8);
            warmup.PrepareCapacity(1);
            warmup.Rebuild(entries.GetRange(0, 1), preferredRoot);

            var tree = new LooseQuadtreeBroadphase(1, 8);
            tree.PrepareCapacity(entryCount);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = GC.GetAllocatedBytesForCurrentThread();
            tree.Rebuild(entries, preferredRoot);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero,
                "A prepared rebuild must not grow per-node managed containers.");

            var results = new List<int>(entryCount);
            tree.Query(new SpatialAabbXZ(-1, -1, 3, 3), results);
            Assert.That(results.Count, Is.EqualTo(entryCount));
            for (int index = 0; index < entryCount; index++)
                Assert.That(results[index], Is.EqualTo(index));
        }

        private struct SlotFilter : IIncrementalPointNearestFilter
        {
            public bool[] AcceptedSlots;
            public uint[] ExpectedGenerations;

            public IncrementalPointFilterDecision Evaluate(RuntimeEntityHandle handle)
            {
                if (handle.Slot < 0 || handle.Slot >= AcceptedSlots.Length)
                    return IncrementalPointFilterDecision.Abort;
                if (ExpectedGenerations != null &&
                    ExpectedGenerations[handle.Slot] != handle.Generation)
                {
                    return IncrementalPointFilterDecision.Abort;
                }
                return AcceptedSlots[handle.Slot]
                    ? IncrementalPointFilterDecision.Accept
                    : IncrementalPointFilterDecision.Reject;
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(20)]
        [TestCase(100)]
        [TestCase(1000)]
        public void BestFirstNearest_MatchesBruteAcrossRandomLayouts(int count)
        {
            const int queryCount = 80;
            var random = new Random(0x5EED + count);
            var entries = new List<IncrementalSpatialEntry>(count);
            var accepted = new bool[Math.Max(1, count)];
            var generations = new uint[Math.Max(1, count)];
            var positionsX = new int[Math.Max(1, count)];
            var positionsZ = new int[Math.Max(1, count)];
            for (int slot = 0; slot < count; slot++)
            {
                int x = random.Next(-20000, 20001);
                int z = random.Next(-20000, 20001);
                uint generation = (uint)(slot + 1);
                accepted[slot] = random.Next(0, 5) != 0;
                generations[slot] = generation;
                positionsX[slot] = x;
                positionsZ[slot] = z;
                entries.Add(new IncrementalSpatialEntry(
                    new RuntimeEntityHandle(slot, generation),
                    new SpatialAabbXZ(x, z, x + 1, z + 1)));
            }

            var tree = new LooseQuadtreeBroadphase(4, 8);
            SpatialSynchronizeResult synchronized = tree.Synchronize(
                entries,
                new SpatialAabbXZ(-20000, -20000, 20001, 20001));
            Assert.That(synchronized.Succeeded, Is.True);

            var filter = new SlotFilter
            {
                AcceptedSlots = accepted,
                ExpectedGenerations = generations,
            };
            for (int queryIndex = 0; queryIndex < queryCount; queryIndex++)
            {
                int pointX = random.Next(-22000, 22001);
                int pointZ = random.Next(-22000, 22001);
                BruteNearest(
                    entries,
                    accepted,
                    pointX,
                    pointZ,
                    10000,
                    10000,
                    10000,
                    out RuntimeEntityHandle expected,
                    out int expectedDistance);

                bool succeeded = tree.TryFindNearestPointManhattan(
                    pointX,
                    pointZ,
                    10000,
                    10000,
                    10000,
                    ref filter,
                    out RuntimeEntityHandle actual,
                    out int actualDistance,
                    out _);

                Assert.That(succeeded, Is.True);
                Assert.That(actual, Is.EqualTo(expected));
                Assert.That(actualDistance, Is.EqualTo(expectedDistance));
            }
        }

        [Test]
        public void BestFirstNearest_PreservesTieDistanceAndStrictBoundaries()
        {
            var entries = new List<IncrementalSpatialEntry>
            {
                Entry(9, 1, -5, 5),
                Entry(2, 1, 5, -5),
                Entry(4, 1, 9999, 0),
                Entry(1, 1, 10000, 0),
                Entry(6, 1, 249, 39),
                Entry(7, 1, 250, 0),
                Entry(8, 1, 0, 40),
            };
            var accepted = new bool[10];
            var generations = new uint[10];
            for (int i = 0; i < entries.Count; i++)
            {
                RuntimeEntityHandle handle = entries[i].Handle;
                accepted[handle.Slot] = true;
                generations[handle.Slot] = handle.Generation;
            }

            var tree = BuildTree(entries);
            var filter = new SlotFilter
            {
                AcceptedSlots = accepted,
                ExpectedGenerations = generations,
            };

            AssertNearest(tree, ref filter, 0, 0, 11, 11, 11, 2, 10);

            accepted[2] = false;
            accepted[9] = false;
            accepted[6] = false;
            accepted[7] = false;
            accepted[8] = false;
            AssertNearest(tree, ref filter, 0, 0, 10000, 10000, 10000, 4, 9999);

            accepted[4] = false;
            AssertNearest(tree, ref filter, 0, 0, 10000, 10000, 10000, -1, 10000);

            accepted[6] = true;
            accepted[7] = true;
            accepted[8] = true;
            AssertNearest(tree, ref filter, 0, 0, 10000, 250, 40, 6, 288);
            accepted[6] = false;
            AssertNearest(tree, ref filter, 0, 0, 10000, 250, 40, -1, 10000);
        }

        [Test]
        public void BestFirstNearest_RejectsStaleGenerationAndAcceptsReusedSlot()
        {
            var tree = new LooseQuadtreeBroadphase(1, 6);
            var entries = new List<IncrementalSpatialEntry>
            {
                Entry(3, 1, 4, 0),
                Entry(8, 1, 20, 0),
            };
            Assert.That(tree.Synchronize(
                entries,
                new SpatialAabbXZ(0, 0, 32, 1)).Succeeded, Is.True);

            var accepted = new bool[9];
            var generations = new uint[9];
            accepted[3] = true;
            accepted[8] = true;
            generations[3] = 1;
            generations[8] = 1;
            var filter = new SlotFilter
            {
                AcceptedSlots = accepted,
                ExpectedGenerations = generations,
            };
            AssertNearest(tree, ref filter, 0, 0, 10000, 10000, 10000, 3, 4);

            entries[0] = Entry(3, 2, 12, 0);
            generations[3] = 2;
            Assert.That(tree.Synchronize(
                entries,
                new SpatialAabbXZ(0, 0, 32, 1)).Succeeded, Is.True);
            AssertNearest(tree, ref filter, 0, 0, 10000, 10000, 10000, 3, 12);

            generations[3] = 1;
            bool succeeded = tree.TryFindNearestPointManhattan(
                0,
                0,
                10000,
                10000,
                10000,
                ref filter,
                out _,
                out _,
                out _);
            Assert.That(succeeded, Is.False);
        }

        [Test]
        public void IncrementalPointMutation_UpsertsMovesRemovesAndReusesGeneration()
        {
            var tree = new LooseQuadtreeBroadphase(1, 6);
            var entries = new List<IncrementalSpatialEntry>();
            Assert.That(tree.Synchronize(
                entries,
                new SpatialAabbXZ(-256, -256, 256, 256)).Succeeded, Is.True);

            var accepted = new bool[4];
            var generations = new uint[4];
            accepted[3] = true;
            generations[3] = 1;
            var filter = new SlotFilter
            {
                AcceptedSlots = accepted,
                ExpectedGenerations = generations,
            };
            bool emptySucceeded = tree.TryFindNearestPointManhattan(
                0,
                0,
                10000,
                250,
                40,
                ref filter,
                out RuntimeEntityHandle emptyHandle,
                out int emptyDistance,
                out int emptyVisited);
            Assert.That(emptySucceeded, Is.True);
            Assert.That(emptyHandle.IsValid, Is.False);
            Assert.That(emptyDistance, Is.EqualTo(10000));
            Assert.That(emptyVisited, Is.Zero);

            RuntimeEntityHandle first = new RuntimeEntityHandle(3, 1);
            Assert.That(tree.TryUpsertIncremental(
                first,
                new SpatialAabbXZ(30, 0, 31, 1)), Is.True);
            AssertNearest(tree, ref filter, 0, 0, 10000, 10000, 10000, 3, 30);

            Assert.That(tree.TryUpsertIncremental(
                first,
                new SpatialAabbXZ(-12, 4, -11, 5)), Is.True);
            AssertNearest(tree, ref filter, 0, 0, 10000, 10000, 10000, 3, 16);

            Assert.That(tree.TryRemoveIncremental(first), Is.True);
            AssertNearest(tree, ref filter, 0, 0, 10000, 10000, 10000, -1, 10000);
            Assert.That(tree.TryRemoveIncremental(first), Is.False);

            RuntimeEntityHandle reused = new RuntimeEntityHandle(3, 2);
            generations[3] = 2;
            Assert.That(tree.TryUpsertIncremental(
                reused,
                new SpatialAabbXZ(7, 8, 8, 9)), Is.True);
            AssertNearest(tree, ref filter, 0, 0, 10000, 10000, 10000, 3, 15);
        }

        [Test]
        [Category("NTSD_W08Regression")]
        public void AirRoleIndex_LiveGroundAirTransitionsMatchAuthorityBruteScan()
        {
            var world = new SimulationWorld();
            world.ForceLegacyAiNearestFilterForDiagnostics = true;
            LF2Character self = CreateCharacter("AirRole_Self", 33, 0, 1, 0, 0, 0);
            LF2Character target = CreateCharacter(
                "AirRole_Target",
                4,
                5,
                2,
                100,
                0,
                10);
            world.Register(self);
            world.Register(target);

            Assert.That(InvokeAirRoleMutationParity(
                world,
                self,
                target,
                2,
                -3), Is.True, "ground-to-air role mutation");
            Assert.That(InvokeAirRoleMutationParity(
                world,
                self,
                target,
                2,
                0), Is.True, "air-to-ground role mutation");

            target.RelationTeam = 1;
            Assert.That(InvokeAirRoleMutationParity(
                world,
                self,
                target,
                2,
                -3), Is.True, "same-team air role mutation");
            target.RelationTeam = 2;
            target.Runtime.HP = 0;
            Assert.That(InvokeAirRoleMutationParity(
                world,
                self,
                target,
                2,
                0), Is.True, "dead ground role mutation");
            target.Runtime.HP = 500;
            Assert.That(InvokeAirRoleMutationParity(
                world,
                self,
                target,
                1,
                -3), Is.True, "phase-one revived air role mutation");
        }

        [Test]
        public void IncrementalMembership_ConcentratedRemoveReinsertAllocatesNoManagedMemory()
        {
            const int count = 1000;
            var entries = new List<IncrementalSpatialEntry>(count);
            var sharedBounds = new SpatialAabbXZ(-8, -8, 8, 8);
            for (int slot = 0; slot < count; slot++)
            {
                entries.Add(new IncrementalSpatialEntry(
                    new RuntimeEntityHandle(slot, 1),
                    sharedBounds));
            }

            var tree = new LooseQuadtreeBroadphase();
            tree.PrepareCapacity(count);
            Assert.That(tree.Synchronize(
                entries,
                new SpatialAabbXZ(-256, -256, 256, 256)).Succeeded, Is.True);

            RuntimeEntityHandle recycled = entries[0].Handle;
            Assert.That(tree.TryRemoveIncremental(recycled), Is.True);
            Assert.That(tree.TryUpsertIncremental(recycled, sharedBounds), Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                if (!tree.TryRemoveIncremental(recycled) ||
                    !tree.TryUpsertIncremental(recycled, sharedBounds))
                {
                    Assert.Fail("Concentrated incremental membership mutation failed.");
                }
            }
            long after = GC.GetAllocatedBytesForCurrentThread();

            var result = new List<RuntimeEntityHandle>(count);
            tree.QueryHandles(sharedBounds, result);
            Assert.That(after - before, Is.EqualTo(0));
            Assert.That(result.Count, Is.EqualTo(count));
        }

        [Test]
        public void BestFirstNearest_SteadyStateAllocatesNoManagedMemory()
        {
            const int count = 1000;
            var entries = new List<IncrementalSpatialEntry>(count);
            var accepted = new bool[count];
            var generations = new uint[count];
            for (int slot = 0; slot < count; slot++)
            {
                int x = (slot % 40) * 40;
                int z = (slot / 40) * 40;
                entries.Add(Entry(slot, 1, x, z));
                accepted[slot] = true;
                generations[slot] = 1;
            }

            LooseQuadtreeBroadphase tree = BuildTree(entries);
            var filter = new SlotFilter
            {
                AcceptedSlots = accepted,
                ExpectedGenerations = generations,
            };
            for (int i = 0; i < 32; i++)
            {
                tree.TryFindNearestPointManhattan(
                    i,
                    i,
                    10000,
                    10000,
                    10000,
                    ref filter,
                    out _,
                    out _,
                    out _);
            }

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 256; i++)
            {
                bool updated = tree.TryUpsertIncremental(
                    new RuntimeEntityHandle(0, 1),
                    new SpatialAabbXZ(0, 0, 1, 1));
                bool succeeded = tree.TryFindNearestPointManhattan(
                    i % 1600,
                    i % 1000,
                    10000,
                    10000,
                    10000,
                    ref filter,
                    out _,
                    out _,
                    out _);
                if (!updated || !succeeded)
                    Assert.Fail("Nearest traversal aborted unexpectedly.");
            }
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.EqualTo(0));
        }

        private static IncrementalSpatialEntry Entry(
            int slot,
            uint generation,
            int x,
            int z)
        {
            return new IncrementalSpatialEntry(
                new RuntimeEntityHandle(slot, generation),
                new SpatialAabbXZ(x, z, x + 1, z + 1));
        }

        private static LF2Character CreateCharacter(
            string name,
            int objectId,
            int runtimeSlot,
            int team,
            int x,
            int y,
            int z)
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
            character.FrameDelay = 0;
            character.SetRequiredRuntimeSlot(runtimeSlot);
            character.Team = team;
            character.RelationTeam = team;
            character.Runtime.SetPosition(x, y, z);
            character.Runtime.SyncIntegerPosition();
            return character;
        }

        private static bool InvokeAirRoleMutationParity(
            SimulationWorld world,
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int candidateY)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                "AiAirRoleMutationMatchesBruteForSelfCheck",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (bool)method.Invoke(
                world,
                new object[] { self, candidate, inputPhase, candidateY });
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

        private static LooseQuadtreeBroadphase BuildTree(
            List<IncrementalSpatialEntry> entries)
        {
            var tree = new LooseQuadtreeBroadphase(4, 8);
            SpatialSynchronizeResult result = tree.Synchronize(
                entries,
                new SpatialAabbXZ(-32768, -32768, 32768, 32768));
            Assert.That(result.Succeeded, Is.True);
            return tree;
        }

        private static void AssertNearest(
            LooseQuadtreeBroadphase tree,
            ref SlotFilter filter,
            int pointX,
            int pointZ,
            int maxDistanceExclusive,
            int maxAbsXExclusive,
            int maxAbsZExclusive,
            int expectedSlot,
            int expectedDistance)
        {
            bool succeeded = tree.TryFindNearestPointManhattan(
                pointX,
                pointZ,
                maxDistanceExclusive,
                maxAbsXExclusive,
                maxAbsZExclusive,
                ref filter,
                out RuntimeEntityHandle handle,
                out int distance,
                out _);
            Assert.That(succeeded, Is.True);
            Assert.That(handle.Slot, Is.EqualTo(expectedSlot));
            Assert.That(distance, Is.EqualTo(expectedDistance));
        }

        private static void BruteNearest(
            List<IncrementalSpatialEntry> entries,
            bool[] accepted,
            int pointX,
            int pointZ,
            int maxDistanceExclusive,
            int maxAbsXExclusive,
            int maxAbsZExclusive,
            out RuntimeEntityHandle selected,
            out int selectedDistance)
        {
            selected = RuntimeEntityHandle.Invalid;
            selectedDistance = maxDistanceExclusive;
            for (int i = 0; i < entries.Count; i++)
            {
                IncrementalSpatialEntry entry = entries[i];
                if (!accepted[entry.Handle.Slot])
                    continue;
                int deltaX = Math.Abs(entry.Bounds.MinX - pointX);
                int deltaZ = Math.Abs(entry.Bounds.MinZ - pointZ);
                int distance = deltaX + deltaZ;
                if (deltaX >= maxAbsXExclusive ||
                    deltaZ >= maxAbsZExclusive ||
                    distance >= maxDistanceExclusive)
                {
                    continue;
                }
                if (distance < selectedDistance ||
                    (distance == selectedDistance &&
                     selected.IsValid &&
                     entry.Handle.Slot < selected.Slot))
                {
                    selected = entry.Handle;
                    selectedDistance = distance;
                }
            }
        }
    }
}
#endif
