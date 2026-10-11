> **2026-10-10 H07 owned-input有限评估完成：COMPLETED_NO_DISCERNIBLE_GAIN / 本版不采用。** NTSD-H07-OWNED-INPUT-EVALUATION-001 VERIFIED仅Editor诊断：最新3/3 focused、唯一千人合成native输入两布局8窗完成，256稳态pass0B、逐tick完整checksum/双RNG/41字段一致；两个布局均无可辨收益，不跑后继实景或推广，生产SnapshotCopy不变。不是GC门阻止测量，也不是已解决旧Sakura约12ms。主H07仍OPEN/未形成新的生产实施方案；未派发、监测暂停。报告artifacts/diagnostics/NTSD-H07-AI-DECISION-WORK-REVIEW-20261010/owned-input-evaluation/REPORT.md。以下保留此前时点。

> **2026-10-10 H07输入配置复核新增证据：** 当前程序集Debuggable263+/optimize-已记录，但历史12ms程序集不同，未做模式因果对照。DataOrientedCanonical不自动启用CanonicalStoreDirect；当前启动链默认SnapshotCopy，旧报告未记actual owned-mode，不以能力存在排除中间copy。原Editor三项现有direct/default/跨tick测试3/3通过，覆盖有限，尚缺完整native RNG/字段/边界及成本。详 `artifacts/diagnostics/NTSD-H07-AI-DECISION-WORK-REVIEW-20261010/input-config-review/REPORT.md`。主Task IN_PROGRESS；生产/编译模式未改，无新性能窗或派发，监测保持暂停。下方保留此前时点。

> **2026-10-10 H07同因跨模块复核进展：局部诊断 NTSD-H07-NATIVE-ROUTING-COST-PROBE-001 VERIFIED（仅诊断），性能结论 INCONCLUSIVE。** 已沿Native输入路由、状态解析、动作写入、legacy/AI/InputState镜像及快照尾部检查。有效千人合成idle/walk/run共12窗、12语义例、384测量轮0B/各40轮checksum一致/容量1024无拒绝、原Menu clean8与2361保护稳定；首版400槽错误整组INVALID，不能用旧NUnit PASS冒称有效。减少状态解析未在三场景稳定获益，无新生产优化或派发。主Task NTSD-H07-AI-DECISION-WORK-REVIEW-20261010 仍 IN_PROGRESS / CAUSAL_CHAIN_REVIEW，完整方案尚未形成，不称H07/12ms优化完成。新报告 artifacts/diagnostics/NTSD-H07-AI-DECISION-WORK-REVIEW-20261010/native-routing-cost-probe/REPORT.md。下方状态均为此前时点，最新修正优先。

> **2026-10-10 用户更正：按同一问题的因果链确定优化范围。** 用户明确：“如果检验过程，发现其他范围会影响到同一种情况，那就应该优化，而不是跳过啊”。NTSD-H07-AI-DECISION-WORK-REVIEW-20261010恢复IN_PROGRESS / CAUSAL_CHAIN_REVIEW。原H07角色输入耗时为目标；其它模块只要直接导致/放大同一耗时或阻断该优化，应纳入同一方案，不能仅因目录、旧写域或历史Q06标签跳过。调查可沿直接相关方法展开；实际修改仍须形成完整方案、声明准确路径与验证后交执行会话。下方窄域NO_HANDOFF及“后继不展开Q06”是此前时点，不再构成相关因果链分析的绝对禁令。历史失败/暂缓不抹去、旧候选不自动重跑，完整方案后再派发约定保持。精确决定见docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md最新更正。

## 2026-10-10 人类范围更正：同一输入问题的关联模块必须纳入

用户原文：“如果检验过程，发现其他范围会影响到同一种情况，那就应该优化，而不是跳过啊”。这修正了此前将OPTIMIZATION_PLAN_ONLY等同于固定文件白名单的过窄解释。

1. 原目标保持H07角色输入/AI输入阶段耗时；以真实调用、数据依赖和测量关系确定调查边界。直接导致、放大同一耗时，或直接阻断该改进的其它模块，属于本问题应查的范围。
2. 发现关联先追到具体操作、读写方/可见边界；若证据支持优化，就纳入同一完整方案，明确如何减少工作以及为什么不改变规则。不能仅以“不在之前两/五文件中”“是另一个模块”“带Q06历史编号”为理由跳过。直接相关Q06方法可按本次新授权具名窄读；不将本次授权追溯成之前误读已获授权，不整体展开无关Q06。
3. 调查范围可以跨模块，实施仍为最小充分改动。原任务的文件白名单是一次候选执行边界，不能反过来限制后续根因调查；完整方案应重新准确列明必要文件，不能让实施会话猜范围。
4. 已暂停/无收益/失败候选保留真实状态，不自动恢复旧实验。若新证据证明其中具体操作是同一问题的因果环节，应写清新证据和恢复理由并纳入评审，不能机械忽略，也不能把旧方案原样重跑当新优化。
5. 与本次输入问题无关的顺手优化继续不做；不恢复34项队列或旧Goal。战斗规则/顺序、33ms、输入双RNG/checksum、publication只读、容量与有序关闭等正确性合同不变。纯移计时范围、滤GC或降低验收标准不算优化。
6. 本讨论会话继续调查和整合；形成完整可实施方案后才发送既有“处理优化方案”会话。此次更正本身没有派发实施任务、恢复监测或修改生产代码。

