# NTSD 联机帧同步代码模板与补充说明

> 本文档是 `NTSD_Lockstep_Execution_Guide.md` 的补充
> 包含所有需要的代码模板、目录结构、常见问题排查

---

## 目录结构

### 推荐的文件组织

```
Assets/NTSD/Scripts/
├── Simulation/
│   ├── Core/
│   │   ├── DeterministicRng.cs          # Phase A1 新增
│   │   └── WorldStateHasher.cs          # Phase A2 新增
│   ├── Driver/
│   │   └── SimulationTickDriver.cs      # 已有
│   ├── Input/
│   │   ├── SimInputBuffer.cs            # 已有
│   │   ├── SimInputEvent.cs             # 已有
│   │   ├── InputRecorder.cs             # Phase A2 新增
│   │   └── InputReplayer.cs             # Phase A2 新增
│   ├── SimulationWorld.cs               # 已有
│   ├── SimContext.cs                    # 已有（需修改）
│   └── ...
├── Netcode/
│   ├── Fusion/
│   │   ├── FusionLockstepManager.cs     # Phase B2 新增
│   │   ├── NetworkInputData.cs          # Phase B2 新增
│   │   └── FusionConnectionTest.cs      # Phase B1 测试用
│   └── ...
├── Animation/
│   ├── LF2Objects/
│   │   └── LF2Character.cs              # 已有（需修改 3 处随机）
│   └── ...
└── ...
```

---

## Phase A：确定性修复代码模板

### A1. DeterministicRng.cs

**路径**：`Assets/NTSD/Scripts/Simulation/Core/DeterministicRng.cs`

```csharp
using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// 确定性随机数生成器
    /// 用于替代 UnityEngine.Random，保证跨端一致性
    /// </summary>
    public class DeterministicRng
    {
        private System.Random _rng;
        private int _seed;
        
        public DeterministicRng(int seed)
        {
            _seed = seed;
            _rng = new System.Random(seed);
        }
        
        /// <summary>
        /// 返回 [0.0, 1.0) 的随机浮点数
        /// 替代 UnityEngine.Random.value
        /// </summary>
        public float NextFloat()
        {
            return (float)_rng.NextDouble();
        }
        
        /// <summary>
        /// 返回 [min, max) 的随机整数
        /// 替代 UnityEngine.Random.Range(min, max)
        /// </summary>
        public int Next(int min, int max)
        {
            return _rng.Next(min, max);
        }
        
        /// <summary>
        /// 返回 [min, max] 的随机浮点数
        /// 替代 UnityEngine.Random.Range(min, max)
        /// </summary>
        public float NextFloat(float min, float max)
        {
            return min + (float)_rng.NextDouble() * (max - min);
        }
        
        /// <summary>
        /// 重置随机数生成器（用于回放测试）
        /// </summary>
        public void Reset()
        {
            _rng = new System.Random(_seed);
        }
        
        /// <summary>
        /// 获取当前种子（调试用）
        /// </summary>
        public int Seed => _seed;
    }
}
```

---

### A2. SimContext 修改

**路径**：`Assets/NTSD/Scripts/Simulation/SimContext.cs`

**修改前**：
```csharp
public class SimContext
{
    public SimulationWorld World { get; }
    
    public SimContext(SimulationWorld world)
    {
        World = world;
    }
}
```

**修改后**：
```csharp
public class SimContext
{
    public SimulationWorld World { get; }
    public DeterministicRng Rng { get; }  // 新增
    
    public SimContext(SimulationWorld world, int seed)
    {
        World = world;
        Rng = new DeterministicRng(seed);  // 新增
    }
}
```

**对应修改 SimulationWorld 构造函数**：
```csharp
// 在 SimulationWorld.cs 中
public SimulationWorld(int seed = 12345)  // 添加 seed 参数
{
    _context = new SimContext(this, seed);  // 传入 seed
    ItrKindService = new NTSDItrKindService();
    SceneQuery = new BruteForceSceneQuery(this);
}
```

---

### A3. LF2Character.cs 修改示例

**路径**：`Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`

