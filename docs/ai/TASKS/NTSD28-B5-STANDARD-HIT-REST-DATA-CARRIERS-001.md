# Task Contract — NTSD28-B5-STANDARD-HIT-REST-DATA-CARRIERS-001

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`
> 依赖：`NTSD28-B5-STANDARD-HIT-REST-RECOVER-AUDIT-001 / VERIFIED`

## 目标

补齐standard-hit rest后续resolver所需的三类确定性数据：`InteractionArea.recover`、definition-level
`<bmp> effect`、world timing reduction 0..5。闭合parser/copy/runtime reset/snapshot/restore/checksum/parity；本包不
改变伤害/rest行为，不接完整selection UI，不修改冻结Config。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2FrameData.cs`
- `Assets/NTSD/Scripts/Animation/LF2CharacterData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- `Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestDataCarrierEditorTests.cs`
- 必要的既有focused tests与SelfCheck断言
- 本Task/Change、Ledger、STATE、handoff、总表与rest manifest

## 不变量

- 三个字段默认均0；timing reduction写入必须clamp到0..5并参与reset/snapshot/restore/checksum/parity。
- ITR recover参与`CopyFrom`与converter；definition effect只来自`<bmp> effect`，不得误读ITR/weapon-strength/
  system/mode的同名字段。
- 本包不得改变FrameDelay、AttackExempt、vrest或任何命中结果；后续resolver包才消费carrier。
- 不增加selection菜单、Scene/Prefab/Config写入，不改Direction B。

## 验收与回滚

test-first覆盖三项缺失、默认/边界/clamp、parser section ownership、copy、world reset、snapshot/restore与checksum/
parity变化；之后compile、focused、相关snapshot/checksum、exact broad、SelfCheck、Console、Scene、Ledger。

回滚删除三类carrier及其序列化/校验投影，不触碰已验证type3行为。

## 完成结果

- test-first `8dc2b685b2f542b7b7d4df48d7476537`：5/5 expected fail。
- focused `0ca5345f6a494c0e9003da37b8bcc1f4`：5/5 PASS；相关snapshot/restore/schema
  `0c110b3733e747058ce8ef2e3482b37a`：74/74 PASS。
- B5+HitPlan `d1c948c424384c049742ac4bcd300b8a`：383/383 PASS；104个`NTSD28*`
  `37e774ebbf0649c98657b8113d953e6f`：758/758 PASS。
- final compile0；SelfCheck `2026-09-05T21:17:06Z` PASS；filtered Console仅7条既有故意拒绝日志。
- Scene SHA/length/mtime未变；Ledger 277 records / 237 governed code files PASS。
- 三载体尚未被命中算法消费；下一pure resolver不得把carrier ready写成行为已对齐。
