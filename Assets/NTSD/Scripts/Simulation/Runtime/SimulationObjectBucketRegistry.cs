using System;
using System.Collections.Generic;

using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    internal sealed class SimulationObjectBucket
    {
        internal SimulationObjectBucket()
        {
            items = new List<ISimObject>();
        }

        public readonly List<ISimObject> items;
        public bool dirty;
        internal int SimOrder { get; private set; }
        internal bool IsInUse { get; private set; }

        internal void Activate(int simOrder)
        {
            SimOrder = simOrder;
            IsInUse = true;
            dirty = false;
            items.Clear();
        }

        internal void Release()
        {
            items.Clear();
            dirty = false;
            SimOrder = 0;
            IsInUse = false;
        }

        internal void PrepareItemCapacity(int capacity)
        {
            if (capacity > items.Capacity)
                items.Capacity = capacity;
        }

        public void EnsureSorted(IComparer<ISimObject> comparer)
        {
            if (!dirty)
                return;

            items.Sort(comparer);
            dirty = false;
        }
    }

    /// <summary>
    /// Owns simulation-order lookup and deterministic traversal. Buckets are pooled
    /// before battle; hot-path traversal uses a compact sorted list and never
    /// enumerates SortedDictionary nodes.
    /// </summary>
    internal sealed class SimulationObjectBucketRegistry
    {
        private const int InitialBucketCapacity = 16;

        private readonly Dictionary<int, SimulationObjectBucket> lookup;
        private readonly List<SimulationObjectBucket> orderedBuckets;
        private SimulationObjectBucket[] bucketPool;
        private bool capacitySealed;

        internal SimulationObjectBucketRegistry()
        {
            lookup = new Dictionary<int, SimulationObjectBucket>(InitialBucketCapacity * 2);
            orderedBuckets = new List<SimulationObjectBucket>(InitialBucketCapacity);
            bucketPool = CreateBucketPool(InitialBucketCapacity);
        }

        internal Dictionary<int, SimulationObjectBucket> LookupForCompatibility => lookup;
        internal int Count => lookup.Count;
        internal int OrderedCount => orderedBuckets.Count;
        internal long RejectedBucketGrowthCount { get; private set; }

        internal SimulationObjectBucket GetOrderedBucket(int index)
        {
            return orderedBuckets[index];
        }

        internal bool TryGetValue(int simOrder, out SimulationObjectBucket bucket)
        {
            return lookup.TryGetValue(simOrder, out bucket);
        }

        internal SimulationObjectBucket GetOrCreate(int simOrder)
        {
            if (lookup.TryGetValue(simOrder, out SimulationObjectBucket existing))
                return existing;

            SimulationObjectBucket bucket = AcquireBucket();
            if (bucket == null)
                return null;

            bucket.Activate(simOrder);
            lookup.Add(simOrder, bucket);
            int insertionIndex = FindInsertionIndex(simOrder);
            orderedBuckets.Insert(insertionIndex, bucket);
            return bucket;
        }

        internal void RemoveIfEmpty(int simOrder, SimulationObjectBucket bucket)
        {
            if (bucket == null || bucket.items.Count != 0 ||
                !lookup.TryGetValue(simOrder, out SimulationObjectBucket current) ||
                !ReferenceEquals(current, bucket))
            {
                return;
            }

            lookup.Remove(simOrder);
            int orderedIndex = FindOrderedBucketIndex(bucket);
            if (orderedIndex >= 0)
                orderedBuckets.RemoveAt(orderedIndex);
            bucket.Release();
        }

        internal void PrepareCapacity(int maximumItemsPerBucket)
        {
            if (capacitySealed || maximumItemsPerBucket <= 0)
                return;

            for (int index = 0; index < bucketPool.Length; index++)
                bucketPool[index].PrepareItemCapacity(maximumItemsPerBucket);
        }

        internal void SealCapacity()
        {
            capacitySealed = true;
        }

        internal void UnsealCapacity()
        {
            capacitySealed = false;
        }

        internal void Clear()
        {
            for (int index = 0; index < orderedBuckets.Count; index++)
                orderedBuckets[index].Release();

            orderedBuckets.Clear();
            lookup.Clear();
        }

        private SimulationObjectBucket AcquireBucket()
        {
            for (int index = 0; index < bucketPool.Length; index++)
            {
                if (!bucketPool[index].IsInUse)
                    return bucketPool[index];
            }

            if (capacitySealed)
            {
                RejectedBucketGrowthCount++;
                return null;
            }

            int previousLength = bucketPool.Length;
            int nextLength = checked(previousLength * 2);
            Array.Resize(ref bucketPool, nextLength);
            for (int index = previousLength; index < nextLength; index++)
                bucketPool[index] = new SimulationObjectBucket();
            return bucketPool[previousLength];
        }

        private int FindInsertionIndex(int simOrder)
        {
            int low = 0;
            int high = orderedBuckets.Count;
            while (low < high)
            {
                int middle = low + ((high - low) >> 1);
                if (orderedBuckets[middle].SimOrder < simOrder)
                    low = middle + 1;
                else
                    high = middle;
            }
            return low;
        }

        private int FindOrderedBucketIndex(SimulationObjectBucket bucket)
        {
            int low = 0;
            int high = orderedBuckets.Count - 1;
            int simOrder = bucket.SimOrder;
            while (low <= high)
            {
                int middle = low + ((high - low) >> 1);
                int comparison = orderedBuckets[middle].SimOrder.CompareTo(simOrder);
                if (comparison == 0)
                    return ReferenceEquals(orderedBuckets[middle], bucket) ? middle : -1;
                if (comparison < 0)
                    low = middle + 1;
                else
                    high = middle - 1;
            }
            return -1;
        }

        private static SimulationObjectBucket[] CreateBucketPool(int capacity)
        {
            var buckets = new SimulationObjectBucket[capacity];
            for (int index = 0; index < buckets.Length; index++)
                buckets[index] = new SimulationObjectBucket();
            return buckets;
        }
    }
}
