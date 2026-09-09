# NTSD28-B1-HOST-RUNTIME-TRACE-001 — Real Play Host control trace

<!-- CHANGE-RECORD
id: NTSD28-B1-HOST-RUNTIME-TRACE-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHostControlPlayModeProbeEditor.cs
authority: NTSD 2.8-Logan playable main.cpp host loop, native_function_keys.h F1/F2/F5 transition and exact 33ms/3ms intervals.
evidence: TASK-CONTRACT-CREATED / REAL-PLAY / SYNTHETIC-INPUTSYSTEM-KEYBOARD / PRODUCTION-CAPTURE-HOST-CONTROL-EDGES / DIAGNOSTIC-COMMAND-SEAM-NOT-USED / NORMAL-32.76602MS / FAST-4.4956583MS / FAST-NORMAL-RATIO-0.1372049 / PAUSED-F2-DELTA-1 / RUNNING-F2-LATENT-FALSE / MAX-TICK-JUMP-2 / REPORT-SHA256-07364B5362B5DCFC8D743EC9DCCAFD6F2CBBDFAA1FD94E99EBF99645FF95B95F / OS-PHYSICAL-PENDING / WORKER-INACTIVE
-->

> 状态：`FOCUSED_TEST_PASS / REAL_PLAY_SYNTHETIC_DEVICE_PASS / OS_PHYSICAL_PENDING / WORKER_INACTIVE`

本包是Editor-only真实Play探针，不修改production Host。它负责证明或否证当前Unity Update接线能否
在真实墙钟条件下复现权威F1/F2/F5和33/3ms节拍。

## 验收结果

- 第一阶段外部Keyboard queue因Editor窗口不聚焦停于`WaitPause`，后续诊断证明注入点状态不能
  单独代表Driver消费；失败报告全部保存在`Temp/NTSD28B1/`。
- 实际阻断为旧R8 poller无request仍`SetPaused(false)`；隔离后production Host edge完整运行。
- 最终报告SHA-256为`07364B...B95F`：Normal32.76602ms、Fast4.4956583ms、ratio0.1372049，
  paused F2仅+1，running F2无latent，最大单Update 2ticks。
- 自动化使用临时虚拟Keyboard，但实际经过`Keyboard.current`、production Driver和完整tick入口，
  未调用Host diagnostic command seam。OS实体键与inactive dedicated worker路径仍未覆盖。
