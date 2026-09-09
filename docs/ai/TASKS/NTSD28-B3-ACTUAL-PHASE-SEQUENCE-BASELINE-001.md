# Task Contract — NTSD28-B3-ACTUAL-PHASE-SEQUENCE-BASELINE-001

> 状态：`FOCUSED_TEST_PASS / ACTUAL-SEQUENCE-READY / FULL-30-PARTIAL-5 / FIRST-DIFF-SPARK-VS-COOLDOWN / ZERO-ALLOC / PRODUCTION-BEHAVIOR-UNCHANGED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3`  
> 依赖：`NTSD28-B3-PASS-ORDER-CONTRACT-001 / FOCUSED_TEST_PASS`  
> 建立日期：2026-09-04

## 目标

扩展现有opt-in `BattleTickPhaseDiagnostics`，在不改变调度和默认运行成本的前提下，按发生顺序记录
当前Unity phase occurrence（含重复的StageBounds/HeldProcess）。用真实空world完整tick与input-clear partial
tick冻结现状，并明确记录相对2.8 contract的第一个B3结构差异。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`：只扩展已存在的diagnostics recorder。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`（新增）及`.meta`。
- 本Task/Record、Ledger、STATE、handoff、总表和B3 manifest。

禁止修改具体pass调用顺序、World/Entity行为、Host/worker、Config/DAT、Scene/Prefab、ProjectSettings、
Packages或authority。

## 合同

- recorder默认disabled；disabled时sequence count为0。
- enabled后每次`BeginTick`清零count/overflow，每次有效`BeginPhase`按发生顺序追加一次。
- 固定容量64；超出时保留前64项并置overflow，不分配、不覆盖。
- `TryGetLastPhaseAt`按值返回；越界fail closed。
- 正常空world完整tick必须记录当前30个occurrence；input-clear partial tick必须只记录5项。
- 用actual sequence与2.8 contract明确首差：共同的input phase边界之后，expected为`CoreSparkAdvance`，
  current Unity下一项为`Cooldown`（对应authority末尾reaction/timer族，而非tick-start spark）。

## Test-first验收

1. 先写focused tests，因sequence API不存在得到预期compile red。
2. 实现最小固定容量recorder；Unity compile 0 error。
3. focused与相关phase/order测试通过，查询暖机后4096次零分配。
4. 完整SelfCheck PASS、known negative日志复核、Console error=0。
5. validator与`git diff --check`通过。

## 回滚

移除新增sequence字段/API和测试即可；本包不改变任何生产pass调用。

## 实施与验证结果

- test-first red：新增test后得到18个预期`CS1061`，全部指向尚不存在的sequence count/overflow/query API。
- recorder：复用现有opt-in diagnostics；固定64项buffer，`BeginTick`清count/overflow，`BeginPhase`按occurrence
  追加；越界保留前64并置overflow；disabled不记录。
- 真实`NTSDBattleTickSystem.RunReleaseTick`空world完整路径固定为30项；`NeedClearInput` partial path固定为5项。
- runtime first difference：把`BattleFlow`内的input-phase advance作为共同C00后，Authority下一项为
  `CoreSparkAdvance`，Unity实际下一occurrence为`Cooldown`。
- Unity compile 0 error。
- focused job `503e6b91e405448c9335a777acbe7dc9`：6/6 PASS。
- related job `5255c46b424b4611831f5da4f8a068a7`：90/90 PASS，覆盖新/旧phase diagnostics、
  immutable order contract与AI shadow diagnostics。
- query API暖机后4096次零分配。
- `BattleRuntimeSelfCheck`于`2026-09-04T21:24:11.0655874+08:00`写入`PASS`；7条已知negative-path
  rest-binding日志复核后清空，Console error=0。
- production phase调用及其顺序零修改；Config/DAT/Scene/Prefab/ProjectSettings/Packages与authority未改。

下一步：`NTSD28-B3-SPARK-ADVANCE-BOUNDARY-AUDIT-001`只读闭合Authority C01与Unity hit-record/spark
age/publication链，确认能否只移动逻辑边界而不提前重写B9视觉行为。
