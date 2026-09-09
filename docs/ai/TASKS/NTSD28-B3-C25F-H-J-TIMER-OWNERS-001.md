# Task Contract — NTSD28-B3-C25F-H-J-TIMER-OWNERS-001

> 状态：`VERIFIED / C25F-H-J-PRODUCTION-OWNERS / JOINT-RAW-RENDER-PHASE-EQUAL`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C25f-h-j`
> 依赖：`NTSD28-B3-C25F-J-STATE-CARRIERS-001 / VERIFIED`

## 目标

按当前 NTSD 2.8-Logan 的同一升序 live-slot transaction，把 C25f computer refresh、C25h reaction/status timer 与到期清理、C25j attacker-rest decrement 接入 Unity production owner，并移除 C25g/后置 legacy serial 对这些字段的重复或错误速率写入。

## Authority 合同

- 正式 EXE SHA-256：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
- playable closure：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- `simulation_tick_driver.cpp:996-1027`：C25f refresh → frame → reaction timers → armor → attacker rest。
- `battle_world.cpp:3662-3798`：C25h body gate、render phase、Fall/Bdefend/kind6、`Unk338`、十字段 bank、positive-HP status、poison、join/proxy cleanup。
- `battle_world.cpp:2008-2025`：C25j 只按 `attacker_rest>0 && (motion_hold==0 || type3)` 递减，不看 relation。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameTickPass.cs`
- 新增 focused Editor test；仅修正被当前 authority 直接 supersede 的旧精确断言。
- 本 Task/Record、Ledger、STATE、handoff、总表与 C25f-j manifest。

## 必须保持

- C25f 只刷新 slot 0..9、current DAT type0、current frame state 7000..7999；随后同 tick 的 C25h positive-only bank 再减 1。
- body skip 为 `LinkState<0 || (FrameDelay!=0 && type!=3)`；只冻结 frame/reaction/positive-HP status，不冻结 `Unk338`、十字段 bank 或 join/proxy cleanup。
- render phase 复用已验证 `HitStop/HitStun`；caught-exit 新写 15 的同 tick 不减，其余正负值向 0，action202 写 20 后同 tick 可减为 19。
- C25h body 中 Fall、Bdefend、HitConfirmEa 只在正值时减 1；Unity legacy `HitStateCount--` 没有本 authority owner，不得继续冒充 C25h。
- poison 在 decrement 后仍正且低 5 bit 为 0 时结算；type 0/1 为常量，2/3 为当前 HP 百分比，4/5 为 base max MP 百分比；累计 `InputHpConsumedTotal34C`，负 HP 按 poison type 奇偶落到 0/1。
- C25j 位于 C25h 后；canonical `AttackExempt` 为真值，`ItrRest.Arest` 只做既有条件镜像。
- 后置 production serial 必须核验是否存在 Fall/Bdefend writer；若无实际 caller，不新增虚假的抑制状态。direct compatibility 入口保留旧行为。

## 不做

- 不实现 C25i armor recovery，不修改 armor/content/schema/hit producer。
- 不修改 Config、DAT、PNG、WAV、Prefab、Scene、ProjectSettings 或权威目录。
- 不在本包修改 AI render-phase consumers；它使用独立 package 与 RNG 回归。
- 不改 C25g all-object frame body、heavy-weapon extra、B10 audio 或 B11 transition content。

## 验收

- 新 focused tests 在实现前因 owner 缺失或现值错误形成红灯。
- 覆盖 C25f 同 tick refresh/decrement、slot/type边界、body skip/type3例外、十字段 bank、正 HP status、poison三类计算与奇偶下限、join/proxy cleanup、render armed 15、Bdefend/Fall/kind6、C25j relation-independent gate与 mirror。
- 证明 production LateEntity 只有 C25h/C25j writer；后置 production serial writer 审计闭合，direct compatibility 仍可运行。
- Unity compile0、focused、相关回归、NTSD28 broad、SelfCheck通过；本包不要求资源型 Play，Scene unchanged、Editor 非 Play、Console0。

## 回滚

撤销本包明确列出的 production owner/抑制 seam 与 focused tests，恢复旧 C25g/serial 行为；不得回退 C25f-j carriers、render-phase binding 或其他已验证 C25 包。

## 实施与证据

- test-first job `f1f2abecd895441b9e63ab40b431c406`：13/13 预期失败，分别命中 refresh、bank/status、body gate、poison、render phase、Bdefend 与 C25j。
- C25f/h/j 现位于同一 `BattleLateEntityLifecycleModule` 升序 live-slot transaction；C25g 的 direct compatibility counters 通过原子 transient marker 与 production owner隔离。
- caught-exit 新写15当tick保留；action202写20后C25h变19；dead primary state14在向0移动后按lives重置30。
- 十字段bank、positive-HP status、poison、join/proxy cleanup及C25j relation-independent gate均有程序化fixture。
- 审计确认 `RecoverLegacyHitCounters` 仅由 `RunTUCore` 调用，当前 post-C25 production serial 对 exact character 不走该方法；因此没有加入无效 suppression flag，direct self-check compatibility仍保留。
- final focused job `c0959a5169cc432992b13c2bbde6dd27` 15/15；相邻 job `4b84a12ec61c4c38ba8d1a55dcbff05a` 29/29；NTSD28 regex broad `eb75aafe91854de880bff789519bbf34` 425/425；frame/ownership相关 `3d0427d2d39f4604bfcd5dd5588e48b4` 38/38。
- parity tool build 0 warning/0 error、trace self-test 21/21、raw self-test 5/5。
- render-phase raw：Authority SHA `C1661264...00C8`，Unity SHA `563CA834...6AA`，comparison SHA `1DE6B25C...3298`；3tick/6pair/288 occurrences、`combat.renderPhase` equal，既有first difference仍为`frame.action`。
- SelfCheck 2026-09-05 09:48:48 PASS；Scene SHA `0D74E174...D77`、length与mtime不变；未进入Play，post-clear Console0。
