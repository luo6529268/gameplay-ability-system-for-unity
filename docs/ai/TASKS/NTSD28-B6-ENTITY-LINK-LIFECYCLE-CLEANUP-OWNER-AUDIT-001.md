# Task Contract — NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / TWO_PRODUCTION_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001 / VERIFIED`

## 目标

冻结Authority `despawn()->clear_entity_links()`在Unity的exact/compat字段映射、structural owner、
失败原子性、ECS发布与ordered shutdown边界，并拆分lifecycle cleanup与invalid-handler纠正。

## 字段映射

| Authority | Unity exact/primary | Unity compatibility | 清理条件与结果 |
|---|---|---|---|
| `interaction_state` | `Runtime.LinkState` | 无 | 关联removed slot时写0。 |
| `linked_child_slot` | `Runtime.TargetSlotIndex` | `HeldWeaponStableId`/held reference | target等于removed slot时Target写0，Link写0；compat写-1并清throw guard/reference。 |
| `linked_parent_slot` | `Runtime.HolderStableId` | `HolderCopySlot`不是同字段 | Link非0且holder等于removed slot时Holder写0、Link写0；不得清HolderCopy。 |
| `catch_target_slot_8c` | `Runtime.CaughtSlotIndex` | `Catching` object reference | 等于removed slot时写-1并清`CaughtDuration`/reference。 |
| `catch_source_slot_90` | `Runtime.CatchSourceSlot90` | `Runtime.CatcherSlotIndex`为当前B6 legacy mirror | exact按`>=0x2000 ? value-0x2000 : value`解码后匹配；exact写-1、duration写0；plain legacy匹配时同步-1。 |
| `catch_timeout_94` | `Runtime.CaughtDuration` | `CatchTimer`无关 | 任一catch target/source命中removed slot时写0。 |

`Kind4SourceCount92`是standalone模型为native相邻/共享语义拆出的独立字段；Authority
`clear_entity_links()`只写`catch_source_slot_90=-1`，本cleanup不得清Kind4 count。

## Production owner

- 新建无状态/无分配的`BattleEntityLinkLifecycleWriter`（最终名称可跟随相邻风格），由
  `SimulationRegistryModule.ReleaseRuntimeSlot()`唯一调用。
- 先完成现有slot/rest/current-occupant preflight；`RuntimeSlots.Release()`成功后，在函数返回、
  generation row release、任何allocator/registration可见点之前同步扫描active slots并清反向引用。
  这是Unity为避免release失败后回滚复杂关系事务的等价适配；方法内无interleave，对外仍原子。
- `LinkState/TargetSlotIndex`必须通过现有property setter发布到`BattleRelationLinkStore`和unified row；
  其他live runtime字段由后续snapshot读取。仅在实际改变的entity上刷新必要compat snapshot。
- removed entity自身随后按现有顺序释放Identity/Input/Frame/Relation/Vital generation rows与rest绑定；
  不新增全局singleton或Unity对象。

## 两个 production 包

1. `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001`：接入上述原子cleanup，保留
   当前C09/C20 invalid handler作为防御性fallback；focused证明正常free/reuse不再产生invalid child。
2. `NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001`：在前包runtime绿灯后，将
   missing/mismatch分支从clear改成Authority diagnostic/preserve，并加失败计数/trace而不改关系。

## 验收矩阵

- removed held child/parent、catcher/caught、encoded `0x2000+slot` source、slot0、extended high slot；
- immediate unregister、ticking deferred unregister、pending destroy、same-tick lowest-slot reuse；
- `Kind4SourceCount92`、HolderCopy、owner/credit、unrelated Spawner、RNG、action/motion均保持；
- relation SoA/unified row、snapshot/checksum、rest row/column与generation handle无陈旧值；
- 4096 warmed cleanup零managed allocation；ordered shutdown phase8 world cleanup幂等，不在Stopping
  自动创建服务或发布presentation。

## 不变量 / 阻塞

- 不在本包迁移catch producer/settlement算法；`CatcherSlotIndex`仍只是待后续B6替换的compat mirror。
- 不改kind3、DVX、refill、capacity、content、Scene或顶层shutdown顺序。
- 当前多个B6 production仍无Unity runtime绿灯；两包保持held，本轮不修改脚本。
- 后继`NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001`并未改变本cleanup的
  当前边界：在HolderCopy behavior/carrier尚未退休时仍不得由despawn局部清它；它将在所有B5/B6/B7
  consumers/producers迁移后由联合carrier disposition整体删除。

## 回滚

仅移除治理记录；没有代码、content或Scene回滚。
