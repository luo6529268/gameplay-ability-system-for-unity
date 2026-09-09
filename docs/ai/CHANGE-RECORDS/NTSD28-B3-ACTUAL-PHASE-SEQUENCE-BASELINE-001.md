# NTSD28-B3-ACTUAL-PHASE-SEQUENCE-BASELINE-001 — Unity实际phase顺序基线

<!-- CHANGE-RECORD
id: NTSD28-B3-ACTUAL-PHASE-SEQUENCE-BASELINE-001
status: FOCUSED_TEST_PASS
change-kind: TEST_FIRST_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
authority: NTSD28BattlePassOrder contract backed by NTSD 2.8-Logan playable GameSession/SimulationTickDriver; current Unity NTSDBattleTickSystem actual execution.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-18-CS1061 / FIXED-CAPACITY-64 / DEFAULT-DISABLED / BEGIN-TICK-RESET / OCCURRENCE-ORDER / OVERFLOW-PRESERVES-PREFIX / FAIL-CLOSED-QUERY / EMPTY-WORLD-FULL-30 / INPUT-CLEAR-PARTIAL-5 / FIRST-DIFFERENCE-EXPECTED-CORE-SPARK-ADVANCE-ACTUAL-COOLDOWN / WARM-4096-ZERO-ALLOC / COMPILE-0 / FOCUSED-6-OF-6-503E6B91 / RELATED-90-OF-90-5255C46B / FULL-SELFCHECK-2026-09-04-212411-PASS / EXPECTED-NEGATIVE-7-REVIEWED / CONSOLE-0 / PRODUCTION-PASS-ORDER-UNCHANGED / AUTHORITY-READ-ONLY / NEXT-SPARK-ADVANCE-BOUNDARY-AUDIT
-->

> 状态：`FOCUSED_TEST_PASS / ACTUAL-SEQUENCE-READY / FULL-30-PARTIAL-5 / FIRST-DIFF-SPARK-VS-COOLDOWN / ZERO-ALLOC / PRODUCTION-BEHAVIOR-UNCHANGED`

## 改前事实

- `BattleTickPhaseDiagnostics`只累计各phase耗时，不保留发生顺序，因此重复phase与partial return无法从结果恢复。
- Unity current完整空world路径按源码有30个occurrence；这仍需真实`RunReleaseTick`测试冻结。
- 2.8 expected contract已存在，但未与production actual trace建立可重复首差证据。

## 计划

- test-first添加disabled/full/partial/overflow/fail-closed/zero-allocation测试。
- 在现有diagnostics内增加固定64项数组与只读query；只在Enabled路径记录。
- 记录首差，不在本包调整任何phase。

## 验证记录

- red：18个预期`CS1061`，仅缺sequence count/overflow/query API。
- compile：Unity scripts 0 error。
- focused：`503e6b91e405448c9335a777acbe7dc9`，6/6 PASS。
- related：`5255c46b424b4611831f5da4f8a068a7`，90/90 PASS。
- actual：完整空world tick=30 occurrences；input-clear partial=5；buffer 64项overflow保前缀。
- first difference：共同C00之后，expected `CoreSparkAdvance`，actual `Cooldown`。
- performance：query暖机后4096次零分配。
- SelfCheck：`2026-09-04T21:24:11.0655874+08:00 / PASS`；7 known negative复核后Console0。

## 实际改动与边界

- `BattleTickPhaseDiagnostics`新增固定容量sequence观测，不改变`BeginPhase/EndPhase`之外的生产调用。
- 新增6项Editor tests；未改任何phase顺序、world writer、Host/worker或presentation。
- 下一步只读审计spark advance/publication，不因当前首差直接在本包移动代码。

## 回滚

见Task Contract；没有战斗行为变更。
