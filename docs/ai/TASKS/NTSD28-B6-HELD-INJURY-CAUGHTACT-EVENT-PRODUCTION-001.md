# Task Contract — NTSD28-B6-HELD-INJURY-CAUGHTACT-EVENT-PRODUCTION-001

> 状态：`VERIFIED / RED_5_FAIL_10_PASS_OF_15 / FOCUSED_15_OF_15 / B6_CATEGORY_97_OF_97 / NTSD28_223_OF_223 / HITPLAN_185_OF_185 / PREINTERACTION_15_OF_15 / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_POSITIVE_LINK / CONSOLE_0_ERROR / SCENE_UNCHANGED / LEDGER_PASS_428_364`
> 依赖：
> - `NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001 / VERIFIED`
> - `NTSD28-B5-NATIVE-COMBO-ORDINARY-PRODUCER-001 / VERIFIED`

## 目标

把 Authority `WorldCatchSettlementPass28::hold_injury_events` 的临时有序事件边界接入 Unity：
只收集真正完成正 held injury accounting 的 catcher/caught slot pair，并在整个 catch settlement
升序 pass 完成后，按事件顺序调用既有 `BattleNativeComboOrdinaryProducer`。

## Authority 合同

- Authority 在 `BattleWorld28::settle_catch_relations()` 中，只有 `injury > 0`、catcher frame
  counter 为0、vaction后仍有有效 kind-2 caught frame且完整 held accounting 已执行时，才把
  `{catcher_slot,caught_slot}` 追加到本 tick 的 transient event list。
- `SimulationTickDriver28::produce_caughtact_combo_hits()` 严格位于整个 settlement 返回后；
  只有 formal combo record present、`bound==1` 且 `caughtact==1` 才消费事件。
- 每个事件在消费时重新按当前 live slot 解析 caught；caught 必须为 definition type-0。
  `facing==1` 选择 catcher，否则选择 caught；非 type-0 source 只解引用一层 `OwnerSlotIndex`。
- 成功生产复用既有 canonical `NativeComboHitCount1E0++` 与
  `NativeComboHitLastTick1E4=NativeFrameSequence+1`；不新增 persistent carrier。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/Interaction/BattleInteractionPipeline.cs`
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldInjuryCaughtActEventProductionEditorTests.cs`
  及其 `.meta`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs`
- 本 Change 的治理文档与恢复入口。

## 不变量

- 不在 `ApplyHeldInjury()` 内立即生产 combo；必须等 settlement 全部 slot 完成。
- direct `RunWeaponSyncHeldStep10()` 不得越过 pipeline 边界生产 combo。
- event 只含两个 physical slot，消费时按 Authority live lookup；不把 entity 引用、generation 或
  event list 写入 snapshot/checksum/parity。
- zero/negative injury、already-attacking、broken reciprocal、vaction preflight失败均不得入队。
- 保持 B5 producer 的 record/bound/type/facing/one-hop owner/last-tick语义；不修改 expiry。
- event scratch 预热后每 tick 0 managed allocation，并在 no-op、异常与下一个 tick 前清空。
- 不改完整 MP resource、world KO feed、Scene、Prefab、Config、资源、RNG、shutdown或 HitPlan。

## Test-first 验收

1. RED 先证明正 injury 尚未通过 full pre-interaction boundary 生产 caughtact count，并冻结
   settlement 尾后可见性与第二 tick frame-counter gate。
2. 覆盖 record/bound/caughtact gates、facing 0/1、caught type-0 gate、non-type0 catcher
   one-hop owner、direct writer no-op、zero/negative/already-attacking/vaction-invalid exclusions。
3. 覆盖两个 slot-ordered applied events及 warmed full pass 4096 次 0B。
4. compile、focused、B6/NTSD28/HitPlan/PreInteraction、full SelfCheck、真实 Battle grab Play probe、
   Console、Scene 与 Ledger 按新鲜证据推进。

## 回滚

只移除本包 transient event collection、post-settlement consumer及其测试/治理记录；不得回退
held accounting、settlement preflight或 B5 combo producer。
