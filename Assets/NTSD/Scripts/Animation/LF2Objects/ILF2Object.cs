using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 对象统一接口（纯数据/逻辑层）
    /// 继承 ISimObject，保证生命周期统一
    ///
    /// 对应 FLF 各对象类型的 init 方法
    ///
    /// 参考：
    /// - FLF specialattack.prototype.init (specialattack.js:303)
    /// - FLF typeweapon.prototype.init (weapon.js:204)
    /// </summary>
    public interface ILF2Object : ISimObject
    {
        /// <summary>
        /// 对象类型枚举（强类型）
        /// </summary>
        LF2ObjectType ObjectTypeEnum { get; }

        /// <summary>
        /// 对象类型（int，向后兼容）
        /// 0: character, 1: lightweapon, 2: heavyweapon, 3: specialattack, 4: baseball, 5: criminal, 6: drink
        /// </summary>
        int ObjectType { get; }

        /// <summary>
        /// 对象 ID（oid）
        /// </summary>
        int ObjectId { get; set; }

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
        /// 重置状态（归还引用池前调用）
        /// 负责：
        /// 1. 从 SimulationWorld 反注册
        /// 2. 清空所有字段
        /// 3. 重置 StableId
        /// </summary>
        void Reset();

        /// <summary>
        /// 销毁逻辑（对应 FLF destroy）
        /// </summary>
        void Destroy();
    }
}
