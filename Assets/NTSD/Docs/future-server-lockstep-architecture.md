# NTSD 未来服务器权威帧同步架构备忘

> 状态：未来设计，当前不实施  
> 记录日期：2026-08-08  
> 当前前置目标：先完成 `singleplayer-1000ai-performance-plan.md`
> 上位统一方案：`unified-battle-lockstep-ecs-server-architecture-plan.md`
> 知识来源与取舍：`lockstep-knowledge-base-audit.md`

## 2026-08-10：帧同步资料复核后的当前定调

本节依据 `I:\GitHub\ZhiHu_MD\output\网络游戏` 中的帧同步、逻辑/表现分离、追帧、回滚、快照和网络协议资料，并结合当前代码重新审计后确定。资料中的具体项目经验只作为参考；与 NTSD 的 C# 权威战斗规则冲突时，以权威调用链为准。

2026-08-11 已完成全目录审计：96 个 Markdown、24 份正文，去重并合并同文变体后为 19 个独立主题。本文件只保留未来服务器细节；统一架构决策和明确拒绝项以上位方案与知识审计为准。

### 三种频率必须分离

- **战斗逻辑频率：固定 30 Hz。** 这是 DAT、状态机、碰撞、输入窗口和 C# 权威 pass 的时间语义，不能为了性能或网络打包改成 15 Hz。
- **服务器广播/网络打包频率：独立配置。** 未来可以 30 Hz 逐帧广播，也可以 15 Hz 每包携带两个连续的 30 Hz 权威输入帧；这只改变组包与发送节奏，不合并逻辑帧。
- **Unity 渲染频率：跟随设备 60/90/120 Hz。** 表现读取前后两个已确认逻辑快照并插值，不反写逻辑状态。

逻辑帧长度不是 CPU 预算。单个 30 Hz tick 持续超过 33.33 ms 是容量失败；追帧、降低网络发包频率或提高渲染帧率都不能修复该问题。

### 当前实现的准确状态

当前代码已经具备固定 30 Hz、稳定 pass 顺序、确定性 RNG、`FrameInputSet`、严格输入缓冲、手动逐 tick 入口和无分配 checksum，因此属于“可继续收敛为帧同步核心的运行时”。但下列闭环尚未完成，不能称为完整帧同步架构：

1. `LocalFreeRun` 的人类输入仍由 `CharacterInputModule` 直接写角色 `SimInputBuffer`；本地 provider 返回空 `FrameInputSet`。单机实际输入不能只靠 `FrameInputSet` journal 完整重放。
2. `SimulationTickDriver` 同时持有 Unity wall-clock 累积、模式判断、输入就绪判断、追帧循环和核心执行入口；本地、网络和回放策略尚未成为明确独立的 host policy。
3. 表现最终在 `LateUpdate`/URP 绘制，但 C# 权威 `prePostprocessRender` 对应边界上的 `CaptureEntities + BuildCommands` 仍同步发生在逻辑 tick 内。当前只实现了绘制 API 分离，没有完成“逻辑发布快照、表现独立构建”。
4. 现有 parity/checksum snapshot 用于比较，尚不是可版本化序列化并完整 `RestoreWorld` 的生产恢复快照。
5. 权威运行时仍包含 `double` 战斗字段。是否能跨 Mono、IL2CPP、x64、ARM64 长时间逐帧一致尚无证据；不能仅凭同进程双世界 hash 宣称跨平台确定性。

### 服务器接入前的当前实施顺序

以下批次先服务单机确定性闭环和 1000 AI，不引入真实 transport：

