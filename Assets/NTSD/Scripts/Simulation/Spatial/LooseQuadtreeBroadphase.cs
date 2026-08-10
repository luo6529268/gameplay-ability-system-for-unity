using System;
using System.Collections.Generic;

namespace NTSD.Simulation.Spatial
{
    public enum IncrementalPointFilterDecision
    {
        Reject = 0,
        Accept = 1,
        Abort = 2,
    }

    public interface IIncrementalPointNearestFilter
    {
        IncrementalPointFilterDecision Evaluate(RuntimeEntityHandle handle);
    }

    public sealed class LooseQuadtreeBroadphase : ISpatialBroadphase, IIncrementalSpatialBroadphase
    {
        public const int DefaultLeafCapacity = 16;
        public const int DefaultMaxDepth = 8;
        public const int MinimumRootSide = 256;

        private sealed class Node
        {
            public long MinX;
            public long MinZ;
            public long Side;
            public int Depth;
            public int Child0 = -1;
            public int Child1 = -1;
            public int Child2 = -1;
            public int Child3 = -1;
            public int FirstEntryIndex = -1;
            public int LastEntryIndex = -1;
            public int EntryCount;

            public bool HasChildren => Child0 >= 0;

            public void Reset(long minX, long minZ, long side, int depth)
            {
                MinX = minX;
                MinZ = minZ;
                Side = side;
                Depth = depth;
                Child0 = -1;
                Child1 = -1;
                Child2 = -1;
                Child3 = -1;
                ClearEntries();
            }

            public void ClearEntries()
            {
                FirstEntryIndex = -1;
                LastEntryIndex = -1;
                EntryCount = 0;
            }

            public int ChildAt(int index)
            {
                return index switch
                {
                    0 => Child0,
                    1 => Child1,
                    2 => Child2,
                    _ => Child3,
                };
            }
        }

        private sealed class IncrementalNode
        {
            public long MinX;
            public long MinZ;
            public long Side;
            public int Depth;
            public int Child0 = -1;
            public int Child1 = -1;
            public int Child2 = -1;
            public int Child3 = -1;
            public int FirstRecordIndex = -1;
            public int LastRecordIndex = -1;
            public int EntryCount;

            public bool HasChildren => Child0 >= 0;

            public void Reset(long minX, long minZ, long side, int depth)
            {
                MinX = minX;
                MinZ = minZ;
                Side = side;
                Depth = depth;
                Child0 = -1;
                Child1 = -1;
                Child2 = -1;
                Child3 = -1;
                ClearEntries();
            }

            public void ClearEntries()
            {
                FirstRecordIndex = -1;
                LastRecordIndex = -1;
                EntryCount = 0;
            }

            public int ChildAt(int index)
            {
                return index switch
                {
                    0 => Child0,
                    1 => Child1,
                    2 => Child2,
                    _ => Child3,
                };
            }
        }

        private sealed class IncrementalRecord
        {
            public RuntimeEntityHandle Handle;
            public SpatialAabbXZ Bounds;
            public int NodeIndex = -1;
            public int PreviousNodeRecordIndex = -1;
            public int NextNodeRecordIndex = -1;
            public int ValidationToken;
            public bool Active;

            public void Reset(RuntimeEntityHandle handle, in SpatialAabbXZ bounds)
            {
                Handle = handle;
                Bounds = bounds;
                NodeIndex = -1;
                PreviousNodeRecordIndex = -1;
                NextNodeRecordIndex = -1;
                ValidationToken = 0;
                Active = true;
            }

            public void Release()
            {
                Handle = RuntimeEntityHandle.Invalid;
                Bounds = default;
                NodeIndex = -1;
                PreviousNodeRecordIndex = -1;
                NextNodeRecordIndex = -1;
                ValidationToken = 0;
                Active = false;
            }
        }

        private struct NearestNodeQueueEntry
        {
            public int NodeIndex;
            public int ManhattanLowerBound;
            public int XLowerBound;
            public int ZLowerBound;
        }

        private readonly int leafCapacity;
        private readonly int maxDepth;
        private Node[] nodes = new Node[16];
        private SpatialBroadphaseEntry[] entries = new SpatialBroadphaseEntry[32];
        private int[] entryNextIndices = new int[32];
        private int nodeCount;
        private int entryCount;
        private IncrementalNode[] incrementalNodes = new IncrementalNode[16];
        private IncrementalRecord[] incrementalRecords = new IncrementalRecord[32];
        private readonly Dictionary<RuntimeEntityHandle, int> incrementalHandleToRecord =
            new Dictionary<RuntimeEntityHandle, int>();
        private readonly HashSet<RuntimeEntityHandle> incrementalDesiredHandles =
            new HashSet<RuntimeEntityHandle>();
        private readonly HashSet<int> incrementalDesiredSlots = new HashSet<int>();
        private readonly List<int> incrementalStaleRecords = new List<int>(32);
        private readonly Stack<int> incrementalFreeRecords = new Stack<int>(32);
        private int incrementalNodeCount;
        private int incrementalRecordCount;
        private int incrementalActiveCount;
        private int incrementalValidationToken;
        private bool incrementalInitialized;
        private NearestNodeQueueEntry[] nearestNodeQueue =
            new NearestNodeQueueEntry[16];
        private int nearestNodeQueueCount;

        public LooseQuadtreeBroadphase(
            int leafCapacity = DefaultLeafCapacity,
            int maxDepth = DefaultMaxDepth)
        {
            if (leafCapacity < 1)
                throw new ArgumentOutOfRangeException(nameof(leafCapacity));
            if (maxDepth < 0 || maxDepth > 30)
                throw new ArgumentOutOfRangeException(nameof(maxDepth));

            this.leafCapacity = leafCapacity;
            this.maxDepth = maxDepth;
        }

        /// <summary>
        /// Reserves the managed storage used by the broadphase before battle starts.
        /// It does not publish an index and therefore does not change query results.
        /// </summary>
        public void PrepareCapacity(int entryCapacity)
        {
            if (entryCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(entryCapacity));

            int normalizedEntryCapacity = Math.Max(32, entryCapacity);
            int nodeCapacity = checked(
                Math.Max(16, normalizedEntryCapacity * 4 + 1));

            EnsureEntryCapacity(normalizedEntryCapacity);
            EnsureNodeCapacity(nodeCapacity);
            EnsureIncrementalRecordCapacity(normalizedEntryCapacity);
            EnsureIncrementalNodeCapacity(nodeCapacity);
            EnsureNearestNodeQueueCapacity(nodeCapacity);

            incrementalHandleToRecord.EnsureCapacity(normalizedEntryCapacity);
            incrementalDesiredHandles.EnsureCapacity(normalizedEntryCapacity);
            incrementalDesiredSlots.EnsureCapacity(normalizedEntryCapacity);
            if (incrementalStaleRecords.Capacity < normalizedEntryCapacity)
                incrementalStaleRecords.Capacity = normalizedEntryCapacity;

            for (int index = 0; index < nodeCapacity; index++)
            {
                nodes[index] ??= new Node();
                incrementalNodes[index] ??= new IncrementalNode();
            }
            for (int index = 0; index < normalizedEntryCapacity; index++)
                incrementalRecords[index] ??= new IncrementalRecord();

        }

