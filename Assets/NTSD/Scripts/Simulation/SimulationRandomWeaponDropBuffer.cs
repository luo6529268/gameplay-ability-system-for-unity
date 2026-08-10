using System;

namespace NTSD.Simulation
{
    internal sealed class SimulationRandomWeaponDropBuffer
    {
        private const int MinWeaponOid = 100;
        private const int MaxWeaponOidExclusive = 200;
        private const int Capacity = MaxWeaponOidExclusive - MinWeaponOid;

        private readonly int[] candidates = new int[Capacity];
        private readonly bool[] seen = new bool[Capacity];
        private int count;

        public int Count => count;

        public int this[int index]
        {
            get
            {
                if ((uint)index >= (uint)count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return candidates[index];
            }
        }

        public void Reset()
        {
            count = 0;
            Array.Clear(seen, 0, seen.Length);
        }

        public bool TryMarkUnique(int oid)
        {
            int offset = oid - MinWeaponOid;
            if ((uint)offset >= Capacity || seen[offset])
                return false;

            seen[offset] = true;
            return true;
        }

        public bool TryAdd(int oid)
        {
            if (count >= candidates.Length)
                return false;

            candidates[count++] = oid;
            return true;
        }

    }
}
