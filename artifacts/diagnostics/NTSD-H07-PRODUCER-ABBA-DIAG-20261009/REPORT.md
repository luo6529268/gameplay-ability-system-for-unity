# 生产者快照复用混战 ABBA 有限诊断

状态：SCOPED_DIAGNOSIS_COMPLETED / CAUSE_NOT_FULLY_RESOLVED / BASELINE_GC_FAIL / PRODUCTION_DEFAULT_OFF。开始日期2026-10-09，完成2026-10-10（Asia/Shanghai；Task ID保持启动日期）。

结论：本组未复现“复用开启就稳定变慢”；不能证明候选有稳定整tick收益，也没有证据定位一个应修改的生产逻辑。重复OFF均值相差2.051069ms（2.265761%），两次ON相差15.517993ms（后一次低16.714968%）；状态内波动已大于此前约0.53ms producer局部节省。另首OFF完整Driver稳态2次GC.Alloc是本组新失败，不能给本组0GC证书。候选保持生产OFF，暂搁置；Role默认接线保持，不自动生产修复/推广。H06父项/H07/Android仍开放，95OFF，旧Goal不恢复。

Task/Operation NTSD-H07-PRODUCER-ABBA-DIAG-20261009；Change NTSD-H07-PRODUCER-ABBA-DIAG-001。旧Task COMPLETED_NOT_ADMITTED未复开，新结果追加关系，不抹去原+14.45%回退。

## 实验问题、实际改动与前置
用户在讨论会话明确授权针对性检验，原human turn/范围已记录Task。事前假设与改变决定条件见HYPOTHESES.md。
Suite新增具名菜单、ProducerSnapshotExperimentCase（sequenceIndex/action/reuseEnabled/inputBoundaryTimingEnabled）、ABBA builder/冷路由和owner case。旧ApplyProducerSnapshotReuse仍按原奇偶/计时true语义包装；新ApplyProducerSnapshotReuseCase按显式case设置，QueryValid验证case下标与实际两flags，Restore恢复原flags并释放case/world。无新并行Profiler、无生产World/Host/AI/输入/collector算法或热guard改动。
新增Editor状态owner为原Suite：模式bool容量1，starting finally清；4 case随原Session终态清；当前case引用容量1与producerWorld在原Restore正常/异常/PlayExit路径同时释放。新增focused覆盖恢复幂等和录制停止后结构owner；11阶段顺序不变。

一次独立初态combat1000四窗，顺序OFF→ON→ON→OFF；每窗120warm+180sample，固定Role、DataOrientedCanonical、Sakura1000、seed/输入/资源/presentation/容量等。四份65字段request仅outputPath不同；四份case及运行observation证明中间两个ON/最后OFF。既有runner初始化/seed和逐窗teardown保持，非演进World中途热切。逐角色输入边界细分计时均false，实际四边界calls全0；整tickStopwatch、既有相位和可靠同步主线程Driver0GC仍开。本设计仍是受控诊断环境，不能称完全无诊断开销的生产帧。
原项目Unity2022.3.62f3/PID39108，CIM核到准确-projectPath；启动前sole Menu clean8、非Play/编译/测试。未开第二实例/保存dirty Scene/改系统电源或进程设置。

## 实际结果
|序列/复用|tick均值ms|P95 ms|P99 ms|最大ms|稳态GC.Alloc事件|drop（全run）|
|---|---:|---:|---:|---:|---:|---:|
|00-combat1000-off|90.524507|188.667325|247.675414|255.689100|2|1254|
|01-combat1000-on|92.838902|199.923780|241.086834|369.009900|0|1312|
|02-combat1000-on|77.320909|145.528370|169.224398|213.196500|0|1008|
|03-combat1000-off|92.575576|172.079360|236.566951|282.874200|0|796|

每窗180个样本，报告P95/P99直接取原件，不从均值推导。elapsed包含线程运行以外的经过时间，不是独占CPU执行时间。窗口均值均权平均OFF91.550042、ON85.079905ms只是描述统计；不能据此宣布6.470136ms候选收益或推导汇总P95。

