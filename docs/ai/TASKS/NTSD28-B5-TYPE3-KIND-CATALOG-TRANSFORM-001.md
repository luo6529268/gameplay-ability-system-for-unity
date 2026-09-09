# Task Contract — NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001

> 状态：`VERIFIED / TYPE3_LOCKED_KIND_TRANSFORM_ALIGNED`
> 依赖：`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-AUDIT-001 / VERIFIED`

## 目标

将正式one-record type3 transform接入Unity actual与HitPlan：bound type3 attacker命中respond type3 target时，
直接复制attacker group/owner/definition/id/type，写action/latch/Prev=40、counter0和HitConfirm2，只清pending impulse
total并保留其他字段；随后保持既有pair reset/effect tail顺序。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3KindCatalogTransformEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本Task/Change、Ledger、STATE、handoff、总表与type3 manifests

## 不变量

- candidate gate顺序和值保持，补exact matrix；不新增/部署kind.dat或通用内容parser。
- transform source始终是attacker本身，不读取negative-link parent，不扫描active OID209，不经外部config resolver。
- 保留target HolderCopySlot、AnimCounter、WeaponCount、Runtime XYZ、HitCount、Prev2及Frame.PN；只清Knockback XYZ。
- response action固定record.frame=40；同时写WaitCounter和Frame.Prev=40。source wrapper/frame40必须在mutation前可用。
- pair reset与effect override继续后置；真正kind9、generic continuation、内容、Scene不改。

## 验收与回滚

test-first覆盖3 bound×7 respond、candidate gate双方向、negative-link direct ownership、完整preserve/write矩阵、
无active209/config resolver、state3005 skip、pair reset和HitPlan parity。随后fresh compile、focused、HitPlan/B5、
exact broad、SelfCheck、filtered errors、Scene hash和Ledger。

回滚移除统一transform transaction/projection与新测试，恢复前一generic continuation已验证状态；不回退前置包。

## 最终证据

- 初始red：`f56956a9d53d4b91bb9a8686e7e46f67`，7项中4 fail/3 pass。
- actual focused：`23e81c942dd94520a7b32c77fb3c8d4a` 7/7。
- transferred-definition effect projection：red `5d59eb8f9e1646128a52d997d850cbdc` 1 fail；green
  `79039bca114d45e0803c85efbc1f3c56` 1/1。
- final focused+HitPlan：`1fa5a45700a646c6b74b4f500a427d60` 190/190；B5+HitPlan：
  `81851020bd4a4800b31a501cb1368142` 367/367。
- exact101 broad：`62423486feae4073879c3e0be2b7ae64` 742/742；SelfCheck 19:31:26Z PASS。
- fresh compile0、filtered CS0；7条error-type为既有预期测试日志；Scene并发基线不变；Ledger PASS。
- 下一步：`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001`只读确认type3 family无遗留首差并选择B5下一owner。
