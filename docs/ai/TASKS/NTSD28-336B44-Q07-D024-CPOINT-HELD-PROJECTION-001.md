# NTSD28-336B44-Q07-D024-CPOINT-HELD-PROJECTION-001

状态：`VERIFIED_SCOPED_CPOINT_HELD_PROJECTION`。上级：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07 / D-024`。选定自然链和共用CPOINT写者验收见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-CPOINT-HELD-PROJECTION-001/REPORT.md)；其它挂点及Q07仍开放。

授权与依据：用户要求在保留完整背景相机的前提下，让角色、武器等战斗实体位移按正式视口比例一致，并由统一入口管理比例，不修改 DAT。当前规则权威为根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 和 playable live source。正式 `battle_world.cpp:6262-6294` 的 CPOINT 持有以源规则整数 X/Z 与 DAT 中心/挂点计算。独立 [自然Play首差](../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-HELD-SPATIAL-WITNESS-001/REPORT.md) 已证 OID75/action355→OID2/X650 的59持有tick，首tick源X间距9、实际Unity物理间距8.5、按水平比例应13.827457；源Z间距-1、实际-1.232877、按纵深比例应-1.578082。旧/新源字段2880/2880一致。

范围：仅改 `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs::SyncHeldPosition`，在正式源规则位置写成后，用同一 `SimulationWorld.SpatialProjection.SourceDeltaToViewX/Z` 把持有相对位移投影到物理坐标；以抓取者当前物理整数位置为共享锚点，保留已有Y与源规则字段写入。仅扩现有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C042NaturalThrowBattlePlayProbeEditor.cs::TryStart` 的新唯一绿色 runId `bee75-a355-x650-spatial-witness-02`，保留旧请求白名单/防覆盖门。诊断脚本其余字段和流程不动。其它持有/武器挂点写者本包只读标记后继，不据本单例宣称它们已修。

不变量：33ms/3ms逻辑时间、输入、正式源规则 X/Z、DAT 数值、Y、对象关系/生命周期、Scene 与非战斗逻辑不变；身份比例下应维持原整数定位语义；真实完整视口中物理相对距离由共用投影导出。固定相机、项目地图与已批准例外保持。

验收：先保留红色首差原件；生成Editor工程0 error、原Editor脚本刷新0 error；原 Battle Scene 以同seed/初态/输入新唯一RunId运行60tick，源旧/绿共有实体字段2880/2880，抓取者/被抓者X/Z物理间距按源整数间距×世界比例在整数像素量化范围内，退出池0、Scene clean、旧红原件与四保护文件SHA不变；跑最近的CPOINT/持有聚焦测试与完整SelfCheck，运行 `Tools/Validate-ChangeLedger.ps1`、`git diff --check`。若新位置影响后续战斗字段，记录首差并调整共用投影语义，不能忽略。

回滚：只审阅本单一写者和探针新增runId的精确逆向差量；不覆盖、移动、删除已有工作区文件或证据，不用 blanket Git restore/reset/clean。
