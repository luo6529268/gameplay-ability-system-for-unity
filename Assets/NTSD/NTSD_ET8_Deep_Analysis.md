# ET8 帧同步深度分析与 NTSD 对照

> 本文档基于 ET8 Release 2.0 源码深度分析
> 目标：让 Claude 在另一台电脑/新对话中能准确理解 ET8 的实现细节

---

## 📋 文档说明

### 为什么需要这个文档？

**问题**：
- 跨电脑/跨对话时，Claude 只能看到文档，不知道我们讨论过什么
- 遇到问题时，如果不知道 ET8 的设计意图，很难正确调试

**解决**：
- 深度分析 ET8 源码，提取关键逻辑和设计意图
- 逐行对照 ET8 和 NTSD 的实现
- 记录边界情况处理和调试思路

---

## 1. ET8 核心架构深度解析

### 1.1 核心类关系图

```
Room (对局管理)
├── LSWorld (确定性世界)
│   ├── LSUpdater (执行器)
│   │   └── SortedDictionary<long, LSEntity> (按 ID 排序)
│   ├── TSRandom (确定性随机)
│   ├── Frame (当前帧号)
│   └── EndFrame (结束帧号)
├── FrameBuffer (帧缓冲)
│   ├── frameMessages (输入缓存)
│   └── snapshots (快照缓存)
├── FixedTimeCounter (时间计数器)
├── ProcessLog (日志/Hash)
├── AuthorityFrame (权威帧)
├── PredictionFrame (预测帧)
└── Replay (录像)
```

### 1.2 ET8 vs NTSD 核心对照

| ET8 | NTSD | 一致性 | 说明 |
|-----|------|--------|------|
| `LSWorld` | `SimulationWorld` | ✅ 高度一致 | 都是确定性执行容器 |
| `LSUpdater` | `SimulationWorld._buckets` | ✅ 一致 | ET8 用 ID 排序，NTSD 用 SimOrder+StableId |
| `Room` | 缺失 | ❌ **需要补充** | NTSD 缺少对局管理层 |
| `FrameBuffer` | 缺失 | ❌ **需要补充** | NTSD 只有 SimInputBuffer |
| `AuthorityFrame/PredictionFrame` | 缺失 | ❌ **需要补充** | NTSD 没有区分权威帧和预测帧 |
| `ProcessLog` | 缺失 | ❌ **需要补充** | NTSD 只有简单的 Hash |

---

## 2. ET8 关键机制深度解析

### 2.1 权威帧 vs 预测帧（核心概念）

#### ET8 的实现

```csharp
// Room.cs
public int PredictionFrame { get; set; } = -1;  // 客户端预测到的帧
public int AuthorityFrame { get; set; } = -1;   // 服务器确认的帧
```

**关键逻辑**：
1. 客户端不等服务器，先预测推进（PredictionFrame）
2. 服务器返回权威输入后，更新 AuthorityFrame
3. 如果预测错误，回滚到 AuthorityFrame 重新执行

**代码证据**（Room2C_FrameMessageHandler.cs）：
```csharp
++room.AuthorityFrame;

// 服务端返回的消息比预测的还早（客户端落后了）
if (room.AuthorityFrame > room.PredictionFrame)
{
    // 直接使用服务器输入
    message.CopyTo(authorityFrameMessage);
}
else
{
    // 对比预测输入和服务器输入
    if (!message.Equals(predictionFrameMessage))
    {
        // 预测失败，回滚
        Log.Debug($"frame diff: {room.AuthorityFrame}");
        LSClientHelper.Rollback(room, room.AuthorityFrame);
    }
    else
    {
        // 预测成功
        room.Record(room.AuthorityFrame);
        room.SendHash(room.AuthorityFrame);
    }
}
```

#### NTSD 需要补充

**当前问题**：
- NTSD 只有一个 `currentTick`，没有区分权威和预测
- 无法实现"预测推进 + 服务器校正"

**建议实现**：
```csharp
// 在 SimulationWorld 或 Room 中添加
public int AuthorityTick { get; set; } = -1;   // 服务器确认的 tick
public int PredictionTick { get; set; } = -1;  // 客户端预测的 tick
```

---

### 2.2 输入预测与回滚（ET8 的核心优势）

#### ET8 的预测策略

**代码证据**（LSClientServerUpdaterSystem.cs）：
```csharp
private static Room2C_FrameMessage GetFrameMessage(this LSClientServerUpdater self, int frame)
{
    Room room = self.GetParent<Room>();
    FrameBuffer frameBuffer = room.FrameBuffer;
    Room2C_FrameMessage frameMessage = frameBuffer.GetFrameMessage(frame);
    
    // 若要获取的帧数据已经是服务器返回的直接用
    if (frame <= room.AuthorityFrame)
        return frameMessage;
    
    // 若没有服务器返回的帧数据 组织预测数据
    frameMessage.Frame = frame;
    frameMessage.FrameIndex = frame;
    
    LSCommandsComponent lsCommandsComponent = room.GetComponent<LSCommandsComponent>();
    lsCommandsComponent.AppendToFrameMessage(frame, frameMessage);
    
    return frameMessage;
}
```

**关键点**：
1. `frame <= AuthorityFrame`：使用服务器权威输入
2. `frame > AuthorityFrame`：使用本地预测输入（通常是重复上一帧）

