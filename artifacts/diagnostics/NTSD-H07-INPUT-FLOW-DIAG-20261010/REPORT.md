# H07角色输入完整流程补充检验

Task：NTSD-H07-INPUT-FLOW-DIAG-20261010  
Change：NTSD-H07-INPUT-FLOW-DIAG-001  
状态：SCOPED_DIAGNOSIS_COMPLETED_WITH_LIMITS / NO_PRODUCTION_HANDOFF  
诊断工具与本次采集：VERIFIED；完整Driver稳态0GC：FAIL；尖峰根因：UNRESOLVED。  
授权：用户要求“先全部检验清楚后，再交给另一个会话去执行”。本轮由讨论会话检测，没有发送实施任务。

## 当前结论与交付边界

1. 原提议的兼容外壳确实在这个千人AI夹具中每帧读取/复制17字段，再发现没有变动而跳过提交。当前180个稳态tick共180000次skip、0次commit；上轮相同事实复用，本轮再次可直接对账。它是有代码与调用证据的重复工作，但并非整个约12ms输入阶段都可省掉，也没有新优化后收益。
2. 提前守卫必须同时检查native输入管线所有权、exact AI路径、DataOrientedCanonical profile、有效Runtime与本World writer的canonical绑定。CanEvaluateCanonicalDecision只判断绑定；Legacy profile下它也可能true，不能单独替代TryCaptureProgressState的完整准入条件。新增focused已证实这一差别。
3. 生产者输入冻结、producer后的快照、代理应用、native按键/组合技/动作路由以及routing后的发布是不同职责和可见边界，不能一起删除或合并。producer复用仍USER_DEFERRED/OFF。
4. 输入后段现有计时已补齐，当前两次GC.Alloc已由本次raw调用栈定位；但耗时尖峰不是“已经全部查清”。查询访问数量、提交数量、这两次分配，均不足以单独解释全部尖峰。没有把系统抢占/降频/GC停顿写成已证原因。
5. 本轮为有Profiler扰动的一次取证，不是生产A/B。按用户要求仍不交实施会话；H07-INPUT保留在方案/诊断阶段，不升格READY或宣称最终性能/Android通过。

## 本轮实际范围与运行

只编辑两个Editor工具文件：

- Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs：新独立冷菜单、一次请求/状态持久化和INSTRUMENTED证据标签；相对任务前dirty字节25增/4删。
- Assets/NTSD/Scripts/Test/Editor/H07InputCostDiagnostics.cs：复用原GC observer与CPU捕获器，固定300行、42槽时间与访问数、300个冷预建tick marker、同raw冷解析、4个guard/request focused；214增/7删。

默认不开启；没有生产World/Query/AI/writer/动作callee改动，没有读取活跃Q06方法体。调用栈中的方法名/路径/行号只是原raw的元数据，没有展开方法体。

原Editor PID39108/TCP6402/Unity2022.3.62f3复用，没有第二实例。unity-cli status返回STATUS_NO_INSTANCES不能用来判断Editor关闭；本轮以原进程、原桥和live状态确认。编译后桥短暂重连不是生产失败。

实际执行：

- refresh_unity(mode=force, scope=scripts, compile=request)：原Editor reload后idle，无CS编译错误；compile-state-01.json、compile-console-01.json。
- run_tests(mode=EditMode, testNames=具名8项)：job 885c283979eb4ac180eba04de638e08e，8/8 PASS。10597只是发现总数，不是运行数。请求与最终结果分别为focused-request-01.json、focused-result-01.json。
- execute_menu_item：NTSD/Validation/Optimization/H07 Full Input Flow Diagnostic。
- 唯一Role/DataOrientedCanonical/1000 Sakura/combat1000/seed1314149188窗口：120warm + 180steady，完整300tick；没有第二窗、其它布局或1800证书。
- execute_menu_item：NTSD/Validation/Optimization/H07 Parse Retained Input Flow Capture。只向现有Profiler历史追加加载同一raw，没有清历史、重采集或重跑实景。
- get_editor_state、manage_scene(get_active)：最终idle/nonPlay，原NTSD_Menu clean，8roots。

当前磁盘request与上轮首OFF request逐字段仅outputPath不同（actual-request-comparison.json）；诊断owner配置不同：本轮全部输入边界细分ON并开启Profiler，不能因此称两次测量配置完全相同。producer/95/Role-record/旧六分支均OFF。input-cost.json中实际owner的boundaryEnabled/fullFlowDiagnostic及300行actualFlagFailures=0为本轮事实；observation.json中旧Suite输入边界子实验字段false不是新helper的实际开关证明。

