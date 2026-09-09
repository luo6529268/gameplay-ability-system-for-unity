# NTSD28-B5-REDUCED-STATE2000-AWAY-DAMPING-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B5-REDUCED-STATE2000-AWAY-DAMPING-PRODUCTION-001
status: VERIFIED
change-kind: BATTLE_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ReducedState2000AwayDampingEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan apply_native_reduced_attacker_post_hit state2000 branch; formal OID150 w/1.dat frames0-6/21; EXE B1E13AE1, closure 39DDDA15.
evidence: RED 6e15a70e34c54894bc92fafdda7851a3 10 expected failures; focused 7a65b48c 14/14, HitPlan 7d7467f2 185/185, B5 315b74d7 813/813, NTSD28 1159/1160 plus isolated contaminated test e8e8b89c 1/1, SelfCheck PASS, Console0, Scene unchanged.
-->

> 状态：`VERIFIED / REDUCED_STATE2000_AWAY_DAMPING_ALIGNED`

## Authority与原状

Authority按double X严格分左右，只在Vx向远离方向（含0）时将X/Z除2.5；equal X无分支。Unity actual
用XInt且只在movingToward时衰减，造成同整数桶漏判与方向反转；HitPlan同样使用XInt和toward极性。

## 计划

新增无分配共享predicate，actual使用Runtime.X，HitPlan使用snapshot double X；focused先RED覆盖六方向、equal、
fractional同整数桶和actual/ShadowCompare/DataOriented，再做最小替换。

## 验证

RED：Unity EditMode job `6e15a70e34c54894bc92fafdda7851a3`，14项完成、10项按预期失败：

- actual：同整数桶away未衰减（4.0而非1.6），明确分离时toward被错误衰减（-1.6而非-4.0）；
- HitPlan：away未衰减（4.0而非1.6）；
- 7组严格左右/equal/零速predicate因共享入口尚不存在而失败。

## 实现

- `BattleDamageWriter.ShouldDampenNativeReducedState2000`以逻辑double X实现严格左右与away/zero语义；
- actual reduced writer和HitPlan projection共享该predicate，并按Authority使用`/ 2.5`；
- 新focused夹具覆盖左右、零速、equal、同XInt小数桶，以及actual/HitPlan away/toward；
- 旧HitPlan与SelfCheck反向断言已按当前Authority更正，不改变其他路径。

首次HitPlan broad job `4bfddab0debe48efa89f8b0055bc1bff` 为184/185；唯一失败是旧
`ShadowCompare_AlternateDamageAppliesAttackerStateTailBeforeHitRecord` 对state2000 toward案例仍断言
旧错误衰减`8/6 -> 3.2/2.4`。该夹具已在修改前纳入允许路径，按Authority更新为保持`8/6`。

首次SelfCheck结果`2026-09-06T19:27:05Z / FAIL`同样只暴露两条旧反向文字/期望：toward要求
`*0.4`、away要求保持。`BattleRuntimeSelfCheck.cs`已在Task起始允许路径中，仅反转这两个state2000
断言以匹配当前Authority，随后必须重跑。

## 最终验证

- focused：`7a65b48ca0f941228c34f6c99f0432fa`，14/14；
- HitPlan：`7d7467f2b3ba4772ba42a66881e099a2`，185/185；
- B5：`315b74d74f3243779da85646f8bec6e3`，813/813；
- NTSD28 broad：`c244b67d8d7d41319633c9732923f9f0`，完成1160项，其中1159通过；唯一失败为
  MCP断线`NetworkStream disposed`日志污染，非断言差异；清理Console后同一测试
  `e8e8b89cd81c44d39cd5b0432d9ca819`独立1/1通过；
- request-file SelfCheck：`2026-09-06T19:29:04Z / PASS`；
- Unity Console清理后error 0；活动Scene `NTSD_Battle`、dirty=false、rootCount=13；Scene SHA-256
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`不变；
- `git diff --check`通过（仅既有LF/CRLF提示）；未进入Play，未保存Scene。

## 回滚边界

如需回滚，只撤销共享predicate、actual/HitPlan两个调用点、本focused夹具和两处旧反向断言修正；
不得回退其他B5包或用户工作树。