1. **L0 输入事实源闭环**：Unity 输入回调只采集本地意图；每个逻辑 tick 由 `LocalFrameInputCollector` 生成 canonical `FrameInputSet`。单机、回放、未来网络全部只通过 `ApplyFrameInputSet -> StepOneTick` 推进。
2. **L1 host policy 拆分**：`OfflineLocalTickPolicy` 只按本地 wall clock 正常推进；`ManualReplayTickPolicy` 不读 wall clock；`NetworkLockstepTickPolicy` 未来只依据连续权威帧和服务器帧差追帧。三者共享唯一核心 step。
3. **L2 逐帧 journal/checksum 验证**：录制单机完整输入帧，重置相同 seed 后无表现重放；逐 tick 输入 hash、核心 checksum、RNG 状态和最终 checksum 必须一致。
4. **L3 表现发布边界**：保留 C# `prePostprocessRender` 的精确观察时点，但该时点只写入预分配的纯数据快照和确认事件；Sprite/资源解析、排序、command build、Mesh 提交在表现 host 中执行。追帧中间 tick 可不物化表现，但不能漏掉逻辑事件。
5. **L4 恢复快照**：实现版本化 `BattleStateSnapshot` 的 capture/restore 往返，覆盖世界、实体/slot generation、RNG、rest、输入历史、stage、统计和待处理逻辑队列；不包含 Unity 对象。
6. **L5 跨运行时确定性门禁**：同一 seed/journal 分别在 Editor Mono、Windows IL2CPP 和 Android ARM64 重放并逐 tick 比对。若 `double` 域实际分叉，再按字段域迁移到整数/定点数；不得在没有证据时一次性改写全部 C# 权威数值语义。

真实服务器、ACK/冗余包、Jitter Buffer、快照下发和联网回滚从 S0 开始，排在 L0～L5 之后。格斗/ACT 手感是否最终需要预测回滚，要等 `BattleStateSnapshot` 可恢复和真实网络延迟测试完成后决定；当前不把回滚与基础帧同步混成同一批修改。

## 1. 目的与边界

本文记录未来增加服务器后的架构和代码逻辑，避免单机阶段结束后重新调查。它不是当前 1000 AI / 30 FPS 的验收项，也不授权现在引入网络库或服务器依赖。

目标模式为：

> 服务器权威的输入帧同步：服务器维护房间帧号、汇总每帧所有人类玩家输入、运行同一战斗模拟、广播完整权威输入帧，并通过 checksum、历史帧和可恢复快照处理不同步与重连。

常规战斗网络只同步输入，不逐实体同步位置、HP 或 Transform。AI、碰撞、命中、opoint 和生命周期由相同确定性内核在服务器和客户端计算。

## 2. 推荐总体分层

### 2.1 Shared Battle Core

职责：

- 固定 30 Hz 战斗模拟。
- `FrameInputSet -> next BattleState`。
- 确定性 RNG、stable id、runtime slot、generation 和 pass 顺序。
- checksum、快照导出和快照恢复。

约束：

- 不读取 Unity `Time`、Transform、Animator、Physics 或异步资源完成时机作为逻辑真相。
- Unity 表现组件仍留在客户端，不要求把整个 Unity 项目改成纯 C#。
- 服务器复用战斗核心，不复用 Sprite、Mesh、音频和 GameObject 表现层。

### 2.2 Shared Protocol

职责：

- 定义版本化的输入、权威帧、ACK、checksum、快照和重连消息。
- 客户端和服务器共享同一 C# DTO/二进制布局。
- 协议字段使用固定宽度类型并显式规定字节序、版本和最大长度。

### 2.3 Unity Client Host

职责：

- 采集本地输入并量化为完整按键状态。
- 接收权威帧并写入 `NetworkFrameBuffer`。
- 根据服务器进度决定正常推进、等待、追帧或请求恢复。
- 将最终逻辑快照交给中央表现、UI 和音频。

### 2.4 Battle Server Host

职责：

- 管理房间、连接、玩家身份、种子、资源指纹和开始屏障。
- 运行每房间 30 Hz 权威时钟。
- 收集未来帧输入，锁定并广播完整 `FrameInputSet`。
- 运行服务器权威 Battle Core。
- 保存输入历史、checksum 和可恢复快照。
- 处理超时、断线、重连和房间结束。

### 2.5 Transport

控制面与高频数据面保持接口分离：

- 控制面：登录、匹配、房间、握手、开始对局、重连和快照请求。
- 数据面：高频输入、权威帧、ACK 和 checksum。

未来可以让 MagicOnion/StreamingHub 承担控制面；高频帧数据通过 `IFrameTransport` 抽象后再决定使用冗余 UDP、KCP、ENet 或其他方案。当前不提前绑定具体库。

## 3. 未来客户端代码模块

建议职责而非当前强制目录：

