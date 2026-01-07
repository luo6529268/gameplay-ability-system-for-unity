using System.Collections;
using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// An ability that will let the Character rotate its associated camera, using the PlayerID_CameraRotationAxis input axis
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Rotate Camera")]
	public class CharacterRotateCamera : MonoBehaviour, MMEventListener<TopDownEngineEvent>
	{

		[Header("Rotation axis")]  // 在Inspector中显示的分组标题：旋转轴
		/// <summary>
		/// 相机旋转的坐标系空间（通常使用世界坐标系）
		/// </summary>
		[Tooltip("相机旋转的坐标系空间（通常使用世界坐标系）")]
		public Space RotationSpace = Space.World;
		
		/// <summary>
		/// 相机的前向向量，通常为(0,0,1)
		/// </summary>
		[Tooltip("相机的前向向量，通常为(0,0,1)")]
		public Vector3 RotationForward = Vector3.forward;
		
		/// <summary>
		/// 相机旋转的轴向（3D中通常为(0,1,0)，2D中通常为(0,0,1)）
		/// </summary>
		[Tooltip("相机旋转的轴向（3D中通常为(0,1,0)，2D中通常为(0,0,1)）")]
		public Vector3 RotationAxis = Vector3.up;

		[Header("Camera Speed")]  // 在Inspector中显示的分组标题：相机速度
		/// <summary>
		/// 相机旋转的速度
		/// </summary>
		[Tooltip("相机旋转的速度")]
		public float CameraRotationSpeed = 1f;
		
		/// <summary>
		/// 相机向目标位置插值移动的速度
		/// </summary>
		[Tooltip("相机向目标位置插值移动的速度")]
		public float CameraInterpolationSpeed = 0.2f;

		[Header("Input Manager")]  // 在Inspector中显示的分组标题：输入管理器
		/// <summary>
		/// 如果为false，该能力将不会读取输入
		/// </summary>
		[Tooltip("如果为false，该能力将不会读取输入")]
		public bool InputAuthorized = true;
		
		/// <summary>
		/// 该能力是否应该修改InputManager以将其设置为相机驱动输入模式
		/// </summary>
		[Tooltip("该能力是否应该修改InputManager以将其设置为相机驱动输入模式")]
		public bool AutoSetupInputManager = true;


		protected float _requestedCameraAngle = 0f;
		protected Camera _mainCamera;
		#if MM_CINEMACHINE
		protected CinemachineBrain _brain;
		protected CinemachineVirtualCamera _virtualCamera;
		#elif MM_CINEMACHINE3
		protected CinemachineBrain _brain;
		protected CinemachineCamera _virtualCamera;
		#endif
		protected float _targetRotationAngle;
		protected Vector3 _cameraDirection;
		protected float _cameraDirectionAngle;

		/// <summary>
		/// On init we grab our camera and setup our input manager if needed
		/// </summary>
		protected void Awake()
		{
			_mainCamera = Camera.main;
			StartCoroutine(DelayedInitialization());
			if (AutoSetupInputManager)
			{
			}
		}

		/// <summary>
		/// Because Cinemachine only initializes in LateUpdate, and doesn't offer events to know when it'll be ready, we wait a bit for it to be done
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator DelayedInitialization()
		{
			yield return MMCoroutine.WaitForFrames(2);
			GetCurrentCamera();
		}

		/// <summary>
		/// Stores the current camera
		/// </summary>
		protected virtual void GetCurrentCamera()
		{
			#if MM_CINEMACHINE
			_brain = _mainCamera.GetComponent<CinemachineBrain>();
			if (_brain != null)
			{
				_virtualCamera = _brain.ActiveVirtualCamera as CinemachineVirtualCamera;
			}
			#elif MM_CINEMACHINE3
			_brain = _mainCamera.GetComponent<CinemachineBrain>();
			if (_brain != null)
			{
				_virtualCamera = _brain.ActiveVirtualCamera as CinemachineCamera;
			}
			#endif
		}

		/// <summary>
		/// If InputAuthorized is false, you can use this method to force a camera angle from another script
		/// </summary>
		/// <param name="newAngle"></param>
		public virtual void SetCameraAngle(float newAngle)
		{
			_requestedCameraAngle = newAngle;
		}

		/// <summary>
		/// Changes the rotation of the camera to match input
		/// </summary>
		protected virtual void RotateCamera()
		{
			_targetRotationAngle = MMMaths.Lerp(_targetRotationAngle, _requestedCameraAngle, CameraInterpolationSpeed, Time.deltaTime);

			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (_virtualCamera != null)
			{
				_virtualCamera.transform.Rotate(RotationAxis, _targetRotationAngle, RotationSpace);
				_cameraDirectionAngle = (_character.CharacterDimension == Character.CharacterDimensions.Type3D) ? _virtualCamera.transform.localEulerAngles.y : _virtualCamera.transform.localEulerAngles.z;

			}
			else  if (_mainCamera != null)
			{
				_mainCamera.transform.Rotate(RotationAxis, _targetRotationAngle, RotationSpace);
				_cameraDirectionAngle = (_character.CharacterDimension == Character.CharacterDimensions.Type3D) ? _mainCamera.transform.localEulerAngles.y : _mainCamera.transform.localEulerAngles.z;
			}
			#endif
			_cameraDirection = Quaternion.AngleAxis(_cameraDirectionAngle, RotationAxis) * RotationForward;
		}
		
		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDestroy()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}

        public void OnMMEvent(TopDownEngineEvent eventType)
        {
            throw new System.NotImplementedException();
        }
    }
}