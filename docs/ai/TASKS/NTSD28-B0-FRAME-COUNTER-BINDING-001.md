# Task Contract — NTSD28-B0-FRAME-COUNTER-BINDING-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-DIFFERENCE-CLOSED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 目标

修正 Unity raw projection 中 `frame.frameCounter` 的错误字段绑定。新权威
`FrameMachine28::step(...)` 在当前 action 驻留时递增 `FrameCursor28::frame_counter`，换帧/直接写入
时清零；Unity `LF2Entity.RunCommonFrameTick()` 对应递增、比较和清零的是
`NTSDEntityRuntime.AttackingCounter`。`FrameWaitCounter` 是另一类直接 frame write 保留/清零字段，
不是该 counter。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task、同 ID Change、Ledger、STATE、handoff、总表

## 不变量

- 只改 diagnostic binding/schema maturity 与相关测试/header count；不改战斗 counter 写入逻辑。
- `frame.frameCounter` 从 CANDIDATE 晋级 VERIFIED；总数改为 22 verified / 11 candidate / 14 missing。
- `FrameWaitCounter` 仍保留现有 runtime 语义，不删除、不重命名、不改 checksum。
- 不处理 allocationEpoch、baseMaxMp、environmentState 或 14 missing fields。

## 验收

- 工具 Release build、总 self-test、raw self-test。
- Unity compile 0 error；projection + exporter focused 6/6。
- 重新生成公共 Unity raw 后，frameCounter 为 tick1/2/3，真实 unique differences 从18降为17，
  equal fields从29增为30；其余差异集合不变。
- Ledger 与 diff check 通过。

## 当前证据

- `frame.frameCounter` contract/projection 改为 `NTSDEntityRuntime.AttackingCounter` 并晋级 VERIFIED；
  maturity 为22 verified / 11 candidate / 14 missing。
- 工具 Release build 0 warning / 0 error；总 self-test21/21、raw self-test5/5、format PASS。
- fresh Unity compile0；联合 job `3065f714adee42fd9a75cc09d28df067` 6/6 PASS。
- 修复独立 completed-tick前置后，真实 raw frameCounter 双实体均为1/2/3，差异项消失；unique
  differences18→17，equal29→30，occurrences105→99。
- 新 contract SHA：`B186E4C6904B3D319FE7C84CA49FEFBE6333FD63CE9EDBE8D3999840C43834C3`。
- Ledger 72 records / 14 governed code files / PASS；diff check无error。
