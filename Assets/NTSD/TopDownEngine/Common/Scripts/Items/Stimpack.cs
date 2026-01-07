using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A Stimpack / health bonus, that gives health back when picked
    /// 一个刺激包/健康奖励，当拾取时会恢复健康
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Stimpack")]
	public class Stimpack : PickableItem
	{
		[Header("Stimpack")]
        /// 当收集时增加的分数数量
        [Tooltip("当收集时增加的分数数量")]
        public float HealthToGive = 10f;
        /// 如果这个为真，只有玩家角色可以拾取这个
        [Tooltip("如果这个为真，只有玩家角色可以拾取这个")]
        public bool OnlyForPlayerCharacter = true;

        /// <summary>
        /// Triggered when something collides with the stimpack
        /// </summary>
        /// <param name="collider">Other.</param>
        protected override void Pick(GameObject picker)
		{
			Character character = picker.gameObject.MMGetComponentNoAlloc<Character>();
			if (OnlyForPlayerCharacter && (character != null) && (_character.CharacterType != Character.CharacterTypes.Player))
			{
				return;
			}

			Health characterHealth = picker.gameObject.MMGetComponentNoAlloc<Health>();
			// else, we give health to the player
			if (characterHealth != null)
			{
				characterHealth.ReceiveHealth(HealthToGive, gameObject);
			}            
		}
	}
}