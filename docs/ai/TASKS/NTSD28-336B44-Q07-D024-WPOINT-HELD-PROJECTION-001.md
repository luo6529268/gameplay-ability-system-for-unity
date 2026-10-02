# NTSD28-336B44-Q07-D024-WPOINT-HELD-PROJECTION-001

状态：`RUNTIME_PENDING / FOCUSED_WEAPON_PASS / WEAPON_AND_CANONICAL_SCENE_RATIO_SCOPED_PASS_ROOT_POSE_PENDING`。上级：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07 / D-024`。当前证据与未闭出口见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-HELD-PROJECTION-001/REPORT.md)。

授权/权威：用户明确要求保留固定完整背景相机，并将角色、武器及其它战斗实体的实际位移改成与正式视口**比例**一致，且所有比例由已有共用入口管理；DAT 数值不得改。正式 EXE 身份为 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；当前对应 playable `battle_world.cpp::settle_held_refill_objects` 以持有者和双方 WPOINT 求源规则整数挂点。当前源码在同正式 runtime 的OID2自然拾取OID120/24tick重编诊断新CSV与旧CSV逐SHA相同，原Editor当前DAT同链1/1仍暴露物理比例首差，见[只读审计](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-SPATIAL-AUDIT-20261002/REPORT.md)。

精确脚本范围：`Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs::SyncHeldFrameAndPosition`（非LF2WeaponBase持有及直接Pose）；`Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs::ApplyHeldWPointSync`（LF2WeaponBase运行时持有）；`Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HeldWeaponDualDomainEditorTests.cs::NaturalPickupMovingHeldWeapon_KeepsRuleAndScaledViewDomains/ReadNativeRows`（把旧raw物理偏移断言换为世界投影比例目标，并读取当前336B44源码重核CSV）。只在源规则持有坐标已完成且两端源坐标初始化时，用现有 `SimulationWorld.SpatialProjection.SourceDeltaToViewX/Z` 投影**双方源整数相对位移**，以持有者当前物理整数坐标为锚点更新持有物理 X/Z；缺源和身份路径保留。不得新建第二套比例常量或特判OID/角色。

不变量：源规则坐标、动作、输入、Y、cover 排序关系、33ms/3ms、对象生命周期、DAT/PNG、Scene/Prefab、项目相机/地图、非战斗逻辑不变。物理X/Z偏移变化是D-024授权的预期效果；可能触及武器攻击/拾取接触和画面挂点，须观察后续完整tick源字段首差。已有CPOINT持有修复保持。

验收：先更正现有具名测试为当前正式源码CSV与物理比例断言，原Editor定向RED且原因只为WPOINT物理间距；再两个共用持有写者最小GREEN，生成Editor工程和原Editor编译0错；当前正式内容同初态OID2/120完整Driver24tick源规则全字段与当前336B44源码CSV相同，持有物理X/Z相对距离对源整数间距×世界比例误差小于1输出像素；完整SelfCheck和最近武器持有/Scene Play按影响运行，保护Battle/Menu/GameConfig/Mode Asset与旧证据SHA不变，退出clean；`Tools/Validate-ChangeLedger.ps1`与`git diff --check`通过。若原Scene已存在合适的OID120自然持有入口，优先复用，不要求全部角色/全部场景。

回滚：只审阅上述三处脚本的精确逆向差量并保留红绿证据；任何已有工作区文件、DAT、Scene、旧CSV及测试结果都不得以blanket Git restore/reset/clean、覆盖或删除处理。旧测试和证据的历史事实保留，以本包版本化更正其当前目标断言。

2026-10-02后续更正：原Battle Scene武器OID120的物理键拾取门已通过独立相位配对诊断，18持有tick比例<1px；OID8→OID420/type3 canonical非武器自然链21持有tick比例X/Z<1px，旧字段1320/1320同。两者均为Scene限定出口；正式OID420指定LFR源坐标已与根EXE按Z载体差归一246/246同；其它挂点、完整World及画面未闭，本父包仍RUNTIME_PENDING。见canonical同ID报告。
