using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Do not use this class directly, use TopDownController2D for 2D characters, or TopDownController3D for 3D characters
    /// Both of these classes inherit from this one
    /// 不要直接使用这个类，请为2D角色使用`TopDownController2D`，或者为3D角色使用`TopDownController3D`。///这两个类都继承自此类。
    /// </summary>
    public abstract class  TopDownController : TopDownMonoBehaviour 
	{
		[Header("Gravity")]

        [Tooltip("应用于我们角色的当前重力（正值向下，负值向上，值越高，加速度越大）")]
        public float Gravity = 40f;

        [Tooltip("是否当前正在对此角色应用重力")]
        public bool GravityActive = true;

        [Header("General Raycasts")]

        [Tooltip("默认情况下，用于恢复正常大小的射线长度将根据角色的常规/站立高度自动生成，但在这里你可以指定一个不同的值")]
        public float CrouchedRaycastLengthMultiplier = 1f;

        [Tooltip("如果为真，将在所有4个侧面额外投射射线以检测障碍物，并更新CollidingWithCardinalObstacle布尔值，仅在处理网格移动时或出于某种原因需要该信息时有用")]
        public bool PerformCardinalObstacleRaycastDetection = false;

        /// the current speed of the character
        [MMReadOnly]
        [Tooltip("角色当前的速度")]
		public Vector3 Speed;

        [MMReadOnly]
        [Tooltip("每秒的当前速度")]
        public Vector3 Velocity;

        [MMReadOnly]
        [Tooltip("上一帧角色的速度")]
        public Vector3 VelocityLastFrame;

        [MMReadOnly]
        [Tooltip("当前的加速度")]
        public Vector3 Acceleration;

        [MMReadOnly]
        [Tooltip("角色是否着地")]
        public bool Grounded;

        [MMReadOnly]
        [Tooltip("角色是否在本帧着地")]
        public bool JustGotGrounded;

        [MMReadOnly]
        [Tooltip("角色当前的移动")]
        public Vector3 CurrentMovement;

        [MMReadOnly]
        [Tooltip("角色前进的方向")]
        public Vector3 CurrentDirection;

        [MMReadOnly]
        [Tooltip("当前的摩擦力")]
        public float Friction;

        [MMReadOnly]
        [Tooltip("当前添加的力，将被添加到角色的移动中")]
        public Vector3 AddedForce;

        [MMReadOnly]
        [Tooltip("角色是否处于自由移动模式")]
        public bool FreeMovement = true;

        /// 碰撞器的中心坐标
        public virtual Vector3 ColliderCenter { get { return Vector3.zero; } }
        /// 碰撞器的底部坐标
        public virtual Vector3 ColliderBottom { get { return Vector3.zero; } }
        /// 碰撞器的顶部坐标
        public virtual Vector3 ColliderTop { get { return Vector3.zero; } }
        /// 我们角色下方的对象（如果有）
        public virtual GameObject ObjectBelow { get; set; }
        /// 我们角色下方的表面修改器对象（如果有）
        public virtual SurfaceModifier SurfaceModifierBelow { get; set; }
        public virtual Vector3 AppliedImpact { get { return _impact; } }
        /// 移动平台的速度
        public virtual Vector3 MovingPlatformSpeed { get; set; }

        // 此控制器左侧的障碍物（只有在调用DetectObstacles时才会更新）
        public virtual GameObject DetectedObstacleLeft { get; set; }
        // 此控制器右侧的障碍物（只有在调用DetectObstacles时才会更新）
        public virtual GameObject DetectedObstacleRight { get; set; }
        // 此控制器上方的障碍物（只有在调用DetectObstacles时才会更新）
        public virtual GameObject DetectedObstacleUp { get; set; }
        // 此控制器下方的障碍物（只有在调用DetectObstacles时才会更新）
        public virtual GameObject DetectedObstacleDown { get; set; }
        // 如果在任何基本方向上检测到障碍物，则为true
        public virtual bool CollidingWithCardinalObstacle { get; set; }

        protected Vector3 _positionLastFrame;
		protected Vector3 _speedComputation;
		protected bool _groundedLastFrame;
		protected Vector3 _impact;		
		protected const float _smallValue=0.0001f;

		/// <summary>
		/// On awake, we initialize our current direction
		/// </summary>
		protected virtual void Awake()
		{			
			CurrentDirection = transform.forward;
		}

		/// <summary>
		/// On update, we check if we're grounded, and determine the direction
		/// </summary>
		protected virtual void Update()
		{
            CheckIfGrounded();
            HandleFriction();
            DetermineDirection();
        }

        /// <summary>
        /// 计算速度
        /// </summary>
        protected virtual void ComputeSpeed ()
		{
			if (Time.deltaTime != 0f)
			{
				Speed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
			}			
			// we round the speed to 2 decimals
			Speed.x = Mathf.Round(Speed.x * 100f) / 100f;
			Speed.y = Mathf.Round(Speed.y * 100f) / 100f;
			Speed.z = Mathf.Round(Speed.z * 100f) / 100f;
			_positionLastFrame = this.transform.position;
		}

        /// <summary>
        /// 确定控制器的当前方向
        /// </summary>
        protected virtual void DetermineDirection()
		{
			
		}

        /// <summary>
        /// 手动执行障碍物检测
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="offset"></param>
        public virtual void DetectObstacles(float distance, Vector3 offset)
		{

		}

		/// <summary>
		/// Called at FixedUpdate
		/// </summary>
		protected virtual void FixedUpdate()
		{

		}

		/// <summary>
		/// On LateUpdate, computes the speed of the agent
		/// </summary>
		protected virtual void LateUpdate()
		{
		}

        /// <summary>
        /// 检查角色是否着地
        /// </summary>
        protected virtual void CheckIfGrounded()
		{
			JustGotGrounded = (!_groundedLastFrame && Grounded);
			_groundedLastFrame = Grounded;
		}

        /// <summary>
        /// 用此方法对控制器施加冲击，使其以指定的方向和力度移动
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="force"></param>
        public virtual void Impact(Vector3 direction, float force)
		{

		}

        /// <summary>
        /// 设置重力是否激活
        /// </summary>
        /// <param name="status"></param>
        public virtual void SetGravityActive(bool status)
		{
			GravityActive = status;
		}

        /// <summary>
        /// 向控制器添加指定的力
        /// </summary>
        /// <param name="movement"></param>
        public virtual void AddForce(Vector3 movement)
		{

		}

        /// <summary>
        ///  将控制器的当前移动设置为指定的Vector3
        /// </summary>
        /// <param name="movement"></param>
        public virtual void SetMovement(Vector3 movement)
		{

		}

        /// <summary>
        /// / 将控制器移动到指定位置（世界空间）
        /// </summary>
        /// <param name="newPosition"></param>
        public virtual void MovePosition(Vector3 newPosition, bool targetTransform = false)
		{
			
		}

        /// <summary>
        ///  调整控制器的碰撞器大小
        /// </summary>
        /// <param name="newHeight"></param>
        public virtual void ResizeColliderHeight(float newHeight, bool translateCenter = false)
		{

		}

		/// <summary>
		/// Resets the controller's collider size
		/// </summary>
		public virtual void ResetColliderSize()
		{

		}

        /// <summary>
        /// 如果控制器的碰撞器可以在不碰到障碍物的情况下恢复到原始大小，则返回`true`，否则返回`false`。
        /// </summary>
        /// <returns></returns>
        public virtual bool CanGoBackToOriginalSize()
		{
			return true;
		}

		/// <summary>
		/// Turns the controller's collisions on
		/// </summary>
		public virtual void CollisionsOn()
		{

		}

		/// <summary>
		/// Turns the controller's collisions off
		/// </summary>
		public virtual void CollisionsOff()
		{

		}

		/// <summary>
		/// Sets the controller's rigidbody to Kinematic (or not kinematic)
		/// </summary>
		/// <param name="state"></param>
		public virtual void SetKinematic(bool state)
		{

		}

		/// <summary>
		/// Handles friction collisions
		/// </summary>
		protected virtual void HandleFriction()
		{

		}

		/// <summary>
		/// Resets all values for this controller
		/// </summary>
		public virtual void Reset()
		{
			_impact = Vector3.zero;
			GravityActive = true;
			Speed = Vector3.zero;
			Velocity = Vector3.zero;
			VelocityLastFrame = Vector3.zero;
			Acceleration = Vector3.zero;
			Grounded = true;
			JustGotGrounded = false;
			CurrentMovement = Vector3.zero;
			CurrentDirection = Vector3.zero;
			AddedForce = Vector3.zero;
		}
	}
}