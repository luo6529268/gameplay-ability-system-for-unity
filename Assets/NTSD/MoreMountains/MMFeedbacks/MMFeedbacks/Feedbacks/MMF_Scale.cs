using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will animate the scale of the target object over time when played
    /// </summary>
    [AddComponentMenu("")]
    [MovedFrom(false, null, "MoreMountains.Feedbacks")]
    [FeedbackPath("Transform/Scale")]
    [FeedbackHelp("这段反馈将在指定的持续时间（秒）内，根据指定的三个动画曲线对目标的缩放进行动画处理。你可以应用一个乘数，该乘数将乘以每个动画曲线的值。")]
    public class MMF_Scale : MMF_Feedback
    {
        /// <summary>
        /// 一个静态布尔值，用于一次性禁用所有此类反馈
        /// </summary>
        public static bool FeedbackTypeAuthorized = true;

        /// <summary>
        /// 此反馈可以操作的可能模式
		/// Absolute：绝对
		/// Additive：累加
		/// ToDestination：到达目的地
        /// </summary>
        public enum Modes { Absolute, Additive, ToDestination }

        /// <summary>
        /// 缩放动画的可能时间尺度
		/// Scaled：缩放
		/// Unscaled：未缩放
        /// </summary>
        public enum TimeScales { Scaled, Unscaled }
        /// sets the inspector color for this feedback
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
        public override bool EvaluateRequiresSetup() { return (AnimateScaleTarget == null); }
        public override string RequiredTargetText { get { return AnimateScaleTarget != null ? AnimateScaleTarget.name : ""; } }
        public override string RequiresSetupText { get { return "这个反馈需要设置一个AnimateScaleTarget才能正常工作。你可以在下面设置一个。"; } }
        public override bool HasCustomInspectors { get { return true; } }
#endif
        public override bool HasAutomatedTargetAcquisition => true;
        public override bool CanForceInitialValue => true;
        protected override void AutomateTargetAcquisition() => AnimateScaleTarget = FindAutomatedTarget<Transform>();

        [MMFInspectorGroup("Scale Mode", true, 12, true)]
        /// the mode this feedback should operate on
        /// Absolute : follows the curve
        /// Additive : adds to the current scale of the target
        /// ToDestination : sets the scale to the destination target, whatever the current scale is
        [Tooltip("the mode this feedback should operate on" +
                 "Absolute : （绝对）：遵循曲线" +
                 "Additive : （累加）：加到目标的当前缩放上" +
                 "ToDestination : （到达目的地）：无论当前缩放如何，都将缩放设置为目的地目标")]
        public Modes Mode = Modes.Absolute;

        [Tooltip("累加状态下的最大缩放值")]
        [MMFEnumCondition("Mode", (int)Modes.Additive)]
        public float MaxScale;
        /// the object to animate
        [Tooltip("要动画化的物体")]
        public Transform AnimateScaleTarget;

        [MMFInspectorGroup("Scale Animation", true, 13)]
        /// <summary>
        /// 动画的持续时间
        /// </summary>
        [Tooltip("动画的持续时间")]
        public float AnimateScaleDuration = 0.2f;

        /// <summary>
        /// 将曲线的0值重新映射到的值
        /// </summary>
        [Tooltip("将曲线的0值重新映射到的值")]
        public float RemapCurveZero = 1f;

        /// <summary>
        /// 将曲线的1值重新映射到的值
        /// </summary>
        [Tooltip("将曲线的1值重新映射到的值")]
        [FormerlySerializedAs("Multiplier")]
        public float RemapCurveOne = 2f;

        /// <summary>
        /// 应该加到曲线上的值
        /// </summary>
        [Tooltip("应该加到曲线上的值")]
        public float Offset = 0f;

        /// <summary>
        /// 如果为真，应该动画化X轴缩放值
        /// </summary>
        [Tooltip("如果为真，应该动画化X轴缩放值")]
        public bool AnimateX = true;

        /// <summary>
        /// X轴缩放动画的定义
        /// </summary>
        [Tooltip("X轴缩放动画的定义")]
        [MMFCondition("AnimateX", true)]
        public MMTweenType AnimateScaleTweenX = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0)));
        // <summary>
        /// 如果为真，应该动画化Y轴缩放值
        /// </summary>
        [Tooltip("如果为真，应该动画化Y轴缩放值")]
        public bool AnimateY = true;

        /// <summary>
        /// Y轴缩放动画的定义
        /// </summary>
        [Tooltip("Y轴缩放动画的定义")]
        [MMFCondition("AnimateY", true)]
        public MMTweenType AnimateScaleTweenY = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0)));
        /// <summary>
        /// 如果为真，应该动画化Z轴缩放值
        /// </summary>
        [Tooltip("如果为真，应该动画化Z轴缩放值")]
        public bool AnimateZ = true;

        /// <summary>
        /// Z轴缩放动画的定义
        /// </summary>
        [Tooltip("Z轴缩放动画的定义")]
        [MMFCondition("AnimateZ", true)]
        public MMTweenType AnimateScaleTweenZ = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0)));
        /// <summary>
        /// 如果为真，仅使用AnimateX曲线，并将其应用于所有轴
        /// </summary>
        [Tooltip("如果为真，仅使用AnimateX曲线，并将其应用于所有轴")]
        public bool UniformScaling = false;

        /// <summary>
        /// 如果为真，即使正在进行中，调用该反馈也会触发它。如果为假，它将阻止任何新的播放，直到当前的播放结束
        /// </summary>
        [Tooltip("如果为真，即使正在进行中，调用该反馈也会触发它。如果为假，它将阻止任何新的播放，直到当前的播放结束")]
        public bool AllowAdditivePlays = false;

        /// <summary>
        /// 如果为真，每次播放时都会重新计算初始和目标缩放
        /// </summary>
        [Tooltip("如果为真，每次播放时都会重新计算初始和目标缩放")]
        public bool DetermineScaleOnPlay = false;

        /// <summary>
        /// 当处于ToDestination模式时，要达到的缩放
        /// </summary>
        [Tooltip("当处于ToDestination模式时，要达到的缩放")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public Vector3 DestinationScale = new Vector3(0.5f, 0.5f, 0.5f);

        /// the duration of this feedback is the duration of the scale animation
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateScaleDuration); } set { AnimateScaleDuration = value; } }
        public override bool HasRandomness => true;

        /// [DEPRECATED] the x scale animation definition
        [HideInInspector] public AnimationCurve AnimateScaleX = null;
        /// [DEPRECATED] the y scale animation definition
        [HideInInspector] public AnimationCurve AnimateScaleY = null;
        /// [DEPRECATED] the z scale animation definition
        [HideInInspector] public AnimationCurve AnimateScaleZ = null;

        protected Vector3 _initialScale;
        protected Vector3 _newScale;
        protected Coroutine _coroutine;

        /// <summary>
        /// On init we store our initial scale
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
        {
            base.CustomInitialization(owner);
            if (Active && (AnimateScaleTarget != null))
            {
                GetInitialScale();
            }
        }

        /// <summary>
        /// Stores initial scale for future use
        /// </summary>
        protected virtual void GetInitialScale()
        {
            _initialScale = AnimateScaleTarget.localScale;
        }

        /// <summary>
        /// On Play, triggers the scale animation
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized || (AnimateScaleTarget == null))
            {
                return;
            }

            if (DetermineScaleOnPlay && NormalPlayDirection)
            {
                GetInitialScale();
            }

            float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
            if (Active || Owner.AutoPlayOnEnable)
            {
                if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
                {
                    if (!AllowAdditivePlays && (_coroutine != null))
                    {
                        return;
                    }
                    if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
                    _coroutine = Owner.StartCoroutine(AnimateScale(AnimateScaleTarget, Vector3.zero, FeedbackDuration, AnimateScaleTweenX, AnimateScaleTweenY, AnimateScaleTweenZ, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
                }
                if (Mode == Modes.ToDestination)
                {
                    if (!AllowAdditivePlays && (_coroutine != null))
                    {
                        return;
                    }
                    if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
                    _coroutine = Owner.StartCoroutine(ScaleToDestination());
                }
            }
        }

        /// <summary>
        /// An internal coroutine used to scale the target to its destination scale
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ScaleToDestination()
        {
            if (AnimateScaleTarget == null)
            {
                yield break;
            }

            if ((AnimateScaleTweenX == null) || (AnimateScaleTweenY == null) || (AnimateScaleTweenZ == null))
            {
                yield break;
            }

            if (FeedbackDuration == 0f)
            {
                yield break;
            }

            float journey = NormalPlayDirection ? 0f : FeedbackDuration;

            _initialScale = AnimateScaleTarget.localScale;
            _newScale = _initialScale;
            IsPlaying = true;
            while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
            {
                float percent = Mathf.Clamp01(journey / FeedbackDuration);

                if (AnimateX)
                {
                    _newScale.x = Mathf.LerpUnclamped(_initialScale.x, DestinationScale.x, AnimateScaleTweenX.Evaluate(percent) + Offset);
                    _newScale.x = MMFeedbacksHelpers.Remap(_newScale.x, 0f, 1f, RemapCurveZero, RemapCurveOne);
                    _newScale.x = Mathf.Min(MaxScale, _newScale.x);
                }

                if (AnimateY)
                {
                    _newScale.y = Mathf.LerpUnclamped(_initialScale.y, DestinationScale.y, AnimateScaleTweenY.Evaluate(percent) + Offset);
                    _newScale.y = MMFeedbacksHelpers.Remap(_newScale.y, 0f, 1f, RemapCurveZero, RemapCurveOne);
                    _newScale.y = Mathf.Min(MaxScale, _newScale.y);
                }

                if (AnimateZ)
                {
                    _newScale.z = Mathf.LerpUnclamped(_initialScale.z, DestinationScale.z, AnimateScaleTweenZ.Evaluate(percent) + Offset);
                    _newScale.z = MMFeedbacksHelpers.Remap(_newScale.z, 0f, 1f, RemapCurveZero, RemapCurveOne);
                    _newScale.z = Mathf.Min(MaxScale, _newScale.z);
                }

                if (UniformScaling)
                {
                    _newScale.y = Mathf.Min(MaxScale, _newScale.x);
                    _newScale.z = Mathf.Min(MaxScale, _newScale.x);
                }

                AnimateScaleTarget.localScale = _newScale;

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;

                yield return null;
            }

            AnimateScaleTarget.localScale = NormalPlayDirection ? DestinationScale : _initialScale;
            _coroutine = null;
            IsPlaying = false;
            yield return null;
        }

        /// <summary>
        /// An internal coroutine used to animate the scale over time
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="vector"></param>
        /// <param name="duration"></param>
        /// <param name="curveX"></param>
        /// <param name="curveY"></param>
        /// <param name="curveZ"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        protected virtual IEnumerator AnimateScale(Transform targetTransform, Vector3 vector, float duration, MMTweenType curveX, MMTweenType curveY, MMTweenType curveZ, float remapCurveZero = 0f, float remapCurveOne = 1f)
        {
            if (targetTransform == null)
            {
                yield break;
            }

            if ((curveX == null) || (curveY == null) || (curveZ == null))
            {
                yield break;
            }

            if (duration == 0f)
            {
                yield break;
            }

            float journey = NormalPlayDirection ? 0f : duration;

            _initialScale = targetTransform.localScale;

            IsPlaying = true;

            while ((journey >= 0) && (journey <= duration) && (duration > 0))
            {
                vector = Vector3.zero;
                float percent = Mathf.Clamp01(journey / duration);

                if (AnimateX)
                {
                    vector.x = AnimateX ? curveX.Evaluate(percent) + Offset : targetTransform.localScale.x;
                    vector.x = MMFeedbacksHelpers.Remap(vector.x, 0f, 1f, remapCurveZero, remapCurveOne);
                    if (Mode == Modes.Additive)
                    {
                        vector.x += _initialScale.x;
                    }
                }
                else
                {
                    vector.x = targetTransform.localScale.x;
                }

                if (AnimateY)
                {
                    vector.y = AnimateY ? curveY.Evaluate(percent) + Offset : targetTransform.localScale.y;
                    vector.y = MMFeedbacksHelpers.Remap(vector.y, 0f, 1f, remapCurveZero, remapCurveOne);
                    if (Mode == Modes.Additive)
                    {
                        vector.y += _initialScale.y;
                    }
                }
                else
                {
                    vector.y = targetTransform.localScale.y;
                }

                if (AnimateZ)
                {
                    vector.z = AnimateZ ? curveZ.Evaluate(percent) + Offset : targetTransform.localScale.z;
                    vector.z = MMFeedbacksHelpers.Remap(vector.z, 0f, 1f, remapCurveZero, remapCurveOne);
                    if (Mode == Modes.Additive)
                    {
                        vector.z += _initialScale.z;
                    }
                }
                else
                {
                    vector.z = targetTransform.localScale.z;
                }

                if (UniformScaling)
                {
                    vector.y = vector.x;
                    vector.z = vector.x;
                }

                if (vector.x > MaxScale)
                    vector = new Vector3(MaxScale, MaxScale, MaxScale);

                targetTransform.localScale = vector;

                journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;

                yield return null;
            }

            vector = Vector3.zero;

            if (AnimateX)
            {
                vector.x = AnimateX ? curveX.Evaluate(FinalNormalizedTime) + Offset : targetTransform.localScale.x;
                vector.x = MMFeedbacksHelpers.Remap(vector.x, 0f, 1f, remapCurveZero, remapCurveOne);
                if (Mode == Modes.Additive)
                {
                    vector.x += _initialScale.x;
                }
            }
            else
            {
                vector.x = targetTransform.localScale.x;
            }

            if (AnimateY)
            {
                vector.y = AnimateY ? curveY.Evaluate(FinalNormalizedTime) + Offset : targetTransform.localScale.y;
                vector.y = MMFeedbacksHelpers.Remap(vector.y, 0f, 1f, remapCurveZero, remapCurveOne);
                if (Mode == Modes.Additive)
                {
                    vector.y += _initialScale.y;
                }
            }
            else
            {
                vector.y = targetTransform.localScale.y;
            }

            if (AnimateZ)
            {
                vector.z = AnimateZ ? curveZ.Evaluate(FinalNormalizedTime) + Offset : targetTransform.localScale.z;
                vector.z = MMFeedbacksHelpers.Remap(vector.z, 0f, 1f, remapCurveZero, remapCurveOne);
                if (Mode == Modes.Additive)
                {
                    vector.z += _initialScale.z;
                }
            }
            else
            {
                vector.z = targetTransform.localScale.z;
            }

            if (UniformScaling)
            {
                vector.y = vector.x;
                vector.z = vector.x;
            }

            if (vector.x > MaxScale)
                vector = new Vector3(MaxScale, MaxScale, MaxScale);

            targetTransform.localScale = vector;
            IsPlaying = false;
            _coroutine = null;
            yield return null;
        }

        /// <summary>
        /// On stop, we interrupt movement if it was active
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
            {
                return;
            }
            IsPlaying = false;
            Owner.StopCoroutine(_coroutine);
            _coroutine = null;

        }

        /// <summary>
        /// On disable we reset our coroutine
        /// </summary>
        public override void OnDisable()
        {
            _coroutine = null;
        }

        /// <summary>
        /// On Validate, we migrate our deprecated animation curves to our tween types if needed
        /// </summary>
        public override void OnValidate()
        {
            base.OnValidate();
            MMFeedbacksHelpers.MigrateCurve(AnimateScaleX, AnimateScaleTweenX, Owner);
            MMFeedbacksHelpers.MigrateCurve(AnimateScaleY, AnimateScaleTweenY, Owner);
            MMFeedbacksHelpers.MigrateCurve(AnimateScaleZ, AnimateScaleTweenZ, Owner);
        }

        /// <summary>
        /// On restore, we restore our initial state
        /// </summary>
        protected override void CustomRestoreInitialValues()
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }
            AnimateScaleTarget.localScale = _initialScale;
        }
    }
}