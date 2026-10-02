<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: selected 336B44 playable WorldAudioEvent28.world_x and original Battle Scene actual PendingSounds
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001.md
-->

# Q07/C053 命中投影音效坐标共用入口

脚本前记录。RED证据：[字段报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-FIELD-001/REPORT.md)首次投影X897/真实与正式X584，数量/cue/tick/排队/拒绝同；[Q10实际音频](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001/REPORT.md)10条路径/X/顺序42/42同态。Unity现状是真实 `QueueBattleSound` 调用 `NTSDEntityRuntime.ResolveBattleSoundWorldXInt`，只读投影仍将实体 `Runtime.XInt` 直接记为声音X。计划在 `ProjectQueuedSound` 共用末端加入发声实体，所有实体音效调用显式给出emitter；保留原fallback X，统一用现有Runtime方法。避免单案例特判、重复比例常量或DAT改值。

准确脚本范围、不可回退边界、验收、风险与回滚见Task。主要风险为各调用点发声实体映射错误、旧无SourceRulePosition场景fallback被改变、投影掩盖其他首差；通过方法签名强制每个调用点声明emitter、现有聚焦测试及真实Scene红绿/实际事件对照控制。未修改任何脚本前建立本Record；完成后补实际符号、diff和证据。

实际修改：`BattleEcsHitExecutionPlan.ProjectQueuedSound` 增加发声 `LF2Entity emitter` 与原 `physicalFallbackX`；方法末端统一通过 `emitter.Runtime.ResolveBattleSoundWorldXInt(physicalFallbackX)` 决定写入投影指纹/WorldX，未初始化或无emitter时仍使用原物理 fallback。17个实体声音调用按生产`QueueBattleSound`对应发声者传 attacker/target，包含受害者或攻击者条件分支；Cue、顺序、数目和真实 writer 未改。原请求式测试探针改新唯一Green RunId并要求两自然命中计划观测零差，旧RED与Q10音频JSON保留。生产DAT、Scene、Prefab、ProjectSettings、菜单均未改。

编译：Temp-only targets 纳入探针；`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` exit0、281 warnings/0 errors，[日志](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001/offline-editor-build.txt)。原Editor导入及Scene运行待；现仅`COMPILE_PASS`。

后继验收（覆盖上段待测快照）：原Editor导入后同三人Battle Scene12完整生产tick `SCOPED_PASS / DONE`，所选战斗132/132、实际待播音频42/42同正式源，tick7两自然Damage且ShadowCompare观测/失败0、writer掩码0；退出非Play/idle、Scene clean、四SHA稳。直接声音fallback EditMode 1/1 PASS。扩展三个方法组的34项中17项失败：15项`target.HitStateCount`45/0，2项writer掩码无bit63。失败原件保留，不在本包修测试或非音频writer；全分支证据未达，故保持`RUNTIME_PENDING`。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001/REPORT.md)。

字段定位更正：本段初版把15项`45/0`误记为vrest；当前逐行核对测试断言实际为`target.HitStateCount`，后续vrest断言尚未运行到。JSON原件不变。

`Tools/Validate-ChangeLedger.ps1` exit0/PASSED，1122 Records、当前diff 3脚本均COVERED，[原件](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001/change-ledger-validation.txt)；`git diff --check` exit0，只有行尾转换提示。未运行整套SelfCheck或Player/device；两次聚焦任务分别为34项17失败及单项1/1成功，不能合并成全通过。
