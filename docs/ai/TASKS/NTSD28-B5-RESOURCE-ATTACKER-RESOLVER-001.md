# Task Contract — NTSD28-B5-RESOURCE-ATTACKER-RESOLVER-001

> 状态：`VERIFIED / RESOLVER_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-RESOURCE-CARRIER-ATTRIBUTION-AUDIT-001 / VERIFIED`

## 目标

实现resource-attacker的authority-exact physical-slot两跳owner解析，不接完整resource transaction。

## Authority 合同

- 从active physical attacker slot开始。
- 最多追`OwnerSlotIndex`两跳。
- negative owner或self-owner正常终止并返回当前实体。
- 声明的next slot失活/越界时返回null，不保留上一跳fallback。
- 超过两跳不继续。

## 不变量

- 只使用`OwnerSlotIndex`和当前runtime slot；不得使用stable/holder/relation/spawner字段。
- 不分配、不写状态、不接production。
- 不改Config/DAT/Scene/Authority/资源/snapshot schema。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5ResourceAttackerResolverEditorTests.cs`

## 验收

test-first compile red；negative/self terminal、一跳/两跳、第三跳停止、first/second missing、字段隔离；
相关registry/B5、精确NTSD28 broad、SelfCheck、Scene/Console/Ledger。

## 回滚

移除resolver与focused test；production不受影响。

## 验证结论

- test-first fresh compile red：8个预期`CS0117`。
- fresh compile 0 error；focused `d95507dbe5784cdfa27741b45aea8ad2` 7/7。
- B5/hit/runtime-slot related `3a2305e3ce264e45900b76fccf58daa6` 302/302。
- 精确NTSD28 broad `c3b4c51aec5c42aaba4c6cd005bbfe75` 645/645。
- BattleRuntimeSelfCheck `2026-09-05T13:03:42Z` PASS；Scene unchanged。
