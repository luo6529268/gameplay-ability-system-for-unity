using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// 将此脚本添加到平台上，当可操控角色踩上去时平台会掉落。
	/// 为平台添加 AutoRespawn 组件，角色死亡时平台会自动重置。
	/// </summary>
	[AddComponentMenu("TopDown Engine/Environment/Falling Platform 2D")]
	public class FallingPlatform2D : TopDownMonoBehaviour 
	{
		/// 掉落平台的可能状态
		public enum FallingPlatformStates { Idle, Shaking, Falling, ColliderOff }

		/// 掉落平台的当前状态
		[MMReadOnly]
		[Tooltip("掉落平台的当前状态")]
		public FallingPlatformStates StateNode;

		/// 如果为 true，平台一旦被触碰就会不可避免地掉落
		[Tooltip("如果为 true，平台一旦被触碰就会不可避免地掉落")]
		public bool InevitableFall = false;
		/// 平台掉落前的等待时间（秒）
		[Tooltip("平台掉落前的等待时间（秒）")]
		public float TimeBeforeFall = 2f;
		/// 平台开始掉落后，碰撞体关闭前的延迟时间（秒）
		[Tooltip("平台开始掉落后，碰撞体关闭前的延迟时间（秒）")]
		public float DelayBetweenFallAndColliderOff = 0.5f;

		// 私有变量
		protected Animator _animator;                  // 动画控制器
		protected Vector2 _newPosition;                // 新位置
		protected Bounds _bounds;                      // 边界
		protected Collider2D _collider;                // 2D 碰撞体
		protected Vector3 _initialPosition;            // 初始位置
		protected float _timeLeftBeforeFall;           // 掉落前剩余时间
		protected float _fallStartedAt;                // 掉落开始的时间点
		protected bool _contact = false;               // 是否与角色接触

		/// <summary>
		/// 初始化入口
		/// </summary>
		protected virtual void Start()
		{
			Initialization ();
		}

		/// <summary>
		/// 获取组件引用，保存初始位置和计时器
		/// </summary>
		protected virtual void Initialization()
		{
			// 获取动画控制器
			StateNode = FallingPlatformStates.Idle;
			_animator = GetComponent<Animator>();
			_collider = GetComponent<Collider2D> ();
			_collider.enabled = true;
			_initialPosition = this.transform.position;
			_timeLeftBeforeFall = TimeBeforeFall;

		}

		/// <summary>
		/// 每帧调用（固定更新）
		/// </summary>
		protected virtual void FixedUpdate()
		{		
			// 将各种状态发送给动画控制器		
			UpdateAnimator ();	

			if (_contact)
			{
				_timeLeftBeforeFall -= Time.deltaTime;
			}

			if (_timeLeftBeforeFall < 0)
			{
				if (StateNode != FallingPlatformStates.Falling)
				{
					_fallStartedAt = Time.time;
				}
				StateNode = FallingPlatformStates.Falling;
			}

			if (StateNode == FallingPlatformStates.Falling)
			{
				if (Time.time - _fallStartedAt >= DelayBetweenFallAndColliderOff)
				{
					_collider.enabled = false;
				}
			}            
		}

		/// <summary>
		/// 禁用掉落平台。不销毁它，以便在重生时可以恢复。
		/// </summary>
		protected virtual void DisableFallingPlatform()
		{
			this.gameObject.SetActive (false);					
			this.transform.position = _initialPosition;		
			_timeLeftBeforeFall = TimeBeforeFall;
			StateNode = FallingPlatformStates.Idle;
		}

		/// <summary>
		/// 更新平台的动画控制器状态
		/// </summary>
		protected virtual void UpdateAnimator()
		{				
			if (_animator!=null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Idle", (StateNode == FallingPlatformStates.Idle));
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Shaking", (StateNode == FallingPlatformStates.Shaking));
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, "Falling", (StateNode == FallingPlatformStates.Falling));
			}
		}

		/// <summary>
		/// 当 TopDownController 停留在平台上时触发
		/// </summary>
		/// <param name="collider">与平台碰撞的碰撞体</param>
		public virtual void OnTriggerStay2D(Collider2D collider)
		{
			TopDownController2D controller = collider.gameObject.MMGetComponentNoAlloc<TopDownController2D>();
			if (controller == null)
			{
				return;
			}

			if (StateNode == FallingPlatformStates.Falling)
			{
				return;
			}

			if (TimeBeforeFall>0)
			{
				_contact = true;
				StateNode = FallingPlatformStates.Shaking;
			}	
			else
			{
				if (!InevitableFall)
				{
					_contact = false;
					StateNode = FallingPlatformStates.Idle;
				}
			}
		}
		/// <summary>
		/// 当 TopDownController 离开平台时触发
		/// </summary>
		/// <param name="collider">与平台碰撞的碰撞体</param>
		protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			if (InevitableFall)
			{
				return;
			}

			TopDownController controller = collider.gameObject.GetComponent<TopDownController>();
			if (controller==null)
				return;

			_contact = false;
			if (StateNode == FallingPlatformStates.Shaking)
			{
				StateNode = FallingPlatformStates.Idle;
			}
		}

		/// <summary>
		/// 重生时恢复平台的状态
		/// </summary>
		protected virtual void OnRevive()
		{
			this.transform.position = _initialPosition;		
			_timeLeftBeforeFall = TimeBeforeFall;
			StateNode = FallingPlatformStates.Idle;

		}
	}
}