# 战斗性能全链路路线图：1000 实体 × Stats 120 渲染帧

> 计划标识：`BATTLE-PERF-STATS120-ROADMAP-001`
>
> 创建日期：2026-09-13
>
> 当前状态：`DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`
>
> 性质：性能路线方案与未来实施框架。本文件创建时不授权修改任何 C#、Scene、
> Prefab、asmdef、资源、ProjectSettings 或运行行为；所有实施批次获批后须按
> `docs/ai/CHANGE-LEDGER.md` 规则独立立项。
>
> 权威入口：`docs/ai/CURRENT-AUTHORITY.md`；本计划不改变 NTSD 2.8-Logan 战斗
> 行为权威、33 ms 正常逻辑间隔、十一阶段有序关闭合同与 Direction B 内容权威。
>
> 评审说明：本文件供后续综合评审（含外部模型评审）使用。全文对每条陈述标注
> 证据等级：`[已验证-代码]`（附文件与位置）、`[已验证-文档]`、`[推演-待测]`、
> `[提案-待批]`。评审时请按证据等级区分"事实"与"假设"。

## 1. 目标与不变量

### 1.1 目标定义

在 1000 实体极限战斗场景下，Unity Stats 面板渲染帧率稳定达到 120 FPS。

"稳定"的判定标准（防误读，验收时按此执行）：

- 连续 soak 运行（建议 10 分钟）内，帧时间 p99 ≤ 8.33 ms；
- 不允许"平均 120 但周期性掉到 60"的尖峰形态通过验收。

### 1.2 不变量（本计划任何步骤不得触碰）

| 不变量 | 依据 |
|---|---|
| 逻辑帧固定 33 ms 一格（30 Hz），逻辑内容、pass 顺序、RNG 调用次数与顺序不变 | `[已验证-文档]` AGENTS.md 第 6 节 cadence 契约（NTSD28-B1-CADENCE-CONTRACT-001） |
| 战斗可观察行为与 NTSD 2.8-Logan 正式 EXE 一致 | `[已验证-文档]` AGENTS.md 第 2 节权威体系 |
| 同 seed、同输入、同 tick 的 checksum 与回放结果逐位一致 | `[已验证-文档]` AGENTS.md 第 7 节 |
| `Running → Stopping → Stopped` 十一阶段有序关闭 | `[已验证-文档]` AGENTS.md 6.1 |
| Direction B 内容数值权威 | `[已验证-文档]` AGENTS.md 2.2 |

### 1.3 术语澄清（评审前必读，防止目标误读）

1. **Stats FPS ≠ 逻辑帧率。** Stats 统计的是 Unity 主循环（Update + 渲染）每秒
   完成次数；逻辑帧固定 30 Hz，与 Stats 数字相互独立。120 目标是渲染帧率，
   不是把逻辑提速到 120 Hz。
2. **"逻辑帧不变"贯穿全计划。** 后文 Step B（worker 化）搬运的是 tick 的
   **执行线程**，不是 tick 的节奏、内容或结果；Step C 优化的是**同一 tick 的
   计算成本**。两者的硬验收都是第 1.2 节的逐位一致。
3. **插值 ≠ 提帧率。** 插值层解决"30 Hz 逻辑在 120 Hz 显示上的运动平滑"，
   属于呈现正确性；Stats 数字由主线程每帧成本决定，两者是不同指标。
4. **垂直同步前置条件。** 120 Stats 需要显示环境支持（120 Hz 屏或关闭
   垂直同步）；这是测试环境配置项，不是性能项，但验收前必须固定，否则数字
   无意义。

### 1.4 范围外（明确排除，防止评审误判遗漏）

- **模拟 LOD / 降频 / 分帧摊**：NTSD 权威对全部实体每 tick 一视同仁，任何
  降频直接改变行为，违反复刻合同。永久排除，除非获得权威证据。
- **移动端专项**（AGENTS.md 第 14 节延长线）：本计划先在开发机口径下测量与
  实施；低端机/移动端指标是否单列见第 8 节开放决策 D3。
- 网络、回滚 netcode、资源格式重构、UI 改版。

