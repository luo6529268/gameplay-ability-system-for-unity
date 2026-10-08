<!-- CHANGE-RECORD
id: NTSD-OPT-H07-LOGIC-GC-SCOPE-054
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleLogicTickGcObserverEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
authority: approved finite-phase sections0-8 H07 reliable allocation gate; Editor diagnostic only, no gameplay or production-default change
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH54-LOGIC-GC-SCOPE-20261008/REPORT.md
-->
# 第54批完整 Driver tick GC scope

VERIFIED，仅诊断接入/生命周期；RELIABLE_LOGIC_GC_MIXED / PERFORMANCE_FAIL，H07/H11不关闭。两实际1000AI各120warm+180sample，300 begun/ended/accepted、覆盖/前后正负校准有效，invalid/reject/sequence0。分散steady0event；混战steady6event、first total tick158，raw1600单位TimeNanoseconds，不能解释为字节；bytes=-1/UNKNOWN。旧byte API两窗仍raw0，与混战真实非零证据矛盾，不能使用旧raw gate称0GC。

logic mean81.768/85.219ms、P95112.550/114.910ms，drop802/789；CandidateCollect51.462/54.595ms，PairExactLoop50.949/54.055ms。FrameTiming CPU均值205.441/214.030ms不是Stats FPS或GPU时间，没有同期收益A/B。正式1800、独立central unresolved/stale、Android/Player/GPU未验，不把短窗升格。

两terminal PASS仅指cleanly stopped；两整份末tick300 extended snapshot SHA与52同workload baseline逐字节相同，不称逐tick正式EXE。capacityCriticalDelta0、两窗minimum AI/roster1000、orderedShutdown残留objects/slots/borrowers0，原Menu clean8roots/idle。2026-10-07T19:01:40Z末审61guards/8备份/4冻结源SHA/HEAD均同，shared request absent、同PID19040；terminal-audit-01.json。pwsh validator实际0/1350Record/4253历史WARNING（更正旧^WARN得到0）；git diff --check0/25CRLF提示。一次Windows PowerShell wrapper exit1过滤未保留原因，不作为validator证据；随后pwsh有效通过。代码/场景无进一步改动。下一必要混战调用点诊断，旧43表现FAIL保留。

下列为追加保留的事前和过程快照，当前以本段/REPORT及实际窗口为准。

FOCUSED_TEST_PASS / WINDOWS_READY：最终new24/24，0skip/2.4874223s，job5fffd14af69a431facb0fb78d45f3c04；旧CPU两项/steady policy/计时collector/cleanup共5/5，0skip/0.9717776s，job73233fd499ac4c2994d46647995312aa。新empty/known allocation/warm-vs-steady/拒绝与漏采/序列/容量/owner/Dispose/Begin异常全部过；主代理实际对事前dirty两脚本全diff与新文件生命周期已审。无新正式EXE/完整Driver逻辑改动资格要求扩展或53cost重跑。

窗口前UTC2026-10-07T18:44:11.4773676Z：61guards/8dirty副本/HEAD同，validator actualexit0/1350Record、diff-check exit0且25CRLF提示无问题；warning提取只匹配^WARN不可靠，原JSON的0不能当作无历史warning，未核确数。三C#冻结B07CEBD1...AD928 / E36B84D3...02D57 / F2DC9B54...26993及metaDC295B14...898C1，见pre-window-audit。原Menu clean8roots；未启动Windows/尚无实际可靠logicGC/FPS。

异常边界RED已实际复现：job8c0af44211404e588397df6200ae4c48，24completed、唯一具名failure为Begin未成功却End=1；summary null不伪造通过/skip数。仅新增局部成功Begin布尔，End只平衡已成功打开scope，inFlight仍在finally归零，不改变Driver调用。其它校准/容量/owner/request未报告失败。待最终GREEN及必要旧影响域。

首编译真实CS0019：Config是struct，不能与null比较；仅移除多余null guard，保留已配置report/firsttick/worker门。主代理实际三C#差异复核发现Begin回调失败后仍调用End的异常平衡风险，先加1个有效异常RED与2个必要live-owner/dispose回归，不扩大任务或缩scope；最终聚焦数量为24而非原21。尚未GREEN/实景。

CODE_WRITTEN：有效RED job27b5e1cc7fc64a14a7e8f679035166cf为21completed/failed，各为缺新接口/实现Expected not null；summary null不补造skip/duration。仅三声明C#已写默认空Editor整组回调、完整Driver前后/既有steady判定、固定256 owner和opt-in Batch54双窗/sidecar/正常及abort释放。首个多文件patch末Suite锚不匹配而整体未写，两现有SHA核同后分拆正确patch；没有覆盖用户内容。GREEN/实际scope/实景未验。

TESTS_WRITTEN：仅新Editor测试文件与meta已创建，先通过反射要求新接口/owner/request以得到有效缺实现RED；两现有脚本尚未改。8事前副本已逐SHA核同；Operation/Index/Ledger/STATE/handoff已在代码之前登记。原Editor仅请求脚本编译，尚无RED终态/可靠GC/窗口结论。

PLANNED。原状：完整同步StepMeasuredTick直包StepOneTick但只读不响应的byte API；Suite始终把raw gate保留并可靠GC标UNKNOWN。新包不修改这些历史语义，只新增opt-in校准事件证据。[准确Task](../TASKS/NTSD-OPTIMIZATION-BATCH54-LOGIC-GC-SCOPE-20261008.md)；[操作与回滚源](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH54-LOGIC-GC-SCOPE-20261008/RECORD.md)。

预期副作用：Editor hook空时不记录；开时有固定ProfilerRecorder读写开销，不能拿测得差异当FPS优化；校准明确热路径外分配。一个run一个owner，固定256事件，不新建生产World服务；不存在资源/预算/bank或关闭顶层顺序变化。首次attach/异常/退出/Dispose均有聚焦测试，scope整段Driver、样本选择复用既有最终SamplePolicy，不能缩scope避开事件。

验收/未验证：test-first RED、GREEN、scope/校准、双120+180新窗、生命周期/dirty保护及validator待实际执行；尚无可靠logic0GC、收益、设备/正式1800或完成结论。任一缺失/饱和/失配不得标zeroGC。现有53默认OFF与52失败不变，H07/H11 OPEN，Goal active。
