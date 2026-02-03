using System;
using System.Collections.Generic;

namespace NTSD.Load
{
    public sealed class NTSD_ResourceRequest
    {
        private readonly List<NTSD_LoadTask> tasks = new List<NTSD_LoadTask>();
        private readonly List<Action<NTSD_LoadTask>> originalOnCompleted = new List<Action<NTSD_LoadTask>>();
        private readonly List<Action<NTSD_LoadTask, Exception>> originalOnFailed = new List<Action<NTSD_LoadTask, Exception>>();

        public NTSD_ResourceRequest(string cacheKey)
        {
            CacheKey = cacheKey;
        }

        public string CacheKey { get; }

        public void Register(NTSD_LoadTask task)
        {
            if (task == null)
            {
                return;
            }

            originalOnCompleted.Add(task.OnCompleted);
            originalOnFailed.Add(task.OnFailed);
            tasks.Add(task);
        }

        public void NotifyCompleted(NTSD_LoadTask task)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                var t = tasks[i];
                t.Result = task.Result;
                t.Status = NTSD_LoadTaskStatus.Completed;
                originalOnCompleted[i]?.Invoke(t);
            }
        }

        public void NotifyFailed(NTSD_LoadTask task, Exception exception)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                var t = tasks[i];
                t.Status = NTSD_LoadTaskStatus.Failed;
                originalOnFailed[i]?.Invoke(t, exception);
            }
        }

        public void NotifyProgress(float progress)
        {
            foreach (var t in tasks)
            {
                t.OnProgress?.Invoke(progress);
            }
        }

        public void NotifyProgressText(string text)
        {
            foreach (var t in tasks)
            {
                t.OnProgressText?.Invoke(text);
            }
        }
    }
}
