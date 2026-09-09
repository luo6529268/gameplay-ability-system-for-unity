# Task Contract — NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001

> 状态：`VERIFIED / FOCUSED_7_OF_7 / B6_CATEGORY_15_OF_15 / RELATED_91_OF_91 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / ATOMIC_RELEASE_CLEANUP / RED_NOT_EXECUTED`
> 依赖：
> - `NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001 / VERIFIED`

## 目标

在`SimulationRegistryModule.ReleaseRuntimeSlot()`成功释放目标slot之后、generation rows清理和任何
allocator/registration复用可见之前，原子扫描active实体并清除指向removed physical slot的held/catch
exact与compat关系，阻断free→same-tick reuse ABA。

## Authority 与映射

- Authority：SHA-256 `EB37E8EC1186BF0349A3B3B5759666C3A8F44F0E1640C159E06F805189FBCCB1`
  的playable `battle_world.cpp`中`despawn()->clear_entity_links()`。
- `LinkState/TargetSlotIndex`、`HolderStableId`、`CaughtSlotIndex`、`CatchSourceSlot90`、
  `CatcherSlotIndex`、`CaughtDuration`按owner audit映射；encoded CatchSource只为匹配removed slot解码
  `0x2000 + slot`。
- `HolderCopySlot`、`Kind4SourceCount92`、Owner/credit、Spawner、action/motion/RNG均不属于本cleanup。

## 允许修改

- 新`Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleEntityLinkLifecycleWriter.cs`。
- `Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs`的成功release边界。
- 新focused Editor test；必要时只更新`BattleRuntimeSelfCheck`中由本生命周期差异造成的旧断言。
- 本Change治理文档。

## 不变量

- 先完成现有occupant/rest/current-slot preflight；`RuntimeSlots.Release()`失败时零关系副作用。
- cleanup在成功release后、Identity/Input/Frame/Relation/Vital generation release与slot reuse前完成；
  对外保持同一同步事务。
- `TargetSlotIndex`/`LinkState`通过现有property publication维护relation SoA/unified row；只刷新实际变更实体。
- 保留C09/C20 invalid handler、positive-link validation与compat mirrors；它们分别由后继包退休/纠正。
- 不创建singleton、GameObject或presentation publication；Stopping/ordered shutdown路径幂等。
- 不改Scene、Prefab、Config、ProjectSettings、Input Actions、network或Authority。

## Test-first 验收

1. RED证明removed held child/parent、catcher/caught、encoded source和same-slot reuse会留下stale关系。
2. 覆盖slot0/extended high、immediate unregister、deferred/pending destroy、same-tick reuse与release失败零副作用。
3. 验证exact/compat字段、references、throw guard/duration及relation row被清；HolderCopy、Kind4、owner、
   Spawner、action/motion/RNG sentinel保持。
4. 4096 warmed cleanup不产生managed allocation；compile、focused、registry/B6相关、SelfCheck、targeted Play、
   Console、Scene和Ledger按实际证据记录。

## 回滚

移除新writer和调用点，删除本focused test并恢复本Change修改的SelfCheck fixture。无schema/content/Scene
迁移；回滚会重新暴露stale physical-slot关系与same-tick ABA。

## 当前结果（2026-09-09）

- 成功release后的同步cleanup、relation publication、compat reference清理和same-slot ABA阻断已写入生产。
- focused `7/7`、B6 category `15/15`、七类相邻回归`91/91`、真实Play `7` cases通过；4096-slot
  warmed cleanup为0 managed bytes。
- Runtime/Editor build均0 error（既有warning 47/129），最终Play后Console 0 error。
- full SelfCheck已纠正并跨过本Change直接触发的旧P7 rebind夹具，下一首差为独立held injury accounting
  (`BattleRuntimeSelfCheck.cs:11403`)。
- test-first fixture先于production落盘，但实际RED因当时的Unity工具限制未执行；状态如实保留
  `RED_NOT_EXECUTED`。
- Scene验证副作用已精确恢复并重新加载；最终SHA-256为
  `89AA621640A3818F2C7CD307836C50D72CAB84B6D4F2B6DB333FF7CBF525A673`，Unity `isDirty=false`。
