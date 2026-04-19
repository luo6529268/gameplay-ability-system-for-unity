namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 对象类型枚举
    /// 
    /// 参考：
    /// - LF-Empire 官方文档: https://lf-empire.de/lf2-empire/data-changing/types
    /// - LF2 data.txt 格式: id: X type: Y file: Z
    /// </summary>
    public enum LF2ObjectType
    {
        /// <summary>角色 (type: 0) - 对应 LF2 "Type 0 - Characters"</summary>
        Character = 0,

        /// <summary>轻武器 (type: 1) - 对应 LF2 "Type 1 - Light Weapons"</summary>
        LightWeapon = 1,

        /// <summary>重武器 (type: 2) - 对应 LF2 "Type 2 - Heavy Weapons"</summary>
        HeavyWeapon = 2,

        /// <summary>攻击/投射物 (type: 3) - 对应 LF2 "Type 3 - Attacks"</summary>
        SpecialAttack = 3,

        /// <summary>投掷武器 (type: 4) - 对应 LF2 "Type 4 - Throw Weapons"</summary>
        ThrowWeapon = 4,

        /// <summary>其他/杂物 (type: 5) - 对应 LF2 "Type 5 - Other"</summary>
        Other = 5,

        /// <summary>饮料/回复物 (type: 6) - 对应 LF2 "Type 6 - Drinks"</summary>
        Drink = 6,
    }
}
