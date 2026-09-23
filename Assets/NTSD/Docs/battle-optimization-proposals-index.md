# 战斗优化提案索引（供综合评审使用）

> 创建日期：2026-09-13
>
> 性质：导航索引。本文件不含独立方案内容，仅汇总当前会话产出的提案文档、
> 各自范围、依赖关系与评审注意事项。所有提案均为
> `DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`，未获实施授权。

## 一、提案文档清单

| 文档 | 计划标识 | 治理对象 | 一句话方案 |
|---|---|---|---|
| `battle-performance-stats120-roadmap-plan.md` | `BATTLE-PERF-STATS120-ROADMAP-001` | 每帧时间（Stats 渲染帧率） | 1000 实体 Stats 120：测量 → 表现侧安全区 → 边界重构前置 → tick worker 化 → 热点优化 |
| `battle-atlas-memory-lowend-roadmap-plan.md` | `BATTLE-ATLAS-MEMORY-LOWEND-ROADMAP-001` | 纹理内存与设备适配 | 全量一本图集改为"公共册+按角色分册"，构建期预印 + ASTC 变体 + 设备档位预算门禁 |
| `simulation-mono-nonmono-boundary-refactor-plan.md` | `SIMULATION-MONO-BOUNDARY-REFACTOR-001` | Mono/非 Mono 架构边界 | 既有计划（非本次产出）；本次仅追加第 19 节外部评审附录 |

## 二、三份文档的关系

```text
性能计划 (PERF) ──依赖──> 边界重构计划 (MONO) B0-B6 + L1
     ▲                            ▲
     │ 无依赖，正交并行            │ 其第 19 节评审附录
资源计划 (ATLAS) ────────────────┘
（资源计划与性能计划互为独立工程：一个治"每帧时间"，一个治"纹理内存"）
```

- PERF 的 Step B（tick worker 化）以 MONO 计划完成为前置；
- ATLAS 无前置工程依赖，M0 测量可独立先行；
- 三份文档共享同一套不变量：逻辑 33 ms 契约、NTSD 2.8-Logan 行为权威、
  checksum 逐位一致、十一阶段关闭、Direction B 内容权威。

## 三、本次会话新增产出记录（2026-09-13）

1. PERF 计划文档（新建）；
2. ATLAS 计划文档（新建）；
3. MONO 计划第 19 节外部评审附录（追加）：总评"方案合理，可作实施合同
   冻结"，含诊断真实性抽查（三条反向依赖代码核实）、认可要点 6 条、
   建议 S1–S6（其中 S6 指出该计划第 3 节"30 Hz"表述与头部 33 ms 权威更新
   的内部不一致）。

## 四、评审注意事项（提交综合评审时建议关注）

1. 三份文档均对陈述标注证据等级（`已验证-代码/文档`、`推演-待测`、
   `提案-待批`），评审时请按等级区分事实与假设；
2. PERF 计划的瓶颈模型（tick ≈33 ms）是待 M0 证实的推演，文档内建了
   "模型被推翻则重排路线"的修正出口，这不属于方案缺陷；
3. ATLAS 计划的 opoint 可达闭包穷尽性是正确性硬门禁（漏扫 = fail-closed
   拒画），评审请重点检查该机制设计；
4. 两计划各自列有开放决策清单（PERF D1–D4、ATLAS D1–D5），需用户拍板后
   才能进入实施；
5. 所有明确的排除项（模拟 LOD/降频、运行时动态图集、按时间回收、运行时
   压缩等）均来自会话内论证，评审若建议重启其中任何一项，请同时给出
   对应权威证据或合同变更理由。

## 五、当前全局状态

```text
BATTLE-PERF-STATS120-ROADMAP-001      DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD
BATTLE-ATLAS-MEMORY-LOWEND-ROADMAP-001 DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD
SIMULATION-MONO-BOUNDARY-REFACTOR-001  DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD
```

综合评审结论不影响上述状态；任何提案进入实施均须用户单独批准，并按
`docs/ai/CHANGE-LEDGER.md` 规则独立立项。

## 六、评审轮次记录

### R1 评审（2026-09-14，评审方：GPT6）

- **裁定**：三份文档均为"修改后采纳"，无退回；文档 3（边界计划）架构基础
  最好，文档 1（性能）需修改最多。
