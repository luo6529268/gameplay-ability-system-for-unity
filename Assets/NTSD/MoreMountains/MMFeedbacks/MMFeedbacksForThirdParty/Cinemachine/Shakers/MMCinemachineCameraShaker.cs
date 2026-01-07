using System.Collections;
using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this component to your Cinemachine Virtual Camera to have it shake when calling its ShakeCamera methods.
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Camera Shaker")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	#elif MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineCamera))]
	#endif
	public class MMCinemachineCameraShaker : MonoBehaviour 
	{
		[Header("Settings")]
        /// 是否监听由整数定义的通道或由MMChannel脚本对象定义的通道。整数设置简单，但可能会变得混乱，难以记住哪个整数对应哪个通道。
        /// MMChannel脚本对象需要您提前创建，但带有可读名称，并且更可扩展
        [Tooltip("是否监听由整数定义的通道或由MMChannel脚本对象定义的通道。整数设置简单，但可能会变得混乱，难以记住哪个整数对应哪个通道。 " +
         "MMChannel脚本对象需要您提前创建，但带有可读名称，并且更可扩展")]
        public MMChannelModes ChannelMode = MMChannelModes.Int;
        /// 要监听的通道 - 必须与反馈上的通道匹配
        [Tooltip("要监听的通道 - 必须与反馈上的通道匹配")]
        [MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
        public int Channel = 0;

        /// 用于监听事件的MMChannel定义资产。针对此震动器的反馈必须引用相同的MMChannel定义才能接收事件 - 要创建一个MMChannel，
        /// 在项目中的任何位置右键点击（通常在一个Data文件夹中），然后选择MoreMountains > MMChannel，然后给它一个独特的名称
        [Tooltip("用于监听事件的MMChannel定义资产。针对此震动器的反馈必须引用相同的MMChannel定义才能接收事件 - 要创建一个MMChannel， " +
                "在项目中的任何位置右键点击（通常在一个Data文件夹中），然后选择MoreMountains > MMChannel，然后给它一个独特的名称")]
        [MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
        public MMChannel MMChannelDefinition = null;
        /// 如果你没有指定一个振幅，将应用到你的震动上的默认振幅
        [Tooltip("如果你没有指定一个振幅，将应用到你的震动上的默认振幅")]
        public float DefaultShakeAmplitude = 0.5f;
        /// 如果你没有指定一个频率，将应用到你的震动上的默认频率
        [Tooltip("如果你没有指定一个频率，将应用到你的震动上的默认频率")]
        public float DefaultShakeFrequency = 10f;
        /// 相机在空闲时的噪声振幅
        [Tooltip("相机在空闲时的噪声振幅")]
        [MMFReadOnly]
        public float IdleAmplitude;
        /// 相机在空闲时的噪声频率
        [Tooltip("相机在空闲时的噪声频率")]
        [MMFReadOnly]
        public float IdleFrequency = 1f;
        /// 插值震动的速度
        [Tooltip("插值震动的速度")]
        public float LerpSpeed = 5f;


        [Header("Test")]
        /// 测试震动时应用的持续时间（秒）
        [Tooltip("测试震动时应用的持续时间（秒）")]
        public float TestDuration = 0.3f;
        /// 测试震动时应用的振幅
        [Tooltip("测试震动时应用的振幅")]
        public float TestAmplitude = 2f;
        /// 测试震动时应用的频率
        [Tooltip("测试震动时应用的频率")]
        public float TestFrequency = 20f;

        [MMFInspectorButton("TestShake")]
		public bool TestShakeButton;

		public virtual float GetTime() { return (_timescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (_timescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

		protected TimescaleModes _timescaleMode;
		protected Vector3 _initialPosition;
		protected Quaternion _initialRotation;
		#if MM_CINEMACHINE
		protected Cinemachine.CinemachineBasicMultiChannelPerlin _perlin;
		protected Cinemachine.CinemachineVirtualCamera _virtualCamera;
		#elif MM_CINEMACHINE3
		protected CinemachineBasicMultiChannelPerlin _perlin;
		protected CinemachineCamera _virtualCamera;
		#endif
		protected float _targetAmplitude;
		protected float _targetFrequency;
		private Coroutine _shakeCoroutine;

		/// <summary>
		/// On awake we grab our components
		/// </summary>
		protected virtual void Awake()
		{
			#if MM_CINEMACHINE
			_virtualCamera = this.gameObject.GetComponent<CinemachineVirtualCamera>();
			_perlin = _virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
			#elif MM_CINEMACHINE3
			_virtualCamera = this.gameObject.GetComponent<CinemachineCamera>();
			_perlin = _virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
			#endif
		}

		/// <summary>
		/// On Start we reset our camera to apply our base amplitude and frequency
		/// </summary>
		protected virtual void Start()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (_perlin != null)
			{
				#if MM_CINEMACHINE
				IdleAmplitude = _perlin.m_AmplitudeGain;
				IdleFrequency = _perlin.m_FrequencyGain;
				#elif MM_CINEMACHINE3
				IdleAmplitude = _perlin.AmplitudeGain;
				IdleFrequency = _perlin.FrequencyGain;
				#endif
			}            
			#endif

			_targetAmplitude = IdleAmplitude;
			_targetFrequency = IdleFrequency;
		}

		protected virtual void Update()
		{
			#if MM_CINEMACHINE
			if (_perlin != null)
			{
				_perlin.m_AmplitudeGain = _targetAmplitude;
				_perlin.m_FrequencyGain = Mathf.Lerp(_perlin.m_FrequencyGain, _targetFrequency, GetDeltaTime() * LerpSpeed);
			}
			#elif MM_CINEMACHINE3
			if (_perlin != null)
			{
				_perlin.AmplitudeGain = _targetAmplitude;
				_perlin.FrequencyGain = Mathf.Lerp(_perlin.FrequencyGain, _targetFrequency, GetDeltaTime() * LerpSpeed);
			}
			#endif
		}

		/// <summary>
		/// Use this method to shake the camera for the specified duration (in seconds) with the default amplitude and frequency
		/// </summary>
		/// <param name="duration">Duration.</param>
		public virtual void ShakeCamera(float duration, bool infinite, bool useUnscaledTime = false)
		{
			StartCoroutine(ShakeCameraCo(duration, DefaultShakeAmplitude, DefaultShakeFrequency, infinite, useUnscaledTime));
		}

		/// <summary>
		/// Use this method to shake the camera for the specified duration (in seconds), amplitude and frequency
		/// </summary>
		/// <param name="duration">Duration.</param>
		/// <param name="amplitude">Amplitude.</param>
		/// <param name="frequency">Frequency.</param>
		public virtual void ShakeCamera(float duration, float amplitude, float frequency, bool infinite, bool useUnscaledTime = false)
		{
			if (_shakeCoroutine != null)
			{
				StopCoroutine(_shakeCoroutine);
			}
			_shakeCoroutine = StartCoroutine(ShakeCameraCo(duration, amplitude, frequency, infinite, useUnscaledTime));
		}

		/// <summary>
		/// This coroutine will shake the 
		/// </summary>
		/// <returns>The camera co.</returns>
		/// <param name="duration">Duration.</param>
		/// <param name="amplitude">Amplitude.</param>
		/// <param name="frequency">Frequency.</param>
		protected virtual IEnumerator ShakeCameraCo(float duration, float amplitude, float frequency, bool infinite, bool useUnscaledTime)
		{
			_targetAmplitude  = amplitude;
			_targetFrequency = frequency;
			_timescaleMode = useUnscaledTime ? TimescaleModes.Unscaled : TimescaleModes.Scaled;
			if (!infinite)
			{
				yield return new WaitForSeconds(duration);
				CameraReset();
			}                        
		}

		/// <summary>
		/// Resets the camera's noise values to their idle values
		/// </summary>
		public virtual void CameraReset()
		{
			_targetAmplitude = IdleAmplitude;
			_targetFrequency = IdleFrequency;
		}

		public virtual void OnCameraShakeEvent(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool infinite, MMChannelData channelData, bool useUnscaledTime)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}
			this.ShakeCamera(duration, amplitude, frequency, infinite, useUnscaledTime);
		}

		public virtual void OnCameraShakeStopEvent(MMChannelData channelData)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}
			if (_shakeCoroutine != null)
			{
				StopCoroutine(_shakeCoroutine);
			}            
			CameraReset();
		}

		protected virtual void OnEnable()
		{
			MMCameraShakeEvent.Register(OnCameraShakeEvent);
			MMCameraShakeStopEvent.Register(OnCameraShakeStopEvent);
		}

		protected virtual void OnDisable()
		{
			MMCameraShakeEvent.Unregister(OnCameraShakeEvent);
			MMCameraShakeStopEvent.Unregister(OnCameraShakeStopEvent);
		}

		protected virtual void TestShake()
		{
			MMCameraShakeEvent.Trigger(TestDuration, TestAmplitude, TestFrequency, 0f, 0f, 0f, false, new MMChannelData(ChannelMode, Channel, MMChannelDefinition));
		}
	}
}