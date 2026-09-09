# Task Contract — NTSD28-B5-KIND4-SOURCE-COUNT-CARRIER-001

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`
> 依赖：`NTSD28-B5-KIND4-ENVIRONMENT-OWNER-AUDIT-001 / VERIFIED`

## 目标

新增独立16位语义 `Kind4SourceCount92` runtime carrier：默认/full reset为0、input reset保留、canonical copy与
entity snapshot roundtrip、lockstep checksum、parity snapshot均精确保留；本包不接candidate或damage行为。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs`
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4SourceCountCarrierEditorTests.cs` 与 `.meta`
- 仅同步受影响schema断言的既有Editor tests
- 本Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- field独立于 `CatchSourceSlot90`、`WeaponCount`、`EnvironmentState320`；input reset保留，full reset清零。
- schema entity9→10、aggregate15→16、checksum18→19；所有既有精确schema断言同步。
- 只改变该值必须改变checksum/parity；warm copy/snapshot/checksum保持0 B。
- test-first后运行focused、snapshot/checksum相关、B5、Unity侧NTSD28自动回归、SelfCheck、Console、Scene和Ledger。

## 完成证据

compile-red为13条仅针对缺失 `Kind4SourceCount92` 的CS1061/CS0117；focused
`6ae72f6008fc4891a3e7a47a9339e397` 5/5、schema/carrier related
`502c544a9ea747cab6be71b60a8dfc26` 47/47。首次B5
`e9913404d1e84c94bf2a84e5b9cbcdec` 472/474只发现两条遗漏旧schema断言，补齐全部旧断言后
`1bfeb35c52824a4b8a006b4c30b7eb92` 474/474；Unity侧NTSD28自动回归
`71700a821f134906817fa29c37512af6` 939/939。06:26:19Z SelfCheck PASS，Console仅7条预期负路径，
Scene `D4266C6D...583B` unchanged，Ledger 317 Records / 273 governed files PASS。candidate/consumer行为未接。