#### ET8 的回滚机制

**代码证据**（LSClientHelper.cs）：
```csharp
public static void Rollback(Room room, int frame)
{
    room.IsRollback = true;
    
    // 1. 关闭日志（回滚过程不记录）
    room.ProcessLog.SetLogEnable(false);
    
    // 2. 销毁当前世界
    room.LSWorld.Dispose();
    
    // 3. 从快照恢复到 frame-1
    room.LSWorld = room.GetLSWorld(frame - 1);
    
    // 4. 重新开启日志
    room.ProcessLog.SetLogEnable(true);
    
    // 5. 执行 AuthorityFrame（使用服务器权威输入）
    Room2C_FrameMessage authorityFrameMessage = frameBuffer.GetFrameMessage(frame);
    room.Update(authorityFrameMessage);
    room.SendHash(frame);
    
    // 6. 重新执行预测的帧（AuthorityFrame+1 到 PredictionFrame）
    for (int i = room.AuthorityFrame + 1; i <= room.PredictionFrame; ++i)
    {
        Room2C_FrameMessage frameMessage = frameBuffer.GetFrameMessage(i);
        room.Update(frameMessage);
    }
    
    // 7. 通知表现层回滚
    RunLSRollbackSystem(room);
    
    room.IsRollback = false;
}
```

**关键步骤**：
1. 从快照恢复到 `frame-1`
2. 用服务器权威输入执行 `frame`
3. 重新执行 `frame+1` 到 `PredictionFrame` 的所有预测帧
4. 通知表现层更新

#### NTSD 需要补充

**当前问题**：
- NTSD 没有快照系统
- 无法回滚

**建议实现**：
参考 `NTSD_ET8_Critical_Features.md` 的 Phase E（快照/回滚）

---

### 2.3 FrameBuffer（ET8 的核心数据结构）

#### ET8 的实现

**代码证据**（FrameBuffer.cs）：
```csharp
public class FrameBuffer
{
    public int MaxFrame { get; private set; }
    private readonly List<Room2C_FrameMessage> frameMessages;  // 输入缓存
    private readonly List<MemoryBuffer> snapshots;             // 快照缓存
    
    public FrameBuffer(int frame = 0, int capacity = LSConstValue.FrameCountPerSecond * 60)
    {
        // MaxFrame = 当前帧 + 30 秒缓冲
        this.MaxFrame = frame + LSConstValue.FrameCountPerSecond * 30;
        
        // 预分配 60 秒容量（30fps × 60 = 1800 帧）
        this.frameMessages = new List<Room2C_FrameMessage>(capacity);
        this.snapshots = new List<MemoryBuffer>(capacity);
        
        // 预创建所有对象（避免运行时 GC）
        for (int i = 0; i < this.snapshots.Capacity; ++i)
        {
            this.frameMessages.Add(Room2C_FrameMessage.Create());
            MemoryBuffer memoryBuffer = new(204800);  // 200KB per snapshot
            this.snapshots.Add(memoryBuffer);
        }
    }
    
    public Room2C_FrameMessage GetFrameMessage(int frame)
    {
        EnsureFrame(frame);
        // 循环使用（ring buffer）
        Room2C_FrameMessage frameMessage = this.frameMessages[frame % this.frameMessages.Capacity];
        return frameMessage;
    }
    
    public MemoryBuffer Snapshot(int frame)
    {
        EnsureFrame(frame);
        // 循环使用（ring buffer）
        MemoryBuffer memoryBuffer = this.snapshots[frame % this.snapshots.Capacity];
        return memoryBuffer;
    }
    
    public void MoveForward(int frame)
    {
        // 至少留出 1 秒的空间
        if (this.MaxFrame - frame > LSConstValue.FrameCountPerSecond)
            return;
        
        ++this.MaxFrame;
        
        // 清理即将被覆盖的帧
        Room2C_FrameMessage frameMessage = this.GetFrameMessage(this.MaxFrame);
        frameMessage.Commands.Clear();
    }
}
```

**关键设计**：
1. **Ring Buffer**：循环使用固定大小的数组，避免频繁分配/释放
2. **预分配**：启动时分配所有内存，运行时零 GC
3. **双缓冲**：同时缓存输入（frameMessages）和快照（snapshots）
4. **容量管理**：动态调整 MaxFrame，保持 1 秒缓冲

#### NTSD 对照

**当前实现**（SimInputBuffer.cs）：
```csharp
private Dictionary<int, List<SimInputEvent>> _buffer = new Dictionary<int, List<SimInputEvent>>();
```

**问题**：
- 使用 Dictionary，每帧都要 new List（GC 压力）
- 没有快照缓存
- 没有容量限制（可能内存泄漏）

**建议改进**：
```csharp
public class FrameBuffer
{
    private const int CAPACITY = 1800;  // 60 秒 @ 30fps
    
    private Room2C_FrameMessage[] _frameMessages;  // Ring buffer
    private MemoryBuffer[] _snapshots;             // Ring buffer
    
    public int MaxFrame { get; private set; }
    
    public FrameBuffer(int startFrame)
    {
        _frameMessages = new Room2C_FrameMessage[CAPACITY];
        _snapshots = new MemoryBuffer[CAPACITY];
        
        // 预分配
        for (int i = 0; i < CAPACITY; i++)
        {
            _frameMessages[i] = new Room2C_FrameMessage();
            _snapshots[i] = new MemoryBuffer(204800);
        }
        
        MaxFrame = startFrame + 30 * 30;  // 30 秒缓冲
    }
    
    public Room2C_FrameMessage GetFrameMessage(int frame)
    {
        return _frameMessages[frame % CAPACITY];
    }
    
    public MemoryBuffer GetSnapshot(int frame)
    {
        return _snapshots[frame % CAPACITY];
    }
}
```

