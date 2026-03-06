# NTSD 帧同步项目配置

> **重要**：项目特定配置集中管理，避免混淆
> 最后更新：2026-03-02（项目初始化）

---

## 📋 配置更新规则

### Claude 必须遵守的规则

**每次修改配置后，必须**：
1. ✅ 更新本文件对应章节
2. ✅ 在"配置变更历史"中记录变更
3. ✅ 说明变更原因
4. ✅ 提醒用户推送到 Git

**配置变更格式**：
```markdown
| 时间 | 配置项 | 旧值 | 新值 | 原因 | 操作人 |
|------|--------|------|------|------|--------|
| 2026-03-03 | InputDelayTicks | 6 | 4 | 网络环境良好，降低延迟 | 电脑 A |
```

---

## ⚙️ 核心配置

### 帧同步参数
```yaml
# Tick 频率（必须和 SimulationTickDriver 一致）
TickRate: 30  # fps

# 输入延迟（根据网络质量调整）
InputDelayTicks: 6  # 帧数（200ms @ 30fps）
# 推荐范围：
#   - 网络很好（ping < 50ms）：3-4 帧
#   - 网络良好（ping 50-100ms）：4-5 帧
#   - 网络一般（ping 100-150ms）：5-6 帧
#   - 网络较差（ping 150-200ms）：6-8 帧

# 超时阈值（连续预测多少帧后踢出玩家）
TimeoutThreshold: 90  # 帧数（3 秒 @ 30fps）

# 回滚窗口（保留多少帧的快照）
RollbackWindow: 60  # 帧数（2 秒 @ 30fps）

# 输入队列清理阈值
InputBufferCleanup: 60  # 帧数（保留最近 60 帧）
```

### 确定性参数
```yaml
# 随机数种子
RandomSeed:
  SinglePlayer: 12345  # 单机模式固定种子
  Multiplayer: "由服务器下发（基于房间 ID + 时间戳）"

# Hash 计算精度
HashPrecision: 0.001  # 位置/速度精度（避免浮点误差）

# Hash 计算频率
HashCheckInterval: 30  # 帧数（每 1 秒检查一次）
```

---

## 🌐 Photon Fusion 配置

### 基本信息
```yaml
# App ID（从 Photon Dashboard 获取）
AppID: "12345678-abcd-1234-5678-1234567890ab"

# 模式
Mode: "Sandbox"  # Sandbox（测试）/ Production（正式）

# 区域
Region: "asia"  # 亚洲服务器（新加坡/日本/香港）
# 可选：us, eu, asia, usw

# 最大玩家数
MaxPlayers: 20

# 游戏模式
GameMode: "Shared"  # 共享模式（帧同步 + 预测）
```

### 网络参数
```yaml
# 客户端发送频率
ClientSendRate: 30  # 每秒发送 30 次

# 输入传输模式
InputTransferMode: "Synchronous"  # 同步模式

# 连接超时
ConnectionTimeout: 10000  # 毫秒（10 秒）
```

---

## 🎮 Unity 配置

### 项目信息
```yaml
# Unity 版本
UnityVersion: "2022.3.15f1"

# .NET 版本
DotNetVersion: ".NET Standard 2.1"

# Scripting Backend
ScriptingBackend:
  Development: "Mono"  # 开发模式（编译快）
  Release: "IL2CPP"    # 发布模式（性能好）

# API Compatibility Level
APICompatibility: ".NET Standard 2.1"
```

### 项目路径
```yaml
# 电脑 A
PathA: "D:\\Test\\gameplay-ability-system-for-unity"

# 电脑 B
PathB: "D:\\Projects\\NTSD"

# 注意：路径不同时，确保相对路径一致
```

---

## 🎯 项目特定规则

### StableId 分配规则
```yaml
# 玩家角色
PlayerCharacters: 1-99

# AI 角色
AICharacters: 100-199

# 道具/武器
Items: 200-299

# 特效/临时对象
Effects: 300+

# 分配方式
SinglePlayer: "本地自增（从对应范围起始值开始）"
Multiplayer: "服务器统一分配并广播"
```

### SimOrder 规则
```yaml
# 执行顺序（数字越小越先执行）
Input: -100        # 输入处理
Player: 0          # 玩家角色
AI: 10             # AI 角色
Projectile: 20     # 投射物
Effect: 30         # 特效
UI: 100            # UI 更新

# 同 SimOrder 内按 StableId 升序执行
```

### Hash 计算规则
```yaml
# 包含字段
IncludeFields:
  - StableId
  - Position (x, y, z)
  - Velocity (vx, vy, vz)
  - Health (Hp, Mp)
  - CurrentFrame
  - Direction (left/right)

# 排除字段（表现层，不影响逻辑）
ExcludeFields:
  - Transform.position
  - Animator.state
  - SpriteRenderer.sprite
  - ParticleSystem
```

---

