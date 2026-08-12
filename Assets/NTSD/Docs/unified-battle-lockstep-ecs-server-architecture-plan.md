# NTSD 统一战斗内核、帧同步、自研 ECS 与未来服务器架构方案

> 建立日期：2026-08-11
> 当前状态：U0～U4 已完成；U5 正在执行。`CharacterHitConsume` 与 `ObjectHitConsume` 的可证明空候选路径均已晋升默认；ObjectHit 两组 1000 AI A/B 的目标 pass average/P95 合并改善 27.80%/22.20%，但整 tick 没有稳定改善，因此只声明局部收益，证据见 `Docs/unified-battle-u5-empty-character-hit-consume-20260811.md` 与 `Docs/unified-battle-u5-empty-object-hit-consume-20260811.md`。Stage 场景数据已收敛为每 tick 一次宿主快照；`PreInteraction` whole-pass no-op 与逐 participant 精确过滤均已晋升；Late tail no-op、正向 link 数据化候选与 character runtime candidate-count gate 都因目标 pass 负优化而保持 Legacy。真实 hit writer 的权威原子边界已经闭合，默认关闭的固定容量计划影子已通过正式旧链候选读取、preprocess、全部 kind disposition、预消费副作用、dispatch、OID300 abort，以及 kind `6/8/14/1/3/2/7/10/11/15/16` 的精确状态副作用。damage `0/9` 已闭合标准角色轻/中/重/倒地、标准致死统计与强制倒地、alternate 非致死/致死、标准武器类型 `1/2/4/6` 的 effect0/effect4 分支，以及 type3 基础尾链、state3005/3006 同步、D1 直接/活动身份替换与可达 effect 矩阵；oracle 已实际检出并修复多处双帧镜像遗漏。OID `0xD6` 字段投影已写入且独立 C# 编译通过，但定向 Unity 测试被 Editor 的 `MDB_READERS_FULL` 原生崩溃阻断；OID `0xC9` 自释放、剩余特殊 effect、character-DAT type3 effect tail、声音事件全集、opoint 与结构生命周期仍未关闭，正式 canonical writer 尚未切换
> 战斗逻辑唯一权威：`J:\QQFile\NTSD2.4\ntsd_release_C#`
> 当前实现目标：先完成单机确定性闭环与 1000 AI / 30 FPS，再接入服务器
> 排除项：T8 默认 `stage.dat` 部署、Android 真机验收

## 1. 文档定位与替代关系

本文是以下工作的统一总方案：

- 固定 30 Hz 的单机与未来网络帧同步；
- NTSD 专用的自研数据导向 ECS 战斗内核；
- 1000 个真实生产 AI 的 CPU、GC、碰撞和表现性能治理；
- 中央战斗表现与逻辑真值分离；
- 未来无表现服务器复用同一战斗内核；
- 可重放、可校验、可快照恢复的确定性运行时。

`Assets/NTSD/NTSD_Lockstep_Framework_Plan.md` 是旧计划，不再作为实施依据。现有文档继续承担专项记录职责：

| 文档 | 保留职责 |
|---|---|
| `Docs/singleplayer-1000ai-performance-plan.md` | 性能基线、Profiler 归因和 1000 AI 验收证据 |
| `Docs/battle-runtime-zero-gc-architecture-plan.md` | 零 GC、池、容器、static 与 partial 治理证据 |
| `Docs/central-battle-render-system-plan.md` | 中央表现、Texture2DArray、Mesh、排序与 SetPass 证据 |
| `Docs/future-server-lockstep-architecture.md` | 服务器协议、房间、追帧、恢复与部署细节 |
| `Docs/lockstep-knowledge-base-audit.md` | `网络游戏` 知识库来源覆盖、取舍和拒绝项 |
| `.omc/plans/battle-kernel-ecs-lockstep-migration-20260727.md` | 既有 ECS shadow 实验、历史基线和未关闭合同 |
| `Docs/csharp-vs-unity-battle-alignment.md` | C# 权威到 Unity 的战斗差异与验收证据 |
| `Docs/HANDOFF-codex-battle-alignment.md` | 当前实现进度与后续交接 |

发生冲突时按以下优先级处理：

1. 用户当前明确要求；
2. 权威 C# 的实际调用链；
3. 本文确定的架构边界和实施顺序；
4. 各专项文档的测量证据；
5. 历史计划和旧结论。

### 1.1 ECS 知识库取舍依据

本方案复核了以下本地知识库：

| 资料 | 采用内容 | 不直接照搬的内容 |
|---|---|---|
| `I:\GitHub\ZhiHu_MD\output\Unity引擎\ECS、物理与性能优化` | 连续 struct 数据、逻辑/表现延后一帧解耦、控制并行线程数、Transform 每渲染帧统一同步 | Unity Entities、Chunk、Burst 和 Job 不是本项目依赖前提 |
| `I:\GitHub\ZhiHu_MD\output\游戏架构\ECS与数据导向设计` | Entity/Component/System 分离、generation 身份、bitset 签名、World/Coordinator 所有权 | 热路径不使用 `unordered_map`/`set`/虚调用式通用组件管理 |
| `I:\GitHub\ZhiHu_MD\output\游戏开发中的 ECS 框架_323304` | Direct Array、SoA、Sparse Set、Bitset、Command Buffer、Pipeline 和结构变化成本分析 | 不把 Archetype/Chunk 当作所有项目的唯一正确方案，不因 ECS 名称默认获得性能 |

综合结论是：ECS 的实际收益来自数据布局和访问路径，不来自类型命名。NTSD 的槽位顺序和结构可见时点具有规则语义，因此采用专用混合存储，并用权威 pass boundary 约束命令播放。

### 1.2 帧同步知识库审计依据

本方案已审计 `I:\GitHub\ZhiHu_MD\output\网络游戏` 全部子目录：96 个 Markdown、24 份正文；正文去除 3 组字节级重复并合并 2 组同文变体后，共 19 个独立主题。完整来源、重复关系、采用项、适配项和拒绝项见 `Docs/lockstep-knowledge-base-audit.md`。

审计后确定的总原则是：

1. NTSD 正常战斗只同步并授权输入，结果由同一 BattleKernel 计算；
2. 服务器运行相同 C# 内核，不另写伤害、命中或生命周期规则；
3. 状态同步只用于 bootstrap、快照恢复、晚加入、观战和诊断，不在正常 tick 覆盖 HP/位置；
4. 逻辑 30 Hz、网络组包频率和 Unity 渲染频率相互独立；
5. 权威帧一旦锁定不可修改，重复包只允许幂等接受；
6. Jitter Buffer、缺帧、追帧和恢复必须由显式状态机管理；
7. 预表现只能表达输入与意图，扣血、命中、opoint、控制和死亡只来自确认逻辑；
8. ECS 解决数据布局与批量处理，不自动解决确定性、错误复杂度、网络抖动或持续超预算。

知识库中的示例帧率、缓冲长度、transport、定点库和性能阈值均不是 NTSD 生产常量。任何与权威 C# 行为冲突的网络文章结论只记录为拒绝项，不进入实现。

当前工作树已经存在尚未统一验收的输入、host policy、AI、碰撞、零 GC 和表现优化。它们全部视为“候选实现”，进入第一个基线审计阶段；本文不假定这些修改已经完成，也不授权覆盖或回退用户改动。

## 2. 最终目标

最终只保留一套战斗规则实现：

```text
单机本地输入 ─┐
回放输入 ─────┼─> Canonical FrameInputSet
服务器权威帧 ─┘             │
                             v
                   Shared Battle Kernel
                   固定 30 Hz / 纯 C#
                   自研 ECS / 确定性 RNG
                             │
                 ┌───────────┼───────────┐
                 v           v           v
              Checksum   状态快照    表现观察/事件
                                         │
                              ┌──────────┴─────────┐
                              v                    v
                         Unity Client         Headless Server
                         渲染/音频/UI          房间/协议/恢复
```

必须达到的结果：

1. 单机、回放、客户端和服务器都调用同一个 `StepOneTick(FrameInputSet)` 战斗入口。
2. C# 权威的规则、pass 顺序、slot 顺序、RNG、opoint 可见边界和可观察结果保持不变。
3. 战斗真值由纯 C# 数据世界持有，不依赖 Unity Transform、Physics、Renderer、Time 或异步资源完成顺序。
4. 高频战斗循环使用连续数据、预分配容器和显式顺序，不在正式战斗窗口产生托管分配。
5. Unity 只负责输入采样、资源、GameObject 壳、中央表现、音频和编辑器接线。
6. 服务器可以在普通 .NET 或后续选定的 headless host 中复用战斗内核，不依赖表现程序集。
7. 1000 个真实生产 AI 的 `Dispersed1000` 与 `Combat1000` 达到 30 Hz / 30 FPS 正式门禁。
8. 逻辑 30 Hz 与表现 60/90/120 Hz 解耦；提高显示帧率不改变战斗规则。