**修改位置 1（Line 985）**：
```csharp
// 修改前：
int NormalWeaponAtck = UnityEngine.Random.value < 0.5f ? 
    LF2StandardFrames.NormalWeaponAtck : LF2StandardFrames.NormalWeaponAtck2;

// 修改后：
int NormalWeaponAtck = Match.Context.Rng.NextFloat() < 0.5f ? 
    LF2StandardFrames.NormalWeaponAtck : LF2StandardFrames.NormalWeaponAtck2;
```

**修改位置 2（Line 1013）**：
```csharp
// 修改前：
int punchFrame = UnityEngine.Random.value < 0.5f ? 
    LF2StandardFrames.Punch : LF2StandardFrames.Punch4;

// 修改后：
int punchFrame = Match.Context.Rng.NextFloat() < 0.5f ? 
    LF2StandardFrames.Punch : LF2StandardFrames.Punch4;
```

**修改位置 3（Line 1320）**：
```csharp
// 修改前：
(target.PS.y >= 0 && UnityEngine.Random.value < 0.15f)

// 修改后：
(target.PS.y >= 0 && Match.Context.Rng.NextFloat() < 0.15f)
```

---

### A4. WorldStateHasher.cs

**路径**：`Assets/NTSD/Scripts/Simulation/Core/WorldStateHasher.cs`

```csharp
using System.Collections.Generic;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    /// <summary>
    /// 世界状态哈希计算器
    /// 用于验证多端一致性
    /// </summary>
    public class WorldStateHasher
    {
        private List<LF2LivingObject> _tmpObjects = new List<LF2LivingObject>(32);
        
        /// <summary>
        /// 计算当前世界状态的哈希值
        /// </summary>
        public int ComputeHash(SimulationWorld world)
        {
            int hash = 17;
            
            // 获取所有对象
            world.GetAllLivingObjects(_tmpObjects);
            
            // 按 StableId 排序（保证顺序一致）
            _tmpObjects.Sort((a, b) => a.StableId.CompareTo(b.StableId));
            
            foreach (var obj in _tmpObjects)
            {
                if (obj == null || obj.PS == null) continue;
                
                // StableId
                hash = hash * 31 + obj.StableId;
                
                // 位置（精度到 0.001）
                hash = hash * 31 + (int)(obj.PS.x * 1000);
                hash = hash * 31 + (int)(obj.PS.y * 1000);
                hash = hash * 31 + (int)(obj.PS.z * 1000);
                
                // 速度
                hash = hash * 31 + (int)(obj.PS.vx * 1000);
                hash = hash * 31 + (int)(obj.PS.vy * 1000);
                hash = hash * 31 + (int)(obj.PS.vz * 1000);
                
                // 血量
                if (obj.Health != null)
                {
                    hash = hash * 31 + obj.Health.Hp;
                    hash = hash * 31 + obj.Health.Mp;
                }
                
                // 当前帧号
                if (obj.Frame != null)
                {
                    hash = hash * 31 + obj.Frame.N;
                }
                
                // 朝向
                hash = hash * 31 + (obj.PS.dir == "right" ? 1 : 0);
            }
            
            return hash;
        }
    }
}
```

---

### A5. InputRecorder.cs

**路径**：`Assets/NTSD/Scripts/Simulation/Input/InputRecorder.cs`

```csharp
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NTSD.Input;

namespace NTSD.Simulation
{
    /// <summary>
    /// 输入录制器
    /// 用于录制游戏输入序列，供回放测试
    /// </summary>
    public class InputRecorder
    {
        [System.Serializable]
        public class RecordedInput
        {
            public int tick;
            public int keyMask;  // FuncKeyMask 转为 int
            public bool down;
        }
        
        [System.Serializable]
        public class Recording
        {
            public int seed;  // RNG 种子
            public List<RecordedInput> inputs = new List<RecordedInput>();
        }
        
        private Recording _recording = new Recording();
        private bool _isRecording = false;
        
        public void StartRecording(int seed)
        {
            _recording = new Recording();
            _recording.seed = seed;
            _isRecording = true;
            Debug.Log($"[InputRecorder] 开始录制，种子：{seed}");
        }
        
        public void Record(int tick, FuncKeyMask key, bool down)
        {
            if (!_isRecording) return;
            
            _recording.inputs.Add(new RecordedInput
            {
                tick = tick,
                keyMask = (int)key,
                down = down
            });
        }
        
        public void StopRecording()
        {
            _isRecording = false;
            Debug.Log($"[InputRecorder] 停止录制，共 {_recording.inputs.Count} 条输入");
        }
        
        public void SaveToFile(string path)
        {
            string json = JsonUtility.ToJson(_recording, true);
            File.WriteAllText(path, json);
            Debug.Log($"[InputRecorder] 保存到：{path}");
        }
    }
}
```

