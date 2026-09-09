# Task Contract — NTSD28-B0-FRAME-HISTORY-BINDINGS-001

> 状态：`FOCUSED_TEST_PASS / REAL-BASELINE-PASS / REQUEST-RERUN-DETERMINISTIC / JOINT-6-OF-6`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

联合更正两个frame history字段：authority `previous_action_078` 在每实体frame/state完成后写当前
action，对应Unity late tail `Frame.Prev`；authority `tick_action_snapshot` 在pair geometry前统一快照
当前action，对应Unity collision snapshot `Runtime.PrevFrame2`。旧 `previousAction→PrevFrame2` 候选
映射必须被纠正，不能留下双占用。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只改diagnostic bindings；不改MirrorLatePrevFrame/CaptureCollisionFrameSnapshot生产顺序。
- previousAction和tickActionSnapshot均晋级VERIFIED；maturity26 verified/10 candidate/11 missing。
- focused用不同非零值证明不会交叉：Frame.Prev=9、PrevFrame2=6。
- 工具build0/0、21/21、5/5、format；Unity compile0、联合6/6。
- 真实tickActionSnapshot差异关闭：unique14→13、equal33→34、occurrence84→78；Ledger/diff通过。

## 当前证据

- 工具build0/0、self-test21/21、raw self-test5/5、format PASS；contract SHA
  `9A7162337538C89D75ED279C5EA264BD90EAA00F4CC0FE4E581A2AE7F2976B03`。
- Unity fresh compile0。MCP bridge恢复后 focused job `16bf469ff22d48f9ad1ba1d977496949`
  6/6 PASS，非零 `Frame.Prev=9 / PrevFrame2=6` 分离断言实际执行通过。
- 同一Editor自带request通道连续两次PASS；两个raw SHA均为
  `9F5ABB4EC2197834B4E5311DB8AFD8F00BACDAE8251C1CEBEBC5D19D0CF6E0D5`，证明真实baseline确定性。
- 真实报告：13 unique differences / 34 equal / 78 occurrences；tickActionSnapshot差异已消失，
  首差异转为baseMaxMp。
- 本包现满足 focused 验收；后续 weaponHp raw 继续保持该映射，未出现回归。
