<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-CANONICAL-HELD-SPATIAL-WITNESS-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C043FusionHeldBattlePlayProbeEditor.cs
authority: 336B44 natural OID8 to OID420 held chain and user D-024 ratio requirement
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-CANONICAL-HELD-SPATIAL-WITNESS-001.md
-->

# Q07/D-024 canonical 非武器持有比例只读见证

脚本修改前建立。原状、精确路径/符号、预期副作用、风险、验收和回滚见同 ID Task。仅扩既有 C043 Battle Scene 探针的 TickRow 与唯一 runId 接口；无生产逻辑、DAT、Scene、Prefab、Input Actions 或非战斗改动。完成后据实际编译、Play、正式 CSV、比例误差、退出与哈希更新状态；探针子出口不能自动关闭 WPOINT 父包或 Q07。

实际只在 `NTSD28Q07C043FusionHeldBattlePlayProbeEditor.TickRow/MeasureOneTick/TryStart` 加10个只读源/物理字段和一个唯一runId，不更改原60tick输入、初态、生产调用或旧结果。编译0 error；原Editor Battle Scene完整60tick `NATURAL_GATE_PASS`，21个非武器持有tick画面距离比例最大误差X0.915229/Z0.232877输出像素；旧22字段×60tick共1320/1320原值无差，原Scene退出clean/借用0/六SHA稳。后续只读复用当前336B44根EXE两次同SHA trace，指定输入/初态的存在性和源X/Z经根载体Z偏移归一后246/246零差；根报告`nativeParityClaim=false`，WPOINT父包和Q07不关闭。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-CANONICAL-HELD-SPATIAL-WITNESS-001/REPORT.md)。

交付检查：`Tools/Validate-ChangeLedger.ps1`退出0，报告`Change ledger validation PASSED`（1147 Records，23个已列代码diff受覆盖）；原始输出归档在[校验日志](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-CANONICAL-HELD-SPATIAL-WITNESS-001/change-ledger-validation.log)。`git diff --check`退出0、空白错误0。该探针脚本此前即为未跟踪用户工作，仍未跟踪；本次不通过Git清理/重置，也不覆盖既有结果文件。