## 完整流程及当前计时

以下均为180steady、1000AI合计每逻辑帧的Stopwatch经过时间，单位ms。176/180稳态tick记录到Profiler enabled，只有121—124未开；这不是普通生产配置成本或独占CPU时间，不能跨运行当优化收益。父子包含关系不能相加。当前CharacterInput父段均值14.699981，P95 25.144445，最大45.965400；EntityInputPass均值14.248751包含下列大部分阶段。

| 现有职责 | 均值ms | 中位数ms | 最大ms | 包含关系/解释 |
|---|---:|---:|---:|---|
| AI决策 RemainingAiDecision | 5.819874 | 4.978400 | 23.232600 | 在输入父段内 |
| canonical数据捕获 | 0.234081 | 见JSON | 1.080800 | 在AI决策内 |
| AI规则kernel | 3.612408 | 3.147400 | 13.340400 | 在AI决策内 |
| 最近目标查询 | 1.752419 | 1.573200 | 9.234500 | 在kernel内 |
| 特殊目标查询 | 0.157779 | 见JSON | 0.368900 | 在kernel内 |
| 决策提交校验 | 0.185753 | 见JSON | 0.799200 | 在AI决策内 |
| 决策应用 | 0.577930 | 见JSON | 1.505900 | 在AI决策内 |
| 兼容ComboUpdate外壳 | 0.577643 | 0.469050 | 4.561500 | 不等于native组合技业务成本 |
| 外壳progress捕获 | 0.093653 | 见JSON | 0.572300 | 在兼容外壳内 |
| 外壳ComboDirect守卫 | 0.055304 | 见JSON | 0.166500 | 在兼容外壳内 |
| 外壳Release守卫 | 0.034896 | 见JSON | 0.344900 | 在兼容外壳内 |
| 外壳compare/skip提交 | 0.120843 | 0.089600 | 3.619700 | 在兼容外壳内；本fixture无实际提交 |
| producer输入冻结 | 0.328127 | 见JSON | 0.896100 | 保留输入冻结边界 |
| producer后Runtime快照刷新 | 1.251857 | 1.039950 | 5.003400 | producer复用暂缓，不由本轮重开 |
| 输入代理应用 | 0.062709 | 见JSON | 0.680900 | 冻结之后 |
| native sampled-state后段 | 3.621347 | 3.049100 | 9.844200 | 后续真实消费，不是兼容外壳 |
| AI可见行发布 | 0.314834 | 见JSON | 2.281600 | 发布时点保留 |

NativeSampledState内部八段均有现有槽证据：preprocess 0.536554、combo route 0.186294、three-button 0.148648、direction 0.314161、builtins 1.288263、legacy projection 0.227301、canonical AI sync 0.403216、input state sync 0.099494ms均值。它们是native父段内部子段，不能与3.621347ms再加一次。42槽是固定schema；为0的未走分支或没有显式call计数的槽，不证明相关业务从未执行。

计时与计数的scope均来自同180steady，nearest访问3509191，不使用300warm+steady累计除180。原始300行、汇总分位数、峰值tick在report.json.input-cost.json和diagnosis-summary.json。

## 守卫策略核验：已明确什么，尚未实施什么

当前调用：

- BattleEcsCharacterInputPass.TryExecute：仅exact LF2Character且AiControlled进入data-oriented输入路径；其它类型/玩家保持兼容路线；调用ApplyFrameInputFromRuntimeProgress之后仍执行frame-motion tail。
- BattleCharacterInputActionResolver.ApplyFrameInputFromRuntimeProgress：读取17字段，保存original，调用ApplyFrameInput，再调用CommitResolvedProgressState。
- ApplyFrameInput：native管线拥有时不执行这里的combo/direct/release业务；原真正native后段仍存在。
- BattleCharacterInputWriter.TryCaptureProgressState：先检查DataOrientedCanonical，再store.TryCaptureProgress。
- CanEvaluateCanonicalDecision：仅转到store.TryResolve，未检查profile。
- Runtime版store.TryResolve检查slot边界、store generation非0、ReferenceEquals(owner,runtime)。不能误写成它比较了runtime generation；handle重载是另一合同。

