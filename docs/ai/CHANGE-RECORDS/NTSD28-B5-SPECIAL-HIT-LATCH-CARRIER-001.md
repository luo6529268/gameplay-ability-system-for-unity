# NTSD28-B5-SPECIAL-HIT-LATCH-CARRIER-001

<!-- CHANGE-RECORD
id: NTSD28-B5-SPECIAL-HIT-LATCH-CARRIER-001
status: VERIFIED
change-kind: TEST_FIRST_DATA_AND_TRACE_CARRIER
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5SpecialHitLatchCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitResourceSuppressionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4SourceCountCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitResourceRulesCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
authority: NTSD 2.8-Logan EntityState28::special_hit_latch_0eb bool and scenario28 raw key specialHitLatch0eb; EXE B1E13AE1, closure 39DDDA15.
evidence: red1 CS1061; focused carrier 8553aece 5/5; raw 4f0c88c7 3/3; related final 10+8+9+12 pass; B5 47d71e47 520/520; NTSD28 63d8652e 1078/1078; parity build0+selftests21/5; source-model build and 3-tick/6-entity capture validation pass; 18:25:08 SelfCheck PASS; final Console0; Scene 50FD4D8F unchanged dirtyfalse root13; Ledger327/282 PASS; behavior unconnected.
-->

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`

Authority字段出生为false、只有true writer、实体生命周期内无逐tick false writer。Unity旧`HitConfirm2`
同时承载临时weapon语义且会逐tick清零，因此必须新增独立carrier；本包只建立状态与trace合同，不接行为。

实际新增独立bool并闭合full reset、input-preserve、copy、entity/aggregate snapshot、lockstep/ECS hash、
parity、Unity raw与C++ source-model raw；schema现为entity11/aggregate17/checksum20，raw为49 fields / 43 verified / 6 missing。
中间related运行曾暴露一条旧`verifiedCount:42`断言并受到一次本地MCP disposed-stream日志污染；断言已同步，
污染后采用测试期间不轮询方式重跑，最终相关批次、B5和NTSD28全绿。

回滚见同名Task Contract。
