using System.Collections.Generic;
using UnityEngine;
using NTSD.Game;
using NTSD.Input;

namespace NTSD.Simulation
{
	/// <summary>
	/// 模拟输入缓冲区（Plan B: Tick-Aligned Input）
	///
	/// 职责：
	/// - 存储按 tickIndex 组织的输入事件
	/// - Unity InputSystem 回调写入，SimTick 消费
	/// - 避免输入时机依赖 Unity 帧率/回调时序
	///
	/// 架构原则：
	/// - "next tick" 语义：输入写入到下一个 tick，避免同帧竞态
	/// - 单机模式：使用 EnqueueForNextTick
	/// - 多人模式：服务器使用 EnqueueForTick 注入（带 tickIndex）
	/// - 自动清理旧数据（防止内存泄漏）
	/// </summary>
	public class SimInputBuffer
	{
		// ==================== 内部存储 ====================

		/// <summary>
		/// 按 tickIndex 组织的输入事件队列
		/// Key: tickIndex
		/// Value: 该 tick 的所有输入事件
		/// </summary>
		private Dictionary<int, List<SimInputEvent>> _buffer = new Dictionary<int, List<SimInputEvent>>();

		/// <summary>
		/// 当前 SimTick 索引（用于 "next tick" 计算）
		/// 每次 TryDequeueAll 时更新
		/// </summary>
		private int _currentTickIndex = 0;

		/// <summary>
		/// 历史清理阈值（保留最近 N 个 tick 的数据）
		/// 单机模式：30 ticks (1秒) 足够
		/// 多人模式：可能需要更大（网络延迟 + 回滚窗口）
		/// </summary>
		private const int CLEANUP_THRESHOLD = 60;  // 2秒历史

		// ==================== 公共 API ====================

		/// <summary>
		/// 写入输入事件到下一个 Tick（单机模式）
		///
		/// 用途：
		/// - Unity InputSystem 回调中调用
		/// - 确保输入在下一个 SimTick 才生效，避免同帧竞态
		///
		/// 注意：
		/// - "下一个 tick" = _currentTickIndex + 1
		/// - _currentTickIndex 在 TryDequeueAll 时更新
		/// </summary>
		/// <param name="key">按键类型</param>
		/// <param name="down">按下/抬起</param>
		public void EnqueueForNextTick(FuncKeyMask key, bool down)
		{
			int targetTick = _currentTickIndex + 1;
			EnqueueForTick(targetTick, key, down);
		}

		/// <summary>
		/// 写入输入事件到指定 Tick（多人模式/调试）
		///
		/// 用途：
		/// - 服务器注入输入（lockstep）
		/// - 录制/回放系统
		/// - 单元测试
		/// </summary>
		/// <param name="tickIndex">目标 Tick 索引</param>
		/// <param name="key">按键类型</param>
		/// <param name="down">按下/抬起</param>
		public void EnqueueForTick(int tickIndex, FuncKeyMask key, bool down)
		{
			// 获取或创建目标 tick 的事件列表
			if (!_buffer.TryGetValue(tickIndex, out List<SimInputEvent> events))
			{
				events = new List<SimInputEvent>();
				_buffer[tickIndex] = events;
			}

			// 添加事件
			SimInputEvent evt = new SimInputEvent(tickIndex, key, down);
			events.Add(evt);

			// Debug: 记录输入写入（可选）
			// Debug.Log($"[SimInputBuffer] Enqueued: {evt}");
		}

		/// <summary>
		/// 读取并清空指定 Tick 的所有输入事件
		///
		/// 调用时机：
		/// - ActionSequenceDetector.SimTick(tickIndex) 开头
		/// - 每个 tick 调用一次且仅一次
		///
		/// 行为：
		/// - 读取 tickIndex 对应的所有输入
		/// - 从 buffer 中移除这些输入
		/// - 更新 _currentTickIndex（用于 EnqueueForNextTick）
		/// - 自动清理旧数据
		///
		/// 注意：
		/// - 如果该 tick 没有输入，返回 false
		/// - events 参数是 out，调用者不需要预先分配
		/// </summary>
		/// <param name="tickIndex">当前 Tick 索引</param>
		/// <param name="events">输出：该 tick 的所有输入事件（如果有）</param>
		/// <returns>是否有输入事件</returns>
		public bool TryDequeueAll(int tickIndex, out List<SimInputEvent> events)
		{
			// 更新当前 tick（用于 EnqueueForNextTick）
			_currentTickIndex = tickIndex;

			// 尝试读取该 tick 的输入
			if (_buffer.TryGetValue(tickIndex, out events))
			{
				// 从 buffer 移除（已消费）
				_buffer.Remove(tickIndex);

				// Debug: 记录输入消费（可选）
				// Debug.Log($"[SimInputBuffer] Dequeued {events.Count} events for tick {tickIndex}");

				// 清理旧数据
				CleanupOldData(tickIndex);

				return true;
			}

			// 没有输入
			events = null;
			return false;
		}

		/// <summary>
		/// 清理旧数据（防止内存泄漏）
		///
		/// 策略：
		/// - 保留最近 CLEANUP_THRESHOLD 个 tick 的数据
		/// - 删除更早的数据
		///
		/// 为什么需要清理：
		/// - 单机模式：输入可能写入到未来很远的 tick（例如玩家暂停游戏）
		/// - 多人模式：需要保留回滚窗口的数据
		/// </summary>
		/// <param name="currentTick">当前 Tick 索引</param>
		private void CleanupOldData(int currentTick)
		{
			int cleanupBefore = currentTick - CLEANUP_THRESHOLD;

			// 收集需要删除的 key（避免在遍历时修改字典）
			List<int> keysToRemove = new List<int>();

			foreach (var kvp in _buffer)
			{
				if (kvp.Key < cleanupBefore)
				{
					keysToRemove.Add(kvp.Key);
				}
			}

			// 删除旧数据
			foreach (int key in keysToRemove)
			{
				_buffer.Remove(key);

				// Debug: 记录清理（可选）
				// Debug.LogWarning($"[SimInputBuffer] Cleaned up old data for tick {key} (current tick: {currentTick})");
			}
		}

		/// <summary>
		/// 清空所有数据（用于场景切换/重置）
		/// </summary>
		public void Clear()
		{
			_buffer.Clear();
			_currentTickIndex = 0;
			Debug.Log("[SimInputBuffer] Cleared all buffered inputs");
		}

		/// <summary>
		/// 获取当前缓冲的 tick 数量（调试用）
		/// </summary>
		public int BufferedTickCount => _buffer.Count;

		/// <summary>
		/// 获取当前 tick 索引（调试用）
		/// </summary>
		public int CurrentTickIndex => _currentTickIndex;
	}
}
