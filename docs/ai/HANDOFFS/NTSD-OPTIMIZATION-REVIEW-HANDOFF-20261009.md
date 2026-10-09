> 2026-10-09 准备/诊断已实测通过：`NTSD-H06-PROFILER-RESET-100-PREP-001` / VERIFIED（仅准备测试）；100对象三次collect各22event/40B，首managed frame FindEffect71，新raw成功导出，设置及原Scene恢复/保护合格。H06准入仍BLOCKED、Brute默认及95OFF保持，不恢复旧Goal，不计新增优化包。[Task](../TASKS/NTSD-H06-PROFILER-RESET-100-PREP-20261009.md) / [报告](../../../artifacts/diagnostics/NTSD-H06-PROFILER-RESET-100-PREP-20261009/REPORT.md)。下方IN_PROGRESS为本轮事前快照。

> 2026-10-09 用户仅授权准备阶段：`NTSD-H06-PROFILER-RESET-100-PREP-001` / IN_PROGRESS。原Editor旧Profiler缓存清空＋100对象短分配定位；不改生产逻辑、不恢复自动目标，H06准入仍BLOCKED。 [Task](../TASKS/NTSD-H06-PROFILER-RESET-100-PREP-20261009.md)。

> **2026-10-09 优化文档整理入口（不恢复执行）：** 用户授权整理总清单/单项方案/进度/依赖；本轮只改文档。[组织与规则](../OPTIMIZATION-REVIEW-20261009/README.md)、[当前进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)、[依赖](../OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md)、[D1=A已批准](../OPTIMIZATION-REVIEW-20261009/DECISIONS.md)。34父项与旧证据保留；用户已批准D1=A：既有Role必要准入门通过后可先默认推广，H06/H07最终性能门不变；本会话仍未实施，H07仍FAIL；PERF/ATLAS/MONO/EXT-1保持原门。下方旧active/继续语句是历史，不得自动恢复；正式战斗规则不变；限定准入分阶段依D1=A，最终验收不降低。

# NTSD 优化外部评审交接：方案入口与截至目前已做的处理

日期：2026-10-09。状态：`USER_CANCELLED_EXECUTION / EXTERNAL_REVIEW_HANDOFF`。本文件是已做工作说明与评审入口，不是第二张进度总表、不授予继续实施权限。

## 1. 先看结论

- 用户本轮主动取消优化目标，转交另一位 AI 重新整理。此前暂停调用返回 paused；随后 fresh get_goal 返回 goal:null，当前无活动 Goal。不标完成、不回退现存代码、不再启动优化、测试或测量。
- 最新完成编号94，共73个已完成实施包；95是第74个已启动包，尚未完成验收，执行已停止。没有第96批。批号包括诊断、工具、测试、评估和无收益候选，不是95项有效性能优化。
- 34个父项（高12/中14/低8）完整关闭数仍为0；选定首阶段六项有5/6达到各自限定完成条件：H11、M03为选定实现/验收域通过，H06、M13、M14为选定评估完成。不是五个父项全面完成。
- H07 千人性能仍 `PERFORMANCE_FAIL`。同角色千人 SetPass 问题已取得实测修复，但逻辑耗时仍超33ms。没有 Android、稳定120FPS、全角色混战或持续热稳态证书。
- 本文的实测数字来自已有具名原件，本次仅重新阅读文档和必要代码，不新增运行、测试、Profiler或性能测量。

## 2. 优化方案到底是哪几份

| 阅读顺序 | 文档 | 用途 |
|---|---|---|
| 1 | [优化重基线与共同启动门](../../../Assets/NTSD/Docs/battle-optimization-rebaseline-and-start-gates-20261006.md) | 总体范围、依赖、共同合同与启动条件；旧状态按日期辨别。 |
| 2 | [Android与1000AI高/中/低风险登记表](../../../Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md) | 全部34项清单及逐项独立方案链接；不是当前完成事实的替代来源。 |
| 3 | [六项首阶段执行合同](../TASKS/NTSD-OPTIMIZATION-BOUNDED-AUTOGOAL-20261007.md) | 当前选定阶段、验收边界、用户后续更正。第0—8.1节优先于后文旧次数停点；本轮停止通知优先于旧自动执行文字。 |
| 4 | [唯一进度总表](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md) | 全量批次、状态、失败、不采用决定与证据回链；历史“当前/Goal active”按文首停止通知解释。 |
| 5 | [H07千人性能方案](../../../Assets/NTSD/Docs/android-mobile-readiness/h-07-fresh-1000-ai-certificate.md) | 本阶段Windows Dispersed1000/Combat1000条件；父项Android证书另列。 |

