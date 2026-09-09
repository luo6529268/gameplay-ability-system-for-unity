# NTSD28-B5-UNARMORED-HP-CONSUMPTION-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-UNARMORED-HP-CONSUMPTION-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_INTEGRATION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5UnarmoredHpConsumptionProductionEditorTests.cs
authority: NTSD 2.8-Logan battle_world.cpp resolve_confirmed_unarmored_hit lines 6664-6701 and type6 skip; EXE B1E13AE1, closure 39DDDA15.
evidence: red 5f42f05b0c2a4ba183c10ad28466666a 13 expected failures/15; focused b2808f294bff41d79528326c1d04639e 15/15; HitPlan bad86aac590c4efc9c9cc1b696b3f5b0 184/184; B5 a52a7cc737b84da99a412e0870ce6f41 371/371; broad 90bde445d48c45608fac3ea1232058b4 836/836; SelfCheck PASS 2026-09-06T03:33:54Z; Console 7 intentional negative-path errors; scene D4266C6D...583B unchanged; scoped diff and ledger PASS.
-->

> 状态：`VERIFIED / UNARMORED_HP_CONSUMPTION_ALIGNED / FULL_RESOURCE_BLOCKED`

## Authority 与原状

- Authority在`skips_native_damage_tail = object_type == 6`门内，标准 mutation 后将
  `result.damage.hp_damage`累加到`Entity28+0x34C`；因此type 0/1/2/3/4/5写，type 6不写。
- Unity已在Runtime/HitPlan snapshot观察该字段，也已为reduced Type-1 armor写入；但standard character、
  normal weapon与type3/other normal vital helper尚未写入，构成当前first difference。

## 允许改动与风险

- 在三个既有normal vital helper及对应三个HitPlan projection helper中，同步累加effective injury。
- 新增focused测试，覆盖全部类型族、type6排除、broken armor fallback与actual/HitPlan一致。
- 风险是重复累计或把raw injury用于累计；通过helper单一所有权、边界测试和DifferenceMask捕获。

## 回滚

仅移除新增`InputHpConsumedTotal34C`写入和本包测试/文档；不触碰此前armor/runtime carrier。

## 验证

- 有效red `5f42f05b0c2a4ba183c10ad28466666a`：15项中13项按预期因`100 != 122/120`失败；
  type6 actual与HitPlan helper exclusion两项通过。
- focused green `b2808f294bff41d79528326c1d04639e`：15/15通过；Unity已完成脚本编译。
- HitPlan `bad86aac590c4efc9c9cc1b696b3f5b0` 184/184、B5
  `a52a7cc737b84da99a412e0870ce6f41` 371/371、NTSD28 broad
  `90bde445d48c45608fac3ea1232058b4` 836/836通过。
- BattleRuntimeSelfCheck在`2026-09-06T03:33:54Z` PASS；Console仅7条既有rest-binding负路径错误；
  Scene SHA `D4266C6D...583B`、length 205625、mtime不变；scoped diff与Ledger通过。
- 完整MP resource仍由baseMax、mode override与child suppression阻塞；cpoint/weapon-strength保持独立。

## 实际修改

- actual的standard-character、type3/5、type1/2/4 normal-vital helper在HP/HPBound mutation后，
  以同一effective injury对`InputHpConsumedTotal34C`做unchecked累加。
- HitPlan三个对应projection helper执行同值累加；既有DifferenceMask bit53负责actual/projection比较。
- type6在weapon helper的`normalVitalWeapon`门前返回，未新增写入；reduced armor仍由其独立helper单次写入。
