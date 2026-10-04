# Q09 普通第 5 战斗组姓名牌字图选择

2026-10-04；正式根 EXE 身份 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本包只修复战斗中普通关系组 5 姓名牌选图，不部署剧情 `stage.dat`，不修改 DAT、PNG、Scene、菜单或 Unity 框架。

正式 playable `render_snapshot.cpp::native_glyph_resource_slot` 对普通组 1～5 的姓名牌分别选 `WORDS1～5`，复活次数字选 `WORDS0`；`game_session.cpp` 剧情出生可映到组 5，正式剧情 DAT 存在 `team: 5`。正式与 Unity 暂存的六张 WORDS 图逐文件 SHA 同版。Unity `BattleEntityOverlayLayout.ResolveRelationSheet` 原来只认 1～4，使普通组 5 落回 0；特殊 Com 独立选 5，因此不能替代普通组覆盖。

测试先行：改前独立纯 C# 布局探针返回 `ok=True count=3 counter=0/0 label=0`，与正式 `label=5` 不符，exit 1。随后只将共用选择器上限由 4 改为 5，并同步旧 SelfCheck 断言。改后组 0～6 与特殊 Com 探针 exit 0：`group0=0/0/0`、组 1～5 各自 `0/0/组号`、`group6=0/0/0`、`specialCom=5`。第 5 组复活次数仍用 0。

验证层级：

- 生成项目 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q`：exit 0，0 error、328 warning。
- 原 Editor 在保存干净的 Battle Scene 中刷新并完成编译；运行前 idle、非 Play、非编译，Editor DLL 时间晚于三处脚本源。
- 精确三项 EditMode 作业 `670879da596e43498500ab8374adb90c`：`succeeded`，3 completed、0 failure；用例为新组 5 姓名牌/复活次数、既有 inclusive-right-edge 布局、特殊 Com。原始作业状态见 [focused-3of3.json](focused-3of3.json)。后续查询时持久作业结果对象为 null，但原运行当时返回 3/3 Passed 摘要。
- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path`：exit 0，PASSED，16 个当前脚本 diff 均有 Record 覆盖；其大量“Record 声明旧路径不在本次 diff”提示属全仓历史。限定 `git diff --check`：exit 0。

测试操作事故：误将 `run_tests({job_id: ...})` 用作状态查询，接口忽略该参数并启动全套 EditMode 作业 `7da68dc008ee442eab7dc937bf822b0c`。正确只读接口是 `get_test_job({job_id: ...})`。事故作业在 1152/8823 项处停滞，`stuck_suspected=true`；已观察到的全套失败属于该额外作业，未分析或归因，不能作为本包回归结论。Unity MCP 的 `clear_stuck` 只清除作业管理记录；`execute_code` 因 Mono CodeDom 命令行过长、Roslyn 不可用而不能调用 Unity 官方取消路径。用户已在原 Editor 手动取消。取消后 Editor `idle / is_running=false / non-Play`、Battle Scene `isDirty=false`，Battle/Menu 磁盘哈希分别仍为 `D8C01FD3...5AFE7F` / `9EAAA0B4...6C1BA`；随后仅清理残留 job 管理记录，最终 `failed / Job cleared manually (stuck or orphaned)`。原始运行状态见 [accidental-suite-status.json](accidental-suite-status.json)，清理后状态见 [accidental-suite-cleared.json](accidental-suite-cleared.json)。这不是全套测试的有效失败或通过报告。

本包状态 `FOCUSED_TEST_PASS`，只关闭共用选图与聚焦检查子门。正式根 EXE 同条件 GPU Present、原 Unity 真实剧情组 5 Game View、Q09/Q12 完整视觉门均未验。Battle Scene 的 HUD 修改来自另一任务/用户，本包没有修改或回退。
