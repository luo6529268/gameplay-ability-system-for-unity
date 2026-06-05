using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimOrder 常量定义。
    /// 复刻基准是 C++ release 的实体处理顺序；Unity 使用分桶排序保证同帧遍历稳定。
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

        // ========== 逻辑层（按 C++ release 实体类型分桶）==========

        /// <summary>角色实体。</summary>
        public const int Character = 10;

        /// <summary>轻武器实体。</summary>
        public const int LightWeapon = 20;

        /// <summary>重武器实体。</summary>
        public const int HeavyWeapon = 30;

        /// <summary>特殊攻击和技能生成物实体。</summary>
        public const int SpecialAttack = 40;

        /// <summary>特效实体。</summary>
        public const int Effect = 50;

        // ========== 渲染层 ==========

        /// <summary>渲染器（在所有逻辑之后）</summary>
        public const int Renderer = 100;

        // ========== 工具方法 ==========

        /// <summary>
        /// 根据对象类型枚举获取 SimOrder
        /// 根据对象类型枚举获取稳定的模拟执行顺序。
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

                case LF2ObjectType.Other:
                    return Effect;

                default:
                    Debug.LogWarning($"[SimOrderConstants] Unknown object type: {objectType}, using Character order");
                    return Character;
            }
        }

        /// <summary>
        /// 根据 int 类型获取 SimOrder。
        /// </summary>
        /// <param name="objectType">对象类型（int）</param>
        /// <returns>对应的 SimOrder 值</returns>
        public static int GetSimOrderByObjectType(int objectType)
        {
            return GetSimOrderByObjectType((LF2ObjectType)objectType);
        }
    }
}
