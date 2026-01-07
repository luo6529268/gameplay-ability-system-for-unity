using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个控制器，用于在俯视图中移动Rigidbody2D和Collider2D。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Core/TopDown Controller 2D")]
	public class TopDownController2D : TopDownController
	{
        /// <summary>
		///  碰撞体的中心位置
		/// </summary>
        public override Vector3 ColliderCenter { get { return (Vector2)this.transform.position + ColliderOffset; } }
        /// <summary>
        ///  碰撞体的底部位置
        /// </summary>
        public override Vector3 ColliderBottom { get { return (Vector2)this.transform.position + ColliderOffset + Vector2.down * ColliderBounds.extents.y; } }
        /// <summary>
        /// 碰撞体的顶部位置
        /// </summary>
        public override Vector3 ColliderTop { get { return (Vector2)this.transform.position + ColliderOffset + Vector2.up * ColliderBounds.extents.y; } }

		// Step D6: Plan A 物理开关
		[Header("Step D6: Physics Mode")]
		[Tooltip("是否使用 Unity Physics2D 系统（Rigidbody2D + Collider2D）。\n" +
			"Plan A 角色（使用 PhysicsState ps）应设为 false。\n" +
			"Legacy 角色（使用 Unity 物理）保持 true。")]
		public bool UseUnityPhysics2D = true;

        [Tooltip("视为地面的层掩码")]
        public LayerMask GroundLayerMask = LayerManager.GroundLayerMask;

        [Tooltip("视为障碍物的层掩码（将阻止移动）")]
        public LayerMask ObstaclesLayerMask = LayerManager.ObstaclesLayerMask;

		public Vector2 ColliderSize
		{
			get
			{
				if (!_boxColliderNull)
				{
					return _boxCollider.size;
				}
				if (!_capsuleColliderNull)
				{
					return _capsuleCollider.size;
				}
				if (!_circleColliderNull)
				{
					return Vector2.one * _circleCollider.radius;
				}
				return Vector2.zero;
			}
			set
			{
				if (!_boxColliderNull)
				{
					_boxCollider.size = value;
					return;
				}
				if (!_capsuleColliderNull)
				{
					_capsuleCollider.size = value;
					return;
				}
				if (!_circleColliderNull)
				{
					_circleCollider.radius = value.x;
					return;
				}
			}
		}
        
		public Vector2 ColliderOffset
		{
			get
			{
				if (!_boxColliderNull)
				{
					return _boxCollider.offset;
				}
				if (!_capsuleColliderNull)
				{
					return _capsuleCollider.offset;
				}
				if (!_circleColliderNull)
				{
					return _circleCollider.offset;
				}
				return Vector2.zero;
			}
			set
			{
				if (!_boxColliderNull)
				{
					_boxCollider.offset = value;
					return;
				}
				if (!_capsuleColliderNull)
				{
					_capsuleCollider.offset = value;
					return;
				}
				if (!_circleColliderNull)
				{
					_circleCollider.offset = value;
					return;
				}
			}
		}
        
		public Bounds ColliderBounds
		{
			get
			{
				if (!_boxColliderNull)
				{
					return _boxCollider.bounds;
				}
				if (!_capsuleColliderNull)
				{
					return _capsuleCollider.bounds;
				}
				if (!_circleColliderNull)
				{
					return _circleCollider.bounds;
				}
				return new Bounds();
			}
		}

		protected Rigidbody2D _rigidBody;
		protected BoxCollider2D _boxCollider;
		protected bool _boxColliderNull;
		protected CapsuleCollider2D _capsuleCollider;
		protected bool _capsuleColliderNull;
		protected CircleCollider2D _circleCollider;
		protected bool _circleColliderNull;
		protected Vector2 _originalColliderSize;
		protected Vector3 _originalColliderCenter;
		protected Vector3 _originalSizeRaycastOrigin;
		protected Vector3 _orientedMovement;
		protected Collider2D _groundedTest;
		protected Vector3 _movingPlatformPositionLastFrame;

		// collision detection
		protected RaycastHit2D _raycastUp;
		protected RaycastHit2D _raycastDown;
		protected RaycastHit2D _raycastLeft;
		protected RaycastHit2D _raycastRight;


		/// <summary>
		/// On awake we grabd our components
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

			// Physics Plan A Step P3: 允许在没有 Rigidbody2D/Collider2D 的情况下初始化
			// FLF 角色使用 PhysicsState (ps) 而非 Unity 物理系统
			_rigidBody = GetComponent<Rigidbody2D>();
			_boxCollider = GetComponent<BoxCollider2D>();
			_capsuleCollider = GetComponent<CapsuleCollider2D>();
			_circleCollider = GetComponent<CircleCollider2D>();

			_boxColliderNull = _boxCollider == null;
			_capsuleColliderNull = _capsuleCollider == null;
			_circleColliderNull = _circleCollider == null;

			// 只在有碰撞体时缓存初始值
			if (!_boxColliderNull || !_capsuleColliderNull || !_circleColliderNull)
			{
				_originalColliderSize = ColliderSize;
				_originalColliderCenter = ColliderOffset;
			}
		}

		/// <summary>
		/// Determines whether or not this character is grounded
		///
		/// Step D6: Plan A 模式下不使用 Physics2D 查询
		/// grounded 状态应由 PhysicsState (ps) 或角色系统提供
		/// </summary>
		protected override void CheckIfGrounded()
        {
			// Step D6: Plan A 模式（UseUnityPhysics2D=false）彻底旁路
			if (!UseUnityPhysics2D)
			{
				// Plan A: 默认为 grounded（2D 侧滚游戏）
				// 如需精确判定：从 LF2CharacterAnimator.ps.y == 0 获取
				Grounded = true;
				JustGotGrounded = (!_groundedLastFrame && Grounded);
				_groundedLastFrame = Grounded;
				return;
			}

			// Legacy: Unity Physics2D 查询（需要 Collider2D）
			_groundedTest = Physics2D.OverlapPoint((Vector2)this.transform.position, GroundLayerMask);
			Grounded = (_groundedTest != null);
			JustGotGrounded = (!_groundedLastFrame && Grounded);
			// Step D6: 移除 Debug.LogError spam（避免 1000 AI 刷屏）
            _groundedLastFrame = Grounded;
		}

		/// <summary>
		/// On fixed update, we move our rigidbody
		///
		/// Step D6: Plan A 模式下完全跳过
		/// </summary>
		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			// Step D6: Plan A 模式（UseUnityPhysics2D=false）完全跳过
			// FLF 角色由 CharacterSim → LF2CharacterAnimator → PhysicsState (ps) 驱动
			if (!UseUnityPhysics2D)
			{
				return;
			}

			// Physics Plan A Step P3: 如果没有 Rigidbody2D，跳过物理更新
			// （向后兼容：Legacy 角色仍可能缺少 Rigidbody2D）
			if (_rigidBody == null)
			{
				return;
			}

			ApplyImpact();

			if (!FreeMovement)
			{
				return;
			}

			if (Friction > 1)
			{
				CurrentMovement = CurrentMovement / Friction;
			}

			// if we have a low friction (ice, marbles...) we lerp the speed accordingly
			if (Friction > 0 && Friction < 1)
			{
				CurrentMovement = Vector3.Lerp(Speed, CurrentMovement, Time.fixedDeltaTime * Friction);
			}

			Vector2 newMovement = _rigidBody.position + (Vector2)(CurrentMovement + AddedForce) * Time.fixedDeltaTime;

			
			_rigidBody.MovePosition(newMovement);

			ComputeNewVelocity();
			ComputeSpeed();
		}
		
		/// <summary>
		/// // 根据当前位置和上一帧的位置来确定新的速度值
		/// </summary>
		protected virtual void ComputeNewVelocity()
		{
			Velocity = (_rigidBody.transform.position - _positionLastFrame) / Time.fixedDeltaTime;
			Acceleration = (Velocity - VelocityLastFrame) / Time.fixedDeltaTime;
			VelocityLastFrame = Velocity;
		}

		/// <summary>
		/// On update we determine our acceleration
		/// </summary>
		protected override void Update()
		{
			base.Update();
		}

		/// <summary>
		/// On late update, we apply an impact
		/// </summary>
		protected override void LateUpdate()
		{
			base.LateUpdate();
		}

		/// <summary>
		/// Handles the friction, still a work in progress (todo)
		/// </summary>
		protected override void HandleFriction()
		{
			if (SurfaceModifierBelow == null)
			{
				Friction = 0f;
				AddedForce = Vector3.zero;
				return;
			}
			else
			{
				Friction = SurfaceModifierBelow.Friction;

				if (AddedForce.y != 0f)
				{
					AddForce(AddedForce);
				}

				AddedForce.y = 0f;
				AddedForce = SurfaceModifierBelow.AddedForce;
			}
		}

        /// <summary>
        /// 添加指定大小和方向的力。
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="force"></param>
        public override void Impact(Vector3 direction, float force)
		{
			direction = direction.normalized;
			_impact += direction.normalized * force;
		}

		/// <summary>
		/// Applies the current impact
		/// </summary>
		protected virtual void ApplyImpact()
		{
			if (_impact.magnitude > 0.2f)
			{
				_rigidBody.AddForce(_impact);
			}
			_impact = Vector3.Lerp(_impact, Vector3.zero, 5f * Time.fixedDeltaTime);
		}

		/// <summary>
		/// Adds a force of the specified vector
		/// </summary>
		/// <param name="movement"></param>
		public override void AddForce(Vector3 movement)
		{
			Impact(movement.normalized, movement.magnitude);
		}
        
		/// <summary>
		/// Sets the current movement
		/// </summary>
		/// <param name="movement"></param>
		public override void SetMovement(Vector3 movement)
		{
			_orientedMovement = movement;
			_orientedMovement.y = _orientedMovement.z;
			_orientedMovement.z = 0f;
			CurrentMovement = _orientedMovement;
		}

		/// <summary>
		/// Tries to move to the specified position
		/// </summary>
		/// <param name="newPosition"></param>
		public override void MovePosition(Vector3 newPosition, bool targetTransform = false)
		{
			if (targetTransform)
			{
				this.transform.position = newPosition;
			}
			else
			{
				// Physics Plan A Step P3: 如果没有 Rigidbody2D，回退到 transform
				if (_rigidBody != null)
				{
					_rigidBody.MovePosition(newPosition);
				}
				else
				{
					this.transform.position = newPosition;
				}
			}
		}
		
		/// <summary>
		/// Resizes the collider to the new size set in parameters
		/// </summary>
		/// <param name="newSize">New size.</param>
		public override void ResizeColliderHeight(float newHeight, bool translateCenter = false)
		{
			float newYOffset = _originalColliderCenter.y - (_originalColliderSize.y - newHeight) / 2;
			Vector2 newSize = ColliderSize;
			newSize.y = newHeight;
			ColliderSize = newSize;
			ColliderOffset = newYOffset * Vector3.up;
		}

		/// <summary>
		/// Returns the collider to its initial size
		/// </summary>
		public override void ResetColliderSize()
		{
			ColliderSize = _originalColliderSize;
			ColliderOffset = _originalColliderCenter;
		}
        
		/// <summary>
		/// Determines the controller's current direction
		/// </summary>
		protected override void DetermineDirection()
		{
			if (CurrentMovement != Vector3.zero)
			{
				CurrentDirection = CurrentMovement.normalized;
			}
		}

		/// <summary>
		/// Sets this rigidbody as kinematic
		/// </summary>
		/// <param name="state"></param>
		public override void SetKinematic(bool state)
		{
			// Physics Plan A Step P3: 如果没有 Rigidbody2D，忽略
			if (_rigidBody != null)
			{
				_rigidBody.isKinematic = state;
			}
		}

		/// <summary>
		/// Enables the collider
		/// </summary>
		public override void CollisionsOn()
		{
			if (!_boxColliderNull)
			{
				_boxCollider.enabled = true;
			}
			if (!_capsuleColliderNull)
			{
				_capsuleCollider.enabled = true;
			}
			if (!_circleColliderNull)
			{
				_circleCollider.enabled = true;
			}
		}

		/// <summary>
		/// Disables the collider
		/// </summary>
		public override void CollisionsOff()
		{
			if (!_boxColliderNull)
			{
				_boxCollider.enabled = false;
			}
			if (!_capsuleColliderNull)
			{
				_capsuleCollider.enabled = false;
			}
			if (!_circleColliderNull)
			{
				_circleCollider.enabled = false;
			}
		}

        /// <summary>
        /// Performs a cardinal collision check and stores collision objects informations
        /// 执行一个基本方向的碰撞检测，并将碰撞对象的信息存储起来。
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="offset"></param>
        public override void DetectObstacles(float distance, Vector3 offset)
		{
			if (!PerformCardinalObstacleRaycastDetection)
			{
				return;
			}
            
			CollidingWithCardinalObstacle = false;
			_raycastRight = MMDebug.RayCast(this.transform.position + offset, Vector3.right, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_raycastRight.collider != null) { DetectedObstacleRight = _raycastRight.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleRight = null; }
			_raycastLeft = MMDebug.RayCast(this.transform.position + offset, Vector3.left, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_raycastLeft.collider != null) { DetectedObstacleLeft = _raycastLeft.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleLeft = null; }
			_raycastUp = MMDebug.RayCast(this.transform.position + offset, Vector3.up, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_raycastUp.collider != null) { DetectedObstacleUp = _raycastUp.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleUp = null; }
			_raycastDown = MMDebug.RayCast(this.transform.position + offset, Vector3.down, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_raycastDown.collider != null) { DetectedObstacleDown = _raycastDown.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleDown = null; }
		}
		

		/// <summary>
		/// On reset, we reset our rb's velocity
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			if (_rigidBody != null)
			{
				_rigidBody.velocity = Vector2.zero;	
			}
		}
	}
}