相关专项： [PERF 1000实体×120FPS](../../../Assets/NTSD/Docs/battle-performance-stats120-roadmap-plan.md)、[ATLAS内存/低端机](../../../Assets/NTSD/Docs/battle-atlas-memory-lowend-roadmap-plan.md)、[Mono/非Mono B0—B9](../../../Assets/NTSD/Docs/simulation-mono-nonmono-boundary-refactor-plan.md)、[EXT-1外部提案](../../../Assets/NTSD/Docs/battle-optimization-proposals-index.md)。

这些专项没有因本轮优化自动升格或全部实施。PERF最终显示目标与H07当前逻辑阶段门不是同一指标，后续整理应解释关系，不擅自改写现有红线或验收标准。

## 3. 已经做过什么：进入生产脚本的主要处理

下表“进入生产脚本”指当前代码与接线已存在，不等于每项父问题、所有场景和设备均验收完成。中央渲染系统在这轮之前就已存在，不能将整个系统计为本轮新成果。

| 处理 | 实际改动/作用 | 证据与准确边界 |
|---|---|---|
| H11 容量预热与封存 | 对现有表现容量、缓冲及相关缓存做冷准备、增长守卫和既有fail-closed约束，避免战斗中临时扩容。 | 01—05等早期批次；86最终补直接Battle冷接线并完成选定CPU表现域验收。不是GPU完成、全1000AI或Android内存证书。 |
| M03 子网格元数据批量上传 | 在稳定多submesh条件下复用既有批量上传路线，减少逐段元数据提交成本。 | [13报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH13-SUBMESH-BATCH-UPLOAD-20261006/REPORT.md)：受控1000-segment backend均值约下降46%—47%；不是整场FPS提升46%。 |
| M03 每camera物化消重 | 同一camera不重复物化已处理的显示样本，消除对应重复Build/上传。 | [15报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007/REPORT.md)：自然窗口Build/camera由2→1、实体顶点上传1408→704B/camera。不是通用dirty-chunk跳过已实现或整场耗时减半。 |
| Foot配置冷接线 | 无显式authoring时读取既有GameConfig脚印帧，保留显式配置/禁用，不改Scene/Prefab/资源。 | [22报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH22-RUNTIME-FOOT-CONFIG-20261007/REPORT.md)、[23限定收口](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH23-FULL-CHAIN-CLOSURE-20261007/REPORT.md)：活动Foot/Health、关闭/重进通过；catalog命令重放不冒充真实1000AI。 |
| BruteForce四个fast path | 空ITR pair门、空ITR roster、exact几何缓存、geometry-first前置，减少明确无效检查和重复几何准备。 | [40生产准入报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH40-BRUTE-PRODUCTION-ADMISSION-20261007/REPORT.md)。本轮重扫BruteForceSceneQuery.cs:295—298四flag确为true；历史性能仍不达33ms，不把跨版旧比值当当前严格A/B。 |
| 击杀播报名字冷预热 | BattleKnockoutFeedRowProjection冷绑定名称文本；热BuildName查缓存，封口后不临时拼接或增长。 | [56报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH56-KNOCKOUT-LABEL-PREWARM-20261008/REPORT.md)、[57实局验证](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH57-LABEL-PREWARM-WINDOWS-20261008/REPORT.md)。解决已观察名称投影分配源，不代表所有fixture/整个游戏永远0GC；旧资源失败保留。 |
| 本局完整视觉依赖＋共享纹理绑定 | CharacterAnimtorManager收集本局完整视觉依赖、在既有预算内建立共享绑定；保留完整逻辑配置、未知/超尺寸合法fallback。App接线，86补直接Battle及原地重开。 | [80实测](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH80-SETPASS-MATCH-BINDINGS-20261008/REPORT.md)：同Sakura Combat1000全帧Profiler SetPass均值1995→5，中央segment约1990.49→1，中央CPU DrawMesh约1991.49→2。未改segment合并规则、未用Instancing；非所有角色/设备恒为5，也不是GPU capture batch。 |
| H11直接Battle与重进闭合 | 原入口遗漏共享binding冷准备已补；复用既有生命周期，不另建Owner或修改关闭顺序。 | [86报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH86-DIRECT-BATTLE-SHARED-BINDINGS-20261009/REPORT.md)：两1800camera完整CPU表现热路径校准0事件、双slot、seal增长0、CPUlease0、Foot/Health活动、关闭三类残留0、Scene保持；选定H11阶段通过，父项更广证书未通过。 |

