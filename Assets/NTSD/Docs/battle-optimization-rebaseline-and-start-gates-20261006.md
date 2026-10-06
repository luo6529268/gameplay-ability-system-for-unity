# NTSD 优化文档重基线与启动门（2026-10-06）

> 标识：`NTSD-OPTIMIZATION-DOC-REBASELINE-20261006`
> 文档状态：`DOCUMENTS_REBASELINED / IMPLEMENTATION_STARTED`
> 文档整理阶段仅授权说明保留为历史。用户后续已批准按文档开始优化；
> 当前H-11最小预热及聚焦验证，统一状态见[独立进度总表](battle-optimization-progress-tracker.md)。
> EXT-1、MONO、资源/Scene/配置专项门不因总体授权自动取消，完整M0未完成。
>
> 主表：[34项优化风险登记表](android-mobile-readiness-priority-risk-register.md)。
> 主表只管理说明/进度/留痕，本文件管理共同合同与启动门，各项方案管理具体解决方案和验收。
> 静态重扫证据时间为2026-10-06；下列矩阵为验收条件，实际首批结果仅在进度总表/具名报告中记录。

## 1. 优先级与证据规则

- 高：启动/内存安全、0GC硬合同、当前性能基线或移动落地硬前置；中：成本候选、
  准入/所有权/设备策略或证据集成；低：研发后置的产品/发布配置与条件性A/B。
- 优先级不等于收益已测，也不等于必须先一次性完成所有高项才能做局部优化。
  ID是稳定追踪号，不从H/M/L前缀反推当前优先级：M-03已由中调高。
- 34项包含性能优化、就绪度、正确性/证据门和发布项；GPU Instancing不重复增加风险数。
- [已验证-代码/配置]仅证明本轮静态路径；[既有报告-限定]保留原运行层级与范围；
  [推演-待测]不当事实；[提案-待批]不得作为现有实现。
- 各独立文档旧日期/行号只作当日历史；以本文及当日修订为当前启动依据，
  后续实施仍重扫，不把2026-09-05 self-check或历史压力结果当当前证书。

## 2. 共同不可变合同

