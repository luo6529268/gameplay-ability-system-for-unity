using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{	
	/// <summary>
	/// Add this component to an object so it can be picked and added to an inventory
	/// </summary>
	public class ItemPicker : MonoBehaviour 
	{
		[Header("Item to pick")]
        /// the item that should be picked 
        [MMInformation("<size=15>将这个组件添加到一个触发器盒子碰撞体2D上，它将使其可以被拾取，并将指定的物品添加到其目标库存中。</size>" +
            "<size=15>只需将之前创建的物品拖放到下面的插槽中。有关如何创建物品的更多信息，</size>" +
            "<size=15>请查看文档。在这里，您还可以指定拾取对象时应该拾取多少数量的该物品。</size>", MMInformationAttribute.InformationType.Info, false)]
        public InventoryItem Item;

        [Header("Pick Quantity")]
        /// the initial quantity of that item that should be added to the inventory when picked
        [Tooltip("当拾取时，应该添加到库存中的该物品的初始数量")]
        public int Quantity = 1;
        /// the current quantity of that item that should be added to the inventory when picked
        [MMReadOnly]
        [Tooltip("当拾取时，应该添加到库存中的该物品的当前数量")]
        public int RemainingQuantity = 1;

        [Header("Conditions")]
        /// if you set this to true, a character will be able to pick this item even if its inventory is full
        [Tooltip("如果设置为true，则即使其库存已满，角色也可以拾取此物品")]
        public bool PickableIfInventoryIsFull = false;
        /// if you set this to true, the object will be disabled when picked
		[Tooltip("如果设置为true，则在物品被拾取后将禁用该对象")]
        public bool DisableObjectWhenDepleted = false;
        /// if this is true, this object will only be allowed to be picked by colliders with a Player tag
		[Tooltip("如果为真，则只有带有玩家标签的碰撞体才能拾取此对象")]
        public bool RequirePlayerTag = true;

        protected int _pickedQuantity = 0;
		protected Inventory _targetInventory;

		/// <summary>
		/// On Start we initialize our item picker
		/// </summary>
		protected virtual void Start()
		{
			Initialization ();
		}

		/// <summary>
		/// On Init we look for our target inventory
		/// </summary>
		protected virtual void Initialization()
		{
			FindTargetInventory (Item.TargetInventoryName);
			ResetQuantity();
		}

		/// <summary>
		/// Resets the remaining quantity to the initial quantity
		/// </summary>
		public virtual void ResetQuantity()
		{
			RemainingQuantity = Quantity;
		}
        
		/// <summary>
		/// Triggered when something collides with the picker
		/// </summary>
		/// <param name="collider">Other.</param>
		public virtual void OnTriggerEnter(Collider collider)
		{
			// if what's colliding with the picker ain't a characterBehavior, we do nothing and exit
			if (RequirePlayerTag && (!collider.CompareTag("Player")))
			{
				return;
			}

			string playerID = "Player1";
			InventoryCharacterIdentifier identifier = collider.GetComponent<InventoryCharacterIdentifier>();
			if (identifier != null)
			{
				playerID = identifier.PlayerID;
			}

			Pick(Item.TargetInventoryName, playerID);
		}

		/// <summary>
		/// Triggered when something collides with the picker
		/// </summary>
		/// <param name="collider">Other.</param>
		public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			// if what's colliding with the picker ain't a characterBehavior, we do nothing and exit
			if (RequirePlayerTag && (!collider.CompareTag("Player")))
			{
				return;
			}

			string playerID = "Player1";
			InventoryCharacterIdentifier identifier = collider.GetComponent<InventoryCharacterIdentifier>();
			if (identifier != null)
			{
				playerID = identifier.PlayerID;
			}

			Pick(Item.TargetInventoryName, playerID);
		}		

		/// <summary>
		/// Picks this item and adds it to its target inventory
		/// </summary>
		public virtual void Pick()
		{
			Pick(Item.TargetInventoryName);
		}

		/// <summary>
		/// Picks this item and adds it to the target inventory specified as a parameter
		/// </summary>
		/// <param name="targetInventoryName">Target inventory name.</param>
		public virtual void Pick(string targetInventoryName, string playerID = "Player1")
		{
			FindTargetInventory(targetInventoryName, playerID);
			if (_targetInventory == null)
			{
				return;
			}

			if (!Pickable()) 
			{
				PickFail ();
				return;
			}

			DetermineMaxQuantity ();
			if (!Application.isPlaying)
			{
				if (!Item.ForceSlotIndex)
				{
					_targetInventory.AddItem(Item, 1);	
				}
				else
				{
					_targetInventory.AddItemAt(Item, 1, Item.TargetIndex);
				}
			}				
			else
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, Item.TargetInventoryName, Item, _pickedQuantity, 0, playerID);
			}				
			if (Item.Pick(playerID))
			{
				RemainingQuantity = RemainingQuantity - _pickedQuantity;
				PickSuccess();
				DisableObjectIfNeeded();
			}			
		}

		/// <summary>
		/// Describes what happens when the object is successfully picked
		/// </summary>
		protected virtual void PickSuccess()
		{
			
		}

		/// <summary>
		/// Describes what happens when the object fails to get picked (inventory full, usually)
		/// </summary>
		protected virtual void PickFail()
		{

		}

		/// <summary>
		/// Disables the object if needed.
		/// </summary>
		protected virtual void DisableObjectIfNeeded()
		{
			// we desactivate the gameobject
			if (DisableObjectWhenDepleted && RemainingQuantity <= 0)
			{
				gameObject.SetActive(false);	
			}
		}

        /// <summary>
        /// Determines the max quantity of item that can be picked from this
        /// 确定从此可拾取物品的最大数量。
        /// </summary>
        protected virtual void DetermineMaxQuantity()
		{
			int maxQuantity = _targetInventory.CapMaxQuantity(Item, Quantity);
			int stackQuantity = _targetInventory.NumberOfStackableSlots (Item.ItemID, Item.MaximumStack);

			_pickedQuantity = Mathf.Min(maxQuantity, stackQuantity);
			
			if (RemainingQuantity < _pickedQuantity)
			{
				_pickedQuantity = RemainingQuantity;
			}
		}


        /// <summary>
        /// Returns true if this item can be picked, false otherwise
        /// 返回 true 如果这个物品可以被拾取，否则返回 false
        /// </summary>
        public virtual bool Pickable()
		{
			if (!PickableIfInventoryIsFull && _targetInventory.NumberOfFreeSlots == 0)
			{
                // we make sure that there isn't a place where we could store it
                //我们确保没有地方可以存放它
                int spaceAvailable = 0;
				List<int> list = _targetInventory.InventoryContains(Item.ItemID);
				if (list.Count > 0)
				{
					foreach (int index in list)
					{
						spaceAvailable += (Item.MaximumStack - _targetInventory.Content[index].Quantity);
					}
				}

				if (Item.Quantity <= spaceAvailable)
				{
					return true;
				}
				else
				{
					return false;	
				}
			}

			return true;
		}

		/// <summary>
		/// Finds the target inventory based on its name
		/// </summary>
		/// <param name="targetInventoryName">Target inventory name.</param>
		public virtual void FindTargetInventory(string targetInventoryName, string playerID = "Player1")
		{
			_targetInventory = null;
			_targetInventory = Inventory.FindInventory(targetInventoryName, playerID);

			if (_targetInventory == null)
				Debug.LogError("targetInventoryName:     " + targetInventoryName);
		}
	}
}