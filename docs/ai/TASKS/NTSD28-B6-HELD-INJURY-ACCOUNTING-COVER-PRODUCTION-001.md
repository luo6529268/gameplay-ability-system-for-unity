# Task Contract — NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001

> 状态：`VERIFIED / RED_14_FAIL_3_PASS_OF_17 / FOCUSED_17_OF_17 / B6_CATEGORY_82_OF_82 / NTSD28_208_OF_208 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / RELATED_FIXTURES_9_OF_9 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED`
> 依赖：
> - `NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-CATCH-SETTLEMENT-VACTION-PREFLIGHT-PRODUCTION-001 / VERIFIED`
> - `NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001 / VERIFIED`
> - `NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001 / VERIFIED`

## 目标

把正 held CPoint injury 的 actual writer 从legacy `FallDamageDiv`/HolderCopy/global-stat镜像迁移到
Authority canonical accounting：raw display lead、`IncomingDamageScale340`、直接owner/type0-self credit、
HP/HPBound、`InputHpConsumedTotal34C`、`InputScoreTotal348`、`KnockoutCount358`与cover排除定时器。

## Authority 合同

- `injury != 0 && catcher.frame_counter == 0`进入；完整MP resource本包不接线，但当resource attacker可解析时
  复用已验证的raw injury display lead。
- 正injury按`IncomingDamageScale340 > 0 ? raw*100/scale : raw`得到damage；不读`FallDamageDiv`。
- credit只解析catcher直接`OwnerSlotIndex`；owner缺失且catcher当前DAT type0时回退catcher self。禁止两跳
  resource-credit规则和`HolderCopySlot`。
- lethal KO要求victim原HP>0、damage>=HP且`OrdinaryCreditGate2F4 == -1`，对同一direct credit写
  `KnockoutCount358++`；world KO event/feed继续后置。
- 随后写victim `HP -= damage`、`HPBound -= damage/3`、`InputHpConsumedTotal34C += damage`，并对credit
  写`InputScoreTotal348 += damage`；不得写legacy combo/kill/global stat。
- catcher `AttackingCounter=1`；`cover != 3`时，`cover != 1`才写catcher `FrameDelay=2`，
  `cover != 2`才写victim `FrameDelay=-3`。比较完整cover整数，10/11不按个位解释。
- 负injury在正式current/release corpus不可达，fail closed且不产生accounting/counter/timer写入。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs`
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldInjuryAccountingCoverProductionEditorTests.cs`
  及其 `.meta`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryCreditGate2F4CorrectionEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3WeaponFluteLegacyStatRetirementEditorTests.cs`
- 本Change治理文档与恢复入口。

## 不变量

- 不调用完整MP resource transaction；B7 suppression、B8 mode override与B11/H baseMax阻塞不变。
- 不连接caughtact combo event/producer，不连接world/session KO feed，不修改snapshot/checksum/schema carrier。
- 不改变vaction preflight、position、cover facing、mixed pass顺序、relation生命周期或throw路径。
- 保留`KillStat`、`world.KillStats`、`ComboCountVic`、`ComboCountAtk`、`world.DamageStats`及`HPLost`
  全部sentinel。
- 不改Scene、Prefab、Config、资源/importer、RNG、shutdown或HitPlan；warmed 4096次0 allocation。

## Test-first 验收

1. RED覆盖raw/scale与raw display、direct owner/one-hop only/type0 self/missing non-type0 credit。
2. lethal/nonlethal/gate/already-dead写精确KO/score/input consumed，且legacy stat sentinel全保持。
3. cover `0/1/2/3/10/11`逐值验证双方timer排除与catcher counter1。
4. negative injury fail closed；`FallDamageDiv`、`KillCount`、`HolderCopySlot`不得裁决。
5. compile、focused、B6/NTSD28/HitPlan/PreInteraction、full SelfCheck、现有真实Battle grab Play probe、
   Console、Scene与Ledger按新鲜证据推进。

## 回滚

只反向恢复本包actual accounting/cover与相应测试；不得回退前置settlement/mixed/exact包或用户文件。
