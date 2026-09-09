# Task Contract — NTSD28-B1-HOST-PHYSICAL-EDGE-LATCH-001

> 状态：`FOCUSED_TEST_PASS / UNITY_COMPILE_0 / EDGE_LATCH_7_7 / REAL_PLAY_DEVICE_PATH_PASS`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B1`  
> 建立日期：2026-09-03

## 目标

将LocalFreeRun物理F1/F2/F5从Input System瞬时`wasPressedThisFrame`依赖改为Driver自身持有状态的
上升沿latch：读取当前`isPressed`，仅在false→true时产生Host command，release后允许下一次edge。
依据真实Play trace，输入注入点的F1同时为pressed/pressed-this-frame，但Driver Update未消费瞬时标志，
故此包消除Unity输入更新次序对Host命令的影响，不改变F1/F2/F5的权威语义。

## 允许文件

- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickHostPolicy.cs`
- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`
- `Assets/NTSD/Scripts/Test/Editor/SimulationTickHostPolicyEditorTests.cs`
- `AGENTS.md`
- 本Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- F1/F2/F5各自只在false→true产生一次command，持续按住不重复，release后再次按下可重触发。
- 同一采样中多键edge仍按既有F1→F2→F5 transition折叠。
- 非LocalFreeRun、shutdown、reset/recreate必须清held state，不把旧held状态跨边界带回。
- 不改变pause、single-step、33/3ms、worker入口、Manual/Lockstep或Core pass。
- pure latch tests必须先失败后实现；Unity compile0、Host focused与相关回归通过。
- 真实Play探针必须继续走生产`CaptureHostControlEdges`，不得调用diagnostic command seam。

## 回滚

撤销独立held latch及其reset调用，恢复`wasPressedThisFrame`读取；保留真实Play失败报告作为原因。

## 实际验收

- test-first fresh compile产生预期`CS0246`（latch尚不存在）；实现后job
  `3be40d5802984c6d99ecb5b4dc2cc286`为7/7。
- false→true、held no-repeat、release rearm、Clear rearm均由pure test覆盖；shutdown、match reset、
  非LocalFreeRun均清latch。
- final related job `bb773a0d99844a97ac0aec05a72a8021`为31/31。
- 最终真实Play的临时Input System Keyboard production path通过全部F1/F2/F5阶段；OS实体键未自动化。
