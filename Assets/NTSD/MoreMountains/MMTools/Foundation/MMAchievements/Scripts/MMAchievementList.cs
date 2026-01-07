using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
// 在Unity编辑器中创建Asset菜单的属性，用于创建成就列表的ScriptableObject
	[CreateAssetMenu(fileName="AchievementList",menuName="MoreMountains/Achievement List")]
	/// <summary>
	/// 成就列表的脚本化对象类
	/// 需要在Resources文件夹中创建并存储该对象才能正常工作
	/// </summary>
	public class MMAchievementList : ScriptableObject 
	{
		/// 成就列表的唯一标识符，用于保存/加载数据
		public string AchievementsListID = "AchievementsList";

		/// 存储所有成就的列表
		public List<MMAchievement> Achievements;

		/// <summary>
		/// 重置此列表中的所有成就
		/// 所有成就将被重新锁定，进度将会丢失
		/// </summary>
		public virtual void ResetAchievements()
		{
			// 输出重置成就的日志
			Debug.LogFormat ("Reset Achievements");
			// 调用成就管理器重置指定ID的成就列表
			MMAchievementManager.ResetAchievements (AchievementsListID);
		}

		// 私有成员变量，用于管理该类的实例引用
		private MMReferenceHolder<MMAchievementList> _instances;
		
		// 当对象启用时，将当前实例注册到引用持有者中
		protected virtual void OnEnable() { _instances.Reference(this); }
		
		// 当对象禁用时，从引用持有者中移除当前实例
		protected virtual void OnDisable() { _instances.Dispose(); }
		
		// 静态属性，用于获取任意一个可用的成就列表实例
		public static MMAchievementList Any => MMReferenceHolder<MMAchievementList>.Any;
	}

}