---

### 2.4 输入版本号机制（防止过期输入）

#### ET8 的实现

**代码证据**（LSCommand.cs）：
```csharp
// 指令版本号：每生成一条指令版本号+1，0~255循环
[StaticField] private static byte version = 0;

public static LSCommandData GenCommandFloat2(byte seatIndex, OperateCommandType type, float param1 = 0, float param2 = 0)
{
    LSCommandData command = new LSCommandData();
    command.Header |= seatIndex << 24;     // 前8位：座位索引
    command.Header |= (int)type << 16;     // 接下来8位：操作类型
    command.Header |= version++ & 0xFF;    // 最后8位：版本号（0-255循环）
    
    command.Param1 = (int)(param1 * 1000);
    command.Param2 = (int)(param2 * 1000);
    return command;
}
```

**用途**（OneFrameInputs.cs）：
```csharp
// 用于将本地已执行但服务器未执行的指令移动到本地的下一帧
// 如：本地执行指令1、2、3、4、5，服务器只执行3，那么认定1、2、3过期，4、5插入到下一帧
public void InsertTo(Room2C_FrameMessage to, Room2C_FrameMessage authority)
{
    if (authority.Commands.Count > 0)
    {
        byte authVersion = LSCommand.ParseCommandVersion(authority.Commands[^1]);
        for (int i = Commands.Count - 1; i >= 0; i--)
        {
            LSCommandData lsCommand = Commands[i];
            byte cmdVersion = LSCommand.ParseCommandVersion(lsCommand);
            
            // 计算版本差距（考虑 0-255 循环）
            int forwardDis = (cmdVersion - authVersion + 256) % 256;
            
            // 相同和旧的指令均认定过期
            if (forwardDis == 0 || forwardDis > 128)
                continue;
            
            // 新指令插入到下一帧
            to.Commands.Insert(0, lsCommand);
        }
    }
    else
    {
        to.Commands.InsertRange(0, Commands);
    }
}
```

**关键点**：
1. 每个输入都有版本号（0-255 循环）
2. 预测失败时，通过版本号判断哪些输入过期
3. 未过期的输入插入到下一帧继续执行

#### NTSD 需要补充

**当前问题**：
- NTSD 的输入没有版本号
- 预测失败时无法判断哪些输入过期

**建议实现**：
```csharp
public struct NetworkInputData : INetworkInput
{
    public int KeyMask;
    public byte Version;  // 新增：版本号
}

private static byte _inputVersion = 0;

public void OnInput(NetworkRunner runner, NetworkInput input)
{
    var data = new NetworkInputData();
    data.KeyMask = GetCurrentKeyMask();
    data.Version = _inputVersion++;  // 自动递增
    
    input.Set(data);
}
```

---

## 3. ET8 边界情况处理

### 3.1 客户端落后服务器

**场景**：客户端网络卡顿，AuthorityFrame 远远落后 PredictionFrame

**ET8 处理**（LSClientServerUpdaterSystem.cs）：
```csharp
// 限制预测帧数
if (room.PredictionFrame - room.AuthorityFrame > LSConstValue.PredictionFrameMaxCount)
    break;  // 停止预测，等待服务器
```

**关键常量**：
```csharp
public const int PredictionFrameMaxCount = 10;  // 最多预测 10 帧
```

**NTSD 建议**：
```csharp
const int MAX_PREDICTION_FRAMES = 10;

if (predictionTick - authorityTick > MAX_PREDICTION_FRAMES)
{
    // 停止推进，等待服务器输入
    Debug.LogWarning($"预测帧过多，等待服务器（{predictionTick - authorityTick} 帧）");
    return;
}
```

---

### 3.2 客户端超前服务器

**场景**：服务器返回的帧号比客户端预测的还早

**ET8 处理**（Room2C_FrameMessageHandler.cs）：
```csharp
// 服务端返回的消息比预测的还早
if (room.AuthorityFrame > room.PredictionFrame)
{
    // 直接使用服务器输入，不需要对比
    Room2C_FrameMessage authorityFrameMessage = frameBuffer.GetFrameMessage(room.AuthorityFrame);
    message.CopyTo(authorityFrameMessage);
}
```

**原因**：
- 客户端可能暂停/卡顿
- 服务器继续推进
- 客户端恢复后需要快速追上

**NTSD 建议**：
```csharp
if (authorityTick > predictionTick)
{
    // 客户端落后，直接使用服务器输入
    // 不需要对比和回滚
    predictionTick = authorityTick;
}
```

---

### 3.3 Hash 不一致处理

**场景**：服务器检测到客户端 Hash 不一致

