using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback to bind Unity events to and trigger them when played
	/// </summary>
	[AddComponentMenu("")]
    [FeedbackHelp("这个反馈允许你将任何类型的Unity事件绑定到这个反馈的Play（播放）、Stop（停止）、Initialization（初始化）和Reset（重置）方法。")]
    [MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Events/Unity Events")]
	public class MMF_Events : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.EventsColor; } }
		#endif

		[MMFInspectorGroup("Events", true, 44)]
        /// <summary>
        /// 当反馈播放时触发的事件
        /// </summary>
        [Tooltip("当反馈播放时触发的事件")]
        public UnityEvent PlayEvents;

        /// <summary>
        /// 当反馈停止时触发的事件
        /// </summary>
        [Tooltip("当反馈停止时触发的事件")]
        public UnityEvent StopEvents;

        /// <summary>
        /// 当反馈初始化时触发的事件
        /// </summary>
        [Tooltip("当反馈初始化时触发的事件")]
        public UnityEvent InitializationEvents;

        /// <summary>
        /// 当反馈重置时触发的事件
        /// </summary>
        [Tooltip("当反馈重置时触发的事件")]
        public UnityEvent ResetEvents;

        /// <summary>
        /// On init, triggers the init events
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (InitializationEvents != null))
			{
				InitializationEvents.Invoke();
			}
		}

		/// <summary>
		/// On Play, triggers the play events
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (PlayEvents == null))
			{
				return;
			}
			PlayEvents.Invoke();    
		}

		/// <summary>
		/// On Stop, triggers the stop events
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (StopEvents == null))
			{
				return;
			}
			StopEvents.Invoke();
		}

		/// <summary>
		/// On reset, triggers the reset events
		/// </summary>
		protected override void CustomReset()
		{
			if (!Active || !FeedbackTypeAuthorized || (ResetEvents == null))
			{
				return;
			}
			base.CustomReset();
			ResetEvents.Invoke();
		}
	}
}