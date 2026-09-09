# Task Contract — NTSD28-B5-RESOURCE-TRANSACTION-PURE-CORE-001

> 状态：`VERIFIED / PURE_TRANSACTION_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-RESOURCE-INJURY-PURE-CORE-001 / VERIFIED`

## 目标

实现hit-resource的local gate、type0 injury-MP reward、target drain与resource-attacker gain纯事务，
由参数提供尚未落地的rules/suppression/baseMax，不接production。

## Authority 合同

- local disabled：整个MP事务no-op。
- positive resource injury、suppression!=1且双方type0时，先按attacker/target百分比奖励。
- target drain：type0、positive、全额可支付才扣，并累加target consumed total。
- resource-attacker gain：type0；negative为全额可支付成本并累加consumed，nonnegative仅在
  `current + gain <= baseMax`时增加。
- 固定顺序：injury rewards → drain → gain。

## 不变量

- 无RNG/分配/外部查询；不猜测mode rules、suppression carrier、baseMax binding或attribution。
- 不接production，不改Config/DAT/Scene/Authority/资源/snapshot schema。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5ResourceTransactionPureCoreEditorTests.cs`

## 验收

test-first；local gate、reward type/suppression、reward-before-cost顺序、drain/gain all-or-nothing、
positive max gate；相关B5、NTSD28 broad、SelfCheck、Scene/Console/Ledger。

## 回滚

移除pure transaction helper与focused test；production不受影响。

## 验证结论

- test-first fresh compile red：6个预期`CS0117`。
- fresh compile 0 error；focused `229178ebf8614558a98875c2944e5462` 7/7。
- B5/hit/resource related `02c45715780a4977bf3fcf6af2c91637` 256/256。
- 精确NTSD28 broad `0b0291f30d524c47a3bb1fc77c31cba1` 632/632。
- BattleRuntimeSelfCheck `2026-09-05T12:26:10Z` PASS；Scene unchanged。
- production仍等待rules/suppression/baseMax/world gate/attribution carrier，不伪接默认值。
