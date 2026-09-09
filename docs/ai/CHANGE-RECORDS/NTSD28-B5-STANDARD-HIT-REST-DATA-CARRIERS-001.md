# NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001 — standard hit rest data carriers

<!-- CHANGE-RECORD
id: NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001
status: VERIFIED
change-kind: TEST_FIRST_DATA_CONTRACT
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/Animation/LF2CharacterData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitResourceSuppressionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitResourceRulesCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleStateSnapshotRestoreEditorTests.cs
authority: NTSD 2.8-Logan InteractionRecord28.recover, ObjectDefinition28 bmp effect and BattleWorld28 timing_reduction_4a9ff4; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-5-EXPECTED-FAIL / FOCUSED-5-5 / RELATED-74-74 / B5-HITPLAN-383-383 / NTSD28-758-758 / SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`

## 原状与边界

Unity只有ITR rawProperties可能保留recover字符串，缺typed consumer；LF2CharacterData缺definition effect；world缺
timing reduction及确定性投影。当前正式/冻结内容前两项均0，本包只补载体，不改行为或内容，不实现selection UI。

## 实际改动

- `InteractionArea.recover`默认0，进入converter、CopyFrom、HitPlan `ItrProjection`和两种fingerprint。
- `LF2CharacterData.definition_effect`默认0，仅从`<bmp> effect`转换，不读取ITR/weapon/system同名字段。
- `NTSD28StandardHitRestRuntimeState`提供0..5 clamp/reset/restore；进入core snapshot、full restore、checksum与
  full parity `standardHitRest.timingReduction4A9FF4`。
- Core/full/checksum schema由7/14/17推进为8/15/18，并同步所有既有硬编码守卫；完整selection UI未接。

## 验收状态

- 红灯：`8dc2b685b2f542b7b7d4df48d7476537`，5/5 expected fail。
- focused：`0ca5345f6a494c0e9003da37b8bcc1f4`，5/5 PASS。
- related：`0c110b3733e747058ce8ef2e3482b37a`，74/74 PASS，包含聚合snapshot restore。
- B5+HitPlan：`d1c948c424384c049742ac4bcd300b8a`，383/383 PASS。
- exact 104类：`37e774ebbf0649c98657b8113d953e6f`，758/758 PASS。
- final DLL Runtime `2026-09-05T21:05:27.7041309Z`、Editor
  `2026-09-05T21:06:45.0745273Z`；compile error0。
- SelfCheck `2026-09-05T21:17:06Z` PASS；Scene unchanged；Ledger277/237 PASS。

## 未扩大结论

本包不改变任何FrameDelay/arest/vrest结果；resolver和actual/HitPlan production接线仍未开始。
