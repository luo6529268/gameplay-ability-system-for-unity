# NTSD28-B5-KIND8-ATOMIC-PRODUCTION-INTEGRATION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-KIND8-ATOMIC-PRODUCTION-INTEGRATION-001
status: VERIFIED
change-kind: TEST_FIRST_ATOMIC_PRODUCTION_INTEGRATION
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleKind8ControlRelationWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind8AtomicProductionIntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: NTSD 2.8-Logan hit_candidates.cpp classify_kind8_candidate, battle_world.cpp resolve_special_relation_hit lines 5681-5745 and SimulationTickDriver relation branch; EXE B1E13AE1, closure 39DDDA15.
evidence: red db2f1ce104564c0a8cd6c28d0db55821 14/14; focused 0e3093144a2849af846fa06e1957a9d2 14/14; HitPlan ece0b0bad3e04890bc0304be2073539a 184/184; collision 5adea7dd6bb240018facab4d6c418872 258/258; B5 bc78b6630b0442ff952b58b78bcd801a 420/420; clean broad 14cc72613acb477c9c48e09f80e33b86 885/885; SelfCheck PASS 2026-09-06T04:33:16Z; Console 7 intentional negative-path errors; scene D4266C6D...583B unchanged; ledger PASS.
-->

> 状态：`VERIFIED / KIND8_CONTROL_RELATION_ALIGNED`

## 风险与回滚

风险为candidate/consumer半接、整数坐标误写、zero/sentinel错误或HitPlan shadow漂移。必须原子接三层并用完整
矩阵验证。回滚仅移除新writer和本包四处接线/测试，不回退pure resolver。

## 实际修改

- candidate filter只保留kind3 type0硬门，并对kind8调用pure resolver；同入口也覆盖runtime defensive filter。
- shared runner保持原observation/preprocess结构，只把kind8最终dispatch替换为唯一control writer。
- writer防御性复核selector，按非0/非0/非999独立写heal/current PP/action，按dvy同步precise坐标且不写int。
- HitPlan执行同一复核和同值投影；既有snapshot/DifferenceMask字段足够，无schema扩张。

## 验证

- 有效red `db2f1ce104564c0a8cd6c28d0db55821`：14/14按预期失败。
- 首次green编译发现`BruteForceSceneQuery`缺`NTSD.Simulation.Ecs` using，已补；该轮旧assembly测试结果不计green。
- focused green `0e3093144a2849af846fa06e1957a9d2` 14/14。首次HitPlan扩大为183/184；唯一失败是旧测试仍要求kind8立即写int坐标，而actual/HitPlan DifferenceMask已经为0；按Authority改为断言int保持。
- green与扩大验证待执行。
- final focused `0e3093144a2849af846fa06e1957a9d2` 14/14、HitPlan
  `ece0b0bad3e04890bc0304be2073539a` 184/184、collision related
  `5adea7dd6bb240018facab4d6c418872` 258/258通过。
- B5 `bc78b6630b0442ff952b58b78bcd801a` 420/420通过。
- 首次NTSD28 broad `fb85884b07724dd182fa902c23ba859f`只有1项失败，原因是运行中MCP状态轮询向
  Console注入disposed NetworkStream Error，项目断言无失败；清空Console并全程不轮询后，clean broad
  `14cc72613acb477c9c48e09f80e33b86` 885/885通过。
- 04:33:16Z SelfCheck PASS；Console仅7条既有rest-binding负路径日志；Scene SHA
  `D4266C6D...583B`、length 205625、mtime不变；scoped diff与Ledger PASS。
