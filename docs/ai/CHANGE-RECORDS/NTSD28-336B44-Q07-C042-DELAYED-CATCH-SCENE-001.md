<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C042-DELAYED-CATCH-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C042NaturalThrowBattlePlayProbeEditor.cs
authority: selected formal 336B44 playable BattleWorld28 catch and throw path, natural OID75 delayed-catch source and formal-root paired LFR
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C042-DELAYED-CATCH-SCENE-001.md
-->

# C042 延迟自然抓取非零计数 Scene 探针

脚本前记录：现有 C042 Unity 共用投掷 writer 已删除被投者计数的错误清零，聚焦392例×两 profile 通过；原 Scene X550 自然投掷前计数0，只证明一般投掷链。当前正式 DAT/源码完整会话和根 EXE 同 LFR 新证据给出 X650/tick2 抓取→tick56 投前计数1、X800/tick4 抓取→tick58 投前计数3；两组各160 tick×20字段3200/3200零差。正式抓取不清被抓者计数，kind2 帧暂停普通计数推进，投掷仅清抓取者计数。Unity 对应分支尚无原 Scene 非零正例。

只参数化现有 `NTSD28Q07C042NaturalThrowBattlePlayProbeEditor.Request.targetX`、`TryStart` 的目标位置/请求验证，继续用同一 Play 克隆与生产 Driver、60 tick 和独立结果。预期副作用仅是新诊断 JSON 和临时请求文件；不修改生产写者、DAT、资源、Scene、用户内容或非战斗逻辑。请求限已证位置，拒绝覆盖已有结果。成功出口、失败保留、保护 SHA 与回滚方式见同 ID Task。

本段为脚本修改前的登记；修改后必须追加实际符号、编译、原 Editor Play、逐 tick 首差、账本和未验项，不能把正式源/根证书当成 Unity PASS。

实际脚本差量：`Request.targetX` 保存受控源规则目标 X；`TryStart` 只接受正式已证的 X650/X800 及原 X550，并核 runId 与 X 一致；`Report.targetX` 从请求读取。原先写死 X550 的测试入口扩为相同生产 Driver 的三个受控位置。生产战斗代码、DAT、资源与 Scene 未改。旧 X550 证据不重跑。回滚仅本探针三处精确差量，需按仓库批准规则执行。

2026-10-02 验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo '-clp:ErrorsOnly;Summary'` 0 error、251 warning；原 Editor 刷新并将修改后探针编入测试程序集。正式源码与正式根 X650/X800 各160tick×20字段3200/3200；原 Battle Scene 两例各60tick×20字段1200/1200，首差0。两次报告 `CAPTURED/DONE`、退出Play、Scene clean，四保护资产前后SHA一致。X650投前/完整tick后计数1/1；X800投前/完整tick后3/1，后者因新动作帧入口正常重置。未测对象池借用计数、完整World、物理按键和其它动作；聚焦通用 writer 证据见父任务。定向探针状态 `VERIFIED / NATURAL_NONZERO_SCOPED_PASS`，C042/Q07保持开放。报告 `artifacts/diagnostics/NTSD28-336B44-Q07-C042-DELAYED-CATCH-20261002/REPORT.md`。

审计：`Tools/Validate-ChangeLedger.ps1` PASS（1134 Records；既有冗余声明 warning），`git -c core.safecrlf=false diff --check` PASS；本包没有重新运行全量 SelfCheck。
