# Task Contract — NTSD28-B5-STANDARD-HIT-REST-PURE-RESOLVER-001

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001 / VERIFIED`

## 目标

实现allocation-free pure resolver，精确投影Authority standard-hit rest事务的attacker/target hold、arest与可选
vrest写入，覆盖recover/definition-effect/timing-reduction/uint8语义；本包不接actual或HitPlan。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleStandardHitRestResolver.cs`
- 对应Unity `.meta`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestPureResolverEditorTests.cs`
- 对应Unity `.meta`
- 本Task/Change、Ledger、STATE、handoff、总表与rest manifest

## 不变量

- resolver无Unity对象/World写入、无分配、无RNG；所有输入输出显式。
- recover1只抑制attacker hold、2只抑制target hold、3抑制双方；definition effect2抑制双方，3仅attacker，
  4仅target，其他不抑制。
- reduction限制0..5；hold使用3基值，arest保持`arest<4 && vrest==0 =>4`特例，vrest先转uint8再reduction。
- vrest原值<=0不写；原值>0即使byte wrap为0也必须显式写0。
- 不修改任何现有命中行为、selection UI、content或Scene。

## 验收与回滚

test-first穷举recover/effect/reduction、arest边界、vrest 0/1/2/255/256/257/258与warm 4096 zero-allocation；
之后compile、focused、B5、exact broad、SelfCheck、Console、Scene、Ledger。

回滚删除resolver/test文件即可，不影响载体或现有行为。

## 验收结果

- test-first red：job `4a1a81f590e947e29e46a08f085fc174`，20/20按预期失败（resolver尚不存在）。
- focused green：job `74ae79b7f7244410ba1f816a4a079f03`，21/21通过，含warm 4096 zero-allocation。
- B5 + HitPlan：job `f39a6241b4f849b28b85c2157fcaedee`，404/404通过。
- exact NTSD28 broad：job `c26607abe78c40fe9f45a59a3b1b7ad2`，779/779通过。
- `BattleRuntimeSelfCheck`：`2026-09-05T21:32:09Z` PASS；filtered Console仅7条预期注册/rest-binding日志，无编译错误。
- Scene保持SHA-256 `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`、长度205625不变；Change Ledger validator通过（278 records / 239 governed code files）。

本包只建立pure truth table；actual/HitPlan生产接线由下一Change ID实施。