        internal int NodeCount => nodeCount;
        internal int EntryCount => entryCount;
        internal int RootRetainedEntryCount => nodeCount > 0 ? nodes[0].EntryCount : 0;
        internal int MaximumObservedDepth { get; private set; }
        internal long RootMinX => nodeCount > 0 ? nodes[0].MinX : 0;
        internal long RootMinZ => nodeCount > 0 ? nodes[0].MinZ : 0;
        internal long RootSide => nodeCount > 0 ? nodes[0].Side : 0;
        internal int IncrementalNodeCount => incrementalNodeCount;
        internal int IncrementalIndexedCount => incrementalActiveCount;
        internal int IncrementalFullRebuildCount { get; private set; }
        internal int IncrementalMigrationCount { get; private set; }
        internal int IncrementalInPlaceUpdateCount { get; private set; }
        internal long IncrementalRootMinX =>
            incrementalNodeCount > 0 ? incrementalNodes[0].MinX : 0;
        internal long IncrementalRootMinZ =>
            incrementalNodeCount > 0 ? incrementalNodes[0].MinZ : 0;
        internal long IncrementalRootSide =>
            incrementalNodeCount > 0 ? incrementalNodes[0].Side : 0;

        public SpatialSynchronizeResult Synchronize(
            IReadOnlyList<IncrementalSpatialEntry> sourceEntries,
            in SpatialAabbXZ preferredRoot)
        {
            try
            {
                int sourceCount = sourceEntries?.Count ?? 0;
                incrementalDesiredHandles.Clear();
                incrementalDesiredSlots.Clear();
                bool requiresFullRebuild = !incrementalInitialized || incrementalNodeCount == 0;

                for (int i = 0; i < sourceCount; i++)
                {
                    IncrementalSpatialEntry entry = sourceEntries[i];
                    if (!entry.Handle.IsValid || !entry.Bounds.IsValid ||
                        !incrementalDesiredHandles.Add(entry.Handle) ||
                        !incrementalDesiredSlots.Add(entry.Handle.Slot))
                    {
                        ResetIncremental();
                        return SpatialSynchronizeResult.Failed;
                    }

                    if (!requiresFullRebuild &&
                        !ContainsInLooseBounds(incrementalNodes[0], entry.Bounds))
                    {
                        requiresFullRebuild = true;
                    }
                }

                if (requiresFullRebuild)
                    return RebuildIncremental(sourceEntries, preferredRoot);

                int insertedCount = 0;
                int updatedInPlaceCount = 0;
                int migratedCount = 0;
                int removedCount = 0;

                incrementalStaleRecords.Clear();
                foreach (KeyValuePair<RuntimeEntityHandle, int> pair in incrementalHandleToRecord)
                {
                    if (!incrementalDesiredHandles.Contains(pair.Key))
                        incrementalStaleRecords.Add(pair.Value);
                }

                for (int i = 0; i < incrementalStaleRecords.Count; i++)
                {
                    if (!RemoveIncrementalRecord(incrementalStaleRecords[i]))
                    {
                        ResetIncremental();
                        return SpatialSynchronizeResult.Failed;
                    }
                    removedCount++;
                }

                for (int i = 0; i < sourceCount; i++)
                {
                    IncrementalSpatialEntry entry = sourceEntries[i];
                    if (!incrementalHandleToRecord.TryGetValue(entry.Handle, out int recordIndex))
                    {
                        recordIndex = AllocateIncrementalRecord(entry.Handle, entry.Bounds);
                        InsertIncremental(0, recordIndex);
                        insertedCount++;
                        continue;
                    }

                    IncrementalRecord record = incrementalRecords[recordIndex];
                    if (record == null || !record.Active || record.Handle != entry.Handle ||
                        record.NodeIndex < 0 || record.NodeIndex >= incrementalNodeCount)
                    {
                        ResetIncremental();
                        return SpatialSynchronizeResult.Failed;
                    }

                    if (record.Bounds.Equals(entry.Bounds))
                        continue;

                    if (ContainsInLooseBounds(incrementalNodes[record.NodeIndex], entry.Bounds))
                    {
                        record.Bounds = entry.Bounds;
                        updatedInPlaceCount++;
                        IncrementalInPlaceUpdateCount++;
                        continue;
                    }

                    if (!RemoveRecordIndexFromNode(record.NodeIndex, recordIndex))
                    {
                        ResetIncremental();
                        return SpatialSynchronizeResult.Failed;
                    }

                    record.Bounds = entry.Bounds;
                    record.NodeIndex = -1;
                    InsertIncremental(0, recordIndex);
                    migratedCount++;
                    IncrementalMigrationCount++;
                }

                if (!ValidateIncrementalInvariants())
                {
                    ResetIncremental();
                    return SpatialSynchronizeResult.Failed;
                }

                return new SpatialSynchronizeResult(
                    true,
                    false,
                    insertedCount,
                    updatedInPlaceCount,
                    migratedCount,
                    removedCount,
                    incrementalActiveCount);
            }
            catch (Exception)
            {
                ResetIncremental();
                return SpatialSynchronizeResult.Failed;
            }
        }

        public void QueryHandles(
            in SpatialAabbXZ bounds,
            List<RuntimeEntityHandle> resultHandles)
        {
            if (resultHandles == null)
                throw new ArgumentNullException(nameof(resultHandles));

            resultHandles.Clear();
            if (!bounds.IsValid || !incrementalInitialized || incrementalNodeCount == 0)
                return;

            QueryIncrementalNode(0, bounds, resultHandles);
        }

