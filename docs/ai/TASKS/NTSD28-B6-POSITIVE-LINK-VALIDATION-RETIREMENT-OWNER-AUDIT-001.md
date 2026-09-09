# Task Contract — NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / UNITY_ONLY_MUTATING_PASS_CONFIRMED / POST_LIFECYCLE_RETIREMENT_PACKAGE_DEFINED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001 / VERIFIED`

## 目标

核对 Unity `HeldLinkValidation` / `BattleEcsPositiveLinkValidationPass` 是否对应当前 Authority 的独立
战斗 pass，冻结其实际写集、pass placement、SoA index、diagnostic/stress/test 消费者，并定义不掩盖当前
lifecycle 缺口的退休顺序。

只读 Authority 与 Unity；不修改 C#、Config、Scene、Prefab、资源、ProjectSettings 或 Authority。

## Authority 合同

- `SimulationTickDriver28::step()` 在 `advance_catch_relations()`、`settle_catch_relations()` 后直接进入可选
  stage-depth clamp，再执行第二次 held refill；此处没有正向 holder-link validation pass。
- kind2 pickup/catch/held writers 在各自事务内建立双方关系。
- `BattleWorld28::despawn()` 在 slot reset 前调用 `clear_entity_links(slot)`，升序扫描当前 active slots并清除所有
  指向即将移除 slot 的 held/catch 正反向引用。`spawn_at()` 在发布新 occupant 前也对目标 slot执行同一清理。
- 除具体 held/catch consumer 的规则分支外，Authority 不会仅因某一时点读取到 reciprocal mismatch 就单边改写
  holder relation。invalid relation 的诊断/保留边界已由 lifecycle 与 C09/C20 audits 单独拥有。

因此 Authority 的一致性 owner 是 relation transaction + structural lifecycle，而不是一个每 tick 修复 pass。

## Unity 当前写集与首差

生产 `NTSDBattleTickSystem.Tick()` 在 `PreInteraction` 后、`StageBounds` 和第二次 `HeldProcess` 前无条件执行
`HeldLinkValidation`。默认 `DataOriented` 与可配置 `Legacy` 语义相同：

1. 仅枚举 active 且 `holder.Runtime.LinkState > 0` 的 holder；
2. 以 `TargetSlotIndex` 找 active target，并要求 `target.Runtime.HolderStableId == holderSlot`；
3. missing 或 mismatch 时只写 `holder.Runtime.LinkState = 0` 并刷新 compat snapshot；
4. 保留 holder `TargetSlotIndex` / `HeldWeaponStableId`、target `LinkState` / `HolderStableId` 及对象引用；
5. 可额外发出 `positive-link-validation/link-validation` structural event。

这不是原子 unlink：它制造 `LinkState==0` 但其余正反向字段仍指向旧实体的半清理状态，并会改变后续同 tick 的
stage/held/input/AI 可观察结果。ShadowCompare只证明Legacy与DataOriented重复同一Unity规则，不能证明Authority等价。

## reachability 与依赖顺序

当前 Unity `SimulationRegistryModule.ReleaseRuntimeSlot()` 释放 occupant/generation/SoA row时不会扫描其他 active
实体的 held/catch references；slot可在同tick复用。故 missing/mismatch 在当前生产是可达的，但根因是已冻结的
`NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001`尚未实施，而不是本pass应继续拥有修复语义。

正确顺序必须是：

1. 先实现并取得 lifecycle cleanup focused + runtime 绿灯，证明 normal despawn/reuse在任何后续pass前已原子清理；
2. 再退休 Unity-only positive validation 的生产调度与半清理写入；
3. C09/C20 invalid-preserve更正仍按既有 package 独立实施，不能被本退休包偷带。

在第1步完成前直接移除本pass会暴露现有 stale/ABA gap，因此 production 保持 HELD。

## 唯一后继 production 包

`NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001` 在 lifecycle runtime绿灯后原子完成：

- 从正式 tick sequence、`BattleTickPhase` 描述与 actual-phase test中移除 `HeldLinkValidation`；
- 移除/停用 `ValidateHeldLinksAll()`、Legacy/DataOriented/ShadowCompare mode、mutating pass与其 world lifecycle；
- 移除 stress config/CLI/editor window/report 中的 mode、restore、run/mismatch/kept/cleared 指标；
- 取消 formal parity 的 `positive-link-validation/link-validation` event与W07旧 witness；将相应测试改为验证
  lifecycle cleanup 后无陈旧关系，而不是冻结单边清零；
- `positiveLinkWords`、count、find/handle API经全repo确认只供该pass枚举，可一并移除；
  `BattleRelationLinkStore` 的 relation/link/target AI projection、generation binding和property publication必须保留。

若为了开发保留一致性检查，它只能成为显式 opt-in、非生产 tick、只读且不写 runtime/trace 的 diagnostic；本包
不要求新增此工具。已有源文件是否物理删除受仓库删除审批规则约束，可在不删文件的情况下先完成生产退役。

## 验收矩阵

- actual phase sequence与Authority对应位置一致，tick不再产生 positive-link structural event；
- valid holder/child across tick完整保持；despawn child、despawn holder、slot0/high-slot与same-tick reuse由lifecycle
  package原子清理，下一次 held/AI consumer前无stale relation；
- synthetic mismatch在生产 tick中不被静默半清理；后续invalid-preserve package负责其诊断合同；
- relation/link AI projection、snapshot/checksum/restore与unified row不因移除positive bitmap而丢失；
- production stress配置和报告无死mode/死metric，warmed path零新增managed allocation；
- focused、phase-order、registry lifecycle、structural parity、stress、BattleRuntimeSelfCheck与真实Play通过。

## 不变量 / 回滚

- 不改 kind2/catch/held算法、content、capacity、RNG、stage、AI decision、shutdown顶层顺序或Authority。
- 本治理审计不授权绕过 lifecycle runtime gate，也不把当前可达的stale状态写成已修复。
- 回滚本轮只需移除治理记录；没有代码、content或Scene回滚。

