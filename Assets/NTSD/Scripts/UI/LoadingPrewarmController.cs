using Cysharp.Threading.Tasks;
using UnityEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using TMPro;
using System.Collections.Generic;
using NTSD.Load;

namespace NTSD.UI
{
    public sealed class LoadingPrewarmController : MonoBehaviour
    {
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private float minTextDisplaySeconds = 0.15f;

        public TextMeshProUGUI LoadingResourceTxt;

        public bool IsPrewarmed { get; private set; }

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

        public async UniTask PrewarmOnceAsync()
        {
            if (IsPrewarmed) return;

            const int maxWaitFrames = 300;
            int waitedFrames = 0;
            var mgr = CharacterAnimtorManager.Instance;
            while (mgr == null && waitedFrames < maxWaitFrames)
            {
                await UniTask.Yield();
                waitedFrames++;
                mgr = CharacterAnimtorManager.Instance;
            }

            if (mgr == null)
            {
                Debug.LogError("[LoadingPrewarmController] CharacterAnimtorManager.Instance is null after timeout. Aborting prewarm.");
                return;
            }

            if (resourceLoader == null)
            {
                resourceLoader = NTSD_ResourceLoader.Instance;
            }

            if (resourceLoader == null)
            {
                Debug.LogError("[LoadingPrewarmController] NTSD_ResourceLoader.Instance is null. Aborting prewarm.");
                return;
            }

            var configTask = CreateCharacterConfigTask(mgr, FormatLoadingResourcePath, OnPrewarmLoadingResourceChanged);
            var spriteTask = CreateCharacterSpriteTask(mgr, FormatLoadingResourcePath, OnPrewarmLoadingResourceChanged);
            var poolTask = CreatePoolPrewarmTask(OnPrewarmLoadingResourceChanged);

            poolTask.OnCompleted += _ =>
            {
                IsPrewarmed = true;
            };

            resourceLoader.AddTask(configTask);
            resourceLoader.AddTask(spriteTask);
            resourceLoader.AddTask(poolTask);

            while (!resourceLoader.IsIdle())
            {
                await resourceLoader.ProcessFrame();
                await UniTask.Yield();
            }
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

        private NTSD_LoadTask CreatePoolPrewarmTask(System.Action<string> onProgressText)
        {
            return new NTSD_LoadTask
            {
                Name = "PoolPrewarm",
                Type = NTSD_LoadTaskType.Warmup,
                Domain = NTSD_ResourceDomain.Other,
                Priority = 80,
                Execute = async (task, _) =>
                {
                    onProgressText?.Invoke("Prewarming Entity Slots...");
                    // 对齐反汇编 SceneManager_Init: 预分配 400 个实体逻辑对象
                    LF2ReferencePool.Instance.Prewarm(LF2ObjectType.Character, 400);
                    // 同时异步预分配 400 个实体 GameObject 实例
                    await LF2ObjectPool.Instance.PrewarmAsync(400);
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
                Execute = async (task, _) =>
                {
                    var dataManager = GameDataManager.Instance;
                    var configs = await UniTask.RunOnThreadPool(() => manager.ParseCharacterFrameConfigs(dataManager, text =>
                    {
                        var formatted = progressTextFormatter != null ? progressTextFormatter(text) : text;
                        onProgressText?.Invoke(formatted);
                    }));
                    task.Result = configs;
                    manager.ApplyLoadedCharacterConfigs(configs);
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
                CacheKey = "NTSD.CharacterSprites",
                Execute = async (task, _) =>
                {
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
