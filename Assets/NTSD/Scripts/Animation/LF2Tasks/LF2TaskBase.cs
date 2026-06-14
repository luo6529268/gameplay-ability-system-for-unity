namespace NTSD.Animation.LF2Tasks
{
    /// <summary>
    /// LF2 运行时延迟任务基类。
    /// 当前主要用于在模拟阶段之间传递对齐 C++ 的 opoint 创建请求。
    /// </summary>
    public abstract class LF2TaskBase
    {
        /// <summary>
        /// 任务类型标识。
        /// </summary>
        public abstract LF2TaskType TaskType { get; }

        /// <summary>
        /// 可选任务优先级，默认是 0。
        /// </summary>
        public virtual int Priority => 0;
    }

    /// <summary>
    /// 当前支持的延迟任务类型。
    /// </summary>
    public enum LF2TaskType
    {
        CreateObject = 0,
        CreateMultipleObjects = 1,
        CreateNPCCharacters = 2,
        DestroyObject = 3,
    }
}