**ET8 处理**（Room2C_CheckHashFailHandler.cs）：
```csharp
// 1. 保存客户端日志
using var stream = room.ProcessLog.GetLogStream();
using var ms = new MemoryStream();
BZip2.Compress(stream, ms, false, 6);
await fileAddressComponent.UploadFile(ms.ToArray(), LSConstValue.ProcessFolderName, filename);

// 2. 保存服务器日志
await fileAddressComponent.UploadFile(message.LSProcessBytes, LSConstValue.ProcessFolderNameSvr, filename);

// 3. 保存客户端世界状态
LSWorld clientWorld = room.GetLSWorld(message.Frame);
await fileAddressComponent.UploadFile(clientWorld.ToJson().ToUtf8(), LSConstValue.LSWroldFolderName, filename);

// 4. 保存服务器世界状态
LSWorld serverWorld = MemoryPackHelper.Deserialize(typeof(LSWorld), message.LSWorldBytes, 0, message.LSWorldBytes.Length) as LSWorld;
await fileAddressComponent.UploadFile(serverWorld.ToJson().ToUtf8(), LSConstValue.LSWroldFolderNameSvr, filename);
```

**关键点**：
1. 不是简单记录日志，而是保存完整的世界状态
2. 同时保存客户端和服务器的状态，方便对比
3. 压缩后上传（减少带宽）

**NTSD 建议**：
```csharp
public void OnHashMismatch(int tick, int clientHash, int serverHash)
{
    Debug.LogError($"Hash 不一致！Tick={tick}, Client={clientHash}, Server={serverHash}");
    
    // 1. 保存客户端状态
    string clientState = JsonUtility.ToJson(GetWorldState(tick));
    File.WriteAllText($"hash_fail_client_{tick}.json", clientState);
    
    // 2. 保存服务器状态（如果服务器发送了）
    if (serverWorldState != null)
    {
        File.WriteAllText($"hash_fail_server_{tick}.json", serverWorldState);
    }
    
    // 3. 记录到 ISSUES.md
    // Claude 会自动记录
}
```

---

### 3.4 时间膨胀/收缩（追帧优化）

**场景**：客户端需要快速追上服务器进度

**ET8 处理**（LSClientServerUpdaterSystem.cs）：
```csharp
long timeNow = TimeInfo.Instance.ServerNow();

int i = 0;
while (timeNow >= room.FixedTimeCounter.FrameTime(room.PredictionFrame + 1))
{
    // 限制预测帧数
    if (room.PredictionFrame - room.AuthorityFrame > LSConstValue.PredictionFrameMaxCount)
        break;
    
    ++room.PredictionFrame;
    
    // ... 执行帧 ...
    
    room.SpeedMultiply = ++i;  // 记录本次推进了几帧
    
    // 防止单次推进过多帧（最多 5ms）
    long timeNow2 = TimeInfo.Instance.ServerNow();
    if (timeNow2 - timeNow > 5)
        break;
}
```

**关键点**：
1. 一次 Update 可能推进多帧（追帧）
2. 限制单次推进时间（5ms），避免卡顿
3. 记录 `SpeedMultiply`（表现层可以加速动画）

**NTSD 建议**：
参考 `NTSD_ET8_Critical_Features.md` 的追帧机制

---

## 4. ET8 性能优化细节

### 4.1 对象池（零 GC）

**ET8 的实现**：
```csharp
// Room2C_FrameMessage 使用对象池
Room2C_FrameMessage frameMessage = Room2C_FrameMessage.Create();  // 从池中取
using Room2C_FrameMessage _ = message;  // 自动回池
```

**NTSD 建议**：
- 输入事件使用对象池
- 快照 MemoryBuffer 复用

### 4.2 预分配（避免运行时分配）

**ET8 的实现**：
```csharp
// FrameBuffer 预分配所有内存
for (int i = 0; i < this.snapshots.Capacity; ++i)
{
    this.frameMessages.Add(Room2C_FrameMessage.Create());
    MemoryBuffer memoryBuffer = new(204800);  // 200KB
    this.snapshots.Add(memoryBuffer);
}
```

### 4.3 Ring Buffer（循环使用）

**ET8 的实现**：
```csharp
// 通过取模实现循环
Room2C_FrameMessage frameMessage = this.frameMessages[frame % this.frameMessages.Capacity];
```

---

## 5. ET8 调试工具

### 5.1 ProcessLog（详细日志）

**功能**：
- 记录每帧的所有操作
- 计算每帧的 Hash
- 可以导出对比

**NTSD 建议**：
参考 `NTSD_ET8_Critical_Features.md` 的日志系统

### 5.2 Replay（录像回放）

**ET8 的实现**：
```csharp
public partial class Replay
{
    public LockStepMatchInfo MatchInfo;           // 对局信息
    public List<Room2C_FrameMessage> FrameMessages;  // 所有帧输入
    public List<byte[]> Snapshots;                // 定期快照
    public long OwnerPlayerId;                    // 录像拥有者
}
```

**关键点**：
- 每帧记录输入
- 每 N 帧保存快照（用于快速跳转）
- 可以保存到文件

---

## 6. ET8 时间同步机制（关键！）

### 6.1 FixedTimeCounter（动态时间调整）

