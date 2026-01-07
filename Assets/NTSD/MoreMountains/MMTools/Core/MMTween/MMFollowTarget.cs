using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this component to an object and it'll get moved towards the target at update, with or without interpolation based on your settings
	/// 将这个组件添加到一个对象上，它将在更新时向目标移动，根据你的设置，可以选择是否带有插值。
	/// </summary>
	[AddComponentMenu("ThirdParty/More Mountains/Tools/Movement/MM Follow Target")]
	public class MMFollowTarget : MonoBehaviour
	{
        /// 可能的更新模式
        public enum UpdateModes { Update, FixedUpdate, LateUpdate }
        /// 可能的跟随模式
        public enum FollowModes { RegularLerp, MMLerp, MMSpring }
        /// 是否在世界空间或局部空间中操作
        public enum PositionSpaces { World, Local }

        [Header("Follow Position")]
        /// 是否当前对象正在跟随其目标的位置
        [Tooltip("是否当前对象正在跟随其目标的位置")]
        public bool FollowPosition = true;
        /// 此对象是否应该在X轴上跟随其目标
        [MMCondition("FollowPosition", true)]
        [Tooltip("此对象是否应该在X轴上跟随其目标")]
        public bool FollowPositionX = true;
        /// 此对象是否应该在Y轴上跟随其目标
        [MMCondition("FollowPosition", true)]
        [Tooltip("此对象是否应该在Y轴上跟随其目标")]
        public bool FollowPositionY = true;
        /// 此对象是否应该在Z轴上跟随其目标
        [MMCondition("FollowPosition", true)]
        [Tooltip("此对象是否应该在Z轴上跟随其目标")]
        public bool FollowPositionZ = true;
        /// 是否在世界空间或局部空间中操作
        [MMCondition("FollowPosition", true)]
        [Tooltip("是否在世界空间或局部空间中操作")]
        public PositionSpaces PositionSpace = PositionSpaces.World;


        [Header("Follow Rotation")]
        /// 是否当前对象正在跟随其目标的旋转
        public bool FollowRotation = true;

        [Header("跟随缩放")]
		/// 是否当前对象正在跟随其目标的缩放
		[Tooltip("是否当前对象正在跟随其目标的缩放")]
        public bool FollowScale = true;
        /// 在跟随时应用的缩放因子
        [MMCondition("FollowScale", true)]
		[Tooltip("在跟随时应用的缩放因子")]
        public float FollowScaleFactor = 1f;

        [Header("Target")]
        /// 要跟随的目标
        public Transform Target;
        /// 应用到被跟随目标的偏移量
        [MMCondition("FollowPosition", true)]
        [Tooltip("应用到被跟随目标的偏移量")]
        public Vector3 Offset;
        /// 是否将初始的X距离添加到X偏移量
        [MMCondition("FollowPosition", true)]
        [Tooltip("是否将初始的X距离添加到X偏移量")]
        public bool AddInitialDistanceXToXOffset = false;
        /// 是否将初始的Y距离添加到Y偏移量
        [MMCondition("FollowPosition", true)]
        [Tooltip("是否将初始的Y距离添加到Y偏移量")]
        public bool AddInitialDistanceYToYOffset = false;
        /// 是否将初始的Z距离添加到Z偏移量
        [MMCondition("FollowPosition", true)]
        [Tooltip("是否将初始的Z距离添加到Z偏移量")]
        public bool AddInitialDistanceZToZOffset = false;

        [Header("Position Interpolation")]
        /// 是否需要对移动进行插值
		[Tooltip("是否需要对移动进行插值")]
        public bool InterpolatePosition = true;
        /// 在跟随位置时使用的跟随模式
        [MMCondition("InterpolatePosition", true)]
        [Tooltip("在跟随位置时使用的跟随模式")]
        public FollowModes FollowPositionMode = FollowModes.MMLerp;
        /// 跟随者移动插值的速度
        [MMCondition("InterpolatePosition", true)]
        [Tooltip("跟随者移动插值的速度")]
        public float FollowPositionSpeed = 10f;
        /// 较高的值意味着更多的阻尼，更少的弹簧效果，较低的值意味着更少的阻尼，更多的弹簧效果
        [MMEnumCondition("FollowPositionMode", (int)FollowModes.MMSpring)]
        [Range(0.01f, 1.0f)]
        [Tooltip("较高的值意味着更多的阻尼，更少的弹簧效果，较低的值意味着更少的阻尼，更多的弹簧效果")]
        public float PositionSpringDamping = 0.3f;
        /// 弹簧“振动”的频率，单位为赫兹（Hz）（1：弹簧将在一秒钟内完成一个完整的周期）
        [MMEnumCondition("FollowPositionMode", (int)FollowModes.MMSpring)]
        [Tooltip("弹簧“振动”的频率，单位为赫兹（Hz）（1：弹簧将在一秒钟内完成一个完整的周期）")]
        public float PositionSpringFrequency = 3f;

        [Header("Rotation Interpolation")]
        /// 是否需要对旋转进行插值
		[Tooltip("是否需要对旋转进行插值")]
        public bool InterpolateRotation = true;
        /// 在插值旋转时使用的跟随模式
        [MMCondition("InterpolateRotation", true)]
        [Tooltip("在插值旋转时使用的跟随模式")]
        public FollowModes FollowRotationMode = FollowModes.MMLerp;
        /// 跟随者旋转插值的速度
        [MMCondition("InterpolateRotation", true)]
        [Tooltip("跟随者旋转插值的速度")]
        public float FollowRotationSpeed = 10f;
        /// 较高的值意味着更多的阻尼，更少的弹簧效果，较低的值意味着更少的阻尼，更多的弹簧效果
        [MMEnumCondition("FollowRotationMode", (int)FollowModes.MMSpring)]
        [Range(0.01f, 1.0f)]
        [Tooltip("较高的值意味着更多的阻尼，更少的弹簧效果，较低的值意味着更少的阻尼，更多的弹簧效果")]
        public float RotationSpringDamping = 0.3f;
        /// 弹簧“振动”的频率，单位为赫兹（Hz）（1：弹簧将在一秒钟内完成一个完整的周期）
        [MMEnumCondition("FollowRotationMode", (int)FollowModes.MMSpring)]
        [Tooltip("弹簧“振动”的频率，单位为赫兹（Hz）（1：弹簧将在一秒钟内完成一个完整的周期）")]
        public float RotationSpringFrequency = 3f;

        [Header("Scale Interpolation")]
		/// 是否需要对缩放进行插值
		[Tooltip("是否需要对缩放进行插值")]
		public bool InterpolateScale = true;
        /// 在插值缩放时使用的跟随模式
        [MMCondition("InterpolateScale", true)]
		[Tooltip("在插值缩放时使用的跟随模式")]
		public FollowModes FollowScaleMode = FollowModes.MMLerp;
        /// 跟随者缩放插值的速度
        [MMCondition("InterpolateScale", true)]
		[Tooltip("跟随者缩放插值的速度")]
		public float FollowScaleSpeed = 10f;
        /// 较高的值意味着更多的阻尼，更少的弹簧效果，较低的值意味着更少的阻尼，更多的弹簧效果
        [MMEnumCondition("FollowScaleMode", (int)FollowModes.MMSpring)]
        [Range(0.01f, 1.0f)]
		[Tooltip("高的值意味着更多的阻尼，更少的弹簧效果，较低的值意味着更少的阻尼，更多的弹簧效果")]
        public float ScaleSpringDamping = 0.3f;
        /// 弹簧“振动”的频率，单位为赫兹（Hz）（1：弹簧将在一秒钟内完成一个完整的周期）
        [MMEnumCondition("FollowScaleMode", (int)FollowModes.MMSpring)]
		[Tooltip("弹簧“振动”的频率，单位为赫兹（Hz）（1：弹簧将在一秒钟内完成一个完整的周期）")]
        public float ScaleSpringFrequency = 3f;

        [Header("Mode")]
        /// 移动发生时的更新模式
		[Tooltip("移动发生时的更新模式")]
        public UpdateModes UpdateMode = UpdateModes.Update;
		/// 如果为真，当其宿主游戏对象被禁用时，该组件将自动禁用自身
		[Tooltip("如果为真，当其宿主游戏对象被禁用时，该组件将自动禁用自身")]
		public bool DisableSelfOnSetActiveFalse = false;

        [Header("Distances")]
		/// 是否在开始跟随之前强制对象与其目标之间保持最小距离
		[Tooltip("是否在开始跟随之前强制对象与其目标之间保持最小距离")]
		public bool UseMinimumDistanceBeforeFollow = false;
		/// 需要在对象与其目标之间保持的最小距离
		[Tooltip("需要在对象与其目标之间保持的最小距离")]
		public float MinimumDistanceBeforeFollow = 1f;
		/// 是否希望确保对象永远不会离其目标太远
		[Tooltip("是否希望确保对象永远不会离其目标太远")]
		public bool UseMaximumDistance = false;
		/// 对象可以远离其目标的最大距离
		[Tooltip("对象可以远离其目标的最大距离")]
		public float MaximumDistance = 1f;

        [Header("Anchor")]
		/// 如果为真，移动将被限制在初始位置周围
		[Tooltip("移动将被限制在初始位置周围")]
		public bool AnchorToInitialPosition;
        /// 变换可以在初始位置周围移动的最大距离
        [MMCondition("AnchorToInitialPosition", true)]
		[Tooltip("变换可以在初始位置周围移动的最大距离")]
        public float MaxDistanceToAnchor = 1f;


        protected bool _localSpace { get { return PositionSpace == PositionSpaces.Local; } }

		protected Vector3 _positionVelocity = Vector3.zero;
		protected Vector3 _scaleVelocity = Vector3.zero;    
		protected Vector3 _rotationVelocity = Vector3.zero;    
		
		protected Vector3 _initialPosition;
		protected Vector3 _direction;
		
		protected Vector3 _newPosition;
		protected Vector3 _newRotation;
		protected Vector3 _newScale;
		
		protected Vector3 _newTargetPosition;    
		protected Quaternion _newTargetRotation;
		protected Vector3 _newTargetRotationEulerAngles;
		protected Vector3 _newTargetRotationEulerAnglesLastFrame;
		protected Vector3 _newTargetScale;

		protected float _rotationFloatVelocity;
		protected float _rotationFloatCurrent;
		protected float _rotationFloatTarget;

		protected Vector3 _currentRotationEulerAngles;
		protected Quaternion _rotationBeforeSpring;
		
		protected Quaternion _initialRotation;
		protected Vector3 _lastTargetPosition;
        
		/// <summary>
		/// On start we store our initial position
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Initializes the follow
		/// </summary>
		public virtual void Initialization()
		{
			SetInitialPosition();
			SetOffset();
		}

		/// <summary>
		/// Prevents the object from following the target anymore
		/// </summary>
		public virtual void StopFollowing()
		{
			FollowPosition = false;
		}

		/// <summary>
		/// Makes the object follow the target
		/// </summary>
		public virtual void StartFollowing()
		{
			FollowPosition = true;
			SetInitialPosition();
		}

		/// <summary>
		/// Stores the initial position
		/// </summary>
		protected virtual void SetInitialPosition()
		{
			_initialPosition = _localSpace ? this.transform.localPosition : this.transform.position;
			_initialRotation = this.transform.rotation;
			_lastTargetPosition = _localSpace ? this.transform.localPosition : this.transform.position;
		}

		/// <summary>
		/// Adds initial offset to the offset if needed
		/// </summary>
		protected virtual void SetOffset()
		{
			if (Target == null)
			{
				return;
			}
			Vector3 difference = this.transform.position - Target.transform.position;
			Offset.x = AddInitialDistanceXToXOffset ? difference.x : Offset.x;
			Offset.y = AddInitialDistanceYToYOffset ? difference.y : Offset.y;
			Offset.z = AddInitialDistanceZToZOffset ? difference.z : Offset.z;
		}

		/// <summary>
		/// At update we follow our target 
		/// </summary>
		protected virtual void Update()
		{
			if (Target == null)
			{
				return;
			}
			if (UpdateMode == UpdateModes.Update)
			{
				FollowTargetRotation();
				FollowTargetScale();
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// At fixed update we follow our target 
		/// </summary>
		protected virtual void FixedUpdate()
		{
			if (UpdateMode == UpdateModes.FixedUpdate)
			{
				FollowTargetRotation();
				FollowTargetScale();
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// At late update we follow our target 
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (UpdateMode == UpdateModes.LateUpdate)
			{
				FollowTargetRotation();
				FollowTargetScale();
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// Follows the target, lerping the position or not based on what's been defined in the inspector
		/// </summary>
		protected virtual void FollowTargetPosition()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowPosition)
			{
				return;
			}

			_newTargetPosition = Target.position + Offset;
			if (!FollowPositionX) { _newTargetPosition.x = _initialPosition.x; }
			if (!FollowPositionY) { _newTargetPosition.y = _initialPosition.y; }
			if (!FollowPositionZ) { _newTargetPosition.z = _initialPosition.z; }

			float trueDistance = 0f;
			_direction = (_newTargetPosition - this.transform.position).normalized;
			trueDistance = Vector3.Distance(this.transform.position, _newTargetPosition);
            
			float interpolatedDistance = trueDistance;
			if (InterpolatePosition)
			{
				switch (FollowPositionMode)
				{
					case FollowModes.MMLerp:
						interpolatedDistance = MMMaths.Lerp(0f, trueDistance, FollowPositionSpeed, Time.deltaTime);
						interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
						this.transform.Translate(_direction * interpolatedDistance, Space.World);
						break;
					case FollowModes.RegularLerp:
						interpolatedDistance = Mathf.Lerp(0f, trueDistance, Time.deltaTime * FollowPositionSpeed);
						interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
						this.transform.Translate(_direction * interpolatedDistance, Space.World);
						break;
					case FollowModes.MMSpring:
						_newPosition = this.transform.position;
						MMMaths.Spring(ref _newPosition, _newTargetPosition, ref _positionVelocity, PositionSpringDamping, PositionSpringFrequency, Time.deltaTime);
						if (_localSpace)
						{
							this.transform.localPosition = _newPosition;   
						}
						else
						{
							this.transform.position = _newPosition;    
						}
						break;
				}                
			}
			else
			{
				interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
				this.transform.Translate(_direction * interpolatedDistance, Space.World);
			}

			if (AnchorToInitialPosition)
			{
				if (Vector3.Distance(this.transform.position, _initialPosition) > MaxDistanceToAnchor)
				{
					if (_localSpace)
					{
						this.transform.localPosition = _initialPosition + Vector3.ClampMagnitude(this.transform.localPosition - _initialPosition, MaxDistanceToAnchor);   
					}
					else
					{
						this.transform.position = _initialPosition + Vector3.ClampMagnitude(this.transform.position - _initialPosition, MaxDistanceToAnchor);    
					}
				}
			}
		}

		/// <summary>
		/// Applies minimal and maximal distance rules to the interpolated distance
		/// </summary>
		/// <param name="trueDistance"></param>
		/// <param name="interpolatedDistance"></param>
		/// <returns></returns>
		protected virtual float ApplyMinMaxDistancing(float trueDistance, float interpolatedDistance)
		{
			if (UseMinimumDistanceBeforeFollow && (trueDistance - interpolatedDistance < MinimumDistanceBeforeFollow))
			{
				interpolatedDistance = 0f;
			}

			if (UseMaximumDistance && (trueDistance - interpolatedDistance >= MaximumDistance))
			{
				interpolatedDistance = trueDistance - MaximumDistance;
			}

			return interpolatedDistance;
		}
		
		/// <summary>
		/// Makes the object follow its target's rotation
		/// </summary>
		protected virtual void FollowTargetRotation()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowRotation)
			{
				return;
			}

			_newTargetRotation = Target.rotation;
			
			_newTargetRotationEulerAngles = Target.rotation.eulerAngles;
			_currentRotationEulerAngles = this.transform.rotation.eulerAngles;
			
			if (FollowRotationMode == FollowModes.MMSpring && (_newTargetRotationEulerAnglesLastFrame != _newTargetRotationEulerAngles))
			{
				_rotationBeforeSpring = this.transform.rotation;
				_rotationFloatCurrent = 0f;
				_rotationFloatTarget = (Mathf.Abs(_newTargetRotation.eulerAngles.x)
				                        + Mathf.Abs(_newTargetRotation.eulerAngles.y)
				                        + Mathf.Abs(_newTargetRotation.z))
				                       -
				                       (Mathf.Abs(_currentRotationEulerAngles.x)
				                        + Mathf.Abs(_currentRotationEulerAngles.y)
				                        + Mathf.Abs(_currentRotationEulerAngles.z));

				_rotationFloatTarget = Mathf.Abs(_rotationFloatTarget);
			}

			if (InterpolateRotation)
			{
				switch (FollowRotationMode)
				{
					case FollowModes.MMLerp:
						this.transform.rotation = MMMaths.Lerp(this.transform.rotation, _newTargetRotation, FollowRotationSpeed, Time.deltaTime);
						break;
					case FollowModes.RegularLerp:
						this.transform.rotation = Quaternion.Lerp(this.transform.rotation, _newTargetRotation, Time.deltaTime * FollowRotationSpeed);
						break;
					case FollowModes.MMSpring:
						if (_rotationFloatCurrent == _rotationFloatTarget)
						{
							break;
						}
						MMMaths.Spring(ref _rotationFloatCurrent, _rotationFloatTarget, ref _rotationFloatVelocity, RotationSpringDamping, RotationSpringFrequency, Time.deltaTime);
						float lerpValue = MMMaths.Remap(_rotationFloatCurrent, 0f, _rotationFloatTarget, 0f, 1f);
						this.transform.rotation = Quaternion.LerpUnclamped(_rotationBeforeSpring, _newTargetRotation, lerpValue );   
						break;
				}
			}
			else
			{
				this.transform.rotation = _newTargetRotation;
			}

			_newTargetRotationEulerAnglesLastFrame = _newTargetRotationEulerAngles;
		}

		/// <summary>
		/// Makes the object follow its target's scale
		/// </summary>
		protected virtual void FollowTargetScale()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowScale)
			{
				return;
			}

			_newTargetScale = Target.localScale * FollowScaleFactor;

			if (InterpolateScale)
			{
				switch (FollowScaleMode)
				{
					case FollowModes.MMLerp:
						this.transform.localScale = MMMaths.Lerp(this.transform.localScale, _newTargetScale, FollowScaleSpeed, Time.deltaTime);
						break;
					case FollowModes.RegularLerp:
						this.transform.localScale = Vector3.Lerp(this.transform.localScale, _newTargetScale, Time.deltaTime * FollowScaleSpeed);
						break;
					case FollowModes.MMSpring:
						_newScale = this.transform.localScale;
						MMMaths.Spring(ref _newScale, _newTargetScale, ref _scaleVelocity, ScaleSpringDamping, ScaleSpringFrequency, Time.deltaTime);
						this.transform.localScale = _newScale;   
						break;
				}
			}
			else
			{
				this.transform.localScale = _newTargetScale;
			}
		}
        
		public virtual void ChangeFollowTarget(Transform newTarget) => Target = newTarget;

		protected virtual void OnDisable()
		{
			if (DisableSelfOnSetActiveFalse)
			{
				this.enabled = false;
			}
		}
	}
}