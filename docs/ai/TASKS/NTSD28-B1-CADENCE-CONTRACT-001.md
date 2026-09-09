# Task Contract — NTSD28-B1-CADENCE-CONTRACT-001

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0 / HOSTPOLICY-4-OF-4 / RUNTIME-PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B1`  
> 建立日期：2026-09-03

## 目标

建立精确NTSD 2.8 Host cadence合同：Normal=33ms、Fast=3ms、wall-clock debt最多2个当前interval、
每Unity Update最多自动1tick、切换cadence清空debt。当前只让LocalFreeRun默认Normal消费新合同，并为后续
F5 Host control提供纯状态接口；Manual/Lockstep不自动消费wall-clock。

## 允许文件

- `Assets/NTSD/Scripts/Simulation/Core/SimulationConstants.cs`
- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickHostPolicy.cs`
- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`（仅注释/normal inspector denominator）
- `Assets/NTSD/Scripts/Test/Editor/SimulationTickHostPolicyEditorTests.cs`
- `AGENTS.md`（当前权威时间合同更正）
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- `SIM_DT`改为精确0.033f；新增Fast 0.003f。保留`SIM_TICK_RATE=30`只作为现有每tick像素换算常量，
  不再把它描述为wall-clock authority；本包不改物理换算。
- OfflineLocal默认Normal；accumulator cap=`activeInterval*2`，忽略旧8tick wall-clock cap；每Update仍最多1tick。
- `SetCadenceMode`只有模式变化时清accumulator并返回changed；相同模式不清debt。
- NaN/Infinity/negative elapsed继续归零；Manual/Network行为不变。
- 不接键盘、不增加fast runtime字段、不改pause/single-step、不改worker/Scene/Input Actions。
- 先更新focused tests使旧1/30/3tick backlog期望失败，再实现；Unity compile0、focused全过及相关Host tests不回归。
- B0 exporter/domain证据不回归；Ledger/diff通过。

## 回滚

恢复旧SIM_DT与Offline backlog策略及测试；不得回退B0或B1源链审计。

## 实际结果

- red job `bbd2211533f54adbb8b6a7f6f282a50b`：compile0，3项中2项按预期失败，实测旧值
  0.033333335s与0.266666681s debt，证明test确实捕获目标差异。
- `SimulationConstants.SIM_DT=0.033f`、`FAST_SIM_DT=0.003f`；`SIM_TICK_RATE=30`只保留物理每tick
  换算，不再充当Host cadence。
- OfflineLocal默认Normal、debt cap2 active intervals、每Update最多1tick；cadence变化清debt，
  同mode调用保留debt。Manual/Network未改。
- fresh Unity compile0；job `d112130597ff42eb93a483aff175b3a6` HostPolicy4/4；related
  `e0ee2f30ab344c9c8b5936cc6ccda2f0` 11/11；stress `84aa1067fb6141c3a52059bde8322207`
  3/3；B0 exporter joint `f5c7b46083914b5e8e1ab0f36a1ed5cf` 9/9；Console error0。
- Manual B0 domain raw SHA仍`D888201F...BBFC8`，domain compare状态不变；entity raw仅assembly header
  指纹变化为`12A17866...BE13CC`。
- .NET build0/0及comparator6/6、domain12/12、trace21/21、raw5/5、format通过；Ledger94/22与diff通过。
- F1/F2/F5尚未接线，LocalFreeRun真实wall-clock/worker/Play trace留后续B1 Host control/runtime包。
