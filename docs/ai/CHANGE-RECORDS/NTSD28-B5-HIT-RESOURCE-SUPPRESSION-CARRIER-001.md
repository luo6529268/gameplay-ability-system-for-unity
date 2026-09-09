# NTSD28-B5-HIT-RESOURCE-SUPPRESSION-CARRIER-001 — suppression 15C carrier

<!-- CHANGE-RECORD
id: NTSD28-B5-HIT-RESOURCE-SUPPRESSION-CARRIER-001
status: VERIFIED
change-kind: TEST_FIRST_DETERMINISTIC_CARRIER
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitResourceSuppressionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 hit_resource_suppression_15c default/read/child propagation; EXE B1E13AE1, closure 39DDDA15.
evidence: RESOURCE-CARRIER-ATTRIBUTION-AUDIT-VERIFIED / DEFAULT-AND-PRODUCER-BOUNDARY-READ / TEST-FIRST-COMPILE-RED-CS1061-0117-X12 / COMPILE0 / FOCUSED6 / RELATED69 / NTSD28-BROAD638 / SELFCHECK-PASS-2026-09-05T12:49:59Z / SCENE-UNCHANGED
-->

> 状态：`VERIFIED / CARRIER_READY / PRODUCER_DEFERRED`

focused test已先写入，fresh compile得到12个预期`CS1061/CS0117`。

- default/full reset=0、input reset保留、canonical copy、entity snapshot均闭合。
- checksum/full parity加入`hitResourceSuppression15C`；B0 raw 48-field schema不改。
- schema已由8/12/15升级为9/13/16，全部既有schema断言同步。
- fresh compile 0 error；focused `701eecdfd8ff4b069516914cf3bb12d5` 6/6；
  related `21015094ea1445afbae067cb5601bd52` 69/69；精确NTSD28 broad
  `9db7870ab74844bd9aa9d5ffee7b3a44` 638/638。
- BattleRuntimeSelfCheck `2026-09-05T12:49:59Z` PASS；Scene SHA
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、
  mtime unchanged。producer仍后置。
