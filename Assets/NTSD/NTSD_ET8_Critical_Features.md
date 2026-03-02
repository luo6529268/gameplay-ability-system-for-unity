# ET8 关键保底措施实现指南

> 本文档补充 ET8 经过验证的关键保底措施
> 这些措施能显著提升帧同步的稳定性和容错能力

---

## 概述

ET8 作为经过大量项目验证的帧同步框架，包含了很多"保底措施"来应对网络问题和边界情况。本文档将这些关键措施补充到 NTSD 项目中。

### ET8 关键措施清单

| 措施 | 用途 | 优先级 | 实施阶段 |
|------|------|--------|---------|
| 输入确认机制 | 确保所有玩家输入到达 | 🔴 高 | Phase B2 |
| 输入重传机制 | 处理网络丢包 | 🔴 高 | Phase C2 |
| 追帧机制 | 快速追上进度 | 🟠 中 | Phase C2 |
| 帧号对齐检查 | 检测帧号漂移 | 🟠 中 | Phase D1 |
| 网络质量监控 | 动态调整延迟 | 🟡 中 | Phase C1 |
| 输入合法性校验 | 防止作弊 | 🟡 中 | Phase B3 |
| 服务器权威校验 | Hash 不一致处理 | 🟡 中 | Phase D1 |
| 完整日志系统 | 问题排查 | 🟢 低 | Phase D1 |
| 快照压缩 | 减少断线重连开销 | 🟢 低 | Phase E |

---

## 1. 输入确认机制（Phase B2）

### 原理
确保每一帧的输入都从所有玩家收到后才推进，避免"假推进"导致的不同步。

### ET8 实现思路
```csharp
// ET8 的 FrameBuffer
public class FrameBuffer
{
    // 记录每个玩家每一帧的输入是否到达
    private Dictionary<int, Dictionary<int, bool>> _inputConfirmed;
    // Key1: frame, Key2: playerId, Value: 是否确认
    
    public bool CanAdvanceFrame(int frame)
    {
        if (!_inputConfirmed.ContainsKey(frame))
            return false;
            
        foreach (var playerId in _activePlayers)
        {
            if (!_inputConfirmed[frame][playerId])
                return false;  // 有玩家输入未到
        }
        return true;  // 所有玩家输入都到了
    }
}
```

### NTSD 实现代码

**路径**：`Assets/NTSD/Scripts/Netcode/Fusion/InputConfirmationManager.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Netcode
{
    /// <summary>
    /// 输入确认管理器
    /// 确保每一帧的输入都从所有玩家收到
    /// </summary>
    public class InputConfirmationManager
    {
        // 每帧每个玩家的输入确认状态
        private Dictionary<int, Dictionary<int, bool>> _inputConfirmed = new();
        
        // 活跃玩家列表
        private HashSet<int> _activePlayers = new();
        
        // 超时阈值（帧数）
        private const int TIMEOUT_FRAMES = 30;  // 1 秒 @ 30fps
        
        /// <summary>
        /// 添加玩家
        /// </summary>
        public void AddPlayer(int playerId)
        {
            _activePlayers.Add(playerId);
        }
        
        /// <summary>
        /// 移除玩家
        /// </summary>
        public void RemovePlayer(int playerId)
        {
            _activePlayers.Remove(playerId);
        }
        
        /// <summary>
        /// 标记某个玩家的输入已到达
        /// </summary>
        public void ConfirmInput(int frame, int playerId)
        {
            if (!_inputConfirmed.ContainsKey(frame))
            {
                _inputConfirmed[frame] = new Dictionary<int, bool>();
            }
            
            _inputConfirmed[frame][playerId] = true;
        }
        
        /// <summary>
        /// 检查某一帧是否可以推进
        /// </summary>
        public bool CanAdvanceFrame(int frame, int currentFrame)
        {
            // 超时检查：如果等待时间过长，强制推进（使用预测输入）
            if (currentFrame - frame > TIMEOUT_FRAMES)
            {
                Debug.LogWarning($"[InputConfirm] 帧 {frame} 等待超时，强制推进");
                return true;
            }
            
            // 检查是否所有玩家输入都到了
            if (!_inputConfirmed.ContainsKey(frame))
                return false;
            
            foreach (var playerId in _activePlayers)
            {
                if (!_inputConfirmed[frame].ContainsKey(playerId) || 
                    !_inputConfirmed[frame][playerId])
                {
                    return false;  // 有玩家输入未到
                }
            }
            
            return true;  // 所有玩家输入都到了
        }
        
        /// <summary>
        /// 清理旧数据
        /// </summary>
        public void CleanupOldFrames(int currentFrame)
        {
            var framesToRemove = new List<int>();
            
            foreach (var frame in _inputConfirmed.Keys)
            {
                if (frame < currentFrame - 60)  // 保留最近 60 帧
                {
                    framesToRemove.Add(frame);
                }
            }
            
            foreach (var frame in framesToRemove)
            {
                _inputConfirmed.Remove(frame);
            }
        }
    }
}
```

