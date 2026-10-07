<!-- CHANGE-RECORD
id: NTSD-OPT-H07-PAIR-SNAPSHOT-REUSE-031
status: RUNTIME_PENDING
status-correction-history: test-first written; production unchanged; RED pending
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
authority: approved H07 continuation Goal contract section13; pure snapshot reuse only with formal336 cadence and default unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH31-PAIR-SNAPSHOT-REUSE-20261007/REPORT.md
-->

# H07 同一精确方向关系快照复用

最新：两真实1000AI短窗限定行为/关闭验证已完成，120/120 PASS；RUNTIME_PENDING仅指正式性能/默认准入和适用权威证据未齐。局部1000参与者约21%降耗，真实分散48.42→44.16ms、混战45.72→46.90ms、可见帧158–162ms仍慢，PERFORMANCE_INCONCLUSIVE / NO_DEFAULT_PROMOTION，不称FPS修复。两65字段request仅output不同、末tick300十hash/完整JSON同；capacity关键拒绝0。raw zeroGcGatePassed=true来自未校准counter，suite zeroGcPassed=false/UNKNOWN，不撤回H11完整可靠采样FAIL。原菜单一次、十一阶段三残留0/Menu8roots clean/双SceneSHA同，232保护/11备份/HEAD与三C#运行前SHA保持。最终命令、失败原件、局部成本、两实测及证据边界见REPORT.md、comparison-01.json、post-run-audit-01.json，最终文档后审计回链final-audit-01.json。未实施默认切换/EXT1/Mono/ATLAS或资源改造，H07/H11/Goal仍OPEN。

人工diff复核：共享helper只搬原record主体，factory的frame/state/oid/type/team/facing/holder输入未被候选record写回；多itr/body、nearest RNG、Kind4每body计数、不同方向/下次collection及现有110回归覆盖。旧brute仍走捕获入口，但多一helper调用，其默认性能没有本批实测，不能宣称默认brute成本完全不变。没有新持久资源/lease或关闭模块，复用原owner。剩余为完整性能/逐tick适用native准入/0GC，而不是再跑已闭合夹具。

实际命令：原Editor run_tests/get_test_job、单次execute_menu_item；Get-Content/Get-FileHash/git diff/status/Tools/Validate-ChangeLedger.ps1。原件及原菜单/suite受控现场保存，未启动第二Editor/Profiler/GPU capture，未破坏性Git、删除或push。两窗口StoppedCleanly/harnessValidity=true，suite DONE仅表示窗口完成。此前RUNTIME_PENDING段保留为当时历史。

原PID19040/6401在Menu8roots clean/idle后独立31菜单仅调用一次；两个request为120+180真实1000AI、role/strict门保持，正在进入Battle/服务预热。终态/成本/末tick尚待，不称运行通过。运行期间不改C#/Assets文档、不refresh/重启/额外测试；数据输出新目录，旧30shared终态已备份。

GREEN-02原Editor jobec8f93e0fca04ce19ff21d6a146a82b4实际120/120 PASS/5.1263s。7新增collector+3 request以及原相关回归通过；局部1000参与者固定16+16 collector fixture13.960275→11.0231125ms、capture92160→15360（8warm、交替order），局部约21.0%收益，不是1000AI/FPS/0GC证书。准备一次两个真实1000AI smoke，末tick/整tick收益仍待。pre-run-audit-01.json保护232/backup11/HEAD保持、validator PASS；初次审计输出因未转InformationRecord为string而截断/JSON解析失败，未据此发证，已修正序列化后同只读审计成功。

GREEN-01实际120完成、2个新增fixture失败，其余未报失败：成本fixture漏设Role mode读到0 capture；误把kind7当nearest（当前既有IsReleaseNearestCandidatePath明确vrest0且排除1/2/7）。已仅修test明确Role与kind0/vrest0 nearest正例，增加nearest候选1/RNG>0断言；生产规则/实现未改来迎合测试。green-failed-01.json保留。下一同两类重新GREEN，未启动Scene窗口。

CODE_WRITTEN：Runtime仅局部cached direction的惰性stack快照与record共享入口；仍每body执行原record全部门/副作用，legacy/fallback原Capture。Suite新增独立31两smoke，仅output变、原3入口保持；新测试诊断flags仅UNITY_INCLUDE_TESTS，不改变逻辑snapshot/checksum。尚未GREEN或实测，不宣称收益。

有效编译后RED job a5bc6e737a074db5b91582e5b978b649实际10失败：6 collector缺少预期capture control、3 request预期NotImplemented，另1成本fixture误用默认Authority400在slot400被拒，不能把该失败称优化RED。fixture已显式采用DesktopExtended/2000，仅诊断，与生产默认无关；下一GREEN仍必须1000真实逻辑参与者和两个真实1000AI窗，原件red-01.json保留。一次6401拒绝是原PID重载瞬态，没有重启/换Editor。

RED准备首次失败保留：新增test block误放同文件Shadow类、CandidateRun属于Formal类，原Editor出现两CS0246。首次job d8561a7883144326be20eb52cb81e1de实际0用例/旧程序集，NOT_A_TEST_PASS；不当作RED。已在本批移动新增block到Formal类（只新增hunk，不改原helper），下一有效编译/10 RED。生产Runtime仍未改。

test-first已写7 collector case与3 request case；旧Runtime未改。collector通过反射验证尚不存在的逐body control/capture count，request stub NotImplemented。下一原Editor编译后只10项RED，不测1000AI或宣称收益。测试主体固定全PairSnapshot/顺序/RNG/Kind4计数；fixture A/B只1000逻辑实体不代替实际AI。

PLANNED。事前Task：[第31批](../TASKS/NTSD-OPTIMIZATION-BATCH31-PAIR-SNAPSHOT-REUSE-20261007.md)。既有每body重复Factory读取，在一次cached direction栈上惰性复用，不跨tick/实体方向。原kind/team/selection/nearest/capacity/RNG、每body副作用/候选顺序保持；legacy/fallback不变。测试诊断开关只比较同Role路径旧/新capture，不默认启用其它collector。

准确3 code-path已登记，11当前备份/232非写域/HEAD已存[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH31-PAIR-SNAPSHOT-REUSE-20261007/RECORD.md)。原Editor idle非Play。下一7 collector＋3 request RED，最小实施，原两类GREEN、局部1000参与者成本A/B与一次两真实1000AI smoke/末tick300对照；不跑旧四formal/GC校准/全套。新增无heap或持久缓存/lease；完整0GC仍须实际证明，不用未校准counter发证。

不可回退边界：formal336/33ms/3ms/max2/checksum/RNG/publication/segment/fail-closed/11阶段不改，不写Q06/Scene/资源/Settings/Server，不切默认或解冻专项门。副作用只有测试观察计数与减少纯读取，关闭归现有owner；不新增关闭模块。回滚需准确批准，使用Operation当前字节最小patch，不丢其他改动。父H07/H11/Goal保持OPEN，有限交付5/6与FPS优化未完成分别报告。执行命令、失效证据、验证与风险后续追加。
