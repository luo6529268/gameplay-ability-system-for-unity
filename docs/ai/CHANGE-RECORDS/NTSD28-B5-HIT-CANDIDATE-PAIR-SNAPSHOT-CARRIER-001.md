# NTSD28-B5-HIT-CANDIDATE-PAIR-SNAPSHOT-CARRIER-001

<!-- CHANGE-RECORD
id: NTSD28-B5-HIT-CANDIDATE-PAIR-SNAPSHOT-CARRIER-001
status: VERIFIED
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Animation/Character/ILF2SceneQuery.cs
code-path: Assets/NTSD/Scripts/Animation/Character/CollisionCandidateStore.cs
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitCandidatePairSnapshotCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
authority: NTSD 2.8-Logan HitCandidatePairSnapshot28 and frozen-pair producer/consumer live path; EXE B1E13AE1, closure 39DDDA15.
evidence: RED compile2; dedicated4, HitPlan185, RoleAware92, combined198, B5 533 and Unity-side NTSD28 1091 pass; build0; 21:03:07 SelfCheck PASS; Console0; Scene unchanged; Ledger333/288 PASS.
-->

> 状态：`VERIFIED / CARRIER_READY / FORMAL_PRODUCER_UNCONNECTED`

本包只建立完整pair snapshot value/copy/identity链；formal producer与eligibility仍不接线。

## 实际改动

- 新增allocation-free readonly `BattleHitCandidatePairSnapshot`，逐项覆盖Authority的18个payload字段加
  `Valid`，具备exact value equality/operator/hash；`default`明确为invalid兼容值。
- `SceneQueryHit`、`CollisionCandidateStoreEntry`、store-first写入/authority回读、三个cached/rebuild路径和
  shared runner runtime-ITR重建均按值保留snapshot。
- candidate store legacy shadow新增`PairSnapshotMismatch`；HitPlan Entry capture与legacy observation把snapshot
  纳入identity，不复制eligibility真值表。
- formal candidate创建仍未传snapshot，因此保持`Valid=false`；没有改变eligibility、nearest RNG、capacity、
  hit writer、persistent snapshot/checksum schema、content或Scene。

## 验证

RED compile2；专属4/4、HitPlan185/185、RoleAware92/92、combined198/198、B5 533/533、Unity侧NTSD28
1091/1091；build0；21:03:07 SelfCheck PASS；Console0；Scene dirtyfalse/root13/SHA `50FD4D8...FF3C`；
Ledger333/288 PASS。