现成非重叠主相位摘要（不加父子时间）：
|窗口|CharacterInput ms|CandidateCollect ms|LateEntityUpdate ms|
|---|---:|---:|---:|
|00-combat1000-off|21.714868|31.637572|11.216207|
|01-combat1000-on|19.697314|32.402931|11.543141|
|02-combat1000-on|17.143812|27.478167|9.680808|
|03-combat1000-off|23.611509|30.665138|12.522988|

首次ON相对首OFF均值+2.556650%，第二ON相对末OFF低16.478069%；第二ON的collector、late也降低。结果不随开关呈稳定正向惩罚，时间变化跨多个阶段。仅两个重复窗口不足以建立统计因果或稳定收益。

每窗warmup120、sample180、logicTicksExecuted300，steady按原策略对应逻辑tick121—300。下列UTC为已有report的run起始/末更新区间（包括预热和报告收尾），没有逐样本wall-clock时间戳，不能冒充精确采样UTC边界：
- 00-combat1000-off: report startedUtc=2026-10-09T15:58:07.3233742Z, updatedUtc=2026-10-09T15:59:12.5029277Z
- 01-combat1000-on: report startedUtc=2026-10-09T15:59:12.5589497Z, updatedUtc=2026-10-09T16:00:23.1581408Z
- 02-combat1000-on: report startedUtc=2026-10-09T16:00:23.2397092Z, updatedUtc=2026-10-09T16:01:25.2062766Z
- 03-combat1000-off: report startedUtc=2026-10-09T16:01:25.2682762Z, updatedUtc=2026-10-09T16:02:15.3086553Z

## 对三个问题的回答
1. OFF自身重复均值90.524507→92.575576，差2.051069ms/2.265761%；P95188.667325→172.079360、P99247.675414→236.566951，两者长尾仍明显。只就这两个OFF均值不能称极端不稳定，但2ms已高于候选历史约0.53ms局部节省；首OFF另有有效GC失败，不能作为干净准入基线。
2. 去掉边界细分计时并改变顺序后，ON首次92.838902、其次77.320909ms，未复现开启后稳定变慢。不是重复证明上轮回退无效，也不是候选收益证书。
3. 支持“存在显著窗口波动、原先后AB不能单凭均值差归因候选”；可观察到非隔离环境活动。未证明具体CPU抢占、降频、GC停顿、其它进程或候选间接效应是原因。新旧实验同时改变顺序、运行时间与边界计时，不能独立将差异归因计时开关。

## 同机环境观察与未知
process-before/mid/after.json只读记录PID/进程名/累计CPU/working set，不含命令行隐私，无停止或调整他人进程。environment-summary.json中共同PID累计CPU差按整个初始化+四窗区间统计，不对应具体tick线程：
- explorer PID82096: +2164.766 CPU秒（多线程累计）。
- Everything PID93016: +1615.578 CPU秒（多线程累计）。
- Unity PID39108: +534.969 CPU秒（多线程累计）。
- sysclean PID78440: +438.547 CPU秒（多线程累计）。
- msedge PID26972: +267.469 CPU秒（多线程累计）。

这证明区间内还有显著非Unity CPU活动，环境不是隔离；没有调度轨迹/每tickCPU时间/时钟温度等证据，不能判定这些进程造成哪次长尾。运行中Editor曾报告Play transition、进程Responding=true；初始化耗时没有算入logic样本。未观察到编译中断，结束恢复Menu。上轮弹窗原因仍未知，本轮没有重启Editor。

## 正确性与有效GC失败
实际AI/roster均1000、Role mode3、fallback峰值3/3/3/3、abort0；owner applied/unchanged/restored全true。OFF观察max skip0/fullRefresh1000，ON max skip1000/fullRefresh0（sampled Editor观察最大值，非所有tick累计）。四窗输入边界false/calls[0,0,0,0]，GC观察未关。
四窗20 finalParity/finalLockstep hash全部一致，末tick300完整snapshot逐字节同，并与上轮混战snapshot同，SHA E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。包括既有输入/RNG摘要，未新增逐tick全域trace。生产15个guard/字段别名/陈旧修复/逐tick输入双RNG旧资格和88限定Driver证据按未变方法依赖复用，不把本轮末态替代全部tick。
四窗校准前后1MiB正/空负通过，accepted300/steady180/nonsteady120、invalid/rejected/sequence0。首OFF稳态2event、firstNonzeroSteadyTick130、provenSteadyAllocatedBytes=-1（未知字节数），coverageValid=true、zeroAllocationPassed=false；其余三个窗steady0/proven0B。首OFF非稳态140event，后三窗各24。旧窄域managed gate/CollectionCount均0不能覆盖完整scope的2event；Suite MEASUREMENTS_COMPLETED只说明四窗测量结束，不是所有门通过。
已有原件只定位到首OFF的完整同步Driver scope与首次tick130，没有调用栈、第二事件tick或可靠字节数；所以不能归因FindEffect、producer或热路径新增case，更不能将2次事件当GC回收停顿解释数十ms长尾。新case分配在warm/sample均0的冷Apply之前，但这不是全部Editor分配来源的排除证书。没有关闭0GC门、忽略失败或再次测到0来覆盖此窗。