| 模块 | 职责 |
|---|---|
| `LocalFrameInputCollector` | 将 Unity 输入采样转换为完整 held/pressed/released 状态 |
| `ClientInputHistory` | 保存未确认输入，支持冗余发送和未来预测重放 |
| `IFrameTransport` | 发送输入、接收权威帧和控制消息 |
| `NetworkFrameBuffer` | 按 frame id 有序保存权威帧、去重并跟踪连续 ready 边界 |
| `NetworkTickPolicy` | 根据服务器帧差、缓冲深度和 CPU 预算决定本渲染帧推进次数 |
| `NetworkBattleSession` | 客户端会话状态机和握手、开始、运行、恢复、结束 |
| `ChecksumReporter` | 周期性上报客户端 lockstep core checksum |
| `SnapshotRecoveryClient` | 加载服务器快照并重放快照后的权威输入帧 |
| `ConfirmedPresentationQueue` | 对确认事件去重，避免追帧/回放重复音效和特效 |

现有 `StrictDelayedInputBuffer`、`BattleLockstepSession` 和 checksum 类型可作为原型材料，但不能直接视为上述生产模块已经完成。

## 4. 未来服务器代码模块

| 模块 | 职责 |
|---|---|
| `BattleRoomRegistry` | 房间创建、查找、销毁和实例分配 |
| `BattleRoomSession` | 一场对局的玩家、状态机、帧号和配置所有权 |
| `ServerFrameClock` | 独立于网络回调的固定 30 Hz 房间时钟 |
| `ServerInputInbox` | 验证、排序并保存玩家未来帧输入 |
| `AuthoritativeFrameAssembler` | 按 canonical player order 生成完整 `FrameInputSet` |
| `ServerBattleSimulation` | 使用共享 Battle Core 推进权威世界 |
| `FrameHistoryRing` | 保存最近若干秒的完整权威输入帧 |
| `SnapshotRing` | 保存版本化、可恢复的服务器状态快照 |
| `DesyncCoordinator` | 比较 checksum、定位分域差异并触发恢复 |
| `ReconnectCoordinator` | 选择快照、后续帧范围并生成重连响应 |
| `RoomCapacityScheduler` | 将不同房间分配到 worker；单房间内部保持确定性串行 |

每个房间内部默认单线程顺序推进。可以并行运行多个房间，但不得在没有确定性证明时并行修改同一个房间的战斗世界。

## 5. 协议数据草案

### 5.1 对局身份

```text
LockstepSessionIdentity
  schemaVersion
  sessionId
  seed
  catalogFingerprint
  stageFingerprint
  playerSetFingerprint
  canonicalPlayerSlots[]
```

所有开始对局的端必须先确认身份和资源指纹一致。

### 5.2 客户端输入命令

```text
ClientInputCommand
  schemaVersion
  sessionId
  playerSlot
  inputSequence
  targetFrame
  heldButtons
  pressedButtons
  releasedButtons
  lastReceivedServerFrame
  lastReceivedServerSequence
```

输入必须是意图和完整按键状态，不能发送客户端计算后的伤害、位置或命中结果。

### 5.3 服务器权威帧包

```text
AuthoritativeFrameBundle
  schemaVersion
  sessionId
  serverSequence
  serverFrame
  firstFrame
  frameCount
  FrameInputSet[frameCount]
  ackedClientInputSequence[]
  optionalChecksumFrame
  optionalChecksum
```

一个包可以冗余携带最近数帧，使单个 UDP 包丢失时下一包仍能补齐。客户端按 `(sessionId, frameId)` 去重，不依赖到包顺序。

### 5.4 Checksum 消息

```text
ChecksumReport
  sessionId
  frame
  schema
  overall
  optionalDomainHashes
```

正常状态只发送 overall；发生差异时再请求 input、RNG、world、slots、aRest、vRest、stats 和 events 分域 hash。

### 5.5 快照恢复消息

```text
SnapshotRecoveryResponse
  sessionId
  snapshotSchema
  snapshotFrame
  serverFrame
  compressedSnapshotBytes
  authoritativeFramesAfterSnapshot[]
  snapshotChecksum
```

当前 `BattleLockstepChecksumSnapshot` 只能比较，不能恢复。未来必须另建稳定的 `BattleStateSnapshot` 和 `RestoreWorld(snapshot)`，不能把 diagnostic JSON 当生产快照格式。

## 6. 服务器每帧逻辑

概念流程：