**ET8 的实现**（FixedTimeCounter.cs）：
```csharp
public class FixedTimeCounter
{
    private long startTime;      // 起始时间戳
    private int startFrame;      // 起始帧号
    public int Interval { get; private set; }  // 帧间隔（ms）
    
    public FixedTimeCounter(long startTime, int startFrame, int interval)
    {
        this.startTime = startTime;
        this.startFrame = startFrame;
        this.Interval = interval;  // 默认 33ms（30fps）
    }
    
    // 动态调整帧间隔
    public void ChangeInterval(int interval, int frame)
    {
        // 重新计算起始时间，保证连续性
        this.startTime += (frame - this.startFrame) * this.Interval;
        this.startFrame = frame;
        this.Interval = interval;
    }
    
    // 计算某帧应该执行的时间
    public long FrameTime(int frame)
    {
        return this.startTime + (frame - this.startFrame) * this.Interval;
    }
}
```

**关键点**：
- 不是固定 33ms，而是动态调整（30-66ms）
- 根据客户端和服务器的时间差，加速/减速客户端

### 6.2 时间同步流程

**客户端每秒发送时间校准请求**（LSClientServerUpdaterSystem.cs）：
```csharp
if (room.PredictionFrame % LSConstValue.FrameCountPerSecond == 0)
{
    C2Room_TimeAdjust timeAdjust = C2Room_TimeAdjust.Create(true);
    timeAdjust.Frame = room.PredictionFrame;
    room.Root().GetComponent<ClientSenderComponent>().Send(timeAdjust);
}
```

**服务器计算时间差**（C2Room_TimeAdjustHandler.cs）：
```csharp
if (message.Frame % LSConstValue.FrameCountPerSecond == 0)
{
    // 计算客户端该帧的时间 vs 服务器当前时间
    long nowFrameTime = room.FixedTimeCounter.FrameTime(message.Frame);
    int diffTime = (int)(nowFrameTime - TimeInfo.Instance.ServerFrameTime());
    
    // 发送时间差给客户端
    Room2C_TimeAdjust timeAdjust = Room2C_TimeAdjust.Create(true);
    timeAdjust.DiffTime = diffTime;
    gateSession.Send(roomPlayer.Id, timeAdjust);
}
```

**客户端调整帧间隔**（Room2C_TimeAdjustHandler.cs）：
```csharp
// diffTime > 0：客户端快了，需要减速
// diffTime < 0：客户端慢了，需要加速
int diff = message.DiffTime - LSConstValue.UpdateInterval;  // 额外慢一帧
int newInterval = (1000 + diff) * LSConstValue.UpdateInterval / 1000;

// 限制范围：40-66ms（15-25fps）
if (newInterval < 40) newInterval = 40;
if (newInterval > 66) newInterval = 66;

room.FixedTimeCounter.ChangeInterval(newInterval, room.PredictionFrame);
```

**关键设计**：
1. 客户端故意比服务器快 1 帧（减少回滚概率）
2. 动态调整帧间隔，而不是跳帧
3. 限制调整范围（40-66ms），避免过度加速/减速

### 6.3 NTSD 需要补充

**当前问题**：
- NTSD 使用固定 33.33ms，无法动态调整
- 客户端和服务器时间可能漂移

**建议实现**：
```csharp
public class FixedTimeCounter
{
    private long _startTime;
    private int _startTick;
    private int _interval = 33;  // 默认 33ms
    
    public void ChangeInterval(int newInterval, int currentTick)
    {
        _startTime += (currentTick - _startTick) * _interval;
        _startTick = currentTick;
        _interval = Mathf.Clamp(newInterval, 25, 50);  // 20-40fps
    }
    
    public long GetTickTime(int tick)
    {
        return _startTime + (tick - _startTick) * _interval;
    }
}
```

---

## 7. ET8 输入管理机制（精妙设计）

### 7.1 输入分类缓存

**ET8 的实现**（LSCommandsComponent.cs）：
```csharp
public class LSCommandsComponent
{
    public byte SeatIndex { get; set; }
    
    // 按帧号缓存，每帧有 3 种输入队列
    public List<Queue<LSCommandData>> FramesCommandsMove;    // 移动输入（FIFO）
    public List<List<LSCommandData>> FramesCommandsDrag;     // 拖拽输入（保留最新）
    public List<List<LSCommandData>> FramesCommandsNormal;   // 普通输入（按优先级）
}
```

**为什么分 3 类？**（LSCommandsComponentSystem.cs 注释）：
```csharp
// 按帧号缓存指令的意义：
// 客户端会动态调整自己的Tick频率，尽可能保证服务器收到指令时，
// 服务器还没有Tick到客户端发送指令时的客户端帧号
// 这样就尽可能的保证了客户端在对比预测帧和服务器帧的指令时，它们是一致的，也就不用回滚了
// 但当网络时延较大时，客户端的预测帧数量会增大，也就增加了回滚的概率
```

### 7.2 输入去重策略

**移动输入**（Move）：
```csharp
case OperateCommandType.Move:
{
    var commands = self.FramesCommandsMove[index];
    if (commands.Count > 0)  // 只保留最新的
        commands.Dequeue();
    commands.Enqueue(command);
    break;
}
```

**拖拽输入**（Drag）：
```csharp
case OperateCommandType.TouchDragStart:
{
    // 新的 DragStart 来时，移除所有旧的 Drag 相关指令
    var commands = self.FramesCommandsDrag[index];
    for (int i = commands.Count - 1; i >= 0; i--) {
        OperateCommandType cmdType = LSCommand.ParseCommandType(commands[i]);
        if (cmdType >= OperateCommandType.TouchDragStart && 
            cmdType <= OperateCommandType.TouchDragCancel)
            commands.RemoveAt(i);
    }
    commands.Add(command);
    break;
}
```

