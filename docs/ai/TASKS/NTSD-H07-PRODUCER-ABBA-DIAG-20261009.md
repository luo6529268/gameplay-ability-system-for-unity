> 最终：SCOPED_DIAGNOSIS_COMPLETED / CAUSE_NOT_FULLY_RESOLVED / BASELINE_GC_FAIL / KEEP_OFF。完成一次具名诊断，未授权继续修生产或推广。

# Producer快照复用混战ABBA有限诊断
IN_PROGRESS。新Task；旧准入Task COMPLETED_NOT_ADMITTED保持。用户于讨论会话明确“可以，我也建议先针对性的检验下，看看为什么变慢了”，已只读核实原始human turn 01a12156-c95a-7811-9b3c-b91b9ccd5c4c（source thread 01a11fe2-bfaa-72f3-a9ed-c8260f63b193）。仅诊断，不生产推广/修复，不恢复Goal，保持Role默认与producer OFF。

准确写域：Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs；新增H07ProducerAbbaDiagnosticEditorTests.cs/meta。Suite新增ProducerSnapshotExperimentCase、BuildProducerAbbaCase/BuildProducerAbbaRequest/BeginProducerAbbaDiagnostic，SuiteState模式及RunState明确case；BeginSuite/BuildCurrentRequest/冷Apply路由/IsOwnedTerminalOrAbsent必要分支；既有ApplyProducerSnapshotReuse保留旧签名语义，提取ApplyProducerSnapshotReuseCase，QueryValid按当前owner case验证，Restore清case。Observe仍需live录制、结构关闭仍允许录制已停；InputBoundaryTimingObservationValid原有关闭状态calls0合同不变。不得读活跃Q06或修改World/Host/AI/输入/collector生产路径。
新增Editor状态：starting bool容量1/finally清；Suite mode bool与每run case上限4、owner原Suite/session，原终态清；当前producer case引用容量1，归既有producerWorld owner，在原Restore（正常终态、异常退出、PlayExit）随world释放，幂等null。没有新战斗runtime状态/缓存/manager，11阶段不变。

问题/假设：H1候选影响若在同配置两个ON持续慢于两OFF，才支持该条件下可复现关联，非因果；H2关闭状态自身/同开关重复波动可解释原顺序混杂，需报告范围；H3额外边界计时可能影响测量，但新ABBA同时改变顺序和运行条件，不能单独归因开关。已有producer局部收益与字段回退资格复用，不重做八子阶段。
唯一实验combat1000，OFF→ON→ON→OFF四独立初态，每120warm+180sample，固定Role/DataOrientedCanonical/Sakura1000/seed/input/presentation/capacity其余flags；额外输入边界逐角色计时false，可靠完整同步Driver0GC、整tick及现有非重叠相位计时保持。明确case记录sequenceIndex/action/reuseEnabled/inputBoundaryTimingEnabled，请求→owner→World和第二ON/最后OFF实测。不得中途热切同World、不额外分散窗/1800/设备/多组重试。

最小验证：新4case映射/全部request字段同历史combat除output；新case owner在不同原flag状态下应用/拒绝错误index或篡改flag/录制结束前后/恢复幂等；原ProducerSnapshotOwner受影响case（不重跑未改生产15项或整44）。设置前后各一次；四窗input/RNG/20hash末态与旧同配置证据、actual skip/calls0、可靠GC/容量/关闭/源保护。无直接证据不归因CPU抢占/降频/GC停顿；Stopwatch为elapsed。现有低开销文件时间/进程累计CPU快照只说明采集区间，不解释单tick调度。

结果政策：重复OFF与ON若波动盖过收益，保持OFF/暂搁置；稳定变慢只报告具体线索及修复建议，不修生产；有收益只报告最小准入缺口不推广。必要门失败先保留/定位，不无限重跑。完成报告、当前说明追加、H07-INPUT、唯一进度、Change/Operation、validator后停止。
回滚仅本Task精确增量，真实dirty备份保留前两项，禁止HEAD恢复/任何Git破坏、commit/push。风险为ABBA误映射ABAB、计时关闭漏观测、owner恢复错误、顺序/环境混杂；用冷测试和实际case/计数双证。所有原权限红线不变。

结果与停止：2026-10-10 新具名ABBA诊断已完成：OFF90.5245/92.5756ms，ON92.8389/77.3209ms，未复现开启后稳定更慢，ON状态内波动15.518ms，不能宣称稳定收益或具体因果。首OFF校准有效完整Driver稳态2次GC.Alloc、首tick130，后三窗0；本组GC门FAIL、调用点未知。33+1+1测试及配置/20hash/末态/容量/关闭/设置恢复通过；producer生产OFF，Role默认保持，未自动修复/推广/转下一项。Change NTSD-H07-PRODUCER-ABBA-DIAG-001 VERIFIED仅冷诊断实现与记录；Task NTSD-H07-PRODUCER-ABBA-DIAG-20261009 SCOPED_DIAGNOSIS_COMPLETED，H06父/H07/Android开放、95OFF、旧Goal不恢复。详 artifacts/diagnostics/NTSD-H07-PRODUCER-ABBA-DIAG-20261009/REPORT.md；下方IN_PROGRESS/旧准入状态保留历史。
