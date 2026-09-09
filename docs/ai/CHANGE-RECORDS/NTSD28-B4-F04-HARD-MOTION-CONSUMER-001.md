# NTSD28-B4-F04-HARD-MOTION-CONSUMER-001 — pure hard-landing motion consumer

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-HARD-MOTION-CONSUMER-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_KERNEL
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4HardMotionConsumerEditorTests.cs
authority: NTSD 2.8-Logan battle_world.cpp consume_native_hard_landing_motion / FUN_0045A690; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-DX-FACING-CLAMP-AND-STRICT-500-SENTINEL-READ / AUTHORITY-DY-DZ-ADD-OR-OVERRIDE-READ / CONSUMED-FOUR-FIELDS-CLEAR-READ / TEST-FIRST-COMPILE-RED-CS0103-X7 / COMPILE0 / FOCUSED15 / RELATED55 / NTSD28-BROAD542 / SELFCHECK-PASS-2026-09-05T08:12:40Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / PURE_KERNEL_READY / PRODUCTION_TRANSACTION_DEFERRED`

## 原状与计划

7-field deterministic carrier已存在，但Unity尚无authority hard-motion consumer。新增单一无状态kernel，
只做Vx/Vy/Vz计算与dx/dy/dz/gain清理；先以focused compile red锁接口，再实现。

## 不变量与回滚

本包不接production、不选action、不处理environment/credit/KO，也不改schema/raw/Scene/内容。
回滚删除kernel与test，carrier保留。详见Task Contract。

## 当前实施与证据

- test-first导入后得到7个预期`CS0103`，均为缺少pure kernel。
- kernel已按authority实现dx hit-facing clamp、strict `>500` override、dy/dz add/override及四字段清理；不判断gain、不接production。
- fresh compile error0；focused job `1a800f4ae1e648c79aed9c433f167f72` 15/15。
- type0 physics/ordinary/state12-18/carrier related job `85fdd851075843ac8469f5f10d676127` 55/55。
- 精确NTSD28 broad job `d2876a6c37bb4fb68c78a05bf2c32584` 542/542；SelfCheck
  `2026-09-05T08:12:40Z` PASS；Scene/Console/Ledger通过。
- pure kernel已闭合；production transaction、B5 producer与B8 knockout event仍后置。
