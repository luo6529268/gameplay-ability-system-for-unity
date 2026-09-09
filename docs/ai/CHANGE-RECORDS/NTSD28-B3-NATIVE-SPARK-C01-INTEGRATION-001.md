# NTSD28-B3-NATIVE-SPARK-C01-INTEGRATION-001 — Spark C01 production single-writer接线

<!-- CHANGE-RECORD
id: NTSD28-B3-NATIVE-SPARK-C01-INTEGRATION-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/BattleSimulationWorkerBoundary.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeSparkC01IntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitRecordWritebackPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs
authority: NTSD 2.8-Logan C00 input phase then C01 advance_native_sparks before producer/hit; completed snapshot after new-hit base age; verified native spark core.
evidence: TEST-FIRST-RED-CS0117-4 / PRODUCTION-C01-SINGLE-WRITER / PRESENTATION-ACK-READ-ONLY / NO-PRODUCTION-LEGACY-FINALIZE-CALLER / FINAL-FOCUSED-38-OF-38-PASS-JOB-6DD323A0 / RELATED-98-OF-98-PASS / SELFCHECK-2026-09-04T22-14-51-PASS / KNOWN-NEGATIVE-7-REVIEWED / CONSOLE-0 / REAL-PLAY-KIND0-TICKS-3-6-PASS / PUBLISHED-AGES-0-1_0-2_1_0 / NO-PUBLICATION-AGES-3_2_1_0 / WARM-ALLOC-DELTAS-0-0 / CLEANUP-PASS / SCENE-PREPOST-SHA-0D74E174 / RESULT-SHA-397AD7D9 / NEXT-FIRST-DIFF-CORE-PRODUCER-SCAN-VS-COOLDOWN / B9-RESOURCE-MAPPING-EXCLUDED / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / PRODUCTION-C01-SINGLE-WRITER / PRESENTATION-READ-ONLY / REAL-PLAY-PASS / NEXT-FIRST-DIFF-CORE-PRODUCER-SCAN-VS-COOLDOWN`

## 改前事实

- exact native lifecycle primitive存在但无production caller。
- TickSystem在U25 capture后调用presentation finalize或no-publication writer；logical advance位置错误并受资源影响。
- actual phase sequence为normal30/partial5；共同C00后的首差是expected spark / actual Cooldown。
- 首轮实现后的全调用点审计确认，正式同步LateUpdate、worker consumed、worker stop和production stress
  snapshot仍调用旧finalize。它们会在C01后再次改写live HitRecord；必须迁移到只读publication acknowledgement。

## 计划

- test-first覆盖resource-independent old record、新record same-tick base、terminal pop、sync/worker与sequence。
- 在BattleFlow/input phase后添加C01 phase与world loop。
- 从RenderDispatch移除两个production logical writeback调用，保留capture/publication。
- 新增只读publication acknowledgement，并迁移Host/worker/stress正式调用者。
- 保留旧public/internal methods及语义作为compatibility/test only。
- 把旧R8 writeback Play probe更新为B3 C01 lifecycle/playback witness。

## 中间验证记录

- test-first：新增phase引用得到4个预期`CS0117`。
- 首次实现后，修正测试fixture的非空DAT catalog前置条件；随后focused `37/37` PASS，job
  `a60bb4fbb81946d2b6d46703722730d3`。
- 扩大相关测试`97/97` PASS，job `ccedf6056f1144c28218285afa50c019`。
- `rg`全调用点审计发现Host/worker/stress仍是旧finalize正式调用者，因此本Record保持`IN_PROGRESS`，
  扩展scope后继续实现；以上通过结果不是本包最终关闭证据。
- Host/worker/stress迁移后重新编译0 error；focused `38/38` PASS，job
  `3b423688fbd6432882210472a6a4bd5e`；相关回归分两组`43/43`与`55/55` PASS，jobs
  `3c50da5bf2c84b5abccbf7079d06c574`、`f23f85ec7edc44f6b14a541f761f8540`，合计`98/98`。
- 首轮完整SelfCheck于`2026-09-04 21:53:28 +08:00`按预期FAIL：`R6-PRES-005`仍断言
  no-publication RenderDispatch按resource catalog推进。新C01在resource-independent逻辑阶段把
  `[0,5,38,39]`推进为`[1,6,39]`，因此这是旧测试合同，不是产品回归；在修改SelfCheck前扩展scope。

## 实际实现

- `NTSDBattleTickSystem`在`BattleFlow`后、`Cooldown`前执行新增`NativeSparkAdvance` occurrence；
  `SimulationWorld.AdvanceNativeSparkLifecycleAll()`按active runtime slot调用上游native primitive。
- `RenderDispatch`不再在build/no-build任一路径改写HitRecord；同tick新hit保持base ID。
- `BattlePresentationCoordinator.AcknowledgePublishedHitRecordCycle()`只claim当前cycle并释放publication binding，
  不读取或修改entity HitRecord。同步LateUpdate、worker consumed/stop与stress final snapshot均已迁移。
- 全仓调用点审计确认，旧`FinalizePublishedHitRecordCycle`与
  `AdvanceHitRecordsWithoutPublication`除定义外只剩SelfCheck/Editor兼容测试调用，不再进入正式Host路径。
- actual sequence更新为full `31`、input-clear partial `6`；C01首差关闭，下一首差为
  `expected CoreProducerSampleScan / actual Cooldown`。

## 最终验证

- 编译：最终脚本刷新后Unity Console `0 error`。
- EditMode：最终focused `38/38` PASS，job `6dd323a0defa421c93047a7fc742a608`；
  production迁移后的相关测试合计`98/98` PASS（`43/43` job
  `3c50da5bf2c84b5abccbf7079d06c574` + `55/55` job
  `f23f85ec7edc44f6b14a541f761f8540`）。
- 完整SelfCheck：`2026-09-04 22:14:51 +08:00` PASS；7条既有negative rest-binding日志逐条复核，
  随后Console清为0。
- 真实Play：`Temp/NTSD28_B3_NativeSparkC01.result.json` PASS，result SHA-256
  `397AD7D9BAB4FA1D63A1ECFD39C347672FC8573974B92802CAB36C83A281D023`。同步正式Host tick3～6：
  published live/frozen ages依次`[0]`、`[1,0]`、`[2,1,0]`；no-publication tick为
  `[3,2,1,0]`。Late/materialization均不再推进；物化火花命令为`1/2/3`。
- Play中的每tick RNG为隔离fixture的2次kind0 anchor draw加用户例外random-weapon gate 1次；四tick
  gate=`6/2/189/160`，状态/call count与anchor全匹配。warm tick/presentation allocation violation
  delta=`0/0`；对象、slot、pool、RNG、stats、sounds、pause和presentation cleanup全部恢复。
- Play已退出；本包Play前后Scene SHA-256均为
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`，未保存Scene。

