using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback allows you to trigger a target MMFeedbacks, or any MMFeedbacks on the specified Channel within a certain range. You'll need an MMFeedbacksShaker on them.
	/// </summary>
	[AddComponentMenu("")]
    [FeedbackHelp("这个反馈允许你触发一个目标MMFeedbacks，或者在指定频道内一定范围内的任何MMFeedbacks。你需要在它们上面添加一个MMFeedbacksShaker。")]
    [MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Feedbacks/Feedbacks Player")]
	public class MMF_Feedbacks : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif
		/// the duration of this feedback is the duration of our target feedback
		public override float FeedbackDuration 
		{
			get
			{
				if (TargetFeedbacks == Owner)
				{
					return 0f;
				}
				if ((Mode == Modes.PlayTargetFeedbacks) && (TargetFeedbacks != null))
				{
					return TargetFeedbacks.TotalDuration;
				}
				else
				{
					return 0f;    
				}
			} 
		}
		public override bool HasChannel => true;
        
		public enum Modes { PlayFeedbacksInArea, PlayTargetFeedbacks }
        
		[MMFInspectorGroup("Feedbacks", true, 79)]

        /// the selected mode for this feedback
        [Tooltip("这个反馈选择的模式")]
        public Modes Mode = Modes.PlayFeedbacksInArea;

        /// a specific MMFeedbacks / MMF_Player to play
        [MMFEnumCondition("Mode", (int)Modes.PlayTargetFeedbacks)]
        [Tooltip("要播放的特定MMFeedbacks/MMF_Player")]
        public MMFeedbacks TargetFeedbacks;

        /// whether or not to use a range
        [MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
        [Tooltip("是否使用范围")]
        public bool OnlyTriggerPlayersInRange = false;
        /// the range of the event, in units
        [MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
        [Tooltip("事件的范围（单位）")]
        public float EventRange = 100f;
        /// the transform to use to broadcast the event as origin point
        [MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
        [Tooltip("用作广播事件起点的变换")]
        public Transform EventOriginTransform;

        /// <summary>
        /// On init we turn the light off if needed
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
            
			if (EventOriginTransform == null)
			{
				EventOriginTransform = owner.transform;
			}
		}

		/// <summary>
		/// On Play we trigger our target feedback or trigger a feedback shake event to shake feedbacks in the area
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (TargetFeedbacks == Owner)
			{
				return;
			}
			
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.PlayFeedbacksInArea)
			{
				MMFeedbacksShakeEvent.Trigger(ChannelData, OnlyTriggerPlayersInRange, EventRange, EventOriginTransform.position);    
			}
			else if (Mode == Modes.PlayTargetFeedbacks)
			{
				TargetFeedbacks?.PlayFeedbacks(position, feedbacksIntensity);
			}
		}
	}
}