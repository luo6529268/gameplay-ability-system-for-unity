
> NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-20261010 IN_PROGRESS；NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-001 PLANNED。仅两脚本不接生产的局部cursor复用候选，RED/GREEN/0B后一次局部ABBA；旧兼容候选与producer/95 OFF、最终门保持。
> **2026-10-10 最新派发实态：独立评审 SCOPED_REVIEW_COMPLETE / HANDOFF_DISPATCHED；已发送 NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-20261010，执行会话turn01a1246d-a4b3-78a3-8fdd-edaffb253f18已active/inProgress，无error，cursor76；原automation已ACTIVE。准确两脚本/不接生产/有限局部对照范围见execution-handoff.txt及dispatch-receipt.json。下面READY_FOR_DISPATCH为派发前历史；没有C#、Unity或收益通过结论。**

> **2026-10-10 H07-INPUT 当前后继：NTSD-H07-INDEXED-NEAREST-REVIEW-20261010 已 SCOPED_REVIEW_COMPLETE / READY_FOR_DISPATCH。用户“那你来处理即可，然后再让另一个会话去处理”授权本讨论会话完成评审后直接派发。准确后继 NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-20261010 / Change NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-001，仅 AiSensingKernel.cs 与 AiSensingKernelEditorTests.cs 的 internal、不接生产的局部cursor距离复用候选；实际C#语义/局部0B后仅两布局各一次固定1000query局部ABBA。现有public入口/生产调用原样，不改World/Writer/Store/AI/Suite，不跑完整Driver，不豁免正式0GC/33ms/确定性/关闭。旧兼容候选OFF及Driver2event失败、producer暂缓、95OFF/H06不续/旧Goal队列取消保持；整个12ms/H07未完成。**
> 评审与准确指令：artifacts/diagnostics/NTSD-H07-INDEXED-NEAREST-REVIEW-20261010/REPORT.md、execution-handoff.txt。离线23870原Indexed对照/19125线性payload对照0差，只是模型，非C#/Unity/CPU收益；派发实态以dispatch-receipt/status文件为准。下方保留历史。

> **2026-10-10 NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-20261010：FAILED_GC_QUALIFICATION / FINITE_TASK_STOPPED；Change NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-001 BLOCKED，候选默认OFF。** 已批准Core冷接口替换后五脚本候选已写，最终生产19/19及资格4项通过、固定1000capture1000→0/原skip保留/纯pass0GC；唯一无细分OFF120+180完整Driver校准有效但steady2events(tick130/162)，停止余下ON/ON/OFF，收益与实景ON等价未测，不建议准入。300tick旧同fixture14字段0差、17字段原值已存但未配对；容量/关闭三0/Menu clean8/设置恢复，15,558其它保护稳+已批准Core，16备份及五源码窗前后稳。无新调用栈/归因/重测/新优化，producer暂缓/H06不续/95OFF/最终门保持。报告 artifacts/diagnostics/NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-20261010/REPORT.md；下方原状态保留历史。

> 2026-10-10 H07 native-owned candidate: NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-001 BLOCKED / 写域确认待答。仅新增一项测试、原Editor预期RED；生产未改、GREEN/ABBA未运行。World非partial且注释明确禁止该拆分，撤回Diagnostics中World partial计划；待用户确认将冷配置/计数入口放到Core现有诊断入口附近。原Task范围外不实施，不恢复Goal/队列；最终门保持。

> **2026-10-10 H07输入CPU补充检测完成 / BOUNDED_COMPATIBILITY_REPAIR_READY。** NTSD-H07-INPUT-SPIKE-CPU-DIAG-001 VERIFIED仅诊断：原Editor5/5、同raw176tick冷复用、无Profiler两窗各120+180/14字段0差异、兼容各18万skip/0commit；线程CPU量化15.625ms、cycles不换ms，尖峰不全是等待且计数器有开销。实际完整Driver稳态2/0event不抵消/不称资格。只派现有exact AI/native-owned/canonical有效的17字段兼容空转guard候选、默认OFF及原硬门；生产修复尚未实施。原Menu clean8/关闭0、15562保护与11备份通过。producer暂缓/H06不续/95OFF/33ms与Android门保持。[报告](artifacts/diagnostics/NTSD-H07-INPUT-SPIKE-CPU-DIAG-20261010/REPORT.md)。

> **2026-10-10 用户要求继续检验直至准确修复可派发；NTSD-H07-INPUT-SPIKE-CPU-DIAG-20261010 IN_PROGRESS / NTSD-H07-INPUT-SPIKE-CPU-DIAG-001 PLANNED。** 当前H07-INPUT同点补CPU证据，先同raw复用再无Profiler两窗；三诊断写域，生产未改，完整GC事件/最终门保持，未派发。producer暂缓/H06不续/95OFF。

