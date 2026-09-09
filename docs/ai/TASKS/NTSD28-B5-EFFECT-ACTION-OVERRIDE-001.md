# Task Contract — NTSD28-B5-EFFECT-ACTION-OVERRIDE-001

> 状态：`VERIFIED / EFFECT8_16_ACTION_OVERRIDE_ALIGNED`
> 依赖：`NTSD28-B5-EFFECT-ELIGIBILITY-POSTACTION-AUDIT-001 / VERIFIED`

## 目标

实现NTSD 2.8-Logan unarmored ordinary hit的effect8..16 action override：完整target-type矩阵、
action-latch首BDY/state/property抑制、pickedact previous-state gate、positive catchingact/caughtact写入及
target post-damage positive-HP保护，并同步actual character/weapon/type3-other与hit-plan projection。

## 路径

- `Assets/NTSD/Scripts/Animation/LF2CharacterData.cs`
- `Assets/NTSD/Scripts/Animation/LF2FrameData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5EffectActionOverrideEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

## 不变量

- 只在unarmored kind0成功结算后执行；kind9/selected-armor/guarded/rejected candidate不得继承。
- effect8..16只按正式对象类型矩阵接受；当前corpus只有effect8可达也不能硬编码成角色特例。
- action override位于现有type3 post-hit continuation之后；后续direct post-effect另包覆盖target action。
- action写入不得清`AttackingCounter`，只改当前action/frame data；caughtact不得覆盖死亡反应。
- `property`只补definition schema/parser carrier，不覆盖Direction B Config内容。
- 不处理direct post-effect、armor、audio、spark、effect5000/6000或Scene。

## 验收

test-first覆盖矩阵、四类抑制、pickedact、attacker-only、positive/dead target、actual三类owner和hit-plan；
编译、focused、hit-plan、B5、精确NTSD28 broad、SelfCheck、Console/Ledger通过。

## 回滚

移除property carrier/helper/三类writer与projection调用及fixtures；不回退前序damage scale/kind16变更。

Test-first `d45af0c4ea924ef8a51f2401e49833eb`执行23项，16项按预期失败、7项保护分支先验通过；
失败覆盖property缺失、effect target矩阵、caught/catching/picked、death-attacker action与hit-plan目标action。
审计测试夹具时确认既有formal BDY value按旧合同只保留X/Y/W/H；本行为仍需要首BDY kind，故新增独立
`LF2FrameData.PrimaryBodyKindForEffectSuppression`载体，不修改/扩张formal geometry value。

最终证据：编译0 error；focused `6e911e42281d448d930c38240893a590` 24/24；hit-plan+B5
18类 `62387abe2aaa41b7b1f70414f3f5302d` 320/320；精确NTSD28 97类
`250000c7231e48bd9780cc3313a21668` 698/698；2026-09-05T16:01:21Z SelfCheck PASS；
Console0；Ledger 263/230 PASS；diff-check仅CRLF警告。Scene继续有并发用户/Editor写入，本包未写、未回退。