- **阻断级发现 4 项**（已全部核实属实并落入 R1 修订）：
  1. 内容权威已由 D-023（2026-09-12）切换为 NTSD 2.8-Logan 正式
     `resources/runtime`，两份新路线图引用的 Direction B 已过时；
  2. cadence 表述必须统一为精确 33 ms，禁用"30 Hz"（1000/33≈30.30，非
     严格 30；"每 4 渲染帧一 tick"仅为近似）；
  3. dedicated simulation worker 已在 `SimulationTickDriver` 默认启用
     （`useDedicatedSimulationWorker=true`，含资格拒绝与单飞背压），性能
     方案"tick 在主线程同步执行"的事实基线过时；
  4. 性能方案 Step B 原写"worker 自驱 33 ms"会篡改 Host cadence 所有权
     （F1/F2/F5/debt/输入冻结语义），已重写为"Host 拥策略、worker 只执行
     显式提交的 tick"。
- **高/中严重度发现**：draw 公式化（F4）、CentralOnly 跳过 legacy sort
  （F5）、物化频率措辞（F8）、验收 Player 化、ASTC 能力先行与 ETC2 候选、
  切场双预算（steady/transition peak）、逻辑所有权与物理 bank 分离、
  opoint 闭包生产者全集补全、构建确定性分级、`ISimulationTickHost.World`
  泄漏、Presentation ack 非阻塞语义、B6/B7 线程相关 API 前移、证据路径
  笔误等——全部落入 R1 修订或第 19.7 节修正清单。
- **第 19 节 S1–S6 裁定**：S2/S4/S5/S6 采纳（S6 强制）；S1/S3 修改后采纳；
  并新增 S7–S9（端口拆分不暴露 World、ack 非阻塞 + epoch 字段、worker
  准入级 API 清理）。正文修正清单 P-1…P-5 待用户批准后执行。

### R1 修订落地（2026-09-14）

| 文档 | 修订 |
|---|---|
| 性能计划 | R1：事实基线 F2/F2b/F2c/F4/F5/F7 重写，Step B 重写（Host 保留 cadence 所有权），验收 Player 化 + 附加延迟指标，新增 D5/D6，M0 新增 worker 现状取证第一优先组 |
| 资源计划 | R1：D-023 输入口径，P3 预印先于 P2 装载，装载语义"只装载不组装"，双预算（steady/transition peak），逻辑所有权与物理 bank 分离，闭包生产者全集 + 运行时断言，ASTC 能力先行 + ETC2 候选，构建确定性分级，新增 D6/D7 |
| 边界计划 | §19.7 追加 GPT6 裁定与正文修正清单 P-1…P-5（正文待批，未改动） |

### 待办（下一轮）

1. 将三份 R1 文档提交 GPT6 复核；
2. 复核通过后由用户批准：边界计划正文修正 P-1…P-5 落地、PERF D1（M0 启动）、
   ATLAS D1（内容审计启动）；
3. M0 报告产出后回填两份路线图的推演数值，冻结预算与验收阈值。

### R2 复核（2026-09-14，评审方：GPT6）与执行

- **裁定**：PERF `CONDITIONAL_REVIEW_PASS`（条件已在 R2 补齐）；ATLAS
  `REVIEW_PASS`（附 2 项轻微修正，已补齐）；MONO
  `CORRECTION_SET_APPROVED`。上轮 24 项覆盖：19 项入正文、4 项随
  P-1～P-5 回写、1 项（render/GPU 门）R2 补齐，遗漏 0 项。
- **批准与执行（仅文档正文，非代码大包）**：
  - MONO：P-1～P-5 + S1～S6 正文落地——7.1 端口拆分（不再暴露
    `SimulationWorld`）、7.2/7.4 epoch 链 + 比较规则、8.3 非阻塞 ack/slot
    生命周期、9.9/11 线程 API 重划（Debug 硬门）、13.1/13.6 守卫增强、
    3/14/15 33 ms 口径统一、19.6/19.7 排版修复；状态更新为
    `CORRECTION_SET_APPROVED / BODY_UPDATE_DONE`；
  - PERF R2：1.1 端到端 presented frame 硬门 + 归因指标、Step C Burst
    逐位合同、F9/Step B Debug 口径统一、M0 双标签
    （`ContentAuthorityState`/`UnityTestJobConflict`）、完成定义与 D1 更新；
  - ATLAS R2：D7 前移（D1→D3→D6→D7→D2→D5→D4）、P2 回退≠低端认证、
    D1 双标签。