### 集成到 FusionLockstepManager

```csharp
// 在 FusionLockstepManager 中添加
private InputConfirmationManager _inputConfirmation = new();

public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
{
    _inputConfirmation.AddPlayer(player.PlayerId);
}

public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
{
    _inputConfirmation.RemovePlayer(player.PlayerId);
}

public void FixedUpdateNetwork()
{
    int tick = _runner.Simulation.Tick;
    
    // 1. 收集输入并标记确认
    foreach (var player in _runner.ActivePlayers)
    {
        if (_runner.TryGetInputForPlayer(player, out NetworkInputData input))
        {
            _inputConfirmation.ConfirmInput(tick, player.PlayerId);
            // ... 注入到 SimInputBuffer
        }
    }
    
    // 2. 检查是否可以推进
    if (_inputConfirmation.CanAdvanceFrame(tick, _runner.Simulation.Tick))
    {
        // 推进 SimulationWorld
        // ...
    }
    else
    {
        // 等待输入或使用预测
        Debug.LogWarning($"[Fusion] 帧 {tick} 输入未齐，等待中...");
    }
    
    // 3. 清理旧数据
    _inputConfirmation.CleanupOldFrames(tick);
}
```

---

## 2. 输入重传机制（Phase C2）

### 原理
客户端记录最近发送的输入，定期检查服务器是否收到，未收到则重传。

### NTSD 实现代码

**路径**：`Assets/NTSD/Scripts/Netcode/Fusion/InputRetransmissionManager.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Netcode
{
    /// <summary>
    /// 输入重传管理器
    /// 处理网络丢包导致的输入丢失
    /// </summary>
    public class InputRetransmissionManager
    {
        // 已发送但未确认的输入
        private Dictionary<int, NetworkInputData> _pendingInputs = new();
        
        // 最后确认的帧号
        private int _lastAckFrame = 0;
        
        // 重传间隔（帧数）
        private const int RETRANSMIT_INTERVAL = 10;  // 每 10 帧检查一次
        
        // 最大重传次数
        private const int MAX_RETRANSMIT_COUNT = 3;
        
        // 重传计数
        private Dictionary<int, int> _retransmitCount = new();
        
        /// <summary>
        /// 记录已发送的输入
        /// </summary>
        public void RecordSentInput(int frame, NetworkInputData input)
        {
            _pendingInputs[frame] = input;
            _retransmitCount[frame] = 0;
        }
        
        /// <summary>
        /// 确认服务器已收到输入
        /// </summary>
        public void AckInput(int frame)
        {
            if (frame > _lastAckFrame)
            {
                _lastAckFrame = frame;
            }
            
            // 清理已确认的输入
            var framesToRemove = new List<int>();
            foreach (var f in _pendingInputs.Keys)
            {
                if (f <= frame)
                {
                    framesToRemove.Add(f);
                }
            }
            
            foreach (var f in framesToRemove)
            {
                _pendingInputs.Remove(f);
                _retransmitCount.Remove(f);
            }
        }
        
        /// <summary>
        /// 检查并重传丢失的输入
        /// </summary>
        public List<(int frame, NetworkInputData input)> CheckAndRetransmit(int currentFrame)
        {
            var toRetransmit = new List<(int, NetworkInputData)>();
            
            // 每 RETRANSMIT_INTERVAL 帧检查一次
            if (currentFrame % RETRANSMIT_INTERVAL != 0)
                return toRetransmit;
            
            foreach (var kvp in _pendingInputs)
            {
                int frame = kvp.Key;
                var input = kvp.Value;
                
                // 如果输入发送后超过一定时间还未确认，重传
                if (currentFrame - frame > RETRANSMIT_INTERVAL)
                {
                    if (_retransmitCount[frame] < MAX_RETRANSMIT_COUNT)
                    {
                        toRetransmit.Add((frame, input));
                        _retransmitCount[frame]++;
                        Debug.LogWarning($"[InputRetransmit] 重传帧 {frame} 输入（第 {_retransmitCount[frame]} 次）");
                    }
                    else
                    {
                        Debug.LogError($"[InputRetransmit] 帧 {frame} 输入重传失败（超过最大次数）");
                        // 放弃重传，清理
                        _pendingInputs.Remove(frame);
                        _retransmitCount.Remove(frame);
                    }
                }
            }
            
            return toRetransmit;
        }
    }
}
```

