<!-- CHANGE-RECORD
id: NTSD-H07-INPUT-FLOW-DIAG-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H07InputCostDiagnostics.cs
authority: User requests completing H07 input diagnostics before handing implementation to another chat
evidence: artifacts/diagnostics/NTSD-H07-INPUT-FLOW-DIAG-20261010/REPORT.md
-->
# H07输入流程补充诊断
修改前PLANNED。需求/写域/owner/副作用/最小验证/停止/回滚详见Task NTSD-H07-INPUT-FLOW-DIAG-20261010及同Operation。
原状：兼容18万skip已证；旧OFF完整Driver2event、ON未跑、后段/尖峰归因不足。
预期：Editor默认关闭的独立一窗观察，填全42槽与slot访问、tick marker/当前分配栈；不改业务/默认、不降低0GC。
风险：Profiler与逐角色时钟扰动、live回调缺失；只用同raw恢复，不重跑找PASS。
不可回退边界：33ms/确定性/publication只读/透明顺序与failclosed/容量0GC/11阶段关闭；producer暂缓、H06不续办、95OFF。
验证PENDING：原Editorcompile/focused/唯一诊断窗/逐tick对账/保护/Ledger。

## CODE_WRITTEN
两Editor增量已写：Suite独立一窗冷路由与INSTRUMENTED标记、helper完整槽/slot访问/300冷tick marker、旧GC observer组合和既有capture复用；同raw追加加载的冷恢复菜单，不清历史。未修改任何生产脚本；compile/focused未运行。

## 前置验证
原Editor最新compile/reload idle、Console无CS错误；具名8/8 EditMode PASS，含4个新增guard/request边界与4个既有输入测试。focused-result-01.json；尚未启动实景。

## 最新工具交付与范围
VERIFIED仅Editor有界观察：原Editorcompile、8/8 focused、唯一300tick全输入/当前GC栈采集与恢复、14字段逐tick0差异。生产未改，无优化收益声明。完整Driver0GC仍FAIL（2event），Profiler扰动计时根因未全证实，Task SCOPED_DIAGNOSIS_COMPLETED_WITH_LIMITS / NO_PRODUCTION_HANDOFF。
实际两域增量为Suite25增4删、helper214增7删，相对Operation原dirty字节。10before备份SHA匹配；15565清单中2事先声明Assets文档变化，其余15563保护一致，0未知变化/缺失/新增。settings恢复、11阶段关闭objects/slots/borrowers0、Menu clean8、Scene哈希保持。
证据为同Task REPORT及focused-result-01.json、diagnosis-summary.json、current-gc-callsites.json、protection-result.json、scene-after-parse.json；validator/diff交付结果随后追加。不增加生产默认/新缓存，不动暂缓/专项，不读Q06体。此前PLANNED/CODE_WRITTEN/FOCUSED与“尚未实景”为对应历史时点。

## 最终交付检查
2026-10-10原工作树执行Tools/Validate-ChangeLedger.ps1：exit0 / Change ledger validation PASSED / Records1402；历史Record声明不在当前diff中的警告保留于change-ledger-validation.log，没有新增未覆盖脚本错误。git diff --check：exit0，仅既有/已声明文档LF→CRLF提示，无whitespace错误。两条检查都不等于性能/0GC/Android通过。
