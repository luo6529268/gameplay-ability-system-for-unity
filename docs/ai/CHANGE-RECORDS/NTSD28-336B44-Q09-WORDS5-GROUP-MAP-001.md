<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-WORDS5-GROUP-MAP-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs
authority: 336B44 formal root identity and matching playable render_snapshot nameplate glyph selection
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-WORDS5-GROUP-MAP-001.md
-->

# Q09 WORDS5 第五战斗组共用字图选择

脚本修改前建立。正式 playable `native_glyph_resource_slot` 对组1～5选对应WORDS；正式剧情数据有team5，正式/Unity六图逐SHA同版。Unity普通关系组5在 `ResolveRelationSheet` 被回退到0，且旧SelfCheck保存该旧预期。当前只是静态条件首差，尚无根EXE同场景GPU画面证书。

预期改动限三处声明路径：聚焦测试先RED；共享布局选择器使普通组5用WORDS5，复活次数继续WORDS0，其他fallback不变；SelfCheck更新旧组5断言。正式DAT/PNG、Scene、非战斗和框架不动。实际改动、命令、失败/未验与状态在实施后追加；回滚策略见Task。

2026-10-04 实施：先在 Editor 测试文件增加普通关系组5的姓名牌/复活次数聚焦用例。独立 `Add-Type` 仅编译布局文件的改前运行返回 `ok=True count=3 counter=0/0 label=0`，与正式预期 label5 不符（RED，exit 1）。随后仅将 `BattleEntityOverlayLayout.ResolveRelationSheet` 的有效上限4改为5；`BattleRuntimeSelfCheck.CheckBattleEntityOverlayLayoutContracts` 同步更正旧断言。改后同一纯 C# 探针覆盖组0～6及 special Com，返回 `group0=0/0/0 ... group5=0/0/5 group6=0/0/0 specialCom=5`（GREEN，exit 0）。三个声明脚本以外未作本包代码修改。

验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q` exit 0、0 error、328 warning；原 Editor 刷新后 `Assembly-CSharp-Editor.dll` 时间晚于测试源，Editor 状态 idle、非 Play、非编译，单一干净 Battle Scene。原 Editor `run_tests` 用三项完整 `testNames` 的 EditMode job `670879da596e43498500ab8374adb90c` 返回 `succeeded`、3/3 PASS、0 fail（本用例、旧边缘布局用例、特殊 Com 用例）。`git diff --check` 对三脚本 exit 0。根正式 EXE 同条件 GPU、真实剧情 group5 画面、Q09/Q12 总门仍未验证；本包只关闭组5选择器/聚焦测试子门。

测试操作事故：首次三项 job 已成功后，用错误的 `run_tests({job_id: ...})` 查询结果，该接口把参数视为新运行，误启全套 EditMode job `7da68dc008ee442eab7dc937bf822b0c`（约8823项）。已确认正确只读接口为 `get_test_job({job_id: ...})`。该全套作业的结果与本包聚焦3/3分开记录；不把其已有失败归因于本补丁，也不再提交其他测试。末态见证据报告。

事故关闭：用户已在原 Editor 手动取消；`get_editor_state` 确认为 idle、非 Play、tests.is_running=false，Battle Scene clean，Battle/Menu 磁盘哈希同本包前快照。随后 `run_tests({clear_stuck:true})` 只清理残留管理记录，job 标记 `failed / Job cleared manually (stuck or orphaned)`，不解释为功能测试失败。完整情况见 [报告](../../../artifacts/diagnostics/NTSD28-336B44-Q09-WORDS5-GROUP-MAP-001/REPORT.md)。
