# NTSD 2.8 native input action routing crosswalk

> Change ID：`NTSD28-B2-NATIVE-ACTION-ROUTING-CROSSWALK-001`  
> Authority：`source/ntsd28_core/src/simulation/input_routing.cpp`  
> Unity：`BattleCharacterInputActionResolver`、`LF2CharacterActionResolver`、
> `NTSD28NativeComboRouteSelector`及`LF2FrameData/Lf2DatConverter`  
> 日期：2026-09-03

## 1. 正式入口与顺序

- core build：`source/ntsd28_core/scripts/build.ps1:55`编译`input_routing.cpp`，`:167`编译
  `input_routing_tests.cpp`。
- playable build：`source/ntsd28_playable/scripts/build.ps1:62`编译同一生产文件。
- live caller：`simulation_tick_driver.cpp`第二个升序slot scan先做0x21-byte proxy copy，再调用
  `InputRouter28::step_sampled`。
- `step_sampled`严格顺序：

```text
dead type-0 clear
→ optional current-button remap
→ bound jump-edge gate
→ edge window decay + rising history
→ combo10 advance
→ depth intent
→ combo fields
→ hit_a/d/j + hold_a/d/j
→ hit_f/b/uz/dz + hold_f/b/uz/dz
→ common/type-0 built-ins
```

## 2. 路由差异表

| 领域 | NTSD 2.8权威 | Unity当前 | 结论 |
|---|---|---|---|
| combo priority | `Fa/Fj/Ua/Uj/Da/Dj/aj/ad/ja/jd`固定first-match | pure selector已实现同顺序 | selector ready，production caller缺失 |
| combo attempt | 非零字段即attempt；即使action被lock/cost拒绝也按caller清combo | legacy resolver只清投影字段；exact bank不清 | 可导致exact终态跨tick残留/重试 |
| horizontal facing | 仅字段非零才按combo4/5写facing | legacy `RunCombo`有相近行为 | 必须从exact decision执行，不能先投影丢来源 |
| `hit_ja` | oid/use_ai special family、300/HP177/global gate、linked id、`+194/+328/+338`特殊事务 | 仅oid6/300/HP177与部分`Unk328/338` | 条件、attempt和frame-counter副作用不完整 |
| generic action | lock→signed/999 resolve→state redirect→source-frame cost→fallback→last-action/action | `TryCharacterDatInputFrameJumpCompatibility`只有部分signed/999/PP cost | 不是可替换实现 |
| direct J/K/L | edge age严格大于另外两键；非零field尝试后清winning age | 已有`hit_a/d/j`最大值比较 | 基础形状存在，native transaction/镜像仍缺 |
| held J/K/L | previous按钮与其他edge比较；attempt后清对应previous | 无`hold_a/d/j`字段/消费 | production缺失且权威内容可达 |
| direction | facing-aware `hit_f/b`、depth非对称`hit_uz/dz` | 无字段/消费 | production缺失 |
| held direction | `hold_f/b/uz/dz`，四条清理字节规则不对称 | 无字段/消费 | production缺失 |
| remap | phase0且`input_remap_state_138>0`，7-index全有效才原子替换current | 无carrier/consumer | production缺失 |
| jump suppression | `bound_state_198>0`且global record不为1/3，仅禁止K rising，不改current/previous | core支持bool参数但production恒false | caller/carrier缺失 |
| input RNG | standing/linked attacks消费sync `0x82/0x83/0x84` | 通用`BattleRandInt` | stream及call-site错误 |
| built-ins | 状态0/1/2/4/5/85/86，另有110、19/301、215、182/188前置 | `LF2CharacterActionResolver`与shared-shell分散近似 | 顺序、direct/generic action、资源和same-tick reload均未闭合 |

## 3. `apply_action`与direct action不能合并

`apply_action`正式副作用：

- `input_action_lock_130`先拒绝；
- 负数翻转、999→0；
- target frame必须存在；
- source frame state的1000000/2000000编码可重定向action，但费用仍取source frame；
- resource gate启用时应用waiver、`recmp`或mode multiplier、double-cost、frame `mp/hp`及
  effective-max-HP扣减；
- 不可负担时按`+194 → stats.caughtact → mode +0xB8`选择fallback；
- 普通成功同时写`input_last_action_144`与frame action；caller决定是否清frame counter；
- 字段已经尝试与动作成功是两个不同布尔量。

native locomotion direct action故意绕过lock、generic cost、`+0x144`，只有具体分支明确时才清frame
counter。standing/air attack的clamped resource policy与running attack的affordability gate也互不相同。
因此不能用一个“跳到frame并扣PP”的Unity helper覆盖所有入口。

