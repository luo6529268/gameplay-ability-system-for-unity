# Task Contract — NTSD28-B0-PARTICIPANT-CLASS-BINDING-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-BASELINE-UNCHANGED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

将 `identity.participantClass` 从 candidate 晋级 VERIFIED，绑定保持为
`NTSDEntityRuntime.Unk344`。两端均代表 `Entity+0x344`，默认0，并被 OID122/123 边界、
死亡/闪烁、伤害/击杀统计与结果归类消费。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只改maturity/测试；projection字段不换，production Unk344 writer/consumer不改。
- maturity `36/4/7 -> 37/3/7`；neutral真实差异 `9/equal38/occurrence54`保持。
- focused以非零 `Unk344=4` 断言 participantClass=4。
- 工具build0/0、21/21、5/5、format；Unity compile0、联合6/6；Ledger/diff通过。

## 回滚

恢复candidate与maturity36/4/7；不触碰production participant state。

## 实际结果

- contract晋级VERIFIED，maturity37/3/7；contract SHA
  `704A1ED376DD46B8E2992EB6A747C4A24A67E5E9E548A07D3038AD242AE88553`。
- 工具build0/0、21/21、5/5、format PASS；Unity compile0；job
  `e879e34cb57b4f4291d1b33d01d71f9e` 6/6 PASS，Unk344=4投影通过。
- Unity raw SHA `8DA9F2B65B6409200B8C20DD3971A950139DF506605D15F94261FF8A741C79FB`；
  neutral真实差异保持9/equal38/occurrence54，首差异baseMaxMp。
- production participant、DAT、资源与Scene未改。
- Change Ledger validator 84 records / 14 governed code files PASS；scoped diff check PASS（仅既有LF→CRLF提示）。
