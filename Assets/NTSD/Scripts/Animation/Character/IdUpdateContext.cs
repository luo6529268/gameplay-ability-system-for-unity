using BeatEmUpTemplate2D;
using MoreMountains.TopDownEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Animation
{
    /// <summary>
    /// id_update 上下文（对应 FLF 的 id_update 参数）
    ///
    /// 设计原则（Step D9）：
    /// - 使用 struct 避免 GC 分配
    /// - 所有字段 readonly，保证确定性
    /// - 只提供必要的只读引用（不允许修改 Hub/Animator 等外部状态）
    /// - handler 只能通过 ctx.Ps（数据层）与 ctx.Animator（表现层 API，例如 SetFacingDir）以及 ctx.Hub（Target/Grounding/SortingGroup 等接口）来实现
    ///
    /// 对应 FLF character.js 的 id_update 调用：
    /// - this.id_update('generic_combo', comboKey, comboTag)
    /// - this.id_update('state_entry', state)
    /// - this.id_update('frame_force')
    /// </summary>
    public readonly struct IdUpdateContext
    {
        // ==================== 核心引用（只读）====================

        /// <summary>
        /// Character Hub（宿主）
        /// handler 可以通过这个访问所有组件，但不应直接修改 Hub 状态
        /// </summary>
        public readonly Character Hub;

        /// <summary>
        /// 物理状态（对应 FLF 的 $.ps）
        /// handler 可以修改 ps.vx/vy/vz/x/y/z/dir 等物理属性
        /// </summary>
        public readonly PhysicsState Ps;

        // ==================== Hook 特定参数（可选）====================

        /// <summary>
        /// 连招键名（对应 FLF 的 K 参数）
        /// 例如："D>A", "DvA", "DJA", "att", "jump"
        /// 仅在 "generic_combo" hook 中有效
        /// </summary>
        public readonly string ComboKey;

        /// <summary>
        /// 连招标签（对应 FLF 的 tag 参数）
        /// 例如："hit_Fa", "hit_Da", "hit_ja"
        /// 仅在 "generic_combo" hook 中有效
        /// </summary>
        public readonly string ComboTag;

        /// <summary>
        /// 目标帧 ID（对应 FLF 的 frame.D[tag]）
        /// 例如：帧 60, 帧 100
        /// 仅在需要跳帧的 hook 中有效（generic_combo/state_entry 等）
        /// </summary>
        public readonly int TargetFrame;

        /// <summary>
        /// 状态 ID（对应 FLF 的 state 参数）
        /// 仅在 "state_entry"/"state_exit" hook 中有效
        /// </summary>
        public readonly int State;

        /// <summary>
        /// 当前 Tick 索引（用于确定性验证）
        /// handler 可以用这个代替 Time.time 进行时序逻辑
        /// </summary>
        public readonly int TickIndex;

        // ==================== 构造函数 ====================

        /// <summary>
        /// 创建 generic_combo hook 上下文
        /// </summary>
        public IdUpdateContext(
            Character hub,
            PhysicsState ps,
            string comboKey,
            string comboTag,
            int targetFrame,
            int tickIndex = 0)
        {
            Hub = hub;
            Ps = ps;
            ComboKey = comboKey;
            ComboTag = comboTag;
            TargetFrame = targetFrame;
            State = 0;
            TickIndex = tickIndex;
        }

        /// <summary>
        /// 创建 state_entry/state_exit hook 上下文
        /// </summary>
        public IdUpdateContext(
            Character hub,
            PhysicsState ps,
            int state,
            int tickIndex = 0)
        {
            Hub = hub;
            Ps = ps;
            ComboKey = null;
            ComboTag = null;
            TargetFrame = 0;
            State = state;
            TickIndex = tickIndex;
        }

        /// <summary>
        /// 创建通用 hook 上下文（frame_force/TU/hit_stop 等）
        /// </summary>
        public IdUpdateContext(
            Character hub,
            PhysicsState ps,
            int tickIndex = 0)
        {
            Hub = hub;
            Ps = ps;
            ComboKey = null;
            ComboTag = null;
            TargetFrame = 0;
            State = 0;
            TickIndex = tickIndex;
        }
    }
}