## 3. 明确非目标

本文不要求：

- 使用 Unity Entities、DOTS 或 Burst；
- 把整个 Unity 项目改成 ECS；
- 当前立即实现真实网络、匹配、登录、NAT、反作弊或跨服；
- 当前立即实现客户端预测、GGPO 风格回滚或观战；
- 为性能降低 AI 频率、跳过有效碰撞、限制真实命中或修改 DAT 数值；
- 当前处理 T8 默认 `stage.dat`；
- 当前完成 Android 真机验收；
- 在没有跨运行时分叉证据时一次性把全部 `double` 改成定点数。

ECS 只进入战斗运行时和与战斗时序直接相关的表现发布边界。菜单、角色选择、普通 HUD 和通用资源工具继续使用适合 Unity 的现有结构。

## 4. 核心架构决策

### 4.1 采用 NTSD 专用混合 ECS，不采用通用 Archetype ECS

主存储采用：

```text
固定/分页 Slot 域
  + 直接索引 SoA
  + Presence/Tag Bitset
  + 少量 Sparse Set
  + 预分配 Ring/Queue/Pool
  + 确定性空间索引
```

不以 Archetype/Chunk 迁移作为主模型，原因是：

1. 权威 C# 的 runtime slot 升序遍历和复用时机属于战斗规则。
2. NTSD 的 state、frame、oid、link、holder、target 和对象生命周期变化频繁，但这些变化不应被解释为频繁添加/删除组件。
3. Archetype 结构变化会搬移实体，增加快照、排序和同 tick 可见性证明的复杂度。
4. 当前目标规模为 400 权威槽和 1000 扩展槽，直接索引数组的内存成本可控，访问路径更短。
5. 未来服务器需要纯 C# 可移植性，首期不应把共享核心绑定到 Unity Collections 或 Entities。

该方案仍然属于 ECS：Entity 是身份，Component Store 保存纯数据，System 批量处理数据；只是它是针对固定战斗域优化的 ECS，不是通用引擎级 ECS。

### 4.2 新内核使用组合，不新增 partial 和全局可变 static

新内核遵循：

```text
BattleKernel
  -> BattleWorld
  -> Entity/Data Stores
  -> BattlePipeline
  -> World-scoped Systems
  -> Command/Event Buffers
  -> Snapshot/Checksum Services
```

- `BattleKernel`/主类持有普通 module/system 实例引用。
- 新代码不新增 `partial` 类型或 `.partial.cs` 文件。
- 不使用全局可变 singleton 保存当前 World、RNG、当前帧、相机、表现 generation 或测试 override。
- 允许保留编译期常量、只读表、ProfilerMarker 和无状态纯函数 `static`。
- System 可以拥有预分配 scratch 和只属于当前世界的缓存；影响战斗结果的状态必须进入 BattleWorld 或可快照 World Resource。
- 固定 pipeline 直接调用具体 system，不在热路径使用反射、`Dictionary<Type, object>`、虚拟查询调度或每帧生成委托。

### 4.3 逻辑单线程确定性优先，表现和多房间并行

第一阶段每个 BattleWorld 内部保持单线程顺序执行。这样最容易证明：

- slot 顺序；
- RNG 调用次数；
- candidate 和 hit 消费顺序；
- opoint 同 pass 可见性；
- snapshot/checksum 一致性。

并行化顺序：

1. 先消除 GC、对象图遍历、重复快照和错误复杂度；
2. 再把 Unity 表现与 BattleKernel 分线程/分帧；
3. 最后只并行只读收集或写入互不重叠输出区间的 kernel；
4. 所有并行结果按稳定 slot/pair ordinal 确定性合并；
5. 服务器优先并行不同房间，不在同一房间内无证据并行写世界。

## 5. Entity、容量与身份

### 5.1 Entity 身份

逻辑 Entity 使用：

```text
EntityHandle
  slot        // 当前 world 内的直接数组索引
  generation  // slot 每次复用递增，拒绝过期引用
```

以下身份不得混用：

- `slot`：当前运行时位置和权威扫描顺序；
- `generation`：slot 复用安全；
- `stableId`：跨 tick 的逻辑事件和诊断身份；
- `oid`：DAT 对象类型；
- presentation handle：Unity 表现对象身份，只存在于客户端。

holder、target、owner、parent、attacker 等跨实体引用使用 generation-aware handle，不能只保存 slot 后长期信任。

### 5.2 容量 Profile

至少保留两个 profile：

| Profile | 用途 | 容量规则 |
|---|---|---|
| `Authority400` | 与权威 C# 逐 tick 对照 | 保持权威槽域、起始搜索和复用语义 |
| `Extended1000` | 移动端和 1000 AI 正式压力 | 战前封印至少 1000 active 的全部相关容量 |

桌面端不使用编译期“最多 1000”硬限制，而使用分页容量：

- 战斗准备阶段根据本局 profile 预留页；
- 房间开始后容量配置进入 session fingerprint；
- 正式 tick 内禁止托管数组自动扩容；
- 超过已封印容量时产生确定性的 capacity fault 和结构化计数，不允许静默 `new`、漏生成或不同端产生不同结果；
- 后续对局可以选择更大 profile，不需要重新编译。

“无固定编译上限”不等于无限内存。每一局仍必须有确定、可预热、可同步的容量合同。

### 5.3 Slot 分配

不能使用覆盖所有槽位的单一全局最小堆。allocator 必须知道：

- 权威槽域；
- 本次搜索起点；
- 当前 pass 游标；
- 延迟释放与可见边界；
- generation 更新时点。

底层可以组合使用分段最小堆、分层位图和分页 free list，但返回结果必须与权威 C# 的最低合法槽选择一致。

## 6. 数据存储方案

### 6.1 直接 SoA：高频、广泛存在的数据

以下领域使用按 slot 直接索引的连续数组或分页数组：

- Identity：active、generation、stableId、oid、kind、team、owner；
- Motion：X/Y/Z、Vx/Vy/Vz、facing；
- Frame：frameId、state、wait、next、prevFrame；
- Vital/Stats：HP、PP、MP、fall、defend、kill/combo/damage stats；
- Input：held、pressed、released、buffer/history、AI output；
- Links：holder、target、catching、parent、attacker；
- Lifecycle：pending spawn/free/unregister、first visible tick、dormant；
- 高频 collision/runtime flags。

优先使用普通预分配 `T[]`、分页数组和 `Span<T>`，保持纯 .NET 可移植。是否引入 unmanaged backend 必须由 Player/服务器 profile 证明，不在首期预设。

### 6.2 Bitset：存在性、标签和升序查询

Bitset 用于：

- alive/active/pending/dormant；
- character/weapon/projectile/effect；
- has body/has itr/has AI/has holder；
- dirty/presentation visible；
- pass membership。

权威顺序敏感的 pass 必须按 slot 升序扫描 bitset；不能因为 sparse set 的 swap-remove 改变执行顺序。

### 6.3 Sparse Set：真正可选且中低密度的数据

Sparse Set 只用于：

- 少量实体才拥有的数据；
- add/remove 不频繁；
- 或执行顺序与密集数组内部顺序无关的数据。

若结果影响战斗顺序，必须按 slot/ordinal 稳定化后消费，不能直接依赖 dense 数组当前排列。

### 6.4 固定池、环形缓冲和索引链

| 数据模式 | 结构 |
|---|---|
| 输入历史、表现事件、声音事件、回放帧 | 固定环形缓冲 |
| opoint、spawn/free、pass-boundary 命令 | 分段命令缓冲 |
| hit candidate、pair、排序 scratch | 每世界预分配数组/List，容量封印 |
| 高频 O(1) 插入删除并要求稳定节点 | 预分配节点数组 + 整数 `prev/next` |
| 最低合法 free slot | 分段最小堆/分层位图 |
| 冷路径 key 查找 | 战前定容 Dictionary 或开放寻址表 |

普通 `LinkedList<T>` 不作为默认方案，因为节点是分散的引用对象，遍历不连续并可能产生 GC。只有预分配索引链表才进入战斗热路径。

## 7. System 与权威 Pipeline

System 不是任意调度。`BattlePipeline` 必须固定映射权威 `GameTick.Run` 的顺序：

```text
Tick/瞬时状态开始
  -> Results 分支
  -> Cooldown 与输入边界
  -> OID 51/52 等早期维护
  -> BattleEntry clear gate
  -> CharacterInput
  -> EarlyState
  -> FrameLogic
  -> FrameAdvance
  -> PostFrameAdvance / Stage Z Bounds
  -> CPoint / Held / Link
  -> PrevFrame2 Snapshot
  -> CandidateCollect
  -> CharacterHit
  -> Random/F8 Weapon Drop
  -> ObjectHit
  -> PreFrame Bounds / Stage Advance
  -> Presentation Observation Boundary
  -> FramePostProcess
  -> Late Per Entity Update
  -> Mode2 Weapon Drop
  -> Entity PostFrame Tail
  -> Battle Results Update
```