> **2026-10-10 H07输入补充检验：SCOPED_DIAGNOSIS_COMPLETED_WITH_LIMITS / NO_PRODUCTION_HANDOFF。** NTSD-H07-INPUT-FLOW-DIAG-001 VERIFIED仅Editor诊断工具与单窗采集；8/8 focused、300tick完整checksum/输入双RNG14字段0差异，18万skip/0commit。守卫须同时验证DataOrientedCanonical与canonical绑定；当前raw明确tick130/162各36/30B且父Mono.JIT（受击音效/奔跑输入路径），完整Driver0GC仍FAIL。176/180steady有Profiler，所有耗时仅扰动诊断；查询访问数/写回/两次分配不足以解释尖峰，根因UNRESOLVED，不能宣称全部查清。仅两Editor域改动，生产未改、未派实施任务/恢复监测或Goal；producer暂缓/H06不续/95OFF/最终门保持。原Menu clean8、关闭objects/slots/borrowers0；15565保护中2声明文档计划变化，其余15563无漂移。 [报告](../../../artifacts/diagnostics/NTSD-H07-INPUT-FLOW-DIAG-20261010/REPORT.md) · [Task](../TASKS/NTSD-H07-INPUT-FLOW-DIAG-20261010.md)。

> **2026-10-10 用户要求先完成H07输入检验再交执行；NTSD-H07-INPUT-FLOW-DIAG-20261010 / IN_PROGRESS，NTSD-H07-INPUT-FLOW-DIAG-001 / PLANNED。** 仅两Editor域独立一窗全输入细分及当前GC调用栈，复用既有证据；生产代码不改、不派任务、不恢复监测/Goal。旧OFF2event/ABBA停止事实保留，0GC/最终门不降低；producer暂缓、H06不续、95OFF。 [Task](docs/ai/TASKS/NTSD-H07-INPUT-FLOW-DIAG-20261010.md)。

> **2026-10-10 H07-INPUT检测收口：SCOPED_DIAGNOSIS_COMPLETED_WITH_LIMITS / ABBA_STOPPED_AFTER_OFF_GC；Change NTSD-H07-INPUT-COST-DIAG-001 VERIFIED仅Editor有界采集。** 4/4 focused通过；首OFF120+180逐帧progress提交0、skip180000，每tick1000，兼容父段0.603017ms。新300tick与同fixture旧基线14字段0差异；CharacterInput15.042183ms、AI决策6.105529ms为嵌套诊断时间。完整Driver steady2event130/162，0GC FAIL，停止后三窗；无开关扰动/生产收益结论，不归因旧GC。设置恢复/十一阶段关闭/clean Menu8及15562保护通过。下一仅讨论native拥有且canonical有效的兼容外壳提前守卫；未改生产、不派任务、不恢复监测/Goal，producer暂缓/H06不续/95OFF/最终门保持。[报告](../../../artifacts/diagnostics/NTSD-H07-INPUT-COST-DIAG-20261010/REPORT.md) · [Task](../TASKS/NTSD-H07-INPUT-COST-DIAG-20261010.md)。

> **2026-10-10 当前直接授权：H07-INPUT有限检测 / NTSD-H07-INPUT-COST-DIAG-20261010 IN_PROGRESS；NTSD-H07-INPUT-COST-DIAG-001 PLANNED。** 当前讨论会话负责兼容空转定向验证与一次同fixture输入边界计时OFF→ON→ON→OFF诊断，原Editor复用。只两Editor工具域，不修改生产优化逻辑/资源/设置，不派发任务/恢复监测或Goal；额外方向不跟进，producer暂缓/OFF、H06不续办、95OFF、最终门保持。最新用户授权取代下方“仅讨论/无测试授权”的旧时点；[Task](../TASKS/NTSD-H07-INPUT-COST-DIAG-20261010.md)。

> **2026-10-10 当前第1讨论项：角色输入与AI输入计算 / STRATEGY_DISCUSSION_PENDING。** 用户要求按优先级重整并讨论具体实现，本项是原有H-07高优先子方案，不是新增方向。H-01部署后移；producer复用仍USER_DEFERRED/OFF，H06-ROLE-EXACT不续办，95 OFF。本次仅文档及窄源码核对，没有新优化实现或运行证据。下方旧“优先Role/下一Role”结论只保留原时点。[总清单](../../../Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md) · [唯一进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md) · [文档Task](../TASKS/NTSD-OPTIMIZATION-PRIORITY-ORDER-20261010.md)。