1. 正式行为权威保持[CURRENT-AUTHORITY](../../../docs/ai/CURRENT-AUTHORITY.md)与根
   [AGENTS](../../../AGENTS.md)：正式336B44完整SHA为
   `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。
   不晋升候选EXE/当前开发源码。源码与发行身份未确认时标待确认，使用明确冻结来源和
   已有正式证据，不以可变C++树反定义规则。
2. 正常33ms、F5 3ms，LocalFreeRun最多2个active interval debt/排空；
   同seed/input/tick checksum/回放逐位一致，pass/RNG/slot/generation/声音事件不被优化改变。
3. D-023使用非排除正式DAT与角色/技能引用图；保留项目背景、模式及既有表现例外。
   图像输入含PNG/BMP与正式decoder/crop语义；音频依既有独立任务来源，不因D-023自动全切。
4. presentation只读publication，插值/相机/GPU不得反写模拟；透明命令顺序、first-visible、
   latency、segment/fail-closed合同保持。排序内部/透明重叠本轮未新审活跃Q06方法体，
   不能写成全域已认证。
5. 11阶段有序关闭保持：先stop/join worker再清publication/submission/资源/World；
   迟到回调按session/world/entity generation处理。新增缓存/queue/Lease需声明owner和关闭阶段。
6. 热路径0GC、禁止seal后显式/隐式扩容；未来instancing的GPU完成需fence或等价证明，
   CPU lease归零/ExecuteCommandBuffer返回不能单独证明GPU已不读数据。
7. 不实施模拟降频/LOD/分帧摊、运行时动态图集/压缩/战斗中换册换档；
   bank/格式/预算/segment合并规则不在本轮冻结或改变。
8. 既有`USER_ACCEPTED_SCOPED_CLOSURE`保持；未来优化只重测受影响域与必要组合，
   不把旧开放Record或任务外错误自动变成重新执行全部历史campaign的前置。

## 3. 当前事实与方案调整

| 范围 | 本轮重扫证据（仓库相对路径/行号） | 当前处理 |
|---|---|---|
| 场景 | ProjectSettings/EditorBuildSettings.asset:7-13；H-02已有9/23记录 | Menu/Battle已接线，Android构建/冷启动仍待验收 |
| 内容/全量视觉 | CharacterAnimtorManager.cs:52,1473,2514,2582,2958-2965；GameConfig.asset:15 | 按当前Logan内容重新盘点；图集超预算回源不是全局低内存通过 |
| 插值/去重 | BattleCentralRenderSystem.cs:397-406,624-629,668-682,1060起 | publication不变但alpha变化可重建；PERF F3/F7/A3重基线 |
| 上传/payload | BattleDynamicMeshBackend.cs:443-449,663-686 | 活动顶点整范围上传；现有stride44/UV采样边界须承接，收益待测 |
| 0GC容量 | BattlePresentationDisplayMotion.cs:20,162-173；CentralRenderSystem.cs:181-215 | H-11新增；预热覆盖及运行高水位未闭合，非已测GC峰值 |
| 音频 | NTSDSoundPlayer.cs:178-209,278-313,681-713；共用声音报告:31/61 | H-10/M-13新增；约210MiB仅payload估算，已有退出计数0不称泄漏 |
| 加载背压 | CharacterAnimtorManager.cs:2505-2514 | M-12新增；并发数量≠像素字节上限；全量staging另计 |
| 内容验证 | LoganVisualContentCandidate.cs:363-404；CharacterAnimtorManager.cs:116,213,1463,1689 | M-14候选；仅复用已证明同一冻结内容，不移除完整性守卫 |
| 碰撞 | BruteForceSceneQuery.cs:28-29,2084-2091,2674,3267 | H-06用现有Role-aware/Sweep/树，默认仍BruteForce；499500非实际固定次数 |
| Worker/AI | SimulationTickDriver.cs:207,1516-1537；BattleAiExecutionProfile.cs:24 | M-01先测资格/回退/ack；已有DataOriented/SoA/增量查询，不能重新假设全是逐实体旧实现 |
| 池 | LoadingPrewarmController.cs:243-252；BattleRuntimeProfile.cs:137-164 | M-06从10/200容量不足改runtime预热成本；M-05分logic/AI/visible workload |
| 诊断 | BattleCentralRenderSystem.CommitCentralFailurePlan及首错计数 | M-04已有代码，剩设备故障注入/有界报告，不加Legacy静默回退 |
| 正确性/层级 | CURRENT-AUTHORITY.md:33/39；MONO §19.7.5 | H-09限定收尾与源身份边界；P-1…P-5正文已改、代码仍USER_HOLD |
| 构建依赖 | Packages/manifest.json:5 | M-15仅纳入Kernel精确身份；不改Server或包源 |

上述短文件名完整前缀分别为：
`Assets/NTSD/Scripts/Animation/Manager/`（CharacterAnimtorManager）、
`Animation/`（LoganVisualContentCandidate）、
`Animation/Rendering/`（CentralRenderSystem/DynamicMeshBackend）、
`Animation/Character/`（BruteForceSceneQuery）、
`App/`（NTSDSoundPlayer）、
`Simulation/Presentation/`（DisplayMotion）、
`Simulation/Host/`（TickDriver）、
`Simulation/Ai/Runtime/`（AiExecutionProfile）、
`Simulation/Runtime/`（RuntimeProfile）及`UI/`（LoadingPrewarmController）。
配置位于`Assets/NTSD/Config/GameConfig/GameConfig.asset`。
音频报告为`artifacts/diagnostics/NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006/original-common-fix-01/REPORT.md`。
这是一份当日静态证据索引，不改变任何文件的运行所有权。

## 4. 方案覆盖与启动依赖

| 领域 | 条目 | 获批后必要入口 |
|---|---|---|
| 全局内存/资源 | H-08/H-10、M-12/M-14 | 当前内容保守闭包与整局账本；先确定部署载体和Lease所有权，再冻结bank/设备数字 |
| 表现成本与0GC | H-11/M-03/M-02/M-04 | 现有插值/取样合同及容量基线；alpha与publication分别计数，透明/UV/segment不变 |
| 模拟吞吐 | H-06/H-07/M-01/M-05/M-06/M-13 | 有效1000 workload；现有AI/碰撞路径、资格与fallback分布，不先改规则或加线程 |
| 架构 | M-11 | 独立B0 inventory/guards；B0-B9顺序、asmdef后置；与算法/资源改造分批 |
| Android落地 | H-01/H-02/H-03/H-04/H-05、M-07/M-08/M-10 | 正式内容/场景/触屏/ABI闭包，再做具名GPU/API及持续认证 |
| 证据/正确性 | H-09/M-09/M-15 | 正式权威+冻结源/Trace来源、Kernel/dirty/内容/配置/设备指纹，按域失效 |
| 发布与条件性开关 | L-01～L-08 | 产品/安全/发布值和设备A/B；低优先级不豁免发布前门禁 |

**内存口径**：ATLAS既有SteadyResidentBudget/TransitionPeakBudget保持。
新增音频/模拟/renderer/加载/其他系统工作集单独记账，再汇总整局steady和transition总峰值；
共享对象只记一次，读Lease/GPU在途/staging不能漏记。是否将renderer工作集纳入ATLAS账本
或保持独立、设备总预算数值及去重规则在实施前批准；本轮不冻结数字。
已有source fallback或回旧路径不等于低端认证。

**未来统计schema（待批，不代表现有计数可推导）**：逻辑兼容run、物理segment、中央CommandBuffer.DrawMesh命令、
生产RenderPass、ExecuteCommandBuffer、benchmark-local Graphics.DrawMesh、
全帧Profiler draw calls、GPU batch/SetPass分别统计。
兼容身份区分SourceTextureIdentity/AtlasPageIdentity/TextureArrayIdentity/独立slice/
material variant/binding mode/render state；opaque key可还原tuple。
EXT-1附加schema仍属待批，不据CPU命令推导GPU batch，不提前冻bank/格式。

## 5. 分批顺序与当前启动范围

| 顺序 | 建议包 | 输出/停止条件 |
|---|---|---|
| 0 | 用户确认文档及测量范围 | 总体启动已批准；完整M0的平台/workload/运行窗口/准确范围仍须具名 |
| 1 | 当前版本基础取证与总内存/0GC基线 | 先复用既有计数；缺埋点另立Change，记录加载/稳定/过渡/退出、worker和插值成本 |
| 2a | H-11最小容量补齐 | 子批01两处现有预热已接入并聚焦通过；父项完整seal/overflow/0GC仍开放；不做instancing/算法重写 |
| 2b | H-08/H-10资源依赖和预处理设计 | 先Manifest/闭包/Lease/预算决策，再分批烘焙及按局加载；资源写入另批批准 |
| 3 | 数据驱动热点优化 | 现有碰撞/AI/声音/上传路径A/B，收益不成立或first difference即不采用 |
| 4 | B0及后续seam批次 | 单独批准，不因GPU或资源优化自动解冻MONO |
| 5 | Android功能和持续证书 | 工具链/ABI/内容/触屏具备后按H-05/H-07/M-08运行 |

基础取证不是EXT-1专项M0。PERF/ATLAS D1历史方案批准保留。整理阶段曾为
`WAITING_USER_APPROVAL`，后续总体启动授权已替代该状态；未选批次排队，单列专项门保持。

2026-10-06 后续启动更正：上句为整理阶段快照。用户已明确批准启动，
首批采用现有聚焦测试确认两处缓存遗漏，先H-11预热子批；
完整整局/M0基线和父H-11验收仍开放，未测量收益不晋升。

## 6. 实施前批准字段与验收底线

启动请求必须写：条目/Task/Change ID、精确文件/符号或资源清单、平台/设备/API/workload、
是否运行Unity/测试/测量、共享Editor窗口、容量/预算/latency未决项、验证门与可回滚方式。
不以“开始优化”推定改Scene/Input Actions/Server/资源格式/segment语义等新增权限。

每个实际批次按风险执行并报告：
compile → focused/SelfCheck → 固定输入/checksum/RNG/事件A/B →
真实Scene表现/first-visible/UV/透明/声音 → 0GC/steady-transition →
11阶段关闭及重进 → Player/Android具名证书。
文档整理阶段以上全未运行；后续子批01已执行编译/具名聚焦，
其余门未运行。项目级全部完成只在对应证据齐备时声明。
预算/容量/硬性能门数值在获批测量后冻结，不因现在未冻结数字制造文档阻断项。

## 7. 文档收口条件与留痕

本轮收口仅要求34项唯一ID/优先级/入口一致，方案/验收/测试齐全，引用能解析，
PERF/ATLAS/MONO与EXT-1状态边界不冲突、无新增运行授权、历史证据未擦除。
该文档静态检查不等于实施完成或性能达标。

Task：[本轮任务](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-DOC-REBASELINE-20261006.md)；
操作：[before/after与恢复来源](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-DOC-REBASELINE-20261006/RECORD.md)。
2026-10-06 文档阶段历史：用户批准仅文档整理；34项重基线，6项新增，M-03调高。
2026-10-06 后续实施：用户明确批准开始；H-11子批01编译/聚焦通过，父项RUNTIME_PENDING。
EXT-1仍维持`PROPOSED / MODIFY_REQUIRED`，不升格、不启动专项M0。