        public bool TryFindNearestPointManhattan<TFilter>(
            int pointX,
            int pointZ,
            int maxDistanceExclusive,
            int maxAbsXExclusive,
            int maxAbsZExclusive,
            ref TFilter filter,
            out RuntimeEntityHandle nearestHandle,
            out int nearestDistance,
            out int visitedRecordCount)
            where TFilter : struct, IIncrementalPointNearestFilter
        {
            nearestHandle = RuntimeEntityHandle.Invalid;
            nearestDistance = maxDistanceExclusive;
            visitedRecordCount = 0;
            nearestNodeQueueCount = 0;

            if (!incrementalInitialized || incrementalNodeCount == 0 ||
                maxDistanceExclusive <= 0 ||
                maxAbsXExclusive <= 0 ||
                maxAbsZExclusive <= 0)
            {
                return incrementalInitialized && incrementalNodeCount > 0;
            }

            try
            {
                NearestNodeQueueEntry root = CreateNearestQueueEntry(
                    0,
                    pointX,
                    pointZ);
                if (CanNearestNodeContainCandidate(
                    root,
                    nearestDistance,
                    maxDistanceExclusive,
                    maxAbsXExclusive,
                    maxAbsZExclusive))
                {
                    PushNearestNode(root);
                }

                while (nearestNodeQueueCount > 0)
                {
                    NearestNodeQueueEntry queued = PopNearestNode();
                    if (queued.ManhattanLowerBound > nearestDistance)
                        break;
                    if (!CanNearestNodeContainCandidate(
                        queued,
                        nearestDistance,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive))
                    {
                        continue;
                    }

                    if (queued.NodeIndex < 0 || queued.NodeIndex >= incrementalNodeCount)
                        return false;

                    IncrementalNode node = incrementalNodes[queued.NodeIndex];
                    if (node == null)
                        return false;

                    int recordIndex = node.FirstRecordIndex;
                    int visitedInNode = 0;
                    while (recordIndex >= 0)
                    {
                        if (++visitedInNode > node.EntryCount)
                            return false;
                        if (recordIndex < 0 || recordIndex >= incrementalRecordCount)
                            return false;

                        IncrementalRecord record = incrementalRecords[recordIndex];
                        if (record == null || !record.Active ||
                            record.NodeIndex != queued.NodeIndex ||
                            !record.Bounds.IsValid)
                        {
                            return false;
                        }
                        recordIndex = record.NextNodeRecordIndex;

                        visitedRecordCount++;
                        int deltaX = SaturatingAbsDifference(record.Bounds.MinX, pointX);
                        if (deltaX >= maxAbsXExclusive)
                            continue;
                        int deltaZ = SaturatingAbsDifference(record.Bounds.MinZ, pointZ);
                        if (deltaZ >= maxAbsZExclusive)
                            continue;
                        int distance = SaturatingAdd(deltaX, deltaZ);
                        if (distance >= maxDistanceExclusive ||
                            distance > nearestDistance)
                        {
                            continue;
                        }

                        IncrementalPointFilterDecision decision =
                            filter.Evaluate(record.Handle);
                        if (decision == IncrementalPointFilterDecision.Abort)
                            return false;
                        if (decision != IncrementalPointFilterDecision.Accept)
                            continue;

                        if (distance < nearestDistance ||
                            (distance == nearestDistance &&
                             nearestHandle.IsValid &&
                             record.Handle.Slot < nearestHandle.Slot))
                        {
                            nearestDistance = distance;
                            nearestHandle = record.Handle;
                        }

                    }

                    if (visitedInNode != node.EntryCount)
                        return false;

                    if (!node.HasChildren)
                        continue;

                    PushNearestChildIfRelevant(
                        node.Child0,
                        pointX,
                        pointZ,
                        nearestDistance,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive);
                    PushNearestChildIfRelevant(
                        node.Child1,
                        pointX,
                        pointZ,
                        nearestDistance,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive);
                    PushNearestChildIfRelevant(
                        node.Child2,
                        pointX,
                        pointZ,
                        nearestDistance,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive);
                    PushNearestChildIfRelevant(
                        node.Child3,
                        pointX,
                        pointZ,
                        nearestDistance,
                        maxDistanceExclusive,
                        maxAbsXExclusive,
                        maxAbsZExclusive);
                }

                return true;
            }
            finally
            {
                nearestNodeQueueCount = 0;
            }
        }

        public bool TryUpsertIncremental(
            RuntimeEntityHandle handle,
            in SpatialAabbXZ bounds)
        {
            if (!handle.IsValid || !bounds.IsValid ||
                !incrementalInitialized || incrementalNodeCount == 0 ||
                !ContainsInLooseBounds(incrementalNodes[0], bounds))
            {
                return false;
            }

            try
            {
                if (!incrementalHandleToRecord.TryGetValue(handle, out int recordIndex))
                {
                    recordIndex = AllocateIncrementalRecord(handle, bounds);
                    InsertIncremental(0, recordIndex);
                    return true;
                }

                if (recordIndex < 0 || recordIndex >= incrementalRecordCount)
                    return false;
                IncrementalRecord record = incrementalRecords[recordIndex];
                if (record == null || !record.Active || record.Handle != handle ||
                    record.NodeIndex < 0 || record.NodeIndex >= incrementalNodeCount)
                {
                    return false;
                }
                if (record.Bounds.Equals(bounds))
                    return true;

                if (ContainsInLooseBounds(incrementalNodes[record.NodeIndex], bounds))
                {
                    record.Bounds = bounds;
                    IncrementalInPlaceUpdateCount++;
                    return true;
                }

                if (!RemoveRecordIndexFromNode(record.NodeIndex, recordIndex))
                    return false;
                record.Bounds = bounds;
                record.NodeIndex = -1;
                InsertIncremental(0, recordIndex);
                IncrementalMigrationCount++;
                return true;
            }
            catch
            {
                ResetIncremental();
                return false;
            }
        }

        public bool TryRemoveIncremental(RuntimeEntityHandle handle)
        {
            if (!handle.IsValid || !incrementalInitialized ||
                !incrementalHandleToRecord.TryGetValue(handle, out int recordIndex))
            {
                return false;
            }

            try
            {
                return RemoveIncrementalRecord(recordIndex);
            }
            catch
            {
                ResetIncremental();
                return false;
            }
        }

        public void ResetIncremental()
        {
            for (int i = 0; i < incrementalNodeCount; i++)
                incrementalNodes[i]?.ClearEntries();
            for (int i = 0; i < incrementalRecordCount; i++)
                incrementalRecords[i]?.Release();

            incrementalHandleToRecord.Clear();
            incrementalDesiredHandles.Clear();
            incrementalDesiredSlots.Clear();
            incrementalStaleRecords.Clear();
            incrementalFreeRecords.Clear();
            incrementalNodeCount = 0;
            incrementalRecordCount = 0;
            incrementalActiveCount = 0;
            incrementalInitialized = false;
        }

