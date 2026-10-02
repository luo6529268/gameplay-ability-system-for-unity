# Q07/D-024 canonical 非武器持有挂点比例见证

状态：`VERIFIED_SCOPED_CANONICAL_HELD_RATIO`。上级为 336B44 对齐总表 BATCH-04/Q07 和 `NTSD28-336B44-Q07-D024-WPOINT-HELD-PROJECTION-001`。规则权威是根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与对应 playable live source；用户 D-024 要求完整背景下战斗实体位移和挂点按统一比例投影，不允许修改 DAT 数值。

已证入口：正式 OID8 小樱在受控自然 frame256→257 于相对 tick6 生成 OID420/type3 并持有，tick28 与 OID7 李合体；当前源码、根 LFR、原 Battle Scene 的选定动作/关系链已分层证实。旧 C043 场景探针只导出源 X 和关系，未导出小樱及子体物理 X/Z 与源 Z，不能证明 canonical 非武器 WPOINT 比例。OID420 是 `LF2SpecialAttack`，不是 `LF2WeaponBase`；生产 `BattleHeldObjectWriter.RunStep12/SyncHeldFrameAndPosition` 负责持有位置。

范围：仅在 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C043FusionHeldBattlePlayProbeEditor.cs` 的 TickRow 追加只读小樱和子体源/物理 X/Z、源位置初始化标志，接受一个唯一新 runId；保持原 60tick 输入、生产 Driver、源/根对照、原 Scene、DAT、Input Actions、相机和非战斗模块不变。旧 runId/结果不可覆盖。此 Task 不改生产写者。

出口：原 Editor 编译0错；新唯一请求在干净 Battle Scene 完成同一 60tick，既有 C043 自然关系门继续 PASS；对真正持有且双方源坐标已初始化的 tick 逐项计算物理相对 X/Z 对源整数相对 X/Z 乘世界共用比例的误差，记录最大值及首差。对前 60tick 旧17字段重算当前正式源码 CSV，同条件不回退；保护 Battle/Menu/GameConfig/Mode Asset、旧场景 JSON 和源码 CSV 的 SHA，退出 clean、借用0。若实际源坐标未初始化或比例不对，保留红证据并另建最小修复包；不以理论式代替运行结果。运行 `Tools/Validate-ChangeLedger.ps1` 和 `git diff --check`。

回滚：只审阅并逆向移除本诊断脚本精确增量；保留旧请求、旧结果和新失败/成功证据，禁止 blanket Git restore/reset/clean 与删除用户文件。

实际结果：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-CANONICAL-HELD-SPATIAL-WITNESS-001/REPORT.md)记录原Battle Scene完整60tick NATURAL_GATE_PASS、21持有tick物理比例误差X0.915229/Z0.232877像素、旧22字段×60tick 1320/1320同、退出clean/借用0/六SHA稳。现有根正式EXE trace的指定初态/输入经Z载体差142归一后，小樱/子体存在性及源X/Z共246/246零差；该根报告仍`nativeParityClaim=false`，其它挂点/全World及WPOINT父出口待。
