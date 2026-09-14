# 战斗性能全链路路线图：1000 实体 × Stats 120 渲染帧

> 计划标识：`BATTLE-PERF-STATS120-ROADMAP-001`
>
> 创建日期：2026-09-13；本版：R1（2026-09-14，按外部综合评审修正）
>
> 当前状态：`DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`
>
> 性质：性能路线方案与未来实施框架。本文件不授权修改任何 C#、Scene、
> Prefab、asmdef、资源、ProjectSettings 或运行行为；所有实施批次获批后须按
> `docs/ai/CHANGE-LEDGER.md` 规则独立立项。
>
> 权威入口：`docs/ai/CURRENT-AUTHORITY.md`。规则权威为 NTSD 2.8-Logan 正式
> EXE + playable source closure；DAT 与角色相关图片内容权威按 **D-023**
> （2026-09-12）采用 NTSD 2.8-Logan 正式 `resources/runtime`，Unity 原
> 138-DAT/旧图片仅为迁移前基线。本计划不改变战斗行为权威、33 ms 正常逻辑
> cadence、十一阶段有序关闭合同。
>
> 评审说明：全文标注证据等级 `[已验证-代码]`、`[已验证-文档]`、
> `[推演-待测]`、`[提案-待批]`；请按等级区分事实与假设。
>
> 修订记录：R0（2026-09-13 初版）；R1（2026-09-14）——按 GPT6 综合评审修正：
> F2 事实基线（dedicated worker 已默认存在）、F4/F5/F8 措辞降级、Step B 重写
> （Host 保留 cadence 所有权）、cadence 统一为精确 33 ms 表述、内容权威切换
> D-023、验收口径 Player 化、新增 D5/D6。详见第 11 节。

## 1. 目标与不变量

### 1.1 目标定义

在 1000 实体极限战斗场景下，**渲染帧率稳定达到 120 FPS**（正常逻辑 cadence
不变，见 1.2）。

正式验收口径（R1 修正：Stats 面板仅作现场辅助，不作为唯一验收依据）：

- 指定 Development Player 构建 + 指定设备/分辨率/图形 API/render scale/
  VSync/`Application.targetFrameRate`/热状态（具体锁定值见开放决策 D5）；
- 10 分钟 soak；**最终硬门 = 端到端 presented frame interval p99 ≤
  8.33 ms**（R2：CPU/GPU 流水并行下主线程单指标不充分——render thread 或
  GPU 瓶颈同样导致掉帧）；
- CPU 主线程/render thread/GPU frame time 分别统计 p50/p95/p99/max，
  用于**瓶颈归因**：任一阶段不得持续超过 8.33 ms；
- 附加指标：missed present/jank 计数、最长连续超预算帧数、cadence slip
  计数、presentation latency、publication age、input-to-visible latency
  （定义见 4.0）；
- 不接受"平均 120 但周期性掉帧"的尖峰形态；Stats 数字仅作辅助显示。

### 1.2 不变量（本计划任何步骤不得触碰）

| 不变量 | 依据 |
|---|---|
| 正常逻辑 cadence：精确 33 ms 一 tick；F5 快速 cadence：精确 3 ms；LocalFreeRun wall-clock debt 最多 2 个当前 cadence interval。全文不使用"30 Hz"作为 cadence 表述（`SIM_TICK_RATE=30` 仅保留既有每 tick 像素换算含义） | `[已验证-文档]` AGENTS.md 第 6 节 cadence 契约 |
| 战斗可观察行为与 NTSD 2.8-Logan 正式 EXE 一致 | `[已验证-文档]` AGENTS.md 第 2 节权威体系 |
| DAT 与角色相关图片内容权威：NTSD 2.8-Logan 正式 `resources/runtime`（D-023，2026-09-12）；Unity 138-DAT/旧图片仅为迁移前基线 | `[已验证-文档]` AGENTS.md D-023 条款、CURRENT-AUTHORITY.md D-023 决定 |
| 同 seed、同输入、同 tick 的 checksum 与回放结果逐位一致 | `[已验证-文档]` AGENTS.md 第 7 节 |
| `Running → Stopping → Stopped` 十一阶段有序关闭 | `[已验证-文档]` AGENTS.md 6.1 |
| Host cadence 所有权不移入 worker：F1 暂停、paused-only F2 单步、F5 cadence 切换、debt 上限、输入提交时点仍由 Unity Host 拥有 | `[已验证-文档+代码]` AGENTS.md 6 节；`SimulationTickDriver`（见 F2） |