---

### A6. InputReplayer.cs

**路径**：`Assets/NTSD/Scripts/Simulation/Input/InputReplayer.cs`

```csharp
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NTSD.Input;

namespace NTSD.Simulation
{
    /// <summary>
    /// 输入回放器
    /// 用于回放录制的输入序列
    /// </summary>
    public class InputReplayer
    {
        private InputRecorder.Recording _recording;
        
        public int Seed => _recording?.seed ?? 0;
        
        public void LoadFromFile(string path)
        {
            string json = File.ReadAllText(path);
            _recording = JsonUtility.FromJson<InputRecorder.Recording>(json);
            Debug.Log($"[InputReplayer] 加载录制，种子：{_recording.seed}，输入数：{_recording.inputs.Count}");
        }
        
        public void InjectInputs(SimInputBuffer buffer)
        {
            if (_recording == null)
            {
                Debug.LogError("[InputReplayer] 未加载录制文件");
                return;
            }
            
            foreach (var input in _recording.inputs)
            {
                buffer.EnqueueForTick(input.tick, (FuncKeyMask)input.keyMask, input.down);
            }
            
            Debug.Log($"[InputReplayer] 已注入 {_recording.inputs.Count} 条输入");
        }
    }
}
```

---

## Phase B：Photon Fusion 集成代码模板

### B1. Photon Fusion SDK 获取

#### 方式 A：Asset Store（推荐）
1. Unity 菜单 → Window → Asset Store
2. 搜索 "Photon Fusion"
3. 下载并导入

#### 方式 B：官网下载
1. 访问：https://doc.photonengine.com/fusion/current/getting-started/sdk-download
2. 下载 `.unitypackage`
3. 拖入 Unity 项目导入

#### 方式 C：Package Manager（如果支持）
```
Window → Package Manager → + → Add package from git URL
输入：com.photonengine.fusion
```

---

### B2. FusionConnectionTest.cs（验证连接）

**路径**：`Assets/NTSD/Scripts/Netcode/Fusion/FusionConnectionTest.cs`

```csharp
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;

namespace NTSD.Netcode
{
    /// <summary>
    /// Photon Fusion 连接测试
    /// 用于验证 App ID 配置是否正确
    /// </summary>
    public class FusionConnectionTest : MonoBehaviour, INetworkRunnerCallbacks
    {
        async void Start()
        {
            var runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
            
            var result = await runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = "TestRoom",
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });
            
            if (result.Ok)
            {
                Debug.Log("✅ Photon Fusion 连接成功！");
            }
            else
            {
                Debug.LogError($"❌ 连接失败：{result.ShutdownReason}");
            }
        }
        
        // INetworkRunnerCallbacks 必须实现的方法（可以留空）
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}
```

**使用方法**：
1. 创建空 GameObject
2. 挂载 `FusionConnectionTest` 脚本
3. 运行游戏
4. 查看 Console 输出

---

### B3. NetworkInputData.cs

**路径**：`Assets/NTSD/Scripts/Netcode/Fusion/NetworkInputData.cs`

```csharp
using Fusion;
using NTSD.Input;

namespace NTSD.Netcode
{
    /// <summary>
    /// 网络输入数据结构
    /// 用于在 Photon Fusion 中传输玩家输入
    /// </summary>
    public struct NetworkInputData : INetworkInput
    {
        public FuncKeyMask KeyMask;  // 按键状态
        public byte Buttons;         // 额外按钮状态（按下/抬起）
        
        // 可扩展字段（例如摇杆方向、鼠标位置等）
        // public Vector2 MoveDirection;
        // public Vector2 AimPosition;
    }
}
```

