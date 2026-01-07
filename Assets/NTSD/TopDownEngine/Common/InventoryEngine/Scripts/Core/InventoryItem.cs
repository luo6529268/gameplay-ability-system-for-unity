using UnityEngine;
using MoreMountains.Tools;
using System;

namespace MoreMountains.InventoryEngine
{
	[Serializable]
	public class InventoryItemDisplayProperties
	{
		[Header("Buttons")]
		public bool DisplayEquipUseButton = true;
		public bool DisplayMoveButton = true;
		public bool DisplayDropButton = true;
		public bool DisplayEquipButton = true;
		public bool DisplayUseButton = true;
		public bool DisplayUnequipButton = true;
		
		[Header("Shortcuts")]
		public bool AllowEquipUseShortcut = true;
		public bool AllowMoveShortcut = true;
		public bool AllowDropShortcut = true;
		public bool AllowEquipShortcut = true;
		public bool AllowUseShortcut = true;
	}
	
	[Serializable]
	/// <summary>
	/// Base class for inventory items, meant to be extended.
	/// Will handle base properties and drop spawn
	/// </summary>
	public class InventoryItem : ScriptableObject 
	{
		[Header("ID and Target")]
        /// 该物品（唯一）的ID
        [Tooltip("该物品（唯一）的ID")]
        public string ItemID;
        /// 这个物品将被存储的目标库存名称
        [Tooltip("这个物品将被存储的目标库存名称")]
        public string TargetInventoryName = "MainInventory";
        /// if this is true, the item won't be added anywhere's there's room in the inventory, but instead at the specified TargetIndex slot
        [Tooltip("如果为真，物品将不会被添加到库存中有空位的地方，而是会被添加到指定的TargetIndex槽位")]
        public bool ForceSlotIndex = false;
        /// 如果ForceSlotIndex为真，则这是物品将被添加到目标库存中的索引位置
        [Tooltip("如果ForceSlotIndex为真，则这是物品将被添加到目标库存中的索引位置")]
        [MMCondition("ForceSlotIndex", true)]
        public int TargetIndex = 0;

        [Header("Permissions")]
        /// 是否这个物品可以通过“使用”方法被“使用” - 重要的是，这只是这个对象的初始状态，IsUsable用于在此之后任何时候使用
        [Tooltip("是否这个物品可以通过‘USE’方法被‘使用’ - 重要的是，这只是这个对象的初始状态，IsUsable用于在此之后任何时候使用")]
        public bool Usable = false;
        /// 如果这个为真，调用该对象的Use方法将消耗它的一个单位
        [Tooltip("如果这个为真，调用该对象的Use方法将消耗它的一个单位")]
        [MMCondition("Usable", true)]
        public bool Consumable = true;
        /// 如果这个物品是可消耗的，确定每次使用消耗的数量（通常是一）
        [Tooltip("如果这个物品是可消耗的，确定每次使用消耗的数量（通常是一）")]
        [MMCondition("Consumable", true)]
        public int ConsumeQuantity = 1;
        /// 是否这个物品可以被装备 - 重要的是，这只是这个对象的初始状态，IsEquippable用于在此之后任何时候使用
        [Tooltip("是否这个物品可以被装备 - 重要的是，这只是这个对象的初始状态，IsEquippable用于在此之后任何时候使用")]
        public bool Equippable = false;
        /// 是否即使目标库存满了，这个物品也可以被装备
        [Tooltip("是否即使目标库存满了，这个物品也可以被装备")]
        [MMCondition("Equippable", true)]
        public bool EquippableIfInventoryIsFull = true;
        /// 如果这个为真，当这个物品被装备时，它将从原始库存中移除，并移动到它的EquipmentInventory中
        [Tooltip("如果这个为真，当这个物品被装备时，它将从原始库存中移除，并移动到它的EquipmentInventory中")]
        [MMCondition("Equippable", true)]
        public bool MoveWhenEquipped = true;

