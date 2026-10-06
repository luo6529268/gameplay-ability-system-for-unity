# 战斗性能全链路路线图：1000 实体 × Stats 120 渲染帧

> 计划标识：`BATTLE-PERF-STATS120-ROADMAP-001`
>
> 创建日期：2026-09-13；本版：R3（2026-10-06，当前代码基线整理；保留R1/R2历史）
>
> 当前状态：`DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`
>
> 该状态指本专项实施批次，不能解读为相关代码从未变化；插值等已有其他任务实现。
> 本轮仅文档整理，M0/代码优化仍 `WAITING_USER_APPROVAL`。
> [统一优先级与启动门](battle-optimization-rebaseline-and-start-gates-20261006.md)；
> [主登记表](android-mobile-readiness-priority-risk-register.md)。
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
   平滑"，属呈现正确性；实际presented帧时间受主线程/render thread/GPU/显示流水线共同影响。
5. **垂直同步与显示环境是验收前置条件**，不是性能项，但必须按 D5 锁定，
   否则帧率数字无意义。

### 1.4 范围外（明确排除）

- **模拟 LOD / 降频 / 分帧摊**：违反复刻合同，永久排除（除非获得权威证据）；
- **将 Host cadence 所有权移入 worker**（自驱节奏）：这会重新定义 F1/F2/F5/
  debt/输入冻结语义，属 Host 行为变更，不在本计划内实施（见 Step B 边界）；
- 移动端专项（AGENTS.md 第 14 节延长线）是否单列见 D3；
- 网络、回滚 netcode、资源格式重构、UI 改版。

## 2. 当前事实基线（R3，2026-10-06静态重扫）

### 2.1 已验证事实

| 编号 | 事实 | 证据 |
|---|---|---|
| F1 | 正常 cadence 精确 33 ms；F5 快速 3 ms；debt 最多 2 个当前 cadence interval | `[已验证-文档]` AGENTS.md 第 6 节 |
| F2 | Host拥有cadence/输入策略，默认存在条件性dedicated worker；显式tick/单飞/ack组织不等于实际active，更不等于1000 AI并行化 | `[已验证-代码]` `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs:207`；实际资格/吞吐见F2c |
| F2b | 资格拒绝包括非CentralOnly/LocalFreeRun、input-ready、full-frame snapshot、legacy stage refresh、catalog未就绪及Unity presentation bindings | `[已验证-代码]` `SimulationTickDriver.cs:1516-1537`，本轮重扫 |
| F2c | **未知项**：目标生产场景中 worker 是否持续 active；每 tick 实际执行线程分布；主线程/worker 重叠量；publication/ack 等待成本；因资格或背压回退同步路径的比例 | `[推演-待测]`——M0 第一组测量项（4.0） |
| F3 | 中央物化按Unity frame及publication+displayAlpha去重：无新publication但插值alpha变化仍可重建；仅相同取样复用submission | `[已验证-代码]` `Animation/Rendering/BattleCentralRenderSystem.cs:397-406,475`，本轮重扫 |
| F4 | 中央CPU命令公式=可选overlay/foot+每个有效物理segment一次DrawMesh+可选health。逻辑run≠segment，chunk/StrictOrderedDraw/实际资源绑定会分段；CPU命令≠真实GPU batch | `[已验证-代码]` `BattleRenderFeature.cs:272-282,316`、`BattleDynamicMeshBackend.cs:213-224,429-440`；RenderPass/Execute/benchmark-local/Profiler/GPU分别记数，不预设典型个位数 |
| F5 | 当前中央物化仍调用MaterializePresentationOrder；排序边界不改变。9月捕获/legacy sort内部描述仅为历史证据，当前排序内部、first-visible及透明重叠本轮未新读活跃Q06方法体，维持待确认 | `[已验证-调用点/内部待确认]` 当前CentralRenderSystem调用；不得把旧基数排序细节当最新全域合同 |
| F6 | 表现侧 worker 发布缝已存在：`BattlePresentationCoordinator.BeginSimulationWorkerFrame` 支持由 simulation worker 在 CentralOnly 模式发布逻辑快照 | `[已验证-代码]` `BattlePresentationShadowBuild.cs` |
| F7 | 显示插值代码已存在：同publication的不同alpha可多次物化，并调用DisplayMotion及backend Build；当前Upload仍上传活动顶点。A1 dirty-chunk跳过是待实施设计 | `[已验证-代码]` `BattleCentralRenderSystem.cs:624-629,668-682`、`BattleDynamicMeshBackend.cs:663-686`；每publication/显示帧计数都列M0 |
| F8 | MONO代码仍USER_HOLD，B0-B6/worker准入按StepB前置；P-1…P-5文档正文已于9/14落地，不再是待落地清单，不能据此称代码已重构 | `[已验证-文档]` MONO §19.7.5及2026-10-06补充 |
| F9 | Core/Runtime 仍存在线程相关 Unity 静态调用；`Time/Input/Random/Resources/Application/SystemInfo/Object.Instantiate/Destroy` 主线程限定，worker 准入前必须移除或隔离；`Debug` 不一定因线程调用立刻崩溃，但属于 Core→Unity 服务依赖并可能引入日志锁/字符串分配/不可控 IO——生产 worker 准入前必须改走预分配 diagnostics sink 或证明不可达（热路径 `Debug` = 0；L1 完成定义 = Core 对 `UnityEngine.Debug` 依赖为 0，与边界计划 7.5/9.9/13.6 一致） | `[已验证-代码]` `Simulation/Runtime/SimulationRegistryModule.cs` 656/669/737/752/773/787 行 |
| F10 | 物化/装载的资源侧现状与治理见姊妹文档 `BATTLE-ATLAS-MEMORY-LOWEND-ROADMAP-001`；两计划共享 M0 指纹、资源 Manifest 与中央渲染计数（segment/draw/GPU/上传统计），最终设备认证联合执行 | `[已验证-文档]` ATLAS 计划 1.4 与第 5 节 |

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
- 加载/steady/transition总内存：源纹理、staging、音频原clip/PCM副本、双backend及读Lease；
  H-10约210MiB仅已有静态估算。H-11插值/描述数组容量与热路径增长也需单独测。

