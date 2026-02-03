using NTSD.Input;

namespace NTSD.Simulation
{
	/// <summary>
	/// 模拟输入事件（Plan B: Tick-Aligned Input）
	///
	/// 职责：
	/// - 存储单个输入事件（按键按下/抬起）
	/// - 按 tickIndex 组织，确保确定性
	///
	/// 架构原则：
	/// - 纯数据结构（struct）
	/// - 不可变（readonly fields）
	/// - 可序列化（未来网络同步/回放）
	/// </summary>
	public struct SimInputEvent
	{
		/// <summary>
		/// 事件发生的 Tick 索引
		/// 对应 FLF 的 "帧" 概念（30Hz）
		/// </summary>
		public readonly int tickIndex;

		/// <summary>
		/// 按键类型（左/右/上/下/攻击/跳跃/防御等）
		/// 对应 FLF 的 con.state 按键掩码
		/// </summary>
		public readonly FuncKeyMask key;

		/// <summary>
		/// 按键状态
		/// - true: 按下（pressed）
		/// - false: 抬起（released）
		/// </summary>
		public readonly bool down;

		/// <summary>
		/// 创建输入事件
		/// </summary>
		/// <param name="tickIndex">目标 Tick 索引</param>
		/// <param name="key">按键类型</param>
		/// <param name="down">按下/抬起</param>
		public SimInputEvent(int tickIndex, FuncKeyMask key, bool down)
		{
			this.tickIndex = tickIndex;
			this.key = key;
			this.down = down;
		}
	}
}
