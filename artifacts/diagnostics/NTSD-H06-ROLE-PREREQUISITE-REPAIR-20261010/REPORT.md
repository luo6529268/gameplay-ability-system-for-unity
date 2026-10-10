# H06 Role Editor前置修复：资格仍失败，有限任务停止

Task `NTSD-H06-ROLE-PREREQUISITE-REPAIR-20261010`：**FAILED_GC_QUALIFICATION / FINITE_TASK_STOPPED**。
Change `NTSD-H06-ROLE-PREREQUISITE-REPAIR-001`：**BLOCKED**。A状态诊断门修正已验证；B两个准确方法的无业务调用准备入口已实现并实测可返回，但不足以通过完整Driver 0GC，不能声明冷JIT缺口已修复。代码只在具名Editor入口生效，不进入生产默认。七段成本未恢复，无后继自动任务。

## 解决了什么，以及仍缺什么

旧RoleRecordTickObserver要求fallback participant总数为0，会把无ITR/BDY的inert对象误拦。本轮v2门保留原开关/stride64/旧六timer/Role/abort/steady校验，bit12改为既有Getter返回的真实fallbackAttackOrdinals与fallbackBodyOrdinals都0；新增bit15要求总数与非负inert数量相等。两者必须同时通过，不能仅凭两数相等放行。逐tick保存mask、timerMask、实际collector/abort及四项fallback/inert数量。旧取证入口保持v1原总数断言和instrumented非certificate逻辑，旧报告不改写。

冷准备仅通过反射定位：
1. `NTSD.Simulation.Ecs.BattleDamageWriter.RecordAlternateLeadSound`（private static）；
2. `NTSD.Simulation.Ecs.BattleCharacterActionWriter.RouteNativeRunning`（private instance）。

首Driver tick前分别调用`RuntimeHelpers.PrepareMethod(method.MethodHandle)`及`MethodHandle.GetFunctionPointer()`，没有MethodInfo.Invoke、delegate调用、实例构造、实际攻击/跑动预演或全程序集准备。丢失/开放泛型/抽象目标拒绝；API异常在observer attach和首tick前抛出，不继续游戏。没有增加warmtick、过滤Mono.JIT事件、调整Driver GC/计时边界或读取生产方法/callee/Q06方法体。

**新资格窗仍有2次稳态GC.Alloc。** 这证明当前准备方案不足以满足资格，不证明API没有执行，也不证明事件仍在那两个方法。新窗没有Profiler/调用栈，只有firstNonzeroSteadyTick=130；第二次具体tick、调用点与对象类型未知。先前DIAG的130/162、36B/30B、Mono.JIT归因只属于其原raw；不追溯覆盖本次、95或旧ABBA。没有额外捕获、第二窗或改生产逻辑寻找PASS。

## 原Editor验证

最新Editor编译通过，两个源时间早于Editor DLL，最新script reload后CS errors0，见compile-evidence.json。原Editor39108/TCP6402，Unity2022.3.62f3；开窗前idle/Menu clean8/无测试，无第二实例。

Focused job `5be3b7010c004bdabc6c6fd90b53059f`：**10/10 PASS**，详focused-start/result-01.json：

- 实际三实体fixture的inert-only集合通过；ordinal列表分别注入有效注册参与者的attack/body ordinal后都被bit12拒绝，即使总数仍等于inert。它们是受控列表故障注入，不冒称自然geometry触发，也不是只mock两个数字。
- 既有input validation hook造成真实abort，bit11拒绝；开启89错误flag，bit4拒绝。
- 静态与实例sentinel准备不执行方法/构造器；缺失/开放泛型目标安全失败。
- 两个真实目标准备前后完整World RuntimeChecksum、Rng状态/计数无变化；实例方法无需创建实例。实际MethodInfo元数据与module version保存于资格证据。
- 资格request除新output外与原OFF配置完全相同，拒绝-1和1额外窗口。
- 原唯一GC observer的默认空hook/幂等detach、拒绝第二owner/迟attach两项重新通过。