### 1.3 术语澄清（评审前必读）

1. **Stats FPS ≠ 逻辑 cadence。** Stats 统计 Unity 主循环每秒完成次数；
   逻辑按精确 33 ms cadence 推进。120 FPS 目标是渲染帧率，不是提速逻辑。
2. **"逻辑帧不变"贯穿全计划。** Step B（worker 化）只改变 tick 的执行线程与
   流水线组织，不改变 cadence 数值、tick 内容、pass 顺序或结果；Step C 只
   降低同一 tick 的计算成本。两者的硬验收都是 1.2 的逐位一致。
3. **publication 与渲染帧的节律是近似值。** 120 FPS × 33 ms ≈ 3.96，即
   "约每 4 渲染帧出现一次新 publication"仅是近似描述；长序列不得假设严格
   等间隔，验收以实测 publication 间隔为准。
4. **插值 ≠ 提帧率。** 插值层解决"33 ms publication 在 120 Hz 显示上的运动
   平滑"，属呈现正确性；Stats/帧时间由主线程每帧成本决定。
5. **垂直同步与显示环境是验收前置条件**，不是性能项，但必须按 D5 锁定，
   否则帧率数字无意义。

### 1.4 范围外（明确排除）

- **模拟 LOD / 降频 / 分帧摊**：违反复刻合同，永久排除（除非获得权威证据）；
- **将 Host cadence 所有权移入 worker**（自驱节奏）：这会重新定义 F1/F2/F5/
  debt/输入冻结语义，属 Host 行为变更，不在本计划内实施（见 Step B 边界）；
- 移动端专项（AGENTS.md 第 14 节延长线）是否单列见 D3；
- 网络、回滚 netcode、资源格式重构、UI 改版。

## 2. 当前事实基线（R1 修订）

### 2.1 已验证事实

