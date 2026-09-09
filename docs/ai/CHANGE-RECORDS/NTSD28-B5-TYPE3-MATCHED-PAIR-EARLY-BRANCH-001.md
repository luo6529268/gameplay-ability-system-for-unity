# NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001 — type3 matching pair early return

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-MATCHED-PAIR-EARLY-BRANCH-001
status: VERIFIED
change-kind: TEST_FIRST_AUTHORITY_BEHAVIOR_PORT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3MatchedPairEarlyBranchEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan battle_world.cpp 6615-6651 before ordinary unarmored damage; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-3-EXPECTED-FAIL-1-PASS / FOCUSED-4-4 / HITPLAN-183-183 / B5-HITPLAN-378-378 / NTSD28-753-753 / SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / CLOSED`

## Authority与Unity原状

Authority initial matching 3005/3006 non-character pair在普通damage入口前只应用standard rests、pair reset、hold
release并立即返回。Unity `ApplySpecialAttackDamage`当前先写sound/vital/status/object-hurt，再于tail reset，错误产生
HP、统计、状态、音频与record副作用。

## 实际改动

- `ApplySpecialAttackDamage`在任何sound/vital/status/join/object-hurt之前检测target type3与双方initial
  matching 3005/3006；命中后只提交现有standard arest/vrest、双方latch-hit_Uj reset和hold release并return。
- HitPlan把同一gate前置到locked transform和旧identity投影之前；early projection不再写vital、fall、hit count、
  hit-state、sound、RNG或hit record。
- 既有state-sync shadow用例更正为early-return合同；新增actual用例覆盖3005、3006、negative parent和near-miss。

## 验收状态

- 红灯：`9b171dd54db34183b46465dd02deef93`，3 expected fail / 1 pass。
- focused：`9a9361464f7042c1932912543e8ed412`，4/4 PASS。
- HitPlan：`216fb596c9c64bd9868ba955f0b3d0f2`，183/183 PASS。
- B5+HitPlan：`c86abd44e29a4ebba07232757913e391`，378/378 PASS。
- exact 103个`NTSD28*`类：`aebe5b7a2af5432391cb76623d7eb5da`，753/753 PASS。
- final DLL：Runtime `2026-09-05T20:31:37.9028806Z`；Editor
  `2026-09-05T20:31:39.2913144Z`；compile error=0。
- SelfCheck `2026-09-05T20:40:26Z` PASS；7条error均为既有故意拒绝路径。
- Scene SHA/length/mtime未变；Ledger `274 records / 236 governed code files` PASS。

## 未扩大结论

本包不宣称standard rest的全局timing-reduction/recover/definition-effect数值已经完成；这里只复用其既有owner并
闭合Type3-specific early routing。Type3 family仍需独立退出审计。
