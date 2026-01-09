using System.Collections.Generic;
using System.Threading;

namespace NTSD.Simulation
{
    /// <summary>
    /// FLF/LF2 世界规则（global.js 的 gameplay 部分）- 直接死写数值（不走 JSON/ScriptableObject）。
    /// 目标：在 Unity 中复刻 FLF 的常量语义与默认值。
    /// </summary>
    public static class NTSDGlobal
    {
        public static class Gameplay
        {
            public const int Framerate = 30;

            // FLF: GC.min_speed
            public const float MinSpeed = 1f;

            // FLF: GC.gravity
            public const float Gravity = 1.7f;

            // FLF: GC.friction.fell
            // 注意：FLF 的 util.lookup_abs() 依赖 key 的有序遍历（数值 key 升序）。
            public static readonly IReadOnlyDictionary<int, float> FrictionFell = new Dictionary<int, float>
            {
                { 2, 0f },
                { 3, 1f },
                { 5, 2f },
                { 6, 4f },
                { 9, 5f },
                { 13, 7f },
                { 25, 9f },
            };
        }

        public static class Default
        {
            public static class Health
            {
                public const int HpFull = 500;
                public const int MpFull = 500;
                public const int MpStart = 200;
            }

            public static class Itr
            {
                public const float ZWidth = 12f;
                public const int HitStop = 3;
                public const int ThrowInjury = 10;
            }

            public static class CPoint
            {
                public const int Hurtable = 0;
                public const int Cover = 0;
                public const int VAction = 135;
            }

            public static class WPoint
            {
                public const int Cover = 0;
            }

            public static class Effect
            {
                public const int Num = 0;
            }

            public static class Fall
            {
                public const int Value = 20;
                public const float Dvy = -6.9f;
            }

            public static class Weapon
            {
                public const int VRest = 9;
            }

            public static class Character
            {
                public const int ARest = 7;
            }

            public static class Machanics
            {
                // FLF: GC.default.machanics.mass
                public const float Mass = 1f;
            }
        }

        public static class Combo 
        {
           public const int Timeout = 10; // 连招超时时间（时间单位）
        }

        /// <summary>
        /// FLF util.lookup_abs(A, x) 的 C# 等价实现：
        /// - 取 abs(x)
        /// - 返回第一个 key >= abs(x) 对应的 value
        /// - 若 abs(x) 大于所有 key，则返回最后一个 key 的 value
        /// </summary>
        public static float LookupAbs(IReadOnlyDictionary<int, float> table, float x)
        {
            if (table == null || table.Count == 0)
            {
                return 0f;
            }

            if (x < 0f) x = -x;

            // keys 很少，直接排序扫描即可（避免引入额外结构）。
            int? lastKey = null;
            foreach (var kv in SortedByKey(table))
            {
                lastKey = kv.Key;
                if (x <= kv.Key)
                {
                    return kv.Value;
                }
            }

            return lastKey.HasValue ? table[lastKey.Value] : 0f;
        }

        private static IEnumerable<KeyValuePair<int, float>> SortedByKey(IReadOnlyDictionary<int, float> table)
        {
            var keys = new List<int>(table.Keys);
            keys.Sort();
            foreach (var k in keys)
            {
                yield return new KeyValuePair<int, float>(k, table[k]);
            }
        }
    }
}

