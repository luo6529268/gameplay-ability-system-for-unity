using System.Collections.Generic;

namespace NTSD.Animation
{
    [System.Serializable]
    public struct LF2ArmorFrameRange
    {
        public int first;
        public int last;
    }

    [System.Serializable]
    public sealed class LF2ArmorData
    {
        public int type;
        public int ratio;
        public int decrease;
        public int mp;
        public int fall;
        public int bdefend;
        public int injury;
        public int spark;
        public int hp;
        public int recover;
        public int facing;
        public int action;
        public int reserve;
        public int delay;
        public List<LF2ArmorFrameRange> frame_ranges =
            new List<LF2ArmorFrameRange>();
        public List<int> states = new List<int>();
        public List<int> kinds = new List<int>();
        public List<int> ids = new List<int>();
        public List<int> effects = new List<int>();
        public string sound1;
        public string sound2;

        public LF2ArmorData DeepCopy()
        {
            var copy = new LF2ArmorData();
            copy.CopyFrom(this);
            return copy;
        }

        public void CopyFrom(LF2ArmorData source)
        {
            if (source == null)
                throw new System.ArgumentNullException(nameof(source));

            type = source.type;
            ratio = source.ratio;
            decrease = source.decrease;
            mp = source.mp;
            fall = source.fall;
            bdefend = source.bdefend;
            injury = source.injury;
            spark = source.spark;
            hp = source.hp;
            recover = source.recover;
            facing = source.facing;
            action = source.action;
            reserve = source.reserve;
            delay = source.delay;
            sound1 = source.sound1;
            sound2 = source.sound2;

            CopyList(source.frame_ranges, ref frame_ranges);
            CopyList(source.states, ref states);
            CopyList(source.kinds, ref kinds);
            CopyList(source.ids, ref ids);
            CopyList(source.effects, ref effects);
        }

        public ulong ComputeFingerprint64()
        {
            const ulong offset = 1469598103934665603UL;
            ulong hash = offset;
            Add(ref hash, type);
            Add(ref hash, ratio);
            Add(ref hash, decrease);
            Add(ref hash, mp);
            Add(ref hash, fall);
            Add(ref hash, bdefend);
            Add(ref hash, injury);
            Add(ref hash, spark);
            Add(ref hash, hp);
            Add(ref hash, recover);
            Add(ref hash, facing);
            Add(ref hash, action);
            Add(ref hash, reserve);
            Add(ref hash, delay);
            Add(ref hash, frame_ranges);
            Add(ref hash, states);
            Add(ref hash, kinds);
            Add(ref hash, ids);
            Add(ref hash, effects);
            Add(ref hash, sound1);
            Add(ref hash, sound2);
            return hash;
        }

        private static void CopyList<T>(List<T> source, ref List<T> target)
        {
            target ??= new List<T>(source?.Count ?? 0);
            target.Clear();
            if (source != null)
                target.AddRange(source);
        }

        private static void Add(ref ulong hash, int value)
        {
            unchecked
            {
                uint bits = (uint)value;
                for (int shift = 0; shift < 32; shift += 8)
                {
                    hash ^= (byte)(bits >> shift);
                    hash *= 1099511628211UL;
                }
            }
        }

        private static void Add(
            ref ulong hash,
            List<LF2ArmorFrameRange> values)
        {
            Add(ref hash, values?.Count ?? -1);
            if (values == null)
                return;
            for (int index = 0; index < values.Count; index++)
            {
                Add(ref hash, values[index].first);
                Add(ref hash, values[index].last);
            }
        }

        private static void Add(ref ulong hash, List<int> values)
        {
            Add(ref hash, values?.Count ?? -1);
            if (values == null)
                return;
            for (int index = 0; index < values.Count; index++)
                Add(ref hash, values[index]);
        }

        private static void Add(ref ulong hash, string value)
        {
            Add(ref hash, value == null ? -1 : value.Length);
            if (value == null)
                return;
            unchecked
            {
                for (int index = 0; index < value.Length; index++)
                {
                    char character = value[index];
                    hash ^= (byte)character;
                    hash *= 1099511628211UL;
                    hash ^= (byte)(character >> 8);
                    hash *= 1099511628211UL;
                }
            }
        }
    }
}
