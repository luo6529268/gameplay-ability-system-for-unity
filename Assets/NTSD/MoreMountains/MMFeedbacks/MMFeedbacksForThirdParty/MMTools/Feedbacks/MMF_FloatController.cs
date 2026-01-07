using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// This feedback will trigger a one time play on a target FloatController
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("这个反馈允许你触发目标FloatController上的一次性播放。")]
    [MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
    [FeedbackPath("GameObject/FloatController")]
    public class MMF_FloatController : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        /// the different possible modes 
        public enum Modes { OneTime, ToDestination }
        /// sets the inspector color for this feedback
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
        public override bool EvaluateRequiresSetup() { return (TargetFloatController == null); }
        public override string RequiredTargetText { get { return TargetFloatController != null ? TargetFloatController.name : ""; } }
        public override string RequiresSetupText { get { return "This feedback requires that a TargetFloatController be set to be able to work properly. You can set one below."; } }
#endif
        public override bool HasRandomness => true;
        public override bool CanForceInitialValue => true;
        public override bool ForceInitialValueDelayed => true;
        public override bool HasAutomatedTargetAcquisition => true;
        protected override void AutomateTargetAcquisition() => TargetFloatController = FindAutomatedTarget<FloatController>();

        [MMFInspectorGroup("Float Controller", true, 36, true)]
        /// <summary>
        /// 这个控制器的模式
        /// </summary>
        [Tooltip("这个控制器的模式")]
        public Modes Mode = Modes.OneTime;

        /// <summary>
        /// 要触发一次性播放的浮点控制器
        /// </summary>
        [Tooltip("要触发一次性播放的浮点控制器")]
        public FloatController TargetFloatController;

        /// <summary>
        /// 额外的、可选的浮点控制器列表，用于触发一次性播放
        /// </summary>
        [Tooltip("额外的、可选的浮点控制器列表，用于触发一次性播放")]
        public List<FloatController> ExtraTargetFloatControllers;

        /// <summary>
        /// 是否在结束时恢复到原始值
        /// </summary>
        [Tooltip("是否在结束时恢复到原始值")]
        public bool RevertToInitialValueAfterEnd = false;

        /// <summary>
        /// 一次性震动的持续时间
        /// </summary>
        [Tooltip("一次性震动的持续时间")]
        [MMFEnumCondition("Mode", (int)Modes.OneTime)]
        public float OneTimeDuration = 1f;

        /// <summary>
        /// 一次性震动的幅度（这将乘以曲线的高度）
        /// </summary>
        [Tooltip("一次性震动的幅度（这将乘以曲线的高度）")]
        [MMFEnumCondition("Mode", (int)Modes.OneTime)]
        public float OneTimeAmplitude = 1f;

        /// <summary>
        /// 将归一化曲线值重新映射到的低值
        /// </summary>
        [Tooltip("将归一化曲线值重新映射到的低值")]
        [MMFEnumCondition("Mode", (int)Modes.OneTime)]
        public float OneTimeRemapMin = 0f;

        /// <summary>
        /// 将归一化曲线值重新映射到的高值
        /// </summary>
        [Tooltip("将归一化曲线值重新映射到的高值")]
        [MMFEnumCondition("Mode", (int)Modes.OneTime)]
        public float OneTimeRemapMax = 1f;

        /// <summary>
        /// 应用于一次性震动的曲线
        /// </summary>
        [Tooltip("应用于一次性震动的曲线")]
        [MMFEnumCondition("Mode", (int)Modes.OneTime)]
        public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

        /// <summary>
        /// 要移动到的目标值
        /// </summary>
        [Tooltip("要移动到的目标值")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public float ToDestinationValue = 1f;

        /// <summary>
        /// 移动值的持续时间
        /// </summary>
        [Tooltip("移动值的持续时间")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public float ToDestinationDuration = 1f;

        /// <summary>
        /// 在ToDestination模式中移动值的曲线
        /// </summary>
        [Tooltip("在ToDestination模式中移动值的曲线")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

        /// the duration of this feedback is the duration of the one time hit
        public override float FeedbackDuration
        {
            get { return (Mode == Modes.OneTime) ? ApplyTimeMultiplier(OneTimeDuration) : ApplyTimeMultiplier(ToDestinationDuration); }
            set { OneTimeDuration = value; ToDestinationDuration = value; }
        }

        protected float _oneTimeDurationStorage;
        protected float _oneTimeAmplitudeStorage;
        protected float _oneTimeRemapMinStorage;
        protected float _oneTimeRemapMaxStorage;
        protected AnimationCurve _oneTimeCurveStorage;
        protected float _toDestinationValueStorage;
        protected float _toDestinationDurationStorage;
        protected AnimationCurve _toDestinationCurveStorage;
        protected bool _revertToInitialValueAfterEndStorage;

        /// <summary>
        /// On init we grab our initial values on the target float controller
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
        {
            if (Active && (TargetFloatController != null))
            {
                _oneTimeDurationStorage = TargetFloatController.OneTimeDuration;
                _oneTimeAmplitudeStorage = TargetFloatController.OneTimeAmplitude;
                _oneTimeCurveStorage = TargetFloatController.OneTimeCurve;
                _oneTimeRemapMinStorage = TargetFloatController.OneTimeRemapMin;
                _oneTimeRemapMaxStorage = TargetFloatController.OneTimeRemapMax;
                _toDestinationCurveStorage = TargetFloatController.ToDestinationCurve;
                _toDestinationDurationStorage = TargetFloatController.ToDestinationDuration;
                _toDestinationValueStorage = TargetFloatController.ToDestinationValue;
                _revertToInitialValueAfterEndStorage = TargetFloatController.RevertToInitialValueAfterEnd;
            }
        }

        /// <summary>
        /// On play we trigger a one time or ToDestination play on our target float controller
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized || (TargetFloatController == null))
            {
                return;
            }

            float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
            HandleFloatController(TargetFloatController, intensityMultiplier);
            foreach (FloatController floatController in ExtraTargetFloatControllers)
            {
                HandleFloatController(floatController, intensityMultiplier);
            }
        }

        /// <summary>
        /// Applies values to and triggers the target float controller
        /// </summary>
        /// <param name="target"></param>
        /// <param name="intensityMultiplier"></param>
        protected virtual void HandleFloatController(FloatController target, float intensityMultiplier)
        {
            target.RevertToInitialValueAfterEnd = RevertToInitialValueAfterEnd;

            if (Mode == Modes.OneTime)
            {
                target.OneTimeDuration = FeedbackDuration;
                target.OneTimeAmplitude = OneTimeAmplitude;
                target.OneTimeCurve = OneTimeCurve;
                if (NormalPlayDirection)
                {
                    target.OneTimeRemapMin = OneTimeRemapMin * intensityMultiplier;
                    target.OneTimeRemapMax = OneTimeRemapMax * intensityMultiplier;
                }
                else
                {
                    target.OneTimeRemapMin = OneTimeRemapMax * intensityMultiplier;
                    target.OneTimeRemapMax = OneTimeRemapMin * intensityMultiplier;
                }
                target.OneTime();
            }
            if (Mode == Modes.ToDestination)
            {
                target.ToDestinationCurve = ToDestinationCurve;
                target.ToDestinationDuration = FeedbackDuration;
                target.ToDestinationValue = ToDestinationValue;
                target.ToDestination();
            }
        }

        /// <summary>
        /// On reset we reset our values on the target controller with the ones stored initially
        /// </summary>
        protected override void CustomReset()
        {
            base.CustomReset();
            if (Active && FeedbackTypeAuthorized && (TargetFloatController != null))
            {
                ResetFloatController(TargetFloatController);
                foreach (FloatController controller in ExtraTargetFloatControllers)
                {
                    ResetFloatController(controller);
                }
            }
        }

        protected virtual void ResetFloatController(FloatController controller)
        {
            controller.OneTimeDuration = _oneTimeDurationStorage;
            controller.OneTimeAmplitude = _oneTimeAmplitudeStorage;
            controller.OneTimeCurve = _oneTimeCurveStorage;
            controller.OneTimeRemapMin = _oneTimeRemapMinStorage;
            controller.OneTimeRemapMax = _oneTimeRemapMaxStorage;
            controller.ToDestinationCurve = _toDestinationCurveStorage;
            controller.ToDestinationDuration = _toDestinationDurationStorage;
            controller.ToDestinationValue = _toDestinationValueStorage;
            controller.RevertToInitialValueAfterEnd = _revertToInitialValueAfterEndStorage;
        }


        /// <summary>
        /// On stop, we interrupt movement if it was active
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }

            if (TargetFloatController != null)
            {
                TargetFloatController.Stop();
                foreach (FloatController controller in ExtraTargetFloatControllers)
                {
                    controller.Stop();
                }
            }
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
            TargetFloatController.RestoreInitialValues();
            foreach (FloatController controller in ExtraTargetFloatControllers)
            {
                controller.RestoreInitialValues();
            }
        }
    }
}