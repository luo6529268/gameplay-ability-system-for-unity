# Photon Fusion 完整集成指南

> 本文档提供 Photon Fusion 接入 NTSD 的完整代码和步骤

---

## 1. Photon Fusion SDK 安装

### 方式 A：Asset Store（推荐）
1. Unity 菜单 → Window → Asset Store
2. 搜索 "Photon Fusion"
3. 下载并导入

### 方式 B：官网下载
1. 访问：https://doc.photonengine.com/fusion/current/getting-started/sdk-download
2. 下载 `.unitypackage`
3. 拖入 Unity 项目导入

### 方式 C：Package Manager
```
Window → Package Manager → + → Add package from git URL
输入：com.photonengine.fusion
```

---

## 2. Photon 账号注册与 App ID 获取

### Step 1：注册账号
1. 访问：https://www.photonengine.com/
2. 点击 "Sign Up" 注册
3. 验证邮箱

### Step 2：创建 Fusion 应用
1. 登录后进入 Dashboard：https://dashboard.photonengine.com/
2. 点击 "Create a New App"
3. **Photon Type** 选择：**Fusion**（重要！不要选 PUN）
4. **Name**：填写应用名称（例如：NTSD_Lockstep）
5. 勾选 "Sandbox Mode"（测试用）
6. 点击 "Create"

### Step 3：获取 App ID
1. 创建成功后，复制显示的 App ID（类似：`12345678-abcd-1234-5678-1234567890ab`）

### Step 4：在 Unity 中配置
1. Unity 菜单 → Fusion → Realtime Settings
2. 找到 "App Id Fusion" 字段
3. 粘贴你的 App ID
4. 保存

---

## 3. 验证连接（测试 App ID 是否正确）

### 创建测试脚本

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

### 测试步骤
1. 创建空 GameObject，命名为 "FusionTest"
2. 挂载 `FusionConnectionTest` 脚本
3. 运行游戏
4. 查看 Console：
   - ✅ "Photon Fusion 连接成功！" → 配置正确
   - ❌ "连接失败：InvalidAuthentication" → App ID 错误，重新检查

---

## 4. 网络输入数据结构

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
        public int KeyMask;  // FuncKeyMask 转为 int（Fusion 不支持 enum）
        
        // 可扩展字段
        // public Vector2 MoveDirection;
        // public Vector2 AimPosition;
    }
}
```

---

## 5. 完整的 Fusion 帧同步管理器

**路径**：`Assets/NTSD/Scripts/Netcode/Fusion/FusionLockstepManager.cs`

```csharp
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using NTSD.Simulation;
using NTSD.Input;

namespace NTSD.Netcode
{
    /// <summary>
    /// Photon Fusion 帧同步管理器
    /// 负责：房间创建/加入、输入采集/同步、驱动 SimulationWorld
    /// </summary>
    public class FusionLockstepManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [Header("配置")]
        [SerializeField] private int inputDelayTicks = 6;  // 输入延迟（6 帧 = 200ms @ 30fps）
        [SerializeField] private int tickRate = 30;        // Tick 频率（必须和 SimulationTickDriver 一致）
        
        [Header("调试")]
        [SerializeField] private bool debugLog = true;
        
        private NetworkRunner _runner;
        private SimulationWorld _world;
        private SimInputBuffer _inputBuffer;
        
        // 本地输入缓存（用于 OnInput 回调）
        private FuncKeyMask _currentKeyMask;
        
        // ==================== 公共 API ====================
        
