# Task Contract — NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / LIFECYCLE_REACHABILITY_CONFIRMED / ATOMIC_DESPAWN_LINK_CLEANUP_REQUIRED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-HELD-RECIPROCAL-FAILURE-AUDIT-001 / SUPERSEDED`

## 目标

闭合invalid negative reciprocal的正常lifecycle可达性，追踪Authority与Unity的slot释放前后
关系清理顺序，并确定不能局部修改C09/C20 invalid handler的原因。

## Authority lifecycle事实

`BattleWorld28::despawn(slot)`严格按以下顺序执行：

1. 校验slot active；
2. `clear_entity_links(slot)`扫描全部active entity；
3. 清理引用removed slot的held parent/child关系与catch target/source，并清相关catch timeout；
4. reset removed slot；
5. `clear_relation_column(slot)`清rest column。

因此正常despawn完成后，不会留下指向free slot的negative child；invalid reciprocal分支的
“诊断但不修改”是fail-closed保护，不是常规关系回收owner。

## Unity lifecycle事实

- `SimulationRegistryModule.ReleaseRuntimeSlot()`在`RuntimeSlots.Release()`前后只处理rest binding、
  generation-owned ECS rows、structural event与removed entity自身slot；没有扫描或清理其他active
  entity的`TargetSlotIndex/HolderStableId/CaughtSlotIndex/CatcherSlotIndex/CatchSourceSlot90`。
- ticking期间`UnregisterCore()`可立即release runtime slot并延后bucket removal；allocator/registration
  又会先处理pending destroy并可在同tick复用最低slot。
- 因而active holder被free而held child保留时，child会以negative relation进入后续C09/C20；
  现有invalid handler才将其LinkState清0。若slot先被复用且新occupant的target sentinel碰巧匹配，
  旧child甚至可能通过只按物理slot的reciprocal检查。
- Authority依赖“release前清引用”避免ABA；Unity关系字段同样是physical slot而非generation handle，
  所以不能靠generation store自动修复。

## 结论与正确顺序

`DYNAMIC_REACHABILITY_PENDING`已由静态生产调用链确认关闭；但actual不能只把invalid handler改成
preserve。正确依赖为：

1. `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001`：冻结Unity held/catch exact字段、
   split legacy mirrors与`CatchSourceSlot90`共享语义、registry/structural owner及ordered shutdown边界；
2. 原子despawn cleanup production：在slot release/reuse前清所有active反向引用并刷新相关
   ECS/snapshot mirror，随后清rest/generation；
3. invalid reciprocal production：正常lifecycle已无残留后，missing/mismatch分支改为诊断/preserve；
4. focused与structural trace覆盖free→same-tick reuse→C09→C20，证明无ABA、无额外RNG/行为写入。

## 验收与边界

- 必测held parent removed、held child removed、catcher removed、caught removed、slot0、extended high slot、
  same-tick reuse、pending destroy、immediate unregister、shutdown和幂等清理。
- 必须同时验证`RuntimeRestStore`column与ECS generation release顺序；不得破坏有序关闭合同。
- `CaughtSlotIndex/CatcherSlotIndex`与native`catch_target_8c/catch_source_90`并非可机械一一替换；
  exact mapping未完成前不写production。
- 本结论来自静态production closure，不冒充Unity Play复现；当前许可阻塞下保持held。

## 回滚

仅移除治理记录并恢复前一审计的reachability pending状态；没有代码、content或Scene回滚。
