using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// 当到达检查点时触发的事件
	/// </summary>
	public struct CheckPointEvent
	{
		// 检查点的顺序
		public int Order;
		
		// 构造函数，初始化检查点顺序
		public CheckPointEvent(int order)
		{
			Order = order;
		}

		// 事件实例
		static CheckPointEvent e;
		
		// 触发检查点事件
		public static void Trigger(int order)
		{
			e.Order = order;
			MMEventManager.TriggerEvent(e);
		}
	}

	/// <summary>
	/// 检查点类。如果玩家死亡，将使其在此处重生。
	/// </summary>
	[AddComponentMenu("TopDown Engine/Spawn/Checkpoint")]
	public class CheckPoint : TopDownMonoBehaviour 
	{
		[Header("Spawn")]
		[MMInformation("将此脚本添加到一个（最好是空的）GameObject上，它将被添加到关卡的检查点列表中，允许你从那里重生。" +
			"如果你将其绑定到LevelManager的起始点，那么你的角色将在关卡开始时在那里生成。在这里，你可以决定角色应该面向左还是面向右生成。", MMInformationAttribute.InformationType.Info,false)]
        
        /// <summary>
        /// 是否这个检查点应该覆盖任何顺序并在进入时分配自己
        /// </summary>
        [Tooltip("是否这个检查点应该覆盖任何顺序并在进入时分配自己")]
        public bool ForceAssignation = false;
        
        /// <summary>
        /// 检查点的顺序
        /// </summary>
        [Tooltip("检查点的顺序")]
        public int CheckPointOrder;

		/// <summary>
		/// 初始化监听器列表
		/// </summary>
		protected virtual void Awake () 
		{
		}

		/// <summary>
		/// 处理2D碰撞器进入事件
		/// </summary>
		/// <param name="collider">进入的碰撞器</param>
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			TriggerEnter(collider.gameObject);            
		}

		/// <summary>
		/// 处理3D碰撞器进入事件
		/// </summary>
		/// <param name="collider">进入的碰撞器</param>
		protected virtual void OnTriggerEnter(Collider collider)
		{
			TriggerEnter(collider.gameObject);
		}

		/// <summary>
		/// 统一处理碰撞器进入事件
		/// </summary>
		/// <param name="collider">进入的游戏对象</param>
		protected virtual void TriggerEnter(GameObject collider)
		{
			
		}

	}
}
