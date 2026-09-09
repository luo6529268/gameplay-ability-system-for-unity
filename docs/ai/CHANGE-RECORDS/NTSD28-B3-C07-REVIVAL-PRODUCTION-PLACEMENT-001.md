# NTSD28-B3-C07-REVIVAL-PRODUCTION-PLACEMENT-001 — C07复活production placement

<!-- CHANGE-RECORD
id: NTSD28-B3-C07-REVIVAL-PRODUCTION-PLACEMENT-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationPassPipeline.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C07 lines611-673 and BattleWorld28 advance_native_revivals; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-C07-AFTER-C06-BEFORE-C08-GEOMETRY / EXISTING-UNITY-RESPAWN-OWNER-REUSED / BEHAVIOR-DIFFS-DEFERRED-B4-B7-B9 / RED-1-REVIVAL-PHASE / UNITY-COMPILE-0 / FOCUSED-3-OF-3-JOB-B834F881 / C04-C07-ACTUAL-25-OF-25-JOB-6C8E2FEB / W05-WORKER-28-OF-28-JOB-50A4F99E / PREINTERACTION-6-REFERENCE-COMPARER-FAILURES-OUT-OF-SCOPE / SELFCHECK-2026-09-05T01-59-07-PASS / TARGETED-PLAY-PASS / SPAWN-COUNT-1 / HIGH-SLOT-SERIAL-COUNT-1 / NO-C06-RERUN / RESULT-SHA-185F9CBD / CLEANUP-PASS / SCENE-SHA-0D74E174-UNCHANGED / PLAY-EXITED / CONSOLE-0 / FULL-31-PARTIAL-4 / NEXT-C08 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C07-PLACEMENT / HIGH-SLOT-DOWNSTREAM-VISIBLE / BEHAVIOR-PENDING-B4-B7-B9`

## 改前事实

- Authority在所有slot C06完成后统一扫描dead state14/render-phase 1..4，再进入C08/C09/geometry；continuation可
  立即生成OID998，高slot对全部下游可见。
- Unity现有`BattleRespawnModule`在`SerialTickAll`之后运行，phase名为`DeathCleanup`；当前C06抽离后，character
  physics已不在serial，但production结构仍把serial remainder放在C07前。
- 现有Unity算法含已知后续差异：floor硬编码/来源、terminal primary策略、continuation controller/group/visual、
  random gate/offset与OID998细节。它们不在本placement包修改。

## 计划

- 先写placement/single-owner/high-slot visibility red tests。
- 用`Revival`替换旧`DeathCleanup`phase并把现有owner移到C06后；不改模块内部算法。
- 运行相关回归、SelfCheck与真实复活Play，记录下一首差。

## 实际改动

- `BattleTickPhase.DeathCleanup`替换为`Revival`，production调用从FrameAdvance后移到完整C06后、FrameAdvance前；
  occurrence总数不变。
- 现有`BattleRespawnModule`、world/pipeline direct seam与全部复活算法未改。
- C06相邻phase测试更新为C06→C07→serial；新增C07 single-owner/high-slot visibility focused与Play probe。

## 验证

- red1→compile0；focused3 job`b834f881579c4c8d8cb068ceb1b88154`。
- C04～C07/actual首轮24/25仅C06旧相邻断言失败；更新后25/25 job
  `6c8e2febf98b441da0b844568a5570f2`。
- 43项扩大回归的6项失败均为既有PreInteraction proxy引用比较器；W05+worker clean rerun 28/28 job
  `50a4f99ebeac4b3797ef9327691f92d2`。未修改范围外比较器。
- SelfCheck `2026-09-05 01:59:07 +08:00` PASS。
- targeted Play证明production C07一次、slot51同tick serial可见、C06不倒流、cleanup PASS；SHA
  `185F9CBD01B50A0DBBF240E0690BE1318D0C6B7AC0B5FEEFE875D2406EBBF6B1`。Scene unchanged、
  Play exited、Console0。

## 未关闭边界

- 本Record只验证placement与visibility，不证明现有Unity revive gate/floor/RNG/controller/group/visual/OID998
  行为等价；这些差异继续归B4/B7/B9。
- 下一结构首差为C08 stage-depth clamp对serial remainder。
