# D-024 战斗实体直接 X/Z 写者复核（2026-10-04）

结论：`BOUNDED_STATIC_WRITER_AUDIT / NO_CONFIRMED_CURRENT_BYPASS`。在当前工作树的 `Assets/NTSD/Scripts/Animation/LF2Objects` 与 `Assets/NTSD/Scripts/Simulation`，`Runtime.X/Z =` 和 `+=/-=` 共检出 72 处文本匹配；本次沿下表既有生产调用链抽查直接位置写者和后续共享投影，没有逐个做运行时验证。没有观察到新的可裁决 336B44 同初态首差；这不是所有未来生成入口、所有对象或最终 Game View 已逐例通过的证明。

| 写者域 | 当前源码证据 | 裁决 |
| --- | --- | --- |
| 普通角色、武器及非角色帧运动 | `LF2Entity.ApplyNativeFrameMotionForWorldPass`、`LF2Weapon`、ECS 帧 pass 使用 `SimulationWorld.SpatialProjection` 或它的 `FixedViewRunDistanceScale`/`FixedViewRunVerticalDistanceScale` 兼容入口；源坐标初始化时按源增量投影。已有正式 336B44 根/原 Scene OID92 连续运动与平台受控搬运限定证据，见总表 D-024。 | 已证条件复用，不重跑全部实体。 |
| CPOINT/抓取/持有/投掷 | `BattleCpointWriter`、`BattleInteractionWriter`、`BattleHeldObjectWriter` 和 `LF2WeaponHeldStateResolver` 先算源规则 X/Z，再以同一世界投影写物理 X/Z。旧的原始 `±1`、原始 WPOINT 中间值在源坐标初始化的生产分支被后续投影覆盖。已有当前正式版 C040、C042 与 WPOINT/CPOINT 原 Scene 限定证据。 | 当前可比条件未见新比例首差；源坐标未初始化时的回退仍按可达性单独判断。 |
| kind8/传送/复活/融合 | 共用 `BattleKind8ControlRelationWriter` 将源 Z+1 投影；`LF2Entity` 传送将源 offset120/60 投影；`BattleRespawnModule` 随机偏移乘共享比例；`BattleOid5152RuntimeModule` 保留源域判距与独立物理中点。旧 `LF2CharacterHitResolver`/`LF2CharacterDatHitResolver` 的原始 Z+1 文本存在，但先前生产候选分发审计已证当前角色链先交给共用 kind8 writer，不能仅凭文本判生产旁路。 | 复用已有 kind8 近远原 Scene 各 260/260 与融合/复活聚焦证据；外部直调和未覆盖条件不升格为已证通过。 |
| 生成任务的源坐标 | 正常晚期 OPoint、随机武器、state18 粒子、武器碎片、state9996 克隆写者在父源坐标初始化时填写 `useSourceRulePosition`；`LF2Entity.ApplyInitialRuntimePosition` 按任务标志接收物理和源坐标。`LF2Entity` 的若干历史 hit_Fa8/13 子体任务仅显式写物理位置、未普遍填源标志。对正式 `resources/runtime/decoded_dat` 的 405 个 DAT 以 `hit_fa:\s*(8|13)(\s|$)` 检索结果为零。 | 该历史分支未证当前正式内容可达；不能据此改 DAT、加实体特判或宣称所有未来 DAT 都覆盖。 |

## 间接 OPoint 后继核查

正式 decoded DAT 中存在 `w/5.dat` 的 `facing: 40/50/60/70`、`s/5/5.dat` 的 `facing: 90` 等多发 OPoint，不能用“多发不在当前内容”排除。当前生产 `LF2ObjectPointFactory.ProcessOneLateOpoint` 与 logic-only `BattleLogicObjectPointRuntime.ProcessOneLateOpoint` 均把 `Facing > 10` 分成数量与朝向，在同一循环为每个子体配置单个 `OPointCreateTask`。两处 `ConfigureLateOpointPosition` 都按世界比例把原始相对 X/Z 转为物理出生坐标，并在父源坐标已初始化时写入 `useSourceRulePosition` 及源 X/Z；随后 `BattleNativeOpointBirthWriter.ApplySpread` 处理多发扩散。这是当前可达多发 OPoint 的生产路由。

