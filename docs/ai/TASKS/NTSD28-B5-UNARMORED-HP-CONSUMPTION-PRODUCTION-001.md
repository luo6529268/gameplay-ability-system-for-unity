# Task Contract — NTSD28-B5-UNARMORED-HP-CONSUMPTION-PRODUCTION-001

> 状态：`VERIFIED / UNARMORED_HP_CONSUMPTION_ALIGNED / FULL_RESOURCE_BLOCKED`
> 依赖：`NTSD28-B5-HIT-RESOURCE-PRODUCTION-READINESS-AUDIT-002 / VERIFIED`

## 目标

仅把 Authority `resolve_confirmed_unarmored_hit(...)` 的 `Entity28+0x34C` 写入接到 Unity
production actual writer 与 HitPlan：无护甲/Type-0 fallback 成功扣除 HP 后，按有效 `hp_damage`
累加 `InputHpConsumedTotal34C`；覆盖 type 0/1/2/3/4/5，type 6 明确不写。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5UnarmoredHpConsumptionProductionEditorTests.cs` 与 `.meta`
- 本 Task/Change、Ledger、STATE、handoff、总表与 hit-resource manifest

## 不变量

- 只使用已经解析出的有效 HP injury；不把 raw ITR injury、display step 或 weapon durability 当作累计值。
- type 6 继续跳过 Authority 的 damage/resource/status tail；kind 9、matched-pair early return 与 reduced armor 不重复写。
- 不接完整 MP resource transaction，不修改 baseMaxMP、mode override、child suppression、cpoint、weapon-strength self-cost。
- 不修改 content、Scene、Prefab、ProjectSettings 或权威目录。

## 验收

test-first覆盖 character、type1/2/4、type3/5、type6 exclusion、Type-1 armor broken fallback和
HitPlan/actual一致；随后 compile、focused、HitPlan、B5、NTSD28 broad、SelfCheck、Console、Scene、diff与Ledger。

## 完成证据

有效red `5f42f05b0c2a4ba183c10ad28466666a`为13项预期失败/15，type6两项通过；green focused
`b2808f294bff41d79528326c1d04639e` 15/15、HitPlan `bad86aac590c4efc9c9cc1b696b3f5b0`
184/184、B5 `a52a7cc737b84da99a412e0870ce6f41` 371/371、broad
`90bde445d48c45608fac3ea1232058b4` 836/836；SelfCheck 03:33:54Z PASS，Console仅7条已知
负路径日志，Scene `D4266C6D...583B` unchanged，scoped diff与Ledger PASS。