## 3. 路线框架（R1：结论条件化）

### 3.1 帧预算拆分（Stats 120 = 每帧 8.33 ms）

| 场景 | tick 帧主线程成本构成 | 推演结论 |
|---|---|---|
| (a) worker 持续 active | 消费publication + 每有效显示取样物化/插值/上传 + 渲染/声音/底噪 + ack/背压（量值未知） | 可行性取决于每显示帧成本与等待；不能用每publication仅一次低估 |
| (b) worker 回退/不 active | 整个 tick 同步执行 + 捕获 + 物化 + 渲染 | 推演 ~30 ms ≫ 8.33 ms，数学上不可行 |

### 3.2 条件化结论（取代 R0 的"必选"表述）

1. **若 M0 证实 (a) 且物化/等待成本可压入预算**：Step B 性质 = "完善现有
   worker 的生产资格、消除资格拒绝项、按 D6 重估单飞/背压策略"——工作量
   从"新建"降为"完善"；
2. **若 M0 证实 (b) 或 worker 频繁回退**：Step B = 使 worker 达到生产资格
   （含 5.1 准入级线程相关 API 清理），此时它是 120 目标的必经路径；
3. **路线 C（tick ≤25 ms）在任何分支都是 worker cadence 稳定性的配套**：
   30 ms 级 tick 即使在 worker 上，利用率 >90% 也无抖动吸收空间；
4. **路线A承担主线程残余预算**：现有插值下的物化/上传频率及轨迹/latency验收
   需要重基线。A3不从零重写，A1须区分不可变资源数据与随alpha变化的位置等数据。

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

- 单 tick 各阶段毫秒分布（模拟 pass 侧 + 表现捕获侧；CentralOnly 的捕获/排序
  真实路径在获批启动时按 F5 确认，不把本轮未读的排序内部当成已闭合合同）；
- 中央物化次数按publication变化/alpha变化/完全重复取样区分；单次与每显示帧成本并列；
- 逻辑run、物理segment、中央DrawMesh、生产RenderPass/ExecuteCommandBuffer、
  benchmark-local调用、全帧Profiler draw及GPU batch/SetPass分别统计；不由CPU命令推GPU；
- 主线程底噪（URP+UI+引擎，目标环境实测）；
- 完整物化—上传—录制—提交GC/增长、声音聚合及加载/steady/transition内存；
  新统计若现有计数无法推导，必须另批批准最小埋点，不在本文执行。

**场景与口径**：`ProductionEntityStressHarness` 1000 实体配置 + 普通规模
对照组；采集使用既有 `BattleTickDetailPhaseDiagnostics` 阶段枚举与
`NTSD.BattlePresentation.*` Profiler 标记，首选零代码改动；若需补埋点，
单独立 Change Record。环境锁定按 D5。

**口径与执行约束（R2）**：

- 每份 M0 结果必须标注 `ContentAuthorityState`：
  `PRE_D023_MIGRATION_BASELINE`或`D023_FORMAL_CONTENT`。当前已接入LoganRuntime，
  必须冻结实际输入Manifest/项目模式/例外/decoder身份，不能照抄9月“迁移未完成”状态；
  旧内容只作tooling参考，最终预算与证书仅用正式口径。代码/内容/Kernel/插值或音频
  范围变化按受影响域重测，不自动重跑全部历史campaign；
