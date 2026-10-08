<!-- CHANGE-RECORD
id: NTSD-OPT-H07-BRUTE-BRANCH-TIMING-044
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/SimulationWorld.DetailTimingDiagnostics.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressDetailTimingEditorTests.cs
authority: user continued H07 optimization; bounded autogoal corrected sections0-8; formal336B44 rules unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH44-BRUTE-BRANCH-TIMING-20261007/REPORT.md
-->
# 第44批：普通 Brute 分支成本诊断

生命周期措辞更正：重扫runner StopAndCleanup会先CleanupInternal再terminal；正常Suite终态读后恢复flag，不是在runner cleanup之前。Suite abort/最终owner shutdown则先恢复再ShutdownBattleRuntime。44 Task对应追加更正，不改调用链/11阶段，不将恢复bool误作新Runtime owner或GPU完成证明。

最终治理：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot 当前绝对根，exit0、1340Record/14governed diff/4257历史warning/0error；git diff--check exit0（仅CRLF提示）。validation-final-01.json明确保留首次同进程Write-Host未捕获警告数0不可用、Windows PowerShell子进程默认参数Split-Path空值失败，均未更改validator；正确pwsh显式根通过。不是新测试/性能窗口。

最终限定状态VERIFIED / SCOPED_DIAGNOSTIC_VERIFIED / COST_ATTRIBUTION_PERTURBED：四实际1000AI短窗均DONE、有效，原202/202通过、flag/四默认/11阶段/Scene恢复；五源码/22guards/11backup/HEAD保持。OFF logic80.727/84.127ms、P95102.777/110.107ms，collector49.414/52.626ms，显示约3.843/4.074FPS；累计dropped810/1257/742/853非零，H07仍PERFORMANCE_FAIL。ON collector较OFF多35.295/11.034ms，归因受clock及顺序环境干扰，不是纯生产分支成本或FPS收益。两个末300tick snapshot字节与20hash均同，仅限定快照，不称逐tick/native/0GC。下一既有四phase低频诊断及覆盖计数资格；H07/H11仍未过、Goal active、不解冻专项。REPORT最新表与terminal-source-audit-01.json含实际证据；最终治理检查待追加。下方准备中及待采样为历史快照。

唯一44菜单已提交，四新request文件真实落盘并逐字段同40除output；windows-request-freeze.json保存逐路径/SHA。原PID19040 Responding true、CPU增长，当前仍准备阶段未见sample进度，不能把旧40 SharedResult PASS当44结果。Play期间不再MCP观察，终态/实际AI/成本待，保持RUNTIME_PENDING。

最终受影响回归：原PID19040 job750141db25ab4380bf35353a344174f9实际202/202 PASS、0skip、19.2191183s。原件test-regression2-01.json，含17本批schema/branch/request/恢复case及已有候选/Detail/Suite/Formal受影响门；首次202的44→48失败保留。五源码/22guards/11备份/HEAD冻结于windows-source-freeze.json，尚无新1000AI测量或FPS/0GC结论。当前status FOCUSED_TEST_PASS，下一唯一四短窗。

必要范围补充(先声明再改)：原回归job907233c94c92449d8b8416c73512726b实际completed202，唯一已报失败DetailTimingCollector_PopulatesSeparateReportOnlyAfterWarmup第303行期望44、实际48。增加第五准确路径ProductionEntityStressDetailTimingEditorTests.cs，只更新报告schema数量为48，不删除断言或改变warmup/采样门；before-supplement.json/backup11先核，再脚本补丁。其余原4写域不扩生产行为。

首GREEN原Editor job1d5e47db767e48678e416a08abdf57c1实际13/13 PASS(2.5202853s)，原件test-green-01.json。再补必要四case：reject分支覆盖、geometryFirst关闭不计时、Suite恢复原false/true与引用释放。补丁中曾误插重复方法声明，静态回读立即删除本批重复行、未将其算测试结果；最终编译/四case与旧回归待核。

原Editor有效RED：job961991b35a1f4b4083161ef77e2838d2，实际13/13失败，预期缺flag/API/四名称与Count44；非catalog9505执行量。原件test-red-01.json。现已在四声明脚本写入默认OFF四分支计时、phase44—47/Count48和四OFF/ON Suite；无算法/四默认/生产collector修改，实际GREEN/窗口待。

测试先行：两个预声明Editor测试文件已新增phase44—47、默认false、OFF/ON候选/RNG及request四矩阵断言；既有PhaseCount期望48。生产查询/recorder/Suite API仍未改，待原Editor编译与有效RED。初稿PrepareCapacityForBattle不存在，静态读取实际PrepareBattleCapacity后已在编译前更正，不计编译/测试结果。

PLANNED，记录先于本批任何C#修改。第40批约60%逻辑成本仍collector；已有总phase不足以区分coarse、rejected binding、pairAllowed、exact ITR/body。当前没有本批测试/新计时/收益。

Task：[固定合同](../TASKS/NTSD-OPTIMIZATION-BATCH44-BRUTE-BRANCH-TIMING-20261007.md)。Operation：[备份/精确清单](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH44-BRUTE-BRANCH-TIMING-20261007/RECORD.md)。

默认关闭计时，仅沿用世界诊断RecordPhaseElapsed，在普通geometryFirst现有路径记录四独立耗时；追加phase44—47、保留旧ID。顺序/一次性执行/rest失效清理全部保持；不开每pair Profiler marker、不新建索引/缓存/collector。Suite四固定1000AI OFF/ON运行前后原flag恢复；abort在十一阶段关闭之前恢复，不修改其顺序。

新四诊断计数是world既有phase数组的冷准备内容，无新owner/worker/GPU/buffer lease；物理数组bytes另记，不擅改ATLAS预算。默认OFF仍需检查额外分支开销，ON成本不当作无扰动纯函数成本。既有phase sum包含nested，不相加解释整tick。0GC counter仍未校准，H11完整FAIL不撤销。

验收：反射/phase RED→GREEN；聚焦candidate顺序/RNG等价，默认false/关闭phase0，开启scope命中；Suite request同40除output。原Editor编译和必要旧测试实际通过后，冻结源及四窗口请求。120+180真实AI1000、固定seed/profile/collector；同负载OFF/ON末snapshot/hash相同；生产四defaults不变，cache/geometry300应用0fallback，正常/abortflag恢复，11-stage残留0、Scene clean/SHA保持。最终运行/开销/剩余UNKNOWN原件与REPORT，不认证H07性能/Android/FPS。

回滚只本批backup/current差异，保留dirty，先取得精确恢复授权/Operation，禁止破坏性Git。Q06只hash，EXT1/Mono/ATLAS/Role默认冻结；副作用只是诊断clock的耗时和既有冷数组增量。历史与失败证据不抹去。
