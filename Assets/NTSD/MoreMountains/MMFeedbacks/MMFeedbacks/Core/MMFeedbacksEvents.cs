using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace  MoreMountains.Feedbacks
{
	/// <summary>
	/// Events triggered by a MMFeedbacks when playing a series of feedbacks
	/// - play : when a MMFeedbacks starts playing
	/// - pause : when a holding pause is met
	/// - resume : after a holding pause resumes
	/// - revert : when a MMFeedbacks reverts its play direction
	/// - complete : when a MMFeedbacks has played its last feedback
	///
	/// to listen to these events :
	///
	/// public virtual void OnMMFeedbacksEvent(MMFeedbacks source, EventTypes type)
	/// {
	///     // do something
	/// }
	/// 
	/// protected virtual void OnEnable()
	/// {
	///     MMFeedbacksEvent.Register(OnMMFeedbacksEvent);
	/// }
	/// 
	/// protected virtual void OnDisable()
	/// {
	///     MMFeedbacksEvent.Unregister(OnMMFeedbacksEvent);
	/// }
	/// 
	/// </summary>
	public struct MMFeedbacksEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public enum EventTypes { Play, Pause, Resume, Revert, Complete, SkipToTheEnd, RestoreInitialValues, Loop, Enable, Disable, InitializationComplete }
		public delegate void Delegate(MMFeedbacks source, EventTypes type);
		static public void Trigger(MMFeedbacks source, EventTypes type)
		{
			OnEvent?.Invoke(source, type);
		}
	}
	
	/// <summary>
	/// An event used to set the RangeCenter on all feedbacks that listen for it
	/// </summary>
	public struct MMSetFeedbackRangeCenterEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }
		
		public delegate void Delegate(Transform newCenter);

		static public void Trigger(Transform newCenter)
		{
			OnEvent?.Invoke(newCenter);
		}
	}
	
	/// <summary>
	/// A subclass of MMFeedbacks, contains UnityEvents that can be played, 
	/// </summary>
	[Serializable]
	public class MMFeedbacksEvents
	{
        /// 是否应该触发 MMFeedbacks 事件
        [Tooltip("是否应该触发 MMFeedbacks 事件")]
        public bool TriggerMMFeedbacksEvents = false;
        /// 是否应该触发 Unity 事件
        [Tooltip("是否应该触发 Unity 事件")]
        public bool TriggerUnityEvents = true;
        /// 每当此 MMFeedbacks 被播放时，此事件将被触发
        [Tooltip("每当此 MMFeedbacks 被播放时，此事件将被触发")]
        public UnityEvent OnPlay;
        /// 每当此 MMFeedbacks 开始保持暂停时，此事件将被触发
        [Tooltip("每当此 MMFeedbacks 开始保持暂停时，此事件将被触发")]
        public UnityEvent OnPause;
        /// 每当此 MMFeedbacks 在保持暂停后恢复时，此事件将被触发
        [Tooltip("每当此 MMFeedbacks 在保持暂停后恢复时，此事件将被触发")]
        public UnityEvent OnResume;
        /// 每当此 MMFeedbacks 反转其播放方向时，此事件将被触发
        [Tooltip("每当此 MMFeedbacks 反转其播放方向时，此事件将被触发")]
        public UnityEvent OnRevert;
        /// 每当此 MMFeedbacks 播放其最后一个 MMFeedback 时，此事件将被触发
        [Tooltip("每当此 MMFeedbacks 播放其最后一个 MMFeedback 时，此事件将被触发")]
        public UnityEvent OnComplete;
        /// 每当此 MMFeedbacks 恢复到其初始值时，此事件将被触发
        [Tooltip("每当此 MMFeedbacks 恢复到其初始值时，此事件将被触发")]
        public UnityEvent OnRestoreInitialValues;
        /// 每当此 MMFeedbacks 跳转到末尾时，此事件将被触发
        [Tooltip("每当此 MMFeedbacks 跳转到末尾时，此事件将被触发")]
        public UnityEvent OnSkipToTheEnd;
        /// MMF 播放器完成初始化后，此事件将被触发
        [Tooltip("MMF 播放器完成初始化后，此事件将被触发")]
        public UnityEvent OnInitializationComplete;
        /// 此 MMFeedbacks 的游戏对象每次被启用时，此事件将被触发
        [Tooltip("此 MMFeedbacks 的游戏对象每次被启用时，此事件将被触发")]
        public UnityEvent OnEnable;
        /// 此 MMFeedbacks 的游戏对象每次被禁用时，此事件将被触发
        [Tooltip("此 MMFeedbacks 的游戏对象每次被禁用时，此事件将被触发")]
        public UnityEvent OnDisable;


        public virtual bool OnPlayIsNull { get; protected set; }
		public virtual bool OnPauseIsNull { get; protected set; }
		public virtual bool OnResumeIsNull { get; protected set; }
		public virtual bool OnRevertIsNull { get; protected set; }
		public virtual bool OnCompleteIsNull { get; protected set; }
		public virtual bool OnRestoreInitialValuesIsNull { get; protected set; }
		public virtual bool OnSkipToTheEndIsNull { get; protected set; }
		public virtual bool OnInitializationCompleteIsNull { get; protected set; }
		public virtual bool OnEnableIsNull { get; protected set; }
		public virtual bool OnDisableIsNull { get; protected set; }

		/// <summary>
		/// On init we store for each event whether or not we have one to invoke
		/// </summary>
		public virtual void Initialization()
		{
			OnPlayIsNull = OnPlay == null;
			OnPauseIsNull = OnPause == null;
			OnResumeIsNull = OnResume == null;
			OnRevertIsNull = OnRevert == null;
			OnCompleteIsNull = OnComplete == null;
			OnRestoreInitialValuesIsNull = OnRestoreInitialValues == null;
			OnSkipToTheEndIsNull = OnSkipToTheEnd == null;
			OnInitializationCompleteIsNull = OnInitializationComplete == null;
			OnEnableIsNull = OnEnable == null;
			OnDisableIsNull = OnDisable == null;
		}

		/// <summary>
		/// Fires Play events if needed
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnPlay(MMFeedbacks source)
		{
			if (!OnPlayIsNull && TriggerUnityEvents)
			{
				OnPlay.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Play);
			}
		}

		/// <summary>
		/// Fires pause events if needed
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnPause(MMFeedbacks source)
		{
			if (!OnPauseIsNull && TriggerUnityEvents)
			{
				OnPause.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Pause);
			}
		}

		/// <summary>
		/// Fires resume events if needed
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnResume(MMFeedbacks source)
		{
			if (!OnResumeIsNull && TriggerUnityEvents)
			{
				OnResume.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Resume);
			}
		}

		/// <summary>
		/// Fires revert events if needed
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnRevert(MMFeedbacks source)
		{
			if (!OnRevertIsNull && TriggerUnityEvents)
			{
				OnRevert.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Revert);
			}
		}

		/// <summary>
		/// Fires complete events if needed
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnComplete(MMFeedbacks source)
		{
			if (!OnCompleteIsNull && TriggerUnityEvents)
			{
				OnComplete.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Complete);
			}
		}

		/// <summary>
		/// Fires skip events if needed
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnSkipToTheEnd(MMFeedbacks source)
		{
			if (!OnSkipToTheEndIsNull && TriggerUnityEvents)
			{
				OnSkipToTheEnd.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.SkipToTheEnd);
			}
		}

		public virtual void TriggerOnInitializationComplete(MMFeedbacks source)
		{
			if (!OnInitializationCompleteIsNull && TriggerUnityEvents)
			{
				OnInitializationComplete.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.InitializationComplete);
			}
		}

		/// <summary>
		/// Fires revert events if needed
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnRestoreInitialValues(MMFeedbacks source)
		{
			if (!OnRestoreInitialValuesIsNull && TriggerUnityEvents)
			{
				OnRestoreInitialValues.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.RestoreInitialValues);
			}
		}

		/// <summary>
		/// Fires enable events if needed
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnEnable(MMF_Player source)
		{
			if (!OnEnableIsNull && TriggerUnityEvents)
			{
				OnEnable.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Enable);
			}
		}

		/// <summary>
		/// Fires disable events if needed
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnDisable(MMF_Player source)
		{
			if (!OnDisableIsNull && TriggerUnityEvents)
			{
				OnDisable.Invoke();
			}

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Disable);
			}
		}
	}
   
}