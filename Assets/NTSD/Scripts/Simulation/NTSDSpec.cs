using System.Collections.Generic;

namespace NTSD.Simulation
{
    /// <summary>
    /// FLF/LF2 对象属性表（spec/properties.js）- 直接死写数值（不走 JSON/ScriptableObject）。
    /// Key: ObjectId（在本项目中等同于 CharacterID）。
    ///
    /// 来源参考：
    /// - I:\C++Test\NTSD\LF2_19-master\data\properties.js
    /// </summary>
    public static class NTSDSpec
    {
        public readonly struct SpecEntry
        {
            public readonly float? Mass;
            public readonly float? ZWidth;
            public readonly bool? NoShadow;
            public readonly int? Oscillate;

            public readonly bool? Attackable;
            public readonly bool? RunThrow;
            public readonly bool? JumpThrow;
            public readonly bool? DashThrow;
            public readonly bool? StandThrow;
            public readonly bool? JustThrow;

            public readonly bool? DashBackAttack;
            public readonly bool? HeavyWeaponDash;
            public readonly bool? HeavyWeaponJump;

            public SpecEntry(
                float? mass = null,
                float? zWidth = null,
                bool? noShadow = null,
                int? oscillate = null,
                bool? attackable = null,
                bool? runThrow = null,
                bool? jumpThrow = null,
                bool? dashThrow = null,
                bool? standThrow = null,
                bool? justThrow = null,
                bool? dashBackAttack = null,
                bool? heavyWeaponDash = null,
                bool? heavyWeaponJump = null)
            {
                Mass = mass;
                ZWidth = zWidth;
                NoShadow = noShadow;
                Oscillate = oscillate;

                Attackable = attackable;
                RunThrow = runThrow;
                JumpThrow = jumpThrow;
                DashThrow = dashThrow;
                StandThrow = standThrow;
                JustThrow = justThrow;

                DashBackAttack = dashBackAttack;
                HeavyWeaponDash = heavyWeaponDash;
                HeavyWeaponJump = heavyWeaponJump;
            }
        }

        // 只录入 LF2_19-master/data/properties.js 中明确出现的条目（其余 ID 走默认值语义）。
        public static readonly IReadOnlyDictionary<int, SpecEntry> ById = new Dictionary<int, SpecEntry>
        {
            // 1: Deep（角色 ID）- 空表，表示全部走 default
            { 1, new SpecEntry() },

            // 30: Bandit（角色 ID）
            { 30, new SpecEntry(dashBackAttack: false, heavyWeaponDash: false, heavyWeaponJump: false) },

            // 100: 棒球棒（轻武器）
            { 100, new SpecEntry(
                mass: 0.3f,
                attackable: true,
                runThrow: true,
                jumpThrow: true,
                dashThrow: false,
                standThrow: false,
                justThrow: false,
                noShadow: false) },

            // 101: 镐头
            { 101, new SpecEntry(mass: 0.7f, attackable: true, runThrow: true, jumpThrow: true) },

            // 150: 石头（重武器）
            { 150, new SpecEntry(mass: 0.9f) },

            // 201: Henry 的箭 1（特殊攻击）
            { 201, new SpecEntry(mass: 0.3f, zWidth: 1f) },

            // 202: Rudolf 的武器（特殊攻击）
            { 202, new SpecEntry(mass: 0.3f, zWidth: 1f) },

            // 203: Deep 的球（特殊攻击）- 空表
            { 203, new SpecEntry() },

            // 207: Davis 的球（特殊攻击）- 空表
            { 207, new SpecEntry() },

            // 212: 冰弹和旋风（无阴影）
            { 212, new SpecEntry(noShadow: true) },

            // 213: 冰剑
            { 213, new SpecEntry(mass: 0.5f, attackable: true, runThrow: true, jumpThrow: true) },

            // 300: 打击特效（振荡幅度）
            { 300, new SpecEntry(oscillate: 4) },

            // 302: 火焰特效（振荡幅度）
            { 302, new SpecEntry(oscillate: 3) },
        };

        public static bool TryGet(int objectId, out SpecEntry entry) => ById.TryGetValue(objectId, out entry);

        public static SpecEntry Get(int objectId) => ById.TryGetValue(objectId, out var entry) ? entry : new SpecEntry();

        /// <summary>
        /// 对应 FLF livingobject.prototype.proper(id, prop)
        /// 从 spec 配置表读取对象属性，返回 null 表示未定义
        /// 参考：FLF livingobject.js:540-549
        /// </summary>
        public static object Proper(int objectId, string prop)
        {
            if (!ById.TryGetValue(objectId, out var entry))
                return null;

            switch (prop)
            {
                case "mass": return entry.Mass;
                case "zwidth": return entry.ZWidth;
                case "no_shadow": return entry.NoShadow;
                case "oscillate": return entry.Oscillate;
                case "attackable": return entry.Attackable;
                case "run_throw": return entry.RunThrow;
                case "jump_throw": return entry.JumpThrow;
                case "dash_throw": return entry.DashThrow;
                case "stand_throw": return entry.StandThrow;
                case "just_throw": return entry.JustThrow;
                case "dash_back_attack": return entry.DashBackAttack;
                case "heavy_weapon_dash": return entry.HeavyWeaponDash;
                case "heavy_weapon_jump": return entry.HeavyWeaponJump;
                default: return null;
            }
        }

        /// <summary>
        /// 泛型版本的 Proper，带类型转换
        /// </summary>
        public static T Proper<T>(int objectId, string prop, T defaultValue = default)
        {
            var value = Proper(objectId, prop);
            if (value is T typed) return typed;
            return defaultValue;
        }

        public static float GetMassOrDefault(int objectId)
        {
            var entry = Get(objectId);
            return entry.Mass ?? NTSDGlobal.Default.Machanics.Mass;
        }

        public static float GetItrZWidthOrDefault(int objectId)
        {
            var entry = Get(objectId);
            return entry.ZWidth ?? NTSDGlobal.Default.Itr.ZWidth;
        }
    }
}

