using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// A feedback used to trigger an animation (bool, int, float or trigger) on the associated animator, with or without randomness
    /// 用于在相关动画器上触发动画（bool, int， float或trigger）的反馈，无论是否具有随机性
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("这个反馈将允许你将目标动画渐变到指定的状态。")]
    [MovedFrom(false, null, "MoreMountains.Feedbacks")]
    [FeedbackPath("Animation/Animation Crossfade")]
    public class MMF_AnimationCrossfade : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;

        /// the possible modes that pilot triggers        
        public enum TriggerModes { SetTrigger, ResetTrigger }

        /// the possible ways to set a value
        public enum ValueModes { None, Constant, Random, Incremental }

        /// sets the inspector color for this feedback
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.AnimationColor; } }
        public override bool EvaluateRequiresSetup() { return (BoundAnimator == null); }
        public override string RequiredTargetText { get { return BoundAnimator != null ? BoundAnimator.name : ""; } }
        public override string RequiresSetupText { get { return "这个反馈需要一个BoundAnimator被设置为能够正常工作。您可以在下面设置一个。"; } }
#endif

        /// the duration of this feedback is the declared duration 
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value; } }
        public override bool HasRandomness => true;
        public override bool HasAutomatedTargetAcquisition => true;
        protected override void AutomateTargetAcquisition() => BoundAnimator = FindAutomatedTarget<Animator>();

        public enum Modes { Seconds, Normalized }

        [MMFInspectorGroup("Animation", true, 12, true)]
        /// <summary>
        /// 你想要更新参数的动画器
        /// </summary>
        [Tooltip("你想要更新参数的动画器")]
        public Animator BoundAnimator;

        /// <summary>
        /// 你想要更新参数的额外动画器列表
        /// </summary>
        [Tooltip("你想要更新参数的额外动画器列表")]
        public List<Animator> ExtraBoundAnimators;

        /// the duration for the player to consider. This won't impact your animation, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual animation, and setting it can be useful to have this feedback work with holding pauses.
        [Tooltip("玩家需要考虑的持续时间。" +
            "“这不会影响你的动画，但是它是向MMF播放器传达这个反馈持续时间的一种方式。" +
            "通常你会希望它与你的实际动画相匹配，设置它可能有助于让这个反馈与保持暂停一起工作。”")]
        public float DeclaredDuration = 0f;

        [MMFInspectorGroup("CrossFade", true, 16)]

        /// <summary>
        /// 要过渡到的状态的名称。那是你动画器中黄色或灰色框的名称。
        /// </summary>
        [Tooltip("要过渡到的状态的名称。那是你动画器中黄色或灰色框的名称。")]
        public string StateName = "NewState";

        /// <summary>
        /// 你想要交叉淡入发生的动画层的ID
        /// </summary>
        [Tooltip("你想要交叉淡入发生的动画层的ID")]
        public int Layer = -1;

        /// <summary>
        /// 是否以秒为单位指定交叉淡入的时间数据，或者是以标准化（0-1）的值
        /// </summary>
        [Tooltip("是否以秒为单位指定交叉淡入的时间数据，或者是以标准化（0-1）的值")]
        public Modes Mode = Modes.Seconds;

        /// <summary>
        /// 在秒模式下，过渡的持续时间，以秒为单位
        /// </summary>
        [Tooltip("在秒模式下，过渡的持续时间，以秒为单位")]
        [MMFEnumCondition("Mode", (int)Modes.Seconds)]
        public float TransitionDuration = 0.1f;

        /// <summary>
        /// 在秒模式下，要过渡到的偏移量，以秒为单位
        /// </summary>
        [Tooltip("在秒模式下，要过渡到的偏移量，以秒为单位")]
        [MMFEnumCondition("Mode", (int)Modes.Seconds)]
        public float TimeOffset = 0f;

        /// <summary>
        /// 在标准化模式下，过渡的持续时间，标准化在0和1之间
        /// </summary>
        [Tooltip("在标准化模式下，过渡的持续时间，标准化在0和1之间")]
        [MMFEnumCondition("Mode", (int)Modes.Normalized)]
        public float NormalizedTransitionDuration = 0.1f;

        /// <summary>
        /// 在标准化模式下，要过渡到的偏移量，标准化在0和1之间
        /// </summary>
        [Tooltip("在标准化模式下，要过渡到的偏移量，标准化在0和1之间")]
        [MMFEnumCondition("Mode", (int)Modes.Normalized)]
        public float NormalizedTimeOffset = 0f;

        /// <summary>
        /// 根据Unity的文档，'过渡的时间，已标准化'。真的没人确定这个是做什么的。它是可选的。
        /// </summary>
        [Tooltip("根据Unity的文档，'过渡的时间，已标准化'。真的没人确定这个是做什么的。它是可选的。")]
        public float NormalizedTransitionTime = 0f;

        protected int _stateHashName;

        /// <summary>
        /// Custom Init
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
        {
            base.CustomInitialization(owner);
            _stateHashName = Animator.StringToHash(StateName);
        }

        /// <summary>
        /// On Play, checks if an animator is bound and crossfades to the specified state
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }

            if (BoundAnimator == null)
            {
                Debug.LogWarning("No animator was set for " + Owner.name);
                return;
            }

            CrossFade(BoundAnimator);
            foreach (Animator animator in ExtraBoundAnimators)
            {
                CrossFade(animator);
            }
        }

        /// <summary>
        /// Crossfades either via fixed time or regular (normalized) calls
        /// </summary>
        /// <param name="targetAnimator"></param>
        protected virtual void CrossFade(Animator targetAnimator)
        {
            switch (Mode)
            {
                case Modes.Seconds:
                    targetAnimator.CrossFadeInFixedTime(_stateHashName, TransitionDuration, Layer, TimeOffset, NormalizedTransitionTime);
                    break;
                case Modes.Normalized:
                    targetAnimator.CrossFade(_stateHashName, NormalizedTransitionDuration, Layer, NormalizedTimeOffset, NormalizedTransitionTime);
                    break;
            }
        }
    }
}