- **D1 状态**：PERF D1 与 ATLAS D1 均 `APPROVED`；实际测量/Editor 扫描
  启动与活跃 `NTSD28-Q06` 恢复任务协调（R2 修正了复核引用的过时 job
  状态：`aa6b0c9f…` 已终态归档，见 CURRENT-AUTHORITY.md 第 3 行）。
- **代码实施**：仍未启动；边界计划保持 B0→B9 分批 + `USER_HOLD`，逐批
  单独批准（P-1 归 B1、P-2 归 B3/B4、P-3 归 B0–B6、P-4 归 B3、P-5 归 B0）。

## 七、外部技术参考

### EXT-1 GPU Instancing 序列帧合批（2026-09-22 登记；R6 修订 2026-09-23；状态：PROPOSED / MODIFY_REQUIRED）

- **来源**：诗悦网络《永远的蔚蓝星球》微信小游戏性能优化实战
  （Unity UUG 深圳站 2026-01-24 演讲实录，
  https://www.163.com/dy/article/KL1BHTA90526E124.html）。
  应用户要求评估"其方案对 1000 实体序列帧问题是否比我们更好"。
  评估结论：**不是整体更优（战场不同）**——他们解决的是表现层
  DC/CPU/带宽（平台为 CPU 极弱的微信小游戏，逻辑轻），我们两份路线图
  解决的是确定性逻辑 tick + 物化 + 内存治理；但其中 GPU Instancing
  可能是 PERF Step A 的候选方向，登记为待批提案。R1–R3 检验确认：
  当前代码可确认有效中央 segment 产生 `DrawMesh` 命令；URP 项目级
  SRP Batcher 开关虽已启用，但中央路径使用 `MaterialPropertyBlock`，
  且当前中央材质实例化变体关闭、动态合批关闭，因此不能声称中央绘制
  实际使用 SRP Batcher，也不能从源码推导真实 GPU batch 或 A4 收益。
- **边界声明**：本节仅为外部参考提案登记，**不修改 PERF/ATLAS 计划
  正文**；升格（写入 Step A 正文 / M0 统计清单）须用户批准并走对应
  计划修订流程；不构成任何代码实施授权。

#### EXT-1-A A4 候选：instanced quad 渲染（PERF Step A 候选子项）

- **候选做法**：战斗 sprite 统一为单一 4 顶点 quad + GPU Instancing。
  实例 payload 候选包括 position、size/pivot、depth、UV、翻转、
  `Color32` tint、Texture2DArray slice；实例顺序由 CPU 按 publication
  命令序列写入 buffer 表达，不能把排序键或“原始序列顺序”默认当作
  GPU 字段。CPU 侧另维护 compatibility key（材质/变体、shader/render
  state、binding mode、实际绑定的 Texture2D 身份（`SourceTexture2D` 为
  源纹理，`AtlasPageTexture2D` 为实际 atlas 页）或 Texture2DArray identity；
  palette index 不得在 remap 确认前写入既定 payload；GPU 只消费实例数据，不自走
  帧号/动画时间，也不据排序键重新排列实例；
- **现状与收益假设**：当前 F4 事实是每个有效中央 segment 录入一次
  `DrawMesh` 命令；segment 不是每实体一个，且 `Texture2DArray` 模式已
  允许同一 array 的不同 slice 留在同一 segment。必须区分：
  **逻辑兼容连续 run**（按排序后命令序列和 compatibility key 推导）
  与**实际物理 segment**（还受 chunk 边界、`StrictOrderedDraw`、绑定
  模式等约束）。A4 **不预设减少 CPU `DrawMesh` 命令数或 GPU draw 数**；
  当前同 chunk、非严格顺序路径的一个兼容 run 才可能对应一个 segment，
  跨 chunk 或严格顺序路径会额外分段。候选收益仅包括不改变物理分段
  语义的几何/实例数据复用、上传字节和物化耗时；不得把“跨 chunk 复用”
  解释为跨 chunk 合并或减少 draw。文章的 1500 DC → 233 batch 不得
  外推到本项目；
