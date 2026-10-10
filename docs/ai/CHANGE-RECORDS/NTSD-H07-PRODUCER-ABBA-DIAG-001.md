<!-- CHANGE-RECORD
id: NTSD-H07-PRODUCER-ABBA-DIAG-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H07ProducerAbbaDiagnosticEditorTests.cs
authority: User approved limited combat ABBA diagnosis only, production remains OFF
evidence: docs/ai/TASKS/NTSD-H07-PRODUCER-ABBA-DIAG-20261009.md
-->
# Producer ABBA冷诊断
脚本前登记。准确符号、原状、风险、副作用、owner容量关闭、必要测试及回滚见Task。原历史入口语义保持，生产guard/World/Host不改。

CODE_WRITTEN：Suite明确ABBA case与inputBoundaryTiming=false冷owner，旧Apply签名/奇偶语义保持；17新mapping/owner/observation case＋两Explicit设置快照。生产代码无改动，未运行窗口。

最终：2026-10-10 新具名ABBA诊断已完成：OFF90.5245/92.5756ms，ON92.8389/77.3209ms，未复现开启后稳定更慢，ON状态内波动15.518ms，不能宣称稳定收益或具体因果。首OFF校准有效完整Driver稳态2次GC.Alloc、首tick130，后三窗0；本组GC门FAIL、调用点未知。33+1+1测试及配置/20hash/末态/容量/关闭/设置恢复通过；producer生产OFF，Role默认保持，未自动修复/推广/转下一项。Change NTSD-H07-PRODUCER-ABBA-DIAG-001 VERIFIED仅冷诊断实现与记录；Task NTSD-H07-PRODUCER-ABBA-DIAG-20261009 SCOPED_DIAGNOSIS_COMPLETED，H06父/H07/Android开放、95OFF、旧Goal不恢复。详 artifacts/diagnostics/NTSD-H07-PRODUCER-ABBA-DIAG-20261009/REPORT.md；下方IN_PROGRESS/旧准入状态保留历史。
实际命令/源与旧证据复用/未执行项/失败门/回滚见报告。只新增冷case owner和诊断入口，无生产state/cache，原11阶段顺序不变。

最终校验：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity 实际exit0/PASSED；Records: 1397；Governed code files in diff: 7；warnings=4417（保留历史声明路径等警告，非零警告）。git diff --check exit0。最后MCP确认原PID39108 idle/Menu，非Play/编译/测试，未重启或保存Scene。
