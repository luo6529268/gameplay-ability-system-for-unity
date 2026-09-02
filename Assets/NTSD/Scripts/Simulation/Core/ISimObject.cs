namespace NTSD.Simulation
{
    /// <summary>
    /// 可被 SimulationWorld 管理的战斗模拟对象接口。
    /// 纯 C# 逻辑对象由 SimulationWorld 按 SimOrder 和 StableId 稳定驱动；
    /// MonoBehaviour 只负责渲染、输入桥接和对象池装配。
    ///
    /// C++ release 战斗对齐目标：
    /// - 所有对象先统一完成 frame advance，再进入后续 AI_Process2/碰撞/后处理 pass。
    /// - opoint 创建请求在确定的模拟阶段刷新，保证生成顺序可复现。
    /// </summary>
    public interface ISimObject
    {
        /// <summary>
        /// 执行顺序第一优先级。
        /// </summary>
        int SimOrder { get; }

        /// <summary>
        /// 稳定 ID 第二优先级，用于确定性排序。
        /// </summary>
        int StableId { get; }

        /// <summary>
        /// 对象被添加到 SimulationWorld 时调用。
        /// </summary>
        void OnAdded(SimContext ctx) { }

        /// <summary>
        /// 对象从 SimulationWorld 移除时调用。
        /// </summary>
        void OnRemoved(SimContext ctx) { }

        /// <summary>
        /// Transit 阶段：处理帧推进、帧请求和物理前置状态。
        /// 调用时机：SimulationWorld.SerialTickAll 中，所有对象按确定顺序统一执行 Transit。
        /// </summary>
        void SimTransit(int tickIndex) { }

        /// <summary>
        /// TU 阶段：处理对象每 tick 状态逻辑。
        /// 调用时机：所有对象 Transit 完成后，再按确定顺序统一执行 TU。
        /// </summary>
        void SimTU(int tickIndex) { }

        /// <summary>
        /// 角色攻击交互阶段，对齐 C++ release step7：entity_type==0 的 Entity_AI_Update。
        /// </summary>
        void SimPostInteraction(int tickIndex) { }

        /// <summary>
        /// 非角色攻击交互阶段，对齐 C++ release step9：entity_type&gt;0 的 Entity_AI_Update。
        /// </summary>
        void SimObjectInteraction(int tickIndex) { }

        /// <summary>
        /// 抓取、拾取和 cpoint 类交互阶段，对齐 C++ release step10 的 Collision_Check1/2。
        /// </summary>

        /// <summary>
        /// Reserved compatibility hook for legacy implementers.
        /// Production collision handling stays in SimulationWorld's authoritative collision passes.
        /// </summary>
        /// <remarks>
        /// Reserved compatibility hook; production collision passes do not dispatch this method.
        /// </remarks>
        [System.Obsolete(
            "Reserved compatibility hook only. SimulationWorld no longer invokes SimEntityCollision as a production pass.")]
        void SimEntityCollision(int tickIndex) { }

        /// <summary>
        /// C++ release frame_tick 阶段：在 late entity update 中推进 wait/next。
        /// </summary>
        void SimFrameTick(int tickIndex) { }

        /// <summary>
        /// Presentation-only late hook. It is dispatched by the render/presentation
        /// phase after simulation and is not ordered after SimEntityCollision.
        /// </summary>
        void SimLateTick(int tickIndex) { }
    }
}
