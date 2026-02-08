using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace MoreMountains.Tools
{	
	[System.Serializable]
	public class ProgressEvent : UnityEvent<float>{}

	/// <summary>
	/// 用于存储叠加式场景加载设置的简单类
	/// </summary>
	[Serializable]
	public class MMAdditiveSceneLoadingManagerSettings
	{
		/// 卸载场景的可选方式
		public enum UnloadMethods { None, ActiveScene, AllScenes };
		/// 叠加模式下要使用的 MMSceneLoadingManager 场景名称
		[Tooltip("叠加模式下要使用的 MMSceneLoadingManager 场景名称")]
		public string LoadingSceneName = "MMAdditiveLoadingScreen";
		/// 叠加加载模式下，应用于加载的线程优先级
		[Tooltip("叠加加载模式下，应用于加载的线程优先级")]
		public ThreadPriority ThreadPriority = ThreadPriority.High;
		/// 是否进行额外的安全检查（建议保持为 true）
		[Tooltip("是否进行额外的安全检查（建议保持为 true）")]
		public bool SecureLoad = true;
		/// 叠加加载模式下，是否对进度条的进度进行插值平滑
		[Tooltip("叠加加载模式下，是否对进度条的进度进行插值平滑")]
		public bool InterpolateProgress = true;
		/// 叠加加载模式下，进入淡入淡出之前的延迟时长（秒）
		[Tooltip("叠加加载模式下，进入淡入淡出之前的延迟时长（秒）")]
		public float BeforeEntryFadeDelay = 0f;
		/// 叠加加载模式下，进入淡入淡出的持续时长（秒）
		[Tooltip("叠加加载模式下，进入淡入淡出的持续时长（秒）")]
		public float EntryFadeDuration = 0.25f;
		/// 叠加加载模式下，进入淡入淡出之后的延迟时长（秒）
		[Tooltip("叠加加载模式下，进入淡入淡出之后的延迟时长（秒）")]
		public float AfterEntryFadeDelay = 0.1f;
		/// 叠加加载模式下，退出淡入淡出之前的延迟时长（秒）
		[Tooltip("叠加加载模式下，退出淡入淡出之前的延迟时长（秒）")]
		public float BeforeExitFadeDelay = 0.25f;
		/// 叠加加载模式下，退出淡入淡出的持续时长（秒）
		[Tooltip("叠加加载模式下，退出淡入淡出的持续时长（秒）")]
		public float ExitFadeDuration = 0.2f;
		/// 叠加加载模式下，进入时使用的缓动曲线
		[Tooltip("叠加加载模式下，进入时使用的缓动曲线")]
		public MMTweenType EntryFadeTween = null;
		/// 叠加加载模式下，退出时使用的缓动曲线
		[Tooltip("叠加加载模式下，退出时使用的缓动曲线")]
		public MMTweenType ExitFadeTween = null;
		/// 叠加加载模式下，加载器进度条的移动速度
		[Tooltip("叠加加载模式下，加载器进度条的移动速度")]
		public float ProgressBarSpeed = 5f;
		/// 进度区间列表（值应在 0 到 1 之间）及其对应速度，使进度条的推进更加非线性
		[Tooltip("进度区间列表（值应在 0 到 1 之间）及其对应速度，使进度条的推进更加非线性")]
		public List<MMSceneLoadingSpeedInterval> SpeedIntervals;
		/// 叠加加载模式下，选择性叠加淡入淡出模式
		[Tooltip("叠加加载模式下，选择性叠加淡入淡出模式")]
		public MMAdditiveSceneLoadingManager.FadeModes FadeMode = MMAdditiveSceneLoadingManager.FadeModes.FadeInThenOut;
		/// 选择的场景卸载方式（不卸载、仅卸载活动场景、卸载所有已加载场景）
		[Tooltip("选择的场景卸载方式（不卸载、仅卸载活动场景、卸载所有已加载场景）")]
		public UnloadMethods UnloadMethod = UnloadMethods.AllScenes;
		/// 叠加加载时使用的防溢出场景名称。
		/// 如果留空，将自动创建该场景；你也可以指定任意场景用于此目的。
		/// 通常你会希望防溢出场景是一个空场景，但你可以自定义其光照设置等。
		[Tooltip("叠加加载时使用的防溢出场景名称。" +
		         "如果留空，将自动创建该场景；你也可以指定任意场景用于此目的。通常你会希望防溢出场景是一个空场景，但你可以自定义其光照设置等。")]
		public string AntiSpillSceneName = "";
	}

	/// <summary>
	/// 用于为特定进度区间定义不同插值速度的类
	/// </summary>
	[Serializable]
	public class MMSceneLoadingSpeedInterval
	{
		/// 进度区间（值在 0 到 1 之间）
		public MMInterval<float> Interval;
		/// 在该区间内进度条的移动速度
		public float Speed = 1f;
	}
	
	/// <summary>
	/// 使用加载画面来加载场景的类，替代默认的场景加载 API。
	/// 这是经典 LoadingSceneManager 的新版本（为保持一致性已重命名为 MMSceneLoadingManager）。
	/// </summary>
	public class MMAdditiveSceneLoadingManager : MMMonoBehaviour 
	{
		/// 淡入淡出的播放顺序（取决于你在加载画面中设置的淡入淡出方式）
		public enum FadeModes { FadeInThenOut, FadeOutThenIn }
		
		[MMInspectorGroup("Audio Listener", true, 3)]
		public AudioListener LoadingAudioListener;
		
		[MMInspectorGroup("Settings", true, 10)]
		/// 触发淡入淡出的 ID，必须与场景中 Fader 上的 ID 匹配
		[Tooltip("触发淡入淡出的 ID，必须与场景中 Fader 上的 ID 匹配")]
		public int FaderID = 500;
		/// 是否将调试信息输出到控制台
		[Tooltip("是否将调试信息输出到控制台")]
		public bool DebugMode = false;

		[MMInspectorGroup("Progress Events", true, 11)]
		/// 用于更新实时进度的事件
		[Tooltip("用于更新实时进度的事件")]
		public ProgressEvent SetRealtimeProgressValue;
		/// 用于更新插值平滑进度的事件
		[Tooltip("用于更新插值平滑进度的事件")]
		public ProgressEvent SetInterpolatedProgressValue;

		[MMInspectorGroup("StateNode Events", true, 12)]
		/// 加载开始时触发的事件
		[Tooltip("加载开始时触发的事件")]
		public UnityEvent OnLoadStarted;
		/// 进入淡入淡出之前的延迟开始时触发的事件
		[Tooltip("进入淡入淡出之前的延迟开始时触发的事件")]
		public UnityEvent OnBeforeEntryFade;
		/// 进入淡入淡出开始时触发的事件
		[Tooltip("进入淡入淡出开始时触发的事件")]
		public UnityEvent OnEntryFade;
		/// 进入淡入淡出之后的延迟开始时触发的事件
		[Tooltip("进入淡入淡出之后的延迟开始时触发的事件")]
		public UnityEvent OnAfterEntryFade;
		/// 原始场景被卸载时触发的事件
		[Tooltip("原始场景被卸载时触发的事件")]
		public UnityEvent OnUnloadOriginScene;
		/// 目标场景开始加载时触发的事件
		[Tooltip("目标场景开始加载时触发的事件")]
		public UnityEvent OnLoadDestinationScene;
		/// 目标场景加载完成时触发的事件
		[Tooltip("目标场景加载完成时触发的事件")]
		public UnityEvent OnLoadProgressComplete;
		/// 目标场景插值加载完成时触发的事件
		[Tooltip("目标场景插值加载完成时触发的事件")]
		public UnityEvent OnInterpolatedLoadProgressComplete;
		/// 退出淡入淡出之前的延迟开始时触发的事件
		[Tooltip("退出淡入淡出之前的延迟开始时触发的事件")]
		public UnityEvent OnBeforeExitFade;
		/// 退出淡入淡出开始时触发的事件
		[Tooltip("退出淡入淡出开始时触发的事件")]
		public UnityEvent OnExitFade;
		/// 目标场景被激活时触发的事件
		[Tooltip("目标场景被激活时触发的事件")]
		public UnityEvent OnDestinationSceneActivation;
		/// 场景加载器被卸载时触发的事件
		[Tooltip("场景加载器被卸载时触发的事件")]
		public UnityEvent OnUnloadSceneLoader;

		// ==================== 静态配置字段（跨实例共享） ====================
		protected static bool _interpolateProgress;                          // 是否启用进度插值
		protected static float _progressInterpolationSpeed;                  // 进度插值速度
		protected static List<MMSceneLoadingSpeedInterval> _speedIntervals;  // 速度区间列表
		protected static float _beforeEntryFadeDelay;                        // 进入淡入淡出前的延迟
		protected static MMTweenType _entryFadeTween;                        // 进入淡入淡出的缓动曲线
		protected static float _entryFadeDuration;                           // 进入淡入淡出的持续时长
		protected static float _afterEntryFadeDelay;                         // 进入淡入淡出后的延迟
		protected static float _beforeExitFadeDelay;                         // 退出淡入淡出前的延迟
		protected static MMTweenType _exitFadeTween;                         // 退出淡入淡出的缓动曲线
		protected static float _exitFadeDuration;                            // 退出淡入淡出的持续时长
		protected static FadeModes _fadeMode;                                // 淡入淡出模式
		protected static string _sceneToLoadName = "";                       // 要加载的目标场景名称
		protected static string _loadingScreenSceneName;                     // 加载画面场景名称
		protected static List<string> _scenesInBuild;                        // 构建设置中的场景列表
		protected static Scene[] _initialScenes;                             // 需要卸载的初始场景数组

		// ==================== 实例字段 ====================
		protected float _loadProgress = 0f;                                  // 实际加载进度（0~1）
		protected float _interpolatedLoadProgress;                           // 插值平滑后的加载进度
		protected static bool _loadingInProgress = false;                    // 是否正在加载中（防止重复加载）
		protected AsyncOperation _unloadOriginAsyncOperation;                // 卸载原始场景的异步操作
		protected AsyncOperation _loadDestinationAsyncOperation;             // 加载目标场景的异步操作
		protected AsyncOperation _unloadLoadingAsyncOperation;               // 卸载加载画面的异步操作
		protected bool _setRealtimeProgressValueIsNull;                      // 实时进度事件是否为空的缓存
		protected bool _setInterpolatedProgressValueIsNull;                  // 插值进度事件是否为空的缓存
		protected const float _asyncProgressLimit = 0.9f;                    // 异步加载进度上限（Unity 异步加载最大到 0.9）
		protected MMSceneLoadingAntiSpill _antiSpill = new MMSceneLoadingAntiSpill(); // 防溢出场景管理器
		protected static string _antiSpillSceneName = "";                    // 防溢出场景名称
		
		/// <summary>
		/// 静态字段初始化，用于支持 Unity 的 Enter Play Mode 设置（域重载）
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_loadingInProgress = false;
			_interpolateProgress = false;
			_progressInterpolationSpeed = 0f;
			_speedIntervals = new List<MMSceneLoadingSpeedInterval>();
			_beforeEntryFadeDelay = 0f;
			_entryFadeTween = null;
			_entryFadeDuration = 0f;
			_afterEntryFadeDelay = 0f;
			_beforeExitFadeDelay = 0f;
			_exitFadeTween = null;
			_exitFadeDuration = 0f;
			_sceneToLoadName = "";
			_loadingScreenSceneName = "";
			_scenesInBuild = new List<string>();
			_initialScenes = null;
			_antiSpillSceneName = "";
		}

		/// <summary>
		/// 从任意位置调用此静态方法来加载场景（使用打包的设置参数）
		/// </summary>
		/// <param name="sceneToLoadName">要加载的目标场景名称</param>
		/// <param name="settings">叠加加载设置</param>
		public static void LoadScene(string sceneToLoadName, MMAdditiveSceneLoadingManagerSettings settings)
		{
			LoadScene(sceneToLoadName, settings.LoadingSceneName, settings.ThreadPriority, settings.SecureLoad, settings.InterpolateProgress,
				settings.BeforeEntryFadeDelay, settings.EntryFadeDuration, settings.AfterEntryFadeDelay, settings.BeforeExitFadeDelay,
				settings.ExitFadeDuration, settings.EntryFadeTween, settings.ExitFadeTween, settings.ProgressBarSpeed, settings.FadeMode, settings.UnloadMethod, settings.AntiSpillSceneName,
				settings.SpeedIntervals);
		}
        
		/// <summary>
		/// 从任意位置调用此静态方法来加载场景（完整参数签名）。
		/// 该方法会验证场景是否存在于构建设置中，设置加载参数，然后以叠加模式加载加载画面场景。
		/// </summary>
		/// <param name="sceneToLoadName">要加载的目标场景名称</param>
		public static void LoadScene(string sceneToLoadName, string loadingSceneName = "MMAdditiveLoadingScreen", 
			ThreadPriority threadPriority = ThreadPriority.High, bool secureLoad = true,
			bool interpolateProgress = true,
			float beforeEntryFadeDelay = 0f,
			float entryFadeDuration = 0.25f,
			float afterEntryFadeDelay = 0.1f,
			float beforeExitFadeDelay = 0.25f,
			float exitFadeDuration = 0.2f, 
			MMTweenType entryFadeTween = null, MMTweenType exitFadeTween = null,
			float progressBarSpeed = 5f, 
			FadeModes fadeMode = FadeModes.FadeInThenOut,
			MMAdditiveSceneLoadingManagerSettings.UnloadMethods unloadMethod = MMAdditiveSceneLoadingManagerSettings.UnloadMethods.AllScenes,
			string antiSpillSceneName = "",
			List<MMSceneLoadingSpeedInterval> speedIntervals = null)
		{
			if (_loadingInProgress)
			{
				Debug.LogError("MMLoadingSceneManagerAdditive : 在场景加载进行中时收到了新的加载请求");  
				return;
			}

			if (entryFadeTween == null)
			{
				entryFadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
			}

			if (exitFadeTween == null)
			{
				exitFadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
			}

			if (secureLoad)
			{
				// 安全加载模式：检查目标场景和加载画面场景是否存在于构建设置中
				_scenesInBuild = MMScene.GetScenesInBuild();
	            
				if (!_scenesInBuild.Contains(sceneToLoadName))
				{
					Debug.LogError("MMLoadingSceneManagerAdditive : 无法加载场景 '"+sceneToLoadName+"'，" +
					               "该场景不存在于项目的构建设置中。");
					return;
				}
				if (!_scenesInBuild.Contains(loadingSceneName))
				{
					Debug.LogError("MMLoadingSceneManagerAdditive : 无法加载场景 '" + loadingSceneName + "'，" +
								   "该场景不存在于项目的构建设置中。");
					return;
				}
			}

			// 标记加载进行中，防止重复加载
			_loadingInProgress = true;
			// 获取需要卸载的场景列表
			_initialScenes = GetScenesToUnload(unloadMethod);

			// 设置后台加载线程优先级
			Application.backgroundLoadingPriority = threadPriority;
			// 缓存所有加载参数到静态字段
			_sceneToLoadName = sceneToLoadName;					
			_loadingScreenSceneName = loadingSceneName;
			_beforeEntryFadeDelay = beforeEntryFadeDelay;
			_entryFadeDuration = entryFadeDuration;
			_entryFadeTween = entryFadeTween;
			_afterEntryFadeDelay = afterEntryFadeDelay;
			_progressInterpolationSpeed = progressBarSpeed;
			_beforeExitFadeDelay = beforeExitFadeDelay;
			_exitFadeDuration = exitFadeDuration;
			_exitFadeTween = exitFadeTween;
			_fadeMode = fadeMode;
			_interpolateProgress = interpolateProgress;
			_antiSpillSceneName = antiSpillSceneName;
			_speedIntervals = speedIntervals;

			// 以叠加模式加载加载画面场景，加载画面场景中的 Awake 会启动完整的加载流程
			SceneManager.LoadScene(_loadingScreenSceneName, LoadSceneMode.Additive);
		}
        
		/// <summary>
		/// 根据卸载方式获取需要卸载的场景数组
		/// </summary>
		private static Scene[] GetScenesToUnload(MMAdditiveSceneLoadingManagerSettings.UnloadMethods unloaded)
		{
	        
			switch (unloaded) {
				case MMAdditiveSceneLoadingManagerSettings.UnloadMethods.None:
					_initialScenes = new Scene[0];
					break;
				case MMAdditiveSceneLoadingManagerSettings.UnloadMethods.ActiveScene:
					_initialScenes = new Scene[1] {SceneManager.GetActiveScene()};
					break;
				default:
				case MMAdditiveSceneLoadingManagerSettings.UnloadMethods.AllScenes:
					_initialScenes = MMScene.GetLoadedScenes();
					break;
			}
			return _initialScenes;
		}


		/// <summary>
		/// 开始异步加载新关卡
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// 初始化时间缩放、计算空值检查，并启动加载序列
		/// </summary>
		protected virtual void Initialization()
		{
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : Initialization");

			if (DebugMode)
			{
				foreach (Scene scene in _initialScenes)
				{
					MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : Initial scene : " + scene.name);
				}    
			}

			_setRealtimeProgressValueIsNull = SetRealtimeProgressValue == null;
			_setInterpolatedProgressValueIsNull = SetInterpolatedProgressValue == null;
			Time.timeScale = 1f;

			if ((_sceneToLoadName == "") || (_loadingScreenSceneName == ""))
			{
				return;
			}
            
			StartCoroutine(LoadSequence());
		}

		/// <summary>
		/// 每帧根据加载进度平滑填充进度条
		/// </summary>
		protected virtual void Update()
		{
			UpdateProgress();
		}

		/// <summary>
		/// 通过 UnityEvent 发送进度值（实时进度和插值平滑进度）
		/// </summary>
		protected virtual void UpdateProgress()
		{
			if (!_setRealtimeProgressValueIsNull)
			{
				SetRealtimeProgressValue.Invoke(_loadProgress);
			}

			if (_interpolateProgress)
			{
				_interpolatedLoadProgress = MMMaths.Approach(_interpolatedLoadProgress, _loadProgress, Time.unscaledDeltaTime * ComputeInterpolationSpeed(_interpolatedLoadProgress));
				if (!_setInterpolatedProgressValueIsNull)
				{
					SetInterpolatedProgressValue.Invoke(_interpolatedLoadProgress);	
				}
			}
			else
			{
				SetInterpolatedProgressValue.Invoke(_loadProgress);	
			}
		}

		/// <summary>
		/// 计算特定进度时间点应使用的插值速度。
		/// 如果当前进度落在某个速度区间内，则使用该区间的速度；否则使用默认插值速度。
		/// </summary>
		/// <param name="t">当前进度值（0~1）</param>
		/// <returns>对应的插值速度</returns>
		public static float ComputeInterpolationSpeed(float t) 
		{
			if ((_speedIntervals != null) && (_speedIntervals.Count > 0))
			{
				foreach (MMSceneLoadingSpeedInterval interval in _speedIntervals)
				{
					if (interval.Interval.Contains(t))
					{
						return interval.Speed;
					}
				}
			}

			return _progressInterpolationSpeed;
		}

		/// <summary>
		/// 异步加载场景的完整流程协程。
		/// 按顺序执行：防溢出准备 -> 初始化 -> 进入前延迟 -> 进入淡入淡出 -> 进入后延迟
		/// -> 卸载原始场景 -> 加载目标场景 -> 退出前延迟 -> 激活目标场景 -> 退出淡入淡出 -> 卸载加载器
		/// </summary>
		protected virtual IEnumerator LoadSequence()
		{
			_antiSpill?.PrepareAntiFill(_sceneToLoadName, _antiSpillSceneName);
			InitiateLoad();
			yield return ProcessDelayBeforeEntryFade();
			yield return EntryFade();
			yield return ProcessDelayAfterEntryFade();
			yield return UnloadOriginScenes();
			yield return LoadDestinationScene();
			yield return ProcessDelayBeforeExitFade();
			yield return DestinationSceneActivation();
			yield return ExitFade();
			yield return UnloadSceneLoader();
		}

		/// <summary>
		/// 初始化计数器和时间缩放，关闭音频监听器，触发加载开始事件
		/// </summary>
		protected virtual void InitiateLoad()
		{
			_loadProgress = 0f;
			_interpolatedLoadProgress = 0f;
			Time.timeScale = 1f;
			SetAudioListener(false);
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : Initiate Load");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.LoadStarted);
			OnLoadStarted?.Invoke();
		}

		/// <summary>
		/// 等待指定的 BeforeEntryFadeDelay 时长
		/// </summary>
		protected virtual IEnumerator ProcessDelayBeforeEntryFade()
		{
			if (_beforeEntryFadeDelay > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : delay before entry fade, duration : " + _beforeEntryFadeDelay);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.BeforeEntryFade);
				OnBeforeEntryFade?.Invoke();
				
				yield return MMCoroutine.WaitForUnscaled(_beforeEntryFadeDelay);
			}
		}

		/// <summary>
		/// 执行进入时的淡入淡出效果。
		/// 根据 FadeMode 决定是先淡入还是先淡出。
		/// </summary>
		protected virtual IEnumerator EntryFade()
		{
			if (_entryFadeDuration > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : entry fade, duration : " + _entryFadeDuration);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.EntryFade);
				OnEntryFade?.Invoke();
				
				if (_fadeMode == FadeModes.FadeOutThenIn)
				{
					yield return null;
					MMFadeOutEvent.Trigger(_entryFadeDuration, _entryFadeTween, FaderID, true);
				}
				else
				{
					yield return null;
					MMFadeInEvent.Trigger(_entryFadeDuration, _entryFadeTween, FaderID, true);
				}           

				yield return MMCoroutine.WaitForUnscaled(_entryFadeDuration);
			}
		}

		/// <summary>
		/// 等待指定的 AfterEntryFadeDelay 时长
		/// </summary>
		protected virtual IEnumerator ProcessDelayAfterEntryFade()
		{
			if (_afterEntryFadeDelay > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : delay after entry fade, duration : " + _afterEntryFadeDelay);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.AfterEntryFade);
				OnAfterEntryFade?.Invoke();
				
				yield return MMCoroutine.WaitForUnscaled(_afterEntryFadeDelay);
			}
		}

		/// <summary>
		/// 卸载原始场景并等待卸载完成。
		/// 遍历所有初始场景，逐个异步卸载，跳过无效或未加载的场景。
		/// </summary>
		protected virtual IEnumerator UnloadOriginScenes()
		{
			foreach (Scene scene in _initialScenes)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : unload scene " + scene.name);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.UnloadOriginScene);
				OnUnloadOriginScene?.Invoke();
				
				if (!scene.IsValid() || !scene.isLoaded)
				{
					Debug.LogWarning("MMLoadingSceneManagerAdditive : 无效的场景 : " + scene.name);
					continue;
				}
				
				_unloadOriginAsyncOperation = SceneManager.UnloadSceneAsync(scene);
				SetAudioListener(true);
				while (_unloadOriginAsyncOperation.progress < _asyncProgressLimit)
				{
					yield return null;
				}
			}
		}

		/// <summary>
		/// 异步加载目标场景。
		/// 先禁止场景自动激活，等待加载进度到达上限后设置进度为100%，
		/// 然后等待插值进度条视觉上填满后继续。
		/// </summary>
		protected virtual IEnumerator LoadDestinationScene()
		{
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : load destination scene");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.LoadDestinationScene);
			OnLoadDestinationScene?.Invoke();

			_loadDestinationAsyncOperation = SceneManager.LoadSceneAsync(_sceneToLoadName, LoadSceneMode.Additive );
			_loadDestinationAsyncOperation.completed += OnLoadOperationComplete;

			_loadDestinationAsyncOperation.allowSceneActivation = false;
            
			while (_loadDestinationAsyncOperation.progress < _asyncProgressLimit)
			{
				_loadProgress = _loadDestinationAsyncOperation.progress;
				yield return null;
			}
            
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : load progress complete");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.LoadProgressComplete);
			OnLoadProgressComplete?.Invoke();

			// 当加载接近完成时（Unity 异步加载永远不会到达 1.0），将进度设为 100%
			_loadProgress = 1f;

			// 等待进度条在视觉上完全填满后再继续
			if (_interpolateProgress)
			{
				while (_interpolatedLoadProgress < 1f)
				{
					yield return null;
				}
			}			

			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : interpolated load complete");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.InterpolatedLoadProgressComplete);
			OnInterpolatedLoadProgressComplete?.Invoke();
		}

		/// <summary>
		/// 等待 BeforeExitFadeDelay 秒
		/// </summary>
		protected virtual IEnumerator ProcessDelayBeforeExitFade()
		{
			if (_beforeExitFadeDelay > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : delay before exit fade, duration : " + _beforeExitFadeDelay);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.BeforeExitFade);
				OnBeforeExitFade?.Invoke();
				
				yield return MMCoroutine.WaitForUnscaled(_beforeExitFadeDelay);
			}
		}

		/// <summary>
		/// 执行退出时的淡入淡出效果。
		/// 根据 FadeMode 决定是淡入还是淡出。
		/// </summary>
		protected virtual IEnumerator ExitFade()
		{
			SetAudioListener(false);
			if (_exitFadeDuration > 0f)
			{
				MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : exit fade, duration : " + _exitFadeDuration);
				MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.ExitFade);
				OnExitFade?.Invoke();
				
				if (_fadeMode == FadeModes.FadeOutThenIn)
				{
					MMFadeInEvent.Trigger(_exitFadeDuration, _exitFadeTween, FaderID, true);
				}
				else
				{
					MMFadeOutEvent.Trigger(_exitFadeDuration, _exitFadeTween, FaderID, true);
				}
				yield return MMCoroutine.WaitForUnscaled(_exitFadeDuration);
			}
		}

		/// <summary>
		/// 激活目标场景。
		/// 允许场景激活后等待加载进度到达 1.0，然后触发激活事件。
		/// </summary>
		protected virtual IEnumerator DestinationSceneActivation()
		{
			yield return MMCoroutine.WaitForFrames(1);
			_loadDestinationAsyncOperation.allowSceneActivation = true;
			while (_loadDestinationAsyncOperation.progress < 1.0f)
			{
				yield return null;
			}
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : activating destination scene");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.DestinationSceneActivation);
			OnDestinationSceneActivation?.Invoke();
		}

		/// <summary>
		/// 异步操作完成时的回调方法，将目标场景设为活动场景
		/// </summary>
		/// <param name="obj">已完成的异步操作</param>
		protected virtual void OnLoadOperationComplete(AsyncOperation obj)
		{
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneToLoadName));
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : set active scene to " + _sceneToLoadName);

		}

		/// <summary>
		/// 卸载加载画面场景
		/// </summary>
		protected virtual IEnumerator UnloadSceneLoader()
		{
			MMLoadingSceneDebug("MMLoadingSceneManagerAdditive : unloading scene loader");
			MMSceneLoadingManager.LoadingSceneEvent.Trigger(_sceneToLoadName, MMSceneLoadingManager.LoadingStatus.UnloadSceneLoader);
			OnUnloadSceneLoader?.Invoke();
			
			yield return null; // 必须的 yield，避免产生不合理的警告
			_unloadLoadingAsyncOperation = SceneManager.UnloadSceneAsync(_loadingScreenSceneName);
			while (_unloadLoadingAsyncOperation.progress < _asyncProgressLimit)
			{
				yield return null;
			}
		}

		/// <summary>
		/// 开启或关闭加载画面的音频监听器
		/// </summary>
		/// <param name="state">true 为开启，false 为关闭</param>
		protected virtual void SetAudioListener(bool state)
		{
			if (LoadingAudioListener != null)
			{
				//LoadingAudioListener.gameObject.SetActive(state);
			}
		}

		/// <summary>
		/// 销毁时重置加载状态标志
		/// </summary>
		protected virtual void OnDestroy()
		{
			_loadingInProgress = false;
		}

		/// <summary>
		/// 调试方法，仅在 DebugMode 开启时向控制台输出带有帧号和时间戳的调试信息
		/// </summary>
		/// <param name="message">要输出的调试信息</param>
		protected virtual void MMLoadingSceneDebug(string message)
		{
			if (!DebugMode)
			{
				return;
			}
			
			string output = "";
			output += "<color=#82d3f9>[" + Time.frameCount + "]</color> ";
			output += "<color=#f9a682>[" + MMTime.FloatToTimeString(Time.time, false, true, true, true) + "]</color> ";
			output +=  message;
			MMDebug.DebugLogInfo(output);
		}
	}
}