## 4. 字段与内容库存

Unity `LF2FrameData/Lf2DatConverter`当前缺以下11个frame字段。2.8 decoded runtime的文本库存为：

| 字段 | 2.8 occurrences | 2.8 files | Unity正式Config tokens |
|---|---:|---:|---:|
| `hit_f` | 0 | 0 | 0 |
| `hit_b` | 136 | 12 | 0 |
| `hit_uz` | 0 | 0 | 0 |
| `hit_dz` | 0 | 0 | 0 |
| `hold_a` | 3 | 2 | 0 |
| `hold_d` | 112 | 109 | 0 |
| `hold_j` | 16 | 15 | 0 |
| `hold_f` | 0 | 0 | 0 |
| `hold_b` | 0 | 0 | 0 |
| `hold_uz` | 0 | 0 | 0 |
| `hold_dz` | 0 | 0 | 0 |

已由前包补parser能力、但Unity正式Config仍为0 token的combo字段：`hit_aj=1110/96`、
`hit_ad=1089/96`、`hit_jd=99/99`（occurrences/files）。所有内容值迁移仍受B11策略门约束。

### 4.1 下一carrier包的精确字段合同

这里区分“B2必须能够读取/写入的状态”与“其正式producer所属阶段”。下一carrier包只建立类型、
默认值、deep-copy、reset、snapshot/schema和checksum；不得为了让输入测试通过而提前迁入B3/B5/B8
producer或timer pass。

| Authority字段 | 类型/初始值 | Authority生产或消费 | Unity当前结论 | 正式owner |
|---|---|---|---|---|
| `input_action_lock_130` | `int / 0` | `manacle`命中状态写入；正HP实体晚阶段递减；`apply_action`最先拒绝 | missing | carrier B2；producer B5；timer B3/B5 |
| `input_last_action_144` | `int / 0` | 普通`apply_action`成功写入；cost fallback/direct built-in不写 | missing | B2 transaction |
| `input_remap_state_138` | `int / 0` | `confus`状态写入；正HP晚阶段递减；`step_sampled` phase0读取 | missing | carrier B2；producer B5；timer B3/B5 |
| `input_remap_indices_13c` | fixed `byte[7] / 0..6` | `confus`先填`0xff`再用sync `0x004166B2`形成无重复排列；全索引有效才原子remap | missing | carrier B2；producer B5 |
| `bound_state_198` | `int / 0` | `bound`命中状态写入；十dword timer bank无条件递减；输入只屏蔽K rising | missing | carrier B2；producer B5；timer B3/B5 |
| `input_global_record_state_20` | `int / 0` | 活跃global record投影；1/3绕过bound jump gate | missing | carrier B2；projection B8 |
| `input_mode_cost_multiplier_30` | `int / 0` | stats `recmp<1`时使用battle-mode `recmp`百分比 | missing | carrier B2；projection B8 |
| `input_double_cost` (`+0x19C`) | `int / 0` | `facing`命中状态写入；timer bank无条件递减；正值令adjusted MP cost乘2 | missing | carrier B2；producer B5；timer B3/B5 |
| `input_cost_waived` (`+0x1B4`) | `int / 0` | quest kind12写持续时间；timer bank无条件递减；非零令MP cost为0 | missing | carrier B2；producer B8；timer B3/B5 |
| `input_special_gate_194` | `int / 0` | generic cost fallback第一优先级；`hit_ja` special tail gate | missing | carrier B2；producer B7 fusion |
| `input_mode_fallback_action_b8` | `int / 0` | cost failure在`+194`、stats.caughtact之后第三优先级 | missing | carrier B2；projection B8 |
| `input_local_resource_enabled_49d034` | `bool / true` | 世界级F6 gate在spawn时投影；false绕过完整MP/HP transaction | missing exact carrier | carrier B2；world producer B8 |
| `input_hp_consumed_total` (`+0x34C`) | `int / 0` | 成功资源事务累计HP cost | missing；不可借用`HPLost` | B2 transaction |
| `input_mp_consumed_total` (`+0x350`) | `int / 0` | 成功资源事务累计adjusted MP cost | missing；不可借用`PpDisplay` | B2 transaction |
| frame `hp` | `int / 0` | `hp_delta=(mp/1000)*10+hp`，并令effective-max-HP减少`hp/3` | frame carrier/parser missing | B2 carrier/parser；content B11 gate |
| stats `recmp` / `caughtact` | `int / 0` | definition multiplier优先于mode；fallback第二优先级 | `LF2CharacterData`与loader missing | B2 carrier/parser；content B11 gate |
| bmp `use_ai` | `int / absent→0` | `hit_ja` special family alias，随后fallback到object id | character data/loader missing | B2 carrier/parser；content B11 gate |
| `feature_gate_4a8428` | world bool，locked初值false | true阻断oid/use_ai 6、action300、HP>177 special success | exact global missing；旧`DjaGuardGlobal44F224`不得直接等同 | B2 consumer contract；producer B8 |
| `input_linked_definition_id` (`+0x324`) | `int / -1` | `hit_ja` special-family tail条件 | `TransformOriginalObjectId`仅为candidate，尚未完成2.8 exact binding | B2 transaction先显式注入；B7闭producer/binding |
| `input_special_gate_328` / `input_special_timer_338` | `int / 0`；fusion return可写`-1`/wait | `hit_ja`只在gate==1时清timer并consume | `Unk328/Unk338`已有fusion行为；复用前需focused验证initial、1、-1三态 | B2 transaction消费；B7 producer |