另有 `OPointCreateMultipleTask` → `LF2ObjectPointFactory.MaterializeMultipleLogicObjects` / `BattleLogicObjectPointRuntime.MaterializeMultipleObjectsForStructuralWriter` 的旧批量路由。复制到单体任务时它没有源坐标载体，并令 `skipPostInitZOffset=false`，而 `BattleLogicEntityFactory.PrepareFinalRuntimePosition` 对这种任务把物理 Z 原样加 1。当前 `rg` 调用者仅有 `LF2ObjectPointModule.ProcessFrame`、`BattleRuntimeSelfCheck` 和 Editor 测试；在所检正式 `SimulationTickDriver`/生产结构写者调用链中，没有发现 `LF2ObjectPointModule.ProcessFrame` 的调用。故这个潜在比例旁路当前仅是**未证生产可达的旧入口**，不授权据它修改运行规则。这里的负搜索不证明任何未来或外部直接调用绝无可能。

这条批量路由的判断已在 [2026-09-26 出口对账](../../NTSD28-Q07-EXIT-RECONCILIATION-20260926/REPORT.md)记录；本轮新增的是以当前 336B44 正式 DAT 的真实多发条目复核生产 `Facing > 10` 分流，**不把旧发现重新计为一个待修任务**。同一旧报告曾把 N30 立即 OID998 生成为另一个待证候选；该结论已被后续 [N30 生产退休验收](../../NTSD28-Q07-N30-UNITY-DRIVER-WITNESS-001/PRODUCTION-RETIREMENT-ACCEPTANCE.md)覆盖。当前 `LF2Entity` 基类晚尾不调用旧 helper；`LF2Character` 虽仍经 `LF2CharacterLateRuntimeModule` 调用同名 N30 分支，但它把 `InputHistory[0..3]` 当作四个键码。当前生产 `NTSDEntityRuntime.PushInputHistory` 只滚动 `[1..5]`，`[0]` 是 `SetInputHistoryGate(bool)` 写入的 0/1；ECS `BattleCharacterInputStore` 同样只写 0/1。因此在所检正常生产输入历史中，该角色分支首项不可能等于触发条件 9，不能凭存在调用就判新生 OID998。外部灌入非法历史或未检快照边界不在此限定结论内；没有新首差，不清理该历史代码，也不重跑旧 N30 案例。

补查 `LF2Entity` 直接创建子任务的 `hit_Fa` 分支：当前正式 405 DAT 中 `hit_Fa:6/8/9/11/13` 均为 0 条；`hit_Fa:7` 有 28 条，分布在 8 份 DAT，其 `data/data.txt` 索引 OID 251/210/875/206/221/815/228/232 全部为 `type:3`，按当前 `RunCurrentDatFrameLogicBeforeAdvance` 的类型分流走非角色分支，不进入 `SpawnHitFa7Clone` 的缺源坐标任务。唯一 `hit_Fa:5` 在 `w/e.dat` 的 OID219（`type:3`），其生产 `RunHitFa5FrameLogic` 在双亲源位置完整时填写子任务源 X/Z 与整数载体；源位置不完整的回退仍须按实际出生条件判断。这是当前索引内容与已检调用链的可达性缩域，不能外推到外部注入定义或未来 DAT，也不把旧代码存在本身列为 P0。

本次静态审计仍未逐一运行 `OPointCreateTask` 的所有 producer，也没有覆盖源历史未初始化的回退、外部直调旧 resolver 或实际 Game View 比例。上述多发生产路由的源标志赋值是源码事实，不是所有父实体都已在运行时证实源坐标初始化。没有新增生产代码、DAT、图片、Scene 或测试；当前总表 D-024/Q07 保持限定证据复用、全实体总门开放。