## 2. 当前事实基线（按证据等级）

### 2.1 已验证事实

| 编号 | 事实 | 证据 |
|---|---|---|
| F1 | 逻辑节奏契约：正常 33 ms 一 tick，F5 快速 3 ms，wall-clock debt 最多 2 个 interval | `[已验证-文档]` AGENTS.md 第 6 节 |
| F2 | tick interval 在 Unity Update 内由 LocalFreeRun 在主线程排空（单次 Update 最多 2 个 interval），模拟与渲染同线程串行 | `[已验证-文档]` AGENTS.md 第 6 节（NTSD28-B1-UNITY-HOST-LOOP-BRIDGE-001） |
| F3 | 中央渲染系统对"同一 publication 的重复显示帧"去重：publication version 未变化时跳过重建网格，仅重复提交既有 submission | `[已验证-代码]` `Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs`，`MaterializeLatestPublishedFrame` 内 `lastMaterializedPublicationVersion` 比对 |
| F4 | 渲染提交成本为个位数 draw call：主网格按 segment（chunk subMesh）+ 脚印批 + 血条批 + 底部覆盖层 | `[已验证-代码]` `Assets/NTSD/Scripts/Animation/Rendering/BattleRenderFeature.cs`，`BattleRenderPass.Execute` |
| F5 | 表现捕获为每 tick 全量：扫描全部表现实体、Z 基数排序、构建命令（实体量 × 每实体约 2+ 条 + 命中记录）、抓取命中周期 | `[已验证-代码]` `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs`（`BeginFrame`、`CaptureBuildAndPublishFrame`、`MaterializeCommands`、`MaterializePresentationOrder`） |
| F6 | 碰撞候选已有空间 broadphase（松散四叉树）与权威 brute-force 对数回退门 | `[已验证-代码]` `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`（`LooseQuadtreeBroadphase` 字段与 fallback 计数门） |
| F7 | 表现侧 worker 发布缝已存在：`BattlePresentationCoordinator.BeginSimulationWorkerFrame` 已支持由 simulation worker 在 CentralOnly 模式发布逻辑快照 | `[已验证-代码]` `BattlePresentationShadowBuild.cs`，`BeginSimulationWorkerFrame` |
| F8 | 中央物化（frozen frame 拷贝、quad 写入、顶点上传）在每个新 publication 的渲染时机执行，频率 = 逻辑 tick 频率（30 Hz），非每渲染帧 | `[已验证-代码]` `BattleCentralRenderSystem.PrepareFrameImmediate` / `BattleDynamicMeshBackend.Build` |
| F9 | 边界重构计划 `SIMULATION-MONO-BOUNDARY-REFACTOR-001` 已存在且状态 `DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`；其 B6 批次（tick publication 与 Unity dispatch 分离）是本计划 Step B 的前置 | `[已验证-文档+代码]` `Assets/NTSD/Docs/simulation-mono-nonmono-boundary-refactor-plan.md`；其第 19 节为本会话评审附录 |

### 2.2 观察报告（未经实测确认）

| 编号 | 观察 | 等级 |
|---|---|---|
| O1 | 用户报告：1000 实体对战 Stats 稳定 30 FPS 已需努力，60/120 目前不可达 | `[推演-待测]`，M0 第一项就是复核并量化该现象 |
| O2 | 瓶颈模型：tick 帧主线程成本 ≈ 33 ms（模拟流水线 + 表现捕获 + 中央物化），将主循环钉在 30 FPS | `[推演-待测]`，由 F2+F5+F8 推导，依赖 M0 数据证实或推翻 |

### 2.3 已知数据缺口

- 单 tick 各 pass 毫秒级耗时分布（哪个 pass 占多少）——未知；
- 中央物化在 1000 实体下的单次毫秒成本——未知；
- 主线程非战斗底噪（URP + UI + 引擎）在目标机上的占用——未知；
- 1000 实体下的 GC 分配现状——未知（合同要求 0GC，但没有 1000 实体口径的实测）。

以上缺口是 Step 0（M0 测量）的存在理由；M0 数据可能证实或**推翻**第 3 节的
瓶颈模型。若被推翻（例如实测 tick 仅 10 ms），本路线图的 Step 顺序必须重排，
这属于计划的正常修正路径而非失败。

