<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-TOBI-NATURAL40-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053TobiNaturalBattlePlayProbeEditor.cs
authority: selected 336B44 root EXE and corresponding playable OID0 Tobi natural 40 tick chain
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-TOBI-NATURAL40-SCENE-001.md
-->

# Q07/C053 Tobi 自然出生链后26 tick 原场景对照

脚本修改前登记。原状：同一原Battle Scene的OID0 Tobi/OID251自然出生前14tick八字段112/112已通过，正式源/根有同输入40tick八字段320/320，但Unity后26tick无证。拟在现有Editor-only探针中增加独立40tick菜单、目标tick序列化和独占结果路径，并补外部Play提前退出留痕；保留旧14tick入口与原结果。预计副作用仅Editor诊断菜单和新结果文件；不得修改生产、DAT、图、Scene、Prefab或非战斗。验收、风险和回滚见[Task](../TASKS/NTSD28-336B44-Q07-C053-TOBI-NATURAL40-SCENE-001.md)。实际修改与验证结果在写脚本后追加。

实际脚本已写：同一Editor探针增加40tick菜单、独占结果/残留路径、Report序列化目标tick数；既有14tick菜单委托相同启动方法，旧输出路径及输入保持。`MeasureOneTick`按目标tick数退出，`Finish`按目标选择独占路径，外部Play提前退出按`INTERRUPTED`保留诊断。无生产、DAT、图、Scene、Prefab或非战斗修改。编译、正式CSV配对、原Scene Play及残留检查待完成，状态暂为`CODE_WRITTEN`。

2026-10-04 验证追加：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` exit0/0 error/299 warnings；原Editor刷新、导入并完成真实Battle Scene Play。第一次菜单因Scene dirty安全拒绝，第二次40tick原件齐全；[机械配对](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL40-SCENE-001/tobi-jump-natural-40-paired.json)对当前正式CSV八字段320/320同。Play内Battle/Menu/GameConfig/ModeAsset四SHA同；[独立残留](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL40-SCENE-001/tobi-jump-natural-40-postplay.json)原Scene Driver World0/Pool0、Battle SHA同且Scene clean。仅此限定场景子门`VERIFIED`；物理设备输入、全World、其它自然条件、C053/Q07/Q12仍未验。详[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL40-SCENE-001/REPORT.md)。