| 编号 | 事实 | 证据 |
|---|---|---|
| F1 | 正常 cadence 精确 33 ms；F5 快速 3 ms；debt 最多 2 个当前 cadence interval | `[已验证-文档]` AGENTS.md 第 6 节 |
| F2 | **Host 拥有 cadence 与策略，worker 条件性执行**：cadence 累计、pause/F1、单步 F2、F5 切换、input gate、显式 nextTickIndex 选择均由 Unity Host Update 驱动；存在默认开启的 dedicated simulation worker（`useDedicatedSimulationWorker = true`）——资格满足时 Host 提交显式 tick → worker 执行 → 主线程消费 publication → acknowledgment → 才允许下一次提交（单飞）；资格不满足或不可用时回退当前线程同步执行 | `[已验证-代码]` `Simulation/Host/SimulationTickDriver.cs`：201（`useDedicatedSimulationWorker = true` 及 tooltip）、392/470（`CanAdvanceTick`）、1264（`ResolveDedicatedSimulationWorkerIneligibilityReason`） |
| F2b | worker 资格拒绝条件包括：非 CentralOnly、非 LocalFreeRun、input-ready gate、full-frame snapshot、RuntimeDataCatalog 未准备、World 仍有 Unity presentation binding | `[已验证-代码]` `ResolveDedicatedSimulationWorkerIneligibilityReason`（SimulationTickDriver.cs:1264） |
| F2c | **未知项**：目标生产场景中 worker 是否持续 active；每 tick 实际执行线程分布；主线程/worker 重叠量；publication/ack 等待成本；因资格或背压回退同步路径的比例 | `[推演-待测]`——M0 第一组测量项（4.0） |
| F3 | 中央渲染系统对同一 publication 的重复显示帧去重：publication version 未变化时跳过重建，仅重复提交既有 submission | `[已验证-代码]` `BattleCentralRenderSystem.MaterializeLatestPublishedFrame` |
| F4 | 渲染提交公式：draw = 底部覆盖层（可选）+ 脚印批（可选）+ **每个活动中央 segment 一次** + 血条批（可选）；`SegmentCount` 无代码上限 | `[已验证-代码]` `BattleRenderFeature.BattleRenderPass.Execute` 逐 segment `DrawMesh` 循环。"典型场景个位数"仅为特定场景推演，具体由资源 bank、page、材质、binding mode、chunk 与可见命令决定 → `[推演-待测]` |
| F5 | 表现捕获按模式分工：**所有模式**扫描表现实体、维护 handle cache、捕获命中记录；**非 CentralOnly** 额外执行 legacy Z sort 及诊断；**CentralOnly 跳过该 legacy sort**，最终排序由 `MaterializePresentationOrder`（稳定槽位基数排序）承担，其成本单独计量 | `[已验证-代码]` `BattlePresentationShadowBuild.BeginFrameCore`（CentralOnly 分支跳过 `SortEntitiesByZPreservingSlotOrder`）、`MaterializePresentationOrder` |
| F6 | 表现侧 worker 发布缝已存在：`BattlePresentationCoordinator.BeginSimulationWorkerFrame` 支持由 simulation worker 在 CentralOnly 模式发布逻辑快照 | `[已验证-代码]` `BattlePresentationShadowBuild.cs` |
| F7 | 中央物化对**每个新 publication 最多执行一次**（受 Unity frame、publication version、物化锁三重去重与相机/render pass 调用时机影响）；无新 publication 时复用现有 submission。实际物化次数可能低于 tick 次数 | `[已验证-代码]` `BattleCentralRenderSystem.PrepareFrameImmediate` 去重逻辑；实测计数列入 M0 |
| F8 | 边界重构计划 `SIMULATION-MONO-BOUNDARY-REFACTOR-001` 存在（USER_HOLD）；其 B6（tick publication 与 Unity dispatch 分离）是 Step B 深化的前置；第 19 节含外部评审附录与待批正文修正清单（P-1…P-5） | `[已验证-文档]` 该计划文档 |
| F9 | Core/Runtime 仍存在线程相关 Unity 静态调用；`Time/Input/Random/Resources/Application/SystemInfo/Object.Instantiate/Destroy` 主线程限定，worker 准入前必须移除或隔离；`Debug` 不一定因线程调用立刻崩溃，但属于 Core→Unity 服务依赖并可能引入日志锁/字符串分配/不可控 IO——生产 worker 准入前必须改走预分配 diagnostics sink 或证明不可达（热路径 `Debug` = 0；L1 完成定义 = Core 对 `UnityEngine.Debug` 依赖为 0，与边界计划 7.5/9.9/13.6 一致） | `[已验证-代码]` `Simulation/Runtime/SimulationRegistryModule.cs` 656/669/737/752/773/787 行 |
| F10 | 物化/装载的资源侧现状与治理见姊妹文档 `BATTLE-ATLAS-MEMORY-LOWEND-ROADMAP-001`；两计划共享 M0 指纹、资源 Manifest 与中央渲染计数（segment/draw/GPU/上传统计），最终设备认证联合执行 | `[已验证-文档]` ATLAS 计划 5.3 |

### 2.2 观察与模型（`[推演-待测]`）

| 编号 | 内容 | 状态 |
|---|---|---|
| O1 | 用户报告：1000 实体对战 Stats 约 30，60/120 不可达 | M0 复核并量化 |
| O2 | 瓶颈模型（R1 重写）：**分叉取决于 F2c**——(a) 若 worker 持续 active：主线程 tick 帧成本主要是 publication 等待/ack 背压与物化，真实瓶颈待测；(b) 若 worker 不 active 或频繁回退：tick 同步在主线程执行（推演 ~30 ms），主循环被钉在约 33 ms 周期。两种分支的判定与数值均由 M0 裁定 | 待 M0 证实或推翻；若推翻则按第 6 节修正路线 |

### 2.3 数据缺口（M0 输入）

- F2c 全部未知项（worker active 率、回退率、in-flight、ack latency、
  publication age）；
- 单 tick 各 pass 毫秒分布（模拟 pass 侧 + 表现捕获侧）；
- 中央物化单次成本（1000 实体口径）与物化/提交复用计数；
- 主线程非战斗底噪（URP+UI+引擎）在目标环境；
- GC 分配 1000 实体口径现状；
- `SegmentCount`/draw 的真实分布（与 ATLAS 计划共享统计）。

## 3. 路线框架（R1：结论条件化）

### 3.1 帧预算拆分（Stats 120 = 每帧 8.33 ms）

