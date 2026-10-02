<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C051-EFFECT22-SELFCHECK-ORACLE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal 336B44 playable hit_response.cpp::HitResponseResolver28::accumulate_unarmored_horizontal and battle_world.cpp unarmored call path
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C051-EFFECT22-SELFCHECK-ORACLE-001.md
-->

# Q07/C051 SelfCheck effect22 无甲冲量预期

脚本修改前建立。原Editor完整SelfCheck第四轮已越过C042/C044旧口径，首差停在B5合成effect22 `KnockbackX=-8`断言，失败原件另存 `selfcheck-result-04.txt`。当前336B44正式普通无甲分支按攻击者朝向乘完整dvx；合成初态攻击者面右、state0、目标type0、`dvx=8`，故预期+8。Unity共用写者C051已按同规则实现；旧B1E13时期SelfCheck的-8来自当时的相对位置规则，不得用于新版裁决。

声明代码路径仅 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs::CheckStandardCharacterDamageAlignmentContracts`。计划把一条effect22断言X期望改为+8，原Y12、动作203、actual/shared同态继续验证；失败消息补快照。可能影响完整SelfCheck后续首差暴露，但不修改战斗生产结果。保护现有脏文件与四个Scene/配置SHA；不编辑DAT或非战斗功能，不清理临时/历史文件。菜单运行生成的Temp结果每轮覆盖前先另存，记录原件。

验收：生成工程编译0 error；原Editor实际SelfCheck越过此断言或记录下一首差；`Tools/Validate-ChangeLedger.ps1`、`git diff --check`和四保护SHA。状态按实际证据推进；完整SelfCheck与Q07不能凭单断言关闭。回滚仅精确反向调整本断言，不用 blanket restore/reset/clean。

2026-10-02 代码已写：只把 `CheckStandardCharacterDamageAlignmentContracts` effect22终值X从-8改为+8，并在同条失败消息附 actual/shared 快照；原Y12、动作203、两路径同态条件均保留。生产、DAT、Scene及其它测试未改。当前`CODE_WRITTEN`，编译与原Editor复验待执行。

2026-10-02 修后验收：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo '-clp:ErrorsOnly;Summary'` 退出0、0 error/281 warning。原Editor刷新后 `Assembly-CSharp.dll` 新于脚本，完整 SelfCheck 第五轮已越过 effect22 断言及后续 B5 方法，下一首差进入独立 C056 `CheckOid5152MergeSuccessAndDormantIsolation` 的旧合体锁存/计数断言，结果另存 `artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/selfcheck-result-05.txt`。菜单MCP调用30秒超时，但磁盘结果时间更新并明确FAIL；不称完整自检通过。本包限定`FOCUSED_TEST_PASS`；C051正式自然effect22与Q07仍开放。Temp结果为本轮生成物，覆盖前第四轮原件已另存 `selfcheck-result-04.txt`；未删除文件。

后继独立C056/C054旧断言更正后，第八轮原Editor完整SelfCheck磁盘结果`PASS`（`selfcheck-result-08.txt`，SHA-256 `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`）。本effect22合成口径`VERIFIED`，不覆盖C051自然可达/逐hit或Q07父出口。