原则：

- 不为了“ECS 纯度”把每个小函数拆成一个 System；一个 system 可以包含多个紧密相关的连续循环。
- 不为减少循环随意合并在权威 C# 中分开的 pass。
- 只有 profile 证明数据被重复读取且合并不改变观察边界时，才允许合并内部数据准备。
- 每个 system 的 canonical writer、读取集合、scratch 和命令输出必须明确。

## 8. 结构变化与 opoint 可见边界

通用 ECS 的“所有创建/销毁统一到 tick 末”不适用于 NTSD。

结构命令需要携带权威播放边界：

```text
StructuralCommand
  type
  source handle
  target/oid/data
  requested slot domain
  playback boundary
  authority ordinal
```

至少区分：

- 当前实体结束后立即可见；
- 当前 pass 分段结束可见；
- 下一 pass 可见；
- tick 结束后可见；
- 延迟 unregister/free。

当权威 late live-slot 循环要求当前实体产生的高槽 opoint 在同一 pass 后续参与时，必须使用 cursor-local immediate playback；不能强行等到全 tick 结束。

spawn、destroy、free、generation、link invalidation 和对象池 release 的顺序全部进入 checksum 和 self-check 合同。

## 9. 帧同步与宿主策略

### 9.1 唯一逻辑入口

目标接口：

```text
ResetWorld(BattleBootstrap)
StepOneTick(FrameInputSet)
CaptureStateSnapshot()
RestoreStateSnapshot()
ComputeChecksum()
Export/ReplayInputJournal()
```

`BattleBootstrap` 至少包含：

- seed；
- capacity/profile；
- immutable DAT catalog；
- stage runtime snapshot；
- player canonical order；
- catalog/stage/config fingerprint。

BattleKernel 不读取 Unity wall clock、Input API、Transform 或网络回调。

### 9.2 三种本地推进策略

| 模式 | 帧来源 | 推进规则 |
|---|---|---|
| `OfflineLocal` | 本地 canonical collector | wall clock 只决定本可见帧是否执行 0/1 tick；不执行网络追帧 |
| `ManualReplay` | journal/测试/恢复调用方 | 不读取 wall clock，调用方显式逐 tick 推进 |
| `NetworkLockstep` | 连续 ready 的服务器权威帧 | 只有落后服务器目标缓冲时，才按 ready 数量和 CPU 预算有限追帧 |

单机普通 `Update` 不再因为本地累计时间一次执行四个完整 tick。若未来需要单机卡顿恢复，必须建立独立 `LocalHitchRecovery` 策略，不能借用网络帧差语义。

### 9.3 三种频率分离

- 战斗逻辑：固定 30 Hz；
- 网络广播/组包：独立配置，可以 15 Hz 每包携带两个 30 Hz 输入帧；
- Unity 渲染：60/90/120 Hz，读取逻辑快照插值，不写回逻辑。

降低网络发包频率不能合并逻辑帧；提高渲染帧率不能多执行战斗规则。

### 9.4 三种时间与过载定义

必须严格区分：

| 时间 | 含义 | 能否改变战斗规则 |
|---|---|---|
| Logic Time | `tick * SIM_DT` 的离散规则时间 | 只能按固定 30 Hz 逐 tick 前进 |
| Wall Clock | Unity、OS 或服务器现实流逝时间 | 只供 host policy 判断调度和积压 |
| Compute Time | `StepOneTick` 实际 CPU 耗时 | 只用于预算、告警和容量判定 |

某 tick 计算超过 `33.33 ms` 不代表该 tick 的逻辑时间变长，而是本机没有跟上目标逻辑时钟。偶发慢帧可以进入 backlog 并有限恢复；持续慢帧是算法、数据、Unity API、GC 或内容容量失败，不能通过动态 dt、无限追帧或删减规则处理。

### 9.5 网络客户端的四个帧游标

`NetworkLockstep` 至少维护：

```text
latestLocalSampleFrame
highestReceivedServerFrame
highestContiguousReadyFrame
localExecutedFrame
```

- `latestLocalSampleFrame`：已采样并发送的本地未来输入目标帧；
- `highestReceivedServerFrame`：收到过的最高服务器帧，允许中间有洞；
- `highestContiguousReadyFrame`：从 `localExecutedFrame + 1` 开始连续 ready 的最高帧；
- `localExecutedFrame`：BattleKernel 已完成的最后一帧。

追帧只能依据 `highestContiguousReadyFrame`，不能依据收到过的最高帧跳过中间缺口。服务器广播序号、客户端输入序号和战斗 frame id 是不同维度，也不得混用。

### 9.6 Jitter Buffer 状态机

权威帧缓冲不是一个普通 Queue，而是以下会话状态的一部分：

```text
Priming
  -> Running
  -> WaitingForGap
  -> CatchingUp
  -> RecoveringSnapshot
  -> Running
  -> Faulted / Ending
```

- `Priming`：积累目标缓冲深度，不能刚收到第一帧就反复执行和等待；
- `Running`：维持目标缓冲，通常每个可见帧执行 0/1 tick；
- `WaitingForGap`：下一连续帧缺失，等待或补发，禁止跳过；
- `CatchingUp`：连续权威帧已经 ready 且本地落后目标缓冲，有限追帧；
- `RecoveringSnapshot`：落后超出历史窗口、预计追赶代价过高或 checksum 分叉，恢复权威快照并重放；
- `Faulted`：协议冲突、资源指纹不一致或恢复失败，显式停止，不在已知错误世界继续运行。

目标缓冲帧数允许根据会话握手时的网络配置选择，但运行中不得高频抖动调整。所有切换只影响“何时执行已有权威帧”，不改变帧内容、`SIM_DT`、RNG 和 pass 顺序。

### 9.7 有限追帧与容量失败出口

网络追帧同时受三个上限约束：

1. 本地实际落后的连续 ready 帧数；
2. 每个可见帧最多可执行的 catch-up tick 数；
3. 本次主循环允许消费的 CPU 时间预算。

中间追帧 tick 完整执行战斗规则，但可不构建 Sprite、Mesh、UI 和音频表现；最后可见 tick 发布完整表现。若 backlog 连续增长、长时间处于 catch-up 或超出 `FrameHistoryRing` 覆盖范围，必须进入快照恢复或容量失败处理，不允许进入死亡螺旋。

## 10. 输入事实源

Unity 输入回调只采集意图，不直接改变角色 runtime。

每个逻辑 tick 的输入必须形成完整、可记录的：

```text
FrameInputSet
  tick
  canonical player order
  held buttons
  pressed edges
  released edges
```

单机、回放和网络的差别只在 FrameInputSet 的授权方：

- 单机：`LocalFrameInputCollector`；
- 回放：`ReplayInputSource`；
- 未来客户端：服务器 `AuthoritativeFrameBundle`；
- 未来服务器：`AuthoritativeFrameAssembler`。

AI 不是网络输入。AI 在相同 BattleWorld、seed、输入和顺序下由内核确定性计算；客户端不能发送 AI 的位置、伤害或最终决策作为战斗真值。

### 10.1 输入必须表达完整事实

`FrameInputSet` 不能只保存“这一帧有哪些 key-down 事件”。对每个 canonical player slot，至少要能重建：

- 当前完整 held bitset；
- 本 tick pressed edges；
- 本 tick released edges；
- 量化后的方向、目标或技能附加参数；
- 是否由真实玩家输入、确定性缺失输入规则或服务器托管产生；
- 输入 schema/version 和内容 hash。

组合键窗口、按住、松开和边沿消费仍按权威 C# 顺序执行。网络层只量化和封装输入，不能为了节省字段重定义 C# 输入语义。

### 10.2 权威帧不可变合同

每个 `(sessionId, frameId, playerSlot)` 只能产生一个权威输入：

1. 首次合法输入写入尚未锁定的 `ServerInputInbox`；
2. 内容完全相同的重复包幂等接受；
3. 同一键出现不同内容时记为 protocol conflict 并拒绝，不能以后到覆盖先到；
4. 到统一 frame deadline 后，服务器按房间规则补齐并锁定完整 `FrameInputSet`；
5. 锁定、模拟或广播之后的迟到输入不得修改该帧；
6. 权威帧内容、补齐原因和输入来源写入 `FrameHistoryRing`，成为 checksum、重连和回放共同事实。

客户端对重复 `AuthoritativeFrameBundle` 采用同样规则：同帧同内容幂等去重，同帧不同内容立即报错并停止晋升连续 ready 边界。

### 10.3 缺失输入是服务器会话规则

首个同进程原型可以严格等待全部玩家输入，用于验证协议和 checksum；生产模式不能永久被最慢连接无界拖住。候选规则为：

- 在短 grace 内沿用上一帧 held，pressed/released edges 置零；
- 超过 grace 后切 neutral；
- 到固定断线阈值后切服务器 AI 托管或结束连接。