```text
OnRoomFrameDeadline(frame N)
  1. 从 ServerInputInbox 读取 N 帧各玩家输入
  2. 根据缺失输入规则补齐并锁定 N 帧
  3. 按 canonical player order 生成 FrameInputSet(N)
  4. 运行 ServerBattleSimulation.Step(FrameInputSet(N))
  5. 写入 FrameHistoryRing
  6. 周期性生成 checksum
  7. 周期性生成可恢复快照
  8. 广播包含 N 帧及必要冗余历史的 AuthoritativeFrameBundle
  9. 处理房间超时、断线和结束条件
```

网络回调只验证并入队，不得直接推进战斗世界。这样不会因某个包抵达时间改变 pass 顺序。

## 7. 输入延迟与缺失输入规则

### 7.1 Input Delay

- 客户端提交未来第 `serverFrame + inputDelay` 帧输入。
- `inputDelay` 是房间级确定性配置，不由每个客户端自行决定。
- 初始阶段可以从 2～3 个逻辑帧测试，但最终值必须通过真实网络和手感验证确定。

### 7.2 原型阶段

- 严格等待所有玩家输入 ready 后才推进，用于证明协议、回放和 checksum 一致。
- 该策略会被最慢玩家拖住，只用于第一阶段联调。

### 7.3 生产阶段

- 服务器为每帧设定统一截止时间。
- 截止后使用确定性的缺失输入策略并立即锁定该帧；迟到输入不能修改已经广播的权威帧。
- 建议策略为：短暂 grace 内沿用上一帧 held 状态且 edges 为 0，超过断线阈值后切换为 neutral 或服务器 AI 托管。
- grace、neutral 和托管切换点必须属于服务器规则并进入 checksum/回放记录。

### 7.4 权威帧锁定与重复包

- 首个合法 `(sessionId, frame, playerSlot)` 输入进入未锁定 inbox；
- 相同内容的重复包幂等接受；
- 同 key 不同内容是协议冲突，拒绝并记录，不能以后到覆盖先到；
- frame deadline 后由服务器补齐并锁定完整 `FrameInputSet`；
- 锁定、模拟或广播后，迟到输入不能修改历史；
- 客户端收到重复权威帧时遵循同一规则，同帧不同内容立即进入 `Faulted`。

## 8. 客户端推进与追帧

客户端维护：

```text
localExecutedFrame
highestReceivedServerFrame
highestContiguousReadyFrame
targetBufferFrames
```

缓冲状态固定为：

```text
Priming
  -> Running
  -> WaitingForGap
  -> CatchingUp
  -> RecoveringSnapshot
  -> Running
  -> Faulted / Ending
```

`highestReceivedServerFrame` 可能有洞，只供诊断和补发；实际推进只能使用 `highestContiguousReadyFrame`。`Priming` 先建立目标缓冲，`WaitingForGap` 不跳帧，`CatchingUp` 只处理真实连续积压，`RecoveringSnapshot` 处理超出历史窗口或 checksum 分叉。

正常执行目标：

```text
targetExecutionFrame = highestContiguousReadyFrame - targetBufferFrames
```

推进原则：

- 下一帧未 ready：等待，不能跳过。
- 位于目标缓冲：每个渲染帧正常执行一个 tick。
- 短暂落后：最多执行 2～4 个 tick，同时服从 CPU 时间预算。
- 追帧中间 tick 完整执行逻辑，但只在最终可见 tick 构建正式表现。
- 落后超过历史窗口或预计追赶成本过大：请求服务器快照恢复。
- 单 tick 长期超过 `33.33 ms` 是设备/容量失败，不能靠无限追帧解决。

## 9. 表现、预表现与事件去重

逻辑真相只来自确认的战斗模拟。未来可分三层：

1. `Input Echo`：本地立即显示按键、轻量音效或 UI 反馈。
2. `Intent Presentation`：有限的朝向、起手或移动意图表现，可撤销且不写入逻辑。
3. `Confirmed Result`：伤害、HP、命中、opoint、死亡和正式技能结果只由确认逻辑事件触发。

每个正式表现事件应携带稳定事件键，例如：

```text
(sessionId, frame, sourceStableId, eventSequence, eventType)
```

客户端用它去重，防止追帧、重放、回滚或重复网络包导致音效和特效重复播放。

## 10. Checksum、快照与重连

### 10.1 Checksum

- 每 N 帧比较一次，不要求每帧传输完整状态。
- 服务器 checksum 为权威诊断基准。
- 首次不一致先比较分域 hash，再决定记录、恢复或结束对局。