## 未关闭边界

- B9继续负责spark资源映射/完整可观察素材策略；本包只关闭C01逻辑生命周期与presentation只读边界。
- Play证书覆盖同步Host；worker与buildPresentation true/false共享入口由focused/related测试覆盖。
- B3整体仍未完成；下一包从producer/sample scan与Cooldown位置首差继续。
- 本包完成后的只读审计发现指定root物理identity已漂移；本包Unity证据与先前锁定C01合同保留，但若
  `B1E13AE1...9033`被用户晋升，必须按`GOVERNANCE-NTSD28-AUTHORITY-IDENTITY-DRIFT-001`
  对新正式身份重新核验，不自动继承为新权威证书。

## 2026-09-04 Bug修复版重新核验

用户已由`GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002`晋升`B1E13AE1...9033`。当前source
`simulation_tick_driver.cpp:388-396`仍保持C00后、producer前推进spark的同一合同；57项contract索引6～8也保持
input phase→spark→producer。B3 contract/spark相关38/38与完整SelfCheck通过；随后C02/C03 production placement
完成后再次运行真实kind0 Play tick3～6，年龄/RNG/presentation/cleanup全部保持PASS，result SHA
`C52DE7F522C89D611F61F5E62045957C84E2970355EF5658EE07DF0E78F68AE7`，Scene SHA仍`0D74E174...D77`。
因此C01在新身份下重新核验通过；B9资源映射仍未关闭。

## 回滚

见Task Contract。