- **进入条件**：M0 必须同时统计逻辑兼容 run、chunk/模式造成的实际物理
  segment、中央 CPU `DrawMesh` 命令数、生产 RenderPass/command-buffer
  调用数、benchmark-local presenter 调用数和实际 GPU batch/SetPass
  （可用 Frame Debugger/GPU capture 时单列）；同时确认 segment/draw
  或物化成本确实是瓶颈。该条件需要新增按兼容键与连续 run 分组的统计，
  不能声称现有总数已足够；
- **合同兼容性**：逻辑 tick 权威、精确 33 ms cadence、逐位一致
  checksum、表现只读 publication 均不触碰——实例数据属表现层数据，
  A4 不改变任何逻辑 pass；当前帧号仍由 tick 决定，GPU 不自走动画；
- **升格前须闭合的数据契约**：
  - 实例 payload 与 CPU compatibility key 必须分开定义；数组 `slice`
    是实例数据/分布字段，不能默认作为 array identity 的分裂条件；
  - 先审计 NTSD 队伍色 remap 是否存在及其受影响角色范围；当前中央
    路径已确认有顶点 `Color32` tint，但尚未确认战斗 palette lookup。
    若确有 remap，才设计 palette array/实例索引，并把 palette 纹理、
    旧/新 submission 与 staging 的内存计入双预算；
  - Point 过滤保留；
  - 实例记录格式、每个 submission slot 的生命周期、读 lease、GPU
    consumer 已不再读取 buffer 的完成证明（如需 fence 或等价管线证据）、
    **每个 slot 的硬容量上限，以 instance record 数计量**（数值可在实施
    前确定）、超限时必须拒绝整份 submission 并 `fail-closed`；不得局部
    截断、静默丢弃实例或继续提交部分结果；预热容量、0GC 验收；CPU
    lease 归零或 `ExecuteCommandBuffer` 返回本身不足以证明 GPU 消费完成；
    不得在热路径动态扩容或覆盖仍被 RenderPass/GPU 消费的实例缓冲；
  - 稳态驻留的双 slot CPU/GPU instance buffer 必须计入
    `SteadyResidentBudget`；旧/新 slot、在途数据和 staging 的切换峰值
    必须计入 `TransitionPeakBudget`。若采用独立 renderer working-set
    budget，必须分别记录 steady/transition 两种状态并汇总到总峰值，
    同时避免与 ATLAS 两项预算重复计算；不能只在 palette remap 存在时计入；
  - 透明 sprite 的排序合同：CPU 必须按排序后的 publication 命令序列
    填充实例缓冲，GPU 不重新排序；只能在排序后兼容且连续的 run 内合批，
    任何 compatibility key 变化或层级断点都终止 run，不能为减少 draw
    任意跨越 `MaterializePresentationOrder` 的边界；
  - A1 的 chunk 脏区跳过仍是待实施设计，不能当作当前代码能力；A4
    必须定义自己的实例数据变更判定与上传粒度；
  - A2/A3 顺序：先冻结 A3 的 publication 插值、排序、first-visible 和
    latency 语义；实例位置、UV 等由同一显示时刻的 interpolated publication
    样本在 CPU 侧生成；再选择实例/顶点产物，最后定型 A2 job 输出，不把
    A4 假设写入既有 MeshData 合同；
- **验收草案（升格时细化）**：1.2 不变量全表不变；同 seed/输入
  checksum 逐位一致；逻辑兼容 run、物理 segment、中央 `DrawMesh` 命令、
  生产 RenderPass/`ExecuteCommandBuffer` 调用、benchmark-local presenter
  调用、全帧 Profiler draw calls、真实 GPU batch/SetPass 分口径记录；
  实例缓冲完整物化—上传—录制—提交热路径 0GC、容量、lease 与 GPU
  consumer 完成生命周期合同通过；first-visible tick、透明排序和
  `MaterializePresentationOrder` 层级结果不变。

