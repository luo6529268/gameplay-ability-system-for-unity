using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Tools
{
	/// <summary>
	/// 用于存储MMSoundManager播放选项的结构体
	/// </summary>
	[Serializable]
	public struct MMSoundManagerPlayOptions
	{
		[HideInInspector]
		public bool Initialized;
		
		[Header("Track")] // 轨道设置
		/// <summary>
		/// 指定播放声音的轨道
		/// </summary>
		public MMSoundManager.MMSoundManagerTracks MmSoundManagerTrack;
		
		/// <summary>
		/// 如果不想在预设轨道上播放，可以指定一个自定义的AudioMixerGroup
		/// </summary>
		public AudioMixerGroup AudioGroup;
		
		[Header("Sound")] // 声音设置
		/// <summary>
		/// 声音是否循环播放
		/// </summary>
		public bool Loop;
		
		/// <summary>
		/// 播放音量（范围：0-2）
		/// </summary>
		[Range(0f,2f)]
		public float Volume;
		
		/// <summary>
		/// 音频源的音高（范围：-3到3）
		/// </summary>
		[Range(-3f,3f)]
		public float Pitch;
		
		/// <summary>
		/// 声音的唯一ID，可用于后续查找该声音
		/// </summary>
		public int ID;
		
		[Header("Fade")] // 淡入淡出设置
		/// <summary>
		/// 播放时是否淡入
		/// </summary>
		public bool Fade;
		
		/// <summary>
		/// 淡入前的初始音量
		/// </summary>
		[MMCondition("Fade", true)]
		public float FadeInitialVolume;
		
		/// <summary>
		/// 淡入持续时间（秒）
		/// </summary>
		[MMCondition("Fade", true)]
		public float FadeDuration;
		
		/// <summary>
		/// 淡入时使用的补间动画类型
		/// </summary>
		[MMCondition("Fade", true)]
		public MMTweenType FadeTween;
		
		
		/// <summary>
		/// 声音是否在场景切换时保持播放
		/// </summary>
		public bool Persistent;
		
		/// <summary>
		/// 如果不想从对象池中获取，可以指定一个自定义的AudioSource
		/// </summary>
		public AudioSource RecycleAudioSource;
		
		[Header("Time")] // 时间设置
		/// <summary>
		/// 开始播放声音的时间点（秒）
		/// </summary>
		public float PlaybackTime;
		
		/// <summary>
		/// 播放持续时间（秒），超过此时间将停止播放
		/// </summary>
		public float PlaybackDuration;
		
		[Header("Spatial Settings")] // 空间音频设置
		/// <summary>
		/// 立体声声像（范围：-1左声道到1右声道），仅适用于单声道或立体声
		/// </summary>
		[Range(-1f,1f)]
		public float PanStereo;
		
		/// <summary>
		/// 3D空间混合比例（0.0为完全2D，1.0为完全3D）
		/// </summary>
		[Range(0f,1f)]
		public float SpatialBlend;
		
		/// <summary>
		/// 声音可以跟随的Transform对象
		/// </summary>
		public Transform AttachToTransform;
		
		[Header("Solo")] // 独奏设置
		/// <summary>
		/// 是否在目标轨道上独奏播放（独奏时该轨道其他声音静音）
		/// </summary>
		public bool SoloSingleTrack;
		
		/// <summary>
		/// 是否在所有轨道上独奏播放（独奏时所有其他轨道静音）
		/// </summary>
		public bool SoloAllTracks;
		
		/// <summary>
		/// 独奏模式下，声音结束时是否自动取消独奏
		/// </summary>
		public bool AutoUnSoloOnEnd;
		
		/// <summary>
		/// 是否绕过效果器（来自滤镜组件或全局监听器滤镜）
		/// </summary>
		public bool BypassEffects;
		
		/// <summary>
		/// 是否绕过AudioListener的全局效果（不适用于混音器组）
		/// </summary>
		public bool BypassListenerEffects;
		
		/// <summary>
		/// 是否绕过混响区域的全局混响
		/// </summary>
		public bool BypassReverbZones;
		
		/// <summary>
		/// 音频源的优先级（范围：0-256，数值越小优先级越高）
		/// </summary>
		[Range(0, 256)]
		public int Priority;
		
		/// <summary>
		/// 混响区域混合量（范围：0-1.1）
		/// </summary>
		[Range(0f,1.1f)]
		public float ReverbZoneMix;
		
		[Header("3D Sound Settings")] // 3D声音设置
		/// <summary>
		/// 多普勒效应强度（范围：0-5）
		/// </summary>
		[Range(0f,5f)]
		public float DopplerLevel;
		
		/// <summary>
		/// 声音播放位置
		/// </summary>
		public Vector3 Location;
		
		/// <summary>
		/// 3D立体声或多声道声音的扩散角度（度）
		/// </summary>
		[Range(0,360)]
		public int Spread;
		
		/// <summary>
		/// 声音随距离衰减的方式
		/// </summary>
		public AudioRolloffMode RolloffMode;
		
		/// <summary>
		/// 最小距离：在此距离内声音不再增大
		/// </summary>
		public float MinDistance;
		
		/// <summary>
		/// 最大距离：声音停止衰减的距离（对数衰减模式）
		/// </summary>
		public float MaxDistance;
		
		/// <summary>
		/// 如果声音未播放完毕是否自动回收
		/// </summary>
		public bool DoNotAutoRecycleIfNotDonePlaying;
		
		/// <summary>
		/// 是否使用自定义音量衰减曲线
		/// </summary>
		public bool UseCustomRolloffCurve;
		
		/// <summary>
		/// 自定义音量衰减曲线（当UseCustomRolloffCurve为true时使用）
		/// </summary>
		[MMCondition("UseCustomRolloffCurve", true)]
		public AnimationCurve CustomRolloffCurve;
		
		/// <summary>
		/// 是否使用自定义空间混合曲线
		/// </summary>
		public bool UseSpatialBlendCurve;
		
		/// <summary>
		/// 自定义空间混合曲线（当UseSpatialBlendCurve为true时使用）
		/// </summary>
		[MMCondition("UseSpatialBlendCurve", true)]
		public AnimationCurve SpatialBlendCurve;
		
		/// <summary>
		/// 是否使用自定义混响区域混合曲线
		/// </summary>
		public bool UseReverbZoneMixCurve;
		
		/// <summary>
		/// 自定义混响区域混合曲线（当UseReverbZoneMixCurve为true时使用）
		/// </summary>
		[MMCondition("UseReverbZoneMixCurve", true)]
		public AnimationCurve ReverbZoneMixCurve;
		
		/// <summary>
		/// 是否使用自定义扩散曲线
		/// </summary>
		public bool UseSpreadCurve;
		
		/// <summary>
		/// 自定义扩散曲线（当UseSpreadCurve为true时使用）
		/// </summary>
		[MMCondition("UseSpreadCurve", true)]
		public AnimationCurve SpreadCurve;
        
		/// <summary>
		/// 默认选项集合，适用于大多数常见情况
		/// 使用时建议以此为基础，只覆盖需要修改的选项
		///
		/// 示例：
		/// 
		/// MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
		/// options.Loop = Loop;
		/// options.Location = Vector3.zero;
		/// options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
		///     
		/// MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
		///
		/// 这里初始化了一个新的本地选项集，覆盖了循环、位置和轨道设置，并使用它触发播放事件
		/// 
		/// </summary>
		public static MMSoundManagerPlayOptions Default
		{
			get
			{
				MMSoundManagerPlayOptions defaultOptions = new MMSoundManagerPlayOptions();
				defaultOptions.Initialized = true;
				defaultOptions.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
				defaultOptions.Location = Vector3.zero;
				defaultOptions.Loop = false;
				defaultOptions.Volume = 1.0f;
				defaultOptions.ID = 0;
				defaultOptions.Fade = false;
				defaultOptions.FadeInitialVolume = 0f;
				defaultOptions.FadeDuration = 1f;
				defaultOptions.FadeTween = MMTweenType.DefaultEaseInCubic;
				defaultOptions.Persistent = false;
				defaultOptions.RecycleAudioSource = null;
				defaultOptions.AudioGroup = null;
				defaultOptions.Pitch = 1f;
				defaultOptions.PanStereo = 0f;
				defaultOptions.SpatialBlend = 0.0f;
				defaultOptions.SoloSingleTrack = false;
				defaultOptions.SoloAllTracks = false;
				defaultOptions.AutoUnSoloOnEnd = false;
				defaultOptions.BypassEffects = false;
				defaultOptions.BypassListenerEffects = false;
				defaultOptions.BypassReverbZones = false;
				defaultOptions.Priority = 128;
				defaultOptions.ReverbZoneMix = 1f;
				defaultOptions.DopplerLevel = 1f;
				defaultOptions.Spread = 0;
				defaultOptions.RolloffMode = AudioRolloffMode.Logarithmic;
				defaultOptions.MinDistance = 1f;
				defaultOptions.MaxDistance = 500f;
				defaultOptions.DoNotAutoRecycleIfNotDonePlaying = true;
				return defaultOptions;
			}
		}
	}

}