| 场景 | tick 帧主线程成本构成 | 推演结论 |
|---|---|---|
| (a) worker 持续 active | 消费 publication + 物化（每新 publication）+ 渲染 + 底噪 + ack/背压等待（量值未知） | 可行性取决于 (a1) 物化尖峰与 (a2) ack/背压延迟的实测值 |
| (b) worker 回退/不 active | 整个 tick 同步执行 + 捕获 + 物化 + 渲染 | 推演 ~30 ms ≫ 8.33 ms，数学上不可行 |

### 3.2 条件化结论（取代 R0 的"必选"表述）

1. **若 M0 证实 (a) 且物化/等待成本可压入预算**：Step B 性质 = "完善现有
   worker 的生产资格、消除资格拒绝项、按 D6 重估单飞/背压策略"——工作量
   从"新建"降为"完善"；
2. **若 M0 证实 (b) 或 worker 频繁回退**：Step B = 使 worker 达到生产资格
   （含 5.1 准入级线程相关 API 清理），此时它是 120 目标的必经路径；
3. **路线 C（tick ≤25 ms）在任何分支都是 worker cadence 稳定性的配套**：
   30 ms 级 tick 即使在 worker 上，利用率 >90% 也无抖动吸收空间；
4. **路线 A 承担主线程残馀预算**：物化尖峰（每新 publication 一次，F7）与
   插值缺失（120 Hz 上每 publication 重复约 4 次）仍是两处已知呈现问题。

## 4. 路线图（四步；每步独立立项、独立验收、独立回滚）

```text
Step 0 测量（M0）──┬──> Step A 表现侧优化（不改模拟 state/checksum）
                   └──> Step C 热点清单（数据驱动）
边界重构 B0-B6 + worker 准入清理（前置，USER_HOLD，见 5.1）
                   └──> Step B worker 生产资格与流水线解耦 ──> Step C 收尾 ──> 联合验收
```

### 4.0 Step 0：测量（M0）

**第一优先组——worker 现状取证（R1 新增）**：

- worker active/eligible 比例、资格拒绝原因分布（F2b 各项）、回退同步执行
  比例；
- tick in-flight 时长、ack/publication 等待延迟、publication age；
- `CanAdvanceTick` 单飞背压实际阻塞频率与时长；
- 每 tick 实际执行线程归属。

**第二组——成本分布**：

- 单 tick 各阶段毫秒分布（模拟 pass 侧 + 表现捕获侧；CentralOnly 口径按
  F5 的分工计量，不含已跳过的 legacy sort）；
- 中央物化单次成本与物化/提交复用计数（F7）；
- `SegmentCount`/draw 分布（与 ATLAS 计划共享）；
- 主线程底噪（URP+UI+引擎，目标环境实测）；
- GC 分配现状。

**场景与口径**：`ProductionEntityStressHarness` 1000 实体配置 + 普通规模
对照组；采集使用既有 `BattleTickDetailPhaseDiagnostics` 阶段枚举与
`NTSD.BattlePresentation.*` Profiler 标记，首选零代码改动；若需补埋点，
单独立 Change Record。环境锁定按 D5。

**口径与执行约束（R2）**：

- 每份 M0 结果必须标注 `ContentAuthorityState`：
  `PRE_D023_MIGRATION_BASELINE` 或 `D023_FORMAL_CONTENT`。当前正式资源
  迁移尚未完成，首轮结果属前者，**非最终性能证书**；D-023 迁移完成后必须
  重跑全部口径（1000 实体构成、表现 command 数、atlas/segment、碰撞/
  OPoint 路径、内存、渲染与加载）并升级标签；只有 `D023_FORMAL_CONTENT`
  结果可用于冻结预算与验收阈值；
- 标注 `UnityTestJobConflict` 状态。PERF D1 已获方案批准，但**实际
  Profiler/Unity 测量启动必须与当前活跃恢复任务协调**：`CURRENT-AUTHORITY.md`
  记录的回归 job `aa6b0c9f033e4b8b83174826936e782c` 已有终态（FAILED，
  58/24，XML 已归档），其后继活跃任务为 `NTSD28-Q06`（276 条真实首差待
  处理）——测量排程不得干扰该任务的验证管线，启动时点由用户与该任务协调；
- 若需补埋点，先建立最小独立 Change Record，不混入性能优化。

**产出**：M0 报告——对 3.2 的分支 (a)/(b) 给出判定，并对每条条件化结论
回填实测值。

### 4.1 Step A：表现侧优化（不修改模拟 state/checksum 的表现路径）

