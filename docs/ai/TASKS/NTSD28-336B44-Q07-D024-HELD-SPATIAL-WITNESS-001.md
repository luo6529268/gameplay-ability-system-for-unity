# NTSD28-336B44-Q07-D024-HELD-SPATIAL-WITNESS-001

状态：`VERIFIED_DIAGNOSTIC_FIRST_DIFFERENCE / PRODUCTION_FIX_PENDING`。上级：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07 / D-024`。

依据：用户要求保留完整背景，并让角色、武器等实体位移与正式版画面比例一致，比例由统一入口持有；当前正式规则权威是根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式源码 `battle_world.cpp:6262-6294` 在 CPOINT 持有末尾以源坐标和 DAT 中心/挂点值写被抓者 X/Z。已验 OID75/action355、目标X650 自然链在 tick2 源X630/639、tick3持有。Unity `BattleCpointWriter.SyncHeldPosition` 同时写物理坐标与源规则坐标，但物理写入点直接用 DAT 偏移；此前 C042 Scene 探针只导出源规则坐标。

仅扩展现有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C042NaturalThrowBattlePlayProbeEditor.cs` 的诊断结果，每实体追加 `Runtime.X/Z` 及世界 `SpatialProjection.SourceToViewX/Z` 期望值，并记录水平/纵深比例；在 `TryStart` 仅放行目标 X650 的新唯一 `bee75-a355-x650-spatial-witness-01`，保留既有三种精确 runId。保留现有源字段、输入、seed、60 tick 和输出目录；使用全新唯一 runId，写前核对不存在旧结果。先取得原 Battle Scene 自然 Play 的物理/源坐标，计算持有期间双实体画面距离与正式源距离乘比值的差。只读诊断不修改生产写者、DAT、资源、场景、配置或非战斗逻辑。

验收：原 Editor 编译0错；唯一新 Play 结果 `CAPTURED`、自然抓取/持有、完整60tick、退出池借用0及 Scene clean；已存在的 C042 原件和四项保护文件 SHA 不变；按同 tick 的正式源码 CSV 对照源 X/Z 与新物理坐标，明确是否有首差。运行 `Tools/Validate-ChangeLedger.ps1` 和 `git diff --check`。若有物理首差，另建生产修复 Task/Change，不在此诊断包顺手改写者。

回滚：只逆向审阅并撤去本探针新增的诊断字段与赋值；保留已有未提交修改和新旧证据，不执行 blanket restore、reset、clean 或删除。

2026-10-02 结果：[报告](../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-HELD-SPATIAL-WITNESS-001/REPORT.md)。诊断已完成，生产修复单独立包。