---

## 常见问题排查

### Q1：回放 hash 不一致怎么办？

**排查步骤**：

1. **检查随机数来源**
   ```bash
   # 在项目中搜索所有 UnityEngine.Random 调用
   # Windows PowerShell:
   Get-ChildItem -Path "Assets\NTSD\Scripts" -Recurse -Filter "*.cs" | Select-String "UnityEngine.Random"
   ```

2. **检查时间依赖**
   ```bash
   # 搜索 Time/DateTime 依赖
   Get-ChildItem -Path "Assets\NTSD\Scripts\Simulation" -Recurse -Filter "*.cs" | Select-String "Time\.time|Time\.deltaTime|DateTime\.Now"
   ```

3. **检查集合遍历顺序**
   - 确保所有 `Dictionary` / `HashSet` 遍历前排序
   - 或使用 `SortedDictionary` / `SortedSet`

4. **检查浮点运算顺序**
   - `a + b + c` 和 `c + b + a` 可能因精度问题产生不同结果
   - 确保运算顺序固定

5. **打印详细日志**
   ```csharp
   // 在 hash 不一致时打印详细状态
   if (hash1 != hash2)
   {
       foreach (var obj in objects)
       {
           Debug.Log($"Obj {obj.StableId}: pos=({obj.PS.x},{obj.PS.y}), hp={obj.Health.Hp}, frame={obj.Frame.N}");
       }
   }
   ```

---

### Q2：Photon Fusion 连接失败？

**错误：InvalidAuthentication**
- **原因**：App ID 错误
- **解决**：
  1. 检查 `PhotonAppSettings.asset` 中的 App ID
  2. 重新从 Dashboard 复制 App ID
  3. 确保选择的是 **Fusion** 应用（不是 PUN）

**错误：Timeout**
- **原因**：网络问题
- **解决**：
  1. 检查防火墙设置
  2. 尝试切换网络（手机热点）
  3. 指定区域服务器：
     ```csharp
     var result = await runner.StartGame(new StartGameArgs()
     {
         // ...
         CustomPhotonAppSettings = new AppSettings()
         {
             FixedRegion = "asia"  // 亚洲服务器
         }
     });
     ```

**错误：CCU Limit Exceeded**
- **原因**：超过 20 CCU 限制
- **解决**：
  1. 关闭其他测试客户端
  2. 或升级到付费版

---

### Q3：联机后角色位置不同步？

**排查步骤**：

1. **检查 StableId 是否一致**
   ```csharp
   // 在两端打印所有对象的 StableId
   foreach (var obj in world.GetAllLivingObjects())
   {
       Debug.Log($"[Client] Obj StableId={obj.StableId}, pos=({obj.PS.x},{obj.PS.y})");
   }
   ```

2. **检查输入是否正确注入**
   ```csharp
   // 在 SimInputBuffer.EnqueueForTick 中打印
   Debug.Log($"[Input] Tick={tick}, Key={key}, Down={down}");
   ```

3. **检查 tick 是否对齐**
   ```csharp
   // 在 FixedUpdateNetwork 中打印
   Debug.Log($"[Tick] Current={runner.Simulation.Tick}");
   ```

4. **检查是否有本地预测干扰**
   - 确保表现层不会反向修改 `PhysicsState`
   - 确保 `Transform.position` 只从 `PhysicsState` 读取，不写入

---

### Q4：输入延迟太高/太低怎么调整？

**调整 InputDelayTicks**：
```csharp
var result = await runner.StartGame(new StartGameArgs()
{
    // ...
    CustomConfig = new NetworkProjectConfig()
    {
        Simulation = new SimulationConfig()
        {
            InputDelayTicks = 6,  // 调整这个值
            // 2 = 66ms（格斗游戏）
            // 4 = 133ms（动作游戏）
            // 6 = 200ms（MOBA/RTS）
            // 8 = 266ms（回合制）
        }
    }
});
```

**经验值**：
- 延迟越低 → 手感越好，但网络要求越高
- 延迟越高 → 容错越好，但操作手感延迟

**建议**：
- 先用 6 帧测试
- 根据实际网络环境调整（4~8 帧）