未重跑旧44/6/12或四场景全套：没有生产规则/候选构造/消费变化，原候选语义证据未受影响；不把旧测试历史当本次全跑。原GC observer本轮只读复用，没有校准或热边界改动。原业务业务函数未调用做预演，完整checksum与最终14字段对照是补充状态证据，不把GetFunctionPointer成功称为已消除JIT分配。

## 唯一实景资格窗

独立menu `NTSD/Validation/Optimization/H06 Role Prerequisite Qualification`，仅运行一次。Suite字段`rolePrerequisiteQualification=true`、`roleRecordPrerequisiteDiagnostic=false`、runs.Length=1；未经过原callsite-only强制非certificate路径。Sakura1000 combat1000/AI/Role/DataOrientedCanonical，seed1314149188，120warm+180steady，正常33ms/catch-up合同不变。record七段/旧六分支/输入细分/producer/95/84/89/92/94 OFF。没有Profiler recording、callstack capture或额外tick marker，qualificationNoProfiler每tick为真。

| 检查 | 原始结果 | 结论 |
|---|---|---|
| v2实际诊断门 | 300tick failureMask全0；inert非零恰113tick；真实fallback attack/body全0 | A修正通过，保留真实fallback拒绝 |
| 完整逐tick对照 | 与前置DIAG300行14字段4200比较项0差异；input数量/fullRuntimeChecksum/双RNG状态计数与同步表均一致 | 不是只比较末态或小checksum |
| 冷准备 | 两个准确method、metadata token100671541/100671391、module2e408eec-6c16-42f8-8e8d-23efd28e7297；completed=true | 入口可运行，效果由GC门裁定 |
| 校准 | before/after正控1MiB各1event；empty各0；capacity256，无wrap/饱和 | 有效 |
| 同步完整Driver覆盖 | begun/ended/accepted300，steady180，warm120；invalid/rejected/sequence0 | 有效 |
| 稳态分配 | **2event，first130，raw900，unit TimeNanoseconds，provenBytes=-1**；warm另139event | **0GC FAIL**，不是900字节/回收停顿；没有忽略任何稳态事件 |
| workload/容量 | active AI/base roster1000；容量180scope、violating0、critical/reject/drop/growth增量0 | 通过 |
| 关闭 | orderedShutdown=true；objects/slots/borrowers0，cleanupExceptions0；设置/driver/logger恢复 | 通过 |
| 持有容量 | inactive pool10→1050，活跃借用0 | 正常预热容量保留，不宣称释放全部内存 |
| tick统计 | 180sample均值50.256139ms、P95 90.65848ms、P99 112.578151ms、max132.1075ms | 直接读取报告，无性能收益结论 |
| cadence | maximum backlog0，droppedBacklogTicks552 | 不满足H07正常dropped0，不能宣称33ms通过 |
| Editor | 回原NTSD_Menu、clean、8roots、idle/nonPlay/noncompile/nontest | 恢复通过 |

Suite原件为phaseDONE/statusPARTIAL/completedRuns1/runIndex0，error明确失败停止；roleRecordObservationValid/workloadValid/teardownRestored=true，zeroGcPassed=false。runner terminal PASS只表示干净跑完，不覆盖GC失败。record计数与七槽300行全0，不解释为业务成本0。

证据：windows/suite-result.json；windows/00-combat1000-off/report.json及其request/logic-gc/record-cost/final-checksum/observation/terminal；qualification-summary.json含14字段逐项独立复核；pre-window-fingerprints.json及final-protection.json。

## 准确改动与生命周期

仅两代码文件，相对本轮真实dirty before的补丁见Operation task-source-diff-v1.patch：

