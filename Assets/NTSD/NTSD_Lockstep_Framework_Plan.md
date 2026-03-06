# NTSD 联机帧同步（Lockstep）框架布置方案（核对清单）

> 目标：联机时战斗核心按帧同步推进（只传输入、不传状态）；当前项目核心目录为 `Assets/NTSD`。
> 本文只讨论架构与目录布置，不涉及代码修改。

---

## 0. 当前已有的“帧同步核心资产”（你可逐条核对）

### 0.1 唯一时钟源（Fixed Tick）
- 文件：`Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs`
- 特点：
  - 在 `FixedUpdate` 里累积 `Time.fixedDeltaTime`
  - 按 `SimulationConstants.SIM_DT` 以固定频率（当前为 30Hz）循环驱动
  - 每次调用 `RunOneSimTick(tickIndex)`

### 0.2 确定性执行容器（Deterministic Order）
- 文件：`Assets/NTSD/Scripts/Simulation/SimulationWorld.cs`
- 特点：
  - `SortedDictionary<int, Bucket>` 按 `SimOrder` 升序遍历
  - 同一 `SimOrder` 内按 `StableId` 升序（lazy sort）
  - 提供 `TransitTickAll / TUTickAll / LateTick` 三段式执行

### 0.3 按 Tick 对齐的输入缓冲（Tick-aligned Input）
- 文件：`Assets/NTSD/Scripts/Simulation/Input/SimInputBuffer.cs`
- 特点：
  - `EnqueueForNextTick(...)`：本地输入写入“下一帧”（避免同帧竞态）
  - `EnqueueForTick(tick, ...)`：可用于联机输入注入/回放
  - `TryDequeueAll(tick, out events)`：每 tick 消费一次

结论：
- NTSD 已具备 lockstep 的三大基石：`固定tick` + `确定性顺序` + `输入按tick对齐`。
- 后续要补的是：`联机输入分发/延迟窗口/校验/快照/回滚` 等 glue。

---

## 1. 推荐分层架构（目录布置建议）

> 原则：**战斗逻辑权威在“确定性核心层”**；Unity 表现层只渲染结果；网络层只传输入。

### Layer A：Deterministic Core（确定性核心）
- 建议目录：`Assets/NTSD/Scripts/Simulation/Core/`
- 放置内容：
  - `SimulationWorld`、`ISimObject`、`ISimTickable`、`SimContext`（现有）
  - 未来新增：
    - `DeterministicRng`（统一随机）
    - `WorldStateHash`（每 tick hash）
    - `Snapshot`（快照数据结构）

**约束**：
- Core 层尽量不依赖 UnityEngine 的行为（可以暂时有 log，但不要依赖 Transform/Physics/Time）。

### Layer B：Tick Driver（Unity 桥接的时钟驱动层）
- 建议目录：`Assets/NTSD/Scripts/Simulation/Driver/`
- 放置内容：
  - `SimulationTickDriver`（现有）

**约束**：
- Driver 层可以依赖 Unity（因为它负责 FixedUpdate），但不做战斗逻辑。

### Layer C：Input（输入帧系统：本地/联机/回放共用）
- 建议目录：`Assets/NTSD/Scripts/Simulation/Input/`
- 放置内容：
  - `SimInputBuffer`、`SimInputEvent`（现有）
  - 建议新增输入来源适配：
    - `LocalInputSource`：Unity InputSystem 回调 → `EnqueueForNextTick`
    - `NetInputSource`：网络收到 tick 输入 → `EnqueueForTick`
    - `ReplayInputSource`：回放文件/内存记录 → `EnqueueForTick`

**约束**：
- Core 不直接读 Unity Input；只从 `SimInputBuffer.TryDequeueAll` 获取该 tick 的输入。

### Layer D：Lockstep Session（联机会话层）
- 建议目录：`Assets/NTSD/Scripts/Netcode/Lockstep/`
- 放置内容（未来必补）：
  - Session（对局状态机、当前 tick、是否等待输入）
  - FrameInput / PlayerInput（每帧输入包）
  - InputDelay / JitterBuffer（输入延迟窗口）
  - Checksum/Hash（校验）
  - SnapshotStore / Rollback（回滚重演）
  - Resync/Rejoin（断线重连）

### Layer E：Presentation（表现层）
- 建议目录：`Assets/NTSD/Scripts/Presentation/`
- 放置内容：
  - MonoBehaviour、Animator、VFX、UI、相机、音频等

**约束**：
- 表现层只“消费核心状态/事件”，不能反向修改核心权威状态。

---

