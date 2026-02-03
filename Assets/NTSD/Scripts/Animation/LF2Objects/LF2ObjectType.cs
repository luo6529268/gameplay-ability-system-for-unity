namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 对象类型枚举
    /// 对应 FLF factories.js 的类型定义
    ///
    /// 参考：
    /// - FLF AI.js:20 (type: 0=character, 1=lightweapon, 2=heavyweapon, 3=specialattack, 4=baseball, 5=criminal, 6=drink)
    /// - FLF factories.js:14-23 (factory object keys)
    /// </summary>
    public enum LF2ObjectType
    {
        /// <summary>角色 - 对应 FLF character</summary>
        Character = 0,

        /// <summary>轻武器 - 对应 FLF lightweapon</summary>
        LightWeapon = 1,

        /// <summary>重武器 - 对应 FLF heavyweapon</summary>
        HeavyWeapon = 2,

        /// <summary>特殊攻击（投射物、能量球等） - 对应 FLF specialattack</summary>
        SpecialAttack = 3,

        /// <summary>棒球（待实现） - 对应 FLF baseball</summary>
        Baseball = 4,

        /// <summary>杂物（待实现） - 对应 FLF miscell</summary>
        Miscellaneous = 5,

        /// <summary>饮料（待实现） - 对应 FLF drinks</summary>
        Drink = 6,

        /// <summary>特效 - 对应 FLF effect</summary>
        Effect = 7
    }
}