### 集成示例

```csharp
// 在 FusionLockstepManager 中
private InputRetransmissionManager _retransmission = new();

public void OnInput(NetworkRunner runner, NetworkInput input)
{
    var data = new NetworkInputData();
    // ... 采集输入
    
    input.Set(data);
    
    // 记录已发送的输入
    _retransmission.RecordSentInput(runner.Simulation.Tick, data);
}

public void FixedUpdateNetwork()
{
    int tick = _runner.Simulation.Tick;
    
    // 检查并重传丢失的输入
    var toRetransmit = _retransmission.CheckAndRetransmit(tick);
    foreach (var (frame, input) in toRetransmit)
    {
        // 重新发送输入（通过 RPC 或其他方式）
        RPC_ResendInput(frame, input);
    }
}

// 当收到服务器确认时
public void OnInputAck(int frame)
{
    _retransmission.AckInput(frame);
}
```

---

## 3. 追帧机制（Phase C2）

### 原理
当客户端落后服务器太多帧时，加速推进（一次推进多帧）快速追上。

### NTSD 实现代码

**路径**：`Assets/NTSD/Scripts/Netcode/Fusion/CatchUpManager.cs`

```csharp
using UnityEngine;

namespace NTSD.Netcode
{
    /// <summary>
    /// 追帧管理器
    /// 处理客户端落后服务器的情况
    /// </summary>
    public class CatchUpManager
    {
        // 追帧阈值（落后多少帧开始追）
        private const int CATCHUP_THRESHOLD = 10;  // 落后 10 帧
        
        // 追帧速度（每次推进几帧）
        private const int CATCHUP_SPEED = 2;  // 一次推进 2 帧
        
        // 最大追帧速度
        private const int MAX_CATCHUP_SPEED = 4;  // 最多一次推进 4 帧
        
        /// <summary>
        /// 计算需要推进的帧数
        /// </summary>
        public int CalculateFramesToAdvance(int clientFrame, int serverFrame)
        {
            int frameDiff = serverFrame - clientFrame;
            
            if (frameDiff <= 0)
            {
                // 客户端没有落后，正常推进
                return 1;
            }
            else if (frameDiff < CATCHUP_THRESHOLD)
            {
                // 落后不多，正常推进
                return 1;
            }
            else if (frameDiff < CATCHUP_THRESHOLD * 2)
            {
                // 落后较多，加速追帧
                Debug.Log($"[CatchUp] 落后 {frameDiff} 帧，加速追帧（速度 {CATCHUP_SPEED}）");
                return CATCHUP_SPEED;
            }
            else
            {
                // 落后很多，最大速度追帧
                Debug.LogWarning($"[CatchUp] 落后 {frameDiff} 帧，最大速度追帧（速度 {MAX_CATCHUP_SPEED}）");
                return MAX_CATCHUP_SPEED;
            }
        }
        
        /// <summary>
        /// 检查是否需要追帧
        /// </summary>
        public bool NeedsCatchUp(int clientFrame, int serverFrame)
        {
            return serverFrame - clientFrame >= CATCHUP_THRESHOLD;
        }
    }
}
```

### 集成示例