        private void PushNearestChildIfRelevant(
            int nodeIndex,
            int pointX,
            int pointZ,
            int nearestDistance,
            int maxDistanceExclusive,
            int maxAbsXExclusive,
            int maxAbsZExclusive)
        {
            NearestNodeQueueEntry entry = CreateNearestQueueEntry(
                nodeIndex,
                pointX,
                pointZ);
            if (CanNearestNodeContainCandidate(
                entry,
                nearestDistance,
                maxDistanceExclusive,
                maxAbsXExclusive,
                maxAbsZExclusive))
            {
                PushNearestNode(entry);
            }
        }

        private NearestNodeQueueEntry CreateNearestQueueEntry(
            int nodeIndex,
            int pointX,
            int pointZ)
        {
            if (nodeIndex < 0 || nodeIndex >= incrementalNodeCount)
            {
                return new NearestNodeQueueEntry
                {
                    NodeIndex = nodeIndex,
                    ManhattanLowerBound = int.MaxValue,
                    XLowerBound = int.MaxValue,
                    ZLowerBound = int.MaxValue,
                };
            }

            IncrementalNode node = incrementalNodes[nodeIndex];
            long looseMinX4 = node.MinX * 4 - node.Side;
            long looseMinZ4 = node.MinZ * 4 - node.Side;
            long looseMaxX4 = (node.MinX + node.Side) * 4 + node.Side;
            long looseMaxZ4 = (node.MinZ + node.Side) * 4 + node.Side;
            int xLowerBound = DistanceToQuarterInterval(
                (long)pointX * 4,
                looseMinX4,
                looseMaxX4);
            int zLowerBound = DistanceToQuarterInterval(
                (long)pointZ * 4,
                looseMinZ4,
                looseMaxZ4);
            return new NearestNodeQueueEntry
            {
                NodeIndex = nodeIndex,
                ManhattanLowerBound = SaturatingAdd(xLowerBound, zLowerBound),
                XLowerBound = xLowerBound,
                ZLowerBound = zLowerBound,
            };
        }

        private static bool CanNearestNodeContainCandidate(
            in NearestNodeQueueEntry entry,
            int nearestDistance,
            int maxDistanceExclusive,
            int maxAbsXExclusive,
            int maxAbsZExclusive)
        {
            return entry.ManhattanLowerBound <= nearestDistance &&
                   entry.ManhattanLowerBound < maxDistanceExclusive &&
                   entry.XLowerBound < maxAbsXExclusive &&
                   entry.ZLowerBound < maxAbsZExclusive;
        }

        private void PushNearestNode(in NearestNodeQueueEntry entry)
        {
            EnsureNearestNodeQueueCapacity(nearestNodeQueueCount + 1);
            int index = nearestNodeQueueCount++;
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (CompareNearestQueueEntries(nearestNodeQueue[parent], entry) <= 0)
                    break;
                nearestNodeQueue[index] = nearestNodeQueue[parent];
                index = parent;
            }
            nearestNodeQueue[index] = entry;
        }

        private NearestNodeQueueEntry PopNearestNode()
        {
            NearestNodeQueueEntry result = nearestNodeQueue[0];
            int lastIndex = --nearestNodeQueueCount;
            if (lastIndex <= 0)
                return result;

            NearestNodeQueueEntry replacement = nearestNodeQueue[lastIndex];
            int index = 0;
            while (true)
            {
                int left = index * 2 + 1;
                if (left >= nearestNodeQueueCount)
                    break;
                int right = left + 1;
                int child = right < nearestNodeQueueCount &&
                            CompareNearestQueueEntries(
                                nearestNodeQueue[right],
                                nearestNodeQueue[left]) < 0
                    ? right
                    : left;
                if (CompareNearestQueueEntries(replacement, nearestNodeQueue[child]) <= 0)
                    break;
                nearestNodeQueue[index] = nearestNodeQueue[child];
                index = child;
            }
            nearestNodeQueue[index] = replacement;
            return result;
        }

        private static int CompareNearestQueueEntries(
            in NearestNodeQueueEntry left,
            in NearestNodeQueueEntry right)
        {
            int comparison = left.ManhattanLowerBound.CompareTo(
                right.ManhattanLowerBound);
            if (comparison != 0)
                return comparison;
            comparison = left.XLowerBound.CompareTo(right.XLowerBound);
            if (comparison != 0)
                return comparison;
            comparison = left.ZLowerBound.CompareTo(right.ZLowerBound);
            return comparison != 0
                ? comparison
                : left.NodeIndex.CompareTo(right.NodeIndex);
        }

        private void EnsureNearestNodeQueueCapacity(int required)
        {
            if (required <= nearestNodeQueue.Length)
                return;

            int capacity = nearestNodeQueue.Length;
            while (capacity < required)
                capacity *= 2;
            Array.Resize(ref nearestNodeQueue, capacity);
        }

        private static int DistanceToQuarterInterval(
            long point4,
            long min4,
            long max4)
        {
            long quarterDistance = point4 < min4
                ? min4 - point4
                : point4 > max4
                    ? point4 - max4
                    : 0;
            long distance = quarterDistance / 4;
            return distance >= int.MaxValue ? int.MaxValue : (int)distance;
        }

        private static int SaturatingAbsDifference(int left, int right)
        {
            long difference = Math.Abs((long)left - right);
            return difference >= int.MaxValue ? int.MaxValue : (int)difference;
        }

        private static int SaturatingAdd(int left, int right)
        {
            long sum = (long)left + right;
            return sum >= int.MaxValue ? int.MaxValue : (int)sum;
        }

