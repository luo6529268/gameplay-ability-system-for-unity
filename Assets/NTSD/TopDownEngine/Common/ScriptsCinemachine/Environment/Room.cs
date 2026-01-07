using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// This class lets you define the boundaries of rooms in your level.
    /// Rooms are useful if you want to cut your level into portions (think Super Metroid or Hollow Knight for example).
    /// These rooms will require their own virtual camera, and a confiner to define their size. 
    /// Note that the confiner is different from the collider that defines the room.
    /// You can see an example of rooms in action in the KoalaRooms demo scene.
    /// 这个类允许你定义你关卡中的房间边界。
    /// 房间很有用，如果你想将你的关卡分割成部分（想想超级银河战士或空洞骑士）。
    /// 这些房间将需要自己的虚拟相机，和一个定义它们大小的围栏。
    /// 注意，围栏与定义房间的碰撞器是不同的。
    /// 你可以在KoalaRooms演示场景中看到房间的实际例子。
    /// </summary>
    public class Room : TopDownMonoBehaviour, MMEventListener<TopDownEngineEvent>//,MMEventListener<RoomEvent>
	{
		public enum Modes { TwoD, ThreeD }

        /// the collider for this room <summary>
        /// 这个房间的碰撞器
        /// </summary>
        public Vector3 RoomColliderCenter
		{
			get
			{
				if (_roomCollider2D != null)
				{
					return _roomCollider2D.bounds.center;
				}
				else
				{
					return _roomCollider.bounds.center;
				}
			}
		}
        
		public Vector3 RoomColliderSize
		{
			get
			{
				if (_roomCollider2D != null)
				{
					return _roomCollider2D.bounds.size;
				}
				else
				{
					return _roomCollider.bounds.size;
				}
			}
		}

		public Bounds RoomBounds
		{
			get
			{
				if (_roomCollider2D != null)
				{
					return _roomCollider2D.bounds;
				}
				else
				{
					return _roomCollider.bounds;
				}
			}
		}

		[Header("Mode")]
        /// 这个房间是否打算在2D或3D模式下工作
        [Tooltip("这个房间是否打算在2D或3D模式下工作")]
        public Modes Mode = Modes.TwoD;

#if MM_CINEMACHINE
		[Header("Camera")]
		/// 与这个房间关联的虚拟相机
        [Tooltip("与这个房间关联的虚拟相机")]
        public CinemachineVirtualCamera VirtualCamera;
        /// 这个房间的围栏，将限制虚拟相机，通常放置在Room的子对象上
        [Tooltip("这个房间的围栏，将限制虚拟相机，通常放置在Room的子对象上")]
        public BoxCollider Confiner;
        /// 虚拟相机的围栏组件
        [Tooltip("虚拟相机的围栏组件")]
        public CinemachineConfiner CinemachineCameraConfiner;
#elif MM_CINEMACHINE3
        [Header("Camera")]
        /// 与这个房间关联的虚拟相机
        [Tooltip("与这个房间关联的虚拟相机")]
        public CinemachineCamera VirtualCamera;
        /// 这个房间的2D围栏，将限制虚拟相机，通常放置在Room的子对象上
        [Tooltip("这个房间的2D围栏，将限制虚拟相机，通常放置在Room的子对象上")]
        public BoxCollider2D Confiner2D;
        /// 这个房间的3D围栏，将限制虚拟相机，通常放置在Room的子对象上
        [Tooltip("这个房间的3D围栏，将限制虚拟相机，通常放置在Room的子对象上")]
        public BoxCollider Confiner3D;
        /// 虚拟相机的2D围栏组件
        [Tooltip("虚拟相机的2D围栏组件")]
        public CinemachineConfiner2D CinemachineCameraConfiner2D;
        /// 虚拟相机的3D围栏组件
        [Tooltip("虚拟相机的3D围栏组件")]
        public CinemachineConfiner3D CinemachineCameraConfiner3D;
#endif
        /// 是否应该在开始时自动调整围栏大小以匹配相机的大小和比例
        [Tooltip("是否应该在开始时自动调整围栏大小以匹配相机的大小和比例")]
        public bool ResizeConfinerAutomatically = true;
        /// 是否应该在开始时让这个房间查看关卡的起始位置，并声明自己为当前房间
        [Tooltip("是否应该在开始时让这个房间查看关卡的起始位置，并声明自己为当前房间")]
        public bool AutoDetectFirstRoomOnStart = true;
        /// 房间的深度（用于调整围栏的z值）
        [MMEnumCondition("Mode", (int)Modes.TwoD)]
        [Tooltip("房间的深度（用于调整围栏的z值）")]
        public float RoomDepth = 100f;

		[Header("Level")]
		public int LevelID = 0;

        [Header("StateNode")]
        /// 这个房间是否是当前房间
        [Tooltip("这个房间是否是当前房间")]
        public bool CurrentRoom = false;
        /// 这个房间是否已经被访问过
        [Tooltip("这个房间是否已经被访问过")]
        public bool RoomVisited = false;

        [Header("Actions")]
        /// 玩家首次进入房间时触发的事件
        [Tooltip("玩家首次进入房间时触发的事件")]
        public UnityEvent OnPlayerEntersRoomForTheFirstTime;
        /// 每次玩家进入房间时触发的事件
        [Tooltip("每次玩家进入房间时触发的事件")]
        public UnityEvent OnPlayerEntersRoom;
        /// 每次玩家退出房间时触发的事件
        [Tooltip("每次玩家退出房间时触发的事件")]
        public UnityEvent OnPlayerExitsRoom;

        [Header("Activation")]
        /// 进入房间时启用，退出房间时禁用的游戏对象列表
        [Tooltip("进入房间时启用，退出房间时禁用的游戏对象列表")]
        public List<GameObject> ActivationList;

        protected Collider _roomCollider;
		protected Collider2D _roomCollider2D;
		protected Camera _mainCamera;
		protected Vector2 _cameraSize;
		protected bool _initialized = false;

		/// <summary>
		/// On Awake we reset our camera's priority
		/// </summary>
		protected virtual void Awake()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (VirtualCamera != null)
			{
				VirtualCamera.Priority = 0;	
			}
			#endif
		}
        
		/// <summary>
		/// On Start we initialize our room
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// Grabs our Room collider, our main camera, and starts the confiner resize
        /// 获取房间碰撞器，主相机，并开始调整围栏大小
        /// </summary>
        protected virtual void Initialization()
		{
			if (_initialized)
			{
				return;
			}
			_roomCollider = this.gameObject.GetComponent<Collider>();
			_roomCollider2D = this.gameObject.GetComponent<Collider2D>();
			_mainCamera = Camera.main;          
			StartCoroutine(ResizeConfiner());
			_initialized = true;
		}

		/// <summary>
		/// Resizes the confiner 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ResizeConfiner()
		{
#if MM_CINEMACHINE
			if ((VirtualCamera == null) || (Confiner == null) || !ResizeConfinerAutomatically)
			{
				yield break;
			}

			// we wait two more frame for Unity's pixel perfect camera component to be ready because apparently sending events is not a thing.
			//我们等待两帧，以便于Unity的像素完美相机组件准备就绪，因为显然发送事件不是一件事。
			yield return null;
			yield return null;

			Confiner.transform.position = RoomColliderCenter;
			Vector3 size = RoomColliderSize;

			switch (Mode)
			{
				case Modes.TwoD:
					size.z = RoomDepth;
					Confiner.size = size;
					_cameraSize.y = 2 * _mainCamera.orthographicSize;
					_cameraSize.x = _cameraSize.y * _mainCamera.aspect;

					Vector3 newSize = Confiner.size;

					if (Confiner.size.x < _cameraSize.x)
					{
						newSize.x = _cameraSize.x;
					}
					if (Confiner.size.y < _cameraSize.y)
					{
						newSize.y = _cameraSize.y;
					}

					Confiner.size = newSize;
					break;
				case Modes.ThreeD:
					Confiner.size = size;
					break;
			}
            
			CinemachineCameraConfiner.InvalidatePathCache();
#elif MM_CINEMACHINE3
            if ((VirtualCamera == null) || ((Confiner2D == null) && (Confiner3D == null)) || !ResizeConfinerAutomatically)
			{
				yield break;
			}

            // we wait two more frame for Unity's pixel perfect camera component to be ready because apparently sending events is not a thing.
            //我们等待两帧，以便于Unity的像素完美相机组件准备就绪，因为显然发送事件不是一件事。
            yield return null;
			yield return null;

			if (Confiner2D != null)
			{
				Confiner2D.transform.position = RoomColliderCenter;	
			}

			if (Confiner3D != null)
			{
				Confiner3D.transform.position = RoomColliderCenter;	
			}
			
			Vector3 size = RoomColliderSize;

			switch (Mode)
			{
				case Modes.TwoD:
					size.z = RoomDepth;
					Confiner2D.size = size;
					_cameraSize.y = 2 * _mainCamera.orthographicSize;
					_cameraSize.x = _cameraSize.y * _mainCamera.aspect;

					Vector3 newSize = Confiner2D.size;

					if (Confiner2D.size.x < _cameraSize.x)
					{
						newSize.x = _cameraSize.x;
					}
					if (Confiner2D.size.y < _cameraSize.y)
					{
						newSize.y = _cameraSize.y;
					}

					Confiner2D.size = newSize;
					break;
				case Modes.ThreeD:
					Confiner3D.size = size;
					break;
			}

			CinemachineCameraConfiner2D.InvalidateBoundingShapeCache();
			#else
			yield return null;
			#endif
		}

        /// <summary>
        /// Looks for the level start position and if it's inside the room, makes this room the current one
        /// 查找关卡起始位置，如果它在房间内，将这个房间设为当前房间
        /// </summary>
        protected virtual void HandleLevelStartDetection()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3	
			if (!_initialized)
			{
				Initialization();
			}

			if (AutoDetectFirstRoomOnStart)
			{
				if (LevelManager.HasInstance)
				{
					if (RoomBounds.Contains(LevelManager.Instance.Players[0].transform.position))
					{
						MMCameraEvent.Trigger(MMCameraEventTypes.ResetPriorities);
						MMCinemachineBrainEvent.Trigger(MMCinemachineBrainEventTypes.ChangeBlendDuration, 0f);

						MMSpriteMaskEvent.Trigger(MMSpriteMaskEvent.MMSpriteMaskEventTypes.MoveToNewPosition,
							RoomColliderCenter,
							RoomColliderSize,
							0f, MMTween.MMTweenCurve.LinearTween);

						PlayerEntersRoom();
						if (VirtualCamera != null)
						{
							VirtualCamera.Priority = 10;
							VirtualCamera.enabled = true;	
						}
					}
					else
					{
						if (VirtualCamera != null)
						{
							VirtualCamera.Priority = 0;
							VirtualCamera.enabled = false;	
						}
					}
				}
			}
			#endif
		}

        /// <summary>
        /// Call this to let the room know a player entered
        /// 通知房间有玩家进入
        /// </summary>
        public virtual void PlayerEntersRoom()
		{
			CurrentRoom = true;
			if (RoomVisited)
			{
				OnPlayerEntersRoom?.Invoke();
			}
			else
			{
				RoomVisited = true;
				OnPlayerEntersRoomForTheFirstTime?.Invoke();
			}  
			foreach(GameObject go in ActivationList)
			{
				go.SetActive(true);
			}
		}

        /// <summary>
        /// Call this to let this room know a player exited
        ///  通知房间有玩家退出
        /// </summary>
        public virtual void PlayerExitsRoom()
		{
			CurrentRoom = false;
			OnPlayerExitsRoom?.Invoke();
			foreach (GameObject go in ActivationList)
			{
				go.SetActive(false);
			}
		}

        /// <summary>
        /// When we get a respawn event, we ask for a camera reposition
        /// 当我们接收到重生事件时，我们请求重新定位相机
        /// </summary>
        /// <param name="topDownEngineEvent"></param>
        public virtual void OnMMEvent(TopDownEngineEvent topDownEngineEvent)
		{
			if ((topDownEngineEvent.EventType == TopDownEngineEventTypes.RespawnComplete)
			    || (topDownEngineEvent.EventType == TopDownEngineEventTypes.LevelStart))
			{
				HandleLevelStartDetection();
			}
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// On enable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}