## 为什么排在第一

当前主目标是减少千人战斗整tick耗时。H-06既有Role限定准入已完成；原方案中的H07-INPUT优先级高，并有历史实景成本和当前代码入口。H-07父项的33ms证书是最终验收目标，不能靠继续生成报告代替优化。H-01等Android部署工作后移，未受影响的正确性/容量/关闭证据复用。

| 证据范围 | 已确认内容 | 决策限制 |
|---|---|---|
| 91历史千人窗口，报告第41行 | CharacterInput约12.1—12.6ms/tick；RemainingAiDecision约4.94—5.11ms/tick | 是整个输入阶段及嵌套AI阶段，不是可消除收益，也不是当前工作树新鲜测量 |
| 91混战ON原JSON，180样本 | IndexedCanonicalKernel均值3.117646ms，其中NearestSearch均值1.506964ms | nearest包含在kernel内，kernel又在上级阶段内，不能直接相加；已存在Indexed策略，不能假设仍是全量平方扫描 |
| 本轮窄源码静态核对 | SimulationAiDecisionModule在DataOrientedCanonical/direct模式调用inputWriter.TryEvaluateCanonicalDecision，其它分支调用AiDecisionKernel.TryEvaluate；AiDecisionKernel内部调用AiSensingKernel.TryFindNearest | 需沿实际生效分支讨论，不能只改一个未生效旁路；静态调用链不证明有新收益 |
| 原小快照方向 | producer复用已被用户暂缓；capture/sync处于不同可见边界，旧局部省读未证明当前整tick收益 | 不把本次排序当成重开producer、95或换名微缓存的授权 |

原证据：[91报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH91-NATIVE-SAMPLED-STATE-WINDOWS-20261009/REPORT.md)；[混战ON JSON](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH91-NATIVE-SAMPLED-STATE-WINDOWS-20261009/windows-01/03-combat1000-native-on/report.json)。这里只复用相位归因，不把旧工作树性能当当前证书。

## 当前具体讨论范围

优先讨论原方案“AI决策计算/目标查询及其数据访问”这一分支，目标是寻找可以等价减少的计算或访问工作。暂不选定新的算法、缓存或预期节省毫秒数；准确改法需要由当前讨论确定。

- 主要代码入口：Simulation/Ai/Runtime/SimulationAiDecisionModule.cs的IndexedCanonicalKernel实际分支；Simulation/Ai/Kernel/AiDecisionKernel.cs及其AiSensingKernel查询；CharacterInput发布/读取边界只按相关性核对。
- 现有DataOrientedCanonical/Indexed/canonical writer能力优先复用；不重建一套SoA，不直接认定“再加空间索引”就是缺口。
- 输入逐tick结果、目标选择和遍历/tie-break顺序、双RNG及完整checksum必须保持；本项不包含AI降频/疏散/迁区攻击行为改变。
- producer Runtime自复制复用保持暂缓，95候选仍OFF；不回到H06-ROLE-EXACT七段诊断，不重跑91八子阶段或无信息量旧窗口。
- 当前只讨论：明确优化哪一块、消除什么工作、为何等价、准确所有者/失效条件、最小验证和停止条件。没有安全且足够有价值的改法就保持“待方案/不实施”，不把排序升格为READY，不自动加实验或新优化点。

实施与最终性能验收仍使用下方原合同；具体方案确认后才另立准确Task/Change和备份，不因本次文档重排启动Unity/测试/Profiler/测量或发送消息。

---

# H07-INPUT：CharacterInput等价优化子方案

> 用户决定（2026-10-10）：87/88 producer复用 USER_DEFERRED / 生产OFF，原准入失败和ABBA GC失败保留，恢复条件见[单项记录](IMPLEMENTATION-RECORDS/H07-PRODUCER-SNAPSHOT-REUSE.md)。下一项选择[H06-ROLE-EXACT](H06-ROLE-EXACT.md)进入静态分析/方案；下方“由用户作决定”已有本次答复，不自动恢复旧候选。

本轮静态复核：当前CaptureSharedOwnedSnapshot已有unified/shared及direct-canonical分支；producer刷新后和native routing后的AI行/状态同步处于不同可见边界。不能把同名capture/sync当成同一份可直接删除的复制。91混战同窗IndexedCanonicalCapture约0.209ms、UnifiedSnapshotExecutionRowRefresh约0.245ms、NativeCanonicalAiSync约0.360ms、NativeInputStateSync约0.069ms；这些是历史嵌套计时，不相加、不当当前新鲜耗时或可节省量。IndexedCanonicalKernel约3.118ms（内含Nearest约1.507ms）也不能被上述省读整体覆盖。因此本轮不据“CharacterInput总12ms”继续提出无证微缓存；一般输入路线保留，优先核Role精确配对这块较大的已有热点。

