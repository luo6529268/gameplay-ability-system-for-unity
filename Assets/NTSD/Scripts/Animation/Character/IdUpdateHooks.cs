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

        /// <summary>
        /// Rudolf 变身 hook（对应 FLF id_updates[5].rudolf_transform）
        /// 调用时机：state_9 帧240 / state_501 帧298
        /// </summary>
        public const string RudolfTransform = "rudolf_transform";

        /// <summary>
        /// Rudolf 解除变身 hook（对应 FLF default.revert_transform）
        /// 调用时机：Generic_Combo DJA 且处于变身状态
        /// </summary>
        public const string RevertTransform = "revert_transform";

        // state_3 专用 hooks（对应 FLF character.js id_updates 各角色分支）
        public const string State3Frame        = "state3_frame";        // 每帧触发（frame 事件）
        public const string State3FlyCrash     = "state3_fly_crash";    // 帧253：飞行碰撞
        public const string State3HitStop      = "state3_hit_stop";     // hit_stop 事件
        public const string State3FrameForce   = "state3_frame_force";  // frame_force 事件
        public const string State1280Disappear = "state1280_disappear"; // 帧257：消失
    }
}