        /// <summary>
        /// 创建房间（Host 模式）
        /// </summary>
        public async void CreateRoom(string roomName, int maxPlayers = 20)
        {
            if (_runner != null)
            {
                Debug.LogError("[Fusion] 已经在房间中");
                return;
            }
            
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            
            var result = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Shared,  // 共享模式（帧同步 + 预测）
                SessionName = roomName,
                PlayerCount = maxPlayers,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                
                // 关键配置
                CustomConfig = new NetworkProjectConfig()
                {
                    Simulation = new SimulationConfig()
                    {
                        InputDelayTicks = inputDelayTicks,  // 输入延迟
                        TickRate = tickRate,                // Tick 频率
                        InputTransferMode = InputTransferMode.Synchronous
                    }
                }
            });
            
            if (result.Ok)
            {
                Debug.Log($"✅ [Fusion] 房间创建成功：{roomName}");
                InitializeSimulation();
            }
            else
            {
                Debug.LogError($"❌ [Fusion] 房间创建失败：{result.ShutdownReason}");
            }
        }
        
        /// <summary>
        /// 加入房间（Client 模式）
        /// </summary>
        public async void JoinRoom(string roomName)
        {
            if (_runner != null)
            {
                Debug.LogError("[Fusion] 已经在房间中");
                return;
            }
            
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            
            var result = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = roomName,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                
                CustomConfig = new NetworkProjectConfig()
                {
                    Simulation = new SimulationConfig()
                    {
                        InputDelayTicks = inputDelayTicks,
                        TickRate = tickRate,
                        InputTransferMode = InputTransferMode.Synchronous
                    }
                }
            });
            
            if (result.Ok)
            {
                Debug.Log($"✅ [Fusion] 加入房间成功：{roomName}");
                InitializeSimulation();
            }
            else
            {
                Debug.LogError($"❌ [Fusion] 加入房间失败：{result.ShutdownReason}");
            }
        }
        
        /// <summary>
        /// 离开房间
        /// </summary>
        public void LeaveRoom()
        {
            if (_runner != null)
            {
                _runner.Shutdown();
                _runner = null;
                Debug.Log("[Fusion] 已离开房间");
            }
        }
        
        // ==================== 初始化 ====================
        
        private void InitializeSimulation()
        {
            // 获取 SimulationWorld 和 SimInputBuffer
            _world = SimulationTickDriver.Instance?.World;
            _inputBuffer = SimInputBuffer.Instance;  // 假设你有单例
            
            if (_world == null)
            {
                Debug.LogError("[Fusion] SimulationWorld 未找到！");
                return;
            }
            
            if (_inputBuffer == null)
            {
                Debug.LogError("[Fusion] SimInputBuffer 未找到！");
                return;
            }
            
            Debug.Log("[Fusion] 模拟系统初始化完成");
        }
        
        // ==================== Fusion 回调 ====================
        
        /// <summary>
        /// 采集本地输入（每帧调用）
        /// </summary>
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            // 从 Unity Input System 读取输入
            var data = new NetworkInputData();
            
            // 示例：键盘输入映射到 FuncKeyMask
            FuncKeyMask keyMask = FuncKeyMask.None;
            
            if (UnityEngine.Input.GetKey(KeyCode.A)) keyMask |= FuncKeyMask.Left;
            if (UnityEngine.Input.GetKey(KeyCode.D)) keyMask |= FuncKeyMask.Right;
            if (UnityEngine.Input.GetKey(KeyCode.W)) keyMask |= FuncKeyMask.Up;
            if (UnityEngine.Input.GetKey(KeyCode.S)) keyMask |= FuncKeyMask.Down;
            if (UnityEngine.Input.GetKey(KeyCode.J)) keyMask |= FuncKeyMask.Attack;
            if (UnityEngine.Input.GetKey(KeyCode.K)) keyMask |= FuncKeyMask.Jump;
            if (UnityEngine.Input.GetKey(KeyCode.L)) keyMask |= FuncKeyMask.Defend;
            
            data.KeyMask = (int)keyMask;
            
            // 发送给 Fusion
            input.Set(data);
            
            // 缓存本地输入（用于调试）
            _currentKeyMask = keyMask;
        }
        
        /// <summary>
        /// 输入缺失时的回调（用于预测）
        /// </summary>
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            // Fusion 会自动重复上一帧输入（默认行为）
            // 你也可以在这里自定义预测逻辑
            if (debugLog)
            {
                Debug.LogWarning($"[Fusion] 玩家 {player} 输入缺失，使用预测");
            }
        }
        
        /// <summary>
        /// 固定更新网络（Fusion 的 FixedUpdate）
        /// 这里驱动 SimulationWorld
        /// </summary>
        public void FixedUpdateNetwork()
        {
            if (_world == null || _inputBuffer == null) return;
            
            int tick = _runner.Simulation.Tick;
            
            // 1. 收集所有玩家的输入
            foreach (var player in _runner.ActivePlayers)
            {
                if (_runner.TryGetInputForPlayer(player, out NetworkInputData input))
                {
                    FuncKeyMask keyMask = (FuncKeyMask)input.KeyMask;
                    
                    // 注入到 SimInputBuffer
                    // 注意：这里简化处理，实际需要区分按下/抬起
                    _inputBuffer.EnqueueForTick(tick, keyMask, true);
                    
                    if (debugLog && keyMask != FuncKeyMask.None)
                    {
                        Debug.Log($"[Fusion] Tick {tick}, Player {player}, Input: {keyMask}");
                    }
                }
            }
            
            // 2. 驱动 SimulationWorld（这里假设 SimulationTickDriver 已经在驱动）
            // 如果你想手动驱动，可以调用：
            // _world.TransitTickAll(tick);
            // _world.TUTickAll(tick);
            // _world.LateTick(tick);
            
            // 注意：通常不需要在这里手动驱动，因为 SimulationTickDriver 已经在 FixedUpdate 中驱动了
            // 但你需要确保 Fusion 的 tick 和 SimulationTickDriver 的 tick 同步
        }
        
        // ==================== 其他回调（可选实现） ====================
        
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[Fusion] 玩家加入：{player}");
        }
        
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[Fusion] 玩家离开：{player}");
        }
        
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log($"[Fusion] 连接关闭：{shutdownReason}");
            _runner = null;
        }
        
        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("[Fusion] 已连接到服务器");
        }
        
        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            Debug.Log("[Fusion] 已断开服务器连接");
        }
        
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.LogError($"[Fusion] 连接失败：{reason}");
        }
        
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