## 3. 瓶颈模型与预算推演（全部标注 `[推演-待测]`，M0 后回填实测值）

### 3.1 帧预算拆分（Stats 120 = 每帧 8.33 ms）

| 帧类型 | 频率 | 主线程必须完成 | 推演结论 |
|---|---|---|---|
| tick 帧 | 每 4 渲染帧一次（30 Hz 逻辑） | 当前结构：整个 tick + 表现捕获 + 中央物化 + 渲染 | 推演 tick 成本 ~30 ms ≫ 8.33 ms，**数学上不可行** |
| 纯渲染帧 | 其余 3 帧 | 消费现成 submission + 渲染提交 + 引擎底噪 | 可行；底噪实测后确定剩余预算 |

### 3.2 三条硬结论

1. **路线 B（tick 移出主线程）对任何 ≥60 的 1000 实体 Stats 目标都是必选项**，
   不是可选项。只要 tick 在主线程同步执行，tick 帧成本就直接过不了 16.6 ms
   （60）或 8.33 ms（120）预算。
2. **路线 C（tick 提速）是 B 的稳定性配套**：worker 化后 tick 仍须自守 33 ms
   节奏；当前 ~30 ms 的推演利用率 >90%，无抖动吸收空间，长跑必然滑帧。
   C 的目标定为 worker 上 tick 稳定 ≤25 ms（约 75% 利用率）。
3. **路线 A 承担主线程残馀预算**：worker 化后主线程每帧剩"消费 publication +
   渲染 + 底噪"，但每 4 帧一次的物化尖峰（F8）是最大威胁；且无插值时 120 Hz
   屏幕上每个逻辑帧重复 4 次，运动为 30 Hz 顿挫。A 负责：压物化尖峰、
   （可选）物化移出主线程、补插值层。

## 4. 路线图（四步；每步独立立项、独立验收、独立回滚）

```text
Step 0 测量（M0）──┬──> Step A 表现侧安全区（零确定性风险，可先行）
                   └──> Step C 热点清单（数据驱动）
边界重构 B0-B6 解冻并完成（前置，USER_HOLD，见 5.1）
                   └──> Step B worker 化 ──> Step C 收尾 ──> 联合验收
```

关键路径：边界重构 → Step B → Step C 收尾。Step A 与 Step 0 可并行先行，
但不改变关键路径长度。

### 4.0 Step 0：测量（M0）

目标：把第 2.3 节全部缺口变成数字，证实或推翻 3.2 的模型。

- 场景：`ProductionEntityStressHarness` 1000 实体配置 `[已验证-代码]`
  （`Animation/Rendering/ProductionEntityStressHarness.cs`），外加一场普通
  规模对战作对照组；
- 采集点（全部使用既有基建，不新增埋点为首选）：
  - 单 tick 各阶段耗时：`BattleTickDetailPhaseDiagnostics` 既有阶段枚举
    （RenderBeginFrame、RenderPrepareFrameResolveCommands、MeshResolve、
    UploadChunks 等）+ 模拟 pass 侧阶段；
  - 物化单次成本：`NTSD.BattlePresentation.*` Profiler 标记
    （MaterializeFrame / BuildMesh / PublishSubmission）；
  - 主线程帧分解：Unity Profiler（主线程 Update / 渲染 / 底噪）；
  - GC：Profiler 分配视图，1000 实体口径；
- 产出物：M0 报告，含每阶段均值/峰值/p99、帧类型分解、底噪占比，以及对
  3.2 三条结论的"证实/推翻"判定；
- 改动边界：首选零代码改动（外部 Profiler + 既有诊断导出）。若既有埋点
  不足以覆盖模拟 pass 侧耗时，需新增采样代码时，单独立 Change Record，
  不与任何优化混合。

### 4.1 Step A：表现侧安全区（零确定性风险路线）

定位：只动表现层，不触碰逻辑状态与模拟结果；可在 M0 后立即并行启动。