现有生命值映射可复用：authority `current_hp/effective_max_hp/base_max_hp/current_mp/base_max_mp`
分别对应Unity `HP/HPBound/HP3/PP/PPMax`；`MP`、`HPLost`和`PpDisplay`不是可替代字段。
`BattleWorldEntityRuntimeSnapshot`通过`TryCopyCanonicalStateTo`保存runtime，因此新增字段必须同时更新
copy/reset、entity snapshot schema `2→3`、aggregate schema `4→5`、checksum schema `7→8`和反射
全字段守卫。固定数组必须预分配，warm capture/copy/checksum仍须0 B。

## 5. Authority验收矩阵

`input_routing_tests.cpp`共有46个`test_*`定义，且46个均由`main`实际调用；初始44计数已经直接重计
纠正。必须按域移植/复用为Unity focused矩阵：

- edge/remap/dead：edge priority、bound jump suppression、current remap、dead type0；
- field order：simple DAT、direct/hold/depth、locked real trace、missing horizontal field；
- combo：directional、same-sample、premature terminal、history priority/miss、Shisui J→K；
- action：rejection consumes、`hit_ja`、reapplication、cost/negative、fallback、multiplier/double/maxHP；
- built-ins：base/double-tap、current+buffer gate、diagonal scaling、linked stats、heavy relation、
  running/air/dash、action215、state85/86、182/188。

Unity只有在同seed/input/tick下覆盖相同字段读写和副作用后，才能把action routing标为aligned。

### 5.1 46项逐项归属

下表中的“主包”只表示首次完整承担该断言的实现包；所有项仍须在
`B2-INPUT-JOINT-TRACE`中按同seed/input/tick复跑，不能以单元测试通过代替联合trace。