        /// 如果这个为真，这个物品可以被丢弃
        [Tooltip("如果这个为真，这个物品可以被丢弃")]
        public bool Droppable = true;
        /// 如果这个为真，对象可以被移动
        [Tooltip("如果这个为真，对象可以被移动")]
        public bool CanMoveObject = true;
        /// 如果这个为真，对象可以与另一个对象交换
        [Tooltip("如果这个为真，对象可以与另一个对象交换")]
        public bool CanSwapObject = true;
        /// 一组属性，定义当该物品被选中时是否显示库存操作按钮
        [Tooltip("一组属性，定义当该物品被选中时是否显示库存操作按钮")]
        public InventoryItemDisplayProperties DisplayProperties;

        /// whether or not this object can be used
        public virtual bool IsUsable {  get { return Usable;  } }
		/// whether or not this object can be equipped
		public virtual bool IsEquippable { get { return Equippable; } }

		[HideInInspector]
		/// the base quantity of this item
		public int Quantity = 1;

		[Header("Basic info")]
        /// 物品的名称 - 将显示在详情面板中
        [Tooltip("物品的名称 - 将显示在详情面板中")]
        public string ItemName;
        /// 物品的简短描述，用于在详情面板中显示
        [TextArea]
        [Tooltip("物品的简短描述，用于在详情面板中显示")]
        public string ShortDescription;
        [TextArea]
        /// 物品的详细描述，用于在详情面板中显示
        [Tooltip("物品的详细描述，用于在详情面板中显示")]
        public string Description;

        [Header("Image")]
        /// 将在库存槽位上显示的图标
        [Tooltip("将在库存槽位上显示的图标")]
        public Sprite Icon;

        [Header("Prefab Drop")]
        /// 当物品被丢弃时将实例化的预制体
        [Tooltip("当物品被丢弃时将实例化的预制体")]
        public GameObject Prefab;
        /// 如果这个为真，当物品被丢弃时，对象的数量将被强制设置为PrefabDropQuantity
        [Tooltip("如果这个为真，当物品被丢弃时，对象的数量将被强制设置为PrefabDropQuantity")]
        public bool ForcePrefabDropQuantity = false;
        /// 如果ForcePrefabDropQuantity为真，则在生成的物品上强制设置的数量
        [Tooltip("如果ForcePrefabDropQuantity为真，则在生成的物品上强制设置的数量")]
        [MMCondition("ForcePrefabDropQuantity", true)]
        public int PrefabDropQuantity = 1;
        /// 物品被丢弃时应该生成的最小距离
        [Tooltip("物品被丢弃时应该生成的最小距离")]
        public MMSpawnAroundProperties DropProperties;

        [Header("Inventory Properties")]
        /// 如果这个对象可以堆叠（在单个库存槽位中存放多个实例），你可以在这里指定堆叠的最大大小。
        [Tooltip("如果这个对象可以堆叠（在单个库存槽位中存放多个实例），你可以在这里指定堆叠的最大大小。")]
        public int MaximumStack = 1;
        /// 在目标库存中允许的此物品的最大数量
        [Tooltip("在目标库存中允许的此物品的最大数量")]
        public int MaximumQuantity = 999999999;
        /// 物品的类别
        [Tooltip("物品的类别")]
        public ItemClasses ItemClass;

        [Header("Equippable")]
        /// 如果这个物品是可装备的，你可以在这里设置它的目标库存名称（例如ArmorInventory）。当然，你需要在你的场景中有一个匹配名称的库存。
        [Tooltip("如果这个物品是可装备的，你可以在这里设置它的目标库存名称（例如ArmorInventory）。当然，你需要在你的场景中有一个匹配名称的库存。")]
        public string TargetEquipmentInventoryName;
        /// 物品装备时应播放的声音（可选）
        [Tooltip("物品装备时应播放的声音（可选）")]
        public AudioClip EquippedSound;

        [Header("Usable")]
        /// 如果这个物品可以使用，你可以在这里设置一个在使用时播放的声音，如果你不设置，将会播放默认声音。
        [Tooltip("如果这个物品可以使用，你可以在这里设置一个在使用时播放的声音，如果你不设置，将会播放默认声音。")]
        public AudioClip UsedSound;