### 10.2 Snapshot

生产快照至少覆盖：

- 当前 frame、房间规则和 seed。
- runtime slot 的 claimed/generation/stable id 状态。
- 全部逻辑实体和生命周期状态。
- 位置、速度、frame/state、HP/PP、team、owner/link/holder/target。
- RNG state/call count。
- aRest、vRest、stats、stage、输入消费游标和待处理结构事件。

不包含 Sprite、Mesh、材质、Animator、Transform、音频播放状态或 Editor 诊断对象。

### 10.3 Rejoin

```text
1. 客户端重新握手并提交 sessionId 与 lastExecutedFrame
2. 服务器选择最近可用权威快照
3. 下发快照和 snapshotFrame 之后的权威输入帧
4. 客户端 Reset/Restore 世界
5. 无正式表现地快速重放到接近服务器目标帧
6. 重建当前表现快照并恢复正常缓冲运行
```

正常战斗只广播输入帧。服务器快照只用于 bootstrap、重连、严重 desync、观战或晚加入，不周期性覆盖客户端位置、HP、Buff 来维持“表面一致”。客户端本地磁盘快照可作诊断缓存，但不是服务器可信的权威恢复源。

### 10.4 安全与反作弊假设

- 加密只保护传输，不证明客户端诚实；
- 客户端提交输入意图，不能提交命中、伤害或最终位置作为权威结果；
- 服务器同核运行的 checksum 是基准，不能靠两个或多个客户端多数投票决定真相；
- 高频帧协议必须校验 session、player、schema、frame window、序号、包长和输入范围；
- 纯帧同步客户端通常持有完整战斗信息，透视风险不能用 checksum 或加密宣称消除。

## 11. 服务器承载与部署路线

### 当前阶段冻结边界

本节记录未来设计，不代表当前已经开始服务器实施。当前只执行统一方案的 U0～U9：完成单机 BattleKernel、确定性、零 GC、Snapshot/Restore 和 1000 AI 性能验收，同时只保留未来服务器所需的纯 C# 接口边界。

- U9 完成前不实现服务器业务、ACK、Jitter Buffer、房间、登录、匹配或重连；
- U9 完成前不选择或接入具体网络库；
- U9 验收完成后必须等待用户明确确认，不能自动进入 S0；
- S0 获批后只做同进程、内存直连的服务器与多客户端世界，不使用真实 Socket；
- transport 和独立进程选型必须等待同核模拟、权威帧、checksum 与恢复合同获得证据。

### S0：同进程 Loopback

- 在 Unity Editor 内运行内存版服务器帧时钟和多个客户端世界。
- 不接真实 Socket，先证明 StartBarrier、权威帧锁定、输入顺序和 checksum。

### S1：内存网络语义

- 实现 Jitter Buffer、ACK、冗余帧、重复、冲突、乱序、缺帧和 frame deadline。
- 仍使用内存 transport，把协议语义与具体网络库分开证明。

### S2：恢复闭环

- 完成 `FrameHistoryRing + SnapshotRing + ChecksumHistory`。
- 证明断线、严重落后和 desync 均可通过快照 + 后续输入重放恢复。
- 在本阶段末根据真实冲突场景决定是否需要客户端预测回滚。

### S3：独立进程原型

- 先选择最低迁移成本的 C# host。
- 若 Battle Core 仍依赖 Unity 程序集，可先使用 Unity Dedicated Server/headless build 验证房间循环。
- 长期目标是把共享确定性核心收敛到不需要表现层的 C# assembly，让普通 .NET 服务器也能复用。

### S4：真实网络

- 接控制面和数据面 transport。
- 注入延迟、抖动、丢包、重复和乱序。
- 完成 ACK、冗余帧、checksum 和恢复闭环。

### S5：房间扩展

- 单房间保持确定性串行。
- 多房间分配到多个 worker/process。
- 监控每房间 tick Avg/P95/P99、积压、内存、历史和快照成本。

## 12. 未来实施验收顺序

1. 单机录制同一输入重复运行，逐 tick checksum 一致。
2. 同进程服务器 + 两个客户端，连续数千帧 checksum 一致。
3. 输入乱序、重复和丢包后仍按相同帧序列推进。
4. 人为制造客户端落后，限量追帧后恢复目标缓冲。
5. 从服务器快照恢复，重放后 checksum 与未断线世界一致。
6. Windows Mono/IL2CPP、Android IL2CPP 与服务器运行时做跨平台 checksum 验证。
7. 最后才增加本地预测、和解或回滚表现。

