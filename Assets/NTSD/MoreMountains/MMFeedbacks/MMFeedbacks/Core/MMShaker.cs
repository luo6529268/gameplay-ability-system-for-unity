using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    public class MMShaker : MMMonoBehaviour
	{
        [MMInspectorGroup("Shaker Settings", true, 3)]
        /// <summary>
        /// 是否通过一个由整数定义的频道或由MMChannel脚本化对象定义的频道来监听。整数设置简单，但可能会变得混乱，难以记住哪个整数对应哪个频道。
        /// MMChannel脚本化对象需要您提前创建，但带有可读名称，且更易于扩展。
        /// </summary>
        [Tooltip("是否通过一个由整数定义的频道或由MMChannel脚本化对象定义的频道来监听。整数设置简单，但可能会变得混乱，难以记住哪个整数对应哪个频道。 " +
                 "MMChannel脚本化对象需要您提前创建，但带有可读名称，且更易于扩展")]
        public MMChannelModes ChannelMode = MMChannelModes.Int;
        /// the channel to listen to - has to match the one on the feedback
        /// <summary>
        /// 要监听的频道 - 必须与反馈上的频道匹配
        /// </summary>
        [Tooltip("要监听的频道 - 必须与反馈上的频道匹配")]
        [MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
        public int Channel = 0;
        /// <summary>
        /// 用于监听事件的MMChannel定义资产。针对此震动器的反馈必须引用相同的MMChannel定义才能接收事件 - 要创建MMChannel，
        /// 在项目中的任何位置（通常在Data文件夹中）右键单击，然后选择MoreMountains > MMChannel，然后给它一个独特的名称
        /// </summary>
        [Tooltip("用于监听事件的MMChannel定义资产。针对此震动器的反馈必须引用相同的MMChannel定义才能接收事件 - 要创建MMChannel， " +
                 "在项目中的任何位置（通常在Data文件夹中）右键单击，然后选择MoreMountains > MMChannel，然后给它一个独特的名称")]
        [MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
        public MMChannel MMChannelDefinition = null;
        /// the duration of the shake, in seconds
        /// <summary>
        /// 震动的持续时间，以秒为单位
        /// </summary>
        [Tooltip("震动的持续时间，以秒为单位")]
        public float ShakeDuration = 0.2f;
        /// <summary>
        /// 如果为真，此震动器将在Awake时播放
        /// </summary>
        [Tooltip("如果为真，此震动器将在Awake时播放")]
        public bool PlayOnAwake = false;
        /// <summary>
        /// 如果为真，只要其游戏对象处于活动状态，震动器将永久震动
        /// </summary>
        [Tooltip("如果为真，只要其游戏对象处于活动状态，震动器将永久震动")]
        public bool PermanentShake = false;
        /// <summary>
        /// 如果为真，可以在震动时发生新的震动
        /// </summary>
        [Tooltip("如果为真，可以在震动时发生新的震动")]
        public bool Interruptible = true;
        /// <summary>
        /// 如果为真，此震动器将始终重置目标值，无论它是如何被调用的
        /// </summary>
        [Tooltip("如果为真，此震动器将始终重置目标值，无论它是如何被调用的")]
        public bool AlwaysResetTargetValuesAfterShake = false;
        /// <summary>
        /// 如果为真，此震动器将忽略在触发它的事件中传递的任何值，而只使用在检视器中设置的值
        /// </summary>
        [Tooltip("如果为真，此震动器将忽略在触发它的事件中传递的任何值，而只使用在检视器中设置的值")]
        public bool OnlyUseShakerValues = false;
        /// <summary>
        /// 震动后的一个冷却时间，在此期间不允许其他震动开始
        /// </summary>
        [Tooltip("震动后的一个冷却时间，在此期间不允许其他震动开始")]
        public float CooldownBetweenShakes = 0f;
        /// <summary>
        /// 此震动器是否正在震动
        /// </summary>
        [Tooltip("此震动器是否正在震动")]
        [MMFReadOnly]
        public bool Shaking = false;

        [HideInInspector] 
		public bool ForwardDirection = true;

		[HideInInspector] 
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;

		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
		public virtual MMChannelData ChannelData => new MMChannelData(ChannelMode, Channel, MMChannelDefinition);
        
		public virtual bool ListeningToEvents => _listeningToEvents;

		[HideInInspector]
		internal bool _listeningToEvents = false;
		protected float _shakeStartedTimestamp = -Single.MaxValue;
		protected float _remappedTimeSinceStart;
		protected bool _resetShakerValuesAfterShake;
		protected bool _resetTargetValuesAfterShake;
		protected float _journey;
        
		/// <summary>
		/// On Awake we grab our volume and profile
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
			// in case someone else trigger StartListening before Awake
			if (!_listeningToEvents)
			{
				StartListening();
			}
			Shaking = PlayOnAwake;
			this.enabled = PlayOnAwake;
		}

		/// <summary>
		/// Override this method to initialize your shaker
		/// </summary>
		protected virtual void Initialization()
		{
		}

		/// <summary>
		/// Call this externally if you need to force a new initialization
		/// </summary>
		public virtual void ForceInitialization()
		{
			Initialization();
		}

        /// <summary>
        /// Starts shaking the values
        /// 开始震动值
        /// </summary>
        public virtual void StartShaking()
		{
			_journey = ForwardDirection ? 0f : ShakeDuration;

			if (GetTime() - _shakeStartedTimestamp < CooldownBetweenShakes)
			{
				return;
			}
            
			if (Shaking)
			{
				return;
			}
			else
			{
				this.enabled = true;
				_shakeStartedTimestamp = GetTime();
				Shaking = true;
				GrabInitialValues();
				ShakeStarts();
			}
		}

        /// <summary>
        /// Describes what happens when a shake starts
        /// 描述震动开始时会发生什么
        /// </summary>
        protected virtual void ShakeStarts()
		{

		}

        /// <summary>
        /// A method designed to collect initial values
        /// 用于收集初始值的方法
        /// </summary>
        protected virtual void GrabInitialValues()
		{

		}

        /// <summary>
        /// On Update, we shake our values if needed, or reset if our shake has ended
        /// 在Update时，如果需要，我们震动值，或者如果震动结束则重置
        /// </summary>
        protected virtual void Update()
		{
			if (Shaking || PermanentShake)
			{
				Shake();
				_journey += ForwardDirection ? GetDeltaTime() : -GetDeltaTime();
			}

			if (Shaking && !PermanentShake && ((_journey < 0) || (_journey > ShakeDuration)))
			{
				Shaking = false;
				ShakeComplete();
			}

			if (PermanentShake)
			{
				if (_journey < 0)
				{
					_journey = ShakeDuration;
				}

				if (_journey > ShakeDuration)
				{
					_journey = 0;
				}
			}
		}

        /// <summary>
        /// Override this method to implement shake over time
        ///  覆盖此方法以实现随时间震动
        /// </summary>
        protected virtual void Shake()
		{

		}

        /// <summary>
        /// A method used to "shake" a flot over time along a curve
        /// 用于沿曲线随时间“震动”浮点数的方法
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="remapMin"></param>
        /// <param name="remapMax"></param>
        /// <param name="relativeIntensity"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        protected virtual float ShakeFloat(AnimationCurve curve, float remapMin, float remapMax, bool relativeIntensity, float initialValue)
		{
			float newValue = 0f;
            
			float remappedTime = MMFeedbacksHelpers.Remap(_journey, 0f, ShakeDuration, 0f, 1f);
            
			float curveValue = curve.Evaluate(remappedTime);
			newValue = MMFeedbacksHelpers.Remap(curveValue, 0f, 1f, remapMin, remapMax);
			if (relativeIntensity)
			{
				newValue += initialValue;
			}
			return newValue;
		}

		protected virtual Color ShakeGradient(Gradient gradient)
		{
			float remappedTime = MMFeedbacksHelpers.Remap(_journey, 0f, ShakeDuration, 0f, 1f);
			return gradient.Evaluate(remappedTime);
		}

		/// <summary>
		/// Resets the values on the target
		/// </summary>
		protected virtual void ResetTargetValues()
		{

		}

		/// <summary>
		/// Resets the values on the shaker
		/// </summary>
		protected virtual void ResetShakerValues()
		{

		}

        /// <summary>
        /// Describes what happens when the shake is complete
        /// 描述震动完成时会发生什么
        /// </summary>
        protected virtual void ShakeComplete()
		{
			_journey = ForwardDirection ? ShakeDuration : 0f;
			Shake();
			
			if (_resetTargetValuesAfterShake || AlwaysResetTargetValuesAfterShake)
			{
				ResetTargetValues();
			}   
			if (_resetShakerValuesAfterShake)
			{
				ResetShakerValues();
			}            
			this.enabled = false;
		}

		/// <summary>
		/// On enable we start shaking if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			StartShaking();
		}
             
		/// <summary>
		/// On destroy we stop listening for events
		/// </summary>
		protected virtual void OnDestroy()
		{
			StopListening();
		}

		/// <summary>
		/// On disable we complete our shake if it was in progress
		/// </summary>
		protected virtual void OnDisable()
		{
			if (Shaking)
			{
				ShakeComplete();
			}
		}

		/// <summary>
		/// Starts this shaker
		/// </summary>
		public virtual void Play()
		{
			if (GetTime() - _shakeStartedTimestamp < CooldownBetweenShakes)
			{
				return;
			}
			this.enabled = true;
		}

		/// <summary>
		/// Stops this shaker
		/// </summary>
		public virtual void Stop()
		{
			Shaking = false;
			ShakeComplete();
		}
        
		/// <summary>
		/// Starts listening for events
		/// </summary>
		public virtual void StartListening()
		{
			_listeningToEvents = true;
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public virtual void StopListening()
		{
			_listeningToEvents = false;
		}

        /// <summary>
        /// Returns true if this shaker should listen to events, false otherwise
        /// 如果这个震动器应该监听事件，则返回真（true），否则返回假（false）。
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        protected virtual bool CheckEventAllowed(MMChannelData channelData, bool useRange = false, float range = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return false;
			}
			if (!this.gameObject.activeInHierarchy)
			{
				return false;
			}
			else
			{
				if (useRange)
				{
					if (Vector3.Distance(this.transform.position, eventOriginPosition) > range)
					{
						return false;
					}
				}

				return true;
			}
		}
		
		public virtual float ComputeRangeIntensity(bool useRange, float rangeDistance, bool useRangeFalloff, AnimationCurve rangeFalloff, Vector2 remapRangeFalloff, Vector3 rangePosition)
		{
			if (!useRange)
			{
				return 1f;
			}

			float distanceToCenter = Vector3.Distance(rangePosition, this.transform.position);

			if (distanceToCenter > rangeDistance)
			{
				return 0f;
			}

			if (!useRangeFalloff)
			{
				return 1f;
			}

			float normalizedDistance = MMMaths.Remap(distanceToCenter, 0f, rangeDistance, 0f, 1f);
			float curveValue = rangeFalloff.Evaluate(normalizedDistance);
			float newIntensity = MMMaths.Remap(curveValue, 0f, 1f, remapRangeFalloff.x, remapRangeFalloff.y);
			return newIntensity;
		}
	}
}