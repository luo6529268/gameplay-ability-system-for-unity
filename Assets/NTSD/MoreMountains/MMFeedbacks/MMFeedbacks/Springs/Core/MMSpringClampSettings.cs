using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[Serializable]
	public class MMSpringClampSettings

	{
		[Header("Min")]

		[Tooltip("是否限制弹簧的最小值，防止其低于某个值")]
		public bool ClampMin = false;
		
		[Tooltip("弹簧不能低于的值")]
		[MMCondition("ClampMin", true)]
		public float ClampMinValue = 0f;
		
		[Tooltip("如果启用了ClampMin，是否使用初始值作为最小值")]
		[MMCondition("ClampMin", true)]
		public bool ClampMinInitial = false;
		
		[Tooltip("弹簧是否应该在最小值处反弹")]
		[MMCondition("ClampMin", true)]
		public bool ClampMinBounce = false;

		
		[Header("Max")]

		[Tooltip("是否限制弹簧的最大值，防止其超过某个值")]
		public bool ClampMax = false;

		[Tooltip("弹簧不能超过的最大值")]
		[MMCondition("ClampMax", true)]
		public float ClampMaxValue = 10f;

		[Tooltip("如果启用ClampMax，是否使用初始值作为最大值")]
		[MMCondition("ClampMax", true)]
		public bool ClampMaxInitial = false;

		[Tooltip("弹簧是否在达到最大值时反弹")]
		[MMCondition("ClampMax", true)]
		public bool ClampMaxBounce = false;


		public bool ClampNeeded => ClampMin || ClampMax || ClampMinBounce || ClampMaxBounce;

		public virtual float GetTargetValue(float value, float initialValue)
		{
			float targetValue = value;
			float clampMinValue = ClampMinInitial ? initialValue : ClampMinValue;
			if (ClampMin && value < clampMinValue)
			{
				targetValue = clampMinValue;
			}
			float clampMaxValue = ClampMaxInitial ? initialValue : ClampMaxValue;
			if (ClampMax && value > clampMaxValue)
			{
				targetValue = clampMaxValue;
			}
			return targetValue;
		}
	}
}

