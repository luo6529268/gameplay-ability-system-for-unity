# NTSD28-B4-F04-COLLISION-Y-CARRIER-001 — collision-Y deterministic carrier

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-COLLISION-Y-CARRIER-001
status: VERIFIED
change-kind: TEST_FIRST_DATA_CONTRACT
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
authority: NTSD 2.8-Logan EntityState28::collision_y_reference lifecycle and trace schema; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-RED-CS1061-CS0117-X14 / COMPILE0 / FOCUSED6 / RELATED29 / RAW-MATURITY-FIXTURE3 / TOOL-BUILD0-TRACE21-RAW5 / NTSD28-BROAD465 / SELFCHECK-PASS-2026-09-05T04:58:24Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / CARRIER_READY / PRODUCERS_UNCONNECTED`

## 原状

`combat.collisionYReference`已在47/48-field raw contract中存在，但Unity binding为missing且exporter
恒null；runtime、snapshot和checksum没有独立字段。

## 计划与回滚

按现有EnvironmentState/C25 carrier模式增加单一signed int并推进相关schema version。回滚时删除
字段/test、恢复version与raw missing binding；不涉及生产行为或数据迁移。

## 已写入

- 红灯：新测试产生14个预期CS1061/CS0117，全部来自缺失`CollisionYReference`。
- runtime新增signed carrier，full reset清0、input reset保留、canonical copy原值复制。
- entity/full/checksum schema 6/10/13→7/11/14；checksum与parity transform纳入字段。
- raw exporter从41/7变为42/6并输出真实值；NTSD28Parity contract从MISSING改为VERIFIED。
- 既有精确schema与raw maturity断言同步到新版本；所有producer/behavior consumer未接。

## 验证

- Unity compile error0；focused job `7fa860714e60492198f305bd750b188f` 6/6，含4096次
  copy/checksum 0 B；联合 job `476bb680451240bbaee362ec0809f639` 29/29。
- tool Release build 0 warning/0 error；trace self-test 21/21、raw self-test 5/5。
- 首次broad `85cac1290fe5408d8420fe8322b6323e` 仅旧MissingBindings长度7断言失败；更正为6并
  增加collisionY不在missing后，专门fixture job `f842387476de41ca8634d3e37b6a6ec2` 3/3，
  final broad job `c38e3e256d014932831b9e7cae865a88` 465/465。
- BattleRuntimeSelfCheck `2026-09-05T04:58:24Z` PASS；Scene SHA/length/mtime仍
  `0D74E174...D77 / 203477 / 2026-09-04T13:12:45Z`；clear后Console error0；
  diff-check无whitespace error；Ledger 215 Records / 193 governed files PASS。

## 未关闭

operation30/platform source、linked dvy、defusion copy、physics、teleport、next999、input、fusion和
hit consumers均未连接；本状态只证明carrier与确定性/trace边界，不代表非零floor行为已对齐。
