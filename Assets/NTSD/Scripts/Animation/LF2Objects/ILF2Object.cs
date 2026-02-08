using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 对象统一接口（纯数据/逻辑层）
    /// 组合接口：继承 ILF2Poolable（对象池）+ ISimObject（模拟系统）
    ///
    /// 职责分离：
    /// - ILF2Poolable: 对象池生命周期（ObjectTypeEnum, ObjectId, Reset）
    /// - ISimObject: 模拟系统参与（SimTransit, SimTU, StableId）
    /// - ILF2Object: 完整对象接口（Init, Destroy, ObjectType）
    ///
    /// 参考：
    /// - FLF specialattack.prototype.init (specialattack.js:303)
    /// - FLF typeweapon.prototype.init (weapon.js:204)
    /// </summary>
    public interface ILF2Object : ILF2Poolable, ISimObject
    {
        /// <summary>
        /// 对象类型（int，向后兼容）
        /// 0: character, 1: lightweapon, 2: heavyweapon, 3: specialattack, 4: baseball, 5: criminal, 6: drink
        /// </summary>
        int ObjectType { get; }

        /// <summary>
        /// 初始化方法（对应 FLF 的 obj.init(T)）
        /// 负责：
        /// 1. 分配 StableId
        /// 2. 初始化位置、速度、方向、帧
        /// 3. 注册到 SimulationWorld
        /// </summary>
        /// <param name="task">创建任务数据</param>
        /// <param name="renderer">渲染器引用（用于访问 Animator）</param>
        void Init(LF2TaskBase task, LF2ObjectRenderer renderer);

        /// <summary>
        /// 销毁逻辑（对应 FLF destroy）
        /// </summary>
        void Destroy();
    }
}
