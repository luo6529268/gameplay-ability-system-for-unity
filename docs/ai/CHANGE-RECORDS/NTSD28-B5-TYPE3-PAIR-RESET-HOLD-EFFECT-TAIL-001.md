# NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001 — type3 pair reset, hold and effect tail

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-PAIR-RESET-HOLD-EFFECT-TAIL-001
status: VERIFIED
change-kind: TEST_FIRST_AUTHORITY_BEHAVIOR_PORT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3PairResetHoldEffectTailEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan reset_native_matched_projectile_pair + release_native_attacker_motion_hold and post-type3 common effect order; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-6_EXPECTED_FAIL_1_PASS / FOCUSED-7-7 / HITPLAN-183-183 / B5-HITPLAN-374-374 / NTSD28-749-749 / SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / CLOSED`

## Authority与Unity原状

Authority pair reset按action-latch frame hit_Uj且只清pending total，hold release无条件后置，type3目标没有
legacy effect5000/6000/23 tail。Unity固定20、清Runtime XYZ、hold只在pair内并保留旧effect helper。

## 实际改动

- `BattleDamageWriter.ApplyKind0Type3Tail`在matching 3005/3006 pair中分别按双方action-latch帧
  `hit_Uj`选择动作（0回退20），只清`KnockbackVx/Vy/Vz`与`AttackingCounter`，保留runtime motion。
- `ReleaseNativeType3AttackerMotionHold`移到每次type3 continuation的pair尝试之后；ordinary正hold取负，
  negative link先镜像给active parent再取负，parent缺失时不改attacker。
- 删除type3专属`ApplyType3EffectTail`/burning helper；公共effect8..16 override与type0 direct tail保持。
- HitPlan同步相同pair/latch/pending-only/hold/effect合同；既有HitPlan与SelfCheck断言按当前权威纠正。

## 验收状态

- 红灯：`990bb3b11b804b679656766dad07fc37`，7项中6 expected fail / 1 pass。
- focused：`a975c05b325d4189b115b316c3dbfc45`，7/7 PASS。
- HitPlan：`8fccff2ed7be4586b0ba456ccf0ecd2b`，183/183 PASS。
- B5+HitPlan：`b4926e1c0a064c40b7ef24da4d4f61cb`，374/374 PASS。
- exact 102个`NTSD28*`类：`613297726cff432cbb4c99b4ec9b24c4`，749/749 PASS。
- final DLL：Runtime `2026-09-05T20:12:32.8801555Z`；Editor
  `2026-09-05T20:03:01.6622092Z`；filtered compile error=0。
- SelfCheck先因陈旧effect6007/frame7断言失败；按Other attacker的`hit_Uj`零值回退20纠正后，
  `2026-09-05T20:14:00Z` PASS。7条error均为既有故意拒绝路径。
- Scene SHA/length/mtime未变；Ledger `272 records / 235 governed code files` PASS。

## 未扩大结论

本Record关闭type3 pair-reset/hold/effect-tail包，不单独证明真实Play或整个B5；type3 family是否退出由后续
独立退出审计裁决。
