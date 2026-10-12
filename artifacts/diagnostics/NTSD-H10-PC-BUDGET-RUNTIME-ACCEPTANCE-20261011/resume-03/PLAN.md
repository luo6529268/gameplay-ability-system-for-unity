# H10 resume-03：冷播放观察身份修正（脚本修改前）

Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 / Change NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001。授权来源：人类继续H10及已批准PC/320MiB/2GiB/3GiB预算；讨论会话01a11fe2-bfaa-72f3-a9ed-c8260f63b193独立审阅后的resume-03准确派发（执行turn01a12921-3e24-7370-9258-494349611e63）。此前byte97校准、raw-key及第二轮generic播放观察失败完整保留。

唯一脚本为Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs；只更改PcBudgetOutput/RestoreKey、本Task新增PcBudget冷快照/选择helper、RunThreeSessions的formal/generic播放观察、CloseDriver的关闭前后观察。旧FindVoiceWithClip及非PcBudget前缀，三recorder/资格算法、sink/emitter、ExerciseHot逐字冻结。所有生产/其它测试/Scene/资源/Settings/Plugins冻结，不读Q06正文。

复制调用前后AudioSource值（包括inactive、ID、clip、parent/host归属、enabled/active/isPlaying/loop/pitch/timeSamples/dsp），复制oneShotVoices槽位和long启动序号。仅留原数组引用用于ReferenceEquals身份验证，不把可变数组内容当before；保存旧selector、全部matching/playing及序号推进映射。直接只读原生产播放/loop/load/build/failure/rejection计数，不写hook、不调用任意getter。

每次单formal或generic调用必须总播放计数增1、无load/build/failure/rejection增量、原数组及逐槽source身份不变，恰一启动序号推进且新声源clip为期望同引用，逐项断言非null/enabled/active/isPlaying。完整冷证据CreateNew落盘先于播放断言。8帧波形也先落盘再generic，非零输出与人工听感false分列。

CloseDriver传本次真实generic源；在公开ShutdownBattleRuntime前后立即采其身份/clip/启用/在播/timeSamples/dsp并先落盘，保留原借用不可停断言、stage10返回、空地图stage11、tail/drain/后帧owned归零。自然结束与Stop无法区分时FAIL/UNKNOWN，不重播/loop/延时/改pitch制造通过。

新状态均属单个PcBudget测试调用的冷诊断快照，容量由既有64voice池和自有audioHost实际source数量一次确定，逐槽复制数组，无生产缓存/热增长；只在热scope外创建。证据输出后快照由方法释放，不参与生产阶段；fixture仍stage1停止请求、stage5原声音owner回收，stage10/11公开关闭、await drain和后帧Destroy。无新增runtime服务或关闭顺序。

最小验证：当前dirty11份真实备份、源码/程序集/recorder9指纹、4094保护与旧全部artifact；原Editor新鲜idle/nonPlay/noncompile/update/nontest/Menu clean8。使用既有MCP（已知CLI Pipeline不可用，不安装/改工具）最窄编译；本版PcBudgetAllocationMeasurementQualification和PcBudgetRuntimeQualification两项。全部通过才一次PcBudgetRuntimeThreeSessionsPlay隔离EditMode Enter/ExitPlayMode组，三轮原负载/真实play/public close/reentry后同组pending取消和自然UnityDestroy。任一硬门首差立即停止剩余步骤，finally仅安全清理，源保持实际受测版本，不重跑找PASS。

三独立current-thread GC.Alloc/Internal/cap256及前后1MiB正控/empty保持，Unit TimeNanoseconds不是字节；1000声音请求不是1000实体，非wholeDriver/CPU收益。旧218.313758MiB对象账本仅当前指纹匹配后复用，不重解码全catalog；进程/Player/目标PC/Android门不声称通过。

风险：旧selector选中已停的同clip声音、快照可变引用污染、证据观察耗时使短clip自然结束、关闭误停借用、测试失败后现场恢复。所有观察在热外且不跨await；未知字段null/UNKNOWN，关键缺失fail-closed。generic三轮同引用且最终精确key/clip/borrower检查后才RemoveCache/去重Destroy；不清旧资源、不GC.Collect、不UnloadUnusedAssets/Editor重启。

回滚仅从本次真实dirty备份审计撤本诊断增量，不能HEAD覆盖或撤生产。新原件仅resume-03 CreateNew；旧root/resume01/02冻结；Temp/Goal18_LastTestResults.xml每次callback覆盖前备份真实前件。治理限Task/Change/Ledger/STATE/CURRENT-AUTHORITY/handoff/H10卡/进度/对齐表H10状态与Operation/INDEX追加，预算决定不改。进程字段仅PID/创建时间，禁止CommandLine/环境/凭据及全字段输出；旧脱敏事件保留不复制原敏感数据。H10父项仍OPEN，无Goal/34队列/其它点。
