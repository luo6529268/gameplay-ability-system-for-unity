# Task Contract — CLIENT-FORMAL-KERNEL-FRAME-INPUT-SEAM-001

> 状态：`FOCUSED_TEST_PASS / FRAME_INPUT_SEAM_READY / GOVERNANCE_CLOSED / S0_NOT_VERIFIED`
> 阶段：Formal S0 shared-owner Cut B prerequisite

## 1. 目标

把现有`FrameInputSet.cs`中混合的Client capture、preallocation、dense-trace helper与平台无关的公开input value/hash合同分开；保持所有现有type identity、按钮bit、player order、held/pressed/released值、hash字节和warmed 0 B行为不变。

本包只建立可搬迁seam，不把`FrameInputSet`移入共享package。后续移动包固定命名为`CLIENT-FORMAL-KERNEL-FRAME-INPUT-SHARED-OWNER-001`，需要新的独立授权。

## 2. 允许范围

- 修改Server Change Record中逐项声明的Client runtime/test文件。
- 新建`LocalFrameInputSource.cs`、`FrameInputSetPreallocation.cs`、`FrameInputDenseTraceBuilder.cs`、`FrameInputSetSeamEditorTests.cs`及必要`.meta`。
- 在不改变现有consumer签名的前提下更新可复用frame初始化与reset调用。
- 运行Unity编译、FrameInput focused tests、`BattleRuntimeSelfCheck`、S0 8/8和existing lockstep 9/9。

## 3. 不允许

不改battle rules、30 Hz tick、Scene、资源、Input Actions、TargetTick/InputDelayFrames、transport、Socket、database、公网、snapshot/recovery、S1 wire、formal AI或formal marker；不在Server复制FrameInput实现。

## 4. 验收

- `FrameInputSet.cs`只保留公开平台无关值/hash/canonical-order合同。
- Client capture、preallocation和dense trace各自有清晰owner文件。
- exact bit layout与golden hash `0x25B94F895B464DCB`通过。
- reusable/immutable hash与replay行为相同；existing warmed allocation tests仍为0 B。
- Unity focused/SelfCheck/S0/lockstep均有fresh通过证据。
- S0/S5仍NOT_VERIFIED，formal marker仍false。

## 5. 回滚

只回退本Task声明的seam/call-site/test文件并恢复单文件职责；不得回退RNG Cut A或其他用户改动。

## 6. 完成证据

- Unity 编译最终 `error CS=0`；首次刷新发现的 3 个 test-only `CS1061` 已在声明的测试文件内改为显式 helper 调用并复验清零。
- FrameInput seam focused job `6089d3694527487a8aff01f8cf347257`：`4/4 passed`，含 golden hash 与 warmed `0 B`。
- 相关 FrameInput/worker/history/buffer/replay/checksum 回归 job `a5723463beba46e684dbdd2d75ea02ed`：`44/44 passed`。
- S0 witness job `f945e8a369a048b4b21761f43a946c94`：`8/8 passed`；existing lockstep job `138335c1ae6e416dbee3078b3d5bb6a4`：`9/9 passed`。
- `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `2026-08-30 11:32:54 +08:00` 写入 `PASS`；最终 Console C# error 查询为 0。
- Client Change Ledger validator、双仓库 scoped diff、FrameInput 纯度扫描与 Server Debug/Release 全链均通过；formal marker 仍为 `false`。
- 本包没有修改 package manifest/lock 或 asmdef，没有执行 shared-source move；后续 `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SHARED-OWNER-001` 仍需新的明确授权。
