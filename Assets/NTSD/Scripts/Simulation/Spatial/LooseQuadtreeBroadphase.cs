using System;
using System.Collections.Generic;

namespace NTSD.Simulation.Spatial
{
    public sealed class LooseQuadtreeBroadphase : ISpatialBroadphase
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

        private readonly int leafCapacity;
        private readonly int maxDepth;
        private Node[] nodes = new Node[16];
        private SpatialBroadphaseEntry[] entries = new SpatialBroadphaseEntry[32];
        private int nodeCount;
        private int entryCount;

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