本次重扫的直接代码锚点：CharacterAnimtorManager.cs:565 CollectBattleVisualDependencyIds、:612 PrepareBattleCentralBindings、:751 TryBuildMatchCentralAtlasPublication；AppManager.cs:197正常冷接线。行号是2026-10-09本轮观察，后继须重新定位。

## 4. 已做但没有作为生产默认推广的候选

| 候选/评估 | 做到哪里 | 当前决定 |
|---|---|---|
| Role-aware碰撞收集 | 复用已有Direct/Sweep/tree/Role路径，四形状评估、完整Driver资格及实际千人对照；不是新空间网格或Burst。 | [81报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH81-POST-BINDINGS-ROLE-WINDOWS-20261009/REPORT.md)：collector均值约53—56→14—15ms，整tick约83—86→44ms，P95仍50.44/52.71ms且有drop。有收益信号，但未推广为生产默认，普通默认仍BruteForce。 |
| Brute/Role其它微缓存与拒绝前置 | 深度、资格复用、coarse/envelope/packet、capacity guard、producer快照等尝试已分别留下代码/资格/局部或实局证据。 | 不将“写了代码/聚焦PASS”算生产收益。部分无收益/不适用，部分仅局部信号；除上表明确准入项外，不自动切默认。逐项以总表和对应Record为准。 |
| 89四帧引用复用 | 局部正确性通过，但分散/集中收益不一致。 | DEFAULT_OFF / NO_CONSISTENT_GAIN；不重复旧版本窗口找PASS。 |
| 92/93 group快照拒绝前置 | 92局部及Driver资格；93实际千人OFF/ON成本对照。 | [93报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH93-ROLE-GROUP-SNAPSHOT-WINDOWS-20261009/REPORT.md)：整tick无收益证明，不推广，不升级1800刷证书；单次顺序波动不证明因果退化。 |
| 94 endpoint快照复用 | 最终38/38安全回归；局部夹具有信号，但最终没有实际AI/完整Driver收益证明。 | [94报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH94-ROLE-ENDPOINT-SNAPSHOT-20261009/REPORT.md)：LOCAL_ONLY / NOT_ADMITTED / DEFAULT_OFF。 |
| 95源坐标AI行增量刷新 | 局部37/37；两正式root各12tick、两真实catalog千人布局各32tick OFF/ON的声明hash/RNG/28行/索引等一致。isolated实际AI输入均值12.5261→12.1755ms，约0.35ms/2.80%，四块有一块回退。 | [95报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH95-SOURCE-COORDINATE-INCREMENTAL-20261009/REPORT.md)：IN_PROGRESS（执行已停止）/ DRIVER_PARITY_PASS / DRIVER_GC_FAIL / NOT_ADMITTED / DEFAULT_OFF。完整Driver资格OFF/ON均有GC事件；调用点仍未知。不能称整tick收益、0GC通过、候选导致GC或95已完成。 |

95现存诊断03/04已将事件分到stage准备每tick1次及core通常1次；05三次调用点导出均FAIL于envelope0，raw/失败/设置恢复证据保留。不是用户需要再批准一次测量才能整理方案，也不是新增性能收益。若将来恢复，先决定这些已有失败是否影响主方案，不应机械重跑同一导出。

## 5. 评估、工具和验证工作：不能计作优化收益

- H06：Brute/Role-aware固定四种形状的强一致和成本评估已完成选定产物；集中拥挤退化仍是风险，不代表全部OPoint/原生/生产默认准入闭合。
- M13声音：已有合成成本与真实热点资格评估，未建立值得实施的真实热点结论，未改声音聚合算法。
- M14完整性校验IO：记录906张图片约29,939,600B及重复校验成本；缺immutable manifest/read lease前置，因此保留校验，未实现绕过或缓存。
- M03/H11：新增上传/物化/segment/DrawMesh统计、同样本像素、插值/活动显示、双slot/lease/关闭与重进等聚焦验证。只提供各声明证据，不把观测代码当渲染加速。
- H07：新增固定千人冷入口、请求/结果冻结、逐scope可靠GC正负校准、完整状态/RNG比较、父子热点计时、开关恢复和关闭审计。
- 85/90/91等主要补输入/NativeSampledState归因；95部分后续工作为GC诊断。它们有调查价值，但没有直接减少生产算法耗时，批次和测试数量不能替代有效优化数量。

## 6. 停止时真正的瓶颈与未完成项

以下是已有Windows Editor、Sakura1000、共享绑定、显式Role窗口；不是当前普通生产Brute默认的速度，也不是Android/Player结果。93的OFF只表示关闭第92候选，不表示关闭全部优化。