新增4项focused覆盖：正常绑定/注销/重用后的新绑定、null/借用他人slot的错误Runtime身份、Legacy profile的反例、唯一request和拒绝其它sequence；原4项输入测试覆盖无变更提交skip、direct字段留给native第二pass、registered canonical消费提交、计时关闭/下一tick清槽，均通过。

可讨论的最小优化仍只是在既有exact AI入口中，把“已知本路径不会改变progress”的判断提前到17字段捕获之前：

| 条件 | 为什么必须保留 |
|---|---|
| exact LF2Character、AI、当前DataOriented输入pass | 避免影响玩家、未知派生和Legacy调用者 |
| entity注册World与执行World一致，native输入管线确实拥有路由 | 避免跳过真正需要的兼容业务 |
| DataOrientedCanonical profile | 已有反例证明仅canonical绑定有效不够 |
| runtime/writer非null，writer绑定解析有效 | null/注销/slot身份错误必须走原逻辑 |
| 只跳兼容progress外壳 | 不跳native消费、tail、发布和后续AI可见边界 |
| 维持诊断skip语义及返回值语义 | 不把内部计数/调用者观察静默改变 |

共享resolver另有LF2Character/LF2Entity生产调用点，不能全局无条件早退。上表只是方案边界；没有实现新的guard，也没有证明实际收益。未来候选还须自己的改前/改后正确性、受影响玩家/派生/Legacy/fallback反例、完整Driver0GC和无解释整tick回退检查。本轮不提前把这些未来实施验证写成PASS。

## 当前完整Driver分配：本次raw直接证据

校准前后已知正分配/空边界均通过；300begin/end/accept、180steady、invalid0、sequence/rejected0。稳态2 GC.Alloc，tick130/162；140 nonsteady事件未被宣称0。Recorder单位TimeNanoseconds，raw总283300，不能写成283300B或GC停顿。

| tick | raw主线程帧 | GC metadata bytes | 直接Profiler父样本 | 首个resolved managed frame | 逻辑位置 |
|---|---:|---:|---|---|---|
| 130 | 93 | 36 | Mono.JIT | BattleDamageWriter.RecordAlternateLeadSound | CharacterHitConsumePostInteraction，输入之外 |
| 162 | 109 | 30 | Mono.JIT | BattleCharacterActionWriter.RouteNativeRunning | CharacterInput/EntityInputPass之内 |

两次66B来自当前raw的GC.Alloc metadata，不是Recorder时间换算。只有这些精确事件得到当前栈定位；未确认对象具体类型，不称它们是业务每帧反复new，也不声称已修复。不能仅因为tick相同就把旧OFF/ABBA/95的历史事件追溯成同一来源。

live八帧回调0/PARTIAL原件保留；cpu-gc.raw实际7412092B。只在同raw选出两个含稳态Driver GC.Alloc的主线程帧，恢复文件133969697B；128线程扫描上限命中，不声称全线程完整。cpu-gc-recovery.json的allocationCountsMatch/allocationCallstacksPresent为旧捕获器未重计算字段，不能引用它们证明恢复成功；本轮逐事件从父tick marker/主线程/bytes/调用栈与Recorder两个tick独立对账，见current-gc-callsites.json。

本窗标识INSTRUMENTED_CALLSITE_ONLY / CALIBRATED_FULL_DRIVER_NONZERO_EVENTS，zeroAllocationPassed=false。Suite PARTIAL/DONE/completedRuns1是完整单窗后的有限资格失败停止；实际runner StoppedCleanly、300tick完成，不是没跑出数据。没有ON/OFF收益比较，也没有过滤Mono.JIT或降低0GC门。

## 全流程浮动：解释到哪一层

- 查询访问量确实随状态变化：每tick16641—22451，median19368，mean19495.505556；每帧1000次nearest，总体平均每次约19.5个被访问row。现有索引查询不是每AI固定扫1000人；访问row不等于全部查询成本或GPU概念。
- 访问量最大/最小约1.35倍；nearest最大tick136为9.2345ms（约5.87倍median），却只访问17852，低于median，Driver allocationEvents0。因此仅凭row数量增加不能解释最慢tick。
- tick258有progress compare/skip 3.6197ms、producer刷新5.0034ms、compat外壳4.5615ms、AI18.0816ms，但仍0commit/1000skip、该tick Driver allocationEvents0。不能把峰值解释为发生了更多实际写回。
- 上轮tick291 AI35.213ms，本轮同完整RuntimeChecksum/输入/双RNG基线的tick291 AI4.9368ms；本轮AI峰值改在226。上述状态校验量一致而峰值位置变化，说明目前没有“固定该逻辑帧必然产生相应峰值”的证据；Profiler/运行顺序/系统争用等变量仍混杂，不能反过来称系统争用已证。
- 当前两次JIT分配不在上述最大峰值tick，不能拿66B解释全部尖峰。

