# NTSD28-B5-KIND4-SOURCE-COUNT-CARRIER-001

<!-- CHANGE-RECORD
id: NTSD28-B5-KIND4-SOURCE-COUNT-CARRIER-001
status: VERIFIED
change-kind: TEST_FIRST_DATA_CARRIER
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4SourceCountCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitResourceSuppressionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitResourceRulesCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
authority: NTSD 2.8-Logan uint16 kind4_source_count_92 persistent carrier; EXE B1E13AE1, closure 39DDDA15.
evidence: compile-red 13 CS1061/CS0117 only for missing field; focused 6ae72f6008fc4891a3e7a47a9339e397 5/5; related 502c544a9ea747cab6be71b60a8dfc26 47/47; first B5 e9913404d1e84c94bf2a84e5b9cbcdec 472/474 exposed only stale schema assertions; corrected B5 1bfeb35c52824a4b8a006b4c30b7eb92 474/474; broad 71700a821f134906817fa29c37512af6 939/939; 06:26:19Z SelfCheck PASS; Console expected7; Scene D4266C6D unchanged; Ledger317/273 PASS; behavior unconnected.
-->

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`

回滚为移除独立carrier/parity/checksum、新测试并恢复schema9/15/18；不改EnvironmentState320或WeaponCount。

`Kind4SourceCount92`现为独立persistent carrier，schema为10/16/19；下一原子接production behavior。