| 93实际1000AI基线 | 完整logic均值 | logic P95 | 正常debt dropped ticks | CandidateCollect均值 | PairExactLoop均值 |
|---|---:|---:|---:|---:|---:|
| Dispersed1000 / OFF | 48.309674ms | 63.747075ms | 406 | 16.051197ms | 8.255408ms |
| Combat1000 / OFF | 57.639042ms | 88.605835ms | 505 | 19.701257ms | 10.379653ms |

两窗口全帧SetPass均5、中央CPU DrawMesh均2、physical segment均1；同步完整Driver的180个steady sample校准0事件。其显示帧间隔均值约173.75/192.45ms，不能把1/logic耗时当画面FPS。95是另一个Manual/headless资格fixture，GC FAIL不抹去93证据，也不被93结果覆盖。

[91热点报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH91-NATIVE-SAMPLED-STATE-WINDOWS-20261009/REPORT.md)：CandidateCollect约15.7—16.2ms、CharacterInput约12.1—12.6ms、LateEntityUpdate约5.8—5.9ms；NativeBuiltins子步骤约1.0—1.1ms，不宜当最大计算块。阶段存在父子嵌套，不得直接相加或叠加P95。

H07仍需一条能显著降低整tick成本的主方案，现有95约0.35ms局部信号不足以解释33ms目标如何实现。已识别热点、已有多项局部改法，不等于已经验证出足够跨越性能缺口的整体路径。

当前阶段完成门保持：两实际1000AI布局，先有效120warm+180冒烟，再120warm+1800正式；logic P95<33ms、无持续backlog、正常dropped tick0、暖logic0GC、容量reject/central unresolved-stale/teardown残留0及必要正确性门。性能门判断阶段完成，不是自动停止次数边界；本次停止来自用户主动取消。

尚未实施/未授权自动展开：

- ATLAS编辑器/构建期预印、正式bank拆分、ASTC/设备档位或预算/资源格式变更；80是当前冷期共享绑定修复，不等于这些完成。
- EXT-1 GPU Instancing仍PROPOSED / MODIFY_REQUIRED，未升格、未启动专项M0。
- Mono/非Mono B0—B9与asmdef独立专项仍USER_HOLD。
- 新空间网格、Burst/Jobs主线、Role生产默认推广及千人专用AI行为策略没有自动实施授权。
- Android构建/触控/ARM64/设备、持续热稳态/全角色与120FPS证书仍待各自任务，不从Windows局部证据推导。

用户提出的“密集区降低AI决策频率、分批疏散、迁区后攻击”应作为可评审方向，不得仅以原版AI一致否决。它与不改规则的空间候选裁剪不是同类：若选择新模式，要明确适用模式、确定性调度/RNG/回放、拥挤迁移与攻击策略及授权。完整模拟tick仍33ms；空间分区要按实际ITR/BDY保守覆盖，不能假设中心格＋八邻格永远足够。本段是评审边界，不是设计已闭合或实现已批准。

## 7. 给下一位 AI 的工作要求

本轮只读评审/整理建议，不继续代码或测试。请基于当前代码和原件回答：

1. 哪些实际生产优化应保留；哪些默认OFF候选应结束研究；哪些工具/重复验证没有必要继续。
2. 为什么已有工作未解决H07：算法复杂度、数据重复搬运/采样、候选规模、输入/AI、生产默认与诊断配置差异、Editor测量限制分别有多少证据；未知不猜。
3. 重新按高/中/低优先级组织34项，先聚焦未过H07；把收益假设、代码落点、最小验证、依赖和需用户决定项写清楚。
4. 提出少量高杠杆主方案，说明收益可覆盖哪块整tick成本、不能覆盖什么；不要继续无据微缓存或用更多“诊断批”替代方案。
5. 分开“方案已足够明确可实施”“候选值得继续”“阶段性能通过”“Android最终通过”。只对真正阶段验收设置完成门，不设批次数到限即停，不无限追加可选对齐/验收。
6. 不降低33ms/checksum/只读publication/有序关闭等既有红线；若AI行为方向需要独立模式或调整合同，明确提案交用户决定，不偷偷改也不直接否决。
7. 先交用户可审阅文本；需要修改方案文档或实施时另获明确批准。

阅读入口优先本说明→当前权威/AGENTS→总方案/六项合同→95、93、91、81、80、86报告；按需回查其余原件，不要求先重读全部历史或重跑所有测试。

留痕：[本轮文档交接Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-PAUSE-REVIEW-HANDOFF-20261009/RECORD.md)。本说明只汇总既有证据；唯一完整进度仍是第2节总表。
