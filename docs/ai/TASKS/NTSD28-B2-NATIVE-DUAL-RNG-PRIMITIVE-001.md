# Task Contract — NTSD28-B2-NATIVE-DUAL-RNG-PRIMITIVE-001

> 状态：`FOCUSED_TEST_PASS / PRIMITIVE_READY / WORLD_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

在 Unity Client 内建立未接入 `SimulationWorld` 的 NTSD 2.8 双 RNG 基元，逐字段复现
`Msvcr80Random28`、`NativeRandom28` 与 `SynchronizedRandomState28`。先以权威常量和状态转换写
focused EditMode tests，取得缺失类型的 red evidence 后再实现。

## Authority

- `source/ntsd28_core/include/ntsd28/native_random.h`
- `source/ntsd28_core/src/simulation/native_random.cpp`
- `source/ntsd28_core/tests/native_random_tests.cpp`
- playable/core build scripts均显式包含 `native_random.cpp`；`README_SOURCE.md` 声明源码对应当前发行 EXE。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs`（新增）
- `Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs.meta`（新增）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomEditorTests.cs`（新增）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomEditorTests.cs.meta`（新增）
- 本 Task/Change/Ledger/STATE/handoff/总表文档

## 不变量

- 不修改 Server-owned `DeterministicRng`、`AiDecisionRandomStream` 或任何现有消费者。
- 不接 `SimulationWorld`，不改变 seed、snapshot、checksum、trace、input 或生产行为。
- table 固定 3001 bytes；capture/restore 必须避免可变数组别名污染。
- 非正 upper bound 不消费 synchronized state；restore 对 index/counter 做与 C++ 相同的正模归一化。

## 验收

1. test-first 先得到缺失生产类型的预期编译失败；
2. seed 1 CRT 首三个值为 41/18467/6334；
3. reset seed 1 后 CRT calls=3000，table[0]=42、table[1]=108、table[3000]=0，前3000项均1..255；
4. 2999/1233 wrap、advance-before-read、call-site记录及 nonpositive no-consume 与权威一致；
5. snapshot/restore、负 index/counter 归一化、table deep-copy 和 hash 确定性通过；
6. Unity compile 0 error，focused suite全过，相关旧 RNG tests无回归，Ledger和diff check通过。

## 回滚

删除本包新增的两个 C# 文件及 meta，并把治理记录标为 `ROLLED_BACK`；不触碰任何现有 RNG owner。

## 实际结果

- test-first red：Unity 报 12 个预期 `CS0246`，全部为四个缺失的 2.8 RNG 类型。
- 新增 `NTSD28Msvcr80Random`、`NTSD28SynchronizedRandomState`、`NTSD28NativeRandomState`、
  `NTSD28NativeRandom`；状态 capture/restore 对 table 深拷贝，index/counter 正模归一化。
- focused job `736e7f06a7c14471b6ad41170a2fc58f`：本包 6/6、shared RNG 1/1、
  boundary RNG 3/3，共 10/10 PASS，0 failed/skipped。
- B0 authority seed `682973786` 的 CRT state `1758127634`、calls 3000、table hash
  `A1BA1B90EA55796D` 与真实 source-model raw 相等；seed1、wrap/no-consume、5000-call确定性均通过。
- Unity compile 0 error、Console 0 error。world、AI、现有消费者、Scene/DAT/资源均未改。