| 子项 | 内容 | 验收 |
|---|---|---|
| A1 物化降本 | 未变化 chunk 跳过重传、quad 脏区更新、`MeshDataArray` 批量上传 | 1000 实体单次物化耗时较 M0 基线下降（目标值由 M0 定） |
| A2 物化出主线程 | `Mesh.MeshData` worker 写顶点 + 主线程 Apply，把物化尖峰从 tick 帧摘除 | tick 帧主线程不再包含物化；提交正确性 focused test 不变 |
| A3 插值层 | 按权威 `presentation_interpolation.cpp` 语义实现 Unity 侧呈现插值（上一 publication 与当前 publication 之间按显示时间插值）——注意这是**对齐权威已有呈现机制**，不是发明新行为 | 插值轨迹与权威语义一致（对照验收）；Stats 与逻辑契约不变 |

确定性论证：A 全部位于表现侧；模拟 checksum/回放逐位不变的 focused test
作为每子项必跑门禁。

### 4.2 Step B：tick worker 化（关键路径）

前置条件（缺一不可）：

1. `SIMULATION-MONO-BOUNDARY-REFACTOR-001` 解冻，完成 B0–B6 并达成 L1
   （Core 无 Mono 生命周期、无 GameObject/Transform/Renderer/Sprite/Mono
   singleton 依赖）。L1 达成后 Core 仅剩 `Vector3/Mathf` 等线程安全纯值类型，
   具备上 worker 资格 `[已验证-文档+代码]`（该计划 2.3/14.11 及其 19.3 评审）。
2. M0 报告完成，tick 成本与热点分布已知。

实施要点（复用既有缝，不新发明机制）：

- tick 循环由专用 worker 自驱固定 33 ms 节奏；主线程只消费 immutable
  publication；
- 表现捕获随 tick 走 worker：复用 `BeginSimulationWorkerFrame` 既有路径
  （F7）；
- 输入以 `FrameInputSet` 纯值递交；Unity 对象永不上 worker（边界计划第 15 节
  既有约束）；
- 发布同步复用中央渲染系统既有版本化 pending publication 与 lease/generation
  机制（F3）；
- 十一阶段关闭中"停 worker / join"阶段照旧执行。

硬验收：

- 同 seed 同输入，worker 化前后 checksum 与回放**逐位一致**（first
  difference 即停工）；
- 压力场 Stats ≥110 持续（120 的验收在 C 之后联合进行）；
- 长跑无 cadence slip 积累。

### 4.3 Step C：热点优化（数据驱动，B 之后收尾）

- 清单来源：M0 实测的 pass 耗时排序（先验候选：碰撞候选消费、交互流水线、
  状态机 pass；表现捕获若已随 B 移入 worker 则从主线程瓶颈清单除名）；
- 首选单线程 Burst：L1 纯净 Core 的 struct/array 内循环可安全 Burst 化，
  单线程内不改变结果顺序，确定性风险最低；
- 并行化仅对"逐实体独立可证"的 pass 考虑，且汇聚顺序固定；每项独立确定性
  论证与 checksum 门禁；这是最贵的一类改动，排最后；
- 验收：worker 上 tick 稳定 ≤25 ms；10 分钟 soak 零 cadence slip；
- 联合验收（Step A+B+C 全部完成后）：1000 实体压力场 Stats p99 帧时间
  ≤8.33 ms，达成 1.1 的"稳定 120"判定。

## 5. 与既有计划/合同的关系

### 5.1 依赖

- `SIMULATION-MONO-BOUNDARY-REFACTOR-001`（USER_HOLD）：Step B 的前置。
  本计划不重复其内容，也不推动其解冻；解冻由用户单独批准。该计划第 19 节
  评审附录 S2（性能因果定位）正是源于本次讨论的同一结论。

### 5.2 不触碰

- 十一阶段有序关闭合同（Step B 在其既有阶段内执行，不重排）；
- 33 ms cadence 契约与输入延迟语义；
- Direction B 内容权威与 DAT 资产；
- `Gen/`、`Plugins/`。

### 5.3 与 AGENTS.md 第 14 节（移动端）的关系