---

## 6. 使用示例

### 创建简单的 UI 控制器

**路径**：`Assets/NTSD/Scripts/UI/LockstepRoomUI.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using NTSD.Netcode;

namespace NTSD.UI
{
    /// <summary>
    /// 房间 UI 控制器
    /// </summary>
    public class LockstepRoomUI : MonoBehaviour
    {
        [SerializeField] private InputField roomNameInput;
        [SerializeField] private Button createButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button leaveButton;
        
        private FusionLockstepManager _fusionManager;
        
        void Start()
        {
            // 获取或创建 FusionLockstepManager
            _fusionManager = FindObjectOfType<FusionLockstepManager>();
            if (_fusionManager == null)
            {
                var go = new GameObject("FusionLockstepManager");
                _fusionManager = go.AddComponent<FusionLockstepManager>();
                DontDestroyOnLoad(go);
            }
            
            // 绑定按钮事件
            createButton.onClick.AddListener(OnCreateRoom);
            joinButton.onClick.AddListener(OnJoinRoom);
            leaveButton.onClick.AddListener(OnLeaveRoom);
        }
        
        void OnCreateRoom()
        {
            string roomName = roomNameInput.text;
            if (string.IsNullOrEmpty(roomName))
            {
                roomName = "Room_" + Random.Range(1000, 9999);
            }
            
            _fusionManager.CreateRoom(roomName, maxPlayers: 4);
        }
        
        void OnJoinRoom()
        {
            string roomName = roomNameInput.text;
            if (string.IsNullOrEmpty(roomName))
            {
                Debug.LogError("请输入房间名");
                return;
            }
            
            _fusionManager.JoinRoom(roomName);
        }
        
        void OnLeaveRoom()
        {
            _fusionManager.LeaveRoom();
        }
    }
}
```