定位（R1 措辞修正）：不触碰模拟状态与 checksum；但 presentation 侧仍有
first-visible tick、submission generation、stale lease、排序、shutdown 等
合同，须逐项验收，不是"无风险"。

| 子项 | 内容 | 验收 |
|---|---|---|
| A1 物化降本 | 未变化 chunk 跳过重传、quad 脏区更新、批量上传 | 1000 实体单次物化耗时较 M0 基线下降（目标值 M0 后定） |
| A2 物化出主线程 | **独立 Presentation 侧 MeshData job**：主线程分配/调度 → job 只写 NativeArray/MeshData → 主线程 fence 后 ApplyAndDispose。不复用 dedicated simulation worker（避免所有权混合与 tick deadline 争用）；并实测 `AllocateWritableMeshData` 开销是否优于现有持久 Mesh + `SetVertexBufferData`，不优则不采用 | tick 帧主线程不再包含物化；提交正确性 focused test 不变 |
| A3 插值层 | 按权威 `presentation_interpolation.cpp` 语义实现（上一 publication 与当前 publication 之间按显示时间插值）——对齐权威已有呈现机制。作为独立批次（见 D4） | 插值轨迹与权威语义对照一致；presentation latency/publication age 有定义且达标 |

### 4.2 Step B：完成现有 dedicated worker 的生产资格与流水线解耦（R1 重写）

**语义边界（首要）**：

- Host 继续拥有 cadence 累计、pause/F1、单步 F2、F5 切换、debt 上限、输入
  提交时点，并向 worker 提交**显式 tick**；
- worker 只执行 Host 提交的 tick，**不自驱节奏**；
- 不在本计划内重定义 F1/F2/F5/debt 语义；将来若需 worker 拥有 cadence，
  另立 Host cadence 行为变更任务并重新做权威对照。

**前置条件**：

1. 边界重构 B0–B6 完成（含其第 19.7 节待批修正 P-1…P-5 落地）；
2. **worker 准入级 API 清理完成（R2 与 F9 统一口径）**：`Time/Input/Random/
   Resources/Application/SystemInfo/Object.Instantiate/Destroy` 等主线程
   限定或进程环境型调用从 worker 路径移除或隔离；`Debug` 为**硬准入门**：
   生产 worker 热路径调用必须为 0（预分配 diagnostics sink 或证明不可达；
   L1 完成定义 = Core 对 `UnityEngine.Debug` 依赖为 0，见边界计划
   7.5/9.9/13.6）；`Vector2/Vector3/Mathf` 纯值可留 L2（对应边界计划 B7
   范围收窄，见其 19.7-P3）；
3. M0 完成，分支 (a)/(b) 判定与成本已知。

**实施内容（完善而非新建）**：

- 消除 F2b 资格拒绝项中可消除者（如 presentation binding 解耦依赖 B4/B6）；
- 按 D6 决定是否放宽 `CanAdvanceTick` 单飞/ack 背压（涉及 presentation
  latency 合同）；
- 保持十一阶段关闭中 stop/join 语义；输入以 `FrameInputSet` 纯值递交；
  Unity 对象永不上 worker。

**硬验收**：

- 同 seed 同输入，改造前后 checksum 与回放逐位一致（first difference 即停）；
- 压力场 Stats ≥110 持续（120 的最终验收在 C 后联合进行，按 1.1 口径）；
- 长跑无 cadence slip 积累；worker 回退率与 ack 延迟达标（阈值 D6 定）。

### 4.3 Step C：热点优化（数据驱动，B 之后收尾）

- 清单来源：M0 实测（先验候选：碰撞候选消费、交互流水线、状态机 pass；
  表现捕获若已随 worker 路径移出主线程则重新归类）；
- 单线程 Burst 只是候选优化，**不自动获得确定性资格**（R2：单线程只保证
  调用顺序易于保持，不保证浮点逐位一致——Burst 浮点模式/精度影响运算重排、
  FMA、倒数替换、NaN/Inf、正负零与数学函数精度；`FloatMode.Fast` 明确允许
  改变结果的代数优化，`FloatMode.Deterministic` 尚非完整跨平台保证）。每个
  Burst kernel 必须：固定 Burst 版本/`FloatMode`/`FloatPrecision`/编译
  选项；禁止默认 `FloatMode.Fast`；与 managed canonical 路径逐字段/逐位
  shadow compare；分别在 x86_64 与 Android ARM64 验证；任一 first
  difference 即不得晋升生产，该 kernel 保持 managed/canonical；
