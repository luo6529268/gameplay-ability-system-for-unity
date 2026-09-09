# Task Contract — NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001

> 状态：`VERIFIED / RED_2_OF_2 / FOCUSED_7_OF_7 / B6_CATEGORY_22_OF_22 / RELATED_47_OF_47 / BROAD_56_OF_62_6_UNRELATED_NATIVE_INPUT_PROXY / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / DIAGNOSTIC_PRESERVE`
> 依赖：
> - `NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001 / VERIFIED`
> - `NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001 / VERIFIED`

## 目标

将C09/C20共享的negative held relation invalid分支从“清`LinkState`”改为Authority的
diagnostic-and-continue：missing/out-of-range parent或reciprocal target mismatch时记录失败但不修改child、
parent、action、motion或RNG。正常despawn残留已由前置lifecycle cleanup消除。

## Authority 与映射

- Authority：SHA-256 `EB37E8EC1186BF0349A3B3B5759666C3A8F44F0E1640C159E06F805189FBCCB1`
  的playable `BattleWorld28::settle_held_refill_objects()`。
- `interaction_state < 0`映射`Runtime.LinkState < 0`；`linked_parent_slot`映射
  `Runtime.HolderStableId`；reciprocal `parent.linked_child_slot`映射parent
  `Runtime.TargetSlotIndex`。
- invalid只改变pass success/diagnostics；Unity适配为累计/last-pass失败计数与可选structural diagnostic event，
  生产状态零写入。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Passes/Interaction/SimulationQueryAndLinkModule.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Test/Editor/SimulationQueryAndLinkModuleEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本Change治理文档。

## 不变量

- 只改negative held C09/C20 invalid分支；positive-link validation继续由独立pass所有。
- invalid时保留child Link/Holder、parent Target与双方其他字段；不刷新snapshot、不消费RNG、不执行WPoint。
- slot0按LinkState discriminator有效；扫描上限使用Unity当前logical capacity，不硬编码1000。
- lifecycle cleanup后的正常removed/reuse路径不得产生invalid计数或ABA。
- trace只在既有diagnostic sink存在时materialize；sink关闭的warmed invalid path必须0 managed allocation。
- 不改catch relation、mixed catch advance、settlement vaction、held accounting、content、Scene或shutdown顺序。

## Test-first 验收

1. focused RED覆盖out-of-range/missing、active mismatch、C09→C20 preserve。
2. 覆盖slot0、extended high slot、前置lifecycle cleanup后无invalid、双方sentinel与RNG零变化。
3. 验证累计/last-pass失败计数与missing/mismatch structural trace；sink-off warmed路径0 allocation。
4. compile、focused、B6/interaction related、SelfCheck、targeted Play、Console、Scene和Ledger按实际证据记录。

## 回滚

恢复invalid分支的旧clear与旧夹具，移除新增diagnostic计数/trace；不得回滚前置lifecycle cleanup或用户Scene。

## 当前结果（2026-09-09）

- invalid negative-held分支已变为counter/optional trace + preserve；不写双方关系、action/motion或RNG。
- RED `0/2`、focused `7/7`、B6 category `22/22`、clean adjacent `47/47`、真实Play `7` cases、
  Runtime/Editor build 0 error。
- 较宽job为`56/62`，6项全是独立`NativeInputProxy`引用比较旧夹具；未混入本包。
- full SelfCheck通过本检查后仍停在held accounting line11403；Scene自动保存副作用已精确恢复到
  `89AA…A673`并确认clean。
