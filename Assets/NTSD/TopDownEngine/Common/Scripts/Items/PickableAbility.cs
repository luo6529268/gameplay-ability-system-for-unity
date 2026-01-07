using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这是一个可拾取物品类，将它添加到带有触发器盒碰撞体2D的游戏对象上，
    /// 该对象将变成一个可拾取物品，能够允许或禁止角色使用某种能力
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Pickable Ability")]
    public class PickableAbility : PickableItem
    {
        // 定义枚举类型，表示可拾取物品的作用方式
        public enum Methods
        {
            Permit,    // 允许能力
            Forbid     // 禁止能力
        }

        [Header("Pickable Ability")]
        /// 该物品被拾取时是允许还是禁止某种能力
        [Tooltip("该物品被拾取时是允许还是禁止某种能力")]
        public Methods Method = Methods.Permit;

        /// 是否只有玩家类型的角色才能拾取此物品
        [Tooltip("是否只有玩家类型的角色才能拾取此物品")]
        public bool OnlyPickableByPlayerCharacters = true;

        // 用于存储能力类型的字符串表示（隐藏在Inspector中）
        [HideInInspector] public string AbilityTypeAsString;

        /// <summary>
        /// 检查物品是否可以被拾取
        /// </summary>
        /// <returns>如果可拾取返回true，否则返回false</returns>
        protected override bool CheckIfPickable()
        {
            // 获取碰撞对象上的角色组件
            _character = _collidingObject.GetComponent<Character>();

            // 如果碰撞对象没有角色组件，则不可拾取
            if (_character == null)
            {
                return false;
            }

            // 如果设置了只能被玩家拾取，且当前角色不是玩家类型，则不可拾取
            if (OnlyPickableByPlayerCharacters && (_character.CharacterType != Character.CharacterTypes.Player))
            {
                return false;
            }

            // 通过所有检查，可以拾取
            return true;
        }

        /// <summary>
        /// 当物品被拾取时，允许或禁止目标能力
        /// </summary>
        protected override void Pick(GameObject picker)
        {
            // 如果角色引用为空，直接返回
            if (_character == null)
            {
                return;
            }

            // 根据设置的方法类型确定新的能力状态
            bool newState = (Method == Methods.Permit);

            // TODO: 在这里应该添加实际允许或禁止能力的代码
        }
    }

}