具体规则和阈值暂不写死，但最终必须由 `BattleRoomSession` 在 StartBarrier 固定，进入 session fingerprint、权威输入历史和回放。客户端不能自行决定“沿用、neutral、预测或等待”。

## 11. AI 与空间查询

ECS 数据布局只能降低遍历成本，不能自动消除错误算法复杂度。

AI 目标查询采用：

1. 每 tick 按 slot 升序建立或确定性更新空间索引；
2. 使用 Loose Quadtree 作为 2D 战斗空间的 broad query；
3. 预建角色、队伍、特殊 OID 等只读索引；
4. 每个 AI 从局部候选中按权威距离和 tie-break 选择目标；
5. query 结果按稳定 slot/ordinal 消费；
6. 空间索引是可重建派生缓存，不把节点布局当作战斗身份；
7. AI 最终 target、输入和 RNG 结果进入 checksum。

不得：

- 每个 AI 扫描全部 runtime slot；
- 按有限半径删除权威本应检查的特殊对象；
- 因候选已满提前终止会改变结果的扫描；
- 以降频或跳过 AI tick 冒充性能优化。

## 12. Collision 与交互

碰撞广阶段采用 role-aware 空间结构：

- body bounds 进入可被攻击索引；
- attack itr 查询 body；
- 不为 body-body、itr-itr 或没有有效 role 的组合制造无用 pair；
- role/bounds 无法证明时走等价 fallback；
- broadphase 只减少不可能相交的 pair，不能删除真实有效 pair；
- 最终 pair/candidate 按权威 ordinal 排序和消费；
- A 攻击 B 与 B 攻击 A 的方向检查按 C# 权威保留。

`Concentrated1000` 若确实产生 499,500 个真实有效实体对，任何 ECS 或四叉树都不能把真实输出工作量变成 O(N)。该场景用于报告极限复杂度，不在未改变玩法合同的情况下预先承诺 30 FPS。

## 13. 表现发布边界

逻辑状态快照、表现观察和正式恢复快照是三种不同数据：

1. **逻辑状态**：BattleWorld 的 canonical state；
2. **表现观察**：在权威 `prePostprocessRender` 对应时点发布的纯数据画面输入；
3. **恢复快照**：完整 tick 正式边界上的可序列化世界状态。

表现方案：

```text
BattleKernel
  -> 在准确观察边界写入预分配 BattlePresentationSnapshot
  -> 始终写入确定性的 BattleEventJournal

Unity Presentation Host
  -> 每个渲染帧读取最新已发布 snapshot
  -> 解析 sprite/material/texture page
  -> 排序并 BuildCommands 一次
  -> 更新 central mesh / UI / audio
```

约束：

- `BuildCommands`、Sprite/Material 查找和 Mesh 上传不再属于逻辑 tick 成本；
- 追帧中间 tick 可以不物化完整画面，但必须保存逻辑事件；
- 最后可见 tick 必须发布准确画面；
- 正式事件使用稳定事件键去重，避免重放、追帧或恢复重复声音和特效；
- Unity Transform 每个渲染帧最多统一刷新一次；
- 表现插值不改变 BattleWorld 的位置、速度、碰撞或 checksum；
- Scene/Game 显示问题不得反向修改逻辑状态。

### 13.1 预表现三层合同

未来网络手感优化必须分层，不能把“预测”理解为提前执行技能：

| 层 | 允许 | 禁止 |
|---|---|---|
| `InputEcho` | 本地按键/UI、瞄准提示、低承诺轻音效 | 修改角色输入缓冲之外的战斗字段 |
| `IntentPresentation` | 可撤销起手、朝向预热、有限显示位置和技能预热 | 扣 HP、正式 opoint、命中、硬直、控制、死亡 |
| `ConfirmedResult` | 消费 BattleKernel 已确认事件，播放正式命中、受击、技能、音效和 UI | 反写逻辑或把 Animator 当状态机 |

首期网络闭环只保证 `InputEcho` 与 `ConfirmedResult`；是否增加本地移动/技能意图预测，要等 U7 可恢复快照和 S2 网络仿真后决定。远端对象以确认快照插值为主，不做与本地玩家同强度的预测。

### 13.2 表现事件稳定身份

每个正式事件至少包含：

```text
sessionId
logicFrame
sourceStableId
eventSequence
eventType
payload
```

稳定键用于处理重复网络包、追帧、回放、快照恢复和未来回滚。可重放事件、仅本地即时反馈和可撤销状态表现分池管理；音频、粒子、镜头和 UI 播放游标不进入 BattleWorld，但恢复后必须用确认事件游标避免重复播放。

## 14. 零 GC 与内存边界

战斗准备完成后执行 capacity seal。正式战斗窗口禁止：

- 新建临时 class、数组、字符串和装箱对象；
- lambda/闭包、LINQ、热路径委托生成；
- List/Dictionary 自动扩容和 rehash；
- 对象池耗尽后 Instantiate 或 new；
- 每 tick 反射、类型扫描、日志格式化和报告序列化；
- 音频首次查找、首次 coroutine 或首次 follower 组件创建。

必须预热：

- entity/task/resolver/GameObject；
- candidate、pair、VRest/aRest；
- AI snapshot 和空间索引节点；
- 输入、事件、sound、opoint 队列；
- presentation snapshot、sort、command、mesh；
- sprite/material/texture page/voice。

正式声明范围：

- NTSD formal battle tick：`0 B GC.Alloc`；
- NTSD driver 与 presentation 稳态：`0 B/frame`；
- Player 战斗窗口：项目战斗链不分配，Gen0/1/2 collection 不增加；
- Editor 只做观察，不能把 Profiler、SceneView、IMGUI 或插件分配算成 Player 正式失败，也不能据此声称整个 Editor 进程永不 GC。

池或容量耗尽必须有结构化 rejection/fault 计数。零 GC 不能以静默丢技能、漏 opoint、漏声音或漏碰撞为代价。

## 15. Snapshot、Checksum 与重放

### 15.1 Checksum

分域 checksum 至少覆盖：

- tick、seed、RNG state 和 call count；
- slots、active/pending/dormant、generation、stableId；
- identity、motion、frame、vital、input、links、combat；
- allocator、rest、stats、stage、battle flow；
- pending structural commands 和 visibility boundary；
- event sequence/cursor；
- catalog/stage/profile fingerprint。

表现对象、Sprite、Mesh、Material、Transform、Camera、音频播放头和 Editor 诊断不得进入逻辑 checksum。

### 15.2 可恢复快照

生产 `BattleStateSnapshot` 必须：

- 有 schema/version；
- 使用固定宽度字段和显式字节序；
- 完整恢复 World、allocator、RNG、输入游标、命令队列和事件游标；
- 不依赖 CLR 对象地址或 Dictionary 枚举顺序；
- 不直接把进程内存块当成跨平台协议；
- 使用复用 writer/buffer，避免采样期间分配。

恢复等价测试：

```text
World A 在 tick N 后继续运行
World B 从 tick N snapshot 恢复
A/B 输入相同的 N+1...M FrameInputSet
逐 tick checksum、RNG、slot、事件和最终世界一致
```

### 15.3 历史、快照与校验的统一生命周期

三种存储不能互相替代：

| 存储 | 内容 | 用途 |
|---|---|---|
| `FrameHistoryRing` | 每一帧不可变权威 `FrameInputSet` | 补发、回放、快照后重放 |
| `SnapshotRing` | 周期性完整 `BattleStateSnapshot` | 重连、严重落后、desync 恢复、观战起点 |
| `ChecksumHistory` | 周期性 overall 与按域 hash | 主动发现分叉并定位域 |

它们必须共享 frame id、session identity、schema 和生命周期。快照覆盖到 S 时，服务器至少保留能够从 S 重放到当前目标帧的完整输入历史；不能只发“当前状态”而丢失 RNG、技能内部状态、待处理结构命令和事件游标。

客户端本地快照可以用于诊断或减少本机重建成本，但服务器不得信任客户端磁盘快照作为权威。正常对局也不得周期性下发位置、HP 或 Buff 覆盖客户端来掩盖分叉；一旦需要状态恢复，必须进入显式 `RecoveringSnapshot` 并完成恢复后 checksum。

### 15.4 跨运行时确定性

首期保留权威 C# 的数值语义。按同一 seed/journal 在以下环境对比：

- Unity Editor Mono；
- Windows IL2CPP Player；
- Android ARM64 IL2CPP（架构兼容项；真机验证由用户后续执行）；
- 未来 .NET/服务器运行时。

若确认某个 `double`/数学域发生分叉，再只迁移该字段域到整数或定点数，并重新进行 C# 行为对照。不得在没有分叉证据时整体改写移动、伤害或碰撞数值。

## 16. Unity Client Host

Unity Host 负责：

