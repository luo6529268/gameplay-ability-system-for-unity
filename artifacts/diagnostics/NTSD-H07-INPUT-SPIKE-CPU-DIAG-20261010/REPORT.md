# H07角色输入：CPU证据补充与准确修复范围

Task: NTSD-H07-INPUT-SPIKE-CPU-DIAG-20261010
Change: NTSD-H07-INPUT-SPIKE-CPU-DIAG-001
状态：SCOPED_DIAGNOSIS_COMPLETE / BOUNDED_COMPATIBILITY_REPAIR_READY；仅诊断工具VERIFIED，生产修复未实施、未准入。
用户授权：继续检验直到可以发送给另一会话修复。仅原H07-INPUT同点；额外方向不跟进。

## 结论与修复边界
已充分确认一个可精确移除的重复处理：在原生输入管线接管、DataOrientedCanonical、当前World拥有且canonical input store仍有效绑定的exact LF2Character AI中，ECS输入pass仍调用兼容resolver，捕获17个进度字段、复制original、进入两个已被native接管的空分支，再逐字段比较并记录skip。先前完整流程与本轮两窗都显示每tick1000次skip、0次commit；两窗各180000次steady skip。
可实施改法是在现有ECS input pass的这条调用前检查准确所有权/模式/绑定条件，满足时只保留原skip计数及frame-motion tail；任何条件不满足仍走原resolver。不是删除原生组合键、AI决策、目标查询、producer刷新或行发布，也不是共享resolver全局提前return。详见 execution-handoff.txt 与单项记录。
本结论只表示准确候选可实施，不代表整阶段性能或生产准入已通过。首窗完整Driver仍2个稳态分配，需按原0GC门处理；不能借第二窗0事件改写首窗。

## 实际执行
原Unity2022.3.62f3 / Editor39108 / TCP6402。原Menu clean8，复用原实例；未启动第二实例。
- 三诊断域编译/reload完成；当前Console无CS error。
- run_tests EditMode：H07InputGuardDiagnosticTests四个CPU计数器/两窗request focused 4/4；CharacterInputLiveSlotLoopEditorTests.NativeSampleTiming_DisableAndNextTickClearChildren 1/1，共5/5。job原件见focused-*-final.json，10601为发现总量，不是本轮测试数。
- 同上一份7412092B raw冷追加导入，导出主线程176个可达steady tick(125-300)，无重新Profiler捕获；retained-raw-ticks.json，settingsRestored=true。
- 唯一新组：combat1000 / 1000 Sakura / seed1314149188 / Role / DataOrientedCanonical，phase CPU provider OFF→ON，各120warm+180steady。两窗Profiler全部OFF，42个原elapsed槽两边相同且ON；仅7槽可选周期provider不同。producer/95/record/旧六分支等OFF。
- 两窗各300完整tick，14字段输入数量/完整RuntimeChecksum/双RNG记录一致；本轮OFF与上一Profiler窗300行也0差异。CPU周期provider每selected phase每tick1000对，OFF全部0。flags失败0、invalid0、校准/GC覆盖有效。

## 测量事实
以下耗时为有诊断的经过时间，均值单位ms/tick；有父子嵌套，不相加。CPU envelope包含GC observer边界开销。

| 范围 | 周期provider OFF | 周期provider ON | 决策限制 |
|---|---:|---:|---|
| CharacterInput | 11.996463 | 15.697957 | ON增加逐角色CPU计数，不可当生产退化/优化A/B |
| 兼容ComboUpdate父段 | 0.478806 | 1.423142 | OFF也保留elapsed计时；不是全部可省收益 |
| RemainingAiDecision | 4.961281 | 6.509212 | 内含kernel/nearest，不能相加 |
| kernel | 3.115163 | 4.124308 | 已有Indexed路径 |
| nearest | 1.540116 | 2.033463 | 两窗平均访问19495.505556行/tick，约19.5行/1000个查询之一；不是每AI全扫1000 |
| producer refresh | 1.000001 | 1.507874 | 用户暂缓，不能修 |
| native sampled state | 2.926126 | 3.538172 | 原动作处理保留，不展开Q06 |
| 完整Driver CPU envelope elapsed | 47.495048 | 50.747742 | 诊断非33ms证书 |
| 当前线程kernel+user CPU时间均值 | 47.656250 | 50.868056 | 实测以15.625ms量化，不能逐tick精确相减算waiting |

