# NTSD28-B1-HOST-CONTROL-001 — F1/F2/F5 Host control

<!-- CHANGE-RECORD
id: NTSD28-B1-HOST-CONTROL-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickHostPolicy.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationTickHostPolicyEditorTests.cs
authority: NTSD 2.8-Logan native_function_keys.h host transition and playable main.cpp paused/running/single-step/fast-mode loop ordering.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST / LOCAL-FREE-RUN-PHYSICAL-HOST-ONLY / F1-F2-F5-FOLD-ORDER / RUNNING-F2-DROPPED / PAUSED-F2-EXACTLY-ONE-PRODUCTION-TICK / SINGLE-STEP-REUSES-PRODUCTION-TICK-ENTRY / F5-EXACT-33-3MS / PAUSE-AND-CADENCE-CLEAR-DEBT / MANUAL-LOCKSTEP-UNCHANGED / UNITY-COMPILE-0 / HOST-CONTROL-6-OF-6-JOB-3625DFDC23E04C5D815CD673B21EB6A0 / RELATED-33-OF-33-JOB-A92CA1F4DF3F444196635657FB2A6EE4 / DOTNET-BUILD-0-0 / TOOL-6-12-21-5-PASS / FORMAT-PASS / GLOBAL-LEDGER-95-RECORDS-22-FILES-PASS / SCOPED-DIFF-CHECK-PASS / RUNTIME-PENDING
-->

> 状态：`FOCUSED_TEST_PASS / UNITY-COMPILE-0 / HOST-CONTROL-6-OF-6 / RUNTIME-PENDING`

本包只接Host control，不修改Core pass或双RNG。

## 验收结果

- `SimulationHostControl.Apply(...)`按F1→F2→F5折叠同一Unity frame；F2依据F1后的pause状态，
  running F2丢弃，paused F2只生成一个single-step request。
- `SimulationTickDriver.Update()`只在`LocalFreeRun`读取InputSystem物理F1/F2/F5；Manual/Lockstep
  清Host request。pause每Update清本地debt，F5在33/3ms间切换并清debt。
- paused F2复用`StepOneTickInternal(nextTick, true)`，没有调用会停止dedicated worker的public manual
  overload；成功后只推进一次并保持paused。
- 初次fresh compile因测试误引`MoreMountains.Tools`产生`CS0246`，改用真实
  `NTSD.Tools.SingletonBehaviour`后编译恢复0 error。初次driver fixture因未绑定frame input provider而失败，
  诊断得到`frame-input-provider-is-null`；fixture改为调用既有生产选择规则
  `SetFrameInputProvider(null)`，未放宽生产前置条件。
- 最终Unity Host policy/control job `3625dfdc23e04c5d815cd673b21eb6a0`为`6/6`；worker、
  ordered shutdown、B0 raw等相关回归job `a92ca1f4df3f444196635657fb2a6ee4`为`33/33`；
  fresh Console为0 error。
- parity tool build为`0 warning / 0 error`，self-test为`6/6 + 12/12 + 21/21 + 5/5`；
  format、scoped diff check、Ledger validator通过，Ledger为`95 records / 22 governed files`。
- 本包只达到focused/compile证据。真实Play中的物理F1/F2/F5、normal/fast wall-clock cadence、
  pause debt和dedicated worker运行时路径仍待后续B1 runtime包，因此状态保持`RUNTIME_PENDING`。
