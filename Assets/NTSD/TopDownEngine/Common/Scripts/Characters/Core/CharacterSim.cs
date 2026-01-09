using NTSD.Simulation;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// 角色的纯 C# 游戏逻辑模块（Plan B: Sim/View 分离）
	///
	/// 职责：
	/// - 驱动角色的游戏逻辑（不包括渲染）
	/// - 由 SimulationWorld 在 30Hz 驱动
	/// - 通过 Character Hub 访问 Unity 组件
	///
	/// 架构原则：
	/// - 纯 C# 实现（不继承 MonoBehaviour）
	/// - 不直接调用 GetComponent（通过 Hub 获取）
	/// - 游戏逻辑真相只在 SimTick 中推进
	/// </summary>
	public class CharacterSim : ISimObject
	{
		// ==================== ISimObject 实现 ====================

		/// <summary>
		/// 执行顺序优先级（角色模拟层）
		/// 注意：必须在输入系统（50）之后执行
		/// </summary>
		public int SimOrder => 100;

		/// <summary>
		/// 确定性 ID（用于网络同步/回放）
		/// 从 Character Hub 读取
		/// </summary>
		public int StableId { get; private set; }

		// ==================== Hub 引用 ====================

		/// <summary>
		/// Character Hub（Unity 组件缓存 + 配置）
		/// </summary>
		private Character _hub;

		// ==================== 构造函数 ====================

		/// <summary>
		/// 创建 CharacterSim
		/// </summary>
		/// <param name="hub">Character Hub 实例</param>
		public CharacterSim(Character hub)
		{
			_hub = hub;
			StableId = hub.StableIdRuntime;
		}

		// ==================== 生命周期 ====================

		/// <summary>
		/// 注册到 SimulationWorld 时调用
		/// </summary>
		/// <param name="ctx">模拟上下文</param>
		public void OnAdded(SimContext ctx)
		{
			Debug.Log($"[CharacterSim] OnAdded: StableId={StableId}, GameObject={_hub.gameObject.name}");

			// TODO (B8): 在这里初始化子系统
			// - 输入消费（通过 CharacterInput 的 buffer）
			// - 动画驱动（LF2CharacterAnimator）
			// - 状态机（CharacterStates）
		}

		/// <summary>
		/// 从 SimulationWorld 移除时调用
		/// </summary>
		/// <param name="ctx">模拟上下文</param>
		public void OnRemoved(SimContext ctx)
		{
			Debug.Log($"[CharacterSim] OnRemoved: StableId={StableId}, GameObject={_hub.gameObject.name}");

			// TODO: 清理资源
		}

		/// <summary>
		/// 每个 Sim Tick 调用一次（30Hz）
		///
		/// 对应 FLF: character.js 的主循环
		///
		/// 执行顺序（完全对齐 FLF）：
		/// 1. Transit: 输入处理 + 帧转换 + 物理（B5-B8）
		/// 2. TU_Update: 状态更新 + 武器点 + 帧切换（B8）
		/// </summary>
		/// <param name="tickIndex">当前 Tick 索引</param>
		public void SimTick(int tickIndex)
		{
			// ✅ Plan B Step B8: 驱动动画器（严格按 FLF 顺序）
			if (_hub._LF2CharacterAnimator != null)
			{
				// 阶段 1: Transit（输入、帧转换、物理）
				_hub._LF2CharacterAnimator.Transit();

				// 阶段 2: TU_Update（状态更新、武器点）
				_hub._LF2CharacterAnimator.TU_Update();
			}

			// Debug: 每 30 ticks 打印一次心跳（1秒）
			if (tickIndex % 30 == 0)
			{
				//Debug.Log($"[CharacterSim] SimTick {tickIndex}: {_hub.gameObject.name} (StableId={StableId})");
			}
		}

		/// <summary>
		/// 后期处理 Tick（可选）
		///
		/// 用途：视图更新、调试绘制等
		/// </summary>
		/// <param name="tickIndex">当前 Tick 索引</param>
		public void SimLateTick(int tickIndex)
		{
			// TODO (B8): 视图同步（Transform, SpriteRenderer 等）
		}
	}
}
