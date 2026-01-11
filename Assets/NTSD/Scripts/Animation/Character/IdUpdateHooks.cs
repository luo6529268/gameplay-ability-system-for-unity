namespace NTSD.Animation
{
    /// <summary>
    /// id_update Hook 名称常量（Step D9R）
    ///
    /// 职责：
    /// - 集中管理所有 hookName 字符串常量
    /// - 避免拼写漂移和 typo 错误
    ///
    /// 对应 FLF character.js 中的 id_updates[characterId] 的 key
    /// 例如：id_updates[0].generic_combo, id_updates[0].TU 等
    /// </summary>
    public static class IdUpdateHooks
    {
        /// <summary>
        /// 通用连招处理 hook
        /// 对应 FLF: id_updates[characterId].generic_combo
        /// 调用时机：CharacterStates.HandleGenericCombo 在默认跳帧前
        /// </summary>
        public const string GenericCombo = "generic_combo";

        /// <summary>
        /// 状态进入 hook
        /// 对应 FLF: id_updates[characterId].state_entry
        /// 调用时机：进入新状态时
        /// </summary>
        public const string StateEntry = "state_entry";

        /// <summary>
        /// 蹲下
        /// </summary>
        public const string State15_Crouch = "state15_crouch";

        /// <summary>
        /// 状态退出 hook
        /// 对应 FLF: id_updates[characterId].state_exit
        /// 调用时机：退出当前状态时
        /// </summary>
        public const string StateExit = "state_exit";

        /// <summary>
        /// Time Unit（每帧更新）hook
        /// 对应 FLF: id_updates[characterId].TU
        /// 调用时机：每个 SimTick
        /// </summary>
        public const string TU = "TU";

        /// <summary>
        /// 帧力应用 hook
        /// 对应 FLF: id_updates[characterId].frame_force
        /// 调用时机：ApplyDynamics 前
        /// </summary>
        public const string FrameForce = "frame_force";

        // Future: 根据 FLF character.js 逐步补充其他 hooks
        // 例如：hit_stop, revert_transform 等
    }
}