- 并行化仅对"逐实体独立可证"的 pass 考虑且汇聚顺序固定，每项独立确定性
  论证，排最后；
- 验收：worker 上 tick 稳定 ≤25 ms；10 分钟 soak 零 cadence slip；
- 联合验收：按 1.1 口径（Player、锁定环境、双指标、附加延迟指标）达成
  120 判定。

## 5. 与既有计划/合同的关系

### 5.1 边界重构计划（前置）

Step B 前置 = B0–B6 完成 + 19.7 节 P-1…P-5 修正落地 + worker 准入级 API
清理。解冻由用户单独批准（D2，建议分阶段：先 B0）。

### 5.2 不触碰

十一阶段关闭、cadence 契约与输入延迟语义、Host cadence 所有权、D-023
内容权威切换事务、`Gen/`、`Plugins/`。

### 5.3 与 ATLAS 计划的关系（R1 修正措辞）

两计划**可独立实施**，但**非零交点**：分册策略影响 `SegmentCount`/draw
（F4），压缩格式影响 GPU 带宽，纹理上传影响主线程/render thread，切场缓存
影响内存与帧尖峰。因此：共享 M0 指纹、资源 Manifest 与中央渲染计数；最终
设备认证联合执行。

## 6. 风险与回滚

| 风险 | 表现 | 控制 |
|---|---|---|
| M0 判定分支 (b)（worker 不 active） | Step B 恢复"必经路径"量级 | 如实报告排期影响；Step A/C 先行交付部分收益 |
| worker 资格拒绝项无法全部消除 | worker 频繁回退同步 | M0 拒绝原因分布先行；逐项消除或如实降级目标 |
| ack/单飞背压延迟超标 | 主线程等待拖帧 | D6 决策与阈值；实测驱动放宽与否 |
| 插值与权威呈现节奏不一致 | 平滑但轨迹不符 | A3 以 `presentation_interpolation.cpp` 对照验收，first difference 即停 |
| 物化尖峰压不进预算 | 周期性 >8.33 ms 帧 | A1→A2 递进；A2 后仍超则重估 120 可行性并如实报告 |
| 引擎底噪占比过高 | 战斗侧再快也到不了 120 | M0 实测；超预算报告为环境约束（D5 环境下） |
| 性能改造引入分配 | 0GC 合同破坏 | 每子项 allocation 计数对比门禁 |
| Editor 数字与 Player 不一致 | 误报达成 | 1.1 口径强制 Player 验收，Stats 仅辅助 |

回滚单位：每子项独立 Change；Step B 回退 = 回 Host 同步/既有 worker 提交
路径（端口保留），不影响 A 已交付成果。

## 7. 完成定义

仅当以下全部满足才可报告"1000 实体 Stats 120 达成"：

1. 第 1.1 节判定通过（Player、锁定环境、10 分钟 soak、端到端 presented
   frame p99 ≤ 8.33 ms 硬门、三线程/GPU 归因无持续超预算、附加延迟指标
   达标、无周期性尖峰形态）；
2. 逻辑契约逐项复核不变（1.2 全表，含 D-023 与 Host cadence 所有权）；
3. checksum/回放与基线逐位一致；
4. 十一阶段关闭通过（含 worker stop/join）；
5. `BattleRuntimeSelfCheck` 与完整 EditMode 通过；
6. 各步 Change Record 状态链完整；
7. 最终判定基于 `D023_FORMAL_CONTENT` 口径的 M0/soak 证据（若达成时点早于
   D-023 迁移完成，迁移后必须重验并如实标注两轮结果）。

只完成部分步骤只能报告阶段性状态，不得宣称达成。

## 8. 开放决策清单

