# Task Contract — NTSD28-B5-KIND0-DIRECT-POST-EFFECT-ACTION-001

> 状态：`VERIFIED / KIND0_DIRECT_POST_EFFECT_ACTION_ALIGNED`
> 依赖：`NTSD28-B5-EFFECT-ACTION-OVERRIDE-001 / VERIFIED`

## 目标

实现unarmored type0的direct post-effect action：effect3/30→200，effect2/21/22及合格effect20→203，
使用previous-action state gate，并在203按最终pending X impulse写朝向；顺序必须位于effect8..16 override之后。

## 路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind0DirectPostEffectActionEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

## 不变量

- 仅kind0、unarmored、type0成功结算；kind9、weapon/type3/other、alternate/reduced均不进入。
- 读`Frame.Prev` state；3/30在state13抑制，20在state18抑制。
- action写入清AttackingCounter；203 facing：pending X<0→right/0，>=0→left/1。
- 不实现audio/spark/armor/effect5000/6000，不修改content或Scene。

## 验收与回滚

test-first覆盖effect/gate/order/facing/excluded branches与hit-plan；compile/focused/related/broad/SelfCheck/Ledger。
回滚移除helper/actual+projection调用和fixtures，不回退effect8..16 override。

Test-first `f54a21e4c1b947f5a13d6cc0e1ee56f4`执行16项，10项按预期失败、6项排除/gate
先验通过；失败覆盖200/203 action、三组final-X facing及hit-plan目标action。

最终：compile0；focused `e68ea7054e044b24b59f7089eb718a90` 16/16；hit-plan+B5 19类
`eb7d15dec17b46ba9a104ee0fa0ab6f6` 336/336；98类精确NTSD28 clean broad
`b92c041687be4bf7acc265ee39a84136` 713/713；SelfCheck在修正两组旧预期后
2026-09-05T16:25:18Z PASS；Console0；Ledger264/231 PASS；Scene保持并发基线D4266C6D不变。
