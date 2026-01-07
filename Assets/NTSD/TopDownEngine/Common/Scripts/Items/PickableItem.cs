using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// An event typically fired when picking an item, letting listeners know what item has been picked
    ///  通常在拾取物品时触发的事件，让监听者知道哪个物品被拾取了
    /// </summary>
    public struct PickableItemEvent
	{
		public GameObject Picker;
		public PickableItem PickedItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.PickableItemEvent"/> struct.
        /// 初始化一个新的<see cref="MoreMountains.TopDownEngine.PickableItemEvent"/>结构实例。
        /// </summary>
        /// <param name="pickedItem">Picked item.</param>
        public PickableItemEvent(PickableItem pickedItem, GameObject picker) 
		{
			Picker = picker;
			PickedItem = pickedItem;
		}
		static PickableItemEvent e;
		public static void Trigger(PickableItem pickedItem, GameObject picker)
		{
			e.Picker = picker;
			e.PickedItem = pickedItem;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// A simple class, meant to be extended, that will handle all the mechanics of a pickable thing : feedbacks, collision, pick consequences, etc
    /// 一个简单的类，旨在被扩展，它将处理所有可拾取物品的机制：反馈、碰撞、拾取后果等
    /// </summary>
    public class PickableItem : TopDownMonoBehaviour
	{
		[Header("Pickable Item")]
        /// <summary>
        /// 当对象被拾取时播放的反馈
        /// </summary>
        [Tooltip("当对象被拾取时播放的反馈")]
        public MMFeedbacks PickedMMFeedbacks;
        /// <summary>
        /// 如果为真，拾取时拾取者的碰撞体将被禁用
        /// </summary>
        [Tooltip("如果为真，拾取时拾取者的碰撞体将被禁用")]
        public bool DisableColliderOnPick = false;
        /// <summary>
        /// 如果设置为真，当被拾取时对象将被禁用
        /// </summary>
        [Tooltip("如果设置为真，当被拾取时对象将被禁用")]
        public bool DisableObjectOnPick = true;
        /// <summary>
        /// 禁用对象之前的持续时间（秒），如果为0则立即禁用
        /// </summary>
        [MMCondition("DisableObjectOnPick", true)]
        [Tooltip("禁用对象之前的持续时间（秒），如果为0则立即禁用")]
        public float DisableDelay = 0f;
        /// <summary>
        /// 如果设置为真，当被拾取时对象的模型将被禁用
        /// </summary>
        [Tooltip("如果设置为真，当被拾取时对象的模型将被禁用")]
        public bool DisableModelOnPick = false;
        /// <summary>
        /// 如果设置为真，目标对象将在拾取时被禁用
        /// </summary>
        [Tooltip("如果设置为真，目标对象将在拾取时被禁用")]
        public bool DisableTargetObjectOnPick = false;
        /// <summary>
        /// 如果DisableTargetObjectOnPick为真，则在拾取时禁用此对象
        /// </summary>
        [Tooltip("如果DisableTargetObjectOnPick为真，则在拾取时禁用此对象")]
        [MMCondition("DisableTargetObjectOnPick", true)]
        public GameObject TargetObjectToDisable;
        /// <summary>
        /// 如果DisableTargetObjectOnPick为真，则在禁用目标之前的时间（秒）
        /// </summary>
        [Tooltip("如果DisableTargetObjectOnPick为真，则在禁用目标之前的时间（秒）")]
        [MMCondition("DisableTargetObjectOnPick", true)]
        public float TargetObjectDisableDelay = 1f;
        /// <summary>
        /// 此拾取器的视觉表现
        /// </summary>
        [MMCondition("DisableModelOnPick", true)]
        [Tooltip("此拾取器的视觉表现")]
        public GameObject Model;

        [Header("Pick Conditions")]
        /// <summary>
        /// 如果为真，这个可拾取物品将只能被带有角色组件的对象拾取
        /// </summary>
        [Tooltip("如果为真，这个可拾取物品将只能被带有角色组件的对象拾取")]
        public bool RequireCharacterComponent = true;
        /// <summary>
        /// 如果为真，这个可拾取物品将只能被带有角色组件的玩家类型对象拾取
        /// </summary>
        [Tooltip("如果为真，这个可拾取物品将只能被带有角色组件的玩家类型对象拾取")]
        public bool RequirePlayerType = true;

        protected Collider _collider;
		protected Collider2D _collider2D;
		protected GameObject _collidingObject;
		protected Character _character = null;
		protected bool _pickable = false;
		protected ItemPicker _itemPicker = null;
		protected WaitForSeconds _disableDelay;

		protected virtual void Start()
		{
			_disableDelay = new WaitForSeconds(DisableDelay);
			_collider = gameObject.GetComponent<Collider>();
			_collider2D = gameObject.GetComponent<Collider2D>();
			_itemPicker = gameObject.GetComponent<ItemPicker> ();
			PickedMMFeedbacks?.Initialization(this.gameObject);
		}

		/// <summary>
		/// Triggered when something collides with the coin
		/// </summary>
		/// <param name="collider">Other.</param>
		public virtual void OnTriggerEnter (Collider collider) 
		{
			_collidingObject = collider.gameObject;
			PickItem (collider.gameObject);
		}

		/// <summary>
		/// Triggered when something collides with the coin
		/// </summary>
		/// <param name="collider">Other.</param>
		public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			_collidingObject = collider.gameObject;
			PickItem (collider.gameObject);
		}

        /// <summary>
        /// Check if the item is pickable and if yes, proceeds with triggering the effects and disabling the object
        /// 检查物品是否可拾取，如果是，则触发效果并禁用对象
        /// </summary>
        public virtual void PickItem(GameObject picker)
		{
			if (CheckIfPickable ())
			{
				Effects ();
				PickableItemEvent.Trigger(this, picker);
				Pick (picker);
				if (DisableColliderOnPick)
				{
					if (_collider != null)
					{
						_collider.enabled = false;
					}
					if (_collider2D != null)
					{
						_collider2D.enabled = false;
					}
				}
				if (DisableModelOnPick && (Model != null))
				{
					Model.gameObject.SetActive(false);
				}
				
				if (DisableObjectOnPick)
				{
                    // we desactivate the gameobject
                    //我们停用游戏对象
                    if (DisableDelay == 0f)
					{
						this.gameObject.SetActive(false);
					}
					else
					{
						StartCoroutine(DisablePickerCoroutine());
					}
				}
				
				if (DisableTargetObjectOnPick && (TargetObjectToDisable != null))
				{
					if (TargetObjectDisableDelay == 0f)
					{
						TargetObjectToDisable.SetActive(false);
					}
					else
					{
						StartCoroutine(DisableTargetObjectCoroutine());
					}
				}			
			} 
		}

		protected virtual IEnumerator DisableTargetObjectCoroutine()
		{
			yield return MMCoroutine.WaitFor(TargetObjectDisableDelay);
			TargetObjectToDisable.SetActive(false);
		}

		protected virtual IEnumerator DisablePickerCoroutine()
		{
			yield return _disableDelay;
			this.gameObject.SetActive(false);
		}

        /// <summary>
        /// Checks if the object is pickable.
        /// 检查对象是否可拾取。
        /// </summary>
        /// <returns><c>true</c>, if if pickable was checked, <c>false</c> otherwise.</returns>
        protected virtual bool CheckIfPickable()
		{
            // if what's colliding with the coin ain't a characterBehavior, we do nothing and exit
            // 如果与硬币碰撞的不是角色行为，我们不做任何操作并退出
            _character = _collidingObject.GetComponent<Character>();
			if (RequireCharacterComponent)
			{
				if (_character == null)
				{
					return false;
				}
				
				if (RequirePlayerType && (_character.CharacterType != Character.CharacterTypes.Player))
				{
					return false;
				}
			}
			if (_itemPicker != null)
			{
				if  (!_itemPicker.Pickable())
				{
					return false;	
				}
			}

			return true;
		}

        /// <summary>
        /// Triggers the various pick effects
        /// 触发各种拾取效果
        /// </summary>
        protected virtual void Effects()
		{
			PickedMMFeedbacks?.PlayFeedbacks();
		}

        /// <summary>
        /// Override this to describe what happens when the object gets picked
        /// 覆盖此方法以描述当对象被拾取时会发生什么
        /// </summary>
        protected virtual void Pick(GameObject picker)
		{
			
		}
	}
}