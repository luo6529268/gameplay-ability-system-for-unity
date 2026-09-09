# NTSD28-B3-PASS-ORDER-CONTRACT-001 — 2.8不可变pass顺序合同

<!-- CHANGE-RECORD
id: NTSD28-B3-PASS-ORDER-CONTRACT-001
status: SUPERSEDED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSD28BattlePassOrder.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattlePassOrderEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
authority: NTSD 2.8-Logan playable GameSession28::step, SimulationTickDriver28::step and main completed-world snapshot boundary; NTSD28-B3-PASS-ORDER-CROSSWALK.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-CS0246-LINE242 / FIFTY-TWO-CHECKPOINTS / FIVE-DOMAINS / EIGHT-TRAVERSALS / SEVEN-FLAGS / PRIVATE-DESCRIPTOR-ARRAY / FAIL-CLOSED-QUERY / C24A-C24H-NESTED-0-7 / DOUBLE-HELD-REFILL / COLLISION-SNAPSHOT-DISTINCT-FROM-PREVIOUS-ACTION-COMMIT / FUNCTION-KEY-PRE-POST / USER-RANDOM-DROP-EXCEPTION-FLAGGED / COMPLETED-TICK-SNAPSHOT-LAST / WARM-4096-ZERO-ALLOC / COMPILE-0 / FOCUSED-10-OF-10-D89B7FCE / RELATED-42-OF-42-A1A0FEA3 / FULL-SELFCHECK-2026-09-04-211721-PASS / EXPECTED-NEGATIVE-7-REVIEWED / CONSOLE-0 / OLD-AUTHORITY-COMMENTS-CORRECTED / PRODUCTION-EXECUTION-UNCHANGED / AUTHORITY-READ-ONLY
-->

> 状态：`FOCUSED_TEST_PASS / IMMUTABLE-PASS-CONTRACT-READY / ZERO-ALLOC / PRODUCTION-UNCONNECTED / NEXT-ACTUAL-SEQUENCE-BASELINE`

## 改前事实

- B3 entry audit已冻结52个正常完整tick原子checkpoint，但Unity源码尚无可复用的2.8不可变order contract。
- 当前`BattleTickPhase`是旧Unity执行phase，不能作为2.8 expected sequence。
- 当前`NTSDBattleTickSystem`与`SimulationWorld.SerialTickAll`仍有旧`game_tick`/`C# authority`注释，
  会误导后续恢复；本包仅纠正措辞，不改变执行。

## 计划

- 先写精确顺序、domain、traversal、flags与fail-closed/zero-allocation tests。
- 再写私有静态descriptor表和只读值返回API，不暴露可变数组。
- 更正两处旧authority注释；不连接production scheduler。

## 验收记录

- red：缺contract时`NTSD28BattlePassOrderEditorTests.cs:242`产生预期`CS0246`。
- compile：Unity scripts 0 error。
- focused：`d89b7fce5a1a43b08320e82006614932`，10/10 PASS。
- related：`a1a0fea356144e77a58d1621bcce3819`，42/42 PASS。
- performance：`GetAt/TryGet/IndexOf/IsBefore`暖机后4096次零分配。
- SelfCheck：`2026-09-04T21:17:21.6544523+08:00 / PASS`；7条已知negative-path error已复核并清空，
  Console error=0。

## 实际改动

- `NTSD28BattlePassOrder.cs`：52项ID、domain/traversal/flags、值语义descriptor与不可变查询API。
- `NTSD28BattlePassOrderEditorTests.cs`：10项顺序、边界、nested tail、exception、fail-closed与zero-alloc测试。
- `NTSDBattleTickSystem.cs`：注释改为当前2.8 playable authority，并明确production仍在B3迁移。
- `SimulationWorld.cs`：`SerialTickAll`旧“C# authority”改为legacy Unity scheduling事实。
- production调用图、运行顺序、Config/DAT、Scene/Prefab、ProjectSettings与Packages均未改变。

## 未关闭项

- contract尚无production caller，不能报告B3顺序已对齐。
- 下一包必须建立actual-sequence recorder/fixture，先捕获当前首差，再按边界分组接线。

## 回滚

见Task Contract；production行为在本包前后应完全相同。

## 2026-09-04 Supersede

用户晋升Bug修复版EXE `B1E13AE1...9033`后，core pass顺序发生实质变化。本52-checkpoint合同的历史focused
证据保留，但当前裁决已由`NTSD28-B3-BUGFIXED-PASS-CONTRACT-REBASELINE-001`的57-checkpoint合同取代。