## 13. 当前明确暂缓的决策

- MagicOnion 是否用于控制面。
- 高频帧数据最终使用 UDP、KCP、ENet 或其他 transport。
- 生产服务器先用 Unity Dedicated Server 还是直接拆纯 .NET Battle Core。
- 正式 input delay、frame deadline、history 长度和 snapshot 周期。
- 是否实现 GGPO 风格本地预测和回滚。
- 观战、录像下载、跨服迁移和反作弊等级。

这些决策必须在单机稳定、双世界 checksum 和真实网络测量之后确定，不能现在凭经验写死。

## 14. 当前代码到服务器架构的迁移映射

| 当前代码 | 当前可复用部分 | 未来必须拆分或补齐 |
|---|---|---|
| `SimulationTickDriver` | `FrameInputSet -> StepOneTick` 的统一入口、固定 30 Hz、checksum 钩子 | 保留 `OfflineLocal`；新增独立的 `NetworkTickPolicy`，只有网络模式才能依据服务器帧差追帧。驱动本身不得持有 Socket 或房间规则 |
| `FrameInputSet` / `SimulationPlayerInput` | 按 tick、canonical player order 表示完整 held/pressed/released 输入 | 增加协议 schema、固定宽度序列化和最大玩家数校验；网络 DTO 不直接序列化 Unity 对象 |
| `StrictDelayedInputBuffer` | 固定容量、去重、冲突重复检测、连续帧消费 | 当前更接近严格原型缓冲；生产版需增加 server sequence、ACK、乱序窗口、帧包冗余和权威帧连续边界 |
| `BattleLockstepSession` | 显式提交输入、严格按下一 tick 推进、回放 journal | 当前是同进程原型，不代表服务器会话完成；未来拆成 `NetworkBattleSession` 与服务端 `BattleRoomSession`，不能让客户端本地提交直接冒充权威帧 |
| `LockstepReplayJournal` | 有序保存已消费 `FrameInputSet` | 改为可配置历史环，并与 checksum、快照帧号和重连窗口统一生命周期 |
| `BattleLockstepChecksumSnapshot` | 分域 checksum 与差异定位 | 只用于比较，不能恢复世界；未来增加版本化 `BattleStateSnapshot` 与 `RestoreWorld` |
| `BattleParitySnapshot` | 诊断 C#/Unity 行为和 hash 域 | 保持 Editor/诊断用途，不作为生产网络载荷或生产快照格式 |
| 中央表现系统 | 读取已完成逻辑帧并绘制 | 增加确认事件去重与无表现追帧入口；不得让 Mesh、Sprite、音频状态进入服务器或 checksum |

迁移时不创建第二套战斗主循环。单机、回放、客户端和服务器最终都调用同一个 `Step(FrameInputSet)`；区别只在“谁生成并授权输入帧”以及“本渲染帧允许推进多少个已就绪逻辑帧”。

## 15. 未来客户端与服务器状态机细节

### 15.1 客户端会话状态

```text
Disconnected
  -> Handshaking
  -> AwaitingStartBarrier
  -> BufferingAuthoritativeFrames
  -> Running
  -> RecoveringSnapshot
  -> Running
  -> Ending
```

- `Handshaking`：确认协议版本、catalog/stage/player fingerprints 和 seed。
- `AwaitingStartBarrier`：资源已准备，但不提前推进战斗；等待服务器开始帧。
- `BufferingAuthoritativeFrames`：积累目标缓冲深度，避免刚收到第一帧就进入反复等待。
- `Running`：只消费连续 ready 的权威 `FrameInputSet`；正常每个可见帧一个 tick。
- `RecoveringSnapshot`：停止正式表现，恢复快照并重放后续权威帧；完成后一次性重建当前表现。
- 任意协议冲突、checksum 恢复失败或资源指纹不一致都进入明确错误状态，不允许静默继续。

### 15.2 服务端房间状态

```text
Created
  -> WaitingForPlayers
  -> StartBarrier
  -> Running
  -> Finishing
  -> Archived
```

