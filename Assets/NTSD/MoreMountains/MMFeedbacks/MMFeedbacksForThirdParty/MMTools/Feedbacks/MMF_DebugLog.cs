using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 这是一个用于输出调试信息的反馈类，可以通过自定义MM调试方法或Unity的标准日志系统（Log、Assertion、Error、Warning）向控制台输出消息。
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("此反馈允许您使用自定义MM调试方法或Log、Assertion、Error、Warning日志向控制台输出消息。")]
    [MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
    [FeedbackPath("Debug/Log")]
    public class MMF_DebugLog : MMF_Feedback
    {
        // 用于一次性禁用此类型所有反馈的静态布尔值
        public static bool FeedbackTypeAuthorized = true;
        // 此反馈的持续时间为0
        public override float FeedbackDuration { get { return 0f; } }

        // 设置此反馈在检视面板中的颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
#endif

        // 可用的调试模式枚举
        public enum DebugLogModes { DebugLogTime, Log, Assertion, Error, Warning }

        [MMFInspectorGroup("Debug", true, 17)]
        // 当前选中的调试模式
        [Tooltip("当前选中的调试模式")]
        public DebugLogModes DebugLogMode = DebugLogModes.DebugLogTime;

        // 要显示的调试消息
        [Tooltip("要显示的调试消息")]
        [TextArea]
        public string DebugMessage = "YOUR DEBUG MESSAGE GOES HERE";
        // 在DebugLogTime模式下消息的颜色
        [Tooltip("在DebugLogTime模式下消息的颜色")]
        [MMFEnumCondition("DebugLogMode", (int)DebugLogModes.DebugLogTime)]
        public Color DebugColor = Color.cyan;
        // 在DebugLogTime模式下是否显示帧数
        [Tooltip("在DebugLogTime模式下是否显示帧数")]
        [MMFEnumCondition("DebugLogMode", (int)DebugLogModes.DebugLogTime)]
        public bool DisplayFrameCount = true;

        /// <summary>
        /// 在播放时，使用选定的模式将消息输出到控制台
        /// </summary>
        /// <param name="position">位置参数</param>
        /// <param name="feedbacksIntensity">反馈强度参数</param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            // 如果反馈未激活或类型未被授权，则直接返回
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }

            // 根据选择的调试模式输出相应的日志
            switch (DebugLogMode)
            {
                case DebugLogModes.Assertion:
                    Debug.LogAssertion(DebugMessage);
                    break;
                case DebugLogModes.Log:
                    Debug.Log(DebugMessage);
                    break;
                case DebugLogModes.Error:
                    Debug.LogError(DebugMessage);
                    break;
                case DebugLogModes.Warning:
                    Debug.LogWarning(DebugMessage);
                    break;
                case DebugLogModes.DebugLogTime:
                    // 转换颜色为HTML格式并调用MMDebug的日志方法
                    string color = "#" + ColorUtility.ToHtmlStringRGB(DebugColor);
                    MMDebug.DebugLogTime(DebugMessage, color, 3, DisplayFrameCount);
                    break;
            }
        }
    }

}