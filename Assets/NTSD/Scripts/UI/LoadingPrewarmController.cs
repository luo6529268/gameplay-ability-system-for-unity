using Cysharp.Threading.Tasks;
using UnityEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using TMPro;
using System.Collections.Generic;
using NTSD.Load;
using NTSD.Simulation;

namespace NTSD.UI
{
    public sealed class LoadingPrewarmController : MonoBehaviour
    {
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private float minTextDisplaySeconds = 0.15f;

        public TextMeshProUGUI LoadingResourceTxt;

        private bool prewarmCompleted;
        private bool prewarmDisposed;
        private bool prewarmRunning;
        private int prewarmGeneration;
        private string prewarmedRoot;
        private string prewarmedKey;
        private CharacterAnimtorManager prewarmedManager;
        private UniTask prewarmTask;
        public bool IsPrewarmed => prewarmCompleted && prewarmedManager != null &&
            ReferenceEquals(CharacterAnimtorManager.TryGetInstance(), prewarmedManager) &&
            prewarmedManager.IsPublishedContentUsable && prewarmedManager.PublishedVisualContentKey == prewarmedKey &&
            CharacterAnimtorManager.ConfiguredContentRoot == prewarmedRoot;

        private readonly Queue<string> pendingTexts = new Queue<string>();
        private string currentText;
        private float nextTextUpdateTime;
        private NTSD_ResourceLoader resourceLoader;

        private async void Start()
        {
            if (!runOnStart) return;

            try
            {
                WarmupTextureAndSprite();
                await PrewarmOnceAsync();
                await UniTask.WaitUntil(() => IsPrewarmed && pendingTexts.Count == 0);

                if (MenuUIController.Instance != null)
                {
                    MenuUIController.Instance.ShowSelectGameMode();
                }
                else
                {
                    Debug.LogError("[LoadingPrewarmController] MenuUIController.Instance is null after prewarm completed.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LoadingPrewarmController] Start failed with exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void WarmupTextureAndSprite()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels(new[] { Color.clear, Color.clear, Color.clear, Color.clear });
            texture.Apply();
            Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
        }

        private void Update()
        {
            if (LoadingResourceTxt == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(currentText))
            {
                if (pendingTexts.Count == 0)
                {
                    return;
                }

                currentText = pendingTexts.Dequeue();
                LoadingResourceTxt.text = currentText;
                nextTextUpdateTime = Time.unscaledTime + minTextDisplaySeconds;
                return;
            }

            if (Time.unscaledTime < nextTextUpdateTime)
            {
                return;
            }

            if (pendingTexts.Count == 0)
            {
                nextTextUpdateTime = Time.unscaledTime + minTextDisplaySeconds;
                return;
            }

            currentText = pendingTexts.Dequeue();
            LoadingResourceTxt.text = currentText;
            nextTextUpdateTime = Time.unscaledTime + minTextDisplaySeconds;
        }

        public UniTask PrewarmOnceAsync()
        {
            if (prewarmDisposed) throw new System.OperationCanceledException();
            if (prewarmRunning) return prewarmTask;
            prewarmRunning = true;
            int generation = ++prewarmGeneration;
            prewarmTask = PrewarmOnceCoreAsync(generation).Preserve();
            return prewarmTask;
        }

        private async UniTask PrewarmOnceCoreAsync(int generation)
        {
            try
            {
                var mgr = CharacterAnimtorManager.Instance;
                if (mgr == null) throw new System.InvalidOperationException("Character prewarm owner is unavailable.");
                string root = CharacterAnimtorManager.ConfiguredContentRoot;
                bool IsCurrent() => this != null && !prewarmDisposed && generation == prewarmGeneration &&
                    mgr != null && root == CharacterAnimtorManager.ConfiguredContentRoot;
                System.Action<string> progress = text => { if (IsCurrent()) OnPrewarmLoadingResourceChanged(text); };
                prewarmCompleted = false;
                resourceLoader ??= NTSD_ResourceLoader.Instance;
                string key;
                if (root.Length != 0)
                {
                    key = await mgr.PrewarmConfiguredLoganContentAsync(progress);
                }
                else
                {
                    if (mgr.PublishedVisualContentKey != null)
                        throw new System.InvalidOperationException("Legacy prewarm cannot reuse a Logan publication.");
                    if (!mgr.IsPrewarmCompleted)
                    {
                        var configTask = CreateCharacterConfigTask(mgr, FormatLoadingResourcePath, progress);
                        var spriteTask = CreateCharacterSpriteTask(mgr, FormatLoadingResourcePath, progress);
                        resourceLoader.AddTask(configTask);
                        await PumpPrewarmTasksAsync();
                        RequireCompleted(configTask);
                        if (!IsCurrent()) throw new System.OperationCanceledException();
                        resourceLoader.AddTask(spriteTask);
                        await PumpPrewarmTasksAsync();
                        RequireCompleted(spriteTask);
                    }
                    key = await mgr.ValidateConfiguredContentForBattleAsync();
                }
                if (!IsCurrent()) throw new System.OperationCanceledException();
                int contentGeneration = mgr.ConfiguredContentGeneration;
                bool CanWarmPool() => IsCurrent() && contentGeneration == mgr.ConfiguredContentGeneration;
                var poolTask = CreatePoolPrewarmTask(progress, CanWarmPool);
                resourceLoader.AddTask(poolTask);
                await PumpPrewarmTasksAsync();
                RequireCompleted(poolTask);
                await mgr.AssertConfiguredContentUnchangedAsync(key);
                if (!CanWarmPool()) throw new System.OperationCanceledException();
                prewarmedManager = mgr;
                prewarmedRoot = root;
                prewarmedKey = key;
                prewarmCompleted = true;
            }
            finally
            {
                if (generation == prewarmGeneration) prewarmRunning = false;
            }
        }

        private static void RequireCompleted(NTSD_LoadTask task)
        {
            if (task.Status != NTSD_LoadTaskStatus.Completed)
                throw new System.InvalidOperationException("Battle prewarm task did not complete: " + task.Name + "/" + task.Status);
        }

        private async UniTask PumpPrewarmTasksAsync()
        {
            while (!resourceLoader.IsIdle())
            {
                await resourceLoader.ProcessFrame();
                await UniTask.Yield();
            }
        }

        private void OnDestroy()
        {
            prewarmDisposed = true;
            prewarmCompleted = false;
            prewarmGeneration++;
        }

        private void OnPrewarmLoadingResourceChanged(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return;
            }

            pendingTexts.Enqueue(resourcePath);
        }

        private string FormatLoadingResourcePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return string.Empty;
            }

            var normalized = filePath.Replace("\\", "/");
            const string marker = "/Sprite/Character/";
            var index = normalized.IndexOf(marker, System.StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return normalized.Substring(index + marker.Length);
            }

            return System.IO.Path.GetFileName(normalized);
        }

