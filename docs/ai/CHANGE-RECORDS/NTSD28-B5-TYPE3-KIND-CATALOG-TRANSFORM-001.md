# NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001 — type3 kind catalog transform

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001
status: VERIFIED
change-kind: TEST_FIRST_AUTHORITY_BEHAVIOR_PORT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3KindCatalogTransformEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan battle_world.cpp 0x0042F854..0x0042F903 with runtime kind.dat SHA39E30DF8; EXE B1E13AE1, closure 39DDDA15.
evidence: AUDIT-VERIFIED / TEST-FIRST-f56956a9d53d4b91bb9a8686e7e46f67-4FAIL-3PASS / EFFECT-PROJECTION-RED-5d59eb8f9e1646128a52d997d850cbdc-1FAIL / COMPILE0 / FOCUSED-23e81c942dd94520a7b32c77fb3c8d4a-7OF7 / EFFECT-PROJECTION-GREEN-79039bca114d45e0803c85efbc1f3c56-1OF1 / HITPLAN-FOCUSED-1fa5a45700a646c6b74b4f500a427d60-190OF190 / B5-HITPLAN-81851020bd4a4800b31a501cb1368142-367OF367 / EXACT101-BROAD-62423486feae4073879c3e0be2b7ae64-742OF742 / SELFCHECK-PASS-20260905T193126Z / FILTERED-ERROR-CS0 / EXPECTED-TEST-ERROR-LOGS7 / LEDGER-PASS / SCENE-D4266C6D-UNCHANGED
-->

> 状态：`VERIFIED / TYPE3_LOCKED_KIND_TRANSFORM_ALIGNED`

## Authority与Unity原状

Authority直接复制attacker identity/group/owner并写三份action history，只清pending total。Unity当前按parent复制
HolderCopy、清Runtime velocity、固定20/30，并让OID8/213依赖active209替换；HitPlan有两套D1专用oracle。

## 验收状态

- one-record bound/respond gate保持；actual transform统一直接复用attacker wrapper和current data OID，不再扫描active209
  或调用外部config resolver。OID8/209/213均按自身身份转移。
- group/owner直接取attacker；action/WaitCounter/Frame.Prev=40、AttackingCounter=0、HitConfirm2=1；只清
  Knockback XYZ，保留HolderCopy、AnimCounter、WeaponCount、Runtime XYZ、HitCount、PN和Prev2。
- matching-state pair reset仍在transform后执行；effect override的HitPlan读取已转移definition的property、frame40
  body和Prev，而不是旧target definition。
- 旧`ReplaceWithActiveKarasuData`生产helper及把它称为canonical的SelfCheck已移除；extended capacity期望随测试实体
  减少由458更正为459。
- red `f56956a9d53d4b91bb9a8686e7e46f67` 4 fail/3 pass；actual focused
  `23e81c942dd94520a7b32c77fb3c8d4a` 7/7。transferred-definition effect projection red
  `5d59eb8f9e1646128a52d997d850cbdc` 1 fail，green
  `79039bca114d45e0803c85efbc1f3c56` 1/1。
- final HitPlan+focused `1fa5a45700a646c6b74b4f500a427d60` 190/190；B5+HitPlan
  `81851020bd4a4800b31a501cb1368142` 367/367；exact101 broad
  `62423486feae4073879c3e0be2b7ae64` 742/742。
- fresh compile runtime 19:27:10Z / Editor 19:27:11Z；SelfCheck 19:31:26Z PASS；filtered CS0；
  Console 7条error-type均为既有预期注册拒绝/回滚日志；Scene `D4266C6D...583B` unchanged。
