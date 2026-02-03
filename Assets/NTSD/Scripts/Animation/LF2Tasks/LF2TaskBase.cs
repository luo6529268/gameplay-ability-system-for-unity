namespace NTSD.Animation.LF2Tasks
{
    /// <summary>
    /// LF2 任务基类
    /// 对应 FLF match.js tasks 数组中的任务对象
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\match.js:194
    /// </summary>
    public abstract class LF2TaskBase
    {
        /// <summary>任务类型</summary>
        public abstract LF2TaskType TaskType { get; }

        /// <summary>任务优先级（可选，默认 0）</summary>
        public virtual int Priority => 0;
    }

    /// <summary>
    /// 任务类型枚举
    /// 对应 FLF match.js process_task 的 case 分支
    /// </summary>
    public enum LF2TaskType
    {
        CreateObject = 0,           // 'create_object'
        CreateMultipleObjects = 1,  // 'create_multiple_objects'
        CreateNPCCharacters = 2,    // 'create_non_player_characters'
        DestroyObject = 3,          // 预留：对象销毁任务
    }
}