---

## 7. 关键注意事项

### 7.1 Tick 同步问题

**问题**：Fusion 的 tick 和 SimulationTickDriver 的 tick 可能不同步

**解决方案 A**：让 Fusion 驱动 SimulationWorld
```csharp
// 在 FusionLockstepManager.FixedUpdateNetwork() 中
public void FixedUpdateNetwork()
{
    int tick = _runner.Simulation.Tick;
    
    // 手动驱动 SimulationWorld
    _world.TransitTickAll(tick);
    _world.TUTickAll(tick);
    _world.LateTick(tick);
}
```

**解决方案 B**：禁用 SimulationTickDriver，完全由 Fusion 驱动
```csharp
// 在联机模式下暂停 SimulationTickDriver
SimulationTickDriver.Instance.SetPaused(true);
```

---

### 7.2 输入按下/抬起问题

**问题**：当前代码只处理了"按住"状态，没有区分"按下"和"抬起"

**解决方案**：扩展 NetworkInputData
```csharp
public struct NetworkInputData : INetworkInput
{
    public int KeyMask;      // 当前按住的键
    public int KeyDown;      // 本帧按下的键
    public int KeyUp;        // 本帧抬起的键
}
```

然后在 OnInput 中检测变化：
```csharp
private FuncKeyMask _lastKeyMask;

public void OnInput(NetworkRunner runner, NetworkInput input)
{
    FuncKeyMask currentMask = GetCurrentKeyMask();
    
    var data = new NetworkInputData();
    data.KeyMask = (int)currentMask;
    data.KeyDown = (int)(currentMask & ~_lastKeyMask);  // 新按下的
    data.KeyUp = (int)(_lastKeyMask & ~currentMask);    // 新抬起的
    
    input.Set(data);
    _lastKeyMask = currentMask;
}
```

---

### 7.3 StableId 分配问题

**问题**：联机时需要 Host 统一分配 StableId

**解决方案**：在创建对象时检查
```csharp
// 在创建角色时
if (_runner.IsServer)
{
    // Host 分配 StableId
    int stableId = _world.AllocateStableId();
    // 通过 RPC 广播给所有客户端
    RPC_CreateCharacter(stableId, ...);
}
```

---

## 8. 测试步骤

### 本机双开测试

1. **准备两个 Unity Editor 实例**
   - 方式 A：打开两个 Unity Editor（需要两个项目副本）
   - 方式 B：一个 Editor + 一个打包的 exe

2. **实例 1：创建房间**
   - 输入房间名：`TestRoom`
   - 点击"创建房间"
   - 等待连接成功

3. **实例 2：加入房间**
   - 输入房间名：`TestRoom`
   - 点击"加入房间"
   - 等待连接成功

4. **测试同步**
   - 在实例 1 中移动角色
   - 观察实例 2 是否同步
   - 反之亦然

---

## 9. 常见问题

### Q1：连接失败 InvalidAuthentication
- **原因**：App ID 错误
- **解决**：重新检查 Fusion → Realtime Settings 中的 App ID

### Q2：找不到房间
- **原因**：房间名输入错误或房间已满
- **解决**：确认房间名一致，检查 maxPlayers 设置

### Q3：输入不同步
- **原因**：tick 不对齐或输入注入错误
- **解决**：在 FixedUpdateNetwork 中打印 tick 和 input，检查是否一致

### Q4：角色位置抖动
- **原因**：Fusion 的预测和 SimulationWorld 的推进冲突
- **解决**：确保只有一个地方驱动 SimulationWorld（要么 Fusion，要么 SimulationTickDriver）

---

## 10. 下一步

完成 Fusion 集成后，继续：
- Phase C1：调整 InputDelayTicks（测试不同延迟）
- Phase C2：实现输入预测策略
- Phase D1：添加 Hash 校验

参考主文档：`NTSD_Lockstep_Execution_Guide.md`