**仍未知：**业务CPU计算增加、线程被调度暂停、Profiler/细分时钟开销，以及运行时pause各自对这些峰值贡献多少。目前采的是经过时间、有限业务计数和分配事件，没有同一帧主线程CPU时间/调度或pause证据；没有测频率/调度就不能宣布抢占/降频。

这不是新增优化点。后续若继续确认尖峰，应只解决“同tick时间增量发生在CPU执行还是等待/工具扰动”这一缺口：先复用当前raw能提供的对应tick样本；不足时才设计同一H07 fixture、相同观测口径下的经过时间与主线程CPU时间/pause对账，明确开销和失败出口。单纯再跑相同一窗、拉长warm或全量回归不能给出该因果区分，不能重复找PASS。本Task的一窗额度已用完，本轮没有自动追加该实验；也没有把未知转给实施会话自行猜改。

## 状态、容量、关闭与保护

- 300tick完整RuntimeChecksum、输入数量及双RNG等14字段与上轮首OFF全同，0差异；末态不替代这项逐tick证据。
- 300000个exact AI输入pass，compatibility/unity shell/unexpected fallback均0；实际开关失败0。
- workloadValid、inputCostObservationValid、harnessValidity、teardownRestored均true；roster/变更/池扩展拒绝与broadphaseAbortedTicks均0，runtimeSlotCapacity1050。不能把这次夹具证明扩成全部容量边界测试。
- Suite orderedShutdown=true，remainingObjects/Slots/Borrowers均0，原Menu恢复，Menu与Battle Scene哈希不变。
- 15565项保护清单中，仅2个事先声明Assets文档有计划内变化（当前handoff、唯一进度）；其余15563项匹配，unexpected/missing/newOutsideTwoScripts均0。原protected-before.json完整保留，没有重写前清单掩盖文档变化。
- 10项dirty原字节备份SHA全部匹配。两Editor变更与文档增量按Operation留痕，没有删除/移动/回退无关工作。
- 本次有Profiler/输入细分扰动，droppedBacklogTicks542；不满足H07性能证书，不报告正常33ms/drop0/无持续backlog通过。Android无设备证据。
- 最终Ledger和diff-check结果见下方交付追加；未受影响证据复用，不重跑旧全套。

## 证据索引与有限出口

- focused-request-01.json / focused-result-01.json：具名8/8。
- windows/00-combat1000-input-on/report.json及其.input-cost.json、.logic-gc.json、.observation.json、.request.json、.terminal.txt。
- diagnosis-summary.json：180steady统计、300tick14字段对账。
- windows/capture/cpu-gc.raw、cpu-gc-state.json（原PARTIAL）、cpu-gc-recovery.json、cpu-gc-samples-recovered.json；current-gc-callsites.json为窄调用栈摘要。
- actual-request-comparison.json、audit-summary.json、protection-result.json、protected-before/after.json。
- scene-before-window.json / scene-after-window.json / scene-after-parse.json，editor-final-state.json。
- 两源码相对Operation before的diff快照；Task、Change及Operation原dirty备份。

本轮完成的是有边界的诊断和事实整理。兼容外壳方案边界更明确，当前分配栈已定位，输入后段成本已有分段；尖峰根因尚未充分证实，未满足用户“全部检验清楚再交实施”的要求，故NO_PRODUCTION_HANDOFF。不存在已批准的生产修复或新优化点。Role既有默认与FindEffect成果保留，producer暂缓、H06不续办、95 OFF、旧Goal取消、34项队列不恢复。


## 最终交付检查
2026-10-10原工作树执行Tools/Validate-ChangeLedger.ps1：exit0 / Change ledger validation PASSED / Records1402；历史Record声明不在当前diff中的警告保留于change-ledger-validation.log，没有新增未覆盖脚本错误。git diff --check：exit0，仅既有/已声明文档LF→CRLF提示，无whitespace错误。两条检查都不等于性能/0GC/Android通过。
