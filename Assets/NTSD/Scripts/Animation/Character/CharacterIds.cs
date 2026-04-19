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
        // 对应 FLF LF2_19-master/data/data.js 中的 object id
        public const int Template = 0;
        public const int Deep     = 1;  // id_updates[1]
        public const int John     = 2;
        public const int Henry    = 4;
        public const int Rudolf   = 5;  // id_updates[5]: rudolf_transform, state3_frame, state1280_disappear
        public const int Louis    = 6;  // id_updates[6]: generic_combo hit_ja 禁用
        public const int Firen    = 7;
        public const int Freeze   = 8;
        public const int Dennis   = 9;
        public const int Woody    = 10; // id_updates[10]: state3_fly_crash
        public const int Davis    = 11; // id_updates[11]: state3_hit_stop, state3_frame_force
    }
}