本计划先以开发机为测量与实施口径；低端机/移动端目标（含 ASTC、拆册等此前
讨论过的资源侧方案）是否并入本路线图作为延伸阶段，见第 8 节开放决策 D3。
资源侧方案与本计划正交：本计划治理"每帧时间"，资源方案治理"纹理内存"。

## 6. 风险与回滚

| 风险 | 表现 | 控制 |
|---|---|---|
| M0 推翻瓶颈模型 | 实测 tick 远低于 33 ms，真实瓶颈在别处（如渲染底噪） | 正常修正路径：重排 Step 顺序，不硬扛原模型 |
| 边界重构延期 | Step B 无法启动，关键路径拉长 | Step A/C 可先行交付部分收益；但 120 目标达成日随之顺延，须如实报告 |
| 插值与权威呈现节奏不一致 | 画面平滑但轨迹与权威不符 | A3 以 `presentation_interpolation.cpp` 为对照验收，first difference 即停 |
| worker 触碰 Unity API | 崩溃/异常 | 边界计划 13 节守卫 + L1 完成为 Step B 准入门禁 |
| 物化尖峰压不进预算 | tick 帧外仍有 >8.33 ms 帧 | A1→A2 递进；A2 后仍超则重估 120 目标可行性并如实报告 |
| 引擎底噪占比过高 | 战斗侧再快也到不了 120 | M0 实测底噪；超预算时报告为环境约束，不伪装成战斗问题 |
| 性能改造引入分配 | 0GC 合同破坏 | 每子项 allocation 计数对比门禁（沿用边界计划 15 节约束） |

回滚单位：每子项独立 Change，可独立回退；Step B 回退 = tick 回主线程
（结构端口保留），不影响 A 已交付成果。

## 7. 完成定义

仅当以下全部满足，才可报告"1000 实体 Stats 120 达成"：

1. 第 1.1 节判定标准通过（10 分钟 soak、p99 ≤ 8.33 ms、无周期性尖峰形态）；
2. 逻辑契约逐项复核不变（1.2 节全表）；
3. checksum/回放与基线逐位一致；
4. 十一阶段关闭通过（含 worker stop/join）；
5. `BattleRuntimeSelfCheck` 与完整 EditMode 通过；
6. 每步的 Change Record 状态链完整（`CODE_WRITTEN → … → VERIFIED`）。

只完成 Step A 或只完成 Step B 不得宣称达成目标；只能报告阶段性状态。

## 8. 开放决策清单（需用户拍板，评审时可检查决策完整性）

| 编号 | 决策 | 影响 |
|---|---|---|
| D1 | 是否批准 M0 测量执行（首选零代码改动；若需补埋点则另立 Change） | 一切数据的前提 |
| D2 | 是否解冻 `SIMULATION-MONO-BOUNDARY-REFACTOR-001`（Step B 前置） | 决定关键路径启动日 |
| D3 | 目标平台口径：仅开发机，还是低端机/移动端指标单列（AGENTS.md 14 延长线是否并入） | 决定底噪预算与 A 系指标线 |
| D4 | 插值层（A3）是否纳入首批：它是对齐任务但触及全局呈现管线 | 决定 A 批次规模 |

## 9. 明确不做的事

1. 不修改逻辑帧率、pass 顺序、RNG、输入语义（1.2 全表）；
2. 不做模拟 LOD / 降频 / 分帧摊（1.4 排除项）；
3. 不在本计划内顺手做资源格式重构、渲染管线更换、UI 改版；
4. 不在未实测前对任何 pass 做优化（所有 C 类优化必须 M0 数据在案）；
5. 不使用破坏性 Git 操作；所有批次按 Change Record 独立回滚。

## 10. 恢复方式

本文档保持 `DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`。用户批准
启动时：

1. 先读 `docs/ai/CURRENT-AUTHORITY.md` 与当前 `AGENTS.md`；
2. 从 M0 开始，按第 8 节决策清单逐项确认；
3. 每步立项时重新扫描代码现状，不假设本文引用的文件行号仍然有效；
4. 若本文档被重命名、移动或拆分，须在引用它的文档（含边界重构计划第 19 节
   关联讨论）中同步更新路径。