- 标注 `UnityTestJobConflict` 状态。PERF D1 已获方案批准，但**实际
  Profiler/Unity启动须用户本轮后续批准并协调当时共享Editor/活跃路径**。
  9月Q06/job信息只为历史，不自动恢复旧任务。当前测量状态WAITING_USER_APPROVAL；
  不启动EXT-1专项M0；
- 若需补埋点，先建立最小独立 Change Record，不混入性能优化。

**产出**：M0 报告——对 3.2 的分支 (a)/(b) 给出判定，并对每条条件化结论
回填实测值。

### 4.1 Step A：表现侧优化（不修改模拟 state/checksum 的表现路径）

定位（R1 措辞修正）：不触碰模拟状态与 checksum；但 presentation 侧仍有
first-visible tick、submission generation、stale lease、排序、shutdown 等
合同，须逐项验收，不是"无风险"。

| 子项 | 内容 | 验收 |
|---|---|---|
| A1物化降本（待实施） | 区分资源/UV/拓扑缓存与alpha位置脏区，评估chunk/字段更新；先补H-11容量，不把现状全活动顶点上传说成已dirty-skip | 同workload每显示帧/取样成本下降，0GC/UV/排序保持 |
| A2输出/调度（待选择） | 保持独立Presentation job、不复用simulation worker；A3合同先冻结，再决定是否另批选择A4候选，最后定顶点或实例输出。未批A4仍比较MeshData与持久Mesh，收益不成立不采用 | 剩余主线程物化/调度/fence/Apply各自量化，不承诺主线程成本全消失 |
| A3现有插值复核 | 复用现有DisplayMotion/alpha；按明确冻结的正式来源验插值取样、排序、first-visible、latency与publication age，未确认域另列 | checksum不变、轨迹/断点/年龄语义有证据，不能以代码存在宣称全域通过 |

### 4.2 Step B：完成现有 dedicated worker 的生产资格与流水线解耦（R1 重写）

**语义边界（首要）**：

- Host 继续拥有 cadence 累计、pause/F1、单步 F2、F5 切换、debt 上限、输入
  提交时点，并向 worker 提交**显式 tick**；
- worker 只执行 Host 提交的 tick，**不自驱节奏**；
- 不在本计划内重定义 F1/F2/F5/debt 语义；将来若需 worker 拥有 cadence，
  另立 Host cadence 行为变更任务并重新做权威对照。

**前置条件**：

1. 边界重构B0-B6达到对应退出门；P-1…P-5文档已落地，实际端口/epoch/guards等代码仍须逐批实现；
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

StepB深化前置=B0-B6对应代码退出门+worker准入API清理；§19.7的P正文已改，
不能重复列为未完成文档。解冻由用户单批批准，先B0；不阻断其他范围独立的容量/资源取证。

### 5.2 不触碰

十一阶段关闭、cadence 契约与输入延迟语义、Host cadence 所有权、D-023
内容权威切换事务、`Gen/`、`Plugins/`。

### 5.3 与 ATLAS 计划的关系（R1 修正措辞）

两计划**可独立实施**，但**非零交点**：分册策略影响 `SegmentCount`/draw
（F4），压缩格式影响 GPU 带宽，纹理上传影响主线程/render thread，切场缓存
影响内存与帧尖峰。因此：共享 M0 指纹、资源 Manifest 与中央渲染计数；最终
设备认证联合执行。

### 5.4 本轮新增交点与候选边界

H-10音频驻留/M-13聚合、H-11表现缓存0GC、M-12背压/M-14校验、
M-15 Kernel指纹加入统一34项登记，各自方案/验收在独立文档。
EXT-1保持PROPOSED / MODIFY_REQUIRED，不入正式StepA、不实施或专项M0；
A3→选择是否A4→定型A2是表示设计顺序，不是A4已获批。

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
| D1 | 是否批准M0 | 2026-09-14历史方案批准保留；当前启动WAITING_USER_APPROVAL，含worker/插值/内存/0GC，缺埋点另批Change，协调当时Editor；不启动EXT-1专项M0 |
| D2 | 是否解冻边界 | 建议先B0；P正文已落地，B1-B6代码仍逐批批准，重新inventory及共享路径窗口，不自动解冻 |
| D3 | 平台口径 | **桌面参考 + Android 分档单列**：桌面 Player 保留 1000 实体 120 工程目标；Android 120 只对具名高刷新设备档认证；中低端门槛由 M0 数据单定，不从桌面自动继承 |
| D4 | 插值层是否纳入 | 已有代码，纳入现状复核而非从零实施；先闭合冻结来源/取样/排序/first-visible/latency，再选择候选表示和A2输出 |
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
| R3 | 2026-10-06 | 仅文档：F3/F7和A3按现有插值重基线，F4计数分离/F5内部待确认，补全热路径0GC与音频/内存/Kernel；P正文已落地状态同步，M0/实施仍待用户再批准，EXT-1不升格。 |
