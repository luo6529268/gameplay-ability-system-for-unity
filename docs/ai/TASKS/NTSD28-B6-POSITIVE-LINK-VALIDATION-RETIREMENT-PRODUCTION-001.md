# Task Contract — NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001

> 状态：`VERIFIED / RED_4_FAIL_2_PASS_OF_6 / FOCUSED_6_OF_6 / RELATED_33_OF_33 / C09_RELATED_9_OF_9 / B6_CATEGORY_103_OF_103 / NTSD28_229_OF_229 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC / CONSOLE_0_ERROR / SCENE_UNCHANGED`
> 依赖：
> - `NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-OWNER-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001 / VERIFIED`
> - `NTSD28-B6-HELD-INJURY-CAUGHTACT-EVENT-PRODUCTION-001 / VERIFIED`

## 目标

退休 Unity-only `HeldLinkValidation` 生产阶段及其单边 `LinkState=0` 修复语义，使 post-catch
顺序从完整 settlement 直接进入 stage-depth/第二次 held pass；正式关系一致性只由 relation transaction
和已验证的 structural lifecycle cleanup 负责。

## Authority 合同

- `SimulationTickDriver28::step()` 在 `advance_catch_relations()`、`settle_catch_relations()`及
  caughtact producer 后直接进入 stage depth clamp；没有正 relation validation pass。
- despawn/spawn-at 在 occupant release/reuse前由`clear_entity_links(slot)`原子清理所有正反向引用；
  Unity lifecycle package已取得slot0/high/deferred/reuse的runtime证据。
- synthetic positive reciprocal mismatch在正式tick不能被额外pass静默半清理；具体catch/held consumer
  与C09/C20 negative invalid-preserve合同不变。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleProductionOwnershipInventory.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsPositiveLinkValidationPass.cs`
- `Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs`
- `Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressWindow.cs`
- `Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleEcsPositiveLinkValidationPassEditorTests.cs`
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28B6PositiveLinkValidationRetirementProductionEditorTests.cs`
  及其`.meta`
- `Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleParityStructuralWitnessEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleParityTraceEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleU6ProductionOwnershipEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/ProductionEntityStressCapacityPressureEditorTests.cs`
- 本Change治理文档与恢复入口。

## 实施边界与风险

- 从`BattleTickPhase`及正式`RunInteractionPhase`移除阶段，随后阶段数值连续前移；phase consumers/tests
  必须同步，不得留下数组空洞。
- `SimulationWorld`不再构造/reset/配置/restore该pass；保留`ValidateHeldLinksAll()`作为明确obsolete、
  allocation-free、无写入/无event的兼容入口，防止历史工具调用重新激活规则。
- 旧pass源文件不物理删除；其`Execute()`必须被强制no-op。专用bitmap的物理清理可在确认所有consumer
  为零后同包完成；若保留，也只能是无可观察副作用的dead storage，不能构成生产owner。
- stress request/config/report/menu移除mode及run/visit/kept/cleared/mismatch指标；U6 inventory canonical
  owner数从9降为8，不能把退休pass继续统计为canonical工作量。
- W07改验证lifecycle cleanup与不存在positive event；negative held `Action=link-validation`诊断及其
  parity字段必须保留。

## 不变量

- 不改 relation producer、caughtact、held/weapon、negative invalid-preserve、AI relation projection、
  snapshot/checksum/restore、stage、RNG、content、Scene或shutdown。
- 不删除源文件，不修改用户Scene/Prefab/Config/资源。
- lifecycle release/reuse必须在下一held/AI consumer前保持原子清理。
- warmed retired entry与正式empty-world tick不得新增managed allocation。

## Test-first 验收

1. RED冻结当前phase/world/stress/U6 surface仍存在、显式validation仍单边清零并发event。
   已由 Unity EditMode job `8bebbd2c698540a68ab0748e1d2bc635` 取得
   `4 fail / 2 pass / 6 total`。
2. GREEN覆盖phase移除、synthetic mismatch preserve、无positive event、lifecycle atomic cleanup、
   no stress/U6 dead surface及4096次0B。
3. 运行actual phase sequence、lifecycle、query/link、structural parity、stress/U6、B6、NTSD28、
   compile、SelfCheck、真实Battle grab Play、Console、Scene和Ledger。

## 完成证据

- 新focused从RED `4 fail / 2 pass`转为GREEN `6/6`；related `33/33`、B6 `103/103`、
  NTSD28 `229/229`，两套build均0 error。
- 真实Battle grab Play为PASS，post-catch只剩三条正式步骤，synthetic positive mismatch完整保留，
  lifecycle cleanup仍为唯一原子清理owner；Console 0 error且Scene字节身份不变。
- full SelfCheck已越过本包旧断言，停在独立R2 catch exact-field旧夹具；不影响本包验证状态，
  但不得据此宣称full SelfCheck或全战斗对齐完成。
- post-verify发现并修正一处C09 phase occurrence旧索引，final相关job
  `edb6022909c5414499e9a5c2364c4c76`为`9/9`。

## 回滚

只恢复本包phase/world/stress/U6/测试接线；不得回退lifecycle cleanup、negative invalid preserve、
caughtact或其他前置B6包。
