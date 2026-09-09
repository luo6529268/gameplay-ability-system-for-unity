# Task Contract — NTSD28-B0-ACTION-LATCH-BINDING-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-DIFFERENCE-CLOSED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 目标

将 `frame.actionLatch` 从 MISSING 绑定到 `NTSDEntityRuntime.WaitCounter`。权威 FrameMachine 比较
action/action_latch、变化时清frame_counter、step结束锁存action；Unity RunCommonFrameTick比较
Frame.N/FrameTransistor.WaitCounter、变化时清AttackingCounter、末尾锁存当前frame，语义和时点闭合。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只补diagnostic binding；不改FrameTransistor/WaitCounter/AttackingCounter生产逻辑。
- maturity 24 verified / 11 candidate / 12 missing。
- 不同时裁决tickActionSnapshot与previousAction。
- 工具build0/0、21/21、5/5、format；Unity compile0、联合6/6；非零WaitCounter投影。
- 真实差异15→14、equal32→33、occurrence90→84；Ledger/diff check通过。

## 当前证据

- contract/projection绑定WaitCounter并晋级VERIFIED；maturity24/11/12。
- 工具build0/0、self-test21/21、raw self-test5/5、format PASS；contract SHA
  `225B50A17EDE245D8ABA505C8EFF52CDCDCD6E195AE916EB121F638B2C3B79D2`。
- Unity compile0；第一次联合job因旧断言仍要求actionLatch:null而5/6，移除被取代断言后job
  `82ca8ec798d940c9a856df552bd2a03d` 6/6 PASS，duration2.68846s；非零WaitCounter8已测。
- 真实差异15→14、equal32→33、occurrence90→84；首差异变为tickActionSnapshot；Unity raw SHA
  `1A48AE32...53597`。
- Ledger75 records/14 governed code files/PASS；diff check无error。
