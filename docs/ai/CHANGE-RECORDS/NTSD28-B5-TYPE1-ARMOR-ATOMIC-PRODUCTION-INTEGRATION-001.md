# NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-INTEGRATION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE1-ARMOR-ATOMIC-PRODUCTION-INTEGRATION-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_INTEGRATION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleOrdinaryCharacterDamageRouteResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorAtomicProductionIntegrationEditorTests.cs
authority: NTSD 2.8-Logan simulation_tick_driver.cpp C14 selection and battle_world.cpp resolve_ordinary_type1_armor_standard_hit/resolve_confirmed_reduced_hit/broken fallback; EXE B1E13AE1, closure 39DDDA15.
evidence: red 7271e82dd0b348808b39b72acf3e0c48 9 expected failures/10; focused c587baa487d1403c8c5f1d323f85c83d 10/10 pass after compatibility correction; B5 a49b1c0fc993408aa1337f0788d2b908 353/353 pass; HitPlan e2725b9e161c4890aa218f2459eb1c11 184/184 pass; NTSD28 broad 08d7c6a1e13f4522b1f225b07e9b2632 818/818 pass; BattleRuntimeSelfCheck PASS 2026-09-06T02:28:31Z; Console only 7 intentional rest-binding negative-path errors; scene baseline D4266C6D...583B unchanged; diff-check and ledger PASS.
-->

> 状态：`VERIFIED / PRODUCTION_CONNECTED / TYPE1_ARMOR_CORE_TRANSACTION_ALIGNED`

所有typed/pure/runtime/HitPlan前置已闭合。本包只接type1 armor普通角色命中原子事务；通用resource transfer、
audio/spark与正式content保持原有独立门。回滚删除route resolver/test/meta并恢复五个生产调用点。

## 实际修改

- 新增结构化`BattleOrdinaryCharacterDamageRouteResolver`，统一ordinary defense优先、首type1且恰好一条、
  match、activation与六种明确route；effective injury独立实现native low-32-bit signed截断语义。
- 两条角色actual入口不再各自提前做布尔alternate分流，统一进入`ApplyStandardCharacterDamage(...)`；
  writer依据同一route提交selected reduced或unarmored fallback。
- selected armor reduced写HP/PP、`InputHpConsumedTotal34C`、`InputMpConsumedTotal350`与
  `RuntimeArmorHp118`，只在armor HP非正时累计bdefend，并按armor ratio调整reaction threshold/reduced rest。
- broken armor fallback先写`-1`，在unarmored horizontal后应用armor action并归零。
- HitPlan的can/project入口使用同一route，投影相同HP/PP/consumption/armor/rest/break副作用。
- `ShouldUseAlternateHurt`保留为无World依赖的ordinary-defense兼容诊断API，不再承担生产type1选择。

## 验证结果

- test-first red：`7271e82dd0b348808b39b72acf3e0c48`，10项中9项按预期失败；MP不足fallback一项因旧路径
  碰巧同为unarmored而通过。
- 初次实现focused `fd9b920e824e42d69a9573f7e5d3d8e4` 10/10、B5
  `a2b8713df8c94c1b856a266867d2c496` 353/353、HitPlan
  `00b295b48e6b467ca253104d85a62a58` 184/184、broad
  `c8bd22990a064aeda8c3a90668d24c2c` 818/818通过。
- 初次SelfCheck在`ShouldUseAlternateHurt`无World场景捕获兼容回归；修正生产HitPlan改为直接读route并恢复纯
  defense API后，focused `c587baa487d1403c8c5f1d323f85c83d` 10/10、HitPlan
  `e2725b9e161c4890aa218f2459eb1c11` 184/184、broad
  `08d7c6a1e13f4522b1f225b07e9b2632` 818/818、最终B5
  `a49b1c0fc993408aa1337f0788d2b908` 353/353均通过。
- `BattleRuntimeSelfCheck`：最终`PASS`，`2026-09-06T02:28:31.5504687Z`。
- Console：仅7条self-check故意触发的rest-binding负路径错误，无编译错误。
- Scene：SHA-256 `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`，
  长度205625、mtime `2026-09-05T15:44:52.7794120Z`未变。
- `git diff --check`与Change Ledger均PASS；Ledger为Records 296、governed code diff 260。

## 剩余边界

本包只宣称type1 armor core transaction接入。Authority同一reduced/unarmored尾中的通用hit-resource transfer、
armor audio/spark以及正式armor content仍分别属于B5 resource、B10、B9、H/B11；下一exit audit重新裁决family状态。
