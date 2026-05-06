namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 通用引用池接口（对齐 ET ObjectPool.IPool 设计）
    /// 用于 Task、轻量数据对象等非逻辑对象的池化复用
    /// </summary>
    public interface ILF2Recyclable
    {
        bool IsFromPool { get; set; }
        void Clear();
    }
}
