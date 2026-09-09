# NTSD28-B3-C02-C03-PRODUCTION-PLACEMENT-001 — producer/route生产位置接线

<!-- CHANGE-RECORD
id: NTSD28-B3-C02-C03-PRODUCTION-PLACEMENT-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C02C03ProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan C02/C03 in SimulationTickDriver28::step, EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / C02-SLOT-INTERLEAVED-PRODUCERS / C03-SECOND-ROUTE-SCAN / HUMAN-SAMPLE-BEFORE-C02 / TEST-FIRST-CS1061 / UNITY-COMPILE-0 / FOCUSED-8-OF-8-JOBS-61871255-34629874 / INPUT-AI-WORKER-RELATED-110-OF-110-JOB-2B0DBBB7 / SELFCHECK-OLD-FW-FLOW-01-EXPECTED-FAIL / SELFCHECK-2026-09-04T23-24-44-PASS / KNOWN-NEGATIVE-7-REVIEWED / REAL-PLAY-KIND0-TICKS-3-6-PASS / RESULT-SHA-C52DE7F5 / SCENE-SHA-0D74E174-UNCHANGED / CONSOLE-0 / NEXT-FIRST-DIFF-FRAME-MOTION-VS-COOLDOWN / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C02-C03-PRODUCTION-PLACED / REAL-PLAY-PASS / NEXT-FRAME-MOTION-VS-COOLDOWN`

## 改前事实

Unity full tick在C01后执行`Cooldown→HumanInput→CharacterInput→maintenance→FrameLogic`；其中CharacterInput
内部已有两遍character scan，non-character hit_Fa却在更晚的FrameLogic独立scan。Authority要求physical sample
先行、C02按slot交错producer/hit_Fa、C03第二遍route，然后才进入frame motion。

## 计划

按Task Contract先取得red，再新增production专用两遍入口并重排TickSystem；Cooldown内部职责留后续包。

## 验证

- test-first：新增production入口测试在旧代码上得到预期`CS1061`；actual phase断言同时先改为目标顺序。
- `SimulationWorld.NativeProducerSampleAndInputRouteAll`复用既有两遍character input框架，仅在production第一遍
  把non-character positive hit_Fa按slot交错执行；direct `CharacterInputAll`仍保持character-only。
- TickSystem把HumanInput与production CharacterInput移到Cooldown前，移除后续FrameLogic occurrence；full actual
  sequence从31变30，partial仍6且HumanInput在Cooldown前。同步与worker共享同一RunTick。
- Unity compile 0 error；focused `8/8` PASS（jobs`61871255dae14fdd94b1b457112cf5c6`、
  `3462987476374ee688fc38e3c8c01cae`）；input/AI/worker相关`110/110` PASS，job
  `2b0dbbb78e95472dbf5652aa5b685630`。
- 首次完整SelfCheck按预期命中旧`FW-FLOW-01`（旧断言要求Cooldown先于HumanInput）；更新为新权威后，
  `2026-09-04 23:24:44 +08:00`完整SelfCheck PASS。7条既有negative rest-binding日志复核，Console最终0。
- 真实`NTSD_Battle`同步Host kind0/C01 probe tick3～6 PASS；result SHA
  `C52DE7F522C89D611F61F5E62045957C84E2970355EF5658EE07DF0E78F68AE7`。live/frozen ages与原证据一致，
  RNG/gate、presentation只读、warm alloc、object/slot/pool/RNG/stats/sound/pause cleanup全部PASS。
- Play已退出；Scene SHA仍`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`，
  LastWriteTime仍`2026-09-04 21:12:45 +08:00`，未被probe保存。

## 未关闭边界

- Cooldown内部`ARest`递减与`AttackExempt`clear仍在错误位置；本包只把整个phase推到C03后。
- production producer scan仍使用deferred unregister；同/低slot立即复用差异归B7。
- 下一actual首差为`Authority CoreFrameMotion / Unity Cooldown`；下一包必须拆分Cooldown职责，不能把它整体
  简单移到C25尾部。
