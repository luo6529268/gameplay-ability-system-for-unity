# NTSD28-B6-HELD-INJURY-CAUGHTACT-EVENT-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-INJURY-CAUGHTACT-EVENT-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_POST_SETTLEMENT_CAUGHTACT_EVENT_PRODUCER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Interaction/BattleInteractionPipeline.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldInjuryCaughtActEventProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs
authority: NTSD 2.8-Logan BattleWorld28::settle_catch_relations 6090..6158 and SimulationTickDriver28::produce_caughtact_combo_hits 153..179; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-5-FAIL-10-PASS-OF-15 / FOCUSED-15-OF-15 / B6-CATEGORY-97-OF-97 / NTSD28-223-OF-223 / HITPLAN-185-OF-185 / PREINTERACTION-15-OF-15 / REAL-BATTLE-GRAB-PLAY-PASS / BUILDS-0-ERROR / SELFCHECK-BLOCKED-UNRELATED-POSITIVE-LINK / CONSOLE-0-ERROR / SCENE-UNCHANGED-CURRENT-BASELINE / LEDGER-PASS-428-RECORDS-364-CODE-FILES / POST-SETTLEMENT-EVENT-EXACT
-->

> 状态：`VERIFIED / RED_5_FAIL_10_PASS_OF_15 / FOCUSED_15_OF_15 / B6_CATEGORY_97_OF_97 / NTSD28_223_OF_223 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_428_364 / POST_SETTLEMENT_EVENT_EXACT`

## 改前事实

- Unity held accounting 已精确完成 canonical damage/score/KO/cover 写入，但没有 Authority
  `hold_injury_events` 对应的 transient collection，也没有 settlement 后 caughtact producer。
- 既有 `BattleNativeComboOrdinaryProducer` 已验证 record/bound、caught type-0、facing、单层 owner
  和 process tick；本包只建立真实 B6 event producer，不复制或改写该逻辑。
- Authority 事件在正 injury accounting 完成后按 catcher slot 升序追加，并在整个 settlement
  返回后消费；writer 内立即生产会破坏这一可观察顺序。

## 预定实现

- 先新增 focused RED，覆盖 full pass 时序、formal tuple gates、type/facing/owner、排除矩阵、
  多事件和 warmed 0B。
- `BattleCpointWriter` 只在 pipeline 显式开启的 transient sink 中记录成功 applied event；
  direct writer 保持不生产。
- `BattleInteractionPipeline` 复用预分配 scratch，在完整 settlement loop 后顺序调用既有 producer，
  并在所有退出路径清空/解绑。
- 更新 SelfCheck 与真实 Battle grab probe，使生产事件进入现有端到端验证。

## 验证记录

- Unity EditMode focused RED：`5 fail / 10 pass / 15 total`。五项失败严格为应产生count的
  complete-settlement、reverse-facing、one-hop-owner、two-events与warmed accumulation；formal tuple、
  non-type0 caught、zero/negative/already-attacking、invalid-vaction与direct-writer十项保护性断言通过。
  生产代码仍未修改。

## 实际实现

- `BattleCpointWriter` 新增 transient sink scope；只有完整正 injury accounting 与cover timer尾完成后，
  才追加 physical catcher/caught slots。direct writer未开启scope时只做原有settlement写入。
- `BattleInteractionPipeline` 持有按entity capacity预分配的event scratch，在整个settlement升序loop后才
  检查formal `record/bound/caughtact`并按序调用既有producer；所有退出路径解绑sink并清空scratch。
- event不进入runtime state、snapshot、checksum或parity；B5 producer本身未修改。

## 最终验证

- focused GREEN：`15/15`；覆盖 complete-settlement 可见性、next-pass frame-counter gate、
  record/bound/caughtact、facing0/1、caught type0、non-type0 catcher one-hop owner、direct writer no-op、
  zero/negative/already-attacking/vaction-invalid、双事件及4096次 warmed full pass 0B。
- Unity Test Framework：B6 category job `2feb02dbea9046e982d642faa7d9d67e` 为`97/97`；
  NTSD28 job `1cde0128f79d4129a41546e9f56ccbe0` 为`223/223`；HitPlan job
  `ae3430b334e641a69d84aafb49accda7` 为`185/185`；PreInteraction job
  `5416cdb0482048bba02958fd40703c33` 为`15/15`。
- fresh full SelfCheck通过新增caughtact断言后，仍首差于独立
  `CheckValidatePositiveLinksMatrix()` inactive-target旧夹具（现`BattleRuntimeSelfCheck.cs:13573`）；
  本包没有成为SelfCheck阻塞，也不能据此声明full SelfCheck通过。
- 真实`NTSD_Battle` grab probe为`PASS`：victim HP`20→-10`、HPBound`100→90`、consumed
  `17→47`、score`11→41`、KO`13→14`，并新增caughtact count`0→1`、last tick`3`；
  后续positive-link/second-held未重复生产，cleanup完成。
- 最终`dotnet build Assembly-CSharp.csproj`与`Assembly-CSharp-Editor.csproj --no-restore`
  均`0 error`（既有warning分别47/104）；Unity Console为0 error。
- Play已退出；active scene仍为`NTSD_Battle / isDirty=false / rootCount=13`。Scene文件last-write
  `14:33:50`早于本Change脚本实现，未被本Change改写；当前用户基线SHA-256
  `D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11`、length209891。
- `git diff --check`通过；`Tools/Validate-ChangeLedger.ps1`通过（428 records、364 governed code files；
  历史路径warning不构成失败）。完整MP resource、world KO feed、B10显示及H正式tuple激活仍后置。

## 回滚

反向移除本 Change 的 transient event collection、post-settlement consumer与测试；不得回退任何前置包。
