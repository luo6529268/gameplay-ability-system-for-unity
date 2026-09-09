# NTSD28-B3-NATIVE-SPARK-LIFECYCLE-CORE-001 — 原生spark逻辑生命周期core

<!-- CHANGE-RECORD
id: NTSD28-B3-NATIVE-SPARK-LIFECYCLE-CORE-001
status: FOCUSED_TEST_PASS
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeSparkLifecycleCoreEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::advance_native_sparks C01 and battle_world_tests native spark lifecycle fixtures; verified B3 spark boundary audit.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-14-COMPILE-ERRORS / TERMINAL-9-TO-99-LAST-DIGIT-9 / NONTAIL-TERMINAL-RETAIN / TERMINAL-TAIL-POP / AT-MOST-ONE-TAIL-POP / NONTERMINAL-0-TO-98-INCREMENT / INVALID-KEEP / XZ-STABLE / LEGACY-LAST-ADVANCE-TICK-UNCHANGED / FULL-TEN-RECORD-4096-ZERO-ALLOC / COMPILE-0 / FOCUSED-16-OF-16-58797CBA / RELATED-67-OF-67-31311A74 / FULL-SELFCHECK-2026-09-04-213521-PASS / EXPECTED-NEGATIVE-7-REVIEWED / CONSOLE-0 / PRODUCTION-UNCONNECTED / PRESENTATION-UNCHANGED / AUTHORITY-READ-ONLY / NEXT-C01-INTEGRATION
-->

> 状态：`FOCUSED_TEST_PASS / NATIVE-SPARK-CORE-READY / TERMINAL-TAIL-EXACT / ZERO-ALLOC / PRODUCTION-UNCONNECTED / NEXT-C01-INTEGRATION`

## 改前事实

- `LF2Entity`已有10槽age/x/z/lastAdvanceTick carrier与compact remove。
- 现有age推进只通过presentation finalize/no-publication，且规则依赖旧catalog；没有独立native lifecycle方法。
- 下一production integration需要一个不依赖资源/renderer/tickIndex的single-owner primitive。

## 计划

- 先写authority exact tests。
- 复用私有数组与remove helper实现最小internal方法。
- 本包不添加TickSystem caller，不修改旧presentation API。

## 验证记录

- red：14项预期missing primitive errors。
- compile：Unity scripts 0 error。
- focused：`58797cba3f0346c3a750cd6806e5d95b`，16/16 PASS。
- related：`31311a749abc487a8e56e632c7aa8b7b`，67/67 PASS。
- performance：满10槽owner反复4096次零分配。
- SelfCheck：`2026-09-04T21:35:21.1028358+08:00 / PASS`；7 known negative复核后Console0。

## 实际改动与边界

- `LF2Entity`新增internal terminal predicate与native lifecycle advance；复用既有compact storage。
- 新增focused Editor tests；未改AddHitRecord producer、presentation lifecycle catalog、TickSystem或renderer。
- 下一production package才允许添加C01 caller和移除U25正式逻辑writeback。

## 回滚

见Task Contract；production行为不变。
