# Task Contract — NTSD28-B1-UNITY-HOST-LOOP-BRIDGE-001

> 状态：`FOCUSED_TEST_PASS / REAL_PLAY_PASS / WORKER_PATH_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B1`  
> 建立日期：2026-09-03

## 目标

纠正B1 cadence包把“NTSD 2.8每个native Host loop最多一tick”机械映射成“每个Unity渲染
Update最多一tick”的适配错误。权威`main.cpp`的Host loop以约1ms独立轮询，logic33/3ms与render
interval分别累积，因此两次Present之间可发生多个Host loop/tick；Unity只有渲染Update入口时，应在
单次Update内最多排空已冻结的two-active-interval debt，仍禁止无界catch-up。

## 允许文件

- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickHostPolicy.cs`
- `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs`（仅注释/tooltip如需）
- `Assets/NTSD/Scripts/Test/Editor/SimulationTickHostPolicyEditorTests.cs`
- `AGENTS.md`
- 本Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- LocalFreeRun每Unity Update最多2个automatic ticks，与debt cap相同；第三次必须拒绝。
- debt仍先cap到2个active intervals；不恢复旧8tick backlog，不使用配置扩大。
- 正常无积压仍约33ms一tick；Fast可在一次Unity Update内消费2个3ms interval。
- 每个tick仍完整执行input/Core/snapshot/presentation publication；LateUpdate只消费最新表现。
- Manual/Lockstep、单tick dt、pause/F2、F5清debt、slot/RNG/Core pass均不变。
- dedicated worker若因单in-flight限制不能同Update提交第二tick，必须如实记录，不能假定已覆盖。
- test-first捕获旧second-tick拒绝；fresh compile0、focused/related通过，真实Play Fast平均需<=6ms
  且Fast/Normal<=0.25。

## 回滚

恢复`OfflineLocalTickPolicy`每Update只允许1tick；保留7.25ms Fast失败报告及Host/Present解释。

## 实际验收

- red job `7ac8d71706d94c7b9614e959416cd0e3`为6 pass/1 expected failure，旧policy拒绝同Update第二interval。
- final job `f5c9e43129e24cb29b4ac42aed60dbd7`为7/7；related job `bb773a0d99844a97ac0aec05a72a8021`为31/31。
- final真实Play：Normal32.76602ms、Fast4.4956583ms、ratio0.1372049、max tick jump2，通过阈值；报告SHA `07364B...B95F`。
- 当前场景dedicated worker inactive，单in-flight worker在同Update的第二tick能力仍未运行时覆盖。