| 编号 | 决策 | 建议（源自 GPT6 评审，待用户批准） |
|---|---|---|
| D1 | 是否批准 M0 | **已批准（R2，2026-09-14）**；首选零代码测量，且**必须包含 worker 现状取证第一优先组**；结果标注 `ContentAuthorityState` 与 `UnityTestJobConflict`（见 4.0）；缺埋点另立最小 Change；实际测量启动与活跃 NTSD28-Q06 恢复任务协调 |
| D2 | 是否解冻边界重构 | **分阶段批准**：先 B0（inventory/guards）；B1/B2 待本计划正文修正（19.7 P-1…P-5）落实后推进；B3–B6 每批独立；与活跃 NTSD 对齐工作共用文件时等稳定边界 |
| D3 | 平台口径 | **桌面参考 + Android 分档单列**：桌面 Player 保留 1000 实体 120 工程目标；Android 120 只对具名高刷新设备档认证；中低端门槛由 M0 数据单定，不从桌面自动继承 |
| D4 | 插值层是否纳入 | 纳入，但作为独立批次：先做权威 trace 与 presentation latency 合同，再实施 A3 |
| D5（新增） | 正式验收环境锁定：Player/设备/分辨率/render scale/VSync/API/热状态 | M0 前锁定；建议先桌面参考环境 |
| D6（新增） | 允许的 input-to-visible latency、最大 publication age、worker queue 深度、ack 背压策略 | M0 实测后定阈值；决定 Step B 是否放宽单飞 |

## 9. 明确不做的事

1. 不修改逻辑 cadence、pass 顺序、RNG、输入语义（1.2 全表）；
2. 不做模拟 LOD / 降频 / 分帧摊；
3. **不将 Host cadence 所有权移入 worker，不在本计划内重定义 F1/F2/F5/debt
   语义**；
4. 不做运行时动态图集（见 ATLAS 计划排除项）；
5. 不在未实测前优化任何 pass；
6. 不顺手做资源格式重构、渲染管线更换、UI 改版；
7. 不使用破坏性 Git 操作。

## 10. 恢复方式

保持 `DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`。批准启动时：

1. 先读 `docs/ai/CURRENT-AUTHORITY.md`（含 D-023）与当前 `AGENTS.md`；
2. 按第 8 节 D1→D5→D6→D2→D3→D4 顺序逐项确认；
3. 立项时重新扫描代码现状，不假设本文引用的文件行号仍有效；
4. 文件被重命名/移动/拆分时同步更新索引文档与关联引用。

## 11. 修订记录

| 版本 | 日期 | 内容 |
|---|---|---|
| R0 | 2026-09-13 | 初版 |
| R1 | 2026-09-14 | 按 GPT6 综合评审修正：①内容权威 Direction B → D-023（阻断级 1）；②cadence 统一精确 33 ms 表述，禁用 30 Hz 措辞（阻断级 2）；③F2 重写为"Host 调度 + 条件性 dedicated worker 执行"（阻断级 3，代码已默认启用 worker）；④Step B 重写为"完善现有 worker 生产资格"，Host 保留 cadence 所有权，删除 worker 自驱表述（阻断级 4）；⑤"路线 B 必选"降为 M0 后条件结论；⑥F4 draw 公式化、取消"个位数"代码声称；⑦F5 补 CentralOnly 跳过 legacy sort；⑧F8 改"每新 publication 最多一次"；⑨M0 新增 worker 现状取证第一优先组；⑩A2 改独立 Presentation job；⑪验收 Player 化 + 附加延迟指标；⑫新增 D5/D6；⑬与 ATLAS 关系改"可独立实施 + 联合认证"；⑭新增不变量"Host cadence 所有权不移入 worker" |
| R2 | 2026-09-14 | 按 GPT6 R1 复核修正（有条件通过 → 条件补齐）：①1.1 最终硬门改为端到端 presented frame interval p99 ≤ 8.33 ms，主线程/render thread/GPU 降为归因指标（任一阶段不得持续超 8.33 ms），新增 missed present/最长连续超预算帧（高）；②Step C 新增 Burst 逐位一致性合同：单线程不自动获得确定性资格，逐 kernel 固定版本/FloatMode/FloatPrecision + x86_64/ARM64 双架构 shadow compare，first difference 即保持 managed（高）；③F9 与 Step B 的 Debug 口径统一为硬准入门（热路径 0 / L1 Core 依赖 0），与边界计划 7.5/9.9/13.6 对齐（中）；④M0 新增 ContentAuthorityState（PRE_D023_MIGRATION_BASELINE / D023_FORMAL_CONTENT）与 UnityTestJobConflict 标注，D-023 迁移后重跑（中）；⑤完成定义补 M0 标签重验项；⑥D1 标记已批准，实际测量启动与 NTSD28-Q06 协调 |
