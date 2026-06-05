namespace NTSD.Simulation
{
    /// <summary>
    /// 可被 SimulationWorld 管理的战斗模拟对象接口。
    /// 纯 C# 逻辑对象由 SimulationWorld 按 SimOrder 和 StableId 稳定驱动，
    /// MonoBehaviour 只负责渲染、输入桥接和对象池装配。
    ///
    /// C++ release 战斗对齐目标：
    /// - 每个对象按 frame/wait/next 推进，再执行对象自身的逐 tick 逻辑
    /// - opoint 创建请求在确定的模拟阶段刷新，保证生成顺序可复现
    /// </summary>
    public interface ISimObject
    {
        /// <summary>
        /// 执行顺序（第一优先级）
        /// </summary>
        int SimOrder { get; }

        /// <summary>
        /// 稳定 ID（第二优先级，用于确定性排序）
        /// </summary>
        int StableId { get; }

        /// <summary>
        /// 当对象被添加到 SimulationWorld 时调用
        /// </summary>
        void OnAdded(SimContext ctx);

        /// <summary>
        /// 当对象从 SimulationWorld 移除时调用
        /// </summary>
        void OnRemoved(SimContext ctx);

        /// <summary>
        /// Transit 阶段：处理帧推进、帧请求和物理前置状态。
        /// 
        /// 职责：输入处理、帧转换、物理
        /// 调用时机：SimulationWorld.SerialTickAll 按对象顺序执行 Transit -> FlushTasks -> TU。
        /// </summary>
        void SimTransit(int tickIndex);

        /// <summary>
        /// TU 阶段：处理对象逐 tick 状态逻辑。
        /// 
        /// 职责：状态更新、武器点
        /// 调用时机：当前对象的 FlushTasks 之后。
        /// </summary>
        void SimTU(int tickIndex);

        /// <summary>
        /// PostInteraction 阶段 - 对齐 C++ release 角色攻击碰撞路径。
        ///
        /// 职责：kind=0/4（普通攻击）的碰撞判定
        /// 调用时机：所有对象 SerialTickAll 完成后统一执行一次全局 pass
        /// C++ release 依据：帧推进完成后先执行角色侧 itr/body 碰撞检测，再进入随机武器掉落。
        /// </summary>
        void SimPostInteraction(int tickIndex) { }

        /// <summary>
        /// PreInteraction 阶段 - 对齐 C++ release 随机武器掉落后的抓取/拾取/cpoint 类碰撞路径
        ///
        /// 职责：kind=1/2/3/7（抓取、拾取）的碰撞判定
        /// 调用时机：PostInteraction 与 RandomWeaponDrop 完成后、FramePostProcess 之前统一执行一次全局 pass
        /// 目的：让抓取/拾取结果由固定全局 pass 决定。
        /// </summary>
        void SimPreInteraction(int tickIndex) { }

        /// <summary>
        /// EntityCollision 阶段 - 对齐 C++ release 的实体碰撞路径
        ///
        /// 职责：武器地面/边界碰撞、state/type 特殊分支（N-1~N-5）
        /// 调用时机：FramePostProcessAll 之后，在 LateEntityUpdateAll 中按实体顺序执行
        /// </summary>
        void SimEntityCollision(int tickIndex) { }

        /// <summary>
        /// 每个模拟 Tick 的后期处理（可选），在同一实体的 SimEntityCollision 后立即执行。
        /// </summary>
        void SimLateTick(int tickIndex) { }
    }
}
