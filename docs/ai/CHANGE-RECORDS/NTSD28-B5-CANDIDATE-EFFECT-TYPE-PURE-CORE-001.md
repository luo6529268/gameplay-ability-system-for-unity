# NTSD28-B5-CANDIDATE-EFFECT-TYPE-PURE-CORE-001

<!-- CHANGE-RECORD
id: NTSD28-B5-CANDIDATE-EFFECT-TYPE-PURE-CORE-001
status: VERIFIED
change-kind: TEST_FIRST_PURE_CORE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitCandidateEffectTypeResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5CandidateEffectTypePureCoreEditorTests.cs
authority: NTSD 2.8-Logan hit_candidates.cpp HitCandidateBuilder28::interaction_effect_accepts_object_type; EXE B1E13AE1, closure 39DDDA15.
evidence: red 08b53ad3074e40888113bc7dd5a66d21 10 expected failures/26; focused 0fa494c910924192b380ac65cf049678 26/26; B5 3a1b8ed4169a4540a42c8938c5a33863 446/446; broad 0f662b4bd4b4479aac5e86f80fec39e3 911/911; SelfCheck PASS 2026-09-06T04:53:26Z; Console 7 intentional negative-path errors; scene D4266C6D...583B unchanged; ledger PASS.
-->

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`

仅新增pure resolver与tests；不复用语义不同的effect-action override helper。回滚为移除新增文件与本包文档。

## 实际修改与验证

- 独立switch只限制13..16；effect16显式含type3、排除0/5；default unrestricted。
- red `08b53ad3074e40888113bc7dd5a66d21`为10项拒绝矩阵失败/26，16项接受与allocation保护通过。
- focused `0fa494c910924192b380ac65cf049678` 26/26、B5
  `3a1b8ed4169a4540a42c8938c5a33863` 446/446、broad
  `0f662b4bd4b4479aac5e86f80fec39e3` 911/911通过。
- 04:53:26Z SelfCheck PASS；Console仅7条既有rest-binding负路径日志；Scene unchanged，Ledger PASS。
- production未接；下一candidate/runtime defensive integration。