### 已核实的直接关联入口

Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs:82-198 的ProcessNativeSampledState直接串接输入重映射/组合状态处理、CharacterActionWriter的combo/three-button/direction/builtins、legacy投影、AI canonical同步和InputState同步。这是同一角色输入流程的后半段，虽分散到不同模块，不能仅按文件范围排除。该调用链证明关联，不单独证明哪段存在冗余或可节省多少；本轮没有新性能测量或收益结论。

下一复核按这条实际链补齐数据读写和重复工作判断，复用既有细分数据；不因前面窄域未找到候选而把整个H07调查判为结束。

# H07 输入阶段：AI 决策整段工作复核

状态：SCOPED_SOURCE_REVIEW_COMPLETE / NO_JUSTIFIED_IMPLEMENTATION_HANDOFF。
H07-INPUT仍OPEN / STRATEGY_NOT_READY；本状态不表示整个输入优化完成，也不表示无法优化。

## 需求与实际动作

用户要求“那你先继续处理，等有完整的优化方案后，再给那个会话处理”。本讨论会话继续独立复核原H07输入/AI计算链，只有形成准确可执行方案才派发。当前没有给执行会话下发新任务，没有恢复监测或旧Goal/队列。
本轮仅源码/既有结果只读核对及文档整理；没有新C#实现、Unity编译、测试、Profiler或性能窗口。主代理负责调用链和语义；luna只读侧审既有方案/记录，结果经主代理核对。

## 已观察的真实调用链

LF2Character.RunCharacterInputProducerPhaseForKnownCharacterDat
→ SimulationWorld.PrepareAiInputBasic
→ SimulationAiDecisionModule.PrepareAiInputBasic（IndexedCanonical）
→ SimulationWorld.TryPrepareAiDecisionIndexedCanonicalForModule
→ CaptureAiDecisionWorldState + CaptureSynchronizedCursor
→ SimulationAiDecisionModule.TryPrepareIndexedCanonical
→ CaptureSharedOwnedSnapshot（已存在Unified/direct canonical）
→ BattleCharacterInputWriter/Store.TryEvaluateCanonicalDecision
→ AiDecisionKernel.TryEvaluateCanonicalInput / TryEvaluateCore
→ 原Indexed最近目标/特殊目标查询 + 有序决策
→ ValidateIndexedCanonicalCommit
→ BattleAiInputWriter.CommitIndexedCanonicalDecision（RNG/input/flow发布）。

实际入口引用：
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs:819-839
- Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs:4363-4387、6308-6331
- Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs:2792-2850、3071-3267、5872-5890
- Assets/NTSD/Scripts/Simulation/Ecs/Stores/BattleCharacterInputStore.cs:141-169

## 本次排除的假设及原因

