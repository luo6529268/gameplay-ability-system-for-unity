# Task Contract — NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / TWO_ACTUAL_BRANCHES_DEFINED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-AUDIT-001 / SUPERSEDED`

## 目标

纠正“只改`DropRandomly()`即可闭合kind-3”的owner遗漏，冻结Authority中DVX release后仍继续
执行kind-3 tail的顺序，以及Unity real/generic两条提前return路径。

## 已观察事实

- Authority `BattleWorld28::settle_held_refill_objects()`先按child type执行DVX release，然后以
  独立`if (parent_wpoint->kind == 3)`继续执行kind-3；前一分支不会return或抑制后一分支。
- type1/4/6在DVX阶段不draw，随后kind-3固定draw`6/7/4/5`共4次；type2在DVX阶段先draw
  `[0,6)`，随后仍drawkind-3四次，总计5次。kind-3最终action和motion覆盖DVX阶段写入。
- Unity real weapon分支在`weapon.Act()`返回`Thrown`时立即return；generic分支也在DVX
  `ThrowHeldObject()`后立即return。两者都会跳过后续`DropRandomly()`。
- release decoded corpus的2744条kind-3中有5条至少一轴authored dv非零；当前Direction-B
  Unity Config的12条kind-3全部dv为零。因此提前return并非当前内容首差，但属于正式规则闭合、
  owner audit既定authored test和未来H内容策略必须覆盖的路径。

## 修正后的 owner

| 责任 | owner | 结论 |
|---|---|---|
| DVX→kind-3 continuation | `BattleHeldObjectWriter.RunStep12()` | real与generic路径均须在DVX后继续kind-3；非kind-3的既有early return保持。 |
| kind-3 RNG/final writes | `BattleHeldObjectWriter.DropRandomly()` | 接收formal WPoint；无条件draw6/7/4/5，逐轴authored-nonzero否则random，Z random为整数-2..2。 |
| type2前置draw | 既有`LF2WeaponHeldStateResolver.Act()` / generic heavy branch | 保留DVX阶段的首个`[0,6)`draw，再执行kind-3四draw。 |
| release/link | 既有release helpers | DVX与kind-3重复清active relation是幂等的，不修改holder/owner identity政策。 |

## 生产包边界

后续`NTSD28-B6-WPOINT-KIND3-RELEASE-PRODUCTION-001`必须覆盖：

- `BattleHeldObjectWriter.RunStep12()`的real/generic DVX→kind-3 continuation；
- `DropRandomly()`的formal WPoint和最终四draw选择；
- focused test分别覆盖real/generic、dv全零、每轴非零、type1/4/6四draw与type2五draw；
- SelfCheck与C09/C20 Play probe同步。

不改`LF2WeaponHeldStateResolver`的DVX算法、C09/C20 placement、refill、cover pose、terminal、
content或Scene。当前已有多个B6包仍`RUNTIME_PENDING`，在Unity许可恢复或用户改变叠加策略前，
本production继续保持held，不写行为代码。

治理验证：`Tools/Validate-ChangeLedger.ps1`通过，362 records / 309 governed code files。

## 回滚

仅移除本纠正记录并恢复旧owner状态；没有脚本、内容或Scene回滚。

## Current corpus correction（2026-09-08）

current kind3由12纠正为holder-primary770，且OID7 Rock Lee action255已有一条authored DV overlap
`dvx100/dvy-1`。所以DVX→kind3 continuation已是current-content reachable，而非仅release/H未来路径。
RunStep12+DropRandomly双owner不变；production matrix必须加入该current witness。

## Held relation producer-domain correction（2026-09-08）

OPoint kind2把完整current held source union扩到41 definitions，kind3从pickup-only770修正为811；
authored-DV overlap仍唯一Rock Lee255。双owner与四draw合同不变，production matrix使用811。
