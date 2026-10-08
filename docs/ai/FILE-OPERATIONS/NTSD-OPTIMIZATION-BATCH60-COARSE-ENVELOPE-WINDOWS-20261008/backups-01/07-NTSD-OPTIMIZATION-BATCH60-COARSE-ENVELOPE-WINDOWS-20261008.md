# 第60批：保守包络真实 Windows 千人收益对照

状态 READY / IMPLEMENTATION_NOT_STARTED。来源：58一次局部cost信号＋59完整Driver8/8/88配对/176tick资格通过；此Task冻结必要下一动作，不计已执行批次，没有本批C#、Unity或性能测量。

## 目的与不变范围

回答现有普通Brute保守包络在真实Dispersed1000/Combat1000中是否降低逻辑/collector成本，并确认完整Driver逻辑GC、实际应用、既有关闭和正确性门。不是新候选或再跑59/局部cost；四production flags true，envelope OFF/ON，kind5/eligibility/rejected-binding/coarse-dispatch/timing均false。不自动切生产默认、collector/backend、EXT1/Mono/ATLAS，不缩33ms门或正式120+1800门。

唯一拟写C#：Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs，复用既有SuiteState/RunState/BeginSuite/BuildCurrentRequest/StartCurrentRun/ObserveRunningSample/OnUpdate/ShutdownAndExit/OnPlayMode及原request/observer/owner；新cold request/apply/restore helper和同文件现有Editor test类，不新建生产系统。写前另建独立Change/Operation/逐文件dirty副本及保护SHA；特别保护公共Temp测试XML。Query58、Admission59、旧fixture、H07GC observer、Harness、Host、renderer、Scene/资源/设置均只读freeze；Q06仅SHA/引用，绝不读活跃body。

## 固定矩阵与请求身份

四fresh windows-01输出：index0 Dispersed OFF、1 Dispersed ON、2 Combat OFF、3 Combat ON，各120warm+180sample。BuildBruteProductionRequest(index/2)原65字段只允许outputPath身份不同；seed/input/roster/spawn/canonicalAI/profile/ForceBruteForce/renderer/resolution/samples保持。不得继承52 eligibility全ON组合，不启用branch timer或任何Profiler/Frame Debugger/GPU/M0。

现有logicGcScopeOnly在outputRoot选择中优先于候选，因此60独立root/BuildCurrentRequest必须在logicGc分支前选择；仍要求bruteProductionOnly＋logicGcScopeOnly，排除labelPrewarmValidation与所有其他候选/capture，不能放宽57的no-candidate guard。固定4个RunState，不沿production默认2窗，也不误写54/57输出。

## 观测与生命周期

- 原6402 Editor必须fresh idle/noPlay/noTest/noCompile、单saved clean Scene；shared pressure request absent/既有terminal owner可核，无并发suite/session/recovery；不启动第二Editor，不覆盖任何旧窗口或清理用户Scene。所有request先CreateNew落盘并freeze，开始后不得热改源。
- StartCurrentRun沿原AreProductionServicesReady与空World门，StartRun后、首logic tick前仅设置当前owned query的envelope，保存旧值；生产四flags仍true且其余candidate/timing均false。OFF和ON都明示设置并记录，无所有权时不得写flag。
- 新RunState记录enabled/flagApplied/flagUnchanged/restored及sampled-observation最大Last directions/rejects。现有ObserveRunningSample约1s观察、并非每tick hook，故该max只能声明抽样观察，不伪造完整累计或300tick应用计数；59已有逐tick应用资格复用。ON千人窗口须实际观察directions/rejects>0，OFF0，观察少于最小现有门则invalid而不是假通过。终态在清空productionQuery前再核flag/noFallback/production机制及保留last计数。
- 正常完成、异常ShutdownAndExit、外部Play退出都先恢复own envelope旧值，再沿原11阶段owner关闭，不改Join/World/pool/Scene序列；helper重复调用幂等，无新worker/cache/Unity对象。域reload中断不自动重启；观察超时先查同handle/Session实际状态。
- 全部300tick沿既有calibrated完整StepOneTick observer，前后空/正例校准、scope覆盖、invalid/overflow/steady event严格门不变；不能用旧uncalibrated raw0覆盖UNKNOWN或减少scope。CPU逻辑observer结果不当H11完整camera0GC或可靠bytes测量。
- 输出包含原actual min1000AI/base roster、120/180准确完成、collector/logic均值与P95、drop/backlog、显示间隔、CPU Profiler已有draw/SetPass口径、原容量snapshot/拒绝/cleanup/flags；缺central unresolved/stale独立门仍UNKNOWN，不从其它0推断。终态snapshot/hash只证明终态，不替代已复用59逐tick或新增全native/全角色证书。

## test-first、验收与出口

先新增有信息量RED：4request只output改变/4 fresh输出、invalid index、模式互斥与root/schema guard、Apply/Complete/Restore DEFAULT-OFF/ON/幂等/owner冲突/flag drift/异常恢复，旧production request/default及受影响已有Suite纯guard必要回归。按实际已存在接口选择最窄case，不重跑59重Driver/cost或旧全campaign。新helpers冷文本绑定/观察热路径不能引入未预热字符串，已有热method literal0检查保持。

纯检查通过并冻结准确源后只启动上述一组4窗。保留失败原件、每个RunState及GC证据；有效OFF/ON尽量同Editor同环境，报告一次顺序窗干扰局限，不把一次非同期百分比当正式长期收益。实际终态比较依赖旧hash身份核验，不自动补测全trace。

如果收益为负/不可辨则NOT_ADMITTED，不盲目重跑找PASS或在此Task堆微候选；按已有真实热点判断同范围更高杠杆动作。若明显有效，仍先记录实际stride/驻留增量（旧17B只是理论字段下界）和必要推广/正式窗口独立附件，不能从本Task推导默认推广权或Android/120FPS认证。

回滚仅本次准确current-dirty备份/反向hunk另批准，不Git reset/restore/checkout/clean/stash、不删除/move/覆盖用户改动。H07/H11 OPEN、阶段4/6、Goal active；59共38已执行，本批READY不计已执行。边界判断阶段是否完成，不按次数停止。
