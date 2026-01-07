using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
/// <summary>
	/// 这个反馈允许你保存一个引用，该引用可以被其他反馈使用来自动设置它们的目标。
	/// 当被播放时它不执行任何操作。
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这个反馈允许你保存一个引用，该引用可以被其他反馈使用来自动设置它们的目标。当被播放时它不执行任何操作。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Feedbacks/MMF Reference Holder")]
	public class MMF_ReferenceHolder : MMF_Feedback
	{
		/// 用于一次性禁用此类型所有反馈的静态布尔值
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		public override string RequiredTargetText => GameObjectReference != null ? GameObjectReference.name : "";  
		#endif
		/// 此反馈的持续时间为0
		public override float FeedbackDuration => 0f;
		public override bool DisplayFullHeaderColor => true;

		[MMFInspectorGroup("References", true, 37, true)]
		/// 要设置为目标的游戏对象（或在其上查找特定组件作为目标），
		/// 供所有可能查看此引用持有者以获取目标的反馈使用
		[Tooltip("要设置为目标的游戏对象（或在其上查找特定组件作为目标），供所有可能查看此引用持有者以获取目标的反馈使用")] 
		public GameObject GameObjectReference;
		/// 是否在MMF播放器列表中的所有兼容反馈上强制使用此引用持有者
		[Tooltip("是否在MMF播放器列表中的所有兼容反馈上强制使用此引用持有者")] 
		public bool ForceReferenceOnAll = false;
		
		/// <summary>
		/// 在初始化时，如果需要，我们在所有反馈上强制设置我们的引用
		/// </summary>
		/// <param name="owner">MMF_Player实例</param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (ForceReferenceOnAll)
			{
				// 遍历所有反馈
				for (int index = 0; index < Owner.FeedbacksList.Count; index++)
				{
					// 如果反馈具有自动目标获取功能
					if (Owner.FeedbacksList[index].HasAutomatedTargetAcquisition)
					{
						// 设置反馈在列表中的索引
						Owner.FeedbacksList[index].SetIndexInFeedbacksList(index);
						// 设置强制引用持有者
						Owner.FeedbacksList[index].ForcedReferenceHolder = this;
						// 强制执行自动目标获取
						Owner.FeedbacksList[index].ForceAutomateTargetAcquisition();
					}
				}
			}
		}

		/// <summary>
		/// 播放时不执行任何操作
		/// </summary>
		/// <param name="position">播放位置</param>
		/// <param name="feedbacksIntensity">反馈强度</param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			return;
		}
	}

}