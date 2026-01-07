using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using System.Linq;
using MoreMountains.Tools;
using UnityEditor.Experimental;
using UnityEngine.Events;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// A collection of MMFeedback, meant to be played altogether.
    /// This class provides a custom inspector to add and customize feedbacks, and public methods to trigger them, stop them, etc.
    /// You can either use it on its own, or bind it from another class and trigger it from there.
    /// 一个MMFeedback的集合，意味着可以一起玩。
    ///这个类提供了一个自定义检查器来添加和自定义反馈，以及公共方法来触发和停止反馈等。
    ///你可以单独使用它，也可以从另一个类绑定它并从那里触发它。
    /// </summary>
    [AddComponentMenu("")]
	public class MMFeedbacks : MonoBehaviour
	{
        /// <summary>
        /// MMFeedbacks可以播放的可能方向
        /// </summary>
        public enum Directions { TopToBottom, BottomToTop }

        /// <summary>
        /// 可能的安全模式（将执行检查以确保没有序列化错误损坏它们）
        /// - nope : 无安全检查
        /// - editor only : 在启用时执行检查
        /// - runtime only : 在Awake时执行检查
        /// - full : 执行编辑器和运行时检查，推荐设置
        /// </summary>
        public enum SafeModes { Nope, EditorOnly, RuntimeOnly, Full }

        /// <summary>
        /// 要触发的MMFeedback列表
        /// </summary>
        public List<MMFeedback> Feedbacks = new List<MMFeedback>();

        /// <summary>
        /// 可能的初始化模式。如果使用Script，你需要手动通过调用Initialization方法并传递一个所有者来初始化
        /// 否则，你可以让这个组件在Awake或Start时自行初始化，在这种情况下，所有者将是MMFeedbacks本身
        /// </summary>
        public enum InitializationModes { Script, Awake, Start }
        /// <summary>
        /// 选择的初始化模式
        /// </summary>
        [Tooltip("选择的初始化模式。如果使用Script，你需要手动通过调用Initialization方法并传递一个所有者来初始化。否则，你可以让这个组件在Awake或Start时自行初始化，在这种情况下，所有者将是MMFeedbacks本身")]
        public InitializationModes InitializationMode = InitializationModes.Start;

        /// <summary>
        /// 如果你将此设置为真，系统将进行更改以确保初始化总是在播放之前发生
        /// </summary>
        [Tooltip("如果你将此设置为真，系统将进行更改以确保初始化总是在播放之前发生")]
        public bool AutoInitialization = true;

        /// <summary>
        /// 选择的安全模式
        /// </summary>
        [Tooltip("选择的安全模式")]
        public SafeModes SafeMode = SafeModes.Full;

        /// <summary>
        /// 选择的方向
        /// </summary>
        [Tooltip("这些反馈应该播放的方向")]
        public Directions Direction = Directions.TopToBottom;

        /// <summary>
        /// 当所有反馈都播放完毕后，这个MMFeedbacks是否应该反转其方向
        /// </summary>
        [Tooltip("当所有反馈都播放完毕后，这个MMFeedbacks是否应该反转其方向")]
        public bool AutoChangeDirectionOnEnd = false;

        /// <summary>
        /// 是否在Start时自动播放这个反馈
        /// </summary>
        [Tooltip("是否在Start时自动播放这个反馈")]
        public bool AutoPlayOnStart = false;

        /// <summary>
        /// 是否在Enable时自动播放这个反馈
        /// </summary>
        [Tooltip("是否在Enable时自动播放这个反馈")]
        public bool AutoPlayOnEnable = false;


        /// if this is true, all feedbacks within that player will work on the specified ForcedTimescaleMode, regardless of their individual settings 
        [Tooltip("如果这是真的，那么玩家的所有反馈都将在指定的ForcedTimescaleMode上工作，而不管他们的个人设置如何")] 
		public bool ForceTimescaleMode = false;
		/// the time scale mode all feedbacks on this player should work on, if ForceTimescaleMode is true
		[Tooltip("如果ForceTimescaleMode为true，那么这个播放器的所有反馈都应该在时间尺度模式上工作")] 
		[MMFCondition("ForceTimescaleMode", true)]
		public TimescaleModes ForcedTimescaleMode = TimescaleModes.Unscaled;
		/// a time multiplier that will be applied to all feedback durations (initial delay, duration, delay between repeats...)
		[Tooltip("将应用于所有反馈持续时间（初始延迟，持续时间，重复之间的延迟……）的时间倍增器。")]
		public float DurationMultiplier = 1f;
		/// a multiplier to apply to all timescale operations (1: normal, less than 1: slower operations, higher than 1: faster operations)
		[Tooltip("适用于所有时间刻度操作的乘数（1：正常，小于1：较慢的操作，大于1：较快的操作）")]
		public float TimescaleMultiplier = 1f;
		/// if this is true, will expose a RandomDurationMultiplier. The final duration of each feedback will be : their base duration * DurationMultiplier * a random value between RandomDurationMultiplier.x and RandomDurationMultiplier.y
		[Tooltip("如果这是真的，将暴露一个 Random Duration Multiplier。每个反馈的最终持续时间将是：它们的基本持续时间 * Duration Multiplier * Random Duration Multiplier. x 和 Random Duration Multiplier.y 之间的随机值")]
		public bool RandomizeDuration = false;
		/// if RandomizeDuration is true, the min (x) and max (y) values for the random duration multiplier
		[Tooltip("如果随机持续时间乘数为真，则随机持续时间乘数的min（x）和max（y）值")]
		[MMCondition("RandomizeDuration", true)]
		public Vector2 RandomDurationMultiplier = new Vector2(0.5f, 1.5f);
		/// if this is true, more editor-only, detailed info will be displayed per feedback in the duration slot
		[Tooltip("如果这是真的，将在持续时间槽中显示更多,仅限编辑器的详细信息")]
		public bool DisplayFullDurationDetails = false;
		/// the timescale at which the player itself will operate. This notably impacts sequencing and pauses duration evaluation.
		[Tooltip("玩家本身操作的时间表。这明显影响了排序和暂停持续时间评估。")]
		public TimescaleModes PlayerTimescaleMode = TimescaleModes.Unscaled;

		/// if this is true, this feedback will only play if its distance to RangeCenter is lower or equal to RangeDistance
		[Tooltip("如果此项为真，只有当该反馈到范围中心（RangeCenter）的距离小于或等于范围距离（RangeDistance）时，此反馈才会播放。")]
		public bool OnlyPlayIfWithinRange = false;
		/// when in OnlyPlayIfWithinRange mode, the transform to consider as the center of the range
		[Tooltip("在“仅在范围内播放”模式下，将被视为范围中心的变换。")]
		public Transform RangeCenter;
		/// when in OnlyPlayIfWithinRange mode, the distance to the center within which the feedback will play
		[Tooltip("在“仅在范围内播放”模式下，到反馈将会播放的那个中心的距离。")]
		public float RangeDistance = 5f;
		/// when in OnlyPlayIfWithinRange mode, whether or not to modify the intensity of feedbacks based on the RangeFallOff curve  
		[Tooltip("在“仅在范围内播放”模式下，是否基于范围衰减（RangeFallOff）曲线来修改反馈的强度。")]
		public bool UseRangeFalloff = false;
		/// the animation curve to use to define falloff (on the x 0 represents the range center, 1 represents the max distance to it)
		[Tooltip("用于定义衰减的动画曲线（在该曲线上，x轴的0代表范围中心，1代表到范围中心的最大距离）。")]
		[MMFCondition("UseRangeFalloff", true)]
		public AnimationCurve RangeFalloff = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
		/// the values to remap the falloff curve's y axis' 0 and 1
		[Tooltip("用于重新映射衰减曲线y轴上的0和1所对应的值。")]
		[MMFVector("Zero","One")]
		public Vector2 RemapRangeFalloff = new Vector2(0f, 1f);
		/// whether or not to ignore MMSetFeedbackRangeCenterEvent, used to set the RangeCenter from anywhere
		[Tooltip("是否忽略“MM设置反馈范围中心事件（MMSetFeedbackRangeCenterEvent）”，该事件可用于从任意位置设置范围中心。")]
		public bool IgnoreRangeEvents = false;

		/// a duration, in seconds, during which triggering a new play of this MMFeedbacks after it's been played once will be impossible
		[Tooltip("一个以秒为单位的持续时间，在此期间，在该“MM反馈（MMFeedbacks）”已经播放过一次之后，触发它再次播放将是不可能的。 ")]
		public float CooldownDuration = 0f;
		/// a duration, in seconds, to delay the start of this MMFeedbacks' contents play
		[Tooltip("一个以秒为单位的持续时间，用于延迟此“MM反馈（MMFeedbacks）”内容播放的开始时间。")]
		public float InitialDelay = 0f;
		/// whether this player can be played or not, useful to temporarily prevent play from another class, for example
		[Tooltip("此播放器是否可以播放，例如，这对于临时阻止来自其他类的播放很有用处。")]
		public bool CanPlay = true;
		/// if this is true, you'll be able to trigger a new Play while this feedback is already playing, otherwise you won't be able to
		[Tooltip("如果此项为真，那么当该反馈已经在播放时，你将能够触发一次新的播放操作，否则将无法触发。")]
		public bool CanPlayWhileAlreadyPlaying = true;
		/// the chance of this sequence happening (in percent : 100 : happens all the time, 0 : never happens, 50 : happens once every two calls, etc)
		[Tooltip("此序列发生的概率（以百分比表示：100表示始终发生，0表示从不发生，50表示每两次调用发生一次，依此类推）。")]
		[Range(0,100)]
		public float ChanceToPlay = 100f;
        
		/// the intensity at which to play this feedback. That value will be used by most feedbacks to tune their amplitude. 1 is normal, 0.5 is half power, 0 is no effect.
		/// Note that what this value controls depends from feedback to feedback, don't hesitate to check the code to see what it does exactly.  
		[Tooltip("播放此反馈时的强度。大多数反馈会使用该值来调节其幅度。1表示正常强度，0.5表示半功率强度，0表示无效果。"
				+"请注意，此值所控制的内容因不同反馈而异，如有需要，请查看代码以确切了解其具体作用。 ")]
		public float FeedbacksIntensity = 1f;

		/// a number of UnityEvents that can be triggered at the various stages of this MMFeedbacks 
		[Tooltip("在这个“MM反馈（MMFeedbacks）”的各个阶段可以触发的若干个Unity事件")] 
		public MMFeedbacksEvents Events;
        
		/// a global switch used to turn all feedbacks on or off globally
		[Tooltip("一个全局开关，用于全局性地打开或关闭所有反馈。")]
		public static bool GlobalMMFeedbacksActive = true;
        
		[HideInInspector]
		/// whether or not this MMFeedbacks is in debug mode
		public bool DebugActive = false;
		/// whether or not this MMFeedbacks is playing right now - meaning it hasn't been stopped yet.
		/// if you don't stop your MMFeedbacks it'll remain true of course
		public bool IsPlaying { get; protected set; }
		/// if this MMFeedbacks is playing the time since it started playing
		public virtual float ElapsedTime => IsPlaying ? GetTime() - _lastStartAt : 0f;
		/// the amount of times this MMFeedbacks has been played
		public int TimesPlayed { get; protected set; }
		/// whether or not the execution of this MMFeedbacks' sequence is being prevented and waiting for a Resume() call
		public bool InScriptDrivenPause { get; set; }
		/// true if this MMFeedbacks contains at least one loop
		public bool ContainsLoop { get; set; }
		/// true if this feedback should change play direction next time it's played
		public bool ShouldRevertOnNextPlay { get; set; }
		/// true if this player is forcing unscaled mode
		public bool ForcingUnscaledTimescaleMode { get { return (ForceTimescaleMode && ForcedTimescaleMode == TimescaleModes.Unscaled);  } }
		/// The total duration (in seconds) of all the active feedbacks in this MMFeedbacks
		public virtual float TotalDuration
		{
			get
			{
				float total = 0f;
				foreach (MMFeedback feedback in Feedbacks)
				{
					if ((feedback != null) && (feedback.Active))
					{
						if (total < feedback.TotalDuration)
						{
							total = feedback.TotalDuration;    
						}
					}
				}
				return ComputedInitialDelay + total;
			}
		}
        
		public virtual float GetTime() { return (PlayerTimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (PlayerTimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
		public virtual float ComputedInitialDelay => ApplyTimeMultiplier(InitialDelay);
		
		protected float _startTime = 0f;
		protected float _holdingMax = 0f;
		protected float _lastStartAt = -float.MaxValue;
		protected int _lastStartFrame = -1;
		protected bool _pauseFound = false;
		protected float _totalDuration = 0f;
		protected bool _shouldStop = false;
		protected const float _smallValue = 0.001f;
		protected float _randomDurationMultiplier = 1f;
		protected float _lastOnEnableFrame = -1;

		#region INITIALIZATION

		/// <summary>
		/// On Awake we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected virtual void Awake()
		{
			// if our MMFeedbacks is in AutoPlayOnEnable mode, we add a little helper to it that will re-enable it if needed if the parent game object gets turned off and on again
			if (AutoPlayOnEnable)
			{
				MMFeedbacksEnabler enabler = GetComponent<MMFeedbacksEnabler>(); 
				if (enabler == null)
				{
					enabler = this.gameObject.AddComponent<MMFeedbacksEnabler>();
				}
				enabler.TargetMMFeedbacks = this;
			}
            
			if ((InitializationMode == InitializationModes.Awake) && (Application.isPlaying))
			{
				Initialization(this.gameObject);
			}
			CheckForLoops();
		}

		/// <summary>
		/// On Start we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected virtual void Start()
		{
			if ((InitializationMode == InitializationModes.Start) && (Application.isPlaying))
			{
				Initialization(this.gameObject);
			}
			if (AutoPlayOnStart && Application.isPlaying)
			{
				PlayFeedbacks();
			}
			CheckForLoops();
		}

		/// <summary>
		/// On Enable we initialize our feedbacks if we're in auto mode
		/// </summary>
		protected virtual void OnEnable()
		{
			if (AutoPlayOnEnable && Application.isPlaying)
			{
				PlayFeedbacks();
			}
		}

		/// <summary>
		/// Initializes the MMFeedbacks, setting this MMFeedbacks as the owner
		/// </summary>
		public virtual void Initialization(bool forceInitIfPlaying = false)
		{
			Initialization(this.gameObject);
		}

		/// <summary>
		/// A public method to initialize the feedback, specifying an owner that will be used as the reference for position and hierarchy by feedbacks
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="feedbacksOwner"></param>
		public virtual void Initialization(GameObject owner)
		{
			if ((SafeMode == MMFeedbacks.SafeModes.RuntimeOnly) || (SafeMode == MMFeedbacks.SafeModes.Full))
			{
				AutoRepair();
			}

			IsPlaying = false;
			TimesPlayed = 0;
			_lastStartAt = -float.MaxValue;

			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					Feedbacks[i].Initialization(owner);
				}                
			}
		}

		#endregion

		#region PLAY
        
		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation
		/// </summary>
		public virtual void PlayFeedbacks()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity);
		}
        
		/// <summary>
		/// Plays all feedbacks and awaits until completion
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="forceRevert"></param>
		public virtual async System.Threading.Tasks.Task PlayFeedbacksTask(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			while (IsPlaying)
			{
				await System.Threading.Tasks.Task.Yield();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks and awaits until completion
		/// </summary>
		public virtual async System.Threading.Tasks.Task PlayFeedbacksTask()
		{
			PlayFeedbacks();
			while (IsPlaying)
			{
				await System.Threading.Tasks.Task.Yield();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks, specifying a position and intensity. The position may be used by each Feedback and taken into account to spark a particle or play a sound for example.
		/// The feedbacks intensity is a factor that can be used by each Feedback to lower its intensity, usually you'll want to define that attenuation based on time or distance (using a lower 
		/// intensity value for feedbacks happening further away from the Player).
		/// Additionally you can force the feedback to play in reverse, ignoring its current condition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksOwner"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void PlayFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
		/// </summary>
		public virtual void PlayFeedbacksInReverse()
		{
			PlayFeedbacksInternal(this.transform.position, FeedbacksIntensity, true);
		}

		/// <summary>
		/// Plays all feedbacks using the MMFeedbacks' position as reference, and no attenuation, and in reverse (from bottom to top)
		/// </summary>
		public virtual void PlayFeedbacksInReverse(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacksInternal(position, feedbacksIntensity, forceRevert);
		}

		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfReversed()
		{
            
			if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
			     || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
			{
				PlayFeedbacks();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in reverse order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfReversed(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
            
			if ( (Direction == Directions.BottomToTop && !ShouldRevertOnNextPlay)
			     || ((Direction == Directions.TopToBottom) && ShouldRevertOnNextPlay) )
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}
        
		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfNormalDirection()
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks();
			}
		}
        
		/// <summary>
		/// Plays all feedbacks in the sequence, but only if this MMFeedbacks is playing in normal order
		/// </summary>
		public virtual void PlayFeedbacksOnlyIfNormalDirection(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			if (Direction == Directions.TopToBottom)
			{
				PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			}
		}

		/// <summary>
		/// A public coroutine you can call externally when you want to yield in a coroutine of yours until the MMFeedbacks has stopped playing
		/// typically : yield return myFeedback.PlayFeedbacksCoroutine(this.transform.position, 1.0f, false);
		/// </summary>
		/// <param name="position">The position at which the MMFeedbacks should play</param>
		/// <param name="feedbacksIntensity">The intensity of the feedback</param>
		/// <param name="forceRevert">Whether or not the MMFeedbacks should play in reverse or not</param>
		/// <returns></returns>
		public virtual IEnumerator PlayFeedbacksCoroutine(Vector3 position, float feedbacksIntensity = 1.0f, bool forceRevert = false)
		{
			PlayFeedbacks(position, feedbacksIntensity, forceRevert);
			while (IsPlaying)
			{
				yield return null;    
			}
		}

		#endregion

		#region SEQUENCE

		/// <summary>
		/// An internal method used to play feedbacks, shouldn't be called externally
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected virtual void PlayFeedbacksInternal(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			if (!CanPlay)
			{
				return;
			}
			
			if (IsPlaying && !CanPlayWhileAlreadyPlaying)
			{
				return;
			}

			if (!EvaluateChance())
			{
				return;
			}

			// if we have a cooldown we prevent execution if needed
			if (CooldownDuration > 0f)
			{
				if (GetTime() - _lastStartAt < CooldownDuration)
				{
					return;
				}
			}

			// if all MMFeedbacks are disabled globally, we stop and don't play
			if (!GlobalMMFeedbacksActive)
			{
				return;
			}

			if (!this.gameObject.activeInHierarchy)
			{
				return;
			}
            
			if (ShouldRevertOnNextPlay)
			{
				Revert();
				ShouldRevertOnNextPlay = false;
			}

			if (forceRevert)
			{
				Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
			}
            
			ResetFeedbacks();
			this.enabled = true;
			TimesPlayed++;
			IsPlaying = true;
			_startTime = GetTime();
			_lastStartAt = _startTime;
			_totalDuration = TotalDuration;
			CheckForPauses();
            
			if (ComputedInitialDelay > 0f)
			{
				StartCoroutine(HandleInitialDelayCo(position, feedbacksIntensity, forceRevert));
			}
			else
			{
				PreparePlay(position, feedbacksIntensity, forceRevert);
			}
		}

		protected virtual void PreparePlay(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			Events.TriggerOnPlay(this);

			_holdingMax = 0f;
			CheckForPauses();
			
			if (!_pauseFound)
			{
				PlayAllFeedbacks(position, feedbacksIntensity, forceRevert);
			}
			else
			{
				// if at least one pause was found
				StartCoroutine(PausedFeedbacksCo(position, feedbacksIntensity));
			}
		}

		protected virtual void CheckForPauses()
		{
			_pauseFound = false;
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					if ((Feedbacks[i].Pause != null) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}
					if ((Feedbacks[i].HoldingPause == true) && (Feedbacks[i].Active) && (Feedbacks[i].ShouldPlayInThisSequenceDirection))
					{
						_pauseFound = true;
					}    
				}
			}
		}

		protected virtual void PlayAllFeedbacks(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			// if no pause was found, we just play all feedbacks at once
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (FeedbackCanPlay(Feedbacks[i]))
				{
					Feedbacks[i].Play(position, feedbacksIntensity);
				}
			}
		}

		protected virtual IEnumerator HandleInitialDelayCo(Vector3 position, float feedbacksIntensity, bool forceRevert = false)
		{
			IsPlaying = true;
			yield return MMFeedbacksCoroutine.WaitFor(ComputedInitialDelay);
			PreparePlay(position, feedbacksIntensity, forceRevert);
		}
        
		protected virtual void Update()
		{
			if (_shouldStop)
			{
				if (HasFeedbackStillPlaying())
				{
					return;
				}
				IsPlaying = false;
				Events.TriggerOnComplete(this);
				ApplyAutoRevert();
				this.enabled = false;
				_shouldStop = false;
			}
			if (IsPlaying)
			{
				if (!_pauseFound)
				{
					if (GetTime() - _startTime > _totalDuration)
					{
						_shouldStop = true;
					}
				}
			}
			else
			{
				this.enabled = false;
			}
		}

		/// <summary>
		/// Returns true if feedbacks are still playing
		/// </summary>
		/// <returns></returns>
		public virtual bool HasFeedbackStillPlaying()
		{
			int count = Feedbacks.Count;
			for (int i = 0; i < count; i++)
			{
				if ((Feedbacks[i] != null) && (Feedbacks[i].IsPlaying))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// A coroutine used to handle the sequence of feedbacks if pauses are involved
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <returns></returns>
		protected virtual IEnumerator PausedFeedbacksCo(Vector3 position, float feedbacksIntensity)
		{
			yield return null;
		}

		#endregion

		#region STOP

		/// <summary>
		/// Stops all further feedbacks from playing, without stopping individual feedbacks 
		/// </summary>
		public virtual void StopFeedbacks()
		{
			StopFeedbacks(true);
		}

		/// <summary>
		/// Stops all feedbacks from playing, with an option to also stop individual feedbacks
		/// </summary>
		public virtual void StopFeedbacks(bool stopAllFeedbacks = true)
		{
			StopFeedbacks(this.transform.position, 1.0f, stopAllFeedbacks);
		}

		/// <summary>
		/// Stops all feedbacks from playing, specifying a position and intensity that can be used by the Feedbacks 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		public virtual void StopFeedbacks(Vector3 position, float feedbacksIntensity = 1.0f, bool stopAllFeedbacks = true)
		{
			if (stopAllFeedbacks)
			{
				for (int i = 0; i < Feedbacks.Count; i++)
				{
					if (Feedbacks[i] != null)
					{
						Feedbacks[i].Stop(position, feedbacksIntensity);	
					}
				}    
			}
			IsPlaying = false;
			StopAllCoroutines();
		}
        
		#endregion 

		#region CONTROLS

		/// <summary>
		/// Calls each feedback's Reset method if they've defined one. An example of that can be resetting the initial color of a flickering renderer.
		/// </summary>
		public virtual void ResetFeedbacks()
		{
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if ((Feedbacks[i] != null) && (Feedbacks[i].Active))
				{
					Feedbacks[i].ResetFeedback();    
				}
			}
			IsPlaying = false;
		}

		/// <summary>
		/// Changes the direction of this MMFeedbacks
		/// </summary>
		public virtual void Revert()
		{
			Events.TriggerOnRevert(this);
			Direction = (Direction == Directions.BottomToTop) ? Directions.TopToBottom : Directions.BottomToTop;
		}

		/// <summary>
		/// Use this method to authorize or prevent this player from being played
		/// </summary>
		/// <param name="newState"></param>
		public virtual void SetCanPlay(bool newState)
		{
			CanPlay = newState;
		}

		/// <summary>
		/// Pauses execution of a sequence, which can then be resumed by calling ResumeFeedbacks()
		/// </summary>
		public virtual void PauseFeedbacks()
		{
			Events.TriggerOnPause(this);
			InScriptDrivenPause = true;
		}

		/// <summary>
		/// Resumes execution of a sequence if a script driven pause is in progress
		/// </summary>
		public virtual void ResumeFeedbacks()
		{
			Events.TriggerOnResume(this);
			InScriptDrivenPause = false;
		}

		#endregion
        
		#region MODIFICATION
        
		public virtual MMFeedback AddFeedback(System.Type feedbackType, bool add = true)
		{
			MMFeedback newFeedback;
            
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				newFeedback = Undo.AddComponent(this.gameObject, feedbackType) as MMFeedback;
			}
			else
			{
				newFeedback = this.gameObject.AddComponent(feedbackType) as MMFeedback;
			}
			#else 
                newFeedback = this.gameObject.AddComponent(feedbackType) as MMFeedback;
			#endif
            
			newFeedback.hideFlags = HideFlags.HideInInspector;
			newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(feedbackType);

			AutoRepair();
            
			return newFeedback;
		}
        
		public virtual void RemoveFeedback(int id)
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Undo.DestroyObjectImmediate(Feedbacks[id]);
			}
			else
			{
				DestroyImmediate(Feedbacks[id]);
			}
			#else
                DestroyImmediate(Feedbacks[id]);
			#endif
            
			Feedbacks.RemoveAt(id);
			AutoRepair();
		}

        #endregion MODIFICATION

        #region HELPERS

        /// <summary>
        /// Evaluates the chance of this feedback to play, and returns true if this feedback can play, false otherwise 评估此反馈播放的机会，如果此反馈可以播放则返回true，否则返回false
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateChance()
		{
			if (ChanceToPlay == 0f)
			{
				return false;
			}
			if (ChanceToPlay != 100f)
			{
				// determine the odds
				float random = Random.Range(0f, 100f);
				if (random > ChanceToPlay)
				{
					return false;
				}
			}

			return true;
		}
        
		/// <summary>
		/// Checks whether or not this MMFeedbacks contains one or more looper feedbacks
		/// </summary>
		protected virtual void CheckForLoops()
		{
			ContainsLoop = false;
			for (int i = 0; i < Feedbacks.Count; i++)
			{
				if (Feedbacks[i] != null)
				{
					if (Feedbacks[i].LooperPause && Feedbacks[i].Active)
					{
						ContainsLoop = true;
						return;
					}
				}                
			}
		}
        
		/// <summary>
		/// This will return true if the conditions defined in the specified feedback's Timing section allow it to play in the current play direction of this MMFeedbacks
		/// </summary>
		/// <param name="feedback"></param>
		/// <returns></returns>
		protected bool FeedbackCanPlay(MMFeedback feedback)
		{
			if (feedback == null)
			{
				return false;
			}
			
			if (feedback.Timing == null)
			{
				return false;
			}
			
			if (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.Always)
			{
				return true;
			}
			else if (((Direction == Directions.TopToBottom) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenForwards))
			         || ((Direction == Directions.BottomToTop) && (feedback.Timing.MMFeedbacksDirectionCondition == MMFeedbackTiming.MMFeedbacksDirectionConditions.OnlyWhenBackwards)))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Readies the MMFeedbacks to revert direction on the next play
		/// </summary>
		protected virtual void ApplyAutoRevert()
		{
			if (AutoChangeDirectionOnEnd)
			{
				ShouldRevertOnNextPlay = true;
			}
		}
        
		/// <summary>
		/// Applies this feedback's time multiplier to a duration (in seconds)
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public virtual float ApplyTimeMultiplier(float duration)
		{
			return duration * Mathf.Clamp(DurationMultiplier, _smallValue, Single.MaxValue);
		}

		/// <summary>
		/// Unity sometimes has serialization issues. 
		/// This method fixes that by fixing any bad sync that could happen.
		/// </summary>
		public virtual void AutoRepair()
		{
			List<Component> components = components = new List<Component>();
			components = this.gameObject.GetComponents<Component>().ToList();
			foreach (Component component in components)
			{
				if (component is MMFeedback)
				{
					bool found = false;
					for (int i = 0; i < Feedbacks.Count; i++)
					{
						if (Feedbacks[i] == (MMFeedback)component)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						Feedbacks.Add((MMFeedback)component);
					}
				}
			}
		} 

		#endregion 
        
		#region EVENTS

		/// <summary>
		/// On Disable we stop all feedbacks
		/// </summary>
		protected virtual void OnDisable()
		{
			/*if (IsPlaying)
			{
			    StopFeedbacks();
			    StopAllCoroutines();
			}*/
		}

		/// <summary>
		/// On validate, we make sure our DurationMultiplier remains positive
		/// </summary>
		protected virtual void OnValidate()
		{
			DurationMultiplier = Mathf.Clamp(DurationMultiplier, _smallValue, Single.MaxValue);
		}

		/// <summary>
		/// On Destroy, removes all feedbacks from this MMFeedbacks to avoid any leftovers
		/// </summary>
		protected virtual void OnDestroy()
		{
			IsPlaying = false;
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{            
				// we remove all binders
				foreach (MMFeedback feedback in Feedbacks)
				{
					EditorApplication.delayCall += () =>
					{
						DestroyImmediate(feedback);
					};                    
				}
			}
			#endif
		}     
        
		#endregion EVENTS
	}
}