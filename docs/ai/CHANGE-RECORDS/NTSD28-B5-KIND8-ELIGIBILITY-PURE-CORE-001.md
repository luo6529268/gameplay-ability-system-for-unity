# NTSD28-B5-KIND8-ELIGIBILITY-PURE-CORE-001

<!-- CHANGE-RECORD
id: NTSD28-B5-KIND8-ELIGIBILITY-PURE-CORE-001
status: VERIFIED
change-kind: TEST_FIRST_PURE_CORE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleKind8EligibilityResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind8EligibilityPureCoreEditorTests.cs
authority: NTSD 2.8-Logan hit_candidates.cpp HitCandidateBuilder28::classify_kind8_candidate and battle_world.cpp duplicate consume gate; EXE B1E13AE1, closure 39DDDA15.
evidence: red d421354c7f7847048c822b783e8e92e6 35 total/at least25 expected failures; focused de1df6b0cf61454d999e804ef6b14496 35/35; B5 766babdb735e4b8fb141f77101184467 406/406; broad c4ec009b207644df8f34ac9530742208 871/871; SelfCheck PASS 2026-09-06T03:59:36Z; clean Console only 7 intentional negative-path errors; scene D4266C6D...583B unchanged; ledger PASS.
-->

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`

## 合同

只新增纯resolver与测试，不接`BruteForceSceneQuery`、actual consumer或HitPlan。回滚为移除两个新增文件和本包文档。

## 实际修改

- target selector按`<7 exact -> 7 weapon group -> 8 unrestricted -> reject`顺序实现；负selector自然不匹配。
- relation先拒绝`<0/>4`，0无条件通过，1/2判same/different group，3叠加same owner，4再叠加owner=mode。
- 使用只含值字段的readonly result，无集合、委托、LINQ或异常路径；production尚未接线。

## 验证

- red `d421354c7f7847048c822b783e8e92e6`：35项中失败列表达到25项工具上限，10项拒绝/zero-allocation保护通过。
- focused green `de1df6b0cf61454d999e804ef6b14496` 35/35通过；4096次warm allocation断言包含在内。
- B5 `766babdb735e4b8fb141f77101184467` 406/406、NTSD28 broad
  `c4ec009b207644df8f34ac9530742208` 871/871通过。
- 首次SelfCheck后Console含1条轮询连接释放产生的MCP工具日志；清空后重跑，03:59:36Z SelfCheck PASS，
  Console仅7条既有rest-binding负路径日志。Scene `D4266C6D...583B` unchanged，Ledger PASS。
- candidate、actual与HitPlan仍未接；下一production owner/write-surface audit。