## 🔧 开发环境配置

### 电脑 A
```yaml
OS: "Windows 11"
CPU: "Intel i7-12700K"
RAM: "32GB"
GPU: "RTX 3070"
IDE: "Rider 2023.3"
```

### 电脑 B
```yaml
OS: "Windows 10"
CPU: "AMD Ryzen 5 5600X"
RAM: "16GB"
GPU: "GTX 1660 Ti"
IDE: "Visual Studio 2022"
```

---

## 🧪 测试配置

### 本机测试
```yaml
# 双开 Unity Editor
Instance1: "Host（创建房间）"
Instance2: "Client（加入房间）"

# 测试房间名
TestRoomName: "TestRoom_Dev"
```

### 网络测试
```yaml
# 模拟延迟工具
Tool: "Clumsy"  # Windows 网络模拟工具
URL: "https://jagt.github.io/clumsy/"

# 测试场景
Scenarios:
  - Name: "良好网络"
    Ping: "50ms"
    PacketLoss: "0%"
    
  - Name: "一般网络"
    Ping: "100ms"
    PacketLoss: "2%"
    
  - Name: "较差网络"
    Ping: "200ms"
    PacketLoss: "5%"
```

---

## 📊 性能目标

### 帧率目标
```yaml
# 客户端
TargetFPS: 60  # Unity 渲染帧率

# 模拟层
SimulationFPS: 30  # 固定 Tick 频率

# 最低要求
MinimumFPS: 30  # 低于此值影响体验
```

### 内存目标
```yaml
# 单局游戏
MaxMemory: "500MB"  # 最大内存占用

# 快照系统
SnapshotMemory: "50MB"  # 快照缓存（60 帧）
```

### 网络目标
```yaml
# 带宽
UploadBandwidth: "50KB/s"    # 上行（每个玩家）
DownloadBandwidth: "100KB/s" # 下行（接收所有玩家）

# 延迟
TargetPing: "< 100ms"  # 目标延迟
MaxPing: "< 200ms"     # 最大可接受延迟
```

---

## 🔄 配置变更历史

| 时间 | 配置项 | 旧值 | 新值 | 原因 | 操作人 |
|------|--------|------|------|------|--------|
| 2026-03-02 | - | - | - | 初始化配置 | 初始化 |

---

## 💡 配置使用示例

### 场景 1：调整输入延迟

**用户反馈**：操作手感延迟太高

**Claude 的操作**：
1. 检查当前配置：`InputDelayTicks: 6`
2. 询问网络质量
3. 建议调整为 `4`
4. **更新本文件**：
   ```yaml
   InputDelayTicks: 4  # 帧数（133ms @ 30fps）
   ```
5. **记录变更**：
   ```markdown
   | 2026-03-03 | InputDelayTicks | 6 | 4 | 网络环境良好，降低延迟提升手感 | 电脑 A |
   ```
6. **提醒用户**：
   ```
   ✅ 配置已更新：InputDelayTicks 从 6 改为 4
   
   请同步修改代码：
   FusionLockstepManager.cs 中的 inputDelayTicks = 4
   
   然后推送到 Git：
   git add Assets/NTSD/CONFIG.md
   git commit -m "调整输入延迟：6 -> 4 帧"
   git push
   ```

---

### 场景 2：切换 Photon 模式

**用户说**：准备正式发布，切换到 Production 模式

**Claude 的操作**：
1. 检查当前配置：`Mode: "Sandbox"`
2. **更新本文件**：
   ```yaml
   Mode: "Production"  # 正式环境
   ```
3. **记录变更**：
   ```markdown
   | 2026-03-10 | Photon Mode | Sandbox | Production | 准备正式发布 | 电脑 A |
   ```
4. **提醒用户**：
   ```
   ⚠️ 切换到 Production 模式后：
   1. 需要在 Photon Dashboard 创建新的 Production 应用
   2. 获取新的 App ID
   3. 更新 Unity → Fusion → Realtime Settings
   4. 更新本文件的 AppID 配置
   
   参考：NTSD_Photon_Fusion_Integration_Guide.md 第 2 节
   ```

---

## 📌 注意事项

1. **配置同步**：修改配置后，确保代码中的硬编码值也同步修改
2. **环境区分**：开发/测试/正式环境的配置要分开管理
3. **敏感信息**：App ID 等敏感信息可以考虑用环境变量
4. **版本控制**：配置变更要记录到"配置变更历史"
5. **文档同步**：配置变更后，检查相关文档是否需要更新

---

## 🔗 相关文档

- 帧同步参数详解：`NTSD_Lockstep_Framework_Plan.md`
- Photon Fusion 配置：`NTSD_Photon_Fusion_Integration_Guide.md`
- 性能优化建议：`NTSD_Lockstep_Risk_Assessment.md`

---

**记住：配置是项目的"控制面板"，修改后必须记录并同步！**
