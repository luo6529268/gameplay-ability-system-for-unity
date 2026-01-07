using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[Serializable]
	public class MMSpringFloat : MMSpringDefinition<float>
	{

		[Tooltip("阻尼比决定了弹簧在受到干扰后的演化速度。低值时，它会长时间振荡；接近1时，它会快速停止振荡")]
		[Range(0.01f, 1f)]
		public float Damping = 0.4f;

		[Tooltip("频率决定了弹簧在受到干扰时的振荡速度，低频率意味着每秒振荡次数较少，高频率意味着每秒振荡次数较多")]
		public float Frequency = 6f;

		[MMInspectorGroup("调试", true, 19, true)]
		/// 弹簧的当前值
		[Tooltip("弹簧的当前值")]
		public override float CurrentValue
		{
			get
			{
				return _returnCurrentValue;
			}
			set
			{
				_actualCurrentValue = value;
				_returnCurrentValue = value;
				UpdateSpringDebug();
			}
		}

		public MMSpringClampSettings ClampSettings = new MMSpringClampSettings();
		

		[Tooltip("弹簧趋向的目标值，一旦停止振荡就会达到该值")]
		public override float TargetValue
		{
			get
			{
				return _targetValue;
			}
			set
			{
				_targetValue = ClampSettings.GetTargetValue(value, InitialValue);
				UpdateSpringDebug();
			}
		}


		[Tooltip("弹簧的当前速度")]
		public override float Velocity
		{
			get
			{
				return _velocity;
			}
			set
			{
				_velocity = value;
				UpdateSpringDebug();
			}
		}

		
		public float InitialValue { get; protected set; }
		
		public MMSpringDebug SpringDebug = new MMSpringDebug();

		[MMHidden]
		public bool UnifiedSpring = false;
		[MMHidden]
		public float CurrentValueDisplay;
		[MMHidden]
		public float TargetValueDisplay;
		[MMHidden]
		public float VelocityDisplay;
		
		protected float _actualCurrentValue;
		protected float _returnCurrentValue;
		protected float _targetValue;
		protected float _velocity;

		public override void UpdateSpringValue(float deltaTime)
		{
			MMMaths.Spring(ref _actualCurrentValue, TargetValue, ref _velocity, Damping, Frequency, deltaTime);
			_returnCurrentValue = _actualCurrentValue;
			if (ClampSettings.ClampNeeded)
			{
				HandleClampMode();
			}
			UpdateSpringDebug();
		}

		protected virtual void HandleClampMode()
		{
			float minValue = ClampSettings.ClampMinInitial ? InitialValue : ClampSettings.ClampMinValue;
			float maxValue = ClampSettings.ClampMaxInitial ? InitialValue : ClampSettings.ClampMaxValue;
			
			if (ClampSettings.ClampMin && (_actualCurrentValue < minValue))
			{
				
				if (ClampSettings.ClampMinBounce)
				{
					_returnCurrentValue = Mathf.Abs(_actualCurrentValue - minValue) + minValue;
				}
				else
				{
					_returnCurrentValue = Mathf.Max(_actualCurrentValue, minValue);	
				}
			}
			
			if (ClampSettings.ClampMax && (_actualCurrentValue > maxValue))
			{
				if (ClampSettings.ClampMaxBounce)
				{
					_returnCurrentValue = maxValue - (_actualCurrentValue - maxValue);
				}
				else
				{
					_returnCurrentValue = Mathf.Min(_actualCurrentValue, maxValue);	
				}
			}
		}

		protected virtual void UpdateSpringDebug() 
		{
			#if UNITY_EDITOR
			CurrentValueDisplay = (float)Math.Round(CurrentValue,3);
			TargetValueDisplay = (float)Math.Round(TargetValue,3);
			VelocityDisplay = (float)Math.Round(Velocity,3);
			SpringDebug.Update(_returnCurrentValue, TargetValue);
			#endif
		}
		
		public override void MoveToInstant(float newValue)
		{
			_actualCurrentValue = newValue;
			_returnCurrentValue = newValue;
			TargetValue = newValue;
			Velocity = 0;
		}

		public override void Stop()
		{
			Velocity = 0f;
			TargetValue = _actualCurrentValue;
		}

		public override void SetInitialValue(float newInitialValue)
		{
			InitialValue = newInitialValue;
		}

		public override void RestoreInitialValue()
		{
			_actualCurrentValue = InitialValue;
			_returnCurrentValue = InitialValue;
			TargetValue = _actualCurrentValue;
			UpdateSpringDebug();
		}

		public override void SetCurrentValueAsInitialValue()
		{
			InitialValue = _actualCurrentValue;
		}
		
		public override void MoveTo(float newValue)
		{
			TargetValue = newValue;
		}
		
		public override void MoveToAdditive(float newValue)
		{
			TargetValue += newValue;
		}
		
		public override void MoveToSubtractive(float newValue)
		{
			TargetValue -= newValue;
		}

		public override void MoveToRandom(float min, float max)
		{
			TargetValue = UnityEngine.Random.Range(min, max);
		}

		public override void Bump(float bumpAmount)
		{
			Velocity += bumpAmount;
		}

		public override void BumpRandom(float min, float max)
		{
			Velocity += UnityEngine.Random.Range(min, max);
		}
		
		public override void Finish()
		{
			Velocity = 0f;
			_actualCurrentValue = TargetValue;
			_returnCurrentValue = TargetValue;
			UpdateSpringDebug();
		}
	}
}