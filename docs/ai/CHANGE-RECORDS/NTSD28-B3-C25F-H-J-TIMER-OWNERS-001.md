# NTSD28-B3-C25F-H-J-TIMER-OWNERS-001 — C25f/h/j timer owners

<!-- CHANGE-RECORD
id: NTSD28-B3-C25F-H-J-TIMER-OWNERS-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_OWNER_MIGRATION
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameTickPass.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FHJTimerOwnerEditorTests.cs
authority: NTSD 2.8-Logan simulation_tick_driver.cpp C25f/g/h/i/j order, battle_world.cpp advance_reaction_timers_slot and decrement_attacker_rest_slot; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-SOURCE-CLOSED / EXE-HASH-RECONFIRMED / RED-13-OF-13 / COMPILE0 / FOCUSED15 / ADJACENT29 / NTSD28-BROAD425 / RELATED38 / TOOL-BUILD0-TRACE21-RAW5 / JOINT-RAW-RENDER-PHASE-EQUAL / SELFCHECK-09-48-48-PASS / SCENE-UNCHANGED / CONSOLE0
-->

> 状态：`VERIFIED / C25F-H-J-PRODUCTION-OWNERS / JOINT-RAW-RENDER-PHASE-EQUAL`

## 改前事实

- C25f 的 `state 7000..7999 -> NativeComputerState1B8` 没有 production writer。
- `HitStop/Fall/HitConfirmEa` 和 `AttackExempt` 在 C25g frame body 内提前递减；`HitStateCount` 也被旧公共 counter 段递减，但 current authority C25h 没有该字段。
- `Bdefend` 不在 frame counter 段，而后置 legacy TU 用 -0.5 accumulator 恢复；与 authority 每 tick -1 不同。
- `Unk338` 已有正确 C25h owner；其余十字段 bank、positive-HP status、poison 与 join/proxy cleanup 未接 production。
- `RecoverLegacyHitCounters`保留旧-0.5路径；修改前需核验它是否实际进入post-C25 production serial，不能仅凭方法名推断重复writer。

## 计划

先新增覆盖 owner/gate/order 的 focused tests并取得红灯；再在现有 per-slot LateEntity transaction 内建立明确 f/h/j helper，把 g 内旧 counter/rest writer 限制到 direct compatibility，并闭合 post-C25 serial 的真实调用者审计。

## 不变量与风险

- 新写 render phase 15 的同 tick skip 必须使用仅在 C25g→h 原子区间有效的 transient marker，不能进入 snapshot/checksum，也不能跨 tick 泄漏。
- status bank 只对正值递减；0/负值保留。
- poison 和 relation restore 改变同 tick 可观察 HP、统计、battle group；必须由 focused tests固定顺序。
- 本包不触碰 H/B11 内容 gate，因此正式 Config 当前可能没有 poison/delay producer；程序化 fixture 仍必须证明 runtime 算法。

## 实际改动

- `BattleLateEntityLifecycleModule`在每个live slot中执行C25f refresh，围绕C25g建立不跨tick的transient marker，再执行完整C25h和C25j。
- C25h实现render phase向0/15-arm/dead30、Fall/Bdefend/HitConfirmEa、`Unk338`、十字段bank、positive-HP status、poison和join/proxy cleanup。
- C25j从frame body迁到h之后，保持held ordinary冻结、type3例外和negative relation不参与gate；canonical变化后沿既有条件同步`ItrRest.Arest`。
- `LF2Entity.RunReleaseFrameTickCounters`和exact frame pass在C25原子区间不再提前写reaction/rest/dead30；direct调用继续保留旧counter/TU行为。
- `HitStateCount--`没有current Authority C25h owner，production C25不再执行；direct compatibility仍保留。
- 真实caller复核：`RecoverLegacyHitCounters -> RunTUCore`；当前post-C25 exact-character production serial走空的post-physics seam，不调用该路径。因此撤销试作的World suppression flag，未留下dead state。

## 验证

- 红灯 job `f1f2abecd895441b9e63ab40b431c406`：13/13 expected fail；随后compile0。
- first green 13/13 `1f5bea4826224ddd80f4027d6c959fa8`；扩充HP0与direct compatibility后final focused `c0959a5169cc432992b13c2bbde6dd27` 15/15。
- 相邻C25/OID/proxy/cooldown/frame `4b84a12ec61c4c38ba8d1a55dcbff05a` 29/29；`.*NTSD28.*` broad `eb75aafe91854de880bff789519bbf34` 425/425；额外ownership/frame/cooldown/snapshot `3d0427d2d39f4604bfcd5dd5588e48b4` 38/38。
- parity tool build 0/0、trace 21/21、raw 5/5。render-phase joint raw comparison SHA `1DE6B25C408294249D3CE5F5EB65E5E9244BB55E932506A6A7CD913BCD113298`，3tick/6pair/288，`combat.renderPhase`继续equal，15个既有差异未掩盖。
- SelfCheck 09:48:48 PASS；测试生成的7条预期negative registration日志已清空，post-clear Console0。
- Scene SHA `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、length203477、mtime不变；没有进入Play。

## 未关闭边界

- C25i armor recovery、C25g all-object body、AI render-phase consumers及正式 poison/delay content仍为后续包。
- 一次只读`execute_code` Play-state查询因UnityMCP/Mono命令行过长失败；它不属于产品编译/测试失败。EditMode jobs与非Play request均正常完成。
