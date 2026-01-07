using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Link this component to a Health component, and it'll be able to process incoming damage through resistances, handling damage reduction/increase, condition changes, movement multipliers, feedbacks and more.
    /// 将这个组件链接到健康组件上，它将能够通过抗性处理传入的伤害，包括处理伤害减少/增加、状态变化、移动乘数、反馈等。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Health/Damage Resistance Processor")]
	public class DamageResistanceProcessor : TopDownMonoBehaviour
	{
		[Header("Damage Resistance List")]

        /// 如果设置为真，此组件将尝试自动从其子组件中填充其伤害抗性列表
        [Tooltip("如果设置为真，此组件将尝试自动从其子组件中填充其伤害抗性列表")]
        public bool AutoFillDamageResistanceList = true;

        /// 如果设置为真，在自动填充时将忽略被禁用的抗性
        [Tooltip("如果设置为真，在自动填充时将忽略被禁用的抗性")]
        public bool IgnoreDisabledResistances = true;

        /// 如果设置为真，此处理器将忽略对没有抗性的伤害类型的处理
        [Tooltip("如果设置为真，此处理器将忽略对没有抗性的伤害类型的处理")]
        public bool IgnoreUnknownDamageTypes = false;

        /// 此处理器将处理的伤害抗性列表。如果AutoFillDamageResistanceList为真，则自动填充
        [FormerlySerializedAs("DamageResitanceList")]
        [Tooltip("此处理器将处理的伤害抗性列表。如果AutoFillDamageResistanceList为真，则自动填充")]
        public List<DamageResistance> DamageResistanceList;


        /// <summary>
        /// On awake we initialize our processor
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Auto finds resistances if needed and sorts them
		/// </summary>
		protected virtual void Initialization()
		{
			if (AutoFillDamageResistanceList)
			{
				DamageResistance[] foundResistances =
					this.gameObject.GetComponentsInChildren<DamageResistance>(
						includeInactive: !IgnoreDisabledResistances);
				if (foundResistances.Length > 0)
				{
					DamageResistanceList = foundResistances.ToList();	
				}
			}
			SortDamageResistanceList();
		}

        /// <summary>
        /// A method used to reorder the list of resistances, based on priority by default.
        /// Don't hesitate to override this method if you'd like your resistances to be handled in a different order
        /// 用于根据优先级重新排序抗性列表的方法，默认按优先级排序。
		/// 如果你希望你的抗性以不同的顺序被处理，不要犹豫覆盖这个方法
        /// </summary>
        public virtual void SortDamageResistanceList()
		{
			// we sort the list by priority
			DamageResistanceList.Sort((p1,p2)=>p1.Priority.CompareTo(p2.Priority));
		}

        /// <summary>
        /// Processes incoming damage through the list of resistances, and outputs the final damage value
        /// 通过抗性列表处理传入的伤害，并输出最终的伤害值。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="typedDamages"></param>
        /// <param name="damageApplied"></param>
        /// <returns></returns>
        public virtual float ProcessDamage(float damage, List<TypedDamage> typedDamages, bool damageApplied)
		{
			float totalDamage = 0f;
			if (DamageResistanceList.Count == 0) // if we don't have resistances, we output raw damage
			{
				totalDamage = damage;
				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						totalDamage += typedDamage.DamageCaused;
					}
				}
				if (IgnoreUnknownDamageTypes)
				{
					totalDamage = damage;
				}
				return totalDamage;
			}
			else // if we do have resistances
			{
				totalDamage = damage;
				
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					totalDamage = resistance.ProcessDamage(totalDamage, null, damageApplied);
				}

				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						float currentDamage = typedDamage.DamageCaused;
						
						bool atLeastOneResistanceFound = false;
						foreach (DamageResistance resistance in DamageResistanceList)
						{
							if (resistance.TypeResistance == typedDamage.AssociatedDamageType)
							{
								atLeastOneResistanceFound = true;
							}
							currentDamage = resistance.ProcessDamage(currentDamage, typedDamage.AssociatedDamageType, damageApplied);
						}
						if (IgnoreUnknownDamageTypes && !atLeastOneResistanceFound)
						{
							// we don't add to the total
						}
						else
						{
							totalDamage += currentDamage;	
						}
						
					}
				}
				
				return totalDamage;
			}
		}

		public virtual void SetResistanceByLabel(string searchedLabel, bool active)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (resistance.Label == searchedLabel)
				{
					resistance.gameObject.SetActive(active);
				}
			}
		}

        /// <summary>
        /// When interrupting all damage over time of the specified type, stops their associated feedbacks if needed
        /// 当中断所有指定类型的伤害时，如果需要，停止它们的相关反馈。
        /// </summary>
        /// <param name="damageType"></param>
        public virtual void InterruptDamageOverTime(DamageType damageType)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if ( resistance.gameObject.activeInHierarchy &&
					((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) ||
				        (resistance.TypeResistance == damageType))
				    && resistance.InterruptibleFeedback)
				{
					resistance.OnDamageReceived?.StopFeedbacks();
				}
			}
		}

        /// <summary>
        /// Checks if any of the resistances prevents the character from changing condition, and returns true if that's the case, false otherwise
        /// 检查任何抗性是否阻止角色改变状态，如果是这种情况则返回真，否则返回假。
        /// </summary>
        /// <param name="typedDamage"></param>
        /// <returns></returns>
        public virtual bool CheckPreventCharacterConditionChange(DamageType typedDamage)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (!resistance.gameObject.activeInHierarchy)
				{
					continue;
				}
				
				if (typedDamage == null)
				{
					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.PreventCharacterConditionChange))
					{
						return true;	
					}
				}
				else
				{
					if ((resistance.TypeResistance == typedDamage) &&
					    (resistance.PreventCharacterConditionChange))
					{
						return true;
					}
				}
			}
			return false;
		}

        /// <summary>
        /// Checks if any of the resistances prevents the character from changing condition, and returns true if that's the case, false otherwise
        /// 检查是否有任何抗性阻止角色改变状态，如果有这种情况则返回`true`，否则返回`false`。
        /// </summary>
        /// <param name="typedDamage"></param>
        /// <returns></returns>
        public virtual bool CheckPreventMovementModifier(DamageType typedDamage)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (!resistance.gameObject.activeInHierarchy)
				{
					continue;
				}
				if (typedDamage == null)
				{
					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.PreventMovementModifier))
					{
						return true;	
					}
				}
				else
				{
					if ((resistance.TypeResistance == typedDamage) &&
					    (resistance.PreventMovementModifier))
					{
						return true;
					}
				}
			}
			return false;
		}

        /// <summary>
        /// Returns true if the resistances on this processor make it immune to knockback, false otherwise
        /// 如果这个处理器上的抗性使其对击退免疫，则返回`true`，否则返回`false`。
        /// </summary>
        /// <param name="typedDamage"></param>
        /// <returns></returns>
        public virtual bool CheckPreventKnockback(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (typedDamages.Count == 0))
			{
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					if (!resistance.gameObject.activeInHierarchy)
					{
						continue;
					}

					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.ImmuneToKnockback))
					{
						return true;	
					}
				}
			}
			else
			{
				foreach (TypedDamage typedDamage in typedDamages)
				{
					foreach (DamageResistance resistance in DamageResistanceList)
					{
						if (!resistance.gameObject.activeInHierarchy)
						{
							continue;
						}

						if (typedDamage == null)
						{
							if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
							    (resistance.ImmuneToKnockback))
							{
								return true;	
							}
						}
						else
						{
							if ((resistance.TypeResistance == typedDamage.AssociatedDamageType) &&
							    (resistance.ImmuneToKnockback))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

        /// <summary>
        /// Processes the input knockback force through the various resistances
        /// 通过各种抗性处理输入的击退力。
        /// </summary>
        /// <param name="knockback"></param>
        /// <param name="typedDamages"></param>
        /// <returns></returns>
        public virtual Vector3 ProcessKnockbackForce(Vector3 knockback, List<TypedDamage> typedDamages)
		{
            // if we don't have resistances, we output raw knockback value
            //如果没有抗性，我们输出原始的击退值。
            if (DamageResistanceList.Count == 0)
			{
				return knockback;
			}
			else 
			{
                // if we do have resistances
                //如果我们确实有抗性
                foreach (DamageResistance resistance in DamageResistanceList)
				{
					knockback = resistance.ProcessKnockback(knockback, null);
				}

				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						foreach (DamageResistance resistance in DamageResistanceList)
						{
							if (IgnoreDisabledResistances && !resistance.isActiveAndEnabled)
							{
								continue;
							}
							knockback = resistance.ProcessKnockback(knockback, typedDamage.AssociatedDamageType);
						}
					}
				}

				return knockback;
			}
		}
	}
}