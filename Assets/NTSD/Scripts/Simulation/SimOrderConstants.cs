using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimOrder 常量定义（对齐 FLF factory 顺序）
    ///
    /// FLF 执行顺序（参考 LF/match.js:316-323）:
    /// for (const objecttype in factory) {
    ///   character -> lightweapon -> heavyweapon -> specialattack -> effect
    /// }
    ///
    /// Unity 执行顺序：
    /// SimulationWorld 按 SimOrder 升序执行所有 ISimObject
    /// 同一 SimOrder 内按 StableId 升序排序
    /// </summary>
    public static class SimOrderConstants
    {
        // ========== 输入层 ==========

        /// <summary>输入检测（在所有逻辑之前）</summary>
        public const int Input = 5;

        // ========== 逻辑层（对齐 FLF factory 顺序）==========

        /// <summary>角色 - 对应 FLF character（第 1 组）</summary>
        public const int Character = 10;

        /// <summary>轻武器 - 对应 FLF lightweapon（第 2 组）</summary>
        public const int LightWeapon = 20;

        /// <summary>重武器 - 对应 FLF heavyweapon（第 3 组）</summary>
        public const int HeavyWeapon = 30;

        /// <summary>特殊攻击 - 对应 FLF specialattack（第 4 组）</summary>
        public const int SpecialAttack = 40;

        /// <summary>特效 - 对应 FLF effect（第 5 组）</summary>
        public const int Effect = 50;

        // ========== 渲染层 ==========

        /// <summary>渲染器（在所有逻辑之后）</summary>
        public const int Renderer = 100;

        // ========== 工具方法 ==========

        /// <summary>
        /// 根据对象类型枚举获取 SimOrder
        /// 对应 FLF factories.js 的顺序
        /// </summary>
        /// <param name="objectType">对象类型枚举</param>
        /// <returns>对应的 SimOrder 值</returns>
        public static int GetSimOrderByObjectType(LF2ObjectType objectType)
        {
            switch (objectType)
            {
                case LF2ObjectType.Character:
                    return Character;

                case LF2ObjectType.LightWeapon:
                    return LightWeapon;

                case LF2ObjectType.HeavyWeapon:
                    return HeavyWeapon;

                case LF2ObjectType.ThrowWeapon:
                case LF2ObjectType.Drink:
                    return LightWeapon;

                case LF2ObjectType.SpecialAttack:
                    return SpecialAttack;

                //case LF2ObjectType.Baseball:
                //case LF2ObjectType.Miscellaneous:
                //case LF2ObjectType.Drink:
                //    // 待实现的类型，暂时归入特效组
                //    return Effect;
                default:
                    Debug.LogWarning($"[SimOrderConstants] Unknown object type: {objectType}, fallback to Character");
                    return Character;
            }
        }

        /// <summary>
        /// 根据旧的 int 类型获取 SimOrder（向后兼容）
        /// </summary>
        /// <param name="objectType">对象类型（int）</param>
        /// <returns>对应的 SimOrder 值</returns>
        public static int GetSimOrderByObjectType(int objectType)
        {
            return GetSimOrderByObjectType((LF2ObjectType)objectType);
        }
    }
}
