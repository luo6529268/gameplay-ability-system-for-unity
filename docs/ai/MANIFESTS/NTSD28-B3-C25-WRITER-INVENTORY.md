# NTSD 2.8-Logan C25 nested live-slot writer inventory

> Change ID：`NTSD28-B3-C25-WRITER-INVENTORY-AUDIT-001`  
> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_PENDING`  
> Authority：formal EXE `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；playable closure `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`

## 1. Authority transaction

`SimulationTickDriver28::step()` 在 C23/C24 推进 resource phases 与 frame sequence 后，动态升序扫描 slot。每次到达 slot 时重新读取当前 occupant；低 slot 创建的高 slot 会在同 tick 被扫描，已越过低 slot 的复用 occupant 到下一 tick 才执行。每个 occupant 的顺序固定为 C25a～p，C25o 消费 slot 后不执行 C25p。

## 2. C25a～p crosswalk

| ID | Authority writer / 关键合同 | Unity 当前 owner | 审计结论 | 实施路由 |
|---|---|---|---|---|
| C25a | `apply_definition_transition_slot`；仅 state 8000..8999，所有 object type；source `next` 决定 target action并重置 definition/frame latches | `TryApplyNativeC25DefinitionTransition` | `CORE_VERIFIED / B11_STATS_PENDING`：production旧9995/4000/render-offset链已隔离；target/action原子解析、action/latch/snapshot/frame-counter已闭。target stats.max_mp/defend/mode scale无Unity载体，留B11 | C25A-B VERIFIED + B11 |
| C25b | `materialize_special_state_clones`；definition 后，frame counter=1；state9996 生成4×217+1×218，sync RNG，立即占最低 transient slot | production `SpawnState9996Children(..., native=true)` | `VERIFIED_LOCKED_STATE9996`：精确34 callsite、native birth、missing/capacity与高低slot visibility已闭；locked DAT无8M producer，Authority 8M本身为address-derived unresolved，Unity保持无生成/无RNG | C25A-B VERIFIED |
| C25c | `advance_native_resources_pre_display_slot`；phase12/phase3 HP/MP、environment、score与状态门控 | `RunPreCollisionRecoveryPhase` / ECS recovery | `INVENTORIED / CURRENT-MP-CONFLICT / CARRIERS+SCHEMA_MISSING`：旧HP/PP恢复不等价；详见C25c-e inventory | C25C-E binding→carriers→B11→algorithm |
| C25d | `advance_native_display_values_slot`；score/damage 向上、HP/maxHP 向下的非对称步进，不在当 tick clamp crossing | 无等价正式 writer | `INVENTORIED / 8_CARRIERS_MISSING`：`PpDisplay`语义不同不可复用 | C25C-E carriers+algorithm + B9 consumer |
| C25e | `advance_native_resources_post_display_slot`；frame_0mp、previous_state 62..66/405/4000..4999/3640..3645、full restore与limits | recovery/分散 frame/health writer | `INVENTORIED / PARTIAL_CARRIERS / CURRENT-MP+B11_BLOCKED` | C25C-E binding→carriers→B11→algorithm |
| C25f | slot<10、type0、current state 7000..7999 时写 `native_computer_state_1b8=state-7000`，随后 C25h 同 tick递减 | 只有 diagnostic DTO，runtime 无正式 carrier/writer | `MISSING` | C25F-J，连同 snapshot/checksum |
| C25g | `step_frame_slot`；hold/relation gates、frame counter/action、type3 drain、transition side effects、audio与 terminal pending | shared `RunNativeC25FrameBodyForWorldPass`，ECS exact只保留资格/diagnostics | `B3_COMMON_CORE_VERIFIED / DETAIL_PENDING`：terminal hold、type3 state3007、heavy extra已闭；collision-Y/cost/pending/audio仍待 | B4/B7/B10/B11/H |
| C25h | `advance_reaction_timers_slot`；frame 后的 render/hit/bdefend/kind6、`+0x338`、10-dword bank、positive-HP status与join/proxy cleanup | `AdvanceOid5152ReactionTimerForModule` + frame counters等分散 writer | `PARTIAL`：`+0x338` production placement已闭合，其余 timer bank 未闭 | C25F-J |
| C25i | `advance_armor_recovery_slot`；与 frame body 相同 hold/relation gate，armor block HP/recover reload | `BattleNativeArmorRecoveryKernel`位于C25h→i→j；两carrier ready，profile默认false | `PROGRAMMATIC_CORE_VERIFIED / FORMAL_CONTENT_PENDING` | 正式armor schema/content/hit归B5+H/B11 |
| C25j | `decrement_attacker_rest_slot`；`rest>0 && (hold==0 || type3)` | frame tick内 `AttackExempt--` + mirror sync | `PARTIAL_PLACEMENT_READY`：已在 per-slot frame 后可观察，但精确 hold/type3 gate需 B5/本包验证 | C25F-J |
| C25k | `materialize_native_frame_zero_entry`；仅 frame counter zero，terminal pending仍拒绝；在所有粒子/lifecycle前占槽 | `ProcessLateOpointSegment` | `BLOCKED_BY_FORMAL_PENDING`：counter gate已有；Unity缺C25g原始terminal pending/code且额外type0 FrameDelay gate | 与B7 terminal producer/carrier及C25o联合 |
| C25l | `materialize_state18_broken_weapon_particles`；以 `previous_action_078` 与 post-frame action比较，OPoint后，sync RNG | `RunNativeC25State18BrokenWeaponParticles` + decision kernel | `PROGRAMMATIC_ORDER_VERIFIED / RESOURCE_RUNTIME_PENDING`：已从state13/200 mixed tail拆出并位于OPoint后、cleanup/C25m前；正式global-delay producer和OID999粒子slot/tuple Play待 | `NTSD28-B3-C25L-STATE18-PARTICLE-OWNER-001` + B7/B8/H/B11 |
| C25m | `previous_action_078 = action`；紧随 C25l，区别于 C10 `+0x7C` snapshot | `MirrorLatePrevFrame` + transient old-Prev virtual-tail context | `SURVIVOR_COMMIT_VERIFIED / TERMINAL_PATH_PENDING_B7`：C25l后commit、cleanup前；兼容branch仍读old Prev，C25p snapshot可见 | `NTSD28-B3-C25M-PREVIOUS-ACTION-COMMIT-001 / VERIFIED` + B7 terminal |
| C25n | `materialize_weapon_piece_fragments`；type1/2/4/6 且 weapon_hp<0；built-in OID999 + DAT pieces + sound，随后标 lifecycle | `TryRunLatePostOpointCleanupPhase` 仅 sound/pending destroy；transition effects另处 | `MISSING_BEHAVIOR` | C25K-P/B7/B10/B11 |
| C25o | `resolve_pending_lifecycle(slot)`；11xx/12xx reset action0，否则 terminal despawn；消费 slot则 continue | `HandleFrameTickExit` 在 C25k 前；pending destroy统一 loop 后 flush | `CONFIRMED_ORDER_DIFFERENCE` | C25K-P；逐 slot关系安全清理与 generation test |
| C25p | `advance_native_healing_slot`；仅未被 lifecycle 消费的type0活体；encoded/ordinary heal与state1700 arm | `BattleLateEntityLifecycleModule.AdvanceNativeHealing` + `BattleNativeHealingKernel` | `B3_VERIFIED / GLOBAL_DUPLICATE_REMOVED`：逐槽survivor owner与算法顺序已闭合；global post-tail保留独立维护 | `NTSD28-B3-C25P-HEALING-OWNER-001 / VERIFIED` |