        private NTSD_LoadTask CreatePoolPrewarmTask(System.Action<string> onProgressText, System.Func<bool> canContinue = null)
        {
            return new NTSD_LoadTask
            {
                Name = "PoolPrewarm",
                Type = NTSD_LoadTaskType.Warmup,
                Domain = NTSD_ResourceDomain.Other,
                Priority = 80,
                Execute = async (task, _) =>
                {
                    if (!(canContinue?.Invoke() ?? true)) throw new System.OperationCanceledException();
                    onProgressText?.Invoke("Prewarming Entity Slots...");
                    BattleRuntimeWorldSettings runtimeSettings =
                        BattleRuntimeProfileProductionSource.Resolve(GameConfig.Instance);
                    int reservedCapacity = runtimeSettings.InitialRuntimeSlotCapacity;
                    LF2ReferencePool.Instance.PrewarmTasks<OPointCreateTask>(reservedCapacity);
                    LF2ReferencePool.Instance.PrewarmTasks<OPointCreateMultipleTask>(reservedCapacity);
                    // 对齐反汇编 SceneManager_Init: 预分配 400 个实体逻辑对象
                    LF2ReferencePool.Instance.PrepareObjectCapacity(
                        LF2ObjectType.Character,
                        reservedCapacity);
                    // 同时异步预分配 400 个实体 GameObject 实例
                    LF2ObjectPointFactory.Instance?.PrepareTaskQueueCapacity(reservedCapacity);
                    if (!await LF2ObjectPool.Instance.PrepareCapacityForContentAsync(
                        reservedCapacity,
                        reservedCapacity, canContinue)) throw new System.OperationCanceledException();
                    task.Result = true;
                }
            };
        }

        private NTSD_LoadTask CreateCharacterConfigTask(CharacterAnimtorManager manager,
            System.Func<string, string> progressTextFormatter, System.Action<string> onProgressText)
        {
            return new NTSD_LoadTask
            {
                Name = "CharacterConfig",
                Type = NTSD_LoadTaskType.LoadConfig,
                Domain = NTSD_ResourceDomain.Character,
                Priority = 100,
                CacheKey = "NTSD.CharacterConfig",
                OnCompleted = task =>
                {
                    if (manager == null || CharacterAnimtorManager.HasConfiguredLoganContent)
                        throw new System.OperationCanceledException();
                    manager.ApplyLoadedCharacterConfigs((Dictionary<int, LF2CharacterDataWrapper>)task.Result);
                },
                Execute = async (task, _) =>
                {
                    var dataManager = GameDataManager.Instance;
                    var configs = await UniTask.RunOnThreadPool(() => manager.ParseCharacterFrameConfigs(dataManager, text =>
                    {
                        var formatted = progressTextFormatter != null ? progressTextFormatter(text) : text;
                        onProgressText?.Invoke(formatted);
                    }));
                    task.Result = configs;
                }
            };
        }

        private NTSD_LoadTask CreateCharacterSpriteTask(CharacterAnimtorManager manager,
            System.Func<string, string> progressTextFormatter, System.Action<string> onProgressText)
        {
            return new NTSD_LoadTask
            {
                Name = "CharacterSprites",
                Type = NTSD_LoadTaskType.LoadSprites,
                Domain = NTSD_ResourceDomain.Character,
                Priority = 90,
                Execute = async (task, _) =>
                {
                    if (manager == null || CharacterAnimtorManager.HasConfiguredLoganContent)
                        throw new System.OperationCanceledException();
                    await manager.LoadCharacterSpritesAsync(text =>
                    {
                        var formatted = progressTextFormatter != null ? progressTextFormatter(text) : text;
                        onProgressText?.Invoke(formatted);
                    });
                    task.Result = true;
                }
            };
        }
    }
}