        private SpatialSynchronizeResult RebuildIncremental(
            IReadOnlyList<IncrementalSpatialEntry> sourceEntries,
            in SpatialAabbXZ preferredRoot)
        {
            int previousCount = incrementalActiveCount;
            for (int i = 0; i < incrementalNodeCount; i++)
                incrementalNodes[i]?.ClearEntries();
            for (int i = 0; i < incrementalRecordCount; i++)
                incrementalRecords[i]?.Release();

            incrementalHandleToRecord.Clear();
            incrementalFreeRecords.Clear();
            incrementalNodeCount = 0;
            incrementalRecordCount = 0;
            incrementalActiveCount = 0;

            long minX = preferredRoot.IsValid ? preferredRoot.MinX : 0;
            long minZ = preferredRoot.IsValid ? preferredRoot.MinZ : 0;
            long maxX = preferredRoot.IsValid ? preferredRoot.MaxX : 1;
            long maxZ = preferredRoot.IsValid ? preferredRoot.MaxZ : 1;
            int sourceCount = sourceEntries?.Count ?? 0;
            for (int i = 0; i < sourceCount; i++)
            {
                SpatialAabbXZ bounds = sourceEntries[i].Bounds;
                if (bounds.MinX < minX) minX = bounds.MinX;
                if (bounds.MinZ < minZ) minZ = bounds.MinZ;
                if (bounds.MaxX > maxX) maxX = bounds.MaxX;
                if (bounds.MaxZ > maxZ) maxZ = bounds.MaxZ;
            }

            ResolveRoot(minX, minZ, maxX, maxZ,
                out long rootMinX, out long rootMinZ, out long rootSide);
            CreateIncrementalNode(rootMinX, rootMinZ, rootSide, 0);

            for (int i = 0; i < sourceCount; i++)
            {
                IncrementalSpatialEntry entry = sourceEntries[i];
                int recordIndex = AllocateIncrementalRecord(entry.Handle, entry.Bounds);
                InsertIncremental(0, recordIndex);
            }

            incrementalInitialized = true;
            IncrementalFullRebuildCount++;
            if (!ValidateIncrementalInvariants())
            {
                ResetIncremental();
                return SpatialSynchronizeResult.Failed;
            }

            return new SpatialSynchronizeResult(
                true,
                true,
                sourceCount,
                0,
                0,
                previousCount,
                incrementalActiveCount);
        }

        private int AllocateIncrementalRecord(
            RuntimeEntityHandle handle,
            in SpatialAabbXZ bounds)
        {
            int recordIndex;
            if (incrementalFreeRecords.Count > 0)
            {
                recordIndex = incrementalFreeRecords.Pop();
            }
            else
            {
                recordIndex = incrementalRecordCount++;
                EnsureIncrementalRecordCapacity(incrementalRecordCount);
            }

            IncrementalRecord record = incrementalRecords[recordIndex] ??= new IncrementalRecord();
            record.Reset(handle, bounds);
            if (!incrementalHandleToRecord.TryAdd(handle, recordIndex))
                throw new InvalidOperationException("Duplicate incremental spatial handle.");
            incrementalActiveCount++;
            return recordIndex;
        }

        private bool RemoveIncrementalRecord(int recordIndex)
        {
            if (recordIndex < 0 || recordIndex >= incrementalRecordCount)
                return false;

            IncrementalRecord record = incrementalRecords[recordIndex];
            if (record == null || !record.Active ||
                !incrementalHandleToRecord.TryGetValue(record.Handle, out int mappedIndex) ||
                mappedIndex != recordIndex ||
                !RemoveRecordIndexFromNode(record.NodeIndex, recordIndex))
            {
                return false;
            }

            incrementalHandleToRecord.Remove(record.Handle);
            record.Release();
            incrementalFreeRecords.Push(recordIndex);
            incrementalActiveCount--;
            return true;
        }

        private bool RemoveRecordIndexFromNode(int nodeIndex, int recordIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= incrementalNodeCount)
                return false;

            if (recordIndex < 0 || recordIndex >= incrementalRecordCount)
                return false;

            IncrementalNode node = incrementalNodes[nodeIndex];
            IncrementalRecord record = incrementalRecords[recordIndex];
            if (node == null || record == null || !record.Active ||
                record.NodeIndex != nodeIndex || node.EntryCount <= 0)
            {
                return false;
            }

            int previousRecordIndex = record.PreviousNodeRecordIndex;
            int nextRecordIndex = record.NextNodeRecordIndex;
            IncrementalRecord previous = null;
            IncrementalRecord next = null;
            if (previousRecordIndex >= 0)
            {
                if (previousRecordIndex >= incrementalRecordCount)
                    return false;
                previous = incrementalRecords[previousRecordIndex];
                if (previous == null || !previous.Active ||
                    previous.NodeIndex != nodeIndex ||
                    previous.NextNodeRecordIndex != recordIndex)
                {
                    return false;
                }
            }
            else if (node.FirstRecordIndex != recordIndex)
            {
                return false;
            }

            if (nextRecordIndex >= 0)
            {
                if (nextRecordIndex >= incrementalRecordCount)
                    return false;
                next = incrementalRecords[nextRecordIndex];
                if (next == null || !next.Active ||
                    next.NodeIndex != nodeIndex ||
                    next.PreviousNodeRecordIndex != recordIndex)
                {
                    return false;
                }
            }
            else if (node.LastRecordIndex != recordIndex)
            {
                return false;
            }

            if (previous != null)
                previous.NextNodeRecordIndex = nextRecordIndex;
            else
                node.FirstRecordIndex = nextRecordIndex;
            if (next != null)
                next.PreviousNodeRecordIndex = previousRecordIndex;
            else
                node.LastRecordIndex = previousRecordIndex;

            node.EntryCount--;
            if (node.EntryCount == 0)
            {
                node.FirstRecordIndex = -1;
                node.LastRecordIndex = -1;
            }