- **路线图边界**：A4 只能替换表现 backend 的表示方式，必须保持已排序
  publication 命令顺序、兼容 run 语义和 fail-closed 行为；若要改变
  ATLAS 当前中央命令流或 segment 合并语义（包括跨 chunk 合并），必须
  先修订 PERF/ATLAS 正文并获批。M0 必须区分当前资源布局与候选预印
  bank 布局，不能用现状统计提前冻结资源格式、bank 或预算。

#### EXT-1-B bank 划分评估因子补"合批率"一列（ATLAS 支柱一候选）

- **内容修正**：不能把 instancing 批数写成材质/纹理页种类数的简单
  下界。候选批次取决于材质/变体、绑定模式、Texture2D 页或 array
  identity、排序后的连续兼容 run，以及实际 command 交错；array `slice`
  是独立分布字段，通常不能作为 array identity 的默认分裂键；若 M0 使用
  opaque compatibility key，必须保留可还原到完整 key tuple 的稳定映射；
- **M0 统计补充（待批准）**：按绑定模式分列记录实际资源身份：
  `SourceTextureIdentity`、`AtlasPageIdentity`、`TextureArrayIdentity`；
  `slice` 独立记录；同时记录 material variant、binding mode。若使用
  opaque compatibility key，必须保留能还原上述字段及其余完整 key tuple
  的稳定映射；并分开记录：
  1. 排序后的逻辑兼容 run 数；
  2. chunk/模式造成的实际物理 segment 数；
  3. 中央 `CommandBuffer.DrawMesh` 命令数（脚印/血条可分列）；
  4. 生产 RenderPass 调用数与 `ExecuteCommandBuffer` 调用数；
  5. benchmark-local `Graphics.DrawMesh` presenter 调用数；
  6. 全帧 Profiler draw calls；
  7. 若环境允许，Frame Debugger/GPU capture 的真实 batch/SetPass。
  总 segment/draw 不能反推出这些分组，benchmark-local presenter 调用数
  也不得命名为生产 RenderPass submission；
- **成本与进入条件**：这不是“无需新增测量”的一行补充。先完成 M0
  分组统计与 A4 可行性判断，再由用户决定是否把该因子写入 ATLAS
  正文；不自动冻结 bank 或预算。

#### EXT-1 明确不引入项

| 项 | 理由 |
|---|---|
| VAT 顶点动画贴图 | 我方动画为 DAT 序列帧，无骨骼/粒子模拟问题；硬搬只增动画贴图内存，与 ATLAS 内存治理目标相反 |
| GPU 自走动画/帧号 | 当前帧号是逻辑权威（33 ms tick 决定），GPU 自走违反表现不反写逻辑与逐位一致合同 |
| CBuffer 飘字专项 | 伤害飘字属战斗表现外围，默认范围外；A1 提案方向可参考其"增量代替重建"思想，但当前代码不能据此宣称已有脏区复用 |
| 团结引擎/WASM 分包/Metal | 平台不符（非微信小游戏） |

#### EXT-1 检验要点（供复核方核对）

1. F4 现状描述是否准确（`BattleRenderPass.Execute` 逐活动中央 segment
   一次 `DrawMesh`）；
2. "per-instance 数据由 publication 物化生成"是否与表现只读/逻辑权威
   合同完全兼容；物化路径是否会引入新的表现延迟或 0GC 违规
   （实例缓冲的分配与复用策略）；
3. 调色板 remap 变体成本是否被低估（受 remap 影响的角色数量、palette
   array 的内存/带宽代价、对 TransitionPeakBudget 的影响）；
4. "instancing 合批率与 bank 页种类数是乘法关系"是否成立；同屏混合
   几十种角色时合批率退化程度是否被如实标注；
5. M0 新增统计的定义是否足够区分中央 `DrawMesh` 命令、
   `ExecuteCommandBuffer`/render pass submission、全帧 Profiler draw calls
   和真实 GPU batch/SetPass；
6. A4 与 A2/A3 的实施顺序依赖（A3 插值作用于实例数据还是顶点缓冲）
   是否需要先论证；
7. 实例 payload 与 CPU compatibility key 是否分离，palette 字段是否在
   remap 未确认时被错误预设，array slice 是否被错误当成 array identity；
8. 双 submission slot、staging、instance buffer 容量和提交完成/等价管线
   生命周期是否已纳入 ATLAS 双预算或另设 renderer 工作集预算；
