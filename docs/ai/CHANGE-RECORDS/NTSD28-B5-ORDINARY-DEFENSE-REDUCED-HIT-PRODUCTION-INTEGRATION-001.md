# NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-PRODUCTION-INTEGRATION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-PRODUCTION-INTEGRATION-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_INTEGRATION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryDefenseReducedHitProductionIntegrationEditorTests.cs
authority: NTSD 2.8-Logan defense_resolution.cpp match_ordinary, damage_resolution.cpp resolve_selected_armor(null), battle_world.cpp 7023-7269/apply_reduced_hit_rest; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-3 / FOCUSED-3 / B5-272 / HITPLAN-184 / NTSD28-BROAD-748 / SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / PRODUCTION_CONNECTED / NULL_ARMOR_DEFENSE_ALIGNED`

## 原状与改动边界

旧 `LF2AlternateDamageResolver` 依赖 OID37/6/52、Prev2 state 与 bdefend<=60；actual/HitPlan alternate 都做
`FallDamageDiv -> raw/10`、固定3/-5，无法表达 current defense、spark/dbdefend/OID822、HP-only target scale 与
definition-effect/timing reduction。四个 pure/data 前置包现已 VERIFIED，本包进行原子接线。

只连接 selected armor=null 的普通防御；type1 model/content、完整 armor activation/HP/recovery、audio/spark 独立后续。
回滚恢复上述三个生产 owner 与旧 selfcheck 段，并删除新增 focused test/meta。

## 验收状态

- selector已用`BattleOrdinaryDefenseResolver`退休OID37/6/52/Prev2启发式，并接current state、spark/dbdefend、OID822。
- actual/HitPlan均用`BattleReducedHitDamageResolver`与`BattleReducedHitRestResolver`；null armor raw `/10`、HP-only
  `+0x340`、weak exclusion、definition-effect/timing rest及active-holder投影一致。
- red `64c185d8ea764361833501871a06986f` 3/3；focused `c135e2f2d75e4f3aae70f7d9680b7d10` 3/3；
  B5 `01820bd8c553461fbbb0447017ec0267` 272/272；HitPlan `99358936b6df409d8c2ad01b41641446`
  184/184；broad `b40650fe9e164dddb3118b1bc83418a7` 748/748。
- Runtime/Editor compile `23:51:38Z / 23:36:23Z`；23:53:08Z SelfCheck PASS；filtered CS0；Scene unchanged；
  Ledger 286/248 PASS。

type1 armor/content未接。下一exit audit；回滚恢复selector/actual/projection与旧selfcheck fixtures并删除focused test/meta。
