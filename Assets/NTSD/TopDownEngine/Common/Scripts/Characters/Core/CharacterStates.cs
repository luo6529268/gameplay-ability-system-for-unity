using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{	
	/// <summary>
	/// The various states you can use to check if your character is doing something at the current frame
	/// </summary>    
	public class CharacterStates 
	{
		/// The possible character conditions
		public enum CharacterConditions
		{
            // 正常
            Normal,
            // 受控移动
            ControlledMovement,
            // 冻结
            Frozen,
            // 暂停
            Paused,
            // 死亡
            Dead,
            // 昏迷
            Stunned,
            // 复活
            Revive,

        }

        /// The possible Movement States the character can be in. These usually correspond to their own class, 
        /// but it's not mandatory
        public enum CharacterState 
		{
            // 空状态
            Null,
            // 空闲
            Idle,
            // 行走
            Walking,
            // 下落
            Falling,
            // 奔跑
            Running,
            // 蹲下
            Crouching,
            // 冲刺
            Dashing,
            // 跳跃
            Jumping,
            // 攻击
            Attacking,

        }
    }
}