- Loading 与 BattleBootstrap；
- 本地输入采样；
- Offline/Replay/Network host policy；
- 将 FrameInputSet 提交给 BattleSession；
- 资源、对象池、中央表现、UI、音频；
- GameObject shell 与 EntityHandle 的表现映射；
- Profiler、Editor 入口和压力工具。

Unity Host 不得：

- 在 MonoBehaviour Update 中直接修改战斗字段；
- 让 Transform/Animator/Physics 成为逻辑真相；
- 因 Addressables/BMP/音频加载完成顺序改变 spawn、碰撞或 RNG；
- 在网络回调中直接推进 BattleWorld；
- 让 presentation generation 或 GameObject 创建顺序影响逻辑 stableId。

当共享核心完全移除 Unity 依赖后，可以把 BattleSession 放入单个专用 simulation worker。核心仍是单线程确定性，只是从 Unity 主线程移到明确所有权的工作线程。输入通过不可变帧队列进入，表现通过双缓冲 snapshot 发布。该步骤用于 60/120 Hz 表现稳定性，不作为首批正确性迁移的前提。

## 17. 未来服务器 Host

未来服务器使用同一个 BattleKernel：

```text
Network Receive
  -> 验证后写 ServerInputInbox

Room Worker at frame deadline N
  -> 确定性补齐缺失输入
  -> 按 canonical player order 生成 FrameInputSet(N)
  -> BattleKernel.StepOneTick
  -> 写 FrameHistory / Checksum / Snapshot
  -> 广播 AuthoritativeFrameBundle
```

原则：

- 网络线程只入队，不直接修改房间世界；
- 一个房间内部串行推进；
- 多个房间可以分配到不同 worker/process；
- 服务器没有 Sprite、GameObject、Transform、Renderer 或 Audio；
- 协议与 transport 解耦，当前不绑定 UDP/KCP/ENet/MagicOnion；
- 先做同进程 loopback，再做独立进程，再接真实网络。

服务器广播可以低于 30 包/秒，但必须携带连续完整的 30 Hz FrameInputSet。客户端只能消费服务器授权的连续帧。

### 17.1 客户端与房间状态机

客户端：

```text
Disconnected
  -> Handshaking
  -> AwaitingStartBarrier
  -> PrimingAuthoritativeFrames
  -> Running / WaitingForGap / CatchingUp
  -> RecoveringSnapshot
  -> Running
  -> Ending / Faulted
```

服务端房间：

```text
Created
  -> WaitingForPlayers
  -> StartBarrier
  -> Running
  -> Finishing
  -> Archived
```

`StartBarrier` 固定协议版本、seed、资源指纹、capacity profile、canonical player slots、input delay、缺失输入策略和起始帧。进入 `Running` 后，单个客户端不能改变这些确定性配置。

### 17.2 控制面与高频数据面

控制面处理：登录、匹配、建房、握手、资源指纹、开始屏障、重连、快照请求和结束。它可以使用 RPC、MagicOnion/StreamingHub 或其他可靠通道。

高频数据面处理：

- `ClientInputCommand`；
- `AuthoritativeFrameBundle`；
- ACK、server/client sequence；
- 最近帧冗余或可靠补发；
- checksum report/request。

权威帧是流式消息，不为每个逻辑帧创建 request id、Future、反射派发和单独响应对象。协议 ID、序列化器和 dispatcher 在构建期生成；运行期使用复用 buffer，并对包长、frame window、session、player identity 和 schema 做边界验证。

### 17.3 混合同步的硬边界

NTSD 的“混合”含义是：

- 正常战斗：输入帧同步；
- bootstrap/rejoin/desync/观战：服务器状态快照 + 快照后的输入重放；
- 表现：确认逻辑快照的插值与可撤销预表现。

它不表示客户端上报伤害、服务器另算伤害，或服务器每几秒下发 HP/位置覆盖客户端。若未来某个玩法确实需要 FPS 式服务器命中、AOI 状态裁剪或 Source 式 lag compensation，必须作为新的同步模型单独立项，不能混入共享 BattleKernel 的基础合同。

### 17.4 安全假设

- transport 加密不能证明客户端输入诚实；
- 客户端只提交受限输入意图，服务器验证 frame window、频率、身份和范围；
- 服务器同核运行产生的 checksum 是权威诊断基准；
- 两端 checksum 不同不能用客户端多数投票确定真相；
- 服务器保留输入历史、关键诊断和必要的战后回放审计；
- 透视属于纯帧同步完整信息下发的固有限制，不能用协议加密或 checksum 宣称彻底解决。

## 18. 性能目标与线程预算

### 18.1 当前第一目标：1000 AI / 30 FPS

正式 `Dispersed1000` 与 `Combat1000`：

- 1000 个真实生产 GameObject 和逻辑实体；
- 全 AI、输入、DAT、碰撞、命中、opoint、生命周期、声音和中央表现开启；
- 逻辑固定 30 Hz；
- 单机每个可见帧最多一个 tick；
- 预热后连续至少 60 秒；
- P95 完整帧不高于 33.33 ms；
- formal tick 与项目表现链稳态 0 B；
- checksum、RNG、slot、事件与 C#/旧 oracle 一致；
- cleanup 恢复完整。

`33.33 ms` 是当前 30 Hz 容量门，不是允许核心长期占满的理想预算。通过该门后还要分别记录 BattleKernel、Unity host、presentation、render thread 和系统余量；正式发布目标必须根据目标 PC/移动设备的 P95/P99 留出网络、快照、OS 调度和偶发尖峰空间。知识库中的 `<5/<10/<15 ms` 只作容量思维参考，未经过 NTSD 同口径实测前不写成硬常量。

### 18.2 未来 60/120 Hz 表现

- 60 Hz 渲染预算为 16.67 ms；
- 120 Hz 渲染预算为 8.33 ms；
- 30 Hz 逻辑 tick 即使低于 33.33 ms，若仍同步阻塞 Unity 主线程，也可能破坏 60/120 Hz；
- 因此先减少单 tick 成本，再把纯 C# BattleKernel 放到专用 worker，通过双缓冲表现快照解耦；
- 移动端是否启用专用 worker、worker 数量和热预算必须通过真机测量，不能照搬 PC。

### 18.3 优化优先级

1. 错误复杂度：AI 全表扫描、无效碰撞 pair、重复查询；
2. 每 tick 重复工作：输入 facts、runtime snapshot、BuildCommands、handle/material 解析；
3. 引用对象和 Unity API：虚调用、组件查找、Transform、协程、日志；
4. 数据布局：对象图改成连续 SoA/bitset/sparse set；
5. 安全并行化：只读 gather、稳定 merge、表现和多房间。

ECS 是第 4 项的结构基础，也帮助前 2、3 项，但不能代替空间算法和表现边界修正。

持续超过预算时的处理顺序固定为：先区分 GC/Unity API 尖峰、错误复杂度、重复工作和数据布局；再决定是否迁移到专用 simulation worker。不得把 AI 降频、跳过有效碰撞、缩短技能链或限制 DAT 结果当作“框架优化”。

## 19. 实施路线

所有阶段都必须小步、可回退。不得一次性删除旧 runtime 后再尝试补行为。

### 19.1 当前执行硬边界

以下边界由用户在 2026-08-11 明确确认。它们的优先级高于后续章节中关于未来服务器的技术展开；上下文压缩、任务交接或实现阶段切换均不得自行扩大范围：

1. 当前只执行 U0～U9，先完成单机 BattleKernel、确定性闭环、零 GC 和 1000 AI 性能目标。
2. U0～U9 只保留未来服务器必需的纯 C# 接口、数据合同和可验证边界，不实现服务器房间、连接、广播或权威业务流程。
3. 当前不选择、不接入、不预埋具体网络库；transport 类型不得进入 BattleKernel。
4. 当前不实现 ACK、Jitter Buffer、服务器房间、登录、匹配、断线重连或真实网络恢复流程。U0～U9 可以定义并测试其所需的不可变帧、快照、历史和 checksum 基础合同，但不能据此把服务器阶段标为已实现。
5. U9 完成全部验收后必须停在阶段门，由用户明确确认是否进入 S0；不得自动继续服务器实施。
6. 用户批准后，S0 首先实现同进程、内存直连的服务器与多客户端世界，不使用真实 Socket；S0 只验证权威帧、连续消费、同核模拟和 checksum，不提前绑定生产 transport。

因此，当前不存在服务器代码不是 U0～U9 的阻塞项。反过来，在 U9 之前新增服务器业务、真实网络或第二套战斗结算属于越界实现。

### U0：工作树与权威基线封套

状态：已于 2026-08-11 完成。完整证据见 `Docs/unified-battle-u0-baseline-20260811.md`。Production Authority400 trace 仍在 tick 0 记录 Unity DAT 适配导致的 manifest 前置差异；authority-DAT diagnostic full trace 为 6/6 tick `equal-diagnostic`。Combat1000 两轮最终 lockstep hash 一致、sampled logic GC 为 0 B/tick，但 Unity frame P95 仍未达到 U9 门禁。

