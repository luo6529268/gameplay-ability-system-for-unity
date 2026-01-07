using System;
using System.Collections;
using UnityEngine;
using MoreMountains.Tools;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks
{
	[Serializable]
	public abstract class MMF_Feedback
	{
		#region Properties

		public const string _randomnessGroupName = "Feedback Randomness";
		public const string _rangeGroupName = "Feedback Range";
		public const string _automaticSetupGroupName = "Automatic Setup";
		
		[MMFInspectorGroup("Feedback Settings", true, 0, false, true)]
        /// whether or not this feedback is active
        [Tooltip("此反馈是否激活")]
        public bool Active = true;

        [HideInInspector] public int UniqueID;

        /// the name of this feedback to display in the inspector
        [Tooltip("在检视面板中显示的此反馈的名称")]
        public string Label = "MMFeedback";

        /// whether to broadcast this feedback's message using an int or a scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
        /// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip(
            "是否通过整型或脚本对象广播此反馈的消息。整型设置简单，但可能会变得混乱，难以记住哪个整型对应哪个消息。 " +
            "MMChannel脚本对象需要事先创建，但带有可读名称，更易于扩展")]
        public MMChannelModes ChannelMode = MMChannelModes.Int;

        /// 此反馈将通信的频道ID
        [Tooltip("此反馈将通信的频道ID")]
        [MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
        public int Channel = 0;

        /// 用于广播此反馈的MMChannel定义资产。震动器需要引用相同的MMChannel定义来接收事件 - 创建MMChannel，
        /// 右键点击项目中的任何位置（通常在Data文件夹中）然后选择MoreMountains > MMChannel，然后给它一个独特的名称
        [Tooltip(
            "用于广播此反馈的MMChannel定义资产。震动器需要引用相同的MMChannel定义来接收事件 - 创建MMChannel， " +
            "右键点击项目中的任何位置（通常在Data文件夹中）然后选择MoreMountains > MMChannel，然后给它一个独特的名称")]
        [MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
        public MMChannel MMChannelDefinition = null;

        /// 此反馈发生的几率（百分比：100：总是发生，0：从不发生，50：每两次调用发生一次，等）
        [Tooltip(
            "此反馈发生的几率（百分比：100：总是发生，0：从不发生，50：每两次调用发生一次，等）")]
        [Range(0, 100)]
        public float Chance = 100f;

        /// 使用此颜色自定义反馈在MMF_Player列表中的背景色
        [Tooltip("使用此颜色自定义反馈在MMF_Player列表中的背景色")]
        public Color DisplayColor = Color.black;

        /// 一系列与时间相关的值（延迟、重复等）
        [Tooltip("一系列与时间相关的值（延迟、重复等）")]
        public MMFeedbackTiming Timing;

        /// 一组设置，允许你为此反馈定义自动目标获取，以（例如）自动获取此游戏对象上的目标，或父对象、子对象，或参考持有者上的目标
        [Tooltip("一组设置，允许你为此反馈定义自动目标获取，以（例如）自动获取此游戏对象上的目标，或父对象、子对象，或参考持有者上的目标")]
        public MMFeedbackTargetAcquisition AutomatedTargetAcquisition;

        [MMFInspectorGroup(_randomnessGroupName, true, 58, false, true)]
        /// 如果为真，则在播放时将强度乘以RandomMultiplier.x和RandomMultiplier.y之间的随机值
        [Tooltip(
            "如果为真，则在播放时将强度乘以RandomMultiplier.x和RandomMultiplier.y之间的随机值")]
        public bool RandomizeOutput = false;


        /// 一个随机值（在它的x和y之间随机化），如果RandomizeOutput为真，则用它乘以此反馈的输出
        [Tooltip(
            "一个随机值（在它的x和y之间随机化），如果RandomizeOutput为真，则用它乘以此反馈的输出")]
        [MMFCondition("RandomizeOutput", true)]
        [MMFVector("最小", "最大")]
        public Vector2 RandomMultiplier = new Vector2(0.8f, 1f);

        /// 如果为真，则在播放时将此反馈的持续时间乘以RandomDurationMultiplier.x和RandomDurationMultiplier.y之间的随机值
        [Tooltip(
            "如果为真，则在播放时将此反馈的持续时间乘以RandomDurationMultiplier.x和RandomDurationMultiplier.y之间的随机值")]
        public bool RandomizeDuration = false;

        /// 一个随机值（在它的x和y之间随机化），如果RandomizeDuration为真，则用它乘以此反馈的持续时间
        [Tooltip(
            "一个随机值（在它的x和y之间随机化），如果RandomizeDuration为真，则用它乘以此反馈的持续时间")]
        [MMFCondition("RandomizeDuration", true)]
        [MMFVector("最小", "最大")]
        public Vector2 RandomDurationMultiplier = new Vector2(0.5f, 2f);

        [MMFInspectorGroup(_rangeGroupName, true, 47)]
        /// 如果为真，则只有指定范围内的震动器会响应此反馈
        [Tooltip("如果为真，则只有指定范围内的震动器会响应此反馈")]
        public bool UseRange = false;

        /// 当处于UseRange模式时，只有在该距离内的震动器会响应此反馈
        [Tooltip("当处于UseRange模式时，只有在该距离内的震动器会响应此反馈")]
        public float RangeDistance = 5f;

        /// 当处于UseRange模式时，是否根据RangeFallOff曲线修改震动强度  
        [Tooltip("当处于UseRange模式时，是否根据RangeFallOff曲线修改震动强度")]
        public bool UseRangeFalloff = false;

        /// 用于定义衰减的动画曲线（在x轴上0代表范围中心，1代表最大距离）
        [Tooltip(
            "用于定义衰减的动画曲线（在x轴上0代表范围中心，1代表最大距离）")]
        public AnimationCurve RangeFalloff = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

        /// 用于重新映射衰减曲线的y轴的0和1的值
        [Tooltip("用于重新映射衰减曲线的y轴的0和1的值")]
        [MMFVector("零", "一")]
        public Vector2 RemapRangeFalloff = new Vector2(0f, 1f);

        [MMFInspectorGroup(_automaticSetupGroupName, true, 49, false, true)]

        /// 一个按钮，用于尝试自动设置此反馈的震动器，将所需的震动器添加到场景中
        [Tooltip("一个按钮，用于尝试自动设置此反馈的震动器，将所需的震动器添加到场景中")]
        public MMF_Button AutomaticShakerSetupButton;

        /// 调用Initialization方法时定义的反馈所有者
        [HideInInspector] public MMF_Player Owner;

        [HideInInspector]
        /// 此反馈是否处于调试模式
        public bool DebugActive = false;

        /// 如果为真，此反馈将暂停执行反馈序列
        public virtual IEnumerator Pause => null;

        /// 如果为真，此反馈将等待所有先前的反馈运行完毕后再运行
        public virtual bool HoldingPause => false;

        /// 如果为真，此反馈将等待所有先前的反馈运行完毕后，然后再次运行所有先前的反馈
        public virtual bool LooperPause => false;

        /// 如果为真，此反馈将在其父MMFeedbacks调用Resume()后继续执行
        public virtual bool ScriptDrivenPause { get; set; }

        /// 如果为正值，如果尚未通过脚本恢复，则反馈将在该持续时间后自动恢复
        public virtual float ScriptDrivenPauseAutoResume { get; set; }

        /// 如果为真，此反馈将等待所有先前的反馈运行完毕后，然后再次运行所有先前的反馈
        public virtual bool LooperStart => false;

        /// 如果为真，将显示Channel属性，否则将隐藏
        public virtual bool HasChannel => false;

        /// 如果为真，此反馈将显示自动震动器设置按钮       
        public virtual bool HasAutomaticShakerSetup => false;

        /// 如果为真，将显示Randomness组，否则将隐藏        
        public virtual bool HasRandomness => false;

        /// 如果为真，此反馈将实施ForceInitialState，否则调用该方法将无效果
        public virtual bool CanForceInitialValue => false;

        /// 如果为真，强制初始值将在两帧后发生
        public virtual bool ForceInitialValueDelayed => false;

        /// whether or not this feedback can automatically grab the target on this game object, or a parent, a child, or on a reference holder
        public virtual bool HasAutomatedTargetAcquisition => false;
		/// when in forced reference mode, this will contain the forced reference holder that will be used (usually set by itself)
		public virtual MMF_ReferenceHolder ForcedReferenceHolder { get; set; }

		/// if this is true, the Range group will be displayed, otherwise it'll be hidden        
		public virtual bool HasRange => false;

		/// the total amount of plays this feedback has left
		public virtual int PlaysLeft => _playsLeft;

		public virtual bool HasCustomInspectors => false;
		/// an overridable color for your feedback, that can be redefined per feedback. White is the only reserved color, and the feedback will revert to 
		/// normal (light or dark skin) when left to White
		#if UNITY_EDITOR
		public virtual Color FeedbackColor => Color.white;
		#endif
		/// returns true if this feedback is in cooldown at this time (and thus can't play), false otherwise
		public virtual bool InCooldown => (Timing.CooldownDuration > 0f) &&
		                                  (FeedbackTime - _lastPlayTimestamp < Timing.CooldownDuration);

		/// if this is true, this feedback is currently playing
		public virtual bool IsPlaying { get; set; }

		/// <summary>
		/// Computes the new intensity, taking into account constant intensity and potential randomness
		/// </summary>
		/// <param name="intensity"></param>
		/// <returns></returns>
		public virtual float ComputeIntensity(float intensity, Vector3 position)
		{
			float result = Timing.ConstantIntensity ? 1f : intensity;
			result *= ComputedRandomMultiplier;
			result *= Owner.ComputeRangeIntensityMultiplier(position);
			return result;
		}

		/// <summary>
		/// Returns the random multiplier to apply to this feedback's output
		/// </summary>
		public virtual float ComputedRandomMultiplier =>
			RandomizeOutput ? Random.Range(RandomMultiplier.x, RandomMultiplier.y) : 1f;

		/// <summary>
		/// Returns the timescale mode to use in logic, taking into account the one set at the feedback level and the player level
		/// </summary>
		public virtual TimescaleModes ComputedTimescaleMode
		{
			get
			{
				if (Owner.ForceTimescaleMode)
				{
					return Owner.ForcedTimescaleMode;
				}

				return Timing.TimescaleMode;
			}
		}

		/// returns true if this feedback is in Scaled timescale mode, false otherwise
		public virtual bool InScaledTimescaleMode
		{
			get
			{
				if (Owner.ForceTimescaleMode)
				{
					return (Owner.ForcedTimescaleMode == TimescaleModes.Scaled);
				}

				return (Timing.TimescaleMode == TimescaleModes.Scaled);
			}
		}

		/// the time (or unscaled time) based on the selected Timing settings
		public virtual float FeedbackTime
		{
			get
			{
				float timescaleMultiplier = Owner.TimescaleMultiplier;
				
				#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					return (float)EditorApplication.timeSinceStartup * timescaleMultiplier;
				}
				#endif

				if (Timing.UseScriptDrivenTimescale)
				{
					return Timing.ScriptDrivenTime * timescaleMultiplier;
				}

				if (Owner.ForceTimescaleMode)
				{
					if (Owner.ForcedTimescaleMode == TimescaleModes.Scaled)
					{
						return Time.time * timescaleMultiplier;
					}
					else
					{
						return Time.unscaledTime * timescaleMultiplier;
					}
				}

				if (Timing.TimescaleMode == TimescaleModes.Scaled)
				{
					return Time.time * timescaleMultiplier;
				}
				else
				{
					return Time.unscaledTime * timescaleMultiplier;
				}
			}
		}

		/// the delta time (or unscaled delta time) based on the selected Timing settings
		public virtual float FeedbackDeltaTime
		{
			get
			{
				float timescaleMultiplier = Owner.TimescaleMultiplier;
				
				if (Timing.UseScriptDrivenTimescale)
				{
					return Timing.ScriptDrivenDeltaTime * timescaleMultiplier;
				}

				if (Owner.ForceTimescaleMode)
				{
					if (Owner.ForcedTimescaleMode == TimescaleModes.Scaled)
					{
						return Time.deltaTime * timescaleMultiplier;
					}
					else
					{
						return Time.unscaledDeltaTime * timescaleMultiplier;
					}
				}

				if (Owner.SkippingToTheEnd)
				{
					return float.MaxValue;
				}

				if (Timing.TimescaleMode == TimescaleModes.Scaled)
				{
					return Time.deltaTime * timescaleMultiplier;
				}
				else
				{
					return Time.unscaledDeltaTime * timescaleMultiplier;
				}
			}
		}

		/// <summary>
		/// The total duration of this feedback :
		/// total = initial delay + duration * (number of repeats + delay between repeats)  
		/// </summary>
		public virtual float TotalDuration
		{
			get
			{
				return _totalDuration;
			}
		}

		public virtual bool IsExpanded { get; set; }

		/// <summary>
		/// A flag used to determine if a feedback has all it needs, or if it requires some extra setup.
		/// This flag will be used to display a warning icon in the inspector if the feedback is not ready to be played.
		/// </summary>
		public virtual bool RequiresSetup => _requiresSetup;
		public virtual string RequiredTarget => _requiredTarget;

		public virtual void CacheRequiresSetup()
		{
			#if UNITY_EDITOR
			
			_requiresSetup = EvaluateRequiresSetup();
			if (_requiresSetup && HasAutomatedTargetAcquisition && (AutomatedTargetAcquisition != null) && (AutomatedTargetAcquisition.Mode != MMFeedbackTargetAcquisition.Modes.None))
			{
				_requiresSetup = false;
			}
			if ((RequiredTargetText != _requiredTargetTextCached) || (RequiredTargetTextExtra != _requiredTargetTextCachedExtra))
			{
				_requiredTarget = RequiredTargetText == "" ? "" : "[" + RequiredTargetText + "]" + RequiredTargetTextExtra;
				_requiredTargetTextCached = RequiredTargetText;
				_requiredTargetTextCachedExtra = RequiredTargetTextExtra;
			}
			
			#endif
		}
		/// if this is true, group inspectors will be displayed within this feedback
		public virtual bool DrawGroupInspectors => true;
		/// if this is true, the feedback will be displayed in the MMF Player's list with a full color background, as opposed to just a small line on the left
		public virtual bool DisplayFullHeaderColor => false;
		/// defines the setup text that will be displayed on the feedback, should setup be required
		public virtual string RequiresSetupText => "This feedback requires some additional setup.";
		/// the text used to describe the required target
		public virtual string RequiredTargetText => "";
		/// the text used to describe the required target, if more info is needed
		public virtual string RequiredTargetTextExtra => "";

		/// <summary>
		/// Override this method to determine if a feedback requires setup 
		/// </summary>
		/// <returns></returns>
		public virtual bool EvaluateRequiresSetup() => false;

		public virtual string RequiredChannelText
		{
			get
			{
				if (ChannelMode == MMChannelModes.MMChannel)
				{
					if (MMChannelDefinition == null)
					{
						return "None";
					}

					return MMChannelDefinition.name;
				}

				return "Channel "+Channel;
			}
		}

		// the timestamp at which this feedback was last played
		public virtual float FeedbackStartedAt => Application.isPlaying ? _lastPlayTimestamp : -1f;

		// the perceived duration of the feedback, to be used to display its progress bar, meant to be overridden with meaningful data by each feedback
		public virtual float FeedbackDuration
		{
			get { return 0f; }
			set {  }
		}

		/// <summary>
		/// Use this method to change the duration of this feedback
		/// </summary>
		/// <param name="newDuration"></param>
		public virtual void SetFeedbackDuration(float newDuration)
		{
			FeedbackDuration = newDuration;
			Owner.ComputeCachedTotalDuration();
		}

		/// whether or not this feedback is playing right now
		public virtual bool FeedbackPlaying =>
			((FeedbackStartedAt > 0f) && (Time.time - FeedbackStartedAt < FeedbackDuration));

		/// a ChannelData object, ready to pass to an event
		public virtual MMChannelData ChannelData => _channelData.Set(ChannelMode, Channel, MMChannelDefinition);

		protected float _lastPlayTimestamp = -1f;
		protected int _playsLeft;
		protected bool _initialized = false;
		protected Coroutine _playCoroutine;
		protected Coroutine _infinitePlayCoroutine;
		protected Coroutine _sequenceCoroutine;
		protected Coroutine _repeatedPlayCoroutine;
		protected bool _requiresSetup = false;
		protected string _requiredTarget = "";
		protected float _randomDurationMultiplier = 1f;
		protected int _sequenceTrackID = 0;
		protected float _beatInterval;
		protected bool BeatThisFrame = false;
		protected int LastBeatIndex = 0;
		protected int CurrentSequenceIndex = 0;
		protected float LastBeatTimestamp = 0f;
		protected MMChannelData _channelData;
		protected float _totalDuration = 0f;
		protected int _indexInOwnerFeedbackList = 0;
		protected string _requiredTargetTextCached = ".";
		protected string _requiredTargetTextCachedExtra = "";
		protected float _repeatOffset = 0f;

		#endregion Properties

		#region Initialization

		/// <summary>
		/// Runs at Awake, lets you preinitialize your custom feedback before Initialization
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="index"></param>
		public virtual void PreInitialization(MMF_Player owner, int index)
		{
			_channelData = new MMChannelData(ChannelMode, Channel, MMChannelDefinition);
		}

		/// <summary>
		/// Typically runs on Start, Initializes the feedback and its timing related variables
		/// </summary>
		/// <param name="owner"></param>
		public virtual void Initialization(MMF_Player owner, int index)
		{
			if (Timing == null)
			{
				Timing = new MMFeedbackTiming();
			}

			SetIndexInFeedbacksList(index);
			_lastPlayTimestamp = -1f;
			Timing.PlayCount = 0;
			_initialized = true;
			Owner = owner;
			_playsLeft = Timing.NumberOfRepeats + 1;
			_repeatOffset = 0f;
			_channelData = new MMChannelData(ChannelMode, Channel, MMChannelDefinition);
			AutomateTargetAcquisitionInternal();
			SetInitialDelay(Timing.InitialDelay);
			SetDelayBetweenRepeats(Timing.DelayBetweenRepeats);
			SetSequence(Timing.Sequence);
			CustomInitialization(owner);
		}

		/// <summary>
		/// Lets you specify at what index this feedback is in the list - use carefully (or don't use at all)
		/// </summary>
		/// <param name="index"></param>
		public virtual void SetIndexInFeedbacksList(int index)
		{
			_indexInOwnerFeedbackList = index;
		}

		/// <summary>
		/// Call this method (either directly or via the inspector button) to try and automatically setup this feedback's
		/// corresponding shaker in the scene
		/// </summary>
		public virtual void AutomaticShakerSetup()
		{
			
		}

		#endregion Initialization
		
		#region Automation
		
		/// <summary>
		/// Performs automated target acquisition, if needed
		/// </summary>
		protected virtual void AutomateTargetAcquisitionInternal()
		{
			if (!HasAutomatedTargetAcquisition)
			{
				return;
			}
			
			if (AutomatedTargetAcquisition == null)
			{
				AutomatedTargetAcquisition = new MMFeedbackTargetAcquisition();
			}

			if (AutomatedTargetAcquisition.Mode == MMFeedbackTargetAcquisition.Modes.None)
			{
				return;
			}

			AutomateTargetAcquisition();
			CacheRequiresSetup();
		}

		/// <summary>
		/// Lets you force target acquisition, outside of initialization where it usually occurs
		/// </summary>
		public virtual void ForceAutomateTargetAcquisition()
		{
			AutomateTargetAcquisition();
			CacheRequiresSetup();
		}

		/// <summary>
		/// A method meant to be implemented per feedback letting you specify what happens (usually setting a target)
		/// </summary>
		protected virtual void AutomateTargetAcquisition()
		{
			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected virtual GameObject FindAutomatedTargetGameObject()
		{
			return MMFeedbackTargetAcquisition.FindAutomatedTargetGameObject(AutomatedTargetAcquisition, Owner, _indexInOwnerFeedbackList);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected virtual T FindAutomatedTarget<T>()
		{
			return MMFeedbackTargetAcquisition.FindAutomatedTarget<T>(AutomatedTargetAcquisition, Owner, _indexInOwnerFeedbackList);
		}
		
		#endregion Automation

		#region Play

		/// <summary>
		/// Plays the feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void Play(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active)
			{
				return;
			}

			if (!_initialized)
			{
				string feedbackName = this.ToString().Replace("MoreMountains.Feedbacks.", "");
				Debug.LogWarning("The " + feedbackName +
				                 " feedback on "+Owner.gameObject.name+" is being played without having been initialized. Always call the Initialization() method first. This can be done manually, or on Start or Awake (automatically on Start is the default). If you're auto playing your feedback on Start or on Enable, initialize on Awake (which runs before Start and Enable). You can change that setting on your MMF Player, unfold the Settings foldout at the top, and change the Initialization Mode.", Owner.gameObject);
			}

			// we check the cooldown
			if (InCooldown)
			{
				return;
			}

			if (Timing.InitialDelay > 0f)
			{
				_playCoroutine = Owner.StartCoroutine(PlayCoroutine(position, feedbacksIntensity));
			}
			else
			{
				RegularPlay(position, feedbacksIntensity);
			}
		}

		/// <summary>
		/// An internal coroutine delaying the initial play of the feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator PlayCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			yield return WaitFor(ApplyTimeMultiplier(Timing.InitialDelay));
			RegularPlay(position, feedbacksIntensity);
		}

		/// <summary>
		/// Triggers delaying coroutines if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void RegularPlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Chance == 0f)
			{
				return;
			}

			if (Chance != 100f)
			{
				// determine the odds
				float random = Random.Range(0f, 100f);
				if (random > Chance)
				{
					return;
				}
			}
			
			if (Timing.LimitPlayCount && (Timing.PlayCount >= Timing.MaxPlayCount))
			{
				return;
			}

			if (Timing.UseIntensityInterval)
			{
				if ((feedbacksIntensity < Timing.IntensityIntervalMin) ||
				    (feedbacksIntensity >= Timing.IntensityIntervalMax))
				{
					return;
				}
			}
			
			_repeatOffset = 0f;

			if (Timing.RepeatForever)
			{
				_infinitePlayCoroutine = Owner.StartCoroutine(InfinitePlay(position, feedbacksIntensity));
				return;
			}

			if (Timing.NumberOfRepeats > 0)
			{
				_repeatedPlayCoroutine = Owner.StartCoroutine(RepeatedPlay(position, feedbacksIntensity));
				return;
			}

			if (Timing.Sequence == null)
			{
				TriggerCustomPlay(position, feedbacksIntensity);
			}
			else
			{
				_sequenceCoroutine = Owner.StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
			}
		}

		/// <summary>
		/// Triggers a custom play
		/// </summary>
		/// <param name="position"></param>
		/// <param name="intensity"></param>
		protected virtual void TriggerCustomPlay(Vector3 position, float intensity)
		{
			Timing.PlayCount++;
			_lastPlayTimestamp = FeedbackTime;
			CustomPlayFeedback(position, intensity);
		}

		/// <summary>
		/// Internal coroutine used for repeated play without end
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator InfinitePlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			while (true)
			{
				yield return TriggerRepeatedPlay(position, feedbacksIntensity);
			}
		}

		/// <summary>
		/// Internal coroutine used for repeated play
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator RepeatedPlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			while (_playsLeft > 0)
			{
				_playsLeft--;
				yield return TriggerRepeatedPlay(position, feedbacksIntensity);
			}

			_playsLeft = Timing.NumberOfRepeats + 1;
		}

		protected virtual IEnumerator TriggerRepeatedPlay(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Timing.Sequence == null)
			{
				TriggerCustomPlay(position, feedbacksIntensity);
				float repeatStartTime = Time.time;
					
				float repeatDuration = Timing.DelayBetweenRepeats + FeedbackDuration;
				if (_repeatOffset <= Timing.DelayBetweenRepeats)
				{
					repeatDuration = Timing.DelayBetweenRepeats + FeedbackDuration - _repeatOffset;	
				}
					
				yield return WaitFor(repeatDuration);
				yield return null;
				_repeatOffset = (Time.time - repeatStartTime - repeatDuration);
			}
			else
			{
				_sequenceCoroutine = Owner.StartCoroutine(SequenceCoroutine(position, feedbacksIntensity));
				float delay = ApplyTimeMultiplier(Timing.DelayBetweenRepeats) + Timing.Sequence.Length;
				yield return WaitFor(delay);
			}
		}

		#endregion Play

		#region Sequence

		/// <summary>
		/// A coroutine used to play this feedback on a sequence
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator SequenceCoroutine(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			yield return null;
			float timeStartedAt = FeedbackTime;
			float lastFrame = FeedbackTime;

			BeatThisFrame = false;
			LastBeatIndex = 0;
			CurrentSequenceIndex = 0;
			LastBeatTimestamp = 0f;

			if (Timing.Quantized)
			{
				while (CurrentSequenceIndex < Timing.Sequence.QuantizedSequence[0].Line.Count)
				{
					_beatInterval = 60f / Timing.TargetBPM;

					if ((FeedbackTime - LastBeatTimestamp >= _beatInterval) || (LastBeatTimestamp == 0f))
					{
						BeatThisFrame = true;
						LastBeatIndex = CurrentSequenceIndex;
						LastBeatTimestamp = FeedbackTime;

						for (int i = 0; i < Timing.Sequence.SequenceTracks.Count; i++)
						{
							if (Timing.Sequence.QuantizedSequence[i].Line[CurrentSequenceIndex].ID == Timing.TrackID)
							{
								TriggerCustomPlay(position, feedbacksIntensity);
							}
						}

						CurrentSequenceIndex++;
					}

					yield return null;
				}
			}
			else
			{
				while (FeedbackTime - timeStartedAt < Timing.Sequence.Length)
				{
					foreach (MMSequenceNote item in Timing.Sequence.OriginalSequence.Line)
					{
						if ((item.ID == Timing.TrackID) && (item.Timestamp >= lastFrame) &&
						    (item.Timestamp <= FeedbackTime - timeStartedAt))
						{
							TriggerCustomPlay(position, feedbacksIntensity);
						}
					}

					lastFrame = FeedbackTime - timeStartedAt;
					yield return null;
				}
			}
		}

		/// <summary>
		/// Use this method to change this feedback's sequence at runtime
		/// </summary>
		/// <param name="newSequence"></param>
		public virtual void SetSequence(MMSequence newSequence)
		{
			Timing.Sequence = newSequence;
			if (Timing.Sequence != null)
			{
				for (int i = 0; i < Timing.Sequence.SequenceTracks.Count; i++)
				{
					if (Timing.Sequence.SequenceTracks[i].ID == Timing.TrackID)
					{
						_sequenceTrackID = i;
					}
				}
			}
		}

		#endregion Sequence

		#region Controls

		/// <summary>
		/// Stops all feedbacks from playing. Will stop repeating feedbacks, and call custom stop implementations
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void Stop(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (_playCoroutine != null)
			{
				Owner.StopCoroutine(_playCoroutine);
			}

			if (_infinitePlayCoroutine != null)
			{
				Owner.StopCoroutine(_infinitePlayCoroutine);
			}

			if (_repeatedPlayCoroutine != null)
			{
				Owner.StopCoroutine(_repeatedPlayCoroutine);
			}

			if (_sequenceCoroutine != null)
			{
				Owner.StopCoroutine(_sequenceCoroutine);
			}

			_lastPlayTimestamp = -1f;
			_playsLeft = Timing.NumberOfRepeats + 1;
			if (Timing.InterruptsOnStop)
			{
				CustomStopFeedback(position, feedbacksIntensity);
			}
		}

		/// <summary>
		/// Called when skipping to the end of MMF_Player, calls custom Skip on all feedbacks
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void SkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			CustomSkipToTheEnd(position, feedbacksIntensity);
		}

		/// <summary>
		/// Forces the feedback to set its initial value (behavior will change from feedback to feedback,
		/// but for example, a Position feedback that moves a Transform from point A to B would
		/// automatically move the Transform to point A when ForceInitialState is called
		/// </summary>
		public virtual void ForceInitialValue(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!CanForceInitialValue)
			{
				return;
			}
			if (ForceInitialValueDelayed)
			{
				Owner.StartCoroutine(ForceInitialValueDelayedCo(position, feedbacksIntensity));
			}
			else
			{
				Play(position, feedbacksIntensity);
				Stop(position, feedbacksIntensity);	
			}
		}

		/// <summary>
		/// A coroutine used to delay the Stop when forcing initial values (used mostly with shaker based feedbacks)
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator ForceInitialValueDelayedCo(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			Play(position, feedbacksIntensity);
			yield return new WaitForEndOfFrame();
			Stop(position, feedbacksIntensity);
			
		}

		/// <summary>
		/// Called when restoring the initial state of a player, calls custom Restore on all feedbacks
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void RestoreInitialValues()
		{
			CustomRestoreInitialValues();
		}

		/// <summary>
		/// Calls this feedback's custom reset 
		/// </summary>
		public virtual void ResetFeedback()
		{
			_playsLeft = Timing.NumberOfRepeats + 1;
			if (Timing.SetPlayCountToZeroOnReset)
			{
				ResetPlayCount();
			}
			CustomReset();
		}

		/// <summary>
		/// This gets called by the MMF Player when all feedbacks have completed playing 
		/// </summary>
		public virtual void PlayerComplete()
		{
			CustomPlayerComplete();
		}

		#endregion

		#region Time

		/// <summary>
		/// Use this method to specify a new delay between repeats at runtime
		/// </summary>
		/// <param name="delay"></param>
		public virtual void SetDelayBetweenRepeats(float delay)
		{
			Timing.DelayBetweenRepeats = delay;
		}

		/// <summary>
		/// Use this method to specify a new initial delay at runtime
		/// </summary>
		/// <param name="delay"></param>
		public virtual void SetInitialDelay(float delay)
		{
			Timing.InitialDelay = delay;
		}

		/// <summary>
		/// Returns the t value at which to evaluate a curve at the end of this feedback's play time
		/// </summary>
		protected virtual float FinalNormalizedTime
		{
			get { return NormalPlayDirection ? 1f : 0f; }
		}

		/// <summary>
		/// Computes a new random duration multiplier
		/// </summary>
		public virtual void ComputeNewRandomDurationMultiplier()
		{
			_randomDurationMultiplier = Random.Range(RandomDurationMultiplier.x, RandomDurationMultiplier.y);
		}
		
		/// <summary>
		/// Resets the play count of this feedback
		/// </summary>
		public virtual void ResetPlayCount()
		{
			Timing.PlayCount = 0;
		}

		/// <summary>
		/// Applies the host MMFeedbacks' time multiplier to this feedback
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		protected virtual float ApplyTimeMultiplier(float duration)
		{
			if (Owner == null)
			{
				return 0f;
			}

			if (RandomizeDuration)
			{
				duration = duration * _randomDurationMultiplier;
			}

			return Owner.ApplyTimeMultiplier(duration);
		}

		/// <summary>
		/// Internal method used to wait for a duration, on scaled or unscaled time
		/// </summary>
		/// <param name="delay"></param>
		/// <returns></returns>
		protected virtual IEnumerator WaitFor(float delay)
		{
			if (InScaledTimescaleMode)
			{
				yield return MMFeedbacksCoroutine.WaitFor(delay);
			}
			else
			{
				yield return MMFeedbacksCoroutine.WaitForUnscaled(delay);
			}
		}

		/// <summary>
		/// Computes the total duration of this feedback
		/// </summary>
		public virtual void ComputeTotalDuration()
		{
			if ((Timing != null) && (!Timing.ContributeToTotalDuration))
			{
				_totalDuration = 0f;
				return;
			}

			float totalTime = 0f;

			if (Timing == null)
			{
				_totalDuration = 0f;
				return;
			}

			if (Timing.InitialDelay != 0)
			{
				totalTime += ApplyTimeMultiplier(Timing.InitialDelay);
			}

			totalTime += FeedbackDuration;

			if (Timing.NumberOfRepeats != 0)
			{
				float delayBetweenRepeats = ApplyTimeMultiplier(Timing.DelayBetweenRepeats);

				totalTime += (Timing.NumberOfRepeats * delayBetweenRepeats);
			}
				
			_totalDuration = totalTime;
		}

		#endregion Time

		#region Direction

		/// <summary>
		/// Returns a new value of the normalized time based on the current play direction of this feedback
		/// </summary>
		/// <param name="normalizedTime"></param>
		/// <returns></returns>
		protected virtual float ApplyDirection(float normalizedTime)
		{
			return NormalPlayDirection ? normalizedTime : 1 - normalizedTime;
		}

		/// <summary>
		/// Returns true if this feedback should play normally, or false if it should play in rewind
		/// </summary>
		public virtual bool NormalPlayDirection
		{
			get
			{
				switch (Timing.PlayDirection)
				{
					case MMFeedbackTiming.PlayDirections.FollowMMFeedbacksDirection:
						return (Owner.Direction == MMF_Player.Directions.TopToBottom);
					case MMFeedbackTiming.PlayDirections.AlwaysNormal:
						return true;
					case MMFeedbackTiming.PlayDirections.AlwaysRewind:
						return false;
					case MMFeedbackTiming.PlayDirections.OppositeMMFeedbacksDirection:
						return !(Owner.Direction == MMF_Player.Directions.TopToBottom);
				}

				return true;
			}
		}

		/// <summary>
		/// Returns true if this feedback should play in the current parent MMFeedbacks direction, according to its MMFeedbacksDirectionCondition setting
		/// </summary>
		public virtual bool ShouldPlayInThisSequenceDirection
		{
			get
			{
				if (Timing == null)
				{
					return true;
				}
				switch (Timing.MMFeedbacksDirectionCondition)
				{
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.Always:
						return true;
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards:
						return (Owner.Direction == MMF_Player.Directions.TopToBottom);
					case MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards:
						return (Owner.Direction == MMF_Player.Directions.BottomToTop);
				}

				return true;
			}
		}

		#endregion Direction

		#region Overrides

		/// <summary>
		/// This method describes all custom initialization processes the feedback requires, in addition to the main Initialization method
		/// </summary>
		/// <param name="owner"></param>
		protected virtual void CustomInitialization(MMF_Player owner) { }

		/// <summary>
		/// This method describes what happens when the feedback gets played
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected abstract void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f);

		/// <summary>
		/// This method describes what happens when the feedback gets stopped
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f) { }

		/// <summary>
		/// This method describes what happens when the feedback gets skipped to the end
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f) { }

		/// <summary>
		/// This method describes what happens when the feedback gets restored
		/// </summary>
		protected virtual void CustomRestoreInitialValues() { }
		/// <summary>
		/// This method describes what happens when the player this feedback belongs to completes playing
		/// </summary>
		protected virtual void CustomPlayerComplete() { }

		/// <summary>
		/// This method describes what happens when the feedback gets reset
		/// </summary>
		protected virtual void CustomReset() { }

		/// <summary>
		/// Use this method to initialize any custom attributes you may have
		/// </summary>
		public virtual void InitializeCustomAttributes()
		{
			if (HasAutomaticShakerSetup)
			{
				AutomaticShakerSetupButton = new MMF_Button("Automatic Shaker Setup", AutomaticShakerSetup);
			}
		}

		#endregion Overrides

		#region Event functions

		/// <summary>
		/// Triggered when a change happens in the inspector
		/// </summary>
		public virtual void OnValidate()
		{
			InitializeCustomAttributes();
			ComputeTotalDuration();
		}

		/// <summary>
		/// Triggered when the feedback gets added to the player
		/// </summary>
		public virtual void OnAddFeedback()
		{
			
		}

		/// <summary>
		/// Triggered when that feedback gets destroyed
		/// </summary>
		public virtual void OnDestroy() { }

		/// <summary>
		/// Triggered when the host MMF Player gets disabled
		/// </summary>
		public virtual void OnDisable() { }

		/// <summary>
		/// Triggered when the host MMF Player gets selected, can be used to draw gizmos
		/// </summary>
		public virtual void OnDrawGizmosSelectedHandler() { }

		#endregion
	}
}