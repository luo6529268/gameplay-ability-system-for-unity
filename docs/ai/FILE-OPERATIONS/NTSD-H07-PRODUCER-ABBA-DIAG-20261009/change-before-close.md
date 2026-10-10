<!-- CHANGE-RECORD
id: NTSD-H07-PRODUCER-ABBA-DIAG-001
status: IN_PROGRESS
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H07ProducerAbbaDiagnosticEditorTests.cs
authority: User approved limited combat ABBA diagnosis only, production remains OFF
evidence: docs/ai/TASKS/NTSD-H07-PRODUCER-ABBA-DIAG-20261009.md
-->
# Producer ABBA冷诊断
脚本前登记。准确符号、原状、风险、副作用、owner容量关闭、必要测试及回滚见Task。原历史入口语义保持，生产guard/World/Host不改。

CODE_WRITTEN：Suite明确ABBA case与inputBoundaryTiming=false冷owner，旧Apply签名/奇偶语义保持；17新mapping/owner/observation case＋两Explicit设置快照。生产代码无改动，未运行窗口。