## 3. Unity structural facts

- `BattleLateEntityLifecycleModule.Run` 已使用 `for runtimeSlot < RuntimeSlotCapacity` 并通过 `FindEntityByRuntimeSlotCurrentForLateModule` 每槽重取当前 active occupant；不是预先冻结 list。
- `CreateObjectImmediate` / structural spawn 会立即注册 occupant；已有 W05 和 W03 测试证明高 slot 同 pass执行、低 slot下一 pass执行、same-slot generation不会复活旧 handle。
- `PendingFlushDestroy` 或 pending unregister 会立即从 active query 排除；物理 unregister/free 的统一 flush 目前仍在整个 late loop 结束，不能直接视为 Authority C25o逐 slot lifecycle完成。
- `NTSD28-B3-C25-NESTED-TAIL-SKELETON-001` 已把`LateEntityUpdate`单一dynamic scan放到C24后；legacy `SerialTickAll`显式后置，`EntityPostFrameTailAll`仍在random-drop后。C25a-b已进入前述dynamic scan，但legacy serial/global post-tail中的C25g/o/p残留仍是confirmed difference。

## 4. Presentation boundary

Authority core C25/C26 完成并回到 `GameSession28::step()` 后才执行 session function-key、feed、earthquake、camera；`snapshot()` 从已经完成的 world 建立render snapshot。该边界已由C25 skeleton把normal `RenderDispatch`移到C25、legacy serial、Stage/session tail、Results之后并通过Play；C25内部尚未闭合的c-p仍可能改变最终snapshot内容。

## 5. 下一包边界

`NTSD28-B3-C25-NESTED-TAIL-SKELETON-001` 已完成生产单一入口、C24后placement、dynamic live-slot cursor与completed-tick Render；`NTSD28-B3-C25A-B-DEFINITION-CLONE-001`已闭合production definition core与locked state9996 clone。C25c-e current_mp纠正与21个entity carrier均已验证，算法受B11继续约束。C25f-j字段/owner与实现已推进；K-P owner审计和C25p逐槽healing也已闭合，详见`docs/ai/MANIFESTS/NTSD28-B3-C25K-P-OWNER-AUDIT.md`。下一C25k/m先闭old-Prev reader与terminal分类，再处理l、n/o。legacy serial/global post-tail仍未关闭。