**按钮输入**（Button）：
```csharp
case OperateCommandType.Button:
{
    // 移除低优先级的按钮指令
    var commands = self.FramesCommandsNormal[index];
    var button = LSCommand.ParseCommandSubType(command);
    for (int i = commands.Count - 1; i >= 0; i--) {
        OperateCommandType cmdType = LSCommand.ParseCommandType(commands[i]);
        if (cmdType == OperateCommandType.Button && 
            button >= LSCommand.ParseCommandSubType(commands[i]))
            commands.RemoveAt(i);
    }
    commands.Add(command);
    break;
}
```

**关键设计**：
1. 移动输入：只保留最新（避免累积）
2. 拖拽输入：状态机式管理（Start → Drag → End/Cancel）
3. 按钮输入：按优先级去重（高优先级覆盖低优先级）

### 7.3 输入安全检查

**服务器强制覆盖座位索引**（LSCommandsComponentSystem.cs）：
```csharp
public static void AddCommand(this LSCommandsComponent self, int frame, LSCommandData command)
{
    // 覆盖座位索引 防止客户端模拟他人消息
    command.Header |= self.SeatIndex << 24;
    // ...
}
```

**关键点**：
- 客户端发送的 SeatIndex 不可信
- 服务器根据连接强制覆盖
- 防止作弊

### 7.4 NTSD 需要补充

**当前问题**：
- NTSD 的输入没有分类
- 没有去重策略
- 可能累积大量重复输入

**建议实现**：
```csharp
public class InputBuffer
{
    // 按 Tick 缓存，每个 Tick 有多个输入队列
    private Dictionary<int, InputFrame> _frames = new Dictionary<int, InputFrame>();
}

public class InputFrame
{
    public Queue<InputEvent> MoveInputs;      // 移动（只保留最新）
    public List<InputEvent> SkillInputs;      // 技能（按优先级）
    public List<InputEvent> InteractInputs;   // 交互（保留所有）
}
```

---

## 8. ET8 回滚系统接口（ILSRollbackSystem）

### 8.1 回滚接口设计

**ET8 的实现**（ILSRollbackSystem.cs）：
```csharp
public interface ILSRollback
{
}

public interface ILSRollbackSystem: ISystemType
{
    void Run(Entity o);
}

[LSEntitySystem]
public abstract class LSRollbackSystem<T>: SystemObject, ILSRollbackSystem 
    where T: Entity, ILSRollback
{
    protected abstract void LSRollback(T self);
}
```

**使用示例**：
```csharp
// 表现层组件实现 ILSRollback
public class LSUnitViewComponent : Entity, ILSRollback
{
    public GameObject GameObject;
    public Animator Animator;
}

// 实现回滚逻辑
[LSEntitySystem]
public class LSUnitViewComponentRollbackSystem : LSRollbackSystem<LSUnitViewComponent>
{
    protected override void LSRollback(LSUnitViewComponent self)
    {
        // 回滚后，同步表现层到逻辑层
        LSUnit lsUnit = self.GetParent<LSUnit>();
        self.GameObject.transform.position = lsUnit.Position.ToVector3();
        self.GameObject.transform.rotation = lsUnit.Rotation.ToQuaternion();
        
        // 重置动画状态
        self.Animator.Play("Idle", 0, 0);
    }
}
```

**关键点**：
1. 逻辑层（LSWorld）回滚后，表现层需要同步
2. 通过 `ILSRollback` 接口，自动调用所有表现层组件的回滚方法
3. 表现层回滚：同步位置、重置动画、清理特效等

### 8.2 回滚调用流程

**ET8 的实现**（LSClientHelper.cs）：
```csharp
public static void RunLSRollbackSystem(Entity entity)
{
    if (entity is LSEntity)
        return;  // LSEntity 不需要回滚（已通过快照恢复）
    
    // 调用该 Entity 的回滚系统
    LSEntitySystemSingleton.Instance.LSRollback(entity);
    
    // 递归调用所有 Component 的回滚系统
    if (entity.Components.Count > 0)
    {
        foreach (var component in entity.Components)
        {
            RunLSRollbackSystem(component);
        }
    }
    
    // 递归调用所有 Child 的回滚系统
    if (entity.Children.Count > 0)
    {
        foreach (var kv in entity.Children)
        {
            RunLSRollbackSystem(kv.Value);
        }
    }
}
```

**调用时机**：
```csharp
public static void Rollback(Room room, int frame)
{
    // 1. 恢复逻辑层快照
    room.LSWorld = room.GetLSWorld(frame - 1);
    
    // 2. 重新执行逻辑帧
    room.Update(authorityFrameMessage);
    for (int i = room.AuthorityFrame + 1; i <= room.PredictionFrame; ++i)
    {
        room.Update(frameBuffer.GetFrameMessage(i));
    }
    
    // 3. 同步表现层
    RunLSRollbackSystem(room);  // 递归调用所有表现层组件
}
```

### 8.3 NTSD 需要补充

**当前问题**：
- NTSD 没有回滚接口
- 表现层和逻辑层耦合

