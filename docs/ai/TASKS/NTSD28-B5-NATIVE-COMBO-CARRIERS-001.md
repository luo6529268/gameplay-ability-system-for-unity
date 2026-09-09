# Task Contract — NTSD28-B5-NATIVE-COMBO-CARRIERS-001

> 状态：`VERIFIED / CARRIERS_READY / BEHAVIOR_UNCONNECTED`
> 依赖：`NTSD28-B5-NATIVE-COMBO-RUNTIME-OWNER-AUDIT-001 / VERIFIED`

## 目标

新增与Authority独立对应的native combo entity/world载体，并闭合reset、canonical copy、raw/entity slot、ECS、
snapshot、checksum、parity和restore；本包不接普通命中、caughtact、expiry或presentation行为。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
- `Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5NativeComboCarriersEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleWorldEntityRuntimeSnapshotEditorTests.cs`（扩大回归发现新增`ulong`后的反射填充值分类）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitResourceSuppressionCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4SourceCountCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5SpecialHitLatchCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestDataCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitGroupModeGateCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitResourceRulesCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs`（仅schema期望）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅需要broad guard时）
- 本Task、Change Record、Ledger、STATE、handoff与总表

## 不变量与验收

- entity默认`count=0,lastTick=0`；world按Authority options默认`record=false,bound=0,facing=1,respond=50,caughtact=0`，不激活行为。
- 字段独立于输入`ComboD*`和伤害累计`ComboCountAtk/Vic`，各自修改不得互相污染checksum/parity。
- canonical copy、Reset、pool复用、entity/raw snapshot与restore、ECS mirror均保持精确值。
- world scalar snapshot/restore/checksum/parity纳入五字段并推进相应schema；旧字段顺序不重排。
- test-first取得缺字段RED后最小实现；不改Config、Scene、Prefab、资源或Authority。

## 回滚

撤销新增载体及其schema/test写入；不回退其他B5包或用户工作树。

## 完成证据

缺类型RED已取得；focused `6/6`、snapshot/ECS相关`29/29`、B5 `819/819`、NTSD28
`1166/1166`、fresh SelfCheck均通过；Scene SHA/mtime不变。本包未连接任何producer、expiry、
caughtact、presentation或正式内容tuple。
