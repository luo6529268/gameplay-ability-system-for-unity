# Task Contract — NTSD28-B5-LEGACY-KIND16-PRODUCTION-RETIREMENT-001

> 状态：`VERIFIED / KIND16_UNSUPPORTED / KIND15_EFFECT16_PRESERVED`
> 依赖：`NTSD28-B5-REMAINING-FALLDAMAGEDIV-CONSUMER-AUDIT-001 / VERIFIED`

## 目标

退役Unity旧`itr.kind=16`角色伤害/扣蓝帧与物体旋风生产路由，使candidate disposition、hit-plan
projection、actual/shared character及generic weapon dispatch统一为unsupported/no mutation。

## Authority 合同

- relation hit consumer实现kind15 separation后直接结束；没有kind16分支，未知kind最终unsupported。
- `effect=16`是独立target-type/action override矩阵字段，不得因退役itr.kind16而改变。
- kind15对character与type1/2/4/6的分离响应必须原样保留。
- unsupported kind16不得写HP/MP/HPBound/stat/frame/attacking/vrest/link/motion、不得消费RNG或发sound。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5LegacyKind16RetirementEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

## 验收

test-first runtime red；disposition unsupported、actual/shared character no mutation、generic weapon no mutation、
kind15保留、kind0/effect16仍走damage、hit-plan无writer projection/RNG/sound；相关hit、精确NTSD28 broad、
SelfCheck、Scene/Console/Ledger。

Test-first证据：focused job `855a8951bd8d445789c89db993d23891`，4/4按预期失败；分别命中
disposition、actual character、shared character与generic weapon旧kind16生产行为。

最终证据：编译0 error；focused `382df2f7b2fd44eba3c6031a853dab82` 5/5；hit-plan
`0a8d625400e7423ab857255991981dd5` 178/178；B5集合
`7ddd31eb8f034d66a477182f7eefc9bf` 117/117；96个精确NTSD28类
`bd0f8318547445588949542629aaf7ab` 675/675；2026-09-05T15:07:48Z SelfCheck PASS；
Console清空后0 error；Scene SHA/长度/mtime不变；Change Ledger 260 records/229 code files通过。

## 回滚

恢复Kind15Or16 disposition、character/weapon kind16 dispatch、ApplyKind16与projection及旧fixtures。