> 最新（2026-10-10）：既有producer的新ABBA有限诊断已结束，复用仍OFF。重复ON均值波动15.518ms，未稳定复现回退也不能证明收益；首OFF可靠完整Driver稳态2event/tick130，调用点未知，其余三窗0，不能以旧窄域0覆盖。[新报告](../../../artifacts/diagnostics/NTSD-H07-PRODUCER-ABBA-DIAG-20261009/REPORT.md)。下一决定由用户作出，本方案不自动继续。

父项：H-07；与H-06、M-01共享成本背景，但不是新增第35父项。状态：ANALYSIS_REQUIRED / EXECUTION_STOPPED。建议高优先级，Role准入之后或其授权等待期间可独立只读分析。

## 本次具名准入结论（2026-10-09）

既有87/88生产者快照复用已按第二项具名授权完成当前判定：NOT_ADMITTED / DEFAULT_OFF。44+1+1测试及一次双布局四窗的正确性/完整Driver稳态0GC/容量/关闭通过；producer刷新两布局下降，但混战整tick50.428038→57.714222ms（+14.448675%），原因未闭合，未进入生产接线，不重复同版窗口。第一项Role限定准入完成，正常空配置默认Role保持。

[独立代码对照与结论](IMPLEMENTATION-RECORDS/H07-PRODUCER-SNAPSHOT-REUSE.md) · [完整报告](../../../artifacts/diagnostics/NTSD-H07-PRODUCER-SNAPSHOT-ADMISSION-20261009/REPORT.md)。下方一般CharacterInput优化仍为方案，不因本次授权自动继续；新路线/独立波动诊断/行为合同由用户决定。复用91相位归因、既有M09/M15指纹和H09影响域；保持publication只读、同tick字段失效与双RNG，不与AI降频/疏散捆绑。

## 事实与代码

91实际窗口CharacterInput约12.1—12.6ms；NativeSampledState八子阶段已测，Builtins约1.0—1.1ms不是全部输入成本。95 isolated输入12.5261→12.1755ms，仅约0.35ms局部信号，四块有一块回退。
已定位：Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs::ProcessNativeSampledState；Simulation/Ai/Runtime/SimulationAiDecisionModule.cs的RefreshCapture与AiDecisionKernel.TryEvaluate调用；Simulation/Ecs/Core/BattleAiUnifiedRowPublisher.cs；InputState.SyncFromRuntime消费口。现有SoA/DataOriented路径已存在，不能重建一套。
这些是候选调查范围，不是允许任意修改整个World/AI模块。

## 方法与有限出口

1. 先读91完整结果，画CharacterInput父子相位包含关系；同窗独占耗时才能合计。只补尚未覆盖的行捕获/感知/同步调用链，不能再重复八子阶段归因。
2. 逐字段列同tick读写时点、producer freeze、双state交换、State/HitStop等失效条件；查整块重复capture/同步。不能用每实体12微秒“感觉慢”代替成本证据。
3. 若同步/行发布主导，评估在原边界消除重复采样或更新，保留原fallback；若查询主导，再评估等价剪枝/数据访问布局。最近目标O(n²)尚未证实，tie-break/顺序/RNG保持。
4. 没有发现可解释且足够有价值的成本块则交付有据“不实施”结论，不开无证微缓存。95不自动成为必须续完的候选。

## 依赖、验证与结束

依赖M-09/M-15具名基线、H-09比较范围和现有数据发布合同；不依赖M-11全关闭、Role默认推广或95 GC修复全部完成。若所选fixture受95问题影响，则先修该fixture/生产调用点，不忽略0GC。
最小验证：受影响字段及边界focused/逐tick完整输入、state、数组索引、双RNG；有效完整Driver0GC校准；同指纹双布局必要A/B。复用原37/正式root及91归因中未受影响结果。必要时检验差异不是顺序环境噪声。
方案阶段完成：明确一个具名成本块、准确改法/写域/失效表/验证，或证据证明不值得继续。
实施阶段完成：候选实际减少声明成本，整tick不出现未解释回退，正确性/0GC/容量/关闭门通过。不承诺input<=6ms；H07<33ms最终门独立保留。
风险：按脏更新漏状态、旧source/physical坐标混读、published/scratch身份判断错误、RNG顺序变化。代码前新Task/Change及dirty备份，原canonical路径保留；回滚仅具名增量并遵守操作授权。
