# 第54批：完整 Driver tick 的可靠 GC 证据

当前：诊断接入 VERIFIED / RELIABLE_LOGIC_GC_MIXED，Windows两120+180已结束，分散steady0event/混战6event(first158)、校准/覆盖有效。性能P95112.550/114.910ms及drop802/789仍FAIL，正式1800未通过，阶段4/6/H07/H11 OPEN。下一必要调用点，不重复同窗；原文PLANNED/过程快照保留。详同ID REPORT/Record及terminal-audit-01.json。

状态：PLANNED / CALIBRATION_AND_SCOPE_PENDING。前一轮是用户状态问答，只读核对，不计优化进展。本包解决 H07 现有计数器已知不响应的问题，不声称帧率改善。

## 来源与范围

用户已批准六项有限首阶段及未达继续；执行合同第0—8节允许必要检测缺陷修复。完整同步主线程 `SimulationTickDriver.StepOneTick` 是选定 logic scope。旧 `GC.GetAllocatedBytesForCurrentThread` 的零读数曾对存活1MiB正对照不响应，52仍 UNKNOWN；复用既有 `BattleScopedGcAllocationRecorder`，不修改它、不删除旧结果。

准确写域：

- Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs：ProductionEntityStressRunner，仅 UNITY_EDITOR 默认空的成组回调，在 StepMeasuredTick 的实际 Driver 调用前后及既有 steady 判定后；不改 tick/采样规则或原字节计数。
- Assets/NTSD/Scripts/Test/Editor/BattleLogicTickGcObserverEditor.cs 及新 .meta：新 Editor-only 固定容量 recorder owner、前后正负校准、单个 pending struct 和数值汇总、聚焦测试。
- Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs：仅新 Batch54 opt-in 双工作负载 family、挂接/收尾/异常和外部退出释放、独立 sidecar verdict；旧 family 与 UNKNOWN 口径保持。
- 五治理文本：CHANGE-LEDGER、STATE、当前 handoff、FILE-OPERATIONS/INDEX、原 optimization progress tracker。
- Temp/NTSD_ProductionEntityStress.result：若通过资格并运行新窗，只允许既有 runner 正常写新终态；先保存现有52 owned终态副本。SharedRequest必须不存在，不生成/删除共享request。
- 新 Task/Record/Operation 和本批 artifacts。无其它写域。

## 生命周期与容量

owner 是现有 Editor suite 的单 run observer，不是 World manager/新战斗服务；只同步主线程，不把该 recorder 证据推广到 worker。默认无回调，Player不编译 Editor hook。attach 必须在首 tick 前、拒绝其它 owner/不完整三回调及 dedicated worker。固定256事件容量，不扩容；饱和/覆盖/未知单位/失配/缺 pending/拒绝tick为无效，不生成假零。只存一个 pending scope，不使用每tick List/JSON/reflection/lambda。

Begin/End 精确覆盖完整 Driver 调用；容量准备、脚本移动、post-tick 统计/写报告、校准均在 scope 外。steady 分类必须复用既有 SamplePolicy 的最终结果（含roster和pool增长），不能仅按 tick序号推测。前后校准各为已知1MiB正对照及空负对照；后置校准在run完成/teardown之后、不在热路径。只 event0可证明选定scope零分配；有事件时按单位报告，不能把时间raw值当字节。

拒绝/异常必须 End 并保留失败；正常、suite abort、外部Play退出都 detach/dispose，同线程幂等释放，不触及有序关闭十一阶段。runner cleanup移除其回调引用，不负责销毁Editor recorder；suite owner负责结束及释放。reload丢owner仍为PARTIAL，禁止自动重跑。

## Test-first 与窗口

先新增反射接口测试（实现缺失有效RED，而非编译失败）：默认空/成组/独占/首tick/dedicated/清理；有效scope、warm/nonsteady不混入steady、已知分配、拒绝tick、缺/重复边界、owner线程、固定容量失效、幂等dispose；新request两个索引及非法索引、65字段除output与原40 request同。再最小实现并运行这些具名影响域检查、已有 recorder 校准相关和 Harness CPU计时/采样相关检查，不重跑53 cost/native/旧Windows。

GREEN后才可原Editor新 Batch54 单菜单运行 Dispersed1000/Combat1000 各120warm+180sample（不是正式1800）；原65字段/1000真实AI/DataOrientedCanonical/Brute/33ms/max2/dispatch/central不变，四已准入production flags ON，38/48/49/53等候选OFF，不启动分项计时。新窗唯一问题是可靠 logic GC，不是旧52组合收益重采。每tick完整性需与report count、steady count、成功end相等，前后校准和所有terminal路径有效才能判可靠，窗口失败保留；与性能达标分开。

## 验收与回滚

原Editor编译/聚焦与 recorder校准、准确scope/生命周期、两个窗口及 sidecar 的真实结论分别汇报。关闭残留、Scene磁盘SHA/dirty、原Menu恢复、保护源与HEAD、dirty备份以及 ChangeLedger/diff-check必须检查。H07 P95/drop/实际FPS按本次实报，GC通过不代替性能通过；H11迟发12event仍开放，阶段4/6保持直到真实条件满足。

保持正式336/D023、33/3/max2、checksum/RNG/pass/input、publication只读、排序/segment/failclosed、容量及十一阶段关闭。禁止Scene/Prefab/assets/importer/ProjectSettings/InputActions/Gen/Plugins/Server写入，不读Q06方法体，不推广collector、不解冻EXT1/Mono/ATLAS。

回滚来源为 Operation 八份事前准确内容及新文件独立清单；本包不自行删除或破坏性恢复。先保留新增证据，再请求准确恢复授权（若需）。候选/工具完成不是Goal结束，继续未满足真实阶段条件。
