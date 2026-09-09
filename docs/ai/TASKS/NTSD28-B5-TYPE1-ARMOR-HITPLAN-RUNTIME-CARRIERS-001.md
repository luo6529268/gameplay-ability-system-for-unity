# Task Contract — NTSD28-B5-TYPE1-ARMOR-HITPLAN-RUNTIME-CARRIERS-001

> 状态：`VERIFIED / HITPLAN_CARRIERS_READY / ARMOR_TRANSACTION_UNCONNECTED`
> 依赖：`NTSD28-B5-DEFINITION-ATTACKING-CARRIER-001 / VERIFIED`

## 目标

把type-1 armor原子事务将写入的target runtime armor HP、input HP消费累计与input MP消费累计纳入
`BattleEcsHitExecutionPlan.WriterEffectSnapshot`的capture与actual/shadow comparison，确保后续DataOriented投影
若漏写、错写或多写这些副作用会fail closed。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorHitPlanRuntimeCarriersEditorTests.cs` 与 `.meta`
- 本Task/Change、Ledger、STATE、handoff、总表与armor manifest

## 不变量

- 只扩展writer-effect观察面，不接armor selection/match/activation/damage/break行为。
- null target保持`int.MinValue` sentinel；有target时读取逻辑`Runtime`真值，不读取Transform/表现层。
- 新字段进入`DifferenceMask`，任何一个字段不同都必须产生非零mask；不改变既有bit语义。
- 不修改content/Scene。

## 验收

test-first覆盖snapshot字段存在、capture精确值、null sentinel及逐字段difference；随后compile、focused、B5、
NTSD28 broad、SelfCheck、Console、Scene与Ledger。

## 完成证据

red `bad15cd869c44364b063b462192132a8` 4/4预期失败；focused
`e4a622735c0847abb8324c79c33067fe` 4/4、B5 `caf23965d9c14de0a6c46c465c5490f4`
343/343、NTSD28 broad `8a61484a2e6e4be793969b92be3dc2ac` 808/808均通过；SelfCheck PASS，
Console仅7条已知负路径日志，Scene基线与Ledger均通过。
