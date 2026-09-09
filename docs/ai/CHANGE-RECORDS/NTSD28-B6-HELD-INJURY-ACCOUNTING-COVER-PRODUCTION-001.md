# NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_HELD_INJURY_CANONICAL_ACCOUNTING_COVER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldInjuryAccountingCoverProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryCreditGate2F4CorrectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3WeaponFluteLegacyStatRetirementEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::settle_catch_relations 6090..6167 and record_native_knockout; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-14-FAIL-3-PASS-OF-17-JOB-2A1DDF / FOCUSED-17-OF-17 / B6-CATEGORY-82-OF-82 / NTSD28-208-OF-208 / HITPLAN-185-OF-185 / PREINTERACTION-15-OF-15 / RELATED-FIXTURES-9-OF-9 / REAL-BATTLE-GRAB-PLAY-PASS / BUILDS-0-ERROR / SELFCHECK-BLOCKED-UNRELATED-POSITIVE-LINK / CONSOLE-0-ERROR / SCENE-UNCHANGED / LEDGER-PASS-427-RECORDS-363-CODE-FILES / CANONICAL-ACCOUNTING-COVER-EXACT
-->

> 状态：`VERIFIED / RED_14_FAIL_3_PASS_OF_17 / FOCUSED_17_OF_17 / B6_CATEGORY_82_OF_82 / NTSD28_208_OF_208 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / RELATED_FIXTURES_9_OF_9 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / CANONICAL_ACCOUNTING_COVER_EXACT`

## 改前事实

- Unity `ApplyHeldInjury()`以legacy `FallDamageDiv`缩放，通过`HolderCopySlot`/`KillCount`/`Unk344`
  写`KillStat`、local combo与world kill/damage arrays，漏写canonical consumed/score/KO carriers。
- Authority使用`IncomingDamageScale340`，credit只是一层owner或type0 self，lethal gate是
  `OrdinaryCreditGate2F4==-1`，并按cover完整值排除双方timer。
- current Direction-B positive kind1 injury为223条，cover分布0:205/1:15/11:3；negative为0。
  release positive为484条且negative为0，因此本差异正式可达。

## 预定实现

- 先新增focused RED，冻结raw/scale/direct credit/KO/canonical writes/legacy sentinel/cover matrix/negative/0B。
- 只重写`ApplyHeldInjury()`的可独立精确子集；raw display lead复用既有helper，完整MP resource、caughtact
  event与world KO feed继续后置。
- 更新SelfCheck和现有真实Battle grab Play probe到canonical字段；不以新synthetic runner替代真实场景probe。

## 验证记录

- Unity EditMode RED job `2a1ddf19358f4b8e81d9c1ec6f3cb201`：`14 fail / 3 pass / 17 total`。
  失败覆盖cover1/2/3、direct/self/missing credit、三项KO gate、legacy stat、negative、raw/scale与0B
  canonical expectations；cover0/10/11旧无条件timer恰好通过。精确命中预定差异。

## 实际实现

- `ApplyHeldInjury()`现对负injury fail closed；正injury先在resource attacker可解析时复用raw injury display
  lead，再仅按`IncomingDamageScale340`计算damage，不再读取`FallDamageDiv`。
- 新的writer-local direct credit只解析catcher一层`OwnerSlotIndex`；owner缺失且catcher当前DAT type0时回退
  catcher self。score和lethal KO共用该credit，不跟随resource attacker的两跳链，也不读`HolderCopySlot`。
- lethal gate在HP写前检查victim原HP>0、damage>=HP与`OrdinaryCreditGate2F4==-1`，写
  `KnockoutCount358`；随后写HP、HPBound/3、`InputHpConsumedTotal34C`、`InputScoreTotal348`。
- catcher `AttackingCounter=1`；cover完整值严格实现3全排除、1仅排除catcher delay、2仅排除victim delay，
  0/10/11写双方timer。legacy KillStat/combo/world arrays与HPLost全部保持。
- SelfCheck的shared、global-stat matrix与phase-owner断言迁到canonical字段；两个B5历史测试分别更新
  反射5参数调用/canonical KO+score和“CPoint legacy writer必须不存在”的源码守卫。
- 真实Battle grab探针迁到canonical证据，并纠正三个前置verified合同的旧夹具：mismatch terminal + mixed
  orphan slot order、negative escape exact source/AttackingCounter、invalid negative relation完整preserve。

## 最终验证

- focused GREEN job `123be2f6ba0148fb90be4c18e0974456`：`17/17`，覆盖raw/scale/display、direct
  one-hop/type0 self/missing non-type0、三项KO gate、cover六值、negative、legacy sentinels与4096次0B。
- B6 category final job `96bab63031ed4cd5ae67427460bb9eda`：`82/82`。
- `NTSD28` category final job `f45e31e942954b4e8363614cb9c72694`：`208/208`；初跑两项
  历史fixture失败经上述合同纠正后全绿。
- `BattleHitExecutionPlanEditorTests` job `df528d5919e94b0caf59562e13403e5e`：`185/185`。
- `PreInteractionNoOpProofEditorTests` job `2626a5dbcd6344369180704d82139863`：`15/15`。
- 两个相邻B5 fixture class job `07de0511c2324d37ba2a9ea3d0e91fd6`：`9/9`。
- 真实 `NTSD_Battle` 的 `BattleGrabCpointLinkPlayModeProbeEditor`最终于14:22返回`PASS`：victim
  `20→-10`、HPBound `100→90`、InputHp `17→47`、catcher score `11→41`、KO `13→14`，
  legacy combo/holder/global stats保持，四个pass、关系尾与cleanup全通过。前三轮失败均是上节列出的历史
  探针期望，不是accounting生产失败。
- fresh full SelfCheck通过本包全部检查后，首差推进到独立
  `CheckValidatePositiveLinksMatrix()` inactive-target旧夹具（`BattleRuntimeSelfCheck.cs:13532`）；
  accounting不再阻塞，不能把该后继差异记为本包失败或完整SelfCheck通过。
- 最终两次`dotnet build Assembly-CSharp*.csproj --no-restore -v:minimal`均`0 errors`（最终增量输出
  各22 warnings）；Unity测试/Play脚本编译无error。
- Unity Console清理后`0 error`；Editor不在Play、active scene `NTSD_Battle / isDirty=false / rootCount=16`；
  scene SHA-256 `89AA621640A3818F2C7CD307836C50D72CAB84B6D4F2B6DB333FF7CBF525A673`、length216762。
- `git diff --check`通过；`Tools/Validate-ChangeLedger.ps1`通过（427 records、363 governed code files；
  历史路径warning不构成失败）。完整MP resource、caughtact event与world KO feed均未接线。

## 回滚

反向恢复本Change的accounting/cover writer与测试；不得回退前置catch settlement包或用户工作树。