**建议实现**：
```csharp
public interface ISimRollback
{
    void OnRollback();
}

// 表现层组件实现接口
public class CharacterView : MonoBehaviour, ISimRollback
{
    public LF2Character LogicCharacter;
    
    public void OnRollback()
    {
        // 同步位置
        transform.position = LogicCharacter.Position;
        
        // 重置动画
        GetComponent<Animator>().Play("Idle", 0, 0);
        
        // 清理特效
        foreach (var effect in GetComponentsInChildren<ParticleSystem>())
        {
            effect.Clear();
        }
    }
}
```

---

## 9. NTSD 实施建议（基于 ET8 分析）

### 9.1 必须补充的核心功能

| 功能 | 优先级 | 工作量 | ET8 对应 | 说明 |
|------|--------|--------|----------|------|
| **AuthorityTick/PredictionTick** | 🔴 高 | 1-2 天 | Room.AuthorityFrame/PredictionFrame | 区分权威和预测 |
| **FrameBuffer（Ring Buffer）** | 🔴 高 | 2-3 天 | FrameBuffer.cs | 替代 Dictionary，零 GC |
| **FixedTimeCounter（动态时间）** | 🔴 高 | 1-2 天 | FixedTimeCounter.cs | 动态调整帧间隔 |
| **时间同步机制** | 🔴 高 | 1-2 天 | C2Room_TimeAdjust | 客户端/服务器时间对齐 |
| **输入版本号** | 🟠 中 | 1 天 | LSCommand.version | 防止过期输入 |
| **输入分类缓存** | 🟠 中 | 1-2 天 | LSCommandsComponent | 移动/拖拽/按钮分类 |
| **预测帧数限制** | 🟠 中 | 0.5 天 | PredictionFrameMaxCount | 防止无限预测 |
| **快照系统** | 🟡 中 | 3-5 天 | FrameBuffer.snapshots | 支持回滚 |
| **回滚机制** | 🟡 中 | 2-3 天 | LSClientHelper.Rollback | 预测失败时修正 |
| **回滚接口** | 🟡 中 | 1-2 天 | ILSRollbackSystem | 表现层同步 |

### 9.2 实施顺序（基于 ET8 分析）

#### Phase 1：基础框架（5-7 天）
**目标**：建立 ET8 同等的基础架构，不改现有逻辑

1. **添加 Room 层**（1 天）
   ```csharp
   public class Room
   {
       public int AuthorityTick { get; set; } = -1;
       public int PredictionTick { get; set; } = -1;
       public FrameBuffer FrameBuffer { get; set; }
       public FixedTimeCounter TimeCounter { get; set; }
       public SimulationWorld World { get; set; }
   }
   ```

2. **实现 FrameBuffer（Ring Buffer）**（2-3 天）
   - 参考 ET8 的 FrameBuffer.cs
   - 预分配 1800 帧容量（60 秒 @ 30fps）
   - 循环使用，零 GC

3. **实现 FixedTimeCounter**（1 天）
   - 参考 ET8 的 FixedTimeCounter.cs
   - 支持动态调整帧间隔（25-50ms）

4. **添加输入版本号**（1 天）
   - 参考 ET8 的 LSCommand.version
   - 每个输入自动递增版本号

5. **测试**（1 天）
   - 验证 Ring Buffer 正确性
   - 验证时间计算准确性

#### Phase 2：时间同步（3-4 天）
**目标**：实现客户端/服务器时间对齐

6. **客户端时间校准**（1 天）
   - 每秒发送 `C2Room_TimeAdjust`
   - 包含当前 PredictionTick

7. **服务器时间计算**（1 天）
   - 计算客户端时间 vs 服务器时间差
   - 返回 `Room2C_TimeAdjust`

8. **客户端动态调整**（1 天）
   - 根据时间差调整帧间隔
   - 限制范围（25-50ms）

9. **测试**（1 天）
   - 模拟网络延迟
   - 验证时间自动对齐

#### Phase 3：预测与回滚（7-10 天）
**目标**：实现 ET8 的核心优势

10. **输入预测**（1-2 天）
    - `frame <= AuthorityTick`：使用服务器输入
    - `frame > AuthorityTick`：使用本地预测

11. **预测帧数限制**（0.5 天）
    - 最多预测 10 帧
    - 超过则等待服务器

12. **快照系统**（3-4 天）
    - 每帧保存 SimulationWorld 快照
    - 使用 Ring Buffer 循环存储

13. **回滚机制**（2-3 天）
    - 对比预测输入 vs 服务器输入
    - 不一致时回滚到 AuthorityTick-1
    - 重新执行到 PredictionTick

14. **回滚接口**（1-2 天）
    - 定义 `ISimRollback` 接口
    - 表现层实现回滚逻辑

15. **测试**（1 天）
    - 模拟预测失败
    - 验证回滚正确性

#### Phase 4：输入优化（2-3 天）
**目标**：减少输入冗余，提升体验

16. **输入分类缓存**（1-2 天）
    - 移动输入：只保留最新
    - 技能输入：按优先级去重
    - 交互输入：保留所有

17. **输入去重策略**（1 天）
    - 参考 ET8 的 LSCommandsComponentSystem
    - 避免累积重复输入

18. **测试**（0.5 天）
    - 验证输入不重复
    - 验证优先级正确

---

### 9.3 关键风险点（基于 ET8 经验）