9. URP 项目级 SRP Batcher 开关、MPB 使用、材质 instancing variant 和
   dynamic batching 状态是否被准确描述；
10. EXT-1 是否明确保持 `PROPOSED / MODIFY_REQUIRED`，且没有提前改变
    PERF/ATLAS 的 segment 合并语义、fail-closed 合同、bank、预算或资源
    格式决策。

#### EXT-1-R1 GPT 检验后修订记录（2026-09-23）

- **检验结论**：`MODIFY_REQUIRED / 保留 PROPOSED`，不升格为 PERF/ATLAS
  正式步骤；本次仅修订索引提案，未修改两份路线图正文、代码、Scene、
  资源或 ProjectSettings。
- **已纠正的过度表述**：
  1. F4 只证明有效 segment 对应 `DrawMesh` 命令，不证明真实 GPU batch；
  2. “每材质/页一 draw”改为待验证假设；
  3. “合批率 × bank 页种类数”改为按兼容键和排序连续 run 统计；
  4. “不新增 M0 测量”改为必须新增分组统计，并区分 CPU submission、
     presenter submission 与 GPU capture；
  5. A1 dirty-chunk 跳过、实例缓冲 0GC、slot/lease/fence、容量及溢出
     行为改为升格前必须闭合的设计合同；
  6. remap 从既定前提改为先审计确认；当前已确认的是顶点 tint，不是
     palette lookup；
  7. A3 publication 插值/排序合同先于 A4 表示选择，A2 job 最后按最终
     顶点或实例产物定型。
- **尚未裁决项**：`BattlePresentationShadowBuild` 方法体因活跃 Q06 修改
  未检查，排序内部细节仍为“待确认”；本提案不得据此宣称透明排序已闭合。
- **下一轮升格门槛**：GPT 二次只读复核通过后，仍需 M0/Player 或 GPU
  capture 证据证明 A4 对当前 1000 实体口径有实际收益，才能考虑写入
  PERF/ATLAS 正文。

#### EXT-1-R2 GPT 检验后修订记录（2026-09-23）

- **检验结论**：继续 `MODIFY_REQUIRED / 保留 PROPOSED`；R1 方向正确，
  但尚未完整闭合 SRP Batcher 适用性、实例 payload/compatibility key、
  双 slot/staging 预算、submission 口径和 ATLAS segment 合同边界。
- **本次修正**：
  1. 明确 URP 项目级 SRP Batcher 开关开启不等于中央路径实际使用；
     MPB、instancing variant、dynamic batching 与真实 GPU batch/SetPass
     分开记录；
  2. 删除 A4 可能减少 CPU `DrawMesh`/GPU draw 的预设，收益收窄为
     几何构建、上传字节、物化耗时和跨 chunk 复用的待测假设；
  3. 将实例 payload 与 CPU compatibility key 分离，palette 只在 remap
     确认后加入，明确 array slice 不默认分裂 array identity；
  4. 增加 CPU publication 顺序填充、GPU 不重排、连续兼容 run 终止规则；
  5. 增加 instance buffer 双 slot、staging、容量、lease、提交完成/等价
     管线生命周期及 ATLAS 双预算边界；
  6. 将 M0 指标拆为中央 `DrawMesh` 命令、render pass/command-buffer
     submission、全帧 Profiler draw calls、真实 GPU batch/SetPass；
  7. 明确 A4 只能替换表现 backend 表示方式，不能未经批准改变 ATLAS
     segment 合并语义或 fail-closed 合同。
- **未改变状态**：未修改 PERF/ATLAS 正文，未修改代码、Scene、资源或
  ProjectSettings；未启动 M0、Player、Frame Debugger 或 GPU capture。
- **仍待确认**：`BattlePresentationShadowBuild` 排序方法体处于活跃 Q06
  修改路径，排序稳定性、first-visible 和透明重叠结果仍不得宣称已闭合。

#### EXT-1-R3 GPT 检验后修订记录（2026-09-23）

- **检验结论**：继续 `MODIFY_REQUIRED / 保留 PROPOSED`；R2 已补齐大部分
  护栏，但仍需明确逻辑兼容 run 与物理 segment、GPU consumer 完成语义、
  实例工作集预算和 M0 指标命名。
