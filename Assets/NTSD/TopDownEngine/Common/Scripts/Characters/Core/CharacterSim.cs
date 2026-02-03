using NTSD.Simulation;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// 角色的纯 C# 游戏逻辑模块（Plan B: Sim/View 分离）
	///
	/// P0 对齐 FLF:
	/// - SimTransit: 对应 FLF livingobject.transit() - 输入、帧转换、物理
	/// - SimTU: 对应 FLF livingobject.TU() - 状态更新
	/// - 执行顺序: all Transit → FlushTasks → all TU
	///
	/// SimOrder=10 (Character): 角色在类型分组执行顺序中排第一
	/// </summary>
	public class CharacterSim : ISimObject
	{
		public int SimOrder => SimOrderConstants.Character;
		public int StableId { get; private set; }

		private Character _hub;

		public CharacterSim(Character hub)
		{
			_hub = hub;
			StableId = hub.StableIdRuntime;
		}

		public void OnAdded(SimContext ctx)
		{
			Debug.Log($"[CharacterSim] OnAdded: StableId={StableId}, GameObject={_hub.gameObject.name}");
		}

		public void OnRemoved(SimContext ctx)
		{
			Debug.Log($"[CharacterSim] OnRemoved: StableId={StableId}");
		}

		/// <summary>
		/// Transit 阶段 - 对应 FLF livingobject.transit()
		/// 职责：输入处理、帧转换、物理
		/// </summary>
		public void SimTransit(int tickIndex)
		{
			// 优先使用 LF2Character（新架构）
			if (_hub._LF2Character != null)
			{
				_hub._LF2Character.Transit();
			}
		}

		/// <summary>
		/// TU 阶段 - 对应 FLF livingobject.TU()
		/// 职责：状态更新
		/// </summary>
		public void SimTU(int tickIndex)
		{
			// 优先使用 LF2Character（新架构）
			if (_hub._LF2Character != null)
			{
				_hub._LF2Character.TUUpdate();
			}
		}

		public void SimLateTick(int tickIndex)
		{
			// 视图同步（可选）
		}
	}
}
