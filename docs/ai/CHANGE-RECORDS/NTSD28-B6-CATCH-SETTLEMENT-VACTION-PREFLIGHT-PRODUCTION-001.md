# NTSD28-B6-CATCH-SETTLEMENT-VACTION-PREFLIGHT-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-CATCH-SETTLEMENT-VACTION-PREFLIGHT-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_CATCH_SETTLEMENT_VACTION_PREFLIGHT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchSettlementVactionPreflightProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan BattleWorld28::settle_catch_relations battle_world.cpp 6033..6089; native_relation_action 83..89; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-4-FAIL-4-PASS-OF-8-JOB-EEBCA1 / FOCUSED-8-OF-8 / B6-CATEGORY-65-OF-65 / NTSD28-191-OF-191 / HITPLAN-185-OF-185 / PREINTERACTION-15-OF-15 / TARGETED-PLAY-8-CASES / BUILDS-0-ERROR / SELFCHECK-BLOCKED-UNRELATED-HELD-ACCOUNTING / CONSOLE-0-ERROR / SCENE-UNCHANGED / LEDGER-PASS-426-RECORDS-362-CODE-FILES / ACTUAL-PREFLIGHT-EXACT
-->

> 状态：`VERIFIED / RED_4_FAIL_4_PASS_OF_8 / FOCUSED_8_OF_8 / B6_CATEGORY_65_OF_65 / NTSD28_191_OF_191 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / TARGETED_PLAY_8_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / ACTUAL_PREFLIGHT_EXACT`

## 改前事实

- Authority在hurtable条件命中时总是按signed/zero vaction提交caught action，随后重新解析新frame及首kind2
  CPoint；失败立即结束当前slot，保留已提交action/facing但不执行injury/placement/cover。
- Unity `SyncCaughtByCpoint()`只在`Vaction != 0`时raw写action，负值依赖事后`Frame.N < 0`补偿，且写后
  没有frame/kind2二次preflight，因此missing/non-kind2目标仍继续held injury与position/cover。
- current OID417 tree有40个静态invalid post-vaction pairs；release OID555 tree有134个。是否能由当前玩家路径
  自然触发仍需定向运行时证据，不能由静态join冒充。

## 预定实现

- 在actual settlement中引入不跳过0的signed relation action提交。
- action提交后重新读取`victim.Frame.D`并验证首CPoint kind2；失败立即return。
- 以独立focused测试覆盖missing/no-cpoint/non-kind2、negative、zero、valid continuation、hurtable false及0B；
  SelfCheck只增加本包边界，不修held accounting。

## 验证记录

- Unity EditMode RED job `eebca150aafe4f69b1dd1b97c44bef50`：`4 fail / 4 pass / 8 total`。
  missing frame517、frame133无CPoint、frame134 kind1三例均错误把HP 400扣到370；zero vaction错误保留
  action132。negative/valid/hurtable-not-taken/0B四项保持绿，精确命中预定差异。

## 实际实现

- `SyncCaughtByCpoint()`的hurtable分支不再用`Vaction != 0`跳过零值，而是无条件提交signed/zero
  relation action：负值先翻转victim facing并取绝对action，零值明确写action0。
- action提交后立即重新读取`victim.Frame.D`及其第一个CPoint；缺frame/EmptyFrame、无CPoint或kind非2
  均立即return，因而fence住injury/resource/accounting、frame counters、hold timers、position与cover尾。
- hurtable条件未命中时保持原先已经验证的当前kind2 frame，不读取未使用的vaction目标；正常有效kind2
  continuation及settlement算法未改。
- SelfCheck旧矩阵中“zero vaction保留进入frame”的断言已按Authority纠正为action0，并按frame0 kind2
  几何重算Y；新增missing-vaction检查兼容shared shell以`EmptyFrame`而非null表示缺帧的内部差异，只断言
  可观察的action提交与post-action非kind2 fence。

## 最终验证

- focused GREEN job `ec24b2617098487caf4e5a4915371a6f`：`8/8`，覆盖三类invalid target、negative、
  zero、valid continuation、hurtable-not-taken与warmed 4096次0B。
- B6 category job `0cc0326c6d074467854d8c087c257478`：`65/65`。
- `NTSD28` category job `5d4949695d514b7aaba97a2e6daaa494`：`191/191`。
- `BattleHitExecutionPlanEditorTests` job `2ce79fb1a14742f382054b55df242c83`：`185/185`。
- `PreInteractionNoOpProofEditorTests` job `65c0df0344a94e378197de4d1eea082f`：`15/15`。
- targeted Play request `NTSD28-B6-CatchSettlementVactionPreflight-Play-v1`：`Passed / 8 cases /
  invalidTargets=3 / signedAndZero=exact / validContinuation=preserved / hurtableGate=preserved /
  warmedAllocationBytes=0 / sceneMutation=none`。该证据为定向代码路径，不冒充自然current-content事件。
- fresh full SelfCheck越过本包新增/纠正断言后，恢复停在独立旧项
  `CheckSharedDatCpointStep10StatsAndInputOrder()`的held injury accounting断言
  （`BattleRuntimeSelfCheck.cs:11548`）；本包没有修该后继差异。
- 最终 `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal`为`22 warnings / 0 errors`；
  `Assembly-CSharp-Editor.csproj`为`104 warnings / 0 errors`。Unity脚本编译与全部上述测试无编译错误。
- 清理后Console `0 error`；active scene `NTSD_Battle / isDirty=false / rootCount=16`；SHA-256
  `89AA621640A3818F2C7CD307836C50D72CAB84B6D4F2B6DB333FF7CBF525A673`、length `216762`。
- `git diff --check`通过；`Tools/Validate-ChangeLedger.ps1`通过（426 records、362 governed code files；
  历史路径warning不构成失败）。下一严格包是held injury exact accounting，CPoint schema/resource/content仍
  按既有owner顺序后置。

## 回滚

反向恢复本Change的settlement action/preflight与测试；不得回退前置catch exact/mixed包或用户工作树。