四窗capacityPressure PASS/criticalDelta0/violating0；每窗对象/entity/slot/active pool/reference pool/cleanupException均0，Suite orderedShutdown true、objects/slots/borrowers0，原Scene恢复、Battle SHA不变。Profiler设置前后完全一致；原Editor最终Menu sole clean8/idle。2452文件保护中只有已登记Suite发生本任务增量，真实备份SHA同、冻结源不变、HEAD/staged同，原两个任务证据不动。

## 实际命令与证据复用
- 原Editor MCP manage_asset action=import path=Assets/NTSD/Scripts/Test/Editor/H07ProducerAbbaDiagnosticEditorTests.cs；新Editor DLL UTC15:52:50，域重载后idle；Editor.log检索未见error CS。非独立Player build。
- run_tests mode=EditMode，groupNames=[^NTSD.Test.Editor.H07ProducerAbbaDiagnosticEditorTests.(CaseAnd|InvalidSequence|OwnerApplies|ObservationRequires), ^NTSD.Test.Editor.BattleOptimizationWindowsAiSuiteRequestTests.ProducerSnapshotOwner_(Applies|Structural|LiveObservation|InvalidOwner|ObservationRequires)]，job f5b5c0e4cff44db48aa14530a73b6a5c：33/33 PASS、0skip（17新＋16受影响旧owner），qualification-tests.xml。未重跑旧整44。
- run_tests testNames=[NTSD.Test.Editor.H07ProducerAbbaDiagnosticEditorTests.EditorSettings("before")]，job d12c7ed3a3f74c2f965e2e263e015552：1/1 PASS；after job dd26bbc4227d4f95a3a5103ecbdb8262：1/1 PASS。
- execute_menu_item menu_path=NTSD/Validation/Optimization/H07 Producer Snapshot ABBA Diagnostic：仅一次，四窗原件windows/，原始失败全部保留。
- Python摘要读取request/case/实际observation/20hash/snapshot/GC/capacity/closure并保存measurements-summary.json；源/备份保护见preservation-audit.json；原生产与上轮冻结SHA核同见reuse-audit.json。
- 有一次rg通配路径在Windows报os error123；改用rg -g过滤后只读查询成功。首次CIM路径只匹配引号得到unknown，补无引号匹配确认准确路径。均非实验失败，无重复窗口。
- 交付前运行pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity及git diff --check，实际结果追加下方。

## 结束与下一建议
本次到此结束，保持producer生产OFF，Role默认及显式Brute/非法回退保持。未改生产代码，未执行额外分散/长窗/1800/设备/GPU/全量回归/95/Q06/新路线；新诊断有价值的结果是限制了原回退的解释，并暴露首OFF0GC失败，不以诊断完成计父项关闭或旧批次完成数。
若用户仍要恢复该候选准入，最小缺口先是对首OFF完整scope首次tick130分配的调用点取证与当前可比较基线；需要独立、明确输出的新诊断合同，复用既有capture工具且不放宽guard/0GC，不直接改生产。即使调用点处理或新的环境对照通过，也仍须另行决定生产接线；当前没有足够证据提出某个具体生产修复。用户也可选择保持OFF并搁置此微收益候选，但本会话不自动转下一优化点。

最终校验：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity 实际exit0/PASSED；Records: 1397；Governed code files in diff: 7；warnings=4417（保留历史声明路径等警告，非零警告）。git diff --check exit0。最后MCP确认原PID39108 idle/Menu，非Play/编译/测试，未重启或保存Scene。