```csharp
// 在 FusionLockstepManager 中
private CatchUpManager _catchUp = new();

public void FixedUpdateNetwork()
{
    int clientFrame = _world.CurrentTick;  // 客户端当前帧
    int serverFrame = _runner.Simulation.Tick;  // 服务器当前帧
    
    // 计算需要推进的帧数
    int framesToAdvance = _catchUp.CalculateFramesToAdvance(clientFrame, serverFrame);
    
    // 推进多帧
    for (int i = 0; i < framesToAdvance; i++)
    {
        if (clientFrame >= serverFrame)
            break;  // 已追上
        
        // 推进一帧
        _world.TransitTickAll(clientFrame);
        _world.TUTickAll(clientFrame);
        _world.LateTick(clientFrame);
        
        clientFrame++;
    }
}
```

---

## 4. 帧号对齐检查（Phase D1）

### 原理
定期检查所有客户端的帧号是否一致，发现漂移及时修正。

### NTSD 实现代码

**路径**：`Assets/NTSD/Scripts/Netcode/Fusion/FrameSyncChecker.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Netcode
{
    /// <summary>
    /// 帧号同步检查器
    /// 检测客户端帧号漂移
    /// </summary>
    public class FrameSyncChecker
    {
        // 检查间隔（帧数）
        private const int CHECK_INTERVAL = 30;  // 每 1 秒检查一次
        
        // 允许的最大帧号差异
        private const int MAX_FRAME_DIFF = 5;
        
        // 记录每个客户端的帧号
        private Dictionary<int, int> _clientFrames = new();
        
        /// <summary>
        /// 更新客户端帧号
        /// </summary>
        public void UpdateClientFrame(int playerId, int frame)
        {
            _clientFrames[playerId] = frame;
        }
        
        /// <summary>
        /// 检查帧号是否同步
        /// </summary>
        public bool CheckFrameSync(int currentFrame, out List<int> desyncedPlayers)
        {
            desyncedPlayers = new List<int>();
            
            // 每 CHECK_INTERVAL 帧检查一次
            if (currentFrame % CHECK_INTERVAL != 0)
                return true;
            
            if (_clientFrames.Count == 0)
                return true;
            
            // 找出最常见的帧号（认为是正确的）
            var frameCounts = new Dictionary<int, int>();
            foreach (var frame in _clientFrames.Values)
            {
                if (!frameCounts.ContainsKey(frame))
                    frameCounts[frame] = 0;
                frameCounts[frame]++;
            }
            
            int correctFrame = currentFrame;
            int maxCount = 0;
            foreach (var kvp in frameCounts)
            {
                if (kvp.Value > maxCount)
                {
                    correctFrame = kvp.Key;
                    maxCount = kvp.Value;
                }
            }
            
            // 检查每个客户端
            bool allSynced = true;
            foreach (var kvp in _clientFrames)
            {
                int playerId = kvp.Key;
                int frame = kvp.Value;
                int diff = Mathf.Abs(frame - correctFrame);
                
                if (diff > MAX_FRAME_DIFF)
                {
                    Debug.LogError($"[FrameSync] 玩家 {playerId} 帧号不同步！" +
                                   $"当前：{frame}，正确：{correctFrame}，差异：{diff}");
                    desyncedPlayers.Add(playerId);
                    allSynced = false;
                }
            }
            
            return allSynced;
        }
    }
}
```

---

## 5. 网络质量监控（Phase C1）

### NTSD 实现代码

