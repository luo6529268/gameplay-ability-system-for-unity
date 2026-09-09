# Task Contract — NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001

> 状态：`VERIFIED / RED_0_OF_3 / FOCUSED_19_OF_19 / B6_CATEGORY_41_OF_41 / HITPLAN_185_OF_185 / NTSD28_167_OF_167 / TARGETED_PLAY_19_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / EXACT_RELATION_ATOMIC`
> 依赖：
> - `NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-OWNER-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001 / VERIFIED`
> - `NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001 / VERIFIED`

## 目标

把 Authority kind3 catch relation 的 first-action、signed facing、双 frame 原子预检、exact reciprocal
slot、respond timeout 与 hit-reaction reset 同时接入 Unity shared actual writer 和 HitPlan shadow，使成功与
unsupported 路径都可被 shadow compare 精确观察。

## Authority 与 Unity 映射

- Authority：正式 playable closure 中 SHA-256 `EB37E8EC1186BF0349A3B3B5759666C3A8F44F0E1640C159E06F805189FBCCB1`
  的 `BattleWorld28::resolve_kind3_catch_relation()`；正式 EXE `B1E13AE1...9033`，closure `39DDDA15...6109`。
- `catchingact` / `caughtact` 取第一个整数，缺失为0；负值翻转对应初始 facing 后取绝对值。
- 双方 relation frame 任一缺失时，在 facing、frame、position、motion、relation、timeout、Fall 和 RNG
  发生任何写入前返回 unsupported。
- 成功时 exact `attacker.CaughtSlotIndex = target slot`、
  `target.Runtime.CatchSourceSlot90 = attacker slot`；`target.CatcherSlotIndex`仅继续作兼容 mirror。
- timeout 为 `itr.respond == 0 ? 300 : itr.respond`，包括负数；target Fall归零。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- 新增本包 focused Editor/Play 测试脚本及 `.meta`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本 Change 的 Task/Record/Ledger/STATE/handoff/总表/CURRENT-AUTHORITY。

## 不变量

- kind1保留既有默认 action、mutation timing、geometry与兼容行为；本包严格规则只作用于kind3。
- 不改 candidate collection/selection、special-hit latch、hit-group、kind2/7 pickup、cpoint settlement、
  mixed catch advance、held accounting、positive-link validation或 lifecycle cleanup。
- 不新增 RNG、服务、队列、publication 或 shutdown 阶段；不改 content、Scene、Prefab、importer 或 Authority。
- HitPlan snapshot增加 exact catch-source，并与 compat catcher 共用既有 bit31，不扩张64-bit diff schema。
- warmed 4096 次 successful/unsupported actual writer与shadow观察不得产生 managed allocation。

## Test-first 验收

1. RED必须至少证明当前 reachable success 漏 exact source、missing frame发生前置 mutation、signed/timeout不符合。
2. focused覆盖slot0/extended high、positive/signed/both-signed、respond `0/1/300/-1`、三种 missing frame、
   whole-state bitwise preserve、RNG与 wait counter、actual/shadow mask。
3. kind1 regression必须继续通过；current criminal positive witness必须覆盖。
4. 运行 compile、focused、B6/HitPlan related、BattleRuntimeSelfCheck、targeted Play、Console、Scene与Ledger验证；
   只按真实证据推进状态。

## 回滚

只反向恢复本 Change 的 kind3 branch、HitPlan exact snapshot/projection/diff 与新增夹具；不得回退前置
lifecycle/invalid-preserve包或工作树中其他用户改动。

## 当前结果（2026-09-09）

- kind3 actual现先完成first/signed action解码和双authored-frame预检；unsupported是whole-pair零写入。
- 成功分支写exact `CatchSourceSlot90`与compat mirror，respond按0→300/nonzero原值，HitPlan capture/
  projection/diff同步闭合；kind1 legacy行为保持。
- Unity RED `0/3`；GREEN focused `19/19`、B6 category `41/41`、HitPlan `185/185`、
  NTSD28 category `167/167`、真实Play `19` cases；Runtime/Editor build与Unity compile均0 error。
- full SelfCheck通过本包提前插入检查后，仍停在独立held injury legacy accounting；Console清理后0 error。
- Scene SHA-256/长度保持进入包前`89AA6216...A673`/216762，`isDirty=false`、rootCount16；无Scene交付。