## 2. Tick Pipeline（每 tick 固定顺序）

建议将每 tick 明确为：

1) `ConsumeInputs(tick)`
- 从 `SimInputBuffer.TryDequeueAll(tick)` 取出该帧输入
- 写入角色/对象的输入状态（例如 keyMask buffer）

2) `Transit`（状态迁移/边界处理）
- 对应：`SimulationWorld.TransitTickAll(tick)`

3) `TU / Sim`（核心逻辑与物理推进）
- 对应：`SimulationWorld.TUTickAll(tick)`

4) `Late`（清理与事件汇总）
- 对应：`SimulationWorld.LateTick(tick)`

5) （可选）`Hash/Record`
- 计算本 tick `WorldStateHash`
- 录制模式保存 `(tick, inputs, hash)`

---

## 3. 联机帧同步“必须补齐”的模块清单（从易到难）

### 3.1 Session（对局会话与 tick 控制）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Session/`
- 职责：
  - 管理对局状态（匹配/加载/战斗/结束）
  - 持有当前 tick、输入延迟窗口、是否允许推进

### 3.2 FrameInput / PlayerInput（每帧输入包）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Input/`
- 职责：
  - 网络传输单位：只传输入，不传世界状态
  - 将输入展开后注入：`SimInputBuffer.EnqueueForTick(tick, key, down)`

### 3.3 输入延迟窗口（InputDelay / JitterBuffer）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Input/`
- 职责：
  - 把网络抖动“吸收”在 buffer 里
  - 常见策略：延迟 N tick 执行（比如 2~6）
  - 若某 tick 输入缺失：选择等待 / 使用空输入 / 触发回滚（见 3.6）

### 3.4 序列化与网络协议（只传输入）
- 目录：`Assets/NTSD/Scripts/Netcode/Transport/`
- 必要消息类型（概念层面）：
  - Join/Match/StartBattle(seed,startTick)
  - InputUpstream(playerId,tick,input)
  - InputBroadcast(tick,allPlayersInputs)
  - Ping/Pong

### 3.5 确定性 RNG（种子下发）
- 目录：`Assets/NTSD/Scripts/Simulation/Core/Determinism/`
- 职责：
  - 全端使用相同 seed 与相同消费顺序
  - 禁止使用 UnityEngine.Random 作为战斗权威

### 3.6 Checksum / WorldStateHash（一致性校验）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Checksum/`
- 职责：
  - 每 tick/每 N tick 计算 hash
  - 客户端上报或服务器广播权威 hash
  - 发现不一致后记录并触发诊断

### 3.7 Snapshot + Rollback（回滚重演，强烈建议）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Rollback/`
- 职责：
  - 保存最近 N tick 快照（回滚窗口）
  - 输入迟到时回滚到 tickX 重演 tickX..current

### 3.8 Resync / Rejoin（断线重连）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Resync/`
- 职责：
  - 服务器下发：最新快照 + 最近若干帧输入
  - 客户端恢复并追帧

---

## 4. 推荐落地里程碑（你可逐条核对）

1) 单机可回放闭环
- 录制每 tick 输入序列 → 重放 → 每 tick hash 一致

2) 本机双端联调（同机跑 server/client）
- 协议跑通 + 输入注入跑通

3) 真联机 + inputDelay（先不回滚）
- 输入必须提前 N tick 到达，否则等待（会卡，但实现简单）

4) 回滚重演
- 解决迟到输入/丢包导致的等待

5) 断线重连 + 完整校验链路

---

## 5. 当前讨论中的“关键约束提醒”（避免踩坑）

- 战斗核心若要严格 lockstep：
  - 不要让 Unity Physics 作为权威
  - 不要让 Unity Time/Animator 驱动数值逻辑
  - 随机必须统一来源与消费顺序
  - 执行顺序必须完全确定（你现在的 SimOrder/StableId 是正确方向）

---

## 6. 你核对时建议重点关注的 6 个问题

1) 你的战斗核心是否还能有地方直接读 Unity Input？（如果有，需要逐步收敛到 SimInputBuffer）
2) 物理/碰撞是否依赖 Unity Physics2D/3D？（若是，lockstep 风险高）
3) `StableId` 的分配规则是否能做到“跨端一致”？（联机时一般由服务器分配并广播）
4) `SimOrder` 是否覆盖所有对象类型，且不会动态变化导致顺序漂移？
5) 随机是否存在多处来源（UnityEngine.Random / System.Random / 自定义）？（需要统一）
6) 状态是否可快照（至少：位置/速度/状态机/关键计数器/输入缓冲窗口/RNG状态）？
