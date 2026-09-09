# Task Contract — NTSD28-B1-HOST-CONTROL-001

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0 / HOST-CONTROL-6-OF-6 / RUNTIME-PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B1`  
> 建立日期：2026-09-03

## 目标

在Unity LocalFreeRun Host接入NTSD 2.8 F1/F2/F5 edge合同：F1切pause，F2只在paused请求一次完整tick，
running F2不排队，F5切Normal33ms/Fast3ms并清wall-clock debt。paused每Update清logic debt；single-step
复用`StepOneTickInternal(int,...)`，让现有dedicated worker选择保持有效。

## 允许文件

- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickHostPolicy.cs`
- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`
- `Assets/NTSD/Scripts/Test/Editor/SimulationTickHostPolicyEditorTests.cs`
- `AGENTS.md`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- physical F1/F2/F5只在LocalFreeRun读取InputSystem `wasPressedThisFrame`；Manual/Lockstep清Host request。
- transition固定按F1→F2→F5折叠同一Unity frame；F2依据F1处理后的paused状态，行为写入test。
- F1进入paused立即清debt；paused后续Update继续清debt。F1退出paused不注入旧debt。
- F2 running不产生pending；paused F2恰好推进1tick，保持paused并清debt；未成功提交时只保留1个pending。
- F5只改变Host cadence/Inspector alpha，不改变Core规则、tick identity、input payload或Manual/Lockstep。
- single-step调用`StepOneTickInternal(int, true)`，不得调用会Stop worker的public synchronous manual overload。
- shutdown/stopped时不接收Host request；SetPaused(false)仍不能越过Stopping/Stopped。
- 先写pure transition与driver diagnostic tests；Unity compile0、focused/worker/ordered-shutdown相关测试通过。
- B0 Manual domain SHA、B1 cadence tests与Ledger/diff不回归；真实Play cadence另包。

## 回滚

撤销Host command/latch与driver接线，恢复只支持Normal policy；保留精确33/3ms cadence合同。

## 实际验收

- Unity fresh compile：Console `0 error`。
- Host policy/control：job `3625dfdc23e04c5d815cd673b21eb6a0`，`6/6`。
- worker、ordered shutdown、B0 raw等相关回归：job
  `a92ca1f4df3f444196635657fb2a6ee4`，`33/33`。
- `.NET` parity tool：build `0 warning / 0 error`；self-test `6/6 + 12/12 + 21/21 + 5/5`。
- format、scoped diff check及全局 Ledger validator通过；Ledger为`95 records / 22 governed files`。
- 真实Play、物理F1/F2/F5和33/3ms wall-clock trace未在本包执行，保留为下一B1 runtime包。