| # | authority test | 主包/处理方式 |
|---:|---|---|
| 1 | `test_custom_running_frame_sequence` | `B2-NATIVE-TYPE0-BUILTINS` |
| 2 | `test_walk_and_run_share_native_entity_zero_cycle_counter` | `B2-NATIVE-TYPE0-BUILTINS` |
| 3 | `test_simple_dat_routes_and_edge_priority` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 4 | `test_native_encoded_target_frame_state_redirects` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 5 | `test_native_hold_fields_consume_exact_previous_or_edge_bytes` | `B2-NATIVE-DIRECT-HOLD-DIRECTION-ROUTING` |
| 6 | `test_native_depth_hit_fields_consume_asymmetric_bytes` | `B2-NATIVE-DIRECT-HOLD-DIRECTION-ROUTING` |
| 7 | `test_locked_native_trace_hit_a_events_route_real_character_dat` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 8 | `test_locked_shisui_j_then_k_routes_hit_aj_and_clears_history` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 9 | `test_bound_suppresses_only_jump_rising_edge` | `B2-NATIVE-ACTION-DATA-CARRIERS`后由transaction复验 |
| 10 | `test_common_routes_are_not_artificially_type0_gated` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 11 | `test_builtin_routes_consume_the_native_five_tick_window` | `B2-NATIVE-TYPE0-BUILTINS` |
| 12 | `test_standing_defend_honors_native_c1_reentry_cooldown` | `B2-NATIVE-TYPE0-BUILTINS` |
| 13 | `test_action110_tracks_native_horizontal_facing` | `B2-NATIVE-TYPE0-BUILTINS` |
| 14 | `test_state19_301_grounded_depth_control` | `B2-NATIVE-TYPE0-BUILTINS` |
| 15 | `test_native_current_button_remap` | `B2-NATIVE-ACTION-DATA-CARRIERS`后由transaction复验 |
| 16 | `test_directional_combo_and_facing` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 17 | `test_direction_and_terminal_may_complete_in_one_native_sample` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 18 | `test_premature_terminal_does_not_cancel_directional_combos` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 19 | `test_missing_horizontal_combo_field_does_not_change_facing` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 20 | `test_native_history_combo_priority_preserves_proxied_later_state` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 21 | `test_native_history_miss_clears_proxy_tail_de` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 22 | `test_nonzero_dat_field_consumes_input_when_action_is_rejected` | transaction与direct routing跨包复验 |
| 23 | `test_hit_ja_ordinary_and_special_family_gates` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 24 | `test_successful_combo_reapplication_restarts_current_action` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 25 | `test_action_cost_gate_and_negative_facing` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 26 | `test_native_direct_hit_fields_preserve_frame_counter` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 27 | `test_native_cost_failure_fallback_priority` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 28 | `test_native_cost_multiplier_double_and_effective_max_hp_accounting` | `B2-NATIVE-COMBO-ACTION-TRANSACTION` |
| 29 | `test_type0_base_actions_and_double_tap_run` | `B2-NATIVE-TYPE0-BUILTINS` |
| 30 | `test_type0_base_actions_require_current_button_with_buffer` | `B2-NATIVE-TYPE0-BUILTINS` |
| 31 | `test_native_diagonal_horizontal_scaling` | `B2-NATIVE-TYPE0-BUILTINS` |
| 32 | `test_interaction_state_attack_uses_linked_stats_and_not_action_lock` | `B2-NATIVE-TYPE0-BUILTINS` |
| 33 | `test_native_builtin_direct_action_side_effects` | `B2-NATIVE-TYPE0-BUILTINS` |
| 34 | `test_native_builtin_resource_policies` | `B2-NATIVE-TYPE0-BUILTINS` |
| 35 | `test_linked_stats_zero_or_missing_uses_native_default` | `B2-NATIVE-TYPE0-BUILTINS` |
| 36 | `test_relation_state2_heavy_walk_run_and_throw` | `B2-NATIVE-TYPE0-BUILTINS` |
| 37 | `test_relation_state2_heavy_running_and_run_throw` | `B2-NATIVE-TYPE0-BUILTINS` |
| 38 | `test_running_interaction_attack_selectors` | `B2-NATIVE-TYPE0-BUILTINS` |
| 39 | `test_state4_airborne_attack_selectors` | `B2-NATIVE-TYPE0-BUILTINS` |
| 40 | `test_state5_forward_dash_uses_held_attack_without_new_edge` | `B2-NATIVE-TYPE0-BUILTINS` |
| 41 | `test_state5_linked_weapon_attack_families` | `B2-NATIVE-TYPE0-BUILTINS` |
| 42 | `test_action215_dash_direction_and_defend` | `B2-NATIVE-TYPE0-BUILTINS` |
| 43 | `test_state85_86_orients_and_advances_from_current_motion` | `B2-NATIVE-TYPE0-BUILTINS` |
| 44 | `test_action182_188_rowing_redirect` | `B2-NATIVE-TYPE0-BUILTINS` |
| 45 | `test_dead_type0_input_is_suppressed_until_hp_is_restored` | producer migration已覆盖清理；built-ins复验恢复后next-sample动作 |
| 46 | `test_web_key_parser` | authority Web入口专用；Unity无对应Web生产入口，B2 exit audit须记录架构N/A，不伪造移植 |

其中第46项证明的是authority Web字符串键入口，不等于Unity Input System需要新增Web parser；若未来
Unity接入同一Web控制面，再另立输入边界包。第22项跨generic/direct两个调用族，必须在两个实现包都
完成后才允许标记通过。

## 6. 实施拆分

1. `B2-NATIVE-ACTION-DATA-CARRIERS`：补remap、jump-suppress、action transaction字段及
   hold/direction frame/parser能力；snapshot/checksum/reset完整。正式Config不改。
2. `B2-NATIVE-COMBO-ACTION-TRANSACTION`：exact selector production caller、generic/direct action
   transaction、attempt/clear、horizontal facing与`hit_ja`特殊gate。
3. `B2-NATIVE-DIRECT-HOLD-DIRECTION-ROUTING`：按native不对称比较/清字节顺序实现
   `hit/hold`三按钮与方向字段。
4. `B2-NATIVE-TYPE0-BUILTINS`：按状态族继续拆standing/walk/run/heavy与air/dash/redirect；接
   synchronized `0x82/83/84`，禁止沿用generic RNG。
5. `B2-INPUT-JOINT-TRACE`：phase、recording、proxy、history、combo、action、motion和双RNG
   first-difference为零后才执行B2退出审计。

当前`NTSD28-B2-NATIVE-INPUT-PRODUCER-MIGRATION-001`仍须先完成reload/Play闭环；后续实现不得
反向恢复producer内edge/combo推进。
