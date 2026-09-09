# Task Contract — NTSD28-B1-HOST-RUNTIME-TRACE-001

> 状态：`FOCUSED_TEST_PASS / REAL_PLAY_SYNTHETIC_DEVICE_PASS / OS_PHYSICAL_PENDING / WORKER_INACTIVE`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B1`  
> 建立日期：2026-09-03

## 目标

在当前真实`NTSD_Battle` Play Mode中通过Unity Input System物理设备状态注入F1/F2/F5，记录
normal/fast wall-clock tick间隔、pause稳定性、paused-only单步、恢复后的debt行为和实际Host
模式。先取得可复现运行时证据；若3ms fast mode受Unity Update上限阻断，必须报告first failure并
拆出生产修复包，不得用常量或diagnostic direct-call冒充真实运行时通过。

## 允许文件

- `Assets/NTSD/Scripts/Test/Editor/BattleHostControlPlayModeProbeEditor.cs`
- 对应`.meta`
- 本Task/Change、Ledger、STATE、handoff、总表

本包默认不允许改`SimulationTickDriver`、HostPolicy、Scene、Prefab、ProjectSettings或Input Actions。
若探针证明生产结构必须调整，另建独立B1实现Change。

## 不变量与验收

- 必须在真实Play Mode和当前`NTSD_Battle`生产driver上运行。
- 自动化层必须使用临时Input System Keyboard设备，经production`Keyboard.current`与
  `CaptureHostControlEdges`进入；不得调用Host diagnostic queue。OS实体键另列为未执行项。
- F1后至少跨多个Editor/player update确认tick不变且debt归零。
- paused F2必须恰好`+1 tick`并继续paused；running F2必须不排队。
- F5必须切换`ActiveHostIntervalSeconds`到`0.003f`，并分别记录normal/fast的elapsed/tick、
  tick jump、平均间隔和相对倍率；通过阈值按权威3/33比例并允许Editor调度抖动，但不得放宽到
  “只要比normal快”。
- 探针结束必须释放键盘状态和Editor update callback，并输出`Temp/NTSD28B1/` JSON。
- Unity fresh compile0、focused static tests不回归、真实Play report可解析；失败结果也必须留证据。

## 回滚

删除Editor-only probe及其meta，并从Ledger/STATE/handoff/总表追加`ABANDONED`或`ROLLED_BACK`
状态；不触碰生产Host control实现。

## 实际验收

- 首轮外部Keyboard队列在Unity窗口未聚焦时停于`WaitPause`；输入注入点虽可观测pressed，不能
  作为生产消费证据。随后以临时Input System Keyboard设备和PlayerLoop前置注入保持可重复。
- 真实调用栈发现两个旧R8 request poller在无request时强制unpause；由独立Change隔离后，
  F1 pause、paused F2、running F2 drop和F5均走production入口通过。
- 最终报告：`Temp/NTSD28B1/host-control-play.final-pass.json`，SHA-256
  `07364B5362B5DCFC8D743EC9DCCAFD6F2CBBDFAA1FD94E99EBF99645FF95B95F`。
- Normal `0.03276602 s/tick`，Fast `0.0044956583 s/tick`，ratio `0.1372049`；
  paused F2 delta=`1`，running F2 latent=`false`，最大tick jump=`2`。
- 自动化是临时虚拟Keyboard但走生产Input System/Driver路径；OS用户实体键未执行。当前真实场景
  dedicated worker为inactive，所以worker cadence runtime也未由本报告覆盖。
