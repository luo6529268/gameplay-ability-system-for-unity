using System;
using System.Collections.Generic;

namespace NTSD.Simulation
{
    /// <summary>
    /// Defensive source-ordered BPoint catalog section. The canonical scalar
    /// writer emits count followed by each X/Y pair for the downstream
    /// fixed-width signed-int encoder.
    /// </summary>
    public sealed class BattleBloodPointCatalog :
        IReadOnlyList<BattleBloodPointValue>
    {
        private readonly BattleBloodPointValue[] entries;

        public static BattleBloodPointCatalog Empty { get; } =
            new BattleBloodPointCatalog(
                Array.Empty<BattleBloodPointValue>());

        public BattleBloodPointCatalog(
            IReadOnlyList<BattleBloodPointValue> source)
        {
            if (source == null || source.Count == 0)
            {
                entries = Array.Empty<BattleBloodPointValue>();
                return;
            }

            entries = new BattleBloodPointValue[source.Count];
            for (int index = 0; index < source.Count; index++)
                entries[index] = source[index];
        }

        public int Count => entries.Length;

        public BattleBloodPointValue this[int index] => entries[index];

        public bool TryGetPrimary(out BattleBloodPointValue value)
        {
            if (entries.Length == 0)
            {
                value = default;
                return false;
            }

            value = entries[0];
            return true;
        }

        public int CopyCanonicalScalars(int[] destination, int offset)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (offset < 0 || offset > destination.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            int required = 1 + entries.Length * 2;
            if (destination.Length - offset < required)
            {
                throw new ArgumentException(
                    "Destination is too small for the canonical BPoint section.",
                    nameof(destination));
            }

            destination[offset] = entries.Length;
            int cursor = offset + 1;
            for (int index = 0; index < entries.Length; index++)
            {
                destination[cursor++] = entries[index].X;
                destination[cursor++] = entries[index].Y;
            }

            return required;
        }

        public IEnumerator<BattleBloodPointValue> GetEnumerator()
        {
            for (int index = 0; index < entries.Length; index++)
                yield return entries[index];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
