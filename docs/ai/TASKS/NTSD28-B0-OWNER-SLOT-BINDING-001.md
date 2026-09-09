# Task Contract — NTSD28-B0-OWNER-SLOT-BINDING-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-BASELINE-UNCHANGED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

将 `identity.ownerSlot` 从 candidate 晋级 VERIFIED，绑定保持为
`NTSDEntityRuntime.OwnerSlotIndex`。权威字段是 native `Entity+0x354` 的物理 slot owner 链，
与 Unity OwnerStableId、SpawnerSlotIndex、RelationOwnerSlotIndex、HolderCopySlotIndex 均不同。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只改maturity/测试；projection字段不换，production owner/attribution/spawn逻辑不改。
- maturity `37/3/7 -> 38/2/7`；neutral真实差异 `9/equal38/occurrence54`保持。
- focused设置OwnerSlot17，并以不同的stable/spawner/relation-owner值防误绑，断言ownerSlot=17。
- 工具build0/0、21/21、5/5、format；Unity compile0、联合6/6；Ledger/diff通过。

## 回滚

恢复candidate与maturity37/3/7；不触碰production owner链。

## 实际结果

- contract晋级VERIFIED，maturity38/2/7；contract SHA
  `992D4821D6CCB482CE2749A28EE2916D9660AB88299760179C0397AB1A53B0E5`。
- 工具build0/0、21/21、5/5、format PASS；Unity compile0；job
  `b4b2472b4c85491291a05d28bc6978cd` 6/6 PASS，OwnerSlot17与19/21/23相邻引用分离。
- Unity raw SHA `7EB8DF9C131B0DD7C229B9C70493B78E20C68C278E0CA4878241C4AF0B6728F9`；
  neutral真实差异保持9/equal38/occurrence54，首差异baseMaxMp。
- production owner/attribution、DAT、资源与Scene未改。
- Change Ledger validator 85 records / 14 governed code files PASS；scoped diff check PASS（仅既有LF→CRLF提示）。
