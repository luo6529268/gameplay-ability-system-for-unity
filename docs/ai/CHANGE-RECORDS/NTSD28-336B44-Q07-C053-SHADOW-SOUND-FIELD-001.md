<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-SHADOW-SOUND-FIELD-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: selected 336B44 playable audio_events and same-seed original Battle Scene actual PendingSounds
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-SHADOW-SOUND-FIELD-001.md
-->

# Q07/C053 ShadowCompare 音频 bit63 字段首差

脚本前记录。现状：`BattleEcsHitExecutionPlan.DifferenceMask` 把 PendingSoundCount/Fingerprint/Cue/WorldX/Tick 和 Queued/Rejected 合并为 bit63；旧两轮原 Scene 只知道两次writer bit63失败，无法判具体分量。Q10后继证实正式/Unity实际待播10条事件一致（42/42），因此不得把 bit63 当作正式音频首差。计划在 `ObserveLegacyWriterEffect` 第一次 bit63 失败时保存七字段的期望/实值，仅加到既有 Diagnostics；测试探针重启只读 ShadowCompare 并记独立结果。无新 gameplay 分支，正常路径不分配诊断字符串。

受影响符号、前置、预期副作用、验收和回滚见Task。只在已存在差异时形成诊断文字，既有观测计数/掩码、真实 writer、QueueSound 不变。修改后登记实际 diff、编译、原场景结果及未验边界；若字段首差确定，再单独审查是否修改投影。

实际代码：`BattleHitExecutionPlanDiagnostics.FirstSoundEffectDifference` 只承载首次音频bit63的七字段期望/实际串；`ObserveLegacyWriterEffect` 在既有 `DifferenceMask` 之后、已判bit63时写入一次，Reset清空。原测试探针使用新独立请求/结果路径，在World tick0重启只读ShadowCompare，并把该串写入每tick JSON；`FIELD_OBSERVED` 只表示案例与字段首差如期复现。真实writer、投影逻辑、QueueSound与声音播放均未改。该复用探针的前一Q10请求入口已被新RunId取代，历史结果不动。

编译：Temp-only targets 显式纳入测试脚本；`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0，281 warnings、0 errors，[日志](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-FIELD-001/offline-editor-build.txt)。原Editor导入、Play及字段实测待，当前仅`COMPILE_PASS`。后续差异归因或投影修复不属于本Change。

后继验收（覆盖上段待测快照）：原 Editor 已导入并在原Battle Scene完整Play12tick，`FIELD_OBSERVED / DONE`；132/132所选战斗字段同正式源，两次writer bit63再现。首次七字段除 WorldX 897/584 与随X变化的Fingerprint外全部一致，正式音频原始事件世界X584。退出Play、Scene clean、四SHA前后一致。[报告及原件](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-FIELD-001/REPORT.md)。实际音频生产逻辑未改；投影修正、其它命中和物理键仍待独立任务。

`Tools/Validate-ChangeLedger.ps1` exit0、PASSED，1121 Records/当前diff 3个受治理脚本均COVERED；历史非当前diff声明仍有WARNING。[校验日志](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-FIELD-001/change-ledger-validation.txt)。
