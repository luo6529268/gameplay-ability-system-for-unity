using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using NTSD.Tools;
using UnityEngine;
using UnityEngine.Networking;

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

        private readonly Dictionary<string, object> cache = new Dictionary<string, object>();
        private readonly Dictionary<string, NTSD_ResourceRequest> requests = new Dictionary<string, NTSD_ResourceRequest>();
        private readonly SortedDictionary<int, LinkedList<NTSD_LoadTask>> priorityBuckets = new SortedDictionary<int, LinkedList<NTSD_LoadTask>>();
        private readonly List<KeyValuePair<int, LinkedList<NTSD_LoadTask>>> priorityBucketSnapshot =
            new List<KeyValuePair<int, LinkedList<NTSD_LoadTask>>>(8);
        private readonly Dictionary<NTSD_ResourceDomain, DomainStats> _domainStats = new Dictionary<NTSD_ResourceDomain, DomainStats>();

        public event System.Action<NTSD_LoadTask> TaskStarted;
        public event System.Action<NTSD_LoadTask> TaskCompleted;
        public event System.Action<NTSD_LoadTask, System.Exception> TaskFailed;
        public event System.Action AllTasksCompleted;

        private bool isRunning;
        private int queuedTaskCount;

        public bool HasQueuedTasks => queuedTaskCount > 0;

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

            AddToBucket(task);
        }

        public async UniTask<T> EnqueueTaskAsync<T>(NTSD_LoadTask task)
        {
            if (task == null)
            {
                return default;
            }

            var completionSource = new UniTaskCompletionSource<T>();
            Action<NTSD_LoadTask> originalCompleted = task.OnCompleted;
            Action<NTSD_LoadTask, Exception> originalFailed = task.OnFailed;

            task.OnCompleted = completedTask =>
            {
                originalCompleted?.Invoke(completedTask);

                if (completedTask.Result == null)
                {
                    completionSource.TrySetResult(default);
                    return;
                }

                if (completedTask.Result is T result)
                {
                    completionSource.TrySetResult(result);
                    return;
                }

                completionSource.TrySetException(new InvalidCastException($"Unable to cast load result of type '{completedTask.Result.GetType().FullName}' to '{typeof(T).FullName}'."));
            };

            task.OnFailed = (failedTask, exception) =>
            {
                originalFailed?.Invoke(failedTask, exception);
                completionSource.TrySetException(exception);
            };

            AddTask(task);

            if (!isRunning)
            {
                await ProcessFrame();
            }

            return await completionSource.Task;
        }

        public async UniTask<AudioClip> LoadSingleAudioClipAsync(string cacheKey, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            if (!string.IsNullOrWhiteSpace(cacheKey) && TryGetCache(cacheKey, out object cached) && cached is AudioClip cachedClip)
                return cachedClip;

            AudioClip clip = await LoadAudioClipFromFileAsync(filePath);
            if (clip != null && !string.IsNullOrWhiteSpace(cacheKey))
                CacheResult(cacheKey, clip);

            return clip;
        }

        public async UniTask<AudioClip[]> LoadAudioClipsAsync(string cacheKey, string directoryPath, int priority = 0)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return Array.Empty<AudioClip>();
            }

            if (!string.IsNullOrWhiteSpace(cacheKey) && TryGetCache(cacheKey, out object cachedResult) && cachedResult is AudioClip[] cachedClips)
            {
                return cachedClips;
            }

            var task = new NTSD_LoadTask
            {
                Name = $"Load Audio Clips: {directoryPath}",
                Type = NTSD_LoadTaskType.LoadAudio,
                Domain = NTSD_ResourceDomain.Audio,
                Priority = priority,
                CacheKey = cacheKey,
                SourceType = NTSD_LoadSourceType.FilePath,
                SourcePath = directoryPath,
                Execute = async (loadTask, _) =>
                {
                    loadTask.Result = await LoadAudioClipsFromDirectoryAsync(directoryPath, loadTask);
                }
            };

            AudioClip[] clips = await EnqueueTaskAsync<AudioClip[]>(task);
            return clips ?? Array.Empty<AudioClip>();
        }


        public UniTask ProcessFrame()
        {
            if (isRunning || queuedTaskCount <= 0)
            {
                return UniTask.CompletedTask;
            }

            return ProcessQueuedTasksAsync();
        }

        private async UniTask ProcessQueuedTasksAsync()
        {
            isRunning = true;
            int processed = 0;
            bool completedAllTasks = false;

            priorityBucketSnapshot.Clear();
            foreach (var bucket in priorityBuckets)
                priorityBucketSnapshot.Add(bucket);

            try
            {
                for (int bucketIndex = priorityBucketSnapshot.Count - 1; bucketIndex >= 0; bucketIndex--)
                {
                    KeyValuePair<int, LinkedList<NTSD_LoadTask>> bucket = priorityBucketSnapshot[bucketIndex];
                    LinkedList<NTSD_LoadTask> list = bucket.Value;
                    if (list == null || list.Count == 0)
                    {
                        priorityBuckets.Remove(bucket.Key);
                        continue;
                    }

                    LinkedListNode<NTSD_LoadTask> node = list.Last;
                    while (node != null)
                    {
                        if (processed >= MaxTasksPerFrame)
                            return;

                        NTSD_LoadTask task = node.Value;
                        LinkedListNode<NTSD_LoadTask> previous = node.Previous;
                        if (task == null)
                        {
                            RemoveQueuedNode(list, node);
                            node = previous;
                            continue;
                        }

                        if (task.IsCancelled)
                        {
                            task.Status = NTSD_LoadTaskStatus.Cancelled;
                            RemoveQueuedNode(list, node);
                            node = previous;
                            continue;
                        }

                        if (task.IsPaused)
                        {
                            node = previous;
                            continue;
                        }

                        await ExecuteTask(task);
                        RemoveQueuedNode(list, node);
                        processed++;

                        node = previous;
                    }

                    if (list.Count == 0)
                        priorityBuckets.Remove(bucket.Key);
                }

                completedAllTasks = queuedTaskCount == 0;
            }
            finally
            {
                priorityBucketSnapshot.Clear();
                isRunning = false;
            }

            if (completedAllTasks)
                AllTasksCompleted?.Invoke();
        }

        private void RemoveQueuedNode(
            LinkedList<NTSD_LoadTask> list,
            LinkedListNode<NTSD_LoadTask> node)
        {
            list.Remove(node);
            if (queuedTaskCount > 0)
                queuedTaskCount--;
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
            return queuedTaskCount;
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
            queuedTaskCount++;
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
            // OnProgress / OnProgressText 不绑定：
            // request.NotifyXxx 会遍历所有 task 调用各自的 OnProgressXxx，
            // 若绑定则形成无限递归 → 栈溢出崩溃。
            // 音频加载无需 progress 回调，调用方忽略即可。
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

        private async UniTask<AudioClip[]> LoadAudioClipsFromDirectoryAsync(string directoryPath, NTSD_LoadTask task)
        {
            string normalizedDirectory = NormalizeDirectoryPath(directoryPath);
            if (!Directory.Exists(normalizedDirectory))
            {
                Log.Warn($"[NTSD_ResourceLoader] Audio directory not found: {normalizedDirectory}");
                return Array.Empty<AudioClip>();
            }

            string[] audioFiles = Directory
                .EnumerateFiles(normalizedDirectory)
                .Where(IsSupportedAudioFile)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (audioFiles.Length == 0)
            {
                Log.Warn($"[NTSD_ResourceLoader] No audio files found in directory: {normalizedDirectory}");
                return Array.Empty<AudioClip>();
            }

            var clips = new List<AudioClip>(audioFiles.Length);

            for (int i = 0; i < audioFiles.Length; i++)
            {
                string audioFile = audioFiles[i];
                task?.OnProgressText?.Invoke(audioFile);
                task?.OnProgress?.Invoke((float)i / audioFiles.Length);

                AudioClip clip = await LoadAudioClipFromFileAsync(audioFile);
                if (clip != null)
                {
                    clips.Add(clip);
                }
            }

            task?.OnProgress?.Invoke(1f);
            return clips.ToArray();
        }

        private async UniTask<AudioClip> LoadAudioClipFromFileAsync(string filePath)
        {
            AudioType audioType = GetAudioType(filePath);
            if (audioType == AudioType.UNKNOWN)
            {
                return null;
            }

            string uri = new Uri(filePath).AbsoluteUri;
            using var request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            request.disposeDownloadHandlerOnDispose = true;

            await request.SendWebRequest().ToUniTask();

#if UNITY_2020_2_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isHttpError || request.isNetworkError)
#endif
            {
                Log.Warn($"[NTSD_ResourceLoader] Failed to load audio clip: {filePath}, error: {request.error}");
                return null;
            }

            var clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip != null)
            {
                clip.name = Path.GetFileNameWithoutExtension(filePath);
            }

            return clip;
        }

        private bool IsSupportedAudioFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return string.Equals(extension, ".wav", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(extension, ".ogg", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(extension, ".mp3", StringComparison.OrdinalIgnoreCase);
        }

        private AudioType GetAudioType(string filePath)
        {
            string extension = Path.GetExtension(filePath);

            if (string.Equals(extension, ".wav", StringComparison.OrdinalIgnoreCase))
            {
                return AudioType.WAV;
            }

            if (string.Equals(extension, ".ogg", StringComparison.OrdinalIgnoreCase))
            {
                return AudioType.OGGVORBIS;
            }

            if (string.Equals(extension, ".mp3", StringComparison.OrdinalIgnoreCase))
            {
                return AudioType.MPEG;
            }

            return AudioType.UNKNOWN;
        }

        private string NormalizeDirectoryPath(string directoryPath)
        {
            return Path.GetFullPath(directoryPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar));
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
