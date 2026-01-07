using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
#if MM_UI
using UnityEngine.UI;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this component to an object and it will show a healthbar above it
    /// You can either use a prefab for it, or have the component draw one at the start
    /// 将此组件添加到对象上，它将在其上方显示一个健康条
    /// 你可以使用预制体，或者让组件在开始时自动绘制一个
    /// </summary>
    [AddComponentMenu("ThirdParty/More Mountains/Tools/GUI/MM Health Bar")]
	public class MMHealthBar : MonoBehaviour 
	{
#if MM_UI
        /// the possible health bar types <summary>
        /// Prefab：预制体Drawn：绘制的Existing：现有的
        /// </summary>
        public enum HealthBarTypes { Prefab, Drawn, Existing }
        /// the possible timescales the bar can work on <summary>
        /// UnscaledTime：非缩放时间Time：时间
        /// </summary>
        public enum TimeScales { UnscaledTime, Time }

        [MMInformation("将此组件添加到对象上，它将在其旁边添加一个健康条以实时反映其健康水平。你可以在这里决定健康条是否应该自动绘制或使用预制体。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 是否使用预制体或自动绘制健康条
        [Tooltip("是否使用预制体或自动绘制健康条")]
        public HealthBarTypes HealthBarType = HealthBarTypes.Drawn;
        /// 定义条将在缩放或非缩放时间上工作（例如，如果时间减慢，它是否继续移动）
        [Tooltip("定义条将在缩放或非缩放时间上工作（例如，如果时间减慢，它是否继续移动）")]
        public TimeScales TimeScale = TimeScales.UnscaledTime;

		[Header("Select a Prefab")]
        [MMInformation("选择一个带有进度条脚本的预制体。在Common/Prefabs/GUI中有一个这样的预制体示例。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 用作健康条的预制体
        [Tooltip("用作健康条的预制体")]
        public MMProgressBar HealthBarPrefab;

		[Header("Existing MMProgressBar")]
        /// 此健康条应该更新的MMProgressBar
        [Tooltip("此健康条应该更新的MMProgressBar")]
        public MMProgressBar TargetProgressBar;

		[Header("Drawn Healthbar Settings ")]
        [MMInformation("设置健康条的大小（世界单位）、填充、背景和前景色。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 如果绘制健康条，其大小（世界单位）
        [Tooltip("如果绘制健康条，其大小（世界单位）")]
        public Vector2 Size = new Vector2(1f,0.2f);
        /// 如果绘制健康条，应用于前景的填充（世界单位）
        [Tooltip("如果绘制健康条，应用于前景的填充（世界单位）")]
        public Vector2 BackgroundPadding = new Vector2(0.01f,0.01f);
        /// 绘制时应用于MMHealthBarContainer的旋转
        [Tooltip("绘制时应用于MMHealthBarContainer的旋转")]
        public Vector3 InitialRotationAngles;
        /// 如果绘制健康条，其前景颜色
        [Tooltip("如果绘制健康条，其前景颜色")]
        public Gradient ForegroundColor = new Gradient()
		{
			colorKeys = new GradientColorKey[2] {
				new GradientColorKey(MMColors.BestRed, 0),
				new GradientColorKey(MMColors.BestRed, 1f)
			},
			alphaKeys = new GradientAlphaKey[2] {new GradientAlphaKey(1, 0),new GradientAlphaKey(1, 1)}};
        /// 如果绘制健康条，其延迟条颜色
        [Tooltip("如果绘制健康条，其延迟条颜色")]
        public Gradient DelayedColor = new Gradient()
		{
			colorKeys = new GradientColorKey[2] {
				new GradientColorKey(MMColors.Orange, 0),
				new GradientColorKey(MMColors.Orange, 1f)
			},
			alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
		};
        /// 如果绘制健康条，其边框颜色
        [Tooltip("如果绘制健康条，其边框颜色")]
        public Gradient BorderColor = new Gradient()
		{
			colorKeys = new GradientColorKey[2] {
				new GradientColorKey(MMColors.AntiqueWhite, 0),
				new GradientColorKey(MMColors.AntiqueWhite, 1f)
			},
			alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
		};
        /// 如果绘制健康条，其背景颜色
        [Tooltip("如果绘制健康条，其背景颜色")]
        public Gradient BackgroundColor = new Gradient()
		{
			colorKeys = new GradientColorKey[2] {
				new GradientColorKey(MMColors.Black, 0),
				new GradientColorKey(MMColors.Black, 1f)
			},
			alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
		};
        /// 放置此健康条的排序层名称
        [Tooltip("放置此健康条的排序层名称")]
        public string SortingLayerName = "UI";
        /// 绘制时应用于延迟条的延迟
        [Tooltip("绘制时应用于延迟条的延迟")]
        public float Delay = 0.5f;
        /// 是否前条应该插值
        [Tooltip("是否前条应该插值")]
        public bool LerpFrontBar = true;
        /// 前条插值的速度
        [Tooltip("前条插值的速度")]
        public float LerpFrontBarSpeed = 15f;
        /// 是否延迟条应该插值
        [Tooltip("是否延迟条应该插值")]
        public bool LerpDelayedBar = true;
        /// 延迟条插值的速度
        [Tooltip("延迟条插值的速度")]
        public float LerpDelayedBarSpeed = 15f;
        /// 如果为真，当其值变化时增加健康条的缩放
        [Tooltip("如果为真，当其值变化时增加健康条的缩放")]
        public bool BumpScaleOnChange = true;
        /// 增加动画的持续时间
        [Tooltip("增加动画的持续时间")]
        public float BumpDuration = 0.2f;
        /// 应用于增加动画的动画曲线
        [Tooltip("应用于增加动画的动画曲线")]
        public AnimationCurve BumpAnimationCurve = AnimationCurve.Constant(0,1,1);


        /// 条应该跟随目标的模式
        [Tooltip("条应该跟随目标的模式")]
        public MMFollowTarget.UpdateModes FollowTargetMode = MMFollowTarget.UpdateModes.LateUpdate;
        /// 如果为真，绘制的健康条将调整其旋转以匹配其目标的旋转
        [Tooltip("如果为真，绘制的健康条将调整其旋转以匹配其目标的旋转")]
        public bool FollowRotation = false;
        /// 如果为真，绘制的健康条将调整其缩放以匹配其目标的缩放
        [Tooltip("如果为真，绘制的健康条将调整其缩放以匹配其目标的缩放")]
        public bool FollowScale = true;
        /// 如果为真，绘制的健康条将嵌套在MMHealthBar下方
        [Tooltip("如果为真，绘制的健康条将嵌套在MMHealthBar下方")]
        public bool NestDrawnHealthBar = false;
        /// 如果为真，将向进度条添加一个MMBillboard组件，以确保它始终朝向相机
        [Tooltip("如果为真，将向进度条添加一个MMBillboard组件，以确保它始终朝向相机")]
        public bool Billboard = false;

        [Header("Death")]
        /// 当健康条达到零时实例化的GameObject（通常是粒子系统）
        [Tooltip("当健康条达到零时实例化的GameObject（通常是粒子系统）")]
        public GameObject InstantiatedOnDeath;

		[Header("Offset")]
        [MMInformation("设置相对于对象中心的世界单位偏移量，健康条将在此偏移量处显示。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 相对于对象中心应用的健康条偏移量
        [Tooltip("相对于对象中心应用的健康条偏移量")]
        public Vector3 HealthBarOffset = new Vector3(0f,1f,0f);

		[Header("Display")]
        [MMInformation("在这里，你可以定义健康条是否应该始终可见。如果不是，你可以在这里设置在受到攻击后它将保持可见多长时间。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 是否应该永久显示该条
        [Tooltip("是否应该永久显示该条")]
        public bool AlwaysVisible = true;
        /// 显示条的持续时间（秒）
        [Tooltip("显示条的持续时间（秒）")]
        public float DisplayDurationOnHit = 1f;
        /// 如果设置为真，当条达到零时将自动隐藏
        [Tooltip("如果设置为真，当条达到零时将自动隐藏")]
        public bool HideBarAtZero = true;
        /// 隐藏条之前的延迟时间（秒）
        [Tooltip("隐藏条之前的延迟时间（秒）")]
        public float HideBarAtZeroDelay = 1f;


        [Header("Test")]
        /// 按下TestUpdateHealth按钮时使用的测试值
        [Tooltip("按下TestUpdateHealth按钮时使用的测试值")]
        public float TestMinHealth = 0f;
        /// 按下TestUpdateHealth按钮时使用的测试值
        [Tooltip("按下TestUpdateHealth按钮时使用的测试值")]
        public float TestMaxHealth = 100f;
        /// 按下TestUpdateHealth按钮时使用的测试值
        [Tooltip("按下TestUpdateHealth按钮时使用的测试值")]
        public float TestCurrentHealth = 25f;
        [MMInspectorButton("TestUpdateHealth")]
        public bool TestUpdateHealthButton;


        protected MMProgressBar _progressBar;
		protected MMFollowTarget _followTransform;
		protected float _lastShowTimestamp = 0f;
		protected bool _showBar = false;
		protected Image _backgroundImage = null;
		protected Image _borderImage = null;
		protected Image _foregroundImage = null;
		protected Image _delayedImage = null;
		protected bool _finalHideStarted = false;

		/// <summary>
		/// On Start, creates or sets the health bar up
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// On enable, initializes the bar again
		/// </summary>
		protected void OnEnable()
		{
			_finalHideStarted = false;

			SetInitialActiveState();
		}

		/// <summary>
		/// Forces the bar into its initial active state (hiding it if AlwaysVisible is false)
		/// </summary>
		public virtual void SetInitialActiveState()
		{
			if (!AlwaysVisible && (_progressBar != null))
			{
				ShowBar(false);
			}
		}

		/// <summary>
		/// Shows or hides the bar by changing its object's active state
		/// </summary>
		/// <param name="state"></param>
		public virtual void ShowBar(bool state)
		{
			_progressBar.gameObject.SetActive(state);
		}

		/// <summary>
		/// Whether or not the bar is currently active
		/// </summary>
		/// <returns></returns>
		public virtual bool BarIsShown()
		{
			return _progressBar.gameObject.activeInHierarchy;
		}

		/// <summary>
		/// Initializes the bar (handles visibility, parenting, initial value
		/// </summary>
		public virtual void Initialization()
		{
			_finalHideStarted = false;

			if (_progressBar != null)
			{
				ShowBar(AlwaysVisible);
				return;
			}

			switch (HealthBarType)
			{
				case HealthBarTypes.Prefab:
					if (HealthBarPrefab == null)
					{
						Debug.LogWarning(this.name + " : the HealthBar has no prefab associated to it, nothing will be displayed.");
						return;
					}
					_progressBar = Instantiate(HealthBarPrefab, transform.position + HealthBarOffset, transform.rotation) as MMProgressBar;
					SceneManager.MoveGameObjectToScene(_progressBar.gameObject, this.gameObject.scene);
					_progressBar.transform.SetParent(this.transform);
					_progressBar.gameObject.name = "HealthBar";
					break;
				case HealthBarTypes.Drawn:
					DrawHealthBar();
					UpdateDrawnColors();
					break;
				case HealthBarTypes.Existing:
					_progressBar = TargetProgressBar;
					break;
			}

			if (!AlwaysVisible)
			{
				ShowBar(false);
			}

			if (_progressBar != null)
			{
				_progressBar.SetBar(100f, 0f, 100f);
			}
		}
		

		/// <summary>
		/// Draws the health bar.
		/// </summary>
		protected virtual void DrawHealthBar()
		{
			GameObject newGameObject = new GameObject();
			SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);
			newGameObject.name = "HealthBar|"+this.gameObject.name;
			newGameObject.layer = LayerMask.NameToLayer("UI");

            if (NestDrawnHealthBar)
			{
				newGameObject.transform.SetParent(this.transform);
			}

			_progressBar = newGameObject.AddComponent<MMProgressBar>();

			_followTransform = newGameObject.AddComponent<MMFollowTarget>();
			_followTransform.Offset = HealthBarOffset;
			_followTransform.Target = this.transform;
			_followTransform.FollowRotation = FollowRotation;
			_followTransform.FollowScale = FollowScale; 
			_followTransform.InterpolatePosition = false;
			_followTransform.InterpolateRotation = false;
			_followTransform.UpdateMode = FollowTargetMode;

			Canvas newCanvas = newGameObject.AddComponent<Canvas>();
			newCanvas.renderMode = RenderMode.WorldSpace;
			newCanvas.transform.localScale = Vector3.one;
			newCanvas.GetComponent<RectTransform>().sizeDelta = Size;
			if (!string.IsNullOrEmpty(SortingLayerName))
			{
				newCanvas.sortingLayerName = SortingLayerName;
			}

			GameObject container = new GameObject();
			container.transform.SetParent(newGameObject.transform);
			container.name = "MMProgressBarContainer";
			container.transform.localScale = Vector3.one;
            
			GameObject borderImageGameObject = new GameObject();
			borderImageGameObject.transform.SetParent(container.transform);
			borderImageGameObject.name = "HealthBar Border";
			_borderImage = borderImageGameObject.AddComponent<Image>();
			_borderImage.transform.position = Vector3.zero;
			_borderImage.transform.localScale = Vector3.one;
			_borderImage.GetComponent<RectTransform>().sizeDelta = Size;
			_borderImage.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

			GameObject bgImageGameObject = new GameObject();
			bgImageGameObject.transform.SetParent(container.transform);
			bgImageGameObject.name = "HealthBar Background";
			_backgroundImage = bgImageGameObject.AddComponent<Image>();
			_backgroundImage.transform.position = Vector3.zero;
			_backgroundImage.transform.localScale = Vector3.one;
			_backgroundImage.GetComponent<RectTransform>().sizeDelta = Size - BackgroundPadding*2;
			_backgroundImage.GetComponent<RectTransform>().anchoredPosition = -_backgroundImage.GetComponent<RectTransform>().sizeDelta/2;
			_backgroundImage.GetComponent<RectTransform>().pivot = Vector2.zero;

			GameObject delayedImageGameObject = new GameObject();
			delayedImageGameObject.transform.SetParent(container.transform);
			delayedImageGameObject.name = "HealthBar Delayed Foreground";
			_delayedImage = delayedImageGameObject.AddComponent<Image>();
			_delayedImage.transform.position = Vector3.zero;
			_delayedImage.transform.localScale = Vector3.one;
			_delayedImage.GetComponent<RectTransform>().sizeDelta = Size - BackgroundPadding*2;
			_delayedImage.GetComponent<RectTransform>().anchoredPosition = -_delayedImage.GetComponent<RectTransform>().sizeDelta/2;
			_delayedImage.GetComponent<RectTransform>().pivot = Vector2.zero;

			GameObject frontImageGameObject = new GameObject();
			frontImageGameObject.transform.SetParent(container.transform);
			frontImageGameObject.name = "HealthBar Foreground";
			_foregroundImage = frontImageGameObject.AddComponent<Image>();
			_foregroundImage.transform.position = Vector3.zero;
			_foregroundImage.transform.localScale = Vector3.one;
			_foregroundImage.color = ForegroundColor.Evaluate(1);
			_foregroundImage.GetComponent<RectTransform>().sizeDelta = Size - BackgroundPadding*2;
			_foregroundImage.GetComponent<RectTransform>().anchoredPosition = -_foregroundImage.GetComponent<RectTransform>().sizeDelta/2;
			_foregroundImage.GetComponent<RectTransform>().pivot = Vector2.zero;

			if (Billboard)
			{
				MMBillboard billboard = _progressBar.gameObject.AddComponent<MMBillboard>();
				billboard.NestObject = !NestDrawnHealthBar;
			}

			_progressBar.LerpDecreasingDelayedBar = LerpDelayedBar;
			_progressBar.LerpForegroundBar = LerpFrontBar;
			_progressBar.LerpDecreasingDelayedBarSpeed = LerpDelayedBarSpeed;
			_progressBar.LerpForegroundBarSpeedIncreasing = LerpFrontBarSpeed;
			_progressBar.ForegroundBar = _foregroundImage.transform;
			_progressBar.DelayedBarDecreasing = _delayedImage.transform;
			_progressBar.DecreasingDelay = Delay;
			_progressBar.BumpScaleOnChange = BumpScaleOnChange;
			_progressBar.BumpDuration = BumpDuration;
			_progressBar.BumpScaleAnimationCurve = BumpAnimationCurve;
			_progressBar.TimeScale = (TimeScale == TimeScales.Time) ? MMProgressBar.TimeScales.Time : MMProgressBar.TimeScales.UnscaledTime;
			container.transform.localEulerAngles = InitialRotationAngles;
			_progressBar.Initialization();
		}

		/// <summary>
		/// On Update, we hide or show our healthbar based on our current status
		/// </summary>
		protected virtual void Update()
		{
			if (_progressBar == null) 
			{
				return; 
			}

			if (_finalHideStarted)
			{
				return;
			}

			UpdateDrawnColors();
            
			if (AlwaysVisible)	
			{ 
				return; 
			}

			if (_showBar)
			{
				ShowBar(true);
				float currentTime = (TimeScale == TimeScales.UnscaledTime) ? Time.unscaledTime : Time.time;
				if (currentTime - _lastShowTimestamp > DisplayDurationOnHit)
				{
					_showBar = false;
				}
			}
			else
			{
				if (BarIsShown())
				{
					ShowBar(false);	
				}
			}
		}

		/// <summary>
		/// Hides the bar when it reaches zero
		/// </summary>
		/// <returns>The hide bar.</returns>
		protected virtual IEnumerator FinalHideBar()
		{
			_finalHideStarted = true;
			if (InstantiatedOnDeath != null)
			{
				GameObject instantiatedOnDeath = Instantiate(InstantiatedOnDeath, this.transform.position + HealthBarOffset, this.transform.rotation);
				SceneManager.MoveGameObjectToScene(instantiatedOnDeath.gameObject, this.gameObject.scene);
			}
			if (HideBarAtZeroDelay == 0)
			{
				_showBar = false;
				ShowBar(false);
				yield return null;
			}
			else
			{
				_progressBar.HideBar(HideBarAtZeroDelay);
			}            
		}

		/// <summary>
		/// Updates the colors of the different bars
		/// </summary>
		protected virtual void UpdateDrawnColors()
		{
			if (HealthBarType != HealthBarTypes.Drawn)
			{
				return;
			}

			if (_progressBar.Bumping)
			{
				return;
			}

			if (_borderImage != null)
			{
				_borderImage.color = BorderColor.Evaluate(_progressBar.BarProgress);
			}

			if (_backgroundImage != null)
			{
				_backgroundImage.color = BackgroundColor.Evaluate(_progressBar.BarProgress);
			}

			if (_delayedImage != null)
			{
				_delayedImage.color = DelayedColor.Evaluate(_progressBar.BarProgress);
			}

			if (_foregroundImage != null)
			{
				_foregroundImage.color = ForegroundColor.Evaluate(_progressBar.BarProgress);
			}
		}

		/// <summary>
		/// Updates the bar
		/// </summary>
		/// <param name="currentHealth">Current health.</param>
		/// <param name="minHealth">Minimum health.</param>
		/// <param name="maxHealth">Max health.</param>
		/// <param name="show">Whether or not we should show the bar.</param>
		public virtual void UpdateBar(float currentHealth, float minHealth, float maxHealth, bool show)
		{
			// if the healthbar isn't supposed to be always displayed, we turn it on for the specified duration
			if (!AlwaysVisible && show)
			{
				_showBar = true;
				_lastShowTimestamp = (TimeScale == TimeScales.UnscaledTime) ? Time.unscaledTime : Time.time;
			}

			if (_progressBar != null)
			{
				_progressBar.UpdateBar(currentHealth, minHealth, maxHealth)	;
                
				if (HideBarAtZero && _progressBar.BarTarget <= 0)
				{
					StartCoroutine(FinalHideBar());
				}

				if (BumpScaleOnChange)
				{
					_progressBar.Bump();
				}
			}
		}

		/// <summary>
		/// A test method used to update the bar when pressing the TestUpdateHealth button in the inspector
		/// </summary>
		protected virtual void TestUpdateHealth()
		{
			UpdateBar(TestCurrentHealth, TestMinHealth, TestMaxHealth, true);
		}

		#endif
	}
}