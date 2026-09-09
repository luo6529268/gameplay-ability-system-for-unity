# NTSD 2.8-Logan C25f-j field/owner inventory

> Change ID：`NTSD28-B3-C25F-J-FIELD-OWNER-AUDIT-001`  
> 状态：`VERIFIED / GOVERNANCE_ONLY / FIELD_OWNER_MATRIX_COMPLETE`  
> Authority：formal EXE `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；playable closure `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`

## 1. C25f-j共同顺序

当前Authority在同一个升序live-slot transaction中固定执行：

```text
C25f computer-state refresh
  → C25g frame body
  → C25h reaction/status timer tail
  → C25i armor recovery
  → C25j attacker-rest decrement
```

`C25g`写terminal/lifecycle pending后，实体仍会执行h/i/j；只有slot已经不再active时这些slot API才no-op。新生高slot在同tick依次经历f-j，已扫描低slot等下一tick。

共同body skip为`interaction_state < 0 || (motion_hold_timer != 0 && object_type != 3)`。它冻结C25g、C25h body和C25i，但不冻结C25h的`+0x338`、十dword timer bank及join/proxy expiry cleanup，也不参与C25j的relation gate。Unity对应候选为`LinkState`与`FrameDelay`，type3是明确exception。

## 2. C25f computer-state refresh

| Authority | Unity现状 | 结论 |
|---|---|---|
| 仅slot&lt;10、type0、current frame state 7000..7999写`native_computer_state_1b8=state-7000` | 无runtime carrier/production writer；AI仅有`AiControlled`和旧诊断投影 | `MISSING_CARRIER_AND_WRITER`；不得用AiControlled替代倒计时 |
| 随后C25h对正值减1，所以state7002同tick最终为1，state7000明确覆盖为0 | 无对应timer-bank消费 | `MISSING_SAME_TICK_DECAY` |
| 高slot、非type0不刷新，但已有正值仍由C25h减1 | 无对应carrier | `MISSING` |

locked Authority decoded DAT与Unity Config均实测没有`state: 7xxx` producer；但正式GameSession/Scenario可直接初始化该字段，且native AI/nameplate读取它，因此不能以当前DAT零命中删除该契约。

## 3. C25g frame body

Authority `BattleWorld28::step_frame_slot`的主要边界：

1. inactive/lifecycle-pending no-op；negative relation与ordinary nonzero motion-hold冻结，type3例外；
2. slot&lt;20 type0、HP&lt;=0、state14且无剩余/排队复活时保持倒地action；
3. type3在frame advance前执行`hit_a/hit_d` HP drain；
4. 按action-latch重置counter，counter++，超过wait才处理next、负next翻面、999 runtime resolution、terminal code；
5. 同一pass处理transition side effects、两次frame sound观察、自动HP/MP transition cost、非法post-cost action terminal；
6. 最终action 110/114写defend re-entry cooldown=3；state14离开时按mode/group/id gate写render phase 15且同tickh不递减。

Unity已有`BattleEcsCharacterFrameTickPass`和`RunCommonFrameTick`，但仍是`PARTIAL_AND_SPLIT`：

- exact pass只覆盖真实`LF2Character`且current DAT type0，其他类型回退legacy body；
- type3 drain、counter/next、negative next、next999、defend cooldown及state14→15大体存在；
- `AttackExempt--`属于C25j，却在g入口执行；render/fall/kind6等h字段也混在`RunReleaseFrameTickCounters`中先于frame advance；
- heavy-weapon state12 grounded/低Vx早退是Unity legacy extra，在当前Authority `step_frame_slot`未找到同名分支，需定向trace决定删除；
- frame audio归B10，DAT transition cost/content缺口归B11；不能仅凭现有type0 exact测试声明all-object C25g对齐。

## 4. C25h reaction/status timer matrix

### 4.1 FUN_0040D000 body（受held/relation skip）

| Authority字段/规则 | Unity候选 | 当前状态 |
|---|---|---|
| `render_phase_008`正负向0移动；新写15当tick不减；dead state14多life且&lt;1写30 | `HitStop/HitStun`具有同样显隐公式、正负移动及15/20/30写入；但旧B2 AI文档把render phase临时映射为Y | `STRONG_BINDING_CANDIDATE / CONFLICT`；先做非零joint trace，不新增重复字段 |
| `hit_reaction_timer>0 --` | `Fall`已由B0 verified binding；`RunReleaseFrameTickCounters`减1 | `VALUE_PRESENT / PLACEMENT_SPLIT` |
| `bdefend_accumulator>0 --` | `Bdefend`存在；legacy serial用-0.5 accumulator | `CONFIRMED_RATE_AND_OWNER_DIFFERENCE` |
| `kind6_input_timer_0ea>0 --` | `HitConfirmEa/HitConfirmCounter`为既有候选并在frame counters减1 | `CANDIDATE_PRESENT / PLACEMENT_SPLIT` |

### 4.2 FUN_004503C5 unconditional tail

| 十dword bank顺序 | Unity字段 | 当前状态 |
|---|---|---|
| `bound_state_198` | `BoundState198` | carrier有，production decrement无 |
| `input_double_cost_19c` | `InputDoubleCost19C` | carrier有，production decrement无 |
| `hit_resource_injury_double_1a0` | 无 | `MISSING_CARRIER` |
| `mp_regen_bonus_timer_1a4` | `MpRegenBonusTimer1A4` | carrier有，production decrement无 |
| `effective_max_regen_double_1a8` | `EffectiveMaxRegenDouble1A8` | carrier有，production decrement无 |
| `hp_regen_double_1ac` | `HpRegenDouble1AC` | carrier有，production decrement无 |
| `full_restore_timer_1b0` | `FullRestoreTimer1B0` | carrier有，production decrement无 |
| `input_cost_waived_1b4` | `InputCostWaived1B4` | carrier有，production decrement无 |
| `native_computer_state_1b8` | 无 | `MISSING_CARRIER` |
| `native_timer_1bc` | 无 | `MISSING_CARRIER`；Authority也尚无locked producer/consumer |

只有正值减1，0和负值原样保留。`Unk338`已由`AdvanceOid5152ReactionTimerForModule`在frame后单独正确执行，是当前唯一已接production的unconditional C25h timer。

### 4.3 positive-HP status与unconditional cleanup

| Authority | Unity现状 | 结论 |
|---|---|---|
| HP&gt;0时减`input_action_lock_130/input_remap_state_138/input_proxy_counter_14c/weak_12c/delay_134/join_148/poison_120` | 前四个carrier存在，delay/join/poison三元组缺失；proxy reaction helper仅测试调用 | `PARTIAL_CARRIERS / PRODUCTION_WRITER_MISSING` |
| poison递减后仍&gt;0且`timer&0x1f==0`时按type0..5算伤害，累计HP-consumed，HP下限按type奇偶为0或1 | 无carrier/producer/算法；Authority object DAT有28个poison itr producer，Unity Config/schema为0 | `OBSERVABLE_B11+B5_GAP` |
| join到期恢复original battle group；proxy到期清enabled，即便body skipped也执行cleanup | proxy清理helper存在但未接production；join override/original carrier均缺 | `MISSING_PRODUCTION_OWNER` |

Authority object DAT另有16个itr `delay` producer；Unity Config/schema为0。weak/join的locked object-itr producer为0，但存在其他mode/quest/stage producer路径，仍保留字段契约。

## 5. C25i armor recovery

Authority字段为`runtime_armor_hp_118`默认0和`armor_recovery_timer_11c`默认-1：

- timer&lt;0 no-op；body skip时冻结；
- timer==0立即从definition首个armor block reload；无armor或armor.hp==0则timer=-1；
- timer&gt;0时仅在runtime armor HP&lt;=0时递减，到0同tickreload HP，并把timer重置为positive recover或-1；
- runtime armor HP&gt;0时positive timer保持不动。

Unity raw projection已明确把两字段输出为null，runtime/schema/parser均无正式carrier/armor block。Authority locked decoded DAT实测18个armor block，Unity Config为0；其中6个Authority block显式`hp:1`，全部未声明`recover`，所以当前正式内容会初始化armor HP但C25i timer为-1。即便当前C25i recovery loop常为no-op，armor HP仍直接改变B5命中结果，不能省略。内容/schema写入受H/B11策略gate约束。

## 6. C25j attacker rest

Authority唯一条件为`attacker_rest>0 && (motion_hold_timer==0 || object_type==3)`，不检查interaction relation，并位于h/i后。

Unity `AttackExempt`已是verified canonical binding，`ItrRest.Arest`只是兼容镜像。当前`RunCommonFrameTick`在frame body入口按相同motion-hold/type3外形减值，并且发生在negative LinkState早退前，因此最终gate大体正确；但owner仍混在C25g，未来h/i插入后会造成trace/职责顺序错误。应把减值和mirror commit移到显式C25j，不恢复旧global Cooldown pass。

## 7. 实施拆分

1. `NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001 / VERIFIED`：Y/phase互异raw已裁决`render_phase_008`唯一绑定`HitStop`，无需独立carrier；旧AI Y临时映射失效，production consumer另包纠正。
2. `NTSD28-B3-C25F-J-STATE-CARRIERS-001 / VERIFIED`：computer/native timer、injury-double、delay/join/poison/join-override、armor HP/timer等12载体及reset/copy/snapshot/checksum/parity已闭合；未启用算法、未改content。
3. `NTSD28-B3-C25F-H-J-TIMER-OWNERS-001 / VERIFIED`：C25f refresh、C25h body/unconditional/positive-HP timer/poison/cleanup与C25j explicit owner已接production；g内旧timer/rest只留direct compatibility。caller复核确认legacy -0.5 recovery当前不进入post-C25 production serial，因此未新增无效suppression状态。
4. `NTSD28-B3-C25G-FRAME-BODY-001 / VERIFIED`：exact/fallback已共用单一core，terminal hold、type3 state3007与heavy-weapon extra已闭合；collision-Y/cost/pending/audio继续由B4/B7/B10/B11/H处理。
5. `NTSD28-B3-C25I-ARMOR-RECOVERY-001 / VERIFIED_PROGRAMMATIC_CORE`：程序化fixture、timer/reload/gate与h→i→j placement已闭合；正式content与hit producer继续等待H/B11和B5，未完成前不得声明正式armor对齐。

## 8. 当前结论

- 已实现事实：C25f、C25h timer/status/poison/cleanup与C25j已进入同一production live-slot transaction；direct compatibility仍保留旧counter行为。
- 已实现事实：`Bdefend` production现按C25h每tick减1；十字段bank与positive-HP status已有owner。正式poison/delay producer仍等待H/B11内容策略。
- 已观察事实：`HitStop`已由Y/phase互异3tick raw唯一绑定`render_phase_008`；旧AI Y投影是待迁移的confirmed consumer difference。
- 已观察事实：Authority armor18块（6块hp1）、poison itr28、delay itr16，而Unity Config/schema均无，正式内容切换仍受H/B11决策约束。
- 下一步：C25k-p逐槽OPoint/previous-action/weapon-piece/lifecycle/healing重排。C25i正式content/hit仍等待H/B11+B5；C25g剩余detail按既定B4/B7/B10/B11路由。
