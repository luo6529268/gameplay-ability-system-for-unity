namespace NTSD.Simulation
{
    /// <summary>
    /// 可被 SimulationWorld 管理的模拟对象接口
    ///
    /// 与 ISimTickable 的区别：
    /// - ISimTickable: MonoBehaviour 组件，由 SimulationTickDriver 直接发现和驱动（Plan A）
    /// - ISimObject: 纯 C# 对象，由 SimulationWorld 管理，支持 StableId 确定性排序（Plan B）
    ///
    /// Plan B 架构：
    /// - 所有 gameplay 真相由 ISimObject 实现（纯 C# sim 模块）
    /// - MonoBehaviour 仅作为 "Hub"（组件缓存、注册/反注册）或 "View"（渲染、调试）
    /// - SimulationWorld 负责按确定性顺序（SimOrder → StableId）驱动所有 ISimObject
    ///
    /// FLF 对齐 (P0):
    /// - 每个 tick 分为两个阶段: Transit (all objects) → TU (all objects)
    /// - FlushTasks 在 Transit 和 TU 之间执行一次
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
        /// Transit 阶段 - 对应 FLF livingobject.transit()
        /// 
        /// 职责：输入处理、帧转换、物理
        /// 调用时机：所有对象的 Transit 先执行完，再执行 FlushTasks，再执行所有对象的 TU
        /// </summary>
        void SimTransit(int tickIndex);

        /// <summary>
        /// TU 阶段 - 对应 FLF livingobject.TU()
        /// 
        /// 职责：状态更新、武器点
        /// 调用时机：FlushTasks 之后
        /// </summary>
        void SimTU(int tickIndex);

        /// <summary>
        /// PostInteraction 阶段 - 对应 NTSD 反汇编 GameMode_Process (sub_41BDA0) 碰撞双层循环
        ///
        /// 职责：kind=0/4（普通攻击）的碰撞判定
        /// 调用时机：所有对象 SerialTickAll 完成后统一执行一次全局 pass
        /// 反汇编依据：GameMode_Process 在所有实体 sub_4063B0 执行完毕后，
        ///             才执行双层 for 循环做 itr/body 碰撞检测
        /// </summary>
        void SimPostInteraction(int tickIndex) { }

        /// <summary>
        /// PreInteraction 阶段 - 对应 NTSD 反汇编 GameMode_Process (sub_41BDA0)
        ///
        /// 职责：kind=1/2/3/7（抓取、拾取）的碰撞判定
        /// 调用时机：所有对象 SerialTickAll 完成后、LateTick 之前统一执行一次全局 pass
        /// 目的：保证联机帧同步一致性（不依赖 StableId 顺序）
        /// </summary>
        void SimPreInteraction(int tickIndex) { }

        /// <summary>
        /// 每个模拟 Tick 的后期处理（可选）
        /// </summary>
        void SimLateTick(int tickIndex) { }
    }
}
