<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-MENU-D-PLAYERLOOP-ONE-TICK-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09NameplateNaturalPlayProbeEditor.cs
authority: 336B44 battle input path and existing Hidan PlayerLoop input-device diagnostic pattern
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-MENU-D-PLAYERLOOP-ONE-TICK-001.md
-->

# Q07 Menu D PlayerLoop 单 tick 输入链诊断

脚本前记录。既有 Menu→Battle Editor 回调即时 typed Dynamic 注入在单 tick 前设备 D=false、MoveAction/canonical零；另一次即时 `InputSystem.Update()` 得设备 D=true/MoveAction零。两者不能证明玩家键故障。现有 Hidan 探针的可复用成功相位为设备事件排队、Unity PlayerLoop 正常 Dynamic 更新完成、随后调用同一生产 `StepOneTick`。当前 Editor PID105896/本项目 MCP6401 现场查询为非Play、idle、单一 clean Menu；切场景及 Play 前仍需复核。目标脚本 SHA-256 `A32D13860462F396403352052C8065657FF92900F3A724107BFE5E02DE9573A0`。

仅在既有 Editor-only nameplate probe 增加 opt-in PlayerLoop 一 tick 菜单入口、相位/设备/动作/canonical诊断和唯一结果路径；原长跑、截图、既有即时诊断行为保持。预期副作用是一轮 Menu additive Battle Play、合成 D 设备事件、一个生产逻辑 tick 及其结果文件；通过原有有序卸载/退出释放，四保护文件不写。失败原件也保留。正式 EXE/源、DAT/PNG、Unity 生产、场景和非战斗逻辑均不编辑。验收、回滚与不升格边界见 [Task](../TASKS/NTSD28-336B44-Q07-MENU-D-PLAYERLOOP-ONE-TICK-001.md)。当前 `PLANNED`。

2026-10-04 代码与生成编译：实际只改声明的 `NTSD28Q09NameplateNaturalPlayProbeEditor.cs`。新增独立 MenuItem/结果路径、Dynamic `onAfterUpdate` 计数、排队 D 后等待下一次 PlayerLoop Dynamic 更新、单次完整 Driver tick 前后字段和 canonical Right 判定；默认姓名牌/WORDS及旧即时单tick入口不进入新分支。新增回调在 `Finish` 解绑，旧暂停/卸载出口复用。`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q -clp:ErrorsOnly` exit0、297 warning、0 error；本脚本 `git diff --check` exit0。原Editor导入、单次Play、保护哈希/关闭结果待，状态`COMPILE_PASS`，不能称输入链已有效。

2026-10-04 限定出口：原Editor导入0编译错误，单次原Menu→Battle Play 的PlayerLoop Dynamic 1978→1979；D设备=true且绑定同ID1，但Action/回调/canonical Right=0，一次完整Driver tick3→4、X620不动。原Editor退出非Play/idle、单一clean Menu，四保护SHA与预检同。记录设备→Action首个已观测断点；Editor失焦路由是源码支持的假说而非确定根因，不称玩家物理键/生产逻辑故障。未改生产/DAT/图/Scene/非战斗，原始阴性保留。`VERIFIED`仅指此有界探针完成；父Q07开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-MENU-D-PLAYERLOOP-ONE-TICK-001/REPORT.md)。

交付检查：`Tools/Validate-ChangeLedger.ps1` exit0（[完整输出](../../../artifacts/diagnostics/NTSD28-336B44-Q07-MENU-D-PLAYERLOOP-ONE-TICK-001/change-ledger-validation.txt)；仓库既有旧记录路径警告未当失败）；四保护SHA再次逐一等于预检。`git diff --check` 针对本脚本 exit0；对包含既有脏文档的较宽范围检查 exit1，仅报 `CODEX-CURRENT-HANDOFF.md` 与 `STATE.md` 原有EOF空行，本包只在这两文件头部插入进度，没有编辑尾部。原Editor已按用户确认切到单一干净Battle Scene，非Play/idle；本次未重复F02既有45tick验收。