        [Header("Sounds")]
        /// 物品移动时应播放的声音（可选）
        [Tooltip("物品移动时应播放的声音（可选）")]
        public AudioClip MovedSound;
        /// 物品被丢弃时应播放的声音（可选）
        [Tooltip("物品被丢弃时应播放的声音（可选）")]
        public AudioClip DroppedSound;
        /// 如果设置为false，则不会使用默认声音，也不会播放任何声音
        [Tooltip("如果设置为false，则不会使用默认声音，也不会播放任何声音")]
        public bool UseDefaultSoundsIfNull = true;

        protected Inventory _targetInventory = null;
		protected Inventory _targetEquipmentInventory = null;

		/// <summary>
		/// Gets the target inventory.
		/// </summary>
		/// <value>The target inventory.</value>
		public virtual Inventory TargetInventory(string playerID)
		{ 
			if (TargetInventoryName == null)
			{
				return null;
			}
			_targetInventory = Inventory.FindInventory(TargetInventoryName, playerID);
			return _targetInventory;
		}

		/// <summary>
		/// Gets the target equipment inventory.
		/// </summary>
		/// <value>The target equipment inventory.</value>
		public virtual Inventory TargetEquipmentInventory(string playerID)
		{ 
			if (TargetEquipmentInventoryName == null)
			{
				return null;
			}
			_targetEquipmentInventory = Inventory.FindInventory(TargetEquipmentInventoryName, playerID);
			return _targetEquipmentInventory;
		}

		/// <summary>
		/// Determines if an item is null or not
		/// </summary>
		/// <returns><c>true</c> if is null the specified item; otherwise, <c>false</c>.</returns>
		/// <param name="item">Item.</param>
		public static bool IsNull(InventoryItem item)
		{
			if (item==null)
			{
				return true;
			}
			if (item.ItemID==null)
			{
				return true;
			}
			if (item.ItemID=="")
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Copies an item into a new one
		/// </summary>
		public virtual InventoryItem Copy()
		{
			string name = this.name;
			InventoryItem clone = UnityEngine.Object.Instantiate(this) as InventoryItem;
			clone.name = name;
			return clone;
		}

		/// <summary>
		/// Spawns the associated prefab
		/// </summary>
		public virtual GameObject SpawnPrefab(string playerID)
		{
			if (TargetInventory(playerID) != null)
			{
				// if there's a prefab set for the item at this slot, we instantiate it at the specified offset
				if (Prefab != null && TargetInventory(playerID).TargetTransform != null)
				{
					GameObject droppedObject=(GameObject)Instantiate(Prefab);
					ItemPicker droppedObjectItemPicker = droppedObject.GetComponent<ItemPicker>(); 
					
					if (droppedObjectItemPicker != null)
					{
						if (ForcePrefabDropQuantity)
						{
							droppedObjectItemPicker.Quantity = PrefabDropQuantity;
							droppedObjectItemPicker.RemainingQuantity = PrefabDropQuantity;	
						}
						else
						{
							droppedObjectItemPicker.Quantity = Quantity;
							droppedObjectItemPicker.RemainingQuantity = Quantity;	
						}
					}

					MMSpawnAround.ApplySpawnAroundProperties(droppedObject, DropProperties,
						TargetInventory(playerID).TargetTransform.position);

					return droppedObject;
				}
			}

			return null;
		}

        /// <summary>
        /// What happens when the object is picked - override this to add your own behaviors
        /// 当对象被拾取时会发生什么 - 重写这个方法来添加你自己的行为
        /// </summary>
        public virtual bool Pick(string playerID,bool IsAddPick = false) { return true; }

		/// <summary>
		/// What happens when the object is used - override this to add your own behaviors
		/// </summary>
		public virtual bool Use(string playerID) { return true; }

		/// <summary>
		/// What happens when the object is equipped - override this to add your own behaviors
		/// </summary>
		public virtual bool Equip(string playerID) { return true; }

		/// <summary>
		/// What happens when the object is unequipped (called when dropped) - override this to add your own behaviors
		/// </summary>
		public virtual bool UnEquip(string playerID) { return true; }

		/// <summary>
		/// What happens when the object gets swapped for another object
		/// </summary>
		public virtual void Swap(string playerID) {}

		/// <summary>
		/// What happens when the object is dropped - override this to add your own behaviors
		/// </summary>
		public virtual bool Drop(string playerID) { return true; }
	}
}