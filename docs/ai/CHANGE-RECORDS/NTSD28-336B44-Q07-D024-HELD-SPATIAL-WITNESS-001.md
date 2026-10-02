<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-HELD-SPATIAL-WITNESS-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C042NaturalThrowBattlePlayProbeEditor.cs
authority: current 336B44 formal battle_world.cpp CPOINT held position and user D-024 full-view ratio requirement
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-HELD-SPATIAL-WITNESS-001.md
-->

# Q07/D-024 自然抓取持有物理距离取证

本 Record 在修改脚本前建立。当前原 Battle Scene 的 C042 自然 OID75/action355→OID2/X650 报告只导出源规则 X/Z；正式源码和 Unity 在该链源坐标同态，但不能据此推断固定完整背景下的实际物理距离比例。`BattleCpointWriter.SyncHeldPosition` 当前物理 DAT 偏移不显式乘 `SpatialProjection`，形成待测候选，并非已证画面缺陷。

计划变更仅在现有 Editor 探针的 `EntitySample`、`Report` 和 `Capture` 加物理 X/Z、由现有世界 `SpatialProjection` 计算的期望 X/Z、比例数值；`TryStart` 只放行目标X650的新唯一runId并保留旧三种精确runId。预期副作用是一个新的唯一 JSON 诊断结果，不改变战斗 tick、生产写者、资源、Scene 或配置。不可回退边界：保护旧探针和旧 JSON 原件，禁止覆盖；所有现有工作区修改视为用户工作。

验收、风险和回滚以同 ID Task 为准。须先核唯一输出，再用原项目 Editor 编译和 Play 取证，并执行 ChangeLedger validator、`git diff --check`；只读诊断完成前不得把静态候选写为已修复或 Q07 完成。状态 `CODE_WRITTEN`，原Editor编译与Play待验。

2026-10-02 代码已写：只扩现有C042 Editor探针 EntitySample 的源初始化标志、实际物理X/Z、共用投影的期望X/Z；Report 记双轴比例；TryStart 只增 X650 唯一新runId，保留原三runId与拒绝覆盖门。生产写者、DAT、Scene/Asset、非战斗未改。待原Editor编译/Play/结果与保护SHA核验。

2026-10-02 原Scene验收：生成Editor工程0 error/251既有warning；原Editor MCP刷新后新唯一Play CAPTURED/DONE，60tick、tick2～60持有59tick；旧/新共有实体字段2880/2880一致。tick2正式源/Unity源整数X间距9，Unity实际物理X间距8.5，用户比例应为13.827457；Z源间距-1、物理-1.232877、比例目标-1.578082。原Editor退出idle/nonPlay、Scene clean，四保护资产和旧结果SHA5/5不变。诊断路径本身VERIFIED，生产比例首差另包待修；详 [报告](../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-HELD-SPATIAL-WITNESS-001/REPORT.md)。
