using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// the possible modes for the timescale
	public enum TimescaleModes { Scaled, Unscaled }

    /// <summary>
    /// A class collecting delay, cooldown and repeat values, to be used to define the behaviour of each MMFeedback
    /// 用于定义每个MMFeedback行为的延迟、冷却和重复值的类
    /// </summary>
    [System.Serializable]
	public class MMFeedbackTiming
	{
        /// <summary>
        /// 反馈基于宿主MMFeedbacks的方向可以播放的可能方式
        /// </summary>
        public enum MMFeedbacksDirectionConditions { Always, OnlyWhenForwards, OnlyWhenBackwards }

        /// <summary>
        /// 反馈可以播放的可能方向
        /// </summary>
        public enum PlayDirections { FollowMMFeedbacksDirection, OppositeMMFeedbacksDirection, AlwaysNormal, AlwaysRewind }


        [Header("Timescale")]
        /// <summary>
        /// 是否使用缩放或未缩放的时间
        /// </summary>
        [Tooltip("是否使用缩放或未缩放的时间")]
        public TimescaleModes TimescaleMode = TimescaleModes.Scaled;

        [Header("Exceptions")]
        /// <summary>
        /// 如果为真，保持暂停不会等待此反馈完成
        /// </summary>
        [Tooltip("如果为真，保持暂停不会等待此反馈完成")]
        public bool ExcludeFromHoldingPauses = false;

        /// <summary>
        /// 是否将此反馈计入父级MMFeedbacks（播放器）的总持续时间
        /// </summary>
        [Tooltip("是否将此反馈计入父级MMFeedbacks（播放器）的总持续时间")]
        public bool ContributeToTotalDuration = true;

        [Header("Delays")]
        /// <summary>
        /// 在播放延迟之前应用的初始延迟（秒）
        /// </summary>
        [Tooltip("在播放延迟之前应用的初始延迟（秒）")]
        public float InitialDelay = 0f;

        /// <summary>
        /// 两次播放之间必须的冷却持续时间
        /// </summary>
        [Tooltip("两次播放之间必须的冷却持续时间")]
        public float CooldownDuration = 0f;

        [Header("Stop")]
        /// <summary>
        /// 如果为真，当在其父级MMFeedbacks上调用Stop时，此反馈将中断自身，否则它将继续运行
        /// </summary>
        [Tooltip("如果为真，当在其父级MMFeedbacks上调用Stop时，此反馈将中断自身，否则它将继续运行")]
        public bool InterruptsOnStop = true;

        [Header("Repeat")]
        /// <summary>
        /// 重复模式，反馈应该播放一次、多次还是永远
        /// </summary>
        [Tooltip("重复模式，反馈应该播放一次、多次还是永远")]
        public int NumberOfRepeats = 0;

        /// <summary>
        /// 如果为真，反馈将永远重复
        /// </summary>
        [Tooltip("如果为真，反馈将永远重复")]
        public bool RepeatForever = false;
        /// <summary>
        /// 两次触发此反馈之间的延迟（秒）。这不包括反馈的持续时间。
        /// </summary>
        [Tooltip("两次触发此反馈之间的延迟（秒）。这不包括反馈的持续时间。")]
        public float DelayBetweenRepeats = 1f;

        [Header("PlayCount")]
        /// <summary>
        /// 自初始化（或上次重置如果SetPlayCountToZeroOnReset为真）以来，此反馈已被播放的次数
        /// </summary>
        [Tooltip("自初始化（或上次重置如果SetPlayCountToZeroOnReset为真）以来，此反馈已被播放的次数")]
        [MMFReadOnly]
        public int PlayCount = 0;

        /// <summary>
        /// 是否限制此反馈可以播放的次数。超过该次数后，它将不再播放
        /// </summary>
        [Tooltip("是否限制此反馈可以播放的次数。超过该次数后，它将不再播放")]
        public bool LimitPlayCount = false;

        /// <summary>
        /// 如果LimitPlayCount为真，此反馈可以播放的最大次数
        /// </summary>
        [Tooltip("如果LimitPlayCount为真，此反馈可以播放的最大次数")]
        [MMFCondition("LimitPlayCount", true)]
        public int MaxPlayCount = 3;
        /// <summary>
        /// 如果LimitPlayCount为真，当反馈被重置时，是否将播放次数重置为零
        /// </summary>
        [Tooltip("如果LimitPlayCount为真，当反馈被重置时，是否将播放次数重置为零")]
        [MMFCondition("LimitPlayCount", true)]
        public bool SetPlayCountToZeroOnReset = false;

        [Header("Play Direction")]
        /// <summary>
        /// 这定义了当宿主MMFeedbacks播放时，此反馈应如何播放：
        /// - 总是（默认）：此反馈将始终播放
        /// - 仅当向前：如果宿主MMFeedbacks向上播放（向前），此反馈将仅播放
        /// - 仅当向后：如果宿主MMFeedbacks向下播放（向后），此反馈将仅播放
        /// </summary>
        [Tooltip("Always: 此反馈将始终播放\n" +
            "OnlyWhenForwards: 如果宿主MMFeedbacks向上播放（向前），此反馈将仅播放\n" +
            "OnlyWhenBackwards: 如果宿主MMFeedbacks向下播放（向后），此反馈将仅播放\n")]
        public MMFeedbacksDirectionConditions MMFeedbacksDirectionCondition = MMFeedbacksDirectionConditions.Always;

        /// <summary>
        /// 这定义了此反馈的播放方式。它可以正常播放，或者倒放（声音将向后播放，
        /// 一个通常放大的对象将缩小，曲线将从右到左评估等）
        /// - 基于MMFeedbacks方向：当宿主MMFeedbacks向前播放时正常播放，在向后播放时倒放
        /// - 与MMFeedbacks方向相反：当宿主MMFeedbacks向前播放时倒放，向后播放时正常播放
        /// - 始终正常：无论宿主MMFeedbacks的方向如何，始终正常播放
        /// - 始终倒放：无论宿主MMFeedbacks的方向如何，始终倒放
        /// </summary>
        [Tooltip("这定义了此反馈的播放方式。它可以正常播放，或者倒放（声音将向后播放一个通常放大的对象将缩小，曲线将从右到左评估等）\n" +
            "基于MMFeedbacks方向：当宿主MMFeedbacks向前播放时正常播放，在向后播放时倒放\n" +
            "与MMFeedbacks方向相反：当宿主MMFeedbacks向前播放时倒放，向后播放时正常播放\n" +
            "始终正常：无论宿主MMFeedbacks的方向如何，始终正常播放\n" +
            "始终倒放：无论宿主MMFeedbacks的方向如何，始终倒放")]
        public PlayDirections PlayDirection = PlayDirections.FollowMMFeedbacksDirection;


        [Header("Intensity")]
        /// <summary>
        /// 如果为真，即使父级MMFeedbacks以较低的强度播放，强度也将保持不变
        /// </summary>
        [Tooltip("如果为真，即使父级MMFeedbacks以较低的强度播放，强度也将保持不变")]
        public bool ConstantIntensity = false;

        /// <summary>
        /// 如果为真，此反馈只有在其强度高于或等于IntensityIntervalMin且低于IntensityIntervalMax时才会播放
        /// </summary>
        [Tooltip("如果为真，此反馈只有在其强度高于或等于IntensityIntervalMin且低于IntensityIntervalMax时才会播放")]
        public bool UseIntensityInterval = false;

        /// <summary>
        /// 此反馈播放所需的最小强度
        /// </summary>
        [Tooltip("此反馈播放所需的最小强度")]
        [MMFCondition("UseIntensityInterval", true)]
        public float IntensityIntervalMin = 0f;
        /// <summary>
        /// 此反馈播放所需的最大强度
        /// </summary>
        [Tooltip("此反馈播放所需的最大强度")]
        [MMFCondition("UseIntensityInterval", true)]
        public float IntensityIntervalMax = 0f;

        [Header("Sequence")]
        /// <summary>
        /// 用于播放这些反馈的MMSequence
        /// </summary>
        [Tooltip("用于播放这些反馈的MMSequence")]
        public MMSequence Sequence;

        /// <summary>
        /// 要使用的MMSequence的TrackID
        /// </summary>
        [Tooltip("要使用的MMSequence的TrackID")]
        public int TrackID = 0;

        /// <summary>
        /// 是否使用目标序列的量化版本
        /// </summary>
        [Tooltip("是否使用目标序列的量化版本")]
        public bool Quantized = false;
        /// <summary>
        /// 如果使用目标序列的量化版本，在播放时应用于序列的BPM
        /// </summary>
        [Tooltip("如果使用目标序列的量化版本，在播放时应用于序列的BPM")]
        [MMFCondition("Quantized", true)]
        public int TargetBPM = 120;

        /// <summary>
        /// 从任何类，你可以设置UseScriptDrivenTimescale:true，从那里开始，而不是查看Time.time，Time.deltaTime（或它们的未缩放等价物），此反馈将根据你通过ScriptDrivenDeltaTime和ScriptDrivenTime提供的值计算时间
        /// </summary>
        public virtual bool UseScriptDrivenTimescale { get; set; }
        /// <summary>
        /// 此反馈应使用的delta时间值
        /// </summary>
        public virtual float ScriptDrivenDeltaTime { get; set; }
        /// <summary>
        /// 此反馈应使用的时间值
        /// </summary>
        public virtual float ScriptDrivenTime { get; set; }
    }
}