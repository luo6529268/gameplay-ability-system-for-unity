# NTSD28-B5-ITR-STATUS-FIELDS-001 — 13-field interaction status data contract

<!-- CHANGE-RECORD
id: NTSD28-B5-ITR-STATUS-FIELDS-001
status: VERIFIED
change-kind: TEST_FIRST_DATA_CONTRACT
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ItrStatusFieldCarrierEditorTests.cs
authority: NTSD 2.8-Logan InteractionRecord28 and CombatRecordDecoder28::interaction status fields; EXE B1E13AE1, closure 39DDDA15.
evidence: 13-FIELD-ORDER-READ / LITERAL-CONFUS-READ / UNITY-CONFIG0-AUTHORITY-CONTENT92-HOLD / TEST-FIRST-COMPILE-RED-X53 / COMPILE0 / FOCUSED5 / RELATED195 / NTSD28-BROAD563 / SELFCHECK-PASS-2026-09-05T09:02:27Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / ITR_STATUS_CONTRACT_READY / PRODUCER_DEFERRED`

补字段、Converter、CopyFrom、ECS projection/fingerprint，不执行producer。内容与规则边界见Task Contract。

## 当前实施与证据

- test-first导入得到53个缺字段/连锁compile errors。
- model默认0、Converter literal `confus`、CopyFrom、ECS ItrProjection/Kind5 replacement与两套fingerprint已纳入13字段。
- fresh compile error0；focused `b156533d430c4389a170e736b0253584` 5/5；parser/hit-plan/witness related `70af3a735207482982f8acc25f07d14d` 195/195。
- 精确NTSD28 broad `ec1ff2f0cbe045a6b55b5e1bcbb64d38` 563/563；SelfCheck
  `2026-09-05T09:02:27Z` PASS；Scene/Console/Ledger通过。
- data contract已闭合；producer/content仍后置。
