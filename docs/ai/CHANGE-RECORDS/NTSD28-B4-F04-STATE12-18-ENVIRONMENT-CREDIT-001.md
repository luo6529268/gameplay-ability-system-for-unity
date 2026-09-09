# NTSD28-B4-F04-STATE12-18-ENVIRONMENT-CREDIT-001 — environment damage and credit count

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-STATE12-18-ENVIRONMENT-CREDIT-001
status: VERIFIED
change-kind: TEST_FIRST_CROSS_ENTITY_TRANSACTION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4State1218EnvironmentCreditEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::step_entity_physics environment damage block and record_native_knockout count side effect; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-ABS-SCALE-HP-HPBOUND-CONSUMED-READ / AUTHORITY-SOURCE-DECODE-TWO-OWNER-CREDIT-READ / AUTHORITY-LETHAL-KO-COUNT-BEFORE-SUBTRACTION-READ / B8-EVENT-SINK-DEFERRED / TEST-FIRST-BEHAVIOR-RED6-OF-7 / COMPILE0 / FOCUSED7 / RELATED71 / NTSD28-BROAD558 / SELFCHECK-PASS-2026-09-05T08:45:39Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / DAMAGE_CREDIT_COUNT_ALIGNED / B8_EVENT_DEFERRED`

## 原状与计划

contact action已对齐，但其前置environment damage尚未接入。新增exact/shared共用production method，
通过world slot查找与最多两跳owner归属更新victim/credit carrier；先锁行为红灯再实现。

## 边界

只闭合HP/HPBound/InputHpConsumedTotal/InputScoreTotal/KnockoutCount/EnvironmentState；不创建authority
knockout event feed，不写旧stats数组，不接B5 producer。回滚见Task Contract。

## 当前实施与证据

- test-first focused 1/7：唯一通过为non-contact gate，其余6项按预期因transaction缺失而失败。
- production现于contact action前处理abs/scale damage、`0x2000` decode、最多两跳owner、缺失owner保留当前credit、score与lethal KO count；raw-only claimed slot也可通过read-only view取得runtime。
- 不创建B8 event，不写KillStats/DamageStats。
- fresh compile error0；focused job `d5a0e8a3e419457ba07d85aaefcc8b00` 7/7；related job `ae1683de8f654facaa48d21922b4f152` 71/71。
- 精确NTSD28 broad `f74a0c6baff14a3cb54e1d96dba63262` 558/558；SelfCheck
  `2026-09-05T08:45:39Z` PASS；Scene/Console/Ledger通过。
- damage/credit/count事务已闭合；B8 event feed与B5 producer仍后置。
