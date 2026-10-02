<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-CPOINT-HELD-PROJECTION-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C042NaturalThrowBattlePlayProbeEditor.cs
authority: current 336B44 formal battle_world.cpp CPOINT held position plus user D-024 full-view ratio requirement
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-CPOINT-HELD-PROJECTION-001.md
-->

# Q07/D-024 CPOINT 持有位置共用比例投影

最终检查：`Tools/Validate-ChangeLedger.ps1` 退出0并报告 `Change ledger validation PASSED`（存在既有未处于当前diff的路径提示，不是失败）；`git diff --check` 退出0。原Editor实际回到 idle/nonPlay/noncompiling，当前Battle Scene；五个保护文件重新算SHA仍5/5不变。自检旧Temp结果在触发请求前已保存，新结果亦已保存；删除/替换范围和原因详本包报告。

2026-10-02 验收更新（覆盖下方 `CODE_WRITTEN` 快照）：生成Editor工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q` 0 error；原Editor刷新并运行原Battle Scene 60tick，结果 `CAPTURED/DONE`、退出clean；同链红/绿共有源/战斗字段2880/2880相同。相对tick2～55才是54个持续持有tick，tick56～60投掷不混入：54tick的物理相对X/Z对整数源间距×世界比例，最大误差0.936610/0.232877输出像素。原Editor相邻位置EditMode聚焦1/1 PASS，完整BattleRuntimeSelfCheck PASS；5保护SHA稳。Temp中旧SelfCheck结果先复制到本包证据目录，现有请求入口清理该临时文件后写新PASS，均有同SHA，未删项目资产。`Tools/Validate-ChangeLedger.ps1`与`git diff --check`见最终检查记录。其它挂点写者另审，Q07/Q09和总目标仍开放。[详细报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-CPOINT-HELD-PROJECTION-001/REPORT.md)。

本 Record 在修改脚本前建立。原 Unity `BattleCpointWriter.SyncHeldPosition` 从抓取者物理整数位置直接加 DAT 中心/挂点 X/Z，而另行写出的源规则位置与336B44正式版相同。当前固定完整背景的 `BattleSpatialProjection` 水平比例为1.536384096、纵深1.578082192；原Scene自然红证书首个持有tick实际物理X间距8.5，对应正式源整数间距9的投影应13.827457，且Z也短于投影。旧探针只比源规则字段，未覆盖此差异；新读数与旧字段2880/2880同态。证据见 [诊断报告](../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-HELD-SPATIAL-WITNESS-001/REPORT.md)。

计划仅在此共用持有写者的源规则位置写入后，取抓取者物理整数位置为锚点，调用现有世界投影的 `SourceDeltaToViewX/Z` 转换被抓者相对源位移，再更新物理整数快照；源规则X/Z、Y、关系和生命周期不改。现有Editor探针仅新增绿色唯一runId白名单。正式源、DAT、PNG、Scene/Prefab、项目配置、非战斗模块不动。身份比例和未初始化源坐标时保留现有分支；其它挂点路径待单独验证，避免未经证据扩大修复。

预期副作用是持有期间被抓者物理X/Z及其碰撞/显示相对距离改变，这是用户批准的比例目标；可能改变项目舞台边缘的对象接触，故必须观察完整tick下源规则字段和对象关系。验收、风险与回滚详同ID Task。状态 `PLANNED`；绿色原Scene与聚焦测试前不得宣称修复完成。

2026-10-02 代码已写：BattleCpointWriter.SyncHeldPosition 在源规则X/Z写入后复用现有 BattleSpatialProjection.SourceDeltaToViewX/Z，以抓取者当前物理整数位置为锚点更新被抓者物理X/Z；仅在两端源规则坐标已初始化分支生效，身份/缺源旧分支保持。现有C042 Editor探针白名单仅增绿唯一 ee75-a355-x650-spatial-witness-02，红原件不覆盖。DAT、图片、Scene、配置、非战斗未改。生成编译、原Editor Play、聚焦及保护哈希待验。
