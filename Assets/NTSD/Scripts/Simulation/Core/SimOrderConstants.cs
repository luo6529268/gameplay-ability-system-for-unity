using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// Unity 战斗运行时使用的稳定模拟顺序分桶。
    /// 对象先按 SimOrder 升序执行，再按同桶内的 StableId 排序。
    /// </summary>
    public static class SimOrderConstants
    {
        // 输入
        public const int Input = 5;

        // 核心运行时实体
        public const int Character = 10;
        public const int LightWeapon = 20;
        public const int HeavyWeapon = 30;
        public const int SpecialAttack = 40;
        public const int Effect = 50;

        // 表现层
        public const int Renderer = 100;

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

        public static int GetSimOrderByObjectType(int objectType)
        {
            return GetSimOrderByObjectType((LF2ObjectType)objectType);
        }
    }
}
