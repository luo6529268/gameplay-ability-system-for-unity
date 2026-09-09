# NTSD 2.8-Logan C25c-e resource/display field inventory

> Change ID：`NTSD28-B3-C25C-E-RESOURCE-DISPLAY-INVENTORY-001`  
> 状态：`VERIFIED / GOVERNANCE_ONLY / FIELD_MATRIX_COMPLETE`  
> Authority：formal EXE `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；playable closure `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`

## 1. C25内顺序与共同gate

每个当前occupant在C25a-b后依次执行：C25c `advance_native_resources_pre_display_slot` → C25d `advance_native_display_values_slot` → C25e `advance_native_resources_post_display_slot`。C25c/e只接受active、非`lifecycle_resolution_pending`、definition type0且当前action frame存在；C25d只要求active。`resource_phase_12/resource_phase_3`已由C23在进入dynamic slot loop前各推进一次，所有slot读取同一phase值。

Unity当前把旧简化recovery放在C25a-b后、frame tick前，位置外形可复用；但算法只覆盖固定12-tick HP+1、负WeaponCount自伤与旧PP恢复，不能代表C25c/e。C25d没有正式逻辑owner。

## 2. C25c pre-display字段矩阵

| Authority规则/字段 | Unity现状 | 结论与路由 |
|---|---|---|
| `resource_phase_12/resource_phase_3` | `NTSD28NativeWorldClockState.ResourcePhase12/3`，C23 single owner | `VERIFIED_INPUT` |
| `current_hp/effective_max_hp/base_max_hp` | `Health.HP / HPBound / HP3` | `VERIFIED_BINDING` |
| `current_mp` | `Health.PP / Runtime.PP`；旧B0 `Runtime.MP`投影已由binding correction supersede | `VERIFIED_BINDING`；MP200/PP173红灯、production writer链、tool/Unity回归已闭合，禁止双写 |
| `base_max_mp` | `Health.MaxMP / Runtime.MPMax` | `VERIFIED_BINDING`，但definition `stats.max_mp`尚未进入Unity content schema |
| `weak_timer_12c` | 无正式runtime carrier | `MISSING_CARRIER` |
| definition `stats.regen_dhp/regen_hp/regen_mp/bound`与stats-record-presence | `LF2CharacterData`及converter均无字段 | `MISSING_SCHEMA / B11` |
| `effective_max_regen_double_1a8/hp_regen_double_1ac/mp_regen_bonus_timer_1a4` | 无正式runtime carrier | `MISSING_CARRIER` |
| frame `chp/cmp` | `LF2FrameData`、parser、converter均无字段 | `MISSING_SCHEMA / B11`；当前Authority non-background object DAT实测`cmp` 5 files/7 hits、`chp` 2 files/4 hits，不能按零处理 |
| `input_double_cost` | `Runtime.InputDoubleCost19C` | `VERIFIED_BINDING` |
| `ordinary_credit_gate_2f4` | 无正式carrier；不能用`KillCount`代替 | `MISSING_CARRIER` |
| `render_phase_008` | Unity存在其他render/frame carrier但无已验证精确绑定 | `MISSING_BINDING`，路由C25f-j/B9交叉闭合 |
| mode `regen_hp_28/regen_mp_2c/full_restore_18`与negative-MP global | Unity无C25 resource-rules对象；`PpMode`可能对应F3 global，但未形成字段合同 | `MISSING_WORLD_RULE_CARRIER / B8`；当前普通配置Authority默认1/1/0，negative enable由F3 state投影 |
| `environment_state_320` | `Runtime.EnvironmentState320`已具备copy/reset/snapshot/checksum；B0 field contract仍写MISSING | `CARRIER_PRESENT / GOVERNANCE_STALE`，需同步纠正field contract |
| `incoming_damage_scale_340` | 无正式carrier；C25a target stats.defend尚未落Unity | `MISSING_CARRIER_AND_SCHEMA / B11+B5` |
| `catch_source_slot_90`、encoded 0x2000 form、两级`owner_slot`链 | Unity有`CatcherSlotIndex`与`OwnerSlotIndex`，但尚无+0x90 encoded identity合同 | `CANDIDATE_ONLY / B5` |
| `impact_source_slot_164`、environment lethal KO event | 无精确carrier/producer；现有KillStat等不能直接等同 | `MISSING / B5+B8` |
| `input_score_total_348/knockout_count_358` | 无同义正式carrier | `MISSING_CARRIER / B5+B8` |
| `input_hp_consumed_total` | `Runtime.InputHpConsumedTotal34C` | `VERIFIED_BINDING` |

### 当前普通内容的重要分支事实

- Authority non-background object DAT中：`stats.max_mp` 158 files/158 hits；`regen_dhp/regen_hp/regen_mp/frame_0mp`均0；frame `cmp`为5 files/7 hits、`chp`为2 files/4 hits。
- Unity `Assets/NTSD/Config`与当前`LF2CharacterData/LF2FrameData`无上述键。即使Authority多数regen字段缺省，`stats record present`本身也改变HP fallback gate：普通VS mode的`regen_hp_28=1`会抑制拥有stats record的默认+1。现有Unity recovery对所有type0一律12-tick +1，因此不能继续作为正式C25c。
- 普通配置`regen_mp_2c=1`抑制非负`stats.regen_mp`自动恢复；现有Unity每3 tick执行旧PP恢复公式，是confirmed behavior difference，而不是命名差异。

## 3. C25d asymmetric display字段矩阵

| Authority字段 | 精确步进 | Unity现状 |
|---|---|---|
| `display_score_1f0 / step_1f4` | display&lt;`input_score_total_348`时直接加step，否则赋target；越界当tick不clamp | 无carrier/owner |
| `display_damage_total_1f8 / step_1fc` | display&lt;`input_hp_consumed_total`时直接加step，否则赋target；越界当tick不clamp | 无carrier/owner |
| `display_current_hp_200 / step_204` | display&gt;`current_hp`时直接减step，否则赋target；下穿当tick不clamp | 无carrier/owner |
| `display_effective_max_hp_208 / step_20c` | display&gt;`effective_max_hp`时直接减step，否则赋target；下穿当tick不clamp | 无carrier/owner |

`Runtime.PpDisplay`不是以上任一字段：它由Unity旧输入扣费/退款累计，既不是current MP，也不是四个native HUD mirror。不得复用或重命名来冒充C25d。C25d可在不改资源资产的情况下先新增8个runtime carrier、reset/copy/snapshot/checksum及per-slot algorithm；其step producer仍需B5 hit-resource链闭合。

## 4. C25e post-display字段矩阵

| Authority规则 | Unity现状 | 结论与路由 |
|---|---|---|
| `bmp.frame_0mp`在HP&gt;0、effectiveMaxHP&lt;=0、非保护hurt action时只写current action | schema无字段；Authority locked object DAT当前0 producer | `MISSING_SCHEMA / B11`；当前内容可观察no-op，但edited/new content必须保留 |
| `previous_action_078` | `Frame.Prev`已验证，不能用`PrevFrame2` | `VERIFIED_INPUT` |
| previous state62→HP1；63→HP/effMax0；64→MP0；65→HP/effMax=baseHP；66→MP500 | HP字段存在；MP受blocking binding conflict | `ALGORITHM_MISSING`，绑定纠正后可实现 |
| previous state405→stage中心、precise XYZ同步、motion清零 | `Runtime.Stage.StageWidthPx/ZMin/ZMax`与position/motion carrier存在 | `IMPLEMENTABLE / B8 stage bounds witness` |
| previous state4000..4999→`revive_lives_30c` | `Runtime.HP2Orig` verified | `IMPLEMENTABLE` |
| previous state3640..3645→battle_group | `Runtime.RelationTeam` verified | `IMPLEMENTABLE` |
| ordinary gate + full-restore timer/mode gate→HP/effMax=base、清HP consumed与KO | ordinary gate/full timer/KO缺carrier；mode gate缺world rule | `MISSING_CARRIERS / B5+B8` |
| HP/effMax上限按base HP clamp | HP/HPBound/HP3具备 | `IMPLEMENTABLE`；注意只在currentHP&gt;base时同时把两者设base，第二个分支仅clamp effectiveMax |
| definition `stats.max_mp` positive且currentMP&gt;=max时clamp | Unity schema缺max_mp，runtime仅有MPMax；currentMP binding冲突 | `B11 + BINDING_PREREQUISITE` |

## 5. 后续实施顺序

1. `NTSD28-B3-C25C-E-CURRENT-MP-BINDING-CORRECTION-001`已验证：`current_mp`唯一绑定`Health.PP/Runtime.PP`，旧`Runtime.MP`只保留Unity历史字段，不具native authority身份。
2. `NTSD28-B3-C25C-E-ENTITY-CARRIERS-001 / VERIFIED`：C25d 8字段、C25c/e必要status/score/KO/rule carrier及reset/copy/snapshot/checksum已闭合；未实现行为。
3. B11内容/schema包：在用户确认内容策略后处理definition stats presence/max_mp/regen/bound、bmp frame_0mp及frame cmp/chp；不得提前覆盖Config。
4. `NTSD28-B3-C25C-E-ALGORITHM-001`：按pre→display→post顺序替换旧recovery，并用phase12/3、crossing、previous-state、limits及同slot字段trace验证。
5. environment attribution/KO/score producer与display step producer分别回链B5/B8；缺少这些producer时不得把C25c/d声明为全场景完全对齐。

## 6. 当前结论

- 已观察事实：C25d全套逻辑状态缺失；C25c/e不是旧HP/PP recovery的等价重命名。
- 已观察事实：Unity内容schema漏掉Authority实际使用的`cmp/chp/max_mp`；Direction B保护下本审计未改任何Config。
- 已观察事实：`current_mp`的旧冲突已由MP200/PP173测试与production writer链纠正为`Health.PP/Runtime.PP`；`Runtime.MP`不再是raw authority projection。
- 未知项：environment +0x90 encoded source和KO/score producer尚未闭合。
- 下一步：按Direction B gate先闭合B11内容/schema策略与只读证据，再实现C25c→d→e算法；environment/impact/KO/score/display-step producer继续回链B5/B8。