目标：确定迁移前的可复现 oracle。

工作：

- 审查当前未提交的 L0/L1、AI、碰撞、表现和零 GC 修改；
- 将已有修改按“已验证、候选、负实验、用户工作”分类；
- 固定 seed、roster、DAT/profile、输入 journal 和性能矩阵；
- 记录 Authority400 的逐 tick checksum、RNG、slot 和事件序列；
- fresh compile、focused tests、完整 self-check 和当前 1000 AI 报告。

完成门：迁移前行为与性能基线可重复；不能把当前脏工作树默认视为已完成。

### U1：Canonical Input 与 Host Policy

目标：所有模式共享唯一 FrameInputSet 输入边界。

工作：

- Unity 输入只采集意图；
- Local provider 生成完整 held/pressed/released；
- OfflineLocal、ManualReplay、NetworkLockstep 策略独立；
- 单机普通 Update 最多一个 tick；
- 同一 journal 重放产生相同 checksum；
- 为未来网络入口固定 frame/player key、幂等重复、冲突重复和锁定后不可变测试。

当前工作树已有候选实现，必须先复验再晋升，不能重复另写第二套。

完成记录（2026-08-11）：现有 `FrameInputSet`、local provider、strict delayed input buffer、replay journal 和 `BattleLockstepSession` 已晋升为唯一输入边界；`OfflineLocalTickPolicy` 已固定为普通 Unity `Update` 最多自动推进一个逻辑 tick，积压只留待后续 `Update`，`ManualReplay` 与 `NetworkLockstep` 不消费 Unity 墙钟。23 项聚焦测试 fresh PASS，同一三帧 journal 在重建世界后逐 tick checksum 一致，完整 `BattleRuntimeSelfCheck` fresh PASS。实现与证据详见 `Docs/unified-battle-u1-input-host-policy-20260811.md`。

### U2：表现发布边界

状态：已于 2026-08-11 完成。完整边界证据见 `Docs/unified-battle-u2-presentation-host-20260811.md`。逻辑 tick 只发布纯数据，中央命令、资源解析、排序、Mesh 与音频已移到 Unity host 每个可见帧最多一次的边界；fresh 聚焦测试 262/262 PASS，完整 self-check PASS，Combat1000 最终 lockstep hash 与 U0 相同。后续又移除了 CentralOnly 每个 `LateUpdate` 对全部 Legacy renderer shell 的重复扫描：聚焦测试 246/246、自检、零 GC 与 lockstep hash 均通过，同口径 CPU hierarchy 的 Main Thread 平均从 45.6808 ms 降到 40.1213 ms，详见 `Docs/unified-battle-u2-centralonly-renderer-shell-bypass-20260811.md`。这仍只关闭 U2 架构边界与对应重复扫描，不宣称 U9 性能门禁完成。

目标：把表现构建从逻辑 tick 中移出，同时保留 C# 的观察时点。

工作：

- 预分配 BattlePresentationSnapshot；
- 逻辑边界只复制纯数据；
- BuildCommands、资源解析、排序、Mesh 和音频由 Unity host 每渲染帧处理一次；
- 中间追帧 tick 保留事件但不重复物化表现；
- 有表现/无表现运行 checksum 一致。

### U3：ECS World 与只读 Shadow

状态：已于 2026-08-11 完成。完整证据见 `Docs/unified-battle-u3-ecs-readonly-shadow-20260811.md`。固定容量 Direct SoA、bitset、sparse store、slot/generation 身份和完整 runtime fingerprint 已建立；shadow 默认关闭，Compare 模式只读且没有反写路径。fresh ECS 聚焦测试 8/8 PASS，交叉回归 14/14 PASS，完整 self-check PASS，Authority400 authority-DAT diagnostic 6/6 tick 相等，`Extended1000` 预热后 capture/validate 为 0 B。该阶段只关闭 ECS 数据地基，不宣称 U4～U9 或 1000 AI / 30 FPS 已完成。

目标：建立专用混合 ECS，但不改变 canonical writer。

工作：

- EntityHandle、capacity profile、SoA stores、bitsets、sparse stores；
- 按当前旧世界每个 tick 同步 shadow；
- 对比全部字段、slot、generation 和 query membership；
- 禁止 shadow 反写旧 runtime。

### U4：纯数值与高频 Pass 迁移

状态：已于 2026-08-11 完成。cooldown 切片已完成并晋升默认，权威合同、双实现模式、零 GC、1000 AI A/B、完整 self-check 与 Authority400 full trace 证据见 `Docs/unified-battle-u4-cooldown-migration-20260811.md`。AI 数据化感知/决策链也已通过 1000 AI 严格 A/B、十域 lockstep hash、零 GC、185 项聚焦测试和完整 self-check，`DataOrientedCanonical` 已晋升生产默认；证据见 `Docs/unified-battle-u4-ai-profile-promotion-20260811.md`。character Stage-Z 数据路径已完成行为、零 GC、Authority400 与 1000 AI A/B 验证，但目标 pass P95 只改善 4.66%，未达到 10% 晋升门槛，故正式默认保持 Legacy；完整证据见 `Docs/unified-battle-u4-stagez-migration-20260811.md`。FramePostProcess 同样完成权威合同、33 项交叉回归、Authority400、零 GC与 1000 AI A/B，但 P95 恶化 55.15%，因此默认保持 Legacy；完整证据见 `Docs/unified-battle-u4-frame-postprocess-migration-20260811.md`。CandidateCollect 的 LegacyOnly 零 ITR 前置路径虽保持哈希一致和零 GC，但相邻 A/B 中逻辑均值慢 1.84%、P95 慢 2.78%，已撤回该实验并恢复原路径；证据见 `Docs/unified-battle-u4-candidate-zero-itr-preflight-20260811.md`。LateEntityUpdate 新鲜细分测量表明完整 pass average 为 2.9092 ms，最大子段 0.7622 ms，纯数值 Recovery 仅 0.2290 ms；逐 slot 生命周期段进入 U5，不新增低收益 writer，证据见 `Docs/unified-battle-u4-late-entity-update-assessment-20260811.md`。U4 的完成表示所有计划切片均已迁移或完成数据化取舍，不表示 U5～U9 或 1000 AI / 30 FPS 已完成。

迁移建议顺序：

1. cooldown、基础 frame/motion/bounds；
2. CharacterInput facts 与 AI decision；
3. AI spatial query；
4. CandidateCollect 的 participant/broadphase/exact；
5. LateEntityUpdate 中无结构变化的数值段。

每次只允许一个 canonical writer；旧路径保留只读 oracle，逐 tick 比较后再切换默认。

### U5：Interaction、Hit、Rest 与复杂生命周期

目标：迁移结果敏感的交互域。

工作：

- cpoint、held、link；
- character/object hit；
- aRest/vRest；
- opoint 分段播放；
- spawn/destroy/free/unregister/generation；
- stage 和 battle results。

这是最高风险阶段，必须按权威 boundary 串行迁移，不能用通用 tick-end command buffer 简化。

当前进度（更新至 2026-08-12）：

