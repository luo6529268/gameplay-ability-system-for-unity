using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Used by the DamageResistanceProcessor, this class defines the resistance versus a certain type of damage. 
    /// 由DamageResistanceProcessor使用，这个类定义了对某种特定伤害类型的抗性。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Health/Damage Resistance")]
	public class DamageResistance : TopDownMonoBehaviour
	{
		public enum DamageModifierModes { Multiplier, Flat }
		public enum KnockbackModifierModes { Multiplier, Flat }

		[Header("General \t 通用")]
        /// 这个伤害抗性的优先级。这将被用来确定伤害抗性的评估顺序。优先级最低的意味着首先被评估。
        [Tooltip("这个伤害抗性的优先级。这将被用来确定伤害抗性的评估顺序。优先级最低的意味着首先被评估。")]
        public float Priority = 0;
        /// 这个伤害抗性的标签。用于组织，以及通过标签激活/停用抗性。
        [Tooltip("这个伤害抗性的标签。用于组织，以及通过标签激活/停用抗性。")]
        public string Label = "";

        [Header("Damage Resistance Settings \t 伤害抗性设置")]
        /// 这个抗性是否影响基础伤害或类型化伤害
        [Tooltip("这个抗性是否影响基础伤害或类型化伤害")]
        public DamageTypeModes DamageTypeMode = DamageTypeModes.BaseDamage;
        /// 在类型化伤害模式下，这个抗性将与之交互的伤害类型
        [Tooltip("在类型化伤害模式下，这个抗性将与之交互的伤害类型")]
        [MMEnumCondition("DamageTypeMode", (int)DamageTypeModes.TypedDamage)]
        public DamageType TypeResistance;
        /// 减少（或增加）接收到的伤害的方式。乘数将把传入的伤害乘以一个乘数，平减将从传入的伤害中减去一个常数值。
        [Tooltip("减少（或增加）接收到的伤害的方式。乘数将把传入的伤害乘以一个乘数，平减将从传入的伤害中减去一个常数值。")]
        public DamageModifierModes DamageModifierMode = DamageModifierModes.Multiplier;

        [Header("Damage Modifiers \t 伤害修饰符")]
        /// 在乘数模式下，应用到传入伤害的乘数。0.5将减半，而2的值将造成对指定伤害类型的弱点，伤害将加倍。
        [Tooltip("在乘数模式下，应用到传入伤害的乘数。0.5将减半，而2的值将造成对指定伤害类型的弱点，伤害将加倍。")]
        [MMEnumCondition("DamageModifierMode", (int)DamageModifierModes.Multiplier)]
        public float DamageMultiplier = 0.25f;
        /// 在平减模式下，每次接收到该类型伤害时减去的伤害量
        [Tooltip("在平减模式下，每次接收到该类型伤害时减去的伤害量")]
        [MMEnumCondition("DamageModifierMode", (int)DamageModifierModes.Flat)]
        public float FlatDamageReduction = 10f;
        /// 是否应该将指定类型的传入伤害限制在最小值和最大值之间
        [Tooltip("是否应该将指定类型的传入伤害限制在最小值和最大值之间")]
        public bool ClampDamage = false;
        /// 限制传入伤害的值
        [Tooltip("限制传入伤害的值")]
        [MMVector("Min","Max")]
		public Vector2 DamageModifierClamps = new Vector2(0f,10f);

		[Header("Condition Change \t 状态改变")]
        /// 是否允许这种类型的伤害改变状态
        [Tooltip("是否允许这种类型的伤害改变状态")]
        public bool PreventCharacterConditionChange = false;
        /// 是否允许这种类型的伤害改变移动
        [Tooltip("是否允许这种类型的伤害改变移动")]
        public bool PreventMovementModifier = false;

        [Header("Knockback \t 击退")]
        /// 如果为真，则忽略并不应用击退力
        [Tooltip("如果为真，则忽略并不应用击退力")]
        public bool ImmuneToKnockback = false;
        /// 减少（或增加）接收到的击退的方式。乘数将把传入的击退强度乘以一个乘数，平减将从传入的击退强度中减去一个常数值。
        [Tooltip("减少（或增加）接收到的击退的方式。乘数将把传入的击退强度乘以一个乘数，平减将从传入的击退强度中减去一个常数值。")]
        public KnockbackModifierModes KnockbackModifierMode = KnockbackModifierModes.Multiplier;
        /// 在乘数模式下，应用到传入击退的乘数。0.5将减半，而2的值将造成对指定伤害类型的弱点，击退强度将加倍。
        [Tooltip("在乘数模式下，应用到传入击退的乘数。0.5将减半，而2的值将造成对指定伤害类型的弱点，击退强度将加倍。")]
        [MMEnumCondition("KnockbackModifierMode", (int)DamageModifierModes.Multiplier)]
        public float KnockbackMultiplier = 1f;
        /// 在平减模式下，每次接收到该类型伤害时减去的击退量
        [Tooltip("在平减模式下，每次接收到该类型伤害时减去的击退量")]
        [MMEnumCondition("KnockbackModifierMode", (int)DamageModifierModes.Flat)]
        public float FlatKnockbackMagnitudeReduction = 10f;
        /// 是否应该将指定类型的传入击退限制在最小值和最大值之间
        [Tooltip("是否应该将指定类型的传入击退限制在最小值和最大值之间")]
        public bool ClampKnockback = false;
        /// 限制传入击退强度的值
        [Tooltip("限制传入击退强度的值")]
        [MMCondition("ClampKnockback", true)]
        public float KnockbackMaxMagnitude = 10f;

        [Header("Feedbacks \t 反馈")]
        /// 只有当接收到匹配类型的伤害时，这个反馈才会被触发
        [Tooltip("只有当接收到匹配类型的伤害时，这个反馈才会被触发")]
        public MMFeedbacks OnDamageReceived;
        /// 当该类型的伤害被中断时，这个反馈是否可以被中断（停止）
        [Tooltip("当该类型的伤害被中断时，这个反馈是否可以被中断（停止）")]
        public bool InterruptibleFeedback = false;
        /// 如果为真，则在播放前总是先停止反馈
        [Tooltip("如果为真，则在播放前总是先停止反馈")]
        public bool AlwaysInterruptFeedbackBeforePlay = false;
        /// 如果接收到的伤害为零，是否应该播放这个反馈
        [Tooltip("如果接收到的伤害为零，是否应该播放这个反馈")]
        public bool TriggerFeedbackIfDamageIsZero = false;

        /// <summary>
        /// On awake we initialize our feedback
        /// </summary>
        protected virtual void Awake()
		{
			OnDamageReceived?.Initialization(this.gameObject);
		}

        /// <summary>
        /// When getting damage, goes through damage reduction and outputs the resulting damage
        /// 当受到伤害时，进行伤害减免并输出结果伤害
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="type"></param>
        /// <param name="damageApplied"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual float ProcessDamage(float damage, DamageType type, bool damageApplied)
		{
			if (!this.gameObject.activeInHierarchy)
			{
				return damage;
			}
			
			if ((type == null) && (DamageTypeMode != DamageTypeModes.BaseDamage))
			{
				return damage;
			}

			if ((type != null) && (DamageTypeMode == DamageTypeModes.BaseDamage))
			{
				return damage;
			}

			if ((type != null) && (type != TypeResistance))
			{
				return damage;
			}

            // applies damage modifier or reduction
            //应用伤害修饰符或减免
            switch (DamageModifierMode)
			{
				case DamageModifierModes.Multiplier:
					damage = damage * DamageMultiplier;
					break;
				case DamageModifierModes.Flat:
					damage = damage - FlatDamageReduction;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			// clamps damage
			damage = ClampDamage ? Mathf.Clamp(damage, DamageModifierClamps.x, DamageModifierClamps.y) : damage;

			if (damageApplied)
			{
				if (!TriggerFeedbackIfDamageIsZero && (damage == 0))
				{
					// do nothing
				}
				else
				{
					if (AlwaysInterruptFeedbackBeforePlay)
					{
						OnDamageReceived?.StopFeedbacks();
					}
					OnDamageReceived?.PlayFeedbacks(this.transform.position);	
				}
			}

			return damage;
		}

        /// <summary>
        /// Processes the knockback input value and returns it potentially modified by damage resistances
        /// 处理击退输入值，根据伤害抗性可能修改它
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="type"></param>
        /// <param name="damageApplied"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual Vector3 ProcessKnockback(Vector3 knockback, DamageType type)
		{
			if (!this.gameObject.activeInHierarchy)
			{
				return knockback;
			}

			if ((type == null) && (DamageTypeMode != DamageTypeModes.BaseDamage))
			{
				return knockback;
			}

			if ((type != null) && (DamageTypeMode == DamageTypeModes.BaseDamage))
			{
				return knockback;
			}

			if ((type != null) && (type != TypeResistance))
			{
				return knockback;
			}

			// applies damage modifier or reduction
			switch (KnockbackModifierMode)
			{
				case KnockbackModifierModes.Multiplier:
					knockback = knockback * KnockbackMultiplier;
					break;
				case KnockbackModifierModes.Flat:
					float magnitudeReduction = Mathf.Clamp(Mathf.Abs(knockback.magnitude) - FlatKnockbackMagnitudeReduction, 0f, Single.MaxValue);
					knockback = knockback.normalized * magnitudeReduction * Mathf.Sign(knockback.magnitude);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			// clamps damage
			knockback = ClampKnockback ? Vector3.ClampMagnitude(knockback, KnockbackMaxMagnitude) : knockback;

			return knockback;
		}
	}
}
