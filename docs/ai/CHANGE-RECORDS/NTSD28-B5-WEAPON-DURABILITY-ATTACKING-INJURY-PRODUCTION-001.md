# NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-WEAPON-DURABILITY-ATTACKING-INJURY-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_ALIGNMENT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WeaponDurabilityAttackingInjuryProductionEditorTests.cs
authority: NTSD 2.8-Logan native_attacking_injury28 and type1/2/4/6 weapon_hp_31c writes; EXE B1E13AE1, closure 39DDDA15.
evidence: valid red 2d447eda316042c18b0ff949dea17247 = 10 completed / 7 expected failures / 3 passes; focused e40baaf6310544a6929c5a2dbb92ef7e 10/10; HitPlan 6bd475623acd40b691567ecad9c89dbc 184/184; B5 774e199b808642f1a91fe7680fa77afe 515/515; Unity-side NTSD28 automatic regression de616355c98e4b27bf787251dffdcb1f 1073/1073; SelfCheck PASS 2026-09-06T08:28:37Z; Console 7 intentional errors; Scene D4266C6D...583B unchanged.
-->

> 状态：`VERIFIED / WEAPON_DURABILITY_ATTACKING_INJURY_ALIGNED`

## 实际修改与验证

- 新增共享 `BattleDamageWriter.ResolveNativeAttackingInjury` seam，从attacker definition与world mode读取输入，复用既有低32位乘法/signed `/100` pure resolver。
- actual type1/2/4/6 durability与HitPlan counter projection统一扣effective attacking injury；raw `bdefend==100`仍最终写 `-1`。
- HP damage继续只受既有target scale/weak规则影响，未被attacking multiplier放大；content与完整resource transaction未改。
- valid red `2d447eda316042c18b0ff949dea17247`：10项中7项按预期失败、3项通过；focused green `e40baaf6310544a6929c5a2dbb92ef7e` 10/10。
- HitPlan `6bd475623acd40b691567ecad9c89dbc` 184/184；B5 `774e199b808642f1a91fe7680fa77afe` 515/515；Unity侧 `NTSD28` 自动回归 `de616355c98e4b27bf787251dffdcb1f` 1073/1073。
- SelfCheck 2026-09-06T08:28:37Z PASS；Console仅7条预期负路径；Scene SHA/length/mtime保持基线。

回滚同时撤销actual/HitPlan effective durability接线与本包测试；保留既有pure resolver和definition/mode carriers。
