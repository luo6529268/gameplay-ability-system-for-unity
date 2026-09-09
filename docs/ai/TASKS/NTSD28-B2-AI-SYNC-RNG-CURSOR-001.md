# Task Contract — NTSD28-B2-AI-SYNC-RNG-CURSOR-001

> 状态：`FOCUSED_TEST_PASS / SYNCHRONIZED_CURSOR_READY / CONSUMERS_UNMIGRATED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

为production/indexed/shadow AI提供零分配的2.8 synchronized RNG cursor：多个候选共享同一不可变table，
但各自持有独立counter/index/calls/last-call-site；只有仍属于当前table generation的权威cursor可以提交。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomEditorTests.cs`
- 本Task/Change/Ledger/STATE/handoff/总表

## 不变量

- cursor `Next(callSite, upperBound)`与owner `SynchronizedNext`位级一致；`upperBound<1`不推进。
- struct copy必须只复制scalar，不能deep-copy或分配3001-byte table。
- 未提交cursor不得改变owner；成功提交只写counter/index/calls/last-call-site，不改table/CRT。
- reset、full restore、synchronized restore后旧cursor必须因generation失效，不能覆盖新状态。
- cursor version只用于进程内提交所有权，不进入确定性snapshot/checksum。
- 不改AI kernel、现有consumer、DAT、Config、Scene、ProjectSettings、Packages或权威目录。

## 验收

1. test-first因cursor/capture/commit seam缺失而红；
2. owner/cursor长序列一致；
3. copy隔离、显式提交、stale拒绝、nonpositive无消费通过；
4. warmed 4096 capture/next/commit零分配；
5. compile、focused/related、SelfCheck、Console0、Ledger通过。

## 回滚

删除cursor struct/version/capture/commit方法与新增测试；现有owner RNG状态和consumer不受影响。

## 结果

test-first20项缺失seam；final11/11、相关16/16、5000步位级一致、4096 warmed zero-allocation、
08:35:12 SelfCheck和Console0。consumer仍未迁移。