- `BattleOptimizationWindowsAiSuiteEditor.cs`：新具名资格menu/start/state字段、一窗request/output分支；原唯一RoleRecordTickObserver构造中加载已冻结300tick基线/冷准备，Accept记录v2逐项门及与基线的完整14字段比较。原取证保留v1及Profiler限制；正常游戏默认不变。
- `H06RolePrerequisiteDiagnostics.cs`：QualificationGate/ReadQualificationGate、BuildQualificationRequest、ColdPreparation/PrepareMethodWithoutInvocation/PrepareCapturedMethods；新8个case在既有Editor测试区，旧诊断/retained parse与capture实现保留。

固定状态owner仍是原Suite/observer：300row证据/基线冷分配，GateObservation为值类型；ColdPreparation只有两个方法元数据，没有热路径集合分配或新生产cache。停止在原阶段1禁tick，2原Join，4解seal，8唯一owner detach/恢复gate与stride并清baseline，9World解绑，十一阶段次序未改。两方法native代码属于Mono AppDomain，不能单独un-JIT，正常域重载失效；不为回滚强制reload或清理Profiler。准备失败不会业务调用，部分已编译代码可留在原域内。

## 实际命令、保护与治理

通过PowerShell here-string→python调用原本地Unity MCP bridge：`refresh_unity(scope=scripts,compile=request,wait_for_ready=false)`；`run_tests(mode=EditMode,testNames=8个新case所属7个方法+2个原owner方法)`与`get_test_job`到终态；开窗前后`get_editor_state`/`manage_scene(action=get_loaded_scenes)`；一次`execute_menu_item(menu_path=NTSD/Validation/Optimization/H06 Role Prerequisite Qualification)`。对应JSON均保留，不安装新CLI/桥接，不启动第二Editor。

脚本前Task/Change/Operation与11份真实dirty备份已建立；两个源在开窗前后SHA完全一致，其余2514保护文件0漂移，旧DIAG基线SHA不变，11份备份全部校验通过。没有修改Query/World/Host/AI/两业务writer/GC observer、资源/Scene/Settings/InputActions/Server/Gen/Plugins；只读必要符号及Getter，未读活跃Q06方法体。原共享terminal已事前备份，必要输出只写本Task新目录；没有删除、清理旧数据或破坏Git操作。

首补丁命令因Task读取默认GBK失败，未修改脚本；改为显式UTF8后继续。一次编排工具变量不存在只中止后续工具调用、未重复写文件。无新增C#编译失败或focused失败。唯一资格窗GC失败如上完整保留。

结束前执行Tools/Validate-ChangeLedger.ps1及git diff --check，实际结果在末尾追加；不将未运行项写成通过。

## 交回决定

本任务有限执行结束，**不能交回“前置资格修复通过”**。准确结果是“状态门误拦已修正；两方法无业务冷准备可调用，但完整Driver仍2event，0GC资格未通过”。本次实现保持Editor具名入口，Change BLOCKED，不自动重跑或把准备推广到其它入口。

后继真实缺口：若人类决定继续，需要新的有限证据区分当前剩余两事件的调用点，以及当前Mono准备API是否只返回入口/trampoline或仍存在其他首次路径；本轮没有这些证据，也没有扩大到生产重构、全程序集JIT或新捕获。七段成本继续等待恢复决定。

Role限定生产默认/Brute回退保持，producer USER_DEFERRED/OFF，95/84/89/92/94 OFF；旧ABBA/95未知记录不关闭。H06父/H07的33ms、无持续backlog、正常dropped0及Android设备认证不降低，本次不等于Player/IL2CPP/Android认证或性能收益。旧Goal/34项队列不恢复，不增加父项或优化包数。

最终验证：`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` exit0 / PASSED，1400 Records、当前dirty12个governed code files覆盖（本Task只两个）。4340个既有/历史路径WARNING保留，不清理无关记录。唯一窗口资格GC FAIL不被治理通过覆盖；Task有限执行停止、Change BLOCKED。

最终diff检查：git diff --check exit0（原始输出git-diff-check.txt）；两个代码源仍匹配开窗前指纹，实际report仅1份，无capture目录。操作记录收口VERIFIED仅指声明范围/备份/保护与记录，资格仍FAILED，不再执行实验。