- **本次修正**：
  1. 明确逻辑兼容 run 不一定等于物理 segment；chunk 边界、
     `StrictOrderedDraw` 和绑定模式可能额外分段；
  2. 将“跨 chunk 复用”收窄为不改变物理分段/提交语义的数据或几何复用，
     跨 chunk 合并或减少 draw 必须另行修订并获批；
  3. 实例顺序改为由 CPU 按 publication 命令序列写入表达，排序键和
     原始序列顺序不默认进入 GPU payload；
  4. “提交完成”改为 GPU consumer 已不再读取 buffer 的证明，CPU lease
     归零或 `ExecuteCommandBuffer` 返回不再被视为充分条件；
  5. 双 slot、在途数据和 staging 的实例 buffer 工作集无条件纳入
     ATLAS 总峰值或独立 renderer working-set budget，并避免重复计算；
  6. M0 明确分开逻辑 run、物理 segment、中央 `DrawMesh` 命令、
     生产 RenderPass/`ExecuteCommandBuffer`、benchmark-local presenter
     调用、全帧 Profiler draw calls 和 GPU capture batch/SetPass；
  7. A3 的 interpolated publication、latency 和 first-visible 合同先定，
     再选 A4 表示，最后定型 A2 job 输出。
- **未改变状态**：仍未修改 PERF/ATLAS 正文、代码、Scene、资源或
  ProjectSettings；未启动 M0、Player、Frame Debugger 或 GPU capture。
- **仍待确认**：`BattlePresentationShadowBuild` 排序方法体属于活跃 Q06
  修改路径，排序稳定性、first-visible 和透明重叠结果仍不得宣称已闭合。

#### EXT-1-R4 GPT 检验后修订记录（2026-09-23）

- **检验结论**：继续 `MODIFY_REQUIRED / 保留 PROPOSED`；R3 仅剩三类
  文本合同缺口，本次已回写。
- **本次修正**：
  1. compatibility key 的纹理身份改为“实际绑定 Texture2D 身份”：
     `SourceTexture2D` 使用源纹理，`AtlasPageTexture2D` 使用实际 atlas 页；
     opaque key 必须可映射回完整 key tuple；
  2. 明确每个 submission slot 的硬容量上限，超限必须 fail-closed 拒绝，
     不得动态扩容、静默截断或丢弃实例；
  3. 明确预算合同：稳态驻留 instance buffer 进入
     `SteadyResidentBudget`，切换期间旧/新 slot、在途数据和 staging
     进入 `TransitionPeakBudget`；若使用独立 renderer working-set budget，
     也必须分别记录稳态/过渡峰值并汇总避免重复计算；
- **未改变状态**：未修改 PERF/ATLAS 正文、代码、Scene、资源或
  ProjectSettings；未启动 M0、Player、Frame Debugger 或 GPU capture。

#### EXT-1-R5 GPT 检验后修订记录（2026-09-23）

- **检验结论**：本轮确认 R4 仅剩两处文字合同缺口，已完成回写；状态仍为
  `MODIFY_REQUIRED / 保留 PROPOSED`。
- **本次修正**：
  1. 将 slot 容量单位明确为 instance record 数；超限必须拒绝整份
     submission 并 fail-closed，禁止局部截断、静默丢弃或提交部分结果；
  2. 将 M0 资源身份字段明确拆为 `SourceTextureIdentity`、
     `AtlasPageIdentity`、`TextureArrayIdentity` 和独立 `slice`，并要求
     opaque key 可还原完整 compatibility key tuple。
- **未改变状态**：未修改 PERF/ATLAS 正文、代码、Scene、资源或
  ProjectSettings；未启动 M0、Player、Frame Debugger 或 GPU capture。

#### EXT-1-R6 GPT 检验后修订记录（2026-09-23）

- **检验结论**：R5 的 M0 资源身份字段通过；仅剩 slot 容量合同中
  “隐式扩容”未排除显式热路径动态扩容的缺口。
- **本次修正**：将 A4 操作条款收紧为“不得在热路径动态扩容”，无论调用
  是否显式；其余容量、M0、预算和生命周期合同均未改动。
- **未改变状态**：未修改 PERF/ATLAS 正文、代码、Scene、资源或
  ProjectSettings；未启动 M0、Player、Frame Debugger 或 GPU capture。
