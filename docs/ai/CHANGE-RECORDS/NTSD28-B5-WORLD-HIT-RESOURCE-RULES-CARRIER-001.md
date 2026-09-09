# NTSD28-B5-WORLD-HIT-RESOURCE-RULES-CARRIER-001 — world hit-resource numeric rules carrier

<!-- CHANGE-RECORD
id: NTSD28-B5-WORLD-HIT-RESOURCE-RULES-CARRIER-001
status: VERIFIED
change-kind: TEST_FIRST_DETERMINISTIC_WORLD_CARRIER
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitResourceRulesCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldCoreScalarSnapshotEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleStateSnapshotRestoreEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitResourceSuppressionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
authority: NTSD 2.8-Logan NativeHitResourceRules28 and GameSession28 playable defaults 1C/34/38 = 0/75/75; EXE B1E13AE1, closure 39DDDA15.
evidence: WORLD-RESOURCE-RULES-F6-AUDIT-VERIFIED / TEST-FIRST-COMPILE-RED-CS1061-X24 / COMPILE0 / FOCUSED16 / RELATED59 / NTSD28-BROAD650 / SELFCHECK-PASS-2026-09-05T13:29:18Z / CONSOLE0 / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / CARRIER_READY / F6_PROJECTION_DEFERRED`

只建立world numeric rules carrier及确定性状态闭包。FunctionKeys继续唯一持有local gate；F6投影、
registration inheritance、selected-mode override与production transaction均不在本包。

focused/restore tests先写入，fresh compile取得24个预期`CS1061`；没有范围外编译错误。

- runtime root默认/reset为`0/75/75`；snapshot restore保留精确值，不猜测mode mapping。
- world core/aggregate/checksum schema升级为`7/14/17`，entity schema保持`9`。
- checksum、full parity与warm zero-allocation闭合。
- fresh compile 0 error；focused `00c10176507a4d4d9a0748d96b29ed9d` 16/16；related
  `6102e87e598b4910bc68b25b4b21ab73` 59/59；精确NTSD28 broad
  `a4349bdea8fe4c2387286cbb2d689805` 650/650。
- BattleRuntimeSelfCheck `2026-09-05T13:29:18Z` PASS；Console 0；Scene unchanged；Ledger PASS。
