using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Load
{
    public sealed class NTSD_ResourceLoader
    {
        private class DomainStats
        {
            public int TotalTasks;
            public int CacheHits;
            public int CacheMisses;

            public float HitRate => TotalTasks > 0 ? (float)CacheHits / TotalTasks : 0f;
        }

        private static readonly NTSD_ResourceLoader instance = new NTSD_ResourceLoader();

        public static NTSD_ResourceLoader Instance => instance;

        private NTSD_ResourceLoader()
        {
        }
        public int MaxTasksPerFrame { get; set; } = 4;

        private readonly LinkedList<NTSD_LoadTask> tasks = new LinkedList<NTSD_LoadTask>();
        private readonly Dictionary<string, object> cache = new Dictionary<string, object>();
        private readonly Dictionary<string, NTSD_ResourceRequest> requests = new Dictionary<string, NTSD_ResourceRequest>();
        private readonly SortedDictionary<int, LinkedList<NTSD_LoadTask>> priorityBuckets = new SortedDictionary<int, LinkedList<NTSD_LoadTask>>();
        private readonly Dictionary<NTSD_ResourceDomain, DomainStats> _domainStats = new Dictionary<NTSD_ResourceDomain, DomainStats>();

        public event System.Action<NTSD_LoadTask> TaskStarted;
        public event System.Action<NTSD_LoadTask> TaskCompleted;
        public event System.Action<NTSD_LoadTask, System.Exception> TaskFailed;
        public event System.Action AllTasksCompleted;

        private bool isRunning;

        public void AddTask(NTSD_LoadTask task)
        {
            if (task == null)
            {
                return;
            }

            if (task.Status != NTSD_LoadTaskStatus.Pending)
            {
                task.Status = NTSD_LoadTaskStatus.Pending;
            }

            if (!string.IsNullOrWhiteSpace(task.CacheKey) && cache.ContainsKey(task.CacheKey))
            {
                var hitStats = GetOrCreateDomainStats(task.Domain);
                hitStats.TotalTasks++;
                hitStats.CacheHits++;

                task.Result = cache[task.CacheKey];
                task.Status = NTSD_LoadTaskStatus.Completed;
                task.OnCompleted?.Invoke(task);
                TaskCompleted?.Invoke(task);
                LogTaskCompleted(task);
                return;
            }

            if (!string.IsNullOrWhiteSpace(task.CacheKey) && requests.TryGetValue(task.CacheKey, out var existingRequest))
            {
                existingRequest.Register(task);
                return;
            }

            if (!string.IsNullOrWhiteSpace(task.CacheKey))
            {
                var request = new NTSD_ResourceRequest(task.CacheKey);
                request.Register(task);
                requests[task.CacheKey] = request;
                BindRequestHandlers(task, request);
            }

            var missStats = GetOrCreateDomainStats(task.Domain);
            missStats.TotalTasks++;
            missStats.CacheMisses++;

            tasks.AddLast(task);
            AddToBucket(task);
        }


        public async UniTask ProcessFrame()
        {
            if (isRunning)
            {
                return;
            }

            isRunning = true;
            int processed = 0;

            foreach (var bucket in EnumerateBucketsDescending())
            {
                var list = bucket.Value;
                if (list == null || list.Count == 0)
                {
                    continue;
                }

                var node = list.Last;
                while (node != null)
                {
                    if (processed >= MaxTasksPerFrame)
                    {
                        isRunning = false;
                        return;
                    }

                    var task = node.Value;
                    var prev = node.Previous;
                    if (task == null)
                    {
                        list.Remove(node);
                        node = prev;
                        continue;
                    }

                    if (task.IsCancelled)
                    {
                        task.Status = NTSD_LoadTaskStatus.Cancelled;
                        list.Remove(node);
                        node = prev;
                        continue;
                    }

                    if (task.IsPaused)
                    {
                        continue;
                    }

                    await ExecuteTask(task);
                    list.Remove(node);
                    processed++;

                    node = prev;
                }
            }

            if (priorityBuckets.Count == 0)
            {
                AllTasksCompleted?.Invoke();
            }

            isRunning = false;
        }

        private IEnumerable<KeyValuePair<int, LinkedList<NTSD_LoadTask>>> EnumerateBucketsDescending()
        {
            var pairs = new List<KeyValuePair<int, LinkedList<NTSD_LoadTask>>>(priorityBuckets);
            for (int i = pairs.Count - 1; i >= 0; i--)
            {
                yield return pairs[i];
            }
        }

        public void CacheResult(string cacheKey, object result)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return;
            }

            cache[cacheKey] = result;
        }

        public bool IsCached(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return false;
            }

            return cache.ContainsKey(cacheKey);
        }

        public bool TryGetCache(string cacheKey, out object result)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                result = null;
                return false;
            }

            return cache.TryGetValue(cacheKey, out result);
        }

        public void ClearCache()
        {
            cache.Clear();
        }

        public bool RemoveCache(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return false;
            }

            return cache.Remove(cacheKey);
        }

        #region RequestMap State Check

        /// <summary>
        /// 获取当前进行中的请求数量。
        /// </summary>
        public int GetPendingRequestCount()
        {
            return requests.Count;
        }

        /// <summary>
        /// 检查是否有进行中的请求。
        /// </summary>
        public bool HasPendingRequests()
        {
            return requests.Count > 0;
        }

        /// <summary>
        /// 检查指定 CacheKey 是否有进行中的请求。
        /// </summary>
        public bool HasPendingRequest(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return false;
            }

            return requests.ContainsKey(cacheKey);
        }

        /// <summary>
        /// 获取指定 CacheKey 的请求（如果存在）。
        /// </summary>
        public bool TryGetRequest(string cacheKey, out NTSD_ResourceRequest request)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                request = null;
                return false;
            }

            return requests.TryGetValue(cacheKey, out request);
        }

        /// <summary>
        /// 获取所有进行中请求的 CacheKey 列表。
        /// </summary>
        public IReadOnlyCollection<string> GetPendingRequestKeys()
        {
            return requests.Keys;
        }

        /// <summary>
        /// 获取当前队列中的任务总数。
        /// </summary>
        public int GetQueuedTaskCount()
        {
            int count = 0;
            foreach (var bucket in priorityBuckets.Values)
            {
                count += bucket.Count;
            }
            return count;
        }

        /// <summary>
        /// 检查加载器是否空闲（无进行中请求且无队列任务）。
        /// </summary>
        public bool IsIdle()
        {
            return requests.Count == 0 && GetQueuedTaskCount() == 0;
        }

        #endregion

        private void AddToBucket(NTSD_LoadTask task)
        {
            if (!priorityBuckets.TryGetValue(task.Priority, out var list))
            {
                list = new LinkedList<NTSD_LoadTask>();
                priorityBuckets[task.Priority] = list;
            }

            list.AddLast(task);
        }

        private async UniTask ExecuteTask(NTSD_LoadTask task)
        {
            if (task.Execute == null)
            {
                return;
            }

            task.Status = NTSD_LoadTaskStatus.Running;
            TaskStarted?.Invoke(task);

            try
            {
                await task.Execute(task, this);
                if (!string.IsNullOrWhiteSpace(task.CacheKey))
                {
                    CacheResult(task.CacheKey, task.Result);
                }
                task.Status = NTSD_LoadTaskStatus.Completed;
                task.OnCompleted?.Invoke(task);
                TaskCompleted?.Invoke(task);
                LogTaskCompleted(task);

                if (!string.IsNullOrWhiteSpace(task.CacheKey))
                {
                    requests.Remove(task.CacheKey);
                }
            }
            catch (Exception ex)
            {
                task.Status = NTSD_LoadTaskStatus.Failed;
                task.OnFailed?.Invoke(task, ex);
                TaskFailed?.Invoke(task, ex);

                if (task.CurrentRetries < task.MaxRetries)
                {
                    task.CurrentRetries++;
                    task.Status = NTSD_LoadTaskStatus.Pending;
                    AddToBucket(task);
                }
                else if (!string.IsNullOrWhiteSpace(task.CacheKey))
                {
                    requests.Remove(task.CacheKey);
                }
            }
        }

        private static void BindRequestHandlers(NTSD_LoadTask task, NTSD_ResourceRequest request)
        {
            task.OnCompleted = request.NotifyCompleted;
            task.OnFailed = request.NotifyFailed;
            task.OnProgress = request.NotifyProgress;
            task.OnProgressText = request.NotifyProgressText;
        }

        public void CancelTask(NTSD_LoadTask task)
        {
            if (task == null)
            {
                return;
            }

            task.IsCancelled = true;
            task.Status = NTSD_LoadTaskStatus.Cancelled;
        }

        public void PauseTask(NTSD_LoadTask task)
        {
            if (task == null)
            {
                return;
            }

            task.IsPaused = true;
        }

        public void ResumeTask(NTSD_LoadTask task)
        {
            if (task == null)
            {
                return;
            }

            task.IsPaused = false;
        }

        public void RetryTask(NTSD_LoadTask task)
        {
            if (task == null)
            {
                return;
            }

            if (task.Status != NTSD_LoadTaskStatus.Failed)
            {
                return;
            }

            task.Status = NTSD_LoadTaskStatus.Pending;
            task.IsCancelled = false;
            AddToBucket(task);
        }

        public async UniTask<byte[]> LoadBytesAsync(NTSD_LoadSourceType sourceType, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            switch (sourceType)
            {
                case NTSD_LoadSourceType.FilePath:
                    return await UniTask.RunOnThreadPool(() => System.IO.File.ReadAllBytes(path));
                case NTSD_LoadSourceType.ResourcesPath:
                    return await LoadBytesFromResourcesAsync(path);
                default:
                    return null;
            }
        }

        private async UniTask<byte[]> LoadBytesFromResourcesAsync(string resourcesPath)
        {
            await UniTask.SwitchToMainThread();
            var textAsset = Resources.Load<TextAsset>(resourcesPath);
            return textAsset != null ? textAsset.bytes : null;
        }

        private DomainStats GetOrCreateDomainStats(NTSD_ResourceDomain domain)
        {
            if (!_domainStats.TryGetValue(domain, out var stats))
            {
                stats = new DomainStats();
                _domainStats[domain] = stats;
            }
            return stats;
        }

        private void LogTaskCompleted(NTSD_LoadTask task)
        {
            var stats = GetOrCreateDomainStats(task.Domain);
            Log.Info($"[NTSD_ResourceLoader] Task completed: {task.Name}\n" +
                     $"  Domain: {task.Domain}\n" +
                     $"  Stats: Total={stats.TotalTasks}, Hits={stats.CacheHits}, Misses={stats.CacheMisses}, HitRate={stats.HitRate:P1}\n" +
                     $"  Queue: {GetQueuedTaskCount()}, Cache: {cache.Count}");
        }
    }
}
