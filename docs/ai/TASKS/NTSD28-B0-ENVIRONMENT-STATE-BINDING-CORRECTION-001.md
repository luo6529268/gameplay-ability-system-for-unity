# Task Contract — NTSD28-B0-ENVIRONMENT-STATE-BINDING-CORRECTION-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-CLASSIFICATION-CORRECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

纠正 `combat.environmentState` 的错误 candidate：权威字段是 native `Entity+0x320`，Unity
`NTSDEntityRuntime.Unk328` 是 OID51/52 融合状态而非其等价字段。Unity 当前无已确认绑定，因此
本包把该字段改为 MISSING/null，并把生产实现明确留给 B4/B5。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只改diagnostic contract/projection/测试；不改Unk328、物理、输入、hit、环境伤害或融合生产逻辑。
- maturity `38/2/7 -> 38/1/8`；真实差异仍9/equal38/occurrence54，但environmentState分类改为missing。
- focused保持Unk328=-3，断言environmentState=null且不泄漏-3。
- 工具build0/0、21/21、5/5、format；Unity compile0、联合6/6；Ledger/diff通过。

## 回滚

恢复错误Unk328 candidate仅用于回退本包；不得据此把旧映射重新当作有效结论。

## 实际结果

- contract改为MISSING/none，projection改为null，maturity38/1/8；contract SHA
  `5B2FC535E7BFBE827CF55B36D326FBCB93870A076F471D3960C0EA93DCB62B8A`。
- 工具build0/0、21/21、5/5、format PASS；Unity compile0；job
  `a86feda5f97e4e2a9f4423d0cd2ba22a` 6/6 PASS，Unk328=-3不再泄漏。
- Unity raw SHA `1BEF3981B70D33214B96B96BC07EDD4E5DD02A5E0A075D141E7CF1CDE65768E6`；
  neutral仍9/equal38/occurrence54，environmentState分类已改为UNITY_BINDING_MISSING。
- production Unk328/融合、物理、输入、hit、环境伤害、DAT、资源与Scene未改；真实+0x320实现留B4/B5。
- Change Ledger validator 86 records / 14 governed code files PASS；scoped diff check PASS（仅既有LF→CRLF提示）。