#### 风险 1：时间同步不稳定
**现象**：客户端帧间隔频繁变化，画面抖动

**ET8 的解决方案**：
- 限制调整范围（40-66ms）
- 平滑调整，不要突变

**NTSD 建议**：
```csharp
// 不要直接设置，而是平滑过渡
int targetInterval = CalculateTargetInterval(diffTime);
int currentInterval = TimeCounter.Interval;
int newInterval = Mathf.Lerp(currentInterval, targetInterval, 0.1f);
TimeCounter.ChangeInterval(newInterval, currentTick);
```

#### 风险 2：回滚频繁，表现抖动
**现象**：网络不稳定时，角色频繁瞬移

**ET8 的解决方案**：
- 客户端故意比服务器快 1 帧（减少回滚概率）
- 限制预测帧数（最多 10 帧）
- 表现层平滑插值

**NTSD 建议**：
```csharp
// 表现层不要直接设置位置，而是插值
public void OnRollback()
{
    Vector3 targetPos = LogicCharacter.Position;
    StartCoroutine(SmoothMove(transform.position, targetPos, 0.1f));
}
```

#### 风险 3：快照内存占用过大
**现象**：快照占用几百 MB 内存

**ET8 的解决方案**：
- 预分配固定大小（200KB per snapshot）
- Ring Buffer 循环使用
- 只保留 60 秒（1800 帧）

**NTSD 建议**：
```csharp
// 不要每帧 new，而是复用
MemoryBuffer buffer = FrameBuffer.GetSnapshot(tick);
buffer.Seek(0, SeekOrigin.Begin);
buffer.SetLength(0);
Serialize(World, buffer);
```

#### 风险 4：Hash 不一致难以排查
**现象**：Hash 不一致，但不知道哪里出错

**ET8 的解决方案**：
- 保存完整的客户端/服务器世界状态
- 导出 JSON 对比
- 详细日志记录每帧操作

**NTSD 建议**：
- 参考 `Room2C_CheckHashFailHandler.cs`
- 保存双端状态到文件
- 记录到 `ISSUES.md`

---

### 9.4 性能优化检查清单（ET8 经验）

#### 内存优化
- [ ] 使用 Ring Buffer（FrameBuffer）
- [ ] 预分配所有对象（启动时）
- [ ] 对象池（输入事件、快照）
- [ ] 限制快照容量（60 秒）

#### GC 优化
- [ ] 避免每帧 new（使用预分配）
- [ ] 避免 Dictionary 频繁增删（使用数组）
- [ ] 避免 List.Add（使用固定容量）
- [ ] 避免字符串拼接（使用 StringBuilder）

#### CPU 优化
- [ ] 限制单次推进帧数（最多 5ms）
- [ ] 快照序列化优化（MemoryPack）
- [ ] Hash 计算优化（只计算关键字段）
- [ ] 输入去重（避免重复处理）

#### 网络优化
- [ ] 输入压缩（位运算）
- [ ] 快照差分（只发送变化）
- [ ] 批量发送（减少包数量）
- [ ] 时间同步（减少回滚）

---

## 7. 关键代码片段速查

### 7.1 ET8 的 Update 循环

```csharp
// LSClientServerUpdaterSystem.cs
while (timeNow >= room.FixedTimeCounter.FrameTime(room.PredictionFrame + 1))
{
    if (room.PredictionFrame - room.AuthorityFrame > LSConstValue.PredictionFrameMaxCount)
        break;
    
    ++room.PredictionFrame;
    Room2C_FrameMessage frameMessage = self.GetFrameMessage(room.PredictionFrame);
    room.Update(frameMessage);
    room.SendHash(room.PredictionFrame);
}
```

### 7.2 ET8 的输入对比

```csharp
// Room2C_FrameMessageHandler.cs
if (!message.Equals(predictionFrameMessage))
{
    // 预测失败，回滚
    predictionFrameMessage.InsertTo(nextFrameMessage, message);
    message.CopyTo(predictionFrameMessage);
    LSClientHelper.Rollback(room, room.AuthorityFrame);
}
```

### 7.3 ET8 的快照保存

```csharp
// RoomSystem.cs
private static void SaveLSWorld(this Room self, int frame)
{
    MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
    memoryBuffer.Seek(0, SeekOrigin.Begin);
    memoryBuffer.SetLength(0);
    
    MemoryPackHelper.Serialize(self.LSWorld, memoryBuffer);
    memoryBuffer.Seek(0, SeekOrigin.Begin);
}
```

---

## 8. Claude 调试检查清单

### 当遇到不同步问题时

1. **检查权威帧和预测帧**
   - `AuthorityTick` 和 `PredictionTick` 的差距是否合理（< 10）
   - 是否有预测帧数限制

2. **检查输入版本号**
   - 预测失败时，是否正确判断过期输入
   - 未过期输入是否插入到下一帧

3. **检查 FrameBuffer**
   - 是否使用 Ring Buffer（避免 GC）
   - 容量是否足够（至少 60 秒）

4. **检查回滚**
   - 快照是否正确保存
   - 回滚后是否重新执行所有预测帧

5. **检查 Hash**
   - Hash 不一致时，是否保存了完整状态
   - 是否记录到 ISSUES.md

---

**记住：ET8 的核心优势是"预测 + 回滚"，NTSD 必须实现这两个机制才能达到同等稳定性。**
