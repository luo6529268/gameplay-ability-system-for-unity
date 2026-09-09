# NTSD28-B4-F04-STATE12-18-CONTACT-ACTION-001 — state12/18 contact action transaction

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-STATE12-18-CONTACT-ACTION-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_TRANSACTION
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4State1218ContactActionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4State1218AirborneEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan BattleWorld28::step_entity_physics state12/18 contact action transaction and PhysicsIntegrator28 ground_contact_requires_action_resolution; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-CONTACT-SOFT-HARD-BRANCH-READ / AUTHORITY-ACTION-COUNTER-STATUS-SIDE-EFFECTS-READ / TEST-FIRST-COMPILE-RED-CS1061 / COMPILE0 / FINAL-FOCUSED9 / RELATED64 / NTSD28-BROAD551 / OLD-WEAPONCOUNT-SELFCHECK-X4-CORRECTED / SELFCHECK-PASS-2026-09-05T08:32:06Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / CONTACT_ACTION_SINGLE_OWNER / ENVIRONMENT_DAMAGE_DEFERRED`

## 原状与计划

Unity result只区分strict Landed/Airborne，导致already-grounded state12/18不进入事务；旧shared handler还使用
y0、WeaponCount与固定动作族。新增effective-floor contact同调用栈信号及single production owner，先锁红灯再实现。

## 边界

本包只接动作/速度/status事务。authority中位于它之前的environment damage、credit/KO由下一独立Change
接入；B5 producer与B8 event仍后置。回滚见Task Contract。

## 当前实施与证据

- test-first导入后得到预期`CS1061`：mechanics result缺少effective-floor contact。
- result已新增同调用栈contact flag；exact/shared production先处理state12/18事务，再让非目标strict landing进入ordinary owner。
- soft/hard、counter、picked/picking、status defer/consume及negative floor均按authority实现；environment damage未接。
- fresh compile error0；focused job `bda5d1455d80432cb57a7d76bd23271d` 9/9；related job `0e1940ff4c214f18bbae491b7f97fa3b` 64/64。
- broad 551/551后SelfCheck捕获旧PH-04仍要求state12/18消费WeaponCount并扣HP/HPBound；该期望与当前authority的EnvironmentState320来源相反。只更正测试为HP/HPBound不受WeaponCount影响，不恢复旧行为。
- 同一SelfCheck函数随后又捕获exact state12/18两条相同旧期望；共4条只更正测试，不恢复WeaponCount damage。
- final focused `1b1cc44b273643b7800f8fdbfdff638f` 9/9；精确NTSD28 broad
  `264eca5ec3864fc6a1a5530d062a8743` 551/551；SelfCheck `2026-09-05T08:32:06Z` PASS。
- Scene/Console/Ledger通过；environment damage/credit、B5 producer、B8 event仍后置。