| 假设 | 当前证据 | 本轮决定 |
|---|---|---|
| 每个角色会复制完整世界快照后再决策 | Shared/unified rows已有共享；direct路径直接以in inputs[slot]传入kernel，非重建全世界；2792-2850、Store146-169 | 不提出重复建设direct canonical或全世界快照缓存 |
| AI随机数每次复制整张表 | NTSD28NativeRandom.cs:179-205中cursor持有byte[]引用及原始/当前标量游标；Next218-234推进标量，不复制表 | 不虚构随机表复制瓶颈；调用顺序/次数/随机值不能减 |
| 每角色运行完整索引自检 | AiSensingKernel.AreIndexesReady:838-841只是3个就绪位；ValidateIndexedContract是另一个方法 | 不把布尔就绪判断说成全量扫描 |
| 生产测量中还跑FullScan oracle | 本次复用CPU诊断首OFF原report中aiDecisionIndexedCanonicalFullOracleSampleCount=0；生产分支3131-3135受显式observer/sample gate控制 | 不通过关闭本来就未运行的oracle声称收益 |
| 有缓存目标就能跳过最近目标查询 | kernel查询先生成bestDistance/sameZLane等事实，259-301再执行缓存目标保留及RNG；后续special继续使用nearest事实；Sensing247-289中Air可覆盖slot但保留Ground facts | 不删除整次nearest；否则会改变事实/RNG/目标行为 |
| 每角色重复读取World所以整份WorldState可在tick外缓存 | World6308-6331包含7个Flow字段；BattleAiInputWriter48-55每角色提交更新，后续角色可见 | 不跨角色缓存整份WorldState；静态stage字段能分离不等于有可辨收益 |
| 每次输入提交必然重复发布AI行 | Store503-520已比较history gate/cachedTarget/coordinateX，未变化已有skip；writer同时承担Runtime镜像/flow/RNG提交 | 不把同名commit/sync当可整体删除的重复写 |
| 随意删slot绑定/代际校验可省整段 | Store476-489是常数范围/owner/generation检查；commit和snapshot还有不同身份/epoch责任 | 无足够收益证据，不减安全检查 |
| 从12ms可以直接推算可优化空间 | 12ms为旧同窗细分计时经过时间；查询/kernel/决策为父子关系，计时本身进入父段；production diagnostics disabled时Active getter返回null | 不承诺12→某目标毫秒，不把去计时当生产优化 |
| 在上个cursor候选继续换一种算术写法 | 本版唯一局部ABBA两个布局均无可辨收益；未减少query/候选遍历/业务过滤 | 结束本版，避免无证重复微调 |

以上是源码/已有文件事实及其方案判断，不是新运行时收益测量。源码指纹见source-fingerprints.json。

## 同一既有窗口的成本边界

复用：artifacts/diagnostics/NTSD-H07-INPUT-SPIKE-CPU-DIAG-20261010/cpu-analysis.json 与 windows/00-combat1000-phase-cpu-off/report.json。
- CharacterInput约11.996ms；RemainingAiDecision约4.961ms。
- IndexedCanonicalKernel约3.115ms，其内NearestSearch约1.540ms、SpecialSearch约0.145ms。
- Capture约0.205ms、CommitValidation约0.160ms、CommitApply约0.469ms。
- 上述是既有细分经过时间；不能父子相加，不把残差认作某方法独占CPU。没有新的无细分whole-input收益证据。
- 原完整Driver GC失败仍保留。本轮不把它当所有静态分析必须停止的原因，也不伪造通过。旧compat ON未测、producer暂缓、95OFF保持。

## 尚未证实的事项

1. 决策剩余计算中哪一段实际占用可重复的CPU成本、能整段移除而非仅减少少量表达式，尚未形成源码与成本闭合证据。
2. 测得的elapsed波动分别由业务、计时和调度贡献多少，未查清。源码中未发现大循环不能证明无性能问题。
3. 按当前遍历/发布合同，跨角色共享某项查询结果或跳过整段写入的有效命中率与失效证明不足。不能先新建缓存再找理由。
4. 本轮不是对全AI/所有角色/native sampled-state的穷尽审计，不能将本轮未找到推为不存在。

## 何时可以形成并派发完整方案

下一方案必须同时具备：
- 明确原方案内的一段真实多余工作，精确调用入口和当前实际生效条件；写明原次数/访问和将消除的部分。
- 对所有读写所有者及角色间、producer/routing可见性给出闭合解释；保留同tick状态、目标/tie、双RNG、fullchecksum和fallback。
- 与producer、95、compat、cursor旧候选有真实差异；没有就保持不实施，不能另起名重试。
- 准确最小文件/符号范围、生命周期/容量/回退方案；不暗含Q06、资源或规则变更。
- 测量前给出固定收益判据与有限停止条件；局部证据与完整输入/Driver及设备验收分开；0GC只作为对应范围正式资格条件，不能替代性能目标。

当前这些条件未全部满足，因此本轮交付的是明确的审查结论和剩余证据缺口，不是可执行生产优化指令。没有产生execution-handoff或派发回执，不让执行会话盲改。H07待方案，不将本轮文档工作写成优化完成。

## 范围纪律及偏差记录

本轮读取AiDecisionKernel连续片段时，意外包含1065-1108相邻方法，其尾部带Q06 persisted-alias标记。已在讨论会话披露并停止沿该方法追查；未使用其实现设计新候选、未进入其它Q06方法、未以此扩权。以后先按符号/标记划定范围再读取，不能重复展开该段。

其余边界保持：仅原优化方案；producer USER_DEFERRED/OFF、compat/95OFF、H06不续，Role默认与FindEffect既有证据；33ms/drop0/backlog/Android保持开放；publication只读、透明segment/fail-closed、确定性、容量/0GC、有序关闭不变。无源代码/资源写入、无Unity操作、无Git丢弃/提交/推送。