CPU采样单调校准通过。OFF冷64次空边界mean1.9us/max93.1us(含首次冷调用)，ON已热mean0.4us/max1.0us；保留原始cycles均值5701/782，不换ms。这证明计数器有非零成本，也说明不能机械扣冷mean制造生产收益。OFF/ON顺序与环境仍混杂，不能把差值全因果归计数器。
Windows GetThreadTimes的100ns是单位不是本机分辨率；QueryThreadCycleTime是raw cycles，不能作为指令数或换算经过时间。官方API语义：[GetThreadTimes](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getthreadtimes)、[QueryThreadCycleTime](https://learn.microsoft.com/en-us/windows/win32/api/realtimeapiset/nf-realtimeapiset-querythreadcycletime)。

## 浮动已查清的范围与未知
1. 旧raw最大的输入尖峰258/226/262等：Driver内GC.Alloc、Mono.JIT及GC.Collect样本均0；该主线程范围内两次JIT不能解释所有尖峰，不扩为全线程证据。
2. 新无Profiler两窗steady都没有GC.CollectionCount(0/1/2)增量；该计数覆盖BeginDriver至Accept完成校验的稍宽区间。OFF分配2event130/162、ON0；分配事件不是GC回收或停顿。新窗没有新调用栈，不能用旧raw36/30B和Mono.JIT栈补成新窗对象/bytes证明。
3. OFF Driver峰113.3567ms，线程CPU记109.375ms、339881958cycles；ON多个131-136慢tick的7个段cycles同时增加。Driver elapsed/cycles相关约0.9900/0.9899，输入7段同窗相关约0.919—0.983。支持慢帧不只是经过时间被拉长，不能认定所有尖峰都在等待。
4. 实际逻辑状态/输入/RNG与nearest访问量一致，17字段compat提交一直0；这块是确定的重复读/比较。它不能解释整个输入阶段，不能称修复后会消除全部尖峰。
5. 未采OS调度/硬件计数，所以不能确诊抢占、频率、cache miss、跨线程争用或具体native业务分支的机器成本。QueryThreadCycleTime增加不等于指令条数增加。此未知不需要通过修改AI规则/Q06/系统设置解决，也不作为无证修复对象。
6. 精确compat guard的安全性来自已读调用链、canonical/profile失败分支focused和逐tick零写回；并不依赖猜测这些机器层原因。额外系统归因不会改变这个改法，因此不继续为它追加报告/窗口。

## 准确改法与必要验证
生产写域建议限BattleEcsCharacterInputPass.cs；需要候选OFF/ON时在现有SimulationWorld.DetailTimingDiagnostics.cs只增加该pass的冷配置门/计数facade，实例flag默认OFF。Editor沿既有Suite+H07InputCostDiagnostics及CharacterInputLiveSlotLoopEditorTests复用；不得修改共享resolver、store实现、AI算法、native writer/Q06。
Guard全部同时成立：pass DataOriented；exact LF2Character且AI；runtime非null；RegisteredWorld与pass World相同且UsesNTSD28NativeInputPipeline；World.AiExecutionProfile==DataOrientedCanonical；当前writer.CanEvaluateCanonicalDecision(runtime)有效。后者只证明slot范围/generation非0/owner引用一致，单独不包含profile；不可遗漏profile，不发明runtime generation比较。
允许跳过的只有ApplyFrameInputFromRuntimeProgress兼容外壳。保持world.RecordCharacterInputProgressCommitForDiagnostics(false)、TryExecute返回、外围角色处理计数、applyFrameMotionTail=true/false原分支及后续native routing/AI行发布。
先RED/GREEN guard与完整17字段/fallback/析构重绑/foreign owner/mixed player-AI/derived/invalid frame及frame tail focused；复用此前未受影响8项，不机械扩大。候选默认OFF并有同fixture对照；独立记录旧调用1000→fastskip1000/兼容capture0，整tick/checksum/输入双RNG、容量、关闭不变。
性能对照关闭本轮thread cycles/Profiler/全部逐角色elapsed细分，两边同配置，保留整tick和原可靠同步主线程Driver GC。限定combat1000 OFF→ON→ON→OFF各120+180，同初态；首个硬门失败停止余窗。已知JIT背景不滤、不移、不加warm、不开新Profiler找PASS；若0GC再次失败则候选保持OFF并交回，不能把正确性实现写成生产准入。
通过必要门且收益可分辨才提出限定生产准入；若收益不足或整tick回退无法解释，保留OFF并结束此版，不无限重复。H07精确33ms/drop0/backlog与Android最终门独立保持。

## 保护和收尾
suite两窗MEASUREMENTS_COMPLETED仅表示检测完成：两窗workload/观察有效、覆盖300/180/invalid0；两份zeroAllocationPassed按CPU_DIAGNOSTIC_NOT_CERTIFICATE均false，实际event2/0保留。Scene恢复Menu clean8、Battle SHA不变，有序关闭objects/slots/borrowers0，owner/provider恢复。
15562保护文件SHA全部无漂移/无缺失；11份dirty before备份全部匹配。未覆盖无关修改，无删除/移动/Git丢弃；未读Q06体。audit-summary.json、verified-source-fingerprints.json、windows/suite-result.json可追溯。
Tools/Validate-ChangeLedger.ps1及git diff --check结果另附；执行修复指令需在本次文档/validator收口后发送。

最终静态审计：Tools/Validate-ChangeLedger.ps1 exit0 / PASSED1403 records，三实际diff脚本均覆盖（历史Record路径不在当前diff的warning保留）；git diff --check exit0，仅现有LF/CRLF提示，无whitespace error。未提交/推送。

## 实际派发
准确execution-handoff.txt已直接发送“处理优化方案”thread 01a1201b-7930-7c93-8c84-5bc7baada43f；新turn 01a12421-e651-7e90-802d-06895c559be9 active/inProgress、error null，cursor :66。候选实现与有限验证建议Task NTSD-H07-NATIVE-OWNED-COMPAT-SKIP-20261010，默认OFF。监测automation已ACTIVE，每5分钟只读检查、仅实质事件通知，完成本有限Task暂停；dispatch-receipts.json保留回执。此派发不是生产修复或性能验收完成。