- `CharacterHitConsume` 空候选精确 `LF2Character` 快速路径已经完成并晋升生产默认；派生类型、快照过期、候选源不可读或存在候选时全部 fail closed 到权威对象路径；
- 聚焦测试覆盖空候选等价、候选源不可用、过期快照刷新、派生虚调用与预热后 `0 B`；
- 同 seed、1000 AI、30 warmup + 180 sample 的稳定相邻 Legacy D / Fast E A/B 保持十域 lockstep hash 完全一致和正式 tick `0 B`，目标 pass 均值/P95 分别改善 56.5%/59.9%，逻辑 tick 均值/P95 分别改善 14.8%/27.3%；
- 完整证据见 `Docs/unified-battle-u5-empty-character-hit-consume-20260811.md`；这只关闭 U5 的 character hit 空候选切片，不代表 U5 整体完成；
- character 空候选快速路径内部的 runtime candidate-count gate 已完成 7 项聚焦测试、244 项联合回归、fresh self-check 与 1000 AI 隔离 A/B；行为、零 GC 和十域 hash 均一致，但目标 pass average/P95 分别慢 12.90%/4.71%，因此生产默认继续使用已晋升的 range proof；
- runtime count 候选只作为诊断实验保留，完整证据见 `Docs/unified-battle-u5-character-runtime-candidate-count-gate-20260811.md`；本结论不回退 character 空候选 whole-pass 优化本身；
- Stage 场景配置已经移到 `SimulationTickDriver` 的 tick 宿主边界，每 tick 发布一次 `BattleStageRuntimeState`，`StageBounds`、`PreFrameBounds` 和 ECS Stage-Z pass 只读 runtime 快照；
- 同配置 1000 AI Legacy/Host A/B 中，Unity 场景读取从 630 次降至 210 次，20 个 parity/lockstep 分域 hash 全部一致且正式 tick 维持零 GC；但总体 tick average/P95 分别慢 1.44%/3.30%，所以该切片只作为确定性边界晋升，不宣称性能收益；
- Stage 宿主快照的完整证据见 `Docs/unified-battle-u5-stage-host-snapshot-20260811.md`。复杂 cpoint、held、link、object hit、aRest/vRest 与结构生命周期仍按 U5 待办处理。
- `PreInteraction` whole-pass no-op 证明已经通过 7 项聚焦测试和 1000 AI 正式 A/B；91/210 个 tick 被证明为全局无副作用，跳过 273,000 次对象调用，其余 119 个 tick fail closed 到完整权威路径；
- 该切片的 20 个 parity/lockstep 分域 hash 全部一致且正式 tick 零 GC，目标 pass average/P95 改善 35.14%/27.91%，逻辑 tick average/P95 改善 10.84%/20.50%；完整证据见 `Docs/unified-battle-u5-preinteraction-noop-proof-20260811.md`；
- whole-pass proof 只关闭可证明的空操作路径，不表示真实 cpoint、held、link writer 已迁移；存在交互时仍保留原对象路径和原顺序。
- `PreInteraction` fallback 的逐 participant 精确过滤已完成 8 项真实 kind1/kind2/stale-held 聚焦验证、245 项联合回归、fresh self-check 与三轮 1000 AI A/B；派生类型和过期快照 fail closed，真实 writer 与升序顺序保持不变；
- 三轮目标 pass average/P95 平均改善 35.46%/43.35%，整 tick average/P95 平均改善 2.81%/4.64%，六轮均为正式 tick `0 B` 且十域 hash 一致，故晋升生产默认；完整证据见 `Docs/unified-battle-u5-preinteraction-participant-filtering-20260812.md`；
- 该晋升移除的是被证明无副作用的对象调用，不能扩大为真实 cpoint/held/link canonical writer 已迁移的声明；
- Late tail no-op 候选已完成 6 项聚焦测试和 1000 AI A/B；虽然跳过 210,000 次方法调用且 hash/零 GC 一致，但 `TailAndQueuedFlush` average 慢 8.17%、Late pass average 慢 9.06%、逻辑 tick average 慢 6.29%，因此不晋升；
- 生产默认继续使用完整权威 late tail，候选只作为诊断关闭路径保留；完整证据见 `Docs/unified-battle-u5-late-tail-noop-assessment-20260811.md`。
- `ObjectHitConsume` 空候选 whole-pass 证明已通过 7 项聚焦测试、233 项压力工具回归和两组相邻 1000 AI A/B；当前 DAT 类型、slot generation、派生虚调用和不可读候选源全部 fail closed；
- 两组目标 pass average/P95 合并改善 27.80%/22.20%，正式 tick 均为 `0 B`，最终 lockstep overall hash 完全一致；整 tick average 合并波动为 -2.59%，因此只声明稳定局部收益，不声明总体帧率改善；完整证据见 `Docs/unified-battle-u5-empty-object-hit-consume-20260811.md`；
- `aRest` 的 canonical writer 已由 U4 `BattleEcsCooldownPass` 接管；`vRest` 当前已经由 `RuntimeRestStore` 按 handle/generation 保存并由权威 pair pass 消费。U5 不新增第二套 rest writer，后续只继续核验真实 hit/cpoint/opoint 对 rest 的写入与可见边界；
- 正向 link validation 已完成 live-runtime 数据候选、6 项聚焦测试、243 项联合回归、fresh self-check 与 1000 AI A/B；十域最终 hash 完全一致且正式 tick 为 `0 B`，但目标 pass average/P95 分别慢 50.73%/27.80%，因此生产默认保持 Legacy；完整证据见 `Docs/unified-battle-u5-positive-link-validation-assessment-20260811.md`；
- 该评估只拒绝当前逐 slot 数据候选，不能据此声明真实 cpoint/held/link writer 已迁移；统一 canonical link store 必须与真实 writer 的同 tick 可见性一起处理，不能读取 tick-end shadow；
- character/object hit 的权威 writer 合同已经从 C# `GameTick -> HitResolver -> HitResolve` 闭合；完整原子边界包含 `PrevFrame2`、slot/candidate 顺序、abort residual、preprocess、RNG、所有 kind、伤害统计、rest/link、声音/事件、opoint 与生命周期，禁止只迁移扣血片段；
- Unity 已加入默认 `Disabled`、固定容量、只读的 `BattleEcsHitExecutionPlan` 影子；显式 `ShadowCapture` 时在两个正式 pass 前冻结 attacker/candidate/itr 顺序，任何输入不可读均 fail closed，target generation 只作诊断而不新增权威判定；
- `ShadowCompare` 已在正式旧 writer 消费期间逐项核对 pass、attacker handle、candidate ordinal、target slot、itr index/fingerprint 与原始 consume 标志；Legacy range 只在该诊断模式开启时补取 attacker handle，默认路径不增加 handle 查询；多读、少读和内容不一致均 fail closed；
- 当前 pair preprocess 及预消费副作用影子已经接入四条真实 consumer 链，只在旧 `ApplyReleaseSceneQueryConsumeEffects` 前后观察；计划使用 preprocess 后的实际投影，不错误复用碰撞冻结时的原始标志；
- kind9 已闭合 kind9→kind0、attacker HP 归零；重武器已闭合 target/held link、两组 vRest、随机 frame、Vy 与 RNG state/call count。影子默认关闭、不替代 writer、不推进 RNG，预热后为 `0 B`；
- dispatch 只读观察已接入 character、DAT character、weapon 与 special attack 四条真实 consumer；独立 OID300 投影验证成功 redirect 只终止当前 attacker 的剩余候选，下一个 attacker 必须继续，伪造终止 fail closed；
- 全部权威 kind 的独立 disposition 投影已经接入并覆盖 `0/9/6/8/14/15/16/10/11/1/3/2/7`、未转换 `4/5` 与未知 kind；未转换 kind4 现按权威 no-op，kind5 替换不再倒灌触发前序 held release，weapon/special 的 kind6 现只写 hit-confirm 后返回；错误或缺失 disposition 均 fail closed；
- writer-effect 只读 oracle 已闭合 kind `6/8/14/1/3/2/7/10/11/15/16` 的精确状态变化，且明确不把 Unity dispatch `bool` 当成 C# 权威成功语义；kind1 oracle 实际检出并修正旧 character consumer 未写双方速度、朝向、抓取对位、槽位与持续时间的差异；无效/过期 kind16 link 也按权威保持原状态；
- damage `0/9` 的精确 oracle 已覆盖标准角色 HP/HPMax/统计、轻中重硬直与倒地、标准致死时 HPBound/combo/kill/damage stats/强制 fall、X/Y 击退、rest、RNG、声音和 hit-record，alternate 非致死/致死，以及标准武器类型 `1/2/4/6` 的 hit-confirm2、weapon HP、effect0/effect4 声音、随机帧、vRest 与 heavy low-fall 分支；type3 已覆盖 object-hurt、relation/holder-copy、motion 清零、rest、hit-record、state3005/3006 同步、D1 直接/活动身份替换，以及 effect `0/2/3/5/21/22/23/30/5005/5999/6033` 的 frame、主/追加声音、PP 扣减与下限；effect20 对非角色 DAT 由权威碰撞收集前置拒绝，未伪造为可达 writer 输入；
- damage oracle 实际检出并修复 `LF2WeaponBase.SetFrameDirect`、`LF2SpecialAttack.SetFrameDirect` 与公共 `DirectWriteHeldFramePreserveWaitCounter` 未同步 `Runtime.Frame` 的双帧镜像遗漏；这些修复恢复的是 C# 单一 Frame 真值，不改变权威切帧时序；
- 权威源码全量检索确认 `AbortRemainingHitPairs = true` 只有 `ApplyOid300SpecialHit` 一处，因此 abort 来源与只跳过同 attacker 的边界已经关闭；
- 命中计划在 alternate 致死补齐后聚焦测试 96/96 PASS（job `562ce635bbf64029a5b1319f45ec6dcd`）；命中计划、character/object 空候选与碰撞命中见证联合回归 112/112 PASS（job `798b5f79820c400cbe61497a1de3c186`），完整 `BattleRuntimeSelfCheck` 于 `2026-08-12 08:52:47` fresh PASS，并保留 `CharacterHit -> RandomDrop -> ObjectHit` 边界；
- OID `0xD6` 对角色命中后攻击者 HP 归零的字段投影和定向测试已加入，`dotnet build Assembly-CSharp.csproj --no-restore` 本轮 `EXIT=0`；但测试 job `1402bf42fcab4fea86ceaa01d1babd82` 启动时 Unity Editor 因 AssetDatabase `MDB_READERS_FULL` 原生崩溃，未形成运行时 PASS，故该项保持“逻辑已写、编译通过、运行时未验证”；
- OID `0xC9` 的 `FreeEntity(attackerSlot)` 已建模为独立生命周期 shadow，预期同时核对旧 handle 失效、slot 未占用、generation 精确递增一次、occupant 清空及攻击者 runtime slot 清为 `-1`；观察 world 在 dispatch 前保存，且正向、缺失观察、未释放伪完成三类聚焦测试已写入。Editor C# 工程独立编译通过，但 Unity Editor 尚未恢复，因此同样保持“逻辑已写、编译通过、运行时未验证”；完整证据见 `Docs/unified-battle-u5-hit-writer-contract-20260812.md`；
- 该切片现已关闭“权威合同、输入冻结、旧链候选读取、preprocess、全部 kind 消费 disposition、已覆盖预消费副作用、全部非 damage kind 的基础状态副作用、damage 主要基础分支、标准致死、alternate 致死、type3 D1 identity/状态同步、实际 dispatch 回报与 OID300 abort 对照”，并已写入 OID `0xD6` 字段投影和 OID `0xC9` 生命周期投影；但尚未完成这两项的 Unity 定向验证，也未关闭剩余特殊 effect、character-DAT type3 effect tail、声音/正式事件全集、opoint/其余生命周期副作用，不表示正式 writer 已迁移；
- 当前仍未关闭：真实 cpoint/held/link writer、存在候选时的 character/object hit writer、opoint 分段播放，以及 spawn/destroy/free/unregister/generation 生命周期。

