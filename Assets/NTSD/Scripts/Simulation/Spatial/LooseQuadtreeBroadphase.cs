using System;
using System.Collections.Generic;

namespace NTSD.Simulation.Spatial
{
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
            public readonly List<int> Entries = new List<int>(DefaultLeafCapacity + 1);

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
                Entries.Clear();
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
            public readonly List<int> Entries = new List<int>(DefaultLeafCapacity + 1);

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
                Entries.Clear();
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
            public int ValidationToken;
            public bool Active;

            public void Reset(RuntimeEntityHandle handle, in SpatialAabbXZ bounds)
            {
                Handle = handle;
                Bounds = bounds;
                NodeIndex = -1;
                ValidationToken = 0;
                Active = true;
            }

            public void Release()
            {
                Handle = RuntimeEntityHandle.Invalid;
                Bounds = default;
                NodeIndex = -1;
                ValidationToken = 0;
                Active = false;
            }
        }

        private readonly int leafCapacity;
        private readonly int maxDepth;
        private Node[] nodes = new Node[16];
        private SpatialBroadphaseEntry[] entries = new SpatialBroadphaseEntry[32];
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

        internal int NodeCount => nodeCount;
        internal int EntryCount => entryCount;
        internal int RootRetainedEntryCount => nodeCount > 0 ? nodes[0].Entries.Count : 0;
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

        public void ResetIncremental()
        {
            for (int i = 0; i < incrementalNodeCount; i++)
                incrementalNodes[i]?.Entries.Clear();
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

        private SpatialSynchronizeResult RebuildIncremental(
            IReadOnlyList<IncrementalSpatialEntry> sourceEntries,
            in SpatialAabbXZ preferredRoot)
        {
            int previousCount = incrementalActiveCount;
            for (int i = 0; i < incrementalNodeCount; i++)
                incrementalNodes[i]?.Entries.Clear();
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

            List<int> nodeEntries = incrementalNodes[nodeIndex].Entries;
            for (int i = 0; i < nodeEntries.Count; i++)
            {
                if (nodeEntries[i] != recordIndex)
                    continue;

                int last = nodeEntries.Count - 1;
                nodeEntries[i] = nodeEntries[last];
                nodeEntries.RemoveAt(last);
                return true;
            }

            return false;
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

            node.Entries.Add(recordIndex);
            record.NodeIndex = nodeIndex;
            if (!node.HasChildren && node.Entries.Count > leafCapacity &&
                node.Depth < maxDepth && node.Side > 1)
            {
                SplitIncremental(nodeIndex);
            }
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

            int write = 0;
            for (int i = 0; i < node.Entries.Count; i++)
            {
                int recordIndex = node.Entries[i];
                int childIndex = ResolveContainingIncrementalChild(
                    node,
                    incrementalRecords[recordIndex].Bounds);
                if (childIndex >= 0)
                {
                    InsertIncremental(childIndex, recordIndex);
                }
                else
                {
                    node.Entries[write++] = recordIndex;
                    incrementalRecords[recordIndex].NodeIndex = nodeIndex;
                }
            }

            if (write < node.Entries.Count)
                node.Entries.RemoveRange(write, node.Entries.Count - write);
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

            for (int i = 0; i < node.Entries.Count; i++)
            {
                int recordIndex = node.Entries[i];
                if (recordIndex < 0 || recordIndex >= incrementalRecordCount)
                    throw new InvalidOperationException("Incremental quadtree contains an invalid entry index.");

                IncrementalRecord record = incrementalRecords[recordIndex];
                if (record == null || !record.Active || record.NodeIndex != nodeIndex)
                    throw new InvalidOperationException("Incremental quadtree entry mapping is stale.");
                if (record.Bounds.Overlaps(query))
                    result.Add(record.Handle);
            }

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
                if (node.HasChildren &&
                    (node.Child0 < 0 || node.Child1 < 0 || node.Child2 < 0 || node.Child3 < 0 ||
                     node.Child0 >= incrementalNodeCount || node.Child1 >= incrementalNodeCount ||
                     node.Child2 >= incrementalNodeCount || node.Child3 >= incrementalNodeCount))
                {
                    return false;
                }

                for (int entryIndex = 0; entryIndex < node.Entries.Count; entryIndex++)
                {
                    int recordIndex = node.Entries[entryIndex];
                    if (recordIndex < 0 || recordIndex >= incrementalRecordCount)
                        return false;
                    IncrementalRecord record = incrementalRecords[recordIndex];
                    if (record == null || !record.Active || record.NodeIndex != nodeIndex ||
                        record.ValidationToken == token || !record.Bounds.IsValid)
                    {
                        return false;
                    }

                    record.ValidationToken = token;
                    visited++;
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

                entries[entryCount++] = entry;
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

            for (int i = 0; i < node.Entries.Count; i++)
            {
                int entryIndex = node.Entries[i];
                if (entries[entryIndex].Bounds.Overlaps(query))
                    result.Add(entries[entryIndex].InputIndex);
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

            node.Entries.Add(entryIndex);
            if (!node.HasChildren && node.Entries.Count > leafCapacity && node.Depth < maxDepth && node.Side > 1)
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

            int write = 0;
            for (int i = 0; i < node.Entries.Count; i++)
            {
                int entryIndex = node.Entries[i];
                int childIndex = ResolveContainingChild(node, entries[entryIndex].Bounds);
                if (childIndex >= 0)
                    Insert(childIndex, entryIndex);
                else
                    node.Entries[write++] = entryIndex;
            }

            if (write < node.Entries.Count)
                node.Entries.RemoveRange(write, node.Entries.Count - write);
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
