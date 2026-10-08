# 第44批：普通 Brute 分支成本诊断

最终：SCOPED_DIAGNOSTIC_VERIFIED / COST_ATTRIBUTION_PERTURBED，四短窗已结束；仅诊断出口闭合，H07性能/可靠0GC未过。低频诊断必要性由OFF/ON collector差35.295/11.034ms产生，下方IN_PROGRESS为历史声明，不代表窗口仍运行。完整证据见本批REPORT和Record；未改变六项完成门。

状态：IN_PROGRESS。来源：有限首阶段合同第0—8节及用户要求未达继续优化。

## 范围与问题
第40批 collector 平均50.255/52.672ms、logic83.435/85.775ms；现有 PairExactLoop 包含几何、binding、pair allowed 和精确ITR/body工作，旧汇总不能回答其成本比例。仅增加默认关闭的细分计时及同代码OFF/ON对照，不新增优化算法，不计FPS收益。

## 准确写域

2026-10-07必要补充第五脚本：Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressDetailTimingEditorTests.cs，第303行旧报告Count44同步为48，保留warmup/所有既有断言。202完成的首回归只有此schema期望失败；先before-supplement/backup11，再改。原四脚本/其余红线不变。
- Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs：默认关闭的 EnableBruteBranchTimingForDiagnostics；CollectCandidatesForPairCached既有geometryFirst分支的四独立计时。
- Assets/NTSD/Scripts/Simulation/Diagnostics/SimulationWorld.DetailTimingDiagnostics.cs：既有phase id 0—43保持，追加44—47四名称，Count48；沿用RecordPhaseElapsed，禁止每pair Profiler Begin/End。
- Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs：phase/test-first、默认关闭、candidate顺序/RNG、关闭计时零值与开启覆盖。
- Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs：独立44菜单，四窗口Disp OFF/ON、Combat OFF/ON，同40请求仅输出路径变化；诊断flag每次恢复，normal与abort均在World shutdown前恢复。
- 仅更新原tracker、H07文档、Ledger、STATE、handoff、FILE-OPERATIONS/INDEX；新Task/Record/本Operation/诊断报告，不另建总表。

## 红线与生命周期

恢复时点更正（重扫实际调用链，不改代码）：正常sample-complete由runner StopAndCleanup先CleanupInternal并产出terminal，Suite随后恢复诊断flag、下一运行前清引用；Suite abort/最终owner shutdown入口先RestoreBruteBranchTiming再ShutdownBattleRuntime。故下方“normal与abort均在World shutdown前恢复”过宽；实际合同为正常终态后、下个run前恢复，abort/owner shutdown前恢复。诊断标量无新owner/worker，十一阶段不重排，旧运行恢复证据仍有效。
正式336权威、33ms/3ms/max2、collector四生产默认/算法、RNG/rest失效清理、命中/OPoint/声音、publication/渲染/排序/segment/fail-closed、十一阶段关闭不改。无新owner/worker/lease；只既有world诊断容器准备时增加四固定计数。Q06只hash。无Scene/资源/Settings/Server/Plugins/Gen写入；不解冻EXT1/Mono/ATLAS/Role-aware推广。

## 验证与解释
测试先RED(反射缺字段/API、phaseCount)再GREEN；相关旧CandidateCollectDetailTiming、DetailTiming、Suite必要回归。固定1000 active AI、seed0x4E545344、120warm+180sample、普通Brute、既有renderer/profile，OFF/ON workload request除output全同；同工作负载末snapshot/RNG/hash对照。新phase只geometryFirst普通Brute且flag和recorder开启时记录；四子段不重叠，父PairExactLoop含全部与诊断开销。instrumentation OFF/ON开销必须如实报告，差值不是纯函数成本/优化收益，phase总和含嵌套不得当整tick时间。
禁止Deep/CPU capture/GPU/设备测量，Play期间不MCP观察，仅文件和原PID。可靠0GC并未由本批counter认证，旧H11 FAIL保留。适用门不通过不推广、不无限复跑。

## 操作与回滚
10当前文件逐字节备份已核相同，before.json记录SHA/Git；原PID19040、Menu单Scene8roots clean/idle已核。新目录CreateNew，不覆盖旧报告。旧SharedResult属于40且已保存原报告；既有runner允许覆写该自有terminal，事前另存准确副本/sha；SharedRequest必须不存在，不写或删除用户request。
脚本全apply_patch；若回滚须用户批准并按新Operation精确恢复本批差异，不能reset/checkout整文件或覆盖后续用户工作。

## 出口
取到有用残余热点因果证据并确定下一最小动作；不改变H07性能门，不把诊断通过称优化完成，不因次数结束Goal。H07/H11 OPEN、Goal active。Record NTSD-OPT-H07-BRUTE-BRANCH-TIMING-044；Operation NTSD-OPTIMIZATION-BATCH44-BRUTE-BRANCH-TIMING-20261007。
