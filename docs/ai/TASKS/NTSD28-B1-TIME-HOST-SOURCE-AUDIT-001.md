# Task Contract — NTSD28-B1-TIME-HOST-SOURCE-AUDIT-001

> 状态：`VERIFIED / SOURCE-CHAIN-CLOSED / IMPLEMENTATION-SPLIT-DEFINED / GOVERNANCE-ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B1`  
> 建立日期：2026-09-03

## 目标

闭合NTSD 2.8-Logan正式playable Host的normal/fast cadence、accumulator、pause、single-step、
input sampling与render handoff顺序，并逐项对照Unity `SimulationTickDriver`/HostPolicy，形成可实施的
B1拆包边界；本包只读，不修改运行逻辑。

## 权威已观察事实

- `native_loop_interval_milliseconds28(false/true)`精确返回`33/3`；不是`1/30s`。
- Windows Host每轮先累计wall-clock，再按当前normal/fast interval把logic debt截到最多2 ticks；
  每轮最多执行1次`GameSession28::step()`，扣1 interval，禁止同一Present前连续跑多tick。
- F5是edge host command：切换fast mode并请求下一loop把logic/render accumulator同时清0。
- F1/P切pause。paused仍采样并写入P1/P2 held input，但不step，并在每轮把logic accumulator清0。
- F2/O只在当前paused时请求一次single-step；running按下不会排队。paused single-step不等待interval，
  执行恰好一次完整step/audio/snapshot边界，随后accumulator=0且single-step latch清除。
- 非paused普通step发生于accumulator>=active interval；step后才建立新的battle snapshot。render按独立
  `render_fps` accumulator驱动；高于30时只在previous/current连续tick之间做presentation interpolation。
- `GameSession28::step()`先消费queued F3/F6/F7/F8/F9，再project globals/battle-flow，随后进入world
  combat step；Host F1/F2/F5本身不进入Core战斗状态。

## Unity已观察事实与差异

- `SimulationConstants.SIM_DT=1/30≈33.333ms`，与权威33ms有累计漂移；没有3ms fast cadence。
- `OfflineLocalTickPolicy`每Update最多1 tick，方向与权威一致；但backlog cap为
  `SIM_DT*maxBacklogTicks`（默认8），不是active interval*2。
- paused时`SimulationTickDriver.Update()`在function-key capture和policy BeginUpdate之前返回；
  `SetPaused`不清已有accumulator。Unity没有F1/F2/F5 Host latch或“running F2不排队”的合同。
- public `StepOneTick(ignorePaused:true)`可手动越过paused，但它是同步diagnostic/manual入口，会停worker，
  不能直接当作LocalFreeRun生产single-step实现。
- render alpha/backlog Inspector均除以旧`SIM_DT`；LateUpdate持续表现刷新，逻辑真值未被render反写。
- Manual/Lockstep由显式frame驱动，不消费wall-clock；B1 Host pacing不得改变其tick identity/input边界。

## 实施拆包

1. `B1-CADENCE-CONTRACT`：新增精确33ms/3ms cadence value与纯HostPolicy状态/测试；先不接键盘。
2. `B1-HOST-CONTROL`：接F1/F2/F5 edge、pause/single-step/fast accumulator reset，保证worker路径。
3. `B1-RUNTIME-TRACE`：真实LocalFreeRun与paused step、fast toggle、backlog cap、render handoff验证；更新B1差异表。

## 边界

不改Main pass、input payload语义、双RNG、Manual/Lockstep协议、Scene/Input Actions、DAT或表现插值算法；
不把只读审计写成实现完成。

