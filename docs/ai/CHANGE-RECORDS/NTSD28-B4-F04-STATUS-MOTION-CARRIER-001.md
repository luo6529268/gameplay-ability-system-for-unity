# NTSD28-B4-F04-STATUS-MOTION-CARRIER-001 — status motion/action deterministic carrier

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-STATUS-MOTION-CARRIER-001
status: VERIFIED
change-kind: TEST_FIRST_DATA_CONTRACT
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 status_dx_1c0 through status_picking_action_1d8 defaults/lifecycle and BattleWorld28 producer/consumer persistence; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-HEADER-DEFAULTS-0-0-0-0-0-191-185 / PRODUCER-CONSUMER-PERSISTENCE-READ / RAW-48-LEAF-EXACT-CONTRACT-PRESERVED / TEST-FIRST-COMPILE-RED-CS1061-X21 / COMPILE0 / FINAL-FOCUSED6 / RELATED60 / NTSD28-BROAD527 / SELFCHECK-PASS-2026-09-05T08:00:57Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / CARRIER_READY / PRODUCER_CONSUMER_DEFERRED`

## Authority 与原状

当前权威 `battle_world.h` 声明 7 个持久字段，默认值依次为 `0/0/0/0/0/191/185`；
`battle_world.cpp` 的 hit/status producer 写入它们，state12/18 contact consumer在后续 tick读取并选择性清理。
Unity当前没有同语义字段，不能以即时速度或已有无关carrier代替。

## 计划

只建立完整确定性 carrier 链：default/full reset、input reset preservation、canonical copy、snapshot/
restore、checksum、parity与schema版本。先写 focused tests 取得缺字段红灯，再实现最小数据合同。

## Raw 边界

前置审计曾把 raw 列入完整 carrier 验收，但当前 B0 raw entity contract 是 48-leaf exact schema，
现有 authority capture不输出这7项。单独修改 Unity exporter/Parity contract会令既有权威capture结构失配，
因此本包明确保留raw schema不变；后续如需raw可见性，必须新建 authority+Unity 同步schema migration。

## 不变量与回滚

不连接state12/18 consumer、B5 producer或B8 knockout feed；不改Authority/Scene/内容/资源/网络。
回滚删除字段/test并恢复schema/checksum/parity，无数据迁移。完整验收见Task Contract。

## 当前实施与证据

- test-first导入后得到21个预期`CS1061`，全部指向7个缺失carrier。
- runtime已加入authority默认值、full reset、input reset preservation与canonical copy。
- entity/full/checksum schema `7/11/14 -> 8/12/15`；checksum和parity纳入7字段。
- fresh compile error0；focused job `9e5dcb0201e84dc9aced9053ae77748f` 6/6，含4096次copy/checksum 0 B。
- related carrier/snapshot/checksum job `a0638fe62d7441c6b49e842d08c9bf8c` 60/60。
- 精确77-class NTSD28 broad job `3e7bd0a900794a46833a1e8dcb515d5e` 527/527；最终
  SelfCheck `2026-09-05T08:00:57Z` PASS，Scene/Console/Ledger通过。
- 误启动的超范围namespace job `4f2d0b7d668646e3bb9b8b0e82a7c3f9` 执行1600项后因既存版本字符串、
  GroundRoleNearest、OPoint、structure guard等范围外失败终止；这些文件未修改，不归因于本Change。
- carrier已闭合；state12/18 consumer、B5 producer、B8 event与raw同步schema migration仍后置。
