using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[CreateAssetMenu(fileName = "InventoryEngineKey", menuName = "MoreMountains/TopDownEngine/InventoryEngineKey", order = 1)]
	[Serializable]
	/// <summary>
	/// Pickable key item
	/// </summary>
	public class InventoryEngineKey : InventoryItem 
	{
        /// <summary>
        /// When the item is used, we try to grab our character's Health component, and if it exists, we add our health bonus amount of health
        /// 当物品被使用时，我们尝试获取我们角色的健康组件（Health component），如果它存在，我们就增加我们健康加成量的健康值。
        /// </summary>
        public override bool Use(string playerID)
		{
			base.Use(playerID);
			return true;
		}

		public override bool Pick(string playerID, bool IsAddPick = false) 
		{
			if (!IsAddPick)
				return true;

            MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, TargetInventoryName, this, ConsumeQuantity, 0, playerID);
            return true;
        }
    }
}