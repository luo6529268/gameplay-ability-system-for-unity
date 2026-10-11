<!-- CHANGE-RECORD
id: NTSD-H07-INPUT-SPIKE-CPU-DIAG-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H07InputCostDiagnostics.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/SimulationWorld.DetailTimingDiagnostics.cs
authority: User requires further H07 input diagnostics until a precise repair handoff is supported
evidence: artifacts/diagnostics/NTSD-H07-INPUT-SPIKE-CPU-DIAG-20261010/REPORT.md
-->
# 输入尖峰CPU有限诊断
事前PLANNED。需求、写域/符号、默认OFF、校准/两窗/逐tick/GC/关闭/有限出口与回滚详见同Task及Operation。现状已有elapsed证据与当前2个Mono.JIT栈，但缺CPU执行量，不能归因全部尖峰。
新增副作用限可选诊断provider和Editor计数；热provider只当前线程周期，父子不相加、不将周期转换时间。生产语义/输入时序/权威/最终门不变。未测试，不称性能通过。

CODE_WRITTEN：三域诊断已写，7段可选周期provider默认null、两窗CPU取证、同raw冷解析。尚未编译/测试/实景；实际验证后更新。

## 编译和focused检查点
原Editor脚本reload完成、当前Console无CS编译错误；CPU四项4/4、既有NativeSampleTiming复位1/1通过。两份原始job结果已保留；同raw冷复用导出176主线程steady tick(125-300)，settingsRestored=true。开始唯一OFF→ON CPU诊断组，非资格/收益证书。

## 最终更正与验收
上文事前“未编译/未测试”已由本节取代。原Editor编译和5/5 focused、同raw176tick冷复用、唯一无Profiler两窗各120+180均完成，14字段逐tick0差异、flags/invalid0、CPU校准与可靠GC覆盖通过。两窗实际GC事件2/0，均非证书，0GC资格未通过/未授予；只诊断实现VERIFIED。独立报告与CPU分析/窗口原件齐全。15562保护SHA0漂移/0缺失、11份备份匹配，原Menu clean8/关闭objects slots borrowers0。准确候选修复范围已具名写execution-handoff；本会话未改生产业务代码。

交付检查：Validate-ChangeLedger PASSED1403/exit0、git diff --check exit0；原日志完整保留。不把历史Record warning当本轮失败。