**路径**：`Assets/NTSD/Scripts/Netcode/Fusion/NetworkQualityMonitor.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NTSD.Netcode
{
    /// <summary>
    /// 网络质量监控器
    /// 实时监控 Ping/丢包率/抖动
    /// </summary>
    public class NetworkQualityMonitor
    {
        // Ping 历史记录（用于计算平均值和抖动）
        private Queue<int> _pingHistory = new(30);
        
        // 丢包统计
        private int _totalPackets = 0;
        private int _lostPackets = 0;
        
        public int Ping { get; private set; }
        public int AvgPing { get; private set; }
        public int JitterMs { get; private set; }
        public float PacketLoss { get; private set; }
        
        /// <summary>
        /// 更新 Ping 值
        /// </summary>
        public void UpdatePing(int ping)
        {
            Ping = ping;
            
            _pingHistory.Enqueue(ping);
            if (_pingHistory.Count > 30)
                _pingHistory.Dequeue();
            
            // 计算平均 Ping
            AvgPing = (int)_pingHistory.Average();
            
            // 计算抖动（标准差）
            if (_pingHistory.Count > 1)
            {
                double variance = _pingHistory.Select(p => Mathf.Pow(p - AvgPing, 2)).Average();
                JitterMs = (int)Mathf.Sqrt((float)variance);
            }
        }
        
        /// <summary>
        /// 记录丢包
        /// </summary>
        public void RecordPacket(bool lost)
        {
            _totalPackets++;
            if (lost)
                _lostPackets++;
            
            PacketLoss = (float)_lostPackets / _totalPackets;
        }
        
        /// <summary>
        /// 根据网络质量推荐 InputDelay
        /// </summary>
        public int GetRecommendedInputDelay()
        {
            if (AvgPing > 200 || PacketLoss > 0.1f)
                return 8;  // 网络很差
            else if (AvgPing > 150 || PacketLoss > 0.05f)
                return 6;  // 网络较差
            else if (AvgPing > 100)
                return 5;  // 网络一般
            else if (AvgPing > 50)
                return 4;  // 网络良好
            else
                return 3;  // 网络很好
        }
        
        /// <summary>
        /// 获取网络质量等级
        /// </summary>
        public string GetQualityLevel()
        {
            if (AvgPing > 200 || PacketLoss > 0.1f)
                return "差";
            else if (AvgPing > 100 || PacketLoss > 0.05f)
                return "一般";
            else if (AvgPing > 50)
                return "良好";
            else
                return "优秀";
        }
    }
}
```

---

## 6. 完整日志系统（Phase D1）

### NTSD 实现代码

**路径**：`Assets/NTSD/Scripts/Netcode/Fusion/FrameLogger.cs`

```csharp
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NTSD.Netcode
{
    /// <summary>
    /// 帧日志记录器
    /// 记录每帧的输入/hash/关键状态，用于问题排查
    /// </summary>
    public class FrameLogger
    {
        [System.Serializable]
        public class FrameLog
        {
            public int frame;
            public int hash;
            public long timestamp;
            public List<string> inputs = new();
            public List<string> entities = new();
        }
        
        private List<FrameLog> _logs = new();
        private const int MAX_LOGS = 1800;  // 保留最近 60 秒（30fps）
        
        /// <summary>
        /// 记录一帧
        /// </summary>
        public void LogFrame(int frame, int hash, List<string> inputs, List<string> entities)
        {
            var log = new FrameLog
            {
                frame = frame,
                hash = hash,
                timestamp = System.DateTime.Now.Ticks,
                inputs = new List<string>(inputs),
                entities = new List<string>(entities)
            };
            
            _logs.Add(log);
            
            // 限制日志数量
            if (_logs.Count > MAX_LOGS)
            {
                _logs.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// 保存日志到文件
        /// </summary>
        public void SaveToFile(string path)
        {
            var json = JsonUtility.ToJson(new { logs = _logs }, true);
            File.WriteAllText(path, json);
            Debug.Log($"[FrameLogger] 日志已保存：{path}");
        }
        
        /// <summary>
        /// 定期自动保存
        /// </summary>
        public void AutoSave(int currentFrame)
        {
            // 每 300 帧（10 秒）自动保存一次
            if (currentFrame % 300 == 0)
            {
                string path = Path.Combine(Application.persistentDataPath, 
                                          $"frame_log_{System.DateTime.Now:yyyyMMdd_HHmmss}.json");
                SaveToFile(path);
            }
        }
    }
}
```

---

## 总结

### 实施优先级

#### 🔴 必须实施（Phase B/C）
1. **输入确认机制** - 避免假推进
2. **输入重传机制** - 处理丢包

#### 🟠 强烈建议（Phase C/D）
3. **追帧机制** - 提升重连体验
4. **帧号对齐检查** - 及时发现问题
5. **网络质量监控** - 动态优化

#### 🟡 建议实施（Phase D）
6. **输入合法性校验** - 防止作弊
7. **服务器权威校验** - Hash 不一致处理
8. **完整日志系统** - 问题排查

### 工作量增加

增加这些措施后，总工作量从 **16-25 天** 增加到 **19-30 天**，但稳定性显著提升。

### 下一步

1. 先按原计划完成 Phase A（确定性验证）
2. Phase B/C 实施时，同步加入 ET8 关键措施
3. Phase D 补充日志和监控
4. 根据测试结果调整优先级