            record.NodeIndex = -1;
            record.PreviousNodeRecordIndex = -1;
            record.NextNodeRecordIndex = -1;
            return true;
        }

        private void InsertIncremental(int nodeIndex, int recordIndex)
        {
            IncrementalNode node = incrementalNodes[nodeIndex];
            IncrementalRecord record = incrementalRecords[recordIndex];
            if (node.HasChildren)
            {
                int childIndex = ResolveContainingIncrementalChild(node, record.Bounds);
                if (childIndex >= 0)
                {
                    InsertIncremental(childIndex, recordIndex);
                    return;
                }
            }

            AppendRecordIndexToNode(nodeIndex, recordIndex);
            if (!node.HasChildren && node.EntryCount > leafCapacity &&
                node.Depth < maxDepth && node.Side > 1)
            {
                SplitIncremental(nodeIndex);
            }
        }

        private void AppendRecordIndexToNode(int nodeIndex, int recordIndex)
        {
            IncrementalNode node = incrementalNodes[nodeIndex];
            IncrementalRecord record = incrementalRecords[recordIndex];
            if (record == null || !record.Active || record.NodeIndex >= 0 ||
                record.PreviousNodeRecordIndex >= 0 ||
                record.NextNodeRecordIndex >= 0)
            {
                throw new InvalidOperationException(
                    "Incremental quadtree record is already linked to a node.");
            }

            int previousRecordIndex = node.LastRecordIndex;
            record.NodeIndex = nodeIndex;
            record.PreviousNodeRecordIndex = previousRecordIndex;
            if (previousRecordIndex >= 0)
            {
                incrementalRecords[previousRecordIndex].NextNodeRecordIndex =
                    recordIndex;
            }
            else
            {
                node.FirstRecordIndex = recordIndex;
            }

            node.LastRecordIndex = recordIndex;
            node.EntryCount++;
        }

        private void SplitIncremental(int nodeIndex)
        {
            IncrementalNode node = incrementalNodes[nodeIndex];
            long childSide = node.Side / 2;
            if (childSide <= 0)
                return;

            int depth = node.Depth + 1;
            node.Child0 = CreateIncrementalNode(node.MinX, node.MinZ, childSide, depth);
            node.Child1 = CreateIncrementalNode(node.MinX + childSide, node.MinZ, childSide, depth);
            node.Child2 = CreateIncrementalNode(node.MinX, node.MinZ + childSide, childSide, depth);
            node.Child3 = CreateIncrementalNode(
                node.MinX + childSide,
                node.MinZ + childSide,
                childSide,
                depth);

            int originalEntryCount = node.EntryCount;
            int visited = 0;
            int recordIndex = node.FirstRecordIndex;
            while (recordIndex >= 0)
            {
                if (++visited > originalEntryCount)
                {
                    throw new InvalidOperationException(
                        "Incremental quadtree node membership contains a cycle.");
                }

                IncrementalRecord record = incrementalRecords[recordIndex];
                int nextRecordIndex = record.NextNodeRecordIndex;
                int childIndex = ResolveContainingIncrementalChild(
                    node,
                    record.Bounds);
                if (childIndex >= 0)
                {
                    if (!RemoveRecordIndexFromNode(nodeIndex, recordIndex))
                    {
                        throw new InvalidOperationException(
                            "Incremental quadtree split could not detach a record.");
                    }
                    InsertIncremental(childIndex, recordIndex);
                }

                recordIndex = nextRecordIndex;
            }

            if (visited != originalEntryCount)
            {
                throw new InvalidOperationException(
                    "Incremental quadtree node membership count is stale.");
            }
        }

        private int ResolveContainingIncrementalChild(
            IncrementalNode node,
            in SpatialAabbXZ bounds)
        {
            if (!node.HasChildren)
                return -1;

            long midpointX = node.MinX + node.Side / 2;
            long midpointZ = node.MinZ + node.Side / 2;
            long centerX2 = (long)bounds.MinX + bounds.MaxX;
            long centerZ2 = (long)bounds.MinZ + bounds.MaxZ;
            int east = centerX2 >= midpointX * 2 ? 1 : 0;
            int south = centerZ2 >= midpointZ * 2 ? 1 : 0;
            int childIndex = node.ChildAt(east + south * 2);
            return ContainsInLooseBounds(incrementalNodes[childIndex], bounds) ? childIndex : -1;
        }

        private void QueryIncrementalNode(
            int nodeIndex,
            in SpatialAabbXZ query,
            List<RuntimeEntityHandle> result)
        {
            if (nodeIndex < 0 || nodeIndex >= incrementalNodeCount)
                throw new InvalidOperationException("Incremental quadtree contains an invalid child index.");

            IncrementalNode node = incrementalNodes[nodeIndex];
            if (!OverlapsNodeLooseBounds(node, query))
                return;

            int recordIndex = node.FirstRecordIndex;
            int visited = 0;
            while (recordIndex >= 0)
            {
                if (++visited > node.EntryCount)
                    throw new InvalidOperationException("Incremental quadtree node membership contains a cycle.");
                if (recordIndex < 0 || recordIndex >= incrementalRecordCount)
                    throw new InvalidOperationException("Incremental quadtree contains an invalid entry index.");

                IncrementalRecord record = incrementalRecords[recordIndex];
                if (record == null || !record.Active || record.NodeIndex != nodeIndex)
                    throw new InvalidOperationException("Incremental quadtree entry mapping is stale.");
                recordIndex = record.NextNodeRecordIndex;
                if (record.Bounds.Overlaps(query))
                    result.Add(record.Handle);
            }

            if (visited != node.EntryCount)
                throw new InvalidOperationException("Incremental quadtree node membership count is stale.");

            if (!node.HasChildren)
                return;

            QueryIncrementalNode(node.Child0, query, result);
            QueryIncrementalNode(node.Child1, query, result);
            QueryIncrementalNode(node.Child2, query, result);
            QueryIncrementalNode(node.Child3, query, result);
        }

        private bool ValidateIncrementalInvariants()
        {
            if (!incrementalInitialized && incrementalNodeCount != 1)
            {
                // A rebuild validates before publishing initialized=true.
                if (incrementalNodeCount != 1)
                    return false;
            }
            if (incrementalNodeCount <= 0 || incrementalHandleToRecord.Count != incrementalActiveCount)
                return false;

            int token = ++incrementalValidationToken;
            if (token == 0)
            {
                token = 1;
                incrementalValidationToken = token;
                for (int i = 0; i < incrementalRecordCount; i++)
                {
                    if (incrementalRecords[i] != null)
                        incrementalRecords[i].ValidationToken = 0;
                }
            }

            int visited = 0;
            for (int nodeIndex = 0; nodeIndex < incrementalNodeCount; nodeIndex++)
            {
                IncrementalNode node = incrementalNodes[nodeIndex];
                if (node == null)
                    return false;
                if (node.EntryCount < 0 ||
                    (node.EntryCount == 0 &&
                     (node.FirstRecordIndex >= 0 || node.LastRecordIndex >= 0)) ||
                    (node.EntryCount > 0 &&
                     (node.FirstRecordIndex < 0 || node.LastRecordIndex < 0)))
                {
                    return false;
                }
                if (node.HasChildren &&
                    (node.Child0 < 0 || node.Child1 < 0 || node.Child2 < 0 || node.Child3 < 0 ||
                     node.Child0 >= incrementalNodeCount || node.Child1 >= incrementalNodeCount ||
                     node.Child2 >= incrementalNodeCount || node.Child3 >= incrementalNodeCount))
                {
                    return false;
                }

                int nodeVisited = 0;
                int previousRecordIndex = -1;
                int recordIndex = node.FirstRecordIndex;
                while (recordIndex >= 0)
                {
                    if (++nodeVisited > node.EntryCount)
                        return false;
                    if (recordIndex < 0 || recordIndex >= incrementalRecordCount)
                        return false;
                    IncrementalRecord record = incrementalRecords[recordIndex];
                    if (record == null || !record.Active || record.NodeIndex != nodeIndex ||
                        record.PreviousNodeRecordIndex != previousRecordIndex ||
                        record.ValidationToken == token || !record.Bounds.IsValid)
                    {
                        return false;
                    }

                    record.ValidationToken = token;
                    previousRecordIndex = recordIndex;
                    recordIndex = record.NextNodeRecordIndex;
                    visited++;
                }

                if (nodeVisited != node.EntryCount ||
                    previousRecordIndex != node.LastRecordIndex)
                {
                    return false;
                }
            }

            if (visited != incrementalActiveCount)
                return false;

            foreach (KeyValuePair<RuntimeEntityHandle, int> pair in incrementalHandleToRecord)
            {
                if (pair.Value < 0 || pair.Value >= incrementalRecordCount)
                    return false;
                IncrementalRecord record = incrementalRecords[pair.Value];
                if (record == null || !record.Active || record.Handle != pair.Key ||
                    record.ValidationToken != token)
                {
                    return false;
                }
            }

            return true;
        }

        private int CreateIncrementalNode(long minX, long minZ, long side, int depth)
        {
            EnsureIncrementalNodeCapacity(incrementalNodeCount + 1);
            IncrementalNode node = incrementalNodes[incrementalNodeCount] ??= new IncrementalNode();
            node.Reset(minX, minZ, side, depth);
            return incrementalNodeCount++;
        }

        private void EnsureIncrementalNodeCapacity(int required)
        {
            if (required <= incrementalNodes.Length)
                return;

            int capacity = incrementalNodes.Length;
            while (capacity < required)
                capacity *= 2;
            Array.Resize(ref incrementalNodes, capacity);
        }

        private void EnsureIncrementalRecordCapacity(int required)
        {
            if (required <= incrementalRecords.Length)
                return;

            int capacity = incrementalRecords.Length;
            while (capacity < required)
                capacity *= 2;
            Array.Resize(ref incrementalRecords, capacity);
        }

        public void Rebuild(
            IReadOnlyList<SpatialBroadphaseEntry> sourceEntries,
            in SpatialAabbXZ preferredRoot)
        {
            int sourceCount = sourceEntries?.Count ?? 0;
            EnsureEntryCapacity(sourceCount);
            entryCount = 0;

            long minX = preferredRoot.IsValid ? preferredRoot.MinX : 0;
            long minZ = preferredRoot.IsValid ? preferredRoot.MinZ : 0;
            long maxX = preferredRoot.IsValid ? preferredRoot.MaxX : 1;
            long maxZ = preferredRoot.IsValid ? preferredRoot.MaxZ : 1;

            for (int i = 0; i < sourceCount; i++)
            {
                SpatialBroadphaseEntry entry = sourceEntries[i];
                if (!entry.Bounds.IsValid)
                    continue;

                entries[entryCount] = entry;
                entryNextIndices[entryCount] = -1;
                entryCount++;
                if (entry.Bounds.MinX < minX) minX = entry.Bounds.MinX;
                if (entry.Bounds.MinZ < minZ) minZ = entry.Bounds.MinZ;
                if (entry.Bounds.MaxX > maxX) maxX = entry.Bounds.MaxX;
                if (entry.Bounds.MaxZ > maxZ) maxZ = entry.Bounds.MaxZ;
            }

            ResolveRoot(minX, minZ, maxX, maxZ, out long rootMinX, out long rootMinZ, out long rootSide);
            nodeCount = 0;
            MaximumObservedDepth = 0;
            CreateNode(rootMinX, rootMinZ, rootSide, 0);

            for (int i = 0; i < entryCount; i++)
                Insert(0, i);
        }

        public void Query(in SpatialAabbXZ bounds, List<int> resultEntryIndices)
        {
            if (resultEntryIndices == null)
                throw new ArgumentNullException(nameof(resultEntryIndices));

            resultEntryIndices.Clear();
            if (!bounds.IsValid || nodeCount == 0)
                return;

            QueryNode(0, bounds, resultEntryIndices);
        }

        private void QueryNode(int nodeIndex, in SpatialAabbXZ query, List<int> result)
        {
            Node node = nodes[nodeIndex];
            if (!OverlapsNodeLooseBounds(node, query))
                return;

            int entryIndex = node.FirstEntryIndex;
            for (int visited = 0; visited < node.EntryCount; visited++)
            {
                if ((uint)entryIndex >= (uint)entryCount)
                    break;
                if (entries[entryIndex].Bounds.Overlaps(query))
                    result.Add(entries[entryIndex].InputIndex);
                entryIndex = entryNextIndices[entryIndex];
            }

            if (!node.HasChildren)
                return;

            QueryNode(node.Child0, query, result);
            QueryNode(node.Child1, query, result);
            QueryNode(node.Child2, query, result);
            QueryNode(node.Child3, query, result);
        }

        private void Insert(int nodeIndex, int entryIndex)
        {
            Node node = nodes[nodeIndex];
            if (node.HasChildren)
            {
                int childIndex = ResolveContainingChild(node, entries[entryIndex].Bounds);
                if (childIndex >= 0)
                {
                    Insert(childIndex, entryIndex);
                    return;
                }
            }

            AppendEntry(node, entryIndex);
            if (!node.HasChildren && node.EntryCount > leafCapacity && node.Depth < maxDepth && node.Side > 1)
                Split(nodeIndex);
        }

        private void Split(int nodeIndex)
        {
            Node node = nodes[nodeIndex];
            long childSide = node.Side / 2;
            if (childSide <= 0)
                return;

            int depth = node.Depth + 1;
            node.Child0 = CreateNode(node.MinX, node.MinZ, childSide, depth);
            node.Child1 = CreateNode(node.MinX + childSide, node.MinZ, childSide, depth);
            node.Child2 = CreateNode(node.MinX, node.MinZ + childSide, childSide, depth);
            node.Child3 = CreateNode(node.MinX + childSide, node.MinZ + childSide, childSide, depth);

            int entryIndex = node.FirstEntryIndex;
            int retainedCount = node.EntryCount;
            node.ClearEntries();
            for (int visited = 0; visited < retainedCount; visited++)
            {
                if ((uint)entryIndex >= (uint)entryCount)
                    break;

                int nextEntryIndex = entryNextIndices[entryIndex];
                entryNextIndices[entryIndex] = -1;
                int childIndex = ResolveContainingChild(node, entries[entryIndex].Bounds);
                if (childIndex >= 0)
                    Insert(childIndex, entryIndex);
                else
                    AppendEntry(node, entryIndex);
                entryIndex = nextEntryIndex;
            }
        }

        private void AppendEntry(Node node, int entryIndex)
        {
            entryNextIndices[entryIndex] = -1;
            if (node.LastEntryIndex >= 0)
                entryNextIndices[node.LastEntryIndex] = entryIndex;
            else
                node.FirstEntryIndex = entryIndex;

            node.LastEntryIndex = entryIndex;
            node.EntryCount++;
        }

        private int ResolveContainingChild(Node node, in SpatialAabbXZ bounds)
        {
            if (!node.HasChildren)
                return -1;

            long midpointX = node.MinX + node.Side / 2;
            long midpointZ = node.MinZ + node.Side / 2;
            long centerX2 = (long)bounds.MinX + bounds.MaxX;
            long centerZ2 = (long)bounds.MinZ + bounds.MaxZ;
            int east = centerX2 >= midpointX * 2 ? 1 : 0;
            int south = centerZ2 >= midpointZ * 2 ? 1 : 0;
            int childIndex = node.ChildAt(east + south * 2);
            return ContainsInLooseBounds(nodes[childIndex], bounds) ? childIndex : -1;
        }

        private static bool ContainsInLooseBounds(Node child, in SpatialAabbXZ bounds)
        {
            // Looseness 1.5 represented in quarter units: each side expands by childSide / 4.
            long looseMinX4 = child.MinX * 4 - child.Side;
            long looseMinZ4 = child.MinZ * 4 - child.Side;
            long looseMaxX4 = (child.MinX + child.Side) * 4 + child.Side;
            long looseMaxZ4 = (child.MinZ + child.Side) * 4 + child.Side;
            return (long)bounds.MinX * 4 >= looseMinX4 &&
                   (long)bounds.MaxX * 4 <= looseMaxX4 &&
                   (long)bounds.MinZ * 4 >= looseMinZ4 &&
                   (long)bounds.MaxZ * 4 <= looseMaxZ4;
        }

        private static bool ContainsInLooseBounds(
            IncrementalNode child,
            in SpatialAabbXZ bounds)
        {
            long looseMinX4 = child.MinX * 4 - child.Side;
            long looseMinZ4 = child.MinZ * 4 - child.Side;
            long looseMaxX4 = (child.MinX + child.Side) * 4 + child.Side;
            long looseMaxZ4 = (child.MinZ + child.Side) * 4 + child.Side;
            return (long)bounds.MinX * 4 >= looseMinX4 &&
                   (long)bounds.MaxX * 4 <= looseMaxX4 &&
                   (long)bounds.MinZ * 4 >= looseMinZ4 &&
                   (long)bounds.MaxZ * 4 <= looseMaxZ4;
        }

        private static bool OverlapsNodeLooseBounds(Node node, in SpatialAabbXZ bounds)
        {
            long looseMinX4 = node.MinX * 4 - node.Side;
            long looseMinZ4 = node.MinZ * 4 - node.Side;
            long looseMaxX4 = (node.MinX + node.Side) * 4 + node.Side;
            long looseMaxZ4 = (node.MinZ + node.Side) * 4 + node.Side;
            return (long)bounds.MinX * 4 < looseMaxX4 &&
                   (long)bounds.MaxX * 4 > looseMinX4 &&
                   (long)bounds.MinZ * 4 < looseMaxZ4 &&
                   (long)bounds.MaxZ * 4 > looseMinZ4;
        }

        private static bool OverlapsNodeLooseBounds(
            IncrementalNode node,
            in SpatialAabbXZ bounds)
        {
            long looseMinX4 = node.MinX * 4 - node.Side;
            long looseMinZ4 = node.MinZ * 4 - node.Side;
            long looseMaxX4 = (node.MinX + node.Side) * 4 + node.Side;
            long looseMaxZ4 = (node.MinZ + node.Side) * 4 + node.Side;
            return (long)bounds.MinX * 4 < looseMaxX4 &&
                   (long)bounds.MaxX * 4 > looseMinX4 &&
                   (long)bounds.MinZ * 4 < looseMaxZ4 &&
                   (long)bounds.MaxZ * 4 > looseMinZ4;
        }

        private int CreateNode(long minX, long minZ, long side, int depth)
        {
            EnsureNodeCapacity(nodeCount + 1);
            Node node = nodes[nodeCount] ??= new Node();
            node.Reset(minX, minZ, side, depth);
            if (depth > MaximumObservedDepth)
                MaximumObservedDepth = depth;
            return nodeCount++;
        }

        private void EnsureNodeCapacity(int required)
        {
            if (required <= nodes.Length)
                return;

            int capacity = nodes.Length;
            while (capacity < required)
                capacity *= 2;
            Array.Resize(ref nodes, capacity);
        }

        private void EnsureEntryCapacity(int required)
        {
            if (required <= entries.Length)
                return;

            int capacity = entries.Length;
            while (capacity < required)
                capacity *= 2;
            Array.Resize(ref entries, capacity);
            Array.Resize(ref entryNextIndices, capacity);
        }

        private static void ResolveRoot(
            long minX,
            long minZ,
            long maxX,
            long maxZ,
            out long rootMinX,
            out long rootMinZ,
            out long rootSide)
        {
            long spanX = Math.Max(1, maxX - minX);
            long spanZ = Math.Max(1, maxZ - minZ);
            rootSide = MinimumRootSide;
            long required = Math.Max(spanX, spanZ);
            while (rootSide < required && rootSide <= (1L << 61))
                rootSide <<= 1;

            // Keep rebuilds on a stable base grid. Aligning to the current side can
            // never cover an interval that straddles that side's zero boundary,
            // regardless of how many times the side is doubled.
            rootMinX = FloorDiv(minX, MinimumRootSide) * MinimumRootSide;
            rootMinZ = FloorDiv(minZ, MinimumRootSide) * MinimumRootSide;
            while ((maxX > rootMinX + rootSide || maxZ > rootMinZ + rootSide) &&
                   rootSide <= (1L << 61))
            {
                rootSide <<= 1;
                rootMinX = FloorDiv(minX, MinimumRootSide) * MinimumRootSide;
                rootMinZ = FloorDiv(minZ, MinimumRootSide) * MinimumRootSide;
            }
        }

        private static long FloorDiv(long value, long divisor)
        {
            long quotient = value / divisor;
            long remainder = value % divisor;
            if (remainder != 0 && value < 0)
                quotient--;
            return quotient;
        }
    }
}
