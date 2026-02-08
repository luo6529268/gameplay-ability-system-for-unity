namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 对象池生命周期接口
    /// 定义对象池管理所需的最小能力集
    ///
    /// 职责分离设计：
    /// - ILF2Poolable: 对象池生命周期（获取、归还、重置）
    /// - ISimObject: 模拟系统参与（SimTransit、SimTU）
    /// - ILF2Object: 组合接口，继承两者
    ///
    /// 使用场景：
    /// - LF2ObjectLogicPool 使用此接口管理对象池
    /// - 对象池只关心 ObjectTypeEnum、ObjectId、Reset()
    /// </summary>
    public interface ILF2Poolable
    {
        /// <summary>
        /// 对象类型枚举（用于池分组）
        /// </summary>
        LF2ObjectType ObjectTypeEnum { get; }

        /// <summary>
        /// 对象 ID（oid，数据定义 ID）
        /// </summary>
        int ObjectId { get; set; }

        /// <summary>
        /// 重置状态（归还对象池前调用）
        /// 负责：
        /// 1. 从 SimulationWorld 反注册
        /// 2. 清空所有字段
        /// 3. 重置 StableId
        /// </summary>
        void Reset();
    }
}