### U6：移除对象式逻辑热循环

目标：BattleKernel 成为唯一战斗真值。

- LF2Character/LF2Weapon/LF2OtherObject 逐步退为 Unity shell/兼容 adapter；
- GameObject 不再拥有逻辑字段；
- 移除每实体 MonoBehaviour 战斗 Update；
- 保留对象池与表现绑定；
- 删除已经通过验收的旧 canonical writer，不永久维持双实现。

### U7：生产 Snapshot、Restore 与跨运行时门禁

- 完整 BattleStateSnapshot；
- snapshot restore + journal replay；
- FrameHistory、SnapshotRing 与 ChecksumHistory 的 frame/schema 生命周期一致；
- Editor Mono、Windows IL2CPP 与未来服务器运行时对比；
- Android ARM64 兼容合同保留，但真机结果由用户后续提供，不属于当前 Codex 验收；
- 仅对确认分叉的数值域制定定点迁移。

### U8：专用 Simulation Worker 与 60/120 Hz 表现

- BattleKernel 移出 Unity 主线程；
- 固定所有权的输入队列与双缓冲 publication；
- 无共享可变 Unity 对象；
- 主线程只负责表现；
- 对移动端线程数和发热单独测量。

### U9：1000 AI 正式验收

- Idle/Move/Dispersed/Combat/Concentrated 矩阵；
- Editor 趋势 + Windows Player 正式报告；
- 60 秒以上、P95、GC、SetPass、Render Thread、拒绝计数、checksum、cleanup；
- `Dispersed1000` 与 `Combat1000` 达到 30 FPS 后才关闭单机容量目标。

### S0～S5：服务器实施

- S0：同进程服务器 + 多客户端世界，无 Socket；验证 StartBarrier、权威帧不可变、严格连续消费和双端 checksum；
- S1：内存 transport 的 Jitter Buffer、ACK、冗余帧、重复、冲突、缺帧和 frame deadline；
- S2：服务器快照、历史帧、desync 与重连恢复闭环；在本阶段末才决定是否需要预测回滚；
- S3：独立 headless/.NET 进程与共享协议程序集；
- S4：真实 transport，注入延迟、抖动、丢包、重复、乱序、断线和重连；
- S5：多房间 worker/process 调度、监控、容量、回放审计和部署扩展。

服务器阶段只在 U9 完成且用户再次确认后开始。S0 先做同进程内存原型；S0～S2 证明权威帧、协议语义和恢复合同后，才进入独立进程与真实 transport。任何阶段都不得反向创建第二套战斗循环。

## 20. 每阶段统一验收门

每个阶段只有同时满足以下条件才能标记完成：

1. 实现边界和 canonical owner 已记录；
2. Unity 脚本编译 0 error；
3. 聚焦测试 fresh PASS；
4. 完整 `BattleRuntimeSelfCheck` fresh PASS；
5. Authority400 对照 fresh PASS；
6. 输入、RNG、slots、aRest、vRest、stats、events、overall hash 一致；
7. 目标性能 A/B 使用同一 seed、负载和采样口径；
8. 正式窗口无非预期分配、扩容或 pool rejection；
9. cleanup 后 world、GameObject、slot、pool 和 host policy 恢复；
10. 专项文档与本文状态一致。

必须区分：

- 代码已写；
- 编译通过；
- self-check 通过；
- Authority400 对照通过；
- 1000 AI 性能通过；
- Play Mode 目标场景通过；
- 对应阶段已完成。

不得用隔离编译、单个 hash、短样本或 simulation-only 结果扩大成完整完成。

## 21. 回退与故障策略

- shadow/read-only 功能可以在测试启动前切换；
- canonical writer、allocator、query 和 snapshot schema 只能在 ResetWorld/合法 restore 边界切换；
- 运行中的 BattleWorld 不支持随意热切换数据所有权；
- checksum 分叉立即停止晋升，保留 witness，不继续叠加优化；
- 任何优化若三轮同口径 A/B 的 median 与 P95 改善不足 10%，原则上不提升为默认，除非它关闭了正确性、GC 或结构性风险；
- 发现 capacity fault、pool rejection、候选丢失或事件序列变化时视为失败，不以帧率改善覆盖；
- 旧路径只保留到新路径获得充分证据，避免永久双维护。

## 22. 当前不阻塞实施、但必须后续测量的决策

以下事项现在不写死：

- 未来 transport 使用 UDP/KCP/ENet 或其他方案；
- 正式 input delay、frame deadline、缺失输入 grace/neutral/托管切换点、history 和 snapshot 周期；
- 是否实现客户端预测与回滚；
- PC 每局默认扩展容量；
- unmanaged store 或 SIMD 是否值得引入；
- 客户端专用 simulation worker 在各移动设备上的线程与热预算；
- 哪些已证明跨运行时分叉的字段迁移到定点数。

这些决策不阻塞 U0～U3。必须根据新鲜测量和确定性证据决定，不能凭通用 ECS 或网络经验直接写死。

## 23. 明确禁止的网络与恢复设计

以下做法不进入 NTSD 方案：

1. 丢失攻击或技能输入后直接忽略，不做冗余、ACK 或补发；
2. 同一玩家同一权威帧以后到内容覆盖先到内容；
3. 已锁定或已广播帧被迟到输入修改；
4. 客户端上报命中、伤害、HP 或位置作为战斗权威结果；
5. 服务器另写一套伤害/技能规则，与客户端 BattleKernel 并存；
6. 正常战斗周期性用状态包覆盖位置、HP、Buff 来掩盖 desync；
7. 用多数客户端 checksum 投票代替服务器同核运行；
8. 把传输加密当作客户端不会作弊的证明；
9. 信任客户端磁盘快照作为权威恢复状态；
10. 用动态 dt、无限 while 追帧、AI 降频或跳过战斗 pass 处理性能不足；
11. 把 FPS/Source 式 lag compensation 或 UE 状态复制直接混入基础 lockstep；
12. 在 S0 前绑定具体网络库，或让 RPC/transport 类型进入 BattleKernel。

完整来源与理由见 `Docs/lockstep-knowledge-base-audit.md`。

## 24. 建议执行定调

建议批准以下定调后开始 U0：

1. 战斗逻辑仍以 C# 权威工程为唯一规则依据。
2. 使用 NTSD 专用“Direct SoA + Bitset + Sparse Set + Pool/Ring + Loose Quadtree”混合 ECS。
3. 不使用 Unity DOTS，不实现通用 Archetype ECS。
4. 新内核不新增 partial，不使用全局可变 static 保存战斗会话状态。
5. 单机、回放、客户端和服务器共享唯一 `StepOneTick(FrameInputSet)`。
6. 逻辑固定 30 Hz；网络包频率和渲染频率独立。
7. 先完整执行 U0～U9，完成单机确定性、表现边界和 1000 AI / 30 FPS；U9 验收后停下并等待用户确认是否进入 S0。
8. 按 U0～U9 小步迁移，不进行一次性重写。
9. T8 默认 `stage.dat` 与 Android 真机继续排除。
10. 正常战斗输入同步与恢复快照同步严格分开，权威帧锁定后不可变。
11. U0～U9 只保留服务器所需接口边界，不实现服务器业务、ACK、Jitter Buffer、房间、登录或重连，也不选择网络库。
12. 用户批准进入服务器阶段后按 S0～S5 推进；S0 只做无 Socket 的同进程内存 loopback，证明协议与恢复后再选择 transport。

用户确认该定调后，第一个实际执行批次是 U0：审查并验证当前工作树中已经存在的候选修改，建立可重复基线，而不是继续叠加新的 ECS 代码。
