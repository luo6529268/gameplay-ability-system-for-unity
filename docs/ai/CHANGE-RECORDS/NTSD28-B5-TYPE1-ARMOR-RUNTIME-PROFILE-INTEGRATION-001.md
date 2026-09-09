# NTSD28-B5-TYPE1-ARMOR-RUNTIME-PROFILE-INTEGRATION-001 — profile and birth init

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE1-ARMOR-RUNTIME-PROFILE-INTEGRATION-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_INTEGRATION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorRuntimeProfileIntegrationEditorTests.cs
authority: NTSD 2.8-Logan battle_world.cpp spawn 1280-1284 and recovery 3806-3839; EXE B1E13AE1, closure 39DDDA15.
evidence: red fd23ee46ae4a4eb18c7809654e04ef4e 7/7; focused 3f842c7c401e467a88a91b498f8a6f3c 8/8; B5 0f91f25c5de84c3994de90eab04611e0 324/324; broad 366b59e0a0a3408cb6562b2a0b423228 800/800; SelfCheck PASS 2026-09-06T01:06:53Z; Console no C# error; Scene unchanged; Ledger PASS.
-->

> 状态：`VERIFIED / PRODUCTION_PROFILE_CONNECTED / HIT_SELECTION_UNCONNECTED`

## 原状与边界

Unity typed armor已加载，runtime fields/snapshot/C25i kernel已存在，但profile seam固定false且出生0/-1。
本包只接profile/birth/recovery，不接selection/damage/break或content。回滚恢复profile fail-closed、ModuleBind签名，
删除新test/meta。

## 验收状态

`TryGetNativeArmorRecoveryProfileForWorldPass`现读取current definition首armor；ModuleBind出生/reuse初始化
runtime hp/timer并清stale；snapshot shell显式跳过definition init；C25i复用既有exact owner/kernel。

验证：red `fd23ee46ae4a4eb18c7809654e04ef4e` 7/7；focused
`3f842c7c401e467a88a91b498f8a6f3c` 8/8；B5 `0f91f25c5de84c3994de90eab04611e0`
324/324；broad `366b59e0a0a3408cb6562b2a0b423228` 800/800；01:06:53Z SelfCheck PASS；
Console仅既有故意失败日志，Scene未变。selection/damage/break/content仍未接。
