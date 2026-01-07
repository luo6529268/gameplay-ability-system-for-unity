using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback used to trigger an animation (bool, int, float or trigger) on the associated animator, with or without randomness
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这个反馈可以让你向动画师（绑定在其检查器中）发送布尔值、整数、浮点数或触发器参数，允许你触发一个动画，无论是否带有随机性。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Animation/Animation Parameter")]
	public class MMF_Animation : MMF_Feedback 
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
		public override string RequiredTargetText { get { return BoundAnimator != null ? BoundAnimator.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundAnimator be set to be able to work properly. You can set one below."; } }
		#endif
		
		/// the duration of this feedback is the declared duration 
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundAnimator = FindAutomatedTarget<Animator>();

		[MMFInspectorGroup("Animation", true, 12, true)]
        /// 你想要更新参数的动画器
        [Tooltip("你想要更新参数的动画器")]
        public Animator BoundAnimator;
        /// 你想要更新参数的额外动画器列表
        [Tooltip("你想要更新参数的额外动画器列表")]
        public List<Animator> ExtraBoundAnimators;
        /// 玩家需要考虑的持续时间。这不会影响你的动画，但是是向MMF播放器传达这个反馈持续时间的一种方式。通常你会希望它与你的实际动画匹配，设置它可以有助于使这个反馈与暂停保持一致。
        [Tooltip("玩家需要考虑的持续时间。这不会影响你的动画，但是是向MMF播放器传达这个反馈持续时间的一种方式。通常你会希望它与你的实际动画匹配，设置它可以有助于使这个反馈与暂停保持一致。")]
        public float DeclaredDuration = 0f;


        [MMFInspectorGroup("Trigger", true, 16)]
        /// 如果设置为真，将更新指定的触发器参数
        [Tooltip("如果设置为真，将更新指定的触发器参数")]
        public bool UpdateTrigger = false;
        /// 与此触发器交互的选定模式
        [Tooltip("与此触发器交互的选定模式")]
        [MMFCondition("UpdateTrigger", true)]
        public TriggerModes TriggerMode = TriggerModes.SetTrigger;
        /// 当反馈播放时，触发动画器参数的名称
        [Tooltip("当反馈播放时，触发动画器参数的名称")]
        [MMFCondition("UpdateTrigger", true)]
        public string TriggerParameterName;


        [MMFInspectorGroup("Random Trigger", true, 20)]
        /// 如果设置为真，将从下面的列表中随机选择一个触发器参数进行更新
        [Tooltip("如果设置为真，将从下面的列表中随机选择一个触发器参数进行更新")]
        public bool UpdateRandomTrigger = false;
        /// 与此触发器交互的选定模式
        [Tooltip("与此触发器交互的选定模式")]
        [MMFCondition("UpdateRandomTrigger", true)]
        public TriggerModes RandomTriggerMode = TriggerModes.SetTrigger;
        /// 当反馈播放时，随机触发的动画器参数名称列表
        [Tooltip("当反馈播放时，随机触发的动画器参数名称列表")]
        public List<string> RandomTriggerParameterNames;


        [MMFInspectorGroup("Bool", true, 17)]
        /// 如果设置为真，将更新指定的布尔参数
        [Tooltip("如果设置为真，将更新指定的布尔参数")]
        public bool UpdateBool = false;
        /// 当反馈播放时，将被设置为真的布尔参数
        [Tooltip("当反馈播放时，将被设置为真的布尔参数")]
        [MMFCondition("UpdateBool", true)]
        public string BoolParameterName;
        /// 在布尔模式下，是否将布尔参数设置为真或假
        [Tooltip("在布尔模式下，是否将布尔参数设置为真或假")]
        [MMFCondition("UpdateBool", true)]
        public bool BoolParameterValue = true;


        [MMFInspectorGroup("Random Bool", true, 19)]
        /// 如果设置为真，将从下面的列表中随机选择一个布尔参数进行更新
        [Tooltip("如果设置为真，将从下面的列表中随机选择一个布尔参数进行更新")]
        public bool UpdateRandomBool = false;
        /// 在布尔模式下，是否将布尔参数设置为真或假
        [Tooltip("在布尔模式下，是否将布尔参数设置为真或假")]
        [MMFCondition("UpdateRandomBool", true)]
        public bool RandomBoolParameterValue = true;
        /// 当反馈播放时，随机设置为真的布尔参数名称列表
        [Tooltip("当反馈播放时，随机设置为真的布尔参数名称列表")]
        public List<string> RandomBoolParameterNames;


        [MMFInspectorGroup("Int", true, 24)]
        /// 当反馈播放时，将被设置的整型参数
        [Tooltip("当反馈播放时，将被设置的整型参数")]
        public ValueModes IntValueMode = ValueModes.None;
        /// 当反馈播放时，将被设置为真的整型参数名称
        [Tooltip("当反馈播放时，将被设置为真的整型参数名称")]
        [MMFEnumCondition("IntValueMode", (int)ValueModes.Constant, (int)ValueModes.Random, (int)ValueModes.Incremental)]
        public string IntParameterName;
        /// 要设置给该整型参数的值
        [Tooltip("要设置给该整型参数的值")]
        [MMFEnumCondition("IntValueMode", (int)ValueModes.Constant)]
        public int IntValue;
        /// 随机设置给该整型参数的最小值（包含）
        [Tooltip("随机设置给该整型参数的最小值（包含）")]
        [MMFEnumCondition("IntValueMode", (int)ValueModes.Random)]
        public int IntValueMin;
        /// 随机设置给该整型参数的最大值（不包含）
        [Tooltip("随机设置给该整型参数的最大值（不包含）")]
        [MMFEnumCondition("IntValueMode", (int)ValueModes.Random)]
        public int IntValueMax = 5;
        /// 要增加给该整型参数的值
        [Tooltip("要增加给该整型参数的值")]
        [MMFEnumCondition("IntValueMode", (int)ValueModes.Incremental)]
        public int IntIncrement = 1;


        [MMFInspectorGroup("Float", true, 22)]
        /// 当反馈播放时，将被设置的浮点型参数
        [Tooltip("当反馈播放时，将被设置的浮点型参数")]
        public ValueModes FloatValueMode = ValueModes.None;
        /// 当反馈播放时，将被设置为真的浮点型参数名称
        [Tooltip("当反馈播放时，将被设置为真的浮点型参数名称")]
        [MMFEnumCondition("FloatValueMode", (int)ValueModes.Constant, (int)ValueModes.Random, (int)ValueModes.Incremental)]
        public string FloatParameterName;
        /// 要设置给该浮点型参数的值
        [Tooltip("要设置给该浮点型参数的值")]
        [MMFEnumCondition("FloatValueMode", (int)ValueModes.Constant)]
        public float FloatValue;
        /// 随机设置给该浮点型参数的最小值（包含）
        [Tooltip("随机设置给该浮点型参数的最小值（包含）")]
        [MMFEnumCondition("FloatValueMode", (int)ValueModes.Random)]
        public float FloatValueMin;
        /// 随机设置给该浮点型参数的最大值（不包含）
        [Tooltip("随机设置给该浮点型参数的最大值（不包含）")]
        [MMFEnumCondition("FloatValueMode", (int)ValueModes.Random)]
        public float FloatValueMax = 5;
        /// 要增加给该浮点型参数的值
        [Tooltip("要增加给该浮点型参数的值")]
        [MMFEnumCondition("FloatValueMode", (int)ValueModes.Incremental)]
        public float FloatIncrement = 1;


        [MMFInspectorGroup("Layer Weights", true, 22)]
        /// 是否在播放此反馈时设置指定层的层权重
        [Tooltip("是否在播放此反馈时设置指定层的层权重")]
        public bool SetLayerWeight = false;
        /// 更改层权重时目标层的索引
        [Tooltip("更改层权重时目标层的索引")]
        [MMFCondition("SetLayerWeight", true)]
        public int TargetLayerIndex = 1;
        /// 设置在目标动画器层上的新权重
        [Tooltip("设置在目标动画器层上的新权重")]
        [MMFCondition("SetLayerWeight", true)]
        public float NewWeight = 0.5f;


        protected int _triggerParameter;
		protected int _boolParameter;
		protected int _intParameter;
		protected int _floatParameter;
		protected List<int> _randomTriggerParameters;
		protected List<int> _randomBoolParameters;

		/// <summary>
		/// Custom Init
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			_triggerParameter = Animator.StringToHash(TriggerParameterName);
			_boolParameter = Animator.StringToHash(BoolParameterName);
			_intParameter = Animator.StringToHash(IntParameterName);
			_floatParameter = Animator.StringToHash(FloatParameterName);

			_randomTriggerParameters = new List<int>();
			foreach (string name in RandomTriggerParameterNames)
			{
				_randomTriggerParameters.Add(Animator.StringToHash(name));
			}

			_randomBoolParameters = new List<int>();
			foreach (string name in RandomBoolParameterNames)
			{
				_randomBoolParameters.Add(Animator.StringToHash(name));
			}
		}

		/// <summary>
		/// On Play, checks if an animator is bound and triggers parameters
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

			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);

			ApplyValue(BoundAnimator, intensityMultiplier);
			foreach (Animator animator in ExtraBoundAnimators)
			{
				ApplyValue(animator, intensityMultiplier);
			}
		}

		/// <summary>
		/// Applies values on the target Animator
		/// </summary>
		/// <param name="targetAnimator"></param>
		/// <param name="intensityMultiplier"></param>
		protected virtual void ApplyValue(Animator targetAnimator, float intensityMultiplier)
		{
			if (UpdateTrigger)
			{
				if (TriggerMode == TriggerModes.SetTrigger)
				{
					targetAnimator.SetTrigger(_triggerParameter);
				}
				if (TriggerMode == TriggerModes.ResetTrigger)
				{
					targetAnimator.ResetTrigger(_triggerParameter);
				}
			}
            
			if (UpdateRandomTrigger)
			{
				int randomParameter = _randomTriggerParameters[Random.Range(0, _randomTriggerParameters.Count)];
                
				if (RandomTriggerMode == TriggerModes.SetTrigger)
				{
					targetAnimator.SetTrigger(randomParameter);
				}
				if (RandomTriggerMode == TriggerModes.ResetTrigger)
				{
					targetAnimator.ResetTrigger(randomParameter);
				}
			}

			if (UpdateBool)
			{
				targetAnimator.SetBool(_boolParameter, BoolParameterValue);
			}

			if (UpdateRandomBool)
			{
				int randomParameter = _randomBoolParameters[Random.Range(0, _randomBoolParameters.Count)];
                
				targetAnimator.SetBool(randomParameter, RandomBoolParameterValue);
			}

			switch (IntValueMode)
			{
				case ValueModes.Constant:
					targetAnimator.SetInteger(_intParameter, IntValue);
					break;
				case ValueModes.Incremental:
					int newValue = targetAnimator.GetInteger(_intParameter) + IntIncrement;
					targetAnimator.SetInteger(_intParameter, newValue);
					break;
				case ValueModes.Random:
					int randomValue = Random.Range(IntValueMin, IntValueMax);
					targetAnimator.SetInteger(_intParameter, randomValue);
					break;
			}

			switch (FloatValueMode)
			{
				case ValueModes.Constant:
					targetAnimator.SetFloat(_floatParameter, FloatValue * intensityMultiplier);
					break;
				case ValueModes.Incremental:
					float newValue = targetAnimator.GetFloat(_floatParameter) + FloatIncrement * intensityMultiplier;
					targetAnimator.SetFloat(_floatParameter, newValue);
					break;
				case ValueModes.Random:
					float randomValue = Random.Range(FloatValueMin, FloatValueMax) * intensityMultiplier;
					targetAnimator.SetFloat(_floatParameter, randomValue);
					break;
			}

			if (SetLayerWeight)
			{
				targetAnimator.SetLayerWeight(TargetLayerIndex, NewWeight);
			}
		}
        
		/// <summary>
		/// On stop, turns the bool parameter to false
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !UpdateBool || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			BoundAnimator.SetBool(_boolParameter, false);
			foreach (Animator animator in ExtraBoundAnimators)
			{
				animator.SetBool(_boolParameter, false);
			}
		}
	}
}