- `StartBarrier` 固定 session identity、canonical player slots、seed、input delay 和开始帧；开始后不能被单个客户端修改。
- `Running` 中网络线程只入队；房间 worker 在帧截止点串行锁定输入、推进核心、写历史/checksum/快照并广播。
- 玩家断线不暂停整个房间。缺失输入按服务器确定性规则处理，并把托管或 neutral 切换写入权威历史。
- `Archived` 只保留需要的对局元数据、输入日志和诊断结果；不保留 Unity 表现对象。

### 15.3 服务器接入前必须保持的单机约束

1. `OfflineLocal` 永远不读取“服务器目标帧”或执行网络追帧。
2. 每个逻辑 tick 的输入、RNG、实体顺序和结构变化都可记录并重放。
3. 逻辑世界能在不构建表现的情况下推进，且 checksum 与有表现运行一致。
4. `stable id`、runtime slot generation 和 canonical player order 不依赖 GameObject 创建顺序。
5. 高频逻辑不依赖 `Time.deltaTime`、Transform、Unity Physics、异步资源回调或 Editor API。
6. 当前 1000 AI 优化不得用降频、跳帧或删规则换性能，否则服务器与客户端将无法共享同一核心。

## 16. 从当前单机到未来服务器的实际代码流

本节固定“谁拥有帧、谁推进世界、谁只做表现”的调用关系。未来接服务器时只替换帧输入来源和外层推进策略，不新建第二套战斗 pass。

### 16.1 当前单机流

```text
Unity Update
  -> OfflineLocalTickPolicy 根据本地累计时间判断本帧是否执行 1 个 tick
  -> LocalFrameInputCollector 生成 FrameInputSet(N)
  -> SimulationTickDriver.StepOneTick(FrameInputSet(N), buildPresentation: true)
  -> SimulationWorld.ApplyFrameInputSet
  -> NTSDBattleTickSystem.RunReleaseTick
  -> 可选 checksum / replay journal
  -> 发布逻辑快照和确认表现事件
  -> Unity LateUpdate / URP 只读取已发布结果
```

当前代码仍使用 `SimulationDriveMode.LocalFreeRun` 名称；文档中的 `OfflineLocal` 是未来更明确的职责名。是否重命名应在不影响当前性能主线的独立改动中完成，不能为改名扩大本轮 diff。

### 16.2 未来客户端正常运行流

```text
Unity Update
  -> LocalFrameInputCollector 采样本地意图并写入 ClientInputHistory
  -> IFrameTransport 发送目标 future frame 的 ClientInputCommand
  -> Transport callback 只把 AuthoritativeFrameBundle 入队
  -> NetworkFrameBuffer 在 Unity 主线程按 frame id 去重并推进连续 ready 边界
  -> NetworkTickPolicy 计算本可见帧允许执行的权威 tick 数
  -> 对每个 ready FrameInputSet(N)：
       SimulationTickDriver.StepOneTick(FrameInputSet(N), buildPresentation)
  -> 中间追帧 tick 可不构建正式表现；最后可见 tick 发布完整表现快照
  -> ConfirmedPresentationQueue 按稳定事件键去重并播放
```

`SimulationTickDriver` 不判断 RTT、不读取 Socket，也不自行猜测服务器帧。它只校验传入 frame 是否正好是 `current + 1`，然后执行共享核心。帧是否权威、是否连续、是否应该追赶由 `NetworkBattleSession + NetworkTickPolicy` 在外层决定。

### 16.3 未来服务器房间流

```text
Network receive thread
  -> 校验包尺寸、schema、session/player 身份和序号
  -> ServerInputInbox.Enqueue，不推进世界

Room worker 在 frame deadline(N)
  -> 从 inbox 取得 N 帧所有玩家输入
  -> 按房间规则确定性补齐缺失输入
  -> AuthoritativeFrameAssembler 按 canonical player order 生成 FrameInputSet(N)
  -> ServerBattleSimulation.StepOneTick(FrameInputSet(N), buildPresentation: false)
  -> 写入 FrameHistoryRing
  -> 到周期时生成 checksum 和 BattleStateSnapshot
  -> 生成带 ACK 与冗余历史帧的 AuthoritativeFrameBundle
  -> 广播；网络发送完成时机不影响房间下一 tick 的战斗规则
```

服务器可并行运行多个房间，但单个房间内的输入锁定、模拟、历史、checksum 和快照写入保持串行。不得让网络回调、异步资源回调或多个 worker 同时修改同一个 `SimulationWorld`。

