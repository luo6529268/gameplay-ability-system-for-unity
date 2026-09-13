using System.Collections.Generic;

namespace NTSD.Simulation
{
    /// <summary>
    /// 历史兼容属性表。正式战斗 runtime 不再从这里查询规则；其余 API 保留既有诊断用途。
    /// Key: ObjectId（在本项目中等同于 CharacterID）。
    ///
    /// 来源参考：
    /// - I:\C++Test\NTSD\LF2_19-master\data\properties.js
    /// </summary>
    public static class NTSDSpec
    {
        public readonly struct SpecEntry
        {
            public readonly float? ZWidth;
            public readonly bool? NoShadow;

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
                float? zWidth = null,
                bool? noShadow = null,
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
                ZWidth = zWidth;
                NoShadow = noShadow;

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
                attackable: true,
                runThrow: true,
                jumpThrow: true,
                dashThrow: false,
                standThrow: false,
                justThrow: false,
                noShadow: false) },

            // 101: 镐头
            { 101, new SpecEntry(attackable: true, runThrow: true, jumpThrow: true) },

            // 150: 石头（重武器）
            { 150, new SpecEntry() },

            // 201: Henry 的箭 1（特殊攻击）
            { 201, new SpecEntry(zWidth: 1f) },

            // 202: Rudolf 的武器（特殊攻击）
            { 202, new SpecEntry(zWidth: 1f) },

            // 203: Deep 的球（特殊攻击）- 空表
            { 203, new SpecEntry() },

            // 207: Davis 的球（特殊攻击）- 空表
            { 207, new SpecEntry() },

            // 212: 冰弹和旋风（无阴影）
            { 212, new SpecEntry(noShadow: true) },

            // 213: 冰剑
            { 213, new SpecEntry(attackable: true, runThrow: true, jumpThrow: true) },

            // 300: 历史打击特效 ID，旧振荡属性已退休。
            { 300, new SpecEntry() },

            // 302: 历史火焰特效 ID，旧振荡属性已退休。
            { 302, new SpecEntry() },
        };

        public static bool TryGet(int objectId, out SpecEntry entry) => ById.TryGetValue(objectId, out entry);

        public static SpecEntry Get(int objectId) => ById.TryGetValue(objectId, out var entry) ? entry : new SpecEntry();

        public static bool IsWeaponAttackable(int objectId) => Get(objectId).Attackable == true;

        public static bool CanRunThrowWeapon(int objectId) => Get(objectId).RunThrow == true;

        public static bool CanStandThrowWeapon(int objectId) => Get(objectId).StandThrow == true;

        public static bool CanJustThrowWeapon(int objectId) => Get(objectId).JustThrow == true;

        public static bool CanHeavyWeaponDash(int characterId) => Get(characterId).HeavyWeaponDash != false;

        public static bool CanHeavyWeaponJump(int characterId) => Get(characterId).HeavyWeaponJump != false;



        public static float GetItrZWidthOrDefault(int objectId)
        {
            var entry = Get(objectId);
            return entry.ZWidth ?? NTSDGlobal.Default.Itr.ZWidth;
        }
    }
}
