# Task Contract — NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001

> 状态：`VERIFIED / CLOSED`
> 依赖：`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-002 / VERIFIED`

## 目标

把正式`battle_world.cpp:6615-6651`的initial matching state3005/3006 non-character early-return接入
Unity type3 actual与HitPlan：在任何普通damage/resource/status/audio/hit-record之前只提交standard rests、双方pair
reset与attacker/parent hold release，然后结束本次命中。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3MatchedPairEarlyBranchEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本Task/Change、Ledger、STATE、handoff、总表与type3 manifest

## 不变量

- gate只接受target type3且双方进入命中前相同state3005或相同state3006；不同状态或普通target继续现有路径。
- early branch不写HP/HPBound/PP、damage/combo/stat、display/status/join/mimic、Fall/HitCount/HitState、
  definition/identity/ownership、sound、spark/effect override或hit record，也不消费RNG。
- 只写现有standard arest/vrest owner、双方latch-frame hit_Uj pair reset和已验证hold release；standard rest
  数值合同的全局timing/recover审计仍归B5/F11，不能在本包另造第二套算法。
- late transform/generic/pair、真正kind9、type0/weapon/other、content与Scene保持。

## 验收与回滚

test-first覆盖3005与3006、无伤害/状态/音频/record/RNG副作用、arest/vrest、latch hit_Uj、ordinary与negative
parent hold、near-miss非matching继续普通路径及HitPlan parity；随后compile、focused、B5/HitPlan、exact broad、
SelfCheck、Console、Scene与Ledger。

回滚只删除early gate/helper及对应投影，恢复原先先伤害后tail；不回退前置type3包。

## 完成结果

- test-first `9b171dd54db34183b46465dd02deef93`：3个预期失败、1个near-miss PASS。
- actual focused `9a9361464f7042c1932912543e8ed412`：4/4 PASS；HitPlan
  `216fb596c9c64bd9868ba955f0b3d0f2`：183/183 PASS。
- 全B5+HitPlan `c86abd44e29a4ebba07232757913e391`：378/378 PASS；103个`NTSD28*`
  类 `aebe5b7a2af5432391cb76623d7eb5da`：753/753 PASS。
- final compile0；`BattleRuntimeSelfCheck`于`2026-09-05T20:40:26Z` PASS；filtered Console仅
  7条自检故意registration/rest-binding拒绝日志。
- Scene SHA/length/mtime未变；Change Ledger 274 records / 236 governed code files PASS。
- 本包只关闭Type3 initial matching-pair前置顺序；standard rest全局timing/recover数值仍归B5/F11。
