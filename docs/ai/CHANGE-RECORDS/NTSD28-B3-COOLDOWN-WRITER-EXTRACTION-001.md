# NTSD28-B3-COOLDOWN-WRITER-EXTRACTION-001 — Cooldown writer归位

<!-- CHANGE-RECORD
id: NTSD28-B3-COOLDOWN-WRITER-EXTRACTION-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CooldownWriterExtractionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeSparkC01IntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan C11 attacker-rest clear prelude and C25j per-slot decrement after frame; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / UNITY-FIRST-DIFF-COOLDOWN / NATIVE-C11-CLEAR / NATIVE-C25J-DECREMENT / ATTACKEXEMPT-CANONICAL / ITRREST-AREST-COMPAT-MIRROR / TEST-FIRST-7-OF-10-EXPECTED-FAIL / UNITY-COMPILE-0 / FOCUSED-10-OF-10-JOBS-0115E42D-0E16BC6A / RELATED-136-OF-136-JOB-56B83880 / LEGACY-COOLDOWN-COMPAT-2-FAIL-FIXED / SELFCHECK-THREE-OLD-CONTRACTS-CORRECTED / SELFCHECK-2026-09-04T23-48-17-PASS / REAL-PLAY-KIND0-TICKS-3-6-PASS / RESULT-SHA-0945501F / SCENE-SHA-0D74E174-UNCHANGED / PLAY-EXITED / CONSOLE-0 / NEXT-FIRST-DIFF-FRAME-MOTION-VS-RUNTIME-MAINTENANCE / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / EARLY-COOLDOWN-REMOVED / C11-C25J-OWNERS / REAL-PLAY-PASS`

## 改前事实

生产Tick在C03后调用`BattleEcsCooldownPass`，提前递减`RuntimeRestStore.ARest`并清`AttackExempt`；
`AttackExempt`又在真正frame tick内按native hold/type gate递减。两个Unity字段代表同一authority attacker_rest，
但发生在不同时间，武器与角色candidate gate可能看到不同值。

## 计划

按Task Contract先红灯，再让candidate prelude与late frame tick成为两个正式owner；保留旧Cooldown实现为非生产兼容 seam。

## 验证

- test-first：10项focused中7项按预期失败：full/partial occurrence仍多1、下一phase仍Cooldown、C11双字段
  clear/sync缺失、NeedClear仍错误减rest、late frame后mirror未同步。
- 移除production `RunBattleEcsCooldownPass` occurrence；保留该pass为direct compatibility/diagnostic seam。
- `CaptureCollisionFrameSnapshotsAll`进入角色几何前先执行current-frame rest prelude；无Itr/state1001 parent
  WPoint停止attacking时同时清canonical `AttackExempt`和mirror `ItrRest.Arest`，否则把mirror同步到canonical。
- late entity frame tick前后只在canonical/mirror原本相等时提交新的canonical值到mirror；这让普通hit rest按
  frame gate递减，同时不覆盖held step12只写mirror的独立兼容状态。
- full actual occurrence `29`、input-clear partial `5`；下一首差为`CoreFrameMotion / RuntimeMaintenance`。
- compile0；focused最终`10/10` PASS（jobs`0115e42d...`、`0e16bc6a...`）。相关cooldown/collision/
  late lifecycle/worker `136/136` PASS，final job`56b83880784e4e6080606c2312eafeae`。期间旧direct
  Cooldown兼容测试2项失败后，通过区分`synchronizeMirror`生产/diagnostic调用修复。
- 完整SelfCheck依次暴露并修正三项旧合同：held step12 mirror不得被覆盖；NeedClear early return不进rest；
  results-active current frame无Itr时C11应清0而非17→16。最终`2026-09-04 23:48:17 +08:00` PASS；
  7条既有negative rest-binding日志已复核。
- 真实NTSD_Battle kind0 tick3～6 PASS，result SHA
  `0945501F18D4B0A763496C501DAC0D4588796BB7DC8E65EE66EF0BB8A9466A7A`；age/RNG/drop gate/
  presentation/warm alloc/cleanup均通过。Play已退出；Scene SHA`0D74E174...D77`与mtime不变；Console0。

## 未关闭边界

- `BattleEcsCooldownPass`名称和实现保留给旧测试/diagnostic，但无production caller；后续清理不得误当规则owner。
- current Unity仍在C03后运行`Oid5152RuntimeMaintenance`，而Authority下一项为C04 frame motion；这是新的首差。
- C25完整16步nested tail尚未建立；本包只闭合attacker-rest两个writer的正确相对时点。
