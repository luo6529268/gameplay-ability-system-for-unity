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

        // 监听器列表，用于管理需要响应重生的对象
        protected List<Respawnable> _listeners;

		/// <summary>
		/// 初始化监听器列表
		/// </summary>
		protected virtual void Awake () 
		{
			_listeners = new List<Respawnable>();
		}
				
		/// <summary>
		/// 在检查点生成玩家
		/// </summary>
		/// <param name="player">玩家角色</param>
		public virtual void SpawnPlayer(Character player)
		{
			// 通知所有监听器玩家已重生
			foreach(Respawnable listener in _listeners)
			{
				listener.OnPlayerRespawn(this,player);
			}
		}
		
		/// <summary>
		/// 将可重生对象分配到此检查点
		/// </summary>
		/// <param name="listener">需要监听重生的对象</param>
		public virtual void AssignObjectToCheckPoint (Respawnable listener) 
		{
			_listeners.Add(listener);
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
			// 获取角色组件
			Character character = collider.GetComponent<Character>();

			// 检查是否是玩家角色
			if (character == null) { return; }
			if (character.CharacterType != Character.CharacterTypes.Player) { return; }
			if (!LevelManager.HasInstance) { return; }
			
			// 设置当前检查点并触发事件
			LevelManager.Instance.SetCurrentCheckpoint(this);
			CheckPointEvent.Trigger(CheckPointOrder);
		}

		/// <summary>
		/// 在Scene视图中绘制Gizmos，显示检查点之间的路径
		/// </summary>
		protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR
			// 确保LevelManager存在且有检查点
			if (!LevelManager.HasInstance)
			{
				return;
			}

			if (LevelManager.Instance.Checkpoints == null)
			{
				return;
			}

			if (LevelManager.Instance.Checkpoints.Count == 0)
			{
				return;
			}

			// 绘制检查点之间的连线
			for (int i=0; i < LevelManager.Instance.Checkpoints.Count; i++)
			{
				// 绘制到下一个检查点的线
				if ((i+1) < LevelManager.Instance.Checkpoints.Count)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(LevelManager.Instance.Checkpoints[i].transform.position,LevelManager.Instance.Checkpoints[i+1].transform.position);
				}
			}
			#endif
		}
	}
}