### 16.4 未来客户端恢复流

```text
发现 checksum 分叉或落后超出历史窗口
  -> NetworkBattleSession 进入 RecoveringSnapshot
  -> 暂停正式表现事件消费
  -> 校验并 RestoreWorld(BattleStateSnapshot at frame S)
  -> 依次重放 S+1 ... highestContiguousReadyFrame 的权威 FrameInputSet
  -> 重放期间 buildPresentation=false，但逻辑事件仍进入可重建历史
  -> 核对恢复后 checksum
  -> 清理/重建当前表现快照和事件去重游标
  -> 回到 Buffering 或 Running
```

恢复失败必须显式结束或重新请求快照，不能继续在已知分叉的本地世界上运行。

## 17. 数据所有权与线程边界

| 数据 | 唯一写入方 | 读取方 | 禁止事项 |
|---|---|---|---|
| 本地原始按键 | Unity 客户端输入采集器 | `LocalFrameInputCollector` | 服务器和 Battle Core 直接读取 Unity Input API |
| `ClientInputCommand` | 对应客户端 | transport、服务器 inbox | 客户端写入位置、伤害或命中结果 |
| 权威 `FrameInputSet` | 服务器 `AuthoritativeFrameAssembler` | 服务器核心、全部客户端核心、回放 | 客户端本地帧覆盖同 frame 的权威内容 |
| `SimulationWorld` | 当前 tick 所属的单线程 Battle Core | checksum、快照导出、表现快照构建 | 网络线程、URP、MonoBehaviour 回调并发写逻辑字段 |
| `BattleStateSnapshot` | 服务端房间 worker | 恢复客户端、诊断工具 | 包含 Sprite、GameObject、Transform、Material 或 Editor 引用 |
| 表现快照 | Unity 客户端中央表现构建器 | URP、UI、音频 | 回写 Battle Core，或作为 checksum 真相 |
| 表现事件去重游标 | `ConfirmedPresentationQueue` | 音频、粒子、UI | 进入服务器模拟或改变战斗结果 |

跨线程队列只传输不可变消息或完成所有权转移的数据块。入队后生产者不得继续修改；消费后由明确的池/生命周期回收，避免每帧临时分配与隐式共享引用。

## 18. 网络追帧与单机时钟的硬隔离

- `LocalFreeRun/OfflineLocal`：正常每个 Unity 可见帧最多执行一个逻辑 tick；没有服务器帧差，不存在网络追帧。
- `ManualReplay`：由测试或恢复代码明确逐 tick 推进，不读取 wall clock。
- `NetworkLockstep`：只有连续权威帧已 ready 且本地落后目标缓冲时，才允许限量追帧。
- 追帧上限同时受“连续 ready 帧数量”和“本帧 CPU 时间预算”约束；预算不足时宁可继续落后或请求快照，不允许无限 while。
- 追帧不能改变单 tick 的 `SIM_DT`、输入内容、RNG 消费或 pass 顺序。
- 中间追帧 tick 可以省略 Mesh/音频等正式表现构建，但不能省略会进入 checksum 的逻辑事件；最后可见 tick 必须重建完整表现。
- 单 tick 长期超过 `33.33 ms` 是战斗核心容量失败，不能靠追帧策略掩盖。这也是当前必须先完成 1000 AI 单机性能目标的原因。

## 19. 未来服务器实施前的代码门禁

开始 S0 同进程 Loopback 前必须满足：

1. 单机、无表现手动推进和回放三条入口对同一输入 journal 产生相同分域 checksum。
2. `SimulationTickDriver` 的核心入口能够由外部完整 `FrameInputSet` 驱动，且不要求 Unity wall clock。
3. 生产 `BattleStateSnapshot` 的字段清单、schema/version 和 `RestoreWorld` 具备往返测试。
4. 正式表现事件具有稳定事件键；无表现重放后不会丢事件或重复播放。
5. 单机模式与网络模式的调度配置不能通过同一默认值互相污染。
6. 1000 AI 单 tick 有明确 CPU/GC 预算；服务器不会因禁用渲染就掩盖核心超预算。

达到这些门禁、完成 U9 验收并取得用户明确批准后，才实现 S0；S0 通过后才评估 transport 和部署形态。这样当前优化结果可以直接成为服务器共享核心，而不是未来再做一次战斗架构迁移。
