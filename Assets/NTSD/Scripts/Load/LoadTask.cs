using System;
using Cysharp.Threading.Tasks;

namespace NTSD.Load
{
    public enum NTSD_LoadTaskType
    {
        LoadConfig,
        LoadSprites,
        Warmup,
        Other
    }

    public enum NTSD_ResourceDomain
    {
        Character,
        UI,
        Effect,
        Scene,
        Other
    }

    public enum NTSD_LoadSourceType
    {
        FilePath,
        ResourcesPath
    }

    public enum NTSD_LoadTaskStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled
    }

    public sealed class NTSD_LoadTask
    {
        /// <summary>
        /// 调试/日志用名称（可选）。
        /// </summary>
        public string Name;

        /// <summary>
        /// 任务分类，用于排序和过滤。
        /// </summary>
        public NTSD_LoadTaskType Type;

        /// <summary>
        /// 资源归属领域，用于路由与缓存分区。
        /// </summary>
        public NTSD_ResourceDomain Domain;

        /// <summary>
        /// 优先级，值越大优先级越高。
        /// </summary>
        public int Priority;

        /// <summary>
        /// 缓存键，命中缓存时直接跳过任务。
        /// </summary>
        public string CacheKey;

        /// <summary>
        /// 任务结果，完成时会存入缓存（可选）。
        /// </summary>
        public object Result;

        /// <summary>
        /// 加载来源类型（可选）。
        /// </summary>
        public NTSD_LoadSourceType SourceType;

        /// <summary>
        /// 加载来源路径（可选）。
        /// </summary>
        public string SourcePath;

        /// <summary>
        /// 任务执行器：后台处理（IO/解码）+ 主线程调用 Unity API。
        /// </summary>
        public delegate UniTask TaskExecutor(NTSD_LoadTask task, NTSD_ResourceLoader loader);
        public TaskExecutor Execute;

        /// <summary>
        /// 任务成功完成回调。
        /// </summary>
        public Action<NTSD_LoadTask> OnCompleted;

        /// <summary>
        /// 任务失败回调。
        /// </summary>
        public Action<NTSD_LoadTask, Exception> OnFailed;

        /// <summary>
        /// 进度回调（0-1）。
        /// </summary>
        public Action<float> OnProgress;

        /// <summary>
        /// 进度文本回调（例如当前加载的文件路径）。
        /// </summary>
        public Action<string> OnProgressText;

        /// <summary>
        /// 失败时最大重试次数。
        /// </summary>
        public int MaxRetries = 0;

        /// <summary>
        /// 当前已重试次数。
        /// </summary>
        public int CurrentRetries = 0;

        /// <summary>
        /// 当前任务状态。
        /// </summary>
        public NTSD_LoadTaskStatus Status = NTSD_LoadTaskStatus.Pending;

        /// <summary>
        /// 是否已取消（进入执行前有效）。
        /// </summary>
        public bool IsCancelled;

        /// <summary>
        /// 是否暂停（保留在队列中，等待恢复）。
        /// </summary>
        public bool IsPaused;

        public void Reset()
        {
            Name = null;
            CacheKey = null;
            Execute = null;
            OnCompleted = null;
            OnFailed = null;
            OnProgress = null;
            MaxRetries = 0;
            CurrentRetries = 0;
            Status = NTSD_LoadTaskStatus.Pending;
            IsCancelled = false;
            IsPaused = false;
            Domain = NTSD_ResourceDomain.Other;
        }
    }
}