---

## 工作量估算

| Phase | 内容 | 预估时间 | 难度 |
|-------|------|---------|------|
| **Phase A1** | 替换随机数 | 1-2 天 | ⭐⭐ |
| **Phase A2** | 录制回放测试 | 2-3 天 | ⭐⭐⭐ |
| **Phase B1** | Fusion 配置 | 1 天 | ⭐ |
| **Phase B2** | 输入同步 + 输入确认机制 | 4-5 天 | ⭐⭐⭐⭐ |
| **Phase B3** | StableId 修复 + 输入校验 | 2-3 天 | ⭐⭐⭐ |
| **Phase C1** | 输入延迟配置 + 网络监控 | 2-3 天 | ⭐⭐⭐ |
| **Phase C2** | 输入预测 + 重传 + 追帧 | 3-4 天 | ⭐⭐⭐⭐ |
| **Phase D1** | Hash 校验 + 帧号对齐 + 日志 | 3-4 天 | ⭐⭐⭐⭐ |
| **测试调优** | 联调与压测 | 3-5 天 | ⭐⭐⭐⭐ |
| **总计** | | **19-30 天** | |

*注：增加了 ET8 关键措施后，工作量从 16-25 天增加到 19-30 天*

**假设条件**：
- 每天投入 4-6 小时
- 有一定 Unity 和网络编程经验
- 遇到问题能自行排查或求助

**实际可能更快**：
- 如果你对 Photon 熟悉：减少 3-5 天
- 如果你的核心已经很确定性：减少 2-3 天

**实际可能更慢**：
- 如果遇到复杂的确定性问题：增加 5-10 天
- 如果需要大量重构：增加 10-20 天

---

## Phase C2 超时阈值计算说明

### 超时阈值：90 帧 = 3 秒

**计算公式**：
```
超时阈值（帧） = 超时时间（秒） × Tick Rate（fps）
90 帧 = 3 秒 × 30 fps
```

**为什么是 3 秒？**
- 太短（< 1 秒）：正常网络波动也会误踢
- 太长（> 5 秒）：真掉线玩家会长时间影响游戏
- 3 秒：平衡点，既能容忍短暂卡顿，又能快速踢出掉线玩家

**可调整范围**：
- **格斗游戏**：1-2 秒（30-60 帧）- 要求极高
- **动作游戏**：2-3 秒（60-90 帧）- 推荐
- **MOBA/RTS**：3-5 秒（90-150 帧）- 容错高

**实现示例**：
```csharp
const int TIMEOUT_THRESHOLD = 90;  // 3 秒 @ 30fps

// 每帧检查
foreach (var player in players)
{
    if (player.PredictionCount > TIMEOUT_THRESHOLD)
    {
        Debug.LogWarning($"玩家 {player.Id} 超时 {player.PredictionCount} 帧，踢出");
        KickPlayer(player);
    }
}
```

---

## 快速启动检查清单

在开始实施前，确认以下条件：

### 环境准备
- [ ] Unity 版本：2021.3+ 或 2022.3+
- [ ] .NET 版本：.NET Standard 2.1 或 .NET Framework 4.x
- [ ] Git 已配置（用于版本控制）
- [ ] 项目已备份

### 知识准备
- [ ] 理解帧同步原理（vs 状态同步）
- [ ] 理解确定性概念
- [ ] 熟悉 Unity 基本网络概念
- [ ] 会使用 Unity Profiler（性能分析）

### 工具准备
- [ ] 至少 2 台设备用于测试（或双开 Unity Editor）
- [ ] 网络测试工具（可选，例如 Clumsy 模拟延迟/丢包）
- [ ] 文本编辑器（VS Code / Rider）

### 项目状态确认
- [ ] 当前战斗系统可正常运行
- [ ] 已阅读 `NTSD_Lockstep_Framework_Plan.md`
- [ ] 已阅读 `NTSD_Lockstep_Risk_Assessment.md`
- [ ] 已阅读 `NTSD_Lockstep_Execution_Guide.md`
- [ ] 理解现有 SimulationWorld/SimInputBuffer 架构

**全部确认后，开始 Phase A1！**
