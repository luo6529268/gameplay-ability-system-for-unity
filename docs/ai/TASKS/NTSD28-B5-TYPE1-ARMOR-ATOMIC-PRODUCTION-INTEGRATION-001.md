# Task Contract — NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-INTEGRATION-001

> 状态：`VERIFIED / PRODUCTION_CONNECTED / TYPE1_ARMOR_CORE_TRANSACTION_ALIGNED`
> 依赖：`NTSD28-B5-TYPE1-ARMOR-HITPLAN-RUNTIME-CARRIERS-001 / VERIFIED`

## 目标

按Authority固定顺序，把ordinary defense→恰好一个type1 armor→match→activation→selected reduced或
unarmored fallback→broken armor `-1→action→0`原子接入两条角色actual入口与HitPlan projection。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleOrdinaryCharacterDamageRouteResolver.cs` 与 `.meta`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorAtomicProductionIntegrationEditorTests.cs` 与 `.meta`
- 本Task/Change、Ledger、STATE、handoff、总表与armor manifest

## 不变量

- ordinary defense优先于armor；kind9仍不进入type1 armor普通kind0选择。
- 首armor不是type1按unarmored；首armor为type1时必须恰好一个，否则fail closed，不猜测多record策略。
- effective injury采用`stats.attacking`正值优先、active-mode正值fallback、low-32-bit signed乘法与截断除100；
  不得复用带四舍五入的hit-resource helper。
- bypass/candidate rejection/MP不足回unarmored；HP耗尽先写-1，unarmored horizontal后应用armor.action并归0。
- selected reduced写HP/PP、HP/MP消费累计、runtime armor HP delta、bdefend、reduced rest与既有tail；
  HP/MP消费与runtime armor字段必须由HitPlan同值投影。
- 通用hit-resource transfer、audio/spark精确差异仍按既有B5/B10/B9路由，不在本包扩大。
- 不修改content/Scene。

## 验收

test-first覆盖优先级、首record/恰好一个、match bypass、MP fallback、HP break、selected HP/MP armor writes、
actual入口与HitPlan projection；随后compile、focused、B5、NTSD28 broad、SelfCheck、Console、Scene与Ledger。

## 完成证据

red `7271e82dd0b348808b39b72acf3e0c48` 9/10预期失败；最终focused
`c587baa487d1403c8c5f1d323f85c83d` 10/10、B5 `a49b1c0fc993408aa1337f0788d2b908`
353/353、HitPlan `e2725b9e161c4890aa218f2459eb1c11` 184/184、NTSD28 broad
`08d7c6a1e13f4522b1f225b07e9b2632` 818/818均通过；SelfCheck PASS，Console仅7条已知负路径日志，
Scene基线、diff-check与Ledger均通过。
