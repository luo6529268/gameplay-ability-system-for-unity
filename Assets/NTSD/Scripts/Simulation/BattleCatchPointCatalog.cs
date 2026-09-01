using System;
using System.Collections.Generic;

namespace NTSD.Simulation
{
    /// <summary>
    /// Defensive source-ordered CPoint content. The canonical writer emits
    /// count followed by each entry's 19 fixed-order signed scalars.
    /// </summary>
    public sealed class BattleCatchPointCatalog :
        IReadOnlyList<BattleCatchPointValue>
    {
        private const int ScalarsPerEntry = 19;
        private readonly BattleCatchPointValue[] entries;

        public static BattleCatchPointCatalog Empty { get; } =
            new BattleCatchPointCatalog(
                Array.Empty<BattleCatchPointValue>());

        public BattleCatchPointCatalog(
            IReadOnlyList<BattleCatchPointValue> source)
        {
            if (source == null || source.Count == 0)
            {
                entries = Array.Empty<BattleCatchPointValue>();
                return;
            }

            entries = new BattleCatchPointValue[source.Count];
            for (int index = 0; index < source.Count; index++)
                entries[index] = source[index];
        }

        public int Count => entries.Length;

        public BattleCatchPointValue this[int index] => entries[index];

        public bool TryGetPrimary(out BattleCatchPointValue value)
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

            int required = 1 + entries.Length * ScalarsPerEntry;
            if (destination.Length - offset < required)
            {
                throw new ArgumentException(
                    "Destination is too small for the canonical CPoint section.",
                    nameof(destination));
            }

            destination[offset] = entries.Length;
            int cursor = offset + 1;
            for (int index = 0; index < entries.Length; index++)
            {
                BattleCatchPointValue value = entries[index];
                destination[cursor++] = value.Kind;
                destination[cursor++] = value.X;
                destination[cursor++] = value.Y;
                destination[cursor++] = value.Injury;
                destination[cursor++] = value.Cover;
                destination[cursor++] = value.Vaction;
                destination[cursor++] = value.Aaction;
                destination[cursor++] = value.Jaction;
                destination[cursor++] = value.Daction;
                destination[cursor++] = value.ThrowVx;
                destination[cursor++] = value.ThrowVy;
                destination[cursor++] = value.Hurtable;
                destination[cursor++] = value.Decrease;
                destination[cursor++] = value.DirControl;
                destination[cursor++] = value.Taction;
                destination[cursor++] = value.ThrowInjury;
                destination[cursor++] = value.ThrowVz;
                destination[cursor++] = value.FrontHurtAct;
                destination[cursor++] = value.BackHurtAct;
            }

            return required;
        }

        public IEnumerator<BattleCatchPointValue> GetEnumerator()
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
