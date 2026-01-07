namespace NTSD.Animation
{
    /// <summary>
    /// 角色ID常量（对应 FLF 的 id_updates[characterId] 的 key）
    ///
    /// 职责：
    /// - 集中管理所有角色配置ID
    /// - 作为 CharacterIdUpdate.RegisterDefaultHandlers(characterId) 的参数
    ///
    /// 对应 FLF character.js 中的角色ID：
    /// - id_updates[0]: Template
    /// - id_updates[1]: Deep
    /// - id_updates[5]: Davis
    /// - id_updates[11]: Rudolf
    /// 等等
    ///
    /// 未来：应从角色数据配置中读取，而非硬编码
    /// </summary>
    public static class CharacterIds
    {
        /// <summary>
        /// Template 角色（FLF 的基础模板角色）
        /// </summary>
        public const int Template = 0;

        /// <summary>
        /// Deep（FLF character.js 中 id_updates[1] 有特殊逻辑）
        /// </summary>
        public const int Deep = 1;

        /// <summary>
        /// Davis（FLF character.js 中 id_updates[5] 有特殊 TU handler）
        /// </summary>
        public const int Davis = 5;

        /// <summary>
        /// Rudolf（FLF character.js 中 id_updates[11] 有变身相关逻辑）
        /// </summary>
        public const int Rudolf = 11;

        // Future: 根据实际需要补充其他角色ID
        // 例如：Freeze, Firen, Louis, John, Henry 等
    }
}
