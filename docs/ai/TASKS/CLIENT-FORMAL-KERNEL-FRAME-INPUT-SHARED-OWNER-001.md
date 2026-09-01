# Task Contract — CLIENT-FORMAL-KERNEL-FRAME-INPUT-SHARED-OWNER-001

> 状态：`FOCUSED_TEST_PASS / SHARED_FRAME_INPUT_OWNER_READY / GOVERNANCE_CLOSED / S0_NOT_VERIFIED`
> 阶段：Formal S0 shared-owner Cut B move

## 1. 目标

把已经完成seam拆分的平台无关`FrameInputSet`公开value/hash合同及其`.meta` GUID，从Client源码路径移动到Server-owned `com.ntsd.battle-kernel/Runtime/Abstractions`。Unity与.NET必须编译同一份物理源码；现有namespace/type/API、bit、player/edge order、hash和Client warmed 0 B不得改变。

## 2. 允许范围

- 删除原Client `FrameInputSet.cs/.meta`，前提是同内容与同GUID已在shared package中建立。
- 更新必要的Client package/asmdef引用；当前local UPM依赖已存在，预期无需新建Client gameplay asmdef。
- 若Unity编译暴露由本次assembly move直接造成的问题，只修改事前Record逐项声明且属于value consumer边界的Client文件。
- 运行package FrameInput tests、existing seam/related tests、`BattleRuntimeSelfCheck`、S0 witness和existing lockstep回归。

## 3. 不变量

- `LocalFrameInputSource.cs`、`FrameInputSetPreallocation.cs`、`FrameInputDenseTraceBuilder.cs`继续属于Client，不进入package。
- Client capture继续held-only；pressed/released仍由Server/formal Kernel权威派生/校验。
- 不改变battle rules、30 Hz、Input Actions、Scene、资源、TargetTick/InputDelayFrames、transport、recovery、formal AI或formal marker。
- 不复制第二份FrameInput源码，不把Protocol DTO搬入Kernel。

## 4. 验收

- Client旧路径不存在，package路径是唯一production source，GUID保持`761d289e3f784d428423323b9d356853`。
- `typeof(FrameInputSet).Assembly`为`NTSD.Battle.Kernel.Abstractions`，现有Client consumers无需改type identity。
- Unity compile0、package/direct/artifact consumers、seam/related/S0/lockstep/SelfCheck和warmed0B均通过。
- formal marker保持false，S0/S5仍NOT_VERIFIED。

## 5. 回滚

把同一源码/GUID移回原Client路径，撤销本包的Abstractions/package consumer接线；不得回退RNG Cut A、seam或任何无关用户改动。

## 6. 完成证据

- Client旧source/meta已移除；Server-owned package是唯一production owner，GUID保持不变。
- .NET direct Debug/Release与exact `0.2.0` locked artifact consumer通过；Unity package2/2、seam/related48/48、S0 8/8、lockstep9/9、fresh SelfCheck通过。
- Client local manifest/lock无需本包新增修改；现有UPM引用直接消费新的Abstractions asmdef。
- Server Debug/Release、双Ledger、Queue、matrix、purity/GUID/version/no-pollution/diff审计通过。
- Capture/helpers、battle/tick/Input Actions/wire/transport/recovery/formal AI/marker均未改；S0/S5仍NOT_VERIFIED。
